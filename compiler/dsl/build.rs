fn main() {
    ironplc_spec_requirements_gen::generate(&[
        // Attributes of declarations (`REQ-ATTR-dsl-*`): the field of the
        // declaration nodes and the census of the kinds that carry it.
        "attributes.md",
    ]);
}
