//! Variable declarations: their identifiers, types, qualifiers and directly represented addresses.

use regex::Regex;
use std::fmt::{self, Display};

use dsl_macro_derive::{Attributed, Located, Recurse};

use super::{
    Attributes, EnumeratedInitialValueAssignment, EnumeratedValue,
    FunctionBlockInitialValueAssignment, InitialValueAssignmentKind, LateResolvedInitialValue,
    LateResolvedInitializer, StringInitializer, StringType, StructureInitializationDeclaration,
    TypeName, TypeReference,
};
use crate::configuration::Direction;
use crate::core::{Id, Located, SourceSpan};
use crate::fold::Fold;
use crate::textual::*;
use crate::type_id::TypeId;
use crate::visitor::Visitor;

/// Location prefix for directly represented variables.
///
/// See section 2.4.1.1.
#[derive(Clone, Debug, PartialEq)]
pub enum LocationPrefix {
    /// Input location
    I,
    /// Output location
    Q,
    /// Memory location
    M,
}

impl TryFrom<Option<char>> for LocationPrefix {
    type Error = &'static str;

    fn try_from(value: Option<char>) -> Result<Self, Self::Error> {
        match value {
            Some('I') => Ok(LocationPrefix::I),
            Some('Q') => Ok(LocationPrefix::Q),
            Some('M') => Ok(LocationPrefix::M),
            _ => Err("Value must be one of I, Q, M"),
        }
    }
}

impl TryFrom<&str> for LocationPrefix {
    type Error = &'static str;

    fn try_from(value: &str) -> Result<Self, Self::Error> {
        let c = value.chars().nth(0);
        LocationPrefix::try_from(c)
    }
}

/// Size prefix for directly represented variables. Defines how many bits
/// are associated with the variable.
///
/// See section 2.4.1.1.
#[derive(Clone, Debug, PartialEq)]
pub enum SizePrefix {
    /// Unspecified (indicated by asterisk)
    Unspecified,
    /// Single bit size
    Nil,
    /// Single bit size
    X,
    /// 8-bit size
    B,
    /// 16-bit size
    W,
    /// 32-bit size
    D,
    /// 64-bit size
    L,
}

impl TryFrom<Option<char>> for SizePrefix {
    type Error = &'static str;

    fn try_from(value: Option<char>) -> Result<Self, Self::Error> {
        match value {
            Some('*') => Ok(SizePrefix::Unspecified),
            Some('X') => Ok(SizePrefix::X),
            Some('B') => Ok(SizePrefix::B),
            Some('W') => Ok(SizePrefix::W),
            Some('D') => Ok(SizePrefix::D),
            Some('L') => Ok(SizePrefix::L),
            None => Ok(SizePrefix::Nil),
            _ => Err("Value must be one of *, X, B, W, D, L, NIL"),
        }
    }
}

impl TryFrom<&str> for SizePrefix {
    type Error = &'static str;

    fn try_from(value: &str) -> Result<Self, Self::Error> {
        let c = value.chars().nth(0);
        SizePrefix::try_from(c)
    }
}

/// Container for structures that have variables.
///
/// Several different structures own variables and implementing this trait
/// allows a common handling of those items.
pub trait HasVariables {
    fn variables(&self) -> &Vec<VarDecl>;
}

#[derive(Clone, Debug, PartialEq, Recurse)]
pub struct ProgramAccessDecl {
    pub access_name: Id,
    pub symbolic_variable: SymbolicVariableKind,
    pub type_name: TypeName,
    #[recurse(ignore)]
    pub direction: Option<Direction>,
}

/// Identifies which source `VAR`/`VAR_INPUT`/`VAR_OUTPUT` block a
/// [`VarDecl`] was declared in. Two declarations sharing the same `BlockId`
/// came from the same block in the source text -- this is the only way to
/// tell once declarations from different blocks are flattened together into
/// one `Vec<VarDecl>` (e.g. `FunctionBlockDeclaration::variables`).
///
/// Declarations built by anything other than the parser's per-block grouping
/// logic (helper constructors, tests, other transform passes) each get their
/// own fresh id via [`next_block_id`], so they can never be mistaken for
/// sharing a block with an unrelated declaration.
pub type BlockId = usize;

