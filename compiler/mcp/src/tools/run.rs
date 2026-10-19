//! The `run` MCP tool.
//!
//! Loads a compiled `.iplc` container from the process cache and executes
//! it in the IronPLC VM, returning a time-ordered trace of observed
//! variable values and a summary of task cycles completed.
//!
//! The tool surface is specified in `specs/design/mcp-server.md` §`run`.
//! Not all of it is built: stimuli, non-default trace modes, the `tasks`
//! filter, `container_base64` ingestion, and the full IEC value codec for
//! STRING/DATE/struct/array return `ok: false` with a diagnostic directing
//! the caller to the follow-up. See issue #1480 for that gap.
//!
//! Design references: `specs/design/mcp-server.md` §`run`
//! (REQ-TOL-mcp-040..048), §Variable Naming (REQ-ARC-mcp-020..021), §VM
//! Sandboxing (REQ-ARC-mcp-030..035).

use std::sync::Mutex;
use std::time::Duration;

use ironplc_dsl::core::SourceSpan;
use ironplc_dsl::diagnostic::{Diagnostic, Label};
use ironplc_problems::{text, Text};
use schemars::JsonSchema;
use serde::{Deserialize, Serialize};
use serde_json::{Map, Value};

use ironplc_vm::interval_from_ms;

use crate::cache::{ContainerCache, ResolvedVar, TaskKind, TaskMeta, VariableSymbolMap};
use crate::runner::{self, EffectiveLimits, RunOutcome, TerminatedReason};
use crate::tools::common::{serialize_diagnostic, serialize_diagnostics};

/// Combined input accepted by `run`.
#[derive(Debug, Deserialize, JsonSchema)]
pub struct RunInput {
    /// Opaque handle returned by a prior `compile` call. Exactly one of
    /// `container_id` and `container_base64` must be supplied.
    #[serde(default)]
    pub container_id: Option<String>,
    /// Inline `.iplc` bytes. Phase 11 feature — rejected in MVP.
    #[serde(default)]
    pub container_base64: Option<String>,
    /// Simulated time to run, in milliseconds.
    pub duration_ms: u64,
    /// Cycle time to run tasks that declare no `INTERVAL` at, in milliseconds.
    /// Required when the container has such a task; ignored by containers whose
    /// tasks all declare an `INTERVAL`.
    #[serde(default)]
    pub freewheeling_interval_ms: Option<f64>,
    /// Fully-qualified variable names to include in the trace.
    #[serde(default)]
    pub variables: Vec<String>,
    /// When `true`, the server auto-expands the trace set to include every
    /// observable output in the container.
    #[serde(default)]
    pub trace_outputs: bool,
    /// Time-ordered writes applied to drivable inputs. Phase 11 feature.
    #[serde(default)]
    #[schemars(schema_with = "stimuli_schema")]
    pub stimuli: Vec<Value>,
    /// Controls trace sampling mode and cap.
    #[serde(default)]
    pub trace: Option<TraceOptions>,
    /// Per-call limit tighteners. May not loosen server defaults (REQ-ARC-mcp-031).
    #[serde(default)]
    pub limits: Option<LimitOverrides>,
    /// Restrict trace emission to cycles from named tasks. Phase 11 feature.
    #[serde(default)]
    pub tasks: Option<Vec<String>>,
}

/// schemars schema function for the free-form `stimuli` array.
///
/// The element type is `serde_json::Value`, which schemars renders as the
/// boolean schema `true` for the array's `items`. Some MCP clients reject a
/// boolean schema in that position, so emit an explicit object schema for the
/// items instead. Stimuli are a Phase 11 feature, validated/rejected at runtime.
fn stimuli_schema(_generator: &mut schemars::SchemaGenerator) -> schemars::Schema {
    schemars::json_schema!({
        "type": "array",
        "items": { "type": "object" },
        "description": "Time-ordered writes applied to drivable inputs. Phase 11 feature."
    })
}

