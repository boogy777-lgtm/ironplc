//! The A/B slot store (ADR-0064 amendment): file-backed persistence of the
//! committed artifact beside the container `ironplcvm serve` runs.
//!
//! The store owns five files beside the served container `<file>`:
//!
//! - `<file>.slot-a` / `<file>.slot-b`: two whole generations of the
//!   artifact. Slot contents are exactly the candidate's wire bytes from
//!   `acceptEdits` — never a re-serialization.
//! - `<file>.marker`: a tiny `{slot, seq}` record; `seq` increments per
//!   commit and is the age authority at boot.
//! - `<file>.tmp` / `<file>.marker.tmp`: commit-time residue.
//!
//! The durability invariant: bytes reach a slot only as a verified tmp plus
//! rename, and the marker-named slot is never a write target — so a crash
//! always leaves at least one verified generation (old active or new
//! inactive), never both-invalid. The marker is a boot hint; the verifiable
//! bytes are the durability point.
//!
//! Commit steps, in order: write the wire bytes to `<file>.tmp` and fsync;
//! verify the tmp with the same call boot uses ([`Container::read_from`] —
//! content-hash check plus load-time verification); write `<file>.marker.tmp`
//! naming the inactive slot at `seq + 1` and fsync — *before* the slot swap,
//! so a crash never leaves new bytes in a slot that no marker residue names
//! (the crash table's during-4/5 rows resolve by `seq`); delete the inactive
//! slot if present (`std::fs::rename` never overwrites on Windows; the
//! marker-named slot is never the delete target); rename the tmp into the
//! inactive slot; delete the old marker; rename `marker.tmp` into the marker
//! (same delete-before-rename constraint).
//!
//! Boot adoption: read the marker, then `marker.tmp`; boot the highest-`seq`
//! record whose slot verifies, healing a missing marker; if no record names
//! a verifiable slot (missing, corrupt, or stale marker), boot the newest
//! verifiable bytes — the slots the residue does not name first, then a
//! verifying tmp (residue of a pre-rename crash); if nothing verifies and no
//! store exists at all, seed from the served file (its bytes become slot A
//! and the marker is written, so the rollback anchor exists from the first
//! commit on); if nothing verifies but residue exists, refuse — the device
//! would boot unverified bytes.
//!
//! Crash windows and boot resolutions (ADR-0064):
//!
//! | Crash point | On-disk state at boot | Resolution |
//! |---|---|---|
//! | tmp write/verify | tmp partial or unverified; marker and active intact | boot the marker's generation; discard tmp |
//! | before slot swap | inactive absent or stale; marker residue names the in-flight slot | boot the marker's (previous) generation |
//! | after slot swap, before marker flip | inactive holds verified new bytes; marker residue names it | adopt the verified inactive |
//! | marker flip | marker or marker.tmp names the new slot | boot the newest verifiable bytes the residue names |
//!
//! Single owner: only `ironplcvm serve` composes a store — the one
//! file-backed shell with an assemble path.

use std::fmt;
use std::fs::{self, File};
use std::io::{Cursor, Read, Write};
use std::path::{Path, PathBuf};

use ironplc_container::Container;
use serde::{Deserialize, Serialize};

use crate::error::{self, VmError};

/// One flash slot of the A/B pair.
#[derive(Clone, Copy, Debug, PartialEq, Eq, Serialize, Deserialize)]
#[serde(rename_all = "lowercase")]
enum Slot {
    A,
    B,
}

impl Slot {
    /// The slot a commit writes: always the one the marker does not name.
    fn other(self) -> Self {
        match self {
            Slot::A => Slot::B,
            Slot::B => Slot::A,
        }
    }
}

/// The boot hint: which slot is active and how new it is. Written only as a
/// tmp + rename, like the slots themselves.
#[derive(Clone, Copy, Debug, PartialEq, Eq, Serialize, Deserialize)]
struct Marker {
    slot: Slot,
    seq: u64,
}

/// File-backed A/B slot persistence beside the served container.
pub struct SlotStore {
    /// The served container path — the bootstrap anchor on first serve.
    file: PathBuf,
    slot_a: PathBuf,
    slot_b: PathBuf,
    marker: PathBuf,
    marker_tmp: PathBuf,
    tmp: PathBuf,
}

