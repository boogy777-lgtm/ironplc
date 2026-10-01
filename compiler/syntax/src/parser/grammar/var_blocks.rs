//! Variable blocks: `VAR ... END_VAR` and the family around it.
//!
//! Which blocks a declaration may hold, which qualifier each takes and how
//! its declarations are written are data, in the rule tables here: one table
//! per kind of declaration, one row per block. Adding a block or allowing it
//! in one more place is adding a row. A block the declaration does not allow
//! is still parsed, with an error at its keyword, so the tree keeps its
//! structure and the rest of the declaration parses normally.
//!
//! A block's declarations are `names [AT address] : type [:= value] ;`. What
//! differs between blocks is a [`Shape`]: whether the names may be the
//! contextual words, a location, an edge qualifier or an initial value is
//! allowed. The access and instance-initialisation blocks of a configuration
//! have their own forms.

use super::common::{declared_name, item_terminator, name_list, skip_declaration, NameClass};
use super::expressions::{type_ref, variable};
use super::initializers::initializer;
use super::types::{type_spec, VARIABLE};
use crate::parser::recovery::{BLOCK_END, VAR_OPENERS};
use crate::parser::state::Parser;
use crate::syntax_kind::SyntaxKind as K;

/// What a block's declarations look like.
#[derive(Clone, Copy)]
pub(super) struct Shape {
    names: NameClass,
    /// `AT %address` may follow the names.
    located: bool,
    /// `BOOL R_EDGE` and `BOOL F_EDGE` are types here.
    edge: bool,
    /// `:= value` is allowed.
    initial: bool,
    /// The declaration may stop after the `:`.
    optional_type: bool,
}

const fn shape(names: NameClass, located: bool, initial: bool) -> Shape {
    Shape {
        names,
        located,
        edge: false,
        initial,
        optional_type: false,
    }
}

/// `VAR_INPUT`: variables, a location, and edge detection.
const INPUT: Shape = Shape {
    edge: true,
    ..shape(NameClass::Variable, true, true)
};
/// `VAR`, `VAR_OUTPUT`: variables with a location and a value.
const LOCATED: Shape = shape(NameClass::Variable, true, true);
/// `VAR_TEMP` and the other blocks without a location.
const PLAIN: Shape = shape(NameClass::Variable, false, true);
/// `VAR_IN_OUT`: the variable is another's, so it has no value.
const BORROWED: Shape = shape(NameClass::Variable, false, false);
/// `VAR_EXTERNAL`: a global declared again, so it has no value.
const EXTERNAL: Shape = shape(NameClass::Plain, false, false);
/// `VAR_GLOBAL`: a name or a location alone, and the type may be left out.
const GLOBAL: Shape = Shape {
    optional_type: true,
    ..shape(NameClass::Plain, true, true)
};

/// How a block's items are written.
#[derive(Clone, Copy)]
enum Form {
    Declarations(Shape),
    /// `name : path : type [READ_ONLY | READ_WRITE]`
    Access,
    /// `resource.program.path [AT address] : type [:= value]`
    Instance,
}

/// One row: a block keyword, the qualifiers it takes, and its form.
struct Rule {
    opener: K,
    qualifiers: &'static [K],
    form: Form,
}

const fn rule(opener: K, qualifiers: &'static [K], form: Form) -> Rule {
    Rule {
        opener,
        qualifiers,
        form,
    }
}

const RETENTION: &[K] = &[K::Retain, K::NonRetain];
const CONSTANT: &[K] = &[K::Constant];
const STORAGE: &[K] = &[K::Constant, K::Retain, K::NonRetain, K::Persistent];
const GLOBAL_STORAGE: &[K] = &[K::Constant, K::Retain, K::Persistent];
const NONE: &[K] = &[];

/// Every qualifier word; any of them after a block keyword is a qualifier.
const QUALIFIERS: &[K] = &[K::Constant, K::Retain, K::NonRetain, K::Persistent];

use Form::{Access, Declarations, Instance};

const PROGRAM: &[Rule] = &[
    rule(K::VarAccess, NONE, Access),
    rule(K::VarInput, RETENTION, Declarations(INPUT)),
    rule(K::VarOutput, RETENTION, Declarations(LOCATED)),
    rule(K::VarInOut, NONE, Declarations(BORROWED)),
    rule(K::Var, STORAGE, Declarations(LOCATED)),
    rule(K::VarExternal, CONSTANT, Declarations(EXTERNAL)),
    rule(K::VarStat, CONSTANT, Declarations(PLAIN)),
];

