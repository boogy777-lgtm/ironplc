//! The single-threaded DAP event loop.
//!
//! This module drives the `initialize` → `launch` → `setBreakpoints` →
//! `configurationDone` → (run) → `stopped` → inspect → `continue` →
//! `terminated` → `disconnect` lifecycle against the VM debug engine. Every
//! request is gated through the [`state::legal`] table; an illegal or
//! not-yet-supported request is answered with a DAP error whose message is
//! `requestNotApplicable`. The single-threaded design is described in
//! `specs/design/debugger-support.md` §"Single-threaded DAP loop (v1)".
//!
//! The loop is split at the launch boundary so lifetimes stay simple: the
//! *pre-launch* loop ([`serve`]) handles `initialize` / `disconnect` and the
//! `launch` preconditions with nothing borrowed; once preconditions pass it
//! hands the owned [`Container`] to [`launched_session`], which sizes the VM
//! buffers, starts the VM, and runs the *post-launch* run/stop loop borrowing
//! them.
//!
//! Not yet implemented: a trap ends the session as `terminated` rather than
//! surfacing a `stopped{reason:"exception"}`.

use std::io::{self, BufRead, Write};
use std::num::NonZeroU64;
use std::path::Path;
use std::time::Duration;

use ironplc_container::debug_section::DebugSection;
use ironplc_container::{Container, VarIndex};
use ironplc_vm::{
    has_freewheeling_task, interval_from_ms, interval_us, resolve_cycle_time, BreakpointTable,
    DebuggerHook, PauseReason, RoundOutcome, StepMode, VmBuffers, VmRunning,
    DEFAULT_FREEWHEELING_INTERVAL,
};
use serde::Serialize;
use serde_json::{json, Value};

use super::debug_info;
use super::framing;
use super::launch;
use super::state::{self, Command, Phase};
use super::types::{
    Breakpoint, Capabilities, ContinueResponseBody, Event, LaunchRequestArguments, Request,
    Response, Scope, ScopesResponseBody, SetBreakpointsArguments, SetBreakpointsResponseBody,
    Source, StackFrame, StackTraceResponseBody, StoppedEventBody, Thread, ThreadsResponseBody,
    Variable, VariablesArguments, VariablesResponseBody,
};

/// The id of the single synthetic thread the v1 server exposes.
const THREAD_ID: i64 = 1;

/// The `variablesReference` handle for the `Program` scope: the program's ST
/// variables. Non-zero so DAP treats it as expandable; the (flat) list of
/// program variables is returned for it. Structured expansion (nested FB fields)
/// is a later phase, so every returned
/// [`Variable`](super::types::Variable) has `variablesReference: 0`.
const PROGRAM_REF: i64 = 1;

/// The `variablesReference` handle for the `Runtime` scope: VM-level state that
/// is not a program variable. Keeping it in its own scope means a synthetic
/// entry can never collide with an ST variable of the same name.
const RUNTIME_REF: i64 = 2;

/// The DAP `message` returned for any request that is illegal in the current
/// phase or not supported by this server slice.
const REQUEST_NOT_APPLICABLE: &str = "requestNotApplicable";

/// Serializes a DAP message and writes it with Content-Length framing.
fn send<W: Write, T: Serialize>(writer: &mut W, message: &T) -> io::Result<()> {
    let body =
        serde_json::to_vec(message).map_err(|e| io::Error::new(io::ErrorKind::InvalidData, e))?;
    framing::write_message(writer, &body)
}

/// Returns the current outgoing sequence number and advances the counter.
fn take_seq(seq: &mut i64) -> i64 {
    let current = *seq;
    *seq += 1;
    current
}

