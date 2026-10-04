//! Initial values: what follows the `:=` of a declaration.
//!
//! The values themselves are small objects with a rule each: the elements of
//! an array initializer (`lower_array_elements`, with the repeated element
//! `3(0)`), the members of a structure initializer (`lower_struct_elements`)
//! and the enumeration value (`values`). What a declaration's initial value
//! makes of the declaration, though, depends on its type: `x : INT := 5` is a
//! simple value, `x : T := 5` is a value of a type that only a later stage can
//! classify, `x : T := Red` may be an enumeration value or the name of a
//! constant. That decision is the table `INITIAL_VALUES`: one row for each pair
//! of the form of the type and the kind of the value (`declarations`), the rule
//! of the row builds the object the legacy grammar built, and a pair no row
//! names is a value that does not fit its type.
//!
//! A value written against a name the grammar cannot classify is recorded as
//! written (`late_resolved_members`, `late_resolved_or_enumerated`): the type
//! resolver decides, once the declarations are known.

#[cfg(test)]
mod tests;

use super::declarations::{build, row, Form, Init, Parts, Row};
use super::expressions::{lower_arguments, lower_expr};
use super::literals::lower_constant;
use super::names::lower_name;
use super::tree::{child_of, children_of, significant_tokens};
use super::types::{
    lower_array, lower_enumeration, lower_params, lower_reference, lower_string_specification,
    lower_subrange_specification,
};
use super::values::{lower_enumerated_value, lower_integer};
use super::variables::lower_variable;
use super::{disposition, Area, Disposition, LowerCx};
use crate::syntax_kind::{SyntaxKind as K, SyntaxNode};
use ironplc_dsl::common::{
    ArrayInitialElementKind, ArrayInitialValueAssignment, CharacterStringLiteral, ConstantKind,
    EnumeratedValuesInitializer, FunctionBlockCallInitializer, InitialValueAssignmentKind,
    LateResolvedInitializer, ReferenceInitialValue, ReferenceInitializer, SignedInteger,
    SimpleInitializer, SpecificationKind, StringInitializer, StringType,
    StructInitialValueAssignmentKind, StructureElementInit, StructureInitializationDeclaration,
};
use ironplc_dsl::construct::{
    late_resolved_members, late_resolved_or_enumerated, literal_value_of, resolve_initializer_expr,
};
use ironplc_dsl::diagnostic::Diagnostic;
use ironplc_dsl::textual::LateBound;

/// True for a node that writes a literal.
fn is_literal(node: &SyntaxNode) -> bool {
    disposition(node.kind()) == Disposition::Lowered(Area::Literal)
}

/// The constant an initial value is, for a position that takes a constant: a
/// literal, or a literal with a sign. Anything else is not a value of the
/// type.
pub fn lower_constant_value(
    cx: &LowerCx,
    parts: &Parts,
    value: &SyntaxNode,
) -> Result<ConstantKind, Diagnostic> {
    let expression = lower_expr(cx, value)?;
    literal_value_of(&expression).ok_or_else(|| parts.mismatch(cx))
}

/// The integer an initial value is, for a position that takes an integer: a
/// subrange's default. It is written in digits, with a sign if it has one; a
/// typed or based number, or any other value, is not a value of the type.
pub fn lower_integer_value(
    cx: &LowerCx,
    parts: &Parts,
    value: &SyntaxNode,
) -> Result<SignedInteger, Diagnostic> {
    let tokens = significant_tokens(value);
    let in_digits = match tokens.as_slice() {
        [digits] => digits.kind() == K::IntegerLit,
        [sign, digits] => {
            matches!(sign.kind(), K::Plus | K::Minus) && digits.kind() == K::IntegerLit
        }
        _ => false,
    };
    if value.kind() == K::IntLiteral && in_digits {
        lower_integer(cx, value)
    } else {
        Err(parts.mismatch(cx))
    }
}

/// An element of an array initializer, as the rule that builds it.
type ElementRule = fn(&LowerCx, &SyntaxNode) -> Result<ArrayInitialElementKind, Diagnostic>;

/// The rule for an element, by the kind of the node that writes it: a
/// repeated element, an enumeration value (qualified or a name), or a
/// constant.
fn element_rule(node: &SyntaxNode) -> Option<ElementRule> {
    Some(match node.kind() {
        K::RepeatedInit => repeated_element,
        K::EnumValueRef | K::NameRef => enumerated_element,
        _ if is_literal(node) => constant_element,
        _ => return None,
    })
}

fn constant_element(
    cx: &LowerCx,
    node: &SyntaxNode,
) -> Result<ArrayInitialElementKind, Diagnostic> {
    lower_constant(cx, node).map(ArrayInitialElementKind::Constant)
}

