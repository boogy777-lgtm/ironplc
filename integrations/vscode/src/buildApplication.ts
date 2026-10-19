/**
 * The command `ironplc.buildApplication` ("IronPLC: Build"): compiles the
 * project of a workspace folder as the `ironplc: compile` task does and shows
 * the messages in the Build area, the shared "IronPLC Build" Output channel.
 * This module is glue; the choice of the folder and what is written for each
 * outcome are in `buildAreaLogic`.
 */

import * as vscode from 'vscode';

import { currentCompilerSettings } from './compilerSettings';
import { ReportProblem } from './debugAdapter';
import { ProblemCode } from './problems';
import {
  buildFolder,
  chooseFolder,
  FolderRef,
  NO_FOLDER_MESSAGE,
  problemOfOutcome,
} from './buildAreaLogic';

/** The folder of the build: the one choice, or the person's pick. */
async function folderToBuild(): Promise<FolderRef | undefined> {
  const folders: FolderRef[] = (vscode.workspace.workspaceFolders ?? [])
    .map(folder => ({ name: folder.name, path: folder.uri.fsPath }));
  const active = vscode.window.activeTextEditor?.document.uri;
  const choice = chooseFolder(folders, active?.scheme === 'file' ? active.fsPath : undefined);
  switch (choice.kind) {
    case 'none':
      void vscode.window.showWarningMessage(NO_FOLDER_MESSAGE);
      return undefined;
    case 'folder':
      return choice.folder;
    case 'ask': {
      const picked = await vscode.window.showQuickPick(
        choice.folders.map(folder => ({ label: folder.name, description: folder.path, folder })),
        { placeHolder: 'Select the workspace folder to build' },
      );
      return picked?.folder;
    }
  }
}

export function registerBuildApplication(
  context: vscode.ExtensionContext,
  compilerPath: string | undefined,
  buildArea: vscode.OutputChannel,
  reportProblem: ReportProblem,
): void {
  context.subscriptions.push(
    vscode.commands.registerCommand('ironplc.buildApplication', async () => {
      const folder = await folderToBuild();
      if (!folder) {
        return;
      }
      if (!compilerPath) {
        reportProblem(ProblemCode.NoCompiler, 'Install the compiler to build.');
        return;
      }
      const outcome = await buildFolder(buildArea, compilerPath, folder, currentCompilerSettings());
      const problem = problemOfOutcome(compilerPath, outcome);
      if (problem) {
        reportProblem(problem.code, problem.context);
      }
    }),
  );
}
