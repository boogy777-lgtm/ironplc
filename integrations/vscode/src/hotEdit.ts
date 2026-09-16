import * as vscode from 'vscode';
import * as os from 'os';
import * as path from 'path';
import { execFile, spawn, ChildProcessWithoutNullStreams } from 'child_process';
import { readFile } from 'fs/promises';
import { existsSync } from 'fs';
import { compileArgs } from './taskProviderLogic';
import {
  containerOutputPath,
  firstLine,
  isDebuggableProgram,
  programKind,
} from './debugAdapterLogic';
import { ReportProblem } from './debugAdapter';
import { ProblemCode } from './problems';
import {
  formatStatusDetail,
  formatStatusText,
  HotEditProtocolError,
  HotEditSession,
  HotEditTransport,
  TypeChangePair,
} from './hotEditSession';
import {
  acceptEditsWithDecisions,
  formatMigrationWarning,
  MigrationDecisionUi,
  preservablePairs,
} from './hotEditMigrationLogic';
import {
  candidatesOf,
  describeMapping,
  formatSyncSummary,
  MapUidError,
  SyncCandidate,
  syncVariableIds,
  SyncResolutionUi,
} from './syncUidsLogic';

/**
 * Registers the "IronPLC Hot Edit" commands: a thin client that drives one
 * `ironplcvm serve` session (the hot-edit engineering protocol, ADR-0052)
 * over the child's stdin/stdout, plus the "Sync Variable IDs" command that
 * reconciles the project's stable variable UID sidecar through the
 * `ironplcc refactor sync-uids` / `map-uid` commands (ADR-0053). The
 * sections are glue only — protocol framing and response matching live in
 * the unit-testable `hotEditSession` module, sync parsing and the
 * resolution flow live in the unit-testable `syncUidsLogic` module, the
 * ADR-0061 migration decision planning lives in the unit-testable
 * `hotEditMigrationLogic` module, compilation reuses the debug adapter's
 * compile path (`compileArgs` + `containerOutputPath`), and program
 * classification reuses `programKind`/`isDebuggableProgram` so "a runnable
 * file" means the same thing everywhere in the extension.
 *
 * Like the run commands, this registers unconditionally so the commands exist
 * even without a compiler (they report a coded problem or warn when the
 * compiler, the VM, or a session is missing).
 */
