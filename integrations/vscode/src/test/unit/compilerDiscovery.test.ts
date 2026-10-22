import * as assert from 'assert';
import { ToolchainDiscoveryResult } from '../../vscodeCommon';
import { formatStartFailure } from '../../compilerDiscovery';

suite('formatStartFailure', () => {
  const result: ToolchainDiscoveryResult = {
    path: 'C:\\Program Files\\IronPLC Compiler\\bin\\ironplcc.exe',
    source: 'localappdata',
  };

  test('formatStartFailure_when_error_then_includes_path', () => {
    const message = formatStartFailure(result, new Error('spawn ENOEXEC'));
    assert.ok(message.includes(result.path), 'message should include the compiler path');
  });

  test('formatStartFailure_when_error_then_includes_source', () => {
    const message = formatStartFailure(result, new Error('spawn ENOEXEC'));
    assert.ok(message.includes('localappdata'), 'message should include the discovery source');
  });

  test('formatStartFailure_when_error_then_includes_reason', () => {
    const message = formatStartFailure(result, new Error('spawn ENOEXEC'));
    assert.ok(message.includes('spawn ENOEXEC'), 'message should include the error reason');
  });

  test('formatStartFailure_when_non_error_then_stringifies_reason', () => {
    const message = formatStartFailure(result, 'plain failure');
    assert.ok(message.includes('plain failure'), 'non-Error reasons should be stringified');
  });
});
