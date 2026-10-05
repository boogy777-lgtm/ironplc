//! Which diagnostic is first when an input holds two faults.
//!
//! The legacy parser finds problems in an order: text it cannot read, then
//! each token check rule in turn (`check_tokens`), then the conditional
//! pragmas, then the grammar, and it reports the first one found whatever its
//! position. The new parser ranks its errors the same way (`ranking.rs`). Each
//! test here puts two faults in one file, the one that ranks later first in the
//! text, and requires the diagnostic of the earlier-ranked fault from both
//! parsers, with the same bytes.

use super::diagnostics::{legacy_reported, new_reported, Reported};
use super::legacy::Preset;
use crate::frontend::parse_options;
use crate::options::CompilerOptions;

/// One fault, as a whole program that holds only that fault, and the code the
/// legacy parser reports for it.
struct Fault {
    name: &'static str,
    program: &'static str,
    code: &'static str,
}

/// The faults in the order the legacy parser checks them.
const FAULTS: &[Fault] = &[
    Fault {
        name: "text that is no token",
        program: "PROGRAM f0\nVAR x : INT; END_VAR\nx := 1 ? 2;\nEND_PROGRAM\n",
        code: "P0003",
    },
    Fault {
        name: "C-style comment",
        program: "PROGRAM f1\n// note\nEND_PROGRAM\n",
        code: "P0004",
    },
    Fault {
        name: "empty variable block",
        program: "PROGRAM f2\nVAR\nEND_VAR\nEND_PROGRAM\n",
        code: "P0011",
    },
    Fault {
        name: "partial-access syntax",
        program: "PROGRAM f3\nVAR x : DWORD; END_VAR\nx.%X1 := TRUE;\nEND_PROGRAM\n",
        code: "P4033",
    },
    Fault {
        name: "parenthesis string length",
        program: "PROGRAM f4\nVAR x : STRING(10); END_VAR\nEND_PROGRAM\n",
        code: "P4042",
    },
    Fault {
        name: "incomplete array",
        program: "FUNCTION f5 : INT\nVAR_INPUT a : ARRAY [*] OF INT; END_VAR\nEND_FUNCTION\n",
        code: "P4070",
    },
    Fault {
        name: "identifier spelling",
        program: "PROGRAM f6\nVAR a__b : INT; END_VAR\nEND_PROGRAM\n",
        code: "P4069",
    },
    Fault {
        name: "undefined string escape",
        program: "PROGRAM f7\nVAR s : STRING; END_VAR\ns := 'a$Qb';\nEND_PROGRAM\n",
        code: "P0012",
    },
    Fault {
        name: "stray conditional pragma",
        program: "PROGRAM f8\n{END_IF}\nEND_PROGRAM\n",
        code: "P0023",
    },
    Fault {
        name: "grammar error",
        program: "PROGRAM f9\nVAR x : INT; END_VAR\nx := ;\nEND_PROGRAM\n",
        code: "P0002",
    },
];

/// Every gated form off, the conditional pragmas on: the dialect in which
/// every fault is a fault.
fn preset() -> Preset {
    let mut legacy = CompilerOptions::default();
    legacy.set_flag_by_key("allow_pragmas", true);
    legacy.set_flag_by_key("allow_pragma_if", true);
    Preset {
        name: "strict with conditional pragmas".to_string(),
        new: parse_options(&legacy),
        legacy,
    }
}

/// The diagnostic both parsers report for `text`, which must be the same.
fn first(label: &str, text: &str) -> Reported {
    let preset = preset();
    let legacy = legacy_reported(text, &preset);
    let new = new_reported(text, &preset);
    assert!(
        legacy.is_some() && new.is_some(),
        "{label}: a parser accepts the input"
    );
    let (legacy, new) = (legacy.expect("checked above"), new.expect("checked above"));
    assert_eq!(
        (&legacy.code, legacy.start, legacy.end),
        (&new.code, new.start, new.end),
        "{label}: legacy {legacy:?}, new {new:?}"
    );
    new
}

