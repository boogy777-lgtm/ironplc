# Analysis Cost Measurement

status: implemented
date: 2026-10-08

This document records what one analysis and one code generation cost on a
large project: how the project is made, how the cost is measured, the figures
by pass and by rule, what each pass and rule reads of the whole program, and
what the figures say about splitting the analysis by unit. It states facts that
were measured and decisions the measurement forced. The analysis reports its
steps to an observer
([`compiler/analyzer/src/observe.rs`](../../compiler/analyzer/src/observe.rs)),
and the measurement lives in `compiler/benchmarks`. The figures are those of the
analysis after the contract of a pass in section 9 (no pass keeps a copy of the
library); the figures of the analysis before it are given where they are
compared, and they are the ones section 7 decided from.

Earlier measurements of the front end (lexing, parsing, lowering) are in
[S0 Experiment](parse-tree-s0-experiment.md) section 3. The budget for the
parse path is [Parse-Tree Architecture](parse-tree-architecture.md) section
5.2. This document extends the measurement from the parse to the analysis and
to code generation.

## 1. Summary

1. A call of `analyze` grows with an exponent of at most 1.13 in the scale
   for every pass and every rule but two (`xform_resolve_type_aliases`,
   exponent 1.77, quadratic in the number of types; `rule_constant_range`,
   1.35). On the 39-program project at its largest scale (3.1 MB, 500 files,
   1,027 library elements) parse takes 0.73 s and analysis 1.94 s (4.42 s
   before section 9). Parse, analysis and code generation are 20, 32 and 44
   percent of the pipeline of the one-program shape at `x3` (section 3), where
   they were 14, 51 and 32 percent.
2. **The analysis kept 16 copies of the library and keeps none.** Each pass
   kept a clone of the library so that a failure could give it back, and the
   analysis made 16 such clones: 54 percent of the analysis at every scale,
   144 to 168 ms each at the largest. Section 9 removes them. Analysis time at
   `x3` is 0.44 of what it was, allocations 0.38, bytes allocated 0.33, and the
   peak memory of the largest run 0.50 GB against 0.65 GB. The merge of the
   inputs, which clones every input once (`resolve_types_in_budget`), is the one copy
   left: 6 percent of the call at `x3`.
3. **28 of the 47 semantic rules do nothing but walk the library once each**:
   at the largest scale 9 ms each, 267 ms in all, almost no allocation. 45 of
   the 47 rules are a walk of their own over the whole tree
   (`rule_support.rs:45`).
4. The fixed cost of one `analyze` call on a program of one statement is
   1.6 ms warm and 2.1 ms cold. 68 percent of it is the function
   environment, 12 percent the type environment, built twice. The fixed cost
   does not grow with the project.
5. Of 18 passes (the 17 transforms and the type table), 7 are local to one
   unit, 6 collect a table over the whole program and then rewrite each unit,
   3 read the whole program by nature, and 2 read no unit (section 6).
6. Code generation takes a project of one `PROGRAM`
   (`compiler/codegen/src/compile.rs:492-518`, `:528-541`), so the figures of
   code generation and of the pipeline are of a project of one program
   (section 3.2).

## 2. Method

### 2.1 Environment

| | |
|---|---|
| Machine | Acer Nitro AN517-51, Intel Core i5-9300H @ 2.40 GHz (4 cores, 8 threads), 31.8 GiB RAM |
| OS | Windows 11 Pro 10.0.26200 |
| Toolchain | rustc 1.98.1 (48a229cea 2026-09-01), release profile of the workspace |
| Commit | the branch `analysis/pass-contract`, after the passes took one contract |
| Allocation counting | `stats_alloc` 0.1.10 as the global allocator of the benchmark binary |
| Process memory | private bytes of the benchmark process, sampled every 100 ms from outside; the peak of the largest run is in section 4.5 |

One thread runs the measured call, on a stack that holds the budget
(`ironplc_dsl::stack`); other processes are at normal desktop load.

### 2.2 The large input

There is no large project in the repository, so the input is generated:
[`compiler/benchmarks/src/generated.rs`](../../compiler/benchmarks/src/generated.rs).
A **shape** is the counts of what a project holds; a **scale** is a fraction
of the shape; `generate(shape, scale)` writes the files. The text is
deterministic: a generator with a constant seed and no other source of
variation gives the same bytes for the same shape and scale
(`compiler/benchmarks/tests/generated.rs` asserts it).

The counts of the shape `generated` are taken from an industrial project
exported from an engineering tool; they are numbers and nothing else:
185 structures (20 to a file), 80 function blocks, 30 functions, 39
programs, 8 files of global variables, one configuration, and four program
bodies of 3,000, 5,000, 9,000 and 20,000 lines (the rest are 24 lines).
Function blocks hold a structure, call functions, and (in this shape) hold
instances of function blocks of a lower level and read and write members of
their structure. A declaration holds declarations of a lower level only, four
levels deep, so that what one variable occupies is the same at every scale
(a test asserts that no declaration holds more than 10,000 scalars at any
scale). Programs hold four to eight instances of function blocks and read
and write the global variables of one file.

The second shape, `generated one program`, has the same declarations and one
program, which holds an instance of every function block and has a body of
5,000 lines. Function blocks of this shape call functions and do not use
members of their structure and do not hold instances: code generation
compiles neither (`compiler/codegen/src/compile_struct.rs:236`, `:281`:
"Variable is not a structure"; `compiler/codegen/src/compile_stmt.rs:523`
with `compile_fb_instance.rs:184-193`: an instance that is not a variable of
the program is not found). Both shapes are accepted by the analysis with no
diagnostic, and the second compiles (tests in `tests/generated.rs`).

| Input | Files | Lines | Bytes | Library elements |
|---|---|---|---|---|
| generated x1/16 | 12 | 3,471 | 80,983 | 23 |
| generated x1/4 | 44 | 5,008 | 109,615 | 87 |
| generated x1 | 168 | 44,828 | 1,039,805 | 343 |
| generated x3 | 500 | 134,571 | 3,147,503 | 1,027 |
| generated one program x1/16 | 11 | 5,419 | 127,624 | 22 |
| generated one program x1/4 | 35 | 6,565 | 146,295 | 78 |
| generated one program x1 | 130 | 11,116 | 222,491 | 305 |
| generated one program x3 | 384 | 23,446 | 432,878 | 911 |

Bytes do not grow in proportion to the scale at the small end: the scales
`x1/16` and `x1/4` each hold one body of 3,000 lines (a count never falls
below one), and that body is most of their bytes. Declarations grow with the
scale; bodies only from `x1`. Growth exponents below are taken between `x1`
and `x3`, where the composition is the same and the scale factor is 3.

### 2.3 What is measured

- **Rows of the table of paths.** `compiler/benchmarks/src/paths.rs` lists
  every measured path in one table. An input is a set of files; a path runs
  over one or more sets of inputs; a path whose call needs steps before it
  (a project parsed, a project analyzed) prepares them outside the
  measurement. New rows: `parse project`, `analyze`, `codegen`, `pipeline`
  (parse, analyze and generate code), and the same on a stack that holds the
  budget (`(held)`). Harnesses: `benches/parse_baseline.rs` (time and
  allocations), `benches/parse_benchmark.rs` (Criterion, 10 samples for a
  project).
- **Cold** is the first call on that input in the process; **warm** is the
  median of three calls that follow it; the table reports the median over
  three processes (and the minimum and maximum of the three).
- **By pass and by rule.** `benches/analysis_profile.rs` runs
  `analyze_observed` with one observer
  (`compiler/benchmarks/src/profile.rs`) that adds up, for every step the
  analysis tells it about, the calls, the time and the allocations between a
  sample before and a sample after the step. A step is a pass, a rule or the
  making of an input (`Setup`: merge, type environment, function
  environment). The whole call is measured around
  the analysis, so the difference is the time in no step. The observer
  changes the figure of the whole call by less than 1 percent (analysis of
  `x3`: 4,347 ms with the observer, 4,381 to 4,384 ms without it, in the table of
  paths).
- **Commands**, from `compiler/`:
  `cargo bench -p ironplc-benchmarks --bench parse_baseline -- 3` and
  `cargo bench -p ironplc-benchmarks --bench analysis_profile -- 3`.
  `IRONPLC_BENCH_SCALES=<n>` limits a run to the `n` smallest scales; a new
  shape or path is run with `1`, then `2`, and so on, because the largest
  scale holds more than a gigabyte of memory.

## 3. Rows of the table of paths

All times are milliseconds. `parse project` is `parse_program` for every
file; `analyze` is `ironplc_analyzer::stages::analyze` on the parsed files;
`codegen` is `ironplc_codegen::compile` on the analyzed project;
`pipeline` is the three in one call. `(held)` runs the same call on a thread
that holds the stack budget, as a program of the compiler does; the stages
make a thread of their own where the caller has none. Allocations are of the
warm call; the last column is the bytes allocated by the cold call.

