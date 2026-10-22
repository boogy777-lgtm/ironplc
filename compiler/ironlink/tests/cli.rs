//! Tests of the `ironlink` program itself, started as a process against fake
//! devices on loopback ports. Every process that is started is killed when its
//! guard is dropped, also when a test fails.

// Test-target boundary: the workspace denies panicking constructs in
// production code; tests assert by panicking, so they are exempt here.
#![expect(
    clippy::unwrap_used,
    reason = "integration test target: panicking helpers are sanctioned in tests"
)]

use std::io::{BufRead, BufReader, Read, Write};
use std::net::{Ipv4Addr, SocketAddr, UdpSocket};
use std::process::{Child, ChildStdin, Command, ExitStatus, Stdio};
use std::sync::atomic::{AtomicBool, Ordering};
use std::sync::mpsc::{self, Receiver};
use std::sync::Arc;
use std::thread::JoinHandle;
use std::time::{Duration, Instant};

use ironplc_discovery::{encode_reply, is_request, Reply, MAX_REPLY_LEN};
use serde_json::{json, Value};
use spec_test_macro::spec_test;

/// Spec-conformance requirements generated from `specs/design/ironlink.md`.
/// See ironlink/build.rs.
mod spec_requirements {
    include!(concat!(env!("OUT_DIR"), "/spec_requirements.rs"));
}

/// Meta-test: every requirement in `specs/design/ironlink.md` has a
/// `#[spec_test(REQ_IL_ironlink_NNN)]` somewhere in src/ or tests/. The build
/// script populates `UNTESTED` from the files it scans.
#[test]
fn all_spec_requirements_have_tests() {
    assert!(
        spec_requirements::UNTESTED.is_empty(),
        "Requirements in spec with no conformance test: {:?}",
        spec_requirements::UNTESTED
    );
}

/// How long a test waits for a process before it fails.
const PATIENCE: Duration = Duration::from_secs(20);

/// A device on a loopback port that answers every request with its reply.
struct Device {
    addr: SocketAddr,
    stop: Arc<AtomicBool>,
    thread: Option<JoinHandle<()>>,
}

impl Device {
    /// `id_byte` is repeated for the sixteen bytes of the device id.
    fn start(id_byte: u8, name: &'static str) -> Self {
        let socket = UdpSocket::bind((Ipv4Addr::LOCALHOST, 0)).unwrap();
        let addr = socket.local_addr().unwrap();
        let stop = Arc::new(AtomicBool::new(false));
        let thread = {
            let stop = Arc::clone(&stop);
            std::thread::spawn(move || answer(&socket, id_byte, name, &stop))
        };
        Device {
            addr,
            stop,
            thread: Some(thread),
        }
    }

    /// The device id of `id_byte`, as the program prints it.
    fn id(id_byte: u8) -> String {
        format!("{id_byte:02x}").repeat(16)
    }
}

impl Drop for Device {
    fn drop(&mut self) {
        self.stop.store(true, Ordering::SeqCst);
        // The device waits for a datagram without a timeout; this one ends it.
        UdpSocket::bind((Ipv4Addr::LOCALHOST, 0))
            .unwrap()
            .send_to(&[0], self.addr)
            .unwrap();
        if let Some(thread) = self.thread.take() {
            thread.join().unwrap();
        }
    }
}

fn answer(socket: &UdpSocket, id_byte: u8, name: &str, stop: &AtomicBool) {
    let port = socket.local_addr().unwrap().port();
    let reply = Reply {
        protocol: 1,
        session_port: port,
        device_id: [id_byte; 16],
        protected: false,
        name,
        model: "IronPLC SoftPLC",
        runtime_version: "0.247.0",
    };
    let mut bytes = vec![0u8; MAX_REPLY_LEN];
    let len = encode_reply(&reply, &mut bytes).unwrap();
    let mut buffer = [0u8; 64];
    while !stop.load(Ordering::SeqCst) {
        // No read timeout: a datagram that arrives as a timeout expires can be lost
        // on Windows. The drop of the device sends one datagram to end the wait.
        let received_from = socket.recv_from(&mut buffer);
        if stop.load(Ordering::SeqCst) {
            return;
        }
        let Ok((received, from)) = received_from else {
            continue;
        };
        if is_request(&buffer[..received]) {
            // The client may be gone already.
            drop(socket.send_to(&bytes[..len], from));
        }
    }
}

