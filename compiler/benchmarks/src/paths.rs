//! The measured paths: one table, [`PATHS`], and nothing else enumerates them.
//!
//! A path is one thing the parse benchmarks time: a name, the set of inputs it
//! runs over, the call, and optionally the name of the path it is compared with.
//! The collection, the per-file table, the ratios, the totals, the one-time
//! initialization probe (`benches/parse_baseline.rs`) and the Criterion groups
//! (`benches/parse_benchmark.rs`) all iterate this table, so a path added to it
//! is measured, compared and registered everywhere by its one row.
//!
//! The public functions of `ironplc-parser` (`tokenize_program`,
//! `parse_program`, `parse_st_statements`) are rows like any other. They run
//! the one front end the compiler has, the one built on the lossless tree of
//! `ironplc-syntax`; the rows that call `ironplc-syntax` directly measure the
//! stages the public functions are made of. The legacy pipeline is not
//! reachable from this crate, so no row compares with it: the figures of the
//! legacy pipeline are the recorded history in
//! `specs/design/parse-tree-s0-experiment.md`. A baseline is a row that a
//! stage is a part of, and the ratio is the share of the whole that the stage
//! is.
//!
//! A row says which stack it runs on ([`Stack`]). The stages give themselves the
//! budget when the caller has none, which costs a thread for each stage call; a
//! program of the compiler holds the budget from its entry and its stages make
//! none. The rows marked `held` run the same call as their baseline on a thread
//! that holds the budget, so one table shows both costs of one call.
//!
//! A row's call is typed by what it returns, and a table cannot hold the
//! returned types, so a row gets a [`Probe`] from its caller and brackets the
//! call with it: the caller times and counts allocations between
//! [`Probe::start`] and [`Probe::stop`], and what the call returned is dropped
//! after that, as the result of a call of a consumer is.

// `Diagnostic` is large by design in this compiler (the parser crate allows the
// same lint); the paths return it as the public functions they measure do.
#![allow(clippy::result_large_err)]

use crate::corpus::{plcopen_document, statement_bodies, CorpusFile, PLAIN_DOCUMENT};
use ironplc_dsl::core::FileId;
use ironplc_dsl::stack::within_stack_budget;
use ironplc_parser::options::CompilerOptions;
use ironplc_parser::{parse_program, parse_st_statements, tokenize_program};
use ironplc_sources::{parse_source, FileType};
use ironplc_syntax::{
    lexer::lex, lower::lower_library, lower::lower_statements, parse_source_file, parse_statements,
    tokenize, ParseOptions,
};
use std::hint::black_box;

/// A tiny input without any located variable: the baseline for the init probe.
pub const PLAIN_SOURCE: &str = "PROGRAM main
VAR
  x : INT;
END_VAR
  x := 1;
END_PROGRAM
";

/// A tiny input with located variables: triggers the lazy address regexes.
pub const LOCATED_SOURCE: &str = "PROGRAM main
VAR
  i AT %IX0.0 : BOOL;
  o AT %QW1 : WORD;
END_VAR
  o := 16#00FF;
END_PROGRAM
";

/// A tiny statement body: the baseline for the init probe of the statement
/// paths.
pub const PLAIN_BODY: &str = "x := 1;";

/// What a path needs to run besides the text: the file id and the options as
/// the public functions read them and as the tree reads them.
#[derive(Debug, Default)]
pub struct Ctx {
    pub file_id: FileId,
    pub options: CompilerOptions,
    pub cst_options: ParseOptions,
}

/// Brackets the call a row makes, so that the caller measures the call and not
/// what the row does around it.
pub trait Probe: Send {
    fn start(&mut self);
    fn stop(&mut self);
}

/// Runs `call` between `start` and `stop`, and drops what it returned after
/// `stop`.
pub fn timed<T>(probe: &mut dyn Probe, call: impl FnOnce() -> T) {
    probe.start();
    let result = call();
    probe.stop();
    drop(black_box(result));
}

/// The set of inputs a path runs over.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Over {
    /// Every file of the corpus: whole programs.
    Files,
    /// The statement body of every unit of the corpus
    /// ([`statement_bodies`]): what a PLCopen XML document hands over.
    Bodies,
    /// One PLCopen XML document that holds the bodies of the corpus that
    /// parse ([`plcopen_document`]): a run that reads many bodies.
    Document,
}

impl Over {
    pub const ALL: [Over; 3] = [Over::Files, Over::Bodies, Over::Document];

    /// The inputs of this set, from the files of the corpus.
    pub fn items(self, files: &[CorpusFile]) -> Vec<CorpusFile> {
        match self {
            Over::Files => files.to_vec(),
            Over::Bodies => statement_bodies(files),
            Over::Document => vec![plcopen_document(&statement_bodies(files))],
        }
    }

