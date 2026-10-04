//! Program organisation units: `PROGRAM`, `FUNCTION`, `FUNCTION_BLOCK`, `INTERFACE`, the
//! `NAMESPACE` that groups declarations, and the sections every declaration is
//! read by.
//!
//! A unit, a method, a property accessor, a configuration and a resource are
//! all a header followed by variable blocks and what the declaration holds: a
//! body and, for a function block, members; resources, tasks and program
//! configurations for a configuration and a resource. [`Sections`] reads that
//! part for every one of them: `SECTIONS` has a row for each kind of node that
//! may stand between the header and the closing keyword, and `HEADER` lists the
//! kinds that are the header's own and are read by the rule of the declaration.
//! A node of any other kind is reported, so a part that has no rule is never
//! silently dropped.
//!
//! The variable blocks of a declaration are assembled in the order written: the
//! variables of every block form one list, the edge variables another, the
//! access paths of a program a third and the initialisations of the instances of
//! a configuration a fourth. A body is a row of `BODIES`: statements, or a
//! sequential function chart.
//!
//! A return type is written as a name or as a string with a length, and
//! `RETURN_TYPES` has a row for each. The same table reads the type of a
//! method and of a property.

#[cfg(test)]
mod tests;

use super::configuration::{lower_program_configuration, lower_resource, lower_task};
use super::names::{lower_name, lower_type_ref};
use super::oop::{lower_member, lower_oop, lower_type_list, Member};
use super::sfc::lower_chart;
use super::statements::lower_statement_list;
use super::tree::child_of;
use super::types::lower_string_specification;
use super::var_blocks::{lower_var_block, InstanceInit};
use super::{lower_elements, LowerCx};
use crate::syntax_kind::{SyntaxKind as K, SyntaxNode};
use ironplc_dsl::common::{
    EdgeVarDecl, FunctionBlockBodyKind, FunctionBlockDeclaration, FunctionDeclaration,
    FunctionReturnType, InterfaceDeclaration, NamespaceDeclaration, ProgramAccessDecl,
    ProgramDeclaration, StringType, TypeName, VarDecl,
};
use ironplc_dsl::configuration::{ProgramConfiguration, ResourceDeclaration, TaskConfiguration};
use ironplc_dsl::core::Id;
use ironplc_dsl::diagnostic::Diagnostic;
use ironplc_dsl::textual::StmtKind;

/// What a unit holds between its header and its closing keyword.
#[derive(Debug, Default)]
pub struct Sections {
    /// The variables of every block, in the order written.
    pub variables: Vec<VarDecl>,
    /// The edge variables of every block, in the order written.
    pub edges: Vec<EdgeVarDecl>,
    /// The access paths of the blocks that hold them.
    pub access: Vec<ProgramAccessDecl>,
    /// What the unit does, when it writes a body.
    pub body: Option<FunctionBlockBodyKind>,
    /// The methods and properties, in the order written.
    pub members: Vec<Member>,
    /// The initialisations of the instances of a configuration, in the order
    /// written.
    pub instances: Vec<InstanceInit>,
    /// The resource of a configuration.
    pub resources: Vec<ResourceDeclaration>,
    /// The tasks of a resource, in the order written.
    pub tasks: Vec<TaskConfiguration>,
    /// The program configurations of a resource, in the order written.
    pub programs: Vec<ProgramConfiguration>,
}

/// A rule: reads one section of a unit into its parts.
type SectionRule = fn(&LowerCx, &SyntaxNode, &mut Sections) -> Result<(), Diagnostic>;

/// The kinds of node that stand between the header and the closing keyword,
/// and what each adds to the unit.
const SECTIONS: &[(K, SectionRule)] = &[
    (K::VarBlock, block),
    (K::StatementList, body),
    (K::SfcBody, body),
    (K::MethodDecl, member),
    (K::PropertyDecl, member),
    (K::ResourceDecl, resource),
    (K::TaskDecl, task),
    (K::ProgramConfig, program_configuration),
];

/// The kinds of node that are part of a declaration's header: its name, its
/// type, its qualifiers, the types it extends or implements and the type of the
/// processor a resource runs on. The rule of the declaration reads them.
pub const HEADER: &[K] = &[
    K::Name,
    K::TypeRef,
    K::StringType,
    K::MemberQualifier,
    K::ExtendsClause,
    K::ImplementsClause,
    K::NameRef,
];

/// Adds the variables, edges and access paths of a block.
fn block(cx: &LowerCx, node: &SyntaxNode, into: &mut Sections) -> Result<(), Diagnostic> {
    let block = lower_var_block(cx, node)?;
    into.variables.extend(block.variables);
    into.edges.extend(block.edges);
    into.access.extend(block.access);
    into.instances.extend(block.instances);
    Ok(())
}

