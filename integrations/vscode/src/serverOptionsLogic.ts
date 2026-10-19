/**
 * The options the extension gives the language server when it starts it
 * (`initializationOptions`). The server reads them in
 * `compiler/ironplc-cli/src/lsp.rs` (`extract_compiler_options`): the key of
 * each is the lowerCamelCase form of the option key the compiler declares
 * (`CompilerOptions`, `WarningList::option_key`), so a key written here that the
 * compiler does not know is ignored by the server, never an error.
 *
 * The two lists of message codes are the settings `ironplc.disabledWarnings`
 * and `ironplc.warningsAsErrors`. The extension does not check a code: the
 * compiler owns the one parser of a code (IronPLC `P4083` or CODESYS `C0354`,
 * written exactly as the compiler writes it) and reports a code it does not
 * accept. A value in `settings.json` can be anything, so an entry that is not
 * a string is dropped here instead of reaching the server as a non-string.
 *
 * This module has no `vscode` import; the glue passes the settings in.
 */

/** The settings that become `initializationOptions`. */
export interface ServerSettings {
  dialect: string;
  disabledWarnings: unknown;
  warningsAsErrors: unknown;
}

/** The options of the language server: the keys the server reads. */
export interface ServerInitializationOptions {
  dialect: string;
  disabledWarnings: string[];
  warningsAsErrors: string[];
}

/** The strings of a setting that should be a list of strings, in their order. */
export function stringList(value: unknown): string[] {
  if (!Array.isArray(value)) {
    return [];
  }
  return value.filter((entry): entry is string => typeof entry === 'string');
}

/** The `initializationOptions` for a language server started with `settings`. */
export function serverInitializationOptions(settings: ServerSettings): ServerInitializationOptions {
  return {
    dialect: settings.dialect,
    disabledWarnings: stringList(settings.disabledWarnings),
    warningsAsErrors: stringList(settings.warningsAsErrors),
  };
}
