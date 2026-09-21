/**
 * The "IronPLC HA" panel: the five Studio tabs of the HA engineering UI
 * contract (`specs/design/ha-engineering-ui.md`) as an Explorer tree over
 * the engineering connection's single session (ADR-0065). The provider is
 * a thin renderer over the vscode-free `haPanelLogic` sections; this
 * module owns the vscode objects (tree, commands, input boxes) and the
 * refresh cadence — the connection feature's model event, which fires on
 * the heartbeat poll, is the contract's slow poll; the panel's own
 * actions refresh immediately after they acknowledge.
 *
 * Engineer actions are offered only where the ADR-0064/0065 rules allow
 * them (`canCommandSwap` and friends): the swap exists only at SYNC_READY
 * on a live pair, and every action rides the one active session — nothing
 * here opens a second connection.
 */

import * as vscode from 'vscode';

import { ConnectionFeature } from './connection';
import { HotEditProtocolError, HotEditSession } from './hotEditSession';
import {
  canCommandSwap,
  canRunCalibration,
  canSetTimingBudget,
  disconnectedHaModel,
  HA_COMMANDED_SWAP_COMMAND,
  HA_REFRESH_COMMAND,
  HA_RUN_CALIBRATION_COMMAND,
  HA_SET_TIMING_BUDGET_COMMAND,
  HaPanelModel,
  HaSection,
  haSections,
} from './haPanelLogic';
import { PanelRow } from './devicePanelLogic';

type HaNode = { kind: 'section'; section: HaSection } | { kind: 'row'; row: PanelRow };

