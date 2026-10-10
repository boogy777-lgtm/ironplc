//! Types: the node kinds that write a type, and the type declarations built
//! from them.
//!
//! A type is written by one node (an array, a string, a reference, an
//! enumeration, a subrange, a structure, a name) and each kind has a rule that
//! builds the piece of the specification it spells: `lower_array`,
//! `lower_string_specification`, `lower_reference`, `lower_enumeration` and the
//! others. The same piece is part of a type declaration, of a structure
//! member and of a variable's declaration, so these rules are shared by every
//! place that reads a type.
//!
//! What a `TYPE` declaration is, is decided by the form of its type and the
//! kind of its initial value together (`declarations`): `DECLARATIONS` has a
//! row for each pair that declares a type, and the rule of the row builds the
//! declaration the legacy grammar built for it. A declaration whose pair no
//! row names is reported by the table, not by a rule.
//!
//! A declaration that is written against a plain name is late-bound: the name
//! may be an alias of an elementary type, an enumeration or a structure, and
//! only the declarations in scope say which.

#[cfg(test)]
mod tests;

use super::declarations::{build, row, Form, Init, Parts, Row};
use super::initializers::{
    lower_array_elements, lower_initial_value, lower_integer_value, lower_simple_initializer,
    lower_string_value, lower_struct_elements,
};
use super::names::{lower_name, lower_type_ref, lower_type_token};
use super::tree::{child_of, children_of, significant_tokens, token_of};
use super::values::{lower_enumerated_value, lower_integer, lower_integer_ref, lower_subrange};
use super::LowerCx;
use crate::syntax_kind::{SyntaxKind as K, SyntaxNode};
use ironplc_dsl::common::{
    ArrayBounds, ArrayDeclaration, ArrayElementType, ArraySubranges, DataTypeDeclarationKind,
    ElementaryTypeName, EnumeratedDefault, EnumeratedSpecificationInit,
    EnumeratedSpecificationKind, EnumeratedValue, EnumerationDeclaration, LateBoundDeclaration,
    ParamsDeclaration, ParamsSpecification, RefSyntax, ReferenceDeclaration, ReferenceTarget,
    SimpleDeclaration, SpecificationKind, StringDeclaration, StringSpecification, StringType,
    StructureDeclaration, StructureElementDeclaration, SubrangeDeclaration, SubrangeSpecification,
    TypeName, UnionDeclaration,
};
use ironplc_dsl::construct::structure_alias;
use ironplc_dsl::diagnostic::Diagnostic;

/// The keywords of the string types, and the width of the characters of each.
const STRING_WIDTHS: &[(K, StringType)] = &[
    (K::String, StringType::String),
    (K::WString, StringType::WString),
];

/// The keywords that introduce a reference type, and the surface syntax each
/// is.
const REFERENCE_SYNTAXES: &[(K, RefSyntax)] = &[
    (K::RefTo, RefSyntax::RefTo),
    (K::Reference, RefSyntax::ReferenceTo),
    (K::Pointer, RefSyntax::PointerTo),
];

/// The node of kind `kind` that `node` is, or the diagnostic for a rule given
/// a node of another kind.
fn expect(cx: &LowerCx, node: &SyntaxNode, kind: K) -> Result<(), Diagnostic> {
    if node.kind() == kind {
        Ok(())
    } else {
        Err(cx.unsupported(node))
    }
}

/// The part of a node that is of `kind`, which the grammar guarantees.
fn part(cx: &LowerCx, node: &SyntaxNode, kind: K) -> Result<SyntaxNode, Diagnostic> {
    child_of(node, kind).ok_or_else(|| cx.missing(node, &format!("{kind:?}")))
}

/// The elementary type a type reference names, for the places that restrict a
/// type to the elementary ones: the base of a subrange or of an enumeration.
fn elementary(cx: &LowerCx, node: &SyntaxNode) -> Result<ElementaryTypeName, Diagnostic> {
    let name = lower_type_ref(cx, node)?;
    ElementaryTypeName::try_from(&name.name).map_err(|()| {
        cx.syntax_error(
            node.text_range(),
            format!("'{}' is not an elementary type", name.name),
        )
    })
}

