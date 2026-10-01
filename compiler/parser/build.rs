fn main() {
    ironplc_spec_requirements_gen::generate(&[
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
    ]);
}
