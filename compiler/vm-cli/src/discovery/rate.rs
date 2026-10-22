//! The limit on how often the device answers discovery.
//!
//! Why a limit: the reply is up to 128 times the size of the request (8 bytes
//! in, up to [`ironplc_discovery::MAX_REPLY_LEN`] = 1024 bytes out), and a UDP
//! source address can be forged. Without a limit, anyone who can send datagrams
//! to the device could aim its answers at a victim and multiply their traffic.
//! Two limits bound that, and a third bounds memory:
//!
//! - **Per source address** ([`PER_SOURCE_INTERVAL`]): at most one reply per
//!   250 ms for each source IP address (the port is not part of the key, because
//!   a sender picks its port freely). A scan sends one request, and IronLink may
//!   retry it a couple of times, so the 4 replies per second that this allows
//!   each source are ample.
//! - **In total** ([`GLOBAL_REPLIES_PER_SECOND`]): at most 64 replies in any
//!   one second over all sources, so that the amplification is bounded whatever
//!   the number of spoofed sources. 64 replies of at most 1024 bytes are about
//!   64 KiB per second.
//! - **The table of sources** holds at most [`MAX_TRACKED_SOURCES`] entries.
//!   When it is full, the entry with the oldest last reply is evicted, so memory
//!   is bounded however many sources are forged. (A flood of forged sources can
//!   evict a real one early; the total limit still holds.)
//!
//! The clock is a parameter of [`RateLimiter::allow`], so the tests pass
//! instants and never sleep.

use std::collections::{BTreeMap, VecDeque};
use std::net::IpAddr;
use std::time::{Duration, Instant};

/// The shortest time between two replies to one source address.
pub(crate) const PER_SOURCE_INTERVAL: Duration = Duration::from_millis(250);

/// The most replies in any [`GLOBAL_WINDOW`], over all sources.
pub(crate) const GLOBAL_REPLIES_PER_SECOND: usize = 64;

/// The window of [`GLOBAL_REPLIES_PER_SECOND`].
pub(crate) const GLOBAL_WINDOW: Duration = Duration::from_secs(1);

/// The most source addresses remembered at once.
pub(crate) const MAX_TRACKED_SOURCES: usize = 256;

/// Decides whether the device may answer a request from a source now.
pub(crate) struct RateLimiter {
    /// The time of the last reply to each source address.
    ///
    /// Ordered, so that two sources with the same oldest reply are evicted in
    /// the same order every time.
    last_reply: BTreeMap<IpAddr, Instant>,
    /// The times of the replies in the last [`GLOBAL_WINDOW`], oldest first.
    recent: VecDeque<Instant>,
}

impl RateLimiter {
    pub(crate) fn new() -> Self {
        RateLimiter {
            last_reply: BTreeMap::new(),
            recent: VecDeque::with_capacity(GLOBAL_REPLIES_PER_SECOND),
        }
    }

    /// Whether a reply to `source` at `now` is within both limits. A `true`
    /// answer counts the reply: the caller sends it.
    pub(crate) fn allow(&mut self, source: IpAddr, now: Instant) -> bool {
        let source_ready = self
            .last_reply
            .get(&source)
            .is_none_or(|last| now.saturating_duration_since(*last) >= PER_SOURCE_INTERVAL);
        if !source_ready {
            return false;
        }
        self.forget_replies_out_of_window(now);
        if self.recent.len() >= GLOBAL_REPLIES_PER_SECOND {
            return false;
        }
        if self.last_reply.len() >= MAX_TRACKED_SOURCES && !self.last_reply.contains_key(&source) {
            self.evict_oldest();
        }
        self.last_reply.insert(source, now);
        self.recent.push_back(now);
        true
    }

    fn forget_replies_out_of_window(&mut self, now: Instant) {
        while self
            .recent
            .front()
            .is_some_and(|first| now.saturating_duration_since(*first) >= GLOBAL_WINDOW)
        {
            self.recent.pop_front();
        }
    }

    /// Removes the source whose last reply is the oldest.
    fn evict_oldest(&mut self) {
        let oldest = self
            .last_reply
            .iter()
            .min_by_key(|(_, last)| **last)
            .map(|(source, _)| *source);
        if let Some(source) = oldest {
            self.last_reply.remove(&source);
        }
    }
}

#[cfg(test)]
mod tests {
    use std::net::Ipv4Addr;

    use spec_test_macro::spec_test;

    use super::*;

    /// The address `10.0.<n / 256>.<n % 256>`: a different source for each `n`.
    fn source(n: u16) -> IpAddr {
        let [high, low] = n.to_be_bytes();
        IpAddr::V4(Ipv4Addr::new(10, 0, high, low))
    }

    fn after(start: Instant, millis: u64) -> Instant {
        start + Duration::from_millis(millis)
    }

