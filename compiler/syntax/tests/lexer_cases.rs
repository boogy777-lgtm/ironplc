//! Targeted lexer cases: each asserts losslessness (via the helpers) and the
//! expected kinds.

mod common;

use common::{kinds, lex_exact, significant};
use ironplc_syntax::lexer::lex;
use ironplc_syntax::SyntaxKind as K;
use ironplc_syntax::{parse_source_file, ParseOptions};

#[test]
fn lex_when_empty_input_then_no_tokens_and_empty_tree() {
    assert_eq!(kinds(""), vec![]);
    let parsed = parse_source_file("", &ParseOptions::default());
    assert_eq!(parsed.root.text().to_string(), "");
    assert!(parsed.errors.is_empty());
}

#[test]
fn lex_when_whitespace_only_then_trivia_tokens_only() {
    assert_eq!(
        kinds("  \t \n\r\n"),
        vec![
            (K::Whitespace, "  \t "),
            (K::Newline, "\n"),
            (K::Newline, "\r\n")
        ]
    );
    assert_eq!(
        parse_source_file("  \t ", &ParseOptions::default())
            .root
            .text()
            .to_string(),
        "  \t "
    );
}

#[test]
fn lex_when_crlf_then_one_newline_token_with_crlf_spelling() {
    assert_eq!(
        kinds("a\r\nb"),
        vec![(K::Ident, "a"), (K::Newline, "\r\n"), (K::Ident, "b")]
    );
}

#[test]
fn lex_when_lf_then_newline_token_with_lf_spelling() {
    assert_eq!(kinds("a\nb")[1], (K::Newline, "\n"));
}

#[test]
fn lex_when_lone_cr_then_owned_by_newline_token_with_cr_spelling() {
    let source = "a\rb\r";
    let (tokens, errors) = lex(source);
    assert!(errors.is_empty());
    assert_eq!(
        kinds(source),
        vec![
            (K::Ident, "a"),
            (K::Newline, "\r"),
            (K::Ident, "b"),
            (K::Newline, "\r")
        ]
    );
    assert_eq!(tokens.len(), 4);
}

#[test]
fn lex_when_cr_then_lf_across_tokens_then_not_merged_with_following_text() {
    assert_eq!(
        kinds("\r\r\n\n"),
        vec![(K::Newline, "\r"), (K::Newline, "\r\n"), (K::Newline, "\n")]
    );
}

#[test]
fn lex_when_form_feed_then_newline_token() {
    assert_eq!(kinds("a\u{c}b")[1], (K::Newline, "\u{c}"));
}

#[test]
fn lex_when_tabs_then_whitespace_token_keeps_tabs() {
    assert_eq!(
        kinds("\tx\t\t:= 1"),
        vec![
            (K::Whitespace, "\t"),
            (K::Ident, "x"),
            (K::Whitespace, "\t\t"),
            (K::Assignment, ":="),
            (K::Whitespace, " "),
            (K::IntegerLit, "1"),
        ]
    );
}

#[test]
fn lex_when_non_ascii_in_comment_string_and_identifier_then_preserved() {
    let source = "(* Wärmebild 𝄞 *) s := 'Wärmebild𝄞'; größe := 1; // ünï";
    let tokens = kinds(source);
    assert!(tokens.contains(&(K::BlockComment, "(* Wärmebild 𝄞 *)")));
    assert!(tokens.contains(&(K::StringLit, "'Wärmebild𝄞'")));
    assert!(tokens.contains(&(K::Ident, "größe")));
    assert!(tokens.contains(&(K::LineComment, "// ünï")));
    assert!(lex(source).1.is_empty());
}

#[test]
fn lex_when_non_letter_unicode_outside_text_then_error_token_per_character() {
    let source = "a × b";
    let tokens = kinds(source);
    assert!(tokens.contains(&(K::ErrorToken, "×")));
    let (_, errors) = lex(source);
    assert_eq!(errors.len(), 1);
    assert_eq!(errors[0].message, "unexpected character");
    assert_eq!(usize::from(errors[0].range.start()), 2);
    assert_eq!(usize::from(errors[0].range.end()), 4);
}

#[test]
fn lex_when_stray_ascii_bytes_then_error_tokens_not_dropped() {
    let source = "a ? b \\ c ! d \u{1}";
    let (tokens, errors) = lex(source);
    lex_exact(source);
    let error_texts: Vec<&str> = tokens
        .iter()
        .filter(|token| token.kind == K::ErrorToken)
        .map(|token| token.text)
        .collect();
    assert_eq!(error_texts, vec!["?", "\\", "!", "\u{1}"]);
    assert_eq!(errors.len(), 4);
}

