# Analysis Cost Measurement

status: implemented
date: 2026-10-08

This document records what one analysis and one code generation cost on a
large project: how the project is made, how the cost is measured, the figures
by pass and by rule, what each pass and rule reads of the whole program, and
what the figures say about splitting the analysis by unit. It states facts that
were measured and decisions the measurement forced. It changes no product
code: the analysis reports its steps to an observer
([`compiler/analyzer/src/observe.rs`](../../compiler/analyzer/src/observe.rs)),
and everything else lives in `compiler/benchmarks`.

Earlier measurements of the front end (lexing, parsing, lowering) are in
[S0 Experiment](parse-tree-s0-experiment.md) section 3. The budget for the
parse path is [Parse-Tree Architecture](parse-tree-architecture.md) section
5.2. This document extends the measurement from the parse to the analysis and
to code generation.

## 1. Summary

1. A call of `analyze` grows with an exponent of at most 1.13 in the scale
   for every pass and every rule but two (`xform_resolve_type_aliases`,
   exponent 1.79, quadratic in the number of types; `rule_constant_range`,
   1.35), and the analysis is the largest stage: on the 39-program project at its largest scale
   (3.1 MB, 500 files, 1,027 library elements) parse takes 0.73 s, analysis
   4.38 s. Parse, analysis and code generation are 14, 51 and 32 percent of the
   pipeline of the one-program shape at `x3` (section 3).
2. **55 percent of the analysis is copies of the library.** Each pass keeps a
   clone of the library so that a failure can give it back
   (`compiler/analyzer/src/pass_runner.rs:63`, `:91`), and the analysis makes
   16 such clones. At the largest scale one clone and its release cost 144 to
   168 ms. The merge of the inputs, which clones every input once
   (`stages.rs:127`), costs 3 percent more.
3. **28 of the 47 semantic rules do nothing but walk the library once each**:
   at the largest scale 9 ms each, 267 ms in all, almost no allocation. 45 of
   the 47 rules are a walk of their own over the whole tree
   (`rule_support.rs:45`).
