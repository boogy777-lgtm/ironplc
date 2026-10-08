//! Acceptance tests for what an accepted body-only edit does to the running
//! application: the persistent state it carries over, and the buffers the
//! candidate needs.
//!
//! A body edit that passes `validate_candidate` swaps in new code over the
//! old buffers. Two things must hold afterwards. The state of every
//! persistent variable the edit does not write is the state it had before,
//! wherever in the data region the candidate's code looks for it. And the
//! candidate runs on buffers sized for what *it* needs, however much more
//! stack, call depth, string temporaries or data region that is than the
//! active application used.
//!
//! Fixtures are compiled with the real project pipeline and round-tripped
//! through the container wire format, like the other acceptance tests.

// Test-target boundary: the workspace denies panicking constructs in
// production code; tests assert by panicking, so they are exempt here.
#![allow(
    clippy::unwrap_used,
    clippy::expect_used,
    reason = "integration test target: panicking helpers are sanctioned in tests"
)]

mod common;

use common::{compile_source, compile_with_ids, variable_index};
use ironplc_container::Container;
use ironplc_runtime::{HostMode, OnlineChangeError, RuntimeHost};
use ironplc_test::edit_classes::{edit_classes, EditClass, Layout, WithStableIds};
use rstest::rstest;

/// One kind of persistent variable: the declarations that make the program
/// hold it, the statements that give it a non-initial value during the first
/// scans, and the statements that copy what the code sees into the sinks
/// `a`, `b` and `c` on every scan.
struct Kind {
    /// Declarations at the top level of the source, before the program.
    top: &'static str,
    /// Declaration blocks inside the program.
    declarations: &'static str,
    /// Statements run while the warm-up counter is below its limit.
    warm_up: &'static str,
    /// Statements that run on every scan and read the persistent variables.
    reads: &'static str,
    /// The sinks `reads` writes.
    sinks: &'static [&'static str],
}

const SCALAR: Kind = Kind {
    top: "",
    declarations: "VAR v : DINT; END_VAR",
    warm_up: "v := 41;",
    reads: "a := v;",
    sinks: &["a"],
};

const GLOBAL: Kind = Kind {
    top: "CONFIGURATION plc
  VAR_GLOBAL g : DINT; END_VAR
  RESOURCE res ON PROCESSOR
    TASK scan(PRIORITY := 1);
    PROGRAM instance WITH scan : main;
  END_RESOURCE
END_CONFIGURATION",
    declarations: "VAR_EXTERNAL g : DINT; END_VAR",
    warm_up: "g := 41;",
    reads: "a := g;",
    sinks: &["a"],
};

const STRING: Kind = Kind {
    top: "",
    declarations: "VAR s : STRING[20]; END_VAR",
    warm_up: "s := 'hello';",
    reads: "a := LEN(s); IF s = 'hello' THEN b := 1; ELSE b := 2; END_IF;",
    sinks: &["a", "b"],
};

const WSTRING: Kind = Kind {
    top: "",
    declarations: "VAR w : WSTRING[20]; END_VAR",
    warm_up: "w := \"wide\";",
    reads: "a := LEN(w); IF w = \"wide\" THEN b := 1; ELSE b := 2; END_IF;",
    sinks: &["a", "b"],
};

const ARRAY: Kind = Kind {
    top: "",
    declarations: "VAR arr : ARRAY[1..4] OF DINT; END_VAR",
    warm_up: "arr[2] := 7; arr[4] := 9;",
    reads: "a := arr[2]; b := arr[4];",
    sinks: &["a", "b"],
};

const STRUCT: Kind = Kind {
    top: "TYPE point : STRUCT x : DINT; y : DINT; END_STRUCT; END_TYPE",
    declarations: "VAR p : point; END_VAR",
    warm_up: "p.x := 3; p.y := 4;",
    reads: "a := p.x; b := p.y;",
    sinks: &["a", "b"],
};

const STRING_ARRAY: Kind = Kind {
    top: "",
    declarations: "VAR names : ARRAY[1..3] OF STRING[10]; END_VAR",
    warm_up: "names[2] := 'abc';",
    reads: "a := LEN(names[2]);",
    sinks: &["a"],
};

