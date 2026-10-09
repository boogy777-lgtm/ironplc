//! The semantic rules, as a table, and the stage that runs them.
//!
//! A rule is a function of the resolved library: it reads the library and the
//! context the type resolution built and reports what it found. The table holds
//! every rule once, under the name of its module, which the macro takes from the
//! identifier it is given, so that the name an observer reports is the module
//! that does the work and no second list names the rules. A test ties the table
//! to the rule modules of the crate, so that a rule module that is not in it
//! fails the build instead of never running.

use ironplc_parser::options::CompilerOptions;

use crate::{
    observe::{self, Observer},
    result::SemanticResult,
    semantic_context::SemanticContext,
};
use ironplc_dsl::common::Library;

/// What every rule is: a function of the resolved library.
pub(crate) type Rule = fn(&Library, &SemanticContext, &CompilerOptions) -> SemanticResult;

/// The table of rules from the modules named, each as its name and its `apply`.
macro_rules! rules {
    ($($rule:ident),+ $(,)?) => {
        &[$((stringify!($rule), crate::$rule::apply as Rule)),+]
    };
}

/// Every semantic rule, in the order they run.
pub(crate) static RULES: &[(&str, Rule)] = rules![
    rule_abstract_not_instantiated,
    rule_ambiguous_enumerated_value,
    rule_assignment_aggregate_type_compat,
    rule_decl_struct_element_unique_names,
    rule_range_limits,
    rule_real_literal_range,
    rule_enum_base_type_allowed,
    rule_enum_explicit_value_allowed,
    rule_enumeration_values_unique,
    rule_loop_control_inside_loop,
    rule_jump_target,
    rule_extends_field_duplicated,
    rule_function_block_call_unsupported,
    rule_function_block_invocation,
    rule_function_call_declared,
    rule_function_call_in_out_argument,
    rule_function_call_type_check,
    rule_member_qualifier_allowed,
    rule_member_qualifier_invalid,
    rule_method_call_declared,
    rule_program_task_definition_exists,
    rule_program_var_hides_global,
    rule_no_top_level_var_global,
    rule_operator_operand_type_check,
    rule_task_names_unique,
    rule_stdlib_type_redefinition,
    rule_string_encoding_compat,
    rule_string_length_range,
    rule_string_literal_char_range,
    rule_temporal_literal_range,
    rule_struct_initializer_expression_allowed,
    rule_fb_instance_array_allowed,
    rule_use_declared_enumerated_value,
    rule_use_declared_symbolic_var,
    rule_unsupported_extension,
    rule_var_decl_const_initialized,
    rule_var_decl_const_not_fb,
    rule_var_decl_initializer_type_compat,
    rule_var_decl_global_const_requires_external_const,
    rule_mixed_located_var_declarations,
    rule_pou_hierarchy,
    rule_bit_and_partial_access_range,
    rule_case_bit_string_label,
    rule_case_selector_type,
    rule_condition_type,
    rule_constant_range,
    rule_ref_to,
    rule_special_operator,
];

/// Semantic implements semantic analysis (stage 3).
///
/// Returns `Ok(())` if the library is free of semantic errors.
/// Returns `Err(String)` if the library contains a semantic error.
pub(crate) fn semantic<O: Observer>(
    library: &Library,
    context: &SemanticContext,
    options: &CompilerOptions,
    observer: &O,
) -> SemanticResult {
    let mut all_diagnostics = vec![];
    for (name, rule) in RULES {
        match observer.observe(observe::rule(name), || rule(library, context, options)) {
            Ok(_) => {
                // Nothing to do here
            }
            Err(diagnostics) => {
                all_diagnostics.extend(diagnostics);
            }
        }
    }

    if !all_diagnostics.is_empty() {
        return Err(all_diagnostics);
    }

    Ok(())
}
