# Spec: VM CLI and Variable Dump

## Overview

This spec defines the `ironplcvm` command-line executable and the variable dump output format. The executable loads a bytecode container file and runs it on the VM. The variable dump provides observable output to verify the VM executed correctly.

This spec builds on:

- **[Bytecode Container Format](bytecode-container-format.md)**: The container file that `ironplcvm` reads
- **[Runtime Execution Model](runtime-execution-model.md)**: The VM lifecycle and scan cycle that `ironplcvm` drives
- **[Bytecode Instruction Set](bytecode-instruction-set.md)**: The instructions the VM executes

## Design Goals

1. **Observable execution** — the variable dump provides proof that the VM ran and computed correct results, without polluting stdout
2. **Silent success** — a successful run produces no terminal output; the exit code signals success or failure
3. **Consistent CLI patterns** — follows the same argument structure and conventions as `ironplcc`

## CLI Interface

### Binary Name

`ironplcvm`

### Global Options

| Option | Short | Description |
|--------|-------|-------------|
| `--verbose` | `-v` | Increase logging verbosity. Repeatable up to 4 times (error → warn → info → debug → trace). |
| `--log-file <path>` | `-l` | Write log output to a file instead of stderr. |

### Subcommands

#### `run`

Loads a bytecode container file and executes it.

```
ironplcvm run [OPTIONS] <FILE>
```

**Arguments:**

| Argument | Description |
|----------|-------------|
| `<FILE>` | Path to the bytecode container file (`.iplc`). |

**Options:**

| Option | Description |
|--------|-------------|
| `--dump-vars [PATH]` | After the VM stops, write all variable values to `PATH`. If `PATH` is omitted or `-`, write to stdout. |
| `--scans <N>` | Run exactly `N` scheduling rounds then stop. When omitted, runs continuously until SIGINT (Ctrl+C). |

**Behavior:**

- **REQ-VC-vm-cli-001** `run` opens the container file at `<FILE>`. If the file cannot be opened, the command exits with code 2 and emits V6001 to stderr.
- **REQ-VC-vm-cli-002** `run` decodes the container. If the bytes are not a valid container (bad magic, truncated, unsupported version), the command exits with code 2 and emits V6002 to stderr.
- **REQ-VC-vm-cli-003** `run --scans N` executes exactly `N` scheduling rounds then exits 0.
- **REQ-VC-vm-cli-004** When execution traps (divide by zero, stack overflow, invalid instruction, etc.), `run` exits with code 1 and emits the trap's V-code to stderr.
- **REQ-VC-vm-cli-011** When no `--scans` value is given, `run` loops until SIGINT (Ctrl+C). On SIGINT it requests a clean stop and exits 0 after the current round.
- **REQ-VC-vm-cli-012** Between rounds, `run` sleeps until the next cyclic task is due (based on `next_due_us`) to avoid busy-looping.

#### `benchmark`

Measures execution timing by running a container for a fixed number of rounds.

```
ironplcvm benchmark [OPTIONS] <FILE>
```

**Options:**

| Option | Default | Description |
|--------|---------|-------------|
| `--cycles <N>` | 10000 | Number of measured scan rounds. |
| `--warmup <M>` | 1000 | Number of unmeasured warmup rounds before measurement. |

**Behavior:**

- **REQ-VC-vm-cli-013** `benchmark` prints a single JSON object to stdout containing `program`, `opt_level`, `cycles`, `warmup`, a `scan_us` object with `mean`, `stddev`, `p99`, and `max` in microseconds, and a `tasks` array with per-task metadata.
- **REQ-VC-vm-cli-014** `benchmark --cycles N --warmup M` executes `M` unmeasured warmup rounds followed by `N` measured rounds.
- **REQ-VC-vm-cli-015** For each cyclic task with `interval_us > 0`, the JSON `tasks[*]` entry includes a `budget_pct` object with `mean`, `p99`, and `max` expressed as a percentage of the task's interval.
- **REQ-VC-vm-cli-016** File-open (V6001) and container-read (V6002) errors behave identically to `run`: exit code 2 with the V-code on stderr.
- **REQ-VC-vm-cli-017** If a trap occurs during either the warmup or measured phase, `benchmark` exits with code 1 and emits the trap's V-code to stderr.

#### `version`

Prints the version string.

```
ironplcvm version
```

Output: `ironplcvm version <VERSION>` followed by a newline on stdout.

#### `serve`

Loads a bytecode container file into the runtime host, runs its scan cycle,
and serves the hot-edit command protocol (ADR-0052, ADR-0055) on stdin/stdout
until EOF or Ctrl+C. The scan is run by a device loop (ADR-0086) that owns the
host; the session sends it the commands.

