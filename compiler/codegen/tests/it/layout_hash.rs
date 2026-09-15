//! Layout-hash and type-section variable-table tests (hot-edit P0, T2/T3).
//!
//! The variable table must carry exactly one entry per compiler-assigned
//! variable index, derived from the declarations alone. That is what makes
//! the layout hash (see "Layout Hash and Online Change" in
//! `specs/design/bytecode-container-format.md`) identical for a logic-only
//! edit and different for any declaration change.

use std::collections::HashSet;

use crate::common::parse_and_compile;
use ironplc_container::{FieldType, VAR_FLAG_IS_ARRAY};
use ironplc_parser::options::CompilerOptions;
use rstest::rstest;

/// A `PROGRAM main` with the given declaration list and body.
fn program(declarations: &str, body: &str) -> String {
    format!(
        "PROGRAM main
  VAR
    {declarations}
  END_VAR
  {body}
END_PROGRAM
"
    )
}

/// A program with one DINT variable `x` and the given body.
fn dint_program(body: &str) -> String {
    program("x : DINT;", body)
}

/// A program that instantiates `doubler` so the FB declaration is reachable.
/// `fields` is the FB's declaration list.
fn fb_program(fields: &str) -> String {
    format!(
        "FUNCTION_BLOCK doubler
  {fields}
  y := x * 1;
END_FUNCTION_BLOCK

PROGRAM main
  VAR
    inst : doubler;
    result : DINT;
  END_VAR
  inst(x := 7, y => result);
END_PROGRAM
"
    )
}

/// Compiles `source` and returns its layout hash, as `write_to` would store
/// it in the header.
fn layout_hash(source: &str) -> [u8; 32] {
    parse_and_compile(source, &CompilerOptions::default()).compute_layout_hash()
}

#[test]
fn layout_hash_when_same_source_compiled_twice_then_identical_and_nonzero() {
    let source = dint_program("x := x + 1;");

    let first = layout_hash(&source);
    let second = layout_hash(&source);

    assert_eq!(first, second);
    assert_ne!(first, [0u8; 32]);
}

#[test]
fn layout_hash_when_container_written_then_header_stores_computed_hash() {
    let container = parse_and_compile(&dint_program("x := x + 1;"), &CompilerOptions::default());

    let mut bytes = Vec::new();
    container.write_to(&mut bytes).unwrap();
    let decoded =
        ironplc_container::Container::read_from(&mut std::io::Cursor::new(&bytes)).unwrap();

    assert_eq!(decoded.header.layout_hash, decoded.compute_layout_hash());
    assert_ne!(decoded.header.layout_hash, [0u8; 32]);
}

#[rstest]
#[case::constant_changed("x := x + 1;", "x := x + 10;")]
#[case::control_flow_changed("x := x + 1;", "IF x > 0 THEN x := x + 1; END_IF;")]
fn layout_hash_when_logic_only_edit_then_identical(#[case] before: &str, #[case] after: &str) {
    let before_hash = layout_hash(&dint_program(before));
    let after_hash = layout_hash(&dint_program(after));

    assert_eq!(before_hash, after_hash);
}

#[rstest]
#[case::variable_added("x : DINT;", "x : DINT; y : DINT;")]
#[case::variable_removed("x : DINT; y : DINT;", "x : DINT;")]
fn layout_hash_when_variable_set_changes_then_differs(#[case] before: &str, #[case] after: &str) {
    let before_hash = layout_hash(&program(before, "x := 1;"));
    let after_hash = layout_hash(&program(after, "x := 1;"));

    assert_ne!(before_hash, after_hash);
}

#[test]
fn layout_hash_when_variable_retyped_then_differs() {
    let integral = layout_hash(&dint_program("x := x;"));
    let real = layout_hash(&program("x : REAL;", "x := x;"));

    assert_ne!(integral, real);
}

#[test]
fn layout_hash_when_array_bound_changes_then_differs() {
    let four = layout_hash(&program("a : ARRAY[1..4] OF INT;", "a[1] := 1;"));
    let five = layout_hash(&program("a : ARRAY[1..5] OF INT;", "a[1] := 1;"));

    assert_ne!(four, five);
}

#[test]
fn layout_hash_when_fb_field_added_then_differs() {
    let two_fields = fb_program("VAR_INPUT x : DINT; END_VAR VAR_OUTPUT y : DINT; END_VAR");
    let three_fields = fb_program(
        "VAR_INPUT x : DINT; END_VAR VAR_OUTPUT y : DINT; END_VAR VAR z : DINT; END_VAR",
    );

    assert_ne!(layout_hash(&two_fields), layout_hash(&three_fields));
}

/// Covers every allocation site the variable-table collection knows about:
/// globals, program variables, a user FB instance, a user function's
/// parameter/local/return slots, an array and a string.
const COVERAGE_SOURCE: &str = "
FUNCTION add_one : DINT
  VAR_INPUT v : DINT; END_VAR
  VAR t : DINT; END_VAR
  add_one := v + t;
END_FUNCTION

FUNCTION_BLOCK counter_fb
  VAR_INPUT step : DINT; END_VAR
  VAR_OUTPUT total : DINT; END_VAR
  VAR inner : DINT; END_VAR
  inner := inner + step;
  total := inner;
END_FUNCTION_BLOCK

VAR_GLOBAL
  g : DINT;
END_VAR

