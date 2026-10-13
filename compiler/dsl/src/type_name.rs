//! The names of types: the identifier of a declared type, the elementary type
//! names and the generic type names of IEC 61131-3.
//!
//! Re-exported from [`crate::common`], so these types are also reachable as
//! `common::TypeName` and so on.
use std::fmt;
use std::hash::{Hash, Hasher};

use dsl_macro_derive::{Located, Recurse};

use crate::core::Id;
use crate::fold::Fold;
use crate::visitor::Visitor;

/// Implements a type identifier.
///
/// Types are all identifiers but we use a separate structure
/// because it is convenient to treat types and other identifiers
/// separately.
#[derive(Clone, Debug, PartialEq, Recurse, Located)]
pub struct TypeName {
    #[located(delegate)]
    pub name: Id,
}

impl TypeName {
    /// Converts a `&str` into an `Identifier`.
    pub fn from(str: &str) -> Self {
        Self {
            name: Id::from(str),
        }
    }

    pub fn from_id(name: &Id) -> Self {
        Self { name: name.clone() }
    }
}

impl Eq for TypeName {}

impl Hash for TypeName {
    fn hash<H: Hasher>(&self, state: &mut H) {
        self.name.hash(state);
    }
}

impl fmt::Display for TypeName {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(f, "{}", self.name)
    }
}

/// Represents the type of a variable declaration.
///
/// This enum distinguishes between named types (which can be looked up in a
/// type environment) and inline/anonymous type definitions (which have no
/// named type to look up).
#[derive(Clone, Debug, PartialEq)]
pub enum TypeReference {
    /// A reference to a named type that can be resolved in the type environment.
    Named(TypeName),
    /// An inline/anonymous type definition with no type name to look up.
    /// Examples: `ARRAY[1..10] OF INT`, `INT(0..100)`, `(RED, GREEN, BLUE)`.
    Inline,
    /// No type specification was provided.
    Unspecified,
}

/// Generic specification kind that distinguishes between a named type reference
/// and an inline type definition.
///
/// This pattern is used for enumerations, subranges, and arrays where a
/// declaration can either reference an existing named type or define the
/// type inline.
///
/// See section 2.3.3.1.
#[derive(Clone, Debug, PartialEq)]
pub enum SpecificationKind<T> {
    /// A reference to a named type.
    Named(TypeName),
    /// An inline type definition.
    Inline(T),
}

/// Valid type names for integer literals.
///
/// These are the types that can prefix an integer literal (e.g., `INT#42`).
/// See section 2.2.1.
#[derive(Debug, PartialEq, Clone)]
pub enum IntegerTypeName {
    SINT,
    INT,
    DINT,
    LINT,
    USINT,
    UINT,
    UDINT,
    ULINT,
}

impl fmt::Display for IntegerTypeName {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        let name = match self {
            IntegerTypeName::SINT => "SINT",
            IntegerTypeName::INT => "INT",
            IntegerTypeName::DINT => "DINT",
            IntegerTypeName::LINT => "LINT",
            IntegerTypeName::USINT => "USINT",
            IntegerTypeName::UINT => "UINT",
            IntegerTypeName::UDINT => "UDINT",
            IntegerTypeName::ULINT => "ULINT",
        };
        write!(f, "{name}")
    }
}

/// Valid type names for real (floating-point) literals.
///
/// These are the types that can prefix a real literal (e.g., `REAL#3.14`).
/// See section 2.2.1.
#[derive(Debug, PartialEq, Clone)]
pub enum RealTypeName {
    REAL,
    LREAL,
}

impl fmt::Display for RealTypeName {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        let name = match self {
            RealTypeName::REAL => "REAL",
            RealTypeName::LREAL => "LREAL",
        };
        write!(f, "{name}")
    }
}

/// Valid type names for bit string literals.
///
/// These are the types that can prefix a bit string literal (e.g., `BYTE#16#FF`).
/// See section 2.2.1.
#[derive(Debug, PartialEq, Clone)]
pub enum BitStringTypeName {
    BYTE,
    WORD,
    DWORD,
    LWORD,
}

