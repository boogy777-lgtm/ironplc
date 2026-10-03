//! Variables: the node kinds that name a place to [`Variable`].
//!
//! A variable is a head (a name, `THIS^` or `SUPER^`, or a direct address)
//! with a chain of selectors written after it: a member, a subscript, a bit,
//! a partial access, a dereference. The tree nests each selector around what
//! it selects from, so the chain is as deep as it is long; `lower_symbolic`
//! walks it once from the head outward (`left_spine`) and applies each
//! selector, so no rule recurses along a chain.
//!
//! The selectors are the rows of `SELECTORS`, one per node kind, each building
//! the object the legacy grammar builds for it. A partial-access selector is
//! one token (`%X3`, `%W1`), and the letter after the `%` is a row of
//! `PARTIAL_ACCESS`: the bit selector `%X` is a bit access, the others name
//! the width of the part.
//!
//! The span of a variable is derived by the objects from the identifiers and
//! selectors they hold. What is stored is the position of every name and of a
//! self reference, the span of the index of a bit or partial access, and the
//! extent of a subscript and of a dereference: the closing bracket and the
//! caret belong to no part, so those two objects carry the whole of what was
//! written.

use super::expressions::lower_expr;
use super::names::{lower_id, lower_name};
use super::tree::{left_spine, significant_tokens, token_of};
use super::LowerCx;
use crate::syntax_kind::{SyntaxKind as K, SyntaxNode, SyntaxToken};
use ironplc_dsl::common::{AddressAssignment, Integer};
use ironplc_dsl::core::SourceSpan;
use ironplc_dsl::diagnostic::Diagnostic;
use ironplc_dsl::textual::{
    ArrayVariable, BitAccessVariable, DerefVariable, NamedVariable, PartialAccessSize,
    PartialAccessVariable, SelfRefKind, SelfRefVariable, StructuredVariable, SymbolicVariableKind,
    Variable,
};
use rowan::{TextRange, TextSize};

#[cfg(test)]
mod tests;

/// A rule: applies the selector a node writes to the variable it selects from.
type Selector =
    fn(&LowerCx, &SyntaxNode, SymbolicVariableKind) -> Result<SymbolicVariableKind, Diagnostic>;

/// The selector each kind of node writes.
const SELECTORS: &[(K, Selector)] = &[
    (K::FieldExpr, member),
    (K::IndexExpr, subscript),
    (K::BitAccessExpr, bit),
    (K::PartialAccessExpr, partial),
    (K::DerefExpr, dereference),
];

/// What a selector letter after `%` selects.
enum Selected {
    /// One bit.
    Bit,
    /// A byte, word, double word or long word.
    Part(PartialAccessSize),
}

/// The selector letters of a partial access.
const PARTIAL_ACCESS: &[(char, Selected)] = &[
    ('X', Selected::Bit),
    ('B', Selected::Part(PartialAccessSize::Byte)),
    ('W', Selected::Part(PartialAccessSize::Word)),
    ('D', Selected::Part(PartialAccessSize::DWord)),
    ('L', Selected::Part(PartialAccessSize::LWord)),
];

/// The implicit instance a keyword names.
const SELF_REFERENCES: &[(K, SelfRefKind)] =
    &[(K::This, SelfRefKind::This), (K::Super, SelfRefKind::Super)];

/// True for the kinds of node that write a selector.
fn is_selector(kind: K) -> bool {
    SELECTORS.iter().any(|(selector, _)| *selector == kind)
}

/// The last significant token directly under `node`: the name or number a
/// selector ends with.
fn last_token(cx: &LowerCx, node: &SyntaxNode) -> Result<SyntaxToken, Diagnostic> {
    significant_tokens(node)
        .pop()
        .ok_or_else(|| cx.missing(node, "its selector"))
}

/// A number written in digits, positioned at `span`.
fn index_number(
    cx: &LowerCx,
    node: &SyntaxNode,
    digits: &str,
    span: SourceSpan,
) -> Result<Integer, Diagnostic> {
    Integer::new(digits, span).map_err(|why| {
        cx.syntax_error(
            node.text_range(),
            format!("the index '{digits}' is not valid: {why}"),
        )
    })
}

/// `record.field`.
fn member(
    cx: &LowerCx,
    node: &SyntaxNode,
    record: SymbolicVariableKind,
) -> Result<SymbolicVariableKind, Diagnostic> {
    let field = lower_id(cx, &last_token(cx, node)?);
    Ok(SymbolicVariableKind::Structured(StructuredVariable {
        record: Box::new(record),
        field,
    }))
}

/// `array[a, b, ...]`: the nodes after the first are the subscripts.
fn subscript(
    cx: &LowerCx,
    node: &SyntaxNode,
    array: SymbolicVariableKind,
) -> Result<SymbolicVariableKind, Diagnostic> {
    let subscripts = node
        .children()
        .skip(1)
        .map(|index| lower_expr(cx, &index))
        .collect::<Result<Vec<_>, _>>()?;
    if subscripts.is_empty() {
        return Err(cx.missing(node, "a subscript"));
    }
    Ok(SymbolicVariableKind::Array(ArrayVariable {
        subscripted_variable: Box::new(array),
        subscripts,
        span: cx.node_span(node),
    }))
}

