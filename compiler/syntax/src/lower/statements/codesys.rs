//! The statements CODESYS adds to Structured Text: exception handling, `JMP`
//! and labels, `CALC`, `__WAIT`, and the `__BEGIN_IMPLEMENTATION` marker.
//!
//! A statement that is a word with an optional operand (`JMP`, `__WAIT`,
//! `__THROW`) and a clause that is a word with a body (`__CATCH`) are anchored
//! at the word; the statements that enclose others or end in a closing word
//! are positioned as written.

use super::super::expressions::{lower_call, lower_expr};
use super::super::names::{lower_id, lower_name};
use super::super::tree::{child_of, significant_tokens};
use super::super::variables::lower_variable;
use super::super::LowerCx;
use super::{anchor, body_of};
use crate::syntax_kind::{SyntaxKind as K, SyntaxNode};
use ironplc_dsl::core::SourceSpan;
use ironplc_dsl::diagnostic::Diagnostic;
use ironplc_dsl::textual::{
    CatchClause, ConditionalCall, Expr, Jump, LabelStatement, StmtKind, Throw, TryCatch, Wait,
};

/// The operand a node writes in parentheses after its word, when it has one.
fn operand_of(cx: &LowerCx, node: &SyntaxNode) -> Result<Option<Expr>, Diagnostic> {
    node.first_child()
        .map(|operand| lower_expr(cx, &operand))
        .transpose()
}

/// `__TRY body [__CATCH [([variable])] body] [__FINALLY body] __ENDTRY`
pub fn try_statement(
    cx: &LowerCx,
    node: &SyntaxNode,
    span: SourceSpan,
) -> Result<StmtKind, Diagnostic> {
    let catch = child_of(node, K::CatchClause)
        .map(|clause| -> Result<CatchClause, Diagnostic> {
            Ok(CatchClause {
                exception: clause
                    .first_child()
                    .filter(|variable| variable.kind() != K::StatementList)
                    .map(|variable| lower_variable(cx, &variable))
                    .transpose()?,
                body: body_of(cx, &clause)?,
                span: anchor(cx, &clause)?,
            })
        })
        .transpose()?;
    let finally_body = child_of(node, K::FinallyClause)
        .map(|clause| body_of(cx, &clause))
        .transpose()?
        .unwrap_or_default();
    Ok(StmtKind::TryCatch(TryCatch {
        body: body_of(cx, node)?,
        catch,
        finally_body,
        span,
    }))
}

/// `__THROW [(value)]`
pub fn throw_statement(
    cx: &LowerCx,
    node: &SyntaxNode,
    span: SourceSpan,
) -> Result<StmtKind, Diagnostic> {
    Ok(StmtKind::Throw(Throw {
        value: operand_of(cx, node)?,
        span,
    }))
}

/// `JMP [(condition)] label`: the label is the last node and the condition,
/// when there is one, the node before it.
pub fn jump(cx: &LowerCx, node: &SyntaxNode, span: SourceSpan) -> Result<StmtKind, Diagnostic> {
    let mut nodes = node.children().collect::<Vec<_>>();
    let label = nodes
        .pop()
        .ok_or_else(|| cx.missing(node, "a label"))
        .and_then(|label| lower_name(cx, &label))?;
    let condition = nodes
        .pop()
        .map(|condition| lower_expr(cx, &condition))
        .transpose()?;
    Ok(StmtKind::Jump(Jump {
        condition,
        label,
        span,
    }))
}

/// `name :`, which labels the statement that follows.
pub fn label(cx: &LowerCx, node: &SyntaxNode, span: SourceSpan) -> Result<StmtKind, Diagnostic> {
    let name = significant_tokens(node)
        .first()
        .map(|token| lower_id(cx, token))
        .ok_or_else(|| cx.missing(node, "a name"))?;
    Ok(StmtKind::Label(LabelStatement { name, span }))
}

/// `CALC(condition, call)`: the call runs when the condition holds.
pub fn conditional_call(
    cx: &LowerCx,
    node: &SyntaxNode,
    span: SourceSpan,
) -> Result<StmtKind, Diagnostic> {
    let mut nodes = node.children();
    let condition = nodes
        .next()
        .ok_or_else(|| cx.missing(node, "a condition"))
        .and_then(|condition| lower_expr(cx, &condition))?;
    let call = nodes
        .next()
        .ok_or_else(|| cx.missing(node, "a call"))
        .and_then(|call| lower_call(cx, &call))?;
    let call = call
        .into_fb_call()
        .ok_or_else(|| cx.missing(node, "a function block instance to call"))?;
    Ok(StmtKind::ConditionalCall(ConditionalCall {
        condition,
        call,
        span,
    }))
}

/// `__WAIT [(condition)]`
pub fn wait(cx: &LowerCx, node: &SyntaxNode, span: SourceSpan) -> Result<StmtKind, Diagnostic> {
    Ok(StmtKind::Wait(Wait {
        condition: operand_of(cx, node)?,
        span,
    }))
}

/// `__BEGIN_IMPLEMENTATION`: the marker that opens an implementation.
pub fn implementation_marker(
    _cx: &LowerCx,
    _node: &SyntaxNode,
    span: SourceSpan,
) -> Result<StmtKind, Diagnostic> {
    Ok(StmtKind::BeginImplementation(span))
}