const FUNCTION: &[Rule] = &[
    rule(K::VarInput, RETENTION, Declarations(INPUT)),
    rule(K::VarOutput, RETENTION, Declarations(LOCATED)),
    rule(K::VarInOut, NONE, Declarations(BORROWED)),
    rule(K::Var, CONSTANT, Declarations(PLAIN)),
    rule(K::VarTemp, NONE, Declarations(PLAIN)),
    rule(K::VarStat, CONSTANT, Declarations(PLAIN)),
];

const FUNCTION_BLOCK: &[Rule] = &[
    rule(K::VarInput, RETENTION, Declarations(INPUT)),
    rule(K::VarOutput, RETENTION, Declarations(LOCATED)),
    rule(K::VarInOut, NONE, Declarations(BORROWED)),
    rule(K::Var, STORAGE, Declarations(LOCATED)),
    rule(K::VarExternal, CONSTANT, Declarations(EXTERNAL)),
    rule(K::VarStat, CONSTANT, Declarations(PLAIN)),
    rule(K::VarTemp, NONE, Declarations(PLAIN)),
];

/// A method, and a property's accessor: a function block's blocks, and
/// `VAR_INST`.
const METHOD: &[Rule] = &[
    rule(K::VarInput, RETENTION, Declarations(INPUT)),
    rule(K::VarOutput, RETENTION, Declarations(LOCATED)),
    rule(K::VarInOut, NONE, Declarations(BORROWED)),
    rule(K::Var, STORAGE, Declarations(LOCATED)),
    rule(K::VarExternal, CONSTANT, Declarations(EXTERNAL)),
    rule(K::VarStat, CONSTANT, Declarations(PLAIN)),
    rule(K::VarTemp, NONE, Declarations(PLAIN)),
    rule(K::VarInst, CONSTANT, Declarations(PLAIN)),
];

/// The blocks that follow a function block's name.
const GENERIC: &[Rule] = &[rule(K::VarGeneric, CONSTANT, Declarations(PLAIN))];

/// Global variables, at the top of a file and in a configuration or resource.
const GLOBAL_BLOCKS: &[Rule] = &[rule(K::VarGlobal, GLOBAL_STORAGE, Declarations(GLOBAL))];

/// A configuration's instance initialisations.
const CONFIGURATION_BLOCKS: &[Rule] = &[rule(K::VarConfig, NONE, Instance)];

/// Every block with the most permissive qualifiers: how a block is read where
/// the declaration does not allow it.
const ANY: &[Rule] = &[
    rule(K::Var, STORAGE, Declarations(LOCATED)),
    rule(K::VarInput, STORAGE, Declarations(INPUT)),
    rule(K::VarOutput, STORAGE, Declarations(LOCATED)),
    rule(K::VarInOut, STORAGE, Declarations(BORROWED)),
    rule(K::VarTemp, STORAGE, Declarations(PLAIN)),
    rule(K::VarExternal, STORAGE, Declarations(EXTERNAL)),
    rule(K::VarAccess, STORAGE, Access),
    rule(K::VarConfig, STORAGE, Instance),
    rule(K::VarGlobal, STORAGE, Declarations(GLOBAL)),
    rule(K::VarStat, STORAGE, Declarations(PLAIN)),
    rule(K::VarInst, STORAGE, Declarations(PLAIN)),
    rule(K::VarGeneric, STORAGE, Declarations(PLAIN)),
];

/// The kind of declaration that holds the blocks.
#[derive(Clone, Copy)]
pub(super) enum Scope {
    Program,
    Function,
    FunctionBlock,
    Method,
    /// The blocks right after a function block's name.
    Generic,
    Global,
    Configuration,
}

impl Scope {
    fn rules(self) -> &'static [Rule] {
        match self {
            Scope::Program => PROGRAM,
            Scope::Function => FUNCTION,
            Scope::FunctionBlock => FUNCTION_BLOCK,
            Scope::Method => METHOD,
            Scope::Generic => GENERIC,
            Scope::Global => GLOBAL_BLOCKS,
            Scope::Configuration => CONFIGURATION_BLOCKS,
        }
    }

    fn name(self) -> &'static str {
        match self {
            Scope::Program => "a program",
            Scope::Function => "a function",
            Scope::FunctionBlock => "a function block",
            Scope::Method => "a method",
            Scope::Generic => "the start of a function block",
            Scope::Global => "this place",
            Scope::Configuration => "a configuration",
        }
    }
}

/// True when a block keyword is next.
pub(super) fn block_ahead(p: &Parser) -> bool {
    p.at_any(VAR_OPENERS)
}

