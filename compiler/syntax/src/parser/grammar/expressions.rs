//! Expressions: a Pratt parser over the legacy grammar's operator levels.
//!
//! The levels, loosest first, are `OR`/`OR_ELSE`, `XOR`, `AND`/`AND_THEN`,
//! `=` `<>`, `<` `>` `<=` `>=`, `+` `-`, `*` `/` `MOD`, and `**`. Every level
//! is left-associative, including `**`, because the legacy grammar declares it
//! so. `-` and `NOT` are not Pratt prefix operators: as in the legacy grammar
//! they apply to one primary expression and bind tighter than every binary
//! operator, so `NOT a = b` is `(NOT a) = b` and `-a ** 2` is `(-a) ** 2`.
//!
//! Calls are accepted in exactly the shapes the legacy grammar accepts:
//! `name(...)`, `receiver.method(...)` and `THIS^.method(...)`.

use super::literals::literal;
use crate::parser::event::CompletedMarker;
use crate::parser::state::Parser;
use crate::syntax_kind::SyntaxKind as K;

/// Binding powers `(left, right)` of the binary operator at the cursor.
fn infix_power(p: &Parser) -> Option<(u8, u8)> {
    let kind = p.nth(0)?;
    if !p.at(kind) {
        return None;
    }
    let level: u8 = match kind {
        K::Or | K::OrElse => 1,
        K::Xor => 2,
        K::And | K::AndThen => 3,
        K::Equal | K::NotEqual => 4,
        K::Less | K::Greater | K::LessEqual | K::GreaterEqual => 5,
        K::Plus | K::Minus => 6,
        K::Star | K::Div | K::Mod => 7,
        K::Power => 8,
        _ => return None,
    };
    Some((level * 2, level * 2 + 1))
}

/// Parses an expression. Returns `None`, consuming nothing, when the next
/// token ends the enclosing construct instead of starting an expression; an
/// error has been recorded in that case.
pub(in crate::parser) fn expression(p: &mut Parser) -> Option<CompletedMarker> {
    expr_bp(p, 0)
}

fn expr_bp(p: &mut Parser, min_power: u8) -> Option<CompletedMarker> {
    p.guarded(
        |p| {
            let mut lhs = operand(p)?;
            while let Some((left, right)) = infix_power(p) {
                if left < min_power {
                    break;
                }
                let node = p.precede(lhs);
                p.bump();
                expr_bp(p, right);
                lhs = p.complete(node, K::BinaryExpr);
            }
            Some(lhs)
        },
        |p| p.skip_expression(),
    )
}

/// An operand: one primary, optionally under a unary `-` or `NOT`.
fn operand(p: &mut Parser) -> Option<CompletedMarker> {
    let unary = p.at(K::Minus) || (p.at(K::Not) && !not_is_call(p));
    if !unary {
        return postfix_primary(p);
    }
    let node = p.start();
    p.bump();
    postfix_primary(p);
    Some(p.complete(node, K::UnaryExpr))
}

/// `NOT(...)` is the function `NOT` rather than the operator when the
/// parentheses hold an argument list that is not one parenthesised
/// expression: empty, or with a top-level `,`, `:=` or `=>`.
fn not_is_call(p: &Parser) -> bool {
    if !p.nth_at(1, K::LeftParen) {
        return false;
    }
    let mut depth = 1usize;
    let mut n = 2;
    while let Some(kind) = p.nth(n) {
        match kind {
            K::LeftParen | K::LeftBracket => depth += 1,
            K::RightParen | K::RightBracket => {
                depth -= 1;
                if depth == 0 {
                    return n == 2;
                }
            }
            K::Comma | K::Assignment | K::RightArrow if depth == 1 => return true,
            _ => {}
        }
        n += 1;
    }
    false
}

/// A primary expression and the `^` dereferences that follow it.
fn postfix_primary(p: &mut Parser) -> Option<CompletedMarker> {
    let mut lhs = primary(p)?;
    while p.at(K::Caret) {
        let node = p.precede(lhs);
        p.bump();
        lhs = p.complete(node, K::DerefExpr);
    }
    Some(lhs)
}

fn primary(p: &mut Parser) -> Option<CompletedMarker> {
    if let Some(done) = literal(p) {
        return Some(done);
    }
    if p.at(K::Ref) && p.nth_at(1, K::LeftParen) {
        return Some(ref_expr(p));
    }
    if p.at(K::Null) {
        let node = p.start();
        p.bump();
        return Some(p.complete(node, K::NullLiteral));
    }
    if p.at(K::LeftParen) {
        return Some(paren_expr(p));
    }
    if special_type_operator_ahead(p) {
        return Some(special_operator(p));
    }
    if let Some(done) = call(p, true) {
        return Some(done);
    }
    if let Some(done) = variable(p) {
        return Some(done);
    }
    p.error("expected an expression");
    p.skip_expression()
}

