import { pathForPlatform } from './platformPath';

/**
 * Injected environment for toolchain discovery. Keeping the platform,
 * filesystem, environment variables and settings behind an interface makes the
 * resolution order testable without a real machine, and the injected
 * `platform` decides how paths are composed (see `pathForPlatform`).
 */
export interface ToolchainEnvironment {
  platform: string;
  existsSync: (path: string) => boolean;
  getEnv: (name: string) => string | undefined;
  getConfig: (key: string) => string | undefined;
}

/** Where a toolchain executable was found, and by which trial. */
export interface ToolchainDiscoveryResult {
  path: string;
  source: string;
}

/** The file name of the executable `base` on `platform` (`.exe` on Windows). */
export function executableFileName(base: string, platform: string): string {
  return platform === 'win32' ? base + '.exe' : base;
}

/**
 * Finds the toolchain executable named `executable` (the base name, such as
 * `ironplcc` or `ironplcvm`, without a platform extension). The trials run in
 * this order and the first directory that contains the executable wins:
 *
 * 1. the `path` setting (`configuration`),
 * 2. the `IRONPLC` environment variable (`environment`),
 * 3. `/opt/homebrew/bin` on macOS (`homebrew`),
 * 4. `%LOCALAPPDATA%\Programs\IronPLC Compiler\bin` on Windows
 *    (`localappdata`),
 * 5. each directory of the system `PATH`, in order (`path`). `PATH` is split
 *    with the delimiter of the injected platform, and an empty entry is
 *    skipped. It is the last trial, so every location above still wins.
 *
 * Returns `undefined` when no trial finds the executable.
 */
export function findToolchainExecutable(
  env: ToolchainEnvironment,
  executable: string,
): ToolchainDiscoveryResult | undefined {
  const fileName = executableFileName(executable, env.platform);
  const platformPath = pathForPlatform(env.platform);

  // Each trial yields the candidate directories for its source, in order.
  const trialGenerator: (() => [(string | undefined)[], string])[] = [
    () => {
      // Try to get from configuration
      return [[env.getConfig('path')], 'configuration'];
    },
    () => {
      // Try to get from environment variable. Not generally set.
      return [[env.getEnv('IRONPLC')], 'environment'];
    },
    () => {
      // Mac well known directory
      const homebrewDir = env.platform === 'darwin' ? '/opt/homebrew/bin' : undefined;
      return [[homebrewDir], 'homebrew'];
    },
    () => {
      // Windows user-install well-known path
      const name = 'localappdata';
      const localAppData = env.getEnv('LOCALAPPDATA');

      if (env.platform !== 'win32' || !localAppData) {
        return [[], name];
      }
      const winAppDataDir = platformPath.join(localAppData, 'Programs', 'IronPLC Compiler', 'bin');
      return [[winAppDataDir], name];
    },
    () => {
      // The system PATH, last so every location above still wins. The
      // delimiter belongs to the injected platform, not to the host.
      return [(env.getEnv('PATH') ?? '').split(platformPath.delimiter), 'path'];
    },
  ];

  for (const trial of trialGenerator) {
    const [testDirs, trialType] = trial();

    for (const testDir of testDirs) {
      if (!testDir) {
        continue;
      }

      const testExe = platformPath.join(testDir, fileName);
      if (env.existsSync(testExe)) {
        return { path: testExe, source: trialType };
      }
    }
  }

  return undefined;
}
