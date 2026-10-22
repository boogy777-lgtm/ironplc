# Design: IronLink, the Workstation Tool That Finds Controllers

status: partially implemented
date: 2026-10-22

## Overview

IronLink (`ironlink`, crate `ironplc-ironlink`, `compiler/ironlink`) is the
workstation tool that finds IronPLC controllers on the network and the routes to
them. Why it is a separate program, what it may and may not do, and what a
device says about itself are decided in
[ADR-0084](../adrs/0084-plc-discovery-and-controller-selection.md) and are not
repeated here. The bytes of the request and the reply are in
[plc-discovery.md](plc-discovery.md). The wider engineering connection is in
[engineering-connection.md](engineering-connection.md).

Under the decision of ADR-0084 (option A) IronLink **finds**. It opens no
listening port, holds no session and never writes to a controller: the only
datagram it sends is the discovery request. IronPLC connects.

**Built:** the class of discovery drivers with its two members (broadcast and
address list), the collection they share, the observation and browse model, the
workstation store (the route record and the address list) with the commands
`route` and `address`, the commands `browse`, `identify`, `serve` and `version`,
and the L-codes.
The project path file `<app>.path.iplcinfo` is not IronLink's: IronPLC writes it
(`ironplcc record-path`, [engineering-connection.md](engineering-connection.md)).
The VS Code extension, a thin client of the shell with the Network view, is
built (see [ironlink-vscode-extension.md](ironlink-vscode-extension.md)); its Choose
action sets the route of this workstation and then hands off to IronPLC, and the
extension returns an API for the IronPLC extension.
**Not built:** choosing a controller (`select`, and the IronPLC command that
records the choice), USB enumeration or a gateway as drivers, and the class of
connection methods.

## The class of discovery drivers

A discovery driver is a way of reaching candidate controllers with the discovery
request and collecting what comes back. There are two real members, so there is a
class (ADR-0084): `broadcast` and `address list`. USB enumeration and a gateway
would be members later. The connection method, which is how IronPLC talks to a
controller once it is found, is the near case outside the class: a driver never
carries a session.

**REQ-IL-ironlink-001** The set of drivers a command can name is derived from the
declaration of the members (`DriverKind::ALL`); each member builds a driver of its
own kind, and the match that builds drivers has no wildcard, so a member without
a builder does not compile.

A driver takes from the selection (the interfaces and addresses the user named,
and the port) what it needs, and builds nothing when the selection gives it
nothing to do. Browse asks every member in the declaration and runs the drivers
that were built at the same time, so a new driver is one module, one line of the
declaration and one arm of the builder; `browse`, `identify` and the command
dispatch are not edited.

### Broadcast

The request is sent to the limited broadcast address `255.255.255.255` on the
discovery port, from a socket bound to each chosen local interface address with
broadcast enabled. The broadcast leaves through the interface that has that
address. The user names an interface by a local address of this PC
(`192.168.1.20`); listing the interfaces would need a dependency the standard
library lacks. `0.0.0.0` lets the operating system choose.

**REQ-IL-ironlink-002** The broadcast driver sends to `255.255.255.255` on the
discovery port, which is the session port unless the user gave another.

**REQ-IL-ironlink-003** The broadcast driver opens one socket for each interface,
bound to that interface address with a system-chosen port, so the request leaves
from it; an address that is not an address of this PC fails with L6001.

**REQ-IL-ironlink-004** An IPv6 interface is refused with L6004 and a message
that says IPv6 has no broadcast.

**REQ-IL-ironlink-005** A selection that names no interface and no address means
the broadcast on `0.0.0.0`; a selection that names only addresses has no
broadcast; a selection that names interfaces broadcasts from them. The stored
address list is not something the user named for the browse: it does not change
this rule (REQ-IL-ironlink-074 below).

### Address list

**REQ-IL-ironlink-006** The address list driver sends the request by unicast to
each configured `IP:PORT` (an IP alone takes the discovery port), from one socket
bound to the wildcard address of the family of the targets, and a target that
does not answer does not hide the ones that do.

## Collection

Collection is one function that every driver uses: it sends the request, waits
for the replies until the deadline and turns each datagram into an outcome. It is
the only place IronLink sends or receives a datagram.

**REQ-IL-ironlink-007** The request is sent `attempts` times (default 2, at most
10), the n-th at n times 300 ms after the first and only if that moment is before
the deadline. The spacing is longer than the 250 ms in which a device answers one
request per source address, so a lost datagram is asked again without the retry
being rate-limited away.

**REQ-IL-ironlink-008** Replies are read until the deadline (default 1000 ms, at
most 60000 ms) and the wait is not shortened by a reply, because a browse cannot
know how many devices there are; a timeout or a count outside its range is L6004.

