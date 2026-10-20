//! Libraries and the top-level elements a library contains.

use dsl_macro_derive::{Located, Recurse};

use super::{
    DataTypeDeclarationKind, FunctionBlockDeclaration, FunctionDeclaration, InterfaceDeclaration,
    ProgramDeclaration, VarDecl,
};
use crate::configuration::ConfigurationDeclaration;
use crate::core::{Id, SourceSpan};
use crate::fold::Fold;
use crate::visitor::Visitor;

/// Container for top-level elements that are valid top-level declarations in
/// a library.
///
/// The library element flattens data type declaration blocks so that each
/// enumeration is for a single data type declaration.
#[cfg_attr(not(feature = "count-library-clones"), derive(Clone))]
#[derive(Debug, PartialEq, Recurse)]
pub enum LibraryElementKind {
    DataTypeDeclaration(DataTypeDeclarationKind),
    FunctionDeclaration(FunctionDeclaration),
    FunctionBlockDeclaration(FunctionBlockDeclaration),
    ProgramDeclaration(ProgramDeclaration),
    ConfigurationDeclaration(ConfigurationDeclaration),
    /// Top-level global variable declarations (extension).
    ///
    /// In the IEC 61131-3 standard, VAR_GLOBAL only appears inside
    /// CONFIGURATION/RESOURCE blocks. This variant enables the common
    /// extension of declaring globals at the top level.
    GlobalVarDeclarations(Vec<VarDecl>),
    /// `INTERFACE ... END_INTERFACE` (extension). See
    /// `InterfaceDeclaration`.
    InterfaceDeclaration(InterfaceDeclaration),
    /// `NAMESPACE ... END_NAMESPACE` (CODESYS/TwinCAT). A grouping
    /// declaration: it contains other library elements, including further
    /// namespaces. See [`NamespaceDeclaration`].
    NamespaceDeclaration(NamespaceDeclaration),
}

/// `NAMESPACE name { element } END_NAMESPACE` (CODESYS/TwinCAT).
///
/// The declarations a namespace contains are ordinary declarations; the
/// namespace groups them and qualifies their names. IronPLC keeps the
/// nesting in the AST so the source renders back unchanged, and flattens it
/// during declaration toposort so analysis and code generation see the same
/// top-level declarations as before
/// (`analyzer/src/xform_toposort_declarations.rs`). Resolving qualified
/// access (`ns#name`) is not implemented yet (P1 in
/// `Codesys/LEXER-GAP-ANALYSIS.md`).
#[derive(Clone, Debug, PartialEq, Recurse, Located)]
pub struct NamespaceDeclaration {
    /// The namespace name, as written.
    pub name: Id,
    /// The declarations this namespace contains, in source order.
    pub elements: Vec<LibraryElementKind>,
    #[located(position)]
    pub span: SourceSpan,
}

/// Container for a library that contains top-level elements. Libraries are
/// typically represented as a file resource.
#[derive(Clone, Debug, PartialEq, Recurse)]
pub struct Library {
    pub elements: Vec<LibraryElementKind>,
}

/// How many library elements have been cloned in this process.
#[cfg(feature = "count-library-clones")]
static LIBRARY_ELEMENT_CLONES: std::sync::atomic::AtomicUsize =
    std::sync::atomic::AtomicUsize::new(0);

/// The number of library elements cloned so far in this process.
///
/// A copy of a library is a clone of each of its elements, so the difference
/// between two readings is what a stretch of code copied: a guard that no step
/// of the analysis keeps a copy of the library reads this around each step.
/// Only built with the feature `count-library-clones`, which only that guard
/// enables: without it `LibraryElementKind` has the derived `Clone` and counts
/// nothing.
#[cfg(feature = "count-library-clones")]
pub fn library_element_clones() -> usize {
    LIBRARY_ELEMENT_CLONES.load(std::sync::atomic::Ordering::Relaxed)
}

