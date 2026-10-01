# Design: Time Literals

## Overview

This design specifies the syntax and parsing semantics of IEC 61131-3 duration literals (time literals) in IronPLC. Duration literals denote intervals of time and may be assigned to `TIME` (Edition 2, 32-bit) or `LTIME` (Edition 3, 64-bit) variables.

The design builds on:

- **[ADR-0021: TIME as 32-bit and LTIME as 64-bit with Millisecond Precision](../adrs/0021-time-32bit-ltime-64bit.md)** — storage width, unit, and sub-millisecond truncation semantics
- **[ADR-0022: IEC 61131-3:2013 Compiler Flag for LTIME and Future Features](../adrs/0022-edition-3-compiler-flag.md)** — the Edition 3 gate; see its amendment for the flag that was built

## Design Goals

1. **Standard conformance** — IEC 61131-3 is a case-insensitive language for keywords and literal markers; time literals must accept every case combination.
2. **Edition coverage** — a single grammar spans Edition 2 (`T#` / `TIME#`) and Edition 3 (adds `LTIME#`); Edition 3 constructs are gated by the existing compiler flag.
3. **Testability** — every syntactic claim is a numbered requirement that a parser test can cite by ID.
4. **Narrow scope for precision** — precision and truncation belong to ADR-0021; this document only requires that the parser preserve the input value without loss.

## Scope

**In scope:** The grammar of duration, time-of-day and date literals: prefixes (`T`, `TIME`, `LT`, `LTIME`, `TOD`, `LTOD`, `D`, `LD`, `DATE`, `LDATE`), unit suffixes (`d`, `h`, `m`, `s`, `ms`, `us`, `ns`), compound forms, the underscore visual separator, the optional leading sign, and the optional seconds of a time of day.

**Out of scope:**

- Sub-millisecond precision handling (owned by ADR-0021)
- Timer function blocks (TON, TOF, TP)
- Build-time enforcement of the REQ-TL → test link (see Future Work)

---

## 1. Grammar

A duration literal has the form:

```
duration   = prefix '#' ['-'] interval
prefix     = 'T' | 'TIME' | 'LT' | 'LTIME'              (case-insensitive)
interval   = scalar | compound
scalar     = fixed_point unit
compound   = integer unit ['_'] { integer unit ['_'] } [fixed_point trailing_unit]
unit       = 'd' | 'h' | 'm' | 's' | 'ms' | 'us' | 'ns' (case-insensitive)
```

Units in a compound interval appear in strictly descending magnitude order.

## 2. Prefix

**REQ-TL-parser-001** A duration literal begins with one of the prefixes `T`, `TIME`, or `LTIME`, followed by `#`.

**REQ-TL-parser-002** The prefix is recognized case-insensitively. `T#`, `t#`, `TIME#`, `time#`, `Time#` are equivalent; likewise every case variant of `LTIME#`. The abbreviated spellings of REQ-TL-parser-004 and REQ-TL-parser-005 are case-insensitive too.

**REQ-TL-parser-003** The `LTIME` prefix is only accepted when
`CompilerOptions::allow_long_time_types` is `true` (set directly, or by the
`iec61131-3-ed3`, `codesys` or `twincat` dialect preset). When the flag is
`false`, `xform_demote_keywords` demotes `LTIME` to an ordinary identifier
before parsing, so `LTIME#5s` fails with `P0002 Syntax error` at the `#` rather
than with a message naming the edition — the accepted cost of demotion under
[ADR-0040](../adrs/0040-dialect-violations-diagnosed-in-policy-phase.md) rule 3,
which applies because `LTIME` is a legal variable name in Edition 2. The Edition
3 gate itself is [ADR-0022](../adrs/0022-edition-3-compiler-flag.md).

**REQ-TL-parser-004** `LT` followed by `#` is the abbreviated `LTIME` prefix:
`LT#5s` is an `LTIME` literal of five seconds, the same literal as
`LTIME#5s`. The abbreviation is accepted in every dialect, unlike `LTIME`
(REQ-TL-parser-003): `LT` is not a keyword in any dialect, so there is no
token to demote and the abbreviation cannot be made a per-dialect form. A
`LT` that is not immediately followed by `#` is an ordinary identifier, so
`LT#Red` — an enumeration value of a type named `LT` — is unaffected.

