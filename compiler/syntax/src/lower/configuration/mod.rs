//! Configurations: `CONFIGURATION`, the `RESOURCE` it holds, the tasks and the
//! program configurations of a resource.
//!
//! A configuration and a resource are each a header followed by variable
//! blocks and the declarations they hold, so they are read by the rule that
//! reads the sections of a unit (`pou`): its table has a row for each of them.
//! This module builds what each declaration makes of its parts.
//!
//! A task is a list of properties, and `TASK_PROPERTIES` has a row for each
//! word that may name one, with the rule that reads its value. A program
//! configuration is the program it runs, the task it runs in and a list of
//! connections, each of a kind of node that `PARTS` has a row for; a connection
//! is an input or an output, and `DIRECTIONS` has a row for each operator.
//!
//! A value that stands as a source or a sink (a task's single, an input, an
//! output) is classified once by the kind of node that writes it (`Source`),
//! and each place that reads one has a table of the classes it takes and what
//! it builds from each. A class a place does not take is a value that does not
//! stand there.

#[cfg(test)]
mod tests;

use super::expressions::lower_expr;

use super::names::{lower_name, lower_path, lower_type_ref};
use super::pou::{declared_name, lower_sections};
use super::tree::{child_of, children_of, significant_tokens};
use super::values::{lower_enumerated_value, lower_priority};
use super::var_blocks::{qualifier_of, InstanceInit};
use super::variables::{lower_address, lower_symbolic};
use super::LowerCx;
use crate::syntax_kind::{SyntaxKind as K, SyntaxNode};
use ironplc_dsl::common::{ConstantKind, DeclarationQualifier};
use ironplc_dsl::configuration::{
    ConfigurationDeclaration, DataSourceKind, FunctionBlockTask, GlobalVarReference,
    ProgramConfiguration, ProgramConnectionSink, ProgramConnectionSinkKind,
    ProgramConnectionSource, ProgramConnectionSourceKind, ResourceDeclaration, TaskConfiguration,
};
use ironplc_dsl::construct::literal_value_of;
use ironplc_dsl::core::Id;
use ironplc_dsl::diagnostic::Diagnostic;
use ironplc_dsl::time::DurationLiteral;

/// The kind of node that writes a value that is a source or a sink, which
/// decides what the value stands for.
#[derive(Clone, Copy, PartialEq, Eq, Debug)]
enum Source {
    /// A literal, a signed literal, or any other expression (which is no
    /// constant, and is reported where a constant is wanted).
    Value,
    /// `Type#Value`.
    Qualified,
    /// A name alone: an enumeration value, or the name of a global variable.
    Name,
    /// `a.b` and `a.b.c`: a global variable, selected from a resource or
    /// for one of its members.
    Path,
    /// A direct address.
    Direct,
    /// An array or a structure, which no place takes as a source.
    Aggregate,
}

/// The kinds of node that are not a plain value, and the class each is.
const SOURCES: &[(K, Source)] = &[
    (K::EnumValueRef, Source::Qualified),
    (K::NameRef, Source::Name),
    (K::FieldExpr, Source::Path),
    (K::DirectAddressExpr, Source::Direct),
    (K::ArrayInit, Source::Aggregate),
    (K::StructInit, Source::Aggregate),
];

fn source_of(node: &SyntaxNode) -> Source {
    SOURCES
        .iter()
        .find(|(kind, _)| *kind == node.kind())
        .map_or(Source::Value, |(_, source)| *source)
}

/// A rule: builds what a source or a sink of one class stands for.
type Build<T> = fn(&LowerCx, &SyntaxNode) -> Result<T, Diagnostic>;

/// Builds the source or sink in `node` with the row of `rows` for its class.
/// `expected` says what the place takes, for the problem reported when no row
/// is for the class.
fn read<T>(
    cx: &LowerCx,
    rows: &[(Source, Build<T>)],
    node: &SyntaxNode,
    expected: &str,
) -> Result<T, Diagnostic> {
    let source = source_of(node);
    match rows.iter().find(|(class, _)| *class == source) {
        Some((_, build)) => build(cx, node),
        None => Err(cx.syntax_error(node.text_range(), format!("expected {expected}"))),
    }
}

