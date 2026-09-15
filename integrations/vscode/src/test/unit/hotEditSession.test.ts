import * as assert from 'assert';
import {
  displayMode,
  encodeRequest,
  formatStatusDetail,
  formatStatusText,
  HotEditProtocolError,
  HotEditSession,
  HotEditStatus,
  HotEditTransport,
  parseResponseLine,
} from '../../hotEditSession';

/** An in-memory transport: tests feed lines in and record the lines sent out. */
class MockTransport implements HotEditTransport {
  readonly sent: string[] = [];
  private readonly lineListeners: ((line: string) => void)[] = [];
  private readonly exitListeners: (() => void)[] = [];

  sendLine(line: string): void {
    this.sent.push(line);
  }

  onLine(listener: (line: string) => void): void {
    this.lineListeners.push(listener);
  }

  onExit(listener: () => void): void {
    this.exitListeners.push(listener);
  }

  emitLine(line: string): void {
    for (const listener of this.lineListeners) {
      listener(line);
    }
  }

  emitExit(): void {
    for (const listener of this.exitListeners) {
      listener();
    }
  }
}

const STATUS_LINE = '{"response":"status","mode":"normal","active":1,"normal":1,"candidate":null,"application":1,"migration":false,"rounds":42}';

function createStatus(overrides?: Partial<HotEditStatus>): HotEditStatus {
  return {
    mode: 'normal',
    active: 1,
    normal: 1,
    candidate: null,
    application: 1,
    migration: false,
    rounds: 0,
    ...overrides,
  };
}

suite('encodeRequest', () => {
  test('encodeRequest_when_simple_command_then_tag_only_line', () => {
    assert.strictEqual(encodeRequest('getStatus'), '{"command":"getStatus"}');
    assert.strictEqual(encodeRequest('testEdits'), '{"command":"testEdits"}');
    assert.strictEqual(encodeRequest('untestEdits'), '{"command":"untestEdits"}');
    assert.strictEqual(encodeRequest('assembleEdits'), '{"command":"assembleEdits"}');
    assert.strictEqual(encodeRequest('cancelEdits'), '{"command":"cancelEdits"}');
  });

  test('encodeRequest_when_accept_edits_then_carries_program_bytes', () => {
    assert.strictEqual(
      encodeRequest('acceptEdits', new Uint8Array([1, 2, 255])),
      '{"command":"acceptEdits","program":[1,2,255]}',
    );
  });

  test('encodeRequest_when_accept_edits_without_program_then_empty_array', () => {
    assert.strictEqual(
      encodeRequest('acceptEdits'),
      '{"command":"acceptEdits","program":[]}',
    );
  });
});

suite('parseResponseLine', () => {
  test('parseResponseLine_when_status_then_every_field', () => {
    const response = parseResponseLine(STATUS_LINE);

    assert.deepStrictEqual(response, {
      kind: 'status',
      status: createStatus({ rounds: 42 }),
    });
  });

  test('parseResponseLine_when_testing_with_candidate_then_candidate_present', () => {
    const line = '{"response":"status","mode":"testing","active":2,"normal":1,"candidate":2,"application":1,"migration":true,"rounds":7}';

    const response = parseResponseLine(line);

    assert.deepStrictEqual(response, {
      kind: 'status',
      status: createStatus({ mode: 'testing', active: 2, candidate: 2, migration: true, rounds: 7 }),
    });
  });

  test('parseResponseLine_when_ack_then_ack', () => {
    assert.deepStrictEqual(parseResponseLine('{"response":"ack"}'), { kind: 'ack' });
  });

  test('parseResponseLine_when_error_then_coded_error', () => {
    const response = parseResponseLine('{"response":"error","vCode":"V4012","message":"no candidate is staged"}');

    assert.strictEqual(response.kind, 'error');
    if (response.kind === 'error') {
      assert.strictEqual(response.error.vCode, 'V4012');
      assert.strictEqual(response.error.message, 'no candidate is staged');
    }
  });

  test('parseResponseLine_when_codec_error_then_null_vcode', () => {
    const response = parseResponseLine('{"response":"error","vCode":null,"message":"invalid command line: x"}');

    assert.strictEqual(response.kind, 'error');
    if (response.kind === 'error') {
      assert.strictEqual(response.error.vCode, null);
      assert.strictEqual(response.error.message, 'invalid command line: x');
    }
  });

  test('parseResponseLine_when_not_json_then_throws', () => {
    assert.throws(() => parseResponseLine('not json'), HotEditProtocolError);
  });

  test('parseResponseLine_when_unknown_response_then_throws', () => {
    assert.throws(() => parseResponseLine('{"response":"huh"}'), HotEditProtocolError);
  });

  test('parseResponseLine_when_status_missing_mode_then_throws', () => {
    assert.throws(() => parseResponseLine('{"response":"status","active":1}'), HotEditProtocolError);
  });
});

