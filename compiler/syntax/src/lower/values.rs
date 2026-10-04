//! Values that name an integer or an enumerator, read the same wherever they
//! stand: the bound of a range, the length or count in a type, a qualified
//! enumeration value.
//!
//! A `CASE` label, an array bound, a string length and the value of an
//! enumeration member write these with the same nodes, so one rule reads each
//! and the statements and the type declarations both call it.

use super::literals::lower_constant;
use super::names::lower_name;
use super::LowerCx;
use crate::syntax_kind::{SyntaxKind as K, SyntaxNode};
use ironplc_dsl::common::{
    ConstantKind, EnumeratedValue, IntegerRef, SignedInteger, SignedIntegerRef, Subrange, TypeName,
};
use ironplc_dsl::diagnostic::Diagnostic;

/// The integer a number node spells, with its sign: the node's own range is
/// where the integer is positioned, so a sign that touches the digits is
/// part of it.
pub fn lower_integer(cx: &LowerCx, node: &SyntaxNode) -> Result<SignedInteger, Diagnostic> {
    match lower_constant(cx, node)? {
        ConstantKind::IntegerLiteral(literal) => Ok(literal.value),
        _ => Err(cx.missing(node, "an integer")),
    }
}

/// A count or a length: an integer, or the name of a constant.
pub fn lower_integer_ref(cx: &LowerCx, node: &SyntaxNode) -> Result<IntegerRef, Diagnostic> {
    match node.kind() {
        K::IntLiteral => lower_integer(cx, node).map(|number| IntegerRef::Literal(number.value)),
        K::NameRef => lower_name(cx, node).map(IntegerRef::Constant),
        _ => Err(cx.unsupported(node)),
    }
}

/// The bound of a range: a signed integer, or the name of a constant.
pub fn lower_signed_integer_ref(
    cx: &LowerCx,
    node: &SyntaxNode,
) -> Result<SignedIntegerRef, Diagnostic> {
    match node.kind() {
        K::IntLiteral => lower_integer(cx, node).map(SignedIntegerRef::Literal),
        K::NameRef => lower_name(cx, node).map(SignedIntegerRef::Constant),
        _ => Err(cx.unsupported(node)),
    }
}

/// The two nodes a node of two parts is made of, as the grammar gives them.
fn two_parts(cx: &LowerCx, node: &SyntaxNode) -> Result<(SyntaxNode, SyntaxNode), Diagnostic> {
    let mut nodes = node.children();
    match (nodes.next(), nodes.next()) {
        (Some(first), Some(second)) => Ok((first, second)),
        _ => Err(cx.missing(node, "two parts")),
    }
}

/// `low .. high`: a range written as a node of its two bounds, which is how
/// a type spells it and how a `CASE` label does.
pub fn lower_subrange(cx: &LowerCx, node: &SyntaxNode) -> Result<Subrange, Diagnostic> {
    let (low, high) = two_parts(cx, node)?;
    Ok(Subrange {
        start: lower_signed_integer_ref(cx, &low)?,
        end: lower_signed_integer_ref(cx, &high)?,
    })
}

/// `Type#Value`: an enumeration value qualified by its type, written as a node
/// of the two names.
pub fn lower_qualified_value(
    cx: &LowerCx,
    node: &SyntaxNode,
) -> Result<EnumeratedValue, Diagnostic> {
    let (type_name, value) = two_parts(cx, node)?;
    Ok(EnumeratedValue {
        type_name: Some(TypeName {
            name: lower_name(cx, &type_name)?,
        }),
        value: lower_name(cx, &value)?,
        explicit_value: None,
    })
}

/// `Value`: an enumeration value named without its type.
pub fn lower_bare_value(cx: &LowerCx, node: &SyntaxNode) -> Result<EnumeratedValue, Diagnostic> {
    Ok(EnumeratedValue {
        type_name: None,
        value: lower_name(cx, node)?,
        explicit_value: None,
    })
}

/// An enumeration value as an initial value writes it: qualified by its type
/// (an [`K::EnumValueRef`]) or a name alone.
pub fn lower_enumerated_value(
    cx: &LowerCx,
    node: &SyntaxNode,
) -> Result<EnumeratedValue, Diagnostic> {
    match node.kind() {
        K::EnumValueRef => lower_qualified_value(cx, node),
        K::NameRef => lower_bare_value(cx, node),
        _ => Err(cx.unsupported(node)),
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::{parse_statements, ParseOptions};
    use ironplc_dsl::core::FileId;

    fn case_labels(source: &str) -> Vec<SyntaxNode> {
        parse_statements(source, &ParseOptions::all())
            .root
            .descendants()
            .filter(|node| node.kind() == K::CaseLabel)
            .collect()
    }

    fn cx() -> LowerCx {
        LowerCx::new(FileId::default())
    }

    #[test]
    fn lower_subrange_when_case_range_then_both_bounds_as_numbers_or_constant_names() {
        let labels = case_labels("CASE x OF 1..n: ; -3..+4: ; END_CASE;");
        let ranges: Vec<Subrange> = labels
            .iter()
            .filter_map(|label| lower_subrange(&cx(), label).ok())
            .collect();
        assert_eq!(ranges.len(), 2);
        assert!(matches!(ranges[0].end, SignedIntegerRef::Constant(_)));
        assert!(matches!(&ranges[1].start, SignedIntegerRef::Literal(n) if n.is_neg));
    }

    #[test]
    fn lower_qualified_value_when_type_and_value_then_both_names() {
        let labels = case_labels("CASE x OF Color#Red: ; END_CASE;");
        let value = labels
            .first()
            .and_then(|label| lower_qualified_value(&cx(), label).ok());
        assert_eq!(
            value.map(|value| (
                value.type_name.map(|t| t.to_string()),
                value.value.to_string()
            )),
            Some((Some("Color".to_string()), "Red".to_string()))
        );
    }

    #[test]
    fn lower_integer_ref_when_neither_a_number_nor_a_name_then_error() {
        let parse = parse_statements("x := 1.5;", &ParseOptions::all());
        let real = parse
            .root
            .descendants()
            .find(|node| node.kind() == K::RealLiteral)
            .expect("a real literal");
        assert!(lower_integer_ref(&cx(), &real).is_err());
        assert!(lower_signed_integer_ref(&cx(), &real).is_err());
        assert!(lower_enumerated_value(&cx(), &real).is_err());
        assert!(lower_integer(&cx(), &real).is_err());
    }

    #[test]
    fn two_parts_when_node_has_fewer_then_internal_error() {
        let green =
            rowan::GreenNode::new(rowan::SyntaxKind(K::Subrange as u16), std::iter::empty());
        let node = SyntaxNode::new_root(green);
        assert!(lower_subrange(&cx(), &node).is_err());
        assert!(lower_qualified_value(&cx(), &node).is_err());
    }

    #[test]
    fn lower_bare_value_when_name_then_value_without_a_type() {
        let parse = parse_statements("CASE x OF Red: ; END_CASE;", &ParseOptions::all());
        let name = parse
            .root
            .descendants()
            .find(|node| node.kind() == K::NameRef && node.text() == "Red")
            .expect("a name");
        let value = lower_enumerated_value(&cx(), &name).expect("a value");
        assert!(value.type_name.is_none());
        assert_eq!(value.value.original(), "Red");
    }
}
