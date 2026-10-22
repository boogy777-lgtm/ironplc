/**
 * Code shared by IronPLC's VS Code extensions. Everything the extensions
 * import from this package is exported here.
 */
export { pathForPlatform } from './platformPath';
export type { ToolchainDiscoveryResult, ToolchainEnvironment } from './toolchainDiscovery';
export { executableFileName, findToolchainExecutable } from './toolchainDiscovery';
