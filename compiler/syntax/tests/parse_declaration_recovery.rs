//! Recovery at declaration level: a malformed declaration yields error nodes
//! and parsing resumes at the next declaration, a block left open at the end of
//! the input ends with errors, and pathological nesting degrades to an error
//! node instead of overflowing the stack. Every input keeps the tree's text
//! equal to the source and every error inside it.

mod common;

use ironplc_syntax::{parse_source_file, Parse, ParseOptions, SyntaxKind, SyntaxNode};

fn assert_well_formed(source: &str, parsed: &Parse) {
    assert_eq!(parsed.root.text().to_string(), source);
    for error in &parsed.errors {
        assert!(
            usize::from(error.range.end()) <= source.len(),
            "error outside the source in {source:?}: {error}"
        );
        assert!(error.range.start() <= error.range.end());
    }
}

fn nodes_of(root: &SyntaxNode, kind: SyntaxKind) -> Vec<SyntaxNode> {
    root.descendants()
        .filter(|node| node.kind() == kind)
        .collect()
}

fn parse(source: &str) -> Parse {
    let parsed = parse_source_file(source, &ParseOptions::all());
    assert_well_formed(source, &parsed);
    parsed
}

/// True when no error range starts inside `node`.
fn is_error_free(parsed: &Parse, node: &SyntaxNode) -> bool {
    let range = node.text_range();
    !parsed
        .errors
        .iter()
        .any(|error| range.contains_range(error.range) && !error.range.is_empty())
        && !node
            .descendants()
            .any(|descendant| descendant.kind() == SyntaxKind::ErrorNode)
}

/// The declaration nodes of `kind` whose name is `name`.
fn named(parsed: &Parse, kind: SyntaxKind, name: &str) -> Option<SyntaxNode> {
    nodes_of(&parsed.root, kind).into_iter().find(|node| {
        node.children()
            .find(|child| child.kind() == SyntaxKind::Name)
            .is_some_and(|child| child.text() == name)
    })
}

#[test]
fn parse_source_file_when_var_block_is_not_closed_then_next_program_parses_cleanly() {
    let parsed = parse(include_str!("fixtures/malformed/unterminated_var_block.st"));
    assert!(!parsed.errors.is_empty());
    assert_eq!(nodes_of(&parsed.root, SyntaxKind::ProgramDecl).len(), 2);
    let next = named(&parsed, SyntaxKind::ProgramDecl, "Next");
    assert!(next.is_some_and(|node| is_error_free(&parsed, &node)));
}

#[test]
fn parse_source_file_when_program_is_not_closed_then_next_program_parses_cleanly() {
    let parsed = parse(include_str!("fixtures/malformed/missing_end_program.st"));
    assert!(!parsed.errors.is_empty());
    assert!(parsed
        .errors
        .iter()
        .all(|error| error.message.contains("END_PROGRAM")));
    let first = named(&parsed, SyntaxKind::ProgramDecl, "First");
    let second = named(&parsed, SyntaxKind::ProgramDecl, "Second");
    // The first program ends where the second begins, and neither swallows
    // the other.
    assert!(first.is_some_and(|node| !node.text().to_string().contains("Second")));
    assert!(second.is_some_and(|node| is_error_free(&parsed, &node)));
}

#[test]
fn parse_source_file_when_declarations_are_malformed_then_the_neighbours_still_parse() {
    let parsed = parse(include_str!("fixtures/malformed/broken_declarations.st"));
    let names: Vec<String> = nodes_of(&parsed.root, SyntaxKind::VarDecl)
        .iter()
        .filter_map(|decl| decl.first_child())
        .map(|name| name.text().to_string())
        .collect();
    assert!(names.contains(&"ok".to_string()));
    assert!(names.contains(&"ok2".to_string()), "{names:?}");
    assert_eq!(nodes_of(&parsed.root, SyntaxKind::AssignStmt).len(), 1);
    assert!(!nodes_of(&parsed.root, SyntaxKind::ErrorNode).is_empty());
}

