//! Expressions: the node kinds that compute a value to [`Expr`].
//!
//! `lower_expr` is the one dispatch: the kind of the node selects a rule from
//! `rule`, the rule builds the [`ExprKind`], and `lower_expr` records the
//! node's own range as the span of the expression. The tree holds every token,
//! so that range is where the expression was written, parentheses, operators
//! and carets included.
//!
//! The decisions the legacy grammar made by ordered choice are rows of data:
//!
//! - which operator a token is, and whether it builds a comparison or an
//!   arithmetic operation, is `OPERATIONS`; the unary operators are
//!   `UNARY_OPERATORS`;
//! - a name standing alone in an expression is a late-bound name: the grammar
//!   decides later, by the declarations in scope, whether it is a variable, an
//!   enumerated value or a constant. A name with anything after it (a member,
//!   a subscript, a call) is a variable, and so is the name a `^` follows;
//! - a call is a function, or a method of an instance (`Call`); the statement
//!   form of a call reads the same node;
//! - the arguments of a call are the rows of `ARGUMENTS`: positional, named or
//!   output.
//!
//! A chain of left-associative operators, and a chain of `^`, is as deep as it
//! is long, so both are folded from their innermost node outward
//! (`left_spine`) and cost the stack nothing along their length. Recursion
//! follows only nesting, and the tree is no deeper than [`crate::MAX_DEPTH`].

use super::literals::lower_constant;
use super::names::{lower_id, lower_name, lower_type_ref};
use super::tree::{left_spine, significant_tokens, token_of};
use super::variables::{lower_self_ref, lower_variable};
use super::{Area, Disposition, LowerCx};
use crate::parser::is_special_operator;
use crate::syntax_kind::{SyntaxKind as K, SyntaxNode};
use ironplc_dsl::construct::special_operator_type_call;
use ironplc_dsl::core::{Id, SourceSpan};
use ironplc_dsl::diagnostic::Diagnostic;
use ironplc_dsl::textual::{
    CompareOp, Expr, ExprKind, Function, LateBound, MethodCall, MethodReceiver, NamedInput,
    Operator, Output, ParamAssignmentKind, PositionalInput, UnaryOp,
};

#[cfg(test)]
mod tests;

/// What a binary operator builds: a comparison or an arithmetic operation.
enum Operation {
    Compare(CompareOp),
    Arithmetic(Operator),
}

impl Operation {
    /// The expression of this operation over two operands.
    fn apply(&self, left: Expr, right: Expr) -> Expr {
        match self {
            Operation::Compare(op) => Expr::compare(op.clone(), left, right),
            Operation::Arithmetic(op) => Expr::binary(op.clone(), left, right),
        }
    }
}

/// The binary operators: the token of each and what it builds. The logical
/// operators are comparisons in the objects, as the legacy grammar builds them.
const OPERATIONS: &[(K, Operation)] = &[
    (K::Or, Operation::Compare(CompareOp::Or)),
    (K::OrElse, Operation::Compare(CompareOp::OrElse)),
    (K::Xor, Operation::Compare(CompareOp::Xor)),
    (K::And, Operation::Compare(CompareOp::And)),
    (K::AndThen, Operation::Compare(CompareOp::AndThen)),
    (K::Equal, Operation::Compare(CompareOp::Eq)),
    (K::NotEqual, Operation::Compare(CompareOp::Ne)),
    (K::Less, Operation::Compare(CompareOp::Lt)),
    (K::Greater, Operation::Compare(CompareOp::Gt)),
    (K::LessEqual, Operation::Compare(CompareOp::LtEq)),
    (K::GreaterEqual, Operation::Compare(CompareOp::GtEq)),
    (K::Plus, Operation::Arithmetic(Operator::Add)),
    (K::Minus, Operation::Arithmetic(Operator::Sub)),
    (K::Star, Operation::Arithmetic(Operator::Mul)),
    (K::Div, Operation::Arithmetic(Operator::Div)),
    (K::Mod, Operation::Arithmetic(Operator::Mod)),
    (K::Power, Operation::Arithmetic(Operator::Pow)),
];

