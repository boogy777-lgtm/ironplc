//! The strict comparison of two syntax objects: the legacy parser's and the
//! lowered one.
//!
//! `PartialEq` on the objects is deliberately loose. It ignores every
//! `SourceSpan` (`SourceSpan::eq` is always true), compares identifiers by
//! their lower-case form and leaves out `VarDecl.block`, so two objects can
//! be equal and still differ in where they are, how a name was spelled and
//! which declarations shared a block. A [`Fingerprint`] keeps all three, and
//! two objects are the same only when every part of their fingerprints is:
//!
//! - the `{:#?}` dump, which prints every field and an identifier's original
//!   spelling, with the spans it prints taken out (`VarDecl.block` is taken
//!   out of it too, because it is a process-wide counter, and is compared as a
//!   partition). It is what says the shape and the values are the same;
//! - the spans: every `SourceSpan` the visitor reaches, which includes the
//!   identifier spans the dump leaves out, and every one the dump printed.
//!   It is what says the same objects are in the same places, so a difference
//!   in this part alone is a difference of position and nothing else;
//! - the block partition: which variable declarations share a block, with the
//!   ids renumbered by first appearance.

use ironplc_dsl::common::{
    ConstantKind, DataTypeDeclarationKind, FunctionBlockBodyKind, FunctionReturnType,
    InitialValueAssignmentKind, Library, LibraryElementKind, MethodDeclaration,
    PropertyDeclaration, VarDecl,
};
use ironplc_dsl::core::SourceSpan;
use ironplc_dsl::sfc::{ElementKind, Network};
use ironplc_dsl::textual::{Expr, ExprKind, StmtKind, Variable};
use ironplc_dsl::visitor::Visitor;
use ironplc_syntax::lower::oop::Member;
use ironplc_syntax::lower::var_blocks::{Block, InstanceInit};
use std::fmt::Debug;

/// A span as compared: offsets and file.
pub type Span = (usize, usize, String);

/// What the comparison keeps of an object.
#[derive(Debug, PartialEq, Eq)]
pub struct Fingerprint {
    pub dump: String,
    pub printed_spans: Vec<String>,
    pub spans: Vec<Span>,
    pub blocks: Vec<usize>,
}

/// The part of a fingerprint that differs.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Component {
    Dump,
    Spans,
    Blocks,
}

/// Gathers the spans and block ids of an object while it is walked.
#[derive(Default)]
pub struct Collector {
    spans: Vec<Span>,
    blocks: Vec<usize>,
}

impl Visitor<()> for Collector {
    type Value = ();

    fn visit_source_span(&mut self, node: &SourceSpan) -> Result<(), ()> {
        self.spans
            .push((node.start, node.end, node.file_id.to_string()));
        Ok(())
    }

    fn visit_var_decl(&mut self, node: &VarDecl) -> Result<(), ()> {
        self.blocks.push(node.block);
        node.recurse_visit(self)
    }
}

/// An object the comparison can fingerprint: it can be printed and walked.
pub trait Subject: Debug {
    fn walk(&self, collector: &mut Collector);
}

impl Subject for ConstantKind {
    fn walk(&self, collector: &mut Collector) {
        let _ = self.recurse_visit(collector);
    }
}

impl Subject for Expr {
    fn walk(&self, collector: &mut Collector) {
        let _ = self.recurse_visit(collector);
    }
}

impl Subject for Variable {
    fn walk(&self, collector: &mut Collector) {
        let _ = self.recurse_visit(collector);
    }
}

impl Subject for DataTypeDeclarationKind {
    fn walk(&self, collector: &mut Collector) {
        let _ = self.recurse_visit(collector);
    }
}

impl Subject for InitialValueAssignmentKind {
    fn walk(&self, collector: &mut Collector) {
        let _ = self.recurse_visit(collector);
    }
}

impl Subject for Vec<StmtKind> {
    fn walk(&self, collector: &mut Collector) {
        for statement in self {
            let _ = statement.recurse_visit(collector);
        }
    }
}

impl Subject for Vec<Network> {
    fn walk(&self, collector: &mut Collector) {
        for network in self {
            let _ = network.recurse_visit(collector);
        }
    }
}

