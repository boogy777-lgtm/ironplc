# Spec: Engineering Connection

## Overview

This spec defines the engineering connection mechanisms of the IronPLC
desktop IDE: how an engineer configures a connection to a controller,
establishes it, and starts a build onto the connected device. It covers
three mechanisms:

1. **Connection settings** — the connection profile model, its persistence
   in editor settings, its credential storage in the host secret store, and
   its client-side validation codes.
2. **Establish connection** — one session protocol over two transports
   (spawned stdio and remote TCP), an `identity` handshake that fills the
   connected-device panel, a client-side connection state machine, and the
   reconnect and timeout policies.
3. **Start build** — the wiring of a Build button onto the existing
   compile → upload → verify → run pipeline; almost everything already
   exists and this spec cites it rather than redesigning it.

The reference UI is the owner's desktop IDE layout (Owen Logic style): an
auth block (login/password), connection parameters (IP address, port, a
Connect button), a connected-device info panel (IP, name, model,
modification, firmware version, connection state, application state), an
error toast on bad settings, and a status bar reading "Not connected".

This spec builds on:

- **[ADR-0052](../adrs/0052-online-change-performed-by-the-runtime-host.md)**:
  the host-level online change protocol the session drives
- **[ADR-0055](../adrs/0055-hot-edit-command-layer-in-ironplc-runtime.md)**:
  the typed command layer and its line-delimited JSON codec — the session
  protocol both transports carry
- **[ADR-0063](../adrs/0063-engineering-connection-transport.md)**: the
  binding decisions — dual transport with one protocol, deferred wire
  authentication, build as reuse
- **[vm-cli.md](vm-cli.md)**: the `ironplcvm serve` session requirements
  (REQ-VC-vm-cli-018 through REQ-VC-vm-cli-023)
- **[HA Redundancy FSM](ha-redundancy-fsm.md)**: the epoch and SYNC/CONTROL
  vocabulary the `identity` response's redundancy block mirrors, and the
  state-machine notation this spec reuses
- **[HA Engineering UI Contract](ha-engineering-ui.md)**: the established
  "one vocabulary, thin clients, no new wire machinery" pattern

## Design Goals

1. **One protocol, two transports** — the session is the ADR-0055
   line-delimited JSON command protocol on both stdio and TCP; TCP adds a
   framing envelope, not a second protocol
2. **Reuse before design** — every mechanism lands on an existing call;
   new machinery is limited to what has no existing call (framing, the
   handshake, profile storage)
3. **Every failure is coded** — transport and session failures carry
   stable V-codes server-side; client-side validation and connection
   failures carry E-codes; the codec-error line stays codeless per
   ADR-0055
4. **Secrets never in configuration** — a profile stores a secret-store
   key, never a credential; the editor's SecretStorage is the only store
5. **Extensible identity** — the handshake field set grows by adding
   optional fields; clients ignore fields they do not know, and an old
   server's refusal of `identity` is the version negotiation

## Mechanism 1 — Connection Settings

### Profile model

One connection profile describes one target. Fields:

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `name` | string | yes | Display name; unique within its settings scope. |
| `transport` | `"stdio"` \| `"tcp"` | yes | `stdio` spawns a local `ironplcvm serve` child; `tcp` opens a socket to a remote device. |
| `program` | string | stdio only | The source file or `.iplc` container the spawned session loads (the `serve <FILE>` argument). |
| `address` | string | tcp only | IPv4 dotted-quad, IPv6 literal, or DNS hostname of the device. |
| `port` | integer | tcp only | TCP port, 1–65535. Default 49152 (first IANA dynamic/private port) when omitted. |
| `credentials` | string | no | The **secret-store key** the host resolves a username/password pair from — never the credentials themselves. |

The profile carries no protocol state and no secrets. `credentials` names
an entry in the host secret store; for the VS Code extension that store is
`SecretStorage` (keyed `ironplc-connection/<profile name>`), for a future
standalone desktop host it is the OS keychain. The reference UI's
login/password block edits the secret-store entry, not the settings file.

### Persistence

Profiles persist as an `ironplc.connections` array in workspace or user
settings (standard editor settings precedence applies):

