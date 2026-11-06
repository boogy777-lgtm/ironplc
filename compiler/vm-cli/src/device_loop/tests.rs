//! Tests of the device loop (ADR-0086): the loop is driven one `step` at a
//! time on a fake clock, with the commands sent to its channel before the step
//! that executes them, so the tests decide when rounds run.

use std::sync::atomic::{AtomicBool, Ordering};
use std::sync::mpsc::Receiver;
use std::sync::Arc;
use std::time::{Duration, Instant};

use ironplc_container::Container;
use spec_test_macro::spec_test;

use super::*;
use crate::pacer::test_clock::{drive, FakeClock};
use crate::serve::compose_host;
use crate::serve_retained_tests::Served;
use crate::serve_tests::{compile_container, container_bytes, counter_source};

/// A program with one cyclic task of `interval` (an IEC duration literal), whose
/// program adds `step` to `Counter` in each of its rounds.
fn cyclic_source(step: i32, interval: &str) -> String {
    format!(
        "PROGRAM main
  VAR
    Counter : DINT;
  END_VAR
  Counter := Counter + {step};
END_PROGRAM

CONFIGURATION config
  RESOURCE res ON PLC
    TASK slow(INTERVAL := {interval}, PRIORITY := 1);
    PROGRAM instance WITH slow : main;
  END_RESOURCE
END_CONFIGURATION
"
    )
}

/// A program that traps in its first round: it divides by `Counter`, which
/// starts at 0. The declarations are those of `counter_source`, so a counter
/// candidate has the same layout.
const TRAPPING_SOURCE: &str = "PROGRAM main
  VAR
    Counter : DINT;
  END_VAR
  Counter := 1 / Counter;
END_PROGRAM
";

const TEST: &str = r#"{"command":"testEdits"}"#;
const UNTEST: &str = r#"{"command":"untestEdits"}"#;
const ASSEMBLE: &str = r#"{"command":"assembleEdits"}"#;
const IDENTITY: &str = r#"{"command":"identity"}"#;
const STATUS: &str = r#"{"command":"getStatus"}"#;

/// A device loop on a fake clock, and the ends of its channel and its stop flag.
struct Rig {
    device_loop: DeviceLoop<FakeClock>,
    clock: FakeClock,
    handle: LoopHandle,
    stop: Arc<AtomicBool>,
}

impl Rig {
    fn over(container: Container) -> Rig {
        let (host, shell) = compose_host(container, false).unwrap();
        Rig::with(host, Device::in_memory(), None, shell)
    }

    fn with(host: RuntimeHost, device: Device, store: Option<SlotStore>, shell: Shell) -> Rig {
        let clock = FakeClock::default();
        let stop = Arc::new(AtomicBool::new(false));
        let (handle, commands) = channel();
        let pacer = Pacer::with_clock(clock.clone(), Arc::clone(&stop), None);
        Rig {
            device_loop: DeviceLoop::new(host, store, device, shell, pacer, commands),
            clock,
            handle,
            stop,
        }
    }

    /// Sends a command line; its answer arrives on the returned receiver.
    fn send(&self, line: &str) -> Receiver<Answer> {
        self.handle.send(line.to_string()).unwrap()
    }

    /// Runs `count` iterations.
    fn steps(&mut self, count: usize) {
        for _ in 0..count {
            assert_eq!(self.device_loop.step(), None);
        }
    }

    fn rounds(&mut self) -> u64 {
        self.device_loop.core().host.status().rounds
    }

    /// The uptime at the start of each of `count` iterations that ran a round.
    fn round_times(&mut self, count: usize) -> Vec<u64> {
        let mut times = Vec::new();
        for _ in 0..count {
            let started_us = self.clock.now_us();
            let before = self.rounds();
            self.steps(1);
            if self.rounds() > before {
                times.push(started_us);
            }
        }
        times
    }

    /// Sends a command line and runs the iteration that executes it; returns
    /// the parsed answer.
    fn ask(&mut self, line: &str) -> serde_json::Value {
        let answer = self.send(line);
        self.steps(1);
        parsed(&answer)
    }
}

/// The answer that has arrived, parsed.
fn parsed(answer: &Receiver<Answer>) -> serde_json::Value {
    serde_json::from_str(&answer.try_recv().unwrap().unwrap()).unwrap()
}

fn accept_line(source: &str) -> String {
    serde_json::json!({"command": "acceptEdits", "program": container_bytes(source)}).to_string()
}

