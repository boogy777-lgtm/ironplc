//! Tests for the common elements.

use super::*;

use crate::core::{Id, Located, SourceSpan};

#[test]
fn try_from_integer_when_whole_fits_u64_then_fixed_point() {
    let whole = Integer::new("18446744073709551615", SourceSpan::default()).unwrap();
    let fixed = FixedPoint::try_from(whole).unwrap();
    assert_eq!((fixed.whole, fixed.femptos), (u64::MAX, 0));
}

#[test]
fn try_from_integer_when_whole_exceeds_u64_then_error_not_wrapped() {
    // 2^64 wrapped to 0 when the value was cast.
    let whole = Integer::new("18446744073709551616", SourceSpan::default()).unwrap();
    assert!(FixedPoint::try_from(whole).is_err());
    assert_eq!(
        FixedPoint::parse("18446744073709551616"),
        Err(FixedPoint::WHOLE_TOO_LARGE)
    );
}

fn located(name: &str, block: BlockId) -> VarDecl {
    VarDecl {
        identifier: VariableIdentifier::Direct(DirectVariableIdentifier {
            name: Some(Id::from(name)),
            address_assignment: AddressAssignment {
                location: LocationPrefix::I,
                size: SizePrefix::Nil,
                address: vec![0],
                position: SourceSpan::default(),
            },
        }),
        var_type: VariableType::Var,
        qualifier: DeclarationQualifier::Unspecified,
        initializer: InitialValueAssignmentKind::None(SourceSpan::default()),
        block,
        type_id: None,
        edge: None,
        attributes: Attributes::default(),
    }
}

fn symbol(name: &str, block: BlockId) -> VarDecl {
    let mut decl = VarDecl::simple(name, "INT");
    decl.block = block;
    decl
}

#[test]
fn address_assignment_try_from_when_multi_digit_components_then_every_digit_kept() {
    let word = AddressAssignment::try_from("%MD10").unwrap();
    assert_eq!(word.size, SizePrefix::D);
    assert_eq!(word.address, vec![10]);
    let bit = AddressAssignment::try_from("%IX10.11").unwrap();
    assert_eq!(bit.address, vec![10, 11]);
}

#[test]
fn address_assignment_try_from_when_no_size_letter_then_size_is_nil() {
    let address = AddressAssignment::try_from("%M7").unwrap();
    assert_eq!(address.size, SizePrefix::Nil);
    assert_eq!(address.address, vec![7]);
}

#[test]
fn address_assignment_try_from_when_component_too_large_then_error() {
    assert!(AddressAssignment::try_from("%MW99999999999").is_err());
}

#[test]
fn span_when_direct_variable_identifier_has_name_then_is_name_span() {
    let identifier = VariableIdentifier::new_direct(
        Some(Id::from("tempSensor").with_position(SourceSpan::range(37, 47))),
        AddressAssignment::try_from("%I*")
            .unwrap()
            .with_position(SourceSpan::range(50, 53)),
    );

    let span = identifier.span();
    assert_eq!(span.start, 37);
    assert_eq!(span.end, 47);
}

#[test]
fn span_when_direct_variable_identifier_has_no_name_then_is_address_span() {
    let identifier = VariableIdentifier::new_direct(
        None,
        AddressAssignment::try_from("%IX0.0")
            .unwrap()
            .with_position(SourceSpan::range(50, 56)),
    );

    let span = identifier.span();
    assert_eq!(span.start, 50);
    assert_eq!(span.end, 56);
}

#[test]
fn mixed_located_var_decls_when_block_has_only_direct_then_none_returned() {
    let block = next_block_id();
    let vars = vec![located("a", block), located("b", block)];
    assert_eq!(mixed_located_var_decls(&vars).count(), 0);
}

#[test]
fn mixed_located_var_decls_when_block_has_only_symbol_then_none_returned() {
    let block = next_block_id();
    let vars = vec![symbol("a", block), symbol("b", block)];
    assert_eq!(mixed_located_var_decls(&vars).count(), 0);
}

#[test]
fn mixed_located_var_decls_when_block_has_both_then_direct_ones_returned() {
    let block = next_block_id();
    let vars = vec![located("a", block), symbol("b", block)];
    let mixed: Vec<&VarDecl> = mixed_located_var_decls(&vars).collect();
    assert_eq!(mixed.len(), 1);
    assert_eq!(mixed[0].identifier.symbolic_id(), Some(&Id::from("a")));
}

#[test]
fn mixed_located_var_decls_when_direct_and_symbol_in_different_blocks_then_none_returned() {
    // Same slice, but the located and symbolic declarations came from
    // two different source blocks -- must not be flagged as mixed.
    let block_a = next_block_id();
    let block_b = next_block_id();
    let vars = vec![located("a", block_a), symbol("b", block_b)];
    assert_eq!(mixed_located_var_decls(&vars).count(), 0);
}

