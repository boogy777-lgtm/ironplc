import { ToolchainDiscoveryResult } from './vscodeCommon';

/**
 * Builds the user-facing message shown when the language client fails to
 * launch the compiler. A common cause is a compiler binary built for a
 * different platform (for example, a Linux binary on Windows produces
 * "spawn ENOEXEC"), so the message includes the resolved path and the
 * discovery source to help the user verify the path that was actually used.
 */
export function formatStartFailure(result: ToolchainDiscoveryResult, error: unknown): string {
  const reason = error instanceof Error ? error.message : String(error);
  return 'IronPLC failed to start the compiler at "' + result.path
    + '" (source: ' + result.source + '): ' + reason;
}
