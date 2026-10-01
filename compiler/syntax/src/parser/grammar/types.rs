//! Type specifications and `TYPE` declarations.
//!
//! One rule, [`type_spec`], reads what follows the `:` of a declaration,
//! wherever it is: a variable, a structure member, a type alias, a function's
//! return type. Where the legacy grammar writes a separate rule per position,
//! the position here is a [`Context`]: whether a structure or union may be
//! declared in place, and whether `TypeName(...)` is the call-style
//! initialisation of a function block instance.
//!
//! The forms are `ARRAY [ranges] OF T`, `ARRAY [*] OF T`, `STRING[n]`,
//! `WSTRING(n)`, `REF_TO T`, `REFERENCE TO T`, `POINTER TO T`,
//! `PARAMS(n) OF T`, an inline enumeration `(A, B := 2) BYTE`, a subrange
//! `INT(1..10)`, `STRUCT ... END_STRUCT`, `UNION ... END_UNION`, and a type
//! name. Their gating (`REF_TO`, `PARAMS`, `UNION`, ...) is the keyword table
//! in `ParseOptions`; whether a form is enabled by a flag that is not about a
//! keyword (`STRING(n)`, `ARRAY[*]`) is a gate, see `parser::gates`.

use super::common::{
    declared_name, integer_ref, item_terminator, skip_declaration, NameClass,
};
use super::control::{bound, bound_ahead};
use super::expressions::{arg_list, close_group, expression, type_ref};
use super::initializers::initializer;
use crate::parser::recovery::BLOCK_END;
use crate::parser::state::Parser;
use crate::syntax_kind::SyntaxKind as K;

/// Where a type specification is read.
#[derive(Clone, Copy)]
pub(super) struct Context {
    /// `STRUCT` and `UNION` declare a type in place.
    aggregate: bool,
    /// A type name followed by `(` is the call-style initialisation of a
    /// function block instance.
    call: bool,
}

/// The type of a variable.
pub(super) const VARIABLE: Context = Context {
    aggregate: false,
    call: true,
};
/// The type a `TYPE` declaration defines.
const DECLARED: Context = Context {
    aggregate: true,
    call: false,
};
/// The type of a structure member, an array element or a reference target.
const MEMBER: Context = Context {
    aggregate: false,
    call: false,
};

/// The integer types, which a subrange restricts.
const INTEGER_TYPES: &[K] = &[
    K::Sint,
    K::Int,
    K::Dint,
    K::Lint,
    K::Usint,
    K::Uint,
    K::Udint,
    K::Ulint,
];

/// The types an enumeration may be based on: the integers and the bit strings.
const ENUM_BASE_TYPES: &[K] = &[
    K::Sint,
    K::Int,
    K::Dint,
    K::Lint,
    K::Usint,
    K::Uint,
    K::Udint,
    K::Ulint,
    K::Bool,
    K::Byte,
    K::Word,
    K::Dword,
    K::Lword,
];

/// A type specification. A malformed one is reported and left in place
/// where it stops, so the declaration around it can still end.
pub(super) fn type_spec(p: &mut Parser, context: Context) {
    p.guarded(
        |p| specification(p, context),
        |p| {
            if !p.at_eof() {
                let node = p.start();
                p.bump();
                p.complete(node, K::ErrorNode);
            }
        },
    );
}

fn specification(p: &mut Parser, context: Context) {
    let Some(kind) = p.nth(0).filter(|kind| p.at(*kind)) else {
        return named(p, context);
    };
    match kind {
        K::Array => array_type(p),
        K::String | K::WString => string_type(p),
        K::RefTo => reference_type(p, 1),
        K::Reference | K::Pointer => reference_type(p, 2),
        K::Params => params_type(p),
        K::LeftParen => enum_type(p),
        K::Struct if context.aggregate => aggregate(p, K::StructType, K::EndStruct),
        K::Union if context.aggregate => aggregate(p, K::UnionType, K::EndUnion),
        kind if INTEGER_TYPES.contains(&kind) && p.nth_at(1, K::LeftParen) => subrange_type(p),
        _ => named(p, context),
    }
}

/// A type name, and the arguments of a function block instance that follow
/// it where the position allows them.
fn named(p: &mut Parser, context: Context) {
    let call = context.call && p.name_at(0);
    type_ref(p);
    if call && p.at(K::LeftParen) {
        arg_list(p);
    }
}

/// `STRING`, `WSTRING`, optionally with a length in `[ ]` or `( )`.
pub(super) fn string_type(p: &mut Parser) {
    let node = p.start();
    p.bump();
    let closer = match p.nth(0) {
        Some(K::LeftBracket) => Some((K::RightBracket, "`]`")),
        Some(K::LeftParen) => Some((K::RightParen, "`)`")),
        _ => None,
    };
    if let Some((closer, what)) = closer {
        p.bump();
        integer_ref(p);
        close_group(p, closer, what);
    }
    p.complete(node, K::StringType);
}

/// `ARRAY [ranges] OF T`, or `ARRAY [*] OF T` for the array whose bounds the
/// caller supplies.
fn array_type(p: &mut Parser) {
    let node = p.start();
    p.bump();
    if p.expect(K::LeftBracket, "`[`") {
        if p.at(K::Star) {
            p.bump();
        } else {
            loop {
                subrange(p);
                if !p.eat(K::Comma) {
                    break;
                }
            }
        }
        close_group(p, K::RightBracket, "`]`");
    }
    p.expect(K::Of, "`OF`");
    type_spec(p, MEMBER);
    p.complete(node, K::ArrayType);
}

