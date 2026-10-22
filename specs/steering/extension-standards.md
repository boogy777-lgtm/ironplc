# VS Code Extension Standards

This steering file defines the coding standards and conventions for the IronPLC
VS Code extension under `integrations/vscode/` (TypeScript). It covers README
synchronization and points to the extension-specific testing and error-code
rules.

> **Note**: This file covers *how to write extension code*. For the extension's
> CI test gates and structural invariants, see
> [extension-testing-requirements.md](extension-testing-requirements.md). For the
> `E####` extension error codes, see
> [problem-code-management.md](problem-code-management.md). For build/test
> commands, see [common-tasks.md](common-tasks.md).

## Applies To

This guidance is relevant when working with `integrations/vscode/**`.
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
`integrations/vscode/resources/problem-codes.csv`. Never hardcode error strings;
use the generated `formatProblem(ProblemCode.Name, context)` helper. The full
lifecycle is in [problem-code-management.md](problem-code-management.md).

## Shared Code for VS Code Extensions

`integrations/vscode-common/` is a private npm package (`ironplc-vscode-common`,
never published) holding code that more than one IronPLC VS Code extension needs.
Today that is locating the IronPLC toolchain (`findToolchainExecutable`,
`ToolchainEnvironment`, `pathForPlatform`). Put code there only when two
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
source that imports the package's path itself. The consumer builds the package
(`npm run build-common`) before its own `compile` and `esbuild`, and its
`just setup` and `just update` run the package's first. The package has its
own `just ci` (compile and unit tests), which the extension workflow runs. See
`integrations/vscode-common/README.md`.

## README Synchronization

The project has multiple README files that must stay synchronized:

- **Root `README.md`**: Main project overview, mission, progress, and capabilities
- **`integrations/vscode/README.md`**: VS Code Extension specific documentation for the Marketplace

**When updating the main README:**
1. Review if the extension README needs corresponding updates
2. The extension README should reflect the same capabilities/limitations
3. Keep the "warning" banner (`⚠`) consistent between both files
4. Ensure feature lists match (e.g., syntax highlighting, analysis capabilities)

**When updating the extension README:**
1. Keep it focused on VS Code-specific usage and features
2. Include extension settings, commands, and configuration
3. Reference the main documentation website for detailed information