```
ironplcvm serve <FILE>
```

**Behavior:**

- **REQ-VC-vm-cli-018** `serve` loads `<FILE>` exactly like `run`: a file that cannot be opened exits 2 with V6001, and bytes that are not a container exit 2 with V6002. The container starts on the runtime host (its init functions run once); a trap during init exits 1 with the trap's V-code.
- **REQ-VC-vm-cli-019** After startup, `serve` reads one command line from stdin, executes it through the runtime command layer, and writes exactly one response line to stdout, flushing after every line. Startup prints nothing to stdout; stdout carries response lines only.
- **REQ-VC-vm-cli-020** A line that does not parse as a command is a codec error, not an online change refusal, so it has no V-code (ADR-0055). The session answers it with one error line whose `vCode` is null, logs the diagnostic to stderr, and continues.
- **REQ-VC-vm-cli-021** When stdin reaches EOF, the session ends and the command exits 0. The commands that arrived before the end are answered first.
- **REQ-VC-vm-cli-022** A failure reading stdin or writing stdout ends the session: the command exits 2 and emits V6011 to stderr.
- **REQ-VC-vm-cli-023** `testEdits`, `untestEdits` and `assembleEdits` only record a swap that applies at a scan boundary. After the command layer acknowledges one of them, the device loop runs the next round at once, without waiting for the period of the tasks, and writes the response line after that round, so the acknowledgment means the swap has been applied and a client reads back state that has actually switched. A trap in that round is the application's own fault (REQ-VC-vm-cli-044).

- **REQ-VC-vm-cli-024** The `application` block of the `identity` and `getStatus` responses carries `contentHash` and `layoutHash`: the `content_hash` and `layout_hash` of the header of the normal (committed) artifact the host holds, as 64 lowercase hexadecimal digits, read from the header and not recomputed. They follow the normal artifact through an assemble and stay those of the normal artifact while a candidate runs under Test. The fields are absent when the header carries the all-zero (no hash) value. The generation counters restart when the process restarts; the hashes do not.

#### The device settings

`serve` keeps the settings that belong to the device, not to the application it runs, in the file `<FILE>.device.json`, next to the slot store's files. The file is versioned JSON; each setting is one key:

```json
{
  "version": 1,
  "deviceId": "0123456789abcdef0123456789abcdef",
  "name": "ironplcvm",
  "retainedSavePeriod": 0
}
```

| Key | Value | First value | Changed |
|-----|-------|-------------|---------|
| `deviceId` | 32 lowercase hexadecimal digits | 128 bits from the operating system's random source | in the file only |
| `name` | 1 to 64 characters, not blank, no control characters | `ironplcvm` | in the file and by `setDeviceSettings` |
| `retainedSavePeriod` | a JSON number: a whole number of seconds, 0 to 86400 | `0` (off) | in the file and by `setDeviceSettings` |

- **REQ-VC-vm-cli-025** When `<FILE>.device.json` does not exist, `serve` generates `deviceId`, writes the file and logs one line; every later start with that file reports the same `deviceId`. A device that has no file is a new device, and two new devices have different ids.
- **REQ-VC-vm-cli-026** When `<FILE>.device.json` is not JSON, has a `version` other than 1, names a setting that does not exist, or holds a value that its setting refuses, `serve` does not start: it exits with code 2 and emits V6015 naming the file and the field, and leaves the file as it is. A device never replaces a file it cannot read with a new identity.
- **REQ-VC-vm-cli-027** `{"command":"setDeviceSettings", ...}` names the settings to change by their keys. When every named setting is one that the device allows to change online and every named value passes its setting's check, `serve` writes the file, answers `ack`, and reports the new values in `identity` and after a restart. A setting that is not named stays as it is.
- **REQ-VC-vm-cli-028** `setDeviceSettings` with a value that a setting refuses, or with a key that is no setting, answers V6015, and with a file that cannot be written answers V6016. In both cases neither the file nor the running device changes. The command is allowed while a candidate is staged or under test: the settings are not the application.
- **REQ-VC-vm-cli-029** The `device` block of `identity` carries `deviceId` and `name` from the settings, on stdio, on TCP and in HA pair mode. One function builds the block from the settings and the constants of the build.
- **REQ-VC-vm-cli-030** `setDeviceSettings` naming a setting that is changed in the settings file only (`deviceId`) answers V6015, whatever the value and whatever else the command names; neither the file nor the running device changes. The next start adopts a `deviceId` edited in the file. The id is how a workstation recognises the device again, so it is changed on the device itself and never over the engineering session.

