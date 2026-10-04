//! Lowering: from the lossless tree to the `ironplc_dsl` objects.
//!
//! The tree keeps every byte; the objects keep what the compiler uses. This
//! module tree is the one place that turns the first into the second (design:
//! parse-tree architecture, sections 3.1 and 3.2), so a language element is
//! lowered by one rule wherever it appears.
//!
//! Three pieces are shared by every rule:
//!
//! - [`LowerCx`] carries the file the text came from and the options it was
//!   parsed under (a word the dialect leaves available as a name keeps the kind
//!   of the keyword in the tree, and only the options say which it is), and is
//!   the only place a byte range of the tree becomes a [`SourceSpan`], and a
//!   lowering problem becomes a [`Diagnostic`].
//! - [`disposition`] says, for every kind the tree can hold, whether it is
//!   lowered by a rule (and which area owns it), is consumed by its parent's
//!   rule, is trivia, or has no rule yet. It matches every node kind without
//!   a wildcard arm, so a new node kind does not compile until it has one.
//! - The rules, one module per area, each dispatching on the kind of the node
//!   it is given.
//!
//! Lowering is only run on a tree whose parse reported no error: a node the
//! grammar guarantees but the tree lacks is an internal error, and a node whose
//! area has no rule yet is reported as not implemented.

pub mod declarations;
pub mod expressions;
pub mod initializers;
pub mod literals;
pub mod names;
pub mod statements;
pub mod tree;
pub mod types;
pub mod values;
pub mod variables;

use self::statements::lower_statement_list;
use self::tree::child_of;
use self::types::lower_type_block;
use crate::parser::options::ParseOptions;
use crate::parser::Parse;
use crate::syntax_kind::{NodeKind, SyntaxKind, SyntaxNode, SyntaxToken};
use ironplc_dsl::common::{Library, LibraryElementKind};
use ironplc_dsl::core::{FileId, SourceSpan};
use ironplc_dsl::diagnostic::{Diagnostic, Label};
use ironplc_dsl::textual::StmtKind;
use ironplc_problems::Problem;
use rowan::TextRange;

/// The areas of the language that own lowering rules. A node lowered by a
/// rule names its area in its [`Disposition`].
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum Area {
    /// The literals: numbers, bit strings, booleans, character strings and
    /// the temporal literals (`literals`).
    Literal,
    /// Declared names and the names of types (`names`).
    Name,
    /// Operations on values: operators, groups, calls, references and the
    /// special operators (`expressions`).
    Expression,
    /// What a value is read from or written to: a name, a member, an element,
    /// a bit, a dereference, a direct address (`variables`).
    Variable,
    /// What a program does: the statements and the lists they sit in
    /// (`statements`).
    Statement,
    /// What a declaration says a thing is: the `TYPE` blocks and their
    /// declarations, the forms of type, and the members of a structure
    /// (`types`).
    Type,
    /// The values that follow a `:=` in a declaration: the elements of an
    /// array, the members of a structure and a qualified enumeration value
    /// (`initializers`).
    Initializer,
}

/// What lowering does with a kind the tree can hold.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Disposition {
    /// A rule of the area turns the node into an object.
    Lowered(Area),
    /// Read by the rule of the node that contains it (a token, or a node that
    /// is only a part of a larger one), or a container the entry points walk.
    Structural,
    /// Retained text that is not language: lowering ignores it.
    Trivia,
    /// A node kind whose lowering rule has not been written yet. Asking to
    /// lower one is a not-implemented diagnostic.
    Pending,
}

impl Disposition {
    /// True when a lowering rule for the kind is still to be written.
    pub fn is_pending(self) -> bool {
        self == Disposition::Pending
    }
}

