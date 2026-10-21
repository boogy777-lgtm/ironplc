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

### 2. Device identity becomes unique; device settings

- `DeviceIdentity` gains `deviceId`. The device generates it on the first start
  and keeps it. A reboot keeps it. A new device, or a device that was reset,
  gets a new one.
- The soft PLC has no serial number, so the id is a random 128-bit value. A
  hardware target may report its serial instead.
- **Device settings (owner: the id can be changed in the device's own
  settings).** The device keeps its settings in one file that the device owns,
  beside its slots: `deviceId`, device name, and later the port and whether
  discovery is on.
  - The file can be edited offline.
  - A session command can change it online, through IronLink.
  - Class "device setting": every setting is one row with a type and a check,
    and both ways of changing it go through that row.
- The field is optional and additive, so the protocol version stays 1 (as
  checked in ADR-0083).

### 3. Discovery responder on the device

- With `--listen`, the device also answers discovery on UDP at the same port,
  only on the listen interface.
  - `--no-discovery` turns it off.
  - Replies are rate-limited per source.
  - The request is fixed-size. Anything else gets no reply and no error.
- **Reply: identity only.** It holds the protocol version, `deviceId`, device
  name, model and firmware, the session port, and whether the session is busy.
  - It has nothing about the application. The owner decided this: discovery
    and the comparison of the project with the PLC have different
    responsibilities. Whether the project and the PLC are equal is the internal
    comparison of the online model (download record, ADR-0083), shown once
    connected, as Schneider shows EQUAL.
  - Nothing secret.
- **Wire format:** one small crate (encode and decode, `no_std`-ready), shared
  by the device and the service. It has its own version byte and bounds checks.
  It has fuzz-style tests for truncated and oversized input.

### 4. IronLink, the communication service on the workstation (the RSLinx analog)

