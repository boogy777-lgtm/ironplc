/**
 * The VS Code side of the activity-bar "Commands" view: maps the static
 * model from `iplcViewLogic` onto tree items. The provider owns no session
 * state and adds no guards — every item executes an already-registered
 * command whose handler validates its own preconditions.
 */

import * as vscode from 'vscode';
import {
  ACTIONS_VIEW_ID,
  ActionGroup,
  ActionItem,
  buildActionGroups,
} from './iplcViewLogic';

/** A tree node: either a group header or one command shortcut. */
type ActionsNode
  = | { kind: 'group'; group: ActionGroup }
    | { kind: 'action'; item: ActionItem };

/** Renders the static action model as a two-level tree. */
export class IplcActionsProvider implements vscode.TreeDataProvider<ActionsNode> {
  private readonly groups = buildActionGroups();

  getTreeItem(node: ActionsNode): vscode.TreeItem {
    if (node.kind === 'group') {
      const item = new vscode.TreeItem(node.group.label, vscode.TreeItemCollapsibleState.Expanded);
      item.id = node.group.id;
      item.contextValue = 'ironplc.actions.group';
      return item;
    }
    const item = new vscode.TreeItem(node.item.label, vscode.TreeItemCollapsibleState.None);
    item.id = node.item.id;
    item.iconPath = new vscode.ThemeIcon(node.item.icon);
    item.tooltip = node.item.command;
    item.command = { command: node.item.command, title: node.item.label };
    return item;
  }

  getChildren(node?: ActionsNode): ActionsNode[] {
    if (!node) {
      return this.groups.map(group => ({ kind: 'group', group }));
    }
    if (node.kind === 'group') {
      return node.group.items.map(item => ({ kind: 'action', item }));
    }
    return [];
  }
}

/** Registers the commands view. Called unconditionally, like the other command surfaces. */
export function registerActionsView(context: vscode.ExtensionContext): void {
  context.subscriptions.push(
    vscode.window.registerTreeDataProvider(
      ACTIONS_VIEW_ID,
      new IplcActionsProvider(),
    ),
  );
}
