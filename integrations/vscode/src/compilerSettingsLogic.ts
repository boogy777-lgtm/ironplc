/**
 * The editor settings that change what the compiler does, declared once.
 *
 * A setting of this kind has to reach the compiler in both ways the extension
 * runs it: the language server (`initializationOptions`, read in
 * `compiler/ironplc-cli/src/lsp.rs`) and every `ironplcc compile` the extension
 * starts (the Build area, the build task, the debug launch, the device and the
 * hot-edit compiles). Both are derived here from the one declaration,
 * `COMPILER_SETTINGS`, so a setting cannot reach one and miss the other, and a
 * new setting is one entry:
 *
 * - `serverInitializationOptions` gives the server its keys, the lowerCamelCase
 *   name the compiler derives from the option key it declares;
 * - `compilerFlags` gives the command line its flags, which the compiler
 *   declares as well (`WarningList::cli_flag` in
 *   `compiler/dsl/src/diagnostic/settings.rs`; a unit test reads that file and
 *   fails when the two drift apart).
 *
 * The compile sites take a `CompilerSettings` as an argument (`compileArgs`), so
 * a site that forgets them does not type-check. The settings are read when the
 * compile starts (`readCompilerSettings`, from the glue) and not when the
 * extension is activated, so a change of a setting is used by the next compile.
 *
 * The extension checks no value: the compiler owns the parser of a code (IronPLC
 * `P4083` or CODESYS `C0354`, written exactly so) and refuses one it does not
 * accept. A value in `settings.json` can be anything, so an entry that is not a
 * string is dropped here instead of reaching the compiler as one.
 *
 * Settings that only the extension uses (the compiler path, the connection
 * profiles) are not declared here.
 *
 * This module has no `vscode` import; the glue passes the reader in.
 */

/** A setting that is a list of strings; each entry is one repetition of `flag`. */
interface ListSetting {
  readonly kind: 'list';
  /** The command line flag that adds one entry. */
  readonly flag: string;
}

/**
 * The declaration. The key of an entry is the name of the setting
 * (`ironplc.<name>`) and the key the language server reads.
 */
export const COMPILER_SETTINGS = {
  disabledWarnings: { kind: 'list', flag: '--disable-warning' },
  warningsAsErrors: { kind: 'list', flag: '--warning-as-error' },
} as const satisfies Record<string, ListSetting>;

/** The name of a declared setting. */
export type SettingName = keyof typeof COMPILER_SETTINGS;

/** The value of every declared setting. */
export type CompilerSettings = { [Name in SettingName]: string[] };

/** The names of the declared settings, in the order of the declaration. */
export const SETTING_NAMES = Object.keys(COMPILER_SETTINGS) as SettingName[];

/** The strings of a setting that should be a list of strings, in their order. */
export function stringList(value: unknown): string[] {
  if (!Array.isArray(value)) {
    return [];
  }
  return value.filter((entry): entry is string => typeof entry === 'string');
}

/**
 * The settings as they are now. `get` reads the setting of that name from the
 * configuration (`vscode.workspace.getConfiguration('ironplc').get`); an unset
 * or malformed value is an empty list.
 */
export function readCompilerSettings(get: (name: string) => unknown): CompilerSettings {
  const settings = {} as CompilerSettings;
  for (const name of SETTING_NAMES) {
    settings[name] = stringList(get(name));
  }
  return settings;
}

/** The options of the language server: the keys it reads, with the dialect. */
export interface ServerInitializationOptions extends CompilerSettings {
  dialect: string;
}

/** The `initializationOptions` for a language server started with `settings`. */
export function serverInitializationOptions(
  settings: CompilerSettings,
  dialect: string,
): ServerInitializationOptions {
  return { dialect, ...settings };
}

/**
 * The flags of an `ironplcc` command line for `settings`: the flag of each
 * declared setting once for each entry, in the order of the declaration and of
 * the list.
 */
export function compilerFlags(settings: CompilerSettings): string[] {
  const flags: string[] = [];
  for (const name of SETTING_NAMES) {
    for (const entry of settings[name]) {
      flags.push(COMPILER_SETTINGS[name].flag, entry);
    }
  }
  return flags;
}
