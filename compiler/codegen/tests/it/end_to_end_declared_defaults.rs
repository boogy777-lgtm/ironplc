//! Guard: the initial value a TYPE declaration states reaches every variable
//! of the type, for every kind of type, through every alias, in every place a
//! variable can be.
//!
//! One table. A row is a kind of type; a cell is the row, one way the
//! variable's type is reached (a column) and one place the variable lives (a
//! place). Every cell compiles a program the way `ironplcc compile` does,
//! runs one scan and compares the value the variable holds with the one the
//! cell expects, so a kind that answers "what does a variable start at" in a
//! way of its own shows as the cells of its row.
//!
//! A cell is one of three things: a value that is compared (the default), a
//! refusal with a problem code (`REFUSED`: the program is not accepted, for a
//! reason that is not this guard's), or a defect (`DEFECTS`: the cell fails
//! today). A refusal that stops being one, and a defect that stops being one,
//! fail the guard: the row has to be taken out of the list, which is how the
//! table says that support has arrived.

use crate::common::try_check_and_run;
use ironplc_parser::options::CompilerOptions;
use rstest::rstest;

/// A kind of type, with the values it is written with.
struct Kind {
    name: &'static str,
    /// Declarations the kind is made from (`TYPE` section text).
    prelude: &'static str,
    /// The declaration of the type, after `T0 :`, stating its own value.
    declaration: &'static str,
    /// After `:=`, the value an alias states, and the value a variable states.
    alias_value: &'static str,
    variable_value: &'static str,
    /// The expression that reads the value out of a variable (`{}`).
    probe: &'static str,
    /// What `probe` reads: from the type, from the alias that states a value,
    /// from the variable that states a value.
    expected_type: &'static str,
    expected_alias: &'static str,
    expected_variable: &'static str,
}

