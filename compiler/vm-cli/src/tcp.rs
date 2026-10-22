//! The TCP transport of the engineering connection (ADR-0063): the same
//! `serve_session` loop as stdio, one message per
//! 4-byte-little-endian-length-prefixed frame instead of one message per
//! newline-delimited line. The frame replaces only the delimiter; the JSON
//! inside is byte-identical on both transports. The frame and the timeouts are
//! defined once, in `ironplc_runtime::frame`, which a client reads too.
//!
//! One frame is one message: `u32` little-endian length, then exactly that
//! many bytes of one UTF-8 JSON line (no trailing newline — little-endian
//! matches the container wire format). A declared length over `MAX_FRAME`
//! (16 MiB, the container-upload headroom), a payload that is not exactly one
//! line (raw `\n`), non-UTF-8 bytes, or EOF mid-frame is a framing error:
//! the session answers V6013 on the wire and the connection closes, while
//! the listener keeps serving (framing breaks the stream, unlike the
//! codeless per-message codec error, which leaves the session alive).
//!
//! One engineering session is served at a time (ADR-0065): the accept loop
//! stays responsive while a session runs — each session is one thread — and
//! the next accepted socket receives exactly one coded refusal line (V6014)
//! and is closed. The single-writer rule is the occupied flag's claim in the
//! accept loop, so there is no second session to hold a lock or violate
//! exclusivity. The per-message read timeout drops an idle client the same
//! way, with V6013 on the wire.

use std::io::{self, Read};
use std::net::{IpAddr, Shutdown, SocketAddr, TcpListener, TcpStream};
use std::path::Path;
use std::sync::atomic::{AtomicBool, Ordering};
use std::sync::{Arc, Mutex};
use std::time::Duration;

use ironplc_redundancy::Shell;
use ironplc_runtime::frame::{
    is_framing_error, write_frame, FrameReader, FrameWriter, RESPONSE_TIMEOUT,
};
use ironplc_runtime::{RuntimeHost, DEFAULT_SESSION_PORT};

use crate::device_settings::device::Device;
use crate::discovery::{self, Discovery};
use crate::error::{self, VmError};
use crate::serve::{compose_host, serve_session};
use crate::slot_store::SlotStore;

/// The one-line refusal a second connection receives (V6014): it names the
/// single-session policy the attempt ran into (ADR-0065).
const SESSION_REFUSED_MESSAGE: &str = "the device serves exactly one engineering session and it is already active; only one session is served at a time";

/// Whether a session ended with V6013 on the wire: a framing violation or a
/// per-message read timeout. Everything else (a reset peer, a broken pipe)
/// closes the connection silently — there is no peer left to answer.
fn ends_with_framing_code(err: &io::Error) -> bool {
    is_framing_error(err)
        || matches!(
            err.kind(),
            io::ErrorKind::TimedOut | io::ErrorKind::WouldBlock
        )
}

/// The V6013 message for a dropped session: the framing detail, or the idle
/// timeout the client exceeded.
fn framing_code_message(err: &io::Error) -> String {
    if is_framing_error(err) {
        format!("session framing error: {err}")
    } else {
        format!(
            "no message received within {} s",
            RESPONSE_TIMEOUT.as_secs()
        )
    }
}

/// Writes one coded error line as a single frame — the whole answer a
/// refused or dropped connection receives before the socket closes.
fn write_error_frame(stream: &mut TcpStream, v_code: &str, message: &str) -> io::Result<()> {
    let line = serde_json::json!({
        "response": "error",
        "vCode": v_code,
        "message": message,
    })
    .to_string();
    write_frame(stream, &line)
}

/// How long a closed connection waits for its peer to finish sending.
const CLOSE_LINGER: Duration = Duration::from_secs(1);

/// The most input a closing connection reads and discards.
const CLOSE_DRAIN_LIMIT: usize = 1024 * 1024;

/// Closes a connection after its last frame, so that the peer reads that frame.
///
/// Closing a socket with input the device has not read resets the connection,
/// and a reset discards what the peer has received and not yet read: a client
/// that writes its `identity` while the device refuses it would lose the
/// refusal. The close therefore ends the output (a FIN after the frame), reads
/// and discards what the peer sent until it closes too, and only then drops
/// the socket. A peer that neither closes nor stops sending is cut off after
/// [`CLOSE_LINGER`] of silence or [`CLOSE_DRAIN_LIMIT`] bytes.
fn close_gracefully(stream: &mut TcpStream) {
    if let Err(err) = stream.shutdown(Shutdown::Write) {
        log::debug!("unable to end the output of a closing connection: {err}");
        return;
    }
    if let Err(err) = stream.set_read_timeout(Some(CLOSE_LINGER)) {
        log::debug!("unable to bound the close of a connection: {err}");
        return;
    }
    let mut buffer = [0u8; 4096];
    let mut drained = 0;
    while drained < CLOSE_DRAIN_LIMIT {
        match stream.read(&mut buffer) {
            Ok(0) | Err(_) => return,
            Ok(count) => drained += count,
        }
    }
}

