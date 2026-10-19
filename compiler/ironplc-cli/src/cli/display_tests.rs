//! How the command line shows a message to a person: when it draws colors and
//! how it writes the path of a file.

use codespan_reporting::term::termcolor::{ColorChoice, NoColor};
use ironplc_dsl::{
    core::{FileId, SourceSpan},
    diagnostic::Diagnostic,
};
use ironplc_problems::text;

use super::{color_choice, display_path, write_for_person};

#[test]
fn color_choice_when_stderr_is_a_terminal_then_always() {
    assert_eq!(color_choice(true, false), ColorChoice::Always);
}

#[test]
fn color_choice_when_stderr_is_not_a_terminal_then_never() {
    assert_eq!(color_choice(false, false), ColorChoice::Never);
}

#[test]
fn color_choice_when_no_color_is_set_then_never() {
    assert_eq!(color_choice(true, true), ColorChoice::Never);
    assert_eq!(color_choice(false, true), ColorChoice::Never);
}

#[test]
fn display_path_when_verbatim_disk_path_then_prefix_removed() {
    assert_eq!(display_path(r"\\?\C:\proj\areas.st"), r"C:\proj\areas.st");
}

#[test]
fn display_path_when_verbatim_disk_root_then_prefix_removed() {
    assert_eq!(display_path(r"\\?\C:"), "C:");
    assert_eq!(display_path(r"\\?\D:\"), r"D:\");
}

#[test]
fn display_path_when_verbatim_unc_path_then_double_backslash_server() {
    assert_eq!(
        display_path(r"\\?\UNC\server\share\areas.st"),
        r"\\server\share\areas.st"
    );
}

#[test]
fn display_path_when_verbatim_path_of_another_kind_then_unchanged() {
    assert_eq!(
        display_path(r"\\?\Volume{0123}\areas.st"),
        r"\\?\Volume{0123}\areas.st"
    );
    assert_eq!(display_path(r"\\?\C:x"), r"\\?\C:x");
}

#[test]
fn display_path_when_normal_windows_path_then_unchanged() {
    assert_eq!(display_path(r"C:\proj\areas.st"), r"C:\proj\areas.st");
    assert_eq!(display_path(r"\\server\share\a.st"), r"\\server\share\a.st");
}

#[test]
fn display_path_when_non_windows_path_then_unchanged() {
    assert_eq!(display_path("/home/user/areas.st"), "/home/user/areas.st");
    assert_eq!(display_path("areas.st"), "areas.st");
    assert_eq!(display_path(""), "");
}

#[test]
fn write_for_person_when_file_id_has_verbatim_prefix_then_path_shown_without_it() {
    let diagnostic = Diagnostic::text(
        SourceSpan {
            start: 5,
            end: 6,
            file_id: FileId::from_string(r"\\?\C:\proj\areas.st"),
        },
        text::variable_undefined_undefined_variable(),
    );
    let mut out = NoColor::new(Vec::new());

    write_for_person(&mut out, &[diagnostic], None);

    let output = String::from_utf8(out.into_inner()).unwrap();
    assert!(output.contains(r"C:\proj\areas.st"), "{output}");
    assert!(!output.contains(r"\\?\"), "{output}");
}