impl SlotStore {
    /// Builds the store layout beside `file` without touching disk.
    pub fn beside(file: &Path) -> Self {
        SlotStore {
            slot_a: suffix(file, "slot-a"),
            slot_b: suffix(file, "slot-b"),
            marker: suffix(file, "marker"),
            marker_tmp: suffix(file, "marker.tmp"),
            tmp: suffix(file, "tmp"),
            file: file.to_path_buf(),
        }
    }

    /// Boots the committed artifact, adopting the newest verifiable
    /// generation per the crash table. First serve of a plain file seeds the
    /// store: the file's bytes become slot A and the marker is written.
    pub fn boot(&self) -> Result<Container, VmError> {
        let mut records: Vec<Marker> = [
            self.read_marker(&self.marker),
            self.read_marker(&self.marker_tmp),
        ]
        .into_iter()
        .flatten()
        .collect();
        records.sort_by_key(|record| std::cmp::Reverse(record.seq));

        // Highest-seq record whose slot verifies; heal the marker afterwards
        // and discard the tmp residue (see [`SlotStore::discard_tmp`]).
        for record in &records {
            if let Some(container) = self.verify_slot(record.slot) {
                if let Err(err) = self.heal_marker(record) {
                    log::warn!("slot store marker heal failed (adoption stands): {err}");
                }
                self.discard_tmp();
                return Ok(container);
            }
        }

        // No marker record names a verifiable slot (missing, corrupt, or
        // stale): the newest verifiable bytes are the durability point — the
        // slots the residue does not name first, then a verifying tmp.
        for slot in [Slot::A, Slot::B] {
            if records.iter().any(|record| record.slot == slot) {
                continue;
            }
            if let Some(container) = self.verify_slot(slot) {
                if let Err(err) = self.heal_marker(&Marker { slot, seq: 1 }) {
                    log::warn!("slot store marker heal failed (adoption stands): {err}");
                }
                return Ok(container);
            }
        }
        if let Some(container) = self.verify_file(&self.tmp) {
            return Ok(container);
        }
        // Unverifiable tmp is garbage residue of a crashed write: discard it.
        let _ = fs::remove_file(&self.tmp);

        if self.store_files_present() {
            return Err(VmError::io(
                error::CONTAINER_READ,
                format!(
                    "no verifiable committed artifact beside {}",
                    self.file.display()
                ),
            ));
        }

        // First serve of a plain file: seed the store from it.
        let bytes = fs::read(&self.file).map_err(|err| {
            VmError::io(
                error::FILE_OPEN,
                format!("Unable to open {}: {err}", self.file.display()),
            )
        })?;
        let container = Container::read_from(&mut Cursor::new(&bytes)).map_err(|err| {
            VmError::io(
                error::CONTAINER_READ,
                format!("Unable to read container {}: {err}", self.file.display()),
            )
        })?;
        self.write_fsync(&self.slot_a, &bytes)?;
        self.write_fsync(
            &self.marker,
            &marker_json(&Marker {
                slot: Slot::A,
                seq: 1,
            }),
        )?;
        Ok(container)
    }

    /// Persists one commit: the ordered steps of the durability invariant.
    /// Every failure surfaces as V6012 — the caller's RAM promotion stands,
    /// but the client learns the commit is live and not durable.
    pub fn commit(&self, wire: &[u8]) -> Result<(), VmError> {
        let marker = self.read_marker(&self.marker).ok_or_else(|| {
            persist_error(format!(
                "no readable marker beside {} (store not booted)",
                self.file.display()
            ))
        })?;
        let next = Marker {
            slot: marker.slot.other(),
            seq: marker.seq.saturating_add(1),
        };

        // Bytes reach the store only as a verified tmp plus rename.
        self.write_fsync(&self.tmp, wire)?;
        self.verify_file(&self.tmp).ok_or_else(|| {
            persist_error("committed bytes failed load verification as a tmp file")
        })?;

        // Marker residue before the slot swap: from here on, a crash leaves
        // marker.tmp naming the in-flight slot, so boot can always tell the
        // new bytes from the old ones by seq.
        self.write_fsync(&self.marker_tmp, &marker_json(&next))?;

        // rename never overwrites on Windows: delete the inactive slot
        // first. The marker-named slot is never the delete target.
        self.delete_if_present(&self.slot_path(next.slot))?;
        fs::rename(&self.tmp, self.slot_path(next.slot)).map_err(|err| {
            persist_error(format!(
                "unable to rename tmp into the inactive slot: {err}"
            ))
        })?;

        // Flip the marker under the same delete-before-rename constraint.
        self.delete_if_present(&self.marker)?;
        fs::rename(&self.marker_tmp, &self.marker).map_err(|err| {
            persist_error(format!(
                "unable to rename the marker residue into place: {err}"
            ))
        })?;
        Ok(())
    }

