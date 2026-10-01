//! Tree shape of expressions and statements.
//!
//! Each case names the source and the exact tree it must produce, rendered
//! with trivia left out as `Kind(child child ...)` with tokens as their
//! text, so precedence, associativity and node structure are asserted and
//! not only "parsed without error".

mod common;

use common::{expression, statements};
use ironplc_syntax::{parse_expression, parse_statements, ParseOptions};

fn assert_expression_shapes(options: &ParseOptions, cases: &[(&str, &str)]) {
    for (source, expected) in cases {
        assert_eq!(expression(source, options), *expected, "{source}");
        assert!(parse_expression(source, options).is_ok(), "{source}");
    }
}

fn assert_statement_shapes(options: &ParseOptions, cases: &[(&str, &str)]) {
    for (source, expected) in cases {
        assert_eq!(statements(source, options), *expected, "{source}");
        assert!(parse_statements(source, options).is_ok(), "{source}");
    }
}

fn rejects_statements(source: &str, options: &ParseOptions) -> bool {
    !parse_statements(source, options).is_ok()
}

#[test]
fn parse_expression_when_mixed_precedence_then_tighter_operators_nest_deeper() {
    assert_expression_shapes(
        &ParseOptions::default(),
        &[
            (
                "a + b * c",
                "BinaryExpr(NameRef(a) + BinaryExpr(NameRef(b) * NameRef(c)))",
            ),
            (
                "a * b + c",
                "BinaryExpr(BinaryExpr(NameRef(a) * NameRef(b)) + NameRef(c))",
            ),
            (
                "a = b < c",
                "BinaryExpr(NameRef(a) = BinaryExpr(NameRef(b) < NameRef(c)))",
            ),
            (
                "a OR b XOR c AND d = e < f + g * h ** i",
                "BinaryExpr(NameRef(a) OR BinaryExpr(NameRef(b) XOR BinaryExpr(NameRef(c) AND BinaryExpr(NameRef(d) = BinaryExpr(NameRef(e) < BinaryExpr(NameRef(f) + BinaryExpr(NameRef(g) * BinaryExpr(NameRef(h) ** NameRef(i)))))))))",
            ),
            (
                "a | b & c",
                "BinaryExpr(NameRef(a) | BinaryExpr(NameRef(b) & NameRef(c)))",
            ),
            ("a MOD b", "BinaryExpr(NameRef(a) MOD NameRef(b))"),
        ],
    );
}

#[test]
fn parse_expression_when_same_level_then_left_associative() {
    assert_expression_shapes(
        &ParseOptions::default(),
        &[
            (
                "a - b - c",
                "BinaryExpr(BinaryExpr(NameRef(a) - NameRef(b)) - NameRef(c))",
            ),
            (
                "a / b * c",
                "BinaryExpr(BinaryExpr(NameRef(a) / NameRef(b)) * NameRef(c))",
            ),
            (
                "a ** b ** c",
                "BinaryExpr(BinaryExpr(NameRef(a) ** NameRef(b)) ** NameRef(c))",
            ),
            (
                "a < b > c",
                "BinaryExpr(BinaryExpr(NameRef(a) < NameRef(b)) > NameRef(c))",
            ),
            (
                "a = b <> c",
                "BinaryExpr(BinaryExpr(NameRef(a) = NameRef(b)) <> NameRef(c))",
            ),
        ],
    );
}

#[test]
fn parse_expression_when_short_circuit_operators_then_same_levels_as_and_or() {
    assert_expression_shapes(
        &ParseOptions::all(),
        &[(
            "a OR_ELSE b AND_THEN c",
            "BinaryExpr(NameRef(a) OR_ELSE BinaryExpr(NameRef(b) AND_THEN NameRef(c)))",
        )],
    );
}

#[test]
fn parse_expression_when_unary_operator_then_binds_tighter_than_binary() {
    assert_expression_shapes(
        &ParseOptions::default(),
        &[
            (
                "NOT a = b",
                "BinaryExpr(UnaryExpr(NOT NameRef(a)) = NameRef(b))",
            ),
            (
                "-a ** 2",
                "BinaryExpr(UnaryExpr(- NameRef(a)) ** IntLiteral(2))",
            ),
            ("NOT (a)", "UnaryExpr(NOT ParenExpr(( NameRef(a) )))"),
            ("-a^", "UnaryExpr(- DerefExpr(NameRef(a) ^))"),
        ],
    );
}

