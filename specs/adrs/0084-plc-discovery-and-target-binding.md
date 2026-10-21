# PLC Discovery and Target Binding: A Device Has an Id of Its Own and the Project Binds to It

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
  path in the project.
- **CODESYS.** A gateway listens on TCP 1217, and the scan runs on UDP
  1740 to 1743. The scan result shows device name, address, serial number,
  vendor, target id, type and version. The project binds to the device name when
  it is unique, otherwise to the device address, and not to the IP. A filter
  hides devices whose target id differs from the project's.
- **Others.** Beckhoff ADS searches on UDP 48899 with the session on TCP 48898,
  and an advisory of 2019 describes one empty UDP packet that stopped the search
  service. Siemens uses layer 2 (DCP), which a VPN does not pass.

(The vendor behaviour above is taken from the vendors' help and from public
protocol descriptions, not from their code.)

A discovery reply is unauthenticated, so it carries no secret and is a
reconnaissance surface for anyone on the network. Wire authentication stays
deferred (ADR-0063), so the check that a project is bound to a device is an
identity check, not a security check.

What identifies a device, where does it live, who may change it, what does
discovery say, and how does a project remember its target?

## Decision Drivers

- **A binding that survives an address change.** The project must recognise its
  PLC when the address moves, and must notice when a different PLC answers at the
  address it knew.
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
  settings are unreadable breaks every project bound to it, without a message.
- **One definition of everything that two sides must agree on:** the port, the
  identity fields, the command vocabulary.

## Considered Options

1. **Bind to the address.** Today's behaviour. Rejected: it is the problem.
2. **Bind to the device name.** CODESYS does this when the name is unique. Rejected
   for a soft PLC: every `ironplcvm` has the same name until someone renames it,
   and a name is a label that an operator edits without meaning to change the
   identity.
3. **A random, persistent device id that is changed on the device itself and
   reported by `identity` (chosen).**
4. **A hardware serial number or a MAC address.** A hardware target may report its
   serial as its id. Rejected as the general answer: a soft PLC has neither, and
   a MAC address is a location (it changes with the interface).
5. **An id derived from the application or its hash.** Rejected: the id would
   change with every download, and equality of content is the online model's
   question, not the identity's.

## Questions and Entities

Every function of the engineering connection answers one of five questions. A
function that answers none of them is not needed.

1. For which controller is this application meant?
2. How does this workstation reach it?
3. Is the device that answers really that controller?
4. Can a session be opened, and if not, why not?
5. Once connected, which operations are allowed, and on what grounds?

A session alone says nothing about whether the program is equal, what state the
device is in, or whether the engineer may act on the process.

| Entity | Meaning | Not to be mixed with |
|---|---|---|
| Device | one physical or soft controller instance | an address, a name, a tree row |
| Device id | how the instance is recognised (the identity check) | the name; the address; authenticity |
| Address | one possible access point | the identity of the device |
| Route | how this workstation reaches the device (driver, interface, address) | the device itself |
| Observation | what a discovery or an identify request returned, with its time | a promise that the device is reachable now |
| Target of an application | the intent: the device this application is meant for (a device id) | an open session |
| Workstation route record | how this workstation prefers to reach a device | the target, which is shared with the project |
| Session | the current interaction of this workstation with a device | the stored target |

The [glossary](../steering/glossary.md) defines these words.

## Decision Outcome

Chosen option: **a device id of its own, generated once and kept, in a settings
file that the device owns**, with the following parts.

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
    local UI of the device. It is never changed over the engineering session:
    the id is the basis of the identity check, and an id that an engineer could
    change over the network would let the engineer silence a "different device"
    refusal. The legitimate reasons to change it are local: giving a replacement
    device the id of the one it replaces, and resolving a duplicate after a cloned
    image. The name stays changeable online.
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
- **Discovery says identity and nothing else.** The reply holds the protocol
  version, the device id, the name, the model and the firmware version, the session
  port, and whether the session is busy. It holds nothing about the application and
  no hash: whether the project equals the PLC is the online model's comparison
  (the download record, ADR-0083), shown once connected. The reply is built from
  the same value as the session `identity`, by the same function, so the two never
  disagree. Nothing in it is secret.
- **IronLink, a communication service on the workstation.** A separate binary that
  listens on localhost only and owns the sessions to devices, as RSLinx does. It
  has three responsibilities: it finds and establishes the route, it discovers
  devices on the network, and it organises the session. Starting a device is not
  one of them. Its API is the command mechanism that exists (JSON commands, framed
  on TCP) with its own commands added, and the editor, the MCP server and the
  command line are thin clients of it. Several clients can then use one device
  without fighting over its one session (ADR-0065).
- **IronLink decides; the clients are thin.** IronLink decides which observed
  device matches the target, which routes exist, whether a connect is allowed or
  refused and why, which operations a device offers now, and what changed after a
  command. A client renders these answers and holds no rule. This is a checked
  property: a conformance test runs against the IronLink command API, and a test
  of the VS Code layer fails if it computes enablement on its own. A new discovery
  method (a new driver) adds observations and routes only; the meaning of target,
  identity check and session does not change.
- **Three separate actions.** *Select target* changes the intent. *Find route*
  produces observations and routes. *Connect* opens a session. Each can happen
  without the others: a target can be selected while it is unreachable, a device
  can be discovered without connecting to it, and a connection can be made for
  diagnosis without changing the target.
