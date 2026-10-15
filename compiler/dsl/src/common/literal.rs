//! Literals and constants: the elementary values a program writes in its source text.

use core::str::FromStr;
use std::fmt;

use dsl_macro_derive::{Located, Recurse};

use super::{BitStringTypeName, IntegerTypeName, RealTypeName, StringType};
use crate::core::{Id, Located, SourceSpan};
use crate::fold::Fold;
use crate::time::*;
use crate::visitor::Visitor;

/// Container for elementary constants.
///
/// See section 2.2.
#[derive(PartialEq, Clone, Debug, Recurse)]
pub enum ConstantKind {
    IntegerLiteral(IntegerLiteral),
    RealLiteral(RealLiteral),
    Boolean(BooleanLiteral),
    CharacterString(CharacterStringLiteral),
    Duration(DurationLiteral),
    TimeOfDay(TimeOfDayLiteral),
    Date(DateLiteral),
    DateAndTime(DateAndTimeLiteral),
    BitStringLiteral(BitStringLiteral),
}

impl ConstantKind {
    pub fn integer_literal(value: &str) -> Result<Self, &'static str> {
        Ok(Self::IntegerLiteral(IntegerLiteral {
            value: SignedInteger::new(value, SourceSpan::default())?,
            data_type: None,
        }))
    }

    /// Returns the constant with `span` recorded as its position in the
    /// source text.
    ///
    /// Every literal kind carries its own span rather than the enumeration
    /// carrying one, because a literal is also reachable outside a
    /// `ConstantKind` (a subrange bound, a case label). Setting them all in
    /// one place is what keeps `Located for ConstantKind` -- and so
    /// `Located for ExprKind`, which joins the spans of an expression's
    /// operands -- from reporting position 0 for an expression built from
    /// literals.
    pub fn with_span(self, span: SourceSpan) -> Self {
        match self {
            ConstantKind::IntegerLiteral(mut lit) => {
                lit.value.value.span = span;
                ConstantKind::IntegerLiteral(lit)
            }
            ConstantKind::BitStringLiteral(mut lit) => {
                lit.value.span = span;
                ConstantKind::BitStringLiteral(lit)
            }
            ConstantKind::RealLiteral(mut lit) => {
                lit.span = span;
                ConstantKind::RealLiteral(lit)
            }
            ConstantKind::Boolean(mut lit) => {
                lit.span = span;
                ConstantKind::Boolean(lit)
            }
            ConstantKind::CharacterString(mut lit) => {
                lit.span = span;
                ConstantKind::CharacterString(lit)
            }
            ConstantKind::Duration(mut lit) => {
                lit.span = span;
                ConstantKind::Duration(lit)
            }
            ConstantKind::TimeOfDay(mut lit) => {
                lit.span = span;
                ConstantKind::TimeOfDay(lit)
            }
            ConstantKind::Date(mut lit) => {
                lit.span = span;
                ConstantKind::Date(lit)
            }
            ConstantKind::DateAndTime(mut lit) => {
                lit.span = span;
                ConstantKind::DateAndTime(lit)
            }
        }
    }
}

impl Located for ConstantKind {
    fn span(&self) -> SourceSpan {
        match self {
            ConstantKind::IntegerLiteral(lit) => lit.value.value.span(),
            ConstantKind::BitStringLiteral(lit) => lit.value.span(),
            ConstantKind::RealLiteral(lit) => lit.span.clone(),
            ConstantKind::Boolean(lit) => lit.span.clone(),
            ConstantKind::CharacterString(lit) => lit.span.clone(),
            ConstantKind::Duration(lit) => lit.span.clone(),
            ConstantKind::TimeOfDay(lit) => lit.span.clone(),
            ConstantKind::Date(lit) => lit.span.clone(),
            ConstantKind::DateAndTime(lit) => lit.span.clone(),
        }
    }
}

