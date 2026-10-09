//! Transformation rule that changes the order of declarations
//! so that items only have a reference to an already declared item.
//!
//! The order is complete when:
//! 1. there are no cycles and
//! 2. the calls respect the POU hierarchy.
//!
//! Program can call function or function block
//! Function block can call function or other function block
//! Function can call other functions
//!
//! A recursive cycle is the error of its members only: it is reported once,
//! naming the members, the members are entered in the type environment as
//! declarations with an error (see [`crate::resolution`]), and every other
//! declaration is ordered and analyzed as before. The same holds for a
//! construct the sort does not support: the declaration that holds it is
//! reported and entered as a declaration with an error, and the walk goes on.
//! Nothing here stops the analysis.
//!
//! ## Passes
//!
//! ```ignore
//! FUNCTION_BLOCK Callee
//!    VAR
//!       IN1: BOOL;
//!    END_VAR
//! END_FUNCTION_BLOCK
//!
//! FUNCTION_BLOCK Caller
//!    VAR
//!       CalleeInstance : Callee;
//!    END_VAR
//! END_FUNCTION_BLOCK
//! ```
//!
//! ## Fails
//!
//! ```ignore
//! FUNCTION_BLOCK SelfRecursive
//!    VAR
//!       SelfRecursiveInstance : SelfRecursive;
//!    END_VAR
//! END_FUNCTION_BLOCK
//! ```
use core::fmt;
use ironplc_dsl::{
    common::*,
    core::{Id, Located, SourceSpan},
    diagnostic::{Diagnostic, Label},
    visitor::Visitor,
};
use ironplc_problems::Problem;
use log::debug;
use petgraph::{
    algo::{tarjan_scc, toposort},
    dot::{Config, Dot},
    stable_graph::{NodeIndex, StableDiGraph},
    Direction,
};
use std::collections::{HashMap, HashSet, VecDeque};
use std::convert::Infallible;

use crate::type_environment::TypeEnvironment;

/// What the sort gives back: the library in dependency order, the
/// declarations reachable from the programs, what it found wrong, and the
/// declarations it entered as declarations with an error.
pub struct Sorted {
    pub library: Library,
    pub reachable: HashSet<Id>,
    pub diagnostics: Vec<Diagnostic>,
    pub failed: FailedDeclarations,
}

/// The declarations the sort could not order, with what each declares.
pub struct FailedDeclarations(Vec<(Id, Declares)>);

impl FailedDeclarations {
    /// Enters the declarations as declarations with an error. The sort does
    /// this to the environment it is given; a type environment that is made
    /// again from the library needs the same entries.
    pub fn enter(&self, type_environment: &mut TypeEnvironment) {
        for (name, declares) in &self.0 {
            declares.enter_failed(name, type_environment);
        }
    }
}

/// What a declaration is to the environments that hold declared names. A
/// declaration that has an error is entered in the environment of its kind
/// (the same one a valid declaration of that kind is entered in), so its name
/// is declared whatever else happens to it.
#[derive(Clone, Copy, Debug, PartialEq)]
pub enum Declares {
    /// A data type or an interface: a type of the type environment.
    Type,
    /// A function block: a type of the type environment, and a program
    /// organization unit.
    FunctionBlock,
    /// A function. Its signature is made from names and is valid whatever
    /// its parameter types are, so a call of it is checked against it; the
    /// problem is in the order of the calls, not in the function.
    Function,
    /// A program or a configuration, which no declaration uses.
    Unit,
}

impl Declares {
    /// Enters the declaration `name` of this kind as a declaration with an
    /// error.
    fn enter_failed(self, name: &Id, type_environment: &mut TypeEnvironment) {
        match self {
            Declares::Type => type_environment.insert_failed(&TypeName::from_id(name)),
            Declares::FunctionBlock => {
                type_environment.insert_failed_function_block(&TypeName::from_id(name))
            }
            Declares::Function | Declares::Unit => {}
        }
    }
}

/// The names the elements of a library declare, in source order, each with
/// the place of its declaration and what it declares.
fn declared_names(elements: &[LibraryElementKind]) -> Vec<(Id, Declares)> {
    elements.iter().filter_map(declared_name).collect()
}

