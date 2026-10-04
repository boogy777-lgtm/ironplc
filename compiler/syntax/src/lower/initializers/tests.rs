//! Tests of the initial values: the elements of an array and of a structure
//! initializer, and a row of `INITIAL_VALUES` for each pair of the form of a
//! type and the kind of a value.

use super::*;
use crate::lower::INTERNAL_ERROR;
use crate::{parse_source_file, ParseOptions};
use ironplc_dsl::common::{
    ArrayBounds, EnumeratedValue, IntegerRef, LateResolvedInitialValue, RefSyntax, ReferenceTarget,
    StringType,
};
use ironplc_dsl::core::{FileId, Located};
use ironplc_dsl::textual::{ExprKind, ParamAssignmentKind};
use ironplc_problems::Problem;

fn file() -> FileId {
    FileId::from_string("t.st")
}

macro_rules! variant {
    ($value:expr, $pattern:path) => {
        match $value {
            $pattern(inner) => Some(inner),
            _ => None,
        }
        .expect(concat!("a ", stringify!($pattern)))
    };
}

fn lower_first(
    source: &str,
    kind: K,
    options: &ParseOptions,
) -> Result<InitialValueAssignmentKind, Diagnostic> {
    let parse = parse_source_file(source, options);
    assert!(parse.is_ok(), "{source}: {:?}", parse.errors);
    let node = parse
        .root
        .descendants()
        .find(|node| node.kind() == kind)
        .expect("a declaration");
    lower_initial_value(&LowerCx::new(file()).with_options(*options), &node)
}

/// What `x : <text>;` in a variable block makes of its type and value.
fn variable_with(
    text: &str,
    options: &ParseOptions,
) -> Result<InitialValueAssignmentKind, Diagnostic> {
    lower_first(
        &format!("PROGRAM p VAR x : {text}; END_VAR END_PROGRAM"),
        K::VarDecl,
        options,
    )
}

fn variable(text: &str) -> InitialValueAssignmentKind {
    variable_with(text, &ParseOptions::all()).expect("the declaration lowers")
}

fn rejection(text: &str) -> Diagnostic {
    variable_with(text, &ParseOptions::all()).expect_err("the declaration is rejected")
}

/// What `a : <text>;` as a member of a structure makes of its type and value.
fn member(text: &str) -> InitialValueAssignmentKind {
    lower_first(
        &format!("TYPE t : STRUCT a : {text}; END_STRUCT; END_TYPE"),
        K::StructMember,
        &ParseOptions::all(),
    )
    .expect("the member lowers")
}

fn kind_name(initial: &InitialValueAssignmentKind) -> &'static str {
    match initial {
        InitialValueAssignmentKind::None(_) => "none",
        InitialValueAssignmentKind::Simple(_) => "simple",
        InitialValueAssignmentKind::String(_) => "string",
        InitialValueAssignmentKind::EnumeratedValues(_) => "enumerated values",
        InitialValueAssignmentKind::EnumeratedType(_) => "enumerated type",
        InitialValueAssignmentKind::FunctionBlock(_) => "function block",
        InitialValueAssignmentKind::FunctionBlockCall(_) => "function block call",
        InitialValueAssignmentKind::Subrange(_) => "subrange",
        InitialValueAssignmentKind::Structure(_) => "structure",
        InitialValueAssignmentKind::Array(_) => "array",
        InitialValueAssignmentKind::Params(_) => "params",
        InitialValueAssignmentKind::Reference(_) => "reference",
        InitialValueAssignmentKind::LateResolvedType(_) => "late resolved type",
        InitialValueAssignmentKind::SimpleExpr(_) => "simple expression",
    }
}

// ---- One row of `INITIAL_VALUES` for each pair of a type and a value.

