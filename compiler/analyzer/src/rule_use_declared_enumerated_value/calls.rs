//! The inputs of a function block and of a method.
//!
//! An input is checked at the position of the input it binds to
//! ([`Position::CallInput`]), a named argument by its name and a positional
//! one by its place among the inputs, as every other check of a call binds
//! them (`call_assignment_check::bind_inputs`).

use ironplc_dsl::{
    common::HasVariables,
    textual::{FbCall, MethodCall, MethodReceiver, ParamAssignmentKind},
};

use super::position::Position;
use super::RuleDeclaredEnumeratedValues;
use crate::{call_assignment_check::bind_inputs, type_attributes::PartType};

impl RuleDeclaredEnumeratedValues<'_> {
    /// Checks the arguments of a call against the inputs the callee declares.
    pub(super) fn check_inputs(
        &mut self,
        callee: &dyn HasVariables,
        params: &[ParamAssignmentKind],
    ) {
        for (param, input) in bind_inputs(callee, params) {
            if let (Some(argument), Some(input)) = (param.input_expr(), input) {
                self.check_expression_at(Position::CallInput(input), &argument.kind);
            }
        }
    }

    /// Checks the arguments of the invocation of a function block instance.
    pub(super) fn check_fb_call(&mut self, call: &FbCall) {
        let types = self.context.types();
        let Some(PartType::Of(instance)) = self.variable_part(&call.callee) else {
            return;
        };
        let Some(name) = types.name_of(instance) else {
            return;
        };
        if let Some(block) = self.function_blocks.get(name) {
            self.check_inputs(block, &call.params);
        }
    }

    /// Checks the arguments of the call of a method of a function block
    /// instance.
    pub(super) fn check_method_call(&mut self, call: &MethodCall) {
        let MethodReceiver::Instance(receiver) = &call.receiver else {
            // `THIS^` and `SUPER^` are not resolved by any rule yet.
            return;
        };
        let types = self.context.types();
        let Some(instance) = self
            .declarations
            .find(receiver)
            .and_then(|declared| declared.type_id(types))
        else {
            return;
        };
        let Some(name) = types.name_of(instance) else {
            return;
        };
        if let Some((_, method)) = self.function_blocks.resolve_method(name, &call.method) {
            self.check_inputs(method, &call.params);
        }
    }
}