#[test]
fn parse_expression_when_parenthesised_then_paren_node_overrides_precedence() {
    assert_expression_shapes(
        &ParseOptions::default(),
        &[
            (
                "(a + b) * c",
                "BinaryExpr(ParenExpr(( BinaryExpr(NameRef(a) + NameRef(b)) )) * NameRef(c))",
            ),
            ("((a))", "ParenExpr(( ParenExpr(( NameRef(a) )) ))"),
        ],
    );
}

#[test]
fn parse_expression_when_literal_forms_then_one_literal_node_owns_the_pieces() {
    assert_expression_shapes(
        &ParseOptions::default(),
        &[
            ("5", "IntLiteral(5)"),
            ("16#FF", "IntLiteral(16#FF)"),
            ("INT#-5", "IntLiteral(INT # - 5)"),
            ("10#12", "IntLiteral(10 # 12)"),
            ("+5", "IntLiteral(+ 5)"),
            ("BYTE#16#FF", "BitStringLiteral(BYTE # 16#FF)"),
            ("1.5e3", "RealLiteral(1.5e3)"),
            ("REAL#1.5", "RealLiteral(REAL # 1.5)"),
            ("TRUE", "BoolLiteral(TRUE)"),
            ("BOOL#0", "BoolLiteral(BOOL # 0)"),
            ("'a'", "StringLiteral('a')"),
            ("\"w\"", "StringLiteral(\"w\")"),
            ("STRING#'x'", "StringLiteral(STRING # 'x')"),
            ("UTF8#'x'", "StringLiteral(UTF8 # 'x')"),
            ("T#1m30s", "DurationLiteral(T # 1 m30s)"),
            ("T#-5s", "DurationLiteral(T # - 5 s)"),
            ("TIME#1.5s", "DurationLiteral(TIME # 1.5 s)"),
            (
                "TOD#10:00:00.250",
                "TimeOfDayLiteral(TOD # 10 : 00 : 00.250)",
            ),
            ("D#2020-01-01", "DateLiteral(D # 2020 - 01 - 01)"),
            (
                "DT#2020-01-01-12:00:00",
                "DateTimeLiteral(DT # 2020 - 01 - 01 - 12 : 00 : 00)",
            ),
        ],
    );
}

#[test]
fn parse_expression_when_negative_number_then_unary_minus_over_literal() {
    assert_expression_shapes(
        &ParseOptions::default(),
        &[
            ("-5", "UnaryExpr(- IntLiteral(5))"),
            ("-1.5", "UnaryExpr(- RealLiteral(1.5))"),
        ],
    );
}

#[test]
fn parse_expression_when_variable_access_then_postfix_nodes_nest_outward() {
    assert_expression_shapes(
        &ParseOptions::all(),
        &[
            ("a.b", "FieldExpr(NameRef(a) . b)"),
            (
                "a[1, 2]",
                "IndexExpr(NameRef(a) [ IntLiteral(1) , IntLiteral(2) ])",
            ),
            (
                "a.b[1]^.c",
                "FieldExpr(DerefExpr(IndexExpr(FieldExpr(NameRef(a) . b) [ IntLiteral(1) ]) ^) . c)",
            ),
            ("a.3", "BitAccessExpr(NameRef(a) . 3)"),
            ("a.%X3", "PartialAccessExpr(NameRef(a) . %X3)"),
            ("a^", "DerefExpr(NameRef(a) ^)"),
            ("%IX0.1", "DirectAddressExpr(%IX0.1)"),
            ("THIS^", "SelfRefExpr(THIS ^)"),
            (
                "__CURRENTTASK^.Index",
                "FieldExpr(DerefExpr(NameRef(__CURRENTTASK) ^) . Index)",
            ),
            ("STEP", "NameRef(STEP)"),
        ],
    );
}

