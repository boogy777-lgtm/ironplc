//! Tests of the answer to discovery: the reply built from a device, the
//! decisions of the responder with an injected clock, and the responder on a
//! real loopback socket.

use std::net::{Ipv4Addr, SocketAddr, UdpSocket};
use std::time::Duration;

use ironplc_discovery::{decode_reply, request, REQUEST_LEN};
use ironplc_runtime::DeviceIdentity;
use rstest::rstest;
use serde_json::json;
use spec_test_macro::spec_test;

use super::rate::PER_SOURCE_INTERVAL;
use super::*;
use crate::device_settings::device::{Device, TEST_DEVICE_ID};
use crate::device_settings::Entries;

/// How long a client waits for a reply before it concludes there is none.
const NO_REPLY_AFTER: Duration = Duration::from_millis(200);

const SESSION_PORT: u16 = 49300;

fn source() -> IpAddr {
    IpAddr::V4(Ipv4Addr::new(192, 0, 2, 1))
}

fn rename(device: &mut Device, name: &str) {
    let entries = Entries::from([("name".to_string(), json!(name))]);
    device.change(&entries).unwrap();
}

fn responder_of(device: &Device) -> Responder {
    Responder::new(device.identity_view(), SESSION_PORT)
}

/// The reply of `responder` to `datagram` at `now`, decoded into the identity
/// the tests compare: `None` when the device stays silent.
fn answer_to(
    responder: &mut Responder,
    datagram: &[u8],
    now: Instant,
) -> Option<(String, u16, u32)> {
    let mut out = [0u8; MAX_REPLY_LEN];
    let len = responder.answer(datagram, source(), now, &mut out)?;
    let reply = decode_reply(&out[..len]).unwrap();
    Some((reply.name.to_string(), reply.session_port, reply.protocol))
}

fn identity_with(device_id: Option<&str>) -> DeviceIdentity {
    DeviceIdentity {
        device_id: device_id.map(str::to_string),
        name: "cell 1".into(),
        model: "model".into(),
        modification: "mod".into(),
        runtime_version: "1.2.3".into(),
    }
}

/// A discovery socket on a free loopback port, answering for `device`; the
/// address of the socket.
fn start_responder(device: &Device) -> SocketAddr {
    let socket = bind(SocketAddr::from((Ipv4Addr::LOCALHOST, 0))).unwrap();
    let addr = socket.local_addr().unwrap();
    spawn(socket, device.identity_view(), SESSION_PORT).unwrap();
    addr
}

/// A client socket that gives up waiting after [`NO_REPLY_AFTER`].
fn client() -> UdpSocket {
    let socket = UdpSocket::bind((Ipv4Addr::LOCALHOST, 0)).unwrap();
    socket.set_read_timeout(Some(NO_REPLY_AFTER)).unwrap();
    socket
}

/// Sends `datagram` to `addr` and returns the reply, or `None` when none
/// arrives in time.
fn ask(client: &UdpSocket, addr: SocketAddr, datagram: &[u8]) -> Option<Vec<u8>> {
    client.send_to(datagram, addr).unwrap();
    let mut buffer = [0u8; 2048];
    let (len, from) = client.recv_from(&mut buffer).ok()?;
    assert_eq!(from, addr);
    Some(buffer[..len].to_vec())
}

fn request_with_byte_changed(index: usize) -> Vec<u8> {
    let mut bytes = request().to_vec();
    bytes[index] ^= 0x01;
    bytes
}

/// REQ-VC-vm-cli-032: the reply carries the identity of the device, the
/// session protocol, the session port and an open device.
#[spec_test(REQ_VC_vm_cli_032)]
#[test]
fn encode_identity_when_device_then_reply_carries_its_identity_protocol_and_port() {
    let device = Device::in_memory();
    let mut out = [0u8; MAX_REPLY_LEN];

    let len = encode_identity(&device.identity(), SESSION_PORT, &mut out).unwrap();

    let reply = decode_reply(&out[..len]).unwrap();
    let identity = device.identity();
    assert_eq!(reply.name, identity.name);
    assert_eq!(reply.model, identity.model);
    assert_eq!(reply.runtime_version, identity.runtime_version);
    assert_eq!(reply.protocol, SESSION_PROTOCOL_VERSION);
    assert_eq!(reply.session_port, SESSION_PORT);
    assert!(!reply.protected);
    assert_eq!(
        reply.device_id.as_bytes(),
        &[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16]
    );
    assert_eq!(identity.device_id.as_deref(), Some(TEST_DEVICE_ID));
}

#[test]
fn encode_identity_when_device_has_no_id_then_no_device_id() {
    let mut out = [0u8; MAX_REPLY_LEN];

    let result = encode_identity(&identity_with(None), SESSION_PORT, &mut out);

    assert_eq!(result, Err(ReplyError::NoDeviceId));
}

#[test]
fn encode_identity_when_device_id_is_not_hex_then_malformed_device_id() {
    let mut out = [0u8; MAX_REPLY_LEN];

    let result = encode_identity(&identity_with(Some("not an id")), SESSION_PORT, &mut out);

    assert_eq!(result, Err(ReplyError::MalformedDeviceId));
}

