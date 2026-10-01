//! The CODESYS special operators that are typed calls.
//!
//! `__NEW`, `__DELETE`, `__TYPEOF` and `__XADD` parse as ordinary calls
//! (`codesys-st-surface-syntax.md`). They are registered like any other
//! dialect-extension function, gated by `allow_special_operators`, so the
//! declared-call and argument-count rules apply to them unchanged. What a
//! function signature cannot say -- that `__NEW`'s first argument is a type
//! and its result a pointer to that type, that `__DELETE` takes a pointer --
//! is data on [`SpecialOperator`], read by result typing and by
//! `rule_special_operator`.
//!
//! The typing follows the reference compiler: `__NEW(T[, n])` is a
//! `POINTER TO T` whatever `n` is, `__DELETE` and `__TYPEOF` are `BOOL` and
//! `INT`, and `__XADD(p, v)` is `DINT` with `p : POINTER TO DINT`.
//!
//! `__CURRENTTASK`, `__SYSTEM` and `__POOL` are values and scope prefixes
//! whose types come from the target's system library, which this compiler
//! does not have. They are recognised by [`unsupported_name`] so that they
//! are reported as unsupported rather than as undeclared.

use ironplc_dsl::common::TypeName;
use ironplc_dsl::core::Id;
use ironplc_parser::options::CompilerOptions;

use crate::function_environment::FunctionSignature;
use crate::intermediates::stdlib_function::input_param;

/// A special operator that is a typed call.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum SpecialOperator {
    New,
    Delete,
    TypeOf,
    XAdd,
}

/// What the first argument of an operator is.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum TypeArgument {
    /// An ordinary expression.
    No,
    /// A type name (`__NEW`).
    Required,
    /// A type name or an expression (`__TYPEOF`).
    TypeOrExpression,
}

/// How the type of a call to an operator is found.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum ResultType {
    /// A fixed type.
    Named(&'static str),
    /// A pointer to the type named by the first argument.
    PointerToTypeArgument,
}

/// The names recognised but not supported, in the spelling CODESYS writes.
const UNSUPPORTED_NAMES: [&str; 3] = ["__CURRENTTASK", "__SYSTEM", "__POOL"];

impl SpecialOperator {
    pub const ALL: [SpecialOperator; 4] = [
        SpecialOperator::New,
        SpecialOperator::Delete,
        SpecialOperator::TypeOf,
        SpecialOperator::XAdd,
    ];

    /// The operator `name` spells, in any dialect. Code generation uses this
    /// after analysis, which has already decided whether the dialect enables
    /// the operators; analysis uses [`Self::of_call`].
    pub fn named(name: &Id) -> Option<SpecialOperator> {
        Self::ALL
            .into_iter()
            .find(|op| name.original().eq_ignore_ascii_case(op.name()))
    }

    /// The operator a call to `name` is, when the dialect enables them.
    pub fn of_call(name: &Id, options: &CompilerOptions) -> Option<SpecialOperator> {
        if !options.allow_special_operators {
            return None;
        }
        Self::named(name)
    }

    /// The operator's name, in the spelling CODESYS writes.
    pub fn name(self) -> &'static str {
        match self {
            SpecialOperator::New => "__NEW",
            SpecialOperator::Delete => "__DELETE",
            SpecialOperator::TypeOf => "__TYPEOF",
            SpecialOperator::XAdd => "__XADD",
        }
    }

    pub fn type_argument(self) -> TypeArgument {
        match self {
            SpecialOperator::New => TypeArgument::Required,
            SpecialOperator::TypeOf => TypeArgument::TypeOrExpression,
            SpecialOperator::Delete | SpecialOperator::XAdd => TypeArgument::No,
        }
    }

    pub fn result(self) -> ResultType {
        match self {
            SpecialOperator::New => ResultType::PointerToTypeArgument,
            SpecialOperator::Delete => ResultType::Named("BOOL"),
            SpecialOperator::TypeOf => ResultType::Named("INT"),
            SpecialOperator::XAdd => ResultType::Named("DINT"),
        }
    }

    /// The signature the function environment knows the operator by.
    ///
    /// The parameter types are as loose as the function environment can say;
    /// `rule_special_operator` checks what they cannot. `__NEW`'s return type
    /// is a placeholder: its real type depends on its argument
    /// ([`ResultType::PointerToTypeArgument`]).
    pub fn signature(self) -> FunctionSignature {
        match self {
            SpecialOperator::New => FunctionSignature::stdlib_extensible(
                self.name(),
                TypeName::from("ANY"),
                vec![input_param("IN1", "ANY")],
                Some(2),
            ),
            SpecialOperator::Delete => FunctionSignature::stdlib(
                self.name(),
                TypeName::from("BOOL"),
                vec![input_param("IN", "ANY")],
            ),
            SpecialOperator::TypeOf => FunctionSignature::stdlib(
                self.name(),
                TypeName::from("INT"),
                vec![input_param("IN", "ANY")],
            ),
            SpecialOperator::XAdd => FunctionSignature::stdlib(
                self.name(),
                TypeName::from("DINT"),
                vec![input_param("IN1", "ANY"), input_param("IN2", "ANY_INT")],
            ),
        }
    }
}

