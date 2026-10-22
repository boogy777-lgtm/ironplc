import * as vscode from 'vscode';
import { existsSync } from 'fs';

import { BrowseMemory, createIronLinkApi } from './api';
import { chooseDevice } from './chooseSequence';
import { ActionContext, DEVICE_ACTION_COMMANDS, DeviceAction, decideDeviceAction } from './deviceActions';
import { describeFailure } from './failures';
import { IronLinkClient } from './ironlinkClient';
import { BrowseOptions } from './ironlinkProtocol';
import { ironLinkLauncher } from './ironlinkProcess';
import { describeAddressAdd, describeAddressRemove, describeBrowse, describeIdentify, describeRouteSet } from './messages';
import { NetworkNode, buildNetworkTree, socketAddress } from './networkTree';
import { browseOptionsFromSettings } from './settings';
import { IronLinkApi, ToolchainEnvironment, contributedCommandIds, problemHelpUrl, runHandoff } from './vscodeCommon';

/** The Network view: the last search, rendered with the current age of each observation. */
class NetworkProvider implements vscode.TreeDataProvider<NetworkNode> {
  private readonly changed = new vscode.EventEmitter<void>();
  readonly onDidChangeTreeData = this.changed.event;

  constructor(private readonly memory: BrowseMemory) {}

  refresh(): void {
    this.changed.fire();
  }

  getTreeItem(node: NetworkNode): vscode.TreeItem {
    const item = new vscode.TreeItem(
      node.label,
      node.children.length > 0 ? vscode.TreeItemCollapsibleState.Expanded : vscode.TreeItemCollapsibleState.None,
    );
    item.id = node.id;
    item.description = node.description;
    item.tooltip = node.tooltip;
    item.contextValue = node.contextValue;
    return item;
  }

  getChildren(node?: NetworkNode): NetworkNode[] {
    if (node) {
      return node.children;
    }
    const last = this.memory.last();
    return last ? buildNetworkTree(last.response, Date.now()) : [];
  }
}

function toolchainEnvironment(): ToolchainEnvironment {
  return {
    platform: process.platform,
    existsSync,
    getEnv: (name: string) => process.env[name],
    getConfig: (key: string) => vscode.workspace.getConfiguration('ironlink').get<string>(key),
  };
}

/** The browse defaults the user set; an unset one is left to the program. */
function browseOptions(): BrowseOptions {
  const config = vscode.workspace.getConfiguration('ironlink.browse');
  return browseOptionsFromSettings(config.get('timeoutMs'), config.get('attempts'));
}

let client: IronLinkClient | undefined;

