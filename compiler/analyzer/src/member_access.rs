//! Which code may read or write a member of a function block instance.
//!
//! The answer is one function, [`check`], so that every rule that selects a
//! member from an instance asks the same question and gets the same answer. It
//! follows CODESYS, the reference (C0037 and C0178, see the problem pages of
//! P4081 and P4082):
//!
//! | Member declared in | Read from outside | Write from outside |
//! |---|---|---|
//! | `VAR_INPUT` | yes | yes |
//! | `VAR_OUTPUT` | yes | no |
//! | `VAR`, `VAR_STAT`, `VAR_INST` | yes | no |
//! | `VAR_IN_OUT` | no | no |
//!
//! "Outside" is any code that is not code of the block that declares the
//! member: the body of the block, its methods, and the body of a block that
//! extends it are inside. The decision is made on the member's own variable kind,
//! which `FunctionBlockVarType::of` derives from the declaration, and on the
//! `EXTENDS` lineage of the block whose code makes the access. A member that is
//! not a variable of a function block (an element of a structure, a property, a
//! method) has no restriction here.
//!
//! A new kind of variable is one more arm of the exhaustive match in
//! [`check`]; a new reason to refuse is one more [`Refusal`] variant, which the
//! rule that reports it must then say how to describe.

use ironplc_dsl::common::TypeName;

use crate::{
    intermediate_type::FunctionBlockVarType,
    type_members::{Member, TypeMembers},
};

/// What the code does with the member.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub(crate) enum Access {
    Read,
    Write,
}

/// Why the code may not access the member.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub(crate) enum Refusal {
    /// A `VAR_OUTPUT` is written from outside the block (CODESYS C0037).
    OutputWritten,
    /// A `VAR`, `VAR_STAT` or `VAR_INST` is written from outside the block
    /// (CODESYS C0037).
    InternalWritten,
    /// A `VAR_IN_OUT` is read or written from outside the block (CODESYS
    /// C0178).
    InOutTouched,
}

/// Whether code of the function block `accessor` (`None` for a program or a
/// function) may do `access` to `member`.
pub(crate) fn check(
    members: &TypeMembers<'_>,
    accessor: Option<&TypeName>,
    member: &Member<'_>,
    access: Access,
) -> Result<(), Refusal> {
    let Some(declared) = member.declared_variable() else {
        return Ok(());
    };
    let inside = match (accessor, declared.block) {
        (Some(accessor), Some(block)) => members.is_code_of(accessor, block),
        _ => false,
    };
    if inside {
        return Ok(());
    }
    match declared.kind {
        FunctionBlockVarType::Input => Ok(()),
        FunctionBlockVarType::Output => match access {
            Access::Read => Ok(()),
            Access::Write => Err(Refusal::OutputWritten),
        },
        FunctionBlockVarType::Internal => match access {
            Access::Read => Ok(()),
            Access::Write => Err(Refusal::InternalWritten),
        },
        FunctionBlockVarType::InOut => Err(Refusal::InOutTouched),
    }
}
