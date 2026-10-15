//! Tests that the lexer produces the expected token types for source text.

use super::*;
use logos::Logos;

#[test]
fn tokenize_when_time_alone_then_produces_time_token() {
    let mut lex = TokenType::lexer("TIME");
    let tok = lex.next().unwrap().unwrap();
    assert_eq!(tok, TokenType::Time);
}

#[test]
fn tokenize_when_time_of_day_then_produces_time_of_day_token() {
    let mut lex = TokenType::lexer("TIME_OF_DAY");
    let tok = lex.next().unwrap().unwrap();
    assert_eq!(tok, TokenType::TimeOfDay);
}

#[test]
fn tokenize_when_time_followed_by_semicolon_then_produces_time_token() {
    let mut lex = TokenType::lexer("TIME;");
    let tok = lex.next().unwrap().unwrap();
    assert_eq!(tok, TokenType::Time);
}

#[test]
fn tokenize_when_scientific_no_decimal_then_produces_floating_point() {
    let mut lex = TokenType::lexer("2E-3");
    let tok = lex.next().unwrap().unwrap();
    assert_eq!(tok, TokenType::FloatingPoint);
}

#[test]
fn tokenize_when_scientific_positive_exponent_then_produces_floating_point() {
    let mut lex = TokenType::lexer("1.5E+2");
    let tok = lex.next().unwrap().unwrap();
    assert_eq!(tok, TokenType::FloatingPoint);
}

#[test]
fn tokenize_when_scientific_no_sign_no_decimal_then_produces_floating_point() {
    let mut lex = TokenType::lexer("2E3");
    let tok = lex.next().unwrap().unwrap();
    assert_eq!(tok, TokenType::FloatingPoint);
}

#[test]
fn tokenize_when_ref_to_then_produces_single_ref_to_token() {
    let mut lex = TokenType::lexer("REF_TO");
    let tok = lex.next().unwrap().unwrap();
    assert_eq!(tok, TokenType::RefTo);
    assert!(lex.next().is_none());
}

#[test]
fn tokenize_when_ref_to_lowercase_then_produces_ref_to_token() {
    let mut lex = TokenType::lexer("ref_to");
    let tok = lex.next().unwrap().unwrap();
    assert_eq!(tok, TokenType::RefTo);
}

#[test]
fn tokenize_when_ref_alone_then_produces_ref_token() {
    let mut lex = TokenType::lexer("REF");
    let tok = lex.next().unwrap().unwrap();
    assert_eq!(tok, TokenType::Ref);
}

#[test]
fn tokenize_when_ref_paren_then_produces_ref_and_left_paren() {
    let mut lex = TokenType::lexer("REF(");
    let tok1 = lex.next().unwrap().unwrap();
    let tok2 = lex.next().unwrap().unwrap();
    assert_eq!(tok1, TokenType::Ref);
    assert_eq!(tok2, TokenType::LeftParen);
}

#[test]
fn tokenize_when_null_then_produces_null_token() {
    let mut lex = TokenType::lexer("NULL");
    let tok = lex.next().unwrap().unwrap();
    assert_eq!(tok, TokenType::Null);
}

#[test]
fn tokenize_when_null_lowercase_then_produces_null_token() {
    let mut lex = TokenType::lexer("null");
    let tok = lex.next().unwrap().unwrap();
    assert_eq!(tok, TokenType::Null);
}

#[test]
fn tokenize_when_caret_then_produces_caret_token() {
    let mut lex = TokenType::lexer("^");
    let tok = lex.next().unwrap().unwrap();
    assert_eq!(tok, TokenType::Caret);
}