/// Answers a connection the device cannot serve with one coded line, then
/// closes it so the peer reads the line.
fn refuse(mut stream: TcpStream, peer: SocketAddr) {
    if let Err(err) =
        write_error_frame(&mut stream, error::SESSION_REFUSED, SESSION_REFUSED_MESSAGE)
    {
        log::warn!("unable to send the session refusal to {peer}: {err}");
        return;
    }
    close_gracefully(&mut stream);
}

/// Serves one TCP session: the stdio session loop over the framed transport,
/// with the per-message read timeout the timeout policy sets. `stream` stays
/// with the caller so a failed session can still receive its V6013 line.
fn serve_connection(
    host: &mut RuntimeHost,
    shell: &mut Shell,
    device: &mut Device,
    store: &mut SlotStore,
    stream: &mut TcpStream,
) -> io::Result<()> {
    stream.set_read_timeout(Some(RESPONSE_TIMEOUT))?;
    let reader = FrameReader::new(stream.try_clone()?);
    let writer = FrameWriter::new(stream.try_clone()?);
    serve_session(host, reader, writer, Some(store), device, Some(shell))
}

/// Serves one accepted connection to its end, then releases the occupied
/// flag whatever the outcome — a lost session must never wedge the listener.
fn serve_accepted(
    occupied: Arc<AtomicBool>,
    host: Arc<Mutex<RuntimeHost>>,
    shell: Arc<Mutex<Shell>>,
    device: Arc<Mutex<Device>>,
    store: Arc<Mutex<SlotStore>>,
    mut stream: TcpStream,
    peer: SocketAddr,
) {
    let result = {
        // The occupied flag admits one session thread at a time, so the
        // locks are never contended; a poisoned lock (a panicked sibling)
        // must not wedge the device either, so it is recovered like the
        // MCP session's mutex (`mcp/src/tools/hot_edit.rs`).
        let mut host = host.lock().unwrap_or_else(|err| err.into_inner());
        let mut shell = shell.lock().unwrap_or_else(|err| err.into_inner());
        let mut store = store.lock().unwrap_or_else(|err| err.into_inner());
        let mut device = device.lock().unwrap_or_else(|err| err.into_inner());
        serve_connection(&mut host, &mut shell, &mut device, &mut store, &mut stream)
    };
    let mut framing_answered = false;
    match result {
        Ok(()) => log::info!("session with {peer} ended"),
        Err(err) if ends_with_framing_code(&err) => {
            log::warn!("dropping session with {peer}: {err}");
            let message = framing_code_message(&err);
            match write_error_frame(&mut stream, error::SESSION_FRAMING, &message) {
                Ok(()) => framing_answered = true,
                Err(err) => log::warn!("unable to send the framing error to {peer}: {err}"),
            }
        }
        Err(err) => log::warn!("session with {peer} ended with an error: {err}"),
    }
    // The next client may connect while the dropped one finishes its close.
    occupied.store(false, Ordering::SeqCst);
    if framing_answered {
        close_gracefully(&mut stream);
    }
}

/// Reads the value of `--listen`: `IP:PORT`, or an IP address (v4 or v6) alone,
/// which listens on [`DEFAULT_SESSION_PORT`]. The port is defined once, in the
/// runtime crate, and read from there. A host name is not accepted: the device
/// listens on an address it is told, and resolving a name would make the bind
/// depend on a name service.
pub(crate) fn parse_listen_address(text: &str) -> Result<SocketAddr, String> {
    if let Ok(addr) = text.parse::<SocketAddr>() {
        return Ok(addr);
    }
    text.parse::<IpAddr>()
        .map(|ip| SocketAddr::new(ip, DEFAULT_SESSION_PORT))
        .map_err(|_| {
            format!(
                "`{text}` is not IP:PORT (for IPv6, [IP]:PORT) or an IP address alone, which uses port {DEFAULT_SESSION_PORT}; a host name is not accepted"
            )
        })
}

