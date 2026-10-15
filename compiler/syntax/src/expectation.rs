//! What a syntax error says was expected.
//!
//! An "expected ..." error means that at this position one of a few things
//! was to stand: a token, one of several tokens, one of several words, or a
//! part of the grammar (a name, an expression, the lower bound of a range). The
//! error carries that as data, an [`Expectation`], and its text is derived from
//! it by one template row of `problem-texts.csv`:
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
//! The expectation is a struct so that it can later also hold the token that
//! was found instead; whether an error says it is not decided.

use crate::kind_set::KindSet;
use crate::syntax_kind::SyntaxKind;
use core::fmt;
use ironplc_dsl::construct::DurationUnit;
use ironplc_problems::{text, Text};
use std::borrow::Cow;

/// What was expected at the position of an error.
#[derive(Debug, Clone, PartialEq, Eq)]
pub(crate) struct Expectation {
    expected: Expected,
}

/// The things that can be expected; see the module documentation.
#[derive(Debug, Clone, PartialEq, Eq)]
enum Expected {
    /// One token or keyword.
    Token(SyntaxKind),
    /// Any one token or keyword of a set.
    OneOf(KindSet),
    /// Any one of several words that are read as names, not as keywords.
    Words(Vec<&'static str>),
    /// A part of the grammar.
    Item(Item),
}

impl Expectation {
    /// The token or keyword `kind`. The kind must have a spelling.
    pub(crate) fn token(kind: SyntaxKind) -> Expectation {
        debug_assert!(kind.spelling().is_some(), "{kind:?} has no spelling");
        Expectation {
            expected: Expected::Token(kind),
        }
    }

    /// Any one of the tokens and keywords of `kinds`, which must all have a
    /// spelling.
    pub(crate) fn one_of(kinds: KindSet) -> Expectation {
        debug_assert!(
            kinds.iter().all(|kind| kind.spelling().is_some()),
            "{kinds:?} holds a kind that has no spelling"
        );
        Expectation {
            expected: Expected::OneOf(kinds),
        }
    }

    /// Any one of `words`, in the order given.
    pub(crate) fn words(words: impl IntoIterator<Item = &'static str>) -> Expectation {
        Expectation {
            expected: Expected::Words(words.into_iter().collect()),
        }
    }

    /// The text of the error that says this was expected.
    pub(crate) fn text(&self) -> Text {
        text::syntax_error_expected_what(self)
    }
}

impl From<SyntaxKind> for Expectation {
    fn from(kind: SyntaxKind) -> Expectation {
        Expectation::token(kind)
    }
}

impl From<KindSet> for Expectation {
    fn from(kinds: KindSet) -> Expectation {
        Expectation::one_of(kinds)
    }
}

impl From<Item> for Expectation {
    fn from(item: Item) -> Expectation {
        Expectation {
            expected: Expected::Item(item),
        }
    }
}

impl fmt::Display for Expectation {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match &self.expected {
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
    /// "the enumeration value".
    EnumerationValue,
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
            Item::EnumerationValue => Cow::Borrowed("the enumeration value"),
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

