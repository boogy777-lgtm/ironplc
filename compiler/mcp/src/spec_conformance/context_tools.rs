//! Conformance tests for the context tools: project manifest, project I/O, POU scope, POU lineage, and all-types.

use spec_test_macro::spec_test;

use crate::tools;

use crate::tools::test_support::{
    ed2_options, source, PROGRAM_USING_FB, PROGRAM_USING_STDLIB_FB, PROGRAM_WITH_INPUT,
    PROGRAM_WITH_INPUT_AND_LOCAL, SEMANTIC_ERROR_PROGRAM, USER_TYPES_PROGRAM,
};

// ===========================================================================
// Context tools: `project_manifest` (REQ-TOL-mcp-200..201) — Milestone 1 (later)
// ===========================================================================

/// REQ-TOL-mcp-200: The `project_manifest` tool returns file names, POU names,
/// and UDTs grouped by kind.
#[spec_test(REQ_TOL_mcp_200)]
fn mcp_spec_req_tol_200_project_manifest_returns_declarations() {
    use crate::tools::common::SourceInput;

    let sources = vec![SourceInput {
        name: "main.st".into(),
        content: "TYPE MyEnum : (A, B, C); END_TYPE\n\
                  FUNCTION_BLOCK fb\nEND_FUNCTION_BLOCK\n\
                  PROGRAM p\nVAR inst : fb; END_VAR\nEND_PROGRAM"
            .into(),
    }];
    let options = ed2_options();

    let resp = tools::project_manifest::build_response(&sources, &options);
    let json = serde_json::to_value(&resp).unwrap();

    assert_eq!(json["ok"], true, "diagnostics: {}", json["diagnostics"]);
    assert!(json["files"]
        .as_array()
        .unwrap()
        .iter()
        .any(|v| v == "main.st"));
    assert!(json["programs"]
        .as_array()
        .unwrap()
        .iter()
        .any(|v| v == "p"));
    assert!(json["function_blocks"]
        .as_array()
        .unwrap()
        .iter()
        .any(|v| v == "fb"));
    assert!(json["enumerations"]
        .as_array()
        .unwrap()
        .iter()
        .any(|v| v == "MyEnum"));

    // All seven UDT buckets are always present as arrays, even when empty.
    for key in [
        "enumerations",
        "structures",
        "arrays",
        "subranges",
        "aliases",
        "strings",
        "references",
    ] {
        assert!(json[key].is_array(), "bucket {key} should be an array");
    }
}

/// REQ-TOL-mcp-201: On semantic failure, `project_manifest` returns `ok: false`,
/// a partial manifest, and analysis diagnostics.
#[spec_test(REQ_TOL_mcp_201)]
fn mcp_spec_req_tol_201_project_manifest_partial_on_failure() {
    let sources = source(SEMANTIC_ERROR_PROGRAM);
    let options = ed2_options();

    let resp = tools::project_manifest::build_response(&sources, &options);
    let json = serde_json::to_value(&resp).unwrap();

    assert_eq!(json["ok"], false);
    assert!(!json["diagnostics"].as_array().unwrap().is_empty());
    // Partial manifest preserved even though semantic analysis failed.
    assert!(json["files"]
        .as_array()
        .unwrap()
        .iter()
        .any(|v| v == "main.st"));
    assert!(json["programs"]
        .as_array()
        .unwrap()
        .iter()
        .any(|v| v == "p"));
}

// ===========================================================================
// Context tools: `project_io` (REQ-TOL-mcp-210..212)
// ===========================================================================

/// REQ-TOL-mcp-210: Every variable drivable from outside the program — program
/// `VAR_INPUT`/`VAR_IN_OUT`, `VAR_EXTERNAL`, non-addressed globals, and
/// `%I*` hardware inputs — appears in the `inputs` array.
#[spec_test(REQ_TOL_mcp_210)]
fn mcp_spec_req_tol_210_project_io_returns_inputs() {
    use crate::tools::common::SourceInput;

    let sources = vec![SourceInput {
        name: "main.st".into(),
        content: "PROGRAM p\n\
                  VAR_INPUT start : BOOL; END_VAR\n\
                  VAR_IN_OUT swap : BOOL; END_VAR\n\
                  VAR button AT %IX0.0 : BOOL; END_VAR\n\
                  END_PROGRAM"
            .into(),
    }];
    let options = ed2_options();

    let resp = tools::project_io::build_response(&sources, &options);
    let json = serde_json::to_value(&resp).unwrap();

    assert_eq!(json["ok"], true, "diagnostics: {}", json["diagnostics"]);
    let inputs = json["inputs"].as_array().unwrap();
    assert!(inputs.iter().any(|v| v["name"] == "p.start"));
    assert!(inputs.iter().any(|v| v["name"] == "p.swap"));
    assert!(inputs.iter().any(|v| v["address"] == "%IX0.0"));
}

