import * as assert from 'assert';
import { Server, Socket, createConnection, createServer } from 'net';

import {
  connectTcpLineTransport,
  MAX_FRAME_LENGTH,
  TcpLineTransport,
} from '../../tcpTransport';

/** One ADR-0063 frame: 4-byte little-endian length, then the line bytes. */
function frame(line: string): Buffer {
  const payload = Buffer.from(line, 'utf8');
  const header = Buffer.alloc(4);
  header.writeUInt32LE(payload.length, 0);
  return Buffer.concat([header, payload]);
}

/** The lines a test server received, parsed out of the frames on `socket`. */
function listenForLines(socket: Socket, onLine: (line: string) => void): void {
  let buffer: Buffer = Buffer.alloc(0);
  socket.on('data', (chunk: Buffer) => {
    buffer = buffer.length === 0 ? chunk : Buffer.concat([buffer, chunk]);
    for (;;) {
      if (buffer.length < 4) {
        return;
      }
      const length = buffer.readUInt32LE(0);
      if (buffer.length < 4 + length) {
        return;
      }
      onLine(buffer.subarray(4, 4 + length).toString('utf8'));
      buffer = buffer.subarray(4 + length);
    }
  });
}

/**
 * A test server on an ephemeral port: `handler` answers each accepted
 * socket; closes when the suite drops it.
 */
function withServer(
  handler: (socket: Socket) => void,
): Promise<{ server: Server; port: number }> {
  return new Promise((resolve, reject) => {
    const server = createServer(handler);
    server.on('error', reject);
    server.listen(0, '127.0.0.1', () => {
      const address = server.address();
      if (address === null || typeof address === 'string') {
        reject(new Error('no ephemeral address'));
        return;
      }
      resolve({ server, port: address.port });
    });
  });
}

/**
 * Resolves once `condition` holds, polling: a TCP loopback round trip has
 * no fixed latency a sleep could pin down.
 */
async function waitFor(condition: () => boolean): Promise<void> {
  for (let attempt = 0; attempt < 100; attempt++) {
    if (condition()) {
      return;
    }
    await new Promise<void>(resolve => setTimeout(resolve, 10));
  }
  assert.fail('condition was not met within 1 s');
}

suite('TcpLineTransport', () => {
  test('sendLine_when_line_written_then_server_receives_one_frame', async () => {
    const received: string[] = [];
    const { server, port } = await withServer(socket => listenForLines(socket, line => received.push(line)));
    const transport = await connectTcpLineTransport('127.0.0.1', port);

    transport.sendLine('{"command":"identity"}');
    await waitFor(() => received.length > 0);

    assert.deepStrictEqual(received, ['{"command":"identity"}']);
    transport.dispose();
    server.close();
  });

  test('onLine_when_server_sends_frame_then_line_delivered', async () => {
    const { server, port } = await withServer((socket) => {
      socket.write(frame('{"response":"ack"}'));
    });
    const transport = await connectTcpLineTransport('127.0.0.1', port);
    const lines: string[] = [];
    transport.onLine(line => lines.push(line));
    await waitFor(() => lines.length > 0);

    assert.deepStrictEqual(lines, ['{"response":"ack"}']);
    transport.dispose();
    server.close();
  });

  test('onLine_when_frame_splits_across_chunks_then_line_assembled_once', async () => {
    const payload = frame('{"command":"getStatus"}');
    const { server, port } = await withServer((socket) => {
      socket.write(payload.subarray(0, 2));
      setTimeout(() => socket.write(payload.subarray(2, 7)), 10);
      setTimeout(() => socket.write(payload.subarray(7)), 20);
    });
    const transport = await connectTcpLineTransport('127.0.0.1', port);
    const lines: string[] = [];
    transport.onLine(line => lines.push(line));
    await waitFor(() => lines.length > 0);

    assert.deepStrictEqual(lines, ['{"command":"getStatus"}']);
    transport.dispose();
    server.close();
  });

  test('onLine_when_two_frames_share_a_chunk_then_lines_delivered_in_order', async () => {
    const { server, port } = await withServer((socket) => {
      socket.write(Buffer.concat([frame('{"a":1}'), frame('{"b":2}')]));
    });
    const transport = await connectTcpLineTransport('127.0.0.1', port);
    const lines: string[] = [];
    transport.onLine(line => lines.push(line));
    await waitFor(() => lines.length > 1);

    assert.deepStrictEqual(lines, ['{"a":1}', '{"b":2}']);
    transport.dispose();
    server.close();
  });

  test('onExit_when_server_sends_refusal_and_closes_then_line_then_exit', async () => {
    // The single-session refusal (V6014): one coded error line, then the
    // socket closes — the transport surfaces the line first, then exit.
    const refusal = '{"response":"error","vCode":"V6014","message":"one session only"}';
    const { server, port } = await withServer((socket) => {
      socket.write(frame(refusal));
      socket.end();
    });
    const transport = await connectTcpLineTransport('127.0.0.1', port);
    const lines: string[] = [];
    let exits = 0;
    transport.onLine(line => lines.push(line));
    transport.onExit(() => exits++);
    await waitFor(() => exits > 0);

    assert.deepStrictEqual(lines, [refusal]);
    assert.strictEqual(exits, 1);
    server.close();
  });

  test('onExit_when_length_exceeds_cap_then_transport_faults', async () => {
    const header = Buffer.alloc(4);
    header.writeUInt32LE(MAX_FRAME_LENGTH + 1, 0);
    const { server, port } = await withServer((socket) => {
      socket.write(header);
    });
    const transport = await connectTcpLineTransport('127.0.0.1', port);
    let exits = 0;
    transport.onExit(() => exits++);
    await waitFor(() => exits > 0);

    assert.strictEqual(exits, 1);
    server.close();
  });

  test('onExit_when_disposed_then_exit_reported_once', async () => {
    const { server, port } = await withServer(() => {});
    const transport = await connectTcpLineTransport('127.0.0.1', port);
    let exits = 0;
    transport.onExit(() => exits++);

    transport.dispose();
    transport.dispose();
    await waitFor(() => exits > 0);

    assert.strictEqual(exits, 1);
    server.close();
  });

  test('connect_when_connection_refused_then_rejects', async () => {
    const { server, port } = await withServer(() => {});
    server.close();
    await new Promise<void>(resolve => server.on('close', resolve));

    await assert.rejects(connectTcpLineTransport('127.0.0.1', port, 1000));
  });

  test('connect_when_custom_transport_constructed_then_frame_round_trip', async () => {
    // The socket-injection seam: later slices reuse an already-connected
    // socket (e.g. from a profile's validated address).
    const { server, port } = await withServer((socket) => {
      listenForLines(socket, line => socket.write(frame(line)));
    });
    const socket = createConnection({ host: '127.0.0.1', port });
    await new Promise<void>((resolve, reject) => {
      socket.on('connect', resolve);
      socket.on('error', reject);
    });
    const transport = new TcpLineTransport(socket);
    const lines: string[] = [];
    transport.onLine(line => lines.push(line));

    transport.sendLine('{"command":"identity"}');
    await waitFor(() => lines.length > 0);

    assert.deepStrictEqual(lines, ['{"command":"identity"}']);
    transport.dispose();
    server.close();
  });
});
