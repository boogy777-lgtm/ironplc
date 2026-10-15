//! The constant pool and the interning of constants into it.

use super::CompileContext;

/// A constant in the pool: integer, float, or string.
///
/// `Str` holds Latin-1 bytes (narrow STRING); `WStr` holds UTF-16LE bytes
/// (wide WSTRING). The encoding is tagged per pool entry so the VM can verify
/// it against the destination (ADR-0034).
#[derive(Debug, PartialEq)]
pub(crate) enum PoolConstant {
    I32(i32),
    I64(i64),
    F32(f32),
    F64(f64),
    Str(Vec<u8>),
    WStr(Vec<u8>),
}

/// Returns the index of `value` in `constants`, appending it if absent.
///
/// Free-standing rather than a `CompileContext` method because the peephole
/// optimizer interns constants while holding only the pool — it has no
/// context to reach through. `CompileContext::add_i32_constant` delegates
/// here so both paths dedupe against the same pool.
pub(crate) fn intern_i32_constant(constants: &mut Vec<PoolConstant>, value: i32) -> u16 {
    if let Some(i) = constants
        .iter()
        .position(|c| matches!(c, PoolConstant::I32(v) if *v == value))
    {
        return i as u16;
    }
    let index = constants.len() as u16;
    constants.push(PoolConstant::I32(value));
    index
}

impl CompileContext {
    /// Returns the index of an existing constant matching `matches`, or
    /// pushes `new_value` and returns its index.
    fn intern_constant(
        &mut self,
        matches: impl FnMut(&PoolConstant) -> bool,
        new_value: PoolConstant,
    ) -> u16 {
        if let Some(i) = self.constants.iter().position(matches) {
            return i as u16;
        }
        let index = self.constants.len() as u16;
        self.constants.push(new_value);
        index
    }

    /// Adds an i32 constant to the pool and returns its index.
    pub(crate) fn add_i32_constant(&mut self, value: i32) -> u16 {
        intern_i32_constant(&mut self.constants, value)
    }

    /// Adds an f32 constant to the pool and returns its index.
    ///
    /// Equality uses `to_bits()` so that distinct NaN bit patterns are not
    /// collapsed to the same pool entry.
    pub(crate) fn add_f32_constant(&mut self, value: f32) -> u16 {
        self.intern_constant(
            |c| matches!(c, PoolConstant::F32(v) if v.to_bits() == value.to_bits()),
            PoolConstant::F32(value),
        )
    }

    /// Adds an f64 constant to the pool and returns its index.
    ///
    /// Equality uses `to_bits()` so that distinct NaN bit patterns are not
    /// collapsed to the same pool entry.
    pub(crate) fn add_f64_constant(&mut self, value: f64) -> u16 {
        self.intern_constant(
            |c| matches!(c, PoolConstant::F64(v) if v.to_bits() == value.to_bits()),
            PoolConstant::F64(value),
        )
    }

    /// Adds an i64 constant to the pool and returns its index.
    pub(crate) fn add_i64_constant(&mut self, value: i64) -> u16 {
        self.intern_constant(
            |c| matches!(c, PoolConstant::I64(v) if *v == value),
            PoolConstant::I64(value),
        )
    }

    /// Adds a string constant (raw bytes) to the pool and returns its index.
    pub(crate) fn add_str_constant(&mut self, value: Vec<u8>) -> u16 {
        if let Some(i) = self
            .constants
            .iter()
            .position(|c| matches!(c, PoolConstant::Str(v) if v == &value))
        {
            return i as u16;
        }
        let index = self.constants.len() as u16;
        self.constants.push(PoolConstant::Str(value));
        index
    }

    /// Adds a wide (UTF-16LE / WSTRING) string constant, deduplicating against
    /// existing wide entries. Kept distinct from [`Self::add_str_constant`] so
    /// the pool entry carries the wide encoding tag the VM verifies.
    pub(crate) fn add_wstr_constant(&mut self, value: Vec<u8>) -> u16 {
        if let Some(i) = self
            .constants
            .iter()
            .position(|c| matches!(c, PoolConstant::WStr(v) if v == &value))
        {
            return i as u16;
        }
        let index = self.constants.len() as u16;
        self.constants.push(PoolConstant::WStr(value));
        index
    }
}
