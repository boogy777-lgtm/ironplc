//! The two-process HA pair proof: two real `ironplcvm serve` processes
//! form a redundant pair over UDP on 127.0.0.1, and the ADR-0064
//! hot-edit synchronization contract is asserted against both units
//! through their own engineering sessions.
//!
//! What this adds over the in-process loopback suites
//! (`ironplc-redundancy/tests/pair_link.rs`, `tests/crossload.rs`): real
//! processes, a real UDP socket binding behind the `NicPort` seam, real
//! piped stdio sessions, process death as the failover trigger, and the
//! slot-store persistence of both units on assemble. The scenarios they
//! pin — accept crossloads the same candidate generation and snapshot
//! bytes, takeover mid-Test executes the CANDIDATE (never a revert to
//! Original, V4011), assemble is one transaction across the pair — are
//! the contract this file proves end to end; the loopback binding stays
//! the test vehicle for the FSM internals.
//!
//! Windows-safe process handling: ephemeral ports via reserve-release,
//! one reader thread per child (a pipe read would deadlock the pump if
//! the child died), every wait bounded, and `Drop` kills the children so
//! a failing assertion leaks no processes.

// Test-target boundary: the workspace denies panicking constructs in
// production code; tests assert by panicking, so they are exempt here.
#![allow(
    clippy::unwrap_used,
    clippy::expect_used,
    reason = "integration test target: panicking helpers are sanctioned in tests"
)]

use std::io::{BufRead, BufReader, Write};
use std::net::{SocketAddr, UdpSocket};
use std::path::{Path, PathBuf};
use std::process::{Child, ChildStdin, Command, Stdio};
use std::sync::mpsc;
use std::time::{Duration, Instant};

use assert_cmd::cargo;
use ironplc_container::Container;
use serde_json::Value;
use tempfile::TempDir;

/// The base application both units boot: one DINT `Counter`, +1/scan,
/// compiled with a stable variable UID so a declaration edit stages as a
/// migration candidate.
const BASE_SOURCE: &str = "PROGRAM main
  VAR
    Counter : DINT;
  END_VAR
  Counter := Counter + 1;
END_PROGRAM
";

/// The edit candidate: an added DINT `Gauge` (the schema change that
/// makes the candidate a migration candidate) and a +10 scan step (so
/// the CANDIDATE is distinguishable from the ORIGINAL on execution).
const CANDIDATE_SOURCE: &str = "PROGRAM main
  VAR
    Counter : DINT;
    Gauge : DINT;
  END_VAR
  Counter := Counter + 10;
  Gauge := Gauge + 1;
END_PROGRAM
";

/// The edit name the Accept carries (the pending-edit record contract).
const EDIT_NAME: &str = "add-gauge";

/// One observed step of the pair's synchronization, recorded by the
/// polling queries and printed with the test's output: the actual
/// event/transition sequence the demo documents.
#[derive(Debug)]
struct Observation {
    elapsed_ms: u128,
    unit: &'static str,
    sync: String,
    mode: String,
    epoch: u64,
    application: u64,
    candidate: Option<u64>,
    rounds: u64,
}

impl Observation {
    fn render(&self) -> String {
        format!(
            "t+{:>5}ms  {}  sync={:<9} mode={:<7} epoch={:<3} app={:<3} candidate={:<9} rounds={}",
            self.elapsed_ms,
            self.unit,
            self.sync,
            self.mode,
            self.epoch,
            self.application,
            self.candidate
                .map(|c| c.to_string())
                .unwrap_or_else(|| "-".to_string()),
            self.rounds,
        )
    }
}

/// Compiles `source` with the engineering-side stable variable IDs and
/// round-trips the container through the wire format, mirroring the
/// serve-session test fixtures.
fn compile_with_ids(source: &str, ids: &[(&str, u64)]) -> Container {
    let options = ironplc_parser::options::CompilerOptions::default();
    let library =
        ironplc_parser::parse_program(source, &ironplc_dsl::core::FileId::default(), &options)
            .unwrap();
    let (analyzed, context) =
        ironplc_analyzer::stages::resolve_types(&[&library], &options).unwrap();
    let codegen_options = ironplc_codegen::CodegenOptions {
        stable_var_ids: ids
            .iter()
            .map(|(name, uid)| (ironplc_dsl::core::Id::from(name), *uid))
            .collect(),
        ..ironplc_codegen::CodegenOptions::default()
    };
    let container = ironplc_codegen::compile(
        &analyzed,
        &context,
        &codegen_options,
        &ironplc_codegen::EmptyLookup,
    )
    .unwrap();
    let mut bytes = Vec::new();
    container.write_to(&mut bytes).unwrap();
    Container::read_from(&mut std::io::Cursor::new(&bytes)).unwrap()
}