| path | input | bytes | outcome | cold ms | warm ms, median of 3 runs (min-max) | allocations (warm) | MiB allocated |
|---|---|---|---|---|---|---|---|
| parse project | generated x1/16 | 80983 | ok, 12 files | 19.4 | 18.8 (18.8-18.8) | 119410 | 15.3 |
| parse project | generated x1/4 | 109615 | ok, 44 files | 30.1 | 28.8 (28.8-43.2) | 164300 | 20.1 |
| parse project | generated x1 | 1039805 | ok, 168 files | 258.7 | 248.9 (245.3-251.7) | 1529526 | 191.6 |
| parse project | generated x3 | 3147503 | ok, 500 files | 736.9 | 735.2 (731.9-747.3) | 4608562 | 575.7 |
| parse project | generated one program x1/16 | 127624 | ok, 11 files | 27.7 | 27.2 (27.1-27.5) | 186961 | 23.6 |
| parse project | generated one program x1/4 | 146295 | ok, 35 files | 34.4 | 34.3 (34.0-34.5) | 215651 | 27.0 |
| parse project | generated one program x1 | 222491 | ok, 130 files | 62.0 | 61.3 (61.0-61.4) | 331367 | 40.2 |
| parse project | generated one program x3 | 432878 | ok, 384 files | 140.8 | 134.0 (133.5-134.4) | 653070 | 76.8 |
| parse project (held) | generated x1/16 | 80983 | ok, 12 files | 17.2 | 17.2 (17.1-17.5) | 119362 | 15.2 |
| parse project (held) | generated x1/4 | 109615 | ok, 44 files | 23.3 | 22.9 (22.2-23.0) | 164124 | 20.0 |
| parse project (held) | generated x1 | 1039805 | ok, 168 files | 224.1 | 216.2 (214.8-242.4) | 1528854 | 191.5 |
| parse project (held) | generated x3 | 3147503 | ok, 500 files | 659.0 | 656.3 (651.8-657.1) | 4606562 | 575.5 |
| parse project (held) | generated one program x1/16 | 127624 | ok, 11 files | 25.6 | 25.6 (25.6-25.8) | 186917 | 23.6 |
| parse project (held) | generated one program x1/4 | 146295 | ok, 35 files | 29.4 | 29.7 (29.5-29.8) | 215511 | 27.0 |
| parse project (held) | generated one program x1 | 222491 | ok, 130 files | 44.9 | 44.3 (44.0-44.6) | 330847 | 40.2 |
| parse project (held) | generated one program x3 | 432878 | ok, 384 files | 85.7 | 85.1 (84.6-88.0) | 651534 | 76.7 |
| analyze | generated x1/16 | 80983 | ok, 23 elements, 0 diagnostics | 38.7 | 39.2 (38.8-40.8) | 300063 | 31.5 |
| analyze | generated x1/4 | 109615 | ok, 87 elements, 0 diagnostics | 51.0 | 51.0 (50.6-52.2) | 374251 | 37.5 |
| analyze | generated x1 | 1039805 | ok, 343 elements, 0 diagnostics | 613.2 | 599.6 (596.9-601.0) | 3362808 | 339.7 |
| analyze | generated x3 | 3147503 | ok, 1027 elements, 0 diagnostics | 1938.4 | 1939.3 (1931.1-1940.6) | 11120243 | 1152.1 |
| analyze | generated one program x1/16 | 127624 | ok, 22 elements, 0 diagnostics | 54.4 | 55.6 (53.8-55.8) | 378530 | 37.7 |
| analyze | generated one program x1/4 | 146295 | ok, 78 elements, 0 diagnostics | 64.1 | 65.2 (64.0-65.4) | 436177 | 43.4 |
| analyze | generated one program x1 | 222491 | ok, 305 elements, 0 diagnostics | 143.2 | 110.0 (109.8-111.8) | 772396 | 79.1 |
| analyze | generated one program x3 | 432878 | ok, 911 elements, 0 diagnostics | 216.7 | 216.4 (214.2-223.3) | 1333324 | 130.6 |
| analyze (held) | generated x1/16 | 80983 | ok, 23 elements, 0 diagnostics | 39.2 | 38.1 (37.8-38.4) | 300059 | 31.5 |
| analyze (held) | generated x1/4 | 109615 | ok, 87 elements, 0 diagnostics | 50.5 | 50.2 (49.7-51.7) | 374247 | 37.5 |
| analyze (held) | generated x1 | 1039805 | ok, 343 elements, 0 diagnostics | 593.5 | 597.9 (595.4-598.4) | 3362804 | 339.7 |
| analyze (held) | generated x3 | 3147503 | ok, 1027 elements, 0 diagnostics | 1944.4 | 1928.5 (1927.9-1944.0) | 11120239 | 1152.1 |
| analyze (held) | generated one program x1/16 | 127624 | ok, 22 elements, 0 diagnostics | 54.3 | 54.5 (53.3-55.0) | 378526 | 37.7 |
| analyze (held) | generated one program x1/4 | 146295 | ok, 78 elements, 0 diagnostics | 63.7 | 63.3 (62.5-63.7) | 436173 | 43.4 |
| analyze (held) | generated one program x1 | 222491 | ok, 305 elements, 0 diagnostics | 110.0 | 108.7 (108.5-109.8) | 772392 | 79.1 |
| analyze (held) | generated one program x3 | 432878 | ok, 911 elements, 0 diagnostics | 213.7 | 214.4 (213.4-218.5) | 1333320 | 130.6 |
| codegen | generated one program x1/16 | 127624 | ok, 9 functions, 65371 code bytes, 84 variables | 27.5 | 27.9 (27.7-28.5) | 198028 | 11.4 |
| codegen | generated one program x1/4 | 146295 | ok, 30 functions, 70101 code bytes, 297 variables | 32.3 | 32.5 (32.5-32.6) | 232121 | 14.6 |
| codegen | generated one program x1 | 222491 | ok, 112 functions, 94255 code bytes, 1161 variables | 74.4 | 74.4 (73.8-74.7) | 648464 | 63.7 |
| codegen | generated one program x3 | 432878 | ok, 332 functions, 156443 code bytes, 3473 variables | 296.6 | 296.0 (295.7-296.5) | 2944964 | 346.7 |
| pipeline | generated one program x1/16 | 127624 | ok, 9 functions | 114.5 | 114.9 (114.5-116.0) | 763519 | 72.7 |
| pipeline | generated one program x1/4 | 146295 | ok, 30 functions | 135.7 | 136.7 (135.9-137.3) | 883949 | 84.9 |
| pipeline | generated one program x1 | 222491 | ok, 112 functions | 255.2 | 254.6 (253.3-257.1) | 1752227 | 183.0 |
| pipeline | generated one program x3 | 432878 | ok, 332 functions | 663.6 | 671.9 (661.4-698.0) | 4931358 | 554.1 |
| pipeline (held) | generated one program x1/16 | 127624 | ok, 9 functions | 111.1 | 112.4 (111.7-122.0) | 763467 | 72.7 |
| pipeline (held) | generated one program x1/4 | 146295 | ok, 30 functions | 129.4 | 129.6 (128.1-134.8) | 883801 | 84.9 |
| pipeline (held) | generated one program x1 | 222491 | ok, 112 functions | 234.4 | 236.8 (233.9-237.0) | 1751699 | 183.0 |
| pipeline (held) | generated one program x3 | 432878 | ok, 332 functions | 613.3 | 614.5 (608.8-628.2) | 4929814 | 554.0 |

### 3.1 What the rows say

- `analyze` is linear in size: `x1` to `x3` is a factor of 3.2 in time at a
  factor of 3.03 in bytes and 3.3 in allocations.
- The thread of the stack budget is not a cost of the analysis: `analyze` and
  `analyze (held)` differ by less than 1 percent from `x1`. It is a cost of
  the parse: one thread per file, 8 to 37 percent of `parse project` (the
  held row takes 0.78 to 0.90 of the time at the project shape, 0.63 to 0.92
  at the one-program shape).
- `pipeline (held)` is 2 to 9 percent faster than `pipeline` (0.98 to 0.91 of
  its time from `x1/16` to `x3`), and what it saves is the thread of every
  file and of the stages.
- Code generation of one program grows worse than linearly: 74.5 ms at `x1`,
  296 ms at `x3`, a factor of 4.0 at a factor of 3 in declarations and of 1.95
  in bytes (exponent 1.26 in the scale). It is 24 percent of the pipeline at
  `x1/16` and 44 percent at `x3`.

### 3.2 Code generation of a project of more than one program

Code generation takes a project of one `PROGRAM`: `find_program`
(`compiler/codegen/src/compile.rs:492-518`) reports the second declaration
with `P9999` ("the compiler currently runs only one PROGRAM"), and
`check_single_program_instance` (`:528-541`) does the same for a
configuration with a second program instance. The 39-program shape is
therefore an input of the analysis only, and every figure of `codegen` and
`pipeline` above is of a project of one program with the declarations of the
same counts. That a project of the shape of the industrial one cannot be
compiled is a fact of the compiler today; this document does not measure what
it would cost.

## 4. The analysis by step

### 4.1 Whole call and share of each kind of step

Warm medians of three processes (the range over the three in parentheses).
"Live" is the memory held at the end of the cold call, when the result of the
analysis is still alive.

