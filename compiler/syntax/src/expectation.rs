//! What a syntax error says was expected, and what it found.
//!
//! An "expected ..." error means that at this position one of a few things
//! was to stand: a token, one of several tokens, one of several words, or a
//! part of the grammar (a name, an expression, the lower bound of a range). The
//! error carries that as data, an [`Expectation`] of an [`Expected`] and the
//! [`Found`] input there, and its text, `found `END_IF`, expected `;``, is
//! derived from it by one template row of `problem-texts.csv`. What is expected
//! is:
//!
//! - a token is named by the spelling its kind is declared with
//!   ([`SyntaxKind::spelling`]), so `p.expect(K::Semicolon)` is all a rule says;
//! - a set of tokens is named by the spellings of its kinds, in the order the
//!   kinds are declared ([`KindSet::iter`]), which is not a fact of the set;
//! - a word that is not a keyword (`GET`, `PRIORITY`) is named by the word;
//! - a part of the grammar is an [`Item`], and the phrase for it is the one
//!   exhaustive `match` of [`Item::phrase`].
//!
//! Expecting a new part of the grammar is one variant of [`Item`] and its
//! phrase; expecting a token is the token. Neither is a row of the dictionary.
//! What was found is never written by a rule: the parser takes it from the
//! next token and the lowering from the node it rejects, so every expectation
//! says it.

use crate::found::Found;
use crate::kind_set::KindSet;
use crate::syntax_kind::SyntaxKind;
use core::fmt;
use ironplc_dsl::construct::DurationUnit;
use ironplc_problems::{text, Text};
use std::borrow::Cow;

/// What was expected at the position of an error, and what was found there.
#[derive(Debug, Clone, PartialEq, Eq)]
pub(crate) struct Expectation {
    found: Found,
    expected: Expected,
}

/// The things that can be expected; see the module documentation. A rule
/// names one of them (a [`SyntaxKind`], a [`KindSet`], an [`Item`] or
/// [`Expected::words`]) and the position says what was found.
#[derive(Debug, Clone, PartialEq, Eq)]
pub(crate) enum Expected {
    /// One token or keyword.
    Token(SyntaxKind),
    /// Any one token or keyword of a set.
    OneOf(KindSet),
    /// Any one of several words that are read as names, not as keywords.
    Words(Vec<&'static str>),
    /// A part of the grammar.
    Item(Item),
}

impl Expected {
    /// Any one of `words`, in the order given.
    pub(crate) fn words(words: impl IntoIterator<Item = &'static str>) -> Expected {
        Expected::Words(words.into_iter().collect())
    }
}

impl Expectation {
    /// That `expected` was to stand where `found` stands.
    pub(crate) fn new(found: Found, expected: impl Into<Expected>) -> Expectation {
        Expectation {
            found,
            expected: expected.into(),
        }
    }

    /// The text of the error that says this was expected and that was found.
    pub(crate) fn text(&self) -> Text {
        text::syntax_error_found_expected(&self.found, &self.expected)
    }
}

impl From<SyntaxKind> for Expected {
    fn from(kind: SyntaxKind) -> Expected {
        debug_assert!(kind.spelling().is_some(), "{kind:?} has no spelling");
        Expected::Token(kind)
    }
}

impl From<KindSet> for Expected {
    fn from(kinds: KindSet) -> Expected {
        debug_assert!(
            kinds.iter().all(|kind| kind.spelling().is_some()),
            "{kinds:?} holds a kind that has no spelling"
        );
        Expected::OneOf(kinds)
    }
}

impl From<Item> for Expected {
    fn from(item: Item) -> Expected {
        Expected::Item(item)
    }
}

impl fmt::Display for Expectation {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        f.write_str(self.text().message())
    }
}

impl fmt::Display for Expected {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            Expected::Token(kind) => list(f, kind.spelling()),
            Expected::OneOf(kinds) => list(f, kinds.iter().filter_map(SyntaxKind::spelling)),
            Expected::Words(words) => list(f, words.iter().copied()),
            Expected::Item(item) => f.write_str(&item.phrase()),
        }
    }
}

