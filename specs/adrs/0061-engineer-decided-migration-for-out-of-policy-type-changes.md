# Engineer-decided migration for out-of-policy type changes

status: accepted
date: 2026-09-15

## Context

ADR-0060 admits six widening and same-family storage-class conversions and
rejects every other `(base, candidate)` pair at stage time (V4010,
`MigrationUnsupported`). A hard reject blocks legitimate edits: the engineer
changed the declaration on purpose, and the new code often expects the new
type's semantics, so forcing the old value forward is the dangerous option,
not the safe one.

Vendor practice shows the right answer is an explicit decision, not a rule:

- Rockwell Logix preserves the binary storage when the element size matches;
  the bits are reinterpreted and the value "may no longer be valid"
  (documented warning). A `DINT` 123 (`0x0000007B`) redeclared as `REAL`
  keeps the bits and reads as ~1.72e-43, not 123.0.
- Siemens TIA reinitializes actual values when the structure or data type
  changes.
- Schneider Control Expert reinitializes unlocated variables on an online
  type edit but preserves the binary value of located ones, with an
  unexpected-behavior warning.

The runtime must never guess (ADR-0054, ADR-0057); the engineering tool asks.

## Decision

For every shared-UID variable whose `(base, candidate)` storage-class pair is
outside the ADR-0060 policy, the migration planner reports **all** such pairs
in a structured error payload (`uid`, name, from-class, to-class,
`size_equal`) instead of rejecting on the first offender. The engineering
client presents one decision per variable:

- **initialize (default)** — the variable receives its candidate initial
  value, or zero. This is what an unchecked control means; it is the
  fail-closed default because it never fabricates an interpretation of old
  bytes.
- **preserve storage** — available only when base and candidate slots have
  equal width (the 32-bit family `I32`/`U32`/`F32`, the 64-bit family
  `I64`/`U64`/`F64`) and, for arrays, when element size **and** length are
  equal. The storage bytes are kept and reinterpreted under the candidate
  type (Rockwell semantics). An explicit opt-in; the client warns that the
  value may no longer be valid.

The wire command gains an optional decisions map
(`migration: { <uid>: "init" | "preserve" }`). Without decisions, staging
fails closed with V4010 naming every problematic pair. Decisions are
validated: an unknown UID, or a `preserve` decision on a size-mismatched
pair, is rejected. Strings follow their existing rules (same-size copy is
already preservation; a width change stays rejected).

The vocabulary is per-UID and may also override an ADR-0060 convertible pair
(`init` or `preserve` instead of `convert`); the automatic widening remains
the default and clients surface such pairs only on explicit request.

Decisions are per-edit and transient — they are not stored in the UID
sidecar (ADR-0057), which records identity only.

Decisions travel with the staged candidate payload, so a redundant peer
applies the identical migration plan; the schema generation commit is atomic
across nodes (elaborated in the HA phase).

Initialization and preservation are destructive with respect to the old
interpretation: untest is already forbidden after a schema edit (V4011) and
values never roll back.

FB instance fields follow the same mechanism via their field UIDs
(ADR-0059), replacing today's rejection of field retypes.

## Consequences

- The runtime keeps zero heuristics: no value crosses an out-of-policy edit
  unless a named engineer decision says so, and the default decision discards
  the value.
- The planner reports all offending pairs in one round trip; clients render a
  decision list instead of a single error.
- The command layer gains a decisions field; it is additive and backward
  compatible.
- V4010 carries a structured payload in addition to its message.

## Alternatives

- **More value-conversion pairs**: rejected — it silently invents semantics
  that vendors deliberately do not provide (Rockwell preserves bits; Siemens
  reinitializes). The ADR-0060 widenings stay because each is exact or
  standard-defined.
- **Preserve as the default**: rejected — an unchecked default must not
  reinterpret old bytes under a new type; initialization is the least
  surprising outcome.
- **Persisting decisions in the sidecar**: rejected — a migration decision is
  a one-shot act tied to a specific edit, not an identity fact.
