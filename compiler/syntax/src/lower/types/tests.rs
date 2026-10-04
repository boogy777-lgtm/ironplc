//! Tests of the types: each piece of a specification, and a row of
//! `DECLARATIONS` for each declaration it builds.

use super::*;
use crate::lower::{disposition, Area, Disposition, INTERNAL_ERROR};
use crate::{parse_source_file, ParseOptions};
use ironplc_dsl::common::{
    ArrayInitialElementKind, ConstantKind, InitialValueAssignmentKind, IntegerRef,
    SignedIntegerRef, StructInitialValueAssignmentKind,
};
use ironplc_dsl::core::{FileId, Located};
use ironplc_problems::Problem;

fn file() -> FileId {
    FileId::from_string("t.st")
}

/// The variant of a value, or a failure naming the variant it should have
/// been.
macro_rules! variant {
    ($value:expr, $pattern:path) => {
        match $value {
            $pattern(inner) => Some(inner),
            _ => None,
        }
        .expect(concat!("a ", stringify!($pattern)))
    };
}

fn lower_source(
    source: &str,
    options: &ParseOptions,
) -> Result<Vec<DataTypeDeclarationKind>, Diagnostic> {
    let parse = parse_source_file(source, options);
    assert!(parse.is_ok(), "{source}: {:?}", parse.errors);
    let cx = LowerCx::new(file()).with_options(*options);
    let block = parse
        .root
        .descendants()
        .find(|node| node.kind() == K::TypeBlock)
        .expect("a TYPE block");
    lower_type_block(&cx, &block)
}

/// The declarations of `TYPE <declarations> END_TYPE`.
fn declarations(text: &str) -> Vec<DataTypeDeclarationKind> {
    lower_source(&format!("TYPE {text} END_TYPE"), &ParseOptions::all())
        .expect("the declarations lower")
}

/// The declaration of `TYPE t : <text>; END_TYPE`.
fn declaration(text: &str) -> DataTypeDeclarationKind {
    declarations(&format!("t : {text};")).remove(0)
}

fn rejection(text: &str) -> Diagnostic {
    lower_source(&format!("TYPE t : {text}; END_TYPE"), &ParseOptions::all())
        .expect_err("the declaration is rejected")
}

fn integer_ref(value: &IntegerRef) -> Option<u128> {
    value.as_integer().map(|integer| integer.value)
}

fn bound(value: &SignedIntegerRef) -> Option<i64> {
    value.as_signed_integer().map(|number| number.to_i64())
}

// ---- The pieces of a specification.

#[test]
fn lower_string_specification_when_length_in_brackets_or_parentheses_then_width_length_and_keyword_span(
) {
    for (text, width, length) in [
        ("STRING[10]", StringType::String, Some(10)),
        ("STRING(10)", StringType::String, Some(10)),
        ("WSTRING[4]", StringType::WString, Some(4)),
        ("WSTRING(4)", StringType::WString, Some(4)),
        ("STRING", StringType::String, None),
        ("wstring", StringType::WString, None),
    ] {
        let source = format!("TYPE t : ARRAY[1..2] OF {text}; END_TYPE");
        let parse = parse_source_file(&source, &ParseOptions::all());
        let node = parse
            .root
            .descendants()
            .find(|node| node.kind() == K::StringType)
            .expect("a string type");
        let specification =
            lower_string_specification(&LowerCx::new(file()), &node).expect("a specification");
        assert_eq!(specification.width, width, "{text}");
        assert_eq!(
            specification.length.as_ref().and_then(integer_ref),
            length,
            "{text}"
        );
        assert_eq!(
            (
                specification.keyword_span.start,
                specification.keyword_span.end
            ),
            (24, 24 + text.split(['[', '(']).next().unwrap_or("").len()),
            "{text}"
        );
    }
}

