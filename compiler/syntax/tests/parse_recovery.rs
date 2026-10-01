//! Recovery: malformed input still yields a complete tree, parsing resumes at
//! the next statement, errors stay inside the source, and pathological
//! nesting degrades to an error node instead of overflowing the stack.

mod common;

use ironplc_syntax::{
    parse_expression, parse_source_file, parse_statements, Parse, ParseOptions, SyntaxKind,
    SyntaxNode,
};

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

/// The text of the last assignment statement, outside any error node.
fn last_assignment(parsed: &Parse) -> Option<String> {
    nodes_of(&parsed.root, SyntaxKind::AssignStmt)
        .into_iter()
        .rfind(|node| {
            !node
                .ancestors()
                .any(|ancestor| ancestor.kind() == SyntaxKind::ErrorNode)
        })
        .map(|node| node.text().to_string())
}

#[test]
fn parse_statements_when_malformed_statement_then_error_reported_and_next_statement_parsed() {
    let malformed = [
        "x := ; y := 2;",
        ":= 1; y := 2;",
        "x := 1 y := 2;",
        "foo bar; y := 2;",
        "IF a THEN x := ; END_IF; y := 2;",
        "f(1,; y := 2;",
        "x := (1 + ; y := 2;",
        "1 := 2; y := 2;",
        "END_IF; y := 2;",
        "x := 1; ) ; y := 2;",
        "CASE x OF 1: z := ; 2: z := 3; END_CASE; y := 2;",
        "FOR i := TO 3 DO a := 1; END_FOR; y := 2;",
        "x := a b c; y := 2;",
        "x := [1]; y := 2;",
        "x := ?; y := 2;",
        "x := 'unterminated;\ny := 2;",
        "WHILE DO a := 1; END_WHILE; y := 2;",
        "a.; y := 2;",
        "a[; y := 2;",
    ];
    for source in malformed {
        let parsed = parse_statements(source, &ParseOptions::all());
        assert_well_formed(source, &parsed);
        assert!(!parsed.errors.is_empty(), "{source:?} should be an error");
        assert_eq!(
            last_assignment(&parsed).as_deref(),
            Some("y := 2;"),
            "{source:?} did not resume at the next statement"
        );
    }
}

#[test]
fn parse_statements_when_garbage_statement_then_error_node_holds_the_tokens() {
    let parsed = parse_statements("foo bar baz; y := 2;", &ParseOptions::default());
    let errors = nodes_of(&parsed.root, SyntaxKind::ErrorNode);
    assert!(!errors.is_empty());
    assert!(errors
        .iter()
        .any(|node| node.text().to_string().contains("bar baz;")));
}

#[test]
fn parse_statements_when_unterminated_constructs_then_tree_is_complete() {
    let sources = [
        "IF a THEN",
        "IF a THEN b := 1;",
        "CASE x OF",
        "CASE x OF 1:",
        "FOR i := 1 TO",
        "WHILE a DO",
        "REPEAT x := 1;",
        "__TRY x := 1;",
        "f(",
        "x := (",
        "x := a[",
        "x := T#",
        "x := D#2020-",
        "(* unterminated",
        "x := 'abc",
        "{pragma",
        "x := `esc",
    ];
    for options in [ParseOptions::default(), ParseOptions::all()] {
        for source in sources {
            let parsed = parse_statements(source, &options);
            assert_well_formed(source, &parsed);
            assert!(!parsed.errors.is_empty(), "{source:?}");
        }
    }
}

#[test]
fn parse_statements_when_unexpected_block_word_at_top_level_then_consumed_into_error_node() {
    for source in ["END_IF;", "ELSE", "UNTIL x", "END_CASE; y := 2;"] {
        let parsed = parse_statements(source, &ParseOptions::all());
        assert_well_formed(source, &parsed);
        assert!(!nodes_of(&parsed.root, SyntaxKind::ErrorNode).is_empty());
    }
}

#[test]
fn parse_statements_when_block_word_closes_the_wrong_block_then_enclosing_block_recovers() {
    let source = "WHILE a DO IF b THEN c := 1; END_WHILE; y := 2;";
    let parsed = parse_statements(source, &ParseOptions::default());
    assert_well_formed(source, &parsed);
    // The IF reports its missing END_IF; the WHILE still closes on END_WHILE.
    assert_eq!(nodes_of(&parsed.root, SyntaxKind::WhileStmt).len(), 1);
    assert_eq!(last_assignment(&parsed).as_deref(), Some("y := 2;"));
}

