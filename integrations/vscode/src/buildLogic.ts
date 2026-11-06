/**
 * The build sequencing of the engineering connection (ADR-0063, Mechanism 3):
 * the Build button wired onto the existing hot-edit FSM, zero new commands.
 * Build & Commit is the finalize-equivalent — one action running
 * `acceptEdits` → mandatory `testEdits` (ADR-0064: assemble requires Test,
 * so Test never runs skipped here and V4017 stays impossible-by-construction)
 * → `assembleEdits` → the post-assemble `getStatus` that shows the new
 * generation. The online-edit dialog (`onlineEditLogic`) runs the same stages
 * one at a time.
 *
 * The phases (compiling → uploading → verifying → running) are the client's
 * own position in the sequence; each is one existing call, reported through
 * the injected `onPhase` so the device panel renders them. Errors are the
 * existing codes: a staging refusal surfaces its V#### through the thrown
 * `HotEditProtocolError`, a migration refusal (V4010) drives the reused
 * ADR-0061 decision flow, and the caller maps a compile failure to E0006.
 *
 * The module is vscode-free: the session is injected as the minimal command
 * surface, so unit tests record the exact call order with a mock.
 */

import {
  EditIdentity,
  HotEditSession,
  HotEditStatus,
} from './hotEditSession';
import {
  acceptEditsWithDecisions,
  MigrationDecisionUi,
} from './hotEditMigrationLogic';

/** The client-side phases of one build, in the order the panel renders them. */
export type BuildPhase = 'compiling' | 'uploading' | 'verifying' | 'running';

/** The minimal session surface a build drives — `HotEditSession` satisfies it. */
export type BuildSession = Pick<HotEditSession, 'acceptEdits' | 'testEdits' | 'assembleEdits' | 'getStatus'>;

/**
 * Build & Commit: upload the compiled `bytes` with the ADR-0064 edit
 * identity, run the candidate under Test, promote it, and return the
 * post-assemble status (the running generation the panel renders).
 */
export async function runBuildCommit(
  session: BuildSession,
  bytes: Uint8Array,
  edit: EditIdentity,
  decisions: MigrationDecisionUi,
  onPhase: (phase: BuildPhase) => void,
): Promise<HotEditStatus> {
  onPhase('uploading');
  await acceptEditsWithDecisions(session, bytes, decisions, edit);
  // Load-verify and the layout_hash comparison are host-side work inside the
  // accept response: by the time it acknowledged, the verify phase is done.
  onPhase('verifying');
  await session.testEdits();
  await session.assembleEdits();
  onPhase('running');
  return session.getStatus();
}
