//! Intermediate representation of types in the IronPLC compiler.
//!
//! This module defines the IntermediateType enum and related structures that represent
//! the type system during the compilation process. These types are used after parsing
//! but before code generation to perform type checking and semantic analysis.
//!
//! The intermediate type system is designed to support both primitive types (like integers,
//! booleans) and complex types (like structures, arrays, and function blocks) found in
//! IEC 61131-3 standard and similar PLC programming languages.

use ironplc_container::{string_region_size, CharWidth, DEFAULT_STRING_MAX_LENGTH};
use ironplc_dsl::common::{StructInitialValueAssignmentKind, VariableType};
use ironplc_dsl::core::Id;

use crate::intermediates::layout::{align_up, layout, AlignmentBound, Layout};

/// Bounds for a single dimension of an array type.
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct ArrayDimension {
    /// Lower bound (inclusive), e.g., 1 in ARRAY[1..10]
    pub lower: i32,
    /// Upper bound (inclusive), e.g., 10 in ARRAY[1..10]
    pub upper: i32,
}

/// Represents the size of a data type in bits, aligned to common byte boundaries.
/// Used for memory layout and type checking of numeric and bit-based types.
#[derive(Debug, Clone, PartialEq)]
pub enum ByteSized {
    /// 8-bit (1 byte) size
    B8,
    /// 16-bit (2 bytes) size
    B16,
    /// 32-bit (4 bytes) size
    B32,
    /// 64-bit (8 bytes) size
    B64,
}

impl ByteSized {
    /// Converts the ByteSized variant to its corresponding bit width
    pub fn into(&self) -> u8 {
        match self {
            ByteSized::B8 => 8,
            ByteSized::B16 => 16,
            ByteSized::B32 => 32,
            ByteSized::B64 => 64,
        }
    }

    pub fn as_bytes(&self) -> u8 {
        match self {
            ByteSized::B8 => 1,
            ByteSized::B16 => 2,
            ByteSized::B32 => 4,
            ByteSized::B64 => 8,
        }
    }
}

/// Represents a type in the intermediate representation of the PLC program.
///
/// This enum captures all possible types that can appear in a PLC program,
/// from primitive types to complex user-defined types. The intermediate
/// representation is used during semantic analysis and code generation.
#[derive(Debug, Clone, PartialEq)]
pub enum IntermediateType {
    /// Boolean type (true/false)
    Bool,
    /// Signed integer with specified bit width
    Int { size: ByteSized },
    /// Unsigned integer with specified bit width
    UInt { size: ByteSized },
    /// Floating-point number with specified precision
    Real {
        size: ByteSized, // Typically B32 (single precision) or B64 (double precision)
    },
    /// Fixed-size byte array
    Bytes { size: ByteSized },
    /// Time duration type with specified bit width.
    /// TIME is 32-bit (milliseconds), LTIME is 64-bit (milliseconds).
    Time { size: ByteSized },
    /// Calendar date type (DATE is 32-bit, LDATE is 64-bit).
    /// Stored as unsigned seconds since 1970-01-01.
    Date { size: ByteSized },
    /// Time-of-day type (TIME_OF_DAY/TOD is 32-bit, LTOD is 64-bit).
    /// Both store unsigned milliseconds since midnight; LTOD is the wider
    /// variant, not a higher-resolution one (ADR-0025 amendment).
    TimeOfDay { size: ByteSized },
    /// Combined date-and-time type (DATE_AND_TIME/DT is 32-bit, LDT is 64-bit).
    /// Both store unsigned seconds since 1970-01-01; LDT is the wider
    /// variant, not a higher-resolution one (ADR-0025 amendment).
    DateAndTime { size: ByteSized },

    /// Variable-length string with an optional maximum length and a
    /// per-code-unit `char_width`: [`CharWidth::Narrow`] for STRING (Latin-1
    /// per ADR-0016), [`CharWidth::Wide`] for WSTRING (UTF-16LE per ADR-0016).
    /// The two encodings are not implicitly convertible (ADR-0034); the
    /// type checker rejects `STRING := WSTRING` and the reverse.
    String {
        max_len: Option<u128>,
        char_width: CharWidth,
    },

