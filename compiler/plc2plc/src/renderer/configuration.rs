//! Renders configurations, resources, tasks and the program and function block instances they hold.

use super::{LibraryRenderer, Render};
use dsl::configuration::LocatedVarInit;
use ironplc_dsl::common::*;
use ironplc_dsl::{diagnostic::Diagnostic, visitor::Visitor};
use paste::paste;

impl LibraryRenderer {
    fn render_data_source_kind(
        &mut self,
        source: &dsl::configuration::DataSourceKind,
    ) -> Result<(), Diagnostic> {
        match source {
            dsl::configuration::DataSourceKind::Constant(constant) => {
                self.visit_constant_kind(constant)?;
            }
            dsl::configuration::DataSourceKind::GlobalVarReference(gvr) => {
                if let Some(resource_name) = &gvr.resource_name {
                    self.visit_id(resource_name)?;
                    self.write(".");
                }
                self.visit_id(&gvr.global_var_name)?;
                if let Some(structure_element) = &gvr.structure_element_name {
                    self.write(".");
                    self.visit_id(structure_element)?;
                }
            }
        }
        Ok(())
    }
}

// 2.7.1
impl Render for dsl::configuration::ResourceDeclaration {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.write_ws("RESOURCE");
        r.visit_id(&self.name)?;
        r.write_ws("ON");
        r.visit_id(&self.resource)?;
        r.newline();

        r.indent();
        // The grammar puts the resource's global variables before its tasks
        // and programs; after them, they do not parse.
        for var in self.global_vars.iter() {
            r.visit_var_decl(var)?;
        }

        for task in self.tasks.iter() {
            r.visit_task_configuration(task)?;
        }

        for program in self.programs.iter() {
            r.visit_program_configuration(program)?;
        }

        r.outdent();

        r.write_ws("END_RESOURCE");
        r.newline();
        Ok(())
    }
}

// 2.7.2
impl Render for dsl::configuration::ProgramConfiguration {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.write_ws("PROGRAM");

        if let Some(storage) = &self.storage {
            let storage = match storage {
                DeclarationQualifier::Unspecified => "",
                DeclarationQualifier::Constant => "",
                DeclarationQualifier::Retain => "RETAIN",
                DeclarationQualifier::NonRetain => "NON_RETAIN",
                // Unreachable via the grammar: `program_configuration()`'s
                // `storage` is only ever produced from RETAIN/NON_RETAIN.
                // Exhaustive match still needs an arm.
                DeclarationQualifier::Persistent => "",
            };
            r.write_ws(storage);
        }

        r.visit_id(&self.name)?;

        if let Some(task) = &self.task_name {
            r.write_ws("WITH");
            r.visit_id(task)?;
        }

        r.write_ws(":");
        r.visit_id(&self.type_name)?;

        r.write_ws(";");
        r.newline();

        Ok(())
    }
}

// 2.7.2
impl Render for dsl::configuration::ConfigurationDeclaration {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.write_ws("CONFIGURATION");
        r.visit_id(&self.name)?;
        r.newline();

        r.indent();
        // The grammar puts the configuration's global variables before its
        // resources.
        for var in self.global_var.iter() {
            r.visit_var_decl(var)?;
        }

        for res in self.resource_decl.iter() {
            r.visit_resource_declaration(res)?;
        }
        r.outdent();

        // A configuration has a single VAR_CONFIG block holding every
        // instance-specific initialization.
        if !self.fb_inits.is_empty() || !self.located_var_inits.is_empty() {
            r.indent();
            r.write_ws("VAR_CONFIG");
            r.newline();

            r.indent();
            for fb_init in self.fb_inits.iter() {
                r.visit_function_block_init(fb_init)?;
            }
            for loc_var_init in self.located_var_inits.iter() {
                r.visit_located_var_init(loc_var_init)?;
            }
            r.outdent();

            r.write_ws("END_VAR");
            r.newline();
            r.outdent();
        }

        r.newline();

        r.write_ws("END_CONFIGURATION");
        r.newline();
        Ok(())
    }
}

/// Renders one initializer line; the enclosing `VAR_CONFIG` block is
/// written once by `visit_configuration_declaration`.
impl Render for dsl::configuration::FunctionBlockInit {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        let mut vars: Vec<&String> = Vec::new();
        vars.push(self.resource_name.original());
        vars.push(self.program_name.original());
        vars.extend(self.fb_path.iter().map(|v| v.original()));

        write_period_separated!(r, vars.iter());

        r.write_ws(":");

        r.visit_type_name(&self.type_name)?;

        r.write_ws(":=");

        r.write_ws("(");

        visit_comma_separated!(r, self.initializer.iter(), StructureElementInit);

        r.write_ws(")");
        r.write_ws(";");
        r.newline();

        Ok(())
    }
}

/// Renders one initializer line; see `visit_function_block_init`.
impl Render for LocatedVarInit {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        let mut vars: Vec<&String> = Vec::new();
        vars.push(self.resource_name.original());
        vars.push(self.program_name.original());
        vars.extend(self.fb_path.iter().map(|v| v.original()));

        write_period_separated!(r, vars.iter());

        if let Some(loc) = &self.address {
            r.write_ws("AT");
            r.visit_address_assignment(loc)?;
        }

        r.write_ws(":");

        r.visit_initial_value_assignment_kind(&self.initializer)?;

        r.write_ws(";");
        r.newline();

        Ok(())
    }
}

// 2.7.1
impl Render for dsl::configuration::TaskConfiguration {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.write_ws("TASK");

        r.visit_id(&self.name)?;
        r.write_ws("(");

        if let Some(single) = &self.single {
            r.write_ws("SINGLE");
            r.write_ws(":=");
            r.render_data_source_kind(single)?;
            r.write_ws(",");
        }

        if let Some(interval) = &self.interval {
            r.write_ws("INTERVAL");
            r.write_ws(":=");
            r.visit_duration_literal(interval)?;
            r.write_ws(",");
        }

        r.write_ws("PRIORITY");
        r.write_ws(":=");
        r.write_ws(self.priority.to_string().as_str());

        r.write_ws(")");

        r.write_ws(";");
        r.newline();
        Ok(())
    }
}
