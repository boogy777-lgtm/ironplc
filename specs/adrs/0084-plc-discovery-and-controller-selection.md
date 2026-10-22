# PLC Discovery and Controller Selection: A Device Has an Id of Its Own and the Project Remembers the Path

status: proposed
date: 2026-10-21

## Context and Problem Statement

The engineer picks the PLC to work with by writing a connection profile
(`ironplc.connections`) with an address and a port. The profile names an IP
address, and an IP address is not a device: a DHCP lease moves, two cells reuse
one address, and a replaced device answers at the old address. Nothing in the
protocol says *which* device answered. Every `ironplcvm` reports the same
`identity` (`ironplcvm`, `IronPLC SoftPLC`, `vm-cli`, its version), so two
devices cannot be told apart, and the engineer cannot find the PLCs on the
network without knowing their addresses.

The established engineering tools solve both problems, and the shapes they share
are the shape this decision takes:

- **RSLinx (Rockwell).** A separate communication server owns the network, and
  Studio 5000 and FactoryTalk go through it. EtherNet/IP `ListIdentity`
  (command `0x63`) is a broadcast on UDP 44818, and the session uses TCP 44818:
  one port number for both. The reply holds vendor, device type, product code,
  revision, status, serial number, product name and state. Behind a router a
  broadcast does not pass, so the "Ethernet devices" driver sends the same
  request to a configured list of addresses. "Set Project Path" stores the chosen
  path in the project as a convenience; the engineer can connect to any
  controller, and Studio 5000 then shows what the controller holds against the
  project ("Connected To Go Online").
- **CODESYS.** A gateway listens on TCP 1217, and the scan runs on UDP
  1740 to 1743. The scan result shows device name, address, serial number,
  vendor, target id, type and version. The project remembers the device by its
  name when it is unique, otherwise by its address, and not by the IP. After the
  login the engineer chooses what to do with the program in the controller.
- **Others.** Beckhoff ADS searches on UDP 48899 with the session on TCP 48898,
  and an advisory of 2019 describes one empty UDP packet that stopped the search
  service. Siemens uses layer 2 (DCP), which a VPN does not pass.

(The vendor behaviour above is taken from the vendors' help and from public
protocol descriptions, not from their code.)

A discovery reply is unauthenticated, so it carries no secret and is a
reconnaissance surface for anyone on the network. Wire authentication stays
deferred (ADR-0063), so what the identity says is who the device claims to be,
not that it is trustworthy.

What identifies a device, where does it live, who may change it, what does
discovery say, who connects, and what does a project remember about the
controller it was last used with?

## Decision Drivers

- **The project is the master.** The project is its sources. A controller is any
  controller the engineer chooses to connect to; nothing binds a project to one
  device, and nothing refuses a controller for being a different one.
- **A remembered path that survives an address change.** The workstation must
  recognise a controller again when its address moves, and must notice that a
  different controller answers at an address it knew, as information.
- **A soft PLC has no serial number.** The id has to be generated, and has to
  survive a restart.
- **The device does not know where it runs.** The same `ironplcvm` runs on a
  Windows or Linux desktop and, later, on embedded hardware. Nothing in the
  identity or the settings may assume a location, a file system layout or an
  interface.
- **Discovery and comparison are different questions.** Finding a device and
  asking whether its application equals the project's are different
  responsibilities, with different costs and a different attack surface.
- **No silent change of identity.** A device that generates a new id whenever its
  settings are unreadable breaks the recognition of the device and the records
  keyed by its id, without a message.
- **One definition of everything that two sides must agree on:** the port, the
  identity fields, the command vocabulary.
- **Maximum modularity.** How a controller is reached (stdio, TCP, USB later) and
  how controllers are found (broadcast, address list, USB enumeration later) are
  classes whose members register themselves.

## Considered Options

For the identity of a device:

1. **The address.** Today's behaviour. Rejected: it is the problem.
2. **The device name.** CODESYS does this when the name is unique. Rejected for a
   soft PLC: every `ironplcvm` has the same name until someone renames it, and a
   name is a label that an operator edits without meaning to change the
   identity.
3. **A random, persistent device id that is changed on the device itself and
   reported by `identity` (chosen).**
4. **A hardware serial number or a MAC address.** A hardware target may report its
   serial as its id. Rejected as the general answer: a soft PLC has neither, and
   a MAC address is a location (it changes with the interface).
5. **An id derived from the application or its hash.** Rejected: the id would
   change with every download, and equality of content is the online model's
   question, not the identity's.

For who connects (the fundamental question):

- **A. IronLink finds and records (chosen).** IronLink finds controllers and
  routes and records the routes it uses on the workstation. The remembered path
  in the project is written by IronPLC (see the two places below). It never
  writes to a controller. IronPLC connects, and every conclusion after the connection
  belongs to IronPLC.
- **B. IronLink carries the session.** IronLink is also the transport, and the
  editor connects through it. RSLinx does this. It lets several clients share one
  device and carries transports that are not plain TCP, at the cost of a relay,
  session multiplexing and a lifecycle. Named, not built: there is no second real
  case yet.

## Questions and Entities

Every function of the engineering connection answers one of five questions. A
function that answers none of them is not needed.

1. Which controllers can this workstation see, and how does it reach each?
2. Which controller does the engineer connect to now? The engineer chooses.
3. Which controller answered, and what does it hold compared with the project?
4. Can a session be opened, and if not, why not? This is the answer to the
   connect attempt: unreachable, busy or protected.
5. Once connected, which operations are allowed, and on what grounds?

A session alone says nothing about whether the program is equal, what state the
device is in, or whether the engineer may act on the process.

| Entity | Meaning | Not to be mixed with |
|---|---|---|
| Project | the master: the sources; the build makes the application container | a controller, or what a controller holds |
| Device | one physical or soft controller instance | an address, a name, a tree row |
| Device id | how the instance is recognised again (in the comparison, and as the key of the download record) | the name; the address; authenticity |
| Address | one possible access point | the identity of the device |
| Route | how this workstation reaches the device (driver, interface, address) | the device itself |
| Observation | what a discovery or an identify request returned, with its time | a promise that the device is reachable now |
| Connection path (remembered) | the controller the engineer last chose, with its route, remembered for convenience (as Set Project Path) | an intent or a binding of the project; an open session |
| Workstation route record | the route this workstation uses for a device id | the connection path of the project |
| Session | the current interaction of this workstation with a device | the remembered connection path |

The [glossary](../steering/glossary.md) defines these words.

## Decision Outcome

Chosen option: **a device id of its own, generated once and kept, in a settings
file that the device owns; the project stays the master and remembers only a
convenient path; IronLink finds and records and IronPLC connects**, with the
following parts.

- **One engineering port.** The IronPLC port (`DEFAULT_SESSION_PORT`,
  `compiler/runtime`) is TCP for the session and UDP for discovery, as EtherNet/IP
  uses one number for both. It is defined once in Rust, and the extension and the
  documentation take it from that definition.
- **The device id is random and persistent, and is changed only on the device.**
  It is 128 bits from the operating system's random source, written as 32
  lowercase hexadecimal digits. The device generates it on its first start and
  keeps it across restarts. A device that is reset (its settings file removed with
  its other files) or a new device gets a new one. A hardware target that has a
  serial number may report that instead; the field is optional on the wire.
  - **Where it is changed.** In the device's own settings file, and later in a
    local UI of the device. It is never changed over the engineering session
    (confirmed by the owner). The id is how a workstation recognises an instance
    again and the key of the download record, so a session that could rewrite it
    would let a device take over the identity, and the records, of another one
    over the network. The legitimate reasons to change it are local: giving a
    replacement device the id of the one it replaces, and resolving a duplicate
    after a cloned image. The name stays changeable online.
  - **What the format check proves.** That the id has the form of an id. It proves
    neither that the id is unique nor that the device has the right to take it.