#[test]
fn parse_expression_when_calls_then_callee_and_argument_list_nodes() {
    assert_expression_shapes(
        &ParseOptions::all(),
        &[
            ("f()", "CallExpr(NameRef(f) ArgList(( )))"),
            (
                "f(1, x := 2, y => z)",
                "CallExpr(NameRef(f) ArgList(( PositionalArg(IntLiteral(1)) , NamedArg(NameRef(x) := IntLiteral(2)) , OutputArg(NameRef(y) => NameRef(z)) )))",
            ),
            (
                "f(NOT a => b)",
                "CallExpr(NameRef(f) ArgList(( OutputArg(NOT NameRef(a) => NameRef(b)) )))",
            ),
            (
                "a.m(1)",
                "CallExpr(FieldExpr(NameRef(a) . m) ArgList(( PositionalArg(IntLiteral(1)) )))",
            ),
            (
                "THIS^.m(1)",
                "CallExpr(FieldExpr(SelfRefExpr(THIS ^) . m) ArgList(( PositionalArg(IntLiteral(1)) )))",
            ),
            (
                "MOD(a, b)",
                "CallExpr(NameRef(MOD) ArgList(( PositionalArg(NameRef(a)) , PositionalArg(NameRef(b)) )))",
            ),
            (
                "NOT(a, b)",
                "CallExpr(NameRef(NOT) ArgList(( PositionalArg(NameRef(a)) , PositionalArg(NameRef(b)) )))",
            ),
            (
                "ADR(x)",
                "CallExpr(NameRef(ADR) ArgList(( PositionalArg(NameRef(x)) )))",
            ),
            (
                "__DELETE(p)",
                "CallExpr(NameRef(__DELETE) ArgList(( PositionalArg(NameRef(p)) )))",
            ),
        ],
    );
}

#[test]
fn parse_expression_when_reference_and_special_operators_then_dedicated_nodes() {
    assert_expression_shapes(
        &ParseOptions::all(),
        &[
            ("REF(a)", "RefExpr(REF ( NameRef(a) ))"),
            ("NULL", "NullLiteral(NULL)"),
            (
                "__NEW(INT, 5)",
                "SpecialOpExpr(__NEW ( TypeRef(INT) , IntLiteral(5) ))",
            ),
            ("__NEW(MyFb)", "SpecialOpExpr(__NEW ( TypeRef(MyFb) ))"),
            ("__TYPEOF(x)", "SpecialOpExpr(__TYPEOF ( TypeRef(x) ))"),
        ],
    );
}

#[test]
fn parse_statements_when_assignment_forms_then_one_assign_node_with_operator_node() {
    assert_statement_shapes(
        &ParseOptions::default(),
        &[
            (
                "x := 1;",
                "StatementList(AssignStmt(NameRef(x) AssignOp(:=) IntLiteral(1) ;))",
            ),
            (
                "x S= y;",
                "StatementList(AssignStmt(NameRef(x) AssignOp(S =) NameRef(y) ;))",
            ),
            (
                "x R= y;",
                "StatementList(AssignStmt(NameRef(x) AssignOp(R =) NameRef(y) ;))",
            ),
            (
                "x REF= y;",
                "StatementList(AssignStmt(NameRef(x) AssignOp(REF =) NameRef(y) ;))",
            ),
            (
                "p^ := 1;",
                "StatementList(AssignStmt(DerefExpr(NameRef(p) ^) AssignOp(:=) IntLiteral(1) ;))",
            ),
            (
                "a.b[1] := 2;",
                "StatementList(AssignStmt(IndexExpr(FieldExpr(NameRef(a) . b) [ IntLiteral(1) ]) AssignOp(:=) IntLiteral(2) ;))",
            ),
        ],
    );
}

#[test]
fn parse_statements_when_call_and_simple_statements_then_statement_nodes() {
    assert_statement_shapes(
        &ParseOptions::all(),
        &[
            (
                "f(1);",
                "StatementList(CallStmt(CallExpr(NameRef(f) ArgList(( PositionalArg(IntLiteral(1)) ))) ;))",
            ),
            (
                "a.m(1);",
                "StatementList(CallStmt(CallExpr(FieldExpr(NameRef(a) . m) ArgList(( PositionalArg(IntLiteral(1)) ))) ;))",
            ),
            (";", "StatementList(EmptyStmt(;))"),
            ("EXIT;", "StatementList(ExitStmt(EXIT ;))"),
            ("CONTINUE;", "StatementList(ContinueStmt(CONTINUE ;))"),
            ("RETURN;", "StatementList(ReturnStmt(RETURN ;))"),
        ],
    );
}

