//! The object-oriented declarations: member qualifiers, `EXTENDS` and
//! `IMPLEMENTS`, `METHOD`, `PROPERTY` with its accessors, and `INTERFACE`.
//!
//! The keywords are gated by `allow_fb_inheritance` in the keyword table, so a
//! dialect without them reads the words as names and never reaches these
//! rules. The qualifier words other than `ABSTRACT` are contextual: ordinary
//! names everywhere but between a declaration's keyword and its name.

use super::common::{close, declaration_stops, declared_name, skip_stray, NameClass, Order, Part};
use super::expressions::type_ref;
use super::pou::{sections, Body, Layout};
use super::types::return_type;
use super::var_blocks::Scope;
use crate::parser::state::Parser;
use crate::syntax_kind::SyntaxKind as K;

/// The qualifier words that are not keywords.
const QUALIFIER_WORDS: &[&str] = &[
    "PUBLIC",
    "PRIVATE",
    "PROTECTED",
    "INTERNAL",
    "FINAL",
    "OVERRIDE",
    "OVERLOAD",
];

fn qualifier_word_at(p: &Parser, n: usize) -> bool {
    p.nth_at(n, K::Abstract) || QUALIFIER_WORDS.iter().any(|word| p.nth_is_word(n, word))
}

/// True when the token at `n` continues a statement that began with a name:
/// `:=`, `(`, `.`, `[`, `^`, or the binding operators `REF=`, `S=` and `R=`.
fn statement_continues_at(p: &Parser, n: usize) -> bool {
    if matches!(
        p.nth(n),
        Some(K::Assignment | K::LeftParen | K::Period | K::LeftBracket | K::Caret)
    ) {
        return true;
    }
    let binds = p.nth_at(n, K::Ref)
        || p.nth_is_word(n, "REF")
        || p.nth_is_word(n, "S")
        || p.nth_is_word(n, "R");
    binds && p.nth_at(n + 1, K::Equal) && p.adjacent(n)
}

/// True when the word at the cursor is a qualifier and not the declared
/// name. A word is a qualifier only when the name still follows it, so
/// `METHOD Override : BOOL` is a method named `Override`; and the name must
/// not start a statement either, so in `METHOD Override x := 1;` the method
/// has no header and `x := 1;` is its body.
fn qualifier_here(p: &Parser) -> bool {
    qualifier_word_at(p, 0)
        && (qualifier_word_at(p, 1) || (p.name_at(1) && !statement_continues_at(p, 2)))
}

/// Qualifiers in source order, each as a node. Their order and combination
/// are for the analysis to check.
pub(super) fn member_qualifiers(p: &mut Parser) {
    while qualifier_here(p) {
        let node = p.start();
        p.bump();
        p.complete(node, K::MemberQualifier);
    }
}

/// `IMPLEMENTS name {, name}`
pub(super) fn implements_clause(p: &mut Parser) {
    let node = p.start();
    p.bump();
    loop {
        type_ref(p);
        if !p.eat(K::Comma) {
            break;
        }
    }
    p.complete(node, K::ImplementsClause);
}

/// A member of a function block, at `METHOD` or `PROPERTY`.
pub(super) fn member(p: &mut Parser) {
    if p.at(K::Method) {
        method(p);
    } else {
        property(p);
    }
}

/// `METHOD [qualifiers] name [: type] ... END_METHOD`. Unlike a function, a
/// method may have an empty body: an `ABSTRACT` method has none.
fn method(p: &mut Parser) {
    let node = p.start();
    p.bump();
    member_qualifiers(p);
    declared_name(p, NameClass::Plain);
    if p.eat(K::Colon) {
        return_type(p);
    }
    sections(
        p,
        &Layout {
            scope: Scope::Method,
            closer: K::EndMethod,
            members: false,
            body: Body::Optional,
            chart: false,
        },
    );
    close(p, K::EndMethod, "`END_METHOD`");
    p.complete(node, K::MethodDecl);
}

const GET: Part = Part {
    rank: 0,
    once: true,
    name: "`GET`",
};
const SET: Part = Part {
    rank: 1,
    once: true,
    name: "`SET`",
};

/// `PROPERTY [qualifiers] name : type [GET ... END_GET] [SET ... END_SET]
/// END_PROPERTY`. `GET` and `SET` are names everywhere else, so they are
/// matched by their text.
fn property(p: &mut Parser) {
    let node = p.start();
    p.bump();
    member_qualifiers(p);
    declared_name(p, NameClass::Plain);
    if p.expect(K::Colon, "`:`") {
        return_type(p);
    }
    let mut order = Order::default();
    while !declaration_stops(p, K::EndProperty, &[]) {
        if p.nth_is_word(0, "GET") {
            order.enter(p, GET);
            accessor(p, K::GetAccessor, K::EndGet);
        } else if p.nth_is_word(0, "SET") {
            order.enter(p, SET);
            accessor(p, K::SetAccessor, K::EndSet);
        } else {
            skip_stray(p, K::EndProperty, &[], "expected `GET` or `SET`");
        }
    }
    close(p, K::EndProperty, "`END_PROPERTY`");
    p.complete(node, K::PropertyDecl);
}

fn accessor(p: &mut Parser, node_kind: K, closer: K) {
    let node = p.start();
    p.bump();
    sections(
        p,
        &Layout {
            scope: Scope::Method,
            closer,
            members: false,
            body: Body::Optional,
            chart: false,
        },
    );
    close(p, closer, "the end of the accessor");
    p.complete(node, node_kind);
}

/// `INTERFACE name [EXTENDS name {, name}] END_INTERFACE`. The legacy grammar
/// reads only the header: an interface holds no member declarations.
pub(super) fn interface(p: &mut Parser) {
    let node = p.start();
    p.bump();
    declared_name(p, NameClass::Plain);
    if p.at(K::Extends) {
        let clause = p.start();
        p.bump();
        loop {
            type_ref(p);
            if !p.eat(K::Comma) {
                break;
            }
        }
        p.complete(clause, K::ExtendsClause);
    }
    if !declaration_stops(p, K::EndInterface, &[]) {
        skip_stray(p, K::EndInterface, &[], "an interface has no body");
    }
    close(p, K::EndInterface, "`END_INTERFACE`");
    p.complete(node, K::InterfaceDecl);
}