/// The same periodic program gives the loop the round times that `run` gives:
/// both ask the pacer, and the pacer's deadline is the program's.
#[spec_test(REQ_VC_vm_cli_042)]
#[test]
fn step_when_periodic_program_then_round_times_are_those_of_the_run_driver() {
    let mut rig = Rig::over(compile_container(&cyclic_source(1, "T#1ms")));

    let loop_times = rig.round_times(4);

    let run_clock = FakeClock::default();
    let mut run_pacer =
        Pacer::with_clock(run_clock.clone(), Arc::new(AtomicBool::new(false)), Some(4));
    let run_times = drive(&mut run_pacer, &run_clock, 100, |rounds| {
        Some(rounds * 1_000)
    });
    assert_eq!(loop_times, vec![0, 1_000, 2_000, 3_000]);
    assert_eq!(loop_times, run_times);
}

#[spec_test(REQ_VC_vm_cli_042)]
#[test]
fn step_when_period_is_long_then_no_round_between_deadlines_and_waits_are_the_stop_poll() {
    let mut rig = Rig::over(compile_container(&cyclic_source(1, "T#10s")));

    let times = rig.round_times(4);

    assert_eq!(times, vec![0]);
    assert_eq!(rig.clock.waits(), vec![50_000, 100_000, 150_000, 200_000]);
}

/// A request of a swap is answered after the boundary that applied it, and the
/// boundary does not wait for the period.
#[spec_test(REQ_VC_vm_cli_023)]
#[test]
fn step_when_test_requested_then_answered_after_the_boundary_that_applied_it() {
    let mut rig = Rig::over(compile_container(&cyclic_source(1, "T#10s")));
    rig.steps(1);
    assert_eq!(
        rig.ask(&accept_line(&cyclic_source(10, "T#10s")))["response"],
        "ack"
    );
    let before = rig.rounds();
    let answer = rig.send(TEST);
    assert!(answer.try_recv().is_err());

    rig.steps(1);

    assert_eq!(parsed(&answer)["response"], "ack");
    assert_eq!(rig.rounds(), before + 1);
    assert_eq!(rig.ask(STATUS)["mode"], "testing");
}

#[spec_test(REQ_VC_vm_cli_023)]
#[test]
fn step_when_test_untest_and_assemble_then_each_is_answered_after_its_boundary() {
    let mut rig = Rig::over(compile_container(&cyclic_source(1, "T#10s")));
    rig.steps(1);
    rig.ask(&accept_line(&cyclic_source(10, "T#10s")));

    let tested = rig.ask(TEST);
    let status_testing = rig.ask(STATUS);
    let untested = rig.ask(UNTEST);
    let status_untested = rig.ask(STATUS);
    let retested = rig.ask(TEST);
    let assembled = rig.ask(ASSEMBLE);
    let status_assembled = rig.ask(STATUS);

    assert_eq!(tested["response"], "ack");
    assert_eq!(status_testing["mode"], "testing");
    assert_eq!(untested["response"], "ack");
    assert_eq!(status_untested["mode"], "normal");
    assert_eq!(retested["response"], "ack");
    assert_eq!(assembled["response"], "ack");
    assert_eq!(status_assembled["mode"], "normal");
    assert_eq!(status_assembled["normal"], 2);
    assert_eq!(status_assembled["candidate"], serde_json::Value::Null);
}

#[spec_test(REQ_VC_vm_cli_043)]
#[test]
fn step_when_command_arrives_mid_period_then_answered_in_that_iteration_without_a_round() {
    let mut rig = Rig::over(compile_container(&cyclic_source(1, "T#10s")));
    rig.steps(2);
    let rounds = rig.rounds();
    let started_us = rig.clock.now_us();

    let identity = rig.ask(IDENTITY);

    assert_eq!(identity["response"], "identity");
    assert_eq!(rig.rounds(), rounds);
    assert!(rig.clock.now_us() - started_us <= 50_000);
}

#[spec_test(REQ_VC_vm_cli_042)]
#[spec_test(REQ_VC_vm_cli_043)]
#[test]
fn step_when_program_is_freewheeling_then_every_iteration_runs_a_round_and_commands_are_answered() {
    let mut rig = Rig::over(compile_container(&counter_source(1)));

    rig.steps(3);
    let status = rig.ask(STATUS);

    // The command is executed before the iteration's round.
    assert_eq!(status["rounds"], 3);
    assert_eq!(rig.rounds(), 4);
    assert!(rig.clock.waits().is_empty());
}

#[spec_test(REQ_VC_vm_cli_043)]
#[test]
fn step_when_program_is_freewheeling_then_many_commands_in_one_iteration_are_all_answered() {
    let mut rig = Rig::over(compile_container(&counter_source(1)));
    let answers = [rig.send(STATUS), rig.send(IDENTITY), rig.send(STATUS)];

    rig.steps(1);

    assert_eq!(parsed(&answers[0])["response"], "status");
    assert_eq!(parsed(&answers[1])["response"], "identity");
    assert_eq!(parsed(&answers[2])["response"], "status");
}