- **The device owns a settings file and fails closed.** `ironplcvm serve` keeps
  `<file>.device.json` beside the slot store. Each setting is one row of one table
  (key, first value, where it may be changed, check), and the file reader, the
  online command `setDeviceSettings` and the `identity` answer all go through the
  rows. The row says whether the command may change the setting; a command that
  names a setting the row reserves for the file (the device id) is refused as a
  whole with V6015, and the file and the device stay unchanged. A file
  that cannot be used (not JSON, another version, an unknown setting, a refused
  value) stops the start with V6015. The device never replaces it with a new
  identity: the operator corrects the file, or deletes it to make the device a new
  one. A write that fails is V6016.
- **Discovery says public identity and nothing else.** The reply holds the
  protocol version, the device id, the name, the model, the session port, and
  whether the device is protected (requires credentials). It also holds the
  runtime version, for a protected device too, as Rockwell's ListIdentity gives
  the revision to anyone and has no setting to hide it; protection is the
  interfaces discovery answers on, turning discovery off, and network
  segmentation. The reply does not say whether the session is busy: busy is the
  answer to a connect attempt.

  | Data | Discovery (anyone on the network) | Session (after connect; credentials when protected) | Never on the wire |
  |---|---|---|---|
  | device id, name, model, protocol, session port, protected flag | yes | yes | |
  | runtime version | yes, protected device too | yes | |
  | application identity (name, `contentHash`, `layoutHash`, generations) | no | yes | |
  | pending edit record, run state, device settings | no | yes | |
  | credentials, keys | | | yes |
  | sources | | | yes (the container holds none) |

  The reply holds nothing about the application and no hash: whether the project
  equals the PLC is the online model's comparison (the download record,
  ADR-0083), shown once connected. The public fields are built from the same
  value as the session `identity`, by the same function, so the two never
  disagree.
- **The project is the master, and the engineer chooses the controller.** A
  controller is any controller. There is no binding of a project or an
  application to a device, no "target of an application", and no refusal because
  a controller is a different one. A project with several controllers (a DCS) has
  several applications, and so several remembered paths.
- **The connection path is remembered for convenience, in two independent places,
  each overwritable on its own.** Neither is an intent or a binding.
  - The **project file** `<app>.path.iplcinfo` beside `<app>.iplc` holds the device id of the controller last chosen and a
    route hint, the last address that is not loopback. It travels with the
    project, so a project moved to another PC keeps its last path (RSLinx's Set
    Project Path). It is project data, so **IronPLC writes it**, as a record kind
    of the record mechanism ([ADR-0083](0083-build-and-download-records.md)):
    `ironplcc record-path` is its one writer. A loopback address is never the
    hint; choosing by a loopback address leaves the hint of the same controller
    as it was.
  - The **workstation route record** holds, for a device id, the route this
    workstation uses. **IronLink writes it**, and only it. A loopback address is
    stored only here, because it means "this PC" and points elsewhere on every
    PC.
- **Three separate actions.** *Choose a controller* remembers a connection path.
  *Find a route* produces observations and routes. *Connect* opens a session and
  shows the go-online comparison. Each can happen without the others.
- **Connect shows the comparison and never refuses on identity.** It opens the
  session and shows what the controller is, what it holds, what the project has,
  and which actions follow (Rockwell "Connected To Go Online"; the CODESYS login
  choices). When the controller is not the one this project last connected to,
  the comparison says so in an information row. The answers to the attempt are
  unreachable (offer a search), busy (V6014) and protected (credentials needed; a
  later class, so a protected controller cannot be connected until it exists).
- **Markers in the tree come from observations only:** reachability with the age
  of the observation, protected or open, and the "last chosen" mark. *Busy* is
  not a marker. It is one answer to a connect attempt, and only the attempt can
  know it.
