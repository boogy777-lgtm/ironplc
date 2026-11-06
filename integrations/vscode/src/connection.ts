/**
 * The engineering-connection commands (`ironplc.connect`, `ironplc.disconnect`)
 * and their composition: connection profiles from the `ironplc.connections`
 * setting, validated before any transport opens (E0010–E0012), credentials
 * resolved only through the VS Code secret store, and the spec's connection
 * state machine (`connectionState`) driving one `HotEditSession` over a
 * spawned stdio child or a TCP socket — one active connection per workspace
 * (ADR-0065). This module is the glue: it owns the vscode objects (status
 * bar, QuickPick, output channel, `SecretStorage`) and
 * renders what the vscode-free logic decides; the device panel and the
 * build commands subscribe to the model it publishes. When a session opens (or
 * a connection fails) it asks the compiler for the go-online comparison
 * (`ironplcc online-compare`, ADR-0089) and publishes it: the status line
 * follows it, and the dialog opens unless the application is up to date.
 */

import * as vscode from 'vscode';
import { existsSync } from 'fs';

import { currentCompilerSettings } from './compilerSettings';
import { runCompiler } from './taskProviderLogic';
import { ReportProblem } from './debugAdapter';
import {
  ControllerArgument,
  HANDOFF_ACTIONS,
  IronLinkApi,
  IronLinkRoute,
  isControllerArgument,
  ProblemCode,
  shortDeviceId,
} from './vscodeCommon';
import {
  HotEditStatus,
  IdentityInfo,
} from './hotEditSession';
import { connectionMethod, OpenContext } from './connectionMethods';
import {
  ConnectionProfile,
  validateProfiles,
} from './connectionProfiles';
import { applicationAnchor, ChosenController, readChosen } from './chooseControllerLogic';
import {
  connectingText,
  controllerProfile,
  identityNotes,
  resolveChosenRoute,
  ROUTE_SOURCE_TEXT,
} from './connectControllerLogic';
import { accessIronLink } from './ironlinkAccess';
import { statusCommand, statusText, statusTooltip } from './statusBarLogic';
import {
  ConnectionManager,
  IdentityCause,
  OpenedConnection,
} from './connectionState';
import {
  FolderRef,
  planDownload,
  RunCompiler,
  writeDownloadRecord,
} from './downloadRecordLogic';
import {
  attemptOfFailure,
  compareOnConnect,
  ComparisonEnv,
  ComparisonRequest,
} from './onlineComparisonRun';
import { Comparison, ComparisonModel } from './onlineComparisonLogic';
import { compilerFlags } from './compilerSettingsLogic';
import { BuildPhase } from './buildLogic';
import { ControllerRun } from './controllerRunLogic';
import { DevicePanelModel } from './devicePanelLogic';
import { readTextIfPresent } from './readText';
import { vmFileName, vmPathBeside } from './stdioTransport';

/** What the connection feature exposes to the device panel and the build commands. */
export interface ConnectionFeature {
  readonly manager: ConnectionManager;
  /** The "IronPLC Connection" output channel. */
  readonly output: vscode.OutputChannel;
  /**
   * The go-online comparison of the current connection, or of the last attempt
   * that failed; undefined before any, after a disconnect, and when the
   * compiler could not make it.
   */
  comparison(): Comparison | undefined;
  /**
   * Asks the controller for its identity again and compares it with the project,
   * so the answer is the controller as it is now. Resolves the new comparison,
   * or undefined when there is none.
   */
  refreshComparison(): Promise<Comparison | undefined>;
  /** Fires when a comparison is to be shown to the person: a connection that is not up to date, or one that failed. */
  readonly onDidAnnounceComparison: vscode.Event<{ model: ComparisonModel; comparison: Comparison }>;
  /** Connects again as the last attempt did (the Retry action). */
  retry(): Promise<void>;
  /** The full panel model for the current moment. */
  model(): DevicePanelModel;
  setBuildPhase(phase: BuildPhase | null): void;
  updateStatus(status: HotEditStatus): void;
  /**
   * Shows what the device said about scanning in answer to Run or Stop, until the
   * next identity or the end of the connection. The model holds no run state of
   * its own: this is the device's answer.
   */
  updateRun(run: ControllerRun): void;
  /**
   * Handles a download the device acknowledged: records that the device
   * received the container `container` (the file that was sent), then compares
   * the device with the project again. The one place a build records an
   * assemble acknowledgement. `keepContainer` also keeps a copy of the container
   * beside the record (a Download that replaced the application).
   */
  recordDownload(container: string, status: HotEditStatus, keepContainer: boolean): Promise<void>;
  /** Resolves when the comparison of the device with the project has finished. */
  settled(): Promise<void>;
  readonly onDidChangeModel: vscode.Event<DevicePanelModel>;
  /**
   * Connects to the controller chosen for the project, or to the controller of
   * `argument` (`{deviceId, address, sessionPort}`, from the IronLink Network
   * view). The raw argument is checked here: any extension can run the command.
   */
  connectController(argument: unknown): Promise<void>;
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

