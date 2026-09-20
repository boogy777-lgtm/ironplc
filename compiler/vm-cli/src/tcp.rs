//! The TCP transport of the engineering connection (ADR-0063): the same
//! `serve_session` loop as stdio, one message per
//! 4-byte-little-endian-length-prefixed frame instead of one message per
//! newline-delimited line. The frame replaces only the delimiter; the JSON
//! inside is byte-identical on both transports.
//!
//! One frame is one message: `u32` little-endian length, then exactly that
//! many bytes of one UTF-8 JSON line (no trailing newline — little-endian
//! matches the container wire format). A declared length over [`MAX_FRAME`]
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

use std::fmt;
use std::io::{self, BufRead, Read, Write};
use std::mem;
use std::net::{SocketAddr, TcpListener, TcpStream};
use std::path::Path;
use std::sync::atomic::{AtomicBool, Ordering};
use std::sync::{Arc, Mutex};
use std::time::Duration;

use ironplc_runtime::{DeviceIdentity, RuntimeHost};

use crate::error::{self, VmError};
use crate::serve::{device_identity, serve_session, start_host};
use crate::slot_store::SlotStore;

/// The largest frame the transport accepts: 16 MiB, the container-upload
/// headroom (`acceptEdits` bytes ride one message, ADR-0055). A larger
/// declared length is a framing error — the client must be speaking
/// something else, and the session must not allocate on its word.
pub(crate) const MAX_FRAME: usize = 16 * 1024 * 1024;

/// The per-message read timeout: a command's response must arrive within
/// 30 s of its request, so an idle client is dropped rather than holding the
/// one engineering session forever.
const READ_TIMEOUT: Duration = Duration::from_secs(30);

/// The one-line refusal a second connection receives (V6014): it names the
/// single-session policy the attempt ran into (ADR-0065).
const SESSION_REFUSED_MESSAGE: &str = "the device serves exactly one engineering session and it is already active; only one session is served at a time";

/// The payload of the io error a framing violation raises, so the session
/// loop can tell a broken stream (V6013, connection closes) apart from every
/// other I/O failure — and from the codeless codec error, which breaks one
/// message only.
#[derive(Debug)]
struct FramingError(String);

impl fmt::Display for FramingError {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(f, "{}", self.0)
    }
}

impl std::error::Error for FramingError {}

/// Raises `message` as the io error a framing violation surfaces as.
fn framing(message: String) -> io::Error {
    io::Error::new(io::ErrorKind::InvalidData, FramingError(message))
}

/// Whether `err` is a framing violation raised by [`framing`].
fn is_framing(err: &io::Error) -> bool {
    err.get_ref()
        .is_some_and(|inner| inner.is::<FramingError>())
}

/// Whether a session ended with V6013 on the wire: a framing violation or a
/// per-message read timeout. Everything else (a reset peer, a broken pipe)
/// closes the connection silently — there is no peer left to answer.
fn ends_with_framing_code(err: &io::Error) -> bool {
    is_framing(err)
        || matches!(
            err.kind(),
            io::ErrorKind::TimedOut | io::ErrorKind::WouldBlock
        )
}

/// The V6013 message for a dropped session: the framing detail, or the idle
/// timeout the client exceeded.
fn framing_code_message(err: &io::Error) -> String {
    if is_framing(err) {
        format!("session framing error: {err}")
    } else {
        format!("no message received within {} s", READ_TIMEOUT.as_secs())
    }
}

/// Reads the 4-byte frame header; `Ok(None)` is a clean EOF at a frame
/// boundary (the client's orderly hang-up), which ends the session exactly
/// like EOF on stdio. EOF after a partial header is a framing error: the
/// stream broke mid-frame.
fn read_header_or_eof(reader: &mut impl Read) -> io::Result<Option<[u8; 4]>> {
    let mut header = [0u8; 4];
    let mut read = 0;
    while read < header.len() {
        match reader.read(&mut header[read..]) {
            Ok(0) if read == 0 => return Ok(None),
            Ok(0) => return Err(framing("connection closed mid-frame".to_string())),
            Ok(count) => read += count,
            Err(err) if err.kind() == io::ErrorKind::Interrupted => continue,
            Err(err) => return Err(err),
        }
    }
    Ok(Some(header))
}

/// Presents a frame stream as lines: each frame's payload is exactly one JSON
/// line, so serving it through [`BufRead`] with a `\n` sentinel appended
/// makes the stdio session loop (`reader.lines()`) serve TCP byte-for-byte
/// unchanged. Framing violations surface as the io error [`framing`] raises.
struct FrameReader<R> {
    inner: R,
    frame: Vec<u8>,
    pos: usize,
}

