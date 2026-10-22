import * as fs from 'fs';
import * as path from 'path';

import { DEFAULT_SESSION_PORT, SUPPORTED_SESSION_PROTOCOL } from '../sessionConstants';

const packageJson = JSON.parse(fs.readFileSync(path.join(__dirname, '..', '..', 'package.json'), 'utf-8'));

// Collect all test file contents
const testFiles = findTestFiles(path.join(__dirname));
const testContent = testFiles.map(f => fs.readFileSync(f, 'utf-8')).join('\n');

const failures: string[] = [];

// Check languages with extensions
for (const lang of packageJson.contributes.languages) {
  if (lang.extensions && lang.extensions.length > 0) {
    if (!testContent.includes(lang.id)) {
      failures.push(`Language '${lang.id}' has no test reference`);
    }
  }
}

// Check that languages with extensions have a grammar assigned
const languagesWithGrammars = new Set<string>(
  packageJson.contributes.grammars.map((g: { language: string }) => g.language),
);
for (const lang of packageJson.contributes.languages) {
  if (lang.extensions && lang.extensions.length > 0) {
    if (!languagesWithGrammars.has(lang.id)) {
      failures.push(`Language '${lang.id}' has no grammar assigned`);
    }
  }
}

// Check commands
for (const cmd of packageJson.contributes.commands) {
  if (!testContent.includes(cmd.command)) {
    failures.push(`Command '${cmd.command}' has no test reference`);
  }
}

// Check custom editors
for (const editor of packageJson.contributes.customEditors) {
  if (!testContent.includes(editor.viewType)) {
    failures.push(`Custom editor '${editor.viewType}' has no test reference`);
  }
}

// Check task definitions
if (packageJson.contributes.taskDefinitions) {
  for (const taskDef of packageJson.contributes.taskDefinitions) {
    if (!testContent.includes(taskDef.type)) {
      failures.push(`Task definition '${taskDef.type}' has no test reference`);
    }
  }
}

// Check that every hand-written statement of the default port agrees with the
// generated constant. Each file below must state a default at least once, so
// rewording a sentence cannot silently switch its check off.
const repoRoot = path.join(__dirname, '..', '..', '..', '..');
const designText = fs.readFileSync(path.join(repoRoot, 'specs', 'design', 'engineering-connection.md'), 'utf-8');
const portStatements: Array<[string, string]> = [
  ['package.json', JSON.stringify(packageJson)],
  ['specs/design/engineering-connection.md', designText],
];
for (const [file, text] of portStatements) {
  const stated = [...text.matchAll(/\bdefaults?(?:\s+to)?\s+(\d+)/gi)].map(m => Number(m[1]));
  if (stated.length === 0) {
    failures.push(`${file} states no default port`);
  }
  for (const port of stated) {
    if (port !== DEFAULT_SESSION_PORT) {
      failures.push(`${file} states default port ${port}, but the session port is ${DEFAULT_SESSION_PORT}`);
    }
  }
}

// The design document's identity example states the protocol version; it must
// agree with the generated constant. The example must be there, so rewording it
// cannot silently switch the check off.
const protocolsStated = [...designText.matchAll(/"protocol":\s*(\d+)/g)].map(m => Number(m[1]));
if (protocolsStated.length === 0) {
  failures.push('specs/design/engineering-connection.md states no session protocol version');
}
for (const protocol of protocolsStated) {
  if (protocol !== SUPPORTED_SESSION_PROTOCOL) {
    failures.push(
      `specs/design/engineering-connection.md states session protocol ${protocol}, but the protocol is ${SUPPORTED_SESSION_PROTOCOL}`,
    );
  }
}

if (failures.length > 0) {
  console.error('Test coverage invariant failures:');
  failures.forEach(f => console.error(`  - ${f}`));
  process.exit(1);
}
else {
  console.log('All test coverage invariants satisfied.');
}

function findTestFiles(dir: string): string[] {
  const results: string[] = [];
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const fullPath = path.join(dir, entry.name);
    if (entry.isDirectory()) {
      results.push(...findTestFiles(fullPath));
    }
    else if (entry.name.endsWith('.js')) {
      results.push(fullPath);
    }
  }
  return results;
}
