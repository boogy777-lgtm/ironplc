//! End-to-end integration tests for the SIZEOF operator.

use ironplc_parser::options::{CompilerOptions, Dialect};
use rstest::rstest;

fn sizeof_options() -> CompilerOptions {
    CompilerOptions {
        allow_sizeof: true,
        ..CompilerOptions::default()
    }
}

e2e_i32_with!(
    end_to_end_when_sizeof_int_then_returns_2,
    sizeof_options(),
    "
PROGRAM main
  VAR
    x : INT;
    s : DINT;
  END_VAR
  s := SIZEOF(x);
END_PROGRAM
",
    &[(1, 2)],
);

e2e_i32_with!(
    end_to_end_when_sizeof_dint_then_returns_4,
    sizeof_options(),
    "
PROGRAM main
  VAR
    x : DINT;
    s : DINT;
  END_VAR
  s := SIZEOF(x);
END_PROGRAM
",
    &[(1, 4)],
);

e2e_i32_with!(
    end_to_end_when_sizeof_dword_then_returns_4,
    sizeof_options(),
    "
PROGRAM main
  VAR
    y : DWORD;
    s : DINT;
  END_VAR
  s := SIZEOF(y);
END_PROGRAM
",
    &[(1, 4)],
);

e2e_i32_with!(
    end_to_end_when_sizeof_bool_then_returns_1,
    sizeof_options(),
    "
PROGRAM main
  VAR
    b : BOOL;
    s : DINT;
  END_VAR
  s := SIZEOF(b);
END_PROGRAM
",
    &[(1, 1)],
);

e2e_i32_with!(
    end_to_end_when_sizeof_real_then_returns_4,
    sizeof_options(),
    "
PROGRAM main
  VAR
    r : REAL;
    s : DINT;
  END_VAR
  s := SIZEOF(r);
END_PROGRAM
",
    &[(1, 4)],
);

e2e_i32_with!(
    end_to_end_when_sizeof_lreal_then_returns_8,
    sizeof_options(),
    "
PROGRAM main
  VAR
    r : LREAL;
    s : DINT;
  END_VAR
  s := SIZEOF(r);
END_PROGRAM
",
    &[(1, 8)],
);

// 10 elements × 2 bytes each = 20
e2e_i32_with!(
    end_to_end_when_sizeof_array_of_int_then_returns_total_bytes,
    sizeof_options(),
    "
PROGRAM main
  VAR
    arr : ARRAY[1..10] OF INT;
    s : DINT;
  END_VAR
  s := SIZEOF(arr);
END_PROGRAM
",
    &[(1, 20)],
);

// The structure of the third example of the CODESYS help for `pack_mode`, in
// each mode. A structure is laid out as the byte image CODESYS describes, not
// as the 8-byte slots the virtual machine stores it in.
#[rstest]
#[case::without_attribute_natural_alignment("", 28)]
#[case::mode_0_same_as_mode_1("{attribute 'pack_mode' := '0'}", 19)]
#[case::mode_1_no_padding("{attribute 'pack_mode' := '1'}", 19)]
#[case::mode_2_aligned_to_2("{attribute 'pack_mode' := '2'}", 20)]
#[case::mode_4_aligned_to_4("{attribute 'pack_mode' := '4'}", 24)]
#[case::mode_8_byte_image_ends_with_last_member("{attribute 'pack_mode' := '8'}", 28)]
fn end_to_end_when_sizeof_structure_with_pack_mode_then_size_of_the_byte_image(
    #[case] pragma: &str,
    #[case] size: i32,
) {
    let source = format!(
        "{pragma}
TYPE Example : STRUCT
    a : BYTE; b : LWORD; c : BYTE; d : BYTE; e : DWORD; f : BYTE; g : BYTE; h : WORD;
END_STRUCT END_TYPE

PROGRAM main
  VAR
    x : Example;
    s : DINT;
  END_VAR
  s := SIZEOF(x);
END_PROGRAM
"
    );

    crate::common::assert_run_i32_with(&source, &codesys_options(), &[(1, size)]);
}

fn codesys_options() -> CompilerOptions {
    CompilerOptions::from_dialect(Dialect::Codesys)
}

// An element of an array occupies its stride: the byte image of the structure
// (LWORD and BYTE: 9 bytes) padded up to its alignment (16 bytes).
e2e_i32_with!(
    end_to_end_when_sizeof_array_of_structure_then_each_element_occupies_its_stride,
    codesys_options(),
    "
TYPE Element : STRUCT a : LWORD; b : BYTE; END_STRUCT END_TYPE

PROGRAM main
  VAR
    arr : ARRAY[1..3] OF Element;
    s : DINT;
  END_VAR
  s := SIZEOF(arr);
END_PROGRAM
",
    &[(1, 48)],
);

e2e_i32_with!(
    end_to_end_when_sizeof_array_of_packed_structure_then_elements_follow_each_other,
    codesys_options(),
    "
{attribute 'pack_mode' := '1'}
TYPE Element : STRUCT a : LWORD; b : BYTE; END_STRUCT END_TYPE

PROGRAM main
  VAR
    arr : ARRAY[1..3] OF Element;
    s : DINT;
  END_VAR
  s := SIZEOF(arr);
END_PROGRAM
",
    &[(1, 27)],
);

// A structure that nests in another occupies its stride: Inner is 9 bytes,
// 8-aligned, so it occupies 16 and the BYTE after it stands at 8 + 16.
e2e_i32_with!(
    end_to_end_when_sizeof_structure_with_nested_structure_then_nested_occupies_its_stride,
    codesys_options(),
    "
TYPE Inner : STRUCT a : BYTE; b : LWORD; END_STRUCT END_TYPE
TYPE Outer : STRUCT x : BYTE; inner : Inner; y : BYTE; END_STRUCT END_TYPE

PROGRAM main
  VAR
    o : Outer;
    s : DINT;
  END_VAR
  s := SIZEOF(o);
END_PROGRAM
",
    &[(1, 25)],
);
