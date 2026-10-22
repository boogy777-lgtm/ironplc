//! Real programs of the workspace, for tests of a client that talks to them.
//!
//! A test cannot ask cargo for a binary of another package, so it finds the
//! program beside its own: cargo builds every binary of the workspace for
//! `cargo test --workspace`, and a test executable lives in `deps/` of the same
//! profile directory as the binaries. A test run for one package alone
//! (`cargo test -p ...`) has not built the others, and the helpers say which
//! program to build.

use std::io::{BufRead, BufReader};
use std::net::SocketAddr;
use std::path::{Path, PathBuf};
use std::process::{Child, Command, Stdio};
use std::sync::mpsc;
use std::thread;
use std::time::Duration;

/// How long a started device may take to say where it listens.
const START_TIMEOUT: Duration = Duration::from_secs(30);

/// The line `ironplcvm serve --listen` logs (at `-vv`) when it listens; the rest
/// of the line is the address it is bound to.
const LISTENING: &str = "serving the hot-edit session on ";

/// The path of a binary of the workspace, built in the profile directory of the
/// running test.
pub fn workspace_binary(name: &str) -> PathBuf {
    let exe = std::env::current_exe().expect("the path of the running test");
    let profile_dir = exe
        .parent()
        .and_then(Path::parent)
        .expect("a test executable is in the deps directory of a profile");
    let path = profile_dir.join(format!("{name}{}", std::env::consts::EXE_SUFFIX));
    assert!(
        path.is_file(),
        "{} is not built; run the tests of the whole workspace, or `cargo build -p <package of {name}>` first",
        path.display()
    );
    path
}

/// A running `ironplcvm serve --listen`: a device on the loopback interface
/// that this test started and stops.
pub struct SoftDevice {
    child: Child,
    address: SocketAddr,
}

impl SoftDevice {
    /// Starts a device that serves `container` on a free loopback port and
    /// waits until it listens. It answers no discovery.
    pub fn start(container: &Path) -> SoftDevice {
        let mut child = Command::new(workspace_binary("ironplcvm"))
            .args(["-vv", "serve", "--listen", "127.0.0.1:0", "--no-discovery"])
            .arg(container)
            .stdin(Stdio::null())
            .stdout(Stdio::null())
            .stderr(Stdio::piped())
            .spawn()
            .expect("start ironplcvm");
        let stderr = child.stderr.take().expect("the piped error output");
        let (sender, receiver) = mpsc::channel();
        // The thread keeps reading after the address line, so the device never
        // blocks on a full pipe.
        thread::spawn(move || {
            let mut announced = false;
            for line in BufReader::new(stderr).lines().map_while(Result::ok) {
                if !announced {
                    if let Some((_, address)) = line.split_once(LISTENING) {
                        announced = true;
                        if sender.send(address.trim().to_string()).is_err() {
                            return;
                        }
                    }
                }
            }
        });
        let address = receiver
            .recv_timeout(START_TIMEOUT)
            .expect("the device did not say where it listens")
            .parse()
            .expect("the address the device listens on");
        SoftDevice { child, address }
    }

    /// The address the device listens on.
    pub fn address(&self) -> SocketAddr {
        self.address
    }

    /// Stops the device: the connection of a client to it is dropped.
    pub fn stop(&mut self) {
        self.child.kill().expect("stop the device");
        self.child.wait().expect("wait for the device");
    }
}

impl Drop for SoftDevice {
    fn drop(&mut self) {
        // The device may have been stopped already.
        if self.child.kill().is_ok() {
            self.child.wait().expect("wait for the device");
        }
    }
}
