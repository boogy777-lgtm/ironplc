/**
 * The commands that change whether the connected controller scans, one for each
 * member of `SCAN_ACTIONS` (`ironplc.runController`, `ironplc.stopController`).
 * They send `startScan` or `stopScan` over the session of the engineering
 * connection and compile nothing; they are not the local simulation
 * (`ironplc.runProgram`). This module is glue: what a command comes to is
 * decided in `controllerRunLogic`, and the run state it leaves is shown by the
 * connection's status item.
 */

import * as vscode from 'vscode';

import { ConnectionFeature } from './connection';
import { runScanAction, SCAN_ACTIONS, scanActionNames } from './controllerRunLogic';

export function registerControllerRunCommands(
  context: vscode.ExtensionContext,
  connection: ConnectionFeature,
  /** Shows an error whose text starts with its code, with the link to the page of the code. */
  showCoded: (code: string, text: string) => void,
): void {
  context.subscriptions.push(
    ...scanActionNames().map(action =>
      vscode.commands.registerCommand(SCAN_ACTIONS[action].commandId, async () => {
        const outcome = await runScanAction(connection.manager.activeSession, action);
        switch (outcome.kind) {
          case 'noSession':
            void vscode.window.showWarningMessage(outcome.message);
            return;
          case 'done':
            connection.updateRun(outcome.run);
            void vscode.window.showInformationMessage(outcome.message);
            return;
          case 'refused':
            if (outcome.run !== undefined) {
              connection.updateRun(outcome.run);
            }
            if (outcome.vCode === null) {
              void vscode.window.showErrorMessage(`IronPLC: ${SCAN_ACTIONS[action].title}: ${outcome.text}`);
            }
            else {
              showCoded(outcome.vCode, outcome.text);
            }
            return;
        }
      }),
    ),
  );
}
