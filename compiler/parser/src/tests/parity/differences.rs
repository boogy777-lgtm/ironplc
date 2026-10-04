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
use super::{Basis, Class, Reason};
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
    pub reason: Reason,
    pub expected: usize,
}

const MARKER_IN_STRING: Reason = Reason::new(Class::LegacyDefect, "the legacy pre-pass takes marker text inside a string literal as a ranged-comment marker and blanks the statement between, so the legacy tokens are not those of the text; the new parser lexes the strings first and keeps the code (a deliberate difference, pinned by its own test)");
const SEVERAL_PAIRS: Reason = Reason::new(Class::OwnerDecided, "the legacy pre-pass blanks the first ranged-comment pair only, so the legacy lexer rejects the file at the second pair's body and produces no tokens after it; the new parser makes every pair a region (a deliberate difference, listed for the file)");

const KEYWORD_NAME_ALONE: Reason = Reason::new(Class::AcceptedOnPurpose, "deliberate behaviour change: a bare `STEP`, `ON`, `R_EDGE` or `F_EDGE` is a late-bound name like every other bare name. The legacy rule for a late-bound name (`identifier`) rejects those tokens, so the grammar fell through to the rule for a variable");
const BIND_SPANS: Reason = Reason::new(Class::LegacyDefect, "legacy bug not ported: an assignment is positioned at its whole operator (`S=`, `R=`, `REF=`) where the legacy grammar keeps the `=` token alone, and the value of `REF=` is positioned at the place it names where the legacy grammar builds it without a position");
const POSITIVE_LABEL: Reason = Reason::new(Class::LegacyDefect, "legacy bug not ported: the legacy grammar positions a `CASE` label written with a `+` at its digits alone and one written with a `-` at the sign and the digits; the lowering positions the number as written");
const LABEL_NAME: Reason = Reason::new(Class::LegacyDefect, "legacy bug not ported: the legacy grammar builds the name of a statement label without its position; the lowering gives it the position of the name like every other name");
const TYPE_NAME_POSITION: Reason = Reason::new(Class::LegacyDefect, "legacy bug not ported: the legacy grammar builds the name of an elementary type in a declaration (`INT` in `x : INT := 5`, the base of an alias, the return type of a function, a method or a property) without a position, because it converts the keyword to a name instead of recording where it was written; the lowering gives it the position of the keyword like every other type name (pinned by its own test)");
const BLOCK_SHAPES: Reason = Reason::new(Class::LegacyDefect, "legacy shapes differ from the one table of initial values that every other declaration is built with: an input-output variable of an elementary type is a late-resolved type in the legacy grammar (the type resolver turns it into the simple type the lowering builds), and a global or external variable of a named type, or a global string, is a simple type there, which the resolver does not resolve: a global function block instance is reported as an undeclared variable (P4012: `VAR_EXTERNAL g : Fb; END_VAR g();`) instead of resolving to the instance, where the lowering gives it the late-resolved type that a local declaration has; the string shape of the lowering is the one a local string has");
const UNNAMED_GLOBAL_LOCATION: Reason = Reason::new(Class::LegacyDefect, "legacy bug not ported: the legacy global-variable rule reads a location without a name (`VAR_GLOBAL AT %MW0 : INT; END_VAR`) and records a variable with the empty name and no location, so the address is lost; the lowering declares the located variable, which has no name and the address (a located variable is allowed to have none, as in a plain block)");
const LONG_STEP_LIST: Reason = Reason::new(Class::LegacyDefect, "legacy bug not ported: the legacy grammar reads a parenthesised list of three or more steps as its first two (a note in the grammar says the rest still need to be added), so a transition that leaves or enters more steps loses them; the lowering keeps every step written (pinned by its own test)");
const LISTED_INPUT: Reason = Reason::new(Class::AcceptedOnPurpose, "deliberate behaviour changes listed with their reasons in the statement, body and declaration tables of the verdict comparison: the legacy parser rejects the input and so builds nothing to compare, and the new parser accepts it on purpose");

