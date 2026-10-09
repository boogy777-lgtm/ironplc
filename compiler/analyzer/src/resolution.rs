//! What a lookup of a declared name answers.
//!
//! A name has three results, not two: it is **declared and valid**, it is
//! **declared with an error** (the declaration was reported and could not be
//! used), or it is **not declared**. The environment that owns the names owns
//! that fact, so a declaration that fails is entered where a valid one would
//! be and the question "is this name declared" keeps its answer.
//!
//! Only the third result is a reason to say that a name is not declared. A
//! declaration that uses a failed one fails too, and says nothing about it,
//! because the first cause is already in the report: [`Failure::Inherited`].
//!
//! ```ignore
//! TYPE T : T_NOWHERE; END_TYPE       (* P2011: T_NOWHERE is not declared *)
//! TYPE U : ARRAY[1..2] OF T; END_TYPE (* T is declared, with an error: no message *)
//! ```

use ironplc_dsl::diagnostic::Diagnostic;

/// The result of looking a declared name up.
#[derive(Debug)]
pub(crate) enum Resolved<'a, T> {
    /// Declared, and usable.
    Valid(&'a T),
    /// Declared, with an error that has been reported.
    Failed,
    /// Not declared.
    Absent,
}

impl<'a, T> Resolved<'a, T> {
    /// The declaration for a declaration that is made from it. A failed
    /// declaration makes the user fail without a message of its own; only a
    /// name that is not declared is reported, with the diagnostic `absent`
    /// makes.
    pub(crate) fn or_failure(
        self,
        absent: impl FnOnce() -> Diagnostic,
    ) -> Result<&'a T, Failure> {
        match self {
            Resolved::Valid(declared) => Ok(declared),
            Resolved::Failed => Err(Failure::Inherited),
            Resolved::Absent => Err(Failure::Reported(absent())),
        }
    }
}

/// Why a declaration could not be made.
#[derive(Debug, Clone)]
pub(crate) enum Failure {
    /// The declaration has a problem of its own, which is reported.
    Reported(Diagnostic),
    /// The declaration is made from one that has an error. The first cause is
    /// reported where that declaration is, so nothing is reported here.
    Inherited,
}

impl From<Diagnostic> for Failure {
    fn from(diagnostic: Diagnostic) -> Self {
        Failure::Reported(diagnostic)
    }
}

impl Failure {
    /// The message to report for this failure, if there is one.
    pub(crate) fn into_diagnostic(self) -> Option<Diagnostic> {
        match self {
            Failure::Reported(diagnostic) => Some(diagnostic),
            Failure::Inherited => None,
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use ironplc_dsl::core::SourceSpan;
    use ironplc_dsl::diagnostic::Label;
    use ironplc_problems::Problem;

    fn a_diagnostic() -> Diagnostic {
        Diagnostic::problem(
            Problem::ParentTypeNotDeclared,
            Label::span(SourceSpan::default(), "x"),
        )
    }

    #[test]
    fn or_failure_when_valid_then_the_declaration() {
        let declared = 5;

        let result = Resolved::Valid(&declared).or_failure(a_diagnostic);

        assert!(matches!(result, Ok(5)));
    }

    #[test]
    fn or_failure_when_failed_then_inherited_and_nothing_reported() {
        let result = Resolved::<i32>::Failed.or_failure(a_diagnostic);

        assert!(matches!(result, Err(Failure::Inherited)));
    }

    #[test]
    fn or_failure_when_absent_then_reported() {
        let result = Resolved::<i32>::Absent.or_failure(a_diagnostic);

        assert!(matches!(result, Err(Failure::Reported(_))));
    }

    #[test]
    fn into_diagnostic_when_inherited_then_none() {
        assert!(Failure::Inherited.into_diagnostic().is_none());
    }
}