impl fmt::Display for ConstantKind {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            ConstantKind::IntegerLiteral(lit) => write!(f, "{}", lit.value),
            ConstantKind::RealLiteral(lit) => write!(f, "{}", lit.value),
            ConstantKind::Boolean(lit) => write!(f, "{}", lit.value),
            ConstantKind::CharacterString(lit) => {
                let delimiter = lit.width.delimiter();
                let text = crate::string_escape::encode(&lit.value, &lit.width);
                write!(f, "{delimiter}{text}{delimiter}")
            }
            ConstantKind::Duration(lit) => write!(f, "{:?}", lit),
            ConstantKind::TimeOfDay(lit) => write!(f, "{:?}", lit),
            ConstantKind::Date(lit) => write!(f, "{:?}", lit),
            ConstantKind::DateAndTime(lit) => write!(f, "{:?}", lit),
            ConstantKind::BitStringLiteral(lit) => write!(f, "{:?}", lit),
        }
    }
}

#[derive(Clone, Debug, PartialEq)]
pub enum Boolean {
    True,
    False,
}

impl fmt::Display for Boolean {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            Boolean::True => write!(f, "TRUE"),
            Boolean::False => write!(f, "FALSE"),
        }
    }
}

// Numeric liberals declared by 2.2.1. Numeric literals define
// how data is expressed and are distinct from but associated with
// data types.

/// Integer liberal. The representation is of the largest possible integer
/// and later bound to smaller types depend on context.
#[derive(Debug, Clone, PartialEq, Recurse, Located)]
pub struct Integer {
    #[located(position)]
    pub span: SourceSpan,
    /// The value in the maximum possible size. An integer is inherently
    /// an unsigned value.
    #[recurse(ignore)]
    pub value: u128,
}

#[derive(Debug)]
pub struct TryFromIntegerError();

impl TryFrom<Integer> for u8 {
    type Error = TryFromIntegerError;
    fn try_from(value: Integer) -> Result<u8, Self::Error> {
        value.value.try_into().map_err(|_e| TryFromIntegerError {})
    }
}

impl TryFrom<Integer> for u32 {
    type Error = TryFromIntegerError;
    fn try_from(value: Integer) -> Result<u32, Self::Error> {
        value.value.try_into().map_err(|_e| TryFromIntegerError {})
    }
}

impl TryFrom<Integer> for i128 {
    type Error = TryFromIntegerError;
    fn try_from(value: Integer) -> Result<i128, Self::Error> {
        value.value.try_into().map_err(|_e| TryFromIntegerError {})
    }
}

impl TryFrom<Integer> for f64 {
    type Error = TryFromIntegerError;
    fn try_from(value: Integer) -> Result<f64, Self::Error> {
        let res: Result<u32, _> = value.value.try_into();
        let val = res.map_err(|_e| TryFromIntegerError {})?;

        let res: f64 = val.into();
        Ok(res)
    }
}

impl From<Integer> for f32 {
    fn from(value: Integer) -> f32 {
        value.value as f32
    }
}

impl Integer {
    pub fn new(a: &str, span: SourceSpan) -> Result<Self, &'static str> {
        let without_underscore: String = a.chars().filter(|c| c.is_ascii_digit()).collect();
        without_underscore
            .as_str()
            .parse::<u128>()
            .map(|value| Integer { span, value })
            .map_err(|_e| "dec")
    }

    pub fn try_hex(a: &str) -> Result<Self, &'static str> {
        if !a.starts_with("16#") {
            return Err("Non-hex start");
        }

        let (hex, remainder): (Vec<_>, Vec<_>) = a
            .chars()
            .skip(3)
            .filter(|c| *c != '_')
            .partition(|c| c.is_ascii_hexdigit());
        if !remainder.is_empty() {
            return Err("Non-hex characters");
        }
        let hex: String = hex.into_iter().collect();
        u128::from_str_radix(hex.as_str(), 16)
            .map(|value| Integer {
                span: SourceSpan::default(),
                value,
            })
            .map_err(|_e| "hex")
    }

    pub fn hex(a: &str, span: SourceSpan) -> Result<Self, &'static str> {
        let without_underscore: String = a.chars().filter(|c| c.is_ascii_hexdigit()).collect();
        u128::from_str_radix(without_underscore.as_str(), 16)
            .map(|value| Integer { span, value })
            .map_err(|_e| "hex")
    }

    pub fn try_octal(a: &str) -> Result<Self, &'static str> {
        if !a.starts_with("8#") {
            return Err("Non-octal start");
        }

        let (oct, remainder): (Vec<_>, Vec<_>) = a
            .chars()
            .skip(2)
            .filter(|c| *c != '_')
            .partition(|c| matches!(c, '0'..='7'));
        if !remainder.is_empty() {
            return Err("Non-octal characters");
        }
        let oct: String = oct.into_iter().collect();
        u128::from_str_radix(oct.as_str(), 8)
            .map(|value| Integer {
                span: SourceSpan::default(),
                value,
            })
            .map_err(|_e| "octal")
    }

    pub fn octal(a: &str, span: SourceSpan) -> Result<Self, &'static str> {
        let without_underscore: String = a.chars().filter(|c| matches!(c, '0'..='7')).collect();
        u128::from_str_radix(without_underscore.as_str(), 8)
            .map(|value| Integer { span, value })
            .map_err(|_e| "octal")
    }

    pub fn try_binary(a: &str) -> Result<Self, &'static str> {
        if !a.starts_with("2#") {
            return Err("Non-binary start");
        }

        let (bin, remainder): (Vec<_>, Vec<_>) = a
            .chars()
            .skip(2)
            .filter(|c| *c != '_')
            .partition(|c| matches!(c, '0'..='1'));
        if !remainder.is_empty() {
            return Err("Non-binary characters");
        }
        let bin: String = bin.into_iter().collect();
        u128::from_str_radix(bin.as_str(), 2)
            .map(|value| Integer {
                span: SourceSpan::default(),
                value,
            })
            .map_err(|_e| "binary")
    }

    pub fn binary(a: &str, span: SourceSpan) -> Result<Self, &'static str> {
        let without_underscore: String = a.chars().filter(|c| matches!(c, '0'..='1')).collect();
        u128::from_str_radix(without_underscore.as_str(), 2)
            .map(|value| Integer { span, value })
            .map_err(|_e| "binary")
    }
}

