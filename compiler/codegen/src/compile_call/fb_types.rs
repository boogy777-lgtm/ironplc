//! Resolves standard function block type names to their type id, field count and field map.

use std::collections::HashMap;

use ironplc_container::opcode;

// --- Standard function block type resolution and field maps ---

/// Resolves a standard FB type name to its (type_id, total_num_fields, field_name->index map).
/// Returns None for unknown FB types.
pub(crate) fn resolve_fb_type(name: &str) -> Option<(u16, usize, HashMap<String, u8>)> {
    match name {
        "TON" => Some((opcode::fb_type::TON, 6, timer_fb_fields())),
        "TOF" => Some((opcode::fb_type::TOF, 6, timer_fb_fields())),
        "TP" => Some((opcode::fb_type::TP, 6, timer_fb_fields())),
        "CTU" | "CTU_INT" | "CTU_DINT" | "CTU_LINT" | "CTU_UDINT" | "CTU_ULINT" => {
            Some((opcode::fb_type::CTU, 6, ctu_fb_fields()))
        }
        "CTD" | "CTD_INT" | "CTD_DINT" | "CTD_LINT" | "CTD_UDINT" | "CTD_ULINT" => {
            Some((opcode::fb_type::CTD, 6, ctd_fb_fields()))
        }
        "CTUD" | "CTUD_INT" | "CTUD_DINT" | "CTUD_LINT" | "CTUD_UDINT" | "CTUD_ULINT" => {
            Some((opcode::fb_type::CTUD, 10, ctud_fb_fields()))
        }
        "SR" => Some((opcode::fb_type::SR, 3, sr_fb_fields())),
        "RS" => Some((opcode::fb_type::RS, 3, rs_fb_fields())),
        "R_TRIG" => Some((opcode::fb_type::R_TRIG, 3, edge_trig_fb_fields())),
        "F_TRIG" => Some((opcode::fb_type::F_TRIG, 3, edge_trig_fb_fields())),
        _ => None,
    }
}

/// Returns the shared field map for timer FBs (TON, TOF, TP).
/// Fields 4-5 are hidden (start_time, running) and not included.
fn timer_fb_fields() -> HashMap<String, u8> {
    let mut fields = HashMap::new();
    fields.insert("in".to_string(), 0);
    fields.insert("pt".to_string(), 1);
    fields.insert("q".to_string(), 2);
    fields.insert("et".to_string(), 3);
    fields
}

/// Returns the field map for CTU (count up) FBs.
/// Field 5 is hidden (prev_cu) and not included.
fn ctu_fb_fields() -> HashMap<String, u8> {
    let mut fields = HashMap::new();
    fields.insert("cu".to_string(), 0);
    fields.insert("r".to_string(), 1);
    fields.insert("pv".to_string(), 2);
    fields.insert("q".to_string(), 3);
    fields.insert("cv".to_string(), 4);
    fields
}

/// Returns the field map for CTD (count down) FBs.
/// Field 5 is hidden (prev_cd) and not included.
fn ctd_fb_fields() -> HashMap<String, u8> {
    let mut fields = HashMap::new();
    fields.insert("cd".to_string(), 0);
    fields.insert("ld".to_string(), 1);
    fields.insert("pv".to_string(), 2);
    fields.insert("q".to_string(), 3);
    fields.insert("cv".to_string(), 4);
    fields
}

/// Returns the field map for CTUD (count up/down) FBs.
/// Fields 8-9 are hidden (prev_cu, prev_cd) and not included.
fn ctud_fb_fields() -> HashMap<String, u8> {
    let mut fields = HashMap::new();
    fields.insert("cu".to_string(), 0);
    fields.insert("cd".to_string(), 1);
    fields.insert("r".to_string(), 2);
    fields.insert("ld".to_string(), 3);
    fields.insert("pv".to_string(), 4);
    fields.insert("qu".to_string(), 5);
    fields.insert("qd".to_string(), 6);
    fields.insert("cv".to_string(), 7);
    fields
}

/// Returns the field map for SR (set-reset) FBs.
fn sr_fb_fields() -> HashMap<String, u8> {
    let mut fields = HashMap::new();
    fields.insert("s1".to_string(), 0);
    fields.insert("r".to_string(), 1);
    fields.insert("q1".to_string(), 2);
    fields
}

/// Returns the field map for RS (reset-set) FBs.
fn rs_fb_fields() -> HashMap<String, u8> {
    let mut fields = HashMap::new();
    fields.insert("s".to_string(), 0);
    fields.insert("r1".to_string(), 1);
    fields.insert("q1".to_string(), 2);
    fields
}

/// Returns the field map for edge trigger FBs (R_TRIG, F_TRIG).
/// Field 2 is hidden (M / previous CLK) and not included.
fn edge_trig_fb_fields() -> HashMap<String, u8> {
    let mut fields = HashMap::new();
    fields.insert("clk".to_string(), 0);
    fields.insert("q".to_string(), 1);
    fields
}
