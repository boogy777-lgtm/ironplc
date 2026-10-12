extern crate ironplc_dsl as dsl;

use ironplc_dsl::{common::Library, diagnostic::Diagnostic, stack::within_stack_budget};
use renderer::apply;

mod renderer;
#[cfg(test)]
mod tests;

// Spec conformance testing infrastructure (test-only).
#[cfg(test)]
mod spec_requirements {
    include!(concat!(env!("OUT_DIR"), "/spec_requirements.rs"));
}
#[cfg(test)]
mod spec_conformance;
#[cfg(test)]
mod spec_conformance_pointer_to;
#[cfg(test)]
mod spec_conformance_string_literals;

/// Renders the library as text, on the stack budget (`ironplc_dsl::stack`).
pub fn write_to_string(lib: &Library) -> Result<String, Vec<Diagnostic>> {
    within_stack_budget(|| apply(lib))
}
