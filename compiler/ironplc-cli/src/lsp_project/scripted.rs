//! A project whose analysis reports the messages it is given, for the tests of
//! what the editor does with the messages of a run.

use ironplc_dsl::core::FileId;
use ironplc_dsl::diagnostic::{Area, Diagnostic};
use ironplc_parser::token::Token;

/// The messages are what the analysis produces; `semantic` keeps them by the
/// rule of the area it is asked for, as every `Project` does.
pub(super) struct Scripted(pub Vec<Diagnostic>);

impl ironplc_project::Project for Scripted {
    fn initialize(&mut self, _dir: &std::path::Path) -> Vec<Diagnostic> {
        vec![]
    }

    fn change_text_document(&mut self, _file_id: &FileId, _content: String) {}

    fn tokenize(&self, _file_id: &FileId) -> (Vec<Token>, Vec<Diagnostic>) {
        (vec![], vec![])
    }

    fn semantic(&mut self, area: Area) -> Vec<Diagnostic> {
        area.keep(self.0.clone())
    }

    fn semantic_context(&self) -> Option<&ironplc_analyzer::SemanticContext> {
        None
    }

    fn analyzed_library(&self) -> Option<&ironplc_dsl::common::Library> {
        None
    }

    fn sources(&self) -> Vec<&ironplc_sources::Source> {
        vec![]
    }

    fn sources_mut(&mut self) -> Vec<&mut ironplc_sources::Source> {
        vec![]
    }

    fn find(&self, _file_id: &FileId) -> Option<&ironplc_sources::Source> {
        None
    }
}
