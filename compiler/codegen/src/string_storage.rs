//! Where the characters of a STRING/WSTRING variable live, and how bytecode
//! reaches them.
//!
//! Every string occupies a run of the data region (a header and its
//! characters). What differs between variables is who owns the run, and so
//! how an instruction names it:
//!
//! * A program variable, a global, a function's local or its return value has
//!   one run for the whole program. Its instructions name the run by a fixed
//!   byte offset ([`StringPlace::Static`]).
//! * A variable of a function block belongs to an instance, and two instances
//!   of one block hold two runs. The body is compiled once, so it cannot name
//!   either: it reads the offset of the instance's run from the variable's own
//!   slot, which the instance's setup wrote ([`StringPlace::InstanceOwned`]).
//!   The slot is copied in with the instance's other fields on every call, so
//!   the same bytecode serves every instance.
//!
//! Both are one registration ([`register_string_variable`]) and one set of
//! access routines on [`StringVarInfo`]; the callers differ only in the
//! [`StringHome`] they ask for.

use ironplc_container::{CharWidth, ContainerBuilder, FieldType, VarIndex};
use ironplc_dsl::common::StringInitializer;
use ironplc_dsl::core::{Id, Located};
use ironplc_dsl::diagnostic::Diagnostic;

use super::compile::{
    char_width_for_string_type, string_region_size, CompileContext, StringVarInfo,
};
use super::compile_stmt::resolve_string_max_length;
use crate::emit::Emitter;

/// How the bytecode names the run of the data region a string occupies.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub(crate) enum StringPlace {
    /// The run is at a fixed byte offset.
    Static { data_offset: u32 },
    /// The variable's slot holds the byte offset of the run, and the run is
    /// read as the only element of the string array `desc_index` describes.
    InstanceOwned {
        var_index: VarIndex,
        desc_index: u16,
    },
}

/// Who owns the run a string variable is registered with.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub(crate) enum StringHome {
    /// One run for the whole program, reserved now.
    Static,
    /// One run per function block instance, laid out with the instance (see
    /// `compile_fb_layout`); the body finds it through the slot `var_index`.
    InstanceOwned { var_index: VarIndex },
}

impl StringVarInfo {
    /// A string at a fixed offset of the data region.
    pub(crate) fn fixed(data_offset: u32, max_length: u16, char_width: CharWidth) -> Self {
        StringVarInfo {
            place: StringPlace::Static { data_offset },
            max_length,
            char_width,
        }
    }

    /// The fixed offset of the string's run, for the instructions that can
    /// only name one. `None` when the run belongs to an instance.
    pub(crate) fn static_offset(&self) -> Option<u32> {
        match self.place {
            StringPlace::Static { data_offset } => Some(data_offset),
            StringPlace::InstanceOwned { .. } => None,
        }
    }

    /// Emits the write of the header: capacity, encoding and length zero.
    pub(crate) fn emit_init(&self, emitter: &mut Emitter) {
        match self.place {
            StringPlace::Static { data_offset } => {
                emitter.emit_str_init(data_offset, self.max_length, self.char_width)
            }
            StringPlace::InstanceOwned {
                var_index,
                desc_index,
            } => emitter.emit_str_init_array(var_index, desc_index),
        }
    }

    /// Emits the read of the string into a temporary buffer, pushing its
    /// index.
    pub(crate) fn emit_load(&self, emitter: &mut Emitter, ctx: &mut CompileContext) {
        match self.place {
            StringPlace::Static { data_offset } => emitter.emit_str_load_var(data_offset),
            StringPlace::InstanceOwned {
                var_index,
                desc_index,
            } => {
                emit_only_element(emitter, ctx);
                emitter.emit_str_load_array_elem(var_index, desc_index);
            }
        }
    }

    /// Emits the write of the temporary buffer on the stack into the string.
    pub(crate) fn emit_store(&self, emitter: &mut Emitter, ctx: &mut CompileContext) {
        match self.place {
            StringPlace::Static { data_offset } => emitter.emit_str_store_var(data_offset),
            StringPlace::InstanceOwned {
                var_index,
                desc_index,
            } => {
                emit_only_element(emitter, ctx);
                emitter.emit_str_store_array_elem(var_index, desc_index);
            }
        }
    }
}

/// Pushes the index of the only element of the one-element array an
/// instance-owned string is addressed as.
fn emit_only_element(emitter: &mut Emitter, ctx: &mut CompileContext) {
    let index = ctx.add_i32_constant(0);
    emitter.emit_load_const_i32(index);
}

/// Registers a STRING/WSTRING variable and returns how it is reached.
///
/// This is the one place a string declaration becomes storage, whichever unit
/// declares it: a [`StringHome::Static`] string reserves its run now, a
/// [`StringHome::InstanceOwned`] one reserves nothing -- its run is part of
/// the instance, sized by the instance layout from the same declaration -- and
/// instead registers the descriptor its accesses go through.
pub(crate) fn register_string_variable(
    ctx: &mut CompileContext,
    builder: &mut ContainerBuilder,
    id: &Id,
    string_init: &StringInitializer,
    home: StringHome,
) -> Result<StringVarInfo, Diagnostic> {
    let max_length = resolve_string_max_length(string_init)?;
    let char_width = char_width_for_string_type(&string_init.width);

    let place = match home {
        StringHome::Static => {
            let total_bytes = string_region_size(max_length, char_width);
            let data_offset = crate::data_region::reserve(ctx, total_bytes, &string_init.span())?;
            StringPlace::Static { data_offset }
        }
        StringHome::InstanceOwned { var_index } => {
            let element_type = if char_width.is_wide() {
                FieldType::WString
            } else {
                FieldType::String
            };
            let desc_index = builder.add_array_descriptor(element_type as u8, 1, max_length);
            StringPlace::InstanceOwned {
                var_index,
                desc_index,
            }
        }
    };

    // Temporary buffers are sized for the widest string the program holds.
    if max_length > ctx.max_string_capacity {
        ctx.max_string_capacity = max_length;
    }
    if char_width.is_wide() {
        ctx.has_wide_string = true;
    }

    let info = StringVarInfo {
        place,
        max_length,
        char_width,
    };
    ctx.string_vars.insert(id.clone(), info.clone());
    Ok(info)
}

#[cfg(test)]
mod tests {
    use ironplc_container::{CharWidth, VarIndex};

    use super::*;

    #[test]
    fn static_offset_when_fixed_then_offset() {
        let info = StringVarInfo::fixed(40, 10, CharWidth::Narrow);

        assert_eq!(Some(40), info.static_offset());
    }

    #[test]
    fn static_offset_when_instance_owned_then_none() {
        let info = StringVarInfo {
            place: StringPlace::InstanceOwned {
                var_index: VarIndex::new(1),
                desc_index: 0,
            },
            max_length: 10,
            char_width: CharWidth::Narrow,
        };

        assert_eq!(None, info.static_offset());
    }
}
