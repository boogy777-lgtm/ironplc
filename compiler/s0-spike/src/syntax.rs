//! Syntax kinds and the rowan [`Language`] binding for the spike CST.
//!
//! The kind set is deliberately small (one POU form, a few statements, Pratt
//! expressions). It exists to prove byte-exact storage plus red navigation,
//! not to describe the full ST grammar.

use rowan::Language;

/// Every kind a spike green tree can contain: trivia tokens, significant
/// tokens, and nodes. The first five kinds are trivia retained verbatim
/// (design section 3.1).
#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash)]
#[repr(u16)]
pub enum SyntaxKind {
    // Trivia: whitespace, line structure, comments, and pragmas are stored,
    // not skipped (design section 3.1; pragmas per section 3.3).
    Whitespace = 0,
    Newline,
    LineComment,
    BlockComment,
    Pragma,
    // Significant tokens.
    Ident,
    IntegerLit,
    RealLit,
    StringLit,
    Operator,
    /// Bytes no token rule matched; stored so the tree still tiles the source.
    ErrorToken,
    // Nodes.
    SourceFile,
    ProgramDecl,
    VarBlock,
    VarDecl,
    DirectVar,
    TypeRef,
    StatementList,
    AssignStmt,
    IfStmt,
    CallStmt,
    EmptyStmt,
    ArgList,
    NameRef,
    LiteralExpr,
    UnaryExpr,
    BinaryExpr,
    ParenExpr,
    CallExpr,
    /// Recovery node: tokens consumed while resynchronizing after an error.
    ErrorNode,
    /// Fallback for raw kinds this language never produces.
    Unknown,
}

impl SyntaxKind {
    /// All kinds in discriminant order; used by [`SyntaxKind::from_u16`] and
    /// checked by a unit test below.
    pub const ALL: &'static [SyntaxKind] = &[
        SyntaxKind::Whitespace,
        SyntaxKind::Newline,
        SyntaxKind::LineComment,
        SyntaxKind::BlockComment,
        SyntaxKind::Pragma,
        SyntaxKind::Ident,
        SyntaxKind::IntegerLit,
        SyntaxKind::RealLit,
        SyntaxKind::StringLit,
        SyntaxKind::Operator,
        SyntaxKind::ErrorToken,
        SyntaxKind::SourceFile,
        SyntaxKind::ProgramDecl,
        SyntaxKind::VarBlock,
        SyntaxKind::VarDecl,
        SyntaxKind::DirectVar,
        SyntaxKind::TypeRef,
        SyntaxKind::StatementList,
        SyntaxKind::AssignStmt,
        SyntaxKind::IfStmt,
        SyntaxKind::CallStmt,
        SyntaxKind::EmptyStmt,
        SyntaxKind::ArgList,
        SyntaxKind::NameRef,
        SyntaxKind::LiteralExpr,
        SyntaxKind::UnaryExpr,
        SyntaxKind::BinaryExpr,
        SyntaxKind::ParenExpr,
        SyntaxKind::CallExpr,
        SyntaxKind::ErrorNode,
        SyntaxKind::Unknown,
    ];

    fn from_u16(value: u16) -> SyntaxKind {
        SyntaxKind::ALL
            .get(usize::from(value))
            .copied()
            .unwrap_or(SyntaxKind::Unknown)
    }

    /// True for tokens that must be retained but never drive parsing.
    pub fn is_trivia(self) -> bool {
        matches!(
            self,
            SyntaxKind::Whitespace
                | SyntaxKind::Newline
                | SyntaxKind::LineComment
                | SyntaxKind::BlockComment
                | SyntaxKind::Pragma
        )
    }

    /// True for leaf tokens (as opposed to interior nodes).
    pub fn is_token(self) -> bool {
        !matches!(
            self,
            SyntaxKind::SourceFile
                | SyntaxKind::ProgramDecl
                | SyntaxKind::VarBlock
                | SyntaxKind::VarDecl
                | SyntaxKind::DirectVar
                | SyntaxKind::TypeRef
                | SyntaxKind::StatementList
                | SyntaxKind::AssignStmt
                | SyntaxKind::IfStmt
                | SyntaxKind::CallStmt
                | SyntaxKind::EmptyStmt
                | SyntaxKind::ArgList
                | SyntaxKind::NameRef
                | SyntaxKind::LiteralExpr
                | SyntaxKind::UnaryExpr
                | SyntaxKind::BinaryExpr
                | SyntaxKind::ParenExpr
                | SyntaxKind::CallExpr
                | SyntaxKind::ErrorNode
                | SyntaxKind::Unknown
        )
    }
}

/// The rowan language marker for the spike.
#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash)]
pub enum StLanguage {}

impl Language for StLanguage {
    type Kind = SyntaxKind;

    fn kind_from_raw(raw: rowan::SyntaxKind) -> SyntaxKind {
        SyntaxKind::from_u16(raw.0)
    }

    fn kind_to_raw(kind: SyntaxKind) -> rowan::SyntaxKind {
        rowan::SyntaxKind(kind as u16)
    }
}

/// Red (navigable) node over the green storage.
pub type SyntaxNode = rowan::SyntaxNode<StLanguage>;
/// Red token over the green storage.
pub type SyntaxToken = rowan::SyntaxToken<StLanguage>;
/// Node-or-token element.
pub type SyntaxElement = rowan::SyntaxElement<StLanguage>;

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn all_kinds_when_indexed_then_match_discriminants() {
        for (index, kind) in SyntaxKind::ALL.iter().enumerate() {
            assert_eq!(*kind as usize, index, "ALL out of order at {index}");
        }
    }

    #[test]
    fn kind_roundtrip_when_converted_then_identity() {
        for kind in SyntaxKind::ALL {
            let raw = StLanguage::kind_to_raw(*kind);
            assert_eq!(StLanguage::kind_from_raw(raw), *kind);
        }
    }

    #[test]
    fn unknown_kind_when_out_of_range_then_fallback() {
        assert_eq!(
            StLanguage::kind_from_raw(rowan::SyntaxKind(9999)),
            SyntaxKind::Unknown
        );
    }
}
