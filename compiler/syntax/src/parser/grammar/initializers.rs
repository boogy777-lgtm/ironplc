//! Initial values: `:= value`.
//!
//! A value is an array initialiser `[1, 2, 3(0)]`, a structure initialiser
//! `(a := 1, b := (c := 2))`, an enumeration value `Color#Red`, or an
//! expression. An expression is accepted wherever the legacy grammar takes a
//! literal or a name, because a constant expression (`PI / 180.0`) is a
//! dialect extension that the analysis, not the syntax, decides; the same
//! value rule serves variables, structure members, `TYPE` declarations,
//! array elements and the members of an instance initialisation.

use super::expressions::{close_group, expression, name_ref};
use crate::parser::state::Parser;
use crate::syntax_kind::SyntaxKind as K;

/// `:= value`, at the `:=`.
pub(super) fn initializer(p: &mut Parser) {
    let node = p.start();
    p.bump();
    value(p);
    p.complete(node, K::Initializer);
}

/// One value. Nesting is bounded by the depth guard.
pub(super) fn value(p: &mut Parser) {
    p.guarded(
        |p| {
            if p.at(K::LeftBracket) {
                array_initializer(p);
            } else if struct_initializer_ahead(p) {
                struct_initializer(p);
            } else if qualified_value_ahead(p) {
                qualified_value(p);
            } else {
                expression(p);
            }
        },
        |p| {
            if !p.at_eof() {
                let node = p.start();
                p.bump();
                p.complete(node, K::ErrorNode);
            }
        },
    );
}

/// `( name :=`: a structure initialiser, as opposed to a parenthesised
/// expression.
fn struct_initializer_ahead(p: &Parser) -> bool {
    p.at(K::LeftParen) && p.variable_name_at(1) && p.nth_at(2, K::Assignment)
}

/// `Type#Value`: the qualified spelling of an enumeration value.
fn qualified_value_ahead(p: &Parser) -> bool {
    p.name_at(0) && p.nth_at(1, K::Hash) && p.adjacent(0)
}

fn qualified_value(p: &mut Parser) {
    let node = p.start();
    name_ref(p);
    p.bump();
    if p.variable_name_at(0) {
        name_ref(p);
    } else {
        p.error("expected the enumeration value");
    }
    p.complete(node, K::EnumValueRef);
}

/// `( name := value {, name := value} )`
fn struct_initializer(p: &mut Parser) {
    let node = p.start();
    p.bump();
    loop {
        struct_element(p);
        if !p.eat(K::Comma) {
            break;
        }
    }
    close_group(p, K::RightParen, "`)`");
    p.complete(node, K::StructInit);
}

fn struct_element(p: &mut Parser) {
    let node = p.start();
    if p.variable_name_at(0) {
        name_ref(p);
    } else {
        p.error("expected a member name");
        p.abandon(node);
        return;
    }
    if p.expect(K::Assignment, "`:=`") {
        value(p);
    }
    p.complete(node, K::StructInitElement);
}

/// `[ element {, element} ]`; an element is a value or `count ( [value] )`.
fn array_initializer(p: &mut Parser) {
    let node = p.start();
    p.bump();
    if !p.at(K::RightBracket) {
        loop {
            array_element(p);
            if !p.eat(K::Comma) {
                break;
            }
        }
    }
    close_group(p, K::RightBracket, "`]`");
    p.complete(node, K::ArrayInit);
}

fn array_element(p: &mut Parser) {
    if p.at(K::IntegerLit) && p.nth_at(1, K::LeftParen) {
        let node = p.start();
        let count = p.start();
        p.bump();
        p.complete(count, K::IntLiteral);
        p.bump();
        if !p.at(K::RightParen) {
            value(p);
        }
        close_group(p, K::RightParen, "`)`");
        p.complete(node, K::RepeatedInit);
    } else {
        value(p);
    }
}
