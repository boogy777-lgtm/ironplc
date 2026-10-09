//! The data-region layout of a function block instance.
//!
//! An instance owns everything its declaration lists. Most fields take one
//! 8-byte slot, which is where the body finds them (the VM copies the slots in
//! before the body runs and back out after). A field that does not fit a slot
//! -- today a STRING or WSTRING -- keeps its characters in a run of its own,
//! and its slot holds the offset of that run. Both belong to the instance, so
//! both are laid out here, by one routine, as one record:
//!
//! ```text
//! instance start -> | slot 0 | slot 1 | ... | slot n-1 | run | run | ... |
//! ```
//!
//! Two instances of one block therefore never share a character, whichever
//! section declares it. The same layout answers every question about where a
//! field of an instance lives: how many bytes to reserve for the instance
//! ([`FbLayout::instance_bytes`]), which offset the slot of a field is set to
//! at setup ([`emit_instance_storage`]), and how code outside the body reaches
//! the run of a particular instance ([`FbLayout::views`], which is a fixed
//! offset because the instance is).
//!
//! Setup starts every run through the one routine that starts every other
//! variable, `emit_declaration_initial_value`, so a declared value and an empty
//! string are applied the way they are anywhere else.

use std::collections::{BTreeMap, HashMap};

use ironplc_analyzer::TypeEnvironment;
use ironplc_container::{CharWidth, FbTypeDescriptor, FbTypeId, FieldEntry, VarEntry};
use ironplc_dsl::common::{
    FunctionBlockDeclaration, InitialValueAssignmentKind, VarDecl, VariableType,
};
use ironplc_dsl::core::{Id, Located};
use ironplc_dsl::diagnostic::{Diagnostic, Label};

use super::compile::{
    char_width_for_string_type, string_region_size, CompileContext, StringVarInfo, UserFbTypeInfo,
};
use super::compile_edge::hidden_variables;
use super::compile_initial_value::{emit_declaration_initial_value, Start};
use super::compile_stmt::resolve_string_max_length;
use crate::emit::Emitter;
use crate::string_storage::StringPlace;

/// A field of a function block whose characters live outside its slot.
#[derive(Clone)]
pub(crate) struct FieldStorage {
    /// The field's declaration, from which setup starts the run.
    pub(crate) decl: VarDecl,
    /// The field's name, lower case, as instance field lookups spell it.
    pub(crate) key: String,
    /// The ordinal of the slot that holds the offset of the run.
    pub(crate) slot: u8,
    /// Byte offset of the run from the start of the instance.
    pub(crate) offset: u32,
    /// Maximum number of code units the run holds.
    pub(crate) max_length: u16,
    /// Per-code-unit byte width of the run.
    pub(crate) char_width: CharWidth,
}

/// The layout of one instance of a function block type.
#[derive(Clone, Default)]
pub(crate) struct FbLayout {
    /// The fields whose characters live outside their slots, in field order.
    pub(crate) storage: Vec<FieldStorage>,
    /// Bytes an instance occupies: its slots, then its runs.
    pub(crate) instance_bytes: u32,
}

impl FbLayout {
    /// How code outside the body names the run of each field of the instance
    /// that starts at `instance_offset`, by field name: the instance is at a
    /// known place, so each run is at a fixed offset.
    pub(crate) fn views(&self, instance_offset: u32) -> HashMap<String, StringVarInfo> {
        self.storage
            .iter()
            .map(|field| (field.key.clone(), field.view_at(instance_offset)))
            .collect()
    }
}

impl FieldStorage {
    fn view_at(&self, instance_offset: u32) -> StringVarInfo {
        StringVarInfo::fixed(
            instance_offset + self.offset,
            self.max_length,
            self.char_width,
        )
    }
}

/// The fields of an instance of `fb`, in the order they take slots: the inputs,
/// then the outputs, then the rest of the variables it keeps, then the hidden
/// variables its edge inputs need (`compile_edge`). The VM copies the slots in
/// and out of the body in this order.
pub(crate) fn instance_fields(fb: &FunctionBlockDeclaration) -> Vec<VarDecl> {
    let declared =
        |keep: fn(&VarDecl) -> bool| fb.variables.iter().filter(move |d| keep(d)).cloned();
    declared(|d| d.var_type == VariableType::Input)
        .chain(declared(|d| d.var_type == VariableType::Output))
        .chain(declared(|d| d.var_type.is_pou_storage()))
        .chain(hidden_variables(&fb.variables))
        .collect()
}

/// Lays out an instance of a function block whose fields are `fields`, in the
/// order they take slots.
pub(crate) fn layout_instance(fields: &[&VarDecl]) -> Result<FbLayout, Diagnostic> {
    let mut instance_bytes = fields.len() as u32 * ironplc_container::SLOT_BYTES;
    let mut storage = Vec::new();

    for (slot, decl) in fields.iter().enumerate() {
        let (Some(id), InitialValueAssignmentKind::String(string_init)) =
            (decl.identifier.symbolic_id(), &decl.initializer)
        else {
            continue;
        };
        let max_length = resolve_string_max_length(string_init)?;
        let char_width = char_width_for_string_type(&string_init.width);
        let offset = instance_bytes;
        instance_bytes = instance_bytes
            .checked_add(string_region_size(max_length, char_width))
            .ok_or_else(|| {
                Diagnostic::not_supported(Label::span(
                    string_init.span(),
                    "Function block instance exceeds the data region",
                ))
            })?;
        storage.push(FieldStorage {
            decl: (*decl).clone(),
            key: id.to_string().to_lowercase(),
            slot: slot as u8,
            offset,
            max_length,
            char_width,
        });
    }

    Ok(FbLayout {
        storage,
        instance_bytes,
    })
}