| input | bytes | cold ms | warm ms (median of 3 processes; min-max) | warm allocs | warm MiB allocated | live MiB at end |
|---|---|---|---|---|---|---|
| one statement (first call of the process) | 58 | 2.1 | 1.6 (1.5-1.6) | 12110 | 1.6 | 0.6 |
| generated x1/16 | 80983 | 39.9 | 39.3 (39.3-39.5) | 300063 | 31.5 | 19.2 |
| generated x1/4 | 109615 | 51.8 | 51.1 (50.9-51.9) | 374251 | 37.5 | 22.8 |
| generated x1 | 1039805 | 599.1 | 599.3 (596.1-603.7) | 3362808 | 339.7 | 157.9 |
| generated x3 | 3147503 | 1928.8 | 1921.1 (1919.5-1928.2) | 11120243 | 1152.1 | 457.8 |
| generated one program x1/16 | 127624 | 57.6 | 55.2 (54.9-55.8) | 378530 | 37.7 | 20.7 |
| generated one program x1/4 | 146295 | 65.1 | 64.8 (64.1-65.1) | 436177 | 43.4 | 23.1 |
| generated one program x1 | 222491 | 113.5 | 110.3 (109.4-112.1) | 772396 | 79.1 | 33.3 |
| generated one program x3 | 432878 | 222.7 | 219.7 (218.9-224.5) | 1333324 | 130.6 | 61.5 |

Share of the call by kind of step (warm ms, and percent of the call). `setup`
is the merge of the inputs and the two environments; `pass` is the
transforms; `rule` is the semantic rules; "not in a step" is what the call does outside every
step (the extension of the merged library, the semantic context, the
samples of the observer).

| input | setup | pass | rule | not in a step |
|---|---|---|---|---|
| one statement (first call of the process) | 1.25 (78%) | 0.09 (6%) | 0.02 (1%) | 0.24 (15%) |
| generated x1/16 | 4.44 (11%) | 18.36 (47%) | 15.85 (40%) | 0.65 (2%) |
| generated x1/4 | 5.50 (11%) | 26.24 (51%) | 18.66 (37%) | 0.70 (1%) |
| generated x1 | 43.69 (7%) | 288.57 (48%) | 262.43 (44%) | 4.61 (1%) |
| generated x3 | 122.34 (6%) | 888.12 (46%) | 910.56 (47%) | 0.08 (0%) |
| generated one program x1/16 | 5.78 (10%) | 30.72 (56%) | 17.86 (32%) | 0.84 (2%) |
| generated one program x1/4 | 6.53 (10%) | 36.26 (56%) | 20.94 (32%) | 1.07 (2%) |
| generated one program x1 | 8.58 (8%) | 58.19 (53%) | 41.81 (38%) | 1.72 (2%) |
| generated one program x3 | 13.91 (6%) | 125.59 (57%) | 77.10 (35%) | 3.10 (1%) |

At the project shape, the passes are 46 to 51 percent of the call at every
scale, the rules 37 to 47, the setup 6 to 11. Before section 9 the library
copies were 54 to 57 percent of the call, the passes 21 to 23, the rules 14 to
21, the setup 3 to 5.

### 4.2 Passes, shape `generated`

| step | calls | x1/16 | x1/4 | x1 | x3 | warm allocs at x3 | growth exponent x1 to x3 |
|---|---|---|---|---|---|---|---|
| merge sources | 500 | 3.265 | 4.314 | 42.470 | 121.137 | 1131946 | 0.95 |
| type environment | 2 | 0.205 | 0.231 | 0.219 | 0.221 | 1792 | 0.01 |
| function environment | 1 | 0.978 | 0.995 | 1.006 | 1.022 | 10020 | 0.01 |
| xform_resolve_constant_expressions | 1 | 0.956 | 1.226 | 12.907 | 38.679 | 125738 | 1.00 |
| xform_toposort_declarations | 1 | 0.314 | 0.625 | 6.066 | 19.336 | 21719 | 1.06 |
| xform_resolve_type_decl_environment | 2 | 1.772 | 2.936 | 29.477 | 88.836 | 361032 | 1.00 |
| xform_resolve_late_bound_expr_kind | 1 | 0.966 | 1.401 | 15.674 | 47.188 | 144120 | 1.00 |
| xform_resolve_late_bound_type_initializer | 1 | 0.858 | 1.172 | 16.094 | 50.273 | 128133 | 1.04 |
| xform_insert_implicit_deref | 1 | 0.936 | 1.192 | 15.158 | 45.942 | 125738 | 1.01 |
| xform_resolve_adr | 1 | 0.000 | 0.000 | 0.000 | 0.000 | 0 | - |
| xform_fold_initializer_expressions | 1 | 0.778 | 1.000 | 13.415 | 40.397 | 126186 | 1.00 |
| xform_int_to_bool_initializer | 1 | 0.000 | 0.000 | 0.000 | 0.000 | 0 | - |
| xform_resolve_symbol_and_function_environment | 1 | 0.278 | 0.916 | 6.049 | 18.732 | 107091 | 1.03 |
| xform_named_to_positional_args | 1 | 1.113 | 1.444 | 17.584 | 54.241 | 244809 | 1.03 |
| xform_resolve_decl_types | 1 | 0.789 | 1.197 | 13.633 | 42.239 | 140759 | 1.03 |
| xform_resolve_expr_types | 1 | 3.976 | 5.400 | 54.637 | 165.592 | 1526777 | 1.01 |
| xform_fold_constant_expressions | 1 | 0.909 | 1.230 | 15.221 | 46.465 | 125738 | 1.02 |
| xform_remove_unsigned_abs | 1 | 0.943 | 1.237 | 15.698 | 48.031 | 125738 | 1.02 |
| xform_resolve_type_aliases | 1 | 0.093 | 0.283 | 1.967 | 13.731 | 1695 | 1.77 |
| xform_mark_unwritten_constants | 1 | 3.515 | 4.605 | 51.066 | 153.921 | 1301711 | 1.00 |
| type_table | 1 | 0.123 | 0.273 | 3.914 | 12.365 | 25176 | 1.05 |

Before section 9 the table had a last row, the 15 passes that kept a copy of the
library (the library cloned 16 times: once before each pass and once more at
`compiler/analyzer/src/stages.rs:289`, before the second run of
`xform_resolve_type_decl_environment`). It took 2,328 ms at `x3` (17.9 million
allocations), 144 to 168 ms for one copy and its release, and is gone: no step
of the analysis keeps a copy, and the second run of
`xform_resolve_type_decl_environment` is the 2 calls of that row.

### 4.3 Rules, shape `generated`

