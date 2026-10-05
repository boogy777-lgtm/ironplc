use super::*;
use crate::lower::{disposition, Area, Disposition, INTERNAL_ERROR};
use crate::{parse_source_file, parse_statements, ErrorKind, ParseOptions, SyntaxKind, MAX_DEPTH};
use ironplc_dsl::common::{BitStringLiteral, SignedIntegerRef};
use ironplc_dsl::core::{FileId, Located};
use ironplc_dsl::stack::STACK_BUDGET;
use ironplc_dsl::textual::{
    ArrayVariable, Assignment, Case, CaseSelectionKind, DerefVariable, ExprKind,
    ParamAssignmentKind, StructuredVariable, SymbolicVariableKind, TryCatch, Variable,
};
use ironplc_problems::Problem;
use rowan::{GreenNode, NodeOrToken};

fn file() -> FileId {
    FileId::from_string("t.st")
}

/// Every flag on, so that the dialect statements (`JMP`, `__TRY`, labels) are
/// statements of the language.
fn all() -> ParseOptions {
    ParseOptions::all()
}

fn lower_with(source: &str, options: &ParseOptions) -> Result<Vec<StmtKind>, Diagnostic> {
    let parse = parse_statements(source, options);
    assert!(parse.is_ok(), "{source}: {:?}", parse.errors);
    crate::lower::lower_statements(&parse, &file())
}

fn lower_result(source: &str) -> Result<Vec<StmtKind>, Diagnostic> {
    lower_with(source, &all())
}

fn lower(source: &str) -> Vec<StmtKind> {
    lower_result(source).expect("the statements lower")
}

/// The one statement of `source`.
fn one(source: &str) -> StmtKind {
    let mut statements = lower(source);
    assert_eq!(statements.len(), 1, "{source}");
    statements.remove(0)
}

fn assignment_of(source: &str) -> Assignment {
    match one(source) {
        StmtKind::Assignment(assignment) => Some(assignment),
        _ => None,
    }
    .expect("an assignment")
}

fn case_of(source: &str) -> Case {
    match one(source) {
        StmtKind::Case(case) => Some(case),
        _ => None,
    }
    .expect("a CASE")
}

fn try_of(source: &str) -> TryCatch {
    match one(source) {
        StmtKind::TryCatch(statement) => Some(statement),
        _ => None,
    }
    .expect("a __TRY")
}

fn array_of(target: &Variable) -> &ArrayVariable {
    match target {
        Variable::Symbolic(SymbolicVariableKind::Array(array)) => Some(array),
        _ => None,
    }
    .expect("an array access")
}

fn structured_of(target: &Variable) -> &StructuredVariable {
    match target {
        Variable::Symbolic(SymbolicVariableKind::Structured(field)) => Some(field),
        _ => None,
    }
    .expect("a member")
}

fn deref_of(variable: &SymbolicVariableKind) -> &DerefVariable {
    match variable {
        SymbolicVariableKind::Deref(deref) => Some(deref),
        _ => None,
    }
    .expect("a dereference")
}

fn bits_of(selector: &CaseSelectionKind) -> &BitStringLiteral {
    match selector {
        CaseSelectionKind::BitStringLiteral(bits) => Some(bits),
        _ => None,
    }
    .expect("a bit string")
}

fn code<T>(result: Result<T, Diagnostic>) -> Option<String> {
    result.err().map(|diagnostic| diagnostic.code)
}

/// An expression on one line, as far as the tests tell expressions apart: a
/// late-bound name as `?name`, a variable as `$variable`, an operation as
/// `(operator left right)`, a call as `name(...)`.
fn show_expr(expr: &Expr) -> String {
    match &expr.kind {
        ExprKind::Compare(op) => format!(
            "({} {} {})",
            op.op,
            show_expr(&op.left),
            show_expr(&op.right)
        ),
        ExprKind::BinaryOp(op) => format!(
            "({} {} {})",
            op.op,
            show_expr(&op.left),
            show_expr(&op.right)
        ),
        ExprKind::UnaryOp(op) => format!("({} {})", op.op, show_expr(&op.term)),
        ExprKind::Const(constant) => constant.to_string(),
        ExprKind::Variable(variable) => format!("${variable}"),
        ExprKind::LateBound(late) => format!("?{late}"),
        ExprKind::Ref(variable) => format!("(REF ${variable})"),
        ExprKind::Function(function) => format!("{}(..)", function.name),
        _ => "..".to_string(),
    }
}

fn show_params(params: &[ParamAssignmentKind]) -> String {
    params
        .iter()
        .map(|param| match param {
            ParamAssignmentKind::PositionalInput(input) => show_expr(&input.expr),
            ParamAssignmentKind::NamedInput(input) => {
                format!("{} := {}", input.name, show_expr(&input.expr))
            }
            ParamAssignmentKind::Output(output) => format!(
                "{}{} => ${}",
                if output.not { "NOT " } else { "" },
                output.src,
                output.tgt
            ),
        })
        .collect::<Vec<_>>()
        .join(", ")
}

