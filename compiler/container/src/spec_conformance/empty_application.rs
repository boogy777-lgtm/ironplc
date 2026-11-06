//! Conformance tests for the empty application: the container of a device that
//! holds no program.

use std::io::Cursor;
use std::vec;

use spec_test_macro::spec_test;

use crate::{
    empty_application, empty_application_bytes, integrity, verify_load, Container, ContainerRef,
    EMPTY_APPLICATION_HASH,
};

// ---------------------------------------------------------------------------
// Container Format — The Empty Application (REQ-CF-container-045 through
// REQ-CF-container-046)
// ---------------------------------------------------------------------------

/// REQ-CF-container-045: The empty application has no task, no program
/// instance, no variable, no function and no constant, and passes the load
/// verification that every container passes.
#[spec_test(REQ_CF_container_045)]
fn container_spec_req_cf_045_empty_application_has_no_program_and_no_task_and_verifies() {
    let container = empty_application().unwrap();

    assert!(container.task_table.tasks.is_empty());
    assert!(container.task_table.programs.is_empty());
    assert_eq!(container.header.num_variables, 0);
    assert_eq!(container.header.num_functions, 0);
    assert!(container.type_section.is_none());
    assert!(container.constant_pool.is_empty());
    assert!(container.code.functions.is_empty());
    assert!(verify_load(&container).is_ok());

    let bytes = empty_application_bytes().unwrap();
    let mut offsets = vec![0u32; 4];
    assert!(ContainerRef::from_slice(&bytes, &mut offsets).is_ok());
}

/// REQ-CF-container-046: The content hash of the empty application is
/// `EMPTY_APPLICATION_HASH`, and it is the digest the hash scope defines, not
/// a value that was only written.
#[spec_test(REQ_CF_container_046)]
fn container_spec_req_cf_046_empty_application_content_hash_is_the_constant() {
    let bytes = empty_application_bytes().unwrap();
    let container = Container::read_from(&mut Cursor::new(&bytes)).unwrap();

    assert_eq!(
        container.header.content_hash,
        EMPTY_APPLICATION_HASH,
        "the format changed: update EMPTY_APPLICATION_HASH to {}",
        integrity::hash_hex(&container.header.content_hash)
    );
    assert_eq!(bytes, empty_application_bytes().unwrap());
}
