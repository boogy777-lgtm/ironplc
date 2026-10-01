# Design: Character String Literals

## Overview

A character string literal is written between single quotes (`STRING`,
single-byte) or double quotes (`WSTRING`, double-byte), and uses `$` escapes
to spell characters that cannot appear as themselves (IEC 61131-3 edition 2,
B.1.2.2). This document fixes what a literal denotes: the characters the
compiler stores and measures, which are the decoded characters, not the
source text.

The character encoding of the stored string is ADR-0016 (Latin-1 `STRING`,
UTF-16LE `WSTRING`); a decoded character the type cannot hold is P4052.

There is one escape table, `dsl::string_escape`. The parser decodes with it
and every renderer that writes a literal back as source encodes with it.

## Escapes

| Requirement | Escape | Width | Denotes |
|---|---|---|---|
| **REQ-SL-parser-001** | `$$` | both | `$` |
| **REQ-SL-parser-002** | `$L`, `$l` | both | line feed, U+000A |
| **REQ-SL-parser-003** | `$N`, `$n` | both | line feed, U+000A |
| **REQ-SL-parser-004** | `$P`, `$p` | both | form feed, U+000C |
| **REQ-SL-parser-005** | `$R`, `$r` | both | carriage return, U+000D |
| **REQ-SL-parser-006** | `$T`, `$t` | both | tab, U+0009 |
| **REQ-SL-parser-007** | `$'` | both | `'` |
| **REQ-SL-parser-008** | `$"` | both | `"` |
| **REQ-SL-parser-009** | `$` and two hex digits | `STRING` | the character with that code, U+0000 to U+00FF (REQ-SL-parser-012) |
| **REQ-SL-parser-010** | `$` and four hex digits | `WSTRING` | the character with that code, except a surrogate (REQ-SL-parser-012) |
| **REQ-SL-parser-011** | `$U` and exactly eight hex digits | both | the Unicode character with that code, except a surrogate |
| **REQ-SL-parser-012** | a numeric escape whose value is 0x80 to 0xFF | both | the character Windows-1252 gives that byte |

The standard leaves the newline character to the implementation; `$N` is a
line feed, like `$L`. The standard defines `$'` only for `STRING` and `$"`
only for `WSTRING`; the other delimiter escaped stands for itself, because
several toolchains accept it and nothing else could be meant.

`$U` (REQ-SL-parser-011) and the Windows-1252 reading of the 0x80–0xFF band
(REQ-SL-parser-012) are the spellings CODESYS gives those characters, and
this compiler accepts them in every dialect: an escape's meaning belongs to
the one escape table, and recognising the code point cannot change what any
program that used a defined escape denotes. Values 0xA0 to 0xFF are the same
in Windows-1252 and Latin-1; the 0x80 to 0x9F band is where the two differ,
and there `$80` is `€` (U+20AC), not U+0080. The `U` is upper case, as
CODESYS spells it; the hex digits are not case sensitive. A ninth hex digit
is not part of the escape: `$U000000421` is `B` followed by `1`.

**REQ-SL-parser-020** Any other `$` sequence -- `$` followed by a character
that is not listed above, too few hex digits for the width, a surrogate
code, or `$` at the end of the literal -- is P0012 at the escape. The literal
keeps the sequence as written, so that parsing continues.

## Rendering

**REQ-SL-plc2plc-001** A rendered literal re-parses to the same characters:
`$` as `$$`, the literal's own delimiter as `$'` or `$"`, line feed, carriage
return, form feed and tab as `$L`, `$R`, `$P` and `$T`, any other control
character as a hex escape of the literal's width — or, when the literal's
width has no numeric escape that reads back as that character (the
Windows-1252 band of REQ-SL-parser-012), as `$U` and eight hex digits — and
every other character as itself.

## Values

A string variable assigned a literal holds the decoded characters:
`LEN('$41$$')` is 2, and the variable holds `A$`. Code generation encodes the
characters it is given (ADR-0016), so this follows from REQ-SL-parser-001 to
010; the end-to-end tests in `codegen/tests/it/end_to_end_string_escapes.rs`
check it.
