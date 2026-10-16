//! The questions an enumeration answers about being a number.

use super::*;

fn enumeration(strict: bool) -> IntermediateType {
    IntermediateType::Enumeration {
        underlying_type: Box::new(IntermediateType::Int {
            size: ByteSized::B8,
        }),
        base_type: Box::new(IntermediateType::UInt {
            size: ByteSized::B16,
        }),
        numbers: vec![0, 1],
        strict,
    }
}

#[test]
fn is_strict_enumeration_when_enumeration_then_its_strictness() {
    assert!(enumeration(true).is_strict_enumeration());
    assert!(!enumeration(false).is_strict_enumeration());
}

#[test]
fn is_strict_enumeration_when_not_an_enumeration_then_false() {
    assert!(!IntermediateType::Bool.is_strict_enumeration());
    assert!(!IntermediateType::Int {
        size: ByteSized::B16
    }
    .is_strict_enumeration());
}

#[test]
fn stored_as_when_enumeration_then_underlying_type_if_strict_and_base_type_if_not() {
    assert_eq!(
        enumeration(true).stored_as(),
        &IntermediateType::Int {
            size: ByteSized::B8
        }
    );
    assert_eq!(
        enumeration(false).stored_as(),
        &IntermediateType::UInt {
            size: ByteSized::B16
        }
    );
}

#[test]
fn stored_as_when_not_an_enumeration_then_itself() {
    assert_eq!(IntermediateType::Bool.stored_as(), &IntermediateType::Bool);
}
