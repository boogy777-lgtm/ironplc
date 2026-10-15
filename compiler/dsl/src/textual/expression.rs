//! Expressions: operators, operands, and the typed expression wrapper.

use super::{MethodCall, ParamAssignmentKind, Variable};
use crate::common::{
    ConstantKind, EnumeratedValue, GenericTypeName, IntegerLiteral, SignedInteger,
};
use crate::core::{Id, Located, SourceSpan};
use crate::type_id::TypeId;
use std::fmt;

use crate::fold::Fold;
use crate::visitor::Visitor;
use dsl_macro_derive::{Located, Recurse};

/// A binary expression that produces a Boolean result by comparing operands.
///
/// See section 3.3.1.
#[derive(Debug, Clone, PartialEq, Recurse)]
pub struct CompareExpr {
    #[recurse(ignore)]
    pub op: CompareOp,
    pub left: Expr,
    pub right: Expr,
}

/// A binary expression that produces an arithmetic result by operating on
/// two operands.
///
/// See section 3.3.1.
#[derive(Debug, Clone, PartialEq, Recurse)]
pub struct BinaryExpr {
    #[recurse(ignore)]
    pub op: Operator,
    pub left: Expr,
    pub right: Expr,
}

/// A unary expression that produces a boolean or arithmetic result by
/// transforming the operand.
///
/// See section 3.3.1.
#[derive(Debug, Clone, PartialEq, Recurse)]
pub struct UnaryExpr {
    #[recurse(ignore)]
    pub op: UnaryOp,
    pub term: Expr,
}

#[derive(Debug, Clone, PartialEq, Recurse)]
pub struct Function {
    pub name: Id,
    pub param_assignment: Vec<ParamAssignmentKind>,
}

impl fmt::Display for Function {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(f, "{}(...)", self.name)
    }
}

#[derive(Debug, PartialEq, Clone, Recurse)]
pub struct LateBound {
    pub value: Id,
}

impl fmt::Display for LateBound {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(f, "{}", self.value)
    }
}

/// The type of an expression's value, as the analyzer resolved it.
#[derive(Debug, PartialEq, Clone)]
pub enum ExprType {
    /// A value of exactly this type.
    Concrete(TypeId),
    /// An untyped literal, or a generic function applied only to untyped
    /// literals: its type is fixed by where it is used, within this
    /// category. ADR-0028 and ADR-0031 say which types it may take.
    Literal(GenericTypeName),
    /// `NULL`: a reference to no variable, of whichever reference type it
    /// is used as.
    Null,
}

/// Wrapper around `ExprKind` that carries what is true of an expression but
/// not of the operation it performs: the type of its value, and where it was
/// written.
///
/// The `expr_type` field is populated by a later analysis pass. During
/// parsing and initial construction, it is always `None`.
///
/// `expr_type` is left out of equality (see the manual `PartialEq` below):
/// the ids it holds are allocated per compilation, so an expected
/// expression built by hand cannot know them. A test asserts an
/// expression's type through the environment that allocated it.
#[derive(Debug, Clone, Recurse, Located)]
pub struct Expr {
    pub kind: ExprKind,
    /// The type of the expression's value, by identity. `None` where the
    /// analyzer could not resolve one.
    #[recurse(ignore)]
    pub expr_type: Option<ExprType>,
    /// Where the expression was written, as written.
    ///
    /// The parser records the span of every token it matched, including the
    /// ones no child node holds -- a unary operator, a caret, the
    /// parentheses of a group or of a function call's argument list. That is
    /// why the span lives here rather than being derived from `kind`:
    /// `Located for ExprKind` can only join the spans of the children, so it
    /// reports `NOT g` as `g`. An expression built by a transform rather
    /// than parsed has no tokens to record, and `Expr::new` falls back to
    /// that derivation for it.
    #[located(position)]
    pub span: SourceSpan,
}

impl PartialEq for Expr {
    fn eq(&self, other: &Self) -> bool {
        self.kind == other.kind && self.span == other.span
    }
}

impl Expr {
    /// Creates a new `Expr` with no resolved type, spanning whatever its
    /// `kind` spans.
    pub fn new(kind: ExprKind) -> Expr {
        Expr {
            span: kind.span(),
            kind,
            expr_type: None,
        }
    }

