//! Compilation metadata for user-defined function block types, their instances and methods.

use std::collections::HashMap;

use ironplc_container::{FunctionId, VarIndex};
use ironplc_dsl::common::{FunctionBlockDeclaration, StructureElementInit};
use ironplc_dsl::core::Id;
use ironplc_dsl::diagnostic::Diagnostic;

use super::{OpType, StringVarInfo, VarTypeInfo};

/// Snapshot of the program-level variable-mapping state, captured by
/// `compile_user_function_block` before it repopulates `CompileContext`
/// for a single FB type's own body (and, in the same scope, that type's
/// methods -- OOP extension, ADR-0041 Phase 1). The caller restores from
/// this once *both* the body and its methods have been compiled, since
/// method bodies need the type's field mappings to still be visible in
/// `ctx.variables` after the FB body compile returns.
pub(crate) struct SavedFbScope {
    pub(crate) variables: HashMap<Id, VarIndex>,
    pub(crate) var_types: HashMap<Id, VarTypeInfo>,
    pub(crate) string_vars: HashMap<Id, StringVarInfo>,
    pub(crate) array_vars: HashMap<Id, crate::compile_array::ArrayVarInfo>,
    pub(crate) struct_vars: HashMap<Id, crate::compile_struct::StructVarInfo>,
    pub(crate) struct_array_vars: HashMap<Id, crate::compile_array_struct::StructArrayVarInfo>,
    pub(crate) fb_instances: HashMap<Id, FbInstanceInfo>,
}

/// Metadata for a function block instance variable.
pub(crate) struct FbInstanceInfo {
    /// Variable table index holding the data region offset.
    pub(crate) var_index: VarIndex,
    /// Type ID for FB_CALL dispatch.
    pub(crate) type_id: u16,
    /// Data region byte offset where this instance's fields start.
    pub(crate) data_offset: u32,
    /// Maps field name (lowercase) to field index.
    pub(crate) field_indices: HashMap<String, u8>,
    /// The fields whose characters live in this instance's own runs, by name
    /// (lowercase), as code outside the body reaches them.
    pub(crate) strings: HashMap<String, StringVarInfo>,
    /// The array of instances, when the variable declares several; `data_offset`
    /// is then where the first starts. See `compile_fb_instance`.
    pub(crate) array: Option<crate::compile_fb_instance::InstanceArray>,
}

/// The first type ID of a user-defined function block.
const FIRST_USER_FB_TYPE_ID: u16 = 0x1000;

/// The type ID of each user-defined function block, by upper-cased name.
///
/// The ID of a type is its rank among the names of the compiled types, not the
/// position of its declaration in the container. The position comes from the
/// dependency sort of the declarations, and an edit of a body that starts to
/// call a function can move blocks that have nothing to do with the call; an ID
/// taken from the position would then change the entry of every instance in
/// the variable table and with it the layout of the state (ADR-0073). The set
/// of compiled types is fixed by the declarations of the instances that reach
/// them, so the rank is stable under every edit of a body.
pub(super) fn user_fb_type_ids(
    fb_decls: &[&FunctionBlockDeclaration],
) -> Result<HashMap<String, u16>, Diagnostic> {
    let mut names: Vec<String> = fb_decls
        .iter()
        .map(|fb_decl| fb_decl.name.name.to_string().to_uppercase())
        .collect();
    names.sort();
    names
        .into_iter()
        .enumerate()
        .map(|(rank, name)| {
            let type_id = u16::try_from(rank)
                .ok()
                .and_then(|rank| FIRST_USER_FB_TYPE_ID.checked_add(rank))
                .ok_or_else(Diagnostic::internal_error)?;
            Ok((name, type_id))
        })
        .collect()
}

/// Metadata for a compiled user-defined function block type.
pub(crate) struct UserFbTypeInfo {
    /// Unique type ID for FB_CALL dispatch (starts at 0x1000; see
    /// [`user_fb_type_ids`]).
    pub(crate) type_id: u16,
    /// Number of data-region fields in each instance.
    pub(crate) num_fields: usize,
    /// Maps field name (lowercase) to field index (ordinal position).
    pub(crate) field_indices: HashMap<String, u8>,
    /// Function ID of the compiled FB body in the container.
    pub(crate) function_id: FunctionId,
    /// Variable table offset where the FB body's slots start.
    pub(crate) var_offset: u16,
    /// How an instance is laid out: its slots and the runs of the fields that
    /// do not fit one.
    pub(crate) layout: crate::compile_fb_layout::FbLayout,
    /// Maps field name (lowercase) to its op type for codegen at call sites.
    pub(crate) field_op_types: HashMap<String, OpType>,
    /// The values the fields declare for themselves, which every instance
    /// starts from.
    pub(crate) field_defaults: Vec<StructureElementInit>,
    /// Maps method name (lowercase) to compilation metadata (OOP
    /// extension, ADR-0041 Phase 1). Populated in two steps: `function_id`,
    /// `num_params`, `param_op_types`, and `has_return_value` are known
    /// from the AST alone and set during the pre-scan registration pass;
    /// `param_var_off` is set once the method body is actually compiled
    /// and its scratch region is known (mirrors `var_offset` above).
    pub(crate) methods: HashMap<String, UserMethodInfo>,
}

/// Compilation metadata for a single `METHOD` declared on a user-defined
/// function block type (OOP extension, ADR-0041 Phase 1: static
/// dispatch). A method shares its owning type's field scratch region
/// (`UserFbTypeInfo::var_offset`/`num_fields`) for `self` access, and has
/// its own additional, non-shared param/local scratch region allocated
/// immediately after -- see `compile_method::compile_user_method`.
pub(crate) struct UserMethodInfo {
    /// Function ID of the compiled method body in the container.
    pub(crate) function_id: FunctionId,
    /// Variable table offset where this method's own param/local slots
    /// start (immediately after the owning type's field region, or after
    /// a previously-compiled sibling method's own region). `0` until the
    /// method is actually compiled.
    pub(crate) param_var_off: u16,
    /// Number of VAR_INPUT/VAR_IN_OUT parameters, in declaration order.
    pub(crate) num_params: u16,
    /// Parameter names (lowercase) and op types, in declaration order --
    /// used to resolve both positional and named call-site arguments.
    pub(crate) param_names_in_order: Vec<String>,
    pub(crate) param_op_types: Vec<OpType>,
    pub(crate) has_return_value: bool,
    /// Max operand-stack depth used by the method body. `0` until the
    /// method is actually compiled; used for the scan function's
    /// worst-case stack accounting, same as `max_fb_body_stack`.
    pub(crate) max_stack_depth: u16,
}
