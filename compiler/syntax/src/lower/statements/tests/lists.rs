use super::*;

// Lists.

#[test]
fn lower_statements_when_empty_statements_then_none_for_them_and_the_others_in_order() {
    assert_eq!(shows(""), "[]");
    assert_eq!(shows(";"), "[]");
    assert_eq!(shows(";;"), "[]");
    assert_eq!(shows("(* only a comment *)"), "[]");
    assert_eq!(shows("x := 1;; y := 2;"), "[(:= $x 1) (:= $y 2)]");
    assert_eq!(shows("; x := 1; ;"), "[(:= $x 1)]");
    assert_eq!(shows("IF a THEN ;; END_IF;"), "[(IF ?a [])]");
}

#[test]
fn lower_statements_when_block_ends_without_a_semicolon_then_the_same_statements() {
    let options = ParseOptions {
        allow_missing_semicolon: true,
        ..ParseOptions::default()
    };
    let shows = |source: &str| {
        lower_with(source, &options)
            .map(|list| show_all(&list))
            .ok()
    };
    assert_eq!(
        shows("IF a THEN b := 1; END_IF\nIF c THEN d := 2; END_IF\n").as_deref(),
        Some("[(IF ?a [(:= $b 1)]) (IF ?c [(:= $d 2)])]")
    );
    assert_eq!(
        shows("WHILE a DO IF b THEN c := 1; END_IF END_WHILE").as_deref(),
        Some("[(WHILE ?a [(IF ?b [(:= $c 1)])])]")
    );
    assert_eq!(
        shows("REPEAT IF a THEN b := 1; END_IF UNTIL c END_REPEAT x := 2;").as_deref(),
        Some("[(REPEAT [(IF ?a [(:= $b 1)])] UNTIL ?c) (:= $x 2)]")
    );
}

#[test]
fn lower_statements_when_comments_and_pragmas_between_statements_then_ignored() {
    assert_eq!(
        shows("x := 1; (* c *) // d\n {pragma} y := 2; /* e */"),
        "[(:= $x 1) (:= $y 2)]"
    );
}

#[test]
fn lower_statement_list_when_not_a_list_then_internal_error() {
    let parse = parse_statements("x := 1;", &all());
    let assignment = parse
        .root
        .descendants()
        .find(|node| node.kind() == SyntaxKind::AssignStmt)
        .expect("an assignment");
    assert_eq!(
        code(lower_statement_list(&LowerCx::new(file()), &assignment)),
        Some(INTERNAL_ERROR.to_string())
    );
}

#[test]
fn lower_statement_when_empty_statement_then_no_statement() {
    let parse = parse_statements(";", &all());
    let empty = parse
        .root
        .descendants()
        .find(|node| node.kind() == SyntaxKind::EmptyStmt)
        .expect("an empty statement");
    let lowered = lower_statement(&LowerCx::new(file()), &empty);
    assert_eq!(
        lowered.map(|statement| statement.is_none()).ok(),
        Some(true)
    );
}

// The tables.

#[test]
fn lower_statement_when_every_statement_kind_then_it_has_a_row_or_writes_nothing() {
    for kind in SyntaxKind::ALL {
        if disposition(*kind) != Disposition::Lowered(Area::Statement) {
            continue;
        }
        let listed = *kind == SyntaxKind::StatementList
            || WRITES_NOTHING.contains(kind)
            || STATEMENTS.iter().any(|(row, _, _)| row == kind);
        assert!(listed, "{kind:?} has no statement rule");
    }
}

#[test]
fn lower_statement_when_a_row_of_the_table_then_the_disposition_says_the_statement_area_owns_it() {
    for (kind, _, _) in STATEMENTS {
        assert_eq!(
            disposition(*kind),
            Disposition::Lowered(Area::Statement),
            "{kind:?}"
        );
    }
    for kind in [
        SyntaxKind::AssignOp,
        SyntaxKind::ElsifClause,
        SyntaxKind::ElseClause,
        SyntaxKind::CaseBranch,
        SyntaxKind::CaseLabel,
        SyntaxKind::CatchClause,
        SyntaxKind::FinallyClause,
    ] {
        assert_eq!(disposition(kind), Disposition::Structural, "{kind:?}");
    }
}

// Errors.

/// A node of `kind` holding `children` as the tree would, built by hand: the
/// parser never builds a tree that lacks what its kind requires, so a rule's
/// answer to one is only reachable this way.
fn built(kind: SyntaxKind, children: Vec<NodeOrToken<GreenNode, rowan::GreenToken>>) -> SyntaxNode {
    SyntaxNode::new_root(GreenNode::new(rowan::SyntaxKind(kind as u16), children))
}

