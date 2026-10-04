//! Variable blocks: `VAR ... END_VAR` and the family around it.
//!
//! A block is one rule and the tables it reads. `BLOCKS` has a row for each
//! block keyword: what kind of variable the block declares, or that it holds
//! another form of item (an access path, the initialisation of an instance in
//! a configuration). `QUALIFIERS` has a row for each word that may follow the
//! keyword, and `EDGES` for each edge a declaration may name. Another block, or
//! another qualifier, is another row.
//!
//! What a declaration declares is not decided here. `names : type [:= value]`
//! is read apart by `declarations` and its type and value are built by the
//! table of initial values, which is the one that builds them for a structure
//! member and a configuration. What this module adds is what only a block
//! knows: the kind of variable, the qualifier every declaration of the block
//! takes, the location that may follow the names, and the identity of the
//! block. Every declaration of one block shares one [`BlockId`], so a later
//! stage can tell which declarations were written together (a located
//! variable beside plain ones is one the standard does not allow).
//!
//! The declarations of a block are in the order written. A declaration that
//! lists several names declares each of them, all with the same type and
//! value.

#[cfg(test)]
mod tests;

use super::declarations::{build, row, Form, Init, Parts, Row};
use super::initializers::{lower_initial_value, lower_struct_elements};
use super::names::{lower_id, lower_name, lower_type_ref};
use super::tree::{child_of, children_of, left_spine, significant_tokens};
use super::variables::{lower_address, lower_symbolic};
use super::LowerCx;
use crate::syntax_kind::{SyntaxKind as K, SyntaxNode};
use ironplc_dsl::common::{
    next_block_id, BlockId, DeclarationQualifier, EdgeDirection, EdgeVarDecl, ProgramAccessDecl,
    VarDecl, VariableIdentifier, VariableType,
};
use ironplc_dsl::configuration::{Direction, FunctionBlockInit, LocatedVarInit};
use ironplc_dsl::core::Id;
use ironplc_dsl::diagnostic::Diagnostic;

/// The initialisation of one instance in a configuration: a function block
/// given the values of its members, or a variable given a location or a
/// value.
#[derive(Debug, Clone, PartialEq)]
pub enum InstanceInit {
    FunctionBlock(Box<FunctionBlockInit>),
    Located(Box<LocatedVarInit>),
}

/// What a block declares, by the kind of item it holds.
#[derive(Debug, Clone, Default, PartialEq)]
pub struct Block {
    /// The variables, in the order written.
    pub variables: Vec<VarDecl>,
    /// The variables that name an edge (`x : BOOL R_EDGE`), which are
    /// declarations of their own.
    pub edges: Vec<EdgeVarDecl>,
    /// The access paths of `VAR_ACCESS`.
    pub access: Vec<ProgramAccessDecl>,
    /// The initialisations of `VAR_CONFIG`.
    pub instances: Vec<InstanceInit>,
}

/// The form of the items a block holds.
#[derive(Clone)]
enum Items {
    /// `names [AT address] : type [:= value] ;`, declaring variables of the
    /// kind.
    Declarations(VariableType),
    /// `name : path : type [READ_ONLY | READ_WRITE] ;`
    Access,
    /// `resource.program.path [AT address] : type [:= value] ;`
    Instances,
}

use Items::{Access, Declarations, Instances};

/// The block keywords, and what each block holds.
const BLOCKS: &[(K, Items)] = &[
    (K::Var, Declarations(VariableType::Var)),
    (K::VarInput, Declarations(VariableType::Input)),
    (K::VarOutput, Declarations(VariableType::Output)),
    (K::VarInOut, Declarations(VariableType::InOut)),
    (K::VarTemp, Declarations(VariableType::VarTemp)),
    (K::VarExternal, Declarations(VariableType::External)),
    (K::VarGlobal, Declarations(VariableType::Global)),
    (K::VarStat, Declarations(VariableType::VarStat)),
    (K::VarInst, Declarations(VariableType::VarInst)),
    (K::VarGeneric, Declarations(VariableType::VarGeneric)),
    (K::VarAccess, Access),
    (K::VarConfig, Instances),
];

/// The words that may follow a block keyword, and the qualifier each is. A
/// block without one is unqualified.
const QUALIFIERS: &[(K, DeclarationQualifier)] = &[
    (K::Constant, DeclarationQualifier::Constant),
    (K::Retain, DeclarationQualifier::Retain),
    (K::NonRetain, DeclarationQualifier::NonRetain),
    (K::Persistent, DeclarationQualifier::Persistent),
];

