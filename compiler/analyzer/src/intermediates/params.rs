//! `PARAMS(n) OF T` type processing.
//!
//! A PARAMS list is addressed by index, like an array, so it is provisionally
//! lowered to the array type `ARRAY[0 .. n-1] OF T`: the element type is the
//! same resolution, the same validation and the same code generation as any
//! other fixed-size array. The variadic call-site semantics of a PARAMS
//! parameter (a call may pass fewer arguments than the list has elements) are
//! not implemented yet; only the declaration itself is.
//!
//! The lowering lives here rather than in the parser so the AST keeps the
//! surface spelling (`PARAMS(3) OF INT`), which is what the renderer writes
//! back.

use ironplc_dsl::common::{
    ArrayBounds, ArrayElementType, ArraySubranges, Integer, IntegerRef, ParamsSpecification,
    SignedInteger, SignedIntegerRef, SpecificationKind, Subrange, TypeName,
};
use ironplc_dsl::core::Located;
use ironplc_dsl::diagnostic::{Diagnostic, Label};
use ironplc_problems::Problem;

use crate::intermediates::array;
use crate::resolution::Failure;
use crate::type_environment::TypeEnvironment;

/// The array bounds a `PARAMS(n) OF T` list lowers to: the single dimension
/// `0 .. n-1`.
///
/// Returns `ArrayDimensionInvalid` (P2024) when the count is not a literal or
/// is zero: a parameter list has at least one element, and a count that never
/// folded to a literal has no bounds to build.
pub fn to_array_subranges(spec: &ParamsSpecification) -> Result<ArraySubranges, Diagnostic> {
    let span = spec.type_name.span();
    let count = match &spec.count {
        IntegerRef::Literal(count) => count.value,
        IntegerRef::Constant(_) => {
            return Err(Diagnostic::problem(
                Problem::ArrayDimensionInvalid,
                Label::span(span, "PARAMS count"),
            )
            .with_help("Give the count as an integer literal."));
        }
    };
    if count == 0 {
        return Err(Diagnostic::problem(
            Problem::ArrayDimensionInvalid,
            Label::span(span, "PARAMS count"),
        )
        .with_help("A PARAMS list has at least one element."));
    }

    let literal = |value: u128| {
        SignedIntegerRef::Literal(SignedInteger {
            value: Integer {
                span: span.clone(),
                value,
            },
            is_neg: false,
        })
    };
    Ok(ArraySubranges {
        bounds: ArrayBounds::Ranges(vec![Subrange {
            start: literal(0),
            end: literal(count - 1),
        }]),
        type_name: ArrayElementType::Named(spec.type_name.clone()),
        ref_to: None,
    })
}

/// Resolves the PARAMS type against the type environment, exactly as the
/// array it lowers to would resolve.
pub fn try_from(
    node_name: &TypeName,
    spec: &ParamsSpecification,
    type_environment: &TypeEnvironment,
) -> Result<array::IntermediateResult, Failure> {
    let subranges = to_array_subranges(spec)?;
    array::try_from(
        node_name,
        &SpecificationKind::Inline(subranges),
        type_environment,
    )
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::intermediate_type::{ArrayDimension, ByteSized, IntermediateType};
    use crate::type_environment::TypeEnvironmentBuilder;
    use ironplc_dsl::core::SourceSpan;
    use ironplc_test::{cast, cast_struct};

    /// REQ-CS-analyzer-002: A PARAMS declaration resolves to the array
    /// `ARRAY[0 .. n-1]` of its element type.
    #[spec_test_macro::spec_test(REQ_CS_analyzer_002)]
    fn try_from_when_params_of_type_then_array_with_zero_based_dimension() {
        let env = TypeEnvironmentBuilder::new()
            .with_elementary_types()
            .build()
            .unwrap();

        let spec = ParamsSpecification {
            count: IntegerRef::Literal(Integer {
                span: SourceSpan::default(),
                value: 3,
            }),
            type_name: TypeName::from("int"),
        };

        let result = try_from(&TypeName::from("MyParams"), &spec, &env).unwrap();

        let attrs = cast!(result, array::IntermediateResult::Type);
        let (element_type, dimensions) = cast_struct!(
            attrs.representation,
            IntermediateType::Array {
                element_type,
                dimensions
            }
        );
        assert_eq!(dimensions, vec![ArrayDimension { lower: 0, upper: 2 }]);
        assert_eq!(
            *element_type,
            IntermediateType::Int {
                size: ByteSized::B16
            }
        );
    }
}
