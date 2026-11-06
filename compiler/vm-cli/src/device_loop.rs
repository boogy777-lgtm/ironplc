//! The device loop of `ironplcvm serve`: the one owner of the runtime host
//! that scans by itself and executes the commands of the session between
//! rounds (ADR-0086).
//!
//! The loop owns the host, the HA shell, the slot store and the device
//! settings ([`Owned`]). Session threads (stdio, TCP) never touch them: a
//! session thread reads a command line, hands it to the loop over a channel
//! together with a reply channel ([`LoopHandle::call`]), and writes the answer
//! when it comes. So exactly one thread touches the host, no lock is held
//! across a round, and the swap of a hot edit stays at the round boundary.
//!
//! One iteration of the loop ([`DeviceLoop::step`]):
//!
//! 1. execute every command that is waiting, each through the dispatch the HA
//!    pair path shares ([`execute_line`](crate::serve::execute_line)); an
//!    answer that depends on a scan boundary is parked;
//! 2. run one round when one is due and the device may scan, or when a parked
//!    answer needs its boundary (the next round is the boundary: a swap
//!    request does not wait for the period);
//! 3. answer the parked commands, which have now seen their boundary: the ack
//!    of `testEdits`, `untestEdits` and `assembleEdits` means applied;
//! 4. end when the stop flag is set or every session sender is gone;
//! 5. wait for the next deadline of a cyclic task, or for the next command,
//!    whichever comes first (the pacer's policy, see [`crate::pacer`]).
//!
//! A program without a cyclic task (freewheeling) has no deadline to wait for,
//! so its next round starts at once; the channel is still looked at once per
//! round, so commands are not starved.
//!
//! A round that traps stops the scan: the device is faulted, runs no further
//! round, keeps answering every command, and reports the fault in `identity`
//! (see [`crate::device_settings::device::Device::note_fault`]).
//!
//! The operator can stop the scan too (`stopScan`): the run state is the
//! device's, apart from the fault. A stopped device runs no round, answers every
//! command, and applies a requested swap at the boundary without executing the
//! logic, as a faulted device does. When the scan resumes after any gap (a
//! start, a permit that returns), the loop restarts the deadlines of the cyclic
//! tasks from that moment ([`ironplc_runtime::RuntimeHost::rearm_cyclic_tasks`])
//! so the gap is neither a burst of rounds nor a counted overrun.
//!
//! [`Core`] is the part of the loop that has no clock and no channel: execute a
//! line, run a boundary round at a given uptime, finish a command. The
//! scripted session of the tests ([`scripted`]) drives it directly, so the
//! command tests decide when rounds run and still run the code of the loop.

use std::io::{self, BufRead, Write};
use std::sync::mpsc::{self, Receiver, RecvTimeoutError, Sender, TryRecvError};

use ironplc_redundancy::{NicPort, PairLink, Shell, UdpPort};
use ironplc_runtime::{FaultNote, RuntimeHost};

use crate::device_settings::device::Device;
use crate::error::VmError;
use crate::ha_pair::{self, PUMP_PERIOD_US};
use crate::pacer::{is_due, Clock, Pacer, SystemClock};
use crate::serve::{
    execute_line, finish_line, paced_round, stop_device, Executed, Executing, Ha, RoundOutcome,
};
use crate::slot_store::SlotStore;

#[cfg(test)]
pub(crate) mod scripted;
#[cfg(test)]
mod tests;

/// The answer to a command line, or the I/O failure that ends its session.
type Answer = io::Result<String>;

/// A command line and where its answer goes.
pub(crate) struct Envelope {
    line: String,
    reply: Sender<Answer>,
}

/// The end of the sending side of a device loop: what a session thread holds.
/// Cloned freely; every transport sends to the same loop.
#[derive(Clone)]
pub(crate) struct LoopHandle {
    commands: Sender<Envelope>,
}

impl LoopHandle {
    /// Sends one command line to the loop and waits for its answer: the single
    /// response line. A swap request is answered after the boundary round that
    /// applied it, so the wait is at most one round. Fails when the loop has
    /// stopped.
    pub(crate) fn call(&self, line: String) -> Answer {
        self.send(line)?.recv().map_err(|_| stopped())?
    }

