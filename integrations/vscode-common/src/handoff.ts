/**
 * The handoff from one IronPLC VS Code extension to another: an action the
 * IronLink extension offers but the IronPLC extension performs. IronLink finds
 * controllers; what is done with a controller that was found (record the path
 * of a project) belongs to IronPLC, which writes project data.
 *
 * Each handoff action is one member of {@link HANDOFF_ACTIONS}: it declares the
 * IronPLC command that performs it, the type of the argument that command takes
 * (a member of {@link HandoffArguments}) and the reason shown while the command
 * does not exist. The IronLink extension runs the command, and the IronPLC
 * extension registers it; both import this table, so a command id is written
 * once. The command is looked up by its id among the registered commands, never
 * by the id of the extension that registers it, because the IronPLC extension
 * ships under two ids (Open VSX and the Visual Studio Marketplace). It is found
 * among the commands the installed extensions contribute as well as the ones
 * registered, because an installed extension that has not activated yet has
 * registered nothing.
 */

/** The argument of the `choose` handoff: the controller the engineer chose. */
export interface ChooseControllerArgument {
  /** The device id, 32 lowercase hexadecimal digits. */
  deviceId: string;
  /** The address the controller answered from (an IP address, no port). */
  address: string;
  /** The TCP port of the controller's session. */
  sessionPort: number;
}

/** The argument type of each handoff action, keyed by the action's name. */
export interface HandoffArguments {
  choose: ChooseControllerArgument;
}

/** The name of a handoff action. */
export type HandoffName = keyof HandoffArguments;

/** What one handoff action declares about itself. */
export interface HandoffDeclaration {
  /** The id of the IronPLC command that performs the action. */
  command: string;
  /** Why the action is unavailable while `command` is not registered. */
  unavailableReason: string;
}

/**
 * Every handoff action. The mapped type requires one declaration for each
 * member of {@link HandoffArguments}, so an action added to the argument table
 * without a declaration here (or the reverse) does not compile.
 */
export const HANDOFF_ACTIONS: { readonly [K in HandoffName]: HandoffDeclaration } = {
  choose: {
    command: 'ironplc.chooseController',
    unavailableReason:
      'Choosing a controller needs the IronPLC extension, and the IronPLC command that records '
      + 'the choice is not available. Install or update the IronPLC extension.',
  },
};

/** The names of every handoff action, in declaration order. */
export function handoffNames(): HandoffName[] {
  return Object.keys(HANDOFF_ACTIONS) as HandoffName[];
}

/**
 * The command ids that can run now, from the two places a command can be known:
 *
 * - `registered`: commands an activated extension registered
 *   (`vscode.commands.getCommands(true)`). An installed extension that has not
 *   activated yet has registered nothing, so this alone misses it.
 * - `contributed`: commands an installed extension declares in its manifest
 *   (`contributes.commands`). Running such a command activates its extension
 *   (VS Code 1.74 and later, which both extensions require), so it is available
 *   whether or not the extension is active.
 */
export interface CommandSources {
  registered: ReadonlySet<string>;
  contributed: ReadonlySet<string>;
}

/**
 * The command ids that the manifests (`packageJSON`) of the installed extensions
 * contribute. A manifest that is not an object, or has no well-formed command
 * list, contributes nothing.
 */
export function contributedCommandIds(manifests: ReadonlyArray<unknown>): Set<string> {
  const ids = new Set<string>();
  for (const manifest of manifests) {
    const commands = (manifest as { contributes?: { commands?: unknown } } | null | undefined)?.contributes?.commands;
    if (!Array.isArray(commands)) {
      continue;
    }
    for (const entry of commands) {
      const id = (entry as { command?: unknown } | null | undefined)?.command;
      if (typeof id === 'string') {
        ids.add(id);
      }
    }
  }
  return ids;
}

/**
 * Whether the IronPLC command of `name` can be run: an installed extension
 * contributes it, or an active one has registered it.
 */
export function isHandoffAvailable(name: HandoffName, sources: CommandSources): boolean {
  const command = HANDOFF_ACTIONS[name].command;
  return sources.contributed.has(command) || sources.registered.has(command);
}

/** Runs a command by id; `vscode.commands.executeCommand` has this shape. */
export type ExecuteCommand = (command: string, ...args: unknown[]) => PromiseLike<unknown>;

/** Runs the IronPLC command of the handoff action `name` with its typed argument. */
export async function runHandoff<K extends HandoffName>(
  name: K,
  argument: HandoffArguments[K],
  execute: ExecuteCommand,
): Promise<void> {
  await execute(HANDOFF_ACTIONS[name].command, argument);
}