export function registerHotEditSupport(
  context: vscode.ExtensionContext,
  compilerPath: string | undefined,
  sourceExtensions: readonly string[],
  reportProblem: ReportProblem,
): void {
  const compilerDir = compilerPath ? path.dirname(compilerPath) : undefined;

  const outputChannel = vscode.window.createOutputChannel('IronPLC Hot Edit');
  context.subscriptions.push(outputChannel);

  const statusItem = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Left, 98);
  statusItem.command = 'ironplc.showHotEditStatus';
  statusItem.tooltip = 'IronPLC hot edit status';
  context.subscriptions.push(statusItem);

  let session: HotEditSession | undefined;
  let child: ChildProcessWithoutNullStreams | undefined;
  let program: string | undefined;
  let stopping = false;

  context.subscriptions.push({ dispose: () => stopSession() });

  context.subscriptions.push(
    vscode.commands.registerCommand('ironplc.startHotEditSession', startSession),
    vscode.commands.registerCommand('ironplc.acceptEdits', acceptEdits),
    vscode.commands.registerCommand('ironplc.testEdits', () => runSessionCommand(s => s.testEdits())),
    vscode.commands.registerCommand('ironplc.untestEdits', () => runSessionCommand(s => s.untestEdits())),
    vscode.commands.registerCommand('ironplc.assembleEdits', () => runSessionCommand(s => s.assembleEdits())),
    vscode.commands.registerCommand('ironplc.cancelEdits', () => runSessionCommand(s => s.cancelEdits())),
    vscode.commands.registerCommand('ironplc.showHotEditStatus', showStatus),
    vscode.commands.registerCommand('ironplc.syncVariableIds', syncVariableIdsCommand),
  );

  async function startSession(): Promise<void> {
    if (session) {
      void vscode.window.showInformationMessage(
        `A hot edit session is already running for "${program}".`,
      );
      return;
    }

    const selected = await resolveProgram();
    if (!selected) {
      return;
    }

    if (!compilerPath || !compilerDir) {
      reportProblem(ProblemCode.NoCompiler, 'Install the compiler to start a hot edit session.');
      return;
    }
    const vmPath = path.join(compilerDir, vmFileName(process.platform));
    if (!existsSync(vmPath)) {
      reportProblem(
        ProblemCode.VmNotFound,
        `Install the IronPLC VM so ${vmFileName(process.platform)} sits next to the compiler at "${compilerDir}".`,
      );
      return;
    }

    // The serve session loads a compiled container, so a source program is
    // compiled first — the same single-file compile the debug launch uses.
    let container = selected;
    if (programKind(selected, sourceExtensions) === 'source') {
      try {
        container = await compileToContainer(compilerPath, selected);
      }
      catch (err) {
        reportCompileFailure(selected, err);
        return;
      }
    }

    outputChannel.appendLine(`$ ${vmPath} serve ${container}`);
    const spawned = spawn(vmPath, ['serve', container]);
    child = spawned;
    spawned.stderr.on('data', (chunk: Buffer) => outputChannel.append(chunk.toString()));

    session = new HotEditSession(new StdioLineTransport(spawned), () => {
      const expected = stopping;
      stopping = false;
      session = undefined;
      child = undefined;
      program = undefined;
      statusItem.hide();
      if (!expected) {
        void vscode.window.showErrorMessage(
          'IronPLC Hot Edit: the ironplcvm process exited unexpectedly. See the "IronPLC Hot Edit" output for details.',
        );
      }
    });
    program = selected;

    try {
      await refreshStatus();
    }
    catch (err) {
      // The child died at startup (for example a container from another VM
      // version); its stderr is already in the output channel.
      showSessionError(err);
      stopSession();
    }
  }

  async function acceptEdits(): Promise<void> {
    const current = requireSession();
    if (!current) {
      return;
    }

    const editor = vscode.window.activeTextEditor;
    if (!editor) {
      void vscode.window.showWarningMessage('Open the program to accept as edits first.');
      return;
    }

    try {
      let bytes: Uint8Array;
      if (programKind(editor.document.uri.fsPath, sourceExtensions) === 'container') {
        bytes = await readFile(editor.document.uri.fsPath);
      }
      else {
        if (!compilerPath) {
          reportProblem(ProblemCode.NoCompiler, 'Install the compiler to accept edits.');
          return;
        }
        if (!(await editor.document.save())) {
          void vscode.window.showWarningMessage('Save the program before accepting it as edits.');
          return;
        }
        const compiled = await compileToContainer(compilerPath, editor.document.uri.fsPath);
        bytes = await readFile(compiled);
      }
      await acceptEditsWithDecisions(current, bytes, migrationUi);
      await refreshStatus();
    }
    catch (err) {
      if (err instanceof HotEditProtocolError) {
        showSessionError(err);
      }
      else {
        reportCompileFailure(editor.document.uri.fsPath, err);
      }
    }
  }

  async function runSessionCommand(action: (s: HotEditSession) => Promise<void>): Promise<void> {
    const current = requireSession();
    if (!current) {
      return;
    }
    try {
      await action(current);
      await refreshStatus();
    }
    catch (err) {
      showSessionError(err);
    }
  }

  async function showStatus(): Promise<void> {
    const current = requireSession();
    if (!current) {
      return;
    }
    try {
      const status = await current.getStatus();
      void vscode.window.showInformationMessage(`IronPLC Hot Edit\n${formatStatusDetail(status)}`);
    }
    catch (err) {
      showSessionError(err);
    }
  }

  async function syncVariableIdsCommand(): Promise<void> {
    const selected = await resolveProject();
    if (!selected) {
      return;
    }
    if (!compilerPath) {
      reportProblem(ProblemCode.NoCompiler, 'Install the compiler to sync variable IDs.');
      return;
    }

    try {
      const report = await syncVariableIds(compilerPath, selected, runCompiler, resolutionUi);
      if (candidatesOf(report).length > 0) {
        void vscode.window.showWarningMessage(
          `IronPLC Hot Edit: Sync Variable IDs: ${formatSyncSummary(report)}. `
          + 'Rename/swap candidates remain unresolved, so variable IDs are not fully synced. '
          + 'Run the command again to resolve them.',
        );
      }
      else {
        void vscode.window.showInformationMessage(
          `IronPLC Hot Edit: Sync Variable IDs: ${formatSyncSummary(report)}.`,
        );
      }
    }
    catch (err) {
      if (err instanceof MapUidError) {
        reportProblem(
          ProblemCode.MapUidFailed,
          `${selected}: ${err.message} (see the "IronPLC Hot Edit" output for details).`,
        );
      }
      else {
        reportCompileFailure(selected, err);
      }
    }
  }

  /** UI callbacks for the sync resolution flow; the flow logic itself is unit-testable. */
  const resolutionUi: SyncResolutionUi = {
    async pickCandidate(candidates: SyncCandidate[]): Promise<SyncCandidate | undefined> {
      const picked = await vscode.window.showQuickPick(
        candidates.map(candidate => ({
          label: `${candidate.kind === 'rename' ? 'Rename' : 'Swap'}: ${describeMapping(candidate)}`,
          candidate,
        })),
        { title: 'Select a candidate to resolve, or press Esc to leave the IDs unresolved' },
      );
      return picked?.candidate;
    },
    async confirmMapping(mapping: string): Promise<boolean> {
      const choice = await vscode.window.showInformationMessage(
        `Map ${mapping}? The recorded variable ID moves to the new declaration.`,
        'Map',
        'Cancel',
      );
      return choice === 'Map';
    },
  };

  /** UI callbacks for the migration decision flow; the flow logic itself is unit-testable. */
  const migrationUi: MigrationDecisionUi = {
    async choosePreserved(pairs: readonly TypeChangePair[]): Promise<readonly number[] | undefined> {
      const preservable = preservablePairs(pairs);
      let preserved: readonly number[] = [];
      if (preservable.length > 0) {
        const picked = await vscode.window.showQuickPick(
          preservable.map(pair => ({
            label: pair.name ?? `Variable ${pair.uid}`,
            description: `${pair.from} -> ${pair.to}`,
            detail: `uid ${pair.uid}: keep the raw bits; the value may no longer be valid`,
            pair,
          })),
          {
            title: 'IronPLC Hot Edit: variables whose type changed',
            placeHolder: 'Checked variables preserve their storage bytes; unchecked variables are reinitialized (the default).',
            canPickMany: true,
          },
        );
        if (!picked) {
          return undefined;
        }
        preserved = picked.map(item => item.pair.uid);
      }
      const choice = await vscode.window.showWarningMessage(
        formatMigrationWarning(pairs.length, preserved.length),
        { modal: true },
        'Apply Migration',
      );
      return choice === 'Apply Migration' ? preserved : undefined;
    },
  };

  function requireSession(): HotEditSession | undefined {
    if (!session) {
      void vscode.window.showWarningMessage(
        'No hot edit session is running. Run "IronPLC Hot Edit: Start Session" first.',
      );
      return undefined;
    }
    return session;
  }

  async function refreshStatus(): Promise<void> {
    const status = await session!.getStatus();
    statusItem.text = `$(sync) Hot Edit: ${formatStatusText(status)}`;
    statusItem.tooltip = formatStatusDetail(status);
    statusItem.show();
  }

  function stopSession(): void {
    if (!session && !child) {
      return;
    }
    stopping = true;
    session?.dispose();
    session = undefined;
    child?.kill();
    child = undefined;
    program = undefined;
    statusItem.hide();
  }

  /** The serve session loads a compiled container; a source program must be compiled first. */
  function compileToContainer(compiler: string, source: string): Promise<string> {
    const output = containerOutputPath(source, os.tmpdir());
    const args = compileArgs(source, output);
    return runCompiler(compiler, args).then(() => output);
  }

  /**
   * Runs one compiler invocation, echoing the command and its output to the
   * output channel; rejects with the first diagnostic line on failure. The
   * single transport every compiler-spawning command in this section shares.
   */
  function runCompiler(compiler: string, args: string[]): Promise<{ stdout: string; stderr: string }> {
    outputChannel.appendLine(`$ ${compiler} ${args.join(' ')}`);
    return new Promise((resolve, reject) => {
      execFile(compiler, args, (error, stdout, stderr) => {
        if (stdout) {
          outputChannel.append(stdout);
        }
        if (stderr) {
          outputChannel.append(stderr);
        }
        if (error) {
          const detail = firstLine(stderr) || firstLine(stdout) || String(error);
          reject(new Error(detail));
          return;
        }
        resolve({ stdout, stderr });
      });
    });
  }

  function reportCompileFailure(source: string, err: unknown): void {
    const detail = err instanceof Error ? err.message : String(err);
    reportProblem(
      ProblemCode.CompileFailed,
      `${source}: ${detail} (see the "IronPLC Hot Edit" output for details).`,
    );
  }

  function showSessionError(err: unknown): void {
    if (err instanceof HotEditProtocolError) {
      void vscode.window.showErrorMessage(`IronPLC Hot Edit: ${err.toString()}`);
      return;
    }
    void vscode.window.showErrorMessage(`IronPLC Hot Edit: ${err instanceof Error ? err.message : String(err)}`);
  }

  async function resolveProgram(): Promise<string | undefined> {
    return resolvePathInteractive(
      'Select the program to hot edit',
      candidate => isDebuggableProgram(candidate, sourceExtensions),
      'IronPLC programs',
      ['iplc', ...sourceExtensions.map(ext => ext.replace('.', ''))],
    );
  }

  /**
   * The project the sync command reconciles: the active editor when it holds
   * a source file (sync analyzes sources; a compiled container is not a
   * valid input), otherwise a file picked from the same open dialog pattern.
   */
  async function resolveProject(): Promise<string | undefined> {
    return resolvePathInteractive(
      'Select the project to sync variable IDs',
      candidate => programKind(candidate, sourceExtensions) === 'source',
      'IronPLC sources',
      sourceExtensions.map(ext => ext.replace('.', '')),
    );
  }
}