    /// User-defined enumeration type
    Enumeration {
        /// The primitive type the values are stored in (usually Int { size: 8 })
        underlying_type: Box<IntermediateType>,
        /// The integer type the values are numbers of: `INT` unless the
        /// declaration names another (`(A, B) BYTE`). It is what an
        /// enumeration is where an integer is expected, so it states which
        /// integer types accept it; the storage above only sizes it.
        base_type: Box<IntermediateType>,
        /// The number of each value the enumeration declares, in
        /// declaration order. A constant may be stored in the enumeration
        /// when it is one of these.
        numbers: Vec<i64>,
        /// Whether the enumeration is strict: no number is computed from its
        /// values or stored into it. A type that is not strict is its base
        /// type wherever a check asks whether the value is a number. Decided
        /// where the enumeration is resolved (see
        /// `intermediates::enumeration`); every check asks
        /// [`IntermediateType::is_strict_enumeration`].
        strict: bool,
    },
    /// Structure type containing named fields
    Structure {
        /// Ordered list of fields in the structure
        fields: Vec<IntermediateStructField>,
        /// The bound the declaration puts on the alignment of its fields
        /// (`pack_mode`). The offsets of the fields, the alignment and the
        /// size of the structure derive from it.
        bound: AlignmentBound,
    },
    /// Array type with element type and per-dimension bounds.
    Array {
        /// Type of elements in the array
        element_type: Box<IntermediateType>,
        /// Per-dimension bounds. Empty for dynamic/unknown-size arrays.
        dimensions: Vec<ArrayDimension>,
    },
    /// Subrange type with base type and bounds
    Subrange {
        /// The base type this subrange is derived from (must be integer type)
        base_type: Box<IntermediateType>,
        /// Minimum value (inclusive)
        min_value: i128,
        /// Maximum value (inclusive)
        max_value: i128,
    },
    /// Function block type
    FunctionBlock {
        /// Name of the function block type
        name: String,
        /// Ordered list of fields (variables) in the function block instance
        fields: Vec<IntermediateStructField>,
    },
    /// Function type with return type and parameters
    Function {
        /// Return type of the function, None for procedures
        return_type: Option<Box<IntermediateType>>,
        /// List of function parameters
        parameters: Vec<IntermediateFunctionParameter>,
    },
    /// Reference type (REF_TO) pointing to a target type.
    /// References are stored as variable-table indices (u64) at runtime.
    Reference {
        /// The type that this reference points to
        target_type: Box<IntermediateType>,
    },
}

impl IntermediateType {
    /// Returns if the type is a primitive type.
    pub fn is_primitive(&self) -> bool {
        matches!(
            self,
            IntermediateType::Bool
                | IntermediateType::Int { .. }
                | IntermediateType::UInt { .. }
                | IntermediateType::Real { .. }
                | IntermediateType::Bytes { .. }
                | IntermediateType::String { .. }
                | IntermediateType::Time { .. }
                | IntermediateType::Date { .. }
                | IntermediateType::TimeOfDay { .. }
                | IntermediateType::DateAndTime { .. }
        )
    }

    /// The integer type a value of this type is where an integer is expected:
    /// the base type of an enumeration. `None` for a type that is not
    /// accepted for an integer.
    pub fn number_type(&self) -> Option<&IntermediateType> {
        match self {
            IntermediateType::Enumeration { base_type, .. } => Some(base_type),
            _ => None,
        }
    }

    /// Returns if the type is an enumeration.
    pub fn is_enumeration(&self) -> bool {
        matches!(self, IntermediateType::Enumeration { .. })
    }

    /// Returns if the type is an enumeration that is no number: the one
    /// question every check that treats an enumeration differently from its
    /// base type asks (`value_type`, `value_range`, the operator and `FOR`
    /// checks). A type that is not a strict enumeration is a number, or the
    /// base type of one, wherever the check asks.
    pub fn is_strict_enumeration(&self) -> bool {
        matches!(self, IntermediateType::Enumeration { strict: true, .. })
    }

