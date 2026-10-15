//! Renders the ways a variable is accessed: dereference, subscript, field and partial access.

use super::LibraryRenderer;
use ironplc_dsl::{diagnostic::Diagnostic, visitor::Visitor};
use paste::paste;

impl LibraryRenderer {
    pub(super) fn render_array_variable(
        &mut self,
        node: &dsl::textual::ArrayVariable,
    ) -> Result<(), Diagnostic> {
        self.visit_symbolic_variable_kind(&node.subscripted_variable)?;

        // No space before `[`: the tight spelling is the canonical output,
        // not a parser constraint -- `symbolic_variable` has admitted a gap
        // before a subscript since #1437. Inside the brackets is fine too,
        // and there `subscript_list` is what allows it.
        self.write("[");
        visit_comma_separated!(self, node.subscripts.iter(), Expr);
        self.write_ws("]");

        Ok(())
    }
}
