//! Enumeration support for IEC 61131-3 code generation.
//!
//! Builds an ordinal map from enumeration type declarations and provides
//! helpers to resolve enumeration values to their integer ordinals.
//!
//! See `specs/design/enumeration-codegen.md` for the full design.

use std::collections::HashMap;

use indexmap::IndexMap;

use ironplc_analyzer::{TypeEnvironment, ValueOwners};
use ironplc_dsl::common::{
    DataTypeDeclarationKind, EnumeratedValue, Library, LibraryElementKind, SpecificationKind,
};
use ironplc_dsl::core::Located;
use ironplc_dsl::diagnostic::{Diagnostic, Label};

use super::compile::{OpWidth, Signedness, VarTypeInfo};

/// Which enumeration an unqualified value name denotes, as the type
/// environment answers it (`TypeEnvironment::enumerations_declaring`).
enum ValueOwner {
    /// Exactly one enumeration declares the name: its declared name, upper case.
    One(String),
    /// More than one enumeration declares the name. The analyzer refuses a
    /// program that uses such a name bare, so a use that reaches code
    /// generation is a defect in the compiler.
    Several,
}

/// Pre-computed ordinal mappings for all named enumeration types.
///
/// Built once at codegen entry from the library's type declarations and the
/// type environment, and stored in `CompileContext` for use by all codegen
/// phases.
#[derive(Default)]
pub(crate) struct EnumOrdinalMap {
    /// Maps (declared_name_upper, value_name_upper) → 0-based ordinal, for
    /// each enumeration by the name it was declared with.
    ordinals: HashMap<(String, String), i32>,

    /// Maps the name of an enumeration or of an alias of one, upper case, to
    /// the name the enumeration was declared with: `E#V` names an
    /// enumeration by either.
    declared_as: HashMap<String, String>,

    /// Maps a value_name_upper to the enumeration the type environment says
    /// declares it. The map records that answer and chooses nothing: a name
    /// that several enumerations declare is recorded as such.
    value_owners: HashMap<String, ValueOwner>,

    /// Maps type_name_upper → ordered list of value names (for debug output).
    ///
    /// In the order the types are declared in the library, so the debug
    /// section lists them in the same order on every run.
    pub(crate) definitions: IndexMap<String, Vec<String>>,
}

/// Builds the ordinal map by walking enumeration type declarations in the AST.
///
/// For each `TYPE X : (A, B, C) := A; END_TYPE`, records:
/// - ordinals: (X, A)→0, (X, B)→1, (X, C)→2
/// - declared_as: X→X (and EA→X for `TYPE EA : X;`)
/// - value_owners: A→the enumeration `types` says declares A, and likewise B, C
/// - definitions: X→[A, B, C]
pub(crate) fn build_enum_ordinal_map(library: &Library, types: &TypeEnvironment) -> EnumOrdinalMap {
    let mut ordinals = HashMap::new();
    let mut declared_as = HashMap::new();
    let mut value_owners = HashMap::new();
    let mut definitions = IndexMap::new();

    for element in &library.elements {
        if let LibraryElementKind::DataTypeDeclaration(DataTypeDeclarationKind::Enumeration(decl)) =
            element
        {
            let type_upper = decl.type_name.to_string().to_uppercase();
            if let Some(declared) = types.enumeration_declared_as(&decl.type_name) {
                declared_as.insert(type_upper.clone(), declared.to_string().to_uppercase());
            }

            if let SpecificationKind::Inline(spec_values) = &decl.spec_init.spec {
                let mut value_names = Vec::new();

                // Uses the resolved ordinal (not just declaration
                // position) so explicit values (`member := 5`,
                // an extension) are reflected at runtime,
                // not just at the type-sizing stage.
                let resolved = ironplc_analyzer::resolve_ordinal_values(&spec_values.values);
                for (ev, ordinal) in spec_values.values.iter().zip(resolved) {
                    let val_upper = ev.value.to_string().to_uppercase();
                    let ordinal = ordinal as i32;
                    ordinals.insert((type_upper.clone(), val_upper.clone()), ordinal);
                    if let Some(owner) = value_owner(types, &ev.value) {
                        value_owners.insert(val_upper.clone(), owner);
                    }
                    value_names.push(val_upper);
                }

                definitions.insert(type_upper, value_names);
            }
        }
    }

    EnumOrdinalMap {
        ordinals,
        declared_as,
        value_owners,
        definitions,
    }
}

