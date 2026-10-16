//! Assembles the container for a program and its user-defined functions, function blocks and methods.

use std::collections::HashMap;

use ironplc_analyzer::{FunctionEnvironment, TypeEnvironment};
use ironplc_container::debug_section::{EnumDefEntry, FuncNameEntry};
use ironplc_container::{
    Container, ContainerBuilder, FbFieldUidEntry, FbTypeId, FunctionId, InstanceId,
    PersistentExtent, UserFbDescriptor, VarIndex,
};
use ironplc_dsl::common::{
    FunctionBlockDeclaration, FunctionDeclaration, InitialValueAssignmentKind, ProgramDeclaration,
    VarDecl,
};
use ironplc_dsl::core::Located;
use ironplc_dsl::diagnostic::Diagnostic;
use ironplc_parser::options::CompilerOptions;

use super::function_block_info::user_fb_type_ids;
use super::{
    finalize_function, string_region_size, CodegenOptions, CompileContext, CompiledFunction,
    OpType, PoolConstant, UserFbTypeInfo, UserMethodInfo, DEFAULT_OP_TYPE, NARROW_CHAR_WIDTH,
    WIDE_CHAR_WIDTH,
};
use crate::compile_fn::{compile_user_function, compile_user_function_block};
use crate::compile_initial_value::{emit_initial_values, emit_temporaries_prologue};
use crate::compile_setup::assign_variables;
use crate::compile_stmt::compile_body;
use crate::emit::Emitter;
use crate::persistent::{program_declarations, program_temporaries};

/// Idempotently registers a POU's source file with the debug section's
/// SOURCE_FILE_TABLE. Called from `compile_program_with_functions` for
/// each top-level program / function / function-block declaration.
fn register_pou_source_file(
    ctx: &mut CompileContext,
    file_id: &ironplc_dsl::core::FileId,
    sources: &dyn crate::source_lookup::SourceLookup,
) {
    if ctx.debug_source_files.get(file_id).is_some() {
        return;
    }
    let bytes = sources.source_bytes(file_id);
    ctx.debug_source_files.register(file_id, bytes);
}

/// Pushes a function's line-map entries (already remapped through the
/// optimizer) into the container builder, paired with the function's
/// `FunctionId`.
fn add_line_map_entries(
    mut builder: ContainerBuilder,
    function_id: FunctionId,
    entries: &[crate::emit::EmittedLineMapEntry],
) -> ContainerBuilder {
    for entry in entries {
        builder = builder.add_line_map_entry(ironplc_container::LineMapEntry {
            function_id,
            bytecode_offset: entry.bytecode_offset,
            file_id: entry.file_id,
            source_line: entry.source_line,
            source_column: entry.source_column,
        });
    }
    builder
}

/// The AST inputs that [`compile_program_with_functions`] operates on,
/// all extracted from the same `Library` at the start of `compile()`.
/// Grouped to keep the helper's argument count manageable.
pub(super) struct ProgramInputs<'a> {
    pub(super) program: &'a ProgramDeclaration,
    pub(super) func_decls: &'a [&'a FunctionDeclaration],
    pub(super) fb_decls: &'a [&'a FunctionBlockDeclaration],
    pub(super) global_vars: &'a [VarDecl],
}

