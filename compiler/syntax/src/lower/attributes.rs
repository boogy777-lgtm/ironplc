//! Attributes: which declaration each `{attribute 'name'}` pragma is attached
//! to, and which pragma is attached to none.
//!
//! A pragma is one trivia token, and the tree places the trivia before a node
//! in the node that holds it, before the node. So the pragmas that stand
//! before a node are the run of trivia tokens just before it among its
//! siblings, and one rule reads them (`claimed_by`):
//!
//! - a [`Role::Target`] node claims the attribute pragmas of the run before it;
//! - a [`Role::Group`] node that holds a target claims the attribute pragmas of
//!   the run before it, and passes them down to every target it holds;
//! - any other node claims none.
//!
//! The two readings of the rule come from this one function. A declaration
//! asks for its attributes ([`attributes_of`]): what its group passes down,
//! then what it claims. The check asks for the pragmas no node claims
//! ([`unattached`]). A pragma is therefore attached or reported, never both
//! and never neither.
//!
//! [`role`] matches every node kind without a wildcard arm: a node kind added
//! to the syntax declaration does not compile until it says whether an
//! attribute can stand before it.

use super::LowerCx;
use crate::pragma::{kind, Kind};
use crate::syntax_kind::{NodeKind, SyntaxKind, SyntaxNode, SyntaxToken};
use ironplc_dsl::attribute::Attribute;
use ironplc_dsl::common::Attributes;
use ironplc_dsl::diagnostic::{Diagnostic, Label};
use ironplc_problems::Problem;
use rowan::{NodeOrToken, TextRange};
use std::collections::HashSet;

/// What a node is to the attribute pragmas that stand before it.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Role {
    /// A declaration that has the field `attributes`.
    Target,
    /// A node that only holds declarations: the attributes before it are the
    /// attributes of each declaration it holds.
    Group,
    /// Nothing an attribute can stand before.
    Neither,
}

/// The role of a node kind. There is no wildcard arm: a node kind added to
/// the syntax declaration fails to compile here until someone decides it.
pub fn node_role(node: NodeKind) -> Role {
    use Role::{Group, Neither, Target};
    match node {
        NodeKind::TypeDecl
        | NodeKind::StructMember
        | NodeKind::VarDecl
        | NodeKind::ProgramDecl
        | NodeKind::FunctionDecl
        | NodeKind::FunctionBlockDecl
        | NodeKind::InterfaceDecl
        | NodeKind::MethodDecl
        | NodeKind::PropertyDecl
        | NodeKind::ActionDecl => Target,
        NodeKind::TypeBlock | NodeKind::VarBlock => Group,
        NodeKind::SourceFile
        | NodeKind::ErrorNode
        | NodeKind::NamespaceDecl
        | NodeKind::GetAccessor
        | NodeKind::SetAccessor
        | NodeKind::MemberQualifier
        | NodeKind::ExtendsClause
        | NodeKind::ImplementsClause
        | NodeKind::Name
        | NodeKind::Location
        | NodeKind::EdgeSpec
        | NodeKind::AccessDecl
        | NodeKind::InstanceInit => Neither,
        NodeKind::ArrayType
        | NodeKind::Subrange
        | NodeKind::StringType
        | NodeKind::RefType
        | NodeKind::ParamsType
        | NodeKind::SubrangeType
        | NodeKind::EnumType
        | NodeKind::EnumValue
        | NodeKind::StructType
        | NodeKind::UnionType => Neither,
        NodeKind::Initializer
        | NodeKind::ArrayInit
        | NodeKind::RepeatedInit
        | NodeKind::StructInit
        | NodeKind::StructInitElement
        | NodeKind::EnumValueRef => Neither,
        NodeKind::ConfigurationDecl
        | NodeKind::ResourceDecl
        | NodeKind::TaskDecl
        | NodeKind::TaskInit
        | NodeKind::TaskInitItem
        | NodeKind::ProgramConfig
        | NodeKind::TaskBinding
        | NodeKind::ProgramConnection => Neither,
        NodeKind::SfcBody
        | NodeKind::InitialStepDecl
        | NodeKind::StepDecl
        | NodeKind::ActionAssociation
        | NodeKind::ActionQualifier
        | NodeKind::TransitionDecl
        | NodeKind::TransitionPriority
        | NodeKind::StepList
        | NodeKind::TransitionCondition => Neither,
        NodeKind::StatementList
        | NodeKind::EmptyStmt
        | NodeKind::AssignStmt
        | NodeKind::AssignOp
        | NodeKind::CallStmt
        | NodeKind::IfStmt
        | NodeKind::ElsifClause
        | NodeKind::ElseClause
        | NodeKind::CaseStmt
        | NodeKind::CaseBranch
        | NodeKind::CaseLabel
        | NodeKind::ForStmt
        | NodeKind::WhileStmt
        | NodeKind::RepeatStmt
        | NodeKind::ExitStmt
        | NodeKind::ContinueStmt
        | NodeKind::ReturnStmt
        | NodeKind::TryStmt
        | NodeKind::CatchClause
        | NodeKind::FinallyClause
        | NodeKind::ThrowStmt
        | NodeKind::JmpStmt
        | NodeKind::LabelStmt
        | NodeKind::CalcStmt
        | NodeKind::WaitStmt
        | NodeKind::ImplementationMarker => Neither,
        NodeKind::BoolLiteral
        | NodeKind::IntLiteral
        | NodeKind::RealLiteral
        | NodeKind::BitStringLiteral
        | NodeKind::StringLiteral
        | NodeKind::DurationLiteral
        | NodeKind::TimeOfDayLiteral
        | NodeKind::DateLiteral
        | NodeKind::DateTimeLiteral
        | NodeKind::NullLiteral
        | NodeKind::TypeRef
        | NodeKind::BinaryExpr
        | NodeKind::UnaryExpr
        | NodeKind::ParenExpr
        | NodeKind::CallExpr
        | NodeKind::RefExpr
        | NodeKind::SpecialOpExpr
        | NodeKind::ArgList
        | NodeKind::PositionalArg
        | NodeKind::NamedArg
        | NodeKind::OutputArg
        | NodeKind::NameRef
        | NodeKind::FieldExpr
        | NodeKind::IndexExpr
        | NodeKind::BitAccessExpr
        | NodeKind::PartialAccessExpr
        | NodeKind::DerefExpr
        | NodeKind::SelfRefExpr
        | NodeKind::DirectAddressExpr => Neither,
    }
}

