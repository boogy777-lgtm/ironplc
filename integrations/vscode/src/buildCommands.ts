/**
 * The build commands of the engineering connection: `ironplc.build` (Build &
 * Commit, the finalize-equivalent) and `ironplc.buildTrial` (Build & Trial,
 * with the verification checkpoint between upload and resolve). Both reuse
 * the connected `HotEditSession` and the vscode-free sequencing in
 * `buildLogic`; this module is glue — it resolves the program, compiles it
 * with the shared compile path, renders phases and errors, and offers the
 * trial exits. Build refuses while the baseline is stale (E0015, fail-
 * closed) and only in the Connected state, the only state that may send
 * commands. The device's acknowledgement of an assemble (Build & Commit, or
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
import { ProblemCode } from './problems';
import {
  EditIdentity,
  formatStatusDetail,
  formatStatusText,
  HotEditProtocolError,
  HotEditStatus,
} from './hotEditSession';
import { ConnectionFeature } from './connection';
import {
  BuildSession,
  resolveTrial,
  runBuildCommit,
  runBuildTrial,
  TrialResolution,
} from './buildLogic';
import { createMigrationDecisionUi, resolvePathInteractive } from './hotEdit';

/** The container a build sends: the file it is read from, and its bytes. */
interface SentContainer {
  path: string;
  bytes: Uint8Array;
}

/** The advisory origin label every build stamps on its pending-edit record. */
const BUILD_ORIGIN = 'ironplc-vscode';

export function registerBuildCommands(
  context: vscode.ExtensionContext,
  compilerPath: string | undefined,
  sourceExtensions: readonly string[],
  connection: ConnectionFeature,
  reportProblem: ReportProblem,
  buildArea: BuildAreaChannel,
): void {
  context.subscriptions.push(
    vscode.commands.registerCommand('ironplc.build', () => build(false)),
    vscode.commands.registerCommand('ironplc.buildTrial', () => build(true)),
  );

  async function build(trial: boolean): Promise<void> {
    const session: BuildSession | undefined = connection.manager.activeSession;
    if (!session) {
      void vscode.window.showWarningMessage(
        'No device is connected. Run "IronPLC: Connect to Device" first.',
      );
      return;
    }
    // The check of the device against its download record may still be running.
    await connection.settled();
    if (connection.baselineState === 'mismatch') {
      reportProblem(
        ProblemCode.StaleBaseline,
        'The application on the device is not the one this project last downloaded to it. Re-sync the project from the device, rebuild, then connect again.',
      );
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
    let sent: SentContainer;
    try {
      sent = await compileProgram(program);
    }
    catch (err) {
      connection.setBuildPhase(null);
      reportProblem(
        ProblemCode.CompileFailed,
        `${program}: ${err instanceof Error ? err.message : String(err)} (see the "IronPLC Build" output for details).`,
      );
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
  async function acknowledgeAssemble(container: string, status: HotEditStatus): Promise<void> {
    await connection.recordDownload(container, status);
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
}
