use super::*;
use crate::lower::{disposition, INTERNAL_ERROR, NOT_IMPLEMENTED};
use crate::{parse_expression, parse_statements, ParseOptions, SyntaxKind, MAX_DEPTH};
use ironplc_dsl::core::{FileId, Located};
use ironplc_dsl::textual::{SelfRefKind, SymbolicVariableKind, Variable};
use ironplc_problems::Problem;
use rowan::{GreenNode, NodeOrToken};

fn file() -> FileId {
    FileId::from_string("t.st")
}

/// Every flag on, so that the dialect keywords (`REF`, `OR_ELSE`) are words of
/// the language.
fn all() -> ParseOptions {
    ParseOptions::all()
}

/// The expression node of `source`, ignoring any error the parse reported, so
/// that malformed trees reach the rules.
fn expression_node(source: &str) -> Option<SyntaxNode> {
    parse_expression(source, &all()).root.first_child()
}

fn lower_result(source: &str) -> Result<Expr, Diagnostic> {
    let node = expression_node(source).ok_or_else(Diagnostic::internal_error)?;
    lower_expr(&LowerCx::new(file()), &node)
}

fn lower(source: &str) -> Expr {
    lower_result(source).expect("the expression lowers")
}

fn code(result: Result<Expr, Diagnostic>) -> Option<String> {
    result.err().map(|diagnostic| diagnostic.code)
}

/// An argument as written: `value`, `name := value` or `[NOT ]name => $target`.
fn show_param(param: &ParamAssignmentKind) -> String {
    match param {
        ParamAssignmentKind::PositionalInput(input) => show(&input.expr),
        ParamAssignmentKind::NamedInput(input) => {
            format!("{} := {}", input.name, show(&input.expr))
        }
        ParamAssignmentKind::Output(output) => format!(
            "{}{} => ${}",
            if output.not { "NOT " } else { "" },
            output.src,
            output.tgt
        ),
    }
}

fn show_args(params: &[ParamAssignmentKind]) -> String {
    params.iter().map(show_param).collect::<Vec<_>>().join(", ")
}

/// The expression written as one line: an operation as `(operator operands)`, a
/// late-bound name as `?name`, a variable as `$variable`, a dereference as
/// `(^ operand)`.
fn show(expr: &Expr) -> String {
    match &expr.kind {
        ExprKind::Compare(op) => format!("({} {} {})", op.op, show(&op.left), show(&op.right)),
        ExprKind::BinaryOp(op) => format!("({} {} {})", op.op, show(&op.left), show(&op.right)),
        ExprKind::UnaryOp(op) => format!("({} {})", op.op, show(&op.term)),
        ExprKind::Expression(inner) => show(inner),
        ExprKind::Const(constant) => constant.to_string(),
        ExprKind::EnumeratedValue(value) => value.to_string(),
        ExprKind::Variable(variable) => format!("${variable}"),
        ExprKind::Function(function) => {
            format!(
                "{}({})",
                function.name,
                show_args(&function.param_assignment)
            )
        }
        ExprKind::MethodCall(call) => format!(
            "{}.{}({})",
            call.receiver,
            call.method,
            show_args(&call.params)
        ),
        ExprKind::LateBound(late) => format!("?{late}"),
        ExprKind::Ref(variable) => format!("(REF ${variable})"),
        ExprKind::Deref(inner) => format!("(^ {})", show(inner)),
        ExprKind::Null(_) => "NULL".to_string(),
    }
}

fn shows(source: &str) -> String {
    show(&lower(source))
}

/// The text of `source` that `expr` says it was written as.
fn written<'a>(source: &'a str, expr: &Expr) -> &'a str {
    let span = expr.span();
    &source[span.start..span.end]
}

fn function(expr: &Expr) -> Option<&Function> {
    match &expr.kind {
        ExprKind::Function(function) => Some(function),
        _ => None,
    }
}

fn method(expr: &Expr) -> Option<&MethodCall> {
    match &expr.kind {
        ExprKind::MethodCall(call) => Some(call),
        _ => None,
    }
}

