//! The retained runs of the container (ADR-0085): which variable slots and
//! which bytes of the data region hold a value that survives a restart.
//!
//! The runs are written at the point where code generation assigns the storage
//! of a persistent declaration, from the qualifier of the declaration (or, for a
//! function block instance, from the qualifiers of the fields of its type).
//! Every container here is written and read back, so the load check runs on it
//! and the runs are the ones a reader sees.

use crate::common::parse_and_compile;
use ironplc_container::{
    string_region_size, CharWidth, Container, RetainKind, RetainSpace, RetainedRun, SLOT_BYTES,
};
use ironplc_dsl::common::{DeclarationQualifier, LibraryElementKind};
use ironplc_parser::options::CompilerOptions;

fn options() -> CompilerOptions {
    CompilerOptions {
        allow_persistent_var: true,
        allow_fb_inheritance: true,
        ..CompilerOptions::default()
    }
}

/// Compiles `source`, writes the container and reads it back, so that the
/// load check has accepted the retained runs of the result.
fn compile(source: &str) -> Container {
    let container = parse_and_compile(source, &options());
    let mut bytes = Vec::new();
    container.write_to(&mut bytes).unwrap();
    Container::read_from(&mut std::io::Cursor::new(&bytes)).unwrap()
}

/// The size of one instance of a function block with `slots` fields and no
/// string.
fn instance_bytes(slots: u32) -> u32 {
    slots * SLOT_BYTES
}

#[test]
fn retained_runs_when_global_retain_scalar_then_its_slot_only() {
    let container = compile(
        "
VAR_GLOBAL RETAIN
  g : DINT;
END_VAR

PROGRAM main
  VAR x : DINT; END_VAR
  x := x + 1;
END_PROGRAM
",
    );

    assert_eq!(
        container.retained_runs(),
        vec![RetainedRun::slots(RetainKind::Retain, 0, 1)]
    );
}

#[test]
fn retained_runs_when_global_persistent_then_kind_is_persistent() {
    let container = compile(
        "
VAR_GLOBAL PERSISTENT
  g : DINT;
END_VAR

PROGRAM main
  VAR x : DINT; END_VAR
  x := x + 1;
END_PROGRAM
",
    );

    assert_eq!(
        container.retained_runs(),
        vec![RetainedRun::slots(RetainKind::Persistent, 0, 1)]
    );
}

#[test]
fn retained_runs_when_program_persistent_string_then_slot_and_characters() {
    let container = compile(
        "
PROGRAM main
  VAR PERSISTENT s : STRING[10]; END_VAR
  VAR n : DINT; END_VAR
  n := LEN(s);
END_PROGRAM
",
    );

    assert_eq!(
        container.retained_runs(),
        vec![
            RetainedRun::slots(RetainKind::Persistent, 0, 1),
            RetainedRun::bytes(
                RetainKind::Persistent,
                0,
                string_region_size(10, CharWidth::Narrow)
            ),
        ]
    );
}

#[test]
fn retained_runs_when_program_retain_array_then_slot_and_whole_array() {
    let container = compile(
        "
PROGRAM main
  VAR RETAIN values : ARRAY[1..4] OF INT; END_VAR
  VAR n : DINT; END_VAR
  n := n + 1;
END_PROGRAM
",
    );

    let extent = container.persistent_extents()[0];
    assert!(extent.data_len > 0);
    assert_eq!(
        container.retained_runs(),
        vec![
            RetainedRun::slots(RetainKind::Retain, 0, 1),
            RetainedRun::bytes(RetainKind::Retain, 0, extent.data_len),
        ]
    );
}

#[test]
fn retained_runs_when_retain_declaration_follows_others_then_run_starts_after_them() {
    let container = compile(
        "
PROGRAM main
  VAR before : STRING[4]; n : DINT; END_VAR
  VAR RETAIN kept : STRING[6]; END_VAR
  n := LEN(before);
END_PROGRAM
",
    );

    let before = string_region_size(4, CharWidth::Narrow);
    assert_eq!(
        container.retained_runs(),
        vec![
            RetainedRun::slots(RetainKind::Retain, 2, 1),
            RetainedRun::bytes(
                RetainKind::Retain,
                before,
                string_region_size(6, CharWidth::Narrow)
            ),
        ]
    );
}

