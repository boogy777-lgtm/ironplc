//! A row of the registry: what the compiler knows about one attribute.

use std::marker::PhantomData;

use super::targets::Targets;
use super::value::{Value, ValueKind};
use crate::attribute::Attributes;
use crate::common::VariableType;

/// Declares [`Family`], its list [`Family::ALL`] and its titles from one list.
macro_rules! families {
    ($($variant:ident => $title:literal,)*) => {
        /// What an attribute concerns in CODESYS; the reference page groups the
        /// rows by it.
        #[derive(Clone, Copy, Debug, PartialEq, Eq)]
        pub enum Family {
            $($variant,)*
        }

        impl Family {
            /// Every family, in the order the reference page lists them.
            pub const ALL: &'static [Family] = &[$(Family::$variant,)*];

            /// How the reference page names the family.
            pub fn title(self) -> &'static str {
                match self {
                    $(Family::$variant => $title,)*
                }
            }
        }
    };
}

families! {
    Enumeration => "Enumeration",
    NameScope => "Name scope",
    Layout => "Layout",
    Visibility => "Visibility",
    Display => "Display",
    Messages => "Messages",
    CompileChecks => "Compile checks",
    StaticAnalysis => "Static analysis",
    Initialisation => "Initialisation",
    OnlineChange => "Online change",
    CodeGeneration => "Code generation",
    Inheritance => "Inheritance",
    Symbols => "Symbols",
    IoMapping => "I/O mapping",
    GraphicalEditors => "Graphical editors",
}

/// What IronPLC does with a row.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum State {
    /// No part of IronPLC acts on the attribute yet: the compiler reads and
    /// checks it and does nothing else with it.
    Waiting,
    /// A part of IronPLC acts on the attribute.
    Honoured,
}

impl State {
    /// How the reference page names the state.
    pub fn title(self) -> &'static str {
        match self {
            State::Waiting => "Waiting",
            State::Honoured => "Honoured",
        }
    }
}

/// A row without the type of its value: what every reader of the registry
/// needs, whatever the attribute takes.
#[derive(Clone, Copy, Debug)]
pub struct Info {
    /// The name, as the row spells it.
    pub name: &'static str,
    /// Other names of the attribute.
    pub aliases: &'static [&'static str],
    /// The kinds of declaration the attribute may stand before.
    pub targets: Targets,
    /// The kinds of variable block that a declaration with the attribute may
    /// not hold; none for most rows.
    pub forbids: &'static [VariableType],
    /// What the attribute takes.
    pub value: ValueKind,
    pub family: Family,
    /// One line: what the attribute does in CODESYS.
    pub summary: &'static str,
    pub state: State,
    /// Whether the value is one the type of the row reads: the same
    /// [`Value::parse`] as the reader.
    valid: fn(Option<&str>) -> bool,
}

/// Whether `V` reads `text`.
fn reads<V: Value>(text: Option<&str>) -> bool {
    V::parse(text).is_some()
}

impl Info {
    /// Whether the attribute `name` is this one: its name or an alias, without
    /// regard to case.
    pub fn answers_to(&self, name: &str) -> bool {
        self.name.eq_ignore_ascii_case(name)
            || self
                .aliases
                .iter()
                .any(|alias| alias.eq_ignore_ascii_case(name))
    }

    /// Whether the row reads `value`, the value of an attribute as written.
    pub fn accepts(&self, value: Option<&str>) -> bool {
        (self.valid)(value)
    }
}

/// Two rows are the same row when they have the same name; the guard test of
/// the registry holds the names apart.
impl PartialEq for Info {
    fn eq(&self, other: &Self) -> bool {
        self.name == other.name
    }
}

impl Eq for Info {}

/// A row with the type of its value, and so the reader of it.
///
/// A row exists only as an entry of the declaration list (`rows!`), which
/// makes both the constant of the row and its place in [`super::ALL`].
#[derive(Debug)]
pub struct Row<V> {
    info: Info,
    value: PhantomData<fn() -> V>,
}

impl<V: Value> Row<V> {
    /// A row. Only the declaration list makes one.
    pub(super) const fn new(
        name: &'static str,
        aliases: &'static [&'static str],
        targets: Targets,
        family: Family,
        summary: &'static str,
        state: State,
    ) -> Self {
        Row {
            info: Info {
                name,
                aliases,
                targets,
                forbids: &[],
                value: V::KIND,
                family,
                summary,
                state,
                valid: reads::<V>,
            },
            value: PhantomData,
        }
    }

    /// The row, with the kinds of variable block that a declaration with the
    /// attribute may not hold.
    pub(super) const fn forbidding(mut self, forbids: &'static [VariableType]) -> Self {
        self.info.forbids = forbids;
        self
    }

    /// The row without the type of its value.
    pub const fn info(&self) -> Info {
        self.info
    }

    /// The value of this attribute on a declaration: the first attribute of
    /// the declaration named by the row or an alias, read as `V`. Nothing when
    /// the declaration has none, or when its value is not one the row reads
    /// (the check of the value reports that).
    pub fn of(&self, attributes: &Attributes) -> Option<V> {
        let attribute = attributes
            .iter()
            .find(|attribute| self.info.answers_to(&attribute.name))?;
        V::parse(attribute.value.as_deref())
    }
}
