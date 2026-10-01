/**
 * The VS Code side of the stable variable UID sync flow (ADR-0053): a thin,
 * unit-testable client for the `ironplcc refactor sync-uids` and
 * `ironplcc refactor map-uid` commands.
 *
 * The compiler prints the `SyncReport` verbatim and documents the text as
 * the machine-readable contract for tooling, so [`parseSyncReport`] pins
 * that format rather than scraping an unstable rendering. The module owns
 * no process or UI state — the caller injects an [`ExecFileFn`] transport
 * (the extension glue builds one on `child_process.execFile`, mirroring
 * `hotEditSession`'s transport injection) and [`SyncResolutionUi`]
 * callbacks for the quick-pick resolution, which keeps every flow decision
 * (sync, candidate pick, mapping confirm, re-sync) unit-testable without a
 * real `ironplcc` child process.
 *
 * One CLI behavior drives the flow here: `sync-uids` rewrites the sidecar
 * only when the reconciliation is unambiguous. A report with rename/swap
 * candidates is printed but NOT persisted (saving would drop the removed
 * keys, leaving nothing for `map-uid` to move), so the resolution sequence
 * sync -> pick candidate -> confirm mapping -> map-uid -> re-sync stays
 * possible.
 */

/** A sidecar key: the declaring scope path (`global` or a program name) and the variable name. */
export interface SidecarKey {
  scope: string;
  name: string;
}

/** One report entry: a key and the UID it kept or was assigned. */
export interface KeyedUid {
  key: SidecarKey;
  uid: number;
}

/** The parsed `SyncReport` text: five sections, one entry per line. */
export interface SyncReport {
  preserved: KeyedUid[];
  assigned: KeyedUid[];
  removed: KeyedUid[];
  renameCandidates: { old: SidecarKey; new: SidecarKey }[];
  swapCandidates: { removed: [SidecarKey, SidecarKey]; added: [SidecarKey, SidecarKey] }[];
}

/** The five report sections, keyed by their internal short name. */
type Section = 'preserved' | 'assigned' | 'removed' | 'rename' | 'swap';

/**
 * A resolution the user can apply: a rename moves one removed key's UID to
 * one added key; a swap moves both removed keys' UIDs to the added keys
 * (paired by sorted position, exactly as the CLI reports it).
 */
export type SyncCandidate
  = | { kind: 'rename'; old: SidecarKey; new: SidecarKey }
    | { kind: 'swap'; removed: [SidecarKey, SidecarKey]; added: [SidecarKey, SidecarKey] };

/** `sync-uids` failed (transport or report parsing): the project did not produce a usable report. */
export class SyncUidsError extends Error {
  constructor(message: string) {
    super(message);
    this.name = 'SyncUidsError';
  }
}

/** `map-uid` failed: the recorded mapping could not be updated (for example, the sidecar changed in the meantime). */
export class MapUidError extends Error {
  constructor(message: string) {
    super(message);
    this.name = 'MapUidError';
  }
}

/**
 * The transport one compiler invocation speaks over. The extension glue
 * implements this on `child_process.execFile`; unit tests inject a mock, so
 * no real child process is needed to exercise the flow.
 */
export type ExecFileFn
  = (file: string, args: string[]) => Promise<{ stdout: string; stderr: string }>;

/** UI decisions the resolution flow needs, injected so the flow is unit-testable. */
export interface SyncResolutionUi {
  /**
   * Presents the candidates of the latest report and returns the one to
   * resolve now, or `undefined` to stop and leave the rest unresolved.
   */
  pickCandidate(candidates: SyncCandidate[]): Promise<SyncCandidate | undefined>;
  /**
   * Asks the user to confirm applying `mapping` (see [`describeMapping`]).
   * Returns false to leave this candidate unresolved and stop.
   */
  confirmMapping(mapping: string): Promise<boolean>;
}

/** Builds the `refactor sync-uids` argument vector for `projectPath`. */
export function syncUidsArgs(projectPath: string): string[] {
  return ['refactor', 'sync-uids', projectPath];
}

