//! Literals: the nine literal node kinds to [`ConstantKind`].
//!
//! The tree keeps a literal as the tokens the lexer made (`T#1m30s` is `T`,
//! `#`, `1` and `m30s`); a rule here reads the type prefix and the value
//! pieces and builds the object with the same constructors the legacy grammar
//! calls, from `ironplc_dsl`: `Integer::try_hex` and its siblings, `unquote`,
//! `combine_interval_parts`, `time_of_day`, `calendar_date`. No rule decodes
//! text itself. The one exception is the text of a duration value, which the
//! parser also checks and so shares with it (`interval_text`).
//!
//! `lower_constant` is the one dispatch: the kind of the node selects the
//! rule, the rule builds the literal without a position, and `lower_constant`
//! records the node's span on it, as the legacy grammar does for every literal.
//! Each family's prefix words are rows of a table, so a type name added to the
//! language is a row, not a branch.

use super::tree::significant_tokens;
use super::LowerCx;
use crate::interval_text;
use crate::syntax_kind::{SyntaxKind as K, SyntaxNode, SyntaxToken};
use ironplc_dsl::common::{
    BitStringLiteral, BitStringTypeName, Boolean, BooleanLiteral, CharacterStringLiteral,
    ConstantKind, FixedPoint, Integer, IntegerLiteral, IntegerTypeName, RealLiteral, RealTypeName,
    SignedInteger, StringType,
};
use ironplc_dsl::construct::{
    calendar_date, combine_interval_parts, time_of_day, unquote, IntervalError,
};
use ironplc_dsl::core::SourceSpan;
use ironplc_dsl::diagnostic::Diagnostic;
use ironplc_dsl::time::{
    DateAndTimeLiteral, DateLiteral, DurationLiteral, DurationOutOfRange, TemporalWidth,
    TimeOfDayLiteral,
};
use time::{PrimitiveDateTime, Time};

#[cfg(test)]
mod tests;

/// A rule: builds the literal a node denotes from the node's significant
/// tokens, leaving its position to the caller.
type Rule = fn(&LowerCx, &SyntaxNode, &[SyntaxToken]) -> Result<ConstantKind, Diagnostic>;

/// The rule for each literal node kind.
fn rule(kind: K) -> Option<Rule> {
    Some(match kind {
        K::IntLiteral => integer,
        K::RealLiteral => real,
        K::BitStringLiteral => bit_string,
        K::BoolLiteral => boolean,
        K::StringLiteral => string,
        K::DurationLiteral => duration,
        K::TimeOfDayLiteral => time_of_day_literal,
        K::DateLiteral => date,
        K::DateTimeLiteral => date_and_time,
        _ => return None,
    })
}

/// Lowers a literal node to its constant.
///
/// The constant's span is the node's, which equals the legacy join of the
/// literal's first and last token: its type prefix and sign included.
pub fn lower_constant(cx: &LowerCx, node: &SyntaxNode) -> Result<ConstantKind, Diagnostic> {
    let Some(rule) = rule(node.kind()) else {
        return Err(cx.unsupported(node));
    };
    let tokens = significant_tokens(node);
    rule(cx, node, &tokens).map(|constant| constant.with_span(cx.node_span(node)))
}

// The prefix words of each family, with the type they name.

const INTEGER_TYPES: &[(K, IntegerTypeName)] = &[
    (K::Sint, IntegerTypeName::SINT),
    (K::Int, IntegerTypeName::INT),
    (K::Dint, IntegerTypeName::DINT),
    (K::Lint, IntegerTypeName::LINT),
    (K::Usint, IntegerTypeName::USINT),
    (K::Uint, IntegerTypeName::UINT),
    (K::Udint, IntegerTypeName::UDINT),
    (K::Ulint, IntegerTypeName::ULINT),
];

const REAL_TYPES: &[(K, RealTypeName)] = &[
    (K::Real, RealTypeName::REAL),
    (K::Lreal, RealTypeName::LREAL),
];

const BIT_STRING_TYPES: &[(K, BitStringTypeName)] = &[
    (K::Byte, BitStringTypeName::BYTE),
    (K::Word, BitStringTypeName::WORD),
    (K::Dword, BitStringTypeName::DWORD),
    (K::Lword, BitStringTypeName::LWORD),
];

/// The delimiter of a character string literal selects its width.
const STRING_WIDTHS: &[(K, StringType)] = &[
    (K::StringLit, StringType::String),
    (K::WStringLit, StringType::WString),
];

/// The constructor of an integer from the text of a based literal.
type BasedInteger = fn(&str) -> Result<Integer, &'static str>;

