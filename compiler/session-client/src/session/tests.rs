//! The session against a scripted channel: the handshake and the mapping of
//! what comes back, with no process and no socket.

use std::collections::VecDeque;
use std::sync::{Arc, Mutex};

use ironplc_runtime::SESSION_PROTOCOL_VERSION;
use spec_test_macro::spec_test;

use super::*;

/// A channel that answers from a script and records what was sent.
struct Scripted {
    sent: Arc<Mutex<Vec<String>>>,
    send_failure: Option<ChannelError>,
    answers: VecDeque<Result<String, ChannelError>>,
}

impl Channel for Scripted {
    fn send(&mut self, line: &str) -> Result<(), ChannelError> {
        self.sent
            .lock()
            .unwrap_or_else(|err| err.into_inner())
            .push(line.to_string());
        self.send_failure.take().map_or(Ok(()), Err)
    }

    fn receive(&mut self, _timeout: Duration) -> Result<String, ChannelError> {
        self.answers
            .pop_front()
            .unwrap_or(Err(ChannelError::Closed(None)))
    }
}

/// A scripted channel and the list of lines it will have been sent.
fn scripted(
    answers: Vec<Result<String, ChannelError>>,
) -> (Box<dyn Channel>, Arc<Mutex<Vec<String>>>) {
    let sent = Arc::new(Mutex::new(Vec::new()));
    let channel = Scripted {
        sent: Arc::clone(&sent),
        send_failure: None,
        answers: answers.into(),
    };
    (Box::new(channel), sent)
}

fn identity_line(protocol: u32) -> String {
    format!(
        r#"{{"response":"identity","protocol":{protocol},"device":{{"deviceId":"0123456789abcdef0123456789abcdef","name":"ironplcvm","model":"IronPLC SoftPLC","modification":"vm-cli","runtimeVersion":"0.247.0"}},"application":{{"mode":"normal","active":1,"normal":1,"candidate":null,"application":1,"migration":false,"rounds":0}}}}"#
    )
}

const ACK: &str = r#"{"response":"ack"}"#;

const CODELESS: &str = r#"{"response":"error","vCode":null,"message":"unknown variant"}"#;

fn lines(sent: &Arc<Mutex<Vec<String>>>) -> Vec<String> {
    sent.lock().unwrap_or_else(|err| err.into_inner()).clone()
}