- **IronLink finds and records; IronPLC connects (option A).** IronLink is a
  separate binary, started by each client as a child process on stdio, as the
  language server is; it opens no listening port. Its VS Code part is a separate
  extension, a thin client of the binary. It discovers controllers, finds
  and establishes routes, and writes the workstation route record. It does not
  write the project file, which IronPLC writes with the record mechanism. It
  never writes to a controller. Connecting,
  and every conclusion after it, are implemented once in Rust by IronPLC, and the
  editor, the MCP server and the command line render them. Its API is the command
  mechanism that exists (JSON commands) with its own commands added. A shared
  service on localhost is one more member of the same class, built only for a
  real case. Starting a device is not one of its responsibilities. Option B (IronLink
  carries the session, a transport relay) is named and not built; the session
  protocol stays as it is, so B would change the transport only.
- **Two classes, each with one interface and members that register themselves.**
  - **Connection method** (how IronPLC talks to a controller): stdio (a spawned
    local `ironplcvm`) and TCP today, which are two real members; USB later. A new
    method is one implementation of the transport interface (open, send a frame,
    receive a frame, close) plus its configuration row. The session protocol
    above it does not change. The session tests run once per registered method,
    and a method that is not registered fails a completeness test.
  - **Discovery driver** (how IronLink finds controllers): broadcast and address
    list today; USB enumeration later. A driver produces observations and routes,
    and each route names the connection method that reaches it. A driver never
    carries a session.
- **A soft PLC on the same PC is a device, not a driver.** It is started on its
  own, found like any device, and reached through the loopback interface like any
  other interface.
- **Decisions are made in one place; the clients are thin.** Discovery and route
  decisions are IronLink's: which devices were observed, which routes exist,
  which identity fields are public. Online decisions are IronPLC's: the go-online
  comparison, the allowed operations and the refusal reasons. A client renders
  these answers and holds no rule. This is a checked property: a conformance test
  runs against the IronLink command API, and a test of the VS Code layer fails if
  it computes enablement on its own. A new discovery method (a new driver) adds
  observations and routes only; the meaning of the comparison and of the session
  does not change.
- **The go-online comparison** takes as inputs the answer to the connect attempt,
  the application the controller holds, the build of the project, the download
  record and the pending edit on the controller. Its decision table, and the
  login choices that follow from it, belong to the online model and are specified
  with it, not here.
- **One device id at several addresses.** When the identity fields of the replies
  are equal, it is one device with several routes, shown with the same markers.
  When they differ, it is an identity conflict, which is marked in the tree and
  in the comparison. A cloned settings file is not always detectable.
- **Identify is a fresh request.** It asks the route again and shows the reply
  with its time. Physical identification (blinking a LED) is a later device
  operation of a hardware target, not part of Identify.
- **Identity is not authenticity.** A matching id says that the device is the one
  seen before, not that it is trustworthy. Trust is a later class (credential and
  permission, below).
- **Equal means, in the online model:** the device's committed application
  `contentHash` equals the `contentHash` of the application container the project
  builds now, and the build is up to date (ADR-0083). Run and Stop are live
  observations; after the link is lost they are "unknown", never the last value. A
  working UI shows what exists and no placeholders.
- **The download record is keyed by the device id.** It is
  `<app>.<deviceId>.download.iplcinfo` (what this project last put on this
  device; [ADR-0083](0083-build-and-download-records.md)). It was keyed by the
  connection profile name while no device had an id; records under that name are
  not read.

Classes that fit none of the above are named here so that the design does not close
them, and are not built:

- **Transport relay:** IronLink carries the session (option B).
- **Named path:** an alias or topic for a route. The connection profile name is the
  named path today; it never goes into the device identity.
- **Route:** a path through several hops. The stored path is separate from the
  device id and has one hop; a longer route is a new version of the path format.
