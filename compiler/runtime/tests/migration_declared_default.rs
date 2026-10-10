//! A variable that an edit adds is not migrated from anywhere: it holds what
//! the candidate's init image gives it, so the value its type declares has to
//! be in that image for every kind of type, and through every alias.
//!
//! The candidate adds a variable of an alias of an enumeration, of an alias of
//! an elementary type, of an array type and of a string type, each declared
//! with a default, and the test reads them after one scan of the migrated
//! candidate.

// Test-target boundary: the workspace denies panicking constructs in
// production code; tests assert by panicking, so they are exempt here.

mod common;

use common::{compile_with_ids, compile_with_uid_keys, variable_index};
use ironplc_runtime::RuntimeHost;

const TYPES: &str = "
TYPE
  Level : (LOW, MID, HIGH, TOP) := MID;
  LevelAlias : Level;
  Score : INT := 5;
  ScoreAlias : Score;
  Row : ARRAY [0..2] OF INT := [1, 2, 3];
  Label : STRING[10] := 'abc';
  LabelAlias : Label;
END_TYPE
";

#[test]
fn run_when_variables_of_types_with_declared_defaults_added_then_they_start_at_the_defaults() {
    let base = compile_with_ids(
        &format!(
            "{TYPES}
PROGRAM main
  VAR
    Counter : DINT;
  END_VAR
  Counter := Counter + 1;
END_PROGRAM
"
        ),
        &[("Counter", 1)],
    );
    let mut host = RuntimeHost::new(base).unwrap();
    host.permit_execution();
    host.run(3, || 0).unwrap();

    let candidate = compile_with_ids(
        &format!(
            "{TYPES}
PROGRAM main
  VAR
    Counter : DINT;
    Chosen : LevelAlias;
    Points : ScoreAlias;
    Values : Row;
    Title : LabelAlias;
    ChosenIsMid : BOOL;
    ValuesAreDeclared : BOOL;
    TitleIsDeclared : BOOL;
  END_VAR
  Counter := Counter + 1;
  ChosenIsMid := (MID = Chosen);
  ValuesAreDeclared := (2 = Values[1]);
  TitleIsDeclared := ('abc' = Title);
END_PROGRAM
"
        ),
        &[
            ("Counter", 1),
            ("Chosen", 2),
            ("Points", 3),
            ("Values", 4),
            ("Title", 5),
            ("ChosenIsMid", 6),
            ("ValuesAreDeclared", 7),
            ("TitleIsDeclared", 8),
        ],
    );
    let read = |host: &RuntimeHost, name: &str| {
        host.read_variable(variable_index(&candidate, name))
            .unwrap()
    };
    let counter = variable_index(&candidate, "Counter");

    host.stage(candidate.clone()).unwrap();
    assert!(host.status().migration);
    host.test().unwrap();
    host.run(1, || 0).unwrap();

    assert_eq!(host.read_variable(counter).unwrap(), 4);
    // MID is the second value of the enumeration.
    assert_eq!(read(&host, "Chosen"), 1);
    assert_eq!(read(&host, "Points"), 5);
    assert_eq!(read(&host, "ChosenIsMid"), 1);
    assert_eq!(read(&host, "ValuesAreDeclared"), 1);
    assert_eq!(read(&host, "TitleIsDeclared"), 1);
}

#[test]
fn run_when_function_block_field_of_type_with_declared_default_added_then_it_starts_at_the_default()
{
    let base = compile_with_uid_keys(
        &format!(
            "{TYPES}
FUNCTION_BLOCK Accumulator
  VAR_INPUT step : DINT; END_VAR
  VAR total : DINT; END_VAR
  total := total + step;
END_FUNCTION_BLOCK

PROGRAM main
  VAR acc : Accumulator; END_VAR
  acc(step := 1);
END_PROGRAM
"
        ),
        &[
            ("main", "acc", 1),
            ("Accumulator", "step", 101),
            ("Accumulator", "total", 102),
        ],
    );
    let mut host = RuntimeHost::new(base).unwrap();
    host.permit_execution();
    host.run(2, || 0).unwrap();

    let candidate = compile_with_uid_keys(
        &format!(
            "{TYPES}
FUNCTION_BLOCK Accumulator
  VAR_INPUT step : DINT; END_VAR
  VAR
    chosen : LevelAlias;
    points : ScoreAlias;
    total : DINT;
  END_VAR
  total := total + step;
END_FUNCTION_BLOCK

PROGRAM main
  VAR acc : Accumulator; END_VAR
  acc(step := 1);
END_PROGRAM
"
        ),
        &[
            ("main", "acc", 1),
            ("Accumulator", "step", 101),
            ("Accumulator", "chosen", 103),
            ("Accumulator", "points", 104),
            ("Accumulator", "total", 102),
        ],
    );
    let total = variable_index(&candidate, "total");
    let chosen = variable_index(&candidate, "chosen");
    let points = variable_index(&candidate, "points");

    host.stage(candidate).unwrap();
    assert!(host.status().migration);
    host.test().unwrap();
    host.run(1, || 0).unwrap();

    assert_eq!(host.read_variable(total).unwrap(), 3);
    // MID is the second value of the enumeration.
    assert_eq!(host.read_variable(chosen).unwrap(), 1);
    assert_eq!(host.read_variable(points).unwrap(), 5);
}
