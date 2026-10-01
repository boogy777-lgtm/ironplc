//! Configurations: `CONFIGURATION`, `RESOURCE`, `TASK` and program
//! configurations.
//!
//! A configuration holds no statements, so its words are declarations
//! throughout: `PROGRAM inst WITH task : Type` is a program configuration and
//! `task` is a task name, not a label (the legacy pipeline needs a token pass
//! to know that; here the grammar position says it). Like the legacy grammar,
//! a configuration has at most one resource and at most one block of global
//! variables and of instance initialisations, in that order.

use super::common::{
    close, declaration_stops, declared_name, item_terminator, skip_declaration, skip_stray, Order,
    NameClass, Part,
};
use super::expressions::{close_group, name_ref, type_ref, variable};
use super::initializers::value;
use super::var_blocks::{block_ahead, var_block, Scope};
use crate::parser::state::Parser;
use crate::syntax_kind::SyntaxKind as K;

const GLOBALS: Part = Part {
    rank: 0,
    once: true,
    name: "the global variables",
};
const RESOURCE: Part = Part {
    rank: 1,
    once: true,
    name: "the resource",
};
const INSTANCES: Part = Part {
    rank: 2,
    once: true,
    name: "the instance initialisations",
};

/// `CONFIGURATION name [VAR_GLOBAL ...] RESOURCE ... [VAR_CONFIG ...]
/// END_CONFIGURATION`
pub(super) fn configuration(p: &mut Parser) {
    let node = p.start();
    p.bump();
    declared_name(p, NameClass::Plain);
    let owns = [K::VarGlobal];
    let mut order = Order::default();
    while !declaration_stops(p, K::EndConfiguration, &owns) {
        let before = p.position();
        if p.at(K::Resource) {
            order.enter(p, RESOURCE);
            resource(p);
        } else if p.at(K::VarGlobal) {
            order.enter(p, GLOBALS);
            var_block(p, Scope::Global);
        } else if block_ahead(p) {
            order.enter(p, INSTANCES);
            var_block(p, Scope::Configuration);
        } else {
            skip_stray(
                p,
                K::EndConfiguration,
                &owns,
                "expected a resource or a variable block",
            );
        }
        if p.position() == before {
            p.bump_as_error("unexpected input");
        }
    }
    if !order.has(RESOURCE.rank) {
        p.error("a configuration needs a resource");
    }
    close(p, K::EndConfiguration, "`END_CONFIGURATION`");
    p.complete(node, K::ConfigurationDecl);
}

const RESOURCE_GLOBALS: Part = Part {
    rank: 0,
    once: true,
    name: "the global variables",
};
const TASKS: Part = Part {
    rank: 1,
    once: false,
    name: "a task",
};
const PROGRAMS: Part = Part {
    rank: 2,
    once: false,
    name: "a program",
};

/// `RESOURCE name ON type [VAR_GLOBAL ...] {TASK ...;} {PROGRAM ...;}
/// END_RESOURCE`
fn resource(p: &mut Parser) {
    let node = p.start();
    p.bump();
    declared_name(p, NameClass::Plain);
    p.expect(K::On, "`ON`");
    if p.name_at(0) {
        name_ref(p);
    } else {
        p.error("expected the type of the resource");
    }
    let owns = [K::VarGlobal, K::Program];
    let mut order = Order::default();
    while !declaration_stops(p, K::EndResource, &owns) {
        let before = p.position();
        if p.at(K::VarGlobal) {
            order.enter(p, RESOURCE_GLOBALS);
            var_block(p, Scope::Global);
        } else if p.at(K::Task) {
            order.enter(p, TASKS);
            task(p);
        } else if p.at(K::Program) {
            order.enter(p, PROGRAMS);
            program_configuration(p);
        } else {
            skip_stray(
                p,
                K::EndResource,
                &owns,
                "expected a task or a program",
            );
        }
        if p.position() == before {
            p.bump_as_error("unexpected input");
        }
    }
    if !order.has(PROGRAMS.rank) {
        p.error("a resource needs a program");
    }
    close(p, K::EndResource, "`END_RESOURCE`");
    p.complete(node, K::ResourceDecl);
}

