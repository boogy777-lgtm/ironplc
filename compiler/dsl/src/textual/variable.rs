//! Variables: direct and symbolic references, their accessors, and the OOP self reference.

use super::Expr;
use crate::common::{AddressAssignment, Integer};
use crate::core::{Id, Located, SourceSpan};
use std::fmt;

use crate::fold::Fold;
use crate::visitor::Visitor;
use dsl_macro_derive::{Located, Recurse};

/// A variable.
///
/// See section B.1.4.
#[derive(Debug, PartialEq, Clone, Recurse)]
pub enum Variable {
    // A variable that maps to a hardware address.
    Direct(AddressAssignment),
    // A variable that maps to a symbolic name (essentially not a hardware address).
    Symbolic(SymbolicVariableKind),
}

impl From<SymbolicVariableKind> for Variable {
    fn from(item: SymbolicVariableKind) -> Self {
        match item {
            SymbolicVariableKind::Named(named) => {
                Variable::Symbolic(SymbolicVariableKind::Named(named))
            }
            SymbolicVariableKind::Array(array) => {
                Variable::Symbolic(SymbolicVariableKind::Array(array))
            }
            SymbolicVariableKind::Structured(structured) => {
                Variable::Symbolic(SymbolicVariableKind::Structured(structured))
            }
            SymbolicVariableKind::BitAccess(bit_access) => {
                Variable::Symbolic(SymbolicVariableKind::BitAccess(bit_access))
            }
            SymbolicVariableKind::PartialAccess(partial) => {
                Variable::Symbolic(SymbolicVariableKind::PartialAccess(partial))
            }
            SymbolicVariableKind::Deref(deref) => {
                Variable::Symbolic(SymbolicVariableKind::Deref(deref))
            }
            SymbolicVariableKind::SelfRef(self_ref) => {
                Variable::Symbolic(SymbolicVariableKind::SelfRef(self_ref))
            }
        }
    }
}

impl fmt::Display for Variable {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            Variable::Direct(assignment) => f.write_fmt(format_args!("{assignment}")),
            Variable::Symbolic(named) => f.write_fmt(format_args!("{named}")),
        }
    }
}

#[derive(Debug, PartialEq, Clone, Recurse)]
pub enum SymbolicVariableKind {
    Named(NamedVariable),
    Array(ArrayVariable),
    Structured(StructuredVariable),
    BitAccess(BitAccessVariable),
    PartialAccess(PartialAccessVariable),
    Deref(DerefVariable),
    SelfRef(SelfRefVariable),
}

impl fmt::Display for SymbolicVariableKind {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            SymbolicVariableKind::Named(named) => f.write_fmt(format_args!("{named}")),
            SymbolicVariableKind::Array(array) => f.write_fmt(format_args!("{array}")),
            SymbolicVariableKind::Structured(structured) => {
                f.write_fmt(format_args!("{structured}"))
            }
            SymbolicVariableKind::BitAccess(bit_access) => {
                f.write_fmt(format_args!("{bit_access}"))
            }
            SymbolicVariableKind::PartialAccess(partial) => f.write_fmt(format_args!("{partial}")),
            SymbolicVariableKind::Deref(deref) => f.write_fmt(format_args!("{deref}")),
            SymbolicVariableKind::SelfRef(self_ref) => f.write_fmt(format_args!("{self_ref}")),
        }
    }
}

impl Located for SymbolicVariableKind {
    fn span(&self) -> SourceSpan {
        match self {
            SymbolicVariableKind::Named(named) => named.span(),
            SymbolicVariableKind::Array(array) => array.span(),
            SymbolicVariableKind::Structured(structured) => structured.span(),
            SymbolicVariableKind::BitAccess(bit_access) => bit_access.span(),
            SymbolicVariableKind::PartialAccess(partial) => partial.span(),
            SymbolicVariableKind::Deref(deref) => deref.span(),
            SymbolicVariableKind::SelfRef(self_ref) => self_ref.span(),
        }
    }
}

impl Located for Variable {
    fn span(&self) -> SourceSpan {
        match self {
            Variable::Direct(addr) => addr.position.clone(),
            Variable::Symbolic(sym) => sym.span(),
        }
    }
}

impl Variable {
    pub fn named(name: &str) -> Variable {
        Variable::Symbolic(SymbolicVariableKind::Named(NamedVariable {
            name: Id::from(name),
        }))
    }

    pub fn structured(record: &str, field: &str) -> Variable {
        Variable::Symbolic(SymbolicVariableKind::Structured(StructuredVariable {
            record: Box::new(SymbolicVariableKind::Named(NamedVariable {
                name: Id::from(record),
            })),
            field: Id::from(field),
        }))
    }
}

#[derive(Debug, PartialEq, Clone, Recurse, Located)]
pub struct NamedVariable {
    #[located(delegate)]
    pub name: Id,
}

impl fmt::Display for NamedVariable {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        f.write_fmt(format_args!("{}", self.name))
    }
}

#[derive(Debug, PartialEq, Clone, Recurse, Located)]
pub struct ArrayVariable {
    /// The variable that is being accessed by subscript (the array).
    pub subscripted_variable: Box<SymbolicVariableKind>,
    /// The ordered set of subscripts. These should be expressions that
    /// evaluate to an index.
    pub subscripts: Vec<Expr>,
    /// Where the access is written: the array through the closing bracket,
    /// which no other part of the object holds.
    #[located(position)]
    pub span: SourceSpan,
}

