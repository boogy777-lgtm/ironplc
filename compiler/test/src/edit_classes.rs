//! The table of edit classes: what each kind of edit of a running program does
//! to its state layout, and what the host does with it.
//!
//! One row is a pair of sources that differ by one edit, and what the edit
//! must do ([`Layout`]). The guards of code generation (`layout_hash.rs`) and
//! of the runtime (`body_edit_swap.rs`) both read this table and nothing else:
//! a new class of edit is a new row, and a guard cannot miss it.
//!
//! The state layout is the persistent part of a container (ADR-0073): the
//! globals, the variables of the program, the arrays they use and the fields of
//! function blocks. An edit of a body, including the first call of a user
//! function and the removal of the last one, leaves it unchanged. An edit of a
//! declaration of persistent state changes it.

/// What an edit does to the state layout, and what a host does with the
/// candidate that carries it.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum Layout {
    /// The layout is the same. The host accepts the candidate as an ordinary
    /// online change, with and without stable variable IDs: it swaps at a scan
    /// boundary, `untest` works, and every persistent value is kept.
    Unchanged,
    /// The layout changed. Without stable variable IDs the host refuses the
    /// candidate; with them, the outcome is the given one.
    Changed(WithStableIds),
}

/// What a host does with a candidate whose layout changed when both containers
/// carry stable variable IDs.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum WithStableIds {
    /// The migration planner can justify every copy: the candidate is
    /// accepted as a migration.
    Migration,
    /// The migration planner cannot: the candidate is refused.
    Refused,
    /// A persistent variable of one of the containers has no stable ID (a
    /// variable declared after the last synchronization, or one never keyed):
    /// the candidate is refused with that cause, whatever else the planner could
    /// have justified (ADR-0074).
    Unidentified,
}

/// One class of edit.
#[derive(Clone, Debug)]
pub struct EditClass {
    /// A name for the class, unique in the table.
    pub name: &'static str,
    /// The complete source before the edit.
    pub before: String,
    /// The complete source after the edit.
    pub after: String,
    /// Whether the edit flips constant inference for some declaration (a first
    /// write added, or a last write removed), which the analysis decides
    /// from the bodies.
    pub flips_constant_inference: bool,
    /// What the edit does to the layout.
    pub layout: Layout,
    /// The program variables that carry a stable ID when a host runs the
    /// class with stable IDs, as `(name, uid)`. A persistent variable of the
    /// sources that is not listed has no ID, which is the case of a class whose
    /// outcome is [`WithStableIds::Unidentified`].
    pub stable_ids: &'static [(&'static str, u64)],
    /// The fields of function blocks that carry a stable ID when a host runs the
    /// class with stable IDs, as `(function block type, field, uid)`.
    pub field_ids: &'static [(&'static str, &'static str, u64)],
    /// Variables that are non-zero once the program has run a few scans and
    /// that the edit must not change: a swap keeps them.
    pub observed: &'static [&'static str],
    /// Variables the edited code computes after the swap, with their values.
    pub computed: &'static [(&'static str, i32)],
}

