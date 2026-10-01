//! Losslessness, tree shape, and recovery tests for the spike CST.
//!
//! These are the S0 adapter checks for design section 3.1: every byte is
//! stored, trivia is retained, red navigation works, and recovery keeps
//! making progress on malformed input.

use ironplc_s0_spike::cst::{build_cst, nodes_of_kind, reconstruct, sexpr, tree_stats};
use ironplc_s0_spike::lexer::{check_coverage, lex};
use ironplc_s0_spike::syntax::SyntaxKind;

const POU: &str = "PROGRAM Mixer\r\nVAR_INPUT\r\n\tspeed : REAL; (* target speed *)\r\nEND_VAR\r\nVAR\r\n\tat_speed : BOOL := FALSE;\r\nEND_VAR\r\n{attribute 'strict'}\r\nat_speed := (speed * 2.0 + 1.0) >= 3.5;\r\nIF at_speed THEN\r\n\tspeed := speed + 1.0;\r\nEND_IF\r\nEND_PROGRAM\r\n";

#[test]
fn cst_when_pou_with_crlf_tabs_comment_pragma_then_reconstructs_exactly() {
    let parsed = build_cst(POU);
    assert_eq!(parsed.errors, vec![], "expected a clean parse");
    assert_eq!(reconstruct(&parsed.root), POU);
}

#[test]
fn cst_when_crlf_then_newline_tokens_keep_crlf_spelling() {
    let parsed = build_cst(POU);
    let newlines: Vec<String> = parsed
        .root
        .descendants_with_tokens()
        .filter_map(|element| element.into_token())
        .filter(|token| token.kind() == SyntaxKind::Newline)
        .map(|token| token.text().to_string())
        .collect();
    assert!(!newlines.is_empty());
    assert!(newlines.iter().all(|text| text == "\r\n"));
}

#[test]
fn cst_when_pou_then_trivia_is_retained_in_tree() {
    let parsed = build_cst(POU);
    let stats = tree_stats(&parsed.root);
    assert!(stats.trivia_tokens > 0);
    assert_eq!(stats.error_nodes, 0);
    let pragmas = parsed
        .root
        .descendants_with_tokens()
        .filter_map(|element| element.into_token())
        .filter(|token| token.kind() == SyntaxKind::Pragma)
        .count();
    assert_eq!(pragmas, 1, "pragma must be stored as trivia");
}

#[test]
fn cst_when_pou_then_named_nodes_exist() {
    let parsed = build_cst(POU);
    assert_eq!(
        nodes_of_kind(&parsed.root, SyntaxKind::ProgramDecl).len(),
        1
    );
    assert_eq!(nodes_of_kind(&parsed.root, SyntaxKind::VarBlock).len(), 2);
    assert_eq!(nodes_of_kind(&parsed.root, SyntaxKind::VarDecl).len(), 2);
    assert_eq!(nodes_of_kind(&parsed.root, SyntaxKind::IfStmt).len(), 1);
}

#[test]
fn cst_when_binary_expression_then_precedence_shapes_the_tree() {
    let source = "PROGRAM P\nx := 1 + 2 * 3;\nEND_PROGRAM";
    let parsed = build_cst(source);
    assert_eq!(parsed.errors, vec![]);
    assert!(
        sexpr(&parsed.root).contains(
            "(BinaryExpr (LiteralExpr 1) + (BinaryExpr (LiteralExpr 2) * (LiteralExpr 3)))"
        ),
        "unexpected tree: {}",
        sexpr(&parsed.root)
    );
    assert_eq!(reconstruct(&parsed.root), source);
}

#[test]
fn cst_when_unicode_comment_and_string_then_bytes_survive() {
    let source = "PROGRAM P\n(* Wärmebild *)\ns := 'Wärmebild';\nEND_PROGRAM";
    let parsed = build_cst(source);
    assert_eq!(parsed.errors, vec![]);
    assert_eq!(reconstruct(&parsed.root), source);
}

#[test]
fn cst_when_unmatched_byte_then_error_token_and_exact_reconstruction() {
    let source = "PROGRAM P\nx := 1 ? 2;\nEND_PROGRAM";
    let parsed = build_cst(source);
    assert_eq!(reconstruct(&parsed.root), source);
    let tokens = parsed
        .root
        .descendants_with_tokens()
        .filter_map(|element| element.into_token())
        .filter(|token| token.kind() == SyntaxKind::ErrorToken)
        .count();
    assert_eq!(tokens, 1);
}

#[test]
fn cst_when_missing_expression_then_recovers_with_diagnostic() {
    let source = "PROGRAM P\nx := 1 + ;\ny := 2;\nEND_PROGRAM";
    let parsed = build_cst(source);
    assert_eq!(reconstruct(&parsed.root), source);
    assert!(!parsed.errors.is_empty());
    assert!(
        !nodes_of_kind(&parsed.root, SyntaxKind::ErrorNode).is_empty(),
        "expected an ErrorNode"
    );
    let assignments = nodes_of_kind(&parsed.root, SyntaxKind::AssignStmt).len();
    assert_eq!(assignments, 2, "recovery must resume at the next statement");
}