#[test]
fn lower_expr_when_binary_operator_then_the_operation_of_its_row() {
    let rows = [
        ("a OR b", "(OR ?a ?b)"),
        ("a OR_ELSE b", "(OR_ELSE ?a ?b)"),
        ("a XOR b", "(XOR ?a ?b)"),
        ("a AND b", "(AND ?a ?b)"),
        ("a AND_THEN b", "(AND_THEN ?a ?b)"),
        ("a = b", "(= ?a ?b)"),
        ("a <> b", "(<> ?a ?b)"),
        ("a < b", "(< ?a ?b)"),
        ("a > b", "(> ?a ?b)"),
        ("a <= b", "(<= ?a ?b)"),
        ("a >= b", "(>= ?a ?b)"),
        ("a + b", "(+ ?a ?b)"),
        ("a - b", "(- ?a ?b)"),
        ("a * b", "(* ?a ?b)"),
        ("a / b", "(/ ?a ?b)"),
        ("a MOD b", "(MOD ?a ?b)"),
        ("a ** b", "(** ?a ?b)"),
    ];
    for (source, expected) in rows {
        assert_eq!(shows(source), expected, "{source}");
    }
}

#[test]
fn lower_expr_when_binary_operator_then_logical_ones_are_comparisons_and_the_rest_arithmetic() {
    let kind = |source: &str| match lower(source).kind {
        ExprKind::Compare(_) => "compare",
        ExprKind::BinaryOp(_) => "binary",
        _ => "other",
    };
    for source in ["a OR b", "a XOR b", "a AND b", "a = b", "a >= b"] {
        assert_eq!(kind(source), "compare", "{source}");
    }
    for source in ["a + b", "a - b", "a * b", "a / b", "a MOD b", "a ** b"] {
        assert_eq!(kind(source), "binary", "{source}");
    }
}

#[test]
fn lower_expr_when_operators_of_several_levels_then_the_shape_follows_precedence() {
    let rows = [
        ("1 + 2 * 3", "(+ 1 (* 2 3))"),
        ("1 * 2 + 3", "(+ (* 1 2) 3)"),
        ("a OR b AND c", "(OR ?a (AND ?b ?c))"),
        ("a AND b OR c XOR d", "(OR (AND ?a ?b) (XOR ?c ?d))"),
        ("a + b < c * d", "(< (+ ?a ?b) (* ?c ?d))"),
        ("a < b = c", "(= (< ?a ?b) ?c)"),
        ("a * b ** c", "(* ?a (** ?b ?c))"),
        ("a - (b - c)", "(- ?a (- ?b ?c))"),
        ("(a + b) * c", "(* (+ ?a ?b) ?c)"),
        ("NOT a = b", "(= (NOT ?a) ?b)"),
        ("NOT a AND b", "(AND (NOT ?a) ?b)"),
        ("-a ** 2", "(** (- ?a) 2)"),
    ];
    for (source, expected) in rows {
        assert_eq!(shows(source), expected, "{source}");
    }
}

#[test]
fn lower_expr_when_operators_of_one_level_then_they_associate_to_the_left() {
    let rows = [
        ("1 - 2 - 3", "(- (- 1 2) 3)"),
        ("1 / 2 * 3", "(* (/ 1 2) 3)"),
        ("1 ** 2 ** 3", "(** (** 1 2) 3)"),
        ("a = b = c", "(= (= ?a ?b) ?c)"),
        ("a OR b OR c", "(OR (OR ?a ?b) ?c)"),
        ("1 + 2 - 3 + 4", "(+ (- (+ 1 2) 3) 4)"),
    ];
    for (source, expected) in rows {
        assert_eq!(shows(source), expected, "{source}");
    }
}

#[test]
fn lower_expr_when_unary_operator_then_the_operation_on_its_operand() {
    let rows = [
        ("-a", "(- ?a)"),
        ("NOT a", "(NOT ?a)"),
        ("- a", "(- ?a)"),
        ("-a.b", "(- $a.b)"),
        ("NOT f(1)", "(NOT f(1))"),
        ("-a^", "(- (^ $a))"),
        ("-(a + b)", "(- (+ ?a ?b))"),
    ];
    for (source, expected) in rows {
        assert_eq!(shows(source), expected, "{source}");
    }
}

