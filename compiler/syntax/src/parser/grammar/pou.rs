//! Program organisation units: `PROGRAM`, `FUNCTION` and `FUNCTION_BLOCK`,
//! and the layout of what is inside them.
//!
//! A unit is a header, variable blocks, a body, and for a function block its
//! members. [`sections`] reads the part between the header and the closing
//! keyword for every kind of unit, driven by a [`Layout`]: which blocks it may
//! hold, whether it has members, and whether the body may be empty. The body
//! is a statement list, or a sequential function chart when it starts with
//! `INITIAL_STEP`.
//!
//! Recovery keeps the structure. A variable block after the body, or a second
//! body, is parsed and reported where it stands. A keyword that closes some
//! other construct is wrapped in an error node. A unit that is missing its
//! closer ends where the next declaration begins.

use super::common::{
    close, declaration_stops, declared_name, name_ahead, Order, NameClass, Part,
};
use super::expressions::type_ref;
use super::oop::{self, member};
use super::sfc;
use super::statements::{statement_list, ListEnd};
use super::types::return_type;
use super::var_blocks::{block_ahead, var_block, Scope};
use crate::parser::recovery::{BLOCK_END, MEMBER_START};
use crate::parser::state::Parser;
use crate::syntax_kind::SyntaxKind as K;

const VARIABLES: Part = Part {
    rank: 0,
    once: false,
    name: "a variable block",
};
const BODY: Part = Part {
    rank: 1,
    once: true,
    name: "the body",
};
const MEMBERS: Part = Part {
    rank: 2,
    once: false,
    name: "a member",
};

/// Whether a body may be empty.
#[derive(Clone, Copy, PartialEq, Eq)]
pub(super) enum Body {
    Required,
    Optional,
}

/// What a unit holds between its header and its closer.
pub(super) struct Layout {
    pub scope: Scope,
    pub closer: K,
    /// `METHOD` and `PROPERTY` may follow the body.
    pub members: bool,
    pub body: Body,
    /// The body may be a sequential function chart: a program, a function
    /// block or an action, not a function, a method or an accessor.
    pub chart: bool,
}

/// A body: a sequential function chart if it starts with `INITIAL_STEP`,
/// otherwise a statement list.
pub(super) fn body(p: &mut Parser, chart: bool) {
    if chart && p.at(K::InitialStep) {
        sfc::chart(p);
    } else {
        statement_list(p, ListEnd::Block, false);
    }
}

/// The variable blocks, body and members of a unit, up to its closer.
pub(super) fn sections(p: &mut Parser, layout: &Layout) {
    let mut order = Order::default();
    while !declaration_stops(p, layout.closer, &[]) {
        let before = p.position();
        if block_ahead(p) {
            order.enter(p, VARIABLES);
            var_block(p, layout.scope);
        } else if layout.members && p.at_any(MEMBER_START) {
            order.enter(p, MEMBERS);
            member(p);
        } else if p.at_any(BLOCK_END) {
            p.bump_as_error("this keyword does not belong here");
        } else {
            order.enter(p, BODY);
            body(p, layout.chart);
        }
        if p.position() == before {
            p.bump_as_error("unexpected input");
        }
    }
    if layout.body == Body::Required && !order.has(BODY.rank) {
        p.error("expected a statement");
    }
}

/// `PROGRAM name ... END_PROGRAM`
pub(super) fn program(p: &mut Parser) {
    let node = p.start();
    p.bump();
    declared_name(p, NameClass::Plain);
    sections(
        p,
        &Layout {
            scope: Scope::Program,
            closer: K::EndProgram,
            members: false,
            body: Body::Optional,
            chart: true,
        },
    );
    close(p, K::EndProgram, "`END_PROGRAM`");
    p.complete(node, K::ProgramDecl);
}

/// `FUNCTION name : type ... END_FUNCTION`; the body needs a statement.
pub(super) fn function(p: &mut Parser) {
    let node = p.start();
    p.bump();
    // `FUNCTION TIME` names a function where the dialect allows it.
    if name_ahead(p, NameClass::Plain)
        || (p.options.allow_time_as_function_name && p.at(K::Time))
    {
        let name = p.start();
        p.bump();
        p.complete(name, K::Name);
    } else {
        p.error("expected a name");
    }
    if p.expect(K::Colon, "`:`") {
        return_type(p);
    }
    sections(
        p,
        &Layout {
            scope: Scope::Function,
            closer: K::EndFunction,
            members: false,
            body: Body::Required,
            chart: false,
        },
    );
    close(p, K::EndFunction, "`END_FUNCTION`");
    p.complete(node, K::FunctionDecl);
}

/// `FUNCTION_BLOCK [qualifiers] name [VAR_GENERIC ...] [EXTENDS t]
/// [IMPLEMENTS t, ...] ... END_FUNCTION_BLOCK`
pub(super) fn function_block(p: &mut Parser) {
    let node = p.start();
    p.bump();
    oop::member_qualifiers(p);
    declared_name(p, NameClass::Plain);
    while p.at(K::VarGeneric) {
        var_block(p, Scope::Generic);
    }
    if p.at(K::Extends) {
        let clause = p.start();
        p.bump();
        type_ref(p);
        p.complete(clause, K::ExtendsClause);
    }
    if p.at(K::Implements) {
        oop::implements_clause(p);
    }
    sections(
        p,
        &Layout {
            scope: Scope::FunctionBlock,
            closer: K::EndFunctionBlock,
            members: true,
            body: Body::Optional,
            chart: true,
        },
    );
    close(p, K::EndFunctionBlock, "`END_FUNCTION_BLOCK`");
    p.complete(node, K::FunctionBlockDecl);
}