#[test]
fn cst_when_unterminated_comment_then_diagnostic_and_exact_reconstruction() {
    let source = "PROGRAM P\n(* never closed\nEND_PROGRAM";
    let parsed = build_cst(source);
    assert_eq!(reconstruct(&parsed.root), source);
    assert!(parsed
        .errors
        .iter()
        .any(|error| error.message.contains("unterminated")));
}

#[test]
fn cst_when_junk_between_pous_then_recovers_and_covers_every_byte() {
    let source = "PROGRAM A\nEND_PROGRAM\n???\nPROGRAM B\nEND_PROGRAM\n";
    let parsed = build_cst(source);
    assert_eq!(reconstruct(&parsed.root), source);
    assert_eq!(
        nodes_of_kind(&parsed.root, SyntaxKind::ProgramDecl).len(),
        2
    );
    assert!(!parsed.errors.is_empty());
}

#[test]
fn cst_when_deep_parentheses_then_terminates_within_depth_limit() {
    let mut source = String::from("PROGRAM P\nx := ");
    source.push_str(&"(".repeat(400));
    source.push('1');
    source.push_str(&")".repeat(400));
    source.push_str(";\nEND_PROGRAM");
    let parsed = build_cst(&source);
    assert_eq!(reconstruct(&parsed.root), source);
    assert!(parsed
        .errors
        .iter()
        .any(|error| error.message.contains("expression nesting limit exceeded")));
}

#[test]
fn cst_when_any_prefix_of_sample_then_reconstructs_and_terminates() {
    let full = POU;
    for (offset, _) in full.char_indices() {
        let prefix = &full[..offset];
        let parsed = build_cst(prefix);
        assert_eq!(
            reconstruct(&parsed.root),
            prefix,
            "prefix of length {offset} did not reconstruct"
        );
    }
}

#[test]
fn lexer_when_sample_then_lexer_coverage_has_no_gaps() {
    let (tokens, diagnostics) = lex(POU);
    assert!(diagnostics.is_empty());
    let coverage = check_coverage(POU, &tokens);
    assert_eq!(coverage.gaps, vec![]);
    assert_eq!(coverage.mismatches, vec![]);
}

const WIDE: &str = r#"{attribute 'x'}
FUNCTION Avg : REAL
VAR_INPUT
	a : INT;
	b : INT;
END_VAR
VAR
	acc : DINT := 0;
END_VAR
acc := a + b;
Avg(acc);
Avg := acc / 2.0;
END_FUNCTION
FUNCTION_BLOCK Counter
VAR CONSTANT
	step : INT := 2;
END_VAR
VAR_TEMP
	tmp : INT;
END_VAR
VAR_OUTPUT
	out : INT;
END_VAR
VAR
	value AT %QW1 : WORD;
	items : ARRAY [0..3] OF INT;
	name : ns.types.t;
	deref : DINT;
END_VAR
;
tmp := -step;
items[2] := add(1, 2, step);
IF tmp < 0 THEN
	value := 16#00FF;
ELSIF tmp = 0 THEN
	value := 0;
ELSE
	value := 1;
END_IF
deref^ := step;
END_FUNCTION_BLOCK"#;

#[test]
fn cst_when_wide_sample_then_reconstructs_and_builds_all_constructs() {
    let parsed = build_cst(WIDE);
    assert_eq!(parsed.errors, vec![], "expected a clean parse");
    assert_eq!(reconstruct(&parsed.root), WIDE);

    assert_eq!(
        nodes_of_kind(&parsed.root, SyntaxKind::ProgramDecl).len(),
        2
    );
    assert_eq!(nodes_of_kind(&parsed.root, SyntaxKind::VarBlock).len(), 6);
    assert_eq!(nodes_of_kind(&parsed.root, SyntaxKind::VarDecl).len(), 10);
    assert_eq!(nodes_of_kind(&parsed.root, SyntaxKind::DirectVar).len(), 1);
    assert_eq!(nodes_of_kind(&parsed.root, SyntaxKind::CallStmt).len(), 1);
    assert_eq!(nodes_of_kind(&parsed.root, SyntaxKind::CallExpr).len(), 1);
    assert_eq!(nodes_of_kind(&parsed.root, SyntaxKind::UnaryExpr).len(), 1);
    assert_eq!(nodes_of_kind(&parsed.root, SyntaxKind::EmptyStmt).len(), 1);
    assert_eq!(nodes_of_kind(&parsed.root, SyntaxKind::IfStmt).len(), 1);
}

const BROKEN: &str = "PROGRAM Broken\nVAR\n\tok : INT;\n\tmissing : ;\n\tp AT : INT;\n\tok2 : INT;\nEND_VAR\nx := f(,);\nEND_PROGRAM";

#[test]
fn cst_when_malformed_declarations_and_arguments_then_recovers_without_hanging() {
    let parsed = build_cst(BROKEN);
    assert_eq!(reconstruct(&parsed.root), BROKEN);
    assert_eq!(
        parsed.errors.len(),
        4,
        "missing type, bad direct-variable location, two bad arguments: {:?}",
        parsed.errors
    );
    assert_eq!(nodes_of_kind(&parsed.root, SyntaxKind::VarDecl).len(), 4);
    assert_eq!(nodes_of_kind(&parsed.root, SyntaxKind::AssignStmt).len(), 1);
    assert_eq!(nodes_of_kind(&parsed.root, SyntaxKind::ErrorNode).len(), 4);
}