/// An address on which nobody answers: a bound socket that never reads.
fn silent_address() -> (UdpSocket, SocketAddr) {
    let socket = UdpSocket::bind((Ipv4Addr::LOCALHOST, 0)).unwrap();
    let addr = socket.local_addr().unwrap();
    (socket, addr)
}

/// A started `ironlink`, killed when dropped.
struct Process(Child);

impl Drop for Process {
    fn drop(&mut self) {
        drop(self.0.kill());
        drop(self.0.wait());
    }
}

/// What a finished process wrote.
struct Finished {
    status: ExitStatus,
    stdout: String,
    stderr: String,
}

impl Process {
    fn spawn(args: &[&str], stdin: Stdio) -> Self {
        let child = Command::new(env!("CARGO_BIN_EXE_ironlink"))
            .args(args)
            .stdin(stdin)
            .stdout(Stdio::piped())
            .stderr(Stdio::piped())
            .spawn()
            .unwrap();
        Process(child)
    }

    /// Waits for the process to exit, up to [`PATIENCE`], and reads what it
    /// wrote to stderr and, if the caller did not take it, to stdout.
    fn finish(mut self) -> Finished {
        let deadline = Instant::now() + PATIENCE;
        let status = loop {
            if let Some(status) = self.0.try_wait().unwrap() {
                break status;
            }
            assert!(Instant::now() < deadline, "ironlink did not exit in time");
            std::thread::sleep(Duration::from_millis(20));
        };
        let mut stdout = String::new();
        if let Some(mut pipe) = self.0.stdout.take() {
            pipe.read_to_string(&mut stdout).unwrap();
        }
        let mut stderr = String::new();
        if let Some(mut pipe) = self.0.stderr.take() {
            pipe.read_to_string(&mut stderr).unwrap();
        }
        Finished {
            status,
            stdout,
            stderr,
        }
    }
}

/// Runs `ironlink` with `args` to its end.
fn run(args: &[&str]) -> Finished {
    Process::spawn(args, Stdio::null()).finish()
}

/// A running `ironlink serve`: lines go in, response lines come out.
struct Shell {
    process: Option<Process>,
    stdin: Option<ChildStdin>,
    lines: Receiver<String>,
}

impl Shell {
    fn start(extra_args: &[&str]) -> Self {
        let mut args = extra_args.to_vec();
        args.push("serve");
        let mut process = Process::spawn(&args, Stdio::piped());
        let stdin = process.0.stdin.take().unwrap();
        let stdout = process.0.stdout.take().unwrap();
        let (sender, lines) = mpsc::channel();
        std::thread::spawn(move || {
            for line in BufReader::new(stdout).lines() {
                if sender.send(line.unwrap()).is_err() {
                    return;
                }
            }
        });
        Shell {
            process: Some(process),
            stdin: Some(stdin),
            lines,
        }
    }

    /// Sends one line and returns the next line of the output.
    fn ask(&mut self, line: &str) -> String {
        let stdin = self.stdin.as_mut().unwrap();
        writeln!(stdin, "{line}").unwrap();
        stdin.flush().unwrap();
        self.lines.recv_timeout(PATIENCE).unwrap()
    }

    /// Closes the input, which ends the shell, and returns how it ended and what
    /// it wrote to stderr.
    fn close(mut self) -> (ExitStatus, String) {
        drop(self.stdin.take());
        let finished = self.process.take().unwrap().finish();
        (finished.status, finished.stderr)
    }
}

fn json_of(line: &str) -> Value {
    serde_json::from_str(line).unwrap()
}

/// `value` with every `seenAt` removed, which differs between two runs.
fn without_times(value: &mut Value) {
    match value {
        Value::Object(map) => {
            map.remove("seenAt");
            map.values_mut().for_each(without_times);
        }
        Value::Array(items) => items.iter_mut().for_each(without_times),
        _ => {}
    }
}

fn identify_args(address: &str) -> Vec<&str> {
    vec![
        "identify",
        address,
        "--timeout-ms",
        "1500",
        "--attempts",
        "1",
    ]
}

