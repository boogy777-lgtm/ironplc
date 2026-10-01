//! Where a type specification may appear, and what each position accepts.
//!
//! The legacy grammar has a rule for what follows the `:` in each kind of
//! declaration, and the rules differ in which forms of type they take and
//! what initial value may follow. This module is the table of those
//! positions: a [`Context`] says which kinds of type ([`Spec`]) a position
//! accepts, and [`Initial`] says what initial value a kind of type takes there.
//! The type rule in `types` is one rule for every position; a position is a
//! row here. A form a position does not accept is parsed all the same, with an
//! error at it, so the tree keeps its structure.

/// What a type specification turned out to be.
#[derive(Clone, Copy, PartialEq, Eq, Debug)]
pub(super) enum Spec {
    Array,
    /// `STRING` or `WSTRING` with a length.
    String,
    /// `REF_TO`, `REFERENCE TO` or `POINTER TO`.
    Reference,
    Params,
    Enumeration,
    Subrange,
    Struct,
    Union,
    /// A declared type's name.
    Named,
    /// An elementary or generic type's keyword, and a string without a
    /// length. A position that accepts names accepts these.
    Elementary,
    /// A declared type's name with the arguments of a function block
    /// instance.
    Call,
    /// Nothing usable was there; an error has been reported.
    Missing,
}

/// What may follow the `:=` after a kind of type.
#[derive(Clone, Copy, PartialEq, Eq, Debug)]
pub(super) enum Initial {
    /// An expression, a structure value or a qualified enumeration value.
    Expression,
    /// An expression and nothing else.
    Plain,
    /// A constant: a literal, an enumeration value or a structure value.
    Constant,
    /// A literal.
    Literal,
    /// An array initialiser `[...]`.
    Array,
    /// A character string literal.
    String,
    /// `NULL` or `REF(variable)`.
    Reference,
    /// No initial value.
    Forbidden,
}

/// One position: the kinds of type it accepts, and how it reads an initial
/// value.
#[derive(Clone, Copy)]
pub(super) struct Context {
    /// The position declares a type, so `STRUCT` and `UNION` may be written
    /// in place, an enumeration may name values and a base type, and a
    /// reference type has no initial value.
    pub declares: bool,
    /// An initial value after a type name or subrange is a constant, not an
    /// expression.
    pub constants_only: bool,
    /// A structure value `(a := 1)` may initialise a declared type.
    pub structured: bool,
    pub allowed: &'static [Spec],
}

impl Context {
    /// This position, for a variable with a location: it takes the kinds of
    /// type `allowed` and no structure value.
    pub(super) const fn located(self, allowed: &'static [Spec]) -> Context {
        Context {
            allowed,
            structured: false,
            ..self
        }
    }

    pub(super) fn accepts(&self, spec: Spec) -> bool {
        spec == Spec::Missing
            || self.allowed.contains(&spec)
            || (spec == Spec::Elementary && self.allowed.contains(&Spec::Named))
    }

    /// What follows the `:=` after a type of kind `spec` in this position.
    pub(super) fn initial(&self, spec: Spec) -> Initial {
        let open = if self.structured {
            Initial::Expression
        } else {
            Initial::Plain
        };
        match spec {
            Spec::Array => Initial::Array,
            Spec::String => Initial::String,
            Spec::Reference if self.declares => Initial::Forbidden,
            Spec::Reference => Initial::Reference,
            Spec::Params | Spec::Struct | Spec::Union | Spec::Call => Initial::Forbidden,
            Spec::Enumeration => Initial::Constant,
            Spec::Elementary | Spec::Subrange if self.constants_only => Initial::Literal,
            Spec::Named if self.constants_only => Initial::Constant,
            Spec::Elementary | Spec::Subrange | Spec::Named | Spec::Missing => open,
        }
    }
}

const fn position(allowed: &'static [Spec]) -> Context {
    Context {
        declares: false,
        constants_only: false,
        structured: true,
        allowed,
    }
}

use Spec as S;