/// The signatures to register when `allow_special_operators` is set.
pub fn special_operator_signatures() -> Vec<FunctionSignature> {
    SpecialOperator::ALL
        .into_iter()
        .map(SpecialOperator::signature)
        .collect()
}

/// The recognised-but-unsupported name `name` is, when the dialect enables
/// the special operators.
pub fn unsupported_name(name: &Id, options: &CompilerOptions) -> Option<&'static str> {
    if !options.allow_special_operators {
        return None;
    }
    UNSUPPORTED_NAMES
        .into_iter()
        .find(|unsupported| name.original().eq_ignore_ascii_case(unsupported))
}

#[cfg(test)]
mod tests {
    use super::*;

    fn enabled() -> CompilerOptions {
        CompilerOptions {
            allow_special_operators: true,
            ..CompilerOptions::default()
        }
    }

    #[test]
    fn of_call_when_flag_on_and_name_any_case_then_operator() {
        assert_eq!(
            SpecialOperator::of_call(&Id::from("__new"), &enabled()),
            Some(SpecialOperator::New)
        );
        assert_eq!(
            SpecialOperator::of_call(&Id::from("__XADD"), &enabled()),
            Some(SpecialOperator::XAdd)
        );
    }

    #[test]
    fn of_call_when_flag_off_then_none() {
        assert_eq!(
            SpecialOperator::of_call(&Id::from("__NEW"), &CompilerOptions::default()),
            None
        );
    }

    #[test]
    fn of_call_when_name_only_contains_operator_then_none() {
        assert_eq!(
            SpecialOperator::of_call(&Id::from("__NEW_ITEM"), &enabled()),
            None
        );
    }

    #[test]
    fn unsupported_name_when_flag_on_then_names_recognised() {
        assert_eq!(
            unsupported_name(&Id::from("__currenttask"), &enabled()),
            Some("__CURRENTTASK")
        );
        assert_eq!(
            unsupported_name(&Id::from("__SYSTEM"), &enabled()),
            Some("__SYSTEM")
        );
        assert_eq!(
            unsupported_name(&Id::from("__POOL"), &enabled()),
            Some("__POOL")
        );
        assert_eq!(unsupported_name(&Id::from("x"), &enabled()), None);
    }

    #[test]
    fn unsupported_name_when_flag_off_then_none() {
        assert_eq!(
            unsupported_name(&Id::from("__SYSTEM"), &CompilerOptions::default()),
            None
        );
    }

    #[test]
    fn special_operator_signatures_when_called_then_one_per_operator() {
        let names: Vec<String> = special_operator_signatures()
            .iter()
            .map(|s| s.name.original().to_string())
            .collect();
        assert_eq!(names, vec!["__NEW", "__DELETE", "__TYPEOF", "__XADD"]);
    }

    #[test]
    fn signature_when_new_then_accepts_type_and_optional_count() {
        let sig = SpecialOperator::New.signature();
        assert!(sig.is_extensible);
        assert_eq!(sig.max_inputs, Some(2));
        assert_eq!(sig.input_parameter_count(), 1);
    }
}
