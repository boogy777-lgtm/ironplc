/**
 * The text, tooltip and command of the connection status item, from the
 * connection model and the controller chosen for the project (ADR-0084).
 * Disconnected, the item names the chosen controller and a click connects to it;
 * in every other state it behaves as it always did. It shows what exists and no
 * placeholders: nothing is said of the equality of the application or of Run and
 * Stop before a controller is connected. Vscode-free.
 */

import { ConnectionState } from './connectionState';
import { DevicePanelModel } from './devicePanelLogic';
import { formatStatusDetail } from './hotEditSession';
import { HANDOFF_ACTIONS } from './vscodeCommon';

/**
 * The text of the item. `chosen` is how the chosen controller is named (its
 * name, else its short device id), or undefined when none is chosen.
 */
export function statusText(model: DevicePanelModel, chosen: string | undefined): string {
  switch (model.state) {
    case 'disconnected':
      return chosen === undefined
        ? '$(debug-disconnect) IronPLC: Not connected'
        : `$(debug-disconnect) IronPLC: ${chosen} (not connected)`;
    case 'connecting':
      return '$(sync~spin) IronPLC: Connecting…';
    case 'reconnecting':
      return '$(sync~spin) IronPLC: Reconnecting…';
    case 'connected':
      return `$(plug) IronPLC: ${model.identity?.device.name ?? model.profileName ?? 'Connected'}`;
  }
}

export function statusTooltip(model: DevicePanelModel, chosen: string | undefined): string {
  switch (model.state) {
    case 'disconnected':
      return chosen === undefined
        ? 'Connect to a device (IronPLC: Connect to Device)'
        : `Chosen controller: ${chosen}. Click to connect to it (IronPLC: Network: Connect to Chosen Controller).`;
    case 'connecting':
      return 'Opening the transport and running the identity handshake…';
    case 'reconnecting':
      return 'The connection was lost; retrying with backoff. The device panel keeps the last data, marked stale.';
    case 'connected': {
      const lines = [`Profile: ${model.profileName ?? 'unknown'}`];
      if (model.lastStatus) {
        lines.push(formatStatusDetail(model.lastStatus));
      }
      lines.push('Click for the IronPLC Device panel.');
      return lines.join('\n');
    }
  }
}

/** The command a click on the item runs. */
export function statusCommand(state: ConnectionState, chosen: string | undefined): string {
  if (state !== 'disconnected') {
    return 'ironplc.devicePanel.focus';
  }
  return chosen === undefined ? 'ironplc.connect' : HANDOFF_ACTIONS.connect.command;
}
