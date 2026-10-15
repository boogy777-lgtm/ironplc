//! Browser-based playground for IronPLC.
//!
//! Exposes functions to JavaScript:
//! - [`compile`] - Parse IEC 61131-3 source and produce bytecode
//! - [`run`] - Execute pre-compiled bytecode (.iplc)
//! - [`run_source`] - Compile and execute in one step
//! - [`load_program`] - Compile source and create a stepping session
//! - [`step`] - Execute N scans within a stepping session
//! - [`reset_session`] - Clear the stepping session

use std::cell::RefCell;
use std::io::Cursor;

use base64::engine::general_purpose::STANDARD as BASE64;
use base64::Engine;
use ironplc_container::debug_format::VariableRenderer;
use ironplc_container::{Container, InstanceId, TaskId};
use ironplc_dsl::common::Library;
use ironplc_dsl::core::FileId;
use ironplc_dsl::diagnostic::{Diagnostic, LineColumn, DOCS_SECTIONS};
use ironplc_parser::options::{CompilerOptions, Dialect, FeatureDescriptor};
use ironplc_project::MemoryBackedProject;
use ironplc_sources::{parse_source, FileType};
use ironplc_vm::error::Trap;
use ironplc_vm::{Slot, VariableView, Vm, VmBuffers};
use serde::{Deserialize, Serialize};
use wasm_bindgen::prelude::*;

/// Persistent state for step-through execution.
///
/// Stores compiled bytecode and variable buffer so variables persist
/// across calls to [`step`]. The VM is re-created each step because
/// `VmRunning` borrows all buffers and cannot be stored across WASM calls.
struct VmSession {
    container_bytes: Vec<u8>,
    var_buf: Vec<Slot>,
    data_region: Vec<u8>,
    scan_count: u64,
    cycle_time_us: u64,
    faulted: bool,
}

thread_local! {
    static SESSION: RefCell<Option<VmSession>> = const { RefCell::new(None) };
}

/// Resolve a playground dialect string to a [`Dialect`].
///
/// Accepts the canonical [`Dialect::cli_name`] values (`"iec61131-3-ed2"`,
/// `"iec61131-3-ed3"`, `"rusty"`, `"codesys"`).
///
/// The empty string (and any unrecognized value) resolves to the RuSTy
/// dialect, which enables the broadest set of extensions. This keeps the many existing
/// documentation embeds that omit a dialect working, since they rely on the
/// lenient default to explore non-standard features without toggling flags.
fn dialect_from(dialect: &str) -> Dialect {
    if dialect.is_empty() {
        return Dialect::Rusty;
    }
    dialect.parse().unwrap_or(Dialect::Rusty)
}

/// Build [`CompilerOptions`] from a dialect string and an optional list of
/// `--allow-*` feature flags layered on top.
///
/// The dialect string is resolved by [`dialect_from`]; see that function for
/// the accepted values and the lenient default.
///
/// `allows` is a comma-separated list of feature short names — the part
/// after `--allow-` in the CLI flag, e.g. `"sizeof,c-style-comments"`.
/// Unknown names are ignored.
fn compiler_options_from(dialect: &str, allows: &str) -> CompilerOptions {
    let mut options = CompilerOptions::from_dialect(dialect_from(dialect));
    for name in allows.split(',').map(str::trim).filter(|s| !s.is_empty()) {
        let cli_flag = format!("--allow-{name}");
        if let Some(fd) = CompilerOptions::FEATURE_DESCRIPTORS
            .iter()
            .find(|fd: &&FeatureDescriptor| fd.cli_flag == cli_flag)
        {
            options.set_flag_by_key(fd.option_key, true);
        }
    }
    options
}

/// Install a panic hook that logs to `console.error` with a full stack trace.
///
/// Called once from JavaScript before using any other exports.
#[wasm_bindgen]
pub fn init_panic_hook() {
    console_error_panic_hook::set_once();
}

/// Return the crate version so the playground can include it in problem-code URLs.
#[wasm_bindgen]
pub fn version() -> String {
    env!("CARGO_PKG_VERSION").to_string()
}

