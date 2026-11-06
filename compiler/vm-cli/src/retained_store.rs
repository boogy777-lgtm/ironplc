//! The file that keeps the retained image across a restart of the device
//! (ADR-0085).
//!
//! The file is `<file>.retained` beside the served container, next to the slot
//! store's files; its temporary file is `<file>.retained.tmp`. Its content is
//! [`RetainedImage::encode`], which carries the tag of the application the
//! values were read from, so the file needs no other header. Every write is a
//! temporary file renamed over the file ([`durable_file::replace_file`]): the
//! file is the old image or the new, never a torn mix.
//!
//! A member of the class that [`crate::stored_item`] states: at boot the image
//! is applied only when its tag equals the booting application, and every other
//! outcome is a [`BootResult`] that the `identity` answer reports. The writes
//! are the device's to schedule; see [`crate::device_settings::device::Device`]
//! for when they happen (a controlled stop, `assembleEdits`, and the optional
//! period).

use std::fs;
use std::io;
use std::path::PathBuf;
use std::time::Instant;

use ironplc_cli_support::durable_file;
use ironplc_runtime::{HostMode, RetainedApplyError, RetainedImage, RetainedMismatch, RuntimeHost};

use crate::error::{self, VmError};
use crate::slot_store::suffix;
use crate::stored_item::{BootResult, DiscardReason, StoredItem};

/// Milliseconds since the store was made. A test passes its own, so that a
/// period elapses without a sleep.
type Clock = Box<dyn Fn() -> u64 + Send>;

/// The retained file of one served container, and what the device last wrote
/// to it.
pub(crate) struct RetainedStore {
    path: PathBuf,
    temporary: PathBuf,
    /// The image the file holds, when this process knows it: what boot applied
    /// or what was last written. `None` when the file is absent, discarded or
    /// not yet written. A write of an equal image is skipped.
    last_saved: Option<RetainedImage>,
    /// The clock reading of the last write attempt, successful or not, so that
    /// a failing write is not retried at every scan boundary.
    last_write: u64,
    clock: Clock,
}

impl RetainedStore {
    /// The store beside the served container `served`; touches no disk.
    pub(crate) fn beside(served: &std::path::Path) -> Self {
        let started = Instant::now();
        let clock: Clock =
            Box::new(move || u64::try_from(started.elapsed().as_millis()).unwrap_or(u64::MAX));
        RetainedStore {
            path: suffix(served, "retained"),
            temporary: suffix(served, "retained.tmp"),
            last_saved: None,
            last_write: clock(),
            clock,
        }
    }

    /// [`RetainedStore::beside`] with the clock of the caller.
    #[cfg(test)]
    pub(crate) fn with_clock(mut self, clock: Clock) -> Self {
        self.last_write = clock();
        self.clock = clock;
        self
    }

    /// Reads the file. A missing file is [`StoredItem::Absent`]; a file that
    /// cannot be read or does not decode is [`StoredItem::Damaged`], and the
    /// reason is logged. Whether the image belongs to the booting application
    /// is [`RetainedStore::boot`]'s question.
    pub(crate) fn load(&self) -> StoredItem<RetainedImage> {
        let bytes = match fs::read(&self.path) {
            Ok(bytes) => bytes,
            Err(err) if err.kind() == io::ErrorKind::NotFound => return StoredItem::Absent,
            Err(err) => {
                log::warn!("unable to read {}: {err}", self.path.display());
                return StoredItem::Damaged;
            }
        };
        match RetainedImage::decode(&bytes) {
            Ok(image) => StoredItem::Found(image),
            Err(err) => {
                log::warn!("{} is not a retained image: {err}", self.path.display());
                StoredItem::Damaged
            }
        }
    }

    /// The boot step: reads the file and applies the image to `host`, which
    /// has not run a scan round. The image is applied only when its tag equals
    /// the host's application; otherwise nothing is written to the host and
    /// the result names why. The file is left as it is in every case, so a
    /// discarded image stays on the disk until the next write replaces it.
    pub(crate) fn boot(&mut self, host: &mut RuntimeHost) -> BootResult {
        let image = match self.load() {
            StoredItem::Absent => return BootResult::Absent,
            StoredItem::Damaged => return BootResult::Discarded(DiscardReason::Damaged),
            StoredItem::Found(image) => image,
        };
        match host.apply_retained(&image) {
            Ok(()) => {
                self.last_saved = Some(image);
                BootResult::Applied
            }
            Err(err) => {
                let reason = discard_reason(err);
                log::warn!("discarded {}: {err}", self.path.display());
                BootResult::Discarded(reason)
            }
        }
    }

    /// Writes `image` over the file, atomically. V6018 when it cannot be
    /// written; the file is then the old image.
    pub(crate) fn save(&mut self, image: &RetainedImage) -> Result<(), VmError> {
        self.last_write = (self.clock)();
        durable_file::replace_file(&self.path, &self.temporary, &image.encode()).map_err(
            |err| {
                VmError::io(
                    error::RETAINED_WRITE,
                    format!(
                        "Unable to replace the retained file {}: {err}",
                        self.path.display()
                    ),
                )
            },
        )?;
        self.last_saved = Some(image.clone());
        Ok(())
    }

    /// Writes the retained image of `host` unless the file already holds an
    /// equal one. Returns whether it wrote.
    ///
    /// A host that runs a candidate under Test writes nothing: its values are
    /// tagged with the candidate, the device boots the normal application, and
    /// the write would replace the image that application can still use.
    pub(crate) fn save_host(&mut self, host: &RuntimeHost) -> Result<bool, VmError> {
        if host.status().mode != HostMode::Normal {
            return Ok(false);
        }
        let image = host.retained_image();
        if self.last_saved.as_ref() == Some(&image) {
            return Ok(false);
        }
        self.save(&image)?;
        Ok(true)
    }

    /// The periodic write, for a scan boundary: writes the image of `host`
    /// when `period_seconds` is not 0, that long has passed since the last
    /// write attempt, and the image differs from the one in the file. A device
    /// that writes at every boundary of a changing image would wear its
    /// storage, so at most one write falls in a period. Returns whether it
    /// wrote.
    pub(crate) fn save_when_due(
        &mut self,
        host: &RuntimeHost,
        period_seconds: u64,
    ) -> Result<bool, VmError> {
        if period_seconds == 0 {
            return Ok(false);
        }
        let waited = (self.clock)().saturating_sub(self.last_write);
        if waited < period_seconds.saturating_mul(1000) {
            return Ok(false);
        }
        self.save_host(host)
    }
}

/// Why `err` kept an image from being applied. No wildcard arm: a new way to
/// refuse an image does not compile until it has a reason.
fn discard_reason(err: RetainedApplyError) -> DiscardReason {
    match err {
        RetainedApplyError::AfterFirstRound => DiscardReason::NotAtBoot,
        RetainedApplyError::Mismatch(RetainedMismatch::ContentHash) => {
            DiscardReason::OtherApplication
        }
        RetainedApplyError::Mismatch(RetainedMismatch::Runs) => DiscardReason::OtherRetainedLayout,
        RetainedApplyError::Mismatch(RetainedMismatch::Length) => DiscardReason::Damaged,
    }
}

#[cfg(test)]
mod tests;
