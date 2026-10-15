//! Disassembler for IPLC bytecode containers.
//!
//! Reads an IPLC bytecode container and produces structured JSON suitable
//! for display in a VS Code custom editor. The output includes the file
//! header, constant pool entries, and decoded bytecode instructions with
//! cross-referenced operands.

use std::fs::File;
use std::io::BufReader;
use std::path::Path;

use ironplc_container::opcode;
use ironplc_container::opcode::{DecodeStop, DecodedInstruction, Instruction, Operand};
use ironplc_container::{
    ConstType, Container, FLAG_HAS_DEBUG_SECTION, FLAG_HAS_SYSTEM_UPTIME, FLAG_HAS_TYPE_SECTION,
};
use serde_json::{json, Value};

/// Disassembles a bytecode container into a structured JSON value.
///
/// The returned JSON has three top-level keys:
/// - `header`: file header fields
/// - `constants`: array of constant pool entries
/// - `functions`: array of function disassemblies with decoded instructions
pub fn disassemble(container: &Container) -> Value {
    let header = disassemble_header(container);
    let task_table = disassemble_task_table(container);
    let constants = disassemble_constants(container);
    let functions = disassemble_functions(container);

    json!({
        "header": header,
        "taskTable": task_table,
        "constants": constants,
        "functions": functions,
    })
}

/// Disassembles an IPLC file at the given path, returning structured JSON.
///
/// On error, returns `{"error": "message"}`.
pub fn disassemble_file(path: &Path) -> Value {
    let file = match File::open(path) {
        Ok(f) => f,
        Err(e) => return json!({"error": format!("Failed to open file: {}", e)}),
    };

    let mut reader = BufReader::new(file);
    let container = match Container::read_from(&mut reader) {
        Ok(c) => c,
        Err(e) => return json!({"error": format!("Failed to parse container: {}", e)}),
    };

    disassemble(&container)
}

/// Converts the file header into a JSON object.
fn disassemble_header(container: &Container) -> Value {
    let h = &container.header;
    let flags = h.flags;

    json!({
        "formatVersion": h.format_version,
        "profile": h.profile,
        "flags": {
            "raw": flags,
            "hasSystemUptime": (flags & FLAG_HAS_SYSTEM_UPTIME) != 0,
            // No flag bit marks a signature section; its directory entry does.
            "hasContentSignature": h.sig_section_offset != 0,
            "hasDebugSection": (flags & FLAG_HAS_DEBUG_SECTION) != 0,
            "hasTypeSection": (flags & FLAG_HAS_TYPE_SECTION) != 0,
        },
        "contentHash": hex_string(&h.content_hash),
        "debugHash": hex_string(&h.debug_hash),
        "layoutHash": hex_string(&h.layout_hash),
        "maxStackDepth": h.max_stack_depth,
        "maxCallDepth": h.max_call_depth,
        "numVariables": h.num_variables,
        "dataRegionBytes": h.data_region_bytes,
        "numTempBufs": h.num_temp_bufs,
        "maxTempBufBytes": h.max_temp_buf_bytes,
        "numFunctions": h.num_functions,
        "numFbTypes": h.num_fb_types,
        "inputImageBytes": h.input_image_bytes,
        "outputImageBytes": h.output_image_bytes,
        "memoryImageBytes": h.memory_image_bytes,
        "sigSection": {
            "offset": h.sig_section_offset,
            "size": h.sig_section_size,
        },
        "debugSigSection": {
            "offset": h.debug_sig_offset,
            "size": h.debug_sig_size,
        },
        "typeSection": {
            "offset": h.type_section_offset,
            "size": h.type_section_size,
        },
        "constSection": {
            "offset": h.const_section_offset,
            "size": h.const_section_size,
        },
        "codeSection": {
            "offset": h.code_section_offset,
            "size": h.code_section_size,
        },
        "debugSection": {
            "offset": h.debug_section_offset,
            "size": h.debug_section_size,
        },
        "taskSection": {
            "offset": h.task_section_offset,
            "size": h.task_section_size,
        },
    })
}