#[test]
fn lower_expr_when_negated_literal_then_still_an_operation_on_the_literal() {
    // Only an initial value reads a negated literal as a signed constant.
    assert_eq!(shows("-5"), "(- 5)");
    assert_eq!(shows("- 5"), "(- 5)");
    assert_eq!(shows("-1.5"), "(- 1.5)");
    assert_eq!(shows("a - -5"), "(- ?a (- 5))");
    // A sign that touches its digits after an operator belongs to the literal.
    assert_eq!(shows("--5"), "(- -5)");
}

#[test]
fn lower_expr_when_group_then_the_grouped_expression_spanning_the_parentheses() {
    for source in ["(a)", "((a))", "( a )", "(a + b)", "(NOT c)"] {
        let expr = lower(source);
        assert_eq!(written(source, &expr), source, "{source}");
    }
    assert_eq!(shows("(a)"), "?a");
    assert_eq!(shows("((a + b))"), "(+ ?a ?b)");
    // The inner expression keeps the range it was written at.
    let source = "(a + b) * c";
    let expr = lower(source);
    if let ExprKind::BinaryOp(op) = &expr.kind {
        assert_eq!(written(source, &op.left), "(a + b)");
        assert_eq!(written(source, &op.right), "c");
    } else {
        unreachable!("not a product: {expr:?}");
    }
}

#[test]
fn lower_expr_when_name_alone_then_late_bound_at_its_token() {
    let expr = lower("  speed ");
    match &expr.kind {
        ExprKind::LateBound(late) => {
            assert_eq!(late.value.original(), "speed");
            assert_eq!(
                (late.value.span.start, late.value.span.end),
                (2, 7),
                "positioned at the token"
            );
            assert_eq!(late.value.span.file_id, file());
        }
        other => unreachable!("not late-bound: {other:?}"),
    }
}

#[test]
fn lower_expr_when_name_with_something_after_it_then_a_variable() {
    let rows = [
        ("a.b", "$a.b"),
        ("a[1]", "$a[1]"),
        ("a[1, 2]", "$a[1, 2]"),
        ("a^", "(^ $a)"),
        ("a^^", "(^ (^ $a))"),
        ("a^.b", "$a^.b"),
        ("a^[1]", "$a^[1]"),
        ("a[1]^", "(^ $a[1])"),
        ("a.b^", "(^ $a.b)"),
        ("f(1)^", "(^ f(1))"),
        ("(a)^", "(^ ?a)"),
        ("THIS^", "$THIS^"),
        ("SUPER^.x", "$SUPER^.x"),
    ];
    for (source, expected) in rows {
        assert_eq!(shows(source), expected, "{source}");
    }
}

#[test]
fn lower_expr_when_name_that_the_legacy_grammar_treats_apart_then_late_bound_like_any_name() {
    for name in ["STEP", "ON", "R_EDGE", "F_EDGE"] {
        let expr = lower(name);
        match &expr.kind {
            ExprKind::LateBound(late) => {
                assert_eq!(late.value.original(), name);
                assert_eq!(
                    (late.value.span.start, late.value.span.end),
                    (0, name.len()),
                    "{name} is positioned"
                );
            }
            other => unreachable!("{name} is not late-bound: {other:?}"),
        }
    }
}

#[test]
fn lower_expr_when_special_operator_word_alone_then_a_variable_not_a_late_bound_name() {
    assert_eq!(shows("__CURRENTTASK"), "$__CURRENTTASK");
    assert_eq!(shows("__CURRENTTASK^.Index"), "$__CURRENTTASK^.Index");
    assert_eq!(shows("__currenttask"), "$__currenttask");
}

#[test]
fn lower_expr_when_constant_then_a_constant_spanning_the_literal() {
    let source = "T#5s + 1";
    let expr = lower(source);
    if let ExprKind::BinaryOp(op) = &expr.kind {
        assert!(matches!(op.left.kind, ExprKind::Const(_)));
        assert_eq!(written(source, &op.left), "T#5s");
        assert_eq!(written(source, &op.right), "1");
    } else {
        unreachable!("not a sum: {expr:?}");
    }
}

