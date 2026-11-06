//! Acceptance tests for the retained image (ADR-0085, stage 2b): the host
//! reads the values of its retained variables as an image and applies a saved
//! image in the warm-start step, before its first scan round.
//!
//! Fixtures are compiled with the real project pipeline and round-tripped
//! through the container wire format, so `header.content_hash` is the value a
//! deployed artifact carries.

// Test-target boundary: the workspace denies panicking constructs in
// production code; tests assert by panicking, so they are exempt here.
#![expect(
    clippy::unwrap_used,
    reason = "integration test target: the helpers of this file unwrap, like its tests"
)]

mod common;

use common::{compile_with_options, variable_index};
use ironplc_container::{Container, RetainSpace, SLOT_BYTES};
use ironplc_parser::options::CompilerOptions;
use ironplc_runtime::{
    RetainedApplyError, RetainedDecodeError, RetainedImage, RetainedMismatch, RuntimeHost,
};

fn compile(source: &str) -> Container {
    let options = CompilerOptions {
        allow_persistent_var: true,
        allow_top_level_var_global: true,
        ..CompilerOptions::default()
    };
    compile_with_options(source, &options)
}

fn host(container: Container) -> RuntimeHost {
    let mut host = RuntimeHost::new(container).unwrap();
    host.permit_execution();
    host
}

/// A retained global, a persistent string, a retained field of an instance
/// and three variables that are not retained.
const MIXED: &str = "
VAR_GLOBAL RETAIN
  kept_global : DINT;
END_VAR

FUNCTION_BLOCK acc
  VAR_INPUT step : DINT; END_VAR
  VAR RETAIN kept_field : DINT; END_VAR
  VAR plain_field : DINT; END_VAR
  kept_field := kept_field + step;
  plain_field := plain_field + step;
END_FUNCTION_BLOCK

PROGRAM main
  VAR PERSISTENT text : STRING[10]; END_VAR
  VAR inst : acc; plain : DINT; END_VAR
  kept_global := kept_global + 1;
  plain := plain + 1;
  text := 'abc';
  inst(step := 2);
END_PROGRAM
";

/// A plain instance of a block with one `RETAIN` field.
const PLAIN_INSTANCE: &str = "
FUNCTION_BLOCK mixed
  VAR_INPUT step : DINT; END_VAR
  VAR other : DINT; END_VAR
  VAR RETAIN kept : DINT; END_VAR
  other := other + step;
  kept := kept + step;
END_FUNCTION_BLOCK

PROGRAM main
  VAR inst : mixed; END_VAR
  inst(step := 1);
END_PROGRAM
";

fn global_program(qualifier: &str, body: &str) -> String {
    format!(
        "VAR_GLOBAL {qualifier}
  kept : DINT;
END_VAR

PROGRAM main
  VAR other : DINT; END_VAR
  {body}
END_PROGRAM
"
    )
}

/// The i32 a slot of the data region holds at `offset`.
fn data_i32(host: &RuntimeHost, offset: u32) -> i32 {
    let at = offset as usize;
    i32::from_le_bytes(host.data_region()[at..at + 4].try_into().unwrap())
}

#[test]
fn apply_retained_when_fresh_host_of_same_container_then_retained_values_restored_and_others_initial(
) {
    let container = compile(MIXED);
    let kept_global = variable_index(&container, "kept_global");
    let plain = variable_index(&container, "plain");
    let mut source = host(container.clone());
    source.run(3, || 0).unwrap();
    let image = source.retained_image();
    assert_eq!(source.read_variable(kept_global).unwrap(), 3);
    assert_eq!(source.read_variable(plain).unwrap(), 3);
    assert!(image.data.windows(3).any(|window| window == b"abc"));
    let fresh_data = host(container.clone()).data_region().to_vec();
    let mut restored = host(container.clone());

    restored.apply_retained(&image).unwrap();

    assert_eq!(restored.retained_image(), image);
    assert_eq!(restored.read_variable(kept_global).unwrap(), 3);
    assert_eq!(restored.read_variable(plain).unwrap(), 0);
    // Inside the retained runs the data region is the source's, outside it is
    // what a fresh host has.
    let mut in_runs = vec![false; fresh_data.len()];
    for run in container.retained_runs() {
        if run.space == RetainSpace::Bytes {
            in_runs[run.range()].fill(true);
        }
    }
    for (offset, retained) in in_runs.iter().enumerate() {
        let expected = if *retained {
            source.data_region()[offset]
        } else {
            fresh_data[offset]
        };
        assert_eq!(restored.data_region()[offset], expected, "byte {offset}");
    }
    // The first round reads the restored values.
    restored.run(1, || 0).unwrap();
    assert_eq!(restored.read_variable(kept_global).unwrap(), 4);
    assert_eq!(restored.read_variable(plain).unwrap(), 1);
}

#[test]
fn apply_retained_when_plain_instance_has_one_retain_field_then_only_that_field_restored() {
    let container = compile(PLAIN_INSTANCE);
    let mut source = host(container.clone());
    source.run(4, || 0).unwrap();
    let image = source.retained_image();
    let runs = container.retained_runs();
    assert_eq!(runs.len(), 1);
    let kept = runs[0].start;
    let other = kept - SLOT_BYTES;
    assert_eq!(data_i32(&source, kept), 4);
    assert_eq!(data_i32(&source, other), 4);
    let mut restored = host(container);

    restored.apply_retained(&image).unwrap();

    assert!(image.vars.is_empty());
    assert_eq!(image.data.len(), SLOT_BYTES as usize);
    assert_eq!(data_i32(&restored, kept), 4);
    assert_eq!(data_i32(&restored, other), 0);
    restored.run(1, || 0).unwrap();
    assert_eq!(data_i32(&restored, kept), 5);
    assert_eq!(data_i32(&restored, other), 1);
}