    /// Heals the marker after adoption: the adopted record becomes the
    /// marker (tmp + rename, like every other store file) and the residue
    /// that named it is removed. Failure is the caller's to weigh: boot has
    /// verified bytes either way, and adoption reruns next boot.
    fn heal_marker(&self, adopted: &Marker) -> Result<(), VmError> {
        if self.read_marker(&self.marker) == Some(*adopted) && !self.marker_tmp.exists() {
            return Ok(());
        }
        self.write_fsync(&self.marker_tmp, &marker_json(adopted))?;
        self.delete_if_present(&self.marker)?;
        fs::rename(&self.marker_tmp, &self.marker).map_err(|err| {
            persist_error(format!("unable to heal the marker after adoption: {err}"))
        })?;
        Ok(())
    }

    /// Removes the tmp residue after an adoption named by marker records.
    /// Even a verifying tmp is only residue here: the commit interrupted
    /// before its rename and flip, so no acknowledgment was ever sent for
    /// those bytes (the crash table's during-2–3 resolution).
    fn discard_tmp(&self) {
        match fs::remove_file(&self.tmp) {
            Ok(()) => {}
            Err(err) if err.kind() == std::io::ErrorKind::NotFound => {}
            Err(err) => log::warn!("unable to discard the slot store tmp residue: {err}"),
        }
    }

    /// Parses one marker file. A missing or corrupt hint is not a boot
    /// failure: `None`, and the adoption algorithm decides.
    fn read_marker(&self, path: &Path) -> Option<Marker> {
        let mut text = String::new();
        File::open(path).ok()?.read_to_string(&mut text).ok()?;
        serde_json::from_str(&text).ok()
    }

    /// Verifies one slot file with the same call boot uses.
    fn verify_slot(&self, slot: Slot) -> Option<Container> {
        self.verify_file(&self.slot_path(slot))
    }

    /// Reads and verifies bytes with [`Container::read_from`]: content-hash
    /// check plus load-time verification in one call (ADR-0006).
    fn verify_file(&self, path: &Path) -> Option<Container> {
        let mut bytes = Vec::new();
        File::open(path).ok()?.read_to_end(&mut bytes).ok()?;
        Container::read_from(&mut Cursor::new(&bytes)).ok()
    }

    /// Writes `bytes` to `path`, flushing and fsyncing the file. Directory
    /// durability has no portable std API and stays the residual risk the
    /// adoption algorithm exists to heal.
    fn write_fsync(&self, path: &Path, bytes: &[u8]) -> Result<(), VmError> {
        let mut file = File::create(path)
            .map_err(|err| persist_error(format!("unable to create {}: {err}", path.display())))?;
        file.write_all(bytes)
            .map_err(|err| persist_error(format!("unable to write {}: {err}", path.display())))?;
        file.flush()
            .map_err(|err| persist_error(format!("unable to flush {}: {err}", path.display())))?;
        file.sync_all()
            .map_err(|err| persist_error(format!("unable to fsync {}: {err}", path.display())))?;
        Ok(())
    }

    /// Deletes `path` if present; a missing file is the normal case (the
    /// first commit has no inactive slot yet).
    fn delete_if_present(&self, path: &Path) -> Result<(), VmError> {
        match fs::remove_file(path) {
            Ok(()) => Ok(()),
            Err(err) if err.kind() == std::io::ErrorKind::NotFound => Ok(()),
            Err(err) => Err(persist_error(format!(
                "unable to delete {}: {err}",
                path.display()
            ))),
        }
    }

