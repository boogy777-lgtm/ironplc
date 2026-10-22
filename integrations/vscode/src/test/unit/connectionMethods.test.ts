import * as assert from 'assert';

import {
  CONNECTION_METHODS,
  connectionMethod,
  connectionTransports,
  describeTransports,
  isConnectionTransport,
} from '../../connectionMethods';
import { ProblemCode } from '../../vscodeCommon';
import { DEFAULT_SESSION_PORT } from '../../sessionConstants';

suite('connectionMethods table', () => {
  test('connectionTransports_when_listed_then_stdio_and_tcp_in_order', () => {
    assert.deepStrictEqual(connectionTransports(), ['stdio', 'tcp']);
  });

  test('isConnectionTransport_when_a_method_name_then_true', () => {
    assert.strictEqual(isConnectionTransport('stdio'), true);
    assert.strictEqual(isConnectionTransport('tcp'), true);
  });

  test('isConnectionTransport_when_not_a_method_name_then_false', () => {
    assert.strictEqual(isConnectionTransport('usb'), false);
    assert.strictEqual(isConnectionTransport(undefined), false);
    assert.strictEqual(isConnectionTransport(7), false);
  });

  test('isConnectionTransport_when_an_inherited_object_member_then_false', () => {
    assert.strictEqual(isConnectionTransport('toString'), false);
    assert.strictEqual(isConnectionTransport('__proto__'), false);
  });

  test('describeTransports_when_two_methods_then_quoted_alternative', () => {
    assert.strictEqual(describeTransports(), '"stdio" or "tcp"');
  });

  test('connectionMethod_when_profile_then_the_member_of_its_transport', () => {
    assert.strictEqual(connectionMethod({ transport: 'stdio' }), CONNECTION_METHODS.stdio);
    assert.strictEqual(connectionMethod({ transport: 'tcp' }), CONNECTION_METHODS.tcp);
  });
});

suite('stdio connection method', () => {
  const stdio = CONNECTION_METHODS.stdio;

  test('checkFields_when_program_set_then_no_fault', () => {
    assert.strictEqual(stdio.checkFields({ name: 'a', program: 'main.st' }), undefined);
  });

  test('checkFields_when_program_blank_then_profile_invalid', () => {
    assert.strictEqual(stdio.checkFields({ name: 'a', program: '  ' })?.code, ProblemCode.ConnectionProfileInvalid);
  });

  test('checkFields_when_address_set_then_address_invalid', () => {
    assert.strictEqual(stdio.checkFields({ name: 'a', program: 'main.st', address: '10.0.0.1' })?.code, ProblemCode.ConnectionAddressInvalid);
  });

  test('checkFields_when_port_set_then_port_invalid', () => {
    assert.strictEqual(stdio.checkFields({ name: 'a', program: 'main.st', port: 5 })?.code, ProblemCode.ConnectionPortInvalid);
  });

  test('stdio_when_declared_then_needs_tools_records_session_start_and_does_not_reconnect', () => {
    assert.strictEqual(stdio.needsLocalTools, true);
    assert.strictEqual(stdio.recordsSessionStart, true);
    assert.strictEqual(stdio.reconnects, false);
    assert.strictEqual(stdio.defaultPort, undefined);
  });

  test('detail_when_stdio_profile_then_the_program', () => {
    assert.strictEqual(stdio.detail({ name: 'a', program: 'main.st' }), 'main.st');
  });
});

suite('tcp connection method', () => {
  const tcp = CONNECTION_METHODS.tcp;

  test('checkFields_when_address_valid_then_no_fault', () => {
    assert.strictEqual(tcp.checkFields({ name: 'a', address: '192.168.1.10' }), undefined);
  });

  test('checkFields_when_address_missing_then_profile_invalid', () => {
    assert.strictEqual(tcp.checkFields({ name: 'a' })?.code, ProblemCode.ConnectionProfileInvalid);
  });

  test('checkFields_when_address_malformed_then_address_invalid', () => {
    assert.strictEqual(tcp.checkFields({ name: 'a', address: '999.1.1.1' })?.code, ProblemCode.ConnectionAddressInvalid);
  });

  test('tcp_when_declared_then_needs_no_tools_reconnects_and_has_the_session_port', () => {
    assert.strictEqual(tcp.needsLocalTools, false);
    assert.strictEqual(tcp.recordsSessionStart, false);
    assert.strictEqual(tcp.reconnects, true);
    assert.strictEqual(tcp.defaultPort, DEFAULT_SESSION_PORT);
  });

  test('detail_when_tcp_profile_then_address_and_port', () => {
    assert.strictEqual(tcp.detail({ name: 'a', address: '10.0.0.1', port: 502 }), '10.0.0.1:502');
  });
});
