//! Salsa adapter validation for S0 (design section 3.3).
//!
//! One tracked input (the source text), one tracked query over it (a
//! declaration summary — the "tiny semantic input" the experiment calls for),
//! and one consumer query. The database records Salsa's own `WillExecute` /
//! `DidValidateMemoizedValue` events so tests can observe memoization,
//! recomputation on change, and backdating of equal summaries without
//! instrumenting the queries themselves.
//!
//! This is deliberately the smallest shape that exercises the mechanism the
//! design needs: text in, meaning-bearing summary out, consumers that stop
//! propagating when the summary is unchanged.

use crate::lexer::{lex, Token, TokenKind};
use std::sync::{Arc, Mutex};

/// Salsa database trait for the spike.
#[salsa::db]
pub trait Db: salsa::Database {}

#[salsa::db]
impl<T: salsa::Database> Db for T {}

/// The tracked input: one source file's text.
#[salsa::input(debug)]
pub struct SourceFile {
    #[returns(deref)]
    pub text: String,
}

/// One declared variable: name and written type name.
#[derive(Debug, Clone, PartialEq, Eq, Hash, salsa::SalsaValue)]
pub struct VariableDecl {
    pub name: String,
    pub type_name: String,
}

/// The tracked semantic summary of a file (declaration level only).
#[derive(Debug, Default, Clone, PartialEq, Eq, Hash, salsa::SalsaValue)]
pub struct DeclarationSummary {
    pub pou_kind: Option<String>,
    pub pou_name: Option<String>,
    pub variables: Vec<VariableDecl>,
}

/// Tracked query: lex the file and summarize its POU declaration.
///
/// `returns(clone)` hands callers an owned value so a snapshot can outlive the
/// database borrow; equality-based backdating is unaffected.
#[salsa::tracked(returns(clone))]
pub fn declaration_summary(db: &dyn Db, file: SourceFile) -> DeclarationSummary {
    summarize(file.text(db))
}

/// Consumer query over the summary. Backdating means it is not re-executed
/// when the summary is recomputed to an equal value.
#[salsa::tracked(returns(copy))]
pub fn interface_fingerprint(db: &dyn Db, file: SourceFile) -> u64 {
    fingerprint(&declaration_summary(db, file))
}

/// Spike database: Salsa storage plus an event log for the tests.
#[salsa::db]
#[derive(Clone)]
pub struct Database {
    storage: salsa::Storage<Self>,
    events: Arc<Mutex<Vec<String>>>,
}

impl Default for Database {
    fn default() -> Self {
        let events = Arc::new(Mutex::new(Vec::new()));
        let sink = Arc::clone(&events);
        let storage = salsa::Storage::new(Some(Box::new(move |event| {
            let label = match event.kind {
                salsa::EventKind::WillExecute { .. } => "WillExecute",
                salsa::EventKind::DidValidateMemoizedValue { .. } => "DidValidateMemoizedValue",
                _ => return,
            };
            push_event(&sink, label);
        })));
        Self { storage, events }
    }
}

#[salsa::db]
impl salsa::Database for Database {}

impl Database {
    /// Creates a tracked source file with `text`.
    pub fn add_source(&self, text: &str) -> SourceFile {
        SourceFile::new(self, text.to_string())
    }

    /// Replaces the text of `file`, creating a new input revision.
    pub fn set_source(&mut self, file: SourceFile, text: &str) {
        use salsa::Setter;
        file.set_text(self).to(text.to_string());
    }

    /// Drains the recorded Salsa events (query executions and validations).
    pub fn take_events(&self) -> Vec<String> {
        lock(&self.events).drain(..).collect()
    }

    /// Number of times the given label was observed since the last drain.
    pub fn count_events(&self, label: &str) -> usize {
        lock(&self.events)
            .iter()
            .filter(|entry| entry.as_str() == label)
            .count()
    }
}

