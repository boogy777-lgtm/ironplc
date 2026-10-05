use super::*;

// Assignments: every operator the table has.

#[test]
fn lower_statements_when_assignment_operator_then_the_flags_of_its_row() {
    let rows = [
        ("x := y;", "[(:= $x ?y)]"),
        ("x S= y;", "[(S= $x ?y)]"),
        ("x R= y;", "[(R= $x ?y)]"),
        ("x REF= y;", "[(REF= $x (REF $y))]"),
        ("x := y + 1;", "[(:= $x (+ ?y 1))]"),
        ("x REF= y.z;", "[(REF= $x (REF $y.z))]"),
        ("x REF= a[1];", "[(REF= $x (REF $a[1]))]"),
    ];
    for (source, expected) in rows {
        assert_eq!(shows(source), expected, "{source}");
    }
    assert_eq!(ASSIGNMENT_OPERATORS.len(), 4);
}

#[test]
fn lower_statements_when_bind_operators_then_exactly_one_flag_is_on() {
    let flags = |source: &str| match one(source) {
        StmtKind::Assignment(assignment) => (
            assignment.ref_bind,
            assignment.set_bind,
            assignment.reset_bind,
            assignment.deref,
        ),
        _ => (false, false, false, false),
    };
    assert_eq!(flags("x := 1;"), (false, false, false, false));
    assert_eq!(flags("x S= 1;"), (false, true, false, false));
    assert_eq!(flags("x R= 1;"), (false, false, true, false));
    assert_eq!(flags("x REF= y;"), (true, false, false, false));
    assert_eq!(flags("p^ := 1;"), (false, false, false, true));
}

#[test]
fn lower_statements_when_operator_is_lower_case_then_the_same_form() {
    assert_eq!(shows("x s= y;"), "[(S= $x ?y)]");
    assert_eq!(shows("x r= y;"), "[(R= $x ?y)]");
    assert_eq!(shows("x ref= y;"), "[(REF= $x (REF $y))]");
}

#[test]
fn lower_statements_when_target_ends_in_a_caret_then_the_assignment_is_through_the_pointer() {
    assert_eq!(shows("p^ := 1;"), "[(:= $p^ 1)]");
    assert_eq!(shows("a[1]^ := 1;"), "[(:= $a[1]^ 1)]");
    assert_eq!(shows("a.b^ := 1;"), "[(:= $a.b^ 1)]");
    // A caret in the middle of the target is a part of the target.
    assert_eq!(shows("a^.b := 1;"), "[(:= $a^.b 1)]");
}

#[test]
fn lower_statements_when_assignment_then_positioned_at_its_operator() {
    for (source, operator) in [
        ("x := 1;", ":="),
        ("x S= 1;", "S="),
        ("x R= 1;", "R="),
        ("x REF= y;", "REF="),
        ("p^ := 1;", ":="),
    ] {
        let statement = one(source);
        assert_eq!(at(source, &statement.span()), operator, "{source}");
    }
}

#[test]
fn lower_statements_when_reference_binding_then_the_value_is_positioned_at_the_place_it_names() {
    let source = "x REF= y.z;";
    let assignment = assignment_of(source);
    assert_eq!(at(source, &assignment.value.span()), "y.z");
}

#[test]
fn lower_statements_when_every_assignment_form_then_each_part_is_positioned_where_written() {
    let source = "a.b[i] := c + d;";
    let assignment = assignment_of(source);
    assert_eq!(at(source, &assignment.target.span()), "a.b[i]");
    assert_eq!(at(source, &assignment.value.span()), "c + d");
}

// The extent of a target: the closing bracket and the caret belong to no part
// of the objects, so the subscript and the dereference carry them.

#[test]
fn lower_statements_when_target_is_subscripted_then_its_span_covers_the_closing_bracket() {
    for (source, target) in [
        ("a[i] := 1;", "a[i]"),
        ("a[1, 2] := 1;", "a[1, 2]"),
        ("a . b [ 1 ] := 1;", "a . b [ 1 ]"),
        ("a[1][2] := 1;", "a[1][2]"),
    ] {
        let assignment = assignment_of(source);
        assert_eq!(at(source, &assignment.target.span()), target, "{source}");
    }
}

