//! Initial value assignments that give a declared variable its starting value.

use dsl_macro_derive::{Located, Recurse};

use super::{
    ArrayInitialElementKind, ArraySpecificationKind, CharacterStringLiteral, ConstantKind,
    EnumeratedSpecificationKind, EnumeratedValue, HasEnumeratedValues, IntegerLiteral, IntegerRef,
    ParamsSpecification, RefSyntax, ReferenceTarget, SpecificationKind, StringType,
    StructureDeclaration, StructureElementInit, StructureInitializationDeclaration,
    SubrangeInitializer, SubrangeSpecificationKind, TypeName, TypeReference,
};
use crate::core::{Id, Located, SourceSpan};
use crate::fold::Fold;
use crate::textual::*;
use crate::visitor::Visitor;

/// Container for initial value assignments. The initial value specifies a
/// "coarse grained assignment",
///
/// Declarations of variables can be associated with an initial value. The
/// initial value assignment is not necessarily compatible with the associated
/// variable.
///
/// See section 2.4.3.2.
#[derive(Clone, PartialEq, Debug, Recurse)]
pub enum InitialValueAssignmentKind {
    /// Represents no type initializer.
    ///
    /// Some types allow no initializer and this avoids nesting of the
    /// enumeration with an Option enumeration.
    None(SourceSpan),
    /// Represents an initializer that is a constant.
    Simple(SimpleInitializer),
    String(StringInitializer),
    EnumeratedValues(EnumeratedValuesInitializer),
    EnumeratedType(EnumeratedInitialValueAssignment),
    FunctionBlock(FunctionBlockInitialValueAssignment),
    /// The CODESYS/TwinCAT call-style function-block instance initializer
    /// (`name : FB_Type(args)`, no `:=`), which passes an argument list to
    /// the function block's constructor (`FB_init` method in CODESYS).
    /// This is a distinct construct from [`Self::FunctionBlock`], whose
    /// `init` sets the instance's own member values -- see
    /// [`FunctionBlockCallInitializer`].
    FunctionBlockCall(FunctionBlockCallInitializer),
    Subrange(SubrangeInitializer),
    Structure(StructureInitializationDeclaration),
    Array(ArrayInitialValueAssignment),
    /// CODESYS parameter-list type (`PARAMS(n) OF T`); see
    /// [`ParamsSpecification`].
    Params(ParamsSpecification),
    /// Reference type initializer (REF_TO).
    Reference(ReferenceInitializer),
    /// A declaration whose type is a user-defined name the parser cannot
    /// classify, with the initializer exactly as written. The type
    /// resolver replaces it with the kind the type implies (ADR-0050).
    LateResolvedType(LateResolvedInitializer),
    /// A constant-expression initializer not yet folded to a literal
    /// (extension — see `allow_constant_initializer_expressions`).
    /// Always normalized to `Simple` by
    /// `xform_fold_initializer_expressions` before any other pass runs;
    /// no other code should ever match on this variant.
    SimpleExpr(SimpleExprInitializer),
}

/// A declaration against a user-defined type name, before the type is known.
///
/// `T` in `x : T`, `x : T := (a := 1)` or `x : T := Red` may be a structure,
/// a function block, an enumeration or an alias of an elementary type, and
/// only the type resolver can tell. The parser records the initializer as
/// written and decides nothing; see ADR-0050.
#[derive(Clone, Debug, PartialEq, Recurse)]
pub struct LateResolvedInitializer {
    pub type_name: TypeName,
    pub initial_value: Option<LateResolvedInitialValue>,
}

impl LateResolvedInitializer {
    /// A bare declaration, `x : T`.
    pub fn bare(type_name: TypeName) -> Self {
        LateResolvedInitializer {
            type_name,
            initial_value: None,
        }
    }
}

/// The initializer written against a type the parser could not classify.
#[derive(Clone, Debug, PartialEq, Recurse)]
pub enum LateResolvedInitialValue {
    /// `(a := 1, b := 2)`: a structure's fields, or a function block's
    /// members.
    Members(Vec<StructureElementInit>),
    /// `Red`: an enumeration's value, or a named constant for any other
    /// type.
    Value(Id),
}

