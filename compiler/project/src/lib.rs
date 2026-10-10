pub mod compile;
pub mod disassemble;
pub mod id_agreement;
pub mod project;
pub mod sidecar;
pub mod tokenizer;

pub use compile::{compile, CompileOutput};
pub use id_agreement::{id_difference, IdDifference};
pub use project::{FileBackedProject, MemoryBackedProject, Project};
pub use sidecar::{sidecar_path_for, Sidecar, SidecarKey, SplitVarUids, SyncReport};

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