#[test]
fn lower_statement_when_node_lacks_what_its_kind_requires_then_internal_error() {
    let cx = LowerCx::new(file());
    for (kind, _, _) in STATEMENTS {
        let diagnostic = lower_statement(&cx, &built(*kind, vec![])).err();
        let code = diagnostic.map(|diagnostic| diagnostic.code);
        // Return, and the markers that hold only a word, need nothing more.
        let needs_nothing = matches!(
            kind,
            SyntaxKind::ReturnStmt
                | SyntaxKind::WaitStmt
                | SyntaxKind::ThrowStmt
                | SyntaxKind::TryStmt
        );
        if needs_nothing && *kind != SyntaxKind::TryStmt {
            continue;
        }
        assert_eq!(code, Some(INTERNAL_ERROR.to_string()), "{kind:?}");
    }
}

#[test]
fn lower_statement_when_assignment_operator_is_unknown_then_internal_error() {
    let parse = parse_statements("x := 1;", &all());
    let assignment = parse
        .root
        .descendants()
        .find(|node| node.kind() == SyntaxKind::AssignStmt)
        .expect("an assignment");
    // `:=` read as `=` is no operator of the table.
    let green = assignment.green().into_owned().replace_child(
        1,
        NodeOrToken::Node(GreenNode::new(
            rowan::SyntaxKind(SyntaxKind::AssignOp as u16),
            vec![NodeOrToken::Token(rowan::GreenToken::new(
                rowan::SyntaxKind(SyntaxKind::Equal as u16),
                "=",
            ))],
        )),
    );
    let node = SyntaxNode::new_root(green);
    assert_eq!(
        code(lower_statement(&LowerCx::new(file()), &node)),
        Some(INTERNAL_ERROR.to_string())
    );
}

#[test]
fn lower_statement_when_calc_calls_a_method_then_internal_error() {
    let parse = parse_statements("CALC(c, f(1));", &all());
    let calc = parse
        .root
        .descendants()
        .find(|node| node.kind() == SyntaxKind::CalcStmt)
        .expect("a CALC");
    let method_call = parse_statements("a.b(1);", &all());
    let call = method_call
        .root
        .descendants()
        .find(|node| node.kind() == SyntaxKind::CallExpr)
        .expect("a call");
    let condition = calc.first_child().expect("a condition");
    let green = GreenNode::new(
        rowan::SyntaxKind(SyntaxKind::CalcStmt as u16),
        vec![
            NodeOrToken::Node(condition.green().into_owned()),
            NodeOrToken::Node(call.green().into_owned()),
        ],
    );
    assert_eq!(
        code(lower_statement(
            &LowerCx::new(file()),
            &SyntaxNode::new_root(green)
        )),
        Some(INTERNAL_ERROR.to_string())
    );
}

#[test]
fn lower_statement_when_node_is_pending_then_not_implemented() {
    let parse = parse_source_file("PROGRAM p\nEND_PROGRAM\n", &all());
    let declaration = parse
        .root
        .descendants()
        .find(|node| node.kind() == SyntaxKind::ProgramDecl)
        .expect("a declaration");
    assert_eq!(
        code(lower_statement(&LowerCx::new(file()), &declaration)),
        Some(NOT_IMPLEMENTED.to_string())
    );
}

#[test]
fn lower_statements_when_literal_does_not_denote_a_value_then_the_error_comes_back_through_the_result(
) {
    let too_large = "99999999999999999999999999999999999999999";
    for source in [
        format!("x := {too_large};"),
        format!("IF a THEN WHILE b DO x := {too_large}; END_WHILE; END_IF;"),
        format!("CASE x OF {too_large}: a := 1; END_CASE;"),
        format!("CASE x OF 1: a := 1; ELSE b := {too_large}; END_CASE;"),
        format!("FOR i := 1 TO {too_large} DO ; END_FOR;"),
        format!("a[{too_large}] := 1;"),
    ] {
        assert_eq!(
            code(lower_result(&source)),
            Some(Problem::SyntaxError.code().to_string()),
            "{source}"
        );
    }
}

#[test]
fn lower_statements_when_a_statement_fails_then_the_error_is_over_its_text() {
    let source = "a := 1; b := 99999999999999999999999999999999999999999;";
    let diagnostic = lower_result(source).err();
    let range = diagnostic.map(|d| (d.primary.location.start, d.primary.location.end));
    assert_eq!(range, Some((13, 13 + 41)));
}

#[test]
fn lower_statements_when_parse_reported_errors_then_the_first_error_and_no_statements() {
    let parse = parse_statements("x := ;", &ParseOptions::default());
    assert_eq!(
        code(crate::lower::lower_statements(&parse, &file())),
        Some(Problem::SyntaxError.code().to_string())
    );
}

// Stack safety. A list of any length is walked by a loop, and only nesting
// recurses, which the tree depth limit bounds.

