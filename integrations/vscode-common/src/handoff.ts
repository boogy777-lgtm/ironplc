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
 * ships under two ids (Open VSX and the Visual Studio Marketplace).
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
 * Whether the IronPLC command of `name` is among `availableCommands` (the ids
 * returned by `vscode.commands.getCommands(true)`).
 */
export function isHandoffAvailable(name: HandoffName, availableCommands: ReadonlySet<string>): boolean {
  return availableCommands.has(HANDOFF_ACTIONS[name].command);
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