/// A `PROGRAM main` with the given declaration list and body.
pub fn program(declarations: &str, body: &str) -> String {
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
pub fn dint_program(body: &str) -> String {
    program("x : DINT;", body)
}

/// A function with a parameter and a local, so a compiled call has slots.
/// It returns its argument plus one.
const ADD_ONE: &str = "
FUNCTION add_one : DINT
  VAR_INPUT i : DINT; END_VAR
  VAR t : DINT; END_VAR
  add_one := i + t + 1;
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

/// A program that warms up a persistent variable `v` for three scans (`n`
/// counts them), then runs `{body}` on every scan. `declarations` are
/// additional variables.
fn warm_program(declarations: &str, body: &str) -> String {
    program(
        &format!("n : DINT; v : DINT; r : DINT; a : DINT; b : DINT; {declarations}"),
        &format!("IF n < 3 THEN v := 41; n := n + 1; END_IF;\n  {body}"),
    )
}

/// A function block `holder` that keeps a persistent field `c`, set while
/// `set` is TRUE, and whose body is the template's `{body}`.
const HOLDER: &str = "
FUNCTION_BLOCK holder
  VAR_INPUT set : BOOL; END_VAR
  VAR_OUTPUT q : DINT; c : DINT; END_VAR
  IF set THEN c := 42; END_IF;
  {body}
END_FUNCTION_BLOCK
";

/// `template` with its `{body}` marker replaced by `body`.
fn with_body(template: &str, body: &str) -> String {
    template.replace("{body}", body)
}

/// A row for an edit of a body: the one source text with a `{body}` marker,
/// the body before and after. The layout does not change.
fn body_edit(
    name: &'static str,
    template: String,
    before: &str,
    after: &str,
    flips_constant_inference: bool,
) -> EditClass {
    EditClass {
        name,
        before: with_body(&template, before),
        after: with_body(&template, after),
        flips_constant_inference,
        layout: Layout::Unchanged,
        stable_ids: &[],
        field_ids: &[],
        observed: &[],
        computed: &[],
    }
}

/// A row for an edit of a declaration of persistent state.
fn declaration_edit(
    name: &'static str,
    before: String,
    after: String,
    outcome: WithStableIds,
    stable_ids: &'static [(&'static str, u64)],
) -> EditClass {
    EditClass {
        name,
        before,
        after,
        flips_constant_inference: false,
        layout: Layout::Changed(outcome),
        stable_ids,
        field_ids: &[],
        observed: &[],
        computed: &[],
    }
}

/// A function block `fb_name` with a persistent counter, as a template.
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

/// The rows. The order is the order of the report of a failing guard.
pub fn edit_classes() -> Vec<EditClass> {
    let mut rows = Vec::new();
    rows.extend(body_edits());
    rows.extend(call_edits());
    rows.extend(declaration_edits());
    rows
}