/// The wire-format bytes an `acceptEdits` command carries.
fn container_bytes(container: &Container) -> Vec<u8> {
    let mut bytes = Vec::new();
    container.write_to(&mut bytes).unwrap();
    bytes
}

/// Reserves one ephemeral loopback UDP port (bind, read, release) — the
/// standard pattern; on loopback the reuse window is benign.
fn free_udp_port() -> u16 {
    let socket = UdpSocket::bind("127.0.0.1:0").unwrap();
    let port = socket.local_addr().unwrap().port();
    drop(socket);
    port
}

/// One spawned serve process with its piped session.
struct Session {
    name: &'static str,
    child: Child,
    stdin: ChildStdin,
    /// One response line per command line, fed by the reader thread.
    responses: mpsc::Receiver<String>,
    start: Instant,
    observations: Vec<Observation>,
}

impl Session {
    /// Spawns one pair unit: `ironplcvm serve <file> --ha-role <role>`
    /// with the pair link bound to `bind`, peering at `peer`.
    fn start(
        name: &'static str,
        role: &str,
        file: &Path,
        bind: SocketAddr,
        peer: SocketAddr,
    ) -> Self {
        let child = Command::new(cargo::cargo_bin("ironplcvm"))
            .arg("serve")
            .arg(file)
            .arg("--ha-role")
            .arg(role)
            .arg("--ha-peer-bind")
            .arg(bind.to_string())
            .arg("--ha-peer-peer")
            .arg(peer.to_string())
            .stdin(Stdio::piped())
            // stderr is dropped: the logger only writes diagnostics, and
            // an undrained pipe would deadlock a chatty child.
            .stdout(Stdio::piped())
            .stderr(Stdio::null())
            .spawn()
            .unwrap();
        let mut child = child;
        let stdin = child.stdin.take().unwrap();
        let stdout = child.stdout.take().unwrap();
        let (tx, responses) = mpsc::channel();
        std::thread::spawn(move || {
            let mut reader = BufReader::new(stdout);
            let mut line = String::new();
            loop {
                line.clear();
                match reader.read_line(&mut line) {
                    Ok(0) | Err(_) => return,
                    Ok(_) => {
                        if tx.send(line.clone()).is_err() {
                            return;
                        }
                    }
                }
            }
        });
        Self {
            name,
            child,
            stdin,
            responses,
            start: Instant::now(),
            observations: Vec::new(),
        }
    }

    /// Sends one command line and reads its one response line.
    fn command(&mut self, line: &str) -> Value {
        self.stdin.write_all(line.as_bytes()).unwrap();
        self.stdin.write_all(b"\n").unwrap();
        self.stdin.flush().unwrap();
        let received = self.responses.recv_timeout(Duration::from_secs(10));
        assert!(received.is_ok(), "{}: no response to {line}", self.name);
        let response = received.unwrap();
        let parsed = serde_json::from_str(&response);
        assert!(parsed.is_ok(), "{}: bad response {response}", self.name);
        parsed.unwrap()
    }

    /// Repeats `command` until `predicate` holds or the deadline passes,
    /// recording every observation. The pair converges on the pump
    /// cadence, so polling is the scripted client's read model.
    fn query_until(
        &mut self,
        command: &str,
        deadline: Duration,
        mut predicate: impl FnMut(&Value) -> bool,
    ) -> Value {
        let deadline = Instant::now() + deadline;
        loop {
            let response = self.command(command);
            self.record(&response);
            if predicate(&response) {
                return response;
            }
            assert!(
                Instant::now() <= deadline,
                "{}: {command} never satisfied the predicate; last response: {response}",
                self.name
            );
            std::thread::sleep(Duration::from_millis(20));
        }
    }

    /// Records one status response as an observation of this unit.
    fn record(&mut self, response: &Value) {
        let sync = response["local"]["sync"]
            .as_str()
            .unwrap_or("-")
            .to_string();
        let (mode, application, candidate, rounds) = match response["mode"].as_str() {
            Some(mode) => (
                mode.to_string(),
                response["application"].as_u64().unwrap_or(0),
                response["candidate"].as_u64(),
                response["rounds"].as_u64().unwrap_or(0),
            ),
            None => ("-".to_string(), 0, None, 0),
        };
        let epoch = response["epoch"]
            .as_u64()
            .unwrap_or(response["local"]["epoch"].as_u64().unwrap_or(0));
        self.observations.push(Observation {
            elapsed_ms: self.start.elapsed().as_millis(),
            unit: self.name,
            sync,
            mode,
            epoch,
            application,
            candidate,
            rounds,
        });
    }

