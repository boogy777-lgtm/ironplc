# ironplc-vscode-common

Code shared by IronPLC's VS Code extensions. The package is private (never
published to a registry) and is consumed by relative import, not through an npm
dependency; see
[extension-standards.md](../../specs/steering/extension-standards.md#shared-code-for-vs-code-extensions)
for what belongs here.

## Contents

- `findToolchainExecutable(env, executable)` finds a toolchain executable
  (`ironplcc`, `ironplcvm`, ...) in this order: the `path` setting, the
  `IRONPLC` environment variable, Homebrew (macOS), and
  `%LOCALAPPDATA%\Programs\IronPLC Compiler\bin` (Windows). It returns
  `{ path, source }` or `undefined`.
- `ToolchainEnvironment` is the injected platform, filesystem, environment and
  settings that make discovery unit-testable.
- `pathForPlatform(platform)` returns `path.win32` or `path.posix`. Compose a
  path for an injected platform with it, never with the host `path.join`.
- `executableFileName(base, platform)` appends `.exe` on Windows.
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
