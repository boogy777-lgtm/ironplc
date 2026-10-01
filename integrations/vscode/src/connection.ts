/**
 * The engineering-connection commands (`ironplc.connect`, `ironplc.disconnect`)
 * and their composition: connection profiles from the `ironplc.connections`
 * setting, validated before any transport opens (E0010–E0012), credentials
 * resolved only through the VS Code secret store, and the spec's connection
 * state machine (`connectionState`) driving one `HotEditSession` over a
 * spawned stdio child or a TCP socket — one active connection per workspace
 * (ADR-0065). This module is the glue: it owns the vscode objects (status
 * bar, QuickPick, output channel, `workspaceState`, `SecretStorage`) and
 * renders what the vscode-free logic decides; the device panel and the
 * build commands subscribe to the model it publishes.
 */

import * as vscode from 'vscode';
import * as path from 'path';
import { spawn } from 'child_process';
import { existsSync } from 'fs';

import { compileSourceToContainer } from './taskProviderLogic';
import { programKind } from './debugAdapterLogic';
import { ReportProblem } from './debugAdapter';
import { ProblemCode } from './problems';
import {
  formatStatusDetail,
  HotEditStatus,
  IdentityInfo,
} from './hotEditSession';
import {
  ConnectionProfile,
  validateProfiles,
} from './connectionProfiles';
import {
  ConnectionManager,
  OpenedConnection,
} from './connectionState';
import {
  BaselineSnapshot,
  BaselineState,
  captureBaseline,
  checkBaseline,
} from './baselineLogic';
import { BuildPhase } from './buildLogic';
import { DevicePanelModel } from './devicePanelLogic';
import { StdioLineTransport, vmFileName } from './hotEdit';
import { connectTcpLineTransport } from './tcpTransport';

/** The `workspaceState` key of the last verified-equal baseline. */
const BASELINE_STATE_KEY = 'ironplc.baseline';

/** What the connection feature exposes to the device panel and the build commands. */
export interface ConnectionFeature {
  readonly manager: ConnectionManager;
  /** The current baseline check result; builds refuse while 'mismatch'. */
  readonly baselineState: BaselineState;
  /** The full panel model for the current moment. */
  model(): DevicePanelModel;
  setBuildPhase(phase: BuildPhase | null): void;
  updateStatus(status: HotEditStatus): void;
  /** Captures the verified-equal baseline after our own assemble acknowledged. */
  captureVerifiedBaseline(bytes: Uint8Array, status: HotEditStatus): void;
  readonly onDidChangeModel: vscode.Event<DevicePanelModel>;
}

