//! Tree shape of declarations.
//!
//! Each case names the source and the exact tree it must produce, rendered
//! with trivia left out as `Kind(child child ...)` with tokens as their text,
//! so the structure of every declaration form is asserted and not only
//! "parsed without error". The cases also pin the meaning of words that depend
//! on declaration context: a statement label against a declared name, a
//! contextual keyword used as a name, and the dialect gates on declaration
//! syntax.

mod common;

use common::file;
use ironplc_syntax::{parse_source_file, ParseOptions, SyntaxKind};

fn assert_shapes(options: &ParseOptions, cases: &[(&str, &str)]) {
    for (source, expected) in cases {
        assert_eq!(file(source, options), *expected, "{source}");
        let parsed = parse_source_file(source, options);
        assert_eq!(parsed.errors, vec![], "{source}");
        assert_eq!(parsed.root.text().to_string(), *source);
    }
}

fn count(source: &str, options: &ParseOptions, kind: SyntaxKind) -> usize {
    parse_source_file(source, options)
        .root
        .descendants()
        .filter(|node| node.kind() == kind)
        .count()
}

#[test]
fn parse_source_file_when_program_function_and_function_block_then_header_blocks_body_closer() {
    assert_shapes(
        &ParseOptions::all(),
        &[
            (
                "PROGRAM p VAR x : INT; END_VAR x := 1; END_PROGRAM",
                "ProgramDecl(PROGRAM Name(p) VarBlock(VAR VarDecl(Name(x) : TypeRef(INT)) ; END_VAR) StatementList(AssignStmt(NameRef(x) AssignOp(:=) IntLiteral(1) ;)) END_PROGRAM)",
            ),
            (
                "FUNCTION f : INT f := 1; END_FUNCTION",
                "FunctionDecl(FUNCTION Name(f) : TypeRef(INT) StatementList(AssignStmt(NameRef(f) AssignOp(:=) IntLiteral(1) ;)) END_FUNCTION)",
            ),
            (
                "FUNCTION f : STRING[8] f := 'a'; END_FUNCTION",
                "FunctionDecl(FUNCTION Name(f) : StringType(STRING [ IntLiteral(8) ]) StatementList(AssignStmt(NameRef(f) AssignOp(:=) StringLiteral('a') ;)) END_FUNCTION)",
            ),
            (
                "FUNCTION_BLOCK fb END_FUNCTION_BLOCK",
                "FunctionBlockDecl(FUNCTION_BLOCK Name(fb) END_FUNCTION_BLOCK)",
            ),
            (
                "PROGRAM p END_PROGRAM PROGRAM q END_PROGRAM",
                "ProgramDecl(PROGRAM Name(p) END_PROGRAM) ProgramDecl(PROGRAM Name(q) END_PROGRAM)",
            ),
        ],
    );
}

#[test]
fn parse_source_file_when_function_block_inherits_then_qualifiers_extends_and_implements_nodes() {
    assert_shapes(
        &ParseOptions::all(),
        &[
            (
                "FUNCTION_BLOCK ABSTRACT fb EXTENDS base IMPLEMENTS i, j END_FUNCTION_BLOCK",
                "FunctionBlockDecl(FUNCTION_BLOCK MemberQualifier(ABSTRACT) Name(fb) ExtendsClause(EXTENDS TypeRef(base)) ImplementsClause(IMPLEMENTS TypeRef(i) , TypeRef(j)) END_FUNCTION_BLOCK)",
            ),
            (
                "FUNCTION_BLOCK FINAL fb VAR_GENERIC CONSTANT n : INT := 4; END_VAR END_FUNCTION_BLOCK",
                "FunctionBlockDecl(FUNCTION_BLOCK MemberQualifier(FINAL) Name(fb) VarBlock(VAR_GENERIC CONSTANT VarDecl(Name(n) : TypeRef(INT) Initializer(:= IntLiteral(4))) ; END_VAR) END_FUNCTION_BLOCK)",
            ),
        ],
    );
}