/// REQ-TOL-mcp-211: Every variable observable outside the program — program
/// `VAR_OUTPUT`/`VAR_IN_OUT`, non-addressed globals, and `%Q*` hardware
/// outputs — appears in `outputs`. `VAR_IN_OUT` and non-addressed globals
/// appear in both arrays. `%M*` marker memory appears in neither.
#[spec_test(REQ_TOL_mcp_211)]
fn mcp_spec_req_tol_211_project_io_returns_outputs() {
    use crate::tools::common::SourceInput;

    let sources = vec![SourceInput {
        name: "main.st".into(),
        content: "PROGRAM p\n\
                  VAR_OUTPUT done : BOOL; END_VAR\n\
                  VAR_IN_OUT swap : BOOL; END_VAR\n\
                  VAR buzzer AT %QX0.0 : BOOL; END_VAR\n\
                  VAR marker AT %MX0.0 : BOOL; END_VAR\n\
                  END_PROGRAM"
            .into(),
    }];
    let options = ed2_options();

    let resp = tools::project_io::build_response(&sources, &options);
    let json = serde_json::to_value(&resp).unwrap();

    assert_eq!(json["ok"], true, "diagnostics: {}", json["diagnostics"]);
    let inputs = json["inputs"].as_array().unwrap();
    let outputs = json["outputs"].as_array().unwrap();
    assert!(outputs.iter().any(|v| v["name"] == "p.done"));
    assert!(outputs.iter().any(|v| v["address"] == "%QX0.0"));
    // VAR_IN_OUT appears in both arrays.
    assert!(inputs.iter().any(|v| v["name"] == "p.swap"));
    assert!(outputs.iter().any(|v| v["name"] == "p.swap"));
    // %M* appears in neither.
    assert!(!inputs.iter().any(|v| v["address"] == "%MX0.0"));
    assert!(!outputs.iter().any(|v| v["address"] == "%MX0.0"));
}

/// REQ-TOL-mcp-212: Each entry in `inputs`/`outputs` has `name`, `type`, and
/// `address` (string or null).
#[spec_test(REQ_TOL_mcp_212)]
fn mcp_spec_req_tol_212_project_io_entry_format() {
    let sources = source(PROGRAM_WITH_INPUT);
    let options = ed2_options();

    let resp = tools::project_io::build_response(&sources, &options);
    let json = serde_json::to_value(&resp).unwrap();

    assert_eq!(json["ok"], true, "diagnostics: {}", json["diagnostics"]);
    let entry = json["inputs"]
        .as_array()
        .unwrap()
        .iter()
        .find(|e| e["name"] == "p.start")
        .unwrap();
    assert!(entry.get("name").is_some());
    assert!(entry.get("type").is_some());
    assert!(entry.get("address").is_some());
    // `address` is `null` when the variable is not hardware-mapped.
    assert!(entry["address"].is_null());
}

// ===========================================================================
// Context tools: `pou_scope` (REQ-TOL-mcp-220..221)
// ===========================================================================

/// REQ-TOL-mcp-220: `pou_scope` returns the queried POU's variables with
/// `name`, `type`, `direction`, and `initial_value` fields.
#[spec_test(REQ_TOL_mcp_220)]
fn mcp_spec_req_tol_220_pou_scope_returns_variables() {
    let sources = source(PROGRAM_WITH_INPUT_AND_LOCAL);
    let options = ed2_options();

    let resp = tools::pou_scope::build_response(&sources, &options, "p");
    assert!(resp.ok, "diagnostics: {:?}", resp.diagnostics);
    assert!(resp.found);

    let start = resp.variables.iter().find(|v| v.name == "start").unwrap();
    assert_eq!(start.direction, "In");
    assert_eq!(start.initial_value.as_deref(), Some("FALSE"));
    assert!(!start.type_name.is_empty());

    let count = resp.variables.iter().find(|v| v.name == "count").unwrap();
    assert_eq!(count.direction, "Local");
    assert_eq!(count.initial_value, None);
}

/// REQ-TOL-mcp-221: `pou_scope` returns `ok:false`, `found:false`, empty
/// `variables`, and a diagnostic when the POU does not exist. Program
/// resolution also wins over Function Block when names collide.
#[spec_test(REQ_TOL_mcp_221)]
fn mcp_spec_req_tol_221_pou_scope_unknown_pou() {
    use crate::tools::common::SourceInput;

    let sources = vec![SourceInput {
        name: "main.st".into(),
        content: "FUNCTION_BLOCK Motor\nVAR fb_var : INT; END_VAR\nEND_FUNCTION_BLOCK\nPROGRAM Motor\nVAR prog_var : INT; END_VAR\nEND_PROGRAM".into(),
    }];
    let options = ed2_options();

    // Program resolves first.
    let resp = tools::pou_scope::build_response(&sources, &options, "Motor");
    assert!(resp.found);
    assert!(resp.variables.iter().any(|v| v.name == "prog_var"));
    assert!(resp.variables.iter().all(|v| v.name != "fb_var"));

    // Unknown POU.
    let missing = tools::pou_scope::build_response(&sources, &options, "nonexistent");
    assert!(!missing.ok);
    assert!(!missing.found);
    assert!(missing.variables.is_empty());
    assert!(missing.diagnostics.iter().any(|d| d["number"] == 8001));
}

