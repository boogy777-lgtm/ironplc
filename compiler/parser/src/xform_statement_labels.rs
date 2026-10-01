//! Promote the identifier a statement label starts with to [`TokenType::Label`].
//!
//! A statement label is `name :` in statement position (`JMP name;` targets
//! it). The PEG parser dispatches on `token_type` alone, so the flag that
//! enables jump statements has to reach the grammar through the token stream:
//! this transform marks the label name, the grammar matches `Label` `:`, and
//! no other rule can accidentally accept a statement label.
//!
//! `Identifier` `:` is not always a label. The reference statement parser
//! (`JumpStatementParser.cs` callers) draws the same line: a name followed by
//! `:` is a label only when the parser is not in a declaration and not inside
//! a `CASE` statement (`TryParseDeclarationOrLabel`). This transform applies
//! that rule over tokens:
//!
//! * inside a declaration region (`VAR`-family ... `END_VAR`, `TYPE ...
//!   END_TYPE`, `STRUCT ... END_STRUCT`) the run is a declaration;
//! * inside a `CASE` statement it is a case label, the next branch's selector;
//! * directly after a declaration's introducing keyword (`FUNCTION f : INT`)
//!   it is the declared name.
//!
//! Everything else is statement position. The transform is deliberately
//! conservative: a `name :` it does not recognize stays an identifier, so the
//! statement is a parse error rather than a declaration silently re-read as a
//! label.

use crate::options::CompilerOptions;
use crate::token::{Token, TokenType};

/// Token types that carry no meaning for context tracking. Pragmas are
/// metadata that may sit between a name and its colon, so they are skipped
/// like whitespace.
fn is_trivia(tt: &TokenType) -> bool {
    matches!(
        tt,
        TokenType::Whitespace
            | TokenType::Newline
            | TokenType::Comment
            | TokenType::DocComment
            | TokenType::Pragma
    )
}

/// Returns the `END_*` keyword that closes the declaration region a keyword
/// opens, if it opens one.
fn region_closer(tt: &TokenType) -> Option<TokenType> {
    match tt {
        TokenType::Var
        | TokenType::VarInput
        | TokenType::VarOutput
        | TokenType::VarInOut
        | TokenType::VarTemp
        | TokenType::VarExternal
        | TokenType::VarAccess
        | TokenType::VarConfig
        | TokenType::VarGlobal
        | TokenType::VarStat
        | TokenType::VarInst
        | TokenType::VarGeneric => Some(TokenType::EndVar),
        TokenType::Type => Some(TokenType::EndType),
        TokenType::Struct => Some(TokenType::EndStruct),
        _ => None,
    }
}

/// Keywords that introduce a declaration whose name is followed by `:` —
/// `FUNCTION f : INT`, `METHOD m : INT`, `STEP s:`, and the like. The name is
/// not a label.
fn introduces_declared_name(tt: &TokenType) -> bool {
    matches!(
        tt,
        TokenType::Function
            | TokenType::FunctionBlock
            | TokenType::Program
            | TokenType::Method
            | TokenType::Property
            | TokenType::Interface
            | TokenType::Action
            | TokenType::Transition
            | TokenType::Step
            | TokenType::InitialStep
    )
}

/// Contextual qualifier words that may sit between a declaration introducer
/// and the declared name (`METHOD PUBLIC FINAL m : INT`). The grammar reads
/// them via `contextual_keyword`, so they stay `Identifier` tokens; a
/// case-insensitive spelling match mirrors that rule.
const QUALIFIER_WORDS: [&str; 8] = [
    "PUBLIC",
    "PRIVATE",
    "PROTECTED",
    "INTERNAL",
    "FINAL",
    "ABSTRACT",
    "OVERRIDE",
    "OVERLOAD",
];

fn is_qualifier_word(text: &str) -> bool {
    QUALIFIER_WORDS.iter().any(|w| text.eq_ignore_ascii_case(w))
}

/// Marks every statement label in the token stream as [`TokenType::Label`].
///
/// Only runs when `options.allow_jump_statement` is set; otherwise the stream
/// is returned unchanged and `name :` keeps its current meaning (a
/// declaration, a case label, or a syntax error).
pub fn apply(tokens: &mut [Token], options: &CompilerOptions) {
    if !options.allow_jump_statement {
        return;
    }

    // The `END_*` keyword that closes each open declaration region.
    let mut regions: Vec<TokenType> = Vec::new();
    let mut case_depth: u32 = 0;
    // Whether the preceding significant tokens are a declaration introducer
    // followed only by qualifier words — then the next identifier is the
    // declared name, not a statement label.
    let mut after_decl_introducer = false;

    for index in 0..tokens.len() {
        let token_type = tokens[index].token_type.clone();

        if let Some(closer) = region_closer(&token_type) {
            regions.push(closer);
        } else if regions.last() == Some(&token_type) {
            regions.pop();
        }
        match token_type {
            TokenType::Case => case_depth += 1,
            TokenType::EndCase if case_depth > 0 => case_depth -= 1,
            _ => {}
        }

        let is_qualifier =
            token_type == TokenType::Identifier && is_qualifier_word(&tokens[index].text);

        if token_type == TokenType::Identifier
            && !is_qualifier
            && regions.is_empty()
            && case_depth == 0
            && !after_decl_introducer
            && followed_by_colon(tokens, index)
        {
            tokens[index].token_type = TokenType::Label;
        }

        if introduces_declared_name(&token_type) {
            after_decl_introducer = true;
        } else if !is_trivia(&token_type) && !is_qualifier {
            after_decl_introducer = false;
        }
    }
}