/// A selectable dialect for the playground dialect picker.
#[derive(Serialize, Deserialize)]
struct DialectOption {
    /// Canonical dialect name (matches [`Dialect::cli_name`]), passed back to
    /// the compile/run entry points.
    value: String,
    /// Human-readable label for display in the picker.
    label: String,
    /// Whether this is the default selection.
    is_default: bool,
}

/// Return the selectable dialects as a JSON array so the UI builds its dialect
/// picker from the compiler's own [`Dialect`] list. This keeps the picker from
/// drifting out of sync with the dialects the compiler actually supports.
#[wasm_bindgen]
pub fn dialects() -> String {
    let default = Dialect::default();
    let options: Vec<DialectOption> = Dialect::ALL
        .iter()
        .map(|d| DialectOption {
            value: d.cli_name().to_string(),
            label: d.display_name().to_string(),
            is_default: *d == default,
        })
        .collect();
    serde_json::to_string(&options).unwrap_or_else(|_| "[]".to_string())
}

/// Return the problem-code prefix → documentation-section map as a JSON object,
/// so the front end links a diagnostic's code to the right part of the reference
/// site without knowing the mapping itself.
///
/// The playground used to test code prefixes with its own regex, which had no
/// `E####` arm and so disagreed with the compiler. Serving
/// [`DOCS_SECTIONS`] across the boundary — the same table `docs_section` reads —
/// leaves one authority for the mapping instead of two that can drift.
#[wasm_bindgen]
pub fn doc_sections() -> String {
    let map: std::collections::BTreeMap<String, &str> = DOCS_SECTIONS
        .iter()
        .map(|(prefix, section)| (prefix.to_string(), *section))
        .collect();
    serde_json::to_string(&map).unwrap_or_else(|_| "{}".to_string())
}

/// Result of a compilation attempt.
#[derive(Serialize, Deserialize)]
struct CompileResult {
    ok: bool,
    #[serde(default, skip_serializing_if = "Option::is_none")]
    bytecode: Option<String>,
    #[serde(default, skip_serializing_if = "Vec::is_empty")]
    diagnostics: Vec<DiagnosticInfo>,
}

/// A single diagnostic (error or warning) from compilation.
///
/// Line and column fields are 1-based for display, computed from the
/// diagnostic's byte offsets using the same helper the LSP server uses.
#[derive(Debug, Serialize, Deserialize)]
struct DiagnosticInfo {
    code: String,
    message: String,
    #[serde(default, skip_serializing_if = "String::is_empty")]
    label: String,
    /// Guidance on how to resolve the problem (e.g. "use `(* *)` comments").
    /// Empty when the diagnostic carries no help notes.
    #[serde(default, skip_serializing_if = "Vec::is_empty")]
    help: Vec<String>,
    start_line: u32,
    start_column: u32,
    end_line: u32,
    end_column: u32,
    /// The compiler source file that produced this diagnostic (e.g. the
    /// `file!()` recorded by `Diagnostic::todo`). Present for the P9999/P9998
    /// families; empty otherwise. This is the compiler's own location, never
    /// the user's program, so it is safe to report automatically.
    #[serde(default, skip_serializing_if = "String::is_empty")]
    compiler_file: String,
    /// The compiler source line paired with `compiler_file`. Zero when absent.
    #[serde(default, skip_serializing_if = "is_zero")]
    compiler_line: u32,
}

fn is_zero(n: &u32) -> bool {
    *n == 0
}

/// Build a [`DiagnosticInfo`] from a compiler diagnostic, computing 1-based
/// line/column from the supplied source text.
fn diagnostic_info(diag: &Diagnostic, source: &str) -> DiagnosticInfo {
    let start = LineColumn::from_offset(source, diag.primary.location.start);
    let end = LineColumn::from_offset(source, diag.primary.location.end);
    DiagnosticInfo {
        code: diag.code.clone(),
        message: diag.description(),
        label: diag.primary.message.clone(),
        help: diag.help().to_vec(),
        start_line: start.line + 1,
        start_column: start.column + 1,
        end_line: end.line + 1,
        end_column: end.column + 1,
        compiler_file: diag.source_file.clone().unwrap_or_default(),
        compiler_line: diag.source_line.unwrap_or(0),
    }
}