#[test]
fn parse_statements_when_if_then_clauses_nest_in_order() {
    assert_statement_shapes(
        &ParseOptions::default(),
        &[
            (
                "IF a THEN b := 1; ELSIF c THEN d := 2; ELSE e := 3; END_IF;",
                "StatementList(IfStmt(IF NameRef(a) THEN StatementList(AssignStmt(NameRef(b) AssignOp(:=) IntLiteral(1) ;)) ElsifClause(ELSIF NameRef(c) THEN StatementList(AssignStmt(NameRef(d) AssignOp(:=) IntLiteral(2) ;))) ElseClause(ELSE StatementList(AssignStmt(NameRef(e) AssignOp(:=) IntLiteral(3) ;))) END_IF ;))",
            ),
            (
                "IF a THEN END_IF;",
                "StatementList(IfStmt(IF NameRef(a) THEN StatementList() END_IF ;))",
            ),
        ],
    );
}

#[test]
fn parse_statements_when_case_then_branches_hold_labels_and_statements() {
    assert_statement_shapes(
        &ParseOptions::default(),
        &[(
            "CASE x OF 1, 3..5: a := 1; Color#Red: b := 2; 16#FF: c := 1; Green: ; ELSE d := 1; END_CASE;",
            "StatementList(CaseStmt(CASE NameRef(x) OF CaseBranch(CaseLabel(IntLiteral(1)) , CaseLabel(IntLiteral(3) .. IntLiteral(5)) : StatementList(AssignStmt(NameRef(a) AssignOp(:=) IntLiteral(1) ;))) CaseBranch(CaseLabel(NameRef(Color) # NameRef(Red)) : StatementList(AssignStmt(NameRef(b) AssignOp(:=) IntLiteral(2) ;))) CaseBranch(CaseLabel(BitStringLiteral(16#FF)) : StatementList(AssignStmt(NameRef(c) AssignOp(:=) IntLiteral(1) ;))) CaseBranch(CaseLabel(NameRef(Green)) : StatementList(EmptyStmt(;))) ElseClause(ELSE StatementList(AssignStmt(NameRef(d) AssignOp(:=) IntLiteral(1) ;))) END_CASE ;))",
        )],
    );
}

#[test]
fn parse_statements_when_signed_and_named_range_labels_then_bounds_are_literal_or_name() {
    assert_statement_shapes(
        &ParseOptions::default(),
        &[(
            "CASE x OF -1: ; LOW..HIGH: ; END_CASE;",
            "StatementList(CaseStmt(CASE NameRef(x) OF CaseBranch(CaseLabel(IntLiteral(- 1)) : StatementList(EmptyStmt(;))) CaseBranch(CaseLabel(NameRef(LOW) .. NameRef(HIGH)) : StatementList(EmptyStmt(;))) END_CASE ;))",
        )],
    );
}

#[test]
fn parse_statements_when_loops_then_loop_nodes() {
    assert_statement_shapes(
        &ParseOptions::default(),
        &[
            (
                "FOR i := 1 TO 3 BY 2 DO ; END_FOR;",
                "StatementList(ForStmt(FOR NameRef(i) := IntLiteral(1) TO IntLiteral(3) BY IntLiteral(2) DO StatementList(EmptyStmt(;)) END_FOR ;))",
            ),
            (
                "WHILE a DO ; END_WHILE;",
                "StatementList(WhileStmt(WHILE NameRef(a) DO StatementList(EmptyStmt(;)) END_WHILE ;))",
            ),
            (
                "REPEAT ; UNTIL a END_REPEAT;",
                "StatementList(RepeatStmt(REPEAT StatementList(EmptyStmt(;)) UNTIL NameRef(a) END_REPEAT ;))",
            ),
        ],
    );
}