impl Subject for Member {
    fn walk(&self, collector: &mut Collector) {
        let _ = match self {
            Member::Method(method) => method.recurse_visit(collector),
            Member::Property(property) => property.recurse_visit(collector),
        };
    }
}

impl Subject for Block {
    fn walk(&self, collector: &mut Collector) {
        for variable in &self.variables {
            let _ = collector.visit_var_decl(variable);
        }
        for access in &self.access {
            let _ = access.recurse_visit(collector);
        }
        for instance in &self.instances {
            let _ = match instance {
                InstanceInit::FunctionBlock(init) => init.recurse_visit(collector),
                InstanceInit::Located(init) => init.recurse_visit(collector),
            };
        }
    }
}

/// The block without what its declarations are initialised with, so that what a
/// block owns (the kind of variable, the qualifier, the names and locations,
/// the order, which declarations share a block) is compared on its own: an
/// initial value is compared where its own rule is.
pub fn without_initial_values(mut block: Block) -> Block {
    let none = || InitialValueAssignmentKind::None(SourceSpan::default());
    for variable in &mut block.variables {
        variable.initializer = none();
    }
    for instance in &mut block.instances {
        match instance {
            InstanceInit::FunctionBlock(init) => init.initializer.clear(),
            InstanceInit::Located(init) => init.initializer = none(),
        }
    }
    block
}

/// The declarations without the parts that have comparisons of their own, so
/// that what a unit or a member owns (its name, its qualifiers, what it extends
/// and implements, the kind of each variable and which share a block, the kind
/// of body, the accessors and the order of the members) is compared on its own:
/// the initial values of the variables, the statements of the bodies and the
/// position of the name of a return type are compared where their own rules
/// are.
pub fn without_inner_parts(mut elements: Vec<LibraryElementKind>) -> Vec<LibraryElementKind> {
    for element in &mut elements {
        match element {
            LibraryElementKind::ProgramDeclaration(program) => {
                mask_variables(&mut program.variables);
                mask_body(&mut program.body);
            }
            LibraryElementKind::FunctionDeclaration(function) => {
                mask_variables(&mut function.variables);
                mask_return_type(&mut function.return_type);
                function.body.clear();
            }
            LibraryElementKind::FunctionBlockDeclaration(block) => {
                mask_variables(&mut block.variables);
                mask_body(&mut block.body);
                block.methods.iter_mut().for_each(mask_method);
                block.properties.iter_mut().for_each(mask_property);
            }
            LibraryElementKind::ConfigurationDeclaration(configuration) => {
                mask_variables(&mut configuration.global_var);
                for init in &mut configuration.fb_inits {
                    init.initializer.clear();
                }
                for init in &mut configuration.located_var_inits {
                    init.initializer = InitialValueAssignmentKind::None(SourceSpan::default());
                }
                for resource in &mut configuration.resource_decl {
                    mask_variables(&mut resource.global_vars);
                }
            }
            LibraryElementKind::GlobalVarDeclarations(variables) => mask_variables(variables),
            LibraryElementKind::NamespaceDeclaration(namespace) => {
                let inner = std::mem::take(&mut namespace.elements);
                namespace.elements = without_inner_parts(inner)
                    .into_iter()
                    .filter(|element| {
                        !matches!(element, LibraryElementKind::DataTypeDeclaration(_))
                    })
                    .collect();
            }
            _ => {}
        }
    }
    elements
}

/// The member without the parts that have comparisons of their own.
pub fn member_without_inner_parts(mut member: Member) -> Member {
    match &mut member {
        Member::Method(method) => mask_method(method),
        Member::Property(property) => mask_property(property),
    }
    member
}

fn mask_variables(variables: &mut [VarDecl]) {
    for variable in variables {
        variable.initializer = InitialValueAssignmentKind::None(SourceSpan::default());
    }
}

fn mask_body(body: &mut FunctionBlockBodyKind) {
    match body {
        FunctionBlockBodyKind::Statements(statements) => statements.body.clear(),
        FunctionBlockBodyKind::Sfc(chart) => chart.networks.clear(),
        FunctionBlockBodyKind::Empty => {}
    }
}