```json
"ironplc.connections": [
  {
    "name": "Local VM",
    "transport": "stdio",
    "program": "${workspaceFolder}/main.st"
  },
  {
    "name": "Cell 1 PLC",
    "transport": "tcp",
    "address": "192.168.1.10",
    "port": 49152,
    "credentials": "ironplc-connection/cell-1-plc"
  }
]
```

One profile is the active connection per workspace; the choice persists as
a single `ironplc.activeConnection` name.

### Validation

Validation runs in the editor before any transport opens, so a bad profile
fails without network traffic — the reference UI's error toast on bad
settings. Every failure is an E-code from the extension's existing CSV
registry (`integrations/vscode/resources/problem-codes.csv`, next free
E0010) and renders through `formatProblem`
(`integrations/vscode/src/problems.ts:30`):

| Rule | Code on failure |
|------|-----------------|
| Profile missing `name`, duplicate `name`, or unknown `transport` | E0010 ConnectionProfileInvalid |
| `address` is not an IPv4/IPv6 literal or RFC 1123 hostname; `address` present on a stdio profile | E0011 ConnectionAddressInvalid |
| `port` not an integer in 1–65535 | E0012 ConnectionPortInvalid |

Documentation and the codegen lifecycle follow
`specs/steering/problem-code-management.md` (CSV row, generated
`problems.ts`, `docs/vscode/problems/E####.rst`).

## Mechanism 2 — Establish Connection

### One session protocol, two transports

The session protocol is the ADR-0055 command layer unchanged: one JSON
command in, one JSON response out, rendered by `parse_command` /
`render_response` (`compiler/runtime/src/commands.rs:270`,
`compiler/runtime/src/commands.rs:275`) and dispatched by `execute`
(`compiler/runtime/src/commands.rs:284`). Both transports carry the same
JSON values; they differ only in the envelope:

- **stdio** — the existing behavior: the client spawns
  `ironplcvm serve <FILE>` and speaks newline-delimited JSON over the
  child's stdin/stdout (`compiler/vm-cli/src/serve.rs:67`,
  `integrations/vscode/src/hotEdit.ts:133`). One line is one message.
- **tcp** — the device listens on the profile's address/port; each message
  travels as one frame: a 4-byte little-endian unsigned length followed by
  exactly that many bytes of one UTF-8 JSON line (no trailing newline).
  Little-endian matches the container wire format. The frame replaces the
  newline delimiter; the JSON inside the frame is byte-identical to the
  stdio line. One frame in, one frame out — the REQ-VC-vm-cli-019 ordering
  guarantee holds verbatim, so the existing client session logic
  (`integrations/vscode/src/hotEditSession.ts:258`, request FIFO matching)
  works over either transport behind the `HotEditTransport` interface
  (`integrations/vscode/src/hotEditSession.ts:238`), exactly as the stdio
  line buffer already does (`integrations/vscode/src/hotEdit.ts:460`).

A frame whose declared length exceeds 16 MiB (the container-upload
headroom; `acceptEdits` bytes ride one message, ADR-0055) is a framing
error: the device closes the connection and reports V6013.

### The `identity` handshake

`identity` is a new command in the session vocabulary, the first command a
client sends on every (re)opened transport. It fills the reference UI's
connected-device panel and carries the application state the runtime
already reports, reusing the `StatusPayload` shape
(`compiler/runtime/src/commands.rs:136`) as a nested block.

Request:

```json
{"command":"identity"}
```

Response (all blocks shown; `redundancy` is present only when the
redundancy layer is loaded):

```json
{
  "response": "identity",
  "protocol": 1,
  "device": {
    "name": "ironplcvm",
    "model": "IronPLC SoftPLC",
    "modification": "vm-cli",
    "firmwareVersion": "0.13.0"
  },
  "application": {
    "mode": "normal",
    "active": 1,
    "normal": 1,
    "candidate": null,
    "application": 1,
    "migration": false,
    "rounds": 0
  },
  "redundancy": {
    "pairId": "7f3a9c",
    "role": "primary",
    "epoch": 12,
    "sync": "syncReady",
    "control": "active"
  }
}
```

Field set:

- `protocol` — session protocol version, `1` for the version this spec
  defines. A client refuses a higher number.
- `device` — the panel fields: `name`, `model`, `modification`,
  `firmwareVersion`. A soft device reports its binary name and version; a
  hardware target reports its catalog values.