/// The unary operators: the token of each and the operation.
const UNARY_OPERATORS: &[(K, UnaryOp)] = &[(K::Minus, UnaryOp::Neg), (K::Not, UnaryOp::Not)];

/// A rule: builds the expression a node denotes, leaving its span to the
/// caller.
type Rule = fn(&LowerCx, &SyntaxNode) -> Result<ExprKind, Diagnostic>;

/// The rule for each kind of node that is an expression. A kind that the
/// disposition table gives to the literal area is a constant, and any other
/// kind it gives to the variable area (a name alone and a dereference are
/// decided first) is a place.
fn rule(kind: K) -> Option<Rule> {
    Some(match kind {
        K::BinaryExpr => binary,
        K::UnaryExpr => unary,
        K::ParenExpr => group,
        K::NameRef => late_bound,
        K::DerefExpr => dereference,
        K::CallExpr => call_expression,
        K::RefExpr => reference,
        K::NullLiteral => null,
        K::SpecialOpExpr => special_operator,
        kind if is_lowered_by(kind, Area::Literal) => constant,
        kind if is_lowered_by(kind, Area::Variable) => variable,
        _ => return None,
    })
}

fn is_lowered_by(kind: K, area: Area) -> bool {
    super::disposition(kind) == Disposition::Lowered(area)
}

/// Lowers an expression node to its expression, spanning the node.
pub fn lower_expr(cx: &LowerCx, node: &SyntaxNode) -> Result<Expr, Diagnostic> {
    let rule = rule(node.kind()).ok_or_else(|| cx.unsupported(node))?;
    rule(cx, node).map(|kind| Expr::new(kind).with_span(cx.node_span(node)))
}

/// The operand of a node that has one: its first node.
fn operand(cx: &LowerCx, node: &SyntaxNode) -> Result<Expr, Diagnostic> {
    let inner = node
        .first_child()
        .ok_or_else(|| cx.missing(node, "an operand"))?;
    lower_expr(cx, &inner)
}

fn constant(cx: &LowerCx, node: &SyntaxNode) -> Result<ExprKind, Diagnostic> {
    lower_constant(cx, node).map(ExprKind::Const)
}

fn variable(cx: &LowerCx, node: &SyntaxNode) -> Result<ExprKind, Diagnostic> {
    lower_variable(cx, node).map(ExprKind::Variable)
}

fn null(cx: &LowerCx, node: &SyntaxNode) -> Result<ExprKind, Diagnostic> {
    Ok(ExprKind::Null(cx.node_span(node)))
}

/// A name standing alone: late-bound, because the declarations in scope decide
/// whether it is a variable, an enumerated value or a constant. A special
/// operator word that reaches here (`__CURRENTTASK`) names a built-in value, not
/// a declaration, and is a variable.
fn late_bound(cx: &LowerCx, node: &SyntaxNode) -> Result<ExprKind, Diagnostic> {
    let name = lower_name(cx, node)?;
    if is_special_operator(name.original()) {
        return variable(cx, node);
    }
    Ok(ExprKind::LateBound(LateBound { value: name }))
}

/// `(expression)`: the grouped expression, spanning the parentheses.
fn group(cx: &LowerCx, node: &SyntaxNode) -> Result<ExprKind, Diagnostic> {
    operand(cx, node).map(|inner| inner.kind)
}

/// The operator a node's first token is, among `table`.
fn operator_of<'a, T>(
    cx: &LowerCx,
    node: &SyntaxNode,
    table: &'a [(K, T)],
) -> Result<&'a T, Diagnostic> {
    let kinds: Vec<K> = table.iter().map(|(kind, _)| *kind).collect();
    let token = token_of(node, &kinds).ok_or_else(|| cx.missing(node, "an operator"))?;
    table
        .iter()
        .find(|(kind, _)| *kind == token.kind())
        .map(|(_, operation)| operation)
        .ok_or_else(|| cx.missing(node, "an operator"))
}

