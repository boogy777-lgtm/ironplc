//! Tests of the configurations: what each declaration makes of its parts, a row
//! of `TASK_PROPERTIES`, of the tables of sources and sinks and of `DIRECTIONS`
//! for each form a task or a connection may be written in.

use super::*;
use crate::lower::{lower_library, INTERNAL_ERROR};
use crate::{parse_source_file, ParseOptions};
use ironplc_dsl::common::{LibraryElementKind, VariableType};
use ironplc_dsl::core::FileId;
use ironplc_problems::Problem;

fn file() -> FileId {
    FileId::from_string("t.st")
}

/// The configuration `source` is written as, lowered with every keyword
/// enabled.
fn lowered(source: &str) -> Result<ConfigurationDeclaration, String> {
    let parse = parse_source_file(source, &ParseOptions::all());
    assert!(parse.is_ok(), "{source}: {:?}", parse.errors);
    let library = lower_library(&parse, &file()).map_err(|error| error.code)?;
    let configuration = match library.elements.into_iter().next() {
        Some(LibraryElementKind::ConfigurationDeclaration(configuration)) => Some(configuration),
        _ => None,
    };
    Ok(configuration.expect("a configuration"))
}

/// The configuration of a resource that holds `body`.
fn configuration(body: &str) -> ConfigurationDeclaration {
    lowered(&format!(
        "CONFIGURATION c RESOURCE r ON t {body} END_RESOURCE END_CONFIGURATION"
    ))
    .expect("the configuration lowers")
}

fn error_code(body: &str) -> String {
    lowered(&format!(
        "CONFIGURATION c RESOURCE r ON t {body} END_RESOURCE END_CONFIGURATION"
    ))
    .err()
    .unwrap_or_default()
}

fn task(properties: &str) -> TaskConfiguration {
    let configuration = configuration(&format!("TASK k ({properties}); PROGRAM p WITH k : q;"));
    configuration.resource_decl[0].tasks[0].clone()
}

fn program(declaration: &str) -> ProgramConfiguration {
    let configuration = configuration(&format!("TASK k (PRIORITY := 1); {declaration}"));
    configuration.resource_decl[0].programs[0].clone()
}

fn names(ids: &[Id]) -> Vec<String> {
    ids.iter().map(|id| id.original().to_string()).collect()
}

// ---- Configurations and resources.

