//! A session end never touches the edit: the owner's decision that a candidate
//! under Test stays under Test when the session drops, because the side that
//! runs the test owns it, which is the controller.
//!
//! Each test runs a real accept loop and a real device loop on threads,
//! stages and tests an edit through a TCP client, ends the session in one of
//! the three ways a session ends — a clean EOF, an abrupt drop in the middle of
//! a frame, a panic of the session thread — and reads the edit again as a new
//! client. The edit stage, the generations and the mode must be those that the
//! first client last saw.

use std::io::{BufRead, Write};
use std::net::Shutdown;
use std::time::Duration;

use ironplc_runtime::frame::{read_frame, write_frame};
use spec_test_macro::spec_test;

use super::*;
use crate::device_settings::device::Device;
use crate::pacer::Pacer;
use crate::serve::compose_host;
use crate::serve_tests::{compile_container, container_bytes, counter_source};

const STATUS: &str = r#"{"command":"getStatus"}"#;
const TEST: &str = r#"{"command":"testEdits"}"#;

/// The line that makes [`panicking_session`] panic.
const PANIC: &str = r#"{"command":"panic"}"#;

/// A device that serves TCP on a loopback port; the loop stops when it drops.
struct TestDevice {
    address: SocketAddr,
    stop: Arc<AtomicBool>,
}

impl Drop for TestDevice {
    fn drop(&mut self) {
        self.stop.store(true, Ordering::Relaxed);
    }
}

/// Starts a device that runs `counter_source(1)` with the session body
/// `session` on every accepted connection.
fn start(session: SessionBody) -> TestDevice {
    let stop = Arc::new(AtomicBool::new(false));
    let (handle, commands) = device_loop::channel();
    let loop_stop = Arc::clone(&stop);
    std::thread::spawn(move || {
        let (host, shell) = compose_host(compile_container(&counter_source(1)), false).unwrap();
        DeviceLoop::new(
            host,
            None,
            Device::in_memory(),
            shell,
            Pacer::start(loop_stop, None),
            commands,
        )
        .run();
    });
    let listener = TcpListener::bind("127.0.0.1:0").unwrap();
    let address = listener.local_addr().unwrap();
    std::thread::spawn(move || accept_sessions(&listener, &handle, session));
    TestDevice { address, stop }
}

/// A session body that relays commands like the real one and panics when it
/// reads [`PANIC`]. The only way a test can make a session thread panic without
/// a panic path in the production body.
fn panicking_session(handle: &LoopHandle, stream: &mut TcpStream) -> io::Result<()> {
    let reader = FrameReader::new(stream.try_clone()?);
    let mut writer = FrameWriter::new(stream.try_clone()?);
    for line in reader.lines() {
        let line = line?;
        assert_ne!(line, PANIC, "the session thread fails on request");
        writeln!(writer, "{}", handle.call(line)?)?;
        writer.flush()?;
    }
    Ok(())
}

/// Sends one command and reads its answer.
fn ask(stream: &mut TcpStream, line: &str) -> serde_json::Value {
    write_frame(stream, line).unwrap();
    serde_json::from_str(&read_frame(stream).unwrap().unwrap()).unwrap()
}

/// Accepts a candidate with the same layout and tests it; returns the status.
fn stage_and_test(stream: &mut TcpStream) -> serde_json::Value {
    let accept = serde_json::json!({"command": "acceptEdits", "program": container_bytes(&counter_source(10))});
    assert_eq!(ask(stream, &accept.to_string())["response"], "ack");
    assert_eq!(ask(stream, TEST)["response"], "ack");
    ask(stream, STATUS)
}

/// The status as a new client reads it once the device serves a new session:
/// a client that finds the device occupied by the session that is ending is
/// refused with V6014 and tries again.
fn status_of_the_next_session(device: &TestDevice) -> serde_json::Value {
    for _ in 0..500 {
        let mut stream = TcpStream::connect(device.address).unwrap();
        let answer = ask(&mut stream, STATUS);
        if answer["vCode"] != "V6014" {
            return answer;
        }
        std::thread::sleep(Duration::from_millis(10));
    }
    panic!("the device stayed occupied after the session ended");
}

/// What a session end must not change.
fn the_edit(status: &serde_json::Value) -> serde_json::Value {
    serde_json::json!({
        "mode": status["mode"],
        "active": status["active"],
        "normal": status["normal"],
        "candidate": status["candidate"],
        "application": status["application"],
        "edit": status["edit"],
    })
}

#[spec_test(REQ_VC_vm_cli_055)]
#[test]
fn session_end_when_the_client_closes_cleanly_then_the_edit_is_still_under_test() {
    let device = start(serve_connection);
    let mut client = TcpStream::connect(device.address).unwrap();
    let before = stage_and_test(&mut client);
    assert_eq!(before["edit"]["stage"], "testing");

    client.shutdown(Shutdown::Both).unwrap();
    drop(client);
    let after = status_of_the_next_session(&device);

    assert_eq!(the_edit(&after), the_edit(&before));
}

#[spec_test(REQ_VC_vm_cli_055)]
#[test]
fn session_end_when_the_client_drops_in_the_middle_of_a_frame_then_the_edit_is_still_under_test() {
    let device = start(serve_connection);
    let mut client = TcpStream::connect(device.address).unwrap();
    let before = stage_and_test(&mut client);
    assert_eq!(before["edit"]["stage"], "testing");

    // A frame that promises 64 bytes and delivers 15, then the socket is gone.
    client.write_all(&64u32.to_le_bytes()).unwrap();
    client.write_all(&STATUS.as_bytes()[..15]).unwrap();
    drop(client);
    let after = status_of_the_next_session(&device);

    assert_eq!(the_edit(&after), the_edit(&before));
}

#[spec_test(REQ_VC_vm_cli_055)]
#[test]
fn session_end_when_the_session_thread_panics_then_the_edit_is_still_under_test() {
    let device = start(panicking_session);
    let mut client = TcpStream::connect(device.address).unwrap();
    let before = stage_and_test(&mut client);
    assert_eq!(before["edit"]["stage"], "testing");

    write_frame(&mut client, PANIC).unwrap();
    let after = status_of_the_next_session(&device);

    // The device serves a new session at all: the flag was released by the
    // drop of the claim, not by the thread that panicked.
    assert_eq!(the_edit(&after), the_edit(&before));
}
