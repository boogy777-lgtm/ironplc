//! The `stdio` connection method: spawn a program and speak one JSON command
//! per line over the child's stdin and stdout. For a device the program is
//! `ironplcvm serve <program>` ([`StdioEndpoint::serving`]).
//!
//! The child is the device. It is never respawned: when it ends, the connection
//! is closed and the caller opens a new one. Starting it with a container is a
//! download, which is the caller's to record.

use std::ffi::OsString;
use std::io::{BufRead, BufReader, Read, Write};
use std::path::PathBuf;
use std::process::{Child, ChildStdin, Command, Stdio};
use std::sync::mpsc::{self, Receiver, RecvTimeoutError};
use std::sync::{Arc, Mutex};
use std::thread::{self, JoinHandle};
use std::time::{Duration, Instant};

use ironplc_runtime::frame::CLOSE_LINGER;

use super::Method;
use crate::channel::{Channel, ChannelError};
use crate::error::SessionError;

/// How much of the error output of the child is kept to explain why it ended.
const STDERR_KEPT: usize = 2048;

/// A program that this client starts and talks to in lines.
#[derive(Clone, Debug, PartialEq, Eq)]
pub struct StdioEndpoint {
    /// The executable.
    pub executable: PathBuf,
    /// Its arguments.
    pub args: Vec<OsString>,
    /// Environment variables set for the child, on top of the environment of
    /// this process.
    pub env: Vec<(String, String)>,
}

impl StdioEndpoint {
    /// The device that is `executable` (`ironplcvm`) serving the compiled
    /// container `program` (`.iplc`).
    pub fn serving(executable: PathBuf, program: PathBuf) -> StdioEndpoint {
        StdioEndpoint {
            executable,
            args: vec![OsString::from("serve"), program.into_os_string()],
            env: Vec::new(),
        }
    }
}

impl Method for StdioEndpoint {
    fn open(&self, _connect_timeout: Duration) -> Result<Box<dyn Channel>, SessionError> {
        let mut child = Command::new(&self.executable)
            .args(&self.args)
            .envs(self.env.iter().map(|(key, value)| (key, value)))
            .stdin(Stdio::piped())
            .stdout(Stdio::piped())
            .stderr(Stdio::piped())
            .spawn()
            .map_err(|err| SessionError::Open {
                method: "stdio",
                message: format!("unable to start {}: {err}", self.executable.display()),
            })?;
        let (Some(stdin), Some(stdout), Some(stderr)) =
            (child.stdin.take(), child.stdout.take(), child.stderr.take())
        else {
            return Err(SessionError::Open {
                method: "stdio",
                message: "the program started without its standard streams".to_string(),
            });
        };
        Ok(Box::new(StdioChannel {
            child,
            stdin: Some(stdin),
            lines: read_lines_in_thread(stdout),
            stderr: keep_tail_in_thread(stderr),
            is_closed: false,
        }))
    }
}

/// The lines of `stdout`, read by a thread because a pipe has no read timeout.
/// The channel ends when the thread drops its sender at the end of the stream.
fn read_lines_in_thread(stdout: impl Read + Send + 'static) -> Receiver<std::io::Result<String>> {
    let (sender, receiver) = mpsc::channel();
    thread::spawn(move || {
        for line in BufReader::new(stdout).lines() {
            if sender.send(line).is_err() {
                return;
            }
        }
    });
    receiver
}

/// The last [`STDERR_KEPT`] bytes of `stderr`, kept by a thread so that a child
/// that writes a lot to it is never blocked. The thread ends when the pipe does.
fn keep_tail_in_thread(stderr: impl Read + Send + 'static) -> ErrorOutput {
    let kept = Arc::new(Mutex::new(String::new()));
    let sink = Arc::clone(&kept);
    let reader = thread::spawn(move || {
        for line in BufReader::new(stderr).lines().map_while(Result::ok) {
            let mut text = sink.lock().unwrap_or_else(|err| err.into_inner());
            text.push_str(&line);
            text.push('\n');
            let excess = text.len().saturating_sub(STDERR_KEPT);
            if excess > 0 {
                // Cut at a character boundary at or after the excess.
                let cut = (excess..=text.len())
                    .find(|&at| text.is_char_boundary(at))
                    .unwrap_or(text.len());
                text.drain(..cut);
            }
        }
    });
    ErrorOutput {
        kept,
        reader: Some(reader),
    }
}

/// The error output of the child, as far as it has written it.
struct ErrorOutput {
    kept: Arc<Mutex<String>>,
    reader: Option<JoinHandle<()>>,
}

/// The child process and the lines it writes.
struct StdioChannel {
    child: Child,
    stdin: Option<ChildStdin>,
    lines: Receiver<std::io::Result<String>>,
    stderr: ErrorOutput,
    is_closed: bool,
}

impl StdioChannel {
    /// The channel has ended: stops the child, if it still runs, so that its
    /// error output ends too, and reports what it wrote there (the V-code of a
    /// program that exited at start).
    fn closed(&mut self) -> ChannelError {
        if let Err(err) = self.child.kill() {
            log::debug!("the session program had already ended: {err}");
        }
        if let Some(reader) = self.stderr.reader.take() {
            if reader.join().is_err() {
                log::debug!("the reader of the error output of the session program failed");
            }
        }
        let text = self
            .stderr
            .kept
            .lock()
            .unwrap_or_else(|err| err.into_inner());
        let detail = text.trim();
        ChannelError::Closed((!detail.is_empty()).then(|| detail.to_string()))
    }
}

impl Channel for StdioChannel {
    fn send(&mut self, line: &str) -> Result<(), ChannelError> {
        let Some(stdin) = self.stdin.as_mut() else {
            return Err(ChannelError::Closed(None));
        };
        writeln!(stdin, "{line}")
            .and_then(|()| stdin.flush())
            .map_err(|_| self.closed())
    }

    fn receive(&mut self, timeout: Duration) -> Result<String, ChannelError> {
        match self.lines.recv_timeout(timeout) {
            Ok(Ok(line)) => Ok(line),
            Ok(Err(err)) => Err(ChannelError::Broken(err.to_string())),
            Err(RecvTimeoutError::Timeout) => Err(ChannelError::TimedOut),
            Err(RecvTimeoutError::Disconnected) => Err(self.closed()),
        }
    }

    fn close(&mut self) {
        if self.is_closed {
            return;
        }
        self.is_closed = true;
        // The same close as the `tcp` method, over a pipe: end the input, which
        // is the end of the client's output, and wait for the program to end,
        // which is the device letting go. A program that has not ended after
        // `CLOSE_LINGER` is stopped. What it still writes is discarded, so that
        // it is never blocked on a full pipe while it ends.
        self.stdin = None;
        let deadline = Instant::now() + CLOSE_LINGER;
        while Instant::now() < deadline {
            match self.child.try_wait() {
                Ok(Some(_)) | Err(_) => return,
                Ok(None) => {}
            }
            // Waiting for output is the pause between two checks of the child.
            if let Err(RecvTimeoutError::Disconnected) =
                self.lines.recv_timeout(Duration::from_millis(1))
            {
                // Its output has ended with it: only its exit remains.
                if let Err(err) = self.child.wait() {
                    log::debug!("unable to wait for the session program: {err}");
                }
                return;
            }
        }
        log::debug!("the session program did not end in time and is stopped");
        if let Err(err) = self.child.kill() {
            log::debug!("the session program had already ended: {err}");
        }
        if let Err(err) = self.child.wait() {
            log::debug!("unable to wait for the session program: {err}");
        }
    }
}

impl Drop for StdioChannel {
    fn drop(&mut self) {
        self.close();
    }
}