#[test]
fn lower_statements_when_array_in_a_chain_then_each_subscript_covers_what_it_selects_from() {
    let source = "a[1][2] := 1;";
    let assignment = assignment_of(source);
    let outer = array_of(&assignment.target);
    assert_eq!(at(source, &outer.span), "a[1][2]");
    assert_eq!(at(source, &outer.subscripted_variable.span()), "a[1]");
}

#[test]
fn lower_statements_when_dereference_inside_the_target_then_its_span_covers_the_caret() {
    let source = "p^.x := 1;";
    let assignment = assignment_of(source);
    let field = structured_of(&assignment.target);
    let deref = deref_of(field.record.as_ref());
    assert_eq!(at(source, &deref.span), "p^");
    assert_eq!(at(source, &assignment.target.span()), "p^.x");
}

#[test]
fn lower_statements_when_pointer_target_then_the_target_is_the_pointer_and_the_flag_says_through() {
    let source = "p^ := 1;";
    let assignment = assignment_of(source);
    assert!(assignment.deref);
    assert_eq!(at(source, &assignment.target.span()), "p");
}

// Calls.

#[test]
fn lower_statements_when_call_then_a_function_block_call_or_a_method_call() {
    let rows = [
        ("f();", "[(call f())]"),
        ("f(1, 2);", "[(call f(1, 2))]"),
        ("f(a := 1, b := 2);", "[(call f(a := 1, b := 2))]"),
        ("f(a => x);", "[(call f(a => $x))]"),
        ("f(1, a := 2, b => c);", "[(call f(1, a := 2, b => $c))]"),
        ("inst.Run(1);", "[(call inst.Run(1))]"),
        ("THIS^.Run();", "[(call THIS^.Run())]"),
        ("SUPER^.Run(x := 1);", "[(call SUPER^.Run(x := 1))]"),
        ("fbs[i](IN := x);", "[(call fbs[i](IN := ?x))]"),
        ("fbs[i, j](Q => y);", "[(call fbs[i, j](Q => $y))]"),
        ("fbs[i][j]();", "[(call fbs[i][j]())]"),
    ];
    for (source, expected) in rows {
        assert_eq!(shows(source), expected, "{source}");
    }
}

#[test]
fn lower_statements_when_negated_output_argument_then_the_negation_is_recorded() {
    assert_eq!(shows("f(NOT a => x);"), "[(call f(NOT a => $x))]");
    assert_eq!(shows("inst.M(NOT a => x);"), "[(call inst.M(NOT a => $x))]");
}

#[test]
fn lower_statements_when_call_then_positioned_from_the_callee_through_the_closing_parenthesis() {
    let source = "inst . Run(1, 2) ;";
    let statement = one(source);
    assert_eq!(at(source, &statement.span()), "inst . Run(1, 2)");
    let source = "f(1) ;";
    assert_eq!(at(source, &one(source).span()), "f(1)");
    let source = "fbs [ i ] (1) ;";
    assert_eq!(at(source, &one(source).span()), "fbs [ i ] (1)");
}

// Selection.

#[test]
fn lower_statements_when_if_then_condition_branches_and_else() {
    let rows = [
        ("IF a THEN b := 1; END_IF;", "[(IF ?a [(:= $b 1)])]"),
        ("IF a THEN END_IF;", "[(IF ?a [])]"),
        (
            "IF a THEN b := 1; ELSE c := 2; END_IF;",
            "[(IF ?a [(:= $b 1)] ELSE [(:= $c 2)])]",
        ),
        (
            "IF a THEN b := 1; ELSIF c THEN d := 2; ELSIF e THEN f := 3; ELSE g := 4; END_IF;",
            "[(IF ?a [(:= $b 1)] ELSIF ?c [(:= $d 2)] ELSIF ?e [(:= $f 3)] ELSE [(:= $g 4)])]",
        ),
        (
            "IF a THEN IF b THEN c := 1; END_IF; END_IF;",
            "[(IF ?a [(IF ?b [(:= $c 1)])])]",
        ),
        (
            "IF a THEN ELSIF b THEN ; END_IF;",
            "[(IF ?a [] ELSIF ?b [])]",
        ),
        ("if a then b := 1; end_if;", "[(IF ?a [(:= $b 1)])]"),
    ];
    for (source, expected) in rows {
        assert_eq!(shows(source), expected, "{source}");
    }
}

