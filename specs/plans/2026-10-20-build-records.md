# Build and Download Records

## Goal

The engineering side remembers what it built and what it put on each device, as
CODESYS does with its `.compileinfo` file. Two records are kept beside the
application `<app>.iplc`:

- the **build record**: what the last successful Build produced, and from which
  inputs;
- the **download record**: per connection, what the device received at the last
  successful download or online change.

With them:

- Build skips a compile when nothing changed, and says the application is up to
  date. CODESYS prints "ProjectUpToDate".
- The editor knows, without asking the device, which application the device was
  last given.
- On connect, it compares the application of the device with that record by
  content, not by generation counters. The counters restart when the device
  reboots.

The online model is its own plan, after this one: login choices (full download,
online change, login without change) and Start/Stop. It builds on these records.
Parse-tree stage S4 comes after it (plan #142, owner's order).

## What CODESYS does (verified in the assemblies we have)

- **Identity of a build.**
  - `CodeId` is a CRC of the code.
  - `DataId` is a CRC of the signatures and of the layout of the retained and
    persistent variables.
  - Both are written in phase 6 (`Compiler35220.plugin/-/-.402.cs:196-219`).
- **The last build is kept in memory only.** Build sets it in memory
  (`CompilerPhaseControllerGenerateCode.cs:590-596`). After a restart, the
  "compiled set" falls back to the file (`CompiledSetStorage.cs:76-91`).
- **`.compileinfo` is the downloaded set.** It is written after a download or an
  online change (`LanguageModelManagerConsolidated.cs:2401-2413`, `UpdateDownloadContext`).
  - **Name:** `<project>.<application>.<appGuid>.compileinfo`, with a separate
    `.simulation` variant.
  - **Content:**
    - the two ids and their CRCs;
    - the signatures;
    - the variables and their locations;
    - the compiled code of each POU;
    - the memory layout;
    - the defines, the compile options and the libraries;
    - the simulation mode and the device id;
    - a project checksum.
- **Up to date.**
  - The project is compared with the last build (a fast project checksum, then a
    detailed comparison).
  - The project is also compared with the downloaded set. A change of simulation
    mode counts as not up to date (`DownloadedApplicationService.cs:140-150`).
  - The result says up to date or not, whether an online change is possible, and
    whether the fast online change is possible.
- **Failure handling.**
  - A missing file means a full build and a full download.
  - A corrupt file is deleted with a warning (`CompiledSetStorage.cs:250-257`).
  - A change of warning options, of the device or of the download mode clears it.
- **Other files.** `.precompileinfo` (precompile reuse, saved with the project)
  and `.bootinfo` (the boot copy) are separate files.

## Current state in IronPLC (sweep of `main` 0bb29097b)

- **Identity already in the container** (`container/src/header.rs:61-74`):
  - `content_hash` is BLAKE3 over the header, tasks, types, constants and code,
    without the debug section (`integrity.rs:54-83`). It plays the role of
    `CodeId`.
  - `layout_hash` covers the persistent part (`layout_hash.rs:45-77`, ADR-0073).
    It plays the role of `DataId`.
  - The debug section holds a BLAKE3 of each source file (`codegen/src/source_lookup.rs:95-103`).
- **No record of a build.** `ironplcc compile -o` always compiles. VS Code builds
  `<folder>.iplc` (`taskProviderLogic.ts:25-42`, `buildAreaLogic.ts:172-182`).
- **The download record exists only in VS Code workspace state.**
  - `ironplc.baseline` holds `{active, application, containerHash}`
    (`baselineLogic.ts:28-52`, `connection.ts:122-142`).
  - It is captured after Build & Commit and after an assembled Trial
    (`buildCommands.ts:109`, `:162-164`).
  - The comparison reads only the generation counters, not the hash
    (`baselineLogic.ts:61-68`). A device reboot restarts the counters, so it
    fails closed into E0015.
  - MCP keeps nothing (`mcp/src/tools/hot_edit.rs:11-25`).
- **The device does not report its identity.** `identity` gives the device panel
  and generations only (`runtime/src/commands.rs:157-178`, `:208-217`). The
  device keeps the committed bytes in A/B slots (`vm-cli/src/slot_store.rs:4-38`).
  It could report their `content_hash` and `layout_hash`, but does not.
- **One engineering-side record already exists:** `<stem>.uids.json`.
  - Its name comes from one function, `sidecar_path_for` (`project/src/sidecar.rs:409-416`).
  - It is versioned (`FORMAT_VERSION = 1`).
  - It is fail-soft: missing or malformed means an empty table (`sidecar.rs:260-271`).

## Architecture

- **One module for engineering records.** It lives in the crate `ironplc-project`
  (the engineering side; the device never reads these files).
  - A `RecordKind` enum: `Uids` (the existing sidecar, name unchanged), `Build`,
    `Download`.
  - One function derives each path from the application path. It is an
    exhaustive match, so a kind added later (boot copy, precompile record) has a
    name before it compiles.
  - Every record is versioned JSON.
  - Read is fail-soft. A missing file means no record. A malformed file or another
    version means no record plus one Text line that names the file, which is then
    rewritten, as CODESYS deletes a corrupt file.
- **Names (iplc family, owner).** Beside `<app>.iplc`:
  - `<app>.iplc` is the application (exists);
  - `<app>.build.iplcinfo` is the build record;
  - `<app>.<connection>.download.iplcinfo` is the download record, one per
    connection profile. The device reports no unique id, so the record is keyed
    by the engineering side's name for it, as CODESYS keys by application and
    simulation.
- **Build record.**
  - It holds the container identity: `content_hash`, `layout_hash`, `debug_hash`
    and the size.
  - It holds the input fingerprint:
    - the hash of each source;
    - the activated libraries with the hash of their text;
    - the effective `CompilerOptions`;
    - the message settings;
    - the UID table hash;
    - the compiler version;
    - the application name.
  - It is written by `ironplcc compile` after the container is written, in the
    same step.
- **Up to date.** `ironplcc compile` reads the build record first. If the
  fingerprint is equal and `<app>.iplc` exists with that `content_hash`, it does
  not compile. It prints the Build lines with "application is up to date" and
  exits 0. The fingerprint is computed from the inputs that the compile receives,
  in the function that builds them, so an input added to the compile changes the
  fingerprint. `--force` (Clean build) bypasses the skip.
- **Download record.**
  - It holds the container identity that was sent, the connection, the
    generations the device reported after the commit, and the time.
  - It is written by one function after the device acknowledged an assemble
    (Build & Commit, an assembled Trial) and after a session started with a
    container (`serve <app.iplc>` seeds the device).
  - VS Code calls it through a CLI subcommand, so the format has one writer.
  - It replaces the workspace-state `ironplc.baseline` (ΔR < 0).
- **The device reports its identity.** The `application` block of
  `identity`/`getStatus` gains `contentHash` and `layoutHash` of the normal
  artifact. These are additive optional fields. `SESSION_PROTOCOL_VERSION` stays
  1 if the client ignores unknown fields; that is checked in PR 3.
- **Connect compares content.** Device identity against the download record:
  - equal: match;
  - different: mismatch, and E0015 as today;
  - no record: none.

  A reboot no longer causes a false mismatch. What to offer on a mismatch belongs
  to the online model.

## Class / Inv / N+1

- **Class "engineering record file":** `.uids.json`, the build record, the
  download record. Later: boot copy, precompile record (path 2). Near case
  outside: the device's A/B slots and marker, which the device owns.
  - **Inv:** one path function (exhaustive over `RecordKind`), a version field,
    fail-soft read.
  - **Check:** an exhaustive match, and a test per kind for the missing,
    malformed and wrong-version cases.
  - **N+1:** a new kind is one enum variant plus its schema.
- **Class "input of a build":** sources, libraries, options, message settings, UID
  table, compiler version.
  - **Inv:** changing any input makes the build not up to date.
  - **Check:** a test changes each input once and asserts a compile. The options
    are covered by iterating the flag list that `CompilerOptions` is generated
    from, so a new flag is covered without an edit.
- **Class "download to a device":** Build & Commit, an assembled Trial, a session
  started with a container, MCP hot-edit assemble.
  - **Inv:** each path writes the download record through the one function.
  - **Check:** in TypeScript, the assemble acknowledgement is handled in one
    place, which calls the writer. A test fails if `ironplc.baseline` is still
    read or written anywhere. MCP has no file system project, so its row in the
    class table says "no record" with that reason.

## Prefactoring

- **PR 1 (prefactor).** `sidecar_path_for` becomes the record-path function over
  `RecordKind`, with only `Uids`. The fail-soft versioned JSON reader of the
  sidecar becomes the shared reader. Behaviour unchanged.
- Fix the stale module comment `container/src/integrity.rs:13` ("layout_hash is
  still not computed"). `container.rs:85-95` computes it.

## Design doc reference

- [Bytecode container format](../design/bytecode-container-format.md): the hashes.
- ADR-0053, ADR-0057: the UID sidecar.
- ADR-0073: the layout hash covers the persistent part.
- ADR-0063, ADR-0065: the engineering connection, pending edits on the IDE side.
- A new ADR, "Build and download records", is written in PR 2: the CODESYS
  reference, the names and the fail-soft rule.

## File map

- `compiler/project/src/sidecar.rs` → `records/` (`mod.rs` path and reader,
  `build.rs`, `download.rs`)
- `compiler/ironplc-cli/src/cli.rs`, `bin/main.rs` (`compile` skip, `--force`,
  subcommand `record-download`)
- `compiler/runtime/src/{commands,host}.rs`, `compiler/vm-cli/src/serve.rs`
  (identity hashes)
- `compiler/mcp/src/tools/hot_edit.rs` (class row only)
- `integrations/vscode/src/{baselineLogic,connection,buildCommands,buildLogic}.ts`
  and their tests, README
- `docs/reference/compiler/ironplcc.rst`, the editor docs and a problem page if a
  new code is needed
- `specs/adrs/NNNN-build-and-download-records.md`

## Tasks

### PR 1: one record-path function and reader (prefactor)
- [ ] `RecordKind { Uids }`, the path function, and the shared versioned fail-soft JSON reader; `.uids.json` behaviour unchanged
- [ ] Stale comment in `integrity.rs` fixed

### PR 2: build record and up-to-date Build (core)
- [ ] `RecordKind::Build`, its schema and the fingerprint built where the compile inputs are built
- [ ] `ironplcc compile` writes it after the container and skips an up-to-date build ("application is up to date"); `--force`
- [ ] Tests: each input changes the result; a missing, malformed or other-version record means a compile and a Text line; a deleted or altered `<app>.iplc` means a compile
- [ ] ADR; CLI and editor docs

### PR 3: device identity and download record (core)
- [ ] `contentHash` and `layoutHash` in the `application` block, from the normal artifact; client compatibility checked
- [ ] `RecordKind::Download` and the CLI subcommand that writes it
- [ ] VS Code: the assemble acknowledgement and the session start write the record; connect compares content; `ironplc.baseline` removed
- [ ] Tests: a reboot keeps a match; another container is a mismatch; no record is none

## Owner's answers

- **Q1. Names:** `<app>.build.iplcinfo` and `<app>.<connection>.download.iplcinfo`,
  accepted.
- **Q2. Format:** JSON, as the UID sidecar. The container already holds the code.

## Not included

- The login choices (full download, online change, login without change) and
  Start/Stop: the online-model plan.
- The precompile record (reuse of analysis results): path 2, only by measurement.
- The boot copy: the device's A/B slots already hold the boot application.
- MCP records: MCP has no project on disk.
