//! Every type with every initial value, lowered and compared to the legacy
//! parser.
//!
//! The tables hold the forms of the language one by one; this holds their
//! product: each form of type that a declaration can be written against, with
//! each kind of initial value, as a type declaration, as a member of a
//! structure and as a variable. Most pairs are not values of their type, and
//! that is what it tests: whatever the new grammar accepts, the lowering
//! either builds the object the legacy parser builds or reports a problem, and
//! it never fails inside, and never rejects what the legacy parser builds.
//!
//! The inputs the legacy parser rejects and the new parser accepts are the
//! differences of the verdict tables, in the thousands here; what is held is
//! the shape of every difference, not each one.

use super::legacy::presets;
use super::literals::{run, Case};
use super::Kind;

/// The forms of type.
const TYPES: &[&str] = &[
    "INT",
    "BOOL",
    "REAL",
    "TIME",
    "STRING",
    "WSTRING",
    "STRING[5]",
    "WSTRING(5)",
    "BYTE",
    "TOD",
    "DT",
    "MyT",
    "ANY_NUM",
    "(A, B)",
    "(A := 1, B) BYTE",
    "INT(1..5)",
    "ARRAY[1..3] OF INT",
    "ARRAY[1..2, lo..hi] OF STRING[3]",
    "ARRAY[*] OF INT",
    "REF_TO INT",
    "REFERENCE TO MyT",
    "POINTER TO ARRAY[1..2] OF INT",
    "PARAMS(2) OF INT",
    "STRUCT a : INT; END_STRUCT",
    "UNION a : INT; END_UNION",
    "Fb(x := 1)",
    "Fb()",
];

/// The kinds of initial value, each with its `:=`, and none.
const VALUES: &[&str] = &[
    "",
    " := 5",
    " := -5",
    " := 1.5",
    " := TRUE",
    " := 'a'",
    " := \"a\"",
    " := T#5s",
    " := name",
    " := a.b",
    " := 1 + 2",
    " := NULL",
    " := REF(y)",
    " := [1, 2]",
    " := [2(1), Red]",
    " := (a := 1)",
    " := (a := (b := 2), c := [1])",
    " := Red",
    " := MyT#Red",
    " := (5)",
    " := f(1)",
    " := -name",
    " := INT#5",
    " := 16#FF",
    " := TOD#10:00",
];

/// Where a type and a value are written: a declaration of a type, a member of a
/// structure, and the variables of the kinds of block.
const PLACES: &[fn(&str) -> String] = &[
    |text| format!("TYPE t : {text}; END_TYPE"),
    |text| format!("TYPE t : STRUCT a : {text}; END_STRUCT; END_TYPE"),
    |text| format!("PROGRAM p VAR x : {text}; END_VAR END_PROGRAM"),
    |text| format!("PROGRAM p VAR_TEMP x, y : {text}; END_VAR END_PROGRAM"),
    |text| format!("FUNCTION f : INT VAR x : {text}; END_VAR f := 1; END_FUNCTION"),
    |text| format!("PROGRAM p VAR_IN_OUT x : {text}; END_VAR END_PROGRAM"),
    |text| format!("PROGRAM p VAR_EXTERNAL x : {text}; END_VAR END_PROGRAM"),
];

#[test]
fn parity_when_every_type_has_every_initial_value_then_the_lowering_builds_what_the_legacy_builds_or_reports(
) {
    let mut cases = Vec::new();
    for text in TYPES {
        for value in VALUES {
            for place in PLACES {
                let source = place(&format!("{text}{value}"));
                cases.push(Case {
                    kind: Kind::Declarations,
                    key: source.clone(),
                    text: source,
                });
            }
        }
    }
    let tally = run(&cases, &presets());
    assert!(
        tally.counts(super::sites::Unit::TypeDeclaration).compared > 2_000,
        "{} type declarations compared",
        tally.counts(super::sites::Unit::TypeDeclaration).compared
    );
    assert!(
        tally.counts(super::sites::Unit::VariableInitial).compared > 3_000,
        "{} variable initial values compared",
        tally.counts(super::sites::Unit::VariableInitial).compared
    );
    // A difference of the object itself (not of what is accepted) is explained
    // by an entry; none is left over.
    let objects: Vec<&(String, String)> = tally
        .unexplained
        .iter()
        .filter(|(class, _)| !class.ends_with("[]"))
        .collect();
    assert!(objects.is_empty(), "{objects:#?}");
    // The lowering never fails inside, and never rejects what the legacy
    // parser builds.
    let broken: Vec<&(String, String)> = tally
        .unexplained
        .iter()
        .filter(|(_, what)| {
            what.contains("where legacy builds") || what.contains("P9998") || what.contains("P9999")
        })
        .collect();
    assert!(broken.is_empty(), "{broken:#?}");
}
