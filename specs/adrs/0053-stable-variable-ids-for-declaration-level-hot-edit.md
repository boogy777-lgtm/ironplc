# Stable Variable IDs for Declaration-Level Hot Edit

status: accepted
date: 2026-09-15

## Context and Problem Statement

The container format's layout hash covers every declaration, and the runtime
host that performs online change ([ADR-0052](0052-online-change-performed-by-the-runtime-host.md))
rejects a candidate whose hash differs. That is safe, but it means a rename —
a change to a name only — is rejected exactly like a type change, even though
the variable's data was never in question. Extending hot edit to declaration
edits requires the container to carry the one thing the type section
deliberately omits: the identity of a declaration as distinct from its name.

Source text cannot supply that identity. A same-type name swap (`a` and `b`
exchange names) is textually identical to a declaration reorder (values stay
with names versus values stay with entities are both valid readings), and a
diff-type name swap is textually identical to two retypes. A runtime that
guesses between those readings can silently bind a value to the wrong
entity. Identity therefore has to be an explicit model property, created when
the engineering environment creates the declaration and carried through the
container. How is it represented, what does the container store, and what
role does it play in the layout hash?

## Decision Drivers

* A rename must move no data: type and value belong to the entity, the name
  is only a binding.
* The runtime must never guess at declaration intent; where the raw text is
  ambiguous, the engineering environment that has the edit history resolves
  it, and an unresolved candidate is rejected safely.
* The layout hash must stay a memory-layout signature, not an identity
  registry: decisions that are not about memory layout must not change it.
* The VM stays a pure execution kernel: the table is data for a host-level
  migration planner, never an execution input.
* The container format stays compact and name-free in its type section.

## Considered Options

* **Extend `VarEntry` with a UID field.** Every variable record would pay for
  an identity that transient slots (function locals, scratch) do not have,
  and declaration identity would be mixed into the same wire record the
  layout hash is computed over.
* **Carry variable names in the type section and match by name.** Names are
  mutable bindings, so a name swap would silently swap values; it also
  bloats every container with metadata that already lives in the debug
  section.
* **Separate `stable_vars` sub-table mapping `var_index` to an opaque UID
  (chosen).** Persistent variables get an identity; transient slots get
  nothing; the table is excluded from the layout hash.
* **Representation: u64 opaque counter (chosen), 128-bit UUID, or a string
  ID.** A UUID doubles the wire cost of every entry for globally unique
  identity this stage does not need; a string ID is variable length and
  duplicates names. A `u64` counter allocated by the engineering side fits
  one fixed 10-byte entry and leaves room to revisit the width if
  cross-project merging ever needs it.

## Decision Outcome

Chosen option: **a separate `stable_vars` sub-table carrying opaque u64
UIDs**, because it keeps declaration identity out of both the layout record
and the execution path while giving the migration planner exactly the map it
needs.

* **Entity identity.** The UID is assigned by the engineering side when the
  declaration is created. A rename — including a name swap — keeps the UID,
  so the type and the value stay with the entity and a rename performs zero
  data movement.
* **Wire content.** The container carries only `var_index` -> `uid` pairs,
  in ascending `var_index` order, with no names. Transient or scratch slots
  have no UID and no entry. The table is the fifth and last type-section
  sub-table, always emitted (count 0 when empty); a reader accepts a type
  section that ends before it, as with every earlier sub-table.
* **No layout-hash membership.** UIDs are excluded from `layout_hash` by
  design: migration compatibility is decided by the runtime migration
  planner, which compares the active and candidate tables and the per-entry
  type data, not by a single hash. A candidate that differs only in UIDs and
  names therefore keeps the active hash.
* **Ambiguity stays outside the runtime.** A same-type name swap is
  textually ambiguous, and the engineering environment's Pending phase
  resolves the intent before a candidate is offered. The runtime never
  guesses; a candidate it cannot justify is rejected.
* **Population path.** For this stage the table is populated through the
  compiler API (`TypeSection::stable_vars`, `ContainerBuilder::add_stable_var`);
  project-model and IDE plumbing that allocates and persists UIDs follows.

The container format version moves to 5 for the new sub-table. The VM is
unchanged: it ignores the table, and the interpreter still uses
compiler-assigned indices directly.

### Consequences

* Good, because a rename or name swap migrates by rebinding a UID: no bytes
  move, and the layout hash correctly reports no layout change.
* Good, because migration becomes a per-variable decision with the planner as
  the single authority, instead of all-or-nothing behind one hash.
* Good, because the UID is a prerequisite for later multi-arena and
  redundant-host state models, without committing to one now.
* Bad, because the engineering environment must allocate and persist UIDs
  across sessions and refactors; a project that loses its UID store degrades
  to no-migration (safe rejection) rather than silent guessing.
* Neutral, because the UID width is a fixed `u64` counter; collision and
  project-merge policy are deferred, and widening later is a format-version
  change like this one.
* Neutral, because older readers reject a v5 container at the header version
  check, as with every earlier format bump.

## More Information

* `specs/design/bytecode-container-format.md`, "Type Section" (Stable
  Variable IDs) and "Layout Hash and Online Change".
* [ADR-0052](0052-online-change-performed-by-the-runtime-host.md) — the
  host-level online change whose validation this table extends.
* `compiler/container/src/type_section.rs`, `builder.rs`,
  `container_ref.rs`; `compiler/runtime/src/online_change.rs`.
