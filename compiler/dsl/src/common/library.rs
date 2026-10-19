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

    /// The configuration a build compiles: the first one the library declares.
    ///
    /// This is the one place that chooses it. Code generation compiles it, the
    /// MCP tool `compile` reports its tasks, and the line "Build started:
    /// Application: {name}" names it, so a program with several configurations
    /// is named for the configuration that is built. The analyzer keeps the
    /// configurations in the order they were written (`xform_toposort_declarations`),
    /// so "the first the library declares" is the first in source order.
    pub fn compiled_configuration(&self) -> Option<&ConfigurationDeclaration> {
        self.elements.iter().find_map(|element| match element {
            LibraryElementKind::ConfigurationDeclaration(configuration) => Some(configuration),
            LibraryElementKind::DataTypeDeclaration(_)
            | LibraryElementKind::FunctionDeclaration(_)
            | LibraryElementKind::FunctionBlockDeclaration(_)
            | LibraryElementKind::ProgramDeclaration(_)
            | LibraryElementKind::GlobalVarDeclarations(_)
            | LibraryElementKind::InterfaceDeclaration(_)
            | LibraryElementKind::NamespaceDeclaration(_) => None,
        })
    }
}
