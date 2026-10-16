//! Renders sequential function chart networks, steps, transitions and actions.

use super::{LibraryRenderer, Render};
use ironplc_dsl::sfc::ActionQualifier;
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
        r.write_ws("TRANSITION");

        if let Some(name) = &self.name {
            r.visit_id(name)?;
        }
        if let Some(priority) = self.priority {
            r.write_ws("(");
            r.write_ws("PRIORITY");
            r.write_ws(":=");
            r.write_ws(priority.to_string().as_str());
            r.write_ws(")");
        }

        r.write_ws("FROM");

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
            let (word, time) = match qualifier {
                ActionQualifier::N => ("N", None),
                ActionQualifier::R => ("R", None),
                ActionQualifier::S => ("S", None),
                ActionQualifier::L => ("L", None),
                ActionQualifier::D => ("D", None),
                ActionQualifier::P => ("P", None),
                ActionQualifier::SD(time) => ("SD", Some(time)),
                ActionQualifier::DS(time) => ("DS", Some(time)),
                ActionQualifier::SL(time) => ("SL", Some(time)),
                ActionQualifier::PR(time) => ("P1", Some(time)),
                ActionQualifier::PF(time) => ("P0", Some(time)),
            };
            r.write_ws(word);
            if let Some(time) = time {
                r.write_ws(",");
                r.visit_action_time_kind(time)?;
            }
            if !self.indicators.is_empty() {
                r.write_ws(",");
            }
        }

        visit_comma_separated!(r, self.indicators.iter(), Id);
        r.write_ws(");");

        Ok(())
    }
}