/// The role of a node of kind `kind`; a token has none.
fn role(kind: SyntaxKind) -> Role {
    kind.node().map_or(Role::Neither, node_role)
}

/// The name and the value of an attribute pragma, or `None` for any other
/// token or kind of pragma.
fn attribute_of(token: &SyntaxToken) -> Option<(String, Option<String>)> {
    if token.kind() != SyntaxKind::Pragma {
        return None;
    }
    match kind(token.text()) {
        Kind::Attribute { name, value } => Some((name, value)),
        Kind::Directive(_) | Kind::Message { .. } | Kind::Other => None,
    }
}

/// The attribute pragmas in the run of trivia just before `node` among its
/// siblings, in source order.
fn run_before(node: &SyntaxNode) -> Vec<SyntaxToken> {
    let mut run = Vec::new();
    let mut previous = node.prev_sibling_or_token();
    while let Some(NodeOrToken::Token(token)) = previous {
        if !token.kind().is_trivia() {
            break;
        }
        previous = token.prev_sibling_or_token();
        if attribute_of(&token).is_some() {
            run.push(token);
        }
    }
    run.reverse();
    run
}

/// The attribute pragmas `node` claims for itself, which is the only reading
/// of what stands before a node: see the module documentation.
fn claimed_by(node: &SyntaxNode) -> Vec<SyntaxToken> {
    match role(node.kind()) {
        Role::Target => run_before(node),
        Role::Group
            if node
                .children()
                .any(|child| role(child.kind()) == Role::Target) =>
        {
            run_before(node)
        }
        Role::Group | Role::Neither => Vec::new(),
    }
}

/// The attributes attached to the declaration at `node`: those its group
/// passes down, then its own, each in source order. A node that is not a
/// target has none.
pub fn attributes_of(cx: &LowerCx, node: &SyntaxNode) -> Attributes {
    if role(node.kind()) != Role::Target {
        return Attributes::default();
    }
    let passed = node
        .parent()
        .filter(|parent| role(parent.kind()) == Role::Group)
        .map(|group| claimed_by(&group))
        .unwrap_or_default();
    Attributes::new(
        passed
            .into_iter()
            .chain(claimed_by(node))
            .filter_map(|token| {
                attribute_of(&token).map(|(name, value)| Attribute {
                    name,
                    value,
                    span: cx.token_span(&token),
                })
            })
            .collect(),
    )
}

/// The attribute pragmas under `root` that no node claims, in source order.
pub fn unattached(root: &SyntaxNode) -> Vec<SyntaxToken> {
    let pragmas: Vec<SyntaxToken> = root
        .descendants_with_tokens()
        .filter_map(NodeOrToken::into_token)
        .filter(|token| attribute_of(token).is_some())
        .collect();
    if pragmas.is_empty() {
        return pragmas;
    }
    let claimed: HashSet<TextRange> = root
        .descendants()
        .flat_map(|node| claimed_by(&node))
        .map(|token| token.text_range())
        .collect();
    pragmas
        .into_iter()
        .filter(|token| !claimed.contains(&token.text_range()))
        .collect()
}

/// Fails with the first attribute pragma under `root` that is attached to
/// nothing (P0043): an accepted attribute is never without a declaration by
/// accident.
pub fn reject_unattached(cx: &LowerCx, root: &SyntaxNode) -> Result<(), Diagnostic> {
    match unattached(root).first() {
        None => Ok(()),
        Some(token) => {
            let name = attribute_of(token)
                .map(|(name, _)| name)
                .unwrap_or_default();
            Err(Diagnostic::problem(
                Problem::AttributeNotAttached,
                Label::span(
                    cx.token_span(token),
                    format!("The attribute '{name}' is not followed by a declaration it can be attached to"),
                ),
            )
            .with_help(
                "Write the attribute pragma directly before the declaration it describes: a type, a variable, a structure member, a program, a function, a function block, an interface, a method, a property or an action",
            ))
        }
    }
}

#[cfg(test)]
mod reported_tests;
#[cfg(test)]
mod tests;