#[spec_test(REQ_VC_vm_cli_044)]
#[test]
fn step_when_round_traps_then_device_is_faulted_and_scans_no_more() {
    let mut rig = Rig::over(compile_container(TRAPPING_SOURCE));

    rig.steps(3);
    let identity = rig.ask(IDENTITY);

    assert_eq!(identity["fault"]["vCode"], "V4001");
    assert_eq!(identity["fault"]["taskId"], 0);
    assert_eq!(identity["fault"]["instanceId"], 0);
    assert!(identity["fault"]["message"]
        .as_str()
        .unwrap()
        .contains("divide by zero"));
    assert_eq!(identity["application"]["rounds"], 0);
}

#[spec_test(REQ_VC_vm_cli_044)]
#[test]
fn step_when_device_is_faulted_then_commands_are_answered_and_the_wait_is_idle() {
    let mut rig = Rig::over(compile_container(TRAPPING_SOURCE));
    rig.steps(1);

    let status = rig.ask(STATUS);

    assert_eq!(status["response"], "status");
    assert_eq!(rig.clock.waits().last(), Some(&100_000));
}

#[spec_test(REQ_VC_vm_cli_044)]
#[spec_test(REQ_VC_vm_cli_023)]
#[test]
fn step_when_device_is_faulted_then_a_swap_request_still_reaches_its_boundary() {
    let mut rig = Rig::over(compile_container(TRAPPING_SOURCE));
    rig.steps(1);
    rig.ask(&accept_line(&counter_source(1)));

    let tested = rig.ask(TEST);
    let status = rig.ask(STATUS);

    assert_eq!(tested["response"], "ack");
    assert_eq!(status["mode"], "testing");
    assert_eq!(status["rounds"], 0);
}

#[test]
fn step_when_nothing_trapped_then_identity_has_no_fault() {
    let mut rig = Rig::over(compile_container(&counter_source(1)));

    let identity = rig.ask(IDENTITY);

    assert!(identity.get("fault").is_none());
}

#[spec_test(REQ_VC_vm_cli_042)]
#[test]
fn step_when_host_has_no_permit_then_no_round_runs_and_commands_are_answered() {
    let mut rig = Rig::over(compile_container(&counter_source(1)));
    rig.device_loop.core().host.revoke_execution_permit();

    let status = rig.ask(STATUS);

    assert_eq!(status["rounds"], 0);
    assert_eq!(rig.rounds(), 0);
    assert_eq!(rig.clock.waits(), vec![50_000]);
}

#[spec_test(REQ_VC_vm_cli_045)]
#[test]
fn step_when_stop_flag_set_then_ends_stopped() {
    let mut rig = Rig::over(compile_container(&counter_source(1)));
    rig.steps(1);

    rig.stop.store(true, Ordering::Relaxed);

    assert_eq!(rig.device_loop.step(), Some(End::Stopped));
}

#[spec_test(REQ_VC_vm_cli_045)]
#[test]
fn step_when_stop_flag_set_with_a_swap_waiting_then_the_swap_is_answered_first() {
    let mut rig = Rig::over(compile_container(&counter_source(1)));
    rig.steps(1);
    rig.ask(&accept_line(&counter_source(10)));
    let answer = rig.send(TEST);
    rig.stop.store(true, Ordering::Relaxed);

    assert_eq!(rig.device_loop.step(), Some(End::Stopped));

    assert_eq!(parsed(&answer)["response"], "ack");
}

#[spec_test(REQ_VC_vm_cli_021)]
#[test]
fn step_when_every_sender_is_gone_then_waiting_commands_are_answered_and_the_loop_ends_closed() {
    let rig = Rig::over(compile_container(&counter_source(1)));
    let answer = rig.send(STATUS);
    let Rig {
        mut device_loop,
        handle,
        ..
    } = rig;
    drop(handle);

    assert_eq!(device_loop.step(), Some(End::Closed));

    assert_eq!(parsed(&answer)["response"], "status");
}

/// The stop flag is the controlled stop (Ctrl+C): the retained values are
/// written.
#[spec_test(REQ_VC_vm_cli_038)]
#[spec_test(REQ_VC_vm_cli_045)]
#[test]
fn stop_when_the_stop_flag_ended_the_loop_then_the_retained_file_is_written() {
    let served = Served::new();
    let (host, device, store) = served.boot();
    let mut rig = Rig::with(host, device, Some(store), Shell::standalone());
    rig.steps(2);
    rig.stop.store(true, Ordering::Relaxed);
    let Rig {
        mut device_loop,
        handle,
        ..
    } = rig;
    assert_eq!(device_loop.step(), Some(End::Stopped));
    assert!(!served.retained().exists());

    let stopped = device_loop.stop(Ok(()));

    drop(handle);
    assert!(stopped.is_ok());
    assert!(served.retained().exists());
}

