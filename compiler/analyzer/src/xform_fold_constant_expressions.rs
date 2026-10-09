//! Transform that folds constant expressions at compile time.
//!
//! When both operands of a binary expression are compile-time constant
//! literals (integer or real), the pass evaluates the operation and replaces
//! the expression node with a single constant. Similarly, unary negation of
//! a constant literal is folded into the negated constant.
//!
//! This runs after `xform_resolve_expr_types` so that `expr_type` is
//! available on every `Expr` node.
//!
//! ## Before
//!
//! ```ignore
//! x := 2 + 3;
//! ```
//!
//! ## After
//!
//! ```ignore
//! x := 5;
//! ```
use ironplc_dsl::common::*;
use ironplc_dsl::core::{Located, SourceSpan};
use ironplc_dsl::diagnostic::Diagnostic;
use ironplc_dsl::fold::Fold;
use ironplc_dsl::textual::*;

use crate::constant_folding::{fold_error_to_diagnostic, try_fold_binary, try_fold_unary};
use crate::pass_runner::Outcome;
use std::convert::Infallible;

/// An operation on constants that has no result (a division by zero, an
/// overflow) is diagnosed where it stands and stays an operation; every other
/// constant operation is folded.
pub fn apply(lib: Library) -> Outcome {
    let mut folder = ConstantFolder {
        diagnostics: Vec::new(),
    };
    let Ok(library) = folder.fold_library(lib);
    Outcome::new(library, folder.diagnostics)
}

struct ConstantFolder {
    diagnostics: Vec<Diagnostic>,
}

/// Gives a folded integer literal the span of the expression it replaces.
///
/// A folded real literal is built with its span, so it needs nothing here.
fn with_span(kind: ExprKind, span: SourceSpan) -> ExprKind {
    match kind {
        ExprKind::Const(ConstantKind::IntegerLiteral(mut literal)) => {
            literal.value.value.span = span;
            ExprKind::Const(ConstantKind::IntegerLiteral(literal))
        }
        other => other,
    }
}