#[test]
fn lower_array_when_ranges_then_each_range_with_its_bounds() {
    let spec = variant!(
        declaration("ARRAY[-2..+3, lo..hi] OF INT"),
        DataTypeDeclarationKind::Array
    );
    let array = variant!(spec.spec, SpecificationKind::Inline);
    let ranges = variant!(array.bounds, ArrayBounds::Ranges);
    assert_eq!(ranges.len(), 2);
    assert_eq!(bound(&ranges[0].start), Some(-2));
    assert_eq!(bound(&ranges[0].end), Some(3));
    assert!(matches!(&ranges[1].start, SignedIntegerRef::Constant(id) if id.original() == "lo"));
    assert!(matches!(&array.type_name, ArrayElementType::Named(name) if name.to_string() == "INT"));
    assert_eq!(array.ref_to, None);
}

#[test]
fn lower_array_when_incomplete_then_the_span_of_the_star() {
    let spec = variant!(
        declaration("ARRAY[*] OF INT"),
        DataTypeDeclarationKind::Array
    );
    let array = variant!(spec.spec, SpecificationKind::Inline);
    let star = variant!(array.bounds, ArrayBounds::Incomplete);
    assert_eq!((star.start, star.end), (15, 16));
}

#[test]
fn lower_array_when_element_is_a_string_then_the_string_specification() {
    let spec = variant!(
        declaration("ARRAY[1..2] OF WSTRING[5]"),
        DataTypeDeclarationKind::Array
    );
    let array = variant!(spec.spec, SpecificationKind::Inline);
    let element = variant!(array.type_name, ArrayElementType::WString);
    assert_eq!(element.length.as_ref().and_then(integer_ref), Some(5));
    let bare = variant!(
        declaration("ARRAY[1..2] OF STRING"),
        DataTypeDeclarationKind::Array
    );
    let array = variant!(bare.spec, SpecificationKind::Inline);
    let element = variant!(array.type_name, ArrayElementType::String);
    assert!(element.length.is_none());
}

#[test]
fn lower_array_when_element_is_a_reference_then_the_keyword_and_the_type_it_points_to() {
    for (text, syntax) in [
        ("REF_TO", RefSyntax::RefTo),
        ("REFERENCE TO", RefSyntax::ReferenceTo),
        ("POINTER TO", RefSyntax::PointerTo),
    ] {
        let spec = variant!(
            declaration(&format!("ARRAY[1..2] OF {text} my_type")),
            DataTypeDeclarationKind::Array
        );
        let array = variant!(spec.spec, SpecificationKind::Inline);
        assert_eq!(array.ref_to, Some(syntax), "{text}");
        assert!(
            matches!(&array.type_name, ArrayElementType::Named(name) if name.to_string() == "my_type"),
            "{text}"
        );
    }
}

#[test]
fn parse_array_when_element_is_a_reference_to_an_array_then_the_error_is_at_the_array() {
    // The object holds an array element as a name or a string, so the grammar
    // does not offer an array as the target of a reference element.
    let source = "TYPE t : ARRAY[1..2] OF REF_TO ARRAY[1..2] OF INT; END_TYPE";
    let parse = parse_source_file(source, &ParseOptions::all());
    let ranges: Vec<_> = parse
        .errors
        .iter()
        .map(|error| {
            (
                usize::from(error.range.start()),
                usize::from(error.range.end()),
            )
        })
        .collect();
    assert_eq!(ranges, vec![(31, 36)]);
    assert_eq!(parse.root.text().to_string(), source);
}

#[test]
fn lower_reference_when_target_is_a_name_an_array_or_a_string_then_that_target() {
    let named = variant!(
        declaration("REF_TO my_type"),
        DataTypeDeclarationKind::Reference
    );
    assert_eq!(named.syntax, RefSyntax::RefTo);
    assert!(matches!(&named.target, ReferenceTarget::Named(name) if name.to_string() == "my_type"));
    let array = variant!(
        declaration("POINTER TO ARRAY[1..2] OF INT"),
        DataTypeDeclarationKind::Reference
    );
    assert_eq!(array.syntax, RefSyntax::PointerTo);
    assert!(matches!(array.target, ReferenceTarget::Array(_)));
    let string = variant!(
        declaration("REF_TO STRING"),
        DataTypeDeclarationKind::Reference
    );
    assert!(matches!(&string.target, ReferenceTarget::Named(name) if name.to_string() == "STRING"));
}