/// Runs the DAP server over `reader`/`writer` until the client disconnects or
/// the stream ends.
///
/// This is the pre-launch loop: it services `initialize` and `disconnect`,
/// evaluates the `launch` preconditions, and — on a satisfied `launch` — hands
/// off to [`launched_session`] and returns whatever that returns. A launch that
/// fails a precondition is answered with an error and the loop continues, so
/// the client may retry or disconnect.
pub fn serve<R: BufRead, W: Write>(reader: &mut R, writer: &mut W) -> io::Result<()> {
    let mut seq: i64 = 1;
    let mut phase = Phase::Initialized;

    loop {
        let Some(body) = framing::read_message(reader)? else {
            // Clean end-of-stream between messages: the client went away.
            return Ok(());
        };
        let Ok(request) = serde_json::from_slice::<Request>(&body) else {
            // A frame we cannot parse as a request carries no seq to answer;
            // skip it rather than crash the session.
            continue;
        };

        let command = Command::from_request(&request.command);
        let legal_here = command.is_some_and(|c| state::legal(phase, c));

        match command {
            Some(Command::Initialize) if legal_here => {
                let caps = Capabilities {
                    supports_configuration_done_request: true,
                };
                let body = serde_json::to_value(caps).ok();
                send(
                    writer,
                    &Response::success(take_seq(&mut seq), &request, body),
                )?;
                // DAP: the `initialized` event follows the initialize response.
                send(writer, &Event::new(take_seq(&mut seq), "initialized", None))?;
                phase = Phase::Configuring;
            }
            Some(Command::Launch) if legal_here => {
                match load_and_check(&request) {
                    Ok((container, args, scan_limit)) => {
                        // Preconditions hold: own the container and run the
                        // rest of the session against a live VM.
                        return launched_session(
                            reader, writer, &mut seq, container, args, scan_limit, &request,
                        );
                    }
                    Err(message) => {
                        send(
                            writer,
                            &Response::error(take_seq(&mut seq), &request, message),
                        )?;
                    }
                }
            }
            Some(Command::Disconnect) => {
                send(
                    writer,
                    &Response::success(take_seq(&mut seq), &request, None),
                )?;
                return Ok(());
            }
            _ => {
                // Illegal in this phase, an unknown command, or a request this
                // slice does not yet implement.
                send(
                    writer,
                    &Response::error(take_seq(&mut seq), &request, REQUEST_NOT_APPLICABLE),
                )?;
            }
        }
    }
}

/// Parses the `launch` arguments, loads the container, and checks the launch
/// preconditions. Returns the loaded container, the parsed arguments, and the
/// validated scan bound on success, or the DAP error message to report on
/// failure.
///
/// The scan bound is validated here rather than in the session so an
/// unsatisfiable `scanLimit` fails the `launch` request itself, before the VM
/// starts and before any event is sent.
fn load_and_check(
    request: &Request,
) -> Result<(Container, LaunchRequestArguments, Option<NonZeroU64>), String> {
    // A value no argument's type can hold (a fractional `scanLimit`, say)
    // fails the whole parse and lands here too. The VS Code schema types
    // `scanLimit` as an integer, so the editor flags that before launch.
    let args: LaunchRequestArguments = request
        .arguments
        .as_ref()
        .and_then(|v| serde_json::from_value(v.clone()).ok())
        .ok_or_else(|| launch::LaunchError::ProgramArgMissing.to_string())?;

    let scan_limit = launch::check_scan_limit(args.scan_limit).map_err(|e| e.to_string())?;
    let container = launch::load_container(Path::new(&args.program)).map_err(|e| e.to_string())?;
    launch::check_preconditions(&container).map_err(|e| e.to_string())?;
    Ok((container, args, scan_limit))
}

