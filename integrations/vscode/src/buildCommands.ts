/**
 * The build support of the engineering connection and the Download
 * (`ironplc.download`, the replace mode, ADR-0088). The online-edit dialog
 * (`onlineEditCommands`) sends an edit step by step and shares the compile, the
 * session and the record of an acknowledged assemble through `BuildSupport`, so
 * each exists once. The Download reuses the connected `HotEditSession` and the
 * vscode-free sequencing in `downloadLogic`; this module is glue: it resolves
 * the program, compiles it with the shared compile path, and renders phases and
 * errors. Download does not refuse because the controller holds another
 * application, because it replaces the application and says so in its
 * confirmation. The device's acknowledgement of
 * an assemble (the dialog's Assemble, the Download) is handled in one function,
 * `acknowledgeAssemble`, which records the download. A build writes the shared
 * Build area (`buildAreaLogic`), which it receives from the caller and clears
 * when it starts.
 */

import * as vscode from 'vscode';
import * as path from 'path';
import * as os from 'os';
import { readFile } from 'fs/promises';

import { BuildAreaChannel, buildContainer, startBuild } from './buildAreaLogic';
import { currentCompilerSettings } from './compilerSettings';
import { isDebuggableProgram, programKind } from './debugAdapterLogic';
import { ReportProblem } from './debugAdapter';
import { ProblemCode } from './vscodeCommon';
import {
  formatStatusText,
  HotEditSession,
  HotEditStatus,
} from './hotEditSession';
import { runCompiler } from './taskProviderLogic';
import { MapUidError, syncVariableIds } from './syncUidsLogic';
import { NO_CONTROLLER_MESSAGE } from './controllerRunLogic';
import {
  DOWNLOAD_ACTIONS,
  DownloadIo,
  DownloadOutcome,
  formatReplaceConfirmation,
  outcomeMessage,
  runDownload,
  syncIdsStep,
} from './downloadLogic';
import { ConnectionFeature } from './connection';
import { replacesOtherApplication } from './onlineComparisonLogic';
import { createSyncResolutionUi, resolvePathInteractive } from './hotEdit';

/** The container a build sends: the file it is read from, and its bytes. */
export interface SentContainer {
  path: string;
  bytes: Uint8Array;
}

/** The advisory origin label every build stamps on its pending-edit record. */
export const BUILD_ORIGIN = 'ironplc-vscode';

/** A step of a Download failed and the person has been told: the sequence stops quietly. */
class ReportedError extends Error {}

/**
 * What the commands that send a build share, so each step exists once: the
 * session of the open connection, the compile into the Build area and the
 * handling of an acknowledged assemble. The Download and the online-edit dialog
 * use them.
 */
export interface BuildSupport {
  /** The session of the open connection, or undefined with a warning when there is none. */
  connectedSession(): Promise<HotEditSession | undefined>;
  /**
   * Compiles the program into the Build area (or reads the container). A
   * failure is reported as E0006 and answers undefined.
   */
  compile(program: string): Promise<SentContainer | undefined>;
  /** The device acknowledged an assemble of `container`: records the download. */
  acknowledgeAssemble(container: string, status: HotEditStatus, keepContainer?: boolean): Promise<void>;
}