    /// Sends one command line to the loop without waiting; the answer arrives
    /// on the returned receiver. Fails when the loop has stopped.
    pub(crate) fn send(&self, line: String) -> io::Result<Receiver<Answer>> {
        let (reply, answer) = mpsc::channel();
        self.commands
            .send(Envelope { line, reply })
            .map_err(|_| stopped())?;
        Ok(answer)
    }
}

fn stopped() -> io::Error {
    io::Error::new(io::ErrorKind::BrokenPipe, "the device has stopped")
}

/// A channel to a device loop: the handle sessions send through and the
/// receiving end the loop is built with.
pub(crate) fn channel() -> (LoopHandle, Receiver<Envelope>) {
    let (commands, receiver) = mpsc::channel();
    (LoopHandle { commands }, receiver)
}

/// Serves one command session through the loop: reads one line per command
/// from `reader`, sends it to the loop, writes the answer as one line to
/// `writer` and flushes, one line in and one line out. The session of every
/// transport: stdio reads lines, TCP reads frames, both write the answer with
/// the same call.
///
/// Returns when the reader reaches EOF, or with the first I/O failure, or when
/// the loop has stopped.
pub(crate) fn relay_session(
    handle: &LoopHandle,
    reader: impl BufRead,
    mut writer: impl Write,
) -> io::Result<()> {
    for line in reader.lines() {
        let answer = handle.call(line?)?;
        writeln!(writer, "{answer}")?;
        writer.flush()?;
    }
    Ok(())
}

/// The HA backend the loop owns: the shell of the standalone and the simulated
/// pair, or the link of a real pair. The pair is a backend like the shell, not a
/// second driver: the loop ticks it, reconciles the permit from its verdict, and
/// runs the rounds.
pub(crate) enum Backend<P: NicPort> {
    /// The honest standalone shell or the simulated pair.
    Shell(Box<Shell>),
    /// The link of one unit of a real two-process pair.
    Pair(PairLink<P>),
}

/// What the loop owns: the host and everything that is touched together with
/// it. Only the loop thread holds this.
struct Owned<P: NicPort> {
    host: RuntimeHost,
    store: Option<SlotStore>,
    device: Device,
    backend: Backend<P>,
}

impl<P: NicPort> Owned<P> {
    fn core(&mut self) -> Core<'_, P> {
        Core {
            host: &mut self.host,
            store: self.store.as_mut(),
            device: &mut self.device,
            ha: Some(match &mut self.backend {
                Backend::Shell(shell) => Ha::Shell(shell.as_mut()),
                Backend::Pair(link) => Ha::Pair(link),
            }),
        }
    }
}

/// What executing a line came to: its answer, or the boundary it waits for.
pub(crate) enum Submitted {
    /// The line is answered.
    Answered(String),
    /// The command was executed and requested a swap; its answer is written by
    /// [`Core::finish`] after a round.
    AwaitsBoundary(Executing),
}

/// The loop without a clock and a channel: the host and its companions by
/// reference, and the three things a loop does with them.
pub(crate) struct Core<'a, P: NicPort = UdpPort> {
    pub(crate) host: &'a mut RuntimeHost,
    pub(crate) store: Option<&'a mut SlotStore>,
    pub(crate) device: &'a mut Device,
    pub(crate) ha: Option<Ha<'a, P>>,
}