/// A rule: builds the body a node writes.
type BodyRule = fn(&LowerCx, &SyntaxNode) -> Result<FunctionBlockBodyKind, Diagnostic>;

/// The kinds of node that write a body, and the body each is.
const BODIES: &[(K, BodyRule)] = &[(K::StatementList, statements), (K::SfcBody, chart)];

/// A list with nothing written in it is no body: the body of an action that
/// has none is the same body as that of a unit that has none.
fn statements(cx: &LowerCx, node: &SyntaxNode) -> Result<FunctionBlockBodyKind, Diagnostic> {
    if node.text_range().is_empty() {
        return Ok(FunctionBlockBodyKind::empty());
    }
    lower_statement_list(cx, node).map(FunctionBlockBodyKind::stmts)
}

fn chart(cx: &LowerCx, node: &SyntaxNode) -> Result<FunctionBlockBodyKind, Diagnostic> {
    lower_chart(cx, node).map(FunctionBlockBodyKind::sfc)
}

/// The body a node of a kind of `BODIES` writes.
fn build_body(cx: &LowerCx, node: &SyntaxNode) -> Result<FunctionBlockBodyKind, Diagnostic> {
    let (_, rule) = BODIES
        .iter()
        .find(|(kind, _)| *kind == node.kind())
        .ok_or_else(|| cx.unsupported(node))?;
    rule(cx, node)
}

/// The body a declaration writes between its header and its closing keyword:
/// an empty one when it writes none. An action is the one declaration that has
/// only this part.
pub fn lower_body(cx: &LowerCx, node: &SyntaxNode) -> Result<FunctionBlockBodyKind, Diagnostic> {
    match node
        .children()
        .find(|child| BODIES.iter().any(|(kind, _)| *kind == child.kind()))
    {
        Some(written) => build_body(cx, &written),
        None => Ok(FunctionBlockBodyKind::empty()),
    }
}

/// Sets the body the node writes.
fn body(cx: &LowerCx, node: &SyntaxNode, into: &mut Sections) -> Result<(), Diagnostic> {
    into.body = Some(build_body(cx, node)?);
    Ok(())
}

/// Adds the method or property a node declares.
fn member(cx: &LowerCx, node: &SyntaxNode, into: &mut Sections) -> Result<(), Diagnostic> {
    into.members.push(lower_member(cx, node)?);
    Ok(())
}

/// Adds the resource a node declares.
fn resource(cx: &LowerCx, node: &SyntaxNode, into: &mut Sections) -> Result<(), Diagnostic> {
    into.resources.push(lower_resource(cx, node)?);
    Ok(())
}

/// Adds the task a node declares.
fn task(cx: &LowerCx, node: &SyntaxNode, into: &mut Sections) -> Result<(), Diagnostic> {
    into.tasks.push(lower_task(cx, node)?);
    Ok(())
}

/// Adds the program configuration a node declares.
fn program_configuration(
    cx: &LowerCx,
    node: &SyntaxNode,
    into: &mut Sections,
) -> Result<(), Diagnostic> {
    into.programs.push(lower_program_configuration(cx, node)?);
    Ok(())
}

/// Reads the sections of the declaration at `node`: a unit, a method, an
/// accessor, a configuration or a resource.
pub fn lower_sections(cx: &LowerCx, node: &SyntaxNode) -> Result<Sections, Diagnostic> {
    let mut sections = Sections::default();
    for child in node.children() {
        if HEADER.contains(&child.kind()) {
            continue;
        }
        let (_, rule) = SECTIONS
            .iter()
            .find(|(kind, _)| *kind == child.kind())
            .ok_or_else(|| cx.unsupported(&child))?;
        rule(cx, &child, &mut sections)?;
    }
    Ok(sections)
}

/// The statements of a body that holds statements and nothing else: the body of
/// a function, a method or an accessor, which cannot be a chart.
pub fn statements_of(
    cx: &LowerCx,
    node: &SyntaxNode,
    body: Option<FunctionBlockBodyKind>,
) -> Result<Vec<StmtKind>, Diagnostic> {
    match body {
        Some(FunctionBlockBodyKind::Statements(statements)) => Ok(statements.body),
        Some(FunctionBlockBodyKind::Empty) | None => Ok(vec![]),
        Some(FunctionBlockBodyKind::Sfc(_)) => Err(cx.internal_error(
            node.text_range(),
            "a sequential function chart is a body of a program or a function block",
        )),
    }
}

/// The body of a program or a function block: the one it writes, or an empty
/// one.
fn body_of(body: Option<FunctionBlockBodyKind>) -> FunctionBlockBodyKind {
    body.unwrap_or_else(FunctionBlockBodyKind::empty)
}