#[test]
fn retained_runs_when_adjacent_declarations_share_a_kind_then_runs_are_merged() {
    let container = compile(
        "
PROGRAM main
  VAR RETAIN a : DINT; b : DINT; c : DINT; END_VAR
  a := a + 1;
END_PROGRAM
",
    );

    assert_eq!(
        container.retained_runs(),
        vec![RetainedRun::slots(RetainKind::Retain, 0, 3)]
    );
}

#[test]
fn retained_runs_when_adjacent_declarations_differ_in_kind_then_runs_are_kept_apart() {
    let container = compile(
        "
PROGRAM main
  VAR RETAIN a : DINT; END_VAR
  VAR PERSISTENT b : DINT; END_VAR
  a := a + 1;
END_PROGRAM
",
    );

    assert_eq!(
        container.retained_runs(),
        vec![
            RetainedRun::slots(RetainKind::Retain, 0, 1),
            RetainedRun::slots(RetainKind::Persistent, 1, 1),
        ]
    );
}

const COUNTER_FB: &str = "
FUNCTION_BLOCK counter_fb
  VAR_INPUT step : DINT; END_VAR
  VAR_OUTPUT total : DINT; END_VAR
  VAR inner : DINT; text : STRING[5]; END_VAR
  inner := inner + step;
  total := inner;
END_FUNCTION_BLOCK
";

#[test]
fn retained_runs_when_function_block_instance_retain_then_whole_instance() {
    let source = format!(
        "{COUNTER_FB}
PROGRAM main
  VAR RETAIN cfb : counter_fb; END_VAR
  VAR n : DINT; END_VAR
  cfb(step := 1);
  n := cfb.total;
END_PROGRAM
"
    );
    let container = compile(&source);

    let extent = container.persistent_extents()[0];
    // Four slots and the characters of the string field.
    assert_eq!(
        extent.data_len,
        instance_bytes(4) + string_region_size(5, CharWidth::Narrow)
    );
    assert_eq!(
        container.retained_runs(),
        vec![
            RetainedRun::slots(RetainKind::Retain, 0, 1),
            RetainedRun::bytes(RetainKind::Retain, 0, extent.data_len),
        ]
    );
}

const MIXED_FB: &str = "
FUNCTION_BLOCK mixed_fb
  VAR_INPUT step : DINT; END_VAR
  VAR other : DINT; END_VAR
  VAR RETAIN kept : DINT; END_VAR
  VAR PERSISTENT forever : DINT; END_VAR
  kept := kept + step;
END_FUNCTION_BLOCK
";

#[test]
fn retained_runs_when_plain_instance_of_block_with_retain_field_then_only_those_fields() {
    let source = format!(
        "{MIXED_FB}
PROGRAM main
  VAR inst : mixed_fb; n : DINT; END_VAR
  inst(step := 1);
END_PROGRAM
"
    );
    let container = compile(&source);

    // Fields in slot order: step (input), other, kept, forever.
    assert_eq!(
        container.retained_runs(),
        vec![
            RetainedRun::bytes(RetainKind::Retain, 2 * SLOT_BYTES, SLOT_BYTES),
            RetainedRun::bytes(RetainKind::Persistent, 3 * SLOT_BYTES, SLOT_BYTES),
        ]
    );
}

#[test]
fn retained_runs_when_two_plain_instances_then_each_instance_contributes_its_fields() {
    let source = format!(
        "{MIXED_FB}
PROGRAM main
  VAR first : mixed_fb; second : mixed_fb; END_VAR
  first(step := 1);
END_PROGRAM
"
    );
    let container = compile(&source);

    let size = instance_bytes(4);
    assert_eq!(
        container.retained_runs(),
        vec![
            RetainedRun::bytes(RetainKind::Retain, 2 * SLOT_BYTES, SLOT_BYTES),
            RetainedRun::bytes(RetainKind::Persistent, 3 * SLOT_BYTES, SLOT_BYTES),
            RetainedRun::bytes(RetainKind::Retain, size + 2 * SLOT_BYTES, SLOT_BYTES),
            RetainedRun::bytes(RetainKind::Persistent, size + 3 * SLOT_BYTES, SLOT_BYTES),
        ]
    );
}

