# AGENTS.md

## Skill

```
BEFORE compiler/** work: load skill `ironplc-dev` via skill tool
subagents := skill NOT inherited -> include "load skill ironplc-dev" in task prompt (or paste relevant section)
```

## Steering (READ FIRST)

```
BEFORE changes: read specs/steering/<topic>.md matching touched path
  compiler/**    -> compiler-standards.md && compiler-architecture.md
  **/analyzer/** -> iec-61131-3-compliance.md;  parser|codegen|plc2plc -> syntax-support-guide.md
  xml            -> plcopen-xml-module.md;      docs|vscode -> doc|extension-standards.md
terminology := specs/steering/glossary.md; !coin_new_terms
```

## Gates (MUST pass before PR)

```
pre_pr  := cd compiler && just   # compile + coverage(>=85% lines) + clippy + fmt + dupes(10% exact / 5% near)
fix     := cd compiler && just format; single crate: cargo test -p <crate>
no_just := cargo build && cargo test --quiet --workspace && cargo clippy --all-targets && cargo fmt --all -- --check
specs   := cd specs && just   # adr-numbers + adr-front-matter + plan-citations
           # windows: recipe broken (just+cygpath path mangling) -> run the bash
           # recipes via Git Bash: sh.exe <recipe-body> with cd /f/IronPLC
```

## LLM fences (build-enforced, DO NOT weaken)

```
warnings      := deny via compiler/.cargo/config.toml [build] warnings="deny" (cargo >= 1.97)
clippy_deny   := [workspace.lints.clippy] unwrap_used|expect_used|panic|todo|unimplemented = "deny"
tests         := keep unwrap/expect via compiler/clippy.toml (allow-*-in-tests)
exempt        := build-time/test tooling ONLY: ironplc-test, problems, spec_requirements_gen,
                 dsl_macro_derive (own [lints] tables) + test/bench targets + test_support modules
exemption_way := per-target/crate #![allow(..., reason = "...")] — never silently, always with reason
                 && never for production compiler code (parser|analyzer|codegen|vm|dsl|sources src/)
invariant     := Result + ? | Diagnostic::internal_error() | Trap — panic is the last resort with reason
```

## Workflow

```
main     := !push_direct; always feature_branch + PR
trivial  := typo|format|dep_bump|one_line_fix|docs_only -> skip plan
else     := plan as FIRST commit (Goal|Architecture|Prefactoring|File map|Tasks) && open plan PR for approval
        && prefactor own PR && implement && decisions -> ADR(status+date under H1)|specs/design/
        && undelivered -> issue && delete plan before merge && !cite plans from code/docs/workflows
versions := auto_managed; !edit_manually
readme   := root README.md == integrations/vscode/README.md (capabilities, limitations, warning banner)
docs     := !duplicate; share via docs/includes/ + .. include:: (checked: cd docs && just duplicates)
```
