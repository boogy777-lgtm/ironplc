//! Sequential function chart elements in a body: `INITIAL_STEP`, `STEP`,
//! `TRANSITION` and `ACTION`.
//!
//! The legacy grammar reads a chart as one or more networks, each an initial
//! step followed by any steps, transitions and actions. An action's body is a
//! body again, so a chart can nest; the depth guard bounds it.

use super::common::{close, declared_name, item_terminator, skip_stray, NameClass};
use super::configuration::PRIORITY_WORD;
use super::expressions::{close_group, expression, name_ref};
use super::pou::body;
use crate::expectation::{Expected, Item};
use crate::kind_set::KindSet;
use crate::parser::recovery::BLOCK_END;
use crate::parser::state::Parser;
use crate::syntax_kind::SyntaxKind as K;
use ironplc_problems::text;

/// The qualifiers of an action association. The legacy grammar matches them
/// by their exact upper-case spelling.
const QUALIFIERS: &[&str] = &["N", "R", "S", "L", "D", "P", "SD", "DS", "SL", "P1", "P0"];

/// The qualifiers that name a time as well.
const TIMED: &[&str] = &["SD", "DS", "SL", "P1", "P0"];

/// The elements of a chart, from an `INITIAL_STEP` on, up to the first token
/// that is not one.
fn elements(p: &mut Parser) {
    let node = p.start();
    while let Some(kind) = p.nth(0) {
        match kind {
            K::InitialStep => step(p, K::InitialStepDecl),
            K::Step => step(p, K::StepDecl),
            K::Action => action(p),
            K::Transition => transition(p),
            _ => break,
        }
    }
    p.complete(node, K::SfcBody);
}

/// A chart in a body, with the depth guard for charts that nest through
/// action bodies.
pub(super) fn chart(p: &mut Parser) {
    p.guarded(elements, |p| p.skip_nested_statement());
}

/// `INITIAL_STEP name : {action(qualifier, indicators);} END_STEP`, and `STEP`
/// the same.
fn step(p: &mut Parser, node_kind: K) {
    let node = p.start();
    p.bump();
    declared_name(p, NameClass::Plain);
    p.expect(K::Colon);
    while !p.at_eof() && !p.at(K::EndStep) && !p.at_any(BLOCK_END) {
        let before = p.position();
        if p.at(K::Semicolon) {
            p.bump();
        } else {
            association(p);
        }
        if p.position() == before {
            p.bump_as_error(p.expectation(Item::ActionAssociation));
        }
    }
    close(p, K::EndStep);
    p.complete(node, node_kind);
}

/// `action ( [qualifier [, time]] [, indicator {, indicator}] ) ;`
fn association(p: &mut Parser) {
    let node = p.start();
    if p.name_at(0) {
        name_ref(p);
    } else {
        p.expected(Item::ActionName);
        super::common::skip_declaration(p);
        p.abandon(node);
        return;
    }
    if p.expect(K::LeftParen) {
        qualifier(p);
        if p.at(K::Comma) {
            p.bump();
            while p.variable_name_at(0) {
                name_ref(p);
                if !p.eat(K::Comma) {
                    break;
                }
            }
        }
        close_group(p, K::RightParen);
    }
    p.complete(node, K::ActionAssociation);
    item_terminator(p);
}

fn qualifier(p: &mut Parser) {
    let text = p.nth_text(0);
    if p.nth(0) != Some(K::Ident) || !QUALIFIERS.contains(&text) {
        return;
    }
    let node = p.start();
    p.bump();
    if TIMED.contains(&text) {
        p.expect(K::Comma);
        expression(p);
    }
    p.complete(node, K::ActionQualifier);
}

/// `ACTION name : body END_ACTION`
fn action(p: &mut Parser) {
    let node = p.start();
    p.bump();
    declared_name(p, NameClass::Plain);
    p.expect(K::Colon);
    body(p, true);
    close(p, K::EndAction);
    p.complete(node, K::ActionDecl);
}

/// `TRANSITION [name] [(PRIORITY := n)] FROM steps TO steps := condition ;
/// END_TRANSITION`
fn transition(p: &mut Parser) {
    let node = p.start();
    p.bump();
    if p.name_at(0) {
        declared_name(p, NameClass::Plain);
    }
    if p.at(K::LeftParen) {
        priority(p);
    }
    if p.expect(K::From) {
        steps(p);
        if p.expect(K::To) {
            steps(p);
            condition(p);
        }
    }
    if !p.at(K::EndTransition) && !p.at_eof() && !p.at_any(BLOCK_END) {
        skip_stray(
            p,
            K::EndTransition,
            KindSet::EMPTY,
            p.expectation(K::EndTransition),
        );
    }
    close(p, K::EndTransition);
    p.complete(node, K::TransitionDecl);
}

/// `( PRIORITY := n )`
fn priority(p: &mut Parser) {
    let node = p.start();
    p.bump();
    if p.nth(0) == Some(K::Ident) && p.nth_text(0) == PRIORITY_WORD {
        p.bump();
    } else {
        p.expected(Expected::words([PRIORITY_WORD]));
    }
    p.expect(K::Assignment);
    if p.at(K::IntegerLit) {
        let literal = p.start();
        p.bump();
        p.complete(literal, K::IntLiteral);
    } else {
        p.expected(Item::Priority);
    }
    close_group(p, K::RightParen);
    p.complete(node, K::TransitionPriority);
}

/// One step name, or two or more in parentheses.
fn steps(p: &mut Parser) {
    let node = p.start();
    if p.at(K::LeftParen) {
        p.bump();
        let mut count = 0usize;
        loop {
            if p.name_at(0) {
                name_ref(p);
                count += 1;
            } else {
                p.expected(Item::StepName);
            }
            if !p.eat(K::Comma) {
                break;
            }
        }
        if count < 2 {
            p.error(text::syntax_error_expected_at_least_two_steps());
        }
        close_group(p, K::RightParen);
    } else if p.name_at(0) {
        name_ref(p);
    } else {
        p.expected(Item::StepName);
    }
    p.complete(node, K::StepList);
}

/// `:= expression ;`
fn condition(p: &mut Parser) {
    let node = p.start();
    if p.expect(K::Assignment) {
        expression(p);
        p.expect(K::Semicolon);
    }
    p.complete(node, K::TransitionCondition);
}
