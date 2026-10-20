# Build and Download Records

status: accepted
date: 2026-10-20

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
from which inputs, so that an unchanged build is skipped; and, later, what each
device received, so that the editor can compare the application on a device with
what it last sent by content and not by generation counters, which restart when
the device reboots. Both are engineering-side files: the device never reads
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
2. **One file for the build and the downloads.** A download is per connection,
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
  `<app>.build.iplcinfo` for the build record, and later
  `<app>.<connection>.download.iplcinfo` for the download record, one for each
  connection profile (a device reports no unique id, so the engineering side's
  name for the connection keys it). `RecordKind` in `ironplc-project` lists the
  kinds, and its path and version functions are exhaustive matches, so a kind
  without a name or a version does not compile. The UID table is the kind
  `Uids`, with its name unchanged.
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
- `compiler/ironplc-cli/src/cli/up_to_date_tests.rs`: the second build is up to
  date and leaves the container untouched, `--force`, a changed source, a changed
  option, a deleted or altered container, and a missing, damaged and
  other-version record.