/// Owns the loaded `container`, starts the VM, answers the `launch` request,
/// and runs the post-launch run/stop loop.
///
/// The `container` and the buffers sized from it live here so the [`VmRunning`]
/// can borrow them for the remainder of the session.
///
/// The loop alternates between two modes (see
/// `specs/design/debugger-support.md` §"Single-threaded DAP loop (v1)"): when
/// `Running`, it drives one `run_round_debug` and reacts to the outcome;
/// otherwise it reads and services one client request. Because the
/// [`BreakpointTable`] is mutated between rounds (a `setBreakpoints` at a
/// pause) while the [`DebuggerHook`] borrows it during a round, the hook is
/// built fresh per round; after a breakpoint pause the next hook is told to
/// suppress that location once so `continue` makes forward progress.
///
/// The loop keeps scanning: on `RoundOutcome::Completed` it runs the next scan
/// (so breakpoints re-fire every cycle and variables evolve across cycles)
/// rather than terminating after one scan. `scan_limit` bounds a runaway
/// program — the session terminates once `scan_count` reaches it, and `None`
/// (the client sent no `scanLimit`) means no bound — and `stopOnEntry` pauses
/// before the first instruction of the first scan.
///
/// Execution control is `continue`, single-stepping (`next`/`stepIn`/
/// `stepOut`), and scan stepping (`ironplc/stepScan`); a step is armed on the
/// next round's hook, seeded from the paused frames so it measures from the
/// real pause point. A scan step spans two rounds: the first runs the cycle out
/// to `RoundOutcome::PausedAfterScan`, the second stops at the first
/// instruction of the new scan so the stop has a frame to show.
///
/// Current limitations: trap→`exception` is not yet implemented. A free-running
/// program with no breakpoint and no `scanLimit` scans until the client
/// disconnects — the single-threaded loop cannot service an interactive `pause`
/// (a Phase 6 cut).
fn launched_session<R: BufRead, W: Write>(
    reader: &mut R,
    writer: &mut W,
    seq: &mut i64,
    mut container: Container,
    args: LaunchRequestArguments,
    scan_limit: Option<NonZeroU64>,
    launch_request: &Request,
) -> io::Result<()> {
    // How far program time moves per scan. `run_round_debug` bypasses the
    // scheduler, so this only feeds the uptime system variable -- but that is
    // what timers read, so it decides when a TON elapses. Resolving it before
    // the VM loads the container puts the rate in the task table rather than in
    // this driver, through the same call `run` makes.
    //
    // The fallbacks are this session's policy, not the shared rule: a container
    // naming no rate would otherwise freeze the clock and stop every timer
    // without saying why.
    let freewheeling = has_freewheeling_task(&container);
    let scan_advance = resolve_cycle_time(
        &mut container,
        Some(assumed_freewheeling_interval(args.freewheeling_interval_ms)),
    )
    .filter(|interval| !interval.is_zero())
    .unwrap_or(DEFAULT_FREEWHEELING_INTERVAL);

    let mut bufs = VmBuffers::from_container(&container);

    // Construct + start the VM. Buffer sizing (operand stack, variable table,
    // data region, and the frame stack from `header.max_call_depth`) is done by
    // `VmBuffers::from_container`, reused from the `ironplcvm` embedding path.
    let mut running = match launch::start_vm(&container, &mut bufs) {
        Ok(running) => running,
        Err(err) => {
            send(
                writer,
                &Response::error(take_seq(seq), launch_request, err.to_string()),
            )?;
            return Ok(());
        }
    };

    // Preconditions, buffer sizing, and start all succeeded.
    send(
        writer,
        &Response::success(take_seq(seq), launch_request, None),
    )?;

    let debug = container.debug_section.as_ref();
    let mut breakpoints = BreakpointTable::new();
    // The session opens in `Configuring`: the client sets breakpoints and then
    // sends `configurationDone` to begin the run.
    let mut phase = Phase::Configuring;
    let mut uptime_us: u64 = 0;
    if freewheeling {
        // Report the rate actually used, not the one requested: an out-of-range
        // or vanishingly small request lands on the fallback above.
        send(
            writer,
            &console_output(take_seq(seq), &freewheeling_notice(scan_advance)),
        )?;
    }
    // Set after a breakpoint pause so the next resume skips that one location
    // instead of re-triggering in place.
    let mut suppress_bp = false;
    // Armed once, before the first scan, when the launch requested `stopOnEntry`.
    let mut pending_stop_on_entry = args.stop_on_entry;
    // Set by a `next`/`stepIn`/`stepOut`/`ironplc/stepScan` request; armed on
    // the next round's hook.
    let mut pending_step: Option<StepMode> = None;
    // Set when a scan step ran its cycle out: the next round stops before its
    // first instruction, so the scan step lands at the start of the new scan
    // rather than at the frame-less boundary between the two.
    let mut pending_scan_landing = false;

    loop {
        if phase == Phase::Running {
            let outcome = {
                let mut hook = DebuggerHook::new(&breakpoints);
                if suppress_bp {
                    hook.suppress_next_breakpoint();
                    suppress_bp = false;
                }
                if pending_stop_on_entry {
                    hook.stop_on_entry();
                    pending_stop_on_entry = false;
                }
                if pending_scan_landing {
                    hook.land_scan_step();
                    pending_scan_landing = false;
                }
                if let Some(mode) = pending_step.take() {
                    // Seed the hook to the paused position so the step's origin
                    // is where the VM actually stopped, not scan entry. Frames
                    // are outermost-first; the entry frame is depth 0.
                    let frames = running.debug_frames();
                    let depth = frames.len().saturating_sub(1);
                    let offset = frames.last().map_or(0, |f| f.pc);
                    hook.seed_resume_position(depth, offset);
                    match mode {
                        StepMode::Over => hook.step_over(),
                        StepMode::In => hook.step_in(),
                        StepMode::Out => hook.step_out(),
                        StepMode::Scan => hook.step_scan(),
                        StepMode::None => {}
                    }
                }
                running.run_round_debug(uptime_us, &mut hook)
            };
            // Program time moves one cycle per *completed* scan. A round that
            // paused mid-scan ran no cycle, so advancing there would make a
            // timer depend on how the user stepped — the clock would jump a
            // cycle for every breakpoint hit, and the scan after a pause would
            // start late. `PausedAfterScan` did finish its cycle and stops at
            // the boundary, so it counts.
            if matches!(
                outcome,
                Ok(RoundOutcome::Completed | RoundOutcome::PausedAfterScan)
            ) {
                uptime_us = uptime_us.saturating_add(interval_us(scan_advance));
            }

            match outcome {
                // A completed scan keeps the debugger scanning: run the next
                // cycle unless a `scanLimit` bound has been reached.
                Ok(RoundOutcome::Completed) => {
                    if scan_limit_reached(scan_limit, &running) {
                        send(writer, &Event::new(take_seq(seq), "terminated", None))?;
                        phase = Phase::Terminated;
                    }
                    // Otherwise stay in `Running`: the loop drives the next scan.
                }
                // A scan step ran its cycle out. The boundary itself has no
                // frames to inspect, so run one more round with the landing
                // armed and stop at the first instruction of the new scan --
                // unless the finished cycle reached the `scanLimit`, in which
                // case there is no next scan to land in.
                Ok(RoundOutcome::PausedAfterScan) => {
                    if scan_limit_reached(scan_limit, &running) {
                        send(writer, &Event::new(take_seq(seq), "terminated", None))?;
                        phase = Phase::Terminated;
                    } else {
                        pending_scan_landing = true;
                    }
                }
                Ok(RoundOutcome::Paused(reason)) => {
                    let dap_reason = match reason {
                        PauseReason::Breakpoint(_) => {
                            // Skip this location on the following resume.
                            suppress_bp = true;
                            "breakpoint"
                        }
                        // A step landing, whether from `next`/`stepIn`/
                        // `stepOut` or from a scan step's landing round.
                        PauseReason::Step => {
                            suppress_bp = true;
                            "step"
                        }
                        PauseReason::Entry => "entry",
                    };
                    send(writer, &stopped_event(take_seq(seq), dap_reason))?;
                    phase = Phase::Paused;
                }
                // Trap-stop is not yet implemented; for now a trap ends the
                // session like a normal termination rather than surfacing an
                // `exception` stop.
                Err(_fault) => {
                    send(writer, &Event::new(take_seq(seq), "terminated", None))?;
                    phase = Phase::Terminated;
                }
            }
            continue;
        }

        // Stopped mode: read and service one request.
        let Some(body) = framing::read_message(reader)? else {
            return Ok(());
        };
        let Ok(request) = serde_json::from_slice::<Request>(&body) else {
            continue;
        };

        let command = Command::from_request(&request.command);
        let legal_here = command.is_some_and(|c| state::legal(phase, c));

        match command {
            Some(Command::Disconnect) => {
                send(writer, &Response::success(take_seq(seq), &request, None))?;
                return Ok(());
            }
            Some(Command::ConfigurationDone) if legal_here => {
                send(writer, &Response::success(take_seq(seq), &request, None))?;
                phase = Phase::Running;
            }
            Some(Command::SetBreakpoints) if legal_here => {
                let body = set_breakpoints(&request, debug, &mut breakpoints);
                send(writer, &Response::success(take_seq(seq), &request, body))?;
            }
            Some(Command::Threads) if legal_here => {
                let body = serde_json::to_value(ThreadsResponseBody {
                    threads: vec![Thread {
                        id: THREAD_ID,
                        name: "plc".to_string(),
                    }],
                })
                .ok();
                send(writer, &Response::success(take_seq(seq), &request, body))?;
            }
            Some(Command::StackTrace) if legal_here => {
                let body = stack_trace_body(&running, debug);
                send(writer, &Response::success(take_seq(seq), &request, body))?;
            }
            Some(Command::Scopes) if legal_here => {
                // Two scopes: the program's own variables, and VM-level runtime
                // state. The client re-requests both at every stop, so the
                // runtime values track execution without any polling.
                let body = serde_json::to_value(ScopesResponseBody {
                    scopes: vec![
                        Scope {
                            // Not "Variables": DAP clients render scopes inside
                            // a pane already titled Variables, so that name
                            // reads as Variables > Variables. The scope names
                            // what kind of state it holds -- the program's, as
                            // against the VM's.
                            name: "Program".to_string(),
                            variables_reference: PROGRAM_REF,
                            expensive: false,
                        },
                        Scope {
                            name: "Runtime".to_string(),
                            variables_reference: RUNTIME_REF,
                            expensive: false,
                        },
                    ],
                })
                .ok();
                send(writer, &Response::success(take_seq(seq), &request, body))?;
            }
            Some(Command::Variables) if legal_here => {
                // Dispatch on the handle the client asked for. Before the
                // `Runtime` scope existed this argument was ignored and the
                // program variables were returned for *any* reference; a
                // reference we never handed out now yields an empty list
                // rather than a plausible-looking wrong answer.
                let reference = request
                    .arguments
                    .as_ref()
                    .and_then(|v| serde_json::from_value::<VariablesArguments>(v.clone()).ok())
                    .map_or(PROGRAM_REF, |a| a.variables_reference);
                let body = match reference {
                    PROGRAM_REF => program_variables_body(&running, debug),
                    RUNTIME_REF => runtime_variables_body(&running),
                    _ => serde_json::to_value(VariablesResponseBody { variables: vec![] }).ok(),
                };
                send(writer, &Response::success(take_seq(seq), &request, body))?;
            }
            Some(Command::Continue) if legal_here => {
                let body = serde_json::to_value(ContinueResponseBody {
                    all_threads_continued: true,
                })
                .ok();
                send(writer, &Response::success(take_seq(seq), &request, body))?;
                phase = Phase::Running;
            }
            Some(Command::Next) if legal_here => {
                send(writer, &Response::success(take_seq(seq), &request, None))?;
                pending_step = Some(StepMode::Over);
                phase = Phase::Running;
            }
            Some(Command::StepIn) if legal_here => {
                send(writer, &Response::success(take_seq(seq), &request, None))?;
                pending_step = Some(StepMode::In);
                phase = Phase::Running;
            }
            Some(Command::StepOut) if legal_here => {
                send(writer, &Response::success(take_seq(seq), &request, None))?;
                pending_step = Some(StepMode::Out);
                phase = Phase::Running;
            }
            Some(Command::StepScan) if legal_here => {
                send(writer, &Response::success(take_seq(seq), &request, None))?;
                pending_step = Some(StepMode::Scan);
                phase = Phase::Running;
            }
            _ => {
                // Illegal in this phase or an unknown command.
                send(
                    writer,
                    &Response::error(take_seq(seq), &request, REQUEST_NOT_APPLICABLE),
                )?;
            }
        }
    }
}