impl<P: NicPort> Core<'_, P> {
    /// Whether the device may run a scheduled round: it holds the execution
    /// permit, its scan has not trapped and it is not stopped. The one place
    /// that decides it.
    pub(crate) fn can_scan(&self) -> bool {
        self.host.execution_permitted() && self.device.scans()
    }

    /// Executes one command line. The simulated pair ticks first: the command
    /// cadence is its clock, one tick per line whatever the command.
    pub(crate) fn submit(&mut self, line: &str) -> io::Result<Submitted> {
        if let Some(Ha::Shell(shell)) = self.ha.as_mut() {
            if shell.has_pair() {
                shell.tick();
            }
        }
        match execute_line(self.host, self.device, self.ha.as_mut(), line)? {
            Executed::Answer(answer) => Ok(Submitted::Answered(answer)),
            Executed::Command(executing) if executing.awaits_boundary() => {
                Ok(Submitted::AwaitsBoundary(executing))
            }
            Executed::Command(executing) => self.finish(executing).map(Submitted::Answered),
        }
    }

    /// Runs one scan boundary at `uptime_us`: one round, then the periodic write
    /// of the retained values. A device that is faulted or stopped scans no
    /// more; a swap that was requested still reaches its boundary without
    /// executing the logic, so an acknowledgment still means applied. A trap
    /// faults the device.
    pub(crate) fn round(&mut self, uptime_us: u64) {
        if !self.device.scans() {
            if let Err(err) = self.host.apply_pending_at_boundary() {
                log::error!("the swap of a device that does not scan was not applied: {err}");
            }
            return;
        }
        match paced_round(self.host, self.ha.as_mut(), uptime_us) {
            RoundOutcome::Completed => self.device.scan_boundary(self.host),
            RoundOutcome::Refused | RoundOutcome::Invariant => {}
            RoundOutcome::Trapped(context) => {
                let err = VmError::from_trap(&context.trap, context.task_id, context.instance_id);
                log::error!("scan round trapped, the device is faulted: {err}");
                self.device.note_fault(FaultNote {
                    v_code: err.v_code().to_string(),
                    message: err.message().to_string(),
                    task_id: context.task_id.raw(),
                    instance_id: context.instance_id.raw(),
                });
            }
        }
    }

    /// Writes the response line of an executed command, after its boundary.
    pub(crate) fn finish(&mut self, executing: Executing) -> io::Result<String> {
        finish_line(
            self.host,
            self.store.as_deref_mut(),
            self.device,
            self.ha.as_mut(),
            executing,
        )
    }

    /// Whether the composition has a pair link for the loop to pump.
    pub(crate) fn is_paired(&self) -> bool {
        matches!(self.ha, Some(Ha::Pair(_)))
    }

    /// Ticks the pair link once, if the composition has one, and acts on what
    /// the tick reports; see [`ha_pair::pump`]. The loop calls it at the pump
    /// cadence, never more often: the link's liveness counts exchanges, so a
    /// link ticked faster than its peer would count the peer dead.
    pub(crate) fn pump_pair(&mut self) {
        if let Some(Ha::Pair(link)) = self.ha.as_mut() {
            ha_pair::pump(link, self.host, self.store.as_deref_mut(), self.device);
        }
    }
}

/// Why the loop ended.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub(crate) enum End {
    /// Every session sender is gone: the input of the process ended.
    Closed,
    /// The stop flag was set (Ctrl+C).
    Stopped,
}

/// The device loop; see the module documentation.
pub(crate) struct DeviceLoop<C: Clock = SystemClock, P: NicPort = UdpPort> {
    owned: Owned<P>,
    pacer: Pacer<C>,
    commands: Receiver<Envelope>,
    /// A command that arrived while the loop waited for a deadline; executed
    /// first in the next iteration.
    arrived: Option<Envelope>,
    /// Commands whose answer waits for a boundary, with where it goes.
    parked: Vec<(Executing, Sender<Answer>)>,
    /// Every sender is gone.
    closed: bool,
    /// Whether the previous iteration could scan; a change to `true` is a resume.
    scanning: bool,
    /// The uptime at which the pair link is ticked next; only a loop with a pair
    /// link looks at it.
    next_pump_us: u64,
}

impl<C: Clock> DeviceLoop<C> {
    /// A loop over the composed device with the shell of the standalone or the
    /// simulated pair. `commands` is the receiving end of [`channel`]; the loop
    /// ends with [`End::Closed`] when every [`LoopHandle`] has been dropped.
    pub(crate) fn new(
        host: RuntimeHost,
        store: Option<SlotStore>,
        device: Device,
        shell: Shell,
        pacer: Pacer<C>,
        commands: Receiver<Envelope>,
    ) -> Self {
        Self::with_backend(
            host,
            store,
            device,
            Backend::Shell(Box::new(shell)),
            pacer,
            commands,
        )
    }
}

impl<C: Clock, P: NicPort> DeviceLoop<C, P> {
    /// A loop over the composed device with the link of one unit of a real
    /// pair: the loop ticks the link at the pump cadence ([`PUMP_PERIOD_US`]).
    pub(crate) fn paired(
        host: RuntimeHost,
        store: SlotStore,
        device: Device,
        link: PairLink<P>,
        pacer: Pacer<C>,
        commands: Receiver<Envelope>,
    ) -> Self {
        Self::with_backend(
            host,
            Some(store),
            device,
            Backend::Pair(link),
            pacer,
            commands,
        )
    }