| step | calls | x1/16 | x1/4 | x1 | x3 | warm allocs at x3 | growth exponent x1 to x3 |
|---|---|---|---|---|---|---|---|
| rule_abstract_not_instantiated | 1 | 0.000 | 0.001 | 0.004 | 0.011 | 0 | - |
| rule_assignment_aggregate_type_compat | 1 | 0.258 | 0.539 | 6.263 | 18.663 | 36779 | 0.99 |
| rule_decl_struct_element_unique_names | 1 | 0.085 | 0.122 | 2.942 | 9.326 | 971 | 1.05 |
| rule_range_limits | 1 | 0.082 | 0.107 | 2.866 | 9.405 | 0 | 1.08 |
| rule_real_literal_range | 1 | 0.082 | 0.101 | 2.899 | 9.536 | 0 | 1.08 |
| rule_enum_base_type_allowed | 1 | 0.077 | 0.100 | 2.718 | 9.077 | 0 | 1.10 |
| rule_enum_explicit_value_allowed | 1 | 0.079 | 0.097 | 2.812 | 9.233 | 0 | 1.08 |
| rule_enumeration_values_unique | 1 | 0.076 | 0.096 | 2.855 | 9.007 | 0 | 1.05 |
| rule_loop_control_inside_loop | 1 | 0.083 | 0.104 | 3.066 | 10.396 | 0 | 1.11 |
| rule_jump_target | 1 | 0.155 | 0.185 | 5.190 | 17.227 | 0 | 1.09 |
| rule_extends_field_duplicated | 1 | 0.004 | 0.009 | 0.047 | 0.140 | 728 | 0.99 |
| rule_function_block_call_unsupported | 1 | 0.079 | 0.096 | 2.631 | 8.962 | 0 | 1.12 |
| rule_function_block_invocation | 1 | 0.810 | 0.971 | 11.834 | 36.446 | 366383 | 1.02 |
| rule_function_call_declared | 1 | 0.093 | 0.115 | 3.188 | 10.280 | 5177 | 1.07 |
| rule_function_call_in_out_argument | 1 | 0.400 | 0.796 | 7.543 | 23.421 | 152703 | 1.03 |
| rule_function_call_type_check | 1 | 1.407 | 2.012 | 21.309 | 64.450 | 592545 | 1.01 |
| rule_member_qualifier_allowed | 1 | 0.076 | 0.097 | 2.833 | 9.094 | 0 | 1.06 |
| rule_member_qualifier_invalid | 1 | 0.000 | 0.000 | 0.000 | 0.000 | 0 | - |
| rule_method_call_declared | 1 | 0.090 | 0.155 | 3.137 | 10.502 | 7031 | 1.10 |
| rule_program_task_definition_exists | 1 | 0.077 | 0.098 | 2.775 | 9.086 | 1 | 1.08 |
| rule_program_var_hides_global | 1 | 0.098 | 0.145 | 3.083 | 10.166 | 2601 | 1.09 |
| rule_no_top_level_var_global | 1 | 0.000 | 0.000 | 0.000 | 0.000 | 0 | - |
| rule_operator_operand_type_check | 1 | 3.106 | 4.363 | 43.327 | 139.311 | 1523405 | 1.06 |
| rule_task_names_unique | 1 | 0.079 | 0.100 | 2.906 | 9.092 | 1 | 1.04 |
| rule_stdlib_type_redefinition | 1 | 0.098 | 0.190 | 3.182 | 10.193 | 15840 | 1.06 |
| rule_string_encoding_compat | 1 | 0.083 | 0.106 | 3.022 | 10.048 | 0 | 1.09 |
| rule_string_length_range | 1 | 0.076 | 0.097 | 2.677 | 9.056 | 0 | 1.11 |
| rule_string_literal_char_range | 1 | 0.077 | 0.096 | 2.640 | 8.960 | 0 | 1.11 |
| rule_temporal_literal_range | 1 | 0.078 | 0.098 | 2.872 | 9.603 | 0 | 1.10 |
| rule_struct_initializer_expression_allowed | 1 | 0.076 | 0.095 | 2.652 | 8.930 | 0 | 1.11 |
| rule_fb_instance_array_allowed | 1 | 0.078 | 0.104 | 2.662 | 9.182 | 728 | 1.13 |
| rule_use_declared_enumerated_value | 1 | 0.076 | 0.095 | 2.604 | 8.979 | 0 | 1.13 |
| rule_use_declared_symbolic_var | 1 | 0.263 | 0.437 | 6.891 | 22.300 | 18842 | 1.07 |
| rule_unsupported_extension | 1 | 0.080 | 0.100 | 2.936 | 9.703 | 0 | 1.09 |
| rule_var_decl_const_initialized | 1 | 0.076 | 0.096 | 2.692 | 9.009 | 0 | 1.10 |
| rule_var_decl_const_not_fb | 1 | 0.076 | 0.093 | 2.651 | 9.049 | 0 | 1.12 |
| rule_var_decl_initializer_type_compat | 1 | 0.077 | 0.098 | 2.651 | 9.198 | 0 | 1.13 |
| rule_var_decl_global_const_requires_external_const | 1 | 0.161 | 0.212 | 5.348 | 18.468 | 1729 | 1.13 |
| rule_mixed_located_var_declarations | 1 | 0.081 | 0.111 | 2.781 | 9.459 | 447 | 1.11 |
| rule_pou_hierarchy | 1 | 0.072 | 0.093 | 2.488 | 8.759 | 270 | 1.15 |
| rule_bit_and_partial_access_range | 1 | 0.151 | 0.393 | 4.070 | 13.516 | 36779 | 1.09 |
| rule_case_bit_string_label | 1 | 0.082 | 0.103 | 2.694 | 9.284 | 0 | 1.13 |
| rule_case_selector_type | 1 | 0.077 | 0.097 | 2.691 | 9.196 | 0 | 1.12 |
| rule_condition_type | 1 | 0.085 | 0.105 | 2.892 | 9.758 | 3 | 1.11 |
| rule_constant_range | 1 | 6.317 | 4.797 | 55.147 | 241.896 | 2538877 | 1.35 |
| rule_ref_to | 1 | 0.274 | 0.572 | 7.448 | 22.896 | 42479 | 1.02 |
| rule_special_operator | 1 | 0.000 | 0.000 | 0.000 | 0.000 | 0 | - |

Reading the table:

- 28 rules allocate fewer than 1,000 times and take 8.6 to 10.2 ms each at
  `x3` (2.5 to 3.2 ms at `x1`), except `rule_jump_target` (17.4 ms; it also
  collects the labels of every body, `:78`): `rule_range_limits`, `rule_real_literal_range`,
  `rule_enum_base_type_allowed`, `rule_jump_target`, `rule_pou_hierarchy` and
  the others in the table with 0 allocations. They sum to 267 ms at `x3` (81
  ms at `x1`). That is the cost of walking the library once: each rule calls
  `run_rule`, which calls `walk` on the whole library
  (`compiler/analyzer/src/rule_support.rs:45-46`), and the loop that runs the
  rules (`compiler/analyzer/src/semantic_rules.rs:92`) makes one walk per rule.
  Five rules cost nothing because an option or the input skips them
  (`rule_member_qualifier_invalid`, `rule_no_top_level_var_global`,
  `rule_special_operator`; `rule_abstract_not_instantiated` and
  `rule_extends_field_duplicated` find no abstract or derived function
  block).
- Three rules take 64 to 241 ms at `x3`: `rule_constant_range` (241 ms, 2.5
  million allocations), `rule_operator_operand_type_check` (139 ms, 1.5
  million), `rule_function_call_type_check` (64 ms, 0.6 million). Together
  they are 49 percent of the rule time.
- `rule_constant_range` grows with exponent 1.35 between `x1` and `x3`
  (54.7 to 240.7 ms) and takes 6.4 ms at `x1/16` and 4.8 ms at `x1/4` (the
  one body of 3,000 lines).

### 4.4 Passes and rules, shape `generated one program`

| step | calls | prog x1/16 | prog x1/4 | prog x1 | prog x3 | warm allocs at prog x3 | growth exponent x1 to x3 |
|---|---|---|---|---|---|---|---|
| merge sources | 384 | 4.680 | 5.297 | 7.357 | 12.667 | 129451 | 0.49 |
| type environment | 2 | 0.191 | 0.234 | 0.215 | 0.229 | 1792 | 0.06 |
| function environment | 1 | 0.926 | 1.009 | 1.020 | 1.014 | 10020 | -0.01 |
| xform_resolve_constant_expressions | 1 | 1.596 | 1.773 | 2.360 | 4.154 | 9488 | 0.51 |
| xform_toposort_declarations | 1 | 0.520 | 0.773 | 1.911 | 4.879 | 19123 | 0.85 |
| xform_resolve_type_decl_environment | 2 | 3.006 | 3.736 | 7.072 | 15.265 | 109398 | 0.70 |
| xform_resolve_late_bound_expr_kind | 1 | 1.682 | 1.942 | 3.008 | 5.934 | 20230 | 0.62 |
| xform_resolve_late_bound_type_initializer | 1 | 1.494 | 1.797 | 2.778 | 5.415 | 11883 | 0.61 |
| xform_insert_implicit_deref | 1 | 1.596 | 1.771 | 2.562 | 4.814 | 9488 | 0.57 |
| xform_resolve_adr | 1 | 0.000 | 0.000 | 0.000 | 0.000 | 0 | - |
| xform_fold_initializer_expressions | 1 | 1.364 | 1.483 | 2.184 | 4.316 | 9820 | 0.62 |
| xform_int_to_bool_initializer | 1 | 0.000 | 0.000 | 0.000 | 0.000 | 0 | - |
| xform_resolve_symbol_and_function_environment | 1 | 0.341 | 0.791 | 2.666 | 7.722 | 74979 | 0.97 |
| xform_named_to_positional_args | 1 | 1.921 | 2.163 | 3.072 | 5.669 | 21977 | 0.56 |
| xform_resolve_decl_types | 1 | 1.352 | 1.670 | 2.534 | 5.101 | 19643 | 0.64 |
| xform_resolve_expr_types | 1 | 6.481 | 7.371 | 10.797 | 20.364 | 180237 | 0.58 |
| xform_fold_constant_expressions | 1 | 1.627 | 1.954 | 2.614 | 4.820 | 9488 | 0.56 |
| xform_remove_unsigned_abs | 1 | 1.655 | 1.940 | 2.660 | 4.813 | 9488 | 0.54 |
| xform_resolve_type_aliases | 1 | 0.099 | 0.277 | 1.696 | 11.536 | 1695 | 1.75 |
| xform_mark_unwritten_constants | 1 | 5.580 | 6.557 | 9.435 | 17.667 | 137355 | 0.57 |
| type_table | 1 | 0.178 | 0.299 | 0.895 | 2.713 | 20310 | 1.01 |