    /// Returns this expression, recording `span` as where it was written.
    ///
    /// The parser calls this wherever the expression is written with tokens
    /// that none of its children hold, and so that `Expr::new` cannot
    /// recover from `kind` alone.
    pub fn with_span(mut self, span: SourceSpan) -> Expr {
        self.span = span;
        self
    }

    /// Creates a comparison of two expressions.
    pub fn compare(op: CompareOp, left: Expr, right: Expr) -> Expr {
        Expr::new(ExprKind::Compare(Box::new(CompareExpr { op, left, right })))
    }

    /// Creates an arithmetic or bitwise operation on two expressions.
    pub fn binary(op: Operator, left: Expr, right: Expr) -> Expr {
        Expr::new(ExprKind::BinaryOp(Box::new(BinaryExpr { op, left, right })))
    }

    /// Creates an operation on a single expression.
    pub fn unary(op: UnaryOp, term: Expr) -> Expr {
        Expr::new(ExprKind::UnaryOp(Box::new(UnaryExpr { op, term })))
    }
}

impl fmt::Display for Expr {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(f, "{}", self.kind)
    }
}

/// Reconstructs an expression's span from its children.
///
/// This is a fallback, and lossy: an expression is written with tokens that
/// none of its children hold -- the operator of `NOT g`, the caret of `p^`,
/// the parentheses of `(a + b)` and of `MAX(a, b)` -- and those tokens
/// cannot be recovered from the tree. Prefer `Expr::span`, which is what
/// the parser records; this is what seeds it for an expression that a
/// transform built rather than the parser.
impl Located for ExprKind {
    fn span(&self) -> SourceSpan {
        match self {
            ExprKind::Compare(expr) => SourceSpan::join(&expr.left.span(), &expr.right.span()),
            ExprKind::BinaryOp(expr) => SourceSpan::join(&expr.left.span(), &expr.right.span()),
            ExprKind::UnaryOp(expr) => expr.term.span(),
            ExprKind::Expression(inner) => inner.span(),
            ExprKind::Const(constant) => constant.span(),
            ExprKind::EnumeratedValue(value) => value.span(),
            ExprKind::Variable(var) => var.span(),
            ExprKind::Function(func) => func.name.span(),
            ExprKind::MethodCall(call) => call.span(),
            ExprKind::LateBound(late) => late.value.span(),
            ExprKind::Ref(var) => var.span(),
            ExprKind::Deref(expr) => expr.span(),
            ExprKind::Null(span) => span.clone(),
        }
    }
}

/// Expression that yields a value derived from the input(s) to the expression.
#[derive(Debug, PartialEq, Clone, Recurse)]
pub enum ExprKind {
    Compare(Box<CompareExpr>),
    BinaryOp(Box<BinaryExpr>),
    UnaryOp(Box<UnaryExpr>),
    Expression(Box<Expr>),
    Const(ConstantKind),
    EnumeratedValue(EnumeratedValue),
    Variable(Variable),
    Function(Function),
    MethodCall(MethodCall),
    LateBound(LateBound),
    Ref(Box<Variable>),
    Deref(Box<Expr>),
    Null(SourceSpan),
}

impl ExprKind {
    pub fn compare(op: CompareOp, left: ExprKind, right: ExprKind) -> ExprKind {
        Expr::compare(op, Expr::new(left), Expr::new(right)).kind
    }

    pub fn binary(op: Operator, left: ExprKind, right: ExprKind) -> ExprKind {
        Expr::binary(op, Expr::new(left), Expr::new(right)).kind
    }

    pub fn unary(op: UnaryOp, term: ExprKind) -> ExprKind {
        Expr::unary(op, Expr::new(term)).kind
    }

    pub fn named_variable(name: &str) -> ExprKind {
        ExprKind::Variable(Variable::named(name))
    }

    pub fn late_bound(name: &str) -> ExprKind {
        ExprKind::LateBound(LateBound {
            value: Id::from(name),
        })
    }

    #[expect(
        clippy::unwrap_used,
        reason = "AST test builder: callers pass validated literals; a Result here would pollute every test callsite"
    )]
    pub fn integer_literal(value: &str) -> ExprKind {
        ExprKind::Const(ConstantKind::IntegerLiteral(IntegerLiteral {
            value: SignedInteger::new(value, SourceSpan::default()).unwrap(),
            data_type: None,
        }))
    }
}

