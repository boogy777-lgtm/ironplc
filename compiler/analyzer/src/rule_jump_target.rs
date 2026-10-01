//! Semantic rule that a `JMP` names a label defined in the same POU body,
//! and that a label name is defined at most once there.
//!
//! A statement label is a jump target within its POU body; a `JMP` to a name
//! no label defines has nowhere to go (the reference reports `ErrNoSuchLabel`
//! for it), and two labels with the same name make the target ambiguous (the
//! reference reports `Err_DuplicateLabelDefinition`).
//!
//! ## Passes
//!
//! ```ignore
//! PROGRAM main
//! VAR x : INT; END_VAR
//!     x := 1;
//!     JMP done;
//!     x := 2;
//! done:
//!     x := 3;
//! END_PROGRAM
//! ```
//!
//! ## Fails
//!
//! ```ignore
//! PROGRAM main
//!     JMP missing;   (* P0027: no such label *)
//! END_PROGRAM
//! ```
//!
//! ```ignore
//! PROGRAM main
//!     JMP twice;
//! twice:
//!     ;
//! twice:             (* P0042: the name is a duplicate *)
//!     ;
//! END_PROGRAM
//! ```
//!
//! Code generation needs the same labels to emit the jump, and checks the
//! target defensively; this rule is what `check`, which stops after semantic
//! analysis, reports.

use ironplc_dsl::{
    common::*,
    core::{Id, Located, SourceSpan},
    diagnostic::{Diagnostic, Label},
    textual::*,
    visitor::Visitor,
};
use ironplc_problems::Problem;
use std::collections::HashMap;
use std::convert::Infallible;

use crate::{
    result::SemanticResult,
    rule_support::{run_rule, DiagnosticVisitor},
    semantic_context::SemanticContext,
};
use ironplc_parser::options::CompilerOptions;

pub fn apply(
    lib: &Library,
    _context: &SemanticContext,
    _options: &CompilerOptions,
) -> SemanticResult {
    run_rule(RuleJumpTarget::default(), lib)
}

#[derive(Default)]
struct RuleJumpTarget {
    diagnostics: Vec<Diagnostic>,
}

impl RuleJumpTarget {
    /// Checks one POU body: labels are collected first, so a jump may target
    /// a label that appears later in the body.
    fn check_body(&mut self, body: &[StmtKind]) {
        let mut collector = BodyLabels::default();
        for stmt in body {
            // The collector has no failure mode; recursing into every
            // statement reaches the nested bodies too.
            let _ = stmt.recurse_visit(&mut collector);
        }

        let mut seen: HashMap<String, ()> = HashMap::new();
        for (name, span) in &collector.labels {
            if seen.insert(name.lower_case().clone(), ()).is_some() {
                self.diagnostics.push(Diagnostic::problem(
                    Problem::DuplicateLabelDefinition,
                    Label::span(
                        span.clone(),
                        format!("Label '{}' is defined more than once", name.original()),
                    ),
                ));
            }
        }

        for (name, span) in &collector.jumps {
            if !seen.contains_key(name.lower_case()) {
                self.diagnostics.push(Diagnostic::problem(
                    Problem::JumpDestinationInvalid,
                    Label::span(
                        span.clone(),
                        format!("No label '{}' in this body", name.original()),
                    ),
                ));
            }
        }
    }
}

impl Visitor<Infallible> for RuleJumpTarget {
    type Value = ();

    fn visit_statements(&mut self, node: &Statements) -> Result<(), Infallible> {
        self.check_body(&node.body);
        node.recurse_visit(self)
    }
}

impl DiagnosticVisitor for RuleJumpTarget {
    fn into_diagnostics(self) -> Vec<Diagnostic> {
        self.diagnostics
    }
}

/// Collects the labels and jumps of one body, at any nesting depth.
#[derive(Default)]
struct BodyLabels {
    labels: Vec<(Id, SourceSpan)>,
    jumps: Vec<(Id, SourceSpan)>,
}

impl Visitor<Infallible> for BodyLabels {
    type Value = ();

    fn visit_label_statement(&mut self, node: &LabelStatement) -> Result<(), Infallible> {
        self.labels.push((node.name.clone(), node.span()));
        Ok(())
    }

    fn visit_jump(&mut self, node: &Jump) -> Result<(), Infallible> {
        self.jumps.push((node.label.clone(), node.span()));
        node.recurse_visit(self)
    }
}

#[cfg(test)]
mod tests {
    use super::apply;
    use crate::test_helpers::parse_and_resolve_types_with_options;
    use ironplc_parser::options::CompilerOptions;
    use ironplc_problems::Problem;
    use spec_test_macro::spec_test;

    fn options() -> CompilerOptions {
        CompilerOptions {
            allow_jump_statement: true,
            ..CompilerOptions::default()
        }
    }

    fn codes_for(program: &str) -> Vec<String> {
        let (library, context) = parse_and_resolve_types_with_options(program, &options());
        apply(&library, &context, &options())
            .err()
            .unwrap_or_default()
            .into_iter()
            .map(|diagnostic| diagnostic.code)
            .collect()
    }

    fn p0027() -> String {
        Problem::JumpDestinationInvalid.code().to_string()
    }

    fn p0042() -> String {
        Problem::DuplicateLabelDefinition.code().to_string()
    }

    /// REQ-JMP-analyzer-001: a jump whose target names no label reports P0027.
    #[spec_test(REQ_JMP_analyzer_001)]
    #[test]
    fn apply_when_jump_target_missing_then_p0027() {
        let program = "PROGRAM main
VAR x : INT; END_VAR
x := 1;
JMP nowhere;
END_PROGRAM";

        assert_eq!(codes_for(program), vec![p0027()]);
    }

    /// REQ-JMP-analyzer-001: a jump may target a label that appears later in
    /// the body, and one label may be the target of several jumps.
    #[spec_test(REQ_JMP_analyzer_001)]
    #[test]
    fn apply_when_jump_target_defined_later_then_ok() {
        let program = "PROGRAM main
VAR x : INT; END_VAR
JMP done;
x := 1;
JMP done;
done:
x := 2;
END_PROGRAM";

        assert!(codes_for(program).is_empty());
    }

    /// REQ-JMP-analyzer-001: a label of another POU is not in scope.
    #[spec_test(REQ_JMP_analyzer_001)]
    #[test]
    fn apply_when_target_is_in_another_pou_then_p0027() {
        let program = "PROGRAM first
JMP shared;
shared:
;
END_PROGRAM

PROGRAM second
JMP shared;
END_PROGRAM";

        // Only the second program reports: the first defines the label.
        assert_eq!(codes_for(program), vec![p0027()]);
    }

    /// REQ-JMP-analyzer-002: two labels with the same name in one body report
    /// P0042.
    #[spec_test(REQ_JMP_analyzer_002)]
    #[test]
    fn apply_when_label_duplicated_then_p0042() {
        let program = "PROGRAM main
VAR x : INT; END_VAR
JMP twice;
twice:
x := 1;
TWICE:
x := 2;
END_PROGRAM";

        assert_eq!(codes_for(program), vec![p0042()]);
    }

    /// REQ-JMP-analyzer-002: the same label name in two bodies is not a
    /// duplicate -- labels are POU-body scoped.
    #[spec_test(REQ_JMP_analyzer_002)]
    #[test]
    fn apply_when_same_label_in_two_bodies_then_ok() {
        let program = "PROGRAM first
JMP done;
done:
;
END_PROGRAM

PROGRAM second
JMP done;
done:
;
END_PROGRAM";

        assert!(codes_for(program).is_empty());
    }
}
