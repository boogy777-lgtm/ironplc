//! Renders variable declarations, direct addresses and the initial values that follow a variable.

use super::text::character_string_text;
use super::{LibraryRenderer, Render};
use ironplc_dsl::common::*;
use ironplc_dsl::{diagnostic::Diagnostic, visitor::Visitor};
use paste::paste;

impl Render for ProgramAccessDecl {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.newline();

        r.write_ws("VAR_ACCESS");

        r.newline();
        r.indent();

        r.visit_id(&self.access_name)?;
        r.write_ws(":");
        r.visit_symbolic_variable_kind(&self.symbolic_variable)?;
        r.write_ws(":");
        r.visit_type_name(&self.type_name)?;

        if let Some(dir) = &self.direction {
            let dir = match dir {
                dsl::configuration::Direction::ReadWrite => "READ_WRITE",
                dsl::configuration::Direction::ReadOnly => "READ_ONLY",
            };
            r.write_ws(dir);
        }

        r.write(";");
        r.newline();
        r.outdent();

        r.write_ws("END_VAR");
        r.newline();
        Ok(())
    }
}

// 2.4.3
impl Render for VarDecl {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.newline();

        let var_type = match self.var_type {
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
        r.write_declaration_keyword(self, var_type);

        match self.qualifier {
            DeclarationQualifier::Unspecified => {}
            DeclarationQualifier::Constant => r.write_ws("CONSTANT"),
            DeclarationQualifier::Retain => r.write_ws("RETAIN"),
            DeclarationQualifier::NonRetain => r.write_ws("NON_RETAIN"),
            DeclarationQualifier::Persistent => r.write_ws("PERSISTENT"),
        }

        r.newline();

        r.indent();
        match &self.identifier {
            VariableIdentifier::Symbol(id) => {
                r.visit_id(id)?;
            }
            VariableIdentifier::Direct(direct) => {
                r.visit_direct_variable_identifier(direct)?;
            }
        }

        r.write_ws(":");
        r.visit_initial_value_assignment_kind(&self.initializer)?;

        if let Some(edge) = self.edge {
            r.write_ws(match edge {
                EdgeDirection::Rising => "R_EDGE",
                EdgeDirection::Falling => "F_EDGE",
            });
        }

        r.write(";");
        r.newline();
        r.outdent();

        r.write_ws("END_VAR");
        r.newline();
        Ok(())
    }
}

// 2.4.3.1
impl Render for AddressAssignment {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        let mut address = String::from("%");

        let loc = match &self.location {
            LocationPrefix::I => 'I',
            LocationPrefix::Q => 'Q',
            LocationPrefix::M => 'M',
        };
        address.push(loc);

        let size = match &self.size {
            SizePrefix::Unspecified => "*",
            SizePrefix::Nil => "",
            SizePrefix::X => "X",
            SizePrefix::B => "B",
            SizePrefix::W => "W",
            SizePrefix::D => "D",
            SizePrefix::L => "L",
        };
        address.push_str(size);

        let location: String = self
            .address
            .iter()
            .map(|&id| id.to_string() + ".")
            .collect();
        address.push_str(location.trim_end_matches('.'));

        r.write_ws(address.as_str());

        Ok(())
    }
}

// 2.4.3.2
impl Render for SimpleInitializer {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.visit_type_name(&self.type_name)?;

        if let Some(iv) = &self.initial_value {
            r.write_ws(":=");
            r.visit_constant_kind(iv)?;
        }

        Ok(())
    }
}

// 2.4.3.1 and 2.4.3.2
impl Render for StringInitializer {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.write_ws(self.width.keyword());

        if let Some(len) = &self.length {
            r.write_ws("[");
            r.visit_integer(len.as_integer().ok_or_else(Diagnostic::internal_error)?)?;
            r.write_ws("]");
        }

        if let Some(init) = &self.initial_value {
            r.write_ws(":=");
            r.write(&character_string_text(&init.width, &init.value));
        }

        Ok(())
    }
}

// 2.4.3.2
impl Render for EnumeratedValuesInitializer {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.write_ws("(");
        visit_comma_separated!(r, self.values.iter(), EnumeratedValue);
        r.write_ws(")");

        if let Some(init) = &self.initial_value {
            r.write_ws(":=");

            r.visit_enumerated_value(init)?;
        }

        Ok(())
    }
}

// 2.4.3.2
impl Render for ArrayInitialValueAssignment {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.visit_array_specification_kind(&self.spec)?;

        if !self.initial_values.is_empty() {
            r.write_ws(":=");

            r.write_ws("[");
            visit_comma_separated!(r, self.initial_values.iter(), ArrayInitialElementKind);
            r.write_ws("]");
        }

        Ok(())
    }
}

impl Render for EnumeratedInitialValueAssignment {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.visit_type_name(&self.type_name)?;

        if let Some(init) = &self.initial_value {
            r.write_ws(":=");
            r.visit_enumerated_value(init)?;
        }

        Ok(())
    }
}

impl Render for DirectVariableIdentifier {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        if let Some(name) = &self.name {
            r.visit_id(name)?;
        }

        r.write_ws("AT");

        r.visit_address_assignment(&self.address_assignment)?;

        Ok(())
    }
}
