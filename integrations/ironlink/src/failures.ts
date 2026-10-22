/**
 * The typed failures of the IronLink extension, and how one is shown.
 *
 * A failure is one of two things, and each has a code:
 * - `IronLinkProblem`: the extension's own error, an `E####` code of the shared
 *   catalog (the program is missing, did not start, exited, or answered
 *   something unreadable);
 * - `IronLinkCommandError`: `ironlink` refused a command and said why, with its
 *   `L####` code and message.
 *
 * Both carry a code, so both are shown with a link to the page that documents
 * it (`problemHelpUrl`). No error text is written here: the catalog owns the
 * messages, and the program owns its own.
 */

import { ProblemCode, formatProblem, problemHelpUrl } from './vscodeCommon';

/** An error of the extension itself: a code of the shared `E####` catalog. */
export class IronLinkProblem extends Error {
  constructor(
    readonly problem: ProblemCode,
    readonly detail?: string,
  ) {
    super(formatProblem(problem, detail));
    this.name = 'IronLinkProblem';
  }
}

/** A command that `ironlink` answered with an error response that carries an L-code. */
export class IronLinkCommandError extends Error {
  constructor(
    readonly lCode: string,
    readonly reason: string,
  ) {
    super(formatCommandError(lCode, reason));
    this.name = 'IronLinkCommandError';
  }
}

/** `L#### - message`, the form of every coded message. */
export function formatCommandError(lCode: string, reason: string): string {
  return `${lCode} - ${reason}`;
}

/** A failure ready to show: its text and the page that documents its code. */
export interface ShownFailure {
  text: string;
  /** The documentation URL of the failure's code. */
  helpUrl: string;
}

/**
 * How a failure is shown. `version` is the version of the extension, which the
 * documentation site uses to tell a stale page from a current one. An error
 * that is neither failure is not an IronLink error and has no code: it is
 * shown as the extension's protocol problem rather than as a bare string.
 */
export function describeFailure(error: unknown, version: string): ShownFailure {
  if (error instanceof IronLinkCommandError) {
    return { text: error.message, helpUrl: problemHelpUrl(error.lCode, version) };
  }
  if (error instanceof IronLinkProblem) {
    return { text: error.message, helpUrl: problemHelpUrl(error.problem, version) };
  }
  const unexpected = new IronLinkProblem(
    ProblemCode.IronLinkProtocolError,
    error instanceof Error ? error.message : String(error),
  );
  return { text: unexpected.message, helpUrl: problemHelpUrl(unexpected.problem, version) };
}