/// The end of the input is the controlled stop too.
#[spec_test(REQ_VC_vm_cli_038)]
#[spec_test(REQ_VC_vm_cli_045)]
#[test]
fn stop_when_the_input_ended_then_the_retained_file_is_written() {
    let served = Served::new();
    let (host, device, store) = served.boot();
    let mut rig = Rig::with(host, device, Some(store), Shell::standalone());
    rig.steps(2);
    let Rig {
        mut device_loop,
        handle,
        ..
    } = rig;
    drop(handle);
    assert_eq!(device_loop.step(), Some(End::Closed));
    assert!(!served.retained().exists());

    let stopped = device_loop.stop(Ok(()));

    assert!(stopped.is_ok());
    assert!(served.retained().exists());
}

/// The periodic write of the retained values fires on the cadence of the
/// loop: at a boundary, when the period has passed.
#[test]
fn step_when_retained_save_period_is_on_then_the_retained_file_is_written_at_a_boundary() {
    let served = Served::new();
    let (host, mut device, store) = served.boot();
    let clock_ms = Arc::new(std::sync::atomic::AtomicU64::new(0));
    let milliseconds = Arc::clone(&clock_ms);
    device.use_retained_clock(Box::new(move || milliseconds.load(Ordering::Relaxed)));
    let mut rig = Rig::with(host, device, Some(store), Shell::standalone());
    let set = rig.ask(r#"{"command":"setDeviceSettings","retainedSavePeriod":10}"#);
    assert_eq!(set["response"], "ack");
    rig.steps(1);
    assert!(!served.retained().exists());

    clock_ms.store(10_000, Ordering::Relaxed);
    rig.steps(1);

    assert!(served.retained().exists());
}

#[test]
fn call_when_loop_has_stopped_then_error() {
    let (handle, commands) = channel();
    drop(commands);

    let err = handle.call(STATUS.to_string()).unwrap_err();

    assert_eq!(err.kind(), io::ErrorKind::BrokenPipe);
}

/// What `serve` does on a wall clock: the loop on its thread, a session
/// relaying lines to it.
#[spec_test(REQ_VC_vm_cli_019)]
#[spec_test(REQ_VC_vm_cli_021)]
#[test]
fn relay_session_when_lines_then_one_answer_line_each_and_the_loop_ends_with_the_input() {
    let (handle, commands) = channel();
    let runner = std::thread::spawn(move || {
        let (host, shell) = compose_host(compile_container(&counter_source(1)), false).unwrap();
        let pacer = Pacer::start(Arc::new(AtomicBool::new(false)), None);
        DeviceLoop::new(host, None, Device::in_memory(), shell, pacer, commands).run()
    });
    let mut output = Vec::new();

    relay_session(
        &handle,
        io::Cursor::new(format!("{STATUS}\n{IDENTITY}\n")),
        &mut output,
    )
    .unwrap();
    drop(handle);

    let text = String::from_utf8(output).unwrap();
    assert_eq!(text.lines().count(), 2);
    assert_eq!(runner.join().unwrap(), End::Closed);
}

/// A command that arrives while the loop waits for a deadline a period away is
/// answered in well under the period, on the wall clock, and the loop ends when
/// the stop flag is set.
#[spec_test(REQ_VC_vm_cli_043)]
#[spec_test(REQ_VC_vm_cli_045)]
#[test]
fn run_when_period_is_long_then_command_is_answered_within_the_stop_poll() {
    let stop = Arc::new(AtomicBool::new(false));
    let (handle, commands) = channel();
    let loop_stop = Arc::clone(&stop);
    let runner = std::thread::spawn(move || {
        let (host, shell) =
            compose_host(compile_container(&cyclic_source(1, "T#10s")), false).unwrap();
        let pacer = Pacer::start(loop_stop, None);
        DeviceLoop::new(host, None, Device::in_memory(), shell, pacer, commands).run()
    });
    std::thread::sleep(Duration::from_millis(100));
    let asked = Instant::now();

    let answer = handle.call(IDENTITY.to_string()).unwrap();

    assert!(asked.elapsed() < Duration::from_secs(2));
    assert!(answer.contains("identity"));
    stop.store(true, Ordering::Relaxed);
    assert_eq!(runner.join().unwrap(), End::Stopped);
    drop(handle);
}
