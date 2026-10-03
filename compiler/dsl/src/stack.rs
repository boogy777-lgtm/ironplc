//! The stack the compile pipeline runs on, and the deepest tree it is built to
//! hold.
//!
//! Every stage that reads or builds a tree of language elements (parse,
//! analyze, generate code, print) recurses as deep as the tree is. How deep a
//! tree may be and how much stack the stages are given are therefore one
//! decision, and it is made here and nowhere else:
//!
//! - [`STACK_BUDGET`] is a property of the platform: what a thread of the
//!   compiler is given.
//! - [`MAX_DEPTH`] is derived from it by one rule, below.
//! - [`within_stack_budget`] is how a stage gets the budget. Each public entry
//!   of a stage runs its work through it, so a caller cannot run a stage
//!   without the budget and a new caller of a stage has nothing to remember. A
//!   new stage is one more entry that does the same.

use std::cell::Cell;
use std::panic::resume_unwind;
use std::thread;

/// What a thread of the compiler is given.
///
/// On a 64-bit system, which is all the compiler ships for (Windows x64 and
/// Linux), a thread stack is a reservation of address space: pages are
/// committed as they are used, so a large stack costs nothing until a deep
/// tree uses it. WebAssembly has no threads and a shadow stack of 1 MiB that
/// cannot grow, so there the budget is that.
pub const STACK_BUDGET: usize = if cfg!(target_arch = "wasm32") {
    1024 * 1024
} else {
    64 * 1024 * 1024
};

/// The depth the compiler is built to accept: a path of a thousand nested
/// elements, from the root of the tree down. It is a policy, not a measurement;
/// no program written by a person nests anything near it. Where the budget
/// cannot hold it, [`MAX_DEPTH`] is lower.
pub const DEPTH_TARGET: usize = 1000;

/// The most stack, in bytes, that any stage of the pipeline uses for one level
/// of nesting, measured over every stage (parse, syntax tree walk, lowering,
/// `Visitor`, `Fold`, analysis, code generation, printing, the drops) and every
/// construct that nests (calls, parentheses, `IF`, `CASE`, a long sum, `^`,
/// `.member`, `[index]`, ...) at a depth of 1000. The worst is the parse of
/// nested `IF` by the legacy parser, then the analysis of nested calls; the
/// stages of the new parser need a fifth or less. A debug build, whose frames
/// are several times larger, is measured on its own.
pub const WORST_COST_PER_LEVEL: usize = if cfg!(debug_assertions) {
    COST_PER_LEVEL_DEBUG
} else {
    COST_PER_LEVEL_RELEASE
};

/// A release build, the one that ships: 10.8 KB measured (6.6 KB for
/// analysis), with margin.
const COST_PER_LEVEL_RELEASE: usize = 12 * 1024;

/// A debug build: 40.4 KB measured (39.9 KB for analysis), with margin.
const COST_PER_LEVEL_DEBUG: usize = 48 * 1024;

/// The deepest tree the compiler accepts, counting the root: no path from the
/// root down holds more nodes. It is the largest depth the stack budget holds,
/// capped at [`DEPTH_TARGET`]:
///
/// `MAX_DEPTH = min(DEPTH_TARGET, STACK_BUDGET / WORST_COST_PER_LEVEL)`
///
/// which is 1000 where the budget is 64 MiB (Windows x64 and Linux), and 85
/// on `wasm32`, where it is 1 MiB. The same rule gives the same result for
/// both builds of a platform where it matters: native, 1000 in a release and
/// in a debug build.
pub const MAX_DEPTH: usize = {
    let fits = STACK_BUDGET / WORST_COST_PER_LEVEL;
    if fits < DEPTH_TARGET {
        fits
    } else {
        DEPTH_TARGET
    }
};

// What the rule promises, checked where it is made, for every platform and
// build: the limit never exceeds the target, the budget holds a tree that deep
// at the worst cost of a level, and the limit is the target unless one more
// level would not fit.
const _: () = {
    assert!(MAX_DEPTH <= DEPTH_TARGET);
    assert!(MAX_DEPTH * WORST_COST_PER_LEVEL <= STACK_BUDGET);
    assert!(MAX_DEPTH == DEPTH_TARGET || (MAX_DEPTH + 1) * WORST_COST_PER_LEVEL > STACK_BUDGET);
};