#### The retained file

`serve` keeps the values of the `RETAIN` and `PERSISTENT` variables across a restart in the file `<FILE>.retained`, next to the slot store's files (ADR-0085). The file is an encoded retained image: its tag holds the content hash of the application the values were read from and a hash of that application's retained runs. The temporary file of a write is `<FILE>.retained.tmp`. The retained file is a stored item of the device: data it keeps across a restart, with the tag of the application it belongs to (`crate::stored_item`).

- **REQ-VC-vm-cli-037** At boot, after the host is composed and before its first scan round, `serve` applies the retained file to the host only when the tag of the image equals the application that boots (the content hash and the hash of the retained runs). A file that is missing is `absent`. A file that is damaged (it does not decode, cannot be read, or does not hold as many values as the retained runs), or whose tag is another application's or another layout of the retained runs, is discarded: nothing is applied and the file stays until the next write replaces it. Variables that are not retained start with their initial values.
- **REQ-VC-vm-cli-038** The device writes the retained file, atomically (a temporary file renamed over it), at a controlled stop (see REQ-VC-vm-cli-045), after `assembleEdits` has committed the new application to the slot store (the image of the new application, tagged with its content hash), and, when `retainedSavePeriod` is not 0, at a scan boundary when that many seconds have passed since the last write attempt and the image differs from the one in the file. It writes nothing while a candidate runs under Test, and nothing for an application that did not reach the slot store (V6012), because the device would not boot it. The controlled stop is the end of stdin and Ctrl+C, on stdio and on TCP alike (REQ-VC-vm-cli-045).
- **REQ-VC-vm-cli-039** A write that fails is V6018. At `assembleEdits` the error replaces the acknowledgment (as V6012 does) and the promotion in RAM stands; at the period it is logged and the device goes on; at the stop it is logged and `serve` exits with code 2 and V6018, unless the session itself failed, in which case that error is the one reported. A failed write leaves the file as it was.
- **REQ-VC-vm-cli-040** The `identity` answer carries two additive optional fields (the protocol version stays 2): `persistedContentHash`, the content hash of the application the device would boot (the newest committed slot), which differs from `application.contentHash` after a commit that is live and not durable (V6012); and `retained`, what boot did with the retained file: `{"result":"applied"}`, `{"result":"absent"}` or `{"result":"discarded","reason":...}` with the reason `damaged`, `otherApplication`, `otherRetainedLayout` or `notAtBoot`. A discarded file is never silent. One function fills both fields for stdio, TCP and HA pair sessions.
- **REQ-VC-vm-cli-041** `retainedSavePeriod` is a JSON number, a whole number of seconds from 0 to 86400. A negative number, a fraction, a number in quotes, `null` or a number over 86400 is refused with V6015 in the file and by `setDeviceSettings`, and the device is unchanged.

The session stays one line in, one line out: the scan boundary an FSM-advancing command waits for happens before its response line is written, so the acknowledgment means the swap has been applied, not merely recorded.

#### The device loop

`serve` runs the scan by itself. One thread, the device loop (`compiler/vm-cli/src/device_loop.rs`, ADR-0086), owns the runtime host, the redundancy shell, the slot store and the device settings; the session threads (stdio, TCP) send it each command line and write its answer. No lock is held across a round.

- **REQ-VC-vm-cli-042** The device loop runs a scan round whenever one is due, the host holds the execution permit and the device is neither faulted nor stopped (REQ-VC-vm-cli-047), as `run` does, and the pacing is the pacer's (`compiler/vm-cli/src/pacer.rs`), shared with `run`: the round starts at the uptime read from the clock immediately before it, a cyclic task keeps its period (its deadline stays on the grid of its interval, a late round does not move the next deadline), and a program without a cyclic task starts its next round at once. A device that holds no execution permit runs no round and answers its commands.
- **REQ-VC-vm-cli-043** A command is executed between rounds and is not left waiting for the period: the loop waits for the next deadline for a command to arrive, so a command waits at most for the round that is running, and a program that never waits still looks for commands once per round. A command that requests a swap is answered after the boundary round that applied it (REQ-VC-vm-cli-023). Exactly one thread touches the host.
- **REQ-VC-vm-cli-044** A trap in a round stops the scan: the device is faulted, runs no further round until the process restarts, and keeps answering every command. A request of a swap still reaches its boundary (the swap is applied without a scan). The `identity` answer carries the additive optional field `fault` (the protocol version stays 2) exactly while the device is faulted: `{"vCode":"V4001","message":"...","taskId":0,"instanceId":0}`, the V-code of the trap, its message, and the task and the program instance that were running.
- **REQ-VC-vm-cli-045** Ctrl+C is the controlled stop of the process, on stdio and on TCP: the loop ends after the round that is running, answers the commands it has executed, writes the retained values (REQ-VC-vm-cli-038) and exits 0, or exits 2 with V6018 when the write fails. The end of stdin ends the loop the same way on stdio.