#[spec_test(REQ_IL_ironlink_051)]
#[test]
fn identify_when_json_then_prints_the_observation_of_the_shell() {
    let device = Device::start(5, "Press line 3");
    let address = device.addr.to_string();
    let mut args = identify_args(&address);
    args.push("--json");

    let finished = run(&args);

    assert!(finished.status.success(), "{}", finished.stderr);
    let response = json_of(finished.stdout.trim());
    assert_eq!(response["response"], "identify");
    assert_eq!(response["answer"], "device");
    assert_eq!(response["observation"]["name"], "Press line 3");
    assert_eq!(response["observation"]["deviceId"], Device::id(5));
    assert_eq!(response["observation"]["route"]["address"], "127.0.0.1");
    assert_eq!(response["observation"]["route"]["method"], "tcp");
    assert_eq!(response["observation"]["sessionPort"], device.addr.port());
}

#[spec_test(REQ_IL_ironlink_051)]
#[test]
fn identify_when_json_then_it_is_the_response_the_shell_gives_for_the_same_command() {
    let device = Device::start(5, "Press line 3");
    let address = device.addr.to_string();
    let mut args = identify_args(&address);
    args.push("--json");
    let mut from_command_line = json_of(run(&args).stdout.trim());
    let mut shell = Shell::start(&[]);
    let command = json!({
        "command": "identify", "address": address, "timeoutMs": 1500, "attempts": 1
    });

    let mut from_shell = json_of(&shell.ask(&command.to_string()));

    without_times(&mut from_command_line);
    without_times(&mut from_shell);
    assert_eq!(from_command_line, from_shell);
}

#[spec_test(REQ_IL_ironlink_050)]
#[test]
fn identify_when_text_then_one_row_with_the_name_the_id_and_the_route() {
    let device = Device::start(5, "Press line 3");

    let finished = run(&identify_args(&device.addr.to_string()));

    assert!(finished.status.success(), "{}", finished.stderr);
    let row = finished.stdout.trim();
    assert!(
        row.starts_with("Press line 3  IronPLC SoftPLC  runtime 0.247.0  id "),
        "{row}"
    );
    assert!(row.contains(&Device::id(5)), "{row}");
    assert!(row.contains(&device.addr.to_string()), "{row}");
}

#[spec_test(REQ_IL_ironlink_052)]
#[test]
fn identify_when_nothing_answers_then_exit_2_with_l6003_and_no_stdout() {
    let (_socket, address) = silent_address();

    let finished = run(&[
        "identify",
        &address.to_string(),
        "--timeout-ms",
        "300",
        "--attempts",
        "1",
    ]);

    assert_eq!(finished.status.code(), Some(2));
    assert!(finished.stderr.contains("L6003"), "{}", finished.stderr);
    assert!(finished.stdout.is_empty());
}

#[spec_test(REQ_IL_ironlink_052)]
#[test]
fn identify_when_nothing_answers_and_json_then_the_none_answer_and_exit_2() {
    let (_socket, address) = silent_address();

    let finished = run(&[
        "identify",
        &address.to_string(),
        "--timeout-ms",
        "300",
        "--attempts",
        "1",
        "--json",
    ]);

    assert_eq!(finished.status.code(), Some(2));
    assert!(finished.stderr.contains("L6003"), "{}", finished.stderr);
    let response = json_of(finished.stdout.trim());
    assert_eq!(response["answer"], "none");
    assert_eq!(response["address"], address.to_string());
}

#[spec_test(REQ_IL_ironlink_050)]
#[test]
fn browse_when_an_address_is_given_then_text_rows_of_the_tree() {
    let device = Device::start(7, "Lathe");
    let address = device.addr.to_string();

    let finished = run(&[
        "browse",
        "--address",
        &address,
        "--timeout-ms",
        "500",
        "--attempts",
        "1",
    ]);

    assert!(finished.status.success(), "{}", finished.stderr);
    let lines: Vec<&str> = finished.stdout.lines().collect();
    assert_eq!(lines.len(), 3, "{}", finished.stdout);
    assert!(!lines[0].is_empty());
    assert_eq!(lines[1], format!("  address list: {address}"));
    assert!(
        lines[2].starts_with("    Lathe  IronPLC SoftPLC  runtime 0.247.0  id "),
        "{}",
        lines[2]
    );
}

