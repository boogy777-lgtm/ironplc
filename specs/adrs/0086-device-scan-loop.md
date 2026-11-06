# One Device Loop Owns the Host and Scans by Itself

status: accepted
date: 2026-11-06
amended: 2026-11-06 (added Run and Stop, stage 3c)

## Context and Problem Statement

`ironplcvm serve` composes the runtime host, the redundancy shell, the slot store
and the device settings, and answers one command line at a time. It executes a
scan round only when a command asks for one: `drive_scan_round`
(`compiler/vm-cli/src/serve.rs`) runs after `testEdits`, `untestEdits` and
`assembleEdits`, at a constant zero uptime, so that a scripted client reads back
state that has actually switched. Between commands the application does not run.
A device that the engineering session has connected to is therefore a program
that executes only while someone is talking to it, and the timing of its tasks
(`run` paces them against a clock; `serve` does not) is never exercised in the
process that holds the application.

The device must scan continuously, as `ironplcvm run` does, and still answer
session commands between rounds. Two things stand in the way:

- The host is single-threaded state. The stdio session owns it for the length of
  the process; the TCP transport shares it between the accept loop's session
  threads through four `Arc<Mutex<..>>` (host, shell, device, slot store) that a
  session thread holds for the length of its session
  (`serve_accepted` in `compiler/vm-cli/src/tcp.rs`). A scan thread would need
  the same locks, and a session that holds them for its whole length would stop
  the scan for as long as it is connected.
- The retained values (ADR-0085) are written at a scan boundary when the period
  setting is on, and at a controlled stop. TCP has no controlled stop, so the
  device it serves ends without writing them.

The scan round is a free-running driver of the VM: `run` is one, the device loop
becomes the second. The benchmark (rounds as fast as possible), the MCP runner
(simulated time) and the debug adapter (steps on request) are not drivers of a
free-running scan and stay as they are.

## Decision Drivers

- **One owner.** Exactly one thread touches the host, so no lock is held across a
  round and no two threads can disagree about the state of the application.
- **The ack means applied.** A client that reads `ack` after `testEdits` must find
  the candidate running (ADR-0064); the command that requests a swap is answered
  after the boundary that applied it.
- **A command never waits for more than one round.** A device with a one second
  task period answers `identity` at once, not on the next deadline.
- **Pacing in one place.** The deadline and stop policy is the pacer's
  (`compiler/vm-cli/src/pacer.rs`); `run` and the device loop both call it.
- **No second copy of the command logic.** The loop executes a line through the
  dispatch the pair mode already shares; it adds no command of its own.
- **A new transport adds nothing here.** A session over USB sends commands to the
  same loop and changes none of it.
- **Tests decide when rounds run.** The command tests of `serve` assert counters;
  a free-running scan must not make them depend on timing.

## Considered Options

1. **Keep command-driven rounds.**
2. **A mutex-shared host and a scan thread.**
3. **One owner thread with a command channel (chosen).**

## Decision Outcome

Chosen option: **a device loop that owns the host, the shell, the slot store and
the device settings; session threads send it a command line with a reply channel.**

1. **The loop.** `DeviceLoop` (`compiler/vm-cli/src/device_loop.rs`) runs on the
   thread of the served process. It owns the `RuntimeHost`, the HA shell (the
   honest standalone shell or the simulated pair), the `SlotStore` and the
   `Device`. Session threads (stdio, TCP) read a line and send `(line, reply)`
   over an `mpsc` channel, then wait for the reply and write it. The loop is the
   only thread that touches the host. The mutexes of `tcp.rs` are gone.
2. **One iteration.** (a) Execute every command that is waiting, each through
   `execute_line` of `serve.rs` (the part of the old `handle_line` before the
   boundary), so no command logic is copied. (b) Run one round when a round is
   due, the host is permitted and the device is not faulted, or when a command
   needs a boundary. (c) After a round, `Device::scan_boundary` (the periodic
   write of the retained values). (d) Answer the commands that waited for the
   boundary. (e) Wait for the next deadline, or for the next command, whichever
   comes first. The loop never waits when no cyclic task exists (a freewheeling
   program): it checks the channel once per round and runs the next round at once.