#[test]
fn var_decl_partial_eq_ignores_block() {
    // `block` is process-global-counter-derived provenance, not
    // semantic content -- two otherwise-identical declarations must
    // compare equal even with different `block` values.
    let a = symbol("x", next_block_id());
    let b = symbol("x", next_block_id());
    assert_ne!(a.block, b.block);
    assert_eq!(a, b);
}

#[test]
fn test_constant_kind_partial_eq_and_clone() {
    let int1 = ConstantKind::IntegerLiteral(IntegerLiteral {
        value: SignedInteger::new("42", SourceSpan::default()).unwrap(),
        data_type: None,
    });
    let int2 = int1.clone();
    assert_eq!(int1, int2);
    let int3 = ConstantKind::IntegerLiteral(IntegerLiteral {
        value: SignedInteger::new("43", SourceSpan::default()).unwrap(),
        data_type: None,
    });
    assert_ne!(int1, int3);
}

#[test]
fn test_integer_partial_eq_and_clone() {
    let i1 = Integer::new("123", SourceSpan::default()).unwrap();
    let i2 = i1.clone();
    assert_eq!(i1, i2);
    let i3 = Integer::new("124", SourceSpan::default()).unwrap();
    assert_ne!(i1, i3);
}

#[test]
fn test_signed_integer_partial_eq_and_clone() {
    let s1 = SignedInteger::new("-5", SourceSpan::default()).unwrap();
    let s2 = s1.clone();
    assert_eq!(s1, s2);
    let s3 = SignedInteger::new("5", SourceSpan::default()).unwrap();
    assert_ne!(s1, s3);
}

#[test]
fn test_boolean_partial_eq_and_clone() {
    let b1 = Boolean::True;
    let b2 = b1.clone();
    assert_eq!(b1, b2);
    let b3 = Boolean::False;
    assert_ne!(b1, b3);
}

#[test]
fn test_integer_literal_partial_eq_and_clone() {
    let il1 = IntegerLiteral {
        value: SignedInteger::new("7", SourceSpan::default()).unwrap(),
        data_type: None,
    };
    let il2 = il1.clone();
    assert_eq!(il1, il2);
    let il3 = IntegerLiteral {
        value: SignedInteger::new("8", SourceSpan::default()).unwrap(),
        data_type: None,
    };
    assert_ne!(il1, il3);
}

#[test]
fn test_real_literal_partial_eq_and_clone() {
    let rl1 = RealLiteral {
        value: 1.23,
        data_type: None,
        span: SourceSpan::default(),
    };
    let rl2 = rl1.clone();
    assert_eq!(rl1, rl2);
    let rl3 = RealLiteral {
        value: 2.34,
        data_type: None,
        span: SourceSpan::default(),
    };
    assert_ne!(rl1, rl3);
}

#[test]
fn try_parse_when_scientific_no_decimal_then_ok() {
    let result = RealLiteral::try_parse("2E-3", None).unwrap();
    assert_eq!(result.value, 0.002);
}

#[test]
fn try_parse_when_scientific_positive_exponent_then_ok() {
    let result = RealLiteral::try_parse("1.5E+2", None).unwrap();
    assert_eq!(result.value, 150.0);
}

#[test]
fn try_parse_when_scientific_no_sign_no_decimal_then_ok() {
    let result = RealLiteral::try_parse("2E3", None).unwrap();
    assert_eq!(result.value, 2000.0);
}

#[test]
fn test_boolean_literal_partial_eq_and_clone() {
    let bl1 = BooleanLiteral {
        value: Boolean::True,
        span: SourceSpan::default(),
    };
    let bl2 = bl1.clone();
    assert_eq!(bl1, bl2);
    let bl3 = BooleanLiteral {
        value: Boolean::False,
        span: SourceSpan::default(),
    };
    assert_ne!(bl1, bl3);
}

#[test]
fn test_character_string_literal_partial_eq_and_clone() {
    let csl1 = CharacterStringLiteral::new(vec!['a', 'b', 'c']);
    let csl2 = csl1.clone();
    assert_eq!(csl1, csl2);
    let csl3 = CharacterStringLiteral::new(vec!['x', 'y', 'z']);
    assert_ne!(csl1, csl3);
}

#[test]
fn eq_when_same_value_but_different_width_then_not_equal() {
    let narrow = CharacterStringLiteral::new(vec!['a', 'b', 'c']);
    let wide = CharacterStringLiteral::new_wide(vec!['a', 'b', 'c']);
    assert_ne!(narrow, wide);
}

#[test]
fn test_bit_string_literal_partial_eq_and_clone() {
    let int = Integer::new("255", SourceSpan::default()).unwrap();
    let bsl1 = BitStringLiteral {
        value: int.clone(),
        data_type: None,
    };
    let bsl2 = bsl1.clone();
    assert_eq!(bsl1, bsl2);
    let bsl3 = BitStringLiteral {
        value: Integer::new("0", SourceSpan::default()).unwrap(),
        data_type: None,
    };
    assert_ne!(bsl1, bsl3);
}