fn enumerated_element(
    cx: &LowerCx,
    node: &SyntaxNode,
) -> Result<ArrayInitialElementKind, Diagnostic> {
    lower_enumerated_value(cx, node).map(ArrayInitialElementKind::EnumValue)
}

/// `count ( [element] )`: the element repeated `count` times.
fn repeated_element(
    cx: &LowerCx,
    node: &SyntaxNode,
) -> Result<ArrayInitialElementKind, Diagnostic> {
    let mut parts = node.children();
    let count = parts.next().ok_or_else(|| cx.missing(node, "a count"))?;
    let element = parts.next().map(|element| lower_element(cx, &element));
    Ok(ArrayInitialElementKind::repeated(
        lower_integer(cx, &count)?.value,
        element.transpose()?,
    ))
}

fn lower_element(cx: &LowerCx, node: &SyntaxNode) -> Result<ArrayInitialElementKind, Diagnostic> {
    let rule = element_rule(node).ok_or_else(|| cx.unsupported(node))?;
    rule(cx, node)
}

/// Lowers `[element, ...]`: the elements of an array initializer, in order.
pub fn lower_array_elements(
    cx: &LowerCx,
    node: &SyntaxNode,
) -> Result<Vec<ArrayInitialElementKind>, Diagnostic> {
    if node.kind() != K::ArrayInit {
        return Err(cx.unsupported(node));
    }
    node.children()
        .map(|element| lower_element(cx, &element))
        .collect()
}

/// A value of a structure member, as the rule that builds it.
type MemberRule = fn(&LowerCx, &SyntaxNode) -> Result<StructInitialValueAssignmentKind, Diagnostic>;

/// The rule for the value of a structure member, by the kind of the node that
/// writes it. A literal, or a literal with a sign, is a constant; a name alone
/// is late-bound, because nothing says whether it is an enumeration value or a
/// variable; any other value is an expression.
fn member_rule(node: &SyntaxNode) -> MemberRule {
    match node.kind() {
        K::ArrayInit => member_array,
        K::StructInit => member_structure,
        K::EnumValueRef => member_enumerated,
        K::NameRef => member_late_bound,
        K::UnaryExpr => member_constant_or_expression,
        _ if is_literal(node) => member_constant_or_expression,
        _ => member_expression,
    }
}

fn member_array(
    cx: &LowerCx,
    node: &SyntaxNode,
) -> Result<StructInitialValueAssignmentKind, Diagnostic> {
    lower_array_elements(cx, node).map(StructInitialValueAssignmentKind::Array)
}

fn member_structure(
    cx: &LowerCx,
    node: &SyntaxNode,
) -> Result<StructInitialValueAssignmentKind, Diagnostic> {
    lower_struct_elements(cx, node).map(StructInitialValueAssignmentKind::Structure)
}

fn member_enumerated(
    cx: &LowerCx,
    node: &SyntaxNode,
) -> Result<StructInitialValueAssignmentKind, Diagnostic> {
    lower_enumerated_value(cx, node).map(StructInitialValueAssignmentKind::EnumeratedValue)
}

fn member_late_bound(
    cx: &LowerCx,
    node: &SyntaxNode,
) -> Result<StructInitialValueAssignmentKind, Diagnostic> {
    lower_name(cx, node)
        .map(|value| StructInitialValueAssignmentKind::LateBound(LateBound { value }))
}

fn member_expression(
    cx: &LowerCx,
    node: &SyntaxNode,
) -> Result<StructInitialValueAssignmentKind, Diagnostic> {
    lower_expr(cx, node).map(StructInitialValueAssignmentKind::Expression)
}

fn member_constant_or_expression(
    cx: &LowerCx,
    node: &SyntaxNode,
) -> Result<StructInitialValueAssignmentKind, Diagnostic> {
    let expression = lower_expr(cx, node)?;
    Ok(match literal_value_of(&expression) {
        Some(constant) => StructInitialValueAssignmentKind::Constant(constant),
        None => StructInitialValueAssignmentKind::Expression(expression),
    })
}

/// `member := value`.
fn lower_struct_element(
    cx: &LowerCx,
    node: &SyntaxNode,
) -> Result<StructureElementInit, Diagnostic> {
    let mut parts = node.children();
    let name = parts
        .next()
        .ok_or_else(|| cx.missing(node, "a member name"))?;
    let value = parts.next().ok_or_else(|| cx.missing(node, "a value"))?;
    Ok(StructureElementInit {
        name: lower_name(cx, &name)?,
        init: member_rule(&value)(cx, &value)?,
    })
}

/// Lowers `(member := value, ...)`: the members of a structure initializer, in
/// order. A value that is itself a structure initializer nests.
pub fn lower_struct_elements(
    cx: &LowerCx,
    node: &SyntaxNode,
) -> Result<Vec<StructureElementInit>, Diagnostic> {
    if node.kind() != K::StructInit {
        return Err(cx.unsupported(node));
    }
    children_of(node, K::StructInitElement)
        .map(|element| lower_struct_element(cx, &element))
        .collect()
}

