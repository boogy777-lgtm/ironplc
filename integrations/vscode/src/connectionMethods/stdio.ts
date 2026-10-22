/**
 * The `stdio` connection method: spawn a local `ironplcvm serve` on the
 * profile's `program` and talk to it over the child's stdin/stdout. The
 * child is never respawned, and starting it with a container is a download.
 */

import { spawn } from 'child_process';
import * as path from 'path';

import type { Anchor } from '../baselineLogic';
import { programKind } from '../debugAdapterLogic';
import { ProblemCode } from '../vscodeCommon';
import { StdioLineTransport, vmPathBeside } from '../stdioTransport';
import { compileSourceToContainer } from '../taskProviderLogic';
import type { ConnectionMethod } from '../connectionMethods';

export const stdioMethod: ConnectionMethod = {
  checkFields(fields) {
    if (fields.program === undefined || fields.program.trim().length === 0) {
      return { code: ProblemCode.ConnectionProfileInvalid, detail: 'a stdio profile needs the program the spawned session loads' };
    }
    if (fields.address !== undefined) {
      return { code: ProblemCode.ConnectionAddressInvalid, detail: 'a stdio profile spawns a local session and must not set an address' };
    }
    if (fields.port !== undefined) {
      return { code: ProblemCode.ConnectionPortInvalid, detail: 'a stdio profile spawns a local session and must not set a port' };
    }
    return undefined;
  },

  needsLocalTools: true,

  detail: fields => fields.program,

  async open(fields, context) {
    // The serve session loads a compiled container, so a source program is
    // compiled first — the same single-file compile every other path uses.
    let container = fields.program!;
    if (programKind(container, context.sourceExtensions) === 'source') {
      container = await compileSourceToContainer(
        context.compilerPath!,
        container,
        text => context.output.append(text),
        context.compilerSettings(),
      );
    }
    const vmPath = vmPathBeside(context.compilerPath!, process.platform);
    context.output.appendLine(`$ ${vmPath} serve ${container}`);
    const child = spawn(vmPath, ['serve', container]);
    child.stderr.on('data', (chunk: Buffer) => context.output.append(chunk.toString()));
    return {
      transport: new StdioLineTransport(child),
      close: () => {
        child.kill();
      },
    };
  },

  // The spawned child is never respawned: a fault goes straight to E0014.
  reconnects: false,

  // A session started with a container is a download.
  recordsSessionStart: true,

  /**
   * A `program` that is a container: that container, which the session
   * serves. A source file: none, since the session compiles it to a container
   * of its own that the engineer has no file for.
   */
  anchor(fields, _folders, sourceExtensions): Anchor {
    const program = fields.program ?? '';
    if (programKind(program, sourceExtensions) === 'container') {
      return { anchor: path.resolve(program) };
    }
    return { reason: `the connection "${fields.name}" runs ${program}, a program that the session compiles itself: there is no application container to name a record after` };
  },
};
