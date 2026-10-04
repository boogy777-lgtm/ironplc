//! Where a lowered object is compared to the legacy one: the sites of a tree.
//!
//! A site is a node that the lowering reads by itself, and that the legacy
//! grammar reads with a rule of its own: a literal (`constant`), an operand or
//! an argument or a condition (`expression`), a place that is written to or
//! referred to (`variable`), a statement (`statement_list` over its tokens),
//! a list of statements, a unit, an interface or a namespace (`library` over its
//! tokens) and a method or a property of a function block
//! (`function_block_member`). The legacy rule is applied to the legacy tokens of the
//! same bytes, which is a fair oracle for a node only when the node means the
//! same thing outside its parent. A name that heads a member access, a
//! subscript or a call is not a site: it is read with its parent, which is.
//!
//! `PARTS` is the one table of which child of which parent is read by which
//! rule: a part names the parent, where the child stands, what is compared
//! there and the kinds of child that are read with the parent after all. A
//! construct whose children are read differently is a row; none is code.

use ironplc_syntax::lower::{disposition, Area, Disposition};
use ironplc_syntax::{SyntaxKind as K, SyntaxNode};

/// What is compared at a site, and so which legacy rule is its oracle.
#[derive(Clone, Copy, PartialEq, Eq, Debug)]
pub enum Unit {
    Literal,
    Expression,
    Variable,
    Statement,
    Statements,
    TypeDeclaration,
    VariableInitial,
    VariableBlock,
    VariableBlockFacts,
    Pou,
    Member,
}

impl Unit {
    pub const ALL: [Unit; 11] = [
        Unit::Literal,
        Unit::Expression,
        Unit::Variable,
        Unit::Statement,
        Unit::Statements,
        Unit::TypeDeclaration,
        Unit::VariableInitial,
        Unit::VariableBlock,
        Unit::VariableBlockFacts,
        Unit::Pou,
        Unit::Member,
    ];

    pub fn name(self) -> &'static str {
        match self {
            Unit::Literal => "literals",
            Unit::Expression => "expressions",
            Unit::Variable => "variables",
            Unit::Statement => "statements",
            Unit::Statements => "statement lists",
            Unit::TypeDeclaration => "type declarations",
            Unit::VariableInitial => "variable initial values",
            Unit::VariableBlock => "variable blocks",
            Unit::VariableBlockFacts => "variable blocks, initial values left out",
            Unit::Pou => "units",
            Unit::Member => "members",
        }
    }

    /// True for a unit that compares part of what another unit compares of
    /// the same node: it is a view of the node, not a part of it.
    pub fn is_view(self) -> bool {
        self == Unit::VariableBlockFacts
    }

    /// True for a node of `kind` that this unit compares.
    fn holds(self, kind: K) -> bool {
        match self {
            Unit::Literal => is_literal(kind),
            Unit::Expression | Unit::Variable => matches!(
                disposition(kind),
                Disposition::Lowered(Area::Literal | Area::Expression | Area::Variable)
            ),
            Unit::Statement => {
                kind != K::StatementList
                    && disposition(kind) == Disposition::Lowered(Area::Statement)
            }
            Unit::Statements => kind == K::StatementList,
            Unit::TypeDeclaration => kind == K::TypeDecl,
            Unit::VariableInitial => kind == K::VarDecl,
            Unit::VariableBlock | Unit::VariableBlockFacts => kind == K::VarBlock,
            Unit::Pou => disposition(kind) == Disposition::Lowered(Area::Unit),
            Unit::Member => disposition(kind) == Disposition::Lowered(Area::Member),
        }
    }

    /// True when the legacy tokens of the construct end with the terminator
    /// that the legacy pipeline inserts after a block statement written
    /// without its `;`: the construct is read with it.
    pub fn ends_in_terminator(self) -> bool {
        matches!(self, Unit::Statement | Unit::Statements)
    }
}

/// A node, the kind of its parent, and what is compared there.
pub struct Site {
    pub node: SyntaxNode,
    pub parent: K,
    pub unit: Unit,
}