/// Writes `names` as code: `A`, `A` or `B`, `A`, `B` or `C`.
fn list<'a>(f: &mut fmt::Formatter<'_>, names: impl IntoIterator<Item = &'a str>) -> fmt::Result {
    let names: Vec<&str> = names.into_iter().collect();
    for (index, name) in names.iter().enumerate() {
        let separator = if index == 0 {
            ""
        } else if index + 1 == names.len() {
            " or "
        } else {
            ", "
        };
        write!(f, "{separator}`{name}`")?;
    }
    Ok(())
}

/// A part of the grammar that a rule expected and did not find.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub(crate) enum Item {
    /// "0, 1 or a boolean word".
    BooleanValue,
    /// "a case selector".
    CaseSelector,
    /// "a constant".
    Constant,
    /// "a declaration".
    Declaration,
    /// "a direct address".
    DirectAddress,
    /// "a duration".
    Duration,
    /// "a duration or the name of a variable".
    DurationOrVariableName,
    /// "a duration value".
    DurationValue,
    /// "a function block call".
    FunctionBlockCall,
    /// "a global variable".
    GlobalVariable,
    /// "a label name".
    LabelName,
    /// "a literal".
    Literal,
    /// "a member".
    Member,
    /// "a member name".
    MemberName,
    /// "a name".
    Name,
    /// "a number in the duration".
    NumberInDuration,
    /// "a resource or a variable block".
    ResourceOrVariableBlock,
    /// "a statement".
    Statement,
    /// "a task or a program".
    TaskOrProgram,
    /// "a task property".
    TaskProperty,
    /// "a type declaration".
    TypeDeclaration,
    /// "a type name".
    TypeName,
    /// "a variable".
    Variable,
    /// "an action association".
    ActionAssociation,
    /// "an expression".
    Expression,
    /// "an integer".
    Integer,
    /// "an integer or the name of a constant".
    IntegerOrConstantName,
    /// "the end of the structure".
    StructureEnd,
    /// "the loop variable".
    LoopVariable,
    /// "the lower bound of a range".
    RangeLowerBound,
    /// "the name of a step".
    StepName,
    /// "the name of a task".
    TaskName,
    /// "the name of a variable, not an address".
    VariableNameNotAddress,
    /// "the name of an action".
    ActionName,
    /// "the path of a variable".
    VariablePath,
    /// "the path `resource.program.variable`".
    InstancePath,
    /// "the priority".
    Priority,
    /// "the type of the resource".
    ResourceType,
    /// "the upper bound of a range".
    RangeUpperBound,
    /// "the upper bound of the range".
    CaseRangeUpperBound,
    /// A duration unit, as the one table of units spells them.
    DurationUnit,
    /// "digits after `10#`".
    DigitsAfterBase,
    /// "an integer value".
    IntegerValue,
    /// "a real value".
    RealValue,
    /// "a bit-string value".
    BitStringValue,
    /// "a double-quoted string".
    DoubleQuotedString,
    /// "a single-quoted string".
    SingleQuotedString,
    /// "an hour".
    Hour,
    /// "a minute".
    Minute,
    /// "a second".
    Second,
    /// "a year".
    Year,
    /// "a month".
    Month,
    /// "a day".
    Day,
    /// "a constant or a global variable".
    ConstantOrGlobalVariable,
    /// "a constant, an enumeration value or an address".
    ConstantEnumerationValueOrAddress,
    /// "a global variable or an address".
    GlobalVariableOrAddress,
    /// "the end of the accessor".
    AccessorEnd,
}