4. The fixed cost of one `analyze` call on a program of one statement is
   1.7 ms warm and 2.2 ms cold. 66 percent of it is the function
   environment (`stages.rs:134`), 12 percent the type environment, built twice
   (`stages.rs:132`, `:287`). The fixed cost does not grow with the project.
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
| Commit | 1a126a3c9 (the table of paths, the generator and the profile harness) |
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
  sample before and a sample after the step. A step is a pass, a rule, the
  copy a pass keeps (`Fallback`) or the making of an input (`Setup`: merge,
  type environment, function environment). The whole call is measured around
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
| parse project | generated x1/16 | 80983 | ok, 12 files | 19.3 | 18.8 (18.5-19.1) | 119410 | 15.3 |
| parse project | generated x1/4 | 109615 | ok, 44 files | 30.1 | 28.0 (28.0-30.2) | 164300 | 20.1 |
| parse project | generated x1 | 1039805 | ok, 168 files | 249.4 | 245.0 (244.4-248.8) | 1529526 | 191.6 |
| parse project | generated x3 | 3147503 | ok, 500 files | 744.0 | 734.1 (725.8-739.7) | 4608562 | 575.7 |
| parse project | generated one program x1/16 | 127624 | ok, 11 files | 28.1 | 27.8 (27.6-28.2) | 186961 | 23.6 |
| parse project | generated one program x1/4 | 146295 | ok, 35 files | 36.6 | 34.3 (34.0-34.7) | 215651 | 27.0 |
| parse project | generated one program x1 | 222491 | ok, 130 files | 69.4 | 60.5 (60.1-61.0) | 331367 | 40.2 |
| parse project | generated one program x3 | 432878 | ok, 384 files | 139.9 | 134.7 (132.6-134.7) | 653070 | 76.8 |
| parse project (held) | generated x1/16 | 80983 | ok, 12 files | 16.1 | 16.8 (16.3-17.4) | 119362 | 15.2 |
| parse project (held) | generated x1/4 | 109615 | ok, 44 files | 21.9 | 21.9 (21.9-22.2) | 164124 | 20.0 |
| parse project (held) | generated x1 | 1039805 | ok, 168 files | 214.8 | 214.3 (213.4-215.7) | 1528854 | 191.5 |
| parse project (held) | generated x3 | 3147503 | ok, 500 files | 654.4 | 657.7 (657.6-666.8) | 4606562 | 575.5 |
| parse project (held) | generated one program x1/16 | 127624 | ok, 11 files | 25.8 | 25.7 (25.2-25.9) | 186917 | 23.6 |
| parse project (held) | generated one program x1/4 | 146295 | ok, 35 files | 29.9 | 29.5 (29.4-29.5) | 215511 | 27.0 |
| parse project (held) | generated one program x1 | 222491 | ok, 130 files | 44.6 | 44.3 (43.9-44.6) | 330847 | 40.2 |
| parse project (held) | generated one program x3 | 432878 | ok, 384 files | 85.8 | 84.7 (84.2-85.3) | 651534 | 76.7 |
| analyze | generated x1/16 | 80983 | ok, 23 elements, 0 diagnostics | 96.0 | 95.4 (95.3-99.8) | 764573 | 91.6 |
| analyze | generated x1/4 | 109615 | ok, 87 elements, 0 diagnostics | 129.7 | 128.2 (127.6-128.9) | 978467 | 114.9 |
| analyze | generated x1 | 1039805 | ok, 343 elements, 0 diagnostics | 1409.7 | 1408.4 (1405.9-1413.2) | 9283364 | 1107.5 |
| analyze | generated x3 | 3147503 | ok, 1027 elements, 0 diagnostics | 4385.3 | 4384.4 (4383.9-4493.4) | 28990430 | 3464.9 |
| analyze | generated one program x1/16 | 127624 | ok, 22 elements, 0 diagnostics | 149.5 | 150.9 (150.5-151.5) | 1112095 | 133.8 |
| analyze | generated one program x1/4 | 146295 | ok, 78 elements, 0 diagnostics | 171.1 | 170.2 (170.0-172.0) | 1245735 | 150.3 |
| analyze | generated one program x1 | 222491 | ok, 305 elements, 0 diagnostics | 256.1 | 257.0 (256.2-259.0) | 1904265 | 229.0 |
| analyze | generated one program x3 | 432878 | ok, 911 elements, 0 diagnostics | 475.1 | 476.4 (476.2-478.9) | 3373977 | 401.1 |
| analyze (held) | generated x1/16 | 80983 | ok, 23 elements, 0 diagnostics | 94.2 | 95.7 (94.8-96.4) | 764569 | 91.6 |
| analyze (held) | generated x1/4 | 109615 | ok, 87 elements, 0 diagnostics | 125.4 | 126.2 (122.9-129.1) | 978463 | 114.9 |
| analyze (held) | generated x1 | 1039805 | ok, 343 elements, 0 diagnostics | 1410.6 | 1407.7 (1401.0-1408.0) | 9283360 | 1107.5 |
| analyze (held) | generated x3 | 3147503 | ok, 1027 elements, 0 diagnostics | 4394.0 | 4381.0 (4373.3-4382.4) | 28990426 | 3464.9 |
| analyze (held) | generated one program x1/16 | 127624 | ok, 22 elements, 0 diagnostics | 152.9 | 148.8 (148.6-149.5) | 1112091 | 133.8 |
| analyze (held) | generated one program x1/4 | 146295 | ok, 78 elements, 0 diagnostics | 171.1 | 169.9 (168.2-170.4) | 1245731 | 150.3 |
| analyze (held) | generated one program x1 | 222491 | ok, 305 elements, 0 diagnostics | 257.9 | 255.0 (253.2-257.4) | 1904261 | 229.0 |
| analyze (held) | generated one program x3 | 432878 | ok, 911 elements, 0 diagnostics | 475.8 | 473.3 (470.8-475.9) | 3373973 | 401.1 |
| codegen | generated one program x1/16 | 127624 | ok, 9 functions, 65371 code bytes, 84 variables | 27.6 | 27.8 (27.6-28.0) | 198028 | 11.4 |
| codegen | generated one program x1/4 | 146295 | ok, 30 functions, 70101 code bytes, 297 variables | 32.5 | 32.0 (31.9-32.3) | 232121 | 14.6 |
| codegen | generated one program x1 | 222491 | ok, 112 functions, 94255 code bytes, 1161 variables | 74.2 | 74.5 (73.3-74.7) | 648464 | 63.7 |
| codegen | generated one program x3 | 432878 | ok, 332 functions, 156443 code bytes, 3473 variables | 295.3 | 296.2 (295.3-297.6) | 2944964 | 346.7 |
| pipeline | generated one program x1/16 | 127624 | ok, 9 functions | 210.1 | 212.5 (212.0-224.4) | 1497084 | 168.9 |
| pipeline | generated one program x1/4 | 146295 | ok, 30 functions | 243.5 | 246.4 (245.3-246.8) | 1693507 | 191.9 |
| pipeline | generated one program x1 | 222491 | ok, 112 functions | 412.4 | 402.5 (399.8-409.1) | 2884096 | 333.0 |
| pipeline | generated one program x3 | 432878 | ok, 332 functions | 927.4 | 930.2 (926.6-930.7) | 6972011 | 824.6 |
| pipeline (held) | generated one program x1/16 | 127624 | ok, 9 functions | 205.3 | 209.2 (207.6-209.7) | 1497032 | 168.9 |
| pipeline (held) | generated one program x1/4 | 146295 | ok, 30 functions | 234.4 | 235.7 (234.1-236.0) | 1693359 | 191.9 |
| pipeline (held) | generated one program x1 | 222491 | ok, 112 functions | 382.6 | 381.3 (378.8-381.6) | 2883568 | 332.9 |
| pipeline (held) | generated one program x3 | 432878 | ok, 332 functions | 873.8 | 869.8 (864.5-870.4) | 6970467 | 824.5 |

