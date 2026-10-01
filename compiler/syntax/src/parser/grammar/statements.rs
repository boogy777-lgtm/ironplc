//! Statement lists and the simple statements.
//!
//! The legacy grammar requires a `;` after every statement; the keyword
//! statements that end in an `END_*` word may omit it when the dialect sets
//! `allow_missing_semicolon`. The legacy pipeline inserts the missing `;`
//! as a synthetic token before parsing. Here the terminator is simply
//! optional at that one point, so the tree holds only source tokens.

use super::codesys;
use super::control;
use super::expressions::{call, call_ahead, expression, variable};
use crate::parser::recovery::BLOCK_END;
use crate::parser::state::Parser;
use crate::syntax_kind::SyntaxKind as K;

/// What ends a statement list.
#[derive(Clone, Copy, PartialEq, Eq)]
pub(in crate::parser) enum ListEnd {
    /// Only the end of the input; a stray block word is an error.
    TopLevel,
    /// Any block word: the enclosing construct decides whether it belongs.
    Block,
    /// A block word, or the selector of the next `CASE` branch.
    CaseBranch,
}

/// A statement list. `required` demands at least one item (a `;` alone, a
/// label or a statement counts), which is how the legacy grammar treats the
/// bodies of loops, `ELSIF`, `ELSE` and `CASE` branches.
pub(in crate::parser) fn statement_list(p: &mut Parser, end: ListEnd, required: bool) {
    let list = p.start();
    let mut items = 0usize;
    while !p.at_eof() && !list_ends(p, end) {
        let before = p.position();
        item(p);
        if p.position() == before {
            p.bump_as_error("unexpected token in statement list");
        }
        items += 1;
    }
    if required && items == 0 {
        p.error("expected a statement");
    }
    p.complete(list, K::StatementList);
}

fn list_ends(p: &Parser, end: ListEnd) -> bool {
    match end {
        ListEnd::TopLevel => false,
        ListEnd::Block => p.at_any(BLOCK_END),
        ListEnd::CaseBranch => p.at_any(BLOCK_END) || control::case_selector_ahead(p),
    }
}

/// One item of a list: an empty statement, the implementation marker, a
/// label, or a statement.
fn item(p: &mut Parser) {
    if p.at(K::Semicolon) {
        let node = p.start();
        p.bump();
        p.complete(node, K::EmptyStmt);
    } else if p.at(K::BeginImplementation) {
        codesys::implementation_marker(p);
    } else if label_ahead(p) {
        codesys::label(p);
    } else {
        statement(p);
    }
}

/// `name :` in statement position. Inside a `CASE` it selects the next
/// branch instead, and only a plain identifier token can be a label: a
/// keyword that the dialect leaves disabled is not one.
fn label_ahead(p: &Parser) -> bool {
    p.options.allow_jump_statement
        && p.case_depth == 0
        && p.nth(0) == Some(K::Ident)
        && p.nth_at(1, K::Colon)
}

/// Consumes a statement's `;`. After an `END_*` keyword it may be omitted
/// when the dialect allows missing semicolons.
pub(in crate::parser) fn terminator(p: &mut Parser, optional: bool) {
    if !p.eat(K::Semicolon) && !(optional && p.options.allow_missing_semicolon) {
        p.error("expected `;`");
    }
}

fn statement(p: &mut Parser) {
    p.guarded(statement_by_keyword, |p| p.skip_nested_statement());
}

fn statement_by_keyword(p: &mut Parser) {
    let Some(kind) = p.nth(0).filter(|kind| p.at(*kind)) else {
        return assignment_or_call(p);
    };
    match kind {
        K::If => control::if_statement(p),
        K::Case => control::case_statement(p),
        K::For => control::for_statement(p),
        K::While => control::while_statement(p),
        K::Repeat => control::repeat_statement(p),
        K::Exit => keyword_statement(p, K::ExitStmt),
        K::Continue => keyword_statement(p, K::ContinueStmt),
        K::Return => keyword_statement(p, K::ReturnStmt),
        K::Try => codesys::try_statement(p),
        K::Throw => codesys::throw_statement(p),
        K::Jmp => codesys::jump_statement(p),
        K::Calc => codesys::calc_statement(p),
        K::Wait => codesys::wait_statement(p),
        _ => assignment_or_call(p),
    }
}

/// `EXIT;`, `CONTINUE;` and `RETURN;`.
fn keyword_statement(p: &mut Parser, node_kind: K) {
    let node = p.start();
    p.bump();
    terminator(p, false);
    p.complete(node, node_kind);
}

/// The assignment operator at the cursor, as its token count: `:=` (1),
/// `S=`, `R=` and `REF=` (2, the two tokens touching).
fn assignment_operator(p: &Parser) -> Option<usize> {
    if p.at(K::Assignment) {
        return Some(1);
    }
    let bind_word = p.nth_is_word(0, "S") || p.nth_is_word(0, "R") || p.nth_is_word(0, "REF");
    let ref_keyword = p.at(K::Ref);
    ((bind_word || ref_keyword) && p.nth_at(1, K::Equal) && p.adjacent(0)).then_some(2)
}

fn assignment_or_call(p: &mut Parser) {
    if call_ahead(p, false) {
        let node = p.start();
        call(p, false);
        terminator(p, false);
        p.complete(node, K::CallStmt);
        return;
    }
    let Some(mut target) = variable(p) else {
        return p.skip_statement("expected a statement");
    };
    if p.at(K::Caret) && p.nth_at(1, K::Assignment) {
        let deref = p.precede(target);
        p.bump();
        target = p.complete(deref, K::DerefExpr);
    }
    let node = p.precede(target);
    let Some(operator_len) = assignment_operator(p) else {
        p.skip_statement("expected `:=` after the assignment target");
        p.complete(node, K::ErrorNode);
        return;
    };
    let binds_reference = operator_len == 2 && (p.at(K::Ref) || p.nth_is_word(0, "REF"));
    let operator = p.start();
    p.bump_n(operator_len);
    p.complete(operator, K::AssignOp);
    if binds_reference {
        if variable(p).is_none() {
            p.error("expected a variable after `REF=`");
        }
    } else {
        expression(p);
    }
    terminator(p, false);
    p.complete(node, K::AssignStmt);
}
