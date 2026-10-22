use std::process;

use ironplc_msg_gen::{write_code_constants, ConstantUse};

fn main() {
    match std::env::var("OPT_LEVEL") {
        Ok(level) => println!("cargo:rustc-env=BUILD_OPT_LEVEL={level}"),
        Err(err) => {
            eprintln!("OPT_LEVEL not set: {err}");
            process::exit(1);
        }
    }

    // The V-code constants come from the shared generator of
    // `ironplc-msg-gen`. The two binaries that include the file use
    // different subsets of the codes, so the generator reads them all.
    println!("cargo:rerun-if-changed=resources/problem-codes.csv");
    if let Err(err) = write_code_constants("io_codes.rs", ConstantUse::ReadEvery) {
        eprintln!("problem generating io_codes.rs: {err}");
        process::exit(1);
    }
    ironplc_spec_requirements_gen::generate(&["vm-cli.md"]);
}