export function registerConnectionSupport(
  context: vscode.ExtensionContext,
  compilerPath: string | undefined,
  sourceExtensions: readonly string[],
  reportProblem: ReportProblem,
): ConnectionFeature {
  const outputChannel = vscode.window.createOutputChannel('IronPLC Connection');
  context.subscriptions.push(outputChannel);

  const statusItem = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Left, 99);
  context.subscriptions.push(statusItem);

  const modelEmitter = new vscode.EventEmitter<DevicePanelModel>();

  let baselineState: BaselineState = 'none';
  let buildPhase: BuildPhase | null = null;
  let lastStatusOverride: HotEditStatus | null = null;

  const manager = new ConnectionManager({
    open,
    events: {
      onState: () => render(),
      onIdentity: (identity: IdentityInfo) => {
        lastStatusOverride = null;
        reconcileBaseline(identity);
        render();
      },
      onStatus: (status) => {
        lastStatusOverride = status;
        render();
      },
      report: (code, detail) => reportProblem(code, detail),
    },
    clock: {
      sleep: (ms: number) => new Promise(resolve => setTimeout(resolve, ms)),
      random: () => Math.random(),
      startHeartbeat: (tick: () => void, intervalMs: number) => {
        const timer = setInterval(tick, intervalMs);
        return () => clearInterval(timer);
      },
    },
  });

  const feature: ConnectionFeature = {
    manager,
    get baselineState() {
      return baselineState;
    },
    model,
    setBuildPhase(phase: BuildPhase | null): void {
      buildPhase = phase;
      render();
    },
    updateStatus(status: HotEditStatus): void {
      lastStatusOverride = status;
      render();
    },
    captureVerifiedBaseline(bytes: Uint8Array, status: HotEditStatus): void {
      const snapshot = captureBaseline(status, bytes);
      void context.workspaceState.update(BASELINE_STATE_KEY, snapshot);
      baselineState = 'match';
      lastStatusOverride = status;
      render();
    },
    onDidChangeModel: modelEmitter.event,
  };

  function loadBaseline(): BaselineSnapshot | undefined {
    const stored = context.workspaceState.get<BaselineSnapshot>(BASELINE_STATE_KEY);
    if (
      typeof stored === 'object' && stored !== null
      && typeof stored.active === 'number' && typeof stored.application === 'number'
      && typeof stored.containerHash === 'string'
    ) {
      return stored;
    }
    return undefined;
  }

  function model(): DevicePanelModel {
    const snapshot = manager.snapshot();
    return {
      state: manager.currentState,
      profileName: snapshot.profileName,
      identity: snapshot.identity,
      lastStatus: lastStatusOverride ?? snapshot.lastStatus,
      baseline: baselineState,
      buildPhase,
    };
  }

  function render(): void {
    const state = manager.currentState;
    const current = model();
    statusItem.text = statusText(current);
    statusItem.tooltip = statusTooltip(current);
    statusItem.command = state === 'disconnected' ? 'ironplc.connect' : 'ironplc.devicePanel.focus';
    statusItem.show();
    modelEmitter.fire(current);
  }

  function reconcileBaseline(identity: IdentityInfo): void {
    const next = checkBaseline(loadBaseline(), identity.application);
    const becameStale = next === 'mismatch' && baselineState !== 'mismatch';
    baselineState = next;
    if (becameStale) {
      reportProblem(
        ProblemCode.StaleBaseline,
        'The device was edited elsewhere or rebooted. Re-sync the project from the device, rebuild, then connect again.',
      );
    }
  }

  async function connect(): Promise<void> {
    const config = vscode.workspace.getConfiguration('ironplc');
    const { profiles, errors } = validateProfiles(config.get<unknown>('connections', []));
    for (const failure of errors) {
      reportProblem(failure.code, `Profile "${failure.profile}": ${failure.detail}`);
    }
    if (errors.length > 0) {
      return;
    }
    if (profiles.length === 0) {
      void vscode.window.showInformationMessage(
        'No connection profiles. Add an "ironplc.connections" entry in the settings to connect to a device.',
      );
      return;
    }

    // The last-used profile is the workspace default: reconnect it directly;
    // otherwise ask. (QuickPick cannot preselect an item, so a matching
    // active profile short-circuits the picker entirely.)
    const activeName = config.get<string>('activeConnection', '');
    const active = profiles.find(profile => profile.name === activeName);
    const profile = active ?? await pickProfile(profiles);
    if (!profile) {
      return;
    }

    if (profile.transport === 'stdio' && !ensureStdioTools()) {
      return;
    }
    if (!(await resolveCredentials(profile))) {
      return;
    }

    const result = await manager.connect(profile);
    if (result === 'already-connected') {
      void vscode.window.showInformationMessage(
        `IronPLC is already connected${profileSuffix()}. Run "IronPLC: Disconnect from Device" first.`,
      );
    }
    else if (result === 'connected') {
      await config.update('activeConnection', profile.name, vscode.ConfigurationTarget.Workspace);
    }
  }

  async function pickProfile(profiles: ConnectionProfile[]): Promise<ConnectionProfile | undefined> {
    const picked = await vscode.window.showQuickPick(
      profiles.map(profile => ({
        label: profile.name,
        description: profile.transport,
        detail: profile.transport === 'stdio' ? profile.program : `${profile.address}:${profile.port}`,
        profile,
      })),
      { title: 'IronPLC: connect to a device' },
    );
    return picked?.profile;
  }

  function profileSuffix(): string {
    const name = manager.currentProfile?.name;
    return name ? ` to "${name}"` : '';
  }

  /** The stdio profile's tools, reported with the same codes the hot-edit path uses. */
  function ensureStdioTools(): boolean {
    if (!compilerPath) {
      reportProblem(ProblemCode.NoCompiler, 'Install the compiler to start a stdio connection.');
      return false;
    }
    const vmPath = path.join(path.dirname(compilerPath), vmFileName(process.platform));
    if (!existsSync(vmPath)) {
      reportProblem(
        ProblemCode.VmNotFound,
        `Install the IronPLC VM so ${vmFileName(process.platform)} sits next to the compiler.`,
      );
      return false;
    }
    return true;
  }

  /**
   * The profile stores only a secret-store key; the credential itself lives
   * exclusively in `SecretStorage`. v1 sends nothing over the wire — the
   * transport-authentication decision is deferred (ADR-0063) — so the
   * resolved secret simply validates the store and waits for that decision.
   */
  async function resolveCredentials(profile: ConnectionProfile): Promise<boolean> {
    if (profile.credentialsKey === undefined) {
      return true;
    }
    if ((await context.secrets.get(profile.credentialsKey)) !== undefined) {
      return true;
    }
    const entered = await vscode.window.showInputBox({
      title: `IronPLC: credentials for "${profile.name}"`,
      prompt: `Store the credential under the secret-store key "${profile.credentialsKey}"`,
      password: true,
      ignoreFocusOut: true,
    });
    if (entered === undefined) {
      return false;
    }
    await context.secrets.store(profile.credentialsKey, entered);
    return true;
  }

  async function open(profile: ConnectionProfile): Promise<OpenedConnection> {
    if (profile.transport === 'tcp') {
      const transport = await connectTcpLineTransport(profile.address!, profile.port!);
      return { transport, close: () => transport.dispose() };
    }

    // The serve session loads a compiled container, so a source program is
    // compiled first — the same single-file compile every other path uses.
    let container = profile.program!;
    if (programKind(container, sourceExtensions) === 'source') {
      container = await compileSourceToContainer(compilerPath!, container, text => outputChannel.append(text));
    }
    const vmPath = path.join(path.dirname(compilerPath!), vmFileName(process.platform));
    outputChannel.appendLine(`$ ${vmPath} serve ${container}`);
    const child = spawn(vmPath, ['serve', container]);
    child.stderr.on('data', (chunk: Buffer) => outputChannel.append(chunk.toString()));
    return {
      transport: new StdioLineTransport(child),
      close: () => {
        child.kill();
      },
    };
  }

  context.subscriptions.push(
    vscode.commands.registerCommand('ironplc.connect', connect),
    vscode.commands.registerCommand('ironplc.disconnect', () => manager.disconnect()),
  );

  render();

  return feature;
}

function statusText(model: DevicePanelModel): string {
  switch (model.state) {
    case 'disconnected':
      return '$(debug-disconnect) IronPLC: Not connected';
    case 'connecting':
      return '$(sync~spin) IronPLC: Connecting…';
    case 'reconnecting':
      return '$(sync~spin) IronPLC: Reconnecting…';
    case 'connected':
      return `$(plug) IronPLC: ${model.identity?.device.name ?? model.profileName ?? 'Connected'}`;
  }
}

function statusTooltip(model: DevicePanelModel): string {
  switch (model.state) {
    case 'disconnected':
      return 'Connect to a device (IronPLC: Connect to Device)';
    case 'connecting':
      return 'Opening the transport and running the identity handshake…';
    case 'reconnecting':
      return 'The connection was lost; retrying with backoff. The device panel keeps the last data, marked stale.';
    case 'connected': {
      const lines = [`Profile: ${model.profileName ?? 'unknown'}`];
      if (model.lastStatus) {
        lines.push(formatStatusDetail(model.lastStatus));
      }
      lines.push('Click for the IronPLC Device panel.');
      return lines.join('\n');
    }
  }
}