/// A type written as one name: a name or a keyword, or `STRING` and `WSTRING`
/// without a length, which name a type where a type is only named.
fn lower_type_name(cx: &LowerCx, node: &SyntaxNode) -> Result<TypeName, Diagnostic> {
    match node.kind() {
        K::TypeRef => lower_type_ref(cx, node),
        K::StringType if node.first_child().is_none() => significant_tokens(node)
            .first()
            .map(|keyword| lower_type_token(cx, keyword))
            .ok_or_else(|| cx.missing(node, "a type keyword")),
        _ => Err(cx.syntax_error(node.text_range(), "this type is not allowed here")),
    }
}

/// Lowers `STRING` or `WSTRING`, with the length in `[ ]` or `( )` when it
/// has one.
pub fn lower_string_specification(
    cx: &LowerCx,
    node: &SyntaxNode,
) -> Result<StringSpecification, Diagnostic> {
    expect(cx, node, K::StringType)?;
    let keyword = significant_tokens(node)
        .into_iter()
        .next()
        .ok_or_else(|| cx.missing(node, "a type keyword"))?;
    let width = STRING_WIDTHS
        .iter()
        .find(|(kind, _)| *kind == keyword.kind())
        .map(|(_, width)| width.clone())
        .ok_or_else(|| cx.missing(node, "STRING or WSTRING"))?;
    let length = node
        .first_child()
        .map(|length| lower_integer_ref(cx, &length))
        .transpose()?;
    Ok(StringSpecification {
        width,
        length,
        keyword_span: cx.token_span(&keyword),
    })
}

/// The element type of an array: a name, or a string with its length.
fn lower_element(cx: &LowerCx, node: &SyntaxNode) -> Result<ArrayElementType, Diagnostic> {
    match node.kind() {
        K::TypeRef => lower_type_ref(cx, node).map(ArrayElementType::Named),
        K::StringType => {
            let specification = lower_string_specification(cx, node)?;
            Ok(match specification.width {
                StringType::String => ArrayElementType::String(specification),
                StringType::WString => ArrayElementType::WString(specification),
            })
        }
        _ => Err(cx.syntax_error(
            node.text_range(),
            "this type is not allowed as the element of an array",
        )),
    }
}

/// The keyword that introduces a reference type and the node of the type it
/// points to.
fn reference_parts(cx: &LowerCx, node: &SyntaxNode) -> Result<(RefSyntax, SyntaxNode), Diagnostic> {
    expect(cx, node, K::RefType)?;
    let keyword = significant_tokens(node)
        .into_iter()
        .next()
        .ok_or_else(|| cx.missing(node, "a reference keyword"))?;
    let syntax = REFERENCE_SYNTAXES
        .iter()
        .find(|(kind, _)| *kind == keyword.kind())
        .map(|(_, syntax)| *syntax)
        .ok_or_else(|| cx.missing(node, "a reference keyword"))?;
    let target = node
        .first_child()
        .ok_or_else(|| cx.missing(node, "a type to point to"))?;
    Ok((syntax, target))
}

/// Lowers `ARRAY [ranges] OF T` and `ARRAY [*] OF T`. An element that is a
/// reference is recorded on the array, with the type it points to.
pub fn lower_array(cx: &LowerCx, node: &SyntaxNode) -> Result<ArraySubranges, Diagnostic> {
    expect(cx, node, K::ArrayType)?;
    let bounds = match token_of(node, &[K::Star]) {
        Some(star) => ArrayBounds::Incomplete(cx.token_span(&star)),
        None => ArrayBounds::Ranges(
            children_of(node, K::Subrange)
                .map(|range| lower_subrange(cx, &range))
                .collect::<Result<Vec<_>, _>>()?,
        ),
    };
    let element = node
        .children()
        .last()
        .ok_or_else(|| cx.missing(node, "an element type"))?;
    let (ref_to, element) = match element.kind() {
        K::RefType => {
            let (syntax, target) = reference_parts(cx, &element)?;
            (Some(syntax), target)
        }
        _ => (None, element),
    };
    Ok(ArraySubranges {
        bounds,
        type_name: lower_element(cx, &element)?,
        ref_to,
    })
}