/** Builds the `refactor map-uid` argument vector moving `oldKey`'s UID to `newKey`. */
export function mapUidArgs(projectPath: string, oldKey: SidecarKey, newKey: SidecarKey): string[] {
  return [
    'refactor',
    'map-uid',
    projectPath,
    oldKey.scope,
    oldKey.name,
    newKey.scope,
    newKey.name,
  ];
}

/** Renders a key the way the CLI report renders it: `scope.name`. */
export function formatKey(key: SidecarKey): string {
  return `${key.scope}.${key.name}`;
}

/** The rename and swap candidates of a report, in the order the CLI prints them. */
export function candidatesOf(report: SyncReport): SyncCandidate[] {
  const candidates: SyncCandidate[] = report.renameCandidates.map(candidate => ({
    kind: 'rename' as const,
    old: candidate.old,
    new: candidate.new,
  }));
  for (const candidate of report.swapCandidates) {
    candidates.push({ kind: 'swap', removed: candidate.removed, added: candidate.added });
  }
  return candidates;
}

/** The human-readable mapping a candidate applies, matching the CLI report's arrow form. */
export function describeMapping(candidate: SyncCandidate): string {
  if (candidate.kind === 'rename') {
    return `${formatKey(candidate.old)} -> ${formatKey(candidate.new)}`;
  }
  return `(${formatKey(candidate.removed[0])}, ${formatKey(candidate.removed[1])})`
    + ` -> (${formatKey(candidate.added[0])}, ${formatKey(candidate.added[1])})`;
}

/** One-line summary of a report for notifications, e.g. "3 preserved, 1 assigned, 1 removed". */
export function formatSyncSummary(report: SyncReport): string {
  return `${report.preserved.length} preserved, ${report.assigned.length} assigned,`
    + ` ${report.removed.length} removed`;
}

const HEADER_PATTERN = /^(preserved|assigned|removed|rename candidates|swap candidates): (\d+)$/;
const KEY_UID_PATTERN = /^  (\S+) \(uid (\d+)\)$/;
const RENAME_PATTERN = /^  (\S+) -> (\S+)$/;
const SWAP_PATTERN = /^  \((\S+), (\S+)\) -> \((\S+), (\S+)\)$/;

/** Maps a report section header to its internal section name. */
const SECTIONS: { header: string; section: Section }[] = [
  { header: 'preserved', section: 'preserved' },
  { header: 'assigned', section: 'assigned' },
  { header: 'removed', section: 'removed' },
  { header: 'rename candidates', section: 'rename' },
  { header: 'swap candidates', section: 'swap' },
];

/**
 * Parses the `SyncReport` text the CLI prints verbatim (five sections, each
 * with a count header and one entry line per entry). Throws [`SyncUidsError`]
 * on any deviation from the documented contract so a format change surfaces
 * as a coded failure instead of a silently wrong report.
 */
export function parseSyncReport(text: string): SyncReport {
  const report: SyncReport = {
    preserved: [],
    assigned: [],
    removed: [],
    renameCandidates: [],
    swapCandidates: [],
  };
  const expected: Partial<Record<Section, number>> = {};
  let section: Section | undefined;

  for (const rawLine of text.split(/\r?\n/)) {
    const line = rawLine.trimEnd();
    if (line.length === 0) {
      continue;
    }
    const header = HEADER_PATTERN.exec(line);
    if (header) {
      const match = SECTIONS.find(candidate => candidate.header === header[1]);
      if (!match) {
        throw new SyncUidsError(`sync report has an unknown section header: ${line}`);
      }
      section = match.section;
      expected[section] = Number(header[2]);
      continue;
    }
    if (!section) {
      throw new SyncUidsError(`sync report starts without a section header: ${line}`);
    }
    parseEntry(line, section, report);
  }

  assertSectionCount('preserved', report.preserved.length, expected.preserved);
  assertSectionCount('assigned', report.assigned.length, expected.assigned);
  assertSectionCount('removed', report.removed.length, expected.removed);
  assertSectionCount('rename candidates', report.renameCandidates.length, expected.rename);
  assertSectionCount('swap candidates', report.swapCandidates.length, expected.swap);
  return report;
}

