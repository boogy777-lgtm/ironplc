import * as assert from 'assert';
import { ToolchainEnvironment, executableFileName, findToolchainExecutable } from '../toolchainDiscovery';

function createTestEnv(overrides?: Partial<ToolchainEnvironment>): ToolchainEnvironment {
  return {
    platform: 'linux',
    existsSync: () => false,
    getEnv: () => undefined,
    getConfig: () => undefined,
    ...overrides,
  };
}

suite('findToolchainExecutable', () => {
  test('findToolchainExecutable_when_config_path_exists_then_returns_config_path', () => {
    const env = createTestEnv({
      getConfig: key => key === 'path' ? '/custom/dir' : undefined,
      existsSync: p => p === '/custom/dir/ironplcc',
    });
    const result = findToolchainExecutable(env, 'ironplcc');
    assert.ok(result);
    assert.strictEqual(result.path, '/custom/dir/ironplcc');
    assert.strictEqual(result.source, 'configuration');
  });

  test('findToolchainExecutable_when_config_path_missing_then_tries_env', () => {
    const env = createTestEnv({
      getConfig: () => undefined,
      getEnv: name => name === 'IRONPLC' ? '/env/dir' : undefined,
      existsSync: p => p === '/env/dir/ironplcc',
    });
    const result = findToolchainExecutable(env, 'ironplcc');
    assert.ok(result);
    assert.strictEqual(result.path, '/env/dir/ironplcc');
    assert.strictEqual(result.source, 'environment');
  });

  test('findToolchainExecutable_when_env_var_exists_then_returns_env_path', () => {
    const env = createTestEnv({
      getEnv: name => name === 'IRONPLC' ? '/my/compiler' : undefined,
      existsSync: p => p === '/my/compiler/ironplcc',
    });
    const result = findToolchainExecutable(env, 'ironplcc');
    assert.ok(result);
    assert.strictEqual(result.path, '/my/compiler/ironplcc');
    assert.strictEqual(result.source, 'environment');
  });

  test('findToolchainExecutable_when_env_var_missing_then_tries_platform_paths', () => {
    const env = createTestEnv({
      platform: 'darwin',
      existsSync: p => p === '/opt/homebrew/bin/ironplcc',
    });
    const result = findToolchainExecutable(env, 'ironplcc');
    assert.ok(result);
    assert.strictEqual(result.path, '/opt/homebrew/bin/ironplcc');
    assert.strictEqual(result.source, 'homebrew');
  });

  test('findToolchainExecutable_when_darwin_and_homebrew_exists_then_returns_homebrew_path', () => {
    const env = createTestEnv({
      platform: 'darwin',
      existsSync: p => p === '/opt/homebrew/bin/ironplcc',
    });
    const result = findToolchainExecutable(env, 'ironplcc');
    assert.ok(result);
    assert.strictEqual(result.path, '/opt/homebrew/bin/ironplcc');
    assert.strictEqual(result.source, 'homebrew');
  });

  test('findToolchainExecutable_when_win32_and_localappdata_exists_then_returns_windows_path', () => {
    const localAppData = 'C:\\Users\\me\\AppData\\Local';
    const expected = 'C:\\Users\\me\\AppData\\Local\\Programs\\IronPLC Compiler\\bin\\ironplcc.exe';
    const env = createTestEnv({
      platform: 'win32',
      getEnv: name => name === 'LOCALAPPDATA' ? localAppData : undefined,
      existsSync: p => p === expected,
    });
    const result = findToolchainExecutable(env, 'ironplcc');
    assert.ok(result);
    assert.strictEqual(result.path, expected);
    assert.strictEqual(result.source, 'localappdata');
  });

  test('findToolchainExecutable_when_nothing_found_then_returns_undefined', () => {
    const env = createTestEnv();
    const result = findToolchainExecutable(env, 'ironplcc');
    assert.strictEqual(result, undefined);
  });

  test('findToolchainExecutable_when_win32_then_uses_exe_extension', () => {
    const configDir = 'C:\\tools\\ironplc';
    const expected = 'C:\\tools\\ironplc\\ironplcc.exe';
    const env = createTestEnv({
      platform: 'win32',
      getConfig: key => key === 'path' ? configDir : undefined,
      existsSync: p => p === expected,
    });
    const result = findToolchainExecutable(env, 'ironplcc');
    assert.ok(result);
    assert.strictEqual(result.path, expected);
  });

  test('findToolchainExecutable_when_win32_then_joins_with_backslashes_on_any_host', () => {
    // A forward-slash directory is normalised to the target platform's
    // separator; the host's separator must not leak into the result.
    const expected = 'C:\\tools\\ironplc\\ironplcc.exe';
    const env = createTestEnv({
      platform: 'win32',
      getConfig: key => key === 'path' ? 'C:/tools/ironplc' : undefined,
      existsSync: p => p === expected,
    });
    assert.strictEqual(findToolchainExecutable(env, 'ironplcc')?.path, expected);
  });

  test('findToolchainExecutable_when_linux_then_joins_with_forward_slashes_on_any_host', () => {
    // A trailing separator and a doubled one collapse with '/', never a backslash.
    const expected = '/usr/local/bin/ironplcc';
    const env = createTestEnv({
      platform: 'linux',
      getConfig: key => key === 'path' ? '/usr/local//bin/' : undefined,
      existsSync: p => p === expected,
    });
    assert.strictEqual(findToolchainExecutable(env, 'ironplcc')?.path, expected);
  });

  test('findToolchainExecutable_when_linux_then_no_exe_extension', () => {
    const env = createTestEnv({
      platform: 'linux',
      getConfig: key => key === 'path' ? '/usr/bin' : undefined,
      existsSync: p => p === '/usr/bin/ironplcc',
    });
    const result = findToolchainExecutable(env, 'ironplcc');
    assert.ok(result);
    assert.ok(!result.path.endsWith('.exe'));
    assert.ok(result.path.endsWith('ironplcc'));
  });

  test('findToolchainExecutable_when_other_executable_requested_then_searches_for_that_name', () => {
    const env = createTestEnv({
      platform: 'linux',
      getConfig: key => key === 'path' ? '/opt/ironplc' : undefined,
      existsSync: p => p === '/opt/ironplc/ironlink',
    });
    assert.strictEqual(findToolchainExecutable(env, 'ironplcc'), undefined);
    assert.deepStrictEqual(
      findToolchainExecutable(env, 'ironlink'),
      { path: '/opt/ironplc/ironlink', source: 'configuration' },
    );
  });

  test('findToolchainExecutable_when_win32_and_other_executable_then_appends_exe', () => {
    const expected = 'C:\\tools\\ironplcvm.exe';
    const env = createTestEnv({
      platform: 'win32',
      getConfig: key => key === 'path' ? 'C:\\tools' : undefined,
      existsSync: p => p === expected,
    });
    assert.strictEqual(findToolchainExecutable(env, 'ironplcvm')?.path, expected);
  });

  test('findToolchainExecutable_when_configuration_and_environment_both_hold_it_then_configuration_wins', () => {
    const env = createTestEnv({
      getConfig: key => key === 'path' ? '/cfg' : undefined,
      getEnv: name => name === 'IRONPLC' ? '/env' : undefined,
      existsSync: () => true,
    });
    assert.strictEqual(findToolchainExecutable(env, 'ironplcc')?.source, 'configuration');
  });

  test('findToolchainExecutable_when_configuration_lacks_it_then_falls_through_to_environment', () => {
    const env = createTestEnv({
      getConfig: key => key === 'path' ? '/cfg' : undefined,
      getEnv: name => name === 'IRONPLC' ? '/env' : undefined,
      existsSync: p => p === '/env/ironplcc',
    });
    assert.strictEqual(findToolchainExecutable(env, 'ironplcc')?.source, 'environment');
  });

  test('findToolchainExecutable_when_non_darwin_then_does_not_try_homebrew', () => {
    const env = createTestEnv({
      platform: 'linux',
      existsSync: p => p === '/opt/homebrew/bin/ironplcc',
    });
    assert.strictEqual(findToolchainExecutable(env, 'ironplcc'), undefined);
  });
});