| step | calls | prog x1/16 | prog x1/4 | prog x1 | prog x3 | warm allocs at prog x3 | growth exponent x1 to x3 |
|---|---|---|---|---|---|---|---|
| rule_constant_range | 1 | 4.352 | 4.942 | 15.342 | 14.126 | 127236 | -0.08 |
| rule_operator_operand_type_check | 1 | 3.454 | 3.926 | 5.752 | 11.043 | 123828 | 0.59 |
| rule_function_call_type_check | 1 | 2.220 | 2.638 | 4.255 | 9.095 | 94942 | 0.69 |
| rule_function_call_in_out_argument | 1 | 0.557 | 0.808 | 1.775 | 4.769 | 47473 | 0.90 |
| rule_ref_to | 1 | 0.395 | 0.627 | 1.688 | 4.121 | 27889 | 0.81 |
| rule_assignment_aggregate_type_compat | 1 | 0.529 | 0.734 | 1.549 | 3.823 | 24943 | 0.82 |
| rule_bit_and_partial_access_range | 1 | 0.191 | 0.335 | 0.954 | 2.858 | 24943 | 1.00 |
| rule_use_declared_symbolic_var | 1 | 0.404 | 0.522 | 0.975 | 2.486 | 13125 | 0.85 |
| rule_function_block_invocation | 1 | 1.206 | 1.206 | 1.392 | 1.890 | 17152 | 0.28 |
| rule_stdlib_type_redefinition | 1 | 0.148 | 0.228 | 0.550 | 1.702 | 15840 | 1.03 |
| rule_program_var_hides_global | 1 | 0.148 | 0.193 | 0.350 | 1.107 | 2601 | 1.05 |
| rule_var_decl_global_const_requires_external_const | 1 | 0.262 | 0.296 | 0.443 | 1.083 | 1729 | 0.81 |

`xform_resolve_type_aliases` grows with exponent 1.75 at this shape (1.7 ms
at `x1`, 11.7 ms at `x3`) and 1.79 at the other: it does not read the library
(`xform_resolve_type_aliases.rs:18` takes `_lib` and ignores it); for every
type of the type environment it looks for the first other type with the same
representation by iterating the whole environment
(`xform_resolve_type_aliases.rs:25-26` and `:88-93`), which is quadratic in
the number of types.

### 4.5 Fixed cost of one `analyze` call, and memory

The input of one statement (`PROGRAM main ... x := 1; END_PROGRAM`), the
first call of the process: 2.1 ms cold, 1.6 ms warm, 12,110 allocations,
1.6 MiB allocated.

| step | calls | cold ms | warm ms | warm allocs | warm KiB |
|---|---|---|---|---|---|
| merge sources | 1 | 0.020 | 0.006 | 15 | 1.9 |
| type environment | 2 | 0.242 | 0.187 | 1792 | 185.8 |
| function environment | 1 | 1.214 | 1.067 | 10020 | 1433.9 |

Where the fixed cost is paid:

| Cost | Where | Warm ms | Share of the call | Allocations |
|---|---|---|---|---|
| Function environment, built from the standard functions | `resolve_types_in_budget` in `stages.rs`; `FunctionEnvironmentBuilder::with_stdlib_functions`, `function_environment.rs:271` | 1.07 | 68 % | 10,020 |
| Type environment, built twice | the first and the second derivation in `resolve_types_in_budget`; both through `build_type_environment` | 0.19 (0.094 each) | 12 % | 1,792 (896 each) |
| Merge of the inputs, one clone of each input | `resolve_types_in_budget` in `stages.rs` | 0.006 for one input; 121 ms for 500 files (grows) | 0.4 % here, 6 % at `x3` | 15 per input |
| Passes and rules | `stages.rs` passes, `semantic_rules.rs:92` | 0.13 | 8 % | few |
| Outside every step: extension of the merged library, `SemanticContext::new`, the samples of the observer | `resolve_types_in_budget` in `stages.rs` | 0.17 | 10 % | |

The first two rows do not depend on the project: `function environment` is
0.98 to 1.02 ms and `type environment` 0.21 to 0.23 ms at every scale. The
process pays 0.5 ms more on its first call (2.1 ms against 1.6 ms).

Memory (private bytes of the benchmark process, peak of the sampled
values): `x1/16` and `x1/4` together 0.03 GB; up to `x1` 0.17 GB; all four
scales of both shapes 0.50 GB (working set 0.46 GB; 0.49 GB for the analysis
profile alone). Before section 9 these were 0.04, 0.22 and 0.65 GB (working set
0.60 GB). The analysis result at `x3` is 458 MiB live at the end of the call,
and the call allocates 1.1 GiB in all (3.4 GiB before section 9).

## 5. What scales and what does not

| Step | Growth with the project | Evidence |
|---|---|---|
| Parse of a project | linear | 245 ms at `x1`, 734 ms at `x3`: factor 3.0 |
| Analysis whole | linear | factor 3.2 |
| Every pass except `xform_resolve_type_aliases` | linear at the project shape (exponent 0.99 to 1.07), at most linear at the one-program shape (0.5 to 0.93; the 5,000-line body does not scale) | tables of 4.2 and 4.4 |
| `xform_resolve_type_aliases` | worse than linear (1.75 to 1.79) | quadratic in the number of types, section 4.4 |
| 28 walk-only rules | linear (exponent 1.0 to 1.13) | 2.5 to 3.2 ms at `x1`, 8.6 to 10.2 ms at `x3` (one at 5.3 and 17.4) |
| `rule_constant_range` | worse than linear at the project shape (1.35); flat at the one-program shape (15.3 ms at `x1`, 14.1 ms at `x3`: the cost is in the one big body) | tables of 4.3 and 4.4 |
| Function environment, type environment | constant | 1.0 ms and 0.2 ms at every scale |
| Merge of the inputs | linear in the size of the library, paid once | section 4.2 |
| Code generation, one program | worse than linear between `x1` and `x3` | 74.5 ms to 296 ms: factor 4.0 at a factor of 3 in declarations (exponent 1.26) |

## 6. Passes and rules that read the whole program

A **unit** is a function, a function block, a program or a type declaration.
A **local** step reads one unit and, at most, looks up types and signatures
of other units through the environments that earlier steps built. A
**collecting** step first collects a table over the whole program and then
rewrites or checks each unit with it. A **whole-program** step reads the
program by nature. For each, the fact it reads, and whether a per-unit
summary (writes, calls, lookups) can replace the read, are stated; the
evidence is the line of the code that reads it.

### 6.1 Passes (`compiler/analyzer/src`)

| Pass | Class | Reads | Per-unit summary can replace the read |
|---|---|---|---|
| `xform_resolve_constant_expressions` | collecting | `collect_constants` (`xform_resolve_constant_expressions.rs:53`, `:72-86`): name and initial value of every constant of a top-level `VAR_GLOBAL`, a configuration and a resource | Yes: the constants table is a summary of the global declarations; no body is read to build it |
| `xform_toposort_declarations` | whole-program | the graph of types and units (`:63-64`, visitors `:325-600`), including the callee of every call in a body (`visit_function`, `:549`); the order of declarations; `reachable_from` (`:236-259`), the set of declarations a `PROGRAM` reaches | Yes for the edges: a summary of the names a unit calls, instantiates and refers to. The order and the reachable set need the summaries of all units |
| `xform_resolve_type_decl_environment` | collecting | folds every type declaration and function block name into the type environment (`:34-36`); reports repeated names (`take_duplicates`, `:37`); reads no body | Yes: one summary per type declaration; duplicates need the names of all |
| `xform_resolve_late_bound_expr_kind` | collecting | `collect_enum_values` (`:24`): the values of every enumeration | Yes: summary of the enumerations declared |
| `xform_resolve_late_bound_type_initializer` | collecting | `type_to_type_kind.walk(&lib)` (`:52`): kind of every type name | Yes: summary of the types declared |
| `xform_fold_initializer_expressions` | collecting | `collect_constants` (`:73`), the same table | Yes |
| `xform_resolve_symbol_and_function_environment` | whole-program | `walk(lib)` (`:64`); handlers act on declarations (`visit_var_decl` `:136`, `visit_function_declaration` `:177`, `visit_function_block_declaration` `:268`, `visit_data_type_declaration_kind` `:300`), none on a statement; registers every symbol and function signature in the environments | Yes: summary of the declarations and signatures of a unit |
| `xform_resolve_expr_types` | collecting | `collect_inherited_fields` (`:38`) and `collect_method_return_types` (`:39`, `:70-82`): the fields a function block inherits, the return type of every method; then the type of each expression of each body | Yes for the two tables (declaration summaries); the body is local given them |
| `xform_mark_unwritten_constants` | whole-program | `FunctionBlocks::from_library` (`:74`) and the writes of every body: assignment (`:349`), `FOR` variable (`:354`), output (`:359`), function (`:371`), function block call (`:402`), method call (`:418`); a variable with no write anywhere becomes `CONSTANT` | Yes: the set of declarations a body writes (resolved through the symbol environment), per unit, which is the summary this document calls "writes". The decision needs the writes of all units |
| `xform_resolve_type_aliases` | whole-program, reads no unit | the type environment only (`:25-26`, `:88`); the library argument is unused | The input is the type environment; it is not per unit |
| `type_table` (`type_table.rs:14`) | whole-program | every type name referenced anywhere (`visit_type_name`); its result is only written to the debug log (`stages.rs:86`) | Not needed: the result is not used |
| `xform_insert_implicit_deref` | local | the `REFERENCE TO` variables of the unit being walked (`reference_to_vars`, `:47`, `:56`) | local to one unit |
| `xform_resolve_adr` | local | an expression and its operand; skipped unless `allow_adr` (`:46`) | local to one unit |
| `xform_int_to_bool_initializer` | local | a variable initializer and the type environment; skipped unless the option is set (`:37`) | local to one unit, lookup of a type |
| `xform_named_to_positional_args` | local | a call and the signature of its callee (`function_environment`, `:45`) | local given the signature (a recorded lookup) |
| `xform_resolve_decl_types` | local | a declaration and the type environment (`:35`) | local to one unit, lookup of a type |
| `xform_fold_constant_expressions` | local | an expression (`ConstantFolder` holds no state, `:31`) | local to one unit |
| `xform_remove_unsigned_abs` | local | a call of `ABS` and the type of its argument (`:32`) | local to one unit, lookup of a type |

