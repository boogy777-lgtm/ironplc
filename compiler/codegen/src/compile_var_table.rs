//! Variable-table collection for the container's type section.
//!
//! Every variable index the compiler assigns gets exactly one [`VarEntry`]
//! here, recorded by the site that assigns it. The table is the primary
//! input to the layout hash (`specs/design/bytecode-container-format.md`,
//! "Layout Hash and Online Change"), so a missing entry would make the
//! from-source hash non-deterministic. [`CompileContext::collect_variable_table`]
//! therefore fails the compilation rather than emitting a gap.
//!
//! The collection is deliberately *not* one of the per-scope maps
//! `CompileContext` saves and restores: an index, once assigned, belongs to
//! the container regardless of which POU owned the name at the time.
//!
//! Separated from `compile.rs` to keep module sizes within the 1000-line
//! guideline.

use ironplc_container::{
    CharWidth, FbFieldUidEntry, FieldType, StableVarEntry, VarEntry, VarIndex, VAR_FLAG_IS_ARRAY,
};
use ironplc_dsl::common::{
    ElementaryTypeName, FunctionReturnType, InitialValueAssignmentKind, SpecificationKind,
    TypeName, VarDecl,
};
use ironplc_dsl::core::{FileId, Id};
use ironplc_dsl::diagnostic::{Diagnostic, Label};

use super::compile::{CompileContext, OpWidth, Signedness, VarTypeInfo};
use super::compile_array::{var_type_info_to_type_byte, ArrayVarInfo};
use super::type_info::resolve_type_name;

/// The entry for a slot whose type codegen cannot name: compiler scratch, or
/// a declaration whose type this backend does not model. The layout hash
/// still distinguishes it by position and count.
pub(crate) fn slot_entry() -> VarEntry {
    VarEntry {
        var_type: FieldType::Slot,
        flags: 0,
        extra: 0,
    }
}

/// Records the entry for a declaration that has just been registered at
/// `index`. Must be called after the declaration's registration side effects
/// (`string_vars`, `array_vars`, `fb_instances`, `struct_vars`, ...) because
/// the classification reads them, so it cannot drift from how the variable is
/// laid out.
pub(crate) fn record_decl_var_entry(
    ctx: &mut CompileContext,
    decl: &VarDecl,
    id: &Id,
    index: VarIndex,
) {
    let entry = registered_entry(ctx, id).unwrap_or_else(|| scalar_entry(ctx, decl, id));
    ctx.record_var_entry(index, entry);
}

/// Records the stable variable ID for a persistent declaration that has just
/// been assigned `index`, when the engineering-side table names it (ADR 0053).
///
/// Only the program/global allocation path (`compile_setup::assign_variables`)
/// calls this, so transient slots -- function locals, method parameters, FB
/// field regions, compiler scratch -- get no entry even when a name matches.
pub(crate) fn record_stable_var_entry(
    ctx: &mut CompileContext,
    stable_var_ids: &[(Id, u64)],
    id: &Id,
    index: VarIndex,
) {
    let uid = stable_var_ids
        .iter()
        .find(|(name, _)| name == id)
        .map(|(_, uid)| *uid);
    if let Some(uid) = uid {
        ctx.stable_var_entries.push(StableVarEntry {
            var_index: index,
            uid,
        });
    }
}

/// Records the entry for a function- or method-return slot. The slot has no
/// `VarDecl`; `return_type` is `None` for a method that returns nothing (the
/// slot exists but is never read).
pub(crate) fn record_return_var_entry(
    ctx: &mut CompileContext,
    return_type: Option<&FunctionReturnType>,
    id: &Id,
    index: VarIndex,
) {
    let entry = registered_entry(ctx, id).unwrap_or_else(|| match return_type {
        Some(FunctionReturnType::Named(type_name)) => var_entry_for_type_name(&type_name.name),
        _ => slot_entry(),
    });
    ctx.record_var_entry(index, entry);
}