thread_local! {
    /// True on a thread that [`within_stack_budget`] made, whose stack is the
    /// budget, so that a stage that calls another does not make a thread each.
    static ON_BUDGET: Cell<bool> = const { Cell::new(false) };
}

/// Runs `work` on a stack of [`STACK_BUDGET`] and returns what it returns.
///
/// The work is run on a thread of its own and the caller waits for it, so it
/// borrows from the caller and the result comes back like any call. Only what
/// crosses the thread has to be `Send`: the work and its result. A panic of
/// the work is the panic of the caller, with its payload. Called from work
/// that already runs on the budget, it just runs, so a stage that calls
/// another stage takes one thread, not two. Where there are no threads
/// (`wasm32`), or the system cannot make one, the work runs on the caller's
/// stack, which is then the budget there is.
pub fn within_stack_budget<R: Send>(work: impl FnOnce() -> R + Send) -> R {
    if cfg!(target_arch = "wasm32") || ON_BUDGET.get() {
        return work();
    }
    // A thread that cannot start drops the work it was given, so the thread
    // takes the work from a slot the caller keeps, and the caller still has it
    // when there is no thread.
    let mut slot = Some(work);
    let outcome = thread::scope(|scope| {
        let lent = &mut slot;
        thread::Builder::new()
            .stack_size(STACK_BUDGET)
            .spawn_scoped(scope, move || {
                ON_BUDGET.set(true);
                lent.take().map(|work| work())
            })
            .map(|handle| handle.join())
    });
    match outcome {
        Ok(Ok(Some(result))) => result,
        Ok(Err(panic)) => resume_unwind(panic),
        // The thread took the work and the slot is empty: not reachable.
        Ok(Ok(None)) => resume_unwind(Box::new("the stack budget thread ran no work")),
        Err(_) => match slot.take() {
            Some(work) => work(),
            None => resume_unwind(Box::new("the stack budget work is gone")),
        },
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::panic::{catch_unwind, resume_unwind, AssertUnwindSafe};

    /// Uses 64 KiB of stack a level, so that a few dozen levels need more than
    /// the 2 MiB a test thread has.
    fn burn_stack(levels: usize) -> usize {
        let frame = [1u8; 64 * 1024];
        let frame = std::hint::black_box(frame);
        if levels == 0 {
            0
        } else {
            burn_stack(levels - 1) + usize::from(frame[0])
        }
    }

    #[cfg(not(target_arch = "wasm32"))]
    #[test]
    fn max_depth_when_native_then_the_target_on_a_budget_of_64_mib() {
        assert_eq!(STACK_BUDGET, 64 * 1024 * 1024);
        assert_eq!(MAX_DEPTH, 1000);
    }

    #[cfg(not(target_arch = "wasm32"))]
    #[test]
    fn within_stack_budget_when_work_needs_more_than_the_callers_stack_then_it_completes() {
        // 160 levels of 64 KiB are 10 MiB.
        assert_eq!(within_stack_budget(|| burn_stack(160)), 160);
    }

    #[test]
    fn within_stack_budget_when_work_borrows_from_the_caller_then_the_borrow_is_kept() {
        let mut seen = vec![];
        within_stack_budget(|| seen.push(1));
        assert_eq!(seen, vec![1]);
    }

    #[test]
    fn within_stack_budget_when_called_from_the_budget_then_it_runs_on_the_same_thread() {
        let (outer, inner) = within_stack_budget(|| {
            (
                thread::current().id(),
                within_stack_budget(|| thread::current().id()),
            )
        });
        assert_eq!(inner, outer);
        assert_ne!(outer, thread::current().id());
    }

    #[test]
    fn within_stack_budget_when_work_panics_then_the_panic_reaches_the_caller() {
        let caught = catch_unwind(AssertUnwindSafe(|| {
            within_stack_budget(|| resume_unwind(Box::new(7_u32)));
        }));
        assert_eq!(
            caught.err().and_then(|p| p.downcast::<u32>().ok()),
            Some(Box::new(7))
        );
    }
}