/// A variable block, at its keyword.
pub(super) fn var_block(p: &mut Parser, scope: Scope) {
    let Some(opener) = p.nth(0) else {
        return;
    };
    let allowed = scope.rules().iter().find(|rule| rule.opener == opener);
    let Some(rule) = allowed
        .or_else(|| ANY.iter().find(|rule| rule.opener == opener))
    else {
        return;
    };
    let node = p.start();
    if allowed.is_none() {
        p.error(&format!(
            "this kind of variable block is not allowed in {}",
            scope.name()
        ));
    }
    p.bump();
    if let Some(qualifier) = p.nth(0).filter(|kind| p.at(*kind) && QUALIFIERS.contains(kind)) {
        if !rule.qualifiers.contains(&qualifier) {
            p.error("this qualifier is not allowed on this variable block");
        }
        p.bump();
    }
    match rule.form {
        Declarations(shape) => items(p, false, |p| declaration(p, shape)),
        Access => items(p, true, access_declaration),
        Instance => items(p, true, instance_initialization),
    }
    if !p.eat(K::EndVar) {
        p.error("expected `END_VAR`");
    }
    p.complete(node, K::VarBlock);
}

/// The items of a block, each ending in `;`, up to the block's end. The
/// legacy grammar's list lets a lone `;` stand for a list without items.
fn items(p: &mut Parser, at_least_one: bool, mut item: impl FnMut(&mut Parser)) {
    let mut count = 0usize;
    if p.at(K::Semicolon) && p.nth_at(1, K::EndVar) && !at_least_one {
        p.bump();
        return;
    }
    while !p.at_eof() && !p.at_any(BLOCK_END) {
        let before = p.position();
        item(p);
        count += 1;
        if p.position() == before {
            p.bump_as_error("expected a declaration");
        }
    }
    if at_least_one && count == 0 {
        p.error("expected a declaration");
    }
}

/// `names [AT address] : type [R_EDGE | F_EDGE] [:= value] ;`
fn declaration(p: &mut Parser, shape: Shape) {
    let node = p.start();
    let no_name = shape.located && p.at(K::At);
    if !no_name && !name_list(p, shape.names) {
        skip_declaration(p);
        p.abandon(node);
        return;
    }
    if shape.located && p.at(K::At) {
        location(p);
    }
    if p.expect(K::Colon, "`:`") && !(shape.optional_type && p.at(K::Semicolon)) {
        type_spec(p, VARIABLE);
        if shape.edge && p.at_any(&[K::REdge, K::FEdge]) {
            let edge = p.start();
            p.bump();
            p.complete(edge, K::EdgeSpec);
        }
        if p.at(K::Assignment) {
            if !shape.initial {
                p.error("an initial value is not allowed on this variable block");
            }
            initializer(p);
        }
    }
    p.complete(node, K::VarDecl);
    item_terminator(p);
}

/// `AT %address`, a complete address or an incomplete one (`%I*`).
fn location(p: &mut Parser) {
    let node = p.start();
    p.bump();
    if p.at(K::DirectAddress) || p.at(K::DirectAddressIncomplete) {
        p.bump();
    } else {
        p.error("expected a direct address");
    }
    p.complete(node, K::Location);
}

/// `name : path : type [READ_ONLY | READ_WRITE] ;`
fn access_declaration(p: &mut Parser) {
    let node = p.start();
    if !declared_name(p, NameClass::Plain) {
        skip_declaration(p);
        p.abandon(node);
        return;
    }
    if p.expect(K::Colon, "`:`") {
        access_path(p);
        if p.expect(K::Colon, "`:`") {
            type_ref(p);
            if p.at_any(&[K::ReadOnly, K::ReadWrite]) {
                p.bump();
            }
        }
    }
    p.complete(node, K::AccessDecl);
    item_terminator(p);
}

/// A path to a variable: `a.b.c`, `%IX0.1`, or `resource.%IX0.1`.
fn access_path(p: &mut Parser) {
    if p.name_at(0) && p.nth_at(1, K::Period) && p.nth_at(2, K::DirectAddress) {
        let node = p.start();
        p.bump_n(3);
        p.complete(node, K::DirectAddressExpr);
    } else if variable(p).is_none() {
        p.error("expected a variable path");
    }
}

/// `resource.program.path [AT address] : type [:= value] ;`
fn instance_initialization(p: &mut Parser) {
    let node = p.start();
    if variable(p).is_none() {
        p.error("expected the path of an instance");
        skip_declaration(p);
        p.abandon(node);
        return;
    }
    if p.at(K::At) {
        location(p);
    }
    if p.expect(K::Colon, "`:`") {
        type_spec(p, VARIABLE);
        if p.at(K::Assignment) {
            initializer(p);
        }
    }
    p.complete(node, K::InstanceInit);
    item_terminator(p);
}
