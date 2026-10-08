//! A project of any size, generated: the input of the analysis benchmarks.
//!
//! Analysis and code generation cost what the whole program makes them cost,
//! and the corpus of the parse benchmarks is a few thousand bytes of unrelated
//! files. This generates one project, as a set of files, whose shape is a
//! [`Shape`] and whose size is a [`Scale`] of that shape, so that the cost can be
//! read as a function of the size.
//!
//! The shape is the one of an industrial project exported from an engineering
//! tool: structures, function blocks that hold structures and other function
//! blocks, functions that call functions, programs that hold instances of
//! function blocks and call functions, global variables in files of their own,
//! one configuration, and a few bodies of thousands of lines. The counts are
//! taken from such a project and are numbers only.
//!
//! The text is deterministic: the same [`Scale`] gives the same files, byte for
//! byte, with a generator seeded by a constant and no other source of
//! variation. It is accepted by the analysis with no diagnostic
//! (`tests/generated.rs` holds the smallest scale to that).
//!
//! Code generation takes a project of one program and reports the second with
//! `P9999`, so a project of the shape of the industrial one (39 programs) is an
//! input of analysis only. [`PROGRAM`] is the shape for code generation: one
//! program, which holds an instance of every function block of the project and
//! so reaches the function blocks and functions of the project. A shape is data
//! ([`SHAPES`]); the generator is one.

use crate::corpus::CorpusFile;
use ironplc_parser::options::CompilerOptions;
use std::path::PathBuf;

/// How many of something a project holds: `fixed`, plus `scaled` taken at the
/// [`Scale`] of the project (never fewer than one when `scaled` is not zero).
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct Count {
    pub fixed: usize,
    pub scaled: usize,
}

impl Count {
    pub const fn fixed(count: usize) -> Self {
        Self {
            fixed: count,
            scaled: 0,
        }
    }

    pub const fn scaled(count: usize) -> Self {
        Self {
            fixed: 0,
            scaled: count,
        }
    }

    fn at(&self, scale: &Scale) -> usize {
        if self.scaled == 0 {
            self.fixed
        } else {
            self.fixed + scale.of(self.scaled)
        }
    }
}

/// What the body of a function block does with what it holds.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct BlockUse {
    /// Reads and writes members of the structure it holds (`state.f_0`).
    pub members: bool,
    /// Holds instances of function blocks of a lower level and calls them.
    pub blocks: bool,
}

/// What a project is made of, before it is given a size.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct Shape {
    /// The name of the inputs this shape makes.
    pub name: &'static str,
    pub structures: Count,
    pub function_blocks: Count,
    pub functions: Count,
    pub programs: Count,
    pub global_files: Count,
    /// The function block instances one program holds: this many, plus up to
    /// `instance_spread` more (chosen by the generator). The function blocks of
    /// the project are shared out among the instances of a program in order, so
    /// as many instances as function blocks reach every function block.
    pub instances: Count,
    pub instance_spread: usize,
    /// What a function block holds and uses of other declarations. Analysis
    /// accepts all of it; code generation does not compile all of it.
    pub blocks: BlockUse,
    /// The bodies of a few programs that are thousands of lines long, in
    /// lines; the first programs have them, in this order.
    pub big_bodies: &'static [usize],
}

/// The shape of an industrial project: the counts are taken from one and are
/// numbers only. Its 39 programs are an input of analysis.
pub const PROJECT: Shape = Shape {
    name: "generated",
    structures: Count::scaled(185),
    function_blocks: Count::scaled(80),
    functions: Count::scaled(30),
    programs: Count::scaled(39),
    global_files: Count::scaled(8),
    instances: Count::fixed(4),
    instance_spread: 5,
    blocks: BlockUse {
        members: true,
        blocks: true,
    },
    big_bodies: &[3_000, 5_000, 9_000, 20_000],
};

/// The same declarations with one program, which holds an instance of each
/// function block and has the longest body: the input of code generation, which
/// compiles one program.
pub const PROGRAM: Shape = Shape {
    name: "generated one program",
    programs: Count::fixed(1),
    instances: Count::scaled(80),
    instance_spread: 0,
    blocks: BlockUse {
        members: false,
        blocks: false,
    },
    big_bodies: &[20_000],
    ..PROJECT
};