/// Returns true when the next meaningful token after `index` is a `:`.
fn followed_by_colon(tokens: &[Token], index: usize) -> bool {
    for tok in &tokens[index + 1..] {
        if is_trivia(&tok.token_type) {
            continue;
        }
        return tok.token_type == TokenType::Colon;
    }
    false
}

#[cfg(test)]
mod tests {
    use super::apply;
    use crate::options::CompilerOptions;
    use crate::token::TokenType;

    fn opts() -> CompilerOptions {
        CompilerOptions {
            allow_jump_statement: true,
            ..CompilerOptions::default()
        }
    }

    /// The token types of a source, with trivia dropped.
    fn types(source: &str, options: &CompilerOptions) -> Vec<TokenType> {
        let (mut tokens, diagnostics) =
            crate::lexer::tokenize(source, &dsl::core::FileId::default(), 0, 0);
        assert!(diagnostics.is_empty(), "{diagnostics:?}");
        apply(&mut tokens, options);
        tokens
            .into_iter()
            .map(|t| t.token_type)
            .filter(|t| {
                !matches!(
                    t,
                    TokenType::Whitespace | TokenType::Newline | TokenType::Comment
                )
            })
            .collect()
    }

    #[test]
    fn apply_when_flag_off_then_identifier_unchanged() {
        let types = types(
            "PROGRAM p lbl: x := 1; END_PROGRAM",
            &CompilerOptions::default(),
        );
        assert!(!types.contains(&TokenType::Label));
    }

    #[test]
    fn apply_when_label_in_statement_position_then_promoted() {
        let types = types("PROGRAM p x := 1; lbl: x := 2; END_PROGRAM", &opts());
        let label = types.iter().position(|t| *t == TokenType::Label);
        assert!(label.is_some(), "{types:?}");
        assert_eq!(types[label.unwrap() + 1], TokenType::Colon);
    }

    #[test]
    fn apply_when_label_is_first_statement_then_promoted() {
        let types = types("PROGRAM p lbl: x := 1; END_PROGRAM", &opts());
        assert!(types.contains(&TokenType::Label), "{types:?}");
    }

    #[test]
    fn apply_when_declaration_in_var_block_then_not_a_label() {
        let types = types(
            "PROGRAM p VAR x : INT; y : INT; END_VAR lbl: x := 1; END_PROGRAM",
            &opts(),
        );
        let labels: Vec<usize> = types
            .iter()
            .enumerate()
            .filter(|(_, t)| **t == TokenType::Label)
            .map(|(i, _)| i)
            .collect();
        // Only `lbl` is a label; `x` and `y` are declarations.
        assert_eq!(labels.len(), 1, "{types:?}");
    }

    #[test]
    fn apply_when_declaration_in_var_stat_block_then_not_a_label() {
        for block in ["VAR_STAT", "VAR_INST", "VAR_GENERIC"] {
            let types = types(
                &format!("PROGRAM p {block} x : INT; END_VAR lbl: x := 1; END_PROGRAM"),
                &opts(),
            );
            let labels: Vec<usize> = types
                .iter()
                .enumerate()
                .filter(|(_, t)| **t == TokenType::Label)
                .map(|(i, _)| i)
                .collect();
            // Only `lbl` is a label; `x` is a declaration.
            assert_eq!(labels.len(), 1, "{types:?}");
        }
    }

    #[test]
    fn apply_when_qualified_method_header_then_not_a_label() {
        let types = types(
            "FUNCTION_BLOCK fb METHOD PUBLIC FINAL m : INT m := 1; END_METHOD END_FUNCTION_BLOCK",
            &opts(),
        );
        assert!(!types.contains(&TokenType::Label), "{types:?}");
    }

    #[test]
    fn apply_when_declaration_in_type_block_then_not_a_label() {
        let types = types("TYPE T : STRUCT a : INT; END_STRUCT END_TYPE", &opts());
        assert!(!types.contains(&TokenType::Label), "{types:?}");
    }

    #[test]
    fn apply_when_pou_header_then_not_a_label() {
        let types = types("FUNCTION f : INT f := 1; END_FUNCTION", &opts());
        assert!(!types.contains(&TokenType::Label), "{types:?}");
    }

    #[test]
    fn apply_when_method_and_step_headers_then_not_labels() {
        let method = types(
            "FUNCTION_BLOCK fb METHOD m : INT m := 1; END_METHOD END_FUNCTION_BLOCK",
            &opts(),
        );
        assert!(!method.contains(&TokenType::Label), "{method:?}");

        let step = types(
            "PROGRAM p INITIAL_STEP Start: END_STEP STEP Idle: END_STEP END_PROGRAM",
            &opts(),
        );
        assert!(!step.contains(&TokenType::Label), "{step:?}");
    }

    #[test]
    fn apply_when_case_selector_then_not_a_label() {
        let types = types(
            "PROGRAM p CASE x OF Red: x := 1; Blue: x := 2; END_CASE END_PROGRAM",
            &opts(),
        );
        assert!(!types.contains(&TokenType::Label), "{types:?}");
    }

    #[test]
    fn apply_when_result_assignment_in_case_then_selector_still_works() {
        // The `:` after `END_CASE` (the statement terminator) must not leak
        // case tracking into the statements that follow it.
        let types = types(
            "PROGRAM p CASE x OF 1: x := 1; END_CASE lbl: x := 2; END_PROGRAM",
            &opts(),
        );
        assert!(types.contains(&TokenType::Label), "{types:?}");
    }

    #[test]
    fn apply_when_name_at_end_of_stream_then_not_a_label() {
        let types = types("PROGRAM p x", &opts());
        assert!(!types.contains(&TokenType::Label), "{types:?}");
    }
}