    /// The tiny inputs of the one-time initialization probe: a label and a text.
    /// A body has no variable block, so it has no located variable.
    pub fn probes(self) -> &'static [(&'static str, &'static str)] {
        match self {
            Over::Files => &[("plain", PLAIN_SOURCE), ("located", LOCATED_SOURCE)],
            Over::Bodies => &[("plain", PLAIN_BODY)],
            Over::Document => &[("one body", PLAIN_DOCUMENT)],
        }
    }

    pub fn label(self) -> &'static str {
        match self {
            Over::Files => "files",
            Over::Bodies => "bodies",
            Over::Document => "documents",
        }
    }
}

/// The stack a path runs on.
///
/// The stages give themselves the budget when the caller has none, which costs
/// a thread for each stage call. A program of the compiler holds the budget
/// from its entry (`ironplc_dsl::stack`), and the stages it calls make no
/// thread. A path says which of the two it measures, so that one table shows
/// both costs of the same call.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Stack {
    /// The thread the harness is on, which has no budget: each stage call
    /// makes a thread of its own.
    Caller,
    /// A thread that holds the budget, as the run of a program does: the call
    /// is measured on it, and the thread is made outside the measurement.
    Held,
}

/// One measured path.
pub struct Path {
    /// The name in the tables and in the baseline of other paths.
    pub name: &'static str,
    /// The Criterion group the path registers its per-input benchmarks in.
    pub group: &'static str,
    pub over: Over,
    /// The name of the path whose result this path is compared with (a path
    /// over the same inputs), or none.
    pub baseline: Option<&'static str>,
    /// The stack the call runs on.
    pub stack: Stack,
    /// Makes the call on one text, bracketed by the probe. Call it through
    /// [`Path::call`], which puts it on the stack the path measures.
    pub run: fn(&Ctx, &str, &mut dyn Probe),
    /// What the call makes of one text, in a few words (a count of tokens or
    /// errors, `ok`, or the problem code). It is made outside any measurement.
    pub describe: fn(&Ctx, &str) -> String,
}

impl Path {
    /// One call of the path on one text, on the stack the path measures,
    /// bracketed by the probe.
    pub fn call(&self, ctx: &Ctx, source: &str, probe: &mut dyn Probe) {
        match self.stack {
            Stack::Caller => (self.run)(ctx, source, probe),
            Stack::Held => within_stack_budget(|| (self.run)(ctx, source, probe)),
        }
    }
}

/// One row of [`PATHS`] from the call written once: `run` brackets it, and
/// `describe` shows what it returned.
macro_rules! path {
    (
        @row $stack:expr, $name:expr, $group:expr, $over:expr, $baseline:expr,
        |$ctx:ident, $src:ident| $call:expr,
        |$out:ident| $describe:expr
    ) => {
        Path {
            name: $name,
            group: $group,
            over: $over,
            baseline: $baseline,
            stack: $stack,
            run: |$ctx, $src, probe| timed(probe, || $call),
            describe: |$ctx, $src| {
                let $out = $call;
                $describe
            },
        }
    };
    // A row that is measured on a stack that holds the budget.
    (
        held $name:expr, $group:expr, $over:expr, $baseline:expr,
        |$ctx:ident, $src:ident| $call:expr,
        |$out:ident| $describe:expr
    ) => {
        path!(
            @row Stack::Held, $name, $group, $over, $baseline,
            |$ctx, $src| $call,
            |$out| $describe
        )
    };
    (
        $name:expr, $group:expr, $over:expr, $baseline:expr,
        |$ctx:ident, $src:ident| $call:expr,
        |$out:ident| $describe:expr
    ) => {
        path!(
            @row Stack::Caller, $name, $group, $over, $baseline,
            |$ctx, $src| $call,
            |$out| $describe
        )
    };
}

/// `ok` or `err[<problem code>]`.
macro_rules! status {
    ($out:expr) => {
        match $out {
            Ok(_) => "ok".to_string(),
            Err(diagnostic) => format!("err[{}]", diagnostic.code.as_str()),
        }
    };
}

