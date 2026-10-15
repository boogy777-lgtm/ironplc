//! Tests of the `serve` commit store: assemble, reboot recovery and persistence failure.

use std::path::Path;

use assert_cmd::cargo;
use tempfile::TempDir;

use crate::{compiled_bytes, write_compiled_container, COUNTER_PROGRAM};

/// A logic-only edit of [`COUNTER_PROGRAM`] that divides by zero on every
/// scan (`x - x` is always 0). Stages against the counter, so it can be
/// committed and then recognized after a reboot: the driven round of the
/// running trapper faults with V4001 on stderr.
const TRAPPER_PROGRAM: &str = "
PROGRAM main
  VAR
    Counter : DINT;
  END_VAR
  Counter := 1 / (Counter - Counter);
END_PROGRAM
";

/// One `ironplcvm serve` process fed `stdin`, asserted to exit 0.
fn serve_with_stdin(path: &Path, stdin: &str) -> std::process::Output {
    let mut cmd = assert_cmd::Command::new(cargo::cargo_bin!("ironplcvm"));
    cmd.arg("serve").arg(path).write_stdin(stdin);
    cmd.assert().success().get_output().clone()
}

/// The marker JSON of the store file `name` beside `counter.iplc`.
fn marker_value(dir: &TempDir, name: &str) -> serde_json::Value {
    let text = std::fs::read_to_string(dir.path().join(format!("counter.iplc.{name}"))).unwrap();
    serde_json::from_str(&text).unwrap()
}

fn write_marker_value(dir: &TempDir, name: &str, value: serde_json::Value) {
    std::fs::write(
        dir.path().join(format!("counter.iplc.{name}")),
        value.to_string(),
    )
    .unwrap();
}

/// The accept/test/assemble lines that commit `edit` in one session.
fn commit_stdin(edit: &[u8]) -> String {
    let accept = serde_json::json!({"command": "acceptEdits", "program": edit}).to_string();
    format!("{accept}\n{{\"command\":\"testEdits\"}}\n{{\"command\":\"assembleEdits\"}}\n")
}

/// A probe session that reveals which artifact booted: accept the clean
/// counter edit, test it, then untest — the untest's driven round runs the
/// BOOTED artifact. When the committed trapper booted, that round faults
/// with V4001 on stderr; the clean counter rounds stay silent.
fn probe_stdin() -> String {
    let accept =
        serde_json::json!({"command": "acceptEdits", "program": compiled_bytes(COUNTER_PROGRAM)})
            .to_string();
    format!("{accept}\n{{\"command\":\"testEdits\"}}\n{{\"command\":\"untestEdits\"}}\n")
}

/// ADR-0064 amendment: accept → test → assemble commits the candidate's exact
/// wire bytes beside the served container, and a reboot (a new serve process)
/// boots the committed artifact from the store.
#[test]
fn serve_when_assemble_then_reboot_loads_the_committed_artifact(
) -> Result<(), Box<dyn std::error::Error>> {
    let dir = TempDir::new()?;
    let container_path = dir.path().join("counter.iplc");
    write_compiled_container(&container_path, COUNTER_PROGRAM);

    // Session 1: accept the trapper, run it under Test, assemble it. The
    // driven boundary rounds trap (the candidate divides by zero) — that is
    // the program's own fault, logged to stderr, and does not change the
    // wire answers or the commit.
    let trapper = compiled_bytes(TRAPPER_PROGRAM);
    let output = serve_with_stdin(&container_path, &commit_stdin(&trapper));
    let lines: Vec<&str> = std::str::from_utf8(&output.stdout)?.lines().collect();
    assert_eq!(lines.len(), 3);
    for line in lines {
        let response: serde_json::Value = serde_json::from_str(line)?;
        assert_eq!(response["response"], "ack");
    }

    // The commit landed: slot B holds the wire bytes verbatim (never a
    // re-serialization) and the marker flipped to the new slot.
    assert_eq!(
        std::fs::read(dir.path().join("counter.iplc.slot-b"))?,
        trapper
    );
    assert_eq!(
        marker_value(&dir, "marker"),
        serde_json::json!({"slot": "b", "seq": 2})
    );

    // Session 2, the simulated reboot: the probe's untest round runs the
    // booted artifact — the committed trapper faults, proving the identity.
    let output = serve_with_stdin(&container_path, &probe_stdin());
    let stderr = String::from_utf8(output.stderr)?;
    assert!(
        stderr.contains("V4001"),
        "expected the committed trapper to boot and fault the probe round, stderr:\n{stderr}"
    );
    Ok(())
}

/// No assemble, no commit: a reboot boots the originally served artifact.
#[test]
fn serve_without_assemble_then_reboot_loads_the_original_artifact(
) -> Result<(), Box<dyn std::error::Error>> {
    let dir = TempDir::new()?;
    let container_path = dir.path().join("counter.iplc");
    write_compiled_container(&container_path, COUNTER_PROGRAM);

    // Session 1 only seeds the store (first serve); nothing commits.
    serve_with_stdin(&container_path, "{\"command\":\"getStatus\"}\n");

    let output = serve_with_stdin(&container_path, &probe_stdin());
    let stderr = String::from_utf8(output.stderr)?;
    assert!(
        !stderr.contains("V4001"),
        "expected the clean counter to boot, stderr:\n{stderr}"
    );
    Ok(())
}

