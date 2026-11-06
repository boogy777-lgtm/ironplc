//! What the device keeps beside the application and when it writes it: the
//! retained image (ADR-0085) and the facts about it that `identity` reports.
//!
//! The retained image is written at three points, all through
//! [`RetainedStore`]:
//!
//! - a controlled stop ([`Device::stopped`]): the device loop ends at the end of
//!   its input or at Ctrl+C, on stdio and on TCP;
//! - `assembleEdits` ([`Device::assembled`]): the image of the application that
//!   was just committed, tagged with its content hash;
//! - a scan boundary, when the `retainedSavePeriod` setting is on
//!   ([`Device::scan_boundary`]).
//!
//! A write that fails is V6018. At the periodic point it is logged and the
//! device goes on; at the other two the caller reports it (the wire at
//! assemble, the exit of the process at a stop). Nothing stops the device.

use ironplc_runtime::{IdentityPayload, RuntimeHost, StatusPayload};

use super::Device;
use crate::error::VmError;
use crate::stored_item::BootResult;

/// The content hash of the application `host` runs as normal, in the form the
/// session reports it.
fn content_hash_of(host: &RuntimeHost) -> Option<String> {
    StatusPayload::from(host.status()).content_hash
}

impl Device {
    /// The boot step, right after the host is composed and before its first
    /// scan round: notes the application the device boots, and applies the
    /// retained file to it when the file belongs to that application. The
    /// result is what `identity` reports; see [`BootResult`].
    pub(crate) fn boot_application(&mut self, host: &mut RuntimeHost) {
        self.kept.persisted_content_hash = content_hash_of(host);
        if let Some(store) = &mut self.kept.retained {
            let result = store.boot(host);
            match result {
                BootResult::Applied => log::info!("applied the retained values"),
                BootResult::Absent => log::info!("no retained values are stored"),
                BootResult::Discarded(reason) => {
                    log::warn!("discarded the stored retained values: {reason:?}")
                }
            }
            self.kept.retained_boot = Some(result);
        }
    }

    /// The commit of `assembleEdits` reached the slot store: the device would
    /// now boot the application `host` runs, and the retained image of that
    /// application is written. V6018 when it cannot be; the commit stands.
    pub(crate) fn assembled(&mut self, host: &RuntimeHost) -> Result<(), VmError> {
        self.kept.persisted_content_hash = content_hash_of(host);
        self.save_retained(host)
    }

    /// A controlled stop: writes the retained image. V6018 when it cannot be.
    pub(crate) fn stopped(&mut self, host: &RuntimeHost) -> Result<(), VmError> {
        self.save_retained(host)
    }

    /// A scan boundary: writes the retained image when the period of the
    /// `retainedSavePeriod` setting has passed and the values changed. A
    /// failure is logged with its code and the device goes on; the next period
    /// tries again.
    pub(crate) fn scan_boundary(&mut self, host: &RuntimeHost) {
        // The device loop calls this at every round: the common case (the
        // period is off) must cost no more than reading the setting.
        let period = self.retained_save_period();
        if period == 0 || !self.runs_persisted_application(host) {
            return;
        }
        if let Some(store) = &mut self.kept.retained {
            if let Err(err) = store.save_when_due(host, period) {
                log::error!("periodic write of the retained values failed: {err}");
            }
        }
    }

    /// Fills the facts of the device that the runtime layer cannot know into an
    /// `identity` answer: what it stores and what it did with it at boot, and
    /// the fault of its scan. The one place they are put into the payload, for
    /// every transport.
    pub(crate) fn complete_identity(&self, payload: &mut IdentityPayload) {
        payload.persisted_content_hash = self.kept.persisted_content_hash.clone();
        payload.retained = self.kept.retained_boot.map(BootResult::note);
        payload.fault = self.fault.clone();
    }

    /// Whether the application `host` runs is the one the device would boot.
    /// After a commit that did not reach the slot store (V6012) it is not, and
    /// the values of the running application are not written: a later boot
    /// would find an image of an application it does not boot, and the image
    /// of the one it boots would be gone.
    fn runs_persisted_application(&self, host: &RuntimeHost) -> bool {
        content_hash_of(host) == self.kept.persisted_content_hash
    }

    fn save_retained(&mut self, host: &RuntimeHost) -> Result<(), VmError> {
        if !self.runs_persisted_application(host) {
            log::warn!(
                "the retained values are not written: the running application is not the one \
                 the device would boot"
            );
            return Ok(());
        }
        match &mut self.kept.retained {
            Some(store) => store.save_host(host).map(|_| ()),
            None => Ok(()),
        }
    }

    /// Gives the retained file the clock of a test.
    #[cfg(test)]
    pub(crate) fn use_retained_clock(&mut self, clock: Box<dyn Fn() -> u64 + Send>) {
        if let Some(store) = self.kept.retained.take() {
            self.kept.retained = Some(store.with_clock(clock));
        }
    }
}
