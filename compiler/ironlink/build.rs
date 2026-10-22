use std::process;

use ironplc_msg_gen::{write_code_constants, ConstantUse};

fn main() {
    // The L-code constants come from the shared generator of
    // `ironplc-msg-gen`. The generator reads every code, so a code that no
    // path raises yet is not dead code.
    println!("cargo:rerun-if-changed=resources/problem-codes.csv");
    if let Err(err) = write_code_constants("problem_codes.rs", ConstantUse::ReadEvery) {
        eprintln!("problem generating problem_codes.rs: {err}");
        process::exit(1);
    }
    ironplc_spec_requirements_gen::generate(&["ironlink.md"]);
}