impl InitialValueAssignmentKind {
    /// Returns the type this initializer declares: the name it states, or
    /// [`TypeReference::Inline`] for a type spelled out in place.
    pub fn type_reference(&self) -> TypeReference {
        match self {
            InitialValueAssignmentKind::None(_source_span) => TypeReference::Unspecified,
            InitialValueAssignmentKind::Simple(simple_initializer) => {
                TypeReference::Named(simple_initializer.type_name.clone())
            }
            InitialValueAssignmentKind::String(string_initializer) => {
                TypeReference::Named(string_initializer.type_name())
            }
            InitialValueAssignmentKind::EnumeratedValues(_enumerated_values_initializer) => {
                TypeReference::Inline
            }
            InitialValueAssignmentKind::EnumeratedType(enumerated_initial_value_assignment) => {
                TypeReference::Named(enumerated_initial_value_assignment.type_name.clone())
            }
            InitialValueAssignmentKind::FunctionBlock(function_block_initial_value_assignment) => {
                TypeReference::Named(function_block_initial_value_assignment.type_name.clone())
            }
            InitialValueAssignmentKind::FunctionBlockCall(function_block_call_initializer) => {
                TypeReference::Named(function_block_call_initializer.type_name.clone())
            }
            InitialValueAssignmentKind::Subrange(subrange_initializer) => {
                match &subrange_initializer.spec {
                    SpecificationKind::Inline(_subrange_specification) => TypeReference::Inline,
                    SpecificationKind::Named(type_name) => TypeReference::Named(type_name.clone()),
                }
            }
            InitialValueAssignmentKind::Structure(structure_initialization_declaration) => {
                TypeReference::Named(structure_initialization_declaration.type_name.clone())
            }
            InitialValueAssignmentKind::Array(array_initial_value_assignment) => {
                match &array_initial_value_assignment.spec {
                    SpecificationKind::Named(type_name) => TypeReference::Named(type_name.clone()),
                    SpecificationKind::Inline(_) => TypeReference::Inline,
                }
            }
            InitialValueAssignmentKind::Reference(_) => TypeReference::Inline,
            InitialValueAssignmentKind::Params(_) => TypeReference::Inline,
            InitialValueAssignmentKind::LateResolvedType(late) => {
                TypeReference::Named(late.type_name.clone())
            }
            InitialValueAssignmentKind::SimpleExpr(simple_expr_initializer) => {
                TypeReference::Named(simple_expr_initializer.type_name.clone())
            }
        }
    }

    /// Returns whether the declaration states an initial value: a literal,
    /// enumerated or string value, at least one array element, at least one
    /// structure member, or a reference target. A function-block instance
    /// has state rather than a value, and an initializer whose kind is not
    /// yet resolved states nothing.
    pub fn has_initial_value(&self) -> bool {
        match self {
            InitialValueAssignmentKind::Simple(si) => si.initial_value.is_some(),
            InitialValueAssignmentKind::String(si) => si.initial_value.is_some(),
            InitialValueAssignmentKind::EnumeratedValues(ev) => ev.initial_value.is_some(),
            InitialValueAssignmentKind::EnumeratedType(et) => et.initial_value.is_some(),
            InitialValueAssignmentKind::Array(arr) => !arr.initial_values.is_empty(),
            InitialValueAssignmentKind::Structure(st) => !st.elements_init.is_empty(),
            InitialValueAssignmentKind::Reference(re) => re.initial_value.is_some(),
            InitialValueAssignmentKind::Subrange(sr) => sr.initial_value.is_some(),
            InitialValueAssignmentKind::None(_)
            | InitialValueAssignmentKind::FunctionBlock(_)
            | InitialValueAssignmentKind::FunctionBlockCall(_)
            | InitialValueAssignmentKind::Params(_)
            | InitialValueAssignmentKind::LateResolvedType(_)
            | InitialValueAssignmentKind::SimpleExpr(_) => false,
        }
    }

    /// The value the declaration states, in the form a member initializer or
    /// a storage location takes it: `None` when it states none (the type's
    /// own default then applies).
    ///
    /// This is the one place that reads a declared value out of the
    /// declaration kinds, so a structure member and a variable agree on what
    /// "declared initial value" means.
    pub fn stated_value(&self) -> Option<StructInitialValueAssignmentKind> {
        match self {
            InitialValueAssignmentKind::Simple(si) => si
                .initial_value
                .clone()
                .map(StructInitialValueAssignmentKind::Constant),
            InitialValueAssignmentKind::String(si) => si.initial_value.clone().map(|lit| {
                StructInitialValueAssignmentKind::Constant(ConstantKind::CharacterString(lit))
            }),
            InitialValueAssignmentKind::Subrange(sr) => sr.initial_value.clone().map(|value| {
                StructInitialValueAssignmentKind::Constant(ConstantKind::IntegerLiteral(
                    IntegerLiteral {
                        value,
                        data_type: None,
                    },
                ))
            }),
            InitialValueAssignmentKind::EnumeratedValues(ev) => ev
                .initial_value
                .clone()
                .map(StructInitialValueAssignmentKind::EnumeratedValue),
            InitialValueAssignmentKind::EnumeratedType(et) => et
                .initial_value
                .clone()
                .map(StructInitialValueAssignmentKind::EnumeratedValue),
            InitialValueAssignmentKind::Array(arr) if !arr.initial_values.is_empty() => Some(
                StructInitialValueAssignmentKind::Array(arr.initial_values.clone()),
            ),
            InitialValueAssignmentKind::Structure(st) if !st.elements_init.is_empty() => Some(
                StructInitialValueAssignmentKind::Structure(st.elements_init.clone()),
            ),
            _ => None,
        }
    }