    /// The text of the error that says this item was expected.
    pub(crate) fn text(self) -> Text {
        Expectation::from(self).text()
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::syntax_kind::SyntaxKind as K;

    fn said(expectation: impl Into<Expectation>) -> String {
        expectation.into().text().message().to_string()
    }

    #[test]
    fn text_when_token_expected_then_its_spelling_in_code_quotes() {
        assert_eq!(said(K::Semicolon), "expected `;`");
        assert_eq!(said(K::Assignment), "expected `:=`");
        assert_eq!(said(K::EndVar), "expected `END_VAR`");
    }

    #[test]
    fn text_when_every_symbol_expected_then_names_it_by_its_declared_symbol() {
        for (symbol, kind) in K::SYMBOLS {
            assert_eq!(said(*kind), format!("expected `{symbol}`"));
        }
    }

    #[test]
    fn text_when_set_of_tokens_expected_then_spellings_in_declaration_order() {
        // `Assignment` is declared before `RightArrow`, `Else` before `If`,
        // whatever order the set was written in.
        assert_eq!(
            said(KindSet::new(&[K::RightArrow, K::Assignment])),
            "expected `:=` or `=>`"
        );
        assert_eq!(
            said(KindSet::new(&[K::If, K::Else, K::EndIf])),
            "expected `ELSE`, `IF` or `END_IF`"
        );
        assert_eq!(said(KindSet::new(&[K::If])), "expected `IF`");
    }

    #[test]
    fn text_when_words_expected_then_listed_in_the_order_given() {
        assert_eq!(
            said(Expectation::words(["SINGLE", "INTERVAL", "PRIORITY"])),
            "expected `SINGLE`, `INTERVAL` or `PRIORITY`"
        );
        assert_eq!(
            said(Expectation::words(["GET", "SET"])),
            "expected `GET` or `SET`"
        );
        assert_eq!(
            said(Expectation::words(["PRIORITY"])),
            "expected `PRIORITY`"
        );
    }

    #[test]
    fn text_when_item_expected_then_its_phrase() {
        for (item, message) in [
            (Item::BooleanValue, "expected 0, 1 or a boolean word"),
            (Item::CaseSelector, "expected a case selector"),
            (Item::Constant, "expected a constant"),
            (Item::Declaration, "expected a declaration"),
            (Item::DirectAddress, "expected a direct address"),
            (Item::Duration, "expected a duration"),
            (
                Item::DurationOrVariableName,
                "expected a duration or the name of a variable",
            ),
            (Item::DurationValue, "expected a duration value"),
            (Item::FunctionBlockCall, "expected a function block call"),
            (Item::GlobalVariable, "expected a global variable"),
            (Item::LabelName, "expected a label name"),
            (Item::Literal, "expected a literal"),
            (Item::Member, "expected a member"),
            (Item::MemberName, "expected a member name"),
            (Item::Name, "expected a name"),
            (Item::NumberInDuration, "expected a number in the duration"),
            (
                Item::ResourceOrVariableBlock,
                "expected a resource or a variable block",
            ),
            (Item::Statement, "expected a statement"),
            (Item::TaskOrProgram, "expected a task or a program"),
            (Item::TaskProperty, "expected a task property"),
            (Item::TypeDeclaration, "expected a type declaration"),
            (Item::TypeName, "expected a type name"),
            (Item::Variable, "expected a variable"),
            (Item::ActionAssociation, "expected an action association"),
            (Item::Expression, "expected an expression"),
            (Item::Integer, "expected an integer"),
            (
                Item::IntegerOrConstantName,
                "expected an integer or the name of a constant",
            ),
            (Item::StructureEnd, "expected the end of the structure"),
            (Item::EnumerationValue, "expected the enumeration value"),
            (Item::LoopVariable, "expected the loop variable"),
            (Item::RangeLowerBound, "expected the lower bound of a range"),
            (Item::StepName, "expected the name of a step"),
            (Item::TaskName, "expected the name of a task"),
            (
                Item::VariableNameNotAddress,
                "expected the name of a variable, not an address",
            ),
            (Item::ActionName, "expected the name of an action"),
            (Item::VariablePath, "expected the path of a variable"),
            (
                Item::InstancePath,
                "expected the path `resource.program.variable`",
            ),
            (Item::Priority, "expected the priority"),
            (Item::ResourceType, "expected the type of the resource"),
            (Item::RangeUpperBound, "expected the upper bound of a range"),
            (
                Item::CaseRangeUpperBound,
                "expected the upper bound of the range",
            ),
            (Item::DigitsAfterBase, "expected digits after `10#`"),
            (Item::IntegerValue, "expected an integer value"),
            (Item::RealValue, "expected a real value"),
            (Item::BitStringValue, "expected a bit-string value"),
            (Item::DoubleQuotedString, "expected a double-quoted string"),
            (Item::SingleQuotedString, "expected a single-quoted string"),
            (Item::Hour, "expected an hour"),
            (Item::Minute, "expected a minute"),
            (Item::Second, "expected a second"),
            (Item::Year, "expected a year"),
            (Item::Month, "expected a month"),
            (Item::Day, "expected a day"),
            (
                Item::ConstantOrGlobalVariable,
                "expected a constant or a global variable",
            ),
            (
                Item::ConstantEnumerationValueOrAddress,
                "expected a constant, an enumeration value or an address",
            ),
            (
                Item::GlobalVariableOrAddress,
                "expected a global variable or an address",
            ),
            (Item::AccessorEnd, "expected the end of the accessor"),
        ] {
            assert_eq!(said(item), message, "{item:?}");
        }
    }

    #[test]
    fn text_when_duration_unit_expected_then_the_units_come_from_the_one_table() {
        assert_eq!(
            said(Item::DurationUnit),
            format!("expected a {}", DurationUnit::expectation())
        );
    }

    #[test]
    fn text_when_expectation_is_made_then_the_problem_is_the_grammars() {
        assert_eq!(
            Item::Name.text().problem(),
            &ironplc_problems::Problem::SyntaxError
        );
    }
}