impl fmt::Display for Integer {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        f.write_fmt(format_args!("{}", self.value))
    }
}

#[derive(Debug, Clone, PartialEq, Recurse)]
pub struct SignedInteger {
    pub value: Integer,
    #[recurse(ignore)]
    pub is_neg: bool,
}

impl SignedInteger {
    pub fn new(a: &str, span: SourceSpan) -> Result<Self, &'static str> {
        match a.chars().next() {
            Some('+') => {
                let whole = a.get(1..).ok_or("int")?;
                Ok(Self {
                    value: Integer::new(whole, span)?,
                    is_neg: false,
                })
            }
            Some('-') => {
                let whole = a.get(1..).ok_or("int")?;
                Ok(Self {
                    value: Integer::new(whole, span)?,
                    is_neg: true,
                })
            }
            _ => Ok(Self {
                value: Integer::new(a, span)?,
                is_neg: false,
            }),
        }
    }

    pub fn positive(a: &str, span: SourceSpan) -> Result<Self, &'static str> {
        Ok(Self {
            value: Integer::new(a, span)?,
            is_neg: false,
        })
    }

    pub fn negative(a: &str, span: SourceSpan) -> Result<Self, &'static str> {
        Ok(Self {
            value: Integer::new(a, span)?,
            is_neg: true,
        })
    }

    /// The signed numeric value, saturating at `i64::MIN`/`i64::MAX` if
    /// the underlying magnitude doesn't fit (values of this magnitude
    /// aren't realistic for an enum's explicit ordinal value).
    pub fn to_i64(&self) -> i64 {
        let magnitude = i64::try_from(self.value.value).unwrap_or(i64::MAX);
        if self.is_neg {
            -magnitude
        } else {
            magnitude
        }
    }
}

/// An integer value that may be either a literal or a reference to a named constant.
///
/// This enables extensions where constant identifiers can be used in
/// positions that normally require integer literals, such as STRING lengths
/// and array bounds.
#[derive(Clone, Debug, PartialEq, Recurse)]
pub enum IntegerRef {
    Literal(Integer),
    Constant(Id),
}

impl IntegerRef {
    pub fn as_integer(&self) -> Option<&Integer> {
        match self {
            IntegerRef::Literal(i) => Some(i),
            IntegerRef::Constant(_) => None,
        }
    }
}

/// A signed integer value that may be either a literal or a reference to a named constant.
///
/// See `IntegerRef` for details on the extension support.
#[derive(Clone, Debug, PartialEq, Recurse)]
pub enum SignedIntegerRef {
    Literal(SignedInteger),
    Constant(Id),
}

