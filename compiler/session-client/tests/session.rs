//! The session client against a real `ironplcvm serve`, over both connection
//! methods, and against a socket that says nothing.

// Test-target boundary: the workspace denies panicking constructs in production
// code; tests assert by panicking, so the helpers are exempt here.
#![expect(
    clippy::unwrap_used,
    clippy::panic,
    reason = "integration test target: panicking helpers are sanctioned in tests"
)]

use std::net::TcpListener;
use std::path::{Path, PathBuf};
use std::sync::atomic::{AtomicBool, Ordering};
use std::sync::Arc;
use std::time::{Duration, Instant};

use ironplc_container::test_support::{container_bytes, steel_thread_debug_builder};
use ironplc_runtime::frame::{read_frame, write_frame, CLOSE_LINGER};
use ironplc_runtime::{Command, Response, SESSION_PROTOCOL_VERSION};
use ironplc_session_client::{
    Endpoint, Phase, Session, SessionError, StdioEndpoint, TcpEndpoint, Timeouts,
};
use ironplc_test::device::{workspace_binary, SoftDevice};
use rstest::rstest;
use spec_test_macro::spec_test;
use tempfile::TempDir;

/// Spec-conformance requirements generated from `specs/design/session-client.md`.
mod spec_requirements {
    include!(concat!(env!("OUT_DIR"), "/spec_requirements.rs"));
}

/// The compiled container every device in these tests serves.
fn container() -> Vec<u8> {
    container_bytes(&steel_thread_debug_builder().build())
}

/// Writes the container to a file of `dir` and returns its path.
fn container_file(dir: &TempDir) -> PathBuf {
    let path = dir.path().join("main.iplc");
    std::fs::write(&path, container()).unwrap();
    path
}

fn stdio_endpoint(program: &Path) -> Endpoint {
    Endpoint::Stdio(StdioEndpoint::serving(
        workspace_binary("ironplcvm"),
        program.to_path_buf(),
    ))
}

fn tcp_endpoint(device: &SoftDevice) -> Endpoint {
    Endpoint::Tcp(TcpEndpoint {
        address: device.address().ip().to_string(),
        port: device.address().port(),
    })
}

/// A device served over each connection method, kept alive by the guard.
struct Served {
    endpoint: Endpoint,
    _keep: (TempDir, Option<SoftDevice>),
}

fn serve_by_stdio() -> Served {
    let dir = TempDir::new().unwrap();
    let program = container_file(&dir);
    Served {
        endpoint: stdio_endpoint(&program),
        _keep: (dir, None),
    }
}

fn serve_by_tcp() -> Served {
    let dir = TempDir::new().unwrap();
    let device = SoftDevice::start(&container_file(&dir));
    Served {
        endpoint: tcp_endpoint(&device),
        _keep: (dir, Some(device)),
    }
}

/// Each member of the class of connection methods is served here. A method
/// that is added to the table without a row in this function fails the test
/// below it.
fn serve(method: &str) -> Served {
    match method {
        "stdio" => serve_by_stdio(),
        "tcp" => serve_by_tcp(),
        other => panic!("no way to serve the connection method {other} in this test"),
    }
}

#[spec_test(REQ_SC_session_client_001)]
#[test]
fn every_connection_method_when_declared_then_the_tests_serve_it() {
    let served: Vec<&'static str> = Endpoint::METHODS
        .iter()
        .map(|method| serve(method).endpoint.method())
        .collect();

    assert_eq!(served, Endpoint::METHODS);
}

#[spec_test(REQ_SC_session_client_002)]
#[spec_test(REQ_SC_session_client_003)]
#[spec_test(REQ_SC_session_client_004)]
#[rstest]
#[case::stdio("stdio")]
#[case::tcp("tcp")]
fn session_when_connected_by_a_method_then_identity_first_and_the_edit_commands_answer(
    #[case] method: &str,
) {
    let served = serve(method);

    let mut session = Session::connect(&served.endpoint).unwrap();

    let identity = session.identity().unwrap();
    assert_eq!(identity.protocol, SESSION_PROTOCOL_VERSION);
    assert_eq!(identity.device.name, "ironplcvm");
    assert_eq!(
        identity.device.device_id.as_ref().map(String::len),
        Some(32)
    );
    let accept = Command::AcceptEdits {
        program: container(),
        edit: None,
        migration: Default::default(),
    };
    assert_eq!(session.send(&accept).unwrap(), Response::Ack);
    let status = session.send(&Command::GetStatus).unwrap();
    assert!(matches!(status, Response::Status(status) if status.candidate.is_some()));
}