    /// Whether any store file exists beside the served container.
    fn store_files_present(&self) -> bool {
        self.slot_a.exists()
            || self.slot_b.exists()
            || self.marker.exists()
            || self.marker_tmp.exists()
            || self.tmp.exists()
    }

    fn slot_path(&self, slot: Slot) -> PathBuf {
        match slot {
            Slot::A => self.slot_a.clone(),
            Slot::B => self.slot_b.clone(),
        }
    }
}

/// The sibling path `<file>.<suffix>` (the suffix appends to the served
/// file's full name, e.g. `app.iplc.slot-a`).
fn suffix(file: &Path, extension: &str) -> PathBuf {
    let mut name = file.as_os_str().to_os_string();
    name.push(".");
    name.push(extension);
    PathBuf::from(name)
}

/// Renders one marker record. `serde_json` cannot fail to serialize this
/// flat struct; an empty render would fail verification downstream, not
/// silently persist.
fn marker_json(marker: &Marker) -> Vec<u8> {
    serde_json::to_vec(marker).unwrap_or_default()
}

/// Builds the V6012 error for a failed persist step.
fn persist_error(context: impl fmt::Display) -> VmError {
    VmError::io(
        error::SLOT_COMMIT_PERSIST,
        format!("unable to persist the assembled commit to the slot store: {context}"),
    )
}

#[cfg(test)]
mod tests {
    use super::*;
    use ironplc_container::test_support::container_bytes;
    use ironplc_container::{ContainerBuilder, FunctionId};
    use tempfile::TempDir;

    /// Wire bytes of a minimal verifiable artifact; `seed` distinguishes
    /// generations (it lands in the constant pool).
    fn artifact(seed: i32) -> Vec<u8> {
        let container = ContainerBuilder::new()
            .num_variables(0)
            .add_i32_constant(seed)
            .add_function(FunctionId::new(0), &[0x8C], 1, 0, 0)
            .max_call_depth(1)
            .build();
        container_bytes(&container)
    }

    /// The served file (generation 1) and its store in a fresh temp dir.
    fn store_in(dir: &TempDir) -> SlotStore {
        let file = dir.path().join("app.iplc");
        std::fs::write(&file, artifact(1)).unwrap();
        SlotStore::beside(&file)
    }

    fn slot(dir: &TempDir, name: &str) -> PathBuf {
        dir.path().join(format!("app.iplc.{name}"))
    }

    fn write_marker(dir: &TempDir, name: &str, slot_name: &str, seq: u64) {
        let marker = serde_json::json!({"slot": slot_name, "seq": seq}).to_string();
        std::fs::write(slot(dir, name), marker).unwrap();
    }

    fn marker_of(dir: &TempDir, name: &str) -> serde_json::Value {
        let text = std::fs::read_to_string(slot(dir, name)).unwrap();
        serde_json::from_str(&text).unwrap()
    }

    #[test]
    fn boot_when_no_store_then_seeds_slot_a_and_marker_from_the_file() {
        let dir = TempDir::new().unwrap();
        let store = store_in(&dir);

        let booted = store.boot().unwrap();

        // The served bytes boot and become the rollback anchor.
        assert_eq!(container_bytes(&booted), artifact(1));
        assert_eq!(std::fs::read(slot(&dir, "slot-a")).unwrap(), artifact(1));
        assert_eq!(
            marker_of(&dir, "marker"),
            serde_json::json!({"slot": "a", "seq": 1})
        );
    }

    #[test]
    fn boot_when_file_is_not_a_container_then_v6002() {
        let dir = TempDir::new().unwrap();
        let file = dir.path().join("app.iplc");
        std::fs::write(&file, "not a container").unwrap();
        let store = SlotStore::beside(&file);

        let err = store.boot().unwrap_err();

        assert!(err.to_string().starts_with("V6002"));
    }