/// The networks of a chart without the parts that have comparisons of their own,
/// so that what a chart owns (its networks, the steps and what each is
/// associated with, the steps each transition leaves and enters and its name and
/// priority, the names of the actions and the kind of each body) is compared on
/// its own: the conditions and the bodies of the actions are compared where their
/// own rules are.
pub fn without_inner_chart_parts(mut networks: Vec<Network>) -> Vec<Network> {
    for network in &mut networks {
        for element in &mut network.elements {
            match element {
                ElementKind::Transition(transition) => {
                    transition.condition = Expr::new(ExprKind::late_bound(""));
                }
                ElementKind::Action(action) => mask_body(&mut action.body),
                ElementKind::Step(_) => {}
            }
        }
    }
    networks
}

fn mask_return_type(return_type: &mut FunctionReturnType) {
    if let FunctionReturnType::Named(name) = return_type {
        name.name.span = SourceSpan::default();
    }
}

fn mask_method(method: &mut MethodDeclaration) {
    mask_variables(&mut method.variables);
    method.body.clear();
    if let Some(return_type) = &mut method.return_type {
        mask_return_type(return_type);
    }
}

fn mask_property(property: &mut PropertyDeclaration) {
    mask_return_type(&mut property.property_type);
    for accessor in [&mut property.get, &mut property.set].into_iter().flatten() {
        mask_method(accessor);
    }
}

impl Subject for Library {
    fn walk(&self, collector: &mut Collector) {
        let _ = collector.walk(self);
    }
}

/// Block ids renumbered in order of first appearance, so that two parses that
/// group their declarations alike agree whatever the counter held.
fn partition(blocks: &[usize]) -> Vec<usize> {
    let mut seen: Vec<usize> = Vec::new();
    blocks
        .iter()
        .map(|block| match seen.iter().position(|id| id == block) {
            Some(index) => index,
            None => {
                seen.push(*block);
                seen.len() - 1
            }
        })
        .collect()
}

/// The dump without its `block:` lines (the one field that is an identity and
/// not a value, compared as a partition instead), and with each printed
/// `SourceSpan` taken out of it: the dump keeps the shape and the values, and
/// the spans come back as their own list, in order of appearance.
fn dump_and_printed_spans<T: Debug>(value: &T) -> (String, Vec<String>) {
    let text = format!("{value:#?}");
    let mut dump: Vec<String> = Vec::new();
    let mut spans: Vec<String> = Vec::new();
    let mut lines = text.lines();
    while let Some(line) = lines.next() {
        if line.trim_start().starts_with("block: ") {
            continue;
        }
        if !line.trim_end().ends_with("SourceSpan {") {
            dump.push(line.to_string());
            continue;
        }
        // The printed span runs to the line that closes it, at the indent of
        // the line that opened it.
        let indent = line.len() - line.trim_start().len();
        let mut printed = String::new();
        for inner in lines.by_ref() {
            let closes = inner.len() - inner.trim_start().len() == indent;
            printed.push_str(inner.trim());
            if closes {
                break;
            }
        }
        spans.push(printed);
        dump.push(format!(
            "{}<span>",
            &line[..line.len() - "SourceSpan {".len()]
        ));
    }
    (dump.join("\n"), spans)
}

impl Fingerprint {
    pub fn of<T: Subject>(value: &T) -> Fingerprint {
        let mut collector = Collector::default();
        value.walk(&mut collector);
        let (dump, printed_spans) = dump_and_printed_spans(value);
        Fingerprint {
            dump,
            printed_spans,
            spans: collector.spans,
            blocks: partition(&collector.blocks),
        }
    }

    /// The parts in which `self` and `other` differ.
    pub fn differences(&self, other: &Fingerprint) -> Vec<Component> {
        let mut parts = Vec::new();
        if self.dump != other.dump {
            parts.push(Component::Dump);
        }
        if self.spans != other.spans || self.printed_spans != other.printed_spans {
            parts.push(Component::Spans);
        }
        if self.blocks != other.blocks {
            parts.push(Component::Blocks);
        }
        parts
    }
}

/// The parts in which two objects differ; empty when they are the same.
pub fn compare<T: Subject>(legacy: &T, lowered: &T) -> Vec<Component> {
    Fingerprint::of(legacy).differences(&Fingerprint::of(lowered))
}