#[test]
fn parse_source_file_when_type_declaration_is_malformed_then_later_types_and_programs_parse() {
    let parsed = parse(include_str!("fixtures/malformed/bad_type_declarations.st"));
    assert!(!parsed.errors.is_empty());
    let types = nodes_of(&parsed.root, SyntaxKind::TypeDecl);
    let names: Vec<String> = types
        .iter()
        .filter_map(|decl| decl.first_child())
        .map(|name| name.text().to_string())
        .collect();
    for expected in ["Good", "Broken", "Colors", "Point"] {
        assert!(names.contains(&expected.to_string()), "{names:?}");
    }
    let after = named(&parsed, SyntaxKind::ProgramDecl, "After");
    assert!(after.is_some_and(|node| is_error_free(&parsed, &node)));
}

#[test]
fn parse_source_file_when_closers_do_not_match_then_each_declaration_ends_and_the_next_begins() {
    let parsed = parse(include_str!("fixtures/malformed/mismatched_closers.st"));
    assert!(!parsed.errors.is_empty());
    assert_eq!(
        nodes_of(&parsed.root, SyntaxKind::FunctionBlockDecl).len(),
        1
    );
    assert!(named(&parsed, SyntaxKind::FunctionDecl, "f").is_some());
}

#[test]
fn parse_source_file_when_method_is_not_closed_then_function_block_and_next_program_parse() {
    let parsed = parse(include_str!("fixtures/malformed/unterminated_method.st"));
    assert!(!parsed.errors.is_empty());
    assert_eq!(nodes_of(&parsed.root, SyntaxKind::MethodDecl).len(), 1);
    assert!(named(&parsed, SyntaxKind::FunctionBlockDecl, "Fb").is_some());
    let main = named(&parsed, SyntaxKind::ProgramDecl, "Main");
    assert!(main.is_some_and(|node| is_error_free(&parsed, &node)));
}

#[test]
fn parse_source_file_when_stray_end_var_and_unclosed_if_then_errors_without_losing_statements() {
    let parsed = parse(include_str!("fixtures/malformed/stray_end_var.st"));
    assert!(!parsed.errors.is_empty());
    assert_eq!(nodes_of(&parsed.root, SyntaxKind::ProgramDecl).len(), 1);
    assert_eq!(nodes_of(&parsed.root, SyntaxKind::IfStmt).len(), 1);
    let stray = nodes_of(&parsed.root, SyntaxKind::ErrorNode);
    assert!(stray
        .iter()
        .any(|node| node.text().to_string().contains("END_VAR")));
}

#[test]
fn parse_source_file_when_configuration_is_truncated_then_nodes_close_with_errors() {
    let parsed = parse(include_str!(
        "fixtures/malformed/truncated_configuration.st"
    ));
    assert_eq!(
        nodes_of(&parsed.root, SyntaxKind::ConfigurationDecl).len(),
        1
    );
    assert_eq!(nodes_of(&parsed.root, SyntaxKind::ResourceDecl).len(), 1);
    assert_eq!(nodes_of(&parsed.root, SyntaxKind::TaskDecl).len(), 1);
    assert_eq!(nodes_of(&parsed.root, SyntaxKind::ProgramConfig).len(), 1);
    let messages: Vec<&str> = parsed
        .errors
        .iter()
        .map(|error| error.message.as_str())
        .collect();
    assert!(
        messages.contains(&"expected `END_RESOURCE`"),
        "{messages:?}"
    );
    assert!(
        messages.contains(&"expected `END_CONFIGURATION`"),
        "{messages:?}"
    );
}

#[test]
fn parse_source_file_when_parts_are_missing_then_each_declaration_reports_and_continues() {
    let parsed = parse(include_str!(
        "fixtures/malformed/empty_and_missing_parts.st"
    ));
    assert!(parsed.errors.len() >= 5, "{:?}", parsed.errors);
    assert_eq!(nodes_of(&parsed.root, SyntaxKind::ProgramDecl).len(), 1);
    assert_eq!(nodes_of(&parsed.root, SyntaxKind::FunctionDecl).len(), 2);
    assert_eq!(nodes_of(&parsed.root, SyntaxKind::TypeBlock).len(), 1);
    assert_eq!(nodes_of(&parsed.root, SyntaxKind::VarBlock).len(), 1);
}

