// Allow large errors because this is a compiler - we expect large errors.
#![allow(clippy::result_large_err)]
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
//! the statements and expressions inside them. The [`lower`] module turns the
//! tree into the `ironplc_dsl` objects, one rule per language element; it
//! covers the literals, names, expressions and variables so far, and neither it
//! nor the parser is used by a production consumer yet.
//!
//! The text is kept as written. Two constructs make a stretch of it not code:
//! OSCAT ranged comments (what a `(*@KEY@:NAME*)` pair holds) and, with
//! `allow_pragma_if`, the branches of `{IF ...}` that are not taken. Each is
//! one trivia token in the tree (`RangedComment`, `InactiveRegion`) that the
//! grammar skips, so no byte is dropped. Any other pragma is trivia and its
//! content is not examined.

mod cst;
mod diagnostic;
pub mod error;
mod interval_text;
pub mod lexer;
pub mod lower;
pub mod parser;
pub mod pragma;
pub mod syntax_kind;

pub use error::{ErrorKind, SyntaxError};
pub use parser::options::ParseOptions;
pub use parser::{parse_expression, parse_source_file, parse_statements, Parse};
pub use syntax_kind::{StLanguage, SyntaxElement, SyntaxKind, SyntaxNode, SyntaxToken};