#[derive(Debug, Default, Deserialize, JsonSchema)]
pub struct TraceOptions {
    #[serde(default)]
    pub mode: Option<String>,
    #[serde(default)]
    pub interval_ms: Option<u64>,
    #[serde(default)]
    pub max_samples: Option<usize>,
}

#[derive(Debug, Default, Deserialize, JsonSchema)]
pub struct LimitOverrides {
    #[serde(default)]
    pub max_duration_ms: Option<u64>,
    #[serde(default)]
    pub max_fuel: Option<u64>,
    #[serde(default)]
    pub max_wall_clock_ms: Option<u64>,
    #[serde(default)]
    pub max_samples: Option<usize>,
    #[serde(default)]
    pub max_variables_per_run: Option<usize>,
}

/// Top-level response for the `run` tool.
#[derive(Debug, Serialize)]
pub struct RunResponse {
    pub ok: bool,
    pub trace: Vec<TraceEntry>,
    pub truncated: bool,
    pub terminated_reason: String,
    pub summary: RunSummary,
    pub diagnostics: Vec<Value>,
}

#[derive(Debug, Serialize)]
pub struct TraceEntry {
    pub time_ms: u64,
    pub task: String,
    pub variables: Map<String, Value>,
}

#[derive(Debug, Serialize)]
pub struct RunSummary {
    pub final_values: Map<String, Value>,
    pub completed_cycles: Map<String, Value>,
    pub terminated_reason: String,
    /// The cycle time the run executed at, in milliseconds — the task's
    /// declared `INTERVAL`, or the assumed one when it declares none
    /// (REQ-TOL-mcp-049). Every `time_ms` in the trace is a multiple of it.
    pub interval_ms: f64,
}

/// Entry point. Validates input, resolves the trace set, runs the VM,
/// and returns a `RunResponse`. Never returns an MCP-layer error —
/// compiler-surfaced problems always come back as diagnostics
/// (REQ-TOL-mcp-024).
pub fn build_response(input: &RunInput, cache: &Mutex<ContainerCache>) -> RunResponse {
    // --- Shape validation (REQ-TOL-mcp-040 sentence 1) ---
    match (&input.container_id, &input.container_base64) {
        (None, None) => {
            return fail(vec![validation(
                text::mcp_input_validation_run_container_missing(),
            )]);
        }
        (Some(_), Some(_)) => {
            return fail(vec![validation(
                text::mcp_input_validation_run_container_both(),
            )]);
        }
        _ => {}
    }

    if input.container_base64.is_some() {
        return fail(vec![validation(
            text::mcp_input_validation_run_container_base64_deferred(),
        )]);
    }

    // --- Phase 11 feature guards ---
    if !input.stimuli.is_empty() {
        return fail(vec![validation(
            text::mcp_input_validation_run_stimuli_unimplemented(),
        )]);
    }
    if input.tasks.is_some() {
        return fail(vec![validation(
            text::mcp_input_validation_run_tasks_filter_unimplemented(),
        )]);
    }
    if let Some(trace) = &input.trace {
        if let Some(mode) = trace.mode.as_deref() {
            if mode != "every_cycle" {
                return fail(vec![validation(
                    text::mcp_input_validation_run_trace_mode_unimplemented(mode),
                )]);
            }
        }
    }

    // --- Limit override validation (REQ-ARC-mcp-031) ---
    let defaults = EffectiveLimits::DEFAULTS;
    let limits = match resolve_limits(defaults, input.limits.as_ref()) {
        Ok(l) => l,
        Err(diags) => return fail(diags),
    };

    // --- Container lookup (REQ-ARC-mcp-073) ---
    let container_id = input.container_id.as_deref().unwrap_or_default();
    let mut guard = cache.lock().unwrap_or_else(|e| e.into_inner());
    let cached = match guard.get(container_id) {
        Some(c) => c,
        None => {
            return fail(vec![validation(
                text::mcp_input_validation_run_container_unknown(container_id),
            )]);
        }
    };

    // REQ-TOL-mcp-040 sentence 4: a container with no tasks cannot be scheduled.
    if cached.tasks.is_empty() {
        return fail(vec![validation(text::mcp_input_validation_run_no_tasks())]);
    }

    // --- Freewheeling cycle time (REQ-TOL-mcp-049) ---
    let freewheeling_interval =
        match resolve_freewheeling_interval(input.freewheeling_interval_ms, &cached.tasks) {
            Ok(interval) => interval,
            Err(diags) => return fail(diags),
        };

    // --- Trace set resolution (REQ-TOL-mcp-041, REQ-ARC-mcp-020/021) ---
    let trace_set = match resolve_trace_set(
        &input.variables,
        input.trace_outputs,
        &cached.symbols,
        limits.max_variables_per_run,
    ) {
        Ok(set) => set,
        Err(diags) => return fail(diags),
    };

    // --- Apply a caller-supplied `trace.max_samples` as a tightening cap ---
    let effective_samples = input
        .trace
        .as_ref()
        .and_then(|t| t.max_samples)
        .map(|n| n.min(limits.max_samples))
        .unwrap_or(limits.max_samples);
    let effective_limits = EffectiveLimits {
        max_samples: effective_samples,
        ..limits
    };

    // Clone what we need from the cache so we can release the lock
    // before calling into the VM (VM execution can take up to
    // `max_wall_clock_ms` and must not block other MCP calls).
    let cached_snapshot = (*cached).clone_for_run();
    drop(guard);

    // --- Execute ---
    let outcome = match runner::execute(
        &cached_snapshot,
        &trace_set,
        input.duration_ms,
        freewheeling_interval,
        effective_limits,
    ) {
        Ok(o) => o,
        Err(msg) => {
            let diag = Diagnostic::internal_error_at(Label::span(SourceSpan::default(), msg));
            return fail(vec![diag]);
        }
    };

    build_success_response(outcome, input.duration_ms, effective_limits)
}