const BOOL: Kind = Kind {
    name: "BOOL",
    prelude: "",
    declaration: "BOOL := TRUE",
    alias_value: "FALSE",
    variable_value: "FALSE",
    probe: "{}",
    expected_type: "TRUE",
    expected_alias: "FALSE",
    expected_variable: "FALSE",
};
const INT: Kind = Kind {
    name: "INT",
    prelude: "",
    declaration: "INT := 5",
    alias_value: "9",
    variable_value: "3",
    probe: "{}",
    expected_type: "5",
    expected_alias: "9",
    expected_variable: "3",
};
const DINT: Kind = Kind {
    name: "DINT",
    prelude: "",
    declaration: "DINT := 100000",
    alias_value: "200000",
    variable_value: "300000",
    probe: "{}",
    expected_type: "100000",
    expected_alias: "200000",
    expected_variable: "300000",
};
const REAL: Kind = Kind {
    name: "REAL",
    prelude: "",
    declaration: "REAL := 2.5",
    alias_value: "3.5",
    variable_value: "4.5",
    probe: "{}",
    expected_type: "2.5",
    expected_alias: "3.5",
    expected_variable: "4.5",
};
const LREAL: Kind = Kind {
    name: "LREAL",
    prelude: "",
    declaration: "LREAL := 2.5",
    alias_value: "3.5",
    variable_value: "4.5",
    probe: "{}",
    expected_type: "2.5",
    expected_alias: "3.5",
    expected_variable: "4.5",
};
const TIME: Kind = Kind {
    name: "TIME",
    prelude: "",
    declaration: "TIME := T#5s",
    alias_value: "T#7s",
    variable_value: "T#9s",
    probe: "{}",
    expected_type: "T#5s",
    expected_alias: "T#7s",
    expected_variable: "T#9s",
};
const DATE: Kind = Kind {
    name: "DATE",
    prelude: "",
    declaration: "DATE := D#2024-01-02",
    alias_value: "D#2024-02-03",
    variable_value: "D#2024-03-04",
    probe: "{}",
    expected_type: "D#2024-01-02",
    expected_alias: "D#2024-02-03",
    expected_variable: "D#2024-03-04",
};
const TOD: Kind = Kind {
    name: "TOD",
    prelude: "",
    declaration: "TOD := TOD#10:00:00",
    alias_value: "TOD#11:00:00",
    variable_value: "TOD#12:00:00",
    probe: "{}",
    expected_type: "TOD#10:00:00",
    expected_alias: "TOD#11:00:00",
    expected_variable: "TOD#12:00:00",
};
const DT: Kind = Kind {
    name: "DT",
    prelude: "",
    declaration: "DT := DT#2024-01-02-10:00:00",
    alias_value: "DT#2024-02-03-11:00:00",
    variable_value: "DT#2024-03-04-12:00:00",
    probe: "{}",
    expected_type: "DT#2024-01-02-10:00:00",
    expected_alias: "DT#2024-02-03-11:00:00",
    expected_variable: "DT#2024-03-04-12:00:00",
};
const STRING: Kind = Kind {
    name: "STRING",
    prelude: "",
    declaration: "STRING := 'abc'",
    alias_value: "'def'",
    variable_value: "'ghi'",
    probe: "{}",
    expected_type: "'abc'",
    expected_alias: "'def'",
    expected_variable: "'ghi'",
};
const STRING_N: Kind = Kind {
    name: "STRING[10]",
    prelude: "",
    declaration: "STRING[10] := 'abc'",
    alias_value: "'def'",
    variable_value: "'ghi'",
    probe: "{}",
    expected_type: "'abc'",
    expected_alias: "'def'",
    expected_variable: "'ghi'",
};
const WSTRING: Kind = Kind {
    name: "WSTRING",
    prelude: "",
    declaration: "WSTRING := \"abc\"",
    alias_value: "\"def\"",
    variable_value: "\"ghi\"",
    probe: "{}",
    expected_type: "\"abc\"",
    expected_alias: "\"def\"",
    expected_variable: "\"ghi\"",
};
const WSTRING_N: Kind = Kind {
    name: "WSTRING[10]",
    prelude: "",
    declaration: "WSTRING[10] := \"abc\"",
    alias_value: "\"def\"",
    variable_value: "\"ghi\"",
    probe: "{}",
    expected_type: "\"abc\"",
    expected_alias: "\"def\"",
    expected_variable: "\"ghi\"",
};
const SUBRANGE: Kind = Kind {
    name: "subrange",
    prelude: "",
    declaration: "INT (2..10) := 5",
    alias_value: "7",
    variable_value: "9",
    probe: "{}",
    expected_type: "5",
    expected_alias: "7",
    expected_variable: "9",
};
const ENUMERATION: Kind = Kind {
    name: "enumeration",
    prelude: "",
    declaration: "(LOW, MID, HIGH, TOP) := MID",
    alias_value: "HIGH",
    variable_value: "TOP",
    probe: "{}",
    expected_type: "MID",
    expected_alias: "HIGH",
    expected_variable: "TOP",
};
const STRUCTURE: Kind = Kind {
    name: "structure",
    prelude: "",
    declaration: "STRUCT x : INT := 7; END_STRUCT",
    alias_value: "(x := 9)",
    variable_value: "(x := 3)",
    probe: "{}.x",
    expected_type: "7",
    expected_alias: "9",
    expected_variable: "3",
};
const ARRAY_INT: Kind = Kind {
    name: "array of INT",
    prelude: "",
    declaration: "ARRAY [0..2] OF INT := [1, 2, 3]",
    alias_value: "[4, 5, 6]",
    variable_value: "[7, 8, 9]",
    probe: "{}[1]",
    expected_type: "2",
    expected_alias: "5",
    expected_variable: "8",
};
const ARRAY_STRUCTURE: Kind = Kind {
    name: "array of structure",
    prelude: "S : STRUCT x : INT := 7; END_STRUCT;",
    declaration: "ARRAY [0..1] OF S",
    alias_value: "[(x := 9), (x := 9)]",
    variable_value: "[(x := 3), (x := 3)]",
    probe: "{}[1].x",
    expected_type: "7",
    expected_alias: "9",
    expected_variable: "3",
};
const ARRAY_ENUMERATION: Kind = Kind {
    name: "array of enumeration",
    prelude: "Lvl : (LOW, MID, HIGH, TOP) := MID;",
    declaration: "ARRAY [0..1] OF Lvl",
    alias_value: "[HIGH, HIGH]",
    variable_value: "[TOP, TOP]",
    probe: "{}[1]",
    expected_type: "MID",
    expected_alias: "HIGH",
    expected_variable: "TOP",
};

