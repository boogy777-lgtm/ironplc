//! The frame of the TCP transport (ADR-0063) and the timeouts of a framed
//! session: the wire rules a device and a client both follow, defined once.
//!
//! One frame is one message: a `u32` length in little-endian byte order, then
//! exactly that many bytes of one UTF-8 JSON line without its trailing newline
//! (little-endian matches the container wire format). The frame replaces only
//! the newline of the stdio transport; the JSON inside is byte-identical on
//! both. A declared length over [`MAX_FRAME`], a payload that is not exactly one
//! line, a payload that is not UTF-8 and EOF in the middle of a frame are
//! framing errors ([`framing_error`]): the stream is broken and the connection
//! closes, unlike the codeless codec error, which breaks one message only.
//!
//! [`read_frame`] and [`write_frame`] are the primitives. A device reads and
//! writes through [`FrameReader`] and [`FrameWriter`], which present the frames
//! as lines so that the session loop of the stdio transport serves TCP
//! unchanged; a client calls the primitives.

use std::fmt;
use std::io::{self, BufRead, Read, Write};
use std::mem;
use std::time::Duration;

/// The largest frame the transport accepts: 16 MiB, the container-upload
/// headroom (`acceptEdits` bytes ride one message, ADR-0055). A larger declared
/// length is a framing error: the peer must be speaking something else, and the
/// reader must not allocate on its word.
pub const MAX_FRAME: usize = 16 * 1024 * 1024;

/// The connect timeout: opening the transport and the `identity` round trip
/// must finish within it (the timeout policy of the engineering connection).
pub const CONNECT_TIMEOUT: Duration = Duration::from_secs(5);

/// The response timeout: the longest wait for the next message on a framed
/// connection. A client gives up on a command after it, and a device drops a
/// client that sends nothing for as long, so an idle client does not hold the
/// one engineering session forever.
pub const RESPONSE_TIMEOUT: Duration = Duration::from_secs(30);

/// The payload of the io error a framing violation raises, so a session loop
/// can tell a broken stream (the connection closes) apart from every other I/O
/// failure.
#[derive(Debug)]
struct FramingError(String);

impl fmt::Display for FramingError {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(f, "{}", self.0)
    }
}

impl std::error::Error for FramingError {}

/// Raises `message` as the io error a framing violation surfaces as.
pub fn framing_error(message: String) -> io::Error {
    io::Error::new(io::ErrorKind::InvalidData, FramingError(message))
}

/// Whether `err` is a framing violation raised by [`framing_error`].
pub fn is_framing_error(err: &io::Error) -> bool {
    err.get_ref()
        .is_some_and(|inner| inner.is::<FramingError>())
}

/// Reads the 4-byte frame header; `Ok(None)` is a clean EOF at a frame
/// boundary (the peer's orderly hang-up). EOF after a partial header is a
/// framing error: the stream broke mid-frame.
fn read_header_or_eof(reader: &mut impl Read) -> io::Result<Option<[u8; 4]>> {
    let mut header = [0u8; 4];
    let mut read = 0;
    while read < header.len() {
        match reader.read(&mut header[read..]) {
            Ok(0) if read == 0 => return Ok(None),
            Ok(0) => return Err(framing_error("connection closed mid-frame".to_string())),
            Ok(count) => read += count,
            Err(err) if err.kind() == io::ErrorKind::Interrupted => continue,
            Err(err) => return Err(err),
        }
    }
    Ok(Some(header))
}

/// Reads the next frame and returns its line without a newline; `Ok(None)` is a
/// clean EOF at a frame boundary. A framing violation is the io error
/// [`framing_error`] raises. A read timeout and friends propagate as they are,
/// so that the caller can name the timeout.
pub fn read_frame(reader: &mut impl Read) -> io::Result<Option<String>> {
    let Some(header) = read_header_or_eof(reader)? else {
        return Ok(None);
    };
    let len = u32::from_le_bytes(header) as usize;
    if len > MAX_FRAME {
        return Err(framing_error(format!(
            "frame length {len} exceeds the {MAX_FRAME}-byte session limit"
        )));
    }
    let mut payload = vec![0u8; len];
    reader
        .read_exact(&mut payload)
        .map_err(|err| match err.kind() {
            io::ErrorKind::UnexpectedEof => {
                framing_error("connection closed mid-frame".to_string())
            }
            _ => err,
        })?;
    if payload.contains(&b'\n') {
        return Err(framing_error(
            "frame payload is not one JSON line".to_string(),
        ));
    }
    String::from_utf8(payload)
        .map(Some)
        .map_err(|_| framing_error("frame payload is not valid UTF-8".to_string()))
}

/// Writes `line` (one JSON line, without a newline) as one frame and flushes.
///
/// The header and the payload leave in one write, so a small message is one
/// segment and the peer's delayed acknowledgement cannot hold the second half
/// back.
pub fn write_frame(writer: &mut impl Write, line: &str) -> io::Result<()> {
    if line.contains('\n') {
        return Err(framing_error(
            "frame payload is not one JSON line".to_string(),
        ));
    }
    let len = u32::try_from(line.len())
        .map_err(|_| framing_error("response line exceeds the frame length field".to_string()))?;
    let mut frame = Vec::with_capacity(4 + line.len());
    frame.extend_from_slice(&len.to_le_bytes());
    frame.extend_from_slice(line.as_bytes());
    writer.write_all(&frame)?;
    writer.flush()
}

/// Presents a frame stream as lines: each frame's payload is exactly one JSON
/// line, so serving it through [`BufRead`] with a `\n` sentinel appended makes
/// the stdio session loop (`reader.lines()`) serve TCP byte-for-byte unchanged.
/// Framing violations surface as the io error [`framing_error`] raises.
pub struct FrameReader<R> {
    inner: R,
    frame: Vec<u8>,
    pos: usize,
}

impl<R: Read> FrameReader<R> {
    /// Reads frames from `inner`.
    pub fn new(inner: R) -> Self {
        FrameReader {
            inner,
            frame: Vec::new(),
            pos: 0,
        }
    }

    /// Reads the next frame into the line buffer; `Ok(false)` is a clean EOF at
    /// a frame boundary.
    fn next_frame(&mut self) -> io::Result<bool> {
        let Some(line) = read_frame(&mut self.inner)? else {
            return Ok(false);
        };
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

/// Frames what is written to it: the buffered line becomes one frame on
/// [`Write::flush`]. The session loop writes one line and flushes per response,
/// so one flush is exactly one frame.
pub struct FrameWriter<W> {
    inner: W,
    pending: Vec<u8>,
}

impl<W: Write> FrameWriter<W> {
    /// Writes frames to `inner`.
    pub fn new(inner: W) -> Self {
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
        // The session loop delimits lines with writeln!; the frame carries the
        // line bytes only.
        if line.last() == Some(&b'\n') {
            line.pop();
        }
        let line = String::from_utf8(line)
            .map_err(|_| framing_error("frame payload is not valid UTF-8".to_string()))?;
        write_frame(&mut self.inner, &line)
    }
}

#[cfg(test)]
mod tests;
