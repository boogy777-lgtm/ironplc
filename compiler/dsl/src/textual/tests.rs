//! Tests for the textual language definitions.

use super::*;
use crate::common::Integer;
use crate::core::{Id, Located, SourceSpan};

#[test]
fn array_variable_display_when_single_subscript_then_formats_with_brackets() {
    let array_var = ArrayVariable {
        subscripted_variable: Box::new(SymbolicVariableKind::Named(NamedVariable {
            name: Id::from("data"),
        })),
        subscripts: vec![Expr::new(ExprKind::integer_literal("0"))],
        span: SourceSpan::default(),
    };

    let result = format!("{}", array_var);

    assert_eq!(result, "data[0]");
}

#[test]
fn array_variable_display_when_multiple_subscripts_then_formats_with_comma_separated() {
    let array_var = ArrayVariable {
        subscripted_variable: Box::new(SymbolicVariableKind::Named(NamedVariable {
            name: Id::from("matrix"),
        })),
        subscripts: vec![
            Expr::new(ExprKind::integer_literal("1")),
            Expr::new(ExprKind::integer_literal("2")),
        ],
        span: SourceSpan::default(),
    };

    let result = format!("{}", array_var);

    assert_eq!(result, "matrix[1, 2]");
}

#[test]
fn array_variable_display_when_variable_subscript_then_formats_variable_name() {
    let array_var = ArrayVariable {
        subscripted_variable: Box::new(SymbolicVariableKind::Named(NamedVariable {
            name: Id::from("arr"),
        })),
        subscripts: vec![Expr::new(ExprKind::named_variable("i"))],
        span: SourceSpan::default(),
    };

    let result = format!("{}", array_var);

    assert_eq!(result, "arr[i]");
}

#[test]
fn display_when_named_variable_then_name() {
    let v = Variable::named("x");
    assert_eq!(format!("{v}"), "x");
}

#[test]
fn display_when_structured_variable_then_dot_notation() {
    let v = Variable::structured("rec", "field");
    assert_eq!(format!("{v}"), "rec.field");
}

#[test]
fn display_when_bit_access_variable_then_dot_index() {
    let ba = BitAccessVariable {
        variable: Box::new(SymbolicVariableKind::Named(NamedVariable {
            name: Id::from("val"),
        })),
        index: Integer::new("3", SourceSpan::default()).unwrap(),
    };
    assert_eq!(format!("{ba}"), "val.3");
}

#[test]
fn display_when_deref_variable_then_caret() {
    let d = DerefVariable {
        variable: Box::new(SymbolicVariableKind::Named(NamedVariable {
            name: Id::from("ptr"),
        })),
        span: SourceSpan::default(),
    };
    assert_eq!(format!("{d}"), "ptr^");
}

#[test]
fn span_when_array_variable_then_the_written_extent_and_not_the_array_alone() {
    let array_var = ArrayVariable {
        subscripted_variable: Box::new(SymbolicVariableKind::Named(NamedVariable {
            name: Id::from("a").with_position(SourceSpan::range(0, 1)),
        })),
        subscripts: vec![Expr::new(ExprKind::integer_literal("0"))],
        span: SourceSpan::range(0, 4),
    };
    assert_eq!(array_var.span(), SourceSpan::range(0, 4));
    assert_eq!(
        SymbolicVariableKind::Array(array_var).span(),
        SourceSpan::range(0, 4)
    );
}

#[test]
fn span_when_deref_variable_then_the_written_extent_and_not_the_pointer_alone() {
    let deref = DerefVariable {
        variable: Box::new(SymbolicVariableKind::Named(NamedVariable {
            name: Id::from("p").with_position(SourceSpan::range(0, 1)),
        })),
        span: SourceSpan::range(0, 2),
    };
    assert_eq!(deref.span(), SourceSpan::range(0, 2));
}

#[test]
fn display_when_param_assignment_positional_then_value() {
    let pa = ParamAssignmentKind::positional(ExprKind::integer_literal("42"));
    assert_eq!(format!("{pa}"), "42");
}