fn paren_expr(p: &mut Parser) -> CompletedMarker {
    let node = p.start();
    p.bump();
    expression(p);
    close_group(p, K::RightParen, "`)`");
    p.complete(node, K::ParenExpr)
}

/// Consumes the `closer` of a bracketed group. A malformed group is skipped
/// up to its closer first, so one bad element does not hide the rest.
pub(in crate::parser) fn close_group(p: &mut Parser, closer: K, what: &str) {
    if !p.at(closer) {
        p.error(&format!("expected {what}"));
        p.skip_expression();
    }
    p.eat(closer);
}

pub(in crate::parser) fn ref_expr(p: &mut Parser) -> CompletedMarker {
    let node = p.start();
    p.bump_n(2);
    if variable(p).is_none() {
        p.error("expected a variable");
    }
    close_group(p, K::RightParen, "`)`");
    p.complete(node, K::RefExpr)
}

fn special_type_operator_ahead(p: &Parser) -> bool {
    (p.nth_is_word(0, "__NEW") || p.nth_is_word(0, "__TYPEOF")) && p.nth_at(1, K::LeftParen)
}

/// `__NEW(T)`, `__NEW(T, count)` and `__TYPEOF(T)`: these take a type where a
/// call takes an expression.
fn special_operator(p: &mut Parser) -> CompletedMarker {
    let node = p.start();
    let takes_count = p.nth_is_word(0, "__NEW");
    p.bump_n(2);
    type_ref(p);
    if takes_count && p.eat(K::Comma) {
        expression(p);
    }
    close_group(p, K::RightParen, "`)`");
    p.complete(node, K::SpecialOpExpr)
}

const TYPE_KEYWORDS: &[K] = &[
    K::Sint,
    K::Int,
    K::Dint,
    K::Lint,
    K::Usint,
    K::Uint,
    K::Udint,
    K::Ulint,
    K::Real,
    K::Lreal,
    K::Date,
    K::Ldate,
    K::TimeOfDay,
    K::Ltod,
    K::DateAndTime,
    K::Ldt,
    K::Bool,
    K::Byte,
    K::Word,
    K::Dword,
    K::Lword,
    K::Bit,
    K::String,
    K::WString,
    K::Time,
    K::Ltime,
    K::Any,
    K::AnyDerived,
    K::AnyElementary,
    K::AnyMagnitude,
    K::AnyNum,
    K::AnyReal,
    K::AnyInt,
    K::AnyBit,
    K::AnyString,
    K::AnyDate,
];

/// A type name: an elementary or generic type keyword, or a name. Returns
/// false, having reported an error and consumed nothing, when there is none.
pub(in crate::parser) fn type_ref(p: &mut Parser) -> bool {
    if p.at_any(TYPE_KEYWORDS) || p.name_at(0) {
        let node = p.start();
        p.bump();
        p.complete(node, K::TypeRef);
        true
    } else {
        p.error("expected a type name");
        false
    }
}

/// A name token as a node.
pub(in crate::parser) fn name_ref(p: &mut Parser) -> CompletedMarker {
    let node = p.start();
    p.bump();
    p.complete(node, K::NameRef)
}

/// Words that name a function although they are keywords or special
/// operators.
fn operator_function_ahead(p: &Parser) -> bool {
    p.at_any(&[K::Mod, K::And, K::Or, K::Xor, K::Not])
        || ["__DELETE", "__ISVALIDREF", "__XADD"]
            .iter()
            .any(|word| p.nth_is_word(0, word))
}

/// True when a call of an array element starts at the cursor: `name[i](`, or
/// `name[i][j](`. Only a statement calls an instance, so only a statement
/// admits it. The subscripts are skipped by their brackets, whatever they hold.
pub(in crate::parser) fn element_call_ahead(p: &Parser) -> bool {
    if !(p.variable_name_at(0) && p.nth_at(1, K::LeftBracket)) {
        return false;
    }
    let mut n = 1;
    loop {
        let mut depth = 0usize;
        loop {
            match p.nth(n) {
                Some(K::LeftBracket) => depth += 1,
                Some(K::RightBracket) => {
                    depth -= 1;
                    if depth == 0 {
                        break;
                    }
                }
                Some(K::Semicolon) | None => return false,
                Some(_) => {}
            }
            n += 1;
        }
        n += 1;
        if !p.nth_at(n, K::LeftBracket) {
            return p.nth_at(n, K::LeftParen);
        }
    }
}

