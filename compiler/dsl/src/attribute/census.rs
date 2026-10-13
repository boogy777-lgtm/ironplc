//! The census of the declarations an attribute can stand before.
//!
//! [`TargetKind`] names the kinds of declaration that have the field
//! `attributes`, once, and [`declared`] lists the declarations of a library
//! with the attributes each carries. A reader of attributes asks the census
//! and does not walk the library for the kinds itself, so a kind added to
//! [`TargetKind`] is answered for by every reader at once.

use super::Attributes;
use crate::common::{
    ArrayDeclaration, EnumerationDeclaration, FunctionBlockDeclaration, FunctionDeclaration,
    InterfaceDeclaration, LateBoundDeclaration, Library, MethodDeclaration, ParamsDeclaration,
    ProgramDeclaration, PropertyDeclaration, ReferenceDeclaration, SimpleDeclaration,
    StringDeclaration, StructureDeclaration, StructureElementDeclaration, SubrangeDeclaration,
    UnionDeclaration, VarDecl,
};
use crate::sfc::Action;
use crate::visitor::Visitor;
use std::convert::Infallible;

/// Declares [`TargetKind`], its list [`TargetKind::ALL`] and its names from
/// one list of kinds.
macro_rules! target_kinds {
    ($($variant:ident => $label:literal,)*) => {
        /// A kind of declaration that an attribute can stand before: a node of
        /// the library that has the field `attributes`.
        #[derive(Clone, Copy, Debug, PartialEq, Eq, Hash)]
        pub enum TargetKind {
            $($variant,)*
        }

        impl TargetKind {
            /// Every kind, in the order declared.
            pub const ALL: &'static [TargetKind] = &[$(TargetKind::$variant,)*];

            /// How a message names the kind.
            pub fn label(self) -> &'static str {
                match self {
                    $(TargetKind::$variant => $label,)*
                }
            }
        }
    };
}

target_kinds! {
    Enumeration => "enumeration type",
    Subrange => "subrange type",
    Simple => "simple type",
    Array => "array type",
    Structure => "structure type",
    Union => "union type",
    String => "string type",
    Reference => "reference type",
    Params => "parameter list type",
    LateBound => "late-bound type",
    Member => "structure member",
    Variable => "variable",
    Function => "function",
    FunctionBlock => "function block",
    Program => "program",
    Interface => "interface",
    Method => "method",
    Property => "property",
    Action => "action",
}

/// One declaration of a library that an attribute can stand before.
#[derive(Clone, Debug, PartialEq)]
pub struct Declared {
    pub kind: TargetKind,
    /// The name the declaration declares.
    pub name: String,
    pub attributes: Attributes,
}

/// The declarations of `library` that an attribute can stand before, in the
/// order the library holds them, with the attributes each carries.
pub fn declared(library: &Library) -> Vec<Declared> {
    let mut census = Census::default();
    let Ok(()) = census.walk(library);
    census.found
}

#[derive(Default)]
struct Census {
    found: Vec<Declared>,
}

impl Census {
    fn add(&mut self, kind: TargetKind, name: String, attributes: &Attributes) {
        self.found.push(Declared {
            kind,
            name,
            attributes: attributes.clone(),
        });
    }
}

/// One method of the visitor for each kind of [`TargetKind`]: the census
/// records the node and goes on into it.
macro_rules! record {
    ($($method:ident($node:ident): $kind:ident => $name:expr;)*) => {
        $(
            fn $method(&mut self, node: &$node) -> Result<(), Infallible> {
                let name = ($name)(node);
                self.add(TargetKind::$kind, name, &node.attributes);
                node.recurse_visit(self)
            }
        )*
    };
}

impl Visitor<Infallible> for Census {
    type Value = ();

    record! {
        visit_enumeration_declaration(EnumerationDeclaration): Enumeration => |n: &EnumerationDeclaration| n.type_name.to_string();
        visit_subrange_declaration(SubrangeDeclaration): Subrange => |n: &SubrangeDeclaration| n.type_name.to_string();
        visit_simple_declaration(SimpleDeclaration): Simple => |n: &SimpleDeclaration| n.type_name.to_string();
        visit_array_declaration(ArrayDeclaration): Array => |n: &ArrayDeclaration| n.type_name.to_string();
        visit_structure_declaration(StructureDeclaration): Structure => |n: &StructureDeclaration| n.type_name.to_string();
        visit_union_declaration(UnionDeclaration): Union => |n: &UnionDeclaration| n.type_name.to_string();
        visit_string_declaration(StringDeclaration): String => |n: &StringDeclaration| n.type_name.to_string();
        visit_reference_declaration(ReferenceDeclaration): Reference => |n: &ReferenceDeclaration| n.type_name.to_string();
        visit_params_declaration(ParamsDeclaration): Params => |n: &ParamsDeclaration| n.type_name.to_string();
        visit_late_bound_declaration(LateBoundDeclaration): LateBound => |n: &LateBoundDeclaration| n.data_type_name.to_string();
        visit_structure_element_declaration(StructureElementDeclaration): Member => |n: &StructureElementDeclaration| n.name.to_string();
        visit_var_decl(VarDecl): Variable => |n: &VarDecl| n.identifier.to_string();
        visit_function_declaration(FunctionDeclaration): Function => |n: &FunctionDeclaration| n.name.to_string();
        visit_function_block_declaration(FunctionBlockDeclaration): FunctionBlock => |n: &FunctionBlockDeclaration| n.name.to_string();
        visit_program_declaration(ProgramDeclaration): Program => |n: &ProgramDeclaration| n.name.to_string();
        visit_interface_declaration(InterfaceDeclaration): Interface => |n: &InterfaceDeclaration| n.name.to_string();
        visit_method_declaration(MethodDeclaration): Method => |n: &MethodDeclaration| n.name.to_string();
        visit_property_declaration(PropertyDeclaration): Property => |n: &PropertyDeclaration| n.name.to_string();
        visit_action(Action): Action => |n: &Action| n.name.to_string();
    }
}
