//! The function block instances a variable names.
//!
//! A variable that holds function block instances is one of two shapes: a
//! single instance (`timer : TON`), or an array of them
//! (`timers : ARRAY[0..3] OF TON`). Both are entries of `ctx.fb_instances`, and
//! every use of an instance -- calling it, reading a member, writing a member,
//! starting it -- resolves the variable it is spelled with through
//! [`resolve_instance`], to the one answer those uses need: the instance's
//! type, its fields, and how to push a reference to it ([`InstanceReference`]).
//! A plain `timer(...)` and a `timers[i](...)` differ only in that reference.
//!
//! An instance is a contiguous run of slots, so an array of instances is a flat
//! slot array, laid out as an array of structures is
//! (`compile_array_struct`): the variable's slot holds the data-region offset of
//! element 0, and the array descriptor spans every slot of every instance.
//! Element `k` starts `k * element_slots` slots in. Selecting an element is
//! therefore an index computation, and the descriptor is what bounds-checks it
//! at run time.

use std::collections::HashMap;

use ironplc_analyzer::intermediate_type::ArrayDimension;
use ironplc_container::{ContainerBuilder, FieldType, VarIndex, SLOT_BYTES};
use ironplc_dsl::common::{ArrayElementType, ArraySpecificationKind, SpecificationKind};
use ironplc_dsl::core::{Id, Located, SourceSpan};
use ironplc_dsl::diagnostic::{Diagnostic, Label};
use ironplc_dsl::textual::{ArrayVariable, Expr, StructuredVariable, SymbolicVariableKind};

use crate::compile::{CompileContext, FbInstanceInfo, StringVarInfo, MAX_DATA_REGION_SLOTS};
use crate::compile_array::{array_spec_from_inline, dimensions_from_intermediate, emit_flat_index};
use crate::compile_call::resolve_fb_type;
use crate::compile_fb_init::{emit_fb_instance_member_initializers, instance_members};
use crate::compile_fb_layout::FbLayout;
use crate::data_region;
use crate::emit::Emitter;

/// What an instance of a function block type looks like, wherever it is
/// declared.
pub(crate) struct InstanceTemplate {
    pub(crate) type_id: u16,
    /// Bytes one instance occupies.
    pub(crate) instance_bytes: u32,
    pub(crate) field_indices: HashMap<String, u8>,
    pub(crate) layout: FbLayout,
}

/// The template of the function block type `fb_name` (upper case), when it
/// names one: a standard library block is its slots; a user-defined one is its
/// slots and the runs of the fields that do not fit one (`compile_fb_layout`).
pub(crate) fn instance_template(ctx: &CompileContext, fb_name: &str) -> Option<InstanceTemplate> {
    match resolve_fb_type(fb_name) {
        Some((type_id, num_fields, field_indices)) => Some(InstanceTemplate {
            type_id,
            instance_bytes: num_fields as u32 * SLOT_BYTES,
            field_indices,
            layout: FbLayout::default(),
        }),
        None => ctx
            .user_fb_types
            .get(fb_name)
            .map(|user_fb| InstanceTemplate {
                type_id: user_fb.type_id,
                instance_bytes: user_fb.layout.instance_bytes,
                field_indices: user_fb.field_indices.clone(),
                layout: user_fb.layout.clone(),
            }),
    }
}

/// The array of a variable that holds several instances.
#[derive(Clone)]
pub(crate) struct InstanceArray {
    /// Slot-typed descriptor over every slot of every instance.
    pub(crate) desc_index: u16,
    /// The bounds, in instances.
    pub(crate) dimensions: Vec<ArrayDimension>,
    /// Slots one instance occupies.
    pub(crate) instance_slots: u32,
}

/// How code gets a reference to an instance: the value `FB_CALL` and the field
/// instructions find on the stack.
pub(crate) enum InstanceReference<'ast> {
    /// A variable whose slot holds the instance's data-region offset.
    Variable(VarIndex),
    /// An instance at a place known when compiling.
    Fixed(u32),
    /// An element of the array whose first instance the variable's slot holds,
    /// selected by `subscripts`.
    Element {
        var_index: VarIndex,
        array: InstanceArray,
        subscripts: Vec<&'ast Expr>,
        span: SourceSpan,
    },
}