/// Edits whose statements change and whose declarations do not. The layout
/// is identical either way: the variable layout is a function of the
/// declarations alone, not of anything the bodies imply about them.
fn body_edits() -> Vec<EditClass> {
    vec![
        body_edit(
            "logic_constant_changed",
            dint_program("{body}"),
            "x := x + 1;",
            "x := x + 10;",
            false,
        ),
        body_edit(
            "logic_control_flow_changed",
            dint_program("{body}"),
            "x := x + 1;",
            "IF x > 0 THEN x := x + 1; END_IF;",
            false,
        ),
        // Constant inference flips: one row per kind of declaration it can mark.
        body_edit(
            "flip_program_scalar",
            program("v : DINT := 1; r : DINT;", "{body}"),
            "r := v;",
            "r := v; v := 2;",
            true,
        ),
        body_edit(
            "flip_global_through_external",
            "CONFIGURATION plc
  VAR_GLOBAL g : DINT := 5; END_VAR
  RESOURCE res ON PROCESSOR
    TASK scan(PRIORITY := 1);
    PROGRAM instance WITH scan : main;
  END_RESOURCE
END_CONFIGURATION
PROGRAM main
  VAR_EXTERNAL g : DINT; END_VAR
  VAR r : DINT; END_VAR
  {body}
END_PROGRAM"
                .to_string(),
            "r := g;",
            "r := g; g := 6;",
            true,
        ),
        body_edit(
            "flip_fb_field_written_in_fb_body",
            [
                counter_fb("VAR c : DINT := 1; END_VAR"),
                program("inst : counter;", "inst();"),
            ]
            .concat(),
            "q := c;",
            "q := c; c := c + 1;",
            true,
        ),
        body_edit(
            "flip_fb_field_written_through_instance",
            [
                with_body(&counter_fb("VAR c : DINT := 1; END_VAR"), "q := c;"),
                program("inst : counter;", "inst(); {body}"),
            ]
            .concat(),
            "",
            "inst.c := 3;",
            true,
        ),
        body_edit(
            "flip_string_read_by_len",
            program("s : STRING[10] := 'abc'; n : INT;", "{body}"),
            "n := LEN(s);",
            "n := LEN(s); s := 'xyz';",
            true,
        ),
        body_edit(
            "flip_array_element",
            program("a : ARRAY[1..3] OF INT := [1, 2, 3]; r : INT;", "{body}"),
            "r := a[1];",
            "r := a[1]; a[2] := 5;",
            true,
        ),
        body_edit(
            "flip_function_local",
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
            true,
        ),
        body_edit(
            "flip_output_binding",
            [
                with_body(&counter_fb(""), "q := 1;"),
                program("inst : counter; v : DINT := 0;", "{body}"),
            ]
            .concat(),
            "inst();",
            "inst(q => v);",
            true,
        ),
        body_edit(
            "flip_for_control_variable",
            program("i : DINT := 0; r : DINT;", "{body}"),
            "r := i;",
            "r := i; FOR i := 1 TO 3 DO r := r + 1; END_FOR;",
            true,
        ),
        body_edit(
            "flip_write_inside_case_branch",
            program("v : DINT := 1; sel : DINT; r : DINT;", "{body}"),
            "CASE sel OF 1: r := v; END_CASE;",
            "CASE sel OF 1: r := v; 2: v := 5; END_CASE;",
            true,
        ),
        body_edit(
            "flip_write_inside_if_branch",
            program("v : DINT := 1; r : DINT;", "{body}"),
            "IF r > 0 THEN r := v; END_IF;",
            "IF r > 0 THEN r := v; ELSE v := 5; END_IF;",
            true,
        ),
        // Other facts a body determines about the compiled program.
        body_edit(
            "first_call_of_standard_fb",
            program("t : TON; r : BOOL;", "{body}"),
            "r := FALSE;",
            "t(IN := TRUE, PT := T#1s); r := t.Q;",
            false,
        ),
        body_edit(
            "first_string_temporary",
            program("s : STRING[20]; r : STRING[20];", "{body}"),
            "r := s;",
            "r := CONCAT(s, s);",
            false,
        ),
        body_edit(
            "deeper_string_temporaries",
            program("s : STRING[20]; r : STRING[20];", "{body}"),
            "r := CONCAT(s, s);",
            "r := CONCAT(CONCAT(s, s), s);",
            false,
        ),
        body_edit(
            "string_temporary_inside_function_body",
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
            false,
        ),
        body_edit(
            "new_integer_constant",
            dint_program("{body}"),
            "x := x + 1;",
            "x := x + 700000;",
            false,
        ),
        body_edit(
            "new_string_constant",
            program("s : STRING[20];", "{body}"),
            "s := 'abc';",
            "s := 'abcdefgh';",
            false,
        ),
        body_edit(
            "deeper_expression_stack",
            dint_program("{body}"),
            "x := x + 1;",
            "x := x + (x * (x - (x + (x * x))));",
            false,
        ),
        body_edit(
            "deeper_call_chain",
            [
                ADD_ONE.to_string(),
                counter_fb(""),
                program("inst : counter; r : DINT;", "r := add_one(i := 1); inst();"),
            ]
            .concat(),
            "q := 1;",
            "q := add_one(i := 2);",
            false,
        ),
    ]
}

