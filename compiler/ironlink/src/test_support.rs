//! Fake devices for the tests: a thread with a UDP socket on a loopback port
//! that answers discovery requests the way a test asks it to, and remembers
//! every datagram it received and when.
//!
//! The real device is `ironplcvm serve --listen`; this one stands in for it so
//! that the tests of IronLink need no other process and no fixed port.

use std::net::{IpAddr, Ipv4Addr, SocketAddr, UdpSocket};
use std::sync::atomic::{AtomicBool, Ordering};
use std::sync::{Arc, Mutex};
use std::thread::JoinHandle;
use std::time::{Duration, Instant};

use ironplc_discovery::{encode_reply, is_request, DeviceId, Reply, MAX_REPLY_LEN};

use crate::collect::{Stop, Window};
use crate::observation::{Heard, Observation, OtherVersion};
use crate::store::Store;

/// What the fake device says about itself.
#[derive(Clone, Debug)]
pub(crate) struct FakeIdentity {
    pub(crate) device_id: DeviceId,
    pub(crate) name: String,
    pub(crate) model: String,
    pub(crate) runtime_version: String,
    pub(crate) protected: bool,
}

impl FakeIdentity {
    /// An identity whose device id is `id_byte` sixteen times.
    pub(crate) fn new(id_byte: u8, name: &str) -> Self {
        FakeIdentity {
            device_id: DeviceId::from_bytes([id_byte; 16]),
            name: name.to_string(),
            model: "IronPLC SoftPLC".to_string(),
            runtime_version: "0.247.0".to_string(),
            protected: false,
        }
    }
}

/// How the fake device answers a request.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub(crate) enum Behaviour {
    /// One valid reply.
    Answer,
    /// The same valid reply twice.
    AnswerTwice,
    /// A reply that names another discovery wire version.
    WireVersion(u8),
    /// A datagram that is not a reply.
    Garbage,
    /// A datagram longer than the longest reply.
    Oversize,
    /// No answer at all.
    Silent,
}

/// A datagram the fake device received, and when.
#[derive(Clone, Debug)]
pub(crate) struct Received {
    pub(crate) at: Instant,
    pub(crate) from: SocketAddr,
    pub(crate) bytes: Vec<u8>,
}

/// A device on a loopback port, stopped when it is dropped.
pub(crate) struct FakeDevice {
    addr: SocketAddr,
    received: Arc<Mutex<Vec<Received>>>,
    stop: Arc<AtomicBool>,
    thread: Option<JoinHandle<()>>,
}

impl FakeDevice {
    /// Starts a device on `127.0.0.1` with a port of the system's choosing.
    pub(crate) fn start(identity: FakeIdentity, behaviour: Behaviour) -> Self {
        let socket = UdpSocket::bind((Ipv4Addr::LOCALHOST, 0)).unwrap();
        let addr = socket.local_addr().unwrap();
        let received = Arc::new(Mutex::new(Vec::new()));
        let stop = Arc::new(AtomicBool::new(false));
        let thread = {
            let received = Arc::clone(&received);
            let stop = Arc::clone(&stop);
            std::thread::spawn(move || {
                answer_until_stopped(&socket, &identity, behaviour, &received, &stop);
            })
        };
        FakeDevice {
            addr,
            received,
            stop,
            thread: Some(thread),
        }
    }

    /// An answering device named `name`, with a device id of `id_byte`s.
    pub(crate) fn answering(id_byte: u8, name: &str) -> Self {
        Self::start(FakeIdentity::new(id_byte, name), Behaviour::Answer)
    }

    pub(crate) fn addr(&self) -> SocketAddr {
        self.addr
    }

    /// The datagrams received, in order, when the caller expects `expected` of
    /// them.
    ///
    /// The device records on a thread of its own, so a datagram sent just before
    /// the caller asks may not be written down yet, and on a loaded machine it
    /// may be late by a good while. This waits (up to five seconds) until
    /// `expected` are recorded, and then a little longer, so that an unexpected
    /// extra one is seen too.
    pub(crate) fn received(&self, expected: usize) -> Vec<Received> {
        for _ in 0..100 {
            if self.received.lock().unwrap().len() >= expected {
                break;
            }
            std::thread::sleep(Duration::from_millis(50));
        }
        std::thread::sleep(Duration::from_millis(100));
        self.received.lock().unwrap().clone()
    }

    /// The times between successive datagrams, when `expected` are expected.
    pub(crate) fn gaps(&self, expected: usize) -> Vec<Duration> {
        self.received(expected)
            .windows(2)
            .map(|pair| pair[1].at.duration_since(pair[0].at))
            .collect()
    }
}