/// Returns a fresh [`BlockId`], globally unique for the process's lifetime.
pub fn next_block_id() -> BlockId {
    use std::sync::atomic::{AtomicUsize, Ordering};
    static NEXT: AtomicUsize = AtomicUsize::new(0);
    NEXT.fetch_add(1, Ordering::Relaxed)
}

/// Variable declaration.
///
/// See section 2.4.3.
#[derive(Clone, Debug, Recurse, Located, Attributed)]
pub struct VarDecl {
    // Not all variable types have a "name", so the name is part of the type.
    #[located(delegate)]
    pub identifier: VariableIdentifier,
    #[recurse(ignore)]
    pub var_type: VariableType,
    #[recurse(ignore)]
    pub qualifier: DeclarationQualifier,
    pub initializer: InitialValueAssignmentKind,
    /// Which source block this declaration came from -- see [`BlockId`].
    /// Deliberately excluded from `PartialEq` (see the manual impl below):
    /// it is assigned from a process-global counter, so two declarations
    /// that are otherwise identical (e.g. an expected AST built by hand in
    /// a test, or the same source parsed twice) would only match by
    /// coincidence if this field were compared.
    #[recurse(ignore)]
    pub block: BlockId,
    /// The type the declaration declares, by identity. Populated by the
    /// analyzer; `None` before it runs and for a declaration whose type it
    /// could not resolve. Left out of `PartialEq` like `block`: it is
    /// derived from `initializer`.
    #[recurse(ignore)]
    pub type_id: Option<TypeId>,
    /// The edge an input declaration names (`x : BOOL R_EDGE`): the
    /// variable reads `TRUE` only in the cycle its actual input changes in
    /// that direction (IEC 61131-3 section 2.4.3). `None` for every other
    /// declaration. An edge input is otherwise an ordinary `BOOL` input, so
    /// every consumer of the variable list sees it.
    #[recurse(ignore)]
    pub edge: Option<EdgeDirection>,
    #[recurse(ignore)]
    pub attributes: Attributes,
}

impl PartialEq for VarDecl {
    fn eq(&self, other: &Self) -> bool {
        self.identifier == other.identifier
            && self.var_type == other.var_type
            && self.qualifier == other.qualifier
            && self.initializer == other.initializer
            && self.edge == other.edge
            && self.attributes == other.attributes
    }
}

impl VarDecl {
    /// Creates a variable declaration for simple type and no initialization.
    /// The declaration has type `VAR` and no qualifier.
    pub fn simple(name: &str, type_name: &str) -> Self {
        Self {
            identifier: VariableIdentifier::new_symbol(name),
            var_type: VariableType::Var,
            qualifier: DeclarationQualifier::Unspecified,
            initializer: InitialValueAssignmentKind::simple_uninitialized(TypeName::from(
                type_name,
            )),
            block: next_block_id(),
            type_id: None,
            edge: None,
            attributes: Attributes::default(),
        }
    }

    pub fn string(name: &str, var_type: VariableType, qualifier: DeclarationQualifier) -> Self {
        Self {
            identifier: VariableIdentifier::new_symbol(name),
            var_type,
            qualifier,
            initializer: InitialValueAssignmentKind::String(StringInitializer {
                length: None,
                width: StringType::String,
                initial_value: None,
                keyword_span: SourceSpan::default(),
            }),
            block: next_block_id(),
            type_id: None,
            edge: None,
            attributes: Attributes::default(),
        }
    }

    /// Creates a variable declaration for enumeration without having an initial value.
    /// The declaration has type `VAR` and no qualifier.
    pub fn uninitialized_enumerated(name: &str, type_name: &str) -> Self {
        VarDecl {
            identifier: VariableIdentifier::new_symbol(name),
            var_type: VariableType::Var,
            qualifier: DeclarationQualifier::Unspecified,
            initializer: InitialValueAssignmentKind::EnumeratedType(
                EnumeratedInitialValueAssignment {
                    type_name: TypeName::from(type_name),
                    initial_value: None,
                },
            ),
            block: next_block_id(),
            type_id: None,
            edge: None,
            attributes: Attributes::default(),
        }
    }

