import * as fs from 'fs';
import * as path from 'path';

import { DEVICE_ACTION_COMMANDS } from '../deviceActions';
import { CONTEXT_VALUES } from '../networkTree';
import { findGatewayViolations, handoffNames } from '../vscodeCommon';

// out/test/checkInvariants.js -> the extension root, and the repository root.
const extensionRoot = path.join(__dirname, '..', '..');
const sourceRoot = path.join(extensionRoot, 'src');
const repoRoot = path.join(extensionRoot, '..', '..');

const packageJson = JSON.parse(fs.readFileSync(path.join(extensionRoot, 'package.json'), 'utf-8'));
const ironplcPackageJson = JSON.parse(
  fs.readFileSync(path.join(repoRoot, 'integrations', 'vscode', 'package.json'), 'utf-8'),
);
const designDoc = 'specs/design/ironlink-vscode-extension.md';
const designText = fs.readFileSync(path.join(repoRoot, ...designDoc.split('/')), 'utf-8');
const extensionSource = fs.readFileSync(path.join(sourceRoot, 'extension.ts'), 'utf-8');

// Collect all test file contents
const testFiles = findFiles(path.join(__dirname), '.js');
const testContent = testFiles.map(f => fs.readFileSync(f, 'utf-8')).join('\n');

const failures: string[] = [];

// The version is the toolchain's, and the editor engine is the IronPLC
// extension's: the two extensions are released together.
if (packageJson.version !== ironplcPackageJson.version) {
  failures.push(`The version is ${packageJson.version}, but the IronPLC extension's is ${ironplcPackageJson.version}`);
}
if (packageJson.engines.vscode !== ironplcPackageJson.engines.vscode) {
  failures.push(
    `engines.vscode is ${packageJson.engines.vscode}, but the IronPLC extension's is ${ironplcPackageJson.engines.vscode}`,
  );
}

// Every command has a test reference, and a registration. The commands are
// registered either by a literal id or through the table of device actions.
const declared: string[] = packageJson.contributes.commands.map((cmd: { command: string }) => cmd.command);
const registered = new Set<string>([
  ...[...extensionSource.matchAll(/register\('(ironlink\.[A-Za-z]+)'/g)].map(m => m[1]),
  ...Object.values(DEVICE_ACTION_COMMANDS),
]);
for (const command of declared) {
  if (!testContent.includes(command)) {
    failures.push(`Command '${command}' has no test reference`);
  }
  if (!registered.has(command)) {
    failures.push(`Command '${command}' is declared in package.json but not registered by extension.ts`);
  }
}
for (const command of registered) {
  if (!declared.includes(command)) {
    failures.push(`Command '${command}' is registered but not declared in package.json`);
  }
}

// Every device action is on the device row's context menu, and is hidden from
// the command palette, where there is no row to act on.
const menus = packageJson.contributes.menus;
for (const command of Object.values(DEVICE_ACTION_COMMANDS)) {
  const onRow = menus['view/item/context'].some(
    (item: { command: string; when: string }) => item.command === command && item.when.includes('viewItem == device'),
  );
  if (!onRow) {
    failures.push(`Device action '${command}' is not on the context menu of a device row`);
  }
  const hidden = menus.commandPalette.some(
    (item: { command: string; when: string }) => item.command === command && item.when === 'false',
  );
  if (!hidden) {
    failures.push(`Device action '${command}' is not hidden from the command palette`);
  }
}

// Every handoff action of the shared table has a device action that offers it.
for (const name of handoffNames()) {
  if (!(name in DEVICE_ACTION_COMMANDS)) {
    failures.push(`Handoff action '${name}' has no device action`);
  }
}

// A `viewItem` a menu tests must be a context value the tree builder can make.
const contextValues = new Set<string>(Object.values(CONTEXT_VALUES));
for (const group of Object.values(menus) as Array<Array<{ when?: string }>>) {
  for (const item of group) {
    for (const match of (item.when ?? '').matchAll(/viewItem == ([\w.]+)/g)) {
      if (!contextValues.has(match[1]) && !/^driver\.[A-Za-z]+$/.test(match[1])) {
        failures.push(`A menu tests viewItem '${match[1]}', which the tree builder never produces`);
      }
    }
  }
}

// Every setting is in the `ironlink` namespace and is read from its section by
// the extension (`ironlink.browse.timeoutMs` from `getConfiguration('ironlink.browse')`).
for (const setting of Object.keys(packageJson.contributes.configuration.properties)) {
  const section = setting.slice(0, setting.lastIndexOf('.'));
  if (!section.startsWith('ironlink') || !extensionSource.includes(`getConfiguration('${section}')`)) {
    failures.push(`Setting '${setting}' is not read by extension.ts from getConfiguration('${section}')`);
  }
}

// The requirements of the extension are in its own design document and in the
// tests, both ways. No Rust crate lists that document (the Rust guard requires
// every requirement of a listed document to belong to a listing crate), so the
// completeness check is here.
const idPattern = /REQ-IL-ironlink-vscode-\d{3}/g;
const specified = new Set([...designText.matchAll(/\*\*(REQ-IL-ironlink-vscode-\d{3})\*\*/g)].map(m => m[1]));
const tested = new Set(testContent.match(idPattern) ?? []);
if (specified.size === 0) {
  failures.push(`${designDoc} specifies no requirement of the VS Code extension`);
}
for (const id of specified) {
  if (!tested.has(id)) {
    failures.push(`${id} is specified but no test names it`);
  }
}
for (const id of tested) {
  if (!specified.has(id)) {
    failures.push(`${id} is named by a test but not specified in ${designDoc}`);
  }
}

// The shared package (integrations/vscode-common) is reached through exactly
// one module, src/vscodeCommon.ts, so the relative path to it is written once.
// The shared package owns the check, so every extension applies the same one.
failures.push(...findGatewayViolations(sourceRoot));

if (failures.length > 0) {
  console.error('Test coverage invariant failures:');
  failures.forEach(f => console.error(`  - ${f}`));
  process.exit(1);
}
else {
  console.log('All test coverage invariants satisfied.');
}

function findFiles(dir: string, extension: string): string[] {
  const results: string[] = [];
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const fullPath = path.join(dir, entry.name);
    if (entry.isDirectory()) {
      results.push(...findFiles(fullPath, extension));
    }
    else if (entry.name.endsWith(extension)) {
      results.push(fullPath);
    }
  }
  return results;
}