/// Every shape the benchmarks measure; an input set is made of each.
pub const SHAPES: &[&Shape] = &[&PROJECT, &PROGRAM];

/// The lines of the body of a function block or of a program that is not one of
/// the big ones.
const SMALL_BODY: usize = 24;

/// The counts of one project: a [`Shape`] at a [`Scale`].
#[derive(Debug, Clone, Copy)]
struct Counts {
    structures: usize,
    function_blocks: usize,
    functions: usize,
    programs: usize,
    global_files: usize,
    instances: usize,
    big_bodies: usize,
}

/// How much of a [`Shape`] a project is: `numerator / denominator` of every
/// scaled count, never fewer than one of each.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct Scale {
    pub numerator: usize,
    pub denominator: usize,
}

/// The series of projects the benchmarks measure, smallest first. The largest
/// of the shape [`PROJECT`] holds 447 function blocks, functions and programs
/// and more than 130,000 lines.
pub const SCALES: &[Scale] = &[
    Scale::new(1, 16),
    Scale::new(1, 4),
    Scale::new(1, 1),
    Scale::new(3, 1),
];

impl Scale {
    pub const fn new(numerator: usize, denominator: usize) -> Self {
        Self {
            numerator,
            denominator,
        }
    }

    /// The name of the input a shape makes at this scale.
    pub fn label(&self, shape: &Shape) -> String {
        if self.denominator == 1 {
            format!("{} x{}", shape.name, self.numerator)
        } else {
            format!("{} x{}/{}", shape.name, self.numerator, self.denominator)
        }
    }

    fn of(&self, count: usize) -> usize {
        ((count * self.numerator + self.denominator / 2) / self.denominator).max(1)
    }

    fn counts(&self, shape: &Shape) -> Counts {
        Counts {
            structures: shape.structures.at(self),
            function_blocks: shape.function_blocks.at(self),
            functions: shape.functions.at(self),
            programs: shape.programs.at(self),
            global_files: shape.global_files.at(self),
            instances: shape.instances.at(self),
            big_bodies: self.of(shape.big_bodies.len()),
        }
    }
}

/// The options the text is written for: top-level `VAR_GLOBAL`, as the files of
/// global variables of an engineering tool declare them.
pub fn options() -> CompilerOptions {
    CompilerOptions {
        allow_top_level_var_global: true,
        ..CompilerOptions::default()
    }
}

/// A generator seeded by a constant: every choice of the text comes from it.
struct Lcg(u64);

impl Lcg {
    fn next(&mut self) -> u64 {
        self.0 = self
            .0
            .wrapping_mul(6_364_136_223_846_793_005)
            .wrapping_add(1_442_695_040_888_963_407);
        self.0 >> 33
    }

    /// A number below `bound`, which is more than zero.
    fn below(&mut self, bound: usize) -> usize {
        (self.next() % bound.max(1) as u64) as usize
    }
}

/// What a body may use: the variables of each kind it can read and write, the
/// function block instances it can call, and how many functions there are.
#[derive(Default)]
struct Scope {
    integers: Vec<String>,
    reals: Vec<String>,
    booleans: Vec<String>,
    /// A structure variable and the index of its structure. Every structure
    /// has `f_0 : DINT`, `f_1 : LREAL`, `f_2 : BOOL`; one that is not of the
    /// lowest level also has `f_3`, the structure [`below`] it.
    records: Vec<(String, usize)>,
    /// A function block instance and the index of its function block.
    instances: Vec<(String, usize)>,
    /// Functions a body may call: the indices `0..functions`.
    functions: usize,
}

impl Scope {
    fn integer(&self, rng: &mut Lcg) -> String {
        pick(&self.integers, rng)
    }
    fn real(&self, rng: &mut Lcg) -> String {
        pick(&self.reals, rng)
    }
    fn boolean(&self, rng: &mut Lcg) -> String {
        pick(&self.booleans, rng)
    }
}

fn pick(names: &[String], rng: &mut Lcg) -> String {
    names
        .get(rng.below(names.len()))
        .cloned()
        .unwrap_or_default()
}

