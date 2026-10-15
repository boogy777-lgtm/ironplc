//! Renders structured text statements: assignments, selection, iteration, jumps and exception handling.

use super::{LibraryRenderer, Render};
use ironplc_dsl::{diagnostic::Diagnostic, visitor::Visitor};
use paste::paste;

// A method call is rendered by `visit_method_call` in both positions;
// only the statement form ends with `;`. `RETURN`, `EXIT` and `CONTINUE` have no
// node to visit, so they are written here.
impl Render for dsl::textual::StmtKind {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        match self {
            dsl::textual::StmtKind::MethodCall(call) => {
                r.visit_method_call(call)?;
                r.write_ws(";");
                r.newline();
                Ok(())
            }
            dsl::textual::StmtKind::Return => r.write_keyword_statement("RETURN"),
            dsl::textual::StmtKind::Exit(_) => r.write_keyword_statement("EXIT"),
            // The marker carries no statement terminator of its own: the
            // reference grammar's `implementationBlock` is the marker
            // followed by the statements, with no `;` in between.
            dsl::textual::StmtKind::BeginImplementation(_) => {
                r.write_ws("__BEGIN_IMPLEMENTATION");
                r.newline();
                Ok(())
            }
            dsl::textual::StmtKind::Continue(_) => r.write_keyword_statement("CONTINUE"),
            _ => self.recurse_visit(r),
        }
    }
}

// 3.3.2.1
impl Render for dsl::textual::Assignment {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.visit_variable(&self.target)?;
        if self.set_bind {
            // Reproduce the TwinCAT/CODESYS `S=` set-bind operator.
            r.write_ws("S=");
            r.visit_expr(&self.value)?;
            r.write_ws(";");
            r.newline();
            return Ok(());
        }
        if self.reset_bind {
            // Reproduce the TwinCAT/CODESYS `R=` reset-bind operator.
            r.write_ws("R=");
            r.visit_expr(&self.value)?;
            r.write_ws(";");
            r.newline();
            return Ok(());
        }
        if self.ref_bind {
            // Reproduce the TwinCAT/CODESYS `REF=` binding. The value is always
            // an `ExprKind::Ref(referent)`; render `target REF= referent`.
            r.write_ws("REF=");
            if let dsl::textual::ExprKind::Ref(referent) = &self.value.kind {
                r.visit_variable(referent)?;
            } else {
                r.visit_expr(&self.value)?;
            }
            r.write_ws(";");
            r.newline();
            return Ok(());
        }
        if self.deref {
            r.write("^");
        }
        r.write_ws(":=");
        r.visit_expr(&self.value)?;

        r.write_ws(";");
        r.newline();
        Ok(())
    }
}

impl Render for dsl::textual::Repeat {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.write_ws("REPEAT");
        r.newline();

        r.indent();
        for item in self.body.iter() {
            r.visit_stmt_kind(item)?;
        }
        r.outdent();

        r.write_ws("UNTIL");
        r.visit_expr(&self.until)?;
        r.newline();

        r.write_ws("END_REPEAT");
        r.write_ws(";");
        r.newline();
        r.newline();

        Ok(())
    }
}

// CODESYS exception handling. The `__TRY`-family keywords are not in
// `StmtKind` themselves, so the statement is written here.
impl Render for dsl::textual::TryCatch {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.write_ws("__TRY");
        r.newline();

        r.indent();
        for item in self.body.iter() {
            r.visit_stmt_kind(item)?;
        }
        r.outdent();

        if let Some(catch) = &self.catch {
            r.visit_catch_clause(catch)?;
        }

        if !self.finally_body.is_empty() {
            r.write_ws("__FINALLY");
            r.newline();

            r.indent();
            for item in self.finally_body.iter() {
                r.visit_stmt_kind(item)?;
            }
            r.outdent();
        }

        r.write_ws("__ENDTRY");
        r.write_ws(";");
        r.newline();
        r.newline();

        Ok(())
    }
}

impl Render for dsl::textual::CatchClause {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.write_ws("__CATCH");
        if let Some(exception) = &self.exception {
            r.write("(");
            r.visit_variable(exception)?;
            r.write(")");
        }
        r.newline();

        r.indent();
        for item in self.body.iter() {
            r.visit_stmt_kind(item)?;
        }
        r.outdent();

        Ok(())
    }
}

