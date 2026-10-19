//! Tests of the command line arguments of `ironplcc` and of the commands that
//! need no file system.

use super::*;
use clap::ValueEnum;
use ironplc_dsl::stack::spawns_by_current_thread;
use ironplc_test::shared_resource_path;

#[test]
fn clap_dialect_value_variants_when_compared_then_matches_dialect_all() {
    let clap_variants: Vec<Dialect> = ClapDialect::value_variants().iter().map(|c| c.0).collect();
    assert_eq!(clap_variants.as_slice(), Dialect::ALL);
}

/// Guards the hand-maintained `FileArgs` flag list against drifting out of
/// sync with the compiler's `FEATURE_DESCRIPTORS`: every dialect flag must be
/// reachable via its `--allow-*` CLI form and wired through to
/// `CompilerOptions`. clap needs a static field per arg, so the list cannot
/// be derived — but this test makes an omission fail CI instead of shipping.
#[test]
fn file_args_when_each_dialect_flag_cli_form_passed_then_option_enabled() {
    #[derive(Parser)]
    struct TestCli {
        #[command(flatten)]
        file_args: FileArgs,
    }

    for fd in CompilerOptions::FEATURE_DESCRIPTORS {
        let cli = TestCli::try_parse_from(["ironplcc", fd.cli_flag]);
        assert!(
            cli.is_ok(),
            "CLI does not accept `{}` (for CompilerOptions.{}): {:?}",
            fd.cli_flag,
            fd.option_key,
            cli.as_ref().err()
        );
        let cli = cli.unwrap();
        let options = cli.file_args.compiler_options();
        assert_eq!(
            options.get_flag_by_key(fd.option_key),
            Some(true),
            "`{}` parsed but did not enable CompilerOptions.{}",
            fd.cli_flag,
            fd.option_key
        );
    }
}

/// Guards the hand-maintained `FileArgs` policy list the same way: every
/// behavior policy must be settable to each of its alternatives via its
/// `--policy-*` CLI form, and a policy flag replaces the dialect's own
/// selection rather than merely enabling.
#[test]
fn file_args_when_each_policy_alternative_passed_then_option_selected() {
    #[derive(Parser)]
    struct TestCli {
        #[command(flatten)]
        file_args: FileArgs,
    }

    for pd in CompilerOptions::POLICY_DESCRIPTORS {
        for alt in pd.alternatives {
            // `--dialect codesys` selects a non-default alternative for
            // every policy, so the flag is proven to replace it.
            let cli =
                TestCli::try_parse_from(["ironplcc", "--dialect", "codesys", pd.cli_flag, alt]);
            assert!(
                cli.is_ok(),
                "CLI does not accept `{} {alt}` (for CompilerOptions.{}): {:?}",
                pd.cli_flag,
                pd.option_key,
                cli.as_ref().err()
            );
            let cli = cli.unwrap();
            let options = cli.file_args.compiler_options();
            assert_eq!(
                options.get_policy_by_key(pd.option_key),
                Some(*alt),
                "`{} {alt}` parsed but did not select it for CompilerOptions.{}",
                pd.cli_flag,
                pd.option_key
            );
        }
    }
}

/// The flags of the lists of warnings are the ones `WarningList` declares:
/// each is accepted, repeatable, reads its code with `WarningSet::from_code`
/// (the IronPLC and the CODESYS form) and lands in its own list. The
/// behaviour of the flags in the command is `REQ-DS-cli-015`, tested in
/// `tests/cli_message_forms.rs`.
#[test]
fn file_args_when_each_warning_list_flag_passed_then_the_codes_are_in_that_list_only() {
    #[derive(Parser)]
    struct TestCli {
        #[command(flatten)]
        file_args: FileArgs,
    }

    for list in WarningList::ALL {
        let cli = TestCli::try_parse_from(["ironplcc", list.cli_flag(), "P4083"]).unwrap();
        let options = cli.file_args.compiler_options();

        for other in WarningList::ALL {
            assert_eq!(
                options
                    .messages
                    .list(other)
                    .contains(ironplc_problems::WarningProblem::EnumerationTypesCompared),
                other == list,
                "`{}` and list {other:?}",
                list.cli_flag()
            );
        }
        let cli = TestCli::try_parse_from([
            "ironplcc",
            list.cli_flag(),
            "C0354",
            list.cli_flag(),
            "P4083",
        ])
        .unwrap();
        assert!(!cli
            .file_args
            .compiler_options()
            .messages
            .list(list)
            .is_empty());
    }
}