/// A function block whose scalar and STRING fields are written only
/// while `set` is TRUE; the program reads them through its outputs.
const FB_FIELDS: Kind = Kind {
    top: "FUNCTION_BLOCK holder
  VAR_INPUT set : BOOL; END_VAR
  VAR_OUTPUT q : DINT; ql : DINT; END_VAR
  VAR c : DINT; s : STRING[10]; END_VAR
  IF set THEN c := 42; s := 'fbstr'; END_IF;
  q := c; ql := LEN(s);
END_FUNCTION_BLOCK",
    declarations: "VAR h : holder; END_VAR",
    warm_up: "",
    reads: "h(set := n < 3); a := h.q; b := h.ql;",
    sinks: &["a", "b"],
};

/// The source for `kind` whose edited statements are the body of a function
/// the program calls. Functions are compiled before the program body and the
/// function-block bodies, so the string temporaries the edit adds are
/// allocated before any of them.
fn edited_in_function(kind: &Kind) -> String {
    format!(
        "{top}
FUNCTION churn : DINT
  VAR_INPUT i : DINT; END_VAR
  VAR t : STRING[8]; END_VAR
  {{body}}
END_FUNCTION
{program}",
        top = kind.top,
        program = scenario_program(kind, "", "r := churn(i := 1);", ""),
    )
}

/// The source for `kind` whose edited statements are the body of a function
/// block the program instantiates and calls.
fn edited_in_fb_body(kind: &Kind) -> String {
    format!(
        "{top}
FUNCTION_BLOCK churner
  VAR_OUTPUT q : DINT; END_VAR
  VAR t : STRING[8]; END_VAR
  {{body}}
END_FUNCTION_BLOCK
{program}",
        top = kind.top,
        program = scenario_program(kind, "VAR ch : churner; END_VAR", "ch(); r := ch.q;", ""),
    )
}

/// The source for `kind` whose edited statements end the program body.
fn edited_in_program(kind: &Kind) -> String {
    format!(
        "{top}
{program}",
        top = kind.top,
        program = scenario_program(kind, "", "", "{body}"),
    )
}

/// `PROGRAM main` holding `kind`'s persistent variables plus `extra`
/// declarations. `call` runs first on every scan. `kind`'s warm-up statements
/// run for the first three scans only, then `kind`'s reads copy the state
/// into the sinks, then `tail` ends the body.
fn scenario_program(kind: &Kind, extra: &str, call: &str, tail: &str) -> String {
    format!(
        "PROGRAM main
  VAR n : DINT; r : DINT; a : DINT; b : DINT; END_VAR
  {extra}
  {declarations}
  {call}
  IF n < 3 THEN {warm_up} n := n + 1; END_IF;
  {reads}
  {tail}
END_PROGRAM
",
        declarations = kind.declarations,
        warm_up = kind.warm_up,
        reads = kind.reads,
    )
}

/// Adds a string temporary the active body had none of. The literal is long
/// enough to need a temporary wider than any string declared.
const GROW_BEFORE_FUNCTION: &str = "churn := i;";
const GROW_AFTER_FUNCTION: &str =
    "churn := LEN(CONCAT(t, 'abcdefghijklmnopqrstuvwxyz0123456789'));";
const GROW_BEFORE_FB: &str = "q := 1;";
const GROW_AFTER_FB: &str = "q := LEN(CONCAT(t, 'abcdefghijklmnopqrstuvwxyz0123456789'));";
const GROW_BEFORE_PROGRAM: &str = "r := 1;";
const GROW_AFTER_PROGRAM: &str = "r := LEN('abcdefghijklmnopqrstuvwxyz0123456789');";

/// A program whose `out` is computed by the edited statement `{body}`.
const OUT_PROGRAM: &str = "PROGRAM main
  VAR s : STRING[8]; x : DINT; out : DINT; END_VAR
  x := 3;
  {body}
END_PROGRAM
";

/// Three functions chained `f1 -> f2 -> f3`; the program calls `f1` and `f2`
/// so every function is reachable before and after the edit, and the edit
/// to `f1` lengthens the deepest call path.
const CHAIN_PROGRAM: &str = "FUNCTION f3 : DINT
  VAR_INPUT i : DINT; END_VAR
  f3 := i;
END_FUNCTION
FUNCTION f2 : DINT
  VAR_INPUT i : DINT; END_VAR
  f2 := f3(i := i) + 1;