    /// The integer type that holds a value of this type where the virtual
    /// machine stores one in a member of a structure or an element: for a
    /// strict enumeration its underlying type, which holds every value it
    /// declares and no other, and for an enumeration that is not strict its
    /// base type, which holds every number the base type holds. Any other
    /// type is held as itself.
    pub fn stored_as(&self) -> &IntermediateType {
        match self {
            IntermediateType::Enumeration {
                underlying_type, ..
            } if self.is_strict_enumeration() => underlying_type,
            IntermediateType::Enumeration { base_type, .. } => base_type,
            other => other,
        }
    }

    /// Returns if the type is a structure.
    pub fn is_structure(&self) -> bool {
        matches!(self, IntermediateType::Structure { .. })
    }

    /// Returns if the type is an array.
    pub fn is_array(&self) -> bool {
        matches!(self, IntermediateType::Array { .. })
    }

    /// Returns if the type is a subrange.
    pub fn is_subrange(&self) -> bool {
        matches!(self, IntermediateType::Subrange { .. })
    }

    /// Returns if the type is a function block.
    pub fn is_function_block(&self) -> bool {
        matches!(self, IntermediateType::FunctionBlock { .. })
    }

    /// Returns if the type is a function.
    pub fn is_function(&self) -> bool {
        matches!(self, IntermediateType::Function { .. })
    }

    /// Returns if the type exposes named members reachable with `.` access.
    ///
    /// Structures and function block instances both expose named fields, so
    /// both answer `true`.
    pub fn has_members(&self) -> bool {
        self.member_fields().is_some()
    }

    /// Returns the named fields reachable with `.` access, None if the type
    /// has no such members.
    ///
    /// A function block instance exposes its variables (`timer.Q`) the same way
    /// a structure exposes its fields (`setup.FLAG`), so both are returned
    /// here. Use [`IntermediateType::is_structure`] instead when the caller
    /// needs a structure specifically.
    pub fn member_fields(&self) -> Option<&Vec<IntermediateStructField>> {
        match self {
            IntermediateType::Structure { fields, .. }
            | IntermediateType::FunctionBlock { fields, .. } => Some(fields),
            _ => None,
        }
    }

    /// Returns if the type is a reference (REF_TO).
    pub fn is_reference(&self) -> bool {
        matches!(self, IntermediateType::Reference { .. })
    }

    /// Returns the target type if this is a reference type, None otherwise.
    pub fn referenced_type(&self) -> Option<&IntermediateType> {
        match self {
            IntermediateType::Reference { target_type } => Some(target_type),
            _ => None,
        }
    }

    /// Returns if the type is numeric (integer, unsigned integer, or real).
    pub fn is_numeric(&self) -> bool {
        matches!(
            self,
            IntermediateType::Int { .. }
                | IntermediateType::UInt { .. }
                | IntermediateType::Real { .. }
        )
    }

    /// Returns the total number of elements across all dimensions,
    /// or None if dimensions is empty.
    pub fn array_total_elements(&self) -> Option<u32> {
        match self {
            IntermediateType::Array { dimensions, .. } if !dimensions.is_empty() => {
                let mut total: u32 = 1;
                for dim in dimensions {
                    // Guard: if lower > upper, this dimension is invalid.
                    // The analyzer validates this, but defense-in-depth.
                    let span = dim.upper as i64 - dim.lower as i64 + 1;
                    if span <= 0 {
                        return None;
                    }
                    let dim_size = span as u32;
                    total = total.checked_mul(dim_size)?;
                }
                Some(total)
            }
            _ => None,
        }
    }

    /// Returns if the type is an integer type (signed or unsigned).
    pub fn is_integer(&self) -> bool {
        matches!(
            self,
            IntermediateType::Int { .. } | IntermediateType::UInt { .. }
        )
    }

