//! Attributes of declarations: the `{attribute 'name'}` and
//! `{attribute 'name' := 'value'}` pragmas that stand before a declaration
//! (CODESYS).
//!
//! An attribute is data of the declaration it stands before, so it is a field
//! of the declaration node and travels with the node through every transform.
//! Every node that an attribute can stand before has the field
//! `attributes: Attributes` and implements [`Attributed`]; the match over the
//! declaration kinds in this module has one arm per kind and no wildcard, so
//! a declaration kind added later does not compile until it answers.
//!
//! The lowering of the syntax tree fills the field: each attribute pragma is
//! attached to the declaration it stands before (design: attributes). A node
//! built by hand, or by a parser that reads no pragma, has the empty set.
pub mod census;

use crate::common::{DataTypeDeclarationKind, LibraryElementKind};
use crate::core::SourceSpan;

/// One attribute pragma attached to a declaration: `{attribute 'name'}` or
/// `{attribute 'name' := 'value'}`.
#[derive(Clone, Debug, PartialEq)]
pub struct Attribute {
    /// The name, as written (case is kept).
    pub name: String,
    /// The value, as written, when the pragma has one.
    pub value: Option<String>,
    /// Where the pragma is in the source.
    pub span: SourceSpan,
}

/// The attributes of one declaration, in source order. The default is empty.
///
/// Compared by `PartialEq`: an attribute is source text, and two declarations
/// that carry different attributes are different.
#[derive(Clone, Debug, Default, PartialEq)]
pub struct Attributes(Vec<Attribute>);

impl Attributes {
    /// The attributes of a declaration, in source order.
    pub fn new(attributes: Vec<Attribute>) -> Self {
        Self(attributes)
    }

    /// The attributes in source order.
    pub fn iter(&self) -> std::slice::Iter<'_, Attribute> {
        self.0.iter()
    }

    /// Whether the declaration has no attribute.
    pub fn is_empty(&self) -> bool {
        self.0.is_empty()
    }
}

/// A node that an attribute can stand before.
///
/// Implemented with `#[derive(Attributed)]` for a node that has the field
/// `attributes`, and by hand for the enumerations of nodes, where each
/// variant answers in its own arm.
pub trait Attributed {
    /// The attributes of the declaration.
    fn attributes(&self) -> &Attributes;
}

/// The answer of a declaration kind that no attribute stands before.
static NO_ATTRIBUTES: Attributes = Attributes(Vec::new());

impl Attributed for DataTypeDeclarationKind {
    fn attributes(&self) -> &Attributes {
        match self {
            DataTypeDeclarationKind::Enumeration(declaration) => declaration.attributes(),
            DataTypeDeclarationKind::Subrange(declaration) => declaration.attributes(),
            DataTypeDeclarationKind::Simple(declaration) => declaration.attributes(),
            DataTypeDeclarationKind::Array(declaration) => declaration.attributes(),
            DataTypeDeclarationKind::Structure(declaration) => declaration.attributes(),
            DataTypeDeclarationKind::Union(declaration) => declaration.attributes(),
            DataTypeDeclarationKind::String(declaration) => declaration.attributes(),
            DataTypeDeclarationKind::Reference(declaration) => declaration.attributes(),
            DataTypeDeclarationKind::Params(declaration) => declaration.attributes(),
            DataTypeDeclarationKind::LateBound(declaration) => declaration.attributes(),
        }
    }
}

impl DataTypeDeclarationKind {
    /// This declaration with `attributes` as its attributes, whatever kind of
    /// declaration it is. One arm per kind and no wildcard, as for
    /// [`Attributed`].
    pub fn with_attributes(mut self, attributes: Attributes) -> Self {
        match &mut self {
            DataTypeDeclarationKind::Enumeration(declaration) => {
                declaration.attributes = attributes
            }
            DataTypeDeclarationKind::Subrange(declaration) => declaration.attributes = attributes,
            DataTypeDeclarationKind::Simple(declaration) => declaration.attributes = attributes,
            DataTypeDeclarationKind::Array(declaration) => declaration.attributes = attributes,
            DataTypeDeclarationKind::Structure(declaration) => declaration.attributes = attributes,
            DataTypeDeclarationKind::Union(declaration) => declaration.attributes = attributes,
            DataTypeDeclarationKind::String(declaration) => declaration.attributes = attributes,
            DataTypeDeclarationKind::Reference(declaration) => declaration.attributes = attributes,
            DataTypeDeclarationKind::Params(declaration) => declaration.attributes = attributes,
            DataTypeDeclarationKind::LateBound(declaration) => declaration.attributes = attributes,
        }
        self
    }
}

