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
    ]);
}
