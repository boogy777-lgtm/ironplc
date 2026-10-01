# Design: Per-POU Code Artifacts

## Overview

This design splits the compiler's output from one monolithic container into a
set of per-POU code artifacts: one artifact per program organization unit
(POU) carrying the code-section entries that POU compiles to, one shared
artifact carrying everything else, and a JSON manifest that binds them. A
linker assembles the set into the exact byte stream a standalone
[Bytecode Container Format](bytecode-container-format.md) container produces
today.

The split is an engineering- and build-time granularity. The linked container
remains the only unit the VM loads, the runtime host stages, and the
online-change protocol swaps: the VM stays a pure execution kernel
([ADR-0010](../adrs/0010-no-std-vm-for-embedded-targets.md),
[ADR-0052](../adrs/0052-online-change-performed-by-the-runtime-host.md)),
and the controller FSM gains no states and no user-visible behavior
(roadmap Phase 4, "Per-POU code artifacts: design first; no user-visible FSM
change").

## Background

### What Exists

`project::compile` lowers all sources to one `dsl::Library`, and codegen
emits one container through `ContainerBuilder`: a 256-byte header, the task
table, the type section, the constant pool, the code section, and the debug
section, serialized back to back in a fixed order
([File Layout](bytecode-container-format.md#file-layout)). Every edit to any
POU — including a one-line body change — recompiles every POU and rewrites the
whole container. The runtime then hashes, verifies, stages, and swaps that
whole container at a scan boundary, so the size of the edit and the size of
the payload it forces are unrelated.

Three existing properties make a per-POU split cheap to define:

1. **Deterministic ordering.** The compiler assigns every numeric identity
   (variable indices, FB type IDs, function IDs, constant pool entries) from
   source-derived sort keys, so two compilations of the same declarations
   produce byte-identical shared sections regardless of which body changed
   ([Deterministic Ordering](bytecode-container-format.md#deterministic-ordering)).
2. **A section is a byte range.** Every section has a directory offset and
   size; the header's hashes are computed over the exact section bytes the
   writer writes ([Content Hash Scope](bytecode-container-format.md#content-hash-scope)).
   Concatenating per-POU entry bytes in the deterministic order reproduces the
   linked code section exactly.
3. **A JSON sidecar precedent.** The UID sidecar
   ([ADR-0057](../adrs/0057-stable-variable-uid-sidecar-persistence.md))
   already persists a deterministic, sorted, timestamp-free JSON table next to
   the project; the artifact manifest reuses that shape and its rules.

### What This Design Adds

```
IEC sources                compiler                   artifact set
───────────                ────────                   ────────────
main.st  ─┐                                    ┌─ manifest.json
util.st  ─┼─► parse ─► analyze ─► codegen ─► link │   (order + hashes)
gvl.st   ─┘                    │                 ├─ shared.bin
                               │                 │   (task table, type
                               ▼                 │    section, constants)
                        per-POU entries          ├─ main.code.bin
                        (deterministic IDs)      ├─ main.debug.bin
                                               └─ util.code.bin
                                                        │ link
                                                        ▼
                                              linked .iplc container
                                              (byte-identical to today's
                                               single-file output)
```

A POU-body edit recompiles one POU, rewrites that POU's artifact, and relinks.
The shared sections, the type section, the variable table, and every hash the
runtime compares are unchanged by construction.

## Detailed Design

### 1. Artifact Set Layout

An artifact set is a directory of files, one level deep, in this fixed shape:

| File | Content |
|------|---------|
| `manifest.json` | Manifest (see below). The only JSON file in the set. |
| `shared.bin` | The task table, type section, and constant pool, linked in container file order. Excludes the 256-byte header (its directory offsets and hashes are link outputs, not inputs) and excludes the code and debug sections. |
| `<pou>.code.bin` | The code-section entries — function directory records and bodies — for the functions this POU owns. |
| `<pou>.debug.bin` | The debug-section sub-tables this POU owns. Absent when the compilation carries no debug info. |

The code and debug sections of the linked container are the concatenation of
the per-POU artifacts in the manifest's listed order. An artifact set with no
per-POU artifacts is not valid: every compilation owns at least the
configuration's programs.

### 2. Manifest

The manifest binds the set. It mirrors the sidecar's discipline: fixed field
order, entries sorted, no timestamps, no environment-specific data, so equal
sets serialize byte-identically and diffs stay clean.

```json
{
  "version": 1,
  "format_version": 6,
  "pous": [
    {"name": "MAIN", "code": "main.code.bin", "debug": "main.debug.bin",
     "code_hash": "…", "debug_hash": "…"},
    {"name": "UTIL", "code": "util.code.bin", "code_hash": "…"}
  ],
  "shared_hash": "…"
}
```

Fields:

- `version` — manifest schema version, independent of the container
  `format_version`.
- `format_version` — the container format version the linked output targets;
  the linker rejects a set whose value the reader does not support.
- `pous` — one entry per POU, sorted by the POU's qualified name in the same
  case-insensitive UTF-8 byte order the deterministic ordering rules use.
  `code` names the POU's code artifact; `debug` is present only when the debug
  artifact exists. `code_hash` and `debug_hash` are BLAKE3 over the artifact
  bytes, lowercase hex.
- `shared_hash` — BLAKE3 over `shared.bin`, lowercase hex.

A missing or malformed manifest, a missing artifact, or any hash mismatch
rejects the whole set at load, in the same fail-closed spirit as the
load-time verifier ([ADR-0058](../adrs/0058-integrity-hashes-and-load-time-verification.md)):
the set degrades to rejected, never to partially linked.

### 3. Naming Rules

- **The set** takes the output stem: `<stem>.artifacts/`, the sibling of the
  UID sidecar's `<stem>.uids.json`. A directory input addresses the set by the
  directory name; a file input by the file stem — the same rule the sidecar
  uses, resolved through one function so every caller agrees.
- **Per-POU artifacts** take the lowercased qualified POU name plus the
  extension: `<pou>.code.bin`, `<pou>.debug.bin`. Lowercasing keeps the name
  unique on case-insensitive filesystems (Windows); IEC 61131-3 identifiers
  compare case-insensitively, so two POUs whose names differ only in case
  are the same POU — a compile error — not two artifacts.
- **The linked container**, when a single file is needed (a target upload, a
  bytes payload), keeps today's `<stem>.iplc` name. The linker produces it;
  nothing new is invented for the one-file form.

### 4. Linking

The linker is a serialization step, not a new code path. It reads the set,
checks the manifest hashes, concatenates the code and debug artifacts in
manifest order, and writes the container sections in the fixed file order with
the header computed exactly as today: `layout_hash`, `content_hash`, and
`debug_hash` over the linked section bytes, directory offsets and sizes
filled, signature slots left zero (issue #1583 unchanged). The in-memory
header keeps zeros until serialized, preserving the hash contract of
[ADR-0052](../adrs/0052-online-change-performed-by-the-runtime-host.md).

Linking is deterministic: `link(compile(P))` and `link(split(x))` for any
linked container `x = link(compile(P))` produce byte-identical output,
because every numeric identity and every sort order is already
source-derived. Relinking after a one-POU edit therefore changes exactly that
POU's bytes in the code section — which is precisely the "logic-only change"
the `layout_hash` already admits
([What counts as a "logic-only" change](bytecode-container-format.md#what-counts-as-a-logic-only-change)).

### 5. Loader Changes

The trust boundary does not move; it gains one door:

- **`ironplc_container` gains an artifact reader.** `read_artifact_set`
  validates the manifest and per-artifact hashes, then links and runs the
  existing `verify_load` checks and the nonzero-hash verifications of
  [ADR-0058](../adrs/0058-integrity-hashes-and-load-time-verification.md) on
  the linked bytes. A tampered artifact is rejected before any VM state
  exists, exactly as a tampered container is today. `Container::read_from`
  stays unchanged for standalone `.iplc` files.
- **Every loader accepts either form.** vm-cli, the runtime host, the MCP
  server, and the playground reach one verified `Container` through the same
  call, so no loader becomes a second, weaker path.
- **Codegen emits the set.** `compile_program` writes the manifest, the
  shared artifact, and one code/debug artifact pair per POU instead of one
  container file. The deterministic ordering pass is unchanged; the split is
  where its output is written, not what it assigns.

### 6. Hot-Edit Interaction

- **Swap granularity is unchanged.** The runtime host stages, validates, and
  swaps the linked container at a scan boundary
  ([ADR-0052](../adrs/0052-online-change-performed-by-the-runtime-host.md)).
  A swap always replaces the whole code section; the runtime never mixes POU
  bodies from two generations within one scan or across a buffer rebuild.
- **Transfer granularity improves.** The per-artifact hashes let a client
  diff an active application against a candidate set and transfer only the
  changed artifacts; linking happens at the host. This is a transport
  optimization outside the runtime protocol: the command vocabulary, the
  container-bytes `AcceptEdits` payload, the typed validation errors, and the
  V-codes of [ADR-0055](../adrs/0055-hot-edit-command-layer-in-ironplc-runtime.md)
  are untouched.
- **Migration is unchanged.** The migration planner
  ([ADR-0054](../adrs/0054-state-migration-across-declaration-level-edits.md),
  [ADR-0059](../adrs/0059-fb-field-stable-ids.md),
  [ADR-0060](../adrs/0060-type-changing-migration-policies.md)) consumes the
  type section and the stable-ID tables, which live in `shared.bin` and are
  identical to today's. A POU-body edit still yields a matching
  `layout_hash`; a declaration edit still rejects or migrates on the same
  evidence. The FSM — `stage`, `test`, `untest`, `assemble`, `cancel` — and
  the two generations stay exactly as they are; no state or transition
  becomes visible to users or clients.

## Non-Goals

- **No wire-format change.** The linked container is byte-identical to what
  the compiler emits today; `FORMAT_VERSION` stays 6 and the section order,
  header layout, and hash scope are untouched.
- **No VM change.** The VM still borrows one container and caller-owned
  buffers; there is no runtime dynamic linking, no per-POU loading, and no
  second code image.
- **No partial swap.** An online change always replaces the whole linked code
  section at a scan boundary; per-POU granularity never leaks into execution.
- **No protocol or FSM change.** The command layer, the host's controller
  FSM, the generations, and the V-code tables are unchanged; artifact-aware
  transfer compression is a client-side follow-up, not a protocol version.
- **No change to the UID sidecar.** The sidecar's format, sync, and map-uid
  flows ([ADR-0057](../adrs/0057-stable-variable-uid-sidecar-persistence.md))
  are untouched; the manifest borrows its discipline, not its table.
- **No signatures.** Signature sections stay zero; issue #1583 remains open
  for them.
- **Not a distribution format.** The set addresses one compiled application,
  not library packaging or a package manager.

## Testing Strategy

- **Link determinism.** `link(compile(P))` twice, and `link(split(x))` after
  a round trip, produce byte-identical containers.
- **Artifact round trip.** Split a linked container into a set and relink;
  the result equals the original bytes.
- **Verification parity.** A byte flipped in any artifact, a missing
  artifact, and a doctored manifest hash each reject at load, before VM
  state exists — the same fail-closed evidence the load-time verifier gives
  for containers today.
- **Hot-edit parity.** The existing migration and online-change acceptance
  suites run with candidates produced through the artifact path and assert
  unchanged behavior: equal `layout_hash` on body edits, the same
  copy/init/drop and conversion decisions, the same rejections.
- **Naming.** Case-colliding POU names produce one compile error, not two
  files; a directory input and a file input address the same set.