#[test]
fn encode_identity_when_the_buffer_is_too_small_then_the_encoder_error() {
    let mut out = [0u8; 8];

    let result = encode_identity(&identity_with(Some(TEST_DEVICE_ID)), SESSION_PORT, &mut out);

    assert!(matches!(result, Err(ReplyError::Encode(_))));
}

#[test]
fn answer_when_request_then_the_reply_of_the_device_on_the_session_port() {
    let device = Device::in_memory();
    let mut responder = responder_of(&device);

    let answered = answer_to(&mut responder, &request(), Instant::now());

    assert_eq!(
        answered,
        Some((
            "ironplcvm".to_string(),
            SESSION_PORT,
            SESSION_PROTOCOL_VERSION
        ))
    );
}

/// REQ-VC-vm-cli-032: the identity is read for every request.
#[spec_test(REQ_VC_vm_cli_032)]
#[test]
fn answer_when_device_is_renamed_between_requests_then_the_next_reply_has_the_new_name() {
    let mut device = Device::in_memory();
    let mut responder = responder_of(&device);
    let start = Instant::now();
    answer_to(&mut responder, &request(), start);

    rename(&mut device, "cell 2");
    let answered = answer_to(&mut responder, &request(), start + PER_SOURCE_INTERVAL);

    assert_eq!(
        answered.map(|(name, _, _)| name),
        Some("cell 2".to_string())
    );
}

#[test]
fn answer_when_request_repeats_within_the_interval_then_no_reply() {
    let device = Device::in_memory();
    let mut responder = responder_of(&device);
    let start = Instant::now();
    answer_to(&mut responder, &request(), start);

    let answered = answer_to(
        &mut responder,
        &request(),
        start + Duration::from_millis(10),
    );

    assert_eq!(answered, None);
}

#[test]
fn answer_when_datagram_is_not_a_request_then_no_reply_and_the_rate_is_not_used() {
    let device = Device::in_memory();
    let mut responder = responder_of(&device);
    let start = Instant::now();

    let junk = answer_to(&mut responder, &request_with_byte_changed(5), start);
    let genuine = answer_to(&mut responder, &request(), start);

    assert_eq!(junk, None);
    assert!(genuine.is_some());
}

#[test]
fn answer_when_the_reply_cannot_be_built_then_no_reply_and_the_failure_is_reported() {
    let device = Device::in_memory();
    let mut responder = responder_of(&device);
    let mut out = [0u8; 4];

    let answered = responder.answer(&request(), source(), Instant::now(), &mut out);

    assert_eq!(answered, None);
    assert!(responder.reported_reply_failure);
}

#[test]
fn bind_when_the_udp_port_is_taken_then_v6017_naming_the_address() {
    let taken = UdpSocket::bind((Ipv4Addr::LOCALHOST, 0)).unwrap();
    let addr = taken.local_addr().unwrap();

    let err = bind(addr).unwrap_err();

    assert_eq!(err.v_code(), "V6017");
    assert_eq!(err.exit_code(), 2);
    assert!(err.message().contains(&addr.to_string()), "{err}");
    assert!(err.message().contains("--no-discovery"), "{err}");
}

/// REQ-VC-vm-cli-032: a request on the socket gets one reply that decodes to
/// the identity of the device.
#[spec_test(REQ_VC_vm_cli_032)]
#[test]
fn responder_when_request_arrives_on_a_socket_then_replies_with_the_device_identity() {
    let device = Device::in_memory();
    let addr = start_responder(&device);

    let bytes = ask(&client(), addr, &request()).unwrap();

    let reply = decode_reply(&bytes).unwrap();
    let identity = device.identity();
    assert_eq!(reply.name, identity.name);
    assert_eq!(reply.model, identity.model);
    assert_eq!(reply.runtime_version, identity.runtime_version);
    assert_eq!(reply.session_port, SESSION_PORT);
    assert_eq!(reply.protocol, SESSION_PROTOCOL_VERSION);
    assert_eq!(reply.device_id.as_bytes()[0], 1);
    assert!(!reply.protected);
}

/// REQ-VC-vm-cli-033: anything but the exact request gets no reply, and the
/// responder goes on answering.
#[spec_test(REQ_VC_vm_cli_033)]
#[rstest]
#[case::empty(vec![])]
#[case::seven_bytes(request()[..REQUEST_LEN - 1].to_vec())]
#[case::nine_bytes([request().to_vec(), vec![0]].concat())]
#[case::a_kilobyte(vec![0u8; 1024])]
#[case::changed_magic(request_with_byte_changed(0))]
#[case::changed_version(request_with_byte_changed(4))]
#[case::changed_kind(request_with_byte_changed(5))]
#[case::changed_first_reserved(request_with_byte_changed(6))]
#[case::changed_second_reserved(request_with_byte_changed(7))]
fn responder_when_datagram_is_not_the_request_then_no_reply_and_the_next_request_is_answered(
    #[case] datagram: Vec<u8>,
) {
    let device = Device::in_memory();
    let addr = start_responder(&device);
    let client = client();

    let to_junk = ask(&client, addr, &datagram);
    let to_request = ask(&client, addr, &request());

    assert_eq!(to_junk, None);
    assert!(to_request.is_some());
}
