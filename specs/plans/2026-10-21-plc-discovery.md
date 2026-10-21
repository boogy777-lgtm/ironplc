# PLC Discovery and Controller Selection

## Goal

The engineer finds the PLCs on the network, picks the one to connect to, and
the project remembers the path as a convenience. The project, which is its
sources, stays the master. This is what RSLinx (Rockwell) and the gateway
with "Scan network" (CODESYS) do.

- **Discovery.** A PLC answers a "who is there" request on its own port, with
  identity only.
- **Browse.** A tree with workstation, then network (an interface or a list of
  addresses), then device, as RSWho shows it.
- **Controller selection.** The engineer chooses any controller. The path
  (`deviceId` and route) is remembered. Connect shows what the controller holds
  against the project (section 8), and never refuses a controller for being a
  different one.
- **A communication service on the workstation**, the RSLinx part. It finds
  controllers and routes. Whether it also carries the sessions is the open
  question of section 4a. VS Code, MCP and the command line are thin clients of
  its commands.

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

### 0. Ontology (owner's audit and answers, 2026-10-21)

**The project is the master.** The project is its sources. A controller is any
controller the engineer chooses to connect to; nothing binds a project to one
device.

**The engineering questions.** Every function answers one of these.

1. Which controllers can this workstation see, and how does it reach each?
   (IronLink)
2. Which controller does the engineer connect to now? The engineer chooses.
3. Which controller answered, and what does it hold compared with the project?
   (the go-online comparison, section 8)
4. Can a session be opened, and if not, why not? This is the answer to the
   connect attempt: unreachable, busy, protected.
5. Once connected, which operations are allowed, and on what grounds? (the
   truth table, section 8)

A session alone says nothing about equality of the program, the run state, or
permission to act on the process.

**The entities, and what each must not be mixed with:**

| Entity | Meaning | Not to be mixed with |
|---|---|---|
| Project | the master: the sources; the build makes the application container | a controller, or what a controller holds |
| Device | one physical or soft controller instance | an IP address, a name, a tree row |
| Device id (`deviceId`) | how an instance is recognised again (in the comparison, and as the key of the download record) | the name; the address; authenticity (trust is a separate, later class) |
| Address | one possible access point | the identity of the device |
| Route | how this workstation reaches the device (driver, interface, address) | the device itself |
| Observation | what a discovery or an identify request returned, with its time | a promise that the device is reachable now |
| Connection path (remembered) | the controller the engineer last chose, with its route, remembered for convenience (as Set Project Path) | an intent or a binding of the project; an open session |
| Session | the current interaction of this workstation with a device | the remembered connection path |

**Three separate actions.**

- **Choose a controller** remembers a connection path.
- **Find a route** produces observations and routes.
- **Connect** opens a session and shows the go-online comparison.

**Markers in the tree, from observations only.**

- Reachability, with the age of the observation.
- Protected or open: whether the controller requires credentials (section 3).
- The "last chosen" mark.

Busy is not a marker. It is one answer to a connect attempt (V6014).

**Scope: one remembered path per application.** A project with several
controllers (DCS) has several applications, and so several paths.

### 0a. Thin clients, as a checked property

Every decision is made in one place, and the clients render it.

- **Discovery and route decisions are IronLink's:**
  - which devices were observed;
  - which routes exist;
  - which identity fields are public.
- **Online decisions are IronPLC's.** They are implemented once, in Rust, and
  every client calls them:
  - the go-online comparison;
  - the allowed operations (the truth table, section 8);
  - the refusal reasons.
- No client computes enablement on its own rule.

- **Check:** a conformance test suite runs against the IronLink command API.
  The VS Code layer has no decision logic: it maps answers to widgets, and a
  test fails if it computes enablement on its own.
