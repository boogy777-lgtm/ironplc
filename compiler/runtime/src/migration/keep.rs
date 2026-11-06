//! The keep rule of a replace (a Download, ADR-0088): which values of the
//! running application survive into an application that replaces it.
//!
//! A replace builds the candidate's state fresh and runs its init image. The
//! values it then writes over that state are those of the `PERSISTENT`
//! variables whose declaration is equal; everything else, a `RETAIN` variable
//! and an ordinary one included, keeps its initial value (CODESYS behaviour).
//! "Equal" is the same stable variable ID (ADR-0053) in the running and in the
//! candidate container, the same `PERSISTENT` kind on both sides, and equal type
//! data.
//!
//! [`replace_keep_plan`] decides it as a function of the two containers, and
//! its answer is a [`StateMigrationPlan`] of the kept copies, so the copy is the
//! one the migration swap performs ([`StateMigrationPlan::apply`]) and the type
//! data is sized by the same code (`copy_action`). The other half of the answer
//! is the list of `PERSISTENT` variables of the running application that were
//! not kept, each with its reason: a reset is never silent.
//!
//! | Variable of the running application | Outcome |
//! |---|---|
//! | `PERSISTENT`, same ID and kind and equal type data in the candidate | kept |
//! | `PERSISTENT`, no stable ID | reinitialised, [`ReinitReason::NoStableId`] |
//! | `PERSISTENT`, the candidate carries no stable IDs at all | reinitialised, [`ReinitReason::NoStableId`] |
//! | `PERSISTENT`, its ID is not in the candidate | reinitialised, [`ReinitReason::NotInCandidate`] |
//! | `PERSISTENT`, the candidate declares it with another qualifier | reinitialised, [`ReinitReason::QualifierChanged`] |
//! | `PERSISTENT`, type, size or layout differ | reinitialised, [`ReinitReason::TypeChanged`] |
//! | `PERSISTENT`, a type whose copy the runtime cannot yet size | reinitialised, [`ReinitReason::UnsupportedType`] |
//! | `RETAIN`, or no qualifier | not kept, not listed (it was never promised across a download) |
//! | only in the candidate | its initial value |
//!
//! The `PERSISTENT` fields of a function block instance that is not itself
//! declared `PERSISTENT` have a retained run but no variable of their own, so
//! they are not offered to this rule and are reinitialised without a line in the
//! list. A `PERSISTENT` instance is kept whole.

use std::collections::{HashMap, HashSet};
use std::string::String;
use std::vec::Vec;

use ironplc_container::{
    Container, FbTypeId, FieldType, RetainKind, RetainSpace, TypeSection, VarEntry, VarIndex,
    VAR_FLAG_IS_ARRAY,
};
use ironplc_vm::VmBuffers;
use serde::{Deserialize, Serialize};

use super::decision::debug_name;
use super::fb::{fb_fields, has_string_field, same_instance_layout, user_fb_descriptor};
use super::{
    copy_action, index_by_uid, stable_vars, variable_at, variable_table, MigrationAction,
    MigrationError, StateMigrationPlan,
};

/// Why a `PERSISTENT` variable of the running application starts again at its
/// initial value after a replace. On the wire it is the camelCase name.
#[derive(Clone, Copy, Debug, PartialEq, Eq, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub enum ReinitReason {
    /// The variable has no stable ID in the running application, or the
    /// candidate carries no stable IDs at all, so the two cannot be matched.
    NoStableId,
    /// The candidate has no variable with this ID.
    NotInCandidate,
    /// The candidate declares the variable without `PERSISTENT`.
    QualifierChanged,
    /// The type, the size or the layout of the variable differs.
    TypeChanged,
    /// The runtime cannot yet carry a value of this type across a replace (a
    /// standard-library function block instance, or an instance with a
    /// string field).
    UnsupportedType,
}

