//! The table of the rows for the reference page, generated from the registry.
//!
//! The text is a CSV file with a header line, read by the page
//! (`docs/reference/language/attributes.rst`) as a `csv-table`. The page holds
//! no copy of a name, a place or a value: the checked-in file is the text
//! [`table`] generates, and a test fails when the two differ.

use super::{Family, Info, ALL};

/// The columns of the table, in order.
const HEADER: [&str; 7] = [
    "Family",
    "Name",
    "Aliases",
    "Stands before",
    "Value",
    "In CODESYS",
    "State",
];

/// The text as a cell of a CSV file.
fn cell(text: &str) -> String {
    if text.contains([',', '"', '\n']) {
        format!("\"{}\"", text.replace('"', "\"\""))
    } else {
        text.to_string()
    }
}

/// The names as literal text of the page: `:code:` for each.
fn literals(names: &[&str]) -> String {
    names
        .iter()
        .map(|name| format!(":code:`{name}`"))
        .collect::<Vec<String>>()
        .join(", ")
}

/// A summary as text of the page: a span between backticks is literal text.
fn summary(text: &str) -> String {
    text.split('`')
        .enumerate()
        .map(|(index, part)| {
            if index % 2 == 1 {
                format!(":code:`{part}`")
            } else {
                part.to_string()
            }
        })
        .collect()
}

/// The line of one row.
fn line(row: &Info) -> String {
    [
        row.family.title().to_string(),
        literals(&[row.name]),
        literals(row.aliases),
        row.targets.describe(),
        row.value.describe(),
        summary(row.summary),
        row.state.title().to_string(),
    ]
    .iter()
    .map(|text| cell(text))
    .collect::<Vec<String>>()
    .join(",")
}

/// The CSV text of the table: the header, then the rows grouped by family in
/// the order of [`Family::ALL`], and in the order of the declaration list
/// inside a family.
pub fn table() -> String {
    let rows = Family::ALL.iter().flat_map(|family| {
        ALL.iter()
            .filter(move |row| row.family == *family)
            .map(line)
    });
    std::iter::once(HEADER.join(","))
        .chain(rows)
        .map(|text| text + "\n")
        .collect()
}

#[cfg(test)]
mod tests;