fn build_success_response(
    outcome: RunOutcome,
    requested_duration_ms: u64,
    limits: EffectiveLimits,
) -> RunResponse {
    let trace: Vec<TraceEntry> = outcome
        .trace
        .into_iter()
        .map(|s| TraceEntry {
            time_ms: s.time_ms,
            task: s.task,
            variables: s.variables.into_iter().collect(),
        })
        .collect();

    let final_values: Map<String, Value> = outcome.final_values.into_iter().collect();
    let completed_cycles: Map<String, Value> = outcome
        .completed_cycles
        .into_iter()
        .map(|(name, count)| (name, Value::from(count)))
        .collect();

    let mut diagnostics = Vec::new();
    let reason = outcome.terminated_reason;
    let stop_message = match reason {
        TerminatedReason::Completed => None,
        TerminatedReason::Duration => Some(text::mcp_input_validation_run_duration_limited(
            requested_duration_ms,
            limits.max_duration_ms,
        )),
        TerminatedReason::Fuel => Some(text::mcp_input_validation_run_fuel_exhausted()),
        TerminatedReason::WallClock => Some(text::mcp_input_validation_run_wall_clock_exceeded()),
        TerminatedReason::SampleCap => Some(text::mcp_input_validation_run_sample_cap_reached()),
        TerminatedReason::Error => Some(match outcome.error_message.as_deref() {
            Some(reason) => text::mcp_input_validation_run_vm_error(reason),
            None => text::mcp_input_validation_run_vm_trap(),
        }),
    };
    if let Some(message) = stop_message {
        diagnostics.push(serialize_diagnostic(&validation(message), &[]));
    }

    let reason_str = reason.as_str().to_string();
    RunResponse {
        ok: reason == TerminatedReason::Completed,
        trace,
        truncated: outcome.truncated,
        terminated_reason: reason_str.clone(),
        summary: RunSummary {
            final_values,
            completed_cycles,
            terminated_reason: reason_str,
            interval_ms: outcome.interval.as_secs_f64() * 1_000.0,
        },
        diagnostics,
    }
}