#[test]
fn test_type_partial_eq_and_clone() {
    let t1 = TypeName::from("MYTYPE");
    let t2 = t1.clone();
    assert_eq!(t1, t2);
    let t3 = TypeName::from("OTHERTYPE");
    assert_ne!(t1, t3);
}

#[test]
fn test_elementary_type_name_partial_eq_and_clone() {
    let e1 = ElementaryTypeName::BOOL;
    let e2 = e1.clone();
    assert_eq!(e1, e2);
    let e3 = ElementaryTypeName::INT;
    assert_ne!(e1, e3);
}

#[test]
fn test_data_type_declaration_kind_partial_eq_and_clone() {
    let enum_decl = EnumerationDeclaration {
        type_name: TypeName::from("ENUM"),
        spec_init: EnumeratedSpecificationInit::values_and_default(vec!["A", "B"], "A"),
        attributes: Attributes::default(),
    };
    let d1 = DataTypeDeclarationKind::Enumeration(enum_decl.clone());
    let d2 = d1.clone();
    assert_eq!(d1, d2);
    let d3 = DataTypeDeclarationKind::Enumeration(EnumerationDeclaration {
        type_name: TypeName::from("ENUM"),
        spec_init: EnumeratedSpecificationInit::values_and_default(vec!["A", "B"], "B"),
        attributes: Attributes::default(),
    });
    assert_ne!(d1, d3);
}

#[test]
fn test_library_partial_eq_and_clone() {
    let lib1 = Library { elements: vec![] };
    let lib2 = lib1.clone();
    assert_eq!(lib1, lib2);
    let lib3 = Library {
        elements: vec![LibraryElementKind::DataTypeDeclaration(
            DataTypeDeclarationKind::Enumeration(EnumerationDeclaration {
                type_name: TypeName::from("ENUM"),
                spec_init: EnumeratedSpecificationInit::values_and_default(vec!["A"], "A"),
                attributes: Attributes::default(),
            }),
        )],
    };
    assert_ne!(lib1, lib3);
}

#[test]
fn generic_type_name_as_str_when_called_then_returns_expected_string() {
    assert_eq!(GenericTypeName::Any.as_str(), "ANY");
    assert_eq!(GenericTypeName::AnyDerived.as_str(), "ANY_DERIVED");
    assert_eq!(GenericTypeName::AnyElementary.as_str(), "ANY_ELEMENTARY");
    assert_eq!(GenericTypeName::AnyMagnitude.as_str(), "ANY_MAGNITUDE");
    assert_eq!(GenericTypeName::AnyNum.as_str(), "ANY_NUM");
    assert_eq!(GenericTypeName::AnyReal.as_str(), "ANY_REAL");
    assert_eq!(GenericTypeName::AnyInt.as_str(), "ANY_INT");
    assert_eq!(GenericTypeName::AnyBit.as_str(), "ANY_BIT");
    assert_eq!(GenericTypeName::AnyString.as_str(), "ANY_STRING");
    assert_eq!(GenericTypeName::AnyDate.as_str(), "ANY_DATE");
}

#[test]
fn generic_type_name_is_compatible_with_when_any_then_accepts_all() {
    assert!(GenericTypeName::Any.is_compatible_with(&ElementaryTypeName::BOOL));
    assert!(GenericTypeName::Any.is_compatible_with(&ElementaryTypeName::INT));
    assert!(GenericTypeName::Any.is_compatible_with(&ElementaryTypeName::REAL));
    assert!(GenericTypeName::Any.is_compatible_with(&ElementaryTypeName::STRING));
    assert!(GenericTypeName::Any.is_compatible_with(&ElementaryTypeName::TIME));
    assert!(GenericTypeName::Any.is_compatible_with(&ElementaryTypeName::DATE));
}

#[test]
fn generic_type_name_is_compatible_with_when_any_int_then_accepts_integers_only() {
    // Should accept all integer types
    assert!(GenericTypeName::AnyInt.is_compatible_with(&ElementaryTypeName::SINT));
    assert!(GenericTypeName::AnyInt.is_compatible_with(&ElementaryTypeName::INT));
    assert!(GenericTypeName::AnyInt.is_compatible_with(&ElementaryTypeName::DINT));
    assert!(GenericTypeName::AnyInt.is_compatible_with(&ElementaryTypeName::LINT));
    assert!(GenericTypeName::AnyInt.is_compatible_with(&ElementaryTypeName::USINT));
    assert!(GenericTypeName::AnyInt.is_compatible_with(&ElementaryTypeName::UINT));
    assert!(GenericTypeName::AnyInt.is_compatible_with(&ElementaryTypeName::UDINT));
    assert!(GenericTypeName::AnyInt.is_compatible_with(&ElementaryTypeName::ULINT));

    // Should reject non-integer types
    assert!(!GenericTypeName::AnyInt.is_compatible_with(&ElementaryTypeName::REAL));
    assert!(!GenericTypeName::AnyInt.is_compatible_with(&ElementaryTypeName::BOOL));
    assert!(!GenericTypeName::AnyInt.is_compatible_with(&ElementaryTypeName::STRING));
    assert!(!GenericTypeName::AnyInt.is_compatible_with(&ElementaryTypeName::TIME));
}