/// Converts the task table into a JSON object with tasks and programs.
fn disassemble_task_table(container: &Container) -> Value {
    let tt = &container.task_table;

    let tasks: Vec<Value> = tt
        .tasks
        .iter()
        .map(|t| {
            json!({
                "taskId": t.task_id.raw(),
                "priority": t.priority,
                "taskType": t.task_type.as_str(),
                "enabled": (t.flags & 0x01) != 0,
                "intervalUs": t.interval_us,
                "singleVarIndex": t.single_var_index.raw(),
                "watchdogUs": t.watchdog_us,
            })
        })
        .collect();

    let programs: Vec<Value> = tt
        .programs
        .iter()
        .map(|p| {
            json!({
                "instanceId": p.instance_id.raw(),
                "taskId": p.task_id.raw(),
                "entryFunctionId": p.entry_function_id.raw(),
                "varTableOffset": p.var_table_offset,
                "varTableCount": p.var_table_count,
                "fbInstanceOffset": p.fb_instance_offset,
                "fbInstanceCount": p.fb_instance_count,
            })
        })
        .collect();

    json!({
        "sharedGlobalsSize": tt.shared_globals_size,
        "tasks": tasks,
        "programs": programs,
    })
}

/// Converts the constant pool into a JSON array of entries.
fn disassemble_constants(container: &Container) -> Value {
    let entries: Vec<Value> = container
        .constant_pool
        .iter()
        .enumerate()
        .map(|(index, entry)| {
            let value_str = format_const_value(entry.const_type, entry.bytes());
            json!({
                "index": index,
                "type": entry.const_type.as_str(),
                "value": value_str,
            })
        })
        .collect();

    Value::Array(entries)
}

/// Formats a constant value as a human-readable string based on its type.
fn format_const_value(const_type: ConstType, bytes: &[u8]) -> String {
    match const_type {
        ConstType::I32 if bytes.len() >= 4 => {
            i32::from_le_bytes([bytes[0], bytes[1], bytes[2], bytes[3]]).to_string()
        }
        ConstType::U32 if bytes.len() >= 4 => {
            u32::from_le_bytes([bytes[0], bytes[1], bytes[2], bytes[3]]).to_string()
        }
        ConstType::I64 if bytes.len() >= 8 => i64::from_le_bytes([
            bytes[0], bytes[1], bytes[2], bytes[3], bytes[4], bytes[5], bytes[6], bytes[7],
        ])
        .to_string(),
        ConstType::U64 if bytes.len() >= 8 => u64::from_le_bytes([
            bytes[0], bytes[1], bytes[2], bytes[3], bytes[4], bytes[5], bytes[6], bytes[7],
        ])
        .to_string(),
        ConstType::F32 if bytes.len() >= 4 => {
            f32::from_le_bytes([bytes[0], bytes[1], bytes[2], bytes[3]]).to_string()
        }
        ConstType::F64 if bytes.len() >= 8 => f64::from_le_bytes([
            bytes[0], bytes[1], bytes[2], bytes[3], bytes[4], bytes[5], bytes[6], bytes[7],
        ])
        .to_string(),
        // Narrow strings are Latin-1: each byte maps directly to a code point.
        ConstType::Str => format!(
            "\"{}\"",
            bytes.iter().map(|&b| b as char).collect::<String>()
        ),
        // Wide strings are UTF-16LE: pair up bytes into code units.
        ConstType::WStr if bytes.len().is_multiple_of(2) => {
            let units: Vec<u16> = bytes
                .as_chunks::<2>()
                .0
                .iter()
                .map(|c| u16::from_le_bytes([c[0], c[1]]))
                .collect();
            format!("\"{}\"", String::from_utf16_lossy(&units))
        }
        _ => format!("<invalid: {} bytes>", bytes.len()),
    }
}

/// Disassembles all functions into a JSON array.
fn disassemble_functions(container: &Container) -> Value {
    let functions: Vec<Value> = container
        .code
        .functions
        .iter()
        .map(|func| {
            let bytecode = container
                .code
                .get_function_bytecode(func.function_id)
                .unwrap_or(&[]);
            let instructions = decode_instructions(bytecode, container);
            json!({
                "id": func.function_id.raw(),
                "bytecodeOffset": func.code_offset,
                "bytecodeLength": func.code_length,
                "maxStackDepth": func.max_stack_depth,
                "numLocals": func.num_locals,
                "instructions": instructions,
            })
        })
        .collect();

    Value::Array(functions)
}

