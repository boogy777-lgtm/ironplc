//! Bytecode-level integration tests for arrays of function block instances.

use ironplc_container::{opcode, ArrayDescriptor, FieldType};
use ironplc_parser::options::CompilerOptions;

use crate::common::{bc, parse_and_compile};

fn fb_arrays() -> CompilerOptions {
    CompilerOptions {
        allow_fb_instance_arrays: true,
        ..CompilerOptions::default()
    }
}

// ts=var:0. Six slots per TON, three elements: eighteen slots, in one
// descriptor that names the block, so that a second container can tell whether
// the same slots mean the same thing.
#[test]
fn compile_when_array_of_timers_then_one_descriptor_spans_the_slots_of_every_instance() {
    let source = "
PROGRAM main
  VAR
    ts : ARRAY[0..2] OF TON;
  END_VAR
  ts[1](IN := TRUE, PT := T#1s);
END_PROGRAM
";
    let container = parse_and_compile(source, &fb_arrays());

    let descriptors = &container.type_section.as_ref().unwrap().array_descriptors;
    assert_eq!(
        descriptors,
        &vec![ArrayDescriptor::new(
            FieldType::FbInstance as u8,
            18,
            opcode::fb_type::TON
        )]
    );
}

// The reference to the instance is computed from the index, and the index is
// checked by loading the instance's first slot before the address is used, so
// that an index outside the array traps instead of addressing a neighbour.
//   flat slot index (element 1 of 6-slot instances) = 6
#[test]
fn compile_when_element_called_then_index_is_checked_before_the_address_is_used() {
    let source = "
PROGRAM main
  VAR
    ts : ARRAY[0..2] OF TON;
  END_VAR
  ts[1](IN := TRUE, PT := T#1s);
END_PROGRAM
";
    let container = parse_and_compile(source, &fb_arrays());

    let bytecode = container
        .code
        .get_function_bytecode(ironplc_container::FunctionId::new(1))
        .unwrap();
    let expected_prefix = [
        bc::load_const_i32(1), // pool:1 = flat index 6 (pool:0 is the data offset the init stores)
        bc::dup(),
        bc::load_array(0, 0), // bounds check against the whole array
        bc::pop(),
        bc::load_const_i64(2), // pool:2 = bytes per slot
        bc::mul_i64(),
        bc::fb_load_instance(0), // offset of the first instance
        bc::add_i64(),
    ]
    .concat();
    assert_eq!(
        &bytecode[..expected_prefix.len()],
        expected_prefix.as_slice()
    );
}