#[test]
fn lower_statements_when_block_then_positioned_through_its_closing_word_without_the_terminator() {
    // A block statement may omit its `;`; the others may not.
    let blocks = [
        "IF a THEN b := 1; END_IF",
        "CASE a OF 1: b := 1; END_CASE",
        "FOR i := 1 TO 2 DO a := 1; END_FOR",
        "WHILE a DO b := 1; END_WHILE",
        "REPEAT a := 1; UNTIL b END_REPEAT",
        "__TRY a := 1; __ENDTRY",
    ];
    let terminated = ["JMP l", "CALC(c, f(1))", "x := 1", "f(1)", "EXIT"];
    let spellings = |statement: &str, can_omit: bool| {
        let mut spellings = vec![format!("{statement};"), format!("{statement} ;")];
        if can_omit {
            spellings.push(format!("{statement}\n"));
        }
        spellings
    };
    for (statement, can_omit) in blocks
        .iter()
        .map(|block| (*block, true))
        .chain(terminated.iter().map(|statement| (*statement, false)))
    {
        for source in spellings(statement, can_omit) {
            let lowered = lower(&source).remove(0);
            assert_eq!(
                at(&source, &lowered.span()),
                match lowered {
                    // An assignment is positioned at its operator.
                    StmtKind::Assignment(_) => ":=",
                    _ => statement,
                },
                "{source:?}"
            );
        }
    }
}

#[test]
fn lower_statements_when_case_labels_then_the_selector_each_form_denotes() {
    let rows = [
        (
            "CASE x OF 1: a := 1; 2: b := 2; END_CASE;",
            "[(CASE ?x (+1: [(:= $a 1)]) (+2: [(:= $b 2)]))]",
        ),
        (
            "CASE x OF 1, 2, 3: a := 1; END_CASE;",
            "[(CASE ?x (+1, +2, +3: [(:= $a 1)]))]",
        ),
        (
            "CASE x OF -1: a := 1; +2: b := 2; END_CASE;",
            "[(CASE ?x (-1: [(:= $a 1)]) (+2: [(:= $b 2)]))]",
        ),
        (
            "CASE x OF 1..5: a := 1; END_CASE;",
            "[(CASE ?x (+1..+5: [(:= $a 1)]))]",
        ),
        (
            "CASE x OF -3..-1: a := 1; END_CASE;",
            "[(CASE ?x (-3..-1: [(:= $a 1)]))]",
        ),
        (
            "CASE x OF A..B: a := 1; END_CASE;",
            "[(CASE ?x (A..B: [(:= $a 1)]))]",
        ),
        (
            "CASE x OF 1..B: a := 1; END_CASE;",
            "[(CASE ?x (+1..B: [(:= $a 1)]))]",
        ),
        (
            "CASE x OF Red: a := 1; Green, Blue: b := 2; END_CASE;",
            "[(CASE ?x (Red: [(:= $a 1)]) (Green, Blue: [(:= $b 2)]))]",
        ),
        (
            "CASE x OF Color#Red: a := 1; END_CASE;",
            "[(CASE ?x (Color#Red: [(:= $a 1)]))]",
        ),
        (
            "CASE x OF 16#FF: a := 1; END_CASE;",
            "[(CASE ?x (bits 255: [(:= $a 1)]))]",
        ),
        (
            "CASE x OF 8#17: a := 1; END_CASE;",
            "[(CASE ?x (bits 15: [(:= $a 1)]))]",
        ),
        (
            "CASE x OF 2#1010: a := 1; END_CASE;",
            "[(CASE ?x (bits 10: [(:= $a 1)]))]",
        ),
        (
            "CASE x OF 1, 16#FF, Red, 2..3, T#V: a := 1; END_CASE;",
            "[(CASE ?x (+1, bits 255, Red, +2..+3, T#V: [(:= $a 1)]))]",
        ),
        (
            "CASE x OF 1: a := 1; ELSE b := 2; END_CASE;",
            "[(CASE ?x (+1: [(:= $a 1)]) ELSE [(:= $b 2)])]",
        ),
        ("CASE x OF 1: ; END_CASE;", "[(CASE ?x (+1: []))]"),
    ];
    for (source, expected) in rows {
        assert_eq!(shows(source), expected, "{source}");
    }
}