    /// Prints the observed transition sequence (the demo's captured
    /// output with `--nocapture`).
    fn print_observations(&self) {
        for observation in &self.observations {
            eprintln!("{}", observation.render());
        }
    }

    /// Kills the child and reaps it, bounded.
    fn kill(mut self) {
        let _ = self.child.kill();
        let _ = self.child.wait();
    }
}

impl Drop for Session {
    fn drop(&mut self) {
        let _ = self.child.kill();
        let _ = self.child.wait();
    }
}

/// Writes the base container beside two unit files and returns their
/// paths (each unit's slot store lives beside its own file).
fn unit_files(dir: &TempDir, base: &Container) -> (PathBuf, PathBuf) {
    let bytes = container_bytes(base);
    let file_a = dir.path().join("unit-a.iplc");
    let file_b = dir.path().join("unit-b.iplc");
    std::fs::write(&file_a, &bytes).unwrap();
    std::fs::write(&file_b, &bytes).unwrap();
    (file_a, file_b)
}

/// The active slot file the marker names: the committed artifact's home.
fn active_slot(file: &Path) -> PathBuf {
    let marker = std::fs::read_to_string(file.with_extension("iplc.marker")).unwrap();
    let marker: Value = serde_json::from_str(&marker).unwrap();
    let slot = marker["slot"].as_str().unwrap();
    file.with_extension(format!("iplc.slot-{slot}"))
}

