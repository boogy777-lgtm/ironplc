//! What the type checks compare about a value: the type an expression's
//! value has, and whether it may be used where a given type is required.
//!
//! [`type_compat::are_types_compatible`] states the relation between two
//! type *names*: exact matches, the generic categories, literal inference and
//! implicit widening. That relation is about scalars. A value whose type is
//! not a scalar -- a whole array, a structure, an enumeration, a function
//! block instance -- is only ever the type it is, and it may have no name at
//! all (`a : ARRAY[1..2] OF DINT`). This module classifies a value by its
//! [`ExprType`] so each check asks one question of it, [`check`], and a
//! composite value reaching a scalar position is a mismatch rather than
//! something to skip.
//!
//! ```ignore
//! VAR a : ARRAY[1..2] OF DINT; n : DINT; END_VAR
//!     n := LEN(a);   (* P4026: ANY_STRING is not ARRAY[1..2] OF DINT *)
//!     n := a;        (* P4035 *)
//! ```

use ironplc_dsl::common::{ConstantKind, GenericTypeName, TypeName};
use ironplc_dsl::textual::{Expr, ExprKind, ExprType};
use ironplc_dsl::type_id::TypeId;
use ironplc_parser::options::CompilerOptions;

use crate::intermediate_type::IntermediateType;
use crate::resolution::Resolved;
use crate::type_compat::{are_types_compatible, is_checkable_type};
use crate::type_environment::TypeEnvironment;
use crate::value_range;

/// The type of a value, as the checks compare it.
#[derive(Debug, PartialEq)]
pub(crate) enum ValueType {
    /// A scalar, named as [`are_types_compatible`] understands it: an
    /// elementary type or a user-defined one it compares by name, a generic
    /// category for an untyped literal, or the base type of a subrange.
    Scalar(TypeName),
    /// A value that is not a scalar: a whole array, a structure, an
    /// enumeration or a function block instance.
    Composite(TypeId),
}

/// What a place does with the value it is given.
///
/// This is the one place the language states when an enumeration is a number.
/// An enumeration is the number it stands for (its base type, `INT` unless the
/// declaration names another) where the place only holds the value: stores it,
/// passes it, compares it, selects it, converts it. A strict enumeration (see
/// [`IntermediateType::is_strict_enumeration`]) is no number where the place
/// computes a new number from it: an arithmetic operator, the function form of
/// one, a numerical function, the counter of a `FOR`. CODESYS refuses those
/// for an enumeration declared `strict`, and an operation on a named value has
/// no meaning. An enumeration that is not strict is its base type there too.
///
/// A place states its use as data -- a row of the function environment, an
/// operator -- and every check asks [`ValueUse::refuses`], so a new place
/// cannot answer differently from the others.
#[derive(Debug, Clone, Copy, PartialEq, Eq, Default)]
pub enum ValueUse {
    /// The place stores, passes, compares, selects or converts the value, and
    /// the value stays what it is.
    #[default]
    Holds,
    /// The place makes a new number from the value.
    Computes,
}

impl ValueUse {
    /// Whether a value of `representation` is refused by a place that makes
    /// this use of it: a strict enumeration is no number where the place
    /// computes.
    pub fn refuses(self, representation: &IntermediateType) -> bool {
        self == ValueUse::Computes && representation.is_strict_enumeration()
    }
}

/// A value used where its type is not accepted.
#[derive(Debug, PartialEq)]
pub(crate) struct Mismatch {
    /// The value's type as a diagnostic shows it.
    pub(crate) actual: String,
}