// `JMP` / `label:` / `CALC` / `__WAIT`, the CODESYS jump statements.
impl Render for dsl::textual::Jump {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.write_ws("JMP");
        if let Some(condition) = &self.condition {
            r.write("(");
            r.visit_expr(condition)?;
            r.write(")");
        }
        r.write_ws(self.label.original().as_str());
        r.write_ws(";");
        r.newline();

        Ok(())
    }
}

impl Render for dsl::textual::ConditionalCall {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.write_ws("CALC");
        r.write("(");
        r.visit_expr(&self.condition)?;
        r.write(",");
        r.visit_variable(&self.call.callee)?;
        r.write("(");
        visit_comma_separated!(r, self.call.params.iter(), ParamAssignmentKind);
        r.write(")");
        r.write(")");
        r.write_ws(";");
        r.newline();

        Ok(())
    }
}

impl Render for dsl::textual::Wait {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.write_ws("__WAIT");
        if let Some(condition) = &self.condition {
            r.write("(");
            r.visit_expr(condition)?;
            r.write(")");
        }
        r.write_ws(";");
        r.newline();

        Ok(())
    }
}

impl Render for dsl::textual::Throw {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.write_ws("__THROW");
        if let Some(value) = &self.value {
            r.write("(");
            r.visit_expr(value)?;
            r.write(")");
        }
        r.write_ws(";");
        r.newline();

        Ok(())
    }
}

impl Render for dsl::textual::If {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.write_ws("IF");
        r.visit_expr(&self.expr)?;
        r.write_ws("THEN");
        r.newline();

        r.indent();
        for item in self.body.iter() {
            r.visit_stmt_kind(item)?;
        }
        r.outdent();

        for item in self.else_ifs.iter() {
            r.visit_else_if(item)?;
        }

        if !self.else_body.is_empty() {
            r.write_ws("ELSE");
            r.newline();

            r.indent();
            for item in self.else_body.iter() {
                r.visit_stmt_kind(item)?;
            }
            r.outdent();
        }

        r.write_ws("END_IF");
        r.write_ws(";");
        r.newline();
        r.newline();
        Ok(())
    }
}

impl Render for dsl::textual::ElseIf {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.write_ws("ELSIF");
        r.visit_expr(&self.expr)?;
        r.write_ws("THEN");
        r.newline();

        r.indent();
        for item in self.body.iter() {
            r.visit_stmt_kind(item)?;
        }
        r.outdent();

        Ok(())
    }
}

impl Render for dsl::textual::Case {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.write_ws("CASE");
        r.visit_expr(&self.selector)?;
        r.write_ws("OF");
        r.newline();

        r.indent();
        for item in self.statement_groups.iter() {
            r.visit_case_statement_group(item)?;
        }

        if !self.else_body.is_empty() {
            r.write_ws("ELSE");
            r.newline();
            r.indent();

            for item in self.else_body.iter() {
                r.visit_stmt_kind(item)?;
            }

            r.outdent();
        }
        r.outdent();

        r.write_ws("END_CASE");
        r.write_ws(";");
        r.newline();
        r.newline();

        Ok(())
    }
}

impl Render for dsl::textual::CaseStatementGroup {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        for selector in self.selectors.iter() {
            r.visit_case_selection_kind(selector)?;
            r.write_ws(":");
            r.newline();
        }

        r.indent();

        if self.statements.is_empty() {
            r.write_ws("(* empty *)");
            r.write_ws(";");
            r.newline();
        } else {
            for item in self.statements.iter() {
                r.visit_stmt_kind(item)?;
            }
        }
        r.outdent();

        Ok(())
    }
}

impl Render for dsl::textual::For {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.write_ws("FOR");
        r.visit_id(&self.control)?;
        r.write_ws(":=");
        r.visit_expr(&self.from)?;
        r.write_ws("TO");
        r.visit_expr(&self.to)?;

        if let Some(by) = &self.step {
            r.write_ws("BY");
            r.visit_expr(by)?;
        }

        r.write_ws("DO");
        r.newline();

        r.indent();
        for item in self.body.iter() {
            r.visit_stmt_kind(item)?;
        }
        r.outdent();

        r.write_ws("END_FOR");
        r.write_ws(";");
        r.newline();
        r.newline();
        Ok(())
    }
}

impl Render for dsl::textual::While {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.write_ws("WHILE");
        r.visit_expr(&self.condition)?;
        r.write_ws("DO");
        r.newline();

        r.indent();
        for item in self.body.iter() {
            r.visit_stmt_kind(item)?;
        }
        r.outdent();

        r.write_ws("END_WHILE");
        r.write_ws(";");
        r.newline();
        r.newline();
        Ok(())
    }
}