/// Boots the committed artifact and serves the hot-edit session on `addr`:
/// one listener, one session at a time. The accept loop is the
/// single-session authority (ADR-0065): it stays responsive while a session
/// thread runs, and claims the occupied flag for the accepted socket — a
/// claimed device refuses every further connection with one V6014 line.
/// Connection-level failures — a framing violation, an idle client, a reset
/// peer — never end the listener; only an accept error or a store/boot
/// failure ends `serve` with the usual V-code.
///
/// The HA shell is composed exactly like the stdio session — standalone by
/// default, the simulated pair under `ha_simulated_peer` — and lives as
/// long as the host: sequential sessions resume the same pair state.
///
/// With `discovery` on, the device also answers discovery on UDP at the
/// address and port the listener actually has (the same number as the session,
/// ADR-0084), bound before the first connection is accepted: V6017 when it
/// cannot be.
pub(crate) fn serve_tcp(
    path: &Path,
    addr: SocketAddr,
    ha_simulated_peer: bool,
    discovery: Discovery,
) -> Result<(), VmError> {
    let store = SlotStore::beside(path);
    let container = store.boot()?;
    let (host, shell) = compose_host(container, ha_simulated_peer)?;
    let device = Device::open(path)?;

    let listener = TcpListener::bind(addr).map_err(|err| {
        VmError::io(
            error::SESSION_IO,
            format!("unable to listen on {addr}: {err}"),
        )
    })?;
    let session_addr = listener.local_addr().map_err(|err| {
        VmError::io(
            error::SESSION_IO,
            format!("unable to read the address {addr} is bound to: {err}"),
        )
    })?;
    log::info!("serving the hot-edit session on {session_addr}");
    if discovery == Discovery::Answer {
        let socket = discovery::bind(session_addr)?;
        discovery::spawn(socket, device.identity_view(), session_addr.port())?;
        log::info!("answering discovery on {session_addr} (UDP)");
    }

    let occupied = Arc::new(AtomicBool::new(false));
    let host = Arc::new(Mutex::new(host));
    let shell = Arc::new(Mutex::new(shell));
    let device = Arc::new(Mutex::new(device));
    let store = Arc::new(Mutex::new(store));
    loop {
        let (stream, peer) = listener.accept().map_err(|err| {
            VmError::io(
                error::SESSION_IO,
                format!("unable to accept a connection: {err}"),
            )
        })?;
        // The claim is the refusal: whoever finds the device occupied gets
        // one coded line and is closed, never a session.
        if occupied.swap(true, Ordering::SeqCst) {
            log::info!("refusing {peer}: one engineering session is already active");
            // The close waits for the peer, so it does not hold the accept loop.
            if let Err(err) = std::thread::Builder::new()
                .name(format!("serve-refusal-{peer}"))
                .spawn(move || refuse(stream, peer))
            {
                log::warn!("unable to spawn the refusal of {peer}: {err}");
            }
            continue;
        }
        let session_occupied = Arc::clone(&occupied);
        let session_host = Arc::clone(&host);
        let session_shell = Arc::clone(&shell);
        let session_device = Arc::clone(&device);
        let session_store = Arc::clone(&store);
        std::thread::Builder::new()
            .name(format!("serve-session-{peer}"))
            .spawn(move || {
                serve_accepted(
                    session_occupied,
                    session_host,
                    session_shell,
                    session_device,
                    session_store,
                    stream,
                    peer,
                );
            })
            .map_err(|err| {
                occupied.store(false, Ordering::SeqCst);
                VmError::io(
                    error::SESSION_IO,
                    format!("unable to spawn a session thread: {err}"),
                )
            })?;
    }
}

#[cfg(test)]
mod tests {
    use std::io::Write;

    use ironplc_runtime::frame::framing_error;
    use rstest::rstest;
    use spec_test_macro::spec_test;

    use super::*;

    #[test]
    fn write_error_frame_then_one_coded_error_line_frame() {
        let listener = TcpListener::bind("127.0.0.1:0").unwrap();
        let addr = listener.local_addr().unwrap();
        let mut stream = TcpStream::connect(addr).unwrap();
        let (mut server, _) = listener.accept().unwrap();

        write_error_frame(&mut server, "V6014", "one session only").unwrap();
        drop(server);
        let mut bytes = Vec::new();
        stream.read_to_end(&mut bytes).unwrap();

        let len = u32::from_le_bytes(bytes[..4].try_into().unwrap()) as usize;
        assert_eq!(len, bytes.len() - 4);
        let text = std::str::from_utf8(&bytes[4..]).unwrap();
        let value: serde_json::Value = serde_json::from_str(text).unwrap();
        assert_eq!(value["response"], "error");
        assert_eq!(value["vCode"], "V6014");
        assert_eq!(value["message"], "one session only");
    }