impl Drop for FakeDevice {
    fn drop(&mut self) {
        self.stop.store(true, Ordering::SeqCst);
        wake(self.addr);
        if let Some(thread) = self.thread.take() {
            thread.join().unwrap();
        }
    }
}

/// Ends the wait of the device at `addr` for a datagram, after its stop flag is
/// set.
fn wake(addr: SocketAddr) {
    let sender = UdpSocket::bind((Ipv4Addr::LOCALHOST, 0)).unwrap();
    sender.send_to(&[0], addr).unwrap();
}

/// The valid reply of `identity` for a device at `session_port`.
fn reply_bytes(identity: &FakeIdentity, session_port: u16) -> Vec<u8> {
    let reply = Reply {
        protocol: 1,
        session_port,
        device_id: identity.device_id,
        protected: identity.protected,
        name: &identity.name,
        model: &identity.model,
        runtime_version: &identity.runtime_version,
    };
    let mut buffer = vec![0u8; MAX_REPLY_LEN];
    let len = encode_reply(&reply, &mut buffer).unwrap();
    buffer.truncate(len);
    buffer
}

/// The datagrams `behaviour` sends in answer to one request.
fn answers(identity: &FakeIdentity, behaviour: Behaviour, session_port: u16) -> Vec<Vec<u8>> {
    let valid = reply_bytes(identity, session_port);
    match behaviour {
        Behaviour::Answer => vec![valid],
        Behaviour::AnswerTwice => vec![valid.clone(), valid],
        Behaviour::WireVersion(version) => {
            let mut other = valid;
            // The wire version is the byte after the four of the magic.
            other[4] = version;
            vec![other]
        }
        Behaviour::Garbage => vec![b"this is not a discovery reply".to_vec()],
        Behaviour::Oversize => vec![vec![0u8; MAX_REPLY_LEN + 10]],
        Behaviour::Silent => Vec::new(),
    }
}

fn answer_until_stopped(
    socket: &UdpSocket,
    identity: &FakeIdentity,
    behaviour: Behaviour,
    received: &Mutex<Vec<Received>>,
    stop: &AtomicBool,
) {
    let session_port = socket.local_addr().unwrap().port();
    let mut buffer = [0u8; 2048];
    while !stop.load(Ordering::SeqCst) {
        // The wait has no timeout: a datagram that arrives as a read timeout
        // expires can be lost on Windows, and a fake that loses requests makes a
        // test of retries fail at random. [`wake`] ends the wait. An error is
        // Windows reporting that an earlier reply reached a closed port.
        let received_from = socket.recv_from(&mut buffer);
        if stop.load(Ordering::SeqCst) {
            return;
        }
        let Ok((len, from)) = received_from else {
            continue;
        };
        let bytes = buffer[..len].to_vec();
        let request = is_request(&bytes);
        received.lock().unwrap().push(Received {
            at: Instant::now(),
            from,
            bytes,
        });
        if request {
            for datagram in answers(identity, behaviour, session_port) {
                // The client may be gone already; a lost reply is what a test of
                // silence looks like anyway.
                drop(socket.send_to(&datagram, from));
            }
        }
    }
}

/// A window for the tests, with the stop rule of a browse.
pub(crate) fn window(timeout_ms: u64, attempts: u32) -> Window {
    Window::from_options(Some(timeout_ms), Some(attempts), Stop::AtDeadline).unwrap()
}

/// A store in a directory that does not exist yet, under a new temporary
/// directory that the guard removes. Every test of the store, and every test that
/// runs a command, uses this: no test reaches the directory of the user.
pub(crate) fn temp_store() -> (tempfile::TempDir, Store) {
    let guard = tempfile::tempdir().unwrap();
    let store = Store::at(guard.path().join("ironlink"));
    (guard, store)
}

/// The loopback address as an `IpAddr`.
pub(crate) fn loopback() -> IpAddr {
    IpAddr::V4(Ipv4Addr::LOCALHOST)
}

/// The observations among what was heard, in order.
pub(crate) fn devices_of(heard: &[Heard]) -> Vec<Observation> {
    heard
        .iter()
        .filter_map(|heard| match heard {
            Heard::Device(observation) => Some(observation.clone()),
            Heard::OtherVersion(_) => None,
        })
        .collect()
}

/// The other-version outcomes among what was heard, in order.
pub(crate) fn other_versions_of(heard: &[Heard]) -> Vec<OtherVersion> {
    heard
        .iter()
        .filter_map(|heard| match heard {
            Heard::OtherVersion(other) => Some(other.clone()),
            Heard::Device(_) => None,
        })
        .collect()
}