/// The name an element declares, if it declares one.
fn declared_name(element: &LibraryElementKind) -> Option<(Id, Declares)> {
    match element {
        LibraryElementKind::DataTypeDeclaration(decl) => {
            Some((data_type_name(decl), Declares::Type))
        }
        LibraryElementKind::FunctionDeclaration(decl) => {
            Some((decl.name.clone(), Declares::Function))
        }
        LibraryElementKind::FunctionBlockDeclaration(decl) => {
            Some((decl.name.name.clone(), Declares::FunctionBlock))
        }
        LibraryElementKind::ProgramDeclaration(decl) => Some((decl.name.clone(), Declares::Unit)),
        LibraryElementKind::ConfigurationDeclaration(decl) => {
            Some((decl.name.clone(), Declares::Unit))
        }
        LibraryElementKind::InterfaceDeclaration(decl) => Some((decl.name.clone(), Declares::Type)),
        // Global variables are placed first and take no part in the order;
        // `flatten_namespaces` spliced every namespace out before this.
        LibraryElementKind::GlobalVarDeclarations(_)
        | LibraryElementKind::NamespaceDeclaration(_) => None,
    }
}

/// Orders the declarations of `lib` so that each follows what it uses, and
/// enters every declaration that cannot be ordered in `type_environment` as a
/// declaration with an error.
pub fn apply(lib: Library, type_environment: &mut TypeEnvironment) -> Sorted {
    // A `NAMESPACE` only groups declarations; the semantic model is flat, so
    // its contents are spliced in at the position of the namespace before
    // anything else looks at the library. Namespaces nest, so this recurses.
    let lib = Library {
        elements: flatten_namespaces(lib.elements),
    };
    let declared = declared_names(&lib.elements);
    let mut diagnostics = Vec::new();

    // Walk to build a graph of types, POUs and their relationships
    let mut data_type_visitor = RuleGraphReferenceableElements::new();
    let Ok(()) = data_type_visitor.walk(&lib);
    diagnostics.append(&mut data_type_visitor.diagnostics);

    debug!("Sorted declarations {:?}", data_type_visitor.declarations);

    // A declaration that cannot be ordered, with the name it declares: the
    // members of each cycle and the declarations that hold a construct the
    // sort does not support.
    let mut failed: Vec<Id> = data_type_visitor.unsupported_in.clone();
    let order = data_type_visitor.declarations.order(&declared);
    diagnostics.extend(order.diagnostics);
    failed.extend(order.cyclic);

    let failed: Vec<(Id, Declares)> = failed
        .iter()
        .filter_map(|name| declared.iter().find(|(id, _)| id == name))
        .cloned()
        .collect();

    let sorted_ids = order.sorted;
    debug!("Sorted identifiers {sorted_ids:?}");

    // Compute the set of declarations reachable from PROGRAM roots.
    // This allows downstream passes (e.g. codegen) to skip unused functions.
    let reachable = data_type_visitor
        .declarations
        .reachable_from(&data_type_visitor.program_nodes);

    // Split based on the type so that we put all of the data type declarations
    // at the beginning. Every declaration is kept, a repeated name included:
    // the environments built from the sorted library diagnose the repeat and
    // keep the first declaration, so dropping one here would hide it.
    let mut types_by_name: HashMap<Id, Vec<DataTypeDeclarationKind>> = HashMap::new();
    let mut elems_by_name: HashMap<Id, Vec<LibraryElementKind>> = HashMap::new();
    let mut global_var_decls: Vec<Vec<VarDecl>> = Vec::new();
    for element in lib.elements {
        match element {
            LibraryElementKind::DataTypeDeclaration(decl) => {
                types_by_name
                    .entry(data_type_name(&decl))
                    .or_default()
                    .push(decl);
            }
            LibraryElementKind::FunctionDeclaration(decl) => {
                elems_by_name
                    .entry(decl.name.clone())
                    .or_default()
                    .push(LibraryElementKind::FunctionDeclaration(decl));
            }
            LibraryElementKind::FunctionBlockDeclaration(decl) => {
                elems_by_name
                    .entry(decl.name.name.clone())
                    .or_default()
                    .push(LibraryElementKind::FunctionBlockDeclaration(decl));
            }
            LibraryElementKind::ProgramDeclaration(decl) => {
                elems_by_name
                    .entry(decl.name.clone())
                    .or_default()
                    .push(LibraryElementKind::ProgramDeclaration(decl));
            }
            LibraryElementKind::ConfigurationDeclaration(decl) => {
                elems_by_name
                    .entry(decl.name.clone())
                    .or_default()
                    .push(LibraryElementKind::ConfigurationDeclaration(decl));
            }
            LibraryElementKind::GlobalVarDeclarations(decls) => {
                global_var_decls.push(decls);
            }
            LibraryElementKind::InterfaceDeclaration(decl) => {
                elems_by_name
                    .entry(decl.name.clone())
                    .or_default()
                    .push(LibraryElementKind::InterfaceDeclaration(decl));
            }
            LibraryElementKind::NamespaceDeclaration(_) => {
                // `flatten_namespaces` spliced every namespace out before
                // this loop, so a namespace element cannot reach it.
            }
        }
    }

    // Merge things back together
    let mut elements = Vec::new();
    // Global var declarations go first so they are available for constant resolution
    for decls in global_var_decls {
        elements.push(LibraryElementKind::GlobalVarDeclarations(decls));
    }
    elements.extend(
        sorted_ids
            .iter()
            .filter_map(|id| types_by_name.remove(id))
            .flatten()
            .map(LibraryElementKind::DataTypeDeclaration),
    );
    elements.extend(
        sorted_ids
            .iter()
            .filter_map(|id| elems_by_name.remove(id))
            .flatten(),
    );

    let failed = FailedDeclarations(failed);
    failed.enter(type_environment);
    Sorted {
        library: Library { elements },
        reachable,
        diagnostics,
        failed,
    }
}