/// The name the name-based type relations -- [`are_types_compatible`] and
/// the arithmetic overloads -- know a value of type `expr_type` by.
///
/// An expression's type is its [`ExprType`]; this name is derived from it
/// each time it is asked for, so the two cannot disagree:
///
/// - an untyped literal is its generic category (`ANY_INT`);
/// - an elementary type, or an alias of one, is the elementary type's name;
/// - a string is `STRING` or `WSTRING`, sized or not;
/// - a reference is known by the type it references, as a `REF_TO`
///   parameter records its type;
/// - a subrange is its own name, or its base type's when it has none;
/// - an enumeration that is not strict is its base type's name;
/// - any other type is its own name, and has none when it is anonymous.
///
/// `None` for `NULL`, which is not of one type.
pub fn operand_type_name(types: &TypeEnvironment, expr_type: &ExprType) -> Option<TypeName> {
    match expr_type {
        ExprType::Literal(generic) => Some(generic.clone().into()),
        ExprType::Null => None,
        ExprType::Concrete(id) => operand_name_of(types, *id),
    }
}

fn operand_name_of(types: &TypeEnvironment, id: TypeId) -> Option<TypeName> {
    let representation = &types.get_by_id(id)?.representation;
    match representation {
        IntermediateType::Bool
        | IntermediateType::Int { .. }
        | IntermediateType::UInt { .. }
        | IntermediateType::Real { .. }
        | IntermediateType::Bytes { .. }
        | IntermediateType::Time { .. }
        | IntermediateType::Date { .. }
        | IntermediateType::TimeOfDay { .. }
        | IntermediateType::DateAndTime { .. } => types.elementary_type_name_for(representation),
        IntermediateType::String { char_width, .. } => {
            Some(TypeName::from(if char_width.is_wide() {
                "wstring"
            } else {
                "string"
            }))
        }
        IntermediateType::Reference { .. } => operand_name_of(types, types.referenced_type(id)?),
        // A named subrange is known by its name; one spelled out in place
        // (`x : INT(-100..100)`) by its base type's.
        IntermediateType::Subrange { base_type, .. } => types
            .name_of(id)
            .cloned()
            .or_else(|| types.elementary_type_name_for(base_type)),
        // A strict enumeration is known by its own name, which no overload
        // judges. One that is not strict is the number it stands for.
        IntermediateType::Enumeration { base_type, .. }
            if !representation.is_strict_enumeration() =>
        {
            types.elementary_type_name_for(base_type)
        }
        IntermediateType::Enumeration { .. }
        | IntermediateType::Structure { .. }
        | IntermediateType::Array { .. }
        | IntermediateType::FunctionBlock { .. }
        | IntermediateType::Function { .. } => types.name_of(id).cloned(),
    }
}

/// The type of the result of a call whose declared result is a generic
/// category, from the types of the inputs declared with that category, in the
/// order of the arguments. `None` when no input has a type.
///
/// The result takes the type of the first input, as the inputs of numbers
/// bind: how two different integer types meet is not decided here. A strict
/// enumeration changes that in one way, because it is the number it stands
/// for only where a value is held:
///
/// - every input is the same strict enumeration: the result is that
///   enumeration, as `SEL(g, a, b)` gives back one of the values it was given;
/// - an enumeration among inputs that are not all that strict enumeration:
///   each stands as its number, so the result is a number and not an
///   enumeration. `MAX(x, 1)` is an `INT`, and storing it into a strict
///   enumeration is refused by the check of the place that stores it, as any
///   `INT` is. An enumeration that is not strict is its number at every input.
pub(crate) fn generic_result(types: &TypeEnvironment, inputs: &[ExprType]) -> Option<ExprType> {
    let first = inputs.first()?;
    let declarations: Vec<Option<TypeId>> = inputs
        .iter()
        .map(|input| strict_enumeration_declaration(types, input))
        .collect();
    let same_enumeration = declarations
        .first()
        .copied()
        .flatten()
        .is_some_and(|head| declarations.iter().all(|d| *d == Some(head)));
    if same_enumeration {
        return Some(first.clone());
    }
    match first {
        ExprType::Concrete(id) if types.enumeration_declaration(*id).is_some() => {
            number_of(types, *id)
        }
        ExprType::Concrete(_) | ExprType::Literal(_) | ExprType::Null => Some(first.clone()),
    }
}