suite('HotEditProtocolError', () => {
  test('toString_when_vcode_then_code_dash_message', () => {
    assert.strictEqual(
      new HotEditProtocolError('V4012', 'no candidate is staged').toString(),
      'V4012 - no candidate is staged',
    );
  });

  test('toString_when_no_vcode_then_message_only', () => {
    assert.strictEqual(
      new HotEditProtocolError(null, 'invalid response line: x').toString(),
      'invalid response line: x',
    );
  });
});

suite('status formatting', () => {
  test('displayMode_when_normal_then_capitalized', () => {
    assert.strictEqual(displayMode('normal'), 'Normal');
    assert.strictEqual(displayMode('testing'), 'Testing');
  });

  test('formatStatusText_when_normal_then_mode_and_generation', () => {
    assert.strictEqual(formatStatusText(createStatus()), 'Normal (gen 1)');
  });

  test('formatStatusDetail_when_no_candidate_then_omits_candidate_and_migration', () => {
    const detail = formatStatusDetail(createStatus({ rounds: 42 }));

    assert.ok(detail.includes('Mode: Normal'));
    assert.ok(detail.includes('Active generation: 1'));
    assert.ok(detail.includes('Normal generation: 1'));
    assert.ok(detail.includes('Application generation: 1'));
    assert.ok(detail.includes('Rounds: 42'));
    assert.ok(!detail.includes('Candidate'));
  });

  test('formatStatusDetail_when_candidate_then_includes_candidate_and_migration', () => {
    const detail = formatStatusDetail(createStatus({ mode: 'testing', active: 2, candidate: 2, migration: true }));

    assert.ok(detail.includes('Mode: Testing'));
    assert.ok(detail.includes('Candidate generation: 2'));
    assert.ok(detail.includes('schema'));
  });
});

