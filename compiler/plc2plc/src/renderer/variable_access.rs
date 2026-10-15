//! Renders the ways a variable is accessed: dereference, subscript, field and partial access.

use super::{LibraryRenderer, Render};
use ironplc_dsl::{diagnostic::Diagnostic, visitor::Visitor};
use paste::paste;

impl Render for dsl::textual::ArrayVariable {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.visit_symbolic_variable_kind(&self.subscripted_variable)?;

        // No space before `[`: the tight spelling is the canonical output,
        // not a parser constraint -- `symbolic_variable` has admitted a gap
        // before a subscript since #1437. Inside the brackets is fine too,
        // and there `subscript_list` is what allows it.
        r.write("[");
        visit_comma_separated!(r, self.subscripts.iter(), Expr);
        r.write_ws("]");

        Ok(())
    }
}
