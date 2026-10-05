//! Semantic rule that gates arrays of function block instances
//! (`timers : ARRAY[0..3] OF TON;`) behind `--allow-fb-instance-arrays`.
//!
//! The IEC 61131-3 Edition 2 grammar declares a function block instance by
//! name (`fb_name_decl ::= fb_name_list ':' function_block_type_name`), so an
//! array of instances, and with it a call of one of its elements
//! (`timers[i](...)`), is an extension. Both parsers accept the form in every
//! dialect; this rule is what enforces the flag. A call is not checked on its
//! own: the element it calls can only come from an array this rule has already
//! reported.
//!
//! ## Passes
//!
//! ```ignore
//! PROGRAM main
//! VAR
//!     timer : TON;
//!     values : ARRAY[0..3] OF INT;
//! END_VAR
//! END_PROGRAM
//! ```
//!
//! ## Fails (without the flag)
//!
//! ```ignore
//! PROGRAM main
//! VAR
//!     timers : ARRAY[0..3] OF TON;
//! END_VAR
//! END_PROGRAM
//! ```
use std::convert::Infallible;

use ironplc_dsl::{
    common::*,
    core::Located,
    diagnostic::{Diagnostic, Label},
    visitor::Visitor,
};
use ironplc_parser::options::CompilerOptions;
use ironplc_problems::Problem;

use crate::{
    callee_resolution::FunctionBlocks,
    intermediates::stdlib_function_block::is_stdlib_function_block,
    result::SemanticResult,
    rule_support::{run_rule, DiagnosticVisitor},
    semantic_context::SemanticContext,
};

pub fn apply(
    lib: &Library,
    _context: &SemanticContext,
    options: &CompilerOptions,
) -> SemanticResult {
    if options.allow_fb_instance_arrays {
        return Ok(());
    }
    let function_blocks = FunctionBlocks::from_library(lib);
    run_rule(
        RuleFbInstanceArray {
            function_blocks: &function_blocks,
            diagnostics: Vec::new(),
        },
        lib,
    )
}

struct RuleFbInstanceArray<'a> {
    function_blocks: &'a FunctionBlocks<'a>,
    diagnostics: Vec<Diagnostic>,
}

impl RuleFbInstanceArray<'_> {
    /// Whether `type_name` names a function block, declared or standard.
    fn is_function_block(&self, type_name: &TypeName) -> bool {
        self.function_blocks.contains(type_name) || is_stdlib_function_block(&type_name.name)
    }
}

impl DiagnosticVisitor for RuleFbInstanceArray<'_> {
    fn into_diagnostics(self) -> Vec<Diagnostic> {
        self.diagnostics
    }
}

impl Visitor<Infallible> for RuleFbInstanceArray<'_> {
    type Value = ();

    fn visit_var_decl(&mut self, node: &VarDecl) -> Result<Self::Value, Infallible> {
        if let InitialValueAssignmentKind::Array(array) = &node.initializer {
            if let SpecificationKind::Inline(subranges) = &array.spec {
                if let (ArrayElementType::Named(element), None) =
                    (&subranges.type_name, &subranges.ref_to)
                {
                    if self.is_function_block(element) {
                        self.diagnostics.push(
                            Diagnostic::problem(
                                Problem::FbInstanceArrayNotAllowed,
                                Label::span(
                                    node.identifier.span(),
                                    "Array of function block instances",
                                ),
                            )
                            .with_context("function block", &element.to_string()),
                        );
                    }
                }
            }
        }
        Ok(())
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    fn flag_enabled() -> CompilerOptions {
        CompilerOptions {
            allow_fb_instance_arrays: true,
            ..CompilerOptions::default()
        }
    }

    const STANDARD_BLOCK: &str = "
PROGRAM main
VAR
    timers : ARRAY[0..2] OF TON;
END_VAR
    timers[1](IN := TRUE, PT := T#1s);
END_PROGRAM";

    const USER_BLOCK: &str = "
FUNCTION_BLOCK Acc
VAR_INPUT x : DINT; END_VAR
END_FUNCTION_BLOCK

PROGRAM main
VAR
    accs : ARRAY[0..2] OF Acc;
END_VAR
    accs[0](x := 1);
END_PROGRAM";

    rule_err1!(
        apply_when_array_of_standard_block_and_flag_disabled_then_error,
        STANDARD_BLOCK,
        Problem::FbInstanceArrayNotAllowed
    );

    rule_err1!(
        apply_when_array_of_user_block_and_flag_disabled_then_error,
        USER_BLOCK,
        Problem::FbInstanceArrayNotAllowed
    );

    rule_ok_with!(
        apply_when_array_of_standard_block_and_flag_enabled_then_ok,
        flag_enabled(),
        STANDARD_BLOCK
    );

    rule_ok_with!(
        apply_when_array_of_user_block_and_flag_enabled_then_ok,
        flag_enabled(),
        USER_BLOCK
    );

    rule_ok!(
        apply_when_array_of_elementary_type_then_never_flagged,
        "
PROGRAM main
VAR
    values : ARRAY[0..2] OF INT;
    timer : TON;
END_VAR
    timer(IN := TRUE, PT := T#1s);
END_PROGRAM"
    );
}