#[test]
fn lex_when_nested_block_comment_then_single_comment_token() {
    assert_eq!(
        kinds("(* a (* b *) c *) x"),
        vec![
            (K::BlockComment, "(* a (* b *) c *)"),
            (K::Whitespace, " "),
            (K::Ident, "x")
        ]
    );
    assert_eq!(kinds("/* a /* b */ c */")[0].0, K::BlockComment);
}

#[test]
fn lex_when_comment_forms_then_distinct_kinds() {
    assert_eq!(kinds("// a")[0], (K::LineComment, "// a"));
    assert_eq!(kinds("/// a")[0], (K::DocComment, "/// a"));
    assert_eq!(kinds("//// a")[0], (K::DocComment, "//// a"));
    assert_eq!(kinds("(* a *)")[0], (K::BlockComment, "(* a *)"));
    assert_eq!(kinds("/* a */")[0], (K::BlockComment, "/* a */"));
}

#[test]
fn lex_when_line_comment_before_line_break_then_break_is_its_own_token() {
    assert_eq!(
        kinds("// c\r\nx"),
        vec![
            (K::LineComment, "// c"),
            (K::Newline, "\r\n"),
            (K::Ident, "x")
        ]
    );
    assert_eq!(kinds("// c\rx")[1], (K::Newline, "\r"));
}

#[test]
fn lex_when_block_comment_spans_lines_then_line_breaks_inside_token() {
    assert_eq!(kinds("(* a\r\nb *)")[0], (K::BlockComment, "(* a\r\nb *)"));
}

#[test]
fn lex_when_comment_opener_looks_like_paren_star_paren_then_comment_not_closed() {
    // `(*)` opens a comment; the `)` is content, not a closer.
    let (tokens, errors) = lex("(*)");
    assert_eq!(tokens.len(), 1);
    assert_eq!(tokens[0].kind, K::ErrorToken);
    assert_eq!(errors[0].message, "unterminated block comment");
}

#[test]
fn lex_when_pragma_then_single_pragma_token() {
    assert_eq!(
        kinds("TYPE {attribute 'strict'} x"),
        vec![
            (K::Type, "TYPE"),
            (K::Whitespace, " "),
            (K::Pragma, "{attribute 'strict'}"),
            (K::Whitespace, " "),
            (K::Ident, "x")
        ]
    );
    assert!(K::Pragma.is_trivia());
}

#[test]
fn lex_when_stray_right_brace_then_right_brace_token() {
    assert_eq!(kinds("}"), vec![(K::RightBrace, "}")]);
}

#[test]
fn lex_when_unterminated_block_comment_then_error_token_covers_rest() {
    let source = "x := 1; (* never closed\r\ny := 2;";
    let (tokens, errors) = lex(source);
    lex_exact(source);
    let last = tokens.last().unwrap();
    assert_eq!(last.kind, K::ErrorToken);
    assert_eq!(last.text, "(* never closed\r\ny := 2;");
    assert_eq!(errors.len(), 1);
    assert_eq!(errors[0].message, "unterminated block comment");
    assert_eq!(errors[0].range, last.range);
}

#[test]
fn lex_when_unbalanced_nested_comment_then_it_ends_at_the_first_close() {
    // The inner opener never closes, so the comment ends at the first `*)`,
    // as the legacy pipeline reads it, and the rest is ordinary tokens.
    let (tokens, errors) = lex("(* a (* b *) c");
    assert_eq!(tokens[0].kind, K::BlockComment);
    assert_eq!(tokens[0].text, "(* a (* b *)");
    assert_eq!(errors, vec![]);
}

#[test]
fn lex_when_nested_comment_has_no_close_at_all_then_error_token() {
    let (tokens, errors) = lex("(* a (* b c");
    assert_eq!(tokens.len(), 1);
    assert_eq!(tokens[0].kind, K::ErrorToken);
    assert_eq!(errors.len(), 1);
}

#[test]
fn lex_when_unterminated_pragma_then_error_token_covers_rest() {
    let source = "a {attribute 'x'\r\nb";
    let (tokens, errors) = lex(source);
    lex_exact(source);
    let last = tokens.last().unwrap();
    assert_eq!(last.kind, K::ErrorToken);
    assert_eq!(last.text, "{attribute 'x'\r\nb");
    assert_eq!(errors[0].message, "unterminated pragma");
}

#[test]
fn lex_when_unterminated_string_then_error_token_covers_rest_of_line() {
    let source = "s := 'abc;\r\nnext := 1;";
    let (tokens, errors) = lex(source);
    lex_exact(source);
    let error = tokens
        .iter()
        .find(|token| token.kind == K::ErrorToken)
        .unwrap();
    assert_eq!(error.text, "'abc;");
    assert_eq!(errors.len(), 1);
    assert_eq!(errors[0].message, "unterminated string literal");
    // Lexing resumes on the next line.
    assert!(significant(source).contains(&(K::Ident, "next")));
}

