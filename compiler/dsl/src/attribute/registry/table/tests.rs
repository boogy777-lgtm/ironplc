//! Tests of the table of the reference page.

use std::path::PathBuf;

use spec_test_macro::spec_test;

use super::*;

/// The file the reference page reads.
fn checked_in() -> PathBuf {
    PathBuf::from(env!("CARGO_MANIFEST_DIR"))
        .join("../../docs/reference/language/attributes-table.csv")
}

#[spec_test(REQ_ATTR_dsl_040)]
fn table_when_registry_then_equals_the_checked_in_file() {
    let written = std::fs::read_to_string(checked_in())
        .unwrap()
        .replace("\r\n", "\n");

    assert!(
        written == table(),
        "docs/reference/language/attributes-table.csv is not what the registry generates; \
         write it with `cargo test -p ironplc-dsl write_table -- --ignored`"
    );
}

/// Rewrites the file the reference page reads from the registry.
#[test]
#[ignore = "writes docs/reference/language/attributes-table.csv"]
fn write_table() {
    std::fs::write(checked_in(), table()).unwrap();
}

#[spec_test(REQ_ATTR_dsl_041)]
fn table_when_registry_then_one_line_for_each_row_after_the_header() {
    let text = table();

    assert_eq!(text.lines().count(), ALL.len() + 1);
    assert!(text.starts_with("Family,Name,Aliases,Stands before,Value,In CODESYS,State\n"));
}

#[spec_test(REQ_ATTR_dsl_041)]
fn table_when_registry_then_rows_are_grouped_in_the_order_of_the_families() {
    let order: Vec<usize> = table()
        .lines()
        .skip(1)
        .map(|line| {
            let title = line.split(',').next().unwrap_or_default();
            Family::ALL
                .iter()
                .position(|family| family.title() == title)
                .unwrap()
        })
        .collect();

    assert!(order.windows(2).all(|pair| pair[0] <= pair[1]), "{order:?}");
}

#[spec_test(REQ_ATTR_dsl_041)]
fn table_when_row_has_aliases_targets_and_a_value_then_its_line_says_them() {
    let row = ALL.iter().find(|row| row.name == "noinit").unwrap();

    let text = line(row);

    assert_eq!(
        text,
        "Initialisation,:code:`noinit`,\":code:`no_init`, :code:`no-init`\",\"structure member, variable\",no value,The variable keeps its value when the application is reset,Waiting"
    );
}

#[spec_test(REQ_ATTR_dsl_041)]
fn summary_when_backticks_then_the_span_is_literal_text_and_every_summary_pairs_them() {
    assert_eq!(
        summary("`TO_STRING` of a value"),
        ":code:`TO_STRING` of a value"
    );
    let unpaired: Vec<&str> = ALL
        .iter()
        .filter(|row| row.summary.matches('`').count() % 2 != 0)
        .map(|row| row.name)
        .collect();
    assert!(unpaired.is_empty(), "{unpaired:?}");
}

#[spec_test(REQ_ATTR_dsl_041)]
fn cell_when_text_has_a_comma_or_quote_then_quoted() {
    assert_eq!(cell("plain"), "plain");
    assert_eq!(cell("a, b"), "\"a, b\"");
    assert_eq!(cell("say \"x\""), "\"say \"\"x\"\"\"");
}