/// What differs, shown for a failing comparison: the first line of the dump
/// and the first span that disagree.
pub fn explain<T: Subject>(legacy: &T, lowered: &T) -> String {
    let (left, right) = (Fingerprint::of(legacy), Fingerprint::of(lowered));
    let line = left
        .dump
        .lines()
        .zip(right.dump.lines())
        .find(|(a, b)| a != b)
        .map(|(a, b)| format!("legacy `{}` / lowered `{}`", a.trim(), b.trim()));
    let span = left
        .spans
        .iter()
        .zip(&right.spans)
        .find(|(a, b)| a != b)
        .map(|(a, b)| format!("span {a:?} / {b:?}"));
    format!(
        "{:?}: {} {}",
        left.differences(&right),
        line.unwrap_or_default(),
        span.unwrap_or_default()
    )
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::legacy::parse_program;
    use crate::options::CompilerOptions;
    use ironplc_dsl::common::{FunctionBlockBodyKind, LibraryElementKind};
    use ironplc_dsl::core::FileId;

    fn library(source: &str) -> Library {
        parse_program(source, &FileId::default(), &CompilerOptions::default())
            .expect("the source parses")
    }

    fn constant(source: &str) -> ConstantKind {
        let library = library(&format!("TYPE t : INT := {source}; END_TYPE"));
        let mut found = Vec::new();
        let mut collector = ConstantCollector(&mut found);
        let _ = collector.walk(&library);
        found.remove(0)
    }

    struct ConstantCollector<'a>(&'a mut Vec<ConstantKind>);

    impl Visitor<()> for ConstantCollector<'_> {
        type Value = ();

        fn visit_constant_kind(&mut self, node: &ConstantKind) -> Result<(), ()> {
            self.0.push(node.clone());
            Ok(())
        }
    }

    const TWO_BLOCKS: &str =
        "PROGRAM p\nVAR a, b : INT; END_VAR\nVAR c : INT; END_VAR\nEND_PROGRAM\n";

    fn program_variables(library: &mut Library) -> Option<&mut Vec<VarDecl>> {
        library
            .elements
            .iter_mut()
            .find_map(|element| match element {
                LibraryElementKind::ProgramDeclaration(program) => Some(&mut program.variables),
                _ => None,
            })
    }

    #[test]
    fn compare_when_same_text_parsed_twice_then_no_difference_even_though_block_ids_differ() {
        let (first, second) = (library(TWO_BLOCKS), library(TWO_BLOCKS));
        assert!(compare(&first, &second).is_empty());
    }

    #[test]
    fn compare_when_a_span_moves_then_only_the_spans_differ_although_partial_eq_holds() {
        let (a, b) = (constant("5"), constant(" 5"));
        assert_eq!(a, b, "PartialEq ignores spans");
        assert_eq!(compare(&a, &b), vec![Component::Spans]);
    }

    #[test]
    fn compare_when_only_the_file_differs_then_only_the_spans_differ() {
        let a = constant("5");
        let moved = a
            .clone()
            .with_span(SourceSpan::range(16, 17).with_file_id(&FileId::from_string("other.st")));
        let same_range = a.clone().with_span(SourceSpan::range(16, 17));
        assert!(compare(&a, &same_range).is_empty());
        assert_eq!(compare(&a, &moved), vec![Component::Spans]);
    }

    #[test]
    fn compare_when_identifier_spelling_differs_then_dump_differs_although_partial_eq_holds() {
        let upper = library("PROGRAM Speed\nEND_PROGRAM\n");
        let lower = library("PROGRAM speed\nEND_PROGRAM\n");
        assert_eq!(upper, lower, "PartialEq compares the lower-case form");
        assert_eq!(compare(&upper, &lower), vec![Component::Dump]);
    }

    #[test]
    fn compare_when_identifier_span_differs_then_spans_differ() {
        // The dump prints an identifier by its spelling alone; the span
        // sequence is what sees where it was written.
        let a = library("PROGRAM p\nEND_PROGRAM\n");
        let b = library("PROGRAM  p\nEND_PROGRAM\n");
        let parts = compare(&a, &b);
        assert!(parts.contains(&Component::Spans), "{parts:?}");
    }

    #[test]
    fn compare_when_block_partition_differs_then_blocks_differ_and_nothing_else() {
        let first = library(TWO_BLOCKS);
        let mut regrouped = first.clone();
        if let Some(variables) = program_variables(&mut regrouped) {
            let shared = variables[0].block;
            for variable in variables.iter_mut() {
                variable.block = shared;
            }
        }
        assert_eq!(first, regrouped, "PartialEq ignores the block");
        assert_eq!(compare(&first, &regrouped), vec![Component::Blocks]);
    }

    #[test]
    fn without_initial_values_when_only_the_initial_values_differ_then_the_blocks_are_the_same() {
        let block = |text: &str| {
            let library = library(&format!(
                "PROGRAM p VAR a : INT := {text}; END_VAR END_PROGRAM"
            ));
            let variables = match &library.elements[0] {
                LibraryElementKind::ProgramDeclaration(program) => program.variables.clone(),
                _ => vec![],
            };
            Block {
                variables,
                ..Block::default()
            }
        };
        let (five, six) = (block("5"), block("6"));
        assert!(!compare(&five, &six).is_empty());
        assert!(compare(&without_initial_values(five), &without_initial_values(six)).is_empty());
    }

    #[test]
    fn without_initial_values_when_blocks_are_regrouped_then_the_partition_still_differs() {
        let library = library(TWO_BLOCKS);
        let mut regrouped = library.clone();
        let variables = |library: &Library| match &library.elements[0] {
            LibraryElementKind::ProgramDeclaration(program) => program.variables.clone(),
            _ => vec![],
        };
        if let Some(variables) = program_variables(&mut regrouped) {
            let shared = variables[0].block;
            for variable in variables.iter_mut() {
                variable.block = shared;
            }
        }
        let block = |library: &Library| {
            without_initial_values(Block {
                variables: variables(library),
                ..Block::default()
            })
        };
        assert_eq!(
            compare(&block(&library), &block(&regrouped)),
            vec![Component::Blocks]
        );
    }

    #[test]
    fn without_inner_parts_when_only_an_inner_part_differs_then_the_same_and_when_an_own_fact_differs_then_not(
    ) {
        let elements = |source: &str| library(source).elements;
        let masked = |source: &str| without_inner_parts(elements(source));
        let compare_all = |a: &str, b: &str| {
            compare(
                &Library {
                    elements: masked(a),
                },
                &Library {
                    elements: masked(b),
                },
            )
        };
        let unit = |body: &str, init: &str, name: &str| {
            format!("PROGRAM {name} VAR a : INT := {init}; END_VAR {body} END_PROGRAM")
        };
        // The initial value and the statements have comparisons of their own.
        assert!(compare_all(&unit("x := 1;", "1", "p"), &unit("x := 2;", "2", "p")).is_empty());
        // The name, and the kind of body, are the unit's own.
        assert!(!compare_all(&unit("x := 1;", "1", "p"), &unit("x := 1;", "1", "q")).is_empty());
        assert!(!compare_all(&unit("x := 1;", "1", "p"), &unit("", "1", "p")).is_empty());
    }
    #[test]
    fn partition_when_ids_arbitrary_then_renumbered_by_first_appearance() {
        assert_eq!(partition(&[40, 40, 7, 40, 9]), vec![0, 0, 1, 0, 2]);
        assert!(partition(&[]).is_empty());
    }

    #[test]
    fn explain_when_values_differ_then_names_the_first_difference() {
        let text = explain(&constant("5"), &constant("6"));
        assert!(text.contains("Dump"), "{text}");
        assert!(text.contains("value: 5"), "{text}");
    }

    #[test]
    fn compare_when_function_block_body_differs_then_dump_differs() {
        let a = library("PROGRAM p\nEND_PROGRAM\n");
        let b = library("PROGRAM p\nx := 1;\nEND_PROGRAM\n");
        let body = |library: &Library| match &library.elements[0] {
            LibraryElementKind::ProgramDeclaration(program) => {
                matches!(program.body, FunctionBlockBodyKind::Statements(_))
            }
            _ => false,
        };
        assert_ne!(body(&a), body(&b));
        assert!(compare(&a, &b).contains(&Component::Dump));
    }
}
