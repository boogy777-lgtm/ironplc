//! Error recovery: synchronisation sets and the primitives built on them.
//!
//! Recovery never loses a token: input a rule cannot use is wrapped in an
//! [`SyntaxKind::ErrorNode`] so the tree still owns every byte (design: parse-tree
//! S0 audit, finding F9). Every primitive here either consumes at least one
//! token or leaves the position alone, and the loops that call them check
//! which of the two happened, so no loop can spin without progress.

use super::event::CompletedMarker;
use super::state::Parser;
use crate::syntax_kind::SyntaxKind as K;
use crate::syntax_kind::SyntaxKind;

/// Keywords that open a variable block.
pub(crate) const VAR_OPENERS: &[SyntaxKind] = &[
    K::Var,
    K::VarInput,
    K::VarOutput,
    K::VarInOut,
    K::VarTemp,
    K::VarExternal,
    K::VarAccess,
    K::VarConfig,
    K::VarGlobal,
    K::VarStat,
    K::VarInst,
    K::VarGeneric,
];

/// Keywords that open a declaration at the top of a file, or inside a
/// namespace.
pub(crate) const DECLARATION_START: &[SyntaxKind] = &[
    K::Program,
    K::Function,
    K::FunctionBlock,
    K::Type,
    K::Configuration,
    K::Interface,
    K::Namespace,
    K::VarGlobal,
];

/// Keywords that open a member of a function block, after its body.
pub(crate) const MEMBER_START: &[SyntaxKind] = &[K::Method, K::Property];

/// The `END_*` keywords that close a declaration. A declaration that is
/// missing its own closer stops at another declaration's closer and leaves it
/// for the enclosing declaration, instead of consuming it.
pub(crate) const DECLARATION_END: &[SyntaxKind] = &[
    K::EndProgram,
    K::EndFunction,
    K::EndFunctionBlock,
    K::EndMethod,
    K::EndProperty,
    K::EndGet,
    K::EndSet,
    K::EndAction,
    K::EndTransition,
    K::EndStep,
    K::EndType,
    K::EndConfiguration,
    K::EndResource,
    K::EndNamespace,
    K::EndInterface,
];

/// Keywords that close or divide a block. A statement list ends at any of
/// them, and recovery never consumes one on its own: whichever enclosing
/// construct owns the keyword gets to see it. The keywords that open a
/// declaration or a variable block are in the set too, so a block left open
/// before the next declaration ends there instead of swallowing it.
pub(crate) const BLOCK_END: &[SyntaxKind] = &[
    K::EndIf,
    K::EndCase,
    K::EndFor,
    K::EndWhile,
    K::EndRepeat,
    K::EndTry,
    K::Else,
    K::Elsif,
    K::Until,
    K::Catch,
    K::Finally,
    K::EndProgram,
    K::EndFunction,
    K::EndFunctionBlock,
    K::EndMethod,
    K::EndProperty,
    K::EndGet,
    K::EndSet,
    K::EndAction,
    K::EndTransition,
    K::EndVar,
    K::EndType,
    K::EndStruct,
    K::EndUnion,
    K::EndConfiguration,
    K::EndResource,
    K::EndNamespace,
    K::EndInterface,
    K::EndStep,
    K::Program,
    K::Function,
    K::FunctionBlock,
    K::Type,
    K::Configuration,
    K::Interface,
    K::Namespace,
    K::Method,
    K::Property,
    K::Var,
    K::VarInput,
    K::VarOutput,
    K::VarInOut,
    K::VarTemp,
    K::VarExternal,
    K::VarAccess,
    K::VarConfig,
    K::VarGlobal,
    K::VarStat,
    K::VarInst,
    K::VarGeneric,
];

/// Keywords that begin a statement; recovery inside a malformed statement
/// stops before one so the next statement is parsed normally.
pub(crate) const STATEMENT_START: &[SyntaxKind] = &[
    K::If,
    K::Case,
    K::For,
    K::While,
    K::Repeat,
    K::Return,
    K::Exit,
    K::Continue,
    K::Try,
    K::Throw,
    K::Jmp,
    K::Calc,
    K::Wait,
];

/// Tokens at which a malformed expression stops, when not nested inside
/// brackets the expression itself opened: the separators and block words an
/// enclosing construct is waiting for.
const EXPRESSION_STOP: &[SyntaxKind] = &[
    K::Comma,
    K::RightParen,
    K::RightBracket,
    K::Then,
    K::Do,
    K::Of,
    K::To,
    K::By,
    K::Colon,
    K::Assignment,
    K::RightArrow,
];

impl Parser<'_, '_> {
    /// Records `message`, then wraps the next token (if any) in an error
    /// node. Always makes progress unless the input is exhausted.
    pub(crate) fn bump_as_error(&mut self, message: &str) {
        self.error(message);
        if !self.at_eof() {
            let marker = self.start();
            self.bump();
            self.complete(marker, K::ErrorNode);
        }
    }

    /// Reports `message` and wraps every remaining token in one error node.
    pub(crate) fn skip_rest(&mut self, message: &str) {
        self.error(message);
        let marker = self.start();
        let mut consumed = false;
        while !self.at_eof() {
            self.bump();
            consumed = true;
        }
        if consumed {
            self.complete(marker, K::ErrorNode);
        } else {
            self.abandon(marker);
        }
    }

