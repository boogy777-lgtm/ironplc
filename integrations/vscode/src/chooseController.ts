/**
 * The command `ironplc.chooseController` ("IronPLC: Choose Controller…"): the
 * engineer chooses the controller of the project (ADR-0084). Thin glue: what the
 * picker shows, what picking does and the arguments of the compiler are in
 * `chooseControllerLogic`; how IronLink is reached is in `ironlinkAccess`.
 *
 * Invoked with a controller (the Choose action of the IronLink Network view), the
 * workstation route is already stored, so only the project's path record is
 * written. Invoked without one (the command palette), a picker lists the
 * controller last chosen, the controllers with a route on this PC and the
 * controllers recently observed; picking one stores the route through IronLink
 * and then writes the path record. IronLink is optional: without it the picker
 * lists the controller last chosen and says what searching needs.
 */

import * as path from 'path';
import * as vscode from 'vscode';

import { ReportProblem } from './debugAdapter';
import {
  applicationAnchor,
  buildPickEntries,
  ChosenController,
  decidePick,
  describeIronLinkFailure,
  IronLinkState,
  PickEntry,
  readChosen,
  writeChoice,
} from './chooseControllerLogic';
import { accessIronLink } from './ironlinkAccess';
import { readTextIfPresent } from './readText';
import { runCompiler } from './taskProviderLogic';
import {
  ControllerArgument,
  isControllerArgument,
  IronLinkApi,
  ProblemCode,
  shortDeviceId,
  socketAddress,
} from './vscodeCommon';

/** What choosing needs from the extension. */
export interface ChooseControllerContext {
  compilerPath: string | undefined;
  sourceExtensions: readonly string[];
  output: vscode.OutputChannel;
  reportProblem: ReportProblem;
  /** Shows a failure that carries a code of another family (an `L####` of IronLink) with its help link. */
  showCoded: (code: string, text: string) => void;
}

/** A picker row with the entry it was made from. */
interface EntryItem extends vscode.QuickPickItem {
  entry?: PickEntry;
}

function folders(): Array<{ name: string; path: string }> {
  return (vscode.workspace.workspaceFolders ?? []).map(folder => ({ name: folder.name, path: folder.uri.fsPath }));
}

function toItem(entry: PickEntry): EntryItem {
  switch (entry.kind) {
    case 'separator':
      return { label: entry.label, kind: vscode.QuickPickItemKind.Separator };
    case 'device':
      return { label: entry.label, description: entry.description, detail: entry.detail, entry };
    case 'search':
      return { label: `$(search) ${entry.label}`, detail: entry.detail, entry };
    case 'ironlinkNeeded':
    case 'ironlinkFailed':
      return { label: `$(info) ${entry.label}`, detail: entry.detail, entry };
  }
}

/** The handler of the command: the raw argument it was run with, if any. */
export function chooseController(context: ChooseControllerContext): (argument: unknown) => Promise<void> {
  const { compilerPath, sourceExtensions, output, reportProblem, showCoded } = context;

  const run = (compiler: string) => (args: string[]) => runCompiler(compiler, args, text => output.append(text));

  /** The anchor and the compiler a choice needs, or a reported reason why there are none. */
  function target(): { anchor: string; compiler: string } | undefined {
    const anchor = applicationAnchor(folders(), sourceExtensions);
    if ('reason' in anchor) {
      reportProblem(ProblemCode.ChooseControllerFailed, anchor.reason);
      return undefined;
    }
    if (!compilerPath) {
      reportProblem(ProblemCode.NoCompiler, 'Install the compiler to record the chosen controller.');
      return undefined;
    }
    return { anchor: anchor.anchor, compiler: compilerPath };
  }

  /** Writes the project's path record. */
  async function record(controller: ControllerArgument): Promise<void> {
    const where = target();
    if (where === undefined) {
      return;
    }
    try {
      await writeChoice(run(where.compiler), where.anchor, controller);
    }
    catch (err) {
      reportProblem(ProblemCode.ChooseControllerFailed, err instanceof Error ? err.message : String(err));
      return;
    }
    const text = `Chose controller ${shortDeviceId(controller.deviceId)} at ${socketAddress(controller.address, controller.sessionPort)} for ${path.basename(where.anchor)}.`;
    output.appendLine(text);
    void vscode.window.showInformationMessage(`IronPLC: ${text}`);
  }

  /** The state of IronLink for the picker: its routes, or why there are none. */
  async function ironlinkState(api: IronLinkApi | undefined, reason: string): Promise<IronLinkState> {
    if (api === undefined) {
      return { state: 'unavailable', reason };
    }
    try {
      return { state: 'ready', routes: await api.routeList(), lastBrowse: api.lastBrowse() };
    }
    catch (err) {
      const shown = describeIronLinkFailure(err);
      if (shown === undefined) {
        throw err;
      }
      output.appendLine(shown.text);
      return { state: 'failed', message: shown.text };
    }
  }

  /** Searches the network in IronLink's Network view. */
  async function search(api: IronLinkApi): Promise<void> {
    await api.revealNetwork();
    try {
      await api.browse();
    }
    catch (err) {
      const shown = describeIronLinkFailure(err);
      if (shown === undefined) {
        throw err;
      }
      output.appendLine(shown.text);
      showCoded(shown.code, shown.text);
    }
  }

  async function pick(): Promise<void> {
    const where = target();
    if (where === undefined) {
      return;
    }
    let chosen: ChosenController | undefined;
    try {
      chosen = await readChosen(run(where.compiler), readTextIfPresent, where.anchor);
    }
    catch (err) {
      output.appendLine(`The path record could not be read, so no controller is shown as chosen: ${err instanceof Error ? err.message : String(err)}`);
    }
    const access = await accessIronLink(id => vscode.extensions.getExtension(id));
    const api = access.available ? access.api : undefined;
    const state = await ironlinkState(api, access.available ? '' : access.reason);

    const items = buildPickEntries({ chosen, ironlink: state, nowMs: Date.now() }).map(toItem);
    const picked = await vscode.window.showQuickPick<EntryItem>(items, {
      title: 'IronPLC: Choose Controller',
      placeHolder: 'Select the controller of this project',
      matchOnDescription: true,
      matchOnDetail: true,
    });
    if (picked?.entry === undefined) {
      return;
    }
    const decision = decidePick(picked.entry);
    switch (decision.action) {
      case 'nothing':
        return;
      case 'explain':
        void vscode.window.showInformationMessage(decision.message);
        return;
      case 'search':
        await search(api!);
        return;
      case 'choose': {
        const outcome = await api!.setRoute(
          decision.controller.deviceId,
          socketAddress(decision.controller.address, decision.controller.sessionPort),
        );
        if (!outcome.ok) {
          output.appendLine(outcome.message);
          showCoded(outcome.code, outcome.message);
          return;
        }
        await record({
          deviceId: outcome.route.deviceId,
          address: outcome.route.address,
          sessionPort: outcome.route.sessionPort,
        });
        return;
      }
    }
  }

  return async (argument: unknown) => {
    if (argument === undefined) {
      await pick();
      return;
    }
    if (!isControllerArgument(argument)) {
      reportProblem(
        ProblemCode.ChooseControllerFailed,
        'The command was run with an argument that is not a controller: expected {deviceId, address, sessionPort}',
      );
      return;
    }
    await record(argument);
  };
}
