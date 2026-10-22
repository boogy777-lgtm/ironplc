import * as assert from 'assert';

import { IronLinkLauncher, IronLinkClient, IronLinkProcess } from '../../ironlinkClient';
import { IronLinkCommandError, IronLinkProblem } from '../../failures';
import { LineTransport, ProblemCode } from '../../vscodeCommon';
import { BROWSE_LINE } from './browseFixture';

/** A fake `ironlink serve`: records the command lines, answers on demand. */
class FakeProcess implements IronLinkProcess, LineTransport {
  readonly sent: string[] = [];
  stopped = false;
  private lineListener: (line: string) => void = () => {};
  private exitListener: () => void = () => {};

  get transport(): LineTransport {
    return this;
  }

  sendLine(line: string): void {
    this.sent.push(line);
  }

  onLine(listener: (line: string) => void): void {
    this.lineListener = listener;
  }

  onExit(listener: () => void): void {
    this.exitListener = listener;
  }

  stop(): void {
    this.stopped = true;
    this.exitListener();
  }

  answer(response: object | string): void {
    this.lineListener(typeof response === 'string' ? response : JSON.stringify(response));
  }

  die(): void {
    this.exitListener();
  }
}

function launcherOf(...processes: FakeProcess[]): { launcher: IronLinkLauncher; launches: () => number } {
  let launched = 0;
  return {
    launcher: async () => {
      const next = processes[launched++];
      if (!next) {
        throw new Error('no more fake processes');
      }
      return next;
    },
    launches: () => launched,
  };
}

/** Lets the client reach the point where it has sent its command. */
async function settle(): Promise<void> {
  await new Promise(resolve => setImmediate(resolve));
}

// REQ-IL-ironlink-vscode-080
suite('IronLinkClient start and stop', () => {
  test('constructor_when_built_then_nothing_is_started_until_the_first_request', async () => {
    const fake = new FakeProcess();
    const { launcher, launches } = launcherOf(fake);
    const client = new IronLinkClient(launcher);
    await settle();
    assert.strictEqual(launches(), 0);
    const pending = client.version();
    await settle();
    assert.strictEqual(launches(), 1);
    fake.answer({ response: 'version', ironlink: '0.247.0', discoveryWireVersion: 1 });
    await pending;
  });

  test('request_when_called_twice_then_one_process_serves_both', async () => {
    const fake = new FakeProcess();
    const { launcher, launches } = launcherOf(fake);
    const client = new IronLinkClient(launcher);
    const first = client.addressList();
    const second = client.version();
    await settle();
    fake.answer({ response: 'addressList', addresses: [] });
    fake.answer({ response: 'version', ironlink: '0.247.0', discoveryWireVersion: 1 });
    await Promise.all([first, second]);
    assert.strictEqual(launches(), 1);
  });

  test('dispose_when_a_process_runs_then_stops_it_and_later_requests_fail_as_exited', async () => {
    const fake = new FakeProcess();
    const client = new IronLinkClient(launcherOf(fake).launcher);
    const first = client.version();
    await settle();
    fake.answer({ response: 'version', ironlink: '0.247.0', discoveryWireVersion: 1 });
    await first;
    client.dispose();
    assert.strictEqual(fake.stopped, true);
    await assert.rejects(client.version(), (err: unknown) =>
      err instanceof IronLinkProblem && err.problem === ProblemCode.IronLinkExited);
  });

  test('dispose_when_nothing_was_started_then_does_not_start_anything', async () => {
    const { launcher, launches } = launcherOf();
    const client = new IronLinkClient(launcher);
    client.dispose();
    assert.strictEqual(launches(), 0);
  });

  test('request_when_the_launcher_fails_then_the_failure_reaches_the_caller_and_the_next_request_retries', async () => {
    const fake = new FakeProcess();
    let attempt = 0;
    const client = new IronLinkClient(async () => {
      if (attempt++ === 0) {
        throw new IronLinkProblem(ProblemCode.IronLinkNotFound);
      }
      return fake;
    });
    await assert.rejects(client.version(), (err: unknown) =>
      err instanceof IronLinkProblem && err.problem === ProblemCode.IronLinkNotFound);
    const retry = client.version();
    await settle();
    fake.answer({ response: 'version', ironlink: '0.247.0', discoveryWireVersion: 1 });
    assert.strictEqual((await retry).ironlink, '0.247.0');
  });

  test('constructor_when_a_process_starts_then_reports_it_once', async () => {
    const fake = new FakeProcess();
    const started: IronLinkProcess[] = [];
    const client = new IronLinkClient(launcherOf(fake).launcher, process => started.push(process));
    for (let request = 0; request < 2; request++) {
      const pending = client.version();
      await settle();
      fake.answer({ response: 'version', ironlink: '0.247.0', discoveryWireVersion: 1 });
      await pending;
    }
    assert.deepStrictEqual(started, [fake]);
  });
});

