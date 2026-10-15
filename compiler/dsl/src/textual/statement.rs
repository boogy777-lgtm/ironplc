//! Statements: assignments, selection, iteration, and the CODESYS control-flow statements.

use super::{
    Expr, ExprKind, FbCall, Function, LateBound, MethodCall, NamedVariable, ParamAssignmentKind,
    StructuredVariable, SymbolicVariableKind, Variable,
};
use crate::common::{BitStringLiteral, EnumeratedValue, SignedInteger, Subrange};
use crate::core::{Id, Located, SourceSpan};

use crate::fold::Fold;
use crate::visitor::Visitor;
use dsl_macro_derive::{Located, Recurse};

/// A body of a function bock (one of the possible types).
///
/// See section 3.
#[derive(Debug, PartialEq, Clone, Recurse)]
pub struct Statements {
    pub body: Vec<StmtKind>,
}

/// Statements.
///
/// See section 3.3.2.
#[derive(Debug, PartialEq, Clone, Recurse)]
#[expect(clippy::large_enum_variant, reason = "the AST nodes are held by value")]
pub enum StmtKind {
    Assignment(Assignment),
    // Function and function block control
    FbCall(FbCall),
    MethodCall(MethodCall),
    // Selection statements
    If(If),
    Case(Case),
    // Iteration statements
    For(For),
    While(While),
    Repeat(Repeat),
    #[recurse(ignore)]
    Return,
    // Exit statement.
    #[recurse(ignore)]
    Exit(SourceSpan),
    // Continue statement.
    #[recurse(ignore)]
    Continue(SourceSpan),
    /// `__BEGIN_IMPLEMENTATION` (CODESYS): the marker that begins a POU's
    /// implementation section in a textual export. It carries no behavior:
    /// the statements after it follow it in the same list.
    #[recurse(ignore)]
    BeginImplementation(SourceSpan),
    // CODESYS exception handling.
    TryCatch(TryCatch),
    Throw(Throw),
    // CODESYS jump, conditional call and wait statements.
    Jump(Jump),
    Label(LabelStatement),
    ConditionalCall(ConditionalCall),
    Wait(Wait),
}

impl Located for StmtKind {
    fn span(&self) -> SourceSpan {
        match self {
            StmtKind::Assignment(a) => a.span(),
            StmtKind::FbCall(f) => f.span(),
            StmtKind::MethodCall(m) => m.span(),
            StmtKind::If(i) => i.span(),
            StmtKind::Case(c) => c.span(),
            StmtKind::For(f) => f.span(),
            StmtKind::While(w) => w.span(),
            StmtKind::Repeat(r) => r.span(),
            StmtKind::Return => SourceSpan::default(),
            StmtKind::Exit(s) => s.clone(),
            StmtKind::Continue(s) => s.clone(),
            StmtKind::BeginImplementation(s) => s.clone(),
            StmtKind::TryCatch(t) => t.span(),
            StmtKind::Throw(t) => t.span(),
            StmtKind::Jump(j) => j.span(),
            StmtKind::Label(l) => l.span(),
            StmtKind::ConditionalCall(c) => c.span(),
            StmtKind::Wait(w) => w.span(),
        }
    }
}

impl StmtKind {
    pub fn if_then(condition: ExprKind, body: Vec<StmtKind>) -> StmtKind {
        StmtKind::If(If {
            expr: Expr::new(condition),
            body,
            else_ifs: vec![],
            else_body: vec![],
            span: SourceSpan::default(),
        })
    }

    pub fn if_then_else(
        condition: ExprKind,
        body: Vec<StmtKind>,
        else_body: Vec<StmtKind>,
    ) -> StmtKind {
        StmtKind::If(If {
            expr: Expr::new(condition),
            body,
            else_ifs: vec![],
            else_body,
            span: SourceSpan::default(),
        })
    }

    pub fn fb_assign(fb_name: &str, inputs: Vec<&str>, output: &str) -> StmtKind {
        let assignments = inputs
            .into_iter()
            .map(|input| ParamAssignmentKind::positional(ExprKind::late_bound(input)))
            .collect::<Vec<ParamAssignmentKind>>();

        StmtKind::assignment(
            Variable::named(output),
            ExprKind::Function(Function {
                name: Id::from(fb_name),
                param_assignment: assignments,
            }),
        )
    }
    pub fn fb_call_mapped(fb_instance_name: &str, inputs: Vec<(&str, &str)>) -> StmtKind {
        let assignments = inputs
            .into_iter()
            .map(|pair| {
                ParamAssignmentKind::named(
                    pair.0,
                    ExprKind::LateBound(LateBound {
                        value: Id::from(pair.1),
                    }),
                )
            })
            .collect::<Vec<ParamAssignmentKind>>();

        StmtKind::FbCall(FbCall {
            callee: Variable::named(fb_instance_name),
            params: assignments,
            position: SourceSpan::default(),
        })
    }