**REQ-IL-ironlink-009** A datagram that `decode_reply` does not accept is counted
and logged at debug level, never an error; the receive buffer is
`MAX_REPLY_LEN + 1` bytes, so a datagram that is too long is seen as too long.

**REQ-IL-ironlink-010** A reply that names another wire version
(`UnsupportedVersion`) is its own outcome, kept with its address, wire version and
time, so that the user is told to update IronLink; an outcome is an enum
(`Heard::Device`, `Heard::OtherVersion`), not a flag.

**REQ-IL-ironlink-011** Repetitions from one source are merged: one observation
for each (device id, address, session port), with the latest time; one other-version outcome for
each (address, wire version).

**REQ-IL-ironlink-012** The wait of an identify ends at the first outcome.

**REQ-IL-ironlink-013** Every datagram IronLink sends is exactly
`ironplc_discovery::request()`.

## Observation and browse model

The JSON below is what the shell prints and what the command line prints with
`--json`; the names are in camel case.

An **observation** is what one discovery reply said, with the route it came by
and the time it was seen (ADR-0084: it ages, and is not a promise that the device
is reachable now):

```json
{
  "deviceId": "00112233445566778899aabbccddeeff",
  "name": "Press line 3",
  "model": "IronPLC SoftPLC",
  "runtimeVersion": "0.247.0",
  "protocol": 1,
  "sessionPort": 49152,
  "protected": false,
  "route": {
    "driver": "addressList",
    "address": "192.168.1.50",
    "sessionPort": 49152,
    "method": "tcp"
  },
  "seenAt": 1761136000123
}
```

**REQ-IL-ironlink-020** An observation has exactly these fields: `deviceId` (32
lowercase hexadecimal digits), `name`, `model`, `runtimeVersion`, `protocol`,
`sessionPort`, `protected`, `route` (`driver`, `address` the source address of the
reply, `sessionPort`, `method`) and `seenAt`; every observation is made from a
reply that `decode_reply` accepted.

**REQ-IL-ironlink-025** `seenAt` is the time the datagram was received, in
milliseconds since the Unix epoch.

A **browse result** is a tree, workstation then drivers. Devices are grouped by
device id within a driver; each device has one variant for each distinct set of
identity fields (`name`, `model`, `runtimeVersion`, `protocol`, `protected`), and
each variant lists the routes by which it answered. The session port is not an
identity field: it belongs to the route.

```json
{
  "response": "browse",
  "workstation": { "name": "PC-ONE" },
  "drivers": [
    {
      "kind": "broadcast",
      "targets": ["192.168.1.20 -> 255.255.255.255:49152"],
      "devices": [
        {
          "deviceId": "00112233445566778899aabbccddeeff",
          "conflict": false,
          "variants": [
            {
              "name": "Press line 3",
              "model": "IronPLC SoftPLC",
              "runtimeVersion": "0.247.0",
              "protocol": 1,
              "protected": false,
              "routes": [
                {
                  "driver": "broadcast",
                  "address": "192.168.1.50",
                  "sessionPort": 49152,
                  "method": "tcp",
                  "seenAt": 1761136000123
                }
              ]
            }
          ]
        }
      ],
      "otherVersions": [],
      "undecodable": 0
    }
  ]
}
```

**REQ-IL-ironlink-021** Replies under one device id with equal identity fields are
one device with several routes.

**REQ-IL-ironlink-022** Replies under one device id with different identity fields
are one device with `conflict: true` and one variant for each different answer
(an identity conflict, ADR-0084). The result does not depend on the order in which
the replies arrived.

**REQ-IL-ironlink-023** A browse result has the workstation, and for each driver
that ran its `kind`, its `targets`, its `devices`, the `otherVersions` it heard
and the `undecodable` count.

**REQ-IL-ironlink-024** The name of the workstation is the environment variable
`COMPUTERNAME`, else `HOSTNAME`, else "this workstation".

Devices are grouped within a driver, so a device found by two drivers is shown
under both and a conflict between drivers is not detected. The route record of
the workstation store (below) holds one route for each device id, and marks the
routes of a browse that it holds.

## Identify

Identify sends a fresh request to one route (`IP`, or `IP:PORT`) and answers with
the observation, with the other wire version it heard, or with `none`.

**REQ-IL-ironlink-030** Identify answers the observation of the device that
answered, or the other version that answered.

**REQ-IL-ironlink-031** No answer within the time is a normal answer
(`"answer": "none"` with the address and `timeoutMs`), not an error of the shell;
the command line turns it into L6003.

```json
{ "response": "identify", "address": "192.168.1.50:49152", "timeoutMs": 1000, "answer": "none" }
```

## The workstation store