/// True when a call starts at the cursor: `name(`, `receiver.method(`,
/// `THIS^.method(` or, in a statement, `name[i](`. `operators` also admits the
/// operator words used as function names (`MOD(a, b)`), which a call statement
/// does not, and which is how an expression is told from a statement.
pub(in crate::parser) fn call_ahead(p: &Parser, operators: bool) -> bool {
    let function =
        (p.name_at(0) || (operators && operator_function_ahead(p))) && p.nth_at(1, K::LeftParen);
    let method =
        p.name_at(0) && p.nth_at(1, K::Period) && p.name_at(2) && p.nth_at(3, K::LeftParen);
    let self_method = p.at_any(&[K::This, K::Super])
        && p.nth_at(1, K::Caret)
        && p.nth_at(2, K::Period)
        && p.name_at(3)
        && p.nth_at(4, K::LeftParen);
    function || method || self_method || (!operators && element_call_ahead(p))
}

/// A call expression, when one starts at the cursor.
pub(in crate::parser) fn call(p: &mut Parser, operators: bool) -> Option<CompletedMarker> {
    if !call_ahead(p, operators) {
        return None;
    }
    let callee = if !operators && element_call_ahead(p) {
        variable(p)?
    } else if p.at_any(&[K::This, K::Super]) {
        self_ref(p)
    } else {
        name_ref(p)
    };
    let callee = if p.at(K::Period) {
        let member = p.precede(callee);
        p.bump_n(2);
        p.complete(member, K::FieldExpr)
    } else {
        callee
    };
    let node = p.precede(callee);
    arg_list(p);
    Some(p.complete(node, K::CallExpr))
}

fn self_ref(p: &mut Parser) -> CompletedMarker {
    let node = p.start();
    p.bump_n(2);
    p.complete(node, K::SelfRefExpr)
}

/// `( [argument {, argument}] )`. Each argument is positional, `name := value`
/// or an output binding `[NOT] name => variable`.
pub(in crate::parser) fn arg_list(p: &mut Parser) {
    let node = p.start();
    p.bump();
    if !p.at(K::RightParen) && !p.at_eof() {
        loop {
            let before = p.position();
            argument(p);
            if p.position() == before || !p.eat(K::Comma) {
                break;
            }
        }
    }
    close_group(p, K::RightParen, "`)`");
    p.complete(node, K::ArgList);
}

fn output_argument_ahead(p: &Parser) -> bool {
    let name = usize::from(p.at(K::Not));
    p.variable_name_at(name) && p.nth_at(name + 1, K::RightArrow)
}

fn argument(p: &mut Parser) {
    let node = p.start();
    if output_argument_ahead(p) {
        p.eat(K::Not);
        name_ref(p);
        p.bump();
        if variable(p).is_none() {
            p.error("expected a variable");
        }
        p.complete(node, K::OutputArg);
    } else if p.variable_name_at(0) && p.nth_at(1, K::Assignment) {
        name_ref(p);
        p.bump();
        expression(p);
        p.complete(node, K::NamedArg);
    } else if expression(p).is_some() {
        p.complete(node, K::PositionalArg);
    } else {
        p.abandon(node);
    }
}

/// A variable: a name, `THIS^`/`SUPER^`, `__CURRENTTASK` or a direct address,
/// followed by any chain of `.field`, `.bit`, `.%Xn`, `[index, ...]` and
/// `^`. A `^` belongs to the chain only when a `[` or `.` follows it; a
/// trailing one is a dereference of the whole expression.
pub(in crate::parser) fn variable(p: &mut Parser) -> Option<CompletedMarker> {
    let mut lhs = if p.at_any(&[K::This, K::Super]) && p.nth_at(1, K::Caret) {
        self_ref(p)
    } else if p.at(K::DirectAddress) {
        let node = p.start();
        p.bump();
        return Some(p.complete(node, K::DirectAddressExpr));
    } else if p.variable_name_at(0) || p.nth_is_word(0, "__CURRENTTASK") {
        name_ref(p)
    } else {
        return None;
    };
    loop {
        let element = if p.at(K::Period) {
            match p.nth(1) {
                Some(K::IntegerLit) => K::BitAccessExpr,
                Some(K::PartialAccess) => K::PartialAccessExpr,
                _ if p.name_at(1) => K::FieldExpr,
                _ => break,
            }
        } else if p.at(K::LeftBracket) {
            K::IndexExpr
        } else if p.at(K::Caret) && (p.nth_at(1, K::LeftBracket) || p.nth_at(1, K::Period)) {
            K::DerefExpr
        } else {
            break;
        };
        let node = p.precede(lhs);
        match element {
            K::IndexExpr => {
                p.bump();
                loop {
                    if expression(p).is_none() || !p.eat(K::Comma) {
                        break;
                    }
                }
                close_group(p, K::RightBracket, "`]`");
            }
            K::DerefExpr => p.bump(),
            _ => p.bump_n(2),
        }
        lhs = p.complete(node, element);
    }
    Some(lhs)
}