/// The disposition of every node kind. There is no wildcard arm: a node kind
/// added to the syntax declaration fails to compile here until someone decides
/// how it is lowered.
fn node_disposition(node: NodeKind) -> Disposition {
    use Disposition::{Lowered, Pending, Structural};
    match node {
        // The container the entry point walks.
        NodeKind::SourceFile => Structural,
        // Present only where the parse reported an error, which lowering
        // never sees.
        NodeKind::ErrorNode => Structural,
        NodeKind::BoolLiteral
        | NodeKind::IntLiteral
        | NodeKind::RealLiteral
        | NodeKind::BitStringLiteral
        | NodeKind::StringLiteral
        | NodeKind::DurationLiteral
        | NodeKind::TimeOfDayLiteral
        | NodeKind::DateLiteral
        | NodeKind::DateTimeLiteral => Lowered(Area::Literal),
        NodeKind::Name | NodeKind::TypeRef => Lowered(Area::Name),
        NodeKind::BinaryExpr
        | NodeKind::UnaryExpr
        | NodeKind::ParenExpr
        | NodeKind::CallExpr
        | NodeKind::RefExpr
        | NodeKind::SpecialOpExpr
        | NodeKind::NullLiteral => Lowered(Area::Expression),
        NodeKind::NameRef
        | NodeKind::FieldExpr
        | NodeKind::IndexExpr
        | NodeKind::BitAccessExpr
        | NodeKind::PartialAccessExpr
        | NodeKind::DerefExpr
        | NodeKind::SelfRefExpr
        | NodeKind::DirectAddressExpr => Lowered(Area::Variable),
        // The arguments of a call, read by the rule of the call.
        NodeKind::ArgList | NodeKind::PositionalArg | NodeKind::NamedArg | NodeKind::OutputArg => {
            Structural
        }
        // The `:=` of a declaration and its value, read by the rule of the
        // declaration.
        NodeKind::Initializer => Structural,
        NodeKind::TypeBlock
        | NodeKind::TypeDecl
        | NodeKind::ArrayType
        | NodeKind::Subrange
        | NodeKind::StringType
        | NodeKind::RefType
        | NodeKind::ParamsType
        | NodeKind::SubrangeType
        | NodeKind::EnumType
        | NodeKind::EnumValue
        | NodeKind::StructType
        | NodeKind::UnionType
        | NodeKind::StructMember => Lowered(Area::Type),
        NodeKind::ArrayInit
        | NodeKind::RepeatedInit
        | NodeKind::StructInit
        | NodeKind::StructInitElement
        | NodeKind::EnumValueRef => Lowered(Area::Initializer),
        // The parts of a statement, read by the rule of the statement.
        NodeKind::AssignOp
        | NodeKind::ElsifClause
        | NodeKind::ElseClause
        | NodeKind::CaseBranch
        | NodeKind::CaseLabel
        | NodeKind::CatchClause
        | NodeKind::FinallyClause => Structural,
        NodeKind::StatementList
        | NodeKind::EmptyStmt
        | NodeKind::AssignStmt
        | NodeKind::CallStmt
        | NodeKind::IfStmt
        | NodeKind::CaseStmt
        | NodeKind::ForStmt
        | NodeKind::WhileStmt
        | NodeKind::RepeatStmt
        | NodeKind::ExitStmt
        | NodeKind::ContinueStmt
        | NodeKind::ReturnStmt
        | NodeKind::TryStmt
        | NodeKind::ThrowStmt
        | NodeKind::JmpStmt
        | NodeKind::LabelStmt
        | NodeKind::CalcStmt
        | NodeKind::WaitStmt
        | NodeKind::ImplementationMarker => Lowered(Area::Statement),
        NodeKind::ProgramDecl
        | NodeKind::FunctionDecl
        | NodeKind::FunctionBlockDecl
        | NodeKind::InterfaceDecl
        | NodeKind::NamespaceDecl
        | NodeKind::MethodDecl
        | NodeKind::PropertyDecl
        | NodeKind::GetAccessor
        | NodeKind::SetAccessor
        | NodeKind::MemberQualifier
        | NodeKind::ExtendsClause
        | NodeKind::ImplementsClause
        | NodeKind::VarBlock
        | NodeKind::VarDecl
        | NodeKind::Location
        | NodeKind::EdgeSpec
        | NodeKind::AccessDecl
        | NodeKind::InstanceInit
        | NodeKind::ConfigurationDecl
        | NodeKind::ResourceDecl
        | NodeKind::TaskDecl
        | NodeKind::TaskInit
        | NodeKind::TaskInitItem
        | NodeKind::ProgramConfig
        | NodeKind::TaskBinding
        | NodeKind::ProgramConnection
        | NodeKind::SfcBody
        | NodeKind::InitialStepDecl
        | NodeKind::StepDecl
        | NodeKind::ActionAssociation
        | NodeKind::ActionQualifier
        | NodeKind::ActionDecl
        | NodeKind::TransitionDecl
        | NodeKind::TransitionPriority
        | NodeKind::StepList
        | NodeKind::TransitionCondition => Pending,
    }
}

