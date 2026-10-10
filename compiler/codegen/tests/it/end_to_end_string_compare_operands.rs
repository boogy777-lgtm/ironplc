//! Guard: a string comparison gives the same answer whatever kind of operand
//! is on either side of it.
//!
//! One table. A row is a kind of operand (a variable, a member of a
//! structure, ...); a cell is the kind on the left, the kind on the right, the
//! type of the strings (`STRING`, `STRING[10]`, `WSTRING`, `WSTRING[10]`) and a
//! pair of values. Every cell compiles a program the way `ironplcc compile`
//! does, runs one scan and compares the six results of `=`, `<>`, `<`, `>`,
//! `<=` and `>=`, and the answer of a condition `IF left = right`, with the
//! ordering of the two values, so an operand kind that
//! the comparison loads in a way of its own shows as the cells of its row, on
//! the side where it does.
//!
//! A cell whose program is not accepted is listed in `REFUSED` with the
//! problem code that says so; support arriving for it takes it out of the
//! list, which fails the guard until it is.

use crate::common::checked::try_check_and_run;
use ironplc_parser::options::CompilerOptions;
use rstest::rstest;
use std::cmp::Ordering;

/// A kind of operand, written as text with `{P}` (a prefix that keeps the
/// names of the left and the right operand apart), `{T}` (the type of the
/// string) and `{V}` (the string literal it holds).
struct Operand {
    name: &'static str,
    /// Declarations outside the program: types, function blocks, functions.
    types: &'static str,
    /// Declarations in the `VAR` block of the program.
    decl: &'static str,
    /// The operand as an expression.
    expr: &'static str,
}

const VARIABLE: Operand = Operand {
    name: "variable",
    types: "",
    decl: "{P}v : {T} := {V};",
    expr: "{P}v",
};
const MEMBER: Operand = Operand {
    name: "member",
    types: "TYPE {P}Rec : STRUCT m : {T} := {V}; END_STRUCT; END_TYPE",
    decl: "{P}s : {P}Rec;",
    expr: "{P}s.m",
};
const NESTED_MEMBER: Operand = Operand {
    name: "nested member",
    types: "TYPE {P}Inner : STRUCT m : {T} := {V}; END_STRUCT; END_TYPE \
            TYPE {P}Outer : STRUCT i : {P}Inner; END_STRUCT; END_TYPE",
    decl: "{P}o : {P}Outer;",
    expr: "{P}o.i.m",
};
const ELEMENT_MEMBER: Operand = Operand {
    name: "member of an array element",
    types: "TYPE {P}Rec : STRUCT m : {T} := {V}; END_STRUCT; END_TYPE",
    decl: "{P}r : ARRAY[1..2] OF {P}Rec;",
    expr: "{P}r[2].m",
};
const ARRAY_ELEMENT: Operand = Operand {
    name: "array element",
    types: "",
    decl: "{P}a : ARRAY[1..2] OF {T} := [{V}, {V}];",
    expr: "{P}a[2]",
};
const FIELD: Operand = Operand {
    name: "function block field",
    types: "FUNCTION_BLOCK {P}Blk VAR_OUTPUT s : {T} := {V}; END_VAR END_FUNCTION_BLOCK",
    decl: "{P}f : {P}Blk;",
    expr: "{P}f.s",
};
const FUNCTION_RESULT: Operand = Operand {
    name: "function result",
    types: "FUNCTION {P}Fun : {T} {P}Fun := {V}; END_FUNCTION",
    decl: "",
    expr: "{P}Fun()",
};
const LITERAL: Operand = Operand {
    name: "literal",
    types: "",
    decl: "",
    expr: "{V}",
};

const OPERANDS: [&Operand; 8] = [
    &VARIABLE,
    &MEMBER,
    &NESTED_MEMBER,
    &ELEMENT_MEMBER,
    &ARRAY_ELEMENT,
    &FIELD,
    &FUNCTION_RESULT,
    &LITERAL,
];

/// A pair of values: the same, different with the same length, and different
/// in length with the longer one on either side.
const PAIRS: [(&str, &str); 4] = [("abc", "abc"), ("abc", "abd"), ("ab", "abc"), ("abc", "ab")];

/// The operators the program compares with, in the order of the results.
const OPERATORS: [&str; 6] = ["=", "<>", "<", ">", "<=", ">="];