    /// Creates a variable declaration against a user type name with a bare
    /// identifier as its initial value, `name : type_name := initial_value`,
    /// as the parser leaves it before the type is known (ADR-0050).
    /// The declaration has type `VAR` and no qualifier.
    pub fn late_bound_value(name: &str, type_name: &str, initial_value: &str) -> Self {
        VarDecl {
            identifier: VariableIdentifier::new_symbol(name),
            var_type: VariableType::Var,
            qualifier: DeclarationQualifier::Unspecified,
            initializer: InitialValueAssignmentKind::LateResolvedType(LateResolvedInitializer {
                type_name: TypeName::from(type_name),
                initial_value: Some(LateResolvedInitialValue::Value(Id::from(initial_value))),
            }),
            block: next_block_id(),
            type_id: None,
            edge: None,
            attributes: Attributes::default(),
        }
    }

    /// Creates a variable declaration for enumeration having an initial value.
    /// The declaration has type `VAR` and no qualifier.
    pub fn enumerated(name: &str, type_name: &str, initial_value: &str) -> Self {
        VarDecl {
            identifier: VariableIdentifier::new_symbol(name),
            var_type: VariableType::Var,
            qualifier: DeclarationQualifier::Unspecified,
            initializer: InitialValueAssignmentKind::EnumeratedType(
                EnumeratedInitialValueAssignment {
                    type_name: TypeName::from(type_name),
                    initial_value: Some(EnumeratedValue {
                        type_name: None,
                        value: Id::from(initial_value),
                        explicit_value: None,
                    }),
                },
            ),
            block: next_block_id(),
            type_id: None,
            edge: None,
            attributes: Attributes::default(),
        }
    }

    /// Creates a variable declaration for a function block.
    /// The declaration has type `VAR` and no qualifier.
    pub fn function_block(name: &str, type_name: &str) -> Self {
        VarDecl {
            identifier: VariableIdentifier::new_symbol(name),
            var_type: VariableType::Var,
            qualifier: DeclarationQualifier::Unspecified,
            initializer: InitialValueAssignmentKind::FunctionBlock(
                FunctionBlockInitialValueAssignment {
                    type_name: TypeName::from(type_name),
                    init: vec![],
                },
            ),
            block: next_block_id(),
            type_id: None,
            edge: None,
            attributes: Attributes::default(),
        }
    }

    /// Creates a variable declaration for a structure.
    pub fn structure(name: &str, type_name: &str) -> Self {
        VarDecl {
            identifier: VariableIdentifier::new_symbol(name),
            var_type: VariableType::Var,
            qualifier: DeclarationQualifier::Unspecified,
            initializer: InitialValueAssignmentKind::Structure(
                StructureInitializationDeclaration {
                    type_name: TypeName::from(type_name),
                    elements_init: vec![],
                },
            ),
            block: next_block_id(),
            type_id: None,
            edge: None,
            attributes: Attributes::default(),
        }
    }

    /// Creates a variable declaration that is ambiguous on the type.
    /// The declaration has type `VAR` and no qualifier.
    ///
    /// The language has some ambiguity for types. The late bound represents
    /// a placeholder that is later resolved once all types are known.
    pub fn late_bound(name: &str, type_name: &str) -> Self {
        VarDecl {
            identifier: VariableIdentifier::new_symbol(name),
            var_type: VariableType::Var,
            qualifier: DeclarationQualifier::Unspecified,
            initializer: InitialValueAssignmentKind::LateResolvedType(
                LateResolvedInitializer::bare(TypeName::from(type_name)),
            ),
            block: next_block_id(),
            type_id: None,
            edge: None,
            attributes: Attributes::default(),
        }
    }

    /// Assigns the type of the variable declaration.
    pub fn with_type(mut self, var_type: VariableType) -> Self {
        self.var_type = var_type;
        self
    }

    /// Assigns the qualifier of the variable declaration.
    pub fn with_qualifier(mut self, qualifier: DeclarationQualifier) -> Self {
        self.qualifier = qualifier;
        self
    }

    pub fn type_name(&self) -> TypeReference {
        self.initializer.type_reference()
    }
}