/// Replaces each `NAMESPACE` element by its contents, in source order.
/// Namespaces have no semantic model of their own yet (qualified access is
/// P1), so a declaration inside one becomes an ordinary library element.
fn flatten_namespaces(elements: Vec<LibraryElementKind>) -> Vec<LibraryElementKind> {
    let mut flattened = Vec::with_capacity(elements.len());
    for element in elements {
        match element {
            LibraryElementKind::NamespaceDeclaration(namespace) => {
                flattened.extend(flatten_namespaces(namespace.elements));
            }
            other => flattened.push(other),
        }
    }
    flattened
}

/// The declared name of a data type declaration.
fn data_type_name(decl: &DataTypeDeclarationKind) -> Id {
    match decl {
        DataTypeDeclarationKind::Enumeration(d) => d.type_name.name.clone(),
        DataTypeDeclarationKind::Subrange(d) => d.type_name.name.clone(),
        DataTypeDeclarationKind::Simple(d) => d.type_name.name.clone(),
        DataTypeDeclarationKind::Array(d) => d.type_name.name.clone(),
        DataTypeDeclarationKind::Params(d) => d.type_name.name.clone(),
        DataTypeDeclarationKind::Structure(d) => d.type_name.name.clone(),
        DataTypeDeclarationKind::Union(d) => d.type_name.name.clone(),
        DataTypeDeclarationKind::String(d) => d.type_name.name.clone(),
        DataTypeDeclarationKind::Reference(d) => d.type_name.name.clone(),
        DataTypeDeclarationKind::LateBound(d) => d.data_type_name.name.clone(),
    }
}

struct DeclarationsGraph {
    // Represents the types and POUs in the library as a directed graph.
    // Each node is a single type or POU.
    graph: StableDiGraph<Id, (), u32>,

    // Maps between the identifier for some element and the index
    // of tht item in the graph.
    id_to_index: HashMap<Id, NodeIndex>,
    index_to_id: HashMap<NodeIndex, Id>,
}

impl DeclarationsGraph {
    fn new() -> Self {
        Self {
            graph: StableDiGraph::new(),
            id_to_index: HashMap::new(),
            index_to_id: HashMap::new(),
        }
    }