### 3.1 What the rows say

- `analyze` is linear in size: `x1` to `x3` is a factor of 3.1 in time at a
  factor of 3.03 in bytes and 3.1 in allocations.
- The thread of the stack budget is not a cost of the analysis: `analyze` and
  `analyze (held)` differ by less than 1 percent from `x1`. It is a cost of
  the parse: one thread per file, 8 to 37 percent of `parse project` (the
  held row takes 0.78 to 0.90 of the time at the project shape, 0.63 to 0.92
  at the one-program shape).
- `pipeline (held)` is 2 to 7 percent faster than `pipeline` (0.98 to 0.93 of
  its time from `x1/16` to `x3`), and what it saves is the thread of every
  file and of the stages.
- Code generation of one program grows worse than linearly: 74.5 ms at `x1`,
  296 ms at `x3`, a factor of 4.0 at a factor of 3 in declarations and of 1.95
  in bytes (exponent 1.26 in the scale). It is 13 percent of the pipeline at
  `x1/16` and 32 percent at `x3`.

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
| one statement (first call of the process) | 58 | 2.2 | 1.7 (1.6-1.7) | 12350 | 1.6 | 0.6 |
| generated x1/16 | 80983 | 103.2 | 102.7 (102.6-103.3) | 764573 | 91.6 | 19.2 |
| generated x1/4 | 109615 | 137.4 | 130.7 (130.5-130.9) | 978467 | 114.9 | 22.8 |
| generated x1 | 1039805 | 1437.3 | 1402.3 (1402.2-1404.6) | 9283364 | 1107.5 | 157.9 |
| generated x3 | 3147503 | 4335.7 | 4346.8 (4346.2-4351.6) | 28990430 | 3464.9 | 457.3 |
| generated one program x1/16 | 127624 | 154.3 | 151.0 (148.5-151.8) | 1112095 | 133.8 | 20.7 |
| generated one program x1/4 | 146295 | 175.7 | 170.9 (168.9-171.2) | 1245735 | 150.3 | 23.1 |
| generated one program x1 | 222491 | 256.7 | 256.2 (255.6-256.3) | 1904265 | 229.0 | 33.2 |
| generated one program x3 | 432878 | 484.5 | 475.9 (475.1-476.8) | 3373977 | 401.1 | 61.4 |

