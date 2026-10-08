//! The layout hash: the identity of the state layout of a container.
//!
//! Two containers with the same layout hash lay out their persistent state
//! the same way, so the running state of one is the state of the other and a
//! change of code alone can be swapped in at a scan boundary. The layout is
//! the persistent part of the container ([ADR-0073](../../../specs/adrs/0073-state-layout-is-the-persistent-part.md)):
//! the persistent extent ([`Container::persistent_extents`]), the variables in
//! it, the array descriptors those variables name, and the field layout of the
//! function blocks. The working slots of functions and function block bodies,
//! the user function block descriptors (function IDs and the position of the
//! working slots), the stable IDs, the code, the constants and the debug
//! information are not part of it.

use std::vec::Vec;

use blake3::Hasher;

use crate::type_section::{ArrayDescriptor, TypeSection, VarEntry, VAR_FLAG_IS_ARRAY};
use crate::Container;

impl Container {
    /// Computes the layout hash for online change: BLAKE3 over the persistent
    /// extent, in this order:
    ///
    /// 1. the number of extent rows, then for each row its `var_start`,
    ///    `var_count`, `data_start` and `data_len` (little-endian), followed by
    ///    the variable table entries in the row's run, in table order: the type
    ///    tag and the flags, then, for an array variable, the array descriptor
    ///    the entry names (element type, total elements, element extra,
    ///    element stride; never the descriptor's index), and for any other
    ///    variable the entry's `extra`;
    /// 2. the function block type table, in ascending type ID order: the
    ///    number of descriptors, then for each the type ID, the number of
    ///    fields, and the type tag and extra of each field.
    ///
    /// A logic-only edit yields the same hash, and so does an edit that only
    /// adds or removes working slots (the first call of a user function), so
    /// it can be swapped in without restarting. Specified in "Layout Hash and
    /// Online Change" (`specs/design/bytecode-container-format.md`).
    ///
    /// [`write_to`](Self::write_to) stores this value in
    /// `header.layout_hash`, along with `content_hash` and `debug_hash`;
    /// the in-memory header keeps zeros until serialized (ADR-0052's hash
    /// contract).
    pub fn compute_layout_hash(&self) -> [u8; 32] {
        let mut hasher = Hasher::new();

        let empty_type_section = TypeSection::default();
        let type_section = self.type_section.as_ref().unwrap_or(&empty_type_section);

        let extents = self.persistent_extents();
        hasher.update(&(extents.len() as u16).to_le_bytes());
        for row in &extents {
            hasher.update(&row.var_start.to_le_bytes());
            hasher.update(&row.var_count.to_le_bytes());
            hasher.update(&row.data_start.to_le_bytes());
            hasher.update(&row.data_len.to_le_bytes());
            let entries = type_section.variable_table.get(row.vars()).unwrap_or(&[]);
            for entry in entries {
                hash_variable(&mut hasher, entry, type_section);
            }
        }

        let mut fb_types: Vec<_> = type_section.fb_types.iter().collect();
        fb_types.sort_by_key(|descriptor| descriptor.type_id.raw());
        hasher.update(&(fb_types.len() as u16).to_le_bytes());
        for descriptor in fb_types {
            hasher.update(&descriptor.type_id.to_le_bytes());
            hasher.update(&(descriptor.fields.len() as u16).to_le_bytes());
            for field in &descriptor.fields {
                hasher.update(&[field.field_type as u8]);
                hasher.update(&field.field_extra.to_le_bytes());
            }
        }

        *hasher.finalize().as_bytes()
    }
}

/// Hashes one persistent variable: what it is, and for an array what the
/// array is. A descriptor index that names no descriptor hashes as absent; the
/// load check reports it.
fn hash_variable(hasher: &mut Hasher, entry: &VarEntry, type_section: &TypeSection) {
    hasher.update(&[entry.var_type as u8, entry.flags]);
    if entry.flags & VAR_FLAG_IS_ARRAY == 0 {
        hasher.update(&entry.extra.to_le_bytes());
        return;
    }
    match type_section.array_descriptors.get(usize::from(entry.extra)) {
        Some(descriptor) => hash_array(hasher, descriptor),
        None => {
            hasher.update(&[0]);
        }
    }
}

fn hash_array(hasher: &mut Hasher, descriptor: &ArrayDescriptor) {
    hasher.update(&[1, descriptor.element_type]);
    hasher.update(&descriptor.total_elements.to_le_bytes());
    hasher.update(&descriptor.element_extra.to_le_bytes());
    hasher.update(&descriptor.element_stride.to_le_bytes());
}

#[cfg(test)]
mod tests {
    use std::vec;

    use super::*;
    use crate::id_types::{FbTypeId, FunctionId, InstanceId};
    use crate::persistent_extent::PersistentExtent;
    use crate::type_section::{FbTypeDescriptor, FieldEntry, FieldType, UserFbDescriptor};
    use crate::ContainerBuilder;

    fn variable(var_type: FieldType, flags: u8, extra: u16) -> VarEntry {
        VarEntry {
            var_type,
            flags,
            extra,
        }
    }

    fn field(field_type: FieldType) -> FieldEntry {
        FieldEntry {
            field_type,
            field_extra: 0,
        }
    }