/// Based integers: the token of the base and the legacy constructor for its
/// text (`16#FF`, `8#17`, `2#1010`).
const BASED_INTEGERS: &[(K, BasedInteger)] = &[
    (K::HexLit, Integer::try_hex),
    (K::OctLit, Integer::try_octal),
    (K::BinLit, Integer::try_binary),
];

/// The keyword prefixes of the 64-bit members of the temporal types, and the
/// vendor abbreviations (`LT#`, `LD#`) that are ordinary words.
const LONG_KEYWORDS: &[K] = &[K::Ltime, K::Ltod, K::Ldate, K::Ldt];
const LONG_WORDS: &[&str] = &["LT", "LD"];

/// The tokens of a typed literal split at its prefix: the prefix word, when
/// the literal has one (`INT#`, `T#`, `STRING#`), and the pieces of the value.
/// A leading number is not a prefix: `10#5` is a based integer.
fn split_prefix(tokens: &[SyntaxToken]) -> (Option<&SyntaxToken>, &[SyntaxToken]) {
    match tokens {
        [prefix, hash, value @ ..] if hash.kind() == K::Hash && prefix.kind() != K::IntegerLit => {
            (Some(prefix), value)
        }
        _ => (None, tokens),
    }
}

/// The type the prefix names according to `table`: `Some(None)` when there is
/// no prefix, and `None` when the prefix is not in the table.
fn prefixed_type<T: Clone>(prefix: Option<&SyntaxToken>, table: &[(K, T)]) -> Option<Option<T>> {
    match prefix {
        None => Some(None),
        Some(token) => table
            .iter()
            .find(|(kind, _)| *kind == token.kind())
            .map(|(_, data_type)| Some(data_type.clone())),
    }
}

/// The width a temporal prefix names: the 32-bit member unless the prefix is
/// one of the 64-bit ones. `None` when there is no prefix.
fn temporal_width(prefix: Option<&SyntaxToken>) -> Option<TemporalWidth> {
    prefix.map(|token| {
        let long = LONG_KEYWORDS.contains(&token.kind())
            || (token.kind() == K::Ident
                && LONG_WORDS
                    .iter()
                    .any(|word| word.eq_ignore_ascii_case(token.text())));
        if long {
            TemporalWidth::Long
        } else {
            TemporalWidth::Short
        }
    })
}

/// A token of digits, or of a based integer, as the integer it spells.
fn integer_token(token: &SyntaxToken) -> Option<Result<Integer, &'static str>> {
    if token.kind() == K::IntegerLit {
        return Some(Integer::new(token.text(), SourceSpan::default()));
    }
    BASED_INTEGERS
        .iter()
        .find(|(kind, _)| *kind == token.kind())
        .map(|(_, parse)| parse(token.text()))
}

/// The diagnostic for a tree whose shape the grammar does not produce.
fn shape(cx: &LowerCx, node: &SyntaxNode) -> Diagnostic {
    cx.internal_error(
        node.text_range(),
        format!(
            "{:?} does not have the tokens the grammar gives it",
            node.kind()
        ),
    )
}

/// The diagnostic for a literal whose text does not denote a value.
fn out_of_range(cx: &LowerCx, node: &SyntaxNode, why: impl std::fmt::Display) -> Diagnostic {
    cx.syntax_error(
        node.text_range(),
        format!("the literal '{}' is not valid: {why}", node.text()),
    )
}

/// An integer literal: an optional integer type prefix, then a sign and
/// digits, a based integer, or `10#` and digits.
fn integer(
    cx: &LowerCx,
    node: &SyntaxNode,
    tokens: &[SyntaxToken],
) -> Result<ConstantKind, Diagnostic> {
    let (prefix, value) = split_prefix(tokens);
    let data_type = prefixed_type(prefix, INTEGER_TYPES).ok_or_else(|| shape(cx, node))?;
    let default = SourceSpan::default();
    let value = match value {
        [token] => integer_token(token).map(|parsed| parsed.map(SignedInteger::from)),
        [ten, hash, digits]
            if ten.kind() == K::IntegerLit
                && hash.kind() == K::Hash
                && digits.kind() == K::IntegerLit =>
        {
            Some(Integer::new(digits.text(), default).map(SignedInteger::from))
        }
        [sign, digits] if digits.kind() == K::IntegerLit => match sign.kind() {
            K::Plus => Some(SignedInteger::positive(digits.text(), default)),
            K::Minus => Some(SignedInteger::negative(digits.text(), default)),
            _ => None,
        },
        _ => None,
    };
    let value = value
        .ok_or_else(|| shape(cx, node))?
        .map_err(|why| out_of_range(cx, node, why))?;
    Ok(ConstantKind::IntegerLiteral(IntegerLiteral {
        value,
        data_type,
    }))
}