Share of the call by kind of step (warm ms, and percent of the call). `setup`
is the merge of the inputs and the two environments; `pass` is the
transforms; `fallback` is the clone each pass keeps and its release; `rule`
is the semantic rules; "not in a step" is what the call does outside every
step (the extension of the merged library, the semantic context, the
samples of the observer).

| input | setup | pass | fallback | rule | not in a step |
|---|---|---|---|---|---|
| one statement (first call of the process) | 1.30 (80%) | 0.10 (6%) | 0.03 (2%) | 0.03 (2%) | 0.17 (10%) |
| generated x1/16 | 4.71 (5%) | 21.77 (21%) | 58.48 (57%) | 15.97 (16%) | 2.03 (2%) |
| generated x1/4 | 5.52 (4%) | 30.59 (23%) | 73.66 (56%) | 18.75 (14%) | 2.30 (2%) |
| generated x1 | 40.05 (3%) | 309.32 (22%) | 768.95 (55%) | 261.48 (19%) | 22.61 (2%) |
| generated x3 | 116.66 (3%) | 937.92 (22%) | 2330.38 (54%) | 906.27 (21%) | 57.93 (1%) |
| generated one program x1/16 | 5.66 (4%) | 35.72 (24%) | 88.92 (59%) | 18.04 (12%) | 2.37 (2%) |
| generated one program x1/4 | 6.09 (4%) | 40.91 (24%) | 99.07 (58%) | 21.08 (12%) | 3.09 (2%) |
| generated one program x1 | 7.72 (3%) | 62.75 (24%) | 138.90 (54%) | 41.81 (16%) | 5.03 (2%) |
| generated one program x3 | 13.18 (3%) | 127.77 (27%) | 250.55 (53%) | 75.59 (16%) | 8.81 (2%) |

At the project shape, the library copies are 54 to 57 percent of the call at
every scale, the passes 21 to 23, the rules 14 to 21, the setup 3 to 5.

### 4.2 Passes, shape `generated`

| step | calls | x1/16 | x1/4 | x1 | x3 | warm allocs at x3 | growth exponent x1 to x3 |
|---|---|---|---|---|---|---|---|
| merge sources | 500 | 3.427 | 4.329 | 38.814 | 115.401 | 1131946 | 0.99 |
| type environment | 2 | 0.210 | 0.190 | 0.197 | 0.198 | 1792 | 0.00 |
| function environment | 1 | 1.055 | 0.975 | 0.987 | 1.021 | 10020 | 0.03 |
| xform_resolve_constant_expressions | 1 | 1.059 | 1.347 | 14.132 | 42.665 | 125738 | 1.01 |
| xform_toposort_declarations | 1 | 0.371 | 0.725 | 6.267 | 19.360 | 21719 | 1.03 |
| xform_resolve_type_decl_environment | 2 | 2.239 | 3.476 | 31.970 | 95.371 | 361032 | 0.99 |
| xform_resolve_late_bound_expr_kind | 1 | 1.190 | 1.698 | 16.927 | 50.671 | 144120 | 1.00 |
| xform_resolve_late_bound_type_initializer | 1 | 1.254 | 1.667 | 20.021 | 59.939 | 128133 | 1.00 |
| xform_insert_implicit_deref | 1 | 1.167 | 1.501 | 16.446 | 49.756 | 125738 | 1.01 |
| xform_resolve_adr | 1 | 0.000 | 0.001 | 0.000 | 0.000 | 0 | - |
| xform_fold_initializer_expressions | 1 | 1.062 | 1.369 | 14.965 | 44.528 | 126186 | 0.99 |
| xform_int_to_bool_initializer | 1 | 0.000 | 0.000 | 0.000 | 0.000 | 0 | - |
| xform_resolve_symbol_and_function_environment | 1 | 0.357 | 1.040 | 6.262 | 18.882 | 107091 | 1.00 |
| xform_named_to_positional_args | 1 | 1.398 | 1.760 | 19.027 | 57.103 | 244809 | 1.00 |
| xform_resolve_decl_types | 1 | 1.034 | 1.415 | 14.900 | 44.452 | 140759 | 0.99 |
| xform_resolve_expr_types | 1 | 4.267 | 5.720 | 56.607 | 169.505 | 1526777 | 1.00 |
| xform_fold_constant_expressions | 1 | 1.226 | 1.576 | 17.201 | 51.538 | 125738 | 1.00 |
| xform_remove_unsigned_abs | 1 | 1.221 | 1.593 | 17.469 | 52.174 | 125738 | 1.00 |
| xform_resolve_type_aliases | 1 | 0.101 | 0.300 | 1.960 | 13.972 | 1695 | 1.79 |
| xform_mark_unwritten_constants | 1 | 3.729 | 5.123 | 50.916 | 153.537 | 1301711 | 1.00 |
| type_table | 1 | 0.124 | 0.278 | 3.975 | 12.370 | 25176 | 1.03 |
| library copies of the passes (15 passes, clone and release) | 31 | 58.120 | 73.624 | 767.379 | 2327.841 | 17870187 | 1.01 |

