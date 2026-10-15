//! Tests for the intermediate type representation, grouped by the question they answer.

use crate::intermediate_type::{
    ArrayDimension, ByteSized, FunctionBlockVarType, IntermediateStructField, IntermediateType,
    SlotCountError,
};
use crate::intermediates::layout::AlignmentBound;
use ironplc_container::{CharWidth, STRING_HEADER_BYTES};
use ironplc_dsl::core::Id;

mod field_access;
mod function_block_layout;
mod size_and_alignment;
mod slot_count;
mod structure_layout;
mod validate_bounds;
