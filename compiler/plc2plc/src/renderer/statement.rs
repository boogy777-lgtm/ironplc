//! Renders structured text statements: assignments, selection, iteration, jumps and exception handling.

use super::LibraryRenderer;
use ironplc_dsl::{diagnostic::Diagnostic, visitor::Visitor};
use paste::paste;

impl LibraryRenderer {
    // A method call is rendered by `visit_method_call` in both positions;
    // only the statement form ends with `;`. `RETURN`, `EXIT` and `CONTINUE` have no
    // node to visit, so they are written here.
    pub(super) fn render_stmt_kind(
        &mut self,
        node: &dsl::textual::StmtKind,
    ) -> Result<(), Diagnostic> {
        match node {
            dsl::textual::StmtKind::MethodCall(call) => {
                self.visit_method_call(call)?;
                self.write_ws(";");
                self.newline();
                Ok(())
            }
            dsl::textual::StmtKind::Return => self.write_keyword_statement("RETURN"),
            dsl::textual::StmtKind::Exit(_) => self.write_keyword_statement("EXIT"),
            // The marker carries no statement terminator of its own: the
            // reference grammar's `implementationBlock` is the marker
            // followed by the statements, with no `;` in between.
            dsl::textual::StmtKind::BeginImplementation(_) => {
                self.write_ws("__BEGIN_IMPLEMENTATION");
                self.newline();
                Ok(())
            }
            dsl::textual::StmtKind::Continue(_) => self.write_keyword_statement("CONTINUE"),
            _ => node.recurse_visit(self),
        }
    }

    // 3.3.2.1
    pub(super) fn render_assignment(
        &mut self,
        node: &dsl::textual::Assignment,
    ) -> Result<(), Diagnostic> {
        self.visit_variable(&node.target)?;
        if node.set_bind {
            // Reproduce the TwinCAT/CODESYS `S=` set-bind operator.
            self.write_ws("S=");
            self.visit_expr(&node.value)?;
            self.write_ws(";");
            self.newline();
            return Ok(());
        }
        if node.reset_bind {
            // Reproduce the TwinCAT/CODESYS `R=` reset-bind operator.
            self.write_ws("R=");
            self.visit_expr(&node.value)?;
            self.write_ws(";");
            self.newline();
            return Ok(());
        }
        if node.ref_bind {
            // Reproduce the TwinCAT/CODESYS `REF=` binding. The value is always
            // an `ExprKind::Ref(referent)`; render `target REF= referent`.
            self.write_ws("REF=");
            if let dsl::textual::ExprKind::Ref(referent) = &node.value.kind {
                self.visit_variable(referent)?;
            } else {
                self.visit_expr(&node.value)?;
            }
            self.write_ws(";");
            self.newline();
            return Ok(());
        }
        if node.deref {
            self.write("^");
        }
        self.write_ws(":=");
        self.visit_expr(&node.value)?;

        self.write_ws(";");
        self.newline();
        Ok(())
    }

    pub(super) fn render_repeat(&mut self, node: &dsl::textual::Repeat) -> Result<(), Diagnostic> {
        self.write_ws("REPEAT");
        self.newline();

        self.indent();
        for item in node.body.iter() {
            self.visit_stmt_kind(item)?;
        }
        self.outdent();

        self.write_ws("UNTIL");
        self.visit_expr(&node.until)?;
        self.newline();

        self.write_ws("END_REPEAT");
        self.write_ws(";");
        self.newline();
        self.newline();

        Ok(())
    }

    // CODESYS exception handling. The `__TRY`-family keywords are not in
    // `StmtKind` themselves, so the statement is written here.
    pub(super) fn render_try_catch(
        &mut self,
        node: &dsl::textual::TryCatch,
    ) -> Result<(), Diagnostic> {
        self.write_ws("__TRY");
        self.newline();

        self.indent();
        for item in node.body.iter() {
            self.visit_stmt_kind(item)?;
        }
        self.outdent();

        if let Some(catch) = &node.catch {
            self.visit_catch_clause(catch)?;
        }

        if !node.finally_body.is_empty() {
            self.write_ws("__FINALLY");
            self.newline();

            self.indent();
            for item in node.finally_body.iter() {
                self.visit_stmt_kind(item)?;
            }
            self.outdent();
        }

        self.write_ws("__ENDTRY");
        self.write_ws(";");
        self.newline();
        self.newline();

        Ok(())
    }

    pub(super) fn render_catch_clause(
        &mut self,
        node: &dsl::textual::CatchClause,
    ) -> Result<(), Diagnostic> {
        self.write_ws("__CATCH");
        if let Some(exception) = &node.exception {
            self.write("(");
            self.visit_variable(exception)?;
            self.write(")");
        }
        self.newline();

        self.indent();
        for item in node.body.iter() {
            self.visit_stmt_kind(item)?;
        }
        self.outdent();

        Ok(())
    }

    // `JMP` / `label:` / `CALC` / `__WAIT`, the CODESYS jump statements.
    pub(super) fn render_jump(&mut self, node: &dsl::textual::Jump) -> Result<(), Diagnostic> {
        self.write_ws("JMP");
        if let Some(condition) = &node.condition {
            self.write("(");
            self.visit_expr(condition)?;
            self.write(")");
        }
        self.write_ws(node.label.original().as_str());
        self.write_ws(";");
        self.newline();

        Ok(())
    }