/// What lowering does with a node or token of `kind`: trivia is ignored, a
/// token is read by the rule of the node holding it, and a node is decided by
/// the node table.
pub fn disposition(kind: SyntaxKind) -> Disposition {
    if kind.is_trivia() {
        return Disposition::Trivia;
    }
    match kind.node() {
        Some(node) => node_disposition(node),
        None => Disposition::Structural,
    }
}

/// True when `root` or a node under it is of a kind whose lowering rule has
/// not been written yet.
pub fn contains_pending(root: &SyntaxNode) -> bool {
    root.descendants()
        .any(|node| disposition(node.kind()).is_pending())
}

/// The codes of the two compiler-located problems. `Problem` marks them
/// deprecated so that only the `Diagnostic` constructors build them; tests
/// compare the codes.
#[cfg(test)]
pub(crate) const NOT_IMPLEMENTED: &str = "P9999";
#[cfg(test)]
pub(crate) const INTERNAL_ERROR: &str = "P9998";

/// What every lowering rule is given besides the node: the file the text came
/// from, and the means to report a problem.
#[derive(Debug, Clone)]
pub struct LowerCx {
    file_id: FileId,
    options: ParseOptions,
}

impl LowerCx {
    /// A context for lowering text of `file_id` under every keyword enabled.
    /// The text of a dialect is lowered with [`LowerCx::with_options`].
    pub fn new(file_id: FileId) -> Self {
        LowerCx {
            file_id,
            options: ParseOptions::all(),
        }
    }

    /// This context for text parsed under `options`.
    pub fn with_options(self, options: ParseOptions) -> Self {
        LowerCx { options, ..self }
    }

    /// True when `kind` is a keyword in the dialect the text was written in,
    /// and false when the dialect leaves the word available as a name.
    pub fn keyword_enabled(&self, kind: SyntaxKind) -> bool {
        self.options.keyword_enabled(kind)
    }

    /// True when `TIME`, followed by a token of kind `next`, is an ordinary
    /// name in the dialect the text was written in (see
    /// [`ParseOptions::time_is_name`]).
    pub fn time_is_name(&self, next: Option<SyntaxKind>) -> bool {
        self.options.time_is_name(next)
    }

    /// The file the lowered text came from.
    pub fn file_id(&self) -> &FileId {
        &self.file_id
    }

    /// The span of a byte range of the tree: the one mapping from tree
    /// ranges to the spans of the objects, so every span carries the file it
    /// is in and means the same thing wherever it is built.
    ///
    /// The range of a node equals the join of its first and last significant
    /// token, because the tree places trivia outside the node that ends or
    /// begins at it.
    pub fn span(&self, range: TextRange) -> SourceSpan {
        SourceSpan {
            start: usize::from(range.start()),
            end: usize::from(range.end()),
            file_id: self.file_id.clone(),
        }
    }

    /// The span of a node.
    pub fn node_span(&self, node: &SyntaxNode) -> SourceSpan {
        self.span(node.text_range())
    }

    /// The span of a token.
    pub fn token_span(&self, token: &SyntaxToken) -> SourceSpan {
        self.span(token.text_range())
    }

    /// A syntax error (P0002) over `range`: the text parsed but does not
    /// denote a value, such as a number too large for any integer type.
    pub fn syntax_error(&self, range: TextRange, message: impl Into<String>) -> Diagnostic {
        Diagnostic::problem(Problem::SyntaxError, Label::span(self.span(range), message))
    }

    /// An internal error (P9998) over `range`: the tree is not shaped as the
    /// grammar guarantees.
    #[track_caller]
    pub fn internal_error(&self, range: TextRange, message: impl Into<String>) -> Diagnostic {
        Diagnostic::internal_error_at(Label::span(self.span(range), message))
    }

    /// The diagnostic for a node that lacks a part the grammar guarantees:
    /// the tree is not shaped as the parser builds it.
    #[track_caller]
    pub fn missing(&self, node: &SyntaxNode, what: &str) -> Diagnostic {
        self.internal_error(
            node.text_range(),
            format!("{:?} does not hold {what}", node.kind()),
        )
    }

    /// The diagnostic for a node that was asked to be lowered by a rule that
    /// does not exist: not implemented (P9999) when the node's kind is
    /// pending, an internal error otherwise (the caller is not the rule's
    /// owner).
    #[track_caller]
    pub fn unsupported(&self, node: &SyntaxNode) -> Diagnostic {
        let kind = node.kind();
        if disposition(kind).is_pending() {
            Diagnostic::not_implemented(Label::span(
                self.node_span(node),
                format!("lowering {kind:?} is not available yet"),
            ))
        } else {
            self.internal_error(
                node.text_range(),
                format!("{kind:?} is not lowered by this rule"),
            )
        }
    }
}

