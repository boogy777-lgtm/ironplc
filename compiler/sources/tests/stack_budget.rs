//! The stack budget of the document and the library loads: a run that reads
//! many bodies, or several library files, costs one thread for the run when the
//! caller holds the budget (a process entry does), and one for each stage call
//! when it does not (a caller that is not a process entry, such as a test).

use ironplc_dsl::core::FileId;
use ironplc_dsl::stack::{spawns_by_current_thread, within_stack_budget};
use ironplc_parser::options::CompilerOptions;
use ironplc_sources::{parse_source, FileType, LibraryName, SourceProject};

/// How many threads `run` makes on behalf of the calling thread.
fn spawned_by(run: impl FnOnce()) -> usize {
    let before = spawns_by_current_thread();
    run();
    spawns_by_current_thread() - before
}

const BODIES: usize = 20;

/// A PLCopen XML document of `BODIES` programs, each with a statement body.
fn document() -> String {
    let mut pous = String::new();
    for index in 0..BODIES {
        pous.push_str(&format!(
            r#"<pou name="P{index}" pouType="program">
  <interface><localVars><variable name="x"><type><INT/></type></variable></localVars></interface>
  <body><ST><xhtml xmlns="http://www.w3.org/1999/xhtml">IF x &gt; {index} THEN x := {index}; END_IF;</xhtml></ST></body>
</pou>
"#
        ));
    }
    format!(
        r#"<?xml version="1.0" encoding="UTF-8"?>
<project xmlns="http://www.plcopen.org/xml/tc6_0201">
  <fileHeader companyName="Test" productName="Test" productVersion="1.0" creationDateTime="2024-01-01T00:00:00"/>
  <contentHeader name="TestProject">
    <coordinateInfo><fbd><scaling x="1" y="1"/></fbd><ld><scaling x="1" y="1"/></ld><sfc><scaling x="1" y="1"/></sfc></coordinateInfo>
  </contentHeader>
  <types><dataTypes/><pous>{pous}</pous></types>
</project>"#
    )
}

fn read_document() {
    let library = parse_source(
        FileType::Xml,
        &document(),
        &FileId::from_string("many.xml"),
        &CompilerOptions::default(),
    );
    let library = library.expect("the document parses");
    assert_eq!(library.elements.len(), BODIES);
}

#[test]
fn parse_source_when_xml_document_has_many_bodies_and_run_has_the_budget_then_one_thread_for_the_run(
) {
    assert_eq!(spawned_by(|| within_stack_budget(read_document)), 1);
}

#[test]
fn parse_source_when_xml_document_has_many_bodies_and_caller_has_no_budget_then_each_body_gets_it_from_its_stage_entry(
) {
    assert_eq!(spawned_by(read_document), BODIES);
}

fn load_every_bundled_library() {
    let mut project = SourceProject::new();
    project.set_activated_libraries(
        ["Tc2_BuiltIns", "Tc2_Math", "Tc2_System", "Tc2_Utilities"]
            .map(LibraryName::from)
            .to_vec(),
    );
    let (libraries, diagnostics) = project.load_activated_libraries();
    assert_eq!((libraries.len(), diagnostics.len()), (4, 0));
}

#[test]
fn load_activated_libraries_when_run_has_the_budget_then_one_thread_for_the_run() {
    assert_eq!(
        spawned_by(|| within_stack_budget(load_every_bundled_library)),
        1
    );
}

#[test]
fn load_activated_libraries_when_caller_has_no_budget_then_each_file_gets_it_from_its_stage_entry()
{
    assert_eq!(spawned_by(load_every_bundled_library), 4);
}
