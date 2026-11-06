//! The TCP transport of the engineering connection (ADR-0063): the same
//! session as stdio, relayed to the device loop ([`crate::device_loop`]), one
//! message per 4-byte-little-endian-length-prefixed frame instead of one
//! message per newline-delimited line. The frame replaces only the delimiter; the JSON
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
use std::sync::{mpsc, Arc};

use ironplc_runtime::frame::{
    is_framing_error, write_frame, FrameReader, FrameWriter, CLOSE_LINGER, RESPONSE_TIMEOUT,
};
use ironplc_runtime::DEFAULT_SESSION_PORT;

use crate::device_loop::{self, relay_session, DeviceLoop, LoopHandle};
use crate::device_settings::device::Device;
use crate::discovery::{self, Discovery};
use crate::error::{self, VmError};
use crate::pacer::Pacer;
use crate::serve::compose_host;
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

/// The body of a session thread: serves the connection to its end. The thread
/// is generic over it so that a test can end a session in a way the production
/// body does not.
type SessionBody = fn(&LoopHandle, &mut TcpStream) -> io::Result<()>;

/// The claim on the one engineering session (ADR-0065): taking it is the
/// refusal of every other connection, and dropping it frees the device.
///
/// The release is the drop, so it happens on every way a session thread can
/// end: it returns, it fails, or it panics. A flag that the thread had to clear
/// by hand would stay set after a panic and the device would refuse every
/// client with V6014 until it was restarted.
struct SessionClaim(Arc<AtomicBool>);

impl SessionClaim {
    /// Claims the session, or `None` when another connection holds it.
    fn take(occupied: &Arc<AtomicBool>) -> Option<SessionClaim> {
        (!occupied.swap(true, Ordering::SeqCst)).then(|| SessionClaim(Arc::clone(occupied)))
    }
}

impl Drop for SessionClaim {
    fn drop(&mut self) {
        self.0.store(false, Ordering::SeqCst);
    }
}

/// Serves one TCP session: the stdio session over the framed transport,
/// relayed to the device loop, with the per-message read timeout the timeout
/// policy sets. `stream` stays with the caller so a failed session can still
/// receive its V6013 line.
fn serve_connection(handle: &LoopHandle, stream: &mut TcpStream) -> io::Result<()> {
    stream.set_read_timeout(Some(RESPONSE_TIMEOUT))?;
    let reader = FrameReader::new(stream.try_clone()?);
    let writer = FrameWriter::new(stream.try_clone()?);
    relay_session(handle, reader, writer)
}

/// Serves one accepted connection to its end, then releases the claim
/// whatever the outcome — a lost session must never wedge the listener. A
/// session ends nothing but itself: the edit on the device stays as it was,
/// because the body only relays commands to the device loop.
///
/// `claim` is the last parameter so that it is dropped first when the thread
/// unwinds, and a panic frees the device before the socket closes, as the
/// normal end does.
fn serve_accepted(
    handle: LoopHandle,
    mut stream: TcpStream,
    peer: SocketAddr,
    session: SessionBody,
    claim: SessionClaim,
) {
    let result = session(&handle, &mut stream);
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
    // INVARIANT (the other site: `TcpChannel::close` in the session client,
    // `compiler/session-client/src/endpoint/tcp.rs`): the claim is released
    // BEFORE the socket is dropped, here and when a panic unwinds. The end of the connection that a
    // client reads (EOF after its own close) therefore means the slot is free,
    // and a client that closes gracefully and then connects again is never
    // refused with V6014. Nothing between this store and the drop of `stream`
    // may be moved above it. The next client may also connect while a dropped
    // client finishes its close below.
    drop(claim);
    if framing_answered {
        close_gracefully(&mut stream);
    }
    drop(stream);
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
/// one listener, one session at a time, while the device loop scans. The
/// accept loop (its own thread) is the single-session authority (ADR-0065):
/// it stays responsive while a session thread runs, and claims the occupied
/// flag for the accepted socket — a claimed device refuses every further
/// connection with one V6014 line. Connection-level failures — a framing
/// violation, an idle client, a reset peer — never end the listener; only an
/// accept error or a store/boot failure ends `serve` with the usual V-code.
///
/// The device loop runs on the calling thread and owns the host, the HA shell,
/// the device and the slot store; the session threads relay their lines to it
/// (ADR-0086). The shell is composed exactly like the stdio session —
/// standalone by default, the simulated pair under `ha_simulated_peer` — and
/// lives as long as the host: sequential sessions resume the same pair state.
///
/// Ctrl+C is the controlled stop of the process: the loop ends, and the device
/// writes the retained values before `serve` returns.
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
    let (mut host, shell) = compose_host(container, ha_simulated_peer)?;
    let mut device = Device::open(path)?;
    device.boot_application(&mut host);

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

    let stop = crate::cli::stop_on_ctrl_c()?;
    let (handle, commands) = device_loop::channel();
    let (failed, failures) = mpsc::channel::<VmError>();
    let acceptor_stop = Arc::clone(&stop);
    std::thread::Builder::new()
        .name("serve-accept".to_string())
        .spawn(move || {
            if let Err(err) = accept_sessions(&listener, &handle, serve_connection) {
                // The listener is gone: the device stops in order.
                if failed.send(err).is_err() {
                    log::error!("the listener failed after the device had stopped");
                }
                acceptor_stop.store(true, Ordering::Relaxed);
            }
        })
        .map_err(|err| {
            VmError::io(
                error::SESSION_IO,
                format!("unable to spawn the accept thread: {err}"),
            )
        })?;

    let mut device_loop = DeviceLoop::new(
        host,
        Some(store),
        device,
        shell,
        Pacer::start(stop, None),
        commands,
    );
    device_loop.run();
    let outcome = match failures.try_recv() {
        Ok(err) => Err(err),
        Err(_) => Ok(()),
    };
    device_loop.stop(outcome)
}