/// A `PERSISTENT` variable of the running application that a replace
/// reinitialises: the line of the reply that makes the reset visible.
#[derive(Clone, Debug, PartialEq, Eq, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct Reinitialised {
    /// The stable ID, when the variable has one.
    #[serde(default, skip_serializing_if = "Option::is_none")]
    pub uid: Option<u64>,
    /// The name of the variable, when a debug section names it.
    #[serde(default, skip_serializing_if = "Option::is_none")]
    pub name: Option<String>,
    /// Why it is not kept.
    pub reason: ReinitReason,
}

/// What a replace keeps: the copies of the equal `PERSISTENT` variables and the
/// list of those that are reinitialised.
#[derive(Debug)]
pub struct KeepPlan {
    plan: StateMigrationPlan,
    kept: Vec<u64>,
    reinitialised: Vec<Reinitialised>,
}

impl KeepPlan {
    /// The stable IDs of the variables that are kept.
    pub fn kept(&self) -> &[u64] {
        &self.kept
    }

    /// The `PERSISTENT` variables of the running application that are
    /// reinitialised, with the reason for each.
    pub fn reinitialised(&self) -> &[Reinitialised] {
        &self.reinitialised
    }

    /// Copies the kept values from the running application's buffers into the
    /// candidate's freshly initialized ones, exactly as the migration swap does.
    pub fn apply(&self, running: &VmBuffers, fresh: &mut VmBuffers) -> Result<(), MigrationError> {
        self.plan.apply(running, fresh)
    }
}

/// The two type sections a keep decision compares: the running application's
/// and the candidate's.
type Sections<'a> = (Option<&'a TypeSection>, Option<&'a TypeSection>);

/// The keep rule of a replace: see the module documentation.
///
/// Fails only on a corrupt container (a stable ID bound twice, an index outside
/// the variable table); a variable that cannot be kept is never an error, it is
/// a line of [`KeepPlan::reinitialised`].
pub fn replace_keep_plan(
    running: &Container,
    candidate: &Container,
) -> Result<KeepPlan, MigrationError> {
    let sections = (
        running.type_section.as_ref(),
        candidate.type_section.as_ref(),
    );
    let running_stable = stable_vars(sections.0);
    let candidate_stable = stable_vars(sections.1);
    let uid_at: HashMap<VarIndex, u64> = running_stable
        .iter()
        .map(|entry| (entry.var_index, entry.uid))
        .collect();
    index_by_uid(running_stable)?;
    let candidate_by_uid = index_by_uid(candidate_stable)?;
    let candidate_persistent: HashSet<VarIndex> = persistent_slots(candidate).into_iter().collect();

    let mut actions = Vec::new();
    let mut kept = Vec::new();
    let mut reinitialised = Vec::new();
    for from_index in persistent_slots(running) {
        let uid = uid_at.get(&from_index).copied();
        let to_index = uid.and_then(|uid| candidate_by_uid.get(&uid).copied());
        let name = || {
            debug_name(running, from_index)
                .or_else(|| to_index.and_then(|to_index| debug_name(candidate, to_index)))
        };
        let outcome = match (uid, to_index) {
            (None, _) => Err(ReinitReason::NoStableId),
            (Some(_), None) if candidate_stable.is_empty() => Err(ReinitReason::NoStableId),
            (Some(_), None) => Err(ReinitReason::NotInCandidate),
            (Some(_), Some(to_index)) if !candidate_persistent.contains(&to_index) => {
                Err(ReinitReason::QualifierChanged)
            }
            (Some(uid), Some(to_index)) => keep_action(uid, from_index, to_index, sections)?,
        };
        match outcome {
            Ok(action) => {
                actions.push(action);
                kept.extend(uid);
            }
            Err(reason) => reinitialised.push(Reinitialised {
                uid,
                name: name(),
                reason,
            }),
        }
    }
    Ok(KeepPlan {
        plan: StateMigrationPlan { actions },
        kept,
        reinitialised,
    })
}

