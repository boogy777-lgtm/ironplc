//! The values written where an enumeration is expected.
//!
//! A place that stores or compares an enumeration knows which one it expects
//! (`position::Position` lists the places). The value written there has to be
//! one of it.
//!
//! - A bare name that the expected enumeration does not declare is P2006, as
//!   it is in an initial value. A name that no enumeration declares is not a
//!   value at all; `report_unknown_value` reports it once, wherever it stands.
//! - A value written with the type of another enumeration is P4035 where
//!   nothing else compares it with the expected type. An assignment to a
//!   variable and an argument of a function type their operands and report the
//!   mismatch themselves, so those places leave it to them.
//! - An enumeration that is not strict is its base integer type and holds a
//!   value of any other enumeration: `check_value` reports neither of the two
//!   for it, in every place alike.

use ironplc_dsl::{
    common::{EnumeratedValue, TypeName},
    core::{Id, Located},
    diagnostic::Diagnostic,
    type_id::TypeId,
};
use ironplc_problems::text;

use super::RuleDeclaredEnumeratedValues;
use crate::type_environment::ValueOwners;

/// The enumeration a place expects.
#[derive(Clone, Copy)]
pub(super) struct Expected<'a> {
    /// The declaration: the enumeration itself, whichever alias names it.
    declaration: TypeId,
    values: &'a [Id],
    /// Whether the enumeration is strict (`IntermediateType::is_strict_enumeration`,
    /// the one answer to that question). One that is not strict is its base
    /// integer type, which holds the value of any enumeration.
    strict: bool,
}

/// Whether the place compares a value of another enumeration with what it
/// expects, or another check does.
#[derive(Clone, Copy, PartialEq, Eq)]
pub(super) enum OtherEnumeration {
    /// This rule reports it (P4035).
    Report,
    /// The check of the types of the operands reports it.
    ReportedByTypes,
}

impl<'a> RuleDeclaredEnumeratedValues<'a> {
    /// The enumeration the type `id` is, or `None` when it is no enumeration.
    pub(super) fn expected_of(&self, id: TypeId) -> Option<Expected<'a>> {
        let types = self.context.types();
        let declaration = types.enumeration_declaration(id)?;
        let attributes = types.get_by_id(declaration)?;
        Some(Expected {
            declaration,
            values: attributes.enumerated_values.as_slice(),
            strict: attributes.representation.is_strict_enumeration(),
        })
    }

    /// The enumeration the type `name` names, or `None`.
    fn expected_named(&self, name: &TypeName) -> Option<Expected<'a>> {
        self.expected_of(self.context.types().id_of(name)?)
    }

    /// Whether a bare name that is not a value of `expected` is refused: it
    /// is a value of another enumeration, which only a strict enumeration
    /// refuses. This and [`Self::check_value`] are the one point where a value
    /// of another enumeration is judged, so every position answers alike.
    pub(super) fn refuses_other_enumeration(&self, expected: Expected<'_>, name: &Id) -> bool {
        expected.strict
            && self.is_declared_by_named_enumeration(name)
            && !expected.values.contains(name)
    }

    /// Whether an initial value of the enumeration the type `type_name` names
    /// is a name of another enumeration that the enumeration holds because it
    /// is not strict.
    pub(super) fn holds_other_enumeration(&self, type_name: &TypeName, name: &Id) -> bool {
        self.expected_named(type_name)
            .is_some_and(|expected| !expected.strict && self.is_declared_by_named_enumeration(name))
    }

    /// Checks that `value`, written where `expected` is expected, is one of it.
    ///
    /// An enumeration that is not strict is its base integer type, as in
    /// CODESYS, and holds a value of any other enumeration: nothing is
    /// reported here, wherever the value is written. A fault of the value
    /// itself (a type that is not declared or not an enumeration, a value the
    /// type does not declare, a name no enumeration declares) is reported
    /// elsewhere and is the same for both.
    pub(super) fn check_value(
        &mut self,
        expected: Expected<'a>,
        value: &EnumeratedValue,
        other: OtherEnumeration,
    ) {
        if !expected.strict {
            return;
        }
        match &value.type_name {
            None => {
                // A name no enumeration declares is reported by itself.
                if self.refuses_other_enumeration(expected, &value.value) {
                    self.diagnostics.push(self.value_not_defined(value));
                }
            }
            Some(qualifier) if other == OtherEnumeration::Report => {
                // A qualifier that is not an enumeration, or is not declared,
                // is reported with the value.
                let Some(written) = self.expected_named(qualifier) else {
                    return;
                };
                if written.declaration != expected.declaration {
                    let types = self.context.types();
                    let name_of = |declaration: TypeId| {
                        types
                            .name_of(declaration)
                            .map_or_else(String::new, ToString::to_string)
                    };
                    self.diagnostics.push(
                        Diagnostic::text(
                            value.span(),
                            text::assignment_type_mismatch_value_of_another_enumeration(),
                        )
                        .with_context("target_type", &name_of(expected.declaration))
                        .with_context("value_type", &name_of(written.declaration)),
                    );
                }
            }
            Some(_) => {}
        }
    }

    /// Whether an enumeration declared by name declares `value`.
    fn is_declared_by_named_enumeration(&self, value: &Id) -> bool {
        !matches!(
            self.context.types().enumerations_declaring(value),
            ValueOwners::None
        )
    }

    /// Reports a name that is written as an enumeration value and that no
    /// enumeration of the library lists. It is the same problem as a variable that is not
    /// declared (P4007): the name means nothing. Code generation has no
    /// ordinal for it.
    pub(super) fn report_unknown_value(&mut self, value: &EnumeratedValue) {
        if value.type_name.is_some() {
            return;
        }
        if !self.listed_values.contains(&value.value)
            && !self.is_declared_by_named_enumeration(&value.value)
        {
            self.diagnostics.push(
                Diagnostic::text(value.span(), text::variable_undefined_undefined_variable())
                    .with_context_id("variable", &value.value),
            );
        }
    }
}
