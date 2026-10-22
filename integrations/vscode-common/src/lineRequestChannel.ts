import { LineTransport } from './lineTransport';

/** The errors a {@link LineRequestChannel} raises, supplied by the protocol that owns it. */
export interface LineRequestChannelErrors {
  /** The error for a request made, or still pending, after the channel was ended on purpose. */
  ended(): Error;
  /** The error for requests pending when the peer closes the channel. */
  exited(): Error;
}

interface PendingRequest {
  resolve(line: string): void;
  reject(reason: Error): void;
}

/**
 * Matches responses to requests over a {@link LineTransport}. A serve session
 * answers every command line with exactly one response line, in order, so a
 * FIFO of pending requests is sufficient and pipelined callers stay correct.
 * The channel resolves the raw response line; each protocol parses it, so
 * several vocabularies can ride one session.
 */
export class LineRequestChannel {
  private readonly pending: PendingRequest[] = [];
  private exited = false;

  constructor(
    private readonly transport: LineTransport,
    private readonly errors: LineRequestChannelErrors,
    private readonly onChannelExit: () => void = () => {},
  ) {
    transport.onLine(line => this.handleLine(line));
    transport.onExit(() => this.handleExit());
  }

  /** False once the peer has closed the channel. */
  get isActive(): boolean {
    return !this.exited;
  }

  /** Sends one line and resolves with the raw response line, in order. */
  async request(line: string): Promise<string> {
    if (this.exited) {
      throw this.errors.ended();
    }
    const response = new Promise<string>((resolve, reject) => {
      this.pending.push({ resolve, reject });
    });
    this.transport.sendLine(line);
    return response;
  }

  /** Rejects every pending request with `reason`. */
  rejectPending(reason: Error): void {
    const pending = this.pending.splice(0, this.pending.length);
    for (const request of pending) {
      request.reject(reason);
    }
  }

  /** Rejects pending requests as ended; the transport itself is owned by the caller. */
  end(): void {
    this.rejectPending(this.errors.ended());
  }

  private handleLine(line: string): void {
    const pending = this.pending.shift();
    if (!pending) {
      // A serve session sends nothing unasked; drop the line defensively.
      return;
    }
    pending.resolve(line);
  }

  private handleExit(): void {
    if (this.exited) {
      return;
    }
    this.exited = true;
    this.rejectPending(this.errors.exited());
    this.onChannelExit();
  }
}