/// The enumeration the type environment says declares `value`, as the map
/// records it; `None` for a name no enumeration of the environment declares.
fn value_owner(types: &TypeEnvironment, value: &ironplc_dsl::core::Id) -> Option<ValueOwner> {
    match types.enumerations_declaring(value) {
        ValueOwners::None => None,
        ValueOwners::One(owner) => Some(ValueOwner::One(
            owner.enumeration.to_string().to_uppercase(),
        )),
        ValueOwners::Several(_) => Some(ValueOwner::Several),
    }
}

/// Resolves an `EnumeratedValue` AST node to its integer ordinal.
///
/// For qualified values (`COLOR#GREEN`), uses the explicit type name, which
/// may be an alias of the enumeration. For unqualified values (`GREEN`), uses
/// the enumeration the type environment says declares the name.
pub(crate) fn resolve_enum_ordinal(
    map: &EnumOrdinalMap,
    ev: &EnumeratedValue,
) -> Result<i32, Diagnostic> {
    let value_upper = ev.value.to_string().to_uppercase();

    let ordinal_of = |declared: &str| {
        map.ordinals
            .get(&(declared.to_string(), value_upper.clone()))
    };
    if let Some(type_name) = &ev.type_name {
        // Qualified: COLOR#GREEN
        let type_upper = type_name.to_string().to_uppercase();
        map.declared_as
            .get(&type_upper)
            .and_then(|declared| ordinal_of(declared))
            .copied()
            .ok_or_else(|| {
                Diagnostic::not_implemented(Label::span(ev.span(), "Unknown qualified enum value"))
            })
    } else {
        // Unqualified: GREEN
        match map.value_owners.get(&value_upper) {
            Some(ValueOwner::One(declared)) => ordinal_of(declared).copied().ok_or_else(|| {
                Diagnostic::not_implemented(Label::span(ev.span(), "Unknown enum value"))
            }),
            Some(ValueOwner::Several) => Err(Diagnostic::internal_error_at(Label::span(
                ev.span(),
                "Enum value is declared by several enumerations",
            ))),
            None => Err(Diagnostic::not_implemented(Label::span(
                ev.span(),
                "Unknown enum value",
            ))),
        }
    }
}

/// Returns the `VarTypeInfo` for an enumeration variable.
///
/// All enumerations use DINT (W32, Signed, 32-bit) at the codegen level,
/// regardless of the analyzer's underlying type sizing (B8/B16). This avoids
/// unnecessary truncation opcodes since every VM slot is 64 bits wide.
pub(crate) fn enum_var_type_info() -> VarTypeInfo {
    VarTypeInfo {
        op_width: OpWidth::W32,
        signedness: Signedness::Signed,
        storage_bits: 32,
    }
}

/// The ordinal map of a library as parsed: the library is analyzed first, since
/// the map reads the type environment the analysis builds.
#[cfg(test)]
pub(crate) fn ordinal_map_of(library: &Library) -> EnumOrdinalMap {
    let (analyzed, context) = ironplc_analyzer::stages::resolve_types(
        &[library],
        &ironplc_parser::options::CompilerOptions::default(),
    )
    .unwrap();
    build_enum_ordinal_map(&analyzed, context.types())
}

#[cfg(test)]
mod tests {
    use super::*;
    use ironplc_dsl::core::FileId;
    use ironplc_parser::options::CompilerOptions;

    fn parse_library(source: &str) -> Library {
        ironplc_parser::parse_program(source, &FileId::default(), &CompilerOptions::default())
            .unwrap()
    }