// REQ-IL-ironlink-vscode-081
suite('IronLinkClient commands and responses', () => {
  test('browse_when_options_are_set_then_sends_one_json_line_with_only_those_options', async () => {
    const fake = new FakeProcess();
    const client = new IronLinkClient(launcherOf(fake).launcher);
    const pending = client.browse({ timeoutMs: 2500 });
    await settle();
    assert.deepStrictEqual(fake.sent, ['{"command":"browse","timeoutMs":2500}']);
    fake.answer(BROWSE_LINE);
    const response = await pending;
    assert.strictEqual(response.workstation.name, 'PC-ONE');
    assert.strictEqual(response.drivers.length, 2);
  });

  test('browse_when_no_options_then_the_command_has_none_so_the_program_applies_its_defaults', async () => {
    const fake = new FakeProcess();
    const client = new IronLinkClient(launcherOf(fake).launcher);
    const pending = client.browse({ timeoutMs: undefined, attempts: undefined });
    await settle();
    assert.deepStrictEqual(fake.sent, ['{"command":"browse"}']);
    fake.answer(BROWSE_LINE);
    await pending;
  });

  test('requests_when_pipelined_then_each_gets_the_response_in_order', async () => {
    const fake = new FakeProcess();
    const client = new IronLinkClient(launcherOf(fake).launcher);
    const list = client.addressList();
    const added = client.addressAdd('10.0.4.7');
    const removed = client.addressRemove('10.0.4.8:49152');
    await settle();
    assert.deepStrictEqual(fake.sent, [
      '{"command":"addressList"}',
      '{"command":"addressAdd","address":"10.0.4.7"}',
      '{"command":"addressRemove","address":"10.0.4.8:49152"}',
    ]);
    fake.answer({ response: 'addressList', addresses: ['10.0.4.8:49152'] });
    fake.answer({ response: 'addressAdd', address: '10.0.4.7:49152', added: true });
    fake.answer({ response: 'addressRemove', address: '10.0.4.8:49152', removed: true });
    assert.deepStrictEqual((await list).addresses, ['10.0.4.8:49152']);
    assert.strictEqual((await added).added, true);
    assert.strictEqual((await removed).removed, true);
  });

  test('identify_when_the_route_answers_none_then_resolves_with_the_none_answer', async () => {
    const fake = new FakeProcess();
    const client = new IronLinkClient(launcherOf(fake).launcher);
    const pending = client.identify('10.0.4.7:49152', { attempts: 3 });
    await settle();
    assert.deepStrictEqual(fake.sent, ['{"command":"identify","address":"10.0.4.7:49152","attempts":3}']);
    fake.answer({ response: 'identify', address: '10.0.4.7:49152', timeoutMs: 1000, answer: 'none' });
    assert.strictEqual((await pending).answer, 'none');
  });
});

