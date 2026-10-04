//! The object-oriented parts of a function block: member qualifiers, the
//! `EXTENDS` and `IMPLEMENTS` clauses, methods and properties.
//!
//! A qualifier is a word, and `QUALIFIERS` has a row for each word and the
//! kind of qualifier it is: another qualifier is another row. The words other
//! than `ABSTRACT` are ordinary names outside the place a qualifier stands, so
//! the table is read by the text of the word and not by the kind of the token.
//!
//! A method is read by the same rule as a unit's sections (`pou`), and a
//! property is the two accessors it holds, each built as the method it behaves
//! as by the constructors the legacy grammar uses (`PropertyDeclaration`).

#[cfg(test)]
mod tests;

use super::names::lower_type_ref;
use super::pou::{
    declared_name, lower_return_type, lower_sections, required_return_type, statements_of,
};
use super::tree::{child_of, children_of, significant_tokens};
use super::LowerCx;
use crate::syntax_kind::{SyntaxKind as K, SyntaxNode};
use ironplc_dsl::common::{
    EdgeVarDecl, FunctionBlockOop, FunctionReturnType, MethodDeclaration, PropertyDeclaration,
    TypeName, VarDecl,
};
use ironplc_dsl::core::{Id, SourceSpan};
use ironplc_dsl::diagnostic::Diagnostic;
use ironplc_dsl::member_qualifier::{
    AccessSpecifier, MemberQualifier, MemberQualifierKind, MemberQualifiers,
};
use ironplc_dsl::textual::StmtKind;
use rowan::TextRange;

/// A method or a property: what a function block may declare after its body.
#[derive(Debug, Clone, PartialEq)]
pub enum Member {
    Method(Box<MethodDeclaration>),
    Property(Box<PropertyDeclaration>),
}

/// The words that are qualifiers, upper case, and the kind each is.
const QUALIFIERS: &[(&str, MemberQualifierKind)] = &[
    ("ABSTRACT", MemberQualifierKind::Abstract),
    (
        "PUBLIC",
        MemberQualifierKind::Access(AccessSpecifier::Public),
    ),
    (
        "PRIVATE",
        MemberQualifierKind::Access(AccessSpecifier::Private),
    ),
    (
        "PROTECTED",
        MemberQualifierKind::Access(AccessSpecifier::Protected),
    ),
    (
        "INTERNAL",
        MemberQualifierKind::Access(AccessSpecifier::Internal),
    ),
    ("FINAL", MemberQualifierKind::Final),
    ("OVERRIDE", MemberQualifierKind::Override),
    ("OVERLOAD", MemberQualifierKind::Overload),
];

/// Lowers one qualifier.
fn lower_qualifier(cx: &LowerCx, node: &SyntaxNode) -> Result<MemberQualifier, Diagnostic> {
    let word = significant_tokens(node)
        .into_iter()
        .next()
        .ok_or_else(|| cx.missing(node, "a qualifier word"))?;
    let (_, kind) = QUALIFIERS
        .iter()
        .find(|(spelling, _)| word.text().eq_ignore_ascii_case(spelling))
        .ok_or_else(|| cx.missing(node, "a qualifier word"))?;
    Ok(MemberQualifier {
        kind: *kind,
        span: cx.token_span(&word),
    })
}

/// The qualifiers a declaration is written with, in the order written.
pub fn lower_qualifiers(cx: &LowerCx, node: &SyntaxNode) -> Result<MemberQualifiers, Diagnostic> {
    children_of(node, K::MemberQualifier)
        .map(|qualifier| lower_qualifier(cx, &qualifier))
        .collect::<Result<Vec<_>, _>>()
        .map(MemberQualifiers::new)
}

/// The types an `EXTENDS` or `IMPLEMENTS` clause names, in the order written.
pub fn lower_type_list(cx: &LowerCx, clause: &SyntaxNode) -> Result<Vec<TypeName>, Diagnostic> {
    children_of(clause, K::TypeRef)
        .map(|name| lower_type_ref(cx, &name))
        .collect()
}

/// The clauses and qualifiers that make up the object-oriented facet of a
/// function block.
const FACET: &[K] = &[K::MemberQualifier, K::ExtendsClause, K::ImplementsClause];