- `application` — the live hot-edit status snapshot (mode, generation
  counters, rounds), the same fields `getStatus` returns
  (`compiler/runtime/src/commands.rs:136`).
- `redundancy` — optional: `pairId`, configured `role`, ownership `epoch`,
  and the SYNC/CONTROL substates of
  [HA Redundancy FSM](ha-redundancy-fsm.md). Absent means standalone.

Extensibility is structural, not negotiated: servers add optional fields,
clients ignore fields they do not know, and every block is an object so
fields can grow inside it. Version negotiation rides the ADR-0055 codec
behavior: a server that predates `identity` answers the line with the
codeless codec error (`compiler/vm-cli/src/serve.rs:137`), which the client
reads as "protocol 0 — hot edit only" and keeps the session for hot edit
commands.

### Connection state machine

The client owns one state machine per profile. It adds no state to the
server: the session itself stays stateless per ADR-0055.

```text
                 Connect command
                 (profile valid)
                       │
                       ▼
              ┌────────────────┐   transport open    ┌────────────┐
              │  Disconnected  │────────────────────►│ Connecting │
              └────────────────┘                     └─────┬──────┘
                    ▲    ▲                                 │
                    │    │            identity ok          │ open failure /
                    │    └─────────────────────────────────┤ timeout /
                    │             exhaustion │             │ identity error
                    │                        ▼             ▼
                    │                  ┌─────────────┐  (report E0013)
                    │    reconnect ok  │ Reconnecting│
                    │   ┌──────────────┴─────────────┤
                    │   │ attempt failed,            │
                    │   │ attempts remain            │
                    │   ▼                            │
                    │  (backoff, next attempt)       │
                    │                                │
              ┌─────┴───────┐  transport error  ┌────┴─────┐
              │ Disconnected│◄──────────────────│ Connected│
              └─────────────┘   user disconnect └──────────┘
```

| State | Description |
|-------|-------------|
| Disconnected | No transport open. The status bar reads "Not connected"; the device panel is empty. Terminal state of every failure path. |
| Connecting | Transport open in progress, then the `identity` handshake in progress. No other command may be sent. Bounded by the connect timeout. |
| Connected | Handshake complete; the device panel renders the `identity` data; the session answers commands. The idle heartbeat runs. |
| Reconnecting | The transport was lost; a bounded retry sequence with exponential backoff is in progress. The panel keeps the last `identity` data, marked stale. No command may be sent. |

| From | To | Trigger | Action |
|------|----|---------|--------|
| (start) | Disconnected | Extension host start | Render "Not connected" |
| Disconnected | Connecting | Connect command; guard: profile valid | Open the transport per the profile; start the connect timeout |
| Disconnected | Disconnected | Connect command; guard: profile invalid | Report E0010/E0011/E0012 (the toast) |
| Connecting | Connected | Transport open and `identity` response ok | Cancel the connect timeout; render the device panel; start the heartbeat |
| Connecting | Disconnected | Open failure, connect timeout, or `identity` error | Report E0013 with the server's V-code when the error carries one |
| Connected | Connected | Any command fails with a coded `error` response | Surface the V-code; the state does not change (a refusal is not a transport fault) |
| Connected | Reconnecting | Transport error: read/write failure, response timeout, or heartbeat miss | Stop the heartbeat; schedule attempt 1 |
| Connected | Disconnected | User disconnect | Close the transport; render "Not connected" |
| Reconnecting | Connected | Transport open and `identity` response ok | Resume the heartbeat; re-render the panel (fresh data) |
| Reconnecting | Reconnecting | Attempt failed; guard: attempts remain | Wait the next backoff; attempt again |
| Reconnecting | Disconnected | Attempt failed; guard: attempts exhausted | Report E0014 |
| Reconnecting | Disconnected | User disconnect | Abandon the retry sequence; close the transport |

Invariants:

```text
one open session per profile
identity is the first command on every (re)opened transport
no command is sent outside Connected
a coded error response is a protocol answer, never a reconnect trigger
user disconnect from any state lands in Disconnected and cancels nothing else
```

### Reconnect policy

Bounded retries with exponential backoff and full jitter:

- attempts: 5
- delay before attempt *n*: `random(0, min(500ms * 2^(n-1), 8s))`
- the sequence restarts from attempt 1 after any successful reconnect

