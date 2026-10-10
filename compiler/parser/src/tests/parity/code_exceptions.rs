//! The documented differences between the legacy diagnostic and the primary
//! diagnostic of the new parser (`diagnostics.rs` holds the comparison).

use super::diagnostics::{CodeException, Relation};
use super::{Class, Reason};

const MISSING_AT_PLACE: Reason = Reason::new(Class::AcceptedOnPurpose, "something that is missing (a closing keyword, a name, a statement) is reported where it belongs, as an empty range after the last token read or at the end of the input; the legacy parser reports the next token it could not read and, at the end of the input, the last one it did read");
const WHOLE_TOKEN: Reason = Reason::new(Class::AcceptedOnPurpose, "a pragma the dialect does not have, and a comment or pragma that is never closed, are one token of the new lexer and are reported over all of it; the legacy lexer has no token for them and reports their first byte (a brace or a parenthesis) as a token the grammar cannot use");
const STRING_RECOVERY: Reason = Reason::new(Class::AcceptedOnPurpose, "an unterminated string is an error token up to the end of its line in the new lexer, so a stray quote does not swallow the rest of the file; the legacy lexer reports everything from the quote to the end of the input");
const TEMPORAL_FIELD: Reason = Reason::new(Class::OwnerDecided, "owner decision on temporal literals: the diagnostic names the field that is wrong (hour, minute, month, second, a unit, the order of units, a fraction before the last part) and is positioned there; the legacy parser reports the token where its literal rule stopped, which is after the field");
const FIRST_OFFENDER: Reason = Reason::new(Class::AcceptedOnPurpose, "the new parser reports the first token that cannot belong where it is; the legacy parser reports the furthest token any alternative of its ordered choice read, which is one or more tokens later when an alternative read the offender as the start of something else");
const FALLBACK_POINT: Reason = Reason::new(Class::AcceptedOnPurpose, "the new parser reports the token that breaks the construct (the missing bound, the second occurrence, the separator after a list); the legacy parser reports the token where its ordered choice fell back, which is earlier, usually the start of the construct");
const VALUE_AND_TYPE: Reason = Reason::new(Class::AcceptedOnPurpose, "a value that is not an initial value of the type it follows is reported by the lowering as an initializer type mismatch (P4022) at the value; the legacy grammar can only fail with a syntax error where its choice for the type gave up");
const VALUE_BEFORE_SYNTAX: Reason = Reason::new(Class::AcceptedOnPurpose, "the legacy parser raises the out-of-range temporal literal while it parses, before it reaches a later syntax error, and reports the first; the new parser lowers only a tree that has no syntax error, so the syntax error is reported first (the literal is reported once the syntax is repaired)");
const PRAGMA_CONTENT: Reason = Reason::new(Class::OwnerDecided, "owner decision (pragma content is not examined): the legacy lexer reads the inside of a pragma and fails on a character it has no token for; the new lexer keeps the pragma as one token, so the problem it reports is the next one the grammar finds");

/// Differences between the diagnostics, against the legacy parser.
pub const CODE_EXCEPTIONS: &[CodeException] = &[
    CodeException {
        legacy_code: "P0002",
        new_code: "P0002",
        relation: Some(Relation::Empty),
        sites: &[
            "expected `_`",
            "expected a statement",
            "expected a name",
            "expected `_` after the assignment target",
            "expected a type declaration",
        ],
        reason: MISSING_AT_PLACE,
        expected: 378,
    },
    CodeException {
        legacy_code: "P0002",
        new_code: "P0002",
        relation: Some(Relation::Wider),
        sites: &[
            "pragmas are not enabled in this dialect",
            "unterminated block comment",
            "unterminated pragma",
        ],
        reason: WHOLE_TOKEN,
        expected: 194,
    },
    CodeException {
        legacy_code: "P0003",
        new_code: "P0003",
        relation: Some(Relation::Narrower),
        sites: &["unterminated string literal"],
        reason: STRING_RECOVERY,
        expected: 6,
    },
    CodeException {
        legacy_code: "P0002",
        new_code: "P0002",
        relation: None,
        sites: &[
            "the hour is out of range: '_'",
            "the minute is out of range: '_'",
            "the month is out of range: '_'",
            "expected a second: '_'",
            "expected a duration unit (d, h, m, s, ms, us, ns): '_'",
            "duration units must be in descending order: '_'",
            "only the last duration part may have a fraction: '_'",
        ],
        reason: TEMPORAL_FIELD,
        expected: 58,
    },
    CodeException {
        legacy_code: "P0002",
        new_code: "P0002",
        relation: Some(Relation::Earlier),
        sites: &[
            "a sign must touch the number after it",
            "expected a statement",
            "expected `_` after the assignment target",
            "expected a function block call",
            "expected `_`",
            "expected an expression",
            "expected a name",
            "this kind of type is not allowed here",
            "a value is part of a type declaration",
            "expected the path `_`",
            "`_` takes a duration",
        ],
        reason: FIRST_OFFENDER,
        expected: 300,
    },
    CodeException {
        legacy_code: "P0002",
        new_code: "P0002",
        relation: Some(Relation::Later),
        sites: &[
            "no whitespace is allowed inside a literal",
            "no whitespace is allowed inside a literal: '_'",
            "expected the upper bound of a range",
            "expected `_`",
            "an initial value is not allowed for this kind of type",
            "this variable block declares one variable at a time",
            "`_` comes too late here",
            "`_` may appear only once",
            "the resource comes too late here",
        ],
        reason: FALLBACK_POINT,
        expected: 73,
    },
    CodeException {
        legacy_code: "P0002",
        new_code: "P4022",
        relation: None,
        sites: &[
            "a value is not an initial value of an enumeration",
            "an array value is not an initial value of an elementary type",
            "a structure value is not an initial value of an elementary type",
            "a qualified enumeration value is not an initial value of an elementary type",
            "a structure value is not an initial value of an enumeration",
        ],
        reason: VALUE_AND_TYPE,
        expected: 30,
    },
    CodeException {
        legacy_code: "P2039",
        new_code: "P0002",
        relation: Some(Relation::Later),
        sites: &["expected an expression"],
        reason: VALUE_BEFORE_SYNTAX,
        expected: 6,
    },
    CodeException {
        legacy_code: "P0003",
        new_code: "P0002",
        relation: Some(Relation::Earlier),
        sites: &["pragmas are not enabled in this dialect", "expected a name"],
        reason: PRAGMA_CONTENT,
        expected: 8,
    },
];