/// The constant a node writes: a literal, with its sign when it has one.
fn constant(cx: &LowerCx, node: &SyntaxNode) -> Result<ConstantKind, Diagnostic> {
    literal_value_of(&lower_expr(cx, node)?)
        .ok_or_else(|| cx.syntax_error(node.text_range(), "expected a constant"))
}

/// `[resource .] variable [. member]`: the global variable a path names. The
/// first of two names is the resource, as the legacy grammar reads it.
fn global_reference(cx: &LowerCx, node: &SyntaxNode) -> Result<GlobalVarReference, Diagnostic> {
    let mut names = lower_path(cx, node, "a global variable")?.into_iter();
    let reference = |resource_name, global_var_name, structure_element_name| GlobalVarReference {
        resource_name,
        global_var_name,
        structure_element_name,
    };
    match (names.next(), names.next(), names.next(), names.next()) {
        (Some(variable), None, _, _) => Ok(reference(None, variable, None)),
        (Some(resource), Some(variable), None, _) => Ok(reference(Some(resource), variable, None)),
        (Some(resource), Some(variable), Some(member), None) => {
            Ok(reference(Some(resource), variable, Some(member)))
        }
        _ => Err(cx.syntax_error(node.text_range(), "expected a global variable")),
    }
}

/// What a task's `SINGLE` is read from: a constant, or a global variable.
const DATA_SOURCES: &[(Source, Build<DataSourceKind>)] = &[
    (Source::Value, |cx, node| {
        constant(cx, node).map(DataSourceKind::Constant)
    }),
    (Source::Name, global_data_source),
    (Source::Path, global_data_source),
];

fn global_data_source(cx: &LowerCx, node: &SyntaxNode) -> Result<DataSourceKind, Diagnostic> {
    global_reference(cx, node).map(DataSourceKind::GlobalVarReference)
}

/// What an input of a program configuration is read from.
const CONNECTION_SOURCES: &[(Source, Build<ProgramConnectionSourceKind>)] = &[
    (Source::Value, |cx, node| {
        constant(cx, node).map(ProgramConnectionSourceKind::Constant)
    }),
    (Source::Qualified, enumerated_source),
    // A name alone is read as an enumeration value, as the legacy grammar
    // reads it: only the declaration of the global variable can say it is not.
    (Source::Name, enumerated_source),
    (Source::Direct, |cx, node| {
        lower_address(cx, node).map(ProgramConnectionSourceKind::DirectVariable)
    }),
];

fn enumerated_source(
    cx: &LowerCx,
    node: &SyntaxNode,
) -> Result<ProgramConnectionSourceKind, Diagnostic> {
    lower_enumerated_value(cx, node).map(ProgramConnectionSourceKind::EnumeratedValue)
}

/// What an output of a program configuration is written to.
const SINKS: &[(Source, Build<ProgramConnectionSinkKind>)] = &[
    (Source::Name, global_sink),
    (Source::Path, global_sink),
    (Source::Direct, |cx, node| {
        lower_address(cx, node).map(ProgramConnectionSinkKind::DirectVariable)
    }),
];

fn global_sink(cx: &LowerCx, node: &SyntaxNode) -> Result<ProgramConnectionSinkKind, Diagnostic> {
    global_reference(cx, node).map(ProgramConnectionSinkKind::GlobalVarReference)
}

/// What a task is given by its properties, before it is known to be complete.
#[derive(Default)]
struct Properties {
    single: Option<DataSourceKind>,
    interval: Option<DurationLiteral>,
    priority: Option<u32>,
}

/// A rule: reads the value of one property into the properties.
type Property = fn(&LowerCx, &SyntaxNode, &mut Properties) -> Result<(), Diagnostic>;