/// A real literal: an optional real type prefix, an optional sign and a
/// fixed or floating point number.
fn real(
    cx: &LowerCx,
    node: &SyntaxNode,
    tokens: &[SyntaxToken],
) -> Result<ConstantKind, Diagnostic> {
    let (prefix, value) = split_prefix(tokens);
    let data_type = prefixed_type(prefix, REAL_TYPES).ok_or_else(|| shape(cx, node))?;
    let (sign, number) = match value {
        [number] => (1.0, number),
        [sign, number] if sign.kind() == K::Minus => (-1.0, number),
        [sign, number] if sign.kind() == K::Plus => (1.0, number),
        _ => return Err(shape(cx, node)),
    };
    let parsed = RealLiteral::try_parse(number.text(), data_type)
        .map_err(|why| out_of_range(cx, node, why))?;
    Ok(ConstantKind::RealLiteral(RealLiteral {
        value: parsed.value * sign,
        ..parsed
    }))
}

/// A bit-string literal: a bit-string type prefix (absent in a case label),
/// then a based integer or digits.
fn bit_string(
    cx: &LowerCx,
    node: &SyntaxNode,
    tokens: &[SyntaxToken],
) -> Result<ConstantKind, Diagnostic> {
    let (prefix, value) = split_prefix(tokens);
    let data_type = prefixed_type(prefix, BIT_STRING_TYPES).ok_or_else(|| shape(cx, node))?;
    let [token] = value else {
        return Err(shape(cx, node));
    };
    let value = integer_token(token)
        .ok_or_else(|| shape(cx, node))?
        .map_err(|why| out_of_range(cx, node, why))?;
    Ok(ConstantKind::BitStringLiteral(BitStringLiteral {
        value,
        data_type,
    }))
}

/// A boolean literal: `TRUE`, `FALSE`, or `BOOL#` or `BIT#` with `0`, `1`,
/// `TRUE` or `FALSE`.
fn boolean(
    cx: &LowerCx,
    node: &SyntaxNode,
    tokens: &[SyntaxToken],
) -> Result<ConstantKind, Diagnostic> {
    let (_, value) = split_prefix(tokens);
    let value = match value {
        [token] if token.kind() == K::True => Boolean::True,
        [token] if token.kind() == K::False => Boolean::False,
        [token] if token.kind() == K::IntegerLit && token.text() == "1" => Boolean::True,
        [token] if token.kind() == K::IntegerLit && token.text() == "0" => Boolean::False,
        _ => return Err(shape(cx, node)),
    };
    Ok(ConstantKind::Boolean(BooleanLiteral::new(value)))
}

/// A character string literal: an optional string prefix, then a quoted
/// string whose delimiter gives the width.
fn string(
    cx: &LowerCx,
    node: &SyntaxNode,
    tokens: &[SyntaxToken],
) -> Result<ConstantKind, Diagnostic> {
    let (_, value) = split_prefix(tokens);
    let [token] = value else {
        return Err(shape(cx, node));
    };
    let width = STRING_WIDTHS
        .iter()
        .find(|(kind, _)| *kind == token.kind())
        .map(|(_, width)| width.clone())
        .ok_or_else(|| shape(cx, node))?;
    Ok(ConstantKind::CharacterString(CharacterStringLiteral {
        value: unquote(token.text(), &width),
        width,
        span: SourceSpan::default(),
    }))
}

/// A duration literal: a prefix, an optional `-`, then the parts of the
/// value, each a number and a unit.
fn duration(
    cx: &LowerCx,
    node: &SyntaxNode,
    tokens: &[SyntaxToken],
) -> Result<ConstantKind, Diagnostic> {
    let (prefix, value) = split_prefix(tokens);
    let width = temporal_width(prefix).ok_or_else(|| shape(cx, node))?;
    let (negative, pieces) = match value {
        [minus, pieces @ ..] if minus.kind() == K::Minus => (true, pieces),
        _ => (false, value),
    };
    let text: String = pieces.iter().map(SyntaxToken::text).collect();
    // A number too large for any count is a duration too large for any type,
    // the same as a count that fits but whose duration does not.
    let too_large = || {
        let written: String = tokens.iter().map(SyntaxToken::text).collect();
        DurationOutOfRange.diagnostic(cx.node_span(node), &written, width)
    };
    let mut parts = interval_text::parse(&text)
        .map_err(|why| out_of_range(cx, node, why))?
        .into_iter()
        .map(|part| FixedPoint::parse(&part.number).map(|number| (number, part.unit)))
        .collect::<Result<Vec<_>, _>>()
        .map_err(|_| too_large())?
        .into_iter();
    let first = parts.next().ok_or_else(|| shape(cx, node))?;
    let total = combine_interval_parts(first, parts.collect())
        .map_err(|why| match why {
            IntervalError::OutOfRange => too_large(),
            _ => shape(cx, node),
        })?
        .interval;
    Ok(ConstantKind::Duration(DurationLiteral {
        interval: if negative { -total } else { total },
        width,
        span: SourceSpan::default(),
    }))
}