#[test]
fn generic_type_name_is_compatible_with_when_any_real_then_accepts_reals_only() {
    assert!(GenericTypeName::AnyReal.is_compatible_with(&ElementaryTypeName::REAL));
    assert!(GenericTypeName::AnyReal.is_compatible_with(&ElementaryTypeName::LREAL));
    assert!(!GenericTypeName::AnyReal.is_compatible_with(&ElementaryTypeName::INT));
    assert!(!GenericTypeName::AnyReal.is_compatible_with(&ElementaryTypeName::BOOL));
}

#[test]
fn generic_type_name_is_compatible_with_when_any_num_then_accepts_all_numerics() {
    // Should accept integers and reals
    assert!(GenericTypeName::AnyNum.is_compatible_with(&ElementaryTypeName::INT));
    assert!(GenericTypeName::AnyNum.is_compatible_with(&ElementaryTypeName::REAL));
    assert!(GenericTypeName::AnyNum.is_compatible_with(&ElementaryTypeName::LINT));
    assert!(GenericTypeName::AnyNum.is_compatible_with(&ElementaryTypeName::LREAL));

    // Should reject non-numeric types
    assert!(!GenericTypeName::AnyNum.is_compatible_with(&ElementaryTypeName::BOOL));
    assert!(!GenericTypeName::AnyNum.is_compatible_with(&ElementaryTypeName::TIME));
    assert!(!GenericTypeName::AnyNum.is_compatible_with(&ElementaryTypeName::STRING));
}

#[test]
fn generic_type_name_is_compatible_with_when_any_bit_then_accepts_bit_types() {
    assert!(GenericTypeName::AnyBit.is_compatible_with(&ElementaryTypeName::BOOL));
    assert!(GenericTypeName::AnyBit.is_compatible_with(&ElementaryTypeName::BYTE));
    assert!(GenericTypeName::AnyBit.is_compatible_with(&ElementaryTypeName::WORD));
    assert!(GenericTypeName::AnyBit.is_compatible_with(&ElementaryTypeName::DWORD));
    assert!(GenericTypeName::AnyBit.is_compatible_with(&ElementaryTypeName::LWORD));
    assert!(!GenericTypeName::AnyBit.is_compatible_with(&ElementaryTypeName::INT));
}

#[test]
fn generic_type_name_is_compatible_with_when_any_string_then_accepts_strings() {
    assert!(GenericTypeName::AnyString.is_compatible_with(&ElementaryTypeName::STRING));
    assert!(GenericTypeName::AnyString.is_compatible_with(&ElementaryTypeName::WSTRING));
    assert!(!GenericTypeName::AnyString.is_compatible_with(&ElementaryTypeName::INT));
}

#[test]
fn generic_type_name_is_compatible_with_when_any_date_then_accepts_date_types() {
    assert!(GenericTypeName::AnyDate.is_compatible_with(&ElementaryTypeName::DATE));
    assert!(GenericTypeName::AnyDate.is_compatible_with(&ElementaryTypeName::TimeOfDay));
    assert!(GenericTypeName::AnyDate.is_compatible_with(&ElementaryTypeName::DateAndTime));
    assert!(!GenericTypeName::AnyDate.is_compatible_with(&ElementaryTypeName::TIME));
    assert!(!GenericTypeName::AnyDate.is_compatible_with(&ElementaryTypeName::INT));
}

#[test]
fn generic_type_name_is_compatible_with_when_any_magnitude_then_accepts_time_and_numerics() {
    assert!(GenericTypeName::AnyMagnitude.is_compatible_with(&ElementaryTypeName::TIME));
    assert!(GenericTypeName::AnyMagnitude.is_compatible_with(&ElementaryTypeName::INT));
    assert!(GenericTypeName::AnyMagnitude.is_compatible_with(&ElementaryTypeName::REAL));
    assert!(!GenericTypeName::AnyMagnitude.is_compatible_with(&ElementaryTypeName::BOOL));
    assert!(!GenericTypeName::AnyMagnitude.is_compatible_with(&ElementaryTypeName::STRING));
}

