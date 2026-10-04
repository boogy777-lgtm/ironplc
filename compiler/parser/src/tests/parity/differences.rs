//! The documented differences between the lowering and the legacy parser: where
//! each is found, why it exists and how often the corpus shows it.
//!
//! A difference must be listed here with its reason. An unlisted difference
//! fails; so does a listed one whose count is not the number observed, which
//! is how a stale entry (fewer) and a growing one (more) are both caught.

use super::ast::Component;
use super::tables::{
    BODY_EXCEPTIONS, DECLARATION_EXCEPTIONS, LEGACY_DECLARATION_EXCEPTIONS, STATEMENT_EXCEPTIONS,
};
use super::Basis;
use ironplc_syntax::SyntaxKind;

/// Where a difference is found.
#[derive(Clone, Copy, PartialEq, Eq, Debug)]
pub enum Scope {
    /// Every site of the named input (a file, with or without its CRLF and
    /// tab spellings, or a snippet).
    Input(&'static str),
    /// An input that the new parser accepts on purpose and the legacy parser
    /// rejects, listed as such in the verdict tables, where the legacy parser
    /// built nothing to compare.
    AcceptedOnPurpose,
    /// A site whose node is of one of the kinds `nodes`, that differs in
    /// exactly `parts` of the comparison, and in which no site inside it
    /// differs.
    Origin {
        nodes: &'static [SyntaxKind],
        parts: &'static [Component],
    },
}

/// A documented difference: where it is, why, and how often the corpus shows
/// it.
pub struct Difference {
    pub scope: Scope,
    pub reason: &'static str,
    pub expected: usize,
}

const MARKER_IN_STRING: &str = "the legacy pre-pass takes marker text inside a string literal as a ranged-comment marker and blanks the statement between, so the legacy tokens are not those of the text; the new parser lexes the strings first and keeps the code (a deliberate difference, pinned by its own test)";
const SEVERAL_PAIRS: &str = "the legacy pre-pass blanks the first ranged-comment pair only, so the legacy lexer rejects the file at the second pair's body and produces no tokens after it; the new parser makes every pair a region (a deliberate difference, listed for the file)";

const KEYWORD_NAME_ALONE: &str = "deliberate behaviour change: a bare `STEP`, `ON`, `R_EDGE` or `F_EDGE` is a late-bound name like every other bare name. The legacy rule for a late-bound name (`identifier`) rejects those tokens, so the grammar fell through to the rule for a variable";
const BIND_SPANS: &str = "deliberate behaviour change: an assignment is positioned at its whole operator (`S=`, `R=`, `REF=`) where the legacy grammar keeps the `=` token alone, and the value of `REF=` is positioned at the place it names where the legacy grammar builds it without a position";
const POSITIVE_LABEL: &str = "legacy bug not ported: the legacy grammar positions a `CASE` label written with a `+` at its digits alone and one written with a `-` at the sign and the digits; the lowering positions the number as written";
const LABEL_NAME: &str = "legacy bug not ported: the legacy grammar builds the name of a statement label without its position; the lowering gives it the position of the name like every other name";
const TYPE_NAME_POSITION: &str = "legacy bug not ported: the legacy grammar builds the name of an elementary type in a declaration (`INT` in `x : INT := 5`, and the base of an alias) without a position, because it converts the keyword to a name instead of recording where it was written; the lowering gives it the position of the keyword like every other type name (pinned by its own test)";
const INLINE_ENUMERATION_DEFAULT: &str = "legacy bug not ported: the legacy grammar drops the default value of a structure member whose type is an inline enumeration (`a : (X, Y) := X`; a TODO in the rule) and the lowering keeps it, as it does for a variable of that type";
const LISTED_INPUT: &str = "deliberate behaviour changes listed with their reasons in the statement, body and declaration tables of the verdict comparison: the legacy parser rejects the input and so builds nothing to compare, and the new parser accepts it on purpose";

pub const DIFFERENCES: &[Difference] = &[
    Difference {
        scope: Scope::Input("tests/fixtures/lexical/oscat_marker_in_string.st"),
        reason: MARKER_IN_STRING,
        expected: 126,
    },
    Difference {
        scope: Scope::Input("tests/fixtures/lexical/oscat_several_pairs.st"),
        reason: SEVERAL_PAIRS,
        expected: 72,
    },
    Difference {
        scope: Scope::Origin {
            nodes: &[SyntaxKind::NameRef],
            parts: &[Component::Dump],
        },
        reason: KEYWORD_NAME_ALONE,
        expected: 30,
    },
    Difference {
        scope: Scope::Origin {
            nodes: &[SyntaxKind::AssignStmt],
            parts: &[Component::Spans],
        },
        reason: BIND_SPANS,
        expected: 144,
    },
    Difference {
        scope: Scope::Origin {
            nodes: &[SyntaxKind::CaseStmt],
            parts: &[Component::Spans],
        },
        reason: POSITIVE_LABEL,
        expected: 6,
    },
    Difference {
        scope: Scope::Origin {
            nodes: &[SyntaxKind::LabelStmt],
            parts: &[Component::Spans],
        },
        reason: LABEL_NAME,
        expected: 164,
    },
    Difference {
        scope: Scope::Origin {
            nodes: &[SyntaxKind::VarDecl, SyntaxKind::TypeDecl],
            parts: &[Component::Spans],
        },
        reason: TYPE_NAME_POSITION,
        expected: 6498,
    },
    Difference {
        scope: Scope::Origin {
            nodes: &[SyntaxKind::TypeDecl],
            parts: &[Component::Dump, Component::Spans],
        },
        reason: INLINE_ENUMERATION_DEFAULT,
        expected: 12,
    },
    Difference {
        scope: Scope::AcceptedOnPurpose,
        reason: LISTED_INPUT,
        expected: 100,
    },
];

impl Scope {
    pub fn covers(&self, key: &str, node: SyntaxKind, parts: &[Component]) -> bool {
        match *self {
            Scope::Input(name) => {
                key == name
                    || key
                        .strip_prefix(name)
                        .is_some_and(|rest| rest.starts_with(" ("))
            }
            Scope::AcceptedOnPurpose => {
                parts.is_empty()
                    && [
                        STATEMENT_EXCEPTIONS,
                        BODY_EXCEPTIONS,
                        DECLARATION_EXCEPTIONS,
                        LEGACY_DECLARATION_EXCEPTIONS,
                    ]
                    .iter()
                    .flat_map(|table| table.iter())
                    .any(|entry| {
                        entry.snippet == key && !entry.legacy && entry.basis == Basis::Deliberate
                    })
            }
            Scope::Origin {
                nodes: wanted_nodes,
                parts: wanted_parts,
            } => wanted_nodes.contains(&node) && wanted_parts == parts,
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn covers_when_input_scope_then_the_file_and_its_spellings_only() {
        let scope = Scope::Input("a/b.st");
        let any = SyntaxKind::Unknown;
        assert!(scope.covers("a/b.st", any, &[]));
        assert!(scope.covers("a/b.st (CRLF)", any, &[]));
        assert!(scope.covers("a/b.st (tabs)", any, &[]));
        assert!(!scope.covers("a/b.st.bak", any, &[]));
        assert!(!scope.covers("a/bb.st", any, &[]));
    }

    #[test]
    fn covers_when_origin_scope_then_any_of_its_nodes_and_exactly_its_parts() {
        let scope = Scope::Origin {
            nodes: &[SyntaxKind::IndexExpr, SyntaxKind::FieldExpr],
            parts: &[Component::Dump, Component::Spans],
        };
        let both = [Component::Dump, Component::Spans];
        assert!(scope.covers("x", SyntaxKind::IndexExpr, &both));
        assert!(!scope.covers("x", SyntaxKind::IndexExpr, &[Component::Dump]));
        assert!(scope.covers("x", SyntaxKind::FieldExpr, &both));
        assert!(!scope.covers("x", SyntaxKind::CallExpr, &both));
    }
}
