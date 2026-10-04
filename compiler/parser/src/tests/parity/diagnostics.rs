//! Parity of the problem codes: the legacy parser's one diagnostic against the
//! diagnostics of the new parser.

use super::legacy::{rejection, Preset};
use super::Reason;
use ironplc_dsl::core::FileId;
use ironplc_syntax::parse_source_file;

/// A rejected input whose legacy problem code the new parser does not report,
/// with the reason.
pub struct CodeException {
    /// The key of the input, as the comparison names it.
    pub key: &'static str,
    /// The dialect preset, or every preset when `None`.
    pub preset: Option<&'static str>,
    /// The code the legacy parser reports.
    pub legacy_code: &'static str,
    pub reason: Reason,
}

impl CodeException {
    fn covers(&self, key: &str, preset: &str, legacy_code: &str) -> bool {
        self.key == key
            && self.preset.is_none_or(|name| name == preset)
            && self.legacy_code == legacy_code
    }
}

/// Differences in the problem code, against the legacy parser.
pub const CODE_EXCEPTIONS: &[CodeException] = &[];

/// The problem codes of the diagnostics the new parser reports for `source`.
pub fn new_codes(source: &str, preset: &Preset) -> Vec<String> {
    parse_source_file(source, &preset.new)
        .diagnostics(&FileId::default())
        .into_iter()
        .map(|diagnostic| diagnostic.code)
        .collect()
}

/// The outcome of comparing problem codes over inputs.
#[derive(Default)]
pub struct CodeReport {
    /// Inputs both parsers reject.
    pub both_reject: usize,
    /// Those whose legacy code the new parser also reports.
    pub same_code: usize,
    pub unexplained: Vec<String>,
    pub stale: Vec<String>,
}

/// For every input the legacy parser rejects and the new parser rejects too,
/// the legacy code must be among the new codes, or the difference must be an
/// exception that still applies.
pub fn compare_codes(
    inputs: &[(String, String)],
    presets: &[Preset],
    exceptions: &[CodeException],
) -> CodeReport {
    let mut report = CodeReport::default();
    let mut used = vec![false; exceptions.len()];
    for (key, text) in inputs {
        for preset in presets {
            let Some((legacy_code, _, _)) = rejection(text, &preset.legacy) else {
                continue;
            };
            let codes = new_codes(text, preset);
            if codes.is_empty() {
                continue;
            }
            report.both_reject += 1;
            if codes.contains(&legacy_code) {
                report.same_code += 1;
                continue;
            }
            let covering: Vec<usize> = (0..exceptions.len())
                .filter(|index| exceptions[*index].covers(key, &preset.name, &legacy_code))
                .collect();
            for index in &covering {
                used[*index] = true;
            }
            if covering.is_empty() {
                report.unexplained.push(format!(
                    "{key:?} under {}: legacy {legacy_code}, new {codes:?}",
                    preset.name
                ));
            }
        }
    }
    for (index, exception) in exceptions.iter().enumerate() {
        if !used[index] {
            report.stale.push(format!(
                "{:?} ({}): {}",
                exception.key, exception.legacy_code, exception.reason
            ));
        }
    }
    report
}