export function registerBuildCommands(
  context: vscode.ExtensionContext,
  compilerPath: string | undefined,
  sourceExtensions: readonly string[],
  connection: ConnectionFeature,
  reportProblem: ReportProblem,
  buildArea: BuildAreaChannel,
  /** Shows an error whose text starts with its code, with the link to the page of the code. */
  showCoded: (code: string, text: string) => void,
): BuildSupport {
  context.subscriptions.push(
    vscode.commands.registerCommand(DOWNLOAD_ACTIONS.download.commandId, download),
  );

  /**
   * The session of the open connection, or undefined with a warning when there
   * is none. The check of the device against its download record may still be
   * running, so it is awaited: what follows reads its result.
   */
  async function connectedSession(): Promise<HotEditSession | undefined> {
    const session = connection.manager.activeSession;
    if (!session) {
      void vscode.window.showWarningMessage(NO_CONTROLLER_MESSAGE);
      return undefined;
    }
    await connection.settled();
    return session;
  }

  /**
   * The device acknowledged an assemble of the container file `container`: the
   * device now runs it. The one place that handles that acknowledgement, for
   * Build & Commit, the dialog and the Download; it records the download.
   */
  async function acknowledgeAssemble(container: string, status: HotEditStatus, keepContainer = false): Promise<void> {
    await connection.recordDownload(container, status, keepContainer);
  }

  /**
   * Download (ADR-0088): sync the variable IDs, compile, accept in the replace
   * mode, confirm what is re-initialised, then follow the device's edit view to
   * the assemble. `downloadLogic` sequences; this function is the UI and the
   * mapping of each failure to the way it is reported.
   */
  async function download(): Promise<void> {
    const session = await connectedSession();
    if (!session) {
      return;
    }
    const program = await resolvePathInteractive(
      'Select the program to download',
      candidate => isDebuggableProgram(candidate, sourceExtensions),
      'IronPLC programs',
      ['iplc', ...sourceExtensions.map(ext => ext.replace('.', ''))],
    );
    if (!program) {
      return;
    }

    const title = 'Download';
    const io: DownloadIo<SentContainer> = {
      syncIds: () => syncIdsStep(
        programKind(program, sourceExtensions) === 'container',
        () => syncProjectIds(program),
      ),
      compile: async () => {
        const sent = await compileReported(program);
        if (sent === undefined) {
          throw new ReportedError('compile failed');
        }
        return sent;
      },
      confirm: async (request) => {
        const { message, detail } = formatReplaceConfirmation(request);
        const choice = await vscode.window.showWarningMessage(message, { modal: true, detail }, title);
        return choice === title;
      },
      acknowledge: async (sent, status) => {
        await acknowledgeAssemble(sent.path, status, true);
      },
      onPhase: phase => connection.setBuildPhase(phase),
    };
    try {
      const outcome = await runDownload(
        session,
        io,
        { name: path.basename(program), origin: BUILD_ORIGIN },
        replacesOtherApplication(connection.comparison()),
      );
      showDownloadOutcome(outcome, title);
    }
    catch (err) {
      if (err instanceof ReportedError) {
        return;
      }
      void vscode.window.showErrorMessage(
        `IronPLC ${title}: ${err instanceof Error ? err.message : String(err)}`,
      );
    }
    finally {
      connection.setBuildPhase(null);
    }
  }

  /** Shows what a Download came to: the state the device reported, then the message at its level. */
  function showDownloadOutcome(outcome: DownloadOutcome, title: string): void {
    if ('status' in outcome && outcome.status !== undefined) {
      connection.updateStatus(outcome.status);
    }
    const message = outcomeMessage(outcome, title, formatStatusText);
    if (message.level === 'info') {
      void vscode.window.showInformationMessage(message.text);
    }
    else if (message.level === 'warning') {
      void vscode.window.showWarningMessage(message.text);
    }
    else if (message.vCode !== undefined) {
      showCoded(message.vCode, message.text);
    }
    else {
      void vscode.window.showErrorMessage(message.text);
    }
  }

  /**
   * Syncs the stable variable IDs of the project the program belongs to, so the
   * container carries them and the device can keep `PERSISTENT` variables. The
   * same flow as the Sync Variable IDs command, with its resolution UI.
   */
  async function syncProjectIds(program: string) {
    if (!compilerPath) {
      reportProblem(ProblemCode.NoCompiler, 'Install the compiler to download.');
      throw new ReportedError('no compiler');
    }
    try {
      return await syncVariableIds(
        compilerPath,
        program,
        (compiler, args) => runCompiler(compiler, args, text => connection.output.append(text)),
        createSyncResolutionUi(),
      );
    }
    catch (err) {
      reportProblem(
        err instanceof MapUidError ? ProblemCode.MapUidFailed : ProblemCode.CompileFailed,
        `${program}: ${err instanceof Error ? err.message : String(err)} (see the "IronPLC Connection" output for details).`,
      );
      throw new ReportedError('sync failed');
    }
  }

  /** Compiles the program; a failure is reported as E0006 and answers undefined. */
  async function compileReported(program: string): Promise<SentContainer | undefined> {
    try {
      return await compileProgram(program);
    }
    catch (err) {
      reportProblem(
        ProblemCode.CompileFailed,
        `${program}: ${err instanceof Error ? err.message : String(err)} (see the "IronPLC Build" output for details).`,
      );
      return undefined;
    }
  }

  /**
   * Compiles a source program into the Build area, which shows everything the
   * compiler wrote, also when it fails; a container is read as-is (nothing is
   * compiled, but the build has started, so the area is cleared). Resolves with
   * the container file that is sent and its bytes: the program itself for a
   * container, the container compiled into the temporary directory for a source.
   */
  async function compileProgram(program: string): Promise<SentContainer> {
    if (programKind(program, sourceExtensions) === 'container') {
      startBuild(buildArea);
      return { path: program, bytes: await readFile(program) };
    }
    if (!compilerPath) {
      throw new Error('IronPLC compiler not found');
    }
    const container = await buildContainer(buildArea, compilerPath, program, os.tmpdir(), currentCompilerSettings());
    return { path: container, bytes: await readFile(container) };
  }

  return { connectedSession, compile: compileReported, acknowledgeAssemble };
}