    /// Four variables: a scalar, a string and an array (persistent), then one
    /// working slot of a function. Two array descriptors, the second named by
    /// the array variable. One function block type of two fields.
    fn container() -> Container {
        let mut builder = ContainerBuilder::new();
        builder.add_array_descriptor(FieldType::F64 as u8, 9, 0);
        builder.add_array_descriptor(FieldType::I32 as u8, 4, 0);
        builder
            .num_variables(4)
            .data_region_bytes(200)
            .add_var_entry(variable(FieldType::I32, 0, 0))
            .add_var_entry(variable(FieldType::String, 0, 10))
            .add_var_entry(variable(FieldType::I32, VAR_FLAG_IS_ARRAY, 1))
            .add_var_entry(variable(FieldType::F32, 0, 0))
            .add_fb_type(FbTypeDescriptor {
                type_id: FbTypeId::new(0x1000),
                fields: vec![field(FieldType::I32), field(FieldType::Time)],
            })
            .add_fb_type(FbTypeDescriptor {
                type_id: FbTypeId::new(0x1001),
                fields: vec![field(FieldType::F32)],
            })
            .add_user_fb_type(UserFbDescriptor {
                type_id: FbTypeId::new(0x1000),
                function_id: FunctionId::new(2),
                var_offset: 3,
                num_fields: 2,
            })
            .add_persistent_extent(PersistentExtent {
                instance_id: InstanceId::DEFAULT,
                var_start: 0,
                var_count: 3,
                data_start: 0,
                data_len: 100,
            })
            .add_function(FunctionId::INIT, &[0x8C], 0, 4, 0)
            .build()
    }

    fn hash_of(edit: impl FnOnce(&mut Container)) -> [u8; 32] {
        let mut container = container();
        edit(&mut container);
        container.compute_layout_hash()
    }

    fn section(container: &mut Container) -> &mut TypeSection {
        container.type_section.as_mut().unwrap()
    }

    #[test]
    fn compute_layout_hash_when_working_slots_added_or_changed_then_equal() {
        let before = hash_of(|_| {});

        let grown = hash_of(|c| {
            section(c)
                .variable_table
                .push(variable(FieldType::U64, 0, 0));
            c.header.num_variables = 5;
            c.header.data_region_bytes = 400;
        });
        let retyped = hash_of(|c| section(c).variable_table[3] = variable(FieldType::String, 0, 8));

        assert_eq!(before, grown);
        assert_eq!(before, retyped);
    }

    #[test]
    fn compute_layout_hash_when_persistent_variable_changes_then_differs() {
        let before = hash_of(|_| {});

        let retyped = hash_of(|c| section(c).variable_table[0].var_type = FieldType::F32);
        let longer_string = hash_of(|c| section(c).variable_table[1].extra = 11);

        assert_ne!(before, retyped);
        assert_ne!(before, longer_string);
    }

    #[test]
    fn compute_layout_hash_when_extent_changes_then_differs() {
        let before = hash_of(|_| {});

        let more_variables = hash_of(|c| section(c).persistent_extents[0].var_count = 4);
        let more_data = hash_of(|c| section(c).persistent_extents[0].data_len = 104);

        assert_ne!(before, more_variables);
        assert_ne!(before, more_data);
    }

    #[test]
    fn compute_layout_hash_when_array_named_by_a_persistent_variable_changes_then_differs() {
        let before = hash_of(|_| {});

        let resized = hash_of(|c| section(c).array_descriptors[1].total_elements = 5);
        let strided = hash_of(|c| section(c).array_descriptors[1].element_stride = 16);

        assert_ne!(before, resized);
        assert_ne!(before, strided);
    }

    #[test]
    fn compute_layout_hash_when_array_no_persistent_variable_names_changes_then_equal() {
        let before = hash_of(|_| {});

        let unnamed = hash_of(|c| section(c).array_descriptors[0].total_elements = 10);

        assert_eq!(before, unnamed);
    }

    #[test]
    fn compute_layout_hash_when_same_array_sits_at_another_descriptor_index_then_equal() {
        let before = hash_of(|_| {});

        let moved = hash_of(|c| {
            let section = section(c);
            section.array_descriptors.swap(0, 1);
            section.variable_table[2].extra = 0;
        });

        assert_eq!(before, moved);
    }

    #[test]
    fn compute_layout_hash_when_function_block_fields_change_then_differs() {
        let before = hash_of(|_| {});

        let retyped = hash_of(|c| section(c).fb_types[0].fields[1] = field(FieldType::I64));
        let added = hash_of(|c| section(c).fb_types[1].fields.push(field(FieldType::F32)));

        assert_ne!(before, retyped);
        assert_ne!(before, added);
    }

    #[test]
    fn compute_layout_hash_when_function_block_types_listed_in_another_order_then_equal() {
        let before = hash_of(|_| {});

        let reordered = hash_of(|c| section(c).fb_types.reverse());

        assert_eq!(before, reordered);
    }

    #[test]
    fn compute_layout_hash_when_body_addresses_of_a_function_block_move_then_equal() {
        let before = hash_of(|_| {});

        let moved = hash_of(|c| {
            let descriptor = &mut section(c).user_fb_types[0];
            descriptor.var_offset = 9;
            descriptor.function_id = FunctionId::new(5);
        });

        assert_eq!(before, moved);
    }
}
