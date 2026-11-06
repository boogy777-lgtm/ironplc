//! The note that the device keeps of an unfinished edit, so that the boot
//! after a restart can say the candidate was lost.
//!
//! A candidate lives in RAM only (owner decision): a restart loses a staged or
//! tested candidate, the device boots the assembled application, and the
//! engineer repeats the edit. This note is not the candidate and never brings
//! it back; it is the memory of the device that there was one, with the label,
//! the origin and the stage of the edit and the content hash of the candidate,
//! so that the `identity` answer of the next boot reports it
//! ([`ironplc_runtime::CandidateLost`]).
//!
//! The file is `<file>.edit` beside the served container, next to the slot
//! store's files, and its temporary file is `<file>.edit.tmp`. It is written
//! when a candidate is accepted, rewritten when the stage changes (test,
//! untest) and removed when the edit ends (assemble, cancel); every write is a
//! temporary file renamed over the file ([`durable_file::replace_file`]), so
//! the file is the old note or the new, never a torn mix. At boot the file is
//! read, reported and removed.
//!
//! A member of the class that [`crate::stored_item`] states: its reader returns
//! a [`StoredItem`]. A note that cannot be read is still reported, without the
//! details, because an edit was lost and the engineer should know.
//!
//! A write that fails is logged with its error and the device goes on. The
//! note is advisory: the candidate it describes is in RAM only, and an
//! acknowledgment that turned into an error would tell the engineer that the
//! accept failed while the candidate stands. The cost of a missed write is that
//! a restart does not say that the candidate was lost.

use std::fs;
use std::io;
use std::path::PathBuf;

use ironplc_cli_support::durable_file;
use ironplc_container::integrity::hash_hex;
use ironplc_runtime::{edit_view, CandidateLost, EditStage, HostStatus};
use serde::{Deserialize, Serialize};

use crate::slot_store::suffix;
use crate::stored_item::StoredItem;

/// The only version of the file this device reads and writes.
const FILE_VERSION: u32 = 1;

/// The content of the file: the unfinished edit as of the last write.
#[derive(Clone, Debug, PartialEq, Eq, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub(crate) struct EditMarker {
    version: u32,
    #[serde(default, skip_serializing_if = "Option::is_none")]
    name: Option<String>,
    #[serde(default, skip_serializing_if = "Option::is_none")]
    origin: Option<String>,
    /// `accepted`, `testing`, `testingMigration` or `testingReplace`.
    stage: EditStage,
    /// The `contentHash` of the candidate in the form the session reports it;
    /// all zeros when the writer of the container computed none.
    candidate_content_hash: String,
}

/// What a host status asks of the file.
enum Wanted {
    /// A note of this edit.
    Note(EditMarker),
    /// No file: there is no edit.
    NoFile,
    /// Leave the file as it is: a swap is pending, the boundary decides the
    /// stage, and the sync that follows it records the stage.
    AsItIs,
}

impl Wanted {
    fn of(status: &HostStatus) -> Wanted {
        let stage = edit_view(status).stage;
        match stage {
            EditStage::Idle => Wanted::NoFile,
            EditStage::SwapPending => Wanted::AsItIs,
            EditStage::Accepted
            | EditStage::Testing
            | EditStage::TestingMigration
            | EditStage::TestingReplace => {
                let record = status.pending_edit.as_ref();
                Wanted::Note(EditMarker {
                    version: FILE_VERSION,
                    name: record.and_then(|record| record.name.clone()),
                    origin: record.and_then(|record| record.origin.clone()),
                    stage,
                    candidate_content_hash: status
                        .candidate_content_hash
                        .map(|hash| hash_hex(&hash).to_string())
                        .unwrap_or_default(),
                })
            }
        }
    }
}

/// The note file of one served container, and what the device last wrote to it.
pub(crate) struct EditMarkerStore {
    path: PathBuf,
    temporary: PathBuf,
    /// The note the file holds, as far as this process knows: `None` when the
    /// file is absent. An equal note is not written again.
    written: Option<EditMarker>,
}

