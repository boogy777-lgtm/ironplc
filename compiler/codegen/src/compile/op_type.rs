//! Operation width, signedness and variable type descriptors that select opcodes.

/// The native operation width used for arithmetic and comparisons.
#[derive(Clone, Copy, Debug, PartialEq)]
pub(crate) enum OpWidth {
    /// 32-bit integer operations (for SINT, INT, DINT, USINT, UINT, UDINT).
    W32,
    /// 64-bit integer operations (for LINT, ULINT).
    W64,
    /// 32-bit float operations (for REAL).
    F32,
    /// 64-bit float operations (for LREAL).
    F64,
}

/// Whether a type uses signed or unsigned semantics for division and comparison.
#[derive(Clone, Copy, Debug, PartialEq)]
pub(crate) enum Signedness {
    Signed,
    Unsigned,
}

/// Type information for a variable, used to select the correct opcodes.
#[derive(Clone, Copy)]
pub(crate) struct VarTypeInfo {
    /// The native operation width (i32 or i64).
    pub(crate) op_width: OpWidth,
    /// Whether signed or unsigned opcodes are used for division/comparison.
    pub(crate) signedness: Signedness,
    /// The declared storage width in bits (8, 16, 32, or 64).
    pub(crate) storage_bits: u8,
}

/// Shorthand for the operation type tuple used during expression compilation.
pub(crate) type OpType = (OpWidth, Signedness);

/// The default operation type: 32-bit signed (used for pure-constant expressions).
pub(crate) const DEFAULT_OP_TYPE: OpType = (OpWidth::W32, Signedness::Signed);