#[test]
fn lower_params_when_count_is_a_number_or_a_name_then_the_count_and_the_element_type() {
    let literal = variant!(
        declaration("PARAMS(3) OF INT"),
        DataTypeDeclarationKind::Params
    );
    assert_eq!(integer_ref(&literal.spec.count), Some(3));
    assert_eq!(literal.spec.type_name.to_string(), "INT");
    let constant = variant!(
        declaration("PARAMS(n) OF STRING"),
        DataTypeDeclarationKind::Params
    );
    assert!(matches!(&constant.spec.count, IntegerRef::Constant(id) if id.original() == "n"));
    assert_eq!(constant.spec.type_name.to_string(), "STRING");
}

#[test]
fn lower_subrange_specification_when_base_and_range_then_the_elementary_base_and_the_bounds() {
    let subrange = variant!(
        declaration("UINT(1..10)"),
        DataTypeDeclarationKind::Subrange
    );
    let inline = variant!(subrange.spec, SpecificationKind::Inline);
    assert_eq!(inline.type_name, ElementaryTypeName::UINT);
    assert_eq!(bound(&inline.subrange.start), Some(1));
    assert_eq!(bound(&inline.subrange.end), Some(10));
    assert!(subrange.default.is_none());
}

#[test]
fn lower_enumeration_when_values_explicit_values_and_base_type_then_each_part() {
    let enumeration = variant!(
        declaration("(A, B := 2, C := -1) BYTE := B"),
        DataTypeDeclarationKind::Enumeration
    );
    let values = variant!(enumeration.spec_init.spec, SpecificationKind::Inline).values;
    let names: Vec<String> = values.iter().map(|value| value.value.to_string()).collect();
    assert_eq!(names, vec!["A", "B", "C"]);
    assert!(values[0].explicit_value.is_none());
    assert_eq!(
        values[1].explicit_value.as_ref().map(|n| n.to_i64()),
        Some(2)
    );
    assert_eq!(
        values[2].explicit_value.as_ref().map(|n| n.to_i64()),
        Some(-1)
    );
    assert!(values.iter().all(|value| value.type_name.is_none()));
    assert_eq!(
        enumeration.spec_init.underlying_type,
        Some(ElementaryTypeName::BYTE)
    );
    assert_eq!(
        enumeration
            .spec_init
            .default
            .map(|value| value.value.to_string()),
        Some("B".to_string())
    );
}

#[test]
fn lower_members_when_structure_then_each_member_with_its_type_and_value() {
    let structure = variant!(
        declaration("STRUCT a : INT := 5; b : REAL; END_STRUCT"),
        DataTypeDeclarationKind::Structure
    );
    assert_eq!(structure.elements.len(), 2);
    assert_eq!(structure.elements[0].name.to_string(), "a");
    let simple = variant!(
        structure.elements[0].init.clone(),
        InitialValueAssignmentKind::Simple
    );
    assert!(simple.initial_value.is_some());
}

// ---- A row of `DECLARATIONS` for each declaration.

#[test]
fn lower_type_declaration_when_sized_string_then_string_declaration_with_the_declared_width() {
    let string = variant!(declaration("STRING[10]"), DataTypeDeclarationKind::String);
    assert_eq!(string.type_name.to_string(), "t");
    assert_eq!(string.width, StringType::String);
    assert_eq!(integer_ref(&string.length), Some(10));
    assert!(string.init.is_none());
    for (text, declared) in [
        ("STRING[10] := 'ab'", StringType::String),
        ("STRING[10] := \"ab\"", StringType::String),
        ("STRING(10) := 'ab'", StringType::String),
        ("WSTRING[10] := \"ab\"", StringType::WString),
        ("WSTRING[10] := 'ab'", StringType::WString),
    ] {
        let string = variant!(declaration(text), DataTypeDeclarationKind::String);
        let init = string.init.expect("a value");
        assert_eq!(string.width, declared, "{text}");
        assert_eq!(init.width, declared, "{text}: the declared width governs");
        assert_eq!(init.value, vec!['a', 'b'], "{text}");
    }
    let named = variant!(declaration("STRING[n]"), DataTypeDeclarationKind::String);
    assert!(matches!(named.length, IntegerRef::Constant(_)));
}

