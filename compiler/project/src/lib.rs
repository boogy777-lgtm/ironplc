// Allow large errors because this is a compiler - we expect large errors.
#![allow(clippy::result_large_err)]

pub mod compile;
pub mod disassemble;
pub mod project;
pub mod sidecar;
pub mod tokenizer;

pub use compile::{compile, CompileOutput};
pub use project::{FileBackedProject, MemoryBackedProject, Project};
pub use sidecar::{sidecar_path_for, Sidecar, SidecarKey, SplitVarUids, SyncReport};

#[cfg(test)]
#[ctor::ctor(unsafe)]
fn init_test_logger() {
    let _ = env_logger::builder()
        .is_test(true)
        .filter_level(log::LevelFilter::Trace)
        .try_init();
}
