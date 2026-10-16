//! Semantic rule that references to enumerations use enumeration values
//! that are part of the enumeration declaration.
//!
//! ## Passes
//!
//! ```ignore
//! TYPE
//!    LEVEL : (CRITICAL) := CRITICAL;
//! END_TYPE
//!
//! FUNCTION_BLOCK LOGGER
//!    VAR_INPUT
//!       LEVEL : LEVEL := CRITICAL;
//!    END_VAR
//! END_FUNCTION_BLOCK
//! ```
//!
//! ## Fails
//!
//! ```ignore
//! TYPE
//!    LEVEL : (INFO) := INFO;
//! END_TYPE
//!
//! FUNCTION_BLOCK LOGGER
//!    VAR_INPUT
//!       LEVEL : LEVEL := CRITICAL;
//!    END_VAR
//! END_FUNCTION_BLOCK
//! ```
//!
//! ## A value written with its type
//!
//! `Type#Value` names the enumeration it belongs to, wherever it is written: an
//! initial value, an element of an array or a structure, a `CASE` label, an
//! operand of an expression, an argument of a call. The type before the `#` must
//! be declared (P2008), must be an enumeration (P2004) and must declare the value
//! (P2006). The rule looks at the value and not at the place it stands, as
//! `rule_ambiguous_enumerated_value` does. Whether the enumeration is the one the
//! place expects is a question of types, answered where the types of the
//! operands are compared.
//!
//! ```ignore
//! TYPE LEVEL : (INFO, WARN); END_TYPE
//!
//! PROGRAM main
//!    VAR l : LEVEL; END_VAR
//!    l := LEVEL#INFO;     (* passes *)
//!    l := LEVEL#BLAH;     (* P2006: BLAH is not a value of LEVEL *)
//!    l := NOWHERE#INFO;   (* P2008: NOWHERE is not declared *)
//! END_PROGRAM
//! ```
use ironplc_dsl::{
    common::*,
    core::{Id, Located},
    diagnostic::Diagnostic,
    visitor::Visitor,
};
use ironplc_problems::text;
use std::convert::Infallible;

use crate::{
    resolution::Resolved,
    result::SemanticResult,
    rule_support::{run_rule, DiagnosticVisitor},
    semantic_context::SemanticContext,
};
use ironplc_parser::options::CompilerOptions;

pub fn apply(
    lib: &Library,
    context: &SemanticContext,
    _options: &CompilerOptions,
) -> SemanticResult {
    // Walk the library to find all references to enumerations
    // checking that all references use an enumeration value
    // that is part of the enumeration
    run_rule(RuleDeclaredEnumeratedValues::new(context), lib)
}

struct RuleDeclaredEnumeratedValues<'a> {
    context: &'a SemanticContext,
    diagnostics: Vec<Diagnostic>,
}

impl<'a> RuleDeclaredEnumeratedValues<'a> {
    fn new(context: &'a SemanticContext) -> Self {
        RuleDeclaredEnumeratedValues {
            context,
            diagnostics: Vec::new(),
        }
    }

    /// Returns the values of the enumeration a type name names.
    ///
    /// The type environment holds the values with the type, so an alias of an
    /// enumeration answers the values of the enumeration it names.
    ///
    /// # Errors
    ///
    /// Returns a diagnostic if the type name does not name an enumeration.
    fn find_enum_declaration_values(&self, type_name: &TypeName) -> Result<&'a [Id], Diagnostic> {
        self.context
            .types()
            .enumerated_values(type_name)
            .ok_or_else(|| {
                Diagnostic::text(
                    type_name.span(),
                    text::enum_not_declared_type_is_not_an_enumeration(),
                )
            })
    }

    /// Reports what is wrong with a value written `Type#Value`: the type is
    /// not declared, is not an enumeration, or does not declare the value.
    /// Returns whether it reported. A value written without its type, and a
    /// type that is declared with an error (reported where it is declared),
    /// have nothing to check here.
    fn report_qualified_value(&mut self, value: &EnumeratedValue) -> bool {
        let Some(type_name) = &value.type_name else {
            return false;
        };
        match self.context.types().lookup(type_name) {
            Resolved::Failed => return false,
            Resolved::Absent => {
                self.diagnostics.push(Diagnostic::text(
                    type_name.span(),
                    text::undeclared_unknown_type_enumeration_value_type(),
                ));
                return true;
            }
            Resolved::Valid(_) => {}
        }
        match self.find_enum_declaration_values(type_name) {
            Err(diagnostic) => {
                self.diagnostics.push(diagnostic);
                true
            }
            Ok(defined_values) if !defined_values.contains(&value.value) => {
                self.diagnostics.push(self.value_not_defined(value));
                true
            }
            Ok(_) => false,
        }
    }

    fn value_not_defined(&self, value: &EnumeratedValue) -> Diagnostic {
        Diagnostic::text(
            value.span(),
            text::enum_value_not_defined_expected_value_in_enumeration(),
        )
        .with_context_id("value", &value.value)
    }
}

