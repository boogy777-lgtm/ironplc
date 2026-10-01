/**
 * The build sequencing of the engineering connection (ADR-0063, Mechanism 3):
 * the Build button wired onto the existing hot-edit FSM, zero new commands.
 * Build & Commit is the finalize-equivalent — one action running
 * `acceptEdits` → mandatory `testEdits` (ADR-0064: assemble requires Test,
 * so Test never runs skipped here and V4017 stays impossible-by-construction)
 * → `assembleEdits` → the post-assemble `getStatus` that shows the new
 * generation. Build & Trial runs accept → test, reports the status, and
 * stops for the engineer to resolve with assemble / untest / cancel.
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

/** The engineer's resolution of a trial run (ADR-0064 commit policies). */
export type TrialResolution = 'assemble' | 'untest' | 'cancel';

/** The minimal session surface a build drives — `HotEditSession` satisfies it. */
export type BuildSession = Pick<HotEditSession, 'acceptEdits' | 'testEdits' | 'assembleEdits' | 'untestEdits' | 'cancelEdits' | 'getStatus'>;

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

/**
 * Build & Trial: upload and run the candidate, then stop — the caller shows
 * the status and offers the exits. A migration candidate has no revert path
 * (the panel omits untest when the status reports `migration: true`, the
 * V4011 case); the caller decides, this sequence only runs accept → test.
 */
export async function runBuildTrial(
  session: BuildSession,
  bytes: Uint8Array,
  edit: EditIdentity,
  decisions: MigrationDecisionUi,
  onPhase: (phase: BuildPhase) => void,
): Promise<HotEditStatus> {
  onPhase('uploading');
  await acceptEditsWithDecisions(session, bytes, decisions, edit);
  onPhase('verifying');
  await session.testEdits();
  // The candidate now executes under Test: the panel's Testing phase, the
  // candidate's live values. The sequence holds here for the engineer.
  onPhase('running');
  return session.getStatus();
}

/** Resolves a finished trial observation: promote, revert, or discard. */
export async function resolveTrial(
  session: BuildSession,
  resolution: TrialResolution,
): Promise<HotEditStatus> {
  if (resolution === 'assemble') {
    await session.assembleEdits();
  }
  else if (resolution === 'untest') {
    await session.untestEdits();
  }
  else {
    await session.cancelEdits();
  }
  return session.getStatus();
}