/// The facet of a function block, when it writes any of it: the clauses, the
/// qualifiers, and the range from the first of them to the last.
pub fn lower_oop(cx: &LowerCx, node: &SyntaxNode) -> Result<Option<FunctionBlockOop>, Diagnostic> {
    let written: Vec<SyntaxNode> = node
        .children()
        .filter(|child| FACET.contains(&child.kind()))
        .collect();
    let Some(range) = written
        .iter()
        .map(|part| part.text_range())
        .reduce(TextRange::cover)
    else {
        return Ok(None);
    };
    let base = match child_of(node, K::ExtendsClause) {
        Some(clause) => lower_type_list(cx, &clause)?.into_iter().next(),
        None => None,
    };
    let implements = match child_of(node, K::ImplementsClause) {
        Some(clause) => lower_type_list(cx, &clause)?,
        None => vec![],
    };
    Ok(Some(FunctionBlockOop {
        base,
        implements,
        qualifiers: lower_qualifiers(cx, node)?,
        span: cx.span(range),
    }))
}

/// `METHOD [qualifiers] name [: type] ... END_METHOD`
pub fn lower_method(cx: &LowerCx, node: &SyntaxNode) -> Result<MethodDeclaration, Diagnostic> {
    let sections = lower_sections(cx, node)?;
    Ok(MethodDeclaration {
        qualifiers: lower_qualifiers(cx, node)?,
        name: declared_name(cx, node)?,
        return_type: lower_return_type(cx, node)?,
        variables: sections.variables,
        edge_variables: sections.edges,
        body: statements_of(cx, node, sections.body)?,
        span: cx.node_span(node),
    })
}

/// A constructor of the method an accessor behaves as.
type Accessor = fn(
    &Id,
    &FunctionReturnType,
    Vec<VarDecl>,
    Vec<EdgeVarDecl>,
    Vec<StmtKind>,
    SourceSpan,
) -> MethodDeclaration;

/// The accessors, by the kind of node that writes each, and the constructor of
/// the method each behaves as.
const ACCESSORS: &[(K, Accessor)] = &[
    (K::GetAccessor, PropertyDeclaration::get_accessor),
    (K::SetAccessor, PropertyDeclaration::set_accessor),
];

/// The method the accessor of kind `kind` of a property behaves as, when the
/// property writes one.
fn lower_accessor(
    cx: &LowerCx,
    property: &SyntaxNode,
    kind: K,
    name: &Id,
    property_type: &FunctionReturnType,
) -> Result<Option<MethodDeclaration>, Diagnostic> {
    let Some(node) = child_of(property, kind) else {
        return Ok(None);
    };
    let (_, build) = ACCESSORS
        .iter()
        .find(|(accessor, _)| *accessor == kind)
        .ok_or_else(|| cx.unsupported(&node))?;
    let sections = lower_sections(cx, &node)?;
    Ok(Some(build(
        name,
        property_type,
        sections.variables,
        sections.edges,
        statements_of(cx, &node, sections.body)?,
        cx.node_span(&node),
    )))
}

/// `PROPERTY [qualifiers] name : type [GET ... END_GET] [SET ... END_SET]
/// END_PROPERTY`
pub fn lower_property(cx: &LowerCx, node: &SyntaxNode) -> Result<PropertyDeclaration, Diagnostic> {
    let name = declared_name(cx, node)?;
    let property_type = required_return_type(cx, node)?;
    Ok(PropertyDeclaration {
        qualifiers: lower_qualifiers(cx, node)?,
        get: lower_accessor(cx, node, K::GetAccessor, &name, &property_type)?,
        set: lower_accessor(cx, node, K::SetAccessor, &name, &property_type)?,
        name,
        property_type,
        span: cx.node_span(node),
    })
}

/// A rule: builds the member a node declares.
type MemberRule = fn(&LowerCx, &SyntaxNode) -> Result<Member, Diagnostic>;

/// The kinds of node that declare a member, and the member each is.
const MEMBERS: &[(K, MemberRule)] = &[
    (K::MethodDecl, |cx, node| {
        lower_method(cx, node).map(|method| Member::Method(Box::new(method)))
    }),
    (K::PropertyDecl, |cx, node| {
        lower_property(cx, node).map(|property| Member::Property(Box::new(property)))
    }),
];

/// Lowers a method or a property.
pub fn lower_member(cx: &LowerCx, node: &SyntaxNode) -> Result<Member, Diagnostic> {
    let (_, rule) = MEMBERS
        .iter()
        .find(|(kind, _)| *kind == node.kind())
        .ok_or_else(|| cx.unsupported(node))?;
    rule(cx, node)
}
