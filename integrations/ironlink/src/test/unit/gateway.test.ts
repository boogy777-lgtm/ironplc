import * as assert from 'assert';
import * as path from 'path';

import { findGatewayViolations } from '../../vscodeCommon';

// REQ-IL-ironlink-vscode-095
suite('shared package gateway', () => {
  test('findGatewayViolations_when_the_extension_sources_are_checked_then_only_vscodeCommon_names_the_package', () => {
    // out/test/unit -> the extension root -> src
    const sourceRoot = path.join(__dirname, '..', '..', '..', 'src');
    assert.deepStrictEqual(findGatewayViolations(sourceRoot), []);
  });
});