/// The entry implied by a variable's registration, if it has one. Reads only
/// maps the allocation sites already populate, so it cannot disagree with the
/// layout those sites chose.
fn registered_entry(ctx: &CompileContext, id: &Id) -> Option<VarEntry> {
    if let Some(info) = ctx.string_vars.get(id) {
        return Some(VarEntry {
            var_type: string_field_type(info.char_width),
            flags: 0,
            extra: info.max_length,
        });
    }
    if let Some(info) = ctx.fb_instances.get(id) {
        return Some(VarEntry {
            var_type: FieldType::FbInstance,
            flags: 0,
            extra: info.type_id,
        });
    }
    if let Some(info) = ctx.array_vars.get(id) {
        // A REF_TO ARRAY slot holds the target's variable index, not a
        // data-region offset, so it is not an array to a verifier even though
        // its access path uses a descriptor.
        if info.is_ref {
            return Some(VarEntry {
                var_type: FieldType::U64,
                flags: 0,
                extra: 0,
            });
        }
        return Some(VarEntry {
            var_type: array_element_type(info),
            flags: VAR_FLAG_IS_ARRAY,
            extra: info.desc_index,
        });
    }
    if let Some(info) = ctx.struct_array_vars.get(id) {
        return Some(VarEntry {
            var_type: FieldType::Slot,
            flags: VAR_FLAG_IS_ARRAY,
            extra: info.desc_index,
        });
    }
    ctx.struct_vars.contains_key(id).then(slot_entry)
}

/// Element type encoding of an array variable, matching the descriptor
/// `register_array_variable` emitted for it: STRING/WSTRING for string
/// elements, the codegen type projection otherwise.
fn array_element_type(info: &ArrayVarInfo) -> FieldType {
    if info.is_string_element {
        return string_field_type(info.string_char_width);
    }
    FieldType::from_u8(var_type_info_to_type_byte(&info.element_var_type_info))
        .unwrap_or(FieldType::Slot)
}

/// Maps a string's per-code-unit width to its `VarEntry` type tag.
fn string_field_type(char_width: CharWidth) -> FieldType {
    if char_width.is_wide() {
        FieldType::WString
    } else {
        FieldType::String
    }
}

/// Classifies a declaration the registration maps did not claim. An
/// elementary type name is matched first because `TIME` shares its 32-bit
/// signed operation width with `DINT` but has its own encoding; everything
/// else projects through the same table codegen uses for expressions.
fn scalar_entry(ctx: &CompileContext, decl: &VarDecl, id: &Id) -> VarEntry {
    if let Some(type_name) = declared_type_name(decl) {
        return var_entry_for_type_name(&type_name);
    }
    match ctx.var_types.get(id) {
        Some(info) => var_entry_for_op_type(info),
        None => slot_entry(),
    }
}

/// The declared type name of a scalar declaration, when the AST states one.
/// Subrange declarations that name an existing type are classified from
/// `var_types` instead, which carries the subrange's base type.
fn declared_type_name(decl: &VarDecl) -> Option<Id> {
    match &decl.initializer {
        InitialValueAssignmentKind::Simple(simple) => Some(simple.type_name.name.clone()),
        InitialValueAssignmentKind::Subrange(SpecificationKind::Inline(inline)) => {
            let base: TypeName = inline.type_name.clone().into();
            Some(base.name)
        }
        _ => None,
    }
}

/// Maps a declared type name to its `VarEntry`, or [`slot_entry`] when the
/// name does not project onto an encoded type.
fn var_entry_for_type_name(name: &Id) -> VarEntry {
    if let Ok(ElementaryTypeName::TIME) = ElementaryTypeName::try_from(name) {
        return VarEntry {
            var_type: FieldType::Time,
            flags: 0,
            extra: 0,
        };
    }
    resolve_type_name(name).map_or_else(slot_entry, |info| var_entry_for_op_type(&info))
}

/// Projects codegen's operation type onto the container's type encoding,
/// matching `var_type_info_to_type_byte` for the array case.
fn var_entry_for_op_type(info: &VarTypeInfo) -> VarEntry {
    let var_type = match (info.op_width, info.signedness) {
        (OpWidth::W32, Signedness::Signed) => FieldType::I32,
        (OpWidth::W32, Signedness::Unsigned) => FieldType::U32,
        (OpWidth::W64, Signedness::Signed) => FieldType::I64,
        (OpWidth::W64, Signedness::Unsigned) => FieldType::U64,
        (OpWidth::F32, _) => FieldType::F32,
        (OpWidth::F64, _) => FieldType::F64,
    };
    VarEntry {
        var_type,
        flags: 0,
        extra: 0,
    }
}