    /// Gets the size in bytes for this type.
    ///
    /// Returns the exact size in bytes for fixed-size types. For types that require
    /// complex calculations or are dynamically sized, returns `None`.
    ///
    /// Note: IEC 61131-3 doesn't support truly dynamically sized objects at runtime,
    /// but some types require compile-time analysis to determine their final size.
    pub fn size_in_bytes(&self) -> Option<u32> {
        match self {
            IntermediateType::Bool => Some(1),
            IntermediateType::Int { size } | IntermediateType::UInt { size } => {
                Some(size.as_bytes() as u32)
            }
            IntermediateType::Real { size } => Some(size.as_bytes() as u32),
            IntermediateType::Bytes { size } => Some(size.as_bytes() as u32),
            IntermediateType::Time { size } => Some(size.as_bytes() as u32),
            IntermediateType::Date { size } => Some(size.as_bytes() as u32),
            IntermediateType::TimeOfDay { size } => Some(size.as_bytes() as u32),
            IntermediateType::DateAndTime { size } => Some(size.as_bytes() as u32),
            IntermediateType::String {
                max_len,
                char_width,
            } => max_len.map(|len| len as u32 * char_width.byte_width() as u32),
            IntermediateType::Subrange { base_type, .. } => base_type.size_in_bytes(),
            IntermediateType::Enumeration {
                underlying_type, ..
            } => underlying_type.size_in_bytes(),
            IntermediateType::Structure { fields, bound } => {
                member_layout(fields, *bound).byte_image_size()
            }
            IntermediateType::Array { element_type, .. } => {
                let total_elements = self.array_total_elements()?;
                // The elements repeat, so each occupies its stride.
                let elem_stride = element_type.stride_bytes()?;
                elem_stride.checked_mul(total_elements)
            }
            IntermediateType::FunctionBlock { fields, .. } => {
                // Function blocks follow the same memory layout rules as
                // structures, with no bound on the alignment.
                member_layout(fields, AlignmentBound::Natural).byte_image_size()
            }
            IntermediateType::Function { .. } => {
                // Functions don't have memory layout in the traditional sense
                None
            }
            IntermediateType::Reference { .. } => {
                // References are stored as 64-bit variable-table indices
                Some(8)
            }
        }
    }

    /// The number of bytes a value of this type occupies where it repeats (the
    /// element of an array) or nests (a member of a structure): its size
    /// padded up to its alignment.
    ///
    /// For a type that is not a structure or a function block this is the
    /// size. Returns `None` where `size_in_bytes` does.
    pub fn stride_bytes(&self) -> Option<u32> {
        self.size_in_bytes()
            .map(|size| align_up(size, self.alignment_bytes()))
    }

    /// Gets the alignment requirement in bytes for this type.
    ///
    /// Returns the memory alignment requirement following typical C-style alignment rules.
    /// For types that require complex calculations, returns a conservative default.
    ///
    /// # Alignment Rules
    /// - 8-bit types (BOOL, SINT, USINT, BYTE): 1-byte alignment
    /// - 16-bit types (INT, UINT, WORD): 2-byte alignment  
    /// - 32-bit types (DINT, UDINT, REAL, DWORD): 4-byte alignment
    /// - 64-bit types (LINT, ULINT, LREAL, LWORD, LTIME, LDATE, DT, LDT): 8-byte alignment
    pub fn alignment_bytes(&self) -> u8 {
        match self {
            IntermediateType::Bool => 1,
            IntermediateType::Int { size } | IntermediateType::UInt { size } => size.as_bytes(),
            IntermediateType::Real { size } => size.as_bytes(),
            IntermediateType::Bytes { size } => size.as_bytes(),
            IntermediateType::Time { size } => size.as_bytes(),
            IntermediateType::Date { size } => size.as_bytes(),
            IntermediateType::TimeOfDay { size } => size.as_bytes(),
            IntermediateType::DateAndTime { size } => size.as_bytes(),
            IntermediateType::String { .. } => 1, // Strings are byte-aligned
            IntermediateType::Subrange { base_type, .. } => base_type.alignment_bytes(),
            IntermediateType::Enumeration {
                underlying_type, ..
            } => underlying_type.alignment_bytes(),
            IntermediateType::Structure { fields, bound } => {
                member_layout(fields, *bound).alignment
            }
            IntermediateType::Array { element_type, .. } => element_type.alignment_bytes(),
            IntermediateType::FunctionBlock { fields, .. } => {
                member_layout(fields, AlignmentBound::Natural).alignment
            }
            IntermediateType::Function { .. } => 1, // Default alignment (functions don't have memory layout)
            IntermediateType::Reference { .. } => 8, // References are 64-bit variable-table indices
        }
    }

