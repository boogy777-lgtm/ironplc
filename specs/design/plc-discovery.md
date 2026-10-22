# Design: PLC Discovery Wire Format

status: partially implemented
date: 2026-10-22

## Overview

A PLC answers discovery on UDP at the session port, and the workstation service
(IronLink) sends the request and reads the replies. This document specifies
the bytes of those two messages, version 1. What discovery says, and what it
never says, is decided in
[ADR-0084](../adrs/0084-plc-discovery-and-controller-selection.md) and is not
repeated here: the reply holds public identity only. The engineering connection
that uses the result is in [engineering-connection.md](engineering-connection.md).

This is a separate document from the engineering connection because the wire
format has its own owner crate, `ironplc-discovery` (`compiler/discovery`), and
its own conformance tests, which the requirement IDs below bind to it.

**Built:** the wire format crate (`encode_reply`, `decode_reply`, `request`,
`is_request`). **Not built:** the responder in `ironplcvm serve --listen` and
the sender in IronLink; this crate only encodes and decodes, and opens no
socket.

The crate is `#![no_std]` without `alloc` and has no dependencies
([ADR-0010](../adrs/0010-no-std-vm-for-embedded-targets.md)), so a device
without an operating system can answer. It has no `std` feature, so the normal
build is the `no_std` build. A reply borrows its strings from the datagram or
from the device's own settings.

## Header

All integers are little-endian, as in the
[container format](bytecode-container-format.md). Every message begins with the
same 8 bytes.

| Requirement | Offset | Field | Type | Description |
|-------------|--------|-------|------|-------------|
| **REQ-DW-discovery-001** | 0 | magic | 4 bytes | `IPLD` in ASCII |
| **REQ-DW-discovery-002** | 4 | wire version | `u8` | `1` |
| **REQ-DW-discovery-003** | 5 | kind | `u8` | `1` for a request, `2` for a reply |
| | 6 | | `u8` | a request: `0`; a reply: the flags |
| | 7 | | `u8` | a request: `0`; a reply: reserved, `0` |

## Request

**REQ-DW-discovery-004** A request is exactly the 8 bytes of the header with kind
1 and bytes 6 and 7 zero.

**REQ-DW-discovery-005** The request recogniser accepts that one byte string and
nothing else: not another length, not the request with any one byte changed, not
a reply. The device sends no reply and no error to anything else, so a stray or
hostile datagram (the Beckhoff advisory of ADR-0084 is one empty packet) costs
the device nothing.

## Reply

The reply is the header (kind 2) followed by the body.

| Requirement | Offset | Field | Type | Description |
|-------------|--------|-------|------|-------------|
| **REQ-DW-discovery-010** | 6 | flags | `u8` | bit 0 is set when the device is protected (requires credentials) |
| **REQ-DW-discovery-011** | 7 | reserved | `u8` | `0` when encoding, ignored when decoding |
| **REQ-DW-discovery-012** | 8 | session protocol | `u32` | version of the session protocol |
| **REQ-DW-discovery-013** | 12 | session port | `u16` | TCP port of the session |
| **REQ-DW-discovery-014** | 14 | device id | 16 bytes | the 128 bits of the device id (its 32 lowercase hexadecimal digits); the crate carries them raw and formats nothing |
| **REQ-DW-discovery-015** | 30 | device name, model, runtime version | 3 strings | each a `u16` byte length followed by that many UTF-8 bytes, in this order |

**REQ-DW-discovery-016** The bits of the flags byte other than bit 0 are zero when
encoding and ignored when decoding, so that a later version can add a flag
without breaking a version-1 reader.

The reply has no field for the application, for busy, or for a secret
(ADR-0084). The runtime version is in the reply of a protected device too.

### Size

**REQ-DW-discovery-020** A reply is at most `MAX_REPLY_LEN` = 1024 bytes, so that
it fits one UDP datagram on any link without fragmentation. The device name
alone may be 64 characters, up to 256 UTF-8 bytes, hence the `u16` string
lengths.

**REQ-DW-discovery-021** The encoder refuses a reply that would exceed
`MAX_REPLY_LEN`, with an error, and never truncates a string.

**REQ-DW-discovery-022** The encoder refuses an output buffer that is too small,
with an error that says how many bytes are needed, and does not write a partial
reply.

**REQ-DW-discovery-023** The decoder refuses an input longer than `MAX_REPLY_LEN`
before it parses.

## Decoding is strict

The decoder accepts exactly the version-1 reply and names the first thing that
is wrong, one error variant for each:

| Requirement | Input | Error |
|-------------|-------|-------|
| **REQ-DW-discovery-030** | does not begin with the magic (checked on as many bytes as there are) | `BadMagic`: not IronPLC |
| **REQ-DW-discovery-031** | wire version other than 1 | `UnsupportedVersion(v)`: so that IronLink can say the device speaks another discovery |
| **REQ-DW-discovery-032** | kind other than 2 | `UnexpectedKind(k)` |
| **REQ-DW-discovery-033** | ends inside the header, a fixed field or a length prefix | `Truncated` |
| **REQ-DW-discovery-034** | a string length runs past the end | `StringPastEnd` |
| **REQ-DW-discovery-035** | a string is not UTF-8 | `InvalidUtf8` |
| **REQ-DW-discovery-036** | bytes follow the last string | `TrailingBytes` |

**REQ-DW-discovery-037** Every strict prefix of a valid reply fails to decode.

**REQ-DW-discovery-040** Decoding any byte string does not panic, and a byte
string that decodes encodes again to the same bytes, apart from the flag bits
and the reserved byte that decoding ignores.

**REQ-DW-discovery-041** Decoding an encoded reply gives the reply, for every
valid reply.

## Evolution

A new reply field is a new wire version. A new boolean is a new bit of the
flags byte, which a version-1 reader ignores. A new message kind is one more
value of the kind byte with its own encoding and decoding. The version is
checked in one place, the decoder of the header, so a newer device is reported
as `UnsupportedVersion` and never misread.