impl fmt::Display for BitStringTypeName {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        let name = match self {
            BitStringTypeName::BYTE => "BYTE",
            BitStringTypeName::WORD => "WORD",
            BitStringTypeName::DWORD => "DWORD",
            BitStringTypeName::LWORD => "LWORD",
        };
        write!(f, "{name}")
    }
}

/// Elementary type names.
///
/// See section 2.3.1.
#[derive(Debug, PartialEq, Eq, Clone)]
pub enum ElementaryTypeName {
    BOOL,
    /// The one-bit type some vendor toolchains add to the standard
    /// elementary types. IronPLC stores it as [`BOOL`](Self::BOOL) (see
    /// `specs/design/numeric-literals.md`, REQ-NL-parser-020).
    BIT,
    SINT,
    INT,
    DINT,
    LINT,
    USINT,
    UINT,
    UDINT,
    ULINT,
    REAL,
    LREAL,
    TIME,
    LTIME,
    DATE,
    LDATE,
    TimeOfDay,
    LTimeOfDay,
    DateAndTime,
    LDateAndTime,
    STRING,
    BYTE,
    WORD,
    DWORD,
    LWORD,
    WSTRING,
}

/// Type family for widening classification. See ADR-0031.
#[derive(Debug, PartialEq)]
enum TypeFamily {
    SignedInteger,
    UnsignedInteger,
    Real,
    BitString,
}

impl ElementaryTypeName {
    pub fn as_id(&self) -> Id {
        match self {
            ElementaryTypeName::BOOL => Id::from("BOOL"),
            ElementaryTypeName::BIT => Id::from("BIT"),
            ElementaryTypeName::SINT => Id::from("SINT"),
            ElementaryTypeName::INT => Id::from("INT"),
            ElementaryTypeName::DINT => Id::from("DINT"),
            ElementaryTypeName::LINT => Id::from("LINT"),
            ElementaryTypeName::USINT => Id::from("USINT"),
            ElementaryTypeName::UINT => Id::from("UINT"),
            ElementaryTypeName::UDINT => Id::from("UDINT"),
            ElementaryTypeName::ULINT => Id::from("ULINT"),
            ElementaryTypeName::REAL => Id::from("REAL"),
            ElementaryTypeName::LREAL => Id::from("LREAL"),
            ElementaryTypeName::TIME => Id::from("TIME"),
            ElementaryTypeName::LTIME => Id::from("LTIME"),
            ElementaryTypeName::DATE => Id::from("DATE"),
            ElementaryTypeName::LDATE => Id::from("LDATE"),
            ElementaryTypeName::TimeOfDay => Id::from("TIME_OF_DAY"),
            ElementaryTypeName::LTimeOfDay => Id::from("LTIME_OF_DAY"),
            ElementaryTypeName::DateAndTime => Id::from("DATE_AND_TIME"),
            ElementaryTypeName::LDateAndTime => Id::from("LDATE_AND_TIME"),
            ElementaryTypeName::STRING => Id::from("STRING"),
            ElementaryTypeName::BYTE => Id::from("BYTE"),
            ElementaryTypeName::WORD => Id::from("WORD"),
            ElementaryTypeName::DWORD => Id::from("DWORD"),
            ElementaryTypeName::LWORD => Id::from("LWORD"),
            ElementaryTypeName::WSTRING => Id::from("WSTRING"),
        }
    }

    /// Returns true if this is an integer type (signed or unsigned).
    pub fn is_integer(&self) -> bool {
        matches!(
            self,
            ElementaryTypeName::SINT
                | ElementaryTypeName::INT
                | ElementaryTypeName::DINT
                | ElementaryTypeName::LINT
                | ElementaryTypeName::USINT
                | ElementaryTypeName::UINT
                | ElementaryTypeName::UDINT
                | ElementaryTypeName::ULINT
        )
    }