/// Declarations that are `AT`-located but share a `BlockId` with at least
/// one plain (symbolic) declaration -- derived from block identity, not
/// stored. This is the extension gated by
/// `--allow-mixed-located-var-declarations`: standard IEC 61131-3 requires
/// located variables to live in their own dedicated block, separate from
/// ordinary symbolic ones.
///
/// `variables` is typically a whole POU's flattened declaration list (e.g.
/// `FunctionBlockDeclaration::variables`), which may span several source
/// blocks -- grouping by [`BlockId`] recovers which declarations came from
/// the same block despite the flattening.
pub fn mixed_located_var_decls(variables: &[VarDecl]) -> impl Iterator<Item = &VarDecl> {
    let mixed_blocks: std::collections::HashSet<BlockId> = variables
        .iter()
        .fold(
            std::collections::HashMap::<BlockId, (bool, bool)>::new(),
            |mut acc, v| {
                let (has_symbol, has_direct) = acc.entry(v.block).or_default();
                match &v.identifier {
                    VariableIdentifier::Symbol(_) => *has_symbol = true,
                    VariableIdentifier::Direct(_) => *has_direct = true,
                }
                acc
            },
        )
        .into_iter()
        .filter_map(|(block, (has_symbol, has_direct))| (has_symbol && has_direct).then_some(block))
        .collect();

    variables
        .iter()
        .filter(move |v| matches!(v.identifier, VariableIdentifier::Direct(_)))
        .filter(move |v| mixed_blocks.contains(&v.block))
}

/// Keywords for declarations.
///
/// IEC 61131-3 defines groups that can contain multiple variables. These
/// groups introduce complexity in parsing and in iterating. This
/// implementation treats the groups as labels on individual variables; in
/// effect, there are no groups.
///
/// The variable type owns the name of a variable because the type
/// defines whether a name is required.
///
/// See section 2.4.3.
#[derive(Clone, Debug, PartialEq)]
pub enum VariableType {
    /// Local to a POU.
    Var,
    /// Local to a POU. Does not need to be maintained
    /// between calls to a POU.
    VarTemp,
    /// `VAR_STAT` (CODESYS/TwinCAT, Siemens SCL): a variable that keeps
    /// its value between calls of the POU.
    ///
    /// Recognized and represented, but stored like [`Var`](Self::Var): the
    /// "initialize once, then persist" placement is not implemented yet
    /// (P1 in `Codesys/LEXER-GAP-ANALYSIS.md`).
    VarStat,
    /// `VAR_INST` (CODESYS/TwinCAT): a variable of a method that belongs to
    /// the method's instance rather than to a single call.
    ///
    /// Stored like [`Var`](Self::Var); the instance placement is not
    /// implemented yet.
    VarInst,
    /// `VAR_GENERIC` (CODESYS/TwinCAT): the generic constants of a function
    /// block, declared directly after its name.
    ///
    /// Stored like [`Var`](Self::Var); compile-time substitution of the
    /// generic constants is not implemented yet.
    VarGeneric,
    /// Variable that is visible to a calling POU as an input.
    Input,
    /// Variable that is visible to calling POU and can only
    /// be ready from the calling POU. It can be written to
    /// by the POU that defines the variable.
    Output,
    /// Variable that is visible to calling POU and is readable
    /// writeable by the calling POU.
    InOut,
    /// Enables a POU to read and (possibly) write to a global
    /// variable.
    External,
    /// A variable that may be read and written by multiple
    /// POUs that also declare the variable as external.
    Global,
    /// Configurations for communication channels.
    Access,
}