/// The elements of the array initializer a declaration has, none when it has
/// no value.
fn array_values(cx: &LowerCx, parts: &Parts) -> Result<Vec<ArrayInitialElementKind>, Diagnostic> {
    match &parts.value {
        Some(elements) => lower_array_elements(cx, elements),
        None => Ok(vec![]),
    }
}

/// `x : ARRAY [ranges] OF T [:= [elements]]`
fn array(cx: &LowerCx, parts: &Parts) -> Result<InitialValueAssignmentKind, Diagnostic> {
    Ok(InitialValueAssignmentKind::Array(
        ArrayInitialValueAssignment {
            spec: SpecificationKind::Inline(lower_array(cx, &parts.spec)?),
            initial_values: array_values(cx, parts)?,
        },
    ))
}

/// `x : ArrayType := [elements]`: the array is a named type, and the value is
/// that of an array.
fn named_array(cx: &LowerCx, parts: &Parts) -> Result<InitialValueAssignmentKind, Diagnostic> {
    Ok(InitialValueAssignmentKind::Array(
        ArrayInitialValueAssignment {
            spec: SpecificationKind::Named(parts.base_name(cx)?),
            initial_values: array_values(cx, parts)?,
        },
    ))
}

/// The value of a string, which takes the width the declaration states
/// whichever delimiter it was written with: `x : STRING[10] := "ab"` is a
/// value of a string of single-byte characters. Whether its characters fit
/// that width is for the analysis of the literal to say.
pub fn lower_string_value(
    cx: &LowerCx,
    parts: &Parts,
    width: &StringType,
) -> Result<Option<CharacterStringLiteral>, Diagnostic> {
    parts
        .value
        .as_ref()
        .map(|value| {
            let literal = is_literal(value)
                .then(|| lower_constant(cx, value))
                .transpose()?;
            match literal {
                Some(ConstantKind::CharacterString(literal)) => Ok(CharacterStringLiteral {
                    width: width.clone(),
                    ..literal
                }),
                _ => Err(parts.mismatch(cx)),
            }
        })
        .transpose()
}

/// `x : STRING[n] [:= 'value']`
fn string(cx: &LowerCx, parts: &Parts) -> Result<InitialValueAssignmentKind, Diagnostic> {
    let specification = lower_string_specification(cx, &parts.spec)?;
    Ok(InitialValueAssignmentKind::String(StringInitializer {
        initial_value: lower_string_value(cx, parts, &specification.width)?,
        length: specification.length,
        width: specification.width,
        keyword_span: specification.keyword_span,
    }))
}

/// `x : REF_TO T [:= NULL | REF(variable)]`
fn reference(cx: &LowerCx, parts: &Parts) -> Result<InitialValueAssignmentKind, Diagnostic> {
    let (target, syntax) = lower_reference(cx, &parts.spec)?;
    let initial_value = parts
        .value
        .as_ref()
        .map(|value| match value.kind() {
            K::NullLiteral => Ok(ReferenceInitialValue::Null(cx.node_span(value))),
            K::RefExpr => {
                let variable = value
                    .first_child()
                    .ok_or_else(|| cx.missing(value, "a variable"))?;
                lower_variable(cx, &variable).map(ReferenceInitialValue::Ref)
            }
            _ => Err(parts.mismatch(cx)),
        })
        .transpose()?;
    Ok(InitialValueAssignmentKind::Reference(
        ReferenceInitializer {
            target,
            initial_value,
            syntax,
        },
    ))
}

/// `x : PARAMS(n) OF T`
fn params(cx: &LowerCx, parts: &Parts) -> Result<InitialValueAssignmentKind, Diagnostic> {
    lower_params(cx, &parts.spec).map(InitialValueAssignmentKind::Params)
}

/// `x : (A, B) [:= A]`
fn enumerated_values(
    cx: &LowerCx,
    parts: &Parts,
) -> Result<InitialValueAssignmentKind, Diagnostic> {
    let (values, _) = lower_enumeration(cx, &parts.spec)?;
    let initial_value = parts
        .value
        .as_ref()
        .map(|value| lower_enumerated_value(cx, value))
        .transpose()?;
    Ok(InitialValueAssignmentKind::EnumeratedValues(
        EnumeratedValuesInitializer {
            values,
            initial_value,
        },
    ))
}

/// `x : INT(low .. high)`. The initial value a member may write after it has no
/// place in the object, so it is checked and not kept.
fn subrange(cx: &LowerCx, parts: &Parts) -> Result<InitialValueAssignmentKind, Diagnostic> {
    if let Some(value) = &parts.value {
        lower_integer_value(cx, parts, value)?;
    }
    Ok(InitialValueAssignmentKind::Subrange(
        SpecificationKind::Inline(lower_subrange_specification(cx, &parts.spec)?),
    ))
}

