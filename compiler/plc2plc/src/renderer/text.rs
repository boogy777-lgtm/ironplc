//! Text the renderer spells itself instead of assembling from the nodes it
//! visits: the characters of a string, and the attribute pragmas that stand
//! before a declaration.
//!
//! An attribute is written before the declaration it belongs to, one pragma
//! per line, as `{attribute 'name'}` or `{attribute 'name' := 'value'}`, with
//! the name and the value as written. Parsing the text puts each attribute
//! back on the declaration it came from: a declaration the renderer writes
//! after a keyword is written after its attributes, and the attributes of a
//! declaration of a block stand before the block's keyword (a block holds one
//! declaration here), which attaches them to the declaration.

use super::LibraryRenderer;
use ironplc_dsl::attribute::{Attribute, Attributed};
use ironplc_dsl::common::StringType;

/// The spelling of a character string: its characters, `$`-escaped where
/// they cannot appear as themselves, inside the delimiter `width` selects.
pub(super) fn character_string_text(width: &StringType, value: &[char]) -> String {
    let delimiter = width.delimiter();
    let mut val = String::from(delimiter);
    val.push_str(&ironplc_dsl::string_escape::encode(value, width));
    val.push(delimiter);
    val
}

/// The pragma that spells `attribute`.
fn attribute_pragma(attribute: &Attribute) -> String {
    match &attribute.value {
        Some(value) => format!("{{attribute '{}' := '{value}'}}", attribute.name),
        None => format!("{{attribute '{}'}}", attribute.name),
    }
}

impl LibraryRenderer {
    /// Writes the attributes of a declaration, one pragma per line, in the
    /// order they were written.
    pub(super) fn write_attributes(&mut self, declaration: &impl Attributed) {
        for attribute in declaration.attributes().iter() {
            self.write_ws(&attribute_pragma(attribute));
            self.newline();
        }
    }

    /// Writes the attributes of a declaration and then the keyword that opens
    /// it.
    pub(super) fn write_declaration_keyword(
        &mut self,
        declaration: &impl Attributed,
        keyword: &str,
    ) {
        self.write_attributes(declaration);
        self.write_ws(keyword);
    }
}
