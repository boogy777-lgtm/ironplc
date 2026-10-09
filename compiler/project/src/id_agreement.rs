//! Whether the stable variable IDs of a project agree with its declarations
//! (ADR-0074).
//!
//! The controller refuses a migration in which a persistent variable has no ID.
//! That refusal comes after the candidate has been sent. The workstation can
//! tell sooner: the keys the project declares ([`declared_var_keys`], the
//! persistent declarations code generation lays out) and the keys the sidecar
//! holds are both known before anything is sent. A sender of a candidate for
//! online change reads the verdict of this module, which [`crate::compile`]
//! attaches to its output, and does not send when the two differ.
//!
//! A project without IDs has an empty table and no variable has an ID, so the
//! controller refuses a change of layout itself (V4007); the table is not
//! compared then. The difference is only a difference when IDs exist.

use std::fmt;

use ironplc_dsl::common::Library;
use ironplc_parser::options::CompilerOptions;

use crate::sidecar::{declared_var_keys, SidecarKey};

/// How the declared persistent variables and the stable variable IDs differ.
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct IdDifference {
    /// Declared persistent variables that have no ID: added, renamed or moved
    /// since the last synchronization, or left out by a synchronization that
    /// stopped at unresolved rename or swap candidates.
    pub without_id: Vec<SidecarKey>,
    /// IDs that no persistent declaration claims: variables removed, renamed or
    /// moved since the last synchronization.
    pub undeclared: Vec<SidecarKey>,
}

impl fmt::Display for IdDifference {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(
            f,
            "the stable variable IDs are out of sync with the declarations:"
        )?;
        let mut separator = " ";
        if !self.without_id.is_empty() {
            write!(f, "{separator}no ID for {}", join(&self.without_id))?;
            separator = "; ";
        }
        if !self.undeclared.is_empty() {
            write!(f, "{separator}IDs of undeclared {}", join(&self.undeclared))?;
        }
        write!(
            f,
            ". Synchronize them with `ironplcc refactor sync-uids` (resolve reported \
             rename or swap candidates with `ironplcc refactor map-uid`) and build again."
        )
    }
}

/// The keys as `scope.name`, comma separated.
fn join(keys: &[SidecarKey]) -> String {
    keys.iter()
        .map(ToString::to_string)
        .collect::<Vec<_>>()
        .join(", ")
}

/// Compares the persistent declarations of `library` with the stable variable
/// IDs `ids` and returns what differs, or `None` when they agree or when there
/// are no IDs.
pub fn id_difference(
    library: &Library,
    options: &CompilerOptions,
    ids: &[(SidecarKey, u64)],
) -> Option<IdDifference> {
    if ids.is_empty() {
        return None;
    }
    let declared = declared_var_keys(library, options);
    let without_id: Vec<SidecarKey> = declared
        .iter()
        .filter(|key| !ids.iter().any(|(held, _)| held == *key))
        .cloned()
        .collect();
    let mut undeclared: Vec<SidecarKey> = ids
        .iter()
        .map(|(key, _)| key)
        .filter(|key| !declared.contains(key))
        .cloned()
        .collect();
    undeclared.sort();
    if without_id.is_empty() && undeclared.is_empty() {
        None
    } else {
        Some(IdDifference {
            without_id,
            undeclared,
        })
    }
}

#[cfg(test)]
mod tests {
    use ironplc_dsl::core::FileId;

    use super::*;

    fn library(source: &str) -> Library {
        ironplc_sources::parse_source(
            ironplc_sources::FileType::StructuredText,
            source,
            &FileId::from_string("main.st"),
            &CompilerOptions::default(),
        )
        .unwrap()
    }

    fn ids(keys: &[(&str, &str, u64)]) -> Vec<(SidecarKey, u64)> {
        keys.iter()
            .map(|(scope, name, uid)| (SidecarKey::new(scope, name), *uid))
            .collect()
    }

    const PROGRAM: &str = "PROGRAM main VAR a : INT; b : INT; END_VAR a := b; END_PROGRAM";

    #[test]
    fn id_difference_when_every_declaration_has_an_id_then_none() {
        let held = ids(&[("main", "a", 1), ("main", "b", 2)]);

        assert_eq!(
            id_difference(&library(PROGRAM), &CompilerOptions::default(), &held),
            None
        );
    }

    #[test]
    fn id_difference_when_there_are_no_ids_then_none() {
        assert_eq!(
            id_difference(&library(PROGRAM), &CompilerOptions::default(), &[]),
            None
        );
    }

    #[test]
    fn id_difference_when_a_declaration_has_no_id_then_names_it() {
        let held = ids(&[("main", "a", 1)]);

        let difference =
            id_difference(&library(PROGRAM), &CompilerOptions::default(), &held).unwrap();

        assert_eq!(difference.without_id, vec![SidecarKey::new("main", "b")]);
        assert!(difference.undeclared.is_empty());
    }

    #[test]
    fn id_difference_when_an_id_is_for_an_undeclared_variable_then_names_it() {
        let held = ids(&[("main", "a", 1), ("main", "b", 2), ("main", "gone", 3)]);

        let difference =
            id_difference(&library(PROGRAM), &CompilerOptions::default(), &held).unwrap();

        assert!(difference.without_id.is_empty());
        assert_eq!(difference.undeclared, vec![SidecarKey::new("main", "gone")]);
    }

    #[test]
    fn display_when_both_differ_then_names_both_and_the_remedy() {
        let held = ids(&[("main", "a", 1), ("main", "gone", 3)]);

        let text = id_difference(&library(PROGRAM), &CompilerOptions::default(), &held)
            .unwrap()
            .to_string();

        assert_eq!(
            text,
            "the stable variable IDs are out of sync with the declarations: no ID for main.b; \
             IDs of undeclared main.gone. Synchronize them with `ironplcc refactor sync-uids` \
             (resolve reported rename or swap candidates with `ironplcc refactor map-uid`) \
             and build again."
        );
    }
}
