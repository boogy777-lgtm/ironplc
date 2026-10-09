//! The persistent declarations of a library: the one definition of which
//! declarations code generation lays out as persistent state, read by code
//! generation to lay them out and by the engineering side to key them
//! (ADR-0074).
//!
//! The state of a running application is its globals, the variables of its
//! program, and the fields of its function blocks (ADR-0073). A declaration of
//! any of them can carry a stable ID, and an online change that migrates state
//! needs one for each. The engineering side keys exactly the declarations
//! listed here, so a declaration that code generation lays out as persistent
//! cannot be laid out without a key to give it an ID by.
//!
//! | Declaration | Scope of its key |
//! |---|---|
//! | the system uptime globals, when the option provides them | [`GLOBAL_SCOPE`] |
//! | a top-level `VAR_GLOBAL` | [`GLOBAL_SCOPE`] |
//! | a `VAR_GLOBAL` of the configuration code generation compiles | [`GLOBAL_SCOPE`] |
//! | a variable of a program that is not a `VAR_EXTERNAL`, and the hidden variables of its edge inputs | the program |
//! | a field of a function block, and the hidden fields of its edge inputs | the function block type |
//!
//! `VAR_EXTERNAL` aliases a global and is not a second variable. A `VAR_GLOBAL`
//! of a `RESOURCE` is not laid out and is not listed.

use ironplc_dsl::common::{
    FunctionBlockDeclaration, Library, LibraryElementKind, ProgramDeclaration, VarDecl,
    VariableType,
};
use ironplc_dsl::core::Id;

use crate::compile::CodegenOptions;
use crate::compile_edge::hidden_variables;
use crate::compile_fb_layout::instance_fields;

/// The scope of the key of every global, wherever it is declared.
///
/// Code generation matches persistent declarations by name only and lays all
/// globals out in one prefix, so the namespace is one. A scope named after the
/// block that declares the global would change the key, and with it the
/// identity, when the block is renamed or the global moves between blocks, and
/// nothing in the layout would have changed.
pub const GLOBAL_SCOPE: &str = "global";

/// The persistent variables of one scope, in the order they are laid out.
#[derive(Clone, Debug)]
pub struct PersistentScope {
    /// The scope of the key of each variable.
    pub scope: Id,
    /// The variables.
    pub variables: Vec<VarDecl>,
}

/// The global variables code generation lays out: the system uptime globals the
/// options provide, the top-level `VAR_GLOBAL` declarations, and the
/// `VAR_GLOBAL` declarations of the configuration it compiles.
pub fn global_declarations(library: &Library, options: &CodegenOptions) -> Vec<VarDecl> {
    let mut globals: Vec<VarDecl> = Vec::new();
    if options.system_uptime_global {
        for global in &ironplc_analyzer::system_globals::SYSTEM_UPTIME_GLOBALS {
            globals.push(
                VarDecl::simple(global.name, global.type_name).with_type(VariableType::Global),
            );
        }
    }
    for element in &library.elements {
        if let LibraryElementKind::GlobalVarDeclarations(declarations) = element {
            globals.extend_from_slice(declarations);
        }
    }
    if let Some(configuration) = compiled_configuration(library) {
        globals.extend_from_slice(&configuration.global_var);
    }
    globals
}

/// The configuration code generation compiles: the first the library declares.
pub(crate) fn compiled_configuration(
    library: &Library,
) -> Option<&ironplc_dsl::configuration::ConfigurationDeclaration> {
    library.elements.iter().find_map(|element| {
        if let LibraryElementKind::ConfigurationDeclaration(configuration) = element {
            Some(configuration)
        } else {
            None
        }
    })
}

/// The variables of `program` code generation lays out: its declarations except
/// the `VAR_EXTERNAL` aliases of globals, then the hidden variables its edge
/// inputs need.
pub fn program_declarations(program: &ProgramDeclaration) -> Vec<VarDecl> {
    program
        .variables
        .iter()
        .filter(|variable| variable.var_type != VariableType::External)
        .cloned()
        .chain(hidden_variables(&program.variables))
        .collect()
}

/// The fields of an instance of `function_block` code generation lays out: its
/// inputs, outputs and other variables, then the hidden fields its edge inputs
/// need.
pub fn function_block_declarations(function_block: &FunctionBlockDeclaration) -> Vec<VarDecl> {
    instance_fields(function_block)
}

/// Every scope of persistent variables the library declares: the globals, then
/// each program, then each function block, in the order the library declares
/// them.
pub fn persistent_scopes(library: &Library, options: &CodegenOptions) -> Vec<PersistentScope> {
    let mut scopes = vec![PersistentScope {
        scope: Id::from(GLOBAL_SCOPE),
        variables: global_declarations(library, options),
    }];
    for element in &library.elements {
        match element {
            LibraryElementKind::ProgramDeclaration(program) => scopes.push(PersistentScope {
                scope: program.name.clone(),
                variables: program_declarations(program),
            }),
            LibraryElementKind::FunctionBlockDeclaration(function_block) => {
                scopes.push(PersistentScope {
                    scope: function_block.name.name.clone(),
                    variables: function_block_declarations(function_block),
                });
            }
            _ => {}
        }
    }
    scopes
}