/// The cycle time to assume for a freewheeling program.
///
/// An out-of-range `freewheelingIntervalMs` falls back to the default rather
/// than failing the launch — a debug session that will not start is a poor
/// answer to a typo in a launch configuration — and the notice reports the
/// value actually used either way.
fn assumed_freewheeling_interval(interval_ms: Option<f64>) -> Duration {
    interval_ms
        .and_then(|ms| interval_from_ms(ms).ok())
        .unwrap_or(DEFAULT_FREEWHEELING_INTERVAL)
}

/// The console line telling the user which cycle time the session assumed.
///
/// Every timer in a freewheeling program elapses on this assumption, so a
/// session that did not state it would leave the user unable to explain what
/// they are watching. It points at the launch configuration rather than at a
/// named setting, which would go stale the moment the setting is renamed.
fn freewheeling_notice(interval: Duration) -> String {
    format!(
        "This program declares no INTERVAL, so it has no scan cycle time of its own. \
         Assuming {} ms per scan; change it in the launch configuration.\n",
        interval.as_secs_f64() * 1_000.0
    )
}

/// A DAP `output` event carrying one line to the debug console.
fn console_output(seq: i64, text: &str) -> Event {
    Event::new(
        seq,
        "output",
        Some(json!({ "category": "console", "output": text })),
    )
}

