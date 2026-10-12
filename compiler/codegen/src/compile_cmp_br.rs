//! Fusing a `var <cmp> const` comparison with its branch into one `CMP_BR_*`
//! instruction.

use ironplc_container::{opcode, VarIndex};
use ironplc_dsl::common::ConstantKind;
use ironplc_dsl::core::Id;
use ironplc_dsl::textual::{CompareOp, Expr, ExprKind, SymbolicVariableKind, UnaryOp, Variable};

use super::compile::{CompileContext, OpWidth, Signedness};
use super::compile_expr::signed_integer_to_i64;

/// Result of classifying a comparison expression as a fusable
/// `var <cmp> const` shape eligible for emission via `CMP_BR_*`.
#[derive(Clone, Copy)]
pub(crate) struct ClassifiedCmp {
    /// `cmp_op` byte (one of `opcode::cmp_op::*`), already adjusted so the
    /// variable is on the LHS and the constant on the RHS.
    pub cmp_op_byte: u8,
    /// Variable table index of the LHS.
    pub var_index: VarIndex,
    /// Constant pool index of the RHS literal.
    pub const_idx: u16,
    /// Operand width: `W32` (→ `CMP_BR_I32`) or `W64` (→ `CMP_BR_I64`).
    pub width: CmpWidth,
    /// The negation of `cmp_op_byte`, for a branch taken when the predicate
    /// is false.
    negated_cmp_op_byte: u8,
}

/// The operand widths a fused `CMP_BR_*` instruction exists for. The float
/// widths have no such instruction, so a classified comparison cannot name them.
#[derive(Clone, Copy)]
pub(crate) enum CmpWidth {
    W32,
    W64,
}

impl ClassifiedCmp {
    /// A classified comparison, or `None` when `cmp_op_byte` has no negation
    /// (is not a comparison code).
    pub(crate) fn new(
        cmp_op_byte: u8,
        var_index: VarIndex,
        const_idx: u16,
        width: CmpWidth,
    ) -> Option<Self> {
        Some(ClassifiedCmp {
            cmp_op_byte,
            var_index,
            const_idx,
            width,
            negated_cmp_op_byte: opcode::cmp_op::negate(cmp_op_byte)?,
        })
    }
}

/// Recognises the fusable shape `var <cmp> const_literal` (or
/// `const_literal <cmp> var`, which is commuted), where the variable is a
/// simple named scalar of a 32- or 64-bit signed integer type and the
/// constant is an integer literal that fits the variable's width.
///
/// Returns `Some` with the constant pooled and the comparison operator
/// resolved to a `cmp_op` byte. Returns `None` for any unsupported shape
/// (float compares, unsigned compares, var-var, complex LHS/RHS, etc.) so
/// the caller can fall back to the unfused emission.
pub(crate) fn try_classify_cmp(ctx: &mut CompileContext, expr: &Expr) -> Option<ClassifiedCmp> {
    let compare = match peel(expr) {
        ExprKind::Compare(c) => c,
        _ => return None,
    };
    let cmp_op_byte = compare_op_to_cmp_op(&compare.op)?;

    let left_kind = peel(&compare.left);
    let right_kind = peel(&compare.right);

    // Try `var <cmp> const`.
    if let (Some(name), Some(value)) = (named_variable_name(left_kind), constant_i64(right_kind)) {
        return classify_with_named(ctx, name, value, cmp_op_byte);
    }
    // Try `const <cmp> var` — commute to `var <cmp> const`.
    if let (Some(value), Some(name)) = (constant_i64(left_kind), named_variable_name(right_kind)) {
        let commuted = opcode::cmp_op::commute(cmp_op_byte)?;
        return classify_with_named(ctx, name, value, commuted);
    }
    None
}

