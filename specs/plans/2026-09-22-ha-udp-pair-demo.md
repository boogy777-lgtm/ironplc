# Plan: Real UDP pair link + two-runtime hot-edit sync demo

## Goal

Bring up two real `ironplcvm serve` processes as an HA pair over
127.0.0.1 UDP and prove ADR-0064 hot-edit synchronization across them:
Accept crossloads the same CandidateGenerationId to the Secondary,
Test flips both selectors, Assemble is one transaction across the pair,
and a takeover mid-Test leaves the survivor executing the CANDIDATE.
Today the pair runs only in-process over the loopback binding
(`compiler/ironplc-redundancy/src/loopback.rs`), exercised by test-support
compositions (`tests/pair_link.rs`, `tests/crossload.rs`) that the crate
docs always intended to be replaced by the production shell.

## Architecture

- **Transport**: a real `NicPort` binding over `std::net::UdpSocket`
  (`compiler/ironplc-redundancy/src/udp.rs`). One datagram per frame, the
  same codec bytes as the loopback binding. No new dependencies.
- **Pair driver**: a production per-unit pair link
  (`compiler/ironplc-redundancy/src/pair_link.rs`, generic over `NicPort`)
  composing the crate's existing modules — `Liveness`, `SyncChart`,
  `Epoch`, `CrossloadReceiver`, admission discovery — plus the ADR-0064
  outbound pipeline (offer/state-update/cancel/untest/**assemble**
  notices). It replaces the test-support `Node`/`TestNode` compositions;
  the loopback binding stays the test vehicle (`PairLink<LoopbackPort>`).
- **Crossload codec**: add the missing `AssembleCandidate` notice
  (ADR-0064(h) is "(c)–(g)" today) and the receiver's assemble mirror.
- **Composition root** (`compiler/vm-cli`): `serve` grows
  `--ha-peer-bind`/`--ha-peer-peer`/`--ha-role`/`--ha-pair-id`. Pair mode
  serves the same JSON session, but the pair pump runs on a stdin-reader
  thread + `recv_timeout` loop so the link stays alive between commands
  (the Secondary pumps with no client at all). Promotion on confirmed
  peer death is the policy-layer modeling the existing tests use (fencing
  mechanism is a later slice).
- **Proof**: a two-process integration test spawns the real binary twice
  over reserved loopback ports, drives accept → test → assemble, asserts
  the ADR-0064 contract on both units, then kills the Primary mid-Test and
  asserts the survivor promotes executing the CANDIDATE.

## Prefactoring

- Extract `serve_session`'s per-line body into `handle_line` so the
  stdio/stdin-channel loops share one dispatch (runtime commands, HA
  commands, identity block, driven rounds, slot persistence, and the
  pair's post-command crossload hooks).
- Generalize the session's HA backend as a `Shell | PairLink` enum;
  `drive_scan_round`'s scan-commit seam feeds either.

## File map

- `compiler/ironplc-redundancy/src/udp.rs` — new: `UdpPort` (std UDP
  `NicPort`), unit tests (round-trip, silence/loss, peer rebind).
- `compiler/ironplc-redundancy/src/pair_link.rs` — new: `PairLink<P:
  NicPort>` driver, `PairLinkStatus` view, promotion policy, tests.
- `compiler/ironplc-redundancy/src/crossload.rs` — `AssembleCandidate`
  message + `assemble_candidate` receiver mirror + tests.
- `compiler/ironplc-redundancy/src/commands.rs` — `pair_status` payload
  constructor for `haStatus` in pair mode.
- `compiler/ironplc-redundancy/src/lib.rs` — module wiring + re-exports.
- `compiler/ironplc-redundancy/src/shell/mod.rs` — doc note only (the
  real pair driver is no longer a "later slice").
- `compiler/ironplc-redundancy/tests/pair_link.rs` — rewrite scenarios
  over `PairLink<LoopbackPort>` (delete `TestNode`).
- `compiler/ironplc-redundancy/tests/crossload.rs` — rewrite scenarios
  over `PairLink<LoopbackPort>` (delete `Node`), add the assemble
  transaction scenario.
- `compiler/vm-cli/src/ha_pair.rs` — new: pair-mode serve loop.
- `compiler/vm-cli/src/serve.rs` — `handle_line` extraction, `Ha` enum,
  pair hooks, `serve_pair` host boot reuse.
- `compiler/vm-cli/src/main.rs` — new flags.
- `compiler/vm-cli/tests/ha_pair.rs` — new: two-process e2e + takeover.
- `docs/how-to-guides/run-a-redundant-pair.rst` + index — demo snippet
  with the observed sync sequence; one-line `specs/roadmap.md` Phase 5
  note.

## Tasks

1. `udp.rs` binding + tests.
2. Crossload `AssembleCandidate` + receiver mirror + codec tests.
3. `PairLink` driver + status view + tests.
4. Replace `Node`/`TestNode` in the two integration suites; add the
   assemble scenario.
5. vm-cli: `handle_line` extraction, `Ha` enum, `ha_pair.rs` loop, flags.
6. Two-process e2e (accept/test/assemble assertions + takeover) and run
   it for real; capture the observed sequence.
7. Docs snippet + roadmap note.
8. Gates: `cd compiler && just`; specs gates if spec docs changed.
