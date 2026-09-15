//! Generation identifiers for online change.
//!
//! The baseline distinguishes two counters. [`LogicGeneration`] versions one
//! compiled logic artifact (an `.iplc` image); a candidate staged for online
//! change receives the next value. [`ApplicationGeneration`] versions the
//! active manifest. For the P0 full-image swap the manifest holds a single
//! logic generation, so test and untest move the active code without
//! committing a new manifest and only `assemble` advances the application
//! generation.

use core::fmt;

/// Version of one compiled logic artifact.
#[derive(Clone, Copy, Debug, PartialEq, Eq, PartialOrd, Ord, Hash)]
pub struct LogicGeneration(u32);

impl LogicGeneration {
    /// Creates a generation from its raw value.
    pub const fn new(raw: u32) -> Self {
        Self(raw)
    }

    /// Returns the raw value.
    pub const fn raw(self) -> u32 {
        self.0
    }
}

impl fmt::Display for LogicGeneration {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(f, "{}", self.0)
    }
}

/// Version of the active application manifest.
#[derive(Clone, Copy, Debug, PartialEq, Eq, PartialOrd, Ord, Hash)]
pub struct ApplicationGeneration(u32);

impl ApplicationGeneration {
    /// Creates a generation from its raw value.
    pub const fn new(raw: u32) -> Self {
        Self(raw)
    }

    /// Returns the raw value.
    pub const fn raw(self) -> u32 {
        self.0
    }
}

impl fmt::Display for ApplicationGeneration {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(f, "{}", self.0)
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn generation_display_when_formatted_then_writes_the_raw_value() {
        assert_eq!(LogicGeneration::new(7).to_string(), "7");
        assert_eq!(ApplicationGeneration::new(3).to_string(), "3");
    }
}