/// Runs `body` on a thread with the smallest stack a caller has: the 1 MiB of
/// the Windows main thread.
fn on_small_stack<T: Send + 'static>(body: impl FnOnce() -> T + Send + 'static) -> Option<T> {
    std::thread::Builder::new()
        .stack_size(1024 * 1024)
        .spawn(body)
        .ok()?
        .join()
        .ok()
}

/// Runs `body` on a thread of the size the compiler gives its stages.
fn on_budget_stack<T: Send + 'static>(body: impl FnOnce() -> T + Send + 'static) -> Option<T> {
    std::thread::Builder::new()
        .stack_size(STACK_BUDGET)
        .spawn(body)
        .ok()?
        .join()
        .ok()
}

#[test]
fn lower_statements_when_list_is_very_long_then_lowered_by_iteration_on_the_smallest_stack() {
    let count = on_small_stack(|| {
        let source = "x := 1;\n".repeat(200_000);
        let parse = parse_statements(&source, &all());
        assert!(parse.is_ok(), "{:?}", parse.errors);
        let statements = crate::lower::lower_statements(&parse, &file()).expect("lowers");
        statements.len()
    });
    assert_eq!(count, Some(200_000));
}

#[test]
fn lower_statements_when_long_lists_inside_blocks_then_each_body_is_iterated() {
    let count = on_small_stack(|| {
        let body = "x := 1; ;\n".repeat(50_000);
        let source = format!("IF a THEN {body} ELSE {body} END_IF; WHILE b DO {body} END_WHILE;");
        let parse = parse_statements(&source, &all());
        assert!(parse.is_ok(), "{:?}", parse.errors);
        let statements = crate::lower::lower_statements(&parse, &file()).expect("lowers");
        match &statements[0] {
            StmtKind::If(node) => Some(node.body.len() + node.else_body.len()),
            _ => None,
        }
        .expect("an IF")
    });
    assert_eq!(count, Some(100_000));
}

/// `IF a THEN` repeated `depth` times around one assignment.
fn nested_ifs(depth: usize) -> String {
    format!(
        "{}x := 1;{}",
        "IF a THEN ".repeat(depth),
        " END_IF;".repeat(depth)
    )
}

/// How many `IF` statements deep `statement` goes.
fn if_depth(statement: &StmtKind) -> usize {
    let mut depth = 0;
    let mut current = statement;
    while let StmtKind::If(node) = current {
        depth += 1;
        match node.body.first() {
            Some(inner) => current = inner,
            None => break,
        }
    }
    depth
}

/// The deepest nesting of `IF` the tree depth limit allows: a statement list
/// and an `IF` for each level, over the file, the outermost list and the
/// assignment with its parts.
fn deepest_nesting() -> usize {
    (1..MAX_DEPTH)
        .rev()
        .find(|depth| parse_statements(&nested_ifs(*depth), &all()).is_ok())
        .unwrap_or(0)
}

#[test]
fn lower_statements_when_nesting_is_as_deep_as_the_tree_allows_then_lowered_on_the_stack_budget() {
    let depth = on_budget_stack(|| {
        let nesting = deepest_nesting();
        let parse = parse_statements(&nested_ifs(nesting), &all());
        assert!(parse.is_ok(), "{:?}", parse.errors);
        let statements = crate::lower::lower_statements(&parse, &file()).expect("lowers");
        (nesting, if_depth(&statements[0]))
    });
    let (nesting, lowered) = depth.expect("the thread finished");
    assert!(nesting > MAX_DEPTH / 4, "{nesting}");
    assert_eq!(lowered, nesting);
}

#[test]
fn lower_statements_when_nesting_is_one_level_too_deep_then_the_parse_reports_the_depth() {
    let nesting = on_budget_stack(deepest_nesting).expect("the thread finished");
    let parse = parse_statements(&nested_ifs(nesting + 1), &all());
    assert!(parse
        .errors
        .iter()
        .any(|error| error.kind == ErrorKind::NestingTooDeep));
}

#[test]
fn lower_statements_when_every_block_nests_then_lowered_on_the_stack_budget() {
    let count = on_budget_stack(|| {
        let mut source = String::from("x := 1;");
        let blocks = [
            ("IF a THEN ", " END_IF;"),
            ("WHILE a DO ", " END_WHILE;"),
            ("FOR i := 1 TO 2 DO ", " END_FOR;"),
            ("REPEAT ", " UNTIL a END_REPEAT;"),
            ("CASE a OF 1: ", " END_CASE;"),
            ("__TRY ", " __ENDTRY;"),
        ];
        for round in 0..100 {
            let (open, close) = blocks[round % blocks.len()];
            source = format!("{open}{source}{close}");
        }
        let parse = parse_statements(&source, &all());
        assert!(parse.is_ok(), "{:?}", parse.errors);
        let statements = crate::lower::lower_statements(&parse, &file()).expect("lowers");
        statements.len()
    });
    assert_eq!(count, Some(1));
}
