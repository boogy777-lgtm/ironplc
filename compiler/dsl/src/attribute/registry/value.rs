//! The values an attribute takes: one type for each value kind, with the one
//! function that reads it.
//!
//! A row names the type of its value. [`Value::parse`] is the only reading of
//! that type: the reader of a row returns what it gives, and the check of a
//! value asks whether it gives anything, so what the check accepts and what the
//! reader gives cannot differ. A type states its [`ValueKind`] beside its
//! `parse`, and the kind is what a message and the reference page say the
//! attribute takes.

/// What an attribute takes, as the user is told. One kind for each type of
/// value; the matches over it have one arm each and no wildcard, so a kind
/// added later does not compile until every one of them answers.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum ValueKind {
    /// No value: a value is a fault.
    Flag,
    /// A value is required and is one of the texts, without regard to case.
    OneOf(&'static [&'static str]),
    /// As `OneOf`, and the attribute may have no value.
    OptionalOneOf(&'static [&'static str]),
    /// A value is required: an unsigned decimal integer that fits `u32`.
    Number,
    /// A value is required and is not empty.
    Text,
    /// Any value, or none.
    OptionalText,
}

impl ValueKind {
    /// What the attribute takes, for a message and the reference page.
    pub fn describe(self) -> String {
        match self {
            ValueKind::Flag => "no value".to_string(),
            ValueKind::OneOf(texts) => format!("one of {}", texts.join(", ")),
            ValueKind::OptionalOneOf(texts) => {
                format!("no value, or one of {}", texts.join(", "))
            }
            ValueKind::Number => "an unsigned integer".to_string(),
            ValueKind::Text => "a text that is not empty".to_string(),
            ValueKind::OptionalText => "any text, or no value".to_string(),
        }
    }
}

/// A type of value of an attribute.
pub trait Value: Sized {
    /// What the type takes.
    const KIND: ValueKind;

    /// The value that `text` is, or nothing when the attribute cannot have that
    /// value. `text` is the value as written, and is `None` when the attribute
    /// has none.
    fn parse(text: Option<&str>) -> Option<Self>;
}

/// The value of an attribute that takes none.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub struct Flag;

impl Value for Flag {
    const KIND: ValueKind = ValueKind::Flag;

    fn parse(text: Option<&str>) -> Option<Self> {
        text.is_none().then_some(Flag)
    }
}

/// The value of an attribute that takes an unsigned integer.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub struct Number(pub u32);

impl Value for Number {
    const KIND: ValueKind = ValueKind::Number;

    fn parse(text: Option<&str>) -> Option<Self> {
        let digits = text?.trim();
        let is_decimal = !digits.is_empty() && digits.bytes().all(|byte| byte.is_ascii_digit());
        if is_decimal {
            digits.parse().ok().map(Number)
        } else {
            None
        }
    }
}

/// The value of an attribute that takes a text that is not empty.
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct Text(pub String);

impl Value for Text {
    const KIND: ValueKind = ValueKind::Text;

    fn parse(text: Option<&str>) -> Option<Self> {
        let text = text?;
        (!text.trim().is_empty()).then(|| Text(text.to_string()))
    }
}

/// The value of an attribute that takes any text or none.
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct OptionalText(pub Option<String>);

impl Value for OptionalText {
    const KIND: ValueKind = ValueKind::OptionalText;

    fn parse(text: Option<&str>) -> Option<Self> {
        Some(OptionalText(text.map(str::to_string)))
    }
}

/// A value that is one of a list of texts.
pub trait Choice: Sized + Copy + 'static {
    /// Each text that is read, with what it is read as. Several texts may be
    /// read as one value.
    const WORDS: &'static [(&'static str, Self)];

    /// The texts of [`Choice::WORDS`], for the message and the reference page.
    const TEXTS: &'static [&'static str];

    /// The choice that `text` is, without regard to case and to the white
    /// space around it.
    fn from_text(text: &str) -> Option<Self> {
        let text = text.trim();
        Self::WORDS
            .iter()
            .find(|(word, _)| word.eq_ignore_ascii_case(text))
            .map(|(_, choice)| *choice)
    }
}

/// Declares a type of value that is one of a list of texts, from one list of
/// the variants and the texts each is read from: the type, the reading and the
/// texts a message gives come from the same tokens.
macro_rules! choices {
    ($(
        $(#[$meta:meta])*
        $name:ident { $($variant:ident = $($text:literal)|+),+ $(,)? }
    )*) => {
        $(
            $(#[$meta])*
            #[derive(Clone, Copy, Debug, PartialEq, Eq)]
            pub enum $name {
                $($variant,)+
            }

            impl Choice for $name {
                const WORDS: &'static [(&'static str, Self)] =
                    &[$($(($text, $name::$variant),)+)+];
                const TEXTS: &'static [&'static str] = &[$($($text,)+)+];
            }

            impl Value for $name {
                const KIND: ValueKind = ValueKind::OneOf(<$name as Choice>::TEXTS);

                fn parse(text: Option<&str>) -> Option<Self> {
                    <$name as Choice>::from_text(text?)
                }
            }
        )*
    };
}

choices! {
    /// How the members of a structure are aligned (`pack_mode`).
    PackMode {
        Mode0 = "0",
        Mode1 = "1",
        Mode2 = "2",
        Mode4 = "4",
        Mode8 = "8",
    }

    /// The number format of a value while monitoring (`displaymode`).
    DisplayMode {
        Binary = "bin" | "binary",
        Decimal = "dec" | "decimal",
        Hexadecimal = "hex" | "hexadecimal",
    }

    /// How the value of a property is read while monitoring (`monitoring`).
    MonitoringMode {
        Variable = "variable",
        Call = "call",
    }

    /// How a string value is decoded while monitoring (`monitoring_encoding`).
    MonitoringEncoding {
        Utf8 = "UTF8" | "UTF-8",
    }

    /// The access to an exported symbol (`symbol`).
    SymbolAccess {
        NoAccess = "none",
        Read = "read",
        Write = "write",
        ReadWrite = "readwrite",
    }
}

/// The value of an attribute that takes one of a list of texts, or none.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub struct Optional<C>(pub Option<C>);

impl<C: Choice> Value for Optional<C> {
    const KIND: ValueKind = ValueKind::OptionalOneOf(C::TEXTS);

    fn parse(text: Option<&str>) -> Option<Self> {
        match text {
            None => Some(Optional(None)),
            Some(text) => C::from_text(text).map(|choice| Optional(Some(choice))),
        }
    }
}

#[cfg(test)]
mod tests;
