//! `ironplcc tokenize` lexes under the dialect and the `--allow-*` flags the
//! user selected, for Structured Text files and for the bodies in PLCopen XML.

use assert_cmd::cargo;
use assert_cmd::prelude::*;
use predicates::prelude::*;
use std::{path::PathBuf, process::Command};
use tempfile::TempDir;

const STRUCTURED_TEXT: &str = "{attribute 'pack_mode' := '1'}
PROGRAM main
VAR
    r : REF_TO INT;
END_VAR
    WHILE TRUE DO
        CONTINUE;
    END_WHILE;
END_PROGRAM
";

const XML_PROJECT: &str = r#"<?xml version="1.0" encoding="UTF-8"?>
<project xmlns="http://www.plcopen.org/xml/tc6_0201">
  <fileHeader companyName="Test" productName="Test" productVersion="1.0" creationDateTime="2024-01-01T00:00:00"/>
  <contentHeader name="TestProject">
    <coordinateInfo>
      <fbd><scaling x="1" y="1"/></fbd>
      <ld><scaling x="1" y="1"/></ld>
      <sfc><scaling x="1" y="1"/></sfc>
    </coordinateInfo>
  </contentHeader>
  <types>
    <dataTypes/>
    <pous>
      <pou name="Main" pouType="program">
        <interface/>
        <body>
          <ST>
            <xhtml xmlns="http://www.w3.org/1999/xhtml">WHILE TRUE DO
  CONTINUE;
END_WHILE;</xhtml>
          </ST>
        </body>
      </pou>
    </pous>
  </types>
</project>
"#;

fn write_file(dir: &TempDir, name: &str, content: &str) -> std::io::Result<PathBuf> {
    let path = dir.path().join(name);
    std::fs::write(&path, content)?;
    Ok(path)
}

fn tokenize(file: &PathBuf, options: &[&str]) -> Command {
    let mut cmd = Command::new(cargo::cargo_bin!("ironplcc"));
    cmd.arg("tokenize").args(options).arg(file);
    cmd
}

#[test]
fn tokenize_when_codesys_dialect_then_extension_words_are_keywords(
) -> Result<(), Box<dyn std::error::Error>> {
    let dir = TempDir::new()?;
    let file = write_file(&dir, "extensions.st", STRUCTURED_TEXT)?;

    tokenize(&file, &["--dialect", "codesys"])
        .assert()
        .success()
        .stdout(predicate::str::contains(
            "Type: Continue, Value: 'CONTINUE'",
        ))
        .stdout(predicate::str::contains("Type: RefTo, Value: 'REF_TO'"))
        .stdout(predicate::str::contains("Type: Pragma, Value: '{attribute"));

    Ok(())
}

#[test]
fn tokenize_when_default_dialect_then_extension_words_are_names_and_the_pragma_is_refused(
) -> Result<(), Box<dyn std::error::Error>> {
    let dir = TempDir::new()?;
    let file = write_file(&dir, "extensions.st", STRUCTURED_TEXT)?;

    tokenize(&file, &[])
        .assert()
        .failure()
        .stdout(predicate::str::contains(
            "Type: Identifier, Value: 'CONTINUE'",
        ))
        .stdout(predicate::str::contains(
            "Type: Identifier, Value: 'REF_TO'",
        ))
        .stdout(predicate::str::contains("Type: Continue").not())
        .stderr(predicate::str::contains("pragmas are not enabled"));

    Ok(())
}

#[test]
fn tokenize_when_allow_flag_then_only_that_word_becomes_a_keyword(
) -> Result<(), Box<dyn std::error::Error>> {
    let dir = TempDir::new()?;
    let file = write_file(&dir, "extensions.st", STRUCTURED_TEXT)?;

    tokenize(&file, &["--allow-continue"])
        .assert()
        .stdout(predicate::str::contains(
            "Type: Continue, Value: 'CONTINUE'",
        ))
        .stdout(predicate::str::contains(
            "Type: Identifier, Value: 'REF_TO'",
        ));

    Ok(())
}

#[test]
fn tokenize_xml_when_codesys_dialect_then_body_words_are_keywords(
) -> Result<(), Box<dyn std::error::Error>> {
    let dir = TempDir::new()?;
    let file = write_file(&dir, "project.xml", XML_PROJECT)?;

    tokenize(&file, &["--dialect", "codesys"])
        .assert()
        .success()
        .stdout(predicate::str::contains(
            "Type: Continue, Value: 'CONTINUE'",
        ));

    Ok(())
}

#[test]
fn tokenize_xml_when_default_dialect_then_body_words_are_names(
) -> Result<(), Box<dyn std::error::Error>> {
    let dir = TempDir::new()?;
    let file = write_file(&dir, "project.xml", XML_PROJECT)?;

    tokenize(&file, &[])
        .assert()
        .stdout(predicate::str::contains(
            "Type: Identifier, Value: 'CONTINUE'",
        ))
        .stdout(predicate::str::contains("Type: Continue").not());

    Ok(())
}
