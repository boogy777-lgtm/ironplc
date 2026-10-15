# Pack Mode Describes the Byte Image, Not the VM Slot Storage

status: accepted
date: 2026-10-16

## Context and Problem Statement

[ADR-0076](0076-a-known-attribute-is-a-row-with-a-typed-reader.md) made
`pack_mode` a known attribute that no part of IronPLC acted on. In CODESYS the
attribute `{attribute 'pack_mode' := 'n'}` before a structure sets an upper
bound `n` (0, 1, 2, 4 or 8, where 0 is 1) on the alignment of its members: the
offset of a member is a multiple of the smaller of its alignment and `n`. The
purpose is to describe a layout that is exchanged as bytes (a network message, a
file, a record of a fieldbus), and what a program observes of it is the offsets
of the members and `SIZEOF`.

The virtual machine does not lay a structure out in bytes. A structure is whole
8-byte slots in the data region, one or more for each member, and a member is
read and written as a slot (see ADR-0035 for strings and the slot layout of the
data region). The analysis already computes a byte layout for each structure, its
`IntermediateType::Structure` carries the offset of every member, but nothing
reads those offsets for execution. Honouring `pack_mode` therefore raises the
question which of the two layouts the attribute applies to.

## Decision Drivers

- **A program that is correct in CODESYS gives the same `SIZEOF` and the same
  offsets here.** The attribute exists for the byte image.
- **The storage of the virtual machine is the hot path.** A change of how every
  structure is stored changes the generated code, the container format and the
  performance of every program, not only of those with the attribute.
- **One rule for offsets.** A mode must apply to every place that computes an
  offset, so that it cannot apply in one and not in another.
- **An accepted attribute is not silently without effect.** The row says
  `Honoured` only when a part of IronPLC acts on it, and the documentation says
  what the action is.

## Considered Options

1. **The attribute describes the byte image (chosen).** The analysis computes the
   offsets, the alignment and the size of a structure under the bound of the
   mode; `SIZEOF` and the byte offsets answer from them. The slots of the
   virtual machine are unchanged.
2. **Change the storage of the virtual machine to packed bytes.** A structure
   would be stored as bytes at the offsets of its layout, and a member would be
   read and written at a byte offset with its width.
3. **Ignore the attribute.** Keep the row `Waiting`.

## Decision Outcome

Chosen option: **the attribute describes the byte image**, because it gives the
observable behaviour of the attribute (`SIZEOF` and the offsets) without
touching the storage that every program depends on.

The bound is part of the type: `IntermediateType::Structure` carries an
`AlignmentBound` that is read from the attribute at one place (the resolution of
a structure or a union), and the alignment, the size and the offsets derive from
it. One function (`intermediates::layout::layout`) lays out the members of a
structure, a union and a function block, so the rule for an offset is written
once. The mode is mapped to the bound by a `match` with no wildcard, so a mode
added to the row does not compile until it answers.

A structure has two sizes. Its byte image size, which `SIZEOF` answers, is the
end of its last member. Its stride, which is the space it occupies where it
repeats (the element of an array) or nests (a member of a structure), is the
byte image size padded up to its alignment. Whether the byte image itself ends
with the padding is a decision that is provisional until it is checked on a live
CODESYS: the help gives 28 bytes for the third example in the mode 8, not 32,
which is the end of the last member; the decision is made in one function
(`Layout::byte_image_size`) so that the opposite decision is one line.

`SIZEOF` answers the size of the resolved type of its argument
(`IntermediateType::size_in_bytes`) for every argument, in place of the special
case of an array variable.

### Consequences

- Good, because `SIZEOF` of a structure, of an array of structures and of a
  structure that nests another is answered, in each mode, as the CODESYS help
  describes it.
- Good, because the storage, the generated code and the container are unchanged
  for every program, with or without the attribute.
- Good, because the three copies of the loop that computed offsets (structures,
  user function blocks and standard function blocks) are one function.
- Bad, because the attribute does not change how the data is stored: a program
  that reads a structure as bytes (a cast of a pointer to a structure to a
  pointer to bytes) does not see the packed layout. IronPLC has no such access
  for structures yet, and the option below is the way to add one.
- Bad, because `SIZEOF` of an enumeration is now the size of the type it is
  stored in (its underlying integer type, 1 byte for a few values) instead of
  4 bytes. Neither is what CODESYS gives (2 bytes, the default base type
  `INT`); that answer belongs to the base type of an enumeration, not to the
  layout.
- Neutral: the default of CODESYS without the attribute is not known to be the
  natural alignment (it may be a bound of 4 or of 8). The natural alignment is
  what the compiler did before, so the default is unchanged until the question
  is answered.

### Confirmation

- `compiler/analyzer/src/intermediates/layout/tests.rs`: the three examples of
  the CODESYS help in each of the five modes.
- `compiler/analyzer/src/intermediates/structure.rs`: a structure and a union
  with the attribute get the offsets of the mode.
- `compiler/codegen/tests/it/end_to_end_sizeof.rs`: `SIZEOF` of a structure in
  each mode, of an array of structures and of a nested structure.

## Pros and Cons of the Options

### The attribute describes the byte image

- Good, because it is the observable behaviour of the attribute, and nothing
  that does not use the attribute changes.
- Good, because the layout already exists in the analysis and is now computed
  once.
- Bad, because the byte image and the storage are two layouts, and a feature that
  needs the bytes of a structure has to bridge them.

### Change the storage of the virtual machine to packed bytes

- Good, because the layout of the bytes would be the layout of the storage, and a
  byte-level access would see the packed structure.
- Bad, because every structure and every array of structures changes, whether it
  has the attribute or not: the generated code, the data region, the debug
  information and the container.
- Bad, because a member would be read from an unaligned byte offset on every
  access, which costs on the hot path of the machine.
- Bad, because it is a large change whose only gain, a byte-level access to a
  structure, has no user in the language today.

### Ignore the attribute

- Good, because nothing changes.
- Bad, because `SIZEOF` of a structure stays unanswered and the attribute stays
  accepted with no effect, which is the thing the registry exists to avoid.

## More Information

- [Attributes of Declarations](../design/attributes.md#layout): what the Layout
  attributes do and what they do not do.
- [ADR-0076](0076-a-known-attribute-is-a-row-with-a-typed-reader.md): the row,
  its state and the typed reader.
