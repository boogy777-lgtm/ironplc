//! Renders configurations, resources, tasks and the program and function block instances they hold.

use super::LibraryRenderer;
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

    // 2.7.1
    pub(super) fn render_resource_declaration(
        &mut self,
        node: &dsl::configuration::ResourceDeclaration,
    ) -> Result<(), Diagnostic> {
        self.write_ws("RESOURCE");
        self.visit_id(&node.name)?;
        self.write_ws("ON");
        self.visit_id(&node.resource)?;
        self.newline();

        self.indent();
        for task in node.tasks.iter() {
            self.visit_task_configuration(task)?;
        }

        for program in node.programs.iter() {
            self.visit_program_configuration(program)?;
        }

        for var in node.global_vars.iter() {
            self.visit_var_decl(var)?;
        }

        self.outdent();

        self.write_ws("END_RESOURCE");
        self.newline();
        Ok(())
    }

    // 2.7.2
    pub(super) fn render_program_configuration(
        &mut self,
        node: &dsl::configuration::ProgramConfiguration,
    ) -> Result<(), Diagnostic> {
        self.write_ws("PROGRAM");

        if let Some(storage) = &node.storage {
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
            self.write_ws(storage);
        }

        self.visit_id(&node.name)?;

        if let Some(task) = &node.task_name {
            self.write_ws("WITH");
            self.visit_id(task)?;
        }

        self.write_ws(":");
        self.visit_id(&node.type_name)?;

        self.write_ws(";");
        self.newline();

        Ok(())
    }

    // 2.7.2
    pub(super) fn render_configuration_declaration(
        &mut self,
        node: &dsl::configuration::ConfigurationDeclaration,
    ) -> Result<(), Diagnostic> {
        self.write_ws("CONFIGURATION");
        self.visit_id(&node.name)?;
        self.newline();

        self.indent();
        for res in node.resource_decl.iter() {
            self.visit_resource_declaration(res)?;
        }
        self.outdent();

        // A configuration has a single VAR_CONFIG block holding every
        // instance-specific initialization.
        if !node.fb_inits.is_empty() || !node.located_var_inits.is_empty() {
            self.indent();
            self.write_ws("VAR_CONFIG");
            self.newline();

            self.indent();
            for fb_init in node.fb_inits.iter() {
                self.visit_function_block_init(fb_init)?;
            }
            for loc_var_init in node.located_var_inits.iter() {
                self.visit_located_var_init(loc_var_init)?;
            }
            self.outdent();

            self.write_ws("END_VAR");
            self.newline();
            self.outdent();
        }

        self.newline();

        self.write_ws("END_CONFIGURATION");
        self.newline();
        Ok(())
    }

    /// Renders one initializer line; the enclosing `VAR_CONFIG` block is
    /// written once by `visit_configuration_declaration`.
    pub(super) fn render_function_block_init(
        &mut self,
        node: &dsl::configuration::FunctionBlockInit,
    ) -> Result<(), Diagnostic> {
        let mut vars: Vec<&String> = Vec::new();
        vars.push(node.resource_name.original());
        vars.push(node.program_name.original());
        vars.extend(node.fb_path.iter().map(|v| v.original()));

        write_period_separated!(self, vars.iter());

        self.write_ws(":");

        self.visit_type_name(&node.type_name)?;

        self.write_ws(":=");

        self.write_ws("(");

        visit_comma_separated!(self, node.initializer.iter(), StructureElementInit);

        self.write_ws(")");
        self.write_ws(";");
        self.newline();

        Ok(())
    }

    /// Renders one initializer line; see `visit_function_block_init`.
    pub(super) fn render_located_var_init(
        &mut self,
        node: &LocatedVarInit,
    ) -> Result<(), Diagnostic> {
        let mut vars: Vec<&String> = Vec::new();
        vars.push(node.resource_name.original());
        vars.push(node.program_name.original());
        vars.extend(node.fb_path.iter().map(|v| v.original()));

        write_period_separated!(self, vars.iter());

        if let Some(loc) = &node.address {
            self.write_ws("AT");
            self.visit_address_assignment(loc)?;
        }

        self.write_ws(":");

        self.visit_initial_value_assignment_kind(&node.initializer)?;

        self.write_ws(";");
        self.newline();

        Ok(())
    }

    // 2.7.1
    pub(super) fn render_task_configuration(
        &mut self,
        node: &dsl::configuration::TaskConfiguration,
    ) -> Result<(), Diagnostic> {
        self.write_ws("TASK");

        self.visit_id(&node.name)?;
        self.write_ws("(");

        if let Some(single) = &node.single {
            self.write_ws("SINGLE");
            self.write_ws(":=");
            self.render_data_source_kind(single)?;
            self.write_ws(",");
        }

        if let Some(interval) = &node.interval {
            self.write_ws("INTERVAL");
            self.write_ws(":=");
            self.visit_duration_literal(interval)?;
            self.write_ws(",");
        }

        self.write_ws("PRIORITY");
        self.write_ws(":=");
        self.write_ws(node.priority.to_string().as_str());

        self.write_ws(")");

        self.write_ws(";");
        self.newline();
        Ok(())
    }
}