- **Data access and subscription:** reading, writing and watching variables. A
  subscription must not assume request and response forever (ADR-0063 has no push
  in version 1).
- **Device operation:** long, exclusive, cancellable operations with progress
  (a runtime update, setting an address). `acceptEdits` is the first one.
- **Diagnostic counters and events:** read-only and resettable; not device
  settings.
- **Credential and permission:** secrets never go into the project file or the
  device settings; wire authentication stays deferred (ADR-0063).
- **Protocol adapter:** a server that re-exposes device data (OPC UA, MQTT); a
  different kind of client.
- **Device description registry:** each device type declares itself.
- **Remote service:** IronLink on another machine; the transport stays behind one
  interface.

### Consequences

- Good, because the engineer connects to any controller, and the project, which
  is its sources, is never held hostage by the identity of one device.
- Good, because the workstation recognises a controller after an address change,
  and tells the engineer when a different controller answers where the last one
  did.
- Good, because discovery is small, stateless on the device and carries no secret
  and no application data; a reply that is lost or spoofed costs a wrong browse
  row, not a wrong download.
- Good, because the device settings are one table: a new setting (the port,
  discovery on or off, the clock) is one row, its check and its line in the
  reference, and the file reader, the command and the identity need no edit.
- Good, because the identity is built in one function, so the session and the
  discovery reply cannot disagree.
- Good, because the connection method and the discovery driver are classes: USB
  is one more member of each, with no change to the session protocol.
- Good, because IronLink never writes to a controller, so its failure or its
  compromise cannot change a program.
- Good, because a damaged settings file cannot silently give a device a new
  identity.
- Bad, because a device with a damaged settings file does not start until someone
  fixes the file; the alternative is a device that changes identity unnoticed.
- Bad, because the id is only as unique as the operator keeps it: copying a
  settings file to a second device gives two devices one id. The recognition and
  the download record cannot tell them apart, and the operator has to change one
  of them.
- Bad, because an id changed in the settings file is a new identity: the
  remembered paths and the download records keyed by the old id no longer find the
  device, and the engineer chooses the controller again.
- Bad, because an engineer who needs the id changed has to reach the device
  itself. That is the point: the session cannot change the identity of a device.
- Bad, because under option A one session per device stays the rule (ADR-0065): a
  second client of the same device gets V6014 until the transport relay (option B)
  exists.
- Neutral: the session protocol stays at version 1. `deviceId` is an optional field
  of the `device` block, absent from a host that has none (the MCP in-process host)
  and from a device that predates it; a reader ignores the fields it does not know,
  and a test on each side parses the block with the field, without it and with a
  field of a later server.
- Future work: the controller also receives the project sources as an archive,
  Upload creates a new project from it, and project creation from templates is
  added; unpacking an uploaded archive is one more way of creating a project.

### Confirmation

What the code implements so far:

- `compiler/vm-cli/src/device_settings.rs` and `device_settings/`: the table of
  settings, the settings file, the refusal on a file that cannot be used, and the
  one function that builds the identity. The row says where a setting may be
  changed. Tests per setting for the file, for an invalid value in the file and
  for an invalid value in the command; a test per file-only row that the command
  refuses it, alone or with another setting, with file and device unchanged, and
  that an edit of the file is adopted at the next start; tests for a new device, a
  restart, two devices, a damaged file and a failed write.
- `compiler/vm-cli/src/serve_device_tests.rs` and `compiler/vm-cli/tests/`: the
  `identity` and `setDeviceSettings` commands over the stdio, TCP and HA pair
  paths.
- `compiler/runtime/src/commands.rs`: the optional `deviceId`, the command, and the
  refusal (V4021) by a host that has no settings.
- `integrations/vscode/src/test/unit/`: the parser reads the optional `deviceId`
  and ignores what it does not know, and the device panel shows it.
- `compiler/discovery/src/tests.rs`: the discovery request and reply round-trip,
  every truncation and any byte string fail without a panic, and only the exact
  request is recognised (format in `specs/design/plc-discovery.md`).