impl fmt::Display for ArrayVariable {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(f, "{}[", self.subscripted_variable)?;
        for (i, subscript) in self.subscripts.iter().enumerate() {
            if i > 0 {
                write!(f, ", ")?;
            }
            write!(f, "{subscript}")?;
        }
        write!(f, "]")
    }
}

#[derive(Debug, PartialEq, Clone, Recurse)]
pub struct StructuredVariable {
    pub record: Box<SymbolicVariableKind>,
    pub field: Id,
}

impl fmt::Display for StructuredVariable {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        f.write_fmt(format_args!("{}.{}", self.record.as_ref(), self.field))
    }
}

impl Located for StructuredVariable {
    fn span(&self) -> SourceSpan {
        SourceSpan::join(&self.record.span(), &self.field.span)
    }
}

/// Bit access on an integer-typed variable.
///
/// See section B.1.4.2.
#[derive(Debug, PartialEq, Clone, Recurse)]
pub struct BitAccessVariable {
    /// The variable being bit-accessed.
    pub variable: Box<SymbolicVariableKind>,
    /// The bit index (unsigned integer).
    pub index: Integer,
}

impl fmt::Display for BitAccessVariable {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(f, "{}.{}", self.variable, self.index.value)
    }
}

impl Located for BitAccessVariable {
    fn span(&self) -> SourceSpan {
        SourceSpan::join(&self.variable.span(), &self.index.span)
    }
}

/// Size of a partial-access slice.
///
/// Used with `.%Bn`, `.%Wn`, `.%Dn`, `.%Ln` syntax to extract or replace
/// a byte/word/dword/lword-sized slice of an integer variable.
#[derive(Debug, PartialEq, Clone, Copy)]
pub enum PartialAccessSize {
    Byte,
    Word,
    DWord,
    LWord,
}

impl PartialAccessSize {
    pub fn bit_width(self) -> u32 {
        match self {
            PartialAccessSize::Byte => 8,
            PartialAccessSize::Word => 16,
            PartialAccessSize::DWord => 32,
            PartialAccessSize::LWord => 64,
        }
    }

    pub fn prefix(self) -> &'static str {
        match self {
            PartialAccessSize::Byte => "%B",
            PartialAccessSize::Word => "%W",
            PartialAccessSize::DWord => "%D",
            PartialAccessSize::LWord => "%L",
        }
    }
}

/// Partial access on an integer-typed variable, extracting or replacing
/// a byte/word/dword/lword-sized slice.
///
/// See section B.1.4.2.
#[derive(Debug, PartialEq, Clone, Recurse)]
pub struct PartialAccessVariable {
    pub variable: Box<SymbolicVariableKind>,
    #[recurse(ignore)]
    pub size: PartialAccessSize,
    pub index: Integer,
}

impl fmt::Display for PartialAccessVariable {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(
            f,
            "{}.{}{}",
            self.variable,
            self.size.prefix(),
            self.index.value
        )
    }
}

impl Located for PartialAccessVariable {
    fn span(&self) -> SourceSpan {
        SourceSpan::join(&self.variable.span(), &self.index.span)
    }
}

/// Dereference of a pointer variable, used when the result is further
/// accessed (e.g., `PT^[0]` or `PT^.field`).
///
/// See section B.1.4.
#[derive(Debug, PartialEq, Clone, Recurse, Located)]
pub struct DerefVariable {
    /// The variable being dereferenced.
    pub variable: Box<SymbolicVariableKind>,
    /// Where the dereference is written: the variable through the caret,
    /// which no other part of the object holds.
    #[located(position)]
    pub span: SourceSpan,
}

impl fmt::Display for DerefVariable {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(f, "{}^", self.variable)
    }
}

/// Which implicit instance a [`SelfRefVariable`] names.
#[derive(Debug, PartialEq, Eq, Clone, Copy)]
pub enum SelfRefKind {
    /// `THIS^` — the function block instance currently executing.
    This,
    /// `SUPER^` — the same instance, viewed as its base type.
    Super,
}

impl SelfRefKind {
    /// The source spelling, including the mandatory dereference caret.
    pub fn spelling(&self) -> &'static str {
        match self {
            SelfRefKind::This => "THIS^",
            SelfRefKind::Super => "SUPER^",
        }
    }
}

/// `THIS^` or `SUPER^` at the head of a variable reference (OOP extension).
///
/// This is the head of a symbolic variable rather than an expression
/// because `THIS^.count := 1;` puts it in assignment-target position,
/// where the AST is a [`Variable`]. Placing it at the head means the
/// ordinary element chain — `.field`, `[i]`, `.%X0` — composes with it
/// for free, in both read and write position.
///
/// The dereference caret is part of this node rather than a wrapping
/// [`DerefVariable`]: `THIS`/`SUPER` have no pointer type to be given,
/// so an un-dereferenced form would be a node nothing could type, and
/// folding the caret in makes "the caret is mandatory" structural.
#[derive(Debug, PartialEq, Clone, Recurse, Located)]
pub struct SelfRefVariable {
    #[recurse(ignore)]
    pub kind: SelfRefKind,
    /// Spans the keyword through the caret.
    pub position: SourceSpan,
}

impl fmt::Display for SelfRefVariable {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        f.write_str(self.kind.spelling())
    }
}

impl crate::extension::LanguageExtension for SelfRefVariable {
    fn extension_name(&self) -> &'static str {
        match self.kind {
            SelfRefKind::This => "THIS^ self reference",
            SelfRefKind::Super => "SUPER^ base reference",
        }
    }

    fn extension_span(&self) -> SourceSpan {
        self.position.clone()
    }
}
