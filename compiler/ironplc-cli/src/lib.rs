extern crate ironplc_dsl;
extern crate ironplc_parser;

pub mod cli;
pub mod logger;
pub mod lsp;
mod lsp_client;
pub mod lsp_project;
pub mod lsp_runner;
mod semantic_tokens;

#[cfg(test)]
mod lsp_stack_budget_tests;

#[cfg(test)]
mod test_helpers;

// Spec conformance testing infrastructure (test-only).
#[cfg(test)]
mod spec_requirements {
    include!(concat!(env!("OUT_DIR"), "/spec_requirements.rs"));
}

#[cfg(test)]
#[test]
fn all_spec_requirements_have_tests() {
    assert!(
        spec_requirements::UNTESTED.is_empty(),
        "Requirements in spec with no conformance test: {:?}",
        spec_requirements::UNTESTED
    );
}

#[cfg(test)]
#[ctor::ctor(unsafe)]
#[expect(
    clippy::let_underscore_must_use,
    reason = "try_init fails only when a logger is already installed, which is what the tests want to keep"
)]
fn init_test_logger() {
    let _ = env_logger::builder()
        .is_test(true)
        .filter_level(log::LevelFilter::Trace)
        .try_init();
}
