//! Selects the one program and the one configuration the container runs and rejects
//! a second program, instance or configuration.
//!
//! A container is one PLC: the VM and the container format know no configuration,
//! and version 1 of code generation supports one `PROGRAM`, one instance of it and
//! one task. Each of these limits is rejected by the one diagnostic
//! [`more_than_one_not_implemented`], which names every declaration it is about in
//! the order of the source.

use ironplc_dsl::common::{Library, LibraryElementKind, ProgramDeclaration};
use ironplc_dsl::configuration::{ConfigurationDeclaration, ProgramConfiguration};
use ironplc_dsl::core::{FileId, Id, Located};
use ironplc_dsl::diagnostic::{Diagnostic, Label};
use ironplc_problems::text;

/// Finds the one PROGRAM declaration that the container runs.
///
/// The VM runs a single program instance, so a library with two or more
/// `PROGRAM` declarations cannot be compiled faithfully. Keeping one of them
/// silently is worse than refusing: which one survived depended on the
/// analyzer's toposort order rather than on the source (issue #1588). Every
/// declaration beyond the first is therefore reported as not yet implemented.
pub(super) fn find_program(library: &Library) -> Result<&ProgramDeclaration, Diagnostic> {
    let mut programs: Vec<&ProgramDeclaration> = library
        .elements
        .iter()
        .filter_map(|element| match element {
            LibraryElementKind::ProgramDeclaration(program) => Some(program),
            _ => None,
        })
        .collect();
    sort_by_source_position(&mut programs, |program| &program.name);

    match programs.as_slice() {
        [] => Err(Diagnostic::text_in_file(
            FileId::default(),
            text::no_program_declaration(),
        )),
        [program] => Ok(program),
        _ => Err(more_than_one_not_implemented(
            Single::Program,
            "PROGRAM declaration",
            &programs
                .iter()
                .map(|program| &program.name)
                .collect::<Vec<_>>(),
        )),
    }
}

/// Rejects a configuration that instantiates more than one program.
///
/// `PROGRAM <instance> WITH <task> : <type>` lines are counted across every
/// `RESOURCE` in the configuration. Two instances of the same program type
/// are as unsupported as two different types: the VM has one variable table
/// and one program instance, so the second instance would be dropped.
pub(super) fn check_single_program_instance(
    config: &ConfigurationDeclaration,
) -> Result<(), Diagnostic> {
    let mut instances: Vec<&ProgramConfiguration> = config
        .resource_decl
        .iter()
        .flat_map(|resource| resource.programs.iter())
        .collect();
    if instances.len() < 2 {
        return Ok(());
    }
    sort_by_source_position(&mut instances, |instance| &instance.name);

    Err(more_than_one_not_implemented(
        Single::Program,
        "program instance",
        &instances
            .iter()
            .map(|instance| &instance.name)
            .collect::<Vec<_>>(),
    ))
}

/// Finds the one CONFIGURATION that the container carries, or none.
///
/// A container is one PLC and carries one configuration, so a library with two
/// or more `CONFIGURATION` declarations cannot be built faithfully. Building the
/// first and ignoring the others silently is worse than refusing, as it is for
/// a second program, so every declaration is reported, in the order of the
/// source ([`Library::configurations_in_source_order`]). A library with none is
/// a program that runs as it is, with no task of its own.
///
/// This runs next to [`find_program`], before anything uses the configuration.
pub(super) fn select_configuration(
    library: &Library,
) -> Result<Option<&ConfigurationDeclaration>, Diagnostic> {
    let configurations = library.configurations_in_source_order();
    match configurations.as_slice() {
        [] => Ok(None),
        [configuration] => Ok(Some(configuration)),
        _ => Err(more_than_one_not_implemented(
            Single::Configuration,
            "CONFIGURATION declaration",
            &configurations
                .iter()
                .map(|configuration| &configuration.name)
                .collect::<Vec<_>>(),
        )),
    }
}

/// Orders `items` by where their identifier appears in the source.
///
/// The analyzer's toposort reorders library elements, and for POUs with no
/// dependency edges between them the order comes out reversed. Sorting by
/// file and offset ([`Id::source_position`]) lets a diagnostic call a program
/// "the second" in the sense the reader expects: the second one in the file.
fn sort_by_source_position<T>(items: &mut [&T], id: impl Fn(&T) -> &Id) {
    items.sort_by_key(|item| id(item).source_position());
}

/// What version 1 of code generation builds only one of.
#[derive(Clone, Copy)]
enum Single {
    /// One `PROGRAM`, and one instance of it.
    Program,
    /// One `CONFIGURATION`: a container is one PLC.
    Configuration,
}

impl Single {
    /// The end of the primary label: what the compiler does today.
    fn limit(self) -> &'static str {
        match self {
            Single::Program => "the compiler currently runs only one PROGRAM",
            Single::Configuration => {
                "the compiler currently builds only one CONFIGURATION, because a container is one PLC"
            }
        }
    }

    /// The help note: what to do now.
    fn help(self) -> &'static str {
        match self {
            Single::Program => {
                "Compile a single PROGRAM for now. Support for more than one PROGRAM is tracked in \
                 https://github.com/ironplc/ironplc/issues/1613"
            }
            Single::Configuration => {
                "Compile a single CONFIGURATION for now: a container carries one PLC, so the \
                 configurations of a library cannot be built into one file."
            }
        }
    }
}

/// Builds the P9999 diagnostic for a second (or later) program, instance or
/// configuration.
///
/// `names` are in source order. The primary label sits on the second name,
/// the first one the compiler cannot honour. The first name and any later
/// ones get secondary labels so the diagnostic points at every declaration it
/// is about, not only the one that tipped the count.
fn more_than_one_not_implemented(single: Single, what: &str, names: &[&Id]) -> Diagnostic {
    let Some(second) = names.get(1) else {
        // Callers only get here with two or more names, so the first name,
        // when there is one, is the best location for the violation.
        return Diagnostic::internal_error_at(Label::span(
            names.first().map(|name| name.span()).unwrap_or_default(),
            format!("Fewer than two {what}s reported as too many"),
        ));
    };
    let mut diagnostic = Diagnostic::not_implemented(Label::span(
        second.span(),
        format!("{} {what}; {}", ordinal(2), single.limit()),
    ))
    .with_help(single.help());
    for (index, name) in names.iter().enumerate() {
        if index == 1 {
            continue;
        }
        diagnostic = diagnostic.with_secondary(Label::span(
            name.span(),
            format!("{} {what}", ordinal(index + 1)),
        ));
    }
    diagnostic
}

/// Formats a 1-based position as an English ordinal: `1st`, `2nd`, `3rd`, `11th`.
fn ordinal(position: usize) -> String {
    let suffix = if (11..=13).contains(&(position % 100)) {
        "th"
    } else {
        match position % 10 {
            1 => "st",
            2 => "nd",
            3 => "rd",
            _ => "th",
        }
    };
    format!("{position}{suffix}")
}