/**
 * Shared "active editor, else open dialog" resolution for the commands that
 * operate on one project path: returns the active editor's path when it
 * satisfies `accept`, otherwise asks the user to pick a file.
 */
async function resolvePathInteractive(
  title: string,
  accept: (path: string) => boolean,
  filterName: string,
  extensions: string[],
): Promise<string | undefined> {
  const active = vscode.window.activeTextEditor?.document.uri.fsPath;
  if (active && accept(active)) {
    return active;
  }
  const picked = await vscode.window.showOpenDialog({
    title,
    filters: { [filterName]: extensions },
  });
  return picked && picked.length > 0 ? picked[0].fsPath : undefined;
}

/** The VM executable name on `platform` (`.exe` on Windows). */
function vmFileName(platform: string): string {
  return platform === 'win32' ? 'ironplcvm.exe' : 'ironplcvm';
}

/**
 * Line-buffers the child process's stdio into [`HotEditTransport`]: stdout is
 * the protocol channel (split into lines, carriage returns trimmed), stdin
 * carries one JSON command per line.
 */
class StdioLineTransport implements HotEditTransport {
  private readonly lineListeners: ((line: string) => void)[] = [];
  private readonly exitListeners: (() => void)[] = [];
  private exited = false;

  constructor(private readonly child: ChildProcessWithoutNullStreams) {
    let buffer = '';
    child.stdout.on('data', (chunk: Buffer) => {
      buffer += chunk.toString();
      for (;;) {
        const newline = buffer.indexOf('\n');
        if (newline < 0) {
          break;
        }
        let line = buffer.slice(0, newline);
        buffer = buffer.slice(newline + 1);
        if (line.endsWith('\r')) {
          line = line.slice(0, -1);
        }
        if (line.length > 0) {
          this.emitLine(line);
        }
      }
    });
    // A spawn failure surfaces as 'error' (and may not be followed by
    // 'exit'); either way the channel is gone, so report it once.
    child.on('error', () => this.emitExit());
    child.on('exit', () => this.emitExit());
  }

  sendLine(line: string): void {
    this.child.stdin.write(line + '\n');
  }

  onLine(listener: (line: string) => void): void {
    this.lineListeners.push(listener);
  }

  onExit(listener: () => void): void {
    this.exitListeners.push(listener);
  }

  private emitLine(line: string): void {
    for (const listener of this.lineListeners) {
      listener(line);
    }
  }

  private emitExit(): void {
    if (this.exited) {
      return;
    }
    this.exited = true;
    for (const listener of this.exitListeners) {
      listener();
    }
  }
}