/// Whether the launch's `scanLimit` bound (if any) has been reached, so the
/// session should terminate rather than start another cycle.
fn scan_limit_reached(scan_limit: Option<NonZeroU64>, running: &VmRunning) -> bool {
    scan_limit.is_some_and(|limit| running.scan_count() >= limit.get())
}

/// Builds the `stopped` event for `reason`, scoped to the single thread.
fn stopped_event(seq: i64, reason: &'static str) -> Event {
    let body = serde_json::to_value(StoppedEventBody {
        reason,
        thread_id: Some(THREAD_ID),
        description: None,
        all_threads_stopped: true,
    })
    .ok();
    Event::new(seq, "stopped", body)
}

/// Applies a `setBreakpoints` request: replaces the breakpoint set and returns
/// the response body echoing each requested breakpoint's resolved state.
///
/// DAP `setBreakpoints` carries the full set for one source, so the table is
/// cleared and rebuilt. v1 debugs a single source, so a table-wide clear is
/// correct. Line→location resolution is delegated to [`debug_info`], which
/// snaps each breakpoint forward to the nearest executable line; the response
/// echoes the *bound* line so the editor moves the marker to where the
/// breakpoint actually took effect.
fn set_breakpoints(
    request: &Request,
    debug: Option<&DebugSection>,
    breakpoints: &mut BreakpointTable,
) -> Option<Value> {
    let args: SetBreakpointsArguments = request
        .arguments
        .as_ref()
        .and_then(|v| serde_json::from_value(v.clone()).ok())?;

    breakpoints.clear();
    let source_path = args.source.path.clone().unwrap_or_default();
    let source = Some(args.source.clone());

    let resolved: Vec<Breakpoint> = args
        .breakpoints
        .iter()
        .map(|bp| {
            // `bp.line` is already narrowed to the container's `SourceLine` by
            // the serde layer; `None` means the client sent a line the debug
            // section cannot represent, which resolves to nothing.
            let resolved = bp
                .line
                .and_then(|line| debug_info::resolve_breakpoint(debug, &source_path, line));
            match resolved {
                Some(resolved) => {
                    for (function_id, offset) in resolved.locations {
                        breakpoints.add(function_id, offset);
                    }
                    Breakpoint {
                        verified: true,
                        line: Some(resolved.line),
                        source: source.clone(),
                        message: None,
                    }
                }
                None => Breakpoint {
                    verified: false,
                    line: bp.line,
                    source: source.clone(),
                    message: Some("no executable location for this line".to_string()),
                },
            }
        })
        .collect();

    serde_json::to_value(SetBreakpointsResponseBody {
        breakpoints: resolved,
    })
    .ok()
}