#[test]
fn retained_runs_when_array_of_plain_instances_then_each_element_contributes_its_fields() {
    let source = format!(
        "{MIXED_FB}
PROGRAM main
  VAR many : ARRAY[1..3] OF mixed_fb; END_VAR
END_PROGRAM
"
    );
    let container = compile(&source);

    let size = instance_bytes(4);
    let runs = container.retained_runs();
    assert_eq!(runs.len(), 6);
    assert_eq!(
        runs[4],
        RetainedRun::bytes(RetainKind::Retain, 2 * size + 2 * SLOT_BYTES, SLOT_BYTES)
    );
    assert!(runs.iter().all(|run| run.space == RetainSpace::Bytes));
}

#[test]
fn retained_runs_when_retain_field_is_a_string_then_slot_and_characters_are_runs_of_the_instance() {
    let source = "
FUNCTION_BLOCK named_fb
  VAR RETAIN name : STRING[8]; END_VAR
  VAR other : DINT; END_VAR
  other := other + 1;
END_FUNCTION_BLOCK

PROGRAM main
  VAR inst : named_fb; END_VAR
  inst();
END_PROGRAM
";
    let container = compile(source);

    // The slot of `name` is the first slot of the instance and its characters
    // follow the two slots, so the two runs are not adjacent.
    assert_eq!(
        container.retained_runs(),
        vec![
            RetainedRun::bytes(RetainKind::Retain, 0, SLOT_BYTES),
            RetainedRun::bytes(
                RetainKind::Retain,
                instance_bytes(2),
                string_region_size(8, CharWidth::Narrow)
            ),
        ]
    );
}

#[test]
fn retained_runs_when_non_retain_instance_of_block_with_retain_field_then_none() {
    let source = format!(
        "{MIXED_FB}
PROGRAM main
  VAR NON_RETAIN inst : mixed_fb; END_VAR
  inst(step := 1);
END_PROGRAM
"
    );

    assert!(compile(&source).retained_runs().is_empty());
}

#[test]
fn retained_runs_when_instance_persistent_and_block_has_retain_field_then_whole_instance() {
    let source = format!(
        "{MIXED_FB}
PROGRAM main
  VAR PERSISTENT inst : mixed_fb; END_VAR
  inst(step := 1);
END_PROGRAM
"
    );
    let container = compile(&source);

    assert_eq!(
        container.retained_runs(),
        vec![
            RetainedRun::slots(RetainKind::Persistent, 0, 1),
            RetainedRun::bytes(RetainKind::Persistent, 0, instance_bytes(4)),
        ]
    );
}

/// Compiles `source` after every variable of every function, and of every
/// method of every function block, has been given the qualifier `RETAIN`, as a
/// PLCopen XML variable list that carries `retain="true"` does. The parser
/// refuses the qualifier in those blocks, so the AST is the way to reach it.
fn compile_with_retain_on_working_locals(source: &str) -> Container {
    let options = options();
    let (mut library, context) = crate::common::parse(source, &options);
    for element in &mut library.elements {
        match element {
            LibraryElementKind::FunctionDeclaration(function) => {
                for variable in &mut function.variables {
                    variable.qualifier = DeclarationQualifier::Retain;
                }
            }
            LibraryElementKind::FunctionBlockDeclaration(block) => {
                for method in &mut block.methods {
                    for variable in &mut method.variables {
                        variable.qualifier = DeclarationQualifier::Retain;
                    }
                }
            }
            _ => {}
        }
    }
    ironplc_codegen::compile(
        &library,
        &context,
        &ironplc_codegen::CodegenOptions::from(&options),
        &ironplc_codegen::EmptyLookup,
    )
    .unwrap()
}

#[test]
fn retained_runs_when_function_local_is_retain_then_none() {
    let source = "
FUNCTION bump : DINT
  VAR_INPUT i : DINT; END_VAR
  VAR t : DINT; s : STRING[6]; END_VAR
  bump := i + t + LEN(s);
END_FUNCTION

PROGRAM main
  VAR n : DINT; END_VAR
  n := bump(i := 1);
END_PROGRAM
";

    let container = compile_with_retain_on_working_locals(source);

    assert!(container.retained_runs().is_empty());
    assert!(container.header.num_variables > container.persistent_var_count() as u16);
}