PROGRAM main
  VAR
    counter : DINT;
    flag : BOOL;
    text : STRING[80];
    values : ARRAY[1..4] OF INT;
    cfb : counter_fb;
    result : DINT;
  END_VAR
  counter := add_one(counter);
  cfb(step := counter, total => result);
  flag := result > 0;
  text := 'abc';
  values[1] := 7;
  g := result;
END_PROGRAM
";

#[test]
fn variable_table_when_every_allocation_kind_then_one_entry_per_variable_index() {
    let container = parse_and_compile(COVERAGE_SOURCE, &CompilerOptions::default());
    let type_section = container.type_section.as_ref().unwrap();
    let debug = container.debug_section.as_ref().unwrap();

    // One entry per index 0..num_variables: the emitter's
    // `collect_variable_table` fails the compilation rather than leaving a
    // gap, so a shorter table here would already have been an error.
    assert_eq!(
        type_section.variable_table.len(),
        container.header.num_variables as usize
    );

    // Every index belongs to a named variable in this source (no scratch
    // slots), and every declared variable got a real type -- never the
    // unknown-slot placeholder.
    let named: HashSet<u16> = debug
        .var_names
        .iter()
        .map(|entry| entry.var_index.raw())
        .collect();
    assert_eq!(named.len(), type_section.variable_table.len());
    for (index, entry) in type_section.variable_table.iter().enumerate() {
        assert!(
            named.contains(&(index as u16)),
            "variable index {index} has no debug name"
        );
        assert_ne!(
            entry.var_type,
            FieldType::Slot,
            "declared variable at index {index} classified as unknown"
        );
    }
}

#[test]
fn variable_table_when_fb_has_method_then_over_reserved_slots_are_entries() {
    // A method's frame spans its type's field region as well as its own
    // params/locals/return, so `compile_user_fb_methods` advances the
    // variable offset past `field_var_off` again for each method. Those
    // over-reserved slots are never read; the variable table still needs an
    // entry for each, recorded as compiler scratch.
    let source = "
FUNCTION_BLOCK FB_Doubler
VAR
    nLast : DINT;
END_VAR
METHOD Double : DINT
VAR_INPUT
    nIn : DINT;
END_VAR
    Double := nIn * 2;
    nLast := Double;
END_METHOD
END_FUNCTION_BLOCK

PROGRAM main
VAR
    d : FB_Doubler;
    x : DINT;
END_VAR
d.Double(21);
x := d.nLast;
END_PROGRAM
";
    let options = CompilerOptions {
        allow_fb_inheritance: true,
        ..CompilerOptions::default()
    };
    let container = parse_and_compile(source, &options);
    let type_section = container.type_section.as_ref().unwrap();
    let debug = container.debug_section.as_ref().unwrap();

    assert_eq!(
        type_section.variable_table.len(),
        container.header.num_variables as usize
    );

    let named: HashSet<u16> = debug
        .var_names
        .iter()
        .map(|entry| entry.var_index.raw())
        .collect();
    let unnamed: Vec<usize> = (0..type_section.variable_table.len())
        .filter(|index| !named.contains(&(*index as u16)))
        .collect();
    // Method params and return slots carry no debug name (unlike a
    // function's), so the unnamed slots are the parameter, the return slot,
    // and the over-reserved one.
    assert_eq!(unnamed.len(), 3, "unnamed slots: {unnamed:?}");
    assert_eq!(
        type_section.variable_table[unnamed[0]].var_type,
        FieldType::I32
    );
    assert_eq!(
        type_section.variable_table[unnamed[1]].var_type,
        FieldType::I32
    );
    assert_eq!(
        type_section.variable_table[unnamed[2]].var_type,
        FieldType::Slot
    );
}

#[test]
fn variable_table_when_every_allocation_kind_then_entries_describe_declarations() {
    let container = parse_and_compile(COVERAGE_SOURCE, &CompilerOptions::default());
    let type_section = container.type_section.as_ref().unwrap();
    let debug = container.debug_section.as_ref().unwrap();

    let index_of = |name: &str| -> usize {
        debug
            .var_names
            .iter()
            .find(|entry| entry.name.eq_ignore_ascii_case(name))
            .unwrap()
            .var_index
            .raw() as usize
    };
    let entry = |name: &str| -> &ironplc_container::VarEntry {
        &type_section.variable_table[index_of(name)]
    };

    // Scalars, program locals, function locals and FB fields.
    assert_eq!(entry("g").var_type, FieldType::I32);
    assert_eq!(entry("counter").var_type, FieldType::I32);
    assert_eq!(entry("flag").var_type, FieldType::I32);
    assert_eq!(entry("v").var_type, FieldType::I32);
    assert_eq!(entry("t").var_type, FieldType::I32);
    assert_eq!(entry("step").var_type, FieldType::I32);
    assert_eq!(entry("inner").var_type, FieldType::I32);

    // STRING: max length in `extra`.
    assert_eq!(entry("text").var_type, FieldType::String);
    assert_eq!(entry("text").extra, 80);

    // Array: element type, array flag, descriptor index in `extra`.
    assert_eq!(entry("values").var_type, FieldType::I32);
    assert_eq!(entry("values").flags, VAR_FLAG_IS_ARRAY);
    assert!((entry("values").extra as usize) < type_section.array_descriptors.len());

    // FB instance: its `extra` names a registered user FB type.
    assert_eq!(entry("cfb").var_type, FieldType::FbInstance);
    assert!(type_section
        .user_fb_types
        .iter()
        .any(|desc| desc.type_id.raw() == entry("cfb").extra));
}
