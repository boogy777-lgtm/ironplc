//! Finalizes an emitted function into optimized bytecode and holds the result per compiled function.

use ironplc_container::FunctionId;
use ironplc_dsl::diagnostic::Diagnostic;

use super::CompileContext;
use crate::emit::Emitter;

/// Holds the compiled bytecode and metadata for a user-defined function.
pub(crate) struct CompiledFunction {
    pub(crate) function_id: FunctionId,
    pub(crate) bytecode: Vec<u8>,
    pub(crate) max_stack_depth: u16,
    /// Most temp string buffers the body ever holds live at once; see
    /// [`FinalizedFunction::max_temp_depth`].
    pub(crate) max_temp_depth: u16,
    pub(crate) num_locals: u16,
    pub(crate) num_params: u16,
    pub(crate) name: String,
    /// Per-statement line-map entries with `bytecode_offset` already
    /// remapped through the optimizer. The codegen driver pairs each
    /// entry with this function's `FunctionId` before pushing to the
    /// container builder.
    pub(crate) line_map: Vec<crate::emit::EmittedLineMapEntry>,
}

/// Holds the finalized bytecode and stack depth for a single emitted function.
///
/// Returned by [`finalize_function`] to centralize the
/// optimize → patch jumps → `max_stack_depth()` sequence shared by every
/// code path that emits a function (init, scan, user functions, FB bodies).
/// Centralizing this makes future cross-cutting additions (e.g. source-map
/// plumbing) a one-site change.
pub(crate) struct FinalizedFunction {
    pub(crate) bytecode: Vec<u8>,
    pub(crate) max_stack_depth: u16,
    /// Most temporary string buffers this function ever holds live at one
    /// time. The VM releases a buffer when the instruction consuming it
    /// runs, so this bounds the function's own draw on the pool however
    /// many times its string operations execute.
    pub(crate) max_temp_depth: u16,
    /// Per-statement line-map entries with `bytecode_offset` already
    /// remapped through the optimizer's old→new offset table. Entries
    /// whose pre-optimization offset fell on an instruction that was
    /// elided snap forward to the next surviving instruction; entries
    /// past the new end-of-function are dropped.
    pub(crate) line_map: Vec<crate::emit::EmittedLineMapEntry>,
}

/// Finalizes an emitter into ready-to-store bytecode plus stack depth.
///
/// The optimizer runs before the emitter patches its jumps, so it works on
/// symbolic edges: it is told which offsets the jumps target and reports
/// where every instruction moved, and the emitter then resolves each jump
/// against the new positions.
pub(crate) fn finalize_function(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
) -> Result<FinalizedFunction, Diagnostic> {
    let raw_line_map = emitter.take_line_map();
    let (optimized, offset_map) =
        crate::optimize::optimize(emitter.unpatched_code(), &mut ctx.constants);
    emitter.apply_optimized(optimized, &offset_map);
    let bytecode = emitter.bytecode().to_vec();
    let max_stack_depth = emitter.max_stack_depth();
    let max_temp_depth = emitter.max_temp_depth();
    let line_map =
        crate::optimize::remap_line_map(raw_line_map, &offset_map, bytecode.len() as u16)?;
    Ok(FinalizedFunction {
        bytecode,
        max_stack_depth,
        max_temp_depth,
        line_map,
    })
}