#[test]
fn retained_runs_when_method_local_is_retain_then_none() {
    let source = "
FUNCTION_BLOCK FB_Counter
  VAR count : DINT; END_VAR
  METHOD Add
    VAR_INPUT step : DINT; END_VAR
    VAR scratch : DINT; END_VAR
    scratch := step;
    count := count + scratch;
  END_METHOD
END_FUNCTION_BLOCK

PROGRAM main
  VAR c : FB_Counter; END_VAR
  c.Add(1);
END_PROGRAM
";

    let container = compile_with_retain_on_working_locals(source);

    assert!(container.retained_runs().is_empty());
}

#[test]
fn retained_runs_when_program_var_temp_is_retain_then_none() {
    let source = "
PROGRAM main
  VAR_TEMP t : DINT; END_VAR
  VAR n : DINT; END_VAR
  t := 1;
  n := t;
END_PROGRAM
";
    let options = options();
    let (mut library, context) = crate::common::parse(source, &options);
    for element in &mut library.elements {
        if let LibraryElementKind::ProgramDeclaration(program) = element {
            for variable in &mut program.variables {
                variable.qualifier = DeclarationQualifier::Retain;
            }
        }
    }
    let container = ironplc_codegen::compile(
        &library,
        &context,
        &ironplc_codegen::CodegenOptions::from(&options),
        &ironplc_codegen::EmptyLookup,
    )
    .unwrap();

    // Every variable of the program was marked, and the persistent one is
    // retained; the temporary, which takes its slot after the persistent part,
    // is not.
    assert_eq!(
        container.retained_runs(),
        vec![RetainedRun::slots(RetainKind::Retain, 0, 1)]
    );
}
#[test]
fn retained_runs_when_non_retain_and_unqualified_then_none() {
    let container = compile(
        "
PROGRAM main
  VAR NON_RETAIN a : DINT; END_VAR
  VAR b : STRING[4]; END_VAR
  a := a + 1;
END_PROGRAM
",
    );

    assert!(container.retained_runs().is_empty());
}

#[test]
fn retained_runs_when_container_written_and_read_then_runs_are_kept() {
    let source = "
PROGRAM main
  VAR RETAIN a : DINT; s : STRING[4]; END_VAR
  a := a + 1;
END_PROGRAM
";
    let compiled = parse_and_compile(source, &options());

    let read_back = compile(source);

    assert!(!compiled.retained_runs().is_empty());
    assert_eq!(compiled.retained_runs(), read_back.retained_runs());
}

#[test]
fn retained_runs_when_only_the_qualifier_changes_then_layout_hash_is_unchanged() {
    let plain = parse_and_compile(
        "PROGRAM main VAR a : DINT; s : STRING[4]; END_VAR a := a + 1; END_PROGRAM",
        &options(),
    );
    let retained = parse_and_compile(
        "PROGRAM main VAR RETAIN a : DINT; s : STRING[4]; END_VAR a := a + 1; END_PROGRAM",
        &options(),
    );
    let persistent = parse_and_compile(
        "PROGRAM main VAR PERSISTENT a : DINT; s : STRING[4]; END_VAR a := a + 1; END_PROGRAM",
        &options(),
    );

    assert_eq!(plain.compute_layout_hash(), retained.compute_layout_hash());
    assert_eq!(
        plain.compute_layout_hash(),
        persistent.compute_layout_hash()
    );
    assert_eq!(plain.persistent_extents(), retained.persistent_extents());
    assert_ne!(plain.retained_runs(), retained.retained_runs());
    assert_ne!(retained.retained_runs(), persistent.retained_runs());
}

#[test]
fn retained_runs_when_only_the_qualifier_changes_then_content_hash_changes() {
    let content_hash = |source: &str| {
        let mut bytes = Vec::new();
        parse_and_compile(source, &options())
            .write_to(&mut bytes)
            .unwrap();
        Container::read_from(&mut std::io::Cursor::new(&bytes))
            .unwrap()
            .header
            .content_hash
    };

    let plain = content_hash("PROGRAM main VAR a : DINT; END_VAR a := a + 1; END_PROGRAM");
    let retained =
        content_hash("PROGRAM main VAR RETAIN a : DINT; END_VAR a := a + 1; END_PROGRAM");

    assert_ne!(plain, retained);
}
