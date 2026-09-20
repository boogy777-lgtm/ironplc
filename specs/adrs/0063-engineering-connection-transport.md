# Engineering Connection Transport: One Session Protocol over stdio and TCP

status: accepted
date: 2026-09-20

## Context and Problem Statement

The desktop IDE connects to a controller in two situations: a local VM the
editor spawns itself, and a remote device on the control network. The
hot-edit protocol already answers how a client talks to a served runtime
(ADR-0052, ADR-0055), but it only exists over child-process stdio, and
nothing defines how a connection is configured, how the client learns what
it connected to, or how a build reaches the device. How should the
engineering connection work: one protocol per transport or one protocol
with two envelopes, where do credentials live, and how much of build is
new machinery?

## Decision Drivers

* One protocol surface per ADR-0055's driver: two transports must not
  become two dialects of one session; the clients stay thin serializers.
* The existing wire behavior stays under test where it lives: the command
  vocabulary, the codec, and the one-message-in/one-message-out ordering
  are already pinned beside the runtime and `serve`.
* Secrets never sit in configuration files: a settings file syncs, gets
  committed, and is read by other extensions.
* Build is mostly shipped: compile, upload, verify and run control all
  exist; a design that invents a parallel build path duplicates knowledge
  the pipeline already owns.
* The reference UI ships now: the auth block needs a credential *model*
  even though the wire authentication decision is not yet made.

## Considered Options

* **A new binary TCP protocol beside the stdio JSON session.** Rejected:
  two protocols for one session, two error-vocabulary drifts, and a second
  wire format to pin — the same failure mode ADR-0055 rejected for
  per-client protocols.
* **TCP carrying the JSON session inside a length-prefixed frame
  (chosen).** The frame replaces only the newline delimiter; the JSON
  values and the one-in/one-out ordering are byte-identical on both
  transports, so the command layer and the client session logic serve
  both without a branch.
* **Credentials in the settings profile.** Rejected: plaintext secrets in
  a synced, committable file. The profile stores a secret-store key; the
  host's secret store (VS Code `SecretStorage` for the extension) holds
  the username/password pair.
* **Application-level login command in v1.** Deferred: no session command
  carries a credential today. The profile model and the auth-block UI land
  now; transport-level authentication (TLS or an equivalent) is a later
  decision that the `credentials` key already points at.
* **A dedicated build/deploy command set.** Rejected: `acceptEdits`
  already uploads and stages container bytes, load-time verification and
  the `layout_hash` comparison already verify, and the hot-edit FSM
  commands already control what runs. Build wires a button onto that
  sequence and reports its phases.

## Decision Outcome

Chosen option: **dual transport — spawned stdio and remote TCP — carrying
one session protocol**, with connection profiles in settings, secrets in
the host secret store, an `identity` handshake, and build as pure reuse.

* **One protocol.** The ADR-0055 line-delimited JSON command protocol runs
  unchanged over stdio (newline-delimited) and TCP (4-byte little-endian
  length prefix framing one JSON line). A framing failure is a new
  transport error (V6013), distinct from the codeless per-message codec
  error.
* **Connection profile.** Name, transport, and per-transport fields
  (`program` for stdio; `address`/`port` for TCP) persist in editor
  settings; `credentials` holds only a secret-store key. Client-side
  validation fails before any transport opens, with E0010–E0012 codes.
* **`identity` handshake.** The first command on every (re)opened
  transport; it returns the device panel fields (name, model,
  modification, firmware version), the application-state snapshot, and an
  optional redundancy block (epoch, SYNC/CONTROL). The field set grows by
  optional fields; an old server's codeless refusal of `identity` is the
  version negotiation.
* **Client connection state machine.** Disconnected → Connecting →
  Connected, with Connected → Reconnecting on transport fault: bounded
  retries (5) under exponential backoff with jitter, then Disconnected
  (E0014). Timeouts: 5 s connect, 30 s response, a 5 s idle heartbeat
  reusing `getStatus`.
* **Build is reuse.** The Build button drives compile → `acceptEdits`
  (upload) → host-side verify → `assembleEdits`/`testEdits` (run control)
  and renders the phases client-side; the only recorded gap is
  incremental upload progress, deferred with the push-channel decision.
  One compile serves both build modes — offline (the artifact for local
  run and CI) and online (the same bytes over the session); only the
  delivery envelope differs.

### Consequences

* Good, because both transports share one tested wire behavior, one client
  session implementation, and one error vocabulary.
* Good, because the profile schema and the auth-block UI land once; the
  deferred wire-authentication decision changes the transport, not the
  settings model or the UI.
* Good, because build adds no server machinery: the phases are the
  client's own position in a sequence of existing calls.
* Neutral, because the TCP listener and framing live in `vm-cli` (its
  V6xxx IO-code registry gains V6012/V6013), while the command vocabulary
  stays in `ironplc-runtime` untouched except for `identity`.
* Bad, because a length-prefixed frame cannot reuse the stdio line reader
  byte-for-byte: the transport gains a small framing codec, the one new
  piece of wire machinery this decision admits.

## More Information

* ADR-0052 — the host protocol the session drives; ADR-0055 — the command
  layer and codec both transports carry.
* [Engineering Connection spec](../design/engineering-connection.md) —
  profile schema, handshake messages, the state machine, timeout and
  reconnect policies, the problem-code tables, and the integration map.
* [HA Engineering UI Contract](../design/ha-engineering-ui.md) — the
  one-vocabulary, thin-clients pattern this connection extends.
* `compiler/vm-cli/src/serve.rs` — the stdio session; V-code registry
  `compiler/vm-cli/resources/problem-codes.csv`.
