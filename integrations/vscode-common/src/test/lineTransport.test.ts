import * as assert from 'assert';
import { EventEmitter } from 'events';
import { PassThrough } from 'stream';
import { ChildProcessWithoutNullStreams } from 'child_process';
import { StdioLineTransport } from '../lineTransport';

/** A child whose stdout the test drives and whose stdin it records. */
function createFakeChild() {
  const emitter = new EventEmitter();
  const stdout = new PassThrough();
  const written: string[] = [];
  const child = Object.assign(emitter, {
    stdout,
    stdin: { write: (text: string) => { written.push(text); return true; } },
  }) as unknown as ChildProcessWithoutNullStreams;
  return { child, stdout, emitter, written };
}

suite('StdioLineTransport', () => {
  test('StdioLineTransport_when_stdout_has_whole_lines_then_emits_each_line', () => {
    const { child, stdout } = createFakeChild();
    const lines: string[] = [];
    new StdioLineTransport(child).onLine(line => lines.push(line));
    stdout.write('{"a":1}\n{"b":2}\n');
    assert.deepStrictEqual(lines, ['{"a":1}', '{"b":2}']);
  });

  test('StdioLineTransport_when_a_line_arrives_in_pieces_then_emits_it_once_complete', () => {
    const { child, stdout } = createFakeChild();
    const lines: string[] = [];
    new StdioLineTransport(child).onLine(line => lines.push(line));
    stdout.write('{"respo');
    assert.deepStrictEqual(lines, []);
    stdout.write('nse":"ok"}\n');
    assert.deepStrictEqual(lines, ['{"response":"ok"}']);
  });

  test('StdioLineTransport_when_line_ends_in_carriage_return_then_trims_it', () => {
    const { child, stdout } = createFakeChild();
    const lines: string[] = [];
    new StdioLineTransport(child).onLine(line => lines.push(line));
    stdout.write('{"a":1}\r\n');
    assert.deepStrictEqual(lines, ['{"a":1}']);
  });

  test('StdioLineTransport_when_a_line_is_empty_then_drops_it', () => {
    const { child, stdout } = createFakeChild();
    const lines: string[] = [];
    new StdioLineTransport(child).onLine(line => lines.push(line));
    stdout.write('\n\r\n{"a":1}\n');
    assert.deepStrictEqual(lines, ['{"a":1}']);
  });

  test('StdioLineTransport_when_sending_then_writes_the_line_with_a_newline', () => {
    const { child, written } = createFakeChild();
    new StdioLineTransport(child).sendLine('{"command":"version"}');
    assert.deepStrictEqual(written, ['{"command":"version"}\n']);
  });

  test('StdioLineTransport_when_child_exits_then_reports_exit_once', () => {
    const { child, emitter } = createFakeChild();
    let exits = 0;
    new StdioLineTransport(child).onExit(() => exits++);
    emitter.emit('exit');
    emitter.emit('error', new Error('spawn failed'));
    assert.strictEqual(exits, 1);
  });

  test('StdioLineTransport_when_spawn_fails_then_reports_exit', () => {
    const { child, emitter } = createFakeChild();
    let exits = 0;
    new StdioLineTransport(child).onExit(() => exits++);
    emitter.emit('error', new Error('spawn ENOENT'));
    assert.strictEqual(exits, 1);
  });
});