#[test]
fn file_args_when_warning_flag_gets_a_code_that_is_not_accepted_then_a_usage_error_that_names_it() {
    #[derive(Parser)]
    struct TestCli {
        #[command(flatten)]
        file_args: FileArgs,
    }

    for list in WarningList::ALL {
        for code in ["P4007", "p4083", "nonsense", ""] {
            let refused = TestCli::try_parse_from(["ironplcc", list.cli_flag(), code])
                .err()
                .unwrap_or_else(|| panic!("`{} {code}` was accepted", list.cli_flag()))
                .to_string();

            assert!(refused.contains("P0013"), "{refused}");
            assert!(refused.contains(&format!("'{code}'")), "{refused}");
            assert!(refused.contains("P4083, C0354"), "{refused}");
        }
    }
}

#[test]
fn file_args_when_no_warning_flag_then_both_lists_are_empty() {
    #[derive(Parser)]
    struct TestCli {
        #[command(flatten)]
        file_args: FileArgs,
    }

    let options = TestCli::try_parse_from(["ironplcc"])
        .unwrap()
        .file_args
        .compiler_options();

    for list in WarningList::ALL {
        assert!(options.messages.list(list).is_empty());
    }
}

#[test]
fn file_args_when_no_policy_flag_then_dialect_selection_kept() {
    #[derive(Parser)]
    struct TestCli {
        #[command(flatten)]
        file_args: FileArgs,
    }
    let cli = TestCli::try_parse_from(["ironplcc", "--dialect", "twincat"]).unwrap();
    let options = cli.file_args.compiler_options();
    assert_eq!(
        options.policy_string_to_num_non_numeric,
        StringToNumNonNumeric::IgnoreTrailing
    );
    assert_eq!(
        options.policy_string_to_num_failure,
        StringToNumFailure::Zero
    );
    assert_eq!(
        options.policy_enumeration_strictness,
        EnumerationStrictness::WithAttribute
    );
}

#[test]
fn file_args_when_strictness_flag_passed_then_replaces_dialect_selection() {
    #[derive(Parser)]
    struct TestCli {
        #[command(flatten)]
        file_args: FileArgs,
    }
    let cli = TestCli::try_parse_from([
        "ironplcc",
        "--dialect",
        "codesys",
        "--policy-enumeration-strictness",
        "every-enumeration",
    ])
    .unwrap();
    let options = cli.file_args.compiler_options();
    assert_eq!(
        options.policy_enumeration_strictness,
        EnumerationStrictness::EveryEnumeration
    );
    assert!(
        TestCli::try_parse_from(["ironplcc", "--policy-enumeration-strictness", "sometimes"])
            .is_err()
    );
}

#[test]
fn file_args_when_unknown_policy_alternative_then_rejected() {
    #[derive(Parser)]
    struct TestCli {
        #[command(flatten)]
        file_args: FileArgs,
    }
    let result = TestCli::try_parse_from(["ironplcc", "--policy-string-to-num-failure", "wrap"]);
    assert!(result.is_err());
}