impl Item {
    /// What the item is called after "expected".
    pub(crate) fn phrase(self) -> Cow<'static, str> {
        match self {
            Item::BooleanValue => Cow::Borrowed("0, 1 or a boolean word"),
            Item::CaseSelector => Cow::Borrowed("a case selector"),
            Item::Constant => Cow::Borrowed("a constant"),
            Item::Declaration => Cow::Borrowed("a declaration"),
            Item::DirectAddress => Cow::Borrowed("a direct address"),
            Item::Duration => Cow::Borrowed("a duration"),
            Item::DurationOrVariableName => Cow::Borrowed("a duration or the name of a variable"),
            Item::DurationValue => Cow::Borrowed("a duration value"),
            Item::FunctionBlockCall => Cow::Borrowed("a function block call"),
            Item::GlobalVariable => Cow::Borrowed("a global variable"),
            Item::LabelName => Cow::Borrowed("a label name"),
            Item::Literal => Cow::Borrowed("a literal"),
            Item::Member => Cow::Borrowed("a member"),
            Item::MemberName => Cow::Borrowed("a member name"),
            Item::Name => Cow::Borrowed("a name"),
            Item::NumberInDuration => Cow::Borrowed("a number in the duration"),
            Item::ResourceOrVariableBlock => Cow::Borrowed("a resource or a variable block"),
            Item::Statement => Cow::Borrowed("a statement"),
            Item::TaskOrProgram => Cow::Borrowed("a task or a program"),
            Item::TaskProperty => Cow::Borrowed("a task property"),
            Item::TypeDeclaration => Cow::Borrowed("a type declaration"),
            Item::TypeName => Cow::Borrowed("a type name"),
            Item::Variable => Cow::Borrowed("a variable"),
            Item::ActionAssociation => Cow::Borrowed("an action association"),
            Item::Expression => Cow::Borrowed("an expression"),
            Item::Integer => Cow::Borrowed("an integer"),
            Item::IntegerOrConstantName => Cow::Borrowed("an integer or the name of a constant"),
            Item::StructureEnd => Cow::Borrowed("the end of the structure"),
            Item::LoopVariable => Cow::Borrowed("the loop variable"),
            Item::RangeLowerBound => Cow::Borrowed("the lower bound of a range"),
            Item::StepName => Cow::Borrowed("the name of a step"),
            Item::TaskName => Cow::Borrowed("the name of a task"),
            Item::VariableNameNotAddress => Cow::Borrowed("the name of a variable, not an address"),
            Item::ActionName => Cow::Borrowed("the name of an action"),
            Item::VariablePath => Cow::Borrowed("the path of a variable"),
            Item::InstancePath => Cow::Borrowed("the path `resource.program.variable`"),
            Item::Priority => Cow::Borrowed("the priority"),
            Item::ResourceType => Cow::Borrowed("the type of the resource"),
            Item::RangeUpperBound => Cow::Borrowed("the upper bound of a range"),
            Item::CaseRangeUpperBound => Cow::Borrowed("the upper bound of the range"),
            Item::DurationUnit => Cow::Owned(format!("a {}", DurationUnit::expectation())),
            Item::DigitsAfterBase => Cow::Borrowed("digits after `10#`"),
            Item::IntegerValue => Cow::Borrowed("an integer value"),
            Item::RealValue => Cow::Borrowed("a real value"),
            Item::BitStringValue => Cow::Borrowed("a bit-string value"),
            Item::DoubleQuotedString => Cow::Borrowed("a double-quoted string"),
            Item::SingleQuotedString => Cow::Borrowed("a single-quoted string"),
            Item::Hour => Cow::Borrowed("an hour"),
            Item::Minute => Cow::Borrowed("a minute"),
            Item::Second => Cow::Borrowed("a second"),
            Item::Year => Cow::Borrowed("a year"),
            Item::Month => Cow::Borrowed("a month"),
            Item::Day => Cow::Borrowed("a day"),
            Item::ConstantOrGlobalVariable => Cow::Borrowed("a constant or a global variable"),
            Item::ConstantEnumerationValueOrAddress => {
                Cow::Borrowed("a constant, an enumeration value or an address")
            }
            Item::GlobalVariableOrAddress => Cow::Borrowed("a global variable or an address"),
            Item::AccessorEnd => Cow::Borrowed("the end of the accessor"),
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::syntax_kind::SyntaxKind as K;

    /// The message of an error that expected `expected` and found `x`.
    fn said(expected: impl Into<Expected>) -> String {
        said_at("x", expected)
    }

    /// The message of an error that expected `expected` where `source` begins.
    fn said_at(source: &str, expected: impl Into<Expected>) -> String {
        Expectation::new(Found::first_in(source), expected)
            .text()
            .message()
            .to_string()
    }

    #[test]
    fn text_when_token_found_then_it_comes_first_as_written() {
        assert_eq!(
            said_at("END_IF", K::Semicolon),
            "found `END_IF`, expected `;`"
        );
        assert_eq!(said_at("42", Item::Name), "found `42`, expected a name");
        assert_eq!(
            said_at("end_if ;", K::Semicolon),
            "found `end_if`, expected `;`"
        );
    }

    #[test]
    fn text_when_input_ended_then_found_says_end_of_input() {
        assert_eq!(
            said_at("", K::EndVar),
            "found end of input, expected `END_VAR`"
        );
        assert_eq!(
            said_at("(* nothing *) ", Item::Statement),
            "found end of input, expected a statement"
        );
    }

    #[test]
    fn text_when_token_expected_then_its_spelling_in_code_quotes() {
        assert_eq!(said(K::Semicolon), "found `x`, expected `;`");
        assert_eq!(said(K::Assignment), "found `x`, expected `:=`");
        assert_eq!(said(K::EndVar), "found `x`, expected `END_VAR`");
    }

    #[test]
    fn text_when_every_symbol_expected_then_names_it_by_its_declared_symbol() {
        for (symbol, kind) in K::SYMBOLS {
            assert_eq!(said(*kind), format!("found `x`, expected `{symbol}`"));
        }
    }

    #[test]
    fn text_when_set_of_tokens_expected_then_spellings_in_declaration_order() {
        // `Assignment` is declared before `RightArrow`, `Else` before `If`,
        // whatever order the set was written in.
        assert_eq!(
            said(KindSet::new(&[K::RightArrow, K::Assignment])),
            "found `x`, expected `:=` or `=>`"
        );
        assert_eq!(
            said(KindSet::new(&[K::If, K::Else, K::EndIf])),
            "found `x`, expected `ELSE`, `IF` or `END_IF`"
        );
        assert_eq!(said(KindSet::new(&[K::If])), "found `x`, expected `IF`");
    }

    #[test]
    fn text_when_words_expected_then_listed_in_the_order_given() {
        assert_eq!(
            said(Expected::words(["SINGLE", "INTERVAL", "PRIORITY"])),
            "found `x`, expected `SINGLE`, `INTERVAL` or `PRIORITY`"
        );
        assert_eq!(
            said(Expected::words(["GET", "SET"])),
            "found `x`, expected `GET` or `SET`"
        );
        assert_eq!(
            said(Expected::words(["PRIORITY"])),
            "found `x`, expected `PRIORITY`"
        );
    }

    #[test]
    fn text_when_item_expected_then_its_phrase() {
        for (item, message) in [
            (
                Item::BooleanValue,
                "found `x`, expected 0, 1 or a boolean word",
            ),
            (Item::CaseSelector, "found `x`, expected a case selector"),
            (Item::Constant, "found `x`, expected a constant"),
            (Item::Declaration, "found `x`, expected a declaration"),
            (Item::DirectAddress, "found `x`, expected a direct address"),
            (Item::Duration, "found `x`, expected a duration"),
            (
                Item::DurationOrVariableName,
                "found `x`, expected a duration or the name of a variable",
            ),
            (Item::DurationValue, "found `x`, expected a duration value"),
            (
                Item::FunctionBlockCall,
                "found `x`, expected a function block call",
            ),
            (
                Item::GlobalVariable,
                "found `x`, expected a global variable",
            ),
            (Item::LabelName, "found `x`, expected a label name"),
            (Item::Literal, "found `x`, expected a literal"),
            (Item::Member, "found `x`, expected a member"),
            (Item::MemberName, "found `x`, expected a member name"),
            (Item::Name, "found `x`, expected a name"),
            (
                Item::NumberInDuration,
                "found `x`, expected a number in the duration",
            ),
            (
                Item::ResourceOrVariableBlock,
                "found `x`, expected a resource or a variable block",
            ),
            (Item::Statement, "found `x`, expected a statement"),
            (
                Item::TaskOrProgram,
                "found `x`, expected a task or a program",
            ),
            (Item::TaskProperty, "found `x`, expected a task property"),
            (
                Item::TypeDeclaration,
                "found `x`, expected a type declaration",
            ),
            (Item::TypeName, "found `x`, expected a type name"),
            (Item::Variable, "found `x`, expected a variable"),
            (
                Item::ActionAssociation,
                "found `x`, expected an action association",
            ),
            (Item::Expression, "found `x`, expected an expression"),
            (Item::Integer, "found `x`, expected an integer"),
            (
                Item::IntegerOrConstantName,
                "found `x`, expected an integer or the name of a constant",
            ),
            (
                Item::StructureEnd,
                "found `x`, expected the end of the structure",
            ),
            (Item::LoopVariable, "found `x`, expected the loop variable"),
            (
                Item::RangeLowerBound,
                "found `x`, expected the lower bound of a range",
            ),
            (Item::StepName, "found `x`, expected the name of a step"),
            (Item::TaskName, "found `x`, expected the name of a task"),
            (
                Item::VariableNameNotAddress,
                "found `x`, expected the name of a variable, not an address",
            ),
            (
                Item::ActionName,
                "found `x`, expected the name of an action",
            ),
            (
                Item::VariablePath,
                "found `x`, expected the path of a variable",
            ),
            (
                Item::InstancePath,
                "found `x`, expected the path `resource.program.variable`",
            ),
            (Item::Priority, "found `x`, expected the priority"),
            (
                Item::ResourceType,
                "found `x`, expected the type of the resource",
            ),
            (
                Item::RangeUpperBound,
                "found `x`, expected the upper bound of a range",
            ),
            (
                Item::CaseRangeUpperBound,
                "found `x`, expected the upper bound of the range",
            ),
            (
                Item::DigitsAfterBase,
                "found `x`, expected digits after `10#`",
            ),
            (Item::IntegerValue, "found `x`, expected an integer value"),
            (Item::RealValue, "found `x`, expected a real value"),
            (
                Item::BitStringValue,
                "found `x`, expected a bit-string value",
            ),
            (
                Item::DoubleQuotedString,
                "found `x`, expected a double-quoted string",
            ),
            (
                Item::SingleQuotedString,
                "found `x`, expected a single-quoted string",
            ),
            (Item::Hour, "found `x`, expected an hour"),
            (Item::Minute, "found `x`, expected a minute"),
            (Item::Second, "found `x`, expected a second"),
            (Item::Year, "found `x`, expected a year"),
            (Item::Month, "found `x`, expected a month"),
            (Item::Day, "found `x`, expected a day"),
            (
                Item::ConstantOrGlobalVariable,
                "found `x`, expected a constant or a global variable",
            ),
            (
                Item::ConstantEnumerationValueOrAddress,
                "found `x`, expected a constant, an enumeration value or an address",
            ),
            (
                Item::GlobalVariableOrAddress,
                "found `x`, expected a global variable or an address",
            ),
            (
                Item::AccessorEnd,
                "found `x`, expected the end of the accessor",
            ),
        ] {
            assert_eq!(said(item), message, "{item:?}");
        }
    }

    #[test]
    fn text_when_duration_unit_expected_then_the_units_come_from_the_one_table() {
        assert_eq!(
            said(Item::DurationUnit),
            format!("found `x`, expected a {}", DurationUnit::expectation())
        );
    }

    /// The sources under `dir`, with their paths.
    fn sources(dir: &std::path::Path) -> Vec<(std::path::PathBuf, String)> {
        let mut found = Vec::new();
        for entry in std::fs::read_dir(dir).into_iter().flatten().flatten() {
            let path = entry.path();
            if path.is_dir() {
                found.extend(sources(&path));
            } else if path.extension().is_some_and(|extension| extension == "rs") {
                let text = std::fs::read_to_string(&path).unwrap_or_default();
                found.push((path, text));
            }
        }
        found
    }

    #[test]
    fn source_when_scanned_then_only_this_module_fills_the_expectation_row() {
        // A row that takes any string for "what" or "found" would let a
        // caller word an expectation itself; the data of an expectation is the
        // only input.
        let root = std::path::Path::new(env!("CARGO_MANIFEST_DIR")).join("src");
        let all = sources(&root);
        assert!(all.len() > 50, "found {} sources", all.len());
        let callers: Vec<_> = all
            .iter()
            .filter(|(path, text)| {
                !path.ends_with("expectation.rs")
                    && text.contains(concat!("syntax_error_found_", "expected("))
            })
            .map(|(path, _)| path)
            .collect();
        assert!(callers.is_empty(), "{callers:?}");
    }

    #[test]
    fn text_when_expectation_is_made_then_the_problem_is_the_grammars() {
        assert_eq!(
            Expectation::new(Found::end_of_input(), Item::Name)
                .text()
                .problem(),
            &ironplc_problems::Problem::SyntaxError
        );
    }
}