/// `x : T`: a type written as a name, which only a later stage can classify.
fn late_resolved(cx: &LowerCx, parts: &Parts) -> Result<InitialValueAssignmentKind, Diagnostic> {
    Ok(InitialValueAssignmentKind::LateResolvedType(
        LateResolvedInitializer::bare(parts.base_name(cx)?),
    ))
}

/// `x : T := (member := value, ...)`
fn late_resolved_structure(
    cx: &LowerCx,
    parts: &Parts,
) -> Result<InitialValueAssignmentKind, Diagnostic> {
    Ok(late_resolved_members(StructureInitializationDeclaration {
        type_name: parts.base_name(cx)?,
        elements_init: lower_struct_elements(cx, parts.value(cx)?)?,
    }))
}

/// `x : T := Red` and `x : T := Color#Red`: a qualified value names an
/// enumeration, and a name alone is left to the resolver.
fn late_resolved_value(
    cx: &LowerCx,
    parts: &Parts,
) -> Result<InitialValueAssignmentKind, Diagnostic> {
    Ok(late_resolved_or_enumerated(
        parts.base_name(cx)?,
        lower_enumerated_value(cx, parts.value(cx)?)?,
    ))
}

/// `x : INT`
fn simple(cx: &LowerCx, parts: &Parts) -> Result<InitialValueAssignmentKind, Diagnostic> {
    Ok(InitialValueAssignmentKind::Simple(SimpleInitializer {
        type_name: parts.base_name(cx)?,
        initial_value: None,
    }))
}

/// `x : INT := value`: a literal is a simple value, and any other expression
/// is kept as one until the analysis folds it or reports it.
fn simple_expression(
    cx: &LowerCx,
    parts: &Parts,
) -> Result<InitialValueAssignmentKind, Diagnostic> {
    Ok(resolve_initializer_expr(
        parts.base_name(cx)?,
        lower_expr(cx, parts.value(cx)?)?,
    ))
}

/// `x : Block(argument, ...)`: a function block instance made with the
/// arguments of its constructor.
fn call(cx: &LowerCx, parts: &Parts) -> Result<InitialValueAssignmentKind, Diagnostic> {
    let arguments = child_of(&parts.node, K::ArgList)
        .ok_or_else(|| cx.missing(&parts.node, "an argument list"))?;
    Ok(InitialValueAssignmentKind::FunctionBlockCall(
        FunctionBlockCallInitializer {
            type_name: parts.base_name(cx)?,
            params: lower_arguments(cx, &arguments)?,
        },
    ))
}

/// What a declaration of a variable or of a structure member makes of its
/// type and initial value, by the form of the type and the kind of the value.
const INITIAL_VALUES: &[Row<InitialValueAssignmentKind>] = &[
    row(&[Form::Array], &[Init::None, Init::Array], array),
    row(&[Form::Named], &[Init::Array], named_array),
    row(
        &[Form::SizedString, Form::BareString],
        &[Init::None, Init::Value],
        string,
    ),
    row(&[Form::Reference], &[Init::None, Init::Value], reference),
    row(&[Form::Params], &[Init::None], params),
    row(
        &[Form::Enumeration],
        &[Init::None, Init::Name, Init::Qualified],
        enumerated_values,
    ),
    row(&[Form::Subrange], &[Init::None, Init::Value], subrange),
    row(&[Form::Named], &[Init::None], late_resolved),
    row(&[Form::Named], &[Init::Struct], late_resolved_structure),
    row(
        &[Form::Named],
        &[Init::Name, Init::Qualified],
        late_resolved_value,
    ),
    row(&[Form::Elementary], &[Init::None], simple),
    row(
        &[Form::Elementary],
        &[Init::Value, Init::Name],
        simple_expression,
    ),
    row(&[Form::Named], &[Init::Value], simple_expression),
    row(&[Form::Call], &[Init::None], call),
];

/// The kinds of node that hold a declaration with an initial value: a
/// variable declaration and a member of a structure.
const OWNERS: &[K] = &[K::VarDecl, K::StructMember];

/// Lowers what a declaration's type and initial value make of the declaration:
/// a [`K::VarDecl`] or a [`K::StructMember`]. Every name a variable
/// declaration lists is declared the same way, so the result is the one the
/// names share.
pub fn lower_initial_value(
    cx: &LowerCx,
    node: &SyntaxNode,
) -> Result<InitialValueAssignmentKind, Diagnostic> {
    if !OWNERS.contains(&node.kind()) {
        return Err(cx.internal_error(
            node.text_range(),
            format!("{:?} does not hold an initial value", node.kind()),
        ));
    }
    build(cx, INITIAL_VALUES, node)
}