`library copies of the passes` is the sum of the 15 passes that keep a copy
(the library is cloned 16 times: once before each pass and once more at
`compiler/analyzer/src/stages.rs:289`, before the second run of
`xform_resolve_type_decl_environment`). The row of a pass in the table is the
pass; its copy is in the last row. One copy and its release take 144 to 168
ms at `x3` (the two copies of `xform_resolve_type_decl_environment`, the pass
and the repeat at `stages.rs:289`, 254 ms together), with 1.08 to 1.13
million allocations each.

### 4.3 Rules, shape `generated`

| step | calls | x1/16 | x1/4 | x1 | x3 | warm allocs at x3 | growth exponent x1 to x3 |
|---|---|---|---|---|---|---|---|
| rule_abstract_not_instantiated | 1 | 0.001 | 0.001 | 0.004 | 0.011 | 0 | 0.92 |
| rule_assignment_aggregate_type_compat | 1 | 0.285 | 0.616 | 6.161 | 18.544 | 36779 | 1.00 |
| rule_decl_struct_element_unique_names | 1 | 0.088 | 0.171 | 2.941 | 9.438 | 971 | 1.06 |
| rule_range_limits | 1 | 0.083 | 0.112 | 2.949 | 9.323 | 0 | 1.05 |
| rule_real_literal_range | 1 | 0.082 | 0.105 | 2.904 | 9.356 | 0 | 1.06 |
| rule_enum_base_type_allowed | 1 | 0.079 | 0.099 | 2.747 | 8.987 | 0 | 1.08 |
| rule_enum_explicit_value_allowed | 1 | 0.077 | 0.098 | 2.743 | 9.448 | 0 | 1.13 |
| rule_enumeration_values_unique | 1 | 0.078 | 0.097 | 2.663 | 8.984 | 0 | 1.11 |
| rule_loop_control_inside_loop | 1 | 0.085 | 0.105 | 3.096 | 10.232 | 0 | 1.09 |
| rule_jump_target | 1 | 0.157 | 0.187 | 5.341 | 17.375 | 0 | 1.07 |
| rule_extends_field_duplicated | 1 | 0.003 | 0.010 | 0.047 | 0.164 | 728 | 1.14 |
| rule_function_block_call_unsupported | 1 | 0.079 | 0.098 | 2.641 | 9.004 | 0 | 1.12 |
| rule_function_block_invocation | 1 | 0.779 | 0.959 | 11.703 | 36.175 | 366383 | 1.03 |
| rule_function_call_declared | 1 | 0.095 | 0.119 | 3.210 | 10.605 | 5177 | 1.09 |
| rule_function_call_in_out_argument | 1 | 0.396 | 0.779 | 7.454 | 23.675 | 152703 | 1.05 |
| rule_function_call_type_check | 1 | 1.384 | 1.970 | 21.078 | 63.802 | 592545 | 1.01 |
| rule_member_qualifier_allowed | 1 | 0.078 | 0.099 | 2.888 | 9.153 | 0 | 1.05 |
| rule_member_qualifier_invalid | 1 | 0.000 | 0.000 | 0.000 | 0.000 | 0 | - |
| rule_method_call_declared | 1 | 0.090 | 0.154 | 3.209 | 10.171 | 7031 | 1.05 |
| rule_program_task_definition_exists | 1 | 0.078 | 0.099 | 2.729 | 9.019 | 1 | 1.09 |
| rule_program_var_hides_global | 1 | 0.107 | 0.158 | 3.113 | 10.081 | 2601 | 1.07 |
| rule_no_top_level_var_global | 1 | 0.000 | 0.000 | 0.000 | 0.000 | 0 | - |
| rule_operator_operand_type_check | 1 | 3.111 | 4.297 | 42.413 | 139.041 | 1523405 | 1.08 |
| rule_task_names_unique | 1 | 0.080 | 0.100 | 2.880 | 9.171 | 1 | 1.05 |
| rule_stdlib_type_redefinition | 1 | 0.099 | 0.189 | 3.182 | 10.064 | 15840 | 1.05 |
| rule_string_encoding_compat | 1 | 0.084 | 0.104 | 3.166 | 9.962 | 0 | 1.04 |
| rule_string_length_range | 1 | 0.077 | 0.098 | 2.820 | 9.133 | 0 | 1.07 |
| rule_string_literal_char_range | 1 | 0.080 | 0.102 | 2.767 | 9.023 | 0 | 1.08 |
| rule_temporal_literal_range | 1 | 0.083 | 0.102 | 2.972 | 9.548 | 0 | 1.06 |
| rule_struct_initializer_expression_allowed | 1 | 0.078 | 0.096 | 2.795 | 8.911 | 0 | 1.06 |
| rule_fb_instance_array_allowed | 1 | 0.080 | 0.110 | 2.823 | 9.322 | 728 | 1.09 |
| rule_use_declared_enumerated_value | 1 | 0.077 | 0.096 | 2.728 | 8.949 | 0 | 1.08 |
| rule_use_declared_symbolic_var | 1 | 0.269 | 0.434 | 6.997 | 22.191 | 18842 | 1.05 |
| rule_unsupported_extension | 1 | 0.081 | 0.102 | 2.946 | 9.611 | 0 | 1.08 |
| rule_var_decl_const_initialized | 1 | 0.078 | 0.097 | 2.733 | 8.967 | 0 | 1.08 |
| rule_var_decl_const_not_fb | 1 | 0.078 | 0.099 | 2.699 | 8.966 | 0 | 1.09 |
| rule_var_decl_initializer_type_compat | 1 | 0.080 | 0.100 | 2.725 | 9.017 | 0 | 1.09 |
| rule_var_decl_global_const_requires_external_const | 1 | 0.165 | 0.215 | 5.475 | 18.466 | 1729 | 1.11 |
| rule_mixed_located_var_declarations | 1 | 0.083 | 0.113 | 2.809 | 9.438 | 447 | 1.10 |
| rule_pou_hierarchy | 1 | 0.074 | 0.093 | 2.518 | 8.601 | 270 | 1.12 |
| rule_bit_and_partial_access_range | 1 | 0.151 | 0.383 | 4.076 | 13.219 | 36779 | 1.07 |
| rule_case_bit_string_label | 1 | 0.080 | 0.106 | 2.727 | 9.172 | 0 | 1.10 |
| rule_case_selector_type | 1 | 0.079 | 0.100 | 2.700 | 9.327 | 0 | 1.13 |
| rule_condition_type | 1 | 0.085 | 0.106 | 2.818 | 9.644 | 3 | 1.12 |
| rule_constant_range | 1 | 6.441 | 4.771 | 54.681 | 240.680 | 2538877 | 1.35 |
| rule_ref_to | 1 | 0.275 | 0.578 | 7.432 | 22.348 | 42479 | 1.00 |
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
| merge sources | 384 | 4.681 | 5.135 | 6.723 | 12.205 | 129451 | 0.54 |
| type environment | 2 | 0.174 | 0.171 | 0.178 | 0.178 | 1792 | 0.00 |
| function environment | 1 | 0.799 | 0.793 | 0.783 | 0.797 | 10020 | 0.02 |
| xform_resolve_constant_expressions | 1 | 1.755 | 1.931 | 2.580 | 4.500 | 9488 | 0.51 |
| xform_toposort_declarations | 1 | 0.601 | 0.856 | 1.887 | 4.630 | 19123 | 0.82 |
| xform_resolve_type_decl_environment | 2 | 3.542 | 4.197 | 6.776 | 13.655 | 109398 | 0.64 |
| xform_resolve_late_bound_expr_kind | 1 | 2.002 | 2.328 | 3.377 | 6.519 | 20230 | 0.60 |
| xform_resolve_late_bound_type_initializer | 1 | 2.129 | 2.379 | 3.496 | 6.051 | 11883 | 0.50 |
| xform_insert_implicit_deref | 1 | 1.975 | 2.199 | 2.979 | 5.502 | 9488 | 0.56 |
| xform_resolve_adr | 1 | 0.000 | 0.000 | 0.000 | 0.000 | 0 | - |
| xform_fold_initializer_expressions | 1 | 1.753 | 1.949 | 2.730 | 4.960 | 9820 | 0.54 |
| xform_int_to_bool_initializer | 1 | 0.000 | 0.000 | 0.000 | 0.000 | 0 | - |
| xform_resolve_symbol_and_function_environment | 1 | 0.459 | 0.902 | 2.704 | 7.484 | 74979 | 0.93 |
| xform_named_to_positional_args | 1 | 2.268 | 2.560 | 3.520 | 6.087 | 21977 | 0.50 |
| xform_resolve_decl_types | 1 | 1.751 | 1.913 | 2.873 | 5.446 | 19643 | 0.58 |
| xform_resolve_expr_types | 1 | 6.841 | 7.705 | 11.116 | 20.329 | 180237 | 0.55 |
| xform_fold_constant_expressions | 1 | 2.034 | 2.252 | 3.060 | 5.485 | 9488 | 0.53 |
| xform_remove_unsigned_abs | 1 | 2.108 | 2.229 | 3.126 | 5.442 | 9488 | 0.50 |
| xform_resolve_type_aliases | 1 | 0.102 | 0.284 | 1.713 | 11.655 | 1695 | 1.75 |
| xform_mark_unwritten_constants | 1 | 6.174 | 6.761 | 9.568 | 17.235 | 137355 | 0.54 |
| type_table | 1 | 0.175 | 0.310 | 0.967 | 2.689 | 20310 | 0.93 |
| library copies of the passes (15 passes, clone and release) | 31 | 88.770 | 99.055 | 138.778 | 250.405 | 2040653 | 0.54 |

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
first call of the process: 2.2 ms cold, 1.7 ms warm, 12,350 allocations,
1.6 MiB allocated.