    /// Returns the type family and bit width, or `None` for types that do
    /// not participate in implicit widening (BOOL, TIME, DATE, STRING, etc.).
    fn type_properties(&self) -> Option<(TypeFamily, u8)> {
        use TypeFamily::*;
        match self {
            ElementaryTypeName::SINT => Some((SignedInteger, 8)),
            ElementaryTypeName::INT => Some((SignedInteger, 16)),
            ElementaryTypeName::DINT => Some((SignedInteger, 32)),
            ElementaryTypeName::LINT => Some((SignedInteger, 64)),
            ElementaryTypeName::USINT => Some((UnsignedInteger, 8)),
            ElementaryTypeName::UINT => Some((UnsignedInteger, 16)),
            ElementaryTypeName::UDINT => Some((UnsignedInteger, 32)),
            ElementaryTypeName::ULINT => Some((UnsignedInteger, 64)),
            ElementaryTypeName::REAL => Some((Real, 32)),
            ElementaryTypeName::LREAL => Some((Real, 64)),
            ElementaryTypeName::BYTE => Some((BitString, 8)),
            ElementaryTypeName::WORD => Some((BitString, 16)),
            ElementaryTypeName::DWORD => Some((BitString, 32)),
            ElementaryTypeName::LWORD => Some((BitString, 64)),
            _ => None,
        }
    }

    /// Returns true if `self` can be implicitly widened to `target` under
    /// standard IEC 61131-3 rules (no flag required). See ADR-0029/0031.
    ///
    /// Allowed widening conversions:
    /// - Signed integer chain: SINT → INT → DINT → LINT
    /// - Unsigned integer chain: USINT → UINT → UDINT → ULINT
    /// - Cross-sign: unsigned n-bit → signed m-bit where m > n
    /// - Integer → REAL (lossless only): src ≤ 16 bits → REAL, any → LREAL
    /// - REAL → LREAL (lossless: 32-bit float → 64-bit float)
    /// - Bit-string chain: BYTE → WORD → DWORD → LWORD (BOOL excluded)
    ///
    /// Not allowed: signed → unsigned, narrowing, cross-family (bit-string ↔ integer).
    pub fn can_widen_to(&self, target: &ElementaryTypeName) -> bool {
        use TypeFamily::*;
        let Some((src_family, src_bits)) = self.type_properties() else {
            return false;
        };
        let Some((tgt_family, tgt_bits)) = target.type_properties() else {
            return false;
        };
        match (&src_family, &tgt_family) {
            // Same-family integer widening
            (SignedInteger, SignedInteger) | (UnsignedInteger, UnsignedInteger) => {
                tgt_bits > src_bits
            }
            // Unsigned → signed: target must be strictly wider
            (UnsignedInteger, SignedInteger) => tgt_bits > src_bits,
            // Signed → unsigned: never allowed
            (SignedInteger, UnsignedInteger) => false,
            // Integer → Real (lossless only, ADR-0031):
            //   LREAL (64-bit, 52-bit mantissa) covers all integers up to 52 bits
            //   REAL (32-bit, 23-bit mantissa) covers integers up to 16 bits
            (SignedInteger | UnsignedInteger, Real) => {
                if tgt_bits == 64 {
                    true // Any integer → LREAL
                } else {
                    src_bits <= 16 // Only ≤16-bit integers → REAL
                }
            }
            // REAL → LREAL widening (lossless: 32-bit float safely widens to
            // 64-bit float). LREAL → REAL is narrowing and NOT allowed here.
            (Real, Real) => tgt_bits > src_bits,
            // Bit-string widening within ANY_BIT family
            (BitString, BitString) => tgt_bits > src_bits,
            // Everything else (cross-family, Real→Int, etc.): not standard
            _ => false,
        }
    }

    /// Returns true if `self` can be implicitly *widened* to `target` under
    /// cross-family rules (requires `--allow-cross-family-widening`).
    ///
    /// Widening moves a value into a type that can hold strictly more: only
    /// bit-string → integer, and only where the target is strictly wider.
    /// Integer → bit-string never widens, and equal-width pairs widen
    /// nothing, so neither is answered here. For the one equal-width pair
    /// that converts implicitly, see [`Self::can_convert_cross_family_to`].
    pub fn can_widen_cross_family_to(&self, target: &ElementaryTypeName) -> bool {
        use TypeFamily::*;
        let Some((src_family, src_bits)) = self.type_properties() else {
            return false;
        };
        let Some((tgt_family, tgt_bits)) = target.type_properties() else {
            return false;
        };
        match (&src_family, &tgt_family) {
            (BitString, SignedInteger | UnsignedInteger) => tgt_bits > src_bits,
            _ => false,
        }
    }

