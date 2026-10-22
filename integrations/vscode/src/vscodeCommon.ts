/**
 * The one place that knows where the code shared with IronPLC's other VS Code
 * extensions (`integrations/vscode-common`) lives. Every other module imports
 * it from here, so moving the package changes this line only; the
 * invariants check fails on a module that imports the package's path itself.
 *
 * `src/` and `out/` sit at the same depth, so the one relative path resolves
 * for tsc (it reads `vscode-common/out/index.d.ts`), for Node at runtime and
 * for esbuild. `npm run build-common` builds the package first.
 */
export * from '../../vscode-common/out/index';