/// Lowers `REF_TO T`, `REFERENCE TO T` and `POINTER TO T`: what it points to,
/// and which keyword wrote it.
pub fn lower_reference(
    cx: &LowerCx,
    node: &SyntaxNode,
) -> Result<(ReferenceTarget, RefSyntax), Diagnostic> {
    let (syntax, target) = reference_parts(cx, node)?;
    let target = match target.kind() {
        K::ArrayType => ReferenceTarget::Array(lower_array(cx, &target)?),
        _ => ReferenceTarget::Named(lower_type_name(cx, &target)?),
    };
    Ok((target, syntax))
}

/// Lowers `PARAMS ( n ) OF T`.
pub fn lower_params(cx: &LowerCx, node: &SyntaxNode) -> Result<ParamsSpecification, Diagnostic> {
    expect(cx, node, K::ParamsType)?;
    let mut nodes = node.children();
    let count = nodes.next().ok_or_else(|| cx.missing(node, "a count"))?;
    let element = nodes
        .next()
        .ok_or_else(|| cx.missing(node, "an element type"))?;
    Ok(ParamsSpecification {
        count: lower_integer_ref(cx, &count)?,
        type_name: lower_type_name(cx, &element)?,
    })
}

/// Lowers `INT ( low .. high )`.
pub fn lower_subrange_specification(
    cx: &LowerCx,
    node: &SyntaxNode,
) -> Result<SubrangeSpecification, Diagnostic> {
    expect(cx, node, K::SubrangeType)?;
    Ok(SubrangeSpecification {
        type_name: elementary(cx, &part(cx, node, K::TypeRef)?)?,
        subrange: lower_subrange(cx, &part(cx, node, K::Subrange)?)?,
    })
}

/// One member of an enumeration: its name, and the value written for it.
fn lower_enumeration_value(cx: &LowerCx, node: &SyntaxNode) -> Result<EnumeratedValue, Diagnostic> {
    Ok(EnumeratedValue {
        type_name: None,
        value: lower_name(cx, &part(cx, node, K::Name)?)?,
        explicit_value: child_of(node, K::IntLiteral)
            .map(|value| lower_integer(cx, &value))
            .transpose()?,
    })
}

/// Lowers `( A, B := 2 )`, with the base type that may follow it: the
/// members, and the base type.
pub fn lower_enumeration(
    cx: &LowerCx,
    node: &SyntaxNode,
) -> Result<(Vec<EnumeratedValue>, Option<ElementaryTypeName>), Diagnostic> {
    expect(cx, node, K::EnumType)?;
    let values = children_of(node, K::EnumValue)
        .map(|value| lower_enumeration_value(cx, &value))
        .collect::<Result<Vec<_>, _>>()?;
    let base = child_of(node, K::TypeRef)
        .map(|base| elementary(cx, &base))
        .transpose()?;
    Ok((values, base))
}

/// Lowers the members of `STRUCT ... END_STRUCT` and `UNION ... END_UNION`,
/// each with its initial value.
pub fn lower_members(
    cx: &LowerCx,
    node: &SyntaxNode,
) -> Result<Vec<StructureElementDeclaration>, Diagnostic> {
    children_of(node, K::StructMember)
        .map(|member| {
            Ok(StructureElementDeclaration {
                name: lower_name(cx, &part(cx, &member, K::Name)?)?,
                init: lower_initial_value(cx, &member)?,
            })
        })
        .collect()
}

/// The name a type declaration declares.
fn declared(cx: &LowerCx, parts: &Parts) -> Result<TypeName, Diagnostic> {
    let name = part(cx, &parts.node, K::Name)?;
    lower_name(cx, &name).map(|name| TypeName { name })
}