#[test]
fn lower_type_declaration_when_array_then_array_declaration_with_its_elements() {
    let array = variant!(
        declaration("ARRAY[1..3] OF INT"),
        DataTypeDeclarationKind::Array
    );
    assert!(array.init.is_empty());
    let array = variant!(
        declaration("ARRAY[1..6] OF INT := [1, -2, 3(7), 2(), Red, Color#Green]"),
        DataTypeDeclarationKind::Array
    );
    assert_eq!(array.init.len(), 6);
    assert!(matches!(
        array.init[0],
        ArrayInitialElementKind::Constant(_)
    ));
    assert!(matches!(
        array.init[1],
        ArrayInitialElementKind::Constant(_)
    ));
    let repeated = variant!(array.init[2].clone(), ArrayInitialElementKind::Repeated);
    assert_eq!(repeated.size.value, 3);
    assert!(repeated.init.is_some());
    let empty = variant!(array.init[3].clone(), ArrayInitialElementKind::Repeated);
    assert_eq!(empty.size.value, 2);
    assert!(empty.init.is_none());
    let bare = variant!(array.init[4].clone(), ArrayInitialElementKind::EnumValue);
    assert!(bare.type_name.is_none());
    let qualified = variant!(array.init[5].clone(), ArrayInitialElementKind::EnumValue);
    assert_eq!(
        qualified.type_name.map(|name| name.to_string()),
        Some("Color".to_string())
    );
}

#[test]
fn lower_type_declaration_when_params_then_params_declaration() {
    let params = variant!(
        declaration("PARAMS(4) OF INT"),
        DataTypeDeclarationKind::Params
    );
    assert_eq!(params.type_name.to_string(), "t");
    assert_eq!(integer_ref(&params.spec.count), Some(4));
}

#[test]
fn lower_type_declaration_when_subrange_then_subrange_declaration_with_its_default() {
    let subrange = variant!(
        declaration("INT(-5..5) := -1"),
        DataTypeDeclarationKind::Subrange
    );
    assert_eq!(subrange.default.map(|default| default.to_i64()), Some(-1));
}

#[test]
fn lower_type_declaration_when_structure_or_union_then_its_members() {
    let structure = variant!(
        declaration("STRUCT a : INT; END_STRUCT"),
        DataTypeDeclarationKind::Structure
    );
    assert_eq!(structure.elements.len(), 1);
    let union = variant!(
        declaration("UNION a : INT; b : REAL; END_UNION"),
        DataTypeDeclarationKind::Union
    );
    assert_eq!(union.type_name.to_string(), "t");
    assert_eq!(union.elements.len(), 2);
}

#[test]
fn lower_type_declaration_when_named_type_with_structure_value_then_structure_alias() {
    let alias = variant!(
        declaration("Point := (x := 1, y := (z := 2), a := [1, 2], e := Color#Red, n := name, k := 1 + 2, m := -5)"),
        DataTypeDeclarationKind::Simple
    );
    // The declaration holds the name it declares, the structure it copies
    // and the values.
    assert_eq!(alias.type_name.to_string(), "t");
    let structure = variant!(alias.spec_and_init, InitialValueAssignmentKind::Structure);
    assert_eq!(structure.type_name.to_string(), "Point");
    let kinds: Vec<&str> = structure
        .elements_init
        .iter()
        .map(|element| match element.init {
            StructInitialValueAssignmentKind::Constant(_) => "constant",
            StructInitialValueAssignmentKind::Structure(_) => "structure",
            StructInitialValueAssignmentKind::Array(_) => "array",
            StructInitialValueAssignmentKind::EnumeratedValue(_) => "enumerated",
            StructInitialValueAssignmentKind::LateBound(_) => "late bound",
            StructInitialValueAssignmentKind::Expression(_) => "expression",
        })
        .collect();
    assert_eq!(
        kinds,
        vec![
            "constant",
            "structure",
            "array",
            "enumerated",
            "late bound",
            "expression",
            "constant"
        ]
    );
}