/// How the type of the variable is reached.
#[derive(Clone, Copy, PartialEq, Eq, Debug)]
enum Column {
    /// `v : T0`
    Direct,
    /// `T1 : T0; v : T1`
    Alias,
    /// `T2 : T1; v : T2`
    AliasOfAlias,
    /// `T1 : T0 := value; T2 : T1; v : T2`: the nearest value wins.
    AliasStatesValue,
    /// `v : T0 := value`
    VariableStatesValue,
}

const COLUMNS: [Column; 5] = [
    Column::Direct,
    Column::Alias,
    Column::AliasOfAlias,
    Column::AliasStatesValue,
    Column::VariableStatesValue,
];

impl Column {
    /// The declarations of the type, and the name the variable is declared
    /// with.
    fn types(self, kind: &Kind) -> (String, &'static str) {
        let base = format!("T0 : {};", kind.declaration);
        match self {
            Column::Direct | Column::VariableStatesValue => (base, "T0"),
            Column::Alias => (format!("{base} T1 : T0;"), "T1"),
            Column::AliasOfAlias => (format!("{base} T1 : T0; T2 : T1;"), "T2"),
            Column::AliasStatesValue => (
                format!("{base} T1 : T0 := {}; T2 : T1;", kind.alias_value),
                "T2",
            ),
        }
    }

    /// What the variable starts at, when the value is stated at the variable.
    fn stated(self, kind: &Kind) -> Option<&str> {
        (self == Column::VariableStatesValue).then_some(kind.variable_value)
    }

    fn expected(self, kind: &Kind) -> &'static str {
        match self {
            Column::Direct | Column::Alias | Column::AliasOfAlias => kind.expected_type,
            Column::AliasStatesValue => kind.expected_alias,
            Column::VariableStatesValue => kind.expected_variable,
        }
    }
}

/// Where the variable lives. `{VAR_TYPE}` is the type the variable is
/// declared with, `{STATED}` what a value stated at the variable is written
/// as, `{ACCESS}` the variable as the program reads it.
struct Place {
    name: &'static str,
    /// Declarations that wrap the type (`TYPE` section text).
    wrapper: &'static str,
    var_type: &'static str,
    stated: &'static str,
    access: &'static str,
    source: &'static str,
    options: fn() -> CompilerOptions,
}

fn default_options() -> CompilerOptions {
    CompilerOptions::default()
}

