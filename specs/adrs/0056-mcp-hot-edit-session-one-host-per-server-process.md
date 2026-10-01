# MCP Hot-Edit Session: One Host Session per Server Process

status: accepted
date: 2026-09-15

## Context and Problem Statement

The MCP server exposes the P0 online change protocol through six
`hot_edit_*` tools that wrap the runtime command layer (ADR-0055). Unlike
the analysis tools, the hot-edit tools are stateful: the protocol operates
on *the running application*, so the server process must own a
`RuntimeHost` somewhere. Where does that state live, how many sessions may
exist at once, and what resets them?

## Decision Drivers

* Every `hot_edit_*` call must act on the same running application an
  agent is engineering; two hosts would let `hot_edit_test` on one say
  nothing about the state `hot_edit_status` reads on the other.
* Restarting the server must return to a clean slate, exactly as
  restarting `ironplcvm serve` does.
* No session keying, table, or eviction without a demonstrated client
  need.

## Considered Options

* **Sessions keyed by program identity or client.** Rejected: no client
  need demonstrated, and it adds a table, an eviction policy, and
  cross-session drift for no current benefit.
* **One host session per server process (chosen).** The server keeps a
  single `HotEditSession` and every tool call acts on it.

## Decision Outcome

Chosen option: **one `HotEditSession` per MCP server process**, a single
`RuntimeHost` behind a mutex, held in `Arc<Mutex<HotEditSession>>` next to
the `ContainerCache`, following `cache.rs`'s sharing convention.

* The first `hot_edit_accept` establishes the session from the compiled
  sources: before it there is no running application, so `AcceptEdits`
  would have nothing to stage against.
* Later `hot_edit_accept` calls stage candidates against the established
  host through the command layer, so every refusal (V4007 layout, V4008
  schedule, V4013 already staged, ...) comes back with its stable V-code.
* Every tool other than `hot_edit_accept` refuses with a P8001 diagnostic
  while no session exists.
* Restarting the server resets the session, exactly as restarting
  `ironplcvm serve` — which also hosts exactly one running application
  per process — does.

## Consequences

* Good, because the state an agent reads is the state of the one running
  application, and restart semantics match the CLI serve session.
* Good, because the sharing convention reuses `cache.rs`'s
  `Arc<Mutex<..>>` pattern rather than inventing a new one.
* Neutral, because parallel sessions keyed by program identity remain a
  follow-up decision; `HotEditSession` is the one place to grow if a
  client ever needs them.

## More Information

* ADR-0055 — the command layer these tools wrap.
* `compiler/mcp/src/tools/hot_edit.rs` — the session type and its module
  documentation, beside `crate::cache::ContainerCache`.