export function registerHaPanel(context: vscode.ExtensionContext, feature: ConnectionFeature): void {
  let model: HaPanelModel = disconnectedHaModel();
  let refreshing: Promise<void> = Promise.resolve();

  const emitter = new vscode.EventEmitter<HaNode | undefined>();
  const provider: vscode.TreeDataProvider<HaNode> = {
    onDidChangeTreeData: emitter.event,
    getChildren(node?: HaNode): Thenable<HaNode[]> {
      if (node === undefined) {
        return Promise.resolve(haSections(model).map(section => ({ kind: 'section' as const, section })));
      }
      if (node.kind === 'section') {
        return Promise.resolve(node.section.rows.map(row => ({ kind: 'row' as const, row })));
      }
      return Promise.resolve([]);
    },
    getTreeItem(node: HaNode): vscode.TreeItem {
      if (node.kind === 'section') {
        const item = new vscode.TreeItem(node.section.title, vscode.TreeItemCollapsibleState.Expanded);
        item.id = node.section.id;
        item.description = node.section.badge;
        item.contextValue = 'ironplc.haPanel.section';
        return item;
      }
      const item = new vscode.TreeItem(node.row.label);
      item.id = node.row.id;
      item.description = node.row.description;
      item.tooltip = node.row.tooltip;
      item.contextValue = 'ironplc.haPanel.row';
      return item;
    },
  };
  context.subscriptions.push(vscode.window.registerTreeDataProvider('ironplc.haPanel', provider));

  /** The live session of the one connection, when connected. */
  function session(): HotEditSession | undefined {
    return feature.manager.activeSession;
  }

  /** Refreshes the model over the live session; failures keep the last good data. */
  async function refreshHa(): Promise<void> {
    const current = session();
    if (current === undefined) {
      model = disconnectedHaModel();
      emitter.fire(undefined);
      return;
    }
    try {
      // Sequential over the one session: the serve loop answers in order.
      const [status, calibration, barrier, ioReady, budget, events] = await Promise.all([
        current.haStatus(),
        current.haCalibration(),
        current.haBarrier(),
        current.haIoReady(),
        current.haTimingBudget(),
        current.haEvents(),
      ]);
      model = {
        connected: true,
        supported: true,
        standalone: status.standalone,
        stale: false,
        status,
        calibration,
        barrier,
        ioReady,
        budget,
        events,
      };
    }
    catch (err) {
      if (err instanceof HotEditProtocolError && err.vCode === null) {
        // A codeless refusal means the server predates the HA surface.
        model = { ...disconnectedHaModel(), connected: true, supported: false };
      }
      else {
        // A coded refusal or a transport fault: keep the last good data.
        model = { ...model, connected: true, stale: true };
      }
    }
    emitter.fire(undefined);
  }

  /** Serializes refreshes so the heartbeat and an action never interleave. */
  function scheduleRefresh(): void {
    refreshing = refreshing.then(refreshHa).catch(() => {
      // refreshHa renders failures into the model; nothing to surface.
    });
    return void refreshing;
  }

  // The contract's slow poll: the connection's heartbeat (getStatus) fires
  // the model event on its cadence, and the HA surface piggybacks it — no
  // new timer machinery.
  context.subscriptions.push(feature.onDidChangeModel(() => scheduleRefresh()));

  async function runAction(action: (current: HotEditSession) => Promise<void>): Promise<void> {
    const current = session();
    if (current === undefined) {
      void vscode.window.showWarningMessage('IronPLC HA: connect to a device first.');
      return;
    }
    try {
      await action(current);
    }
    catch (err) {
      const text = err instanceof HotEditProtocolError ? err.toString() : err instanceof Error ? err.message : String(err);
      void vscode.window.showErrorMessage(`IronPLC HA: ${text}`);
    }
    scheduleRefresh();
  }

  context.subscriptions.push(
    vscode.commands.registerCommand(HA_REFRESH_COMMAND, () => scheduleRefresh()),
    vscode.commands.registerCommand(HA_COMMANDED_SWAP_COMMAND, () => {
      if (!canCommandSwap(model)) {
        void vscode.window.showWarningMessage(
          'IronPLC HA: a commanded swap is available only when the pair is at SYNC_READY.',
        );
        return;
      }
      void vscode.window
        .showWarningMessage(
          'Commanded swap: output control moves to the peer unit. Continue?',
          { modal: true },
          'Swap',
        )
        .then((choice) => {
          if (choice === 'Swap') {
            return runAction(current => current.haCommandedSwap());
          }
          return undefined;
        });
    }),
    vscode.commands.registerCommand(HA_RUN_CALIBRATION_COMMAND, () => {
      if (!canRunCalibration(model)) {
        void vscode.window.showWarningMessage(
          'IronPLC HA: a calibration run requires a connected redundant pair.',
        );
        return;
      }
      void vscode.window
        .showInformationMessage(
          'Run a calibration? The previous timing guarantee is invalid until the run completes.',
          { modal: true },
          'Run Calibration',
        )
        .then((choice) => {
          if (choice === 'Run Calibration') {
            return runAction(current => current.haRunCalibration());
          }
          return undefined;
        });
    }),
    vscode.commands.registerCommand(HA_SET_TIMING_BUDGET_COMMAND, async () => {
      if (!canSetTimingBudget(model)) {
        void vscode.window.showWarningMessage(
          'IronPLC HA: the timing budget requires a connected redundant pair.',
        );
        return;
      }
      const confirmation = await vscode.window.showInputBox({
        title: 'IronPLC HA: peer-failure confirmation time',
        prompt: 'Ticks without the peer before its death is confirmed (a supervision parameter, ADR-0062).',
        validateInput: value => (isPositiveInteger(value) ? undefined : 'Enter a positive whole number of ticks.'),
      });
      if (confirmation === undefined) {
        return;
      }
      const budget = await vscode.window.showInputBox({
        title: 'IronPLC HA: maximum process-recovery budget',
        prompt: 'The recovery time the installation must demonstrate; a budget it cannot meet is refused (V4111).',
        validateInput: value => (isPositiveInteger(value) ? undefined : 'Enter a positive whole number of ticks.'),
      });
      if (budget === undefined) {
        return;
      }
      await runAction(current =>
        current.haSetTimingBudget(Number.parseInt(confirmation, 10), Number.parseInt(budget, 10)),
      );
    }),
  );
}

function isPositiveInteger(value: string): boolean {
  const parsed = Number.parseInt(value, 10);
  return Number.isInteger(parsed) && parsed > 0 && String(parsed) === value.trim();
}
