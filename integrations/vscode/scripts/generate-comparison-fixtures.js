#!/usr/bin/env node
// Regenerates src/test/fixtures/online-comparison/*.json: the output of the real
// `ironplcc online-compare` for one controller and project per case, so the
// mirror in src/onlineComparisonLogic.ts is tested against what the compiler
// prints and not against a hand-written copy. Each case is a project state, a
// download record or not, and the identity a controller answers.
//
//   node scripts/generate-comparison-fixtures.js <path to ironplcc>
//
// The cases cover each headline row and each information row of ADR-0089.

'use strict';

const { execFileSync } = require('child_process');
const fs = require('fs');
const os = require('os');
const path = require('path');

const ironplcc = process.argv[2];
if (!ironplcc) {
  console.error('usage: node scripts/generate-comparison-fixtures.js <path to ironplcc>');
  process.exit(2);
}

const OUT = path.join(__dirname, '..', 'src', 'test', 'fixtures', 'online-comparison');
const DEVICE = '0123456789abcdef0123456789abcdef';
const OTHER_DEVICE = 'fedcba9876543210fedcba9876543210';
const EMPTY_APPLICATION_HASH = 'f363b8d33c67e163acb0b0a669a6d9ee5b3f81e13b59858a9234a85008dcb1c4';
const OTHER_HASH = 'ee'.repeat(32);

const program = value => `PROGRAM main\nVAR y : INT; END_VAR\n  y := ${value};\nEND_PROGRAM\n`;

function run(args, options = {}) {
  return execFileSync(ironplcc, args, { encoding: 'utf-8', stdio: ['pipe', 'pipe', 'pipe'], ...options });
}

/** A project in a fresh directory: one source and the application it builds to. */
class Project {
  constructor() {
    this.dir = fs.mkdtempSync(path.join(os.tmpdir(), 'ironplc-comparison-'));
    this.source = path.join(this.dir, 'main.st');
    this.application = path.join(this.dir, 'app.iplc');
    this.write(program(1));
  }

  write(text) {
    fs.writeFileSync(this.source, text);
  }

  compile() {
    run(['compile', this.source, '-o', this.application]);
  }

  /** The content and layout hash of the last build. */
  built() {
    const record = JSON.parse(fs.readFileSync(path.join(this.dir, 'app.build.iplcinfo'), 'utf-8'));
    return { content: record.container.contentHash, layout: record.container.layoutHash };
  }

  recordDownload(device) {
    run(['record-download', this.application, '--device-id', device]);
  }

  recordPath(device) {
    run(['record-path', this.application, '--device-id', device, '--address', '10.0.0.5', '--port', '49152']);
  }

  compare(attempt, identity) {
    const args = ['online-compare', this.source, '-o', this.application, '--attempt', attempt];
    if (identity !== undefined) {
      args.push('--identity', '-');
    }
    return JSON.parse(run(args, identity === undefined ? {} : { input: JSON.stringify(identity) }));
  }
}

function identity(device, content, layout, extra = {}) {
  const { top = {}, application = {}, deviceFields = {} } = extra;
  return {
    protocol: 2,
    device: { deviceId: device, name: 'press-1', model: 'IronPLC SoftPLC', modification: 'vm-cli', runtimeVersion: '0.247.0', ...deviceFields },
    application: {
      mode: 'normal', active: 4, normal: 4, candidate: null, application: 4, migration: false, rounds: 99,
      contentHash: content, layoutHash: layout, ...application,
    },
    runState: 'running',
    containerFormat: 9,
    ...top,
  };
}

function editView(stage, acceptReason) {
  return {
    stage,
    commands: {
      accept: acceptReason === undefined
        ? { enabled: true, label: 'Accept' }
        : { enabled: false, label: 'Accept', reason: acceptReason },
    },
  };
}

