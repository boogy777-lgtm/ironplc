//! Statements: the node kinds that do something to [`StmtKind`].
//!
//! `lower_statement` is the one dispatch: the kind of the node selects its row
//! in `STATEMENTS`, and the rule of the row builds the statement. A statement
//! list is lowered by walking its items in order, so the length of a list costs
//! the stack nothing; only nesting recurses (a body holds a list, and the tree
//! is no deeper than [`crate::MAX_DEPTH`]).
//!
//! What the legacy grammar decided by ordered choice is data here:
//!
//! - the assignment operators and what each means are the rows of
//!   `ASSIGNMENT_OPERATORS`: the plain `:=`, the set and reset binds and the
//!   reference bind, which differ in a flag and in what the right side is;
//! - the shape of a `CASE` label (a range, a qualified enumeration value, a
//!   bit string, a number, a name) is a row of `LABELS` in `control`;
//! - an item that writes no statement (the empty statement) is a row of
//!   `WRITES_NOTHING`.
//!
//! The position a statement records is a column of the table, its `Extent`,
//! and is taken from the tree and never from the text: the statement as written
//! without its `;`, the word it is anchored at, or a part of it (the operator
//! of an assignment). The rule is given the position and puts it where the
//! object keeps it.
//!
//! The rules are in three modules by what they build: the selection and
//! iteration statements (`control`), the CODESYS statements (`codesys`), and
//! here the assignment, the calls and the statements of one word.

mod codesys;
mod control;

#[cfg(test)]
mod tests;

use super::expressions::{lower_call, lower_expr, Callee};
use super::tree::{child_of, range_before, significant_tokens};
use super::variables::lower_variable;
use super::LowerCx;
use crate::syntax_kind::{SyntaxKind as K, SyntaxNode};
use ironplc_dsl::core::SourceSpan;
use ironplc_dsl::diagnostic::Diagnostic;
use ironplc_dsl::textual::{Assignment, Expr, ExprKind, MethodCall, StmtKind};

/// Where a statement is positioned.
enum Extent {
    /// As written: from its first token through its last, without the `;`
    /// that ends it.
    Written,
    /// At its first word: the keyword or the name it is anchored at.
    Anchor,
    /// At the part of it that is of this kind.
    Part(K),
}

/// A rule: builds the statement a node denotes, positioned at `span`.
type Rule = fn(&LowerCx, &SyntaxNode, SourceSpan) -> Result<StmtKind, Diagnostic>;

/// The rule for each kind of node that is a statement, and where the statement
/// is positioned.
const STATEMENTS: &[(K, Extent, Rule)] = &[
    (K::AssignStmt, Extent::Part(K::AssignOp), assignment),
    (K::CallStmt, Extent::Written, call),
    (K::ExitStmt, Extent::Anchor, exit),
    (K::ContinueStmt, Extent::Anchor, continue_loop),
    (K::ReturnStmt, Extent::Written, return_from),
    (K::IfStmt, Extent::Written, control::if_statement),
    (K::CaseStmt, Extent::Written, control::case_statement),
    (K::ForStmt, Extent::Written, control::for_statement),
    (K::WhileStmt, Extent::Written, control::while_statement),
    (K::RepeatStmt, Extent::Written, control::repeat_statement),
    (K::TryStmt, Extent::Written, codesys::try_statement),
    (K::ThrowStmt, Extent::Anchor, codesys::throw_statement),
    (K::JmpStmt, Extent::Written, codesys::jump),
    (K::LabelStmt, Extent::Anchor, codesys::label),
    (K::CalcStmt, Extent::Written, codesys::conditional_call),
    (K::WaitStmt, Extent::Anchor, codesys::wait),
    (
        K::ImplementationMarker,
        Extent::Anchor,
        codesys::implementation_marker,
    ),
];

/// The items of a list that write no statement.
const WRITES_NOTHING: &[K] = &[K::EmptyStmt];

/// Lowers a statement list: the statements its items write, in order.
pub fn lower_statement_list(cx: &LowerCx, node: &SyntaxNode) -> Result<Vec<StmtKind>, Diagnostic> {
    if node.kind() != K::StatementList {
        return Err(cx.unsupported(node));
    }
    let mut statements = Vec::new();
    for item in node.children() {
        statements.extend(lower_statement(cx, &item)?);
    }
    Ok(statements)
}

/// Lowers a statement node to the statement it writes, or to nothing for an
/// item that writes none.
pub fn lower_statement(cx: &LowerCx, node: &SyntaxNode) -> Result<Option<StmtKind>, Diagnostic> {
    if WRITES_NOTHING.contains(&node.kind()) {
        return Ok(None);
    }
    let (_, extent, rule) = STATEMENTS
        .iter()
        .find(|(kind, _, _)| *kind == node.kind())
        .ok_or_else(|| cx.unsupported(node))?;
    let span = extent.of(cx, node)?;
    rule(cx, node, span).map(Some)
}

impl Extent {
    /// The position of a statement node.
    fn of(&self, cx: &LowerCx, node: &SyntaxNode) -> Result<SourceSpan, Diagnostic> {
        match self {
            Extent::Written => Ok(cx.span(range_before(node, K::Semicolon))),
            Extent::Anchor => anchor(cx, node),
            Extent::Part(kind) => child_of(node, *kind)
                .map(|part| cx.node_span(&part))
                .ok_or_else(|| cx.missing(node, "the part it is positioned at")),
        }
    }
}

