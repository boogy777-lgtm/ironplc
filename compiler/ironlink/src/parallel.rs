//! Work on several items at once, with the results in the order of the items.
//!
//! The drivers of a browse wait for their deadlines at the same time, and so do
//! the sockets of one driver, so the time a browse takes is the longest wait and
//! not the sum of them.

use std::thread;

use crate::error::IronLinkError;

/// Runs `work` on every item on a thread of its own and returns the results in
/// the order of `items`, or the first error in that order.
///
/// A thread that cannot be started, or that panicked, is an internal error
/// (L9001) and not a panic of the caller.
pub(crate) fn map_in_parallel<T, R>(
    items: &[T],
    work: impl Fn(&T) -> Result<R, IronLinkError> + Sync,
) -> Result<Vec<R>, IronLinkError>
where
    T: Sync,
    R: Send,
{
    thread::scope(|scope| {
        let work = &work;
        let started: Vec<_> = items
            .iter()
            .map(|item| {
                thread::Builder::new()
                    .name("ironlink-worker".to_string())
                    .spawn_scoped(scope, move || work(item))
            })
            .collect();
        // Every started thread is joined before the first error is returned, so
        // that the panic of a later one is an error here and not of the scope.
        let finished: Vec<Result<R, IronLinkError>> = started
            .into_iter()
            .map(|started| {
                let handle = started.map_err(|err| {
                    IronLinkError::internal(format!("unable to start a thread: {err}"))
                })?;
                handle
                    .join()
                    .map_err(|_| IronLinkError::internal("a worker thread panicked"))?
            })
            .collect();
        finished.into_iter().collect()
    })
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn map_in_parallel_when_all_succeed_then_results_in_item_order() {
        let items = [3_u64, 1, 2];

        let results = map_in_parallel(&items, |n| Ok(n * 10)).unwrap();

        assert_eq!(results, vec![30, 10, 20]);
    }

    #[test]
    fn map_in_parallel_when_an_item_fails_then_first_error_in_item_order() {
        let items = [1_u64, 2, 3];

        let err = map_in_parallel(&items, |n| {
            if *n >= 2 {
                Err(IronLinkError::argument_invalid(format!("item {n}")))
            } else {
                Ok(*n)
            }
        })
        .unwrap_err();

        assert_eq!(err.message(), "item 2");
    }

    #[test]
    fn map_in_parallel_when_a_worker_panics_then_internal_error() {
        let items = [1_u64];

        let err = map_in_parallel(&items, |_| -> Result<u64, IronLinkError> {
            panic!("worker bug")
        })
        .unwrap_err();

        assert_eq!(err.l_code(), "L9001");
    }

    #[test]
    fn map_in_parallel_when_no_items_then_empty() {
        let items: [u64; 0] = [];

        assert!(map_in_parallel(&items, |n| Ok(*n)).unwrap().is_empty());
    }
}
