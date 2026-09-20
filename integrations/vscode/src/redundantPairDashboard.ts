/**
 * The Redundant Pair Dashboard: a singleton `WebviewPanel` in the editor
 * area that renders the HA engineering contract sections over the pure
 * model in `redundantPairLogic`. The panel owns only the message loop and
 * the HTML shell; every state decision lives in the unit-tested module.
 *
 * Until the HA runtime exposes a pair surface, the panel renders the honest
 * empty state and an explicitly marked DEMO MODE with the contract's
 * scenario controls. See the demo-seam note in `redundantPairLogic.ts`.
 */

import * as vscode from 'vscode';
import {
  DashboardAction,
  DASHBOARD_ACTIONS,
  DASHBOARD_VIEW_TYPE,
  OPEN_DASHBOARD_COMMAND,
  PairDashboardState,
  applyAction,
  buildDashboardViewModel,
  demoPairState,
  emptyPairState,
  enabledActions,
} from './redundantPairLogic';

const DEMO_ACTION_LABELS: Record<DashboardAction, string> = {
  'swap': 'Manual swap',
  'primary-death': 'Primary death',
  'secondary-death': 'Secondary death',
  'resurrect-primary': 'Resurrect primary',
  'resurrect-secondary': 'Resurrect secondary',
  'kill-p': 'Kill P',
  'kill-i': 'Kill I',
  'restore-channels': 'Restore',
  'partition': 'Partition claim',
  'calibrate': 'Calibrate',
  'invalidate': 'Invalidate',
  'degrade': 'Degrade EMA',
  'reset': 'Reset',
};

let current: vscode.WebviewPanel | undefined;
let demo = false;
let state: PairDashboardState = emptyPairState();

/** Registers the dashboard command. Called unconditionally with the other UI surfaces. */
export function registerRedundantPairDashboard(context: vscode.ExtensionContext): void {
  context.subscriptions.push(
    vscode.commands.registerCommand(OPEN_DASHBOARD_COMMAND, () => showDashboard()),
  );
}

function showDashboard(): void {
  if (current) {
    current.reveal();
    return;
  }
  const panel = vscode.window.createWebviewPanel(
    DASHBOARD_VIEW_TYPE,
    'Redundant Pair Dashboard',
    vscode.ViewColumn.Active,
    { enableScripts: true },
  );
  current = panel;
  panel.webview.html = dashboardHtml(panel.webview, createNonce());
  panel.onDidDispose(() => {
    current = undefined;
  });
  panel.webview.onDidReceiveMessage((message: { type?: string; action?: DashboardAction }) => {
    if (message.type === 'toggle-demo') {
      demo = !demo;
      state = demo ? demoPairState() : emptyPairState();
    }
    else if (message.type === 'action' && message.action) {
      state = applyAction(state, message.action);
    }
    postState(panel);
  });
  postState(panel);
}

function postState(panel: vscode.WebviewPanel): void {
  panel.title = demo ? 'Redundant Pair Dashboard (DEMO)' : 'Redundant Pair Dashboard';
  void panel.webview.postMessage({
    type: 'viewModel',
    viewModel: buildDashboardViewModel(state),
    enabled: enabledActions(state),
    labels: DEMO_ACTION_LABELS,
    actions: DASHBOARD_ACTIONS,
    demo,
  });
}

function createNonce(): string {
  const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
  let nonce = '';
  for (let i = 0; i < 32; i++) {
    nonce += chars.charAt(Math.floor(Math.random() * chars.length));
  }
  return nonce;
}