    fn add_node(&mut self, id: &Id) -> NodeIndex<u32> {
        let index = match self.id_to_index.get(id) {
            Some(existing_index) => *existing_index,
            None => {
                let new_index = self.graph.add_node(id.clone());
                self.id_to_index.insert(id.clone(), new_index);
                new_index
            }
        };

        match self.index_to_id.get(&index) {
            Some(_id) => {
                // Already exists
            }
            None => {
                self.index_to_id.insert(index, id.clone());
            }
        }

        index
    }

    /// Computes the set of `Id`s reachable from the given root nodes by
    /// following edges in the *incoming* direction (callee -> caller edges
    /// mean incoming neighbors of a caller are its callees).
    fn reachable_from(&self, roots: &[NodeIndex]) -> HashSet<Id> {
        let mut visited: HashSet<NodeIndex> = HashSet::new();
        let mut queue: VecDeque<NodeIndex> = VecDeque::new();

        for &root in roots {
            queue.push_back(root);
        }

        while let Some(node) = queue.pop_front() {
            if !visited.insert(node) {
                continue;
            }
            for neighbor in self.graph.neighbors_directed(node, Direction::Incoming) {
                queue.push_back(neighbor);
            }
        }

        visited
            .into_iter()
            .filter_map(|idx| self.index_to_id.get(&idx).cloned())
            .collect()
    }

    /// Puts the declarations in dependency order. A declaration in a cycle
    /// cannot be put after what it depends on, so each cycle is reported once
    /// and its members are put last, in source order; the declarations that
    /// are not in a cycle are ordered as if the cycle were not there.
    fn order(&self, declared: &[(Id, Declares)]) -> Order {
        if let Ok(nodes) = toposort(&self.graph, None) {
            return Order {
                sorted: self.ids_of(&nodes),
                cyclic: vec![],
                diagnostics: vec![],
            };
        }

        // Each strongly connected component of more than one declaration, and
        // each declaration that depends on itself, is a cycle.
        let cycles: Vec<Vec<NodeIndex>> = tarjan_scc(&self.graph)
            .into_iter()
            .filter(|component| {
                component.len() > 1
                    || component
                        .first()
                        .is_some_and(|node| self.graph.find_edge(*node, *node).is_some())
            })
            .collect();

        let position = |id: &Id| declared.iter().position(|(declared, _)| declared == id);
        let mut diagnostics = Vec::new();
        let mut cyclic: Vec<Id> = Vec::new();
        let mut members_of_cycles: HashSet<NodeIndex> = HashSet::new();
        for component in &cycles {
            let mut members: Vec<Id> = component
                .iter()
                .filter_map(|node| self.index_to_id.get(node))
                .map(|id| match position(id) {
                    Some(at) => declared[at].0.clone(),
                    None => id.clone(),
                })
                .collect();
            members.sort_by_key(|id| position(id).unwrap_or(usize::MAX));
            diagnostics.push(cycle_diagnostic(&members));
            cyclic.extend(members);
            members_of_cycles.extend(component.iter().copied());
        }
        cyclic.sort_by_key(|id| position(id).unwrap_or(usize::MAX));

        // What is left when the members of the cycles are taken out has no
        // cycle, so it can be ordered.
        let mut rest = self.graph.clone();
        for node in &members_of_cycles {
            rest.remove_node(*node);
        }
        let mut sorted = match toposort(&rest, None) {
            Ok(nodes) => self.ids_of(&nodes),
            Err(_) => {
                diagnostics.push(Diagnostic::internal_error());
                vec![]
            }
        };
        sorted.extend(cyclic.iter().cloned());
        Order {
            sorted,
            cyclic,
            diagnostics,
        }
    }

    /// The identifiers of the nodes, in the order of the nodes.
    fn ids_of(&self, nodes: &[NodeIndex]) -> Vec<Id> {
        nodes
            .iter()
            .filter_map(|node| self.index_to_id.get(node))
            .cloned()
            .collect()
    }
}

/// What ordering the declarations found.
struct Order {
    /// Every declaration, each after the ones it uses; the members of cycles
    /// come last.
    sorted: Vec<Id>,
    /// The members of the cycles, in source order.
    cyclic: Vec<Id>,
    /// One diagnostic for each cycle.
    diagnostics: Vec<Diagnostic>,
}

