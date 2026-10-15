//! Renders expressions, calls and call arguments.

use super::{LibraryRenderer, Render};
use ironplc_dsl::{diagnostic::Diagnostic, visitor::Visitor};
use paste::paste;

// 3.2.3
impl Render for dsl::textual::FbCall {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.visit_variable(&self.callee)?;

        r.write_ws("(");
        visit_comma_separated!(r, self.params.iter(), ParamAssignmentKind);
        r.write_ws(")");
        r.write_ws(";");
        r.newline();

        Ok(())
    }
}

// OOP extension: `instance.MethodName(args)`, `THIS^.MethodName(args)`
// (ADR-0041 Phase 1).
impl Render for dsl::textual::MethodCall {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.visit_method_receiver(&self.receiver)?;
        r.write(".");
        r.visit_id(&self.method)?;

        r.write_ws("(");
        visit_comma_separated!(r, self.params.iter(), ParamAssignmentKind);
        r.write_ws(")");

        Ok(())
    }
}

impl Render for dsl::textual::CompareExpr {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.write_ws("(");
        r.visit_expr(&self.left)?;

        r.write_ws(self.op.as_str());

        r.visit_expr(&self.right)?;
        r.write_ws(")");
        Ok(())
    }
}

impl Render for dsl::textual::BinaryExpr {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.write_ws("(");
        r.visit_expr(&self.left)?;

        let op = match self.op {
            dsl::textual::Operator::Add => "+",
            dsl::textual::Operator::Sub => "-",
            dsl::textual::Operator::Mul => "*",
            dsl::textual::Operator::Div => "/",
            dsl::textual::Operator::Mod => "MOD",
            dsl::textual::Operator::Pow => "**",
        };
        r.write_ws(op);

        r.visit_expr(&self.right)?;
        r.write_ws(")");

        Ok(())
    }
}

impl Render for dsl::textual::ExprKind {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        match self {
            dsl::textual::ExprKind::Ref(var) => {
                r.write_ws("REF(");
                r.visit_variable(var)?;
                r.write_ws(")");
                Ok(())
            }
            dsl::textual::ExprKind::Deref(expr) => {
                r.visit_expr(expr)?;
                // No separating space: the tight spelling is the canonical
                // output, not a parser constraint. #1437 widened
                // `unary_expression` to take `_` before the caret, so
                // `myRef ^` re-parses as well -- the `deref_operator` row in
                // `parser/src/tests/whitespace.rs`.
                r.write("^");
                Ok(())
            }
            dsl::textual::ExprKind::Null(_) => {
                r.write_ws("NULL");
                Ok(())
            }
            _ => self.recurse_visit(r),
        }
    }
}