#[test]
fn lower_type_declaration_when_enumeration_then_enumeration_declaration_of_inline_values() {
    let enumeration = variant!(
        declaration("(Red, Green) := Color#Red"),
        DataTypeDeclarationKind::Enumeration
    );
    let value = enumeration.spec_init.default.expect("a default");
    assert_eq!(value.value.to_string(), "Red");
    assert_eq!(
        value.type_name.map(|name| name.to_string()),
        Some("Color".to_string())
    );
    assert!(enumeration.spec_init.underlying_type.is_none());
}

#[test]
fn lower_type_declaration_when_named_type_with_a_value_then_enumeration_of_that_type() {
    for text in ["Color := Red", "Color := Color#Red"] {
        let enumeration = variant!(declaration(text), DataTypeDeclarationKind::Enumeration);
        let base = variant!(enumeration.spec_init.spec, SpecificationKind::Named);
        assert_eq!(base.to_string(), "Color", "{text}");
        assert_eq!(
            enumeration
                .spec_init
                .default
                .map(|value| value.value.to_string()),
            Some("Red".to_string()),
            "{text}"
        );
    }
}

#[test]
fn lower_type_declaration_when_elementary_or_named_type_with_a_constant_then_simple_declaration() {
    for (text, base, has_value) in [
        ("INT", "INT", false),
        ("INT := 5", "INT", true),
        ("INT := -5", "INT", true),
        ("REAL := 1.5", "REAL", true),
        ("tod", "TIME_OF_DAY", false),
        ("TIME := T#5s", "TIME", true),
        ("STRING", "STRING", false),
        ("WSTRING", "WSTRING", false),
        ("STRING := 'a'", "STRING", true),
        ("Alias := 5", "Alias", true),
    ] {
        let simple = variant!(declaration(text), DataTypeDeclarationKind::Simple);
        let initializer = variant!(simple.spec_and_init, InitialValueAssignmentKind::Simple);
        assert_eq!(initializer.type_name.to_string(), base, "{text}");
        assert_eq!(initializer.initial_value.is_some(), has_value, "{text}");
    }
}

#[test]
fn lower_type_declaration_when_negative_constant_then_signed_literal_positioned_with_its_sign() {
    let simple = variant!(declaration("INT := -5"), DataTypeDeclarationKind::Simple);
    let initializer = variant!(simple.spec_and_init, InitialValueAssignmentKind::Simple);
    let value = initializer.initial_value.expect("a value");
    let literal = variant!(value, ConstantKind::IntegerLiteral);
    assert!(literal.value.is_neg);
    assert_eq!(
        (literal.value.value.span.start, literal.value.value.span.end),
        (16, 18)
    );
}

#[test]
fn lower_type_declaration_when_reference_then_reference_declaration_with_the_keyword() {
    for (text, syntax) in [
        ("REF_TO INT", RefSyntax::RefTo),
        ("REFERENCE TO INT", RefSyntax::ReferenceTo),
        ("POINTER TO INT", RefSyntax::PointerTo),
    ] {
        let reference = variant!(declaration(text), DataTypeDeclarationKind::Reference);
        assert_eq!(reference.syntax, syntax, "{text}");
    }
}

#[test]
fn lower_type_declaration_when_named_type_alone_then_late_bound_declaration() {
    let late = variant!(declaration("Other"), DataTypeDeclarationKind::LateBound);
    assert_eq!(late.data_type_name.to_string(), "t");
    assert_eq!(late.base_type_name.to_string(), "Other");
}