/// `left operator right`, over a chain of them: the innermost operation is
/// built first and each outer one takes the result as its left operand.
fn binary(cx: &LowerCx, node: &SyntaxNode) -> Result<ExprKind, Diagnostic> {
    let (base, links) = left_spine(node, |kind| kind == K::BinaryExpr);
    let base = base.ok_or_else(|| cx.missing(node, "an operand"))?;
    let mut result = lower_expr(cx, &base)?;
    for link in &links {
        let right = link
            .children()
            .nth(1)
            .ok_or_else(|| cx.missing(link, "a right operand"))?;
        let operation = operator_of(cx, link, OPERATIONS)?;
        result = operation
            .apply(result, lower_expr(cx, &right)?)
            .with_span(cx.node_span(link));
    }
    Ok(result.kind)
}

/// `-operand` or `NOT operand`. A negated literal stays an operation on the
/// literal: only an initial value reads it as a signed constant.
fn unary(cx: &LowerCx, node: &SyntaxNode) -> Result<ExprKind, Diagnostic> {
    let op = operator_of(cx, node, UNARY_OPERATORS)?;
    Ok(Expr::unary(op.clone(), operand(cx, node)?).kind)
}

/// `operand^`, over a chain of carets. A name under a caret is a variable,
/// not a late-bound name: the caret says what it is.
fn dereference(cx: &LowerCx, node: &SyntaxNode) -> Result<ExprKind, Diagnostic> {
    let (base, links) = left_spine(node, |kind| kind == K::DerefExpr);
    let base = base.ok_or_else(|| cx.missing(node, "an operand"))?;
    let mut result = match base.kind() {
        K::NameRef => Expr::new(variable(cx, &base)?).with_span(cx.node_span(&base)),
        _ => lower_expr(cx, &base)?,
    };
    for link in &links {
        result = Expr::new(ExprKind::Deref(Box::new(result))).with_span(cx.node_span(link));
    }
    Ok(result.kind)
}

/// `REF(variable)`.
fn reference(cx: &LowerCx, node: &SyntaxNode) -> Result<ExprKind, Diagnostic> {
    let target = node
        .first_child()
        .ok_or_else(|| cx.missing(node, "a variable"))?;
    Ok(ExprKind::Ref(Box::new(lower_variable(cx, &target)?)))
}

/// `__NEW(Type)`, `__NEW(Type, count)` and `__TYPEOF(Type)`: a call whose
/// first argument is the type.
fn special_operator(cx: &LowerCx, node: &SyntaxNode) -> Result<ExprKind, Diagnostic> {
    let operator = significant_tokens(node)
        .first()
        .map(|token| lower_id(cx, token))
        .ok_or_else(|| cx.missing(node, "an operator"))?;
    let mut nodes = node.children();
    let type_ref = nodes.next().ok_or_else(|| cx.missing(node, "a type"))?;
    let type_name = lower_type_ref(cx, &type_ref)?.name;
    let count = nodes
        .next()
        .map(|count| lower_expr(cx, &count))
        .transpose()?;
    Ok(special_operator_type_call(operator, type_name, count, cx.node_span(node)).kind)
}

/// Who a call calls.
#[derive(Debug, Clone, PartialEq)]
pub enum Callee {
    /// A function, or a function block instance, by name.
    Name(Id),
    /// A method of an instance, or of `THIS^` or `SUPER^`.
    Method {
        receiver: MethodReceiver,
        method: Id,
    },
}

/// A call as the tree writes it, before the place it is written in decides
/// the object it becomes: in an expression a function or a method call, as a
/// statement a function block call or a method call.
#[derive(Debug, Clone, PartialEq)]
pub struct Call {
    pub callee: Callee,
    pub params: Vec<ParamAssignmentKind>,
    /// From the callee through the closing parenthesis.
    pub span: SourceSpan,
}

/// `value`, as an argument by position.
fn positional(cx: &LowerCx, node: &SyntaxNode) -> Result<ParamAssignmentKind, Diagnostic> {
    Ok(ParamAssignmentKind::PositionalInput(PositionalInput {
        expr: operand(cx, node)?,
    }))
}