/// An instance, resolved from the variable that names it.
pub(crate) struct ResolvedInstance<'ast> {
    pub(crate) type_id: u16,
    /// Field name (lower case) to field index.
    pub(crate) field_indices: HashMap<String, u8>,
    /// The fields whose characters live in the instance's own runs, as code
    /// outside the body reaches them. Only an instance at a known place has
    /// them.
    pub(crate) strings: HashMap<String, StringVarInfo>,
    reference: InstanceReference<'ast>,
}

impl<'ast> ResolvedInstance<'ast> {
    /// An instance of the variable `info` describes, reached by `reference`.
    fn at(info: &FbInstanceInfo, reference: InstanceReference<'ast>) -> Self {
        ResolvedInstance {
            type_id: info.type_id,
            field_indices: info.field_indices.clone(),
            strings: info.strings.clone(),
            reference,
        }
    }

    /// Emits the push of a reference to the instance. An element's index is
    /// checked against the array's bounds here, by the one instruction that
    /// checks every array access.
    pub(crate) fn emit_reference(
        &self,
        emitter: &mut Emitter,
        ctx: &mut CompileContext,
    ) -> Result<(), Diagnostic> {
        match &self.reference {
            InstanceReference::Variable(var_index) => emitter.emit_fb_load_instance(*var_index),
            InstanceReference::Fixed(offset) => {
                let offset_const = ctx.add_i32_constant(*offset as i32);
                emitter.emit_load_const_i32(offset_const);
            }
            InstanceReference::Element {
                var_index,
                array,
                subscripts,
                span,
            } => {
                // The flat index counts slots, so each stride is scaled by the
                // slots of one instance, as for an array of structures.
                let mut dimensions = dimensions_from_intermediate(&array.dimensions);
                for dimension in &mut dimensions {
                    dimension.stride = dimension.stride.saturating_mul(array.instance_slots);
                }
                emit_flat_index(emitter, ctx, subscripts, &dimensions, span)?;
                // Loading the instance's first slot and dropping it is the
                // bounds check; the index itself stays for the address.
                emitter.emit_dup();
                emitter.emit_load_array(*var_index, array.desc_index);
                emitter.emit_pop();
                let slot_bytes = ctx.add_i64_constant(i64::from(SLOT_BYTES));
                emitter.emit_load_const_i64(slot_bytes);
                emitter.emit_mul_i64();
                emitter.emit_fb_load_instance(*var_index);
                emitter.emit_add_i64();
            }
        }
        Ok(())
    }
}

/// The single instance the variable `name` declares, if it declares one.
pub(crate) fn named_instance<'ast>(
    ctx: &CompileContext,
    name: &Id,
) -> Option<ResolvedInstance<'ast>> {
    let info = ctx
        .fb_instances
        .get(name)
        .filter(|info| info.array.is_none())?;
    Some(ResolvedInstance::at(
        info,
        InstanceReference::Variable(info.var_index),
    ))
}

/// The instance a variable names: a single instance by its name, or an element
/// of an array of instances by the array's name and the subscripts that select
/// it. `None` when the variable is not one, so a caller that cannot tell an
/// instance from a structure (a member's record may be either) can fall through
/// to its own handling.
pub(crate) fn resolve_instance<'ast>(
    ctx: &CompileContext,
    variable: &'ast SymbolicVariableKind,
) -> Result<Option<ResolvedInstance<'ast>>, Diagnostic> {
    match variable {
        SymbolicVariableKind::Named(named) => Ok(named_instance(ctx, &named.name)),
        SymbolicVariableKind::Array(array) => resolve_element(ctx, array),
        _ => Ok(None),
    }
}

