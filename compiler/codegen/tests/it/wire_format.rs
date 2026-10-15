//! Wire-format test suite — the canonical guard against accidental
//! changes to the on-disk bytecode encoding.
//!
//! ## Scope
//!
//! Behavioural codegen tests (in `compile_*.rs`) verify that the right
//! opcodes are emitted in the right order. They are *symbolic*: they
//! talk about `LOAD_VAR_I32` by name, not by byte value. That is the
//! right level for testing codegen behaviour, but it would let a
//! silent renumbering of an opcode pass undetected.
//!
//! This module, with its child files, is the **single canonical source of
//! wire-format truth**.
//! It pins:
//!
//! 1. **Every opcode constant's byte value.** A renumber that changes
//!    `LOAD_VAR_I32` from 0x0C to 0x0D fails exactly one test here
//!    with a clear `expected 0x0C, found 0x0D` message — instead of
//!    cascading into ~30 confusing diffs across `compile_*.rs`.
//! 2. **The structured `[op_class:6][type:2]` encoding scheme.** For
//!    op-classes with multiple type variants (e.g. LOAD_VAR_*), the
//!    high 6 bits are constant within the family and the low 2 bits
//!    encode the type tag. A renumber that violates the scheme is
//!    caught here even if every byte value is internally consistent.
//! 3. **Per-shape golden encodings.** A handful of small programs
//!    compile to known-exact byte sequences, guarding operand widths,
//!    little-endian operand encoding, and per-shape layout.

mod builtin_func_ids;
mod completeness;
mod encoding_scheme;
mod golden_encoding;
mod opcode_bytes;
