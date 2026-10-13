//! Semantic rule that the attributes of a declaration stand where they may,
//! take the values they take, and are not written twice.
//!
//! The registry (`ironplc_dsl::attribute::registry`) holds what the compiler
//! knows about each attribute and does the check: this rule asks it for the
//! faults of every declaration of the library (the census lists them) and turns
//! each fault into a diagnostic. It holds no knowledge of any attribute, so an
//! attribute added to the registry is checked without a change here. An
//! attribute that has no row is user-defined and is accepted.
//!
//! An attribute written before a group (`VAR ... END_VAR`, `TYPE ... END_TYPE`)
//! is attached to each declaration of the group with one span, so the same
//! fault is found once for each declaration. The rule reports it once for each
//! problem and span.
//!
//! ## Passes
//!
//! ```ignore
//! {attribute 'pack_mode' := '2'}
//! TYPE Packet : STRUCT
//!    a : INT;
//! END_STRUCT; END_TYPE
//! ```
//!
//! ## Fails
//!
//! ```ignore
//! PROGRAM main
//!    VAR
//!       {attribute 'pack_mode' := '1'}   (* a structure type takes it, not a variable *)
//!       a : INT;
//!    END_VAR
//! END_PROGRAM
//! ```
use std::collections::HashSet;

use ironplc_dsl::{
    attribute::{
        census::declared,
        registry::{faults, Fault, FaultKind},
    },
    common::Library,
    core::FileId,
    diagnostic::{Diagnostic, Label},
};
use ironplc_parser::options::CompilerOptions;
use ironplc_problems::Problem;

use crate::{result::SemanticResult, semantic_context::SemanticContext};

pub fn apply(
    lib: &Library,
    _context: &SemanticContext,
    _options: &CompilerOptions,
) -> SemanticResult {
    let mut reported: HashSet<(String, FileId, usize, usize)> = HashSet::new();
    let diagnostics: Vec<Diagnostic> = declared(lib)
        .iter()
        .flat_map(|declaration| faults(declaration.kind, &declaration.attributes))
        .map(diagnostic)
        .filter(|diagnostic| {
            let at = &diagnostic.primary;
            reported.insert((
                diagnostic.code.clone(),
                at.file_id.clone(),
                at.location.start,
                at.location.end,
            ))
        })
        .collect();

    if diagnostics.is_empty() {
        Ok(())
    } else {
        Err(diagnostics)
    }
}

/// The diagnostic of one fault: one arm for each kind of fault.
fn diagnostic(fault: Fault) -> Diagnostic {
    let name = fault.name;
    match fault.kind {
        FaultKind::NotAllowedHere { row, kind } => Diagnostic::problem(
            Problem::AttributeNotAllowedHere,
            Label::span(
                fault.span,
                format!(
                    "The attribute '{name}' does not apply to a declaration of this kind ({}); it applies to: {}",
                    kind.label(),
                    row.targets.describe()
                ),
            ),
        ),
        FaultKind::Value { row, found } => Diagnostic::problem(
            Problem::AttributeValueInvalid,
            Label::span(
                fault.span,
                format!(
                    "The attribute '{name}' takes {}, but {}",
                    row.value.describe(),
                    match found {
                        Some(value) => format!("its value is '{value}'"),
                        None => "it has no value".to_string(),
                    }
                ),
            ),
        ),
        FaultKind::Repeated { first } => Diagnostic::problem(
            Problem::AttributeRepeated,
            Label::span(
                fault.span,
                format!("The attribute '{name}' is written again for the same declaration"),
            ),
        )
        .with_secondary(Label::span(first, "First written here")),
    }
}

#[cfg(test)]
mod tests {
    use ironplc_dsl::{core::FileId, diagnostic::Diagnostic};
    use ironplc_parser::{
        options::{CompilerOptions, Dialect},
        parse_program,
    };
    use ironplc_problems::Problem;
    use rstest::rstest;
    use spec_test_macro::spec_test;

    use crate::stages::analyze;

    /// The diagnostics of the analysis of `source` written for CODESYS: what
    /// the semantic rules report is kept in the context of the analysis.
    fn reported(source: &str) -> Vec<Diagnostic> {
        let options = CompilerOptions::from_dialect(Dialect::Codesys);
        let library = parse_program(source, &FileId::default(), &options).unwrap();
        match analyze(&[&library], &options) {
            Ok((_, context)) => context.diagnostics().to_vec(),
            Err(diagnostics) => diagnostics,
        }
    }