/// The strict enumeration a value of type `expr_type` is, aliases resolved, or
/// `None` when it is not an enumeration or the enumeration is not strict.
fn strict_enumeration_declaration(types: &TypeEnvironment, expr_type: &ExprType) -> Option<TypeId> {
    let ExprType::Concrete(id) = expr_type else {
        return None;
    };
    let declaration = types.enumeration_declaration(*id)?;
    types
        .get_by_id(declaration)?
        .representation
        .is_strict_enumeration()
        .then_some(declaration)
}

/// The type of the number the enumeration `id` stands for, or `None` when the
/// base type has no type of its own in the environment.
fn number_of(types: &TypeEnvironment, id: TypeId) -> Option<ExprType> {
    let number = types.get_by_id(id)?.representation.number_type()?;
    let name = types.elementary_type_name_for(number)?;
    types.id_of(&name).map(ExprType::Concrete)
}

/// Classifies the value of `expr`, or `None` when the analyzer resolved no
/// type for it.
pub(crate) fn of(types: &TypeEnvironment, expr: &Expr) -> Option<ValueType> {
    let expr_type = expr.expr_type.as_ref()?;
    let by_name = || operand_type_name(types, expr_type).map(ValueType::Scalar);
    let id = match expr_type {
        ExprType::Concrete(id) => id,
        // `NULL` is accepted for any reference, so there is nothing to
        // compare it with.
        ExprType::Null => return None,
        ExprType::Literal(_) => return by_name(),
    };
    let Some(attributes) = types.get_by_id(*id) else {
        return by_name();
    };
    match &attributes.representation {
        IntermediateType::Array { .. }
        | IntermediateType::Structure { .. }
        | IntermediateType::Enumeration { .. }
        | IntermediateType::FunctionBlock { .. } => Some(ValueType::Composite(*id)),
        IntermediateType::Subrange { base_type, .. } => types
            .elementary_type_name_for(base_type)
            .map(ValueType::Scalar)
            .or_else(by_name),
        // Compared by name (see `operand_type_name`): an elementary type by
        // its own name, a string by `STRING` or `WSTRING`, a reference by
        // the name of the type it references. A function is not the type of
        // any value, so it never gets here with a name that matters.
        IntermediateType::Bool
        | IntermediateType::Int { .. }
        | IntermediateType::UInt { .. }
        | IntermediateType::Real { .. }
        | IntermediateType::Bytes { .. }
        | IntermediateType::Time { .. }
        | IntermediateType::Date { .. }
        | IntermediateType::TimeOfDay { .. }
        | IntermediateType::DateAndTime { .. }
        | IntermediateType::String { .. }
        | IntermediateType::Reference { .. }
        | IntermediateType::Function { .. } => by_name(),
    }
}

/// Checks that the value of `expr` may be used where `expected` is
/// required by a place that only holds it (see [`ValueUse::Holds`]): an
/// assignment, a parameter of a function that does not compute, a condition.
/// `Ok` when it may, and when the analyzer resolved no type for the value,
/// since there is nothing to compare.
pub(crate) fn check(
    types: &TypeEnvironment,
    expected: &TypeName,
    expr: &Expr,
    options: &CompilerOptions,
) -> Result<(), Mismatch> {
    check_use(types, ValueUse::Holds, expected, expr, options)
}

