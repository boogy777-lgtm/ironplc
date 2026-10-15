//! Data type declarations: enumerations, subranges, arrays, structures, strings, references and parameter lists.

use std::fmt;

use dsl_macro_derive::{Attributed, Recurse};

use super::{
    Attributes, CharacterStringLiteral, ConstantKind, ElementaryTypeName, EnumeratedDefault,
    InitialValueAssignmentKind, Integer, IntegerRef, SignedInteger, SignedIntegerRef,
    SpecificationKind, StringSpecification, StructInitialValueAssignmentKind, TypeName,
};
use crate::core::{Id, Located, SourceSpan};
use crate::fold::Fold;
use crate::visitor::Visitor;

/// Kinds of derived data types.
///
/// See section 2.3.3.1
#[derive(Clone, Debug, PartialEq, Recurse)]
pub enum DataTypeDeclarationKind {
    /// Derived data type the restricts permitted values from a set of identifiers.
    Enumeration(EnumerationDeclaration),
    /// Derived data type that restricts permitted values to a smaller range
    /// of the parent data type.
    Subrange(SubrangeDeclaration),
    Simple(SimpleDeclaration),
    /// Derived data type that specifies required storage space for each instance.
    Array(ArrayDeclaration),
    Structure(StructureDeclaration),
    /// `UNION ... END_UNION` declaration (IEC 61131-3:2013 / CODESYS).
    Union(UnionDeclaration),
    String(StringDeclaration),
    /// Reference type declaration (REF_TO).
    Reference(ReferenceDeclaration),
    /// CODESYS parameter-list type declaration (`PARAMS(n) OF T`).
    Params(ParamsDeclaration),
    /// Data declaration that is ambiguous at parse time and must be
    /// resolved to a data type declaration after parsing all types.
    LateBound(LateBoundDeclaration),
}

/// Type declarations that are indistinguishable as parsing time.
/// These are one of the following without an initial value:
/// * enumeration
/// * structure
/// * simple
#[derive(Clone, Debug, PartialEq, Recurse, Attributed)]
pub struct LateBoundDeclaration {
    /// The type name of this declaration. Other library elements
    /// refer to this this type with this name.
    pub data_type_name: TypeName,
    /// The referenced type name.
    ///
    /// For example, if this is an alias then this is the underlying
    /// type.
    pub base_type_name: TypeName,
    #[recurse(ignore)]
    pub attributes: Attributes,
}

impl Located for LateBoundDeclaration {
    fn span(&self) -> SourceSpan {
        SourceSpan::join(&self.data_type_name.span(), &self.base_type_name.span())
    }
}

/// The target of a REF_TO declaration.
///
/// A reference can point to either a named type (e.g., `REF_TO INT`) or an
/// inline array type (e.g., `REF_TO ARRAY[1..10] OF INT`).
///
/// See IEC 61131-3 Edition 3, section 2.3.3.1.
#[derive(Clone, Debug, PartialEq, Recurse)]
pub enum ReferenceTarget {
    /// Reference to a named type: `REF_TO INT`
    Named(TypeName),
    /// Reference to an inline array type: `REF_TO ARRAY[1..10] OF INT`
    Array(ArraySubranges),
}

impl ReferenceTarget {
    /// Returns the type name for named targets, or `None` for inline array targets.
    pub fn type_name(&self) -> Option<&TypeName> {
        match self {
            ReferenceTarget::Named(tn) => Some(tn),
            ReferenceTarget::Array(_) => None,
        }
    }
}

/// The surface syntax that produced a reference declaration or initializer.
///
/// References share a single backend (a variable-table index), but three
/// keywords produce them: the IEC 61131-3 `REF_TO` and the Beckhoff
/// TwinCAT / CODESYS `REFERENCE TO` and `POINTER TO`. This tag records which
/// one appeared in the source so the renderer can reproduce it and so
/// per-declaration dereference behavior can be keyed on it. See
/// `specs/design/reference-to-twincat.md` and
/// `specs/design/adr-and-pointer-to.md`.
///
/// This is a leaf tag, always carried behind a `#[recurse(ignore)]` field, so
/// it deliberately does not derive `Recurse`.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum RefSyntax {
    /// IEC 61131-3 Edition 3 `REF_TO`.
    RefTo,
    /// Beckhoff TwinCAT / CODESYS `REFERENCE TO`.
    ReferenceTo,
    /// Beckhoff TwinCAT / CODESYS `POINTER TO`. Same backend as `REF_TO`
    /// with explicit `^` dereference. See
    /// `specs/design/adr-and-pointer-to.md`.
    PointerTo,
}

