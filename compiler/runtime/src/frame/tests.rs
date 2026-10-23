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
    assert!(is_framing_error(&err));
    assert_eq!(err.to_string(), "connection closed mid-frame");
}

#[test]
fn frame_reader_when_length_exceeds_cap_then_framing_error_without_payload_read() {
    let mut bytes = (MAX_FRAME as u64 + 1).to_le_bytes()[..4].to_vec();
    bytes.extend_from_slice(&[b'x'; 8]);

    let err = read_lines(&bytes).unwrap_err();

    assert!(is_framing_error(&err));
    assert!(err
        .to_string()
        .starts_with("frame length 16777217 exceeds the 16777216-byte session limit"));
}

#[test]
fn frame_reader_when_payload_is_not_utf8_then_framing_error() {
    let mut bytes = 2u32.to_le_bytes().to_vec();
    bytes.extend_from_slice(&[0xff, 0xfe]);

    let err = read_lines(&bytes).unwrap_err();

    assert!(is_framing_error(&err));
    assert_eq!(err.to_string(), "frame payload is not valid UTF-8");
}

#[test]
fn frame_reader_when_payload_carries_newline_then_framing_error() {
    let payload = "{\"command\":\"getStatus\"}\n{\"command\":\"getStatus\"}";
    let mut bytes = (payload.len() as u32).to_le_bytes().to_vec();
    bytes.extend_from_slice(payload.as_bytes());

    let err = read_lines(&bytes).unwrap_err();

    assert!(is_framing_error(&err));
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

    assert_eq!(output, frame_bytes(r#"{"response":"ack"}"#));
}

#[test]
fn frame_writer_when_flushed_empty_then_no_frame() {
    let mut output = Vec::new();
    let mut writer = FrameWriter::new(&mut output);
    writer.flush().unwrap();

    assert!(output.is_empty());
}

#[test]
fn read_frame_when_one_frame_then_the_line_without_newline() {
    let bytes = frame_bytes(r#"{"response":"ack"}"#);

    let line = read_frame(&mut io::Cursor::new(bytes)).unwrap();

    assert_eq!(line.as_deref(), Some(r#"{"response":"ack"}"#));
}

#[test]
fn read_frame_when_eof_at_frame_boundary_then_none() {
    assert_eq!(read_frame(&mut io::Cursor::new(Vec::new())).unwrap(), None);
}

#[test]
fn read_frame_when_eof_inside_the_header_then_framing_error() {
    let err = read_frame(&mut io::Cursor::new(vec![1u8, 0])).unwrap_err();

    assert!(is_framing_error(&err));
    assert_eq!(err.to_string(), "connection closed mid-frame");
}

#[test]
fn read_frame_when_the_reader_times_out_then_the_error_is_not_framing() {
    struct TimedOut;
    impl Read for TimedOut {
        fn read(&mut self, _buf: &mut [u8]) -> io::Result<usize> {
            Err(io::Error::new(io::ErrorKind::TimedOut, "timed out"))
        }
    }

    let err = read_frame(&mut TimedOut).unwrap_err();

    assert_eq!(err.kind(), io::ErrorKind::TimedOut);
    assert!(!is_framing_error(&err));
}

#[test]
fn write_frame_when_line_then_length_prefix_and_line_bytes() {
    let mut output = Vec::new();

    write_frame(&mut output, r#"{"command":"identity"}"#).unwrap();

    assert_eq!(output, frame_bytes(r#"{"command":"identity"}"#));
}

#[test]
fn write_frame_when_line_carries_newline_then_framing_error_and_nothing_written() {
    let mut output = Vec::new();

    let err = write_frame(&mut output, "a\nb").unwrap_err();

    assert!(is_framing_error(&err));
    assert!(output.is_empty());
}

#[test]
fn write_frame_when_written_then_read_frame_reads_the_same_line() {
    let mut output = Vec::new();
    write_frame(&mut output, r#"{"command":"getStatus"}"#).unwrap();

    let line = read_frame(&mut io::Cursor::new(output)).unwrap();

    assert_eq!(line.as_deref(), Some(r#"{"command":"getStatus"}"#));
}

#[test]
fn is_framing_error_when_other_invalid_data_then_false() {
    let err = io::Error::new(io::ErrorKind::InvalidData, "not a framing marker");

    assert!(!is_framing_error(&err));
}

#[test]
fn response_timeout_is_the_thirty_seconds_of_the_timeout_policy() {
    assert_eq!(RESPONSE_TIMEOUT.as_secs(), 30);
    assert_eq!(CONNECT_TIMEOUT.as_secs(), 5);
}
