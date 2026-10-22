# Design: The IronLink VS Code Extension

status: partially implemented
date: 2026-10-22

The extension **IronLink** (`integrations/ironlink`, extension id
`ironplc.ironlink`, package `ironlink`) is the VS Code part of IronLink, as
decided in [ADR-0084](../adrs/0084-plc-discovery-and-controller-selection.md): a
separate extension and a thin client of the shell. It starts `ironlink serve` as a
child process on stdio, as a language server is started, and keeps no daemon and
no port. The program decides; the extension renders a tree, runs commands and
writes a log. Its version is the version of the toolchain. It works on its own,
without the IronPLC extension, and the two extensions share only the code of
`integrations/vscode-common`, reached through one module.

The requirements below are tested in TypeScript, not by a Rust crate, so they
live in this document, which no crate lists: the Rust spec-conformance guard
checks only the documents a crate's `build.rs` lists, and every requirement in
such a document must belong to a listing crate. The extension's own
`check-invariants` takes its place here: each requirement must be named by a
test, and each requirement a test names must be in this document. The program
itself is specified in [ironlink.md](ironlink.md).

## Finding and running the program

**REQ-IL-ironlink-vscode-079** The extension finds `ironlink` with the toolchain
discovery that the IronPLC extension also uses (`findToolchainExecutable`): the
directory of the setting `ironlink.path`, the environment variable `IRONPLC`, the
install directories, and last the directories of `PATH`.

**REQ-IL-ironlink-vscode-080** The extension starts `ironlink serve` when the
first command needs it and not at activation, starts one process for any number
of commands, and stops it when the extension is deactivated.

**REQ-IL-ironlink-vscode-081** The client sends one command as one JSON line and
reads one response line for it; commands sent before their responses arrive are
answered in the order they were sent. Unset options are not sent, so the program
applies its own defaults.

**REQ-IL-ironlink-vscode-082** A response with `"response":"error"` and an L-code
is a failure that carries the L-code and the message, and is shown as
`L#### - message` with a link to the page of that L-code. The process stays and the
next command works.

**REQ-IL-ironlink-vscode-083** When the process exits, the commands waiting for a
response fail with E0018, and the next command starts a new process.

**REQ-IL-ironlink-vscode-084** The errors of the extension itself are codes of the
catalog shared by the VS Code extensions (`integrations/vscode-common`): E0016
when the program is not found, E0017 when it cannot be started, E0018 when it
exits, and E0019 when a response cannot be read (not JSON, the response of another
command, or an error without an L-code). A response that cannot be read ends the
process, so the next command starts a clean one. No error text is written in the
extension.

## The Network view

The view is a tree of text rows, as RSWho shows them. It is built from the browse
response by a function that has no dependency on VS Code.

**REQ-IL-ironlink-vscode-085** The tree is the workstation, under it one row for
each driver that ran (`broadcast` with its targets, `addressList` with its
targets), and under each driver one row for each route to a device. A driver that
heard nothing has one row that says so, and a datagram of another wire version is
a row that says to update IronLink. The words of a driver kind come from its
`kind`, so a new driver needs no table in the extension.

**REQ-IL-ironlink-vscode-086** The label of a route row is the name of the device.
Its description holds the address and session port, the first 8 digits of the
device id, the markers `route` (the stored route is this route), `protected` and
`conflict`, each only when the response says so, and the age of the observation.
The tooltip holds the full observation. The markers are the program's answers and
are not computed by the extension.

**REQ-IL-ironlink-vscode-087** A device that two drivers found is shown under both
drivers. A device with an identity conflict has one row for each route of each
variant, and every one of them has the `conflict` marker.

**REQ-IL-ironlink-vscode-088** The routes of `routesNotSeen` are under a node
"Routes not seen" after the drivers, and the node is absent when there are none.

