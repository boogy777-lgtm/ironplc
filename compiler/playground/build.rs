fn main() {
    // The playground owns the browser-activation requirement
    // (`REQ-CL-playground-001`) for compatibility libraries and the severity of
    // a diagnostic it reports (`REQ-DS-playground-001`).
    ironplc_spec_requirements_gen::generate(&[
        "compatibility-libraries.md",
        "diagnostic-severity.md",
    ]);
}
