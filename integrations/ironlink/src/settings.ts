/**
 * The settings of the extension that shape a command. Vscode-free: the glue
 * reads the raw values from the configuration, and this module decides what
 * they mean.
 */

import { BrowseOptions } from './ironlinkProtocol';

/**
 * The browse options the user set (`ironlink.browse.timeoutMs` and
 * `ironlink.browse.attempts`). A value that is not set (`null`, the default),
 * or is not a number, is left out so the program applies its own default and
 * its own range check: the extension holds neither.
 */
export function browseOptionsFromSettings(timeoutMs: unknown, attempts: unknown): BrowseOptions {
  return {
    ...(typeof timeoutMs === 'number' ? { timeoutMs } : {}),
    ...(typeof attempts === 'number' ? { attempts } : {}),
  };
}