impl<R: Read> FrameReader<R> {
    fn new(inner: R) -> Self {
        FrameReader {
            inner,
            frame: Vec::new(),
            pos: 0,
        }
    }

    /// Reads the next frame into the line buffer; `Ok(false)` is a clean EOF
    /// at a frame boundary.
    fn next_frame(&mut self) -> io::Result<bool> {
        let Some(header) = read_header_or_eof(&mut self.inner)? else {
            return Ok(false);
        };
        let len = u32::from_le_bytes(header) as usize;
        if len > MAX_FRAME {
            return Err(framing(format!(
                "frame length {len} exceeds the {MAX_FRAME}-byte session limit"
            )));
        }
        let mut payload = vec![0u8; len];
        self.inner
            .read_exact(&mut payload)
            .map_err(|err| match err.kind() {
                io::ErrorKind::UnexpectedEof => framing("connection closed mid-frame".to_string()),
                // A read timeout and friends propagate as-is: the caller maps
                // them to the same V6013 drop, with the timeout's own message.
                _ => err,
            })?;
        if payload.contains(&b'\n') {
            return Err(framing("frame payload is not one JSON line".to_string()));
        }
        let line = String::from_utf8(payload)
            .map_err(|_| framing("frame payload is not valid UTF-8".to_string()))?;
        self.frame.clear();
        self.frame.extend_from_slice(line.as_bytes());
        self.frame.push(b'\n');
        self.pos = 0;
        Ok(true)
    }
}

impl<R: Read> Read for FrameReader<R> {
    fn read(&mut self, buf: &mut [u8]) -> io::Result<usize> {
        let available = self.fill_buf()?;
        let count = available.len().min(buf.len());
        buf[..count].copy_from_slice(&available[..count]);
        self.consume(count);
        Ok(count)
    }
}

impl<R: Read> BufRead for FrameReader<R> {
    fn fill_buf(&mut self) -> io::Result<&[u8]> {
        while self.pos >= self.frame.len() {
            if !self.next_frame()? {
                return Ok(&[]);
            }
        }
        Ok(&self.frame[self.pos..])
    }

    fn consume(&mut self, amt: usize) {
        self.pos = (self.pos + amt).min(self.frame.len());
    }
}

/// Frames what is written to it: the buffered line becomes one frame —
/// 4-byte little-endian length, then the line bytes without the trailing
/// newline — on [`Write::flush`]. The session loop writes one line and
/// flushes per response, so one flush is exactly one frame.
struct FrameWriter<W> {
    inner: W,
    pending: Vec<u8>,
}

impl<W: Write> FrameWriter<W> {
    fn new(inner: W) -> Self {
        FrameWriter {
            inner,
            pending: Vec::new(),
        }
    }
}

impl<W: Write> Write for FrameWriter<W> {
    fn write(&mut self, buf: &[u8]) -> io::Result<usize> {
        self.pending.extend_from_slice(buf);
        Ok(buf.len())
    }

    fn flush(&mut self) -> io::Result<()> {
        let mut line = mem::take(&mut self.pending);
        if line.is_empty() {
            return self.inner.flush();
        }
        // The session loop delimits lines with writeln!; the frame carries
        // the line bytes only.
        if line.last() == Some(&b'\n') {
            line.pop();
        }
        let len = u32::try_from(line.len())
            .map_err(|_| framing("response line exceeds the frame length field".to_string()))?;
        self.inner.write_all(&len.to_le_bytes())?;
        self.inner.write_all(&line)?;
        self.inner.flush()
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
    let mut writer = FrameWriter::new(&mut *stream);
    writeln!(writer, "{line}")?;
    writer.flush()
}

/// Serves one TCP session: the stdio session loop over the framed transport,
/// with the per-message read timeout the timeout policy sets. `stream` stays
/// with the caller so a failed session can still receive its V6013 line.
fn serve_connection(
    host: &mut RuntimeHost,
    device: &DeviceIdentity,
    store: &mut SlotStore,
    stream: &mut TcpStream,
) -> io::Result<()> {
    stream.set_read_timeout(Some(READ_TIMEOUT))?;
    let reader = FrameReader::new(stream.try_clone()?);
    let writer = FrameWriter::new(stream.try_clone()?);
    serve_session(host, reader, writer, Some(store), device)
}

/// Serves one accepted connection to its end, then releases the occupied
/// flag whatever the outcome — a lost session must never wedge the listener.
fn serve_accepted(
    occupied: Arc<AtomicBool>,
    host: Arc<Mutex<RuntimeHost>>,
    device: DeviceIdentity,
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
        let mut store = store.lock().unwrap_or_else(|err| err.into_inner());
        serve_connection(&mut host, &device, &mut store, &mut stream)
    };
    match result {
        Ok(()) => log::info!("session with {peer} ended"),
        Err(err) if ends_with_framing_code(&err) => {
            log::warn!("dropping session with {peer}: {err}");
            let message = framing_code_message(&err);
            if let Err(err) = write_error_frame(&mut stream, error::SESSION_FRAMING, &message) {
                log::warn!("unable to send the framing error to {peer}: {err}");
            }
        }
        Err(err) => log::warn!("session with {peer} ended with an error: {err}"),
    }
    occupied.store(false, Ordering::SeqCst);
}