/// The diagnostic for the first error of a parse that reported any. Lowering
/// is only run on an error-free parse; which of several errors to report is
/// the caller's decision.
fn first_error(parse: &Parse, file_id: &FileId) -> Option<Diagnostic> {
    parse
        .errors
        .first()
        .map(|error| error.to_diagnostic(file_id))
}

/// A rule: builds the library elements a top-level node writes.
type ElementRule = fn(&LowerCx, &SyntaxNode) -> Result<Vec<LibraryElementKind>, Diagnostic>;

/// `TYPE ... END_TYPE` is one element for each of its declarations.
fn type_elements(cx: &LowerCx, node: &SyntaxNode) -> Result<Vec<LibraryElementKind>, Diagnostic> {
    Ok(lower_type_block(cx, node)?
        .into_iter()
        .map(LibraryElementKind::DataTypeDeclaration)
        .collect())
}

/// The top-level nodes that have a rule, and the rule of each. A node of any
/// other kind has none yet.
const ELEMENTS: &[(SyntaxKind, ElementRule)] = &[(SyntaxKind::TypeBlock, type_elements)];

/// Lowers the tree of a whole file to a library.
///
/// Fails with the first error of the parse when it has any, and with a
/// not-implemented diagnostic at the first declaration that has no rule yet.
pub fn lower_library(parse: &Parse, file_id: &FileId) -> Result<Library, Diagnostic> {
    if let Some(error) = first_error(parse, file_id) {
        return Err(error);
    }
    let cx = LowerCx::new(file_id.clone()).with_options(parse.options);
    let mut elements = Vec::new();
    for node in parse.root.children() {
        let (_, rule) = ELEMENTS
            .iter()
            .find(|(kind, _)| *kind == node.kind())
            .ok_or_else(|| cx.unsupported(&node))?;
        elements.extend(rule(&cx, &node)?);
    }
    Ok(Library { elements })
}