function parseEntry(line: string, section: Section, report: SyncReport): void {
  const keyUid = KEY_UID_PATTERN.exec(line);
  if (keyUid && (section === 'preserved' || section === 'assigned' || section === 'removed')) {
    report[section].push({ key: parseKey(keyUid[1]), uid: Number(keyUid[2]) });
    return;
  }
  const rename = RENAME_PATTERN.exec(line);
  if (rename && section === 'rename') {
    report.renameCandidates.push({ old: parseKey(rename[1]), new: parseKey(rename[2]) });
    return;
  }
  const swap = SWAP_PATTERN.exec(line);
  if (swap && section === 'swap') {
    report.swapCandidates.push({
      removed: [parseKey(swap[1]), parseKey(swap[2])],
      added: [parseKey(swap[3]), parseKey(swap[4])],
    });
    return;
  }
  throw new SyncUidsError(`invalid sync report line in the ${section} section: ${line}`);
}

/** Splits a `scope.name` key. IEC identifiers contain no dots, so the split is unambiguous. */
function parseKey(text: string): SidecarKey {
  const dot = text.lastIndexOf('.');
  if (dot <= 0 || dot === text.length - 1) {
    throw new SyncUidsError(`invalid variable key in the sync report: ${text}`);
  }
  return { scope: text.slice(0, dot), name: text.slice(dot + 1) };
}

function assertSectionCount(section: string, actual: number, expectedCount: number | undefined): void {
  if (expectedCount === undefined) {
    throw new SyncUidsError(`sync report is missing the ${section} section header`);
  }
  if (actual !== expectedCount) {
    throw new SyncUidsError(
      `sync report ${section} section lists ${actual} entries but the header promises ${expectedCount}`,
    );
  }
}

function errorMessage(err: unknown): string {
  return err instanceof Error ? err.message : String(err);
}

async function runSyncUids(compilerPath: string, projectPath: string, execFile: ExecFileFn): Promise<SyncReport> {
  let stdout: string;
  try {
    stdout = (await execFile(compilerPath, syncUidsArgs(projectPath))).stdout;
  }
  catch (err) {
    throw new SyncUidsError(errorMessage(err));
  }
  try {
    return parseSyncReport(stdout);
  }
  catch (err) {
    throw new SyncUidsError(errorMessage(err));
  }
}

async function runMapUid(
  compilerPath: string,
  projectPath: string,
  oldKey: SidecarKey,
  newKey: SidecarKey,
  execFile: ExecFileFn,
): Promise<void> {
  try {
    await execFile(compilerPath, mapUidArgs(projectPath, oldKey, newKey));
  }
  catch (err) {
    throw new MapUidError(errorMessage(err));
  }
}

/**
 * Runs `refactor sync-uids` on the project and, while the report lists
 * unresolved rename/swap candidates, drives the user through resolving them:
 * pick a candidate, confirm its mapping, record it with `refactor map-uid`,
 * then re-sync (which persists the result once no candidates remain).
 * Returns the final report to display.
 */
export async function syncVariableIds(
  compilerPath: string,
  projectPath: string,
  execFile: ExecFileFn,
  ui: SyncResolutionUi,
): Promise<SyncReport> {
  let report = await runSyncUids(compilerPath, projectPath, execFile);
  let candidates = candidatesOf(report);
  while (candidates.length > 0) {
    const picked = await ui.pickCandidate(candidates);
    if (!picked || !(await ui.confirmMapping(describeMapping(picked)))) {
      return report;
    }
    if (picked.kind === 'rename') {
      await runMapUid(compilerPath, projectPath, picked.old, picked.new, execFile);
    }
    else {
      await runMapUid(compilerPath, projectPath, picked.removed[0], picked.added[0], execFile);
      await runMapUid(compilerPath, projectPath, picked.removed[1], picked.added[1], execFile);
    }
    report = await runSyncUids(compilerPath, projectPath, execFile);
    candidates = candidatesOf(report);
  }
  return report;
}