| step | calls | cold ms | warm ms | warm allocs | warm KiB |
|---|---|---|---|---|---|
| merge sources | 1 | 0.020 | 0.006 | 15 | 1.9 |
| type environment | 2 | 0.239 | 0.192 | 1792 | 185.8 |
| function environment | 1 | 1.209 | 1.086 | 10020 | 1433.9 |
| fallback copies, all passes | - | - | 0.032 | 240 | 30.4 |

Where the fixed cost is paid:

| Cost | Where | Warm ms | Share of the call | Allocations |
|---|---|---|---|---|
| Function environment, built from the standard functions | `compiler/analyzer/src/stages.rs:134-138`; `FunctionEnvironmentBuilder::with_stdlib_functions`, `function_environment.rs:271` | 1.09 | 66 % | 10,020 |
| Type environment, built twice | first at `stages.rs:132`, second at `stages.rs:287`; both through `build_type_environment`, `stages.rs:98-104` | 0.19 (0.096 each) | 12 % | 1,792 (896 each) |
| Merge of the inputs, one clone of each input | `stages.rs:127-128` | 0.006 for one input; 115 ms for 500 files (grows) | 0.3 % here, 3 % at `x3` | 15 per input |
| Copies of the library kept by the passes | `pass_runner.rs:63`, `:66`, `:91`, `:95`; `stages.rs:289` | 0.03 here; 2,330 ms at `x3` (grows) | 2 % here, 54 % at `x3` | 240 here |
| Passes and rules | `stages.rs` passes, `semantic_rules.rs:92` | 0.13 | 8 % | few |
| Outside every step: extension of the merged library, `SemanticContext::new`, the samples of the observer | `stages.rs:128`, `:401` | 0.17 | 10 % | |

