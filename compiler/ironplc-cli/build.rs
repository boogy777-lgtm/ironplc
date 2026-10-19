fn main() {
    // The command line and the language server show a message in the form for a
    // person (`REQ-DS-cli-*`).
    ironplc_spec_requirements_gen::generate(&["diagnostic-severity.md"]);
}