#[test]
fn parse_expression_when_input_left_over_then_error_node_keeps_it() {
    let parsed = parse_expression("a + b ) c", &ParseOptions::default());
    assert_well_formed("a + b ) c", &parsed);
    assert_eq!(parsed.errors.len(), 1);
    let errors = nodes_of(&parsed.root, SyntaxKind::ErrorNode);
    assert_eq!(errors.len(), 1);
    assert_eq!(errors[0].text().to_string(), ") c");
}

#[test]
fn parse_expression_when_empty_then_error_at_end_of_input() {
    let parsed = parse_expression("  ", &ParseOptions::default());
    assert_well_formed("  ", &parsed);
    assert_eq!(parsed.errors.len(), 1);
    assert_eq!(usize::from(parsed.errors[0].range.start()), 2);
}

/// Runs `check` on a thread with a small stack, so a recursion that the depth
/// guard fails to bound overflows here instead of passing on a roomy stack.
fn on_small_stack(check: impl FnOnce() + Send + 'static) {
    let handle = std::thread::Builder::new()
        .stack_size(1024 * 1024)
        .spawn(check);
    assert!(handle.is_ok_and(|handle| handle.join().is_ok()));
}

fn max_depth(node: &SyntaxNode) -> usize {
    node.descendants()
        .map(|descendant| descendant.ancestors().count())
        .max()
        .unwrap_or(0)
}

#[test]
fn parse_expression_when_400_nested_parentheses_then_depth_guard_degrades_to_error_node() {
    on_small_stack(|| {
        let source = format!("{}1{}", "(".repeat(400), ")".repeat(400));
        let parsed = parse_expression(&source, &ParseOptions::all());
        assert_well_formed(&source, &parsed);
        assert!(parsed
            .errors
            .iter()
            .any(|error| error.message == "nesting is too deep"));
        assert!(!nodes_of(&parsed.root, SyntaxKind::ErrorNode).is_empty());
        assert!(max_depth(&parsed.root) < 400, "tree not bounded");
    });
}

#[test]
fn parse_statements_when_400_nested_ifs_then_depth_guard_degrades_to_error_node() {
    on_small_stack(|| {
        let source = format!(
            "{}x := 1;{}",
            "IF a THEN ".repeat(400),
            " END_IF;".repeat(400)
        );
        let parsed = parse_statements(&source, &ParseOptions::all());
        assert_well_formed(&source, &parsed);
        assert!(parsed
            .errors
            .iter()
            .any(|error| error.message == "nesting is too deep"));
        assert!(max_depth(&parsed.root) < 400, "tree not bounded");
    });
}

#[test]
fn parse_statements_when_deep_nesting_of_every_construct_then_terminates_with_complete_tree() {
    on_small_stack(|| {
        let n = 300;
        let sources = [
            format!("x := {}1{};", "f(".repeat(n), ")".repeat(n)),
            format!("x := {}1{};", "a[".repeat(n), "]".repeat(n)),
            format!("x := {}a;", "NOT ".repeat(n)),
            format!("x := {}1;", "1 + (".repeat(n)),
            format!(
                "{}x := 1;{}",
                "CASE a OF 1: ".repeat(n),
                " END_CASE;".repeat(n)
            ),
            format!(
                "{}x := 1;{}",
                "WHILE a DO ".repeat(n),
                " END_WHILE;".repeat(n)
            ),
            format!("{}x := 1;{}", "__TRY ".repeat(n), " __ENDTRY;".repeat(n)),
            format!("x := {}a;", "-".repeat(n)),
        ];
        for source in sources {
            let parsed = parse_statements(&source, &ParseOptions::all());
            assert_well_formed(&source, &parsed);
        }
    });
}