/// A structured runtime error surfaced across the WASM boundary.
///
/// Carries a human-readable `message` and, for VM traps, the trap's stable
/// v-code (e.g. `"V4001"`). Kept as a single object — rather than sibling
/// `error`/`error_code` fields — so the front end can treat it uniformly with a
/// compiler diagnostic (which likewise has a message and a code) and render
/// both through one path. This shape is also the natural fit as the playground
/// moves toward JSON-RPC.
#[derive(Debug, Default, Serialize, Deserialize)]
struct RunError {
    /// Human-readable message including task and instance context for traps.
    message: String,
    /// The error's stable code — a VM trap's v-code (e.g. `"V4001"`) or, for a
    /// host/embedding-layer illegal state, `"P9998"` (the internal-error code).
    /// Every error site populates this, so it is only absent on values
    /// deserialized from an older payload.
    #[serde(default, skip_serializing_if = "Option::is_none")]
    code: Option<String>,
    /// For a `P9998` internal error, the WASM host `file`/`line` where the
    /// illegal state was detected — the same `compiler_file`/`compiler_line`
    /// contract a P9xxx [`DiagnosticInfo`] carries, so the front end ranks host
    /// bugs by location just like compiler ones. Empty for VM traps.
    #[serde(default, skip_serializing_if = "String::is_empty")]
    compiler_file: String,
    /// The host source line paired with `compiler_file`. Zero when absent.
    #[serde(default, skip_serializing_if = "is_zero")]
    compiler_line: u32,
}

/// Builds a `P9998` internal-error [`RunError`] stamped with the WASM host
/// `file`/`line` of the call site.
///
/// Host/embedding-layer illegal states — frontend↔WASM contract violations and
/// failures that should never occur in normal use — are bugs, not distinct user
/// conditions. Rather than mint a bespoke code and doc page per site, they all
/// share the existing internal-error code and are told apart by the recorded
/// location, mirroring how the compiler records `file#Lline` for its own P9998
/// diagnostics (see [`Diagnostic::internal_error`]).
#[track_caller]
fn internal_run_error(message: String) -> RunError {
    let loc = std::panic::Location::caller();
    // Derive the stable code from the shared diagnostic constructor rather than
    // hard-coding "P9998", so it tracks the compiler's internal-error code.
    let code = Diagnostic::internal_error().code;
    RunError {
        message,
        code: Some(code),
        compiler_file: loc.file().to_string(),
        compiler_line: loc.line(),
    }
}

/// Builds the [`RunError`] for a VM trap, keyed by the trap's v-code.
///
/// `location` is the faulting task and program instance, or `None` for a trap
/// raised while loading the container, which belongs to no task.
fn vm_trap_error(context: &str, trap: &Trap, location: Option<(TaskId, InstanceId)>) -> RunError {
    let message = match location {
        Some((task_id, instance_id)) => {
            format!("{context}: {trap} (task {task_id}, instance {instance_id})")
        }
        None => format!("{context}: {trap}"),
    };
    RunError {
        message,
        code: Some(trap.v_code().to_string()),
        ..Default::default()
    }
}