**REQ-IL-ironlink-vscode-089** The settings `ironlink.browse.timeoutMs` and
`ironlink.browse.attempts` are sent with `browse` and `identify` when they are
set, and not otherwise; the extension does not check their range, the program
does (L6004).

## Actions

**REQ-IL-ironlink-vscode-090** "Search network" runs `browse` and shows the tree.
"Add address…" asks for `IP[:PORT]` and sends the text as typed, and the program
validates it: an error is shown with its L-code. "Remove address" offers the
stored addresses (`addressList`) and removes the chosen one.

**REQ-IL-ironlink-vscode-091** Whether an action on a device row is available is
decided by one pure function of the row and the context (the registered
commands): enabled, or unavailable with a reason. Every device action asks it. The
commands are always on the row; an unavailable action is not run and shows its
reason when it is invoked. No `when` clause repeats the rule.

**REQ-IL-ironlink-vscode-092** "Choose" is available if and only if the IronPLC
command of the handoff action is registered. The extension tests for the command
id (`getCommands`) and never for the id of the IronPLC extension, which has two ids
(Open VSX and Visual Studio Marketplace). The command id, the type of its argument
and the reason shown when it is missing are declared once, in the table of handoff
actions of `integrations/vscode-common`, which the IronPLC extension imports to
register the command.

**REQ-IL-ironlink-vscode-093** "Choose" calls the IronPLC command with exactly
`{deviceId, address, sessionPort}` of the row. IronLink writes no project data:
the project path file is IronPLC's.

**REQ-IL-ironlink-vscode-100** "Choose" first sets the route of this workstation
for the device of the row (`routeSet` at the address and session port of the row,
which the program verifies with an identify), and only after that succeeded calls
the IronPLC command. When the route cannot be set, the failure is shown with its
code and the IronPLC command is not called, so nothing is written to the project.
When the IronPLC command is missing, "Choose" is unavailable and no route is set.

**REQ-IL-ironlink-vscode-094** "Identify" sends a fresh `identify` to the address
and session port of the row and shows the reply with its time. No answer is shown
as a warning, because it is a normal answer of the program, and an answer of
another wire version says to update IronLink.

## The API for other extensions

The extension returns an API from `activate`, as the Git extension does, so that
the IronPLC extension can choose and connect a controller without a second
program or a second store. The types (`IronLinkApi` and the values it returns)
are written once in `integrations/vscode-common` (`ironlinkApi.ts`), free of
`vscode`, and both extensions import them. The IronLink extension is found by its
one id, `ironplc.ironlink`. It is optional for the IronPLC extension, which does
not declare a dependency on it.

**REQ-IL-ironlink-vscode-096** The `exports` of `activate` carry `apiVersion` and
exactly the members of the shared type (`IRONLINK_API_MEMBERS`): `routeList`,
`lastBrowse`, `browse`, `setRoute` and `revealNetwork`.

**REQ-IL-ironlink-vscode-097** `routeList` returns the routes stored on this
workstation, as the program lists them. `lastBrowse` is `undefined` before any
browse and afterwards the devices heard in the last browse (once for each address
and port, the later observation when two drivers heard one, newest first) with the
time the browse finished. `browse` runs the browse of the Network view with the
user's settings, shows the result in the view and remembers it as the last browse.

**REQ-IL-ironlink-vscode-098** `setRoute(deviceId, address)` sends `routeSet` and
returns the route that was stored, or a failure that carries the `L####` code and
the text of the program (or the `E####` code when the program could not be run).
A failure of the program is a value and is not thrown; an error that is not one
of these is a defect of the extension and is thrown.

**REQ-IL-ironlink-vscode-099** `revealNetwork` focuses the Network view.

## Packaging

**REQ-IL-ironlink-vscode-095** The extension reaches `integrations/vscode-common`
only through `src/vscodeCommon.ts`, which is checked by the check that the shared
package provides. The extension is bundled, so the package is compiled into
`out/extension.js` and is not in the VSIX.
