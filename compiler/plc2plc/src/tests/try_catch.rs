//! CODESYS exception handling: `__TRY` / `__CATCH` / `__FINALLY` /
//! `__ENDTRY` / `__THROW`.
//!
//! See `specs/design/codesys-try-catch.md`.

use super::common::*;
use spec_test_macro::spec_test;

/// REQ-TC-plc2plc-001: the rendered `__TRY .. __ENDTRY` spelling re-parses to
/// the same AST. The shared resource covers every clause shape: no handler,
/// `__CATCH (e)`, `__CATCH ()`, a bare `__CATCH`, `__FINALLY` alone and the
/// two clauses together, plus `__THROW(value)` and bare `__THROW`.
#[spec_test(REQ_TC_plc2plc_001)]
fn write_to_string_when_try_catch_resource_then_round_trips() {
    let source = read_shared_resource("try_catch.st");
    let options = CompilerOptions::from_dialect(Dialect::Codesys);

    // The round trip already proves the keywords: a rendering that dropped a
    // leading `__` spells an identifier, and the re-parse would fail.
    assert_round_trips(&source, &options);
}