pub const DIFFERENCES: &[Difference] = &[
    Difference {
        scope: Scope::Input("tests/fixtures/lexical/oscat_marker_in_string.st"),
        reason: MARKER_IN_STRING,
        expected: 126,
    },
    Difference {
        scope: Scope::Input("tests/fixtures/lexical/oscat_several_pairs.st"),
        reason: SEVERAL_PAIRS,
        expected: 108,
    },
    Difference {
        scope: Scope::Input("VAR_GLOBAL AT %MW0 : INT; END_VAR"),
        reason: UNNAMED_GLOBAL_LOCATION,
        expected: 12,
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
        expected: 173,
    },
    Difference {
        scope: Scope::Origin {
            nodes: &[
                SyntaxKind::VarDecl,
                SyntaxKind::TypeDecl,
                SyntaxKind::VarBlock,
                SyntaxKind::FunctionDecl,
                SyntaxKind::MethodDecl,
                SyntaxKind::PropertyDecl,
            ],
            parts: &[Component::Spans],
        },
        reason: TYPE_NAME_POSITION,
        expected: 8113,
    },
    Difference {
        scope: Scope::Origin {
            nodes: &[SyntaxKind::VarBlock],
            parts: &[Component::Dump],
        },
        reason: BLOCK_SHAPES,
        expected: 18,
    },
    Difference {
        scope: Scope::Origin {
            nodes: &[SyntaxKind::VarBlock],
            parts: &[Component::Dump, Component::Spans],
        },
        reason: BLOCK_SHAPES,
        expected: 81,
    },
    Difference {
        scope: Scope::Input(
            "PROGRAM p INITIAL_STEP s : END_STEP STEP t : END_STEP TRANSITION FROM (s, t, u) TO t := TRUE; END_TRANSITION END_PROGRAM",
        ),
        reason: LONG_STEP_LIST,
        expected: 12,
    },
    Difference {
        scope: Scope::AcceptedOnPurpose,
        reason: LISTED_INPUT,
        expected: 546,
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
    use crate::tests::parity::diagnostics::CODE_EXCEPTIONS;
    use crate::tests::parity::tables::{EXPRESSION_EXCEPTIONS, FILE_EXCEPTIONS};
    use crate::tests::parity::Exception;

    fn verdict_tables() -> [&'static [Exception]; 6] {
        [
            STATEMENT_EXCEPTIONS,
            EXPRESSION_EXCEPTIONS,
            BODY_EXCEPTIONS,
            FILE_EXCEPTIONS,
            LEGACY_DECLARATION_EXCEPTIONS,
            DECLARATION_EXCEPTIONS,
        ]
    }

    #[test]
    fn reasons_when_every_entry_of_every_table_then_each_has_text_and_a_class_that_fits_its_basis()
    {
        let mut by_class = [0usize; 3];
        for entry in verdict_tables().iter().flat_map(|table| table.iter()) {
            assert!(!entry.reason.text.is_empty(), "{:?}", entry.snippet);
            if entry.basis == Basis::FragmentEntry {
                // The legacy fragment entry is the one that is wrong.
                assert_eq!(
                    entry.reason.class,
                    Class::LegacyDefect,
                    "{:?}",
                    entry.snippet
                );
            }
            by_class[entry.reason.class as usize] += 1;
        }
        for entry in DIFFERENCES {
            assert!(!entry.reason.text.is_empty(), "{:?}", entry.scope);
            by_class[entry.reason.class as usize] += 1;
        }
        for entry in CODE_EXCEPTIONS {
            assert!(!entry.reason.text.is_empty(), "{:?}", entry.key);
            by_class[entry.reason.class as usize] += 1;
        }
        for class in Class::ALL {
            println!("{}: {} entries", class.name(), by_class[class as usize]);
        }
    }

    #[test]
    fn reason_when_displayed_then_the_class_and_the_text() {
        let reason = Reason::new(Class::OwnerDecided, "why");
        assert_eq!(reason.to_string(), "[owner-decided change] why");
    }

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
