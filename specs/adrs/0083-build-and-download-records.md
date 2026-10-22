# Build and Download Records

status: accepted
date: 2026-10-20
amended: 2026-10-22 (the download record is keyed by the id of the device and not by the name of the connection profile; the record is version 2; records under the former name are not read)

## Context and Problem Statement

`ironplcc compile` compiles every time, whether or not anything changed, and
neither the command line nor the editor remembers what a build produced. The
only memory on the engineering side is the stable variable UID table
(`<stem>.uids.json`, [ADR-0053](0053-stable-variable-ids-for-declaration-level-hot-edit.md)). CODESYS
remembers more, and uses it:

- A build with nothing changed says the project is up to date and does not
  compile (the message `ProjectUpToDate`). The project is compared with the last
  build by a fast project checksum, then by a detailed comparison.
- The file `.compileinfo` holds the *downloaded set*: the identities of the code
  and of the data (CRCs), the signatures, the variables, the compiled code, the
  memory layout, the compile options and the libraries. CODESYS writes it after
  a download or an online change (`UpdateDownloadContext`). The last build
  itself is kept in memory (`CompilerPhaseControllerGenerateCode.cs:590-596`)
  and, after a restart, read back from the file (`CompiledSetStorage.cs:76-91`).
- A missing file means a full build and a full download. A corrupt file is
  deleted with a warning (`CompiledSetStorage.cs:250-257`).

IronPLC needs the same two memories: what the last successful build produced and
from which inputs, so that an unchanged build is skipped; and what each device
received, so that the editor can compare the application on a device with what
it last sent by content and not by generation counters, which restart when the
device reboots. Until now the editor kept the second one in its workspace state
(`ironplc.baseline`: the generation counters and a SHA-256 of the container) and
compared only the counters, so a rebooted device was reported as changed. Both are engineering-side files: the device never reads
them. The device's own files (its slots and marker) are a different class and
stay with the device.

## Decision Drivers

- **Content, not time.** A decision that nothing changed must not depend on file
  modification times, which a checkout or a copy changes.
- **A forgotten input must not pass silently.** A build skipped because an input
  was not compared produces a container that is wrong without a message.
- **A bad file is not a failure.** A missing, damaged or foreign record costs one
  build and one line, never a stuck project.
- **One path function and one reader.** Every record is found and read in one
  place, so a new record is one case and not a new convention.
- **The command line is the writer.** The editor and other tools call
  `ironplcc`, so the format has one writer.

## Considered Options

1. **A family of engineering record files beside the application, one
   path function, versioned JSON, fail-soft read (chosen).**
2. **One file for the build and the downloads.** A download is per device,
   and a build record would be rewritten by every download and the other way
   around.
3. **State kept by the editor only** (what `ironplc.baseline` does for the
   download). The command line, the continuous integration pipeline and the MCP
   server cannot see it.
4. **Modification times, as `make` does.** A copy or a checkout changes them
   without changing the content, and a restored older file does not change the
   decision.

## Decision Outcome

Chosen option: **a family of engineering record files**, because it gives the
content comparison with one place to find, read and version every record.

- **Names.** The record sits beside `<app>.iplc` and is named for its kind:
  `<app>.build.iplcinfo` for the build record, and
  `<app>.<deviceId>.download.iplcinfo` for the download record, one for each
  device (every device generates an id of its own, once, and keeps it; the
  record says what this project last put on that device). `RecordKind` in `ironplc-project` lists the
  kinds, and its path and version functions are exhaustive matches, so a kind
  without a name or a version does not compile. The UID table is the kind
  `Uids`, with its name unchanged.
- **The device id in a file name.** `<deviceId>` is the id of the device: 32
  lowercase hexadecimal digits, held by one type in `ironplc-project`
  (`DeviceId`) that has no other way to be made than the check of those digits.
  The digits are valid in a file name on every system and contain no dot, so no
  id can write the suffix of another kind, and the name needs no encoding. A
  device id has no letter case to lose, so two devices never share a file. The
  record holds the id (`deviceId`), and a reader treats a record of another id
  (a copied or renamed file) as malformed. The name of the connection profile is
  not part of the record: a profile is a way to reach a device, and a renamed
  profile or a changed address finds the record of the same device again.
