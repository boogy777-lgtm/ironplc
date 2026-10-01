//! The legacy parser as a test oracle, and the options it runs under.

use ironplc_dsl::core::FileId;
use ironplc_parser::options::{CompilerOptions, Dialect};
use ironplc_parser::parse_st_statements;
use ironplc_syntax::ParseOptions;

/// One dialect configuration, as the legacy options and as the new ones.
pub struct Preset {
    pub name: String,
    pub legacy: CompilerOptions,
    pub new: ParseOptions,
}

/// The legacy options converted flag by flag, by name. A flag the new
/// parser reads must exist in the legacy options: a renamed or removed
/// legacy flag fails here instead of silently reading as off.
pub fn convert(legacy: &CompilerOptions) -> ParseOptions {
    let mut options = ParseOptions::default();
    for key in ParseOptions::FLAG_KEYS {
        let value = legacy.get_flag_by_key(key);
        assert!(value.is_some(), "legacy options have no flag {key}");
        assert!(options.set_flag_by_key(key, value.unwrap_or(false)));
    }
    options
}

/// Every dialect preset, plus all legacy flags on.
pub fn presets() -> Vec<Preset> {
    let mut presets: Vec<Preset> = Dialect::ALL
        .iter()
        .map(|dialect| {
            let legacy = CompilerOptions::from_dialect(*dialect);
            Preset {
                name: dialect.cli_name().to_string(),
                new: convert(&legacy),
                legacy,
            }
        })
        .collect();
    let mut every_flag = CompilerOptions::default();
    for descriptor in CompilerOptions::FEATURE_DESCRIPTORS {
        every_flag.set_flag_by_key(descriptor.option_key, true);
    }
    presets.push(Preset {
        name: "all-flags".to_string(),
        new: convert(&every_flag),
        legacy: every_flag,
    });
    presets
}

/// True when the legacy fragment parser accepts `source` as a statement
/// list.
pub fn accepts(source: &str, options: &CompilerOptions) -> bool {
    parse_st_statements(source, &FileId::default(), options, 0, 0).is_ok()
}

/// True when the legacy parser accepts `body` as the body of a `PROGRAM`.
pub fn accepts_in_program(body: &str, options: &CompilerOptions) -> bool {
    let source = format!("PROGRAM p\n{body}\nEND_PROGRAM\n");
    ironplc_parser::parse_program(&source, &FileId::default(), options).is_ok()
}

/// True when the legacy parser accepts `source` as a whole file.
pub fn accepts_file(source: &str, options: &CompilerOptions) -> bool {
    ironplc_parser::parse_program(source, &FileId::default(), options).is_ok()
}