#[test]
fn lower_initial_value_when_each_pair_of_a_type_and_a_value_then_the_object_of_its_row() {
    let rows = [
        // Array, with and without elements; an array type by its name.
        ("ARRAY[1..3] OF INT", "array"),
        ("ARRAY[1..3] OF INT := [1, 2, 3]", "array"),
        ("MyArray := [1, 2, 3]", "array"),
        // Strings of either width, with and without a length and a value.
        ("STRING", "string"),
        ("STRING[10] := 'ab'", "string"),
        ("WSTRING(10) := \"ab\"", "string"),
        // References.
        ("REF_TO INT", "reference"),
        ("REF_TO INT := NULL", "reference"),
        ("REF_TO INT := REF(y)", "reference"),
        // A list of parameters.
        ("PARAMS(3) OF INT", "params"),
        // Enumerations written in place.
        ("(A, B)", "enumerated values"),
        ("(A, B) := A", "enumerated values"),
        ("(A, B) := T#A", "enumerated values"),
        // A named type: only a later stage can classify it.
        ("MyType", "late resolved type"),
        ("MyType := (a := 1)", "late resolved type"),
        ("MyType := Red", "late resolved type"),
        ("MyType := MyType#Red", "enumerated type"),
        ("MyType := 5", "simple"),
        ("MyType := 1 + 2", "simple expression"),
        // An elementary type.
        ("INT", "simple"),
        ("INT := 5", "simple"),
        ("INT := -5", "simple"),
        ("INT := 1 + 2", "simple expression"),
        ("INT := name", "simple expression"),
        // A function block instance made with arguments.
        ("MyBlock(a := 1)", "function block call"),
    ];
    for (text, expected) in rows {
        assert_eq!(kind_name(&variable(text)), expected, "{text}");
    }
}

#[test]
fn lower_initial_value_when_array_then_inline_ranges_and_the_elements_in_order() {
    let array = variant!(
        variable("ARRAY[1..3, 0..1] OF INT := [1, 2(5), Red]"),
        InitialValueAssignmentKind::Array
    );
    let inline = variant!(array.spec, SpecificationKind::Inline);
    assert_eq!(variant!(inline.bounds, ArrayBounds::Ranges).len(), 2);
    assert_eq!(array.initial_values.len(), 3);
    let empty = variant!(
        variable("ARRAY[*] OF INT"),
        InitialValueAssignmentKind::Array
    );
    assert!(empty.initial_values.is_empty());
}

#[test]
fn lower_initial_value_when_array_type_is_a_name_then_the_array_is_that_type() {
    let array = variant!(
        variable("MyArray := [1, 2]"),
        InitialValueAssignmentKind::Array
    );
    let name = variant!(array.spec, SpecificationKind::Named);
    assert_eq!(name.to_string(), "MyArray");
    assert_eq!(array.initial_values.len(), 2);
}

#[test]
fn lower_initial_value_when_string_then_length_width_and_value_as_written() {
    let string = variant!(
        variable("STRING[10] := 'ab'"),
        InitialValueAssignmentKind::String
    );
    assert_eq!(string.width, StringType::String);
    assert_eq!(
        string
            .length
            .as_ref()
            .and_then(IntegerRef::as_integer)
            .map(|n| n.value),
        Some(10)
    );
    let value = string.initial_value.expect("a value");
    assert_eq!(value.value, vec!['a', 'b']);
    assert_eq!(
        (string.keyword_span.start, string.keyword_span.end),
        (18, 24)
    );
    let bare = variant!(variable("WSTRING"), InitialValueAssignmentKind::String);
    assert_eq!(bare.width, StringType::WString);
    assert!(bare.length.is_none() && bare.initial_value.is_none());
}

#[test]
fn lower_initial_value_when_string_value_has_the_other_delimiter_then_the_width_of_the_declaration()
{
    // The declared width governs, as it does in a type declaration.
    for (text, declared) in [
        ("STRING[10] := \"ab\"", StringType::String),
        ("WSTRING[10] := 'ab'", StringType::WString),
    ] {
        let string = variant!(variable(text), InitialValueAssignmentKind::String);
        assert_eq!(string.width, declared, "{text}");
        let value = string.initial_value.expect("a value");
        assert_eq!(value.width, declared, "{text}");
        assert_eq!(value.value, vec!['a', 'b'], "{text}");
    }
}

#[test]
fn lower_initial_value_when_reference_then_target_keyword_and_value() {
    let null = variant!(
        variable("REF_TO INT := NULL"),
        InitialValueAssignmentKind::Reference
    );
    assert!(matches!(
        null.initial_value,
        Some(ReferenceInitialValue::Null(_))
    ));
    assert_eq!(null.syntax, RefSyntax::RefTo);
    let referenced = variant!(
        variable("POINTER TO my_type := REF(y.z)"),
        InitialValueAssignmentKind::Reference
    );
    assert!(matches!(
        referenced.initial_value,
        Some(ReferenceInitialValue::Ref(_))
    ));
    assert_eq!(referenced.syntax, RefSyntax::PointerTo);
    assert!(matches!(referenced.target, ReferenceTarget::Named(_)));
}