impl fmt::Display for ExprKind {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            ExprKind::Compare(expr) => {
                write!(f, "{} {} {}", expr.left, expr.op, expr.right)
            }
            ExprKind::BinaryOp(expr) => {
                write!(f, "{} {} {}", expr.left, expr.op, expr.right)
            }
            ExprKind::UnaryOp(expr) => {
                write!(f, "{}{}", expr.op, expr.term)
            }
            ExprKind::Expression(inner) => write!(f, "({})", inner),
            ExprKind::Const(constant) => write!(f, "{constant}"),
            ExprKind::EnumeratedValue(value) => write!(f, "{value}"),
            ExprKind::Variable(var) => write!(f, "{var}"),
            ExprKind::Function(func) => {
                write!(f, "{}(", func.name)?;
                for (i, param) in func.param_assignment.iter().enumerate() {
                    if i > 0 {
                        write!(f, ", ")?;
                    }
                    write!(f, "{param}")?;
                }
                write!(f, ")")
            }
            ExprKind::MethodCall(call) => {
                write!(f, "{}.{}(", call.receiver, call.method)?;
                for (i, param) in call.params.iter().enumerate() {
                    if i > 0 {
                        write!(f, ", ")?;
                    }
                    write!(f, "{param}")?;
                }
                write!(f, ")")
            }
            ExprKind::LateBound(late) => write!(f, "{}", late.value),
            ExprKind::Ref(var) => write!(f, "REF({var})"),
            ExprKind::Deref(expr) => write!(f, "{expr}^"),
            ExprKind::Null(_) => write!(f, "NULL"),
        }
    }
}

/// Comparison operators.
///
/// See section 3.2.2, especially table 52.
#[derive(Debug, PartialEq, Clone)]
pub enum CompareOp {
    Or,
    Xor,
    And,
    /// CODESYS/TwinCAT short-circuit `AND` (Beckhoff/CODESYS extension).
    /// Kept as a distinct variant from `And` (not normalized) since the
    /// short-circuit vs. eager evaluation distinction is real and
    /// externally-visible in TwinCAT/CODESYS itself.
    AndThen,
    /// CODESYS/TwinCAT short-circuit `OR`, the dual of [`CompareOp::AndThen`].
    OrElse,
    Eq,
    Ne,
    Lt,
    Gt,
    LtEq,
    GtEq,
}

impl CompareOp {
    /// The operator's source spelling.
    ///
    /// This is the single definition of how a `CompareOp` is written: both
    /// [`fmt::Display`] and the `plc2plc` renderer read it, so an operator
    /// added to this enum is spelled once.
    pub fn as_str(&self) -> &'static str {
        match self {
            CompareOp::Or => "OR",
            CompareOp::Xor => "XOR",
            CompareOp::And => "AND",
            CompareOp::AndThen => "AND_THEN",
            CompareOp::OrElse => "OR_ELSE",
            CompareOp::Eq => "=",
            CompareOp::Ne => "<>",
            CompareOp::Lt => "<",
            CompareOp::Gt => ">",
            CompareOp::LtEq => "<=",
            CompareOp::GtEq => ">=",
        }
    }
}

impl fmt::Display for CompareOp {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(f, "{}", self.as_str())
    }
}

/// Arithmetic operators.
///
/// See section 3.2.2, especially table 52.
#[derive(Debug, PartialEq, Clone)]
pub enum Operator {
    Add,
    Sub,
    Mul,
    Div,
    Mod,
    Pow,
}

impl fmt::Display for Operator {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        let symbol = match self {
            Operator::Add => "+",
            Operator::Sub => "-",
            Operator::Mul => "*",
            Operator::Div => "/",
            Operator::Mod => "MOD",
            Operator::Pow => "**",
        };
        write!(f, "{symbol}")
    }
}

/// Local operators (with single operand).
///
/// See section 3.2.2, especially table 52.
#[derive(Debug, PartialEq, Clone)]
pub enum UnaryOp {
    Neg,
    // Compliment operator (for Boolean values)
    Not,
}

impl fmt::Display for UnaryOp {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        let symbol = match self {
            UnaryOp::Neg => "-",
            UnaryOp::Not => "NOT",
        };
        write!(f, "{symbol}")
    }
}