A user disconnect, a profile-validation failure, and a coded `identity`
refusal never trigger a reconnect: only transport faults do.

### Timeout policy

All values are v1 defaults, overridable per profile in the settings schema:

| Timeout | Default | Fires when |
|---------|---------|------------|
| Connect timeout | 5 s | TCP connect or the `identity` round trip exceeds it during Connecting |
| Response timeout | 30 s | A command's response frame does not arrive (headroom for a container upload in one frame) |
| Heartbeat interval | 5 s | Idle Connected cadence: the client sends `getStatus` — the existing command, no new machinery — and a missing answer is a heartbeat miss |
| Heartbeat tolerance | 2 consecutive misses | The second miss is a transport error and starts Reconnecting |

### Server-side failure codes

New V-codes in `compiler/vm-cli/resources/problem-codes.csv` (next free
V6012, IO category, exit code 2), following the CSV → codegen lifecycle of
`specs/steering/problem-code-management.md`:

| Code | Name | Message | Raised when |
|------|------|---------|-------------|
| V6012 | TcpListen | Unable to listen on the configured TCP address | The device cannot bind the configured address/port |
| V6013 | SessionFraming | A received frame is malformed | Bad length prefix, a frame over 16 MiB, or non-UTF-8 frame bytes; the connection closes |

A JSON line inside a well-formed frame that does not parse as a command
stays a codeless codec error per ADR-0055 — framing and codec are
different failures: framing breaks the stream (V6013, session ends), codec
breaks one message (null `vCode` error line, session continues). The
existing V6011 covers stdio session I/O; V6013 is its TCP-framing
counterpart.

New client-side E-codes (this mechanism's share of the table):

| Code | Name | Raised when |
|------|------|-------------|
| E0013 | ConnectFailed | Opening the transport or the `identity` handshake fails (Connecting → Disconnected); the context carries the server's V-code when one arrived |
| E0014 | ConnectionLost | The reconnect sequence is exhausted (Reconnecting → Disconnected) |

## Mechanism 3 — Start Build

The pipeline already exists end to end; this mechanism wires a Build
button onto it and reports its phases on the device panel.

What exists today:

- **Compile → container.** `ironplcc compile <input> -o <output>`
  (`compiler/ironplc-cli/bin/main.rs:355`, implementation
  `compiler/ironplc-cli/src/cli.rs:95`). The editor builds the argument
  vector in one shared helper (`integrations/vscode/src/taskProviderLogic.ts:8`,
  project-wide form `:15`) and the hot-edit session already compiles a
  source before serving it (`integrations/vscode/src/hotEdit.ts:357`).
- **Upload.** `acceptEdits` carries the compiled container bytes to the
  host over the session (`compiler/runtime/src/commands.rs:36`), staged by
  `execute` (`compiler/runtime/src/commands.rs:287`); the client side is
  `HotEditSession.acceptEdits`
  (`integrations/vscode/src/hotEditSession.ts:290`).
- **Verify.** The container verifies at load — hash recomputation in
  `compiler/container/src/load_verify.rs:282` — and the host compares the
  candidate's `layout_hash` against the active artifact before staging
  (`compiler/runtime/src/online_change.rs:30`,
  `compiler/runtime/src/host.rs:164`). A mismatch or an undecidable
  migration is a coded refusal (V4007–V4010,
  `compiler/runtime/resources/problem-codes.csv`), not a silent deploy.
- **Run control.** The hot-edit FSM commands apply or revert the upload at
  a scan boundary: `testEdits` / `untestEdits` / `assembleEdits` /
  `cancelEdits` (`compiler/runtime/src/commands.rs:49`), with the serve
  session driving the boundary round before acknowledging
  (`compiler/vm-cli/src/serve.rs:78`, REQ-VC-vm-cli-023). `getStatus`
  reads back the running generation.

### Two build modes

The same compile feeds two build modes:

- **Offline build.** `ironplcc compile` writes the container artifact
  (`compiler/ironplc-cli/src/cli.rs:95`) with no connection; the local
  `run`/`serve <FILE>` path loads that artifact through the same
  `load_container` call (`compiler/vm-cli/src/serve.rs:31-33`), and CI
  consumes the same output. Hot edit does not participate.
- **Online build.** The same compiled bytes delivered over the session:
  `acceptEdits` (staging, load-verify, `layout_hash` comparison) then
  `testEdits` (trial) or `assembleEdits` (promote) — the online build is
  the hot-edit FSM flow, so no separate deploy command set exists. An
  initial deploy onto an empty device is the same sequence, as the
  wiring below notes.

The only difference between the modes is the delivery envelope: compile
and the container bytes are identical (DRY).

The design adds only the wiring and the reporting:

1. **Build button → existing commands.** With the profile in Connected,
   Build runs: compile locally (the shared helper), send the bytes with
   `acceptEdits` (with the ADR-0061 migration decisions when the host
   answers V4010), then `assembleEdits` to promote — or `testEdits` when
   the engineer chose trial-run. A full initial deploy onto an empty
   device is the same `acceptEdits` → `assembleEdits` sequence; the
   host's staging path makes no distinction that would need new commands.
2. **Build status on the device panel.** The panel derives its phases
   from the client's own position in the sequence — compiling (local
   compile running) → uploading (awaiting the `acceptEdits` response) →
   verifying (inside the same response: load-verify and the `layout_hash`
   comparison are host-side work before the response is written) →
   running (the post-assemble `getStatus` shows the new generation). No
   server change feeds these phases; each is a client-side rendering of
   one existing call.
