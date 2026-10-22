import * as assert from 'assert';
import { LineRequestChannel } from '../lineRequestChannel';
import { LineTransport } from '../lineTransport';

class FakeTransport implements LineTransport {
  readonly sent: string[] = [];
  private lineListener: (line: string) => void = () => {};
  private exitListener: () => void = () => {};
  sendLine(line: string): void { this.sent.push(line); }
  onLine(listener: (line: string) => void): void { this.lineListener = listener; }
  onExit(listener: () => void): void { this.exitListener = listener; }
  receive(line: string): void { this.lineListener(line); }
  close(): void { this.exitListener(); }
}

const errors = {
  ended: () => new Error('ended'),
  exited: () => new Error('exited'),
};

suite('LineRequestChannel', () => {
  test('request_when_pipelined_then_responses_resolve_in_order', async () => {
    const transport = new FakeTransport();
    const channel = new LineRequestChannel(transport, errors);
    const first = channel.request('one');
    const second = channel.request('two');
    assert.deepStrictEqual(transport.sent, ['one', 'two']);
    transport.receive('answer-one');
    transport.receive('answer-two');
    assert.strictEqual(await first, 'answer-one');
    assert.strictEqual(await second, 'answer-two');
  });

  test('request_when_peer_exits_with_pending_then_rejects_them_with_the_exited_error', async () => {
    const transport = new FakeTransport();
    let exits = 0;
    const channel = new LineRequestChannel(transport, errors, () => exits++);
    const pending = channel.request('one');
    transport.close();
    await assert.rejects(pending, /exited/);
    assert.strictEqual(channel.isActive, false);
    assert.strictEqual(exits, 1);
  });

  test('request_when_channel_already_exited_then_rejects_with_the_ended_error', async () => {
    const transport = new FakeTransport();
    const channel = new LineRequestChannel(transport, errors);
    transport.close();
    await assert.rejects(channel.request('one'), /ended/);
    assert.deepStrictEqual(transport.sent, []);
  });

  test('exit_when_reported_twice_then_calls_the_exit_callback_once', () => {
    const transport = new FakeTransport();
    let exits = 0;
    new LineRequestChannel(transport, errors, () => exits++);
    transport.close();
    transport.close();
    assert.strictEqual(exits, 1);
  });

  test('receive_when_no_request_is_pending_then_drops_the_line', async () => {
    const transport = new FakeTransport();
    const channel = new LineRequestChannel(transport, errors);
    transport.receive('unasked');
    const pending = channel.request('one');
    transport.receive('answer');
    assert.strictEqual(await pending, 'answer');
  });

  test('end_when_requests_are_pending_then_rejects_them_with_the_ended_error', async () => {
    const transport = new FakeTransport();
    const channel = new LineRequestChannel(transport, errors);
    const pending = channel.request('one');
    channel.end();
    await assert.rejects(pending, /ended/);
    assert.strictEqual(channel.isActive, true);
  });

  test('rejectPending_when_given_a_reason_then_rejects_every_pending_request_with_it', async () => {
    const transport = new FakeTransport();
    const channel = new LineRequestChannel(transport, errors);
    const first = channel.request('one');
    const second = channel.request('two');
    channel.rejectPending(new Error('desync'));
    await assert.rejects(first, /desync/);
    await assert.rejects(second, /desync/);
  });
});