/// The one diagnostic of a cycle: at its first member in source order, with
/// the other members as secondary locations.
fn cycle_diagnostic(members: &[Id]) -> Diagnostic {
    let span = members
        .first()
        .map(|id| id.span.clone())
        .unwrap_or_default();
    let names: Vec<String> = members.iter().map(|id| id.to_string()).collect();
    let mut diagnostic = Diagnostic::problem(Problem::RecursiveCycle, Label::span(span, "Cycle"))
        .with_context("members", &names.join(", "));
    for member in members.iter().skip(1) {
        diagnostic = diagnostic.with_secondary(Label::span(member.span.clone(), "Cycle member"));
    }
    diagnostic
}

impl fmt::Debug for DeclarationsGraph {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        let dotfile = Dot::with_config(&self.graph, &[Config::EdgeNoLabel]);
        write!(f, "Graph: {dotfile:?}")
    }
}

struct RuleGraphReferenceableElements {
    declarations: DeclarationsGraph,
    // Represents the context while visiting. Tracks the name of the current
    // POU.
    current_from: Option<Id>,
    // Graph node indices for PROGRAM declarations, used as roots for
    // reachability analysis.
    program_nodes: Vec<NodeIndex>,
    // The name the library element being visited declares.
    unit: Option<Id>,
    // The declarations that hold a construct the sort does not support.
    unsupported_in: Vec<Id>,
    // What the walk found wrong. The walk never stops for it.
    diagnostics: Vec<Diagnostic>,
}
impl RuleGraphReferenceableElements {
    fn new() -> Self {
        Self {
            declarations: DeclarationsGraph::new(),
            current_from: None,
            program_nodes: Vec::new(),
            unit: None,
            unsupported_in: Vec::new(),
            diagnostics: Vec::new(),
        }
    }

    /// A construct the sort does not support: it is the error of the
    /// declaration that holds it, which is reported and entered as a
    /// declaration with an error, and the walk goes on.
    fn unsupported(&mut self, span: SourceSpan, what: &str) {
        self.diagnostics
            .push(Diagnostic::not_implemented(Label::span(span, what)));
        if let Some(unit) = &self.unit {
            self.unsupported_in.push(unit.clone());
        }
    }
}

/// The declared type a reference target depends on: the named type itself,
/// or the element type of an inline array target.
fn reference_target_type_name(target: &ReferenceTarget) -> Id {
    match target {
        ReferenceTarget::Named(type_name) => type_name.name.clone(),
        ReferenceTarget::Array(subranges) => subranges.type_name.to_type_name().name,
    }
}

impl Visitor<Infallible> for RuleGraphReferenceableElements {
    type Value = ();

    fn visit_library_element_kind(
        &mut self,
        node: &LibraryElementKind,
    ) -> Result<Self::Value, Infallible> {
        match node {
            // Global variable declarations are not POUs or types and don't
            // participate in the dependency graph. They are unconditionally
            // placed first in the output so that their constants are available
            // for subsequent passes. Skip recursion to avoid hitting visitor
            // methods that require current_from context.
            LibraryElementKind::GlobalVarDeclarations(_) => Ok(()),
            _ => {
                self.unit = declared_name(node).map(|(id, _)| id);
                let result = node.recurse_visit(self);
                self.unit = None;
                result
            }
        }
    }

    // Type declarations

    fn visit_late_bound_declaration(
        &mut self,
        node: &LateBoundDeclaration,
    ) -> Result<Self::Value, Infallible> {
        let this = self.declarations.add_node(&node.data_type_name.name);
        let depends_on = self.declarations.add_node(&node.base_type_name.name);
        self.declarations.graph.add_edge(depends_on, this, ());

        node.recurse_visit(self)
    }

    fn visit_enumeration_declaration(
        &mut self,
        node: &EnumerationDeclaration,
    ) -> Result<Self::Value, Infallible> {
        let this = self.declarations.add_node(&node.type_name.name);

        if let SpecificationKind::Named(parent) = &node.spec_init.spec {
            let depends_on = self.declarations.add_node(&parent.name);
            self.declarations.graph.add_edge(depends_on, this, ());
        };

        node.recurse_visit(self)
    }

