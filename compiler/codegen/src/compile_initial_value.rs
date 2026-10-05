//! Initial values of declared variables: what every storage location starts
//! at, once for a program, a function block instance or a global, and on every
//! call for the locals of a function or a method.
//!
//! There is one routine per declaration (`emit_declaration_initial_value`);
//! the two ways a location starts differ only in what is done for a location
//! that declares no value (`Start`), not in how a value is read or emitted.

use ironplc_container::VarIndex;
use ironplc_dsl::common::{
    ConstantKind, InitialValueAssignmentKind, ReferenceInitialValue, SpecificationKind,
    SubrangeSpecification, VarDecl, VariableType,
};
use ironplc_dsl::core::{Id, Located};
use ironplc_dsl::diagnostic::{Diagnostic, Label};

use ironplc_analyzer::intermediate_type::IntermediateType;
use ironplc_analyzer::TypeEnvironment;

use super::compile::{emit_string_literal_load, CompileContext, OpType, DEFAULT_OP_TYPE};
use super::compile_expr::{compile_constant, emit_store_var, emit_truncation, resolve_variable};
use super::compile_setup::emit_zero_const;
use crate::emit::Emitter;

/// How a storage location comes to hold its initial value.
#[derive(Clone, Copy, PartialEq, Eq, Debug)]
pub(crate) enum Start {
    /// Once, when the unit is set up, from memory that is already zero: a
    /// location that declares no value is left alone.
    Once,
    /// On every call, from whatever the previous call left: a location that
    /// declares no value is zeroed.
    EveryCall,
}

impl Start {
    /// What a location is written with, given the value its declaration states:
    /// `None` leaves the location as it is, `Some(None)` writes the default of
    /// its type, `Some(Some(value))` writes the stated value.
    fn stated<T>(self, stated: Option<T>) -> Option<Option<T>> {
        match (stated, self) {
            (Some(value), _) => Some(Some(value)),
            (None, Start::EveryCall) => Some(None),
            (None, Start::Once) => None,
        }
    }
}

/// Emits bytecode to initialize the variables that have declared initial
/// values.
pub(crate) fn emit_initial_values(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    declarations: &[VarDecl],
    types: &TypeEnvironment,
) -> Result<(), Diagnostic> {
    for decl in declarations {
        emit_declaration_initial_value(emitter, ctx, decl, types, Start::Once)?;
    }
    Ok(())
}

