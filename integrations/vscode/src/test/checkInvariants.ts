import * as fs from 'fs';
import * as path from 'path';

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

// Check declared views are actually provided: a view id contributed in
// package.json must appear in a source file, and the source must contain at
// least one view-registration call. A view that ships without a provider
// renders "no data provider registered" only after activation, which no unit
// test can catch.
const views = packageJson.contributes.views ?? {};
const viewIds: string[] = Object.values(views).flatMap((containerViews: unknown) =>
  (containerViews as { id: string }[]).map(view => view.id),
);
if (viewIds.length > 0) {
  const sourceContent = findSourceFiles(path.join(__dirname, '..', '..', 'src'))
    .map(file => fs.readFileSync(file, 'utf-8'))
    .join('\n');
  for (const viewId of viewIds) {
    if (!sourceContent.includes(viewId)) {
      failures.push(`View '${viewId}' has no provider reference in src`);
    }
  }
  const activationEvents: string[] = packageJson.activationEvents ?? [];
  for (const viewId of viewIds) {
    if (!activationEvents.includes(`onView:${viewId}`)) {
      failures.push(`View '${viewId}' has no 'onView:${viewId}' activation event`);
    }
  }
  const registrationCalls = [
    'registerTreeDataProvider',
    'createTreeView',
    'registerWebviewViewProvider',
  ];
  if (!registrationCalls.some(call => sourceContent.includes(call))) {
    failures.push(
      `contributes.views declares ${viewIds.length} view(s) but src contains no view-registration call`,
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

function findSourceFiles(dir: string): string[] {
  const results: string[] = [];
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const fullPath = path.join(dir, entry.name);
    if (entry.isDirectory()) {
      results.push(...findSourceFiles(fullPath));
    }
    else if (entry.name.endsWith('.ts')) {
      results.push(fullPath);
    }
  }
  return results;
}