// REQ-IL-ironlink-vscode-082
suite('IronLinkClient failures', () => {
  test('request_when_the_response_is_an_error_with_an_l_code_then_fails_with_that_code_and_message', async () => {
    const fake = new FakeProcess();
    const client = new IronLinkClient(launcherOf(fake).launcher);
    const pending = client.addressAdd('host');
    await settle();
    fake.answer({ response: 'error', lCode: 'L6004', message: "'host' is not an IP address or IP:PORT" });
    await assert.rejects(pending, (err: unknown) =>
      err instanceof IronLinkCommandError
      && err.lCode === 'L6004'
      && err.reason === "'host' is not an IP address or IP:PORT");
  });

  test('request_when_a_command_failed_then_the_process_stays_and_the_next_command_works', async () => {
    const fake = new FakeProcess();
    const { launcher, launches } = launcherOf(fake);
    const client = new IronLinkClient(launcher);
    const failing = client.addressAdd('host');
    await settle();
    fake.answer({ response: 'error', lCode: 'L6004', message: 'bad' });
    await assert.rejects(failing, IronLinkCommandError);
    const next = client.addressList();
    await settle();
    fake.answer({ response: 'addressList', addresses: [] });
    await next;
    assert.strictEqual(launches(), 1);
    assert.strictEqual(fake.stopped, false);
  });

  test('request_when_the_error_has_no_l_code_then_fails_as_the_protocol_problem', async () => {
    const fake = new FakeProcess();
    const client = new IronLinkClient(launcherOf(fake).launcher);
    const pending = client.version();
    await settle();
    fake.answer({ response: 'error', lCode: null, message: 'invalid command line: unknown variant' });
    await assert.rejects(pending, (err: unknown) =>
      err instanceof IronLinkProblem
      && err.problem === ProblemCode.IronLinkProtocolError
      && err.message.includes('unknown variant'));
  });
});

// REQ-IL-ironlink-vscode-083
suite('IronLinkClient process exit', () => {
  test('request_when_the_process_exits_with_requests_pending_then_all_fail_as_exited', async () => {
    const fake = new FakeProcess();
    const client = new IronLinkClient(launcherOf(fake).launcher);
    const first = client.browse();
    const second = client.addressList();
    await settle();
    fake.die();
    for (const pending of [first, second]) {
      await assert.rejects(pending, (err: unknown) =>
        err instanceof IronLinkProblem && err.problem === ProblemCode.IronLinkExited);
    }
  });

  test('request_when_the_process_exited_then_the_next_request_starts_a_new_process', async () => {
    const first = new FakeProcess();
    const second = new FakeProcess();
    const { launcher, launches } = launcherOf(first, second);
    const client = new IronLinkClient(launcher);
    const lost = client.version();
    await settle();
    first.die();
    await assert.rejects(lost, IronLinkProblem);
    const again = client.version();
    await settle();
    second.answer({ response: 'version', ironlink: '0.247.0', discoveryWireVersion: 1 });
    assert.strictEqual((await again).ironlink, '0.247.0');
    assert.strictEqual(launches(), 2);
  });
});

// REQ-IL-ironlink-vscode-084
suite('IronLinkClient unreadable responses', () => {
  test('request_when_the_line_is_not_json_then_fails_as_protocol_problem_and_replaces_the_process', async () => {
    const first = new FakeProcess();
    const second = new FakeProcess();
    const { launcher, launches } = launcherOf(first, second);
    const client = new IronLinkClient(launcher);
    const pending = client.version();
    await settle();
    first.answer('this is not json');
    await assert.rejects(pending, (err: unknown) =>
      err instanceof IronLinkProblem && err.problem === ProblemCode.IronLinkProtocolError);
    assert.strictEqual(first.stopped, true);
    const again = client.version();
    await settle();
    second.answer({ response: 'version', ironlink: '0.247.0', discoveryWireVersion: 1 });
    await again;
    assert.strictEqual(launches(), 2);
  });

  test('request_when_another_command_is_answered_then_fails_as_protocol_problem', async () => {
    const fake = new FakeProcess();
    const client = new IronLinkClient(launcherOf(fake).launcher);
    const pending = client.version();
    await settle();
    fake.answer({ response: 'addressList', addresses: [] });
    await assert.rejects(pending, (err: unknown) =>
      err instanceof IronLinkProblem
      && err.problem === ProblemCode.IronLinkProtocolError
      && err.message.includes('"addressList" response answered the "version" command'));
  });
});
