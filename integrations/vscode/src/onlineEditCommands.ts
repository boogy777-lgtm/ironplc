/**
 * The online-edit dialog: `ironplc.onlineEdit` opens a QuickPick that stays
 * open through all stages of an edit. This module is glue: it sets the widget properties from `dialogModel`, runs the item the
 * person selects with `runItem`, and renders what the device reports. The
 * model and the runner decide everything else and are unit-tested without
 * `vscode`.
 *
 * Esc hides the dialog and leaves the candidate where it is; a status bar item
 * reopens it while a candidate exists. The dialog holds one fact of its own: a
 * build of the current sources succeeded here (and the container it made).
 * Every other label, enablement and reason is the edit view of the device.
 */

import * as vscode from 'vscode';
import * as path from 'path';

import { isDebuggableProgram, programKind } from './debugAdapterLogic';
import { BuildSupport, BUILD_ORIGIN, SentContainer } from './buildCommands';
import { ConnectionFeature } from './connection';
import { DOWNLOAD_ACTIONS } from './downloadLogic';
import { AcceptGate, acceptGate } from './onlineComparisonLogic';
import { formatStatusText, HotEditStatus } from './hotEditSession';
import { acceptEditsWithDecisions } from './hotEditMigrationLogic';
import { createMigrationDecisionUi, resolvePathInteractive } from './hotEdit';
import {
  BuildFact,
  DialogItem,
  dialogModel,
  ONLINE_EDIT,
  reopenItem,
} from './onlineEditLogic';
import { OnlineEditIo, runItem, runMessage } from './onlineEditRunner';

/** A build made in this dialog: the program and the container that was sent. */
interface Built {
  program: string;
  sent: SentContainer;
}

/** A row of the QuickPick: the item of the model it was made from, absent for a notice. */
interface DialogRow extends vscode.QuickPickItem {
  item?: DialogItem;
}

