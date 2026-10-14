//! The faults of the attributes of one declaration: the one check of the
//! registry.
//!
//! A fault is data. The check says what is wrong and where; a part of the
//! compiler that reports it turns the fault into its own diagnostic, and no
//! part of it holds any knowledge of a row.

use super::find;
use super::row::Info;
use crate::attribute::census::TargetKind;
use crate::attribute::{Attribute, Attributes};
use crate::core::SourceSpan;

/// What is wrong with one attribute of a declaration.
#[derive(Clone, Debug)]
pub enum FaultKind {
    /// The attribute is known and does not stand before this kind of
    /// declaration.
    NotAllowedHere {
        row: &'static Info,
        kind: TargetKind,
    },
    /// The attribute is known and its value is not one the row reads: the
    /// value is given to a row that takes none, is missing, or is not one the
    /// row takes. `found` is the value as written, `None` when it is missing.
    Value {
        row: &'static Info,
        found: Option<String>,
    },
    /// The same attribute stands before the declaration already; `first` is
    /// where it stood first.
    Repeated { first: SourceSpan },
}

/// One fault, at the attribute that has it.
#[derive(Clone, Debug)]
pub struct Fault {
    /// The name of the attribute, as written.
    pub name: String,
    /// Where the attribute is in the source.
    pub span: SourceSpan,
    pub kind: FaultKind,
}

/// Whether two attributes of one declaration are the same attribute: the same
/// row (so a name and its alias are one), or, for attributes the compiler does
/// not know, the same name without regard to case.
fn same(first: &Attribute, second: &Attribute) -> bool {
    match (find(&first.name), find(&second.name)) {
        (Some(first_row), Some(second_row)) => first_row == second_row,
        (None, None) => first.name.eq_ignore_ascii_case(&second.name),
        (Some(_), None) | (None, Some(_)) => false,
    }
}

/// The fault of `attribute`, which stands after `before` on a declaration of
/// `kind`. One fault for an attribute, in this order: repeated, not allowed
/// here, value. An attribute the compiler does not know has none unless it is
/// repeated.
fn fault_of<'a>(
    kind: TargetKind,
    mut before: impl Iterator<Item = &'a Attribute>,
    attribute: &Attribute,
) -> Option<FaultKind> {
    if let Some(first) = before.find(|earlier| same(earlier, attribute)) {
        return Some(FaultKind::Repeated {
            first: first.span.clone(),
        });
    }
    let row = find(&attribute.name)?;
    if !row.targets.contains(kind) {
        return Some(FaultKind::NotAllowedHere { row, kind });
    }
    if !row.accepts(attribute.value.as_deref()) {
        return Some(FaultKind::Value {
            row,
            found: attribute.value.clone(),
        });
    }
    None
}

/// The faults of the attributes of a declaration of `kind`, in the order of
/// the attributes.
pub fn faults(kind: TargetKind, attributes: &Attributes) -> Vec<Fault> {
    attributes
        .iter()
        .enumerate()
        .filter_map(|(index, attribute)| {
            fault_of(kind, attributes.iter().take(index), attribute).map(|fault| Fault {
                name: attribute.name.clone(),
                span: attribute.span.clone(),
                kind: fault,
            })
        })
        .collect()
}

#[cfg(test)]
mod tests;
