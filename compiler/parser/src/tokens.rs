//! The token view of the lossless tree: the tokens of the syntax crate as the
//! [`Token`] and [`TokenType`] the rest of the compiler reads.
//!
//! The legacy pipeline builds its tokens by lexing and then rewriting the
//! stream with a chain of transforms. The tree-based front end rewrites
//! nothing: the lexer, the regions and the dialect decisions are
//! `ironplc_syntax`'s, and this module only *reads* them. A keyword the dialect
//! does not enable is a name there (`ParseOptions::keyword_active`,
//! `ParseOptions::time_is_name`), and so it is a name here; the special
//! operators are the syntax crate's list (`is_special_operator`); a stretch
//! the grammar never sees is a region token. The view is derived from the
//! tree's own capabilities and is not a second implementation of the legacy
//! transforms.
//!
//! The kind map ([`row`]) is one dispatch over [`TokenKind`], the leaf kinds of
//! the syntax crate, with no wildcard arm: a token kind added there does not
//! compile until it has a row here that says what the view makes of it.
//!
//! Positions come from [`LineIndex`], the syntax crate's one mapping from a
//! byte offset to a line and a UTF-16 column, started at the line and column
//! the text begins at in its document (the body of a PLCopen XML element).

use crate::token::{Token, TokenType};
use ironplc_dsl::core::{FileId, SourceSpan};
use ironplc_syntax::lexer::Token as SyntaxToken;
use ironplc_syntax::line_index::LineIndex;
use ironplc_syntax::pragma::is_directive;
use ironplc_syntax::{is_special_operator, ParseOptions, SyntaxKind, TokenKind};