    #[test]
    fn commit_when_committed_then_wire_bytes_land_verified_in_inactive_slot() {
        let dir = TempDir::new().unwrap();
        let store = store_in(&dir);
        store.boot().unwrap();

        store.commit(&artifact(2)).unwrap();

        // Slot contents are exactly the committed wire bytes.
        assert_eq!(std::fs::read(slot(&dir, "slot-b")).unwrap(), artifact(2));
        assert_eq!(
            marker_of(&dir, "marker"),
            serde_json::json!({"slot": "b", "seq": 2})
        );

        // The next commit alternates back to slot A at seq 3.
        store.commit(&artifact(3)).unwrap();
        assert_eq!(std::fs::read(slot(&dir, "slot-a")).unwrap(), artifact(3));
        assert_eq!(
            marker_of(&dir, "marker"),
            serde_json::json!({"slot": "a", "seq": 3})
        );
    }

    /// Crash during tmp write/verify: unverified tmp, marker and active
    /// intact → boot the marker's generation; discard the tmp.
    #[test]
    fn boot_when_tmp_unverified_and_marker_intact_then_boots_marker_generation() {
        let dir = TempDir::new().unwrap();
        let store = store_in(&dir);
        store.boot().unwrap();
        std::fs::write(slot(&dir, "tmp"), "partial write").unwrap();

        let booted = store.boot().unwrap();

        assert_eq!(container_bytes(&booted), artifact(1));
        assert!(!slot(&dir, "tmp").exists());
    }

    /// Crash before the slot swap: the marker residue names the in-flight
    /// slot, which is absent; the marker's (previous) generation boots.
    #[test]
    fn boot_when_inactive_absent_and_marker_intact_then_boots_active() {
        let dir = TempDir::new().unwrap();
        let store = store_in(&dir);
        store.boot().unwrap();
        write_marker(&dir, "marker.tmp", "b", 2);
        std::fs::write(slot(&dir, "tmp"), artifact(2)).unwrap();

        let booted = store.boot().unwrap();

        assert_eq!(container_bytes(&booted), artifact(1));
        assert_eq!(
            marker_of(&dir, "marker"),
            serde_json::json!({"slot": "a", "seq": 1})
        );
    }

    /// Crash after the slot swap, before the marker flip: the verified new
    /// bytes sit in the inactive slot and the marker residue names them →
    /// adopt the verified inactive.
    #[test]
    fn boot_when_inactive_verified_and_marker_names_active_then_adopts_inactive() {
        let dir = TempDir::new().unwrap();
        let store = store_in(&dir);
        store.boot().unwrap();
        write_marker(&dir, "marker.tmp", "b", 2);
        std::fs::write(slot(&dir, "slot-b"), artifact(2)).unwrap();

        let booted = store.boot().unwrap();

        assert_eq!(container_bytes(&booted), artifact(2));
        // The flip is healed: the marker names the adopted slot; the
        // residue is gone.
        assert_eq!(
            marker_of(&dir, "marker"),
            serde_json::json!({"slot": "b", "seq": 2})
        );
        assert!(!slot(&dir, "marker.tmp").exists());
    }

    /// Crash during the marker flip: the old marker is gone, the residue
    /// names the new slot → boot the residue and heal the marker.
    #[test]
    fn boot_when_marker_missing_and_residue_names_verified_slot_then_adopts_and_heals() {
        let dir = TempDir::new().unwrap();
        let store = store_in(&dir);
        store.boot().unwrap();
        write_marker(&dir, "marker.tmp", "b", 2);
        std::fs::write(slot(&dir, "slot-b"), artifact(2)).unwrap();
        std::fs::remove_file(slot(&dir, "marker")).unwrap();

        let booted = store.boot().unwrap();

        assert_eq!(container_bytes(&booted), artifact(2));
        assert_eq!(
            marker_of(&dir, "marker"),
            serde_json::json!({"slot": "b", "seq": 2})
        );
        assert!(!slot(&dir, "marker.tmp").exists());
    }

