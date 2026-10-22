//! The constants of a crate whose problem codes are a CSV turned into `&str`
//! constants at build time (`vm-cli`, `runtime`, `ironplc-redundancy`).
//!
//! `resources/problem-codes.csv` has the columns `Code,Name,Message` (more
//! columns are ignored). Each row becomes
//!
//! ```text
//! /// {Message}
//! pub const {NAME_IN_SCREAMING_SNAKE_CASE}: &str = "{Code}";
//! ```
//!
//! A crate with such a table calls [`write_code_constants`] from its
//! `build.rs` and includes the file from `OUT_DIR`. [`generate_code_constants`]
//! is the same without file access, so the tests here check the text.

use std::{env, error::Error, fs, path::PathBuf};

/// Whether the constants of a file are read by the crate that includes it.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum ConstantUse {
    /// The constants are the public surface of a library, so none is dead.
    Exported,
    /// Several binaries include the file and each uses a subset of the codes.
    /// An anonymous const reads every constant, so every code is used in each
    /// binary and none needs a `dead_code` suppression.
    ReadEvery,
}

/// Converts PascalCase to SCREAMING_SNAKE_CASE.
fn pascal_to_screaming_snake(s: &str) -> String {
    let mut result = String::new();
    for (i, ch) in s.chars().enumerate() {
        if ch.is_uppercase() && i > 0 {
            result.push('_');
        }
        result.push(ch.to_ascii_uppercase());
    }
    result
}

/// Renders the constants of the rows of `csv`, in the order of the rows.
pub fn generate_code_constants(csv: &str, constant_use: ConstantUse) -> Result<String, String> {
    let mut out = String::new();
    let mut const_names = Vec::new();
    let mut rdr = csv::Reader::from_reader(csv.as_bytes());
    for result in rdr.records() {
        let record = result.map_err(|e| e.to_string())?;
        let column = |index: usize| {
            record
                .get(index)
                .ok_or_else(|| format!("Record {record:?} is not valid at column {index}"))
        };
        let (code, name, message) = (column(0)?, column(1)?, column(2)?);

        let const_name = pascal_to_screaming_snake(name);
        out.push_str(&format!(
            "/// {message}\npub const {const_name}: &str = \"{code}\";\n\n"
        ));
        const_names.push(const_name);
    }

    if constant_use == ConstantUse::ReadEvery {
        out.push_str(&format!(
            "const _: &[&str] = &[{}];\n",
            const_names.join(", ")
        ));
    }
    Ok(out)
}

/// Reads `resources/problem-codes.csv` of the crate being built and writes the
/// constants to `file_name` in `OUT_DIR`. The build script keeps its own
/// `cargo:rerun-if-changed` line.
pub fn write_code_constants(
    file_name: &str,
    constant_use: ConstantUse,
) -> Result<(), Box<dyn Error>> {
    let src_path = PathBuf::from(env::var("CARGO_MANIFEST_DIR")?)
        .join("resources")
        .join("problem-codes.csv");
    let src = fs::read_to_string(src_path)
        .map_err(|e| format!("Unable to read 'problem-codes.csv': {e}"))?;

    let out = generate_code_constants(&src, constant_use)?;

    let out_path = PathBuf::from(env::var("OUT_DIR")?).join(file_name);
    fs::write(out_path, out).map_err(|e| format!("Unable to create '{file_name}': {e}"))?;
    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;

    const CSV: &str = "Code,Name,Message\nV6001,FileOpen,Unable to open the file\nV6002,ContainerRead,Unable to read\n";

    #[test]
    fn generate_code_constants_when_exported_then_one_documented_constant_for_each_row() {
        let out = generate_code_constants(CSV, ConstantUse::Exported).unwrap();

        assert_eq!(
            out,
            "/// Unable to open the file\npub const FILE_OPEN: &str = \"V6001\";\n\n\
             /// Unable to read\npub const CONTAINER_READ: &str = \"V6002\";\n\n"
        );
    }

    #[test]
    fn generate_code_constants_when_read_every_then_anonymous_const_names_every_constant() {
        let out = generate_code_constants(CSV, ConstantUse::ReadEvery).unwrap();

        assert!(out.starts_with(&generate_code_constants(CSV, ConstantUse::Exported).unwrap()));
        assert!(out.ends_with("const _: &[&str] = &[FILE_OPEN, CONTAINER_READ];\n"));
    }

    #[test]
    fn generate_code_constants_when_no_rows_then_exported_is_empty_and_read_every_is_empty_slice() {
        let csv = "Code,Name,Message\n";

        assert_eq!(
            generate_code_constants(csv, ConstantUse::Exported).unwrap(),
            ""
        );
        assert_eq!(
            generate_code_constants(csv, ConstantUse::ReadEvery).unwrap(),
            "const _: &[&str] = &[];\n"
        );
    }

    #[test]
    fn generate_code_constants_when_more_columns_then_extra_columns_ignored() {
        let csv = "Code,Name,Message,Severity,Codesys\nV1,Ab,M,Error,C1\n";

        assert_eq!(
            generate_code_constants(csv, ConstantUse::Exported).unwrap(),
            "/// M\npub const AB: &str = \"V1\";\n\n"
        );
    }

    #[test]
    fn generate_code_constants_when_name_has_several_words_then_screaming_snake() {
        let csv = "Code,Name,Message\nV1,CrossloadCandidateRejected,M\n";

        assert!(generate_code_constants(csv, ConstantUse::Exported)
            .unwrap()
            .contains("pub const CROSSLOAD_CANDIDATE_REJECTED: &str"));
    }

    #[test]
    fn generate_code_constants_when_row_has_too_few_columns_then_error_names_column() {
        let csv = "Code,Name\nV1,Ab\n";

        let message = generate_code_constants(csv, ConstantUse::Exported).unwrap_err();

        assert!(message.contains("column 2"), "{message}");
    }
}