Pass time at `x3` (886 ms, the 18 passes) by class: local steps 237 ms (27
percent), collecting steps 431 ms, whole-program steps and the two that read no
unit 218 ms, of which `xform_mark_unwritten_constants` is 154 ms
(`xform_resolve_expr_types`, 166 ms, is counted as collecting). The 16 copies
that took 2,330 ms before section 9 belonged to no class: they were a property
of the contract of a pass.

### 6.2 Semantic rules

45 of the 47 rules call `run_rule` (`rule_support.rs:45`) and so walk the
whole library; `rule_extends_field_duplicated` and
`rule_no_top_level_var_global` read the top-level elements and do not walk.
The class below is about the facts a rule reads besides the unit it is
walking.

| Class | Rules | Fact read besides the unit |
|---|---|---|
| Local: no read of another unit | `rule_range_limits`, `rule_real_literal_range`, `rule_enum_base_type_allowed`, `rule_enum_explicit_value_allowed`, `rule_enumeration_values_unique`, `rule_decl_struct_element_unique_names`, `rule_loop_control_inside_loop`, `rule_jump_target` (`check_body`, `:78`), `rule_function_block_call_unsupported`, `rule_member_qualifier_allowed`, `rule_member_qualifier_invalid`, `rule_string_encoding_compat`, `rule_string_length_range`, `rule_string_literal_char_range`, `rule_temporal_literal_range`, `rule_struct_initializer_expression_allowed`, `rule_unsupported_extension`, `rule_var_decl_const_not_fb`, `rule_mixed_located_var_declarations`, `rule_pou_hierarchy` (state is the function being walked, `:74`, `:131`), `rule_case_bit_string_label`, `rule_stdlib_type_redefinition` (the function block being walked, `:54`), `rule_no_top_level_var_global` (`:49`: each top-level `VAR_GLOBAL` is reported alone, with no walk) | none; their `apply` takes `_context` and builds a visitor with no table |
| Local given lookups of types or signatures | `rule_assignment_aggregate_type_compat`, `rule_bit_and_partial_access_range`, `rule_case_selector_type`, `rule_condition_type`, `rule_constant_range` (`function_environment`, `:105`), `rule_function_call_declared`, `rule_function_call_type_check`, `rule_function_call_in_out_argument`, `rule_operator_operand_type_check`, `rule_ref_to`, `rule_special_operator`, `rule_use_declared_enumerated_value` (`:90`), `rule_var_decl_const_initialized`, `rule_var_decl_initializer_type_compat` | the type or the signature of what a body names, through `context.types()` or `context.functions()`; the recorded lookup (name found or not, with the checksum of the declaration) is the summary |
| Collecting: a table of declarations of the whole program first | `rule_abstract_not_instantiated` (`:65`, the abstract function blocks), `rule_extends_field_duplicated` (`:63`, `collect_inherited_fields`), `rule_fb_instance_array_allowed` (`:59`), `rule_function_block_invocation` (`:58`), `rule_method_call_declared` (`:66`) (all three: `FunctionBlocks::from_library`, `callee_resolution.rs:30`), `rule_use_declared_symbolic_var` (`:78`, `collect_inherited_fields`), `rule_program_var_hides_global` (`:70`, `collect_global_var_decls`), `rule_var_decl_global_const_requires_external_const` (`:68`, `collect_global_var_decls`) | the signature of every function block (inputs, outputs, methods, base), the inherited fields, the global variables declared; all declaration facts |
| Configuration level | `rule_program_task_definition_exists` (`visit_resource_declaration`, `:55`), `rule_task_names_unique` (`:59`) | the programs bound to tasks and the tasks of a resource; no unit body |

Rule time at `x3` (906 ms) by class: local 204 ms, local given
lookups 578 ms, collecting 107 ms,
configuration level 18 ms. The 28 walk-only rules (267 ms)
are spread over all four classes.

The classification of a rule outside the lines cited is from the `apply`
function and the names of the visitor methods; no rule was found that reads
the body of another unit. The classes are a reading of the code, not a
measurement, and the two cross-checks in section 7 are the ones to rely on.

## 7. What the later stages must split

These are decisions the figures support. The first is implemented (section 9); the others are not.

1. **The copy of the library per pass was the largest single cost** (55
   percent) and was independent of what a change touches. Section 9 removes it
   from every analysis, full or selective: no pass keeps a copy, and the
   analysis at `x3` takes 0.44 of the time it took. A selective analysis would
   have paid the larger cost already had it kept the copies.
2. **The environments are two separate costs.** The function environment
   (1.0 ms) and the type environment (0.1 ms, built twice) are the same for
   every analysis; they are the whole fixed cost, and a language server that
   analyzes after every keystroke pays them each time.
3. **Semantic rules are 45 walks of the library.** A selective analysis that
   keeps the rule loop as it is walks the whole library 45 times to check one
   unit; the walk-only rules alone are 267 ms at `x3`. The rule loop must take
   the unit to check as an argument, or the rules must share one walk; the
   class table above gives the rules that need more than their unit.
4. **The summaries the later stages need are four.** (a) The declarations of
   a unit: names, types, signatures, constants, enumeration values, inherited
   fields, method return types, global variables. Nine of the 18 passes
   and 8 of the 47 rules (2 more at the configuration level) read one of
   these over the whole program. (b)
   The names a body calls, instantiates and refers to
   (`xform_toposort_declarations` `:549`, `:572`, `:598`): the edges of the
   graph and the reachable set. (c) The declarations a body writes
   (`xform_mark_unwritten_constants`): the set of writes the constant
   inference needs from every unit. (d) The lookups a body made, found or
   not (the 14 rules of the second class and the passes that look up a type
   or a signature). The declaration checksums and the names not found are
   what a reuse decision compares.
5. **Two steps are not per unit and must be handled as they are.**
   `xform_resolve_type_aliases` reads the type environment and nothing else,
   is quadratic in the number of types, and needs either an index from
   representation to type or to run only when the type environment changes.
   `type_table` builds a table that is only written to a debug log; it can be
   left to the debug path.
6. **Code generation of one unit was not the cost to remove first.** At the
   one-program shape the analysis was 51 percent of the pipeline at `x3` and
   71 percent at `x1/16`, code generation 32 and 13 percent. After section 9
   the analysis is 32 and 48 percent and code generation 44 and 24. Code
   generation grows faster than the project (exponent 1.26) and is now the
   largest stage at `x3`: the stage to measure again once the analysis is
   selective.

## 8. Reproduction and guards

- `compiler/benchmarks/tests/generated.rs` asserts that the same shape and
  scale give the same text, that no declaration holds more than 10,000
  scalars at any scale, that both shapes analyze with no diagnostic, that the
  one-program shape holds one program with an instance of every function block,
  and that it compiles.
- `compiler/benchmarks/tests/paths.rs` asserts that every set of inputs a row
  names exists, that every shape has a row over it, that a baseline runs over
  at least the inputs of the path compared with it, and that every row
  brackets exactly one call.
- `compiler/analyzer/src/observe.rs` asserts that every rule module and every
  transform module of the crate is reported to the observer under its own
  name, so the tables above cover every pass and rule by construction.
- The guards of the contract of a pass are in section 9.4.

## 9. The contract of a pass, and what a failed unit costs the others

This section records what a pass of the analysis returns, the inventory that
contract was derived from, and the guards. A **unit** is a function, a function
block, a program or a type declaration.

### 9.1 Decision and contract

When one unit has an error, every other unit is checked in full, in the check
while the user types and in the build. An error in one unit never hides,
changes or adds a message about another unit. The container is still produced
only when nothing reported a problem (`compiler/project/src/compile.rs:69-77`).

1. A pass takes the library and returns an `Outcome`
   (`compiler/analyzer/src/pass_runner.rs`): the library as far as it could
   transform it and the diagnostics it found. A problem in the user's program
   is never an `Err`.
2. The place that finds a problem still holds the node it was about to
   transform. It records the diagnostic and keeps the node as it was.
3. Nothing is given back from a copy, so the runner (`run_pass`) and the passes
   keep none. A new pass is one more `pass!` row in `stages.rs`; the runner
   reads what the pass returned, not its name or kind.
4. No step stops the analysis for a problem in the user's program. The sort of
   the declarations reports a recursive cycle for its members and goes on
   (section 10). `analyze` returns `Err` for a project without sources and for
   a failure to build the environments of the language, which no input causes.
5. Later passes and rules run on a unit that failed as on any other unit. No
   set of failed units is kept: the state a failed node is left in is the state
   every node was in when a pass reverted the library, which later steps were
   written for, and section 9.4 holds for every kind of failure found.