    /// Returns whether this type has an explicitly specified size.
    ///
    /// This method determines whether the type's size is explicitly specified
    /// in the type declaration or needs to be inferred from context or defaults.
    /// All IEC 61131-3 types have known, fixed sizes, but some require size
    /// inference during semantic analysis.
    ///
    /// # Return Values
    /// - `true`: Type size is explicitly specified in the declaration
    /// - `false`: Type size needs to be inferred from context or defaults
    pub fn has_explicit_size(&self) -> bool {
        match self {
            IntermediateType::Bool
            | IntermediateType::Int { .. }
            | IntermediateType::UInt { .. }
            | IntermediateType::Real { .. }
            | IntermediateType::Bytes { .. }
            | IntermediateType::Time { .. }
            | IntermediateType::Date { .. }
            | IntermediateType::TimeOfDay { .. }
            | IntermediateType::DateAndTime { .. } => true,
            IntermediateType::String { max_len, .. } => max_len.is_some(),
            IntermediateType::Subrange { base_type, .. } => base_type.has_explicit_size(),
            IntermediateType::Enumeration {
                underlying_type, ..
            } => underlying_type.has_explicit_size(),
            IntermediateType::Structure { .. } => true, // Structures always have explicit size in IEC 61131-3
            IntermediateType::Array {
                element_type,
                dimensions,
            } => {
                // Array has explicit size if it has known dimensions and elements have explicit size
                !dimensions.is_empty() && element_type.has_explicit_size()
            }
            IntermediateType::FunctionBlock { .. } => true, // Function block instances have explicit size
            IntermediateType::Function { .. } => true, // Functions have explicit size (no variable size)
            IntermediateType::Reference { .. } => true, // References are always 8 bytes
        }
    }

    /// Gets the memory offset of a field within a structure type.
    ///
    /// This method is used to calculate the byte offset of a specific field
    /// within a structure, taking into account field alignment and padding
    /// requirements. It's essential for generating correct memory access code.
    ///
    /// # Parameters
    /// - `field_name`: The Id of the field to find (case-insensitive comparison)
    ///
    /// # Returns
    /// - `Some(offset)`: The byte offset of the field from the start of the structure
    /// - `None`: If the field is not found or this is not a structure type
    ///
    /// # Examples
    /// ```ignore
    /// // For a structure with fields: x: INT (offset 0), y: DINT (offset 4)
    /// let fields = vec![
    ///     IntermediateStructField {
    ///         name: Id::from("x"),
    ///         field_type: IntermediateType::Bool,
    ///         offset: 0,
    ///     }
    /// ];
    /// let struct_type = IntermediateType::Structure { fields, bound: AlignmentBound::Natural };
    /// let field_id = Id::from("x");
    /// assert_eq!(struct_type.get_field_offset(&field_id), Some(0));
    /// let unknown_id = Id::from("unknown");
    /// assert_eq!(struct_type.get_field_offset(&unknown_id), None);
    /// ```
    ///
    /// # Note
    /// Uses case-insensitive comparison following IEC 61131-3 identifier rules.
    /// Currently returns the pre-calculated offset stored in the field definition.
    /// Future enhancements may include dynamic offset calculation with proper
    /// alignment and padding rules.
    pub fn get_field_offset(&self, field_name: &ironplc_dsl::core::Id) -> Option<u32> {
        match self {
            IntermediateType::Structure { fields, .. } => {
                // Find the field by name using case-insensitive Id comparison
                fields
                    .iter()
                    .find(|field| field.name == *field_name)
                    .map(|field| field.offset)
            }
            _ => None, // Not a structure type
        }
    }