suite('HotEditSession', () => {
  test('getStatus_when_response_then_returns_status', async () => {
    const transport = new MockTransport();
    const session = new HotEditSession(transport);
    const request = session.getStatus();
    await waitFor(() => transport.sent.length === 1);
    transport.emitLine(STATUS_LINE);

    const status = await request;

    assert.deepStrictEqual(status, createStatus({ rounds: 42 }));
    assert.strictEqual(transport.sent[0], '{"command":"getStatus"}');
  });

  test('testEdits_when_ack_then_resolves', async () => {
    const transport = new MockTransport();
    const session = new HotEditSession(transport);
    const request = session.testEdits();
    await waitFor(() => transport.sent.length === 1);
    transport.emitLine('{"response":"ack"}');

    await request;

    assert.strictEqual(transport.sent[0], '{"command":"testEdits"}');
  });

  test('acceptEdits_when_bytes_then_sends_array_line', async () => {
    const transport = new MockTransport();
    const session = new HotEditSession(transport);
    const request = session.acceptEdits(new Uint8Array([9, 8]));
    await waitFor(() => transport.sent.length === 1);
    transport.emitLine('{"response":"ack"}');

    await request;

    assert.strictEqual(transport.sent[0], '{"command":"acceptEdits","program":[9,8]}');
  });

  test('command_when_error_response_then_rejects_with_coded_error', async () => {
    const transport = new MockTransport();
    const session = new HotEditSession(transport);
    const request = session.assembleEdits();
    await waitFor(() => transport.sent.length === 1);
    transport.emitLine('{"response":"error","vCode":"V4015","message":"not allowed in this mode"}');

    const err = await rejectWith(request);

    assert.ok(err instanceof HotEditProtocolError);
    assert.strictEqual(err.vCode, 'V4015');
    assert.strictEqual(err.toString(), 'V4015 - not allowed in this mode');
  });

  test('getStatus_when_error_response_then_rejects', async () => {
    const transport = new MockTransport();
    const session = new HotEditSession(transport);
    const request = session.getStatus();
    await waitFor(() => transport.sent.length === 1);
    transport.emitLine('{"response":"error","vCode":null,"message":"invalid command line: x"}');

    await assert.rejects(request, /invalid command line: x/);
  });

  test('request_when_malformed_line_then_rejects_but_session_stays_active', async () => {
    const transport = new MockTransport();
    const session = new HotEditSession(transport);
    const request = session.getStatus();
    await waitFor(() => transport.sent.length === 1);
    transport.emitLine('garbage');

    await assert.rejects(request, HotEditProtocolError);
    assert.strictEqual(session.isActive, true);
  });

  test('request_when_second_command_then_lines_match_in_order', async () => {
    const transport = new MockTransport();
    const session = new HotEditSession(transport);
    const first = session.testEdits();
    await waitFor(() => transport.sent.length === 1);
    const second = session.cancelEdits();
    await waitFor(() => transport.sent.length === 2);
    transport.emitLine('{"response":"ack"}');
    transport.emitLine('{"response":"ack"}');

    await first;
    await second;
    assert.deepStrictEqual(transport.sent, [
      '{"command":"testEdits"}',
      '{"command":"cancelEdits"}',
    ]);
  });

  test('request_when_exit_then_rejects_pending_and_reports_exit_once', async () => {
    const transport = new MockTransport();
    let exits = 0;
    const session = new HotEditSession(transport, () => exits++);
    const pending = session.getStatus();
    await waitFor(() => transport.sent.length === 1);
    transport.emitExit();
    transport.emitExit();

    await assert.rejects(pending, /exited/);
    assert.strictEqual(session.isActive, false);
    assert.strictEqual(exits, 1);
  });

  test('request_after_exit_then_rejects_without_sending', async () => {
    const transport = new MockTransport();
    const session = new HotEditSession(transport);
    transport.emitExit();

    await assert.rejects(session.testEdits(), /session has ended/);
    assert.strictEqual(transport.sent.length, 0);
  });

  test('dispose_when_pending_then_rejects_pending', async () => {
    const transport = new MockTransport();
    const session = new HotEditSession(transport);
    const pending = session.getStatus();
    await waitFor(() => transport.sent.length === 1);

    session.dispose();

    await assert.rejects(pending, /ended/);
  });

  test('handleLine_when_unsolicited_line_then_drops_silently', async () => {
    const transport = new MockTransport();
    new HotEditSession(transport);

    transport.emitLine(STATUS_LINE);
    // No pending request: nothing throws, nothing resolves.
  });
});

/** Resolves once `condition` holds, polling on the macrotask queue. */
async function waitFor(condition: () => boolean): Promise<void> {
  for (let i = 0; i < 100; i++) {
    if (condition()) {
      return;
    }
    await new Promise(resolve => setTimeout(resolve, 0));
  }
  throw new Error('condition was not met');
}

/** Awaits a promise expected to reject, returning the rejection reason. */
async function rejectWith(promise: Promise<unknown>): Promise<unknown> {
  try {
    await promise;
  }
  catch (err) {
    return err;
  }
  throw new Error('expected the promise to reject');
}
