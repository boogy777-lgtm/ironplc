/**
 * The build commands of the engineering connection: `ironplc.build` (Build &
 * Commit, the finalize-equivalent), `ironplc.buildTrial` (Build & Trial,
 * with the verification checkpoint between upload and resolve) and
 * `ironplc.download` (Download, the replace mode, ADR-0088). All reuse
 * the connected `HotEditSession` and the vscode-free sequencing in
 * `buildLogic` and `downloadLogic`; this module is glue — it resolves the program, compiles it
 * with the shared compile path, renders phases and errors, and offers the
 * trial exits. Build refuses while the baseline is stale (E0015, fail-
 * closed) and only in the Connected state, the only state that may send
 * commands; Download does not refuse a stale baseline, because it replaces the
 * application and says so in its confirmation. The device's acknowledgement of an assemble (Build & Commit, or
 * the assemble that resolves a trial) is handled in one function,
 * `acknowledgeAssemble`, which records the download. A build writes the shared Build area (`buildAreaLogic`), which it
 * receives from the caller and clears when it starts.
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
  EditIdentity,
  formatStatusDetail,
  formatStatusText,
  HotEditProtocolError,
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
import {
  BuildSession,
  resolveTrial,
  runBuildCommit,
  runBuildTrial,
  TrialResolution,
} from './buildLogic';
import { createMigrationDecisionUi, createSyncResolutionUi, resolvePathInteractive } from './hotEdit';

/** The container a build sends: the file it is read from, and its bytes. */
export interface SentContainer {
  path: string;
  bytes: Uint8Array;
}

/** The advisory origin label every build stamps on its pending-edit record. */
export const BUILD_ORIGIN = 'ironplc-vscode';

/** What the person is told when a build would go over an application that was not downloaded from this project (E0015). */
export const STALE_BASELINE_DETAIL
  = 'The application on the device is not the one this project last downloaded to it. Re-sync the project from the device, rebuild, then connect again.';

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
    vscode.commands.registerCommand('ironplc.build', () => build(false)),
    vscode.commands.registerCommand('ironplc.buildTrial', () => build(true)),
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

  async function build(trial: boolean): Promise<void> {
    const session: BuildSession | undefined = await connectedSession();
    if (!session) {
      return;
    }
    if (connection.baselineState === 'mismatch') {
      reportProblem(ProblemCode.StaleBaseline, STALE_BASELINE_DETAIL);
      return;
    }

    const program = await resolvePathInteractive(
      trial ? 'Select the program to build and trial' : 'Select the program to build',
      candidate => isDebuggableProgram(candidate, sourceExtensions),
      'IronPLC programs',
      ['iplc', ...sourceExtensions.map(ext => ext.replace('.', ''))],
    );
    if (!program) {
      return;
    }

    connection.setBuildPhase('compiling');
    const sent = await compileReported(program);
    if (sent === undefined) {
      connection.setBuildPhase(null);
      return;
    }

    const edit: EditIdentity = { name: path.basename(program), origin: BUILD_ORIGIN };
    try {
      if (trial) {
        await runTrial(session, sent, edit);
      }
      else {
        const status = await runBuildCommit(
          session,
          sent.bytes,
          edit,
          createMigrationDecisionUi(),
          phase => connection.setBuildPhase(phase),
        );
        await acknowledgeAssemble(sent.path, status);
        void vscode.window.showInformationMessage(`IronPLC Build: ${formatStatusText(status)}.`);
      }
    }
    catch (err) {
      if (err instanceof HotEditProtocolError) {
        void vscode.window.showErrorMessage(`IronPLC Build: ${err.toString()}`);
      }
      else {
        void vscode.window.showErrorMessage(
          `IronPLC Build: ${err instanceof Error ? err.message : String(err)}`,
        );
      }
    }
    finally {
      connection.setBuildPhase(null);
    }
  }

  /**
   * The device acknowledged an assemble of the container file `container`: the
   * device now runs it. The one place that handles that acknowledgement, for
   * Build & Commit and for the assemble that resolves a trial; it records the
   * download.
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
        connection.baselineState === 'mismatch',
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

  /** Build & Trial: accept → test, then hold for the engineer's resolution. */
  async function runTrial(session: BuildSession, sent: SentContainer, edit: EditIdentity): Promise<void> {
    const status = await runBuildTrial(
      session,
      sent.bytes,
      edit,
      createMigrationDecisionUi(),
      phase => connection.setBuildPhase(phase),
    );
    connection.updateStatus(status);

    // A migration candidate has no revert path (untest answers V4011), so
    // the exit list omits Untest — commit or cancel are the only exits.
    const exits = status.migration
      ? ['Assemble', 'Cancel']
      : ['Assemble', 'Untest', 'Cancel'];
    const picked = await vscode.window.showInformationMessage(
      `IronPLC Build & Trial (the candidate is running under Test):\n${formatStatusDetail(status)}`,
      ...exits,
    );
    const resolution: TrialResolution | undefined = picked === 'Assemble'
      ? 'assemble'
      : picked === 'Untest'
        ? 'untest'
        : picked === 'Cancel'
          ? 'cancel'
          : undefined;
    if (resolution === undefined) {
      // Esc keeps the candidate under Test; the device panel shows the
      // Testing state and the exits stay available on the next build.
      return;
    }
    const post = await resolveTrial(session, resolution);
    connection.updateStatus(post);
    if (resolution === 'assemble') {
      await acknowledgeAssemble(sent.path, post);
    }
    void vscode.window.showInformationMessage(`IronPLC Build & Trial: ${formatStatusText(post)}.`);
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
