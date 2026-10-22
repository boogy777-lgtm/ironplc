//! Tests of the discovery answer of `ironplcvm serve --listen`: the device
//! answers the request on UDP at the address and port of its TCP session, also
//! while a session is open, not when it is started with `--no-discovery`, and
//! it refuses to start when the UDP port is taken.

use std::io::Write;
use std::net::{SocketAddr, TcpListener, TcpStream, UdpSocket};
use std::path::PathBuf;
use std::process::{Child, Command, ExitStatus, Stdio};
use std::time::{Duration, Instant};

use assert_cmd::cargo;
use ironplc_discovery::{decode_reply, request};
use spec_test_macro::spec_test;
use tempfile::TempDir;

use super::serve_tcp::{connect_frame_client, read_frame, tcp_frame};
use crate::{write_compiled_container, COUNTER_PROGRAM};

/// How long a test waits for the device to start, or to answer.
const PATIENCE: Duration = Duration::from_secs(15);

/// How long a client waits before it concludes that no reply is coming. Longer
/// than the interval of the rate limit, so that a retry after it is allowed.
const ATTEMPT: Duration = Duration::from_millis(300);

/// A running `ironplcvm serve`, killed when the test ends, also when it fails.
struct Server(Child);

impl Drop for Server {
    fn drop(&mut self) {
        drop(self.0.kill());
        drop(self.0.wait());
    }
}

/// A loopback port that is free for TCP and for UDP, and the container to
/// serve.
struct Setup {
    dir: TempDir,
    container: PathBuf,
    addr: SocketAddr,
}

fn setup() -> Setup {
    let dir = TempDir::new().unwrap();
    let container = dir.path().join("counter.iplc");
    write_compiled_container(&container, COUNTER_PROGRAM);
    Setup {
        dir,
        container,
        addr: free_port(),
    }
}

/// A loopback address whose port is free for both protocols, found by binding
/// both and letting go.
fn free_port() -> SocketAddr {
    let tcp = TcpListener::bind("127.0.0.1:0").unwrap();
    let addr = tcp.local_addr().unwrap();
    UdpSocket::bind(addr).unwrap();
    addr
}

fn serve_command(setup: &Setup, extra: &[&str]) -> Command {
    let mut cmd = Command::new(cargo::cargo_bin!("ironplcvm"));
    cmd.arg("serve")
        .arg("--listen")
        .arg(setup.addr.to_string())
        .args(extra)
        .arg(&setup.container)
        .stdin(Stdio::null());
    cmd
}

fn start(setup: &Setup, extra: &[&str]) -> Server {
    Server(serve_command(setup, extra).spawn().unwrap())
}

/// Sends the request until the device answers, and returns the reply. The
/// device may still be starting, and a request inside the interval of the rate
/// limit is not answered, so this asks again after each [`ATTEMPT`].
fn ask_until_answered(addr: SocketAddr) -> Vec<u8> {
    let client = UdpSocket::bind("127.0.0.1:0").unwrap();
    client.set_read_timeout(Some(ATTEMPT)).unwrap();
    let deadline = Instant::now() + PATIENCE;
    let mut buffer = [0u8; 2048];
    loop {
        client.send_to(&request(), addr).unwrap();
        if let Ok((len, _)) = client.recv_from(&mut buffer) {
            return buffer[..len].to_vec();
        }
        assert!(Instant::now() < deadline, "no discovery reply from {addr}");
    }
}

/// Whether the device answers one request within [`ATTEMPT`].
fn answers_once(addr: SocketAddr) -> bool {
    let client = UdpSocket::bind("127.0.0.1:0").unwrap();
    client.set_read_timeout(Some(ATTEMPT)).unwrap();
    client.send_to(&request(), addr).unwrap();
    client.recv_from(&mut [0u8; 2048]).is_ok()
}

/// A session with the device once it accepts connections.
fn connect_when_up(addr: SocketAddr) -> TcpStream {
    let deadline = Instant::now() + PATIENCE;
    loop {
        if let Ok(stream) = connect_frame_client(addr) {
            return stream;
        }
        assert!(Instant::now() < deadline, "{addr} does not accept");
    }
}

/// Sends one command on the session and returns the answer.
fn command(stream: &mut TcpStream, line: &str) -> serde_json::Value {
    stream.write_all(&tcp_frame(line)).unwrap();
    serde_json::from_str(&read_frame(stream).unwrap()).unwrap()
}

/// The exit status of `server`, when it exits within [`PATIENCE`].
fn exit_status(server: &mut Server) -> Option<ExitStatus> {
    let deadline = Instant::now() + PATIENCE;
    while Instant::now() < deadline {
        if let Some(status) = server.0.try_wait().unwrap() {
            return Some(status);
        }
        std::thread::sleep(Duration::from_millis(50));
    }
    None
}

