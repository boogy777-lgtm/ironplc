# PLC Discovery and Target Selection

## Goal

The engineer finds the PLCs on the network, picks the one to work with, and the
project remembers that choice. This is what RSLinx (Rockwell) and the gateway
with "Scan network" (CODESYS) do.

- **Discovery.** A PLC answers a "who is there" request on its own port, with
  identity only.
- **Browse.** A tree with workstation, then network (an interface or a list of
  addresses), then device, as RSWho shows it.
- **Target selection.** The project binds to a device by its identity, not by
  its IP address. Connect checks that it is the same device.
- **A communication service on the workstation**, the RSLinx part. One process
  owns discovery and the sessions to devices. VS Code, MCP and the command line
  are thin clients of it, through one shell of internal commands.

## Reference (research 2026-10-21)

- **RSLinx.**
  - It is a separate communication server, and Studio 5000 and FactoryTalk go
    through it.
  - EtherNet/IP `ListIdentity` (command `0x63`) is a broadcast on UDP 44818. The
    session uses TCP 44818, the same number.
  - The reply holds vendor, device type, product code, revision, status, serial
    number, product name, state and socket address.
  - Behind a router, the "Ethernet devices" driver uses a static address list
    sent unicast, because a broadcast does not cross routers.
  - "Set Project Path" stores the chosen path in the project.
  - Sources: pycomm3 and Scapy for the reply layout. The Rockwell pages are
    secondary sources.
- **CODESYS.**
  - The gateway listens on TCP 1217. The scan uses UDP 1740–1743.
  - The scan result shows device name, address, serial number, vendor, target
    id, type and version.
  - The project binds to the device name if it is unique, otherwise to the
    device address, not to the IP.
  - A filter hides devices whose target id differs from the project.
  - The gateway and scan assemblies are not in our decompiled corpus. These facts
    come from the CODESYS online help.
- **Others.**
  - Beckhoff ADS: search on UDP 48899, session on TCP 48898. Advisory 2019-004:
    one empty UDP packet stopped the search service.
  - Siemens: DCP on layer 2, which does not pass a VPN.
  - mDNS/DNS-SD (RFC 6763).
- **Security.**
  - A discovery reply is unauthenticated, so it carries no secret. It is a
    reconnaissance surface.
  - Answer only on chosen interfaces and rate-limit the replies.
  - Treat every received length as untrusted.
  - Never connect silently to a device whose identity changed.

## Current state (sweep of `main` 3e5a601ed)

- **Device side.** `ironplcvm serve <FILE>` talks over stdio, or over TCP with
  `--listen <ADDR>` (`vm-cli/src/main.rs:85-93`, `tcp.rs:318-330`).
  - There is no default port.
  - There is one session at a time (V6014, ADR-0065).
  - The session has 7 commands (`runtime/src/commands.rs:29-63`). There is no
    authentication on the wire (ADR-0063 defers it).
- **The port 49152** exists only in TypeScript (`connectionProfiles.ts:48`) and
  in about six repeated literals in the schema and the docs.
- **No UDP discovery.** No broadcast, multicast or beacon code exists in
  `compiler/`. The only UDP is the HA pair link to one configured peer
  (`ironplc-redundancy/src/udp.rs`).
- **No unique device id.** `DeviceIdentity` has name, model, modification and
  firmware only (`commands.rs:228-237`), with the same fixed values for every
  `ironplcvm`.
- **Target selection today.** The user writes profiles in `settings.json`
  (`ironplc.connections`) and picks one from a QuickPick
  (`connection.ts:272-327`).
  - There is no scan and no add command.
  - The device panel does not show the address.
- **The IDE owns the session.** `ConnectionManager` lives in the extension host.
- **MCP reaches no device.** Its hot-edit tools run an in-process host
  (`mcp/src/tools/hot_edit.rs:13-16`).
- **No workstation service** exists or is designed. The "controller daemon"
  in the specs is device-side.

## Architecture (proposal)

### 1. One port number, one definition

- The IronPLC port (proposal: 49152) is TCP for the session and UDP for
  discovery, as EtherNet/IP uses 44818 for both.
- It is defined once in Rust. TypeScript and the docs take it from that
  definition: a generated value, or a test that compares the two.
- `--listen` without a port uses it.

### 2. Device identity becomes unique

- `DeviceIdentity` gains `deviceId`. The device generates it on the first start
  and stores it beside its slots, as a file the device owns. A reboot keeps it.
  A new device, or a device that was reset, gets a new one.
- The soft PLC has no serial number, so the id is a random 128-bit value. A
  hardware target may report its serial instead.
- The field is optional and additive, so the protocol version stays 1 (as
  checked in ADR-0083).

### 3. Discovery responder on the device

- With `--listen`, the device also answers discovery on UDP at the same port,
  only on the listen interface.
  - `--no-discovery` turns it off.
  - Replies are rate-limited per source.
  - The request is fixed-size. Anything else gets no reply and no error.
- **Reply:** the protocol version, `deviceId`, device name, model and firmware,
  the session port, and whether the session is busy.
  - Proposal: also the application name and its `contentHash`, so the browse
    tree shows what the device runs. This is a question for the owner (Q5).
  - Nothing secret.
- **Wire format:** one small crate (encode and decode, `no_std`-ready), shared
  by the device and the service. It has its own version byte and bounds checks.
  It has fuzz-style tests for truncated and oversized input.

