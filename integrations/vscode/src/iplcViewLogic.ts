/**
 * The model behind the IronPLC activity-bar "Commands" view: a static,
 * grouped list of shortcuts to commands the extension already registers.
 *
 * The module is vscode-free (unit-testable, like `syncUidsLogic`): the
 * provider in `iplcViewProvider.ts` maps this model onto `TreeItem`s and
 * nothing here touches the VS Code API. Items carry only data; every item's
 * `command` must be an id declared in `package.json` `contributes.commands`,
 * which the unit tests assert against the actual manifest. The tree adds no
 * session guards: the existing handlers already validate their state.
 */

/** One clickable shortcut in the view. */
export interface ActionItem {
  /** Stable tree-item id, unique within the view. */
  id: string;
  /** Human-readable label shown in the tree. */
  label: string;
  /** Codicon id (without the `$(...)` wrapper), rendered as a `ThemeIcon`. */
  icon: string;
  /** The registered command id the item executes. */
  command: string;
}

/** A collapsible group of shortcuts. */
export interface ActionGroup {
  /** Stable group id, unique within the view. */
  id: string;
  /** Group label shown in the tree. */
  label: string;
  items: ActionItem[];
}

/** The contributed view id (`contributes.views.ironplc`). */
export const ACTIONS_VIEW_ID = 'ironplc.actions';

/** The activity-bar container id (`contributes.viewsContainers.activitybar`). */
export const ACTIONS_CONTAINER_ID = 'ironplc';

/**
 * Builds the view model: two groups over commands the extension already
 * registers. Icons reuse the command's declared codicon where package.json
 * defines one and a fitting codicon otherwise.
 */
export function buildActionGroups(): ActionGroup[] {
  return [
    {
      id: 'program',
      label: 'Program',
      items: [
        { id: 'program.run', label: 'Run Program', icon: 'play', command: 'ironplc.runProgram' },
        { id: 'program.stop', label: 'Stop Program', icon: 'debug-stop', command: 'ironplc.stopProgram' },
        { id: 'program.pause', label: 'Pause/Resume Program', icon: 'debug-pause', command: 'ironplc.pauseProgram' },
        { id: 'program.step', label: 'Step Scan Cycle', icon: 'debug-step-over', command: 'ironplc.stepScan' },
      ],
    },
    {
      id: 'hot-edit',
      label: 'Hot Edit',
      items: [
        { id: 'hot-edit.start', label: 'Start Session', icon: 'debug-start', command: 'ironplc.startHotEditSession' },
        { id: 'hot-edit.accept', label: 'Accept Edits', icon: 'check', command: 'ironplc.acceptEdits' },
        { id: 'hot-edit.test', label: 'Test Edits', icon: 'beaker', command: 'ironplc.testEdits' },
        { id: 'hot-edit.untest', label: 'Untest Edits', icon: 'discard', command: 'ironplc.untestEdits' },
        { id: 'hot-edit.assemble', label: 'Assemble Edits', icon: 'save', command: 'ironplc.assembleEdits' },
        { id: 'hot-edit.cancel', label: 'Cancel Edits', icon: 'close', command: 'ironplc.cancelEdits' },
        { id: 'hot-edit.status', label: 'Show Status', icon: 'info', command: 'ironplc.showHotEditStatus' },
        { id: 'hot-edit.sync-uids', label: 'Sync Variable IDs', icon: 'sync', command: 'ironplc.syncVariableIds' },
      ],
    },
  ];
}

/** All items of a group list, flattened in view order. */
export function actionItems(groups: ActionGroup[]): ActionItem[] {
  return groups.reduce<ActionItem[]>((all, group) => all.concat(group.items), []);
}
