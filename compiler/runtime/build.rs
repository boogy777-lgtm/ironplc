//! Generates V-code constants for the runtime crate from
//! `resources/problem-codes.csv`, mirroring the vm-cli `io_codes` build.
//! The codes cover the online change command layer and the host-level
//! refusals (e.g. the execution permit latch); the VM's own trap codes
//! stay in the vm crate's CSV.

use std::{
    env,
    error::Error,
    fs::{self, File},
    io::Write,
    path::PathBuf,
    process,
};

fn generate_problem_codes() -> Result<(), Box<dyn Error>> {
    println!("cargo:rerun-if-changed=resources/problem-codes.csv");

    let mut src_path = PathBuf::from(env!("CARGO_MANIFEST_DIR"));
    src_path.push("resources");
    src_path.push("problem-codes.csv");

    let src = fs::read_to_string(src_path)
        .map_err(|e| format!("Unable to read 'problem-codes.csv': {e}"))?;
    let src = src.as_bytes();

    let out_path = PathBuf::from(env::var("OUT_DIR")?).join("problem_codes.rs");
    let mut out = File::create(out_path)?;

    let mut rdr = csv::Reader::from_reader(src);
    for result in rdr.records() {
        let record = result?;
        let code = record
            .get(0)
            .ok_or_else(|| format!("Record {record:?} is not valid at column 0"))?;
        let name = record
            .get(1)
            .ok_or_else(|| format!("Record {record:?} is not valid at column 1"))?;
        let message = record
            .get(2)
            .ok_or_else(|| format!("Record {record:?} is not valid at column 2"))?;

        // Convert PascalCase name to SCREAMING_SNAKE_CASE for the constant
        let const_name = pascal_to_screaming_snake(name);

        out.write_all(
            format!("/// {message}\npub const {const_name}: &str = \"{code}\";\n\n").as_bytes(),
        )?;
    }

    out.flush()?;
    Ok(())
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

fn main() {
    if let Err(err) = generate_problem_codes() {
        eprintln!("problem generating problem_codes.rs: {err}");
        process::exit(1);
    }
}