6. The second derivation of the type environment (`stages.rs`) keeps the
   library the pass returns, and replaces the first environment unless it found
   more than the first derivation did.

### 9.2 Inventory

For each of the 15 passes of the runner and the extra copy of `stages.rs`:
whether it could return `Err`, the sites, and whether the site held the node.
"No input found" means the site was read and inputs for it were tried (a type
file, a function block, a function, programs with errors of their own), and none
reached it.

| Pass | Could fail before | Sites | Holds the node | Now |
|---|---|---|---|---|
| `xform_resolve_constant_expressions` | no: the fold was generic in its error and none was produced | - | - | infallible fold |
| `xform_resolve_type_decl_environment` | yes | an alias, a structure or union field, an array element, array bounds, a subrange, a reference, a function block variable, an enumeration alias, a late-bound alias, each of an undeclared or invalid type (P2011, P2021, P2013, P2024, P2002, P2009); a declaration without a type; invariants | yes: the fold of the declaration borrows it to enter it | reported, declaration kept and not entered |
| `xform_resolve_late_bound_expr_kind` | yes | an assignment through `THIS^`/`SUPER^`; a function block variable expression (invariant) | yes | reported, node kept; no input found |
| `xform_resolve_late_bound_type_initializer` | yes | a declared kind of type with no resolution | yes | reported, placeholder kept; no input found |
| `xform_insert_implicit_deref` | no: every question mark passed a child's `Err` up | - | - | infallible fold |
| `xform_resolve_adr` | no: same | - | - | infallible fold |
| `xform_fold_initializer_expressions` | no: a failing initializer is already normalized to a placeholder | - | - | infallible fold |
| `xform_int_to_bool_initializer` | no | - | - | infallible fold |
| `xform_resolve_symbol_and_function_environment` | no: the visitor was `Infallible` | - | - | `Outcome` |
| `xform_named_to_positional_args` | yes | a planned argument that is absent (invariant) | no: the call is half taken apart; the argument is skipped | reported; no input found |
| `xform_resolve_decl_types` | no | - | - | infallible fold |
| `xform_resolve_expr_types` | yes | a variable `THIS^`/`SUPER^` | yes | reported, node kept untyped, every other expression typed |
| `xform_fold_constant_expressions` | yes | a constant operation with no result (P4039, P4040) | yes | reported, operation kept |
| `xform_remove_unsigned_abs` | no | - | - | infallible fold |
| `xform_resolve_type_aliases` | in the code only: a duplicate of an alias symbol; the library was returned as it was | an alias symbol insert | yes | `Outcome`; no input found |
| second derivation of the type environment | an `Err` kept the first environment | as `xform_resolve_type_decl_environment` | - | section 9.1 item 6 |

The hypothesis (a site holds the node, so it reports and keeps it) held for
every pass but one in the letter: `xform_named_to_positional_args` has taken
the call apart when its invariant fails, so it reports and skips the one
argument. The site cannot be reached.

What a reverted library was used for. After a pass returned `Err` the next
pass was given the library as it was before the failed pass, and what the failed
pass had already entered in the environments stayed. After
`xform_resolve_type_decl_environment` failed, every declaration after the
failing one in the sorted order was missing from the type environment and the
late-bound declarations stayed late bound, so the initializer pass reported
every variable of a later type as using an undeclared type. After
`xform_resolve_expr_types` or `xform_fold_constant_expressions` failed, no
expression of the library was typed or folded, and the rules that read
expression types had nothing to check in any unit.

### 9.3 What the user saw

For one failing unit among a type file, a function block, a function, a correct
program and a program with errors of its own:

| Failing unit | The failing unit, before | The other units, before |
|---|---|---|
| a declaration of an undeclared or invalid type (eight rows) | its own code (P2011, P2021, P2013, P2024, P2002, P2009) | an extra P2008 (undeclared type) in the function block and the program for the structure they use |
| `THIS^` written or read | P9999, first use only | the program loses its type errors (P4035, P4049) |
| a constant division by zero | P4039 | the program loses its P4039 or its type errors, by which unit comes first |

After the change the messages about the other units are those of the project
with the failing unit made correct (section 9.4).

### 9.4 Guards

- `compiler/analyzer/tests/failed_unit.rs`: one row for each kind of pass
  failure an input reaches (eleven), each with the failing unit first and last.
  Every message about the other units must equal the one with the unit made
  correct.
- `compiler/analyzer/tests/no_library_copy.rs`: the count of cloned library
  elements (`ironplc_dsl::common::library_element_clones`, built only with the
  feature `count-library-clones`, which this test enables as a dev dependency
  of the analyzer; a product build has the derived `Clone`), read around every
  step the analysis reports, is zero for every step but the merge of the inputs.
  A copy is a clone of elements however it is written, and the observer sees
  every step, so a pass added later is held to it with no edit.
- `compiler/benchmarks/tests/analysis_pins.rs`: the library (as a digest) and
  the diagnostics of every program of the corpus alone, under the default
  options and under the options that allow every extension, and of the
  generated project at the smallest scale in both shapes, against data recorded
  before the change. No program without a diagnostic changed. Two programs with
  diagnostics did: `oop.st` under the options that allow every extension is
  reported once for each `THIS^` or `SUPER^` it uses instead of once, and
  `type_decl.st` reports P2013 for an array of an undeclared type, which the
  first error in the file hid.

### 9.5 What is still seen

A unit that uses a name the failing unit would have declared drew a message of
its own: a variable of a type whose declaration failed was reported as using an
undeclared type (P2008) besides the message about the declaration. Section 10
records the choice made about what the engineer sees and the mechanism that
implements it; the messages that remain are listed in section 10.7.

## 10. A declaration with an error: the first cause only

Section 9.5 left two things to a choice about what the engineer sees. The
choice is made: **a recursive cycle is the error of its members only**, and **a
use of a declaration that has an error shows the first cause only**. This
section is the inventory the mechanism was derived from (10.1 to 10.4) and the
mechanism (10.5). Where it speaks of a "message today" it means the analysis
before this section's change, measured on programs of the shape of
`compiler/analyzer/tests/failed_unit.rs`.

### 10.1 Where an undeclared name is reported

A name of a declaration the analysis cannot find is reported from these sites
(the construction of the diagnostic, outside tests):

| Kind of name | Code | Site | Lookup it makes |
|---|---|---|---|
| type, alias parent | P2011 | `xform_resolve_type_decl_environment.rs` (late-bound alias, simple, function-block alias, structure alias, function block variable) | `TypeEnvironment::get` |
| type, alias parent | P2012 | `type_environment.rs` `insert_alias` | `TypeEnvironment::get` |
| type, reference target | P2011 | `type_environment.rs` `resolve_reference_target` | `TypeEnvironment::get` |
| type, subrange parent | P2011 | `intermediates/subrange.rs` | `TypeEnvironment::get` |
| type, array element and parent | P2013, P2011 | `intermediates/array.rs` | `TypeEnvironment::get` |
| type, structure member | P2021 | `intermediates/structure.rs` (six sites) | `TypeEnvironment::get` |
| enumeration, parent | P2009 | `xform_resolve_type_decl_environment.rs` | `TypeEnvironment::get` |
| type of a variable | P2008 | `xform_resolve_late_bound_type_initializer.rs` (initialized form and bare form) | `TypeEnvironment::get`, then the pass's own table of the declarations of the library |
| enumeration of a variable | P2004 | `rule_use_declared_enumerated_value.rs` | `TypeEnvironment::is_enumeration` |
| function | P4017 | `rule_function_call_declared.rs` | `FunctionEnvironment::contains` |
| function block | P4012 | `rule_function_block_invocation.rs`, `rule_method_call_declared.rs` | the function blocks of the library, and the instances in scope |
| method | P4046 | `rule_method_call_declared.rs` | the function block and its `EXTENDS` chain, from the library |
| variable, enumeration value | P4007 | `rule_use_declared_symbolic_var.rs` | the scoped declarations of the unit, and `SymbolEnvironment` |
| constant of a type parameter | P4030 | `xform_resolve_constant_expressions.rs` | the constants of the library |
| named argument | P4023 | `xform_named_to_positional_args.rs` | `FunctionSignature` |
| input, output of a call | P4002, P4004 | `call_assignment_check.rs` | the declaration of the function block |
| program of a task | P4006 | `rule_program_task_definition_exists.rs` | the programs of the library |

`TypeEnvironment::get_memory_size` and `validate_type_usage` also report P2025;
nothing calls them.

### 10.2 What each lookup structure holds for a declaration that failed

| Structure | Built from | A declaration that failed |
|---|---|---|
| `TypeEnvironment` (`type_environment.rs`): types, function blocks, interfaces, by name and by `TypeId` | the type declaration pass | not entered: the name is absent, as if never declared |
| `FunctionEnvironment` (`function_environment.rs`): signatures by name | the declarations of the library | a signature is built from names, so it exists whatever its parameter types are; the parameter type is a name the type environment may not hold |
| `SymbolEnvironment` (`symbol_environment.rs`): variables, enumeration values, structure fields, types, programs | the declarations of the library | entered: it reads the library, which keeps a failed declaration |
| `TypeDefinitionKind` table of `xform_resolve_late_bound_type_initializer.rs` | the declarations of the library | entered, except a late-bound alias (`TYPE T : T_NOWHERE`), which is not in it |
| function blocks and methods of the library (`rule_function_block_invocation.rs`, `rule_method_call_declared.rs`) | the library | entered |
| dependency graph of `xform_toposort_declarations.rs` | the library | every declaration, whatever its problem |