    /// The places of the first occurrence of `text` in `source`.
    fn at(source: &str, text: &str) -> (usize, usize) {
        let start = source.find(text).unwrap();
        (start, start + text.len())
    }

    fn primary_place(diagnostic: &Diagnostic) -> (usize, usize) {
        (
            diagnostic.primary.location.start,
            diagnostic.primary.location.end,
        )
    }

    const STRUCTURE_PROGRAM: &str = "{attribute 'pack_mode' := '2'}
TYPE Packet : STRUCT
    a : INT;
END_STRUCT; END_TYPE

PROGRAM main
VAR
    p : Packet;
END_VAR
END_PROGRAM";

    #[spec_test(REQ_ATTR_analyzer_010)]
    fn apply_when_known_attribute_stands_before_a_declaration_it_does_not_apply_to_then_p4076() {
        let source = "PROGRAM main
VAR
    {attribute 'pack_mode' := '1'}
    a : INT;
END_VAR
END_PROGRAM";

        let found = reported(source);

        assert_eq!(found.len(), 1, "{found:?}");
        assert_eq!(found[0].code, Problem::AttributeNotAllowedHere.code());
        assert_eq!(
            primary_place(&found[0]),
            at(source, "{attribute 'pack_mode' := '1'}")
        );
        let message = &found[0].primary.message;
        assert!(message.contains("'pack_mode'"), "{message}");
        assert!(
            message.contains("a declaration of this kind (variable)"),
            "{message}"
        );
        assert!(
            message.contains("it applies to: array type, structure type, union type"),
            "{message}"
        );
    }

    #[spec_test(REQ_ATTR_analyzer_010)]
    fn apply_when_attribute_is_written_in_other_case_then_it_is_still_the_known_attribute() {
        let source = "PROGRAM main
VAR
    {attribute 'PACK_MODE' := '1'}
    a : INT;
END_VAR
END_PROGRAM";

        let found = reported(source);

        assert_eq!(found.len(), 1, "{found:?}");
        assert!(found[0].primary.message.contains("'PACK_MODE'"));
    }