fn show_selector(selector: &CaseSelectionKind) -> String {
    match selector {
        CaseSelectionKind::Subrange(range) => {
            let bound = |bound: &SignedIntegerRef| match bound {
                SignedIntegerRef::Literal(number) => {
                    format!(
                        "{}{}",
                        if number.is_neg { "-" } else { "+" },
                        number.value.value
                    )
                }
                SignedIntegerRef::Constant(name) => name.to_string(),
            };
            format!("{}..{}", bound(&range.start), bound(&range.end))
        }
        CaseSelectionKind::SignedInteger(number) => {
            format!(
                "{}{}",
                if number.is_neg { "-" } else { "+" },
                number.value.value
            )
        }
        CaseSelectionKind::EnumeratedValue(value) => match &value.type_name {
            Some(type_name) => format!("{}#{}", type_name.name, value.value),
            None => value.value.to_string(),
        },
        CaseSelectionKind::BitStringLiteral(bits) => {
            assert!(bits.data_type.is_none(), "a label has no type");
            format!("bits {}", bits.value.value)
        }
    }
}

/// A list of statements on one line: a statement as `(kind ...)`, a body as
/// `[statements]`.
fn show_all(statements: &[StmtKind]) -> String {
    format!(
        "[{}]",
        statements.iter().map(show).collect::<Vec<_>>().join(" ")
    )
}

fn show(statement: &StmtKind) -> String {
    match statement {
        StmtKind::Assignment(assignment) => {
            let operator = match (
                assignment.ref_bind,
                assignment.set_bind,
                assignment.reset_bind,
            ) {
                (true, _, _) => "REF=",
                (_, true, _) => "S=",
                (_, _, true) => "R=",
                _ => ":=",
            };
            format!(
                "({operator} ${}{} {})",
                assignment.target,
                if assignment.deref { "^" } else { "" },
                show_expr(&assignment.value)
            )
        }
        StmtKind::FbCall(call) => {
            format!("(call {}({}))", call.callee, show_params(&call.params))
        }
        StmtKind::MethodCall(call) => format!(
            "(call {}.{}({}))",
            call.receiver,
            call.method,
            show_params(&call.params)
        ),
        StmtKind::If(node) => {
            let mut text = format!("(IF {} {}", show_expr(&node.expr), show_all(&node.body));
            for clause in &node.else_ifs {
                text += &format!(
                    " ELSIF {} {}",
                    show_expr(&clause.expr),
                    show_all(&clause.body)
                );
            }
            if !node.else_body.is_empty() {
                text += &format!(" ELSE {}", show_all(&node.else_body));
            }
            text + ")"
        }
        StmtKind::Case(node) => {
            let mut text = format!("(CASE {}", show_expr(&node.selector));
            for group in &node.statement_groups {
                let selectors: Vec<String> = group.selectors.iter().map(show_selector).collect();
                text += &format!(
                    " ({}: {})",
                    selectors.join(", "),
                    show_all(&group.statements)
                );
            }
            if !node.else_body.is_empty() {
                text += &format!(" ELSE {}", show_all(&node.else_body));
            }
            text + ")"
        }
        StmtKind::For(node) => format!(
            "(FOR {} {} {}{} {})",
            node.control,
            show_expr(&node.from),
            show_expr(&node.to),
            node.step
                .as_ref()
                .map(|step| format!(" BY {}", show_expr(step)))
                .unwrap_or_default(),
            show_all(&node.body)
        ),
        StmtKind::While(node) => format!(
            "(WHILE {} {})",
            show_expr(&node.condition),
            show_all(&node.body)
        ),
        StmtKind::Repeat(node) => format!(
            "(REPEAT {} UNTIL {})",
            show_all(&node.body),
            show_expr(&node.until)
        ),
        StmtKind::Return => "RETURN".to_string(),
        StmtKind::Exit(_) => "EXIT".to_string(),
        StmtKind::Continue(_) => "CONTINUE".to_string(),
        StmtKind::BeginImplementation(_) => "BEGIN".to_string(),
        StmtKind::TryCatch(node) => {
            let mut text = format!("(TRY {}", show_all(&node.body));
            if let Some(catch) = &node.catch {
                text += &format!(
                    " CATCH {} {}",
                    catch
                        .exception
                        .as_ref()
                        .map(|variable| format!("${variable}"))
                        .unwrap_or_else(|| "-".to_string()),
                    show_all(&catch.body)
                );
            }
            if !node.finally_body.is_empty() {
                text += &format!(" FINALLY {}", show_all(&node.finally_body));
            }
            text + ")"
        }
        StmtKind::Throw(node) => format!(
            "(THROW{})",
            node.value
                .as_ref()
                .map(|value| format!(" {}", show_expr(value)))
                .unwrap_or_default()
        ),
        StmtKind::Jump(node) => format!(
            "(JMP{} {})",
            node.condition
                .as_ref()
                .map(|condition| format!(" IF {}", show_expr(condition)))
                .unwrap_or_default(),
            node.label
        ),
        StmtKind::Label(node) => format!("(LABEL {})", node.name),
        StmtKind::ConditionalCall(node) => format!(
            "(CALC {} {}({}))",
            show_expr(&node.condition),
            node.call.callee,
            show_params(&node.call.params)
        ),
        StmtKind::Wait(node) => format!(
            "(WAIT{})",
            node.condition
                .as_ref()
                .map(|condition| format!(" {}", show_expr(condition)))
                .unwrap_or_default()
        ),
    }
}

fn shows(source: &str) -> String {
    show_all(&lower(source))
}

/// The text of `source` that `span` covers.
fn at<'a>(source: &'a str, span: &ironplc_dsl::core::SourceSpan) -> &'a str {
    &source[span.start..span.end]
}

mod forms;
mod lists;