const PROGRAM: Place = Place {
    name: "program variable",
    wrapper: "",
    var_type: "{TY}",
    stated: "{VALUE}",
    access: "v",
    source: "
TYPE {TYPES} END_TYPE
PROGRAM main
  VAR r : BOOL; END_VAR
  VAR {DECL}; END_VAR
  r := ({PROBE} = {EXPECTED});
END_PROGRAM
",
    options: default_options,
};
const GLOBAL: Place = Place {
    name: "global variable",
    wrapper: "",
    var_type: "{TY}",
    stated: "{VALUE}",
    access: "v",
    source: "
TYPE {TYPES} END_TYPE
CONFIGURATION config
  VAR_GLOBAL {DECL}; END_VAR
  RESOURCE resource1 ON PLC
    TASK plc_task(INTERVAL := T#100ms, PRIORITY := 1);
    PROGRAM plc_task_instance WITH plc_task : main;
  END_RESOURCE
END_CONFIGURATION
PROGRAM main
  VAR_EXTERNAL {PLAIN}; END_VAR
  VAR r : BOOL; END_VAR
  r := ({PROBE} = {EXPECTED});
END_PROGRAM
",
    options: default_options,
};
const RETAIN: Place = Place {
    name: "RETAIN variable",
    wrapper: "",
    var_type: "{TY}",
    stated: "{VALUE}",
    access: "v",
    source: "
TYPE {TYPES} END_TYPE
PROGRAM main
  VAR r : BOOL; END_VAR
  VAR RETAIN {DECL}; END_VAR
  r := ({PROBE} = {EXPECTED});
END_PROGRAM
",
    options: default_options,
};
const FB_VARIABLE: Place = Place {
    name: "function block variable",
    wrapper: "",
    var_type: "{TY}",
    stated: "{VALUE}",
    access: "v",
    source: "
TYPE {TYPES} END_TYPE
FUNCTION_BLOCK fb
  VAR_OUTPUT ok : BOOL; END_VAR
  VAR {DECL}; END_VAR
  ok := ({PROBE} = {EXPECTED});
END_FUNCTION_BLOCK
PROGRAM main
  VAR r : BOOL; i : fb; j : fb; END_VAR
  i();
  j();
  r := i.ok AND j.ok;
END_PROGRAM
",
    options: default_options,
};
const FB_INPUT: Place = Place {
    name: "function block VAR_INPUT",
    wrapper: "",
    var_type: "{TY}",
    stated: "{VALUE}",
    access: "v",
    source: "
TYPE {TYPES} END_TYPE
FUNCTION_BLOCK fb
  VAR_INPUT {DECL}; END_VAR
  VAR_OUTPUT ok : BOOL; END_VAR
  ok := ({PROBE} = {EXPECTED});
END_FUNCTION_BLOCK
PROGRAM main
  VAR r : BOOL; i : fb; END_VAR
  i();
  r := i.ok;
END_PROGRAM
",
    options: default_options,
};
const FB_OUTPUT: Place = Place {
    name: "function block VAR_OUTPUT",
    wrapper: "",
    var_type: "{TY}",
    stated: "{VALUE}",
    access: "v",
    source: "
TYPE {TYPES} END_TYPE
FUNCTION_BLOCK fb
  VAR_OUTPUT {DECL}; ok : BOOL; END_VAR
  ok := ({PROBE} = {EXPECTED});
END_FUNCTION_BLOCK
PROGRAM main
  VAR r : BOOL; i : fb; END_VAR
  i();
  r := i.ok;
END_PROGRAM
",
    options: default_options,
};
const FUNCTION_LOCAL: Place = Place {
    name: "function local",
    wrapper: "",
    var_type: "{TY}",
    stated: "{VALUE}",
    access: "v",
    source: "
TYPE {TYPES} END_TYPE
FUNCTION f : BOOL
  VAR_INPUT unused : INT; END_VAR
  VAR {DECL}; END_VAR
  f := ({PROBE} = {EXPECTED});
END_FUNCTION
PROGRAM main
  VAR r : BOOL; END_VAR
  r := f(0);
END_PROGRAM
",
    options: default_options,
};
const STRUCTURE_MEMBER: Place = Place {
    name: "member of a structure",
    wrapper: "Holder : STRUCT m : {TY}; END_STRUCT;",
    var_type: "Holder",
    stated: "(m := {VALUE})",
    access: "v.m",
    source: PROGRAM.source,
    options: default_options,
};
const ARRAY_ELEMENT: Place = Place {
    name: "element of an array",
    wrapper: "",
    var_type: "ARRAY [0..1] OF {TY}",
    stated: "[{VALUE}, {VALUE}]",
    access: "v[1]",
    source: PROGRAM.source,
    options: default_options,
};

const PLACES: [&Place; 9] = [
    &PROGRAM,
    &GLOBAL,
    &RETAIN,
    &FB_VARIABLE,
    &FB_INPUT,
    &FB_OUTPUT,
    &FUNCTION_LOCAL,
    &STRUCTURE_MEMBER,
    &ARRAY_ELEMENT,
];

/// A group of cells: every kind, column and place listed.
struct Cells {
    kinds: &'static [&'static str],
    columns: &'static [Column],
    places: &'static [&'static str],
}

impl Cells {
    fn contains(&self, kind: &Kind, column: Column, place: &Place) -> bool {
        self.kinds.contains(&kind.name)
            && self.columns.contains(&column)
            && self.places.contains(&place.name)
    }
}

