import * as assert from 'assert';

import {
  ConnectionProfile,
  DEFAULT_TCP_PORT,
  isValidAddress,
  validateProfiles,
} from '../../connectionProfiles';
import { ProblemCode } from '../../problems';

suite('validateProfiles', () => {
  const STDIO_PROFILE = { name: 'Local VM', transport: 'stdio', program: '${workspaceFolder}/main.st' };
  const TCP_PROFILE = { name: 'Cell 1', transport: 'tcp', address: '192.168.1.10', port: 49152 };

  test('validateProfiles_when_empty_array_then_no_profiles_no_errors', () => {
    const result = validateProfiles([]);
    assert.deepStrictEqual(result.profiles, []);
    assert.deepStrictEqual(result.errors, []);
  });

  test('validateProfiles_when_non_array_then_no_profiles_no_errors', () => {
    const result = validateProfiles(undefined);
    assert.deepStrictEqual(result.profiles, []);
    assert.deepStrictEqual(result.errors, []);
  });

  test('validateProfiles_when_valid_stdio_profile_then_profile_with_defaults', () => {
    const result = validateProfiles([STDIO_PROFILE]);
    assert.deepStrictEqual(result.errors, []);
    assert.strictEqual(result.profiles.length, 1);
    const profile = result.profiles[0];
    assert.strictEqual(profile.name, 'Local VM');
    assert.strictEqual(profile.transport, 'stdio');
    assert.strictEqual(profile.program, '${workspaceFolder}/main.st');
    assert.strictEqual(profile.port, undefined);
  });

  test('validateProfiles_when_tcp_profile_omits_port_then_defaults_to_49152', () => {
    const result = validateProfiles([{ name: 'Cell 1', transport: 'tcp', address: 'plc-01' }]);
    assert.deepStrictEqual(result.errors, []);
    assert.strictEqual(result.profiles[0].port, DEFAULT_TCP_PORT);
  });

  test('validateProfiles_when_profile_not_an_object_then_e0010', () => {
    const result = validateProfiles(['nope']);
    assert.strictEqual(result.profiles.length, 0);
    assert.strictEqual(result.errors.length, 1);
    assert.strictEqual(result.errors[0].code, ProblemCode.ConnectionProfileInvalid);
    assert.strictEqual(result.errors[0].profile, '#1');
  });

  test('validateProfiles_when_name_missing_then_e0010', () => {
    const result = validateProfiles([{ transport: 'stdio', program: 'main.st' }]);
    assert.strictEqual(result.errors.length, 1);
    assert.strictEqual(result.errors[0].code, ProblemCode.ConnectionProfileInvalid);
    assert.ok(result.errors[0].detail.includes('name'));
  });

  test('validateProfiles_when_name_blank_then_e0010', () => {
    const result = validateProfiles([{ name: '  ', transport: 'stdio', program: 'main.st' }]);
    assert.strictEqual(result.errors.length, 1);
    assert.strictEqual(result.errors[0].code, ProblemCode.ConnectionProfileInvalid);
  });

  test('validateProfiles_when_duplicate_name_then_e0010_on_later_profile', () => {
    const result = validateProfiles([STDIO_PROFILE, { ...TCP_PROFILE, name: 'Local VM' }]);
    assert.strictEqual(result.profiles.length, 1);
    assert.strictEqual(result.errors.length, 1);
    assert.strictEqual(result.errors[0].code, ProblemCode.ConnectionProfileInvalid);
    assert.strictEqual(result.errors[0].profile, 'Local VM');
  });

  test('validateProfiles_when_unknown_transport_then_e0010', () => {
    const result = validateProfiles([{ name: 'X', transport: 'serial' }]);
    assert.strictEqual(result.errors.length, 1);
    assert.strictEqual(result.errors[0].code, ProblemCode.ConnectionProfileInvalid);
    assert.ok(result.errors[0].detail.includes('transport'));
  });

  test('validateProfiles_when_stdio_without_program_then_e0010', () => {
    const result = validateProfiles([{ name: 'Local VM', transport: 'stdio' }]);
    assert.strictEqual(result.errors.length, 1);
    assert.strictEqual(result.errors[0].code, ProblemCode.ConnectionProfileInvalid);
    assert.ok(result.errors[0].detail.includes('program'));
  });

  test('validateProfiles_when_tcp_without_address_then_e0010', () => {
    const result = validateProfiles([{ name: 'Cell 1', transport: 'tcp' }]);
    assert.strictEqual(result.errors.length, 1);
    assert.strictEqual(result.errors[0].code, ProblemCode.ConnectionProfileInvalid);
    assert.ok(result.errors[0].detail.includes('address'));
  });

  test('validateProfiles_when_stdio_with_address_then_e0011', () => {
    const result = validateProfiles([{ ...STDIO_PROFILE, address: '192.168.1.10' }]);
    assert.strictEqual(result.errors.length, 1);
    assert.strictEqual(result.errors[0].code, ProblemCode.ConnectionAddressInvalid);
  });

  test('validateProfiles_when_stdio_with_port_then_e0012', () => {
    const result = validateProfiles([{ ...STDIO_PROFILE, port: 49152 }]);
    assert.strictEqual(result.errors.length, 1);
    assert.strictEqual(result.errors[0].code, ProblemCode.ConnectionPortInvalid);
  });

  test('validateProfiles_when_address_malformed_then_e0011', () => {
    for (const address of ['999.1.1.1', 'not a host!', '-bad.example.com', 'bad-.example.com', 'under_score.example.com']) {
      const result = validateProfiles([{ name: 'Cell 1', transport: 'tcp', address }]);
      assert.strictEqual(result.errors.length, 1, `expected one error for ${address}`);
      assert.strictEqual(result.errors[0].code, ProblemCode.ConnectionAddressInvalid, `expected E0011 for ${address}`);
    }
  });

  test('validateProfiles_when_address_valid_then_no_error', () => {
    for (const address of ['192.168.1.10', '0.0.0.0', '255.255.255.255', '::1', 'fd00::1', 'plc-01', 'a.b.example.com']) {
      const result = validateProfiles([{ name: 'Cell 1', transport: 'tcp', address }]);
      assert.deepStrictEqual(result.errors, [], `expected no error for ${address}`);
      assert.strictEqual(result.profiles.length, 1);
    }
  });

  test('validateProfiles_when_port_out_of_range_then_e0012', () => {
    for (const port of [0, 65536, -1, 3.5, '49152']) {
      const result = validateProfiles([{ ...TCP_PROFILE, port }]);
      assert.strictEqual(result.errors.length, 1, `expected one error for port ${port}`);
      assert.strictEqual(result.errors[0].code, ProblemCode.ConnectionPortInvalid);
    }
  });

  test('validateProfiles_when_credentials_key_not_a_string_then_e0010', () => {
    const result = validateProfiles([{ ...STDIO_PROFILE, credentialsKey: 42 }]);
    assert.strictEqual(result.errors.length, 1);
    assert.strictEqual(result.errors[0].code, ProblemCode.ConnectionProfileInvalid);
  });

  test('validateProfiles_when_credentials_key_present_then_profile_carries_it', () => {
    const result = validateProfiles([{ ...STDIO_PROFILE, credentialsKey: 'ironplc-connection/local' }]);
    assert.deepStrictEqual(result.errors, []);
    assert.strictEqual(result.profiles[0].credentialsKey, 'ironplc-connection/local');
  });

  test('validateProfiles_when_multiple_errors_then_collects_all', () => {
    const result = validateProfiles([
      { transport: 'stdio' },
      { name: 'Cell 1', transport: 'tcp', address: '999.1.1.1', port: 70000 },
      TCP_PROFILE,
    ]);
    assert.strictEqual(result.errors.length, 2);
    assert.strictEqual(result.profiles.length, 1);
    assert.strictEqual(result.profiles[0].name, 'Cell 1');
  });
});

suite('isValidAddress', () => {
  test('isValidAddress_when_ipv4_then_true', () => {
    assert.strictEqual(isValidAddress('192.168.1.10'), true);
  });

  test('isValidAddress_when_ipv6_then_true', () => {
    assert.strictEqual(isValidAddress('fd00::1'), true);
  });

  test('isValidAddress_when_hostname_then_true', () => {
    assert.strictEqual(isValidAddress('plc-01.example.com'), true);
  });

  test('isValidAddress_when_hostname_label_edges_then_true', () => {
    assert.strictEqual(isValidAddress('a'), true);
    assert.strictEqual(isValidAddress(`${'a'.padEnd(63, 'a')}.example.com`), true);
  });

  test('isValidAddress_when_empty_or_too_long_then_false', () => {
    assert.strictEqual(isValidAddress(''), false);
    assert.strictEqual(isValidAddress(`${'a'.padEnd(254, 'a')}`), false);
  });
});

suite('ConnectionProfile', () => {
  test('ConnectionProfile_when_defined_then_compiles', () => {
    const profile: ConnectionProfile = { name: 'x', transport: 'tcp', address: '::1', port: DEFAULT_TCP_PORT };
    assert.strictEqual(profile.transport, 'tcp');
  });
});