impl SignedIntegerRef {
    pub fn as_signed_integer(&self) -> Option<&SignedInteger> {
        match self {
            SignedIntegerRef::Literal(si) => Some(si),
            SignedIntegerRef::Constant(_) => None,
        }
    }
}

impl From<Integer> for SignedInteger {
    fn from(value: Integer) -> SignedInteger {
        SignedInteger {
            value,
            is_neg: false,
        }
    }
}

impl TryFrom<SignedInteger> for u8 {
    type Error = TryFromIntegerError;
    fn try_from(value: SignedInteger) -> Result<u8, Self::Error> {
        value.value.try_into().map_err(|_e| TryFromIntegerError {})
    }
}

impl TryFrom<SignedInteger> for u32 {
    type Error = TryFromIntegerError;
    fn try_from(value: SignedInteger) -> Result<u32, Self::Error> {
        value.value.try_into().map_err(|_e| TryFromIntegerError {})
    }
}

impl TryFrom<SignedInteger> for i128 {
    type Error = TryFromIntegerError;
    fn try_from(value: SignedInteger) -> Result<i128, Self::Error> {
        let mut primitive = value
            .value
            .try_into()
            .map_err(|_e| TryFromIntegerError {})?;
        if value.is_neg {
            primitive *= -1;
        }
        Ok(primitive)
    }
}

impl fmt::Display for SignedInteger {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        if self.is_neg {
            f.write_fmt(format_args!("-{}", self.value))
        } else {
            f.write_fmt(format_args!("{}", self.value))
        }
    }
}

impl From<SignedInteger> for String {
    fn from(value: SignedInteger) -> Self {
        if value.is_neg {
            format!("-{value}")
        } else {
            format!("{value}")
        }
    }
}

/// A signed integer literal with an optional type name.
///
/// See section 2.2.1.
#[derive(Debug, PartialEq, Clone, Recurse)]
pub struct IntegerLiteral {
    pub value: SignedInteger,
    #[recurse(ignore)]
    pub data_type: Option<IntegerTypeName>,
}

impl fmt::Display for IntegerLiteral {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match &self.data_type {
            Some(dt) => write!(f, "{}#{}", dt, self.value),
            None => write!(f, "{}", self.value),
        }
    }
}

/// The fixed point structure represents a fixed point number.
///
/// The structure keeps the whole and decimal parts as integers so that
/// we do not lose precision with floating point rounding.
#[derive(Debug, PartialEq, Clone)]
pub struct FixedPoint {
    pub span: SourceSpan,
    pub whole: u64,
    pub femptos: u64,
}

impl FixedPoint {
    pub const FRACTIONAL_UNITS: u64 = 1_000_000_000_000_000;

    /// What a rule expects in place of a whole part beyond the `u64` the
    /// structure holds.
    pub const WHOLE_TOO_LARGE: &'static str = "u64";

    pub fn parse(input: &str) -> Result<FixedPoint, &'static str> {
        // IEC 61131 allows underscores in numbers so remove those before we try to parse.
        let value: String = input
            .chars()
            .filter(|c| c.is_ascii_digit() || *c == '.')
            .collect();

        // We want to handle left and right of the decimal as integers, so start by splitting into
        // the two parts if there is a period character.
        match value.split_once('.') {
            Some((whole, decimal)) => {
                let whole = whole
                    .parse::<u64>()
                    .map_err(|_e| "floating point whole not valid")?;

                // The maximum value for a u64 integer is:
                //   18446744073709551615
                // This has up to 20 digits of precision, but we allow for only 15 to avoid wrapping
                // when parsing and so that there is a nice name. That means the decimal part can be at most
                //    999999999999999
                // To parse this, we need the add post-fix zeros as necessary so that we have the right
                // precision and then we can parse this as an u64 directly.

                // Check that we have not already exceeded the precision
                if decimal.len() > 15 {
                    return Err("floating point decimal excessive precision");
                }

                let mut decimal = decimal.to_owned();

                // Add post-fix zeros as necessary
                let number_of_zeros_to_add = 15 - decimal.len();
                let post_fix_zeros = "0".to_owned().repeat(number_of_zeros_to_add);
                decimal.push_str(post_fix_zeros.as_str());

                // Now parse this value as a u64
                let decimal = decimal
                    .parse::<u64>()
                    .map_err(|_e| "floating point decimal not valid")?;

                Ok(FixedPoint {
                    span: SourceSpan::default(),
                    whole,
                    femptos: decimal,
                })
            }
            None => {
                // There is no decimal point so this is essentially a whole number
                Ok(FixedPoint {
                    span: SourceSpan::default(),
                    whole: value
                        .parse::<u64>()
                        .map_err(|_e| FixedPoint::WHOLE_TOO_LARGE)?,
                    femptos: 0,
                })
            }
        }
    }
}