/// Serializes a fallback [`RunError`] for the serde-to-JSON error path. The full
/// result already failed to serialize, but this tiny error object does not; the
/// static literal is a last-ditch guard should even that fail.
fn fallback_error_json(err: &RunError) -> String {
    serde_json::to_string(err)
        .unwrap_or_else(|_| r#"{"message":"Serialization error","code":"P9998"}"#.to_string())
}

/// The [`DiagnosticInfo`] counterpart of [`internal_run_error`], for host
/// illegal states on the compile path (which report through `diagnostics`
/// rather than a `RunError`). Same `P9998` + `file#Lline` contract.
#[track_caller]
fn internal_diagnostic(message: String) -> DiagnosticInfo {
    let loc = std::panic::Location::caller();
    let code = Diagnostic::internal_error().code;
    DiagnosticInfo {
        code,
        message,
        label: String::new(),
        help: Vec::new(),
        start_line: 1,
        start_column: 1,
        end_line: 1,
        end_column: 1,
        compiler_file: loc.file().to_string(),
        compiler_line: loc.line(),
    }
}

/// Result of executing bytecode.
#[derive(Serialize, Deserialize)]
struct RunResult {
    ok: bool,
    #[serde(default, skip_serializing_if = "Vec::is_empty")]
    variables: Vec<VariableInfo>,
    scans_completed: u64,
    #[serde(default, skip_serializing_if = "Option::is_none")]
    error: Option<RunError>,
}

/// A variable value read from the VM after execution.
#[derive(Serialize, Deserialize)]
struct VariableInfo {
    index: u16,
    value: String,
    #[serde(default, skip_serializing_if = "String::is_empty")]
    name: String,
    #[serde(default, skip_serializing_if = "String::is_empty")]
    type_name: String,
    /// `false` when `value` is a placeholder shown because the actual value
    /// could not be read (e.g., a STRING or WSTRING data-region offset out of
    /// bounds, or an aggregate whose contents the renderer cannot reach).
    #[serde(default = "default_true", skip_serializing_if = "is_true")]
    valid: bool,
}

fn default_true() -> bool {
    true
}

fn is_true(b: &bool) -> bool {
    *b
}

/// Result of compile-and-run (combines both).
#[derive(Serialize, Deserialize)]
struct RunSourceResult {
    ok: bool,
    #[serde(default, skip_serializing_if = "Vec::is_empty")]
    diagnostics: Vec<DiagnosticInfo>,
    #[serde(default, skip_serializing_if = "Vec::is_empty")]
    variables: Vec<VariableInfo>,
    scans_completed: u64,
    #[serde(default, skip_serializing_if = "Option::is_none")]
    error: Option<RunError>,
}

/// Result of a step-through operation (load or step).
#[derive(Serialize, Deserialize)]
struct StepResult {
    ok: bool,
    #[serde(default, skip_serializing_if = "Vec::is_empty")]
    diagnostics: Vec<DiagnosticInfo>,
    #[serde(default, skip_serializing_if = "Vec::is_empty")]
    variables: Vec<VariableInfo>,
    total_scans: u64,
    #[serde(default, skip_serializing_if = "Option::is_none")]
    error: Option<RunError>,
}

/// Parse IEC 61131-3 source code and produce bytecode.
///
/// Returns a JSON string with shape:
/// ```json
/// { "ok": true, "bytecode": "<base64>" }
/// ```
/// or on error:
/// ```json
/// {
///   "ok": false,
///   "diagnostics": [{
///     "code": "...", "message": "...",
///     "start_line": L, "start_column": C,
///     "end_line": L, "end_column": C
///   }]
/// }
/// ```
/// Line and column are 1-based.
#[wasm_bindgen]
pub fn compile(source: &str, dialect: &str, allows: &str, libraries: &str) -> String {
    let result = compile_inner(source, dialect, allows, libraries);
    serde_json::to_string(&result).unwrap_or_else(|e| {
        // Even the full result failed to serialize; the tiny internal-error
        // diagnostic still serializes, so build the fallback payload from it.
        let diag = serde_json::to_string(&internal_diagnostic(format!("Serialization error: {e}")))
            .unwrap_or_else(|_| r#"{"code":"P9998","message":"Serialization error"}"#.to_string());
        format!(r#"{{"ok":false,"diagnostics":[{diag}]}}"#)
    })
}

/// Parse the activated compatibility libraries from their served plain-text
/// sources (`REQ-CL-playground-001`).
///
/// `libraries` is a JSON array of ST source strings — the plain-text library
/// files the browser fetched from the app's served assets. Each is parsed into
/// a [`Library`] to be injected ahead of user source in analysis, so its
/// symbols (e.g. `Tc2_System`'s `PI`) resolve under their exact vendor names.
/// An empty or blank string activates no library.
fn parse_activated_libraries(
    libraries: &str,
    options: &CompilerOptions,
) -> Result<Vec<Library>, CompileResult> {
    if libraries.trim().is_empty() {
        return Ok(Vec::new());
    }

    let sources: Vec<String> = serde_json::from_str(libraries).map_err(|e| CompileResult {
        ok: false,
        bytecode: None,
        diagnostics: vec![internal_diagnostic(format!(
            "Failed to parse library sources: {e}"
        ))],
    })?;

    let mut parsed = Vec::with_capacity(sources.len());
    for source in &sources {
        let file_type = FileType::from_content(source);
        match parse_source(file_type, source, &FileId::default(), options) {
            Ok(lib) => parsed.push(lib),
            Err(diag) => {
                return Err(CompileResult {
                    ok: false,
                    bytecode: None,
                    diagnostics: vec![diagnostic_info(&diag, source)],
                });
            }
        }
    }
    Ok(parsed)
}

fn compile_inner(source: &str, dialect: &str, allows: &str, libraries: &str) -> CompileResult {
    let file_type = FileType::from_content(source);
    let options = compiler_options_from(dialect, allows);

    // Activated compatibility libraries, loaded from their served plain-text
    // files (`REQ-CL-playground-001`). They are injected ahead of user source
    // (base stdlib -> library -> user) and merge as ordinary declarations.
    let compat_libraries = match parse_activated_libraries(libraries, &options) {
        Ok(libs) => libs,
        Err(result) => return result,
    };

    // The pipeline (parse -> analysis -> codegen) is owned by
    // `ironplc-project`, which compiles for wasm32 -- the playground supplies
    // the editor buffer and the fetched library text, and does nothing with
    // the filesystem-backed half of that crate. The editor buffer has no
    // filename, so its type comes from the content rather than an extension.
    let mut project = MemoryBackedProject::new(options);
    project.set_preparsed_libraries(compat_libraries);
    project.add_source_with_file_type(FileId::default(), source.to_owned(), file_type);

    let output = ironplc_project::compile(
        &mut project,
        &options,
        &ironplc_codegen::EmptyLookup,
        vec![],
    );

    let Some(container) = output.container else {
        return CompileResult {
            ok: false,
            bytecode: None,
            diagnostics: output
                .diagnostics
                .iter()
                .map(|d| diagnostic_info(d, source))
                .collect(),
        };
    };

    let mut buf = Vec::new();
    if let Err(e) = container.write_to(&mut buf) {
        return CompileResult {
            ok: false,
            bytecode: None,
            diagnostics: vec![internal_diagnostic(format!(
                "Failed to serialize bytecode: {e}"
            ))],
        };
    }

    CompileResult {
        ok: true,
        bytecode: Some(BASE64.encode(&buf)),
        diagnostics: vec![],
    }
}

/// Execute pre-compiled bytecode (.iplc format).
///
/// `bytecode_base64` is the base64-encoded .iplc file content.
/// `scans` is the number of scan cycles to run.
///
/// Returns a JSON string with variable values after execution.
#[wasm_bindgen]
pub fn run(bytecode_base64: &str, scans: u32) -> String {
    let result = run_inner(bytecode_base64, scans);
    serde_json::to_string(&result).unwrap_or_else(|e| {
        let error = fallback_error_json(&internal_run_error(format!("Serialization error: {e}")));
        format!(r#"{{"ok":false,"variables":[],"scans_completed":0,"error":{error}}}"#)
    })
}

fn run_inner(bytecode_base64: &str, scans: u32) -> RunResult {
    let bytes = match BASE64.decode(bytecode_base64) {
        Ok(b) => b,
        Err(e) => {
            return RunResult {
                ok: false,
                variables: vec![],
                scans_completed: 0,
                error: Some(internal_run_error(format!("Invalid base64: {e}"))),
            };
        }
    };

    run_bytes(&bytes, scans)
}

fn run_bytes(bytes: &[u8], scans: u32) -> RunResult {
    let container = match Container::read_from(&mut Cursor::new(bytes)) {
        Ok(c) => c,
        Err(e) => {
            return RunResult {
                ok: false,
                variables: vec![],
                scans_completed: 0,
                error: Some(internal_run_error(format!(
                    "Invalid bytecode container: {e}"
                ))),
            };
        }
    };

    let mut bufs = VmBuffers::from_container(&container);

    const INIT_TRAP: &str = "VM trap during init";
    let running = Vm::new()
        .load(&container, &mut bufs)
        .map_err(|trap| vm_trap_error(INIT_TRAP, &trap, None))
        .and_then(|ready| {
            ready.start().map_err(|ctx| {
                vm_trap_error(INIT_TRAP, &ctx.trap, Some((ctx.task_id, ctx.instance_id)))
            })
        });
    let mut running = match running {
        Ok(vm) => vm,
        Err(error) => {
            return RunResult {
                ok: false,
                variables: vec![],
                scans_completed: 0,
                error: Some(error),
            };
        }
    };

    let renderer = VariableRenderer::new(&container);

    for round in 0..scans {
        let uptime_us = (round as u64) * 1000;
        if let Err(ctx) = running.run_round(uptime_us) {
            let faulted = running.fault(ctx);
            let variables = read_all_variables(&faulted, &renderer);
            return RunResult {
                ok: false,
                variables,
                scans_completed: round as u64,
                error: Some(vm_trap_error(
                    "VM trap",
                    faulted.trap(),
                    Some((faulted.task_id(), faulted.instance_id())),
                )),
            };
        }
    }

    let variables = read_all_variables(&running, &renderer);
    let scans_completed = running.scan_count();
    running.stop();

    RunResult {
        ok: true,
        variables,
        scans_completed,
        error: None,
    }
}

/// Compile IEC 61131-3 source and execute in one step.
///
/// Returns a JSON string with both compilation diagnostics and execution results.
#[wasm_bindgen]
pub fn run_source(
    source: &str,
    scans: u32,
    dialect: &str,
    allows: &str,
    libraries: &str,
) -> String {
    let result = run_source_inner(source, scans, dialect, allows, libraries);
    serde_json::to_string(&result).unwrap_or_else(|e| {
        let error = fallback_error_json(&internal_run_error(format!("Serialization error: {e}")));
        format!(
            r#"{{"ok":false,"diagnostics":[],"variables":[],"scans_completed":0,"error":{error}}}"#
        )
    })
}

#[expect(
    clippy::unwrap_used,
    reason = "bytecode is Some when diagnostics are empty (checked above); base64 is compiler-produced"
)]
fn run_source_inner(
    source: &str,
    scans: u32,
    dialect: &str,
    allows: &str,
    libraries: &str,
) -> RunSourceResult {
    let compile_result = compile_inner(source, dialect, allows, libraries);
    if !compile_result.ok {
        return RunSourceResult {
            ok: false,
            diagnostics: compile_result.diagnostics,
            variables: vec![],
            scans_completed: 0,
            error: None,
        };
    }

    let bytecode_b64 = compile_result.bytecode.unwrap();
    let bytes = BASE64.decode(&bytecode_b64).unwrap();
    let run_result = run_bytes(&bytes, scans);

    RunSourceResult {
        ok: run_result.ok,
        diagnostics: vec![],
        variables: run_result.variables,
        scans_completed: run_result.scans_completed,
        error: run_result.error,
    }
}

/// Snapshots every variable slot with its debug name, type and formatted value.
///
/// Takes the VM as a [`VariableView`] so a running and a faulted VM share one
/// body: the caller's lifecycle state does not change how a value is read.
///
/// Rendering goes through [`VariableRenderer`], the one place that formats a
/// variable for display (`specs/design/variable-value-rendering.md`), so the
/// playground agrees with `--dump-vars`, the debugger and the VS Code run
/// panel — the playground previously carried its own near-copy of that logic,
/// and the two disagreed on STRING, on the date types and on TIME's unit.
fn read_all_variables(vm: &dyn VariableView, renderer: &VariableRenderer) -> Vec<VariableInfo> {
    let data_region = vm.data_region();
    (0..vm.num_variables())
        .filter_map(|i| {
            vm.read_variable_raw(ironplc_container::VarIndex::new(i))
                .ok()
                .map(|raw| {
                    let rendered = renderer.render(i, raw, data_region);
                    VariableInfo {
                        index: i,
                        value: rendered.text,
                        name: renderer
                            .var(i)
                            .map(|info| info.name.clone())
                            .unwrap_or_default(),
                        type_name: renderer
                            .var(i)
                            .map(|info| info.type_name.clone())
                            .unwrap_or_default(),
                        valid: rendered.valid,
                    }
                })
        })
        .collect()
}

/// Compile IEC 61131-3 source and create a stepping session.
///
/// The session stores compiled bytecode and a variable buffer that persists
/// across calls to [`step`]. Returns a JSON `StepResult` with `total_scans: 0`.
#[wasm_bindgen]
pub fn load_program(
    source: &str,
    cycle_time_us: u32,
    dialect: &str,
    allows: &str,
    libraries: &str,
) -> String {
    let result = load_program_inner(source, cycle_time_us, dialect, allows, libraries);
    serde_json::to_string(&result).unwrap_or_else(|e| {
        let error = fallback_error_json(&internal_run_error(format!("Serialization error: {e}")));
        format!(r#"{{"ok":false,"diagnostics":[],"variables":[],"total_scans":0,"error":{error}}}"#)
    })
}

#[expect(
    clippy::unwrap_used,
    reason = "bytecode is Some when diagnostics are empty (checked above); base64 is compiler-produced"
)]
fn load_program_inner(
    source: &str,
    cycle_time_us: u32,
    dialect: &str,
    allows: &str,
    libraries: &str,
) -> StepResult {
    let compile_result = compile_inner(source, dialect, allows, libraries);
    if !compile_result.ok {
        return StepResult {
            ok: false,
            diagnostics: compile_result.diagnostics,
            variables: vec![],
            total_scans: 0,
            error: None,
        };
    }

    let bytecode_b64 = compile_result.bytecode.unwrap();
    let container_bytes = BASE64.decode(&bytecode_b64).unwrap();

    let container = match Container::read_from(&mut Cursor::new(&container_bytes)) {
        Ok(c) => c,
        Err(e) => {
            return StepResult {
                ok: false,
                diagnostics: vec![],
                variables: vec![],
                total_scans: 0,
                error: Some(internal_run_error(format!("Failed to load bytecode: {e}"))),
            };
        }
    };

    // Run the init function once to apply initial values to the variable buffer.
    // Subsequent calls to step() will use resume() to skip re-initialization.
    let mut bufs = VmBuffers::from_container(&container);

    let trap = match Vm::new().load(&container, &mut bufs) {
        Ok(ready) => ready
            .start()
            .map(|running| running.stop())
            .err()
            .map(|ctx| ctx.trap),
        Err(trap) => Some(trap),
    };
    if let Some(trap) = trap {
        return StepResult {
            ok: false,
            diagnostics: vec![],
            variables: vec![],
            total_scans: 0,
            error: Some(vm_trap_error("VM init trap", &trap, None)),
        };
    }

    SESSION.with(|cell| {
        *cell.borrow_mut() = Some(VmSession {
            container_bytes,
            var_buf: bufs.vars,
            data_region: bufs.data_region,
            scan_count: 0,
            cycle_time_us: cycle_time_us as u64,
            faulted: false,
        });
    });

    StepResult {
        ok: true,
        diagnostics: vec![],
        variables: vec![],
        total_scans: 0,
        error: None,
    }
}

/// Execute N scan cycles within the current stepping session.
///
/// Variable values persist between calls. Returns a JSON `StepResult`
/// with accumulated `total_scans`.
#[wasm_bindgen]
pub fn step(scans: u32) -> String {
    let result = step_inner(scans);
    serde_json::to_string(&result).unwrap_or_else(|e| {
        let error = fallback_error_json(&internal_run_error(format!("Serialization error: {e}")));
        format!(r#"{{"ok":false,"diagnostics":[],"variables":[],"total_scans":0,"error":{error}}}"#)
    })
}

fn step_inner(scans: u32) -> StepResult {
    SESSION.with(|cell| {
        let mut borrow = cell.borrow_mut();
        let session = match borrow.as_mut() {
            Some(s) => s,
            None => {
                return StepResult {
                    ok: false,
                    diagnostics: vec![],
                    variables: vec![],
                    total_scans: 0,
                    error: Some(internal_run_error(
                        "No program loaded. Call load_program first.".to_string(),
                    )),
                };
            }
        };

        if session.faulted {
            return StepResult {
                ok: false,
                diagnostics: vec![],
                variables: vec![],
                total_scans: 0,
                error: Some(internal_run_error(
                    "Session is faulted. Call reset_session to start over.".to_string(),
                )),
            };
        }

        let container = match Container::read_from(&mut Cursor::new(&session.container_bytes)) {
            Ok(c) => c,
            Err(e) => {
                return StepResult {
                    ok: false,
                    diagnostics: vec![],
                    variables: vec![],
                    total_scans: 0,
                    error: Some(internal_run_error(format!("Failed to load bytecode: {e}"))),
                };
            }
        };

        let (variables, total_scans, error) = run_vm_step(
            &container,
            &mut session.var_buf,
            &mut session.data_region,
            session.scan_count,
            scans,
            session.cycle_time_us,
        );

        session.scan_count = total_scans;
        if error.is_some() {
            session.faulted = true;
        }

        StepResult {
            ok: error.is_none(),
            diagnostics: vec![],
            variables,
            total_scans,
            error,
        }
    })
}

/// Run an ephemeral VM for N scans using the given container and variable buffer.
///
/// Uses [`VmReady::resume`] to skip re-initialization so that variable values
/// (including initial values) persist across calls. The VM's internal scan
/// counter is the source of truth for total cycles executed.
///
/// Returns `(variables, total_scan_count, error)`, where `error` carries the
/// message and the trap's v-code when execution stopped on a trap.
fn run_vm_step(
    container: &Container,
    var_buf: &mut Vec<Slot>,
    data_region: &mut Vec<u8>,
    base_scan_count: u64,
    scans: u32,
    cycle_time_us: u64,
) -> (Vec<VariableInfo>, u64, Option<RunError>) {
    let mut bufs = VmBuffers::from_container(container);
    // Swap the session's persistent buffers into VmBuffers so the VM
    // operates on them directly, avoiding a copy.
    std::mem::swap(&mut bufs.vars, var_buf);
    std::mem::swap(&mut bufs.data_region, data_region);

    let result = run_vm_scans(container, &mut bufs, base_scan_count, scans, cycle_time_us);

    // Swap the (now-updated) persistent buffers back to the session.
    std::mem::swap(&mut bufs.vars, var_buf);
    std::mem::swap(&mut bufs.data_region, data_region);

    result
}

/// Runs scan cycles on an already-prepared [`VmBuffers`], returning variable
/// snapshots and the total scan count.
fn run_vm_scans(
    container: &Container,
    bufs: &mut VmBuffers,
    base_scan_count: u64,
    scans: u32,
    cycle_time_us: u64,
) -> (Vec<VariableInfo>, u64, Option<RunError>) {
    let mut running = match Vm::new().load(container, bufs) {
        Ok(ready) => ready.resume(base_scan_count),
        Err(trap) => {
            let error = vm_trap_error("VM trap", &trap, None);
            return (vec![], base_scan_count, Some(error));
        }
    };

    for _ in 0..scans {
        let uptime_us = running.scan_count() * cycle_time_us;
        if let Err(ctx) = running.run_round(uptime_us) {
            let total_scans = running.scan_count();
            let faulted = running.fault(ctx);
            let variables = read_all_variables(&faulted, &VariableRenderer::new(container));
            let error = vm_trap_error(
                "VM trap",
                faulted.trap(),
                Some((faulted.task_id(), faulted.instance_id())),
            );
            return (variables, total_scans, Some(error));
        }
    }

    let variables = read_all_variables(&running, &VariableRenderer::new(container));
    let total_scans = running.scan_count();
    running.stop();
    (variables, total_scans, None)
}

/// Clear the stepping session.
///
/// Returns `{"ok":true}`.
#[wasm_bindgen]
pub fn reset_session() -> String {
    SESSION.with(|cell| {
        *cell.borrow_mut() = None;
    });
    r#"{"ok":true}"#.to_string()
}

// Spec conformance testing infrastructure (test-only).
#[cfg(test)]
mod spec_requirements {
    include!(concat!(env!("OUT_DIR"), "/spec_requirements.rs"));
}
#[cfg(test)]
mod spec_conformance;

#[cfg(test)]
mod tests;
