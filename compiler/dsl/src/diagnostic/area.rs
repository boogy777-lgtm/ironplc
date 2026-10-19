//! The areas: separate lists of messages that a run writes and a viewer
//! selects.
//!
//! CODESYS keeps its messages in separate lists, its message categories
//! (`CompilerMessageCategory` for a build, `PreCompileMessageCategory` for the
//! check while editing), and each list shows only its own messages. The set is
//! open: a component adds its own list. Two facts say which messages go where:
//!
//! - a run writes exactly one [`Area`], and the area declares what it keeps of
//!   the messages of a run ([`Area::keep`]), the `source` text an editor shows
//!   for it ([`Area::source`]) and how many messages it holds
//!   ([`Area::capacity`]);
//! - a message declares the areas it is shown in ([`ShownIn`]), as the CODESYS
//!   `ShowAttribute` does, when it is made, and an area keeps only the messages
//!   shown in it.
//!
//! A new area is one new member of [`Area`]: the compiler then asks for its
//! name, source, description, capacity and keep rule, and no other place
//! changes. The consumers match on `Area` without a wildcard arm.

use super::{run, Diagnostic, MessageSettings, Summary, CHECK_SUMMARY};

/// A list of messages, with the rule for what it keeps of the messages of a run.
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum Area {
    /// The messages of a compile or a build: the command line `check` and
    /// `compile`, the program form of MCP and the playground. CODESYS
    /// `CompilerMessageCategory`.
    Build,
    /// The messages of the check made while editing: the language server's
    /// analysis. CODESYS `PreCompileMessageCategory`.
    Precompile,
}

impl Area {
    /// Every area. `index` has an arm for each member, so a member added to the
    /// enum does not compile until it has one, and the test of this list fails
    /// until the member is listed here.
    pub const ALL: [Area; 2] = [Area::Build, Area::Precompile];

    /// The position of the member in [`Area::ALL`]. It is also the bit of the
    /// area in [`ShownIn`].
    const fn index(self) -> usize {
        match self {
            Area::Build => 0,
            Area::Precompile => 1,
        }
    }

    /// The name of the area, as the Messages view of CODESYS names it.
    pub fn name(self) -> &'static str {
        match self {
            Area::Build => "Build",
            Area::Precompile => "Precompile",
        }
    }

    /// The `source` of a diagnostic of the area in an editor. It names the
    /// product and the area, so that an editor's Problems filter can select one
    /// area.
    pub fn source(self) -> &'static str {
        match self {
            Area::Build => "IronPLC Build",
            Area::Precompile => "IronPLC Precompile",
        }
    }

    /// One line that says which messages the area holds and who writes it.
    pub fn description(self) -> &'static str {
        match self {
            Area::Build => {
                "The messages of a compile or a build, with the summary lines and the limit lines. Written by `check`, `compile` and the tools of MCP."
            }
            Area::Precompile => {
                "The messages of the check made while editing: only messages that have a position, at most 950 in total. Written by the language server."
            }
        }
    }

    /// How many messages the area holds at most, or `None` when it has no cap of
    /// its own (the build is limited by the counts of its kinds, see
    /// [`Area::keep`]).
    ///
    /// CODESYS caps the Precompile area at 950 (`PreCompileContext.cs:2440-2525`).
    pub fn capacity(self) -> Option<usize> {
        match self {
            Area::Build => None,
            Area::Precompile => Some(950),
        }
    }

    /// The summary that closes a check written to the area, or `None` when the
    /// area has none. CODESYS ends a compile with "Compile complete"
    /// (`LanguageModelManagerConsolidated.cs:1972-1989`) and its precompile
    /// ends with no line (`PreCompileContext.cs`).
    pub fn check_summary(self) -> Option<&'static Summary> {
        match self {
            Area::Build => Some(&CHECK_SUMMARY),
            Area::Precompile => None,
        }
    }

    /// What the area keeps of the messages one run produced, in the order they
    /// were produced. The run applies it once, where it collects its messages.
    ///
    /// - `Build`: a fatal error ends the run, no repeats, at
    ///   most 500 errors and 500 warnings with the line of the limit
    ///   (`run::kept`).
    /// - `Precompile`: only the messages shown in it that are not suppressed,
    ///   and no repeats. It has no limit lines and does not end at a fatal
    ///   error: those are rules of the build (`-.352.cs:93-215`), and the
    ///   precompile of CODESYS has none of them (`PreCompileContext.cs`). Its
    ///   cap is on what it stores, see [`Area::store`].
    ///
    /// The lists of the settings of the run are applied first, to each message
    /// ([`MessageSettings::apply`]): a disabled warning is gone before the area
    /// counts and limits anything, and a warning reported as an error is counted as
    /// an error. The settings are a parameter, and not a field the caller may
    /// forget to set, so that no run keeps its messages without them.
    pub fn keep(self, settings: &MessageSettings, diagnostics: Vec<Diagnostic>) -> Vec<Diagnostic> {
        let handled: Vec<Diagnostic> = diagnostics
            .into_iter()
            .filter_map(|diagnostic| settings.apply(self, diagnostic))
            .collect();
        match self {
            Area::Build => run::kept(handled),
            Area::Precompile => run::kept_for_precompile(handled),
        }
    }

    /// What the area stores of the messages written to it, for the objects it
    /// holds messages of. `written` lists the objects in the order they were
    /// written, the oldest first, each with its messages.
    ///
    /// The rule of CODESYS (`PreCompileContext.cs:2440-2525`): an object keeps at
    /// most [`Area::capacity`] messages, the first it produced, and when the area
    /// would hold more than its capacity in total, the oldest messages are
    /// removed, the first stored. An area without a capacity stores everything.
    ///
    /// The object is whatever the viewer groups messages by: the language server
    /// uses the file.
    pub fn store<K, M>(self, written: Vec<(K, Vec<M>)>) -> Vec<(K, Vec<M>)> {
        let Some(capacity) = self.capacity() else {
            return written;
        };
        let mut stored: Vec<(K, Vec<M>)> = written
            .into_iter()
            .map(|(object, mut messages)| {
                messages.truncate(capacity);
                (object, messages)
            })
            .collect();
        let total: usize = stored.iter().map(|(_, messages)| messages.len()).sum();
        let mut excess = total.saturating_sub(capacity);
        for (_, messages) in stored.iter_mut() {
            if excess == 0 {
                break;
            }
            let removed = excess.min(messages.len());
            messages.drain(..removed);
            excess -= removed;
        }
        stored
    }
}

/// The areas a message is shown in, as the CODESYS `ShowAttribute`
/// (`None`, `Precompile`, `Compile`, `All`). A message is given it when it is
/// made and it never changes.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct ShownIn(u8);

impl ShownIn {
    /// Every area: a message that has a position.
    pub const EVERYWHERE: ShownIn = ShownIn((1 << Area::ALL.len()) - 1);

    /// One area alone.
    pub const fn only(area: Area) -> ShownIn {
        ShownIn(1 << area.index())
    }

    /// Whether the message is shown in the area.
    pub const fn contains(self, area: Area) -> bool {
        self.0 & (1 << area.index()) != 0
    }

    /// A message that has no position: it is shown in the Build area alone.
    /// CODESYS clears `Precompile` from the `ShowAttribute` of a message made
    /// without a position (`CompilerMessage.cs:26-60`), because a viewer of
    /// the check while editing has no place to put it.
    pub const NO_POSITION: ShownIn = ShownIn::only(Area::Build);
}

#[cfg(test)]
mod tests;
