//! The serve-session tests: the scripted hot-edit protocol over the
//! stdio session loop (REQ-VC-vm-cli-019 through -023), the slot-store
//! commit path, and the HA engineering surface composed into the same
//! session — every HA query in both standalone and simulated-peer modes,
//! the commanded swap and its permit demotion, the budget refusal, and
//! the partition/resync cycle driven purely by session lines.
//!
//! The tests live beside `main.rs` as a feature file (the module-size
//! rule): `serve.rs` stays the session loop, this module is its table-
//! driven client.

use std::io;

use ironplc_container::Container;
use ironplc_redundancy::Shell;
use ironplc_runtime::{execute, parse_command, Command, Response, RuntimeHost};
use spec_test_macro::spec_test;

use crate::serve::{compose_host, device_identity, drive_scan_round, serve_session};
use crate::slot_store::SlotStore;

/// A `PROGRAM main` with one DINT `Counter` incremented by `step` per
/// scan; same declarations for every step, so two compiles differ only
/// in scan logic and share a layout.
fn counter_source(step: i32) -> String {
    format!(
        "PROGRAM main
  VAR
    Counter : DINT;
  END_VAR
  Counter := Counter + {step};
END_PROGRAM
"
    )
}

/// Compiles `source` through the real pipeline and round-trips the
/// container through the wire format, mirroring
/// `tests/cli.rs::write_compiled_container` and the runtime test fixtures,
/// so `header.layout_hash` is the value a deployed artifact carries.
fn compile_container(source: &str) -> Container {
    let options = ironplc_parser::options::CompilerOptions::default();
    let library =
        ironplc_parser::parse_program(source, &ironplc_dsl::core::FileId::default(), &options)
            .unwrap();
    let (analyzed, context) =
        ironplc_analyzer::stages::resolve_types(&[&library], &options).unwrap();
    let container = ironplc_codegen::compile(
        &analyzed,
        &context,
        &ironplc_codegen::CodegenOptions::default(),
        &ironplc_codegen::EmptyLookup,
    )
    .unwrap();

    let mut bytes = Vec::new();
    container.write_to(&mut bytes).unwrap();
    Container::read_from(&mut io::Cursor::new(&bytes)).unwrap()
}

/// The wire-format bytes an `acceptEdits` command carries for `source`.
fn container_bytes(source: &str) -> Vec<u8> {
    let mut bytes = Vec::new();
    compile_container(source).write_to(&mut bytes).unwrap();
    bytes
}

/// [`compile_container`] for a source compiled with engineering-side
/// stable variable IDs (ADR 0053), so a declaration edit stages as a
/// migration candidate.
fn compile_container_with_ids(source: &str, ids: &[(&str, u64)]) -> Container {
    let options = ironplc_parser::options::CompilerOptions::default();
    let library =
        ironplc_parser::parse_program(source, &ironplc_dsl::core::FileId::default(), &options)
            .unwrap();
    let (analyzed, context) =
        ironplc_analyzer::stages::resolve_types(&[&library], &options).unwrap();
    let codegen_options = ironplc_codegen::CodegenOptions {
        stable_var_ids: ids
            .iter()
            .map(|(name, uid)| (ironplc_dsl::core::Id::from(name), *uid))
            .collect(),
        ..ironplc_codegen::CodegenOptions::default()
    };
    let container = ironplc_codegen::compile(
        &analyzed,
        &context,
        &codegen_options,
        &ironplc_codegen::EmptyLookup,
    )
    .unwrap();

    let mut bytes = Vec::new();
    container.write_to(&mut bytes).unwrap();
    Container::read_from(&mut io::Cursor::new(&bytes)).unwrap()
}

/// The wire-format bytes an `acceptEdits` command carries for `source`
/// compiled with stable variable IDs.
fn container_bytes_with_ids(source: &str, ids: &[(&str, u64)]) -> Vec<u8> {
    let mut bytes = Vec::new();
    compile_container_with_ids(source, ids)
        .write_to(&mut bytes)
        .unwrap();
    bytes
}