The first two rows do not depend on the project: `function environment` is
0.99 to 1.06 ms and `type environment` 0.19 to 0.21 ms at every scale. The
process pays 0.5 ms more on its first call (2.2 ms against 1.7 ms).

Memory (private bytes of the benchmark process, peak of the sampled
values): `x1/16` and `x1/4` together 0.04 GB; up to `x1` 0.22 GB; all four
scales of both shapes 0.65 GB (working set 0.60 GB). The analysis result at
`x3` is 457 MiB live at the end of the call, and the call allocates 3.4 GiB
in all.

## 5. What scales and what does not

| Step | Growth with the project | Evidence |
|---|---|---|
| Parse of a project | linear | 245 ms at `x1`, 734 ms at `x3`: factor 3.0 |
| Analysis whole | linear | factor 3.1 |
| Every pass except `xform_resolve_type_aliases` | linear at the project shape (exponent 0.99 to 1.07), at most linear at the one-program shape (0.5 to 0.93; the 5,000-line body does not scale) | tables of 4.2 and 4.4 |
| `xform_resolve_type_aliases` | worse than linear (1.75 to 1.79) | quadratic in the number of types, section 4.4 |
| 28 walk-only rules | linear (exponent 1.0 to 1.13) | 2.5 to 3.2 ms at `x1`, 8.6 to 10.2 ms at `x3` (one at 5.3 and 17.4) |
| `rule_constant_range` | worse than linear at the project shape (1.35); flat at the one-program shape (15.3 ms at `x1`, 14.1 ms at `x3`: the cost is in the one big body) | tables of 4.3 and 4.4 |
| Function environment, type environment | constant | 1.0 ms and 0.2 ms at every scale |
| Library copies, merge | linear in the size of the library, paid once per pass | section 4.2 |
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

Pass time at `x3` (936 ms, the 18 passes) by class: local steps 255 ms (27 percent),
collecting steps 463 ms, whole-program steps and the two
that read no unit 218 ms, of which `xform_mark_unwritten_constants` is 154 ms
(`xform_resolve_expr_types`, 170 ms, is counted as collecting). The copies
(2,330 ms) belong to no class: they are a property of the contract of a
pass.

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

These are decisions the figures support; none is implemented here.

1. **The copy of the library per pass is the largest single cost** (55
   percent) and is independent of what a change touches. A pass whose failure
   returns the library it was given (no copy) removes it from every analysis,
   full or selective. A selective analysis that still copies the whole
   library per pass has paid the larger cost already.
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
6. **Code generation of one unit is not the cost to remove first.** At the
   one-program shape the analysis is 51 percent of the pipeline at `x3` and
   71 percent at `x1/16`, code generation 32 and 13 percent. A selective
   analysis that re-analyzed one unit but copied the library 16 times would
   remain at the cost of the copies. Code generation grows faster than the
   project (exponent 1.26) and is the stage to measure again once the analysis
   is selective.

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