/// Appends statements to `out` until at least `lines` lines are written.
fn statements(out: &mut String, lines: usize, scope: &Scope, rng: &mut Lcg) {
    let mut written = 0;
    while written < lines {
        let before = out.len();
        statement(out, scope, rng);
        written += out
            .get(before..)
            .map_or(0, |added| added.matches('\n').count());
    }
}

/// One statement of a kind the generator picks, indented by two.
fn statement(out: &mut String, scope: &Scope, rng: &mut Lcg) {
    let (a, b, c) = (scope.integer(rng), scope.integer(rng), scope.integer(rng));
    let (x, y) = (scope.real(rng), scope.real(rng));
    let (p, q) = (scope.boolean(rng), scope.boolean(rng));
    let constant = rng.below(90) + 2;
    match rng.below(10) {
        0 => out.push_str(&format!("  {a} := {b} + {c} * {constant};\n")),
        1 => out.push_str(&format!("  {x} := {y} * 1.5 + {constant}.0;\n")),
        2 => out.push_str(&format!("  {p} := {q} AND NOT ({a} > {constant});\n")),
        3 => out.push_str(&format!(
            "  IF {p} THEN\n    {a} := {b} + 1;\n  ELSIF {c} > {constant} THEN\n    {x} := {y} - 0.5;\n  ELSE\n    {a} := 0;\n  END_IF;\n"
        )),
        4 => record_statement(out, scope, rng, &a, &x, &p),
        5 => instance_statement(out, scope, rng, &a, &x, &p),
        6 if scope.functions > 0 => {
            let function = rng.below(scope.functions);
            out.push_str(&format!(
                "  {a} := Func{function}(a := {b}, b := {x}, c := {p});\n"
            ));
        }
        7 => out.push_str(&format!(
            "  FOR idx := 0 TO {} DO\n    {a} := {a} + idx;\n  END_FOR;\n",
            constant % 8 + 1
        )),
        8 => out.push_str(&format!(
            "  CASE {b} OF\n    0: {a} := 1;\n    1, 2: {a} := {c};\n  ELSE\n    {a} := {constant};\n  END_CASE;\n"
        )),
        _ => out.push_str(&format!("  {a} := {b} - {c};\n")),
    }
}

/// A statement that reads or writes a member of a structure variable.
fn record_statement(out: &mut String, scope: &Scope, rng: &mut Lcg, a: &str, x: &str, p: &str) {
    let Some((record, structure)) = scope.records.get(rng.below(scope.records.len())) else {
        return;
    };
    match (rng.below(4), below(*structure).is_some()) {
        (0, _) => out.push_str(&format!("  {record}.f_0 := {a};\n")),
        (1, _) => out.push_str(&format!("  {x} := {record}.f_1 + 1.0;\n")),
        (2, _) => out.push_str(&format!("  {record}.f_2 := {p};\n")),
        (_, true) => out.push_str(&format!("  {a} := {record}.f_3.f_0 + {record}.f_0;\n")),
        (_, false) => out.push_str(&format!("  {a} := {record}.f_0;\n")),
    }
}

/// A call of a function block instance and the read of one of its outputs.
fn instance_statement(out: &mut String, scope: &Scope, rng: &mut Lcg, a: &str, x: &str, p: &str) {
    let Some((instance, _)) = scope.instances.get(rng.below(scope.instances.len())) else {
        return;
    };
    out.push_str(&format!(
        "  {instance}(in_a := {a}, in_b := {x}, in_c := {p});\n  {a} := {instance}.out_a;\n"
    ));
}

/// How deep a declaration holds declarations of its own kind: a structure in a
/// structure, an instance of a function block in a function block.
///
/// A declaration holds declarations of a level below its own only
/// ([`level`]), so what one variable occupies is bounded by this depth and
/// stays the same at every [`Scale`]. Without the bound a declaration that
/// holds two earlier ones doubles with each declaration, and the state of the
/// project, which analysis and code generation lay out, grows as a power of
/// the scale and not in proportion to it.
const LEVELS: usize = 4;

/// The level of the declaration `index` among those of its kind.
fn level(index: usize) -> usize {
    index % LEVELS
}

/// The declaration before `index`, which is one level below it, or none for a
/// declaration of the lowest level.
fn below(index: usize) -> Option<usize> {
    index.checked_sub(1).filter(|_| level(index) > 0)
}