/** Each case: a name, and a function that sets up its project and returns the comparison. */
const cases = {
  // Rows 1, 2, 3 and 13: the attempt did not reach the controller.
  unreachable: (p) => { p.compile(); return p.compare('unreachable'); },
  busy: (p) => { p.compile(); return p.compare('busy'); },
  protected: (p) => { p.compile(); return p.compare('protected'); },
  'connection-lost': (p) => { p.compile(); return p.compare('connection-lost'); },
  // Row 4: the controller holds the empty application.
  'no-application': (p) => { p.compile(); return p.compare('ok', identity(DEVICE, EMPTY_APPLICATION_HASH, OTHER_HASH)); },
  // Row 5: C = P and the sources are the build's.
  'up-to-date': (p) => { p.compile(); const b = p.built(); return p.compare('ok', identity(DEVICE, b.content, b.layout)); },
  // Row 6: C = P and the sources changed.
  'sources-changed': (p) => { p.compile(); const b = p.built(); p.write(program(2)); return p.compare('ok', identity(DEVICE, b.content, b.layout)); },
  // Row 7: C = R, C is not P, the layout is the project's.
  'previous-download': (p) => {
    p.compile(); const sent = p.built(); p.recordDownload(DEVICE);
    p.write(program(2)); p.compile();
    return p.compare('ok', identity(DEVICE, sent.content, sent.layout));
  },
  // Row 8: C = R, C is not P, the layout is not the project's.
  'previous-download-other-layout': (p) => {
    p.compile(); const sent = p.built(); p.recordDownload(DEVICE);
    p.write(program(2)); p.compile();
    return p.compare('ok', identity(DEVICE, sent.content, 'dd'.repeat(32)));
  },
  // Row 9: C is neither P nor R, with and without a download on record.
  'other-application': (p) => { p.compile(); p.recordDownload(DEVICE); return p.compare('ok', identity(DEVICE, OTHER_HASH, OTHER_HASH)); },
  'other-application-no-record': (p) => { p.compile(); return p.compare('ok', identity(DEVICE, OTHER_HASH, OTHER_HASH)); },
  // Row 10: an edit is unfinished on the controller.
  'edit-in-progress': (p) => {
    p.compile(); const b = p.built();
    return p.compare('ok', identity(DEVICE, b.content, b.layout, {
      application: { edit: editView('testing', { code: 'V4004', text: 'a candidate is staged' }) },
    }));
  },
  // Row 11: the project does not build.
  'build-fails': (p) => {
    p.compile(); const b = p.built(); p.write('PROGRAM main\nVAR y : INT; END_VAR\n  y := ;\nEND_PROGRAM\n');
    return p.compare('ok', identity(DEVICE, b.content, b.layout));
  },
  // Row 12: the controller is not the one last chosen.
  'other-controller': (p) => { p.compile(); const b = p.built(); p.recordPath(OTHER_DEVICE); return p.compare('ok', identity(DEVICE, b.content, b.layout)); },
  // Row 14: the controller restarted.
  'not-persisted': (p) => {
    p.compile(); const b = p.built();
    return p.compare('ok', identity(DEVICE, b.content, b.layout, { top: { persistedContentHash: OTHER_HASH } }));
  },
  'candidate-lost': (p) => {
    p.compile(); const b = p.built();
    return p.compare('ok', identity(DEVICE, b.content, b.layout, { top: { candidateLost: { name: 'tune', stage: 'testing' } } }));
  },
  'retained-discarded': (p) => {
    p.compile(); const b = p.built();
    return p.compare('ok', identity(DEVICE, b.content, b.layout, { top: { retained: { result: 'discarded', reason: 'damaged' } } }));
  },
  // Row 15: the controller refuses the container format of this compiler.
  'container-format-refused': (p) => {
    p.compile(); const b = p.built();
    return p.compare('ok', identity(DEVICE, b.content, b.layout, { top: { containerFormat: 1 } }));
  },
  // The conditions the table adds.
  stopped: (p) => { p.compile(); const b = p.built(); return p.compare('ok', identity(DEVICE, b.content, b.layout, { top: { runState: 'stopped' } })); },
  faulted: (p) => {
    p.compile(); const b = p.built();
    return p.compare('ok', identity(DEVICE, b.content, b.layout, { top: { fault: { vCode: 'V4101', message: 'division by zero', taskId: 0, instanceId: 0 } } }));
  },
  'no-device-id': (p) => {
    p.compile(); const b = p.built();
    const reported = identity(DEVICE, b.content, b.layout);
    delete reported.device.deviceId;
    return p.compare('ok', reported);
  },
  'content-not-reported': (p) => {
    p.compile(); const b = p.built();
    const reported = identity(DEVICE, b.content, b.layout);
    delete reported.application.contentHash;
    delete reported.application.layoutHash;
    return p.compare('ok', reported);
  },
  'never-built': (p) => p.compare('ok', identity(DEVICE, OTHER_HASH, OTHER_HASH)),
};

fs.mkdirSync(OUT, { recursive: true });
for (const [name, make] of Object.entries(cases)) {
  const project = new Project();
  try {
    const comparison = make(project);
    if (comparison.lastDownload !== null) {
      // The time of the record is the one thing that differs between two runs.
      comparison.lastDownload.time = '2026-01-01T00:00:00Z';
    }
    fs.writeFileSync(path.join(OUT, `${name}.json`), `${JSON.stringify(comparison, null, 2)}\n`);
    console.log(`${name}: ${comparison.row}`);
  }
  finally {
    fs.rmSync(project.dir, { recursive: true, force: true });
  }
}
