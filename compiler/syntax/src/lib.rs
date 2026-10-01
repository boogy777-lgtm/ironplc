//! Lossless concrete syntax tree for IEC 61131-3 text.
//!
//! This crate owns the syntax kinds and the rowan language binding, the
//! byte-exact lexer over the original source text, the event-driven parser
//! with its tree sink, and the syntax error type. The tree stores every byte
//! of the input, including trivia and unrecognised bytes (design: parse-tree
//! architecture, section 3.1).
//!
//! The parser covers expressions and statements; declarations are the next
//! slice. It is not used by any production consumer yet.

mod cst;
pub mod error;
pub mod lexer;
pub mod parser;
pub mod syntax_kind;

pub use error::SyntaxError;
pub use parser::options::ParseOptions;
pub use parser::{parse_expression, parse_source_file, parse_statements, Parse};
pub use syntax_kind::{StLanguage, SyntaxElement, SyntaxKind, SyntaxNode, SyntaxToken};
