//! Options that affect code generation, and the engineering-side keys they carry.

use ironplc_dsl::core::Id;
use ironplc_parser::options::{CompilerOptions, StringToNumFailure, StringToNumNonNumeric};

/// An engineering-side key for a function-block field's stable UID
/// (ADR 0059): the qualified FB type name and the field name. This is the
/// UID sidecar's `(scope, name)` key, where an FB field's scope is its type
/// name; `Id` semantics make both comparisons case-insensitive.
#[derive(Debug, Clone, PartialEq, Eq)]
pub struct FbFieldUidKey {
    /// The qualified name of the declaring FB type.
    pub fb_type: Id,
    /// The field's declaration name.
    pub field: Id,
}

impl FbFieldUidKey {
    /// Creates a key from an FB type name and a field name.
    pub fn new(fb_type: &str, field: &str) -> Self {
        FbFieldUidKey {
            fb_type: Id::from(fb_type),
            field: Id::from(field),
        }
    }
}

/// Options that affect code generation.
///
/// Every front end derives this from the project's [`CompilerOptions`] via
/// [`From`], so the mapping from a compiler option to what codegen does with
/// it has exactly one definition. Options the project model owns rather than
/// the parser (the stable variable IDs, ADR 0053, and the FB field UIDs,
/// ADR 0059) are set on the derived value by the orchestrator.
#[derive(Debug, Default, Clone)]
pub struct CodegenOptions {
    /// When `true`, inject `__SYSTEM_UP_TIME` (TIME) and `__SYSTEM_UP_LTIME`
    /// (LTIME) as implicit globals at the start of the variable table.
    pub system_uptime_global: bool,
    /// The behavior policies `STRING_TO_<numeric>` calls are compiled under
    /// (ADR-0049). They select the builtin func_id the call emits.
    pub string_to_num: StringToNumPolicies,
    /// Engineering-side entity UIDs keyed by the current declaration name
    /// (ADR 0053). Names are matched with [`Id`] semantics, so the
    /// comparison is case-insensitive; a name no persistent variable has is
    /// ignored. The project API supplies the table, so the derived
    /// [`From<&CompilerOptions>`] value leaves it empty.
    pub stable_var_ids: Vec<(Id, u64)>,
    /// Engineering-side entity UIDs for user-defined function-block fields
    /// (ADR 0059), keyed by ([`FbFieldUidKey`], uid). A field the table names
    /// records an FB field UID entry at its `(fb_type_id, field_index)` in
    /// the container's type section; fields the table does not name carry no
    /// entry, and the migration planner treats them as fail-closed when the
    /// FB layout changes.
    pub fb_field_uids: Vec<(FbFieldUidKey, u64)>,
}

/// The two behavior policies of a `STRING_TO_<numeric>` conversion.
#[derive(Debug, Default, Clone, Copy, PartialEq, Eq)]
pub struct StringToNumPolicies {
    pub non_numeric: StringToNumNonNumeric,
    pub failure: StringToNumFailure,
}

impl From<&CompilerOptions> for CodegenOptions {
    fn from(options: &CompilerOptions) -> Self {
        CodegenOptions {
            system_uptime_global: options.allow_system_uptime_global,
            string_to_num: StringToNumPolicies {
                non_numeric: options.policy_string_to_num_non_numeric,
                failure: options.policy_string_to_num_failure,
            },
            stable_var_ids: Vec::new(),
            fb_field_uids: Vec::new(),
        }
    }
}