    /// Creates an initial value with
    pub fn simple_uninitialized(type_name: TypeName) -> Self {
        InitialValueAssignmentKind::Simple(SimpleInitializer {
            type_name,
            initial_value: None,
        })
    }

    /// Creates an initial value from the initializer.
    pub fn simple(type_name: &str, value: ConstantKind) -> Self {
        InitialValueAssignmentKind::Simple(SimpleInitializer {
            type_name: TypeName::from(type_name),
            initial_value: Some(value),
        })
    }

    /// Creates an initial value consisting of an enumeration definition and
    /// possible initial value for the enumeration.
    pub fn enumerated_values(
        values: Vec<EnumeratedValue>,
        initial_value: Option<EnumeratedValue>,
    ) -> Self {
        InitialValueAssignmentKind::EnumeratedValues(EnumeratedValuesInitializer {
            values,
            initial_value,
        })
    }
}

/// Container for initial value assignments in structures.
///
/// Initial value assignments in structures are similar to initial value
/// assignments outside of structures except that they cannot have a
/// specification (the specification is with the structure) and that the
/// initialization is required.
///
/// See section 2.4.3.2.
#[derive(Clone, PartialEq, Debug, Recurse)]
pub enum StructInitialValueAssignmentKind {
    Constant(ConstantKind),
    EnumeratedValue(EnumeratedValue),
    Array(Vec<ArrayInitialElementKind>),
    Structure(Vec<StructureElementInit>),
    /// A general expression value (e.g. `pDevice^.Delta`) -- used for
    /// call-style FB-instance/struct initializers where the value is
    /// computed at instantiation time, not a compile-time constant.
    Expression(Expr),
    /// A bare identifier, before anything knows what it names.
    ///
    /// `(x := g)` is one token in a position that accepts both an
    /// enumerated value and a variable reference, and the parser has no
    /// declarations in scope to tell them apart. Recording the ambiguity is
    /// what keeps a diagnostic from naming the wrong construct;
    /// `xform_resolve_late_bound_expr_kind` replaces this with
    /// [`Self::EnumeratedValue`] or [`Self::Expression`] once declarations
    /// are known, and no later stage sees it.
    LateBound(LateBound),
}

#[derive(Clone, PartialEq, Debug, Recurse)]
pub struct EnumeratedInitialValueAssignment {
    pub type_name: TypeName,
    pub initial_value: Option<EnumeratedValue>,
}

/// Initial value for a reference variable.
///
/// See IEC 61131-3 Edition 3, section 2.4.3.2.
#[derive(Clone, Debug, PartialEq, Recurse)]
pub enum ReferenceInitialValue {
    Null(SourceSpan),
    Ref(Variable),
}

/// Initializer for a reference variable declaration (`REF_TO` or
/// `REFERENCE TO`).
///
/// See IEC 61131-3 Edition 3, section 2.4.3.2, and
/// `specs/design/reference-to-twincat.md`.
#[derive(Clone, Debug, PartialEq, Recurse)]
pub struct ReferenceInitializer {
    pub target: ReferenceTarget,
    pub initial_value: Option<ReferenceInitialValue>,
    /// The surface keyword that produced this initializer.
    #[recurse(ignore)]
    pub syntax: RefSyntax,
}

#[derive(Clone, PartialEq, Debug, Recurse)]
pub struct SimpleInitializer {
    pub type_name: TypeName,
    pub initial_value: Option<ConstantKind>,
}

