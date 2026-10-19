//! The settings that change the handling of warnings: the lists of disabled
//! warnings and of warnings reported as errors.
//!
//! CODESYS keeps two lists of message ids outside the source text
//! (`DisabledWarningIds` and `WarningAsErrorIds`, `WarningHelper.cs`). They are
//! one class here, [`WarningList`], and their rules are written once:
//!
//! - **Only a warning can be listed.** A list holds a [`WarningSet`], whose
//!   elements are the problems that declare the category Warning and no other.
//!   [`WarningSet::from_code`] is the one function that reads a code written in a
//!   setting (the IronPLC code `P4083` or the CODESYS code `C0354`, with the case
//!   exact) and refuses anything else with a message of `P0013`.
//! - **The decision is one function,** [`decide`], an exhaustive `match` over the
//!   category of the message, the area that is written and whether the code is in
//!   each list. Reporting as an error beats disabling, and nothing lowers an
//!   error, because only the category Warning is asked.
//! - **It is applied where the area rule is.** [`Area::keep`] takes the settings
//!   and applies [`MessageSettings::apply`] to each message of the run before its
//!   own rule counts and limits the messages, so no run can leave the settings
//!   out and a raised warning counts as an error.
//!
//! Source-text suppression (`{warning disable}`, `suppress_warning_N`) is another
//! mechanism, and shares no state with these lists.
//!
//! A new list is one member of [`WarningList`], one field of
//! [`MessageSettings`] and one arm of [`decide`]; a new warning in
//! `problem-codes.csv` needs no edit here.

use ironplc_problems::{Text, WarningProblem, WarningSet};

use super::{Area, Diagnostic, MessageHandling, Severity};

/// A list of warnings given in a setting.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum WarningList {
    /// The warnings that are not shown and not counted (`DisabledWarningIds`).
    Disabled,
    /// The warnings that are reported as errors (`WarningAsErrorIds`).
    AsError,
}

impl WarningList {
    /// Every list. `option_key` and the other declarations have an arm for each
    /// member, so a member added to the enum does not compile until it has them,
    /// and the test of this list fails until the member is listed here.
    pub const ALL: [WarningList; 2] = [WarningList::Disabled, WarningList::AsError];

    /// The key of the list in the `options` of the MCP tools. The key in the
    /// `initializationOptions` of the language server is its lowerCamelCase form.
    pub const fn option_key(self) -> &'static str {
        match self {
            WarningList::Disabled => "disabled_warnings",
            WarningList::AsError => "warnings_as_errors",
        }
    }

    /// The command line flag that adds one code to the list; the flag can be
    /// repeated.
    pub const fn cli_flag(self) -> &'static str {
        match self {
            WarningList::Disabled => "--disable-warning",
            WarningList::AsError => "--warning-as-error",
        }
    }

    /// One line that says what the list does.
    pub const fn description(self) -> &'static str {
        match self {
            WarningList::Disabled => {
                "Codes of warnings that are not shown and not counted, such as P4083 or C0354."
            }
            WarningList::AsError => {
                "Codes of warnings that are reported as errors, with the same code, and stop the build, such as P4083 or C0354."
            }
        }
    }
}

/// The lists of warnings given in the settings of a run. Both are empty by
/// default, and then no message changes.
#[derive(Debug, Clone, Copy, Default, PartialEq, Eq)]
pub struct MessageSettings {
    disabled_warnings: WarningSet,
    warnings_as_errors: WarningSet,
}

/// What a run does with one message, once its category, its area and its place
/// in the lists are known.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
enum Decision {
    /// The warning becomes an error with the same code.
    Raise,
    /// A handling of the message ([`Diagnostic::handle`]); `Report` keeps it.
    Handle(MessageHandling),
}

/// The one function that decides what the lists do to a message.
///
/// `disabled` and `raised` say whether the code of the message is in the list of
/// disabled warnings and in the list of warnings reported as errors.
///
/// why, for the rows of the table:
/// - Build: CODESYS converts a warning in `SeverityExtensions.GetSeverity`
///   (`-.352.cs:101-110`) before the gate that does not store and does not count
///   a disabled warning (`-.352.cs:184`), and reporting as an error beats
///   disabling (`WarningHelper.cs:42-51`), so a warning in both lists is an error.
/// - Precompile: CODESYS removes a disabled warning that is not also listed as an
///   error (`PreCompileContext.cs:2446`, `WarningHelper.cs:42-51`) and converts
///   nothing there, so a warning reported as an error stays a warning while
///   editing and is an error in the build.
/// - Every other category is kept as it is: an error cannot be disabled or
///   raised, and nothing lowers an error. Because only `Warning` is asked, a
///   category added to the list does not compile here until it is placed.
fn decide(area: Area, severity: Severity, disabled: bool, raised: bool) -> Decision {
    match severity {
        Severity::Warning => match area {
            Area::Build => match (raised, disabled) {
                (true, true) | (true, false) => Decision::Raise,
                (false, true) => Decision::Handle(MessageHandling::Ignore),
                (false, false) => Decision::Handle(MessageHandling::Report),
            },
            Area::Precompile => match (raised, disabled) {
                (true, true) | (true, false) => Decision::Handle(MessageHandling::Report),
                (false, true) => Decision::Handle(MessageHandling::Ignore),
                (false, false) => Decision::Handle(MessageHandling::Report),
            },
        },
        Severity::FatalError
        | Severity::Error
        | Severity::Information
        | Severity::Text
        | Severity::SuppressedWarning
        | Severity::SuppressedInformation => Decision::Handle(MessageHandling::Report),
    }
}

impl MessageSettings {
    /// The warnings of a list.
    pub fn list(&self, list: WarningList) -> WarningSet {
        match list {
            WarningList::Disabled => self.disabled_warnings,
            WarningList::AsError => self.warnings_as_errors,
        }
    }

    /// Adds warnings to a list.
    pub fn add(&mut self, list: WarningList, warnings: WarningSet) {
        match list {
            WarningList::Disabled => {
                self.disabled_warnings = self.disabled_warnings.union(warnings);
            }
            WarningList::AsError => {
                self.warnings_as_errors = self.warnings_as_errors.union(warnings);
            }
        }
    }

    /// Adds the warnings that codes written in a setting name to a list, reading
    /// each code with [`WarningSet::from_code`]. A code that is not accepted is
    /// returned as its message and does not stop the others, so a front end can
    /// report every bad code of a setting at once; the accepted codes are added.
    pub fn add_codes<'a>(
        &mut self,
        list: WarningList,
        codes: impl IntoIterator<Item = &'a str>,
    ) -> Vec<Text> {
        let mut refused = Vec::new();
        for code in codes {
            match WarningSet::from_code(code) {
                Ok(warnings) => self.add(list, warnings),
                Err(text) => refused.push(text),
            }
        }
        refused
    }

    /// What the lists do to a message of a run that writes `area`: the message as
    /// it stays, or `None` when it is dropped.
    ///
    /// A message whose code is no warning (an error, a message without a code)
    /// is in neither list and is returned as it is.
    pub fn apply(&self, area: Area, diagnostic: Diagnostic) -> Option<Diagnostic> {
        let warning = WarningProblem::from_code(&diagnostic.code);
        let listed = |list: WarningSet| warning.is_some_and(|warning| list.contains(warning));
        let decision = decide(
            area,
            diagnostic.severity(),
            listed(self.disabled_warnings),
            listed(self.warnings_as_errors),
        );
        match decision {
            Decision::Raise => Some(diagnostic.raise()),
            Decision::Handle(handling) => diagnostic.handle(handling),
        }
    }
}

#[cfg(test)]
mod tests;
