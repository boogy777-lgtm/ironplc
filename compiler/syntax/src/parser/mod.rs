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
pub use state::MAX_DEPTH;
pub use state::{is_special_operator, SPECIAL_OPERATORS};

use self::options::ParseOptions;
use self::state::Parser;
use crate::cst::build_green;
use crate::error::SyntaxError;
use crate::lexer::{lex_regions, Token};
use crate::syntax_kind::{SyntaxKind, SyntaxNode};
use ironplc_dsl::stack::within_stack_budget;

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
    /// The options the text was parsed under. A keyword the dialect leaves
    /// disabled is an ordinary word in the tree, though its token keeps the kind
    /// of the keyword, so whoever reads the tree needs them to tell the two apart.
    pub options: ParseOptions,
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

/// The tokens of `source` under `options`, and the errors found in them, in
/// source order: the lexer, the regions the dialect sets aside and the gates
/// on lexical forms, without the grammar. A parse starts with exactly this.
///
/// The tokens tile the source, trivia and the regions included. The errors are
/// those that depend on the token stream alone: bytes that make no token, a
/// form the dialect has not enabled, a malformed conditional pragma.
///
/// It runs on the caller's stack. Nothing in it recurses on the input: the
/// lexer and the region pass are loops over the bytes, and the one recursive
/// call in the lexer happens at most once for a comment (its retry as a
/// comment that does not nest). A caller that tokenizes on every keystroke
/// therefore pays for no thread, unlike a parse, whose grammar nests as deep as
/// the text does.
pub fn tokenize<'src>(
    source: &'src str,
    options: &ParseOptions,
) -> (Vec<Token<'src>>, Vec<SyntaxError>) {
    let (tokens, mut errors) = lex_regions(source, options);
    errors.extend(gates::gate_errors(&tokens, options));
    errors.sort_by_key(|error| (error.range.start(), error.range.end()));
    (tokens, errors)
}

/// Builds the tree on the stack budget. What leaves the budget thread is the
/// green tree, which can cross threads; the red tree over it, which cannot, is
/// made here.
fn parse_with(
    source: &str,
    options: &ParseOptions,
    entry: impl FnOnce(&mut Parser) + Send,
) -> Parse {
    let (green, errors) = within_stack_budget(|| {
        let (tokens, mut errors) = tokenize(source, options);

        let mut parser = Parser::new(&tokens, *options);
        let root = parser.start();
        entry(&mut parser);
        parser.complete(root, SyntaxKind::SourceFile);
        let (events, parse_errors) = parser.finish();
        errors.extend(parse_errors);
        errors.sort_by_key(|error| (error.range.start(), error.range.end()));
        (build_green(&tokens, events), errors)
    });

    Parse {
        root: SyntaxNode::new_root(green),
        errors,
        options: *options,
    }
}