impl DiagnosticVisitor for RuleDeclaredEnumeratedValues<'_> {
    fn into_diagnostics(self) -> Vec<Diagnostic> {
        self.diagnostics
    }
}

impl Visitor<Infallible> for RuleDeclaredEnumeratedValues<'_> {
    type Value = ();

    fn visit_enumerated_initial_value_assignment(
        &mut self,
        init: &EnumeratedInitialValueAssignment,
    ) -> Result<Self::Value, Infallible> {
        // A declaration with an error has no values to check this one
        // against, and is reported where it is declared.
        if self.context.types().lookup(&init.type_name).is_failed() {
            return Ok(());
        }
        let defined_values = match self.find_enum_declaration_values(&init.type_name) {
            Ok(values) => values,
            Err(diagnostic) => {
                // The type is not an enumeration, so there is nothing to check
                // this initializer's value against. Report that and carry on to
                // the next declaration.
                self.diagnostics.push(diagnostic);
                return Ok(());
            }
        };
        if let Some(value) = &init.initial_value {
            // The walk does not go on into the value, so a value written with
            // its type is checked here. Its own enumeration is reported once,
            // and is not checked again against the type of the variable.
            if self.report_qualified_value(value) {
                return Ok(());
            }
            // Check if the value is in the list of defined enumeration values
            if !defined_values.contains(&value.value) {
                self.diagnostics.push(self.value_not_defined(value));
            }
        }

        Ok(())
    }

    fn visit_enumerated_value(&mut self, node: &EnumeratedValue) -> Result<(), Infallible> {
        self.report_qualified_value(node);
        Ok(())
    }
}

#[cfg(test)]
mod tests {

    use crate::stages::analyze;
    use ironplc_dsl::core::FileId;
    use ironplc_parser::{options::CompilerOptions, parse_program};
    use rstest::rstest;

    #[test]
    fn apply_when_two_undefined_enum_values_then_reports_both() {
        let program = "
TYPE
LEVEL : (INFO, WARN) := INFO;
END_TYPE

FUNCTION_BLOCK LOGGER
VAR_INPUT
A : LEVEL := CRITICAL;
B : LEVEL := FATAL;
END_VAR
END_FUNCTION_BLOCK";

        let library =
            parse_program(program, &FileId::default(), &CompilerOptions::default()).unwrap();
        let (_library, context) = analyze(&[&library], &CompilerOptions::default()).unwrap();

        let reported: Vec<&String> = context
            .diagnostics()
            .iter()
            .flat_map(|d| &d.described)
            .collect();
        assert!(
            reported.iter().any(|d| d.as_str() == "value=CRITICAL"),
            "expected CRITICAL, got {reported:?}"
        );
        assert!(
            reported.iter().any(|d| d.as_str() == "value=FATAL"),
            "expected FATAL, got {reported:?}"
        );
    }

    #[test]
    fn apply_when_multiple_enum_values_with_one_undefined_then_error() {
        let program = "
TYPE
LEVEL : (INFO, WARN) := INFO;
END_TYPE

FUNCTION_BLOCK LOGGER
VAR_INPUT
A : LEVEL := INFO;
B : LEVEL := CRITICAL;
END_VAR
END_FUNCTION_BLOCK";

        let library =
            parse_program(program, &FileId::default(), &CompilerOptions::default()).unwrap();
        let result = analyze(&[&library], &CompilerOptions::default());

        let (_library, context) = result.unwrap();
        assert!(context.has_diagnostics());
    }

    #[test]
    fn apply_when_var_init_undefined_enum_value_then_error() {
        let program = "
TYPE
LEVEL : (INFO) := INFO;
END_TYPE
        
FUNCTION_BLOCK LOGGER
VAR_INPUT
LEVEL : LEVEL := CRITICAL;
END_VAR
END_FUNCTION_BLOCK";

        let library =
            parse_program(program, &FileId::default(), &CompilerOptions::default()).unwrap();
        let result = analyze(&[&library], &CompilerOptions::default());

        let (_library, context) = result.unwrap();
        assert!(context.has_diagnostics());
    }

