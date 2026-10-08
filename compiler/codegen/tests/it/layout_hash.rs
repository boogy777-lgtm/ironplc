//! Layout-hash and type-section variable-table tests (hot-edit P0, T2/T3).
//!
//! The variable table must carry exactly one entry per compiler-assigned
//! variable index, derived from the declarations alone. That is what makes
//! the layout hash (see "Layout Hash and Online Change" in
//! `specs/design/bytecode-container-format.md`) identical for a logic-only
//! edit and different for any declaration change.

use std::collections::HashSet;

use crate::common::{parse, parse_and_compile};
use ironplc_container::{
    string_region_size, CharWidth, Container, FieldEntry, FieldType, VarEntry, VAR_FLAG_IS_ARRAY,
};
use ironplc_dsl::common::{DeclarationQualifier, LibraryElementKind, VarDecl};
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

/// The persistent extent is declared at the one point where code generation
/// has assigned the last persistent variable. A function's slots and its
/// string local come after it, in the variable table and in the data region.
#[test]
fn persistent_extent_when_function_has_string_local_then_extent_ends_before_function_data() {
    let source = "
FUNCTION join : DINT
  VAR_INPUT i : DINT; END_VAR
  VAR t : STRING[8]; END_VAR
  join := i + LEN(t);
END_FUNCTION

PROGRAM main
  VAR s : STRING[10]; r : DINT; END_VAR
  r := join(i := 1);
END_PROGRAM
";
    let container = parse_and_compile(source, &CompilerOptions::default());

    let extents = container.persistent_extents();

    assert_eq!(extents.len(), 1);
    // The program's two variables; the function's three slots follow.
    assert_eq!((extents[0].var_start, extents[0].var_count), (0, 2));
    assert_eq!(container.header.num_variables, 5);
    assert_eq!(extents[0].data_start, 0);
    assert_eq!(
        extents[0].data_len,
        string_region_size(10, CharWidth::Narrow)
    );
    assert!(container.header.data_region_bytes > extents[0].data_len);
    assert_eq!(
        container.task_table.shared_globals_size, extents[0].var_count,
        "the task table's shared extent and the declared extent agree for one program"
    );
}

/// The function block type table lists the fields of each user function block
/// in the order the VM copies them in and out: inputs, outputs, then the rest.
#[test]
fn fb_types_when_user_function_blocks_then_each_lists_its_fields_in_slot_order() {
    let source = "
FUNCTION_BLOCK mixer
  VAR text : STRING[8]; END_VAR
  VAR_OUTPUT total : REAL; END_VAR
  VAR_INPUT step : DINT; END_VAR
  total := total + 1.0;
END_FUNCTION_BLOCK

FUNCTION_BLOCK plain
  VAR_INPUT x : DINT; END_VAR
  x := x;
END_FUNCTION_BLOCK

PROGRAM main
  VAR m : mixer; p : plain; END_VAR
  m(step := 1);
  p(x := 1);
END_PROGRAM
";
    let container = parse_and_compile(source, &CompilerOptions::default());
    let section = container.type_section.as_ref().unwrap();

    assert_eq!(section.fb_types.len(), 2);
    assert_eq!(container.header.num_fb_types, 2);
    let field = |field_type, field_extra| FieldEntry {
        field_type,
        field_extra,
    };
    let fields_of = |name: &str| -> Vec<FieldEntry> {
        let debug = container.debug_section.as_ref().unwrap();
        let first = debug
            .var_names
            .iter()
            .find(|entry| entry.name.eq_ignore_ascii_case(name))
            .unwrap()
            .var_index;
        let user = section
            .user_fb_types
            .iter()
            .find(|desc| desc.var_offset == first.raw())
            .unwrap();
        let listed = section
            .fb_types
            .iter()
            .find(|desc| desc.type_id == user.type_id)
            .unwrap();
        assert_eq!(listed.fields.len(), usize::from(user.num_fields));
        listed.fields.clone()
    };
    assert_eq!(
        fields_of("step"),
        vec![
            field(FieldType::I32, 0),
            field(FieldType::F32, 0),
            field(FieldType::String, 8),
        ]
    );
    assert_eq!(fields_of("x"), vec![field(FieldType::I32, 0)]);
    assert!(section.fb_types[0].type_id.raw() < section.fb_types[1].type_id.raw());
}

