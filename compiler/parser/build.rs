fn main() {
    ironplc_spec_requirements_gen::generate(&[
        // CODESYS ST surface syntax (`REQ-CS-*`): the `|` operator, the
        // incomplete array type, `PARAMS(n) OF T`, identifier spellings and
        // the special operators.
        "codesys-st-surface-syntax.md",
        "reference-to-twincat.md",
        "adr-and-pointer-to.md",
        // Partial-access syntax (`REQ-PAB-parser-*`): tokens, grammar, AST and
        // gating.
        "partial-access-bit-syntax.md",
        // Behavior policies (`REQ-BP-parser-*`): the option surface and the
        // dialect presets.
        "behavior-policies.md",
        // Character string literals (`REQ-SL-parser-*`): `$` escapes.
        "string-literals.md",
        // ST declaration extensions (`REQ-STX-parser-*`): UNION,
        // VAR_STAT/VAR_INST/VAR_GENERIC, NAMESPACE, __BEGIN_IMPLEMENTATION
        // and the OVERLOAD qualifier.
        "st-declaration-extensions.md",
        // Numeric and boolean literals (`REQ-NL-parser-*`): base-10 based
        // integers, typed boolean digits, and the BIT type.
        "numeric-literals.md",
        // Time literals (`REQ-TL-parser-*`): duration units and prefixes,
        // the abbreviated LT/LD prefixes, optional seconds.
        "time-literals.md",
        // CODESYS exception handling (`REQ-TC-parser-*`): tokens, grammar and
        // gating.
        "codesys-try-catch.md",
        // CODESYS jump statements, comments and conditional pragmas
        // (`REQ-JMP-parser-*`).
        "codesys-jump-and-pragmas.md",
        // Front end on the lossless tree (`REQ-PT-parser-*`): the token view,
        // the conversion of the options and the selection of the front end.
        "parse-tree-architecture.md",
    ]);
}