/// A rule: builds the return type a node writes.
type ReturnTypeRule = fn(&LowerCx, &SyntaxNode) -> Result<FunctionReturnType, Diagnostic>;

/// The kinds of node that write a return type, and the type each is.
const RETURN_TYPES: &[(K, ReturnTypeRule)] = &[
    (K::TypeRef, |cx, node| {
        lower_type_ref(cx, node).map(FunctionReturnType::Named)
    }),
    (K::StringType, string),
];

/// `STRING[n]` and `WSTRING[n]`, the length optional.
fn string(cx: &LowerCx, node: &SyntaxNode) -> Result<FunctionReturnType, Diagnostic> {
    let specification = lower_string_specification(cx, node)?;
    Ok(match specification.width {
        StringType::String => FunctionReturnType::String(specification),
        StringType::WString => FunctionReturnType::WString(specification),
    })
}

/// The return type a declaration writes, or none when it writes none.
pub fn lower_return_type(
    cx: &LowerCx,
    node: &SyntaxNode,
) -> Result<Option<FunctionReturnType>, Diagnostic> {
    let written = node.children().find_map(|child| {
        RETURN_TYPES
            .iter()
            .find(|(kind, _)| *kind == child.kind())
            .map(|(_, rule)| (child.clone(), rule))
    });
    written.map(|(node, rule)| rule(cx, &node)).transpose()
}

/// The return type of a declaration that must write one.
pub fn required_return_type(
    cx: &LowerCx,
    node: &SyntaxNode,
) -> Result<FunctionReturnType, Diagnostic> {
    lower_return_type(cx, node)?.ok_or_else(|| cx.missing(node, "a type"))
}

/// The name a declaration introduces.
pub fn declared_name(cx: &LowerCx, node: &SyntaxNode) -> Result<Id, Diagnostic> {
    let name = child_of(node, K::Name).ok_or_else(|| cx.missing(node, "a name"))?;
    lower_name(cx, &name)
}

/// `PROGRAM name ... END_PROGRAM`
///
/// A program has no place for edge variables (`x : BOOL R_EDGE`), as the legacy
/// grammar has none: those of its blocks are not carried over.
pub fn lower_program(cx: &LowerCx, node: &SyntaxNode) -> Result<ProgramDeclaration, Diagnostic> {
    let sections = lower_sections(cx, node)?;
    Ok(ProgramDeclaration {
        name: declared_name(cx, node)?,
        variables: sections.variables,
        access_variables: sections.access,
        body: body_of(sections.body),
    })
}

/// `FUNCTION name : type ... END_FUNCTION`
pub fn lower_function(cx: &LowerCx, node: &SyntaxNode) -> Result<FunctionDeclaration, Diagnostic> {
    let sections = lower_sections(cx, node)?;
    Ok(FunctionDeclaration {
        name: declared_name(cx, node)?,
        return_type: required_return_type(cx, node)?,
        variables: sections.variables,
        edge_variables: sections.edges,
        body: statements_of(cx, node, sections.body)?,
    })
}

/// `FUNCTION_BLOCK [qualifiers] name [EXTENDS t] [IMPLEMENTS t, ...] ...
/// END_FUNCTION_BLOCK`
pub fn lower_function_block(
    cx: &LowerCx,
    node: &SyntaxNode,
) -> Result<FunctionBlockDeclaration, Diagnostic> {
    let sections = lower_sections(cx, node)?;
    let mut methods = Vec::new();
    let mut properties = Vec::new();
    for member in sections.members {
        match member {
            Member::Method(method) => methods.push(*method),
            Member::Property(property) => properties.push(*property),
        }
    }
    Ok(FunctionBlockDeclaration {
        name: TypeName {
            name: declared_name(cx, node)?,
        },
        variables: sections.variables,
        edge_variables: sections.edges,
        body: body_of(sections.body),
        span: cx.node_span(node),
        oop: lower_oop(cx, node)?,
        methods,
        properties,
    })
}

/// `NAMESPACE name {declaration} END_NAMESPACE`
pub fn lower_namespace(
    cx: &LowerCx,
    node: &SyntaxNode,
) -> Result<NamespaceDeclaration, Diagnostic> {
    Ok(NamespaceDeclaration {
        name: declared_name(cx, node)?,
        elements: lower_elements(cx, node)?,
        span: cx.node_span(node),
    })
}

/// `INTERFACE name [EXTENDS name {, name}] END_INTERFACE`
pub fn lower_interface(
    cx: &LowerCx,
    node: &SyntaxNode,
) -> Result<InterfaceDeclaration, Diagnostic> {
    Ok(InterfaceDeclaration {
        name: declared_name(cx, node)?,
        extends: match child_of(node, K::ExtendsClause) {
            Some(clause) => lower_type_list(cx, &clause)?,
            None => vec![],
        },
    })
}