/// `name := value`.
fn named(cx: &LowerCx, node: &SyntaxNode) -> Result<ParamAssignmentKind, Diagnostic> {
    let mut nodes = node.children();
    let name = nodes
        .next()
        .ok_or_else(|| cx.missing(node, "a name"))
        .and_then(|name| lower_name(cx, &name))?;
    let value = nodes.next().ok_or_else(|| cx.missing(node, "a value"))?;
    Ok(ParamAssignmentKind::NamedInput(NamedInput {
        name,
        expr: lower_expr(cx, &value)?,
    }))
}

/// `[NOT] name => variable`.
fn output(cx: &LowerCx, node: &SyntaxNode) -> Result<ParamAssignmentKind, Diagnostic> {
    let mut nodes = node.children();
    let src = nodes
        .next()
        .ok_or_else(|| cx.missing(node, "an output name"))
        .and_then(|name| lower_name(cx, &name))?;
    let target = nodes.next().ok_or_else(|| cx.missing(node, "a variable"))?;
    Ok(ParamAssignmentKind::Output(Output {
        not: token_of(node, &[K::Not]).is_some(),
        src,
        tgt: lower_variable(cx, &target)?,
    }))
}

/// The rule for each kind of argument.
type ArgumentRule = fn(&LowerCx, &SyntaxNode) -> Result<ParamAssignmentKind, Diagnostic>;

const ARGUMENTS: &[(K, ArgumentRule)] = &[
    (K::PositionalArg, positional),
    (K::NamedArg, named),
    (K::OutputArg, output),
];

/// Lowers the arguments of a call: an [`K::ArgList`] node.
pub fn lower_arguments(
    cx: &LowerCx,
    node: &SyntaxNode,
) -> Result<Vec<ParamAssignmentKind>, Diagnostic> {
    if node.kind() != K::ArgList {
        return Err(cx.unsupported(node));
    }
    node.children()
        .map(|argument| {
            let (_, rule) = ARGUMENTS
                .iter()
                .find(|(kind, _)| *kind == argument.kind())
                .ok_or_else(|| cx.unsupported(&argument))?;
            rule(cx, &argument)
        })
        .collect()
}

/// The receiver of a method call: an instance by name, or `THIS^`/`SUPER^`.
fn receiver(cx: &LowerCx, node: &SyntaxNode) -> Result<MethodReceiver, Diagnostic> {
    match node.kind() {
        K::SelfRefExpr => lower_self_ref(cx, node).map(MethodReceiver::SelfRef),
        _ => lower_name(cx, node).map(MethodReceiver::Instance),
    }
}

/// Lowers a call: a [`K::CallExpr`] node, written `name(...)` or
/// `receiver.method(...)`.
pub fn lower_call(cx: &LowerCx, node: &SyntaxNode) -> Result<Call, Diagnostic> {
    if node.kind() != K::CallExpr {
        return Err(cx.unsupported(node));
    }
    let mut nodes = node.children();
    let callee = nodes.next().ok_or_else(|| cx.missing(node, "a callee"))?;
    let arguments = nodes
        .next()
        .ok_or_else(|| cx.missing(node, "an argument list"))?;
    let callee = match callee.kind() {
        K::FieldExpr => {
            let instance = callee
                .first_child()
                .ok_or_else(|| cx.missing(&callee, "a receiver"))?;
            let method = significant_tokens(&callee)
                .pop()
                .map(|token| lower_id(cx, &token))
                .ok_or_else(|| cx.missing(&callee, "a method name"))?;
            Callee::Method {
                receiver: receiver(cx, &instance)?,
                method,
            }
        }
        _ => Callee::Name(lower_name(cx, &callee)?),
    };
    Ok(Call {
        callee,
        params: lower_arguments(cx, &arguments)?,
        span: cx.node_span(node),
    })
}

/// A call in an expression: a function, or a method call.
fn call_expression(cx: &LowerCx, node: &SyntaxNode) -> Result<ExprKind, Diagnostic> {
    let call = lower_call(cx, node)?;
    Ok(match call.callee {
        Callee::Name(name) => ExprKind::Function(Function {
            name,
            param_assignment: call.params,
        }),
        Callee::Method { receiver, method } => ExprKind::MethodCall(MethodCall {
            receiver,
            method,
            params: call.params,
            position: call.span,
        }),
    })
}