#[spec_test(REQ_SC_session_client_007)]
#[rstest]
#[case::stdio("stdio")]
#[case::tcp("tcp")]
fn session_when_the_device_refuses_a_command_then_its_code_and_the_session_goes_on(
    #[case] method: &str,
) {
    let served = serve(method);
    let mut session = Session::connect(&served.endpoint).unwrap();

    let refused = session.send(&Command::TestEdits);
    let status = session.send(&Command::GetStatus);

    assert_eq!(refused.unwrap_err().v_code(), Some("V4012"));
    assert!(matches!(status, Ok(Response::Status(_))));
}

#[spec_test(REQ_SC_session_client_008)]
#[test]
fn session_when_a_second_tcp_session_connects_then_v6014_and_the_first_goes_on() {
    let served = serve_by_tcp();
    let mut first = Session::connect(&served.endpoint).unwrap();

    let second = Session::connect(&served.endpoint);

    assert_eq!(second.err().unwrap().v_code(), Some("V6014"));
    assert!(matches!(
        first.send(&Command::GetStatus),
        Ok(Response::Status(_))
    ));
}

#[spec_test(REQ_SC_session_client_010)]
#[test]
fn session_when_the_tcp_device_stops_then_closed() {
    let dir = TempDir::new().unwrap();
    let mut device = SoftDevice::start(&container_file(&dir));
    let mut session = Session::connect(&tcp_endpoint(&device)).unwrap();
    device.stop();

    let result = session.send(&Command::GetStatus);

    assert!(matches!(result, Err(SessionError::Closed { .. })));
}

#[spec_test(REQ_SC_session_client_010)]
#[test]
fn session_when_the_stdio_program_exits_at_start_then_closed_with_its_error_output() {
    let endpoint = stdio_endpoint(Path::new("no-such-container.iplc"));

    let result = Session::connect(&endpoint);

    assert!(matches!(
        result,
        Err(SessionError::Closed { detail: Some(detail) }) if detail.contains("V6001")
    ));
}

#[spec_test(REQ_SC_session_client_012)]
#[test]
fn session_when_the_stdio_program_does_not_exist_then_open_error() {
    let endpoint = Endpoint::Stdio(StdioEndpoint::serving(
        PathBuf::from("no-such-ironplcvm"),
        PathBuf::from("main.iplc"),
    ));

    let result = Session::connect(&endpoint);

    assert!(matches!(
        result,
        Err(SessionError::Open {
            method: "stdio",
            ..
        })
    ));
}

#[spec_test(REQ_SC_session_client_012)]
#[test]
fn session_when_nothing_listens_then_open_error() {
    let port = {
        let listener = TcpListener::bind("127.0.0.1:0").unwrap();
        listener.local_addr().unwrap().port()
    };
    let endpoint = Endpoint::Tcp(TcpEndpoint {
        address: "127.0.0.1".to_string(),
        port,
    });

    let result = Session::connect(&endpoint);

    assert!(matches!(
        result,
        Err(SessionError::Open { method: "tcp", .. })
    ));
}

#[spec_test(REQ_SC_session_client_009)]
#[test]
fn session_when_a_socket_accepts_and_says_nothing_then_timed_out_in_the_identity_phase() {
    let listener = TcpListener::bind("127.0.0.1:0").unwrap();
    let endpoint = Endpoint::Tcp(TcpEndpoint {
        address: "127.0.0.1".to_string(),
        port: listener.local_addr().unwrap().port(),
    });
    let timeouts = Timeouts {
        connect: Duration::from_millis(200),
        response: Duration::from_millis(200),
    };

    let result = Session::connect_with(&endpoint, timeouts);

    assert!(matches!(
        result,
        Err(SessionError::TimedOut { phase: Phase::Identity, limit })
            if limit == timeouts.connect
    ));
}