/// The function block type table of the container: the field layout of every
/// user function block, in ascending type ID order.
///
/// The fields of a type are the slots its body works in, which the variable
/// table already describes (the slots from `var_offset`, one per field, in
/// the order the VM copies them in and out). The type table states the same
/// fields once, by type, so that the readers of the layout (the layout hash,
/// the load check, the migration planner) read a field layout from the type
/// and never from the position of the body's slots in the variable table.
///
/// A field of an array or structure kind has no entry here, because the type
/// table carries no flag to say so; a field that the variable table records as
/// one is an internal error, not a silently altered layout.
pub(crate) fn type_descriptors(
    user_fb_types: &BTreeMap<String, UserFbTypeInfo>,
    variable_table: &[VarEntry],
) -> Result<Vec<FbTypeDescriptor>, Diagnostic> {
    let mut descriptors = Vec::with_capacity(user_fb_types.len());
    for info in user_fb_types.values() {
        let first = usize::from(info.var_offset);
        let slots = variable_table
            .get(first..first + info.num_fields)
            .ok_or_else(Diagnostic::internal_error)?;
        let fields = slots
            .iter()
            .map(|slot| {
                (slot.flags == 0)
                    .then_some(FieldEntry {
                        field_type: slot.var_type,
                        field_extra: slot.extra,
                    })
                    .ok_or_else(Diagnostic::internal_error)
            })
            .collect::<Result<Vec<_>, _>>()?;
        descriptors.push(FbTypeDescriptor {
            type_id: FbTypeId::new(info.type_id),
            fields,
        });
    }
    Ok(descriptors)
}

/// Emits the setup of the storage of the instance `instance`: each field's
/// slot is pointed at the field's run, and the run is started.
///
/// The run is started by the routine every variable is started by. That
/// routine names a string through the registration of its field's name, which
/// in the body is the instance-relative access; here the instance is known, so
/// the name is registered at the run's fixed offset for the duration of the
/// call.
pub(crate) fn emit_instance_storage(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    instance: &Id,
    types: &TypeEnvironment,
) -> Result<(), Diagnostic> {
    let Some(fb_info) = ctx.fb_instances.get(instance) else {
        return Ok(());
    };
    let (type_id, instance_offset, var_index) =
        (fb_info.type_id, fb_info.data_offset, fb_info.var_index);
    let storage = ctx
        .user_fb_types
        .values()
        .find(|user_fb| user_fb.type_id == type_id)
        .map(|user_fb| user_fb.layout.storage.clone())
        .unwrap_or_default();

    for field in &storage {
        let view = field.view_at(instance_offset);
        let StringPlace::Static { data_offset } = view.place else {
            return Err(Diagnostic::internal_error());
        };

        // The slot holds the offset of the run once the instance exists.
        let offset_const = ctx.add_i32_constant(data_offset as i32);
        emitter.emit_fb_load_instance(var_index);
        emitter.emit_load_const_i32(offset_const);
        emitter.emit_fb_store_param(field.slot);
        emitter.emit_pop();

        let Some(id) = field.decl.identifier.symbolic_id() else {
            continue;
        };
        let shadowed = ctx.string_vars.insert(id.clone(), view);
        let started = emit_declaration_initial_value(emitter, ctx, &field.decl, types, Start::Once);
        match shadowed {
            Some(previous) => ctx.string_vars.insert(id.clone(), previous),
            None => ctx.string_vars.remove(id),
        };
        started?;
    }
    Ok(())
}

#[cfg(test)]
mod tests {
    use ironplc_dsl::common::{StringInitializer, StringType, VarDecl};

    use super::*;

    fn string_field(name: &str, length: u128) -> VarDecl {
        let mut decl = VarDecl::simple(name, "INT");
        decl.initializer = InitialValueAssignmentKind::String(StringInitializer {
            length: Some(ironplc_dsl::common::IntegerRef::Literal(
                ironplc_dsl::common::Integer::new(&length.to_string(), Default::default()).unwrap(),
            )),
            width: StringType::String,
            initial_value: None,
            keyword_span: Default::default(),
        });
        decl
    }

    #[test]
    fn layout_instance_when_only_scalars_then_slots_only() {
        let a = VarDecl::simple("a", "INT");
        let b = VarDecl::simple("b", "INT");

        let layout = layout_instance(&[&a, &b]).unwrap();

        assert!(layout.storage.is_empty());
        assert_eq!(16, layout.instance_bytes);
    }

    #[test]
    fn layout_instance_when_strings_then_runs_follow_slots_in_field_order() {
        let a = VarDecl::simple("a", "INT");
        let s = string_field("s", 10);
        let t = string_field("t", 3);

        let layout = layout_instance(&[&a, &s, &t]).unwrap();

        assert_eq!(2, layout.storage.len());
        assert_eq!((1, 24), (layout.storage[0].slot, layout.storage[0].offset));
        // Runs follow the three slots, in field order, each as large as its string.
        let first_run = string_region_size(10, CharWidth::Narrow);
        let second_run = string_region_size(3, CharWidth::Narrow);
        assert_eq!(
            (2, 24 + first_run),
            (layout.storage[1].slot, layout.storage[1].offset)
        );
        assert_eq!(24 + first_run + second_run, layout.instance_bytes);
    }

    #[test]
    fn views_when_field_has_run_then_offset_is_instance_plus_run() {
        let s = string_field("s", 10);
        let layout = layout_instance(&[&s]).unwrap();

        let views = layout.views(100);

        assert_eq!(Some(108), views["s"].static_offset());
        assert!(!views.contains_key("other"));
    }
}
