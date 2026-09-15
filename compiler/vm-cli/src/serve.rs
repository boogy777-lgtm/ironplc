//! The `ironplcvm serve` command: a newline-delimited JSON command session
//! over the runtime host's hot-edit protocol.
//!
//! [`serve`] loads and starts a compiled program through the same container
//! path as `run` (see [`crate::cli::load_container`]), then answers one line
//! of JSON per line of stdin until EOF. Stdout is the protocol channel:
//! startup prints nothing, every response line is flushed as written, and
//! anything diagnostic goes to stderr through the logger.
//!
//! The session is a pure command channel: it drives no scan rounds, so a
//! `testEdits`/`untestEdits` swap — applied at the next scan boundary
//! ([`RuntimeHost::run`]) — stays pending for the whole session and commands
//! issued while it is pending are refused with the protocol's usual V-codes.

use std::io::{self, BufRead, Write};
use std::path::Path;

use ironplc_container::Container;
use ironplc_runtime::{execute, parse_command, render_response, RuntimeError, RuntimeHost};

use crate::error::{self, VmError};

/// Loads the container at `path`, starts it on the runtime host, and serves
/// commands until stdin reaches EOF.
pub fn serve(path: &Path) -> Result<(), VmError> {
    let container = crate::cli::load_container(path)?;
    let mut host = start_host(container)?;

    let stdin = io::stdin();
    let stdout = io::stdout();
    serve_session(&mut host, stdin.lock(), stdout.lock()).map_err(|err| {
        VmError::io(
            error::SESSION_IO,
            format!("unable to read or write the command session: {err}"),
        )
    })
}

/// Creates the runtime host for `container`, mapping init traps to the trap's
/// V-code exactly like `run` does.
fn start_host(container: Container) -> Result<RuntimeHost, VmError> {
    RuntimeHost::new(container).map_err(|err| match err {
        RuntimeError::Trap(context) => {
            VmError::from_trap(&context.trap, context.task_id, context.instance_id)
        }
        RuntimeError::Internal { reason } => VmError::io(
            error::SESSION_IO,
            format!("runtime host failed to start: {reason}"),
        ),
    })
}

/// Serves one command session: reads one line per command from `reader`,
/// writes one line per response to `writer`, flushing after every line.
///
/// Returns when the reader reaches EOF, or with the first I/O failure.
pub fn serve_session(
    host: &mut RuntimeHost,
    reader: impl BufRead,
    mut writer: impl Write,
) -> io::Result<()> {
    for line in reader.lines() {
        let line = line?;
        let response = match parse_command(&line) {
            Ok(command) => render_response(&execute(command, host))
                .map_err(|err| io::Error::new(io::ErrorKind::InvalidData, err))?,
            Err(err) => {
                // A malformed line is a codec error, not an online change
                // refusal: it has no V-code (ADR-0055). The transport answers
                // with one error line so the wire stays aligned — one line
                // in, one line out — and the session continues.
                log::warn!("ignoring malformed command line: {err}");
                codec_error_line(&err)
            }
        };
        writeln!(writer, "{response}")?;
        writer.flush()?;
    }
    Ok(())
}

/// Renders the transport-level error for a line that did not parse as a
/// command. Codec errors carry no V-code, so `vCode` is null.
fn codec_error_line(err: &serde_json::Error) -> String {
    serde_json::json!({
        "response": "error",
        "vCode": null,
        "message": format!("invalid command line: {err}"),
    })
    .to_string()
}

#[cfg(test)]
mod tests {
    use super::*;
    use spec_test_macro::spec_test;

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

    /// Runs `lines` through one session, returning the parsed response per
    /// input line.
    fn run_session(host: &mut RuntimeHost, lines: &[&str]) -> Vec<serde_json::Value> {
        let mut input = lines.join("\n");
        input.push('\n');
        let mut output = Vec::new();
        serve_session(host, io::Cursor::new(input.into_bytes()), &mut output).unwrap();
        let text = String::from_utf8(output).unwrap();
        text.lines()
            .map(|line| serde_json::from_str(line).unwrap())
            .collect()
    }

    fn counter_host() -> RuntimeHost {
        RuntimeHost::new(compile_container(&counter_source(1))).unwrap()
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
            r#"{"command":"untestEdits"}"#,
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
        assert_eq!(responses[2]["response"], "ack");
        // The session drives no scan rounds, so the pending test swap never
        // reaches a boundary: untest has no test in progress, and assemble
        // and cancel are refused while a swap is pending (V4014/V4015).
        assert_eq!(responses[3]["response"], "error");
        assert_eq!(responses[3]["vCode"], "V4014");
        assert_eq!(responses[4]["response"], "error");
        assert_eq!(responses[4]["vCode"], "V4015");
        assert_eq!(responses[5]["response"], "error");
        assert_eq!(responses[5]["vCode"], "V4015");
        // The candidate staged by acceptEdits is still staged at the end.
        assert_eq!(responses[6]["response"], "status");
        assert_eq!(responses[6]["candidate"], 2);
        assert_eq!(responses[6]["rounds"], 0);
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

        serve_session(&mut host, io::Cursor::new(Vec::new()), &mut output).unwrap();

        assert!(output.is_empty());
    }

    /// A BufRead whose first read fails.
    struct FailingReader;

    impl io::Read for FailingReader {
        fn read(&mut self, _buf: &mut [u8]) -> io::Result<usize> {
            Err(io::Error::other("simulated read failure"))
        }
    }

    impl BufRead for FailingReader {
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

        let err = serve_session(&mut host, FailingReader, &mut output).unwrap_err();

        assert_eq!(err.kind(), io::ErrorKind::Other);
        assert!(output.is_empty());
    }

    /// A Write impl that always fails, used to cover the write-error path.
    struct FailingWriter;

    impl Write for FailingWriter {
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
        )
        .unwrap_err();

        assert_eq!(err.kind(), io::ErrorKind::Other);
    }
}
