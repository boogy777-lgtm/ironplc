//! The parser's output: a flat event stream and the markers that build it.
//!
//! The parser does not build the tree. It records `Start`/`Token`/`Finish`
//! events that [`crate::cst`] replays into a rowan green tree (design:
//! parse-tree architecture, section 3.1). A binary node has to start before
//! the operands that are already emitted when the parser meets its operator;
//! a forward-parent link on the operand's `Start` lets the sink open the
//! wrapping node first, which is what makes left-associative Pratt parsing
//! possible in a single pass.

use crate::syntax_kind::SyntaxKind;

/// One step of tree construction.
#[derive(Debug, Clone, PartialEq, Eq)]
pub(crate) enum Event {
    /// Opens a node. `forward_parent`, when set, is the distance to a later
    /// `Start` whose node wraps this one.
    Start {
        kind: SyntaxKind,
        forward_parent: Option<u32>,
    },
    /// Emits one token, named by its index in the full token vector.
    Token { index: u32 },
    /// Closes the innermost open node.
    Finish,
    /// A slot reserved by a marker that has not been completed (or was
    /// abandoned); the sink skips it.
    Tombstone,
}

/// A reserved `Start` slot that [`Marker::complete`] fills in.
#[derive(Debug)]
pub(crate) struct Marker {
    pub(super) pos: usize,
}

impl Marker {
    /// Names the node that starts at this marker and closes it.
    pub(crate) fn complete(self, events: &mut Vec<Event>, kind: SyntaxKind) -> CompletedMarker {
        if let Some(slot) = events.get_mut(self.pos) {
            match slot {
                Event::Start { kind: existing, .. } => *existing = kind,
                other => {
                    *other = Event::Start {
                        kind,
                        forward_parent: None,
                    }
                }
            }
        }
        events.push(Event::Finish);
        CompletedMarker { pos: self.pos }
    }

    /// Drops the marker. When nothing was emitted after it the slot is
    /// removed; otherwise it stays as a tombstone the sink skips.
    pub(crate) fn abandon(self, events: &mut Vec<Event>) {
        if self.pos + 1 == events.len() {
            events.pop();
        } else if let Some(slot) = events.get_mut(self.pos) {
            *slot = Event::Tombstone;
        }
    }
}

/// A closed node that a later, enclosing node may adopt.
#[derive(Debug, Clone, Copy)]
pub(crate) struct CompletedMarker {
    pos: usize,
}

impl CompletedMarker {
    /// Starts a node that will wrap this one and everything emitted after it
    /// up to its own completion.
    pub(crate) fn precede(self, events: &mut Vec<Event>) -> Marker {
        let new_pos = events.len();
        events.push(Event::Tombstone);
        link_forward_parent(events, self.pos, new_pos);
        Marker { pos: new_pos }
    }
}

/// Points the outermost existing wrapper of the node at `from` to `to`.
fn link_forward_parent(events: &mut [Event], from: usize, to: usize) {
    let mut current = from;
    while let Some(Event::Start {
        forward_parent: Some(next),
        ..
    }) = events.get(current)
    {
        current += *next as usize;
    }
    if let Some(Event::Start { forward_parent, .. }) = events.get_mut(current) {
        *forward_parent = u32::try_from(to - current).ok();
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    fn start(events: &mut Vec<Event>) -> Marker {
        events.push(Event::Tombstone);
        Marker {
            pos: events.len() - 1,
        }
    }

    #[test]
    fn complete_when_marker_then_start_and_finish_events() {
        let mut events = Vec::new();
        let marker = start(&mut events);
        marker.complete(&mut events, SyntaxKind::NameRef);
        assert_eq!(
            events,
            vec![
                Event::Start {
                    kind: SyntaxKind::NameRef,
                    forward_parent: None
                },
                Event::Finish
            ]
        );
    }

    #[test]
    fn abandon_when_last_event_then_slot_removed() {
        let mut events = Vec::new();
        let marker = start(&mut events);
        marker.abandon(&mut events);
        assert!(events.is_empty());
    }

    #[test]
    fn abandon_when_events_follow_then_slot_is_tombstone() {
        let mut events = Vec::new();
        let marker = start(&mut events);
        events.push(Event::Token { index: 0 });
        marker.abandon(&mut events);
        assert_eq!(events, vec![Event::Tombstone, Event::Token { index: 0 }]);
    }

    #[test]
    fn precede_when_completed_marker_then_forward_parent_points_at_new_start() {
        let mut events = Vec::new();
        let inner = start(&mut events);
        events.push(Event::Token { index: 0 });
        let done = inner.complete(&mut events, SyntaxKind::NameRef);
        let outer = done.precede(&mut events);
        events.push(Event::Token { index: 1 });
        outer.complete(&mut events, SyntaxKind::BinaryExpr);
        assert_eq!(
            events.first(),
            Some(&Event::Start {
                kind: SyntaxKind::NameRef,
                forward_parent: Some(3)
            })
        );
    }

    #[test]
    fn precede_when_twice_then_chain_links_through_outermost_wrapper() {
        let mut events = Vec::new();
        let inner = start(&mut events);
        let done = inner.complete(&mut events, SyntaxKind::NameRef);
        let first = done
            .precede(&mut events)
            .complete(&mut events, SyntaxKind::BinaryExpr);
        first
            .precede(&mut events)
            .complete(&mut events, SyntaxKind::BinaryExpr);
        // NameRef -> first wrapper (at 2) -> second wrapper (at 4).
        assert_eq!(
            events.get(2),
            Some(&Event::Start {
                kind: SyntaxKind::BinaryExpr,
                forward_parent: Some(2)
            })
        );
    }
}
