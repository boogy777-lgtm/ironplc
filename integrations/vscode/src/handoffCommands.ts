/**
 * Registers the commands that the IronLink extension (or anyone else) hands a
 * controller to. The table of handoff actions in the shared package declares the
 * command id of each action; `HandoffHandlers` has one entry for each member of
 * it, so an action added to the table without a handler here does not compile.
 * The handlers receive the raw argument, which the command can be run with from
 * any extension, and check it themselves.
 */

import * as vscode from 'vscode';

import { HANDOFF_ACTIONS, HandoffName, handoffNames } from './vscodeCommon';

/** One handler for each handoff action. */
export type HandoffHandlers = { readonly [K in HandoffName]: (argument: unknown) => Promise<void> };

export function registerHandoffCommands(context: vscode.ExtensionContext, handlers: HandoffHandlers): void {
  for (const name of handoffNames()) {
    context.subscriptions.push(
      vscode.commands.registerCommand(HANDOFF_ACTIONS[name].command, (argument?: unknown) => handlers[name](argument)),
    );
  }
}