/// Edits that decide which functions are compiled: the first call of a user
/// function and the removal of the last one, in a program body and in a
/// function block body, and the declaration of a function nothing calls. The
/// function's parameters, locals and return value join the variable table or
/// leave it; they are working slots and not state, so the layout is unchanged.
fn call_edits() -> Vec<EditClass> {
    // The program keeps `v` and reads it into `r` on every scan; the edit is
    // the statement that computes `r`.
    let in_program = [ADD_ONE.to_string(), warm_program("", "{body}")].concat();
    let program_row = |name, before: &str, after: &str, computed| EditClass {
        observed: &["v"],
        computed,
        stable_ids: &[("n", 1), ("v", 2), ("r", 3)],
        ..body_edit(name, in_program.clone(), before, after, false)
    };

    // The block keeps `c`, which the program reads into the sink `b` on every
    // scan; the edit is the statement that computes the block's output `q`,
    // which the program reads into `a`.
    let in_block = [
        ADD_ONE.to_string(),
        HOLDER.to_string(),
        warm_program("h : holder;", "h(set := n < 3); a := h.q; b := h.c;"),
    ]
    .concat();
    let block_row = |name, before: &str, after: &str, computed| EditClass {
        observed: &["b"],
        computed,
        stable_ids: &[("n", 1), ("v", 2), ("h", 3)],
        ..body_edit(name, in_block.clone(), before, after, false)
    };

    vec![
        program_row(
            "first_call_of_user_function_in_program_body",
            "r := v;",
            "r := add_one(i := v);",
            &[("r", 42)],
        ),
        program_row(
            "last_call_of_user_function_removed_in_program_body",
            "r := add_one(i := v);",
            "r := v;",
            &[("r", 41)],
        ),
        block_row(
            "first_call_of_user_function_in_function_block_body",
            "q := c;",
            "q := add_one(i := c);",
            &[("a", 43)],
        ),
        block_row(
            "last_call_of_user_function_removed_in_function_block_body",
            "q := add_one(i := c);",
            "q := c;",
            &[("a", 42)],
        ),
        // The order of functions and function blocks in the container comes
        // from the dependency sort of the declarations, not from their names or
        // from the order they are written in. A body that starts to call a
        // function declared before it adds a dependency from an earlier node of
        // the sort to a later one, which can reorder blocks that have nothing
        // to do with the call. The persistent state does not care.
        EditClass {
            name: "dependency_added_from_earlier_to_later_declaration",
            before: three_blocks_after_a_function("q := 1;"),
            after: three_blocks_after_a_function("q := helper(i := 1);"),
            flips_constant_inference: false,
            layout: Layout::Unchanged,
            stable_ids: &[("a", 1), ("b", 2), ("c", 3), ("r", 4)],
            field_ids: &[],
            observed: &[],
            computed: &[("r", 5)],
        },
        // A function nothing calls is not compiled, so declaring it changes
        // nothing.
        EditClass {
            name: "unused_function_declared",
            before: warm_program("", "r := v;"),
            after: [ADD_ONE.to_string(), warm_program("", "r := v;")].concat(),
            flips_constant_inference: false,
            layout: Layout::Unchanged,
            stable_ids: &[("n", 1), ("v", 2), ("r", 3)],
            field_ids: &[],
            observed: &["v"],
            computed: &[("r", 41)],
        },
    ]
}

/// A function, then three function blocks, then the program that instantiates
/// the blocks in order. The body of the second block is `second_body`; the edit
/// makes it call the function.
fn three_blocks_after_a_function(second_body: &str) -> String {
    format!(
        "
FUNCTION helper : DINT
  VAR_INPUT i : DINT; END_VAR
  helper := i + 1;
END_FUNCTION

FUNCTION_BLOCK block_a
  VAR_OUTPUT q : DINT; END_VAR
  VAR c : DINT; END_VAR
  q := 1;
END_FUNCTION_BLOCK

FUNCTION_BLOCK block_b
  VAR_OUTPUT q : DINT; END_VAR
  VAR s : STRING[4]; END_VAR
  {second_body}
END_FUNCTION_BLOCK

FUNCTION_BLOCK block_c
  VAR_OUTPUT q : DINT; END_VAR
  q := 1;
END_FUNCTION_BLOCK

PROGRAM main
  VAR a : block_a; b : block_b; c : block_c; r : DINT; END_VAR
  a();
  b();
  c();
  r := a.q + b.q + c.q + 1;
END_PROGRAM
"
    )
}

/// The IDs of the fields of the function block `doubler` of [`fb_program`].
const FB_FIELD_IDS: &[(&str, &str, u64)] = &[
    ("doubler", "x", 11),
    ("doubler", "y", 12),
    ("doubler", "z", 13),
];