#[test]
fn lex_when_unterminated_string_at_end_of_input_then_error_token() {
    let (tokens, _) = lex("'abc");
    assert_eq!(tokens.len(), 1);
    assert_eq!(tokens[0].kind, K::ErrorToken);
    assert_eq!(tokens[0].text, "'abc");
}

#[test]
fn lex_when_unterminated_wide_string_then_error_token() {
    let (tokens, errors) = lex("\"abc\ny");
    assert_eq!(tokens[0].kind, K::ErrorToken);
    assert_eq!(tokens[0].text, "\"abc");
    assert_eq!(errors.len(), 1);
}

#[test]
fn lex_when_string_spans_lines_and_terminates_then_one_string_token() {
    assert_eq!(kinds("'a\nbc'")[0], (K::StringLit, "'a\nbc'"));
}

#[test]
fn lex_when_string_has_dollar_escapes_then_one_token() {
    assert_eq!(kinds("'it$'s'")[0], (K::StringLit, "'it$'s'"));
    assert_eq!(kinds("'costs $$'")[0], (K::StringLit, "'costs $$'"));
    assert_eq!(kinds("\"say $\"hi$\"\"")[0].0, K::WStringLit);
    assert_eq!(kinds("'é$é'")[0], (K::StringLit, "'é$é'"));
}

#[test]
fn lex_when_dollar_at_end_of_unterminated_string_then_lossless_error() {
    let (tokens, _) = lex("'abc$");
    assert_eq!(tokens[0].kind, K::ErrorToken);
    lex_exact("'abc$");
    lex_exact("'abc$\r\nx");
}

#[test]
fn lex_when_escaped_identifier_then_backtick_token_and_unterminated_is_error() {
    assert_eq!(kinds("`my var`")[0], (K::EscapedIdent, "`my var`"));
    let (tokens, errors) = lex("`my var\nx");
    assert_eq!(tokens[0].kind, K::ErrorToken);
    assert_eq!(tokens[0].text, "`my var");
    assert_eq!(errors[0].message, "unterminated escaped identifier");
}

#[test]
fn lex_when_numbers_then_legacy_boundaries() {
    assert_eq!(significant("42"), vec![(K::IntegerLit, "42")]);
    assert_eq!(significant("1_000"), vec![(K::IntegerLit, "1_000")]);
    assert_eq!(significant("3.25"), vec![(K::FixedPointLit, "3.25")]);
    assert_eq!(significant("1.5E-3"), vec![(K::FloatingPointLit, "1.5E-3")]);
    assert_eq!(significant("2e+4"), vec![(K::FloatingPointLit, "2e+4")]);
    assert_eq!(significant("16#D012"), vec![(K::HexLit, "16#D012")]);
    assert_eq!(significant("8#77"), vec![(K::OctLit, "8#77")]);
    assert_eq!(significant("2#1010_0101"), vec![(K::BinLit, "2#1010_0101")]);
}

#[test]
fn lex_when_number_followed_by_range_or_member_then_integer_and_punctuation() {
    assert_eq!(
        significant("1..5"),
        vec![(K::IntegerLit, "1"), (K::Range, ".."), (K::IntegerLit, "5")]
    );
    assert_eq!(
        significant("1.x"),
        vec![(K::IntegerLit, "1"), (K::Period, "."), (K::Ident, "x")]
    );
    assert_eq!(
        significant("1e"),
        vec![(K::IntegerLit, "1"), (K::Ident, "e")]
    );
    assert_eq!(
        significant("16#g"),
        vec![(K::IntegerLit, "16"), (K::Hash, "#"), (K::Ident, "g")]
    );
}

#[test]
fn lex_when_typed_literals_then_parts_are_separate_tokens() {
    assert_eq!(
        significant("T#1m30s"),
        vec![
            (K::Ident, "T"),
            (K::Hash, "#"),
            (K::IntegerLit, "1"),
            (K::Ident, "m30s")
        ]
    );
    assert_eq!(
        significant("TIME#-2.5s"),
        vec![
            (K::Time, "TIME"),
            (K::Hash, "#"),
            (K::Minus, "-"),
            (K::FixedPointLit, "2.5"),
            (K::Ident, "s")
        ]
    );
    assert_eq!(
        significant("INT#16#FF"),
        vec![(K::Int, "INT"), (K::Hash, "#"), (K::HexLit, "16#FF")]
    );
}