/// Crash residue of a tmp write (partial, unverified) with the marker and
/// the active slot intact: boot the marker's generation; discard the tmp.
#[test]
fn serve_when_tmp_residue_present_then_reboot_boots_the_marker_generation(
) -> Result<(), Box<dyn std::error::Error>> {
    let dir = TempDir::new()?;
    let container_path = dir.path().join("counter.iplc");
    write_compiled_container(&container_path, COUNTER_PROGRAM);

    // Session 1 seeds the store; then simulate a crash during a tmp write.
    serve_with_stdin(&container_path, "{\"command\":\"getStatus\"}\n");
    std::fs::write(dir.path().join("counter.iplc.tmp"), "partial write")?;

    let output = serve_with_stdin(&container_path, &probe_stdin());
    let stderr = String::from_utf8(output.stderr)?;
    assert!(
        !stderr.contains("V4001"),
        "expected the marker's generation (the clean counter), stderr:\n{stderr}"
    );
    assert!(!dir.path().join("counter.iplc.tmp").exists());
    Ok(())
}

/// Crash during the marker flip: the marker is missing but the residue names
/// a verified slot — boot it and heal the marker.
#[test]
fn serve_when_marker_missing_then_reboot_adopts_the_residue_and_heals(
) -> Result<(), Box<dyn std::error::Error>> {
    let dir = TempDir::new()?;
    let container_path = dir.path().join("counter.iplc");
    write_compiled_container(&container_path, COUNTER_PROGRAM);
    let trapper = compiled_bytes(TRAPPER_PROGRAM);
    serve_with_stdin(&container_path, &commit_stdin(&trapper));

    // Recreate the flip window: marker deleted, residue names the committed
    // slot.
    std::fs::remove_file(dir.path().join("counter.iplc.marker"))?;
    write_marker_value(
        &dir,
        "marker.tmp",
        serde_json::json!({"slot": "b", "seq": 2}),
    );

    let output = serve_with_stdin(&container_path, &probe_stdin());
    let stderr = String::from_utf8(output.stderr)?;
    assert!(
        stderr.contains("V4001"),
        "expected the residue-named trapper to boot, stderr:\n{stderr}"
    );
    assert_eq!(
        marker_value(&dir, "marker"),
        serde_json::json!({"slot": "b", "seq": 2})
    );
    assert!(!dir.path().join("counter.iplc.marker.tmp").exists());
    Ok(())
}

/// Stale marker: the old marker was restored while the residue names the
/// newer verified slot — the highest-seq verifiable record wins.
#[test]
fn serve_when_stale_marker_then_reboot_boots_the_newest_verifiable(
) -> Result<(), Box<dyn std::error::Error>> {
    let dir = TempDir::new()?;
    let container_path = dir.path().join("counter.iplc");
    write_compiled_container(&container_path, COUNTER_PROGRAM);
    let trapper = compiled_bytes(TRAPPER_PROGRAM);
    serve_with_stdin(&container_path, &commit_stdin(&trapper));

    // Stale state: the marker names the old active slot at the old seq while
    // the residue names the committed slot at the newer seq.
    write_marker_value(&dir, "marker", serde_json::json!({"slot": "a", "seq": 1}));
    write_marker_value(
        &dir,
        "marker.tmp",
        serde_json::json!({"slot": "b", "seq": 2}),
    );

    let output = serve_with_stdin(&container_path, &probe_stdin());
    let stderr = String::from_utf8(output.stderr)?;
    assert!(
        stderr.contains("V4001"),
        "expected the newest verifiable generation (the trapper) to boot, stderr:\n{stderr}"
    );
    assert_eq!(
        marker_value(&dir, "marker"),
        serde_json::json!({"slot": "b", "seq": 2})
    );
    Ok(())
}

/// ADR-0064 amendment wire honesty: a persistence failure answers the
/// assemble line with V6012 instead of an ack; the RAM promotion stands and
/// the store is unchanged, so a reboot would boot the previous generation.
#[test]
fn serve_when_persist_fails_then_assemble_answers_v6012() -> Result<(), Box<dyn std::error::Error>>
{
    let dir = TempDir::new()?;
    let container_path = dir.path().join("counter.iplc");
    write_compiled_container(&container_path, COUNTER_PROGRAM);

    // Session 1 seeds the store; then block the commit's tmp write with a
    // directory where the tmp file must land.
    serve_with_stdin(&container_path, "{\"command\":\"getStatus\"}\n");
    std::fs::create_dir(dir.path().join("counter.iplc.tmp"))?;

    let trapper = compiled_bytes(TRAPPER_PROGRAM);
    let output = serve_with_stdin(&container_path, &commit_stdin(&trapper));
    let lines: Vec<&str> = std::str::from_utf8(&output.stdout)?.lines().collect();
    assert_eq!(lines.len(), 3);
    let assembled: serde_json::Value = serde_json::from_str(lines[2])?;
    assert_eq!(assembled["response"], "error");
    assert_eq!(assembled["vCode"], "V6012");

    // The store still names the seeded generation.
    assert_eq!(
        marker_value(&dir, "marker"),
        serde_json::json!({"slot": "a", "seq": 1})
    );
    assert!(!dir.path().join("counter.iplc.slot-b").exists());
    Ok(())
}
