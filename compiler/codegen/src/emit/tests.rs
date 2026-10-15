//! Tests for the bytecode emitter.

use ironplc_container::{FunctionId, VarIndex};

use super::*;

mod arithmetic;
mod array;
mod boolean;
mod builtin;
mod call;
mod comparison;
mod construction;
mod jump;
mod line_map;
mod stack_manipulation;
mod temp_depth;
