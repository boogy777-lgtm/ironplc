//! The constructs that nest, as source text nested `n` times: the one table
//! every test of the depth limit draws from, whichever parser or stage it
//! drives.

/// What a construct is parsed as.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum Entry {
    Expression,
    Statements,
    File,
}

/// A construct that nests, and what its nesting makes of the tree.
pub struct Nesting {
    pub name: &'static str,
    pub entry: Entry,
    /// The source nested `n` times.
    pub build: fn(usize) -> String,
    /// True when each nesting adds exactly one node on the deepest path, so
    /// the tree is exactly the limit deep where it is as deep as it may be.
    pub one_node_a_level: bool,
}

pub const NESTINGS: &[Nesting] = &[
    Nesting {
        name: "dereferences",
        entry: Entry::Expression,
        build: |n| format!("a{}", "^".repeat(n)),
        one_node_a_level: true,
    },
    Nesting {
        name: "members",
        entry: Entry::Expression,
        build: |n| format!("a{}", ".b".repeat(n)),
        one_node_a_level: true,
    },
    Nesting {
        name: "subscripts",
        entry: Entry::Expression,
        build: |n| format!("a{}", "[1]".repeat(n)),
        one_node_a_level: true,
    },
    Nesting {
        name: "sum",
        entry: Entry::Expression,
        build: |n| vec!["a"; n].join("+"),
        one_node_a_level: true,
    },
    Nesting {
        name: "parentheses",
        entry: Entry::Expression,
        build: |n| format!("{}1{}", "(".repeat(n), ")".repeat(n)),
        one_node_a_level: false,
    },
    Nesting {
        name: "calls",
        entry: Entry::Expression,
        build: |n| format!("{}1{}", "f(".repeat(n), ")".repeat(n)),
        one_node_a_level: false,
    },
    Nesting {
        name: "if statements",
        entry: Entry::Statements,
        build: |n| format!("{}x := 1;{}", "IF c THEN ".repeat(n), " END_IF;".repeat(n)),
        one_node_a_level: false,
    },
    Nesting {
        name: "case statements",
        entry: Entry::Statements,
        build: |n| {
            format!(
                "{}x := 1;{}",
                "CASE a OF 1: ".repeat(n),
                " END_CASE;".repeat(n)
            )
        },
        one_node_a_level: false,
    },
    Nesting {
        name: "namespaces",
        entry: Entry::File,
        build: |n| format!("{}{}", "NAMESPACE n ".repeat(n), "END_NAMESPACE ".repeat(n)),
        one_node_a_level: false,
    },
];

impl Nesting {
    /// The most the construct nests before `too_deep` reports its source as
    /// too deep, found by bisection: the report is there from some depth on.
    /// `limit` is the depth the report is for; a nesting takes at most a few
    /// source levels for each tree level.
    pub fn deepest(&self, limit: usize, too_deep: impl Fn(&str) -> bool) -> usize {
        self.deepest_by(limit, |n| too_deep(&(self.build)(n)))
    }

    /// Like [`Nesting::deepest`], for a test that builds its source around the
    /// nesting itself: `too_deep` is asked about a number of levels.
    pub fn deepest_by(&self, limit: usize, too_deep: impl Fn(usize) -> bool) -> usize {
        let (mut fine, mut too_much) = (0, 4 * limit);
        while too_much - fine > 1 {
            let middle = (fine + too_much) / 2;
            if too_deep(middle) {
                too_much = middle;
            } else {
                fine = middle;
            }
        }
        fine
    }
}