    /// Validates that subrange bounds are within the limits of this type.
    ///
    /// This method checks if the given minimum and maximum values are valid
    /// for this type when used as a subrange base type. It ensures that:
    /// 1. The type is numeric (integer, unsigned integer, or real)
    /// 2. The bounds are within the representable range of the type
    /// 3. For nested subranges, the bounds are within the parent subrange
    ///
    /// # Parameters
    /// - `min_value`: The minimum value of the subrange (inclusive)
    /// - `max_value`: The maximum value of the subrange (inclusive)
    /// - `type_name`: The name of the type being validated (for error reporting)
    ///
    /// # Returns
    /// - `Ok(())`: If the bounds are valid for this type
    /// - `Err(Diagnostic)`: If the bounds are invalid, with specific error details
    ///
    /// # Errors
    /// - `SubrangeBaseTypeNotNumeric`: If this type is not numeric
    /// - `SubrangeOutOfBounds`: If the bounds exceed the type's representable range
    ///
    /// # Examples
    /// ```ignore
    /// let int_type = IntermediateType::Int { size: ByteSized::B16 };
    /// let type_name = TypeName::from("MY_RANGE");
    ///
    /// // Valid bounds for INT (-32768 to 32767)
    /// assert!(int_type.validate_bounds(-1000, 1000, &type_name).is_ok());
    ///
    /// // Invalid bounds (exceeds INT range)
    /// assert!(int_type.validate_bounds(-50000, 50000, &type_name).is_err());
    /// ```
    pub fn validate_bounds(
        &self,
        min_value: i128,
        max_value: i128,
        type_name: &ironplc_dsl::common::TypeName,
    ) -> Result<(), ironplc_dsl::diagnostic::Diagnostic> {
        use ironplc_dsl::core::Located;
        use ironplc_dsl::diagnostic::Diagnostic;
        use ironplc_problems::text;

        let (type_min, type_max) = match self {
            IntermediateType::Int { size } => match size {
                ByteSized::B8 => (i8::MIN as i128, i8::MAX as i128),
                ByteSized::B16 => (i16::MIN as i128, i16::MAX as i128),
                ByteSized::B32 => (i32::MIN as i128, i32::MAX as i128),
                ByteSized::B64 => (i64::MIN as i128, i64::MAX as i128),
            },
            IntermediateType::UInt { size } => match size {
                ByteSized::B8 => (0, u8::MAX as i128),
                ByteSized::B16 => (0, u16::MAX as i128),
                ByteSized::B32 => (0, u32::MAX as i128),
                ByteSized::B64 => (0, u64::MAX as i128),
            },
            IntermediateType::Subrange {
                min_value: base_min,
                max_value: base_max,
                ..
            } => {
                // For nested subranges, use the parent subrange bounds
                (*base_min, *base_max)
            }
            _ => {
                return Err(Diagnostic::text(
                    type_name.span(),
                    text::subrange_base_type_not_numeric_subrange_base_type(),
                ));
            }
        };

        if min_value < type_min || max_value > type_max {
            return Err(Diagnostic::text(
                type_name.span(),
                text::subrange_out_of_bounds_subrange_declaration(),
            ));
        }

        Ok(())
    }
}

/// The layout of the fields of a structure or a function block under the bound.
fn member_layout(fields: &[IntermediateStructField], bound: AlignmentBound) -> Layout {
    layout(fields.iter().map(|field| &field.field_type), bound)
}

/// Represents the variable type for function block fields.
/// This distinguishes between input, output, in-out, and internal variables.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum FunctionBlockVarType {
    /// Input variable (VAR_INPUT)
    Input,
    /// Output variable (VAR_OUTPUT)
    Output,
    /// Input-output variable (VAR_IN_OUT)
    InOut,
    /// Internal variable (VAR)
    Internal,
}