#[test]
fn lower_type_declaration_when_keyword_the_dialect_leaves_a_name_then_a_late_bound_name() {
    // Without `allow_bit_type`, `BIT` is a name, so `t : BIT` declares a type
    // that is another type.
    let source = "TYPE t : BIT; END_TYPE";
    let declared = lower_source(source, &ParseOptions::default()).expect("lowers");
    assert!(matches!(declared[0], DataTypeDeclarationKind::LateBound(_)));
    let enabled = lower_source(source, &ParseOptions::all()).expect("lowers");
    assert!(matches!(enabled[0], DataTypeDeclarationKind::Simple(_)));
}

#[test]
fn lower_type_declaration_when_time_may_name_a_function_then_time_before_an_assignment_is_a_name() {
    let options = ParseOptions {
        allow_time_as_function_name: true,
        ..ParseOptions::default()
    };
    // `TIME` followed by `:=` is the name of a function here, as the parser
    // reads it, so it is a named type written as it was; without a `:=` after
    // it, it is the type.
    let named = lower_source("TYPE t : time := T#5s; END_TYPE", &options).expect("lowers");
    let simple = variant!(named[0].clone(), DataTypeDeclarationKind::Simple);
    let initializer = variant!(simple.spec_and_init, InitialValueAssignmentKind::Simple);
    assert_eq!(initializer.type_name.to_string(), "time");
    let bare = lower_source("TYPE t : time; END_TYPE", &options).expect("lowers");
    assert!(matches!(bare[0], DataTypeDeclarationKind::Simple(_)));
    let keyword =
        lower_source("TYPE t : time := T#5s; END_TYPE", &ParseOptions::default()).expect("lowers");
    let simple = variant!(keyword[0].clone(), DataTypeDeclarationKind::Simple);
    let initializer = variant!(simple.spec_and_init, InitialValueAssignmentKind::Simple);
    assert_eq!(initializer.type_name.to_string(), "TIME");
}

// ---- Declarations that do not fit a row.

#[test]
fn lower_type_declaration_when_value_does_not_fit_the_type_then_initializer_mismatch() {
    for text in [
        "ARRAY[1..2] OF INT := 5",
        "STRING[5] := 5",
        "INT := [1, 2]",
        "INT := Color#Red",
        "(A, B) := 5",
        "PARAMS(2) OF INT := 5",
        "INT(1..5) := 1.5",
        "INT(1..5) := INT#5",
        "INT(1..5) := 16#FF",
        "WSTRING[5] := -name",
    ] {
        let parse = parse_source_file(&format!("TYPE t : {text}; END_TYPE"), &ParseOptions::all());
        if !parse.is_ok() {
            continue;
        }
        let error = rejection(text);
        assert_eq!(
            error.code,
            Problem::InitializerTypeMismatch.code(),
            "{text}"
        );
    }
}

#[test]
fn lower_type_declaration_when_enumeration_default_is_a_number_then_initializer_mismatch() {
    let error = rejection("(A, B) := 5");
    assert_eq!(error.code, Problem::InitializerTypeMismatch.code());
    assert!(error
        .primary
        .message
        .contains("is not an initial value of an enumeration"));
}

#[test]
fn lower_type_declaration_when_not_a_type_declaration_then_internal_error() {
    let parse = parse_source_file("PROGRAM p END_PROGRAM", &ParseOptions::all());
    let node = parse.root.first_child().expect("a program");
    let cx = LowerCx::new(file());
    for result in [
        lower_type_declaration(&cx, &node).err(),
        lower_type_block(&cx, &node).err(),
    ] {
        assert!(result.is_some());
    }
    let error = lower_type_declaration(&cx, &node).expect_err("an error");
    assert_eq!(error.code, INTERNAL_ERROR);
}

// ---- Blocks.