/// The first word of a node: the keyword or name that a statement is
/// anchored at.
fn anchor(cx: &LowerCx, node: &SyntaxNode) -> Result<SourceSpan, Diagnostic> {
    node.first_token()
        .map(|token| cx.token_span(&token))
        .ok_or_else(|| cx.missing(node, "a first word"))
}

/// The statements of the first list a node holds: the body of a block, a
/// branch or a clause.
fn body_of(cx: &LowerCx, node: &SyntaxNode) -> Result<Vec<StmtKind>, Diagnostic> {
    let list = child_of(node, K::StatementList).ok_or_else(|| cx.missing(node, "a body"))?;
    lower_statement_list(cx, &list)
}

/// The expression a node starts with: the condition of a block.
fn condition_of(cx: &LowerCx, node: &SyntaxNode) -> Result<Expr, Diagnostic> {
    let condition = node
        .first_child()
        .ok_or_else(|| cx.missing(node, "a condition"))?;
    lower_expr(cx, &condition)
}

/// What the right side of an assignment operator is.
enum Operand {
    /// A value: an expression.
    Expression,
    /// A place that the target is bound to.
    Place,
}

/// What an assignment operator means: the flags of the object it builds, and
/// what stands on its right.
struct Form {
    /// The operator as spelled, upper case and without spaces.
    spelling: &'static str,
    reference_bind: bool,
    set_bind: bool,
    reset_bind: bool,
    operand: Operand,
}

const fn form(spelling: &'static str, operand: Operand) -> Form {
    Form {
        spelling,
        reference_bind: false,
        set_bind: false,
        reset_bind: false,
        operand,
    }
}

/// The assignment operators. Every flag a row does not name is off.
const ASSIGNMENT_OPERATORS: &[Form] = &[
    form(":=", Operand::Expression),
    Form {
        set_bind: true,
        ..form("S=", Operand::Expression)
    },
    Form {
        reset_bind: true,
        ..form("R=", Operand::Expression)
    },
    Form {
        reference_bind: true,
        ..form("REF=", Operand::Place)
    },
];

/// The operator an [`K::AssignOp`] node spells: one token (`:=`), or two that
/// touch (`S=`).
fn operator_of(cx: &LowerCx, operator: &SyntaxNode) -> Result<&'static Form, Diagnostic> {
    let spelling: String = significant_tokens(operator)
        .iter()
        .map(|token| token.text().to_ascii_uppercase())
        .collect();
    ASSIGNMENT_OPERATORS
        .iter()
        .find(|form| form.spelling == spelling)
        .ok_or_else(|| cx.missing(operator, "a known assignment operator"))
}

/// `target := value`, `target S= value`, `target R= value` and
/// `target REF= place`. A target that ends in `^` is written through the
/// pointer: the assignment is flagged and holds the pointer. The assignment is
/// positioned at its operator.
fn assignment(cx: &LowerCx, node: &SyntaxNode, span: SourceSpan) -> Result<StmtKind, Diagnostic> {
    let target = node
        .first_child()
        .ok_or_else(|| cx.missing(node, "a target"))?;
    let operator = child_of(node, K::AssignOp).ok_or_else(|| cx.missing(node, "an operator"))?;
    let value = operator
        .next_sibling()
        .ok_or_else(|| cx.missing(node, "a value"))?;
    let form = operator_of(cx, &operator)?;
    let (target, deref) = match target.kind() {
        K::DerefExpr => (
            target
                .first_child()
                .ok_or_else(|| cx.missing(&target, "a pointer"))?,
            true,
        ),
        _ => (target, false),
    };
    let value = match form.operand {
        Operand::Expression => lower_expr(cx, &value)?,
        Operand::Place => Expr::new(ExprKind::Ref(Box::new(lower_variable(cx, &value)?)))
            .with_span(cx.node_span(&value)),
    };
    Ok(StmtKind::Assignment(Assignment {
        target: lower_variable(cx, &target)?,
        deref,
        ref_bind: form.reference_bind,
        set_bind: form.set_bind,
        reset_bind: form.reset_bind,
        value,
        span,
    }))
}

/// A call as a statement: a function block call, or a method call.
fn call(cx: &LowerCx, node: &SyntaxNode, _span: SourceSpan) -> Result<StmtKind, Diagnostic> {
    let call = child_of(node, K::CallExpr).ok_or_else(|| cx.missing(node, "a call"))?;
    let call = lower_call(cx, &call)?;
    Ok(match call.callee {
        Callee::Method { receiver, method } => StmtKind::MethodCall(MethodCall {
            receiver,
            method,
            params: call.params,
            position: call.span,
        }),
        Callee::Name(_) | Callee::Element(_) => {
            StmtKind::FbCall(call.into_fb_call().ok_or_else(|| cx.unsupported(node))?)
        }
    })
}

fn exit(_cx: &LowerCx, _node: &SyntaxNode, span: SourceSpan) -> Result<StmtKind, Diagnostic> {
    Ok(StmtKind::Exit(span))
}

fn continue_loop(
    _cx: &LowerCx,
    _node: &SyntaxNode,
    span: SourceSpan,
) -> Result<StmtKind, Diagnostic> {
    Ok(StmtKind::Continue(span))
}

fn return_from(
    _cx: &LowerCx,
    _node: &SyntaxNode,
    _span: SourceSpan,
) -> Result<StmtKind, Diagnostic> {
    Ok(StmtKind::Return)
}
