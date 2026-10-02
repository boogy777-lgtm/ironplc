//! Conditional-compilation pragmas: `{IF}`, `{ELSIF}`, `{ELSE}`, `{END_IF}`,
//! `{DEFINE}` and `{UNDEFINE}`.
//!
//! The token stream this runs on is the one `xform_collapse_pragmas` produced,
//! so every `{ ... }` is a single [`TokenType::Pragma`] token whose text
//! includes the braces. The reference evaluates these in its parser
//! (`PragmaIfStatementParser` and its operand parsers); the token level is the
//! natural home here, because a branch that is not taken must not reach the
//! grammar at all: with the pragmas treated as trivia, both branches of an
//! `{IF}` would otherwise be compiled.
//!
//! The directive parser, the condition language and the branch bookkeeping are
//! `ironplc_syntax::pragma`; this transform only drops the tokens of the
//! branches that are not taken and renders the faults it reports as P0023 and
//! P0024.
//!
//! Only runs when `options.allow_pragma_if` is set; without it the pragmas stay
//! trivia and both branches reach the parser, exactly as before.

use dsl::diagnostic::Diagnostic;
use ironplc_syntax::pragma::{Conditionals, Fault};

use crate::options::CompilerOptions;
use crate::token::{Token, TokenType};

pub fn apply(tokens: Vec<Token>, options: &CompilerOptions) -> (Vec<Token>, Vec<Diagnostic>) {
    if !options.allow_pragma_if {
        return (tokens, Vec::new());
    }

    let mut output = Vec::with_capacity(tokens.len());
    let mut diagnostics = Vec::new();
    let mut conditionals = Conditionals::new();

    for tok in tokens {
        if tok.token_type != TokenType::Pragma {
            if conditionals.is_active() {
                output.push(tok);
            }
            continue;
        }

        let step = conditionals.step(&tok.text, tok.span.clone());
        if let Some(fault) = step.fault {
            diagnostics.push(fault.diagnostic(&tok.span));
        }
        if step.keep {
            output.push(tok);
        }
    }

    // An `{IF}` that never met its `{END_IF}` is reported once, at its own
    // span: everything after it was evaluated as though it were still inside
    // the branch.
    if let Some(span) = conditionals.unclosed() {
        diagnostics.push(Fault::Unmatched.diagnostic(span));
    }

    (output, diagnostics)
}