#[test]
fn lower_type_block_when_several_declarations_then_one_declaration_each_in_order() {
    let declared = declarations(
        "a : INT; b : (X, Y); c : STRUCT d : INT; END_STRUCT; e : ARRAY[1..2] OF INT;",
    );
    let names: Vec<String> = declared
        .iter()
        .map(|declaration| match declaration {
            DataTypeDeclarationKind::Simple(d) => d.type_name.to_string(),
            DataTypeDeclarationKind::Enumeration(d) => d.type_name.to_string(),
            DataTypeDeclarationKind::Structure(d) => d.type_name.to_string(),
            DataTypeDeclarationKind::Array(d) => d.type_name.to_string(),
            _ => String::new(),
        })
        .collect();
    assert_eq!(names, vec!["a", "b", "c", "e"]);
}

#[test]
fn lower_type_block_when_only_a_terminator_then_no_declarations() {
    let parse = parse_source_file("TYPE ; END_TYPE", &ParseOptions::all());
    let block = parse.root.first_child().expect("a block");
    assert_eq!(
        lower_type_block(&LowerCx::new(file()), &block)
            .map(|declarations| declarations.len())
            .ok(),
        Some(0)
    );
}

#[test]
fn lower_library_when_type_block_then_one_element_for_each_declaration() {
    let parse = parse_source_file(
        "TYPE a : INT; b : REAL; END_TYPE TYPE c : (X, Y); END_TYPE",
        &ParseOptions::all(),
    );
    let library = crate::lower::lower_library(&parse, &file()).expect("a library");
    assert_eq!(library.elements.len(), 3);
    assert!(library.elements.iter().all(|element| matches!(
        element,
        ironplc_dsl::common::LibraryElementKind::DataTypeDeclaration(_)
    )));
}

// ---- Positions.

#[test]
fn lower_type_declaration_when_elementary_type_then_the_keyword_is_positioned() {
    let simple = variant!(
        declaration("tod := TOD#10:00"),
        DataTypeDeclarationKind::Simple
    );
    let name = simple.type_name.name.span.clone();
    assert_eq!((name.start, name.end, name.file_id), (5, 6, file()));
    let initializer = variant!(simple.spec_and_init, InitialValueAssignmentKind::Simple);
    let keyword = initializer.type_name.span();
    assert_eq!(
        (keyword.start, keyword.end, keyword.file_id),
        (9, 12, file())
    );
}

// ---- Dispositions.

#[test]
fn disposition_when_type_node_then_lowered_by_the_type_area() {
    for kind in [
        K::TypeBlock,
        K::TypeDecl,
        K::ArrayType,
        K::Subrange,
        K::StringType,
        K::RefType,
        K::ParamsType,
        K::SubrangeType,
        K::EnumType,
        K::EnumValue,
        K::StructType,
        K::UnionType,
        K::StructMember,
    ] {
        assert_eq!(
            disposition(kind),
            Disposition::Lowered(Area::Type),
            "{kind:?}"
        );
    }
    assert_eq!(disposition(K::Initializer), Disposition::Structural);
}

// ---- Long lists.

#[test]
fn lower_type_block_when_very_many_declarations_and_members_then_lowered_by_iteration_on_the_smallest_stack(
) {
    let counts = std::thread::Builder::new()
        .stack_size(1024 * 1024)
        .spawn(|| {
            let many: String = (0..50_000)
                .map(|n| format!("t{n} : INT := {n};"))
                .collect::<Vec<_>>()
                .join(" ");
            let members: String = (0..50_000)
                .map(|n| format!("m{n} : INT;"))
                .collect::<Vec<_>>()
                .join(" ");
            let declared = declarations(&many).len();
            let structure = declarations(&format!("s : STRUCT {members} END_STRUCT;"));
            let member_count = match structure.first() {
                Some(DataTypeDeclarationKind::Structure(structure)) => structure.elements.len(),
                _ => 0,
            };
            (declared, member_count)
        })
        .ok()
        .and_then(|thread| thread.join().ok());
    assert_eq!(counts, Some((50_000, 50_000)));
}