    /// Returns true if `self` can be implicitly *converted* to `target` at
    /// equal width under cross-family rules (requires
    /// `--allow-cross-family-conversion`).
    ///
    /// This is not widening: the two types occupy the same slot, so the
    /// conversion is a bit-pattern no-op and runs in both directions. It is
    /// also the one case where integer → bit-string is implicit.
    ///
    /// Exactly one pair qualifies: `UDINT` ↔ `DWORD` (32-bit). Beckhoff's own
    /// documentation states no implicit conversion exists between bit-string
    /// and integer types even at equal width, but this was confirmed
    /// permissive against a real TcXaeShell build. Other same-width
    /// bit-string/unsigned-integer pairs (`BYTE`↔`USINT`, `WORD`↔`UINT`,
    /// `LWORD`↔`ULINT`) and signed integers are not verified and must not be
    /// assumed to behave the same.
    pub fn can_convert_cross_family_to(&self, target: &ElementaryTypeName) -> bool {
        use ElementaryTypeName::{DWORD, UDINT};
        use TypeFamily::*;
        let Some((src_family, src_bits)) = self.type_properties() else {
            return false;
        };
        let Some((tgt_family, tgt_bits)) = target.type_properties() else {
            return false;
        };
        if src_bits != tgt_bits {
            return false;
        }
        match (&src_family, &tgt_family) {
            (BitString, UnsignedInteger) | (UnsignedInteger, BitString) => {
                matches!(self, DWORD | UDINT) && matches!(target, DWORD | UDINT)
            }
            _ => false,
        }
    }
}

impl fmt::Display for ElementaryTypeName {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(f, "{}", self.as_id())
    }
}

impl From<ElementaryTypeName> for Id {
    fn from(value: ElementaryTypeName) -> Id {
        match value {
            ElementaryTypeName::BOOL => Id::from("BOOL"),
            ElementaryTypeName::BIT => Id::from("BIT"),
            ElementaryTypeName::SINT => Id::from("SINT"),
            ElementaryTypeName::INT => Id::from("INT"),
            ElementaryTypeName::DINT => Id::from("DINT"),
            ElementaryTypeName::LINT => Id::from("LINT"),
            ElementaryTypeName::USINT => Id::from("USINT"),
            ElementaryTypeName::UINT => Id::from("UINT"),
            ElementaryTypeName::UDINT => Id::from("UDINT"),
            ElementaryTypeName::ULINT => Id::from("ULINT"),
            ElementaryTypeName::REAL => Id::from("REAL"),
            ElementaryTypeName::LREAL => Id::from("LREAL"),
            ElementaryTypeName::TIME => Id::from("TIME"),
            ElementaryTypeName::LTIME => Id::from("LTIME"),
            ElementaryTypeName::DATE => Id::from("DATE"),
            ElementaryTypeName::LDATE => Id::from("LDATE"),
            ElementaryTypeName::TimeOfDay => Id::from("TIME_OF_DAY"),
            ElementaryTypeName::LTimeOfDay => Id::from("LTIME_OF_DAY"),
            ElementaryTypeName::DateAndTime => Id::from("DATE_AND_TIME"),
            ElementaryTypeName::LDateAndTime => Id::from("LDATE_AND_TIME"),
            ElementaryTypeName::STRING => Id::from("STRING"),
            ElementaryTypeName::BYTE => Id::from("BYTE"),
            ElementaryTypeName::WORD => Id::from("WORD"),
            ElementaryTypeName::DWORD => Id::from("DWORD"),
            ElementaryTypeName::LWORD => Id::from("LWORD"),
            ElementaryTypeName::WSTRING => Id::from("WSTRING"),
        }
    }
}