/// Where a child stands among the children of its parent.
enum Place {
    /// Anywhere.
    Every,
    /// After the first child, which is a name.
    AfterFirst,
    /// Before the last child.
    BeforeLast,
    /// Before the child of this kind.
    Before(K),
    /// After the child of this kind and, when a spelling is given, only when
    /// that child is written as it (upper case, without spaces).
    After(K, Option<&'static str>),
    /// In a parent that is introduced by one of these keywords.
    InBlock(&'static [K]),
}

/// The kinds of child that a row reads with the parent after all.
enum Except {
    Nothing,
    Literals,
    Kinds(&'static [K]),
    /// A child that has a part of one of these kinds.
    Having(&'static [K]),
}

/// A row: the children of a parent that stand at a place are compared as a
/// unit.
struct Part {
    parent: K,
    place: Place,
    unit: Unit,
    except: Except,
}

const fn part(parent: K, place: Place, unit: Unit) -> Part {
    Part {
        parent,
        place,
        unit,
        except: Except::Nothing,
    }
}

/// The parents whose children are sites; the first row whose place holds and
/// whose unit compares the child decides. A parent not listed has none: the
/// children of a member access, a call or a dereference are read with it. The
/// operand of a unary operator is a primary expression, and a signed literal is
/// one that the expression rule reads as a negation, so a literal operand is
/// read with its operator. The target of an assignment through a pointer
/// (`p^ := v`) is the assignment's own dereference flag, read with the
/// assignment; the value of a reference binding is a place.
const PARTS: &[Part] = &[
    part(K::SourceFile, Place::Every, Unit::Expression),
    part(K::BinaryExpr, Place::Every, Unit::Expression),
    part(K::ParenExpr, Place::Every, Unit::Expression),
    Part {
        except: Except::Literals,
        ..part(K::UnaryExpr, Place::Every, Unit::Expression)
    },
    part(K::PositionalArg, Place::Every, Unit::Expression),
    part(K::SpecialOpExpr, Place::Every, Unit::Expression),
    part(K::RefExpr, Place::Every, Unit::Variable),
    part(K::NamedArg, Place::AfterFirst, Unit::Expression),
    part(K::IndexExpr, Place::AfterFirst, Unit::Expression),
    part(K::OutputArg, Place::AfterFirst, Unit::Variable),
    Part {
        except: Except::Kinds(&[K::DerefExpr]),
        ..part(K::AssignStmt, Place::Before(K::AssignOp), Unit::Variable)
    },
    part(
        K::AssignStmt,
        Place::After(K::AssignOp, Some("REF=")),
        Unit::Variable,
    ),
    part(
        K::AssignStmt,
        Place::After(K::AssignOp, None),
        Unit::Expression,
    ),
    part(K::IfStmt, Place::Every, Unit::Expression),
    part(K::ElsifClause, Place::Every, Unit::Expression),
    part(K::CaseStmt, Place::Every, Unit::Expression),
    part(K::WhileStmt, Place::Every, Unit::Expression),
    part(K::RepeatStmt, Place::Every, Unit::Expression),
    part(K::ForStmt, Place::AfterFirst, Unit::Expression),
    part(K::CatchClause, Place::Every, Unit::Variable),
    part(K::JmpStmt, Place::BeforeLast, Unit::Expression),
    part(K::CalcStmt, Place::BeforeLast, Unit::Expression),
    part(K::WaitStmt, Place::Every, Unit::Expression),
    part(K::ThrowStmt, Place::Every, Unit::Expression),
    part(K::StatementList, Place::Every, Unit::Statement),
    Part {
        except: Except::Having(&[K::Location, K::EdgeSpec]),
        ..part(
            K::VarBlock,
            Place::InBlock(PLAIN_BLOCKS),
            Unit::VariableInitial,
        )
    },
];

/// The blocks whose declarations the legacy grammar reads with one rule
/// (`names : type [:= value]`, the same initial value for every name). The
/// blocks that read a declaration in a form of their own are not compared: a
/// declaration with a location, an edge, or one in an argument, global,
/// external or configuration block.
const PLAIN_BLOCKS: &[K] = &[
    K::Var,
    K::VarInput,
    K::VarOutput,
    K::VarTemp,
    K::VarStat,
    K::VarInst,
    K::VarGeneric,
];

/// Nodes that the rule of their parent reads in place of the rule of their
/// own unit: a bit-string literal in a `CASE` label is read by the label's
/// rule, which builds an untyped bit string where the rule for a constant reads
/// the same tokens as an integer.
const READ_BY_PARENT: &[(K, K)] = &[(K::CaseLabel, K::BitStringLiteral)];

/// The units of which a node of the kind is a site wherever it stands,
/// whatever its parent: a node is a site of every unit that compares its kind
/// (a block is compared whole, and with its initial values left out).
fn standalone(kind: K) -> impl Iterator<Item = Unit> {
    [
        Unit::Literal,
        Unit::Statements,
        Unit::TypeDeclaration,
        Unit::VariableBlock,
        Unit::VariableBlockFacts,
        Unit::Pou,
        Unit::Member,
    ]
    .into_iter()
    .filter(move |unit| unit.holds(kind))
}

fn is_literal(kind: K) -> bool {
    disposition(kind) == Disposition::Lowered(Area::Literal)
}

/// The position of a node among the nodes of its parent.
fn index_of(node: &SyntaxNode) -> usize {
    std::iter::successors(node.prev_sibling(), SyntaxNode::prev_sibling).count()
}

/// The text of the first child of `parent` of `kind`, upper-cased and without
/// whitespace.
fn spelling_of(parent: &SyntaxNode, kind: K) -> Option<String> {
    parent
        .children()
        .find(|child| child.kind() == kind)
        .map(|marker| {
            marker
                .text()
                .to_string()
                .chars()
                .filter(|letter| !letter.is_whitespace())
                .collect::<String>()
                .to_ascii_uppercase()
        })
}

impl Place {
    fn holds(&self, parent: &SyntaxNode, node: &SyntaxNode) -> bool {
        let index = index_of(node);
        let marker = |kind: K| parent.children().position(|child| child.kind() == kind);
        match self {
            Place::Every => true,
            Place::AfterFirst => index > 0,
            Place::BeforeLast => node.next_sibling().is_some(),
            Place::Before(kind) => marker(*kind).is_some_and(|at| index < at),
            Place::After(kind, spelling) => {
                marker(*kind).is_some_and(|at| index > at)
                    && spelling.is_none_or(|wanted| {
                        spelling_of(parent, *kind).is_some_and(|written| written == wanted)
                    })
            }
            Place::InBlock(keywords) => parent
                .children_with_tokens()
                .filter_map(|element| element.into_token())
                .find(|token| !token.kind().is_trivia())
                .is_some_and(|keyword| keywords.contains(&keyword.kind())),
        }
    }
}

impl Except {
    fn covers(&self, node: &SyntaxNode) -> bool {
        let kind = node.kind();
        match self {
            Except::Nothing => false,
            Except::Literals => is_literal(kind),
            Except::Kinds(kinds) => kinds.contains(&kind),
            Except::Having(parts) => node.children().any(|part| parts.contains(&part.kind())),
        }
    }
}

/// The unit compared at `node` according to its parent, if it is a site.
fn site_unit(parent: &SyntaxNode, node: &SyntaxNode) -> Option<Unit> {
    let part = PARTS.iter().find(|part| {
        part.parent == parent.kind()
            && part.unit.holds(node.kind())
            && part.place.holds(parent, node)
    })?;
    (!part.except.covers(node)).then_some(part.unit)
}

/// Every site under `root`, outer before inner and earlier before later. A
/// node that is a site wherever it stands (a literal, a list) and is the child
/// of a parent with a row of its own is a site twice, first for the parent's
/// rule and then as itself.
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
        let read_by_parent = READ_BY_PARENT.contains(&(parent.kind(), node.kind()));
        if !read_by_parent {
            for unit in standalone(node.kind()) {
                found.push(Site {
                    node: node.clone(),
                    parent: parent.kind(),
                    unit,
                });
            }
        }
    }
    found
}

#[cfg(test)]
mod tests {
    use super::*;
    use ironplc_syntax::{parse_source_file, parse_statements, ParseOptions};

    /// The units compared at the sites of `source`, with the text of each.
    fn compared(source: &str) -> Vec<(Unit, String)> {
        let parse = parse_statements(source, &ParseOptions::all());
        assert!(parse.is_ok(), "{source}: {:?}", parse.errors);
        sites(&parse.root)
            .into_iter()
            .map(|site| (site.unit, site.node.text().to_string()))
            .collect()
    }

    fn units_of(source: &str, text: &str) -> Vec<Unit> {
        compared(source)
            .into_iter()
            .filter(|(_, node)| node == text)
            .map(|(unit, _)| unit)
            .collect()
    }

    #[test]
    fn sites_when_assignment_then_the_target_is_a_variable_and_the_value_an_expression() {
        let source = "a[1] := b;";
        assert_eq!(units_of(source, "a[1]"), vec![Unit::Variable]);
        assert_eq!(units_of(source, "b"), vec![Unit::Expression]);
    }

    #[test]
    fn sites_when_reference_binding_then_the_value_is_a_variable() {
        let source = "x REF= y;";
        assert_eq!(units_of(source, "x"), vec![Unit::Variable]);
        assert_eq!(units_of(source, "y"), vec![Unit::Variable]);
        // The other binds take a value.
        assert_eq!(units_of("x S= y;", "y"), vec![Unit::Expression]);
    }

    #[test]
    fn sites_when_assignment_through_a_pointer_then_the_target_is_read_with_the_assignment() {
        let source = "p^ := 1;";
        assert!(units_of(source, "p^").is_empty());
        assert!(units_of(source, "p").is_empty());
        assert_eq!(units_of(source, "1"), vec![Unit::Expression, Unit::Literal]);
        // A caret inside the target is a part of it.
        assert_eq!(units_of("p^.x := 1;", "p^.x"), vec![Unit::Variable]);
    }

    #[test]
    fn sites_when_case_label_then_a_bit_string_is_read_by_the_label_and_a_number_is_a_literal() {
        let source = "CASE x OF 16#FF: a := 1; 7: b := 2; END_CASE;";
        assert!(units_of(source, "16#FF").is_empty());
        assert_eq!(units_of(source, "7"), vec![Unit::Literal]);
    }

    #[test]
    fn sites_when_statement_list_then_the_list_and_each_statement_in_it_are_sites() {
        let source = "x := 1; IF a THEN y := 2; END_IF;";
        let units = compared(source);
        let count = |wanted: Unit| units.iter().filter(|(unit, _)| *unit == wanted).count();
        // The top list, the `THEN` list; `x := 1;`, the `IF`, and `y := 2;`.
        assert_eq!(count(Unit::Statements), 2);
        assert_eq!(count(Unit::Statement), 3);
        assert_eq!(
            units_of(source, "IF a THEN y := 2; END_IF;"),
            vec![Unit::Statement]
        );
    }

    #[test]
    fn sites_when_jump_or_calc_then_the_condition_is_an_expression_and_the_rest_is_not_a_site() {
        assert_eq!(units_of("JMP (c) lbl;", "c"), vec![Unit::Expression]);
        assert!(units_of("JMP (c) lbl;", "lbl").is_empty());
        assert!(units_of("JMP lbl;", "lbl").is_empty());
        assert_eq!(units_of("CALC(c, f(1));", "c"), vec![Unit::Expression]);
        assert!(units_of("CALC(c, f(1));", "f(1)").is_empty());
    }

    #[test]
    fn sites_when_catch_wait_throw_for_then_the_variable_and_the_operands_are_sites() {
        assert_eq!(
            units_of("__TRY __CATCH (e) __ENDTRY;", "e"),
            vec![Unit::Variable]
        );
        assert_eq!(units_of("__WAIT(c);", "c"), vec![Unit::Expression]);
        assert_eq!(units_of("__THROW(c);", "c"), vec![Unit::Expression]);
        assert!(units_of("FOR i := 1 TO b DO ; END_FOR;", "i").is_empty());
        assert_eq!(
            units_of("FOR i := 1 TO b DO ; END_FOR;", "b"),
            vec![Unit::Expression]
        );
    }

    #[test]
    fn sites_when_variable_block_then_a_site_of_the_block_and_of_its_facts_and_each_plain_declaration(
    ) {
        let parse = parse_source_file(
            "PROGRAM p VAR a : INT; b AT %IX0.0 : BOOL; END_VAR END_PROGRAM",
            &ParseOptions::all(),
        );
        let units: Vec<Unit> = sites(&parse.root)
            .into_iter()
            .filter(|site| {
                matches!(
                    site.unit,
                    Unit::VariableBlock | Unit::VariableBlockFacts | Unit::VariableInitial
                )
            })
            .map(|site| site.unit)
            .collect();
        // A declaration with a location is read with its block.
        assert_eq!(
            units,
            vec![
                Unit::VariableBlock,
                Unit::VariableBlockFacts,
                Unit::VariableInitial
            ]
        );
        assert!(Unit::VariableBlockFacts.is_view());
        assert!(!Unit::VariableBlock.is_view());
    }

    #[test]
    fn site_unit_when_every_row_then_its_unit_compares_the_kinds_the_parent_holds() {
        // A row whose unit holds no kind of child would never find a site.
        for part in PARTS {
            let holds_something = K::ALL.iter().any(|kind| part.unit.holds(*kind));
            assert!(holds_something, "{:?}", part.parent);
        }
    }
}