#[test]
fn lower_statements_when_case_label_is_a_bit_string_then_untyped_and_positioned_as_written() {
    let source = "CASE x OF 16#FF: a := 1; END_CASE;";
    let case = case_of(source);
    let bits = bits_of(&case.statement_groups[0].selectors[0]);
    assert!(bits.data_type.is_none());
    assert_eq!(at(source, &bits.value.span), "16#FF");
}

#[test]
fn lower_statements_when_case_label_is_a_signed_number_then_positioned_as_written() {
    let source = "CASE x OF -1: a := 1; +2: b := 2; END_CASE;";
    let case = case_of(source);
    let span = |group: usize| match &case.statement_groups[group].selectors[0] {
        CaseSelectionKind::SignedInteger(number) => at(source, &number.value.span).to_string(),
        _ => String::new(),
    };
    assert_eq!(span(0), "-1");
    assert_eq!(span(1), "+2");
}

#[test]
fn lower_statements_when_case_branches_are_empty_and_the_dialect_allows_it_then_empty_groups() {
    let options = ParseOptions {
        allow_missing_semicolon: true,
        ..ParseOptions::default()
    };
    let lowered = lower_with("CASE x OF 1,\n2:\n3..5:\n6: y := 1; END_CASE", &options)
        .map(|statements| show_all(&statements));
    assert_eq!(
        lowered.ok().as_deref(),
        Some("[(CASE ?x (+1, +2: []) (+3..+5: []) (+6: [(:= $y 1)]))]")
    );
}

#[test]
fn lower_statements_when_case_nested_in_a_branch_then_the_branches_stay_apart() {
    assert_eq!(
        shows("CASE x OF 1: CASE y OF 2: a := 1; END_CASE; 3: b := 2; END_CASE;"),
        "[(CASE ?x (+1: [(CASE ?y (+2: [(:= $a 1)]))]) (+3: [(:= $b 2)]))]"
    );
}

// Iteration.

#[test]
fn lower_statements_when_loops_then_control_bounds_condition_and_body() {
    let rows = [
        (
            "FOR i := 1 TO 10 DO a := i; END_FOR;",
            "[(FOR i 1 10 [(:= $a ?i)])]",
        ),
        (
            "FOR i := 1 TO 10 BY 2 DO a := i; END_FOR;",
            "[(FOR i 1 10 BY 2 [(:= $a ?i)])]",
        ),
        (
            "FOR i := 10 TO 1 BY -1 DO ; END_FOR;",
            "[(FOR i 10 1 BY (- 1) [])]",
        ),
        (
            "FOR i := a + 1 TO b * 2 DO c := 1; END_FOR;",
            "[(FOR i (+ ?a 1) (* ?b 2) [(:= $c 1)])]",
        ),
        ("WHILE a DO b := 1; END_WHILE;", "[(WHILE ?a [(:= $b 1)])]"),
        (
            "REPEAT a := 1; UNTIL b END_REPEAT;",
            "[(REPEAT [(:= $a 1)] UNTIL ?b)]",
        ),
        (
            "REPEAT a := 1; b := 2; UNTIL c = 1 END_REPEAT;",
            "[(REPEAT [(:= $a 1) (:= $b 2)] UNTIL (= ?c 1))]",
        ),
        ("WHILE TRUE DO EXIT; END_WHILE;", "[(WHILE TRUE [EXIT])]"),
        (
            "WHILE TRUE DO CONTINUE; END_WHILE;",
            "[(WHILE TRUE [CONTINUE])]",
        ),
        (
            "FOR i := 1 TO 3 DO WHILE a DO EXIT; END_WHILE; END_FOR;",
            "[(FOR i 1 3 [(WHILE ?a [EXIT])])]",
        ),
    ];
    for (source, expected) in rows {
        assert_eq!(shows(source), expected, "{source}");
    }
}

#[test]
fn lower_statements_when_loop_control_then_positioned_at_the_word() {
    for (source, word) in [("EXIT;", "EXIT"), ("  CONTINUE ;", "CONTINUE")] {
        let statement = one(source);
        assert_eq!(at(source, &statement.span()), word, "{source}");
    }
}

#[test]
fn lower_statements_when_return_then_return() {
    assert_eq!(shows("RETURN;"), "[RETURN]");
}