/// A declaration of a level below `index` and before it, or none for a
/// declaration of the lowest level.
fn lower(index: usize, rng: &mut Lcg) -> Option<usize> {
    let level = level(index);
    if level == 0 {
        return None;
    }
    let group = rng.below(index / LEVELS + 1) * LEVELS;
    Some(group + rng.below(level))
}

fn structure(index: usize, rng: &mut Lcg) -> String {
    let mut text = format!(
        "TYPE\n  Rec{index} :\n  STRUCT\n    f_0 : DINT;\n    f_1 : LREAL;\n    f_2 : BOOL;\n"
    );
    if let Some(before) = below(index) {
        text.push_str(&format!("    f_3 : Rec{before};\n"));
    }
    if let Some(other) = lower(index, rng) {
        text.push_str(&format!("    f_4 : Rec{other};\n"));
    }
    text.push_str("  END_STRUCT;\nEND_TYPE\n");
    text
}

fn function(index: usize, rng: &mut Lcg) -> String {
    let mut text = format!(
        "FUNCTION Func{index} : DINT\n  VAR_INPUT\n    a : DINT;\n    b : LREAL;\n    c : BOOL;\n  END_VAR\n  VAR\n    t : DINT;\n    idx : DINT;\n  END_VAR\n  t := a * 3 + 1;\n"
    );
    let scope = Scope {
        integers: vec!["t".into(), "a".into()],
        reals: vec!["b".into()],
        booleans: vec!["c".into()],
        functions: index,
        ..Scope::default()
    };
    statements(&mut text, 8, &scope, rng);
    text.push_str(&format!("  Func{index} := t;\nEND_FUNCTION\n"));
    text
}

fn function_block(index: usize, shape: &Shape, counts: &Counts, rng: &mut Lcg) -> String {
    let structure = rng.below(counts.structures);
    let mut declarations = format!(
        "FUNCTION_BLOCK Blk{index}\n  VAR_INPUT\n    in_a : DINT;\n    in_b : LREAL;\n    in_c : BOOL;\n  END_VAR\n  VAR_OUTPUT\n    out_a : DINT;\n    out_b : LREAL;\n  END_VAR\n  VAR\n    state : Rec{structure};\n    count : DINT;\n    idx : DINT;\n    ready : BOOL;\n    level : LREAL;\n"
    );
    let mut scope = Scope {
        integers: vec!["count".into(), "in_a".into(), "out_a".into()],
        reals: vec!["level".into(), "in_b".into(), "out_b".into()],
        booleans: vec!["ready".into(), "in_c".into()],
        functions: counts.functions,
        ..Scope::default()
    };
    if shape.blocks.members {
        scope.integers.push("state.f_0".into());
        scope.booleans.push("state.f_2".into());
        scope.records.push(("state".into(), structure));
    }
    for slot in 0..2 {
        if let Some(other) = lower(index, rng).filter(|_| shape.blocks.blocks) {
            declarations.push_str(&format!("    sub_{slot} : Blk{other};\n"));
            scope.instances.push((format!("sub_{slot}"), other));
        }
    }
    declarations.push_str("  END_VAR\n");
    let mut text = declarations;
    statements(&mut text, SMALL_BODY, &scope, rng);
    text.push_str("END_FUNCTION_BLOCK\n");
    text
}

/// What a program reads of the global variables: one file's name and the
/// variables it declares.
struct GlobalFile {
    variables: Vec<String>,
}

fn global_file(index: usize, structures: usize, rng: &mut Lcg) -> (String, GlobalFile) {
    let mut text = String::from("VAR_GLOBAL\n");
    let mut variables = Vec::new();
    for slot in 0..12 {
        let name = format!("g{index}_{slot}");
        match slot % 3 {
            0 => text.push_str(&format!("  {name} : DINT;\n")),
            1 => text.push_str(&format!("  {name} : LREAL;\n")),
            _ => text.push_str(&format!("  {name} : Rec{};\n", rng.below(structures))),
        }
        variables.push(name);
    }
    text.push_str("END_VAR\n");
    (text, GlobalFile { variables })
}

