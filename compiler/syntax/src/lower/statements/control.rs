//! The selection and iteration statements: `IF`, `CASE`, `FOR`, `WHILE` and
//! `REPEAT`.
//!
//! A block is positioned as written, from its first word through the closing
//! word, without the `;` after it. The bodies are lists, lowered by the rule
//! of a list, and a `CASE` label is one of the rows of `LABELS`.

use super::super::expressions::lower_expr;
use super::super::literals::lower_constant;
use super::super::names::lower_name;
use super::super::tree::{child_of, children_of, token_of};
use super::super::values::{
    lower_bare_value, lower_integer, lower_qualified_value, lower_subrange,
};
use super::super::LowerCx;
use super::{body_of, condition_of};
use crate::syntax_kind::{SyntaxKind as K, SyntaxNode};
use ironplc_dsl::common::{BitStringLiteral, ConstantKind};
use ironplc_dsl::core::SourceSpan;
use ironplc_dsl::diagnostic::Diagnostic;
use ironplc_dsl::textual::{
    Case, CaseSelectionKind, CaseStatementGroup, ElseIf, For, If, Repeat, StmtKind, While,
};

/// The body of the `ELSE` clause of a block; empty when there is none.
fn else_body(cx: &LowerCx, node: &SyntaxNode) -> Result<Vec<StmtKind>, Diagnostic> {
    child_of(node, K::ElseClause)
        .map(|clause| body_of(cx, &clause))
        .transpose()
        .map(Option::unwrap_or_default)
}

/// `IF condition THEN body {ELSIF condition THEN body} [ELSE body] END_IF`
pub fn if_statement(
    cx: &LowerCx,
    node: &SyntaxNode,
    span: SourceSpan,
) -> Result<StmtKind, Diagnostic> {
    let else_ifs = children_of(node, K::ElsifClause)
        .map(|clause| {
            Ok(ElseIf {
                expr: condition_of(cx, &clause)?,
                body: body_of(cx, &clause)?,
            })
        })
        .collect::<Result<Vec<_>, Diagnostic>>()?;
    Ok(StmtKind::If(If {
        expr: condition_of(cx, node)?,
        body: body_of(cx, node)?,
        else_ifs,
        else_body: else_body(cx, node)?,
        span,
    }))
}

/// `WHILE condition DO body END_WHILE`
pub fn while_statement(
    cx: &LowerCx,
    node: &SyntaxNode,
    span: SourceSpan,
) -> Result<StmtKind, Diagnostic> {
    Ok(StmtKind::While(While {
        condition: condition_of(cx, node)?,
        body: body_of(cx, node)?,
        span,
    }))
}

/// `REPEAT body UNTIL condition END_REPEAT`: the condition follows the body.
pub fn repeat_statement(
    cx: &LowerCx,
    node: &SyntaxNode,
    span: SourceSpan,
) -> Result<StmtKind, Diagnostic> {
    let until = child_of(node, K::StatementList)
        .and_then(|body| body.next_sibling())
        .ok_or_else(|| cx.missing(node, "a condition"))?;
    Ok(StmtKind::Repeat(Repeat {
        until: lower_expr(cx, &until)?,
        body: body_of(cx, node)?,
        span,
    }))
}

/// `FOR variable := from TO to [BY step] DO body END_FOR`: the loop variable
/// is the first node, the bounds and the step follow it, and the body is
/// last.
pub fn for_statement(
    cx: &LowerCx,
    node: &SyntaxNode,
    span: SourceSpan,
) -> Result<StmtKind, Diagnostic> {
    let mut nodes = node
        .children()
        .filter(|child| child.kind() != K::StatementList);
    let control = nodes
        .next()
        .ok_or_else(|| cx.missing(node, "a loop variable"))
        .and_then(|name| lower_name(cx, &name))?;
    let mut bound = |what: &str| {
        nodes
            .next()
            .ok_or_else(|| cx.missing(node, what))
            .and_then(|value| lower_expr(cx, &value))
    };
    let from = bound("a start")?;
    let to = bound("an end")?;
    let step = nodes.next().map(|step| lower_expr(cx, &step)).transpose()?;
    Ok(StmtKind::For(For {
        control,
        from,
        to,
        step,
        body: body_of(cx, node)?,
        span,
    }))
}

