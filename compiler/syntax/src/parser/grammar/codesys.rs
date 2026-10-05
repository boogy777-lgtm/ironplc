//! The statements CODESYS adds to Structured Text: exception handling,
//! `JMP` and labels, `CALC`, `__WAIT`, and the `__BEGIN_IMPLEMENTATION`
//! marker.
//!
//! Each is gated by its own dialect flag through the keyword table, so a
//! dialect without the flag never reaches these rules: the word is an
//! ordinary name there.

use super::control::close_block;
use super::expressions::{call, close_group, element_call_ahead, expression, name_ref, variable};
use super::statements::{statement_list, terminator, ListEnd};
use crate::parser::state::Parser;
use crate::syntax_kind::SyntaxKind as K;

/// `__BEGIN_IMPLEMENTATION [;]`: the marker opening a POU's implementation.
/// It is not a statement and takes no terminator of its own.
pub(super) fn implementation_marker(p: &mut Parser) {
    let node = p.start();
    p.bump();
    p.eat(K::Semicolon);
    p.complete(node, K::ImplementationMarker);
}

/// `name :`, which labels the statement that follows and takes no `;`.
pub(super) fn label(p: &mut Parser) {
    let node = p.start();
    p.bump_n(2);
    p.complete(node, K::LabelStmt);
}

/// `[( expression )]`, the optional parenthesised operand of `JMP`,
/// `__WAIT` and `__THROW`.
fn optional_condition(p: &mut Parser) {
    if p.at(K::LeftParen) {
        p.bump();
        expression(p);
        close_group(p, K::RightParen, "`)`");
    }
}

/// `__TRY ... [__CATCH [([variable])] ...] [__FINALLY ...] __ENDTRY`
pub(super) fn try_statement(p: &mut Parser) {
    let node = p.start();
    p.bump();
    statement_list(p, ListEnd::Block, false);
    if p.at(K::Catch) {
        let clause = p.start();
        p.bump();
        if p.at(K::LeftParen) {
            p.bump();
            if !p.at(K::RightParen) && variable(p).is_none() {
                p.error("expected a variable");
            }
            close_group(p, K::RightParen, "`)`");
        }
        statement_list(p, ListEnd::Block, false);
        p.complete(clause, K::CatchClause);
    }
    if p.at(K::Finally) {
        let clause = p.start();
        p.bump();
        statement_list(p, ListEnd::Block, false);
        p.complete(clause, K::FinallyClause);
    }
    close_block(p, K::EndTry, "`__ENDTRY`");
    p.complete(node, K::TryStmt);
}

/// `__THROW [( expression )] ;`
pub(super) fn throw_statement(p: &mut Parser) {
    let node = p.start();
    p.bump();
    optional_condition(p);
    terminator(p, false);
    p.complete(node, K::ThrowStmt);
}

/// `JMP [( condition )] label ;`
pub(super) fn jump_statement(p: &mut Parser) {
    let node = p.start();
    p.bump();
    optional_condition(p);
    if p.name_at(0) {
        name_ref(p);
    } else {
        p.error("expected a label name");
    }
    terminator(p, false);
    p.complete(node, K::JmpStmt);
}

/// `CALC ( condition , call ) ;`
pub(super) fn calc_statement(p: &mut Parser) {
    let node = p.start();
    p.bump();
    if p.expect(K::LeftParen, "`(`") {
        expression(p);
        p.expect(K::Comma, "`,`");
        if (p.name_at(0) && p.nth_at(1, K::LeftParen)) || element_call_ahead(p) {
            call(p, false);
        } else {
            p.error("expected a function block call");
        }
        close_group(p, K::RightParen, "`)`");
    }
    terminator(p, false);
    p.complete(node, K::CalcStmt);
}

/// `__WAIT [( condition )] ;`
pub(super) fn wait_statement(p: &mut Parser) {
    let node = p.start();
    p.bump();
    optional_condition(p);
    terminator(p, false);
    p.complete(node, K::WaitStmt);
}
