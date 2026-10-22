//! Generates V-code constants for the HA command surface from
//! `resources/problem-codes.csv` with the shared generator of
//! `ironplc-problems-gen`. The codes live in this crate — not the runtime's
//! hot-edit table — per the HA engineering UI contract (a proposed V41xx
//! block, allocated as the HA command surface raises them).

use std::process;

use ironplc_problems_gen::{write_code_constants, ConstantUse};

fn main() {
    println!("cargo:rerun-if-changed=resources/problem-codes.csv");
    if let Err(err) = write_code_constants("problem_codes.rs", ConstantUse::Exported) {
        eprintln!("problem generating problem_codes.rs: {err}");
        process::exit(1);
    }
}
