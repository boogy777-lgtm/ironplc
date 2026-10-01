//! Stable variable ID emission tests (hot-edit Stage 2, T2).
//!
//! ADR 0053: the engineering side assigns a UID when a declaration is
//! created, and a rename -- including a name swap -- keeps it, so the type
//! and value stay with the entity and a rename moves no data. Codegen only
//! maps names to UIDs, matched with [`Id`] semantics (case-insensitive),
//! for the persistent prefix: globals and program variables. The resulting
//! `stable_vars` table is excluded from the layout hash.

use ironplc_codegen::{compile, CodegenOptions, EmptyLookup};
use ironplc_container::{Container, StableVarEntry, VarIndex};
use ironplc_dsl::core::Id;
use ironplc_parser::options::CompilerOptions;

use crate::common::parse;

/// Compiles `source` with the given engineering-side `(name, uid)` table.
fn compile_with_ids(source: &str, ids: &[(&str, u64)]) -> Container {
    let (library, context) = parse(source, &CompilerOptions::default());
    let options = CodegenOptions {
        stable_var_ids: ids
            .iter()
            .map(|(name, uid)| (Id::from(name), *uid))
            .collect(),
        ..CodegenOptions::default()
    };
    compile(&library, &context, &options, &EmptyLookup).unwrap()
}

/// The container's stable variable ID table; empty without a type section.
fn stable_vars(container: &Container) -> &[StableVarEntry] {
    container
        .type_section
        .as_ref()
        .map_or(&[], |section| section.stable_vars.as_slice())
}

/// The compiler-assigned index of a named variable.
fn index_of(container: &Container, name: &str) -> VarIndex {
    container
        .debug_section
        .as_ref()
        .unwrap()
        .var_names
        .iter()
        .find(|entry| entry.name.eq_ignore_ascii_case(name))
        .unwrap()
        .var_index
}

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

#[test]
fn stable_vars_when_table_names_variable_then_entry_at_its_index() {
    let container = compile_with_ids(&program("A : DINT;", "A := 1;"), &[("A", 7)]);

    assert_eq!(
        stable_vars(&container).to_vec(),
        vec![StableVarEntry {
            var_index: index_of(&container, "A"),
            uid: 7,
        }]
    );
}

#[test]
fn stable_vars_when_no_table_then_empty() {
    let container = compile_with_ids(&program("A : DINT;", "A := 1;"), &[]);

    assert!(stable_vars(&container).is_empty());
}

#[test]
fn stable_vars_when_table_names_unknown_variable_then_ignored() {
    let container = compile_with_ids(
        &program("A : DINT;", "A := 1;"),
        &[("missing", 9), ("A", 7)],
    );

    assert_eq!(
        stable_vars(&container).to_vec(),
        vec![StableVarEntry {
            var_index: index_of(&container, "A"),
            uid: 7,
        }]
    );
}

#[test]
fn stable_vars_when_table_name_differs_only_in_case_then_matched() {
    // `Id` comparison is case-insensitive, so the table follows the DSL's
    // identifier semantics rather than byte equality.
    let container = compile_with_ids(&program("A : DINT;", "A := 1;"), &[("a", 7)]);

    assert_eq!(stable_vars(&container).len(), 1);
    assert_eq!(stable_vars(&container)[0].uid, 7);
}

#[test]
fn stable_vars_when_logic_only_edit_then_identical() {
    let before = compile_with_ids(&program("A : DINT;", "A := 1;"), &[("A", 7)]);
    let after = compile_with_ids(&program("A : DINT;", "A := A + 1;"), &[("A", 7)]);

    assert_eq!(stable_vars(&before), stable_vars(&after));
}

#[test]
fn stable_vars_when_renamed_then_uid_at_the_new_name_index() {
    // The engineering side rebinds the entity's UID to the new name, so the
    // entity -- and with it the value -- needs no data movement (ADR 0053).
    let container = compile_with_ids(&program("B : DINT;", "B := 1;"), &[("B", 7)]);

    assert_eq!(
        stable_vars(&container).to_vec(),
        vec![StableVarEntry {
            var_index: index_of(&container, "B"),
            uid: 7,
        }]
    );
}