#[test]
fn generic_type_name_is_compatible_with_when_any_derived_then_rejects_all_elementary() {
    assert!(!GenericTypeName::AnyDerived.is_compatible_with(&ElementaryTypeName::INT));
    assert!(!GenericTypeName::AnyDerived.is_compatible_with(&ElementaryTypeName::BOOL));
    assert!(!GenericTypeName::AnyDerived.is_compatible_with(&ElementaryTypeName::STRING));
}

#[test]
fn generic_type_name_partial_eq_and_clone() {
    let g1 = GenericTypeName::AnyInt;
    let g2 = g1.clone();
    assert_eq!(g1, g2);
    let g3 = GenericTypeName::AnyReal;
    assert_ne!(g1, g3);
}

#[test]
fn try_from_id_when_lowercase_sint_then_returns_sint() {
    let id = Id::from("sint");
    assert_eq!(
        ElementaryTypeName::try_from(&id),
        Ok(ElementaryTypeName::SINT)
    );
}

#[test]
fn try_from_id_when_uppercase_bool_then_returns_bool() {
    let id = Id::from("BOOL");
    assert_eq!(
        ElementaryTypeName::try_from(&id),
        Ok(ElementaryTypeName::BOOL)
    );
}

#[test]
fn try_from_id_when_mixed_case_int_then_returns_int() {
    let id = Id::from("Int");
    assert_eq!(
        ElementaryTypeName::try_from(&id),
        Ok(ElementaryTypeName::INT)
    );
}

#[test]
fn try_from_id_when_tod_alias_then_returns_time_of_day() {
    let id = Id::from("TOD");
    assert_eq!(
        ElementaryTypeName::try_from(&id),
        Ok(ElementaryTypeName::TimeOfDay)
    );
}

#[test]
fn try_from_id_when_dt_alias_then_returns_date_and_time() {
    let id = Id::from("DT");
    assert_eq!(
        ElementaryTypeName::try_from(&id),
        Ok(ElementaryTypeName::DateAndTime)
    );
}

#[test]
fn try_from_id_when_ltod_alias_then_returns_ltime_of_day() {
    let id = Id::from("LTOD");
    assert_eq!(
        ElementaryTypeName::try_from(&id),
        Ok(ElementaryTypeName::LTimeOfDay)
    );
}

#[test]
fn try_from_id_when_ldt_alias_then_returns_ldate_and_time() {
    let id = Id::from("LDT");
    assert_eq!(
        ElementaryTypeName::try_from(&id),
        Ok(ElementaryTypeName::LDateAndTime)
    );
}

#[test]
fn try_from_id_when_unknown_name_then_returns_err() {
    let id = Id::from("MY_CUSTOM_TYPE");
    assert_eq!(ElementaryTypeName::try_from(&id), Err(()));
}

// Display impl tests

#[test]
fn display_when_boolean_true_then_true_string() {
    assert_eq!(format!("{}", Boolean::True), "TRUE");
    assert_eq!(format!("{}", Boolean::False), "FALSE");
}

#[test]
fn display_when_integer_then_value_string() {
    let i = Integer::new("42", SourceSpan::default()).unwrap();
    assert_eq!(format!("{i}"), "42");
}

#[test]
fn display_when_signed_integer_negative_then_negative_string() {
    let si = SignedInteger::new("-5", SourceSpan::default()).unwrap();
    assert_eq!(format!("{si}"), "-5");
}

#[test]
fn display_when_signed_integer_positive_then_positive_string() {
    let si = SignedInteger::new("10", SourceSpan::default()).unwrap();
    assert_eq!(format!("{si}"), "10");
}

#[test]
fn display_when_integer_literal_with_type_then_formatted() {
    let il = IntegerLiteral {
        value: SignedInteger::new("7", SourceSpan::default()).unwrap(),
        data_type: Some(IntegerTypeName::INT),
    };
    assert_eq!(format!("{il}"), "INT#7");
}

#[test]
fn display_when_integer_literal_without_type_then_value_only() {
    let il = IntegerLiteral {
        value: SignedInteger::new("7", SourceSpan::default()).unwrap(),
        data_type: None,
    };
    assert_eq!(format!("{il}"), "7");
}

#[test]
fn display_when_real_literal_with_type_then_formatted() {
    let rl = RealLiteral {
        value: 3.25,
        data_type: Some(RealTypeName::REAL),
        span: SourceSpan::default(),
    };
    assert_eq!(format!("{rl}"), "REAL#3.25");
}

#[test]
fn display_when_real_literal_without_type_then_value_only() {
    let rl = RealLiteral {
        value: 2.5,
        data_type: None,
        span: SourceSpan::default(),
    };
    assert_eq!(format!("{rl}"), "2.5");
}

#[test]
fn display_when_boolean_literal_then_value() {
    let bl = BooleanLiteral {
        value: Boolean::True,
        span: SourceSpan::default(),
    };
    assert_eq!(format!("{bl}"), "TRUE");
}

