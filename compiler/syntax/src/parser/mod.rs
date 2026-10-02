//! The parser: entry points and the machinery behind them.
//!
//! A parse is lexing, dialect gating of lexical forms, a recursive-descent
//! parse that records events, and the sink that builds the rowan tree
//! (design: parse-tree architecture, sections 3.1 and 4). The tree always
//! covers the whole input: `tree.text() == source` for any input, valid or
//! not, and every error range lies inside the source.
//!
//! The grammar covers expressions, statements and every declaration of a file.
//! Nothing here is wired into a production consumer.

pub(crate) mod event;
mod gates;
mod grammar;
pub mod options;
mod recovery;
mod state;
pub(crate) use state::is_special_operator;
pub use state::MAX_DEPTH;

use self::options::ParseOptions;
use self::state::Parser;
use crate::cst::build_green;
use crate::error::SyntaxError;
use crate::lexer::lex_regions;
use crate::syntax_kind::{SyntaxKind, SyntaxNode};

/// The result of a parse: the tree and every syntax error found.
///
/// The root is always a [`SyntaxKind::SourceFile`] whose text is the source.
/// No path from the root down holds more than [`MAX_DEPTH`] nodes.
/// Errors are in source order and include lexical errors, dialect-gate
/// errors and grammar errors.
#[derive(Debug)]
pub struct Parse {
    pub root: SyntaxNode,
    pub errors: Vec<SyntaxError>,
}

impl Parse {
    /// True when no syntax error was found.
    pub fn is_ok(&self) -> bool {
        self.errors.is_empty()
    }
}

/// Parses a statement list, as found in a POU body or an embedded ST body.
/// The root holds one [`SyntaxKind::StatementList`].
pub fn parse_statements(source: &str, options: &ParseOptions) -> Parse {
    parse_with(source, options, |p| {
        grammar::statement_list(p, grammar::ListEnd::TopLevel, false);
    })
}

/// Parses a single expression. The root holds the expression node; input
/// left over after it is reported as an error and kept in an error node.
pub fn parse_expression(source: &str, options: &ParseOptions) -> Parse {
    parse_with(source, options, |p| {
        grammar::expression(p);
        if !p.at_eof() {
            p.skip_rest("unexpected input after the expression");
        }
    })
}

/// Parses a whole file: a sequence of declarations. Input that starts no
/// declaration is wrapped in an error node up to the next one, and a
/// declaration that is malformed or not closed reports its errors and ends
/// where the next declaration begins.
pub fn parse_source_file(source: &str, options: &ParseOptions) -> Parse {
    parse_with(source, options, grammar::source_file)
}

fn parse_with(source: &str, options: &ParseOptions, entry: impl FnOnce(&mut Parser)) -> Parse {
    let (tokens, mut errors) = lex_regions(source, options);
    errors.extend(gates::gate_errors(&tokens, options));

    let mut parser = Parser::new(&tokens, *options);
    let root = parser.start();
    entry(&mut parser);
    parser.complete(root, SyntaxKind::SourceFile);
    let (events, parse_errors) = parser.finish();
    errors.extend(parse_errors);
    errors.sort_by_key(|error| (error.range.start(), error.range.end()));

    Parse {
        root: SyntaxNode::new_root(build_green(&tokens, events)),
        errors,
    }
}