    pub fn assignment(target: Variable, value: ExprKind) -> StmtKind {
        StmtKind::Assignment(Assignment {
            target,
            deref: false,
            ref_bind: false,
            set_bind: false,
            reset_bind: false,
            value: Expr::new(value),
            span: SourceSpan::default(),
        })
    }

    pub fn simple_assignment(target: &str, src: &str) -> StmtKind {
        StmtKind::Assignment(Assignment {
            target: Variable::named(target),
            deref: false,
            ref_bind: false,
            set_bind: false,
            reset_bind: false,
            value: Expr::new(ExprKind::LateBound(LateBound {
                value: Id::from(src),
            })),
            span: SourceSpan::default(),
        })
    }

    pub fn structured_assignment(target: &str, record: &str, field: &str) -> StmtKind {
        let variable = Variable::Symbolic(SymbolicVariableKind::Structured(StructuredVariable {
            record: Box::new(SymbolicVariableKind::Named(NamedVariable {
                name: Id::from(record),
            })),
            field: Id::from(field),
        }));
        StmtKind::Assignment(Assignment {
            target: Variable::named(target),
            deref: false,
            ref_bind: false,
            set_bind: false,
            reset_bind: false,
            value: Expr::new(ExprKind::Variable(variable)),
            span: SourceSpan::default(),
        })
    }
}

/// Assigns a variable as the evaluation of an expression.
///
/// See section 3.3.2.1.
#[derive(Debug, PartialEq, Clone, Recurse, Located)]
pub struct Assignment {
    pub target: Variable,
    #[recurse(ignore)]
    pub deref: bool,
    /// `true` when the assignment was written with the TwinCAT/CODESYS `REF=`
    /// reference-binding operator rather than `:=`. The value is an
    /// `ExprKind::Ref` either way (so the backend is identical to `r := REF(x)`);
    /// this flag only lets the renderer reproduce the original `REF=` surface
    /// syntax. See `specs/design/reference-to-twincat.md`.
    #[recurse(ignore)]
    pub ref_bind: bool,
    /// `true` when written with the TwinCAT/CODESYS `S=` set-bind operator:
    /// `target` is set `TRUE` when `value` is `TRUE` and left unchanged
    /// otherwise (never cleared). Mutually exclusive with `reset_bind`.
    #[recurse(ignore)]
    pub set_bind: bool,
    /// `true` when written with the TwinCAT/CODESYS `R=` reset-bind operator:
    /// the mirror of `set_bind` (`target` is cleared `FALSE` when `value` is
    /// `TRUE`, left unchanged otherwise).
    #[recurse(ignore)]
    pub reset_bind: bool,
    pub value: Expr,
    #[located(position)]
    pub span: SourceSpan,
}

/// If selection statement.
///
/// See section 3.3.2.3.
#[derive(Debug, PartialEq, Clone, Recurse, Located)]
pub struct If {
    pub expr: Expr,
    pub body: Vec<StmtKind>,
    pub else_ifs: Vec<ElseIf>,
    pub else_body: Vec<StmtKind>,
    #[located(position)]
    pub span: SourceSpan,
}

#[derive(Debug, PartialEq, Clone, Recurse)]
pub struct ElseIf {
    pub expr: Expr,
    pub body: Vec<StmtKind>,
}

/// Case selection statement.
///
/// See section 3.3.2.3.
#[derive(Debug, PartialEq, Clone, Recurse, Located)]
pub struct Case {
    /// An expression, the result of which is used to select a particular case.
    pub selector: Expr,
    pub statement_groups: Vec<CaseStatementGroup>,
    pub else_body: Vec<StmtKind>,
    #[located(position)]
    pub span: SourceSpan,
}

/// A group of statements that can be selected within a case.
///
/// See section 3.3.2.3.
#[derive(Debug, PartialEq, Clone, Recurse)]
pub struct CaseStatementGroup {
    pub selectors: Vec<CaseSelectionKind>,
    pub statements: Vec<StmtKind>,
}

/// A particular value that selects a case statement group.
///
/// See section 3.3.2.3.
#[derive(Debug, PartialEq, Clone, Recurse)]
pub enum CaseSelectionKind {
    Subrange(Subrange),
    SignedInteger(SignedInteger),
    EnumeratedValue(EnumeratedValue),
    /// A radix-prefixed bit-string literal used as a `CASE` label (e.g.
    /// `16#D012:`, `2#1010:`).
    BitStringLiteral(BitStringLiteral),
}