#[test]
fn parse_source_file_when_method_then_qualifiers_return_type_blocks_and_body() {
    assert_shapes(
        &ParseOptions::all(),
        &[
            (
                "FUNCTION_BLOCK fb METHOD PUBLIC FINAL m : BOOL VAR_INPUT a : INT; END_VAR m := TRUE; END_METHOD END_FUNCTION_BLOCK",
                "FunctionBlockDecl(FUNCTION_BLOCK Name(fb) MethodDecl(METHOD MemberQualifier(PUBLIC) MemberQualifier(FINAL) Name(m) : TypeRef(BOOL) VarBlock(VAR_INPUT VarDecl(Name(a) : TypeRef(INT)) ; END_VAR) StatementList(AssignStmt(NameRef(m) AssignOp(:=) BoolLiteral(TRUE) ;)) END_METHOD) END_FUNCTION_BLOCK)",
            ),
            (
                "FUNCTION_BLOCK fb METHOD ABSTRACT m END_METHOD END_FUNCTION_BLOCK",
                "FunctionBlockDecl(FUNCTION_BLOCK Name(fb) MethodDecl(METHOD MemberQualifier(ABSTRACT) Name(m) END_METHOD) END_FUNCTION_BLOCK)",
            ),
            (
                "FUNCTION_BLOCK fb x := 1; METHOD m END_METHOD END_FUNCTION_BLOCK",
                "FunctionBlockDecl(FUNCTION_BLOCK Name(fb) StatementList(AssignStmt(NameRef(x) AssignOp(:=) IntLiteral(1) ;)) MethodDecl(METHOD Name(m) END_METHOD) END_FUNCTION_BLOCK)",
            ),
        ],
    );
}

#[test]
fn parse_source_file_when_property_then_accessors_are_nodes() {
    assert_shapes(
        &ParseOptions::all(),
        &[
            (
                "FUNCTION_BLOCK fb PROPERTY p : INT GET p := 1; END_GET SET x := p; END_SET END_PROPERTY END_FUNCTION_BLOCK",
                "FunctionBlockDecl(FUNCTION_BLOCK Name(fb) PropertyDecl(PROPERTY Name(p) : TypeRef(INT) GetAccessor(GET StatementList(AssignStmt(NameRef(p) AssignOp(:=) IntLiteral(1) ;)) END_GET) SetAccessor(SET StatementList(AssignStmt(NameRef(x) AssignOp(:=) NameRef(p) ;)) END_SET) END_PROPERTY) END_FUNCTION_BLOCK)",
            ),
            (
                "FUNCTION_BLOCK fb PROPERTY p : INT END_PROPERTY END_FUNCTION_BLOCK",
                "FunctionBlockDecl(FUNCTION_BLOCK Name(fb) PropertyDecl(PROPERTY Name(p) : TypeRef(INT) END_PROPERTY) END_FUNCTION_BLOCK)",
            ),
        ],
    );
}

#[test]
fn parse_source_file_when_interface_and_namespaces_then_nodes_nest() {
    assert_shapes(
        &ParseOptions::all(),
        &[
            (
                "INTERFACE i EXTENDS j, k END_INTERFACE",
                "InterfaceDecl(INTERFACE Name(i) ExtendsClause(EXTENDS TypeRef(j) , TypeRef(k)) END_INTERFACE)",
            ),
            (
                "NAMESPACE a NAMESPACE b END_NAMESPACE END_NAMESPACE",
                "NamespaceDecl(NAMESPACE Name(a) NamespaceDecl(NAMESPACE Name(b) END_NAMESPACE) END_NAMESPACE)",
            ),
        ],
    );
}

