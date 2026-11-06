//! The retained declaration of a program: which of its persistent variables
//! hold a value that survives a restart (ADR-0085).
//!
//! A variable declared `RETAIN` or `PERSISTENT` in the persistent part (a
//! global, a variable of the program, or a field of a function block instance)
//! is retained. Code generation records the runs of the variable table and of
//! the data region that hold them at the one point where it has assigned the
//! storage of the declaration ([`record_declaration`], called by
//! [`crate::compile_setup::assign_persistent_variables`]), and the container
//! carries them as its retained runs.
//!
//! | Declaration | Retained |
//! |---|---|
//! | `RETAIN` or `PERSISTENT` variable of any type | its slot and every byte of its data region storage |
//! | an instance of a function block (or an array of them) without a qualifier | the fields its type declares `RETAIN` or `PERSISTENT` (their slots and, for a string, its characters), in each instance |
//! | `NON_RETAIN` or no qualifier on anything else | nothing |
//! | a `RETAIN` local of a function or a method, or a `VAR_TEMP` | nothing: working memory is not state and is never offered to this module |
//!
//! A function block instance declared `NON_RETAIN` retains nothing, whatever
//! its type declares: the qualifier on the declaration decides first.

use std::ops::Range;

use ironplc_container::{RetainKind, RetainedRun};
use ironplc_dsl::common::{DeclarationQualifier, VarDecl};

use crate::compile::CompileContext;

/// The kind of retention a qualifier declares, if any.
///
/// The `match` has no wildcard arm: a new qualifier must decide here whether
/// it retains.
pub(crate) fn retain_kind(qualifier: &DeclarationQualifier) -> Option<RetainKind> {
    match qualifier {
        DeclarationQualifier::Retain => Some(RetainKind::Retain),
        DeclarationQualifier::Persistent => Some(RetainKind::Persistent),
        DeclarationQualifier::Unspecified
        | DeclarationQualifier::Constant
        | DeclarationQualifier::NonRetain => None,
    }
}

/// Records the retained runs of the declaration `decl`, whose storage has just
/// been assigned: its slot is the one `ctx.variables` maps its name to, and
/// `data` is the run of the data region reserved for it.
pub(crate) fn record_declaration(ctx: &mut CompileContext, decl: &VarDecl, data: Range<u32>) {
    let Some(id) = decl.identifier.symbolic_id() else {
        return;
    };
    let Some(slot) = ctx.variables.get(id).copied() else {
        return;
    };
    match (retain_kind(&decl.qualifier), &decl.qualifier) {
        (Some(kind), _) => {
            ctx.retained_runs
                .push(RetainedRun::slots(kind, u32::from(slot.raw()), 1));
            if data.end > data.start {
                ctx.retained_runs
                    .push(RetainedRun::bytes(kind, data.start, data.end - data.start));
            }
        }
        (None, DeclarationQualifier::NonRetain) => {}
        (None, _) => record_instance_fields(ctx, id, data),
    }
}

/// Records the retained fields of the instances that the variable `id` holds,
/// when it holds instances of a function block whose type declares retained
/// fields. `data` is the run of the data region reserved for the variable: one
/// instance, or the instances of an array, laid out one after the other.
fn record_instance_fields(ctx: &mut CompileContext, id: &ironplc_dsl::core::Id, data: Range<u32>) {
    let Some(instance) = ctx.fb_instances.get(id) else {
        return;
    };
    let type_id = instance.type_id;
    let Some(layout) = ctx
        .user_fb_types
        .values()
        .find(|user_fb| user_fb.type_id == type_id)
        .map(|user_fb| &user_fb.layout)
    else {
        return;
    };
    if layout.retained.is_empty() || layout.instance_bytes == 0 {
        return;
    }
    let stride = layout.instance_bytes;
    let instances = (data.end - data.start) / stride;
    let runs: Vec<RetainedRun> = (0..instances)
        .flat_map(|index| {
            let base = data.start + index * stride;
            layout.retained.iter().map(move |(kind, bytes)| {
                RetainedRun::bytes(*kind, base + bytes.start, bytes.end - bytes.start)
            })
        })
        .collect();
    ctx.retained_runs.extend(runs);
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn retain_kind_when_retain_then_retain() {
        assert_eq!(
            retain_kind(&DeclarationQualifier::Retain),
            Some(RetainKind::Retain)
        );
    }

    #[test]
    fn retain_kind_when_persistent_then_persistent() {
        assert_eq!(
            retain_kind(&DeclarationQualifier::Persistent),
            Some(RetainKind::Persistent)
        );
    }

    #[test]
    fn retain_kind_when_non_retain_constant_or_unspecified_then_none() {
        assert_eq!(retain_kind(&DeclarationQualifier::NonRetain), None);
        assert_eq!(retain_kind(&DeclarationQualifier::Constant), None);
        assert_eq!(retain_kind(&DeclarationQualifier::Unspecified), None);
    }
}
