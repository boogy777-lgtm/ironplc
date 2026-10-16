//! Initial values: `:= value`.
//!
//! What may follow the `:=` depends on the type before it, see
//! [`Initial`](super::positions::Initial): an array takes `[...]`, a string
//! takes a string literal, a reference takes `NULL` or `REF(variable)`, a
//! type name takes an expression in a variable and a constant in a `TYPE`
//! declaration. The general value is an array initialiser `[1, 2, 3(0)]`, a
//! structure initialiser `(a := 1, b := (c := 2))`, an enumeration value
//! `Color#Red`, or an expression: a constant expression (`PI / 180.0`) is a
//! dialect extension that the analysis, not the syntax, decides.

use super::enum_value::qualified_value;
use super::expressions::{close_group, expression, name_ref, ref_expr};
use super::literals::{literal, string_literal_ahead};
use super::positions::Initial;
use crate::expectation::Item;
use crate::parser::state::Parser;
use crate::syntax_kind::SyntaxKind as K;
use ironplc_problems::text;

/// `:= value`, at the `:=`, with the value the type before it takes.
pub(super) fn initializer(p: &mut Parser, rule: Initial) {
    let node = p.start();
    p.bump();
    match rule {
        Initial::Expression => value(p),
        Initial::Plain => {
            expression(p);
        }
        Initial::Constant => constant(p),
        Initial::Literal => {
            if literal(p).is_none() {
                p.expected(Item::Literal);
            }
        }
        Initial::Array => {
            if p.at(K::LeftBracket) {
                array_initializer(p);
            } else {
                p.error(text::syntax_error_an_array_starts_as_brackets());
                value(p);
            }
        }
        Initial::String => {
            if string_literal_ahead(p) {
                literal(p);
            } else {
                p.error(text::syntax_error_a_string_starts_as_a_character_string_literal());
                value(p);
            }
        }
        Initial::Reference => reference(p),
        Initial::Forbidden => {
            p.error(text::syntax_error_an_initial_value_is_not_allowed_for_this_kind_of_type());
            value(p);
        }
    }
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
            } else {
                // A qualified enumeration value is an operand of the expression.
                expression(p);
            }
        },
        degraded,
    );
}

/// The degraded form of a value at the depth limit: one token as an error.
fn degraded(p: &mut Parser) {
    if !p.at_eof() {
        let node = p.start();
        p.bump();
        p.complete(node, K::ErrorNode);
    }
}

/// A constant: a literal, an enumeration value (a name, or `Type#Value`), or
/// a structure value.
fn constant(p: &mut Parser) {
    if struct_initializer_ahead(p) {
        struct_initializer(p);
    } else if qualified_value(p).is_none() && literal(p).is_none() {
        if p.variable_name_at(0) {
            name_ref(p);
        } else {
            p.expected(Item::Constant);
        }
    }
}

/// `NULL` or `REF(variable)`: the only values a reference starts with.
fn reference(p: &mut Parser) {
    if p.at(K::Null) {
        let node = p.start();
        p.bump();
        p.complete(node, K::NullLiteral);
    } else if p.at(K::Ref) && p.nth_at(1, K::LeftParen) {
        ref_expr(p);
    } else {
        p.error(text::syntax_error_a_reference_starts_as_null_or_ref_variable());
    }
}

/// `( name :=`: a structure initialiser, as opposed to a parenthesised
/// expression.
fn struct_initializer_ahead(p: &Parser) -> bool {
    p.at(K::LeftParen) && p.variable_name_at(1) && p.nth_at(2, K::Assignment)
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
    close_group(p, K::RightParen);
    p.complete(node, K::StructInit);
}

fn struct_element(p: &mut Parser) {
    let node = p.start();
    if p.variable_name_at(0) {
        name_ref(p);
    } else {
        p.expected(Item::MemberName);
        p.abandon(node);
        return;
    }
    if p.expect(K::Assignment) {
        value(p);
    }
    p.complete(node, K::StructInitElement);
}

/// `[ element {, element} ]`
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
    close_group(p, K::RightBracket);
    p.complete(node, K::ArrayInit);
}

/// An element is a constant, or `count ( [constant] )` for a run of them.
fn array_element(p: &mut Parser) {
    if p.at(K::IntegerLit) && p.nth_at(1, K::LeftParen) {
        let node = p.start();
        let count = p.start();
        p.bump();
        p.complete(count, K::IntLiteral);
        p.bump();
        if !p.at(K::RightParen) {
            simple_value(p);
        }
        close_group(p, K::RightParen);
        p.complete(node, K::RepeatedInit);
    } else {
        simple_value(p);
    }
}

/// A literal or an enumeration value (a name, or `Type#Value`): an array
/// element, or the source of a program connection.
pub(super) fn simple_value(p: &mut Parser) {
    if qualified_value(p).is_none() && literal(p).is_none() {
        if p.variable_name_at(0) {
            name_ref(p);
        } else {
            p.expected(Item::Constant);
        }
    }
}

/// The source of a program connection: a direct address or a simple value.
pub(super) fn connection_source(p: &mut Parser) {
    if p.at(K::DirectAddress) {
        let node = p.start();
        p.bump();
        p.complete(node, K::DirectAddressExpr);
    } else {
        simple_value(p);
    }
}
