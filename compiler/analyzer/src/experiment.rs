//! Switches of the prototypes of this branch. They exist so that one binary
//! runs a path before and after a change and the two results can be compared
//! in one process. The branch is never merged.

use std::sync::atomic::{AtomicU8, Ordering};

/// Not read yet.
const UNSET: u8 = u8::MAX;

/// A switch: its level comes from the environment variable of its name the
/// first time it is read, and a caller can set it afterwards.
pub struct Switch {
    variable: &'static str,
    level: AtomicU8,
}

impl Switch {
    pub const fn new(variable: &'static str) -> Self {
        Self {
            variable,
            level: AtomicU8::new(UNSET),
        }
    }

    /// The level: 0 is the behavior of the branch it was cut from.
    pub fn level(&self) -> u8 {
        let level = self.level.load(Ordering::Relaxed);
        if level != UNSET {
            return level;
        }
        let level = std::env::var(self.variable)
            .ok()
            .and_then(|value| value.parse::<u8>().ok())
            .unwrap_or(0);
        self.level.store(level, Ordering::Relaxed);
        level
    }

    pub fn set(&self, level: u8) {
        self.level.store(level, Ordering::Relaxed);
    }
}

/// B1: a pass keeps no copy of the library. 1 = no copy.
pub static NO_FALLBACK: Switch = Switch::new("IRONPLC_EXP_NO_FALLBACK");

/// B2: the standard environments are built once per process. 1 = a built base
/// is cloned for each analysis; 2 = the function environment is a layer over
/// a shared base and the type environment is cloned.
pub static STANDARD_ENVIRONMENTS: Switch = Switch::new("IRONPLC_EXP_STANDARD_ENVIRONMENTS");