#[test]
fn lower_configuration_when_every_part_then_each_in_its_place() {
    let configuration = lowered(
        "CONFIGURATION Plant
           VAR_GLOBAL shared : INT; END_VAR
           RESOURCE Cpu ON PLC
             VAR_GLOBAL local : BOOL; END_VAR
             TASK Fast (INTERVAL := T#10ms, PRIORITY := 1);
             TASK Slow (SINGLE := trigger, PRIORITY := 5);
             PROGRAM Main WITH Fast : Control (setpoint := 10, out => shared, Motor WITH Slow);
             PROGRAM RETAIN Aux : Logger;
           END_RESOURCE
           VAR_CONFIG
             Cpu.Main.motor.speed AT %QW0 : INT := 5;
             Cpu.Main.motor : Drive := (gain := 2);
           END_VAR
         END_CONFIGURATION",
    )
    .expect("the configuration lowers");
    assert_eq!(configuration.name, Id::from("Plant"));
    assert_eq!(
        names(
            &configuration
                .global_var
                .iter()
                .filter_map(|v| v.identifier.symbolic_id().cloned())
                .collect::<Vec<_>>()
        ),
        ["shared"]
    );
    assert_eq!(configuration.resource_decl.len(), 1);
    let resource = &configuration.resource_decl[0];
    assert_eq!(resource.name, Id::from("Cpu"));
    assert_eq!(resource.resource, Id::from("PLC"));
    assert_eq!(resource.global_vars.len(), 1);
    assert_eq!(resource.global_vars[0].var_type, VariableType::Global);
    assert_eq!(
        names(
            &resource
                .tasks
                .iter()
                .map(|t| t.name.clone())
                .collect::<Vec<_>>()
        ),
        ["Fast", "Slow"]
    );
    assert_eq!(
        names(
            &resource
                .programs
                .iter()
                .map(|p| p.name.clone())
                .collect::<Vec<_>>()
        ),
        ["Main", "Aux"]
    );
    assert_eq!(configuration.located_var_inits.len(), 1);
    assert_eq!(configuration.fb_inits.len(), 1);
}

#[test]
fn lower_configuration_when_the_initialisations_are_of_both_forms_then_each_goes_to_its_list_in_order(
) {
    let configuration = lowered(
        "CONFIGURATION c RESOURCE r ON t PROGRAM p : q; END_RESOURCE
         VAR_CONFIG
           r.p.a : Fb := (x := 1);
           r.p.b AT %MW1 : INT;
           r.p.c : Fb := (y := 2);
           r.p.d : INT := 3;
         END_VAR
         END_CONFIGURATION",
    )
    .expect("the configuration lowers");
    let fb: Vec<String> = configuration
        .fb_inits
        .iter()
        .map(|init| init.fb_path[0].original().to_string())
        .collect();
    let located: Vec<String> = configuration
        .located_var_inits
        .iter()
        .map(|init| init.fb_path[0].original().to_string())
        .collect();
    assert_eq!(fb, ["a", "c"]);
    assert_eq!(located, ["b", "d"]);
}

#[test]
fn lower_configuration_when_no_global_and_no_instance_then_both_lists_are_empty() {
    let configuration = configuration("PROGRAM p : q;");
    assert!(configuration.global_var.is_empty());
    assert!(configuration.fb_inits.is_empty());
    assert!(configuration.located_var_inits.is_empty());
    assert!(configuration.resource_decl[0].global_vars.is_empty());
    assert!(configuration.resource_decl[0].tasks.is_empty());
}

#[test]
fn lower_resource_when_named_then_the_name_and_the_type_are_positioned_at_the_words() {
    let configuration = lowered(
        "CONFIGURATION c RESOURCE res ON PLC PROGRAM p : q; END_RESOURCE END_CONFIGURATION",
    )
    .expect("the configuration lowers");
    let resource = &configuration.resource_decl[0];
    assert_eq!((resource.name.span.start, resource.name.span.end), (25, 28));
    assert_eq!(
        (resource.resource.span.start, resource.resource.span.end),
        (32, 35)
    );
    assert_eq!(resource.resource.span.file_id, file());
}

// ---- Tasks.

#[test]
fn lower_task_when_priority_only_then_no_interval_and_no_single() {
    let task = task("PRIORITY := 5");
    assert_eq!(task.name, Id::from("k"));
    assert_eq!(task.priority, 5);
    assert!(task.interval.is_none());
    assert!(task.single.is_none());
}

#[test]
fn lower_task_when_every_property_then_each_is_read_by_its_row() {
    let task = task("SINGLE := 1, INTERVAL := T#100ms, PRIORITY := 2");
    assert_eq!(task.priority, 2);
    assert!(task.interval.is_some());
    assert!(matches!(task.single, Some(DataSourceKind::Constant(_))));
}

#[test]
fn task_properties_when_the_table_then_one_row_for_each_word_the_grammar_reads() {
    let words: Vec<&str> = TASK_PROPERTIES.iter().map(|(word, _)| *word).collect();
    assert_eq!(words, ["SINGLE", "INTERVAL", "PRIORITY"]);
}

#[test]
fn lower_task_when_interval_then_the_duration_as_written() {
    let task = task("INTERVAL := T#1m30s, PRIORITY := 0");
    let interval = task.interval.expect("an interval");
    assert_eq!(interval.interval, std::time::Duration::from_secs(90));
}

#[test]
fn lower_task_when_single_is_a_global_variable_then_the_reference_as_the_grammar_reads_it() {
    let reference = |written: &str| {
        match task(&format!("SINGLE := {written}, PRIORITY := 1")).single {
            Some(DataSourceKind::GlobalVarReference(reference)) => Some(reference),
            _ => None,
        }
        .expect("a global variable")
    };
    let alone = reference("trigger");
    assert_eq!(alone.resource_name, None);
    assert_eq!(alone.global_var_name, Id::from("trigger"));
    assert_eq!(alone.structure_element_name, None);
    // Two names are a resource and a variable, and three add a member.
    let in_resource = reference("res.flag");
    assert_eq!(in_resource.resource_name, Some(Id::from("res")));
    assert_eq!(in_resource.global_var_name, Id::from("flag"));
    assert_eq!(in_resource.structure_element_name, None);
    let member = reference("res.rec.flag");
    assert_eq!(member.resource_name, Some(Id::from("res")));
    assert_eq!(member.global_var_name, Id::from("rec"));
    assert_eq!(member.structure_element_name, Some(Id::from("flag")));
}

#[test]
fn lower_task_when_single_is_a_signed_constant_then_a_constant_with_its_sign() {
    for written in ["-1", "TRUE", "T#1s", "'x'"] {
        let task = task(&format!("SINGLE := {written}, PRIORITY := 1"));
        assert!(
            matches!(task.single, Some(DataSourceKind::Constant(_))),
            "{written}"
        );
    }
    let negative = task("SINGLE := -1, PRIORITY := 1");
    let literal = match negative.single {
        Some(DataSourceKind::Constant(ConstantKind::IntegerLiteral(literal))) => Some(literal),
        _ => None,
    }
    .expect("an integer");
    assert!(literal.value.is_neg);
    assert!(literal.data_type.is_none());
}

#[test]
fn lower_task_when_single_is_no_constant_and_no_variable_then_syntax_error() {
    for written in ["a + b", "f(x)", "[1, 2]", "(a := 1)", "a.b.c.d", "a[1]"] {
        assert_eq!(
            error_code(&format!(
                "TASK k (SINGLE := {written}, PRIORITY := 1); PROGRAM p WITH k : q;"
            )),
            Problem::SyntaxError.code(),
            "{written}"
        );
    }
}

#[test]
fn lower_task_when_priority_is_beyond_32_bits_then_syntax_error_and_at_the_limit_then_kept() {
    assert_eq!(
        error_code("TASK k (PRIORITY := 4294967296); PROGRAM p WITH k : q;"),
        Problem::SyntaxError.code()
    );
    assert_eq!(task("PRIORITY := 4294967295").priority, u32::MAX);
}

#[test]
fn lower_task_when_a_part_is_missing_from_the_tree_then_internal_error() {
    let green = rowan::GreenNode::new(rowan::SyntaxKind(K::TaskDecl as u16), std::iter::empty());
    let error =
        lower_task(&LowerCx::new(file()), &SyntaxNode::new_root(green)).expect_err("no priority");
    assert_eq!(error.code, INTERNAL_ERROR);
}

// ---- Program configurations.

#[test]
fn lower_program_configuration_when_plain_then_name_type_and_nothing_else() {
    let program = program("PROGRAM inst : prog;");
    assert_eq!(program.name, Id::from("inst"));
    assert_eq!(program.type_name, Id::from("prog"));
    assert_eq!(program.task_name, None);
    assert_eq!(program.storage, None);
    assert!(program.fb_tasks.is_empty());
    assert!(program.sources.is_empty());
    assert!(program.sinks.is_empty());
}

#[test]
fn lower_program_configuration_when_storage_and_task_then_both_are_kept() {
    let retained = program("PROGRAM RETAIN inst WITH k : prog;");
    assert_eq!(retained.storage, Some(DeclarationQualifier::Retain));
    assert_eq!(retained.task_name, Some(Id::from("k")));
    let not_retained = program("PROGRAM NON_RETAIN inst : prog;");
    assert_eq!(not_retained.storage, Some(DeclarationQualifier::NonRetain));
    assert_eq!(not_retained.task_name, None);
}

#[test]
fn lower_program_configuration_when_connections_of_every_form_then_each_goes_to_its_list_in_order()
{
    let program = program(
        "PROGRAM inst : prog (a := 1, fb WITH k, b => out, c := -2.5, d => g.h, other WITH k);",
    );
    let named = |variable: &ironplc_dsl::textual::SymbolicVariableKind| match variable {
        ironplc_dsl::textual::SymbolicVariableKind::Named(named) => {
            Some(named.name.original().to_string())
        }
        _ => None,
    };
    let targets: Vec<Option<String>> = program.sources.iter().map(|s| named(&s.dst)).collect();
    assert_eq!(targets, [Some("a".to_string()), Some("c".to_string())]);
    let sources: Vec<Option<String>> = program.sinks.iter().map(|s| named(&s.src)).collect();
    assert_eq!(sources, [Some("b".to_string()), Some("d".to_string())]);
    let tasks: Vec<(String, String)> = program
        .fb_tasks
        .iter()
        .map(|binding| {
            (
                binding.fb_name.original().to_string(),
                binding.task_name.original().to_string(),
            )
        })
        .collect();
    assert_eq!(
        tasks,
        [
            ("fb".to_string(), "k".to_string()),
            ("other".to_string(), "k".to_string())
        ]
    );
}

#[test]
fn lower_program_configuration_when_the_variable_is_selected_then_the_whole_variable_is_the_target()
{
    let program = program("PROGRAM inst : prog (a.b[1] := 1, c => d);");
    assert!(matches!(
        &program.sources[0].dst,
        ironplc_dsl::textual::SymbolicVariableKind::Array(_)
    ));
}

#[test]
fn lower_program_configuration_when_each_form_of_source_then_its_row() {
    let source = |written: &str| {
        let program = program(&format!("PROGRAM inst : prog (a := {written});"));
        program.sources[0].src.clone()
    };
    for written in ["5", "-5", "TRUE", "T#1s", "'text'", "1.5"] {
        assert!(
            matches!(source(written), ProgramConnectionSourceKind::Constant(_)),
            "{written}"
        );
    }
    let qualified = match source("Color#Red") {
        ProgramConnectionSourceKind::EnumeratedValue(value) => Some(value),
        _ => None,
    }
    .expect("an enumeration value");
    assert_eq!(
        qualified
            .type_name
            .map(|name| name.name.original().to_string()),
        Some("Color".to_string())
    );
    assert_eq!(qualified.value.original(), "Red");
    // A name alone is read as an enumeration value, as the legacy grammar does:
    // only the declaration of the global variable can say it is not.
    let bare = match source("red") {
        ProgramConnectionSourceKind::EnumeratedValue(value) => Some(value),
        _ => None,
    }
    .expect("an enumeration value");
    assert!(bare.type_name.is_none());
    assert!(matches!(
        source("%IX0.1"),
        ProgramConnectionSourceKind::DirectVariable(_)
    ));
}

#[test]
fn lower_program_configuration_when_each_form_of_sink_then_its_row() {
    let sink = |written: &str| {
        let program = program(&format!("PROGRAM inst : prog (a => {written});"));
        program.sinks[0].dst.clone()
    };
    let reference = |written: &str| {
        match sink(written) {
            ProgramConnectionSinkKind::GlobalVarReference(reference) => Some(reference),
            ProgramConnectionSinkKind::DirectVariable(_) => None,
        }
        .expect("a global variable")
    };
    let alone = reference("g");
    assert_eq!(alone.global_var_name, Id::from("g"));
    assert_eq!(alone.resource_name, None);
    let in_resource = reference("res.g");
    assert_eq!(in_resource.resource_name, Some(Id::from("res")));
    assert_eq!(in_resource.global_var_name, Id::from("g"));
    let member = reference("res.g.m");
    assert_eq!(member.structure_element_name, Some(Id::from("m")));
    assert!(matches!(
        sink("%QX0.1"),
        ProgramConnectionSinkKind::DirectVariable(_)
    ));
}

#[test]
fn lower_program_configuration_when_a_connection_does_not_fit_its_place_then_syntax_error() {
    for connection in [
        "a => g.h.i.j",
        "a => b[1]",
        "a => THIS^.x",
        "%IX0.0 := 1",
        "%IX0.0 => b",
    ] {
        assert_eq!(
            error_code(&format!(
                "TASK k (PRIORITY := 1); PROGRAM p WITH k : q ({connection});"
            )),
            Problem::SyntaxError.code(),
            "{connection}"
        );
    }
}

#[test]
fn lower_program_configuration_when_a_part_is_missing_from_the_tree_then_internal_error() {
    let green = rowan::GreenNode::new(
        rowan::SyntaxKind(K::ProgramConfig as u16),
        std::iter::empty(),
    );
    let error = lower_program_configuration(&LowerCx::new(file()), &SyntaxNode::new_root(green))
        .expect_err("no name");
    assert_eq!(error.code, INTERNAL_ERROR);
    let green = rowan::GreenNode::new(
        rowan::SyntaxKind(K::ProgramConnection as u16),
        std::iter::empty(),
    );
    let node = SyntaxNode::new_root(green);
    let mut into = ProgramConfiguration {
        name: Id::from("p"),
        storage: None,
        task_name: None,
        type_name: Id::from("q"),
        fb_tasks: vec![],
        sources: vec![],
        sinks: vec![],
    };
    let error = connection(&LowerCx::new(file()), &node, &mut into).expect_err("no variable");
    assert_eq!(error.code, INTERNAL_ERROR);
}

#[test]
fn directions_when_the_table_then_a_row_for_each_operator_of_a_connection() {
    let operators: Vec<K> = DIRECTIONS.iter().map(|(operator, _)| *operator).collect();
    assert_eq!(operators, [K::Assignment, K::RightArrow]);
}

#[test]
fn lower_program_configuration_when_the_names_are_written_then_positioned_at_them() {
    let source = "CONFIGURATION c RESOURCE r ON t TASK k (PRIORITY := 1); PROGRAM  inst WITH k : prog; END_RESOURCE END_CONFIGURATION";
    let program = lowered(source)
        .expect("the configuration lowers")
        .resource_decl[0]
        .programs[0]
        .clone();
    let at = |word: &str| source.rfind(word).expect("a word");
    assert_eq!(
        (program.name.span.start, program.name.span.end),
        (at("inst"), at("inst") + 4)
    );
    assert_eq!(
        (program.type_name.span.start, program.type_name.span.end),
        (at("prog"), at("prog") + 4)
    );
    assert_eq!(program.type_name.span.file_id, file());
}

// ---- Sources.

#[test]
fn source_of_when_each_kind_of_node_then_its_class() {
    let class = |text: &str| {
        let parse = crate::parse_expression(text, &ParseOptions::all());
        let node = parse.root.first_child().expect("a node");
        source_of(&node)
    };
    assert_eq!(class("5"), Source::Value);
    assert_eq!(class("a + b"), Source::Value);
    assert_eq!(class("name"), Source::Name);
    assert_eq!(class("a.b"), Source::Path);
    assert_eq!(class("a.b.c"), Source::Path);
}