    #[test]
    fn build_enum_ordinal_map_when_simple_enum_then_assigns_ordinals() {
        let lib = parse_library(
            "TYPE COLOR : (RED, GREEN, BLUE) := RED; END_TYPE
             PROGRAM main END_PROGRAM",
        );
        let map = ordinal_map_of(&lib);

        assert_eq!(map.ordinals.get(&("COLOR".into(), "RED".into())), Some(&0));
        assert_eq!(
            map.ordinals.get(&("COLOR".into(), "GREEN".into())),
            Some(&1)
        );
        assert_eq!(map.ordinals.get(&("COLOR".into(), "BLUE".into())), Some(&2));
    }

    #[test]
    fn build_enum_ordinal_map_when_multiple_enums_then_maps_all() {
        let lib = parse_library(
            "TYPE COLOR : (RED, GREEN, BLUE) := RED; END_TYPE
             TYPE LEVEL : (LOW, HIGH) := LOW; END_TYPE
             PROGRAM main END_PROGRAM",
        );
        let map = ordinal_map_of(&lib);

        assert_eq!(map.ordinals.len(), 5);
        assert_eq!(map.ordinals.get(&("COLOR".into(), "BLUE".into())), Some(&2));
        assert_eq!(map.ordinals.get(&("LEVEL".into(), "HIGH".into())), Some(&1));
    }

    #[test]
    fn resolve_enum_ordinal_when_unqualified_then_finds_value() {
        let lib = parse_library(
            "TYPE COLOR : (RED, GREEN, BLUE) := RED; END_TYPE
             PROGRAM main END_PROGRAM",
        );
        let map = ordinal_map_of(&lib);

        let ev = EnumeratedValue::new("GREEN");
        let result = resolve_enum_ordinal(&map, &ev).unwrap();
        assert_eq!(result, 1);
    }

    fn qualified(type_name: &str, value: &str) -> EnumeratedValue {
        let mut ev = EnumeratedValue::new(value);
        ev.type_name = Some(ironplc_dsl::common::TypeName::from(type_name));
        ev
    }

    #[test]
    fn resolve_enum_ordinal_when_value_is_shared_and_qualified_then_ordinal_of_its_enumeration() {
        let lib = parse_library(
            "TYPE E1 : (U2, U1); E2 : (U1, U3); END_TYPE
             PROGRAM main END_PROGRAM",
        );
        let map = ordinal_map_of(&lib);

        assert_eq!(
            resolve_enum_ordinal(&map, &qualified("E1", "U1")).unwrap(),
            1
        );
        assert_eq!(
            resolve_enum_ordinal(&map, &qualified("E2", "U1")).unwrap(),
            0
        );
    }

    #[test]
    fn resolve_enum_ordinal_when_qualified_by_an_alias_then_ordinal_of_the_declaration() {
        let lib = parse_library(
            "TYPE E1 : (U2, U1); E2 : (U1, U3); EA : E2; EB : EA; END_TYPE
             PROGRAM main END_PROGRAM",
        );
        let map = ordinal_map_of(&lib);

        assert_eq!(
            resolve_enum_ordinal(&map, &qualified("EA", "U1")).unwrap(),
            0
        );
        assert_eq!(
            resolve_enum_ordinal(&map, &qualified("EB", "U3")).unwrap(),
            1
        );
    }

    #[test]
    fn resolve_enum_ordinal_when_qualified_value_is_not_of_the_enumeration_then_error() {
        let lib = parse_library(
            "TYPE E1 : (U2, U1); E2 : (U1, U3); END_TYPE
             PROGRAM main END_PROGRAM",
        );
        let map = ordinal_map_of(&lib);

        assert!(resolve_enum_ordinal(&map, &qualified("E1", "U3")).is_err());
    }