/// Reference type declaration (`REF_TO` or `REFERENCE TO`).
///
/// See IEC 61131-3 Edition 3, section 2.3.3.1, and
/// `specs/design/reference-to-twincat.md`.
#[derive(Clone, Debug, PartialEq, Recurse, Attributed)]
pub struct ReferenceDeclaration {
    pub type_name: TypeName,
    pub target: ReferenceTarget,
    /// The surface keyword that produced this declaration.
    #[recurse(ignore)]
    pub syntax: RefSyntax,
    #[recurse(ignore)]
    pub attributes: Attributes,
}

/// See section 2.3.3.1.
#[derive(Clone, Debug, PartialEq, Recurse, Attributed)]
pub struct EnumerationDeclaration {
    pub type_name: TypeName,
    // TODO need to understand when the context name matters in the definition
    pub spec_init: EnumeratedSpecificationInit,
    #[recurse(ignore)]
    pub attributes: Attributes,
}

/// The specification of an enumeration with a possible default value.
///
/// See section 2.3.3.1.
#[derive(Clone, Debug, PartialEq, Recurse)]
pub struct EnumeratedSpecificationInit {
    pub spec: EnumeratedSpecificationKind,
    pub default: Option<EnumeratedDefault>,
    /// Present for the CODESYS/TwinCAT base-type suffix (`(A, B) BYTE;`)
    /// -- overrides the automatic count/value-based sizing in
    /// `enumeration.rs`'s `try_from_values`. `None` uses the existing
    /// automatic sizing.
    #[recurse(ignore)]
    pub underlying_type: Option<ElementaryTypeName>,
}

impl EnumeratedSpecificationInit {
    pub fn values_and_default(values: Vec<&str>, default: &str) -> Self {
        EnumeratedSpecificationInit {
            spec: SpecificationKind::Inline(EnumeratedSpecificationValues {
                values: values.into_iter().map(EnumeratedValue::new).collect(),
            }),
            default: Some(EnumeratedDefault::Value(EnumeratedValue::new(default))),
            underlying_type: None,
        }
    }
}

/// Enumeration specification: either a reference to a named enumeration type
/// or an inline list of enumeration values.
///
/// See section 2.3.3.1.
pub type EnumeratedSpecificationKind = SpecificationKind<EnumeratedSpecificationValues>;

impl EnumeratedSpecificationKind {
    pub fn from_values(values: Vec<&'static str>) -> EnumeratedSpecificationKind {
        let values = values
            .iter()
            .map(|v| EnumeratedValue {
                type_name: None,
                value: Id::from(v),
                explicit_value: None,
            })
            .collect();
        SpecificationKind::Inline(EnumeratedSpecificationValues { values })
    }

    pub fn values(values: Vec<EnumeratedValue>) -> EnumeratedSpecificationKind {
        SpecificationKind::Inline(EnumeratedSpecificationValues { values })
    }

    pub fn recurse_visit<V: Visitor<E> + ?Sized, E>(&self, v: &mut V) -> Result<V::Value, E> {
        match self {
            SpecificationKind::Named(node) => v.visit_type_name(node),
            SpecificationKind::Inline(node) => v.visit_enumerated_specification_values(node),
        }
    }

    pub fn recurse_fold<F: Fold<E> + ?Sized, E>(self, f: &mut F) -> Result<Self, E> {
        match self {
            SpecificationKind::Named(node) => Ok(SpecificationKind::Named(f.fold_type_name(node)?)),
            SpecificationKind::Inline(node) => Ok(SpecificationKind::Inline(
                f.fold_enumerated_specification_values(node)?,
            )),
        }
    }
}

/// See section 2.3.3.1.
#[derive(Clone, Debug, PartialEq, Recurse)]
pub struct EnumeratedSpecificationValues {
    pub values: Vec<EnumeratedValue>,
}

