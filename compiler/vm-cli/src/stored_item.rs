//! What a device does at boot with an item it keeps beside the application.
//!
//! The class (device durability): data a device keeps across a restart, with
//! the tag of the application it belongs to. The application's slots
//! ([`crate::slot_store`]), the device settings ([`crate::device_settings`]),
//! the device id and the retained image ([`crate::retained_store`]) are
//! members; the engineering records (ADR-0083), which the device never reads,
//! are not.
//!
//! The invariant for every member: a stored item is applied only when its tag
//! equals the application that is booting, and an item that is damaged or
//! belongs to another application is discarded and reported in the `identity`
//! answer, never applied and never silent. Both halves are in this module so
//! that the next stored item (a table of forced variables) has one place to
//! reuse: its reader returns a [`StoredItem`], its boot function returns a
//! [`BootResult`], and the exhaustive `match`es below make the compiler list
//! the places a new [`DiscardReason`] touches.

use ironplc_runtime::BootNote;

/// What reading a stored item found. A value of this type is not yet checked
/// against the application that is booting.
#[derive(Debug, PartialEq, Eq)]
pub(crate) enum StoredItem<T> {
    /// A well-formed item.
    Found(T),
    /// Something is stored and it cannot be used: the bytes are not an item of
    /// this kind, or the storage cannot be read.
    Damaged,
    /// Nothing is stored.
    Absent,
}

/// Why a stored item was not applied. Nothing was written to the application.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub(crate) enum DiscardReason {
    /// The item is damaged: its bytes do not decode, its storage cannot be
    /// read, or its content does not have the length its tag promises.
    Damaged,
    /// The item belongs to another application: the content hash of its tag is
    /// not the content hash of the application that boots.
    OtherApplication,
    /// The item belongs to an application with the same content hash that
    /// retains other slots or bytes (another layout of the retained runs).
    OtherRetainedLayout,
    /// The host had already run a scan round, so applying the item would have
    /// overwritten live state. A defect of the caller, reported and not hidden.
    NotAtBoot,
}

impl DiscardReason {
    /// The word the `identity` answer carries.
    fn wire(self) -> &'static str {
        match self {
            DiscardReason::Damaged => "damaged",
            DiscardReason::OtherApplication => "otherApplication",
            DiscardReason::OtherRetainedLayout => "otherRetainedLayout",
            DiscardReason::NotAtBoot => "notAtBoot",
        }
    }
}

/// What the device did with one stored item at boot.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub(crate) enum BootResult {
    /// The item belonged to the booting application and was applied.
    Applied,
    /// The item was stored and not applied, for this reason.
    Discarded(DiscardReason),
    /// Nothing was stored.
    Absent,
}

impl BootResult {
    /// The note the `identity` answer carries for the item.
    pub(crate) fn note(self) -> BootNote {
        match self {
            BootResult::Applied => BootNote {
                result: "applied".to_owned(),
                reason: None,
            },
            BootResult::Discarded(reason) => BootNote {
                result: "discarded".to_owned(),
                reason: Some(reason.wire().to_owned()),
            },
            BootResult::Absent => BootNote {
                result: "absent".to_owned(),
                reason: None,
            },
        }
    }
}

#[cfg(test)]
mod tests {
    use rstest::rstest;

    use super::*;

    #[test]
    fn note_when_applied_then_result_applied_without_reason() {
        let note = BootResult::Applied.note();

        assert_eq!(note.result, "applied");
        assert_eq!(note.reason, None);
    }

    #[test]
    fn note_when_absent_then_result_absent_without_reason() {
        let note = BootResult::Absent.note();

        assert_eq!(note.result, "absent");
        assert_eq!(note.reason, None);
    }

    #[rstest]
    #[case::damaged(DiscardReason::Damaged, "damaged")]
    #[case::other_application(DiscardReason::OtherApplication, "otherApplication")]
    #[case::other_layout(DiscardReason::OtherRetainedLayout, "otherRetainedLayout")]
    #[case::not_at_boot(DiscardReason::NotAtBoot, "notAtBoot")]
    fn note_when_discarded_then_result_discarded_and_the_reason_word(
        #[case] reason: DiscardReason,
        #[case] word: &str,
    ) {
        let note = BootResult::Discarded(reason).note();

        assert_eq!(note.result, "discarded");
        assert_eq!(note.reason.as_deref(), Some(word));
    }
}