impl Attributed for LibraryElementKind {
    fn attributes(&self) -> &Attributes {
        match self {
            LibraryElementKind::DataTypeDeclaration(declaration) => declaration.attributes(),
            LibraryElementKind::FunctionDeclaration(declaration) => declaration.attributes(),
            LibraryElementKind::FunctionBlockDeclaration(declaration) => declaration.attributes(),
            LibraryElementKind::ProgramDeclaration(declaration) => declaration.attributes(),
            LibraryElementKind::InterfaceDeclaration(declaration) => declaration.attributes(),
            // Not targets: an attribute stands before the declarations these
            // contain (or, for `VAR_GLOBAL`, before a variable), not before them.
            LibraryElementKind::ConfigurationDeclaration(_) => &NO_ATTRIBUTES,
            LibraryElementKind::GlobalVarDeclarations(_) => &NO_ATTRIBUTES,
            LibraryElementKind::NamespaceDeclaration(_) => &NO_ATTRIBUTES,
        }
    }
}

#[cfg(test)]
mod tests {
    use rstest::rstest;
    use spec_test_macro::spec_test;

    use super::census::{declared, Declared, TargetKind};
    use super::*;
    use crate::common::{
        ArrayDeclaration, EnumeratedSpecificationInit, EnumerationDeclaration,
        FunctionBlockBodyKind, IntegerRef, LateBoundDeclaration, Library, ParamsDeclaration,
        ParamsSpecification, ProgramDeclaration, RefSyntax, ReferenceDeclaration, ReferenceTarget,
        SimpleDeclaration, SpecificationKind, StringDeclaration, StringType, StructureDeclaration,
        SubrangeDeclaration, TypeName, UnionDeclaration, VarDecl,
    };
    use crate::common::{InitialValueAssignmentKind, Integer};
    use crate::configuration::ConfigurationDeclaration;
    use crate::core::Id;

    fn attribute(name: &str, value: Option<&str>) -> Attribute {
        Attribute {
            name: name.to_string(),
            value: value.map(str::to_string),
            span: SourceSpan::default(),
        }
    }

    fn program(attributes: Attributes) -> ProgramDeclaration {
        ProgramDeclaration {
            name: Id::from("main"),
            variables: vec![],
            access_variables: vec![],
            body: FunctionBlockBodyKind::empty(),
            attributes,
        }
    }

    #[test]
    fn attributes_default_when_created_then_empty() {
        assert!(Attributes::default().is_empty());
    }

    #[test]
    fn attributes_iter_when_two_attributes_then_source_order() {
        let attributes = Attributes::new(vec![
            attribute("pack_mode", Some("2")),
            attribute("hide", None),
        ]);

        let names: Vec<&str> = attributes.iter().map(|a| a.name.as_str()).collect();

        assert_eq!(names, vec!["pack_mode", "hide"]);
    }

    #[test]
    fn attributes_when_program_in_library_element_then_the_attributes_of_the_program() {
        let element =
            LibraryElementKind::ProgramDeclaration(program(Attributes::new(vec![attribute(
                "hide", None,
            )])));

        assert_eq!(element.attributes().iter().count(), 1);
    }

    #[test]
    fn attributes_when_structure_in_data_type_declaration_then_the_attributes_of_the_structure() {
        let declaration = DataTypeDeclarationKind::Structure(StructureDeclaration {
            type_name: TypeName::from("point"),
            elements: vec![],
            attributes: Attributes::new(vec![attribute("pack_mode", Some("1"))]),
        });

        assert_eq!(declaration.attributes().iter().count(), 1);
    }