/// The edges a declaration may name after `BOOL`.
const EDGES: &[(K, EdgeDirection)] = &[
    (K::REdge, EdgeDirection::Rising),
    (K::FEdge, EdgeDirection::Falling),
];

/// What the communication services may do with an access path.
const DIRECTIONS: &[(K, Direction)] = &[
    (K::ReadOnly, Direction::ReadOnly),
    (K::ReadWrite, Direction::ReadWrite),
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

/// What a block's declarations share: the kind of variable, the qualifier and
/// the block.
struct Shared<'a> {
    var_type: &'a VariableType,
    qualifier: &'a DeclarationQualifier,
    block: BlockId,
}

/// The name a declaration lists, in the order written.
fn names_of(cx: &LowerCx, node: &SyntaxNode) -> Result<Vec<Id>, Diagnostic> {
    children_of(node, K::Name)
        .map(|name| lower_name(cx, &name))
        .collect()
}

/// The identifiers a declaration declares: each name, or the one name (if it
/// has one) that stands for a location.
fn identifiers_of(cx: &LowerCx, node: &SyntaxNode) -> Result<Vec<VariableIdentifier>, Diagnostic> {
    let names = names_of(cx, node)?;
    match child_of(node, K::Location) {
        Some(location) => Ok(vec![VariableIdentifier::new_direct(
            names.into_iter().next(),
            lower_address(cx, &location)?,
        )]),
        None => Ok(names.into_iter().map(VariableIdentifier::Symbol).collect()),
    }
}

/// The edge a declaration names, when it names one.
fn edge_of(cx: &LowerCx, node: &SyntaxNode) -> Result<Option<EdgeDirection>, Diagnostic> {
    let Some(spec) = child_of(node, K::EdgeSpec) else {
        return Ok(None);
    };
    let keyword = significant_tokens(&spec)
        .into_iter()
        .next()
        .ok_or_else(|| cx.missing(&spec, "an edge"))?;
    EDGES
        .iter()
        .find(|(kind, _)| *kind == keyword.kind())
        .map(|(_, direction)| Some(direction.clone()))
        .ok_or_else(|| cx.missing(&spec, "R_EDGE or F_EDGE"))
}

/// Lowers one declaration of a block to what it declares: a variable for each
/// name, or an edge variable for each name when it names an edge.
fn lower_declaration(
    cx: &LowerCx,
    node: &SyntaxNode,
    shared: &Shared,
    into: &mut Block,
) -> Result<(), Diagnostic> {
    match edge_of(cx, node)? {
        Some(direction) => {
            for identifier in names_of(cx, node)? {
                into.edges.push(EdgeVarDecl {
                    identifier,
                    direction: direction.clone(),
                    qualifier: shared.qualifier.clone(),
                });
            }
        }
        None => {
            let initializer = lower_initial_value(cx, node)?;
            for identifier in identifiers_of(cx, node)? {
                into.variables.push(VarDecl {
                    identifier,
                    var_type: shared.var_type.clone(),
                    qualifier: shared.qualifier.clone(),
                    initializer: initializer.clone(),
                    block: shared.block,
                    type_id: None,
                });
            }
        }
    }
    Ok(())
}

/// `name : path : type [READ_ONLY | READ_WRITE]`
fn lower_access(cx: &LowerCx, node: &SyntaxNode) -> Result<ProgramAccessDecl, Diagnostic> {
    let path = node
        .children()
        .nth(1)
        .ok_or_else(|| cx.missing(node, "a path"))?;
    let direction = significant_tokens(node).into_iter().find_map(|token| {
        DIRECTIONS
            .iter()
            .find(|(kind, _)| *kind == token.kind())
            .map(|(_, direction)| direction.clone())
    });
    Ok(ProgramAccessDecl {
        access_name: lower_name(cx, &part(cx, node, K::Name)?)?,
        symbolic_variable: lower_symbolic(cx, &path)?,
        type_name: lower_type_ref(cx, &part(cx, node, K::TypeRef)?)?,
        direction,
    })
}

/// The names of `resource.program.path...`: a name, and the member names
/// selected from it.
fn lower_path(cx: &LowerCx, node: &SyntaxNode) -> Result<Vec<Id>, Diagnostic> {
    let (base, links) = left_spine(node, |kind| kind == K::FieldExpr);
    let base = base
        .filter(|base| base.kind() == K::NameRef)
        .ok_or_else(|| {
            cx.syntax_error(
                node.text_range(),
                "expected the path `resource.program.variable`",
            )
        })?;
    let mut path = vec![lower_name(cx, &base)?];
    for link in &links {
        let selected = significant_tokens(link)
            .pop()
            .ok_or_else(|| cx.missing(link, "a name"))?;
        path.push(lower_id(cx, &selected));
    }
    Ok(path)
}