impl CompileContext {
    /// Records the variable-table entry for `index`. First writer wins: an
    /// index belongs to exactly one variable, so a second record for the same
    /// slot could only come from compiler scratch aliasing a declared slot
    /// (see `allocate_scratch_variable`) and must not overwrite the declared
    /// variable's entry.
    pub(crate) fn record_var_entry(&mut self, index: VarIndex, entry: VarEntry) {
        let slot = usize::from(index.raw());
        if self.var_entries.len() <= slot {
            self.var_entries.resize(slot + 1, None);
        }
        if self.var_entries[slot].is_none() {
            self.var_entries[slot] = Some(entry);
        }
    }

    /// Returns the variable table for indices `0..num_variables`, or an
    /// internal error naming the first index no allocation site claimed.
    /// A gap is a codegen bug, not something to paper over with a guess: the
    /// layout hash covers every entry, so a guessed entry would silently make
    /// the from-source hash wrong.
    pub(crate) fn collect_variable_table(
        &self,
        num_variables: u16,
    ) -> Result<Vec<VarEntry>, Diagnostic> {
        let mut table = Vec::with_capacity(usize::from(num_variables));
        for index in 0..num_variables {
            match self.var_entries.get(usize::from(index)) {
                Some(Some(entry)) => table.push(entry.clone()),
                _ => {
                    return Err(Diagnostic::internal_error_at(Label::file(
                        FileId::default(),
                        format!("No variable-table entry was recorded for variable index {index}"),
                    )))
                }
            }
        }
        Ok(table)
    }

    /// Returns the stable variable ID table (ADR 0053), ascending by
    /// `var_index`. Entries are recorded in allocation order, which is
    /// already ascending for the program/global prefix; the sort makes the
    /// writer's ordering contract independent of that. Two entries claiming
    /// the same index are an internal error, never a silent overwrite: the
    /// migration planner keys on the index, so a duplicate would make the
    /// table ambiguous.
    pub(crate) fn collect_stable_vars(&self) -> Result<Vec<StableVarEntry>, Diagnostic> {
        let mut table = self.stable_var_entries.clone();
        table.sort_by_key(|entry| entry.var_index.raw());
        if let Some(duplicate) = table
            .windows(2)
            .find(|pair| pair[0].var_index == pair[1].var_index)
        {
            return Err(Diagnostic::internal_error_at(Label::file(
                FileId::default(),
                format!(
                    "Variable index {} has more than one stable variable ID",
                    duplicate[0].var_index
                ),
            )));
        }
        Ok(table)
    }

    /// Returns the FB field UID table (ADR 0059), ascending by
    /// `(fb_type_id, field_index)`. Entries are recorded in FB pre-scan
    /// order, which is already ascending (type IDs are handed out in
    /// declaration order and field indices within a type ascend); the sort
    /// makes the writer's ordering contract independent of that. Two entries
    /// claiming the same `(fb_type_id, field_index)` are an internal error,
    /// never a silent overwrite: the migration planner keys on the pair, so
    /// a duplicate would make the table ambiguous.
    pub(crate) fn collect_fb_field_uids(&self) -> Result<Vec<FbFieldUidEntry>, Diagnostic> {
        let mut table = self.fb_field_uid_entries.clone();
        table.sort_by_key(|entry| (entry.fb_type_id.raw(), entry.field_index));
        if let Some(duplicate) = table.windows(2).find(|pair| {
            pair[0].fb_type_id == pair[1].fb_type_id && pair[0].field_index == pair[1].field_index
        }) {
            return Err(Diagnostic::internal_error_at(Label::file(
                FileId::default(),
                format!(
                    "FB type {} field {} has more than one field UID",
                    duplicate[0].fb_type_id, duplicate[0].field_index
                ),
            )));
        }
        Ok(table)
    }
}
