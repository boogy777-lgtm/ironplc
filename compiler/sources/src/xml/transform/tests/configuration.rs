//! Tests for transforming configurations, resources, tasks, program instances and durations.

use super::*;
use ironplc_test::cast;

#[test]
fn transform_when_configuration_then_creates_declaration() {
    let xml = format!(
        r#"{}
  <types>
    <dataTypes/>
    <pous>
      <pou name="Main" pouType="program">
        <interface/>
      </pou>
    </pous>
  </types>
  <instances>
    <configurations>
      <configuration name="Config1">
        <resource name="Resource1">
          <task name="MainTask" priority="1" interval="T#100ms">
            <pouInstance name="MainInstance" typeName="Main"/>
          </task>
        </resource>
      </configuration>
    </configurations>
  </instances>
</project>"#,
        minimal_project_header()
    );

    let project = parse_project(&xml);
    let library =
        transform_project(&project, &test_file_id(), &CompilerOptions::default()).unwrap();

    // Should have program declaration and configuration declaration
    assert_eq!(library.elements.len(), 2);

    let config_decl = cast!(
        &library.elements[1],
        LibraryElementKind::ConfigurationDeclaration
    );
    assert_eq!(config_decl.name.to_string(), "Config1");
    assert_eq!(config_decl.resource_decl.len(), 1);
}

#[test]
fn transform_when_resource_then_creates_declaration() {
    let xml = format!(
        r#"{}
  <types>
    <dataTypes/>
    <pous>
      <pou name="Main" pouType="program">
        <interface/>
      </pou>
    </pous>
  </types>
  <instances>
    <configurations>
      <configuration name="Config1">
        <resource name="CPU1">
          <task name="FastTask" priority="1" interval="T#10ms"/>
          <task name="SlowTask" priority="10" interval="T#1s"/>
        </resource>
      </configuration>
    </configurations>
  </instances>
</project>"#,
        minimal_project_header()
    );

    let project = parse_project(&xml);
    let library =
        transform_project(&project, &test_file_id(), &CompilerOptions::default()).unwrap();

    let config_decl = cast!(
        &library.elements[1],
        LibraryElementKind::ConfigurationDeclaration
    );

    let resource = &config_decl.resource_decl[0];
    assert_eq!(resource.name.to_string(), "CPU1");
    assert_eq!(resource.tasks.len(), 2);
}

#[test]
fn transform_when_task_with_interval_then_parses_duration() {
    let xml = format!(
        r#"{}
  <types>
    <dataTypes/>
    <pous/>
  </types>
  <instances>
    <configurations>
      <configuration name="Config1">
        <resource name="CPU1">
          <task name="PeriodicTask" priority="5" interval="T#100ms"/>
        </resource>
      </configuration>
    </configurations>
  </instances>
</project>"#,
        minimal_project_header()
    );

    let project = parse_project(&xml);
    let library =
        transform_project(&project, &test_file_id(), &CompilerOptions::default()).unwrap();

    let config_decl = cast!(
        &library.elements[0],
        LibraryElementKind::ConfigurationDeclaration
    );

    let task = &config_decl.resource_decl[0].tasks[0];
    assert_eq!(task.name.to_string(), "PeriodicTask");
    assert_eq!(task.priority, 5);
    assert!(task.interval.is_some());

    // 100ms = 100,000 microseconds
    let interval = task.interval.as_ref().unwrap();
    assert_eq!(interval.interval, time::Duration::microseconds(100_000));
}

#[test]
fn transform_when_pou_instance_in_task_then_creates_program_config() {
    let xml = format!(
        r#"{}
  <types>
    <dataTypes/>
    <pous>
      <pou name="Main" pouType="program">
        <interface/>
      </pou>
    </pous>
  </types>
  <instances>
    <configurations>
      <configuration name="Config1">
        <resource name="CPU1">
          <task name="MainTask" priority="1" interval="T#100ms">
            <pouInstance name="MainProgram" typeName="Main"/>
          </task>
        </resource>
      </configuration>
    </configurations>
  </instances>
</project>"#,
        minimal_project_header()
    );

    let project = parse_project(&xml);
    let library =
        transform_project(&project, &test_file_id(), &CompilerOptions::default()).unwrap();

    let config_decl = cast!(
        &library.elements[1],
        LibraryElementKind::ConfigurationDeclaration
    );

    let resource = &config_decl.resource_decl[0];
    assert_eq!(resource.programs.len(), 1);

    let program = &resource.programs[0];
    assert_eq!(program.name.to_string(), "MainProgram");
    assert_eq!(program.type_name.to_string(), "Main");
    assert_eq!(program.task_name.as_ref().unwrap().to_string(), "MainTask");
}

#[test]
fn transform_when_pou_instance_at_resource_level_then_creates_program_config() {
    let xml = format!(
        r#"{}
  <types>
    <dataTypes/>
    <pous>
      <pou name="Main" pouType="program">
        <interface/>
      </pou>
    </pous>
  </types>
  <instances>
    <configurations>
      <configuration name="Config1">
        <resource name="CPU1">
          <pouInstance name="FreeRunning" typeName="Main"/>
        </resource>
      </configuration>
    </configurations>
  </instances>
</project>"#,
        minimal_project_header()
    );

    let project = parse_project(&xml);
    let library =
        transform_project(&project, &test_file_id(), &CompilerOptions::default()).unwrap();

    let config_decl = cast!(
        &library.elements[1],
        LibraryElementKind::ConfigurationDeclaration
    );

    let resource = &config_decl.resource_decl[0];
    assert_eq!(resource.programs.len(), 1);

    let program = &resource.programs[0];
    assert_eq!(program.name.to_string(), "FreeRunning");
    assert!(program.task_name.is_none()); // No task association
}

#[test]
fn parse_duration_when_milliseconds_then_correct() {
    let file_id = test_file_id();
    let duration = parse_duration("T#100ms", &file_id).unwrap();
    assert_eq!(duration.interval, time::Duration::microseconds(100_000));
}

#[test]
fn parse_duration_when_seconds_then_correct() {
    let file_id = test_file_id();
    let duration = parse_duration("T#2s", &file_id).unwrap();
    assert_eq!(duration.interval, time::Duration::seconds(2));
}

#[test]
fn parse_duration_when_minutes_then_correct() {
    let file_id = test_file_id();
    let duration = parse_duration("T#5m", &file_id).unwrap();
    assert_eq!(duration.interval, time::Duration::minutes(5));
}

#[test]
fn parse_duration_when_lowercase_prefix_then_correct() {
    let file_id = test_file_id();
    let duration = parse_duration("t#500ms", &file_id).unwrap();
    assert_eq!(duration.interval, time::Duration::microseconds(500_000));
}

#[test]
fn parse_duration_when_no_prefix_then_correct() {
    let file_id = test_file_id();
    let duration = parse_duration("1s", &file_id).unwrap();
    assert_eq!(duration.interval, time::Duration::seconds(1));
}