/// REQ-VC-vm-cli-032: the device answers a request on the UDP port of its
/// session with its identity: the id of its settings file, the name, the
/// session port and the session protocol.
#[spec_test(REQ_VC_vm_cli_032)]
#[test]
fn serve_listen_when_request_arrives_on_udp_then_reply_with_the_identity_of_the_device() {
    let setup = setup();
    let _server = start(&setup, &[]);

    let bytes = ask_until_answered(setup.addr);

    let reply = decode_reply(&bytes).unwrap();
    let settings: serde_json::Value = serde_json::from_str(
        &std::fs::read_to_string(setup.dir.path().join("counter.iplc.device.json")).unwrap(),
    )
    .unwrap();
    assert_eq!(settings["deviceId"], reply.device_id.to_string());
    assert_eq!(reply.name, "ironplcvm");
    assert_eq!(reply.session_port, setup.addr.port());
    assert_eq!(reply.protocol, ironplc_runtime::SESSION_PROTOCOL_VERSION);
    assert!(!reply.protected);
}

/// REQ-VC-vm-cli-032: the reply comes while a session is open, and shows a
/// name that the session changed.
#[spec_test(REQ_VC_vm_cli_032)]
#[test]
fn serve_listen_when_a_session_is_open_then_discovery_answers_and_shows_the_renamed_device() {
    let setup = setup();
    let _server = start(&setup, &[]);
    let mut session = connect_when_up(setup.addr);
    command(&mut session, r#"{"command":"identity"}"#);

    let before = ask_until_answered(setup.addr);
    let renamed = command(
        &mut session,
        r#"{"command":"setDeviceSettings","name":"cell 1"}"#,
    );
    let after = ask_until_answered(setup.addr);

    assert_eq!(decode_reply(&before).unwrap().name, "ironplcvm");
    assert_eq!(renamed["response"], "ack");
    assert_eq!(decode_reply(&after).unwrap().name, "cell 1");
}

/// REQ-VC-vm-cli-035: with `--no-discovery` the UDP port is not opened.
#[spec_test(REQ_VC_vm_cli_035)]
#[test]
fn serve_listen_when_no_discovery_then_no_reply_and_the_udp_port_stays_free() {
    let setup = setup();
    let _server = start(&setup, &["--no-discovery"]);
    let mut session = connect_when_up(setup.addr);
    // The identity answer comes from the accept loop, which runs after the
    // point where the device would have bound the UDP port.
    let identity = command(&mut session, r#"{"command":"identity"}"#);

    let answered = answers_once(setup.addr);

    assert_eq!(identity["response"], "identity");
    assert!(!answered);
    assert!(UdpSocket::bind(setup.addr).is_ok());
}

/// REQ-VC-vm-cli-035: the flag belongs to `--listen`.
#[spec_test(REQ_VC_vm_cli_035)]
#[test]
fn serve_when_no_discovery_without_listen_then_usage_error() {
    let dir = TempDir::new().unwrap();
    let container = dir.path().join("counter.iplc");
    write_compiled_container(&container, COUNTER_PROGRAM);

    let output = Command::new(cargo::cargo_bin!("ironplcvm"))
        .args(["serve", "--no-discovery"])
        .arg(&container)
        .stdin(Stdio::null())
        .output()
        .unwrap();

    assert_eq!(output.status.code(), Some(2));
    assert!(String::from_utf8_lossy(&output.stderr).contains("--listen"));
}

/// REQ-VC-vm-cli-031: a host name is a usage error, not a bind.
#[spec_test(REQ_VC_vm_cli_031)]
#[test]
fn serve_when_listen_is_a_host_name_then_usage_error_naming_the_value() {
    let dir = TempDir::new().unwrap();
    let container = dir.path().join("counter.iplc");
    write_compiled_container(&container, COUNTER_PROGRAM);

    let output = Command::new(cargo::cargo_bin!("ironplcvm"))
        .args(["serve", "--listen", "localhost"])
        .arg(&container)
        .stdin(Stdio::null())
        .output()
        .unwrap();

    assert_eq!(output.status.code(), Some(2));
    assert!(String::from_utf8_lossy(&output.stderr).contains("`localhost`"));
}

/// REQ-VC-vm-cli-036: a UDP port that is taken stops the device with V6017.
#[spec_test(REQ_VC_vm_cli_036)]
#[test]
fn serve_listen_when_the_udp_port_is_taken_then_exit_2_with_v6017() {
    let setup = setup();
    let _taken = UdpSocket::bind(setup.addr).unwrap();
    let mut cmd = serve_command(&setup, &[]);
    cmd.stderr(Stdio::piped());
    let mut server = Server(cmd.spawn().unwrap());

    let status = exit_status(&mut server);

    let mut stderr = String::new();
    std::io::Read::read_to_string(&mut server.0.stderr.take().unwrap(), &mut stderr).unwrap();
    assert_eq!(status.and_then(|status| status.code()), Some(2));
    assert!(stderr.contains("V6017"), "{stderr}");
    assert!(stderr.contains("--no-discovery"), "{stderr}");
}

/// REQ-VC-vm-cli-036: with `--no-discovery` the same taken UDP port does not
/// matter.
#[spec_test(REQ_VC_vm_cli_036)]
#[test]
fn serve_listen_when_the_udp_port_is_taken_and_no_discovery_then_the_device_serves() {
    let setup = setup();
    let _taken = UdpSocket::bind(setup.addr).unwrap();
    let _server = start(&setup, &["--no-discovery"]);

    let mut session = connect_when_up(setup.addr);

    assert_eq!(
        command(&mut session, r#"{"command":"identity"}"#)["response"],
        "identity"
    );
}