#[test]
fn parse_expression_when_very_deep_nesting_then_linear_time_and_complete_tree() {
    on_small_stack(|| {
        let source = format!("{}1{}", "(".repeat(20_000), ")".repeat(20_000));
        let parsed = parse_expression(&source, &ParseOptions::all());
        assert_well_formed(&source, &parsed);
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
    "IF",
    "THEN",
    "ELSIF",
    "ELSE",
    "END_IF",
    "CASE",
    "OF",
    "END_CASE",
    "FOR",
    "TO",
    "BY",
    "DO",
    "END_FOR",
    "WHILE",
    "END_WHILE",
    "REPEAT",
    "UNTIL",
    "END_REPEAT",
    "EXIT",
    "CONTINUE",
    "RETURN",
    "JMP",
    "CALC",
    "__WAIT",
    "__TRY",
    "__CATCH",
    "__FINALLY",
    "__ENDTRY",
    "__THROW",
    "__BEGIN_IMPLEMENTATION",
    "x",
    "y",
    "lbl",
    "1",
    "2",
    "16#FF",
    "1.5",
    "'s'",
    "T#",
    "5s",
    "D#",
    "2020-01-01",
    "TOD#",
    "10:00",
    ":=",
    "S",
    "REF",
    "=",
    "<>",
    "<",
    ">",
    "+",
    "-",
    "*",
    "/",
    "**",
    "MOD",
    "NOT",
    "AND",
    "OR",
    "XOR",
    "(",
    ")",
    "[",
    "]",
    ",",
    ";",
    ":",
    ".",
    "..",
    "^",
    "#",
    "=>",
    "%IX0.1",
    "%X3",
    "THIS",
    "NULL",
    "(* c *)",
    "// c\n",
    " ",
    "\n",
    "?",
    "`e`",
    "{p}",
];

#[test]
fn parse_when_random_token_soup_then_every_entry_point_terminates_with_a_complete_tree() {
    let mut random = Xorshift(0x9E37_79B9_7F4A_7C15);
    for _ in 0..3000 {
        let length = 1 + usize::try_from(random.next() % 40).unwrap_or(0);
        let mut source = String::new();
        for _ in 0..length {
            let pick = usize::try_from(random.next()).unwrap_or(0) % FRAGMENTS.len();
            source.push_str(FRAGMENTS[pick]);
            if !random.next().is_multiple_of(3) {
                source.push(' ');
            }
        }
        for options in [ParseOptions::default(), ParseOptions::all()] {
            assert_well_formed(&source, &parse_statements(&source, &options));
            assert_well_formed(&source, &parse_expression(&source, &options));
            assert_well_formed(&source, &parse_source_file(&source, &options));
        }
    }
}

#[test]
fn parse_source_file_when_declarations_then_each_is_a_node_of_its_kind() {
    let source = "PROGRAM p\nVAR x : INT; END_VAR\nx := 1;\nEND_PROGRAM\n\nFUNCTION f : INT\nf := 1;\nEND_FUNCTION\nTYPE t : INT; END_TYPE\n";
    let parsed = parse_source_file(source, &ParseOptions::default());
    assert_well_formed(source, &parsed);
    assert_eq!(parsed.errors, vec![]);
    let programs = nodes_of(&parsed.root, SyntaxKind::ProgramDecl);
    assert_eq!(programs.len(), 1);
    assert!(programs[0].text().to_string().starts_with("PROGRAM p"));
    assert!(programs[0].text().to_string().ends_with("END_PROGRAM"));
    assert_eq!(nodes_of(&parsed.root, SyntaxKind::FunctionDecl).len(), 1);
    assert_eq!(nodes_of(&parsed.root, SyntaxKind::TypeBlock).len(), 1);
}

#[test]
fn parse_source_file_when_namespaces_nest_then_outer_node_spans_them() {
    let source = "NAMESPACE a NAMESPACE b END_NAMESPACE END_NAMESPACE PROGRAM p END_PROGRAM";
    let parsed = parse_source_file(source, &ParseOptions::all());
    assert_well_formed(source, &parsed);
    assert_eq!(parsed.errors, vec![]);
    let namespaces = nodes_of(&parsed.root, SyntaxKind::NamespaceDecl);
    assert_eq!(namespaces.len(), 2);
    assert!(namespaces[0].text().to_string().ends_with("END_NAMESPACE"));
    assert_eq!(nodes_of(&parsed.root, SyntaxKind::ProgramDecl).len(), 1);
}

#[test]
fn parse_source_file_when_stray_tokens_or_unterminated_declaration_then_errors() {
    for source in [
        "x := 1; PROGRAM p END_PROGRAM",
        "PROGRAM p VAR",
        "END_PROGRAM",
        "PROGRAM",
    ] {
        let parsed = parse_source_file(source, &ParseOptions::default());
        assert_well_formed(source, &parsed);
        assert!(!parsed.errors.is_empty(), "{source:?}");
    }
}

#[test]
fn parse_source_file_when_lexical_and_gate_errors_then_all_reported_in_source_order() {
    let source = "// c\nPROGRAM p x := ?; END_PROGRAM";
    let parsed = parse_source_file(source, &ParseOptions::default());
    assert_well_formed(source, &parsed);
    let starts: Vec<usize> = parsed
        .errors
        .iter()
        .map(|error| usize::from(error.range.start()))
        .collect();
    let mut sorted = starts.clone();
    sorted.sort_unstable();
    assert_eq!(starts, sorted);
    // The C-style comment (gate) and the stray `?` (lexer).
    assert_eq!(starts.len(), 2);
}

/// A mid-sized body that exercises every statement and expression family; it
/// is valid under every flag on.
const STATEMENT_SAMPLE: &str = "(* heater control *)\r\nIF temp > 80.5 AND NOT fault THEN\r\n\tvalve := FALSE; // close\r\nELSIF temp < T#5s + 1 THEN\r\n\tvalve S= TRUE;\r\nELSE\r\n\tf(a := 1, b => out, 2);\r\nEND_IF;\r\nCASE mode OF\r\n\t1, 3..5: x := arr[i + 1].field^;\r\n\tColor#Red: y := 16#FF + INT#-5;\r\nELSE\r\n\tz := TOD#10:00:00.250;\r\nEND_CASE;\r\nFOR i := 0 TO 10 BY 2 DO\r\n\tIF i = 4 THEN CONTINUE; END_IF;\r\nEND_FOR;\r\n__TRY\r\n\tinst.Run(1);\r\n__CATCH (e)\r\n\tp REF= q;\r\n__ENDTRY;\r\nlbl: JMP (done) lbl;\r\nx := 'str$N' + \"w\" + D#2020-01-01 + a.%X3 + %IX0.1;\r\n";

#[test]
fn parse_statements_when_sample_then_valid_with_every_flag_on() {
    let parsed = parse_statements(STATEMENT_SAMPLE, &ParseOptions::all());
    assert_eq!(parsed.errors, vec![]);
    assert_well_formed(STATEMENT_SAMPLE, &parsed);
}

#[test]
fn parse_statements_when_every_prefix_of_sample_then_terminates_with_complete_tree() {
    let mut swept = 0;
    for end in 0..=STATEMENT_SAMPLE.len() {
        if !STATEMENT_SAMPLE.is_char_boundary(end) {
            continue;
        }
        let prefix = &STATEMENT_SAMPLE[..end];
        for options in [ParseOptions::default(), ParseOptions::all()] {
            assert_well_formed(prefix, &parse_statements(prefix, &options));
            assert_well_formed(prefix, &parse_expression(prefix, &options));
            assert_well_formed(prefix, &parse_source_file(prefix, &options));
        }
        swept += 1;
    }
    assert_eq!(swept, STATEMENT_SAMPLE.len() + 1);
}

#[test]
fn parse_statements_when_every_suffix_of_sample_then_terminates_with_complete_tree() {
    for start in 0..=STATEMENT_SAMPLE.len() {
        if STATEMENT_SAMPLE.is_char_boundary(start) {
            let suffix = &STATEMENT_SAMPLE[start..];
            assert_well_formed(suffix, &parse_statements(suffix, &ParseOptions::all()));
        }
    }
}

#[test]
fn parse_statements_when_gated_syntax_disabled_then_error_and_lossless_tree() {
    let gated = [
        "CONTINUE;",
        "JMP lbl;",
        "lbl: x := 1;",
        "__WAIT;",
        "__THROW;",
        "__TRY x := 1; __ENDTRY;",
        "x := a AND_THEN b;",
        "x := a OR_ELSE b;",
        "__BEGIN_IMPLEMENTATION x := 1;",
        "x := BIT#1;",
        "x := LTIME#5s;",
        "x := a.%X1;",
        "{attribute 'x'} x := 1;",
        "x := 1; // c",
        "x := 1; /* c */",
        "x := 1; (* a (* b *) c *)",
        "`a b` := 1;",
        "my__var := 1;",
    ];
    for source in gated {
        let strict = parse_statements(source, &ParseOptions::default());
        assert_well_formed(source, &strict);
        assert!(
            !strict.errors.is_empty(),
            "{source:?} accepted when disabled"
        );
        let enabled = parse_statements(source, &ParseOptions::all());
        assert_well_formed(source, &enabled);
        assert_eq!(enabled.errors, vec![], "{source:?} rejected when enabled");
    }
}
