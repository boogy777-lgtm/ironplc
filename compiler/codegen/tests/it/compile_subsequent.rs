//! Tests that the variables of a program and of a global variable list with
//! the attribute `subsequent` are allocated one after another, in the order
//! they are declared.

use crate::common::parse_and_compile;
use ironplc_container::Container;
use ironplc_parser::options::{CompilerOptions, Dialect};
use spec_test_macro::spec_test;

/// The variable-table indices of the variables named, in the order named.
fn var_indices(container: &Container, names: &[&str]) -> Vec<u16> {
    let debug = container.debug_section.as_ref().unwrap();
    names
        .iter()
        .map(|name| {
            debug
                .var_names
                .iter()
                .find(|entry| entry.name.eq_ignore_ascii_case(name))
                .unwrap()
                .var_index
                .raw()
        })
        .collect()
}

#[test]
#[spec_test(REQ_ATTR_codegen_003)]
fn compile_when_program_has_subsequent_then_variables_have_consecutive_indices_in_declaration_order(
) {
    let source = "
{attribute 'subsequent'}
PROGRAM main
  VAR
    first : DINT;
    second : INT;
    third : BOOL;
    fourth : LREAL;
  END_VAR
  first := 1;
END_PROGRAM
";

    let container = parse_and_compile(source, &CompilerOptions::from_dialect(Dialect::Codesys));

    let indices = var_indices(&container, &["first", "second", "third", "fourth"]);
    assert_eq!(indices, vec![0, 1, 2, 3]);
}

#[test]
#[spec_test(REQ_ATTR_codegen_003)]
fn compile_when_global_list_has_subsequent_then_variables_have_consecutive_indices_in_declaration_order(
) {
    let source = "
{attribute 'subsequent'}
VAR_GLOBAL
  g_first : DINT;
  g_second : INT;
  g_third : BOOL;
END_VAR

PROGRAM main
  VAR_EXTERNAL
    g_first : DINT;
    g_second : INT;
    g_third : BOOL;
  END_VAR
  g_first := 1;
END_PROGRAM
";

    let container = parse_and_compile(source, &CompilerOptions::from_dialect(Dialect::Codesys));

    let indices = var_indices(&container, &["g_first", "g_second", "g_third"]);
    assert_eq!(indices[1], indices[0] + 1);
    assert_eq!(indices[2], indices[1] + 1);
}