/// Every part of a container that online change compares to decide whether
/// the running state can carry over to a candidate: the layout hash, the
/// variable count, the header flags, the process-image sizes, the variable
/// table and the task table. A new compared field is added here, once.
#[derive(Debug, PartialEq)]
struct Layout {
    layout_hash: [u8; 32],
    num_variables: u16,
    flags: u8,
    input_image_bytes: u16,
    output_image_bytes: u16,
    memory_image_bytes: u16,
    variable_table: Vec<VarEntry>,
    task_table: Vec<u8>,
}

fn layout_of(container: &Container) -> Layout {
    let mut task_table = Vec::new();
    container.task_table.write_to(&mut task_table).unwrap();
    Layout {
        layout_hash: container.compute_layout_hash(),
        num_variables: container.header.num_variables,
        flags: container.header.flags,
        input_image_bytes: container.header.input_image_bytes,
        output_image_bytes: container.header.output_image_bytes,
        memory_image_bytes: container.header.memory_image_bytes,
        variable_table: container
            .type_section
            .as_ref()
            .map(|section| section.variable_table.clone())
            .unwrap_or_default(),
        task_table,
    }
}

/// The qualifier of every declaration the analyzer hands to code generation,
/// in declaration order: the result of constant inference, which is the one
/// thing a body edit can change about a declaration.
fn qualifiers(source: &str) -> Vec<DeclarationQualifier> {
    let (library, _) = parse(source, &CompilerOptions::default());
    let declarations = |variables: &[VarDecl]| -> Vec<DeclarationQualifier> {
        variables
            .iter()
            .map(|decl| decl.qualifier.clone())
            .collect()
    };
    library
        .elements
        .iter()
        .flat_map(|element| match element {
            LibraryElementKind::ProgramDeclaration(unit) => declarations(&unit.variables),
            LibraryElementKind::FunctionDeclaration(unit) => declarations(&unit.variables),
            LibraryElementKind::FunctionBlockDeclaration(unit) => declarations(&unit.variables),
            LibraryElementKind::GlobalVarDeclarations(variables) => declarations(variables),
            _ => Vec::new(),
        })
        .collect()
}

/// `template` with its `{body}` marker replaced by `body`.
fn with_body(template: &str, body: &str) -> String {
    template.replace("{body}", body)
}

/// A function with a parameter and a local, so a compiled call has slots.
const ADD_ONE: &str = "
FUNCTION add_one : DINT
  VAR_INPUT i : DINT; END_VAR
  VAR t : DINT; END_VAR
  add_one := i + t;
END_FUNCTION
";

/// A function block `counter` whose body is the template's `{body}`; `fields`
/// is its declaration list.
fn counter_fb(fields: &str) -> String {
    format!(
        "
FUNCTION_BLOCK counter
  VAR_OUTPUT q : DINT; END_VAR
  {fields}
  {{body}}
END_FUNCTION_BLOCK
"
    )
}

/// Edits whose statements change and whose declarations do not. Each row is
/// the one source text with a `{body}` marker, the body before and after, and
/// whether the edit flips constant inference for some declaration (a first
/// write added, or a last write removed). The container's layout must be
/// identical either way: the variable layout is a function of the
/// declarations alone, not of anything the bodies imply about them.
///
/// A first or last call of a user function is deliberately not a row: the call
/// decides whether the function is compiled at all, so its parameter, local
/// and return slots join or leave the layout (see `compile_in_budget` in
/// `compile.rs`, where `reachable` filters the functions).
#[rstest]
// Constant inference flips: one row per kind of declaration it can mark.
#[case::flip_program_scalar(
    program("v : DINT := 1; r : DINT;", "{body}"),
    "r := v;",
    "r := v; v := 2;",
    true
)]
#[case::flip_global_through_external(
    "VAR_GLOBAL g : DINT := 5; END_VAR
PROGRAM main
  VAR_EXTERNAL g : DINT; END_VAR
  VAR r : DINT; END_VAR
  {body}
END_PROGRAM"
        .to_string(),
    "r := g;",
    "r := g; g := 6;",
    true
)]
#[case::flip_fb_field_written_in_fb_body(
    [counter_fb("VAR c : DINT := 1; END_VAR"), program("inst : counter;", "inst();")].concat(),
    "q := c;",
    "q := c; c := c + 1;",
    true
)]
#[case::flip_fb_field_written_through_instance(
    [counter_fb("VAR c : DINT := 1; END_VAR").replace("{body}", "q := c;"),
     program("inst : counter;", "inst(); {body}")].concat(),
    "",
    "inst.c := 3;",
    true
)]
#[case::flip_string_read_by_len(
    program("s : STRING[10] := 'abc'; n : INT;", "{body}"),
    "n := LEN(s);",
    "n := LEN(s); s := 'xyz';",
    true
)]
#[case::flip_array_element(
    program("a : ARRAY[1..3] OF INT := [1, 2, 3]; r : INT;", "{body}"),
    "r := a[1];",
    "r := a[1]; a[2] := 5;",
    true
)]
#[case::flip_function_local(
    "FUNCTION add_t : DINT
  VAR_INPUT i : DINT; END_VAR
  VAR t : DINT := 3; END_VAR
  {body}
