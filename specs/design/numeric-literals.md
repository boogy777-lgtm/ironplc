# Design: Numeric and Boolean Literals

## Overview

This document fixes the numeric and boolean literal forms beyond the
standard ones: the base-10 based integer (`10#`), the typed boolean digits
(`BOOL#0`, `BOOL#1`), and the one-bit `BIT` type with its literals
(`BIT#0`, `BIT#1`). The forms come from the CODESYS/TwinCAT surface
(`Codesys/LEXER-GAP-ANALYSIS.md` sections 6.1 and 4).

A based integer is written `base#digits`; IEC 61131-3 Edition 2 defines the
bases 2, 8 and 16, and `10#` names the same value in decimal. `BOOL#` and
`BIT#` take the digits `0` and `1`, and `BIT` is an elementary type name.

The `BIT` type is a vendor extension: `bit` is a legal identifier in
IEC 61131-3, so the compiler keeps it an ordinary identifier unless
`--allow-bit-type` is set (`specs/steering/syntax-support-guide.md`,
Non-Standard Syntax Gating). The other two forms cannot be confused with an
identifier — a literal's base and its digits begin with digits — so they are
recognized in every dialect, the same permissive direction the `STRING#` and
`WSTRING#` prefixes take.

## Based Integers

**REQ-NL-parser-010** `10#` followed by decimal digits is a based integer
literal whose value is the digits after the `#`: `10#123` is 123, and
`10#1_000` is 1000. The base, the `#` and the digits are adjacent and form
one lexical unit; whitespace between them is a syntax error. The literal
carries no type name unless a type prefix precedes it, exactly like a
decimal integer literal.

The lexer reads the form as the digits `10`, `#` and the digits of the
value, so the grammar recognizes those three adjacent tokens and adds
nothing to the value.

## Typed Boolean Literals

**REQ-NL-parser-020** `BOOL#1` is `TRUE` and `BOOL#0` is `FALSE`, case
insensitively in the prefix. The digits lex as digits, not as identifiers.
`BOOL#TRUE` and `BOOL#FALSE` remain valid.

## The BIT Type

**REQ-NL-parser-030** With `CompilerOptions::allow_bit_type` set, `BIT#1`
is `TRUE` and `BIT#0` is `FALSE`, case insensitively in the prefix.

**REQ-NL-parser-031** With `allow_bit_type` set, `BIT` is an elementary
type name: `b : BIT;` declares a one-bit variable. With the flag off, `BIT`
is demoted to `Identifier` before parsing (the demotion of
`xform_demote_keywords`), so `bit : BOOL;` is a declaration of the variable
`bit` and neither `BIT#1` nor `BIT` in type position parses.

**REQ-NL-analyzer-020** A `BIT` declaration is an alias of `BOOL`: the
analyzer maps the type to `BOOL`'s representation, so a `BIT` variable
takes the two boolean literals, compares, and is stored exactly as a `BOOL`
is. IronPLC has no one-bit storage.

## Test Mapping

| Requirement | Test |
|---|---|
| REQ-NL-parser-010 | `parse_program_when_decimal_based_literal_then_value` |
| REQ-NL-parser-020 | `parse_program_when_typed_boolean_digit_then_value` |
| REQ-NL-parser-030 | `parse_program_when_bit_literal_then_boolean` |
| REQ-NL-parser-031 | `parse_program_when_bit_type_flag_on_then_elementary_type_name_bit`, `parse_program_when_bit_type_flag_off_then_bit_is_an_identifier` |
| REQ-NL-analyzer-020 | `analyzer_spec_req_nl_020_bit_is_an_alias_of_bool` |