END_FUNCTION
FUNCTION f1 : DINT
  VAR_INPUT i : DINT; END_VAR
  {body}
END_FUNCTION
PROGRAM main
  VAR out : DINT; END_VAR
  out := f1(i := 1) + f2(i := 1);
END_PROGRAM
";

/// The header facts a body decides that `validate_candidate` does not compare:
/// the sizes the candidate's buffers must have. A new such fact is added here,
/// once.
#[derive(Debug, PartialEq)]
struct Budgets {
    data_region_bytes: u32,
    max_temp_buf_bytes: u32,
    num_temp_bufs: u16,
    max_stack_depth: u16,
    max_call_depth: u16,
}

fn budgets(container: &Container) -> Budgets {
    let header = &container.header;
    Budgets {
        data_region_bytes: header.data_region_bytes,
        max_temp_buf_bytes: header.max_temp_buf_bytes,
        num_temp_bufs: header.num_temp_bufs,
        max_stack_depth: header.max_stack_depth,
        max_call_depth: header.max_call_depth,
    }
}

/// Two functions: `late`, declared first, owns a STRING local; `early`, whose
/// body is edited, is compiled before it, so a string temporary the edit adds
/// moves `late`'s local in the data region.
const FRAMES_PROGRAM: &str = "FUNCTION late : DINT
  VAR_INPUT i : DINT; END_VAR
  VAR t : STRING[8]; END_VAR
  t := 'xyz';
  late := LEN(t);
END_FUNCTION
FUNCTION early : DINT
  VAR_INPUT i : DINT; END_VAR
  VAR t : STRING[8]; END_VAR
  {body}
END_FUNCTION
PROGRAM main
  VAR r : DINT; out : DINT; END_VAR
  r := early(i := 1);
  out := late(i := 1);
END_PROGRAM
";

fn read(host: &RuntimeHost, container: &Container, names: &[&str]) -> Vec<i32> {
    names
        .iter()
        .map(|name| host.read_variable(variable_index(container, name)).unwrap())
        .collect()
}

