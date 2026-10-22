//! Generates V-code constants for the runtime crate from
//! `resources/problem-codes.csv` with the shared generator of
//! `ironplc-problems-gen`. The codes cover the online change command layer
//! and the host-level refusals (e.g. the execution permit latch); the VM's own
//! trap codes stay in the vm crate's CSV.

use std::process;

use ironplc_problems_gen::{write_code_constants, ConstantUse};

fn main() {
    println!("cargo:rerun-if-changed=resources/problem-codes.csv");
    if let Err(err) = write_code_constants("problem_codes.rs", ConstantUse::Exported) {
        eprintln!("problem generating problem_codes.rs: {err}");
        process::exit(1);
    }
}
