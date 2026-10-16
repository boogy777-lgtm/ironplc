//! The faults of the attributes of one declaration: the one check of the
//! registry.
//!
//! A fault is data. The check says what is wrong and where; a part of the
//! compiler that reports it turns the fault into its own diagnostic, and no
//! part of it holds any knowledge of a row.

use super::find;
use super::row::Info;
use crate::attribute::census::{Declared, Held, TargetKind};
use crate::attribute::{Attribute, Attributes};
use crate::common::VariableType;
use crate::core::SourceSpan;
use crate::diagnostic::{Diagnostic, Label};
use ironplc_problems::text;

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
    /// The attribute is known and allowed here, and the declaration holds a
    /// variable of a kind of block that the row forbids; the fault is at the
    /// first such variable, and `attribute` is where the attribute is.
    ForbidsBlock {
        row: &'static Info,
        block: VariableType,
        attribute: SourceSpan,
    },
}

/// One fault, at the attribute that has it, or at the part of the declaration
/// that the attribute does not allow.
#[derive(Clone, Debug)]
pub struct Fault {
    /// The name of the attribute, as written.
    pub name: String,
    /// Where the fault is in the source.
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

/// The fault of `attribute`, a pragma that marks the edge of a region
/// ([`Info::region`]) and so stands before no declaration: its value is not one
/// the row reads. Nothing for any other attribute, which the check of the
/// declaration it is attached to answers for.
pub fn region_fault(attribute: &Attribute) -> Option<Fault> {
    let row = find(&attribute.name).filter(|row| row.region)?;
    if row.accepts(attribute.value.as_deref()) {
        return None;
    }
    Some(Fault {
        name: attribute.name.clone(),
        span: attribute.span.clone(),
        kind: FaultKind::Value {
            row,
            found: attribute.value.clone(),
        },
    })
}

impl Fault {
    /// The diagnostic of the fault: one arm for each kind of fault, each text a
    /// row of the message dictionary (`problem-texts.csv`).
    pub fn diagnostic(self) -> Diagnostic {
        let name = self.name;
        match self.kind {
            FaultKind::NotAllowedHere { row, kind } => Diagnostic::text(
                self.span,
                text::attribute_not_allowed_here(name, kind.label(), row.targets.describe()),
            ),
            FaultKind::Value { row, found } => Diagnostic::text(
                self.span,
                match found {
                    Some(value) => {
                        text::attribute_value_invalid_found(name, row.value.describe(), value)
                    }
                    None => text::attribute_value_invalid_missing(name, row.value.describe()),
                },
            ),
            FaultKind::Repeated { first } => {
                Diagnostic::text(self.span, text::attribute_repeated(name)).with_secondary(
                    Label::span(first, text::attribute_repeated_first_written_here()),
                )
            }
            FaultKind::ForbidsBlock {
                block, attribute, ..
            } => Diagnostic::text(
                self.span,
                text::attribute_forbids_variable_block(block.keyword(), name),
            )
            .with_secondary(Label::span(
                attribute,
                text::attribute_forbids_variable_block_attribute_written_here(),
            )),
        }
    }
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

/// The faults of the attributes of a declaration of `kind` that holds `holds`:
/// for each attribute whose row forbids a kind of variable block, one fault at
/// the first variable of the declaration written in such a block. An attribute
/// that is written again, or does not stand before this kind, has none here
/// (the faults of [`faults`] already say so).
fn forbidden_faults(kind: TargetKind, attributes: &Attributes, holds: &[Held]) -> Vec<Fault> {
    attributes
        .iter()
        .enumerate()
        .filter(|(index, attribute)| {
            !attributes
                .iter()
                .take(*index)
                .any(|earlier| same(earlier, attribute))
        })
        .filter_map(|(_, attribute)| Some((attribute, find(&attribute.name)?)))
        .filter(|(_, row)| row.targets.contains(kind))
        .flat_map(|(attribute, row)| {
            row.forbids.iter().filter_map(move |block| {
                let held = holds.iter().find(|held| held.block == *block)?;
                Some(Fault {
                    name: attribute.name.clone(),
                    span: held.span.clone(),
                    kind: FaultKind::ForbidsBlock {
                        row,
                        block: block.clone(),
                        attribute: attribute.span.clone(),
                    },
                })
            })
        })
        .collect()
}

/// The faults of a declaration of the census: those of its attributes
/// ([`faults`]), then those of what it holds.
pub fn declared_faults(declared: &Declared) -> Vec<Fault> {
    faults(declared.kind, &declared.attributes)
        .into_iter()
        .chain(forbidden_faults(
            declared.kind,
            &declared.attributes,
            &declared.holds,
        ))
        .collect()
}

#[cfg(test)]
mod tests;