  let latestComparison: Comparison | undefined;
  const announceEmitter = new vscode.EventEmitter<{ model: ComparisonModel; comparison: Comparison }>();
  context.subscriptions.push(announceEmitter);
  // The connection last opened, saved or not, and how: what Retry runs again and
  // what the comparison of a failed attempt is made for. The manager forgets the
  // profile when a connection ends, so it is kept here.
  let lastOpen: { profile: ConnectionProfile; intended: string | undefined; protectedController: boolean } | undefined;
  // True from the moment a connection is requested until its identity arrives:
  // the comparison of that connection opens the dialog. A reconnect, a download
  // and an identity asked for again renew the comparison and open nothing.
  let announceNext = false;
  let pendingCheck: Promise<void> = Promise.resolve();
  let buildPhase: BuildPhase | null = null;
  let lastStatusOverride: HotEditStatus | null = null;
  let lastRunOverride: ControllerRun | null = null;
  // The controller chosen for the project (its path file), what the status item
  // names while disconnected; the names that connected controllers reported
  // (a path file holds no name); and the controller the current connection was
  // meant for, with the information lines about it.
  let chosenController: ChosenController | undefined;
  const knownNames = new Map<string, string>();
  let intendedDeviceId: string | undefined;
  let notes: string[] = [];