suite('findToolchainExecutable PATH trial', () => {
  const pathEnv = (value: string) => (name: string) => name === 'PATH' ? value : undefined;

  test('findToolchainExecutable_when_only_path_holds_it_then_source_is_path', () => {
    const env = createTestEnv({
      getEnv: pathEnv('/usr/bin:/opt/tools'),
      existsSync: p => p === '/opt/tools/ironlink',
    });
    assert.deepStrictEqual(
      findToolchainExecutable(env, 'ironlink'),
      { path: '/opt/tools/ironlink', source: 'path' },
    );
  });

  test('findToolchainExecutable_when_path_has_several_hits_then_first_entry_wins', () => {
    const env = createTestEnv({
      getEnv: pathEnv('/first:/second'),
      existsSync: () => true,
    });
    assert.strictEqual(findToolchainExecutable(env, 'ironlink')?.path, '/first/ironlink');
  });

  test('findToolchainExecutable_when_every_other_trial_hits_then_path_is_tried_last', () => {
    // Configuration, environment, homebrew and PATH all hold the executable:
    // each is removed in turn and the next trial in the documented order wins.
    const holders: Record<string, string> = {
      configuration: '/cfg/ironlink',
      environment: '/env/ironlink',
      homebrew: '/opt/homebrew/bin/ironlink',
      path: '/onpath/ironlink',
    };
    const present = new Set(Object.values(holders));
    const env = createTestEnv({
      platform: 'darwin',
      getConfig: key => key === 'path' ? '/cfg' : undefined,
      getEnv: name => name === 'IRONPLC' ? '/env' : name === 'PATH' ? '/onpath' : undefined,
      existsSync: p => present.has(p),
    });
    const order: string[] = [];
    for (let result = findToolchainExecutable(env, 'ironlink'); result;
      result = findToolchainExecutable(env, 'ironlink')) {
      order.push(result.source);
      present.delete(result.path);
    }
    assert.deepStrictEqual(order, ['configuration', 'environment', 'homebrew', 'path']);
  });

  test('findToolchainExecutable_when_win32_and_localappdata_and_path_hold_it_then_localappdata_wins', () => {
    const local = 'C:\\Users\\me\\AppData\\Local\\Programs\\IronPLC Compiler\\bin\\ironlink.exe';
    const env = createTestEnv({
      platform: 'win32',
      getEnv: name => name === 'LOCALAPPDATA' ? 'C:\\Users\\me\\AppData\\Local'
        : name === 'PATH' ? 'C:\\tools' : undefined,
      existsSync: () => true,
    });
    const result = findToolchainExecutable(env, 'ironlink');
    assert.strictEqual(result?.path, local);
    assert.strictEqual(result?.source, 'localappdata');
  });

  test('findToolchainExecutable_when_win32_then_path_is_split_on_semicolon', () => {
    // A drive letter contains a colon that must not split a win32 PATH.
    const env = createTestEnv({
      platform: 'win32',
      getEnv: pathEnv('C:\\Windows;D:\\tools\\bin'),
      existsSync: p => p === 'D:\\tools\\bin\\ironlink.exe',
    });
    assert.deepStrictEqual(
      findToolchainExecutable(env, 'ironlink'),
      { path: 'D:\\tools\\bin\\ironlink.exe', source: 'path' },
    );
  });

  test('findToolchainExecutable_when_posix_then_path_is_split_on_colon_not_semicolon', () => {
    const env = createTestEnv({
      platform: 'linux',
      getEnv: pathEnv('/a;b:/c'),
      existsSync: p => p === '/c/ironlink' || p === '/a;b/ironlink',
    });
    // '/a;b' is one entry on a posix platform, so it is tried before '/c'.
    assert.strictEqual(findToolchainExecutable(env, 'ironlink')?.path, '/a;b/ironlink');
  });

  test('findToolchainExecutable_when_path_has_empty_entries_then_they_are_skipped', () => {
    const tried: string[] = [];
    const env = createTestEnv({
      getEnv: pathEnv(':/bin::/usr/bin:'),
      existsSync: p => { tried.push(p); return p === '/usr/bin/ironlink'; },
    });
    assert.strictEqual(findToolchainExecutable(env, 'ironlink')?.path, '/usr/bin/ironlink');
    assert.deepStrictEqual(tried, ['/bin/ironlink', '/usr/bin/ironlink']);
  });

  test('findToolchainExecutable_when_path_is_unset_or_empty_then_returns_undefined', () => {
    assert.strictEqual(
      findToolchainExecutable(createTestEnv({ existsSync: () => true, getEnv: pathEnv('') }), 'ironlink'),
      undefined,
    );
    assert.strictEqual(findToolchainExecutable(createTestEnv({ existsSync: () => false }), 'ironlink'), undefined);
  });
});

suite('executableFileName', () => {
  test('executableFileName_when_win32_then_appends_exe', () => {
    assert.strictEqual(executableFileName('ironplcvm', 'win32'), 'ironplcvm.exe');
  });

  test('executableFileName_when_not_win32_then_is_unchanged', () => {
    assert.strictEqual(executableFileName('ironplcvm', 'linux'), 'ironplcvm');
    assert.strictEqual(executableFileName('ironplcvm', 'darwin'), 'ironplcvm');
  });
});
