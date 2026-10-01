# Hot-Edit Command Layer in `ironplc-runtime`

status: accepted
date: 2026-09-15

## Context and Problem Statement

The runtime host implements the P0 online change protocol end to end
(ADR-0052): stage, test, untest, assemble, cancel, and a status snapshot, with
declaration-level state migration behind it (ADR-0054). All of that lives
behind a Rust API in `compiler/runtime`. Three clients need to drive it from
outside the process — the `ironplcvm` CLI in a served session, the MCP server
through tools, and the VS Code extension — and each would otherwise invent its
own request shapes, payload encoding and error strings. How should the protocol
be exposed to external clients: where does the command layer live, what does it
carry, and how do its failures reach users?

## Decision Drivers

* One protocol surface for every client; the clients stay thin serializers
  and the runtime crate keeps its current dependency surface (vm + container).
* The payload is compiled state, not source: compilation stays the client's
  job through the existing compile calls, so the runtime never grows a
  parser or analyzer dependency.
* Errors are user-facing and stable: every online change or migration
  refusal must carry a documented V-code in the established
  `V#### - message` form, without disturbing the VM's `Trap`-driven code
  tables.
* The layer adds no state machine of its own: the host's controller FSM
  remains the only protocol authority.

## Considered Options

* **Per-client protocol implementations** (CLI, MCP and VS Code each define
  their own commands over `RuntimeHost`). Rejected: three dialects of one
  protocol, three error-vocabulary drifts, and no single place to test the
  wire behavior.
* **Command layer in `vm-cli` behind the `ironplcvm` binary.** Rejected: the
  MCP server and the playground would depend on the CLI crate to reach the
  host, and the protocol would be hostage to one client's packaging.
* **Command layer in `ironplc-runtime` (chosen).** The crate already owns the
  protocol state; the new module exposes it as plain serde-serializable
  request/response types with a line-delimited JSON codec, so any transport
  (stdio, MCP, extension host) can drive the same typed surface.

## Decision Outcome

Chosen option: **a command layer in `ironplc-runtime`** (`src/commands.rs`):
externally tagged, serde-serializable `Command` and `Response` enums plus
`parse_command`/`render_response` free functions that put one command and one
response on one line each.

* **Commands.** `GetStatus`, `AcceptEdits`, `TestEdits`, `UntestEdits`,
  `AssembleEdits` and `CancelEdits`, dispatched by one `execute` function that
  maps each command onto the matching `RuntimeHost` method. The layer keeps no
  state and performs no validation the host does not already perform.
* **Container-bytes payload.** `AcceptEdits` carries the compiled container
  as raw wire-format bytes (a JSON array on the line). The runtime accepts
  only what the client compiled; it never sees IEC source.
* **Status payload.** `GetStatus` answers with a snapshot mirroring
  `HostStatus`: the mode, the logic generations of the active, normal and
  candidate artifacts, the application generation, whether the candidate is a
  migration candidate, and the completed scan rounds.
* **V-code range V4007+.** Online change and migration errors take stable
  user-facing V-codes from V4007 upward, registered in the runtime crate's
  own `resources/problem-codes.csv` and surfaced by the command layer's error
  type as `V#### - message`. The VM's `Trap` tables and its CSV stay
  untouched: trap codes remain owned by `compiler/vm`, CLI IO codes by
  `compiler/vm-cli`, and host protocol codes by `compiler/runtime`. A host
  invariant violation is not reachable through the command vocabulary and
  keeps no code of its own; if one becomes reachable it takes a V90xx code.

### Consequences

* Good, because every client drives one typed, tested protocol surface and
  the wire format is under test beside the host it exposes.
* Good, because the runtime crate's dependency surface grows only by serde
  and serde_json, both already workspace members.
* Good, because host protocol codes live in their own CSV, so the VM's
  generated `Trap::v_code`/`exit_code` mapping is not disturbed.
* Neutral, because a malformed command line has no V-code; it is a codec
  error the serving transport reports, not an online change refusal.
* Bad, because the container bytes ride as a JSON array of numbers, which is
  roughly 3.3x the binary size on the wire; a base64 encoding is the
  documented follow-up if that becomes visible.

## More Information

* ADR-0052 — the host-level online change protocol the commands expose.
* ADR-0054 — the state migration rules the accept and untest commands
  surface.
* `compiler/runtime/src/commands.rs`; acceptance tests in
  `compiler/runtime/tests/commands_acceptance.rs`.
* V-code registry: `compiler/runtime/resources/problem-codes.csv`.