/// A whole-number token as a fixed-point value, unless its whole part is
/// beyond the `u64` the structure holds: the same limit
/// [`FixedPoint::parse`] puts on the same digits, so that a value is not read
/// differently for being written with or without a decimal point.
impl TryFrom<Integer> for FixedPoint {
    type Error = TryFromIntegerError;
    fn try_from(value: Integer) -> Result<Self, Self::Error> {
        Ok(FixedPoint {
            span: value.span,
            whole: u64::try_from(value.value).map_err(|_e| TryFromIntegerError {})?,
            femptos: 0,
        })
    }
}

/// See section 2.2.1.
#[derive(Debug, PartialEq, Clone)]
pub struct RealLiteral {
    pub value: f64,
    pub data_type: Option<RealTypeName>,
    /// The literal's position in the source text.
    pub span: SourceSpan,
}

impl RealLiteral {
    pub fn try_parse(a: &str, tn: Option<RealTypeName>) -> Result<Self, &'static str> {
        let (r, remainder): (Vec<_>, Vec<_>) = a.chars().filter(|c| *c != '_').partition(|c| {
            c.is_ascii_digit() || *c == '.' || *c == 'E' || *c == 'e' || *c == '+' || *c == '-'
        });
        if !remainder.is_empty() {
            return Err("Non-real characters");
        }
        let r: String = r.into_iter().collect();
        f64::from_str(r.as_str())
            .map(|value| RealLiteral {
                value,
                data_type: tn,
                span: SourceSpan::default(),
            })
            .map_err(|_e| "real")
    }
}

impl fmt::Display for RealLiteral {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match &self.data_type {
            Some(dt) => write!(f, "{}#{}", dt, self.value),
            None => write!(f, "{}", self.value),
        }
    }
}

#[derive(Clone, Debug, PartialEq)]
pub struct BooleanLiteral {
    pub value: Boolean,
    /// The literal's position in the source text.
    pub span: SourceSpan,
}

impl BooleanLiteral {
    pub fn new(value: Boolean) -> Self {
        Self {
            value,
            span: SourceSpan::default(),
        }
    }
}

impl fmt::Display for BooleanLiteral {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(f, "{}", self.value)
    }
}

// See section 2.2.2
#[derive(Debug, PartialEq, Clone)]
pub struct CharacterStringLiteral {
    pub value: Vec<char>,
    /// The width the source spelled, which is what selects the delimiter:
    /// single quotes for `STRING`, double quotes for `WSTRING`.
    ///
    /// The width belongs on the literal and not only on the declaration it
    /// initializes because a literal also appears in statement bodies, where
    /// there is no declaration to borrow it from.
    pub width: StringType,
    /// The literal's position in the source text.
    pub span: SourceSpan,
}

impl CharacterStringLiteral {
    /// Creates a single-byte (`STRING`) literal.
    pub fn new(value: Vec<char>) -> Self {
        Self {
            value,
            width: StringType::String,
            span: SourceSpan::default(),
        }
    }

    /// Creates a double-byte (`WSTRING`) literal.
    pub fn new_wide(value: Vec<char>) -> Self {
        Self {
            value,
            width: StringType::WString,
            span: SourceSpan::default(),
        }
    }
}

impl fmt::Display for CharacterStringLiteral {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        let s: String = self.value.iter().collect();
        let delimiter = self.width.delimiter();
        write!(f, "{delimiter}{s}{delimiter}")
    }
}

#[derive(Debug, PartialEq, Clone, Recurse)]
pub struct BitStringLiteral {
    pub value: Integer,
    #[recurse(ignore)]
    pub data_type: Option<BitStringTypeName>,
}

impl fmt::Display for BitStringLiteral {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match &self.data_type {
            Some(dt) => write!(f, "{}#{}", dt, self.value),
            None => write!(f, "{}", self.value),
        }
    }
}