/// `variable.3`.
fn bit(
    cx: &LowerCx,
    node: &SyntaxNode,
    variable: SymbolicVariableKind,
) -> Result<SymbolicVariableKind, Diagnostic> {
    let token = last_token(cx, node)?;
    let index = index_number(cx, node, token.text(), cx.token_span(&token))?;
    Ok(SymbolicVariableKind::BitAccess(BitAccessVariable {
        variable: Box::new(variable),
        index,
    }))
}

/// `variable.%X3`, `variable.%W1`: the letter selects a bit or a part, the
/// digits after it are the index, and the index is positioned at the digits.
fn partial(
    cx: &LowerCx,
    node: &SyntaxNode,
    variable: SymbolicVariableKind,
) -> Result<SymbolicVariableKind, Diagnostic> {
    let token = last_token(cx, node)?;
    let text = token.text();
    let selector = text
        .chars()
        .nth(1)
        .map(|letter| letter.to_ascii_uppercase());
    let selected = PARTIAL_ACCESS
        .iter()
        .find(|(letter, _)| Some(*letter) == selector)
        .map(|(_, selected)| selected)
        .ok_or_else(|| cx.missing(node, "a partial-access selector"))?;
    let range = token.text_range();
    let digits = TextRange::new(range.start() + TextSize::from(2), range.end());
    let index = index_number(cx, node, text.get(2..).unwrap_or_default(), cx.span(digits))?;
    let variable = Box::new(variable);
    Ok(match selected {
        Selected::Bit => SymbolicVariableKind::BitAccess(BitAccessVariable { variable, index }),
        Selected::Part(size) => SymbolicVariableKind::PartialAccess(PartialAccessVariable {
            variable,
            size: *size,
            index,
        }),
    })
}

/// `variable^`.
fn dereference(
    cx: &LowerCx,
    node: &SyntaxNode,
    variable: SymbolicVariableKind,
) -> Result<SymbolicVariableKind, Diagnostic> {
    Ok(SymbolicVariableKind::Deref(DerefVariable {
        variable: Box::new(variable),
        span: cx.node_span(node),
    }))
}

/// Lowers `THIS^` or `SUPER^`: the keyword and its caret, positioned as a
/// whole.
pub fn lower_self_ref(cx: &LowerCx, node: &SyntaxNode) -> Result<SelfRefVariable, Diagnostic> {
    if node.kind() != K::SelfRefExpr {
        return Err(cx.unsupported(node));
    }
    let keywords: Vec<K> = SELF_REFERENCES
        .iter()
        .map(|(keyword, _)| *keyword)
        .collect();
    let token = token_of(node, &keywords).ok_or_else(|| cx.missing(node, "THIS or SUPER"))?;
    let kind = SELF_REFERENCES
        .iter()
        .find(|(keyword, _)| *keyword == token.kind())
        .map(|(_, kind)| *kind)
        .ok_or_else(|| cx.missing(node, "THIS or SUPER"))?;
    Ok(SelfRefVariable {
        kind,
        position: cx.node_span(node),
    })
}

/// The address a direct-address token spells, positioned at the token.
fn direct_address(cx: &LowerCx, node: &SyntaxNode) -> Result<AddressAssignment, Diagnostic> {
    let token = last_token(cx, node)?;
    AddressAssignment::try_from(token.text())
        .map(|address| address.with_position(cx.token_span(&token)))
        .map_err(|why| {
            cx.syntax_error(
                node.text_range(),
                format!("'{}' is not a direct address: {why}", token.text()),
            )
        })
}

/// Lowers a chain of selectors, and the head it starts from, to the symbolic
/// variable it names.
pub fn lower_symbolic(cx: &LowerCx, node: &SyntaxNode) -> Result<SymbolicVariableKind, Diagnostic> {
    let (base, links) = left_spine(node, is_selector);
    let base = base.ok_or_else(|| cx.missing(node, "a variable to select from"))?;
    let mut variable = match base.kind() {
        K::NameRef => SymbolicVariableKind::Named(NamedVariable {
            name: lower_name(cx, &base)?,
        }),
        K::SelfRefExpr => SymbolicVariableKind::SelfRef(lower_self_ref(cx, &base)?),
        _ => return Err(cx.unsupported(&base)),
    };
    for link in &links {
        let selector = SELECTORS
            .iter()
            .find(|(kind, _)| *kind == link.kind())
            .map(|(_, selector)| selector)
            .ok_or_else(|| cx.unsupported(link))?;
        variable = selector(cx, link, variable)?;
    }
    Ok(variable)
}

/// Lowers a node that names a place: a direct address, or a symbolic variable.
pub fn lower_variable(cx: &LowerCx, node: &SyntaxNode) -> Result<Variable, Diagnostic> {
    match node.kind() {
        K::DirectAddressExpr => direct_address(cx, node).map(Variable::Direct),
        _ => lower_symbolic(cx, node).map(Variable::Symbolic),
    }
}