#[test]
fn lower_initial_value_when_enumerated_values_then_the_values_and_the_default() {
    let enumerated = variant!(
        variable("(Red, Green) := Color#Green"),
        InitialValueAssignmentKind::EnumeratedValues
    );
    assert_eq!(enumerated.values.len(), 2);
    let default = enumerated.initial_value.expect("a default");
    assert_eq!(default.value.to_string(), "Green");
    assert_eq!(
        default.type_name.map(|name| name.to_string()),
        Some("Color".to_string())
    );
}

#[test]
fn lower_initial_value_when_named_type_then_late_resolved_with_the_value_as_written() {
    let bare = variant!(
        variable("MyType"),
        InitialValueAssignmentKind::LateResolvedType
    );
    assert_eq!(bare.type_name.to_string(), "MyType");
    assert!(bare.initial_value.is_none());
    let members = variant!(
        variable("MyType := (a := 1, b := 2)"),
        InitialValueAssignmentKind::LateResolvedType
    );
    let members = variant!(members.initial_value, Some);
    assert!(matches!(members, LateResolvedInitialValue::Members(ref list) if list.len() == 2));
    let name = variant!(
        variable("MyType := Red"),
        InitialValueAssignmentKind::LateResolvedType
    );
    assert!(
        matches!(name.initial_value, Some(LateResolvedInitialValue::Value(ref id)) if id.original() == "Red")
    );
    let qualified = variant!(
        variable("MyType := MyType#Red"),
        InitialValueAssignmentKind::EnumeratedType
    );
    assert_eq!(
        qualified.initial_value.map(|value| value.value.to_string()),
        Some("Red".to_string())
    );
}

#[test]
fn lower_initial_value_when_value_is_a_literal_then_simple_and_otherwise_kept_as_an_expression() {
    let negative = variant!(variable("INT := -5"), InitialValueAssignmentKind::Simple);
    let value = variant!(
        negative.initial_value.expect("a value"),
        ConstantKind::IntegerLiteral
    );
    assert!(value.value.is_neg);
    // The negation and its operand are one literal, positioned as written.
    assert_eq!(
        (value.value.value.span.start, value.value.value.span.end),
        (25, 27)
    );
    let sum = variant!(
        variable("INT := 1 + 2"),
        InitialValueAssignmentKind::SimpleExpr
    );
    assert_eq!(sum.type_name.to_string(), "INT");
    let negated_boolean = variant!(
        variable("BOOL := -TRUE"),
        InitialValueAssignmentKind::SimpleExpr
    );
    assert!(matches!(
        negated_boolean.initial_value.kind,
        ExprKind::UnaryOp(_)
    ));
}

#[test]
fn lower_initial_value_when_elementary_type_with_a_name_then_simple_expression_of_a_late_bound_name(
) {
    // `x : INT := name`: nothing says whether the name is a constant, so the
    // analysis folds it or reports it.
    let initial = variant!(
        variable("INT := name"),
        InitialValueAssignmentKind::SimpleExpr
    );
    assert_eq!(initial.type_name.to_string(), "INT");
    let late = variant!(initial.initial_value.kind, ExprKind::LateBound);
    assert_eq!(late.value.original(), "name");
}

#[test]
fn lower_initial_value_when_function_block_arguments_then_the_arguments_of_its_constructor() {
    let call = variant!(
        variable("MyBlock(a := 1, 2)"),
        InitialValueAssignmentKind::FunctionBlockCall
    );
    assert_eq!(call.type_name.to_string(), "MyBlock");
    assert_eq!(call.params.len(), 2);
    assert!(matches!(call.params[0], ParamAssignmentKind::NamedInput(_)));
    assert!(matches!(
        call.params[1],
        ParamAssignmentKind::PositionalInput(_)
    ));
    let bare = variant!(
        variable("MyBlock()"),
        InitialValueAssignmentKind::FunctionBlockCall
    );
    assert!(bare.params.is_empty());
}

