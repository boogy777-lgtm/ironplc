import { readFile } from 'fs/promises';

/** The text of a file, or undefined when there is no such file. */
export async function readTextIfPresent(file: string): Promise<string | undefined> {
  try {
    return await readFile(file, 'utf-8');
  }
  catch (err) {
    if ((err as NodeJS.ErrnoException).code === 'ENOENT') {
      return undefined;
    }
    throw err;
  }
}
