//! Time literals with a sub-millisecond part and the abbreviated prefixes.
//! See `specs/design/time-literals.md`.

use super::common::*;

/// The rendering keeps the value of every duration, so a literal with a
/// sub-millisecond part is written in a unit that holds it.
#[test]
fn write_to_string_when_time_literals_then_value_is_not_truncated() {
    let source = read_shared_resource("time_literal_extensions.st");
    let options = CompilerOptions::from_dialect(Dialect::Codesys);
    let rendered = assert_round_trips(&source, &options);

    assert!(rendered.contains("TIME#1us"), "rendered:\n{rendered}");
    assert!(rendered.contains("TIME#500ns"), "rendered:\n{rendered}");
    assert!(rendered.contains("LTIME#1500us"), "rendered:\n{rendered}");
    // A fractional millisecond is not a whole number of milliseconds
    // either; it is written as microseconds.
    assert!(rendered.contains("TIME#1500us"), "rendered:\n{rendered}");
    // The abbreviated prefixes render as the type they name.
    assert!(rendered.contains("LDATE#2024-01-20"), "rendered:\n{rendered}");
    assert!(
        rendered.contains("TIME_OF_DAY#10:00:00"),
        "rendered:\n{rendered}"
    );
}
