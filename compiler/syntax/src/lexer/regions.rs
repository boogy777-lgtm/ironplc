//! Regions of the source that the grammar never sees.
//!
//! Two constructs make a stretch of text not code: an OSCAT ranged comment
//! (`(*@KEY@:NAME*)` ... `(*@KEY@:END_NAME*)`, the text between the markers is
//! documentation) and the branches of a conditional pragma that are not taken
//! (`{IF}` ... `{END_IF}`). The legacy pipeline removes both before parsing and
//! so loses the bytes; here each becomes one trivia token whose text is the
//! source slice, so the tokens still tile the source and the tree still holds
//! every byte (design: parse-tree architecture, section 3.1).
//!
//! A [`Rule`] decides, token by token, whether a token is live, is hidden
//! inside a region, or opens a region that ends further on. One driver,
//! [`lex_regions`], applies the rules that the dialect enables and merges
//! consecutive hidden tokens into a single [`SyntaxKind::InactiveRegion`]
//! token. A region a rule opens is one token of the kind the rule names
//! ([`SyntaxKind::RangedComment`]); inside an untaken branch it is simply one
//! more hidden token.
//!
//! The two rules differ in what they do with lexical errors. A ranged comment
//! body is skipped without being scanned, so it reports none (the legacy
//! preprocessor blanked it before lexing). An untaken branch is scanned like
//! any other text and keeps its lexical errors (the legacy lexer ran before
//! the conditional pragmas were evaluated).

use super::cursor::Cursor;
use super::{next_token, range_of, LexOptions, Token};
use crate::error::SyntaxError;
use crate::parser::options::ParseOptions;
use crate::pragma::{Conditionals, Fault};
use crate::syntax_kind::SyntaxKind;
use rowan::TextRange;

/// What a rule decides about the token just scanned.
enum Judgement {
    /// The grammar reads the token.
    Live,
    /// The token lies inside a region the grammar skips.
    Hidden,
    /// The token opens a region of `kind` that ends at byte `end`, without
    /// scanning the text inside it.
    Opens { kind: SyntaxKind, end: usize },
}

/// One way for source text to stop being code.
trait Rule {
    /// Decides the fate of `token`, which starts at a token boundary of
    /// `source`. Errors the rule finds are pushed to `errors`.
    fn judge(
        &mut self,
        source: &str,
        token: &Token<'_>,
        errors: &mut Vec<SyntaxError>,
    ) -> Judgement;

    /// Reports what the end of the input leaves open.
    fn finish(&mut self, errors: &mut Vec<SyntaxError>);
}

/// A rule and the dialect setting that turns it on.
struct Entry {
    enabled: fn(&ParseOptions) -> bool,
    make: fn() -> Box<dyn Rule>,
}

/// The rules, in the order they judge a token: a region one rule opens is
/// judged by the rules after it.
const RULES: &[Entry] = &[
    Entry {
        enabled: |_| true,
        make: || Box::new(RangedComment),
    },
    Entry {
        enabled: |options| options.allow_pragma_if,
        make: || Box::new(PragmaIf::default()),
    },
];

/// Tokenizes `source` like [`super::lex_with`] and then sets aside the regions
/// the dialect's rules skip. The tokens still tile `[0, len)`.
pub fn lex_regions<'src>(
    source: &'src str,
    options: &ParseOptions,
) -> (Vec<Token<'src>>, Vec<SyntaxError>) {
    let lexing = LexOptions {
        nested_comments: options.allow_nested_comments,
    };
    let mut rules: Vec<Box<dyn Rule>> = RULES
        .iter()
        .filter(|entry| (entry.enabled)(options))
        .map(|entry| (entry.make)())
        .collect();

    let mut cursor = Cursor::new(source);
    let mut tokens = Vec::new();
    let mut errors = Vec::new();
    // The extent of the run of hidden tokens that is not yet a token.
    let mut hidden: Option<(usize, usize)> = None;

    while !cursor.is_at_end() {
        let (mut token, error) = next_token(&mut cursor, lexing);
        errors.extend(error);

        let mut is_hidden = false;
        for rule in &mut rules {
            match rule.judge(source, &token, &mut errors) {
                Judgement::Live => {}
                Judgement::Hidden => {
                    is_hidden = true;
                    break;
                }
                Judgement::Opens { kind, end } => {
                    let start = usize::from(token.range.start());
                    let end = end.max(usize::from(token.range.end()));
                    cursor.set_pos(end);
                    token = region_token(source, kind, start, end);
                }
            }
        }

        if is_hidden {
            let start = hidden.map_or(usize::from(token.range.start()), |(start, _)| start);
            hidden = Some((start, usize::from(token.range.end())));
        } else {
            flush(source, &mut tokens, hidden.take());
            tokens.push(token);
        }
    }
    flush(source, &mut tokens, hidden.take());
    for rule in &mut rules {
        rule.finish(&mut errors);
    }

    (tokens, errors)
}

