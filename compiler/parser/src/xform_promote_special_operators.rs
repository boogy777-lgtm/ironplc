//! Promote the CODESYS special-operator names from identifier tokens.
//!
//! The reference scanner reads a whole identifier and only then looks it up
//! in its operator table (`InternalScanner.ScanIdentifierOrOperator` →
//! `ScanIdentifierOperatorOrTrueFalse`), so `__NEW` is an operator while
//! `__NEW_ITEM` is an ordinary identifier. A logos token cannot make that
//! distinction -- a `#[token("__NEW")]` rule would split the longer name --
//! so the lexer always produces `Identifier` and this transform performs the
//! lookup, matching the whole text exactly.
//!
//! This is the "keyword promotion" transform category of
//! `specs/design/dialect-token-transforms.md`. It is not flag-gated: the
//! reference scanner recognizes these operators in every language
//! (`tables/special_operators.csv` marks them `Operator|AllLanguages`), and
//! the `__` prefix is IronPLC's reserved compiler namespace.
//!
//! Only the names that need their own grammar are promoted. `__SYSTEM` and
//! `__POOL` stay identifiers, because a qualified name already parses
//! (`__SYSTEM.x`); the remaining ST-visible names are ordinary calls.

use crate::token::{Token, TokenType};

pub fn apply(tokens: &mut [Token]) {
    for tok in tokens.iter_mut() {
        if tok.token_type != TokenType::Identifier {
            continue;
        }
        if let Some((_, promoted)) = crate::tokens::SPECIAL_OPERATORS
            .iter()
            .find(|(name, _)| tok.text.eq_ignore_ascii_case(name))
        {
            tok.token_type = promoted.clone();
        }
    }
}

#[cfg(test)]
mod tests {
    use dsl::core::SourceSpan;

    use super::apply;
    use crate::token::{Token, TokenType};

    fn mk_token(token_type: TokenType, text: &str) -> Token {
        Token {
            token_type,
            span: SourceSpan::default(),
            line: 1,
            col: 1,
            text: text.to_string(),
        }
    }

    #[test]
    fn apply_when_exact_name_then_promoted() {
        let mut tokens = vec![mk_token(TokenType::Identifier, "__NEW")];
        apply(&mut tokens);
        assert_eq!(tokens[0].token_type, TokenType::SpecialNew);
    }

    #[test]
    fn apply_when_case_differs_then_promoted() {
        let mut tokens = vec![mk_token(TokenType::Identifier, "__xadd")];
        apply(&mut tokens);
        assert_eq!(tokens[0].token_type, TokenType::SpecialXAdd);
        // The written spelling is preserved for rendering.
        assert_eq!(tokens[0].text, "__xadd");
    }

    #[test]
    fn apply_when_longer_identifier_then_left_alone() {
        let mut tokens = vec![
            mk_token(TokenType::Identifier, "__NEW_ITEM"),
            mk_token(TokenType::Identifier, "__XADD2"),
            mk_token(TokenType::Identifier, "my__NEW"),
        ];
        apply(&mut tokens);
        assert!(tokens.iter().all(|t| t.token_type == TokenType::Identifier));
    }

    #[test]
    fn apply_when_scope_prefix_then_left_alone() {
        // `__SYSTEM`/`__POOL` keep their identifier token: a qualified name
        // parses without a dedicated token.
        let mut tokens = vec![mk_token(TokenType::Identifier, "__SYSTEM")];
        apply(&mut tokens);
        assert_eq!(tokens[0].token_type, TokenType::Identifier);
    }
}