END_FUNCTION
PROGRAM main
  VAR r : DINT; END_VAR
  r := add_t(i := 1);
END_PROGRAM"
        .to_string(),
    "add_t := i + t;",
    "add_t := i + t; t := 4;",
    true
)]
#[case::flip_output_binding(
    [counter_fb("").replace("{body}", "q := 1;"), program("inst : counter; v : DINT := 0;", "{body}")].concat(),
    "inst();",
    "inst(q => v);",
    true
)]
#[case::flip_for_control_variable(
    program("i : DINT := 0; r : DINT;", "{body}"),
    "r := i;",
    "r := i; FOR i := 1 TO 3 DO r := r + 1; END_FOR;",
    true
)]
#[case::flip_write_inside_case_branch(
    program("v : DINT := 1; sel : DINT; r : DINT;", "{body}"),
    "CASE sel OF 1: r := v; END_CASE;",
    "CASE sel OF 1: r := v; 2: v := 5; END_CASE;",
    true
)]
#[case::flip_write_inside_if_branch(
    program("v : DINT := 1; r : DINT;", "{body}"),
    "IF r > 0 THEN r := v; END_IF;",
    "IF r > 0 THEN r := v; ELSE v := 5; END_IF;",
    true
)]
// Other facts a body determines about the compiled program.
#[case::first_call_of_standard_fb(
    program("t : TON; r : BOOL;", "{body}"),
    "r := FALSE;",
    "t(IN := TRUE, PT := T#1s); r := t.Q;",
    false
)]
#[case::first_string_temporary(
    program("s : STRING[20]; r : STRING[20];", "{body}"),
    "r := s;",
    "r := CONCAT(s, s);",
    false
)]
#[case::deeper_string_temporaries(
    program("s : STRING[20]; r : STRING[20];", "{body}"),
    "r := CONCAT(s, s);",
    "r := CONCAT(CONCAT(s, s), s);",
    false
)]
#[case::string_temporary_inside_function_body(
    "FUNCTION join : DINT
  VAR_INPUT i : DINT; END_VAR
  VAR s : STRING[8]; END_VAR
  {body}
END_FUNCTION
PROGRAM main
  VAR r : DINT; END_VAR
  r := join(i := 1);
END_PROGRAM"
        .to_string(),
    "join := i;",
    "s := CONCAT(s, s); join := LEN(s);",
    false
)]
#[case::new_integer_constant(dint_program("{body}"), "x := x + 1;", "x := x + 700000;", false)]
#[case::new_string_constant(
    program("s : STRING[20];", "{body}"),
    "s := 'abc';",
    "s := 'abcdefgh';",
    false
)]
#[case::deeper_expression_stack(
    dint_program("{body}"),
    "x := x + 1;",
    "x := x + (x * (x - (x + (x * x))));",
    false
)]
#[case::deeper_call_chain(
    [ADD_ONE.to_string(), counter_fb(""), program("inst : counter; r : DINT;", "r := add_one(i := 1); inst();")].concat(),
    "q := 1;",
    "q := add_one(i := 2);",
    false
)]
fn layout_when_body_only_edit_then_layout_unchanged(
    #[case] template: String,
    #[case] before: &str,
    #[case] after: &str,
    #[case] flips_constant_inference: bool,
) {
    let (before_source, after_source) = (with_body(&template, before), with_body(&template, after));

    let inference_changed = qualifiers(&before_source) != qualifiers(&after_source);
    assert_eq!(
        inference_changed, flips_constant_inference,
        "the row's declared effect on constant inference does not hold"
    );

    let before_layout = layout_of(&parse_and_compile(
        &before_source,
        &CompilerOptions::default(),
    ));
    let after_layout = layout_of(&parse_and_compile(
        &after_source,
        &CompilerOptions::default(),
    ));
    assert_eq!(before_layout, after_layout);
}
