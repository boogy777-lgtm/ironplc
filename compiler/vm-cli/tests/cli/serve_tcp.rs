//! Tests of the `serve --listen` TCP transport: framing, session refusal and reconnect.

use std::io::{self, Read, Write};
use std::net::{TcpListener, TcpStream};
use std::process::Command;
use std::time::Duration;

use assert_cmd::cargo;
use spec_test_macro::spec_test;
use tempfile::TempDir;

use crate::{compiled_bytes, write_compiled_container, COUNTER_PROGRAM};

/// One ADR-0063 frame: 4-byte little-endian length, then the line bytes.
fn tcp_frame(line: &str) -> Vec<u8> {
    let mut bytes = (line.len() as u32).to_le_bytes().to_vec();
    bytes.extend_from_slice(line.as_bytes());
    bytes
}

/// Reads exactly one frame from `stream`, returning the line bytes.
fn read_frame(stream: &mut TcpStream) -> io::Result<String> {
    let mut header = [0u8; 4];
    stream.read_exact(&mut header)?;
    let len = u32::from_le_bytes(header) as usize;
    let mut payload = vec![0u8; len];
    stream.read_exact(&mut payload)?;
    String::from_utf8(payload).map_err(|err| io::Error::new(io::ErrorKind::InvalidData, err))
}

/// Connects a frame client to `addr` with a read timeout, so a hang fails
/// instead of blocking the suite.
fn connect_frame_client(addr: std::net::SocketAddr) -> io::Result<TcpStream> {
    let stream = TcpStream::connect(addr)?;
    stream.set_read_timeout(Some(Duration::from_secs(15)))?;
    Ok(stream)
}

/// The `vCode` of the next frame answer, parsed.
fn next_frame_v_code(stream: &mut TcpStream) -> io::Result<String> {
    let line = read_frame(stream)?;
    let value: serde_json::Value = serde_json::from_str(&line)
        .map_err(|err| io::Error::new(io::ErrorKind::InvalidData, err))?;
    value["vCode"]
        .as_str()
        .map(str::to_string)
        .ok_or_else(|| io::Error::new(io::ErrorKind::InvalidData, "answer carries no vCode"))
}

/// ADR-0063/0065: the TCP transport serves the same session protocol one
/// frame per message — the `identity` handshake, the edit FSM at one session
/// at a time (a second session is refused with one V6014 line), a framing
/// violation answered with V6013 and dropped, and the listener surviving it
/// all for a fresh session. The identity carries the device id of the
/// settings file (REQ-VC-vm-cli-029).
#[spec_test(REQ_VC_vm_cli_029)]
#[test]
fn serve_tcp_when_scripted_then_identity_refusal_framing_drop_and_reconnect(
) -> Result<(), Box<dyn std::error::Error>> {
    let dir = TempDir::new()?;
    let container_path = dir.path().join("counter.iplc");
    write_compiled_container(&container_path, COUNTER_PROGRAM);

    // Reserve the port the way a dialing client would; the server binds it next.
    let probe = TcpListener::bind("127.0.0.1:0")?;
    let addr = probe.local_addr()?;
    drop(probe);

    let mut cmd = Command::new(cargo::cargo_bin!("ironplcvm"));
    cmd.arg("serve")
        .arg("--listen")
        .arg(addr.to_string())
        .arg(&container_path);
    let mut server = cmd.spawn()?;

    // Session 1: the scripted flow, framed — identity first (ADR-0063), then
    // accept → test → assemble (ADR-0064), each answered in one frame.
    let mut client = connect_frame_client(addr)?;
    client.write_all(&tcp_frame(r#"{"command":"identity"}"#))?;
    let identity: serde_json::Value = serde_json::from_str(&read_frame(&mut client)?)?;
    assert_eq!(identity["response"], "identity");
    assert_eq!(identity["protocol"], 1);
    assert_eq!(identity["device"]["name"], "ironplcvm");
    // The TCP path reports the id of the device settings file (REQ-VC-vm-cli-029).
    let settings: serde_json::Value = serde_json::from_str(&std::fs::read_to_string(
        dir.path().join("counter.iplc.device.json"),
    )?)?;
    assert_eq!(identity["device"]["deviceId"], settings["deviceId"]);
    assert_eq!(identity["device"]["deviceId"].as_str().unwrap().len(), 32);
    assert!(identity["device"]["firmwareVersion"]
        .as_str()
        .unwrap()
        .split('.')
        .next()
        .unwrap()
        .parse::<u64>()
        .is_ok());
    assert_eq!(identity["application"]["mode"], "normal");
    assert_eq!(
        identity["application"]["candidate"],
        serde_json::Value::Null
    );
    assert!(identity.get("redundancy").is_none());

    let edit = compiled_bytes(COUNTER_PROGRAM);
    let accept = serde_json::json!({"command": "acceptEdits", "program": edit}).to_string();
    for command in [
        &accept,
        r#"{"command":"testEdits"}"#,
        r#"{"command":"assembleEdits"}"#,
    ] {
        client.write_all(&tcp_frame(command))?;
        let line = read_frame(&mut client)?;
        let value: serde_json::Value = serde_json::from_str(&line)?;
        assert_eq!(value["response"], "ack");
    }

    // A second session while the first holds the device: exactly one V6014
    // refusal line, then the socket closes — and the first session is
    // unaffected (ADR-0065).
    let mut second = connect_frame_client(addr)?;
    let refusal: serde_json::Value = serde_json::from_str(&read_frame(&mut second)?)?;
    assert_eq!(refusal["response"], "error");
    assert_eq!(refusal["vCode"], "V6014");
    assert!(refusal["message"]
        .as_str()
        .unwrap()
        .contains("one engineering session"));
    let mut leftover = Vec::new();
    second.read_to_end(&mut leftover)?;
    assert!(leftover.is_empty());

    client.write_all(&tcp_frame(r#"{"command":"getStatus"}"#))?;
    let status: serde_json::Value = serde_json::from_str(&read_frame(&mut client)?)?;
    assert_eq!(status["response"], "status");
    assert_eq!(status["normal"], 2);

    // Framing garbage on the active session: V6013 on the wire, then the
    // connection drops — the listener survives.
    client.write_all(&[0xff, 0xff, 0xff, 0xff])?;
    assert_eq!(next_frame_v_code(&mut client)?, "V6013");
    let mut leftover = Vec::new();
    client.read_to_end(&mut leftover)?;
    assert!(leftover.is_empty());

    // A reconnect lands a fresh session: the assembled promotion stands in
    // the host (normal generation 2).
    let mut third = connect_frame_client(addr)?;
    third.write_all(&tcp_frame(r#"{"command":"identity"}"#))?;
    let identity: serde_json::Value = serde_json::from_str(&read_frame(&mut third)?)?;
    assert_eq!(identity["response"], "identity");
    assert_eq!(identity["application"]["normal"], 2);

    drop(third);
    server.kill()?;

    Ok(())
}
