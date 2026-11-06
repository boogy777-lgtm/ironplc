import * as vscode from 'vscode';
import { runCompiler } from './taskProviderLogic';
import { programKind } from './debugAdapterLogic';
import { ReportProblem } from './debugAdapter';
import { ProblemCode } from './vscodeCommon';
import { TypeChangePair } from './hotEditSession';
import {
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
 * Registers "IronPLC Hot Edit: Sync Variable IDs", which reconciles the
 * project's stable variable UID sidecar through the `ironplcc refactor
 * sync-uids` / `map-uid` commands (ADR-0053). The edit itself is the
 * online-edit dialog (`onlineEditCommands`), which drives the connected
 * controller; this command sends nothing to a device. The section is glue only:
 * sync parsing and the resolution flow live in the unit-testable
 * `syncUidsLogic` module, and program classification reuses `programKind` so
 * "a source file" means the same thing everywhere in the extension.
 *
 * Like the run commands, this registers unconditionally so the command exists
 * even without a compiler (it reports a coded problem then).
 */
export function registerHotEditSupport(
  context: vscode.ExtensionContext,
  compilerPath: string | undefined,
  sourceExtensions: readonly string[],
  reportProblem: ReportProblem,
): void {
  const outputChannel = vscode.window.createOutputChannel('IronPLC Hot Edit');
  context.subscriptions.push(outputChannel);

  context.subscriptions.push(
    vscode.commands.registerCommand('ironplc.syncVariableIds', syncVariableIdsCommand),
  );

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
      const report = await syncVariableIds(compilerPath, selected, runCompile, resolutionUi);
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
  const resolutionUi: SyncResolutionUi = createSyncResolutionUi();

  /**
   * Runs one compiler invocation, echoing the command and its output to the
   * output channel; rejects with the first diagnostic line on failure. The
   * shared runner lives in `taskProviderLogic`.
   */
  function runCompile(compiler: string, args: string[]): Promise<{ stdout: string; stderr: string }> {
    return runCompiler(compiler, args, text => outputChannel.append(text));
  }

  function reportCompileFailure(source: string, err: unknown): void {
    const detail = err instanceof Error ? err.message : String(err);
    reportProblem(
      ProblemCode.CompileFailed,
      `${source}: ${detail} (see the "IronPLC Hot Edit" output for details).`,
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
 * The vscode callbacks of the stable variable ID sync resolution (ADR-0053),
 * shared by the Sync Variable IDs command and the Download, which syncs first:
 * pick a rename or swap candidate, then confirm its mapping.
 */
export function createSyncResolutionUi(): SyncResolutionUi {
  return {
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
}

/**
 * The vscode callbacks of the ADR-0061 migration decision flow, shared by
 * the Accept of the online-edit dialog: one checkbox row per preservable type change, then the irreversible-action
 * confirmation. The flow planning itself lives in `hotEditMigrationLogic`.
 */
export function createMigrationDecisionUi(): MigrationDecisionUi {
  return {
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
            title: 'IronPLC Online Edit: variables whose type changed',
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
        formatMigrationWarning(pairs, preserved),
        { modal: true },
        'Apply Migration',
      );
      return choice === 'Apply Migration' ? preserved : undefined;
    },
  };
}

/**
 * Shared "active editor, else open dialog" resolution for the commands that
 * operate on one project path: returns the active editor's path when it
 * satisfies `accept`, otherwise asks the user to pick a file.
 */
export async function resolvePathInteractive(
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