    #[test]
    fn apply_when_var_init_valid_enum_value_then_ok() {
        let program = "
TYPE
LEVEL : (CRITICAL) := CRITICAL;
END_TYPE

FUNCTION_BLOCK LOGGER
VAR_INPUT
LEVEL : LEVEL := CRITICAL;
END_VAR
END_FUNCTION_BLOCK";

        let library =
            parse_program(program, &FileId::default(), &CompilerOptions::default()).unwrap();
        let result = analyze(&[&library], &CompilerOptions::default());

        assert!(result.is_ok());
    }

    #[test]
    fn apply_when_var_init_valid_enum_value_through_alias_then_ok() {
        let program = "
TYPE
LEVEL : (CRITICAL) := CRITICAL;
LEVEL_ALIAS : LEVEL;
END_TYPE

FUNCTION_BLOCK LOGGER
VAR_INPUT
NAME : LEVEL_ALIAS := CRITICAL;
END_VAR

END_FUNCTION_BLOCK";

        let library =
            parse_program(program, &FileId::default(), &CompilerOptions::default()).unwrap();
        let result = analyze(&[&library], &CompilerOptions::default());

        assert!(result.is_ok());
    }

    /// How many times a program is analyzed to see the result of every
    /// analysis. Each analysis makes containers of its own, so a result that
    /// depends on the order of a hash container varies within one process.
    const RUNS: usize = 24;

    /// The distinct results of analyzing `program` `RUNS` times, each result
    /// the sorted codes of the diagnostics reported.
    fn distinct_results(program: &str) -> Vec<Vec<String>> {
        let library =
            parse_program(program, &FileId::default(), &CompilerOptions::default()).unwrap();
        let mut results: Vec<Vec<String>> = (0..RUNS)
            .map(|_| {
                let (_library, context) =
                    analyze(&[&library], &CompilerOptions::default()).unwrap();
                let mut codes: Vec<String> = context
                    .diagnostics()
                    .iter()
                    .map(|d| d.code.clone())
                    .collect();
                codes.sort();
                codes
            })
            .collect();
        results.sort();
        results.dedup();
        results
    }

    /// A program with the two variables `declarations` declare, against the
    /// enumerations and aliases `types` declare.
    fn program_of(types: &str, declarations: &str) -> String {
        format!("TYPE {types} END_TYPE PROGRAM main VAR {declarations} END_VAR END_PROGRAM")
    }

    const NONE: &[&str] = &[];
    const P2006: &[&str] = &["P2006"];
    /// A bare value name that two enumerations declare is refused where it is
    /// used (`rule_ambiguous_enumerated_value`), whatever its type expects.
    const P2042: &[&str] = &["P2042"];