- **A separate binary, IronLink** (owner's name; the binary is `ironlink`). It
  runs on the workstation and listens on localhost only.
- **Its API is the command mechanism we already have.** The session protocol is
  JSON commands, framed on TCP (`runtime/src/commands.rs`, `vm-cli/src/tcp.rs`).
  IronLink speaks the same framing on localhost, with its own commands added.
- **Every GUI is a thin client of these commands:** the VS Code extension
  today, and a standalone GUI later. MCP and the command line are thin clients
  as well.
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
  - The command line uses it (`ironlink <command>`, and an interactive mode).
    VS Code and MCP use the same commands.
  - The session protocol stays as it is. The service relays it. It does not
    reinterpret it.

### 4a. What IronLink is responsible for (owner)

IronLink has three responsibilities:

- it finds and establishes the route;
- it discovers devices on the network;
- it organises the session.

Starting a device is not one of them. A soft PLC, local or remote, is started
on its own and is found like any device.

Today's stdio profile spawns `ironplcvm` from the editor. It stays in the IronPLC
extension, outside IronLink, until the owner decides whether to keep it.

### 4b. Where the IronLink UI lives in VS Code (question to the owner)

The owner considers a separate VS Code extension that works with the IronPLC
extension, unless an elegant native way exists. In both options the hard
boundary is the same: the `ironlink` binary and its command protocol. They are
what make the CLI, MCP and a future GUI thin. The question is only where the
VS Code part (the Network view, Choose Target, the service process) lives.

- **A. A separate extension "IronLink"** (`ironplc.ironlink`).
  - It owns the Network view and Choose Target, and starts and stops `ironlink`.
  - It exposes a typed API through the standard VS Code mechanism, the
    `exports` of `activate`. Microsoft's Git extension exposes its API the same
    way.
  - The IronPLC extension declares it in `extensionDependencies` and calls
    `getTarget()`, `openSession()` and `onDidChangeTarget`.
  - One extension pack installs both.
  - Cost: two packages, two versions, and the API version of the dependency.
    Other tools can use IronLink without the compiler.
- **B. Native, in the IronPLC extension, with an enforced boundary.**
  - The module `src/ironlink/` exports only one API interface, the same shape
    as in A.
  - A check fails the build if any other file imports anything else from it.
  - Moving it to its own extension later is a move of the folder, without any
    change to the API.
  - Cost: one package. The separation holds only through the check.
- **Recommendation: B now, with the API shaped as A's `exports`.** The boundary
  is enforced from the first day, and splitting is cheap when a second consumer
  appears. Choose A if IronLink must be installable without the compiler from
  the start.

### 5. Target selection and binding

- **Browse view in VS Code** ("IronPLC Network", the RSWho analog): it shows
  the tree from the service.
- **"Select as target" writes the target to two places.** Each holds the
  `deviceId` and the path (driver and address).
  - **A project file**, so a project copied to another PC keeps its target, as
    Set Project Path keeps it in the project. It is versioned JSON in the
    `RecordKind` family; the name is chosen in stage 6.
  - **The connection profile** in `settings.json`, for this workstation.
- **The two places are independent** (owner). Either one can be overwritten
  without the other. When both exist and differ, connect shows both and asks
  which to use and which to overwrite. It never chooses silently.
- **Connect checks the device:**
  - it goes to the stored address;
  - it compares `deviceId` from `identity`;
  - a different id is a new problem code: no session, and the option to
    re-select;
  - when the address is not answered, a browse can find the same `deviceId` at a
    new address and offer it.
- **The download record (ADR-0083)** stays keyed by the connection. The binding
  adds which device the connection means.

### 6. GUI skeleton in VS Code

The skeleton follows TwinCAT XAE and SIMATIC AX; the function follows RSLinx.

**References** (research 2026-10-21):

- **TwinCAT XAE** (Infosys pages read directly):
  - "Choose Target System" lists the targets that have a route, and has a
    Search button.
  - "Add Route" has a field "Enter Host Name / IP", plus "Broadcast Search" and
    "Refresh Status".
  - The search result has the columns Host Name, Connected, Address, AMS NetId,
    TwinCAT version and OS version.
  - Routes are of three kinds: Project, Static, Temporary.
  - The chosen target is shown in the menu bar. The mode is shown as Config, Run
    or Stop.
  - On Login with a different program, the choices are Login with Online
    Change, Login with download and Login without changes.
- **SIMATIC AX:**
  - It is built on VS Code ("AX Code"). Most of its documentation is behind
    early access.
  - The confirmed parts: the target IP is an `ipAddress` input of a task
    (`simatic-ax/task-collection`), and SIMATIC AX 2510 adds a "PLC Online Hub"
    for connections.
  - No network browse was found.

**The IronPLC elements:**

- **View "IronPLC Network"** in the IronPLC sidebar. It is a TreeView of text
  rows, as RSWho is:
  - the workstation;
  - one row per driver: a network interface (broadcast) or the address list;
  - the devices under each driver. A device row shows the device name as its
    label. Its description shows the address, a short `deviceId`, and either
    "busy" or "target".
- **Actions:**
  - On the view title: "Search network" (refresh, as Broadcast Search) and
    "Add address…" (as Enter Host Name / IP).
  - On a device: "Select as target", "Connect", "Disconnect", "Identify",
    "Device settings…" and "Copy device id".
  - On the address-list driver: "Add address…" and "Remove address".
- **Command "IronPLC: Choose Target…"** (as Choose Target System). It is a
  QuickPick of the known targets: those of the project file, of the profile, and
  those recently seen. Its last item, "Search network…", opens the view.
- **The two places where a target is stored** correspond to the TwinCAT route
  kinds: the project file is the Project route, and the connection profile is
  the Static route of this workstation.
- **Status bar** (as the target in the XAE menu bar): the target name and the
  connection state. The online model adds EQUAL or DIFFERENT and Run or Stop
  later, in its own plan. The login choices also wait for that plan.
- **The existing view "IronPLC Device"** stays. It shows the identity of the
  connected device and its application.
- **Command category** "IronPLC: Network" for the new commands.

### 7. RSLinx capabilities and the next members (N+1)

Survey of RSLinx Classic and FactoryTalk Linx (2026-10-21; Rockwell help and
literature, part from snippets; the article the owner gave is secondary).

**Members of this plan's classes** (each is one more member, no redesign):

| RSLinx | IronLink | When |
|---|---|---|
| EtherNet/IP driver: broadcast, device list, Browse Remote Subnet | drivers "broadcast" and "address list" | this plan |
| Virtual Backplane / SoftLogix (a soft PLC on the PC) | not a driver: a soft PLC on the PC is one more device (owner). The device does not know where it runs: Windows or Linux desktop, or embedded. The loopback interface is one more interface of the drivers. | this plan |
| RSWho Autobrowse / Refresh, Device Properties | `browse`, `identify` | this plan |
| SDK for third-party applications | the command protocol of IronLink | this plan |
| DF1 serial, emulators | one more driver each | later |
| Time synchronisation | a device setting + a command | later |

**Classes named now, not built** (they fit none of the plan's classes; naming
them keeps the design from closing them):

| Class | RSLinx examples | Rule now | When |
|---|---|---|---|
| Named path | DDE/OPC topic, FT Linx shortcut, alias topic | the connection profile name is our named path; it never goes into device identity | profile = now; aliases later |
| Route | CIP path through backplanes and networks, alternate path | the stored path is separate from `deviceId`; one hop today; a multi-hop route is a new version of the path format | later |
| Data access and subscription | Data Monitor, tag read/write, unsolicited messages | IronLink owns the sessions, so a subscription can fan out to several clients; the relay must not assume request/response forever (ADR-0063 has no push in v1) | next plans (owner's thin-client list: variables read/write/force) |
| Device operation | ControlFLASH, set IP over USB, EDS upload | long, exclusive, cancellable, with progress; `acceptEdits` is the first one | later |
| Diagnostic counters and events | driver counters, port statistics, Syslog | read-only, resettable; not device settings | later |
| Credential and permission | FactoryTalk Security, CIP Security, client access list | secrets never in the project file or device settings; wire auth stays deferred (ADR-0063) | later |
| Protocol adapter | OPC DA/UA, MQTT, DDE | a server that re-exposes data; a different kind of client | later (OPC UA, MQTT); DDE never |
| Device description registry | EDS files, installation tool | each device type declares itself | later |
| Remote service | RSLinx Gateway, Remote Proxy | IronLink listens on localhost; the transport stays behind one interface | later |

Not taken: editions, activation and tag caps (licensing), and drivers for
legacy Rockwell hardware (DH+, DH-485, ControlNet, DeviceNet).

## Class / Inv / N+1

- **Class "network driver"** (a way to reach devices): broadcast, address list.
  Later: a serial port, a remote gateway.
  - Near case outside: the HA pair link. It joins two units of one pair, not
    the engineer and a device.
  - A soft PLC on the same PC is not a driver. It is a device reached through
    the loopback interface, as any device (owner).
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
2. Device: the device settings file (`deviceId`, name), `deviceId` in identity,
   and the session command that changes a setting; ADR "PLC discovery and target
   binding".
3. Device: the discovery wire crate and the responder on `--listen`; docs; a
   problem code for a reply that cannot be parsed, if needed.
4. Service: the `ironlink` binary, the broadcast and address-list drivers,
   browse, the shell of commands, the CLI.
5. Service: sessions owned by the service, relaying the session protocol, and
   several clients on one device.
6. VS Code: the Network view, "Select as target" (project file + profile), the
   binding check on connect, and connecting through the service. The network
   tree is text rows (RSLinx function). The skeleton of views, commands and
   state follows SIMATIC AX and TwinCAT XAE.
7. MCP: browse and device tools through the service, replacing the in-process
   host where a device is given.

A tracking issue lists them when the plan is approved.

## Owner's answers (2026-10-21)

- **Q1. Port:** yes. 49152 is used for the TCP session and for UDP discovery,
  and it is configurable.
- **Q2. Device id:** yes. In addition, it can be changed in the device's own
  settings (section 2).
- **Q3. The shell.** A GUI is a thin client of the commands. It uses the
  command API, which is the session protocol (section 4). The name is
  **IronLink**.
- **Q4. Where the target is stored:** both in a project file and in the
  connection profile. They are two independent places, and either can be
  overwritten. A project copied to another PC keeps its path (section 5).
- **Q5. The reply has no hash.** Discovery is identity. Whether the project and
  the PLC are equal is the internal comparison of the online model, shown as
  Schneider shows EQUAL (section 3).
- **Q6. Order:** no answer yet. The working assumption is that this comes before
  the online model and S4.
- **Plan approved** (owner, 2026-10-21). The owner also said:
  - a local soft PLC is one more device, not a driver (section 7);
  - IronLink finds and establishes the route, discovers devices and organises
    the session (section 4a);
  - a separate VS Code extension is considered (section 4b, open).
- **Reference for the GUI ontology (owner): Beckhoff's TwinCAT extension for
  VS Code.** Its views, commands and settings are studied for their names and
  structure only. Search result (2026-10-21):
  - Beckhoff publishes no VS Code extension, on the Marketplace or on Open VSX.
    TwinCAT 3 Engineering is Visual Studio based (XAE, and the XAE Shell on VS
    2022), and so is its Target Browser.
  - Two community extensions were inventoried: "Explorer for TwinCAT" (MIT) and
    "TwinCAT Structured Text" (GPL-3.0). They cover the project tree, editing
    and git only. They have no target, route, online state or device commands.
  - **Owner's choice:** the GUI skeleton follows Siemens SIMATIC AX (built on VS
    Code) and TwinCAT XAE. They show how to lay out views, commands and state.
    The function follows RSLinx: a device tree of the network.
  - **The tree is text rows** in a VS Code TreeView: workstation, then network,
    then device. No graphical boxes.

## Not included

- Wire authentication and encryption (ADR-0063 keeps it deferred). The binding
  check is identity, not security.
- Login choices, Start/Stop: the online-model plan.
- A gateway on another machine (a later driver).
- mDNS/DNS-SD: possible later as one more driver.