#[cfg(feature = "count-library-clones")]
impl Clone for LibraryElementKind {
    fn clone(&self) -> Self {
        LIBRARY_ELEMENT_CLONES.fetch_add(1, std::sync::atomic::Ordering::Relaxed);
        match self {
            Self::DataTypeDeclaration(x) => Self::DataTypeDeclaration(x.clone()),
            Self::FunctionDeclaration(x) => Self::FunctionDeclaration(x.clone()),
            Self::FunctionBlockDeclaration(x) => Self::FunctionBlockDeclaration(x.clone()),
            Self::ProgramDeclaration(x) => Self::ProgramDeclaration(x.clone()),
            Self::ConfigurationDeclaration(x) => Self::ConfigurationDeclaration(x.clone()),
            Self::GlobalVarDeclarations(x) => Self::GlobalVarDeclarations(x.clone()),
            Self::InterfaceDeclaration(x) => Self::InterfaceDeclaration(x.clone()),
            Self::NamespaceDeclaration(x) => Self::NamespaceDeclaration(x.clone()),
        }
    }
}

impl Default for Library {
    fn default() -> Self {
        Library::new()
    }
}

impl Library {
    // Constructs a new empty library.
    pub fn new() -> Self {
        Library {
            elements: Vec::new(),
        }
    }

    /// Extends a library with the contents of another library.
    pub fn extend(mut self, other: Library) -> Self {
        self.elements.extend(other.elements);
        self
    }

    /// The configurations the library declares, in the order they are written
    /// (file, then offset), not in the order of the elements of an analyzed
    /// library, which the analyzer sorts by what the elements use.
    pub fn configurations_in_source_order(&self) -> Vec<&ConfigurationDeclaration> {
        let mut configurations: Vec<&ConfigurationDeclaration> = self
            .elements
            .iter()
            .filter_map(|element| match element {
                LibraryElementKind::ConfigurationDeclaration(configuration) => Some(configuration),
                LibraryElementKind::DataTypeDeclaration(_)
                | LibraryElementKind::FunctionDeclaration(_)
                | LibraryElementKind::FunctionBlockDeclaration(_)
                | LibraryElementKind::ProgramDeclaration(_)
                | LibraryElementKind::GlobalVarDeclarations(_)
                | LibraryElementKind::InterfaceDeclaration(_)
                | LibraryElementKind::NamespaceDeclaration(_) => None,
            })
            .collect();
        configurations.sort_by_key(|configuration| configuration.name.source_position());
        configurations
    }

    /// The configuration a build compiles, and that the line "Build started:
    /// Application: {name}" names: the first one written.
    ///
    /// A container is one PLC and carries one configuration, so code generation
    /// rejects a library with more than one (`select_configuration`), and no
    /// build chooses between them. The name of the application of a build that
    /// fails for that reason is still the first one written, the same order the
    /// rejection lists them in. This is the one place that chooses it.
    pub fn compiled_configuration(&self) -> Option<&ConfigurationDeclaration> {
        self.configurations_in_source_order().into_iter().next()
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::core::FileId;

    /// A configuration whose name is written at `start` of the file `file`.
    fn configuration_at(name: &str, file: &str, start: usize) -> LibraryElementKind {
        LibraryElementKind::ConfigurationDeclaration(ConfigurationDeclaration {
            name: Id::from(name).with_position(
                SourceSpan::range(start, start + 1).with_file_id(&FileId::from_string(file)),
            ),
            global_var: vec![],
            resource_decl: vec![],
            fb_inits: vec![],
            located_var_inits: vec![],
        })
    }

    fn names(configurations: &[&ConfigurationDeclaration]) -> Vec<String> {
        configurations
            .iter()
            .map(|configuration| configuration.name.to_string())
            .collect()
    }

    #[test]
    fn configurations_in_source_order_when_elements_are_in_another_order_then_by_file_and_offset() {
        // The elements are in the order an analyzer may leave them in, not the
        // order they are written in.
        let library = Library {
            elements: vec![
                configuration_at("Third", "b.st", 5),
                configuration_at("First", "a.st", 90),
                configuration_at("Second", "a.st", 200),
            ],
        };

        assert_eq!(
            names(&library.configurations_in_source_order()),
            ["First", "Second", "Third"]
        );
    }

    #[test]
    fn compiled_configuration_when_several_then_the_first_by_position_and_when_none_then_none() {
        let library = Library {
            elements: vec![
                configuration_at("Written2nd", "a.st", 90),
                configuration_at("Written1st", "a.st", 10),
            ],
        };

        assert_eq!(
            library
                .compiled_configuration()
                .map(|configuration| configuration.name.to_string()),
            Some("Written1st".to_string())
        );
        assert!(Library::new().compiled_configuration().is_none());
    }
}