#[test]
fn stable_vars_when_names_swapped_with_uids_then_table_identical() {
    // Pre-swap: A is index 0 with UID 1 and B index 1 with UID 2. The swap
    // rebinds UID 1 to B and UID 2 to A; because the declarations exchange
    // positions too, every UID keeps its index -- zero data movement.
    let before = compile_with_ids(
        &program("A : DINT; B : DINT;", "A := 1; B := 2;"),
        &[("A", 1), ("B", 2)],
    );
    let after = compile_with_ids(
        &program("B : DINT; A : DINT;", "A := 1; B := 2;"),
        &[("B", 1), ("A", 2)],
    );

    assert_eq!(stable_vars(&before), stable_vars(&after));
}

#[test]
fn stable_vars_when_declarations_reordered_then_uid_follows_entity() {
    // Unchanged table: A keeps UID 1 and B keeps UID 2, each at its entity's
    // new index.
    let container = compile_with_ids(
        &program("B : DINT; A : DINT;", "A := 1; B := 2;"),
        &[("A", 1), ("B", 2)],
    );

    assert_eq!(
        stable_vars(&container).to_vec(),
        vec![
            StableVarEntry {
                var_index: index_of(&container, "B"),
                uid: 2,
            },
            StableVarEntry {
                var_index: index_of(&container, "A"),
                uid: 1,
            },
        ]
    );
}

#[test]
fn stable_vars_when_table_names_transient_slots_then_no_entry() {
    // ADR 0053: function parameters and locals are transient slots. Even a
    // name match must not invent an entry; only the program variable `A`
    // (the persistent prefix) is covered.
    let source = "
FUNCTION add_one : DINT
  VAR_INPUT v : DINT; END_VAR
  VAR t : DINT; END_VAR
  add_one := v + t;
END_FUNCTION

PROGRAM main
  VAR
    A : DINT;
  END_VAR
  A := add_one(A);
END_PROGRAM
";
    let container = compile_with_ids(source, &[("v", 3), ("t", 4), ("A", 5)]);

    assert_eq!(
        stable_vars(&container).to_vec(),
        vec![StableVarEntry {
            var_index: index_of(&container, "A"),
            uid: 5,
        }]
    );
}

#[test]
fn stable_vars_when_table_names_fb_instance_then_entry_and_fields_ignored() {
    // The FB instance itself is a program variable, so its UID is emitted;
    // the FB's fields live in the instance's data-region region, not the
    // persistent variable table, so their table names are ignored (ADR 0053).
    let source = "
FUNCTION_BLOCK doubler
  VAR_INPUT x : DINT; END_VAR
  VAR_OUTPUT y : DINT; END_VAR
  y := x * 1;
END_FUNCTION_BLOCK

PROGRAM main
  VAR
    inst : doubler;
    result : DINT;
  END_VAR
  inst(x := 7, y => result);
END_PROGRAM
";
    let container = compile_with_ids(source, &[("inst", 11), ("x", 12), ("y", 13)]);

    assert_eq!(
        stable_vars(&container).to_vec(),
        vec![StableVarEntry {
            var_index: index_of(&container, "inst"),
            uid: 11,
        }]
    );
}

#[test]
fn stable_vars_when_compiled_with_and_without_table_then_layout_hash_equal() {
    // UIDs are excluded from the layout hash (ADR 0053): a change to the
    // table alone is not a layout change.
    let source = program("A : DINT;", "A := 1;");
    let without = compile_with_ids(&source, &[]);
    let with = compile_with_ids(&source, &[("A", 7)]);

    assert_eq!(without.compute_layout_hash(), with.compute_layout_hash());
    assert!(stable_vars(&without).is_empty());
    assert_eq!(stable_vars(&with).len(), 1);
}