impl From<ElementaryTypeName> for TypeName {
    fn from(value: ElementaryTypeName) -> TypeName {
        match value {
            ElementaryTypeName::BOOL => TypeName::from("BOOL"),
            ElementaryTypeName::BIT => TypeName::from("BIT"),
            ElementaryTypeName::SINT => TypeName::from("SINT"),
            ElementaryTypeName::INT => TypeName::from("INT"),
            ElementaryTypeName::DINT => TypeName::from("DINT"),
            ElementaryTypeName::LINT => TypeName::from("LINT"),
            ElementaryTypeName::USINT => TypeName::from("USINT"),
            ElementaryTypeName::UINT => TypeName::from("UINT"),
            ElementaryTypeName::UDINT => TypeName::from("UDINT"),
            ElementaryTypeName::ULINT => TypeName::from("ULINT"),
            ElementaryTypeName::REAL => TypeName::from("REAL"),
            ElementaryTypeName::LREAL => TypeName::from("LREAL"),
            ElementaryTypeName::TIME => TypeName::from("TIME"),
            ElementaryTypeName::LTIME => TypeName::from("LTIME"),
            ElementaryTypeName::DATE => TypeName::from("DATE"),
            ElementaryTypeName::LDATE => TypeName::from("LDATE"),
            ElementaryTypeName::TimeOfDay => TypeName::from("TIME_OF_DAY"),
            ElementaryTypeName::LTimeOfDay => TypeName::from("LTIME_OF_DAY"),
            ElementaryTypeName::DateAndTime => TypeName::from("DATE_AND_TIME"),
            ElementaryTypeName::LDateAndTime => TypeName::from("LDATE_AND_TIME"),
            ElementaryTypeName::STRING => TypeName::from("STRING"),
            ElementaryTypeName::BYTE => TypeName::from("BYTE"),
            ElementaryTypeName::WORD => TypeName::from("WORD"),
            ElementaryTypeName::DWORD => TypeName::from("DWORD"),
            ElementaryTypeName::LWORD => TypeName::from("LWORD"),
            ElementaryTypeName::WSTRING => TypeName::from("WSTRING"),
        }
    }
}

impl TryFrom<&Id> for ElementaryTypeName {
    type Error = ();
    fn try_from(id: &Id) -> Result<Self, ()> {
        match id.lower_case().as_str() {
            "bool" => Ok(ElementaryTypeName::BOOL),
            "bit" => Ok(ElementaryTypeName::BIT),
            "sint" => Ok(ElementaryTypeName::SINT),
            "int" => Ok(ElementaryTypeName::INT),
            "dint" => Ok(ElementaryTypeName::DINT),
            "lint" => Ok(ElementaryTypeName::LINT),
            "usint" => Ok(ElementaryTypeName::USINT),
            "uint" => Ok(ElementaryTypeName::UINT),
            "udint" => Ok(ElementaryTypeName::UDINT),
            "ulint" => Ok(ElementaryTypeName::ULINT),
            "real" => Ok(ElementaryTypeName::REAL),
            "lreal" => Ok(ElementaryTypeName::LREAL),
            "time" => Ok(ElementaryTypeName::TIME),
            "ltime" => Ok(ElementaryTypeName::LTIME),
            "date" => Ok(ElementaryTypeName::DATE),
            "ldate" => Ok(ElementaryTypeName::LDATE),
            "time_of_day" | "tod" => Ok(ElementaryTypeName::TimeOfDay),
            "ltime_of_day" | "ltod" => Ok(ElementaryTypeName::LTimeOfDay),
            "date_and_time" | "dt" => Ok(ElementaryTypeName::DateAndTime),
            "ldate_and_time" | "ldt" => Ok(ElementaryTypeName::LDateAndTime),
            "string" => Ok(ElementaryTypeName::STRING),
            "byte" => Ok(ElementaryTypeName::BYTE),
            "word" => Ok(ElementaryTypeName::WORD),
            "dword" => Ok(ElementaryTypeName::DWORD),
            "lword" => Ok(ElementaryTypeName::LWORD),
            "wstring" => Ok(ElementaryTypeName::WSTRING),
            _ => Err(()),
        }
    }
}

impl IntegerTypeName {
    pub fn as_id(&self) -> Id {
        match self {
            IntegerTypeName::SINT => Id::from("SINT"),
            IntegerTypeName::INT => Id::from("INT"),
            IntegerTypeName::DINT => Id::from("DINT"),
            IntegerTypeName::LINT => Id::from("LINT"),
            IntegerTypeName::USINT => Id::from("USINT"),
            IntegerTypeName::UINT => Id::from("UINT"),
            IntegerTypeName::UDINT => Id::from("UDINT"),
            IntegerTypeName::ULINT => Id::from("ULINT"),
        }
    }
}