/// A `PROGRAM main` whose single `Counter` carries `ty`, incremented by
/// one per scan (the retype target of the migration wire tests).
fn retyped_counter_source(ty: &str) -> String {
    format!(
        "PROGRAM main
  VAR
    Counter : {ty};
  END_VAR
  Counter := Counter + 1;
END_PROGRAM
"
    )
}

/// The wire-format bytes of a candidate whose scan divides by the
/// zero-initialized `Counter`, so the first driven round traps.
fn trapping_container_bytes() -> Vec<u8> {
    container_bytes(
        "PROGRAM main
  VAR
    Counter : DINT;
  END_VAR
  Counter := 1 / Counter;
END_PROGRAM
",
    )
}

/// Runs `lines` through one session without a slot store, returning the
/// parsed response per input line.
fn run_session(host: &mut RuntimeHost, lines: &[&str]) -> Vec<serde_json::Value> {
    run_session_with_store(host, lines, None)
}

/// Runs `lines` through one session, optionally persisting assembles
/// through a slot store, returning the parsed response per input line.
fn run_session_with_store(
    host: &mut RuntimeHost,
    lines: &[&str],
    store: Option<&mut SlotStore>,
) -> Vec<serde_json::Value> {
    let mut input = lines.join("\n");
    input.push('\n');
    let mut output = Vec::new();
    serve_session(
        host,
        io::Cursor::new(input.into_bytes()),
        &mut output,
        store,
        &device_identity(),
        None,
    )
    .unwrap();
    let text = String::from_utf8(output).unwrap();
    text.lines()
        .map(|line| serde_json::from_str(line).unwrap())
        .collect()
}

/// Runs `lines` through one session over the composed HA shell, returning
/// the parsed response per input line. The shell is the session's
/// simulation clock: every line ticks it.
fn run_ha_session(
    host: &mut RuntimeHost,
    shell: &mut Shell,
    lines: &[&str],
) -> Vec<serde_json::Value> {
    let mut input = lines.join("\n");
    input.push('\n');
    let mut output = Vec::new();
    serve_session(
        host,
        io::Cursor::new(input.into_bytes()),
        &mut output,
        None,
        &device_identity(),
        Some(shell),
    )
    .unwrap();
    let text = String::from_utf8(output).unwrap();
    text.lines()
        .map(|line| serde_json::from_str(line).unwrap())
        .collect()
}

/// A host over the compiled counter: the standalone-shell composition
/// grants the permit at startup.
fn counter_host() -> RuntimeHost {
    let (host, _shell) = compose_host(compile_container(&counter_source(1)), false).unwrap();
    host
}

/// A host + shell over the compiled counter in the given HA mode.
fn counter_ha(simulated: bool) -> (RuntimeHost, Shell) {
    compose_host(compile_container(&counter_source(1)), simulated).unwrap()
}

/// REQ-VC-vm-cli-019: every command line gets exactly one response line,
/// so a scripted session stays aligned line for line.
#[spec_test(REQ_VC_vm_cli_019)]
#[test]
fn serve_session_when_scripted_sequence_then_one_response_line_per_command() {
    let edit = container_bytes(&counter_source(10));
    let accept = serde_json::json!({"command": "acceptEdits", "program": edit}).to_string();
    let lines = [
        r#"{"command":"getStatus"}"#,
        &accept,
        r#"{"command":"testEdits"}"#,
        r#"{"command":"assembleEdits"}"#,
        r#"{"command":"cancelEdits"}"#,
        r#"{"command":"getStatus"}"#,
    ];
    let mut host = counter_host();

    let responses = run_session(&mut host, &lines);

    assert_eq!(responses.len(), lines.len());
    assert_eq!(responses[0]["response"], "status");
    assert_eq!(responses[0]["mode"], "normal");
    assert_eq!(responses[0]["candidate"], serde_json::Value::Null);
    assert_eq!(responses[1]["response"], "ack");
    // Each FSM-advancing command drives one scan round at the boundary,
    // so the test swap is applied — and the assemble promotion likewise
    // — by the time its acknowledgment is written: the whole scripted
    // path is ack. ADR-0064: assemble follows the test, never Accepted.
    assert_eq!(responses[2]["response"], "ack");
    assert_eq!(responses[3]["response"], "ack");
    // Assembling left nothing staged, so the final cancel is refused
    // with the protocol's usual V-code.
    assert_eq!(responses[4]["response"], "error");
    assert_eq!(responses[4]["vCode"], "V4012");
    // Two rounds were driven (test, assemble): the candidate is the
    // promoted, running application.
    assert_eq!(responses[5]["response"], "status");
    assert_eq!(responses[5]["mode"], "normal");
    assert_eq!(responses[5]["normal"], 2);
    assert_eq!(responses[5]["application"], 2);
    assert_eq!(responses[5]["candidate"], serde_json::Value::Null);
    assert_eq!(responses[5]["rounds"], 2);
}