/// Lowers the tree of a statement list to statements.
///
/// Fails with the first error of the parse when it has any, and otherwise
/// with the first error of a statement.
pub fn lower_statements(parse: &Parse, file_id: &FileId) -> Result<Vec<StmtKind>, Diagnostic> {
    if let Some(error) = first_error(parse, file_id) {
        return Err(error);
    }
    let cx = LowerCx::new(file_id.clone()).with_options(parse.options);
    match child_of(&parse.root, SyntaxKind::StatementList) {
        Some(list) => lower_statement_list(&cx, &list),
        None => Ok(vec![]),
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::{parse_expression, parse_source_file, parse_statements, ParseOptions};

    fn options() -> ParseOptions {
        ParseOptions::default()
    }

    fn code_of(diagnostic: Option<Diagnostic>) -> Option<String> {
        diagnostic.map(|diagnostic| diagnostic.code)
    }

    #[test]
    fn disposition_when_any_kind_then_the_table_answers_without_a_gap() {
        for kind in SyntaxKind::ALL {
            // Totality: the call returns for every kind, including `Unknown`.
            let _ = disposition(*kind);
        }
    }

    #[test]
    fn disposition_when_trivia_then_trivia() {
        for kind in SyntaxKind::ALL.iter().filter(|kind| kind.is_trivia()) {
            assert_eq!(disposition(*kind), Disposition::Trivia, "{kind:?}");
        }
    }

    #[test]
    fn disposition_when_token_or_keyword_then_structural() {
        for kind in SyntaxKind::ALL
            .iter()
            .filter(|kind| kind.is_token() && !kind.is_trivia())
        {
            assert_eq!(disposition(*kind), Disposition::Structural, "{kind:?}");
        }
    }

    #[test]
    fn disposition_when_literal_node_then_lowered_by_the_literal_area() {
        for kind in [
            SyntaxKind::BoolLiteral,
            SyntaxKind::IntLiteral,
            SyntaxKind::RealLiteral,
            SyntaxKind::BitStringLiteral,
            SyntaxKind::StringLiteral,
            SyntaxKind::DurationLiteral,
            SyntaxKind::TimeOfDayLiteral,
            SyntaxKind::DateLiteral,
            SyntaxKind::DateTimeLiteral,
        ] {
            assert_eq!(disposition(kind), Disposition::Lowered(Area::Literal));
        }
        assert_eq!(
            disposition(SyntaxKind::Name),
            Disposition::Lowered(Area::Name)
        );
    }

    #[test]
    fn disposition_when_expression_node_then_lowered_by_the_expression_area() {
        for kind in [
            SyntaxKind::BinaryExpr,
            SyntaxKind::UnaryExpr,
            SyntaxKind::ParenExpr,
            SyntaxKind::CallExpr,
            SyntaxKind::RefExpr,
            SyntaxKind::SpecialOpExpr,
            SyntaxKind::NullLiteral,
        ] {
            assert_eq!(disposition(kind), Disposition::Lowered(Area::Expression));
        }
    }

    #[test]
    fn disposition_when_node_that_names_a_place_then_lowered_by_the_variable_area() {
        for kind in [
            SyntaxKind::NameRef,
            SyntaxKind::FieldExpr,
            SyntaxKind::IndexExpr,
            SyntaxKind::BitAccessExpr,
            SyntaxKind::PartialAccessExpr,
            SyntaxKind::DerefExpr,
            SyntaxKind::SelfRefExpr,
            SyntaxKind::DirectAddressExpr,
        ] {
            assert_eq!(disposition(kind), Disposition::Lowered(Area::Variable));
        }
        assert_eq!(
            disposition(SyntaxKind::TypeRef),
            Disposition::Lowered(Area::Name)
        );
    }

    #[test]
    fn disposition_when_argument_node_then_read_by_the_rule_of_the_call() {
        for kind in [
            SyntaxKind::ArgList,
            SyntaxKind::PositionalArg,
            SyntaxKind::NamedArg,
            SyntaxKind::OutputArg,
        ] {
            assert_eq!(disposition(kind), Disposition::Structural);
        }
    }

    #[test]
    fn contains_pending_when_only_expressions_and_variables_then_false() {
        for source in [
            "a + f(b, c := 1, d => e)[1]",
            "m.run(THIS^.x)",
            "REF(a) = NULL",
        ] {
            let parse = parse_expression(source, &ParseOptions::all());
            assert!(!contains_pending(&parse.root), "{source}");
        }
    }

    #[test]
    fn missing_when_node_lacks_a_part_then_internal_error_naming_the_node_and_the_part() {
        let parse = parse_expression("a + b", &options());
        let cx = LowerCx::new(FileId::default());
        let node = parse.root.first_child();
        let diagnostic = node.map(|node| cx.missing(&node, "an operand"));
        let message = diagnostic.as_ref().map(|d| d.primary.message.clone());
        assert_eq!(diagnostic.map(|d| d.code), Some(INTERNAL_ERROR.to_string()));
        assert_eq!(
            message,
            Some("BinaryExpr does not hold an operand".to_string())
        );
    }

    #[test]
    fn disposition_when_node_without_a_rule_yet_then_pending() {
        assert!(disposition(SyntaxKind::SfcBody).is_pending());
        assert!(disposition(SyntaxKind::ProgramDecl).is_pending());
        assert!(!disposition(SyntaxKind::AssignStmt).is_pending());
        assert!(!disposition(SyntaxKind::IntLiteral).is_pending());
        assert!(!disposition(SyntaxKind::SourceFile).is_pending());
    }

    #[test]
    fn contains_pending_when_only_literals_and_statements_then_false_and_with_a_declaration_then_true(
    ) {
        let literal = parse_expression("T#5s", &options());
        assert!(!contains_pending(&literal.root));
        let statement = parse_statements("x := 1; IF a THEN b := 2; END_IF;", &options());
        assert!(!contains_pending(&statement.root));
        let declaration = parse_source_file("PROGRAM p\nEND_PROGRAM\n", &options());
        assert!(contains_pending(&declaration.root));
    }

    #[test]
    fn span_when_range_then_offsets_and_the_file_of_the_context() {
        let file = FileId::from_string("a.st");
        let cx = LowerCx::new(file.clone());
        let span = cx.span(TextRange::new(3.into(), 8.into()));
        assert_eq!((span.start, span.end), (3, 8));
        assert_eq!(span.file_id, file);
        assert_eq!(cx.file_id(), &file);
    }

    #[test]
    fn node_span_when_node_has_trivia_around_it_then_range_excludes_the_trivia() {
        let parse = parse_expression("  5  ", &options());
        let cx = LowerCx::new(FileId::default());
        let literal = parse.root.first_child();
        let span = literal.map(|node| cx.node_span(&node));
        assert_eq!(span.map(|span| (span.start, span.end)), Some((2, 3)));
    }

    #[test]
    fn token_span_when_token_then_its_range() {
        let parse = parse_expression("  5  ", &options());
        let cx = LowerCx::new(FileId::default());
        let token = parse
            .root
            .descendants_with_tokens()
            .filter_map(|element| element.into_token())
            .find(|token| token.kind() == SyntaxKind::IntegerLit);
        let span = token.map(|token| cx.token_span(&token));
        assert_eq!(span.map(|span| (span.start, span.end)), Some((2, 3)));
    }

    #[test]
    fn syntax_error_when_range_then_p0002_over_the_range_in_the_file() {
        let file = FileId::from_string("a.st");
        let cx = LowerCx::new(file.clone());
        let diagnostic = cx.syntax_error(TextRange::new(1.into(), 4.into()), "too large");
        assert_eq!(diagnostic.code, Problem::SyntaxError.code());
        assert_eq!(diagnostic.primary.file_id, file);
        assert_eq!(
            (
                diagnostic.primary.location.start,
                diagnostic.primary.location.end
            ),
            (1, 4)
        );
    }

    #[test]
    fn unsupported_when_pending_node_then_not_implemented_at_the_node() {
        let parse = parse_source_file("PROGRAM p\nEND_PROGRAM\n", &options());
        let cx = LowerCx::new(FileId::default());
        let node = parse
            .root
            .descendants()
            .find(|node| node.kind() == SyntaxKind::ProgramDecl);
        let diagnostic = node.map(|node| cx.unsupported(&node));
        assert_eq!(
            diagnostic.as_ref().map(|d| (
                d.code.as_str(),
                d.primary.location.start,
                d.primary.location.end
            )),
            Some((NOT_IMPLEMENTED, 0, 21))
        );
    }

    #[test]
    fn unsupported_when_node_with_a_rule_then_internal_error() {
        let parse = parse_expression("5", &options());
        let cx = LowerCx::new(FileId::default());
        let diagnostic = parse.root.first_child().map(|node| cx.unsupported(&node));
        assert_eq!(code_of(diagnostic), Some(INTERNAL_ERROR.to_string()));
    }

    #[test]
    fn lower_library_when_no_declaration_then_empty_library() {
        for source in ["", "  (* nothing *)\n{pragma}\n"] {
            let parse = parse_source_file(source, &ParseOptions::all());
            let library = lower_library(&parse, &FileId::default());
            assert_eq!(library.map(|library| library.elements.len()).ok(), Some(0));
        }
    }

    #[test]
    fn lower_library_when_declaration_then_not_implemented_at_it() {
        let parse = parse_source_file("PROGRAM p\nEND_PROGRAM\n", &options());
        let diagnostic = lower_library(&parse, &FileId::default()).err();
        assert_eq!(code_of(diagnostic), Some(NOT_IMPLEMENTED.to_string()));
    }

    #[test]
    fn lower_library_when_parse_has_errors_then_the_first_error_in_the_file() {
        let file = FileId::from_string("bad.st");
        let parse = parse_source_file("PROGRAM p x END_PROGRAM", &options());
        let diagnostic = lower_library(&parse, &file).err();
        assert_eq!(
            diagnostic
                .as_ref()
                .map(|d| (d.code.as_str(), &d.primary.file_id)),
            Some((Problem::SyntaxError.code(), &file))
        );
    }

    #[test]
    fn lower_statements_when_empty_or_comment_only_then_no_statements() {
        for source in ["", "(* nothing *)"] {
            let parse = parse_statements(source, &options());
            let statements = lower_statements(&parse, &FileId::default());
            assert_eq!(statements.map(|list| list.len()).ok(), Some(0), "{source}");
        }
    }

    #[test]
    fn lower_statements_when_items_then_one_statement_each_except_an_empty_one() {
        let parse = parse_statements("x := 1;; IF a THEN END_IF;", &options());
        let statements = lower_statements(&parse, &FileId::default());
        assert_eq!(statements.map(|list| list.len()).ok(), Some(2));
    }

    #[test]
    fn lower_statements_when_parse_has_errors_then_the_first_error() {
        let parse = parse_statements("x := ;", &options());
        let diagnostic = lower_statements(&parse, &FileId::default()).err();
        assert_eq!(
            code_of(diagnostic),
            Some(Problem::SyntaxError.code().to_string())
        );
    }
}
