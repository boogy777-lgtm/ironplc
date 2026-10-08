# Plan: Selective Analysis and Per-POU Hot Edit

**Status of this file:** plan PR, never merged (development standards). No
tracking issue exists yet; open one before the first core change PR.

## Goal

Make the cost of a change depend on the size of the edit, not on the size of
the project, and make the end of that line a hot edit of one POU:

```
edit one POU -> analyze one body -> generate code for one POU
             -> one artifact -> swap on the controller
```

Today the first step is delivered (the lossless tree and its lowering) and
every later step recomputes the whole project. This plan opens the reuse
stages that [Parse-Tree Architecture](../design/parse-tree-architecture.md)
§3.3 reserves ("precompile model", "selective typification", "explicit
invalidation") and names hot edit as their consumer.

The plan does **not** introduce a query framework. Reuse is explicit, in the
CODESYS style: stored stage results, identity of their inputs, recorded
dependencies. The owner withdrew the query mechanism on 2026-10-01; this plan
keeps that decision.

### What "done" means

1. An edit of one POU body re-analyzes that body and the bodies that depend on
   what changed, and nothing else.
2. The result of a selective run equals the result of a full run on the same
   snapshot: same diagnostics, and a container that is byte-identical.
3. A body-only edit never changes the state layout of the container, so hot
   edit accepts it as a code swap with test and untest.
4. The full recomputation stays available as the control path.

## Where we are (verified 2026-10-06, `main` = `4d662a0f7`)

| Fact | Evidence |
|---|---|
| An unchanged user file is not parsed again | `compiler/sources/src/source.rs:91` (`Source::library` caches the AST or the error) |
| A changed file is parsed and lowered in full; the tree is dropped after lowering | `compiler/parser/src/frontend.rs:102-112` |
| Compatibility libraries are read from disk and parsed on every analysis | `compiler/sources/src/project.rs:74-85`, `compiler/sources/src/libraries/mod.rs:338` |
| Every analysis merges all inputs into one `Library` by cloning them | `compiler/analyzer/src/stages.rs:172-175` |
| Every pass clones the whole merged library before it runs (15 passes, plus one more clone) | `stages.rs:119`, `:142`, `:306` |
| The standard type environment is built twice, the function environment once, per analysis | `stages.rs:178`, `:300`, `:184` |
| Highlighting tokenizes the document again on each request | `compiler/ironplc-cli/src/lsp.rs:337` |
| Code generation compiles only functions that some body reaches; a compiled function has slots in the variable table | `compiler/codegen/src/compile.rs:327-341`, `:984-997` |
| Some passes read the whole program by nature | `xform_mark_unwritten_constants.rs:3` (no write anywhere in the library), `xform_toposort_declarations.rs:236-259` (reachability) |
| Code of one POU depends on bodies of other POUs | one constant pool, `compile.rs:1734-1745`; frame offsets in the data region, `compile_string.rs:101-106` |
| Per-POU artifacts are designed, not implemented | [Per-POU Code Artifacts](../design/per-pou-code-artifacts.md) |
| Hot edit swaps a whole container at a scan boundary | `compiler/runtime/src/online_change.rs:26-49`, `host.rs:266-277` |

Measured before the switch of the front end (i5-9300H, Windows, 11 small
programs): parse 2.2 ms, analysis 20.2 ms, code generation 3.2 ms. The fixed
cost of one `analyze` call is about 1.4 ms (function environment 630-660 µs,
type environment 55-100 µs, the rest not attributed).

### Measured on a large project (2026-10-08)

Phase 1 is done on the local branch `perf/analysis-benchmark-rows`; the record
is `specs/design/analysis-cost-measurement.md` on that branch. It corrects
figures of this plan and sizes the later phases:

| This plan said | Measured |
|---|---|
| 15 passes, plus one more clone (17 copies) | 17 transforms and `type_table` (18 passes); 16 copies of the library per analysis |
| about 45 rules | 47 rules; 45 of them walk the whole library, each with its own walk |
| fixed cost 1.4 ms | 1.66 ms warm, 2.2 ms cold; function environment 1.09 ms |
| no figure for a large project | 44,828 lines: analysis 1.43 s; 134,571 lines: 4.44 s |

Share of one analysis, the same at every scale: copies of the library kept by
passes 54 %, passes 22 %, rules 21 %, setup 3 %. Growth is linear in the size
except `xform_resolve_type_aliases` (quadratic in the number of types) and
`rule_constant_range`.

Only two passes read the bodies of other units: `xform_toposort_declarations`
(names called) and `xform_mark_unwritten_constants` (writes). Every other
cross-unit read, in passes and in rules, is a read of declarations.

Prototypes of phase 2 (branch `exp/analysis-reuse-prototypes`, figures only):
no copy per pass gives 0.42-0.46 of the analysis time; standard environments
shared by a layer give a fixed cost of 0.48 ms; bundled libraries parsed once
give 3.2 ms to 1.1 ms per load and nothing visible on a large project.

### Hot edit findings of 2026-10-06

| Edit | No stable IDs | With stable IDs |
|---|---|---|
| Unused function or FB type added | accepted (swap) | accepted (swap) |
| First call of a user function | rejected, V4007 | accepted as a migration; untest unavailable |
| Same, program holds an FB instance | rejected, V4007 | rejected, `FbLayoutUnsupported` |
| Unused variable added | rejected, V4007 | accepted as a migration |

Persistent state survives every accepted body-only swap, and buffers are sized
from the candidate (guard tests on the local branch
`hotedit/body-edit-layout-guard`, commits `7a712023b`, `19d2af972`; not
pushed).

### References

- CODESYS 3.5.22.10, decompiled: declaration checksums
  (`PreCompileContext.cs:1786-1801`), body checksum over the built tree
  (`:2272-2279`), selective typification only in Online Change and never after
  a change of defines, libraries or tasks, or with an existing error
  (`ObjectsToTypifyDetector.cs:68-90`), dependencies by caller, referencer and
  declarer (`:331`, `:420-448`).
- Rockwell Logix 5000 (vendor help v37/v38, 1756-RM094N): data is a separate
  named database and logic versions do not own state; call parameters are
  copied and are not tags; an online edit transfers the changed rung or
  routine; a change of the shape of existing data (tag type, array size, UDT
  structure, AOI definition) is offline only.

## Owner decisions this plan needs

| # | Decision | Recommendation |
|---|---|---|
| D1 | Open the reuse stages of §3.3 with hot edit as their consumer | Yes; approval of this plan is that decision |
| D2 | Scope of "it builds, so it applies" for hot edit | A code edit and new data always apply. A change of the shape of existing data applies when a rule for its values exists (stable IDs, a migration decision), else it is refused with an exact cause |
| D3 | State layout = persistent state only; transient call frames are outside it | Yes. Compiling every declared function instead is rejected: it moves the refusal to "declare a function" and compiles unused library code |
| D4 | Order against the deletion of the legacy parser | Independent; either order |
| D5 | What a pass keeps when it fails, so that no pass keeps a copy of the library | Every pass keeps what it transformed and reports the unit that failed; no pass reverts the whole library. Results on a program without errors do not change |
| D6 | Two consumers of one analysis: the check while the user types, and the build | One service, two requests. See "Check and build" |

### Decided by the owner (2026-10-08)

- Phase 1 is executed (D1).
- Hot edit and code generation of this plan are proven on a project of one
  `PROGRAM`. More than one program is to-do, outside this plan.
- D3: prepare the design note; an agent researches the pipeline and the
  options first, and the note carries that research so that the owner decides
  from it. The owner's objection is one of the options to test: the controller
  holds two copies while a candidate is loaded, so the place where the
  candidate is held may be what has to change, not the definition of the layout.
- D5 and D6 are open. The owner asked for the check while typing and the build
  to be separated before D5 is decided.
- How decisions reach the owner: a technical decision that can be undone and
  changes nothing on a correct program is made by the agent and recorded; a
  decision that changes what a user sees is put as a question about behaviour,
  with what CODESYS does and what a mistake costs; a decision about the goal is
  the owner's.

### Check and build

Today both are the same analysis of the whole project: the language server
runs it on every edit (`compiler/ironplc-cli/src/lsp.rs:243`,
`lsp_project.rs:189`), and the build runs it and then generates code only when
nothing reported a problem (`compiler/project/src/compile.rs:69-77`).

| | Check while typing | Build |
|---|---|---|
| Input | usually has an error | must have none |
| Needs | messages, soon | messages, then code |
| On an error | keep everything that could be analyzed | no code is generated |
| Scope it could have | the edited unit and what depends on it | everything |

In neither is the reverted library of a failed pass used for more than
messages, which is why D5 is asked. The phases 4 and 5 give the check its
scope; the build stays the control path of the invariant.

### To-do outside this plan, in the owner's order

1. Code generation of a function block instance inside a function block
   (reported by the measurement agent, `compile_stmt.rs:523`; to be verified).
2. Code generation of a member of a structure variable declared in a function
   block (reported, `compile_struct.rs:236`; to be verified).
3. More than one `PROGRAM` (`compile.rs:576`); whether the runtime runs more
   than one is not known.

A real project uses all three, so a hot edit of a real project waits for them
whatever this plan delivers.

## Architecture

### One mechanism

```
stage result + identity of its inputs + recorded dependencies
    -> reuse decision
```

Every stage is one row of one table: its name, the inputs whose identity it
compares (text, options, library version, declaration checksums), what it
records as dependencies, and what it stores. A new library, POU kind or
consumer adds a row or reads the same service; it adds no second cache and no
second invalidation rule. CLI, language server, MCP and build ask one
analysis service for the result of a snapshot.

The table is explicit and written by hand. If dependencies start to be
tracked automatically, the design has become the query graph that was
withdrawn; stop and return to the owner.

### One unit and one split of edits

The unit of analysis, of code generation, of an artifact and of a swap is the
same POU with the same qualified name. The split "declaration changed" versus
"body changed" is the same in the analyzer (two checksums), in the layout hash
and in the runtime (swap versus migration). Three definitions of that split
would be three mechanisms.

### Control invariant

One table of edit classes drives every guard: for each class, the selective
result equals the full result (diagnostics and container bytes), and the
layout rule holds. The table of body edits in
`compiler/codegen/tests/it/layout_hash.rs` is its first part.

### What is not in this plan

- Faster lexer, tree build and lowering; LTO and PGO; VM dispatch. They lower
  the cost of a full run and do not change what is recomputed.
- Local reparse inside a file. A full parse of the changed file is compatible
  with selective analysis; decide after measurements (§5 stage S5 of the
  design).
- Swap of one unit inside the runtime. It follows per-POU artifacts and needs
  its own design.
- Deletion of the legacy parser.

## Prefactoring

- **PF1 — pass contract without a copy of the library.** A pass that fails
  returns the library it was given, so the caller keeps no copy. The two
  wrappers `run_best_effort` and `run_reverting_on_error` become one.
  Behaviour-preserving: same diagnostics, same resulting library.
- **PF2 — standard environments built once.** The elementary types, standard
  function blocks and standard functions are an immutable base; an analysis
  starts from it and adds to its own copy or layer. Measure a deep clone of
  the built tables against a layered lookup before choosing.
- **PF3 — compatibility libraries parsed once.** The result is keyed by
  library name, version, content and parse options.
- **PF4 — analysis benchmark rows.** Analysis and code generation join the
  table of measured paths (`compiler/benchmarks/src/paths.rs`), with a large
  project as one of the inputs.

Each is its own PR and changes no result. PF4 comes first: PF1-PF3 are judged
by it.

## Design doc reference

- [Parse-Tree Architecture](../design/parse-tree-architecture.md) §3.2, §3.3,
  §5 (stage S4)
- [Per-POU Code Artifacts](../design/per-pou-code-artifacts.md)
- `specs/design/bytecode-container-format.md`, "Layout Hash and Online Change"
- ADR-0052, ADR-0053, ADR-0054
- `specs/design/constant-variable-inference.md`

## File map

| Area | Files | Action |
|---|---|---|
| Benchmarks | `compiler/benchmarks/src/paths.rs`, `benches/` | add analysis and codegen rows, large input |
| Analyzer pipeline | `compiler/analyzer/src/stages.rs` | pass contract; later split into declaration and body stages |
| Standard base | `compiler/analyzer/src/type_environment.rs`, `function_environment.rs` | build once |
| Libraries | `compiler/sources/src/libraries/mod.rs`, `project.rs` | parse once |
| Analysis service | `compiler/project/src/project.rs`, `compiler/ironplc-cli/src/lsp_project.rs`, `compiler/mcp/src` | one snapshot API; stored tree per document version |
| Syntax | `compiler/syntax/src` | hand out the stored green tree to consumers |
| Layout | `compiler/codegen/src/compile.rs`, `compiler/container/src`, `compiler/runtime/src/online_change.rs`, `migration.rs` | persistent state only |
| Artifacts | per the per-POU design | new |
| Design | `specs/design/parse-tree-architecture.md`, `per-pou-code-artifacts.md`, `rockwell-parity-audit.md` | record decisions and the link between the two halves; correct the audit |

## Tasks

### Phase 0 — record the direction (one documentation PR)

- [ ] `parse-tree-architecture.md`: reuse stages are open; hot edit is their
      consumer; one unit and one split of edits; the control invariant
- [ ] `per-pou-code-artifacts.md`: who decides which POUs are affected (the
      recorded dependencies), and the two cross-POU dependencies found
- [ ] `rockwell-parity-audit.md`: pending edits live on the workstation until
      Accept (vendor help), not on the controller
- [ ] Open the tracking issue and link it here

### Phase 1 — measure (PF4, then a report)

- [x] Large input: a project of hundreds of POUs (bundled compatibility
      library plus generated callers, or a multiplied corpus); record how it
      was made
- [x] Time and allocations per pass and per rule, cold and warm, three runs
- [x] List of passes and rules that need the whole program, with the body
      fact each one reads
- [x] Record in the design document; no product change besides the benchmark

Done on the local branch `perf/analysis-benchmark-rows` (not pushed). The
code generation rows run on a project of one program whose function blocks
hold no instance and read no member, because code generation refuses the
project of many programs.

**Gate.** Phases 4 and 5 are sized from this report. Revise this plan before
starting them. What the report already changes:

- Phase 2, PF1 cannot be done as written: a pass consumes the library and
  fails midway, so it has no library to give back. It needs D5.
- Phase 4 is smaller than feared for the passes (two read bodies of other
  units) and has a part this plan did not name: the rules. Forty-five walks of
  the library become one walk, or the rule loop takes the unit to check.
- `xform_resolve_type_aliases` and `type_table` fit no split by unit; the
  first is quadratic and the result of the second is only logged.

### Phase 2 — stop redoing the unchanged (PF1, PF2, PF3; three PRs)

- [ ] PF1 pass contract; guard: results identical on the corpus
- [ ] PF2 standard base once; guard: an analysis cannot change the base
- [ ] PF3 libraries once; guard: a changed library or option gives a new result
- [ ] Each PR reports the benchmark rows before and after

### Phase 3 — one analysis service (stage S4 of the design)

- [ ] One snapshot API for CLI, language server, MCP and build
- [ ] The tree of a document is stored with its text, errors and options for
      one version; highlighting and lowering read it
- [ ] Results are published for the requested revision only
- [ ] Guard: the same snapshot gives the same diagnostics through every consumer

### Phase H — state layout (independent of phases 1-3; may start now)

- [ ] H1: publish the guard tests (local branch, needs a push by the owner or
      the owner's permission)
- [ ] H2: research. Are persistent slots always a prefix of the variable
      table (globals and programs before function, FB-body and method frames)?
      What does the per-program variable count of the task table cover?
- [ ] H3: design note or ADR: layout = persistent state only; format, VM,
      verifier and runtime consequences
- [ ] H4: implementation; the two missing rows (first and last call of a user
      function, in a program and in an FB body) join the guard table
- [ ] H5: exact cause in the refusal when a candidate is still refused

### Phase 4 — unit of analysis (sized after phase 1)

- [ ] Declaration stage: declarations of all POUs to environments and
      per-declaration summaries
- [ ] Body stage: one body to a checked body, diagnostics, and the names it
      looked up, found or not
- [ ] Whole-program passes read per-body summaries (writes, calls), not bodies
- [ ] Positions: a checksum excludes them; stored diagnostics are relative to
      the POU
- [ ] Guard: results identical to the pipeline before the split

### Phase 5 — selective analysis

- [ ] Two checksums per declaration (with and without initial values), one
      per body
- [ ] Reuse rule: body checksum, checksums of the declarations it looked up,
      the names it did not find, options
- [ ] Full analysis on a change of options, defines, libraries or tasks
- [ ] Guard: the control invariant over the table of edit classes and over
      sequences of edits

### Phase 6 — code generation and artifacts per POU

- [ ] Remove the cross-POU dependencies or record them (constant pool,
      data-region frames)
- [ ] Implement the per-POU artifact design
- [ ] Guard: the linked container is byte-identical to a full build

## Risks

- **Phase 4 is the large one.** Seventeen transforms and forty-seven rules run
  on the whole library today. Phase 1 sized it; see "Measured on a large
  project".
- **The end of the line is not reachable on a real project** until the to-do
  outside this plan is done: the plan can deliver a hot edit of one unit of a
  project that code generation accepts, and today that is a small one.
- **A stale result shown as current.** The only defence is the control
  invariant; no selective stage lands without it.
- **Format change in phase H.** The container, the verifier and the runtime
  move together; an ADR comes first.
- **Editor state with errors.** CODESYS runs a full typification whenever an
  error exists. While a user types, an error usually exists; measure before
  assuming the selective path is the common one in the editor.