/// `VAR`, `VAR_INPUT`, `VAR_OUTPUT`.
pub(super) const VARIABLES: Context = position(&[
    S::Array,
    S::String,
    S::Reference,
    S::Params,
    S::Enumeration,
    S::Named,
    S::Call,
]);
/// `VAR_TEMP`, `VAR_STAT`, `VAR_INST`, `VAR_GENERIC` and the `VAR` of a
/// function.
pub(super) const TEMPORARIES: Context =
    position(&[S::Array, S::String, S::Reference, S::Enumeration, S::Named]);
/// `VAR_IN_OUT`.
pub(super) const BORROWED: Context =
    position(&[S::Array, S::String, S::Enumeration, S::Subrange, S::Named]);
/// `VAR_EXTERNAL`.
pub(super) const EXTERNAL: Context = position(&[S::Array, S::Named]);
/// `VAR_GLOBAL`.
pub(super) const GLOBAL: Context = Context {
    structured: false,
    ..position(&[S::Array, S::Named])
};
/// A variable with a complete location (`x AT %IX0.0 : BOOL`).
pub(super) const COMPLETE_ADDRESS: &[Spec] = &[S::Array, S::Enumeration, S::Named];
/// A variable with an incomplete location (`x AT %I* : STRING[10]`).
pub(super) const INCOMPLETE_ADDRESS: &[Spec] =
    &[S::Array, S::String, S::Enumeration, S::Subrange, S::Named];
/// `VAR_CONFIG`.
pub(super) const INSTANCE: Context = position(&[S::Array, S::Named]);
/// A type that a `TYPE` declaration defines.
pub(super) const DECLARED: Context = Context {
    declares: true,
    constants_only: true,
    structured: true,
    allowed: &[
        S::Array,
        S::String,
        S::Reference,
        S::Params,
        S::Enumeration,
        S::Subrange,
        S::Struct,
        S::Union,
        S::Named,
    ],
};
/// A member of a structure or union.
pub(super) const MEMBER: Context = Context {
    constants_only: true,
    ..position(&[S::Array, S::String, S::Enumeration, S::Subrange, S::Named])
};
/// The element type of an array.
pub(super) const ELEMENT: Context = position(&[S::String, S::Reference, S::Named]);
/// What a reference points to.
pub(super) const TARGET: Context = position(&[S::Array, S::Named]);
/// The element type of a parameter list.
pub(super) const COUNTED: Context = position(&[S::Named]);

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn initial_when_reference_then_value_only_outside_a_type_declaration() {
        assert_eq!(VARIABLES.initial(Spec::Reference), Initial::Reference);
        assert_eq!(DECLARED.initial(Spec::Reference), Initial::Forbidden);
    }

    #[test]
    fn initial_when_type_name_then_constant_in_a_type_declaration_and_expression_in_a_variable() {
        assert_eq!(VARIABLES.initial(Spec::Named), Initial::Expression);
        assert_eq!(DECLARED.initial(Spec::Named), Initial::Constant);
        assert_eq!(DECLARED.initial(Spec::Elementary), Initial::Literal);
    }

    #[test]
    fn initial_when_global_or_located_then_no_structure_value() {
        assert_eq!(GLOBAL.initial(Spec::Named), Initial::Plain);
        assert_eq!(
            VARIABLES.located(COMPLETE_ADDRESS).initial(Spec::Named),
            Initial::Plain
        );
    }

    #[test]
    fn accepts_when_kind_not_listed_then_false_and_missing_is_always_accepted() {
        assert!(!EXTERNAL.accepts(Spec::String));
        assert!(EXTERNAL.accepts(Spec::Array));
        assert!(EXTERNAL.accepts(Spec::Missing));
    }

    #[test]
    fn accepts_when_elementary_then_wherever_names_are() {
        assert!(EXTERNAL.accepts(Spec::Elementary));
        assert!(!VARIABLES
            .located(&[S::Enumeration])
            .accepts(Spec::Elementary));
    }
}
