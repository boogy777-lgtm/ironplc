//! Variable setup and initialization for IEC 61131-3 code generation.
//!
//! Contains variable assignment, initial value emission, function local
//! prologue, and type name resolution. Separated from compile.rs to
//! keep module sizes within the 1000-line guideline.

use ironplc_container::debug_section::{
    function_id, iec_type_tag, var_section, StringLayoutEntry, VarNameEntry,
};
use ironplc_container::{ContainerBuilder, VarIndex};
use ironplc_dsl::common::{
    FunctionReturnType, InitialValueAssignmentKind, SpecificationKind, TypeName, VarDecl,
    VariableType,
};
use ironplc_dsl::core::{Id, Located};
use ironplc_dsl::diagnostic::{Diagnostic, Label};

use ironplc_analyzer::TypeEnvironment;

use super::compile::{char_width_for_string_type, CompileContext, FbInstanceInfo, OpType, OpWidth};
use super::compile_call::resolve_fb_type;
use super::compile_fb_layout::FbLayout;
use super::compile_var_table::{record_decl_var_entry, record_stable_var_entry};
use crate::emit::Emitter;
use crate::string_storage::{register_string_variable, StringHome};

/// Assigns variable table indices and type info for all variable declarations.
///
/// `stable_var_ids` is the engineering-side name -> entity UID table
/// (ADR 0053). A declaration whose name it lists records a stable variable ID
/// entry at its assigned index; transient slots assigned by other paths
/// (`compile_fn`, `compile_method`, scratch) are never looked up here.
pub(crate) fn assign_variables(
    ctx: &mut CompileContext,
    builder: &mut ContainerBuilder,
    declarations: &[VarDecl],
    types: &TypeEnvironment,
    stable_var_ids: &[(Id, u64)],
) -> Result<(), Diagnostic> {
    for decl in declarations {
        if let Some(id) = decl.identifier.symbolic_id() {
            let index = VarIndex::new(ctx.variables.len() as u16);
            ctx.variables.insert(id.clone(), index);

            // Resolve type info and collect debug metadata.
            let (type_tag, type_name_str) = match &decl.initializer {
                InitialValueAssignmentKind::Simple(simple) => {
                    // The global_var_decl parser produces Simple for all named
                    // types, including structs.  Detect struct types via the
                    // type environment and register them properly so that field
                    // access works in codegen.
                    if types.resolve_struct_type(&simple.type_name).is_some() {
                        crate::compile_struct::allocate_struct_variable(
                            ctx,
                            builder,
                            types,
                            &simple.type_name,
                            id,
                            index,
                            &decl.identifier.span(),
                        )?;
                        let type_name_str = simple.type_name.to_string().to_uppercase();
                        (iec_type_tag::STRUCT, type_name_str)
                    } else if let Some(subrange_type) =
                        types.resolve_subrange_type(&simple.type_name)
                    {
                        // Named subrange type with explicit init (e.g., x : MY_RANGE := 75)
                        if let Some(type_info) =
                            crate::compile_struct::var_type_info_for_field(subrange_type)
                        {
                            ctx.var_types.insert(id.clone(), type_info);
                        }
                        let name = simple.type_name.to_string().to_uppercase();
                        (iec_type_tag::OTHER, name)
                    } else {
                        if let Some(type_info) = crate::type_info::decl_type_info(ctx, decl) {
                            ctx.var_types.insert(id.clone(), type_info);
                        }
                        let tag = resolve_iec_type_tag(types, &simple.type_name);
                        let name = simple.type_name.name.to_string().to_uppercase();
                        (tag, name)
                    }
                }
                InitialValueAssignmentKind::String(string_init) => {
                    // Space in the data region: [max_length: u16][cur_length: u16][data]
                    let info = register_string_variable(
                        ctx,
                        builder,
                        id,
                        string_init,
                        StringHome::Static,
                    )?;
                    if let Some(data_offset) = info.static_offset() {
                        ctx.debug_string_layouts.push(StringLayoutEntry {
                            var_index: index,
                            data_offset,
                            max_length: info.max_length,
                        });
                    }
                    if info.char_width.is_wide() {
                        (iec_type_tag::WSTRING, "WSTRING".into())
                    } else {
                        (iec_type_tag::STRING, "STRING".into())
                    }
                }
                InitialValueAssignmentKind::FunctionBlock(fb_init) => {
                    // A member initializer (`(PT := T#100MS)`) is applied by
                    // `emit_initial_values`, which runs after the instance
                    // has its slot offset -- each member store addresses the
                    // instance through it. Nothing to do here but size it.
                    let fb_name = fb_init.type_name.to_string().to_uppercase();
                    // A standard library block is its slots; a user-defined one
                    // is its slots and the runs of the fields that do not fit
                    // one (`compile_fb_layout`). Either way the instance is
                    // reserved whole, so it owns all of its storage.
                    let instance = match resolve_fb_type(&fb_name) {
                        Some((type_id, num_fields, field_map)) => Some((
                            type_id,
                            num_fields as u32 * ironplc_container::SLOT_BYTES,
                            field_map,
                            FbLayout::default(),
                        )),
                        None => ctx.user_fb_types.get(&fb_name).map(|user_fb| {
                            (
                                user_fb.type_id,
                                user_fb.layout.instance_bytes,
                                user_fb.field_indices.clone(),
                                user_fb.layout.clone(),
                            )
                        }),
                    };
                    if let Some((type_id, instance_bytes, field_indices, layout)) = instance {
                        let data_offset = crate::data_region::reserve(
                            ctx,
                            instance_bytes,
                            &decl.identifier.span(),
                        )?;

                        ctx.fb_instances.insert(
                            id.clone(),
                            FbInstanceInfo {
                                var_index: index,
                                type_id,
                                data_offset,
                                field_indices,
                                strings: layout.views(data_offset),
                            },
                        );
                    }
                    (iec_type_tag::FB_INSTANCE, fb_name)
                }
                InitialValueAssignmentKind::Array(array_init) => {
                    // An array whose elements are structures is laid out as
                    // one flat run of slots rather than one slot per element,
                    // so it registers through its own path.
                    if let Some((element_type, debug_type_name, dimensions)) =
                        crate::compile_array_struct::struct_array_declaration(
                            types,
                            &array_init.spec,
                            &decl.identifier.span(),
                        )?
                    {
                        crate::compile_array_struct::register_struct_array_variable(
                            ctx,
                            builder,
                            id,
                            index,
                            &element_type,
                            &debug_type_name,
                            &dimensions,
                            &decl.identifier.span(),
                        )?
                    } else {
                        let spec = crate::compile_array::array_spec_for_declaration(
                            types,
                            &array_init.spec,
                            &decl.identifier.span(),
                        )?;
                        crate::compile_array::register_array_variable(
                            ctx,
                            builder,
                            id,
                            index,
                            &spec,
                            &decl.identifier.span(),
                        )?
                    }
                }
                InitialValueAssignmentKind::Params(params) => {
                    // A PARAMS list is laid out as the array it lowers to:
                    // `ARRAY[0 .. n-1] OF T` (see the analyzer's
                    // `intermediates::params`).
                    let spec = crate::compile_array::array_spec_from_params(
                        params,
                        &decl.identifier.span(),
                    )?;
                    crate::compile_array::register_array_variable(
                        ctx,
                        builder,
                        id,
                        index,
                        &spec,
                        &decl.identifier.span(),
                    )?;
                    (iec_type_tag::ARRAY, "PARAMS".into())
                }
                InitialValueAssignmentKind::Reference(ref_init) => {
                    crate::compile_reference::register_reference_variable(
                        ctx, builder, types, id, index, ref_init,
                    )?;
                    (iec_type_tag::OTHER, "REF_TO".into())
                }
                InitialValueAssignmentKind::Structure(struct_init) => {
                    crate::compile_struct::allocate_struct_variable(
                        ctx,
                        builder,
                        types,
                        &struct_init.type_name,
                        id,
                        index,
                        &decl.identifier.span(),
                    )?;
                    let type_name_str = struct_init.type_name.to_string().to_uppercase();
                    (iec_type_tag::STRUCT, type_name_str)
                }
                InitialValueAssignmentKind::EnumeratedType(enum_init) => {
                    // Enum variables use DINT (W32/Signed/32-bit) per REQ-EN-codegen-010.
                    let type_info = crate::compile_enum::enum_var_type_info();
                    ctx.var_types.insert(id.clone(), type_info);
                    // Debug tag is DINT per REQ-EN-codegen-012; type_name is the
                    // user-defined enum name (e.g. "COLOR").
                    let name = enum_init.type_name.to_string().to_uppercase();
                    (iec_type_tag::DINT, name)
                }
                InitialValueAssignmentKind::Subrange(ref spec) => {
                    // Subrange variable (e.g., x : MY_RANGE or x : INT (1..100))
                    // Resolve VarTypeInfo from the subrange's base type.
                    let subrange_type = match &spec.spec {
                        SpecificationKind::Named(type_name) => {
                            types.resolve_subrange_type(type_name)
                        }
                        SpecificationKind::Inline(inline_spec) => {
                            let base_tn: ironplc_dsl::common::TypeName =
                                inline_spec.type_name.clone().into();
                            types.get(&base_tn).map(|attrs| &attrs.representation)
                        }
                    };
                    if let Some(st) = subrange_type {
                        if let Some(type_info) = crate::compile_struct::var_type_info_for_field(st)
                        {
                            ctx.var_types.insert(id.clone(), type_info);
                        }
                    }
                    let name = match &spec.spec {
                        SpecificationKind::Named(tn) => tn.to_string().to_uppercase(),
                        SpecificationKind::Inline(inline) => {
                            format!("{}", inline.type_name)
                        }
                    };
                    (iec_type_tag::OTHER, name)
                }
                InitialValueAssignmentKind::LateResolvedType(_) => {
                    // LateResolvedType should have been resolved before codegen.
                    // If we reach here, it indicates a bug in the compiler.
                    return Err(Diagnostic::internal_error_at(Label::span(
                        decl.identifier.span(),
                        "Variable type was not resolved before code generation",
                    )));
                }
                // Other initializer kinds (EnumeratedValues, etc.)
                // do not yet have type info tracked in codegen.
                _ => (iec_type_tag::OTHER, String::new()),
            };

            ctx.debug_var_names.push(VarNameEntry {
                var_index: index,
                function_id: function_id::GLOBAL_SCOPE,
                var_section: map_var_section(&decl.var_type),
                iec_type_tag: type_tag,
                name: id.to_string(),
                type_name: type_name_str,
            });

            record_decl_var_entry(ctx, decl, id, index);
            record_stable_var_entry(ctx, stable_var_ids, id, index);
        }
    }
    Ok(())
}