- **Records under the former name.** Before 2026-10-22 the record was
  `<app>.<connection>.download.iplcinfo`, keyed by the name of the connection
  profile, because no device had an id. Those files are not read and not
  migrated: a device has no record until the next download to it writes
  `<app>.<deviceId>.download.iplcinfo`, and the editor shows it as "no record",
  which is no check. The download record is version 2 (it holds `deviceId` where
  version 1 held `connection`), so a version 1 file is malformed even when it is
  found under an id's name. A device that reports no id (a host that has none, or
  a device that predates the field) has no record at all: the editor does not
  write one and says why in the connection channel.
- **Format.** Every record is JSON with an integer `version`. The reader accepts
  the version of the kind and nothing else. A missing file is no record. A file
  that is not JSON, has another version or has a shape that is not the kind's is
  *malformed*: there is no record, one line (a message of category Text, without
  a code) names the file, and the file is rewritten after the build, which
  corresponds to CODESYS deleting a corrupt file. Only a genuine I/O failure is
  a problem.
- **The build record** holds the identity of the container (`contentHash`,
  `layoutHash`, `debugHash` and the size, read from the header of the bytes that
  were written), the name of the application, the version of the compiler, and
  the *inputs* of the build, itemized so that a record says which input changed:
  the path and the hash of each source, the files and their hashes of each
  activated library, every setting of the compiler options (each flag, each
  policy, the lists of disabled warnings and of warnings reported as errors), the
  hash of the UID table, and the name of the project. The output path is not an
  input; it is the anchor of the record.
- **The inputs are read from what the compile receives.** One function builds
  them from the project and the options, and the settings of the options come
  from a list that the macro declaring the options generates, with an exhaustive
  destructuring of the options, so a new flag or policy is covered without an
  edit and a new field does not compile until it is listed.
- **Up to date.** `ironplcc compile` writes the build record after the container
  is written. The next `compile` reads it first. When its inputs equal those of
  this build and `<app>.iplc` has the hashes and the size of the record, the
  command does not parse, analyze or generate. It writes the line that starts the
  build and the line `The application is up to date`, and exits with status 0. A
  failed build writes no container and so no record, and leaves the record that is
  there: it still describes the container on disk. `--force` builds regardless
  and writes the record.

- **The download record** holds the id of the device, the name of the
  application (the one the build record gives when the application has a build
  record, else the stem of the container), the identity of the container that was
  sent (`contentHash`, `layoutHash`, `debugHash` and the size, read from its
  header), the generation counters that the device reported after the download
  (`active`, `normal`, `application`; they describe one session of the device,
  which a reboot restarts), and the time in UTC. `ironplcc record-download` is
  its one writer. It refuses to write when the caller gives the content hash that
  the device reports and it is not the hash of the container: the record says what
  the device received, and a device that runs something else received something
  else. `ironplcc record-download-path` prints the path of the record, so the
  editor reads the file but never derives its name.
- **The device reports what it runs.** The `application` block of the answers to
  `identity` and `getStatus` gains `contentHash` and `layoutHash`: the hashes in
  the header of the normal (committed) artifact the host holds, as lowercase
  hexadecimal, read from the header and not recomputed. They are optional on the
  wire and absent when the header carries no hash. They follow the normal
  artifact through an assemble and stay those of the normal artifact while a
  candidate runs under Test. The session protocol stays at version 1, because a
  server adds optional fields and a client ignores the fields it does not know:
  no deserializer of these payloads in the Rust workspace denies unknown fields,
  the parser of the editor reads the fields it names, and a test on each side
  parses a payload with the fields, without them and with a field of a later
  server.
- **Connect compares content.** The editor compares the `contentHash` that the
  device reports with the record of the connection. A record and the same hash is
  a match, and a reboot keeps it. A record and another hash, or a device that
  reports none (it predates the field), is a mismatch, which is the refusal
  E0015 as before: what a device that cannot say what it runs holds is unknown,
  and the editor does not build over it. No record is no check.
