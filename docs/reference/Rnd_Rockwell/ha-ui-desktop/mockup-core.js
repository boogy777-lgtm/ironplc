/* HA redundancy desktop UI — shared core (state + render + demo sequences).
 * Contract: specs/design/ha-engineering-ui.md (flagship mockup reference).
 * No modules, no imports, no external assets. Exposes global `HA`.
 */
(function (global) {
  'use strict';

  var P1 = 'PLC-1';
  var P2 = 'PLC-2';

  function esc(s) {
    return String(s).replace(/[&<>"']/g, function (c) {
      return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c];
    });
  }
  function sleep(ms) { return new Promise(function (r) { setTimeout(r, ms); }); }
  function f2(n) { return Number(n).toFixed(2); }
  function stamp() {
    var d = new Date();
    return String(d.getHours()).padStart(2, '0') + ':' + String(d.getMinutes()).padStart(2, '0') +
      ':' + String(d.getSeconds()).padStart(2, '0') + '.' + String(d.getMilliseconds()).padStart(3, '0');
  }
  function clock() {
    var d = new Date();
    return String(d.getHours()).padStart(2, '0') + ':' + String(d.getMinutes()).padStart(2, '0') +
      ':' + String(d.getSeconds()).padStart(2, '0');
  }

  var MODULES = [
    { id: 'DO-01', profile: 'preconnected', claim: 0.72, claimMax: 1.74 },
    { id: 'DO-02', profile: 'preconnected', claim: 0.68, claimMax: 1.68 },
    { id: 'DO-03', profile: 'preconnected', claim: 0.81, claimMax: 1.91 },
    { id: 'DO-04', profile: 'preconnected', claim: 0.74, claimMax: 1.83 },
    { id: 'DO-05', profile: 'preconnected', claim: 0.69, claimMax: 1.70 },
    { id: 'DO-06', profile: 'preconnected', claim: 0.77, claimMax: 1.79 },
    { id: 'DO-07', profile: 'reconnect',    claim: 1.41, claimMax: 8.40 },
    { id: 'DO-08', profile: 'preconnected', claim: 0.70, claimMax: 1.72 }
  ];

  function makeMap(a, b) { var o = {}; o[P1] = a; o[P2] = b; return o; }

  function freshState() {
    return {
      mode: 'redundant',
      busy: false,
      tab: 'overview',
      eventsFilter: 'all',
      selectedModule: 'DO-07',
      selection: [],
      treeSel: 'pair',
      plc: {
        1: { role: 'Primary', ip: '192.168.10.11' },
        2: { role: 'Secondary', ip: '192.168.10.12' }
      },
      admission: 'ADMITTED',
      permitOwner: P1,
      epoch: 41,
      appGen: 105,
      stateGen: 1842,
      sync: makeMap('SYNC_READY', 'SYNC_READY'),
      control: makeMap('ACTIVE', 'IDLE'),
      online: makeMap(true, true),
      dead: makeMap(false, false),
      deadInfo: makeMap(null, null),
      unqualified: makeMap(false, false),
      ownerUnreachable: false,
      link: { ch1: 'OK', ch2: 'OK' },
      degraded: false,
      timingLost: false,
      redundancyLost: false,
      calibration: 'CALIBRATED',
      calProgress: 100,
      calBaseline: '2026-09-16 09:14:22',
      calTrigger: 'commissioning',
      counters: {
        ch1: { ping: 120304, pong: 120304, missing: 0, penalty: 0 },
        ch2: { ping: 120304, pong: 120304, missing: 0, penalty: 0 }
      },
      scanPhase: 0.62,
      scan: { ema10: 7.40, ema100: 7.10, max: 11.80, jitter: 0.62 },
      config: { confirmationMs: 5.0, budgetMs: 40.0 },
      modules: MODULES.map(function (m, i) {
        return {
          id: m.id, profile: m.profile, claim: m.claim, claimMax: m.claimMax,
          owner: P1, ownerEpoch: 41, ownerState: 'ARMED', armed: true,
          outSeq: 9000 + i * 17, packetAgeUs: 180 + i * 9
        };
      }),
      barrier: { claimed: 8, total: 8, passed: true },
      connection: 'connected',
      edit: {
        mode: 'monitoring',
        generation: 1,
        code: ['PROGRAM Main', 'Counter := Counter + 1;', 'IF Counter > 100 THEN', '  Counter := 0;', 'END_IF;'],
        baseline: ['PROGRAM Main', 'Counter := Counter + 1;', 'IF Counter > 100 THEN', '  Counter := 0;', 'END_IF;'],
        shadow: ['PROGRAM Main', 'Counter := Counter + 1;', 'IF Counter > 100 THEN', '  Counter := 0;', 'END_IF;'],
        keystrokes: 0,
        changed: [],
        confirm: null,
        live: { Counter: 12537, Temperature: 78.4, Running: true }
      },
      events: []
    };
  }

  var state = freshState();
  var opts = { surface: 'classic', inspector: false, treeStyle: 'tree' };
  var hosts = {};
  var subs = [];

  function notify(tick) {
    for (var i = 0; i < subs.length; i++) subs[i](!!tick);
  }

  function seedEvents() {
    var base = Date.now() - 90000;
    function at(off, sev, msg) {
      var d = new Date(base + off);
      return { t: String(d.getHours()).padStart(2, '0') + ':' + String(d.getMinutes()).padStart(2, '0') +
        ':' + String(d.getSeconds()).padStart(2, '0') + '.' + String(d.getMilliseconds()).padStart(3, '0'),
        sev: sev, msg: msg };
    }
    state.events = [
      at(86000, 'info', 'ping/pong boot: pair found on port 1 — admission complete, permit granted to ' + P1),
      at(60000, 'info', 'commissioning calibration complete — HA link profile accepted (QUALIFIED)'),
      at(42000, 'info', 'claim/ARM epoch 41 — ' + P1 + ' ACTIVE, 8/8 required modules ARMED'),
      at(12000, 'warn', 'channel 2 (I/O chain) jitter envelope widened briefly — within calibration'),
      at(3000, 'info', 'ping/pong healthy on both channels (ch1 pair link, ch2 I/O chain)'),
      at(-40000, 'alarm', 'previous boot: REDUNDANCY_LOST — cleared by manual repair and requalification')
    ];
  }

  function addEvent(sev, msg) {
    state.events.unshift({ t: stamp(), sev: sev, msg: msg });
    if (state.events.length > 160) state.events.pop();
  }

  /* ---------------- derived ---------------- */
  function activeUnit() { return state.control[P1] === 'ACTIVE' || state.control[P1] === 'ACTIVE_DEGRADED' ? P1 : ((state.control[P2] === 'ACTIVE' || state.control[P2] === 'ACTIVE_DEGRADED') ? P2 : null); }
  function standbyUnit() { var a = activeUnit(); return a === P1 ? P2 : (a === P2 ? P1 : null); }
  function channelsOk() { return state.link.ch1 !== 'LOST' && state.link.ch2 !== 'LOST'; }
  function ownerAlive() { var a = activeUnit(); return a !== null && !state.dead[a]; }
  function standbyAlive() { var s = standbyUnit(); return s !== null && !state.dead[s]; }
  function linkValid() { return channelsOk() || state.ownerUnreachable; }
  function ioReady() { return state.mode === 'redundant' && (state.link.ch2 !== 'LOST' || state.ownerUnreachable) && !state.redundancyLost; }
  function takeoverReady(unit) {
    if (state.mode !== 'redundant') return false;
    if (state.calibration !== 'CALIBRATED') return false;
    if (state.redundancyLost) return false;
    if (state.dead[unit]) return false;
    if (unit !== standbyUnit()) return false;
    return state.sync[unit] === 'SYNC_READY' && (channelsOk() || state.ownerUnreachable);
  }
  function can(action) {
    var red = state.mode === 'redundant', free = !state.busy && !state.redundancyLost;
    var owner = activeUnit(), stb = standbyUnit();
    if (action === 'swap') {
      return red && free && ownerAlive() && standbyAlive() && channelsOk() &&
        state.sync[owner] === 'SYNC_READY' && state.sync[stb] === 'SYNC_READY' && state.calibration === 'CALIBRATED';
    }
    if (action === 'death') return red && free && ownerAlive();
    if (action === 'death-secondary') return red && free && ownerAlive() && standbyAlive();
    if (action === 'resurrect-primary') {
      return red && !state.busy && ((state.dead[P1] && state.deadInfo[P1] === 'owner') || (state.dead[P2] && state.deadInfo[P2] === 'owner'));
    }
    if (action === 'resurrect-secondary') return red && !state.busy && ((state.dead[P1] && state.deadInfo[P1] === 'standby') || (state.dead[P2] && state.deadInfo[P2] === 'standby'));
    if (action === 'kill-p') return red && free && state.link.ch1 === 'OK';
    if (action === 'kill-i') return red && free && state.link.ch2 === 'OK';
    if (action === 'restore-channels') return red && !state.busy && (state.link.ch1 === 'LOST' || state.link.ch2 === 'LOST');
    if (action === 'partition') {
      return red && free && ownerAlive() && standbyAlive() && !channelsOk() &&
        state.sync[stb] === 'SYNC_READY' && state.calibration === 'CALIBRATED';
    }
    if (action === 'degrade') return red && !state.busy;
    if (action === 'calibrate') return red && !state.busy;
    return true;
  }
  function claimMaxNow() { return state.degraded ? 24.10 : 20.77; }
  function claimEmaNow() { return state.degraded ? 7.90 : 6.52; }
  function outputMaxNow() { return state.degraded ? 0.90 : 0.60; }
  function outputEmaNow() { return state.degraded ? 0.36 : 0.31; }
  function budgetCheck() {
    var worst = state.config.confirmationMs + claimMaxNow() + state.scan.max + outputMaxNow();
    return { worst: worst, ok: worst <= state.config.budgetMs + 1e-9 };
  }
  function predictedNow() {
    var remaining = (1 - state.scanPhase) * state.scan.ema10;
    return state.config.confirmationMs + claimEmaNow() + 0.42 + remaining + outputEmaNow();
  }
  function alarmsActive() {
    var n = 0;
    if (state.degraded) n++;
    if (state.timingLost) n++;
    if (state.redundancyLost) n++;
    if (state.dead[P1] || state.dead[P2]) n++;
    return n;
  }

  /* ---------------- fragments ---------------- */
  function pill(label, value, tone) {
    return '<span class="ha-pill ' + (tone || '') + '"><span class="ha-dot"></span>' + esc(label) +
      ' <b>' + esc(value) + '</b></span>';
  }
  function chip(label, on, tone) {
    return '<span class="ha-chip' + (on ? ' on ' + (tone || '') : '') + '">' + esc(label) + '</span>';
  }
  function syncChips(unit) {
    var cur = state.sync[unit];
    return ['deSYNC', 'SYNCING', 'SYNC_READY'].map(function (s) {
      return chip(s, cur === s, s === 'SYNC_READY' ? 'ok' : 'warn');
    }).join('');
  }
  function controlChips(unit) {
    var cur = state.control[unit];
    if (state.dead[unit]) cur = 'DEAD';
    else if (!state.online[unit]) cur = 'OFFLINE';
    var list = ['IDLE', 'CLAIMING', 'ACTIVE', 'ACTIVE_DEGRADED', 'REDUNDANCY_LOST', 'DEAD'];
    return list.map(function (s) {
      var tone = (s === 'ACTIVE') ? 'ok' : (s === 'ACTIVE_DEGRADED' ? 'warn' : ((s === 'REDUNDANCY_LOST' || s === 'DEAD') ? 'alarm' : ''));
      var effective = cur;
      if (s === 'ACTIVE' && state.degraded && cur === 'ACTIVE') effective = 'ACTIVE_DEGRADED';
      return chip(s, effective === s, tone);
    }).join('');
  }
  function btn(label, action, variant, disabled) {
    return '<button type="button" class="ha-btn ' + (variant || '') + '" ' + (disabled ? 'disabled' : '') +
      ' onclick="HA.act(\'' + action + '\')">' + esc(label) + '</button>';
  }

  /* ---------------- hosts ---------------- */
  function renderTitle() {
    if (hosts.title) {
      var mode = state.mode === 'redundant' ? 'Redundant pair' : 'Standalone';
      hosts.title.textContent = 'IronPLC Engineering — ' + mode + ' (PAIR-RB-0417)';
    }
  }

  function renderToolbar() {
    if (!hosts.toolbar) return;
    var act = activeUnit();
    var canSwap = state.mode === 'redundant' && !state.busy && act !== null && standbyUnit() !== null &&
      state.sync[act] === 'SYNC_READY' && state.sync[standbyUnit()] === 'SYNC_READY' && state.calibration === 'CALIBRATED';
    var groups = [
      { name: 'Online', items: [
        { g: '\u25B6', t: 'Go Online', a: 'noop' },
        { g: '\u23FB', t: 'Go Offline', a: 'noop' },
        { g: '\u21C4', t: 'Manual swap', a: canSwap ? 'swap' : 'noop', dis: !canSwap }
      ] },
      { name: 'Redundancy', items: [
        { g: '\u2620', t: 'Death sim', a: 'death', dis: state.busy || state.mode !== 'redundant' },
        { g: '\u26A0', t: 'Degrade link', a: 'degrade', dis: state.busy || state.mode !== 'redundant' },
        { g: '\u25B6', t: 'Run calibration', a: 'calibrate', dis: state.busy || state.mode !== 'redundant' }
      ] },
      { name: 'Diagnostics', items: [
        { g: '\u2315', t: 'Events', a: 'tab-events' },
        { g: '\u03A3', t: 'Budget', a: 'tab-timing' },
        { g: '\u21BA', t: 'Reset', a: 'reset' }
      ] }
    ];
    var ribbon = opts.surface === 'ribbon';
    var html = groups.map(function (grp) {
      var items = grp.items.map(function (it) {
        return '<button type="button" class="ha-tbtn" ' + (it.dis ? 'disabled' : '') + ' title="' + esc(it.t) +
          '" onclick="HA.act(\'' + it.a + '\')"><span class="ha-tglyph">' + it.g + '</span>' +
          (ribbon ? '<span class="ha-tlabel">' + esc(it.t) + '</span>' : '') + '</button>';
      }).join('');
      return '<div class="ha-tgroup">' + items + '<span class="ha-tcaption">' + esc(grp.name) + '</span></div>';
    }).join('');
    hosts.toolbar.innerHTML = html;
  }

  function treeNode(id, label, glyph, depth, sel) {
    return '<div class="ha-node' + (sel ? ' sel' : '') + '" style="--d:' + depth + '" tabindex="0" ' +
      'onclick="HA.tree(\'' + id + '\')" onkeydown="HA.treeKey(event,\'' + id + '\')">' +
      '<span class="ha-glyph">' + glyph + '</span>' + esc(label) + '</div>';
  }
  function renderTree() {
    if (!hosts.tree) return;
    var html = '';
    html += treeNode('pair', 'PAIR-RB-0417', '\u25A3', 0, state.treeSel === 'pair');
    html += treeNode('plc1', P1 + '  (' + state.plc[1].role + ')  ' + state.plc[1].ip, '\u25A4', 1, state.treeSel === 'plc1');
    html += treeNode('ch1', 'CH1 — pair link', '\u2261', 2, state.treeSel === 'ch1');
    html += treeNode('ch2', 'CH2 — I/O chain' + (state.link.ch2 === 'DEGRADED' ? '  (degraded)' : ''), '\u2261', 2, state.treeSel === 'ch2');
    html += treeNode('plc2', P2 + '  (' + state.plc[2].role + ')  ' + state.plc[2].ip, '\u25A4', 1, state.treeSel === 'plc2');
    html += treeNode('io', 'I/O modules (8)', '\u25A6', 1, state.treeSel === 'io');
    state.modules.forEach(function (m) {
      var mark = (m.id === state.selectedModule ? ' \u25C9' : '');
      html += treeNode('mod-' + m.id, m.id + ' — ' + m.profile + mark, '\u25CB', 2, state.treeSel === 'mod-' + m.id);
    });
    html += treeNode('events-node', 'Event ring', '\u2315', 1, state.treeSel === 'events-node');
    hosts.tree.innerHTML = html;
  }

  function renderOnline() {
    if (!hosts.online) return;
    var h = '';
    h += '<div class="ha-field"><label>Mode</label><select class="ha-select" onchange="HA.setMode(this.value)">' +
      '<option value="redundant"' + (state.mode === 'redundant' ? ' selected' : '') + '>Redundant pair</option>' +
      '<option value="standalone"' + (state.mode === 'standalone' ? ' selected' : '') + '>Standalone</option></select></div>';
    h += '<div class="ha-field"><label>' + P1 + ' role</label><select class="ha-select" onchange="HA.setRole(1,this.value)">' +
      '<option' + (state.plc[1].role === 'Primary' ? ' selected' : '') + '>Primary</option>' +
      '<option' + (state.plc[1].role === 'Secondary' ? ' selected' : '') + '>Secondary</option></select>' +
      '<input class="ha-input" type="text" value="' + esc(state.plc[1].ip) + '" onchange="HA.setIp(1,this.value)"></div>';
    h += '<div class="ha-field"><label>' + P2 + ' role</label><select class="ha-select" onchange="HA.setRole(2,this.value)">' +
      '<option' + (state.plc[2].role === 'Primary' ? ' selected' : '') + '>Primary</option>' +
      '<option' + (state.plc[2].role === 'Secondary' ? ' selected' : '') + '>Secondary</option></select>' +
      '<input class="ha-input" type="text" value="' + esc(state.plc[2].ip) + '" onchange="HA.setIp(2,this.value)"></div>';
    h += '<div class="ha-field ha-field-pills">';
    if (state.mode === 'redundant') {
      var admTone = state.admission === 'ADMITTED' ? 'ok' : (state.admission === 'CALIBRATING' ? 'warn' : 'alarm');
      h += pill('ADMISSION', state.admission, admTone);
      h += pill('PERMIT', state.permitOwner || 'WITHHELD', state.permitOwner ? 'ok' : 'alarm');
    } else {
      h += pill('ADMISSION', 'STANDALONE', 'warn');
      h += pill('PERMIT', P1 + ' (startup)', 'ok');
    }
    h += '</div>';
    hosts.online.innerHTML = h;
  }

  function renderDemoBar() {
    if (!hosts.demo) return;
    var act = activeUnit();
    var canSwap = state.mode === 'redundant' && !state.busy && act !== null && standbyUnit() !== null &&
      state.sync[act] === 'SYNC_READY' && state.sync[standbyUnit()] === 'SYNC_READY' && state.calibration === 'CALIBRATED';
    var h = '<span class="ha-tag">Demo</span>';
    h += btn('Manual swap (commanded)', 'swap', 'primary', !canSwap);
    h += btn('Simulate primary death', 'death', 'danger', !(state.mode === 'redundant' && !state.busy && act !== null));
    h += btn(state.degraded ? 'Restore link' : 'Degrade link', 'degrade', 'warn', !(state.mode === 'redundant' && !state.busy));
    h += btn('Run calibration', 'calibrate', '', !(state.mode === 'redundant' && !state.busy));
    h += btn('Reset', 'reset', '', state.busy);
    if (state.busy) h += '<span class="ha-hint">working…</span>';
    hosts.demo.innerHTML = h;
  }

  function renderTabs() {
    if (!hosts.tabs) return;
    var tabs = [
      ['overview', 'Pair Overview'],
      ['calibration', 'Calibration'],
      ['barrier', 'Ownership &amp; Barrier'],
      ['timing', 'Timing Budget'],
      ['events', 'Events']
    ];
    hosts.tabs.innerHTML = tabs.map(function (t) {
      var badge = (t[0] === 'events' && alarmsActive() > 0) ? '<span class="ha-badge">' + alarmsActive() + '</span>' : '';
      return '<button type="button" class="ha-tab' + (state.tab === t[0] ? ' active' : '') +
        '" onclick="HA.tab(\'' + t[0] + '\')">' + t[1] + badge + '</button>';
    }).join('');
    if (hosts.panels) {
      Array.prototype.forEach.call(hosts.panels.querySelectorAll('.ha-panel'), function (p) {
        p.classList.toggle('active', p.id === 'panel-' + state.tab);
      });
    }
  }

  /* ---------------- panels ---------------- */
  function unitCard(unit, idx) {
    var role = state.plc[idx].role;
    var ip = state.plc[idx].ip;
    var on = state.online[unit];
    var ctl = !on ? 'OFFLINE' : state.control[unit];
    if (on && state.degraded && ctl === 'ACTIVE') ctl = 'ACTIVE_DEGRADED';
    var tr = takeoverReady(unit);
    var cls = 'ha-card' + (state.mode === 'standalone' && unit === P2 ? ' ha-dim' : '');
    var h = '<div class="' + cls + '"><h3>' + unit + ' — configured ' + esc(role) + '</h3><div class="ha-kv">';
    h += '<span class="k">IP</span><span class="v">' + esc(ip) + '</span>';
    h += '<span class="k">Unit state</span><span class="v">' + (on ? 'online' : 'OFFLINE (no ping/pong)') + '</span>';
    h += '<span class="k">CONTROL</span><span class="v ' + (ctl === 'ACTIVE' ? 'ha-ok' : (ctl === 'ACTIVE_DEGRADED' ? 'ha-warn' : 'ha-muted')) + '">' + ctl + '</span>';
    h += '<span class="k">EXECUTION PERMIT</span><span class="v">' + (state.permitOwner === unit ? 'GRANTED' : '—') + '</span>';
    h += '<span class="k">TakeoverReady</span><span class="v">' + (tr ? '<span class="ha-ok">TRUE</span>' : '<span class="ha-muted">false</span>') + '</span>';
    h += '</div><div class="ha-chips">' + syncChips(unit) + '</div>';
    h += '<div class="ha-chips">' + controlChips(unit) + '</div></div>';
    return h;
  }

  function countersTable(c, chState) {
    var stateTxt = chState === 'DEGRADED' ? '<span class="ha-warn">DEGRADED</span>' :
      (chState === 'LOST' ? '<span class="ha-alarm">LOST</span>' : '<span class="ha-ok">OK</span>');
    return '<table class="ha-table"><thead><tr><th>counter</th><th class="ha-num">value</th><th class="ha-num">step</th></tr></thead><tbody>' +
      '<tr><td>ping_seq</td><td class="ha-num">' + c.ping.toLocaleString() + '</td><td class="ha-num ha-ok">+1</td></tr>' +
      '<tr><td>pong_seq</td><td class="ha-num">' + c.pong.toLocaleString() + '</td><td class="ha-num ha-ok">+1</td></tr>' +
      '<tr><td>missing exchange</td><td class="ha-num">' + c.missing + '</td><td class="ha-num ha-alarm">+1000</td></tr>' +
      '<tr><td>penalty counter</td><td class="ha-num">' + c.penalty.toLocaleString() + '</td><td class="ha-num ha-muted">—</td></tr>' +
      '<tr><td>channel</td><td class="ha-num" colspan="2">' + stateTxt + '</td></tr></tbody></table>';
  }

  function renderOverview() {
    var p = hosts.panelOverview;
    if (!p) return;
    if (state.mode === 'standalone') {
      p.innerHTML = '<div class="ha-card"><h3>Standalone admission</h3><div class="ha-kv">' +
        '<span class="k">Pair</span><span class="v">none configured</span>' +
        '<span class="k">Permit</span><span class="v ha-ok">issued at startup (' + P1 + ')</span>' +
        '<span class="k">Ping/pong</span><span class="v ha-muted">not running (no pair link)</span></div>' +
        '<p class="ha-note">Standalone admission grants the permit before the first scan; the redundancy statechart does not run.</p></div>';
      return;
    }
    var h = '<div class="ha-grid">' + unitCard(P1, 1) + unitCard(P2, 2) + '</div>';
    h += '<div class="ha-grid" style="margin-top:10px">';
    h += '<div class="ha-card"><h3>Pair</h3><div class="ha-kv">' +
      '<span class="k">PairId</span><span class="v">PAIR-RB-0417</span>' +
      '<span class="k">Epoch</span><span class="v">' + state.epoch + '</span>' +
      '<span class="k">Application generation</span><span class="v">' + state.appGen + '</span>' +
      '<span class="k">Committed state generation</span><span class="v">' + state.stateGen + '</span>' +
      '<span class="k">Calibration</span><span class="v">' + esc(state.calibration) + ' (baseline ' + esc(state.calBaseline) + ')</span>' +
      '<span class="k">Last trigger</span><span class="v">' + esc(state.calTrigger) + '</span></div></div>';
    var stb = standbyUnit() || P2;
    var trInputs = [
      ['SYNC_READY (standby)', state.sync[stb] === 'SYNC_READY'],
      ['IO_READY', ioReady()],
      ['RedundancyLinkValid', linkValid()],
      ['Calibration valid', state.calibration === 'CALIBRATED']
    ];
    h += '<div class="ha-card"><h3>TakeoverReady verdict</h3><div class="ha-kv">' +
      trInputs.map(function (r) {
        return '<span class="k">' + r[0] + '</span><span class="v ' + (r[1] ? 'ha-ok' : 'ha-muted') + '">' + (r[1] ? 'true' : 'false') + '</span>';
      }).join('') + '</div><div class="ha-chips" style="margin-top:8px">' +
      (takeoverReady(stb) ? pill('TAKEOVER READY', '', 'ok') : pill('NOT TAKEOVER READY', '', 'warn')) + '</div></div>';
    h += '<div class="ha-card"><h3>IO_READY breakdown</h3><div class="ha-kv">' +
      '<span class="k">Required inputs observable</span><span class="v ha-ok">true</span>' +
      '<span class="k">Standby connections valid</span><span class="v ' + (ioReady() ? 'ha-ok' : 'ha-alarm') + '">' + (ioReady() ? 'true' : 'false') + '</span>' +
      '<span class="k">Configs match</span><span class="v ha-ok">true (ConfigHash equal)</span>' +
      '<span class="k">Epochs valid</span><span class="v ha-ok">true (' + state.epoch + ')</span></div></div>';
    h += '</div>';
    h += '<div class="ha-grid" style="margin-top:10px">' +
      '<div class="ha-card"><h3>Ping/Pong — channel 1 (pair link)</h3>' + countersTable(state.counters.ch1, state.link.ch1) + '</div>' +
      '<div class="ha-card"><h3>Ping/Pong — channel 2 (I/O chain)</h3>' + countersTable(state.counters.ch2, state.link.ch2) + '</div>' +
      '</div>';
    h += '<p class="ha-note">Silence is a <b>missing expected +1</b>, not a bare packet timeout. A missed exchange adds +1000 to the operational penalty counter (HMI diagnostics only — the takeover arbiter is the case table plus the fencing chain).</p>';
    p.innerHTML = h;
  }

  function renderCalibration() {
    var p = hosts.panelCalibration;
    if (!p) return;
    var deg = state.degraded;
    function mrow(label, cur, e10, e100, mx, count) {
      return '<tr><td>' + label + '</td><td class="ha-num">' + f2(cur) + '</td><td class="ha-num">' + f2(e10) +
        '</td><td class="ha-num">' + f2(e100) + '</td><td class="ha-num">' + f2(mx) + '</td><td class="ha-num ha-muted">' + count.toLocaleString() + '</td></tr>';
    }
    var ch2 = deg
      ? { cur: 1.42, e10: 1.18, e100: 0.62, max: 2.06, count: 120198, loss: 0.0021, consec: 1 }
      : { cur: 0.33, e10: 0.34, e100: 0.31, max: 0.88, count: 120304, loss: 0.00003, consec: 0 };
    var h = '<div class="ha-grid"><div class="ha-card"><h3>HA link profile — round-trip (both directions)</h3>';
    h += '<table class="ha-table"><thead><tr><th>direction</th><th class="ha-num">current</th><th class="ha-num">EMA10</th><th class="ha-num">EMA100</th><th class="ha-num">max</th><th class="ha-num">count</th></tr></thead><tbody>';
    h += mrow('A→B→A', 0.31, 0.34, 0.30, 0.82, 120304);
    h += mrow('B→A→B', ch2.cur, ch2.e10, ch2.e100, ch2.max, ch2.count);
    h += '</tbody></table>';
    h += '<div class="ha-kv" style="margin-top:8px">' +
      '<span class="k">Jitter envelope</span><span class="v">' + f2(deg ? 0.18 * 3 : 0.18) + ' ms</span>' +
      '<span class="k">Loss rate</span><span class="v">' + (ch2.loss * 100).toFixed(4) + ' %</span>' +
      '<span class="k">Max consecutive loss</span><span class="v">' + ch2.consec + ' exchange(s)</span></div>';
    h += '<p class="ha-note">Calibration runs under realistic worst load — PLC application running, I/O running, crossload running. A PONG delayed by a heavy scan is otherwise indistinguishable from a peer failure.</p></div>';
    h += '<div class="ha-card"><h3>Per-side processing &amp; scan</h3><div class="ha-kv">' +
      '<span class="k">A processing latency EMA</span><span class="v">0.12 ms</span>' +
      '<span class="k">B processing latency EMA</span><span class="v">' + (deg ? '0.19' : '0.11') + ' ms</span>' +
      '<span class="k">Scan A EMA10 / max / jitter</span><span class="v">' + f2(state.scan.ema10) + ' / ' + f2(state.scan.max) + ' / ' + f2(deg ? state.scan.jitter * 3 : state.scan.jitter) + ' ms</span>' +
      '<span class="k">Scan B EMA10 / max / jitter</span><span class="v">7.6 / 12.1 / 0.7 ms</span></div></div>';
    h += '<div class="ha-card"><h3>Calibration state</h3><div class="ha-kv">' +
      '<span class="k">Run state</span><span class="v ' + (state.calibration === 'CALIBRATED' ? 'ha-ok' : 'ha-warn') + '">' + esc(state.calibration) + '</span>' +
      '<span class="k">Baseline</span><span class="v">' + esc(state.calBaseline) + '</span>' +
      '<span class="k">Last trigger</span><span class="v">' + esc(state.calTrigger) + '</span></div>';
    if (state.calibration === 'CALIBRATING') {
      h += '<div class="ha-progress"><div class="ha-bar" style="width:' + state.calProgress + '%"></div></div>' +
        '<p class="ha-note">Running: ' + state.calProgress + ' % — calibration traffic both directions (A→B→A, B→A→B).</p>';
    } else {
      h += '<div class="ha-progress"><div class="ha-bar ok" style="width:100%"></div></div>';
    }
    h += '<p class="ha-note">Recalibration triggers (invalidate qualification): NIC replaced · medium/SFP changed · link speed changed · topology changed · runtime version changed · HA protocol changed.</p>';
    h += '<div class="ha-row">' + btn('Run calibration', 'calibrate', 'primary', state.busy || state.mode !== 'redundant') +
      btn('Invalidate (NIC)', 'invalidate-nic', '', state.busy || state.mode !== 'redundant') +
      btn('Invalidate (topology)', 'invalidate-topo', '', state.busy || state.mode !== 'redundant') + '</div></div></div>';
    p.innerHTML = h;
  }

  function renderBarrier() {
    var p = hosts.panelBarrier;
    if (!p) return;
    var seqEma = 0, seqMax = 0, limiting = null;
    state.modules.forEach(function (m) {
      seqEma += m.claim;
      seqMax += m.claimMax;
      if (!limiting || m.claimMax > limiting.claimMax) limiting = m;
    });
    var h = '<div class="ha-card"><h3>Ownership barrier — required output modules</h3>';
    h += '<table class="ha-table"><thead><tr><th>module</th><th>profile</th><th>owner</th><th>owner state</th><th class="ha-num">owner epoch</th><th class="ha-num">claim (ms)</th><th class="ha-num">T contrib (ms)</th><th class="ha-num">armed</th><th class="ha-num">packet age (µs)</th></tr></thead><tbody>';
    state.modules.forEach(function (m) {
      var cls = (m.id === limiting.id) ? ' class="limit"' : '';
      var tone = m.ownerState === 'ARMED' ? 'ha-ok' : (m.ownerState === 'CLAIMED_DISARMED' ? 'ha-warn' : 'ha-alarm');
      h += '<tr' + cls + ' onclick="HA.selectModule(\'' + m.id + '\')"><td>' + m.id + '</td><td>' + m.profile + '</td><td>' + esc(m.owner) + '</td>' +
        '<td class="' + tone + '">' + m.ownerState + '</td><td class="ha-num">' + m.ownerEpoch + '</td>' +
        '<td class="ha-num">' + f2(m.claim) + '</td><td class="ha-num">' + f2(m.claimMax) + '</td>' +
        '<td class="ha-num">' + (m.armed ? 'ARMED' : 'DISARMED') + '</td><td class="ha-num ha-muted">' + m.packetAgeUs + '</td></tr>';
    });
    h += '</tbody></table><p class="ha-note">Sequential v1 claim: <span class="mono">T_claim = Σ T_claim,i</span> — limiting device <b>' + limiting.id +
      '</b> (' + limiting.profile + ', ' + f2(limiting.claimMax) + ' ms). Worst ownership recovery = <span class="mono">Σ MaxQualified = ' + f2(seqMax) + ' ms</span>; measured EMA sum = <span class="mono">' + f2(seqEma) + ' ms</span>.</p></div>';
    var pct = Math.round(100 * state.barrier.claimed / state.barrier.total);
    h += '<div class="ha-grid" style="margin-top:10px">';
    h += '<div class="ha-card"><h3>Barrier progress</h3><div class="ha-kv">' +
      '<span class="k">CLAIMED_DISARMED</span><span class="v">' + state.barrier.claimed + ' / ' + state.barrier.total + '</span>' +
      '<span class="k">OWNERSHIP_BARRIER</span><span class="v ' + (state.barrier.passed ? 'ha-ok' : 'ha-warn') + '">' + (state.barrier.passed ? 'PASSED' : 'IN PROGRESS') + '</span>' +
      '</div><div class="ha-progress"><div class="ha-bar ' + (state.barrier.passed ? 'ok' : 'warn') + '" style="width:' + pct + '%"></div></div>' +
      '<p class="ha-note">Acquisition alone never actuates: every module reaches CLAIMED_DISARMED first; ARM is a separate ordered phase after the barrier. Partial ownership never means ACTIVE.</p></div>';
    h += '<div class="ha-card"><h3>Worst ownership recovery</h3><div class="ha-kv">' +
      '<span class="k">Sequential bound</span><span class="v">' + f2(claimMaxNow()) + ' ms</span>' +
      '<span class="k">Measured EMA</span><span class="v">' + f2(claimEmaNow()) + ' ms</span>' +
      '<span class="k">Limiting device</span><span class="v ha-warn">' + limiting.id + '</span>' +
      '<span class="k">Old-owner timeout</span><span class="v">' + (state.barrier.passed ? '0.00 (all released)' : 'owner-release + Forward_Open + verify (reconnect)') + '</span>' +
      '</div></div></div>';
    p.innerHTML = h;
  }

  function renderTiming() {
    var p = hosts.panelTiming;
    if (!p) return;
    var bc = budgetCheck();
    var pred = predictedNow();
    var terms = [
      ['T_peer-detect', state.config.confirmationMs, claimMaxNow() > 20 ? 3.50 : 1.00, 'confirmation time (configured)'],
      ['T_claim (Σ modules)', claimEmaNow(), claimMaxNow(), 'sequential v1'],
      ['T_arm', 0.42, 0.90, 'ordered ARM phase'],
      ['T_scan-safe-point', (1 - state.scanPhase) * state.scan.ema10, state.scan.max, 'phase-aware: T_next-commit − t_takeover'],
      ['T_output-apply', outputEmaNow(), outputMaxNow(), 'commit to physical']
    ];
    var h = '<div class="ha-formula">T_recovery = T_peer-detect + T_claim + T_arm + T_scan-safe-point + T_output-apply</div>';
    h += '<div class="ha-grid" style="margin-top:10px"><div class="ha-card"><h3>Formula terms — measured vs qualification bound</h3>' +
      '<table class="ha-table"><thead><tr><th>term</th><th class="ha-num">measured (ms)</th><th class="ha-num">bound (ms)</th><th>note</th></tr></thead><tbody>' +
      terms.map(function (t) {
        return '<tr><td>' + t[0] + '</td><td class="ha-num">' + f2(t[1]) + '</td><td class="ha-num">' + f2(t[2]) + '</td><td class="ha-muted">' + t[3] + '</td></tr>';
      }).join('') + '</tbody></table></div>';
    h += '<div class="ha-card"><h3>Budget check</h3><div class="ha-kv">' +
      '<span class="k">Configured confirmation</span><span class="v">' + f2(state.config.confirmationMs) + ' ms</span>' +
      '<span class="k">Maximum recovery budget</span><span class="v">' + f2(state.config.budgetMs) + ' ms</span>' +
      '<span class="k">Calculated worst case</span><span class="v ' + (bc.ok ? 'ha-ok' : 'ha-alarm') + '">' + f2(bc.worst) + ' ms</span>' +
      '<span class="k">Predicted if failover now</span><span class="v">' + f2(pred) + ' ms</span></div>';
    h += '<div style="margin-top:10px">' + (bc.ok
      ? '<span class="ha-verdict ok"><span class="ha-dot"></span>QUALIFIED — worst case within budget</span>'
      : '<span class="ha-verdict alarm"><span class="ha-dot"></span>HA REQUIREMENT CANNOT BE GUARANTEED — minimum demonstrated budget ' + f2(bc.worst) + ' ms</span>') + '</div>';
    h += '<p class="ha-note">T_detect + T_claim,max + T_scan,max + T_output,max ≤ budget. A failing budget is reported, never silently applied.</p></div>';
    h += '<div class="ha-card"><h3>Scan phase (live)</h3><div class="ha-row"><input type="range" min="0" max="100" value="' + Math.round(state.scanPhase * 100) +
      '" oninput="HA.onPhase(this.value)"><span class="mono" id="haPhaseVal">' + Math.round(state.scanPhase * 100) + ' %</span></div>' +
      '<div class="ha-kv" style="margin-top:8px">' +
      '<span class="k">T_safepoint</span><span class="v" id="haSafeVal">' + f2((1 - state.scanPhase) * state.scan.ema10) + ' ms</span>' +
      '<span class="k">Scan EMA10 / EMA100</span><span class="v">' + f2(state.scan.ema10) + ' / ' + f2(state.scan.ema100) + ' ms</span>' +
      '<span class="k">Scan max / jitter</span><span class="v">' + f2(state.scan.max) + ' / ' + f2(state.degraded ? state.scan.jitter * 3 : state.scan.jitter) + ' ms</span>' +
      '<span class="k">Predicted if failover now</span><span class="v" id="haPredVal">' + f2(pred) + ' ms</span></div></div>';
    h += '<div class="ha-card"><h3>Engineer parameters (haSetTimingBudget)</h3><div class="ha-row">' +
      '<label class="ha-hint">Confirmation (ms)</label><input class="ha-input" type="number" step="0.5" min="0.5" id="haConfInp" value="' + state.config.confirmationMs + '">' +
      '<label class="ha-hint">Budget (ms)</label><input class="ha-input" type="number" step="1" min="1" id="haBudgetInp" value="' + state.config.budgetMs + '">' +
      btn('Apply', 'apply-budget') + '</div>' +
      '<p class="ha-note">The runtime translates process-reaction times into supervision parameters — the UI never exposes ping periods or missed-ping counts.</p></div></div>';
    p.innerHTML = h;
  }

  function renderEvents() {
    var p = hosts.panelEvents;
    if (!p) return;
    var opts = ['all', 'info', 'warn', 'alarm'].map(function (s) {
      return '<option value="' + s + '"' + (state.eventsFilter === s ? ' selected' : '') + '>' + s.toUpperCase() + '</option>';
    }).join('');
    var h = '<div class="ha-card"><h3>Timestamped event ring</h3><div class="ha-row">' +
      '<label class="ha-hint">Severity filter</label><select class="ha-select" onchange="HA.setFilter(this.value)">' + opts + '</select>' +
      '<span class="ha-hint">appended, never rewritten · read on poll (haEvents)</span></div><div class="ha-log">';
    var rows = state.events.filter(function (e) { return state.eventsFilter === 'all' || e.sev === state.eventsFilter; });
    if (rows.length === 0) h += '<div class="ha-lrow"><span class="ha-t">—</span><span class="ha-sev info">info</span><span class="ha-msg ha-muted">no events match the filter</span></div>';
    rows.forEach(function (e) {
      h += '<div class="ha-lrow"><span class="ha-t">' + esc(e.t) + '</span><span class="ha-sev ' + e.sev + '">' + e.sev +
        '</span><span class="ha-msg">' + esc(e.msg) + '</span></div>';
    });
    h += '</div></div>';
    p.innerHTML = h;
  }

  function renderInspector() {
    if (!opts.inspector || !hosts.inspector) return;
    var m = null;
    state.modules.forEach(function (x) { if (x.id === state.selectedModule) m = x; });
    if (!m) m = state.modules[0];
    var h = '<h3>Inspector</h3>';
    h += '<div class="ha-kv">' +
      '<span class="k">Selection</span><span class="v">' + esc(m.id) + '</span>' +
      '<span class="k">Profile</span><span class="v">' + m.profile + '</span>' +
      '<span class="k">Owner</span><span class="v">' + esc(m.owner) + '</span>' +
      '<span class="k">OwnerState</span><span class="v ' + (m.ownerState === 'ARMED' ? 'ha-ok' : (m.ownerState === 'CLAIMED_DISARMED' ? 'ha-warn' : 'ha-alarm')) + '">' + m.ownerState + '</span>' +
      '<span class="k">OwnerEpoch</span><span class="v">' + m.ownerEpoch + '</span>' +
      '<span class="k">OwnerOutputSeq</span><span class="v">' + m.outSeq + '</span>' +
      '<span class="k">OwnerPacketAge</span><span class="v">' + m.packetAgeUs + ' µs</span>' +
      '<span class="k">ClaimLatency</span><span class="v">' + f2(m.claim) + ' ms</span>' +
      '<span class="k">MaxQualified</span><span class="v">' + f2(m.claimMax) + ' ms</span></div>';
    h += '<div class="ha-chips" style="margin-top:8px">' + pill('TAKEOVER READY', takeoverReady(standbyUnit() || P2) ? 'yes' : 'no', takeoverReady(standbyUnit() || P2) ? 'ok' : 'warn') + '</div>';
    h += '<p class="ha-note">Per-module T->O status, per ADR-0062. Select a module row in Ownership &amp; Barrier.</p>';
    hosts.inspector.innerHTML = h;
  }

  function renderStatus() {
    if (!hosts.status) return;
    var act = activeUnit();
    var ip = act ? state.plc[act === P1 ? 1 : 2].ip : state.plc[1].ip;
    var h = '';
    h += '<span>Connected: ' + esc(ip) + ' · port 3 (engineering)</span><span class="ha-sep">|</span>';
    h += '<span>MODE ' + (state.mode === 'redundant' ? 'Redundant' : 'Standalone') + '</span><span class="ha-sep">|</span>';
    h += '<span>EPOCH ' + state.epoch + '</span><span class="ha-sep">|</span>';
    h += '<span>PERMIT ' + esc(state.permitOwner || 'WITHHELD') + '</span><span class="ha-sep">|</span>';
    var alarmTxt = alarmsActive() === 0 ? 'none' : String(alarmsActive());
    h += '<span class="' + (alarmsActive() ? 'ha-alarm' : 'ha-ok') + '">ALARMS ' + alarmTxt + '</span><span class="ha-sep">|</span>';
    h += '<span>' + clock() + '</span>';
    hosts.status.innerHTML = h;
  }

  function renderAll() {
    renderTitle();
    renderToolbar();
    renderTree();
    renderOnline();
    renderDemoBar();
    renderTabs();
    renderOverview();
    renderCalibration();
    renderBarrier();
    renderTiming();
    renderEvents();
    renderInspector();
    renderStatus();
    notify(false);
  }

  /* ---------------- actions ---------------- */
  function setBusy(b) { state.busy = b; renderDemoBar(); renderToolbar(); }

  async function manualSwap() {
    if (!can('swap')) {
      addEvent('warn', 'commanded swap refused: preconditions not met (SYNC_READY, calibrated, channels P=1 I=1)');
      renderAll();
      return;
    }
    var act = activeUnit(), stb = standbyUnit();
    setBusy(true);
    addEvent('info', 'haCommandedSwap received — pair SYNC_READY, calibrated');
    await sleep(500);
    addEvent('info', 'RELEASE_OWNER epoch ' + state.epoch + ' (' + act + ')');
    state.modules.forEach(function (m) { m.owner = act; m.ownerState = 'SAFE'; m.armed = false; });
    state.control[act] = 'IDLE';
    state.permitOwner = null;
    state.barrier = { claimed: 0, total: state.modules.length, passed: false };
    renderAll();
    await sleep(650);
    state.control[stb] = 'CLAIMING';
    renderAll();
    for (var i = 0; i < state.modules.length; i++) {
      var m = state.modules[i];
      m.owner = stb; m.ownerState = 'CLAIMED_DISARMED'; m.armed = false; m.ownerEpoch = state.epoch + 1;
      state.barrier.claimed = i + 1;
      addEvent('info', 'claim ' + m.id + ' → ' + stb + ' CLAIMED_DISARMED (' + f2(m.claim) + ' ms, ' + m.profile + ')');
      renderAll();
      await sleep(m.profile === 'reconnect' ? 420 : 130);
    }
    state.barrier.passed = true;
    addEvent('info', 'OWNERSHIP_BARRIER PASSED — all required modules CLAIMED_DISARMED');
    renderAll();
    await sleep(400);
    state.epoch += 1;
    state.modules.forEach(function (m) { m.ownerState = 'ARMED'; m.armed = true; m.ownerEpoch = state.epoch; });
    addEvent('info', 'ARM epoch ' + state.epoch + ' — outputs active under ' + stb);
    addEvent('info', 'COMMIT/ARM epoch ' + state.epoch + ' acknowledged by all modules');
    state.control[stb] = 'ACTIVE';
    state.permitOwner = stb;
    addEvent('info', 'CONTROL: ' + stb + ' ACTIVE; EXECUTION PERMIT → ' + stb);
    var r1 = state.plc[1].role; state.plc[1].role = state.plc[2].role; state.plc[2].role = r1;
    renderAll();
    await sleep(500);
    state.sync[act] = 'deSYNC';
    renderAll();
    await sleep(450);
    state.sync[act] = 'SYNCING';
    renderAll();
    await sleep(650);
    state.sync[act] = 'SYNC_READY';
    addEvent('info', 'old owner rejoined as Secondary — SYNC_READY (no automatic failback)');
    setBusy(false);
    renderAll();
  }

  async function primaryDeath() {
    if (!can('death')) return;
    var act = activeUnit(), stb = standbyUnit();
    if (!act || !stb) return;
    setBusy(true);
    state.dead[act] = true;
    state.deadInfo[act] = 'owner';
    state.online[act] = false;
    state.ownerUnreachable = true;
    state.link.ch1 = 'LOST';
    state.link.ch2 = 'LOST';
    addEvent('alarm', act + ' DEAD — both channels silent (P=false, I=false); expected +1 not arriving');
    state.counters.ch1.missing += 1; state.counters.ch1.penalty += 1000;
    state.counters.ch2.missing += 1; state.counters.ch2.penalty += 1000;
    renderAll();
    await sleep(700);
    addEvent('warn', 'OwnerLease on all modules expiring — owner-age beyond confirmation window (' + f2(state.config.confirmationMs) + ' ms)');
    state.modules.forEach(function (m) { m.ownerState = 'FAILSAFE'; m.armed = false; });
    state.sync[act] = 'deSYNC';
    state.control[act] = 'DEAD';
    renderAll();
    await sleep(700);
    addEvent('alarm', act + ' declared failed — T_peer-detect confirms silence; lease expired (T_claim-start = max(peer-detection, lease expiry))');
    state.control[stb] = 'CLAIMING';
    state.barrier = { claimed: 0, total: state.modules.length, passed: false };
    renderAll();
    await sleep(400);
    for (var i = 0; i < state.modules.length; i++) {
      var m = state.modules[i];
      var ms = m.profile === 'reconnect' ? 8.40 : m.claim;
      m.owner = stb; m.ownerState = 'CLAIMED_DISARMED'; m.ownerEpoch = state.epoch + 1; m.packetAgeUs = 120;
      state.barrier.claimed = i + 1;
      addEvent('info', 'claim ' + m.id + ' → ' + stb + ' CLAIMED_DISARMED (' + f2(ms) + ' ms, ' + m.profile + ')');
      renderAll();
      await sleep(m.profile === 'reconnect' ? 750 : 130);
    }
    state.barrier.passed = true;
    addEvent('info', 'OWNERSHIP_BARRIER PASSED');
    await sleep(350);
    state.epoch += 1;
    state.modules.forEach(function (m) { m.ownerState = 'ARMED'; m.armed = true; m.ownerEpoch = state.epoch; });
    addEvent('info', 'ARM epoch ' + state.epoch + ' — safe state released, outputs active');
    state.control[stb] = 'ACTIVE';
    state.permitOwner = stb;
    addEvent('alarm', 'takeover complete: ' + stb + ' ACTIVE epoch ' + state.epoch + ' — worst recovery ' + f2(budgetCheck().worst) + ' ms (calibrated)');
    setBusy(false);
    renderAll();
  }

  function toggleDegrade() {
    if (state.mode !== 'redundant' || state.busy) return;
    state.degraded = !state.degraded;
    if (state.degraded) {
      state.link.ch2 = 'DEGRADED';
      state.counters.ch2.missing += 1;
      state.counters.ch2.penalty += 1000;
      state.timingLost = !budgetCheck().ok;
      addEvent('warn', 'HA_PERFORMANCE_DEGRADED — channel 2 EMA left the calibrated envelope (baseline 0.30 ms, now 1.18 ms); jitter ×3');
      if (state.timingLost) addEvent('alarm', 'HA_TIMING_GUARANTEE_LOST — worst case ' + f2(budgetCheck().worst) + ' ms exceeds configured budget ' + f2(state.config.budgetMs) + ' ms');
      addEvent('warn', 'one valid link remains — redundancy active, degraded path');
    } else {
      state.link.ch2 = 'OK';
      state.counters.ch2.missing = 0;
      state.counters.ch2.penalty = 0;
      state.timingLost = false;
      addEvent('info', 'channel 2 restored — inside calibrated envelope; budget check QUALIFIED (' + f2(budgetCheck().worst) + ' ms)');
    }
    renderAll();
  }

  function killChannel(ch) {
    if (ch === 'ch1' && !can('kill-p')) return;
    if (ch === 'ch2' && !can('kill-i')) return;
    if (ch === 'ch1') {
      state.link.ch1 = 'LOST';
      state.counters.ch1.missing += 1;
      state.counters.ch1.penalty += 1000;
      addEvent('alarm', 'pair link (P) killed — expected +1 missing on channel P');
    } else {
      state.link.ch2 = 'LOST';
      state.counters.ch2.missing += 1;
      state.counters.ch2.penalty += 1000;
      addEvent('alarm', 'I/O path (I) killed — expected +1 missing on channel I');
    }
    applyTruth();
    renderAll();
  }

  function restoreChannels() {
    if (!can('restore-channels')) return;
    if (state.link.ch1 === 'LOST') { state.link.ch1 = 'OK'; state.counters.ch1.missing = 0; state.counters.ch1.penalty = 0; }
    if (state.link.ch2 === 'LOST' || state.link.ch2 === 'DEGRADED') { state.link.ch2 = 'OK'; state.counters.ch2.missing = 0; state.counters.ch2.penalty = 0; }
    if (!state.redundancyLost) {
      if (ownerAlive()) state.control[activeUnit()] = 'ACTIVE';
      var stb = standbyUnit();
      if (stb && !state.dead[stb] && state.control[stb] === 'REDUNDANCY_LOST') state.control[stb] = 'IDLE';
    }
    addEvent('info', 'channels restored — P=1, I=1' + (state.redundancyLost ? ' (REDUNDANCY_LOST remains until manual repair)' : ''));
    renderAll();
  }

  function applyTruth() {
    var P = state.link.ch1 !== 'LOST';
    var I = state.link.ch2 !== 'LOST';
    if (P && I) return;
    var owner = activeUnit(), stb = standbyUnit();
    if (!owner || state.dead[owner]) return;
    if (!P && I) {
      if (stb && !state.dead[stb]) {
        state.control[stb] = 'REDUNDANCY_LOST';
        addEvent('alarm', 'pair link lost, ' + owner + ' alive via I/O path — REDUNDANCY_LOST on observer; NO auto promotion');
      }
    } else if (P && !I) {
      state.control[owner] = 'ACTIVE_DEGRADED';
      addEvent('alarm', 'I/O path degraded — owner continues ACTIVE_DEGRADED; NO auto promotion');
    } else {
      if (stb && !state.dead[stb]) state.control[stb] = 'IDLE';
      addEvent('warn', 'P=false && I=false with a live owner — promotion path only; claim must be rejected (partition)');
    }
  }

  async function secondaryDeath() {
    if (!can('death-secondary')) return;
    var stb = standbyUnit();
    setBusy(true);
    state.dead[stb] = true;
    state.deadInfo[stb] = 'standby';
    state.online[stb] = false;
    state.sync[stb] = 'deSYNC';
    state.control[stb] = 'DEAD';
    addEvent('alarm', 'secondary ' + stb + ' DEAD — no takeover target; owner continues ACTIVE, pair degraded');
    setBusy(false);
    renderAll();
  }

  async function resurrect(kind) {
    if (kind === 'owner' ? !can('resurrect-primary') : !can('resurrect-secondary')) return;
    var unit = null;
    [P1, P2].forEach(function (u) { if (state.dead[u] && state.deadInfo[u] === kind) unit = u; });
    if (!unit) return;
    setBusy(true);
    state.dead[unit] = false;
    state.online[unit] = true;
    state.deadInfo[unit] = null;
    state.unqualified[unit] = true;
    state.sync[unit] = 'deSYNC';
    state.control[unit] = 'IDLE';
    if (kind === 'owner') {
      state.ownerUnreachable = false;
      state.link.ch1 = 'OK';
      state.link.ch2 = 'OK';
      state.counters.ch1.missing = 0; state.counters.ch1.penalty = 0;
      state.counters.ch2.missing = 0; state.counters.ch2.penalty = 0;
      if (ownerAlive()) state.control[activeUnit()] = 'ACTIVE';
    }
    addEvent('info', unit + ' boots deSYNC — UNQUALIFIED; a live primary exists → rejoins as SECONDARY (zombie rule: never re-enters as Primary)');
    renderAll();
    await sleep(700);
    state.sync[unit] = 'SYNCING';
    renderAll();
    await sleep(900);
    state.sync[unit] = 'SYNC_READY';
    state.unqualified[unit] = false;
    addEvent('info', unit + ' SYNC_READY — monitor mode as Secondary; no automatic failback');
    setBusy(false);
    renderAll();
  }

  async function partitionTakeover() {
    if (!can('partition')) return;
    var owner = activeUnit(), stb = standbyUnit();
    setBusy(true);
    addEvent('warn', 'standby ' + stb + ' sees P=false && I=false — takes the promotion path; owner ' + owner + ' is alive');
    state.control[stb] = 'CLAIMING';
    state.barrier = { claimed: 0, total: state.modules.length, passed: false };
    renderAll();
    await sleep(800);
    addEvent('alarm', 'claim DO-01 REJECTED: OWNERSHIP_CONFLICT — ' + owner + ' holds Exclusive Owner epoch ' + state.epoch);
    addEvent('alarm', 'release-all — the claimant never armed anything; no partial ownership');
    state.modules.forEach(function (m) { m.ownerState = 'SAFE'; m.armed = false; });
    state.barrier = { claimed: 0, total: state.modules.length, passed: false };
    state.redundancyLost = true;
    state.control[owner] = 'REDUNDANCY_LOST';
    state.control[stb] = 'REDUNDANCY_LOST';
    addEvent('alarm', 'REDUNDANCY_LOST — both units dropped ownership; manual repair required (Reset)');
    setBusy(false);
    renderAll();
  }

  async function runCalibration() {
    if (state.busy || state.mode !== 'redundant') return;
    setBusy(true);
    state.calibration = 'CALIBRATING';
    state.admission = 'CALIBRATING';
    state.calProgress = 0;
    addEvent('info', 'commissioning calibration started — both directions under realistic worst load');
    renderAll();
    for (var pct = 0; pct <= 100; pct += 10) {
      state.calProgress = pct;
      renderCalibration();
      renderOnline();
      await sleep(110);
    }
    state.calibration = 'CALIBRATED';
    state.admission = 'ADMITTED';
    state.calBaseline = new Date().toISOString().replace('T', ' ').slice(0, 19);
    state.calTrigger = 'manual run';
    addEvent('info', 'calibration complete — HA link profile accepted (QUALIFIED)');
    setBusy(false);
    renderAll();
  }

  function invalidate(trigger) {
    if (state.busy || state.mode !== 'redundant') return;
    state.calibration = 'UNQUALIFIED';
    state.admission = 'UNQUALIFIED';
    state.calBaseline = '—';
    state.calTrigger = trigger;
    addEvent('warn', 'qualification invalidated: ' + trigger + ' — calibration-gated readiness suspends TakeoverReady');
    renderAll();
  }

  function applyBudget() {
    var confEl = document.getElementById('haConfInp');
    var budEl = document.getElementById('haBudgetInp');
    if (!confEl || !budEl) return;
    var conf = parseFloat(confEl.value);
    var budget = parseFloat(budEl.value);
    if (!isFinite(conf) || conf <= 0 || !isFinite(budget) || budget <= 0) {
      addEvent('warn', 'haSetTimingBudget rejected: invalid values');
      renderAll();
      return;
    }
    state.config.confirmationMs = conf;
    state.config.budgetMs = budget;
    var bc = budgetCheck();
    if (bc.ok) addEvent('info', 'haSetTimingBudget applied — confirmation ' + f2(conf) + ' ms, budget ' + f2(budget) + ' ms; QUALIFIED (worst ' + f2(bc.worst) + ' ms)');
    else addEvent('alarm', 'haSetTimingBudget refused — requirement cannot be guaranteed; minimum demonstrated budget ' + f2(bc.worst) + ' ms');
    renderAll();
  }

  function onPhase(v) {
    var pct = Math.max(0, Math.min(100, Number(v) || 0));
    state.scanPhase = pct / 100;
    var safe = document.getElementById('haSafeVal');
    var pred = document.getElementById('haPredVal');
    var lab = document.getElementById('haPhaseVal');
    if (safe) safe.textContent = f2((1 - state.scanPhase) * state.scan.ema10) + ' ms';
    if (pred) pred.textContent = f2(predictedNow()) + ' ms';
    if (lab) lab.textContent = Math.round(pct) + ' %';
  }

  function setMode(mode) {
    if (state.busy) return;
    state.mode = mode === 'standalone' ? 'standalone' : 'redundant';
    if (state.mode === 'standalone') {
      state.admission = 'STANDALONE';
      addEvent('info', 'standalone admission — permit issued at startup; redundancy statechart not running');
    } else {
      state.admission = state.calibration === 'CALIBRATED' ? 'ADMITTED' : 'UNQUALIFIED';
      addEvent('info', 'redundant mode — ping/pong discovery on port 1, pair configured');
    }
    renderAll();
  }

  function setRole(idx, role) {
    state.plc[idx].role = role === 'Primary' ? 'Primary' : 'Secondary';
    addEvent('info', 'configured role changed: ' + (idx === 1 ? P1 : P2) + ' → ' + state.plc[idx].role);
    renderAll();
  }
  function setIp(idx, ip) {
    state.plc[idx].ip = String(ip || '').trim() || state.plc[idx].ip;
    renderAll();
  }
  function setFilter(sev) { state.eventsFilter = sev; renderEvents(); }
  function tab(name) { state.tab = name; renderTabs(); }
  function tree(id) { state.treeSel = id; if (id.indexOf('mod-') === 0) state.selectedModule = id.slice(4); renderTree(); renderInspector(); }
  function treeKey(ev, id) { if (ev && (ev.key === 'Enter' || ev.key === ' ')) { ev.preventDefault(); tree(id); } }
  function selectModule(id) { state.selectedModule = id; renderInspector(); renderBarrier(); }
  function reset() {
    if (state.busy) return;
    state = freshState();
    seedEvents();
    renderAll();
  }

  /* ---- online editing UX (specs/design/online-editing-ux.md) ----
     Monitoring is the default while connected; the first modification
     attempt opens a confirmation; PENDING_LOCAL is the only state that
     accumulates edits. Offline is free and ungated. */

  var EDIT_LINE = 1;

  function editConnected() { return state.connection === 'connected'; }
  function editCan(action) {
    var mode = state.edit.mode, on = editConnected();
    if (action === 'accept-edits') return on && mode === 'pendingLocal';
    if (action === 'test-edits') return on && mode === 'staged';
    if (action === 'untest-edits') return on && mode === 'testing';
    if (action === 'assemble-edits') return on && (mode === 'staged' || mode === 'testing');
    if (action === 'cancel-edits') return on && mode !== 'monitoring';
    if (action === 'discard-local') return mode === 'pendingLocal';
    if (action === 'start-pending') return on && mode === 'monitoring';
    if (action === 'try-edit') return mode === 'monitoring' || mode === 'pendingLocal';
    if (action === 'type-char' || action === 'backspace-edit') return mode === 'monitoring' || mode === 'pendingLocal' || !on;
    return false;
  }
  /** The net line diff of the shadow buffer against the online baseline. */
  function computeDiff() {
    var e = state.edit, diff = [];
    var len = Math.max(e.shadow.length, e.baseline.length);
    for (var i = 0; i < len; i++) {
      var before = e.baseline[i] === undefined ? '' : e.baseline[i];
      var after = e.shadow[i] === undefined ? '' : e.shadow[i];
      if (before !== after) diff.push({ line: i, from: before, to: after });
    }
    return diff;
  }
  function diffEmpty() { return computeDiff().length === 0; }
  function resetShadow() {
    state.edit.shadow = state.edit.baseline.slice();
    state.edit.keystrokes = 0;
  }
  /** Simulated keystroke. While connected it lands in the shadow buffer only and never prompts. */
  function typeChar() {
    if (!editCan('type-char')) return;
    state.edit.keystrokes += 1;
    if (!editConnected() || state.edit.mode === 'pendingLocal') {
      state.edit.code[EDIT_LINE] = state.edit.code[EDIT_LINE] === 'Counter := Counter + 1;'
        ? 'Counter := Counter + 10;' : 'Counter := Counter + 1;';
      addEvent('info', editConnected() ? 'keystroke applied to local edits' : 'keystroke applied — offline, free editing');
      renderAll();
      return;
    }
    state.edit.shadow[EDIT_LINE] = 'Counter := Counter + 10;';
    state.edit.confirm = null;
    addEvent('info', 'keystroke captured in the shadow buffer (controller untouched)');
    renderAll();
  }
  /** Simulated backspace: reverts the shadow buffer toward the online baseline. */
  function backspaceEdit() {
    if (!editCan('backspace-edit')) return;
    if (!editConnected() || state.edit.mode === 'pendingLocal') {
      state.edit.code[EDIT_LINE] = 'Counter := Counter + 1;';
      addEvent('info', 'backspace applied');
      renderAll();
      return;
    }
    resetShadow();
    state.edit.confirm = null;
    addEvent('info', 'backspace — shadow buffer is identical to the online baseline');
    renderAll();
  }
  /** The commit gesture: evaluate the NET diff; prompt only when it is non-empty. */
  function attemptEdit() {
    if (!editCan('try-edit')) return;
    if (!editConnected() || state.edit.mode === 'pendingLocal') return;
    if (diffEmpty()) {
      addEvent('info', 'modification attempt produced no net change — no confirmation');
      renderAll();
      return;
    }
    state.edit.confirm = { diff: computeDiff() };
    addEvent('info', 'net diff detected — confirmation required while connected');
    renderAll();
  }
  function confirmEdit() {
    if (!state.edit.confirm) return;
    state.edit.mode = 'pendingLocal';
    state.edit.code = state.edit.shadow.slice();
    state.edit.changed = computeDiff().map(function (d) { return d.line; });
    state.edit.confirm = null;
    addEvent('warn', 'PENDING LOCAL — edits are local; controller unchanged until Accept');
    renderAll();
  }
  function denyEdit() {
    if (!state.edit.confirm) return;
    resetShadow();
    state.edit.confirm = null;
    addEvent('info', 'edit discarded (No) — shadow buffer dropped; monitoring unchanged');
    renderAll();
  }
  function startPending() {
    if (!editCan('start-pending')) return;
    state.edit.mode = 'pendingLocal';
    state.edit.code = state.edit.shadow.slice();
    state.edit.changed = computeDiff().map(function (d) { return d.line; });
    state.edit.confirm = null;
    addEvent('warn', 'PENDING LOCAL — edits are local; controller unchanged until Accept');
    renderAll();
  }
  function discardLocal() {
    if (!editCan('discard-local')) return;
    revertLocalChange();
    state.edit.mode = 'monitoring';
    addEvent('info', 'local edits discarded — back to monitoring');
    renderAll();
  }
  /** Drops every local artifact and restores the online baseline (code, shadow, markers). */
  function revertLocalChange() {
    state.edit.code = state.edit.baseline.slice();
    state.edit.changed = [];
    resetShadow();
  }
  function acceptEdits() {
    if (!editCan('accept-edits')) return;
    state.edit.mode = 'staged';
    addEvent('info', 'candidate STAGED — validated; controller still runs generation ' + state.edit.generation);
    renderAll();
  }
  function testEdits() {
    if (!editCan('test-edits')) return;
    state.edit.mode = 'testing';
    addEvent('info', 'TESTING — candidate executes; exits: untest / cancel / assemble');
    renderAll();
  }
  function untestEdits() {
    if (!editCan('untest-edits')) return;
    state.edit.mode = 'staged';
    addEvent('info', 'untested — candidate staged, original logic active');
    renderAll();
  }
  function assembleEdits() {
    if (!editCan('assemble-edits')) return;
    state.edit.mode = 'monitoring';
    state.edit.generation += 1;
    state.edit.changed = [];
    state.edit.baseline = state.edit.code.slice();
    resetShadow();
    addEvent('info', 'CLEAN — assembled; controller runs generation ' + state.edit.generation);
    renderAll();
  }
  function cancelEdits() {
    if (!editCan('cancel-edits')) return;
    revertLocalChange();
    state.edit.mode = 'monitoring';
    addEvent('info', 'cancelled — project re-synced from the device; monitoring');
    renderAll();
  }
  function setConnection(on) {
    if (on === editConnected()) return;
    state.connection = on ? 'connected' : 'offline';
    if (on) {
      state.edit.mode = 'monitoring';
      revertLocalChange();
      addEvent('info', 'connected — MONITORING (read-only, live values)');
    }
    else {
      addEvent('warn', 'disconnected — OFFLINE, free editing (no confirmations)');
    }
    renderAll();
  }
  function editBanner() {
    var e = state.edit;
    if (!editConnected()) return { text: 'OFFLINE — free editing; no controller connected', tone: 'warn' };
    if (e.mode === 'monitoring') return { text: 'MONITORING — read-only; controller runs generation ' + e.generation, tone: 'ok' };
    if (e.mode === 'pendingLocal') return { text: 'PENDING LOCAL — edits are local; controller unchanged until Accept', tone: 'warn' };
    if (e.mode === 'staged') return { text: 'STAGED — candidate validated; controller still runs generation ' + e.generation, tone: 'warn' };
    return { text: 'TESTING — candidate executes; untest / cancel / assemble', tone: 'warn' };
  }
  function editBtn(label, action, enabled, variant) {
    return '<button class="ha-btn ' + (variant || '') + '" ' + (enabled ? '' : 'disabled') +
      ' onclick="HA.act(\'' + action + '\')">' + esc(label) + '</button>';
  }
  function editBannerHtml() {
    var b = editBanner();
    return '<div class="' + (b.tone === 'ok' ? 'ha-ok' : 'ha-warn') + '" style="border:1px solid currentColor;border-radius:4px;padding:6px 10px;margin:4px 0;font-weight:600">' +
      esc(b.text) + '</div>';
  }
  function editConfirmHtml() {
    var c = state.edit.confirm;
    if (!c) return '';
    var diff = c.diff.map(function (d) {
      return '<div style="font-family:Consolas,monospace;font-size:12px">' +
        '<div class="ha-alarm">- ' + esc(d.from) + '</div><div class="ha-ok">+ ' + esc(d.to) + '</div></div>';
    }).join('');
    return '<div class="ha-warn" style="border:1px solid currentColor;border-left:5px solid currentColor;border-radius:4px;padding:10px;margin:6px 0">' +
      '<b>Enter code-change mode?</b> Changes stay local (Pending Local) until Accept.' +
      (diff ? '<div style="margin:6px 0">' + diff + '</div>' : '') +
      '<div class="ha-row">' + editBtn('Yes — edit locally', 'confirm-edit', true, 'primary') +
      editBtn('No — stay in monitoring', 'deny-edit', true) + '</div></div>';
  }
  function editCodeHtml() {
    var e = state.edit;
    return '<div style="border:1px solid rgba(127,127,127,.4);border-radius:4px;padding:6px;background:rgba(127,127,127,.06)">' +
      e.code.map(function (text, i) {
        var changed = e.changed.indexOf(i) >= 0;
        var shadowed = e.shadow[i] !== e.baseline[i];
        return '<div onclick="HA.act(\'try-edit\')" title="click to simulate committing a modification attempt (the net diff is evaluated)" ' +
          'style="display:flex;gap:8px;font-family:Consolas,monospace;font-size:12px;white-space:pre;cursor:text">' +
          '<span style="width:24px;text-align:right;opacity:.55">' + (i + 1) + '</span>' +
          '<span style="width:14px" class="' + (changed || shadowed ? 'ha-warn' : '') + '">' + (changed || shadowed ? '\u258C' : '') + '</span>' +
          '<span>' + esc(text) + '</span></div>';
      }).join('') +
      '<div class="ha-hint" style="margin-top:4px">shadow buffer vs online baseline: ' +
      (diffEmpty() ? 'identical (no prompt on commit)' : computeDiff().length + ' changed line(s) — committing prompts') + '</div></div>';
  }
  function editLiveHtml() {
    var e = state.edit;
    return '<b>Live values</b>' + fragTable(['variable', 'value'], [
      { cells: ['Counter', e.live.Counter.toLocaleString()] },
      { cells: ['Temperature', e.live.Temperature.toFixed(1) + ' \u00B0C'] },
      { cells: ['Running', e.live.Running ? 'TRUE' : 'FALSE'] }
    ]);
  }
  function editControlsHtml() {
    var c = dataEdit().can;
    return '<div class="ha-row" style="margin-top:6px">' +
      (editConnected() ? editBtn('Disconnect', 'disconnect', c.disconnect) : editBtn('Connect', 'connect', c.connect, 'primary')) +
      editBtn('Type char (shadow)', 'type-char', c.typeChar) +
      editBtn('Backspace (shadow)', 'backspace-edit', c.backspace) +
      editBtn('Commit edit attempt', 'try-edit', c.tryEdit) +
      editBtn('Start Pending Edits', 'start-pending', c.startPending) +
      editBtn('Accept', 'accept-edits', c.accept, 'primary') +
      editBtn('Test', 'test-edits', c.test) +
      editBtn('Untest', 'untest-edits', c.untest) +
      editBtn('Assemble', 'assemble-edits', c.assemble, 'primary') +
      editBtn('Cancel', 'cancel-edits', c.cancel) +
      editBtn('Discard local', 'discard-local', c.discard, 'warn') +
      '</div>';
  }
  function dataEdit() {
    var e = state.edit;
    return {
      connection: state.connection,
      mode: e.mode,
      generation: e.generation,
      banner: editBanner(),
      code: e.code.map(function (text, i) { return { n: i + 1, text: text, changed: e.changed.indexOf(i) >= 0 }; }),
      shadow: e.shadow.map(function (text, i) { return { n: i + 1, text: text, changed: e.shadow[i] !== e.baseline[i] }; }),
      diff: computeDiff(),
      keystrokes: e.keystrokes,
      live: e.live,
      confirm: e.confirm,
      can: {
        connect: !editConnected(),
        disconnect: editConnected(),
        typeChar: editCan('type-char'),
        backspace: editCan('backspace-edit'),
        tryEdit: editCan('try-edit'),
        startPending: editCan('start-pending'),
        accept: editCan('accept-edits'),
        test: editCan('test-edits'),
        untest: editCan('untest-edits'),
        assemble: editCan('assemble-edits'),
        cancel: editCan('cancel-edits'),
        discard: editCan('discard-local')
      }
    };
  }
  /** One self-contained section: banner + confirmation + code view + live values + controls. */
  function editSectionHtml() {
    return editBannerHtml() + editConfirmHtml() +
      '<div style="display:grid;grid-template-columns:1fr 230px;gap:10px;align-items:start">' +
      editCodeHtml() + '<div>' + editLiveHtml() + '</div></div>' + editControlsHtml();
  }
  function editStatusText() {
    if (!editConnected()) return 'EDIT OFFLINE';
    var mode = state.edit.mode;
    var label = mode === 'monitoring' ? 'MONITORING' : (mode === 'pendingLocal' ? 'PENDING LOCAL' : mode.toUpperCase());
    return 'EDIT ' + label + ' gen ' + state.edit.generation;
  }

  function act(name) {
    if (name === 'swap') return manualSwap();
    if (name === 'death') return primaryDeath();
    if (name === 'death-secondary') return secondaryDeath();
    if (name === 'resurrect-primary') return resurrect('owner');
    if (name === 'resurrect-secondary') return resurrect('standby');
    if (name === 'kill-p') return killChannel('ch1');
    if (name === 'kill-i') return killChannel('ch2');
    if (name === 'restore-channels') return restoreChannels();
    if (name === 'partition') return partitionTakeover();
    if (name === 'degrade') return toggleDegrade();
    if (name === 'calibrate') return runCalibration();
    if (name === 'reset') return reset();
    if (name === 'invalidate-nic') return invalidate('NIC replaced');
    if (name === 'invalidate-topo') return invalidate('topology changed');
    if (name === 'apply-budget') return applyBudget();
    if (name === 'connect') return setConnection(true);
    if (name === 'disconnect') return setConnection(false);
    if (name === 'try-edit') return attemptEdit();
    if (name === 'type-char') return typeChar();
    if (name === 'backspace-edit') return backspaceEdit();
    if (name === 'confirm-edit') return confirmEdit();
    if (name === 'deny-edit') return denyEdit();
    if (name === 'start-pending') return startPending();
    if (name === 'accept-edits') return acceptEdits();
    if (name === 'test-edits') return testEdits();
    if (name === 'untest-edits') return untestEdits();
    if (name === 'assemble-edits') return assembleEdits();
    if (name === 'cancel-edits') return cancelEdits();
    if (name === 'discard-local') return discardLocal();
    if (name === 'tab-events') return tab('events');
    if (name === 'tab-timing') return tab('timing');
    if (name === 'noop') return;
  }

  /* ---------------- composition API (data + fragments) ---------------- */

  function unitData(unit, idx) {
    var dead = state.dead[unit];
    var on = state.online[unit] && !dead;
    var ctl = dead ? 'DEAD' : (!on ? 'OFFLINE' : state.control[unit]);
    if (on && state.degraded && ctl === 'ACTIVE') ctl = 'ACTIVE_DEGRADED';
    return {
      name: unit,
      role: state.plc[idx].role,
      ip: state.plc[idx].ip,
      online: on,
      dead: dead,
      unqualified: state.unqualified[unit],
      control: ctl,
      permit: state.permitOwner === unit,
      takeoverReady: takeoverReady(unit),
      sync: state.sync[unit]
    };
  }

  function dataOverview() {
    var stb = standbyUnit() || P2;
    return {
      mode: state.mode,
      pairId: 'PAIR-RB-0417',
      epoch: state.epoch,
      appGen: state.appGen,
      stateGen: state.stateGen,
      calibration: state.calibration,
      baseline: state.calBaseline,
      trigger: state.calTrigger,
      units: [unitData(P1, 1), unitData(P2, 2)],
      trInputs: [
        { label: 'SYNC_READY (standby)', ok: state.sync[stb] === 'SYNC_READY' },
        { label: 'IO_READY', ok: ioReady() },
        { label: 'RedundancyLinkValid', ok: linkValid() },
        { label: 'Calibration valid', ok: state.calibration === 'CALIBRATED' }
      ],
      takeoverReady: takeoverReady(stb),
      counters: state.counters,
      link: state.link,
      permit: state.permitOwner,
      admission: state.admission,
      truth: {
        P: state.link.ch1 !== 'LOST',
        I: state.link.ch2 !== 'LOST',
        owner: activeUnit(),
        standby: standbyUnit(),
        ownerUnreachable: state.ownerUnreachable,
        redundancyLost: state.redundancyLost,
        dead: { p1: state.dead[P1], p2: state.dead[P2] }
      }
    };
  }

  function dataCalibration() {
    var deg = state.degraded;
    var ch2 = deg
      ? { cur: 1.42, e10: 1.18, e100: 0.62, max: 2.06, count: 120198, loss: 0.0021, consec: 1 }
      : { cur: 0.33, e10: 0.34, e100: 0.31, max: 0.88, count: 120304, loss: 0.00003, consec: 0 };
    return {
      degraded: deg,
      directions: [
        { dir: 'A→B→A', cur: 0.31, e10: 0.34, e100: 0.30, max: 0.82, count: 120304 },
        { dir: 'B→A→B', cur: ch2.cur, e10: ch2.e10, e100: ch2.e100, max: ch2.max, count: ch2.count }
      ],
      jitter: deg ? 0.54 : 0.18,
      loss: ch2.loss,
      consec: ch2.consec,
      sides: [
        { side: 'PLC-1 processing', v: 0.12 },
        { side: 'PLC-2 processing', v: deg ? 0.19 : 0.11 },
        { side: 'Scan A EMA10 / max / jitter', v: state.scan.ema10, max: state.scan.max, jitter: deg ? state.scan.jitter * 3 : state.scan.jitter },
        { side: 'Scan B EMA10 / max / jitter', v: 7.6, max: 12.1, jitter: 0.7 }
      ],
      cal: {
        state: state.calibration, progress: state.calProgress,
        baseline: state.calBaseline, trigger: state.calTrigger
      }
    };
  }

  function dataBarrier() {
    var seqEma = 0, seqMax = 0, limiting = null;
    state.modules.forEach(function (m) {
      seqEma += m.claim;
      seqMax += m.claimMax;
      if (!limiting || m.claimMax > limiting.claimMax) limiting = m;
    });
    return {
      modules: state.modules.map(function (m) {
        return {
          id: m.id, profile: m.profile, owner: m.owner, ownerEpoch: m.ownerEpoch,
          ownerState: m.ownerState, armed: m.armed, claim: m.claim, claimMax: m.claimMax,
          packetAgeUs: m.packetAgeUs, outSeq: m.outSeq, limiting: m.id === limiting.id
        };
      }),
      seqEma: seqEma, seqMax: seqMax, limitingId: limiting.id,
      claimed: state.barrier.claimed, total: state.barrier.total, passed: state.barrier.passed,
      claimMaxNow: claimMaxNow(), claimEmaNow: claimEmaNow(),
      outputMaxNow: outputMaxNow(), outputEmaNow: outputEmaNow(),
      selection: state.selection.slice()
    };
  }

  function dataTiming() {
    var bc = budgetCheck();
    return {
      terms: [
        { name: 'T_peer-detect', measured: state.config.confirmationMs, bound: state.degraded ? 3.50 : 1.00, note: 'confirmation time (configured)' },
        { name: 'T_claim (Σ modules)', measured: claimEmaNow(), bound: claimMaxNow(), note: 'sequential v1' },
        { name: 'T_arm', measured: 0.42, bound: 0.90, note: 'ordered ARM phase' },
        { name: 'T_scan-safe-point', measured: (1 - state.scanPhase) * state.scan.ema10, bound: state.scan.max, note: 'phase-aware: T_next-commit − t_takeover' },
        { name: 'T_output-apply', measured: outputEmaNow(), bound: outputMaxNow(), note: 'commit to physical' }
      ],
      confirmation: state.config.confirmationMs,
      budget: state.config.budgetMs,
      worst: bc.worst,
      ok: bc.ok,
      predicted: predictedNow(),
      scanPhase: state.scanPhase,
      scan: { ema10: state.scan.ema10, ema100: state.scan.ema100, max: state.scan.max, jitter: state.degraded ? state.scan.jitter * 3 : state.scan.jitter },

      safepoint: (1 - state.scanPhase) * state.scan.ema10
    };
  }

  function dataEvents(filter) {
    var f = filter || 'all';
    return state.events.filter(function (e) { return f === 'all' || e.sev === f; });
  }

  function fragKv(rows) {
    return '<div class="ha-kv">' + rows.map(function (r) {
      return '<span class="k">' + esc(r[0]) + '</span><span class="v ' + (r[2] || '') + '">' + r[1] + '</span>';
    }).join('') + '</div>';
  }

  function fragTable(headers, rows) {
    var h = '<table class="ha-table"><thead><tr>' + headers.map(function (t) { return '<th>' + t + '</th>'; }).join('') + '</tr></thead><tbody>';
    rows.forEach(function (r) {
      h += '<tr class="' + (r.cls || '') + '" ' + (r.attrs || '') + '>';
      r.cells.forEach(function (c, i) {
        h += '<td class="' + (i > 0 ? 'ha-num' : '') + ' ' + (r.cellCls && r.cellCls[i] ? r.cellCls[i] : '') + '">' + c + '</td>';
      });
      h += '</tr>';
    });
    return h + '</tbody></table>';
  }

  function selectionModules() {
    return state.modules.filter(function (m) { return state.selection.indexOf(m.id) >= 0; });
  }
  function toggleSelect(id) {
    var i = state.selection.indexOf(id);
    if (i >= 0) state.selection.splice(i, 1); else state.selection.push(id);
    notify();
  }
  function selectAllModules(all) {
    state.selection = all ? state.modules.map(function (m) { return m.id; }) : [];
    notify();
  }
  function recomputeBarrier() {
    var claimed = 0, blocked = false;
    state.modules.forEach(function (m) {
      if (m.ownerState === 'CLAIMED_DISARMED' || m.ownerState === 'ARMED') claimed++;
      else blocked = true;
    });
    state.barrier.claimed = claimed;
    state.barrier.passed = claimed === state.modules.length && !blocked;
  }
  function bulk(action) {
    if (state.mode !== 'redundant' || state.busy) return;
    var sel = selectionModules();
    if (sel.length === 0) {
      addEvent('warn', 'bulk ' + action + ' refused: no modules selected');
      renderAll();
      return;
    }
    if (action === 'claim') {
      var owner = state.permitOwner || activeUnit();
      if (!owner) { addEvent('warn', 'bulk claim refused: no permit holder'); renderAll(); return; }
      sel.forEach(function (m) { m.owner = owner; m.ownerState = 'CLAIMED_DISARMED'; m.armed = false; m.ownerEpoch = state.epoch + 1; });
      addEvent('info', 'bulk claim: ' + sel.length + ' module(s) → ' + owner + ' CLAIMED_DISARMED (no ARM)');
    } else if (action === 'release') {
      sel.forEach(function (m) { m.ownerState = 'SAFE'; m.armed = false; });
      addEvent('warn', 'bulk release: ' + sel.length + ' module(s) → transition state (never ARM on release)');
    } else if (action === 'arm') {
      sel.forEach(function (m) { if (m.ownerState === 'CLAIMED_DISARMED') { m.ownerState = 'ARMED'; m.armed = true; m.ownerEpoch = state.epoch; } });
      addEvent('info', 'bulk ARM: ' + sel.length + ' module(s) under epoch ' + state.epoch);
    } else if (action === 'calibrate') {
      addEvent('info', 'per-module claim-latency samples requested for ' + sel.length + ' module(s); EMA trackers updated');
    }
    recomputeBarrier();
    renderAll();
  }

  function subscribe(fn) { subs.push(fn); }
  function getState() { return state; }

  function mount(config) {
    opts = config || opts;
    hosts = {
      title: document.getElementById('haTitle'),
      toolbar: document.getElementById('haToolbar'),
      tree: document.getElementById('haTree'),
      online: document.getElementById('haOnline'),
      demo: document.getElementById('haDemo'),
      tabs: document.getElementById('haTabs'),
      panels: document.getElementById('haPanels'),
      panelOverview: document.getElementById('panel-overview'),
      panelCalibration: document.getElementById('panel-calibration'),
      panelBarrier: document.getElementById('panel-barrier'),
      panelTiming: document.getElementById('panel-timing'),
      panelEvents: document.getElementById('panel-events'),
      inspector: document.getElementById('haInspector'),
      status: document.getElementById('haStatus')
    };
    seedEvents();
    renderAll();
    setInterval(function () {
      if (state.mode === 'redundant' && !state.busy) {
        if (state.link.ch1 !== 'LOST') { state.counters.ch1.ping += 1; state.counters.ch1.pong += 1; }
        if (state.link.ch2 !== 'LOST') { state.counters.ch2.ping += 1; state.counters.ch2.pong += 1; }
      }
      state.edit.live.Counter += 1;
      if (state.edit.live.Counter > 100) state.edit.live.Counter = 0;
      state.edit.live.Temperature = 70 + (state.edit.live.Counter % 20);
      renderStatus();
      notify(true);
    }, 1000);
  }

  global.HA = {
    mount: mount,
    act: act,
    tab: tab,
    tree: tree,
    treeKey: treeKey,
    selectModule: selectModule,
    setMode: setMode,
    setRole: setRole,
    setIp: setIp,
    setFilter: setFilter,
    onPhase: onPhase,
    subscribe: subscribe,
    getState: getState,
    data: {
      overview: dataOverview,
      calibration: dataCalibration,
      barrier: dataBarrier,
      timing: dataTiming,
      events: dataEvents
    },
    frag: {
      pill: pill,
      chip: chip,
      syncChips: syncChips,
      controlChips: controlChips,
      kv: fragKv,
      table: fragTable
    },
    derive: {
      activeUnit: activeUnit,
      standbyUnit: standbyUnit,
      linkValid: linkValid,
      ioReady: ioReady,
      channelsOk: channelsOk,
      ownerAlive: ownerAlive,
      standbyAlive: standbyAlive,
      takeoverReady: takeoverReady,
      budgetCheck: budgetCheck,
      predictedNow: predictedNow,
      alarmsActive: alarmsActive,
      can: can
    },
    select: {
      toggle: toggleSelect,
      all: selectAllModules,
      modules: selectionModules
    },
    bulk: bulk,
    edit: {
      data: dataEdit,
      sectionHtml: editSectionHtml,
      bannerHtml: editBannerHtml,
      confirmHtml: editConfirmHtml,
      codeHtml: editCodeHtml,
      liveHtml: editLiveHtml,
      controlsHtml: editControlsHtml,
      statusText: editStatusText,
      connected: editConnected,
      can: editCan
    },
    esc: esc,
    f2: f2
  };
})(window);