fn push_event(events: &Mutex<Vec<String>>, label: &str) {
    lock(events).push(label.to_string());
}

fn lock(events: &Mutex<Vec<String>>) -> std::sync::MutexGuard<'_, Vec<String>> {
    match events.lock() {
        Ok(guard) => guard,
        // A poisoned log only means a test panicked; keep the data.
        Err(poisoned) => poisoned.into_inner(),
    }
}

/// Extracts the declaration summary from source text using the spike lexer.
fn summarize(source: &str) -> DeclarationSummary {
    let (tokens, _diagnostics) = lex(source);
    let significant: Vec<&Token<'_>> = tokens
        .iter()
        .filter(|token| !token.kind.is_trivia())
        .collect();

    let mut summary = DeclarationSummary::default();
    let mut in_var_block = false;
    let mut index = 0usize;

    while let Some(token) = significant.get(index) {
        if token.kind == TokenKind::Ident {
            let upper = token.text.to_ascii_uppercase();
            if is_pou_keyword(&upper) {
                summary.pou_kind = Some(upper);
                if let Some(name) = significant.get(index + 1) {
                    if name.kind == TokenKind::Ident {
                        summary.pou_name = Some(name.text.to_string());
                    }
                }
            } else if upper.starts_with("VAR") {
                in_var_block = true;
            } else if upper == "END_VAR" {
                in_var_block = false;
            } else if in_var_block {
                if let Some((decl, next)) = variable_decl(&significant, index) {
                    summary.variables.push(decl);
                    index = next;
                    continue;
                }
            }
        }
        index += 1;
    }

    summary
}

fn is_pou_keyword(upper: &str) -> bool {
    matches!(
        upper,
        "PROGRAM" | "FUNCTION_BLOCK" | "FUNCTION" | "CLASS" | "INTERFACE"
    )
}

/// Parses `name [AT direct-var] : TYPE` starting at `index`.
fn variable_decl(significant: &[&Token<'_>], index: usize) -> Option<(VariableDecl, usize)> {
    let name = significant.get(index)?;
    let mut cursor = index + 1;
    let mut found_colon = false;

    // `name : TYPE` or `name AT %IX0.0 : TYPE`.
    while let Some(token) = significant.get(cursor) {
        if token.kind == TokenKind::Operator && token.text == ":" {
            found_colon = true;
            break;
        }
        // Direct-variable addressing between AT and ':'.
        if token.kind == TokenKind::Operator && matches!(token.text, "%" | "." | "*")
            || token.kind == TokenKind::Ident
            || token.kind == TokenKind::IntegerLit
        {
            cursor += 1;
            continue;
        }
        break;
    }
    if !found_colon {
        return None;
    }

    let type_token = significant.get(cursor + 1)?;
    if type_token.kind != TokenKind::Ident {
        return None;
    }
    let decl = VariableDecl {
        name: name.text.to_string(),
        type_name: type_token.text.to_string(),
    };
    Some((decl, cursor + 1))
}

/// FNV-1a over the summary's observable fields.
fn fingerprint(summary: &DeclarationSummary) -> u64 {
    const OFFSET: u64 = 0xcbf2_9ce4_8422_2325;
    let mut hash = OFFSET;
    if let Some(kind) = &summary.pou_kind {
        hash = fnv(hash, kind.as_bytes());
    }
    if let Some(name) = &summary.pou_name {
        hash = fnv(hash, name.as_bytes());
    }
    for variable in &summary.variables {
        hash = fnv(hash, variable.name.as_bytes());
        hash = fnv(hash, variable.type_name.as_bytes());
    }
    hash
}

fn fnv(mut hash: u64, bytes: &[u8]) -> u64 {
    const PRIME: u64 = 0x0000_0100_0000_01b3;
    for byte in bytes {
        hash ^= u64::from(*byte);
        hash = hash.wrapping_mul(PRIME);
    }
    hash
}