**REQ-TL-parser-005** `LD` followed by `#` is the abbreviated `LDATE` prefix
of a date literal: `LD#2024-01-20` is the same literal as
`LDATE#2024-01-20`, and is accepted in every dialect for the reason
REQ-TL-parser-004 gives.

## 3. Unit Suffixes

**REQ-TL-parser-010** The complete set of supported unit suffixes is `d` (days), `h` (hours), `m` (minutes), `s` (seconds), `ms` (milliseconds), `us` (microseconds) and `ns` (nanoseconds). Any other name, such as `T#5min`, is a parse error. The sub-millisecond units are the vendor extension CODESYS accepts; the parser preserves the value in full (REQ-TL-parser-030) and truncation to the type's unit is ADR-0021's.

**REQ-TL-parser-011** Unit suffixes are recognized case-insensitively. Every case variant of a unit is accepted and produces the same `DurationLiteral`. For example, `T#5S`, `T#5s`, `T#500Ms`, `T#500MS`, `T#500mS`, and `T#500ms` are all valid; `T#1H30M30S` equals `T#1h30m30s`.

**REQ-TL-parser-012** When scanning an interval, a longer unit name is matched before any shorter unit that begins it: `ms` before `m`, so `T#100ms` is not misread as minutes followed by a stray unit. `us` and `ns` begin with no other unit name, so they conflict with nothing.

## 4. Interval Forms

**REQ-TL-parser-020** An interval may be a single scalar: an integer or fixed-point number followed by a unit (e.g., `T#1.5s`, `T#500ms`, `T#2D`).

**REQ-TL-parser-021** An interval may be a compound sequence of parts whose units appear in strictly descending magnitude order (`d` > `h` > `m` > `s` > `ms` > `us` > `ns`). All leading parts are integers; the trailing part may be fixed-point. Example: `T#1d2h30m15s500ms`.

**REQ-TL-parser-022** A compound interval may include `_` between adjacent parts as a visual separator. The separator is optional and has no semantic effect. Example: `T#1d_2h_30m` equals `T#1d2h30m`.

**REQ-TL-parser-023** An optional `-` sign between `#` and the first part negates the interval. Example: `T#-5s` represents negative five seconds.

## 5. Time-of-Day and Date-and-Time Seconds

**REQ-TL-parser-024** The seconds of a time-of-day are optional: `TOD#10:00` is ten o'clock with zero seconds, the same value as `TOD#10:00:00`, and an omitted seconds field takes the value zero. This holds wherever a daytime occurs — the time-of-day prefixes (`TOD#`, `TIME_OF_DAY#`, `LTOD#`, `LTIME_OF_DAY#`) and the time part of a date-and-time literal (`DT#2024-01-01-10:00`).

**REQ-TL-parser-025** The minutes of a time-of-day are required: `TOD#10` is a parse error, and so is `TOD#10:`.

## 6. Precision and Storage

**REQ-TL-parser-030** The parser preserves the full literal value in a `DurationLiteral` without precision loss, down to the nanosecond: `T#1us` is one thousand nanoseconds and `T#500ns` is five hundred. Type-specific truncation — TIME to `i32` milliseconds and LTIME to `i64` milliseconds — happens in codegen as specified by [ADR-0021](../adrs/0021-time-32bit-ltime-64bit.md).

## 7. Edition Coverage

| Edition | Prefixes available | Notes |
|---------|-------------------|-------|
| IEC 61131-3 Edition 2 (1993) | `T#`, `TIME#` | REQ-TL-parser-001 through REQ-TL-parser-002 and REQ-TL-parser-010 through REQ-TL-parser-030 apply |
| IEC 61131-3 Edition 3 (2013) | `T#`, `TIME#`, `LTIME#` | All REQs apply; REQ-TL-parser-003 requires `allow_long_time_types` to enable `LTIME#` |

Unit suffix case-insensitivity (REQ-TL-parser-011) applies in both editions.

## 8. Test Mapping

Parser tests link to requirements via the existing `{area}_spec_req_{id}_{description}` naming convention (see [spec-conformance-testing.md](spec-conformance-testing.md)). Tests live in `compiler/parser/src/tests/`.

