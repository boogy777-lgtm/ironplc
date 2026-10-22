import * as assert from 'assert';
import * as fs from 'fs';
import * as os from 'os';
import * as path from 'path';
import { findGatewayViolations } from '../gatewayImports';

function withSourceTree(files: Record<string, string>, body: (root: string) => void): void {
  const root = fs.mkdtempSync(path.join(os.tmpdir(), 'gateway-'));
  try {
    for (const [name, text] of Object.entries(files)) {
      const file = path.join(root, name);
      fs.mkdirSync(path.dirname(file), { recursive: true });
      fs.writeFileSync(file, text);
    }
    body(root);
  }
  finally {
    fs.rmSync(root, { recursive: true, force: true });
  }
}

const GATEWAY = "export * from '../../vscode-common/out/index';\n";

suite('findGatewayViolations', () => {
  test('findGatewayViolations_when_only_the_gateway_names_the_package_then_returns_nothing', () => {
    withSourceTree({
      'vscodeCommon.ts': GATEWAY,
      'a.ts': "import { x } from './vscodeCommon';\n",
      'test/b.ts': "import { y } from '../vscodeCommon';\n",
    }, root => assert.deepStrictEqual(findGatewayViolations(root), []));
  });

  test('findGatewayViolations_when_the_gateway_is_missing_then_reports_it', () => {
    withSourceTree({ 'a.ts': 'export {};\n' }, root => {
      const failures = findGatewayViolations(root);
      assert.strictEqual(failures.length, 1);
      assert.match(failures[0], /vscodeCommon\.ts.*missing/);
    });
  });

  test('findGatewayViolations_when_a_source_imports_the_package_by_path_then_names_that_file', () => {
    withSourceTree({
      'vscodeCommon.ts': GATEWAY,
      'nested/c.ts': "import { z } from '../../../vscode-common/out/index';\n",
      'd.ts': "const z = require('../../vscode-common/out/index');\n",
    }, root => {
      const failures = findGatewayViolations(root).sort();
      assert.strictEqual(failures.length, 2);
      assert.match(failures[0], /^d\.ts imports the shared package by path/);
      assert.ok(failures[1].includes(path.join('nested', 'c.ts')));
    });
  });
});