/// [`check`] for a place that makes `value_use` of the value: where it
/// computes, an enumeration is no number (see [`ValueUse::refuses`]).
pub(crate) fn check_use(
    types: &TypeEnvironment,
    value_use: ValueUse,
    expected: &TypeName,
    expr: &Expr,
    options: &CompilerOptions,
) -> Result<(), Mismatch> {
    // A required type that is a declaration with an error, or a name that is
    // not declared, is the error type: the declaration that uses it is already
    // reported, so there is nothing to compare the value with.
    if requires_error_type(types, expected) {
        return Ok(());
    }
    if let Some(ExprType::Concrete(id)) = &expr.expr_type {
        if types.id_of(expected) == Some(*id) {
            return Ok(());
        }
    }
    // A required enumeration that is not strict takes the numbers its base
    // type takes.
    let required = required_number(types, expected);
    match of(types, expr) {
        None => Ok(()),
        Some(ValueType::Scalar(actual)) => {
            if are_types_compatible(&required, &actual, options)
                || constant_accepted(types, expected, expr)
            {
                Ok(())
            } else {
                Err(Mismatch {
                    actual: actual.to_string(),
                })
            }
        }
        Some(ValueType::Composite(id)) => {
            if composite_accepted(types, expected, id)
                || (!refused(types, value_use, id)
                    && number_accepted(types, &required, id, options))
            {
                Ok(())
            } else {
                Err(Mismatch {
                    actual: describe(types, id),
                })
            }
        }
    }
}

/// The type a value must be to be stored where `expected` is required, as a
/// number: the base type of an enumeration that is not strict, else `expected`
/// itself. A strict enumeration holds only its own values (and the constants
/// that name one, see [`constant_accepted`]).
fn required_number(types: &TypeEnvironment, expected: &TypeName) -> TypeName {
    let Some(attributes) = types.get(expected) else {
        return expected.clone();
    };
    if attributes.representation.is_strict_enumeration() {
        return expected.clone();
    }
    attributes
        .representation
        .number_type()
        .and_then(|number| types.elementary_type_name_for(number))
        .unwrap_or_else(|| expected.clone())
}

/// Whether `expected` is a type that lists the numbers it holds (see
/// [`value_range::ValueSet::is_listed`]) and `expr` is an integer constant.
///
/// Which numbers such a type holds is a question about the constant's value,
/// and `rule_constant_range` asks it wherever a constant is stored, so this
/// relation only says that an integer constant is a number to such a type. It
/// is a constant by what the analysis folded it to, not by how it was typed:
/// `INT#1` is one, and `i`, an `INT` variable, is not.
fn constant_accepted(types: &TypeEnvironment, expected: &TypeName, expr: &Expr) -> bool {
    is_integer_constant(expr)
        && types.get(expected).is_some_and(|attributes| {
            value_range::numbers_of(&attributes.representation).is_some_and(|set| set.is_listed())
        })
}

/// Whether `expr` is an integer constant, however it was spelled and typed.
fn is_integer_constant(expr: &Expr) -> bool {
    match &expr.kind {
        ExprKind::Const(ConstantKind::IntegerLiteral(_)) => true,
        ExprKind::Expression(inner) => is_integer_constant(inner),
        _ => false,
    }
}

/// Whether the place that makes `value_use` of a value refuses the value of
/// type `id` (see [`ValueUse::refuses`]).
fn refused(types: &TypeEnvironment, value_use: ValueUse, id: TypeId) -> bool {
    types
        .get_by_id(id)
        .is_some_and(|attributes| value_use.refuses(&attributes.representation))
}

/// Whether the composite value of type `id` is the number it stands for where
/// `expected` asks for one: an enumeration is its base type (`INT` unless the
/// declaration names another) wherever that type is accepted, by the same
/// relation as any value of it.
fn number_accepted(
    types: &TypeEnvironment,
    expected: &TypeName,
    id: TypeId,
    options: &CompilerOptions,
) -> bool {
    types
        .get_by_id(id)
        .and_then(|attributes| attributes.representation.number_type())
        .and_then(|number| types.elementary_type_name_for(number))
        .is_some_and(|number| are_types_compatible(expected, &number, options))
}

/// Whether the relation judges a value stored into `target`: the types
/// [`are_types_compatible`] knows by name, and the types that hold numbers
/// but have no elementary name, an enumeration among them.
pub(crate) fn judges(types: &TypeEnvironment, target: &TypeName) -> bool {
    is_checkable_type(target)
        || types
            .get(target)
            .is_some_and(|attributes| attributes.representation.is_enumeration())
}

