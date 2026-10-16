fn main() {
    // The project crate owns the rule for when code generation runs
    // (`REQ-DS-project-*`).
    ironplc_spec_requirements_gen::generate(&["diagnostic-severity.md"]);
}