/// Builds the `stackTrace` response body from the paused instance's live
/// frames. DAP orders frames innermost-first; [`VmRunning::debug_frames`] is
/// outermost-first, so the walk is reversed. Each frame is resolved by
/// [`debug_info`] to its POU name and source location (FUNC_NAME + line map).
fn stack_trace_body(running: &VmRunning, debug: Option<&DebugSection>) -> Option<Value> {
    let frames = running.debug_frames();
    let stack_frames: Vec<StackFrame> = frames
        .iter()
        .enumerate()
        .rev()
        .map(|(index, frame)| {
            let info = debug_info::resolve_frame(debug, frame.function_id, frame.pc);
            StackFrame {
                id: index as i64,
                name: info.name,
                line: info.line,
                column: info.column,
                source: info.source.map(|(name, path)| Source {
                    name: Some(name),
                    path: Some(path),
                }),
            }
        })
        .collect();
    let total = stack_frames.len() as i64;
    serde_json::to_value(StackTraceResponseBody {
        stack_frames,
        total_frames: Some(total),
    })
    .ok()
}

/// Builds the `Program` scope's contents: every program variable slot, rendered
/// by [`debug_info`] with its VAR_NAME name/type and a value formatted per its
/// IEC type tag (STRING values are read from the data region).
///
/// The list is unfiltered — locals and globals together — which is why the scope
/// is named `Program` rather than `Locals`. Splitting it by `var_section` into
/// Locals / Inputs / Outputs / In-Out / Globals is the design's end state; the
/// data for it is already in `VarNameEntry::var_section`.
fn program_variables_body(running: &VmRunning, debug: Option<&DebugSection>) -> Option<Value> {
    let count = running.num_variables();
    let values: Vec<u64> = (0..count)
        .map(|i| running.read_variable_raw(VarIndex::new(i)).unwrap_or(0))
        .collect();
    let variables = debug_info::render_variables(debug, &values, running.data_region());
    serde_json::to_value(VariablesResponseBody { variables }).ok()
}

