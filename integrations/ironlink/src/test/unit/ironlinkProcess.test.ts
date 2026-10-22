import * as assert from 'assert';

import { IronLinkClient } from '../../ironlinkClient';
import { IronLinkProblem } from '../../failures';
import { ironLinkLauncher, locateIronLink, spawnProcess } from '../../ironlinkProcess';
import { ProblemCode, ToolchainEnvironment } from '../../vscodeCommon';

/**
 * A child process that serves like `ironlink serve` for two commands: it
 * answers every command with a `version` response, except `addressList`,
 * which makes it exit.
 */
const FAKE_SERVE = `
const readline = require('readline');
const lines = readline.createInterface({ input: process.stdin });
process.stderr.write('fake serve started\\n');
lines.on('line', line => {
  const command = JSON.parse(line).command;
  if (command === 'addressList') {
    process.exit(3);
  }
  process.stdout.write(JSON.stringify({ response: 'version', ironlink: 'fake', discoveryWireVersion: 1 }) + '\\r\\n');
});
`;

function launcherForFake(stderr: string[]) {
  return () => spawnProcess(process.execPath, ['-e', FAKE_SERVE], text => stderr.push(text));
}

function env(overrides: Partial<ToolchainEnvironment> = {}): ToolchainEnvironment {
  return {
    platform: 'linux',
    existsSync: () => false,
    getEnv: () => undefined,
    getConfig: () => undefined,
    ...overrides,
  };
}

// REQ-IL-ironlink-vscode-079
suite('locateIronLink', () => {
  test('locateIronLink_when_the_ironlink_path_setting_names_the_directory_then_finds_it_there', () => {
    const found = locateIronLink(env({
      getConfig: key => key === 'path' ? '/opt/ironlink-bin' : undefined,
      existsSync: p => p === '/opt/ironlink-bin/ironlink',
    }));
    assert.deepStrictEqual(found, { path: '/opt/ironlink-bin/ironlink', source: 'configuration' });
  });

  test('locateIronLink_when_only_the_system_path_has_it_then_finds_it_by_the_path_trial', () => {
    const found = locateIronLink(env({
      getEnv: name => name === 'PATH' ? '/usr/bin:/home/me/bin' : undefined,
      existsSync: p => p === '/home/me/bin/ironlink',
    }));
    assert.deepStrictEqual(found, { path: '/home/me/bin/ironlink', source: 'path' });
  });

  test('locateIronLink_when_win32_then_looks_for_ironlink_exe', () => {
    const found = locateIronLink(env({
      platform: 'win32',
      getConfig: key => key === 'path' ? 'C:\\tools' : undefined,
      existsSync: p => p === 'C:\\tools\\ironlink.exe',
    }));
    assert.strictEqual(found?.path, 'C:\\tools\\ironlink.exe');
  });
});

// REQ-IL-ironlink-vscode-084
suite('ironLinkLauncher', () => {
  test('ironLinkLauncher_when_the_program_is_not_found_then_fails_as_not_found', async () => {
    const launch = ironLinkLauncher(() => env(), () => {});
    await assert.rejects(launch(), (err: unknown) =>
      err instanceof IronLinkProblem
      && err.problem === ProblemCode.IronLinkNotFound
      && err.message.startsWith('E0016 - '));
  });

  test('ironLinkLauncher_when_found_but_not_startable_then_fails_as_start_failed_with_the_reason', async () => {
    const lines: string[] = [];
    const launch = ironLinkLauncher(
      () => env({
        getConfig: key => key === 'path' ? '/definitely/not/here' : undefined,
        existsSync: () => true,
      }),
      () => {},
      line => lines.push(line),
    );
    await assert.rejects(launch(), (err: unknown) =>
      err instanceof IronLinkProblem
      && err.problem === ProblemCode.IronLinkStartFailed
      && err.message.startsWith('E0017 - ')
      && err.message.includes('/definitely/not/here'));
    assert.strictEqual(lines.length, 1);
    assert.ok(lines[0].includes('serve'), lines[0]);
    assert.ok(lines[0].includes('found by: configuration'), lines[0]);
  });
});

// REQ-IL-ironlink-vscode-080
suite('spawnProcess with a real child', () => {
  test('request_when_the_child_answers_crlf_lines_then_the_client_gets_the_response', async () => {
    const stderr: string[] = [];
    const client = new IronLinkClient(launcherForFake(stderr));
    try {
      const response = await client.version();
      assert.strictEqual(response.ironlink, 'fake');
      const second = await client.version();
      assert.strictEqual(second.discoveryWireVersion, 1);
      assert.ok(stderr.join('').includes('fake serve started'));
    }
    finally {
      client.dispose();
    }
  });

  test('request_when_the_child_exits_then_it_fails_as_exited_and_the_next_request_starts_a_new_child', async () => {
    const client = new IronLinkClient(launcherForFake([]));
    try {
      await client.version();
      await assert.rejects(client.addressList(), (err: unknown) =>
        err instanceof IronLinkProblem && err.problem === ProblemCode.IronLinkExited);
      assert.strictEqual((await client.version()).ironlink, 'fake');
    }
    finally {
      client.dispose();
    }
  });

  test('dispose_when_a_child_runs_then_it_is_killed', async () => {
    const client = new IronLinkClient(launcherForFake([]));
    await client.version();
    client.dispose();
    await assert.rejects(client.version(), IronLinkProblem);
  });
});
