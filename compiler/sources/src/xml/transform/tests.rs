//! Tests for the PLCopen XML to DSL transform.

use super::*;
use crate::xml::position::parse_plcopen_xml;

mod configuration;
mod data_types;
mod edge_cases;
mod pous;
mod sfc;
mod source_position;

fn test_file_id() -> FileId {
    FileId::from_string("test.xml")
}

fn parse_project(xml: &str) -> Project {
    parse_plcopen_xml(xml, &test_file_id()).unwrap()
}

fn minimal_project_header() -> &'static str {
    r#"<?xml version="1.0" encoding="UTF-8"?>
<project xmlns="http://www.plcopen.org/xml/tc6_0201">
  <fileHeader companyName="Test" productName="Test" productVersion="1.0" creationDateTime="2024-01-01T00:00:00"/>
  <contentHeader name="TestProject">
    <coordinateInfo>
      <fbd><scaling x="1" y="1"/></fbd>
      <ld><scaling x="1" y="1"/></ld>
      <sfc><scaling x="1" y="1"/></sfc>
    </coordinateInfo>
  </contentHeader>"#
}
