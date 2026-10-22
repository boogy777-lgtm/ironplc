import * as path from 'path';

/**
 * The path module for a target `platform`: `path.win32` for `'win32'`, else
 * `path.posix`. Toolchain locations are composed for the platform the
 * extension reports (an injected value, so unit tests can target one
 * platform from any host), never for the host the code happens to run on.
 * Every site that joins or splits a path for an injected platform goes
 * through this function; Node's host-dependent `path.join` is reserved for
 * paths that really are on the host.
 */
export function pathForPlatform(platform: string): typeof path.posix {
  return platform === 'win32' ? path.win32 : path.posix;
}
