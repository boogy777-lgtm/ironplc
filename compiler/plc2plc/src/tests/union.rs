//! `UNION` and the additional variable sections (VAR_STAT / VAR_INST /
//! VAR_GENERIC) round-tripping. See specs/design/st-declaration-extensions.md.

use super::common::*;
use rstest::rstest;
use spec_test_macro::spec_test;

fn union_options() -> CompilerOptions {
    CompilerOptions {
        allow_union_type: true,
        ..CompilerOptions::default()
    }
}

fn var_sections_options() -> CompilerOptions {
    CompilerOptions {
        allow_var_stat: true,
        allow_var_inst: true,
        allow_var_generic: true,
        allow_fb_inheritance: true,
        ..CompilerOptions::default()
    }
}

/// REQ-STX-plc2plc-020: a union declaration and the VAR_STAT/VAR_INST/
/// VAR_GENERIC sections render back to text that re-parses to the same AST.
#[spec_test(REQ_STX_plc2plc_020)]
#[rstest]
#[case::union("union.st", union_options)]
#[case::var_sections("var_sections.st", var_sections_options)]
fn plc2plc_spec_req_stx_020_union_and_var_sections_round_trip(
    #[case] resource: &'static str,
    #[case] options: fn() -> CompilerOptions,
) {
    let source = read_shared_resource(resource);
    assert_round_trips(&source, &options());
}