fn classify_with_named(
    ctx: &mut CompileContext,
    name: &Id,
    value: i64,
    cmp_op_byte: u8,
) -> Option<ClassifiedCmp> {
    let info = ctx.var_type_info(name)?;
    if info.signedness != Signedness::Signed {
        return None;
    }
    let var_index = ctx.var_index(name).ok()?;
    match info.op_width {
        OpWidth::W32 => {
            let v32 = i32::try_from(value).ok()?;
            let const_idx = ctx.add_i32_constant(v32);
            ClassifiedCmp::new(cmp_op_byte, var_index, const_idx, CmpWidth::W32)
        }
        OpWidth::W64 => {
            let const_idx = ctx.add_i64_constant(value);
            ClassifiedCmp::new(cmp_op_byte, var_index, const_idx, CmpWidth::W64)
        }
        OpWidth::F32 | OpWidth::F64 => None,
    }
}

/// Emits a `CMP_BR_*` instruction for a previously classified comparison,
/// branching to `target` when the (possibly negated) predicate evaluates
/// to true. When `branch_when_true` is `false`, the comparison operator
/// is negated so the branch fires on the false-polarity (e.g. for
/// "branch to END if NOT cond" zero-trip and IF skip patterns).
pub(crate) fn emit_classified_cmp_br(
    emitter: &mut crate::emit::Emitter,
    classified: ClassifiedCmp,
    branch_when_true: bool,
    target: crate::emit::Label,
) {
    let cmp_op_byte = if branch_when_true {
        classified.cmp_op_byte
    } else {
        classified.negated_cmp_op_byte
    };
    match classified.width {
        CmpWidth::W32 => emitter.emit_cmp_br_i32(
            cmp_op_byte,
            classified.var_index,
            classified.const_idx,
            target,
        ),
        CmpWidth::W64 => emitter.emit_cmp_br_i64(
            cmp_op_byte,
            classified.var_index,
            classified.const_idx,
            target,
        ),
    }
}

/// Strips parenthesised-expression wrappers from an `ExprKind`.
fn peel(expr: &Expr) -> &ExprKind {
    let mut current = &expr.kind;
    while let ExprKind::Expression(inner) = current {
        current = &inner.kind;
    }
    current
}

/// Returns the identifier of a simple named scalar variable reference,
/// or `None` for any other variable shape (array element, struct field,
/// bit access, dereference, etc.).
fn named_variable_name(kind: &ExprKind) -> Option<&Id> {
    match kind {
        ExprKind::Variable(Variable::Symbolic(SymbolicVariableKind::Named(named))) => {
            Some(&named.name)
        }
        ExprKind::LateBound(late) => Some(&late.value),
        _ => None,
    }
}

/// Returns the `i64` value of an `ExprKind` that is a compile-time integer
/// literal (positive, negative, or unary-negated). Returns `None` otherwise.
fn constant_i64(kind: &ExprKind) -> Option<i64> {
    match kind {
        ExprKind::Const(ConstantKind::IntegerLiteral(lit)) => {
            signed_integer_to_i64(&lit.value).ok()
        }
        ExprKind::UnaryOp(unary) if unary.op == UnaryOp::Neg => match &unary.term.kind {
            ExprKind::Const(ConstantKind::IntegerLiteral(lit)) => signed_integer_to_i64(&lit.value)
                .ok()
                .and_then(i64::checked_neg),
            _ => None,
        },
        _ => None,
    }
}

/// Maps a `CompareOp` AST node to the `opcode::cmp_op` byte used by
/// `CMP_BR_*`. Returns `None` for non-comparison (logical/bitwise)
/// operators, which `CMP_BR_*` does not support.
fn compare_op_to_cmp_op(op: &CompareOp) -> Option<u8> {
    match op {
        CompareOp::Eq => Some(opcode::cmp_op::EQ),
        CompareOp::Ne => Some(opcode::cmp_op::NE),
        CompareOp::Lt => Some(opcode::cmp_op::LT_S),
        CompareOp::LtEq => Some(opcode::cmp_op::LE_S),
        CompareOp::Gt => Some(opcode::cmp_op::GT_S),
        CompareOp::GtEq => Some(opcode::cmp_op::GE_S),
        CompareOp::And
        | CompareOp::Or
        | CompareOp::Xor
        | CompareOp::AndThen
        | CompareOp::OrElse => None,
    }
}
