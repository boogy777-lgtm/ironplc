# VS Code Extension Standards

This steering file defines the coding standards and conventions for the IronPLC
VS Code extensions (TypeScript): the IronPLC extension under `integrations/vscode/`
and the IronLink extension under `integrations/ironlink/`. It covers README
synchronization and points to the extension-specific testing and error-code
rules.

> **Note**: This file covers *how to write extension code*. For the extension's
> CI test gates and structural invariants, see
> [extension-testing-requirements.md](extension-testing-requirements.md). For the
> `E####` extension error codes, see
> [problem-code-management.md](problem-code-management.md). For build/test
> commands, see [common-tasks.md](common-tasks.md).

## Applies To

This guidance is relevant when working with `integrations/vscode/**`,
`integrations/ironlink/**` and `integrations/vscode-common/**`.
Cross-component process rules — planning, prefactoring, the git workflow — live
in [development-standards.md](development-standards.md).

## Testing and Coverage

The extension enforces its own coverage threshold (80% on unit-testable modules)
and a set of structural invariants that fail the build when a declared
capability (language, command, custom editor) ships without a test. These gates
and the rules for adding new capabilities are defined in
[extension-testing-requirements.md](extension-testing-requirements.md). Do not
restate them here.

## Error Codes

Extension errors use the `E####` prefix and are generated from
`integrations/vscode-common/resources/problem-codes.csv`, the one catalog shared by
every IronPLC VS Code extension. Never hardcode error strings; use the generated
`formatProblem(ProblemCode.Name, context)` helper, imported from `./vscodeCommon`. The full
lifecycle is in [problem-code-management.md](problem-code-management.md).

## Shared Code for VS Code Extensions

`integrations/vscode-common/` is a private npm package (`ironplc-vscode-common`,
never published) holding code that more than one IronPLC VS Code extension needs.
Today that is locating the IronPLC toolchain (`findToolchainExecutable`,
`ToolchainEnvironment`, `pathForPlatform`), the JSON-lines framing and
in-order request matching for a `serve` child process (`LineTransport`,
`StdioLineTransport`, `LineRequestChannel`), the check that an extension
reaches the package through one module (`findGatewayViolations`) and the `E####` problem catalog
(`problem-codes.csv`, its generator `scripts/generate-problems.js`, the generated
`ProblemCode`, `PROBLEM_MESSAGES` and `formatProblem`, and `problemHelpUrl`); the
package's `precompile` generates `src/problems.ts`, which is gitignored. Put code there only when two
extensions need it; extension-specific logic, anything that imports `vscode`,
and compiler-specific messages stay in the extension. The package has no
runtime dependencies.

A consumer does not declare an npm dependency on it. It reaches the package's
compiled output (`integrations/vscode-common/out/index`) through exactly one
module, `src/vscodeCommon.ts` (`export * from '../../vscode-common/out/index';`),
and every other source and test imports from that module. `src/` and `out/` sit
at the same depth, so the one relative path resolves for tsc, for Node and for
esbuild; the extension is bundled, so the package is compiled into
`out/extension.js` and is not packaged into the VSIX. `checkInvariants` fails a
source that imports the package's path itself (it calls the package's
`findGatewayViolations`). The consumer builds the package
(`npm run build-common`) before its own `compile` and `esbuild`, and its
`just setup` and `just update` run the package's first. The package has its
own `just ci` (compile and unit tests), which the extension workflow runs. See
`integrations/vscode-common/README.md`.

## The IronLink Extension

`integrations/ironlink/` is the second extension: **IronLink** (id
`ironplc.ironlink`, package `ironlink`), the VS Code part of the `ironlink`
program that finds controllers ([ironlink-vscode-extension.md](../design/ironlink-vscode-extension.md)).
It follows the same rules as the IronPLC extension, and these differ:

- **Independent.** It works without the IronPLC extension and has its own
  setting for the program (`ironlink.path`, a directory). It finds `ironlink`
  with the shared `findToolchainExecutable`, the same discovery as the IronPLC
  extension.
- **Thin client.** It starts `ironlink serve` as a child process and renders its
  answers. Decisions (what is routed, what conflicts, what an action may do) are
  the program's; the extension holds no rule of its own. A device action is
  available or not by one pure function (`decideDeviceAction`), and the tree is
  built by a pure function (`buildNetworkTree`); neither imports `vscode`.
- **Hands off, never writes project data.** An action that belongs to IronPLC
  (Choose) runs an IronPLC command named by the table of handoff actions in
  `integrations/vscode-common` (`HANDOFF_ACTIONS`). Each member declares its
  command id, its argument type and the reason shown while the command is
  missing. The extension tests for the command id with `getCommands`, never for
  the id of the IronPLC extension, which has two (Open VSX and the Marketplace).
  The IronPLC extension imports the same table to register the command.
- **Same version, same engine.** `version` and `engines.vscode` equal those of
  `integrations/vscode`, and `checkInvariants` fails when they differ. The root
  `justfile` `version` recipe sets both.
- **Its own invariants.** `checkInvariants` also checks that each command is
  registered and tested, that every device action is on the device row and hidden
  from the command palette, that each `viewItem` a menu tests is a context value
  the tree builder makes, that each `REQ-IL-ironlink-vscode-NNN` of the design
  document is named by a test and the reverse, and, through the shared
  `findGatewayViolations`, that the package is reached through `src/vscodeCommon.ts`
  only.
- **Built and tested in CI, published nowhere.** `partial_ironlink_extension.yaml`
  builds and tests the shared package and the extension, packages the VSIX and the
  SBOM and uploads them as build artifacts. There is no marketplace step.

## README Synchronization

The project has multiple README files that must stay synchronized:

- **Root `README.md`**: Main project overview, mission, progress, and capabilities
- **`integrations/vscode/README.md`**: VS Code Extension specific documentation for the Marketplace
- **`integrations/ironlink/README.md`**: IronLink extension documentation; it describes the extension only and does not repeat the main README

**When updating the main README:**
1. Review if the extension README needs corresponding updates
2. The extension README should reflect the same capabilities/limitations
3. Keep the "warning" banner (`⚠`) consistent between both files
4. Ensure feature lists match (e.g., syntax highlighting, analysis capabilities)

**When updating the extension README:**
1. Keep it focused on VS Code-specific usage and features
2. Include extension settings, commands, and configuration
3. Reference the main documentation website for detailed information
