# Design: The Session Client

status: implemented
date: 2026-10-22

## Overview

The session client (`ironplc-session-client`, `compiler/session-client`) is the
Rust client of the engineering session protocol: it connects to a device, makes
the identity handshake, and sends one command at a time. The protocol, its two
transports and the handshake are defined in
[engineering-connection.md](engineering-connection.md); the types it carries
(`Command`, `Response`, `CommandError`) are those of
`compiler/runtime/src/commands.rs` and the frame and the timeouts are those of
`compiler/runtime/src/frame.rs`. The client adds no second definition of any of
them.

IronPLC connects and IronLink only finds controllers
([ADR-0084](../adrs/0084-plc-discovery-and-controller-selection.md)). The client
is what a Rust program uses to connect: the MCP server is its first user, and a
command line may be the next. The editor extension has its own TypeScript
client of the same protocol.

The crate is a library of its own, beside `runtime` and not in it. `runtime` is
the device side (host, commands, frame) and every program that links it serves
or checks the protocol; a client brings processes and sockets that a program
without a connection should not link. It is not a module of `mcp` because the
second user is already named, and the editor-free connection is not a tool of
that server.

## The class of connection methods

A connection method is a way to reach one device. There are two members:
`stdio`, a local `ironplcvm serve <program>` that the client spawns, one JSON
command per line on its stdin and stdout; and `tcp`, a socket to a device, one
length-prefixed frame per message. The class has the same two members as
`CONNECTION_METHODS` of the extension
(`integrations/vscode/src/connectionMethods.ts`). The discovery driver is the
near case outside the class: it finds controllers and never carries a session.

**REQ-SC-session-client-001** The connection methods are declared in one table
(`connection_methods!` in `endpoint.rs`); the `Endpoint` enum, the list of
names `Endpoint::METHODS`, `Endpoint::method` and the match that opens a
channel are made from it, and the match has no wildcard arm. The names are those
of the `transport` setting of the editor (`stdio`, `tcp`), and a test compares
the two lists.

A new method is one module under `endpoint/` that implements `Method`, one row in
the table, and one row in the function that serves the method in the tests of
the client, which fails the test of the class for a method that has none. The
handshake, the timeouts and the errors belong to the session and are not edited.

**REQ-SC-session-client-002** The `stdio` method starts
`<executable> serve <program>` with its standard streams piped, writes each
command as one line and reads each response as one line.

**REQ-SC-session-client-003** The `tcp` method connects to `address:port` and
carries each message as one frame of `ironplc_runtime::frame` (`write_frame` and
`read_frame`), the frame a device reads and writes.

## The handshake

**REQ-SC-session-client-004** `identity` is the first command on every opened
channel, and the session keeps its payload (`Session::identity`).

**REQ-SC-session-client-005** A device that reports a session protocol version
above `SESSION_PROTOCOL_VERSION` is refused with `SessionError::ProtocolTooNew`.

**REQ-SC-session-client-006** A device that answers `identity` with the error
that has no code (it predates the command) opens a session without an identity,
and the session goes on with the commands that device knows.

## Errors

**REQ-SC-session-client-007** An error response that carries a V-code is
`SessionError::Device`, which holds the `CommandError` of the protocol: the code,
the message and the type-change pairs of a V4010 reach the caller as the device
wrote them. A refusal is an answer and not a fault of the connection, so the
session answers the next command.

**REQ-SC-session-client-008** A device that already serves an engineering session
refuses a second one with V6014, and the second `Session::connect` returns it as
`SessionError::Device` with that code. The first session is unaffected.

**REQ-SC-session-client-009** The limits are those of the timeout policy of the
engineering connection: `Timeouts::default()` is `CONNECT_TIMEOUT` for the open
and the identity round trip and `RESPONSE_TIMEOUT` for the answer to a command,
the two constants of `ironplc_runtime::frame` that the device uses too. A step
that exceeds its limit is `SessionError::TimedOut` with the step (`Phase`) and
the limit.

**REQ-SC-session-client-010** A connection that ends is `SessionError::Closed`,
with the error output of the program when a `stdio` program exited at start (so
the V-code it printed reaches the caller). The session is not used after an
error of the connection.

**REQ-SC-session-client-011** The error that has no code, which a device answers to
a line it cannot read as a command, is `SessionError::Codec` and the session
goes on. It is not a `CommandError`: every `CommandError` has a code.

**REQ-SC-session-client-012** A channel that cannot be opened (a program that does
not start, a refused connection, an address that does not resolve) is
`SessionError::Open` with the method that was tried.

## Out of scope

- The reconnect with backoff and the heartbeat of the editor's connection state
  machine. A session is one connection and a caller that loses it opens another.
- Credentials. A device that refuses a connection for want of them answers with
  a coded error, which the client returns as `SessionError::Device`; the client
  sends none.
- The record of a download. A `stdio` session started with a container is a
  download, which the caller records.
