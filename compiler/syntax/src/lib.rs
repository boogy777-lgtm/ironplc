//! Lossless concrete syntax tree for IEC 61131-3 text.
//!
//! This crate owns the syntax kinds and the rowan language binding, the
//! byte-exact lexer over the original source text, a minimal tree builder, and
//! the syntax error type. The tree stores every byte of the input, including
//! trivia and unrecognised bytes (design: parse-tree architecture, section
//! 3.1).

pub mod cst;
pub mod error;
pub mod lexer;
pub mod syntax_kind;

pub use error::SyntaxError;
pub use syntax_kind::{StLanguage, SyntaxElement, SyntaxKind, SyntaxNode, SyntaxToken};