pub trait HasEnumeratedValues {
    fn values(&self) -> &Vec<EnumeratedValue>;
    fn values_span(&self) -> SourceSpan;
}

impl HasEnumeratedValues for EnumeratedSpecificationValues {
    fn values(&self) -> &Vec<EnumeratedValue> {
        &self.values
    }
    fn values_span(&self) -> SourceSpan {
        match (self.values.first(), self.values.last()) {
            (Some(first), Some(last)) => SourceSpan::join(&first.span(), &last.span()),
            _ => SourceSpan::default(),
        }
    }
}

/// A particular value in a enumeration.
///
/// May include a type name (especially where the enumeration would be
/// ambiguous.)
///
/// See section 2.3.3.1.
#[derive(Clone, Debug, PartialEq, Recurse)]
pub struct EnumeratedValue {
    pub type_name: Option<TypeName>,
    pub value: Id,
    /// Present when this member's value was assigned explicitly
    /// (`member := 5`) in an enum *declaration's* value list -- a
    /// an extension (also standard as of IEC 61131-3:2013).
    /// `None` in every other context `EnumeratedValue` is used in
    /// (references, default values, case labels) -- only the
    /// declaration-list grammar path sets this.
    #[recurse(ignore)]
    pub explicit_value: Option<SignedInteger>,
}

impl EnumeratedValue {
    pub fn new(value: &str) -> Self {
        EnumeratedValue {
            type_name: None,
            value: Id::from(value),
            explicit_value: None,
        }
    }
}

impl Located for EnumeratedValue {
    fn span(&self) -> SourceSpan {
        match &self.type_name {
            Some(name) => SourceSpan::join(&name.span(), &self.value.span),
            None => self.value.span.clone(),
        }
    }
}

impl fmt::Display for EnumeratedValue {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match &self.type_name {
            Some(type_name) => write!(f, "{}#{}", type_name, self.value),
            None => write!(f, "{}", self.value),
        }
    }
}

/// Subrange declaration narrows a type definition to the values in a smaller
/// range. Permitted values are the inclusive range minimum through maximum
/// specified values, that is, `[minimum, maximum]`.
///
/// See section 2.3.3.1.
#[derive(Clone, Debug, PartialEq, Recurse, Attributed)]
pub struct SubrangeDeclaration {
    pub type_name: TypeName,
    pub spec: SubrangeSpecificationKind,
    pub default: Option<SignedInteger>,
    #[recurse(ignore)]
    pub attributes: Attributes,
}

/// Subrange specification: either a reference to a named subrange type
/// or an inline subrange definition.
///
/// See section 2.3.3.1.
pub type SubrangeSpecificationKind = SpecificationKind<SubrangeSpecification>;

impl SubrangeSpecificationKind {
    pub fn recurse_visit<V: Visitor<E> + ?Sized, E>(&self, v: &mut V) -> Result<V::Value, E> {
        match self {
            SpecificationKind::Named(node) => v.visit_type_name(node),
            SpecificationKind::Inline(node) => v.visit_subrange_specification(node),
        }
    }

    pub fn recurse_fold<F: Fold<E> + ?Sized, E>(self, f: &mut F) -> Result<Self, E> {
        match self {
            SpecificationKind::Named(node) => Ok(SpecificationKind::Named(f.fold_type_name(node)?)),
            SpecificationKind::Inline(node) => Ok(SpecificationKind::Inline(
                f.fold_subrange_specification(node)?,
            )),
        }
    }
}

/// A declaration against a subrange, with the initial value it may state.
///
/// A named subrange type states its initial value as a [`SimpleInitializer`]
/// does; this is the declaration of an inline subrange (`INT(1..10) := 5`),
/// which has no type name to carry one.
///
/// See section 2.3.3.1.
#[derive(Clone, Debug, PartialEq, Recurse)]
pub struct SubrangeInitializer {
    pub spec: SubrangeSpecificationKind,
    /// The value the declaration starts at. When absent, the lower bound.
    pub initial_value: Option<SignedInteger>,
}

impl SubrangeInitializer {
    /// A declaration against `spec` that states no initial value.
    pub fn uninitialized(spec: SubrangeSpecificationKind) -> Self {
        Self {
            spec,
            initial_value: None,
        }
    }
}

