//! Pieces every declaration rule shares: declared names, integer references,
//! the closing keyword of a declaration, and the skipping of a malformed
//! declaration.

use super::expressions::name_ref;
use crate::parser::recovery::{BLOCK_END, DECLARATION_END, DECLARATION_START};
use crate::parser::state::Parser;
use crate::syntax_kind::SyntaxKind as K;

/// Which words may be declared as a name.
#[derive(Clone, Copy, PartialEq, Eq)]
pub(super) enum NameClass {
    /// An identifier, or a keyword the dialect has not enabled.
    Plain,
    /// A [`NameClass::Plain`] name, or one of the words that are keywords in
    /// a sequential function chart but common names for variables (`STEP`,
    /// `ON`, `R_EDGE`, `F_EDGE`).
    Variable,
}

pub(super) fn name_ahead(p: &Parser, class: NameClass) -> bool {
    match class {
        NameClass::Plain => p.name_at(0),
        NameClass::Variable => p.variable_name_at(0),
    }
}

/// A declared name as a [`K::Name`] node. Returns false, with an error and
/// nothing consumed, when no name is next.
pub(super) fn declared_name(p: &mut Parser, class: NameClass) -> bool {
    if !name_ahead(p, class) {
        p.error("expected a name");
        return false;
    }
    let node = p.start();
    p.bump();
    p.complete(node, K::Name);
    true
}

/// `name {, name}`. Returns how many names there are, or `None` when the first
/// one is missing.
pub(super) fn name_list(p: &mut Parser, class: NameClass) -> Option<usize> {
    if !declared_name(p, class) {
        return None;
    }
    let mut count = 1;
    while p.at(K::Comma) {
        p.bump();
        if !declared_name(p, class) {
            break;
        }
        count += 1;
    }
    Some(count)
}

/// A count or length: an integer, or the name of a constant.
pub(super) fn integer_ref(p: &mut Parser) {
    if p.at(K::IntegerLit) {
        let node = p.start();
        p.bump();
        p.complete(node, K::IntLiteral);
    } else if p.name_at(0) {
        name_ref(p);
    } else {
        p.error("expected an integer or the name of a constant");
    }
}

/// Consumes the keyword `closer` that ends a declaration, or reports that it
/// is missing and consumes nothing, so that the enclosing declaration or the
/// next one still sees whatever stopped the body.
pub(super) fn close(p: &mut Parser, closer: K, what: &str) {
    if !p.eat(closer) {
        p.error_after_previous(&format!("expected {what}"));
    }
}

/// Skips a malformed declaration in a list whose items end in `;`: tokens
/// up to and including the next `;` go in one error node. It stops, without
/// the `;`, at a block word, and after the first token at the start of the
/// next declaration (`name :`), so one bad item does not swallow its
/// neighbour. Consumes nothing when the next token is a block word.
pub(super) fn skip_declaration(p: &mut Parser) {
    let node = p.start();
    let mut consumed = false;
    while !p.at_eof() {
        if p.at(K::Semicolon) {
            p.bump();
            consumed = true;
            break;
        }
        if p.at_any(BLOCK_END) {
            break;
        }
        if consumed && p.variable_name_at(0) && p.nth_at(1, K::Colon) {
            break;
        }
        p.bump();
        consumed = true;
    }
    if consumed {
        p.complete(node, K::ErrorNode);
    } else {
        p.abandon(node);
    }
}

/// The `;` that ends an item of a declaration list. When it is missing the
/// item's remainder is skipped.
pub(super) fn item_terminator(p: &mut Parser) {
    terminator(p, false);
}

/// Like [`item_terminator`], for an item that may omit its `;` where the
/// dialect allows missing semicolons: the legacy pipeline inserts one after
/// `END_STRUCT`, which ends a type declaration.
pub(super) fn terminator(p: &mut Parser, may_omit: bool) {
    if may_omit && p.options.allow_missing_semicolon {
        p.eat(K::Semicolon);
        return;
    }
    if !p.eat(K::Semicolon) {
        p.error("expected `;`");
        // The next item starts right here: leave it.
        if !(p.variable_name_at(0) && p.nth_at(1, K::Colon)) {
            skip_declaration(p);
        }
    }
}

/// True when a declaration's body stops at the cursor: at the end of input,
/// at the declaration's own closer, at the keyword that opens another
/// declaration, or at another declaration's closer. `owns` lists the opening
/// keywords that belong inside this declaration. A declaration that stops
/// before its closer reports the closer missing and leaves the keyword to
/// whoever owns it.
pub(super) fn declaration_stops(p: &Parser, closer: K, owns: &[K]) -> bool {
    p.at_eof()
        || p.at(closer)
        || (p.at_any(DECLARATION_START) && !owns.iter().any(|kind| p.at(*kind)))
        || p.at_any(DECLARATION_END)
}

/// Wraps the tokens that cannot continue a declaration in one error node,
/// up to where [`declaration_stops`] says the declaration ends.
pub(super) fn skip_stray(p: &mut Parser, closer: K, owns: &[K], message: &str) {
    p.error(message);
    let node = p.start();
    let mut consumed = false;
    while !declaration_stops(p, closer, owns) {
        p.bump();
        consumed = true;
    }
    if consumed {
        p.complete(node, K::ErrorNode);
    } else {
        p.abandon(node);
    }
}

/// One part of a declaration that must come in a fixed order: its rank in
/// the order, whether it may appear only once, and what to call it.
#[derive(Clone, Copy)]
pub(super) struct Part {
    pub rank: u8,
    pub once: bool,
    pub name: &'static str,
}

/// Tracks the parts of a declaration as they appear and reports a part that
/// comes too late or twice. The parts are still parsed, so one misplaced
/// block does not hide the declarations after it.
#[derive(Default)]
pub(super) struct Order {
    reached: u8,
    seen: u32,
}

impl Order {
    pub(super) fn enter(&mut self, p: &mut Parser, part: Part) {
        let bit = 1u32.checked_shl(u32::from(part.rank)).unwrap_or(0);
        if part.rank < self.reached {
            p.error(&format!("{} comes too late here", part.name));
        } else if part.once && self.seen & bit != 0 {
            p.error(&format!("{} may appear only once", part.name));
        }
        self.reached = self.reached.max(part.rank);
        self.seen |= bit;
    }

    /// True when a part of rank `rank` has appeared.
    pub(super) fn has(&self, rank: u8) -> bool {
        1u32.checked_shl(u32::from(rank))
            .is_some_and(|bit| self.seen & bit != 0)
    }
}