/// Emits the initialization of one declaration: the value it states, else the
/// value the type it names declares, else the default of its type.
///
/// For a scalar this is a load, a truncation (if narrow) and a store; for a
/// STRING, STR_INIT to set up the header in the data region and then the
/// stated value; for a structure, an array or a function block instance, the
/// recursive routines of their own modules. This is the one routine through
/// which every kind of unit starts its variables, so a kind of declaration
/// that is handled here is handled for every unit.
pub(crate) fn emit_declaration_initial_value(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    decl: &VarDecl,
    types: &TypeEnvironment,
    start: Start,
) -> Result<(), Diagnostic> {
    if let Some(id) = decl.identifier.symbolic_id() {
        match &decl.initializer {
            InitialValueAssignmentKind::Simple(_) => {
                // The global_var_decl parser produces Simple for all
                // named types, including structs.  If the variable was
                // registered as a struct during assign_variables,
                // initialize it like a Structure initializer.
                if let Some(struct_info) = ctx.struct_vars.get(id).cloned() {
                    crate::compile_struct_init::initialize_struct_variable(
                        emitter,
                        ctx,
                        &struct_info,
                        &crate::compile_struct_init::declared_members(types, &decl.initializer),
                        &decl.identifier.span(),
                    )?;
                } else if let Some(stated) = start.stated(types.initial_value_of(&decl.initializer))
                {
                    let var_index = ctx.var_index(id)?;
                    let type_info = ctx.var_type_info(id);
                    let op_type = type_info
                        .map(|ti| (ti.op_width, ti.signedness))
                        .unwrap_or(DEFAULT_OP_TYPE);

                    crate::compile_struct_init::emit_scalar_initial_value(
                        emitter,
                        ctx,
                        op_type,
                        stated.as_ref(),
                        0,
                    )?;

                    if let Some(ti) = type_info {
                        emit_truncation(emitter, ti);
                    }

                    emit_store_var(emitter, var_index, op_type);
                }
            }
            InitialValueAssignmentKind::String(string_init) => {
                if let Some(info) = ctx.string_vars.get(id).cloned() {
                    // Initialize the string header in the data region.
                    info.emit_init(emitter);

                    // If there's an initial value, load and store it. The
                    // literal is encoded at the variable's width so the
                    // store's encoding check passes (ADR-0034).
                    if let Some(lit) = &string_init.initial_value {
                        emit_string_literal_load(emitter, ctx, &lit.value, info.char_width);
                        info.emit_store(emitter, ctx);
                    }
                }
            }
            InitialValueAssignmentKind::FunctionBlock(fb_init) => {
                if let Some(fb_info) = ctx.fb_instances.get(id) {
                    let data_offset = fb_info.data_offset;
                    let var_index = fb_info.var_index;
                    let type_id = fb_info.type_id;
                    // Store the data region byte offset into the variable slot.
                    let offset_const = ctx.add_i32_constant(data_offset as i32);
                    emitter.emit_load_const_i32(offset_const);
                    emitter.emit_store_var_i32(var_index);

                    // The runs of its strings come first: they are what a stated
                    // value is stored into.
                    crate::compile_fb_layout::emit_instance_storage(emitter, ctx, id, types)?;

                    // An instance starts at the values its type declares for its
                    // fields, with those of `timer : TON := (PT := T#100MS)`
                    // laid over them. The slot offset has to be in place first,
                    // because each member store addresses the instance through
                    // it.
                    let members =
                        crate::compile_fb_init::instance_members(ctx, type_id, &fb_init.init);
                    if let Some(instance) = crate::compile_fb_instance::named_instance(ctx, id) {
                        crate::compile_fb_init::emit_fb_instance_member_initializers(
                            emitter, ctx, &instance, &members,
                        )?;
                    }
                }
            }
            InitialValueAssignmentKind::Array(array_init) => {
                // An array of structures holds the data region offset in
                // its variable slot, like a structure variable does, and
                // each element starts from what its members declare.
                if ctx
                    .fb_instances
                    .get(id)
                    .is_some_and(|info| info.array.is_some())
                {
                    // An array of function block instances starts every
                    // instance as a single instance starts.
                    if !array_init.initial_values.is_empty() {
                        return Err(Diagnostic::not_implemented(Label::span(
                            decl.identifier.span(),
                            "Initial values for an array of function block instances",
                        )));
                    }
                    crate::compile_fb_instance::initialize_instance_array(emitter, ctx, id)?;
                } else if let Some(struct_array_info) = ctx.struct_array_vars.get(id) {
                    if !array_init.initial_values.is_empty() {
                        return Err(Diagnostic::not_implemented(Label::span(
                            decl.identifier.span(),
                            "Initial values for an array of structures",
                        )));
                    }
                    let struct_array_info = struct_array_info.clone();
                    crate::compile_struct_init::initialize_struct_array_variable(
                        emitter,
                        ctx,
                        &struct_array_info,
                        &decl.identifier.span(),
                    )?;
                } else if let Some(array_info) = ctx.array_vars.get(id) {
                    let data_offset = array_info.data_offset;
                    let var_index = array_info.var_index;
                    let desc_index = array_info.desc_index;
                    let element_vti = array_info.element_var_type_info;
                    let is_string = array_info.is_string_element;
                    let element_char_width = array_info.string_char_width;

                    // Store data_offset into the variable slot (like FB instances).
                    let offset_const = ctx.add_i32_constant(data_offset as i32);
                    emitter.emit_load_const_i32(offset_const);
                    emitter.emit_store_var_i32(var_index);

                    if is_string {
                        // Initialize all string headers in the array.
                        emitter.emit_str_init_array(var_index, desc_index);

                        // Emit STR_STORE_ARRAY_ELEM for each initial string value.
                        // String literals are encoded at the element width so
                        // the array element's encoding check passes.
                        if !array_init.initial_values.is_empty() {
                            let values = crate::compile_array::flatten_array_initial_values(
                                &array_init.initial_values,
                            )?;
                            for (i, value) in values.iter().enumerate() {
                                if let ConstantKind::CharacterString(lit) = value {
                                    emit_string_literal_load(
                                        emitter,
                                        ctx,
                                        &lit.value,
                                        element_char_width,
                                    );
                                } else {
                                    compile_constant(emitter, ctx, value, DEFAULT_OP_TYPE)?;
                                }
                                let idx_const = ctx.add_i32_constant(i as i32);
                                emitter.emit_load_const_i32(idx_const);
                                emitter.emit_str_store_array_elem(var_index, desc_index);
                            }
                        }
                    } else {
                        // Emit STORE_ARRAY for each initial value.
                        if !array_init.initial_values.is_empty() {
                            let values = crate::compile_array::flatten_array_initial_values(
                                &array_init.initial_values,
                            )?;
                            let element_op_type = (element_vti.op_width, element_vti.signedness);
                            for (i, value) in values.iter().enumerate() {
                                compile_constant(emitter, ctx, value, element_op_type)?;
                                emit_truncation(emitter, element_vti);
                                let idx_const = ctx.add_i32_constant(i as i32);
                                emitter.emit_load_const_i32(idx_const);
                                emitter.emit_store_array(var_index, desc_index);
                            }
                        }
                    }
                }
            }
            InitialValueAssignmentKind::Reference(ref_init) => {
                let var_index = ctx.var_index(id)?;
                match &ref_init.initial_value {
                    Some(ReferenceInitialValue::Ref(target_var)) => {
                        // REF(var) → load the target variable's index as a u64 constant.
                        let target_index = resolve_variable(ctx, target_var)?;
                        let pool_index = ctx.add_i64_constant(target_index.into());
                        emitter.emit_load_const_i64(pool_index);
                    }
                    _ => {
                        // NULL or no initializer → store null sentinel (u64::MAX).
                        let pool_index = ctx.add_i64_constant(u64::MAX as i64);
                        emitter.emit_load_const_i64(pool_index);
                    }
                }
                emitter.emit_store_var_i64(var_index);
            }
            InitialValueAssignmentKind::Structure(_) => {
                if let Some(struct_info) = ctx.struct_vars.get(id).cloned() {
                    crate::compile_struct_init::initialize_struct_variable(
                        emitter,
                        ctx,
                        &struct_info,
                        &crate::compile_struct_init::declared_members(types, &decl.initializer),
                        &decl.identifier.span(),
                    )?;
                }
            }
            InitialValueAssignmentKind::EnumeratedType(enum_init) => {
                // Emit LOAD_CONST_I32(ordinal) + STORE_VAR_I32 per REQ-EN-codegen-020.
                let var_index = ctx.var_index(id)?;
                let op_type = DEFAULT_OP_TYPE;
                let ordinal = if let Some(ev) = &enum_init.initial_value {
                    crate::compile_enum::resolve_enum_ordinal(&ctx.enum_map, ev)?
                } else {
                    // No explicit init: use type declaration default (REQ-EN-codegen-021/022).
                    let type_upper = enum_init.type_name.to_string().to_uppercase();
                    crate::compile_enum::resolve_enum_default_ordinal(&ctx.enum_map, &type_upper)
                };
                let pool_index = ctx.add_i32_constant(ordinal);
                emitter.emit_load_const_i32(pool_index);
                emit_store_var(emitter, var_index, op_type);
            }
            InitialValueAssignmentKind::EnumeratedValues(_) => {
                // An inline enumeration starts at the value it states,
                // else at its first value, which is zero.
                if let Some(stated) = start.stated(types.initial_value_of(&decl.initializer)) {
                    let var_index = ctx.var_index(id)?;
                    crate::compile_struct_init::emit_scalar_initial_value(
                        emitter,
                        ctx,
                        DEFAULT_OP_TYPE,
                        stated.as_ref(),
                        0,
                    )?;
                    emit_store_var(emitter, var_index, DEFAULT_OP_TYPE);
                }
            }
            InitialValueAssignmentKind::Subrange(ref subrange) => {
                let spec = &subrange.spec;
                // A subrange variable starts at its stated value, else its
                // lower bound (min_value)
                // per IEC 61131-3 §2.4.3.1 (default is the "leftmost value").
                let var_index = ctx.var_index(id)?;
                let type_info = ctx.var_type_info(id);
                let op_type = type_info
                    .map(|ti| (ti.op_width, ti.signedness))
                    .unwrap_or(DEFAULT_OP_TYPE);

                let min_value = subrange_lower_bound(types, spec);

                if let Some(min_val) = min_value {
                    let stated = types.initial_value_of(&decl.initializer);
                    crate::compile_struct_init::emit_scalar_initial_value(
                        emitter,
                        ctx,
                        op_type,
                        stated.as_ref(),
                        min_val,
                    )?;

                    if let Some(ti) = type_info {
                        emit_truncation(emitter, ti);
                    }

                    emit_store_var(emitter, var_index, op_type);
                }
            }
            // Other initializer kinds (EnumeratedValues, etc.)
            // do not yet support initial values in codegen.
            _ => {}
        }
    }
    Ok(())
}