impl FunctionBlockVarType {
    /// The kind of field a variable block declares in a function block
    /// instance, `None` for a block whose variables the instance does not
    /// hold: `VAR_TEMP` belongs to one call, and `VAR_EXTERNAL`,
    /// `VAR_GLOBAL` and `VAR_ACCESS` name storage that is outside the
    /// instance.
    ///
    /// This is the one definition of which variables are members of an
    /// instance (the type environment builds its fields from it, and so does
    /// the list of what `instance.name` may select). The match has no
    /// wildcard, so a new kind of variable block must say which it is.
    pub fn of(variable_type: &VariableType) -> Option<FunctionBlockVarType> {
        match variable_type {
            VariableType::Input => Some(FunctionBlockVarType::Input),
            VariableType::Output => Some(FunctionBlockVarType::Output),
            VariableType::InOut => Some(FunctionBlockVarType::InOut),
            // VAR_STAT/VAR_INST/VAR_GENERIC are stored like VAR until their
            // placement rules are implemented, so they are fields too. See
            // `VariableType::is_pou_storage`.
            VariableType::Var
            | VariableType::VarStat
            | VariableType::VarInst
            | VariableType::VarGeneric => Some(FunctionBlockVarType::Internal),
            VariableType::VarTemp
            | VariableType::External
            | VariableType::Global
            | VariableType::Access => None,
        }
    }
}

/// Reason why `slot_count` could not compute a slot count.
#[derive(Debug, Clone, PartialEq)]
pub enum SlotCountError {
    /// The type contains a field type not yet supported in the data region
    /// (STRING, WSTRING, or FunctionBlock).
    UnsupportedFieldType,
    /// The type nesting depth exceeds the maximum allowed depth (defense-in-depth
    /// guard against recursive type cycles that the analyzer should have rejected).
    MaxDepthExceeded,
    /// The total slot count overflows u32 (structure or array is too large).
    Overflow,
}

impl IntermediateType {
    /// Returns the number of 8-byte slots this type occupies in the data region.
    ///
    /// Returns `Ok(n)` for types with known slot counts.
    ///
    /// Returns `Err(SlotCountError::UnsupportedFieldType)` for types that cannot
    /// yet be stored in the data region (STRING, WSTRING, FunctionBlock).
    ///
    /// Returns `Err(SlotCountError::MaxDepthExceeded)` if the nesting depth
    /// exceeds 32 levels (defense-in-depth against recursive type cycles that
    /// the analyzer's toposort pass should have rejected).
    ///
    /// Returns `Err(SlotCountError::Overflow)` if the total slot count exceeds u32.
    pub fn slot_count(&self) -> Result<u32, SlotCountError> {
        self.slot_count_inner(0)
    }