So the structure that owns the names of types loses the fact that a name was
declared when its declaration fails, and answers "absent" to every later
question. That is the source of every extra message in 10.3.

### 10.3 What the engineer sees today

For a program `user.st` that uses a failed declaration `X` in every way the
language allows (a variable of it, a parameter of a function and of a function
block, a member of a structure, an element of an array, an alias of it, the
variable in an assignment and as an argument) and has one error of its own
(`k := 'text'`):

| Declaration with an error | First cause | Extra messages today, in the using units |
|---|---|---|
| alias of an undeclared type | P2011 at `X` | P2011 for an alias of `X`, P2013 for an array of `X`, P2021 for a structure member of `X`, P2008 for every variable of `X` and of those, P4026 for the call `G(1)` |
| structure with a member of an undeclared type | P2021, P2008 at the member | the same, without the P2008 of `X` |
| alias of an undeclared enumeration | P2009 | the same, and P2004 for each variable of `X` |
| subrange with bounds in the wrong order | P2002 | P2011, P2013, P2021, P2008 (the alias, array, structure, variables), P4026 |
| array of an undeclared element type | P2013 | the same |
| array with bounds in the wrong order | P2024 | the same |
| reference to an undeclared type | P2011 | the same; the P4026 names `T_NOWHERE` |
| function block with a variable of an undeclared type | P2011 or P2008 at the variable | none that depends on the error (P2021 for a function block as a structure member and P4054 for one as a function parameter are reported for a correct function block too) |
| function with a parameter of an undeclared type | P2008 at the parameter | P4026 at every call, with `expected=T_NOWHERE` |
| recursive cycle among types, among function blocks, among functions, or a type or function block that holds itself | P4005 at one member | the analysis stops at the cycle: no message about any other unit |

`FUNCTION F : T_NOWHERE` is not reported anywhere (the return type is not looked
up); that is not changed here.

### 10.4 What the sort refuses

`xform_toposort_declarations::apply` returns `Err` (the `direct!` line of
`resolve_types_in_budget` in `stages.rs` passes it up, and `analyze` returns it
as its `Err`) for three inputs:

- a cycle in the graph of declarations, found by `toposort` in
  `DeclarationsGraph::sorted_ids`: P4005, at one node of the cycle (the label
  says "Cycle"; the node is the one `toposort` returns, not every member);
- a function call met where no declaration is being visited, in
  `visit_function`: P9999, "Function call outside a program organization unit";
- a function block instance met where no declaration is being visited, in
  `visit_function_block_initial_value_assignment`: P9999, "Function block
  instance outside a program organization unit".

The parser produces neither of the last two for any text: every declaration
that can hold an expression or an instance sets the context the visitor reads.
They are reachable only from a tree built by hand.

### 10.5 The mechanism

A lookup of a declared name has three results, not two: **declared and valid**,
**declared with an error** and **not declared**. The structure that owns the
names owns the fact.

- `TypeEnvironment` records a declaration that failed as an entry of its own (a
  `TypeId`, the name and the place of the declaration) whose state is *failed*,
  entered by the code that enters a valid type (`insert_failed` beside
  `insert_type`, both through `TypeEnvironment::enter`, which also reports a
  repeated name). `lookup` answers `Resolved::Valid`, `Resolved::Failed` or
  `Resolved::Absent` (`compiler/analyzer/src/resolution.rs`); `get` and
  `get_by_id` answer a type that can be used only, so a reader that wants a
  representation never meets a failed one, and a reader that does not know about
  failed declarations treats one as a type it cannot resolve, as it always
  treated one.
- The error value is that `TypeId`; there is no variant of `IntermediateType`
  for it. A variable of a failed type is declared with it
  (`xform_resolve_decl_types`) and an expression that reads the variable has it
  as its type. `TypeEnvironment::is_error` is the one definition of "is error".
  The checks already treat a type they cannot resolve as "nothing to compare"
  (`value_type::of` is `None`), so an expression of the error type draws no
  message from them.
- A message "not declared" is made from `Resolved::Absent` only. A declaration
  made from another (an alias, an array element, a structure member, a
  reference target, a function block variable) gets the declaration from
  `Resolved::or_failure`, which answers `Failure::Inherited` for
  `Resolved::Failed`: the declaration fails too and says nothing, because the
  first cause is already in the report. `xform_resolve_type_decl_environment`
  enters a failed declaration whatever the failure was.
- The checks that read a type by name read the error type through the lookup:
  `value_type::check` takes a required type that is failed, or is not declared
  and is not a generic category, as the error type; the variable type of
  `xform_resolve_late_bound_type_initializer` is reported only when `Absent`;
  `rule_use_declared_enumerated_value`, `rule_ref_to` (dereference and `NULL`)
  and the callee of a call or a method call (`callee_resolution::InstanceTypes`,
  read by `rule_function_block_invocation` and `rule_method_call_declared`) do
  not report that a failed type is not of the kind they require.
- A function is declared with a valid signature whatever its parameter types
  are, so a call of it is checked for what it passes, and a parameter of a type
  that is failed or not declared is the error type. A function is not entered
  as failed for a cycle either: the problem is in the order of the calls, not in
  its signature.
- A cycle is reported once, by `DeclarationsGraph::order`: at the first member
  in source order, with the other members as secondary locations and every name
  in the message. The code is P4005, as before. The sort enters each member that
  is a type, an interface or a function block in the type environment as a
  failed declaration, before any later step looks at the library
  (`xform_toposort_declarations::apply`, `Declares::enter_failed`), then orders
  the declarations that are not members as if the cycle were not there and puts
  the members last. A library without a cycle is ordered by the same
  `petgraph::algo::toposort` as before, so a correct program has the same
  library. A construct the sort does not support is reported for the
  declaration that holds it, which is entered as failed too, and the walk goes
  on.
- No step after the sort goes round a cycle: the guard runs every row of a
  cycle under a time limit, and a function that calls itself, two that call each
  other, a type that holds itself and function blocks that hold each other are
  rows.

### 10.6 What the engineer sees now

For the programs of 10.3 the first cause is the only message about a failed
declaration, and a program with an error of its own still reports it:

| Input | Before | After |
|---|---|---|
| `TYPE T : T_NOWHERE;` and `PROGRAM main VAR v : T; END_VAR v := 1; END_PROGRAM` | P2011 at `T`, and P2008 at `v` | P2011 at `T` |
| `TYPE A : STRUCT b : B; END_STRUCT; END_TYPE`, `TYPE B : STRUCT a : A; END_STRUCT; END_TYPE` and `main` with `x := 'text'` | P4005 (at `B`), and nothing about `main` | P4005 at `A`, with `B` as a secondary location and `members=A, B`, and P4035 for `x := 'text'` |
| a function with a parameter of an undeclared type, called | P2008 at the parameter, and P4026 at every call | P2008 at the parameter |
| a function that calls itself, or two functions that call each other | P4005, and nothing else in the project | P4005, and everything else in the project |

### 10.7 What is still seen

- A structure with a member of an undeclared type is reported twice at the
  member, once by the declaration of the structure (P2021) and once by the
  resolution of the member's type (P2008). Both are about the declaration that
  has the error, not about its uses.
- A variable of a type that is not declared (not one declared with an error) is
  reported at each declaration (P2008) and its uses are not analyzed as of the
  error type: a dereference or a call through it is reported as before.
- A name used as the value of an enumeration whose declaration failed because
  its base enumeration is not declared (`TYPE E : E_NOWHERE := A1`) is reported
  as an undefined variable (P4007): the value is declared nowhere.
- An enumeration that repeats a value is not a declaration that failed: the
  type is usable, and the repeat is reported by `rule_enumeration_values_unique`.
- A function whose return type is not declared is not reported (10.3).
- The order of the messages about the other units follows the order of the
  declarations, and a cycle takes edges out of the graph that order is made
  from, so the messages about the other units are the same as with the cycle
  broken but not always in the same order.

### 10.8 Guards

`compiler/analyzer/tests/failed_unit.rs` holds the rows. Each kind of failed
declaration (an alias, a structure, an enumeration alias, a subrange, an array
with an undeclared element and with bounds in the wrong order, a reference, a
function block, a function) is used by a file in every way the language allows
and with an error of its own, and the messages about every other unit must be
those of the project with the declaration correct, in the same order, with the
declaration first and last. A cycle of two types, of three, a type that holds
itself, two aliases, an array of itself, two function blocks, a function block
that holds itself, two functions and a function that calls itself are rows of a
second table, which compares the messages about the other units as a set and
runs each analysis under a time limit. The tests of the rules that report that
a type is not of a kind are in the rule modules.
