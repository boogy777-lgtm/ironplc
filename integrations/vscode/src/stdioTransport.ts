/**
 * Where the `ironplcvm serve` child of the engineering connection (ADR-0063)
 * and of the hot-edit session lives. The child's stdin/stdout framing, one
 * JSON command per line, is `StdioLineTransport` in the shared package. Like
 * `tcpTransport.ts`, the module is vscode-free so the connection methods can
 * use it and unit tests can load it.
 */

import { executableFileName, pathForPlatform } from './vscodeCommon';

/** The VM executable name on `platform` (`.exe` on Windows). */
export function vmFileName(platform: string): string {
  return executableFileName('ironplcvm', platform);
}

/** The VM executable that sits next to the compiler at `compilerPath`. */
export function vmPathBeside(compilerPath: string, platform: string): string {
  const platformPath = pathForPlatform(platform);
  return platformPath.join(platformPath.dirname(compilerPath), vmFileName(platform));
}
