//! Initial values of structure storage.
//!
//! Emits the init-function code that gives every storage location of a
//! structure its initial value. One recursive routine computes it for a
//! location of any shape -- a structure variable, a structure nested in
//! another, an element of an array of structures -- from the value the
//! declaration of that location states, else the value the member itself
//! declares (`a : INT := 5`), else the type's default. Separated from
//! `compile_struct.rs` to keep module sizes within the 1000-line guideline.

use ironplc_dsl::core::{Located, SourceSpan};
use ironplc_dsl::diagnostic::{Diagnostic, Label};

use ironplc_analyzer::intermediate_type::{ArrayDimension, IntermediateType};
use ironplc_analyzer::TypeEnvironment;
use ironplc_container::{SlotIndex, VarIndex};
use ironplc_dsl::common::{
    ConstantKind, InitialValueAssignmentKind, StructInitialValueAssignmentKind,
    StructureElementInit,
};
use ironplc_dsl::construct::merge_member_inits;

use super::compile::{
    emit_string_literal_load, CompileContext, OpType, OpWidth, DEFAULT_STRING_MAX_LENGTH,
};
use super::compile_array_struct::{ElementStringField, StructArrayVarInfo};
use super::compile_expr::compile_constant;
use super::compile_setup::emit_zero_const;
use super::compile_struct::{
    build_struct_fields, emit_truncation_for_field, resolve_field_op_type, StructFieldInfo,
    StructVarInfo,
};
use crate::emit::Emitter;

/// The value a scalar type starts at when its declaration states none: the
/// lower bound of a subrange (IEC 61131-3 §2.4.3.1: the "leftmost value"),
/// else zero.
pub(crate) fn scalar_default(ty: &IntermediateType) -> i128 {
    match ty {
        IntermediateType::Subrange { min_value, .. } => *min_value,
        _ => 0,
    }
}

/// Emits the initial value of a scalar location: the value its declaration
/// states, else `default_value`.
///
/// This is the one place a scalar's initial value is emitted, for a
/// structure member and for a variable alike.
pub(crate) fn emit_scalar_initial_value(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    op_type: OpType,
    stated: Option<&StructInitialValueAssignmentKind>,
    default_value: i128,
) -> Result<(), Diagnostic> {
    if let Some(value) = stated {
        return compile_struct_field_init(emitter, ctx, value, op_type);
    }
    match op_type.0 {
        OpWidth::W32 if default_value != 0 => {
            let pool_index = ctx.add_i32_constant(default_value as i32);
            emitter.emit_load_const_i32(pool_index);
        }
        OpWidth::W64 if default_value != 0 => {
            let pool_index = ctx.add_i64_constant(default_value as i64);
            emitter.emit_load_const_i64(pool_index);
        }
        _ => emit_zero_const(emitter, ctx, op_type),
    }
    Ok(())
}

/// Compiles an explicit initial value for a structure field.
///
/// Handles constant expressions (integer/real/boolean literals) and
/// enumerated values from `StructInitialValueAssignmentKind`.
///
/// Note the `Array`/`Structure` arm returns `Ok(())` without pushing a value.
/// For a well-typed program that arm is unreachable -- `op_type` is `None` for
/// a struct- or array-typed field, so those go through the recursion in
/// `initialize_struct_fields` instead. It is reached only when the initializer
/// does not match the field's type, where pushing nothing leaves the caller's
/// unconditional store unbalanced.
fn compile_struct_field_init(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    init: &StructInitialValueAssignmentKind,
    op_type: OpType,
) -> Result<(), Diagnostic> {
    match init {
        StructInitialValueAssignmentKind::Constant(constant) => {
            compile_constant(emitter, ctx, constant, op_type)
        }
        StructInitialValueAssignmentKind::EnumeratedValue(ev) => {
            // REQ-EN-codegen-050: Resolve enum value to ordinal and push as i32 constant.
            let ordinal = crate::compile_enum::resolve_enum_ordinal(&ctx.enum_map, ev)?;
            let pool_index = ctx.add_i32_constant(ordinal);
            emitter.emit_load_const_i32(pool_index);
            Ok(())
        }
        StructInitialValueAssignmentKind::Array(_)
        | StructInitialValueAssignmentKind::Structure(_) => {
            // Unreachable for a well-typed program: see the note on this
            // function. Nested structures are handled by the recursion in
            // `initialize_struct_fields`, not here.
            Ok(())
        }
        StructInitialValueAssignmentKind::Expression(expr) => {
            // A general (possibly non-constant) expression, e.g.
            // `pDevice^.Delta` -- `ironplcc check` fully supports this;
            // codegen does not yet implement evaluating it at instance
            // construction time.
            Err(Diagnostic::not_implemented(Label::span(
                expr.span(),
                "Expression-valued struct/FB-instance field initializer",
            )))
        }
        StructInitialValueAssignmentKind::LateBound(late_bound) => {
            // `xform_resolve_late_bound_expr_kind` replaces every one of
            // these with an enumerated value or an expression, so reaching
            // codegen with one means that pass did not run.
            Err(Diagnostic::internal_error_at(Label::span(
                late_bound.value.span(),
                "Unresolved struct/FB-instance field initializer",
            )))
        }
    }
}

