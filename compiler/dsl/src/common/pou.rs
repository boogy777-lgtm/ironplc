//! Program organization units: functions, function blocks and programs.

use dsl_macro_derive::{Attributed, Located, Recurse};

use super::{
    Attributes, FunctionBlockOop, HasVariables, MethodDeclaration, ProgramAccessDecl,
    PropertyDeclaration, StringSpecification, TypeName, VarDecl,
};
use crate::core::{Id, SourceSpan};
use crate::fold::Fold;
use crate::scope::ScopeBearing;
use crate::sfc::{Network, Sfc};
use crate::textual::*;
use crate::visitor::Visitor;

/// Return type for a function declaration.
///
/// IEC 61131-3 allows functions to return elementary types, derived types,
/// or string types with an optional length specifier (e.g., `STRING[255]`).
#[derive(Clone, Debug, PartialEq, Recurse)]
pub enum FunctionReturnType {
    /// A simple type name (elementary or derived), e.g. `INT`, `REAL`, `MY_TYPE`.
    Named(TypeName),
    /// `STRING` with optional length, e.g. `STRING[255]`.
    String(StringSpecification),
    /// `WSTRING` with optional length, e.g. `WSTRING[100]`.
    WString(StringSpecification),
}

impl FunctionReturnType {
    /// Returns the type name suitable for type resolution.
    ///
    /// For `Named`, returns the inner `TypeName`.
    /// For `String`/`WString`, returns `TypeName::from("STRING")` or `TypeName::from("WSTRING")`.
    pub fn to_type_name(&self) -> TypeName {
        match self {
            FunctionReturnType::Named(tn) => tn.clone(),
            FunctionReturnType::String(_) => TypeName::from("STRING"),
            FunctionReturnType::WString(_) => TypeName::from("WSTRING"),
        }
    }
}

impl From<TypeName> for FunctionReturnType {
    fn from(tn: TypeName) -> Self {
        FunctionReturnType::Named(tn)
    }
}

///Function Program Organization Unit Declaration
///
/// A function is stateless and has no "memory". Functions
/// consists of a series of statements that provide outputs through the
/// return value and bound variables.
///
/// See section 2.5.1.
#[derive(Clone, Debug, PartialEq, Recurse, Attributed)]
#[recurse(scope)]
pub struct FunctionDeclaration {
    pub name: Id,
    pub return_type: FunctionReturnType,
    pub variables: Vec<VarDecl>,
    pub body: Vec<StmtKind>,
    #[recurse(ignore)]
    pub attributes: Attributes,
}

impl HasVariables for FunctionDeclaration {
    fn variables(&self) -> &Vec<VarDecl> {
        &self.variables
    }
}

/// Function Block Program Organization Unit Declaration
///
/// A function block declaration (as distinct from a particular
/// instance of a function block). The Function block instance is stateful
/// and variables retain values between invocations.
///
/// See section 2.5.2.
#[derive(Clone, Debug, PartialEq, Recurse, Located, Attributed)]
#[recurse(scope)]
pub struct FunctionBlockDeclaration {
    pub name: TypeName,
    pub variables: Vec<VarDecl>,
    pub body: FunctionBlockBodyKind,
    #[located(position)]
    pub span: SourceSpan,
    /// Object-oriented facet (OOP extension).
    ///
    /// `None` for an ordinary function block — the common case — so OOP is
    /// unrepresentable on a plain FB rather than "present but empty."
    /// `Some` only when the source actually uses `EXTENDS`, `IMPLEMENTS`,
    /// or a qualifier such as `ABSTRACT`. See `FunctionBlockOop` for why
    /// the facet is its own struct and why `base` is an `Option` where
    /// `InterfaceDeclaration::extends` is a `Vec`, and the
    /// `LanguageExtension` impl on it for why only the facet, and not the
    /// whole declaration, is an extension.
    pub oop: Option<FunctionBlockOop>,
    /// `METHOD ... END_METHOD` blocks declared on this function block
    /// (OOP extension). Empty for an ordinary function block — same
    /// "empty is the common case" convention as `variables`, rather than
    /// nesting under `FunctionBlockOop` (a plain FB can declare methods
    /// without using `EXTENDS`/`IMPLEMENTS`/`ABSTRACT`). See ADR-0041.
    pub methods: Vec<MethodDeclaration>,
    /// `PROPERTY` declarations (OOP extension), kept apart from `methods`:
    /// their accessors are methods, but they are reached through the
    /// property, not by a method call. See [`PropertyDeclaration`].
    pub properties: Vec<PropertyDeclaration>,
    #[recurse(ignore)]
    pub attributes: Attributes,
}

impl HasVariables for FunctionBlockDeclaration {
    fn variables(&self) -> &Vec<VarDecl> {
        &self.variables
    }
}

/// "Program" Program Organization Unit Declaration Declaration
///
/// Programs assembled the units into a whole that embodies a measurement
/// or control objective.
///
/// See section 2.5.3.
#[derive(Clone, Debug, PartialEq, Recurse, Attributed)]
#[recurse(scope)]
pub struct ProgramDeclaration {
    pub name: Id,
    pub variables: Vec<VarDecl>,
    pub access_variables: Vec<ProgramAccessDecl>,
    pub body: FunctionBlockBodyKind,
    #[recurse(ignore)]
    pub attributes: Attributes,
}

impl HasVariables for ProgramDeclaration {
    fn variables(&self) -> &Vec<VarDecl> {
        &self.variables
    }
}

/// Container for type types of elements that can compose the body of a
/// function block.
///
/// See section 2.5.2.
#[derive(Clone, Debug, PartialEq, Recurse)]
pub enum FunctionBlockBodyKind {
    Sfc(Sfc),
    Statements(Statements),
    /// A function block that has no body (and is therefore no known type).
    ///
    /// This type is not strictly valid, but highly useful and can be detected
    /// with a semantic rule.
    #[recurse(ignore)]
    Empty,
}

impl FunctionBlockBodyKind {
    /// Creates a function body that is composed of statements.
    pub fn stmts(stmts: Vec<StmtKind>) -> FunctionBlockBodyKind {
        FunctionBlockBodyKind::Statements(Statements { body: stmts })
    }

    /// Creates a function body that is composed of a sequential function block.
    pub fn sfc(networks: Vec<Network>) -> FunctionBlockBodyKind {
        FunctionBlockBodyKind::Sfc(Sfc { networks })
    }

    /// Creates an empty function body.
    pub fn empty() -> FunctionBlockBodyKind {
        FunctionBlockBodyKind::Empty
    }
}