/// `low .. high`; each bound is a signed integer or the name of a constant.
pub(super) fn subrange(p: &mut Parser) {
    let node = p.start();
    if bound_ahead(p) {
        bound(p);
    } else {
        p.error("expected the lower bound of a range");
    }
    p.expect(K::Range, "`..`");
    if bound_ahead(p) {
        bound(p);
    } else {
        p.error("expected the upper bound of a range");
    }
    p.complete(node, K::Subrange);
}

/// `REF_TO T`, `REFERENCE TO T` and `POINTER TO T`; `keyword_tokens` is how
/// many tokens the introducer takes.
fn reference_type(p: &mut Parser, keyword_tokens: usize) {
    let node = p.start();
    p.bump();
    if keyword_tokens == 2 {
        p.expect(K::To, "`TO`");
    }
    type_spec(p, MEMBER);
    p.complete(node, K::RefType);
}

/// `PARAMS ( n ) OF T`
fn params_type(p: &mut Parser) {
    let node = p.start();
    p.bump();
    if p.expect(K::LeftParen, "`(`") {
        integer_ref(p);
        close_group(p, K::RightParen, "`)`");
    }
    p.expect(K::Of, "`OF`");
    type_spec(p, MEMBER);
    p.complete(node, K::ParamsType);
}

/// `INT ( low .. high )`
fn subrange_type(p: &mut Parser) {
    let node = p.start();
    type_ref(p);
    p.bump();
    subrange(p);
    close_group(p, K::RightParen, "`)`");
    p.complete(node, K::SubrangeType);
}

/// `( A, B := 2, C ) [BYTE]`
fn enum_type(p: &mut Parser) {
    let node = p.start();
    p.bump();
    loop {
        enum_value(p);
        if !p.eat(K::Comma) {
            break;
        }
    }
    close_group(p, K::RightParen, "`)`");
    if p.at_any(ENUM_BASE_TYPES) {
        type_ref(p);
    }
    p.complete(node, K::EnumType);
}

fn enum_value(p: &mut Parser) {
    let node = p.start();
    if !super::common::declared_name(p, NameClass::Variable) {
        p.abandon(node);
        return;
    }
    if p.at(K::Assignment) {
        p.bump();
        expression(p);
    }
    p.complete(node, K::EnumValue);
}

/// `STRUCT member; ... END_STRUCT` and `UNION member; ... END_UNION`: one or
/// more members, each ending in `;`.
fn aggregate(p: &mut Parser, node_kind: K, closer: K) {
    let node = p.start();
    p.bump();
    let mut members = 0usize;
    while !p.at(closer) && !p.at_eof() && !p.at_any(BLOCK_END) {
        let before = p.position();
        member(p);
        members += 1;
        if p.position() == before {
            p.bump_as_error("expected a member");
        }
    }
    if members == 0 {
        p.error("expected at least one member");
    }
    if !p.eat(closer) {
        p.error("expected the end of the structure");
    }
    p.complete(node, node_kind);
}

fn member(p: &mut Parser) {
    let node = p.start();
    if !declared_name(p, NameClass::Plain) {
        skip_declaration(p);
        p.abandon(node);
        return;
    }
    if p.expect(K::Colon, "`:`") {
        type_spec(p, MEMBER);
        if p.at(K::Assignment) {
            initializer(p);
        }
    }
    p.complete(node, K::StructMember);
    item_terminator(p);
}

/// `TYPE name : type [:= value]; ... END_TYPE`
pub(super) fn type_block(p: &mut Parser) {
    let node = p.start();
    p.bump();
    let mut declarations = 0usize;
    // The legacy grammar's list of declarations needs one `;` even when it
    // holds no declaration.
    let lone_terminator = p.at(K::Semicolon) && p.nth_at(1, K::EndType);
    if lone_terminator {
        p.bump();
    }
    while !lone_terminator && !p.at_eof() && !p.at_any(BLOCK_END) {
        let before = p.position();
        type_declaration(p);
        declarations += 1;
        if p.position() == before {
            p.bump_as_error("expected a type declaration");
        }
    }
    if declarations == 0 && !lone_terminator {
        p.error("expected a type declaration");
    }
    super::common::close(p, K::EndType, "`END_TYPE`");
    p.complete(node, K::TypeBlock);
}

fn type_declaration(p: &mut Parser) {
    let node = p.start();
    if !declared_name(p, NameClass::Plain) {
        skip_declaration(p);
        p.abandon(node);
        return;
    }
    if p.expect(K::Colon, "`:`") {
        type_spec(p, DECLARED);
        if p.at(K::Assignment) {
            initializer(p);
        }
    }
    p.complete(node, K::TypeDecl);
    item_terminator(p);
}

/// The type a function, method or property returns: a string with an
/// optional length, or one name.
pub(super) fn return_type(p: &mut Parser) {
    if p.at(K::String) || p.at(K::WString) {
        string_type(p);
    } else {
        type_ref(p);
    }
}