/// The specification for a subrange. The specification restricts an integer
/// type to a subset of the integer range.
///
/// See section 2.3.3.1.
#[derive(Clone, Debug, PartialEq, Recurse)]
pub struct SubrangeSpecification {
    /// The parent type that is being restricted.
    /// TODO how can this be restricted to integer type names?
    #[recurse(ignore)]
    pub type_name: ElementaryTypeName,
    pub subrange: Subrange,
}

/// The specification for a simple declared type: the name it declares, and the
/// type it is a copy of with the value it starts at.
///
/// An alias of a structure is one too: `P0 : Pt := (x := 1)` declares `P0`,
/// and its `spec_and_init` is a [`InitialValueAssignmentKind::Structure`] that
/// holds the structure `Pt` it copies and the values it states. Both names are
/// kept, so the declaration reads back as it was written.
///
/// See section 2.3.3.1.
#[derive(Clone, Debug, PartialEq, Recurse, Attributed)]
pub struct SimpleDeclaration {
    pub type_name: TypeName,
    pub spec_and_init: InitialValueAssignmentKind,
    #[recurse(ignore)]
    pub attributes: Attributes,
}

/// Derived data type that
#[derive(Clone, Debug, PartialEq, Recurse, Attributed)]
pub struct ArrayDeclaration {
    pub type_name: TypeName,
    pub spec: ArraySpecificationKind,
    pub init: Vec<ArrayInitialElementKind>,
    #[recurse(ignore)]
    pub attributes: Attributes,
}

#[derive(Clone, Debug, PartialEq, Recurse)]
pub enum ArrayInitialElementKind {
    Constant(ConstantKind),
    EnumValue(EnumeratedValue),
    Repeated(Repeated),
}

impl ArrayInitialElementKind {
    pub fn repeated(size: Integer, init: Option<ArrayInitialElementKind>) -> Self {
        ArrayInitialElementKind::Repeated(Repeated {
            size,
            init: Box::new(init),
        })
    }
}

#[derive(Debug, Clone, PartialEq)]
pub struct Repeated {
    pub size: Integer,
    pub init: Box<Option<ArrayInitialElementKind>>,
}

impl Repeated {
    pub fn recurse_visit<V: Visitor<E> + ?Sized, E>(&self, v: &mut V) -> Result<V::Value, E> {
        v.visit_integer(&self.size)?;
        self.init.as_ref().as_ref().map_or_else(
            || Ok(V::Value::default()),
            |val| v.visit_array_initial_element_kind(val),
        )
    }
}

impl Repeated {
    pub fn recurse_fold<F: Fold<E> + ?Sized, E>(self, f: &mut F) -> Result<Repeated, E> {
        let init = self
            .init
            .as_ref()
            .as_ref()
            .map(|x| f.fold_array_initial_element_kind(x.clone()))
            .transpose()?;

        Ok(Repeated {
            size: f.fold_integer(self.size)?,
            init: Box::new(init),
        })
    }
}

/// Structure declaration creates a combination of multiple elements (each having
/// a specific type) as a single unit. Components are accessed by a name. Structures
/// may be nested but must not contain an instance of itself.
///
/// See section 2.3.3.1.
#[derive(Clone, Debug, PartialEq, Recurse, Attributed)]
pub struct StructureDeclaration {
    /// The name of the structure.
    pub type_name: TypeName,
    /// The elements (components) of the structure declaration.
    pub elements: Vec<StructureElementDeclaration>,
    #[recurse(ignore)]
    pub attributes: Attributes,
}

/// `UNION ... END_UNION` declaration: the members are located at the same
/// position in memory, so writing one changes all of them (CODESYS
/// `UNION`, standardized in IEC 61131-3:2013).
///
/// Members reuse [`StructureElementDeclaration`]; only the storage rule
/// differs. Recognizing a union type makes it usable as a declared type
/// name; overlaying the members at offset 0 is not implemented yet (P1 in
/// `Codesys/LEXER-GAP-ANALYSIS.md`).
#[derive(Clone, Debug, PartialEq, Recurse, Attributed)]
pub struct UnionDeclaration {
    /// The name of the union.
    pub type_name: TypeName,
    /// The elements (components) of the union declaration.
    pub elements: Vec<StructureElementDeclaration>,
    #[recurse(ignore)]
    pub attributes: Attributes,
}

