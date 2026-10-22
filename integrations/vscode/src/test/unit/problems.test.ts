import * as assert from 'assert';
import { ProblemCode, PROBLEM_MESSAGES, formatProblem } from '../../vscodeCommon';
import { DEBUG_SERVER_BINARY } from '../../debugAdapterLogic';

// The catalog itself is tested in integrations/vscode-common. These tests keep
// the messages that name this extension's own binaries in step with it.
suite('ProblemCode', () => {
  test('PROBLEM_MESSAGES_when_debug_server_not_found_then_names_the_binary', () => {
    // problem-codes.csv is not TypeScript and cannot import the constant, so
    // this is what keeps the generated message in step with the binary name.
    assert.ok(
      PROBLEM_MESSAGES[ProblemCode.DebugServerNotFound].includes(DEBUG_SERVER_BINARY),
      `E0007 must name ${DEBUG_SERVER_BINARY}, but says: `
      + PROBLEM_MESSAGES[ProblemCode.DebugServerNotFound],
    );
  });
});

suite('formatProblem', () => {
  test('formatProblem_when_debug_server_not_found_then_includes_code', () => {
    const result = formatProblem(ProblemCode.DebugServerNotFound);
    assert.strictEqual(result, `E0007 - IronPLC debug server (${DEBUG_SERVER_BINARY}) not found`);
  });
});