/// Builds the `Runtime` scope's contents: VM-level state that is not a program
/// variable — the completed-scan-cycle count and the VM's monotonic uptime,
/// which the client re-reads at every stop, so they are simply on screen rather
/// than behind a "show me" button. Cycle timing and next-due can join this list
/// without adding another scope.
fn runtime_variables_body(running: &VmRunning) -> Option<Value> {
    let variables = vec![
        Variable {
            name: "scanCount".to_string(),
            value: running.scan_count().to_string(),
            // The VM counter is a u64; ULINT is its IEC 61131-3 spelling.
            type_name: Some("ULINT".to_string()),
            variables_reference: 0,
        },
        Variable {
            name: "systemUptime".to_string(),
            // How long the VM has run as of the current scan cycle, rendered in
            // milliseconds. The VM tracks it whether or not the program
            // declares the uptime globals, so this shows time even for a
            // program compiled without `--allow-system-uptime-global`.
            value: (running.uptime().as_millis() as i64).to_string(),
            // The same i64 milliseconds `__SYSTEM_UP_LTIME` holds; LINT is its
            // IEC 61131-3 spelling.
            type_name: Some("LINT".to_string()),
            variables_reference: 0,
        },
    ];
    serde_json::to_value(VariablesResponseBody { variables }).ok()
}

#[cfg(test)]
mod tests;