/// The words that name a task's properties, spelled as the grammar matches
/// them, and the rule that reads the value of each.
const TASK_PROPERTIES: &[(&str, Property)] = &[
    ("SINGLE", |cx, value, into| {
        into.single = Some(read(
            cx,
            DATA_SOURCES,
            value,
            "a constant or a global variable",
        )?);
        Ok(())
    }),
    ("INTERVAL", |cx, value, into| {
        match read(cx, DATA_SOURCES, value, "a duration")? {
            DataSourceKind::Constant(ConstantKind::Duration(duration)) => {
                into.interval = Some(duration);
                Ok(())
            }
            _ => Err(cx.syntax_error(value.text_range(), "expected a duration")),
        }
    }),
    ("PRIORITY", |cx, value, into| {
        into.priority = Some(lower_priority(cx, value)?);
        Ok(())
    }),
];

/// Reads one `property := value` into the properties of a task.
fn lower_property(
    cx: &LowerCx,
    item: &SyntaxNode,
    into: &mut Properties,
) -> Result<(), Diagnostic> {
    let mut parts = item.children();
    let (word, value) = match (parts.next(), parts.next()) {
        (Some(word), Some(value)) => (word, value),
        _ => return Err(cx.missing(item, "a property and its value")),
    };
    let word = lower_name(cx, &word)?;
    let (_, rule) = TASK_PROPERTIES
        .iter()
        .find(|(spelling, _)| *spelling == word.original())
        .ok_or_else(|| cx.missing(item, "SINGLE, INTERVAL or PRIORITY"))?;
    rule(cx, &value, into)
}

/// `TASK name ( [SINGLE := s,] [INTERVAL := i,] PRIORITY := n ) ;`
pub fn lower_task(cx: &LowerCx, node: &SyntaxNode) -> Result<TaskConfiguration, Diagnostic> {
    let mut properties = Properties::default();
    if let Some(init) = child_of(node, K::TaskInit) {
        for item in children_of(&init, K::TaskInitItem) {
            lower_property(cx, &item, &mut properties)?;
        }
    }
    Ok(TaskConfiguration {
        name: declared_name(cx, node)?,
        priority: properties
            .priority
            .ok_or_else(|| cx.missing(node, "a priority"))?,
        interval: properties.interval,
        single: properties.single,
    })
}

/// `function_block WITH task`
fn binding(
    cx: &LowerCx,
    node: &SyntaxNode,
    into: &mut ProgramConfiguration,
) -> Result<(), Diagnostic> {
    let mut names = children_of(node, K::NameRef);
    let (block, task) = match (names.next(), names.next()) {
        (Some(block), Some(task)) => (block, task),
        _ => return Err(cx.missing(node, "a function block and a task")),
    };
    into.fb_tasks.push(FunctionBlockTask {
        fb_name: lower_name(cx, &block)?,
        task_name: lower_name(cx, &task)?,
    });
    Ok(())
}

/// A rule: reads the variable of a connection and the value on the other side
/// of its operator.
type Connect =
    fn(&LowerCx, &SyntaxNode, &SyntaxNode, &mut ProgramConfiguration) -> Result<(), Diagnostic>;

/// The operators of a connection: `:=` gives an input a source, and `=>` sends
/// an output to a sink.
const DIRECTIONS: &[(K, Connect)] = &[
    (K::Assignment, |cx, variable, other, into| {
        into.sources.push(ProgramConnectionSource {
            dst: lower_symbolic(cx, variable)?,
            src: read(
                cx,
                CONNECTION_SOURCES,
                other,
                "a constant, an enumeration value or an address",
            )?,
        });
        Ok(())
    }),
    (K::RightArrow, |cx, variable, other, into| {
        into.sinks.push(ProgramConnectionSink {
            src: lower_symbolic(cx, variable)?,
            dst: read(cx, SINKS, other, "a global variable or an address")?,
        });
        Ok(())
    }),
];

