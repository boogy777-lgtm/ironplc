# Engineer-decided migration (ADR-0061)

Date: 2026-09-15
Status: draft
Branch: feature/hot-edit-migration-decisions

## Goal

Out-of-policy type changes no longer hard-reject: the planner reports every
problematic pair, the engineer decides `init` (default) or `preserve`
(same-size only, Rockwell bits), and decisions ride the acceptEdits payload.

## Tasks

### T1 — runtime planner (compiler/runtime)

- `MigrationDecision::{Init, Preserve}`; `StateMigrationPlan::build_with_decisions(base, candidate, decisions: &BTreeMap<u64, MigrationDecision>)`; `build` delegates with an empty map (backward compatible).
- Out-of-policy pairs are collected (not first-fail): structured error carries all offenders `{uid, name, from, to, size_equal}`. Keep/rename `MigrationError::TypeChangeUnsupported` per crate conventions; add the payload fields.
- Per decision: `Init` → init action; `Preserve` → allowed only when slot widths equal (arrays: element size and length equal), planned as plain copy; invalid (unknown uid, preserve on size mismatch) → error.
- Decisions may also override an ADR-0060 convertible pair (init/preserve instead of convert); default stays convert.
- FB field retypes (ADR-0059) follow the same path via field uids.
- Tests: rejected-pair × decision matrix, all-pairs-reported, unknown uid, preserve size mismatch, acceptance for DINT→REAL init vs preserve (preserve reads the old bits reinterpreted, e.g. 123 -> ~1.72e-43), FB field retype with decision.
- Update the migration section of specs/design/bytecode-container-format.md; ADR-0061 already exists (refine only if implementation deviates).

### T2 — wire (runtime commands, vm-cli serve, MCP)

- `AcceptEdits` gains optional `migration: { <uid>: "init" | "preserve" }` (serde default empty).
- V4010 error responses carry the structured pair list (additive field; `CommandError` gains optional details).
- vm-cli serve passes through; MCP `hot_edit_accept` accepts decisions and surfaces the structured pairs; tests at both layers.

### T3 — VS Code

- On V4010 with pairs: decision UI (one checkbox per variable: unchecked = init default, checked = preserve, only `size_equal` rows checkable), warning text (value may no longer be valid, irreversible), resubmit acceptEdits with the decisions map.
- Unit tests with mocked transport; follow extension standards.

### T4 — Finalize

Gates (`cd compiler && just`, specs via Git Bash), plan deletion, ff-merge
to `lint-fences`, push.

## Rules

No new external deps; no unsafe/panic/unwrap/expect/todo in non-test code;
KISS — extend existing planner/command patterns; English.