/** The panel HTML: theme-variable styling, no external resources, nonce-only script. */
function dashboardHtml(webview: vscode.Webview, nonce: string): string {
  const csp = [
    'default-src \'none\'',
    `style-src ${webview.cspSource} 'unsafe-inline'`,
    `script-src 'nonce-${nonce}'`,
  ].join('; ');
  return `<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="utf-8">
<meta http-equiv="Content-Security-Policy" content="${csp}">
<meta name="viewport" content="width=device-width, initial-scale=1">
<style>
  body { font-family: var(--vscode-font-family); font-size: var(--vscode-font-size); color: var(--vscode-foreground); padding: 12px 16px; }
  h1 { font-size: 1.25em; margin: 0 0 4px 0; }
  h2 { font-size: 1.05em; margin: 18px 0 6px 0; border-bottom: 1px solid var(--vscode-panel-border); padding-bottom: 3px; }
  .sub { color: var(--vscode-descriptionForeground); margin-bottom: 8px; }
  .banner { border: 1px solid var(--vscode-inputValidation-warningBorder); background: var(--vscode-inputValidation-warningBackground); padding: 8px 10px; border-radius: 4px; margin: 8px 0; display: flex; gap: 10px; align-items: center; flex-wrap: wrap; }
  .empty { border: 1px dashed var(--vscode-panel-border); padding: 24px; text-align: center; color: var(--vscode-descriptionForeground); border-radius: 6px; }
  .toolbar { display: flex; flex-wrap: wrap; gap: 6px; margin: 8px 0; }
  button { font-family: inherit; color: var(--vscode-button-foreground); background: var(--vscode-button-background); border: none; border-radius: 3px; padding: 4px 10px; cursor: pointer; }
  button:hover:not(:disabled) { background: var(--vscode-button-hoverBackground); }
  button.secondary { color: var(--vscode-button-secondaryForeground); background: var(--vscode-button-secondaryBackground); }
  button:disabled { opacity: 0.45; cursor: not-allowed; }
  .grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(280px, 1fr)); gap: 10px; }
  .card { border: 1px solid var(--vscode-panel-border); border-radius: 4px; padding: 10px; }
  table { border-collapse: collapse; width: 100%; }
  th { text-align: left; color: var(--vscode-descriptionForeground); font-weight: 600; font-size: 0.85em; text-transform: uppercase; padding: 4px 6px; border-bottom: 1px solid var(--vscode-panel-border); }
  td { padding: 3px 6px; border-bottom: 1px solid var(--vscode-panel-border); font-family: var(--vscode-editor-font-family, monospace); }
  td.num, th.num { text-align: right; }
  tr.limiting td { background: var(--vscode-inputValidation-warningBackground); }
  .pill { display: inline-block; border: 1px solid var(--vscode-panel-border); border-radius: 10px; padding: 1px 8px; margin: 2px 4px 2px 0; font-size: 0.9em; }
  .ok { color: var(--vscode-testing-iconPassed, var(--vscode-charts-green)); }
  .warn { color: var(--vscode-charts-yellow); }
  .alarm { color: var(--vscode-errorForeground); }
  .muted { color: var(--vscode-descriptionForeground); }
  .kv { display: grid; grid-template-columns: 170px 1fr; gap: 2px 8px; }
  .kv .k { color: var(--vscode-descriptionForeground); }
  .progress { height: 8px; background: var(--vscode-panel-border); border-radius: 4px; overflow: hidden; margin: 6px 0; }
  .progress > div { height: 100%; background: var(--vscode-progressBar-background); }
  .verdict { padding: 6px 10px; border-radius: 4px; display: inline-block; border: 1px solid; }
  .verdict.ok { border-color: var(--vscode-charts-green); }
  .verdict.alarm { border-color: var(--vscode-errorForeground); }
  .events { max-height: 320px; overflow: auto; border: 1px solid var(--vscode-panel-border); border-radius: 4px; }
  .event { display: grid; grid-template-columns: 90px 60px 1fr; gap: 8px; padding: 3px 8px; border-bottom: 1px solid var(--vscode-panel-border); font-size: 0.92em; }
  .event .at { color: var(--vscode-descriptionForeground); }
  .event .sev { font-weight: 700; text-transform: uppercase; font-size: 0.85em; }
</style>
</head>
<body>
  <h1>Redundant Pair Dashboard</h1>
  <div class="sub">HA engineering contract: pair state, calibration, ownership barrier, timing budget, events.</div>
  <div id="banner" class="banner" hidden>
    <strong>DEMO MODE</strong>
    <span class="muted">Simulated pair — not connected to a controller. Controls implement the design-doc semantics.</span>
    <button id="toggleDemo">Leave demo mode</button>
  </div>
  <div id="empty" class="empty" hidden>
    <p>No redundant pair connected.</p>
    <p class="muted">The controller has not exposed an HA runtime surface yet. Enable demo mode to explore the contract.</p>
    <button id="toggleDemoEmpty">Enter demo mode</button>
  </div>
  <div id="content">
    <div class="toolbar" id="toolbar"></div>
    <div id="alarms"></div>
    <h2>Pair Overview</h2>
    <div id="overview"></div>
    <h2>Calibration</h2>
    <div id="calibration"></div>
    <h2>Ownership &amp; Barrier</h2>
    <div id="barrier"></div>
    <h2>Timing Budget</h2>
    <div id="timing"></div>
    <h2>Events</h2>
    <div id="events" class="events"></div>
  </div>
  <script nonce="${nonce}">
    const api = acquireVsCodeApi();
    let enabled = {};
    let labels = {};
    let actions = [];
    function esc(value) { return String(value).replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', '\\'': '&#39;' }[c])); }
    function num(n) { return Number(n).toFixed(2); }
    function el(id) { return document.getElementById(id); }
    function pill(text, tone) { return '<span class="pill ' + (tone || '') + '">' + esc(text) + '</span>'; }
    function kv(rows) { return '<div class="kv">' + rows.map(r => '<span class="k">' + esc(r[0]) + '</span><span class="' + (r[2] || '') + '">' + r[1] + '</span>').join('') + '</div>'; }
    function table(headers, rows) {
      return '<table><thead><tr>' + headers.map(h => '<th>' + esc(h) + '</th>').join('') + '</tr></thead><tbody>' +
        rows.map(cells => '<tr>' + cells.map((c, i) => '<td class="' + (i > 0 ? 'num' : '') + '">' + c + '</td>').join('') + '</tr>').join('') + '</tbody></table>';
    }
    function renderToolbar() {
      el('toolbar').innerHTML = actions.map(a =>
        '<button class="secondary" data-action="' + a + '" ' + (enabled[a] ? '' : 'disabled') + '>' + esc(labels[a] || a) + '</button>'
      ).join('');
    }
    function renderBanner(vm) {
      el('banner').hidden = !vm.demo;
      el('empty').hidden = vm.connected;
      el('content').hidden = !vm.connected;
    }
    function renderAlarms(vm) {
      const badges = [];
      if (vm.alarms.performanceDegraded) badges.push(pill('HA_PERFORMANCE_DEGRADED', 'warn'));
      if (vm.alarms.timingLost) badges.push(pill('HA_TIMING_GUARANTEE_LOST', 'alarm'));
      if (vm.alarms.redundancyLost) badges.push(pill('REDUNDANCY_LOST', 'alarm'));
      for (const unit of vm.alarms.deadUnits) badges.push(pill(unit + ' DEAD', 'alarm'));
      if (badges.length === 0) badges.push(pill('no active alarms', 'ok'));
      el('alarms').innerHTML = badges.join('');
    }
    function renderOverview(vm) {
      let html = '<div class="grid">';
      html += '<div class="card">' + kv([
        ['Mode', esc(vm.pair.mode)],
        ['Epoch', vm.pair.epoch],
        ['Application generation', vm.pair.applicationGeneration],
        ['Committed state generation', vm.pair.stateGeneration],
        ['Admission', esc(vm.pair.admission)],
        ['Permit', vm.pair.permitOwner ? esc(vm.pair.permitOwner) : 'WITHHELD'],
      ]) + '</div>';
      for (const unit of vm.units) {
        const controlTone = unit.control === 'ACTIVE' ? 'ok' : (unit.control === 'ACTIVE_DEGRADED' ? 'warn' : (unit.dead ? 'alarm' : 'muted'));
        html += '<div class="card"><strong>' + esc(unit.id) + ' — configured ' + esc(unit.role) + '</strong>' +
          kv([
            ['SYNC', esc(unit.sync)],
            ['CONTROL', '<span class="' + controlTone + '">' + esc(unit.control) + '</span>'],
            ['Execution permit', unit.permit ? 'GRANTED' : '—'],
            ['TakeoverReady', unit.takeoverReady ? '<span class="ok">TRUE</span>' : 'false'],
          ]) + '</div>';
      }
      html += '<div class="card"><strong>Channels &amp; liveness</strong>' +
        kv([['Pair link (P)', vm.channels.p ? '<span class="ok">up</span>' : '<span class="alarm">lost</span>'],
          ['I/O path (I)', vm.channels.i ? '<span class="ok">up</span>' : '<span class="alarm">lost</span>']]) +
        '<div class="muted">CH1 ping/pong ' + vm.channels.ch1.ping + '/' + vm.channels.ch1.pong +
        ' · miss ' + vm.channels.ch1.missing + ' (+' + vm.channels.ch1.penalty + ')</div>' +
        '<div class="muted">CH2 ping/pong ' + vm.channels.ch2.ping + '/' + vm.channels.ch2.pong +
        ' · miss ' + vm.channels.ch2.missing + ' (+' + vm.channels.ch2.penalty + ')</div></div>';
      html += '<div class="card"><strong>TakeoverReady</strong>' + kv([
        ['Standby SYNC_READY', vm.ioReady.standbyReady ? 'true' : 'false'],
        ['IO_READY (link valid)', vm.ioReady.linkValid ? 'true' : 'false'],
        ['Calibration valid', vm.ioReady.calibrationValid ? 'true' : 'false'],
      ]) + (vm.takeoverReady ? pill('TAKEOVER READY', 'ok') : pill('NOT TAKEOVER READY', 'warn')) + '</div>';
      html += '</div>';
      el('overview').innerHTML = html;
    }
    function renderCalibration(vm) {
      const cal = vm.calibration;
      let html = table(['direction', 'current ms', 'EMA10 ms', 'EMA100 ms', 'max ms', 'count'],
        cal.directions.map(r => [esc(r.direction), num(r.currentMs), num(r.ema10Ms), num(r.ema100Ms), num(r.maxMs), r.count]));
      html += kv([['State', esc(cal.state), cal.state === 'CALIBRATED' ? 'ok' : 'warn'],
        ['Jitter envelope', num(cal.jitterMs) + ' ms'],
        ['Loss rate', cal.lossPercent.toFixed(3) + ' %']]);
      el('calibration').innerHTML = html;
    }
    function renderBarrier(vm) {
      const b = vm.barrier;
      const rows = b.modules.map(m => {
        const tone = m.ownerState === 'ARMED' ? 'ok' : (m.ownerState === 'CLAIMED_DISARMED' ? 'warn' : 'alarm');
        return '<tr' + (m.limiting ? ' class="limiting"' : '') + '><td>' + esc(m.id) + '</td><td>' + esc(m.profile) + '</td><td>' + esc(m.owner) + '</td>' +
          '<td class="' + tone + '">' + esc(m.ownerState) + '</td><td class="num">' + num(m.claimMs) + '</td><td class="num">' + num(m.claimMaxMs) + '</td>' +
          '<td class="num">' + (m.armed ? 'ARMED' : 'DISARMED') + '</td></tr>';
      }).join('');
      let html = '<div class="progress"><div style="width:' + (b.total ? Math.round(100 * b.claimed / b.total) : 0) + '%"></div></div>';
      html += '<div class="muted">CLAIMED_DISARMED ' + b.claimed + ' / ' + b.total + ' · barrier ' + (b.passed ? 'PASSED' : 'in progress') +
        ' · limiting ' + esc(b.limitingDevice) + ' · worst Σ ' + num(b.worstClaimMs) + ' ms</div>';
      html += '<table><thead><tr><th>module</th><th>profile</th><th>owner</th><th>owner state</th><th class="num">claim ms</th><th class="num">T ms</th><th class="num">armed</th></tr></thead><tbody>' + rows + '</tbody></table>';
      el('barrier').innerHTML = html;
    }
    function renderTiming(vm) {
      const b = vm.budget;
      let html = kv([
        ['Peer-failure confirmation', num(b.confirmationMs) + ' ms'],
        ['Recovery budget', num(b.budgetMs) + ' ms'],
        ['Calculated worst case', num(b.worstMs) + ' ms', b.ok ? 'ok' : 'alarm'],
        ['Predicted if failover now', num(b.predictedMs) + ' ms'],
      ]);
      html += b.ok
        ? '<div class="verdict ok ok-text">QUALIFIED — worst case within budget</div>'
        : '<div class="verdict alarm alarm-text">MINIMUM DEMONSTRATED BUDGET ' + num(b.worstMs) + ' ms</div>';
      el('timing').innerHTML = html;
    }
    function renderEvents(vm) {
      el('events').innerHTML = vm.events.length === 0
        ? '<div class="event"><span class="at">—</span><span class="sev muted">info</span><span class="muted">no events</span></div>'
        : vm.events.map(e => '<div class="event"><span class="at">' + esc(e.at) + '</span><span class="sev ' + esc(e.severity) + '">' + esc(e.severity) + '</span><span>' + esc(e.message) + '</span></div>').join('');
    }
    function render(message) {
      const vm = message.viewModel;
      enabled = message.enabled;
      labels = message.labels;
      actions = message.actions;
      renderBanner(vm);
      renderToolbar();
      renderAlarms(vm);
      renderOverview(vm);
      renderCalibration(vm);
      renderBarrier(vm);
      renderTiming(vm);
      renderEvents(vm);
    }
    document.addEventListener('click', event => {
      const target = event.target;
      if (!(target instanceof HTMLElement)) { return; }
      const action = target.dataset.action;
      if (action) { api.postMessage({ type: 'action', action }); return; }
      if (target.id === 'toggleDemo' || target.id === 'toggleDemoEmpty') { api.postMessage({ type: 'toggle-demo' }); }
    });
    window.addEventListener('message', event => render(event.data));
  </script>
</body>
</html>`;
}