/// `input := source` or `output => sink`
fn connection(
    cx: &LowerCx,
    node: &SyntaxNode,
    into: &mut ProgramConfiguration,
) -> Result<(), Diagnostic> {
    let mut parts = node.children();
    let (variable, other) = match (parts.next(), parts.next()) {
        (Some(variable), Some(other)) => (variable, other),
        _ => return Err(cx.missing(node, "a variable and what it connects to")),
    };
    let (_, rule) = significant_tokens(node)
        .iter()
        .find_map(|token| DIRECTIONS.iter().find(|(kind, _)| *kind == token.kind()))
        .ok_or_else(|| cx.missing(node, "`:=` or `=>`"))?;
    rule(cx, &variable, &other, into)
}

/// A rule: reads one part of a program configuration into it.
type PartRule = fn(&LowerCx, &SyntaxNode, &mut ProgramConfiguration) -> Result<(), Diagnostic>;

/// The kinds of node that hold the connections of a program configuration.
const PARTS: &[(K, PartRule)] = &[
    (K::TaskBinding, binding),
    (K::ProgramConnection, connection),
];

/// `PROGRAM [RETAIN | NON_RETAIN] name [WITH task] : type [( connection {,
/// connection} )] ;`
pub fn lower_program_configuration(
    cx: &LowerCx,
    node: &SyntaxNode,
) -> Result<ProgramConfiguration, Diagnostic> {
    let mut names = children_of(node, K::Name);
    let name = names
        .next()
        .ok_or_else(|| cx.missing(node, "a name"))
        .and_then(|name| lower_name(cx, &name))?;
    let task_name = names.next().map(|task| lower_name(cx, &task)).transpose()?;
    let type_name = child_of(node, K::TypeRef)
        .ok_or_else(|| cx.missing(node, "a program type"))
        .and_then(|program| lower_type_ref(cx, &program))?
        .name;
    let mut program = ProgramConfiguration {
        name,
        storage: Some(qualifier_of(node))
            .filter(|storage| *storage != DeclarationQualifier::Unspecified),
        task_name,
        type_name,
        fb_tasks: vec![],
        sources: vec![],
        sinks: vec![],
    };
    for (kind, rule) in PARTS {
        for part in children_of(node, *kind) {
            rule(cx, &part, &mut program)?;
        }
    }
    Ok(program)
}

/// `RESOURCE name ON type [VAR_GLOBAL ...] {TASK ...;} {PROGRAM ...;}
/// END_RESOURCE`
pub fn lower_resource(cx: &LowerCx, node: &SyntaxNode) -> Result<ResourceDeclaration, Diagnostic> {
    let sections = lower_sections(cx, node)?;
    let resource: Id = child_of(node, K::NameRef)
        .ok_or_else(|| cx.missing(node, "the type of the resource"))
        .and_then(|resource| lower_name(cx, &resource))?;
    Ok(ResourceDeclaration {
        name: declared_name(cx, node)?,
        resource,
        global_vars: sections.variables,
        tasks: sections.tasks,
        programs: sections.programs,
    })
}

/// `CONFIGURATION name [VAR_GLOBAL ...] RESOURCE ... [VAR_CONFIG ...]
/// END_CONFIGURATION`
pub fn lower_configuration(
    cx: &LowerCx,
    node: &SyntaxNode,
) -> Result<ConfigurationDeclaration, Diagnostic> {
    let sections = lower_sections(cx, node)?;
    let mut fb_inits = Vec::new();
    let mut located_var_inits = Vec::new();
    for init in sections.instances {
        match init {
            InstanceInit::FunctionBlock(init) => fb_inits.push(*init),
            InstanceInit::Located(init) => located_var_inits.push(*init),
        }
    }
    Ok(ConfigurationDeclaration {
        name: declared_name(cx, node)?,
        global_var: sections.variables,
        resource_decl: sections.resources,
        fb_inits,
        located_var_inits,
    })
}