/// The accept loop: one session thread per accepted connection, the occupied
/// flag as the single-session claim. Returns only with the error that ends
/// the listener.
fn accept_sessions(
    listener: &TcpListener,
    handle: &LoopHandle,
    session: SessionBody,
) -> Result<(), VmError> {
    let occupied = Arc::new(AtomicBool::new(false));
    loop {
        let (stream, peer) = listener.accept().map_err(|err| {
            VmError::io(
                error::SESSION_IO,
                format!("unable to accept a connection: {err}"),
            )
        })?;
        // The claim is the refusal: whoever finds the device occupied gets
        // one coded line and is closed, never a session.
        let Some(claim) = SessionClaim::take(&occupied) else {
            log::info!("refusing {peer}: one engineering session is already active");
            // The close waits for the peer, so it does not hold the accept loop.
            if let Err(err) = std::thread::Builder::new()
                .name(format!("serve-refusal-{peer}"))
                .spawn(move || refuse(stream, peer))
            {
                log::warn!("unable to spawn the refusal of {peer}: {err}");
            }
            continue;
        };
        let session_handle = handle.clone();
        std::thread::Builder::new()
            .name(format!("serve-session-{peer}"))
            .spawn(move || serve_accepted(session_handle, stream, peer, session, claim))
            .map_err(|err| {
                // The closure was dropped with the claim in it: the device is free.
                VmError::io(
                    error::SESSION_IO,
                    format!("unable to spawn a session thread: {err}"),
                )
            })?;
    }
}

#[cfg(test)]
mod session_tests;

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
    fn session_claim_take_when_free_then_claims_and_drop_frees() {
        let occupied = Arc::new(AtomicBool::new(false));

        let claim = SessionClaim::take(&occupied);
        assert!(claim.is_some());
        assert!(occupied.load(Ordering::SeqCst));
        drop(claim);

        assert!(!occupied.load(Ordering::SeqCst));
    }

    #[test]
    fn session_claim_take_when_occupied_then_none_and_the_holder_keeps_the_claim() {
        let occupied = Arc::new(AtomicBool::new(false));
        let holder = SessionClaim::take(&occupied);

        let second = SessionClaim::take(&occupied);

        assert!(second.is_none());
        assert!(occupied.load(Ordering::SeqCst));
        drop(holder);
        assert!(!occupied.load(Ordering::SeqCst));
    }

    /// A connected pair: the device end, its peer address, and the client end.
    fn connected() -> (TcpStream, SocketAddr, TcpStream) {
        let listener = TcpListener::bind("127.0.0.1:0").unwrap();
        let client = TcpStream::connect(listener.local_addr().unwrap()).unwrap();
        let (server, peer) = listener.accept().unwrap();
        (server, peer, client)
    }

    /// A handle to a loop nobody runs: the body of the test never calls it.
    fn unused_handle() -> LoopHandle {
        device_loop::channel().0
    }

    #[spec_test(REQ_VC_vm_cli_055)]
    #[test]
    fn serve_accepted_when_the_session_panics_then_the_claim_is_free_before_the_socket_closes() {
        let (server, peer, mut client) = connected();
        let occupied = Arc::new(AtomicBool::new(false));
        let claim = SessionClaim::take(&occupied).unwrap();
        let session = std::thread::spawn(move || {
            serve_accepted(
                unused_handle(),
                server,
                peer,
                |_, _| panic!("the body of the session failed"),
                claim,
            )
        });

        // The client reads until the device closes the socket.
        let mut sink = Vec::new();
        client.read_to_end(&mut sink).unwrap();

        // At that moment the device must already be free: a client that sees
        // the end of the connection and connects again is not refused.
        assert!(!occupied.load(Ordering::SeqCst));
        assert!(session.join().is_err());
    }

    #[spec_test(REQ_VC_vm_cli_055)]
    #[test]
    fn serve_accepted_when_the_session_ends_then_the_claim_is_free_before_the_socket_closes() {
        let (server, peer, mut client) = connected();
        let occupied = Arc::new(AtomicBool::new(false));
        let claim = SessionClaim::take(&occupied).unwrap();
        let session = std::thread::spawn(move || {
            serve_accepted(unused_handle(), server, peer, |_, _| Ok(()), claim)
        });

        let mut sink = Vec::new();
        client.read_to_end(&mut sink).unwrap();

        assert!(!occupied.load(Ordering::SeqCst));
        assert!(session.join().is_ok());
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
