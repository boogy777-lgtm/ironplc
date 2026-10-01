//! Compilation of the CODESYS exception handling statements
//! (`__TRY`/`__CATCH`/`__FINALLY`/`__ENDTRY` and `__THROW`).
//!
//! The VM has no exception object and no propagation, so a `__THROW` is
//! lowered *statically*: it stores its value in the innermost enclosing
//! `__CATCH (e)` variable and jumps to that clause. A `__THROW` with no
//! enclosing `__CATCH` is refused (P4066) rather than silently discarded.
//! This is why a throw inside a POU that the try block calls is not caught
//! here — the handler is chosen when the throwing statement is compiled, not
//! while the program runs.
//!
//! Shape of the emitted code:
//!
//! ```text
//!   ...protected body...      ; a __THROW jumps to CATCH
//!   JMP FINALLY               ; only when the statement has a __CATCH
//! CATCH:
//!   ...catch body...
//! FINALLY:
//!   ...finally body...
//! END:
//! ```
//!
//! When the statement has no `__CATCH` clause the `JMP` and the `CATCH` label
//! are omitted, so the body falls straight into the `__FINALLY` body.

use ironplc_dsl::core::{Id, Located};
use ironplc_dsl::diagnostic::{Diagnostic, Label};
use ironplc_dsl::textual::{CatchClause, Throw, TryCatch};
use ironplc_problems::Problem;

use super::compile::{CompileContext, DEFAULT_OP_TYPE};
use super::compile_expr::{compile_expr, emit_store_var, emit_truncation, resolve_variable_name};
use super::compile_stmt::compile_stmts;
use crate::emit::{self, Emitter};

/// The handler a `__THROW` in the statement being compiled jumps to.
#[derive(Clone)]
pub(crate) struct TryHandler {
    /// Where the enclosing `__CATCH` clause starts.
    pub(crate) catch: emit::Label,
    /// The `(e)` variable the thrown value is stored into, when the clause
    /// names one.
    pub(crate) exception: Option<Id>,
}

/// Compiles `__TRY ... __ENDTRY`.
pub(crate) fn compile_try_catch(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    node: &TryCatch,
) -> Result<(), Diagnostic> {
    // Bound where the protected body ends: `__THROW` lands here. Only a
    // statement with a `__CATCH` clause can receive a throw.
    let catch_label = node.catch.as_ref().map(|_| emitter.create_label());
    // Bound where the catch body ends, so a body that ran to completion
    // skips the handler.
    let finally_label = emitter.create_label();

    if let (Some(catch), Some(label)) = (node.catch.as_ref(), catch_label) {
        ctx.try_handlers.push(TryHandler {
            catch: label,
            exception: catch
                .exception
                .as_ref()
                .and_then(resolve_variable_name)
                .cloned(),
        });
    }
    let body = compile_stmts(emitter, ctx, &node.body);
    if catch_label.is_some() {
        ctx.try_handlers.pop();
    }
    body?;

    if let (Some(catch), Some(label)) = (node.catch.as_ref(), catch_label) {
        emitter.emit_jmp(finally_label);
        emitter.bind_label(label);
        compile_catch_clause(emitter, ctx, catch)?;
    }

    emitter.bind_label(finally_label);
    compile_stmts(emitter, ctx, &node.finally_body)?;
    Ok(())
}

/// Compiles the body of a `__CATCH` clause.
///
/// The clause carries no control flow of its own once the throw has jumped
/// here: a throw written *inside* the handler belongs to the next enclosing
/// `__TRY` (the handler was popped before this runs).
fn compile_catch_clause(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    catch: &CatchClause,
) -> Result<(), Diagnostic> {
    compile_stmts(emitter, ctx, &catch.body)
}

/// Compiles a `__THROW` statement.
pub(crate) fn compile_throw(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    node: &Throw,
) -> Result<(), Diagnostic> {
    // Copy the handler out before compiling: compiling the thrown value needs
    // `ctx` mutably.
    let Some(handler) = ctx.try_handlers.last().cloned() else {
        return Err(Diagnostic::problem(
            Problem::ThrowOutsideTryCatch,
            Label::span(
                node.span(),
                "__THROW must be inside a __TRY statement with a __CATCH clause",
            ),
        ));
    };

    match (&node.value, &handler.exception) {
        (Some(value), Some(name)) => {
            // Same code as `e := value;`: produce the value at the catch
            // variable's width, truncate, store.
            let type_info = ctx.var_type_info(name);
            let op_type = type_info
                .map(|ti| (ti.op_width, ti.signedness))
                .unwrap_or(DEFAULT_OP_TYPE);
            compile_expr(emitter, ctx, value, op_type)?;
            if let Some(ti) = type_info {
                emit_truncation(emitter, ti);
            }
            let index = ctx.var_index(name)?;
            emit_store_var(emitter, index, op_type);
        }
        (Some(value), None) => {
            // The clause catches without naming a variable, so there is
            // nowhere to put the value; evaluate it for its side effects and
            // discard it.
            compile_expr(emitter, ctx, value, DEFAULT_OP_TYPE)?;
            emitter.emit_pop();
        }
        (None, _) => {}
    }

    emitter.emit_jmp(handler.catch);
    Ok(())
}