/// Declares an element contained within a structure.
///
/// See section 2.3.3.1.
#[derive(Clone, Debug, PartialEq, Recurse, Attributed)]
pub struct StructureElementDeclaration {
    pub name: Id,
    pub init: InitialValueAssignmentKind,
    #[recurse(ignore)]
    pub attributes: Attributes,
}

/// See section 2.3.3.1.
#[derive(Clone, Debug, PartialEq, Recurse)]
pub struct StructureInitializationDeclaration {
    pub type_name: TypeName,
    pub elements_init: Vec<StructureElementInit>,
}

/// Initializes a particular element in a structured type.
///
/// See section 2.3.3.1.
#[derive(Clone, Debug, PartialEq, Recurse)]
pub struct StructureElementInit {
    /// The name of the element in the structure to initialize.
    pub name: Id,
    pub init: StructInitialValueAssignmentKind,
}

#[derive(Clone, Debug, PartialEq)]
pub enum StringType {
    /// String of single-byte characters
    String,
    /// String of double-byte characters
    WString,
}

impl StringType {
    /// Returns the IEC 61131-3 keyword for this string type
    /// (`"STRING"` or `"WSTRING"`).
    pub fn keyword(&self) -> &'static str {
        match self {
            StringType::String => "STRING",
            StringType::WString => "WSTRING",
        }
    }

    /// Returns the character that delimits a literal of this string type:
    /// a single quote for `STRING`, a double quote for `WSTRING`.
    ///
    /// See IEC 61131-3 section 2.2.2.
    pub fn delimiter(&self) -> char {
        match self {
            StringType::String => '\'',
            StringType::WString => '"',
        }
    }
}

/// Declares a string type with restricted length.
///
/// See section 2.3.3.1.
#[derive(Clone, Debug, PartialEq, Recurse, Attributed)]
pub struct StringDeclaration {
    pub type_name: TypeName,
    pub length: IntegerRef,
    /// The size of a single 'character'
    #[recurse(ignore)]
    pub width: StringType,
    /// Default value of the type. The literal's width is the declared width
    /// (the grammar accepts either delimiter here and the declaration
    /// decides), and it keeps its own span so a check on the literal can
    /// point at it.
    pub init: Option<CharacterStringLiteral>,
    #[recurse(ignore)]
    pub attributes: Attributes,
}

/// Array specification: either a reference to a named array type
/// or an inline array definition with explicit subranges.
///
/// See section 2.3.3.1.
pub type ArraySpecificationKind = SpecificationKind<ArraySubranges>;

impl ArraySpecificationKind {
    pub fn recurse_visit<V: Visitor<E> + ?Sized, E>(&self, v: &mut V) -> Result<V::Value, E> {
        match self {
            SpecificationKind::Named(node) => v.visit_type_name(node),
            SpecificationKind::Inline(node) => v.visit_array_subranges(node),
        }
    }

    pub fn recurse_fold<F: Fold<E> + ?Sized, E>(self, f: &mut F) -> Result<Self, E> {
        match self {
            SpecificationKind::Named(node) => Ok(SpecificationKind::Named(f.fold_type_name(node)?)),
            SpecificationKind::Inline(node) => {
                Ok(SpecificationKind::Inline(f.fold_array_subranges(node)?))
            }
        }
    }
}

/// The CODESYS parameter-list type `PARAMS(n) OF T`.
///
/// The count names how many parameters the list holds and the type is the
/// type every one of them has. The elements are addressed by index, like an
/// array, so the type is provisionally lowered to an array with bounds
/// `0 .. n-1` (see the analyzer's `xform_resolve_decl_types`); the variadic
/// call-site semantics of a PARAMS parameter are not implemented yet.
#[derive(Clone, Debug, PartialEq, Recurse)]
pub struct ParamsSpecification {
    /// The number of parameters, as an integer literal or a named constant.
    pub count: IntegerRef,
    /// The type of every parameter in the list.
    pub type_name: TypeName,
}