3. **A command is never behind a period.** The wait of (e) is a receive with a
   timeout, so a command that arrives in the middle of a period wakes the loop,
   is executed, and is answered. The wait is also capped (the stop poll, 50 ms)
   so that the stop flag is seen within that time whatever the period is.
4. **The ack means applied.** `testEdits`, `untestEdits` and `assembleEdits` only
   record a request (REQ-VC-vm-cli-023). The loop parks the answer, runs a round
   at once instead of at the next deadline (a swap rebuilds the task state, so
   every task of the candidate runs in that round), and finishes the command after
   that boundary: the slot store and the retained file for `assembleEdits`
   (V6012, V6018), and the pair notice. The line written to the client is the
   same as before. `drive_scan_round` is no longer part of the stdio and TCP
   paths: the next round is the boundary.
5. **Pacing is the pacer's.** The pacer gains the three questions the loop asks
   and `run` does not: whether a round is due (`is_due`), a wait that a message
   ends (`wait_for_message`, `wait_idle`), and the stop poll. The `Clock` trait
   gains the receive with a deadline, which the wall clock implements with
   `recv_timeout` and the test clock with a jump of its time. `run` stays on `Vm`
   and keeps calling `wait_for` (owner answer 3 of the online-model plan): only
   the pacer is shared. The loop reads the clock once per iteration and uses
   that time both for the due check and as the uptime of the round, as `run` does.
6. **The deadlines survive a round.** `RuntimeHost::run` loads the VM for each
   call, and a load starts every task ready (`Vm::load` resets the task states),
   which is what a driven round at a constant clock needs and what a paced
   driver cannot have: a cyclic task would run in every round it is asked for,
   count a spurious overrun, and drift by the wake-up latency of each period. The
   host therefore gets `run_paced_round`, one round at a given uptime on
   `Vm::load_continuing`, which leaves the task states as the previous round set
   them, and `next_due_us`, the deadline a paced driver waits for (shared with
   the running VM through `earliest_due_us`). `run` keeps its behaviour. A swap
   still rebuilds the task state, so the tasks of the swapped-in artifact run in
   the round that applies it.
7. **A trap is a fault.** A round that traps stops the scan. The loop records the
   fault (the V-code, the message, the task and the instance), runs no further
   round, and keeps answering every command. The fault is reported in the
   `identity` answer as the optional field `fault`; the protocol version stays 2.
   A swap request on a faulted device still reaches its boundary (the host
   applies it without scanning), so `ack` still means applied. A fault is left by
   a restart of the process; Run and Stop (stage 3c) own any other way out.
8. **A controlled stop for the process.** Ctrl+C sets the stop flag of the pacer
   (the `ctrlc` handler that `run` installs); the loop ends and the device writes
   the retained values (`stop_device`). The end of stdin ends the loop the same
   way. TCP had no controlled stop: ending the process did not write the retained
   values. It has one now, and REQ-VC-vm-cli-038 no longer carves it out.
9. **Exclusivity is unchanged.** The accept loop of `serve_tcp` still claims the
   occupied flag, still refuses a second connection with V6014, and still closes
   gracefully (ADR-0065). It moves to its own thread, because the main thread now
   runs the loop. What changes is only what a session thread does: it relays
   lines to the loop instead of locking four mutexes.
10. **Tests decide when rounds run.** The loop is split into `Core`, which holds
    the four owned things by reference and has no clock (execute a line, run a
    boundary round at a given uptime, finish a command), and `DeviceLoop`, which
    adds the channel and the pacer. The scripted session of the tests drives
    `Core` the way the old session loop did (a round after each swap request, at
    a constant zero uptime), so the existing command tests assert the same
    counters on the same code the loop runs. The loop tests drive `DeviceLoop` one
    `step` at a time on a fake clock.

