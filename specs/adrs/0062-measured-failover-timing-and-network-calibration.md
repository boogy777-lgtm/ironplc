# Measured Failover Timing and Network Calibration

status: accepted
date: 2026-09-16

## Context

The redundancy layer (see `specs/design/ha-redundancy-fsm.md`) needs a
failover-time story the engineer can trust. A constant promise is
unattainable: takeover time depends on heartbeat transport, per-module
claim/ARM behavior, scan position, and output apply, all of which vary per
installation and per device. The honest contract is a formula whose terms
are measured, not a marketing number.

## Decision

- **Failover/takeover time is NEVER promised as a constant.** It is computed
  from a formula whose terms come from (a) qualification bounds and (b)
  continuous runtime measurements of the actual installation: per-channel
  heartbeat latencies, per-module claim/ARM latencies, scan safe-point, and
  output apply. Each term is tracked as current / EMA10 / EMA100 / max /
  count. I/O firmware instruments its own delays and reports them; the PLC
  measures peer-detection. The engineering UI (Studio) shows per-device
  contributions and the limiting device.
- **TakeoverReady** = `SYNC_READY && IO_READY && RedundancyLinkValid`;
  **IO_READY** = all required inputs observable && standby connections valid
  && configs match && epochs valid.
- **Controlled (manual) switchover** targets zero old-owner-timeout via an
  explicit `RELEASE_OWNER`; **uncontrolled failover** starts at
  `max(PLC peer-detection, I/O owner-lease expiry)`.
- **Epoch is anti-stale protection ONLY** — it rejects stale packets,
  images, and claims; it is never the arbiter of who may take over (this
  prevents partition bidding wars). **OwnerLease is generated exclusively by
  the HA supervisor at scan commit** — the network task must never mint it.

## Consequences

- Studio can display an honest, per-installation takeover estimate and name
  the device that dominates it, instead of quoting a fixed bound.
- Manual swap avoids the old-owner timeout entirely; uncontrolled failover
  pays detection plus lease expiry, and both terms are measured.
- Epoch and OwnerLease have disjoint producers and purposes, so a network
  fault cannot fabricate ownership and a partition cannot escalate epoch.
