//! The top level of a file: a sequence of declarations.
//!
//! A file holds programs, functions, function blocks, `TYPE` blocks,
//! configurations, interfaces, namespaces and global variable blocks. A
//! namespace holds the same declarations again, so the rule that reads a
//! sequence of them serves both. Input that starts no declaration is wrapped
//! in an error node up to the next one, so a malformed declaration costs the
//! rest of that declaration and no more.

use super::common::{close, declared_name, NameClass};
use super::configuration::configuration;
use super::oop::interface;
use super::pou::{function, function_block, program};
use super::types::type_block;
use super::var_blocks::{var_block, Scope};
use crate::parser::recovery::DECLARATION_START;
use crate::parser::state::Parser;
use crate::syntax_kind::SyntaxKind as K;

pub(in crate::parser) fn source_file(p: &mut Parser) {
    declarations(p, None);
}

/// Declarations up to the end of the input, or up to `until` (not consumed).
fn declarations(p: &mut Parser, until: Option<K>) {
    while !p.at_eof() && !until.is_some_and(|closer| p.at(closer)) {
        let before = p.position();
        if p.at_any(DECLARATION_START) {
            declaration(p);
        } else {
            skip_to_declaration(p, until);
        }
        if p.position() == before {
            p.bump_as_error("unexpected input");
        }
    }
}

fn declaration(p: &mut Parser) {
    match p.nth(0) {
        Some(K::Program) => program(p),
        Some(K::Function) => function(p),
        Some(K::FunctionBlock) => function_block(p),
        Some(K::Type) => type_block(p),
        Some(K::Configuration) => configuration(p),
        Some(K::Interface) => interface(p),
        Some(K::Namespace) => namespace(p),
        Some(K::VarGlobal) => var_block(p, Scope::Global),
        _ => {}
    }
}

/// `NAMESPACE name {declaration} END_NAMESPACE`
fn namespace(p: &mut Parser) {
    p.guarded(
        |p| {
            let node = p.start();
            p.bump();
            declared_name(p, NameClass::Plain);
            declarations(p, Some(K::EndNamespace));
            close(p, K::EndNamespace, "`END_NAMESPACE`");
            p.complete(node, K::NamespaceDecl);
        },
        |p| {
            let node = p.start();
            p.bump();
            p.complete(node, K::ErrorNode);
        },
    );
}

/// Wraps tokens that start no declaration, up to the next declaration or the
/// closer of the enclosing namespace.
fn skip_to_declaration(p: &mut Parser, until: Option<K>) {
    p.error("expected a declaration");
    let node = p.start();
    p.bump();
    while !p.at_eof() && !p.at_any(DECLARATION_START) && !until.is_some_and(|closer| p.at(closer)) {
        p.bump();
    }
    p.complete(node, K::ErrorNode);
}