#[test]
fn lower_initial_value_when_params_or_subrange_then_the_specification() {
    let params = variant!(
        variable("PARAMS(3) OF INT"),
        InitialValueAssignmentKind::Params
    );
    assert_eq!(params.type_name.to_string(), "INT");
    let source = "PROGRAM p VAR_IN_OUT x : INT(1..5); END_VAR END_PROGRAM";
    let subrange = lower_first(source, K::VarDecl, &ParseOptions::all()).expect("lowers");
    let inline = variant!(
        variant!(subrange, InitialValueAssignmentKind::Subrange).spec,
        SpecificationKind::Inline
    );
    assert_eq!(
        inline.type_name,
        ironplc_dsl::common::ElementaryTypeName::INT
    );
}

// ---- Members of a structure read the same rows.

#[test]
fn lower_initial_value_when_structure_member_then_the_object_of_its_row() {
    let rows = [
        ("INT := 5", "simple"),
        ("INT", "simple"),
        ("ARRAY[1..2] OF INT := [1, 2]", "array"),
        ("STRING[3] := 'ab'", "string"),
        ("(X, Y)", "enumerated values"),
        ("(X, Y) := X", "enumerated values"),
        ("INT(0..5)", "subrange"),
        ("Other", "late resolved type"),
        ("Other := (b := 1)", "late resolved type"),
        ("Other := X", "late resolved type"),
        ("Other := Other#X", "enumerated type"),
        ("Other := 5", "simple"),
    ];
    for (text, expected) in rows {
        assert_eq!(kind_name(&member(text)), expected, "{text}");
    }
}

#[test]
fn lower_initial_value_when_member_enumeration_has_a_default_then_it_is_kept() {
    let enumerated = variant!(
        member("(X, Y) := Y"),
        InitialValueAssignmentKind::EnumeratedValues
    );
    assert_eq!(
        enumerated
            .initial_value
            .map(|value| value.value.to_string()),
        Some("Y".to_string())
    );
}

#[test]
fn lower_initial_value_when_member_subrange_has_a_default_then_the_object_holds_it() {
    let subrange = variant!(
        member("INT(0..5) := 3"),
        InitialValueAssignmentKind::Subrange
    );
    assert_eq!(
        subrange
            .initial_value
            .as_ref()
            .map(|value| value.to_string()),
        Some("3".to_string())
    );
    let inline = variant!(subrange.spec, SpecificationKind::Inline);
    assert_eq!(
        inline.subrange.end.as_signed_integer().map(|n| n.to_i64()),
        Some(5)
    );
}

// ---- Values that do not fit.

#[test]
fn lower_initial_value_when_value_does_not_fit_the_type_then_initializer_mismatch_at_the_value() {
    let rows = [
        (
            "INT := [1, 2]",
            "an array value is not an initial value of an elementary type",
        ),
        (
            "INT := (a := 1)",
            "a structure value is not an initial value of an elementary type",
        ),
        (
            "INT := Color#Red",
            "a qualified enumeration value is not an initial value of an elementary type",
        ),
        (
            "(A, B) := 5",
            "a value is not an initial value of an enumeration",
        ),
        (
            "(A, B) := (a := 1)",
            "a structure value is not an initial value of an enumeration",
        ),
    ];
    for (text, message) in rows {
        let error = rejection(text);
        assert_eq!(
            error.code,
            Problem::InitializerTypeMismatch.code(),
            "{text}"
        );
        assert_eq!(error.primary.message, message, "{text}");
    }
    let error = rejection("INT := [1, 2]");
    assert_eq!(
        (error.primary.location.start, error.primary.location.end),
        (25, 31)
    );
}

#[test]
fn lower_initial_value_when_node_does_not_hold_a_declaration_with_a_value_then_internal_error() {
    // A structure is only declared in a `TYPE` block; the grammar reports it
    // in a variable, and a row for it would not be reached.
    let parse = parse_source_file(
        "TYPE t : STRUCT a : INT; END_STRUCT; END_TYPE",
        &ParseOptions::all(),
    );
    let node = parse
        .root
        .descendants()
        .find(|node| node.kind() == K::TypeDecl)
        .expect("a declaration");
    let error = lower_initial_value(&LowerCx::new(file()), &node).expect_err("an error");
    assert_eq!(error.code, INTERNAL_ERROR);
}

