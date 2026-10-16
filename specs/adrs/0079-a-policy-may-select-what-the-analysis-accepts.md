# A Policy May Select What the Analysis Accepts

status: accepted
date: 2026-10-16

## Context and Problem Statement

[ADR-0049](0049-behavior-policies-selected-at-compile-time.md) makes a behavior
policy a compile-time selection among documented alternatives for what an
operation does, and requires the selection to be encoded in the bytecode: each
alternative is its own func_id, so a `.iplc` file means the same on every VM.

The attribute `strict` before an enumeration needs a selection of another kind.
In CODESYS an enumeration declared `{attribute 'strict'}` refuses arithmetic, the
numerical functions, `FOR` and the storing of a number into it. Without the
attribute the enumeration is its base type in all of those, and the program runs
as the program on the integer would. IEC 61131-3 gives the values of an
enumeration no arithmetic, so the standard's behavior is that every enumeration
is strict, and CODESYS and TwinCAT are the vendors who document the other. Which
of the two a compilation uses is a choice that dialects make differently, which
is what a policy is. What the choice changes is whether the analysis reports a
problem; a program that both alternatives accept computes the same values under
either. An enumeration that is not strict is held in the width of its base type,
which changes the instructions, not the values such a program stores into it.

## Decision Drivers

- **One mechanism for a choice that dialects make differently.** A flag only
  enables, and a second kind of selection would have to be wired to the CLI, LSP
  and MCP surfaces again.
- **The default is the standard** (ADR-0049 rule 4).
- **The container crate states encodings.** Its policy enumerations are
  commitments about the bytes of a program, shared with the virtual machine.

## Considered Options

1. **A policy declared with the options (chosen).** The enumeration implements
   `BehaviorPolicy` in `ironplc_parser::options` and is a row of the `policies`
   section of `define_compiler_options!`, as the others are.
2. **A policy declared in `ironplc_container::policy`.** The same, beside the
   encoded policies.
3. **An `--allow-*` flag.** `strict` becomes the default and the flag lifts it.

## Decision Outcome

Chosen option: **a policy declared with the options**. A policy may select what
the analysis accepts; ADR-0049 rule 3 (encode the selection in the bytecode)
applies to a policy that changes what an operation does, and is vacuous for one
whose alternatives differ only in which programs are accepted. The machinery is
that of ADR-0049: the field of `CompilerOptions`, the preset table, the
`POLICY_DESCRIPTORS` that the CLI, LSP and MCP surfaces read, the first
alternative as the default.

The enumeration is not in the container crate, because that crate states what
the virtual machine decodes and this selection is never decoded. The index of
an alternative is its position, as for every policy, and is used for nothing
else.

The policy is applied once, where the type of an enumeration is resolved
(`TypeEnvironment::enumeration_is_strict`): the answer is a field of the type,
and the checks ask the type (`IntermediateType::is_strict_enumeration`), never
the policy or the attribute.

### Consequences

- Good, because a dialect selects it with the same preset table as the others,
  and `--policy-enumeration-strictness` overrides it as the others are overridden.
- Good, because the container crate and the virtual machine do not change.
- Bad, because ADR-0049 and the glossary say that a policy is encoded in the
  bytecode, and this one is not; the glossary now says that a policy may select
  what the analysis accepts instead.
- Bad, because the selection applies to a whole analysis. A composition of
  files of different dialects is analyzed with one set of options (ADR-0012
  accepts a vendor file as it is, but the options are not per file here), so the
  enumerations of all of its files are strict by the same rule.

### Confirmation

- `compiler/parser/src/spec_conformance_behavior_policies.rs`:
  REQ-BP-parser-005, the presets and the reading of the attribute.
- `compiler/ironplc-cli/bin/main.rs`: every alternative is accepted by the
  `--policy-*` flag and replaces the preset.