/// `name : STRING[n] [:= 'value']`
fn string(cx: &LowerCx, parts: &Parts) -> Result<DataTypeDeclarationKind, Diagnostic> {
    let specification = lower_string_specification(cx, &parts.spec)?;
    let length = specification
        .length
        .ok_or_else(|| cx.missing(&parts.spec, "a length"))?;
    let init = lower_string_value(cx, parts, &specification.width)?;
    Ok(DataTypeDeclarationKind::String(StringDeclaration {
        type_name: declared(cx, parts)?,
        length,
        width: specification.width,
        init,
    }))
}

/// `name : ARRAY [ranges] OF T [:= [elements]]`
fn array(cx: &LowerCx, parts: &Parts) -> Result<DataTypeDeclarationKind, Diagnostic> {
    Ok(DataTypeDeclarationKind::Array(ArrayDeclaration {
        type_name: declared(cx, parts)?,
        spec: SpecificationKind::Inline(lower_array(cx, &parts.spec)?),
        init: match &parts.value {
            Some(elements) => lower_array_elements(cx, elements)?,
            None => vec![],
        },
    }))
}

/// `name : PARAMS(n) OF T`
fn params(cx: &LowerCx, parts: &Parts) -> Result<DataTypeDeclarationKind, Diagnostic> {
    Ok(DataTypeDeclarationKind::Params(ParamsDeclaration {
        type_name: declared(cx, parts)?,
        spec: lower_params(cx, &parts.spec)?,
    }))
}

/// `name : INT(low .. high) [:= default]`
fn subrange(cx: &LowerCx, parts: &Parts) -> Result<DataTypeDeclarationKind, Diagnostic> {
    Ok(DataTypeDeclarationKind::Subrange(SubrangeDeclaration {
        type_name: declared(cx, parts)?,
        spec: SpecificationKind::Inline(lower_subrange_specification(cx, &parts.spec)?),
        default: parts
            .value
            .as_ref()
            .map(|value| lower_integer_value(cx, parts, value))
            .transpose()?,
    }))
}

/// `name : STRUCT members END_STRUCT`
fn structure(cx: &LowerCx, parts: &Parts) -> Result<DataTypeDeclarationKind, Diagnostic> {
    Ok(DataTypeDeclarationKind::Structure(StructureDeclaration {
        type_name: declared(cx, parts)?,
        elements: lower_members(cx, &parts.spec)?,
    }))
}

/// `name : UNION members END_UNION`
fn union(cx: &LowerCx, parts: &Parts) -> Result<DataTypeDeclarationKind, Diagnostic> {
    Ok(DataTypeDeclarationKind::Union(UnionDeclaration {
        type_name: declared(cx, parts)?,
        elements: lower_members(cx, &parts.spec)?,
    }))
}

/// `name : Structure := (member := value, ...)`. The declaration holds the
/// declared name, the structure it is a copy of and the values.
fn structure_initialization(
    cx: &LowerCx,
    parts: &Parts,
) -> Result<DataTypeDeclarationKind, Diagnostic> {
    Ok(structure_alias(
        declared(cx, parts)?,
        parts.base_name(cx)?,
        lower_struct_elements(cx, parts.value(cx)?)?,
    ))
}

/// The value written as the default of an enumeration, when there is one: a
/// value of it, or a number (`:= 1`), whose being the number of a value the
/// analysis decides.
fn default_value(cx: &LowerCx, parts: &Parts) -> Result<Option<EnumeratedDefault>, Diagnostic> {
    parts
        .value
        .as_ref()
        .map(|value| match parts.init {
            Init::Value => lower_integer_value(cx, parts, value).map(EnumeratedDefault::Number),
            _ => lower_enumerated_value(cx, value).map(EnumeratedDefault::Value),
        })
        .transpose()
}

/// `name : (A, B := 2) [BYTE] [:= A]`
fn enumeration(cx: &LowerCx, parts: &Parts) -> Result<DataTypeDeclarationKind, Diagnostic> {
    let (values, underlying_type) = lower_enumeration(cx, &parts.spec)?;
    Ok(DataTypeDeclarationKind::Enumeration(
        EnumerationDeclaration {
            type_name: declared(cx, parts)?,
            spec_init: EnumeratedSpecificationInit {
                spec: EnumeratedSpecificationKind::values(values),
                default: default_value(cx, parts)?,
                underlying_type,
            },
        },
    ))
}