/// Decodes a bytecode slice into an array of instruction JSON objects.
///
/// Every row is rendered from the instruction's own declaration -- the
/// mnemonic and operand layout the instruction set gives it -- so there is no
/// per-opcode code here that can fall out of step with the instruction set.
/// An `UNKNOWN(0x..)` row therefore means what it says: a byte that is not an
/// assigned opcode, from a corrupt container or one written by a newer
/// compiler.
fn decode_instructions(bytecode: &[u8], container: &Container) -> Vec<Value> {
    opcode::decode_body(bytecode)
        .map(|decoded| match decoded {
            Ok(instruction) => decode_instruction(&instruction, container),
            // A byte that is not an assigned opcode. The walk resumes after it.
            Err(DecodeStop::UnknownOpcode { offset, byte }) => instruction_json(
                offset,
                &format!("UNKNOWN(0x{byte:02X})"),
                String::new(),
                String::new(),
            ),
            // A truncated container can end part-way through an instruction.
            // Say so rather than reading past the end of the body.
            Err(DecodeStop::Truncated { offset, opcode }) => instruction_json(
                offset,
                Instruction::decode(opcode)
                    .map(|i| i.mnemonic)
                    .unwrap_or("UNKNOWN"),
                "<truncated>".to_string(),
                String::new(),
            ),
        })
        .collect()
}

/// Decodes one instruction: its mnemonic, its operands in order, and whatever
/// comments those operands contribute (a constant's value, a branch target).
fn decode_instruction(decoded: &DecodedInstruction<'_>, container: &Container) -> Value {
    let mut operands = Vec::new();
    let mut comments = Vec::new();

    if !decoded.instruction.note.is_empty() {
        comments.push(decoded.instruction.note.to_string());
    }

    for (operand, bytes) in decoded.operands_with_kinds() {
        let (text, comment) = format_operand(operand, bytes, decoded.next_offset(), container);
        operands.push(text);
        comments.extend(comment);
    }

    instruction_json(
        decoded.offset,
        decoded.instruction.mnemonic,
        operands.join(", "),
        comments.join(", "),
    )
}

/// Renders one operand from its own bytes, and the comment it contributes.
///
/// The match is exhaustive over [`Operand`] with no catch-all, so an operand
/// shape added to the instruction set does not compile until it is rendered
/// here. `next_pc` is the offset of the following instruction, which is what
/// a branch offset is measured from.
fn format_operand(
    operand: Operand,
    bytes: &[u8],
    next_pc: usize,
    container: &Container,
) -> (String, Option<String>) {
    match operand {
        Operand::ConstIndex => {
            let pool_index = read_u16(bytes, 0);
            (
                format!("pool[{pool_index}]"),
                Some(lookup_const_comment(container, pool_index)),
            )
        }
        Operand::VarIndex => (format!("var[{}]", read_u16(bytes, 0)), None),
        Operand::RefIndex => (format!("ref[{}]", read_u16(bytes, 0)), None),
        Operand::ArrayDescIndex => (format!("desc[{}]", read_u16(bytes, 0)), None),
        Operand::DataOffset => (format!("data[{}]", read_u32(bytes, 0)), None),
        Operand::FieldIndex => (format!("field[{}]", bytes[0]), None),
        Operand::FbTypeId => (format!("type[{}]", read_u16(bytes, 0)), None),
        Operand::FunctionId => {
            let function_id = read_u16(bytes, 0);
            (
                format!("func[{function_id}]"),
                lookup_function_comment(container, function_id),
            )
        }
        Operand::BuiltinId => (format_builtin(read_u16(bytes, 0)), None),
        Operand::JumpOffset => {
            let jump_offset = read_i16(bytes, 0);
            let target = (next_pc as isize + jump_offset as isize) as usize;
            (
                format!("offset: {}", format_jump_offset(jump_offset)),
                Some(format!("-> 0x{target:04X}")),
            )
        }
        Operand::CmpOp => (format_cmp_op(bytes[0]).to_string(), None),
        Operand::MaxLength => (format!("max_len: {}", read_u16(bytes, 0)), None),
        Operand::CharWidth => (format!("char_width: {}", bytes[0]), None),
        Operand::NumFields => (format!("num_fields: {}", bytes[0]), None),
        Operand::FieldVarOffset => (format!("fields[{}]", read_u16(bytes, 0)), None),
        Operand::ParamVarOffset => (format!("params[{}]", read_u16(bytes, 0)), None),
    }
}

