//! What a run decides about one message: report it, suppress it or ignore it.
//!
//! CODESYS decides each message of its checker with a `MessageHandling`
//! (`PreCompile/Typification/MessageHandling.cs`) and applies the decision in
//! one way (`TypeCheckerVisitor.cs:380-393`, `-.278.cs:98-103` and
//! `:707-724`):
//!
//! | Handling | Effect on a warning | Effect on an error |
//! |----------|---------------------|--------------------|
//! | `Report` | stored as a warning | stored as an error |
//! | `Suppress` | stored as a suppressed warning | dropped |
//! | `Ignore` | dropped | dropped |
//!
//! The category is what a message is; the handling is what a run decides about
//! it. [`Diagnostic::handle`] is the one function that applies a handling, an
//! exhaustive `match` with no wildcard arm over the handling and, where the
//! answer depends on it, the category, so a category added to the list does not
//! compile here until this function knows what `Suppress` does to it.
//! [`Diagnostic::suppress`] is the change of category that `Suppress` makes, not
//! a second rule.

use super::{Diagnostic, Severity};

/// The decision a run makes about one message.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum MessageHandling {
    /// Keep the message as it is.
    Report,
    /// Keep a warning as a suppressed warning and an information message as a
    /// suppressed information message, which are not shown; drop an error.
    Suppress,
    /// Drop the message.
    Ignore,
}

impl MessageHandling {
    /// Every handling.
    pub const ALL: [MessageHandling; 3] = [
        MessageHandling::Report,
        MessageHandling::Suppress,
        MessageHandling::Ignore,
    ];
}

impl Diagnostic {
    /// Applies a handling to the message: the message as it stays, or `None` when
    /// the handling drops it.
    ///
    /// - `Report` keeps every message as it is.
    /// - `Ignore` drops every message.
    /// - `Suppress` gives a warning and an information message their suppressed
    ///   category (`Diagnostic::suppress`), drops an error and a fatal error as
    ///   CODESYS does (`-.278.cs:98-103`: any handling other than `Report` drops
    ///   an error), and leaves a text and the two suppressed categories as they
    ///   are, because they have no category to be suppressed to.
    ///
    /// why: CODESYS asks for a handling of a warning and of an error only. The
    /// arms for an information message and a text follow the answers the list of
    /// categories already gives (`Severity::suppressed`): `SuppressedInformation`
    /// exists for the information message that belongs to a suppressed warning
    /// (a related position, MSG-061), and a text has no suppressed category.
    pub fn handle(self, handling: MessageHandling) -> Option<Diagnostic> {
        match handling {
            MessageHandling::Report => Some(self),
            MessageHandling::Ignore => None,
            MessageHandling::Suppress => match self.severity {
                Severity::FatalError | Severity::Error => None,
                Severity::Warning | Severity::Information => Some(self.suppress()),
                Severity::Text | Severity::SuppressedWarning | Severity::SuppressedInformation => {
                    Some(self)
                }
            },
        }
    }
}

#[cfg(test)]
mod tests;
