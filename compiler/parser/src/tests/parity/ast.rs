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
//! - the `{:#?}` dump, which prints every field including each derived span
//!   and an identifier's original spelling (`VarDecl.block` is taken out of it
//!   because it is a process-wide counter, and is compared as a partition);
//! - the sequence of every `SourceSpan` the visitor reaches, which includes
//!   the identifier spans the dump leaves out;
//! - the block partition: which variable declarations share a block, with the
//!   ids renumbered by first appearance.

use ironplc_dsl::common::{ConstantKind, Library, VarDecl};
use ironplc_dsl::core::SourceSpan;
use ironplc_dsl::visitor::Visitor;
use std::fmt::Debug;

/// A span as compared: offsets and file.
pub type Span = (usize, usize, String);

/// What the comparison keeps of an object.
#[derive(Debug, PartialEq, Eq)]
pub struct Fingerprint {
    pub dump: String,
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

/// The dump without its `block:` lines: the one field that is an identity and
/// not a value, compared as a partition instead.
fn dump_without_block_ids<T: Debug>(value: &T) -> String {
    format!("{value:#?}")
        .lines()
        .filter(|line| !line.trim_start().starts_with("block: "))
        .collect::<Vec<_>>()
        .join("\n")
}

impl Fingerprint {
    pub fn of<T: Subject>(value: &T) -> Fingerprint {
        let mut collector = Collector::default();
        value.walk(&mut collector);
        Fingerprint {
            dump: dump_without_block_ids(value),
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
        if self.spans != other.spans {
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
    fn compare_when_a_span_moves_then_dump_and_spans_differ_although_partial_eq_holds() {
        let (a, b) = (constant("5"), constant(" 5"));
        assert_eq!(a, b, "PartialEq ignores spans");
        assert_eq!(compare(&a, &b), vec![Component::Dump, Component::Spans]);
    }

    #[test]
    fn compare_when_only_the_file_differs_then_spans_differ() {
        let a = constant("5");
        let moved = a
            .clone()
            .with_span(SourceSpan::range(16, 17).with_file_id(&FileId::from_string("other.st")));
        let same_range = a.clone().with_span(SourceSpan::range(16, 17));
        assert!(compare(&a, &same_range).is_empty());
        assert_eq!(compare(&a, &moved), vec![Component::Dump, Component::Spans]);
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
