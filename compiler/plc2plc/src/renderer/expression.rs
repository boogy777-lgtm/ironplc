//! Renders expressions, calls and call arguments.

use super::LibraryRenderer;
use ironplc_dsl::{diagnostic::Diagnostic, visitor::Visitor};
use paste::paste;

impl LibraryRenderer {
    // 3.2.3
    pub(super) fn render_fb_call(&mut self, node: &dsl::textual::FbCall) -> Result<(), Diagnostic> {
        self.visit_variable(&node.callee)?;

        self.write_ws("(");
        visit_comma_separated!(self, node.params.iter(), ParamAssignmentKind);
        self.write_ws(")");
        self.write_ws(";");
        self.newline();

        Ok(())
    }

    // OOP extension: `instance.MethodName(args)`, `THIS^.MethodName(args)`
    // (ADR-0041 Phase 1).
    pub(super) fn render_method_call(
        &mut self,
        node: &dsl::textual::MethodCall,
    ) -> Result<(), Diagnostic> {
        self.visit_method_receiver(&node.receiver)?;
        self.write(".");
        self.visit_id(&node.method)?;

        self.write_ws("(");
        visit_comma_separated!(self, node.params.iter(), ParamAssignmentKind);
        self.write_ws(")");

        Ok(())
    }

    pub(super) fn render_compare_expr(
        &mut self,
        node: &dsl::textual::CompareExpr,
    ) -> Result<(), Diagnostic> {
        self.write_ws("(");
        self.visit_expr(&node.left)?;

        self.write_ws(node.op.as_str());

        self.visit_expr(&node.right)?;
        self.write_ws(")");
        Ok(())
    }

    pub(super) fn render_binary_expr(
        &mut self,
        node: &dsl::textual::BinaryExpr,
    ) -> Result<(), Diagnostic> {
        self.write_ws("(");
        self.visit_expr(&node.left)?;

        let op = match node.op {
            dsl::textual::Operator::Add => "+",
            dsl::textual::Operator::Sub => "-",
            dsl::textual::Operator::Mul => "*",
            dsl::textual::Operator::Div => "/",
            dsl::textual::Operator::Mod => "MOD",
            dsl::textual::Operator::Pow => "**",
        };
        self.write_ws(op);

        self.visit_expr(&node.right)?;
        self.write_ws(")");

        Ok(())
    }

    pub(super) fn render_expr_kind(
        &mut self,
        node: &dsl::textual::ExprKind,
    ) -> Result<(), Diagnostic> {
        match node {
            dsl::textual::ExprKind::Ref(var) => {
                self.write_ws("REF(");
                self.visit_variable(var)?;
                self.write_ws(")");
                Ok(())
            }
            dsl::textual::ExprKind::Deref(expr) => {
                self.visit_expr(expr)?;
                // No separating space: the tight spelling is the canonical
                // output, not a parser constraint. #1437 widened
                // `unary_expression` to take `_` before the caret, so
                // `myRef ^` re-parses as well -- the `deref_operator` row in
                // `parser/src/tests/whitespace.rs`.
                self.write("^");
                Ok(())
            }
            dsl::textual::ExprKind::Null(_) => {
                self.write_ws("NULL");
                Ok(())
            }
            _ => node.recurse_visit(self),
        }
    }
}