#[test]
fn lower_initial_value_when_member_subrange_default_is_not_digits_then_initializer_mismatch() {
    for value in ["1.5", "TRUE", "INT#5", "16#FF", "'a'", "T#5s"] {
        let parse = parse_source_file(
            &format!("TYPE t : STRUCT a : INT(0..5) := {value}; END_STRUCT; END_TYPE"),
            &ParseOptions::all(),
        );
        let node = parse
            .root
            .descendants()
            .find(|node| node.kind() == K::StructMember)
            .expect("a member");
        let error = lower_initial_value(&LowerCx::new(file()), &node).expect_err(value);
        assert_eq!(
            error.code,
            Problem::InitializerTypeMismatch.code(),
            "{value}"
        );
    }
}

// ---- Elements of an initializer.

#[test]
fn lower_array_elements_when_each_kind_of_element_then_its_object() {
    let array = variant!(
        variable(
            "ARRAY[1..9] OF INT := [1, -2, 3.5, TRUE, 'a', T#5s, Red, Color#Green, 4(7), 2()]"
        ),
        InitialValueAssignmentKind::Array
    );
    let kinds: Vec<&str> = array
        .initial_values
        .iter()
        .map(|element| match element {
            ArrayInitialElementKind::Constant(_) => "constant",
            ArrayInitialElementKind::EnumValue(_) => "enumerated",
            ArrayInitialElementKind::Repeated(_) => "repeated",
        })
        .collect();
    assert_eq!(
        kinds,
        vec![
            "constant",
            "constant",
            "constant",
            "constant",
            "constant",
            "constant",
            "enumerated",
            "enumerated",
            "repeated",
            "repeated"
        ]
    );
}

#[test]
fn lower_array_elements_when_repeated_then_the_count_at_its_digits_and_the_element() {
    let array = variant!(
        variable("ARRAY[1..9] OF INT := [4(7), 3(Red), 2()]"),
        InitialValueAssignmentKind::Array
    );
    let repeated = variant!(
        array.initial_values[0].clone(),
        ArrayInitialElementKind::Repeated
    );
    assert_eq!((repeated.size.span.start, repeated.size.span.end), (41, 42));
    assert!(matches!(
        *repeated.init,
        Some(ArrayInitialElementKind::Constant(_))
    ));
    let named = variant!(
        array.initial_values[1].clone(),
        ArrayInitialElementKind::Repeated
    );
    assert!(matches!(
        *named.init,
        Some(ArrayInitialElementKind::EnumValue(_))
    ));
}

#[test]
fn lower_array_elements_when_not_an_array_initializer_then_internal_error() {
    let parse = parse_source_file(
        "PROGRAM p VAR x : INT := 5; END_VAR END_PROGRAM",
        &ParseOptions::all(),
    );
    let node = parse
        .root
        .descendants()
        .find(|n| n.kind() == K::IntLiteral)
        .expect("a literal");
    let cx = LowerCx::new(file());
    assert_eq!(
        lower_array_elements(&cx, &node).err().map(|e| e.code),
        Some(INTERNAL_ERROR.to_string())
    );
    assert_eq!(
        lower_struct_elements(&cx, &node).err().map(|e| e.code),
        Some(INTERNAL_ERROR.to_string())
    );
}

#[test]
fn lower_struct_elements_when_each_kind_of_value_then_its_object() {
    let late = variant!(
        variable("MyType := (a := 1, b := -2, c := 1.5, d := (e := 3), f := [1, 2], g := Color#Red, h := name, i := 1 + 2, j := -TRUE, k := (5), l := f(1), m := x.y)"),
        InitialValueAssignmentKind::LateResolvedType
    );
    let elements = variant!(
        late.initial_value.expect("members"),
        LateResolvedInitialValue::Members
    );
    let kinds: Vec<&str> = elements
        .iter()
        .map(|element| match element.init {
            StructInitialValueAssignmentKind::Constant(_) => "constant",
            StructInitialValueAssignmentKind::EnumeratedValue(_) => "enumerated",
            StructInitialValueAssignmentKind::Array(_) => "array",
            StructInitialValueAssignmentKind::Structure(_) => "structure",
            StructInitialValueAssignmentKind::Expression(_) => "expression",
            StructInitialValueAssignmentKind::LateBound(_) => "late bound",
        })
        .collect();
    assert_eq!(
        kinds,
        vec![
            "constant",
            "constant",
            "constant",
            "structure",
            "array",
            "enumerated",
            "late bound",
            "expression",
            "expression",
            "expression",
            "expression",
            "expression"
        ]
    );
    let names: Vec<String> = elements
        .iter()
        .map(|e| e.name.original().to_string())
        .collect();
    assert_eq!(names[0], "a");
    assert_eq!(names[11], "m");
}