/// `name : Enumeration := Value`: a named type with a default is an
/// enumeration, because a default is a value of one.
fn enumeration_alias(cx: &LowerCx, parts: &Parts) -> Result<DataTypeDeclarationKind, Diagnostic> {
    Ok(DataTypeDeclarationKind::Enumeration(
        EnumerationDeclaration {
            type_name: declared(cx, parts)?,
            spec_init: EnumeratedSpecificationInit {
                spec: SpecificationKind::Named(parts.base_name(cx)?),
                default: default_value(cx, parts)?,
                underlying_type: None,
            },
        },
    ))
}

/// `name : T [:= value]`: a type that is another type with a value, which is a
/// literal or an expression of constants.
fn simple(cx: &LowerCx, parts: &Parts) -> Result<DataTypeDeclarationKind, Diagnostic> {
    Ok(DataTypeDeclarationKind::Simple(SimpleDeclaration {
        type_name: declared(cx, parts)?,
        spec_and_init: lower_simple_initializer(cx, parts)?,
    }))
}

/// `name : REF_TO T`
fn reference(cx: &LowerCx, parts: &Parts) -> Result<DataTypeDeclarationKind, Diagnostic> {
    let (target, syntax) = lower_reference(cx, &parts.spec)?;
    Ok(DataTypeDeclarationKind::Reference(ReferenceDeclaration {
        type_name: declared(cx, parts)?,
        target,
        syntax,
    }))
}

/// `name : Other`: the other type may be an alias of an elementary type, an
/// enumeration or a structure, and only the declarations in scope say which.
fn late_bound(cx: &LowerCx, parts: &Parts) -> Result<DataTypeDeclarationKind, Diagnostic> {
    Ok(DataTypeDeclarationKind::LateBound(LateBoundDeclaration {
        data_type_name: declared(cx, parts)?,
        base_type_name: parts.base_name(cx)?,
    }))
}

/// The declarations a `TYPE` block holds, by the form of the type and the kind
/// of the initial value.
const DECLARATIONS: &[Row<DataTypeDeclarationKind>] = &[
    row(&[Form::SizedString], &[Init::None, Init::Value], string),
    row(&[Form::Array], &[Init::None, Init::Array], array),
    row(&[Form::Params], &[Init::None], params),
    row(&[Form::Subrange], &[Init::None, Init::Value], subrange),
    row(&[Form::Struct], &[Init::None], structure),
    row(&[Form::Union], &[Init::None], union),
    row(&[Form::Named], &[Init::Struct], structure_initialization),
    row(
        &[Form::Enumeration],
        &[Init::None, Init::Name, Init::Qualified, Init::Value],
        enumeration,
    ),
    row(
        &[Form::Named],
        &[Init::Name, Init::Qualified],
        enumeration_alias,
    ),
    row(
        &[Form::Elementary, Form::BareString],
        &[Init::None, Init::Value, Init::Name],
        simple,
    ),
    row(&[Form::Named], &[Init::Value], simple),
    row(&[Form::Reference], &[Init::None], reference),
    row(&[Form::Named], &[Init::None], late_bound),
];

/// Lowers one declaration of a `TYPE` block: a [`K::TypeDecl`].
pub fn lower_type_declaration(
    cx: &LowerCx,
    node: &SyntaxNode,
) -> Result<DataTypeDeclarationKind, Diagnostic> {
    expect(cx, node, K::TypeDecl)?;
    build(cx, DECLARATIONS, node)
}

/// Lowers a `TYPE ... END_TYPE` block to its declarations, in order.
pub fn lower_type_block(
    cx: &LowerCx,
    node: &SyntaxNode,
) -> Result<Vec<DataTypeDeclarationKind>, Diagnostic> {
    expect(cx, node, K::TypeBlock)?;
    children_of(node, K::TypeDecl)
        .map(|declaration| lower_type_declaration(cx, &declaration))
        .collect()
}