    /// Asks for a reply to each of `count` different sources at `now`, and
    /// returns how many were allowed.
    fn allowed_among_new_sources(limiter: &mut RateLimiter, count: u16, now: Instant) -> usize {
        (0..count)
            .filter(|n| limiter.allow(source(*n), now))
            .count()
    }

    /// Gives each of `count` sources one reply, one every 16 ms (about 62 a
    /// second, under the total limit), starting at `start`.
    fn fill_sources_16_ms_apart(limiter: &mut RateLimiter, count: u16, start: Instant) {
        for n in 0..count {
            assert!(limiter.allow(source(n), after(start, u64::from(n) * 16)));
        }
    }

    #[test]
    fn allow_when_first_request_from_a_source_then_allowed() {
        let mut limiter = RateLimiter::new();

        assert!(limiter.allow(source(1), Instant::now()));
    }

    /// REQ-VC-vm-cli-034: the limits of the reply.
    #[spec_test(REQ_VC_vm_cli_034)]
    #[test]
    fn allow_when_second_request_within_250_ms_then_refused() {
        let mut limiter = RateLimiter::new();
        let start = Instant::now();
        limiter.allow(source(1), start);

        assert!(!limiter.allow(source(1), after(start, 249)));
    }

    #[test]
    fn allow_when_a_refused_request_repeats_then_the_interval_still_counts_from_the_reply() {
        let mut limiter = RateLimiter::new();
        let start = Instant::now();
        limiter.allow(source(1), start);
        limiter.allow(source(1), after(start, 200));

        assert!(limiter.allow(source(1), after(start, 250)));
    }

    #[test]
    fn allow_when_second_request_after_250_ms_then_allowed() {
        let mut limiter = RateLimiter::new();
        let start = Instant::now();
        limiter.allow(source(1), start);

        assert!(limiter.allow(source(1), after(start, 250)));
    }

    #[test]
    fn allow_when_a_different_source_asks_at_once_then_allowed() {
        let mut limiter = RateLimiter::new();
        let start = Instant::now();
        limiter.allow(source(1), start);

        assert!(limiter.allow(source(2), start));
    }

    #[test]
    fn allow_when_the_clock_runs_backwards_then_refused_not_panicking() {
        let mut limiter = RateLimiter::new();
        let start = Instant::now();
        limiter.allow(source(1), after(start, 1000));

        assert!(!limiter.allow(source(1), start));
    }

    /// REQ-VC-vm-cli-034: the limits of the reply.
    #[spec_test(REQ_VC_vm_cli_034)]
    #[test]
    fn allow_when_more_than_64_sources_ask_within_a_second_then_the_65th_is_refused() {
        let mut limiter = RateLimiter::new();
        let start = Instant::now();

        let allowed = allowed_among_new_sources(&mut limiter, 100, start);

        assert_eq!(allowed, GLOBAL_REPLIES_PER_SECOND);
    }

    #[test]
    fn allow_when_a_second_has_passed_since_the_replies_then_the_total_limit_lifts() {
        let mut limiter = RateLimiter::new();
        let start = Instant::now();
        allowed_among_new_sources(&mut limiter, 100, start);

        assert!(!limiter.allow(source(200), after(start, 999)));
        assert!(limiter.allow(source(200), after(start, 1000)));
    }

    /// REQ-VC-vm-cli-034: the limits of the reply.
    #[spec_test(REQ_VC_vm_cli_034)]
    #[test]
    fn allow_when_the_table_is_full_then_it_stays_at_256_and_drops_the_oldest_reply() {
        let mut limiter = RateLimiter::new();
        let start = Instant::now();
        fill_sources_16_ms_apart(&mut limiter, 256, start);
        let now = after(start, 256 * 16);

        assert!(limiter.allow(source(256), now));

        assert_eq!(limiter.last_reply.len(), MAX_TRACKED_SOURCES);
        assert!(!limiter.last_reply.contains_key(&source(0)));
        assert!(limiter.last_reply.contains_key(&source(1)));
        assert!(limiter.last_reply.contains_key(&source(255)));
        assert!(limiter.last_reply.contains_key(&source(256)));
    }

    #[test]
    fn allow_when_the_table_is_full_and_a_known_source_asks_then_nothing_is_evicted() {
        let mut limiter = RateLimiter::new();
        let start = Instant::now();
        fill_sources_16_ms_apart(&mut limiter, 256, start);

        assert!(limiter.allow(source(0), after(start, 256 * 16)));

        assert_eq!(limiter.last_reply.len(), MAX_TRACKED_SOURCES);
        assert!(limiter.last_reply.contains_key(&source(1)));
    }

    #[test]
    fn allow_when_a_request_is_refused_by_the_total_limit_then_its_source_is_not_counted() {
        let mut limiter = RateLimiter::new();
        let start = Instant::now();
        allowed_among_new_sources(&mut limiter, 100, start);

        assert!(!limiter.allow(source(99), start));

        assert!(!limiter.last_reply.contains_key(&source(99)));
    }
}