#[test]
fn lower_struct_elements_when_value_is_a_negative_number_then_one_constant_positioned_as_written() {
    let late = variant!(
        variable("MyType := (a := -2)"),
        InitialValueAssignmentKind::LateResolvedType
    );
    let elements = variant!(
        late.initial_value.expect("members"),
        LateResolvedInitialValue::Members
    );
    let constant = variant!(
        elements[0].init.clone(),
        StructInitialValueAssignmentKind::Constant
    );
    let span = constant.span();
    assert_eq!((span.start, span.end), (34, 36));
}

#[test]
fn lower_enumerated_value_when_qualified_or_bare_then_with_and_without_its_type() {
    let late = variant!(
        variable("MyType := MyType#Red"),
        InitialValueAssignmentKind::EnumeratedType
    );
    let value: EnumeratedValue = late.initial_value.expect("a value");
    assert_eq!(
        value.type_name.map(|name| name.to_string()),
        Some("MyType".to_string())
    );
}

// ---- Long and deep initializers.

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
        .stack_size(ironplc_dsl::stack::STACK_BUDGET)
        .spawn(body)
        .ok()?
        .join()
        .ok()
}

#[test]
fn lower_array_elements_when_very_long_then_lowered_by_iteration_on_the_smallest_stack() {
    let count = on_small_stack(|| {
        let elements = vec!["1"; 100_000].join(", ");
        let value = variable(&format!("ARRAY[1..100000] OF INT := [{elements}]"));
        match value {
            InitialValueAssignmentKind::Array(array) => array.initial_values.len(),
            _ => 0,
        }
    });
    assert_eq!(count, Some(100_000));
}

#[test]
fn lower_struct_elements_when_very_long_then_lowered_by_iteration_on_the_smallest_stack() {
    let count = on_small_stack(|| {
        let members: Vec<String> = (0..50_000).map(|n| format!("m{n} := {n}")).collect();
        match variable(&format!("MyType := ({})", members.join(", "))) {
            InitialValueAssignmentKind::LateResolvedType(late) => match late.initial_value {
                Some(LateResolvedInitialValue::Members(members)) => members.len(),
                _ => 0,
            },
            _ => 0,
        }
    });
    assert_eq!(count, Some(50_000));
}

/// `(a := (a := ... 1))`, nested `depth` times.
fn nested_structures(depth: usize) -> String {
    format!("MyType := {}1{}", "(a := ".repeat(depth), ")".repeat(depth))
}

#[test]
fn lower_struct_elements_when_nesting_is_as_deep_as_the_tree_allows_then_lowered_on_the_stack_budget(
) {
    let depth = on_budget_stack(|| {
        let source = |depth: usize| {
            format!(
                "PROGRAM p VAR x : {}; END_VAR END_PROGRAM",
                nested_structures(depth)
            )
        };
        let deepest = (1..crate::MAX_DEPTH)
            .rev()
            .find(|depth| parse_source_file(&source(*depth), &ParseOptions::all()).is_ok())
            .unwrap_or(0);
        let lowered = variable(&nested_structures(deepest));
        let mut levels = 0;
        let mut current = match lowered {
            InitialValueAssignmentKind::LateResolvedType(late) => late.initial_value,
            _ => None,
        };
        while let Some(LateResolvedInitialValue::Members(members)) = current {
            levels += 1;
            current = match members.into_iter().next().map(|member| member.init) {
                Some(StructInitialValueAssignmentKind::Structure(inner)) => {
                    Some(LateResolvedInitialValue::Members(inner))
                }
                _ => None,
            };
        }
        (deepest, levels)
    });
    let (deepest, levels) = depth.expect("the thread finished");
    assert!(deepest > crate::MAX_DEPTH / 4, "{deepest}");
    assert_eq!(levels, deepest);
}
