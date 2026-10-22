/**
 * The words both IronPLC VS Code extensions use for a controller found on the
 * network: how long ago it was seen, where it answers and a short form of its
 * device id. Written once so the Network view of IronLink and the controller
 * picker of the IronPLC extension read alike.
 */

/** How many hexadecimal digits of the device id the short form shows. */
const SHORT_ID_LENGTH = 8;

/** `address:port`, with an IPv6 address in brackets. */
export function socketAddress(address: string, port: number): string {
  return address.includes(':') ? `[${address}]:${port}` : `${address}:${port}`;
}

/** The first digits of a device id. */
export function shortDeviceId(deviceId: string): string {
  return deviceId.slice(0, SHORT_ID_LENGTH);
}

/** How long ago `seenAt` was at `nowMs`: `just now`, `12 s ago`, `3 min ago`, `2 h ago`, `4 d ago`. */
export function formatAge(seenAt: number, nowMs: number): string {
  const seconds = Math.floor(Math.max(0, nowMs - seenAt) / 1000);
  if (seconds < 1) {
    return 'just now';
  }
  if (seconds < 60) {
    return `${seconds} s ago`;
  }
  const minutes = Math.floor(seconds / 60);
  if (minutes < 60) {
    return `${minutes} min ago`;
  }
  const hours = Math.floor(minutes / 60);
  if (hours < 24) {
    return `${hours} h ago`;
  }
  return `${Math.floor(hours / 24)} d ago`;
}