    pub(super) fn render_conditional_call(
        &mut self,
        node: &dsl::textual::ConditionalCall,
    ) -> Result<(), Diagnostic> {
        self.write_ws("CALC");
        self.write("(");
        self.visit_expr(&node.condition)?;
        self.write(",");
        self.visit_variable(&node.call.callee)?;
        self.write("(");
        visit_comma_separated!(self, node.call.params.iter(), ParamAssignmentKind);
        self.write(")");
        self.write(")");
        self.write_ws(";");
        self.newline();

        Ok(())
    }

    pub(super) fn render_wait(&mut self, node: &dsl::textual::Wait) -> Result<(), Diagnostic> {
        self.write_ws("__WAIT");
        if let Some(condition) = &node.condition {
            self.write("(");
            self.visit_expr(condition)?;
            self.write(")");
        }
        self.write_ws(";");
        self.newline();

        Ok(())
    }

    pub(super) fn render_throw(&mut self, node: &dsl::textual::Throw) -> Result<(), Diagnostic> {
        self.write_ws("__THROW");
        if let Some(value) = &node.value {
            self.write("(");
            self.visit_expr(value)?;
            self.write(")");
        }
        self.write_ws(";");
        self.newline();

        Ok(())
    }

    pub(super) fn render_if(&mut self, node: &dsl::textual::If) -> Result<(), Diagnostic> {
        self.write_ws("IF");
        self.visit_expr(&node.expr)?;
        self.write_ws("THEN");
        self.newline();

        self.indent();
        for item in node.body.iter() {
            self.visit_stmt_kind(item)?;
        }
        self.outdent();

        for item in node.else_ifs.iter() {
            self.visit_else_if(item)?;
        }

        if !node.else_body.is_empty() {
            self.write_ws("ELSE");
            self.newline();

            self.indent();
            for item in node.else_body.iter() {
                self.visit_stmt_kind(item)?;
            }
            self.outdent();
        }

        self.write_ws("END_IF");
        self.write_ws(";");
        self.newline();
        self.newline();
        Ok(())
    }

    pub(super) fn render_else_if(&mut self, node: &dsl::textual::ElseIf) -> Result<(), Diagnostic> {
        self.write_ws("ELSIF");
        self.visit_expr(&node.expr)?;
        self.write_ws("THEN");
        self.newline();

        self.indent();
        for item in node.body.iter() {
            self.visit_stmt_kind(item)?;
        }
        self.outdent();

        Ok(())
    }

    pub(super) fn render_case(&mut self, node: &dsl::textual::Case) -> Result<(), Diagnostic> {
        self.write_ws("CASE");
        self.visit_expr(&node.selector)?;
        self.write_ws("OF");
        self.newline();

        self.indent();
        for item in node.statement_groups.iter() {
            self.visit_case_statement_group(item)?;
        }

        if !node.else_body.is_empty() {
            self.write_ws("ELSE");
            self.newline();
            self.indent();

            for item in node.else_body.iter() {
                self.visit_stmt_kind(item)?;
            }

            self.outdent();
        }
        self.outdent();

        self.write_ws("END_CASE");
        self.write_ws(";");
        self.newline();
        self.newline();

        Ok(())
    }

    pub(super) fn render_case_statement_group(
        &mut self,
        node: &dsl::textual::CaseStatementGroup,
    ) -> Result<(), Diagnostic> {
        for selector in node.selectors.iter() {
            self.visit_case_selection_kind(selector)?;
            self.write_ws(":");
            self.newline();
        }

        self.indent();

        if node.statements.is_empty() {
            self.write_ws("(* empty *)");
            self.write_ws(";");
            self.newline();
        } else {
            for item in node.statements.iter() {
                self.visit_stmt_kind(item)?;
            }
        }
        self.outdent();

        Ok(())
    }

    pub(super) fn render_for(&mut self, node: &dsl::textual::For) -> Result<(), Diagnostic> {
        self.write_ws("FOR");
        self.visit_id(&node.control)?;
        self.write_ws(":=");
        self.visit_expr(&node.from)?;
        self.write_ws("TO");
        self.visit_expr(&node.to)?;

        if let Some(by) = &node.step {
            self.write_ws("BY");
            self.visit_expr(by)?;
        }

        self.write_ws("DO");
        self.newline();

        self.indent();
        for item in node.body.iter() {
            self.visit_stmt_kind(item)?;
        }
        self.outdent();

        self.write_ws("END_FOR");
        self.write_ws(";");
        self.newline();
        self.newline();
        Ok(())
    }

    pub(super) fn render_while(&mut self, node: &dsl::textual::While) -> Result<(), Diagnostic> {
        self.write_ws("WHILE");
        self.visit_expr(&node.condition)?;
        self.write_ws("DO");
        self.newline();

        self.indent();
        for item in node.body.iter() {
            self.visit_stmt_kind(item)?;
        }
        self.outdent();

        self.write_ws("END_WHILE");
        self.write_ws(";");
        self.newline();
        self.newline();
        Ok(())
    }
}
