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

Loads a bytecode container file into the runtime host and serves the
hot-edit command protocol (ADR-0052, ADR-0055) on stdin/stdout until EOF.

```
ironplcvm serve <FILE>
```

**Behavior:**

- **REQ-VC-vm-cli-018** `serve` loads `<FILE>` exactly like `run`: a file that cannot be opened exits 2 with V6001, and bytes that are not a container exit 2 with V6002. The container starts on the runtime host (its init functions run once); a trap during init exits 1 with the trap's V-code.
- **REQ-VC-vm-cli-019** After startup, `serve` reads one command line from stdin, executes it through the runtime command layer, and writes exactly one response line to stdout, flushing after every line. Startup prints nothing to stdout; stdout carries response lines only.
- **REQ-VC-vm-cli-020** A line that does not parse as a command is a codec error, not an online change refusal, so it has no V-code (ADR-0055). The session answers it with one error line whose `vCode` is null, logs the diagnostic to stderr, and continues.
- **REQ-VC-vm-cli-021** When stdin reaches EOF, the session ends and the command exits 0.
- **REQ-VC-vm-cli-022** A failure reading stdin or writing stdout ends the session: the command exits 2 and emits V6011 to stderr.
- **REQ-VC-vm-cli-023** `testEdits`, `untestEdits` and `assembleEdits` only record a swap that applies at the next scan boundary. After the command layer acknowledges one of them, `serve` drives one scan round at a constant zero uptime (the runtime acceptance-test convention), before the response line is written, so a scripted client reads back state that has actually switched. A trap in the driven round is logged to stderr with the trap's V-code and the session continues.

- **REQ-VC-vm-cli-024** The `application` block of the `identity` and `getStatus` responses carries `contentHash` and `layoutHash`: the `content_hash` and `layout_hash` of the header of the normal (committed) artifact the host holds, as 64 lowercase hexadecimal digits, read from the header and not recomputed. They follow the normal artifact through an assemble and stay those of the normal artifact while a candidate runs under Test. The fields are absent when the header carries the all-zero (no hash) value. The generation counters restart when the process restarts; the hashes do not.

#### The device settings

`serve` keeps the settings that belong to the device, not to the application it runs, in the file `<FILE>.device.json`, next to the slot store's files. The file is versioned JSON; each setting is one key:

```json
{
  "version": 1,
  "deviceId": "0123456789abcdef0123456789abcdef",
  "name": "ironplcvm"
}
```

| Key | Value | First value | Changed |
|-----|-------|-------------|---------|
| `deviceId` | 32 lowercase hexadecimal digits | 128 bits from the operating system's random source | in the file only |
| `name` | 1 to 64 characters, not blank, no control characters | `ironplcvm` | in the file and by `setDeviceSettings` |

- **REQ-VC-vm-cli-025** When `<FILE>.device.json` does not exist, `serve` generates `deviceId`, writes the file and logs one line; every later start with that file reports the same `deviceId`. A device that has no file is a new device, and two new devices have different ids.
- **REQ-VC-vm-cli-026** When `<FILE>.device.json` is not JSON, has a `version` other than 1, names a setting that does not exist, or holds a value that its setting refuses, `serve` does not start: it exits with code 2 and emits V6015 naming the file and the field, and leaves the file as it is. A device never replaces a file it cannot read with a new identity.
- **REQ-VC-vm-cli-027** `{"command":"setDeviceSettings", ...}` names the settings to change by their keys. When every named setting is one that the device allows to change online and every named value passes its setting's check, `serve` writes the file, answers `ack`, and reports the new values in `identity` and after a restart. A setting that is not named stays as it is.
- **REQ-VC-vm-cli-028** `setDeviceSettings` with a value that a setting refuses, or with a key that is no setting, answers V6015, and with a file that cannot be written answers V6016. In both cases neither the file nor the running device changes. The command is allowed while a candidate is staged or under test: the settings are not the application.
- **REQ-VC-vm-cli-029** The `device` block of `identity` carries `deviceId` and `name` from the settings, on stdio, on TCP and in HA pair mode. One function builds the block from the settings and the constants of the build.
- **REQ-VC-vm-cli-030** `setDeviceSettings` naming a setting that is changed in the settings file only (`deviceId`) answers V6015, whatever the value and whatever else the command names; neither the file nor the running device changes. The next start adopts a `deviceId` edited in the file. The id is how a workstation recognises the device again, so it is changed on the device itself and never over the engineering session.

The session stays one line in, one line out: the scan round an FSM-advancing command drives happens before its response line is written, so the acknowledgment means the swap has been applied, not merely recorded.

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