/// Resolves the cycle time for the container's freewheeling tasks
/// (REQ-TOL-mcp-049).
///
/// A freewheeling task declares no rate, and a simulated run cannot proceed
/// without one: nothing would advance the clock, so `duration_ms` could never
/// end the run. The server does not pick a rate on the caller's behalf — a
/// scan time is a property of the hardware the program would run on, and a
/// trace built on a number the agent never chose is a trap. So the run is
/// rejected, with a diagnostic naming what to supply.
///
/// A supplied value must be a duration a task could actually run at, which
/// `interval_from_ms` decides; the rejection quotes that rule rather than
/// restating it, so the message cannot claim a bound the check does not
/// enforce. `None` comes back when the container declares an `INTERVAL` for
/// every task, where there is nothing to assume.
fn resolve_freewheeling_interval(
    interval_ms: Option<f64>,
    tasks: &[TaskMeta],
) -> Result<Option<Duration>, Vec<Diagnostic>> {
    let Some(ms) = interval_ms else {
        if let Some(task) = tasks.iter().find(|t| t.kind == TaskKind::Freewheeling) {
            return Err(vec![validation(
                text::mcp_input_validation_run_freewheeling_interval_missing(&task.name),
            )]);
        }
        return Ok(None);
    };

    interval_from_ms(ms).map(Some).map_err(|e| {
        vec![validation(
            text::mcp_input_validation_run_freewheeling_interval_invalid(ms, e),
        )]
    })
}

fn resolve_limits(
    defaults: EffectiveLimits,
    overrides: Option<&LimitOverrides>,
) -> Result<EffectiveLimits, Vec<Diagnostic>> {
    let Some(o) = overrides else {
        return Ok(defaults);
    };
    let mut errors = Vec::new();
    let mut out = defaults;

    if let Some(v) = o.max_duration_ms {
        if v > defaults.max_duration_ms {
            errors.push(validation(
                text::mcp_input_validation_run_limit_exceeds_default(
                    "max_duration_ms",
                    v,
                    defaults.max_duration_ms,
                ),
            ));
        } else {
            out.max_duration_ms = v;
        }
    }
    if let Some(v) = o.max_fuel {
        if v > defaults.max_fuel {
            errors.push(validation(
                text::mcp_input_validation_run_limit_exceeds_default(
                    "max_fuel",
                    v,
                    defaults.max_fuel,
                ),
            ));
        } else {
            out.max_fuel = v;
        }
    }
    if let Some(v) = o.max_wall_clock_ms {
        if v > defaults.max_wall_clock_ms {
            errors.push(validation(
                text::mcp_input_validation_run_limit_exceeds_default(
                    "max_wall_clock_ms",
                    v,
                    defaults.max_wall_clock_ms,
                ),
            ));
        } else {
            out.max_wall_clock_ms = v;
        }
    }
    if let Some(v) = o.max_samples {
        if v > defaults.max_samples {
            errors.push(validation(
                text::mcp_input_validation_run_limit_exceeds_default(
                    "max_samples",
                    v,
                    defaults.max_samples,
                ),
            ));
        } else {
            out.max_samples = v;
        }
    }
    if let Some(v) = o.max_variables_per_run {
        if v > defaults.max_variables_per_run {
            errors.push(validation(
                text::mcp_input_validation_run_limit_exceeds_default(
                    "max_variables_per_run",
                    v,
                    defaults.max_variables_per_run,
                ),
            ));
        } else {
            out.max_variables_per_run = v;
        }
    }

    if errors.is_empty() {
        Ok(out)
    } else {
        Err(errors)
    }
}

