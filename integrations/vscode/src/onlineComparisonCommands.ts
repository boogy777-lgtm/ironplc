/**
 * The go-online dialog (ADR-0089): what Connect shows when the controller does
 * not hold the last build, and what `ironplc.showOnlineComparison` shows again.
 * It is a QuickPick in the style of the online-edit dialog: the lines that say
 * what the controller is and holds, what the project has and what else is true
 * come first (as headers that cannot be chosen), then one item for each action
 * of the comparison. A disabled item shows its reason and does nothing.
 *
 * This module is glue. The model comes from `comparisonModel`, the enablement
 * and the reasons are the compiler's, and what an item does is the table
 * `ACTION_HANDLERS`; here each kind of handler is run once.
 */

import * as vscode from 'vscode';

import { ConnectionFeature } from './connection';
import { accessIronLink } from './ironlinkAccess';
import {
  ActionRun,
  Comparison,
  ComparisonModel,
  comparisonModel,
  ModelItem,
  SHOW_ONLINE_COMPARISON,
} from './onlineComparisonLogic';

/** A row of the QuickPick: the item it was made from, absent for a header. */
interface ComparisonRow extends vscode.QuickPickItem {
  item?: ModelItem;
}

export function registerOnlineComparisonCommands(
  context: vscode.ExtensionContext,
  connection: ConnectionFeature,
): void {
  let picker: vscode.QuickPick<ComparisonRow> | undefined;

  context.subscriptions.push(
    vscode.commands.registerCommand(SHOW_ONLINE_COMPARISON.commandId, show),
    connection.onDidAnnounceComparison(({ model }) => present(model)),
    { dispose: () => picker?.dispose() },
  );

  function show(): void {
    const comparison: Comparison | undefined = connection.comparison();
    if (comparison === undefined) {
      void vscode.window.showInformationMessage('IronPLC: there is no go-online comparison. Connect to a controller first.');
      return;
    }
    present(comparisonModel(comparison));
  }

  function present(model: ComparisonModel): void {
    picker?.dispose();
    const created = vscode.window.createQuickPick<ComparisonRow>();
    created.ignoreFocusOut = true;
    created.title = model.title;
    created.placeholder = 'Choose what to do with this controller';
    created.items = [
      ...model.rows.map((row): ComparisonRow => ({
        label: `${row.label}: ${row.text}`,
        kind: vscode.QuickPickItemKind.Separator,
      })),
      ...model.items.map((item): ComparisonRow => ({
        label: item.label,
        description: item.description,
        detail: item.detail,
        alwaysShow: true,
        item,
      })),
    ];
    created.onDidAccept(() => {
      const item = created.selectedItems[0]?.item;
      // A disabled item does nothing; its reason is shown with it.
      if (item?.enabled === true && item.run !== undefined) {
        created.hide();
        void run(item.run);
      }
    });
    created.onDidHide(() => {
      created.dispose();
      if (picker === created) {
        picker = undefined;
      }
    });
    picker = created;
    created.show();
  }

  /** Runs what an action means. One case for each kind of handler. */
  async function run(handler: ActionRun): Promise<void> {
    switch (handler.kind) {
      case 'close':
      case 'none':
        return;
      case 'command':
        await vscode.commands.executeCommand(handler.command);
        return;
      case 'retry':
        await connection.retry();
        return;
      case 'search':
        await searchNetwork();
        return;
    }
  }

  /** Searches the network for controllers and shows the Network view (IronLink). */
  async function searchNetwork(): Promise<void> {
    const access = await accessIronLink(id => vscode.extensions.getExtension(id));
    if (!access.available) {
      void vscode.window.showInformationMessage(access.reason);
      return;
    }
    try {
      await access.api.revealNetwork();
      await access.api.browse();
    }
    catch (err) {
      void vscode.window.showErrorMessage(`IronPLC: the search of the network failed: ${err instanceof Error ? err.message : String(err)}`);
    }
  }
}
