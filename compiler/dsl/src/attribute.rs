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
//! The lowering does not fill the field in yet: every node has the empty
//! set, and a later change attaches each attribute pragma to the declaration
//! it stands before.
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

    use super::*;
    use crate::common::{
        FunctionBlockBodyKind, ProgramDeclaration, StructureDeclaration, TypeName, VarDecl,
    };
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
    fn eq_when_programs_differ_only_by_attribute_then_not_equal() {
        let plain = program(Attributes::default());
        let attributed = program(Attributes::new(vec![attribute("hide", None)]));

        assert_ne!(plain, attributed);
        assert_eq!(attributed, attributed.clone());
    }

    #[test]
    fn eq_when_variables_differ_only_by_attribute_then_not_equal() {
        let plain = VarDecl::simple("x", "INT");
        let mut attributed = VarDecl::simple("x", "INT");
        attributed.attributes = Attributes::new(vec![attribute("hide", None)]);

        assert_ne!(plain, attributed);
    }
}