#### Run and Stop

Run and Stop are a mechanism of their own beside Build: they change whether the device executes rounds, and nothing else (ADR-0086, "Run and Stop (stage 3c)"). The run state is the device's (`running` or `stopped`), apart from the edit stage and apart from a fault. Nothing here compiles anything, stages anything or touches the edit.

- **REQ-VC-vm-cli-046** `{"command":"startScan"}` and `{"command":"stopScan"}` answer `{"response":"runState","runState":"running"}` or `{"response":"runState","runState":"stopped"}`, the state after the command (the protocol version stays 2). A start of a running device and a stop of a stopped device are acknowledged and change nothing. The commands do not touch the edit: a staged candidate and a test in progress are as they were. The device boots running, and the `identity` answer always carries `runState`; a device that predates the field scans, so a reader that finds it absent reads `running`.
- **REQ-VC-vm-cli-047** A stopped device runs no round, waits idle like a device without the execution permit, and answers every command. A stop executes between rounds, so the round that was running has finished before the answer is written. A stop of a running device writes the retained values (REQ-VC-vm-cli-038), because they do not change while it is stopped; a write that fails answers V6018 instead of the state, and the device is stopped all the same. A stop of a stopped device writes nothing.
- **REQ-VC-vm-cli-048** When the scan resumes after a gap in which no round ran (a start, a returned execution permit), the loop restarts the deadlines of the cyclic tasks from that moment: a deadline that passed becomes due now, so each cyclic task runs once in the first round and its period counts from that round, and a deadline still ahead is kept. The gap is neither a burst of rounds nor a counted overrun.
- **REQ-VC-vm-cli-049** A faulted device accepts a stop: it is stopped and its `fault` is still reported. A start of a faulted device answers V6019 (the device is faulted; restart the device to clear the fault) and changes nothing, whatever its run state; the run state and the fault are reported apart in `identity`.
- **REQ-VC-vm-cli-050** A request of a swap (`testEdits`, `untestEdits`, `assembleEdits`) on a stopped device is applied at its boundary without a round of logic, as on a faulted device, so the acknowledgment still means applied (REQ-VC-vm-cli-023). The swapped-in application runs from the first round after a start.
- **REQ-VC-vm-cli-051** A device that serves the HA pair link (`--ha-peer-bind`) runs on the device loop like every other composition of `serve`; it is not a second driver. The loop owns the pair link and ticks it once per pump period (5 ms), whether the device scans or not, and never more often: the liveness of the link counts exchanges, not time, so a tick per round of a freewheeling program would count a peer that answers at the pump cadence as dead. A wait of the loop ends at the pump when that comes before the next deadline of a cyclic task. After every tick the loop reconciles the execution permit with the verdict of the link, before it decides whether a round is due, so a verdict decided by a tick is the verdict of that iteration. The scan commit of every round reaches the link (`on_scan_commit`), so the Primary scans between commands and mints one epoch per committed round, and the Secondary, which holds no permit, runs no round.
- **REQ-VC-vm-cli-052** A pair unit honours `startScan` and `stopScan` as any device does (REQ-VC-vm-cli-046, REQ-VC-vm-cli-047). A stopped Primary runs no round, so it produces no scan commit and mints no epoch, but it stays the owner of the pair: its verdict does not change, its link keeps being ticked at the pump cadence, and its peer sees a live Primary, so the Secondary does not promote. No new failover policy follows from a stop. A start resumes the scan with the deadline restart of REQ-VC-vm-cli-048. A start of a Secondary is acknowledged and the unit still runs no round, because it holds no permit. The `control` field of the redundancy block of `identity` reports ownership (the permit), and `runState` reports the scan, apart.
- **REQ-VC-vm-cli-053** The survivor of a confirmed peer death (ADR-0064(e)) holds the permit from the tick that promoted it, so the first round it runs is the takeover round it owes the pair: it runs in the iteration of the promotion, applies a pending swap at its boundary, and mints the next epoch through the scan commit. If the operator has stopped the survivor, it runs no round until a start: the stop stands over the promotion.

#### Discovery on the listen address