/// An accepted body-only edit, swapped in at a scan boundary, leaves the
/// observed variables as they were and gives the computed variables the
/// values the new body computes, on buffers that fit the new body.
///
/// Each row is a source with an `{body}` marker, the edited statements before
/// and after, the sinks that read persistent state the edit does not touch
/// (non-zero after warm-up, equal across the swap), and the variables the new
/// body computes with the value it must compute.
#[rstest]
// A persistent variable of each kind, with the edit in a function: the first
// POU compiled after the program's variables have their places.
#[case::scalar(edited_in_function(&SCALAR), GROW_BEFORE_FUNCTION, GROW_AFTER_FUNCTION, SCALAR.sinks, &[])]
#[case::global(edited_in_function(&GLOBAL), GROW_BEFORE_FUNCTION, GROW_AFTER_FUNCTION, GLOBAL.sinks, &[])]
#[case::string(edited_in_function(&STRING), GROW_BEFORE_FUNCTION, GROW_AFTER_FUNCTION, STRING.sinks, &[])]
#[case::wstring(edited_in_function(&WSTRING), GROW_BEFORE_FUNCTION, GROW_AFTER_FUNCTION, WSTRING.sinks, &[])]
#[case::array(edited_in_function(&ARRAY), GROW_BEFORE_FUNCTION, GROW_AFTER_FUNCTION, ARRAY.sinks, &[])]
#[case::structure(edited_in_function(&STRUCT), GROW_BEFORE_FUNCTION, GROW_AFTER_FUNCTION, STRUCT.sinks, &[])]
#[case::string_array(edited_in_function(&STRING_ARRAY), GROW_BEFORE_FUNCTION, GROW_AFTER_FUNCTION, STRING_ARRAY.sinks, &[])]
#[case::fb_fields(edited_in_function(&FB_FIELDS), GROW_BEFORE_FUNCTION, GROW_AFTER_FUNCTION, FB_FIELDS.sinks, &[])]
// The same state with the edit in a function block body and in the program
// body.
#[case::string_edit_in_fb_body(edited_in_fb_body(&STRING), GROW_BEFORE_FB, GROW_AFTER_FB, STRING.sinks, &[])]
#[case::fb_fields_edit_in_fb_body(edited_in_fb_body(&FB_FIELDS), GROW_BEFORE_FB, GROW_AFTER_FB, FB_FIELDS.sinks, &[])]
#[case::string_edit_in_program_body(edited_in_program(&STRING), GROW_BEFORE_PROGRAM, GROW_AFTER_PROGRAM, STRING.sinks, &[])]
#[case::fb_fields_edit_in_program_body(edited_in_program(&FB_FIELDS), GROW_BEFORE_PROGRAM, GROW_AFTER_PROGRAM, FB_FIELDS.sinks, &[])]
// A function's own STRING local, which the edit moves in the data region:
// functions re-initialize their locals on every call, so nothing is carried.
#[case::function_frame_moved_by_earlier_function(
    FRAMES_PROGRAM.to_string(),
    "early := i;",
    "early := LEN(CONCAT(t, 'abcdefghijklmnopqrstuvwxyz0123456789'));",
    &[],
    &[("out", 3)]
)]
// The candidate needs more than the active application of each sized buffer.
#[case::deeper_expression_stack(
    OUT_PROGRAM.to_string(),
    "out := x;",
    "out := x + (x + (x + (x + (x + (x + (x + (x + x)))))));",
    &[],
    &[("out", 27)]
)]
#[case::deeper_call_chain(
    CHAIN_PROGRAM.to_string(),
    "f1 := 5;",
    "f1 := f2(i := 1) + 10;",
    &[],
    &[("out", 14)]
)]
#[case::first_string_temporaries(
    OUT_PROGRAM.to_string(),
    "out := LEN(s);",
    "out := LEN(CONCAT(CONCAT(s, 'ab'), 'cd'));",
    &[],
    &[("out", 4)]
)]
#[case::wider_string_temporary(
    OUT_PROGRAM.to_string(),
    "out := LEN(CONCAT(s, 'ab'));",
    "out := LEN(CONCAT(s, '012345678901234567890123456789012345678901234567890123456789'));",
    &[],
    &[("out", 60)]
)]
#[case::larger_data_region(
    OUT_PROGRAM.to_string(),
    "out := 1;",
    "out := LEN('012345678901234567890123456789012345678901234567890123456789');",
    &[],
    &[("out", 60)]
)]
fn swap_when_body_only_edit_accepted_then_state_survives_and_buffers_fit_candidate(
    #[case] template: String,
    #[case] before: &str,
    #[case] after: &str,
    #[case] observed: &[&str],
    #[case] computed: &[(&str, i32)],
) {
    let active = compile_source(&template.replace("{body}", before));
    let candidate = compile_source(&template.replace("{body}", after));
    assert_ne!(
        budgets(&active),
        budgets(&candidate),
        "the edit does not ask for any buffer the active application lacks"
    );
    let mut host = RuntimeHost::new(active).unwrap();
    host.permit_execution();

    host.run(5, || 0).unwrap();
    let observed_names: Vec<&str> = observed.to_vec();
    let before_swap = read(&host, &candidate, &observed_names);
    assert!(
        before_swap.iter().all(|value| *value != 0),
        "warm-up left an observed variable at its initial value: {before_swap:?}"
    );

    host.stage(candidate.clone()).unwrap();
    host.test().unwrap();
    host.run(1, || 0).unwrap();
    assert_eq!(
        host.status().mode,
        HostMode::Testing,
        "the swap did not apply"
    );

    let computed_names: Vec<&str> = computed.iter().map(|(name, _)| *name).collect();
    let expected: Vec<i32> = computed.iter().map(|(_, value)| *value).collect();
    assert_eq!(read(&host, &candidate, &observed_names), before_swap);
    assert_eq!(read(&host, &candidate, &computed_names), expected);

    host.run(3, || 0).unwrap();
    assert_eq!(read(&host, &candidate, &observed_names), before_swap);
    assert_eq!(read(&host, &candidate, &computed_names), expected);
}

/// Compiles the two sources of `class`, with the class's stable variable IDs
/// or without any.
fn compile_class(class: &EditClass, with_stable_ids: bool) -> (Container, Container) {
    let ids = if with_stable_ids {
        class.stable_ids
    } else {
        &[]
    };
    (
        compile_with_ids(&class.before, ids),
        compile_with_ids(&class.after, ids),
    )
}