/// The `identity` handshake (ADR-0063) answers over the stdio session too:
/// the device block is the `vm-cli` composition, the application block is
/// the same status payload `getStatus` answers with, and no redundancy
/// block means standalone.
#[test]
fn serve_session_when_identity_then_device_panel_and_status_snapshot() {
    let mut host = counter_host();

    let responses = run_session(
        &mut host,
        &[r#"{"command":"identity"}"#, r#"{"command":"getStatus"}"#],
    );

    assert_eq!(responses.len(), 2);
    assert_eq!(responses[0]["response"], "identity");
    assert_eq!(responses[0]["protocol"], 1);
    assert_eq!(responses[0]["device"]["name"], "ironplcvm");
    assert_eq!(responses[0]["device"]["model"], "IronPLC SoftPLC");
    assert_eq!(responses[0]["device"]["modification"], "vm-cli");
    // The firmware version is the binary's own crate version — the one
    // composition point for the device panel (ADR-0063).
    assert_eq!(
        responses[0]["device"]["firmwareVersion"],
        env!("CARGO_PKG_VERSION")
    );
    // The application block is the StatusPayload vocabulary verbatim —
    // the same fields getStatus answers with, minus the response tag.
    let mut status = responses[1].clone();
    status.as_object_mut().unwrap().remove("response");
    assert_eq!(responses[0]["application"], status);
    assert!(responses[0].get("redundancy").is_none());
}

/// REQ-VC-vm-cli-023: a trap in a driven round does not change the
/// protocol answer (the swap was recorded, so the wire carries the ack);
/// the trap surfaces with its V-code and the session keeps serving.
#[spec_test(REQ_VC_vm_cli_023)]
#[test]
fn serve_session_when_driven_round_traps_then_ack_on_wire_and_session_continues() {
    let accept =
        serde_json::json!({"command": "acceptEdits", "program": trapping_container_bytes()})
            .to_string();
    let lines = [
        &accept,
        r#"{"command":"testEdits"}"#,
        r#"{"command":"getStatus"}"#,
    ];
    let mut host = counter_host();

    let responses = run_session(&mut host, &lines);

    assert_eq!(responses.len(), lines.len());
    assert_eq!(responses[0]["response"], "ack");
    // The driven boundary round trapped (divide by zero, V4001, logged
    // to stderr); the acknowledgment still stands and the session
    // answers the next command.
    assert_eq!(responses[1]["response"], "ack");
    assert_eq!(responses[2]["response"], "status");
}

/// A slot store booted in a fresh temp dir beside a written counter
/// file (the first boot seeds slot A and the marker).
fn booted_store(dir: &tempfile::TempDir) -> SlotStore {
    let file = dir.path().join("app.iplc");
    std::fs::write(&file, container_bytes(&counter_source(1))).unwrap();
    let store = SlotStore::beside(&file);
    store.boot().unwrap();
    store
}

/// The assemble session: accept a candidate, test it, assemble it.
fn accept_test_assemble_lines(edit: Vec<u8>) -> Vec<String> {
    let accept = serde_json::json!({"command": "acceptEdits", "program": edit}).to_string();
    vec![
        accept,
        r#"{"command":"testEdits"}"#.to_string(),
        r#"{"command":"assembleEdits"}"#.to_string(),
    ]
}

#[test]
fn serve_session_when_assemble_from_accepted_then_v4017_on_wire() {
    let mut host = counter_host();
    let edit = container_bytes(&counter_source(10));
    let accept = serde_json::json!({"command": "acceptEdits", "program": edit}).to_string();
    let lines = [&accept, r#"{"command":"assembleEdits"}"#];

    // Assemble before the candidate ever ran under Test refuses with the
    // ADR-0064 V-code; test+assemble would ack.
    let responses = run_session(&mut host, &lines);

    assert_eq!(responses[0]["response"], "ack");
    assert_eq!(responses[1]["response"], "error");
    assert_eq!(responses[1]["vCode"], "V4017");
    assert!(host.status().candidate.is_some());
}

#[test]
fn serve_session_when_assemble_with_store_then_committed_bytes_land_in_inactive_slot() {
    let dir = tempfile::TempDir::new().unwrap();
    let mut store = booted_store(&dir);
    let edit = container_bytes(&counter_source(10));
    let lines = accept_test_assemble_lines(edit.clone());
    let lines: Vec<&str> = lines.iter().map(String::as_str).collect();
    let mut host = counter_host();

    let responses = run_session_with_store(&mut host, &lines, Some(&mut store));

    assert_eq!(responses[2]["response"], "ack");
    // Slot contents are exactly the accepted wire bytes — never a
    // re-serialization (ADR-0064 amendment).
    let slot_b = std::fs::read(dir.path().join("app.iplc.slot-b")).unwrap();
    assert_eq!(slot_b, edit);
    // The marker flipped to the inactive slot at seq 2.
    let marker = std::fs::read_to_string(dir.path().join("app.iplc.marker")).unwrap();
    let marker: serde_json::Value = serde_json::from_str(&marker).unwrap();
    assert_eq!(marker["slot"], "b");
    assert_eq!(marker["seq"], 2);
}

#[test]
fn serve_session_when_persist_fails_then_v6012_on_wire_and_ram_promotion_stands() {
    let dir = tempfile::TempDir::new().unwrap();
    let mut store = booted_store(&dir);
    // Block the commit's tmp write: a directory where the tmp file lands.
    std::fs::create_dir(dir.path().join("app.iplc.tmp")).unwrap();
    let lines = accept_test_assemble_lines(container_bytes(&counter_source(10)));
    let lines: Vec<&str> = lines.iter().map(String::as_str).collect();
    let mut host = counter_host();

    let responses = run_session_with_store(&mut host, &lines, Some(&mut store));

    // The acknowledgment became a V6012 error line (ADR-0064 amendment);
    // the host promotion still stands, so a reboot would boot the
    // previous committed generation.
    assert_eq!(responses[2]["response"], "error");
    assert_eq!(responses[2]["vCode"], "V6012");
    assert_eq!(host.status().normal.raw(), 2);
    let marker = std::fs::read_to_string(dir.path().join("app.iplc.marker")).unwrap();
    assert!(marker.contains("\"seq\":1"));
}

#[test]
fn serve_session_when_type_change_without_decision_then_v4010_with_pairs() {
    let base = compile_container_with_ids(&counter_source(1), &[("Counter", 1)]);
    let mut host = RuntimeHost::new(base).unwrap();
    host.permit_execution();
    host.run(3, || 0).unwrap();

    let candidate = container_bytes_with_ids(&retyped_counter_source("UDINT"), &[("Counter", 1)]);
    let accept = serde_json::json!({"command": "acceptEdits", "program": candidate}).to_string();

    let responses = run_session(&mut host, &[&accept]);

    assert_eq!(responses.len(), 1);
    assert_eq!(responses[0]["response"], "error");
    assert_eq!(responses[0]["vCode"], "V4010");
    assert_eq!(
        responses[0]["pairs"],
        serde_json::json!([{
            "uid": 1,
            "name": "Counter",
            "from": "I32",
            "to": "U32",
            "sizeEqual": true,
        }])
    );
    // The refusal staged nothing; the engineer can resubmit the same
    // container bytes with a decision.
    assert_eq!(host.status().candidate, None);
}

#[test]
fn serve_session_when_preserve_decision_then_old_bits_survive_the_retype() {
    let base = compile_container_with_ids(&counter_source(1), &[("Counter", 1)]);
    let mut host = RuntimeHost::new(base).unwrap();
    host.permit_execution();
    host.run(3, || 0).unwrap();

    let candidate = container_bytes_with_ids(&retyped_counter_source("UDINT"), &[("Counter", 1)]);
    let accept = serde_json::json!({
        "command": "acceptEdits",
        "program": candidate,
        "migration": {"1": "preserve"},
    })
    .to_string();
    let lines = [&accept, r#"{"command":"testEdits"}"#];

    let responses = run_session(&mut host, &lines);

    assert_eq!(responses[0]["response"], "ack");
    assert!(host.status().migration);
    // The testEdits acknowledgment drove one boundary round: the DINT 3
    // (0x3) was preserved bit for bit and the UDINT body added one.
    assert_eq!(responses[1]["response"], "ack");
    assert_eq!(
        host.read_variable(ironplc_container::VarIndex::new(0))
            .unwrap(),
        4
    );
}

#[test]
fn serve_session_when_init_decision_then_candidate_initial_value_stands() {
    let base = compile_container_with_ids(&counter_source(1), &[("Counter", 1)]);
    let mut host = RuntimeHost::new(base).unwrap();
    host.permit_execution();
    host.run(3, || 0).unwrap();

    let source = "PROGRAM main
  VAR
    Counter : UDINT := 100;
  END_VAR
  Counter := Counter + 1;
END_PROGRAM
";
    let candidate = container_bytes_with_ids(source, &[("Counter", 1)]);
    let accept = serde_json::json!({
        "command": "acceptEdits",
        "program": candidate,
        "migration": {"1": "init"},
    })
    .to_string();
    let lines = [&accept, r#"{"command":"testEdits"}"#];

    let responses = run_session(&mut host, &lines);

    assert_eq!(responses[0]["response"], "ack");
    // The old 3 was discarded; the candidate's declared 100 initialized
    // the variable before the candidate body ran.
    assert_eq!(responses[1]["response"], "ack");
    assert_eq!(
        host.read_variable(ironplc_container::VarIndex::new(0))
            .unwrap(),
        101
    );
}

#[test]
fn drive_scan_round_when_round_traps_then_trap_v_code() {
    let mut host = counter_host();
    let device = device_identity();
    let accept = parse_command(
        &serde_json::json!({"command": "acceptEdits", "program": trapping_container_bytes()})
            .to_string(),
    )
    .unwrap();
    assert!(matches!(execute(accept, &mut host, &device), Response::Ack));
    assert!(matches!(
        execute(Command::TestEdits, &mut host, &device),
        Response::Ack
    ));

    let err = drive_scan_round(&mut host, None).unwrap();

    assert!(err
        .to_string()
        .starts_with("V4001 - runtime error: divide by zero"));
}

/// REQ-VC-vm-cli-020: a malformed line is a codec error — no V-code — so
/// the answer carries a null vCode and the session keeps serving.
#[spec_test(REQ_VC_vm_cli_020)]
#[test]
fn serve_session_when_malformed_line_then_null_vcode_error_and_session_continues() {
    let mut host = counter_host();

    let responses = run_session(&mut host, &["not json", r#"{"command":"getStatus"}"#]);

    assert_eq!(responses.len(), 2);
    assert_eq!(responses[0]["response"], "error");
    assert_eq!(responses[0]["vCode"], serde_json::Value::Null);
    assert!(responses[0]["message"]
        .as_str()
        .unwrap()
        .starts_with("invalid command line:"));
    assert_eq!(responses[1]["response"], "status");
}

/// REQ-VC-vm-cli-021: EOF ends the session cleanly, with no output when
/// no commands were sent.
#[spec_test(REQ_VC_vm_cli_021)]
#[test]
fn serve_session_when_eof_immediately_then_ok_and_silent() {
    let mut host = counter_host();
    let mut output = Vec::new();

    serve_session(
        &mut host,
        io::Cursor::new(Vec::new()),
        &mut output,
        None,
        &device_identity(),
        None,
    )
    .unwrap();

    assert!(output.is_empty());
}

/// A BufRead whose first read fails.
struct FailingReader;

impl io::Read for FailingReader {
    fn read(&mut self, _buf: &mut [u8]) -> io::Result<usize> {
        Err(io::Error::other("simulated read failure"))
    }
}

impl io::BufRead for FailingReader {
    fn fill_buf(&mut self) -> io::Result<&[u8]> {
        Err(io::Error::other("simulated read failure"))
    }

    fn consume(&mut self, _amt: usize) {}
}

/// REQ-VC-vm-cli-022: a stdin read failure surfaces as the session's I/O
/// error, which the command maps to V6011.
#[spec_test(REQ_VC_vm_cli_022)]
#[test]
fn serve_session_when_stdin_read_fails_then_io_error() {
    let mut host = counter_host();
    let mut output = Vec::new();

    let err = serve_session(
        &mut host,
        FailingReader,
        &mut output,
        None,
        &device_identity(),
        None,
    )
    .unwrap_err();

    assert_eq!(err.kind(), io::ErrorKind::Other);
    assert!(output.is_empty());
}

/// A Write impl that always fails, used to cover the write-error path.
struct FailingWriter;

impl io::Write for FailingWriter {
    fn write(&mut self, _buf: &[u8]) -> io::Result<usize> {
        Err(io::Error::other("simulated write failure"))
    }

    fn flush(&mut self) -> io::Result<()> {
        Ok(())
    }
}

/// REQ-VC-vm-cli-022: a stdout write failure surfaces as the session's
/// I/O error, which the command maps to V6011.
#[spec_test(REQ_VC_vm_cli_022)]
#[test]
fn serve_session_when_stdout_write_fails_then_io_error() {
    let mut host = counter_host();

    let err = serve_session(
        &mut host,
        io::Cursor::new(b"{\"command\":\"getStatus\"}\n".to_vec()),
        FailingWriter,
        None,
        &device_identity(),
        None,
    )
    .unwrap_err();

    assert_eq!(err.kind(), io::ErrorKind::Other);
}

/// The six HA queries and the three actions against the standalone
/// shell: honest standalone answers, V4112 refusals, and one response
/// line per line whatever the command.
#[test]
fn serve_session_when_ha_standalone_then_honest_answers_and_v4112_refusals() {
    let (mut host, mut shell) = counter_ha(false);
    let lines = [
        r#"{"command":"haStatus"}"#,
        r#"{"command":"haCalibration"}"#,
        r#"{"command":"haBarrier"}"#,
        r#"{"command":"haIoReady"}"#,
        r#"{"command":"haTimingBudget"}"#,
        r#"{"command":"haEvents"}"#,
        r#"{"command":"haCommandedSwap"}"#,
        r#"{"command":"haRunCalibration"}"#,
        r#"{"command":"haSetTimingBudget","peerFailureConfirmation":2,"recoveryBudget":100}"#,
        r#"{"command":"identity"}"#,
    ];

    let responses = run_ha_session(&mut host, &mut shell, &lines);

    assert_eq!(responses.len(), lines.len());
    assert_eq!(responses[0]["standalone"], true);
    assert!(responses[0].get("pairId").is_none());
    assert_eq!(responses[0]["takeoverReady"], false);
    assert_eq!(responses[0]["local"]["sync"], serde_json::Value::Null);
    assert_eq!(responses[1]["state"], "unqualified");
    assert_eq!(responses[2]["modules"].as_array().unwrap().len(), 0);
    assert!(responses[2].get("limitingDevice").is_none());
    assert_eq!(responses[3]["requiredInputsObservable"], false);
    assert_eq!(responses[3]["failingItem"], "standalone");
    assert!(responses[5].get("verdict").is_none());
    assert_eq!(responses[5]["count"], 0);
    for refusal in &responses[6..9] {
        assert_eq!(refusal["vCode"], "V4112");
    }
    // Standalone identity carries no redundancy block (ADR-0063).
    assert!(responses[9].get("redundancy").is_none());
}

/// The six HA queries against the simulated pair: the full pair state
/// answers through the same one-line session, and the identity handshake
/// now carries the redundancy block.
#[test]
fn serve_session_when_ha_simulated_pair_then_full_answers_over_the_session() {
    let (mut host, mut shell) = counter_ha(true);
    let lines = [
        r#"{"command":"haStatus"}"#,
        r#"{"command":"haCalibration"}"#,
        r#"{"command":"haBarrier"}"#,
        r#"{"command":"haIoReady"}"#,
        r#"{"command":"haTimingBudget"}"#,
        r#"{"command":"haEvents"}"#,
        r#"{"command":"identity"}"#,
    ];

    let responses = run_ha_session(&mut host, &mut shell, &lines);

    assert_eq!(responses.len(), lines.len());
    assert_eq!(responses[0]["standalone"], false);
    assert_eq!(responses[0]["pairId"], "7");
    assert_eq!(responses[0]["local"]["control"], "active");
    assert_eq!(responses[0]["peer"]["control"], "idle");
    assert_eq!(responses[0]["takeoverReady"], true);
    assert_eq!(responses[1]["state"], "calibrated");
    assert_eq!(responses[2]["modules"].as_array().unwrap().len(), 2);
    assert_eq!(responses[2]["limitingDevice"], 1);
    assert_eq!(responses[3]["failingItem"], serde_json::Value::Null);
    assert_eq!(responses[4]["verdict"]["qualified"], true);
    assert_eq!(responses[4]["calculatedWorstCase"], 16);
    assert!(responses[5]["count"].as_u64().unwrap() >= 4);
    // The identity handshake fills the redundancy block from the shell.
    assert_eq!(responses[6]["redundancy"]["pairId"], "7");
    assert_eq!(responses[6]["redundancy"]["role"], "primary");
    assert_eq!(responses[6]["redundancy"]["sync"], "syncReady");
    assert_eq!(responses[6]["redundancy"]["control"], "active");
}

/// The commanded swap over the session: acknowledged, and the very next
/// query reads the exchanged roles — the state the action acknowledges
/// has actually changed.
#[test]
fn serve_session_when_ha_swap_then_ack_and_roles_exchange() {
    let (mut host, mut shell) = counter_ha(true);

    let responses = run_ha_session(
        &mut host,
        &mut shell,
        &[
            r#"{"command":"haCommandedSwap"}"#,
            r#"{"command":"haStatus"}"#,
        ],
    );

    assert_eq!(responses[0]["response"], "ack");
    assert_eq!(responses[1]["local"]["role"], "secondary");
    assert_eq!(responses[1]["local"]["control"], "idle");
    assert_eq!(responses[1]["peer"]["control"], "active");
    assert_eq!(responses[1]["takeoverReady"], true);
}

/// A second swap immediately after the first is legal (the pair is in
/// SYNC_READY with an active owner again): the roles ping-pong.
#[test]
fn serve_session_when_ha_double_swap_then_roles_ping_pong() {
    let (mut host, mut shell) = counter_ha(true);

    let responses = run_ha_session(
        &mut host,
        &mut shell,
        &[
            r#"{"command":"haCommandedSwap"}"#,
            r#"{"command":"haCommandedSwap"}"#,
            r#"{"command":"haStatus"}"#,
        ],
    );

    assert_eq!(responses[0]["response"], "ack");
    assert_eq!(responses[1]["response"], "ack");
    assert_eq!(responses[2]["local"]["role"], "primary");
    assert_eq!(responses[2]["local"]["control"], "active");
}

/// The swap demotes this unit to the Secondary: the permit latch is
/// revoked, and the hot-edit path's driven rounds refuse from the next
/// line on — observable on the wire as the rounds counter freezing
/// while the protocol answers keep flowing.
#[test]
fn serve_session_when_ha_swap_then_driven_rounds_refuse_and_rounds_freeze() {
    let (mut host, mut shell) = counter_ha(true);
    let edit = container_bytes(&counter_source(10));
    let accept = serde_json::json!({"command": "acceptEdits", "program": edit}).to_string();

    let responses = run_ha_session(
        &mut host,
        &mut shell,
        &[
            &accept,
            r#"{"command":"testEdits"}"#,
            r#"{"command":"haCommandedSwap"}"#,
            r#"{"command":"assembleEdits"}"#,
            r#"{"command":"getStatus"}"#,
        ],
    );

    assert_eq!(responses[1]["response"], "ack");
    assert_eq!(responses[1]["response"], "ack");
    assert_eq!(responses[2]["response"], "ack");
    // The assemble acknowledgment stands, but the driven boundary round
    // refused at the revoked permit latch (the swap demoted this unit):
    // the rounds counter freezes at the single pre-swap round.
    assert_eq!(responses[3]["response"], "ack");
    assert_eq!(responses[4]["rounds"], 1);
}

/// The budget refusal over the session: a budget below the demonstrated
/// worst case answers V4111 carrying the minimum demonstrated budget,
/// and the configured budget stands unchanged.
#[test]
fn serve_session_when_ha_budget_below_demonstrated_then_v4111_with_minimum() {
    let (mut host, mut shell) = counter_ha(true);

    let responses = run_ha_session(
        &mut host,
        &mut shell,
        &[
            r#"{"command":"haSetTimingBudget","peerFailureConfirmation":2,"recoveryBudget":10}"#,
            r#"{"command":"haTimingBudget"}"#,
            r#"{"command":"haSetTimingBudget","peerFailureConfirmation":4,"recoveryBudget":1000}"#,
            r#"{"command":"haRunCalibration"}"#,
        ],
    );

    assert_eq!(responses[0]["vCode"], "V4111");
    assert_eq!(responses[0]["minimumDemonstrated"], 16);
    assert_eq!(responses[1]["recoveryBudget"], 100);
    assert_eq!(responses[2]["response"], "ack");
    assert_eq!(responses[3]["response"], "ack");
    let budget = run_ha_session(&mut host, &mut shell, &[r#"{"command":"haTimingBudget"}"#]);
    assert_eq!(budget[0]["recoveryBudget"], 1000);
}

/// The partition/resync cycle driven purely by session lines: with the
/// link cut (the composition's simulation control), the keepalive lines
/// tick the pair into deSYNC, the swap is refused with V4108, and after
/// the heal the keepalives re-sync the pair back to SYNC_READY.
#[test]
fn serve_session_when_ha_partitioned_then_swap_refused_and_keepalives_resync() {
    let (mut host, mut shell) = counter_ha(true);

    // A healthy keepalive first: the tick advances the exchange.
    let healthy = run_ha_session(&mut host, &mut shell, &[r#"{"command":"haStatus"}"#]);
    assert_eq!(healthy[0]["local"]["sync"], "syncReady");

    shell.set_link_partitioned(true);
    let lines = [
        r#"{"command":"getStatus"}"#,
        r#"{"command":"getStatus"}"#,
        r#"{"command":"getStatus"}"#,
        r#"{"command":"getStatus"}"#,
        r#"{"command":"getStatus"}"#,
        r#"{"command":"getStatus"}"#,
        r#"{"command":"haStatus"}"#,
        r#"{"command":"haCommandedSwap"}"#,
    ];
    let partitioned = run_ha_session(&mut host, &mut shell, &lines);
    assert_eq!(partitioned[6]["local"]["sync"], "deSync");
    assert_eq!(partitioned[7]["vCode"], "V4108");

    shell.set_link_partitioned(false);
    let heal_lines = [
        r#"{"command":"getStatus"}"#,
        r#"{"command":"getStatus"}"#,
        r#"{"command":"getStatus"}"#,
        r#"{"command":"getStatus"}"#,
        r#"{"command":"getStatus"}"#,
        r#"{"command":"getStatus"}"#,
        r#"{"command":"getStatus"}"#,
        r#"{"command":"getStatus"}"#,
        r#"{"command":"haStatus"}"#,
    ];
    let healed = run_ha_session(&mut host, &mut shell, &heal_lines);
    assert_eq!(healed[8]["local"]["sync"], "syncReady");
    assert_eq!(healed[8]["takeoverReady"], true);
}