/// Which STRING headers are already in the data region when a field is
/// initialized.
#[derive(Clone, Copy, PartialEq)]
enum StringHeaders {
    /// None: the field writes its own header.
    Write,
    /// The strided descriptor of an array of structures wrote it
    /// ([`initialize_element_strings`]), so a value can be stored at once.
    Written,
    /// None, and none will be: a STRING nested inside an element of an array
    /// of structures has no access path yet, so it is left untouched.
    Unreachable,
}

/// How the fields of one structure are initialized.
#[derive(Clone, Copy)]
struct Mode {
    headers: StringHeaders,
    /// The region starts zeroed and is not initialized again, as for an
    /// element of an array of structures, so a member whose value is the
    /// plain zero default needs no store.
    zeroed: bool,
}

/// The data region a structure is stored in: the variable that holds its
/// base offset and the slot-typed descriptor over it.
#[derive(Clone, Copy)]
struct Region {
    var_index: VarIndex,
    desc_index: u16,
    data_offset: u32,
}

/// Emits the initialization of a structure variable: stores its data-region
/// offset into the variable's slot, then initializes every field.
///
/// `element_inits` are the explicit field initializers from the declaration,
/// empty when there are none.
pub(crate) fn initialize_struct_variable(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    info: &StructVarInfo,
    element_inits: &[StructureElementInit],
    span: &SourceSpan,
) -> Result<(), Diagnostic> {
    let offset_const = ctx.add_i32_constant(info.data_offset as i32);
    emitter.emit_load_const_i32(offset_const);
    emitter.emit_store_var_i32(info.var_index);

    // The element headers first: a member value stored below must land in a
    // header that is already there.
    initialize_element_strings(
        emitter,
        ctx,
        info.data_offset,
        info.scratch_var_index,
        &info.element_strings,
        span,
    )?;

    initialize_struct_fields(
        emitter,
        ctx,
        Region {
            var_index: info.var_index,
            desc_index: info.desc_index,
            data_offset: info.data_offset,
        },
        0,
        &info.fields,
        element_inits,
        Mode {
            headers: StringHeaders::Write,
            zeroed: false,
        },
        span,
    )
}

/// Emits the initialization of a variable that is an array of structures:
/// stores its data-region offset into the variable's slot, writes the STRING
/// headers of its elements, then gives every element its members' values.
pub(crate) fn initialize_struct_array_variable(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    info: &StructArrayVarInfo,
    span: &SourceSpan,
) -> Result<(), Diagnostic> {
    let offset_const = ctx.add_i32_constant(info.data_offset as i32);
    emitter.emit_load_const_i32(offset_const);
    emitter.emit_store_var_i32(info.var_index);

    initialize_element_strings(
        emitter,
        ctx,
        info.data_offset,
        info.scratch_var_index,
        &info.element_strings,
        span,
    )?;

    initialize_array_elements(
        emitter,
        ctx,
        Region {
            var_index: info.var_index,
            desc_index: info.desc_index,
            data_offset: info.data_offset,
        },
        0,
        &info.element_type,
        &info.dimensions,
        span,
    )
}

/// Writes the header of every element's copy of each STRING field of an
/// array of structures in a variable's data region.
///
/// One `STR_INIT_ARRAY` per field covers every element, through the strided
/// descriptor registered for the field (ADR-0069). The opcode reads the base
/// address from a variable, so it is first computed into the scratch
/// variable. Without this, the headers stay zeroed, and a zero `char_width`
/// traps on first access.
pub(crate) fn initialize_element_strings(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    data_offset: u32,
    scratch_var_index: Option<VarIndex>,
    fields: &[ElementStringField],
    span: &SourceSpan,
) -> Result<(), Diagnostic> {
    if fields.is_empty() {
        return Ok(());
    }
    let scratch = scratch_var_index.ok_or_else(|| {
        Diagnostic::internal_error_at(Label::span(
            span.clone(),
            "STRING fields of array-of-struct elements have no scratch variable",
        ))
    })?;
    for field in fields {
        let byte_offset = field
            .slot_offset
            .checked_mul(8)
            .and_then(|offset| offset.checked_add(data_offset))
            .ok_or_else(|| {
                Diagnostic::not_supported(Label::span(span.clone(), "Data region overflow"))
            })?;
        let offset_const = ctx.add_i32_constant(byte_offset as i32);
        emitter.emit_load_const_i32(offset_const);
        emitter.emit_store_var_i32(scratch);
        emitter.emit_str_init_array(scratch, field.desc_index);
    }
    Ok(())
}

