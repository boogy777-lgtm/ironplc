fn main() {
    ironplc_spec_requirements_gen::generate(&[
        // Lossless tree and CST-to-dsl lowering (`REQ-PT-syntax-*`).
        "parse-tree-architecture.md",
    ]);
}