    #[test]
    fn refuse_when_the_peer_already_sent_input_then_the_peer_still_reads_the_refusal() {
        let listener = TcpListener::bind("127.0.0.1:0").unwrap();
        let mut client = TcpStream::connect(listener.local_addr().unwrap()).unwrap();
        let (server, peer) = listener.accept().unwrap();
        client
            .write_all(b"an identity the device never reads")
            .unwrap();
        // Blocks until the input is in the receive buffer of the device.
        server.peek(&mut [0u8; 1]).unwrap();

        let refusing = std::thread::spawn(move || refuse(server, peer));
        let line = ironplc_runtime::frame::read_frame(&mut client)
            .unwrap()
            .unwrap();
        drop(client);
        refusing.join().unwrap();

        let value: serde_json::Value = serde_json::from_str(&line).unwrap();
        assert_eq!(value["vCode"], "V6014");
    }

    #[test]
    fn ends_with_framing_code_when_timeout_then_true() {
        let timeout = io::Error::new(io::ErrorKind::TimedOut, "timed out");
        let would_block = io::Error::new(io::ErrorKind::WouldBlock, "would block");
        let reset = io::Error::new(io::ErrorKind::ConnectionReset, "reset");

        assert!(ends_with_framing_code(&framing_error(
            "bad length".to_string()
        )));
        assert!(ends_with_framing_code(&timeout));
        assert!(ends_with_framing_code(&would_block));
        assert!(!ends_with_framing_code(&reset));
        assert!(!ends_with_framing_code(&io::Error::new(
            io::ErrorKind::InvalidData,
            "not a framing marker"
        )));
    }

    #[test]
    fn framing_code_message_when_framing_then_detail_when_timeout_then_timeout() {
        let detail = framing_code_message(&framing_error("bad length".to_string()));
        let timeout = framing_code_message(&io::Error::new(io::ErrorKind::TimedOut, "timed out"));

        assert_eq!(detail, "session framing error: bad length");
        assert_eq!(timeout, "no message received within 30 s");
    }

    /// REQ-VC-vm-cli-031: `IP:PORT` is taken as it is.
    #[spec_test(REQ_VC_vm_cli_031)]
    #[rstest]
    #[case::v4("127.0.0.1:7000", "127.0.0.1:7000")]
    #[case::v6("[::1]:7000", "[::1]:7000")]
    #[case::port_zero("0.0.0.0:0", "0.0.0.0:0")]
    fn parse_listen_address_when_ip_and_port_then_that_address(
        #[case] text: &str,
        #[case] expected: SocketAddr,
    ) {
        assert_eq!(parse_listen_address(text), Ok(expected));
    }

    /// REQ-VC-vm-cli-031: an IP address alone listens on the default port.
    #[spec_test(REQ_VC_vm_cli_031)]
    #[rstest]
    #[case::v4("127.0.0.1", IpAddr::from([127, 0, 0, 1]))]
    #[case::v4_any("0.0.0.0", IpAddr::from([0, 0, 0, 0]))]
    #[case::v6("::1", IpAddr::from(std::net::Ipv6Addr::LOCALHOST))]
    #[case::v6_any("::", IpAddr::from(std::net::Ipv6Addr::UNSPECIFIED))]
    fn parse_listen_address_when_ip_alone_then_the_default_session_port(
        #[case] text: &str,
        #[case] ip: IpAddr,
    ) {
        assert_eq!(
            parse_listen_address(text),
            Ok(SocketAddr::new(ip, DEFAULT_SESSION_PORT))
        );
    }

    /// REQ-VC-vm-cli-031: a host name or any other text is refused.
    #[spec_test(REQ_VC_vm_cli_031)]
    #[rstest]
    #[case::host_name("localhost")]
    #[case::host_name_and_port("localhost:7000")]
    #[case::empty("")]
    #[case::port_only(":7000")]
    #[case::port_too_large("127.0.0.1:65536")]
    #[case::bracketed_ip_alone("[::1]")]
    #[case::text("not an address")]
    fn parse_listen_address_when_not_an_address_then_error_naming_the_text(#[case] text: &str) {
        let err = parse_listen_address(text).unwrap_err();

        assert!(err.contains(&format!("`{text}`")), "{err}");
        assert!(err.contains(&DEFAULT_SESSION_PORT.to_string()), "{err}");
    }
}