impl VariableType {
    /// The keyword that opens the block of this kind of variable.
    pub fn keyword(&self) -> &'static str {
        match self {
            VariableType::Var => "VAR",
            VariableType::VarTemp => "VAR_TEMP",
            VariableType::VarStat => "VAR_STAT",
            VariableType::VarInst => "VAR_INST",
            VariableType::VarGeneric => "VAR_GENERIC",
            VariableType::Input => "VAR_INPUT",
            VariableType::Output => "VAR_OUTPUT",
            VariableType::InOut => "VAR_IN_OUT",
            VariableType::External => "VAR_EXTERNAL",
            VariableType::Global => "VAR_GLOBAL",
            VariableType::Access => "VAR_ACCESS",
        }
    }

    /// Returns true if this variable type receives a value from the caller
    /// (i.e., VAR_INPUT or VAR_IN_OUT). These are the parameter kinds that
    /// appear as arguments in a function/FB call.
    pub fn is_input_compatible(&self) -> bool {
        matches!(self, VariableType::Input | VariableType::InOut)
    }

    /// Returns true if this is a local variable (VAR, VAR_TEMP and the
    /// additional local sections VAR_STAT/VAR_INST/VAR_GENERIC).
    pub fn is_local(&self) -> bool {
        matches!(
            self,
            VariableType::Var
                | VariableType::VarTemp
                | VariableType::VarStat
                | VariableType::VarInst
                | VariableType::VarGeneric
        )
    }

    /// Returns true for the sections that hold POU instance storage: the
    /// declarations a function block keeps in its instance data. This is
    /// `VAR` plus the additional sections, which are stored the same way
    /// until their placement rules are implemented (see each variant's
    /// documentation). `VAR_TEMP` is excluded: it is call-scoped.
    pub fn is_pou_storage(&self) -> bool {
        matches!(
            self,
            VariableType::Var
                | VariableType::VarStat
                | VariableType::VarInst
                | VariableType::VarGeneric
        )
    }

    /// Returns true for the section whose variables belong to one call of the
    /// POU: they start at their declared initial value (or the default of
    /// their type) at every call and nothing keeps their value from one call to
    /// the next. This is `VAR_TEMP`, in every POU that declares it. A program's
    /// `VAR_TEMP` is therefore not state of the program: it is not persistent,
    /// and it is initialised at the start of every scan.
    pub fn is_call_scoped(&self) -> bool {
        matches!(self, VariableType::VarTemp)
    }

    /// Returns true if this is any kind of parameter visible to a caller
    /// (VAR_INPUT, VAR_OUTPUT, or VAR_IN_OUT).
    pub fn is_parameter(&self) -> bool {
        matches!(
            self,
            VariableType::Input | VariableType::Output | VariableType::InOut
        )
    }
}

/// Ways of identifying variable data objects. These are used
/// in declarations (as opposed to in statements), hence this
/// does not have multi-element information (arrays and structures).
///
/// See section 2.4.1.1.
#[derive(Clone, Debug, PartialEq, Recurse)]
pub enum VariableIdentifier {
    /// A variable data object that is referenced by a symbol. This is
    /// typical reference type common in most programming languages.
    Symbol(Id),

    /// A variable data object that is "directly" mapped to a hardware
    /// address. These variables are typically used for I/O.
    ///
    /// Directly represented variables have an address and an optional
    /// symbolic identifier.
    Direct(DirectVariableIdentifier),
}

impl VariableIdentifier {
    /// Create a new symbolic variable identifier.
    pub fn new_symbol(name: &str) -> Self {
        VariableIdentifier::Symbol(Id::from(name))
    }

    /// Create a new direct variable identifier.
    pub fn new_direct(name: Option<Id>, location: AddressAssignment) -> Self {
        VariableIdentifier::Direct(DirectVariableIdentifier {
            name,
            address_assignment: location,
        })
    }

    /// Return the symbolic identifier if there is one.
    ///
    /// Direct representations have an optional symbolic identifier.
    pub fn symbolic_id(&self) -> Option<&Id> {
        match self {
            VariableIdentifier::Symbol(name) => Option::Some(name),
            VariableIdentifier::Direct(direct) => match &direct.name {
                Some(n) => Some(n),
                None => None,
            },
        }
    }
}

impl Located for VariableIdentifier {
    fn span(&self) -> SourceSpan {
        match self {
            VariableIdentifier::Symbol(name) => name.span(),
            VariableIdentifier::Direct(direct) => direct.span(),
        }
    }
}

impl Display for VariableIdentifier {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            VariableIdentifier::Symbol(name) => name.fmt(f),
            VariableIdentifier::Direct(direct) => {
                if let Some(n) = &direct.name {
                    n.fmt(f)
                } else {
                    direct.address_assignment.fmt(f)
                }
            }
        }
    }
}

#[derive(Clone, Debug, PartialEq, Recurse)]
pub struct DirectVariableIdentifier {
    pub name: Option<Id>,
    pub address_assignment: AddressAssignment,
}