/// An element of an array of instances: `fbs[i]`, or `fbs[i][j]`, whose
/// subscripts are read outermost first.
fn resolve_element<'ast>(
    ctx: &CompileContext,
    outermost: &'ast ArrayVariable,
) -> Result<Option<ResolvedInstance<'ast>>, Diagnostic> {
    let mut levels: Vec<&[Expr]> = Vec::new();
    let mut current = outermost;
    let name = loop {
        levels.push(&current.subscripts);
        match current.subscripted_variable.as_ref() {
            SymbolicVariableKind::Array(inner) => current = inner,
            SymbolicVariableKind::Named(named) => break &named.name,
            _ => return Ok(None),
        }
    };
    let Some(info) = ctx.fb_instances.get(name) else {
        return Ok(None);
    };
    let Some(array) = &info.array else {
        return Ok(None);
    };
    levels.reverse();
    let subscripts: Vec<&Expr> = levels.into_iter().flatten().collect();
    if subscripts.len() != array.dimensions.len() {
        return Err(Diagnostic::not_implemented(Label::span(
            outermost.span(),
            "Wrong number of array subscripts",
        )));
    }
    Ok(Some(ResolvedInstance::at(
        info,
        InstanceReference::Element {
            var_index: info.var_index,
            array: array.clone(),
            subscripts,
            span: outermost.span(),
        },
    )))
}

/// An array declaration whose elements are function block instances.
pub(crate) struct InstanceArrayDeclaration {
    template: InstanceTemplate,
    dimensions: Vec<ArrayDimension>,
    /// The element type as written, for the debug type name.
    type_name: String,
}

/// Recognizes a declaration of an array of function block instances. `None`
/// for every other array.
pub(crate) fn instance_array_declaration(
    ctx: &CompileContext,
    spec: &ArraySpecificationKind,
    span: &SourceSpan,
) -> Result<Option<InstanceArrayDeclaration>, Diagnostic> {
    let SpecificationKind::Inline(subranges) = spec else {
        return Ok(None);
    };
    let ArrayElementType::Named(element) = &subranges.type_name else {
        return Ok(None);
    };
    if subranges.ref_to.is_some() {
        return Ok(None);
    }
    let type_name = element.to_string().to_uppercase();
    let Some(template) = instance_template(ctx, &type_name) else {
        return Ok(None);
    };
    let bounds = array_spec_from_inline(subranges, span)?;
    let dimensions = bounds
        .dimensions
        .iter()
        .map(|&(lower, upper)| ArrayDimension { lower, upper })
        .collect();
    Ok(Some(InstanceArrayDeclaration {
        template,
        dimensions,
        type_name,
    }))
}

/// Registers a variable that declares an array of instances: reserves the
/// instances' slots, describes them with one slot-typed descriptor, and records
/// the variable as an instance variable.
///
/// Returns the debug type tag and type name, like `register_array_variable`.
pub(crate) fn register_instance_array(
    ctx: &mut CompileContext,
    builder: &mut ContainerBuilder,
    id: &Id,
    var_index: VarIndex,
    declaration: InstanceArrayDeclaration,
    span: &SourceSpan,
) -> Result<(u8, String), Diagnostic> {
    let InstanceArrayDeclaration {
        template,
        dimensions,
        type_name,
    } = declaration;
    // A run of characters belongs to one instance at a fixed place, which
    // an element selected at run time does not have.
    if !template.layout.storage.is_empty() {
        return Err(Diagnostic::not_implemented(Label::span(
            span.clone(),
            format!("Array of function block '{type_name}' instances, which has a STRING field"),
        )));
    }
    let too_large = || Diagnostic::not_supported(Label::span(span.clone(), "Array too large"));
    let instance_slots = template.instance_bytes / SLOT_BYTES;
    let total_elements = dimensions
        .iter()
        .try_fold(1u32, |count, dimension| {
            let size = (i64::from(dimension.upper) - i64::from(dimension.lower) + 1).max(0);
            count.checked_mul(u32::try_from(size).ok()?)
        })
        .ok_or_else(too_large)?;
    let total_slots = total_elements
        .checked_mul(instance_slots)
        .ok_or_else(too_large)?;
    if total_slots > MAX_DATA_REGION_SLOTS {
        return Err(Diagnostic::not_supported(Label::span(
            span.clone(),
            "Array exceeds maximum 32768 slots",
        )));
    }
    let data_offset = data_region::reserve(ctx, total_slots * SLOT_BYTES, span)?;
    // The element tag names what the slots are, and the extra the block, so
    // that two containers' arrays are the same layout only when their blocks
    // are the same type.
    let desc_index =
        builder.add_array_descriptor(FieldType::FbInstance as u8, total_slots, template.type_id);
    ctx.fb_instances.insert(
        id.clone(),
        FbInstanceInfo {
            var_index,
            type_id: template.type_id,
            data_offset,
            field_indices: template.field_indices,
            strings: HashMap::new(),
            array: Some(InstanceArray {
                desc_index,
                dimensions,
                instance_slots,
            }),
        },
    );
    Ok((
        ironplc_container::debug_section::iec_type_tag::ARRAY,
        format!("ARRAY OF {type_name}"),
    ))
}

