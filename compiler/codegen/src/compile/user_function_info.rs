//! Compilation metadata for user-defined functions: parameter passing and string returns.

use ironplc_container::{CharWidth, FunctionId, VarIndex};

use super::OpType;

#[derive(Clone)]
pub(crate) struct StringParamInfo {
    /// Byte offset in the data region where this parameter's string is stored.
    pub(crate) data_offset: u32,
    /// Maximum number of code units this parameter can hold.
    pub(crate) max_length: u16,
    /// Per-code-unit byte width: `Narrow` for STRING, `Wide` for WSTRING.
    pub(crate) char_width: CharWidth,
}

/// How a call site passes an argument to one input parameter
/// (`VAR_INPUT`/`VAR_IN_OUT`) of a user-defined function.
#[derive(Clone)]
pub(crate) enum ParamPassing {
    /// The argument's value is pushed onto the operand stack at this type,
    /// and `CALL` stores it in the parameter's slot.
    Value(OpType),
    /// The argument is copied into the parameter's data region space before
    /// `CALL`, and a dummy value is pushed for the slot `CALL` pops.
    String(StringParamInfo),
    /// A `VAR_IN_OUT` argument: the variable's reference (its
    /// variable-table index) is pushed, and the function reads and writes
    /// the caller's variable through it.
    Reference,
}

/// Metadata for a STRING/WSTRING return value in a user-defined function.
///
/// When a function returns STRING/WSTRING, the return value lives in the
/// data region rather than on the operand stack.
#[derive(Clone)]
pub(crate) struct StringReturnInfo {
    /// Byte offset in the data region where the return string is stored.
    pub(crate) data_offset: u32,
    /// Maximum number of code units the return string can hold.
    pub(crate) max_length: u16,
    /// Per-code-unit byte width: `Narrow` for STRING, `Wide` for WSTRING.
    pub(crate) char_width: CharWidth,
}

/// Metadata for a compiled user-defined function.
#[derive(Clone)]
pub(crate) struct UserFunctionInfo {
    /// The function ID assigned in the container.
    pub(crate) function_id: FunctionId,
    /// The absolute variable table offset where this function's parameters start.
    pub(crate) var_offset: VarIndex,
    /// Number of input parameters.
    pub(crate) num_params: u16,
    /// How the call site passes each input parameter, in declaration order.
    pub(crate) params: Vec<ParamPassing>,
    /// If the function returns STRING/WSTRING, info about the return string
    /// in the data region. Used at call sites to initialize the return string
    /// header before CALL.
    pub(crate) return_string_info: Option<StringReturnInfo>,
    /// If the function returns a structure, the array descriptor index of its
    /// return variable's data region. A call site assigning the result to a
    /// whole variable (`s := f()`) needs it to emit `COPY_REGION`, whose
    /// source size comes from the descriptor rather than an operand.
    pub(crate) return_struct_desc_index: Option<u16>,
    /// Maximum stack depth used by this function's body.
    pub(crate) max_stack_depth: u16,
}