    fn slot_count_inner(&self, depth: u32) -> Result<u32, SlotCountError> {
        // Guard against runaway recursion (defense-in-depth).
        // The analyzer rejects recursive types via toposort, but if a bug
        // allows one through, this prevents a stack overflow.
        const MAX_NESTING_DEPTH: u32 = 32;
        if depth > MAX_NESTING_DEPTH {
            return Err(SlotCountError::MaxDepthExceeded);
        }

        match self {
            // Primitives: 1 slot each
            IntermediateType::Bool
            | IntermediateType::Int { .. }
            | IntermediateType::UInt { .. }
            | IntermediateType::Real { .. }
            | IntermediateType::Bytes { .. }
            | IntermediateType::Time { .. }
            | IntermediateType::Date { .. }
            | IntermediateType::TimeOfDay { .. }
            | IntermediateType::DateAndTime { .. }
            | IntermediateType::Enumeration { .. }
            | IntermediateType::Subrange { .. }
            | IntermediateType::Reference { .. } => Ok(1),

            // Structures: sum of field slot counts
            IntermediateType::Structure { fields, .. } => {
                let mut total = 0u32;
                for field in fields {
                    let field_slots = field.field_type.slot_count_inner(depth + 1)?;
                    total = total
                        .checked_add(field_slots)
                        .ok_or(SlotCountError::Overflow)?;
                }
                Ok(total)
            }

            // Arrays: total_elements * element_slots
            IntermediateType::Array {
                element_type,
                dimensions,
            } => {
                let elem_slots = element_type.slot_count_inner(depth + 1)?;
                let total_elements = dimensions.iter().try_fold(1u32, |acc, dim| {
                    let size = u32::try_from(dim.upper - dim.lower + 1)
                        .map_err(|_| SlotCountError::Overflow)?;
                    acc.checked_mul(size).ok_or(SlotCountError::Overflow)
                })?;
                total_elements
                    .checked_mul(elem_slots)
                    .ok_or(SlotCountError::Overflow)
            }

            // STRING/WSTRING: the data-region layout (header plus
            // `max_len * char_width` payload bytes, per ADR-0035), rounded up
            // to whole 8-byte slots. A WSTRING needs twice the payload of a
            // STRING of the same declared length, so `char_width` is part of
            // the sizing and not discarded.
            IntermediateType::String {
                max_len,
                char_width,
            } => {
                let len = max_len.unwrap_or(DEFAULT_STRING_MAX_LENGTH as u128);
                let len = u16::try_from(len).map_err(|_| SlotCountError::Overflow)?;
                let total_bytes = string_region_size(len, *char_width);
                Ok(total_bytes.div_ceil(8))
            }

            // Not yet supported in data region
            IntermediateType::FunctionBlock { .. } | IntermediateType::Function { .. } => {
                Err(SlotCountError::UnsupportedFieldType)
            }
        }
    }
}

/// Represents a field within a structure or function block type in the intermediate representation.
#[derive(Debug, Clone, PartialEq)]
pub struct IntermediateStructField {
    /// Name of the field
    pub name: Id,
    /// Type of the field
    pub field_type: IntermediateType,
    /// Memory offset of the field from the start of the structure (in bytes)
    pub offset: u32,
    /// Variable type for function block fields (None for structure fields)
    pub var_type: Option<FunctionBlockVarType>,
    /// The value the member declares for itself (`a : INT := 5`), in the form
    /// a variable's member initializer takes: the member's own declared
    /// initial value, applied to every storage location of the structure
    /// unless the declaration of that location states another. `None` when
    /// the member states none; the type's own default applies then.
    pub initial_value: Option<StructInitialValueAssignmentKind>,
}

impl IntermediateStructField {
    /// Whether this field has a default value in the type declaration.
    /// Used for const initialization validation - a const variable of structure type
    /// is fully initialized if all fields without defaults are explicitly initialized.
    ///
    /// A field has a default when it states a value, or when it is a
    /// structure whose every field has one.
    pub fn has_default(&self) -> bool {
        self.initial_value.is_some()
            || matches!(
                &self.field_type,
                IntermediateType::Structure { fields, .. } if fields.iter().all(Self::has_default)
            )
    }
}

/// Represents a parameter in a function or function block declaration.
///
/// Uses `TypeName` instead of resolved `IntermediateType` to allow building
/// complete function signatures even when type resolution fails. Types can
/// be resolved on-demand via `TypeEnvironment` when needed for validation.
#[derive(Debug, Clone, PartialEq)]
pub struct IntermediateFunctionParameter {
    /// Name of the parameter
    pub name: ironplc_dsl::core::Id,
    /// Type name of the parameter (resolved on-demand via TypeEnvironment)
    pub param_type: ironplc_dsl::common::TypeName,
    /// Whether this is an input parameter
    pub is_input: bool,
    /// Whether this is an output parameter
    pub is_output: bool,
    /// Whether this is an input-output parameter
    pub is_inout: bool,
    /// Whether this parameter is a REF_TO reference type.
    /// When true, `param_type` holds the referenced type name (e.g., BYTE for REF_TO BYTE).
    pub is_reference: bool,
}

impl IntermediateFunctionParameter {
    /// Returns true if this parameter receives a value from the caller
    /// (either VAR_INPUT or VAR_IN_OUT).
    pub fn is_input_compatible(&self) -> bool {
        self.is_input || self.is_inout
    }
}

#[cfg(test)]
mod tests;