/// Maps a DSL VariableType to the debug section var_section encoding.
pub(crate) fn map_var_section(vt: &VariableType) -> u8 {
    match vt {
        VariableType::Var => var_section::VAR,
        // The debug section has no encoding for the additional sections
        // (VAR_STAT/VAR_INST/VAR_GENERIC), which are stored like VAR until
        // their placement rules are implemented.
        VariableType::VarStat | VariableType::VarInst | VariableType::VarGeneric => {
            var_section::VAR
        }
        VariableType::VarTemp => var_section::VAR_TEMP,
        VariableType::Input => var_section::VAR_INPUT,
        VariableType::Output => var_section::VAR_OUTPUT,
        VariableType::InOut => var_section::VAR_IN_OUT,
        VariableType::External => var_section::VAR_EXTERNAL,
        VariableType::Global => var_section::VAR_GLOBAL,
        VariableType::Access => var_section::VAR,
    }
}

/// The debug type tag of the type `type_name` names: the type's id when it
/// is elementary, else `OTHER`.
fn resolve_iec_type_tag(types: &TypeEnvironment, type_name: &TypeName) -> u8 {
    types
        .id_of(type_name)
        .and_then(ironplc_analyzer::type_id::elementary_debug_tag)
        .unwrap_or(iec_type_tag::OTHER)
}