  const manager = new ConnectionManager({
    open,
    events: {
      onState: (state) => {
        if (state === 'disconnected') {
          intendedDeviceId = undefined;
          notes = [];
          lastRunOverride = null;
          latestComparison = undefined;
        }
        render();
      },
      onIdentity: (identity: IdentityInfo, cause) => {
        lastStatusOverride = null;
        lastRunOverride = null;
        const announce = cause === 'connected' && announceNext;
        if (cause === 'connected') {
          announceNext = false;
        }
        pendingCheck = Promise.all([settleIdentity(identity, cause, announce), noteIdentity(identity)]).then(render);
        render();
      },
      onStatus: (status) => {
        lastStatusOverride = status;
        render();
      },
      report: (code, detail) => {
        reportProblem(code, detail);
        // A connection that was open and ended: the same view explains it.
        if (code === ProblemCode.ConnectionLost) {
          void explainFailure();
        }
      },
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
    output: outputChannel,
    comparison: () => latestComparison,
    refreshComparison,
    onDidAnnounceComparison: announceEmitter.event,
    retry,
    model,
    setBuildPhase(phase: BuildPhase | null): void {
      buildPhase = phase;
      render();
    },
    updateStatus(status: HotEditStatus): void {
      lastStatusOverride = status;
      render();
    },
    updateRun(run: ControllerRun): void {
      lastRunOverride = run;
      render();
    },
    async recordDownload(container: string, status: HotEditStatus, keepContainer: boolean): Promise<void> {
      lastStatusOverride = status;
      try {
        await recordDownloadOf(container, status, false, connectedDeviceId(), keepContainer);
      }
      catch (err) {
        void vscode.window.showErrorMessage(
          `IronPLC: the device received the application, but the download record could not be written: ${err instanceof Error ? err.message : String(err)}`,
        );
      }
      // The controller holds another application now: ask it again, and compare.
      await manager.refreshIdentity();
      await pendingCheck;
      render();
    },
    settled: () => pendingCheck,
    onDidChangeModel: modelEmitter.event,
    connectController,
  };

  /** The compiler, with its command line and output in the connection channel. */
  function runTool(): RunCompiler | undefined {
    const compiler = compilerPath;
    return compiler === undefined
      ? undefined
      : args => runCompiler(compiler, args, text => outputChannel.append(text));
  }

  /** The id of the connected device, as its identity reports it; undefined when it reports none. */
  function connectedDeviceId(): string | undefined {
    return manager.snapshot().identity?.device.deviceId;
  }

  function workspaceFolders(): FolderRef[] {
    return (vscode.workspace.workspaceFolders ?? []).map(folder => ({ name: folder.name, path: folder.uri.fsPath }));
  }

  /**
   * Records that the device of the current connection received a container,
   * through the compiler's one writer, keyed by the device's id. `sent` is the
   * file that was sent, or undefined for the application container itself. Returns whether a record
   * was written; a download that has no record says why in the connection
   * channel. A failure of the writer is thrown.
   */
  async function recordDownloadOf(
    sent: string | undefined,
    status: HotEditStatus,
    requireDeviceHash: boolean,
    deviceId: string | undefined,
    keepContainer = false,
  ): Promise<boolean> {
    const profile = manager.currentProfile;
    if (profile === undefined) {
      return false;
    }
    const plan = planDownload({
      profile,
      folders: workspaceFolders(),
      sourceExtensions,
      sentContainer: sent,
      deviceId,
      status,
      keepContainer,
      requireDeviceHash,
    });
    if ('reason' in plan) {
      outputChannel.appendLine(`No download record: ${plan.reason}.`);
      return false;
    }
    const run = runTool();
    if (run === undefined) {
      outputChannel.appendLine('No download record: the IronPLC compiler, which writes it, is not installed.');
      return false;
    }
    await writeDownloadRecord(run, plan.download);
    return true;
  }

  function model(): DevicePanelModel {
    const snapshot = manager.snapshot();
    return {
      state: manager.currentState,
      profileName: snapshot.profileName,
      identity: snapshot.identity,
      lastStatus: lastStatusOverride ?? snapshot.lastStatus,
      comparison: latestComparison ?? null,
      buildPhase,
      run: lastRunOverride,
      notes,
    };
  }

  /** How the chosen controller is named: the name it reported, else its short device id. */
  function chosenLabel(): string | undefined {
    return chosenController === undefined
      ? undefined
      : knownNames.get(chosenController.deviceId) ?? shortDeviceId(chosenController.deviceId);
  }

  function render(): void {
    const state = manager.currentState;
    const current = model();
    const chosen = chosenLabel();
    statusItem.text = statusText(current, chosen);
    statusItem.tooltip = statusTooltip(current, chosen);
    statusItem.command = statusCommand(state, chosen);
    statusItem.show();
    modelEmitter.fire(current);
  }

  /**
   * Reads the controller chosen for the project from its path file. A project
   * without an application, a missing compiler or a file that is not a record
   * has none; a read that fails is in the connection channel.
   */
  async function refreshChosen(): Promise<void> {
    const anchor = applicationAnchor(workspaceFolders(), sourceExtensions);
    // Quiet: this runs whenever the project or its path file changes, and is no
    // part of what the engineer asked for, so it leaves no line in the channel.
    const compiler = compilerPath;
    const run: RunCompiler | undefined = compiler === undefined ? undefined : args => runCompiler(compiler, args, () => {});
    let next: ChosenController | undefined;
    if (!('reason' in anchor) && run !== undefined) {
      try {
        next = await readChosen(run, readTextIfPresent, anchor.anchor);
      }
      catch (err) {
        outputChannel.appendLine(`The path record could not be read, so no controller is shown as chosen: ${err instanceof Error ? err.message : String(err)}`);
      }
    }
    chosenController = next;
    render();
  }

  /**
   * After the identity arrives: remember the name it reports, and say in the
   * connection channel and the device panel when the controller is not the one
   * that was meant or chosen. This is information and never a refusal.
   */
  async function noteIdentity(identity: IdentityInfo): Promise<void> {
    const deviceId = identity.device.deviceId;
    if (deviceId !== undefined) {
      knownNames.set(deviceId, identity.device.name);
    }
    await refreshChosen();
    notes = identityNotes({ connected: deviceId, intended: intendedDeviceId, chosen: chosenController?.deviceId });
    for (const line of notes) {
      outputChannel.appendLine(line);
    }
  }

  /**
   * After an identity arrives. A session started with a container is a download:
   * it is recorded, when the device shows that it runs that container, before the
   * comparison reads the records. Then the controller is compared with the
   * project (`ironplcc online-compare`), and the dialog opens when `announce`.
   */
  async function settleIdentity(identity: IdentityInfo, cause: IdentityCause, announce: boolean): Promise<void> {
    const started = manager.currentProfile;
    if (cause === 'connected' && started !== undefined && connectionMethod(started).recordsSessionStart) {
      try {
        await recordDownloadOf(undefined, identity.application, true, identity.device.deviceId);
      }
      catch (err) {
        outputChannel.appendLine(`The session start was not recorded: ${err instanceof Error ? err.message : String(err)}`);
      }
    }
    const profile = started ?? lastOpen?.profile;
    if (profile === undefined) {
      return;
    }
    latestComparison = await compareOnConnect(comparisonEnv(), { profile, attempt: 'ok', identity }, comparisonUi, announce);
    render();
  }

  /** The comparison of an attempt that did not reach the controller, shown with its dialog. */
  async function explainFailure(): Promise<void> {
    // A connection the person ended (no failure was recorded) is not explained.
    if (lastOpen === undefined || manager.lastFailure === undefined) {
      return;
    }
    const attempt = attemptOfFailure(manager.lastFailure, lastOpen.protectedController);
    const request: ComparisonRequest = { profile: lastOpen.profile, attempt };
    latestComparison = await compareOnConnect(comparisonEnv(), request, comparisonUi, true);
    render();
  }

  /** The controller's identity again, and the comparison made from it. */
  async function refreshComparison(): Promise<Comparison | undefined> {
    if (await manager.refreshIdentity() === undefined) {
      return undefined;
    }
    await pendingCheck;
    return latestComparison;
  }

  /** Connects again as the last attempt did. */
  async function retry(): Promise<void> {
    if (lastOpen === undefined) {
      await connect();
      return;
    }
    await openProfile(lastOpen.profile, lastOpen.intended, lastOpen.protectedController);
  }

  const comparisonUi = {
    current: (comparison: Comparison) => {
      latestComparison = comparison;
      render();
    },
    dialog: (model: ComparisonModel, comparison: Comparison) => announceEmitter.fire({ model, comparison }),
  };

  /** What the comparison needs of the editor now: the compiler, the folders, the settings. */
  function comparisonEnv(): ComparisonEnv {
    const compiler = compilerPath;
    return {
      run: compiler === undefined ? undefined : (args, input) => runCompiler(compiler, args, () => {}, undefined, input),
      folders: workspaceFolders(),
      sourceExtensions,
      options: () => compilerFlags(currentCompilerSettings()),
      storageDir: context.globalStorageUri.fsPath,
      say: line => outputChannel.appendLine(line),
    };
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

    if (await openProfile(profile, undefined) === 'connected') {
      await config.update('activeConnection', profile.name, vscode.ConfigurationTarget.Workspace);
    }
  }

  /**
   * Opens a profile, saved or not: the one path every connection takes. The
   * result is `failed` when nothing was opened. `intended` is the device id of
   * the route being connected to (undefined for a saved profile), which the
   * information lines compare the answering controller with.
   */
  async function openProfile(
    profile: ConnectionProfile,
    intended: string | undefined,
    protectedController = false,
  ): Promise<'connected' | 'failed' | 'already-connected'> {
    if (connectionMethod(profile).needsLocalTools && !ensureLocalTools(profile)) {
      return 'failed';
    }
    if (!(await resolveCredentials(profile))) {
      return 'failed';
    }

    // A connection that is open stays as it is, with the controller it was meant for.
    if (manager.currentState === 'disconnected') {
      intendedDeviceId = intended;
      lastOpen = { profile, intended, protectedController };
      announceNext = true;
    }
    const result = await manager.connect(profile);
    if (result === 'already-connected') {
      void vscode.window.showInformationMessage(
        `IronPLC is already connected${profileSuffix()}. Run "IronPLC: Disconnect from Device" first.`,
      );
    }
    else if (result === 'failed') {
      // The same view explains a connection that did not open.
      announceNext = false;
      await explainFailure();
    }
    return result;
  }

  /** The name IronLink last heard for a device, when IronLink has been used. */
  function observedName(api: IronLinkApi | undefined, deviceId: string): string | undefined {
    return api?.lastBrowse()?.devices.find(device => device.deviceId === deviceId)?.name;
  }

  /**
   * Connects to a controller by an unsaved `tcp` profile made from its route,
   * validated like a saved one. `source` says where the route came from, for the
   * connection channel.
   */
  async function connectTo(controller: ControllerArgument, source: string, api: IronLinkApi | undefined): Promise<void> {
    const profile = controllerProfile(
      controller,
      observedName(api, controller.deviceId) ?? knownNames.get(controller.deviceId),
    );
    const { profiles, errors } = validateProfiles([profile]);
    for (const failure of errors) {
      reportProblem(failure.code, `Profile "${failure.profile}": ${failure.detail}`);
    }
    if (errors.length > 0) {
      return;
    }
    outputChannel.appendLine(connectingText(profiles[0], source));
    await openProfile(profiles[0], controller.deviceId, api?.lastBrowse()?.devices.find(device => device.deviceId === controller.deviceId)?.protected === true);
  }

  async function connectController(argument: unknown): Promise<void> {
    const access = await accessIronLink(id => vscode.extensions.getExtension(id));
    const api = access.available ? access.api : undefined;
    if (argument !== undefined) {
      if (!isControllerArgument(argument)) {
        reportProblem(
          ProblemCode.ConnectionProfileInvalid,
          'The command was run with an argument that is not a controller: expected {deviceId, address, sessionPort}',
        );
        return;
      }
      await connectTo(argument, 'the controller given', api);
      return;
    }

    await refreshChosen();
    let routes: IronLinkRoute[] | undefined;
    if (api !== undefined) {
      try {
        routes = await api.routeList();
      }
      catch (err) {
        outputChannel.appendLine(`IronLink did not list the routes of this PC: ${err instanceof Error ? err.message : String(err)}`);
      }
    }
    const resolved = resolveChosenRoute(chosenController, routes);
    if ('reason' in resolved) {
      const choose = 'Choose Controller…';
      const picked = await vscode.window.showInformationMessage(resolved.reason, choose);
      if (picked === choose) {
        await vscode.commands.executeCommand(HANDOFF_ACTIONS.choose.command);
      }
      return;
    }
    await connectTo(resolved.route, ROUTE_SOURCE_TEXT[resolved.source], api);
  }

  async function pickProfile(profiles: ConnectionProfile[]): Promise<ConnectionProfile | undefined> {
    const picked = await vscode.window.showQuickPick(
      profiles.map(profile => ({
        label: profile.name,
        description: profile.transport,
        detail: connectionMethod(profile).detail(profile),
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

  /** The tools a method that needs them runs, reported with the same codes the hot-edit path uses. */
  function ensureLocalTools(profile: ConnectionProfile): boolean {
    if (!compilerPath) {
      reportProblem(ProblemCode.NoCompiler, `Install the compiler to start a ${profile.transport} connection.`);
      return false;
    }
    const vmPath = vmPathBeside(compilerPath, process.platform);
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

  const openContext: OpenContext = {
    compilerPath,
    sourceExtensions,
    output: outputChannel,
    compilerSettings: currentCompilerSettings,
  };

  function open(profile: ConnectionProfile): Promise<OpenedConnection> {
    return connectionMethod(profile).open(profile, openContext);
  }

  context.subscriptions.push(
    vscode.commands.registerCommand('ironplc.connect', connect),
    vscode.commands.registerCommand('ironplc.disconnect', () => manager.disconnect()),
  );

  // The status item names the controller chosen for the project, so it follows
  // the project's path file and the workspace folders.
  const pathFiles = vscode.workspace.createFileSystemWatcher('**/*.path.iplcinfo');
  context.subscriptions.push(
    pathFiles,
    pathFiles.onDidCreate(() => void refreshChosen()),
    pathFiles.onDidChange(() => void refreshChosen()),
    pathFiles.onDidDelete(() => void refreshChosen()),
    vscode.workspace.onDidChangeWorkspaceFolders(() => void refreshChosen()),
  );

  render();
  void refreshChosen();

  return feature;
}