    /// Steady state: both slots verify; the marker names the newest → boot
    /// the marker's generation.
    #[test]
    fn boot_when_both_slots_verify_and_marker_names_newest_then_boots_marker_generation() {
        let dir = TempDir::new().unwrap();
        let store = store_in(&dir);
        store.boot().unwrap();
        store.commit(&artifact(2)).unwrap();
        store.commit(&artifact(3)).unwrap();

        let booted = store.boot().unwrap();

        assert_eq!(container_bytes(&booted), artifact(3));
        assert_eq!(
            marker_of(&dir, "marker"),
            serde_json::json!({"slot": "a", "seq": 3})
        );
    }

    /// Stale marker: the record names an unverifiable slot → boot the
    /// newest verifiable bytes (the other slot), restart the age authority.
    #[test]
    fn boot_when_marker_names_unverifiable_slot_then_adopts_other_slot() {
        let dir = TempDir::new().unwrap();
        let store = store_in(&dir);
        store.boot().unwrap();
        std::fs::write(slot(&dir, "slot-a"), "corrupt").unwrap();
        std::fs::write(slot(&dir, "slot-b"), artifact(2)).unwrap();

        let booted = store.boot().unwrap();

        assert_eq!(container_bytes(&booted), artifact(2));
        assert_eq!(
            marker_of(&dir, "marker"),
            serde_json::json!({"slot": "b", "seq": 1})
        );
    }

    /// Boot with no verifiable slot and a verifying tmp (residue of a
    /// pre-rename crash whose marker residue is gone): adopt the tmp bytes.
    #[test]
    fn boot_when_no_record_verifies_and_tmp_verifies_then_adopts_tmp() {
        let dir = TempDir::new().unwrap();
        let store = store_in(&dir);
        store.boot().unwrap();
        std::fs::write(slot(&dir, "slot-a"), "corrupt").unwrap();
        std::fs::write(slot(&dir, "marker"), "corrupt").unwrap();
        std::fs::write(slot(&dir, "tmp"), artifact(2)).unwrap();

        let booted = store.boot().unwrap();

        assert_eq!(container_bytes(&booted), artifact(2));
    }

    /// A blocked inactive slot (a directory where the slot file belongs)
    /// refuses the delete-before-rename step with V6012; the store is
    /// unchanged.
    #[test]
    fn commit_when_inactive_slot_path_blocked_then_v6012_and_store_unchanged() {
        let dir = TempDir::new().unwrap();
        let store = store_in(&dir);
        store.boot().unwrap();
        std::fs::create_dir(slot(&dir, "slot-b")).unwrap();

        let err = store.commit(&artifact(2)).unwrap_err();

        assert!(err.to_string().starts_with("V6012"));
        assert_eq!(
            marker_of(&dir, "marker"),
            serde_json::json!({"slot": "a", "seq": 1})
        );
    }

    /// Residue but nothing verifies → refuse rather than boot unverified
    /// bytes.
    #[test]
    fn boot_when_residue_present_but_nothing_verifies_then_error() {
        let dir = TempDir::new().unwrap();
        let store = store_in(&dir);
        store.boot().unwrap();
        std::fs::write(slot(&dir, "slot-a"), "corrupt").unwrap();

        let err = store.boot().unwrap_err();

        assert!(err.to_string().contains("no verifiable committed artifact"));
    }

    #[test]
    fn commit_when_marker_missing_then_v6012() {
        let dir = TempDir::new().unwrap();
        let store = store_in(&dir);
        store.boot().unwrap();
        std::fs::remove_file(slot(&dir, "marker")).unwrap();

        let err = store.commit(&artifact(2)).unwrap_err();

        assert!(err.to_string().starts_with("V6012"));
    }

    #[test]
    fn commit_when_tmp_path_blocked_then_v6012_and_store_unchanged() {
        let dir = TempDir::new().unwrap();
        let store = store_in(&dir);
        store.boot().unwrap();
        std::fs::create_dir(slot(&dir, "tmp")).unwrap();

        let err = store.commit(&artifact(2)).unwrap_err();

        assert!(err.to_string().starts_with("V6012"));
        assert!(!slot(&dir, "slot-b").exists());
        assert_eq!(
            marker_of(&dir, "marker"),
            serde_json::json!({"slot": "a", "seq": 1})
        );
    }
}