- **The download paths.** Every path that puts a container on a device records the
  download through the one writer, with the application container as the anchor of
  the record (a stdio connection whose program is a container: that container; any
  other connection: the container the build task writes in the first workspace
  folder, `<folder>/<folder>.iplc`) and the file that was sent as the container
  whose identity is recorded.

  | Path | Record |
  |---|---|
  | Build & Commit (the assemble the device acknowledged) | Recorded, with the device's status after the assemble. |
  | Build & Trial, resolved by assemble | Recorded, the same function. |
  | A session started with a container (a stdio connection whose program is a `.iplc`, `ironplcvm serve <app.iplc>`) | Recorded when the device reports the hash of that container, which the writer checks. A device that kept a committed application boots that one and not the file, so the file alone does not show what the device holds. |
  | A stdio connection whose program is a source file | No record. The session compiles the source to a container of its own in the temporary directory; the engineer has no file for it, and there is no application container to name a record after. |
  | MCP hot-edit assemble | No record: MCP has no project on disk. |

  The editor handles the acknowledgement of an assemble in one function, and a
  test fails if the workspace state `ironplc.baseline` is read or written anywhere
  in the extension, if the writer command is built outside one module, or if the
  extension derives the name of a record file.

### Consequences

- Good, because an unchanged project costs a read of the files and a hash, and
  the decision is by content.
- Good, because a record that cannot be used costs one build, and the message
  says which file.
- Good, because the download record, the boot copy and a precompile record are
  one case of `RecordKind` each, with the path, the version and the reader
  already written.
- Bad, because the wording `The application is up to date` is not verified
  against the CODESYS text of `ProjectUpToDate`.
- Bad, because a library that is changed on disk and a compiler that is changed
  in a way that its version does not show (a development build) are compared
  only by the library files and by the version string. A development build that
  changes code generation needs `--force`.
- Good, because a device reboot no longer reads as a changed application, and a
  device that was changed elsewhere is found by what it runs.
- Good, because the format of the download record has one writer and one rule for
  its name, and the editor, the command line and later tools share them.
- Bad, because a device that predates `contentHash` cannot be compared, so with a
  record it is refused until it is updated.
- Good, because the download record follows the device: renaming a profile or
  reaching the device by another address finds the same record, and two profiles
  for one device share it.
- Bad, because the download records written before the key changed (under the name
  of a connection profile) are left behind and unread: each device is without a
  record, and so unchecked, until the next download to it.
- Bad, because a device that reports no id cannot have a record.
- Bad, because the connection of a stdio profile that runs a source program is
  never checked.
- Neutral: the record holds the absolute paths of the sources, as the debug
  section of the container does, so a moved project builds once.

### Confirmation

- `compiler/project/src/build_record/tests.rs`: a test for each input (source
  text, a source added or moved, library text, the name of the project, the
  version of the compiler, the UID table, each list of warnings), a loop over
  every flag and over every alternative of every policy of the options, the
  reading of a record with a missing key or a wrong value, and the comparison
  with a missing, altered and foreign container.
- `compiler/project/src/records.rs`: the missing, malformed and wrong-version
  cases for each kind.
- `compiler/project/src/device_id.rs`: the one check of a device id.
- `compiler/project/src/records.rs` and `compiler/project/src/download_record/tests.rs`:
  the file name of a device (its id, two ids never sharing a file, never the name
  of another kind), the record that is written and read back, the record of
  another device, the former `connection` shape, another version and a damaged
  shape, the refusal when the device runs another container, and the
  time format.
- `compiler/runtime/tests/commands_acceptance.rs` and `compiler/vm-cli/src/serve_tests.rs`:
  the hashes of the normal artifact after load, during a trial, after a commit and
  after a restart, and the payload with and without them.
- `compiler/ironplc-cli/tests/cli_record_download.rs`: `record-download` and
  `record-download-path` as the editor runs them, the missing file, the file that
  is not a container, a device id that is not 32 lowercase hexadecimal digits and the hash that is not the container's.
- `integrations/vscode/src/test/unit/baselineLogic.test.ts`: a reboot keeps a
  match, another container is a mismatch, no record is none, a device that reports
  no hash is a mismatch, the plan of each download path, a device that reports no
  id (nothing recorded, with the reason), a renamed profile (the same plan), and
  the guards above.
- `compiler/ironplc-cli/src/cli/up_to_date_tests.rs`: the second build is up to
  date and leaves the container untouched, `--force`, a changed source, a changed
  option, a deleted or altered container, and a missing, damaged and
  other-version record.
