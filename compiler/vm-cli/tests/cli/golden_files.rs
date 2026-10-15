//! Tests of the golden container files and the generator that refreshes them.

use std::path::{Path, PathBuf};
use std::process::Command;

use assert_cmd::cargo;
use assert_cmd::prelude::*;
use ironplc_container::test_support::steel_thread_debug_builder;
use tempfile::TempDir;

use crate::{write_container, write_steel_thread_container};

/// One-time generator for golden test files. Run with:
/// cargo test -p ironplc-vm-cli --test cli generate_golden -- --ignored --nocapture
///
/// Both goldens are frozen artifacts that exercise the container reader
/// end-to-end, and both must be refreshed whenever `FORMAT_VERSION` bumps:
/// the reader only accepts the current version. Last refreshed for the
/// format version 7 -> 8 persistent extent bump (ADR-0073), and before that
/// for the 6 -> 7 array-descriptor stride bump (ADR-0069), which changed only
/// the version field because the file has no type section, and for the
/// population of the header integrity hashes (`content_hash`,
/// `debug_hash`, `layout_hash`), so a golden load also exercises the
/// ADR-0006 load-time verifier (REQ-CF-container-029).
/// Adding a new entry to this generator is fine; if you ever need to
/// refresh a golden, do it from a throwaway script with full awareness of
/// what the format change is.
#[test]
#[ignore]
fn generate_golden_files() {
    let steel_thread_path = path_to_golden_resource("steel_thread.iplc");
    write_steel_thread_container(&steel_thread_path);
    eprintln!("Generated golden file: {}", steel_thread_path.display());

    let path = path_to_golden_resource("debug_source_file_table.iplc");
    write_debug_source_file_table_container(&path);
    eprintln!("Generated golden file: {}", path.display());
}

fn path_to_golden_resource(name: &str) -> PathBuf {
    let mut path = PathBuf::from(env!("CARGO_MANIFEST_DIR"));
    path.push("resources");
    path.push("test");
    path.push(name);
    path
}

/// Builds a container exercising the debug section features described in
/// `specs/design/debugger-support.md` §"Tag Registry":
///
/// - `SOURCE_FILE_TABLE` (tag 6) with two entries (`main.st`, `lib.st`)
///   whose `content_hash` fields are real BLAKE3 digests of synthetic
///   source bytes.
/// - A `LINE_MAP` (tag 1) whose entries reference both files via
///   `file_id`, including one entry at the same `(line, column)` as
///   another but with a different `file_id` to exercise the post-PR
///   wire layout.
///
/// The bytecode itself is the same `x := 10; y := x + 32` steel thread,
/// so the example runs end-to-end under `ironplcvm`. Loading the file
/// also exercises the BLAKE3-vs-SHA-256 spec change (`header.source_hash`
/// is gone; bytes 40-71 must be zero), and the backwards-compat read
/// of the original `steel_thread.iplc` is verified by the other vm-cli
/// tests.
fn write_debug_source_file_table_container(path: &Path) {
    use ironplc_container::debug_section::{LineMapEntry, SourceFileEntry};
    use ironplc_container::id_types::{FunctionId, SourceColumn, SourceFileId, SourceLine};

    let main_source =
        b"PROGRAM main\nVAR x, y : DINT; END_VAR\nx := 10;\ny := lib_add(x, 32);\nEND_PROGRAM\n";
    let lib_source = b"FUNCTION lib_add : DINT\nVAR_INPUT a, b : DINT; END_VAR\nlib_add := a + b;\nEND_FUNCTION\n";

    // Same base as `write_steel_thread_container` — the bytecode, constants
    // and `x`/`y` debug names — plus the source-file table and line map this
    // fixture exists to exercise.
    let container = steel_thread_debug_builder()
        // file_id = 0 is the program file, file_id = 1 the library.
        .add_source_file(SourceFileEntry {
            path: "src/main.st".into(),
            content_hash: *blake3::hash(main_source).as_bytes(),
        })
        .add_source_file(SourceFileEntry {
            path: "src/lib.st".into(),
            content_hash: *blake3::hash(lib_source).as_bytes(),
        })
        // LineMap mixing file_ids: the assignment lines are in main.st,
        // the addition is "inlined" from lib.st. The exact line/column
        // numbers are illustrative.
        .add_line_map_entry(LineMapEntry {
            function_id: FunctionId::SCAN,
            bytecode_offset: 0,
            file_id: SourceFileId::new(0),
            source_line: SourceLine::new(3),
            source_column: SourceColumn::new(1),
        })
        .add_line_map_entry(LineMapEntry {
            function_id: FunctionId::SCAN,
            bytecode_offset: 9,
            file_id: SourceFileId::new(1),
            source_line: SourceLine::new(3),
            source_column: SourceColumn::new(1),
        })
        .add_line_map_entry(LineMapEntry {
            function_id: FunctionId::SCAN,
            bytecode_offset: 13,
            file_id: SourceFileId::new(0),
            source_line: SourceLine::new(4),
            source_column: SourceColumn::new(1),
        })
        .build();

    write_container(&container, path);
}