/// What the token view makes of one kind of token of the tree.
#[derive(Clone, Debug, PartialEq)]
pub(crate) enum Row {
    /// A token of this type, whatever the dialect says.
    Plain(TokenType),
    /// A keyword: this type where the dialect enables it, a name where it does
    /// not.
    Keyword(TokenType),
    /// A keyword that is also a name next to the tokens that
    /// `ParseOptions::time_is_name` lists.
    KeywordOrNameBeside(TokenType),
    /// A pragma: a token of this type, except a conditional directive in a
    /// dialect that evaluates them (`allow_pragma_if`), which is consumed and
    /// makes no token.
    Pragma(TokenType),
    /// A word that is a name, or one of the special operators.
    Name,
    /// A partial-access selector, whose type depends on the letter after `%`.
    PartialAccess,
    /// No token: the text is reported as an error instead. The reason says why.
    Dropped(&'static str),
}

/// Declares [`row`] from three lists. `plain` pairs a kind with the type it
/// always is, `keywords` lists the kinds that are named like the type they
/// become, and the remaining arms are the kinds with a row of their own. The
/// match that comes out has no wildcard arm.
macro_rules! rows {
    (
        plain { $($plain:ident => $plain_type:ident),* $(,)? }
        keywords { $($keyword:ident),* $(,)? }
        $($other:ident => $row:expr),* $(,)?
    ) => {
        /// The row of `kind`.
        pub(crate) fn row(kind: TokenKind) -> Row {
            match kind {
                $(TokenKind::$plain => Row::Plain(TokenType::$plain_type),)*
                $(TokenKind::$keyword => Row::Keyword(TokenType::$keyword),)*
                $(TokenKind::$other => $row,)*
            }
        }
    };
}

rows! {
    plain {
        Whitespace => Whitespace,
        Newline => Newline,
        LineComment => Comment,
        DocComment => DocComment,
        BlockComment => Comment,
        // A region is one comment: the grammar does not read what is inside.
        RangedComment => Comment,
        InactiveRegion => Comment,
        EscapedIdent => EscapedIdentifier,
        IntegerLit => Digits,
        FixedPointLit => FixedPoint,
        FloatingPointLit => FloatingPoint,
        HexLit => HexDigits,
        OctLit => OctDigits,
        BinLit => BinDigits,
        StringLit => SingleByteString,
        WStringLit => DoubleByteString,
        DirectAddress => DirectAddress,
        DirectAddressIncomplete => DirectAddressIncomplete,
        LeftParen => LeftParen,
        RightParen => RightParen,
        RightBrace => RightBrace,
        LeftBracket => LeftBracket,
        RightBracket => RightBracket,
        Comma => Comma,
        Semicolon => Semicolon,
        Colon => Colon,
        Period => Period,
        Range => Range,
        Hash => Hash,
        Equal => Equal,
        NotEqual => NotEqual,
        Less => Less,
        Greater => Greater,
        LessEqual => LessEqual,
        GreaterEqual => GreaterEqual,
        Div => Div,
        Star => Star,
        Plus => Plus,
        Minus => Minus,
        Power => Power,
        Caret => Caret,
        Assignment => Assignment,
        RightArrow => RightArrow,
    }
    keywords {
        Action, EndAction, Array, Of, Params, At, Case, Else, EndCase, Constant,
        Configuration, EndConfiguration, En, Eno, Exit, Continue, Try, EndTry, Catch,
        Finally, Throw, Jmp, Calc, Wait, False, FEdge, For, To, By, Do, EndFor,
        Function, EndFunction, FunctionBlock, EndFunctionBlock, Extends, Implements,
        Interface, EndInterface, Abstract, Method, EndMethod, Property, EndProperty,
        EndGet, EndSet, This, Super, If, Then, Elsif, EndIf, InitialStep, EndStep,
        Program, With, EndProgram, Namespace, EndNamespace, BeginImplementation,
        REdge, ReadOnly, ReadWrite, Repeat, Until, EndRepeat, Resource, On,
        EndResource, Retain, NonRetain, Persistent, Return, Step, Struct, EndStruct,
        Union, EndUnion, Task, EndTask, Transition, From, EndTransition, True, Type,
        EndType, Var, EndVar, VarInput, VarOutput, VarInOut, VarTemp, VarExternal,
        VarAccess, VarConfig, VarGlobal, VarStat, VarInst, VarGeneric, While,
        EndWhile, Bool, Bit, Sint, Int, Dint, Lint, Usint, Uint, Udint, Ulint, Real,
        Lreal, Ltime, RefTo, Ref, Null, Reference, Pointer, Date, TimeOfDay,
        DateAndTime, Ldate, Ltod, Ldt, String, Byte, Word, Dword, Lword, WString, Any,
        AnyDerived, AnyElementary, AnyMagnitude, AnyNum, AnyReal, AnyInt, AnyBit,
        AnyString, AnyDate, Xor, AndThen, OrElse, Or, And, Mod, Not,
    }
    Pragma => Row::Pragma(TokenType::Pragma),
    Time => Row::KeywordOrNameBeside(TokenType::Time),
    Ident => Row::Name,
    PartialAccess => Row::PartialAccess,
    ErrorToken => Row::Dropped("an error token is one error of the tokenizer and makes no token"),
}

/// The type of a partial-access selector for each letter that follows `%`, in
/// either case.
const PARTIAL_ACCESS: &[(char, TokenType)] = &[
    ('X', TokenType::PartialAccessBit),
    ('B', TokenType::PartialAccessByte),
    ('W', TokenType::PartialAccessWord),
    ('D', TokenType::PartialAccessDWord),
    ('L', TokenType::PartialAccessLWord),
];

/// The type each special operator is promoted to, matched on the whole name in
/// either case. The names are the ones `ironplc_syntax::is_special_operator`
/// recognises; a test holds the two lists equal.
pub(crate) const SPECIAL_OPERATORS: &[(&str, TokenType)] = &[
    ("__NEW", TokenType::SpecialNew),
    ("__DELETE", TokenType::SpecialDelete),
    ("__ISVALIDREF", TokenType::SpecialIsValidRef),
    ("__TYPEOF", TokenType::SpecialTypeOf),
    ("__CURRENTTASK", TokenType::SpecialCurrentTask),
    ("__XADD", TokenType::SpecialXAdd),
];

/// The type a word is when it is a name: `Identifier`, or the type of the
/// special operator it spells.
pub(crate) fn name_type(text: &str) -> TokenType {
    if !is_special_operator(text) {
        return TokenType::Identifier;
    }
    SPECIAL_OPERATORS
        .iter()
        .find(|(name, _)| name.eq_ignore_ascii_case(text))
        .map_or(TokenType::Identifier, |(_, promoted)| promoted.clone())
}

fn partial_access_type(text: &str) -> TokenType {
    let letter = text.chars().nth(1).map(|c| c.to_ascii_uppercase());
    PARTIAL_ACCESS
        .iter()
        .find(|(candidate, _)| Some(*candidate) == letter)
        .map_or(TokenType::Identifier, |(_, token_type)| token_type.clone())
}

/// The kinds of the nearest tokens that are not trivia before and after the
/// token at `index`.
fn significant_around(
    tokens: &[SyntaxToken<'_>],
    index: usize,
) -> (Option<SyntaxKind>, Option<SyntaxKind>) {
    let significant = |token: &SyntaxToken<'_>| (!token.kind.is_trivia()).then_some(token.kind);
    let before = tokens.iter().take(index).rev().find_map(significant);
    let after = tokens.iter().skip(index + 1).find_map(significant);
    (before, after)
}

/// The type of the token at `index`, or `None` when it makes no token.
pub(crate) fn type_of(
    tokens: &[SyntaxToken<'_>],
    index: usize,
    options: &ParseOptions,
) -> Option<TokenType> {
    let token = tokens.get(index)?;
    let kind = token.kind.token()?;
    match row(kind) {
        Row::Plain(token_type) => Some(token_type),
        Row::Keyword(token_type) => Some(if options.keyword_active(token.kind, token.text) {
            token_type
        } else {
            TokenType::Identifier
        }),
        Row::KeywordOrNameBeside(token_type) => {
            let (previous, next) = significant_around(tokens, index);
            Some(
                if options.keyword_active(token.kind, token.text)
                    && !options.time_is_name(previous, next)
                {
                    token_type
                } else {
                    TokenType::Identifier
                },
            )
        }
        Row::Pragma(token_type) => {
            let directive = options.allow_pragma_if && is_directive(token.text);
            (!directive).then_some(token_type)
        }
        Row::Name => Some(name_type(token.text)),
        Row::PartialAccess => Some(partial_access_type(token.text)),
        Row::Dropped(_) => None,
    }
}

/// The token view of `tokens`, which are the tokens of `source` under
/// `options`. The text starts at line `line_offset` and column `col_offset` of
/// its document.
pub(crate) fn project(
    source: &str,
    tokens: &[SyntaxToken<'_>],
    options: &ParseOptions,
    file_id: &FileId,
    line_offset: usize,
    col_offset: usize,
) -> Vec<Token> {
    let index = LineIndex::new(source, line_offset, col_offset);
    let mut lines = index.walker();
    (0..tokens.len())
        .filter_map(|index| {
            let token_type = type_of(tokens, index, options)?;
            let token = tokens.get(index)?;
            let start = usize::from(token.range.start());
            let end = usize::from(token.range.end());
            let (line, col) = lines.position(start);
            Some(Token {
                token_type,
                span: SourceSpan {
                    file_id: file_id.clone(),
                    start,
                    end,
                },
                line,
                col,
                text: token.text.to_string(),
            })
        })
        .collect()
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::lexer::tokenize as legacy_tokenize;
    use ironplc_syntax::tokenize;
    use ironplc_syntax::SPECIAL_OPERATORS as TREE_SPECIAL_OPERATORS;

    fn view_of(source: &str, options: &ParseOptions) -> Vec<TokenType> {
        let (tokens, _) = tokenize(source, options);
        project(source, &tokens, options, &FileId::default(), 0, 0)
            .into_iter()
            .map(|token| token.token_type)
            .collect()
    }

    #[test]
    fn row_when_every_token_kind_then_it_is_dropped_only_for_an_error_token() {
        for kind in SyntaxKind::ALL.iter().filter_map(|kind| kind.token()) {
            let dropped = matches!(row(kind), Row::Dropped(_));
            assert_eq!(dropped, kind == TokenKind::ErrorToken, "{kind:?}");
        }
    }

    #[test]
    fn project_when_keyword_spelling_then_the_type_the_legacy_lexer_gives_it() {
        let options = ParseOptions::all();
        for (spelling, _) in SyntaxKind::KEYWORDS {
            let (legacy, errors) = legacy_tokenize(spelling, &FileId::default(), 0, 0);
            assert!(errors.is_empty(), "{spelling}");
            let legacy: Vec<TokenType> = legacy.into_iter().map(|t| t.token_type).collect();
            assert_eq!(view_of(spelling, &options), legacy, "{spelling}");
        }
    }

    #[test]
    fn project_when_keyword_of_a_dialect_that_does_not_enable_it_then_an_identifier() {
        let strict = ParseOptions::default();
        assert_eq!(view_of("CONTINUE", &strict), vec![TokenType::Identifier]);
        assert_eq!(view_of("LTIME", &strict), vec![TokenType::Identifier]);
        assert_eq!(view_of("IF", &strict), vec![TokenType::If]);
        // Upper case only, as in the legacy lexer.
        assert_eq!(view_of("mod", &strict), vec![TokenType::Identifier]);
        assert_eq!(view_of("MOD", &strict), vec![TokenType::Mod]);
    }

    #[test]
    fn project_when_time_is_a_function_name_then_an_identifier_only_there() {
        let options = ParseOptions {
            allow_time_as_function_name: true,
            ..ParseOptions::default()
        };
        assert_eq!(view_of("TIME()", &options)[0], TokenType::Identifier);
        assert_eq!(view_of("TIME := 1", &options)[0], TokenType::Identifier);
        assert_eq!(view_of("FUNCTION TIME", &options)[2], TokenType::Identifier);
        assert_eq!(view_of("x : TIME", &options)[4], TokenType::Time);
        assert_eq!(
            view_of("TIME()", &ParseOptions::default())[0],
            TokenType::Time
        );
    }

    #[test]
    fn special_operators_when_listed_then_the_view_and_the_tree_name_the_same_operators() {
        let mut view: Vec<String> = SPECIAL_OPERATORS
            .iter()
            .map(|(name, _)| name.to_string())
            .collect();
        let mut tree: Vec<String> = TREE_SPECIAL_OPERATORS
            .iter()
            .map(|name| name.to_string())
            .collect();
        view.sort();
        tree.sort();
        assert_eq!(view, tree);
        for (name, token_type) in SPECIAL_OPERATORS {
            assert_eq!(name_type(&name.to_lowercase()), *token_type, "{name}");
        }
        assert_eq!(name_type("__NEW_ITEM"), TokenType::Identifier);
    }

    #[test]
    fn project_when_text_is_embedded_then_the_positions_start_at_the_offsets() {
        let source = "ab\n  cd";
        let options = ParseOptions::default();
        let (tokens, _) = tokenize(source, &options);
        let view = project(source, &tokens, &options, &FileId::default(), 5, 7);
        let positions: Vec<(usize, usize, &str)> = view
            .iter()
            .map(|token| (token.line, token.col, token.text.as_str()))
            .collect();
        assert_eq!(
            positions,
            vec![(5, 7, "ab"), (5, 9, "\n"), (6, 0, "  "), (6, 2, "cd")]
        );
    }
}