IronLink writes only its own data, and knows no project. The data is what is true
of THIS workstation, independent of any project: the route record (how this
workstation reaches a device id) and the address list (the persistent targets of
the address-list driver). The project path file, which says which controller a
project goes to, is IronPLC's record
([ADR-0084](../adrs/0084-plc-discovery-and-controller-selection.md)). There are
two members and one mechanism: each is a versioned JSON file of one directory per
user, written atomically and read strictly. A later setting of the workstation is
one more file of the same store.

**REQ-IL-ironlink-060** The directory of the store is `%APPDATA%\IronPLC\ironlink`
on Windows, `$HOME/Library/Application Support/IronPLC/ironlink` on macOS, and
`$XDG_CONFIG_HOME/ironplc/ironlink`, else `$HOME/.config/ironplc/ironlink`, on
other systems; the environment variables are read directly, with no dependency,
and an empty variable is not set.

**REQ-IL-ironlink-061** The option `--store DIR` (global) names the directory of
the store and wins over the environment variable `IRONLINK_STORE`, which wins over
the directory of the user. When no directory can be determined, the commands that
need the store fail with L6004 and the others (`version`) do not.

**REQ-IL-ironlink-062** Each file is JSON with `"version": 1`, pretty-printed with
sorted keys, and written atomically: the text goes to a temporary file in the same
directory, which is then renamed over the file, so no temporary file is left. The
directory is created on the first write.

**REQ-IL-ironlink-063** A file that does not exist reads as empty, and reading
creates nothing.

**REQ-IL-ironlink-064** A file that is not JSON, has another version, or has
another shape (a missing or extra key, a value of the wrong type, a key that is not
a device id, a port 0) is L6006 `StoreUnreadable`, which names the file. IronLink
never changes such a file, not even to add an entry, because writing over it would
destroy what the user kept: the user fixes or deletes it. The other file of the
store is read alone.

**REQ-IL-ironlink-065** A file that cannot be written (the directory cannot be
created, the temporary file cannot be written or renamed) is L6007 `StoreWrite`,
which names the file; the old file stays.

**REQ-IL-ironlink-066** `routes.json` holds one route for each device id on this
workstation, the latest set winning; removing a route that does not exist is not an
error and writes nothing.

```json
{
  "routes": {
    "00112233445566778899aabbccddeeff": {
      "address": "192.168.1.50",
      "driver": "addressList",
      "sessionPort": 49152,
      "verifiedAt": 1761136000123
    }
  },
  "version": 1
}
```

**REQ-IL-ironlink-067** `addresses.json` holds the persistent targets of the
address-list driver as `IP:PORT`, unique and sorted by address and then port as
numbers.

```json
{
  "addresses": ["10.0.4.7:49152", "192.168.1.50:49152"],
  "version": 1
}
```

### Routes

A route says how THIS workstation reaches a device id. IronLink establishes it
itself, so a stored route is a verified one, with the time it was verified.

**REQ-IL-ironlink-070** `route set <deviceId> <IP[:PORT]>` sends a fresh identify
to the address (with the `timeoutMs` and `attempts` of identify) and, only when
the answer carries that device id, stores the route with the address the device
answered from, its session port, the driver and `verifiedAt`, the time of the
answer; a route that exists is replaced and the result says so (`replaced`).

**REQ-IL-ironlink-071** When nothing answers, or the answer is of a discovery
version this IronLink cannot read, `route set` is L6003; when another device id
answers it is L6008 `RouteMismatch`, naming both ids; a device id that is not 32
lowercase hexadecimal digits is L6004, and nothing is sent. In none of these cases
is anything stored.

**REQ-IL-ironlink-072** `route remove <deviceId>` removes the route and answers
whether there was one (`removed`); `route list` answers the routes ordered by
device id.

**REQ-IL-ironlink-073** `address add`, `address remove` and `address list` change
and show the stored address list; an address is `IP` or `IP:PORT` as in browse,
stored with its port, and adding an address that is in the list or removing one
that is not is not an error (`added`, `removed`).

### Browse and the store

**REQ-IL-ironlink-074** The address-list driver of a browse asks the addresses
named for the browse and then the stored ones, each once; a browse that names no
interface and no address broadcasts on `0.0.0.0` and also asks the stored list.

**REQ-IL-ironlink-075** Each route of a device in a browse result has `routed`,
true when the stored route for that device id has that address and session port
and false otherwise; the text of a browse marks such a route with `route`.

**REQ-IL-ironlink-076** `routesNotSeen` lists the stored routes of device ids that
no driver heard in the browse. They are not observations (an observation is made
only from a reply), and the text lists them under their own heading.

## The shell