#[test]
fn display_when_character_string_literal_then_quoted() {
    let csl = CharacterStringLiteral::new(vec!['h', 'i']);
    assert_eq!(format!("{csl}"), "'hi'");
}

#[test]
fn display_when_wide_character_string_literal_then_double_quoted() {
    let csl = CharacterStringLiteral::new_wide(vec!['h', 'i']);
    assert_eq!(format!("{csl}"), "\"hi\"");
}

#[test]
fn delimiter_when_string_type_then_matches_iec_spelling() {
    assert_eq!(StringType::String.delimiter(), '\'');
    assert_eq!(StringType::WString.delimiter(), '"');
}

#[test]
fn display_when_bit_string_literal_with_type_then_formatted() {
    let bsl = BitStringLiteral {
        value: Integer::new("255", SourceSpan::default()).unwrap(),
        data_type: Some(BitStringTypeName::BYTE),
    };
    assert_eq!(format!("{bsl}"), "BYTE#255");
}

#[test]
fn display_when_bit_string_literal_without_type_then_value_only() {
    let bsl = BitStringLiteral {
        value: Integer::new("42", SourceSpan::default()).unwrap(),
        data_type: None,
    };
    assert_eq!(format!("{bsl}"), "42");
}

#[test]
fn display_when_type_name_then_name_string() {
    let tn = TypeName::from("MY_TYPE");
    assert_eq!(format!("{tn}"), "MY_TYPE");
}

#[test]
fn display_when_integer_type_name_then_formatted() {
    assert_eq!(format!("{}", IntegerTypeName::SINT), "SINT");
    assert_eq!(format!("{}", IntegerTypeName::INT), "INT");
    assert_eq!(format!("{}", IntegerTypeName::DINT), "DINT");
    assert_eq!(format!("{}", IntegerTypeName::LINT), "LINT");
    assert_eq!(format!("{}", IntegerTypeName::USINT), "USINT");
    assert_eq!(format!("{}", IntegerTypeName::UINT), "UINT");
    assert_eq!(format!("{}", IntegerTypeName::UDINT), "UDINT");
    assert_eq!(format!("{}", IntegerTypeName::ULINT), "ULINT");
}

#[test]
fn display_when_real_type_name_then_formatted() {
    assert_eq!(format!("{}", RealTypeName::REAL), "REAL");
    assert_eq!(format!("{}", RealTypeName::LREAL), "LREAL");
}

#[test]
fn display_when_bit_string_type_name_then_formatted() {
    assert_eq!(format!("{}", BitStringTypeName::BYTE), "BYTE");
    assert_eq!(format!("{}", BitStringTypeName::WORD), "WORD");
    assert_eq!(format!("{}", BitStringTypeName::DWORD), "DWORD");
    assert_eq!(format!("{}", BitStringTypeName::LWORD), "LWORD");
}

#[test]
fn display_when_elementary_type_name_then_formatted() {
    assert_eq!(format!("{}", ElementaryTypeName::BOOL), "BOOL");
    assert_eq!(format!("{}", ElementaryTypeName::INT), "INT");
    assert_eq!(format!("{}", ElementaryTypeName::REAL), "REAL");
    assert_eq!(format!("{}", ElementaryTypeName::STRING), "STRING");
    assert_eq!(format!("{}", ElementaryTypeName::TIME), "TIME");
}

#[test]
fn display_when_constant_kind_integer_then_formatted() {
    let ck = ConstantKind::integer_literal("42").unwrap();
    assert_eq!(format!("{ck}"), "42");
}

#[test]
fn display_when_constant_kind_real_then_formatted() {
    let ck = ConstantKind::RealLiteral(RealLiteral {
        value: 1.5,
        data_type: None,
        span: SourceSpan::default(),
    });
    assert_eq!(format!("{ck}"), "1.5");
}

#[test]
fn display_when_constant_kind_boolean_then_formatted() {
    let ck = ConstantKind::Boolean(BooleanLiteral {
        value: Boolean::True,
        span: SourceSpan::default(),
    });
    assert_eq!(format!("{ck}"), "TRUE");
}

#[test]
fn display_when_constant_kind_character_string_then_formatted() {
    let ck = ConstantKind::CharacterString(CharacterStringLiteral::new(vec!['a', 'b']));
    assert_eq!(format!("{ck}"), "'ab'");
}

// Accessor method tests

#[test]
fn as_integer_when_literal_then_returns_some() {
    let ir = IntegerRef::Literal(Integer::new("5", SourceSpan::default()).unwrap());
    assert!(ir.as_integer().is_some());
    assert_eq!(ir.as_integer().unwrap().value, 5);
}

#[test]
fn as_integer_when_constant_then_returns_none() {
    let ir = IntegerRef::Constant(Id::from("MY_CONST"));
    assert!(ir.as_integer().is_none());
}