#[test]
fn lower_expr_when_expression_then_its_span_is_the_text_it_was_written_as() {
    let sources = [
        "a",
        "42",
        "undeclared",
        "-a",
        "-42",
        "NOT c",
        "a + b",
        "a < b",
        "(a + b)",
        "(NOT c)",
        "(a + b) * 2",
        "a + -b",
        "a ** b ** c",
        "p^",
        "-p^",
        "p^^",
        "REF(a)",
        "MAX(a, b)",
        "-MAX(a, b)",
        "NULL",
        "a[1]",
        "a[i + 1].b",
        "a.b.c",
        "a.1",
        "a.%X3",
        "a.%W1",
        "a[1]^",
        "THIS^",
        "THIS^.m(1)",
        "m.f(a)",
        "m.f(a)^",
        "__NEW(INT)",
        "__NEW(INT, 5)",
        "%IX0.1",
        "f(a := 1, b => x)",
        "T#1m30s",
    ];
    for source in sources {
        let expr = lower(source);
        assert_eq!(written(source, &expr), source, "{source}");
    }
}

#[test]
fn lower_expr_when_operand_is_unary_or_a_call_argument_then_its_own_span_includes_the_operator() {
    let source = "a + NOT c";
    if let ExprKind::BinaryOp(op) = lower(source).kind {
        assert_eq!(written(source, &op.right), "NOT c");
    } else {
        unreachable!("not a sum");
    }
    let source = "MAX(a, -b)";
    let expr = lower(source);
    let argument = function(&expr)
        .and_then(|function| function.param_assignment.get(1))
        .and_then(|param| param.input_expr());
    assert_eq!(argument.map(|arg| written(source, arg)), Some("-b"));
}

#[test]
fn lower_expr_when_subscripts_then_each_is_an_expression_of_its_own() {
    let source = "a[i + 1, 2]";
    let expr = lower(source);
    let ExprKind::Variable(Variable::Symbolic(SymbolicVariableKind::Array(array))) = &expr.kind
    else {
        unreachable!("not a subscript: {expr:?}");
    };
    let shown: Vec<String> = array.subscripts.iter().map(show).collect();
    assert_eq!(shown, vec!["(+ ?i 1)", "2"]);
    assert_eq!(written(source, &array.subscripts[0]), "i + 1");
    assert_eq!(written(source, &array.subscripts[1]), "2");
}

#[test]
fn lower_expr_when_call_then_function_with_its_arguments_in_order() {
    let expr = lower("f(1, a := 2, b => x, NOT c => y)");
    let call = function(&expr).expect("a function");
    assert_eq!(call.name.original(), "f");
    assert_eq!(call.param_assignment.len(), 4);
    assert!(matches!(
        &call.param_assignment[0],
        ParamAssignmentKind::PositionalInput(_)
    ));
    match &call.param_assignment[1] {
        ParamAssignmentKind::NamedInput(named) => {
            assert_eq!(named.name.original(), "a");
            assert_eq!(show(&named.expr), "2");
        }
        other => unreachable!("not named: {other:?}"),
    }
    match &call.param_assignment[2] {
        ParamAssignmentKind::Output(output) => {
            assert!(!output.not);
            assert_eq!(output.src.original(), "b");
            assert_eq!(output.tgt.to_string(), "x");
        }
        other => unreachable!("not an output: {other:?}"),
    }
    match &call.param_assignment[3] {
        ParamAssignmentKind::Output(output) => {
            assert!(output.not, "the NOT of an output binding is kept");
            assert_eq!(output.src.original(), "c");
        }
        other => unreachable!("not an output: {other:?}"),
    }
}

#[test]
fn lower_expr_when_call_without_arguments_then_function_with_none() {
    let expr = lower("f()");
    assert_eq!(function(&expr).map(|f| f.param_assignment.len()), Some(0));
    assert_eq!(shows("f()"), "f()");
}

#[test]
fn lower_expr_when_call_nested_in_call_then_the_inner_is_an_argument() {
    assert_eq!(shows("f(g(1), 2)"), "f(g(1), 2)");
    assert_eq!(shows("f(a, b + 1)"), "f(?a, (+ ?b 1))");
}

