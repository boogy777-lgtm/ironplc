//! Layout-hash and type-section variable-table tests (hot-edit P0, T2/T3).
//!
//! The variable table must carry exactly one entry per compiler-assigned
//! variable index, derived from the declarations alone. That is what makes
//! the layout hash (see "Layout Hash and Online Change" in
//! `specs/design/bytecode-container-format.md`) identical for a logic-only
//! edit and different for any declaration change.

use std::collections::{BTreeMap, HashSet};

use crate::common::{parse, parse_and_compile};
use ironplc_container::{
    string_region_size, CharWidth, Container, FieldEntry, FieldType, PersistentExtent, VarEntry,
    VAR_FLAG_IS_ARRAY,
};
use ironplc_dsl::common::{DeclarationQualifier, LibraryElementKind, VarDecl};
use ironplc_parser::options::CompilerOptions;
use ironplc_test::edit_classes::{dint_program, edit_classes, Layout};

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
/// the running state can carry over to a candidate (ADR-0073): the layout hash,
/// the persistent extent and the variable table entries in it, the header
/// flags, the process-image sizes, and the task table. A new compared field is
/// added here, once.
///
/// The count of variables of the whole table is not part of it, and neither is
/// the variable count of the program entry in the task table: they describe the
/// working slots too, which the first call of a user function adds.
#[derive(Debug, PartialEq)]
struct StateLayout {
    layout_hash: [u8; 32],
    persistent_extents: Vec<PersistentExtent>,
    persistent_variables: Vec<VarEntry>,
    flags: u8,
    input_image_bytes: u16,
    output_image_bytes: u16,
    memory_image_bytes: u16,
    task_table: Vec<u8>,
}

fn state_layout(container: &Container) -> StateLayout {
    let table = container
        .type_section
        .as_ref()
        .map(|section| section.variable_table.as_slice())
        .unwrap_or_default();
    let persistent_extents = container.persistent_extents();
    let persistent_variables = persistent_extents
        .iter()
        .flat_map(|row| table[row.vars()].iter().cloned())
        .collect();

    let mut schedule = container.task_table.clone();
    for program in &mut schedule.programs {
        program.var_table_count = 0;
    }
    let mut task_table = Vec::new();
    schedule.write_to(&mut task_table).unwrap();

    StateLayout {
        layout_hash: container.compute_layout_hash(),
        persistent_extents,
        persistent_variables,
        flags: container.header.flags,
        input_image_bytes: container.header.input_image_bytes,
        output_image_bytes: container.header.output_image_bytes,
        memory_image_bytes: container.header.memory_image_bytes,
        task_table,
    }
}

/// The qualifier of every declaration the analyzer hands to code generation,
/// by unit and position in its declaration list: the result of constant
/// inference, which is the one thing a body edit can change about a
/// declaration.
fn qualifiers(source: &str) -> BTreeMap<(String, usize), DeclarationQualifier> {
    let (library, _) = parse(source, &CompilerOptions::default());
    let declarations = |unit: String, variables: &[VarDecl]| -> Vec<_> {
        variables
            .iter()
            .enumerate()
            .map(|(position, decl)| ((unit.clone(), position), decl.qualifier.clone()))
            .collect()
    };
    library
        .elements
        .iter()
        .flat_map(|element| match element {
            LibraryElementKind::ProgramDeclaration(unit) => {
                declarations(unit.name.to_string(), &unit.variables)
            }
            LibraryElementKind::FunctionDeclaration(unit) => {
                declarations(unit.name.to_string(), &unit.variables)
            }
            LibraryElementKind::FunctionBlockDeclaration(unit) => {
                declarations(unit.name.name.to_string(), &unit.variables)
            }
            LibraryElementKind::GlobalVarDeclarations(variables) => {
                declarations(String::new(), variables)
            }
            _ => Vec::new(),
        })
        .collect()
}

/// Whether some declaration that both sources declare is qualified differently
/// in them. A declaration only one source has (a function the edit adds) is
/// not an edit of constant inference.
fn inference_flipped(before: &str, after: &str) -> bool {
    let (before, after) = (qualifiers(before), qualifiers(after));
    before
        .iter()
        .any(|(key, qualifier)| after.get(key).is_some_and(|other| other != qualifier))
}

/// Every class of edit in the table does to the state layout what the table
/// says: a body edit (including the first call of a user function and the
/// removal of the last one, and a function that nothing calls) leaves it
/// unchanged, and an edit of a declaration of persistent state changes it.
#[test]
fn state_layout_when_edit_class_applied_then_layout_is_as_the_table_says() {
    for class in edit_classes() {
        assert_eq!(
            inference_flipped(&class.before, &class.after),
            class.flips_constant_inference,
            "{}: the declared effect on constant inference does not hold",
            class.name
        );

        let before = state_layout(&parse_and_compile(
            &class.before,
            &CompilerOptions::default(),
        ));
        let after = state_layout(&parse_and_compile(
            &class.after,
            &CompilerOptions::default(),
        ));

        assert_eq!(
            before == after,
            class.layout == Layout::Unchanged,
            "{}: the layout is not as the table says",
            class.name
        );
    }
}

/// The type ID of a function block is the rank of its name among the compiled
/// blocks, so that the dependency sort of the declarations, which an edit of a
/// body can reorder, never renumbers the blocks.
#[test]
fn user_fb_type_ids_when_blocks_declared_in_any_order_then_numbered_by_name() {
    let source = "
FUNCTION_BLOCK zulu VAR_OUTPUT q : DINT; END_VAR q := 1; END_FUNCTION_BLOCK
FUNCTION_BLOCK alpha VAR_OUTPUT q : DINT; END_VAR q := 2; END_FUNCTION_BLOCK
FUNCTION_BLOCK mike VAR_OUTPUT q : DINT; END_VAR q := 3; END_FUNCTION_BLOCK

PROGRAM main
  VAR z : zulu; a : alpha; m : mike; END_VAR
  z(); a(); m();
END_PROGRAM
";
    let container = parse_and_compile(source, &CompilerOptions::default());
    let section = container.type_section.as_ref().unwrap();
    let names = &container.debug_section.as_ref().unwrap().func_names;

    let type_id_of = |name: &str| {
        let function_id = names
            .iter()
            .find(|entry| entry.name.eq_ignore_ascii_case(name))
            .unwrap()
            .function_id;
        section
            .user_fb_types
            .iter()
            .find(|descriptor| descriptor.function_id == function_id)
            .unwrap()
            .type_id
            .raw()
    };

    assert_eq!(type_id_of("alpha"), 0x1000);
    assert_eq!(type_id_of("mike"), 0x1001);
    assert_eq!(type_id_of("zulu"), 0x1002);
}
