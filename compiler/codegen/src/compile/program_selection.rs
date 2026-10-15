//! Selects the one program the container runs and rejects a second program or instance.

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
        _ => Err(multiple_programs_not_implemented(
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

    Err(multiple_programs_not_implemented(
        "program instance",
        &instances
            .iter()
            .map(|instance| &instance.name)
            .collect::<Vec<_>>(),
    ))
}

/// Orders `items` by where their identifier appears in the source.
///
/// The analyzer's toposort reorders library elements, and for POUs with no
/// dependency edges between them the order comes out reversed. Sorting by
/// file and offset lets a diagnostic call a program "the second" in the sense
/// the reader expects: the second one in the file.
fn sort_by_source_position<T>(items: &mut [&T], id: impl Fn(&T) -> &Id) {
    items.sort_by_key(|item| {
        let span = &id(item).span;
        (span.file_id.to_string(), span.start)
    });
}

/// Builds the P9999 diagnostic for a second (or later) program.
///
/// `names` are in source order. The primary label sits on the second name,
/// the first one the compiler cannot honour. The first name and any later
/// ones get secondary labels so the diagnostic points at every program it is
/// about, not only the one that tipped the count.
fn multiple_programs_not_implemented(what: &str, names: &[&Id]) -> Diagnostic {
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
        format!(
            "{} {what}; the compiler currently runs only one PROGRAM",
            ordinal(2)
        ),
    ))
    .with_help(
        "Compile a single PROGRAM for now. Support for more than one PROGRAM is tracked in \
         https://github.com/ironplc/ironplc/issues/1613",
    );
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