#[test]
fn lower_expr_when_operator_word_is_the_function_then_its_name_is_the_word() {
    for (source, name) in [
        ("MOD(a, b)", "MOD"),
        ("AND(a, b)", "AND"),
        ("OR(a)", "OR"),
        ("XOR(a, b, c)", "XOR"),
        ("NOT(a, b)", "NOT"),
        ("__DELETE(p)", "__DELETE"),
        ("__ISVALIDREF(p)", "__ISVALIDREF"),
        ("__XADD(a, b)", "__XADD"),
    ] {
        let expr = lower(source);
        let call = function(&expr).unwrap_or_else(|| unreachable!("{source} is not a function"));
        assert_eq!(call.name.original(), name, "{source}");
        assert_eq!(
            (call.name.span.start, call.name.span.end),
            (0, name.len()),
            "{source}: the name is positioned"
        );
    }
}

#[test]
fn lower_expr_when_method_call_then_instance_method_and_arguments() {
    let source = "m.Scaled(2, offset := 1)";
    let expr = lower(source);
    let call = method(&expr).expect("a method call");
    assert_eq!(call.receiver, MethodReceiver::Instance(Id::from("m")));
    assert_eq!(call.method.original(), "Scaled");
    assert_eq!(call.params.len(), 2);
    assert_eq!(written(source, &expr), source);
    assert_eq!(
        (call.position.start, call.position.end),
        (0, source.len()),
        "the call is positioned from the receiver to the parenthesis"
    );
    assert_eq!((call.method.span.start, call.method.span.end), (2, 8));
}

#[test]
fn lower_expr_when_self_reference_receives_the_call_then_the_receiver_is_this_or_super() {
    for (source, kind) in [
        ("THIS^.GetSpeed()", SelfRefKind::This),
        ("SUPER^.GetSpeed()", SelfRefKind::Super),
        ("this^.GetSpeed()", SelfRefKind::This),
    ] {
        let expr = lower(source);
        let call = method(&expr).unwrap_or_else(|| unreachable!("{source} is not a method call"));
        match &call.receiver {
            MethodReceiver::SelfRef(self_ref) => {
                assert_eq!(self_ref.kind, kind, "{source}");
                let end = source.find('^').map(|caret| caret + 1);
                assert_eq!(Some(self_ref.position.end), end, "{source}");
                assert_eq!(self_ref.position.start, 0);
            }
            other => unreachable!("{source}: not a self reference: {other:?}"),
        }
    }
}

#[test]
fn lower_expr_when_method_call_is_an_operand_or_an_argument_then_nested() {
    assert_eq!(shows("1.0 + m.Value()"), "(+ 1 m.Value())");
    let expr = lower("m.Scaled(m.Factor())");
    let outer = method(&expr).expect("a method call");
    let inner = outer.params[0].input_expr().and_then(method);
    assert_eq!(
        inner.map(|call| call.method.original().as_str()),
        Some("Factor")
    );
}

#[test]
fn lower_expr_when_reference_then_the_variable_it_refers_to() {
    assert_eq!(shows("REF(a)"), "(REF $a)");
    assert_eq!(shows("REF(a.b[1])"), "(REF $a.b[1])");
}

#[test]
fn lower_expr_when_null_then_null_spanning_the_word() {
    let expr = lower("NULL");
    match &expr.kind {
        ExprKind::Null(span) => assert_eq!((span.start, span.end), (0, 4)),
        other => unreachable!("not NULL: {other:?}"),
    }
    assert_eq!(shows("NULL"), "NULL");
}

#[test]
fn lower_expr_when_special_type_operator_then_a_call_with_the_type_as_a_variable() {
    let source = "__NEW(MyFb, 5)";
    let expr = lower(source);
    let call = function(&expr).expect("a call");
    assert_eq!(call.name.original(), "__NEW");
    assert_eq!((call.name.span.start, call.name.span.end), (0, 5));
    assert_eq!(call.param_assignment.len(), 2);
    let type_argument = call.param_assignment[0].input_expr();
    assert_eq!(type_argument.map(show), Some("$MyFb".to_string()));
    let count = call.param_assignment[1].input_expr();
    assert_eq!(count.map(show), Some("5".to_string()));
    assert_eq!(written(source, &expr), source);

    assert_eq!(shows("__TYPEOF(a)"), "__TYPEOF($a)");
    assert_eq!(shows("__NEW(INT)"), "__NEW($INT)");
}