Not part of this decision: the HA pair path (`serve_pair`, `ha_pair.rs`) keeps its
own pump and keeps `drive_scan_round`, now documented as the path of the pair
until it moves onto the loop (stage 3d of the plan); Run and Stop commands and the
`runState` field (stage 3c); a Run command that clears a fault.

### Consequences

- Good, because the application runs between commands, with the timing `run`
  gives it, and the periodic write of the retained values fires on a real
  cadence.
- Good, because no lock is held across a round and the swap stays at the round
  boundary; the host has one owner, so a second transport (USB) is a new sender
  of the same channel.
- Good, because the pacer is the one place with the deadline and stop policy, and
  a test on a fake clock compares the rounds of the loop with the rounds of `run`.
- Good, because TCP gets the controlled stop the retained file needed.
- Neutral, because a freewheeling program keeps a core busy while the device is
  connected or not, as `run` does today.
- Bad, because the pair path now has a second way to run a round
  (`drive_scan_round`) until stage 3d; it is the only caller and is marked.
- Bad, because every command crosses a thread boundary and pays one channel hop;
  commands are rare and the hop is microseconds.

## Pros and Cons of the Options

### Keep command-driven rounds

- Good, because nothing changes and the tests stay deterministic.
- Bad, because the application does not run between commands, the cyclic tasks
  are not paced, and the periodic write of the retained values fires only when a
  swap command happens to arrive.

### A mutex-shared host and a scan thread

- Good, because the session threads keep their shape.
- Bad, because every command locks the host, and the lock is held across a round
  or the round across a command; a long session either stops the scan or is
  starved by it.
- Bad, because the swap must still happen at the round boundary, which the lock
  does not give: the scan thread would have to release the lock exactly there.
- Bad, because the mutexes of `tcp.rs` grow from the session thread to the scan
  thread, and every new transport has to learn the lock order.

### One owner thread with a command channel (chosen)

- Good, because the owner needs no lock and the boundary is where the loop is.
- Good, because the existing dispatch is reused unchanged.
- Bad, because the main thread now runs the loop and the accept loop and the
  stdin reader move to threads of their own.

## Confirmation

- `compiler/vm-cli/src/device_loop/tests.rs` drives `DeviceLoop` on a fake clock:
  the round times of a periodic program equal those of the `run` driver in
  `pacer/tests.rs`; `testEdits`, `untestEdits` and `assembleEdits` are answered
  after the boundary that applied them; a command that arrives in the middle of a
  period is answered at once; a freewheeling program still answers commands; a
  trap stops the scan while commands are answered and the fault is in `identity`;
  the stop flag ends the loop and the retained file is written.
- `compiler/vm-cli/src/pacer/tests.rs` covers the added questions of the pacer.
- `compiler/vm-cli/tests/cli/serve_tcp.rs` runs the TCP transport against the
  loop: a session sends commands, a second client is refused with V6014.
- The existing `serve_tests.rs`, `serve_device_tests.rs` and
  `serve_retained_tests.rs` run unchanged on the scripted session of `Core`; the
  HA pair tests are not touched.

## Run and Stop (stage 3c)

Run and Stop are a mechanism of their own beside Build: they start and stop the
scan, and no run path compiles. The device loop gains the one fact it needs, the
run state, and two commands that change it.

1. **Wire.** Two additive commands, `startScan` and `stopScan` (the style of
   `getStatus` and `cancelEdits`: a verb and its object, since `start` and `stop`
   alone say nothing next to `testEdits`). Each answers
   `{"response":"runState","runState":"running"|"stopped"}`, the state after the
   command: an acknowledgment that carries what it left. The `identity` answer
   always carries `runState` next to `fault`; a reader that finds it absent reads
   `running`, which is what a device that predates the field does. The protocol
   version stays 2. The status does not carry it, as it does not carry the
   fault: the run state is a fact of the device, and the status is the host's.