/// Appends the pending run of hidden tokens as one inactive-region token.
fn flush<'src>(source: &'src str, tokens: &mut Vec<Token<'src>>, run: Option<(usize, usize)>) {
    if let Some((start, end)) = run {
        tokens.push(region_token(source, SyntaxKind::InactiveRegion, start, end));
    }
}

fn region_token(source: &str, kind: SyntaxKind, start: usize, end: usize) -> Token<'_> {
    Token {
        kind,
        text: source.get(start..end).unwrap_or_default(),
        range: range_of(start, end),
    }
}

/// OSCAT ranged comments: `(*@KEY@:NAME*)` opens a region that ends after the
/// next `(*@KEY@:END_NAME*)`. Every pair in a file is a region. A marker with
/// no closing marker, and a closing marker on its own, are ordinary comments.
struct RangedComment;

impl RangedComment {
    const OPEN: &'static str = "(*@KEY@:";
    const CLOSE: &'static str = "*)";

    /// The byte at which the region opened by the marker at `start` ends.
    fn end_of_region(source: &str, start: usize) -> Option<usize> {
        let after_open = start + Self::OPEN.len();
        let name_len = source.get(after_open..)?.find(Self::CLOSE)?;
        let name = source.get(after_open..after_open + name_len)?;
        if name.starts_with("END_") {
            return None;
        }
        let body_start = after_open + name_len + Self::CLOSE.len();
        let closing = format!("{}END_{}{}", Self::OPEN, name, Self::CLOSE);
        let closing_start = body_start + source.get(body_start..)?.find(&closing)?;
        Some(closing_start + closing.len())
    }
}

impl Rule for RangedComment {
    fn judge(
        &mut self,
        source: &str,
        token: &Token<'_>,
        _errors: &mut Vec<SyntaxError>,
    ) -> Judgement {
        if token.kind != SyntaxKind::BlockComment || !token.text.starts_with(Self::OPEN) {
            return Judgement::Live;
        }
        match Self::end_of_region(source, usize::from(token.range.start())) {
            Some(end) => Judgement::Opens {
                kind: SyntaxKind::RangedComment,
                end,
            },
            None => Judgement::Live,
        }
    }

    fn finish(&mut self, _errors: &mut Vec<SyntaxError>) {}
}

/// Conditional pragmas: the text of a branch that is not taken is hidden. A
/// directive that changes which text is live stays visible as pragma trivia,
/// so a region begins after the directive that ended the live branch and ends
/// before the one that makes text live again. A directive inside a stretch
/// that stays dead (a nested `{IF}`, or an `{ELSE}` after a skipped `{ELSIF}`)
/// is part of the region.
#[derive(Default)]
struct PragmaIf {
    conditionals: Conditionals<TextRange>,
}

impl PragmaIf {
    fn fault(fault: Fault, range: TextRange) -> SyntaxError {
        SyntaxError::new(fault.message(), range).with_kind(fault.into())
    }
}

impl Rule for PragmaIf {
    fn judge(
        &mut self,
        _source: &str,
        token: &Token<'_>,
        errors: &mut Vec<SyntaxError>,
    ) -> Judgement {
        if token.kind != SyntaxKind::Pragma {
            return if self.conditionals.is_active() {
                Judgement::Live
            } else {
                Judgement::Hidden
            };
        }
        let was_active = self.conditionals.is_active();
        let step = self.conditionals.step(token.text, token.range);
        if let Some(fault) = step.fault {
            errors.push(Self::fault(fault, token.range));
        }
        // A pragma that changes the state is a directive and stays visible;
        // so does every pragma in a live branch. Only pragmas inside a branch
        // that stays dead belong to the region.
        if was_active || self.conditionals.is_active() {
            Judgement::Live
        } else {
            Judgement::Hidden
        }
    }