/// Edits of a declaration of persistent state.
fn declaration_edits() -> Vec<EditClass> {
    vec![
        declaration_edit(
            "variable_added",
            program("x : DINT;", "x := 1;"),
            program("x : DINT; y : DINT;", "x := 1;"),
            WithStableIds::Migration,
            &[("x", 1), ("y", 2)],
        ),
        declaration_edit(
            "variable_removed",
            program("x : DINT; y : DINT;", "x := 1;"),
            program("x : DINT;", "x := 1;"),
            WithStableIds::Migration,
            &[("x", 1), ("y", 2)],
        ),
        declaration_edit(
            "variable_retyped",
            dint_program("x := x;"),
            program("x : REAL;", "x := x;"),
            WithStableIds::Migration,
            &[("x", 1)],
        ),
        declaration_edit(
            "array_bound_changed",
            program("a : ARRAY[1..4] OF INT;", "a[1] := 1;"),
            program("a : ARRAY[1..5] OF INT;", "a[1] := 1;"),
            WithStableIds::Refused,
            &[("a", 1)],
        ),
        declaration_edit(
            "variable_added_without_id",
            program("x : DINT;", "x := 1;"),
            program("x : DINT; y : DINT;", "x := 1;"),
            WithStableIds::Unidentified,
            &[("x", 1)],
        ),
        // The case the identity rule exists for: `b` was never given an ID and
        // keeps running through an edit that adds `c`. A migration would return
        // `b` to its initial value without a word.
        declaration_edit(
            "variable_without_id_kept_across_an_added_variable",
            program("x : DINT; b : DINT := 5;", "x := x + 1; b := b + 10;"),
            program("x : DINT; b : DINT := 5; c : DINT;", "x := x + 1; b := b + 10;"),
            WithStableIds::Unidentified,
            &[("x", 1)],
        ),
        EditClass {
            field_ids: FB_FIELD_IDS,
            ..declaration_edit(
                "function_block_field_added",
                fb_program("VAR_INPUT x : DINT; END_VAR VAR_OUTPUT y : DINT; END_VAR"),
                fb_program(
                    "VAR_INPUT x : DINT; END_VAR VAR_OUTPUT y : DINT; END_VAR VAR z : DINT; END_VAR",
                ),
                WithStableIds::Migration,
                &[("inst", 1), ("result", 2)],
            )
        },
        EditClass {
            field_ids: FB_FIELD_IDS,
            ..declaration_edit(
                "function_block_field_retyped",
                fb_program(
                    "VAR_INPUT x : DINT; END_VAR VAR_OUTPUT y : DINT; END_VAR VAR z : DINT; END_VAR",
                ),
                fb_program(
                    "VAR_INPUT x : DINT; END_VAR VAR_OUTPUT y : DINT; END_VAR VAR z : REAL; END_VAR",
                ),
                // With an ID for every field, the retype is a conversion the policy
                // admits (ADR-0060), not a refusal.
                WithStableIds::Migration,
                &[("inst", 1), ("result", 2)],
            )
        },
        // The fields of the block carry IDs except the one the edit adds.
        EditClass {
            field_ids: &[("doubler", "x", 11), ("doubler", "y", 12)],
            ..declaration_edit(
                "function_block_field_added_without_uid",
                fb_program("VAR_INPUT x : DINT; END_VAR VAR_OUTPUT y : DINT; END_VAR"),
                fb_program(
                    "VAR_INPUT x : DINT; END_VAR VAR_OUTPUT y : DINT; END_VAR VAR z : DINT; END_VAR",
                ),
                WithStableIds::Unidentified,
                &[("inst", 1), ("result", 2)],
            )
        },
    ]
}

#[cfg(test)]
mod tests {
    use std::collections::HashSet;

    use super::*;

    #[test]
    fn edit_classes_when_listed_then_names_are_unique() {
        let rows = edit_classes();
        let names: HashSet<&str> = rows.iter().map(|row| row.name).collect();

        assert_eq!(names.len(), rows.len());
    }
}