const ALL_COLUMNS: &[Column] = &COLUMNS;
const TYPE_COLUMNS: &[Column] = &[Column::Direct, Column::Alias, Column::AliasOfAlias];
const ALL_PLACES: &[&str] = &[
    PROGRAM.name,
    GLOBAL.name,
    RETAIN.name,
    FB_VARIABLE.name,
    FB_INPUT.name,
    FB_OUTPUT.name,
    FUNCTION_LOCAL.name,
    STRUCTURE_MEMBER.name,
    ARRAY_ELEMENT.name,
];
const POU_LOCALS: &[&str] = &[
    FB_VARIABLE.name,
    FB_INPUT.name,
    FB_OUTPUT.name,
    FUNCTION_LOCAL.name,
];
const SCALAR_KINDS: &[&str] = &[
    BOOL.name,
    INT.name,
    DINT.name,
    REAL.name,
    LREAL.name,
    TIME.name,
    DATE.name,
    TOD.name,
    DT.name,
    STRING.name,
    STRING_N.name,
    WSTRING.name,
    WSTRING_N.name,
    SUBRANGE.name,
    ENUMERATION.name,
];
const ARRAY_KINDS: &[&str] = &[ARRAY_INT.name, ARRAY_STRUCTURE.name, ARRAY_ENUMERATION.name];

/// The cells whose program is not accepted, with the problem code that says
/// so. The first group that holds a cell names its code. Support arriving
/// for a cell takes it out of this list.
const REFUSED: &[(Cells, &str)] = &[
    // An array of a named type that is not a structure has no element type
    // the code generator can lay out.
    (
        Cells {
            kinds: SCALAR_KINDS,
            columns: ALL_COLUMNS,
            places: &[ARRAY_ELEMENT.name],
        },
        "P9999",
    ),
    // A value an array type states is refused for a type that names another.
    (
        Cells {
            kinds: &[ARRAY_INT.name, ARRAY_ENUMERATION.name],
            columns: &[Column::AliasStatesValue],
            places: ALL_PLACES,
        },
        "P4022",
    ),
    // The grammar of a type or a variable does not take a structure or an
    // array of structures as a value.
    (
        Cells {
            kinds: &[ARRAY_STRUCTURE.name],
            columns: &[Column::AliasStatesValue, Column::VariableStatesValue],
            places: ALL_PLACES,
        },
        "P0002",
    ),
    (
        Cells {
            kinds: &[
                ARRAY_INT.name,
                ARRAY_STRUCTURE.name,
                ARRAY_ENUMERATION.name,
                STRUCTURE.name,
            ],
            columns: &[Column::VariableStatesValue],
            places: &[GLOBAL.name, ARRAY_ELEMENT.name],
        },
        "P0002",
    ),
    // A structure or an array a function block or a function declares is not
    // laid out.
    (
        Cells {
            kinds: &[
                ARRAY_INT.name,
                ARRAY_STRUCTURE.name,
                ARRAY_ENUMERATION.name,
                STRUCTURE.name,
            ],
            columns: ALL_COLUMNS,
            places: POU_LOCALS,
        },
        "P9999",
    ),
    // An array of arrays, and an array of enumerations outside a structure.
    (
        Cells {
            kinds: ARRAY_KINDS,
            columns: TYPE_COLUMNS,
            places: &[ARRAY_ELEMENT.name],
        },
        "P9999",
    ),
    (
        Cells {
            kinds: &[ARRAY_ENUMERATION.name],
            columns: TYPE_COLUMNS,
            places: &[PROGRAM.name, GLOBAL.name, RETAIN.name],
        },
        "P9999",
    ),
    (
        Cells {
            kinds: &[ARRAY_ENUMERATION.name],
            columns: &[Column::VariableStatesValue],
            places: &[PROGRAM.name, RETAIN.name, STRUCTURE_MEMBER.name],
        },
        "P9999",
    ),
];

/// The cells that fail today: the program is accepted and does not hold the
/// value the cell expects, or it is accepted and crashes the compiler. A cell
/// that stops failing takes its group out of this list.
const DEFECTS: &[Cells] = &[
    // A member of a string type with a length loses the value.
    Cells {
        kinds: &[STRING_N.name, WSTRING_N.name],
        columns: ALL_COLUMNS,
        places: &[STRUCTURE_MEMBER.name],
    },
    // The elements of an array of enumerations in a structure start at the
    // first value of the enumeration.
    Cells {
        kinds: &[ARRAY_ENUMERATION.name],
        columns: TYPE_COLUMNS,
        places: &[STRUCTURE_MEMBER.name],
    },
    // The elements of an array of a structure alias that states members start
    // at the members of the structure it names.
    Cells {
        kinds: &[STRUCTURE.name],
        columns: &[Column::AliasStatesValue],
        places: &[ARRAY_ELEMENT.name],
    },
];

