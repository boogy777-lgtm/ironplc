//! Renders the program organization units: functions, function blocks, methods, properties, interfaces, namespaces and programs.

use super::{LibraryRenderer, Render};
use ironplc_dsl::common::*;
use ironplc_dsl::{diagnostic::Diagnostic, visitor::Visitor};

impl LibraryRenderer {
    fn visit_function_return_type(&mut self, node: &FunctionReturnType) -> Result<(), Diagnostic> {
        match node {
            FunctionReturnType::Named(tn) => self.visit_type_name(tn)?,
            FunctionReturnType::String(spec) | FunctionReturnType::WString(spec) => {
                self.write_ws(spec.width.keyword());
                if let Some(len) = &spec.length {
                    self.write_ws("[");
                    self.visit_integer(len.as_integer().ok_or_else(Diagnostic::internal_error)?)?;
                    self.write_ws("]");
                }
            }
        }
        Ok(())
    }

    /// Renders the variable blocks and statements of a method or a property
    /// accessor; the caller writes the header and the closing keyword.
    fn render_callable_body(
        &mut self,
        variables: &[VarDecl],
        body: &[dsl::textual::StmtKind],
    ) -> Result<(), Diagnostic> {
        if !variables.is_empty() {
            self.indent();
            for item in variables.iter() {
                self.visit_var_decl(item)?;
            }
            self.outdent();
            self.newline();
        }

        self.indent();
        for stmt in body.iter() {
            self.visit_stmt_kind(stmt)?;
        }
        self.outdent();
        Ok(())
    }
}

// 2.5.1
impl Render for FunctionDeclaration {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.write_declaration_keyword(self, "FUNCTION");
        r.visit_id(&self.name)?;
        r.write_ws(":");
        r.visit_function_return_type(&self.return_type)?;

        if !self.variables.is_empty() {
            r.indent();
            for item in self.variables.iter() {
                r.visit_var_decl(item)?;
            }
            r.outdent();
            r.newline();
        }

        r.indent();
        for stmt in self.body.iter() {
            r.visit_stmt_kind(stmt)?;
        }
        r.outdent();

        r.write_ws("END_FUNCTION");
        r.newline();
        Ok(())
    }
}

// 2.5.1
impl Render for FunctionBlockDeclaration {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.write_declaration_keyword(self, "FUNCTION_BLOCK");
        if let Some(oop) = &self.oop {
            r.write_qualifiers(&oop.qualifiers);
        }
        r.visit_id(&self.name.name)?;
        // `VAR_GENERIC` sections belong directly after the name, before
        // `EXTENDS`/`IMPLEMENTS` (CODESYS error 544), so they are written
        // first and the remaining sections after the header.
        r.indent();
        for var in self
            .variables
            .iter()
            .filter(|var| var.var_type == VariableType::VarGeneric)
        {
            r.visit_var_decl(var)?;
        }
        r.outdent();
        if let Some(oop) = &self.oop {
            if let Some(base) = &oop.base {
                r.write_ws("EXTENDS");
                r.visit_id(&base.name)?;
            }
            if !oop.implements.is_empty() {
                r.write_ws("IMPLEMENTS");
                for (i, interface) in oop.implements.iter().enumerate() {
                    if i > 0 {
                        r.write_ws(",");
                    }
                    r.visit_id(&interface.name)?;
                }
            }
        }
        r.newline();

        r.indent();
        for var in self
            .variables
            .iter()
            .filter(|var| var.var_type != VariableType::VarGeneric)
        {
            r.visit_var_decl(var)?;
        }
        r.outdent();

        r.indent();
        self.body.recurse_visit(r)?;
        r.outdent();
        r.newline();

        for method in self.methods.iter() {
            r.visit_method_declaration(method)?;
        }

        for property in self.properties.iter() {
            r.visit_property_declaration(property)?;
        }

        r.write_ws("END_FUNCTION_BLOCK");
        r.newline();
        Ok(())
    }
}

// OOP extension: METHOD ... END_METHOD (ADR-0041 Phase 1).
impl Render for MethodDeclaration {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.write_declaration_keyword(self, "METHOD");
        r.write_qualifiers(&self.qualifiers);
        r.visit_id(&self.name)?;
        if let Some(return_type) = &self.return_type {
            r.write_ws(":");
            r.visit_function_return_type(return_type)?;
        }
        r.newline();

        r.render_callable_body(&self.variables, &self.body)?;

        r.write_ws("END_METHOD");
        r.newline();
        Ok(())
    }
}

// OOP extension: PROPERTY ... END_PROPERTY. Each accessor is a
// `MethodDeclaration`, but renders as `GET`/`SET` without a method
// header, and the SET accessor without the implicit input that holds
// the assigned value (see `PropertyDeclaration`).
impl Render for PropertyDeclaration {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.write_declaration_keyword(self, "PROPERTY");
        r.write_qualifiers(&self.qualifiers);
        r.visit_id(&self.name)?;
        r.write_ws(":");
        r.visit_function_return_type(&self.property_type)?;
        r.newline();

        if let Some(get) = &self.get {
            r.write_ws("GET");
            r.newline();
            r.render_callable_body(&get.variables, &get.body)?;
            r.write_ws("END_GET");
            r.newline();
        }

        if let Some(set) = &self.set {
            r.write_ws("SET");
            r.newline();
            r.render_callable_body(self.set_declared_variables(), &set.body)?;
            r.write_ws("END_SET");
            r.newline();
        }

        r.write_ws("END_PROPERTY");
        r.newline();
        Ok(())
    }
}

// OOP extension: INTERFACE ... END_INTERFACE. Only the
// header renders — method/property signatures are not yet parsed (see
// specs/design/beckhoff-twincat-dialect.md §1.3).
// CODESYS/TwinCAT `NAMESPACE ... END_NAMESPACE`: the nested declarations
// are written back inside the namespace, one level deeper.
impl Render for NamespaceDeclaration {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.write_ws("NAMESPACE");
        r.visit_id(&self.name)?;
        r.newline();
        r.indent();
        for element in self.elements.iter() {
            element.recurse_visit(r)?;
        }
        r.outdent();
        r.write_ws("END_NAMESPACE");
        r.newline();
        Ok(())
    }
}

impl Render for InterfaceDeclaration {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.write_declaration_keyword(self, "INTERFACE");
        r.visit_id(&self.name)?;
        if !self.extends.is_empty() {
            r.write_ws("EXTENDS");
            for (i, base) in self.extends.iter().enumerate() {
                if i > 0 {
                    r.write_ws(",");
                }
                r.visit_id(&base.name)?;
            }
        }
        r.newline();

        r.write_ws("END_INTERFACE");
        r.newline();
        Ok(())
    }
}

// 2.5.3
impl Render for ProgramDeclaration {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.write_declaration_keyword(self, "PROGRAM");

        r.visit_id(&self.name)?;
        r.newline();

        for var in self.variables.iter() {
            r.visit_var_decl(var)?;
        }
        for var in self.access_variables.iter() {
            r.visit_program_access_decl(var)?;
        }

        self.body.recurse_visit(r)?;

        r.write_ws("END_PROGRAM");
        r.newline();
        Ok(())
    }
}