/// Boots the committed artifact and serves the hot-edit session on `addr`:
/// one listener, one session at a time. The accept loop is the
/// single-session authority (ADR-0065): it stays responsive while a session
/// thread runs, and claims the occupied flag for the accepted socket — a
/// claimed device refuses every further connection with one V6014 line.
/// Connection-level failures — a framing violation, an idle client, a reset
/// peer — never end the listener; only an accept error or a store/boot
/// failure ends `serve` with the usual V-code.
pub fn serve_tcp(path: &Path, addr: SocketAddr) -> Result<(), VmError> {
    let store = SlotStore::beside(path);
    let container = store.boot()?;
    let host = start_host(container)?;
    let device = device_identity();

    let listener = TcpListener::bind(addr).map_err(|err| {
        VmError::io(
            error::SESSION_IO,
            format!("unable to listen on {addr}: {err}"),
        )
    })?;
    log::info!("serving the hot-edit session on {addr}");

    let occupied = Arc::new(AtomicBool::new(false));
    let host = Arc::new(Mutex::new(host));
    let store = Arc::new(Mutex::new(store));
    loop {
        let (mut stream, peer) = listener.accept().map_err(|err| {
            VmError::io(
                error::SESSION_IO,
                format!("unable to accept a connection: {err}"),
            )
        })?;
        // The claim is the refusal: whoever finds the device occupied gets
        // one coded line and is closed, never a session.
        if occupied.swap(true, Ordering::SeqCst) {
            log::info!("refusing {peer}: one engineering session is already active");
            if let Err(err) =
                write_error_frame(&mut stream, error::SESSION_REFUSED, SESSION_REFUSED_MESSAGE)
            {
                log::warn!("unable to send the session refusal to {peer}: {err}");
            }
            continue;
        }
        let session_occupied = Arc::clone(&occupied);
        let session_host = Arc::clone(&host);
        let session_device = device.clone();
        let session_store = Arc::clone(&store);
        std::thread::Builder::new()
            .name(format!("serve-session-{peer}"))
            .spawn(move || {
                serve_accepted(
                    session_occupied,
                    session_host,
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
    use super::*;

    /// Frames `line` the way a client writes it: length prefix, line bytes.
    fn frame_bytes(line: &str) -> Vec<u8> {
        let mut bytes = (line.len() as u32).to_le_bytes().to_vec();
        bytes.extend_from_slice(line.as_bytes());
        bytes
    }

    /// The lines `FrameReader` assembles from `bytes`, which may split frames
    /// any way a TCP stream can.
    fn read_lines(bytes: &[u8]) -> io::Result<Vec<String>> {
        let mut reader = FrameReader::new(io::Cursor::new(bytes.to_vec()));
        let mut lines = Vec::new();
        let mut line = String::new();
        while reader.read_line(&mut line)? > 0 {
            lines.push(line.clone());
            line.clear();
        }
        Ok(lines)
    }

    /// A reader that yields at most `chunk` bytes per read, like a slow peer.
    struct Chunked {
        bytes: Vec<u8>,
        pos: usize,
        chunk: usize,
    }

    impl Read for Chunked {
        fn read(&mut self, buf: &mut [u8]) -> io::Result<usize> {
            if self.pos >= self.bytes.len() {
                return Ok(0);
            }
            let end = (self.pos + self.chunk).min(self.bytes.len());
            let count = (end - self.pos).min(buf.len());
            buf[..count].copy_from_slice(&self.bytes[self.pos..self.pos + count]);
            self.pos += count;
            Ok(count)
        }
    }

    #[test]
    fn frame_reader_when_one_frame_then_yields_the_line() {
        let lines = read_lines(&frame_bytes(r#"{"command":"getStatus"}"#)).unwrap();

        assert_eq!(lines, vec![r#"{"command":"getStatus"}"#.to_string() + "\n"]);
    }

    #[test]
    fn frame_reader_when_frame_splits_across_reads_then_assembles_the_line() {
        let bytes = frame_bytes(r#"{"command":"acceptEdits","program":[1,2,3]}"#);
        let mut reader = FrameReader::new(Chunked {
            bytes,
            pos: 0,
            chunk: 1,
        });

        let mut line = String::new();
        reader.read_line(&mut line).unwrap();

        assert_eq!(
            line,
            r#"{"command":"acceptEdits","program":[1,2,3]}"#.to_string() + "\n"
        );
    }

    #[test]
    fn frame_reader_when_two_frames_then_yields_two_lines() {
        let mut bytes = frame_bytes(r#"{"command":"getStatus"}"#);
        bytes.extend_from_slice(&frame_bytes(r#"{"command":"identity"}"#));

        let lines = read_lines(&bytes).unwrap();

        assert_eq!(lines.len(), 2);
        assert_eq!(lines[1], r#"{"command":"identity"}"#.to_string() + "\n");
    }

    #[test]
    fn frame_reader_when_eof_at_frame_boundary_then_clean_end() {
        let lines = read_lines(&[]).unwrap();

        assert!(lines.is_empty());
    }

    #[test]
    fn frame_reader_when_eof_mid_frame_then_framing_error() {
        let mut bytes = frame_bytes(r#"{"command":"getStatus"}"#);
        bytes.truncate(bytes.len() - 3);

        let err = read_lines(&bytes).unwrap_err();

        assert_eq!(err.kind(), io::ErrorKind::InvalidData);
        assert!(is_framing(&err));
        assert_eq!(err.to_string(), "connection closed mid-frame");
    }

    #[test]
    fn frame_reader_when_length_exceeds_cap_then_framing_error_without_payload_read() {
        let mut bytes = (MAX_FRAME as u64 + 1).to_le_bytes()[..4].to_vec();
        bytes.extend_from_slice(&[b'x'; 8]);

        let err = read_lines(&bytes).unwrap_err();

        assert!(is_framing(&err));
        assert!(err
            .to_string()
            .starts_with("frame length 16777217 exceeds the 16777216-byte session limit"));
    }

    #[test]
    fn frame_reader_when_payload_is_not_utf8_then_framing_error() {
        let mut bytes = 2u32.to_le_bytes().to_vec();
        bytes.extend_from_slice(&[0xff, 0xfe]);

        let err = read_lines(&bytes).unwrap_err();

        assert!(is_framing(&err));
        assert_eq!(err.to_string(), "frame payload is not valid UTF-8");
    }

    #[test]
    fn frame_reader_when_payload_carries_newline_then_framing_error() {
        let payload = "{\"command\":\"getStatus\"}\n{\"command\":\"getStatus\"}";
        let mut bytes = (payload.len() as u32).to_le_bytes().to_vec();
        bytes.extend_from_slice(payload.as_bytes());

        let err = read_lines(&bytes).unwrap_err();

        assert!(is_framing(&err));
        assert_eq!(err.to_string(), "frame payload is not one JSON line");
    }

    #[test]
    fn frame_writer_when_line_flushed_then_one_frame_without_newline() {
        let mut output = Vec::new();
        {
            let mut writer = FrameWriter::new(&mut output);
            writer.write_all(br#"{"response":"ack"}"#).unwrap();
            writer.write_all(b"\n").unwrap();
            writer.flush().unwrap();
        }

        let expected = r#"{"response":"ack"}"#;
        let mut expected_bytes = (expected.len() as u32).to_le_bytes().to_vec();
        expected_bytes.extend_from_slice(expected.as_bytes());
        assert_eq!(output, expected_bytes);
    }

    #[test]
    fn frame_writer_when_flushed_empty_then_no_frame() {
        let mut output = Vec::new();
        let mut writer = FrameWriter::new(&mut output);
        writer.flush().unwrap();

        assert!(output.is_empty());
    }

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
    fn ends_with_framing_code_when_timeout_then_true() {
        let timeout = io::Error::new(io::ErrorKind::TimedOut, "timed out");
        let would_block = io::Error::new(io::ErrorKind::WouldBlock, "would block");
        let reset = io::Error::new(io::ErrorKind::ConnectionReset, "reset");

        assert!(ends_with_framing_code(&framing("bad length".to_string())));
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
        let detail = framing_code_message(&framing("bad length".to_string()));
        let timeout = framing_code_message(&io::Error::new(io::ErrorKind::TimedOut, "timed out"));

        assert_eq!(detail, "session framing error: bad length");
        assert_eq!(timeout, "no message received within 30 s");
    }
}