/// The number tokens of `a sep b sep c`, when the tokens alternate that way.
fn fields(tokens: &[SyntaxToken], separator: K) -> Option<Vec<&SyntaxToken>> {
    let alternate = tokens.len() % 2 == 1
        && tokens
            .iter()
            .skip(1)
            .step_by(2)
            .all(|token| token.kind() == separator);
    alternate.then(|| tokens.iter().step_by(2).collect())
}

/// The value of a whole-number token of a clock or calendar field.
fn field_value(token: &SyntaxToken) -> Result<u128, &'static str> {
    Integer::new(token.text(), SourceSpan::default()).map(|integer| integer.value)
}

/// A time of day from `hour : minute [: second]`, the seconds optionally
/// fractional.
fn daytime(cx: &LowerCx, node: &SyntaxNode, tokens: &[SyntaxToken]) -> Result<Time, Diagnostic> {
    let numbers = fields(tokens, K::Colon).ok_or_else(|| shape(cx, node))?;
    let (hour, minute, second) = match numbers.as_slice() {
        [hour, minute] => (hour, minute, None),
        [hour, minute, second] => (hour, minute, Some(second)),
        _ => return Err(shape(cx, node)),
    };
    let (whole, nanoseconds) = match second {
        Some(token) => FixedPoint::parse(token.text())
            .map(|second| (u128::from(second.whole), second.nanoseconds()))
            .map_err(|why| out_of_range(cx, node, why))?,
        None => (0, 0),
    };
    let field = |token: &SyntaxToken| field_value(token).map_err(|why| out_of_range(cx, node, why));
    time_of_day(field(hour)?, field(minute)?, whole, nanoseconds).map_err(|field| {
        out_of_range(
            cx,
            node,
            format!(
                "the {} is out of range",
                format!("{field:?}").to_lowercase()
            ),
        )
    })
}

/// A calendar date from `year - month - day`.
fn calendar(
    cx: &LowerCx,
    node: &SyntaxNode,
    tokens: &[SyntaxToken],
) -> Result<time::Date, Diagnostic> {
    let numbers = fields(tokens, K::Minus).ok_or_else(|| shape(cx, node))?;
    let [year, month, day] = numbers.as_slice() else {
        return Err(shape(cx, node));
    };
    let field = |token: &SyntaxToken| field_value(token).map_err(|why| out_of_range(cx, node, why));
    calendar_date(field(year)?, field(month)?, field(day)?).map_err(|field| {
        out_of_range(
            cx,
            node,
            format!("the {} does not exist", format!("{field:?}").to_lowercase()),
        )
    })
}

/// A time-of-day literal: a prefix, then a clock time.
fn time_of_day_literal(
    cx: &LowerCx,
    node: &SyntaxNode,
    tokens: &[SyntaxToken],
) -> Result<ConstantKind, Diagnostic> {
    let (prefix, value) = split_prefix(tokens);
    let width = temporal_width(prefix).ok_or_else(|| shape(cx, node))?;
    let time = daytime(cx, node, value)?;
    Ok(ConstantKind::TimeOfDay(
        TimeOfDayLiteral::new(time).with_width(width),
    ))
}

/// A date literal: a prefix, then `year - month - day`.
fn date(
    cx: &LowerCx,
    node: &SyntaxNode,
    tokens: &[SyntaxToken],
) -> Result<ConstantKind, Diagnostic> {
    let (prefix, value) = split_prefix(tokens);
    let width = temporal_width(prefix).ok_or_else(|| shape(cx, node))?;
    let day = calendar(cx, node, value)?;
    Ok(ConstantKind::Date(DateLiteral::new(day).with_width(width)))
}

/// A date-and-time literal: a prefix, a date, `-`, then a clock time.
fn date_and_time(
    cx: &LowerCx,
    node: &SyntaxNode,
    tokens: &[SyntaxToken],
) -> Result<ConstantKind, Diagnostic> {
    let (prefix, value) = split_prefix(tokens);
    let width = temporal_width(prefix).ok_or_else(|| shape(cx, node))?;
    let (day, rest) = value.split_at_checked(5).ok_or_else(|| shape(cx, node))?;
    let clock = match rest {
        [minus, clock @ ..] if minus.kind() == K::Minus => clock,
        _ => return Err(shape(cx, node)),
    };
    let both = PrimitiveDateTime::new(calendar(cx, node, day)?, daytime(cx, node, clock)?);
    Ok(ConstantKind::DateAndTime(
        DateAndTimeLiteral::new(both).with_width(width),
    ))
}