impl Located for DirectVariableIdentifier {
    /// The span of the symbolic name, or of the address when the declaration
    /// has no name (`VAR AT %IX0.0 : BOOL; END_VAR`).
    ///
    /// Deriving this from the parts rather than storing a span of its own is
    /// deliberate: a stored span is one every construction site has to
    /// remember to fill in, and none of them did -- every located variable
    /// reported position 0, so `P4036` put its caret on the first character
    /// of the file instead of on the variable.
    fn span(&self) -> SourceSpan {
        match &self.name {
            Some(name) => name.span(),
            None => self.address_assignment.position.clone(),
        }
    }
}

/// Qualifier types for definitions.
///
/// IEC 61131-3 defines groups that share common qualifiers. These
/// groups introduce complexity in parsing and in iterating. This
/// implementation treats the groups as labels on individual variables; in
/// effect, there are no groups.
///
/// See section 2.4.3.
#[derive(Clone, Debug, PartialEq)]
pub enum DeclarationQualifier {
    // TODO Some of these are not valid for some contexts - should there be multiple
    // qualifier classes, indicate some how, or fail?
    Unspecified,
    Constant,
    /// Stored so that the value is retained through power loss.
    Retain,
    /// Stored so that the value is NOT retained through power loss.
    NonRetain,
    /// Stored so that the value survives a download, reset, or power loss
    /// (Beckhoff TwinCAT/CODESYS extension, not part of the IEC standard).
    Persistent,
}

/// Location assignment for a variable.
///
/// See section 2.4.3.1.
#[derive(Clone, PartialEq, Recurse)]
pub struct AddressAssignment {
    #[recurse(ignore)]
    pub location: LocationPrefix,
    #[recurse(ignore)]
    pub size: SizePrefix,
    #[recurse(ignore)]
    pub address: Vec<u32>,
    pub position: SourceSpan,
}

impl AddressAssignment {
    /// Returns this address with `position` as its source position.
    ///
    /// [`AddressAssignment::try_from`] parses the address out of the token
    /// text alone, which carries no position, so the caller puts the token's
    /// span back.
    pub fn with_position(mut self, position: SourceSpan) -> Self {
        self.position = position;
        self
    }
}

#[expect(
    clippy::unwrap_used,
    reason = "static regex literals are compile-time-validated patterns; regex has no const constructor"
)]
fn direct_address_unassigned_re() -> &'static Regex {
    static RE: std::sync::LazyLock<Regex> =
        std::sync::LazyLock::new(|| Regex::new(r"%([IQM])\*").unwrap());
    &RE
}

#[expect(
    clippy::unwrap_used,
    reason = "static regex literals are compile-time-validated patterns; regex has no const constructor"
)]
fn direct_address_re() -> &'static Regex {
    static RE: std::sync::LazyLock<Regex> =
        std::sync::LazyLock::new(|| Regex::new(r"%([IQM])([XBWDL])?(\d+(\.\d+)*)").unwrap());
    &RE
}

impl TryFrom<&str> for AddressAssignment {
    type Error = &'static str;

    fn try_from(value: &str) -> Result<Self, Self::Error> {
        if let Some(cap) = direct_address_unassigned_re().captures(value) {
            let location_prefix = LocationPrefix::try_from(&cap[1])?;
            return Ok(AddressAssignment {
                location: location_prefix,
                size: SizePrefix::Unspecified,
                address: vec![],
                position: SourceSpan::default(),
            });
        }

        if let Some(cap) = direct_address_re().captures(value) {
            let location_prefix = LocationPrefix::try_from(&cap[1])?;
            let size_prefix =
                SizePrefix::try_from(cap.get(2).and_then(|size| size.as_str().chars().next()))?;
            let pos: Vec<u32> = cap[3]
                .split('.')
                .map(|v| v.parse::<u32>())
                .collect::<Result<_, _>>()
                .map_err(|_| "address component is not a number")?;

            return Ok(AddressAssignment {
                location: location_prefix,
                size: size_prefix,
                address: pos,
                position: SourceSpan::default(),
            });
        }

        Err("Value not convertible to direct variable")
    }
}

impl fmt::Debug for AddressAssignment {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        f.debug_struct("AddressAssignment")
            .field("location", &self.location)
            .field("size", &self.size)
            .finish()
    }
}

impl fmt::Display for AddressAssignment {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        f.debug_struct("AddressAssignment")
            .field("location", &self.location)
            .field("size", &self.size)
            .finish()
    }
}

// Container for variable specifications.

#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum EdgeDirection {
    Rising,
    Falling,
}