- **N+1:** a new discovery method (a new driver) adds observations and routes
  only. The meaning of the comparison and of the session does not change.

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
  - A session command can change the name online. IronPLC sends it in its
    session; IronLink never writes to a controller (owner).
  - The **device id is changed only on the device itself**: in its settings
    file, or later in the device's own local UI. It is never changed over the
    engineering session (audit 2026-10-21, confirmed by the owner).
    - The id is how a workstation recognises a controller again, and it is the
      key of the download record. A session must not rewrite what the
      engineering side uses to recognise the device it talks to.
    - The scenarios that need a change are local: replacing a device by giving
      the replacement the id of the one it replaces, and resolving a duplicate
      after a cloned image.
    - The format check (32 hex digits) proves the form only. It proves neither
      uniqueness nor the right to take an identity.
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
- **Reply: public identity only.** It holds the protocol version, `deviceId`,
  device name, model and runtime version, the session port, and whether the device is
  protected. It does not say whether the session is busy: busy is the answer to
  a connect attempt.
- **What may be shown, and to whom** (owner: separate what is not confidential
  from what must be hidden):

  | Data | Discovery (anyone on the network) | Session (after connect; credentials when protected) | Never on the wire |
  |---|---|---|---|
  | device id, name, model, protocol, session port, protected flag | yes | yes | |
  | runtime version | yes, protected device too, as Rockwell does (ListIdentity gives the revision to anyone, even for a secured controller, and has no setting to hide it). Protection is the interfaces discovery answers on, `--no-discovery`, and network segmentation (owner, 2026-10-21) | yes | |
  | application identity (name, contentHash, layoutHash, generations) | no | yes | |
  | pending edit record, run state, device settings | no | yes | |
  | credentials, keys | | | yes |
  | sources | | | yes (the container holds none) |

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
  runs on the workstation as a child process of each client, on stdio (owner,
  2026-10-21; section 4b). It opens no listening port.
- **Its API is the command mechanism we already have.** The session protocol is
  JSON commands, framed on TCP (`runtime/src/commands.rs`, `vm-cli/src/tcp.rs`).
  IronLink speaks the same JSON commands on stdio, with its own commands added.
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

### 4a. Who connects: the fundamental question (owner, 2026-10-21)

Two options for IronLink's responsibility:

- **A. IronLink finds and records.** It finds controllers and routes, and writes
  the chosen path into the config (the project file and the workstation record).
  - Connecting, and every conclusion after it, belong to IronPLC.
  - The extension connects to the device directly over the session protocol, as
    `ConnectionManager` does today.
- **B. IronLink carries the session.** IronLink is also the transport: the
  extension connects through IronLink, which relays the session.
  - The conclusions still belong to IronPLC. RSLinx does the same: it provides
    the path and the transport, and Studio 5000 does the go-online comparison
    and the Upload / Download / Online choice.

**In both options the conclusions belong to IronPLC:** the comparison, the
allowed actions and hot edit. They are implemented once, in Rust, so that VS
Code, MCP and the CLI render the same answers. The options differ only in the
transport.

| | A. Find and record | B. Carry the session |
|---|---|---|
| IronLink must run to work online | no (only to search) | yes |
| VS Code and MCP on one controller at the same time | no: one session per device (ADR-0065); the second gets V6014 | yes, through one session |
| A driver that is not plain TCP (serial, a gateway) | needs B for that driver | yes |
| New code | discovery + config writer | + relay, session multiplexing, a lifecycle |
| Today's `ConnectionManager` | stays | moves behind IronLink |

**Owner's decision (2026-10-21): A.**

- IronLink finds controllers and routes, and writes the data into the config
  (the project path file and the workstation route record).
  - It never writes to a controller.
  - Connecting, and every conclusion after it, belong to IronPLC.
