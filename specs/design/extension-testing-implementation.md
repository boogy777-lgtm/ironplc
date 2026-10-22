# VS Code Extension Testing Architecture

This document describes the architecture for bringing the VS Code extension to the testing standards defined in [extension-testing-requirements.md](../steering/extension-testing-requirements.md).

## Overview

The extension's source code currently has three files:
- `src/extension.ts` (176 lines) — activation, compiler discovery, LSP client creation
- `src/iplcEditorProvider.ts` (364 lines) — custom `.iplc` bytecode viewer
- (test files under `src/test/`)

The core problem is that testable logic is locked inside VS Code-dependent code. The design extracts pure logic into testable modules, then adds tests at two levels: unit tests (fast, no VS Code) and expanded functional tests (slow, real VS Code instance).

## Module Structure

### New Files

The following new source files will be created:

**`src/iplcRendering.ts`** — Pure rendering functions and data interfaces extracted from `iplcEditorProvider.ts`:

- **Interfaces** (moved as-is): `DisassemblyResult`, `DisassemblyHeader`, `DisassemblyConstant`, `DisassemblyFunction`, `DisassemblyInstruction`
- **Functions** (moved as-is, add `export`): `escapeHtml`, `formatOffset`, `getOpcodeClass`
- **Methods extracted as functions** (currently private methods on `IplcEditorProvider`): `getErrorHtml`, `renderHeader`, `renderConstants`, `renderFunctions`, `getDisassemblyHtml`

After extraction, `iplcEditorProvider.ts` imports and calls these functions. The class retains only:
- `register()`, `openCustomDocument()`, `resolveCustomEditor()` — VS Code API integration
- `waitForClient()` — LSP lifecycle management