#[test]
fn parse_source_file_when_other_malformed_fixtures_then_complete_tree_and_errors() {
    for source in [
        include_str!("fixtures/malformed/stray_between_programs.st"),
        include_str!("fixtures/malformed/dangling_operator.st"),
        include_str!("fixtures/malformed/unknown_character.st"),
        include_str!("fixtures/malformed/unterminated_comment.st"),
    ] {
        let parsed = parse(source);
        assert!(!parsed.errors.is_empty(), "{source:?}");
    }
    let between = parse(include_str!("fixtures/malformed/stray_between_programs.st"));
    assert_eq!(nodes_of(&between.root, SyntaxKind::ProgramDecl).len(), 2);
}

#[test]
fn parse_source_file_when_input_ends_inside_a_declaration_then_every_block_closes_with_an_error() {
    for source in [
        "PROGRAM p",
        "PROGRAM p VAR",
        "PROGRAM p VAR x",
        "PROGRAM p VAR x :",
        "PROGRAM p VAR x : ARRAY[1..",
        "PROGRAM p VAR x : INT :=",
        "PROGRAM p VAR x : my_type := (a :=",
        "FUNCTION f :",
        "FUNCTION_BLOCK fb METHOD m",
        "FUNCTION_BLOCK fb PROPERTY p : INT GET",
        "INTERFACE i EXTENDS",
        "NAMESPACE n PROGRAM p",
        "TYPE t : STRUCT a :",
        "TYPE t : (A,",
        "CONFIGURATION c RESOURCE r ON t TASK k (PRIORITY :=",
        "CONFIGURATION c RESOURCE r ON t PROGRAM p : q (a :=",
        "PROGRAM p INITIAL_STEP s : a(",
        "PROGRAM p INITIAL_STEP s : END_STEP TRANSITION FROM s TO",
        "VAR_GLOBAL g :",
    ] {
        let parsed = parse(source);
        assert!(!parsed.errors.is_empty(), "{source:?}");
    }
}

/// Runs `check` on a thread with a small stack, so a recursion that the depth
/// guard fails to bound overflows here instead of passing on a roomy stack.
fn on_small_stack(check: impl FnOnce() + Send + 'static) {
    let handle = std::thread::Builder::new()
        .stack_size(1024 * 1024)
        .spawn(check);
    assert!(handle.is_ok_and(|handle| handle.join().is_ok()));
}

fn assert_depth_guard(source: &str) {
    let parsed = parse(source);
    assert!(
        parsed
            .errors
            .iter()
            .any(|error| error.message == "nesting is too deep"),
        "no depth error for {} bytes",
        source.len()
    );
}

#[test]
fn parse_source_file_when_deeply_nested_declarations_then_depth_guard_degrades_to_error_node() {
    on_small_stack(|| {
        let namespaces = format!(
            "{}{}",
            "NAMESPACE n ".repeat(400),
            "END_NAMESPACE ".repeat(400)
        );
        assert_depth_guard(&namespaces);

        let references = format!(
            "PROGRAM p VAR x : {}INT; END_VAR END_PROGRAM",
            "REF_TO ".repeat(400)
        );
        assert_depth_guard(&references);

        let arrays = format!(
            "PROGRAM p VAR x : {}INT; END_VAR END_PROGRAM",
            "ARRAY[1..2] OF ".repeat(400)
        );
        assert_depth_guard(&arrays);

        let values = format!(
            "PROGRAM p VAR x : t := {}1{}; END_VAR END_PROGRAM",
            "(a := ".repeat(400),
            ")".repeat(400)
        );
        assert_depth_guard(&values);

        let charts = format!(
            "PROGRAM p {}x := 1;{}END_PROGRAM",
            "INITIAL_STEP s : END_STEP ACTION a : ".repeat(400),
            "END_ACTION ".repeat(400)
        );
        assert_depth_guard(&charts);
    });
}

