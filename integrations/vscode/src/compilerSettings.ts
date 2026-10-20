import * as vscode from 'vscode';

import { CompilerSettings, readCompilerSettings } from './compilerSettingsLogic';

/**
 * The compiler settings now, read from the `ironplc` configuration. Every site
 * that starts the compiler asks this when it starts, so the change of a setting
 * is used by the next compile. Glue only: the declaration and the projections
 * are in `compilerSettingsLogic.ts`.
 */
export function currentCompilerSettings(): CompilerSettings {
  const config = vscode.workspace.getConfiguration('ironplc');
  return readCompilerSettings(name => config.get<unknown>(name));
}