impl From<IntegerTypeName> for ElementaryTypeName {
    fn from(value: IntegerTypeName) -> ElementaryTypeName {
        match value {
            IntegerTypeName::SINT => ElementaryTypeName::SINT,
            IntegerTypeName::INT => ElementaryTypeName::INT,
            IntegerTypeName::DINT => ElementaryTypeName::DINT,
            IntegerTypeName::LINT => ElementaryTypeName::LINT,
            IntegerTypeName::USINT => ElementaryTypeName::USINT,
            IntegerTypeName::UINT => ElementaryTypeName::UINT,
            IntegerTypeName::UDINT => ElementaryTypeName::UDINT,
            IntegerTypeName::ULINT => ElementaryTypeName::ULINT,
        }
    }
}

impl RealTypeName {
    pub fn as_id(&self) -> Id {
        match self {
            RealTypeName::REAL => Id::from("REAL"),
            RealTypeName::LREAL => Id::from("LREAL"),
        }
    }
}

impl From<RealTypeName> for ElementaryTypeName {
    fn from(value: RealTypeName) -> ElementaryTypeName {
        match value {
            RealTypeName::REAL => ElementaryTypeName::REAL,
            RealTypeName::LREAL => ElementaryTypeName::LREAL,
        }
    }
}

impl BitStringTypeName {
    pub fn as_id(&self) -> Id {
        match self {
            BitStringTypeName::BYTE => Id::from("BYTE"),
            BitStringTypeName::WORD => Id::from("WORD"),
            BitStringTypeName::DWORD => Id::from("DWORD"),
            BitStringTypeName::LWORD => Id::from("LWORD"),
        }
    }
}

impl From<BitStringTypeName> for ElementaryTypeName {
    fn from(value: BitStringTypeName) -> ElementaryTypeName {
        match value {
            BitStringTypeName::BYTE => ElementaryTypeName::BYTE,
            BitStringTypeName::WORD => ElementaryTypeName::WORD,
            BitStringTypeName::DWORD => ElementaryTypeName::DWORD,
            BitStringTypeName::LWORD => ElementaryTypeName::LWORD,
        }
    }
}

/// Generic type names used for polymorphic function signatures.
///
/// These types form a hierarchy where each generic type matches
/// a set of concrete types. Used primarily in standard library
/// function signatures to indicate polymorphism.
///
/// See section B.1.3.2 of IEC 61131-3.
#[derive(Debug, PartialEq, Clone)]
pub enum GenericTypeName {
    /// Matches any type
    Any,
    /// Matches any user-defined type (structures, enumerations, function blocks)
    AnyDerived,
    /// Matches any elementary type
    AnyElementary,
    /// Matches TIME and ANY_NUM types
    AnyMagnitude,
    /// Matches ANY_REAL and ANY_INT types
    AnyNum,
    /// Matches REAL and LREAL
    AnyReal,
    /// Matches all integer types (SINT, INT, DINT, LINT, USINT, UINT, UDINT, ULINT)
    AnyInt,
    /// Matches bit string types (BOOL, BYTE, WORD, DWORD, LWORD)
    AnyBit,
    /// Matches string types (STRING, WSTRING)
    AnyString,
    /// Matches date/time types (DATE, TIME_OF_DAY, DATE_AND_TIME)
    AnyDate,
}

