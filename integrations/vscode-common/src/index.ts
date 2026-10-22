/**
 * Code shared by IronPLC's VS Code extensions. Everything the extensions
 * import from this package is exported here.
 */
export { pathForPlatform } from './platformPath';
export type { ToolchainDiscoveryResult, ToolchainEnvironment } from './toolchainDiscovery';
export { executableFileName, findToolchainExecutable } from './toolchainDiscovery';
export { ProblemCode, PROBLEM_MESSAGES, formatProblem } from './problems';
export { problemHelpUrl } from './problemUrl';
export type { LineTransport } from './lineTransport';
export { StdioLineTransport } from './lineTransport';
export type { LineRequestChannelErrors } from './lineRequestChannel';
export { LineRequestChannel } from './lineRequestChannel';
export { findGatewayViolations } from './gatewayImports';
export type {
  ChooseControllerArgument,
  CommandSources,
  ConnectControllerArgument,
  ControllerArgument,
  ExecuteCommand,
  HandoffArguments,
  HandoffDeclaration,
  HandoffName,
} from './handoff';
export { HANDOFF_ACTIONS, contributedCommandIds, handoffNames, isControllerArgument, isHandoffAvailable, runHandoff } from './handoff';
export type {
  IronLinkApi,
  IronLinkRoute,
  BrowseSnapshot,
  ObservedDevice,
  RouteFailure,
  RouteOutcome,
  RouteSet,
} from './ironlinkApi';
export { IRONLINK_API_MEMBERS, IRONLINK_API_VERSION, IRONLINK_EXTENSION_ID } from './ironlinkApi';
export { formatAge, shortDeviceId, socketAddress } from './deviceText';