2. **Where the state lives.** In `Device`, next to the fault, and not in the
   host. Both are facts about whether a driver runs rounds, and only a driver can
   honour them: the host executes a round when it is asked to, as the benchmark
   and the MCP runner ask it. The runtime therefore owns the vocabulary (`RunState`,
   the commands) and refuses the commands for a host without a scan loop (V4022,
   the sibling of V4021 for settings); `ironplcvm serve` answers them itself
   (`scan_control_line`). The loop asks one question before every round,
   `Core::can_scan`: the host holds the permit, and the device is running and not
   faulted (`Device::scans`).
3. **Stop.** A command executes between rounds, so the round that was running
   has finished when the stop takes effect, and the answer is written after it.
   A stop of a running device writes the retained image through the path the
   stop of the process uses (`Device::stopped`): the values do not change while
   the device is stopped, and a power cut then loses nothing. A failed write
   answers V6018 and the device is stopped all the same.
4. **Start and the deadlines.** `RuntimeHost::run_paced_round` keeps the task
   deadlines between rounds (decision 6), so after a stop the deadlines of the
   cyclic tasks are in the past. Left alone, the first round would run each such
   task once and count an overrun for the whole stop, though the task missed
   nothing: the driver did not ask for rounds. The loop therefore restarts the
   deadlines (`rearm_cyclic_deadlines`, next to `earliest_due_us`) at the moment
   the scan resumes: a deadline that passed becomes due now, a deadline still
   ahead is kept. It does so when the device may scan after an iteration in
   which it could not, so a returned execution permit is a resume too. The result
   is one round at once, no burst, no overrun, and a period that counts from
   that round.
5. **Idempotence.** A start of a running device and a stop of a stopped device
   are acknowledged and change nothing, and a stop of a stopped device writes
   nothing.
6. **A faulted device.** A stop is accepted: the device is stopped and its fault
   stays reported, because the run state and the fault are reported apart. A
   start is refused with V6019 and changes nothing, because a start does not
   clear a fault and an acknowledged start of a device that cannot scan would
   lie; the device is restarted. A command that clears a fault (reset) is a later
   class: one command variant and its row in the device loop, with no new member
   of `RunState`.
7. **Hot edits while stopped.** A request of a swap is applied at its boundary
   without a round of logic, through the path the loop already uses for a
   faulted device (`apply_pending_at_boundary`), so the acknowledgment still
   means applied. `Core::round` has one test for both: the device does not scan.
8. **The pair path.** The HA pair path does not run on the loop yet (stage 3d)
   and does not scan between commands, so it has no scan to start or stop and
   answers both commands with V4022 rather than report a state it cannot keep.
9. **MCP.** `device_start` and `device_stop` take the `device` parameter of the
   other device tools (required: the in-process host has no scan loop) and send
   the command through the session client. They compile nothing.

Confirmation: `compiler/vm-cli/src/device_loop/tests.rs` drives the loop on the
fake clock (a stop runs no round and commands are answered, a start resumes
without a burst after a long stop and keeps a deadline that is still ahead, both
are idempotent, a stop writes the retained file, a start of a faulted device is
refused, a swap on a stopped device is applied without a round);
`compiler/vm/src/scheduler.rs` tests the deadline restart; `serve_device.rs`
runs the commands against a real process.

## More Information

- [ADR-0064](0064-online-change-on-a-redundant-pair.md): the edit lifecycle whose
  acknowledgment means the swap is applied.
- [ADR-0065](0065-engineering-session-exclusivity-and-ide-side-pending-edits.md):
  one engineering session, refused with V6014; unchanged.
- [ADR-0085](0085-retained-variables-in-the-container.md): the retained values
  the loop writes at a boundary and at the stop.
- [Engineering Connection](../design/engineering-connection.md) and
  [vm-cli](../design/vm-cli.md): the session and the `serve` requirements.
- [Glossary](../steering/glossary.md): "Run state", "Edit stage" and "Session".