/// The slots of the variable table that hold a `PERSISTENT` variable, in
/// ascending order: the persistent slot runs of the retained declaration
/// (ADR-0085), the one place that says what is retained.
fn persistent_slots(container: &Container) -> Vec<VarIndex> {
    let slots: HashSet<u16> = container
        .retained_runs()
        .iter()
        .filter(|run| run.kind == RetainKind::Persistent && run.space == RetainSpace::Slots)
        .flat_map(|run| run.range())
        .filter_map(|slot| u16::try_from(slot).ok())
        .collect();
    let mut slots: Vec<u16> = slots.into_iter().collect();
    slots.sort_unstable();
    slots.into_iter().map(VarIndex::new).collect()
}

/// The copy that keeps one shared `PERSISTENT` variable, or the reason it
/// cannot be kept. The outer error is a corrupt container.
fn keep_action(
    uid: u64,
    from_index: VarIndex,
    to_index: VarIndex,
    sections: Sections<'_>,
) -> Result<Result<MigrationAction, ReinitReason>, MigrationError> {
    let (running_section, candidate_section) = sections;
    let running_var = variable_at(variable_table(running_section), from_index)?;
    let candidate_var = variable_at(variable_table(candidate_section), to_index)?;
    if !same_type_data(running_var, candidate_var) {
        return Ok(Err(ReinitReason::TypeChanged));
    }
    let is_instance = candidate_var.var_type == FieldType::FbInstance
        && candidate_var.flags & VAR_FLAG_IS_ARRAY == 0;
    if is_instance {
        return Ok(instance_action(
            from_index,
            to_index,
            FbTypeId::new(candidate_var.extra),
            sections,
        ));
    }
    Ok(
        match copy_action(
            uid,
            from_index,
            to_index,
            running_var,
            candidate_var,
            running_section,
            candidate_section,
        ) {
            Ok(action) => Ok(action),
            Err(MigrationError::ArrayDescriptorMismatch { .. }) => Err(ReinitReason::TypeChanged),
            Err(_) => Err(ReinitReason::UnsupportedType),
        },
    )
}

/// Whether two variable table entries describe the same type. An array's
/// `extra` is the index of its descriptor, which two builds number alike only by
/// chance, so the descriptors are compared by `copy_action` and not here.
fn same_type_data(running: &VarEntry, candidate: &VarEntry) -> bool {
    running.var_type == candidate.var_type
        && running.flags == candidate.flags
        && (running.flags & VAR_FLAG_IS_ARRAY != 0 || running.extra == candidate.extra)
}

/// The copy of a user-defined function block instance: the whole field region,
/// when the block's fields and their UIDs are the same on both sides.
fn instance_action(
    from_index: VarIndex,
    to_index: VarIndex,
    type_id: FbTypeId,
    sections: Sections<'_>,
) -> Result<MigrationAction, ReinitReason> {
    let (running_section, candidate_section) = sections;
    let (Some(_), Some(_)) = (
        user_fb_descriptor(running_section, type_id),
        user_fb_descriptor(candidate_section, type_id),
    ) else {
        // A standard-library block: its region is sized by the VM, not by the
        // type section.
        return Err(ReinitReason::UnsupportedType);
    };
    let (Some(fields), Some(_)) = (
        fb_fields(candidate_section, type_id),
        fb_fields(running_section, type_id),
    ) else {
        return Err(ReinitReason::UnsupportedType);
    };
    if !same_instance_layout(running_section, candidate_section, type_id) {
        return Err(ReinitReason::TypeChanged);
    }
    // A string field's slot holds the offset of its characters inside the
    // instance, which a byte copy of the region would carry as it is.
    if has_string_field(fields) {
        return Err(ReinitReason::UnsupportedType);
    }
    let byte_size = u32::try_from(fields.len())
        .ok()
        .and_then(|count| count.checked_mul(ironplc_container::SLOT_BYTES))
        .ok_or(ReinitReason::UnsupportedType)?;
    Ok(MigrationAction::FbInstance {
        from_index,
        to_index,
        byte_size,
    })
}
