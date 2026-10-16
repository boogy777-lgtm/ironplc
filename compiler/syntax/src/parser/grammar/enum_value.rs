//! An enumeration value written with its type: `Color#Red`.
//!
//! IEC 61131-3 writes `enumerated_value ::= [enumerated_type_name '#']
//! identifier`, and the value stands wherever a value may: an initial value, an
//! element of an array or a structure value, a `CASE` label, the source of a
//! program connection, and an operand of an expression (an assignment, a
//! comparison, an argument of a call). This is the one function that reads the
//! qualified spelling, and each of those places calls it; a place that takes a
//! value does not read the `#` itself.
//!
//! A typed literal (`T#5s`, `INT#5`, `BOOL#TRUE`) also has a name or a keyword
//! before the `#`, and the abbreviations `T`, `D` and `LT` are ordinary names.
//! What follows the `#` tells them apart and nothing else does: a name after
//! it is an enumeration value, and a number, a sign or a string is a literal.
//! A caller therefore asks for the qualified value before it asks for a
//! literal. The pieces must touch, as they do in every literal.

use super::expressions::name_ref;
use crate::parser::event::CompletedMarker;
use crate::parser::state::Parser;
use crate::syntax_kind::SyntaxKind as K;

/// `Type#Value`, when one starts at the cursor; `None`, consuming nothing,
/// when it does not.
pub(super) fn qualified_value(p: &mut Parser) -> Option<CompletedMarker> {
    if !qualified_value_ahead(p) {
        return None;
    }
    let node = p.start();
    name_ref(p);
    p.bump();
    name_ref(p);
    Some(p.complete(node, K::EnumValueRef))
}

/// A name, a touching `#`, and a touching name.
fn qualified_value_ahead(p: &Parser) -> bool {
    p.name_at(0) && p.nth_at(1, K::Hash) && p.adjacent(0) && p.adjacent(1) && p.variable_name_at(2)
}