3. **Errors surface existing codes.** Compile failure is E0006; a staging
   refusal is the V4007–V4016 block rendered as `V#### - message`; a
   transport fault mid-build is the Mechanism 2 state machine
   (E0013/E0014).

**Gap (noted, not designed around):** the upload rides one frame, so a
large container gives no incremental progress — the panel shows an
indeterminate "uploading" until the response arrives. A push channel or
chunked upload is the documented follow-up if this becomes visible; the
HA Engineering UI Contract already defers push channels the same way, and
ADR-0055 records the 3.3x JSON-bytes size note that would motivate
chunking.

## Integration Map

Everything reused, with its one authority:

| Mechanism piece | Existing call |
|-----------------|---------------|
| Session protocol (commands, codec, dispatch) | `compiler/runtime/src/commands.rs:32`, `:270`, `:275`, `:284` |
| stdio session server | `compiler/vm-cli/src/serve.rs:67` |
| Codec error line (codeless, ADR-0055) | `compiler/vm-cli/src/serve.rs:137` |
| Client session (request FIFO, response parsing) | `integrations/vscode/src/hotEditSession.ts:258`, `:118`, `:90` |
| Transport seam the TCP transport implements | `integrations/vscode/src/hotEditSession.ts:238` (stdio impl: `integrations/vscode/src/hotEdit.ts:460`) |
| Device-panel application state | `compiler/runtime/src/commands.rs:136` (`StatusPayload`) |
| Compile | `compiler/ironplc-cli/src/cli.rs:95`; arg vector `integrations/vscode/src/taskProviderLogic.ts:8` |
| Upload | `compiler/runtime/src/commands.rs:36` (`AcceptEdits`) |
| Verify | `compiler/container/src/load_verify.rs:282`; `compiler/runtime/src/online_change.rs:30` |
| Run control | `compiler/runtime/src/commands.rs:49`; boundary round `compiler/vm-cli/src/serve.rs:78` |
| V-code registries | `compiler/vm-cli/resources/problem-codes.csv` (V6012+), `compiler/runtime/resources/problem-codes.csv` (untouched) |
| E-code registry and rendering | `integrations/vscode/resources/problem-codes.csv` (E0010+), `integrations/vscode/src/problems.ts:30` |
| Heartbeat | `getStatus` — `compiler/runtime/src/commands.rs:286` |
| Redundancy block vocabulary | [HA Redundancy FSM](ha-redundancy-fsm.md) (epoch, SYNC/CONTROL) |

## Out of Scope

- Wire authentication and encryption: v1 sends no credential over the
  session; the profile model and the auth-block UI land now, the
  transport-level authentication decision follows (ADR-0063).
- Production code: this spec and ADR-0063 are design only; the CSV rows,
  the `identity` command, the TCP listener, and the profile UI are the
  implementation that follows.
- A push/notification channel (deferred with the HA Engineering UI
  Contract); the build-progress gap above records the one place v1 feels
  its absence.
- Multi-device simultaneous sessions: one active connection per
  workspace.