#[spec_test(REQ_IL_ironlink_051)]
#[test]
fn browse_when_json_then_it_is_the_tree_of_the_shell() {
    let device = Device::start(7, "Lathe");
    let address = device.addr.to_string();
    let finished = run(&[
        "browse",
        "--address",
        &address,
        "--timeout-ms",
        "500",
        "--attempts",
        "1",
        "--json",
    ]);
    let mut from_command_line = json_of(finished.stdout.trim());
    let mut shell = Shell::start(&[]);
    let command = json!({
        "command": "browse", "addresses": [address], "timeoutMs": 500, "attempts": 1
    });

    let mut from_shell = json_of(&shell.ask(&command.to_string()));

    assert_eq!(from_command_line["response"], "browse");
    without_times(&mut from_command_line);
    without_times(&mut from_shell);
    assert_eq!(from_command_line, from_shell);
}

#[spec_test(REQ_IL_ironlink_040)]
#[test]
fn serve_when_piped_then_browse_with_an_address_list_is_answered_and_closing_stdin_ends_it() {
    let device = Device::start(9, "Mill");
    let mut shell = Shell::start(&[]);
    let command = json!({
        "command": "browse", "addresses": [device.addr.to_string()],
        "timeoutMs": 500, "attempts": 1
    });

    let response = json_of(&shell.ask(&command.to_string()));
    let (status, _) = shell.close();

    assert_eq!(response["response"], "browse");
    assert_eq!(response["drivers"][0]["kind"], "addressList");
    assert_eq!(
        response["drivers"][0]["devices"][0]["variants"][0]["name"],
        "Mill"
    );
    assert!(status.success());
}

#[spec_test(REQ_IL_ironlink_053)]
#[test]
fn serve_when_verbose_then_stdout_is_only_protocol_lines_and_the_log_is_on_stderr() {
    let mut shell = Shell::start(&["-vvvv"]);

    let version = shell.ask(r#"{"command":"version"}"#);
    let garbage = shell.ask("this is not a command");
    let (status, stderr) = shell.close();

    assert_eq!(json_of(&version)["response"], "version");
    assert_eq!(json_of(&garbage)["response"], "error");
    assert!(status.success());
    assert!(stderr.contains("not a command"), "{stderr}");
}

#[spec_test(REQ_IL_ironlink_054)]
#[test]
fn browse_when_interface_is_ipv6_then_exit_2_with_l6004() {
    let finished = run(&["browse", "--interface", "::1"]);

    assert_eq!(finished.status.code(), Some(2));
    assert!(finished.stderr.contains("L6004"), "{}", finished.stderr);
    assert!(
        finished.stderr.contains("IPv6 has no broadcast"),
        "{}",
        finished.stderr
    );
}

#[spec_test(REQ_IL_ironlink_054)]
#[test]
fn browse_when_interface_is_not_an_address_of_this_pc_then_exit_2_with_l6001() {
    // 203.0.113.0/24 is reserved for documentation (RFC 5737): no PC has it.
    let finished = run(&[
        "browse",
        "--interface",
        "203.0.113.9",
        "--timeout-ms",
        "200",
    ]);

    assert_eq!(finished.status.code(), Some(2));
    assert!(finished.stderr.contains("L6001"), "{}", finished.stderr);
}

#[spec_test(REQ_IL_ironlink_054)]
#[test]
fn identify_when_address_is_not_an_address_then_exit_2_with_l6004() {
    let finished = run(&["identify", "controller"]);

    assert_eq!(finished.status.code(), Some(2));
    assert!(finished.stderr.contains("L6004"), "{}", finished.stderr);
}

#[spec_test(REQ_IL_ironlink_054)]
#[test]
fn browse_when_attempts_is_zero_then_exit_2_with_l6004() {
    let finished = run(&["browse", "--address", "127.0.0.1:9", "--attempts", "0"]);

    assert_eq!(finished.status.code(), Some(2));
    assert!(finished.stderr.contains("L6004"), "{}", finished.stderr);
}

#[spec_test(REQ_IL_ironlink_054)]
#[test]
fn version_when_verbosity_is_out_of_range_then_exit_2_with_l6004() {
    let finished = run(&["-vvvvv", "version"]);

    assert_eq!(finished.status.code(), Some(2));
    assert!(finished.stderr.contains("L6004"), "{}", finished.stderr);
}

#[spec_test(REQ_IL_ironlink_040)]
#[test]
fn version_when_run_then_prints_both_versions() {
    let finished = run(&["version"]);

    assert!(finished.status.success());
    assert_eq!(
        finished.stdout.trim(),
        format!(
            "ironlink version {} (discovery wire version 1)",
            env!("CARGO_PKG_VERSION")
        )
    );
}
