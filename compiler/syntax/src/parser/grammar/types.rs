//! Type specifications and `TYPE` declarations.
//!
//! One rule, [`type_spec`], reads what follows the `:` of a declaration,
//! wherever it is: a variable, a structure member, a type alias, a function's
//! return type. The legacy grammar writes a separate rule per position; here
//! the position is a row of the table in `positions`, which says which kinds
//! of type it accepts and what initial value each takes.
//!
//! The forms are `ARRAY [ranges] OF T`, `ARRAY [*] OF T`, `STRING[n]`,
//! `WSTRING(n)`, `REF_TO T`, `REFERENCE TO T`, `POINTER TO T`,
//! `PARAMS(n) OF T`, an inline enumeration `(A, B)`, in a `TYPE` declaration
//! also with values and a base type `(A, B := 2) BYTE`, a subrange
//! `INT(1..10)`, `STRUCT ... END_STRUCT`, `UNION ... END_UNION`, and a type
//! name. Their gating (`REF_TO`, `PARAMS`, `UNION`, ...) is the keyword table
//! in `ParseOptions`; whether a form is enabled by a flag that is not about a
//! keyword (`STRING(n)`, `ARRAY[*]`) is a gate, see `parser::gates`.

use super::common::{
    close, declared_name, integer_ref, item_terminator, skip_declaration, terminator, NameClass,
};
use super::control::{bound, bound_ahead};
use super::expressions::{arg_list, close_group, type_ref};
use super::initializers::initializer;
use super::literals::literal;
use super::positions::{Context, Spec, COUNTED, DECLARED, ELEMENT, MEMBER, TARGET};
use crate::parser::recovery::BLOCK_END;
use crate::parser::state::Parser;
use crate::syntax_kind::SyntaxKind as K;

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

/// A type specification in the position `context`. A malformed one is
/// reported and left in place where it stops, so the declaration around it
/// can still end. A kind of type the position does not accept is parsed and
/// reported at its first token.
pub(super) fn type_spec(p: &mut Parser, context: &Context) -> Spec {
    let start = p.current_range();
    let spec = p.guarded(
        |p| specification(p, context),
        |p| {
            if !p.at_eof() {
                let node = p.start();
                p.bump();
                p.complete(node, K::ErrorNode);
            }
            Spec::Missing
        },
    );
    if !context.accepts(spec) {
        p.error_at(start, "this kind of type is not allowed here");
    }
    spec
}

fn specification(p: &mut Parser, context: &Context) -> Spec {
    let Some(kind) = p.nth(0).filter(|kind| p.at(*kind)) else {
        return named(p, context);
    };
    match kind {
        K::Array => {
            array_type(p);
            Spec::Array
        }
        K::String | K::WString => {
            if string_type(p) {
                Spec::String
            } else {
                Spec::Elementary
            }
        }
        K::RefTo => {
            reference_type(p, 1);
            Spec::Reference
        }
        K::Reference | K::Pointer => {
            reference_type(p, 2);
            Spec::Reference
        }
        K::Params => {
            params_type(p);
            Spec::Params
        }
        K::LeftParen => {
            enum_type(p, context.declares);
            Spec::Enumeration
        }
        K::Struct if context.declares => {
            aggregate(p, K::StructType, K::EndStruct);
            Spec::Struct
        }
        K::Union if context.declares => {
            aggregate(p, K::UnionType, K::EndUnion);
            Spec::Union
        }
        kind if INTEGER_TYPES.contains(&kind) && p.nth_at(1, K::LeftParen) => {
            subrange_type(p);
            Spec::Subrange
        }
        _ => named(p, context),
    }
}

/// A type name, and the arguments of a function block instance that follow
/// it where the position allows them.
fn named(p: &mut Parser, context: &Context) -> Spec {
    let declared = p.name_at(0);
    let call = context.accepts(Spec::Call) && declared;
    if !type_ref(p) {
        return Spec::Missing;
    }
    if call && p.at(K::LeftParen) {
        arg_list(p);
        return Spec::Call;
    }
    if declared {
        Spec::Named
    } else {
        Spec::Elementary
    }
}

/// `STRING`, `WSTRING`, optionally with a length in `[ ]` or `( )`. Returns true
/// when there is a length.
pub(super) fn string_type(p: &mut Parser) -> bool {
    let node = p.start();
    p.bump();
    let closer = match p.nth(0) {
        Some(K::LeftBracket) => Some((K::RightBracket, "`]`")),
        Some(K::LeftParen) => Some((K::RightParen, "`)`")),
        _ => None,
    };
    let has_length = closer.is_some();
    if let Some((closer, what)) = closer {
        p.bump();
        integer_ref(p);
        close_group(p, closer, what);
    }
    p.complete(node, K::StringType);
    has_length
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
    type_spec(p, &ELEMENT);
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
    type_spec(p, &TARGET);
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
    type_spec(p, &COUNTED);
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

/// `( A, B, C )`; in a `TYPE` declaration also `( A, B := 2 ) BYTE`.
fn enum_type(p: &mut Parser, declares: bool) {
    let node = p.start();
    p.bump();
    loop {
        enum_value(p, declares);
        if !p.eat(K::Comma) {
            break;
        }
    }
    close_group(p, K::RightParen, "`)`");
    if p.at_any(ENUM_BASE_TYPES) {
        if !declares {
            p.error("a base type is part of a type declaration");
        }
        type_ref(p);
    }
    p.complete(node, K::EnumType);
}

fn enum_value(p: &mut Parser, declares: bool) {
    let node = p.start();
    if !declared_name(p, NameClass::Variable) {
        p.abandon(node);
        return;
    }
    if p.at(K::Assignment) {
        if !declares {
            p.error("a value is part of a type declaration");
        }
        p.bump();
        signed_integer(p);
    }
    p.complete(node, K::EnumValue);
}

/// An integer with an optional sign that touches it.
fn signed_integer(p: &mut Parser) {
    let signed = p.at_any(&[K::Plus, K::Minus]) && p.adjacent(0) && p.nth_at(1, K::IntegerLit);
    if p.at(K::IntegerLit) || signed {
        literal(p);
    } else {
        p.error("expected an integer");
    }
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
        let spec = type_spec(p, &MEMBER);
        if p.at(K::Assignment) {
            initializer(p, MEMBER.initial(spec));
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
    close(p, K::EndType, "`END_TYPE`");
    p.complete(node, K::TypeBlock);
}

fn type_declaration(p: &mut Parser) {
    let node = p.start();
    if !declared_name(p, NameClass::Plain) {
        skip_declaration(p);
        p.abandon(node);
        return;
    }
    let mut spec = Spec::Missing;
    if p.expect(K::Colon, "`:`") {
        spec = type_spec(p, &DECLARED);
        if p.at(K::Assignment) {
            initializer(p, DECLARED.initial(spec));
        }
    }
    p.complete(node, K::TypeDecl);
    // The legacy pipeline supplies the `;` after `END_STRUCT` when the dialect
    // allows missing semicolons.
    terminator(p, spec == Spec::Struct);
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
