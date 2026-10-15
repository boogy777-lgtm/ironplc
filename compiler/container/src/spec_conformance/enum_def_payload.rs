//! Conformance test for the ENUM_DEF payload of the debug section, an enumeration requirement owned by the container.

use std::vec;
use std::vec::Vec;

use spec_test_macro::spec_test;

// ---------------------------------------------------------------------------
// Enumeration debug section — ENUM_DEF payload (cross-crate: this requirement
// is defined in enumeration-codegen.md but owned here because the on-disk
// ENUM_DEF format is a container concern. codegen owns the rest of that doc.)
// ---------------------------------------------------------------------------

/// REQ-EN-container-061: ENUM_DEF sub-table roundtrips through write/read.
#[spec_test(REQ_EN_container_061)]
fn container_spec_req_en_061_enum_def_payload_roundtrips() {
    use crate::debug_section::{DebugSection, EnumDefEntry};
    use std::io::Cursor;

    let section = DebugSection {
        var_names: vec![],
        func_names: vec![],
        line_map: vec![],
        string_layouts: vec![],
        source_files: vec![],
        enum_defs: vec![EnumDefEntry {
            type_name: "COLOR".into(),
            values: vec!["RED".into(), "GREEN".into(), "BLUE".into()],
        }],
    };
    let mut buf = Vec::new();
    section.write_to(&mut buf).unwrap();

    let decoded = DebugSection::read_from(&mut Cursor::new(&buf)).unwrap();
    assert_eq!(decoded.enum_defs.len(), 1);
    assert_eq!(decoded.enum_defs[0].type_name, "COLOR");
    assert_eq!(decoded.enum_defs[0].values, vec!["RED", "GREEN", "BLUE"]);
}