    #[rstest]
    #[case::global_variables(LibraryElementKind::GlobalVarDeclarations(vec![]))]
    #[case::configuration(LibraryElementKind::ConfigurationDeclaration(ConfigurationDeclaration {
        name: Id::from("config"),
        global_var: vec![],
        resource_decl: vec![],
        fb_inits: vec![],
        located_var_inits: vec![],
    }))]
    fn attributes_when_element_is_not_a_target_then_empty(#[case] element: LibraryElementKind) {
        assert!(element.attributes().is_empty());
    }

    #[test]
    fn all_spec_requirements_have_tests() {
        assert!(
            crate::spec_requirements::UNTESTED.is_empty(),
            "Requirements in spec with no conformance test: {:?}",
            crate::spec_requirements::UNTESTED
        );
    }

    #[spec_test(REQ_ATTR_dsl_001)]
    fn eq_when_programs_differ_only_by_attribute_then_not_equal() {
        let plain = program(Attributes::default());
        let attributed = program(Attributes::new(vec![attribute("hide", None)]));

        assert_ne!(plain, attributed);
        assert_eq!(attributed, attributed.clone());
    }

    #[spec_test(REQ_ATTR_dsl_001)]
    fn eq_when_variables_differ_only_by_attribute_then_not_equal() {
        let plain = VarDecl::simple("x", "INT");
        let mut attributed = VarDecl::simple("x", "INT");
        attributed.attributes = Attributes::new(vec![attribute("hide", None)]);

        assert_ne!(plain, attributed);
    }

    /// One declaration of each kind of type, each with the attribute `old`.
    fn data_types() -> Vec<DataTypeDeclarationKind> {
        let old = Attributes::new(vec![attribute("old", None)]);
        let name = || TypeName::from("t");
        let count = || IntegerRef::Literal(Integer::new("2", SourceSpan::default()).unwrap());
        vec![
            DataTypeDeclarationKind::Enumeration(EnumerationDeclaration {
                type_name: name(),
                spec_init: EnumeratedSpecificationInit::values_and_default(vec!["A"], "A"),
                attributes: old.clone(),
            }),
            DataTypeDeclarationKind::Subrange(SubrangeDeclaration {
                type_name: name(),
                spec: SpecificationKind::Named(TypeName::from("base")),
                default: None,
                attributes: old.clone(),
            }),
            DataTypeDeclarationKind::Simple(SimpleDeclaration {
                type_name: name(),
                spec_and_init: InitialValueAssignmentKind::simple_uninitialized(TypeName::from(
                    "INT",
                )),
                attributes: old.clone(),
            }),
            DataTypeDeclarationKind::Array(ArrayDeclaration {
                type_name: name(),
                spec: SpecificationKind::Named(TypeName::from("base")),
                init: vec![],
                attributes: old.clone(),
            }),
            DataTypeDeclarationKind::Structure(StructureDeclaration {
                type_name: name(),
                elements: vec![],
                attributes: old.clone(),
            }),
            DataTypeDeclarationKind::Union(UnionDeclaration {
                type_name: name(),
                elements: vec![],
                attributes: old.clone(),
            }),
            DataTypeDeclarationKind::String(StringDeclaration {
                type_name: name(),
                length: count(),
                width: StringType::String,
                init: None,
                attributes: old.clone(),
            }),
            DataTypeDeclarationKind::Reference(ReferenceDeclaration {
                type_name: name(),
                target: ReferenceTarget::Named(TypeName::from("INT")),
                syntax: RefSyntax::RefTo,
                attributes: old.clone(),
            }),
            DataTypeDeclarationKind::Params(ParamsDeclaration {
                type_name: name(),
                spec: ParamsSpecification {
                    count: count(),
                    type_name: TypeName::from("INT"),
                },
                attributes: old.clone(),
            }),
            DataTypeDeclarationKind::LateBound(LateBoundDeclaration {
                data_type_name: name(),
                base_type_name: TypeName::from("base"),
                attributes: old,
            }),
        ]
    }

    #[spec_test(REQ_ATTR_dsl_002)]
    fn with_attributes_when_any_kind_of_type_declaration_then_its_attributes_are_replaced() {
        let kinds = data_types();
        assert_eq!(kinds.len(), 10);
        for declaration in kinds {
            let before = declaration.clone();
            let replaced = declaration.with_attributes(Attributes::new(vec![
                attribute("first", None),
                attribute("second", Some("2")),
            ]));

            let names: Vec<&str> = replaced
                .attributes()
                .iter()
                .map(|a| a.name.as_str())
                .collect();
            assert_eq!(names, vec!["first", "second"], "{before:?}");
            assert_ne!(replaced, before);
        }
    }

    #[spec_test(REQ_ATTR_dsl_003)]
    fn declared_when_library_has_targets_then_each_with_its_kind_name_and_attributes() {
        let mut variable = VarDecl::simple("x", "INT");
        variable.attributes = Attributes::new(vec![attribute("hide", None)]);
        let mut program = program(Attributes::new(vec![attribute("pack_mode", Some("1"))]));
        program.variables = vec![variable];
        let library = Library {
            elements: vec![LibraryElementKind::ProgramDeclaration(program)],
        };

        let found = declared(&library);

        let summary: Vec<(TargetKind, &str, usize)> = found
            .iter()
            .map(|d: &Declared| (d.kind, d.name.as_str(), d.attributes.iter().count()))
            .collect();
        assert_eq!(
            summary,
            vec![
                (TargetKind::Program, "main", 1),
                (TargetKind::Variable, "x", 1)
            ]
        );
    }

    #[spec_test(REQ_ATTR_dsl_003)]
    fn target_kind_when_all_then_nineteen_kinds_with_distinct_labels() {
        let labels: std::collections::HashSet<&str> =
            TargetKind::ALL.iter().map(|kind| kind.label()).collect();

        assert_eq!(TargetKind::ALL.len(), 19);
        assert_eq!(labels.len(), 19);
    }
}
