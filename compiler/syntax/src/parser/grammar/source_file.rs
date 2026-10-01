//! The top level of a file.
//!
//! Declarations (POUs, types, configurations) are not parsed yet. Each
//! top-level declaration is kept as one [`SyntaxKind::UnparsedDeclaration`]
//! node holding its tokens, from the opening keyword through its matching
//! `END_*` keyword, so the tree stays lossless and the declarations can be
//! given structure without changing what surrounds them. The contents of such
//! a node are not validated.

use crate::parser::state::Parser;
use crate::syntax_kind::SyntaxKind as K;

/// Top-level declaration openers and the keyword that closes each.
const DECLARATIONS: &[(K, K)] = &[
    (K::Program, K::EndProgram),
    (K::Function, K::EndFunction),
    (K::FunctionBlock, K::EndFunctionBlock),
    (K::Type, K::EndType),
    (K::Configuration, K::EndConfiguration),
    (K::Interface, K::EndInterface),
    (K::Namespace, K::EndNamespace),
    (K::VarGlobal, K::EndVar),
];

fn declaration_closer(p: &Parser) -> Option<(K, K)> {
    DECLARATIONS
        .iter()
        .find(|(opener, _)| p.at(*opener))
        .copied()
}

pub(in crate::parser) fn source_file(p: &mut Parser) {
    while !p.at_eof() {
        match declaration_closer(p) {
            Some((opener, closer)) => unparsed_declaration(p, opener, closer),
            None => skip_to_declaration(p),
        }
    }
}

/// Wraps tokens that start no declaration, up to the next declaration.
fn skip_to_declaration(p: &mut Parser) {
    p.error("expected a declaration");
    let node = p.start();
    p.bump();
    while !p.at_eof() && declaration_closer(p).is_none() {
        p.bump();
    }
    p.complete(node, K::ErrorNode);
}

fn unparsed_declaration(p: &mut Parser, opener: K, closer: K) {
    let node = p.start();
    let mut depth = 0usize;
    while !p.at_eof() {
        if p.at(opener) {
            depth += 1;
        } else if p.at(closer) {
            depth = depth.saturating_sub(1);
        }
        p.bump();
        if depth == 0 {
            p.complete(node, K::UnparsedDeclaration);
            return;
        }
    }
    p.error("the declaration is not closed");
    p.complete(node, K::UnparsedDeclaration);
}