**Toolchain discovery** — Discovery with injectable dependencies, extracted from `extension.ts`. It lives in the shared package `integrations/vscode-common` (`src/toolchainDiscovery.ts`), which the IronPLC extensions import through `src/vscodeCommon.ts` (see [extension-standards.md](../steering/extension-standards.md#shared-code-for-vs-code-extensions)). `src/compilerDiscovery.ts` keeps only the compiler-specific `formatStartFailure`.

**`src/test/unit/testHelpers.ts`** — Factory functions for test data.

**`src/test/checkInvariants.ts`** — Structural invariant verification script.

### Directory Layout

```
src/
├── extension.ts                          # Modified: imports toolchain discovery from vscodeCommon.ts
├── iplcEditorProvider.ts                 # Modified: imports from iplcRendering.ts; accepts LanguageClientLike
├── iplcRendering.ts                      # NEW: pure rendering functions
├── compilerDiscovery.ts                  # NEW: compiler start-failure message
└── test/
    ├── checkInvariants.ts                # NEW: invariant verification script
    ├── unit/
    │   ├── iplcRendering.test.ts         # NEW: rendering unit tests
    │   ├── compilerDiscovery.test.ts     # NEW: start-failure message unit tests
    │   ├── iplcEditorProvider.test.ts    # NEW: mock-based editor provider tests
    │   └── testHelpers.ts               # NEW: test data factories
    └── functional/
        ├── suite/
        │   └── extension.test.ts         # Modified: add TwinCAT language detection tests
        └── resources/
            ├── valid.TcPOU               # NEW: TwinCAT POU test fixture
            ├── valid.TcGVL               # NEW: TwinCAT GVL test fixture
            └── valid.TcDUT               # NEW: TwinCAT DUT test fixture
```

## Key Interface Definitions

### `ToolchainEnvironment`

Defined in `integrations/vscode-common/src/toolchainDiscovery.ts`. Abstracts platform and configuration dependencies so `findToolchainExecutable` can be unit-tested without real filesystem or VS Code APIs. The injected `platform` also decides how paths are composed (`pathForPlatform`), so a test targeting one platform gives the same result on any host:

```typescript
export interface ToolchainEnvironment {
  platform: string;
  existsSync: (path: string) => boolean;
  getEnv: (name: string) => string | undefined;
  getConfig: (key: string) => string | undefined;
}
```

### `ToolchainDiscoveryResult`

Defined in `integrations/vscode-common/src/toolchainDiscovery.ts`. Return type for toolchain discovery:

```typescript
export interface ToolchainDiscoveryResult {
  path: string;
  source: string;
}
```

The public function signature is:

```typescript
export function findToolchainExecutable(
  env: ToolchainEnvironment,
  executable: string,
): ToolchainDiscoveryResult | undefined;
```

`extension.ts` creates a `ToolchainEnvironment` from real `process`, `fs`, and `vscode.workspace.getConfiguration` and calls `findToolchainExecutable(env, 'ironplcc')`.

### `LanguageClientLike`

Defined in `src/iplcEditorProvider.ts`. A minimal interface for the LSP client methods actually used, enabling mock-based unit testing:

```typescript
export interface LanguageClientLike {
  isRunning(): boolean;
  sendRequest(method: string, params: any): Promise<any>;
  onDidChangeState(listener: (e: { newState: number }) => void): { dispose(): void };
}
```

The `IplcEditorProvider` constructor accepts `LanguageClientLike` instead of `LanguageClient`. The real `LanguageClient` already satisfies this interface, so no changes are needed at the call site.

## Shared HTML Infrastructure

Both `getErrorHtml` and `getDisassemblyHtml` produce full HTML documents with overlapping CSS (body styles, VS Code CSS variables). The shared CSS is extracted into a constant and a helper function:

```typescript
const BASE_STYLES = `
  body {
    background: var(--vscode-editor-background);
    color: var(--vscode-editor-foreground);
    font-family: var(--vscode-font-family);
    padding: 20px;
  }
`;

export function wrapInHtmlDocument(body: string, extraStyles: string = ''): string {
  return `<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <style>${BASE_STYLES}${extraStyles}</style>
</head>
<body>
  ${body}
</body>
</html>`;
}
```

`getErrorHtml` and `getDisassemblyHtml` then call `wrapInHtmlDocument` with their specific body content and any additional styles, eliminating the duplicated HTML boilerplate.

## Test Infrastructure Architecture

### Unit Test Runner

Unit tests live in `src/test/unit/` and use a standalone mocha runner that does NOT use `@vscode/test-electron`. This ensures unit tests run fast without launching a VS Code instance.

The `compile` step already compiles all TypeScript under `src/` to `out/` (tsconfig has `"outDir": "out"` and `"rootDir": "src"`), so the unit tests will be compiled alongside everything else and the `out/test/unit/` glob will find them.

### Coverage with c8

The `test:unit` npm script uses c8 for coverage:

```json
"test:unit": "c8 --check-coverage --lines 80 --include 'out/iplcRendering.js' --include 'out/compilerDiscovery.js' mocha 'out/test/unit/**/*.test.js'"
```

The `--include` flags restrict coverage measurement to the extracted unit-testable modules. Without them, `c8` would measure coverage of all loaded files (including test helpers and test files themselves), producing misleading numbers.

### Test Helpers

Factory functions in `testHelpers.ts` return valid default objects, allowing individual tests to override specific fields via `Partial<T>` spread patterns (e.g., `createTestHeader({ maxStackDepth: 16 })`).

The test helpers also export numeric values for `vscode-languageclient`'s `State` enum so mock implementations don't need to import that package:

```typescript
export const CLIENT_STATE_STOPPED = 1;
export const CLIENT_STATE_RUNNING = 2;
```

### Invariant Check Script

`src/test/checkInvariants.ts` reads `package.json` and verifies that all declared capabilities (languages, commands, custom editors) appear in test files. It supports an `EXCEPTIONS` map for capabilities that are intentionally untested, with documented justifications.

## New Dependencies

### Required

- **c8** (devDependency) — Node.js native coverage tool. Used by the `test:unit` script to enforce the coverage threshold. No configuration file needed; CLI flags are sufficient.

### Not Required

- No mocking library — the mocks needed for the editor provider tests are simple enough to implement inline (3-5 method stubs).
- No additional test framework — mocha is already a devDependency.

## File Summary

### New Files
| File | Purpose |
|------|---------|
| `src/iplcRendering.ts` | Pure rendering functions and data interfaces |
| `src/compilerDiscovery.ts` | Compiler start-failure message (discovery itself is in `integrations/vscode-common`) |
| `src/test/unit/iplcRendering.test.ts` | Unit tests for rendering |
| `src/test/unit/compilerDiscovery.test.ts` | Unit tests for the start-failure message |
| `src/test/unit/iplcEditorProvider.test.ts` | Mock-based tests for editor provider |
| `src/test/unit/testHelpers.ts` | Test data factory functions |
| `src/test/checkInvariants.ts` | Structural invariant verification script |
| `src/test/functional/resources/valid.TcPOU` | TwinCAT POU test fixture |
| `src/test/functional/resources/valid.TcGVL` | TwinCAT GVL test fixture |
| `src/test/functional/resources/valid.TcDUT` | TwinCAT DUT test fixture |

### Modified Files
| File | Change |
|------|--------|
| `src/iplcEditorProvider.ts` | Import from `iplcRendering.ts`; accept `LanguageClientLike` interface |
| `src/extension.ts` | Import `findToolchainExecutable` from `vscodeCommon.ts`; create `ToolchainEnvironment` adapter |
| `src/test/functional/suite/extension.test.ts` | Add TwinCAT language detection tests |
| `package.json` | Add `test:unit` script; add `c8` devDependency |
| `justfile` | Add `check-invariants`, `test-unit` targets; update `ci` target |