/// How the nodes of a `CASE` label are read.
#[derive(Clone, Copy)]
enum Shape {
    /// The label has a token of this kind.
    Token(K),
    /// The label starts with a node of this kind.
    Child(K),
}

impl Shape {
    fn is_in(self, label: &SyntaxNode) -> bool {
        match self {
            Shape::Token(kind) => token_of(label, &[kind]).is_some(),
            Shape::Child(kind) => label
                .first_child()
                .is_some_and(|child| child.kind() == kind),
        }
    }
}

/// A rule: builds the selector a label denotes.
type LabelRule = fn(&LowerCx, &SyntaxNode) -> Result<CaseSelectionKind, Diagnostic>;

/// The forms of a label, in the order they are told apart: a range
/// (`1..5`), a qualified enumeration value (`Color#Red`), a bit string
/// (`16#FF`), a number, and a name, which is an enumeration value.
const LABELS: &[(Shape, LabelRule)] = &[
    (Shape::Token(K::Range), range),
    (Shape::Token(K::Hash), qualified_value),
    (Shape::Child(K::BitStringLiteral), bit_string),
    (Shape::Child(K::IntLiteral), number),
    (Shape::Child(K::NameRef), value),
];

/// `low..high`
fn range(cx: &LowerCx, label: &SyntaxNode) -> Result<CaseSelectionKind, Diagnostic> {
    lower_subrange(cx, label).map(CaseSelectionKind::Subrange)
}

/// `Type#Value`
fn qualified_value(cx: &LowerCx, label: &SyntaxNode) -> Result<CaseSelectionKind, Diagnostic> {
    lower_qualified_value(cx, label).map(CaseSelectionKind::EnumeratedValue)
}

/// `16#FF`, `8#17`, `2#1010`: a bit string, which a label spells without a
/// type.
fn bit_string(cx: &LowerCx, label: &SyntaxNode) -> Result<CaseSelectionKind, Diagnostic> {
    let literal = label
        .first_child()
        .ok_or_else(|| cx.missing(label, "a bit string"))?;
    match lower_constant(cx, &literal)? {
        ConstantKind::BitStringLiteral(BitStringLiteral { value, data_type }) => {
            Ok(CaseSelectionKind::BitStringLiteral(BitStringLiteral {
                value,
                data_type,
            }))
        }
        _ => Err(cx.missing(label, "a bit string")),
    }
}

/// `5`, `-1`, `+2`
fn number(cx: &LowerCx, label: &SyntaxNode) -> Result<CaseSelectionKind, Diagnostic> {
    let literal = label
        .first_child()
        .ok_or_else(|| cx.missing(label, "a number"))?;
    lower_integer(cx, &literal).map(CaseSelectionKind::SignedInteger)
}

/// `Red`: a name alone selects an enumeration value.
fn value(cx: &LowerCx, label: &SyntaxNode) -> Result<CaseSelectionKind, Diagnostic> {
    let name = label
        .first_child()
        .ok_or_else(|| cx.missing(label, "a name"))?;
    lower_bare_value(cx, &name).map(CaseSelectionKind::EnumeratedValue)
}

/// One selector of a branch.
fn label(cx: &LowerCx, node: &SyntaxNode) -> Result<CaseSelectionKind, Diagnostic> {
    let (_, rule) = LABELS
        .iter()
        .find(|(shape, _)| shape.is_in(node))
        .ok_or_else(|| cx.unsupported(node))?;
    rule(cx, node)
}

/// A branch of a `CASE`: its selectors, and the statements they select.
fn branch(cx: &LowerCx, node: &SyntaxNode) -> Result<CaseStatementGroup, Diagnostic> {
    Ok(CaseStatementGroup {
        selectors: children_of(node, K::CaseLabel)
            .map(|selector| label(cx, &selector))
            .collect::<Result<Vec<_>, _>>()?,
        statements: body_of(cx, node)?,
    })
}

/// `CASE selector OF {labels : body} [ELSE body] END_CASE`
pub fn case_statement(
    cx: &LowerCx,
    node: &SyntaxNode,
    span: SourceSpan,
) -> Result<StmtKind, Diagnostic> {
    Ok(StmtKind::Case(Case {
        selector: condition_of(cx, node)?,
        statement_groups: children_of(node, K::CaseBranch)
            .map(|group| branch(cx, &group))
            .collect::<Result<Vec<_>, _>>()?,
        else_body: else_body(cx, node)?,
        span,
    }))
}