#[test]
fn parse_source_file_when_variable_blocks_then_qualifier_names_location_and_edge_are_children() {
    assert_shapes(
        &ParseOptions::all(),
        &[
            (
                "PROGRAM p VAR_INPUT RETAIN a, b : INT := 1; c : BOOL R_EDGE; END_VAR VAR x AT %IX0.0 : BOOL; END_VAR END_PROGRAM",
                "ProgramDecl(PROGRAM Name(p) VarBlock(VAR_INPUT RETAIN VarDecl(Name(a) , Name(b) : TypeRef(INT) Initializer(:= IntLiteral(1))) ; VarDecl(Name(c) : TypeRef(BOOL) EdgeSpec(R_EDGE)) ; END_VAR) VarBlock(VAR VarDecl(Name(x) Location(AT %IX0.0) : TypeRef(BOOL)) ; END_VAR) END_PROGRAM)",
            ),
            (
                "PROGRAM p VAR CONSTANT n : INT := 1; END_VAR VAR_IN_OUT s : STRING[4]; END_VAR END_PROGRAM",
                "ProgramDecl(PROGRAM Name(p) VarBlock(VAR CONSTANT VarDecl(Name(n) : TypeRef(INT) Initializer(:= IntLiteral(1))) ; END_VAR) VarBlock(VAR_IN_OUT VarDecl(Name(s) : StringType(STRING [ IntLiteral(4) ])) ; END_VAR) END_PROGRAM)",
            ),
            (
                "VAR_GLOBAL CONSTANT g : INT := 1; END_VAR",
                "VarBlock(VAR_GLOBAL CONSTANT VarDecl(Name(g) : TypeRef(INT) Initializer(:= IntLiteral(1))) ; END_VAR)",
            ),
            (
                "PROGRAM p VAR x : fb(a := 1); y : REF_TO INT := REF(z); END_VAR END_PROGRAM",
                "ProgramDecl(PROGRAM Name(p) VarBlock(VAR VarDecl(Name(x) : TypeRef(fb) ArgList(( NamedArg(NameRef(a) := IntLiteral(1)) ))) ; VarDecl(Name(y) : RefType(REF_TO TypeRef(INT)) Initializer(:= RefExpr(REF ( NameRef(z) )))) ; END_VAR) END_PROGRAM)",
            ),
        ],
    );
}

#[test]
fn parse_source_file_when_type_declarations_then_each_kind_of_type_is_a_node() {
    assert_shapes(
        &ParseOptions::all(),
        &[
            (
                "TYPE t : INT(0..10) := 5; u : (A, B := 2) BYTE := A; END_TYPE",
                "TypeBlock(TYPE TypeDecl(Name(t) : SubrangeType(TypeRef(INT) ( Subrange(IntLiteral(0) .. IntLiteral(10)) )) Initializer(:= IntLiteral(5))) ; TypeDecl(Name(u) : EnumType(( EnumValue(Name(A)) , EnumValue(Name(B) := IntLiteral(2)) ) TypeRef(BYTE)) Initializer(:= NameRef(A))) ; END_TYPE)",
            ),
            (
                "TYPE v : ARRAY[1..3, 0..1] OF INT := [1, 2(0)]; w : ARRAY[*] OF STRING[8]; END_TYPE",
                "TypeBlock(TYPE TypeDecl(Name(v) : ArrayType(ARRAY [ Subrange(IntLiteral(1) .. IntLiteral(3)) , Subrange(IntLiteral(0) .. IntLiteral(1)) ] OF TypeRef(INT)) Initializer(:= ArrayInit([ IntLiteral(1) , RepeatedInit(IntLiteral(2) ( IntLiteral(0) )) ]))) ; TypeDecl(Name(w) : ArrayType(ARRAY [ * ] OF StringType(STRING [ IntLiteral(8) ]))) ; END_TYPE)",
            ),
            (
                "TYPE s : STRUCT a : INT := 1; b : STRING[4]; END_STRUCT; o : UNION a : INT; b : REAL; END_UNION; END_TYPE",
                "TypeBlock(TYPE TypeDecl(Name(s) : StructType(STRUCT StructMember(Name(a) : TypeRef(INT) Initializer(:= IntLiteral(1))) ; StructMember(Name(b) : StringType(STRING [ IntLiteral(4) ])) ; END_STRUCT)) ; TypeDecl(Name(o) : UnionType(UNION StructMember(Name(a) : TypeRef(INT)) ; StructMember(Name(b) : TypeRef(REAL)) ; END_UNION)) ; END_TYPE)",
            ),
            (
                "TYPE q : PARAMS(2) OF INT; k : s := (a := 1, b := Color#Red); END_TYPE",
                "TypeBlock(TYPE TypeDecl(Name(q) : ParamsType(PARAMS ( IntLiteral(2) ) OF TypeRef(INT))) ; TypeDecl(Name(k) : TypeRef(s) Initializer(:= StructInit(( StructInitElement(NameRef(a) := IntLiteral(1)) , StructInitElement(NameRef(b) := EnumValueRef(NameRef(Color) # NameRef(Red))) )))) ; END_TYPE)",
            ),
            (
                "TYPE r : REFERENCE TO INT; p : POINTER TO DINT; END_TYPE",
                "TypeBlock(TYPE TypeDecl(Name(r) : RefType(REFERENCE TO TypeRef(INT))) ; TypeDecl(Name(p) : RefType(POINTER TO TypeRef(DINT))) ; END_TYPE)",
            ),
        ],
    );
}