/// Computes the debug `(iec_type_tag, type_name)` pair for a function- or
/// FB-local variable declaration. This mirrors the best-effort resolution
/// the program/global path performs (see [`assign_variables`]) but without
/// its side-effecting data-region allocation, so it is safe to call from
/// the per-function slot-assignment loops in `compile_fn`. Composite or
/// unsupported initializers fall back to [`iec_type_tag::OTHER`] with a
/// best-effort type name, matching the global behavior.
pub(crate) fn debug_type_for_decl(decl: &VarDecl, types: &TypeEnvironment) -> (u8, String) {
    match &decl.initializer {
        InitialValueAssignmentKind::Simple(simple) => (
            resolve_iec_type_tag(types, &simple.type_name),
            simple.type_name.name.to_string().to_uppercase(),
        ),
        InitialValueAssignmentKind::String(string_init) => {
            if char_width_for_string_type(&string_init.width).is_wide() {
                (iec_type_tag::WSTRING, "WSTRING".into())
            } else {
                (iec_type_tag::STRING, "STRING".into())
            }
        }
        InitialValueAssignmentKind::Reference(_) => (iec_type_tag::OTHER, "REF_TO".into()),
        // A local aggregate keeps its contents in the data region and its slot
        // holds their offset, exactly as a program-level one does.
        InitialValueAssignmentKind::Structure(struct_init) => (
            iec_type_tag::STRUCT,
            struct_init.type_name.to_string().to_uppercase(),
        ),
        InitialValueAssignmentKind::FunctionBlock(fb_init) => (
            iec_type_tag::FB_INSTANCE,
            fb_init.type_name.to_string().to_uppercase(),
        ),
        InitialValueAssignmentKind::EnumeratedType(enum_init) => (
            iec_type_tag::DINT,
            enum_init.type_name.to_string().to_uppercase(),
        ),
        _ => (iec_type_tag::OTHER, String::new()),
    }
}