/// A `PARAMS(n) OF T` type declaration, e.g.
/// `TYPE MyParams : PARAMS(3) OF INT; END_TYPE`.
#[derive(Clone, Debug, PartialEq, Recurse, Attributed)]
pub struct ParamsDeclaration {
    /// The type name of this declaration. Other library elements refer to
    /// this type with this name.
    pub type_name: TypeName,
    pub spec: ParamsSpecification,
    #[recurse(ignore)]
    pub attributes: Attributes,
}

impl Located for ParamsDeclaration {
    fn span(&self) -> SourceSpan {
        SourceSpan::join(&self.type_name.span(), &self.spec.type_name.span())
    }
}

/// The element type of an array declaration.
///
/// Distinguishes between named types (e.g. `INT`, `MY_TYPE`) and sized string
/// types (e.g. `STRING[10]`, `WSTRING[100]`). Modeled after [`FunctionReturnType`].
#[derive(Clone, Debug, PartialEq, Recurse)]
pub enum ArrayElementType {
    /// A named type (elementary or derived), e.g. `INT`, `REAL`, `MY_TYPE`.
    Named(TypeName),
    /// `STRING` with optional length, e.g. `STRING[10]`.
    String(StringSpecification),
    /// `WSTRING` with optional length, e.g. `WSTRING[100]`.
    WString(StringSpecification),
}

impl ArrayElementType {
    /// Returns the type name suitable for type resolution.
    ///
    /// For `Named`, returns the inner `TypeName`.
    /// For `String`/`WString`, returns `TypeName::from("STRING")` or `TypeName::from("WSTRING")`.
    pub fn to_type_name(&self) -> TypeName {
        match self {
            ArrayElementType::Named(tn) => tn.clone(),
            ArrayElementType::String(_) => TypeName::from("STRING"),
            ArrayElementType::WString(_) => TypeName::from("WSTRING"),
        }
    }
}

/// The index extent of an array declaration.
///
/// CODESYS allows the bounds to be left open with `*` instead of a range list
/// (`arrayType = "ARRAY" "[" ("*" | indexRange {"," indexRange}) "]" "OF"
/// dataType`). The two forms are alternatives in the grammar, so they are
/// alternatives here: an `ARRAY[*]` declaration has *no* range list, which a
/// `Vec` plus a separate "is incomplete" flag could not state without
/// inventing an empty list that no source can spell.
#[derive(Clone, Debug, PartialEq, Recurse)]
pub enum ArrayBounds {
    /// One or more explicit index ranges: `ARRAY[1..3, 0..2] OF INT`.
    /// The list is never empty.
    Ranges(Vec<Subrange>),
    /// The incomplete array type `ARRAY[*] OF INT`: the element count is
    /// supplied by the caller (CODESYS extension). Carries the span of the
    /// `*` so a later pass can point at the spelling that decided the bounds.
    Incomplete(SourceSpan),
}

#[derive(Clone, Debug, PartialEq, Recurse)]
pub struct ArraySubranges {
    pub bounds: ArrayBounds,
    pub type_name: ArrayElementType,
    /// The reference syntax of the element type, if any. `None` for a
    /// non-reference element; `Some(_)` when the element is wrapped in a
    /// reference keyword (`REF_TO` or `REFERENCE TO`), tagged with which one.
    #[recurse(ignore)]
    pub ref_to: Option<RefSyntax>,
}

impl ArraySubranges {
    /// The explicit index ranges, or an empty slice for the incomplete form.
    ///
    /// Callers that do not care about the difference (bounds validation, code
    /// generation over a concrete array) read the ranges through here; a
    /// caller that must treat the incomplete form specially matches on
    /// [`Self::bounds`].
    pub fn ranges(&self) -> &[Subrange] {
        match &self.bounds {
            ArrayBounds::Ranges(ranges) => ranges,
            ArrayBounds::Incomplete(_) => &[],
        }
    }

    /// The span of the `*` for the incomplete `ARRAY[*]` form, if this is
    /// that form.
    pub fn incomplete_span(&self) -> Option<&SourceSpan> {
        match &self.bounds {
            ArrayBounds::Ranges(_) => None,
            ArrayBounds::Incomplete(span) => Some(span),
        }
    }
}

/// Subrange of an array.
///
/// See section 2.4.2.1.
#[derive(Clone, Debug, PartialEq, Recurse)]
pub struct Subrange {
    pub start: SignedIntegerRef,
    pub end: SignedIntegerRef,
}