#[test]
fn parse_source_file_when_configuration_then_resource_tasks_programs_and_instances_are_nodes() {
    assert_shapes(
        &ParseOptions::all(),
        &[(
            "CONFIGURATION c VAR_GLOBAL g : INT; END_VAR RESOURCE r ON t TASK k (INTERVAL := T#10ms, PRIORITY := 1); PROGRAM RETAIN p WITH k : q (a := 1, b => g, fb WITH k); END_RESOURCE VAR_CONFIG r.p.x AT %QW0 : INT := 5; END_VAR END_CONFIGURATION",
            "ConfigurationDecl(CONFIGURATION Name(c) VarBlock(VAR_GLOBAL VarDecl(Name(g) : TypeRef(INT)) ; END_VAR) ResourceDecl(RESOURCE Name(r) ON NameRef(t) TaskDecl(TASK Name(k) TaskInit(( TaskInitItem(NameRef(INTERVAL) := DurationLiteral(T # 10 ms)) , TaskInitItem(NameRef(PRIORITY) := IntLiteral(1)) ))) ; ProgramConfig(PROGRAM RETAIN Name(p) WITH Name(k) : TypeRef(q) ( ProgramConnection(NameRef(a) := IntLiteral(1)) , ProgramConnection(NameRef(b) => NameRef(g)) , TaskBinding(NameRef(fb) WITH NameRef(k)) )) ; END_RESOURCE) VarBlock(VAR_CONFIG InstanceInit(FieldExpr(FieldExpr(NameRef(r) . p) . x) Location(AT %QW0) : TypeRef(INT) Initializer(:= IntLiteral(5))) ; END_VAR) END_CONFIGURATION)",
        )],
    );
}

#[test]
fn parse_source_file_when_sequential_function_chart_then_steps_transitions_and_actions_are_nodes() {
    assert_shapes(
        &ParseOptions::all(),
        &[(
            "PROGRAM p INITIAL_STEP s : a(N); b(SD, T#1s, i); END_STEP STEP t : END_STEP TRANSITION tr (PRIORITY := 1) FROM s TO (t, u) := x > 1; END_TRANSITION ACTION a : x := 1; END_ACTION END_PROGRAM",
            "ProgramDecl(PROGRAM Name(p) SfcBody(InitialStepDecl(INITIAL_STEP Name(s) : ActionAssociation(NameRef(a) ( ActionQualifier(N) )) ; ActionAssociation(NameRef(b) ( ActionQualifier(SD , DurationLiteral(T # 1 s)) , NameRef(i) )) ; END_STEP) StepDecl(STEP Name(t) : END_STEP) TransitionDecl(TRANSITION Name(tr) TransitionPriority(( PRIORITY := IntLiteral(1) )) FROM StepList(NameRef(s)) TO StepList(( NameRef(t) , NameRef(u) )) TransitionCondition(:= BinaryExpr(NameRef(x) > IntLiteral(1)) ;) END_TRANSITION) ActionDecl(ACTION Name(a) : StatementList(AssignStmt(NameRef(x) AssignOp(:=) IntLiteral(1) ;)) END_ACTION)) END_PROGRAM)",
        )],
    );
}