- **Build in maximum modularity from the start.** There are two classes, each
  with its own trait/interface, and each member registers itself:
  - **Connection method** (how IronPLC talks to a controller). Today: stdio (a
    spawned local `ironplcvm`) and TCP. These are two real members, so the
    class is confirmed. Later: USB. A new method is one implementation of the
    transport interface plus its config row; the session protocol on top does
    not change. TypeScript already has `HotEditTransport` with the stdio and
    TCP implementations; Rust (MCP, CLI) gets the same trait.
  - **Discovery driver** (how IronLink finds controllers). Today: broadcast and
    address list. Later: USB enumeration. A driver produces observations and
    routes; each route names the connection method that reaches it.
- B (IronLink carries the session) stays a named class, not built.

- It is named now ("transport relay", section 7) and not built (ConfirmedClass:
  no second real case yet).
- IronLink's responsibility is then exactly the owner's first wording: find
  controllers and write the data into the config.

The earlier text below (4a original) holds for discovery and routes only.

### 4a-bis. What IronLink is responsible for (owner)

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
VS Code part (the Network view, Choose Controller, the service process) lives.

- **A. A separate extension "IronLink"** (`ironplc.ironlink`).
  - It owns the Network view and Choose Controller, and starts and stops
    `ironlink`.
  - It exposes a typed API through the standard VS Code mechanism, the
    `exports` of `activate`. Microsoft's Git extension exposes its API the same
    way.
  - The IronPLC extension declares it in `extensionDependencies` and calls
    `getConnectionPath()`, `chooseController()` and `onDidChangePath`.
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
- **Owner's choice (2026-10-21): A**, a separate extension.
  - The extension is a thin client of the `ironlink` binary, in the same way as
    the IronPLC extension is a thin client of the language server (`ironplcc
    lsp`) and of the debug adapter (`ironplcvm`). The binary decides; the
    extension renders a TreeView, runs commands and shows a log channel.
  - **Decided (owner, 2026-10-21): stdio.** Under option A IronLink holds no
    sessions, so it is a child process on stdio, started by each client as the
    language server is. No daemon, no port, no lifecycle. A shared service on
    localhost is a second member of the same class, built only if a real case
    needs it.

### 5. Choosing a controller and connecting

- **Browse view in VS Code** ("IronPLC Network", the RSWho analog): it shows
  the tree from the service.
- **The remembered connection path is stored in two places** (owner: both, each
  independent, each can be overwritten). Neither is an intent or a binding.
  - **The project file** `<app>.path.iplcinfo`, beside `<app>.iplc` (name
    confirmed by the owner). It holds the `deviceId` of the controller last chosen, and
    a route hint, the last address that is not loopback. It travels with the
    project, so a project moved to another PC keeps its last path, as Set
    Project Path does.
  - **The workstation route record** (this PC). It holds the route this
    workstation uses for a `deviceId`. A loopback address is stored only here.
- **Connect never refuses because the controller is a different one.** It opens
  the session and shows the go-online comparison (section 8):
  - what the controller is;
  - what it holds;
  - what the project has;
  - which actions follow.

  When the controller is not the one this project last connected to, the
  comparison says so as an information row (Rockwell shows the serial number in
  the same place).
- **Answers to the connect attempt:**
  - unreachable: offer a search;
  - busy (V6014);
  - protected: credentials needed. This is the later credential class; until it
    exists a protected controller cannot be connected.
- **Same `deviceId` seen at several addresses:** when the identity fields are
  equal, it is one device with several routes. When they differ, it is an
  identity conflict, which is marked in the tree and in the comparison.
- **Identify** sends a fresh request to that route and shows the reply with its
  time. A hover shows the last observation and its age.
- **The download record (ADR-0083)** is keyed by `deviceId` in stage 6
  (`<app>.<deviceId>.download.iplcinfo`): what this project last put on this
  controller. It is one input of the comparison.

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
    label. Its description shows the address, a short `deviceId` and the
    markers (section 0): last chosen, reachability with the age of the
    observation, and protected. A row stands for one route to a device: one
    device seen through two drivers has two rows, and both show the same
    markers.
