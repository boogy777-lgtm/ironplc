//! The `.st` sources of the repository as one corpus, and the spellings of
//! each that a comparison runs over.
//!
//! The text of a source file reaches the compiler in whatever spelling its
//! author's editor wrote: LF or CRLF line ends, tabs or spaces to indent. A
//! check over the corpus runs every spelling, so it is made once here, for
//! every file, and not once for each file that needs it.

use std::path::Path;

/// The directories read, relative to this crate, with the label that names
/// their files: the shared test resources, the syntax crate's fixtures, the
/// plc2plc round-trip files, the bundled libraries and the source-discovery
/// fixtures, the CLI resources, the end-to-end library files and the examples.
const ROOTS: [(&str, &str); 8] = [
    ("../resources/test", "../resources/test"),
    ("../syntax/tests/fixtures", "tests/fixtures"),
    ("../plc2plc/resources/test", "../plc2plc/resources/test"),
    ("../sources/resources/libs", "../sources/resources/libs"),
    ("../sources/resources/test", "../sources/resources/test"),
    (
        "../ironplc-cli/resources/test",
        "../ironplc-cli/resources/test",
    ),
    ("../../tests/e2e/library", "../../tests/e2e/library"),
    ("../../examples", "../../examples"),
];

/// The suffix of the name of the spelling with CRLF line ends.
pub const CRLF: &str = " (CRLF)";

/// The suffix of the name of the spelling indented with tabs.
pub const TABS: &str = " (tabs)";

/// Every file of the corpus, each in three spellings: as written, with CRLF
/// line ends (`name (CRLF)`) and indented with tabs (`name (tabs)`). The name
/// is the label of its directory and its path inside it. A file that is not
/// valid UTF-8 reads as empty.
pub fn variants() -> Vec<(String, String)> {
    let manifest = Path::new(env!("CARGO_MANIFEST_DIR"));
    let mut files = Vec::new();
    for (root, label) in ROOTS {
        let root = manifest.join(root);
        for path in crate::st_files(&root) {
            let text = std::fs::read_to_string(&path).unwrap_or_default();
            let relative = path
                .strip_prefix(&root)
                .unwrap_or(&path)
                .to_string_lossy()
                .replace('\\', "/");
            let name = format!("{label}/{relative}");
            files.push((format!("{name}{CRLF}"), text.replace('\n', "\r\n")));
            files.push((format!("{name}{TABS}"), text.replace("    ", "\t")));
            files.push((name, text));
        }
    }
    files
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn variants_when_read_then_each_file_has_three_spellings_under_one_name() {
        let files = variants();
        assert!(files.len() > 180, "only {} files", files.len());
        let name = "../resources/test/oop.st";
        let find = |name: &str| files.iter().find(|(key, _)| key == name).map(|(_, t)| t);
        let plain = find(name).expect("the file is in the corpus");
        let crlf = find(&format!("{name}{CRLF}")).expect("its CRLF spelling");
        let tabs = find(&format!("{name}{TABS}")).expect("its tab spelling");
        assert!(!plain.contains('\r'));
        assert_eq!(&crlf.replace("\r\n", "\n"), plain);
        assert_eq!(&tabs.replace('\t', "    "), plain);
    }
}