export function registerOnlineEditCommands(
  context: vscode.ExtensionContext,
  support: BuildSupport,
  connection: ConnectionFeature,
  sourceExtensions: readonly string[],
  /** Shows an error whose text starts with its code, with the link to the page of the code. */
  showCoded: (code: string, text: string) => void,
): void {
  const reopen = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Left, 97);
  reopen.command = ONLINE_EDIT.commandId;
  context.subscriptions.push(reopen);

  let picker: vscode.QuickPick<DialogRow> | undefined;
  // The status the dialog renders: what the device last reported.
  let status: HotEditStatus | undefined = connection.model().lastStatus ?? undefined;
  let connected = connection.model().state === 'connected';
  // The one fact the dialog holds, and the container of the candidate this dialog staged.
  let built: Built | undefined;
  let accepted: Built | undefined;
  let running: string | undefined;
  let rendered = '';

  context.subscriptions.push(
    vscode.commands.registerCommand(ONLINE_EDIT.commandId, open),
    vscode.workspace.onDidChangeTextDocument((event) => {
      // The sources changed after the build: the build is no longer a build of them.
      if (built !== undefined && event.contentChanges.length > 0 && programKind(event.document.uri.fsPath, sourceExtensions) === 'source') {
        built = undefined;
        render();
      }
    }),
    connection.onDidChangeModel((model) => {
      connected = model.state === 'connected';
      if (model.state === 'disconnected') {
        accepted = undefined;
        status = undefined;
        picker?.hide();
      }
      else if (model.lastStatus !== null && running === undefined) {
        status = model.lastStatus;
      }
      render();
    }),
    { dispose: () => picker?.dispose() },
  );

  const fact = (): BuildFact => built === undefined ? 'notBuilt' : 'built';

  /** The hooks of a run that are not plain session commands. */
  const io: OnlineEditIo = {
    build: async () => {
      const program = await resolvePathInteractive(
        'Select the program to build',
        candidate => isDebuggableProgram(candidate, sourceExtensions),
        'IronPLC programs',
        ['iplc', ...sourceExtensions.map(ext => ext.replace('.', ''))],
      );
      if (!program) {
        return false;
      }
      const sent = await support.compile(program);
      if (sent === undefined) {
        return false;
      }
      built = { program, sent };
      return true;
    },
    accept: async () => {
      const session = await support.connectedSession();
      if (session === undefined) {
        return false;
      }
      // The controller is compared with the project again, now: the answer
      // `hotEdit.allowed` is the one the action of the comparison gave, so this
      // stage and the go-online dialog cannot disagree (ADR-0089).
      const gate = acceptGate(await connection.refreshComparison());
      if (!gate.allowed) {
        refuse(gate);
        return false;
      }
      if (built === undefined) {
        void vscode.window.showWarningMessage('IronPLC Online Edit: build the program first.');
        return false;
      }
      await acceptEditsWithDecisions(
        session,
        built.sent.bytes,
        createMigrationDecisionUi(),
        { name: path.basename(built.program), origin: BUILD_ORIGIN },
      );
      accepted = built;
      return true;
    },
    acknowledge: async (after) => {
      if (accepted === undefined) {
        void vscode.window.showWarningMessage(
          'IronPLC Online Edit: the candidate was not staged by this window, so the download was not recorded. Download the project to record it.',
        );
        return;
      }
      await support.acknowledgeAssemble(accepted.sent.path, after);
    },
    onPhase: phase => connection.setBuildPhase(phase),
  };

  async function open(): Promise<void> {
    const session = await support.connectedSession();
    if (session === undefined) {
      return;
    }
    try {
      status = await session.getStatus();
      connection.updateStatus(status);
    }
    catch (err) {
      void vscode.window.showErrorMessage(`IronPLC Online Edit: ${err instanceof Error ? err.message : String(err)}`);
      return;
    }
    if (picker === undefined) {
      const created = vscode.window.createQuickPick<DialogRow>();
      created.ignoreFocusOut = true;
      created.onDidAccept(() => {
        const item = created.selectedItems[0]?.item;
        // A disabled item does nothing; its reason is in its description. Nothing runs while a command runs.
        if (item?.enabled === true && running === undefined) {
          void runSelected(item);
        }
      });
      created.onDidHide(() => {
        // Esc hides only: the candidate stays where it is, and the status bar item reopens the dialog.
        created.dispose();
        if (picker === created) {
          picker = undefined;
        }
      });
      picker = created;
      rendered = '';
    }
    render();
    picker.show();
  }

  async function runSelected(item: DialogItem): Promise<void> {
    const session = await support.connectedSession();
    if (session === undefined) {
      return;
    }
    running = item.label;
    render();
    try {
      const result = await runItem(session, io, item);
      if (result.status !== undefined) {
        status = result.status;
        connection.updateStatus(result.status);
      }
      if (result.outcome.kind === 'done' && result.outcome.assembled) {
        // The build went onto the device.
        built = undefined;
      }
      if (accepted !== undefined && status?.edit?.stage === 'idle') {
        // The candidate this dialog staged is gone: assembled, cancelled, or lost with a restart.
        accepted = undefined;
      }
      show(runMessage(result, formatStatusText));
    }
    finally {
      running = undefined;
      render();
    }
  }

  /**
   * Tells the person why the online change does not begin, in the words of the
   * comparison (the controller's own refusal keeps its V-code), and offers
   * Download when the comparison enables it.
   */
  function refuse(gate: Extract<AcceptGate, { allowed: false }>): void {
    const download = 'Download';
    if (gate.code !== undefined && /^V\d{4}$/.test(gate.code)) {
      showCoded(gate.code, gate.text);
      return;
    }
    const choices = gate.offerDownload ? [download] : [];
    void vscode.window.showWarningMessage(`IronPLC Online Edit: ${gate.text}`, ...choices).then((picked) => {
      if (picked === download) {
        void vscode.commands.executeCommand(DOWNLOAD_ACTIONS.download.commandId);
      }
    });
  }

  function show(message: ReturnType<typeof runMessage>): void {
    if (message === undefined) {
      return;
    }
    if (message.level === 'info') {
      void vscode.window.showInformationMessage(message.text);
    }
    else if (message.vCode !== undefined) {
      showCoded(message.vCode, message.text);
    }
    else {
      void vscode.window.showErrorMessage(message.text);
    }
  }

  /** Sets the widget properties from the model; a model equal to the last one is not set again. */
  function render(): void {
    const bar = connected ? reopenItem(status) : undefined;
    if (bar === undefined) {
      reopen.hide();
    }
    else {
      reopen.text = bar.text;
      reopen.tooltip = bar.tooltip;
      reopen.show();
    }
    if (picker === undefined) {
      return;
    }
    const model = dialogModel(status, fact(), running);
    const key = JSON.stringify(model);
    if (key === rendered) {
      return;
    }
    rendered = key;
    picker.title = model.title;
    picker.step = model.step;
    picker.totalSteps = model.totalSteps;
    picker.busy = model.busy;
    picker.placeholder = model.prompt;
    picker.items = [
      ...model.notices.map((notice): DialogRow => ({ label: notice, kind: vscode.QuickPickItemKind.Separator })),
      ...model.items.map((item): DialogRow => ({ label: item.label, description: item.description, alwaysShow: true, item })),
    ];
  }
}