- **Markers are independent; there is no single state.** A device row can carry
  any combination of: target or not; reachable, unreachable or unknown, from the
  last observation and its age; session free or busy. *Busy* means that the
  device's one engineering session is held by someone other than this IronLink. It
  does not block selecting a target. Only IronLink decides whether a connect is
  refused (V6014), and a client never disables an action on its own rule.
- **A soft PLC on the same PC is a device, not a driver.** It is started on its
  own, found like any device, and reached through the loopback interface like any
  other interface. The stdio profile that spawns `ironplcvm` from the editor stays
  in the extension, outside IronLink.
- **The target is per application, and the places that hold it have different
  meanings.** A project with several controllers has several applications, and so
  several targets.
  - The **application target file**, `<app>.target.iplcinfo` beside `<app>.iplc`,
    holds the intent: the device id, and a portable route hint (the last address
    that is not loopback). It travels with the project, so a project copied to
    another PC keeps its target (RSLinx's Set Project Path).
  - The **workstation route record** holds, for a device id, the route this
    workstation uses. It is keyed by the device id, so it cannot name a different
    device than the target. A loopback address is stored only here, because it
    means "this PC" and points elsewhere on every PC.
  - Each place can be overwritten on its own. They cannot disagree about which
    device, only about the route, so no "which device?" question exists.
- **Connect checks the device.** The route comes from the workstation record,
  else from the project's hint, else from a browse that finds the device id. The
  device id from `identity` is compared with the target's. Equal: a session.
  Different: a refusal (a new problem code) with three options, *show the
  difference*, *select another target* and *cancel*. There is never "accept this
  device instead" and never "change the device id". Unanswered: a browse can find
  the same device id at a new address, and the workstation record is updated after
  a successful connect. A connect to a device that is not the target, for
  diagnosis, opens a session and leaves the target unchanged; the status bar says
  that the session is not with the target.
- **One device id at several addresses.** When the identity fields of the replies
  are equal, it is one device with several routes, shown with the same markers.
  When they differ, it is an identity conflict: the device is marked and cannot be
  selected as target until it is resolved. A cloned settings file is not always
  detectable; the identity check and the busy session are the defence.
- **Identify is a fresh request.** It asks the route again and shows the reply
  with its time. Physical identification (blinking a LED) is a later device
  operation of a hardware target, not part of Identify.
- **Identity is not authenticity.** The check says that the device is the one the
  project was bound to, not that it is trustworthy. Trust is a later class
  (credential and permission, below).
- **Equal means, in the online model:** the device's committed application
  `contentHash` equals the `contentHash` of the application container the project
  builds now, and the build is up to date (ADR-0083). Run and Stop are live
  observations; after the link is lost they are "unknown", never the last value. A
  working UI shows what exists and no placeholders.
- **The download record is keyed by the device id.** Today it is keyed by the
  connection profile name, because no device id existed; stage 6 keys it by
  `<app>.<deviceId>.download.iplcinfo` (what this device received).

Classes that fit none of the above are named here so that the design does not close
them, and are not built:

- **Named path:** an alias or topic for a route. The connection profile name is the
  named path today; it never goes into the device identity.
- **Route:** a path through several hops. The stored path is separate from the
  device id and has one hop; a longer route is a new version of the path format.
- **Data access and subscription:** reading, writing and watching variables. The
  service owns the sessions, so a subscription can fan out to several clients; the
  relay must not assume request and response forever (ADR-0063 has no push in
  version 1).
- **Device operation:** long, exclusive, cancellable operations with progress
  (firmware update, setting an address). `acceptEdits` is the first one.
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

- Good, because a project recognises its PLC after an address change and refuses a
  different PLC at the old address.
- Good, because discovery is small, stateless on the device and carries no secret
  and no application data; a reply that is lost or spoofed costs a wrong browse
  row, not a wrong download.
- Good, because the device settings are one table: a new setting (the port,
  discovery on or off, the clock) is one row, its check and its line in the
  reference, and the file reader, the command and the identity need no edit.
- Good, because the identity is built in one function, so the session and the
  discovery reply cannot disagree.
- Good, because a damaged settings file cannot silently give a device a new
  identity.
- Bad, because a device with a damaged settings file does not start until someone
  fixes the file; the alternative is a device that changes identity unnoticed.
- Bad, because the id is only as unique as the operator keeps it: copying a
  settings file to a second device gives two devices one id. The binding check
  cannot tell them apart, and the operator has to change one of them.
- Bad, because an id changed in the settings file is a new identity for every
  project bound to the old one; the projects have to select the device again.
- Bad, because an engineer who needs the id changed has to reach the device
  itself. That is the point: the session cannot silence the identity check.
- Neutral: the session protocol stays at version 1. `deviceId` is an optional field
  of the `device` block, absent from a host that has none (the MCP in-process host)
  and from a device that predates it; a reader ignores the fields it does not know,
  and a test on each side parses the block with the field, without it and with a
  field of a later server.

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

What is not built, so the status stays `proposed`: the discovery request and reply
on UDP, the `--listen` default port, IronLink, the browse view, and the target
stored in the project file and in the profile with the check on connect.
