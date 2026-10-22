import * as fs from 'fs';
import * as path from 'path';

/**
 * Checks that an extension reaches this package through exactly one module,
 * `<sourceRoot>/vscodeCommon.ts`, so the relative path to the package is
 * written once. Returns one failure message per violation: the gateway module
 * is missing, or another source or test names the package's path itself. An
 * empty array means the extension is consistent. Each extension's
 * `checkInvariants` calls this rather than carrying its own copy.
 */
export function findGatewayViolations(sourceRoot: string): string[] {
  const failures: string[] = [];
  const gateway = path.join(sourceRoot, 'vscodeCommon.ts');
  if (!fs.existsSync(gateway)) {
    failures.push('src/vscodeCommon.ts, the one module that imports the shared package, is missing');
  }
  const packageImport = /(?:from|require[(])[ ]*['"][^'"]*vscode-common[^'"]*['"]/;
  for (const file of findTypeScriptFiles(sourceRoot)) {
    if (file !== gateway && packageImport.test(fs.readFileSync(file, 'utf-8'))) {
      failures.push(`${path.relative(sourceRoot, file)} imports the shared package by path; import it from vscodeCommon instead`);
    }
  }
  return failures;
}

function findTypeScriptFiles(dir: string): string[] {
  const results: string[] = [];
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const fullPath = path.join(dir, entry.name);
    if (entry.isDirectory()) {
      results.push(...findTypeScriptFiles(fullPath));
    }
    else if (entry.name.endsWith('.ts')) {
      results.push(fullPath);
    }
  }
  return results;
}
