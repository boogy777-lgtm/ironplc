# Engineering connection transport: identity handshake, TCP listener, client transport (ADR-0063/0065)

Date: 2026-09-21
Status: draft
Branch: feature/engineering-connection-transport

## Goal

Land Slice B of the engineering connection in one PR: (1) the `identity`
command in the runtime command layer, answered from the host state plus the
application-composed device panel (ADR-0063 handshake); (2) the `ironplcvm
serve --listen <addr:port>` TCP transport in vm-cli — the same `serve_session`
loop inside a 4-byte little-endian length-prefix frame, one coded refusal
line (V6014 `SessionRefused`) for a second connection while one session is
active (ADR-0065 exclusivity), V6013 `SessionFraming` on the wire for framing
violations and the 30 s per-message read timeout; (3) the client-side
`TcpLineTransport` in the VS Code extension implementing `HotEditTransport`,
exported but not wired into commands/UI (Mechanism 1 is a later slice); (4)
reconciliation of the problem-code table in
`specs/design/engineering-connection.md` to the real allocation (V6012 is the
persist failure from the ADR-0064 PR; framing is V6013; refusal is V6014).

## Architecture

- **One dispatch authority.** `execute` keeps its role as the whole command
  mapping and gains the application-composed `&DeviceIdentity` parameter —
  the type system then requires a device block wherever commands execute, and
  an identity answer without device values is unrepresentable. The host
  supplies the `application` block via `host.status()` (the existing
  `StatusPayload` vocabulary, ADR-0064 additive optional fields included).
  `Response::Identity(IdentityPayload)` carries `protocol: 1`
  (`SESSION_PROTOCOL_VERSION`), the `device` block, the `application`
  snapshot, and the optional `redundancy` block (shape only — always absent
  today, the standalone answer). Version negotiation stays exactly the
  ADR-0063/ADR-0055 behavior: an old server answers `{"command":"identity"}`
  with the codeless codec error (unknown command variant), verified by the
  existing unknown-command tests, kept untouched.
- **Same session loop, one new piece of wire machinery** (the ADR-0063
  consequence). `serve_session` is reused byte-for-byte: a `FrameReader<R:
  Read>` implements `BufRead` by presenting each frame's payload plus a `\n`
  sentinel, and a `FrameWriter<W: Write>` frames the buffer on `flush()`
  (stripping the `writeln!` newline), so the existing loop over
  `reader.lines()` / `writeln!` serves TCP verbatim. A frame is 4-byte LE
  length + exactly that many bytes of one UTF-8 JSON line (no trailing
  newline); a declared length over 16 MiB, a payload that is not one line
  (raw `\n`), non-UTF-8 bytes, or EOF mid-frame is a framing violation →
  io error carrying a `FramingError` marker. Clean EOF at a frame boundary
  ends the session, as does EOF on stdio.
- **Exclusivity is a mechanism at the accept loop** (ADR-0065 decision 2).
  `serve_tcp` boots host + slot store once (the A/B store and the host's
  trial state survive a session loss, per the design spec) and serves
  connections sequentially. While one session is active, the next accepted
  socket receives exactly one framed `V6014` error line and is closed — the
  single-writer rule needs no lock token because there is no second session.
  Framing violation or read timeout on the active session → one framed V6013
  error line, that connection dropped, listener survives. Read timeout is 30 s
  per message; `drive_scan_round` and the assemble persist semantics are
  identical to stdio (same `serve_session`, same store wiring).
- **Failure codes.** `compiler/vm-cli/resources/problem-codes.csv` gains
  V6013 `SessionFraming` and V6014 `SessionRefused` (V6011 `SessionIo` covers
  listen/accept I/O failures; V6012 stays the ADR-0064 persist failure).
  Wire messages for V6013/V6014 are free text next to the stable code, the
  `codec_error_line` pattern. Docs per the steering lifecycle:
  `docs/reference/runtime/problems/V6013.rst`, `V6014.rst`.
- **Client transport.** `TcpLineTransport` (new vscode-free module
  `integrations/vscode/src/tcpTransport.ts`) mirrors `StdioLineTransport`'s
  shape: `sendLine` frames one line, socket `data` is parsed into frames
  with the same 16 MiB cap, `onLine`/`onExit` listener arrays, `dispose()`
  destroys the socket. A `connectTcpLineTransport(host, port, timeoutMs)`
  factory resolves on connect and rejects on error/timeout; unit tests drive
  a real `net` listener on `127.0.0.1:0`. Not referenced from `hotEdit.ts`
  or commands — the Mechanism-1 profile wiring is a later slice.

## File map

- `compiler/runtime/src/commands.rs` — `Command::Identity`,
  `DeviceIdentity`, `RedundancyIdentity`, `IdentityPayload`,
  `SESSION_PROTOCOL_VERSION`, `Response::Identity`, `execute` device param;
  serde round-trip tests.
- `compiler/runtime/src/lib.rs` — re-exports.
- `compiler/runtime/tests/commands_acceptance.rs` — call sites take the test
  device block; identity round-trip acceptance test.
- `compiler/mcp/src/tools/hot_edit.rs` — `execute` call sites take the MCP
  device block ("ironplcmcp" soft device); unreachable `Response::Identity`
  arm maps to `internal_failure`.
- `compiler/vm-cli/resources/problem-codes.csv` — V6013, V6014 rows.
- `compiler/vm-cli/src/serve.rs` — `serve_session` device param; `serve`
  composes the real device block (name `ironplcvm`, model
  `IronPLC SoftPLC`, modification `vm-cli`, firmware
  `env!("CARGO_PKG_VERSION")`); identity round-trip test.
- `compiler/vm-cli/src/tcp.rs` (new) — frame codec (`FrameReader`,
  `FrameWriter`, `FramingError`), `write_error_frame`, `serve_tcp` accept
  loop; codec unit tests (split reads, partial frames, oversized cap).
- `compiler/vm-cli/src/main.rs` — `serve --listen <ADDR>`.
- `compiler/vm-cli/tests/cli.rs` — scripted TCP e2e: identity →
  accept/test/assemble → second connect refused with V6014 → first client
  still served → garbage gets V6013 and drops → reconnect gets a fresh
  session.
- `integrations/vscode/src/tcpTransport.ts` (new) — `TcpLineTransport` +
  factory, exported for later profile wiring.
- `integrations/vscode/src/test/unit/tcpTransport.test.ts` (new) —
  ephemeral-listener tests: framing round-trip, split frames, oversized
  length, refusal line + close, dispose.
- `specs/design/engineering-connection.md` — problem-code table rows to the
  real allocation; Mechanism-2 framing text (V6013 = framing only, refusal
  V6014 added); no other redesign.
- `docs/reference/runtime/problems/V6013.rst`, `V6014.rst` (new) — the
  steering problem-code lifecycle.
- `specs/roadmap.md` — one-line Phase 6 note marking the transport/listener
  slice delivered.

## Tasks

1. Runtime: identity command + payload types + execute param; unit and
   acceptance tests; keep unknown-command codec tests untouched.
2. vm-cli: CSV rows, frame codec + `serve_tcp`, `--listen`, e2e scripted TCP
   test, codec unit tests.
3. MCP: call-site updates + the unreachable Identity arm.
4. VS Code: `TcpLineTransport` + factory + unit tests; eslint clean.
5. Docs: design-doc code table reconciliation, V6013/V6014 rst files,
   roadmap one-liner.
6. Gates: `cd compiler && just` green (coverage ≥ 85 %, clippy, fmt, dupes);
   vscode unit tests + eslint on changed files; remove this plan before
   merge.
