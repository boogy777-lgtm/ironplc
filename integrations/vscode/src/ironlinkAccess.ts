/**
 * Reaching the IronLink extension from the IronPLC extension.
 *
 * IronLink is optional: it is not published with this extension, so this
 * extension declares no dependency on it. It is found by its one id
 * (`IRONLINK_EXTENSION_ID`), activated, and its `exports` are read as the
 * typed API of the shared package. When any step fails, the answer is a reason
 * a person can act on, and the functions that need IronLink say why they are
 * unavailable. Vscode-free: the lookup is injected.
 */

import { IRONLINK_API_VERSION, IRONLINK_EXTENSION_ID, IronLinkApi } from './vscodeCommon';

/** What of an installed extension is needed here; `vscode.Extension` has this shape. */
export interface InstalledExtension {
  activate(): PromiseLike<unknown>;
}

/** Looks an extension up by id; `vscode.extensions.getExtension` has this shape. */
export type FindExtension = (id: string) => InstalledExtension | undefined;

/** The IronLink API, or why there is none. */
export type IronLinkAccess =
  | { available: true; api: IronLinkApi }
  | { available: false; reason: string };

export const IRONLINK_NOT_INSTALLED
  = 'Install the IronLink extension (ironplc.ironlink) to search the network for controllers.';

/** Whether `value` is the API of the version this extension was written for. */
function isSupportedApi(value: unknown): value is IronLinkApi {
  return typeof value === 'object'
    && value !== null
    && (value as { apiVersion?: unknown }).apiVersion === IRONLINK_API_VERSION;
}

/** Finds and activates IronLink, and returns its API. */
export async function accessIronLink(find: FindExtension): Promise<IronLinkAccess> {
  const extension = find(IRONLINK_EXTENSION_ID);
  if (extension === undefined) {
    return { available: false, reason: IRONLINK_NOT_INSTALLED };
  }
  let exported: unknown;
  try {
    exported = await extension.activate();
  }
  catch (err) {
    return {
      available: false,
      reason: `The IronLink extension did not start: ${err instanceof Error ? err.message : String(err)}`,
    };
  }
  if (!isSupportedApi(exported)) {
    return {
      available: false,
      reason: `The IronLink extension offers no API of version ${IRONLINK_API_VERSION}. Update the IronLink and IronPLC extensions to the same version.`,
    };
  }
  return { available: true, api: exported };
}