- **Actions:**
  - On the view title: "Search network" (refresh, as Broadcast Search) and
    "Add address…" (as Enter Host Name / IP).
  - On a device: "Choose", "Connect", "Identify" (fresh request) and "Copy
    device id". Enablement comes from the one decision place (section 0a).
  - "Disconnect" and "Rename device…" act in a session, so they live in the
    IronPLC Device view, not in IronLink's tree. "Rename device…" is the one
    setting changeable online; the device id is not.
  - On the address-list driver: "Add address…" and "Remove address".
- **Command "IronPLC: Choose Controller…"** (as Choose Target System). It is a
  QuickPick of controllers for the application's remembered path. It has three
  groups:
  - the last chosen controller;
  - the devices with a route on this workstation;
  - the devices recently observed, with the age of the observation.

  Its last item, "Search network…", opens the view.
- **The two stores correspond to the TwinCAT route kinds:**
  - the project path file (the last chosen `deviceId` plus a portable hint) is
    the Project route;
  - the workstation route record is the Static route.
- **Status bar** (as the target in the XAE menu bar). It shows the chosen
  controller and the session state: none, connecting, or online with it.
  - EQUAL/DIFFERENT and Run/Stop appear only when the online model provides
    them, never as placeholders.
  - The login choices also wait for that plan.
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
| Remote service | RSLinx Gateway, Remote Proxy | IronLink runs on stdio; a service on localhost or on another machine is one more member behind the same interface | later |

Not taken: editions, activation and tag caps (licensing), and drivers for
legacy Rockwell hardware (DH+, DH-485, ControlNet, DeviceNet).

### 8. Go online: the truth table (input to the online-model plan)

Owner: when connecting, the engineer sees what is in the controller and what is
in the project, and so which actions are possible online (Rockwell "Connected
To Go Online"; CODESYS login choices).

**Rules that hold in every row:**

- Hot edit (online change) exists only inside an online session.
- Edits made offline are ordinary changes of the sources.

**Inputs at connect:**

- **A, the answer to the attempt:** ok, unreachable, busy, or protected.
- **B, the controller's application:** none, or present (`C` = its
  `contentHash`, `Lc` = its `layoutHash`).
- **P, the project's build:** up to date (`P` = `contentHash` of `<app>.iplc`,
  `Lp`), sources changed since the build, or the build fails.
- **R, the download record:** `R` = the `contentHash` this project last put on
  this `deviceId` (ADR-0083), or none.
- **E, the pending edit on the controller:** none, or staged/under test (the
  pending-edit record).

**The table:**