// ===========================================================================
// Context tools: `pou_lineage` (REQ-TOL-mcp-230..231)
// ===========================================================================

/// REQ-TOL-mcp-230: `pou_lineage` returns `pou`, `upstream`, and `downstream`
/// adjacency lists derived from the project dependency DAG, each entry
/// carrying its `name` and `source`.
#[spec_test(REQ_TOL_mcp_230)]
fn mcp_spec_req_tol_230_pou_lineage_returns_dependencies() {
    use crate::tools::pou_lineage::PouSource;

    let sources = source(PROGRAM_USING_FB);
    let options = ed2_options();

    // Main depends on Counter → Counter is upstream.
    let main_resp = tools::pou_lineage::build_response(&sources, &options, "Main");
    assert!(main_resp.ok, "diagnostics: {:?}", main_resp.diagnostics);
    assert!(main_resp.found);
    let counter = main_resp
        .upstream
        .iter()
        .find(|e| e.name.eq_ignore_ascii_case("Counter"))
        .expect("Counter is upstream of Main");
    assert_eq!(counter.source, PouSource::User);

    // Counter has Main as downstream.
    let counter_resp = tools::pou_lineage::build_response(&sources, &options, "Counter");
    assert!(counter_resp.ok);
    assert!(counter_resp.found);
    assert!(counter_resp
        .downstream
        .iter()
        .any(|e| e.name.eq_ignore_ascii_case("Main")));
}

/// REQ-TOL-mcp-232: standard library POUs appear in lineage tagged
/// `source: "stdlib"` and are addressable as the queried POU.
#[spec_test(REQ_TOL_mcp_232)]
fn mcp_spec_req_tol_232_pou_lineage_reports_stdlib_pous() {
    use crate::tools::pou_lineage::PouSource;

    let sources = source(PROGRAM_USING_STDLIB_FB);
    let options = ed2_options();

    let resp = tools::pou_lineage::build_response(&sources, &options, "MotorStartStop");
    assert!(resp.ok, "diagnostics: {:?}", resp.diagnostics);
    assert!(resp.found);
    let timer = resp
        .upstream
        .iter()
        .find(|e| e.name.eq_ignore_ascii_case("TON"))
        .expect("TON is upstream of MotorStartStop");
    assert_eq!(timer.source, PouSource::Stdlib);

    // The standard library POU is itself addressable.
    let ton_resp = tools::pou_lineage::build_response(&sources, &options, "TON");
    assert!(ton_resp.found);
    assert!(ton_resp
        .downstream
        .iter()
        .any(|e| e.name.eq_ignore_ascii_case("MotorStartStop")));
}

/// REQ-TOL-mcp-231: `pou_lineage` returns `ok:false`, `found:false`, empty
/// upstream/downstream arrays, and a diagnostic for an unknown POU.
#[spec_test(REQ_TOL_mcp_231)]
fn mcp_spec_req_tol_231_pou_lineage_unknown_pou() {
    use crate::tools::common::SourceInput;

    let sources = vec![SourceInput {
        name: "main.st".into(),
        content: "PROGRAM Main\nEND_PROGRAM".into(),
    }];
    let options = ed2_options();

    let resp = tools::pou_lineage::build_response(&sources, &options, "nonexistent");
    assert!(!resp.ok);
    assert!(!resp.found);
    assert!(resp.upstream.is_empty());
    assert!(resp.downstream.is_empty());
    assert!(resp.diagnostics.iter().any(|d| d["number"] == 8001));
}

// ===========================================================================
// Context tools: `types_all` (REQ-TOL-mcp-240)
// ===========================================================================

/// REQ-TOL-mcp-240: `types_all` returns every user-defined type with kind-specific
/// detail fields.
#[spec_test(REQ_TOL_mcp_240)]
fn mcp_spec_req_tol_240_types_all_returns_user_defined_types() {
    let sources = source(USER_TYPES_PROGRAM);
    let options = ed2_options();

    let resp = tools::types_all::build_response(&sources, &options);
    assert!(resp.ok, "diagnostics: {:?}", resp.diagnostics);

    let motor = resp.types.iter().find(|t| t.name == "MotorState").unwrap();
    assert_eq!(motor.kind, "enum");
    assert!(motor.values.is_some());

    let pid = resp.types.iter().find(|t| t.name == "PidParams").unwrap();
    assert_eq!(pid.kind, "struct");
    assert!(pid.fields.is_some());

    let buf = resp.types.iter().find(|t| t.name == "Buf").unwrap();
    assert_eq!(buf.kind, "array");
    assert!(buf.element_type.is_some());
    assert!(buf.bounds.is_some());

    let percent = resp.types.iter().find(|t| t.name == "Percent").unwrap();
    assert_eq!(percent.kind, "subrange");
    assert_eq!(percent.low, Some(0));
    assert_eq!(percent.high, Some(100));
}