#[test]
fn parse_statements_when_codesys_statements_then_their_nodes() {
    assert_statement_shapes(
        &ParseOptions::all(),
        &[
            (
                "__TRY a := 1; __CATCH (e) b := 2; __FINALLY c := 3; __ENDTRY;",
                "StatementList(TryStmt(__TRY StatementList(AssignStmt(NameRef(a) AssignOp(:=) IntLiteral(1) ;)) CatchClause(__CATCH ( NameRef(e) ) StatementList(AssignStmt(NameRef(b) AssignOp(:=) IntLiteral(2) ;))) FinallyClause(__FINALLY StatementList(AssignStmt(NameRef(c) AssignOp(:=) IntLiteral(3) ;))) __ENDTRY ;))",
            ),
            (
                "__THROW(1);",
                "StatementList(ThrowStmt(__THROW ( IntLiteral(1) ) ;))",
            ),
            (
                "JMP (c) lbl;",
                "StatementList(JmpStmt(JMP ( NameRef(c) ) NameRef(lbl) ;))",
            ),
            (
                "lbl: x := 1;",
                "StatementList(LabelStmt(lbl :) AssignStmt(NameRef(x) AssignOp(:=) IntLiteral(1) ;))",
            ),
            (
                "CALC(c, f(1));",
                "StatementList(CalcStmt(CALC ( NameRef(c) , CallExpr(NameRef(f) ArgList(( PositionalArg(IntLiteral(1)) ))) ) ;))",
            ),
            (
                "__WAIT(c);",
                "StatementList(WaitStmt(__WAIT ( NameRef(c) ) ;))",
            ),
            (
                "__BEGIN_IMPLEMENTATION; x := 1;",
                "StatementList(ImplementationMarker(__BEGIN_IMPLEMENTATION ;) AssignStmt(NameRef(x) AssignOp(:=) IntLiteral(1) ;))",
            ),
        ],
    );
}

#[test]
fn parse_statements_when_missing_semicolon_flag_then_end_keyword_statements_omit_it() {
    let allowed = ParseOptions {
        allow_missing_semicolon: true,
        ..ParseOptions::default()
    };
    assert_statement_shapes(
        &allowed,
        &[(
            "IF a THEN b := 1; END_IF c := 2;",
            "StatementList(IfStmt(IF NameRef(a) THEN StatementList(AssignStmt(NameRef(b) AssignOp(:=) IntLiteral(1) ;)) END_IF) AssignStmt(NameRef(c) AssignOp(:=) IntLiteral(2) ;))",
        )],
    );
    let strict = parse_statements("IF a THEN b := 1; END_IF c := 2;", &ParseOptions::default());
    assert_eq!(strict.errors.len(), 1);
    assert_eq!(strict.errors[0].message, "expected `;`");
}

#[test]
fn parse_statements_when_empty_case_branch_then_flag_decides_validity() {
    let source = "CASE x OF 1: 2: a := 1; END_CASE;";
    let strict = parse_statements(source, &ParseOptions::default());
    assert_eq!(strict.errors.len(), 1);
    let allowed = ParseOptions {
        allow_missing_semicolon: true,
        ..ParseOptions::default()
    };
    assert!(parse_statements(source, &allowed).is_ok());
}

#[test]
fn parse_statements_when_label_in_case_then_selector_not_label() {
    let all = ParseOptions::all();
    assert!(parse_statements("CASE x OF Red: a := 1; END_CASE;", &all).is_ok());
    // The same words outside a CASE are a label and a statement.
    assert_eq!(
        statements("Red: a := 1;", &all),
        "StatementList(LabelStmt(Red :) AssignStmt(NameRef(a) AssignOp(:=) IntLiteral(1) ;))"
    );
    // Inside a CASE a `name :` after a statement starts the next branch, even
    // with jump statements enabled.
    assert_eq!(
        statements("CASE x OF 1: b := 1; lbl: a := 1; END_CASE;", &all),
        "StatementList(CaseStmt(CASE NameRef(x) OF CaseBranch(CaseLabel(IntLiteral(1)) : StatementList(AssignStmt(NameRef(b) AssignOp(:=) IntLiteral(1) ;))) CaseBranch(CaseLabel(NameRef(lbl)) : StatementList(AssignStmt(NameRef(a) AssignOp(:=) IntLiteral(1) ;))) END_CASE ;))"
    );
}