    fn visit_subrange_declaration(
        &mut self,
        node: &SubrangeDeclaration,
    ) -> Result<Self::Value, Infallible> {
        let this = self.declarations.add_node(&node.type_name.name);

        if let SpecificationKind::Named(parent) = &node.spec {
            let depends_on = self.declarations.add_node(&parent.name);
            self.declarations.graph.add_edge(depends_on, this, ());
        };

        node.recurse_visit(self)
    }

    fn visit_reference_declaration(
        &mut self,
        node: &ReferenceDeclaration,
    ) -> Result<Self::Value, Infallible> {
        // `REF_TO T` depends on `T`, and `REF_TO ARRAY [..] OF T` on the
        // element type, exactly as `visit_array_declaration` does. Without
        // this edge a `REF_TO` to a type declared in the same source may be
        // resolved before its target exists and fail with a spurious P2011.
        let this = self.declarations.add_node(&node.type_name.name);
        let depends_on = self
            .declarations
            .add_node(&reference_target_type_name(&node.target));
        self.declarations.graph.add_edge(depends_on, this, ());

        node.recurse_visit(self)
    }

    fn visit_array_declaration(
        &mut self,
        node: &ArrayDeclaration,
    ) -> Result<Self::Value, Infallible> {
        let this = self.declarations.add_node(&node.type_name.name);

        match &node.spec {
            SpecificationKind::Named(parent) => {
                let depends_on = self.declarations.add_node(&parent.name);
                self.declarations.graph.add_edge(depends_on, this, ());
            }
            SpecificationKind::Inline(array_subranges) => {
                let depends_on = self
                    .declarations
                    .add_node(&array_subranges.type_name.to_type_name().name);
                self.declarations.graph.add_edge(depends_on, this, ());
            }
        }

        node.recurse_visit(self)
    }

    fn visit_params_declaration(
        &mut self,
        node: &ParamsDeclaration,
    ) -> Result<Self::Value, Infallible> {
        // As for an array declaration: the element type must be ordered
        // before the PARAMS type that names it.
        let this = self.declarations.add_node(&node.type_name.name);
        let depends_on = self.declarations.add_node(&node.spec.type_name.name);
        self.declarations.graph.add_edge(depends_on, this, ());

        node.recurse_visit(self)
    }

    fn visit_structure_declaration(
        &mut self,
        node: &StructureDeclaration,
    ) -> Result<Self::Value, Infallible> {
        self.current_from = Some(node.type_name.name.clone());
        self.declarations.add_node(&node.type_name.name);
        let res = node.recurse_visit(self);
        self.current_from = None;
        res
    }

    fn visit_structure_initialization_declaration(
        &mut self,
        node: &StructureInitializationDeclaration,
    ) -> Result<Self::Value, Infallible> {
        // Save and restore current_from because this visitor can be called
        // both as a top-level type declaration and nested within a program's
        // VarDecl initializer (e.g., `s : MyStruct := (a := 10, b := 20)`).
        // Unconditionally resetting to None would wipe the enclosing
        // program's context when visited as a nested node.
        let prev = self.current_from.take();
        self.current_from = Some(node.type_name.name.clone());
        self.declarations.add_node(&node.type_name.name);
        let res = node.recurse_visit(self);
        self.current_from = prev;
        res
    }

    fn visit_simple_declaration(
        &mut self,
        node: &SimpleDeclaration,
    ) -> Result<Self::Value, Infallible> {
        self.current_from = Some(node.type_name.name.clone());
        self.declarations.add_node(&node.type_name.name);
        let res = node.recurse_visit(self);
        self.current_from = None;
        res
    }

    fn visit_string_declaration(
        &mut self,
        node: &StringDeclaration,
    ) -> Result<Self::Value, Infallible> {
        self.current_from = Some(node.type_name.name.clone());
        self.declarations.add_node(&node.type_name.name);
        let res = node.recurse_visit(self);
        self.current_from = None;
        res
    }

    // POU declarations

    fn visit_function_declaration(
        &mut self,
        node: &FunctionDeclaration,
    ) -> Result<Self::Value, Infallible> {
        self.current_from = Some(node.name.clone());
        self.declarations.add_node(&node.name);
        let res = node.recurse_visit(self);
        self.current_from = None;
        res
    }