/// Compiles a PROGRAM and its user-defined functions into a container.
///
/// Always emits at least two functions:
/// - Function 0: init (load constants + store variables, called once by VM)
/// - Function 1: scan (program body, called every scan cycle)
/// - Functions 2+: user-defined functions
///
/// When no initial values exist, the init function is a single RET_VOID.
// Internal codegen helper split out from `compile()` purely for
// readability of the public function; called from exactly one site.
//
// `options` and `compiler_options` are both kept on purpose: the former is
// the front-end-facing policy bundle (including the fork's UID tables), the
// latter is the analyzer's own option set. Codegen must ask the arithmetic
// overload resolver the *same* question the analyzer asked — same flags —
// or an expression accepted in analysis is rejected (or compiled
// differently) in codegen.
pub(super) fn compile_program_with_functions(
    inputs: ProgramInputs<'_>,
    functions: &FunctionEnvironment,
    types: &TypeEnvironment,
    enum_map: crate::compile_enum::EnumOrdinalMap,
    options: &CodegenOptions,
    compiler_options: CompilerOptions,
    sources: &dyn crate::source_lookup::SourceLookup,
) -> Result<Container, Diagnostic> {
    let ProgramInputs {
        program,
        func_decls,
        fb_decls,
        global_vars,
    } = inputs;
    let mut ctx = CompileContext::new();
    ctx.enum_map = enum_map;
    ctx.types = crate::type_info::type_representations(types);
    ctx.operand_names = crate::type_info::operand_names(types);
    ctx.string_to_num = options.string_to_num;
    ctx.compiler_options = compiler_options;
    let mut builder = ContainerBuilder::new();

    // Register every top-level POU's source file with the debug
    // section's SOURCE_FILE_TABLE. Each statement's `compile_statement`
    // call later looks up the resulting `SourceFileId` via
    // `ctx.debug_source_files.get(&span.file_id)` and pairs it with the
    // statement's (line, column) for the LineMapEntry. The
    // registration order (program first, then functions, then FB
    // bodies) determines the `file_id` numbering, which appears in
    // the on-disk container.
    register_pou_source_file(&mut ctx, &program.name.span.file_id, sources);
    for f in func_decls {
        register_pou_source_file(&mut ctx, &f.name.span.file_id, sources);
    }
    for fb in fb_decls {
        register_pou_source_file(&mut ctx, &fb.name.name.span.file_id, sources);
    }

    // Pre-scan user-defined FB declarations to register type metadata
    // (field indices, field op types, type IDs) before assign_variables runs.
    // This allows assign_variables to resolve user-defined FB instance variables.
    // The actual FB body compilation happens after program-local variables are
    // assigned, once var_offsets are known.
    let mut compiled_fb_bodies: Vec<CompiledFunction> = Vec::new();
    let fb_type_ids = user_fb_type_ids(fb_decls)?;

    for (next_function_id, fb_decl) in (2_u16..).zip(fb_decls.iter()) {
        let fb_name = fb_decl.name.name.to_string().to_uppercase();
        let mut field_indices: HashMap<String, u8> = HashMap::new();
        let mut field_op_types: HashMap<String, OpType> = HashMap::new();
        let field_decls_owned = crate::compile_fb_layout::instance_fields(fb_decl);
        let field_decls_tmp: Vec<&VarDecl> = field_decls_owned.iter().collect();
        // UIDs of this type's fields named by the engineering-side table
        // (ADR 0059), collected with the pre-scan and recorded once the
        // type ID is assigned below.
        let mut field_uids: Vec<(u8, u64)> = Vec::new();

        for (i, decl) in field_decls_tmp.iter().enumerate() {
            if let Some(id) = decl.identifier.symbolic_id() {
                let name = id.to_string().to_lowercase();
                field_indices.insert(name.clone(), i as u8);
                if let InitialValueAssignmentKind::Simple(_) = &decl.initializer {
                    if let Some(vti) = crate::type_info::decl_type_info(&ctx, decl) {
                        field_op_types.insert(name, (vti.op_width, vti.signedness));
                    } else {
                        field_op_types.insert(name, DEFAULT_OP_TYPE);
                    }
                } else {
                    field_op_types.insert(name, DEFAULT_OP_TYPE);
                }
                if let Some((_, uid)) = options
                    .fb_field_uids
                    .iter()
                    .find(|(key, _)| key.fb_type == fb_decl.name.name && key.field == *id)
                {
                    field_uids.push((i as u8, *uid));
                }
            }
        }

        let type_id = *fb_type_ids
            .get(&fb_name)
            .ok_or_else(Diagnostic::internal_error)?;
        ctx.user_fb_types.insert(
            fb_name,
            UserFbTypeInfo {
                type_id,
                num_fields: field_decls_tmp.len(),
                field_indices,
                function_id: FunctionId::new(next_function_id),
                var_offset: 0, // updated after program vars are assigned
                layout: crate::compile_fb_layout::layout_instance(&field_decls_tmp)?,
                field_op_types,
                field_defaults: crate::compile_fb_init::declared_field_values(
                    types,
                    &field_decls_tmp,
                ),
                methods: HashMap::new(),
            },
        );
        for (field_index, uid) in field_uids {
            ctx.fb_field_uid_entries.push(FbFieldUidEntry {
                fb_type_id: FbTypeId::new(type_id),
                field_index,
                uid,
            });
        }
    }

    // Pre-scan METHOD declarations (OOP extension, ADR-0041 Phase 1) to
    // register their function IDs, parameter shapes, and return-value
    // presence -- all knowable from the AST alone, same reasoning as the
    // FB pre-scan above. Method function IDs continue the sequence after
    // every FB body and every user FUNCTION, so they must be registered
    // only after both counts are known; `param_var_off`/`max_stack_depth`
    // stay `0` until the method is actually compiled (mirrors
    // `UserFbTypeInfo::var_offset`).
    let mut next_method_function_id = 2_u16 + fb_decls.len() as u16 + func_decls.len() as u16;
    for fb_decl in fb_decls.iter() {
        let fb_name = fb_decl.name.name.to_string().to_uppercase();
        for method in &fb_decl.methods {
            let method_name = method.name.to_string().to_lowercase();
            let mut param_names_in_order: Vec<String> = Vec::new();
            let mut param_op_types: Vec<OpType> = Vec::new();
            for decl in &method.variables {
                if !decl.var_type.is_input_compatible() {
                    continue;
                }
                if let Some(id) = decl.identifier.symbolic_id() {
                    param_names_in_order.push(id.to_string().to_lowercase());
                }
                let op_type = if let InitialValueAssignmentKind::Simple(_) = &decl.initializer {
                    crate::type_info::decl_type_info(&ctx, decl)
                        .map_or(DEFAULT_OP_TYPE, |vti| (vti.op_width, vti.signedness))
                } else {
                    DEFAULT_OP_TYPE
                };
                param_op_types.push(op_type);
            }

            let function_id = FunctionId::new(next_method_function_id);
            next_method_function_id += 1;

            if let Some(fb_info) = ctx.user_fb_types.get_mut(&fb_name) {
                fb_info.methods.insert(
                    method_name,
                    UserMethodInfo {
                        function_id,
                        param_var_off: 0, // updated once the method is compiled
                        num_params: param_names_in_order.len() as u16,
                        param_names_in_order,
                        param_op_types,
                        has_return_value: method.return_type.is_some(),
                        max_stack_depth: 0, // updated once the method is compiled
                    },
                );
            }
        }
    }

    // Assign global variable indices first (indices 0..G), after the pre-scan so
    // that a global user-defined instance resolves its type like a local one.
    assign_variables(
        &mut ctx,
        &mut builder,
        global_vars,
        types,
        &options.stable_var_ids,
    )?;
    let num_globals = ctx.variables.len() as u16;

    // Collect program-local variables, skipping VAR_EXTERNAL declarations
    // since they alias the corresponding global variables.
    let local_vars: Vec<VarDecl> = program_declarations(program);
    let edge_inputs = crate::compile_edge::edge_inputs(&program.variables);

    // Assign program-local variable indices (indices G..N).
    // This can now resolve user-defined FB instances via ctx.user_fb_types.
    // Globals and program variables are the persistent prefix whose UIDs
    // may appear in `options.stable_var_ids`; function/FB/method slots are
    // assigned later by their own compilation paths and never reach the
    // lookup (ADR 0053).
    assign_variables(
        &mut ctx,
        &mut builder,
        &local_vars,
        types,
        &options.stable_var_ids,
    )?;
    let program_var_count = ctx.variables.len() as u16;
    // The persistent part ends here: every variable slot and every byte of the
    // data region reserved so far belongs to a global or to the program, and
    // everything reserved from now on (function slots, function block working
    // slots, string temporaries) is working memory. The container declares
    // this point as its persistent extent (ADR-0073); no other site in the
    // compiler decides what is persistent.
    let persistent_extent = PersistentExtent {
        instance_id: InstanceId::DEFAULT,
        var_start: 0,
        var_count: program_var_count,
        data_start: 0,
        data_len: ctx.data_region_offset,
    };

    // The program's `VAR_TEMP` variables are working memory: they take the
    // slots and the data region right after the persistent part, where the
    // slots of functions are, and are not part of what the container declares
    // persistent. They start again at every scan (the prologue of the scan).
    let temporaries: Vec<VarDecl> = program_temporaries(program);
    assign_variables(&mut ctx, &mut builder, &temporaries, types, &[])?;

    // Now compile the FB bodies with correct var_offsets.
    let mut compiled_functions = Vec::new();
    let mut var_offset = VarIndex::new(ctx.variables.len() as u16);

    // Compile user FUNCTIONs before FUNCTION_BLOCK bodies so that FB bodies
    // can resolve calls to user functions via `ctx.user_functions` (functions
    // register themselves at the end of `compile_user_function`). Per
    // IEC 61131-3 functions cannot instantiate FBs, so they don't require FB
    // bodies to have been compiled first.
    //
    // Function IDs are POSITIONAL in the container (`get_function`
    // indexes the `functions` Vec). FB bodies are inserted at positions
    // 2..(2 + N_fb), so user functions must start at the next free
    // position so each callable POU has a unique runtime id.
    let user_fn_id_base = 2u16 + fb_decls.len() as u16;
    for (next_function_id, func_decl) in (user_fn_id_base..).zip(func_decls.iter()) {
        let compiled = compile_user_function(
            func_decl,
            FunctionId::new(next_function_id),
            var_offset,
            &mut ctx,
            functions,
            &mut builder,
            types,
            num_globals,
        )?;
        var_offset = VarIndex::new(var_offset.raw() + compiled.num_locals);
        compiled_functions.push(compiled);
    }

    let mut compiled_methods: Vec<CompiledFunction> = Vec::new();
    for fb_decl in fb_decls {
        let fb_name = fb_decl.name.name.to_string().to_uppercase();
        let fb_func_id = ctx.user_fb_types[&fb_name].function_id;
        let field_var_off = var_offset.raw();

        // Update the var_offset in the registered type info.
        let Some(fb_type) = ctx.user_fb_types.get_mut(&fb_name) else {
            // The type was registered when `fb_func_id` was read above.
            return Err(Diagnostic::internal_error());
        };
        fb_type.var_offset = field_var_off;

        let (compiled, saved_scope) = compile_user_function_block(
            fb_decl,
            fb_func_id,
            field_var_off,
            &mut ctx,
            &mut builder,
            types,
            num_globals,
        )?;
        var_offset = VarIndex::new(var_offset.raw() + compiled.num_locals);
        compiled_fb_bodies.push(compiled);

        // Compile this type's methods (OOP extension, ADR-0041 Phase 1)
        // while `ctx.variables` still holds this type's field mappings
        // (that's the whole reason `compile_user_function_block` didn't
        // restore them itself -- see its doc comment).
        let methods = crate::compile_method::compile_user_fb_methods(
            fb_decl,
            &fb_name,
            field_var_off,
            &mut var_offset,
            &mut ctx,
            &mut builder,
            types,
        )?;
        compiled_methods.extend(methods);

        // Now restore the program-level view for the next FB type.
        ctx.variables = saved_scope.variables;
        ctx.var_types = saved_scope.var_types;
        ctx.string_vars = saved_scope.string_vars;
        ctx.array_vars = saved_scope.array_vars;
        ctx.struct_vars = saved_scope.struct_vars;
        ctx.struct_array_vars = saved_scope.struct_array_vars;
        ctx.fb_instances = saved_scope.fb_instances;
    }

    let total_variables = var_offset;

    // Emit bytecode for variable initial values into the init emitter.
    let mut init_emitter = Emitter::new();
    emit_initial_values(&mut init_emitter, &mut ctx, global_vars, types)?;
    emit_initial_values(&mut init_emitter, &mut ctx, &local_vars, types)?;
    init_emitter.emit_ret_void();

    // Compile the program body into the scan emitter. Mark
    // FunctionId::SCAN as the current caller so any CALL / user FB_CALL
    // emitted from inside the body records a call-graph edge.
    let mut scan_emitter = Emitter::new();
    ctx.current_function_id = Some(FunctionId::SCAN);
    emit_temporaries_prologue(&mut scan_emitter, &mut ctx, &temporaries, types)?;
    crate::compile_edge::bind_edge_inputs(&mut scan_emitter, &mut ctx, &edge_inputs)?;
    compile_body(
        &mut scan_emitter,
        &mut ctx,
        &program.body,
        &program.name.span(),
    )?;
    ctx.current_function_id = None;
    scan_emitter.emit_ret_void();

    // Build the container.
    builder = builder.num_variables(total_variables.raw());

    // Configure data region for STRING variables.
    if ctx.data_region_offset > 0 {
        builder = builder.data_region_bytes(ctx.data_region_offset);
    }

    // Compute the max stack depth needed by any user-defined FB body or
    // METHOD. The scan function's reported max_stack_depth must include
    // it because FB_CALL / METHOD_CALL recursively enter execute() on
    // the shared stack.
    let max_fb_body_stack: u16 = compiled_fb_bodies
        .iter()
        .chain(compiled_methods.iter())
        .map(|c| c.max_stack_depth)
        .max()
        .unwrap_or(0);

    // Function 0: init, Function 1: scan
    let init = finalize_function(&mut init_emitter, &mut ctx)?;
    builder = builder.add_function(
        FunctionId::INIT,
        &init.bytecode,
        init.max_stack_depth,
        program_var_count,
        0,
    );
    builder = add_line_map_entries(builder, FunctionId::INIT, &init.line_map);

    let scan = finalize_function(&mut scan_emitter, &mut ctx)?;

    // Size the temp string buffer pool. A callee's buffers sit on top of
    // whatever its caller holds live, so the bound is the heaviest path
    // through the call graph, each function weighted by the most buffers
    // it holds live at once. The init function is not reachable from SCAN
    // (it only stores initial values), so it is bounded separately.
    let temp_depths: HashMap<FunctionId, u16> = compiled_functions
        .iter()
        .chain(compiled_fb_bodies.iter())
        .chain(compiled_methods.iter())
        .map(|c| (c.function_id, c.max_temp_depth))
        .chain(core::iter::once((FunctionId::SCAN, scan.max_temp_depth)))
        .collect();
    let num_temp_bufs = crate::call_graph::longest_path(&ctx.call_graph, FunctionId::SCAN, |f| {
        temp_depths.get(&f).copied().unwrap_or(0)
    })?
    .max(init.max_temp_depth);

    if ctx.data_region_offset > 0 && num_temp_bufs > 0 {
        // Temp-buffer slots are uniform. When any wide string exists, size
        // each slot in wide bytes so an intermediate WSTRING value of up to
        // max_string_capacity code units fits (the VM allocator divides the
        // slot capacity by the value's char_width). Narrow-only programs
        // keep the original byte-identical sizing.
        let temp_char_width = if ctx.has_wide_string {
            WIDE_CHAR_WIDTH
        } else {
            NARROW_CHAR_WIDTH
        };
        builder = builder
            .num_temp_bufs(num_temp_bufs)
            .max_temp_buf_bytes(string_region_size(ctx.max_string_capacity, temp_char_width));
    }

    builder = builder.add_function(
        FunctionId::SCAN,
        &scan.bytecode,
        // FB_CALL recursively enters execute() on the shared stack, so the
        // scan function's reported stack depth must include the deepest FB
        // body's depth.
        scan.max_stack_depth + max_fb_body_stack,
        program_var_count,
        0,
    );
    builder = add_line_map_entries(builder, FunctionId::SCAN, &scan.line_map);

    // Add user-defined function block bodies.
    for compiled in &compiled_fb_bodies {
        builder = builder.add_function(
            compiled.function_id,
            &compiled.bytecode,
            compiled.max_stack_depth,
            compiled.num_locals,
            compiled.num_params,
        );
        builder = add_line_map_entries(builder, compiled.function_id, &compiled.line_map);
    }

    // Add user FB type descriptors to the container.
    for fb_info in ctx.user_fb_types.values() {
        builder = builder.add_user_fb_type(UserFbDescriptor {
            type_id: FbTypeId::new(fb_info.type_id),
            function_id: fb_info.function_id,
            var_offset: fb_info.var_offset,
            num_fields: fb_info.num_fields as u8,
        });
    }

    // Add user-defined functions.
    for compiled in &compiled_functions {
        builder = builder.add_function(
            compiled.function_id,
            &compiled.bytecode,
            compiled.max_stack_depth,
            compiled.num_locals,
            compiled.num_params,
        );
        builder = add_line_map_entries(builder, compiled.function_id, &compiled.line_map);
    }

    // Add METHOD bodies (OOP extension, ADR-0041 Phase 1). Function IDs
    // are positional in the container, so these must be added last --
    // registration assigned them `2 + fb_decls.len() + func_decls.len()..`,
    // continuing directly after the user functions just added above.
    for compiled in &compiled_methods {
        builder = builder.add_function(
            compiled.function_id,
            &compiled.bytecode,
            compiled.max_stack_depth,
            compiled.num_locals,
            compiled.num_params,
        );
        builder = add_line_map_entries(builder, compiled.function_id, &compiled.line_map);
    }

    // Add the SOURCE_FILE_TABLE (tag 6). `ctx.debug_source_files`
    // accumulated one entry per POU source file; insertion order is
    // the file's SourceFileId, which the line-map entries above
    // already reference.
    let source_file_entries = std::mem::take(&mut ctx.debug_source_files).into_entries();
    if !source_file_entries.is_empty() {
        builder = builder.add_source_files(source_file_entries);
    }

    // Compute the worst-case PLC call depth from the static call graph
    // and stamp it on the header so `VmReady::start` can reject
    // containers that wouldn't fit in the embedder's frame buffer
    // (Trap::ProgramExceedsCallDepth). Cycles should already be
    // rejected by semantic analysis (Problem::RecursiveCycle); the DFS
    // is a defensive backstop that surfaces an internal error.
    let max_call_depth =
        crate::call_graph::compute_max_call_depth(&ctx.call_graph, FunctionId::SCAN)?;

    builder = builder
        .init_function_id(FunctionId::INIT)
        .entry_function_id(FunctionId::SCAN)
        .shared_globals_size(program_var_count)
        .add_persistent_extent(persistent_extent)
        .max_call_depth(max_call_depth);

    // Add debug info.
    let program_name = program.name.to_string();
    builder = builder
        .add_func_name(FuncNameEntry {
            function_id: FunctionId::INIT,
            name: format!("{program_name}_init"),
        })
        .add_func_name(FuncNameEntry {
            function_id: FunctionId::SCAN,
            name: program_name,
        });

    for compiled in &compiled_fb_bodies {
        builder = builder.add_func_name(FuncNameEntry {
            function_id: compiled.function_id,
            name: compiled.name.clone(),
        });
    }
    for compiled in &compiled_functions {
        builder = builder.add_func_name(FuncNameEntry {
            function_id: compiled.function_id,
            name: compiled.name.clone(),
        });
    }
    for compiled in &compiled_methods {
        builder = builder.add_func_name(FuncNameEntry {
            function_id: compiled.function_id,
            name: compiled.name.clone(),
        });
    }
    // Add the type section's variable table, one entry per index
    // `0..num_variables`. `collect_variable_table` reports an internal error
    // for an index no allocation site recorded, so a forgotten site fails
    // the compilation instead of silently corrupting the layout hash. It is
    // collected before the debug loops below, which move their vectors out
    // of `ctx`.
    let variable_table = ctx.collect_variable_table(total_variables.raw())?;
    // The function block type table states the field layout of each user
    // function block by type, from the same entries.
    for descriptor in
        crate::compile_fb_layout::type_descriptors(&ctx.user_fb_types, &variable_table)?
    {
        builder = builder.add_fb_type(descriptor);
    }
    for entry in variable_table {
        builder = builder.add_var_entry(entry);
    }

    // Add the stable variable ID sub-table (ADR 0053), ascending by
    // `var_index`. It is excluded from the layout hash, so a rename that
    // only rebinds a UID keeps the active hash.
    for entry in ctx.collect_stable_vars()? {
        builder = builder.add_stable_var(entry);
    }

    // Add the FB field UID sub-table (ADR 0059), ascending by
    // `(fb_type_id, field_index)`. Like the stable variable IDs, it is
    // identity rather than layout and stays out of the layout hash.
    for entry in ctx.collect_fb_field_uids()? {
        builder = builder.add_fb_field_uid(entry);
    }

    for entry in ctx.debug_var_names {
        builder = builder.add_var_name(entry);
    }
    for entry in ctx.debug_string_layouts {
        builder = builder.add_string_layout(entry);
    }
    for (type_name, values) in &ctx.enum_map.definitions {
        builder = builder.add_enum_def(EnumDefEntry {
            type_name: type_name.clone(),
            values: values.clone(),
        });
    }

    // Add constants to the pool.
    //
    // This must come after every `finalize_function` call, not merely after
    // the last `compile_*` one: the peephole optimizer interns constants of
    // its own (folding `LOAD_CONST_I32; TRUNC_*` to an already-truncated
    // value), so the pool is still growing until the final function has been
    // finalized. Emitting it earlier leaves those loads pointing past the end
    // of the emitted pool, which the VM rejects as `InvalidConstantIndex`.
    for constant in &ctx.constants {
        match constant {
            PoolConstant::I32(v) => builder = builder.add_i32_constant(*v),
            PoolConstant::I64(v) => builder = builder.add_i64_constant(*v),
            PoolConstant::F32(v) => builder = builder.add_f32_constant(*v),
            PoolConstant::F64(v) => builder = builder.add_f64_constant(*v),
            PoolConstant::Str(v) => builder = builder.add_str_constant(v),
            PoolConstant::WStr(v) => builder = builder.add_wstr_constant(v),
        }
    }

    let container = builder.build();

    // Verify operand-stack discipline before the container escapes codegen.
    // See `crate::stack_balance` for why this is a hard error and
    // `specs/design/bytecode-verifier-rules.md` for the rules it enforces.
    crate::stack_balance::verify_container(&container)?;

    Ok(container)
}