#[test]
fn lower_expr_when_special_operator_names_a_keyword_type_then_the_canonical_name_at_the_keyword() {
    let expr = lower("__new(tod)");
    let call = function(&expr).expect("a call");
    assert_eq!(call.name.original(), "__new", "the operator as written");
    let Some(ExprKind::Variable(Variable::Symbolic(SymbolicVariableKind::Named(named)))) = call
        .param_assignment[0]
        .input_expr()
        .map(|e| e.kind.clone())
    else {
        unreachable!("the type is not a named variable");
    };
    assert_eq!(named.name.original(), "TIME_OF_DAY");
    assert_eq!((named.name.span.start, named.name.span.end), (6, 9));
}

#[test]
fn lower_call_when_function_then_callee_name_arguments_and_span() {
    let node = expression_node("f(1, a := 2)").expect("a node");
    let call = lower_call(&LowerCx::new(file()), &node).expect("a call");
    match &call.callee {
        Callee::Name(name) => assert_eq!(name.original(), "f"),
        other => unreachable!("not a name: {other:?}"),
    }
    assert_eq!(call.params.len(), 2);
    assert_eq!((call.span.start, call.span.end), (0, 12));
}

#[test]
fn lower_call_when_method_then_callee_receiver_and_method() {
    let node = expression_node("m.run(1)").expect("a node");
    let call = lower_call(&LowerCx::new(file()), &node).expect("a call");
    match &call.callee {
        Callee::Method { receiver, method } => {
            assert_eq!(receiver, &MethodReceiver::Instance(Id::from("m")));
            assert_eq!(method.original(), "run");
        }
        other => unreachable!("not a method: {other:?}"),
    }
}

#[test]
fn lower_call_when_node_is_not_a_call_then_internal_error() {
    let node = expression_node("a + b").expect("a node");
    let diagnostic = lower_call(&LowerCx::new(file()), &node).err();
    assert_eq!(
        diagnostic.map(|diagnostic| diagnostic.code),
        Some(INTERNAL_ERROR.to_string())
    );
}

#[test]
fn lower_arguments_when_node_is_not_an_argument_list_then_internal_error() {
    let node = expression_node("a + b").expect("a node");
    let diagnostic = lower_arguments(&LowerCx::new(file()), &node).err();
    assert_eq!(
        diagnostic.map(|diagnostic| diagnostic.code),
        Some(INTERNAL_ERROR.to_string())
    );
}

#[test]
fn lower_expr_when_every_operator_and_form_then_no_node_is_left_without_a_rule() {
    // A kind that the disposition table says is lowered as an expression or a
    // variable has a rule here.
    for kind in SyntaxKind::ALL {
        if matches!(
            disposition(*kind),
            Disposition::Lowered(Area::Expression | Area::Variable | Area::Literal)
        ) {
            assert!(rule(*kind).is_some(), "{kind:?} has no expression rule");
        }
    }
}

#[test]
fn lower_expr_when_operators_table_then_every_row_names_an_operator_token() {
    for (kind, _) in OPERATIONS {
        let spelled = SyntaxKind::KEYWORDS
            .iter()
            .any(|(_, keyword)| keyword == kind)
            || matches!(
                kind,
                SyntaxKind::Equal
                    | SyntaxKind::NotEqual
                    | SyntaxKind::Less
                    | SyntaxKind::Greater
                    | SyntaxKind::LessEqual
                    | SyntaxKind::GreaterEqual
                    | SyntaxKind::Plus
                    | SyntaxKind::Minus
                    | SyntaxKind::Star
                    | SyntaxKind::Div
                    | SyntaxKind::Power
            );
        assert!(spelled, "{kind:?}");
    }
    assert_eq!(OPERATIONS.len(), 17);
}

// Stack safety: a chain the tree depth limit allows is lowered, and dropped,
// on a stack that holds a few hundred frames. The limit is what keeps a chain
// of any length in the text from reaching the rules; the rules fold a chain
// without recursing along it, so it costs the stack nothing.

/// Runs `body` on a thread with the smallest stack the compiler runs on: the
/// 1 MiB of the Windows main thread and of WebAssembly.
fn on_small_stack<T: Send + 'static>(body: impl FnOnce() -> T + Send + 'static) -> Option<T> {
    std::thread::Builder::new()
        .stack_size(1024 * 1024)
        .spawn(body)
        .ok()?
        .join()
        .ok()
}