| # | Situation | Controller shows | Project shows | Allowed actions | Hot edit |
|---|---|---|---|---|---|
| 1 | A unreachable | — | — | search network, choose another controller | — |
| 2 | A busy (V6014) | "serves another engineering session" | — | retry, cancel | — |
| 3 | A protected | "protected" | — | enter credentials (later class); until then none | — |
| 4 | B none: a new device, a factory reset, or an application lost | "no application" | P, or "not built", or errors | Download (when P exists; else Build first); device-level connection | no: there is nothing to change |
| 5 | C = P, P up to date. This includes disconnect then connect with nothing changed | EQUAL | EQUAL | online at once, no dialog: monitor; hot edit on later edits | yes |
| 6 | C = P, sources changed since the build (offline edits started) | = last build | "modified since build" | as row 7. "Login with online change" builds first (as CODESYS asks "generate code now?") and then is row 7 or 8 | after the build |
| 7 | C = R ≠ P, P up to date, Lc = Lp | "previous download from this project" | "changed since that download" | Login without change (with a warning that the project differs); Login with online change (the difference is loaded; it may change the process unpredictably, so it is the engineer's responsibility and is confirmed explicitly); Login with download; Upload into a new project (once the source archive exists) | yes |
| 8 | C = R ≠ P, Lc ≠ Lp (persistent part changed) | as 7 | as 7 | online change with migration decisions (ADR-0061, V4010), or Download; V4020 (persistent variable without a UID) leaves Download only | with migration |
| 9 | C ≠ P and C ≠ R: unknown to this project (another project, engineer or workstation) | DIFFERENT, with its application identity | P | Download (overwrite, with a warning); Upload into a new project (once the source archive exists); device-level connection; cancel. No online change: the base is unknown | no |
| 10 | E staged/under test (this or another session, or after a crash) | the pending edit record | — | resolve it first: test, untest, assemble or cancel (ADR-0064/0065); a new change is blocked until then | after it is resolved |
| 11 | P build fails | as B | errors | monitor only if C = the container still on disk; otherwise device-level; no download, no change | no |
| 12 | `deviceId` differs from the remembered path | information row "not the controller this project last connected to (id X, was Y)" | — | the rows above apply to this controller (R is looked up by its id) | per row |
| 13 | connection lost while online | state "unknown" (never the last value) | — | reconnect, then the rows again | — |
| 14 | the controller restarted while connected; its application was kept by the slots | — | — | reconnect, then row 5 or 7. The comparison is by content, so restarted counters do not matter | per row |
| 15 | the runtime rejects the container format (`container/src/header.rs`, `FORMAT_VERSION`); there is no firmware or update flow (owner) | its runtime version | the format the container has | the message names both versions; no download | no |

**Rockwell, row by row** (research 2026-10-21). Most of it comes from search
snippets of Studio 5000 help pages that could not be read in full. Dialog texts
and button labels are not confirmed.

| Row | Rockwell (Studio 5000 / Logix) | IronPLC |
|---|---|---|
| 4 | cannot go online without a project; Download is required | the same, plus an optional device-level connection (settings, rename) |
| 5 | goes online with no dialog ("correlate"); online changes made by others since are uploaded first | goes online with no dialog. Uploading changes made by others needs the source archive (to-do) |
| 6–7 | offline edits are **not** carried online: Upload (the controller's version replaces the project), Select File, or Download (overwrite) | the **owner chose CODESYS here**: Login with online change loads the difference, on the engineer's responsibility; Upload and Download remain |
| 9 | different project: Upload into a new file (Select File); comments and descriptions are lost, since only logic, configuration and data are in the controller | Upload into a new project from the source archive (to-do). The archive keeps comments, which is more than Rockwell keeps |
| 10 | Accept / Test / Assemble / Cancel pending edits, Finalize All Edits; nothing documented for another workstation or a crash | the same vocabulary (ADR-0064/0065) |
| 12 | "Match Project to Controller" (Controller Properties, Advanced): opt-in; the project stores the controller serial; on a mismatch the transfer is cancelled, and the engineer either connects to the right controller or updates the serial in the project | default: an information row (owner: any controller). **N+1, later:** the same opt-in per application, "match the application to controller id X" |
| 13 | the application goes offline; operations in progress do not complete; Go Online again re-correlates | the same, with "unknown" in place of the last state |
| 15 | a firmware mismatch prompts for a firmware update (firmware kit, ControlFLASH Plus) | no firmware concept: the message names both versions (row 15) |
| — | Change Detection: the controller keeps an Audit Value that changes on forces, mode changes, downloads, firmware updates, and so on; it is blank offline | **N+1, later:** a "controller change detection" class, which tells the engineer that the controller changed since they were last online |
| — | discovery (ListIdentity) gives the revision to anyone on the network, even for a secured controller; there is no setting to hide it; protection is network segmentation and CIP Security on sessions | the same: the runtime version is public (owner, section 3) |

**Gaps that the table shows (for the online-model plan):**

- **Monitoring a previous download (rows 7, 9 and 11) needs that container's
  debug information.** Only the latest `<app>.iplc` is kept, so it is not
  there. CODESYS keeps the downloaded set (`.compileinfo`) for this.
  - Proposal: keep a copy of the downloaded container per `deviceId`, beside
    its download record.
- **A device without an application (row 4)** is a new device state.
  `ironplcvm serve` needs a container today. The online-model plan adds it:
  serving with no application, and Download as the first load.
- **Upload is impossible today**, because a container holds no sources.
  - **Owner's to-do.** The controller also receives the project sources as an
    archive with maximum compression.
  - Upload then reads the archive back and **creates a new project** from it.
  - This needs a project-creation mechanism with templates, as CODESYS has.
    Unpacking an uploaded archive is one more way (N+1) of creating a project.
  - This is its own plan, after the online model.
- Rows 6 and 7 are one situation, before and after the build. The owner's
  choices for it (a warning, loading the difference on the engineer's
  responsibility, or uploading into a new project) are all in row 7.

### 9. Audit: unfinished edits in the controller and offline changes (2026-10-21)

**The owner's rule.** Offline edits go through the standard build and load
mechanism, exactly as a hot edit does:

- build, then `acceptEdits`, `testEdits` and `assembleEdits`;
- the new code is injected at the next scan boundary;
- a minimum of new code, and no workarounds.

**The proposal that follows from the rule.** A full download is the same
pipeline in a "replace" mode.

- At the boundary the application is replaced with a re-initialisation. The
  machinery is the migration swap (fresh buffers, init runs) with an empty
  "keep" set.
- A schedule, I/O or flag change (V4008, V4009, V4007) then has a path. Today
  it has none.
- "No application" is a built-in empty container: no ST files, so no
  programs and no tasks. A device without an application needs no new state in
  the host's state machine.
- **Approved by the owner (2026-10-21)**, both points.

Facts below are from a sweep of `main` 3e5a601ed, with the key lines verified
by hand (`runtime/src/host.rs`, `vm-cli/src/serve.rs`, `vm-cli/src/slot_store.rs`,
`integrations/vscode/src/buildCommands.ts`, `buildLogic.ts`).

| # | Situation | Today | Required | Kind |
|---|---|---|---|---|
| 1 | reconnect, nothing changed | content compare (ADR-0083) → match | go online at once (row 5) | ok |
| 2 | offline edits, same layout | Build & Commit: accept → test → assemble, swap at the boundary | the same; this is the owner's rule | ok |
| 3 | offline edits add or rename persistent variables | needs `sync-uids` by hand, else V4020 at accept; VS Code does no pre-check (only MCP does) | the pre-check in the one decision place; the UID sync is a step of the flow | gap |
| 4 | offline edits change the schedule, I/O sizes or flags (V4008, V4009, V4007) | **no deploy path at all**: no full download, and restarting `serve` does not help | Download = the "replace" mode of the same pipeline | gap |
| 5 | a candidate staged (not tested) by an earlier TCP session | the next build fails with V4013, which does not name the edit; no exit in the UI | row 10 of the comparison shows the record and its exits | gap |
| 6 | a candidate under Test, and the TCP session drops | the candidate keeps running under Test with nobody attached, without limit | **owner (2026-10-21):** the responsibility lies with the side where the test runs, which is the controller. The candidate stays under Test (as Rockwell keeps test edits); losing the session changes nothing; the next connect shows it in row 10 with its exits | decided |
| 7 | "Cancel" in the Trial dialog while testing | **always refused (V4015)**: `cancel()` refuses while the candidate is active (`host.rs`), and `resolveTrial` sends cancel directly | Cancel from Testing = untest, then cancel at the boundary | defect |
| 8 | a migration candidate under Test | no untest (V4011); the dialog still offers Cancel, which cannot work; the only exits are Assemble or a device restart | keep the old buffers so that untest is possible, or say plainly that the only exit is Assemble | gap |
| 9 | Esc in the Trial dialog | the candidate stays under Test; the code comment says "exits stay available on the next build", but the next build fails with V4013 | the exits come from the comparison (row 10) | defect |
| 10 | Build & Commit fails halfway (test or assemble refused, link lost) | no cleanup: the candidate stays staged or under Test | every failure ends in a state whose exits are shown | defect |
| 11 | device restart or crash while staged or testing | the candidate and its record are lost (RAM only); the committed application boots with init, so **all variable state is lost: there is no retained store** | a retained/persistent store across restarts (IEC RETAIN); candidate loss is reported | gap (fundamental) |
| 12 | V6012 at assemble: promoted in RAM, but the slot was not written | the device runs the new code; after a restart the old code returns silently | the device reports both the running and the persisted `contentHash`; the comparison shows "not persisted" | gap |
| 13 | a rebuilt `.iplc`, and `serve app.iplc` restarted | the slot store wins, so the old application runs and the file is ignored | correct for a device, since the file is only the first seed; the extension must stop expecting "recompile and respawn loads new code", and show the comparison, then online change | gap (client) |
| 14 | another workstation changed the controller (no local record) | "no record = no check", so the build proceeds (fail-open) | row 9 of the comparison: unknown application, no online change | closed by the new model |
| 15 | E0015 mismatch with no remedy | the build is refused; the remedy command does not exist | the comparison replaces the refusal | closed by the new model |
| 16 | the old "IronPLC Hot Edit" commands | bypass the checks, write no record, and can start a second device on the same file | remove them | ΔR<0 |
| 17 | a panic in the TCP session thread | the occupied flag is never released, so V6014 is answered until restart | a drop guard | defect |
| 18 | **`serve` does not run the scan loop** | the scan mechanism exists (the task scheduler in the VM, `run_round(uptime)`, `next_due_us()`), and `ironplcvm run` drives it cyclically (`vm-cli/src/cli.rs`). `ironplcvm serve` does not: it runs one round at clock 0 only after `testEdits`, `untestEdits` or `assembleEdits` (`drive_scan_round`), so between commands the device does not execute | `serve` drives the same loop as `run` (one shared loop, not a copy), and handles session commands between rounds; the swap stays at the round boundary. This comes before Run/Stop | gap (fundamental) |
| 19 | HA pair | the peer drops the migration decisions; a Test lost to permit revocation is still acknowledged | the HA plan | gap |
| 20 | a container larger than about 5 MB | `acceptEdits` sends JSON bytes, about 3.3 times larger, and the frame cap is 16 MiB | binary framing or chunks | later |
| 21 | docs and code disagree | `serve.rs` says assemble "requests a swap" (it is immediate); MCP says assemble works from staged (it does not); "needs Test" is in fact "active now" | fix the comments | defect (docs) |

**The classes found** (for the online-model plan):

- **K1, the exits of an unfinished edit (rows 5–10).**
  - Inv: for every host state, the runtime gives the set of allowed commands
    (one function), and the clients render it.
  - Check: a test that walks every state.
- **K2, the deploy modes (rows 2–4, and rows 4 and 9 of the table in section
  8):** online change, migration, and replace. They are one pipeline, swap at
  the boundary.
- **K3, device durability (rows 11 and 12):** the retained/persistent store,
  and the running versus persisted identity.
- **K4, cyclic execution of the device (row 18).** This comes before Run/Stop.
- **Defects** (rows 7, 9, 10, 17 and 21, and the removal in row 16): fixed
  inside K1, not as separate patches (owner, 2026-10-21).
- **Order in the online-model plan (owner, 2026-10-21):** K3 and K4 come
  first.

## Class / Inv / N+1

- **Class "connection method"** (how IronPLC talks to a controller): stdio and
  TCP today, which are two real members; USB later.
  - Near case outside: a discovery driver. It finds a controller and does not
    carry a session.
  - Inv: every method implements one transport interface (open, send a frame,
    receive a frame, close). The session protocol above it is the same.
  - Check: the session tests run once per registered method. A method that is
    not registered fails the completeness test.
  - N+1: USB is one implementation plus its registration and its config row.
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
   and the session command that changes a setting; ADR "PLC discovery" (its
   text follows the owner's answers of 2026-10-21: no binding).
3. Device: the discovery wire crate and the responder on `--listen`; docs; a
   problem code for a reply that cannot be parsed, if needed. The identity field
   `firmwareVersion` (a fixed `"0.13.0"` today, `runtime/src/commands.rs`)
   becomes `runtimeVersion`, taken from the build, since IronPLC has no firmware
   concept.
4. Service: the `ironlink` binary, the broadcast and address-list drivers,
   browse, the shell of commands, the CLI.
5. Connection methods as a class: one transport interface in Rust and in
   TypeScript, with the stdio and TCP members registering themselves; MCP
   gains the TCP method (option A: IronPLC connects directly).
6. VS Code: the Network view, "Choose" (the project path file and the
   workstation route record; the download record keyed by `deviceId`), and
   connect directly (option A). The network tree
   is text rows (RSLinx function). The skeleton of views, commands and state
   follows SIMATIC AX and TwinCAT XAE. The go-online comparison and its actions
   are the online-model plan (section 8).
7. MCP: browse through IronLink; device tools connect the same way as VS Code,
   replacing the in-process host where a device is given.

Tracking issue: #149.

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
  - The audit of 2026-10-21 gives the two places different meanings: the
    project holds the intent (`deviceId`) and a route hint, and the workstation
    holds the route, keyed by `deviceId`.
  - The places stay independent and overwritable. They can no longer disagree
    about which device.
  - To be confirmed by the owner.
- **Q5. The reply has no hash.** Discovery is identity. Whether the project and
  the PLC are equal is the internal comparison of the online model, shown as
  Schneider shows EQUAL (section 3).
- **Q6. Order:** communication first (this plan), then the online model, then
  the parser (S4).
- **Answers of 2026-10-21 (after the audit):**
  - The project is the master (its sources). A controller is any controller,
    and the engineer chooses where to connect. There is no binding and no
    "different device" refusal. Connect shows the comparison instead (sections
    0, 5 and 8).
  - Rockwell is the reference for go-online: the engineer sees the controller
    and the project. A truth table covers the edge cases (section 8).
  - Busy is the answer to a connect attempt, not a marker.
  - A protected flag is added, and what is public is separated from what is
    hidden (section 3).
  - The fundamental question was answered A: IronLink finds and writes the
    config, never writes to a controller, and IronPLC connects. Maximum
    modularity: TCP is one connection method of a class, and USB comes later
    (section 4a).
  - The device id is changed only on the device itself (confirmed).
  - Row 6: a warning, loading the difference on the engineer's responsibility,
    or uploading into a new project. It is merged with row 7, since there is no
    conflict (section 8).
  - To-do: a source archive in the controller, a project-creation mechanism
    with templates, and unpacking as one more way of creating a project.
  - There is no firmware concept in IronPLC; the version is the runtime
    version. It is public in discovery, for a protected controller too, as
    Rockwell does (section 3).
- **Plan approved** (owner, 2026-10-21). The owner also said:
  - a local soft PLC is one more device, not a driver (section 7);
  - IronLink finds and establishes the route, discovers devices and organises
    the session (section 4a);
  - a separate VS Code extension is considered (section 4b, open).
- **Answers of 2026-10-21 (after the hot-edit audit, section 9):**
  - Row 6: the responsibility lies with the side where the test runs; the
    candidate stays under Test after the session drops.
  - The audit defects are fixed inside class K1.
  - Download is the "replace" mode of the same pipeline; "no application" is an
    empty container (no ST files).
  - K3 and K4 come first in the online-model plan.
  - The project path file is `<app>.path.iplcinfo`.
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

- **The source archive in the controller, Upload, and project creation from
  templates** (owner's to-do, section 8). This is its own plan, after the
  online model.

- Wire authentication and encryption (ADR-0063 keeps it deferred). The binding
  check is identity, not security.
- Login choices, Start/Stop: the online-model plan.
- A gateway on another machine (a later driver).
- mDNS/DNS-SD: possible later as one more driver.
