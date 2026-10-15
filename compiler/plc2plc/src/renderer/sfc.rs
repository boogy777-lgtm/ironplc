//! Renders sequential function chart networks, steps, transitions and actions.

use super::{LibraryRenderer, Render};
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
}

// 2.6.2
impl Render for dsl::sfc::Network {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.write_ws("INITIAL_STEP");
        r.write_step_body(&self.initial_step)?;
        r.newline();

        for elem in self.elements.iter() {
            r.visit_element_kind(elem)?;
            r.newline();
        }
        Ok(())
    }
}

// 2.6.3
impl Render for dsl::sfc::Transition {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.write_ws("TRANSITION FROM");

        visit_comma_separated!(r, self.from.iter(), Id);

        r.write_ws("TO");

        visit_comma_separated!(r, self.to.iter(), Id);
        r.newline();

        r.indent();
        r.write_ws(":=");
        r.visit_expr(&self.condition)?;
        r.write_ws(";");
        r.newline();
        r.outdent();

        r.write_ws("END_TRANSITION");
        r.newline();
        Ok(())
    }
}

// 2.6.3
impl Render for dsl::sfc::Action {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.write_declaration_keyword(self, "ACTION");

        r.visit_id(&self.name)?;
        r.write_ws(":");
        r.newline();

        r.indent();
        r.visit_function_block_body_kind(&self.body)?;
        r.outdent();

        r.write_ws("END_ACTION");
        r.newline();
        Ok(())
    }
}

// 2.6.4
impl Render for dsl::sfc::ActionAssociation {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.visit_id(&self.name)?;

        r.write_ws("(");

        if let Some(qualifier) = &self.qualifier {
            r.write_ws(qualifier.to_string().as_str());
            if !self.indicators.is_empty() {
                r.write_ws(",");
            }
        }

        visit_comma_separated!(r, self.indicators.iter(), Id);
        r.write_ws(");");

        Ok(())
    }
}
