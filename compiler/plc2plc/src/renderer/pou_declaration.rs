//! Renders the program organization units: functions, function blocks, methods, properties, interfaces, namespaces and programs.

use super::LibraryRenderer;
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

    // 2.5.1
    pub(super) fn render_function_declaration(
        &mut self,
        node: &FunctionDeclaration,
    ) -> Result<(), Diagnostic> {
        self.write_declaration_keyword(node, "FUNCTION");
        self.visit_id(&node.name)?;
        self.write_ws(":");
        self.visit_function_return_type(&node.return_type)?;

        if !node.variables.is_empty() {
            self.indent();
            for item in node.variables.iter() {
                self.visit_var_decl(item)?;
            }
            self.outdent();
            self.newline();
        }

        self.indent();
        for stmt in node.body.iter() {
            self.visit_stmt_kind(stmt)?;
        }
        self.outdent();

        self.write_ws("END_FUNCTION");
        self.newline();
        Ok(())
    }

    // 2.5.1
    pub(super) fn render_function_block_declaration(
        &mut self,
        node: &FunctionBlockDeclaration,
    ) -> Result<(), Diagnostic> {
        self.write_declaration_keyword(node, "FUNCTION_BLOCK");
        if let Some(oop) = &node.oop {
            self.write_qualifiers(&oop.qualifiers);
        }
        self.visit_id(&node.name.name)?;
        // `VAR_GENERIC` sections belong directly after the name, before
        // `EXTENDS`/`IMPLEMENTS` (CODESYS error 544), so they are written
        // first and the remaining sections after the header.
        self.indent();
        for var in node
            .variables
            .iter()
            .filter(|var| var.var_type == VariableType::VarGeneric)
        {
            self.visit_var_decl(var)?;
        }
        self.outdent();
        if let Some(oop) = &node.oop {
            if let Some(base) = &oop.base {
                self.write_ws("EXTENDS");
                self.visit_id(&base.name)?;
            }
            if !oop.implements.is_empty() {
                self.write_ws("IMPLEMENTS");
                for (i, interface) in oop.implements.iter().enumerate() {
                    if i > 0 {
                        self.write_ws(",");
                    }
                    self.visit_id(&interface.name)?;
                }
            }
        }
        self.newline();

        self.indent();
        for var in node
            .variables
            .iter()
            .filter(|var| var.var_type != VariableType::VarGeneric)
        {
            self.visit_var_decl(var)?;
        }
        self.outdent();

        self.indent();
        node.body.recurse_visit(self)?;
        self.outdent();
        self.newline();

        for method in node.methods.iter() {
            self.visit_method_declaration(method)?;
        }

        for property in node.properties.iter() {
            self.visit_property_declaration(property)?;
        }

        self.write_ws("END_FUNCTION_BLOCK");
        self.newline();
        Ok(())
    }

    // OOP extension: METHOD ... END_METHOD (ADR-0041 Phase 1).
    pub(super) fn render_method_declaration(
        &mut self,
        node: &MethodDeclaration,
    ) -> Result<(), Diagnostic> {
        self.write_declaration_keyword(node, "METHOD");
        self.write_qualifiers(&node.qualifiers);
        self.visit_id(&node.name)?;
        if let Some(return_type) = &node.return_type {
            self.write_ws(":");
            self.visit_function_return_type(return_type)?;
        }
        self.newline();

        self.render_callable_body(&node.variables, &node.body)?;

        self.write_ws("END_METHOD");
        self.newline();
        Ok(())
    }

    // OOP extension: PROPERTY ... END_PROPERTY. Each accessor is a
    // `MethodDeclaration`, but renders as `GET`/`SET` without a method
    // header, and the SET accessor without the implicit input that holds
    // the assigned value (see `PropertyDeclaration`).
    pub(super) fn render_property_declaration(
        &mut self,
        node: &PropertyDeclaration,
    ) -> Result<(), Diagnostic> {
        self.write_declaration_keyword(node, "PROPERTY");
        self.write_qualifiers(&node.qualifiers);
        self.visit_id(&node.name)?;
        self.write_ws(":");
        self.visit_function_return_type(&node.property_type)?;
        self.newline();

        if let Some(get) = &node.get {
            self.write_ws("GET");
            self.newline();
            self.render_callable_body(&get.variables, &get.body)?;
            self.write_ws("END_GET");
            self.newline();
        }

        if let Some(set) = &node.set {
            self.write_ws("SET");
            self.newline();
            self.render_callable_body(node.set_declared_variables(), &set.body)?;
            self.write_ws("END_SET");
            self.newline();
        }

        self.write_ws("END_PROPERTY");
        self.newline();
        Ok(())
    }

    // OOP extension: INTERFACE ... END_INTERFACE. Only the
    // header renders — method/property signatures are not yet parsed (see
    // specs/design/beckhoff-twincat-dialect.md §1.3).
    // CODESYS/TwinCAT `NAMESPACE ... END_NAMESPACE`: the nested declarations
    // are written back inside the namespace, one level deeper.
    pub(super) fn render_namespace_declaration(
        &mut self,
        node: &NamespaceDeclaration,
    ) -> Result<(), Diagnostic> {
        self.write_ws("NAMESPACE");
        self.visit_id(&node.name)?;
        self.newline();
        self.indent();
        for element in node.elements.iter() {
            element.recurse_visit(self)?;
        }
        self.outdent();
        self.write_ws("END_NAMESPACE");
        self.newline();
        Ok(())
    }

    pub(super) fn render_interface_declaration(
        &mut self,
        node: &InterfaceDeclaration,
    ) -> Result<(), Diagnostic> {
        self.write_declaration_keyword(node, "INTERFACE");
        self.visit_id(&node.name)?;
        if !node.extends.is_empty() {
            self.write_ws("EXTENDS");
            for (i, base) in node.extends.iter().enumerate() {
                if i > 0 {
                    self.write_ws(",");
                }
                self.visit_id(&base.name)?;
            }
        }
        self.newline();

        self.write_ws("END_INTERFACE");
        self.newline();
        Ok(())
    }

    // 2.5.3
    pub(super) fn render_program_declaration(
        &mut self,
        node: &ProgramDeclaration,
    ) -> Result<(), Diagnostic> {
        self.write_declaration_keyword(node, "PROGRAM");

        self.visit_id(&node.name)?;
        self.newline();

        for var in node.variables.iter() {
            self.visit_var_decl(var)?;
        }
        for var in node.access_variables.iter() {
            self.visit_program_access_decl(var)?;
        }

        node.body.recurse_visit(self)?;

        self.write_ws("END_PROGRAM");
        self.newline();
        Ok(())
    }
}