/// Resolves every `requested` name into a `ResolvedVar`, expanding
/// `trace_outputs` to every observable output and enforcing the
/// `max_variables_per_run` cap.
fn resolve_trace_set(
    requested: &[String],
    trace_outputs: bool,
    symbols: &VariableSymbolMap,
    max_variables: usize,
) -> Result<Vec<ResolvedVar>, Vec<Diagnostic>> {
    let mut seen: std::collections::HashSet<String> = std::collections::HashSet::new();
    let mut out: Vec<ResolvedVar> = Vec::new();
    let mut errors: Vec<Diagnostic> = Vec::new();

    for name in requested {
        if name.contains('*') {
            errors.push(validation(text::mcp_input_validation_run_wildcard_name(
                name,
            )));
            continue;
        }
        match resolve_name(symbols, name) {
            Ok(resolved) => {
                if seen.insert(resolved.canonical_name.clone()) {
                    out.push(resolved);
                }
            }
            Err(NameError::Unresolved) => {
                errors.push(validation(
                    text::mcp_input_validation_run_variable_unresolved(name),
                ));
            }
            Err(NameError::Ambiguous(candidates)) => {
                errors.push(validation(
                    text::mcp_input_validation_run_variable_ambiguous(name, candidates.join(", ")),
                ));
            }
        }
    }

    if trace_outputs {
        for v in symbols.iter() {
            if is_observable_output(v) && seen.insert(v.canonical_name.clone()) {
                out.push(v.clone());
            }
        }
    }

    if !errors.is_empty() {
        return Err(errors);
    }

    if out.len() > max_variables {
        return Err(vec![validation(
            text::mcp_input_validation_run_trace_set_too_large(out.len(), max_variables),
        )]);
    }

    Ok(out)
}

/// Mirrors the `project_io::classify` output rules (REQ-TOL-mcp-211) without
/// re-running analysis — operates on the pre-built symbol map.
fn is_observable_output(v: &ResolvedVar) -> bool {
    use ironplc_container::debug_section::var_section;
    let addr = v.address.as_deref();
    let is_hw_output = addr.is_some_and(|a| a.starts_with("%Q"));
    matches!(
        v.var_section,
        var_section::VAR_OUTPUT | var_section::VAR_IN_OUT
    ) || is_hw_output
        || (v.program.is_none() && addr.is_none() && v.var_section == var_section::VAR_GLOBAL)
}

#[derive(Debug)]
enum NameError {
    Unresolved,
    Ambiguous(Vec<String>),
}

/// Resolves a fully-qualified or bare name per REQ-ARC-mcp-020.
fn resolve_name(symbols: &VariableSymbolMap, requested: &str) -> Result<ResolvedVar, NameError> {
    // Qualified lookup first.
    if requested.contains('.') {
        return symbols
            .by_qualified(requested)
            .cloned()
            .ok_or(NameError::Unresolved);
    }

    // Bare name: prefer globals, then single match, else ambiguous.
    let candidates = symbols.by_bare(requested);
    if candidates.is_empty() {
        return Err(NameError::Unresolved);
    }
    let globals: Vec<&ResolvedVar> = candidates.iter().filter(|c| c.program.is_none()).collect();
    if globals.len() == 1 {
        return Ok(globals[0].clone());
    }
    if candidates.len() == 1 {
        return Ok(candidates[0].clone());
    }
    let names: Vec<String> = candidates
        .iter()
        .map(|c| c.canonical_name.clone())
        .collect();
    Err(NameError::Ambiguous(names))
}

fn validation(text: Text) -> Diagnostic {
    Diagnostic::text(SourceSpan::default(), text)
}

fn fail(diags: Vec<Diagnostic>) -> RunResponse {
    RunResponse {
        ok: false,
        trace: vec![],
        truncated: false,
        terminated_reason: "error".into(),
        summary: RunSummary {
            final_values: Map::new(),
            completed_cycles: Map::new(),
            terminated_reason: "error".into(),
            interval_ms: 0.0,
        },
        diagnostics: serialize_diagnostics(&diags, &[]),
    }
}

// Allow `run` to snapshot the cached container under the lock so it can
// release the lock before calling into the VM. Implemented as a trait-
// style inherent extension on `CachedContainer` to avoid making the
// full struct `Clone`-able (bytes can be large).
impl crate::cache::CachedContainer {
    fn clone_for_run(&self) -> Self {
        crate::cache::CachedContainer::new(
            self.iplc_bytes.clone(),
            self.tasks.clone(),
            self.programs.clone(),
            self.symbols.clone(),
        )
    }
}

#[cfg(test)]
mod tests;