- `compiler/vm-cli/src/discovery.rs` and `discovery/`: the device answers
  discovery on UDP at the address and port of `serve --listen`, with the reply
  built in one place from the identity of the session and read for every
  request. Tests for the reply built from a device and from a renamed device,
  for the responder on a loopback socket (it answers the request, and answers no
  datagram of another length or with one byte changed), and for the rate limit
  with an injected clock (per source, in total, and the bounded table of
  sources). `compiler/vm-cli/tests/cli/serve_discovery.rs`: the started device
  answers on UDP, also while a session is open and after a rename; it does not
  answer and does not open the port with `--no-discovery`; it stops with V6017
  when the UDP port is taken. `compiler/vm-cli/src/tcp.rs`: `--listen` takes an
  IP address alone and uses the default session port (the rules are
  REQ-VC-vm-cli-031 to 036 in `specs/design/vm-cli.md`).
- `compiler/ironlink` (`ironlink`; format and rules in `specs/design/ironlink.md`,
  REQ-IL-ironlink-001 to 054): IronLink with its two discovery drivers, broadcast
  and address list, as a class whose set is derived from one declaration and
  whose builder has no wildcard. Tests with fake devices on loopback ports: the
  drivers find one and two devices and a silent target yields nothing; the
  request is sent `attempts` times at least 300 ms apart and nothing else is ever
  sent; undecodable datagrams are counted, a reply of another wire version is its
  own outcome, and duplicates from one source are merged; the grouping of one
  device id at several addresses into one device with several routes, and an
  identity conflict; identify, with no answer as a normal answer of the shell;
  the shell by lines in and lines out (each command, the codeless codec error);
  and the program as a process (`compiler/ironlink/tests/cli.rs`): `--json` prints
  the response of the shell, an identify that nothing answers exits with L6003,
  and the log stays off stdout. A broadcast to the real `255.255.255.255` is a
  manual check, because it depends on the network of the PC.

Built since: the path file `<app>.path.iplcinfo` of the project, as the record
kind `Path` of ADR-0083 with `ironplcc record-path` and `record-path-path`
(`compiler/project/src/path_record.rs`, `compiler/ironplc-cli/tests/cli_record_path.rs`:
the body, the loopback rule, a bad device id or address, a missing and a
malformed file). The workstation route record and the stored address list of
IronLink (`compiler/ironlink/src/store.rs`, REQ-IL-ironlink-060 to 078): IronLink
sets a route only after an identify that returned the device id.

Built since: the IronLink extension (`integrations/ironlink`), a thin client of
`ironlink serve` with the browse view and the Network tree; its Choose action hands
`{deviceId, address, sessionPort}` to the IronPLC command named in the handoff
table of `integrations/vscode-common`.

Built since: the IronLink extension returns an API (`IronLinkApi`, defined in
`integrations/vscode-common`) and its Choose action sets the workstation route
before the handoff; the IronPLC command `ironplc.chooseController` writes the
path file with `ironplcc record-path` from the handoff, or from a picker of the
controller last chosen, the controllers with a route on this workstation and the
controllers recently observed
(`integrations/vscode/src/chooseController.ts`, `chooseControllerLogic.ts`).
"Choose" is the workstation route (IronLink `routeSet`) and the path file
(IronPLC); IronLink has no `select` command.

Built since: connecting to the controller chosen (`ironplc.connectController`,
and the `connect` action of the IronLink Network view): the route is the
workstation route of the device id, else the route hint of the path file, and the
session is opened by an unsaved `tcp` profile through the connection method; a
controller whose `deviceId` differs from the chosen one is shown as an
information line and never refused
(`integrations/vscode/src/connectControllerLogic.ts`, `connection.ts`).

What is not built, so the status stays `proposed`: USB enumeration or a gateway as
discovery drivers, the connection method as a class with a registration check, and
the go-online comparison.