/// Computes the debug `(iec_type_tag, type_name)` pair for a user
/// function's return variable, derived from its declared return type.
pub(crate) fn debug_type_for_return(
    return_type: &FunctionReturnType,
    types: &TypeEnvironment,
) -> (u8, String) {
    match return_type {
        FunctionReturnType::String(_) => (iec_type_tag::STRING, "STRING".into()),
        FunctionReturnType::WString(_) => (iec_type_tag::WSTRING, "WSTRING".into()),
        FunctionReturnType::Named(_) => {
            let type_name = return_type.to_type_name();
            (
                resolve_iec_type_tag(types, &type_name),
                type_name.name.to_string().to_uppercase(),
            )
        }
    }
}

/// Emits a LOAD_CONST instruction that pushes a zero value of the given type.
pub(crate) fn emit_zero_const(emitter: &mut Emitter, ctx: &mut CompileContext, op_type: OpType) {
    match op_type.0 {
        OpWidth::W32 => {
            let pool_index = ctx.add_i32_constant(0);
            emitter.emit_load_const_i32(pool_index);
        }
        OpWidth::W64 => {
            let pool_index = ctx.add_i64_constant(0);
            emitter.emit_load_const_i64(pool_index);
        }
        OpWidth::F32 => {
            let pool_index = ctx.add_f32_constant(0.0);
            emitter.emit_load_const_f32(pool_index);
        }
        OpWidth::F64 => {
            let pool_index = ctx.add_f64_constant(0.0);
            emitter.emit_load_const_f64(pool_index);
        }
    }
}