#[test]
fn parse_source_file_when_very_deep_declaration_nesting_then_linear_time_and_complete_tree() {
    on_small_stack(|| {
        let source = format!(
            "{}{}",
            "NAMESPACE n ".repeat(20_000),
            "END_NAMESPACE ".repeat(20_000)
        );
        let parsed = parse_source_file(&source, &ParseOptions::all());
        assert_well_formed(&source, &parsed);
        let values = format!(
            "PROGRAM p VAR x : t := {}1{}; END_VAR END_PROGRAM",
            "(a := ".repeat(20_000),
            ")".repeat(20_000)
        );
        let parsed = parse_source_file(&values, &ParseOptions::all());
        assert_well_formed(&values, &parsed);
    });
}

/// A small deterministic generator so the sweep is reproducible.
struct Xorshift(u64);

impl Xorshift {
    fn next(&mut self) -> u64 {
        self.0 ^= self.0 << 13;
        self.0 ^= self.0 >> 7;
        self.0 ^= self.0 << 17;
        self.0
    }
}

const FRAGMENTS: &[&str] = &[
    "PROGRAM",
    "END_PROGRAM",
    "FUNCTION",
    "END_FUNCTION",
    "FUNCTION_BLOCK",
    "END_FUNCTION_BLOCK",
    "METHOD",
    "END_METHOD",
    "PROPERTY",
    "END_PROPERTY",
    "GET",
    "SET",
    "END_GET",
    "END_SET",
    "INTERFACE",
    "END_INTERFACE",
    "EXTENDS",
    "IMPLEMENTS",
    "ABSTRACT",
    "PUBLIC",
    "NAMESPACE",
    "END_NAMESPACE",
    "TYPE",
    "END_TYPE",
    "STRUCT",
    "END_STRUCT",
    "UNION",
    "END_UNION",
    "VAR",
    "VAR_INPUT",
    "VAR_OUTPUT",
    "VAR_IN_OUT",
    "VAR_TEMP",
    "VAR_EXTERNAL",
    "VAR_GLOBAL",
    "VAR_ACCESS",
    "VAR_CONFIG",
    "VAR_STAT",
    "VAR_INST",
    "VAR_GENERIC",
    "END_VAR",
    "CONSTANT",
    "RETAIN",
    "NON_RETAIN",
    "PERSISTENT",
    "AT",
    "ARRAY",
    "OF",
    "STRING",
    "WSTRING",
    "INT",
    "BOOL",
    "REAL",
    "REF_TO",
    "REFERENCE",
    "POINTER",
    "TO",
    "PARAMS",
    "R_EDGE",
    "F_EDGE",
    "CONFIGURATION",
    "END_CONFIGURATION",
    "RESOURCE",
    "END_RESOURCE",
    "ON",
    "TASK",
    "WITH",
    "READ_ONLY",
    "INITIAL_STEP",
    "STEP",
    "END_STEP",
    "TRANSITION",
    "FROM",
    "END_TRANSITION",
    "ACTION",
    "END_ACTION",
    "IF",
    "THEN",
    "END_IF",
    "CASE",
    "END_CASE",
    "RETURN",
    "x",
    "fb",
    "lbl",
    "PRIORITY",
    "INTERVAL",
    "N",
    "SD",
    "1",
    "16#FF",
    "'s'",
    "'$z'",
    "T#1s",
    ":=",
    "=>",
    "=",
    ":",
    ";",
    ",",
    ".",
    "..",
    "(",
    ")",
    "[",
    "]",
    "*",
    "#",
    "%IX0.1",
    "%I*",
    "(* c *)",
    "// c\n",
    "{p}",
    " ",
    "\n",
    "?",
];

#[test]
fn parse_source_file_when_random_declaration_token_soup_then_terminates_with_a_complete_tree() {
    let mut random = Xorshift(0x2545_F491_4F6C_DD1D);
    for _ in 0..4000 {
        let length = 1 + usize::try_from(random.next() % 60).unwrap_or(0);
        let mut source = String::new();
        for _ in 0..length {
            let pick = usize::try_from(random.next()).unwrap_or(0) % FRAGMENTS.len();
            source.push_str(FRAGMENTS[pick]);
            if !random.next().is_multiple_of(4) {
                source.push(' ');
            }
        }
        for options in [ParseOptions::default(), ParseOptions::all()] {
            assert_well_formed(&source, &parse_source_file(&source, &options));
        }
    }
}
