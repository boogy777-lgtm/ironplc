# HA Phase 5 slice 1: execution permit latch + redundancy crate skeleton

Date: 2026-09-21
Status: draft
Branch: feature/ha-redundancy-permit-latch

## Goal

Land the foundation of the Phase 5 redundancy layer in one PR: (1) the
runtime execution-permit latch (minimal seam 1 of
`specs/design/ha-redundancy-layer-architecture.md`) inside `RuntimeHost` —
the host boots unpermitted, `run()` refuses unpermitted scans with a coded
refusal, and the boundary is the single re-check point for a revoked permit
(external FSM review, takeaway 1 / scenario T07); (2) the
`ironplc-redundancy` workspace crate exposing the admission-verdict permit
policy (Standalone/Primary grant, Secondary refuses); (3) standalone
composition roots (`ironplcvm serve`/TCP, the MCP hot-edit session) grant
the permit at startup; (4) V4018 in the runtime's problem-codes CSV with
its docs page; (5) a one-line roadmap Phase 5 status update. Pair link,
crossload, fencing, calibration, UI: explicitly out of scope.

## Architecture

Policy/mechanism split per "Shell, Not Runtime +1"
(`specs/design/ha-redundancy-layer-architecture.md`):

- **Mechanism (enforcement)** lives in `RuntimeHost`: a private permit
  latch, `permit_execution()` / `revoke_execution_permit()` grant APIs, and
  the refusal inside `run()` — bypass is impossible because the check is at
  the single authority that owns the execution lifecycle, not at callers.
  The boundary application of a pending swap re-validates the permit and
  cancels the operation terminally when the permit was revoked between the
  request and the boundary (external-fsm-review.md, Concrete Takeaways 1).
- **Policy (when to grant)** lives in `ironplc-redundancy`: the
  `AdmissionVerdict` (Standalone | Primary | Secondary) and `permit_for`,
  the one place a verdict maps onto the host's grant API. Admission
  (discovery) is a documented later seam; the verdict shape is the minimal
  honest API today.
- **Code placement:** the unpermitted-run code is V4018 in the *runtime's*
  `resources/problem-codes.csv` (next free V40xx), not the proposed HA
  V41xx block: the refusal is emitted by `RuntimeHost`, the runtime crate
  cannot depend on the redundancy crate (dependency direction redundancy →
  runtime), and the V41xx crate-local CSV is assigned to the redundancy
  command vocabulary (`ha-engineering-ui.md`) that lands with the
  engineering surface. The runtime's hot-edit codes stay untouched.
- `ironplcvm run` drives the VM directly (`cli.rs`) and owns no
  `RuntimeHost`, so there is no permit to grant there; `serve` (and its TCP
  transport) grants in `start_host`, the MCP session grants in `establish`.

## File map

- `compiler/runtime/resources/problem-codes.csv` — V4018 row.
- `compiler/runtime/build.rs` — generated module renamed to the crate-wide
  `problem_codes` (the CSV now covers more than online change).
- `compiler/runtime/src/lib.rs` — crate-wide `problem_codes` module.
- `compiler/runtime/src/error.rs` — `RuntimeError::NotPermitted` +
  `v_code()` accessor.
- `compiler/runtime/src/host.rs` — permit latch, grant/revoke APIs,
  refusal in `run()`, boundary re-validation in `apply_pending_swap`.
- `compiler/runtime/src/commands.rs` — use `crate::problem_codes`.
- `compiler/runtime/tests/**`, `compiler/vm-cli/src/serve.rs` (tests),
  `compiler/mcp/src/tools/hot_edit.rs` (tests) — grant in fixtures.
- `compiler/vm-cli/src/serve.rs` — grant in `start_host`; handle
  `NotPermitted` in `drive_scan_round`.
- `compiler/mcp/src/tools/hot_edit.rs` — grant in `establish`; handle
  `NotPermitted` in `runtime_error`.
- `compiler/ironplc-redundancy/` — new crate: `Cargo.toml`, `src/lib.rs`,
  `src/admission.rs` (verdict + policy + unit tests).
- `compiler/Cargo.toml` — workspace member.
- `docs/reference/runtime/problems/V4018.rst` — docs page.
- `specs/roadmap.md` — Phase 5 delivered line.

## Tasks

1. Permit latch in `RuntimeHost` + V4018 code + `v_code()`.
2. Grants at the composition roots (serve/TCP start_host, MCP establish)
   and in all test fixtures/helpers that construct a host.
3. Tests: unpermitted run refused with the exact code; granted run
   executes; revoked-before-boundary cancels the pending swap terminally
   (T07); shell policy unit tests.
4. `ironplc-redundancy` crate skeleton + admission verdict policy.
5. Docs (V4018 page, roadmap line); full gates (`cd compiler && just`);
   remove this plan before merge.

## Authorities

- `specs/design/ha-redundancy-layer-architecture.md` (Minimal Seams 1,
  Shell-Not-Runtime+1, sequencing step 3)
- `specs/design/external-fsm-review.md` (takeaway 1: commit-time
  re-validation; readiness seam 1)
- `specs/design/ha-redundancy-fsm.md` (admission verdicts)
- `specs/design/ha-engineering-ui.md` (V41xx block assignment)
- ADR-0064/0065 (pair implications, single session)