#[test]
fn lex_when_direct_addresses_then_address_kinds() {
    assert_eq!(significant("%IX0.1"), vec![(K::DirectAddress, "%IX0.1")]);
    assert_eq!(significant("%mw100"), vec![(K::DirectAddress, "%mw100")]);
    assert_eq!(significant("%Q5"), vec![(K::DirectAddress, "%Q5")]);
    assert_eq!(
        significant("%I*"),
        vec![(K::DirectAddressIncomplete, "%I*")]
    );
    assert_eq!(significant("x.%X3")[2], (K::PartialAccess, "%X3"));
    assert_eq!(significant("%w12"), vec![(K::PartialAccess, "%w12")]);
}

#[test]
fn lex_when_malformed_percent_then_error_token_without_dropping_bytes() {
    let (tokens, errors) = lex("% x %IX %Z1");
    lex_exact("% x %IX %Z1");
    assert_eq!(
        tokens
            .iter()
            .filter(|token| token.kind == K::ErrorToken)
            .count(),
        errors.len()
    );
    assert!(errors.len() >= 3);
}

#[test]
fn lex_when_keywords_in_any_case_then_same_keyword_kind() {
    for spelling in ["end_program", "END_PROGRAM", "End_Program", "eNd_PrOgRaM"] {
        assert_eq!(kinds(spelling), vec![(K::EndProgram, spelling)]);
    }
    assert_eq!(kinds("mod")[0].0, K::Mod);
    assert_eq!(kinds("Not")[0].0, K::Not);
    assert_eq!(kinds("tod")[0].0, kinds("TIME_OF_DAY")[0].0);
}

#[test]
fn lex_when_identifier_contains_keyword_then_identifier() {
    assert_eq!(kinds("END_PROGRAMS")[0].0, K::Ident);
    assert_eq!(kinds("my_if")[0].0, K::Ident);
    assert_eq!(kinds("_IF")[0].0, K::Ident);
    assert_eq!(kinds("IF2")[0].0, K::Ident);
}

#[test]
fn lex_when_symbol_operators_then_longest_match() {
    assert_eq!(
        significant(":= => <> <= >= ** .. < > = + - * / ^ | &")
            .iter()
            .map(|(kind, _)| *kind)
            .collect::<Vec<_>>(),
        vec![
            K::Assignment,
            K::RightArrow,
            K::NotEqual,
            K::LessEqual,
            K::GreaterEqual,
            K::Power,
            K::Range,
            K::Less,
            K::Greater,
            K::Equal,
            K::Plus,
            K::Minus,
            K::Star,
            K::Div,
            K::Caret,
            K::Or,
            K::And,
        ]
    );
}

#[test]
fn lex_when_star_paren_without_opener_then_separate_tokens() {
    assert_eq!(
        significant("*)"),
        vec![(K::Star, "*"), (K::RightParen, ")")]
    );
}

#[test]
fn lex_when_keywords_are_not_dialect_gated_then_lexed_uniformly() {
    // Gating (e.g. `bit`, `params`, `__try` as plain identifiers) is parser work.
    for spelling in ["BIT", "params", "__TRY", "THIS", "union"] {
        assert!(kinds(spelling)[0].0.is_keyword(), "{spelling}");
    }
}

#[test]
fn parse_source_file_when_malformed_input_then_root_text_still_equals_source() {
    let source = "x := 'abc\r\n(* open ? ";
    let parsed = parse_source_file(source, &ParseOptions::default());
    assert_eq!(parsed.root.text().to_string(), source);
    assert!(parsed.errors.len() >= 2, "{:?}", parsed.errors);
}

#[test]
fn lex_with_when_nesting_off_then_comment_ends_at_first_close() {
    use ironplc_syntax::lexer::{lex_with, LexOptions};
    let (tokens, errors) = lex_with(
        "(* a (* b *) c *)",
        LexOptions {
            nested_comments: false,
        },
    );
    assert_eq!(tokens[0].kind, K::BlockComment);
    assert_eq!(tokens[0].text, "(* a (* b *)");
    assert_eq!(errors, vec![]);
    // The rest is ordinary tokens: the name `c`, then `*` and `)`.
    assert!(tokens.iter().any(|token| token.text == "c"));
    let joined: String = tokens.iter().map(|token| token.text).collect();
    assert_eq!(joined, "(* a (* b *) c *)");
}

#[test]
fn lex_with_when_c_style_comment_holds_an_opener_then_it_never_nests() {
    let (tokens, errors) = lex("/* a /* b */ c");
    assert_eq!(tokens[0].text, "/* a /* b */");
    assert_eq!(errors, vec![]);
}