fn program_of(kind: &Kind, column: Column, place: &Place) -> (String, String) {
    let (types, ty) = column.types(kind);
    let value = column.stated(kind);
    let var_type = place.var_type.replace("{TY}", ty);
    let plain = format!("v : {var_type}");
    let decl = match value {
        Some(value) => format!("{plain} := {}", place.stated.replace("{VALUE}", value)),
        None => plain.clone(),
    };
    let wrapper = place.wrapper.replace("{TY}", ty);
    let source = place
        .source
        .replace("{TYPES}", &format!("{} {types} {wrapper}", kind.prelude))
        .replace("{DECL}", &decl)
        .replace("{PLAIN}", &plain)
        .replace("{PROBE}", &kind.probe.replace("{}", place.access))
        .replace("{EXPECTED}", column.expected(kind));
    (source, decl)
}

/// What a cell does: `Ok(true)` when the variable holds the value the cell
/// expects, `Err` with the code of the first diagnostic when the program is
/// not accepted.
fn run_cell(kind: &Kind, column: Column, place: &Place) -> Result<bool, String> {
    let (source, _) = program_of(kind, column, place);
    let (container, bufs) = try_check_and_run(&source, &(place.options)())
        .map_err(|d| format!("{}: {}", d.code, d.primary.message))?;
    let debug = container.debug_section.as_ref().unwrap();
    let result = debug.var_names.iter().find(|v| v.name == "r").unwrap();
    Ok(bufs.vars[result.var_index.raw() as usize].as_i32() == 1)
}

fn is_refused(kind: &Kind, column: Column, place: &Place) -> Option<&'static str> {
    REFUSED
        .iter()
        .find(|(cells, _)| cells.contains(kind, column, place))
        .map(|(_, code)| *code)
}

fn is_defect(kind: &Kind, column: Column, place: &Place) -> bool {
    DEFECTS
        .iter()
        .any(|cells| cells.contains(kind, column, place))
}

/// The cells of a row whose outcome is not the one the table states.
fn mismatches(kind: &Kind) -> Vec<String> {
    let mut found = Vec::new();
    for column in COLUMNS {
        for place in PLACES {
            let outcome = run_cell(kind, column, place);
            let cell = format!("{} / {column:?} / {}", kind.name, place.name);
            match (
                is_refused(kind, column, place),
                is_defect(kind, column, place),
                &outcome,
            ) {
                (Some(code), _, Err(got)) if got.starts_with(code) => {}
                (Some(code), _, got) => {
                    found.push(format!("{cell}: expected refusal {code}, got {got:?}"))
                }
                (None, true, Ok(true)) => found.push(format!(
                    "{cell}: listed as a defect and now holds the value"
                )),
                (None, true, _) => {}
                (None, false, Ok(true)) => {}
                (None, false, got) => {
                    found.push(format!("{cell}: expected the value, got {got:?}"))
                }
            }
        }
    }
    found
}

#[rstest]
#[case::bool(&BOOL)]
#[case::int(&INT)]
#[case::dint(&DINT)]
#[case::real(&REAL)]
#[case::lreal(&LREAL)]
#[case::time(&TIME)]
#[case::date(&DATE)]
#[case::time_of_day(&TOD)]
#[case::date_and_time(&DT)]
#[case::string(&STRING)]
#[case::string_with_length(&STRING_N)]
#[case::wstring(&WSTRING)]
#[case::wstring_with_length(&WSTRING_N)]
#[case::subrange(&SUBRANGE)]
#[case::enumeration(&ENUMERATION)]
#[case::structure(&STRUCTURE)]
#[case::array_of_int(&ARRAY_INT)]
#[case::array_of_structure(&ARRAY_STRUCTURE)]
#[case::array_of_enumeration(&ARRAY_ENUMERATION)]
fn declared_default_when_variable_of_type_then_starts_at_the_value_the_table_states(
    #[case] kind: &Kind,
) {
    assert_eq!(mismatches(kind), Vec::<String>::new());
}
