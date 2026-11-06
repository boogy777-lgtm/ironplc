//! The scripted session of the tests: a command script driven through
//! [`Core`], the part of the device loop that has no clock and no channel.
//!
//! The command tests assert counters and the state a command leaves, so they
//! must decide when rounds run. The scripted session runs a round exactly
//! where the old command-driven session did: one boundary round, at a constant
//! zero uptime, after each acknowledged request of a swap and before its answer
//! is written. Everything else is the code of the loop.

use std::io::{self, BufRead, Write};

use ironplc_redundancy::Shell;
use ironplc_runtime::RuntimeHost;

use super::{Core, Submitted};
use crate::device_settings::device::Device;
use crate::serve::Ha;
use crate::slot_store::SlotStore;

/// Serves one command session: reads one line per command from `reader`,
/// writes one line per response to `writer`, flushing after every line.
///
/// Returns when the reader reaches EOF, or with the first I/O failure.
pub(crate) fn serve_session(
    host: &mut RuntimeHost,
    reader: impl BufRead,
    mut writer: impl Write,
    store: Option<&mut SlotStore>,
    device: &mut Device,
    shell: Option<&mut Shell>,
) -> io::Result<()> {
    let mut core: Core<'_> = Core {
        host,
        store,
        device,
        ha: shell.map(Ha::Shell),
    };
    for line in reader.lines() {
        let answer = match core.submit(&line?)? {
            Submitted::Answered(answer) => answer,
            Submitted::AwaitsBoundary(executing) => {
                core.round(0);
                core.finish(executing)?
            }
        };
        writeln!(writer, "{answer}")?;
        writer.flush()?;
    }
    Ok(())
}