#[test]
fn parse_source_file_when_label_in_statement_position_then_label_and_otherwise_declared_name() {
    let options = ParseOptions::all();
    // Statement position: a label.
    for source in [
        "PROGRAM p lbl: x := 1; END_PROGRAM",
        "PROGRAM p VAR x : INT; END_VAR lbl: x := 1; END_PROGRAM",
        "FUNCTION f : INT lbl: f := 1; END_FUNCTION",
        "FUNCTION_BLOCK fb METHOD m lbl: x := 1; END_METHOD END_FUNCTION_BLOCK",
        "PROGRAM p IF a THEN lbl: x := 1; END_IF; END_PROGRAM",
        "PROGRAM p INITIAL_STEP s : END_STEP ACTION a : lbl: x := 1; END_ACTION END_PROGRAM",
    ] {
        assert_eq!(
            count(source, &options, SyntaxKind::LabelStmt),
            1,
            "{source}"
        );
        assert!(parse_source_file(source, &options).is_ok(), "{source}");
    }
    // Declaration position: a name, never a label.
    for source in [
        "FUNCTION f : INT f := 1; END_FUNCTION",
        "FUNCTION_BLOCK fb METHOD Override : BOOL END_METHOD END_FUNCTION_BLOCK",
        "FUNCTION_BLOCK fb METHOD PUBLIC FINAL m : INT END_METHOD END_FUNCTION_BLOCK",
        "FUNCTION_BLOCK fb METHOD ABSTRACT m : INT END_METHOD END_FUNCTION_BLOCK",
        "FUNCTION_BLOCK fb PROPERTY ABSTRACT p : INT END_PROPERTY END_FUNCTION_BLOCK",
        "PROGRAM p VAR lbl : INT; END_VAR END_PROGRAM",
        "TYPE lbl : STRUCT lbl2 : INT; END_STRUCT; END_TYPE",
        "PROGRAM p INITIAL_STEP lbl : END_STEP END_PROGRAM",
        "CONFIGURATION c RESOURCE r ON t TASK lbl (PRIORITY := 1); PROGRAM inst WITH lbl : q; END_RESOURCE END_CONFIGURATION",
        "PROGRAM p CASE a OF red: x := 1; blue: y := 2; END_CASE; END_PROGRAM",
    ] {
        assert_eq!(count(source, &options, SyntaxKind::LabelStmt), 0, "{source}");
        assert!(parse_source_file(source, &options).is_ok(), "{source}");
    }
}

#[test]
fn parse_source_file_when_label_flag_off_then_a_label_is_an_error_and_a_declared_name_is_not() {
    let strict = ParseOptions::default();
    assert!(!parse_source_file("PROGRAM p lbl: x := 1; END_PROGRAM", &strict).is_ok());
    assert!(parse_source_file("FUNCTION f : INT f := 1; END_FUNCTION", &strict).is_ok());
    assert!(parse_source_file("PROGRAM p VAR lbl : INT; END_VAR END_PROGRAM", &strict).is_ok());
}

#[test]
fn parse_source_file_when_qualifier_word_is_the_name_then_it_is_a_name() {
    let options = ParseOptions::all();
    // The word is a qualifier only when the declared name still follows it.
    let named = "FUNCTION_BLOCK fb METHOD Override : BOOL END_METHOD END_FUNCTION_BLOCK";
    assert_eq!(count(named, &options, SyntaxKind::MemberQualifier), 0);
    let body = "FUNCTION_BLOCK fb METHOD Override x := 1; END_METHOD END_FUNCTION_BLOCK";
    assert_eq!(count(body, &options, SyntaxKind::MemberQualifier), 0);
    assert_eq!(count(body, &options, SyntaxKind::AssignStmt), 1);
    let qualified = "FUNCTION_BLOCK fb METHOD Override m : BOOL END_METHOD END_FUNCTION_BLOCK";
    assert_eq!(count(qualified, &options, SyntaxKind::MemberQualifier), 1);
    for source in [named, body, qualified] {
        assert!(parse_source_file(source, &options).is_ok(), "{source}");
    }
}

#[test]
fn parse_source_file_when_contextual_keywords_are_names_then_accepted() {
    let strict = ParseOptions::default();
    for source in [
        "PROGRAM p VAR STEP : INT; ON : INT; R_EDGE : INT; F_EDGE : INT; END_VAR END_PROGRAM",
        "PROGRAM p VAR S : INT; R : INT; T : INT; END_VAR S := 1; END_PROGRAM",
        "PROGRAM p VAR continue : INT; jmp : INT; bit : INT; ltime : INT; ref : INT; null : INT; END_VAR END_PROGRAM",
        "PROGRAM p VAR extends : INT; method : INT; interface : INT; this : INT; persistent : INT; union : INT; namespace : INT; params : INT; END_VAR END_PROGRAM",
        "TYPE t : (STEP, ON, R_EDGE, F_EDGE); END_TYPE",
        "FUNCTION TIME : TIME TIME := T#1s; END_FUNCTION",
    ] {
        let allowed = ParseOptions {
            allow_time_as_function_name: true,
            ..strict
        };
        assert_eq!(parse_source_file(source, &allowed).errors, vec![], "{source}");
    }
    // A keyword the dialect enables is not a name.
    assert!(!parse_source_file(
        "PROGRAM p VAR method : INT; END_VAR END_PROGRAM",
        &ParseOptions::all()
    )
    .is_ok());
}