/// Guards the hand-maintained VS Code `ironplc.dialect` settings schema
/// against drifting out of sync with `Dialect::ALL`. JSON cannot read the
/// Rust enum, so this test parses `integrations/vscode/package.json` and
/// compares its dialect picker back to the source of truth: the `enum`
/// array must match the `cli_name()` list in order, the three parallel
/// label/description arrays must stay index-aligned (same length), and
/// every dialect must be mentioned in the `markdownDescription`. Adding a
/// `Dialect` variant without updating the extension fails here.
#[test]
fn vscode_dialect_setting_when_compared_then_matches_dialect_all() {
    let package_json_path = concat!(
        env!("CARGO_MANIFEST_DIR"),
        "/../../integrations/vscode/package.json"
    );
    let text = std::fs::read_to_string(package_json_path);
    assert!(text.is_ok(), "failed to read {package_json_path}");
    let text = text.unwrap();
    let parsed = serde_json::from_str::<serde_json::Value>(&text);
    assert!(
        parsed.is_ok(),
        "failed to parse {package_json_path} as JSON"
    );
    let package = parsed.unwrap();

    let dialect = &package["contributes"]["configuration"]["properties"]["ironplc.dialect"];
    assert!(
        dialect.is_object(),
        "ironplc.dialect setting not found in {package_json_path}"
    );

    let expected_names: Vec<&str> = Dialect::ALL.iter().map(|d| d.cli_name()).collect();

    // The `enum` array must list exactly the dialect cli_names, in order.
    let enum_values: Vec<&str> = dialect["enum"]
        .as_array()
        .unwrap()
        .iter()
        .map(|v| v.as_str().unwrap())
        .collect();
    assert_eq!(
        enum_values, expected_names,
        "ironplc.dialect.enum in package.json does not match Dialect::ALL cli_names"
    );

    // The parallel label/description arrays must stay index-aligned with
    // `enum`, so a new dialect cannot leave one array short.
    for key in ["enumItemLabels", "enumDescriptions"] {
        let arr = dialect[key].as_array();
        assert!(arr.is_some(), "ironplc.dialect.{key} must be an array");
        let len = arr.unwrap().len();
        assert_eq!(
            len,
            expected_names.len(),
            "ironplc.dialect.{key} has {len} entries but there are {} dialects",
            expected_names.len()
        );
    }

    // The prose description must mention every dialect by cli_name.
    let markdown = dialect["markdownDescription"].as_str().unwrap();
    for name in &expected_names {
        assert!(
            markdown.contains(name),
            "ironplc.dialect.markdownDescription does not mention `{name}`"
        );
    }
}

/// The threads that running the command makes: the cost of the program's
/// own run, which is the run of one command over its files.
fn spawns_of_command(arguments: &[&str]) -> usize {
    let args = Args::try_parse_from(arguments);
    assert!(args.is_ok(), "the arguments are not a command: {args:?}");
    let Ok(args) = args else {
        return usize::MAX;
    };
    let before = spawns_by_current_thread();
    let result = run(args.action);
    assert_eq!(result, Ok(()), "{arguments:?}");
    spawns_by_current_thread() - before
}

fn corpus_files() -> Vec<String> {
    ["steel_thread.st", "bit_access.st", "literal.st"]
        .iter()
        .map(|name| shared_resource_path(name).display().to_string())
        .collect()
}

#[test]
fn run_when_check_over_three_files_then_one_thread_for_the_run() {
    let files = corpus_files();
    let mut arguments = vec!["ironplcc", "check"];
    arguments.extend(files.iter().map(String::as_str));

    assert_eq!(spawns_of_command(&arguments), 1);
}

#[test]
fn run_when_echo_over_three_files_then_one_thread_for_the_run() {
    let files = corpus_files();
    let mut arguments = vec!["ironplcc", "echo"];
    arguments.extend(files.iter().map(String::as_str));

    assert_eq!(spawns_of_command(&arguments), 1);
}

#[test]
fn run_when_compile_over_three_files_then_one_thread_for_the_run() {
    let files = corpus_files();
    let output = tempfile::NamedTempFile::new().unwrap();
    let output = output.path().display().to_string();
    let mut arguments = vec!["ironplcc", "compile", "--output", output.as_str()];
    arguments.extend(files.iter().map(String::as_str));

    assert_eq!(spawns_of_command(&arguments), 1);
}
