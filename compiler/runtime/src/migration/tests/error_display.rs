//! Tests of the messages of `MigrationError`.

use super::*;

#[test]
fn migration_error_display_when_variant_then_names_the_reason() {
    assert_eq!(
        MigrationError::IncompatibleEntry {
            uid: 7,
            reason: "the variable type changed",
        }
        .to_string(),
        "entity 7 cannot migrate: the variable type changed"
    );
    assert_eq!(
        MigrationError::StringShrink {
            current_length: 5,
            candidate_max_length: 4,
        }
        .to_string(),
        "string value of length 5 does not fit the candidate maximum 4"
    );
    assert_eq!(
        MigrationError::ArrayDescriptorMismatch { uid: 7 }.to_string(),
        "entity 7 has a changed array descriptor"
    );
    assert_eq!(
        MigrationError::TypeChangeUnsupported {
            pairs: vec![
                TypeChangePair {
                    uid: 7,
                    name: Some("Counter".into()),
                    from: FieldType::I32,
                    to: FieldType::String,
                    size_equal: false,
                },
                TypeChangePair {
                    uid: 9,
                    name: None,
                    from: FieldType::I32,
                    to: FieldType::U32,
                    size_equal: true,
                },
            ],
        }
        .to_string(),
        "type changes are outside the migration policy: entity 7 (Counter) I32 -> STRING, entity 9 I32 -> U32"
    );
    assert_eq!(
        MigrationError::PreserveSizeMismatch { uid: 7 }.to_string(),
        "entity 7 cannot preserve its storage: the base and candidate sizes differ"
    );
    assert_eq!(
        MigrationError::UnknownDecisionUid { uid: 7 }.to_string(),
        "migration decision names uid 7, which is not a shared entity whose type changed"
    );
    assert_eq!(
        MigrationError::FbLayoutUnsupported.to_string(),
        "function-block instance layout changed and field UIDs cannot justify the change"
    );
    assert_eq!(
        MigrationError::IndexOutOfRange {
            index: VarIndex::new(3)
        }
        .to_string(),
        "variable index 3 is out of range"
    );
    assert_eq!(
        MigrationError::RegionOutOfRange {
            index: VarIndex::new(3)
        }
        .to_string(),
        "the data-region region for variable 3 is out of range"
    );
    assert_eq!(
        MigrationError::DuplicateUid { uid: 7 }.to_string(),
        "stable variable uid 7 is bound more than once"
    );
}
