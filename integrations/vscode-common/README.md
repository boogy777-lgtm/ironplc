# ironplc-vscode-common

Code shared by IronPLC's VS Code extensions. The package is private (never
published to a registry) and is consumed by relative import, not through an npm
dependency; see
[extension-standards.md](../../specs/steering/extension-standards.md#shared-code-for-vs-code-extensions)
for what belongs here.

## Contents

- `findToolchainExecutable(env, executable)` finds a toolchain executable
  (`ironplcc`, `ironplcvm`, ...) in this order: the `path` setting, the
  `IRONPLC` environment variable, Homebrew (macOS),
  `%LOCALAPPDATA%\Programs\IronPLC Compiler\bin` (Windows), and finally each
  directory of the system `PATH` (`source` is `path`; `PATH` is split with the
  delimiter of the injected platform and empty entries are skipped). It returns
  `{ path, source }` or `undefined`.
- `ToolchainEnvironment` is the injected platform, filesystem, environment and
  settings that make discovery unit-testable.
- `pathForPlatform(platform)` returns `path.win32` or `path.posix`. Compose a
  path for an injected platform with it, never with the host `path.join`.
- `executableFileName(base, platform)` appends `.exe` on Windows.
- `LineTransport` is the line-oriented channel a protocol client speaks over
  (`sendLine`, `onLine`, `onExit`). `StdioLineTransport` frames a child
  process's stdio as one JSON command per line. `LineRequestChannel` matches
  each response line to its request in order and fails the pending requests
  when the peer exits; the protocol that owns it supplies the errors. The
  hot-edit session of the IronPLC extension and the `ironlink serve` client
  of the IronLink extension use them.
- `findGatewayViolations(sourceRoot)` is the check each extension's
  `checkInvariants` applies to enforce the one-module rule below.
- `HANDOFF_ACTIONS` is the table of actions that one IronPLC extension offers and
  another performs. Each member of `HandoffArguments` declares the IronPLC
  command id, its argument type and the reason shown while the command does not
  exist; today `choose` -> `ironplc.chooseController` with
  `{ deviceId, address, sessionPort }`. IronLink runs the command
  (`runHandoff`) and tests for it by command id (`isHandoffAvailable`); the
  IronPLC extension registers it.
- The `E####` problem catalog, shared by every IronPLC VS Code extension:
  `resources/problem-codes.csv` is the registry, `scripts/generate-problems.js`
  generates `src/problems.ts` from it (the package's `precompile`; the file is
  gitignored), and the package exports `ProblemCode`, `PROBLEM_MESSAGES` and
  `formatProblem(code, context?)`. `problemHelpUrl(code, version)` builds the
  documentation URL of any `P`, `V`, `E` or `L` code. See
  [problem-code-management.md](../../specs/steering/problem-code-management.md)
  for how to add a code.

## Use from an extension

An extension builds the package before compiling, then reaches its compiled
output from exactly one module, so the relative path is written once. In
`integrations/vscode` that module is `src/vscodeCommon.ts`:

```ts
export * from '../../vscode-common/out/index';
```

`src/` and `out/` sit at the same depth, so this path resolves for tsc (it reads
`out/index.d.ts`), for Node at runtime and for esbuild. Every other module
imports from `./vscodeCommon`. The extension's `build-common` script
(`npm --prefix ../vscode-common run compile`) runs before its `precompile`,
`esbuild-base` and `test-compile`, and its `just setup` and `just update` run the
package's first. Because the extension is bundled with esbuild, the package is
compiled into the bundle and is not shipped in the VSIX.

## Develop

```
just setup      # npm ci
just ci         # generate the catalog, compile and run the unit tests
```