#[test]
fn faults_when_each_alone_then_the_legacy_code_and_the_same_primary_diagnostic() {
    for fault in FAULTS {
        let reported = first(fault.name, fault.program);
        assert_eq!(reported.code, fault.code, "{}", fault.name);
    }
}

#[test]
fn diagnostic_when_two_faults_then_the_one_the_legacy_parser_checks_first_in_either_order() {
    let mut pairs = 0;
    for (index, earlier) in FAULTS.iter().enumerate() {
        for later in &FAULTS[index + 1..] {
            for (label, text) in [
                (
                    "later fault first in the text",
                    format!("{}{}", later.program, earlier.program),
                ),
                (
                    "earlier fault first in the text",
                    format!("{}{}", earlier.program, later.program),
                ),
            ] {
                let label = format!("{} and {}, {label}", earlier.name, later.name);
                let reported = first(&label, &text);
                assert_eq!(reported.code, earlier.code, "{label}");
                pairs += 1;
            }
        }
    }
    assert_eq!(pairs, FAULTS.len() * (FAULTS.len() - 1));
}

#[test]
fn diagnostic_when_three_faults_in_reverse_check_order_then_the_first_checked() {
    let text: String = FAULTS.iter().rev().map(|fault| fault.program).collect();
    let reported = first("every fault, last checked first in the text", &text);
    assert_eq!(reported.code, FAULTS[0].code);
}

/// The three spellings the identifier rule checks, each in a program.
const SPELLINGS: [(&str, &str); 3] = [
    (
        "escaped",
        "PROGRAM s0\nVAR `a b` : INT; END_VAR\nEND_PROGRAM\n",
    ),
    (
        "non-ASCII",
        "PROGRAM s1\nVAR caf\u{e9} : INT; END_VAR\nEND_PROGRAM\n",
    ),
    (
        "underscores",
        "PROGRAM s2\nVAR a__b : INT; END_VAR\nEND_PROGRAM\n",
    ),
];

#[test]
fn diagnostic_when_identifier_spellings_in_any_order_then_the_earliest_in_the_text() {
    for order in [
        [0, 1, 2],
        [0, 2, 1],
        [1, 0, 2],
        [1, 2, 0],
        [2, 0, 1],
        [2, 1, 0],
    ] {
        let text: String = order.iter().map(|index| SPELLINGS[*index].1).collect();
        let first_spelling = SPELLINGS[order[0]];
        let label = format!("{} first", first_spelling.0);
        let reported = first(&label, &text);
        let expected = ["P4067", "P4068", "P4069"][order[0]];
        assert_eq!(reported.code, expected, "{label}");
    }
}

#[test]
fn diagnostic_when_one_identifier_is_non_ascii_and_has_underscores_then_the_letters_first() {
    let text = "PROGRAM p\nVAR caf\u{e9}__x : INT; END_VAR\nEND_PROGRAM\n";
    assert_eq!(first("one identifier, two faults", text).code, "P4068");
}

#[test]
fn diagnostic_when_two_of_one_rule_then_the_earlier_in_the_text() {
    for (fault, text, second) in [
        (
            "C-style comment",
            "PROGRAM p\n// one\n// two\nEND_PROGRAM\n",
            "// two",
        ),
        (
            "text that is no token",
            "PROGRAM p\nVAR x : INT; END_VAR\nx := 1 ? 2 ! 3;\nEND_PROGRAM\n",
            "!",
        ),
        (
            "string escape",
            "PROGRAM p\nVAR s : STRING; END_VAR\ns := 'a$Qb$Zc';\nEND_PROGRAM\n",
            "$Z",
        ),
    ] {
        let reported = first(fault, text);
        let later = text.find(second).unwrap_or(0);
        assert!(reported.start < later, "{fault}: {reported:?}");
    }
}