    /// An `{IF}` that never met its `{END_IF}` is reported once, at its own
    /// range: everything after it was evaluated as though it were still
    /// inside the branch.
    fn finish(&mut self, errors: &mut Vec<SyntaxError>) {
        if let Some(origin) = self.conditionals.unclosed() {
            errors.push(Self::fault(Fault::Unmatched, *origin));
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::error::ErrorKind;

    fn options() -> ParseOptions {
        ParseOptions {
            allow_pragmas: true,
            allow_pragma_if: true,
            allow_nested_comments: true,
            ..ParseOptions::default()
        }
    }

    fn kinds(source: &str, options: &ParseOptions) -> Vec<SyntaxKind> {
        let (tokens, _) = lex_regions(source, options);
        tokens.iter().map(|token| token.kind).collect()
    }

    fn texts(source: &str, kind: SyntaxKind, options: &ParseOptions) -> Vec<String> {
        let (tokens, _) = lex_regions(source, options);
        tokens
            .iter()
            .filter(|token| token.kind == kind)
            .map(|token| token.text.to_string())
            .collect()
    }

    #[test]
    fn lex_regions_when_ranged_comment_then_one_token_for_the_pair_and_its_body() {
        let source = "a (*@KEY@:D*)\nbody ' open\n(*@KEY@:END_D*) b";
        assert_eq!(
            texts(source, SyntaxKind::RangedComment, &options()),
            vec!["(*@KEY@:D*)\nbody ' open\n(*@KEY@:END_D*)"]
        );
    }

    #[test]
    fn lex_regions_when_ranged_comment_body_is_malformed_then_no_lexical_error() {
        let (tokens, errors) = lex_regions("(*@KEY@:D*) ' (* ? (*@KEY@:END_D*) x", &options());
        assert!(errors.is_empty(), "{errors:?}");
        assert_eq!(
            tokens.last().map(|token| token.kind),
            Some(SyntaxKind::Ident)
        );
    }

    #[test]
    fn lex_regions_when_two_ranged_comments_then_both_are_regions() {
        let source = "(*@KEY@:A*)x(*@KEY@:END_A*) y (*@KEY@:B*)z(*@KEY@:END_B*)";
        assert_eq!(
            texts(source, SyntaxKind::RangedComment, &options()).len(),
            2
        );
    }

    #[test]
    fn lex_regions_when_marker_has_no_closing_marker_then_plain_comment() {
        let kinds = kinds("(*@KEY@:A*) x", &options());
        assert_eq!(kinds[0], SyntaxKind::BlockComment);
    }

    #[test]
    fn lex_regions_when_first_marker_is_a_closing_marker_then_a_later_pair_is_still_a_region() {
        let source = "(*@KEY@:END_A*) (*@KEY@:B*)z(*@KEY@:END_B*)";
        assert_eq!(
            texts(source, SyntaxKind::RangedComment, &options()).len(),
            1
        );
    }

    #[test]
    fn lex_regions_when_marker_is_inside_a_string_then_not_a_region() {
        let source = "'(*@KEY@:A*) x (*@KEY@:END_A*)'";
        assert!(texts(source, SyntaxKind::RangedComment, &options()).is_empty());
    }

    #[test]
    fn lex_regions_when_untaken_branch_then_one_inactive_region_between_the_directives() {
        let source = "{IF false} a b {ELSE} c {END_IF}";
        let found = texts(source, SyntaxKind::InactiveRegion, &options());
        assert_eq!(found, vec![" a b "]);
    }

    #[test]
    fn lex_regions_when_pragma_if_flag_off_then_no_region() {
        let source = "{IF false} a {END_IF}";
        let off = ParseOptions {
            allow_pragma_if: false,
            ..options()
        };
        assert!(texts(source, SyntaxKind::InactiveRegion, &off).is_empty());
    }

    #[test]
    fn lex_regions_when_ranged_comment_in_untaken_branch_then_one_inactive_region() {
        let source = "{IF false}(*@KEY@:A*)x(*@KEY@:END_A*){END_IF}";
        let kinds = kinds(source, &options());
        assert_eq!(
            kinds,
            vec![
                SyntaxKind::Pragma,
                SyntaxKind::InactiveRegion,
                SyntaxKind::Pragma
            ]
        );
    }

    #[test]
    fn lex_regions_when_branch_is_empty_then_no_region_token() {
        let kinds = kinds("{IF false}{END_IF}", &options());
        assert_eq!(kinds, vec![SyntaxKind::Pragma, SyntaxKind::Pragma]);
    }

    #[test]
    fn lex_regions_when_if_never_closed_then_region_runs_to_the_end_and_error_at_the_if() {
        let (tokens, errors) = lex_regions("{IF false} a", &options());
        assert_eq!(
            tokens.last().map(|token| token.kind),
            Some(SyntaxKind::InactiveRegion)
        );
        assert_eq!(errors.len(), 1);
        assert_eq!(errors[0].kind, ErrorKind::PragmaIfUnmatched);
        assert_eq!(usize::from(errors[0].range.end()), "{IF false}".len());
    }
}
