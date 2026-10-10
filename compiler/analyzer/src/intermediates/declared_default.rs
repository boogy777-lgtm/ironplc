//! The value a type declaration states for the variables of the type.
//!
//! `TYPE Level : INT := 5; END_TYPE`, `TYPE Mode : (Off, On) := On;`,
//! `TYPE Name : STRING[10] := 'x';` and `TYPE Row : ARRAY [0..2] OF INT := [1, 2, 3];`
//! each state a value that a variable of the type starts at when its own
//! declaration states none. The type environment records it with the type
//! (`TypeEnvironment::set_initial_value`), and every later question of the
//! form "what does a variable of this type start at" is answered from that
//! record (`TypeEnvironment::initial_value_of`).
//!
//! This is the one place that says what each kind of declaration states. A
//! kind of declaration that states nothing says so here, and a new kind of
//! declaration cannot be entered in the environment without saying which.

use ironplc_dsl::common::{
    ArrayDeclaration, ConstantKind, EnumeratedDefault, EnumeratedValue, EnumerationDeclaration,
    IntegerLiteral, InterfaceDeclaration, ParamsDeclaration, ReferenceDeclaration,
    SimpleDeclaration, StringDeclaration, StructInitialValueAssignmentKind, StructureDeclaration,
    SubrangeDeclaration, UnionDeclaration,
};

/// A declaration of a type, and the value it states for its variables.
pub(crate) trait DeclaresDefault {
    /// The value the declaration states, in the form a member initializer or a
    /// storage location takes it; `None` when it states none, so that the
    /// default of the type it names (or of its kind) applies.
    fn declared_default(&self) -> Option<StructInitialValueAssignmentKind>;
}

impl DeclaresDefault for SimpleDeclaration {
    fn declared_default(&self) -> Option<StructInitialValueAssignmentKind> {
        self.spec_and_init.stated_value()
    }
}

impl DeclaresDefault for EnumerationDeclaration {
    /// A value is named by the enumeration it is a value of, so that it
    /// means the same wherever it is read, whichever other enumerations
    /// declare a value of that name. A number is a constant, as the default of
    /// a subrange is.
    fn declared_default(&self) -> Option<StructInitialValueAssignmentKind> {
        self.spec_init
            .default
            .as_ref()
            .map(|default| match default {
                EnumeratedDefault::Value(value) => {
                    StructInitialValueAssignmentKind::EnumeratedValue(EnumeratedValue {
                        type_name: Some(self.type_name.clone()),
                        value: value.value.clone(),
                        explicit_value: None,
                    })
                }
                EnumeratedDefault::Number(number) => StructInitialValueAssignmentKind::Constant(
                    ConstantKind::IntegerLiteral(IntegerLiteral {
                        value: number.clone(),
                        data_type: None,
                    }),
                ),
            })
    }
}

impl DeclaresDefault for StringDeclaration {
    fn declared_default(&self) -> Option<StructInitialValueAssignmentKind> {
        self.init.as_ref().map(|literal| {
            StructInitialValueAssignmentKind::Constant(ConstantKind::CharacterString(
                literal.clone(),
            ))
        })
    }
}

impl DeclaresDefault for ArrayDeclaration {
    fn declared_default(&self) -> Option<StructInitialValueAssignmentKind> {
        (!self.init.is_empty()).then(|| StructInitialValueAssignmentKind::Array(self.init.clone()))
    }
}

impl DeclaresDefault for SubrangeDeclaration {
    fn declared_default(&self) -> Option<StructInitialValueAssignmentKind> {
        self.default.clone().map(|value| {
            StructInitialValueAssignmentKind::Constant(ConstantKind::IntegerLiteral(
                IntegerLiteral {
                    value,
                    data_type: None,
                },
            ))
        })
    }
}

// The members of a structure state their own values, and a type that is a
// copy of a structure states the members it overrides in a `SimpleDeclaration`.
// A union, a `PARAMS` list, a reference and an interface state no value.
impl DeclaresDefault for StructureDeclaration {
    fn declared_default(&self) -> Option<StructInitialValueAssignmentKind> {
        None
    }
}

impl DeclaresDefault for UnionDeclaration {
    fn declared_default(&self) -> Option<StructInitialValueAssignmentKind> {
        None
    }
}

impl DeclaresDefault for ParamsDeclaration {
    fn declared_default(&self) -> Option<StructInitialValueAssignmentKind> {
        None
    }
}

impl DeclaresDefault for ReferenceDeclaration {
    fn declared_default(&self) -> Option<StructInitialValueAssignmentKind> {
        None
    }
}

impl DeclaresDefault for InterfaceDeclaration {
    fn declared_default(&self) -> Option<StructInitialValueAssignmentKind> {
        None
    }
}