/// An initializer (of a variable, of a type or of a structure member)
/// expressed as a constant expression (e.g.
/// `PI/180.0`) rather than a bare literal.
///
/// This is an extension — the IEC 61131-3 standard's `constant()`
/// grammar production only permits literals in this position. Parsed
/// unconditionally; `xform_fold_initializer_expressions` folds it back to
/// `SimpleInitializer` or emits a diagnostic, depending on
/// `allow_constant_initializer_expressions`.
#[derive(Clone, PartialEq, Debug, Recurse)]
pub struct SimpleExprInitializer {
    pub type_name: TypeName,
    pub initial_value: Expr,
}

/// Provides the initialization of a string variable declaration.
///
/// See sections 2.4.3.1 and 2.4.3.2.
#[derive(Clone, PartialEq, Debug, Recurse, Located)]
pub struct StringInitializer {
    /// Maximum length of the string.
    pub length: Option<IntegerRef>,
    /// The size of a single 'character'
    #[recurse(ignore)]
    pub width: StringType,
    /// Default value of the string. If not specified, then
    /// the default value is the empty string. The literal keeps the width
    /// its delimiter spelled and its own span, so a check on the literal
    /// can point at it.
    pub initial_value: Option<CharacterStringLiteral>,

    #[located(position)]
    pub keyword_span: SourceSpan,
}

impl StringInitializer {
    pub fn type_name(&self) -> TypeName {
        match self.width {
            StringType::String => TypeName::from("string"),
            StringType::WString => TypeName::from("wstring"),
        }
    }
}

#[derive(Clone, PartialEq, Debug, Recurse)]
pub struct EnumeratedValuesInitializer {
    pub values: Vec<EnumeratedValue>,
    pub initial_value: Option<EnumeratedValue>,
}

impl Located for EnumeratedValuesInitializer {
    fn span(&self) -> SourceSpan {
        let first = self.values.first();
        let last = self.values.last();

        if let Some(f) = first {
            if let Some(l) = last {
                return SourceSpan::join(&f.span(), &l.span());
            }
            return f.span();
        }
        SourceSpan::default()
    }
}

impl HasEnumeratedValues for EnumeratedValuesInitializer {
    fn values(&self) -> &Vec<EnumeratedValue> {
        &self.values
    }
    fn values_span(&self) -> SourceSpan {
        self.span()
    }
}

#[derive(Clone, PartialEq, Debug, Recurse)]
pub struct FunctionBlockInitialValueAssignment {
    // In this context, the name is referring to a type, much like a function pointer
    // in other languages, so the correct representation here is a type and not
    // an identifier.
    pub type_name: TypeName,
    // The initializer may be empty
    pub init: Vec<StructureElementInit>,
}

/// The CODESYS/TwinCAT call-style function-block instance initializer:
/// `name : FB_Type(args)`, with no `:=`. The argument list uses the same
/// positional-or-named shape as an ordinary function-block call
/// (`param_assignment`), because in CODESYS these arguments are passed to
/// the function block's constructor (`FB_init` method).
///
/// This is deliberately a separate node from
/// [`FunctionBlockInitialValueAssignment`] (whose `init` sets the
/// instance's own member values via the `:= (member := value, ...)` form):
/// the two are different constructs that target different things, so
/// keeping them as distinct AST nodes prevents them from being conflated
/// at code generation.
#[derive(Clone, PartialEq, Debug, Recurse)]
pub struct FunctionBlockCallInitializer {
    /// The function-block type being instantiated. As with
    /// [`FunctionBlockInitialValueAssignment::type_name`], this names a
    /// type, not an identifier.
    pub type_name: TypeName,
    /// The constructor argument list. May be empty (`FB_Type()`).
    pub params: Vec<ParamAssignmentKind>,
}

/// See section 2.4.3.2. #6
#[derive(Clone, PartialEq, Debug, Recurse)]
pub struct ArrayInitialValueAssignment {
    pub spec: ArraySpecificationKind,
    pub initial_values: Vec<ArrayInitialElementKind>,
}

#[derive(Clone, PartialEq, Debug, Recurse)]
pub enum VariableSpecificationKind {
    Simple(TypeName),
    Subrange(SubrangeSpecificationKind),
    Enumerated(EnumeratedSpecificationKind),
    Array(ArraySpecificationKind),
    Struct(StructureDeclaration),
    String(StringSpecification),
    WString(StringSpecification),
    // Represents simple, subrange or structure
    Ambiguous(TypeName),
}

#[derive(Clone, PartialEq, Debug, Recurse)]
pub struct StringSpecification {
    #[recurse(ignore)]
    pub width: StringType,
    pub length: Option<IntegerRef>,
    pub keyword_span: SourceSpan,
}
