//! The two forms of a message: the header a person reads and the fields a
//! program reads.
//!
//! A person reads the words of the CODESYS Messages view (`Error`, `Warning`,
//! `Information`); a program reads the fields of the CODESYS scripting class
//! `ScriptMessage` and the scripting names of the categories (`FatalError` ...
//! `SuppressedInformation`). Both come from the facts the list of categories in
//! `ironplc-problems-gen` declares for each category, so neither form names a
//! category itself: a category added there reaches both without a change here.

use serde::Serialize;

use super::{Diagnostic, LineColumn};

/// A message as a program reads it: the fields of the CODESYS `ScriptMessage`
/// that a diagnostic has, and the CODESYS code of the same condition.
///
/// The serialized names are the names of the fields. CODESYS has two more that
/// a diagnostic does not: `project` and `position_offset`.
#[derive(Debug, Clone, PartialEq, Eq, Serialize)]
pub struct ScriptMessage {
    /// The scripting name of the category: `FatalError`, `Error`, `Warning`,
    /// `Information`, `Text`, `SuppressedWarning` or `SuppressedInformation`.
    pub severity: &'static str,
    /// The value of the category in the CODESYS `Severity` flag set: 1, 2, 4, 8,
    /// 16, 32 or 64. A mask of categories is the sum of their values.
    pub severity_value: u32,
    /// The prefix of the code of the message, `P` for a problem of the compiler.
    pub prefix: Option<String>,
    /// The number of the code of the message, so `P4007` is `P` and 4007.
    pub number: Option<u32>,
    /// The text of the message.
    pub text: String,
    /// The byte offset in the object where the message starts.
    pub position: usize,
    /// The number of bytes the message covers.
    pub length: usize,
    /// The place for a person, `file:line:column`, counted from 1. It is `None`
    /// when the text of the file is not known.
    pub position_text: Option<String>,
    /// The file the message is about.
    pub object: String,
    /// The CODESYS code that reports the same condition, such as `C0046`, or
    /// `None` when none is confirmed. CODESYS has no such field.
    pub codesys: Option<&'static str>,
}

impl ScriptMessage {
    /// Whether the category of the message is in `severities`, a mask of the
    /// values of `Severity` as `system.get_message_objects` takes it.
    pub fn is_in(&self, severities: u32) -> bool {
        self.severity_value & severities != 0
    }
}

/// The prefix and the number of a code such as `P4007`, or `None` for a code
/// that is not a letter and digits.
fn split_code(code: &str) -> Option<(String, u32)> {
    let mut chars = code.chars();
    let prefix = chars.next().filter(char::is_ascii_alphabetic)?;
    let number = chars.as_str().parse().ok()?;
    Some((prefix.to_string(), number))
}

impl Diagnostic {
    /// The CODESYS code that reports the same condition as the problem of the
    /// diagnostic, whichever constructor made it, or `None` when no CODESYS
    /// message is confirmed to be that condition.
    pub fn codesys(&self) -> Option<&'static str> {
        self.codesys
    }

    /// The header of the diagnostic for a person: the word of its category, the
    /// code, and the CODESYS code when there is one, as in
    /// `Error[P4007] (CODESYS C0046)`. It is `None` for a category without a
    /// word: a text is a line with no header, and a suppressed diagnostic is
    /// not shown.
    pub fn person_header(&self) -> Option<String> {
        let word = self.severity.person_word()?;
        Some(match self.codesys {
            Some(codesys) => format!("{word}[{}] (CODESYS {codesys})", self.code),
            None => format!("{word}[{}]", self.code),
        })
    }

    /// The diagnostic as a line of plain text for a person, with no header and no
    /// code: the text of its primary label when it has one, and its description
    /// otherwise. This is how a `Text` is shown, a line about the build.
    pub fn person_text(&self) -> String {
        if self.primary.message.is_empty() {
            self.description()
        } else {
            self.primary.message.clone()
        }
    }

    /// The diagnostic as a program reads it. `source` is the text of the file
    /// of the primary label; the line and column of `position_text` need it.
    ///
    /// The text is the description, with the text of the primary label after it
    /// when the description does not contain it already.
    pub fn script_message(&self, source: Option<&str>) -> ScriptMessage {
        let description = self.description();
        let label = self.primary.message.as_str();
        let text = if label.is_empty() || description.contains(label) {
            description
        } else {
            format!("{description}: {label}")
        };
        let (prefix, number) = match split_code(&self.code) {
            Some((prefix, number)) => (Some(prefix), Some(number)),
            None => (None, None),
        };
        let start = self.primary.location.start;
        let object = self.primary.file_id.to_string();
        let position_text = source.map(|source| {
            let at = source.floor_char_boundary(start);
            let at = LineColumn::from_offset(source, at);
            format!("{object}:{}:{}", at.line + 1, at.column + 1)
        });
        ScriptMessage {
            severity: self.severity.script_name(),
            severity_value: self.severity.script_value(),
            prefix,
            number,
            text,
            position: start,
            length: self.primary.location.end.saturating_sub(start),
            position_text,
            object,
            codesys: self.codesys,
        }
    }
}

#[cfg(test)]
mod tests;