/// The frozen golden predates hashing, so its header carries no hash and
/// the reader accepts it unchecked.
#[test]
fn run_when_golden_container_file_then_ok() -> Result<(), Box<dyn std::error::Error>> {
    let golden_path = path_to_golden_resource("steel_thread.iplc");
    let dir = TempDir::new()?;
    let dump_path = dir.path().join("vars.txt");

    let mut cmd = Command::new(cargo::cargo_bin!("ironplcvm"));
    cmd.arg("run")
        .arg(&golden_path)
        .arg("--dump-vars")
        .arg(&dump_path)
        .arg("--scans")
        .arg("1");
    cmd.assert().success();

    let contents = std::fs::read_to_string(&dump_path)?;
    // The golden is a frozen artifact written by the current container
    // writer: loading it succeeds only because its nonzero integrity hashes
    // verify against its sections — the golden load is itself the
    // end-to-end check of the ADR-0006 load-time verifier. Containers with
    // zero hashes (written before the hashes were populated) stay loadable
    // and are covered by the container crate's legacy-accept tests.
    assert_eq!(contents, "x: 10\ny: 42\n");

    Ok(())
}

/// Loads the debug-source-file-table example container (generated by the
/// `generate_golden_files` ignored test) and asserts the new debug
/// section features round-trip through the reader.
#[test]
fn read_when_debug_source_file_table_golden_then_decodes_new_debug_fields() {
    use ironplc_container::id_types::FunctionId;
    use ironplc_container::Container;
    use std::io::Cursor;

    let bytes = std::fs::read(path_to_golden_resource("debug_source_file_table.iplc")).unwrap();
    let container = Container::read_from(&mut Cursor::new(&bytes)).unwrap();

    let debug = container.debug_section.as_ref().unwrap();

    // SOURCE_FILE_TABLE round-trip
    assert_eq!(debug.source_files.len(), 2);
    assert_eq!(debug.source_files[0].path, "src/main.st");
    assert_eq!(debug.source_files[1].path, "src/lib.st");
    let expected_main = *blake3::hash(
        b"PROGRAM main\nVAR x, y : DINT; END_VAR\nx := 10;\ny := lib_add(x, 32);\nEND_PROGRAM\n",
    )
    .as_bytes();
    let expected_lib =
        *blake3::hash(b"FUNCTION lib_add : DINT\nVAR_INPUT a, b : DINT; END_VAR\nlib_add := a + b;\nEND_FUNCTION\n")
            .as_bytes();
    assert_eq!(debug.source_files[0].content_hash, expected_main);
    assert_eq!(debug.source_files[1].content_hash, expected_lib);

    // LineMap mixes file_ids
    let lm = &debug.line_map;
    assert_eq!(lm.len(), 3);
    assert!(lm.iter().all(|e| e.function_id == FunctionId::SCAN));
    assert!(lm.iter().any(|e| e.file_id.raw() == 0));
    assert!(lm.iter().any(|e| e.file_id.raw() == 1));

    // The new `header.source_hash` is gone; the wire slot must be zero.
    assert_eq!(container.header.reserved_hash_slot, [0u8; 32]);
}
