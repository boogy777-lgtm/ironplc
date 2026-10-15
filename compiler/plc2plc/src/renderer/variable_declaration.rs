//! Renders variable declarations, direct addresses and the initial values that follow a variable.

use super::text::character_string_text;
use super::LibraryRenderer;
use ironplc_dsl::common::*;
use ironplc_dsl::{diagnostic::Diagnostic, visitor::Visitor};
use paste::paste;

impl LibraryRenderer {
    pub(super) fn render_program_access_decl(
        &mut self,
        node: &ProgramAccessDecl,
    ) -> Result<(), Diagnostic> {
        self.newline();

        self.write_ws("VAR_ACCESS");

        self.newline();
        self.indent();

        self.visit_id(&node.access_name)?;
        self.write_ws(":");
        self.visit_symbolic_variable_kind(&node.symbolic_variable)?;
        self.write_ws(":");
        self.visit_type_name(&node.type_name)?;

        if let Some(dir) = &node.direction {
            let dir = match dir {
                dsl::configuration::Direction::ReadWrite => "READ_WRITE",
                dsl::configuration::Direction::ReadOnly => "READ_ONLY",
            };
            self.write_ws(dir);
        }

        self.write(";");
        self.newline();
        self.outdent();

        self.write_ws("END_VAR");
        self.newline();
        Ok(())
    }

    // 2.4.3
    pub(super) fn render_var_decl(&mut self, node: &VarDecl) -> Result<(), Diagnostic> {
        self.newline();

        let var_type = match node.var_type {
            VariableType::Var => "VAR",
            VariableType::VarTemp => "VAR_TEMP",
            VariableType::VarStat => "VAR_STAT",
            VariableType::VarInst => "VAR_INST",
            VariableType::VarGeneric => "VAR_GENERIC",
            VariableType::Input => "VAR_INPUT",
            VariableType::Output => "VAR_OUTPUT",
            VariableType::InOut => "VAR_IN_OUT",
            VariableType::External => "VAR_EXTERNAL",
            VariableType::Global => "VAR_GLOBAL",
            VariableType::Access => "VAR_ACCESS",
        };
        self.write_declaration_keyword(node, var_type);

        match node.qualifier {
            DeclarationQualifier::Unspecified => {}
            DeclarationQualifier::Constant => self.write_ws("CONSTANT"),
            DeclarationQualifier::Retain => self.write_ws("RETAIN"),
            DeclarationQualifier::NonRetain => self.write_ws("NON_RETAIN"),
            DeclarationQualifier::Persistent => self.write_ws("PERSISTENT"),
        }

        self.newline();

        self.indent();
        match &node.identifier {
            VariableIdentifier::Symbol(id) => {
                self.visit_id(id)?;
            }
            VariableIdentifier::Direct(direct) => {
                self.visit_direct_variable_identifier(direct)?;
            }
        }

        self.write_ws(":");
        self.visit_initial_value_assignment_kind(&node.initializer)?;

        if let Some(edge) = node.edge {
            self.write_ws(match edge {
                EdgeDirection::Rising => "R_EDGE",
                EdgeDirection::Falling => "F_EDGE",
            });
        }

        self.write(";");
        self.newline();
        self.outdent();

        self.write_ws("END_VAR");
        self.newline();
        Ok(())
    }

    // 2.4.3.1
    pub(super) fn render_address_assignment(
        &mut self,
        node: &AddressAssignment,
    ) -> Result<(), Diagnostic> {
        let mut address = String::from("%");

        let loc = match &node.location {
            LocationPrefix::I => 'I',
            LocationPrefix::Q => 'Q',
            LocationPrefix::M => 'M',
        };
        address.push(loc);

        let size = match &node.size {
            SizePrefix::Unspecified => "*",
            SizePrefix::Nil => "",
            SizePrefix::X => "X",
            SizePrefix::B => "B",
            SizePrefix::W => "W",
            SizePrefix::D => "D",
            SizePrefix::L => "L",
        };
        address.push_str(size);

        let location: String = node
            .address
            .iter()
            .map(|&id| id.to_string() + ".")
            .collect();
        address.push_str(location.trim_end_matches('.'));

        self.write_ws(address.as_str());

        Ok(())
    }

    // 2.4.3.2
    pub(super) fn render_simple_initializer(
        &mut self,
        node: &SimpleInitializer,
    ) -> Result<(), Diagnostic> {
        self.visit_type_name(&node.type_name)?;

        if let Some(iv) = &node.initial_value {
            self.write_ws(":=");
            self.visit_constant_kind(iv)?;
        }

        Ok(())
    }

    // 2.4.3.1 and 2.4.3.2
    pub(super) fn render_string_initializer(
        &mut self,
        node: &StringInitializer,
    ) -> Result<(), Diagnostic> {
        self.write_ws(node.width.keyword());

        if let Some(len) = &node.length {
            self.write_ws("[");
            self.visit_integer(len.as_integer().ok_or_else(Diagnostic::internal_error)?)?;
            self.write_ws("]");
        }

        if let Some(init) = &node.initial_value {
            self.write_ws(":=");
            self.write(&character_string_text(&init.width, &init.value));
        }

        Ok(())
    }

    // 2.4.3.2
    pub(super) fn render_enumerated_values_initializer(
        &mut self,
        node: &EnumeratedValuesInitializer,
    ) -> Result<(), Diagnostic> {
        self.write_ws("(");
        visit_comma_separated!(self, node.values.iter(), EnumeratedValue);
        self.write_ws(")");

        if let Some(init) = &node.initial_value {
            self.write_ws(":=");

            self.visit_enumerated_value(init)?;
        }

        Ok(())
    }

    // 2.4.3.2
    pub(super) fn render_array_initial_value_assignment(
        &mut self,
        node: &ArrayInitialValueAssignment,
    ) -> Result<(), Diagnostic> {
        self.visit_array_specification_kind(&node.spec)?;

        if !node.initial_values.is_empty() {
            self.write_ws(":=");

            self.write_ws("[");
            visit_comma_separated!(self, node.initial_values.iter(), ArrayInitialElementKind);
            self.write_ws("]");
        }

        Ok(())
    }

    pub(super) fn render_enumerated_initial_value_assignment(
        &mut self,
        node: &EnumeratedInitialValueAssignment,
    ) -> Result<(), Diagnostic> {
        self.visit_type_name(&node.type_name)?;

        if let Some(init) = &node.initial_value {
            self.write_ws(":=");
            self.visit_enumerated_value(init)?;
        }

        Ok(())
    }

    pub(super) fn render_direct_variable_identifier(
        &mut self,
        node: &DirectVariableIdentifier,
    ) -> Result<(), Diagnostic> {
        if let Some(name) = &node.name {
            self.visit_id(name)?;
        }

        self.write_ws("AT");

        self.visit_address_assignment(&node.address_assignment)?;

        Ok(())
    }
}
