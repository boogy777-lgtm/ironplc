/**
 * The "IronPLC Device" panel: a tree view in the Explorer container that
 * renders the engineering-connection model the `connection` feature
 * publishes — connection state, device identity, application state,
 * pending edit, baseline, and build phase. The provider is a thin renderer
 * over the vscode-free `devicePanelLogic` rows: it subscribes to the
 * feature's model event and maps rows to tree items, owning no session or
 * protocol decision.
 */

import * as vscode from 'vscode';

import { ConnectionFeature } from './connection';
import { devicePanelRows, PanelRow } from './devicePanelLogic';

export function registerDevicePanel(context: vscode.ExtensionContext, feature: ConnectionFeature): void {
  const provider = new DevicePanelProvider(feature);
  context.subscriptions.push(vscode.window.registerTreeDataProvider('ironplc.devicePanel', provider));
}

class DevicePanelProvider implements vscode.TreeDataProvider<PanelRow> {
  private readonly emitter = new vscode.EventEmitter<PanelRow | undefined>();
  readonly onDidChangeTreeData = this.emitter.event;

  constructor(private readonly feature: ConnectionFeature) {
    feature.onDidChangeModel(() => this.emitter.fire(undefined));
  }

  getChildren(): Thenable<PanelRow[]> {
    return Promise.resolve(devicePanelRows(this.feature.model()));
  }

  getTreeItem(row: PanelRow): vscode.TreeItem {
    const item = new vscode.TreeItem(row.label);
    item.id = row.id;
    item.description = row.description;
    item.tooltip = row.tooltip;
    item.contextValue = 'ironplc.devicePanel.row';
    return item;
  }
}