### 4. The communication service on the workstation (the RSLinx analog)

- **A separate binary** (name to choose, proposal `ironplclink`). It runs on
  the workstation and listens on localhost only.
- **Drivers**, one per network kind; each is a `Driver` implementation:
  - **broadcast:** discovery on chosen interfaces;
  - **address list:** unicast to configured addresses, for routed networks.

  A later driver (for example a serial port or a gateway on another machine) is
  one more implementation.
- **Browse:** it gives the tree workstation, then driver, then devices, with
  identity and the time last seen.
- **Sessions:** it owns the sessions to devices.
  - One engineering session per device stays the rule (ADR-0065).
  - Through the service, several clients (VS Code and MCP) can use the same
    device without fighting over the one session. This is the main value of
    RSLinx.
- **The shell of internal commands.** One command vocabulary, served to every
  client: `browse`, `identify`, `select`, `connect`, `disconnect`, `status`,
  plus the session commands relayed to the device.
  - The command line uses it (`ironplclink <command>`, and an interactive mode).
    VS Code and MCP use the same commands.
  - The session protocol stays as it is. The service relays it. It does not
    reinterpret it.

### 5. Target selection and binding

- **Browse view in VS Code** ("IronPLC Network", the RSWho analog): it shows
  the tree from the service. "Select as target" binds the workspace connection
  to that `deviceId`, and stores the address it was seen at.
- **Connect checks the device:**
  - it goes to the stored address;
  - it compares `deviceId` from `identity`;
  - a different id is a new problem code: no session, and the option to
    re-select;
  - when the address is not answered, a browse can find the same `deviceId` at a
    new address and offer it.
- **The download record (ADR-0083)** stays keyed by the connection. The binding
  adds which device the connection means.

## Class / Inv / N+1

- **Class "network driver"** (a way to find devices): broadcast, address list.
  Later: a remote gateway, a serial port. Near case outside: a stdio session.
  It is a spawned process, not found on a network.
  - Inv: a driver only produces identities. Sessions do not depend on how a
    device was found.
  - Check: a `Driver` trait with these two implementations, and the service
    tests run on both.
  - N+1: a new driver is one implementation plus its config row.
- **Class "client of the service":** VS Code, MCP, command line.
  - Inv: one command vocabulary, defined once (a Rust enum, as the session
    commands are). Clients only serialize it.
  - Check: an exhaustive match in the service, and a CLI test per command.
  - N+1: a new command is one variant plus its handler, and each client gets
    it.
- **Class "device identity field":** `deviceId`, the device panel fields, and
  the optional application fields.
  - Inv: the discovery reply and the session `identity` take these fields from
    one value, so the two never disagree.
  - Check: one function builds both, and a test compares them.

## Prefactoring

- **One port constant** in Rust, used by `--listen` as the default port. The
  TypeScript constant and the repeated literals in the docs come from it, or a
  test checks they are equal.
- Fix the stale lines in `engineering-connection.md`:
  - "Production code" in Out of Scope;
  - the cited line numbers;
  - `credentials` should be `credentialsKey`;
  - `pairId` is shown in hex, but the code uses decimal.

## Stages (each stage is its own PR or PRs)

1. Prefactor: the port constant, the doc fixes.
2. Device: persistent `deviceId` in identity; ADR "PLC discovery and target
   binding".
3. Device: the discovery wire crate and the responder on `--listen`; docs; a
   problem code for a reply that cannot be parsed, if needed.
4. Service: the `ironplclink` binary, the broadcast and address-list drivers,
   browse, the shell of commands, the CLI.
5. Service: sessions owned by the service, relaying the session protocol, and
   several clients on one device.
6. VS Code: the Network view, "Select as target", the binding check on connect,
   and connecting through the service.
7. MCP: browse and device tools through the service, replacing the in-process
   host where a device is given.

A tracking issue lists them when the plan is approved.

## Questions for the owner

- **Q1. Port.** One number for TCP session and UDP discovery, 49152 (as 44818 in
  EtherNet/IP)? Recommended: yes, configurable on both sides.
- **Q2. Device id.** A random id generated on first start and kept by the device
  (survives reboot; reset gives a new one)? Recommended: yes.
- **Q3. The shell.** Is "a thin shell for internal commands" the one command
  vocabulary of the workstation service, that the CLI (with an interactive
  mode), VS Code and MCP all use? Or did you mean a shell on the device itself?
  And the name `ironplclink`?
- **Q4. Where the chosen target is stored.** Options:
  - in the connection profile of the workspace (`settings.json`);
  - in a file of the project, as Set Project Path stores it in the project.

  Recommended: the profile. It is already per workspace, and VS Code and the
  service read it.
- **Q5. What a discovery reply shows.** Identity only, or also the application
  name and its hash (so the tree shows what runs, CODESYS-like)? It is not
  secret, but it is visible to anyone on the network. Recommended: include it,
  and allow `--discovery-minimal`.
- **Q6. Order.** Does this come before the online-model plan (login choices,
  Start/Stop) and before the parser S4? Recommended: yes, the online model
  builds on the bound target.

## Not included

- Wire authentication and encryption (ADR-0063 keeps it deferred). The binding
  check is identity, not security.
- Login choices, Start/Stop: the online-model plan.
- A gateway on another machine (a later driver).
- mDNS/DNS-SD: possible later as one more driver.