#[test]
fn parse_source_file_when_declaration_syntax_is_gated_then_error_until_the_flag_is_on() {
    for source in [
        "NAMESPACE n END_NAMESPACE",
        "TYPE t : UNION a : INT; END_UNION; END_TYPE",
        "PROGRAM p VAR_STAT x : INT; END_VAR END_PROGRAM",
        "FUNCTION_BLOCK fb VAR_GENERIC CONSTANT n : INT; END_VAR END_FUNCTION_BLOCK",
        "FUNCTION_BLOCK fb VAR x : INT; END_VAR METHOD m END_METHOD END_FUNCTION_BLOCK",
        "FUNCTION_BLOCK fb PROPERTY p : INT END_PROPERTY END_FUNCTION_BLOCK",
        "FUNCTION_BLOCK fb EXTENDS base END_FUNCTION_BLOCK",
        "FUNCTION_BLOCK fb IMPLEMENTS i END_FUNCTION_BLOCK",
        "INTERFACE i END_INTERFACE",
        "PROGRAM p VAR x : REF_TO INT; END_VAR END_PROGRAM",
        "PROGRAM p VAR x : POINTER TO INT; END_VAR END_PROGRAM",
        "PROGRAM p VAR x : REFERENCE TO INT; END_VAR END_PROGRAM",
        "PROGRAM p VAR x : PARAMS(2) OF INT; END_VAR END_PROGRAM",
        "PROGRAM p VAR PERSISTENT x : INT; END_VAR END_PROGRAM",
        "PROGRAM p VAR x : STRING(10); END_VAR END_PROGRAM",
        "PROGRAM p VAR x : ARRAY[*] OF INT; END_VAR END_PROGRAM",
        "PROGRAM p VAR END_VAR END_PROGRAM",
        "PROGRAM p VAR_INPUT RETAIN END_VAR END_PROGRAM",
        "PROGRAM p __BEGIN_IMPLEMENTATION x := 1; END_PROGRAM",
        "PROGRAM p lbl: x := 1; END_PROGRAM",
        "PROGRAM p VAR `a b` : INT; END_VAR END_PROGRAM",
        "PROGRAM p VAR caf\u{e9} : INT; END_VAR END_PROGRAM",
        "PROGRAM p VAR a__b : INT; END_VAR END_PROGRAM",
    ] {
        let strict = parse_source_file(source, &ParseOptions::default());
        assert_eq!(strict.root.text().to_string(), source);
        assert!(!strict.is_ok(), "{source:?} accepted when disabled");
        let enabled = parse_source_file(source, &ParseOptions::all());
        assert_eq!(enabled.errors, vec![], "{source:?} rejected when enabled");
    }
}

fn error_ranges(source: &str) -> Vec<(usize, usize)> {
    parse_source_file(source, &ParseOptions::default())
        .errors
        .iter()
        .map(|error| {
            (
                usize::from(error.range.start()),
                usize::from(error.range.end()),
            )
        })
        .collect()
}

#[test]
fn parse_source_file_when_token_check_rule_fires_then_error_covers_the_offending_bytes() {
    // Empty variable block: the block's keyword.
    assert_eq!(
        error_ranges("PROGRAM p VAR END_VAR END_PROGRAM"),
        vec![(10, 13)]
    );
    // Parenthesised string length: the opening parenthesis.
    assert_eq!(
        error_ranges("PROGRAM p VAR s : STRING(8); END_VAR END_PROGRAM"),
        vec![(24, 25)]
    );
    // Incomplete array: the star.
    assert_eq!(
        error_ranges("PROGRAM p VAR a : ARRAY[*] OF INT; END_VAR END_PROGRAM"),
        vec![(24, 25)]
    );
    // Undefined string escape: the escape, `$` and the character after it.
    assert_eq!(
        error_ranges("PROGRAM p s := 'a$zb'; END_PROGRAM"),
        vec![(17, 19)]
    );
}