`ironlink serve` is the JSON-lines protocol on stdio that a client uses to start
IronLink as a child process. It follows the conventions of the session protocol
of `ironplcvm serve` ([engineering-connection.md](engineering-connection.md)).

| Command | Fields | Response |
|---|---|---|
| `browse` | `interfaces`, `addresses` (lists of strings), `port`, `timeoutMs`, `attempts` | `browse` result |
| `identify` | `address`, `timeoutMs`, `attempts` | `identify` result |
| `routeSet` | `deviceId`, `address`, `timeoutMs`, `attempts` | `{"response":"routeSet","deviceId":..,"route":{..},"replaced":false}` |
| `routeRemove` | `deviceId` | `{"response":"routeRemove","deviceId":..,"removed":true}` |
| `routeList` | | `{"response":"routeList","routes":[{"deviceId":..,"address":..,"driver":..,"sessionPort":..,"verifiedAt":..}]}` |
| `addressAdd` | `address` | `{"response":"addressAdd","address":"10.0.4.7:49152","added":true}` |
| `addressRemove` | `address` | `{"response":"addressRemove","address":..,"removed":true}` |
| `addressList` | | `{"response":"addressList","addresses":["10.0.4.7:49152"]}` |
| `version` | | `{"response":"version","ironlink":"0.247.0","discoveryWireVersion":1}` |

**REQ-IL-ironlink-040** Each command line is answered by one response line, in
order; `version` answers the version of the program and the discovery wire version
it speaks.

**REQ-IL-ironlink-041** A line that is not a command (not JSON, an unknown
command, a missing field) is answered by the codeless codec error
(`{"response":"error","lCode":null,"message":...}`) and the loop goes on.

**REQ-IL-ironlink-042** A command that fails is answered by an error response with
its L-code, and the loop goes on.

**REQ-IL-ironlink-043** End of input ends the loop.

**REQ-IL-ironlink-077** The commands of the store are the shell commands
`routeSet`, `routeRemove`, `routeList`, `addressAdd`, `addressRemove` and
`addressList`, answered by the responses of the same names; a failed command is an
error response with its L-code.

## The command line

`ironlink browse`, `ironlink identify ADDR`, `ironlink route set|remove|list`,
`ironlink address add|remove|list`, `ironlink serve` and `ironlink version`. The command line and the shell call the same functions and
differ only in the rendering: the command line prints text rows, and `--json`
prints the response of the shell.

**REQ-IL-ironlink-050** The text of a browse is the workstation, then each driver,
then each device as
`name  model  runtime <version>  id <deviceId>  <address>:<port>  seen <age> ago`,
with a `protected` marker, a `CONFLICT` marker for an identity conflict, and the
further routes of a device indented under it.

**REQ-IL-ironlink-051** `--json` prints exactly the response line of the shell for
the same command.

**REQ-IL-ironlink-052** An identify that nothing answers exits with code 2 and
L6003 on stderr.

**REQ-IL-ironlink-053** Stdout carries the protocol or the result only; the log
goes to stderr.

**REQ-IL-ironlink-054** An address, interface or option that cannot be used exits
with code 2 and its L-code (L6004, or L6001 for an interface that is not an
address of this PC).

**REQ-IL-ironlink-078** `route` and `address` print text rows (or `--json`, the
response of the shell), and a failed command exits with code 2 and its L-code on
stderr; the global option `--store` and the variable `IRONLINK_STORE` name the
store they use.

## The VS Code extension

The VS Code part of IronLink, the extension `ironplc.ironlink`, is specified in
[ironlink-vscode-extension.md](ironlink-vscode-extension.md). Its requirements
are tested in TypeScript, so they are kept out of this document, which the Rust
crate lists for its spec-conformance tests.

## Problem codes

The prefix is `L`: IronLink is the owner's tool and not the runtime. The registry
is `compiler/ironlink/resources/problem-codes.csv`, the pages are in
`docs/reference/ironlink/problems/`, and the constants are generated from the
registry. L6xxx are IO and network errors (exit code 2) and L9xxx are internal
errors (exit code 3).

| Code | Name | When |
|---|---|---|
| L6001 | InterfaceBind | a socket cannot be bound on the local address |
| L6002 | SendFailed | the request cannot be sent |
| L6003 | NoAnswer | identify, from the command line: nothing answered |
| L6004 | ArgumentInvalid | an address, interface or option value is not valid |
| L6005 | SessionIo | the command session cannot be read or written |
| L6006 | StoreUnreadable | a file of the workstation store cannot be read as a file of this version |
| L6007 | StoreWrite | a file of the workstation store cannot be written |
| L6008 | RouteMismatch | `route set`: the device at the address has another device id |
| L9001 | Internal | a violated invariant of IronLink |