/// Gives every element of an array of structures its members' values.
///
/// `base_slot` is the slot offset of element 0 within the region. The
/// elements are laid out one after another, so each is the same structure
/// initialization at a stride; an array of structures can state no initial
/// value of its own, so every element starts from what its members declare.
fn initialize_array_elements(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    region: Region,
    base_slot: u32,
    element_type: &IntermediateType,
    dimensions: &[ArrayDimension],
    span: &SourceSpan,
) -> Result<(), Diagnostic> {
    let IntermediateType::Structure { fields } = element_type else {
        return Ok(());
    };
    let (element_fields, _) = build_struct_fields(fields, span)?;
    let stride = element_type.slot_count().map_err(|_| {
        Diagnostic::not_implemented(Label::span(
            span.clone(),
            "Array element structure is unsupported",
        ))
    })?;
    let total = dimensions
        .iter()
        .try_fold(1u32, |acc, d| {
            let size = (d.upper as i64 - d.lower as i64 + 1).max(0) as u32;
            acc.checked_mul(size)
        })
        .ok_or_else(|| Diagnostic::not_supported(Label::span(span.clone(), "Array too large")))?;
    for element in 0..total {
        initialize_struct_fields(
            emitter,
            ctx,
            region,
            base_slot + element * stride,
            &element_fields,
            &[],
            Mode {
                headers: StringHeaders::Written,
                zeroed: true,
            },
            span,
        )?;
    }
    Ok(())
}

/// The members a structure location starts at: those its declaration states
/// laid over those its type declares, empty when it is not a structure
/// location. Read through the one source of declared values.
pub(crate) fn declared_members(
    types: &TypeEnvironment,
    init: &InitialValueAssignmentKind,
) -> Vec<StructureElementInit> {
    match types.initial_value_of(init) {
        Some(StructInitialValueAssignmentKind::Structure(members)) => members,
        _ => Vec::new(),
    }
}

/// The members a structure value states, empty when it is not one.
fn structure_members(value: Option<&StructInitialValueAssignmentKind>) -> &[StructureElementInit] {
    match value {
        Some(StructInitialValueAssignmentKind::Structure(members)) => members,
        _ => &[],
    }
}

/// Initializes the fields of a structure stored at `base_slot` of `region`.
///
/// Each field starts at the value its location's declaration states in
/// `overrides` (`s : S := (a := 3)`), else the value the field declares for
/// itself in the type, else the type's default. A structure field applies
/// the same rule to its own fields, so the declaration of one member never
/// discards what the type declares for the others.
#[allow(clippy::too_many_arguments)]
fn initialize_struct_fields(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    region: Region,
    base_slot: u32,
    fields: &[StructFieldInfo],
    overrides: &[StructureElementInit],
    mode: Mode,
    span: &SourceSpan,
) -> Result<(), Diagnostic> {
    for field in fields {
        let slot = SlotIndex::new(base_slot + field.slot_offset.raw());
        let explicit = overrides
            .iter()
            .find(|m| m.name.to_string().to_lowercase() == field.name)
            .map(|m| &m.init);
        // The value the field starts at, when it states one.
        let stated = explicit.or(field.initial_value.as_ref());

        if let Some(op_type) = field.op_type {
            let default_value = scalar_default(&field.field_type);
            if mode.zeroed && stated.is_none() && default_value == 0 {
                continue;
            }
            emit_scalar_initial_value(emitter, ctx, op_type, stated, default_value)?;
            // Truncate narrow types (e.g., SINT stored in W32 slot)
            emit_truncation_for_field(emitter, &field.field_type);
            let idx_const = ctx.add_i32_constant(slot.raw() as i32);
            emitter.emit_load_const_i32(idx_const);
            emitter.emit_store_array(region.var_index, region.desc_index);
            continue;
        }

        match &field.field_type {
            IntermediateType::Structure { fields: inner } => {
                let nested = merge_member_inits(
                    structure_members(field.initial_value.as_ref()),
                    structure_members(explicit),
                );
                let (inner_fields, _) = build_struct_fields(inner, span)?;
                let inner_mode = Mode {
                    headers: match mode.headers {
                        StringHeaders::Written => StringHeaders::Unreachable,
                        other => other,
                    },
                    ..mode
                };
                initialize_struct_fields(
                    emitter,
                    ctx,
                    region,
                    slot.raw(),
                    &inner_fields,
                    &nested,
                    inner_mode,
                    span,
                )?;
            }
            IntermediateType::String { char_width, .. } => {
                let Some(max_length) = field.string_max_length else {
                    continue;
                };
                let byte_offset = region.data_offset + slot.raw() * 8;
                if mode.headers == StringHeaders::Write {
                    emitter.emit_str_init(byte_offset, max_length, *char_width);
                }
                if let Some(StructInitialValueAssignmentKind::Constant(
                    ConstantKind::CharacterString(literal),
                )) = stated
                {
                    if mode.headers != StringHeaders::Unreachable {
                        emit_string_literal_load(emitter, ctx, &literal.value, *char_width);
                        emitter.emit_str_store_var(byte_offset);
                    }
                }
            }
            IntermediateType::Array {
                element_type,
                dimensions,
            } => initialize_array_field(
                emitter,
                ctx,
                region,
                slot.raw(),
                &field.name,
                (element_type, dimensions),
                stated,
                mode,
                span,
            )?,
            _ => {}
        }
    }
    Ok(())
}

