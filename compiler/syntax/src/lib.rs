//! Lossless concrete syntax tree for IEC 61131-3 text.
//!
//! This crate owns the syntax kinds and the rowan language binding, the
//! byte-exact lexer over the original source text, the event-driven parser
//! with its tree sink, and the syntax error type with its rendering as
//! compiler diagnostics. The tree stores every byte
//! of the input, including trivia and unrecognised bytes (design: parse-tree
//! architecture, section 3.1).
//!
//! The parser covers whole files: programs, functions, function blocks with
//! their methods and properties, interfaces, namespaces, variable blocks,
//! `TYPE` declarations, configurations and sequential function charts, with
//! the statements and expressions inside them. It is not used by any
//! production consumer yet.
//!
//! The text is kept as written. Two preprocessing steps of the legacy pipeline
//! change what the text means and are not evaluated here: OSCAT ranged
//! comments (the legacy pipeline blanks what a `(*@KEY@:NAME*)` pair holds)
//! and conditional pragmas (`{IF ...}` and `{END_IF}` drop the branch that is
//! not taken). A pragma is trivia in the tree and its content is not examined.

mod cst;
mod diagnostic;
pub mod error;
pub mod lexer;
pub mod parser;
pub mod pragma;
pub mod syntax_kind;

pub use error::{ErrorKind, SyntaxError};
pub use parser::options::ParseOptions;
pub use parser::{parse_expression, parse_source_file, parse_statements, Parse};
pub use syntax_kind::{StLanguage, SyntaxElement, SyntaxKind, SyntaxNode, SyntaxToken};