impl GenericTypeName {
    /// Returns the string representation of the generic type name.
    pub fn as_str(&self) -> &'static str {
        match self {
            GenericTypeName::Any => "ANY",
            GenericTypeName::AnyDerived => "ANY_DERIVED",
            GenericTypeName::AnyElementary => "ANY_ELEMENTARY",
            GenericTypeName::AnyMagnitude => "ANY_MAGNITUDE",
            GenericTypeName::AnyNum => "ANY_NUM",
            GenericTypeName::AnyReal => "ANY_REAL",
            GenericTypeName::AnyInt => "ANY_INT",
            GenericTypeName::AnyBit => "ANY_BIT",
            GenericTypeName::AnyString => "ANY_STRING",
            GenericTypeName::AnyDate => "ANY_DATE",
        }
    }

    /// Checks if a concrete elementary type is compatible with this generic type.
    ///
    /// Returns true if the elementary type can be used where this generic type
    /// is expected (e.g., INT is compatible with ANY_INT, ANY_NUM, ANY_MAGNITUDE,
    /// ANY_ELEMENTARY, and ANY).
    pub fn is_compatible_with(&self, elementary: &ElementaryTypeName) -> bool {
        match self {
            GenericTypeName::Any => true,
            GenericTypeName::AnyDerived => false, // Elementary types are not derived
            GenericTypeName::AnyElementary => true,
            GenericTypeName::AnyMagnitude => {
                matches!(
                    elementary,
                    ElementaryTypeName::TIME
                        | ElementaryTypeName::LTIME
                        | ElementaryTypeName::SINT
                        | ElementaryTypeName::INT
                        | ElementaryTypeName::DINT
                        | ElementaryTypeName::LINT
                        | ElementaryTypeName::USINT
                        | ElementaryTypeName::UINT
                        | ElementaryTypeName::UDINT
                        | ElementaryTypeName::ULINT
                        | ElementaryTypeName::REAL
                        | ElementaryTypeName::LREAL
                )
            }
            GenericTypeName::AnyNum => {
                matches!(
                    elementary,
                    ElementaryTypeName::SINT
                        | ElementaryTypeName::INT
                        | ElementaryTypeName::DINT
                        | ElementaryTypeName::LINT
                        | ElementaryTypeName::USINT
                        | ElementaryTypeName::UINT
                        | ElementaryTypeName::UDINT
                        | ElementaryTypeName::ULINT
                        | ElementaryTypeName::REAL
                        | ElementaryTypeName::LREAL
                )
            }
            GenericTypeName::AnyReal => {
                matches!(
                    elementary,
                    ElementaryTypeName::REAL | ElementaryTypeName::LREAL
                )
            }
            GenericTypeName::AnyInt => {
                matches!(
                    elementary,
                    ElementaryTypeName::SINT
                        | ElementaryTypeName::INT
                        | ElementaryTypeName::DINT
                        | ElementaryTypeName::LINT
                        | ElementaryTypeName::USINT
                        | ElementaryTypeName::UINT
                        | ElementaryTypeName::UDINT
                        | ElementaryTypeName::ULINT
                )
            }
            GenericTypeName::AnyBit => {
                matches!(
                    elementary,
                    ElementaryTypeName::BOOL
                        | ElementaryTypeName::BYTE
                        | ElementaryTypeName::WORD
                        | ElementaryTypeName::DWORD
                        | ElementaryTypeName::LWORD
                )
            }
            GenericTypeName::AnyString => {
                matches!(
                    elementary,
                    ElementaryTypeName::STRING | ElementaryTypeName::WSTRING
                )
            }
            GenericTypeName::AnyDate => {
                matches!(
                    elementary,
                    ElementaryTypeName::DATE
                        | ElementaryTypeName::TimeOfDay
                        | ElementaryTypeName::DateAndTime
                )
            }
        }
    }
}

impl From<GenericTypeName> for Id {
    fn from(value: GenericTypeName) -> Id {
        Id::from(value.as_str())
    }
}

impl From<GenericTypeName> for TypeName {
    fn from(value: GenericTypeName) -> TypeName {
        TypeName::from(value.as_str())
    }
}

impl TryFrom<&Id> for GenericTypeName {
    type Error = ();
    fn try_from(id: &Id) -> Result<Self, ()> {
        match id.lower_case().as_str() {
            "any" => Ok(GenericTypeName::Any),
            "any_derived" => Ok(GenericTypeName::AnyDerived),
            "any_elementary" => Ok(GenericTypeName::AnyElementary),
            "any_magnitude" => Ok(GenericTypeName::AnyMagnitude),
            "any_num" => Ok(GenericTypeName::AnyNum),
            "any_real" => Ok(GenericTypeName::AnyReal),
            "any_int" => Ok(GenericTypeName::AnyInt),
            "any_bit" => Ok(GenericTypeName::AnyBit),
            "any_string" => Ok(GenericTypeName::AnyString),
            "any_date" => Ok(GenericTypeName::AnyDate),
            _ => Err(()),
        }
    }
}