/// Emits the start of every instance of the array variable `id`: the slot is
/// pointed at the first instance, and each instance starts from the values its
/// type declares for its fields, as a single instance does.
pub(crate) fn initialize_instance_array(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    id: &Id,
) -> Result<(), Diagnostic> {
    let Some(info) = ctx.fb_instances.get(id) else {
        return Ok(());
    };
    let (var_index, type_id, data_offset) = (info.var_index, info.type_id, info.data_offset);
    let (Some(array), field_indices) = (info.array.clone(), info.field_indices.clone()) else {
        return Ok(());
    };
    let offset_const = ctx.add_i32_constant(data_offset as i32);
    emitter.emit_load_const_i32(offset_const);
    emitter.emit_store_var_i32(var_index);

    let members = instance_members(ctx, type_id, &[]);
    let total_elements: u32 = array
        .dimensions
        .iter()
        .map(|d| (i64::from(d.upper) - i64::from(d.lower) + 1).max(0) as u32)
        .product();
    for element in 0..total_elements {
        let offset = data_offset + element * array.instance_slots * SLOT_BYTES;
        let instance = ResolvedInstance {
            type_id,
            field_indices: field_indices.clone(),
            strings: HashMap::new(),
            reference: InstanceReference::Fixed(offset),
        };
        emit_fb_instance_member_initializers(emitter, ctx, &instance, &members)?;
    }
    Ok(())
}

/// Whether `record` names a function block instance, or tries to and is
/// malformed: a member selected from it is then the member of an instance, not
/// of a structure.
pub(crate) fn names_instance(ctx: &CompileContext, record: &SymbolicVariableKind) -> bool {
    !matches!(resolve_instance(ctx, record), Ok(None))
}

/// Compiles the read of a member of an instance (`timer.Q`, `timers[i].Q`),
/// leaving its value on the stack.
pub(crate) fn compile_member_read(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    member: &StructuredVariable,
) -> Result<(), Diagnostic> {
    let Some(instance) = resolve_instance(ctx, &member.record)? else {
        return Err(Diagnostic::internal_error());
    };
    let field_name = member.field.to_string().to_lowercase();
    // A string field keeps its characters in a run of the instance; its slot
    // only holds where the run is.
    if let Some(info) = instance.strings.get(&field_name) {
        info.emit_load(emitter, ctx);
        return Ok(());
    }
    let field_idx = instance
        .field_indices
        .get(&field_name)
        .copied()
        .ok_or_else(|| {
            Diagnostic::not_implemented(Label::span(
                member.field.span(),
                format!(
                    "Unknown field '{}' on a function block instance \
                     (reading a PROPERTY is not supported yet)",
                    member.field
                ),
            ))
        })?;
    instance.emit_reference(emitter, ctx)?;
    emitter.emit_fb_load_param(field_idx);
    emitter.emit_swap();
    emitter.emit_pop();
    Ok(())
}