/// Initializes an array member stored at `slot` of `region`.
///
/// The elements of an array of structures start from what their members
/// declare; the elements of an array of scalars from the values the
/// declaration states, when it states any.
#[allow(clippy::too_many_arguments)]
fn initialize_array_field(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    region: Region,
    slot: u32,
    name: &str,
    (element_type, dimensions): (&IntermediateType, &[ArrayDimension]),
    stated: Option<&StructInitialValueAssignmentKind>,
    mode: Mode,
    span: &SourceSpan,
) -> Result<(), Diagnostic> {
    let values = match stated {
        Some(StructInitialValueAssignmentKind::Array(elements)) => {
            crate::compile_array::flatten_array_initial_values(elements)?
        }
        _ => Vec::new(),
    };
    let unsupported = |what: &str| {
        Diagnostic::not_implemented(Label::span(
            span.clone(),
            format!("Initial values for {what} member '{name}'"),
        ))
    };
    match element_type {
        IntermediateType::String {
            max_len,
            char_width,
        } => {
            // STRING/WSTRING array field: each element is a string of its own,
            // laid out one after another, so it is initialized like a string
            // member at its own offset.
            let max_length = max_len.unwrap_or(DEFAULT_STRING_MAX_LENGTH as u128) as u16;
            let total_elements = dimensions
                .iter()
                .fold(1u32, |acc, d| acc * (d.upper - d.lower + 1) as u32);
            let stride = super::compile::string_region_size(max_length, *char_width);
            let field_byte_offset = region.data_offset + slot * 8;
            if mode.headers == StringHeaders::Write {
                for i in 0..total_elements {
                    emitter.emit_str_init(field_byte_offset + i * stride, max_length, *char_width);
                }
            }
            if mode.headers == StringHeaders::Unreachable {
                return Ok(());
            }
            for (i, value) in values.iter().enumerate() {
                let ConstantKind::CharacterString(literal) = value else {
                    return Err(unsupported(
                        "an array of STRING of a value that is not a string",
                    ));
                };
                emit_string_literal_load(emitter, ctx, &literal.value, *char_width);
                emitter.emit_str_store_var(field_byte_offset + i as u32 * stride);
            }
            Ok(())
        }
        IntermediateType::Structure { .. } => {
            if stated.is_some() {
                return Err(unsupported("an array of structures"));
            }
            initialize_array_elements(emitter, ctx, region, slot, element_type, dimensions, span)
        }
        _ => {
            let Some(op_type) = resolve_field_op_type(element_type) else {
                return if values.is_empty() {
                    Ok(())
                } else {
                    Err(unsupported("an array of this element type"))
                };
            };
            for (i, value) in values.iter().enumerate() {
                compile_constant(emitter, ctx, value, op_type)?;
                emit_truncation_for_field(emitter, element_type);
                let idx_const = ctx.add_i32_constant((slot + i as u32) as i32);
                emitter.emit_load_const_i32(idx_const);
                emitter.emit_store_array(region.var_index, region.desc_index);
            }
            Ok(())
        }
    }
}