// The statements CODESYS adds.

#[test]
fn lower_statements_when_codesys_statements_then_their_parts() {
    let rows = [
        ("JMP lbl;", "[(JMP lbl)]"),
        ("JMP (c) lbl;", "[(JMP IF ?c lbl)]"),
        ("JMP (a AND b) lbl;", "[(JMP IF (AND ?a ?b) lbl)]"),
        ("lbl: x := 1;", "[(LABEL lbl) (:= $x 1)]"),
        ("a: b: x := 1;", "[(LABEL a) (LABEL b) (:= $x 1)]"),
        ("lbl: JMP lbl;", "[(LABEL lbl) (JMP lbl)]"),
        ("CALC(c, f(1));", "[(CALC ?c f(1))]"),
        ("CALC(c, f());", "[(CALC ?c f())]"),
        (
            "CALC(a > 1, f(x := 2, y => z));",
            "[(CALC (> ?a 1) f(x := 2, y => $z))]",
        ),
        ("__WAIT;", "[(WAIT)]"),
        ("__WAIT(c);", "[(WAIT ?c)]"),
        ("__THROW;", "[(THROW)]"),
        ("__THROW(1);", "[(THROW 1)]"),
        ("__BEGIN_IMPLEMENTATION x := 1;", "[BEGIN (:= $x 1)]"),
        ("__BEGIN_IMPLEMENTATION; x := 1;", "[BEGIN (:= $x 1)]"),
        ("__BEGIN_IMPLEMENTATION", "[BEGIN]"),
        (
            "x := 1; __BEGIN_IMPLEMENTATION y := 2;",
            "[(:= $x 1) BEGIN (:= $y 2)]",
        ),
    ];
    for (source, expected) in rows {
        assert_eq!(shows(source), expected, "{source}");
    }
}

#[test]
fn lower_statements_when_try_then_body_catch_variable_and_finally() {
    let rows = [
        (
            "__TRY a := 1; __CATCH b := 2; __ENDTRY;",
            "[(TRY [(:= $a 1)] CATCH - [(:= $b 2)])]",
        ),
        (
            "__TRY a := 1; __CATCH(e) b := 2; __FINALLY c := 3; __ENDTRY;",
            "[(TRY [(:= $a 1)] CATCH $e [(:= $b 2)] FINALLY [(:= $c 3)])]",
        ),
        (
            "__TRY a := 1; __CATCH() b := 2; __ENDTRY;",
            "[(TRY [(:= $a 1)] CATCH - [(:= $b 2)])]",
        ),
        (
            "__TRY a := 1; __FINALLY c := 3; __ENDTRY;",
            "[(TRY [(:= $a 1)] FINALLY [(:= $c 3)])]",
        ),
        ("__TRY __ENDTRY;", "[(TRY [])]"),
        ("__TRY __CATCH (e.f) __ENDTRY;", "[(TRY [] CATCH $e.f [])]"),
    ];
    for (source, expected) in rows {
        assert_eq!(shows(source), expected, "{source}");
    }
}

#[test]
fn lower_statements_when_codesys_statement_then_positioned_as_its_object_defines() {
    // Anchored at the word: a statement whose operand is optional, a label and
    // a clause.
    for (source, word) in [
        ("__WAIT(c);", "__WAIT"),
        ("__THROW(1);", "__THROW"),
        ("lbl: x := 1;", "lbl"),
        ("__BEGIN_IMPLEMENTATION x := 1;", "__BEGIN_IMPLEMENTATION"),
    ] {
        let statement = lower(source).remove(0);
        assert_eq!(at(source, &statement.span()), word, "{source}");
    }
    let source = "__TRY a := 1; __CATCH (e) b := 2; __ENDTRY;";
    let statement = try_of(source);
    assert_eq!(
        at(
            source,
            &statement
                .catch
                .as_ref()
                .map(|c| c.span())
                .unwrap_or_default()
        ),
        "__CATCH"
    );
    assert_eq!(
        at(source, &statement.span()),
        "__TRY a := 1; __CATCH (e) b := 2; __ENDTRY"
    );
    let source = "JMP (c) lbl;";
    assert_eq!(at(source, &one(source).span()), "JMP (c) lbl");
}