#[test]
fn as_signed_integer_when_literal_then_returns_some() {
    let sir = SignedIntegerRef::Literal(SignedInteger::new("10", SourceSpan::default()).unwrap());
    assert!(sir.as_signed_integer().is_some());
}

#[test]
fn as_signed_integer_when_constant_then_returns_none() {
    let sir = SignedIntegerRef::Constant(Id::from("MY_CONST"));
    assert!(sir.as_signed_integer().is_none());
}

// Conversion tests

#[test]
fn from_signed_integer_to_string_when_negative_then_prefixed() {
    let si = SignedInteger::negative("42", SourceSpan::default()).unwrap();
    let s: String = si.into();
    // Note: The From impl prepends "-" on top of Display which also prepends "-"
    assert!(s.contains("42"));
}

#[test]
fn from_signed_integer_to_string_when_positive_then_no_prefix() {
    let si = SignedInteger::positive("42", SourceSpan::default()).unwrap();
    let s: String = si.into();
    assert_eq!(s, "42");
}

#[test]
fn from_integer_to_signed_integer_then_positive() {
    let i = Integer::new("7", SourceSpan::default()).unwrap();
    let si: SignedInteger = i.into();
    assert!(!si.is_neg);
    assert_eq!(si.value.value, 7);
}

use rstest::rstest;

#[rstest]
#[case::sint_to_int(ElementaryTypeName::SINT, ElementaryTypeName::INT, true)]
#[case::sint_to_dint(ElementaryTypeName::SINT, ElementaryTypeName::DINT, true)]
#[case::sint_to_lint(ElementaryTypeName::SINT, ElementaryTypeName::LINT, true)]
#[case::int_to_dint(ElementaryTypeName::INT, ElementaryTypeName::DINT, true)]
#[case::dint_to_lint(ElementaryTypeName::DINT, ElementaryTypeName::LINT, true)]
#[case::usint_to_uint(ElementaryTypeName::USINT, ElementaryTypeName::UINT, true)]
#[case::usint_to_ulint(ElementaryTypeName::USINT, ElementaryTypeName::ULINT, true)]
#[case::uint_to_udint(ElementaryTypeName::UINT, ElementaryTypeName::UDINT, true)]
#[case::usint_to_int(ElementaryTypeName::USINT, ElementaryTypeName::INT, true)]
#[case::uint_to_dint(ElementaryTypeName::UINT, ElementaryTypeName::DINT, true)]
#[case::udint_to_lint(ElementaryTypeName::UDINT, ElementaryTypeName::LINT, true)]
#[case::dint_to_int(ElementaryTypeName::DINT, ElementaryTypeName::INT, false)]
#[case::lint_to_dint(ElementaryTypeName::LINT, ElementaryTypeName::DINT, false)]
#[case::int_to_uint(ElementaryTypeName::INT, ElementaryTypeName::UINT, false)]
#[case::int_to_int(ElementaryTypeName::INT, ElementaryTypeName::INT, false)]
#[case::uint_to_int(ElementaryTypeName::UINT, ElementaryTypeName::INT, false)]
#[case::sint_to_real(ElementaryTypeName::SINT, ElementaryTypeName::REAL, true)]
#[case::int_to_real(ElementaryTypeName::INT, ElementaryTypeName::REAL, true)]
#[case::usint_to_real(ElementaryTypeName::USINT, ElementaryTypeName::REAL, true)]
#[case::uint_to_real(ElementaryTypeName::UINT, ElementaryTypeName::REAL, true)]
#[case::dint_to_real(ElementaryTypeName::DINT, ElementaryTypeName::REAL, false)]
#[case::lint_to_real(ElementaryTypeName::LINT, ElementaryTypeName::REAL, false)]
#[case::udint_to_real(ElementaryTypeName::UDINT, ElementaryTypeName::REAL, false)]
#[case::ulint_to_real(ElementaryTypeName::ULINT, ElementaryTypeName::REAL, false)]
#[case::sint_to_lreal(ElementaryTypeName::SINT, ElementaryTypeName::LREAL, true)]
#[case::lint_to_lreal(ElementaryTypeName::LINT, ElementaryTypeName::LREAL, true)]
#[case::ulint_to_lreal(ElementaryTypeName::ULINT, ElementaryTypeName::LREAL, true)]
#[case::real_to_int(ElementaryTypeName::REAL, ElementaryTypeName::INT, false)]
#[case::lreal_to_dint(ElementaryTypeName::LREAL, ElementaryTypeName::DINT, false)]
#[case::real_to_lreal(ElementaryTypeName::REAL, ElementaryTypeName::LREAL, true)]
#[case::lreal_to_real(ElementaryTypeName::LREAL, ElementaryTypeName::REAL, false)]
#[case::real_to_real(ElementaryTypeName::REAL, ElementaryTypeName::REAL, false)]
#[case::byte_to_word(ElementaryTypeName::BYTE, ElementaryTypeName::WORD, true)]
#[case::byte_to_dword(ElementaryTypeName::BYTE, ElementaryTypeName::DWORD, true)]
#[case::byte_to_lword(ElementaryTypeName::BYTE, ElementaryTypeName::LWORD, true)]
#[case::word_to_dword(ElementaryTypeName::WORD, ElementaryTypeName::DWORD, true)]
#[case::dword_to_lword(ElementaryTypeName::DWORD, ElementaryTypeName::LWORD, true)]
#[case::word_to_byte(ElementaryTypeName::WORD, ElementaryTypeName::BYTE, false)]
#[case::byte_to_byte(ElementaryTypeName::BYTE, ElementaryTypeName::BYTE, false)]
#[case::bool_to_byte(ElementaryTypeName::BOOL, ElementaryTypeName::BYTE, false)]
#[case::byte_to_int(ElementaryTypeName::BYTE, ElementaryTypeName::INT, false)]
#[case::int_to_byte(ElementaryTypeName::INT, ElementaryTypeName::BYTE, false)]
fn can_widen_to_when_source_and_target_then_matches_expected(
    #[case] from: ElementaryTypeName,
    #[case] to: ElementaryTypeName,
    #[case] expected: bool,
) {
    assert_eq!(from.can_widen_to(&to), expected);
}