/// How many sessions a device is opened and closed against, back to back, for
/// each connection method. A process per session makes `stdio` slow, so it
/// gets fewer; one lost race in a hundred would show in fifty.
#[spec_test(REQ_SC_session_client_013)]
#[rstest]
#[case::stdio("stdio", 5)]
#[case::tcp("tcp", 50)]
fn session_when_opened_and_closed_back_to_back_then_every_one_succeeds(
    #[case] method: &str,
    #[case] count: usize,
) {
    let served = serve(method);

    for round in 0..count {
        let mut session = Session::connect(&served.endpoint)
            .unwrap_or_else(|err| panic!("session {round} was not opened: {err:?}"));
        assert!(matches!(
            session.send(&Command::GetStatus),
            Ok(Response::Status(_))
        ));
    }
}

#[spec_test(REQ_SC_session_client_013)]
#[test]
fn session_when_closed_then_the_device_has_released_it_for_the_next_connect() {
    let served = serve_by_tcp();

    drop(Session::connect(&served.endpoint).unwrap());
    let next = Session::connect(&served.endpoint);

    assert!(next.is_ok(), "{:?}", next.err());
}

#[spec_test(REQ_SC_session_client_013)]
#[test]
fn session_when_the_device_never_closes_then_the_close_ends_after_the_linger() {
    let listener = TcpListener::bind("127.0.0.1:0").unwrap();
    let endpoint = Endpoint::Tcp(TcpEndpoint {
        address: "127.0.0.1".to_string(),
        port: listener.local_addr().unwrap().port(),
    });
    // A device that answers `identity` and then holds the connection open.
    let device = std::thread::spawn(move || {
        let (mut stream, _) = listener.accept().unwrap();
        read_frame(&mut stream).unwrap();
        write_frame(&mut stream, &identity_answer()).unwrap();
        // Waits for the client to end its output, and keeps its own open.
        assert_eq!(read_frame(&mut stream).unwrap(), None);
        stream
    });
    let session = Session::connect(&endpoint).unwrap();

    let started = Instant::now();
    drop(session);
    let waited = started.elapsed();

    let _held_open = device.join().unwrap();
    assert!(waited >= CLOSE_LINGER / 2, "returned after {waited:?}");
    assert!(
        waited < CLOSE_LINGER + Duration::from_secs(4),
        "returned after {waited:?}"
    );
}

#[spec_test(REQ_SC_session_client_013)]
#[test]
fn session_when_closed_then_it_returns_only_after_the_device_has_released_the_session() {
    let listener = TcpListener::bind("127.0.0.1:0").unwrap();
    let endpoint = Endpoint::Tcp(TcpEndpoint {
        address: "127.0.0.1".to_string(),
        port: listener.local_addr().unwrap().port(),
    });
    let released = Arc::new(AtomicBool::new(false));
    // A device that takes its time between seeing the end of the session and
    // releasing it, as a loaded one does, and closes its side only then.
    let device = {
        let released = Arc::clone(&released);
        std::thread::spawn(move || {
            let (mut stream, _) = listener.accept().unwrap();
            read_frame(&mut stream).unwrap();
            write_frame(&mut stream, &identity_answer()).unwrap();
            assert_eq!(read_frame(&mut stream).unwrap(), None);
            std::thread::sleep(Duration::from_millis(200));
            released.store(true, Ordering::SeqCst);
        })
    };
    let session = Session::connect(&endpoint).unwrap();

    drop(session);

    assert!(released.load(Ordering::SeqCst));
    device.join().unwrap();
}

/// The answer of a device to `identity`.
fn identity_answer() -> String {
    format!(
        r#"{{"response":"identity","protocol":{SESSION_PROTOCOL_VERSION},"device":{{"deviceId":"0123456789abcdef0123456789abcdef","name":"ironplcvm","model":"IronPLC SoftPLC","modification":"vm-cli","runtimeVersion":"0.247.0"}},"application":{{"mode":"normal","active":1,"normal":1,"candidate":null,"application":1,"migration":false,"rounds":0}}}}"#
    )
}