#[test]
fn display_when_param_assignment_named_then_name_assign_value() {
    let pa = ParamAssignmentKind::named("in1", ExprKind::integer_literal("5"));
    assert_eq!(format!("{pa}"), "in1 := 5");
}

#[test]
fn display_when_param_assignment_output_then_arrow() {
    let pa = ParamAssignmentKind::Output(Output {
        not: false,
        src: Id::from("out1"),
        tgt: Variable::named("result"),
    });
    assert_eq!(format!("{pa}"), "out1 => result");
}

#[test]
fn display_when_param_assignment_output_not_then_not_arrow() {
    let pa = ParamAssignmentKind::Output(Output {
        not: true,
        src: Id::from("out1"),
        tgt: Variable::named("result"),
    });
    assert_eq!(format!("{pa}"), "NOT out1 => result");
}

#[test]
fn display_when_compare_op_then_symbol() {
    assert_eq!(format!("{}", CompareOp::Or), "OR");
    assert_eq!(format!("{}", CompareOp::Xor), "XOR");
    assert_eq!(format!("{}", CompareOp::And), "AND");
    assert_eq!(format!("{}", CompareOp::Eq), "=");
    assert_eq!(format!("{}", CompareOp::Ne), "<>");
    assert_eq!(format!("{}", CompareOp::Lt), "<");
    assert_eq!(format!("{}", CompareOp::Gt), ">");
    assert_eq!(format!("{}", CompareOp::LtEq), "<=");
    assert_eq!(format!("{}", CompareOp::GtEq), ">=");
}

#[test]
fn display_when_operator_then_symbol() {
    assert_eq!(format!("{}", Operator::Add), "+");
    assert_eq!(format!("{}", Operator::Sub), "-");
    assert_eq!(format!("{}", Operator::Mul), "*");
    assert_eq!(format!("{}", Operator::Div), "/");
    assert_eq!(format!("{}", Operator::Mod), "MOD");
    assert_eq!(format!("{}", Operator::Pow), "**");
}

#[test]
fn display_when_unary_op_then_symbol() {
    assert_eq!(format!("{}", UnaryOp::Neg), "-");
    assert_eq!(format!("{}", UnaryOp::Not), "NOT");
}

#[test]
fn display_when_expr_kind_const_then_value() {
    let expr = ExprKind::integer_literal("10");
    assert_eq!(format!("{expr}"), "10");
}

#[test]
fn display_when_expr_kind_variable_then_name() {
    let expr = ExprKind::named_variable("x");
    assert_eq!(format!("{expr}"), "x");
}

#[test]
fn display_when_expr_kind_compare_then_formatted() {
    let expr = ExprKind::compare(
        CompareOp::Gt,
        ExprKind::named_variable("a"),
        ExprKind::integer_literal("0"),
    );
    assert_eq!(format!("{expr}"), "a > 0");
}

#[test]
fn display_when_expr_kind_binary_then_formatted() {
    let expr = ExprKind::binary(
        Operator::Add,
        ExprKind::named_variable("x"),
        ExprKind::integer_literal("1"),
    );
    assert_eq!(format!("{expr}"), "x + 1");
}

#[test]
fn display_when_expr_kind_unary_then_formatted() {
    let expr = ExprKind::unary(UnaryOp::Neg, ExprKind::named_variable("x"));
    assert_eq!(format!("{expr}"), "-x");
}

#[test]
fn display_when_function_then_name_with_parens() {
    let func = Function {
        name: Id::from("ABS"),
        param_assignment: vec![],
    };
    assert_eq!(format!("{func}"), "ABS(...)");
}

#[test]
fn display_when_late_bound_then_value() {
    let lb = LateBound {
        value: Id::from("my_val"),
    };
    assert_eq!(format!("{lb}"), "my_val");
}

#[test]
fn display_when_symbolic_variable_kind_named_then_name() {
    let svk = SymbolicVariableKind::Named(NamedVariable {
        name: Id::from("foo"),
    });
    assert_eq!(format!("{svk}"), "foo");
}