    #[rstest]
    // Enumerations that share no value name.
    #[case::own_value("E1 : (A, B); E2 : (C, D);", "v : E1 := A;", NONE)]
    #[case::other_enumerations_value("E1 : (A, B); E2 : (C, D);", "v : E1 := C;", P2006)]
    #[case::own_value_of_second("E1 : (A, B); E2 : (C, D);", "v : E2 := C;", NONE)]
    // Enumerations that share a value name: the name is a value of each.
    #[case::shared_value_of_first("E1 : (U1, U2); E2 : (U1, U3);", "v : E1 := U1;", P2042)]
    #[case::shared_value_of_second("E1 : (U1, U2); E2 : (U1, U3);", "w : E2 := U1;", P2042)]
    #[case::shared_value_not_of_second("E1 : (U1, U2); E2 : (U1, U3);", "w : E2 := U2;", P2006)]
    #[case::shared_value_not_of_first("E1 : (U1, U2); E2 : (U1, U3);", "v : E1 := U3;", P2006)]
    // An alias has the values of the enumeration its declaration names.
    #[case::alias_own_value("E1 : (A, B); E2 : (C, D); EA : E1;", "v : EA := A;", NONE)]
    #[case::alias_other_enumerations_value(
        "E1 : (A, B); E2 : (C, D); EA : E1;",
        "v : EA := C;",
        P2006
    )]
    #[case::alias_of_second_enumeration("E1 : (A, B); E2 : (C, D); EA : E2;", "v : EA := C;", NONE)]
    #[case::alias_of_second_not_first_value(
        "E1 : (A, B); E2 : (C, D); EA : E2;",
        "v : EA := A;",
        P2006
    )]
    #[case::alias_of_alias_own_value(
        "E1 : (A, B); E2 : (C, D); EA : E1; EB : EA;",
        "v : EB := B;",
        NONE
    )]
    #[case::alias_of_alias_other_value(
        "E1 : (A, B); E2 : (C, D); EA : E1; EB : EA;",
        "v : EB := D;",
        P2006
    )]
    #[case::alias_with_shared_value_name(
        "E1 : (U1, U2); E2 : (U1, U3); EA : E2;",
        "v : EA := U1;",
        P2042
    )]
    #[case::alias_with_shared_value_name_not_of_base(
        "E1 : (U1, U2); E2 : (U1, U3); EA : E2;",
        "v : EA := U2;",
        P2006
    )]
    #[case::aliases_of_two_enumerations(
        "E1 : (U1, U2); E2 : (U1, U3); EA : E1; EB : E2;",
        "v : EA := U2; w : EB := U3;",
        NONE
    )]
    fn apply_when_enumerations_declared_then_value_is_checked_against_its_type_in_every_run(
        #[case] types: &str,
        #[case] declarations: &str,
        #[case] expected: &[&str],
    ) {
        let expected: Vec<String> = expected.iter().map(|code| code.to_string()).collect();

        let results = distinct_results(&program_of(types, declarations));

        assert_eq!(results, vec![expected]);
    }

    /// A value name in an expression has no enumeration to be checked against:
    /// the type of an unqualified value is not known from its spelling, so
    /// the name is an enumerated value when any enumeration declares it and
    /// nothing more. A name that two enumerations declare is refused where it
    /// is used (`rule_ambiguous_enumerated_value`); a name that one declares
    /// is not checked against the type the context expects, which is a
    /// separate piece of work, and when it exists `y := U2` below is reported.
    /// The report is the same in every run.
    #[rstest]
    #[case::compared_with_first_enumeration("IF x = U1 THEN r := TRUE; END_IF;", P2042)]
    #[case::compared_with_second_enumeration("IF y = U1 THEN r := TRUE; END_IF;", P2042)]
    #[case::assigned_to_each_enumeration("x := U1; y := U1;", &["P2042", "P2042"])]
    #[case::case_labels_of_the_selectors_enumeration(
        "CASE x OF U1: r := TRUE; U2: r := FALSE; END_CASE;",
        P2042
    )]
    #[case::value_of_the_other_enumeration_not_checked("y := U2;", NONE)]
    fn apply_when_value_name_in_expression_then_only_a_shared_name_is_reported_in_every_run(
        #[case] body: &str,
        #[case] expected: &[&str],
    ) {
        let program = format!(
            "TYPE E1 : (U1, U2); E2 : (U1, U3); END_TYPE              PROGRAM main VAR x : E1; y : E2; r : BOOL; END_VAR {body} END_PROGRAM"
        );
        let expected: Vec<String> = expected.iter().map(|code| code.to_string()).collect();

        assert_eq!(distinct_results(&program), vec![expected]);
    }

    const P2004: &[&str] = &["P2004"];
    const P2008: &[&str] = &["P2008"];

    /// The enumerations a value written with its type is checked against:
    /// `E_State`, two that declare the same value name, and a type that is
    /// no enumeration.
    const QUALIFIED_TYPES: &str = "E_State : (Idle, Run, Stop); E1 : (A, B); E2 : (A, C); R : INT;         S : STRUCT m : E_State := E_State#Run; END_STRUCT;";

    /// A program of the variables used by the cases below, and a function that
    /// takes an enumeration.
    fn qualified_program(body: &str) -> String {
        format!(
            "TYPE {QUALIFIED_TYPES} END_TYPE              FUNCTION is_run : BOOL VAR_INPUT v : E_State; END_VAR is_run := v = E_State#Run; END_FUNCTION              PROGRAM main VAR s : E_State; b : BOOL; x : E1; y : E2; END_VAR {body} END_PROGRAM"
        )
    }

    #[rstest]
    // Written with a declared enumeration that declares the value.
    #[case::assigned("s := E_State#Run;", NONE)]
    #[case::compared("b := s = E_State#Stop;", NONE)]
    #[case::compared_on_the_left("b := E_State#Idle <> s;", NONE)]
    #[case::argument("b := is_run(E_State#Run);", NONE)]
    #[case::named_argument("b := is_run(v := E_State#Run);", NONE)]
    #[case::case_label("CASE s OF E_State#Run: b := TRUE; Idle: b := FALSE; END_CASE;", NONE)]
    #[case::value_name_of_two_enumerations_in_the_first("x := E1#A;", NONE)]
    #[case::value_name_of_two_enumerations_in_the_second("y := E2#A;", NONE)]
    #[case::value_of_the_second_enumeration_only("y := E2#C;", NONE)]
    // The type before the # is not declared.
    #[case::undeclared_assigned("s := E_Nope#Run;", P2008)]
    #[case::undeclared_compared("b := s = E_Nope#Run;", P2008)]
    #[case::undeclared_argument("b := is_run(E_Nope#Run);", P2008)]
    #[case::undeclared_case_label("CASE s OF E_Nope#Run: b := TRUE; END_CASE;", P2008)]
    // The type before the # is not an enumeration.
    #[case::not_an_enumeration_assigned("s := R#Run;", P2004)]
    #[case::not_an_enumeration_case_label("CASE s OF R#Run: b := TRUE; END_CASE;", P2004)]
    #[case::structure_is_not_an_enumeration("s := S#Run;", P2004)]
    // The enumeration does not declare the value.
    #[case::not_a_value_assigned("s := E_State#Nope;", P2006)]
    #[case::not_a_value_compared("b := s = E_State#Nope;", P2006)]
    #[case::not_a_value_argument("b := is_run(E_State#Nope);", P2006)]
    #[case::not_a_value_case_label("CASE s OF E_State#Nope: b := TRUE; END_CASE;", P2006)]
    #[case::value_of_another_enumeration("x := E1#C;", P2006)]
    #[case::each_fault_once("s := E_Nope#Run; s := E_State#Nope;", &["P2006", "P2008"])]
    fn apply_when_value_written_with_its_type_then_the_type_is_declared_an_enumeration_of_the_value(
        #[case] body: &str,
        #[case] expected: &[&str],
    ) {
        let expected: Vec<String> = expected.iter().map(|code| code.to_string()).collect();

        let results = distinct_results(&qualified_program(body));

        assert_eq!(results, vec![expected]);
    }

    #[rstest]
    // The initial value of a variable, of an element of an array, and the default
    // of a member of a structure.
    #[case::initial_value("v : E_State := E_State#Run;", NONE)]
    #[case::initial_value_shared_name("v : E1 := E1#A; w : E2 := E2#A;", NONE)]
    #[case::initial_value_undeclared("v : E_State := E_Nope#Run;", P2008)]
    #[case::initial_value_not_an_enumeration("v : E_State := R#Run;", P2004)]
    #[case::initial_value_not_a_value("v : E_State := E_State#Nope;", P2006)]
    #[case::array_element("v : ARRAY[1..2] OF E_State := [E_State#Run, E_State#Stop];", NONE)]
    #[case::array_element_undeclared(
        "v : ARRAY[1..2] OF E_State := [E_State#Run, E_Nope#Stop];",
        P2008
    )]
    #[case::array_element_not_a_value(
        "v : ARRAY[1..2] OF E_State := [E_State#Run, E_State#Nope];",
        P2006
    )]
    #[case::structure_member("v : S := (m := E_State#Stop);", NONE)]
    #[case::structure_member_undeclared("v : S := (m := E_Nope#Stop);", P2008)]
    #[case::structure_member_not_a_value("v : S := (m := E_State#Nope);", P2006)]
    fn apply_when_initial_value_written_with_its_type_then_checked_like_one_in_a_statement(
        #[case] declarations: &str,
        #[case] expected: &[&str],
    ) {
        let expected: Vec<String> = expected.iter().map(|code| code.to_string()).collect();

        let results = distinct_results(&program_of(QUALIFIED_TYPES, declarations));

        assert_eq!(results, vec![expected]);
    }

    rule_ctx_ok!(
        apply_when_value_is_written_with_a_type_declared_with_an_error_then_ok,
        "
TYPE E : E_NOWHERE := A1; END_TYPE
PROGRAM main
VAR v : E := E#A1; w : E; END_VAR
w := E#A1;
END_PROGRAM"
    );

    rule_ctx_ok!(
        apply_when_enumeration_is_declared_with_an_error_then_ok,
        "
TYPE E : E_NOWHERE := A1; END_TYPE
PROGRAM main
VAR v : E; END_VAR
END_PROGRAM"
    );
}