/// Every path. A path is one row; nothing else needs to change.
pub static PATHS: &[Path] = &[
    // The public functions of the parser crate: the front end of the compiler.
    path!(
        "tokenize",
        "parse_tokenize",
        Over::Files,
        None,
        |ctx, src| tokenize_program(src, &ctx.file_id, &ctx.options, 0, 0),
        |out| format!("{} tokens", out.0.len())
    ),
    path!(
        "parse",
        "parse_full",
        Over::Files,
        None,
        |ctx, src| parse_program(src, &ctx.file_id, &ctx.options),
        |out| status!(out)
    ),
    // The same call on a stack that holds the budget, as a program of the
    // compiler makes it: the stage finds the budget and makes no thread. Its
    // ratio to `parse` is what the thread of the stage entry costs a program
    // that holds the budget from its entry.
    path!(
        held "parse (held)",
        "parse_full_held",
        Over::Files,
        Some("parse"),
        |ctx, src| parse_program(src, &ctx.file_id, &ctx.options),
        |out| status!(out)
    ),
    // The syntax crate directly: the stages the public functions are made of.
    path!(
        "cst lex",
        "parse_cst_lex",
        Over::Files,
        None,
        |_ctx, src| lex(src),
        |out| format!("{} tokens", out.0.len())
    ),
    path!(
        "cst tokenize",
        "parse_cst_tokenize",
        Over::Files,
        None,
        |ctx, src| tokenize(src, &ctx.cst_options),
        |out| format!("{} tokens, {} errors", out.0.len(), out.1.len())
    ),
    path!(
        "cst parse",
        "parse_cst",
        Over::Files,
        None,
        |ctx, src| parse_source_file(src, &ctx.cst_options),
        |out| format!("{} errors", out.errors.len())
    ),
    // The tree path as a consumer runs it: parse, then lower what parsed. A
    // file the parse rejects is not lowered.
    path!(
        "cst parse + lower",
        "parse_cst_lower",
        Over::Files,
        None,
        |ctx, src| {
            let parse = parse_source_file(src, &ctx.cst_options);
            lower_library(&parse, &ctx.file_id)
        },
        |out| status!(out)
    ),
    // The same on one stack budget thread, as the public function of the parser
    // crate runs it: the parse and the lowering share the thread.
    path!(
        "cst parse + lower (budget)",
        "parse_cst_lower_budget",
        Over::Files,
        None,
        |ctx, src| within_stack_budget(|| {
            let parse = parse_source_file(src, &ctx.cst_options);
            lower_library(&parse, &ctx.file_id)
        }),
        |out| status!(out)
    ),
    // The cost of the stack budget alone: one thread spawned and joined for
    // work that does nothing. Its ratio to `parse` is the share of the thread
    // in a parse.
    path!(
        "stack budget, empty work",
        "stack_budget",
        Over::Files,
        Some("parse"),
        |_ctx, src| within_stack_budget(|| black_box(src.len())),
        |out| format!("{out} bytes")
    ),
    // Statement fragments (PLCopen XML bodies).
    path!(
        "statements",
        "parse_statements",
        Over::Bodies,
        None,
        |ctx, src| parse_st_statements(src, &ctx.file_id, &ctx.options, 0, 0),
        |out| match out {
            Ok(statements) => format!("ok, {} statements", statements.len()),
            Err(diagnostic) => format!("err[{}]", diagnostic.code.as_str()),
        }
    ),
    path!(
        held "statements (held)",
        "parse_statements_held",
        Over::Bodies,
        Some("statements"),
        |ctx, src| parse_st_statements(src, &ctx.file_id, &ctx.options, 0, 0),
        |out| match out {
            Ok(statements) => format!("ok, {} statements", statements.len()),
            Err(diagnostic) => format!("err[{}]", diagnostic.code.as_str()),
        }
    ),
    // A PLCopen XML document with a body for each unit of the corpus that
    // parses: one run that reads many bodies, one `parse_st_statements` call
    // for each. Where the caller holds no budget it is one thread for each
    // body.
    path!(
        "xml document",
        "parse_xml_document",
        Over::Document,
        None,
        |ctx, src| parse_source(FileType::Xml, src, &ctx.file_id, &ctx.options),
        |out| match out {
            Ok(library) => format!("ok, {} units", library.elements.len()),
            Err(diagnostic) => format!("err[{}]", diagnostic.code.as_str()),
        }
    ),
    path!(
        held "xml document (held)",
        "parse_xml_document_held",
        Over::Document,
        Some("xml document"),
        |ctx, src| parse_source(FileType::Xml, src, &ctx.file_id, &ctx.options),
        |out| match out {
            Ok(library) => format!("ok, {} units", library.elements.len()),
            Err(diagnostic) => format!("err[{}]", diagnostic.code.as_str()),
        }
    ),
    path!(
        "cst statements (budget)",
        "parse_cst_statements_budget",
        Over::Bodies,
        None,
        |ctx, src| within_stack_budget(|| {
            let parse = parse_statements(src, &ctx.cst_options);
            lower_statements(&parse, &ctx.file_id)
        }),
        |out| match out {
            Ok(statements) => format!("ok, {} statements", statements.len()),
            Err(diagnostic) => format!("err[{}]", diagnostic.code.as_str()),
        }
    ),
];

/// The path named `name`.
pub fn path_named(name: &str) -> Option<&'static Path> {
    PATHS.iter().find(|path| path.name == name)
}