    #[test]
    fn resolve_enum_ordinal_when_unqualified_value_has_several_enumerations_then_internal_error() {
        let lib = parse_library(
            "TYPE E1 : (U2, U1); E2 : (U1, U3); END_TYPE
             PROGRAM main END_PROGRAM",
        );
        let map = ordinal_map_of(&lib);

        let error = resolve_enum_ordinal(&map, &EnumeratedValue::new("U1")).unwrap_err();

        assert_eq!(error.code, "P9998");
        // The names declared once are not affected.
        assert_eq!(
            resolve_enum_ordinal(&map, &EnumeratedValue::new("U2")).unwrap(),
            0
        );
        assert_eq!(
            resolve_enum_ordinal(&map, &EnumeratedValue::new("U3")).unwrap(),
            1
        );
    }

    #[test]
    fn resolve_enum_ordinal_when_alias_of_an_enumeration_then_its_values_are_not_ambiguous() {
        let lib = parse_library(
            "TYPE E1 : (A, B); EA : E1; END_TYPE
             PROGRAM main END_PROGRAM",
        );
        let map = ordinal_map_of(&lib);

        assert_eq!(
            resolve_enum_ordinal(&map, &EnumeratedValue::new("B")).unwrap(),
            1
        );
    }

    #[test]
    fn enum_var_type_info_when_called_then_returns_dint() {
        let info = enum_var_type_info();
        assert!(matches!(info.op_width, OpWidth::W32));
        assert!(matches!(info.signedness, Signedness::Signed));
        assert_eq!(info.storage_bits, 32);
    }

    #[test]
    fn build_enum_ordinal_map_when_enum_then_stores_definitions() {
        let lib = parse_library(
            "TYPE COLOR : (RED, GREEN, BLUE) := RED; END_TYPE
             PROGRAM main END_PROGRAM",
        );
        let map = ordinal_map_of(&lib);

        assert_eq!(
            map.definitions.get("COLOR"),
            Some(&vec![
                "RED".to_string(),
                "GREEN".to_string(),
                "BLUE".to_string()
            ])
        );
    }

    #[test]
    fn build_enum_ordinal_map_when_enum_then_records_the_enumeration_of_each_value() {
        let lib = parse_library(
            "TYPE COLOR : (RED, GREEN, BLUE) := RED; END_TYPE
             PROGRAM main END_PROGRAM",
        );
        let map = ordinal_map_of(&lib);

        for value in ["RED", "GREEN", "BLUE"] {
            assert!(
                matches!(map.value_owners.get(value), Some(ValueOwner::One(owner)) if owner == "COLOR"),
                "{value}"
            );
        }
    }

    #[test]
    fn build_enum_ordinal_map_when_explicit_values_then_uses_resolved_ordinals() {
        let lib = parse_library(
            "TYPE E_ModeLanguage : (Deutsch := 1, English := 2); END_TYPE
             PROGRAM main END_PROGRAM",
        );
        let map = ordinal_map_of(&lib);

        // Uses the explicit values, not declaration position (which
        // would otherwise give Deutsch=0, English=1).
        assert_eq!(
            map.ordinals
                .get(&("E_MODELANGUAGE".into(), "DEUTSCH".into())),
            Some(&1)
        );
        assert_eq!(
            map.ordinals
                .get(&("E_MODELANGUAGE".into(), "ENGLISH".into())),
            Some(&2)
        );
    }

    #[test]
    fn build_enum_ordinal_map_when_first_explicit_then_continues_for_rest() {
        let lib = parse_library(
            "TYPE E_AssertionType : (Type_UNDEFINED := 0, Type_ANY, Type_BOOL) BYTE; END_TYPE
             PROGRAM main END_PROGRAM",
        );
        let map = ordinal_map_of(&lib);

        assert_eq!(
            map.ordinals
                .get(&("E_ASSERTIONTYPE".into(), "TYPE_UNDEFINED".into())),
            Some(&0)
        );
        assert_eq!(
            map.ordinals
                .get(&("E_ASSERTIONTYPE".into(), "TYPE_ANY".into())),
            Some(&1)
        );
        assert_eq!(
            map.ordinals
                .get(&("E_ASSERTIONTYPE".into(), "TYPE_BOOL".into())),
            Some(&2)
        );
    }
}
