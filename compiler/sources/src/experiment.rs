//! Switches of the prototypes of this branch (see the analyzer crate's
//! `experiment` module). The branch is never merged.

use std::sync::atomic::{AtomicU8, Ordering};

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

/// B3: the bundled compatibility libraries are parsed once per process.
/// 1 = a parsed library is kept under a key of what the parse reads.
pub static LIBRARY_CACHE: Switch = Switch::new("IRONPLC_EXP_LIBRARY_CACHE");