/// What a host does with a candidate whose layout is unchanged: it accepts it
/// as an ordinary online change. The swap applies at a scan boundary, the
/// observed persistent values are those they were, the edited code runs, and
/// an untest returns to the original code without touching the state.
fn assert_ordinary_online_change(class: &EditClass, active: Container, candidate: Container) {
    let name = class.name;
    let mut host = RuntimeHost::new(active).unwrap();
    host.permit_execution();
    host.run(5, || 0).unwrap();
    let before_swap = read(&host, &candidate, class.observed);
    assert!(
        before_swap.iter().all(|value| *value != 0),
        "{name}: warm-up left an observed variable at its initial value: {before_swap:?}"
    );

    host.stage(candidate.clone()).unwrap();
    assert!(
        !host.status().migration,
        "{name}: an unchanged layout must be a plain swap, not a migration"
    );
    host.test().unwrap();
    host.run(1, || 0).unwrap();
    assert_eq!(
        host.status().mode,
        HostMode::Testing,
        "{name}: the swap did not apply"
    );

    let computed_names: Vec<&str> = class.computed.iter().map(|(name, _)| *name).collect();
    let computed: Vec<i32> = class.computed.iter().map(|(_, value)| *value).collect();
    assert_eq!(
        read(&host, &candidate, class.observed),
        before_swap,
        "{name}: state lost at the swap"
    );
    assert_eq!(
        read(&host, &candidate, &computed_names),
        computed,
        "{name}: edited code did not run"
    );

    host.run(3, || 0).unwrap();
    assert_eq!(
        read(&host, &candidate, class.observed),
        before_swap,
        "{name}: state lost after the swap"
    );
    assert_eq!(
        read(&host, &candidate, &computed_names),
        computed,
        "{name}: edited code drifted"
    );

    host.untest().unwrap();
    host.run(1, || 0).unwrap();
    assert_eq!(
        host.status().mode,
        HostMode::Normal,
        "{name}: the untest did not apply"
    );
    assert_eq!(
        read(&host, &candidate, class.observed),
        before_swap,
        "{name}: state lost at the untest"
    );
}

/// What a host does with a candidate whose layout changed: without stable
/// variable IDs it refuses it; with them, the planner decides.
fn assert_layout_change_handled(
    class: &EditClass,
    outcome: WithStableIds,
    with_stable_ids: bool,
    active: Container,
    candidate: Container,
) {
    let name = class.name;
    let mut host = RuntimeHost::new(active).unwrap();
    host.permit_execution();
    host.run(2, || 0).unwrap();

    let staged = host.stage(candidate);

    match (with_stable_ids, outcome) {
        (false, _) => assert!(
            matches!(staged, Err(OnlineChangeError::LayoutIncompatible)),
            "{name}: without stable IDs a changed layout must be refused: {staged:?}"
        ),
        (true, WithStableIds::Migration) => {
            assert!(
                staged.is_ok(),
                "{name}: the planner must accept: {staged:?}"
            );
            assert!(host.status().migration, "{name}: accepted as a plain swap");
        }
        (true, WithStableIds::Refused) => assert!(
            matches!(staged, Err(OnlineChangeError::MigrationUnsupported(_))),
            "{name}: the planner must refuse: {staged:?}"
        ),
    }
}

fn assert_host_handles_class(class: &EditClass, with_stable_ids: bool) {
    let (active, candidate) = compile_class(class, with_stable_ids);
    match class.layout {
        Layout::Unchanged => assert_ordinary_online_change(class, active, candidate),
        Layout::Changed(outcome) => {
            assert_layout_change_handled(class, outcome, with_stable_ids, active, candidate)
        }
    }
}

/// Every class of edit in the table is handled by a host as the table says,
/// without stable variable IDs: a body edit is an ordinary online change, and a
/// changed declaration of persistent state is refused.
#[test]
fn host_when_edit_class_staged_without_stable_ids_then_handled_as_the_table_says() {
    for class in edit_classes() {
        assert_host_handles_class(&class, false);
    }
}

/// The same with stable variable IDs on both containers: an unchanged layout
/// is still a plain swap with test and untest, never a migration, and a
/// changed layout goes to the planner.
#[test]
fn host_when_edit_class_staged_with_stable_ids_then_handled_as_the_table_says() {
    for class in edit_classes() {
        assert_host_handles_class(&class, true);
    }
}
