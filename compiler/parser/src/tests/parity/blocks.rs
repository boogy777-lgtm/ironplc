//! Variable blocks against the legacy parser, test-only: which legacy rule
//! reads the block at a site, and what is compared of it.
//!
//! The legacy grammar reads a block with the rule of the declaration that holds
//! it (a program, a function, a function block, a method, a configuration, the
//! top of a file), each with its own alternatives. `SCOPES` is the one table of
//! which declaration is read by which legacy rule; a declaration that holds
//! blocks is a row, none is code.

use super::ast::without_initial_values;
use super::literals::{settle, Outcome};
use super::sites::{Site, Unit};
use crate::parser::{parse_var_block, BlockScope};
use crate::token::Token;
use ironplc_syntax::lower::{
    var_blocks::{lower_var_block, Block},
    LowerCx,
};
use ironplc_syntax::SyntaxKind;

/// What a legacy block rule is chosen by: the declaration that holds the
/// block, and for the declarations that hold blocks of two kinds the keyword
/// that opens it. The first row that names the parent (and the keyword, when
/// it names one) decides.
struct Scope {
    parents: &'static [SyntaxKind],
    openers: Option<&'static [SyntaxKind]>,
    scope: BlockScope,
}

const fn scope(
    parents: &'static [SyntaxKind],
    openers: Option<&'static [SyntaxKind]>,
    scope: BlockScope,
) -> Scope {
    Scope {
        parents,
        openers,
        scope,
    }
}

const SCOPES: &[Scope] = &[
    scope(
        &[
            SyntaxKind::SourceFile,
            SyntaxKind::NamespaceDecl,
            SyntaxKind::ConfigurationDecl,
            SyntaxKind::ResourceDecl,
        ],
        Some(&[SyntaxKind::VarGlobal]),
        BlockScope::Global,
    ),
    scope(
        &[SyntaxKind::ConfigurationDecl],
        Some(&[SyntaxKind::VarConfig]),
        BlockScope::Configuration,
    ),
    scope(
        &[SyntaxKind::FunctionBlockDecl],
        Some(&[SyntaxKind::VarGeneric]),
        BlockScope::Generic,
    ),
    scope(&[SyntaxKind::ProgramDecl], None, BlockScope::Program),
    scope(&[SyntaxKind::FunctionDecl], None, BlockScope::Function),
    scope(
        &[SyntaxKind::FunctionBlockDecl],
        None,
        BlockScope::FunctionBlock,
    ),
    scope(
        &[
            SyntaxKind::MethodDecl,
            SyntaxKind::GetAccessor,
            SyntaxKind::SetAccessor,
        ],
        None,
        BlockScope::Method,
    ),
];

/// The legacy scope that reads the block at a site.
fn block_scope(site: &Site) -> Option<BlockScope> {
    let opener = site
        .node
        .children_with_tokens()
        .filter_map(|element| element.into_token())
        .find(|token| !token.kind().is_trivia())
        .map(|token| token.kind());
    SCOPES
        .iter()
        .find(|row| {
            row.parents.contains(&site.parent)
                && row
                    .openers
                    .is_none_or(|openers| opener.is_some_and(|opener| openers.contains(&opener)))
        })
        .map(|row| row.scope)
}

/// Compares the lowering of the block at a site to the legacy rule that reads
/// it: all of it, or, for the unit that is a view of the block, all but the
/// initial values, which have their own comparisons.
pub fn judge_block(site: &Site, tokens: &[Token], cx: &LowerCx, written: &str) -> Outcome {
    let Some(scope) = block_scope(site) else {
        return Outcome::Differs {
            parts: vec![],
            what: format!(
                "{written} no legacy rule reads a block under {:?}",
                site.parent
            ),
        };
    };
    let kept = |block: Block| match site.unit {
        Unit::VariableBlockFacts => without_initial_values(block),
        _ => block,
    };
    settle(
        written,
        parse_var_block(tokens, scope).map(kept),
        lower_var_block(cx, &site.node).map(kept),
    )
}

#[cfg(test)]
mod tests {
    use super::*;
    use ironplc_syntax::{parse_source_file, ParseOptions};

    /// The scope of the first block of `source`.
    fn scope_of(source: &str) -> Option<BlockScope> {
        let parse = parse_source_file(source, &ParseOptions::all());
        super::super::sites::sites(&parse.root)
            .into_iter()
            .find(|site| site.unit == Unit::VariableBlock)
            .and_then(|site| block_scope(&site))
    }

    #[test]
    fn block_scope_when_each_declaration_that_holds_blocks_then_the_rule_that_reads_it() {
        let rows = [
            ("PROGRAM p VAR a : INT; END_VAR END_PROGRAM", BlockScope::Program),
            (
                "FUNCTION f : INT VAR a : INT; END_VAR f := 1; END_FUNCTION",
                BlockScope::Function,
            ),
            (
                "FUNCTION_BLOCK f VAR a : INT; END_VAR END_FUNCTION_BLOCK",
                BlockScope::FunctionBlock,
            ),
            (
                "FUNCTION_BLOCK f VAR_GENERIC a : INT; END_VAR END_FUNCTION_BLOCK",
                BlockScope::Generic,
            ),
            (
                "FUNCTION_BLOCK f METHOD m VAR a : INT; END_VAR END_METHOD END_FUNCTION_BLOCK",
                BlockScope::Method,
            ),
            ("VAR_GLOBAL a : INT; END_VAR", BlockScope::Global),
            (
                "NAMESPACE n VAR_GLOBAL a : INT; END_VAR END_NAMESPACE",
                BlockScope::Global,
            ),
            (
                "CONFIGURATION c RESOURCE r ON t PROGRAM p : q; END_RESOURCE VAR_CONFIG r.p.a : INT; END_VAR END_CONFIGURATION",
                BlockScope::Configuration,
            ),
        ];
        for (source, scope) in rows {
            assert_eq!(scope_of(source), Some(scope), "{source}");
        }
    }
}