    /// Wraps tokens that cannot continue an expression in an error node, up
    /// to a separator or block word that an enclosing construct handles.
    /// Brackets opened inside the skipped region are balanced. Returns `None`
    /// when the next token is already such a stop, consuming nothing.
    pub(crate) fn skip_expression(&mut self) -> Option<CompletedMarker> {
        let marker = self.start();
        let mut nesting = 0usize;
        let mut consumed = false;
        while let Some(kind) = self.nth(0) {
            if self.at(K::Semicolon) || self.at_any(BLOCK_END) {
                break;
            }
            if nesting == 0 && EXPRESSION_STOP.contains(&kind) {
                break;
            }
            // `name :=` after skipped input is the next assignment, not more
            // of the malformed expression.
            if consumed && nesting == 0 && self.variable_name_at(0) && self.nth_at(1, K::Assignment)
            {
                break;
            }
            match kind {
                K::LeftParen | K::LeftBracket => nesting += 1,
                K::RightParen | K::RightBracket => nesting = nesting.saturating_sub(1),
                _ => {}
            }
            self.bump();
            consumed = true;
        }
        if consumed {
            Some(self.complete(marker, K::ErrorNode))
        } else {
            self.abandon(marker);
            None
        }
    }

    /// Recovers from a statement that cannot be parsed: reports `message`,
    /// then wraps tokens up to and including the next `;` in an error node.
    /// Stops without the `;` before a block word or, after the first token,
    /// before a keyword that begins a statement. Consumes nothing when the
    /// next token is itself a block word.
    pub(crate) fn skip_statement(&mut self, message: &str) {
        self.error(message);
        let marker = self.start();
        let mut consumed = false;
        while !self.at_eof() {
            if self.at(K::Semicolon) {
                self.bump();
                consumed = true;
                break;
            }
            if self.at_any(BLOCK_END) || (consumed && self.at_any(STATEMENT_START)) {
                break;
            }
            self.bump();
            consumed = true;
        }
        if consumed {
            self.complete(marker, K::ErrorNode);
        } else {
            self.abandon(marker);
        }
    }

    /// The degraded form of a statement at the depth limit: wraps the whole
    /// statement, including nested blocks, in one error node. Block openers
    /// and closers are counted so the node ends at the statement's own `;`.
    pub(crate) fn skip_nested_statement(&mut self) {
        let marker = self.start();
        let mut nesting = 0usize;
        let mut consumed = false;
        while !self.at_eof() {
            if nesting == 0 && self.at_any(BLOCK_END) {
                break;
            }
            if nesting == 0 && self.at(K::Semicolon) {
                self.bump();
                consumed = true;
                break;
            }
            if self.at_any(&[K::If, K::Case, K::For, K::While, K::Repeat, K::Try]) {
                nesting += 1;
            } else if self.at_any(&[
                K::EndIf,
                K::EndCase,
                K::EndFor,
                K::EndWhile,
                K::EndRepeat,
                K::EndTry,
            ]) {
                nesting = nesting.saturating_sub(1);
            }
            self.bump();
            consumed = true;
        }
        if consumed {
            self.complete(marker, K::ErrorNode);
        } else {
            self.abandon(marker);
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::lexer::lex;
    use crate::parser::options::ParseOptions;

    fn consumed_by(source: &str, run: impl FnOnce(&mut Parser)) -> usize {
        let (tokens, _) = lex(source);
        let mut parser = Parser::new(&tokens, ParseOptions::all());
        run(&mut parser);
        parser.position()
    }

    #[test]
    fn skip_expression_when_nested_brackets_then_balanced_and_stops_at_comma() {
        // The comma inside the parentheses the region opened does not stop it.
        assert_eq!(
            consumed_by("a (b , c) d , e", |p| {
                p.skip_expression();
            }),
            7
        );
    }

    #[test]
    fn skip_expression_when_at_stop_then_consumes_nothing() {
        assert_eq!(
            consumed_by(") x", |p| {
                p.skip_expression();
            }),
            0
        );
        assert_eq!(
            consumed_by("; x", |p| {
                p.skip_expression();
            }),
            0
        );
        assert_eq!(
            consumed_by("END_IF", |p| {
                p.skip_expression();
            }),
            0
        );
    }

    #[test]
    fn skip_statement_when_terminator_then_consumed_with_it() {
        assert_eq!(consumed_by("a b c ; d", |p| p.skip_statement("e")), 4);
    }

    #[test]
    fn skip_statement_when_statement_keyword_follows_then_stops_before_it() {
        assert_eq!(consumed_by("a b IF c", |p| p.skip_statement("e")), 2);
    }

    #[test]
    fn skip_statement_when_first_token_is_statement_keyword_then_it_is_consumed() {
        assert_eq!(consumed_by("IF ; x", |p| p.skip_statement("e")), 2);
    }

    #[test]
    fn skip_statement_when_block_word_then_consumes_nothing() {
        assert_eq!(consumed_by("END_IF ;", |p| p.skip_statement("e")), 0);
    }

    #[test]
    fn skip_nested_statement_when_blocks_nest_then_one_node_through_final_semicolon() {
        let source = "IF a THEN IF b THEN x := 1; END_IF; END_IF; y := 2;";
        // Everything through the outer END_IF and its `;`.
        assert_eq!(consumed_by(source, |p| p.skip_nested_statement()), 14);
    }

    #[test]
    fn bump_as_error_when_input_then_one_token_consumed() {
        assert_eq!(consumed_by("a b", |p| p.bump_as_error("e")), 1);
        assert_eq!(consumed_by("", |p| p.bump_as_error("e")), 0);
    }
}