fn program(
    index: usize,
    counts: &Counts,
    body_lines: usize,
    spread: usize,
    globals: &[GlobalFile],
    rng: &mut Lcg,
) -> String {
    let structure = rng.below(counts.structures);
    let mut text = format!(
        "PROGRAM Prog{index}\n  VAR\n    local : Rec{structure};\n    acc : DINT;\n    idx : DINT;\n    flag : BOOL;\n    level : LREAL;\n"
    );
    let mut scope = Scope {
        integers: vec!["acc".into(), "local.f_0".into()],
        reals: vec!["level".into()],
        booleans: vec!["flag".into(), "local.f_2".into()],
        records: vec![("local".into(), structure)],
        functions: counts.functions,
        ..Scope::default()
    };
    let instances = counts.instances + rng.below(spread);
    // The function blocks are shared out among the instances in order, each
    // instance taking one of its own share.
    let share = (counts.function_blocks / instances.max(1)).max(1);
    for slot in 0..instances {
        let other = (slot * counts.function_blocks / instances.max(1) + rng.below(share))
            .min(counts.function_blocks.saturating_sub(1));
        text.push_str(&format!("    inst_{slot} : Blk{other};\n"));
        scope.instances.push((format!("inst_{slot}"), other));
    }
    text.push_str("  END_VAR\n");
    // Each program reads and writes the globals of one file: the integer and
    // the real of its first slots, and the structure of the third.
    let mut external = String::new();
    if let Some(file) = globals.get(index % globals.len().max(1)) {
        external.push_str("  VAR_EXTERNAL\n");
        for (slot, name) in file.variables.iter().take(3).enumerate() {
            let declared = match slot {
                0 => "DINT".to_string(),
                1 => "LREAL".to_string(),
                _ => continue,
            };
            external.push_str(&format!("    {name} : {declared};\n"));
            if slot == 0 {
                scope.integers.push(name.clone());
            } else {
                scope.reals.push(name.clone());
            }
        }
        external.push_str("  END_VAR\n");
    }
    text.push_str(&external);
    statements(&mut text, body_lines, &scope, rng);
    text.push_str("END_PROGRAM\n");
    text
}

fn configuration(programs: usize) -> String {
    let mut text =
        String::from("CONFIGURATION cfg\n  RESOURCE res ON PLC\n    TASK main_task(INTERVAL := T#100ms, PRIORITY := 1);\n");
    for index in 0..programs {
        text.push_str(&format!(
            "    PROGRAM run{index} WITH main_task : Prog{index};\n"
        ));
    }
    text.push_str("  END_RESOURCE\nEND_CONFIGURATION\n");
    text
}

fn file(name: String, source: String) -> CorpusFile {
    CorpusFile {
        name,
        path: PathBuf::new(),
        source,
    }
}

/// The files of the project of `shape` at `scale`: one per structure group, per
/// global variable group, per function block, function and program, and the
/// configuration.
pub fn generate(shape: &Shape, scale: &Scale) -> Vec<CorpusFile> {
    let mut rng = Lcg(0x1ec6_1131_3000_0001);
    let counts = scale.counts(shape);

    let mut files = Vec::new();
    for group in 0..counts.structures.div_ceil(20) {
        let from = group * 20;
        let text: String = (from..(from + 20).min(counts.structures))
            .map(|index| structure(index, &mut rng))
            .collect();
        files.push(file(format!("types_{group}.st"), text));
    }
    let mut globals = Vec::new();
    for index in 0..counts.global_files {
        let (text, declared) = global_file(index, counts.structures, &mut rng);
        files.push(file(format!("globals_{index}.st"), text));
        globals.push(declared);
    }
    for index in 0..counts.functions {
        files.push(file(format!("func_{index}.st"), function(index, &mut rng)));
    }
    for index in 0..counts.function_blocks {
        let text = function_block(index, shape, &counts, &mut rng);
        files.push(file(format!("blk_{index}.st"), text));
    }
    for index in 0..counts.programs {
        let body_lines = if index < counts.big_bodies {
            shape
                .big_bodies
                .get(index % shape.big_bodies.len())
                .copied()
                .unwrap_or(SMALL_BODY)
        } else {
            SMALL_BODY
        };
        let text = program(
            index,
            &counts,
            body_lines,
            shape.instance_spread,
            &globals,
            &mut rng,
        );
        files.push(file(format!("prog_{index}.st"), text));
    }
    files.push(file(
        "config.st".to_string(),
        configuration(counts.programs),
    ));
    files
}
