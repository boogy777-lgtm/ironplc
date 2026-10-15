//! Renders sequential function chart networks, steps, transitions and actions.

use super::LibraryRenderer;
use ironplc_dsl::{diagnostic::Diagnostic, visitor::Visitor};
use paste::paste;

impl LibraryRenderer {
    /// Renders the `name : associations END_STEP` tail shared by
    /// `INITIAL_STEP` and `STEP`; the caller writes the keyword.
    pub(super) fn write_step_body(&mut self, node: &dsl::sfc::Step) -> Result<(), Diagnostic> {
        self.visit_id(&node.name)?;
        self.write_ws(":");
        self.newline();

        self.indent();
        for elem in node.action_associations.iter() {
            self.visit_action_association(elem)?;
            self.newline();
        }
        self.outdent();

        self.write_ws("END_STEP");
        self.newline();
        Ok(())
    }

    // 2.6.2
    pub(super) fn render_network(&mut self, node: &dsl::sfc::Network) -> Result<(), Diagnostic> {
        self.write_ws("INITIAL_STEP");
        self.write_step_body(&node.initial_step)?;
        self.newline();

        for elem in node.elements.iter() {
            self.visit_element_kind(elem)?;
            self.newline();
        }
        Ok(())
    }

    // 2.6.3
    pub(super) fn render_transition(
        &mut self,
        node: &dsl::sfc::Transition,
    ) -> Result<(), Diagnostic> {
        self.write_ws("TRANSITION FROM");

        visit_comma_separated!(self, node.from.iter(), Id);

        self.write_ws("TO");

        visit_comma_separated!(self, node.to.iter(), Id);
        self.newline();

        self.indent();
        self.write_ws(":=");
        self.visit_expr(&node.condition)?;
        self.write_ws(";");
        self.newline();
        self.outdent();

        self.write_ws("END_TRANSITION");
        self.newline();
        Ok(())
    }

    // 2.6.3
    pub(super) fn render_action(&mut self, node: &dsl::sfc::Action) -> Result<(), Diagnostic> {
        self.write_declaration_keyword(node, "ACTION");

        self.visit_id(&node.name)?;
        self.write_ws(":");
        self.newline();

        self.indent();
        self.visit_function_block_body_kind(&node.body)?;
        self.outdent();

        self.write_ws("END_ACTION");
        self.newline();
        Ok(())
    }

    // 2.6.4
    pub(super) fn render_action_association(
        &mut self,
        node: &dsl::sfc::ActionAssociation,
    ) -> Result<(), Diagnostic> {
        self.visit_id(&node.name)?;

        self.write_ws("(");

        if let Some(qualifier) = &node.qualifier {
            self.write_ws(qualifier.to_string().as_str());
            if !node.indicators.is_empty() {
                self.write_ws(",");
            }
        }

        visit_comma_separated!(self, node.indicators.iter(), Id);
        self.write_ws(");");

        Ok(())
    }
}
