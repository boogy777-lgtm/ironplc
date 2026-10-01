//! Compilation of the CODESYS jump statements: `JMP` with `label:` statement
//! labels, the `CALC` conditional call and the `__WAIT` statement.
//!
//! Labels are POU-body scoped: a jump may target a label that appears later in
//! the body, so [`collect_labels`] walks the whole body first and gives every
//! label its emitter label. The statements then bind theirs in emission
//! order, and the emitter patches the forward jumps (the same mechanism
//! `IF`/`CASE` use for their branch labels).
//!
//! A label name that no statement defines is reported by the semantic rule
//! `rule_jump_target`; the check here is the defensive twin codegen needs
//! because it must find a label to emit a jump to.

use std::collections::HashMap;
use std::convert::Infallible;

use ironplc_dsl::core::{Id, Located};
use ironplc_dsl::diagnostic::{Diagnostic, Label};
use ironplc_dsl::textual::{Jump, LabelStatement, StmtKind, Wait};
use ironplc_dsl::visitor::Visitor;
use ironplc_problems::Problem;

use super::compile::CompileContext;
use super::compile_expr::{compile_expr, condition_op_type};
use crate::emit::{self, Emitter};

/// The emitter label of every statement label in a body, collected before the
/// body is emitted so a jump can target a label that comes later.
pub(crate) type JumpLabels = HashMap<Id, emit::Label>;

/// Walks a POU body and creates one emitter label per statement label.
///
/// A repeated name keeps the first label: the semantic rule reports the
/// duplicate, and binding the first label twice is harmless.
pub(crate) fn collect_labels(emitter: &mut Emitter, body: &[StmtKind]) -> JumpLabels {
    let mut collector = LabelCollector {
        emitter,
        labels: JumpLabels::new(),
    };
    for stmt in body {
        // The collector has no failure mode; recursing reaches the labels in
        // nested bodies (IF bodies, loops, TRY clauses) too.
        let _ = stmt.recurse_visit(&mut collector);
    }
    collector.labels
}

struct LabelCollector<'a> {
    emitter: &'a mut Emitter,
    labels: JumpLabels,
}

impl Visitor<Infallible> for LabelCollector<'_> {
    type Value = ();

    fn visit_label_statement(&mut self, node: &LabelStatement) -> Result<(), Infallible> {
        if !self.labels.contains_key(&node.name) {
            let label = self.emitter.create_label();
            self.labels.insert(node.name.clone(), label);
        }
        Ok(())
    }
}

/// Compiles a `label:` statement: binds the label's position in the bytecode.
pub(crate) fn compile_label(
    emitter: &mut Emitter,
    ctx: &CompileContext,
    node: &LabelStatement,
) -> Result<(), Diagnostic> {
    if let Some(label) = ctx.jump_labels.get(&node.name) {
        emitter.bind_label(*label);
    }
    Ok(())
}

/// Compiles a `JMP [(condition)] label;` statement.
pub(crate) fn compile_jump(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    node: &Jump,
) -> Result<(), Diagnostic> {
    let target = ctx.jump_labels.get(&node.label).copied().ok_or_else(|| {
        Diagnostic::problem(
            Problem::JumpDestinationInvalid,
            Label::span(
                node.span(),
                format!("No label '{}' in this body", node.label.original()),
            ),
        )
    })?;

    match &node.condition {
        None => emitter.emit_jmp(target),
        Some(condition) => {
            // `JMP (cond) label` jumps only when the condition holds, so the
            // false path skips over the jump.
            let skip = emitter.create_label();
            let cond_type = condition_op_type(ctx, condition)?;
            compile_expr(emitter, ctx, condition, cond_type)?;
            emitter.emit_jmp_if_not(skip);
            emitter.emit_jmp(target);
            emitter.bind_label(skip);
        }
    }
    Ok(())
}

/// Compiles a `__WAIT [(condition)];` statement.
///
/// The reference lowers `__WAIT(cond)` to `WHILE NOT cond DO SynchWait();
/// END_WHILE`; this VM is synchronous, so the body is the empty statement and
/// the loop spins until the condition holds. The bare `__WAIT;` form has no
/// condition to wait for and is refused.
pub(crate) fn compile_wait(
    emitter: &mut Emitter,
    ctx: &mut CompileContext,
    node: &Wait,
) -> Result<(), Diagnostic> {
    let Some(condition) = &node.condition else {
        return Err(Diagnostic::not_implemented(Label::span(
            node.span(),
            "WAIT statement without a condition",
        )));
    };

    // ```text
    // LOOP:
    //   ...condition...
    //   JMP_IF_NOT LOOP      ; still not true: try again
    // END:                   ; true: go on with the next statement
    // ```
    let loop_label = emitter.create_label();

    emitter.bind_label(loop_label);
    let cond_type = condition_op_type(ctx, condition)?;
    compile_expr(emitter, ctx, condition, cond_type)?;
    emitter.emit_jmp_if_not(loop_label);

    Ok(())
}
