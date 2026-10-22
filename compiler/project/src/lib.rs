pub mod build_record;
pub mod compile;
pub mod device_id;
pub mod disassemble;
pub mod download_record;
pub mod id_agreement;
pub mod path_record;
pub mod project;
pub mod records;
pub mod sidecar;
pub mod tokenizer;

pub use compile::{compile, CompileOutput};
pub use id_agreement::{id_difference, IdDifference};
pub use project::{FileBackedProject, MemoryBackedProject, Project};
pub use records::RecordKind;
pub use sidecar::{Sidecar, SidecarKey, SplitVarUids, SyncReport};

// Spec conformance testing infrastructure (test-only).
#[cfg(test)]
mod spec_requirements {
    include!(concat!(env!("OUT_DIR"), "/spec_requirements.rs"));
}

#[cfg(test)]
#[ctor::ctor(unsafe)]
#[expect(
    clippy::let_underscore_must_use,
    reason = "try_init fails only when a logger is already installed, which is what the tests want to keep"
)]
fn init_test_logger() {
    let _ = env_logger::builder()
        .is_test(true)
        .filter_level(log::LevelFilter::Trace)
        .try_init();
}