/// The resource, the program and the rest of the path an instance
/// initialisation names.
fn instance_path(cx: &LowerCx, node: &SyntaxNode) -> Result<(Id, Id, Vec<Id>), Diagnostic> {
    let first = node
        .first_child()
        .ok_or_else(|| cx.missing(node, "a path"))?;
    let path = lower_path(cx, &first)?;
    let mut names = path.into_iter();
    match (names.next(), names.next()) {
        (Some(resource), Some(program)) => Ok((resource, program, names.collect())),
        _ => Err(cx.syntax_error(
            node.text_range(),
            "expected the path `resource.program.variable`",
        )),
    }
}

/// `resource.program.path : Block := (member := value, ...)`
fn function_block_init(cx: &LowerCx, parts: &Parts) -> Result<InstanceInit, Diagnostic> {
    let (resource_name, program_name, fb_path) = instance_path(cx, &parts.node)?;
    Ok(InstanceInit::FunctionBlock(Box::new(FunctionBlockInit {
        resource_name,
        program_name,
        fb_path,
        // The path names the instance as a whole; no part of the text is the
        // name of the block alone.
        fb_name: Id::from(""),
        type_name: parts.base_name(cx)?,
        initializer: lower_struct_elements(cx, parts.value(cx)?)?,
    })))
}

/// `resource.program.path [AT address] : type [:= value]`
fn located_init(cx: &LowerCx, parts: &Parts) -> Result<InstanceInit, Diagnostic> {
    let (resource_name, program_name, fb_path) = instance_path(cx, &parts.node)?;
    let address = child_of(&parts.node, K::Location)
        .map(|location| lower_address(cx, &location))
        .transpose()?;
    Ok(InstanceInit::Located(Box::new(LocatedVarInit {
        resource_name,
        program_name,
        fb_path,
        address,
        initializer: lower_initial_value(cx, &parts.node)?,
    })))
}

/// What an initialisation in a configuration makes of its type and value: a
/// function block with the values of its members, and anything else is a
/// variable of the instance.
const INSTANCES: &[Row<InstanceInit>] = &[
    row(&[Form::Named], &[Init::Struct], function_block_init),
    row(
        &[
            Form::Array,
            Form::Subrange,
            Form::BareString,
            Form::Elementary,
            Form::Named,
        ],
        &[
            Init::None,
            Init::Array,
            Init::Qualified,
            Init::Name,
            Init::Value,
        ],
        located_init,
    ),
];

/// The qualifier a block's keyword is followed by: the first of its tokens
/// that `QUALIFIERS` has a row for, or none.
fn qualifier_of(node: &SyntaxNode) -> DeclarationQualifier {
    significant_tokens(node)
        .iter()
        .find_map(|token| {
            QUALIFIERS
                .iter()
                .find(|(kind, _)| *kind == token.kind())
                .map(|(_, qualifier)| qualifier.clone())
        })
        .unwrap_or(DeclarationQualifier::Unspecified)
}

/// Lowers a `VAR ... END_VAR` block, or any other block of the family, to what
/// its items declare.
pub fn lower_var_block(cx: &LowerCx, node: &SyntaxNode) -> Result<Block, Diagnostic> {
    expect(cx, node, K::VarBlock)?;
    let keyword = significant_tokens(node)
        .into_iter()
        .next()
        .ok_or_else(|| cx.missing(node, "a block keyword"))?;
    let (_, items) = BLOCKS
        .iter()
        .find(|(opener, _)| *opener == keyword.kind())
        .ok_or_else(|| cx.missing(node, "a block keyword"))?;
    let qualifier = qualifier_of(node);
    let mut block = Block::default();
    match items {
        Declarations(var_type) => {
            let shared = Shared {
                var_type,
                qualifier: &qualifier,
                block: next_block_id(),
            };
            for declaration in children_of(node, K::VarDecl) {
                lower_declaration(cx, &declaration, &shared, &mut block)?;
            }
        }
        Access => {
            for declaration in children_of(node, K::AccessDecl) {
                block.access.push(lower_access(cx, &declaration)?);
            }
        }
        Instances => {
            for declaration in children_of(node, K::InstanceInit) {
                block.instances.push(build(cx, INSTANCES, &declaration)?);
            }
        }
    }
    Ok(block)
}

/// Lowers a block that holds variables to its variables: the blocks a library
/// element is made of (`VAR_GLOBAL` at the top of a file).
pub fn lower_variables(cx: &LowerCx, node: &SyntaxNode) -> Result<Vec<VarDecl>, Diagnostic> {
    lower_var_block(cx, node).map(|block| block.variables)
}