#[spec_test(REQ_SC_session_client_004)]
#[test]
fn session_when_started_then_identity_is_the_first_command_and_the_payload_is_kept() {
    let (channel, sent) = scripted(vec![Ok(identity_line(SESSION_PROTOCOL_VERSION))]);

    let session = Session::start(channel, Timeouts::default()).unwrap();

    assert_eq!(lines(&sent), vec![r#"{"command":"identity"}"#.to_string()]);
    let identity = session.identity().unwrap();
    assert_eq!(identity.device.name, "ironplcvm");
    assert_eq!(
        identity.device.device_id.as_deref(),
        Some("0123456789abcdef0123456789abcdef")
    );
}

#[spec_test(REQ_SC_session_client_005)]
#[test]
fn session_when_device_speaks_a_newer_protocol_then_protocol_too_new() {
    let newer = SESSION_PROTOCOL_VERSION + 1;
    let (channel, _) = scripted(vec![Ok(identity_line(newer))]);

    let result = Session::start(channel, Timeouts::default());

    assert!(matches!(
        result,
        Err(SessionError::ProtocolTooNew { device, supported })
            if device == newer && supported == SESSION_PROTOCOL_VERSION
    ));
}

#[spec_test(REQ_SC_session_client_006)]
#[test]
fn session_when_device_does_not_know_identity_then_opens_without_identity_and_keeps_talking() {
    let (channel, sent) = scripted(vec![Ok(CODELESS.to_string()), Ok(ACK.to_string())]);

    let mut session = Session::start(channel, Timeouts::default()).unwrap();
    let response = session.send(&Command::CancelEdits).unwrap();

    assert!(session.identity().is_none());
    assert_eq!(response, Response::Ack);
    assert_eq!(lines(&sent).len(), 2);
}

#[test]
fn session_when_identity_is_answered_by_something_else_then_protocol_error() {
    let (channel, _) = scripted(vec![Ok(ACK.to_string())]);

    let result = Session::start(channel, Timeouts::default());

    assert!(matches!(
        result,
        Err(SessionError::Protocol { message }) if message.contains("an acknowledgement")
    ));
}

#[spec_test(REQ_SC_session_client_007)]
#[test]
fn session_when_command_is_refused_with_a_code_then_device_error_with_the_code_and_the_session_goes_on(
) {
    let refusal = r#"{"response":"error","vCode":"V4012","message":"no candidate is staged"}"#;
    let (channel, _) = scripted(vec![
        Ok(identity_line(SESSION_PROTOCOL_VERSION)),
        Ok(refusal.to_string()),
        Ok(ACK.to_string()),
    ]);
    let mut session = Session::start(channel, Timeouts::default()).unwrap();

    let refused = session.send(&Command::TestEdits);
    let next = session.send(&Command::CancelEdits);

    assert!(matches!(
        &refused,
        Err(SessionError::Device(error))
            if error.v_code() == "V4012" && error.message() == "no candidate is staged"
    ));
    assert_eq!(refused.unwrap_err().v_code(), Some("V4012"));
    assert_eq!(next.unwrap(), Response::Ack);
}

#[spec_test(REQ_SC_session_client_007)]
#[test]
fn session_when_type_changes_are_refused_then_the_pairs_reach_the_caller() {
    let refusal = r#"{"response":"error","vCode":"V4010","message":"decide","pairs":[{"uid":7,"name":"Counter","from":"I32","to":"U32","sizeEqual":true}]}"#;
    let (channel, _) = scripted(vec![
        Ok(identity_line(SESSION_PROTOCOL_VERSION)),
        Ok(refusal.to_string()),
    ]);
    let mut session = Session::start(channel, Timeouts::default()).unwrap();

    let result = session.send(&Command::TestEdits);

    assert!(matches!(
        result,
        Err(SessionError::Device(error))
            if error.pairs().is_some_and(|pairs| pairs.len() == 1 && pairs[0].uid == 7)
    ));
}

#[spec_test(REQ_SC_session_client_011)]
#[test]
fn session_when_line_is_not_a_command_then_codec_error_and_the_session_goes_on() {
    let (channel, _) = scripted(vec![
        Ok(identity_line(SESSION_PROTOCOL_VERSION)),
        Ok(CODELESS.to_string()),
        Ok(ACK.to_string()),
    ]);
    let mut session = Session::start(channel, Timeouts::default()).unwrap();

    let first = session.send(&Command::GetStatus);
    let second = session.send(&Command::GetStatus);

    assert!(matches!(
        first,
        Err(SessionError::Codec { message }) if message == "unknown variant"
    ));
    assert_eq!(second.unwrap(), Response::Ack);
}

/// Timeouts that no real wait is needed to tell apart.
const SHORT: Timeouts = Timeouts {
    connect: Duration::from_millis(7),
    response: Duration::from_millis(9),
};

#[spec_test(REQ_SC_session_client_009)]
#[test]
fn session_when_the_device_does_not_answer_identity_then_timed_out_in_the_identity_phase() {
    let (channel, _) = scripted(vec![Err(ChannelError::TimedOut)]);

    let handshake = Session::start(channel, SHORT);

    assert!(matches!(
        handshake,
        Err(SessionError::TimedOut { phase: Phase::Identity, limit })
            if limit == SHORT.connect
    ));
}

#[spec_test(REQ_SC_session_client_009)]
#[test]
fn session_when_the_device_does_not_answer_a_command_then_timed_out_in_the_response_phase() {
    let (channel, _) = scripted(vec![
        Ok(identity_line(SESSION_PROTOCOL_VERSION)),
        Err(ChannelError::TimedOut),
    ]);
    let mut session = Session::start(channel, SHORT).unwrap();

    let result = session.send(&Command::GetStatus);

    assert!(matches!(
        result,
        Err(SessionError::TimedOut { phase: Phase::Response, limit })
            if limit == SHORT.response
    ));
}

#[spec_test(REQ_SC_session_client_009)]
#[test]
fn timeouts_default_is_the_timeout_policy_of_the_runtime() {
    let timeouts = Timeouts::default();

    assert_eq!(timeouts.connect, CONNECT_TIMEOUT);
    assert_eq!(timeouts.response, RESPONSE_TIMEOUT);
}

#[spec_test(REQ_SC_session_client_010)]
#[test]
fn session_when_the_connection_ends_then_closed() {
    let (channel, _) = scripted(vec![
        Ok(identity_line(SESSION_PROTOCOL_VERSION)),
        Err(ChannelError::Closed(Some("V6001 no such file".to_string()))),
    ]);
    let mut session = Session::start(channel, Timeouts::default()).unwrap();

    let result = session.send(&Command::GetStatus);

    assert!(matches!(
        result,
        Err(SessionError::Closed { detail: Some(detail) }) if detail == "V6001 no such file"
    ));
}

#[test]
fn session_when_send_fails_and_a_refusal_is_waiting_then_the_refusal_is_the_error() {
    let refusal = r#"{"response":"error","vCode":"V6014","message":"one session only"}"#;
    let sent = Arc::new(Mutex::new(Vec::new()));
    let channel = Scripted {
        sent,
        send_failure: Some(ChannelError::Closed(None)),
        answers: VecDeque::from([Ok(refusal.to_string())]),
    };

    let result = Session::start(Box::new(channel), Timeouts::default());

    assert_eq!(
        result
            .err()
            .and_then(|err| err.v_code().map(str::to_string)),
        Some("V6014".to_string())
    );
}

#[test]
fn session_when_send_fails_and_nothing_is_waiting_then_closed() {
    let sent = Arc::new(Mutex::new(Vec::new()));
    let channel = Scripted {
        sent,
        send_failure: Some(ChannelError::Closed(None)),
        answers: VecDeque::new(),
    };

    let result = Session::start(Box::new(channel), Timeouts::default());

    assert!(matches!(result, Err(SessionError::Closed { detail: None })));
}

#[test]
fn session_when_stream_is_broken_then_protocol_error() {
    let (channel, _) = scripted(vec![Err(ChannelError::Broken("bad length".to_string()))]);

    let result = Session::start(channel, Timeouts::default());

    assert!(matches!(
        result,
        Err(SessionError::Protocol { message }) if message == "bad length"
    ));
}

#[test]
fn decode_when_answer_is_not_json_then_protocol_error() {
    assert!(matches!(
        decode("not json"),
        Err(SessionError::Protocol { .. })
    ));
}

#[test]
fn decode_when_error_carries_a_code_then_device_error() {
    let result = decode(r#"{"response":"error","vCode":"V6013","message":"bad frame"}"#);

    assert!(matches!(
        result,
        Err(SessionError::Device(error)) if error.v_code() == "V6013" && error.message() == "bad frame"
    ));
}