/// The number of operations along the left spine of `expr`.
fn left_depth(expr: &Expr) -> usize {
    let mut depth = 0;
    let mut current = expr;
    while let ExprKind::BinaryOp(op) = &current.kind {
        depth += 1;
        current = &op.left;
    }
    depth
}

/// The terms of the longest sum the tree depth limit allows: the root, one
/// node for each operator and the operand at the bottom make the depth.
const LONGEST_SUM: usize = MAX_DEPTH - 1;

#[test]
fn lower_expr_when_sum_is_as_long_as_the_tree_allows_then_lowered_without_recursion() {
    let depth = on_small_stack(|| {
        let source = vec!["a"; LONGEST_SUM].join("+");
        let parse = parse_expression(&source, &all());
        assert!(parse.is_ok(), "{:?}", parse.errors);
        let node = parse.root.first_child().expect("a node");
        let expr = lower_expr(&LowerCx::new(file()), &node).expect("lowers");
        left_depth(&expr)
    });
    assert_eq!(depth, Some(LONGEST_SUM - 1));
}

#[test]
fn lower_expr_when_sum_is_one_term_too_long_then_the_parse_reports_the_depth_not_the_lowering() {
    let source = vec!["a"; LONGEST_SUM + 1].join("+");
    let parse = parse_expression(&source, &all());
    assert_eq!(
        parse
            .errors
            .iter()
            .map(|error| error.kind)
            .collect::<Vec<_>>(),
        vec![crate::ErrorKind::NestingTooDeep]
    );
}

#[test]
fn lower_expr_when_carets_are_as_many_as_the_tree_allows_then_lowered_without_recursion() {
    let depth = on_small_stack(|| {
        let source = format!("a{}", "^".repeat(MAX_DEPTH - 2));
        let parse = parse_expression(&source, &all());
        assert!(parse.is_ok(), "{:?}", parse.errors);
        let node = parse.root.first_child().expect("a node");
        let expr = lower_expr(&LowerCx::new(file()), &node).expect("lowers");
        let mut depth = 0;
        let mut current = &expr;
        while let ExprKind::Deref(inner) = &current.kind {
            depth += 1;
            current = inner;
        }
        depth
    });
    assert_eq!(depth, Some(MAX_DEPTH - 2));
}

/// The stack the nested expressions below are lowered, walked and dropped on.
/// Nested calls at the limit need 1.7 MiB in a debug build, the most of any
/// construct, and 0.6 MiB in a release build; one level costs 7 KB and 2.3 KB.
const NESTING_STACK: usize = 4 * 1024 * 1024;

/// How deep `build` nests before the parse reports the depth.
fn deepest_allowed(build: fn(usize) -> String) -> usize {
    (1..)
        .find(|n| !parse_expression(&build(*n), &all()).is_ok())
        .map_or(0, |first_too_deep| first_too_deep - 1)
}

#[test]
fn lower_expr_when_nesting_is_as_deep_as_the_tree_allows_then_lowered_walked_and_dropped() {
    use ironplc_dsl::visitor::Visitor;

    struct Walker;
    impl Visitor<std::convert::Infallible> for Walker {
        type Value = ();
    }

    let nestings: [fn(usize) -> String; 2] = [
        |n| format!("{}1{}", "(".repeat(n), ")".repeat(n)),
        |n| format!("{}1{}", "f(".repeat(n), ")".repeat(n)),
    ];
    for build in nestings {
        let walked = std::thread::Builder::new()
            .stack_size(NESTING_STACK)
            .spawn(move || {
                let source = build(deepest_allowed(build));
                let parse = parse_expression(&source, &all());
                assert!(parse.is_ok(), "{:?}", parse.errors);
                let node = parse.root.first_child().expect("a node");
                let expr = lower_expr(&LowerCx::new(file()), &node).expect("lowers");
                Walker.visit_expr(&expr).is_ok()
            })
            .ok()
            .and_then(|thread| thread.join().ok());
        assert_eq!(walked, Some(true));
    }
}