/// The for loop statement.
///
/// See section 3.3.2.4.
#[derive(Debug, PartialEq, Clone, Recurse, Located)]
pub struct For {
    /// The variable that is assigned and contains the value for each loop iteration.
    pub control: Id,
    pub from: Expr,
    pub to: Expr,
    pub step: Option<Expr>,
    pub body: Vec<StmtKind>,
    #[located(position)]
    pub span: SourceSpan,
}

/// The while loop statement.
///
/// See section 3.3.2.4.
#[derive(Debug, PartialEq, Clone, Recurse, Located)]
pub struct While {
    pub condition: Expr,
    pub body: Vec<StmtKind>,
    #[located(position)]
    pub span: SourceSpan,
}

/// The repeat loop statement.
///
/// See section 3.3.2.4.
#[derive(Debug, PartialEq, Clone, Recurse, Located)]
pub struct Repeat {
    pub until: Expr,
    pub body: Vec<StmtKind>,
    #[located(position)]
    pub span: SourceSpan,
}

/// The `__TRY ... __CATCH ... __FINALLY ... __ENDTRY` exception handling
/// statement (CODESYS Structured Text extension).
///
/// See `Codesys/grammar/ST_GRAMMAR.ebnf` (tryCatchStatement).
#[derive(Debug, PartialEq, Clone, Recurse, Located)]
pub struct TryCatch {
    /// The protected statements between `__TRY` and the first handler.
    pub body: Vec<StmtKind>,
    /// The `__CATCH` clause, when the statement has one.
    pub catch: Option<CatchClause>,
    /// The statements of the `__FINALLY` clause; empty when absent.
    pub finally_body: Vec<StmtKind>,
    #[located(position)]
    pub span: SourceSpan,
}

/// The `__CATCH [(e)]` clause of a [`TryCatch`] statement.
#[derive(Debug, PartialEq, Clone, Recurse, Located)]
pub struct CatchClause {
    /// The variable the thrown value is stored into, when the `(e)` form is
    /// written. `None` for a bare `__CATCH` or an empty `__CATCH ()`.
    pub exception: Option<Variable>,
    pub body: Vec<StmtKind>,
    #[located(position)]
    pub span: SourceSpan,
}

/// The `JMP [(condition)] label;` statement (CODESYS Structured Text
/// extension, the Instruction List `JMP`/`JMPC` mnemonics).
///
/// See `Codesys/grammar/ST_GRAMMAR.ebnf` (jumpStatement).
#[derive(Debug, PartialEq, Clone, Recurse, Located)]
pub struct Jump {
    /// The `(condition)` form, which jumps only when the condition is TRUE;
    /// `None` jumps unconditionally.
    pub condition: Option<Expr>,
    /// The name of the [`LabelStatement`] to jump to.
    pub label: Id,
    #[located(position)]
    pub span: SourceSpan,
}

/// A statement label (`name:`), the target of a [`Jump`].
#[derive(Debug, PartialEq, Clone, Recurse, Located)]
pub struct LabelStatement {
    pub name: Id,
    #[located(position)]
    pub span: SourceSpan,
}

/// The `CALC(condition, call);` conditional call statement (CODESYS
/// Structured Text extension, the Instruction List `CALC` mnemonic).
///
/// The call runs only when the condition is TRUE.
#[derive(Debug, PartialEq, Clone, Recurse, Located)]
pub struct ConditionalCall {
    pub condition: Expr,
    /// The function block invocation that runs when the condition holds.
    pub call: FbCall,
    #[located(position)]
    pub span: SourceSpan,
}

/// The `__WAIT [(condition)];` wait statement (CODESYS Structured Text
/// extension): goes on only once the condition holds.
#[derive(Debug, PartialEq, Clone, Recurse, Located)]
pub struct Wait {
    /// The condition waited for; `None` for the bare `__WAIT;` form.
    pub condition: Option<Expr>,
    #[located(position)]
    pub span: SourceSpan,
}

/// The `__THROW [(expression)]` statement (CODESYS Structured Text
/// extension).
///
/// The reference parses `__THROW` as a prefixed operator, so the value is
/// written in parentheses; a bare `__THROW` raises without a value.
#[derive(Debug, PartialEq, Clone, Recurse, Located)]
pub struct Throw {
    /// The thrown value, when written. `None` for a bare `__THROW`.
    pub value: Option<Expr>,
    #[located(position)]
    pub span: SourceSpan,
}