/// The cells whose program is not accepted, as the kinds on the left and the
/// right (`None` for any), with the problem code of the first diagnostic.
const REFUSED: &[(Option<&str>, Option<&str>, &str)] = &[];

fn render(template: &str, side: &str, ty: &str, value: &str) -> String {
    template
        .replace("{P}", side)
        .replace("{T}", ty)
        .replace("{V}", value)
}

fn literal(text: &str, wide: bool) -> String {
    let quote = if wide { '"' } else { '\'' };
    format!("{quote}{text}{quote}")
}

fn program_of(left: &Operand, right: &Operand, ty: &str, wide: bool, pair: (&str, &str)) -> String {
    let (left_value, right_value) = (literal(pair.0, wide), literal(pair.1, wide));
    let left_expr = render(left.expr, "l", ty, &left_value);
    let right_expr = render(right.expr, "r", ty, &right_value);
    let compares: String = OPERATORS
        .iter()
        .enumerate()
        .map(|(i, op)| format!("r{} := {left_expr} {op} {right_expr};", i + 1))
        .collect();
    format!(
        "{} {} PROGRAM main VAR r1 : BOOL; r2 : BOOL; r3 : BOOL; r4 : BOOL; r5 : BOOL; \
         r6 : BOOL; r7 : BOOL; {} {} END_VAR {compares}          IF {left_expr} = {right_expr} THEN r7 := TRUE; END_IF; END_PROGRAM",
        render(left.types, "l", ty, &left_value),
        render(right.types, "r", ty, &right_value),
        render(left.decl, "l", ty, &left_value),
        render(right.decl, "r", ty, &right_value),
    )
}

/// What the six comparisons answer for the pair, in the order of `OPERATORS`,
/// and then what a condition `IF left = right` answers.
fn expected(pair: (&str, &str)) -> [i32; 7] {
    let order = pair.0.cmp(pair.1);
    [
        order == Ordering::Equal,
        order != Ordering::Equal,
        order == Ordering::Less,
        order == Ordering::Greater,
        order != Ordering::Greater,
        order != Ordering::Less,
        order == Ordering::Equal,
    ]
    .map(i32::from)
}

fn refusal_of(left: &Operand, right: &Operand) -> Option<&'static str> {
    REFUSED
        .iter()
        .find(|(l, r, _)| l.is_none_or(|l| l == left.name) && r.is_none_or(|r| r == right.name))
        .map(|(_, _, code)| *code)
}

/// What a cell does: the seven results, or the code and text of the first
/// diagnostic when the program is not accepted.
fn run_cell(
    left: &Operand,
    right: &Operand,
    ty: &str,
    wide: bool,
    pair: (&str, &str),
) -> Result<[i32; 7], String> {
    let source = program_of(left, right, ty, wide, pair);
    let (_, bufs) = try_check_and_run(&source, &CompilerOptions::default())
        .map_err(|d| format!("{}: {}", d.code, d.primary.message))?;
    Ok([0, 1, 2, 3, 4, 5, 6].map(|i| bufs.vars[i].as_i32()))
}

/// The cells of a type whose outcome is not the one the table states.
fn mismatches(ty: &str, wide: bool) -> Vec<String> {
    let mut found = Vec::new();
    for left in OPERANDS {
        for right in OPERANDS {
            for pair in PAIRS {
                let cell = format!("{} = {} / {ty} / {pair:?}", left.name, right.name);
                let outcome = run_cell(left, right, ty, wide, pair);
                match (refusal_of(left, right), &outcome) {
                    (Some(code), Err(got)) if got.starts_with(code) => {}
                    (Some(code), got) => {
                        found.push(format!("{cell}: expected refusal {code}, got {got:?}"))
                    }
                    (None, Ok(got)) if *got == expected(pair) => {}
                    (None, got) => found.push(format!(
                        "{cell}: expected {:?}, got {got:?}",
                        expected(pair)
                    )),
                }
            }
        }
    }
    found
}

#[rstest]
#[case::string("STRING", false)]
#[case::string_with_length("STRING[10]", false)]
#[case::wstring("WSTRING", true)]
#[case::wstring_with_length("WSTRING[10]", true)]
fn string_compare_when_operands_of_any_kind_then_answers_by_the_values(
    #[case] ty: &str,
    #[case] wide: bool,
) {
    let found = mismatches(ty, wide);
    assert_eq!(found, Vec::<String>::new());
}
