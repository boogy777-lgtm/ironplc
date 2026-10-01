//! Selection and iteration statements: `IF`, `CASE`, `FOR`, `WHILE`,
//! `REPEAT`.
//!
//! The bodies follow the legacy grammar's rules about emptiness: the `THEN`
//! body of `IF` may be empty, every other body needs at least one item (a
//! lone `;` counts). A `CASE` branch with no statement at all is accepted
//! only when the dialect allows missing semicolons, where the legacy pipeline
//! inserts an empty statement for it.

use super::expressions::{expression, name_ref};
use super::statements::{statement_list, terminator, ListEnd};
use crate::parser::recovery::BLOCK_END;
use crate::parser::state::Parser;
use crate::syntax_kind::SyntaxKind as K;

/// `IF c THEN ... {ELSIF c THEN ...} [ELSE ...] END_IF`
pub(super) fn if_statement(p: &mut Parser) {
    let node = p.start();
    p.bump();
    expression(p);
    p.expect(K::Then, "`THEN`");
    statement_list(p, ListEnd::Block, false);
    while p.at(K::Elsif) {
        let clause = p.start();
        p.bump();
        expression(p);
        p.expect(K::Then, "`THEN`");
        statement_list(p, ListEnd::Block, true);
        p.complete(clause, K::ElsifClause);
    }
    else_clause(p);
    close_block(p, K::EndIf, "`END_IF`");
    p.complete(node, K::IfStmt);
}

fn else_clause(p: &mut Parser) {
    if p.at(K::Else) {
        let clause = p.start();
        p.bump();
        statement_list(p, ListEnd::Block, true);
        p.complete(clause, K::ElseClause);
    }
}

/// The `END_*` word that closes a block statement and its `;`, which the
/// dialect may let the source omit.
pub(super) fn close_block(p: &mut Parser, closer: K, what: &str) {
    if p.expect(closer, what) {
        terminator(p, true);
    }
}

/// `FOR v := a TO b [BY s] DO ... END_FOR`
pub(super) fn for_statement(p: &mut Parser) {
    let node = p.start();
    p.bump();
    if p.name_at(0) {
        name_ref(p);
    } else {
        p.error("expected the loop variable");
    }
    p.expect(K::Assignment, "`:=`");
    expression(p);
    p.expect(K::To, "`TO`");
    expression(p);
    if p.eat(K::By) {
        expression(p);
    }
    p.expect(K::Do, "`DO`");
    statement_list(p, ListEnd::Block, true);
    close_block(p, K::EndFor, "`END_FOR`");
    p.complete(node, K::ForStmt);
}

/// `WHILE c DO ... END_WHILE`
pub(super) fn while_statement(p: &mut Parser) {
    let node = p.start();
    p.bump();
    expression(p);
    p.expect(K::Do, "`DO`");
    statement_list(p, ListEnd::Block, true);
    close_block(p, K::EndWhile, "`END_WHILE`");
    p.complete(node, K::WhileStmt);
}

/// `REPEAT ... UNTIL c END_REPEAT`
pub(super) fn repeat_statement(p: &mut Parser) {
    let node = p.start();
    p.bump();
    statement_list(p, ListEnd::Block, true);
    p.expect(K::Until, "`UNTIL`");
    expression(p);
    close_block(p, K::EndRepeat, "`END_REPEAT`");
    p.complete(node, K::RepeatStmt);
}

/// `CASE e OF {labels : statements} [ELSE ...] END_CASE`
pub(super) fn case_statement(p: &mut Parser) {
    let node = p.start();
    p.bump();
    expression(p);
    p.expect(K::Of, "`OF`");
    p.case_depth += 1;
    while !p.at_eof() && !p.at_any(BLOCK_END) {
        let before = p.position();
        case_branch(p);
        if p.position() == before {
            p.bump_as_error("expected a case selector");
        }
    }
    else_clause(p);
    p.case_depth -= 1;
    close_block(p, K::EndCase, "`END_CASE`");
    p.complete(node, K::CaseStmt);
}

fn case_branch(p: &mut Parser) {
    let node = p.start();
    loop {
        if !case_label(p) || !p.eat(K::Comma) {
            break;
        }
    }
    p.expect(K::Colon, "`:`");
    let required = !p.options.allow_missing_semicolon;
    statement_list(p, ListEnd::CaseBranch, required);
    p.complete(node, K::CaseBranch);
}

/// True when the cursor is at the first selector of a `CASE` branch rather
/// than at a statement: a number, a signed number, or a name that a `:`, `,`,
/// `..` or `#` follows (no statement starts that way).
pub(super) fn case_selector_ahead(p: &Parser) -> bool {
    match p.nth(0) {
        Some(K::Plus | K::Minus) => p.adjacent(0) && p.nth(1) == Some(K::IntegerLit),
        Some(K::IntegerLit | K::HexLit | K::OctLit | K::BinLit) => true,
        Some(_) if p.variable_name_at(0) => {
            matches!(p.nth(1), Some(K::Colon | K::Comma | K::Range | K::Hash))
        }
        _ => false,
    }
}

/// One selector: a number, a range `a..b` of numbers or constant names, a
/// bit-string literal, or an enumeration value, optionally `Type#Value`.
/// Returns false, having consumed nothing, when no selector starts here.
fn case_label(p: &mut Parser) -> bool {
    let node = p.start();
    if p.at_any(&[K::HexLit, K::OctLit, K::BinLit]) {
        let literal = p.start();
        p.bump();
        p.complete(literal, K::BitStringLiteral);
    } else if p.name_at(0) && p.nth_at(1, K::Hash) && p.adjacent(0) {
        qualified_enumeration_value(p);
    } else if bound_ahead(p) {
        bound(p);
        if p.at(K::Range) {
            p.bump();
            if bound_ahead(p) {
                bound(p);
            } else {
                p.error("expected the upper bound of the range");
            }
        }
    } else if p.variable_name_at(0) {
        name_ref(p);
    } else {
        p.error("expected a case selector");
        p.abandon(node);
        return false;
    }
    p.complete(node, K::CaseLabel);
    true
}

/// A range bound: a signed integer or a constant name.
pub(super) fn bound_ahead(p: &Parser) -> bool {
    match p.nth(0) {
        Some(K::IntegerLit) => true,
        Some(K::Plus | K::Minus) => p.adjacent(0) && p.nth(1) == Some(K::IntegerLit),
        _ => p.name_at(0),
    }
}

pub(super) fn bound(p: &mut Parser) {
    if p.name_at(0) {
        name_ref(p);
        return;
    }
    let literal = p.start();
    if p.at_any(&[K::Plus, K::Minus]) {
        p.bump();
    }
    p.bump();
    p.complete(literal, K::IntLiteral);
}

/// `Type#Value`.
fn qualified_enumeration_value(p: &mut Parser) {
    name_ref(p);
    p.bump();
    if p.variable_name_at(0) {
        name_ref(p);
    } else {
        p.error("expected the enumeration value");
    }
}