    fn visit_function_block_declaration(
        &mut self,
        node: &FunctionBlockDeclaration,
    ) -> Result<Self::Value, Infallible> {
        self.current_from = Some(node.name.name.clone());
        let this = self.declarations.add_node(&node.name.name);
        if let Some(parent) = node.oop.as_ref().and_then(|oop| oop.base.as_ref()) {
            let depends_on = self.declarations.add_node(&parent.name);
            self.declarations.graph.add_edge(depends_on, this, ());
        }
        let res = node.recurse_visit(self);
        self.current_from = None;
        res
    }

    fn visit_program_declaration(
        &mut self,
        node: &ProgramDeclaration,
    ) -> Result<Self::Value, Infallible> {
        self.current_from = Some(node.name.clone());
        let idx = self.declarations.add_node(&node.name);
        self.program_nodes.push(idx);
        let res = node.recurse_visit(self);
        self.current_from = None;
        res
    }

    fn visit_interface_declaration(
        &mut self,
        node: &InterfaceDeclaration,
    ) -> Result<Self::Value, Infallible> {
        self.current_from = Some(node.name.clone());
        let this = self.declarations.add_node(&node.name);
        for parent in &node.extends {
            let depends_on = self.declarations.add_node(&parent.name);
            self.declarations.graph.add_edge(depends_on, this, ());
        }
        let res = node.recurse_visit(self);
        self.current_from = None;
        res
    }

    fn visit_configuration_declaration(
        &mut self,
        node: &ironplc_dsl::configuration::ConfigurationDeclaration,
    ) -> Result<Self::Value, Infallible> {
        self.current_from = Some(node.name.clone());
        self.declarations.add_node(&node.name);
        let res = node.recurse_visit(self);
        self.current_from = None;
        res
    }

    fn visit_function(
        &mut self,
        node: &ironplc_dsl::textual::Function,
    ) -> Result<Self::Value, Infallible> {
        // A function call creates a dependency: the current POU depends on the
        // called function. Add an edge so the called function is ordered first.
        match &self.current_from {
            Some(from) => {
                let from = self.declarations.add_node(from);
                let to = self.declarations.add_node(&node.name);
                self.declarations.graph.add_edge(to, from, ());
            }
            None => self.unsupported(
                node.name.span(),
                "Function call outside a program organization unit",
            ),
        }

        node.recurse_visit(self)
    }

    fn visit_function_block_initial_value_assignment(
        &mut self,
        init: &FunctionBlockInitialValueAssignment,
    ) -> Result<Self::Value, Infallible> {
        // Current context has a reference to this function block. The
        // referenced type must be ordered before the containing POU (same
        // convention as the Structure/LateResolvedType arms in
        // visit_initial_value_assignment_kind below), so the edge points
        // from the referenced type to the containing POU, not the reverse.
        match &self.current_from {
            Some(from) => {
                let from = self.declarations.add_node(from);
                let to = self.declarations.add_node(&init.type_name.name);
                self.declarations.graph.add_edge(to, from, ());
            }
            None => self.unsupported(
                init.type_name.span(),
                "Function block instance outside a program organization unit",
            ),
        }

        Ok(())
    }