/// Emits a bytecode prologue that re-initializes a function's non-parameter
/// local variables and return variable on every call. IEC 61131-3 requires
/// functions to be stateless (locals must not retain values between calls).
///
/// For locals with a declared initial value, emits the same LOAD_CONST +
/// TRUNC + STORE_VAR sequence that `emit_initial_values()` uses. For locals
/// without an initializer and for the return variable, emits a zero-store.
pub(crate) fn emit_function_local_prologue(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    variables: &[VarDecl],
    return_id: &Id,
    return_var_index: VarIndex,
    return_op_type: OpType,
    types: &TypeEnvironment,
) -> Result<(), Diagnostic> {
    // Re-initialize VAR locals (not Input parameters) the way every other
    // unit initializes its variables.
    for decl in variables {
        if decl.var_type == VariableType::Var {
            emit_declaration_initial_value(emitter, ctx, decl, types, Start::EveryCall)?;
        }
    }

    // Zero-initialize the return variable.
    if let Some(struct_info) = ctx.struct_vars.get(return_id).cloned() {
        // Struct return: store data_offset into the return var slot and
        // zero all struct fields. Functions are stateless, so the struct
        // must be re-initialized on every call. The struct was registered
        // under `return_var_index`, so `struct_info.var_index` is that slot.
        crate::compile_struct_init::initialize_struct_variable(
            emitter,
            ctx,
            &struct_info,
            &[],
            &return_id.span(),
        )?;
    } else if let Some(info) = ctx.string_vars.get(return_id) {
        // STRING/WSTRING return: initialize the string header in the data region.
        info.emit_init(emitter);
    } else {
        emit_zero_const(emitter, ctx, return_op_type);
        emit_store_var(emitter, return_var_index, return_op_type);
    }

    Ok(())
}

/// The lower bound of a subrange, which is where a location of the subrange
/// starts when nothing says otherwise (IEC 61131-3 §2.4.3.1: the leftmost
/// value). `None` when the bound is not a number the environment knows.
pub(crate) fn subrange_lower_bound(
    types: &TypeEnvironment,
    spec: &SpecificationKind<SubrangeSpecification>,
) -> Option<i128> {
    match spec {
        SpecificationKind::Named(type_name) => types.get(type_name).and_then(|attrs| {
            if let IntermediateType::Subrange { min_value, .. } = &attrs.representation {
                Some(*min_value)
            } else {
                None
            }
        }),
        SpecificationKind::Inline(inline_spec) => {
            inline_spec.subrange.start.as_signed_integer().map(|si| {
                if si.is_neg {
                    -(si.value.value as i128)
                } else {
                    si.value.value as i128
                }
            })
        }
    }
}
