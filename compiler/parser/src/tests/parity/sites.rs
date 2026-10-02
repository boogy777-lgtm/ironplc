//! Where a lowered object is compared to the legacy one: the sites of a tree.
//!
//! A site is a node that the lowering reads by itself, and that the legacy
//! grammar reads with a rule of its own: a literal (`constant`), an operand or
//! an argument or a condition (`expression`), a place that is written to or
//! referred to (`variable`). The legacy rule is applied to the legacy tokens of
//! the same bytes, which is a fair oracle for a node only when the node means
//! the same thing outside its parent. A name that heads a member access, a
//! subscript or a call is not a site: it is read with its parent, which is.
//!
//! `site_unit` is the one table of which child of which parent is read by which
//! rule; a construct whose children are read differently is a row.

use ironplc_syntax::lower::{disposition, Area, Disposition};
use ironplc_syntax::{SyntaxKind as K, SyntaxNode};

/// What is compared at a site, and so which legacy rule is its oracle.
#[derive(Clone, Copy, PartialEq, Eq, Debug)]
pub enum Unit {
    Literal,
    Expression,
    Variable,
}

impl Unit {
    pub const ALL: [Unit; 3] = [Unit::Literal, Unit::Expression, Unit::Variable];

    pub fn name(self) -> &'static str {
        match self {
            Unit::Literal => "literals",
            Unit::Expression => "expressions",
            Unit::Variable => "variables",
        }
    }
}

/// A node, the kind of its parent, and what is compared there.
pub struct Site {
    pub node: SyntaxNode,
    pub parent: K,
    pub unit: Unit,
}

/// Which children of a parent are sites, and of what unit.
enum Children {
    /// Every child that has a value.
    All(Unit),
    /// Every child that has a value, except a literal.
    ExceptLiterals(Unit),
    /// Every child that has a value after the first, which is a name.
    AfterName(Unit),
}

/// The parents whose children are sites. A parent not listed has none: the
/// children of a member access, a call or a dereference are read with it. The
/// operand of a unary operator is a primary expression, and a signed literal is
/// one that the expression rule reads as a negation, so a literal operand is
/// read with its operator.
const PARENTS: &[(K, Children)] = &[
    (K::SourceFile, Children::All(Unit::Expression)),
    (K::BinaryExpr, Children::All(Unit::Expression)),
    (K::ParenExpr, Children::All(Unit::Expression)),
    (K::UnaryExpr, Children::ExceptLiterals(Unit::Expression)),
    (K::PositionalArg, Children::All(Unit::Expression)),
    (K::SpecialOpExpr, Children::All(Unit::Expression)),
    (K::IfStmt, Children::All(Unit::Expression)),
    (K::ElsifClause, Children::All(Unit::Expression)),
    (K::CaseStmt, Children::All(Unit::Expression)),
    (K::WhileStmt, Children::All(Unit::Expression)),
    (K::RepeatStmt, Children::All(Unit::Expression)),
    (K::RefExpr, Children::All(Unit::Variable)),
    (K::NamedArg, Children::AfterName(Unit::Expression)),
    (K::IndexExpr, Children::AfterName(Unit::Expression)),
    (K::ForStmt, Children::AfterName(Unit::Expression)),
    (K::OutputArg, Children::AfterName(Unit::Variable)),
];

/// True for a node that has a value: a literal, an operation, or a place.
fn has_value(kind: K) -> bool {
    matches!(
        disposition(kind),
        Disposition::Lowered(Area::Literal | Area::Expression | Area::Variable)
    )
}

fn is_literal(kind: K) -> bool {
    disposition(kind) == Disposition::Lowered(Area::Literal)
}

/// The position of a node among the nodes of its parent.
fn index_of(node: &SyntaxNode) -> usize {
    std::iter::successors(node.prev_sibling(), SyntaxNode::prev_sibling).count()
}

/// The text of an assignment's operator, upper-cased.
fn operator_text(assignment: &SyntaxNode) -> String {
    assignment
        .children()
        .find(|child| child.kind() == K::AssignOp)
        .map(|operator| {
            operator
                .text()
                .to_string()
                .chars()
                .filter(|letter| !letter.is_whitespace())
                .collect::<String>()
                .to_ascii_uppercase()
        })
        .unwrap_or_default()
}

/// The unit compared at `node` according to its parent, if it is a site.
///
/// An assignment is the one parent whose children differ by position: the
/// target and the value of a reference binding (`REF=`) are places, the value
/// of any other assignment is an expression. A target that is a bare
/// dereference (`p^ := v`) is the assignment's own dereference flag, read with
/// the assignment.
fn site_unit(parent: &SyntaxNode, node: &SyntaxNode) -> Option<Unit> {
    if !has_value(node.kind()) {
        return None;
    }
    if parent.kind() == K::AssignStmt {
        return match (index_of(node), node.kind()) {
            (0, K::DerefExpr) => None,
            (0, _) => Some(Unit::Variable),
            (_, _) if operator_text(parent).ends_with("REF=") => Some(Unit::Variable),
            _ => Some(Unit::Expression),
        };
    }
    let (_, children) = PARENTS.iter().find(|(kind, _)| *kind == parent.kind())?;
    match children {
        Children::All(unit) => Some(*unit),
        Children::ExceptLiterals(unit) => (!is_literal(node.kind())).then_some(*unit),
        Children::AfterName(unit) => (index_of(node) > 0).then_some(*unit),
    }
}

/// Every site under `root`, outer before inner and earlier before later. A
/// literal that is the child of a parent with a rule of its own is a site
/// twice, first for the parent's rule and then as a literal.
pub fn sites(root: &SyntaxNode) -> Vec<Site> {
    let mut found = Vec::new();
    for node in root.descendants().skip(1) {
        let Some(parent) = node.parent() else {
            continue;
        };
        if let Some(unit) = site_unit(&parent, &node) {
            found.push(Site {
                node: node.clone(),
                parent: parent.kind(),
                unit,
            });
        }
        if is_literal(node.kind()) {
            found.push(Site {
                node,
                parent: parent.kind(),
                unit: Unit::Literal,
            });
        }
    }
    found
}