    fn with_backend(
        host: RuntimeHost,
        store: Option<SlotStore>,
        device: Device,
        backend: Backend<P>,
        pacer: Pacer<C>,
        commands: Receiver<Envelope>,
    ) -> Self {
        Self {
            owned: Owned {
                host,
                store,
                device,
                backend,
            },
            pacer,
            commands,
            arrived: None,
            parked: Vec::new(),
            closed: false,
            scanning: false,
            next_pump_us: 0,
        }
    }

    /// Runs iterations until the loop ends.
    pub(crate) fn run(&mut self) -> End {
        loop {
            if let Some(end) = self.step() {
                return end;
            }
        }
    }

    /// One iteration; `Some` when the loop ended.
    pub(crate) fn step(&mut self) -> Option<End> {
        if let Some(envelope) = self.arrived.take() {
            self.execute(envelope);
        }
        loop {
            match self.commands.try_recv() {
                Ok(envelope) => self.execute(envelope),
                Err(TryRecvError::Empty) => break,
                Err(TryRecvError::Disconnected) => {
                    self.closed = true;
                    break;
                }
            }
        }

        let now_us = self.pacer.uptime_us();
        let mut core = self.owned.core();
        if core.is_paired() && now_us >= self.next_pump_us {
            // The pump of the pair link, before the permit is read: a verdict
            // the tick decides (admission, a promotion) is this iteration's, so
            // the takeover round is the round of this iteration.
            core.pump_pair();
            self.next_pump_us = now_us + PUMP_PERIOD_US;
        }
        let may_scan = core.can_scan();
        if may_scan && !self.scanning {
            // The first iteration, a start, a returned permit: no round ran
            // for a while, so no deadline of a cyclic task is owed.
            core.host.rearm_cyclic_tasks(now_us);
        }
        self.scanning = may_scan;
        let due = may_scan && is_due(now_us, core.host.next_due_us());
        if due || !self.parked.is_empty() {
            core.round(now_us);
        }
        for (executing, reply) in self.parked.drain(..) {
            send(&reply, core.finish(executing));
        }

        if self.closed {
            return Some(End::Closed);
        }
        if self.pacer.should_stop(0) {
            return Some(End::Stopped);
        }

        // The wait ends at the next deadline of a cyclic task or at the next tick
        // of the pair link, whichever comes first; a freewheeling program does
        // not wait for either, a device that does not scan waits for the tick.
        let pump_us = core.is_paired().then_some(self.next_pump_us);
        let waited = if core.can_scan() {
            let wake_us = match (core.host.next_due_us(), pump_us) {
                (Some(due_us), Some(pump_us)) => Some(due_us.min(pump_us)),
                (due_us, _) => due_us,
            };
            self.pacer.wait_for_message(wake_us, &self.commands)
        } else {
            self.pacer.wait_idle(pump_us, &self.commands)
        };
        match waited {
            Ok(envelope) => self.arrived = Some(envelope),
            Err(RecvTimeoutError::Timeout) => {}
            Err(RecvTimeoutError::Disconnected) => self.closed = true,
        }
        None
    }

    fn execute(&mut self, envelope: Envelope) {
        let Envelope { line, reply } = envelope;
        match self.owned.core().submit(&line) {
            Ok(Submitted::Answered(answer)) => send(&reply, Ok(answer)),
            Ok(Submitted::AwaitsBoundary(executing)) => self.parked.push((executing, reply)),
            Err(err) => send(&reply, Err(err)),
        }
    }

    /// The controlled stop of the process: writes the retained values and
    /// returns the outcome of the session, see [`stop_device`].
    pub(crate) fn stop(mut self, session: Result<(), VmError>) -> Result<(), VmError> {
        stop_device(&mut self.owned.device, &self.owned.host, session)
    }

    /// The owned things, for the tests that look at the device after steps.
    #[cfg(test)]
    pub(crate) fn core(&mut self) -> Core<'_, P> {
        self.owned.core()
    }
}

/// Sends an answer to the session that asked. A session that is gone (it
/// closed while its command waited) has nobody to answer.
fn send(reply: &Sender<Answer>, answer: Answer) {
    if reply.send(answer).is_err() {
        log::debug!("the session of a command is gone, its answer is dropped");
    }
}
