//! The state a compilation carries: variable tables, scopes, labels and debug entries.

use std::collections::{BTreeMap, BTreeSet, HashMap, HashSet};

use ironplc_analyzer::IntermediateType;
use ironplc_container::debug_section::{StringLayoutEntry, VarNameEntry};
use ironplc_container::{FbFieldUidEntry, FunctionId, StableVarEntry, VarEntry, VarIndex};
use ironplc_dsl::core::{Id, Located};
use ironplc_dsl::diagnostic::{Diagnostic, Label};
use ironplc_parser::options::CompilerOptions;
use ironplc_problems::text;

use super::{
    FbInstanceInfo, OpType, PoolConstant, StringToNumPolicies, StringVarInfo, UserFbTypeInfo,
    UserFunctionInfo, VarTypeInfo, DEFAULT_OP_TYPE,
};
use crate::compile_var_table::slot_entry;

pub(crate) struct CompileContext {
    /// Maps variable identifiers to their variable table indices.
    pub(crate) variables: HashMap<Id, VarIndex>,
    /// Maps variable identifiers to their type information.
    pub(crate) var_types: HashMap<Id, VarTypeInfo>,
    /// Ordered list of constants added to the constant pool.
    pub(crate) constants: Vec<PoolConstant>,
    /// Stack of the labels of the enclosing loops, for EXIT and CONTINUE
    /// statement compilation. Each loop pushes its labels; EXIT and CONTINUE
    /// jump to those of the top.
    pub(crate) loop_labels: Vec<crate::compile_loop::LoopLabels>,
    /// Stack of the handlers of the enclosing `__TRY` statements, for
    /// `__THROW` compilation. Only a `__TRY` that has a `__CATCH` clause
    /// pushes one, so a throw with no handler on the stack is refused.
    pub(crate) try_handlers: Vec<crate::compile_try_catch::TryHandler>,
    /// The emitter label of every statement label in the POU body being
    /// emitted, for `JMP` compilation and for binding `label:` statements.
    /// Set by [`crate::compile_stmt::compile_statements`] for the duration of
    /// one body.
    pub(crate) jump_labels: crate::compile_jump::JumpLabels,
    /// Maps STRING variable identifiers to their data region metadata.
    pub(crate) string_vars: HashMap<Id, StringVarInfo>,
    /// Maps FB instance variable identifiers to their metadata.
    pub(crate) fb_instances: HashMap<Id, FbInstanceInfo>,
    /// Maps array variable identifiers to their metadata.
    pub(crate) array_vars: HashMap<Id, crate::compile_array::ArrayVarInfo>,
    /// Maps structure variable identifiers to their metadata.
    pub(crate) struct_vars: HashMap<Id, crate::compile_struct::StructVarInfo>,
    /// Maps top-level `ARRAY OF <struct>` variable identifiers to their metadata.
    /// Kept apart from `array_vars`, whose elements occupy a single slot each.
    pub(crate) struct_array_vars: HashMap<Id, crate::compile_array_struct::StructArrayVarInfo>,
    /// Pre-computed ordinal mappings for named enumeration types.
    pub(crate) enum_map: crate::compile_enum::EnumOrdinalMap,
    /// What every type is, by the id an expression's `expr_type` carries.
    /// See [`crate::type_info::expr_type_info`].
    pub(crate) types: HashMap<ironplc_dsl::type_id::TypeId, IntermediateType>,
    /// The name the arithmetic overloads know a value of each type by.
    /// See [`crate::type_info::expr_operand_name`].
    pub(crate) operand_names: HashMap<ironplc_dsl::type_id::TypeId, ironplc_dsl::common::TypeName>,
    /// The behavior policies `STRING_TO_<numeric>` calls select their
    /// builtin by (ADR-0049).
    pub(crate) string_to_num: StringToNumPolicies,
    /// The options the analyzer ran with. Codegen asks the arithmetic
    /// overload resolver the analyzer asked, with the same options, for the
    /// result type of each step of an arithmetic call.
    pub(crate) compiler_options: CompilerOptions,
    /// Next available byte offset in the data region.
    pub(crate) data_region_offset: u32,
    /// Maximum string capacity across all STRING variables (for temp buffer sizing).
    pub(crate) max_string_capacity: u16,
    /// True when any WSTRING (wide) string is declared. Temp buffers are then
    /// sized in wide bytes so an intermediate wide value fits (ADR-0035).
    pub(crate) has_wide_string: bool,
    /// Debug info: variable name entries collected during assign_variables.
    pub(crate) debug_var_names: Vec<VarNameEntry>,
    /// Debug info: STRING variable data-region layouts collected during assign_variables.
    pub(crate) debug_string_layouts: Vec<StringLayoutEntry>,
    /// Debug info: registry mapping each referenced source `FileId` to its
    /// `SourceFileId` (debug section SOURCE_FILE_TABLE index) plus cached
    /// source bytes for (line, column) conversion in `compile_statement`.
    pub(crate) debug_source_files: crate::source_lookup::SourceFileRegistry,
    /// Maps user-defined function name (lowercase) to compilation metadata.
    pub(crate) user_functions: HashMap<String, UserFunctionInfo>,
    /// Maps user-defined FB type name (uppercase) to compilation metadata.
    ///
    /// Ascending by name, which is the order the type ids are ranked in
    /// (`user_fb_type_ids`), so iterating it visits the types in ascending
    /// type id, on every run.
    pub(crate) user_fb_types: BTreeMap<String, UserFbTypeInfo>,
    /// Next available type ID for user-defined function blocks.
    /// When compiling a function body that returns a value, describes how an
    /// early `RETURN` statement should produce the return value before the
    /// `RET` opcode. `None` for programs and FBs (RETURN emits `RET_VOID`).
    pub(crate) current_function_return: Option<CurrentFunctionReturn>,
    /// Function whose body is currently being emitted; `None` outside any
    /// body. Set by the body-compilation entry points (SCAN, user
    /// functions, user-FB bodies) so [`record_call_edge`] knows who the
    /// caller is. Edges are aggregated into [`call_graph`] for the
    /// post-emission `max_call_depth` computation.
    ///
    /// [`record_call_edge`]: CompileContext::record_call_edge
    /// [`call_graph`]: CompileContext::call_graph
    pub(crate) current_function_id: Option<FunctionId>,
    /// Adjacency map: caller `FunctionId` → set of callees. Populated by
    /// [`record_call_edge`] from inside `compile_call.rs` (user CALL)
    /// and `compile_stmt.rs` (user `FB_CALL`). Empty for intrinsic
    /// FBs, which have no PLC body.
    ///
    /// [`record_call_edge`]: CompileContext::record_call_edge
    pub(crate) call_graph: HashMap<FunctionId, BTreeSet<FunctionId>>,
    /// One entry per compiler-assigned variable index, for the type section's
    /// variable table (see `compile_var_table`). Unlike the scope-scoped maps
    /// above, this collection is never saved or restored: an index, once
    /// assigned, is never reused, so its entry must survive the per-function
    /// scope swaps. A `None` slot is an index no allocation site claimed,
    /// which `collect_variable_table` reports as an internal error.
    pub(crate) var_entries: Vec<Option<VarEntry>>,
    /// Stable variable IDs (ADR 0053) for the persistent variables whose
    /// declaration name matched `CodegenOptions::stable_var_ids`, recorded
    /// when their index was assigned. Function locals, method parameters,
    /// FB field regions and scratch never reach this collection. Like
    /// `var_entries`, it is never saved or restored across the per-function
    /// scope swaps; `collect_stable_vars` sorts it and rejects a duplicate
    /// `var_index`.
    pub(crate) stable_var_entries: Vec<StableVarEntry>,
    /// FB field UIDs (ADR 0059) for the user-defined FB type fields the
    /// engineering-side table named, recorded during the FB pre-scan. Like
    /// `var_entries`, it is never saved or restored across the per-function
    /// scope swaps; `collect_fb_field_uids` sorts it and rejects a duplicate
    /// `(fb_type_id, field_index)`.
    pub(crate) fb_field_uid_entries: Vec<FbFieldUidEntry>,
    /// The `VAR_IN_OUT` parameters of the function being compiled. Each
    /// one's slot holds a reference to the caller's variable (a
    /// variable-table index, as `REF_TO` stores) rather than a value, so it
    /// is reached through [`in_out_ref_slot`], never [`var_index`].
    ///
    /// [`in_out_ref_slot`]: CompileContext::in_out_ref_slot
    /// [`var_index`]: CompileContext::var_index
    pub(crate) in_out_params: HashSet<Id>,
}