| Requirement | Test function |
|---|---|
| REQ-TL-parser-001 | `duration_spec_req_tl_001_prefixes` |
| REQ-TL-parser-002 | `duration_spec_req_tl_002_prefix_case_insensitive` |
| REQ-TL-parser-003 | `duration_spec_req_tl_003_ltime_requires_flag` |
| REQ-TL-parser-004 | `duration_spec_req_tl_004_lt_abbreviation` |
| REQ-TL-parser-005 | `date_spec_req_tl_005_ld_abbreviation` |
| REQ-TL-parser-010 | `duration_spec_req_tl_010_supported_units` |
| REQ-TL-parser-011 | `duration_spec_req_tl_011_unit_suffix_uppercase_accepted` |
| REQ-TL-parser-012 | `duration_spec_req_tl_012_ms_matched_before_m` |
| REQ-TL-parser-020 | `parse_program_when_fixed_point_duration_then_ok` |
| REQ-TL-parser-021 | `duration_spec_req_tl_021_compound_interval`, `parse_when_compound_duration_then_sum_of_parts`, `parse_when_compound_duration_malformed_then_error` |
| REQ-TL-parser-022 | `duration_spec_req_tl_022_compound_with_underscore` |
| REQ-TL-parser-023 | `duration_spec_req_tl_023_negative_duration` |
| REQ-TL-parser-024 | `duration_spec_req_tl_024_seconds_optional` |
| REQ-TL-parser-025 | `duration_spec_req_tl_025_minutes_required` |
| REQ-TL-parser-030 | `duration_spec_req_tl_030_submillisecond_value_preserved` |

REQ-TL-parser-001 is covered by `duration_spec_req_tl_001_prefixes`; REQ-TL-parser-003 by `duration_spec_req_tl_003_ltime_requires_flag`; REQ-TL-parser-020 by `parse_program_when_fixed_point_duration_then_ok` in `compiler/parser/src/tests/literals.rs`.

## 9. Implementation

The duration literal grammar lives in `compiler/parser/src/parser.rs`, section marked `// B.1.2.3.1 Duration`. Duration units and the `T`/`D`/`LT`/`LD` prefixes are identifier tokens, matched by the general helper rule `contextual_keyword`, which compares the identifier text case-insensitively (REQ-TL-parser-011):

```rust
rule contextual_keyword(val: &'static str) -> &'input Token = token:[t] {?
  if token.token_type == TokenType::Identifier && token.text.eq_ignore_ascii_case(val) {
    return Ok(token)
  }
  Err(val)
}
```

The prefix productions `T` / `t` / `D` / `d` / `LT` / `LD` are therefore single `contextual_keyword` calls, ordered after the keyword forms (`TIME`, `LTIME`, …) that they could otherwise shadow.

An interval is one rule, `interval()`: one or more `number unit` parts with an optional `_` between them. `combine_interval_parts` checks the order of the units (strictly descending, which also rules out a repeated unit) and that only the last part is fractional; a violation fails the literal with a syntax error.

The lexer alone cannot deliver those parts: an identifier may contain digits and `_`, so `T#1m30s` lexes as `1` and the identifier `m30s`. The token transform `xform_split_duration_units` runs before parsing and, inside a duration literal only (a `T`, `TIME`, `LT` or `LTIME` prefix, `#`, an optional `-`, then the adjacent tokens), splits such an identifier into its letter, digit and `_` runs, and joins `1`, `.`, `5` back into the fixed-point `1.5` when a fractional part follows a unit (`T#1m1.5s`).

The seconds of a daytime are parsed by `daytime()` as an optional `':' fixed_point` group (REQ-TL-parser-024); an absent group is zero.

## 10. Future Work

- **Build-time REQ-TL enforcement.** The `#[spec_test(REQ_XX_NNN)]` macro infrastructure in `compiler/container/build.rs` currently scans only `bytecode-container-format.md` and `bytecode-instruction-set.md`. The parser's `build.rs` now scans this document for the `REQ-TL-parser-*` requirements, so the link is enforced for them.
- **Warning for sub-millisecond literals in TIME.** ADR-0021 notes that sub-millisecond durations truncate to zero for 32-bit TIME; a future analyzer pass could emit a diagnostic.