impl Fold<Infallible> for ConstantFolder {
    fn fold_expr(&mut self, node: Expr) -> Result<Expr, Infallible> {
        // Recurse into children first (bottom-up folding).
        let node = Expr::recurse_fold(node, self)?;

        let folded_kind = match &node.kind {
            ExprKind::BinaryOp(binary) => match try_fold_binary(binary) {
                Ok(folded) => folded,
                Err(error) => {
                    self.diagnostics
                        .push(fold_error_to_diagnostic(error, node.span()));
                    None
                }
            },
            ExprKind::UnaryOp(unary) => try_fold_unary(unary),
            _ => None,
        };

        match folded_kind {
            Some(kind) => {
                // The folded literal is a new node, so it carries the span of
                // the expression it replaces. Without this a diagnostic about
                // `255 + 1` has nowhere to point.
                let span = node.span.clone();
                Ok(Expr {
                    kind: with_span(kind, span),
                    ..node
                })
            }
            None => Ok(node),
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::constant_folding::integer_value;
    use crate::test_helpers::parse_and_resolve_types;
    use ironplc_dsl::visitor::Visitor;
    use ironplc_problems::Problem;
    use rstest::rstest;

    fn apply_fold(program: &str) -> Library {
        let library = parse_and_resolve_types(program);
        apply(library).library
    }

    /// The codes the pass reports for the assignment `x := <expr>` to a variable
    /// of type `declared`.
    fn codes_of(declared: &str, expr: &str) -> Vec<String> {
        let library = parse_and_resolve_types(&format!(
            "PROGRAM main VAR x : {declared}; END_VAR x := {expr}; END_PROGRAM"
        ));
        apply(library)
            .diagnostics
            .into_iter()
            .map(|d| d.code)
            .collect()
    }

    /// Extracts all `Expr` nodes from a library for inspection.
    struct ExprCollector {
        exprs: Vec<ExprKind>,
    }

    impl Visitor<Diagnostic> for ExprCollector {
        type Value = ();
        fn visit_expr(&mut self, node: &Expr) -> Result<(), Diagnostic> {
            self.exprs.push(node.kind.clone());
            node.recurse_visit(self)
        }
    }

    fn collect_exprs(library: &Library) -> Vec<ExprKind> {
        let mut collector = ExprCollector { exprs: vec![] };
        collector.walk(library).unwrap();
        collector.exprs
    }

    fn assert_has_integer_const(exprs: &[ExprKind], expected: i128) {
        let found = exprs.iter().any(|e| {
            if let ExprKind::Const(ConstantKind::IntegerLiteral(lit)) = e {
                integer_value(lit) == expected
            } else {
                false
            }
        });
        assert!(
            found,
            "Expected integer constant {} in expressions: {:?}",
            expected, exprs
        );
    }

    fn assert_has_real_const(exprs: &[ExprKind], expected: f64) {
        let found = exprs.iter().any(|e| {
            if let ExprKind::Const(ConstantKind::RealLiteral(lit)) = e {
                (lit.value - expected).abs() < f64::EPSILON
            } else {
                false
            }
        });
        assert!(
            found,
            "Expected real constant {} in expressions: {:?}",
            expected, exprs
        );
    }

    fn assert_no_binary_ops(exprs: &[ExprKind]) {
        let has_binary = exprs.iter().any(|e| matches!(e, ExprKind::BinaryOp(_)));
        assert!(
            !has_binary,
            "Expected no binary ops but found some in: {:?}",
            exprs
        );
    }

    // --- Binary integer folding ---

    #[test]
    fn fold_expr_when_add_two_integers_then_produces_constant() {
        let lib = apply_fold("PROGRAM main VAR x : INT; END_VAR x := 2 + 3; END_PROGRAM");
        let exprs = collect_exprs(&lib);
        assert_has_integer_const(&exprs, 5);
        assert_no_binary_ops(&exprs);
    }

    #[test]
    fn fold_expr_when_sub_two_integers_then_produces_constant() {
        let lib = apply_fold("PROGRAM main VAR x : INT; END_VAR x := 10 - 4; END_PROGRAM");
        let exprs = collect_exprs(&lib);
        assert_has_integer_const(&exprs, 6);
        assert_no_binary_ops(&exprs);
    }

    #[test]
    fn fold_expr_when_mul_two_integers_then_produces_constant() {
        let lib = apply_fold("PROGRAM main VAR x : INT; END_VAR x := 3 * 7; END_PROGRAM");
        let exprs = collect_exprs(&lib);
        assert_has_integer_const(&exprs, 21);
        assert_no_binary_ops(&exprs);
    }

    #[test]
    fn fold_expr_when_div_two_integers_then_produces_constant() {
        let lib = apply_fold("PROGRAM main VAR x : INT; END_VAR x := 20 / 4; END_PROGRAM");
        let exprs = collect_exprs(&lib);
        assert_has_integer_const(&exprs, 5);
        assert_no_binary_ops(&exprs);
    }

    #[test]
    fn fold_expr_when_mod_two_integers_then_produces_constant() {
        let lib = apply_fold("PROGRAM main VAR x : INT; END_VAR x := 17 MOD 5; END_PROGRAM");
        let exprs = collect_exprs(&lib);
        assert_has_integer_const(&exprs, 2);
        assert_no_binary_ops(&exprs);
    }

    #[test]
    fn fold_expr_when_mod_two_reals_then_left_unfolded() {
        // MOD is defined over ANY_INT only, so a real MOD is left for the
        // operand-type rule to reject rather than folded into a remainder.
        let lib = apply_fold("PROGRAM main VAR x : REAL; END_VAR x := 7.5 MOD 2.0; END_PROGRAM");
        let exprs = collect_exprs(&lib);
        assert!(
            exprs.iter().any(|e| matches!(e, ExprKind::BinaryOp(_))),
            "Expected the MOD expression to remain: {:?}",
            exprs
        );
    }

    #[test]
    fn fold_expr_when_mod_int_by_real_then_left_unfolded() {
        let lib = apply_fold("PROGRAM main VAR x : REAL; END_VAR x := 7 MOD 2.0; END_PROGRAM");
        let exprs = collect_exprs(&lib);
        assert!(
            exprs.iter().any(|e| matches!(e, ExprKind::BinaryOp(_))),
            "Expected the MOD expression to remain: {:?}",
            exprs
        );
    }

    #[test]
    fn fold_expr_when_int_div_by_zero_then_error() {
        assert_eq!(
            vec![Problem::ConstantExpressionDivisionByZero.code()],
            codes_of("INT", "10 / 0")
        );
    }

    #[test]
    fn fold_expr_when_int_mod_by_zero_then_error() {
        assert_eq!(
            vec![Problem::ConstantExpressionDivisionByZero.code()],
            codes_of("INT", "10 MOD 0")
        );
    }

    #[test]
    fn fold_expr_when_real_div_by_zero_then_error() {
        assert_eq!(
            vec![Problem::ConstantExpressionDivisionByZero.code()],
            codes_of("LREAL", "1.0 / 0.0")
        );
    }

    #[test]
    fn fold_expr_when_int_overflow_then_error() {
        assert_eq!(
            vec![Problem::ConstantExpressionOverflow.code()],
            codes_of("LINT", "170141183460469231731687303715884105727 * 2")
        );
    }

    #[rstest]
    #[case("1.0E300 * 1.0E300")]
    #[case("1.0E300 ** 2.0")]
    #[case("1.0E308 + 1.0E308")]
    #[case("-1.0E308 - 1.0E308")]
    #[case("1.0E300 / 1.0E-300")]
    #[case("0.0 ** -1.0")]
    #[case("(-8.0) ** 0.5")]
    fn fold_expr_when_real_result_not_finite_then_overflow_error(#[case] expr: &str) {
        assert_eq!(
            vec![Problem::ConstantExpressionOverflow.code()],
            codes_of("LREAL", expr)
        );
    }

    #[test]
    fn fold_expr_when_real_result_large_but_finite_then_produces_constant() {
        let lib = apply_fold("PROGRAM main VAR x : LREAL; END_VAR x := 1.0E300 * 2.0; END_PROGRAM");
        let exprs = collect_exprs(&lib);
        assert_has_real_const(&exprs, 2.0E300);
        assert_no_binary_ops(&exprs);
    }

    #[test]
    fn fold_expr_when_negative_integer_exponent_then_no_fold_no_error() {
        // Negative integer exponentiation is not meaningful for integers;
        // it stays unfolded and is not reported as an overflow.
        let library =
            parse_and_resolve_types("PROGRAM main VAR x : INT; END_VAR x := 2 ** -1; END_PROGRAM");
        let lib = apply(library).library;
        let exprs = collect_exprs(&lib);
        let has_binary = exprs.iter().any(|e| matches!(e, ExprKind::BinaryOp(_)));
        assert!(has_binary, "Negative exponent should not be folded");
    }

    // --- Nested constant folding ---

    #[test]
    fn fold_expr_when_nested_binary_then_folds_completely() {
        let lib = apply_fold("PROGRAM main VAR x : INT; END_VAR x := (2 + 3) * 4; END_PROGRAM");
        let exprs = collect_exprs(&lib);
        assert_has_integer_const(&exprs, 20);
        assert_no_binary_ops(&exprs);
    }

    // --- Binary real folding ---

    #[test]
    fn fold_expr_when_add_two_reals_then_produces_constant() {
        let lib = apply_fold("PROGRAM main VAR x : REAL; END_VAR x := 1.5 + 2.5; END_PROGRAM");
        let exprs = collect_exprs(&lib);
        assert_has_real_const(&exprs, 4.0);
        assert_no_binary_ops(&exprs);
    }

    #[test]
    fn fold_expr_when_mul_two_reals_then_produces_constant() {
        let lib = apply_fold("PROGRAM main VAR x : REAL; END_VAR x := 3.0 * 2.0; END_PROGRAM");
        let exprs = collect_exprs(&lib);
        assert_has_real_const(&exprs, 6.0);
        assert_no_binary_ops(&exprs);
    }

    // --- Unary negation folding ---

    #[test]
    fn fold_expr_when_negate_integer_then_produces_constant() {
        let lib = apply_fold("PROGRAM main VAR x : INT; END_VAR x := -5; END_PROGRAM");
        let exprs = collect_exprs(&lib);
        assert_has_integer_const(&exprs, -5);
        let has_unary = exprs.iter().any(|e| matches!(e, ExprKind::UnaryOp(_)));
        assert!(!has_unary, "Unary negation should be folded");
    }

    #[test]
    fn fold_expr_when_negate_real_then_produces_constant() {
        let lib = apply_fold("PROGRAM main VAR x : REAL; END_VAR x := -3.25; END_PROGRAM");
        let exprs = collect_exprs(&lib);
        assert_has_real_const(&exprs, -3.25);
        let has_unary = exprs.iter().any(|e| matches!(e, ExprKind::UnaryOp(_)));
        assert!(!has_unary, "Unary negation should be folded");
    }

    // --- Mixed integer + real folding ---

    #[test]
    fn fold_expr_when_add_integer_and_real_then_produces_real_constant() {
        let lib = apply_fold("PROGRAM main VAR x : REAL; END_VAR x := 2 + 3.5; END_PROGRAM");
        let exprs = collect_exprs(&lib);
        assert_has_real_const(&exprs, 5.5);
        assert_no_binary_ops(&exprs);
    }

    #[test]
    fn fold_expr_when_add_real_and_integer_then_produces_real_constant() {
        let lib = apply_fold("PROGRAM main VAR x : REAL; END_VAR x := 1.5 + 2; END_PROGRAM");
        let exprs = collect_exprs(&lib);
        assert_has_real_const(&exprs, 3.5);
        assert_no_binary_ops(&exprs);
    }

    #[test]
    fn fold_expr_when_mul_integer_and_real_then_produces_real_constant() {
        let lib = apply_fold("PROGRAM main VAR x : REAL; END_VAR x := 3 * 2.5; END_PROGRAM");
        let exprs = collect_exprs(&lib);
        assert_has_real_const(&exprs, 7.5);
        assert_no_binary_ops(&exprs);
    }

    // --- Non-constant operands are left unchanged ---

    #[test]
    fn fold_expr_when_variable_operand_then_no_fold() {
        let lib = apply_fold("PROGRAM main VAR x : INT; y : INT; END_VAR x := y + 3; END_PROGRAM");
        let exprs = collect_exprs(&lib);
        let has_binary = exprs.iter().any(|e| matches!(e, ExprKind::BinaryOp(_)));
        assert!(has_binary, "Non-constant binary should not be folded");
    }
}