#[test]
fn apply_retained_when_container_differs_only_in_code_then_content_hash_mismatch_and_nothing_written(
) {
    let source_container = compile(&global_program("RETAIN", "kept := kept + 1;"));
    let other_container = compile(&global_program("RETAIN", "kept := kept + 2;"));
    let kept = variable_index(&other_container, "kept");
    let mut source = host(source_container);
    source.run(3, || 0).unwrap();
    let image = source.retained_image();
    let mut target = host(other_container);
    let before = target.retained_image();

    let result = target.apply_retained(&image);

    assert_eq!(
        result,
        Err(RetainedApplyError::Mismatch(RetainedMismatch::ContentHash))
    );
    assert_eq!(target.retained_image(), before);
    assert_eq!(target.read_variable(kept).unwrap(), 0);
}

#[test]
fn apply_retained_when_qualifier_changed_then_refused_and_nothing_written() {
    let source_container = compile(&global_program("RETAIN", "kept := kept + 1;"));
    let other_container = compile(&global_program("", "kept := kept + 1;"));
    assert!(other_container.retained_runs().is_empty());
    let mut source = host(source_container);
    source.run(3, || 0).unwrap();
    let image = source.retained_image();
    let mut target = host(other_container);
    let before = target.retained_image();

    let result = target.apply_retained(&image);

    assert!(matches!(result, Err(RetainedApplyError::Mismatch(_))));
    assert_eq!(target.retained_image(), before);
}

#[test]
fn apply_retained_when_only_the_retained_runs_differ_then_runs_mismatch_and_nothing_written() {
    let container = compile(&global_program("RETAIN", "kept := kept + 1;"));
    let kept = variable_index(&container, "kept");
    let mut source = host(container.clone());
    source.run(3, || 0).unwrap();
    let image = source.retained_image();
    // The content hash is the one the header carries; the rows are changed
    // after the load, so only the rows tell the two containers apart.
    let mut other = container;
    other.type_section.as_mut().unwrap().retained_runs.clear();
    let mut target = host(other);

    let result = target.apply_retained(&image);

    assert_eq!(
        result,
        Err(RetainedApplyError::Mismatch(RetainedMismatch::Runs))
    );
    assert_eq!(target.read_variable(kept).unwrap(), 0);
}

#[test]
fn apply_retained_when_a_round_has_run_then_after_first_round_and_nothing_written() {
    let container = compile(&global_program("RETAIN", "kept := kept + 1;"));
    let kept = variable_index(&container, "kept");
    let mut source = host(container.clone());
    source.run(3, || 0).unwrap();
    let image = source.retained_image();
    let mut target = host(container);
    target.run(1, || 0).unwrap();

    let result = target.apply_retained(&image);

    assert_eq!(result, Err(RetainedApplyError::AfterFirstRound));
    assert_eq!(target.read_variable(kept).unwrap(), 1);
}

#[test]
fn apply_retained_when_image_was_encoded_and_decoded_then_restores_the_values() {
    let container = compile(MIXED);
    let kept_global = variable_index(&container, "kept_global");
    let mut source = host(container.clone());
    source.run(3, || 0).unwrap();
    let decoded = RetainedImage::decode(&source.retained_image().encode()).unwrap();
    let mut restored = host(container);

    restored.apply_retained(&decoded).unwrap();

    assert_eq!(restored.read_variable(kept_global).unwrap(), 3);
}

#[test]
fn decode_when_image_of_a_host_is_truncated_or_garbled_then_error() {
    let container = compile(MIXED);
    let mut source = host(container);
    source.run(2, || 0).unwrap();
    let encoded = source.retained_image().encode();

    for len in 0..encoded.len() {
        assert!(RetainedImage::decode(&encoded[..len]).is_err(), "{len}");
    }
    let mut garbled = encoded.clone();
    garbled[0] ^= 0xFF;
    assert_eq!(
        RetainedImage::decode(&garbled),
        Err(RetainedDecodeError::BadMagic)
    );
    let mut long = encoded;
    long.push(0);
    assert_eq!(
        RetainedImage::decode(&long),
        Err(RetainedDecodeError::TrailingBytes)
    );
}

#[test]
fn retained_image_when_logic_only_change_runs_then_retained_values_are_kept() {
    let base = compile(&global_program("RETAIN", "kept := kept + 1;"));
    let kept = variable_index(&base, "kept");
    let mut running = host(base);
    running.run(3, || 0).unwrap();
    assert_eq!(running.read_variable(kept).unwrap(), 3);
    let before = running.retained_image();
    let candidate = compile(&global_program("RETAIN", "kept := kept + 10;"));

    running.stage(candidate).unwrap();
    running.test().unwrap();
    running.run(1, || 0).unwrap();

    // The retained value is in the persistent part: the new code continues
    // from it, and the image is read with the tag of the code now running.
    assert_eq!(running.read_variable(kept).unwrap(), 13);
    let after = running.retained_image();
    assert_eq!(before.vars, vec![3]);
    assert_eq!(after.vars, vec![13]);
    assert_ne!(after.tag.content_hash, before.tag.content_hash);
    assert_eq!(after.tag.runs_hash, before.tag.runs_hash);
}