/// Describes how a `RETURN` statement should yield the function's value.
#[derive(Clone, Copy)]
pub(crate) enum CurrentFunctionReturn {
    /// Scalar return: load the variable, then emit `RET`.
    Scalar {
        var_index: VarIndex,
        op_type: OpType,
    },
    /// STRING/WSTRING return: load the string from the data region into a
    /// temp buffer, then emit `RET`.
    String { data_offset: u32 },
}

impl CompileContext {
    pub(crate) fn new() -> Self {
        CompileContext {
            variables: HashMap::new(),
            var_types: HashMap::new(),
            constants: Vec::new(),
            loop_labels: Vec::new(),
            try_handlers: Vec::new(),
            jump_labels: crate::compile_jump::JumpLabels::new(),
            string_vars: HashMap::new(),
            fb_instances: HashMap::new(),
            array_vars: HashMap::new(),
            struct_vars: HashMap::new(),
            struct_array_vars: HashMap::new(),
            in_out_params: HashSet::new(),
            data_region_offset: 0,
            max_string_capacity: 0,
            has_wide_string: false,
            debug_var_names: Vec::new(),
            debug_string_layouts: Vec::new(),
            debug_source_files: crate::source_lookup::SourceFileRegistry::new(),
            user_functions: HashMap::new(),
            user_fb_types: BTreeMap::new(),
            enum_map: crate::compile_enum::EnumOrdinalMap::default(),
            types: HashMap::new(),
            operand_names: HashMap::new(),
            string_to_num: StringToNumPolicies::default(),
            compiler_options: CompilerOptions::default(),
            current_function_return: None,
            current_function_id: None,
            call_graph: HashMap::new(),
            var_entries: Vec::new(),
            stable_var_entries: Vec::new(),
            fb_field_uid_entries: Vec::new(),
        }
    }

