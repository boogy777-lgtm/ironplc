# Integrity Hashes and Load-Time Container Verification

status: accepted
date: 2026-09-15

## Context and Problem Statement

The container header has carried three 32-byte hash slots since the format
gained them, but only `layout_hash` was ever populated. `content_hash` and
`debug_hash` were written as zeros, nothing computed them, and no reader
validated anything — a corrupted or crafted container loaded as long as it
parsed (issue #1583 records the gap for hashes; issue #1582 records it for
verification). The VM therefore trusted bytes that ADR-0006 says must be
verified before execution: the loader is the trust boundary, and it enforced
nothing.

Hot edit raises the stakes. The runtime host stages candidate containers from
untrusted input and swaps them at a scan boundary (ADR-0052); the hash that
gates the swap, `layout_hash`, was the one hash with no integrity protection
of its own — a candidate whose directory or type section was tampered with
was parsed tolerantly and could be compared, and loaded, on false premises.

## Decision Drivers

* ADR-0006 requires that no unverified bytecode reaches the interpreter; the
  load-time checks decided here are the container-metadata half of that
  requirement (the stack-discipline half already exists as a compiler-side
  pass).
* ADR-0052 makes `layout_hash` the online-change gate, so the hash itself
  must be integrity-protected or the gate compares attacker-chosen values.
* Existing containers in the field carry zero hash fields and must keep
  loading; verification cannot be a format-version break.
* The debug section is optional and strippable; its verification must not
  make the executable content of a container unloadable.
* Signature infrastructure (keys, algorithms, key stores) is out of scope;
  the signature sections stay zero and issue #1583 stays open for them.

## Considered Options

* **Populate the hashes and verify them at load.** The writer computes
  `content_hash` (BLAKE3 over `type_section || constant_pool || code_section`)
  and `debug_hash` (BLAKE3 over the debug section); the reader recomputes and
  rejects a nonzero mismatch.
* **Hashes but no reader verification.** Writing hashes that nothing checks
  defends against nothing — the reader is the trust boundary, not the file.
* **Verify always, including zero hash fields.** Rejects every container
  written before this change; a format-version break for no security gain
  against containers that never claimed a hash.
* **Wire the verifier into `Container::read_from`.** Every loader — vm-cli,
  the runtime host, the MCP server, the playground — becomes fail-closed with
  no per-caller change. The alternative, a separate verify call each caller
  must remember, repeats the ADR-0006 mistake it exists to fix.

## Decision Outcome

Chosen option: populate both hashes, verify nonzero values at load in
`Container::read_from`, and accept zero values as legacy containers.

* **`content_hash` / `debug_hash` population.** `Container::write_to`
  serializes each section once, hashes the exact bytes written, and fills all
  three hash slots (`layout_hash` already did this). The in-memory header
  keeps zeros until serialized, preserving ADR-0052's hash contract.
* **Load-time verification lives in the container crate.** A new pass,
  `ironplc_container::verify_load`, checks the type-section invariants:
  variable table count against `num_variables`, FB type descriptor count
  against `num_fb_types`, no reserved variable flag bits, array variables
  referencing an existing descriptor, distinct FB type IDs and user FB type
  IDs, stable variable IDs in bounds and ascending, user FB descriptors
  referencing an existing function and a field range inside the variable
  table, defined array element type tags, and — closing the ADR-0052 gap —
  that `layout_hash` recomputes over the type section. Violations return
  `ContainerError::VerificationFailed(LoadViolation)`, a structured
  diagnostic naming the offending item.
* **Fatality follows the loading sequence.** A `content_hash` or
  `layout_hash` mismatch rejects the container (steps 9–10). A `debug_hash`
  mismatch discards the debug section, non-fatally (step 13), because debug
  info is optional and strippable. A zero hash field — a container written
  before this verification existed — is accepted as legacy, per the hash
  contract.
* **Deliberately unchecked.** An `FbInstance` variable's type ID is not
  matched against the descriptor tables: it may name a standard-library FB
  (TON, TOF, ...) whose descriptor the container does not carry. Bytecode
  operands checked against these tables (LOAD_VAR/STORE_VAR indices, FB field
  indices) remain the future bytecode-level half of the verifier.
* **Signatures stay zero.** `sig_section_offset`, `sig_section_size`,
  `debug_sig_offset` and `debug_sig_size` remain zero; no key infrastructure
  is introduced. Issue #1583 remains open for the signature sections, and the
  dual-signature design is unchanged.

### Consequences

* Good, because every loader is fail-closed by construction: a corrupted or
  crafted container is rejected where it is read, before any VM state exists.
* Good, because `layout_hash` — the online-change gate — can no longer be
  tampered with without the tampering being detected at load.
* Good, because legacy containers load unchanged; zero hash fields mean
  "never claimed a hash", not "claimed and failed".
* Good, because debug tooling cannot be tricked into displaying a patched
  debug section as if it were the compiler's output.
* Bad, because a hand-built container that was internally inconsistent (a
  variable table shorter than `num_variables`) now fails at load; the fixtures
  that did this were fixed rather than the check weakened, and the builder now
  populates `num_fb_types` to match.
* Neutral, because containers without a type section are checked only for the
  layout hash; table-based checks apply where the tables exist.

## More Information

* [ADR-0006](0006-bytecode-verification-requirement.md) — the verification
  requirement this implements the load-time half of.
* [ADR-0052](0052-online-change-performed-by-the-runtime-host.md) — the hash
  contract and the online-change gate that motivated protecting
  `layout_hash`.
* `specs/design/bytecode-container-format.md`, REQ-CF-container-029 and
  "Content Hash Scope"; the load-time checks in
  `compiler/container/src/load_verify.rs` and `Container::read_from`.