#[test]
fn parse_statements_when_label_and_jump_flag_off_then_label_is_a_syntax_error() {
    let off = ParseOptions::default();
    assert!(rejects_statements("lbl: x := 1;", &off));
    let on = ParseOptions {
        allow_jump_statement: true,
        ..ParseOptions::default()
    };
    assert!(parse_statements("lbl: x := 1;", &on).is_ok());
}

#[test]
fn parse_statements_when_label_named_like_disabled_keyword_then_not_a_label() {
    // `continue` is a keyword token when labels are recognised, so it never
    // becomes a label even when the dialect leaves CONTINUE disabled.
    let jumps = ParseOptions {
        allow_jump_statement: true,
        ..ParseOptions::default()
    };
    assert!(rejects_statements("continue: x := 1;", &jumps));
    assert!(parse_statements("__new: x := 1;", &jumps).is_ok());
}

#[test]
fn parse_statements_when_contextual_keyword_gate_then_word_is_a_name_or_keyword_by_dialect() {
    let off = ParseOptions::default();
    let on = ParseOptions::all();
    // Disabled: an ordinary variable.
    assert_eq!(
        statements("continue := 1;", &off),
        "StatementList(AssignStmt(NameRef(continue) AssignOp(:=) IntLiteral(1) ;))"
    );
    // Enabled: the CONTINUE statement, and `continue := 1` is an error.
    assert_eq!(
        statements("CONTINUE;", &on),
        "StatementList(ContinueStmt(CONTINUE ;))"
    );
    assert!(rejects_statements("continue := 1;", &on));
}

#[test]
fn parse_expression_when_mod_or_not_in_lower_case_then_ordinary_names() {
    let options = ParseOptions::default();
    assert!(!parse_expression("a mod b", &options).is_ok());
    assert!(parse_statements("not := mod;", &options).is_ok());
    assert_eq!(
        expression("a MOD b", &options),
        "BinaryExpr(NameRef(a) MOD NameRef(b))"
    );
}

#[test]
fn parse_expression_when_time_as_function_name_flag_then_time_call_parses() {
    let flag = ParseOptions {
        allow_time_as_function_name: true,
        ..ParseOptions::default()
    };
    assert_eq!(
        expression("TIME()", &flag),
        "CallExpr(NameRef(TIME) ArgList(( )))"
    );
    assert!(!parse_expression("TIME()", &ParseOptions::default()).is_ok());
    assert_eq!(expression("TIME#5s", &flag), "DurationLiteral(TIME # 5 s)");
}

#[test]
fn parse_statements_when_reference_keywords_gate_then_ref_is_name_or_keyword() {
    let off = ParseOptions::default();
    // REF(x) is an ordinary call when REF is not a keyword.
    assert_eq!(
        expression("REF(x)", &off),
        "CallExpr(NameRef(REF) ArgList(( PositionalArg(NameRef(x)) )))"
    );
    assert_eq!(
        expression("REF(x)", &ParseOptions::all()),
        "RefExpr(REF ( NameRef(x) ))"
    );
    // REF= binds in both.
    assert!(parse_statements("a REF= b;", &off).is_ok());
    assert!(parse_statements("a REF= b;", &ParseOptions::all()).is_ok());
}

#[test]
fn parse_expression_when_expression_and_statement_entry_then_same_verdict() {
    let sources = [
        "a + b * c",
        "f(1, x := 2)",
        "T#1m30s",
        "a.b[1]^",
        "(a",
        "1 +",
        "NOT NOT a",
        "",
        "a b",
    ];
    for options in [ParseOptions::default(), ParseOptions::all()] {
        for source in sources {
            let direct = parse_expression(source, &options).is_ok();
            let wrapped = format!("x := {source};");
            let through_statement = parse_statements(&wrapped, &options).is_ok();
            assert_eq!(direct, through_statement, "{source}");
        }
    }
}