    /// Returns the exit label for the innermost enclosing loop, if any.
    pub(crate) fn current_loop_exit(&self) -> Option<crate::emit::Label> {
        self.loop_labels.last().map(|labels| labels.exit)
    }

    /// Returns the label where the innermost enclosing loop goes on with its
    /// next iteration, if any, and records that it is used.
    pub(crate) fn current_loop_next(&mut self) -> Option<crate::emit::Label> {
        self.loop_labels.last_mut().map(|labels| {
            labels.next_used = true;
            labels.next
        })
    }

    /// Records a static call-graph edge from the function whose body is
    /// currently being emitted to `callee`.
    ///
    /// No-op when no body is being emitted (e.g. during INIT, which
    /// only stores constants and never CALLs) — `current_function_id`
    /// is `None` outside body-compilation entry points.
    pub(crate) fn record_call_edge(&mut self, callee: FunctionId) {
        if let Some(caller) = self.current_function_id {
            self.call_graph.entry(caller).or_default().insert(callee);
        }
    }

    /// Looks up a variable index by identifier, using the provided span for error reporting.
    ///
    /// A `VAR_IN_OUT` parameter's slot holds a reference, not the value, so
    /// loading or storing it directly would be wrong. Sites that handle one
    /// ask [`Self::in_out_ref_slot`] first; every other site reaches here
    /// and is refused.
    pub(crate) fn var_index(&self, name: &Id) -> Result<VarIndex, Diagnostic> {
        if self.in_out_params.contains(name) {
            return Err(Diagnostic::not_implemented(Label::span(
                name.span(),
                "VAR_IN_OUT parameter used where only a local or global variable is supported",
            )));
        }
        self.variables.get(name).copied().ok_or_else(|| {
            Diagnostic::text(name.span(), text::variable_undefined())
                .with_context("variable", &name.to_string())
        })
    }

    /// Returns the slot holding the reference when `name` is a `VAR_IN_OUT`
    /// parameter of the function being compiled, or `None` for any other
    /// variable.
    pub(crate) fn in_out_ref_slot(&self, name: &Id) -> Option<VarIndex> {
        if !self.in_out_params.contains(name) {
            return None;
        }
        self.variables.get(name).copied()
    }

    /// Looks up type information for a variable by identifier.
    pub(crate) fn var_type_info(&self, name: &Id) -> Option<VarTypeInfo> {
        self.var_types.get(name).copied()
    }

    /// Returns the op_type for a variable by identifier, falling back to defaults.
    pub(crate) fn var_op_type(&self, name: &Id) -> OpType {
        self.var_types
            .get(name)
            .map(|info| (info.op_width, info.signedness))
            .unwrap_or(DEFAULT_OP_TYPE)
    }

    /// Allocates a scratch variable with a synthetic name and returns its index.
    ///
    /// The name uses a `$` prefix which is illegal in IEC 61131-3 identifiers,
    /// guaranteeing no collision with user-defined variables.
    pub(crate) fn allocate_scratch_variable(&mut self, suffix: &str) -> VarIndex {
        let idx = VarIndex::new(self.variables.len() as u16);
        self.variables
            .insert(Id::from(&format!("$scratch_{}", suffix)), idx);
        self.record_var_entry(idx, slot_entry());
        idx
    }
}
