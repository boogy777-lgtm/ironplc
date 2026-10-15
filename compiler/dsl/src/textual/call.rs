//! Calls and their arguments: function block and method invocation, and parameter assignments.

use super::{Expr, ExprKind, SelfRefVariable, Variable};
use crate::core::{Id, Located, SourceSpan};
use std::fmt;

use crate::fold::Fold;
use crate::visitor::Visitor;
use dsl_macro_derive::{Located, Recurse};

/// Function block invocation.
///
/// See section 3.2.3.
#[derive(Debug, PartialEq, Clone, Recurse, Located)]
pub struct FbCall {
    /// The function block instance that is called: a variable, named or an
    /// array element. A plain `inst(...)` is the named case of the same callee.
    pub callee: Variable,
    pub params: Vec<ParamAssignmentKind>,
    pub position: SourceSpan,
}

/// The instance a [`MethodCall`] is invoked on.
#[derive(Debug, PartialEq, Clone, Recurse)]
pub enum MethodReceiver {
    /// A function block instance named by a variable: `instance.M()`.
    Instance(Id),
    /// The enclosing instance itself: `THIS^.M()` or `SUPER^.M()`.
    SelfRef(SelfRefVariable),
}

impl fmt::Display for MethodReceiver {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            MethodReceiver::Instance(name) => f.write_fmt(format_args!("{name}")),
            MethodReceiver::SelfRef(self_ref) => f.write_fmt(format_args!("{self_ref}")),
        }
    }
}

impl Located for MethodReceiver {
    fn span(&self) -> SourceSpan {
        match self {
            MethodReceiver::Instance(name) => name.span(),
            MethodReceiver::SelfRef(self_ref) => self_ref.span(),
        }
    }
}

/// Method invocation: `instance.MethodName(args)` (OOP extension, ADR-0041
/// Phase 1). The same node appears in both positions: as a statement
/// ([`StmtKind::MethodCall`]), where any return value is discarded, and in
/// an expression ([`ExprKind::MethodCall`]), where the method must declare
/// a return type and the call's value is that return value.
#[derive(Debug, PartialEq, Clone, Recurse, Located)]
pub struct MethodCall {
    /// The function block instance the method is called on.
    pub receiver: MethodReceiver,
    /// Name of the method being called.
    pub method: Id,
    pub params: Vec<ParamAssignmentKind>,
    pub position: SourceSpan,
}

/// Input argument to a function or function block invocation.
/// The input is mapped based on the order in a sequence. Also known
/// as a non-formal input.
///
/// See section 3.2.3.
#[derive(Debug, PartialEq, Clone, Recurse)]
pub struct PositionalInput {
    pub expr: Expr,
}

/// Input argument to a function or function block invocation.
/// The input is mapped based on the specified name. Also known as
/// a formal input.
///
/// See section 3.2.3.
#[derive(Debug, PartialEq, Clone, Recurse)]
pub struct NamedInput {
    pub name: Id,
    pub expr: Expr,
}

/// Output argument captured from a function or function block invocation.
///
/// See section 3.2.3.
#[derive(Debug, PartialEq, Clone, Recurse)]
pub struct Output {
    #[recurse(ignore)]
    pub not: bool,
    pub src: Id,
    pub tgt: Variable,
}

#[derive(Debug, PartialEq, Clone, Recurse)]
pub enum ParamAssignmentKind {
    PositionalInput(PositionalInput),
    NamedInput(NamedInput),
    Output(Output),
}

impl ParamAssignmentKind {
    /// The expression this argument passes to the callee, for a positional
    /// or named input. An output assignment passes nothing in.
    pub fn input_expr(&self) -> Option<&Expr> {
        match self {
            ParamAssignmentKind::PositionalInput(input) => Some(&input.expr),
            ParamAssignmentKind::NamedInput(input) => Some(&input.expr),
            ParamAssignmentKind::Output(_) => None,
        }
    }

    pub fn positional(expr: ExprKind) -> ParamAssignmentKind {
        ParamAssignmentKind::PositionalInput(PositionalInput {
            expr: Expr::new(expr),
        })
    }

    pub fn named(name: &str, expr: ExprKind) -> ParamAssignmentKind {
        ParamAssignmentKind::NamedInput(NamedInput {
            name: Id::from(name),
            expr: Expr::new(expr),
        })
    }
}

impl fmt::Display for ParamAssignmentKind {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            ParamAssignmentKind::PositionalInput(input) => write!(f, "{}", input.expr),
            ParamAssignmentKind::NamedInput(input) => write!(f, "{} := {}", input.name, input.expr),
            ParamAssignmentKind::Output(output) => {
                if output.not {
                    write!(f, "NOT {} => {}", output.src, output.tgt)
                } else {
                    write!(f, "{} => {}", output.src, output.tgt)
                }
            }
        }
    }
}