/// Whether `expected` names no type a value can be compared with: a
/// declaration with an error, or a name that is not declared and is not a
/// generic category either (`ANY_INT` is in no environment).
fn requires_error_type(types: &TypeEnvironment, expected: &TypeName) -> bool {
    let generic = GenericTypeName::try_from(&expected.name).is_ok();
    match types.lookup(expected) {
        Resolved::Valid(_) => false,
        Resolved::Failed => true,
        Resolved::Absent => !generic,
    }
}

/// Whether a composite value of type `id` is accepted where `expected` is
/// required, when `expected` is not `id` itself.
///
/// An array or a structure is accepted where an array or structure of the
/// same shape is, as a whole-aggregate assignment is (P2037): an inline
/// `ARRAY[1..2] OF DINT` passes for a parameter declared with a named array
/// type of that shape. An enumeration is accepted where it, or an alias of it,
/// is required, and a function block instance is only ever its own type. Of
/// the generic categories only `ANY` accepts a composite, since only it is not
/// limited to elementary types; `SIZEOF(s)` and `__TYPEOF(s)` take a structure
/// variable.
fn composite_accepted(types: &TypeEnvironment, expected: &TypeName, id: TypeId) -> bool {
    if let Ok(generic) = GenericTypeName::try_from(&expected.name) {
        return generic == GenericTypeName::Any;
    }
    let (Some(required), Some(actual)) = (types.get(expected), types.get_by_id(id)) else {
        return false;
    };
    let same_enumeration = types
        .id_of(expected)
        .and_then(|required| types.enumeration_declaration(required))
        .is_some_and(|required| Some(required) == types.enumeration_declaration(id));
    same_enumeration
        || (matches!(
            actual.representation,
            IntermediateType::Array { .. } | IntermediateType::Structure { .. }
        ) && required.representation == actual.representation)
}

/// The type `id` identifies, as a diagnostic shows it: its name when it
/// has one, else its shape (`ARRAY[1..2] OF DINT`).
pub(crate) fn describe(types: &TypeEnvironment, id: TypeId) -> String {
    if let Some(name) = types.name_of(id) {
        return name.to_string();
    }
    match types.get_by_id(id) {
        Some(attributes) => describe_representation(types, &attributes.representation),
        None => "an unknown type".to_string(),
    }
}