`serve --listen <ADDR>` serves the session on TCP. The device also answers
discovery on UDP at the same address and port, so that a workstation can find it
(ADR-0084). The bytes of the request and the reply are specified in
[plc-discovery.md](plc-discovery.md) and are encoded by the `ironplc-discovery`
crate; this section is what the device does with them.

| Option | Description |
|--------|-------------|
| `--listen <ADDR>` | `IP:PORT`, or an IP address alone for the default session port. |
| `--no-discovery` | Do not open the discovery port. Requires `--listen`. |

- **REQ-VC-vm-cli-031** `--listen` takes `IP:PORT` (for IPv6, `[IP]:PORT`) or an IPv4 or IPv6 address alone, which listens on the default session port. The default port is read from `DEFAULT_SESSION_PORT` of the runtime crate and stated nowhere else. A host name, or any other text, is a usage error.
- **REQ-VC-vm-cli-032** With `--listen`, `serve` binds a UDP socket to the address and port that the TCP listener has, before it accepts the first connection, and answers a discovery request with one reply on a thread of its own, also while a session is open. The reply carries the device id, name, model and runtime version of the `identity` answer (built from the same identity), the session protocol version, the TCP port of the session and `protected` false (no session credentials exist). The identity is read for every request, so a name changed by `setDeviceSettings` is in the next reply.
- **REQ-VC-vm-cli-033** A datagram that is not exactly the 8 bytes of the request gets neither a reply nor an error: not an empty datagram, not a shorter or longer one, not the request with one byte changed. The device goes on answering.
- **REQ-VC-vm-cli-034** The device sends at most one reply per 250 ms to one source IP address (the port does not count), at most 64 replies in any second to all sources together, and remembers at most 256 source addresses (when the table is full, the source with the oldest reply is forgotten). The reply is up to 128 times the size of the request and a source address can be forged, so the limits bound what the device adds to an attack on a third party.
- **REQ-VC-vm-cli-035** With `--no-discovery` the device opens no UDP socket and answers no request. The flag without `--listen` is a usage error.
- **REQ-VC-vm-cli-036** When the UDP port cannot be bound, `serve` exits with code 2 and emits V6017, and does not serve the session. A device that cannot be found is reported, as a TCP port that cannot be bound is. `--no-discovery` runs the device without discovery.

## Variable Dump Format

The `--dump-vars [PATH]` option writes all variable slot values after the VM stops (successfully or from a fault).

### Behavior

- **REQ-VC-vm-cli-005** After a successful run, `--dump-vars <PATH>` writes one variable per line, newline-terminated.
- **REQ-VC-vm-cli-006** If `--dump-vars` is specified without a `PATH`, or with `PATH` equal to `-`, the dump is written to stdout.
- **REQ-VC-vm-cli-007** If a runtime trap occurs and `--dump-vars` is set, the dump of the current variable state is written before the command exits non-zero.
- **REQ-VC-vm-cli-008** When the container's debug section names a variable, the line uses `<name>: <value>`. Otherwise the line uses `var[<index>]: <raw_i32>`.
- **REQ-VC-vm-cli-009** `<value>` is rendered by
  `ironplc_container::debug_format::VariableRenderer` per
  [Variable Value Rendering](variable-value-rendering.md), so a variable reads
  the same in the dump, in the debugger and in the playground:

```
msg: 'hello'
flag: TRUE
n: 42
span: T#1500ms
day: D#2024-01-15
```

  The rendering table itself is owned by that document
  (`REQ-VR-container-*`) rather than restated here — the dump is one of four
  surfaces showing the same values, and a second copy of the table is a copy
  that drifts.

- **REQ-VC-vm-cli-010** If the dump file cannot be created (e.g., parent directory missing), the command exits with code 2 and emits V6004 to stderr.

### Format

One line per variable, zero-indexed, newline-terminated (no debug info):

```
var[0]: 10
var[1]: 42
```

With debug info:

```
Buzzer: TRUE
Counter: 42
```

### Rules

1. **Variable count** comes from `container.header.num_variables`.
2. **Variable order** is ascending by index, 0 through `num_variables - 1`.
3. **File creation**: the dump file is created or overwritten (not appended).
4. **Empty programs**: if `num_variables` is 0, the dump file is created but empty.

### Example

For a program `x := 10; y := x + 32;` with two variables:

```
var[0]: 10
var[1]: 42
```

## Exit Codes

| Code | Meaning |
|------|---------|
| 0 | Success — program loaded and execution completed without traps. |
| 1 | Runtime trap — a VM trap occurred during execution. |
| 2 | IO or container error — file could not be opened, read, created, or written. |

## Future Extensions

- Type-aware variable dump for richer IEC types (arrays, structs) once the container type section covers them.