/// Builds the JSON object for one decoded instruction.
fn instruction_json(offset: usize, opcode: &str, operands: String, comment: String) -> Value {
    json!({
        "offset": offset,
        "opcode": opcode,
        "operands": operands,
        "comment": comment,
    })
}

/// Names the comparison operator a `CMP_BR_*` instruction branches on.
fn format_cmp_op(cmp_op: u8) -> &'static str {
    match cmp_op {
        opcode::cmp_op::EQ => "EQ",
        opcode::cmp_op::NE => "NE",
        opcode::cmp_op::LT_S => "LT_S",
        opcode::cmp_op::LE_S => "LE_S",
        opcode::cmp_op::GT_S => "GT_S",
        opcode::cmp_op::GE_S => "GE_S",
        _ => "INVALID",
    }
}

/// Names the built-in a `BUILTIN` instruction calls, falling back to its raw
/// ID for a byte no built-in claims.
///
/// The names come from the instruction set's own built-in table rather than a
/// list kept here, so a built-in added there is named here by virtue of
/// existing. MUX is the one built-in whose ID encodes its arity, so it is
/// named from its type and IN count.
fn format_builtin(func_id: u16) -> String {
    if let Some(name) = opcode::builtin::name(func_id) {
        return format!("{name} (0x{func_id:04X})");
    }
    if let (Some(width), Some(n)) = (
        opcode::builtin::mux_type_name(func_id),
        opcode::builtin::mux_info(func_id),
    ) {
        return format!("MUX_{width}({n}) (0x{func_id:04X})");
    }
    format!("0x{func_id:04X}")
}

/// Reads a little-endian u16 from the bytecode at the given position.
fn read_u16(bytecode: &[u8], pos: usize) -> u16 {
    u16::from_le_bytes([bytecode[pos], bytecode[pos + 1]])
}

/// Reads a little-endian i16 from the bytecode at the given position.
fn read_i16(bytecode: &[u8], pos: usize) -> i16 {
    i16::from_le_bytes([bytecode[pos], bytecode[pos + 1]])
}

/// Reads a little-endian u32 from the bytecode at the given position.
fn read_u32(bytecode: &[u8], pos: usize) -> u32 {
    u32::from_le_bytes([
        bytecode[pos],
        bytecode[pos + 1],
        bytecode[pos + 2],
        bytecode[pos + 3],
    ])
}

/// Formats a jump offset as a sign and hexadecimal magnitude, for example
/// `+0x0002` or `-0x0003`.
///
/// The magnitude comes from [`i16::unsigned_abs`] rather than from formatting
/// the `i16` directly: `UpperHex` for a signed integer emits the two's
/// complement bit pattern, which would print `-3` as `-0xFFFD`. `unsigned_abs`
/// is also total, so `i16::MIN` in a corrupt container cannot panic.
fn format_jump_offset(value: i16) -> String {
    let sign = if value < 0 { "-" } else { "+" };
    format!("{}0x{:04X}", sign, value.unsigned_abs())
}

/// Looks up a called function's name in the debug section, if the container
/// carries one, and returns it as a display comment.
fn lookup_function_comment(container: &Container, function_id: u16) -> Option<String> {
    let name = &container
        .debug_section
        .as_ref()?
        .func_names
        .iter()
        .find(|entry| entry.function_id.raw() == function_id)?
        .name;
    Some(format!("= {name}"))
}

/// Looks up a constant pool entry by index and returns a display comment.
fn lookup_const_comment(container: &Container, pool_index: u16) -> String {
    let entry = container.constant_pool.iter().nth(pool_index as usize);
    match entry {
        Some(e) => format!("= {}", format_const_value(e.const_type, e.bytes())),
        None => format!("= <invalid pool index {}>", pool_index),
    }
}

/// Converts a byte slice to a lowercase hex string.
fn hex_string(bytes: &[u8]) -> String {
    bytes.iter().map(|b| format!("{:02x}", b)).collect()
}

#[cfg(test)]
mod tests;
