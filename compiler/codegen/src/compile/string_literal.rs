//! String variable metadata and the encoding of string literals into constant-pool bytes.

use ironplc_container::CharWidth;
use ironplc_dsl::common::StringType;

use super::CompileContext;
use crate::emit::Emitter;

/// Metadata for a STRING/WSTRING variable allocated in the data region.
#[derive(Clone)]
pub(crate) struct StringVarInfo {
    /// How bytecode names the run of the data region the string occupies.
    pub(crate) place: crate::string_storage::StringPlace,
    /// Maximum number of code units this string can hold.
    pub(crate) max_length: u16,
    /// Per-code-unit byte width: `Narrow` for STRING, `Wide` for WSTRING.
    pub(crate) char_width: CharWidth,
}

/// Per-code-unit byte width for STRING (Latin-1 per ADR-0016).
pub(crate) const NARROW_CHAR_WIDTH: CharWidth = CharWidth::Narrow;
/// Per-code-unit byte width for WSTRING (UTF-16LE per ADR-0016).
pub(crate) const WIDE_CHAR_WIDTH: CharWidth = CharWidth::Wide;

/// Returns the per-code-unit byte width for an IEC string type:
/// `Narrow` for STRING, `Wide` for WSTRING (ADR-0016).
pub(crate) fn char_width_for_string_type(width: &StringType) -> CharWidth {
    match width {
        StringType::String => NARROW_CHAR_WIDTH,
        StringType::WString => WIDE_CHAR_WIDTH,
    }
}

/// Encode a character-string literal into bytes for the constant pool.
///
/// `char_width` selects the encoding per ADR-0016: `Narrow` for STRING
/// (Latin-1, one byte per character), `Wide` for WSTRING (UTF-16LE, two
/// bytes per code unit). The narrowing casts cannot lose information: the
/// analyzer rejects a literal whose characters do not fit its type (P4052,
/// at most U+00FF for STRING and U+FFFF for WSTRING) before codegen runs.
pub(crate) fn encode_string_literal(chars: &[char], char_width: CharWidth) -> Vec<u8> {
    match char_width {
        CharWidth::Narrow => chars.iter().map(|&ch| ch as u8).collect(),
        CharWidth::Wide => chars
            .iter()
            .flat_map(|&ch| (ch as u16).to_le_bytes())
            .collect(),
    }
}

/// Loads a string literal into a temp buffer at the given encoding width.
///
/// Encodes the literal (Latin-1 for narrow, UTF-16LE for wide), registers it as
/// a width-tagged constant-pool entry, and emits LOAD_CONST_STR (leaving
/// `buf_idx` on the stack). The caller stores that temp buffer into a
/// destination of the same width; the VM verifies the encoding match
/// (ADR-0034).
pub(crate) fn emit_string_literal_load(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    chars: &[char],
    char_width: CharWidth,
) {
    let bytes = encode_string_literal(chars, char_width);
    let pool_index = if char_width.is_wide() {
        ctx.add_wstr_constant(bytes)
    } else {
        ctx.add_str_constant(bytes)
    };
    emitter.emit_load_const_str(pool_index);
}