#[test]
fn lower_expr_when_long_chain_of_one_level_then_the_spans_nest() {
    let source = vec!["a"; 200].join(" - ");
    let expr = lower(&source);
    assert_eq!(left_depth(&expr), 199);
    let mut current = &expr;
    while let ExprKind::BinaryOp(op) = &current.kind {
        assert_eq!(
            written(&source, current).len(),
            current.span().end - current.span().start
        );
        assert_eq!(current.span().start, 0);
        current = &op.left;
    }
    assert_eq!(written(&source, current), "a");
}

// Errors.

/// A node of `kind` holding `children` as the tree would, built by hand: the
/// parser never builds a tree that lacks what its kind requires, so a rule's
/// answer to one is only reachable this way.
fn built(kind: SyntaxKind, children: Vec<NodeOrToken<GreenNode, rowan::GreenToken>>) -> SyntaxNode {
    SyntaxNode::new_root(GreenNode::new(rowan::SyntaxKind(kind as u16), children))
}

fn empty(kind: SyntaxKind) -> SyntaxNode {
    built(kind, vec![])
}

fn internal(node: &SyntaxNode) -> Option<String> {
    lower_expr(&LowerCx::new(file()), node)
        .err()
        .map(|diagnostic| diagnostic.code)
}

#[test]
fn lower_expr_when_node_lacks_what_its_kind_requires_then_internal_error() {
    for kind in [
        SyntaxKind::BinaryExpr,
        SyntaxKind::UnaryExpr,
        SyntaxKind::ParenExpr,
        SyntaxKind::NameRef,
        SyntaxKind::DerefExpr,
        SyntaxKind::CallExpr,
        SyntaxKind::RefExpr,
        SyntaxKind::SpecialOpExpr,
        SyntaxKind::FieldExpr,
        SyntaxKind::IndexExpr,
        SyntaxKind::BitAccessExpr,
        SyntaxKind::PartialAccessExpr,
        SyntaxKind::SelfRefExpr,
        SyntaxKind::DirectAddressExpr,
    ] {
        assert_eq!(
            internal(&empty(kind)),
            Some(INTERNAL_ERROR.to_string()),
            "{kind:?}"
        );
    }
}

#[test]
fn lower_expr_when_call_has_no_argument_list_then_internal_error() {
    let parse = parse_expression("f(1)", &all());
    let call = parse.root.first_child().expect("a call");
    let callee = call.first_child().expect("a callee");
    let green = GreenNode::new(
        rowan::SyntaxKind(SyntaxKind::CallExpr as u16),
        vec![NodeOrToken::Node(callee.green().into_owned())],
    );
    assert_eq!(
        internal(&SyntaxNode::new_root(green)),
        Some(INTERNAL_ERROR.to_string())
    );
}

#[test]
fn lower_expr_when_node_is_not_an_expression_then_internal_error_or_not_implemented() {
    let statement = parse_statements("x := 1;", &all());
    let assignment = statement
        .root
        .descendants()
        .find(|node| node.kind() == SyntaxKind::AssignStmt)
        .expect("an assignment");
    assert_eq!(internal(&assignment), Some(NOT_IMPLEMENTED.to_string()));
    let name = built(SyntaxKind::Name, vec![]);
    assert_eq!(internal(&name), Some(INTERNAL_ERROR.to_string()));
}

#[test]
fn lower_expr_when_literal_does_not_denote_a_value_then_syntax_error_from_the_literal() {
    let too_large = "99999999999999999999999999999999999999999";
    assert_eq!(
        code(lower_result(&format!("a + {too_large}"))),
        Some(Problem::SyntaxError.code().to_string())
    );
    assert_eq!(
        code(lower_result(&format!("f({too_large})"))),
        Some(Problem::SyntaxError.code().to_string())
    );
    assert_eq!(
        code(lower_result(&format!("a[{too_large}]"))),
        Some(Problem::SyntaxError.code().to_string())
    );
}

#[test]
fn lower_expr_when_error_is_inside_an_operand_then_the_whole_expression_fails_with_it() {
    let diagnostic = lower_result("a + (b * 99999999999999999999999999999999999999999)").err();
    assert_eq!(
        diagnostic.as_ref().map(|d| d.code.as_str()),
        Some(Problem::SyntaxError.code())
    );
    let range = diagnostic.map(|d| (d.primary.location.start, d.primary.location.end));
    assert_eq!(range, Some((9, 9 + 41)), "over the literal alone");
}
