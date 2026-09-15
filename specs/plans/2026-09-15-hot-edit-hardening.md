# Hot edit hardening (Phase 4)

Date: 2026-09-15
Status: draft
Branch: feature/hot-edit-hardening

## Goal

Close the known hot edit gaps (roadmap Phase 4): container integrity
hashes, the load-time verifier (ADR-0006), FB field-level stable IDs,
type-changing migration policies, and a per-POU code artifact design.

## Tasks

### T1 — Integrity hashes + load-time verifier

- Populate the header's `content_hash` / `debug_hash` (today zero, issue
  #1583): `content_hash` = BLAKE3 over the code and constant sections;
  `debug_hash` = BLAKE3 over the debug section (exact section list fixed in
  the ADR). Loader verifies when a hash is nonzero; zero = legacy accept.
  The signature field stays zero — key infrastructure is out of scope;
  #1583 stays open for signatures.
- Implement the ADR-0006 load-time verifier in the container crate:
  variable table entries, stable variable IDs, FB/array descriptor
  consistency (indexes in bounds and distinct, name offsets valid, layout
  hash recomputes) with a structured diagnostic on violation.
- Regenerate vm-cli golden containers (header bytes change). ADR-0058.

### T2 — FB field-level stable IDs (format v6)

- New container sub-table: per-FB-type field UIDs. Codegen records them
  from `stable_var_ids` input; the sidecar format already supports it —
  key FB fields as (scope = qualified FB type name, name = field). The
  project's `declared_var_keys` / sync / map-uid flows cover FB fields
  with no format change to the sidecar.
- Migration: match FB instance data by FB type identity, then fields by
  UID; per-field copy/init/drop maps. Fail-closed when a field UID is
  unknown and the layout differs. Bump FORMAT_VERSION to v6, regenerate
  goldens. ADR-0059.

### T3 — Type-changing migration policies

- Policy table for value conversions (e.g. DINT -> REAL, INT -> DINT —
  widening and same-family only; same-size strings copy; arrays only with
  compatible element type and equal length). Stage-time rejection when a
  pair is outside the policy; conversion executes at swap. ADR-0060.

### T4 — Per-POU code artifacts (design only)

- `specs/design/per-pou-code-artifacts.md`: artifact layout, naming,
  loader changes, hot-edit interaction. No implementation; visibility of
  the FSM does not change.

### T5 — Finalize

ADR front matter/number checks, full gates (`cd compiler && just`, specs
gates via Git Bash, extension untouched expected), plan deletion, ff-merge
to `lint-fences`, push.

## Rules

No unsafe/panic/unwrap/expect/todo in non-test code; modules ≤1000 lines;
no new external dependencies; KISS — extend existing patterns (stage 2
migration planner, sidecar, problem-code lifecycle); tests beside code;
golden regen via the ignored `generate_golden_files` test on version bump.
