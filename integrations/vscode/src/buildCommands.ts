/**
 * The build commands of the engineering connection: `ironplc.build` (Build &
 * Commit, the finalize-equivalent) and `ironplc.buildTrial` (Build & Trial,
 * with the verification checkpoint between upload and resolve). Both reuse
 * the connected `HotEditSession` and the vscode-free sequencing in
 * `buildLogic`; this module is glue — it resolves the program, compiles it
 * with the shared compile path, renders phases and errors, and offers the
 * trial exits. Build refuses while the baseline is stale (E0015, fail-
 * closed) and only in the Connected state, the only state that may send
 * commands.
 */

import * as vscode from 'vscode';
import * as path from 'path';
import { readFile } from 'fs/promises';

import { compileSourceToContainer } from './taskProviderLogic';
import { isDebuggableProgram, programKind } from './debugAdapterLogic';
import { ReportProblem } from './debugAdapter';
import { ProblemCode } from './problems';
import {
  EditIdentity,
  formatStatusDetail,
  formatStatusText,
  HotEditProtocolError,
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

/** The advisory origin label every build stamps on its pending-edit record. */
const BUILD_ORIGIN = 'ironplc-vscode';

export function registerBuildCommands(
  context: vscode.ExtensionContext,
  compilerPath: string | undefined,
  sourceExtensions: readonly string[],
  connection: ConnectionFeature,
  reportProblem: ReportProblem,
): void {
  const outputChannel = vscode.window.createOutputChannel('IronPLC Build');
  context.subscriptions.push(outputChannel);

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
    if (connection.baselineState === 'mismatch') {
      reportProblem(
        ProblemCode.StaleBaseline,
        'The device no longer matches the project baseline. Re-sync the project from the device, rebuild, then connect again.',
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
    let bytes: Uint8Array;
    try {
      bytes = await compileProgram(program);
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
        await runTrial(session, bytes, edit);
      }
      else {
        const status = await runBuildCommit(
          session,
          bytes,
          edit,
          createMigrationDecisionUi(),
          phase => connection.setBuildPhase(phase),
        );
        connection.captureVerifiedBaseline(bytes, status);
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

  /** Build & Trial: accept → test, then hold for the engineer's resolution. */
  async function runTrial(session: BuildSession, bytes: Uint8Array, edit: EditIdentity): Promise<void> {
    const status = await runBuildTrial(
      session,
      bytes,
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
      connection.captureVerifiedBaseline(bytes, post);
    }
    void vscode.window.showInformationMessage(`IronPLC Build & Trial: ${formatStatusText(post)}.`);
  }

  /** Compiles a source program with the shared compile path; a container is read as-is. */
  async function compileProgram(program: string): Promise<Uint8Array> {
    if (programKind(program, sourceExtensions) === 'container') {
      return readFile(program);
    }
    if (!compilerPath) {
      throw new Error('IronPLC compiler not found');
    }
    const container = await compileSourceToContainer(compilerPath, program, text => outputChannel.append(text));
    return readFile(container);
  }
}