impl EditMarkerStore {
    /// The store beside the served container `served`; touches no disk.
    pub(crate) fn beside(served: &std::path::Path) -> Self {
        EditMarkerStore {
            path: suffix(served, "edit"),
            temporary: suffix(served, "edit.tmp"),
            written: None,
        }
    }

    /// Reads the file. A missing file is [`StoredItem::Absent`]; a file that
    /// cannot be read or does not decode is [`StoredItem::Damaged`].
    fn load(&self) -> StoredItem<EditMarker> {
        let bytes = match fs::read(&self.path) {
            Ok(bytes) => bytes,
            Err(err) if err.kind() == io::ErrorKind::NotFound => return StoredItem::Absent,
            Err(err) => {
                log::warn!("unable to read {}: {err}", self.path.display());
                return StoredItem::Damaged;
            }
        };
        match serde_json::from_slice::<EditMarker>(&bytes) {
            Ok(marker) if marker.version == FILE_VERSION => StoredItem::Found(marker),
            Ok(marker) => {
                log::warn!(
                    "{} has version {}, not {FILE_VERSION}",
                    self.path.display(),
                    marker.version
                );
                StoredItem::Damaged
            }
            Err(err) => {
                log::warn!("{} is not a note of an edit: {err}", self.path.display());
                StoredItem::Damaged
            }
        }
    }

    /// The boot step: reads the file, removes it, and returns what the
    /// `identity` answer reports. `None` when no edit was unfinished. Nothing is
    /// restored: the candidate is gone, and the note only says so.
    pub(crate) fn boot(&mut self) -> Option<CandidateLost> {
        let lost = match self.load() {
            StoredItem::Absent => return None,
            StoredItem::Damaged => CandidateLost {
                name: None,
                origin: None,
                stage: None,
            },
            StoredItem::Found(marker) => {
                log::warn!(
                    "the previous run ended with an unfinished edit (stage {:?}, candidate {}): the candidate is lost, the assembled application boots",
                    marker.stage,
                    marker.candidate_content_hash
                );
                CandidateLost {
                    name: marker.name,
                    origin: marker.origin,
                    stage: Some(marker.stage),
                }
            }
        };
        self.delete();
        Some(lost)
    }

    /// Makes the file say what `status` says: the note of the edit when a
    /// candidate is staged or under Test, no file when there is none. A pending
    /// swap leaves the file as it is. Writes only on a change. A failure is
    /// logged (see the module documentation) and tried again at the next sync.
    pub(crate) fn sync(&mut self, status: &HostStatus) {
        match Wanted::of(status) {
            Wanted::AsItIs => {}
            Wanted::NoFile => self.remove(),
            Wanted::Note(marker) => self.write(marker),
        }
    }

    fn write(&mut self, marker: EditMarker) {
        if self.written.as_ref() == Some(&marker) {
            return;
        }
        let bytes = match serde_json::to_vec_pretty(&marker) {
            Ok(bytes) => bytes,
            Err(err) => {
                log::error!("unable to render the note of the unfinished edit: {err}");
                return;
            }
        };
        match durable_file::replace_file(&self.path, &self.temporary, &bytes) {
            Ok(()) => self.written = Some(marker),
            Err(err) => log::error!(
                "unable to write the note of the unfinished edit {}: {err}",
                self.path.display()
            ),
        }
    }

    /// Removes the file when this process knows it holds a note: a device that
    /// is not editing does this at every command, and costs nothing.
    fn remove(&mut self) {
        if self.written.is_some() {
            self.delete();
        }
    }

    fn delete(&mut self) {
        match fs::remove_file(&self.path) {
            Ok(()) => self.written = None,
            Err(err) if err.kind() == io::ErrorKind::NotFound => self.written = None,
            Err(err) => log::error!(
                "unable to remove the note of the unfinished edit {}: {err}",
                self.path.display()
            ),
        }
    }
}