/// Boots a converged pair over reserved loopback ports and returns the
/// sessions with B already at SYNC_READY.
fn converged_pair(dir: &TempDir) -> (Session, Session) {
    let base = compile_with_ids(BASE_SOURCE, &[("Counter", 1)]);
    let (file_a, file_b) = unit_files(dir, &base);
    let port_a = free_udp_port();
    let port_b = free_udp_port();
    let addr = |port: u16| SocketAddr::from(([127, 0, 0, 1], port));
    let mut a = Session::start("A/primary", "primary", &file_a, addr(port_a), addr(port_b));
    let mut b = Session::start(
        "B/secondary",
        "secondary",
        &file_b,
        addr(port_b),
        addr(port_a),
    );

    // Boot: both units discover, admit, replicate the initial image, and
    // reach SYNC_READY (the Secondary in monitor mode: no permit, no
    // rounds).
    b.query_until(
        r#"{"command":"haStatus"}"#,
        Duration::from_secs(15),
        |status| status["local"]["sync"] == "syncReady" && status["linkValid"] == true,
    );
    let a_status = a.query_until(
        r#"{"command":"haStatus"}"#,
        Duration::from_secs(15),
        |status| status["local"]["sync"] == "syncReady",
    );
    assert_eq!(a_status["local"]["role"], "primary");
    assert_eq!(a_status["peer"]["role"], "secondary");
    let b_status = b.command(r#"{"command":"haStatus"}"#);
    b.record(&b_status);
    assert_eq!(b_status["local"]["role"], "secondary");
    (a, b)
}

#[test]
fn ha_pair_when_accept_test_assemble_then_both_units_commit_one_generation() {
    let dir = TempDir::new().unwrap();
    let (mut a, mut b) = converged_pair(&dir);

    // Accept on the Primary, naming the edit.
    let candidate = compile_with_ids(CANDIDATE_SOURCE, &[("Counter", 1), ("Gauge", 2)]);
    let candidate_wire = container_bytes(&candidate);
    let accept = serde_json::json!({
        "command": "acceptEdits",
        "program": candidate_wire,
        "edit": {"name": EDIT_NAME},
    })
    .to_string();
    let accepted = a.command(&accept);
    assert_eq!(accepted["response"], "ack");
    let a_status = a.command(r#"{"command":"getStatus"}"#);
    a.record(&a_status);
    let offered = a_status["candidate"].as_u64().unwrap();
    assert!(offered > 0);
    assert_eq!(a_status["pendingEdit"]["name"], EDIT_NAME);

    // ADR-0064(c): the Secondary stages the SAME CandidateGenerationId
    // and applies the snapshot — it reports the candidate and stays
    // redundancy-ready.
    let b_status = b.query_until(
        r#"{"command":"getStatus"}"#,
        Duration::from_secs(10),
        |status| status["candidate"].as_u64() == Some(offered),
    );
    assert_eq!(b_status["migration"], true);
    let b_ha = b.command(r#"{"command":"haStatus"}"#);
    b.record(&b_ha);
    assert_eq!(b_ha["local"]["sync"], "syncReady");

    // Test: both selectors switch at the boundary; on the Secondary the
    // migration candidate's replicated image is what flips it — proof
    // the snapshot applied.
    let tested = a.command(r#"{"command":"testEdits"}"#);
    assert_eq!(tested["response"], "ack");
    let a_status = a.command(r#"{"command":"getStatus"}"#);
    a.record(&a_status);
    assert_eq!(a_status["mode"], "testing");
    let b_status = b.query_until(
        r#"{"command":"getStatus"}"#,
        Duration::from_secs(10),
        |status| status["mode"] == "testing",
    );
    assert_eq!(b_status["candidate"].as_u64(), Some(offered));

    // Assemble (ADR-0064(h)): one transaction across the pair. Both
    // units promote the same canonical application generation, the
    // candidate is gone, and the epochs converge on the minted value.
    let assembled = a.command(r#"{"command":"assembleEdits"}"#);
    assert_eq!(assembled["response"], "ack");
    let a_status = a.query_until(
        r#"{"command":"getStatus"}"#,
        Duration::from_secs(10),
        |status| status["candidate"].is_null() && status["application"].as_u64() == Some(offered),
    );
    let b_status = b.query_until(
        r#"{"command":"getStatus"}"#,
        Duration::from_secs(10),
        |status| status["candidate"].is_null() && status["application"].as_u64() == Some(offered),
    );
    assert_eq!(a_status["mode"], "normal");
    assert_eq!(b_status["mode"], "normal");

    // The epoch transaction: the assemble boundary minted the epoch on
    // the owner; the standby adopted it off the wire.
    let epoch_a = loop {
        let status = a.command(r#"{"command":"haStatus"}"#);
        a.record(&status);
        if status["local"]["sync"] == "syncReady" && status["epoch"] == status["peer"]["epoch"] {
            break status["epoch"].as_u64().unwrap();
        }
        std::thread::sleep(Duration::from_millis(20));
    };
    let status_b = b.query_until(
        r#"{"command":"haStatus"}"#,
        Duration::from_secs(10),
        |status| status["epoch"].as_u64() == Some(epoch_a),
    );
    b.record(&status_b);

    // ADR-0064 amendment: both units persisted the identical committed
    // bytes inside the one transaction.
    for unit_file in [
        dir.path().join("unit-a.iplc"),
        dir.path().join("unit-b.iplc"),
    ] {
        let slot = std::fs::read(active_slot(&unit_file)).unwrap();
        assert_eq!(slot, candidate_wire, "{} slot bytes", unit_file.display());
    }

    a.print_observations();
    b.print_observations();
    a.kill();
    b.kill();
}

#[test]
fn ha_pair_when_primary_dies_mid_test_then_secondary_executes_the_candidate() {
    let dir = TempDir::new().unwrap();
    let (mut a, mut b) = converged_pair(&dir);

    // Accept the migration candidate and test it on the pair: both
    // selectors are on the CANDIDATE when the Primary dies.
    let candidate = compile_with_ids(CANDIDATE_SOURCE, &[("Counter", 1), ("Gauge", 2)]);
    let accept = serde_json::json!({
        "command": "acceptEdits",
        "program": container_bytes(&candidate),
        "edit": {"name": EDIT_NAME},
    })
    .to_string();
    assert_eq!(a.command(&accept)["response"], "ack");
    assert_eq!(a.command(r#"{"command":"testEdits"}"#)["response"], "ack");
    b.query_until(
        r#"{"command":"getStatus"}"#,
        Duration::from_secs(10),
        |status| status["mode"] == "testing",
    );

    // The Primary dies mid-Test.
    a.kill();

    // ADR-0064(e): the survivor detects the death, promotes, and takes
    // over executing the CANDIDATE — the selector never reverts (an
    // untest is refused, V4011), and the boundary round the takeover
    // drives advances the rounds counter on the promoted unit.
    let b_status = b.query_until(
        r#"{"command":"getStatus"}"#,
        Duration::from_secs(15),
        |status| status["mode"] == "testing" && status["rounds"].as_u64().unwrap_or(0) >= 1,
    );
    assert!(b_status["candidate"].as_u64().is_some());
    let b_ha = b.command(r#"{"command":"haStatus"}"#);
    b.record(&b_ha);
    assert_eq!(b_ha["linkValid"], false);
    let untest = b.command(r#"{"command":"untestEdits"}"#);
    assert_eq!(untest["response"], "error");
    assert_eq!(untest["vCode"], "V4011");

    // The survivor completes the lifecycle: it assembles the candidate
    // it is executing — the commit acks on the promoted unit.
    let assembled = b.command(r#"{"command":"assembleEdits"}"#);
    assert_eq!(assembled["response"], "ack");
    let b_status = b.query_until(
        r#"{"command":"getStatus"}"#,
        Duration::from_secs(10),
        |status| status["candidate"].is_null() && status["mode"] == "normal",
    );
    assert!(b_status["application"].as_u64().unwrap() > 0);

    b.print_observations();
    b.kill();
}