fn describe_representation(types: &TypeEnvironment, representation: &IntermediateType) -> String {
    if let Some(name) = types.elementary_type_name_for(representation) {
        return name.to_string().to_uppercase();
    }
    match representation {
        IntermediateType::Array {
            element_type,
            dimensions,
        } => {
            let element = describe_representation(types, element_type);
            if dimensions.is_empty() {
                return format!("ARRAY OF {element}");
            }
            let bounds: Vec<String> = dimensions
                .iter()
                .map(|dimension| format!("{}..{}", dimension.lower, dimension.upper))
                .collect();
            format!("ARRAY[{}] OF {element}", bounds.join(", "))
        }
        IntermediateType::String {
            max_len,
            char_width,
        } => {
            let keyword = if char_width.is_wide() {
                "WSTRING"
            } else {
                "STRING"
            };
            match max_len {
                Some(len) => format!("{keyword}[{len}]"),
                None => keyword.to_string(),
            }
        }
        IntermediateType::Structure { .. } => "a structure".to_string(),
        IntermediateType::Enumeration { .. } => "an enumeration".to_string(),
        IntermediateType::FunctionBlock { name, .. } => name.clone(),
        IntermediateType::Reference { target_type } => {
            format!("REF_TO {}", describe_representation(types, target_type))
        }
        IntermediateType::Subrange { base_type, .. } => {
            format!(
                "a subrange of {}",
                describe_representation(types, base_type)
            )
        }
        IntermediateType::Function { .. } => "a function".to_string(),
        // Every elementary representation is named above; these only reach
        // here in a representation the elementary table does not list.
        IntermediateType::Bool
        | IntermediateType::Int { .. }
        | IntermediateType::UInt { .. }
        | IntermediateType::Real { .. }
        | IntermediateType::Bytes { .. }
        | IntermediateType::Time { .. }
        | IntermediateType::Date { .. }
        | IntermediateType::TimeOfDay { .. }
        | IntermediateType::DateAndTime { .. } => "an elementary type".to_string(),
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::intermediate_type::{ArrayDimension, ByteSized};
    use crate::type_attributes::TypeAttributes;
    use crate::type_environment::TypeEnvironmentBuilder;
    use ironplc_container::CharWidth;
    use ironplc_dsl::core::SourceSpan;

    fn anonymous(types: &mut TypeEnvironment, representation: IntermediateType) -> TypeId {
        types.insert_anonymous(TypeAttributes::new(SourceSpan::default(), representation))
    }

    fn environment() -> TypeEnvironment {
        TypeEnvironmentBuilder::new()
            .with_elementary_types()
            .build()
            .unwrap()
    }

    #[test]
    fn describe_when_named_type_then_name() {
        let types = environment();
        let dint = types.id_of(&TypeName::from("DINT")).unwrap();

        assert_eq!(describe(&types, dint), "dint");
    }

    #[test]
    fn describe_when_anonymous_array_then_bounds_and_element() {
        let mut types = environment();
        let id = anonymous(
            &mut types,
            IntermediateType::Array {
                element_type: Box::new(IntermediateType::Int {
                    size: ByteSized::B32,
                }),
                dimensions: vec![
                    ArrayDimension { lower: 1, upper: 2 },
                    ArrayDimension { lower: 0, upper: 3 },
                ],
            },
        );

        assert_eq!(describe(&types, id), "ARRAY[1..2, 0..3] OF DINT");
    }

    #[test]
    fn describe_when_array_of_sized_strings_then_string_length() {
        let mut types = environment();
        let id = anonymous(
            &mut types,
            IntermediateType::Array {
                element_type: Box::new(IntermediateType::String {
                    max_len: Some(8),
                    char_width: CharWidth::Wide,
                }),
                dimensions: vec![ArrayDimension { lower: 1, upper: 2 }],
            },
        );

        assert_eq!(describe(&types, id), "ARRAY[1..2] OF WSTRING[8]");
    }

    #[test]
    fn describe_when_anonymous_enumeration_then_kind() {
        let mut types = environment();
        let id = anonymous(
            &mut types,
            IntermediateType::Enumeration {
                underlying_type: Box::new(IntermediateType::Int {
                    size: ByteSized::B8,
                }),
                base_type: Box::new(IntermediateType::Int {
                    size: crate::intermediate_type::ByteSized::B16,
                }),
                numbers: vec![],
                strict: true,
            },
        );

        assert_eq!(describe(&types, id), "an enumeration");
    }

    #[test]
    fn describe_when_id_unknown_then_unknown_type() {
        let types = environment();

        assert_eq!(
            describe(&types, TypeId::from_raw(100_000)),
            "an unknown type"
        );
    }

    /// An enumeration `E` of the values 0 and 1 stored in a byte, a number of
    /// `INT`, strict or not, and a second one `F`.
    fn with_enumerations(strict: bool) -> TypeEnvironment {
        let mut types = environment();
        for name in ["E", "F"] {
            let representation = IntermediateType::Enumeration {
                underlying_type: Box::new(IntermediateType::Int {
                    size: ByteSized::B8,
                }),
                base_type: Box::new(IntermediateType::Int {
                    size: ByteSized::B16,
                }),
                numbers: vec![0, 1],
                strict,
            };
            types.insert_type(
                &TypeName::from(name),
                TypeAttributes::new(SourceSpan::default(), representation),
            );
        }
        types
    }

    fn id_of(types: &TypeEnvironment, name: &str) -> ExprType {
        ExprType::Concrete(types.id_of(&TypeName::from(name)).unwrap())
    }

    #[rstest::rstest]
    #[case::strict(true, "e")]
    #[case::not_strict(false, "int")]
    fn operand_type_name_when_enumeration_then_its_own_name_only_when_strict(
        #[case] strict: bool,
        #[case] expected: &str,
    ) {
        let types = with_enumerations(strict);

        let name = operand_type_name(&types, &id_of(&types, "E"));

        assert_eq!(name, Some(TypeName::from(expected)));
    }

    #[rstest::rstest]
    #[case::strict_same_enumeration_stays(true, &["E", "E"], "E")]
    #[case::strict_other_enumeration_is_a_number(true, &["E", "F"], "INT")]
    #[case::not_strict_same_enumeration_is_a_number(false, &["E", "E"], "INT")]
    #[case::not_strict_other_enumeration_is_a_number(false, &["E", "F"], "INT")]
    fn generic_result_when_inputs_are_enumerations_then_an_enumeration_only_when_all_one_strict_one(
        #[case] strict: bool,
        #[case] inputs: &[&str],
        #[case] expected: &str,
    ) {
        let types = with_enumerations(strict);
        let inputs: Vec<ExprType> = inputs.iter().map(|name| id_of(&types, name)).collect();

        let result = generic_result(&types, &inputs);

        assert_eq!(result, Some(id_of(&types, expected)));
    }

    #[rstest::rstest]
    #[case::strict_holds_only_its_values(true, "E", "INT", false)]
    #[case::not_strict_holds_what_its_base_type_holds(false, "E", "INT", true)]
    #[case::not_strict_refuses_what_its_base_type_refuses(false, "E", "STRING", false)]
    fn check_when_number_stored_into_an_enumeration_then_accepted_only_when_not_strict(
        #[case] strict: bool,
        #[case] target: &str,
        #[case] value_type: &str,
        #[case] accepted: bool,
    ) {
        let types = with_enumerations(strict);
        // A variable, not a constant: a constant is accepted by a strict
        // enumeration when it names a value (see `constant_accepted`).
        let mut expr = Expr::new(ExprKind::named_variable("n"));
        expr.expr_type = types
            .id_of(&TypeName::from(value_type))
            .map(ExprType::Concrete);

        let result = check(
            &types,
            &TypeName::from(target),
            &expr,
            &CompilerOptions::default(),
        );

        assert_eq!(result.is_ok(), accepted);
    }

    fn expression_of(types: &TypeEnvironment, type_name: &str) -> Expr {
        let mut expr = Expr::new(ironplc_dsl::textual::ExprKind::integer_literal("1"));
        expr.expr_type = types
            .id_of(&TypeName::from(type_name))
            .map(ExprType::Concrete);
        expr
    }

    #[test]
    fn check_when_required_type_is_declared_with_an_error_then_ok() {
        let mut types = environment();
        types.insert_failed(&TypeName::from("BROKEN"));
        let expr = expression_of(&types, "STRING");

        let result = check(
            &types,
            &TypeName::from("BROKEN"),
            &expr,
            &CompilerOptions::default(),
        );

        assert!(result.is_ok());
    }

    #[test]
    fn check_when_required_type_is_not_declared_then_ok() {
        let types = environment();
        let expr = expression_of(&types, "STRING");

        let result = check(
            &types,
            &TypeName::from("NOWHERE"),
            &expr,
            &CompilerOptions::default(),
        );

        assert!(result.is_ok());
    }

    #[test]
    fn check_when_required_type_is_a_generic_category_then_still_compared() {
        let types = environment();
        let expr = expression_of(&types, "STRING");

        let result = check(
            &types,
            &TypeName::from("ANY_INT"),
            &expr,
            &CompilerOptions::default(),
        );

        assert!(result.is_err());
    }

    #[test]
    fn check_when_required_type_is_declared_then_still_compared() {
        let types = environment();
        let expr = expression_of(&types, "STRING");

        let result = check(
            &types,
            &TypeName::from("INT"),
            &expr,
            &CompilerOptions::default(),
        );

        assert!(result.is_err());
    }
}