    #[rstest]
    #[case::value_to_a_flag("{attribute 'hide' := 'yes'}", "takes no value", "its value is 'yes'")]
    #[case::value_missing("{attribute 'pack_mode'}", "one of 0, 1, 2, 4, 8", "it has no value")]
    #[case::value_not_listed(
        "{attribute 'pack_mode' := '3'}",
        "one of 0, 1, 2, 4, 8",
        "its value is '3'"
    )]
    #[spec_test(REQ_ATTR_analyzer_011)]
    fn apply_when_known_attribute_has_a_value_it_does_not_take_then_p4077(
        #[case] pragma: &str,
        #[case] takes: &str,
        #[case] found_text: &str,
    ) {
        let source =
            format!("{pragma}\nTYPE Packet : STRUCT\n    a : INT;\nEND_STRUCT; END_TYPE\n");
        let program = format!("{source}\nPROGRAM main\nVAR\n    p : Packet;\nEND_VAR\nEND_PROGRAM");

        let found = reported(&program);

        assert_eq!(found.len(), 1, "{found:?}");
        assert_eq!(found[0].code, Problem::AttributeValueInvalid.code());
        assert_eq!(primary_place(&found[0]), at(&program, pragma));
        let message = &found[0].primary.message;
        assert!(message.contains(takes), "{message}");
        assert!(message.contains(found_text), "{message}");
    }

    #[spec_test(REQ_ATTR_analyzer_011)]
    fn apply_when_flag_attribute_has_a_value_before_a_variable_then_p4077() {
        let source = "PROGRAM main
VAR
    {attribute 'hide' := 'yes'}
    a : INT;
END_VAR
END_PROGRAM";

        let found = reported(source);

        assert_eq!(found.len(), 1, "{found:?}");
        assert_eq!(found[0].code, Problem::AttributeValueInvalid.code());
    }

    #[rstest]
    #[case::same_name("{attribute 'hide'}\n    {attribute 'hide'}")]
    #[case::other_case("{attribute 'hide'}\n    {attribute 'HIDE'}")]
    #[case::alias("{attribute 'noinit'}\n    {attribute 'no-init'}")]
    #[spec_test(REQ_ATTR_analyzer_012)]
    fn apply_when_attribute_is_written_twice_for_one_declaration_then_p4078_at_the_second_and_labelled_with_the_first(
        #[case] pragmas: &str,
    ) {
        let source =
            format!("PROGRAM main\nVAR\n    {pragmas}\n    a : INT;\nEND_VAR\nEND_PROGRAM");
        let first = at(&source, "{attribute");
        let second_start = source[first.1..].find("{attribute").unwrap() + first.1;
        let second = (
            second_start,
            source[second_start..].find('}').unwrap() + second_start + 1,
        );

        let found = reported(&source);

        assert_eq!(found.len(), 1, "{found:?}");
        assert_eq!(found[0].code, Problem::AttributeRepeated.code());
        assert_eq!(primary_place(&found[0]), second);
        assert_eq!(found[0].secondary.len(), 1);
        assert_eq!(
            (
                found[0].secondary[0].location.start,
                found[0].secondary[0].location.end
            ),
            (first.0, source[first.0..].find('}').unwrap() + first.0 + 1)
        );
    }

    #[spec_test(REQ_ATTR_analyzer_013)]
    fn apply_when_attribute_stands_before_a_block_of_three_variables_then_one_diagnostic() {
        let source = "PROGRAM main
{attribute 'pack_mode' := '1'}
VAR
    a, b : INT;
    c : INT;
END_VAR
END_PROGRAM";

        let found = reported(source);

        assert_eq!(found.len(), 1, "{found:?}");
        assert_eq!(found[0].code, Problem::AttributeNotAllowedHere.code());
        assert_eq!(
            primary_place(&found[0]),
            at(source, "{attribute 'pack_mode' := '1'}")
        );
    }

    #[spec_test(REQ_ATTR_analyzer_013)]
    fn apply_when_attribute_of_a_block_is_repeated_on_each_of_its_variables_then_one_diagnostic() {
        let source = "PROGRAM main
{attribute 'hide'}
VAR
    {attribute 'hide'}
    a : INT;
    b : INT;
END_VAR
END_PROGRAM";

        let found = reported(source);

        assert_eq!(found.len(), 1, "{found:?}");
        assert_eq!(found[0].code, Problem::AttributeRepeated.code());
    }

    #[spec_test(REQ_ATTR_analyzer_013)]
    fn apply_when_different_faults_in_one_program_then_each_is_reported_once() {
        let source = "PROGRAM main
{attribute 'pack_mode' := '1'}
VAR
    a, b, c : INT;
    {attribute 'hide' := 'x'}
    d : INT;
END_VAR
END_PROGRAM";

        let mut codes: Vec<String> = reported(source).into_iter().map(|d| d.code).collect();
        codes.sort();

        assert_eq!(
            codes,
            vec![
                Problem::AttributeNotAllowedHere.code().to_string(),
                Problem::AttributeValueInvalid.code().to_string()
            ]
        );
    }

    #[spec_test(REQ_ATTR_analyzer_014)]
    fn apply_when_attribute_is_user_defined_then_no_diagnostic() {
        let source = "{attribute 'my_company_tag' := 'x'}
PROGRAM main
VAR
    {attribute 'my_company_tag'}
    {attribute 'Other.Tag' := ''}
    a : INT;
END_VAR
END_PROGRAM";

        assert!(reported(source).is_empty());
    }

    #[rstest]
    #[case::structure_type(STRUCTURE_PROGRAM)]
    #[case::enumeration_and_global_variables(
        "{attribute 'Qualified_Only'}
TYPE Color : (Red, Green); END_TYPE

{attribute 'qualified_only'}
VAR_GLOBAL
    g : INT;
END_VAR

PROGRAM main
END_PROGRAM"
    )]
    #[case::array_type(
        "{attribute 'pack_mode' := '2'}
TYPE Samples : ARRAY [0..3] OF INT; END_TYPE

PROGRAM main
END_PROGRAM"
    )]
    #[case::program_without_attributes("PROGRAM main END_PROGRAM")]
    #[spec_test(REQ_ATTR_analyzer_014)]
    fn apply_when_known_attributes_are_where_they_may_stand_then_no_diagnostic(
        #[case] source: &str,
    ) {
        assert!(reported(source).is_empty());
    }
}