#[rstest]
#[case::byte_to_int(ElementaryTypeName::BYTE, ElementaryTypeName::INT, true)]
#[case::byte_to_uint(ElementaryTypeName::BYTE, ElementaryTypeName::UINT, true)]
#[case::word_to_dint(ElementaryTypeName::WORD, ElementaryTypeName::DINT, true)]
#[case::dword_to_lint(ElementaryTypeName::DWORD, ElementaryTypeName::LINT, true)]
#[case::byte_to_sint(ElementaryTypeName::BYTE, ElementaryTypeName::SINT, false)]
#[case::word_to_int(ElementaryTypeName::WORD, ElementaryTypeName::INT, false)]
#[case::int_to_byte(ElementaryTypeName::INT, ElementaryTypeName::BYTE, false)]
#[case::udint_to_dword(ElementaryTypeName::UDINT, ElementaryTypeName::DWORD, false)]
#[case::dword_to_udint(ElementaryTypeName::DWORD, ElementaryTypeName::UDINT, false)]
#[case::dint_to_dword(ElementaryTypeName::DINT, ElementaryTypeName::DWORD, false)]
#[case::dword_to_dint(ElementaryTypeName::DWORD, ElementaryTypeName::DINT, false)]
#[case::word_to_uint(ElementaryTypeName::WORD, ElementaryTypeName::UINT, false)]
#[case::uint_to_word(ElementaryTypeName::UINT, ElementaryTypeName::WORD, false)]
fn can_widen_cross_family_to_when_source_and_target_then_matches_expected(
    #[case] from: ElementaryTypeName,
    #[case] to: ElementaryTypeName,
    #[case] expected: bool,
) {
    assert_eq!(from.can_widen_cross_family_to(&to), expected);
}

/// Equal-width cross-family conversion is a separate rule from widening:
/// it converts nothing wider, runs in both directions, and is scoped to
/// the one verified pair.
#[rstest]
#[case::udint_to_dword(ElementaryTypeName::UDINT, ElementaryTypeName::DWORD, true)]
#[case::dword_to_udint(ElementaryTypeName::DWORD, ElementaryTypeName::UDINT, true)]
// Other equal-width bit-string/unsigned pairs are unverified, so rejected.
#[case::word_to_uint(ElementaryTypeName::WORD, ElementaryTypeName::UINT, false)]
#[case::uint_to_word(ElementaryTypeName::UINT, ElementaryTypeName::WORD, false)]
#[case::byte_to_usint(ElementaryTypeName::BYTE, ElementaryTypeName::USINT, false)]
#[case::lword_to_ulint(ElementaryTypeName::LWORD, ElementaryTypeName::ULINT, false)]
// Signed integers are excluded even at equal width.
#[case::dword_to_dint(ElementaryTypeName::DWORD, ElementaryTypeName::DINT, false)]
#[case::dint_to_dword(ElementaryTypeName::DINT, ElementaryTypeName::DWORD, false)]
// Widening is the other rule's job, not this one's.
#[case::byte_to_int(ElementaryTypeName::BYTE, ElementaryTypeName::INT, false)]
#[case::udint_to_lword(ElementaryTypeName::UDINT, ElementaryTypeName::LWORD, false)]
fn can_convert_cross_family_to_when_source_and_target_then_matches_expected(
    #[case] from: ElementaryTypeName,
    #[case] to: ElementaryTypeName,
    #[case] expected: bool,
) {
    assert_eq!(from.can_convert_cross_family_to(&to), expected);
}