export function activate(context: vscode.ExtensionContext): IronLinkApi {
  // The version comes from the extension context, not an extension id: the
  // documentation links carry it.
  const version: string = context.extension.packageJSON?.version ?? '';
  const output = vscode.window.createOutputChannel('IronLink');
  const memory = new BrowseMemory();
  const provider = new NetworkProvider(memory);
  const ironlink = new IronLinkClient(
    ironLinkLauncher(toolchainEnvironment, text => output.append(text), line => output.appendLine(line)),
    () => {
      void ironlink.version().then(
        info => output.appendLine(`ironlink ${info.ironlink}, discovery wire version ${info.discoveryWireVersion}`),
        () => {},
      );
    },
  );
  client = ironlink;

  const view = vscode.window.createTreeView('ironlink.network', { treeDataProvider: provider });
  context.subscriptions.push(output, view, { dispose: () => ironlink.dispose() });
  // The ages are relative to now, so a view that becomes visible shows them anew.
  context.subscriptions.push(view.onDidChangeVisibility(event => {
    if (event.visible) {
      provider.refresh();
    }
  }));

  const showFailure = (text: string, helpUrl: string) => {
    output.appendLine(text);
    void vscode.window.showErrorMessage(text, 'Open Online Help').then(selection => {
      if (selection === 'Open Online Help') {
        void vscode.env.openExternal(vscode.Uri.parse(helpUrl));
      }
    });
  };
  const reportFailure = (error: unknown) => {
    const shown = describeFailure(error, version);
    if (shown === undefined) {
      // Not an IronLink failure: a defect of the extension, which has no code.
      // VS Code reports it with its stack.
      throw error;
    }
    showFailure(shown.text, shown.helpUrl);
  };

  // What other extensions see: the same browse and the same program as the view.
  const api = createIronLinkApi({
    client: ironlink,
    memory,
    now: Date.now,
    browseOptions,
    show: result => {
      output.appendLine(describeBrowse(result));
      provider.refresh();
    },
    revealNetwork: async () => {
      await vscode.commands.executeCommand('ironlink.network.focus');
    },
  });
  // Every command reports its failure the same way.
  const register = (command: string, handler: (...args: any[]) => Promise<void>) => {
    context.subscriptions.push(vscode.commands.registerCommand(command, async (...args: unknown[]) => {
      try {
        await handler(...args);
      }
      catch (error) {
        reportFailure(error);
      }
    }));
  };

  register('ironlink.browse', async () => {
    await vscode.window.withProgress({ location: { viewId: 'ironlink.network' } }, async () => {
      await api.browse();
    });
  });

  register('ironlink.addAddress', async () => {
    const address = await vscode.window.showInputBox({
      prompt: 'Controller address, IP or IP:PORT',
      placeHolder: '192.168.1.50:49152',
    });
    if (address === undefined || address.trim().length === 0) {
      return;
    }
    void vscode.window.showInformationMessage(describeAddressAdd(await ironlink.addressAdd(address)));
  });

  register('ironlink.removeAddress', async () => {
    const { addresses } = await ironlink.addressList();
    if (addresses.length === 0) {
      void vscode.window.showInformationMessage('The address list is empty.');
      return;
    }
    const chosen = await vscode.window.showQuickPick(addresses, { placeHolder: 'Address to remove' });
    if (chosen === undefined) {
      return;
    }
    void vscode.window.showInformationMessage(describeAddressRemove(await ironlink.addressRemove(chosen)));
  });

  // The device actions ask one function whether they are available, and show
  // its reason when they are not.
  const deviceAction = (action: DeviceAction, run: (node: NetworkNode) => Promise<void>) => {
    register(DEVICE_ACTION_COMMANDS[action], async (node?: NetworkNode) => {
      const decision = decideDeviceAction(action, node?.device, await actionContext());
      if (!decision.enabled) {
        void vscode.window.showInformationMessage(decision.reason);
        return;
      }
      await run(node!);
    });
  };

  deviceAction('identify', async node => {
    const row = node.device!;
    const reply = await ironlink.identify(socketAddress(row.address, row.sessionPort), browseOptions());
    const notice = describeIdentify(reply, ms => new Date(ms).toLocaleTimeString());
    output.appendLine(notice.text);
    void (notice.severity === 'warning'
      ? vscode.window.showWarningMessage(notice.text)
      : vscode.window.showInformationMessage(notice.text));
  });

  deviceAction('copyDeviceId', async node => {
    await vscode.env.clipboard.writeText(node.device!.deviceId);
  });

  // The route of this workstation first (IronLink's own data, verified by the
  // program); the project path file is IronPLC's and is written only after it.
  deviceAction('choose', async node => {
    const outcome = await chooseDevice(
      node.device!,
      (deviceId, address) => api.setRoute(deviceId, address),
      argument => runHandoff('choose', argument, (command, ...args) =>
        vscode.commands.executeCommand(command, ...args)),
    );
    if (outcome.handedOff) {
      output.appendLine(describeRouteSet(outcome.route));
    }
    else {
      showFailure(outcome.failure.message, problemHelpUrl(outcome.failure.code, version));
    }
  });

  return api;
}

async function actionContext(): Promise<ActionContext> {
  return {
    registeredCommands: new Set(await vscode.commands.getCommands(true)),
    contributedCommands: contributedCommandIds(vscode.extensions.all.map(extension => extension.packageJSON)),
  };
}

export function deactivate() {
  client?.dispose();
  client = undefined;
}