    fn visit_initial_value_assignment_kind(
        &mut self,
        node: &InitialValueAssignmentKind,
    ) -> Result<Self::Value, Infallible> {
        match &self.current_from {
            Some(from) => {
                match node {
                    InitialValueAssignmentKind::None(_) => {}
                    // A simple type name may name a user-defined alias (`a : R := 5`),
                    // which must be in the type environment before this declaration
                    // is resolved, like any other type this declaration refers to.
                    InitialValueAssignmentKind::Simple(simple) => {
                        let from = self.declarations.add_node(from);
                        let to = self.declarations.add_node(&simple.type_name.name);
                        self.declarations.graph.add_edge(to, from, ());
                    }
                    InitialValueAssignmentKind::String(_) => {}
                    InitialValueAssignmentKind::EnumeratedValues(_) => {}
                    InitialValueAssignmentKind::EnumeratedType(enum_init) => {
                        // An enum-typed field or variable depends on its
                        // enumeration type, exactly as the LateResolvedType
                        // arm below records for the uninitialized form
                        // `c : Color;`. The parser produces this arm directly
                        // for a qualified initializer (`c : Color := Color#GREEN`)
                        // and for located declarations, so without this edge
                        // the enumeration may be ordered after the declaration
                        // that references it and is then missing from the type
                        // environment, surfacing as a spurious P2021/P2004.
                        let from = self.declarations.add_node(from);
                        let to = self.declarations.add_node(&enum_init.type_name.name);
                        self.declarations.graph.add_edge(to, from, ());
                    }
                    InitialValueAssignmentKind::FunctionBlock(fb) => {
                        // Same ordering convention as the Structure/LateResolvedType
                        // arms below: the referenced type must come before the
                        // containing POU.
                        let from = self.declarations.add_node(from);
                        let to = self.declarations.add_node(&fb.type_name.name);
                        self.declarations.graph.add_edge(to, from, ());
                    }
                    InitialValueAssignmentKind::FunctionBlockCall(fbc) => {
                        // The call-style FB instance initializer references
                        // an FB type just like the FunctionBlock arm above,
                        // so it needs the same referenced-type-before-POU
                        // dependency edge -- otherwise a forward reference
                        // (a POU instantiating a later-declared FB) surfaces
                        // as a spurious P2011.
                        let from = self.declarations.add_node(from);
                        let to = self.declarations.add_node(&fbc.type_name.name);
                        self.declarations.graph.add_edge(to, from, ());
                    }
                    InitialValueAssignmentKind::Subrange(_) => {}
                    // A PARAMS list depends on its element type for the same
                    // reason an array does: the type must be in the
                    // environment before the list that uses it is resolved.
                    InitialValueAssignmentKind::Params(params) => {
                        let from = self.declarations.add_node(from);
                        let to = self.declarations.add_node(&params.type_name.name);
                        self.declarations.graph.add_edge(to, from, ());
                    }
                    InitialValueAssignmentKind::Structure(struct_init) => {
                        // Track dependency on the nested structure type
                        let from = self.declarations.add_node(from);
                        let to = self.declarations.add_node(&struct_init.type_name.name);
                        self.declarations.graph.add_edge(to, from, ());
                    }
                    InitialValueAssignmentKind::Array(array_init) => {
                        // An array-typed field depends on its element type
                        // exactly as `visit_array_declaration` does for a
                        // top-level array type. Without this edge, the
                        // element type may be ordered after the containing
                        // declaration and is then missing from the type
                        // environment, surfacing as a spurious P2013.
                        let element_type_name = match &array_init.spec {
                            SpecificationKind::Named(parent) => parent.name.clone(),
                            SpecificationKind::Inline(subranges) => {
                                subranges.type_name.to_type_name().name
                            }
                        };
                        let from = self.declarations.add_node(from);
                        let to = self.declarations.add_node(&element_type_name);
                        self.declarations.graph.add_edge(to, from, ());
                    }
                    InitialValueAssignmentKind::Reference(_) => {}
                    InitialValueAssignmentKind::LateResolvedType(LateResolvedInitializer {
                        type_name: lrt,
                        ..
                    }) => {
                        // We only care about these because these may be references to a function block
                        let from = self.declarations.add_node(from);
                        let to = self.declarations.add_node(&lrt.name);
                        self.declarations.graph.add_edge(to, from, ());
                    }
                    InitialValueAssignmentKind::SimpleExpr(simple) => {
                        // The expression references a variable or constant by name,
                        // which needs no ordering edge; the declared type does, as
                        // for a literal initializer.
                        let from = self.declarations.add_node(from);
                        let to = self.declarations.add_node(&simple.type_name.name);
                        self.declarations.graph.add_edge(to, from, ());
                    }
                }
            }
            None => {
                // Global variable declarations have no current_from context
                // because they are not inside a POU or type declaration.
                // They don't need dependency edges — they are always placed
                // first in the output.
            }
        }

        node.recurse_visit(self)
    }
}

#[cfg(test)]
mod cycle_tests;
#[cfg(test)]
mod tests;