const SINGLE: Part = Part {
    rank: 0,
    once: true,
    name: "`SINGLE`",
};
const INTERVAL: Part = Part {
    rank: 1,
    once: true,
    name: "`INTERVAL`",
};
const PRIORITY: Part = Part {
    rank: 2,
    once: true,
    name: "`PRIORITY`",
};

const TASK_PROPERTIES: [(&str, Part); 3] = [
    ("SINGLE", SINGLE),
    ("INTERVAL", INTERVAL),
    ("PRIORITY", PRIORITY),
];

/// `TASK name ( [SINGLE := s,] [INTERVAL := i,] PRIORITY := n ) ;`
fn task(p: &mut Parser) {
    let node = p.start();
    p.bump();
    declared_name(p, NameClass::Plain);
    let init = p.start();
    let mut order = Order::default();
    if p.expect(K::LeftParen, "`(`") {
        loop {
            task_item(p, &mut order);
            if !p.eat(K::Comma) {
                break;
            }
        }
        if !order.has(PRIORITY.rank) {
            p.error("a task needs a `PRIORITY`");
        }
        close_group(p, K::RightParen, "`)`");
    }
    p.complete(init, K::TaskInit);
    p.complete(node, K::TaskDecl);
    item_terminator(p);
}

fn task_item(p: &mut Parser, order: &mut Order) {
    let node = p.start();
    // The legacy grammar matches these names by their exact upper-case spelling.
    let part = TASK_PROPERTIES
        .iter()
        .find(|(word, _)| p.nth(0) == Some(K::Ident) && p.nth_text(0) == *word)
        .map(|(_, part)| *part);
    match part {
        Some(part) => order.enter(p, part),
        None => p.error("expected `SINGLE`, `INTERVAL` or `PRIORITY`"),
    }
    if p.name_at(0) {
        name_ref(p);
    } else {
        p.error("expected a task property");
    }
    if p.expect(K::Assignment, "`:=`") {
        value(p);
    }
    p.complete(node, K::TaskInitItem);
}

/// `PROGRAM [RETAIN | NON_RETAIN] name [WITH task] : type [( connection {,
/// connection} )] ;`
fn program_configuration(p: &mut Parser) {
    let node = p.start();
    p.bump();
    if p.at_any(&[K::Retain, K::NonRetain]) {
        p.bump();
    }
    if !declared_name(p, NameClass::Plain) {
        skip_declaration(p);
        p.abandon(node);
        return;
    }
    if p.at(K::With) {
        p.bump();
        declared_name(p, NameClass::Plain);
    }
    if p.expect(K::Colon, "`:`") {
        type_ref(p);
        if p.at(K::LeftParen) {
            p.bump();
            loop {
                connection(p);
                if !p.eat(K::Comma) {
                    break;
                }
            }
            close_group(p, K::RightParen, "`)`");
        }
    }
    p.complete(node, K::ProgramConfig);
    item_terminator(p);
}

/// `function_block WITH task`, `input := source` or `output => sink`.
fn connection(p: &mut Parser) {
    if p.name_at(0) && p.nth_at(1, K::With) {
        let node = p.start();
        name_ref(p);
        p.bump();
        if p.name_at(0) {
            name_ref(p);
        } else {
            p.error("expected the name of a task");
        }
        p.complete(node, K::TaskBinding);
        return;
    }
    let node = p.start();
    if variable(p).is_none() {
        p.error("expected a variable");
        p.abandon(node);
        return;
    }
    if p.at(K::Assignment) {
        p.bump();
        value(p);
    } else if p.at(K::RightArrow) {
        p.bump();
        if variable(p).is_none() {
            p.error("expected a variable");
        }
    } else {
        p.error("expected `:=` or `=>`");
    }
    p.complete(node, K::ProgramConnection);
}
