import * as assert from 'assert';

import { IronLinkCommandError, IronLinkProblem, describeFailure } from '../../failures';
import { ProblemCode } from '../../vscodeCommon';

// REQ-IL-ironlink-vscode-082
suite('describeFailure', () => {
  test('describeFailure_when_command_error_then_text_has_the_l_code_and_the_help_url_of_that_code', () => {
    const shown = describeFailure(new IronLinkCommandError('L6004', "'x' is not an IP address or IP:PORT"), '0.247.0')!;
    assert.strictEqual(shown.text, "L6004 - 'x' is not an IP address or IP:PORT");
    assert.strictEqual(
      shown.helpUrl,
      'https://www.ironplc.com/reference/ironlink/problems/L6004.html?version=0.247.0&channel=extension',
    );
  });

  test('describeFailure_when_extension_problem_then_text_comes_from_the_catalog_and_links_the_e_code', () => {
    const shown = describeFailure(new IronLinkProblem(ProblemCode.IronLinkNotFound, 'set ironlink.path'), '0.247.0')!;
    assert.strictEqual(shown.text, 'E0016 - IronLink program (ironlink) not found. set ironlink.path');
    assert.strictEqual(
      shown.helpUrl,
      'https://www.ironplc.com/reference/editor/problems/E0016.html?version=0.247.0&channel=extension',
    );
  });

  test('describeFailure_when_problem_has_no_detail_then_text_is_code_and_message_only', () => {
    assert.strictEqual(
      describeFailure(new IronLinkProblem(ProblemCode.IronLinkExited), '1')?.text,
      'E0018 - The IronLink program exited',
    );
  });

  test('describeFailure_when_not_an_ironlink_failure_then_it_is_not_given_a_code', () => {
    assert.strictEqual(describeFailure(new Error('boom'), '1'), undefined);
    assert.strictEqual(describeFailure(new TypeError('x is undefined'), '1'), undefined);
    assert.strictEqual(describeFailure('a string', '1'), undefined);
  });

  test('describeFailure_when_protocol_problem_then_it_carries_e0019', () => {
    const shown = describeFailure(new IronLinkProblem(ProblemCode.IronLinkProtocolError, 'the line is not JSON'), '1');
    assert.strictEqual(
      shown?.text,
      'E0019 - The IronLink program sent a response the extension cannot read. the line is not JSON',
    );
    assert.ok(shown?.helpUrl.includes('/E0019.html'));
  });

  test('IronLinkCommandError_when_built_then_carries_the_l_code_and_the_reason', () => {
    const error = new IronLinkCommandError('L6006', 'routes.json is unreadable');
    assert.strictEqual(error.lCode, 'L6006');
    assert.strictEqual(error.reason, 'routes.json is unreadable');
    assert.ok(error instanceof Error);
  });
});
