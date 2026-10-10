//! Compiling the way the tool does: with the semantic rules.

use ironplc_codegen::compile;
use ironplc_container::Container;
use ironplc_dsl::core::FileId;
use ironplc_dsl::diagnostic::Diagnostic;
use ironplc_parser::options::CompilerOptions;
use ironplc_parser::parse_program;
use ironplc_vm::VmBuffers;

use super::run_one_scan;

/// Compiles and runs one scan the way `ironplcc compile` followed by
/// `ironplcvm run` do: the semantic rules run as well as the type resolution,
/// so a program the tool refuses is refused here. The refusal, from either
/// stage, is the first diagnostic.
pub fn try_check_and_run(
    source: &str,
    options: &CompilerOptions,
) -> Result<(Container, VmBuffers), Diagnostic> {
    let library = parse_program(source, &FileId::default(), options)?;
    let (library, context) = ironplc_analyzer::stages::analyze(&[&library], options)
        .map_err(|mut found| found.remove(0))?;
    if let Some(first) = context.diagnostics().first() {
        return Err(first.clone());
    }
    let codegen_options = ironplc_codegen::CodegenOptions::from(options);
    let container = compile(
        &library,
        &context,
        &codegen_options,
        &ironplc_codegen::EmptyLookup,
    )?;
    Ok(run_one_scan(container).unwrap())
}
