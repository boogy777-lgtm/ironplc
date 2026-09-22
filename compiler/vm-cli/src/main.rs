use std::net::SocketAddr;
use std::path::PathBuf;
use std::process::ExitCode;

use clap::Parser;

mod cli;
mod error;
mod ha_pair;
mod logger;
mod serve;
#[cfg(test)]
mod serve_tests;
mod slot_store;
mod tcp;

#[cfg(test)]
mod spec_requirements {
    include!(concat!(env!("OUT_DIR"), "/spec_requirements.rs"));
}

#[cfg(test)]
#[ctor::ctor(unsafe)]
fn init_test_logger() {
    let _ = env_logger::builder()
        .is_test(true)
        .filter_level(log::LevelFilter::Trace)
        .try_init();
}

const VERSION: &str = env!("CARGO_PKG_VERSION");

#[derive(Parser, Debug)]
#[command(name = "ironplcvm", about = "IronPLC bytecode virtual machine")]
struct Args {
    /// Turn on verbose logging. Repeat to increase verbosity.
    #[arg(short, long, action = clap::ArgAction::Count)]
    verbose: u8,

    /// Sets the logging to write to a file.
    #[arg(short, long)]
    log_file: Option<PathBuf>,

    /// Selects the subcommand.
    #[command(subcommand)]
    action: Action,
}

#[derive(clap::Subcommand, Debug)]
enum Action {
    /// Loads and executes a bytecode container file.
    Run {
        /// Path to the bytecode container file (.iplc).
        file: PathBuf,

        /// Dump variable values after execution. Without a path, prints to
        /// stdout. With a path, writes to the specified file.
        #[arg(long, num_args = 0..=1, default_missing_value = "-")]
        dump_vars: Option<PathBuf>,

        /// Run N scheduling rounds then stop (default: continuous until Ctrl+C).
        #[arg(long)]
        scans: Option<u64>,
    },
    /// Benchmarks a bytecode container by running it many times and reporting timing statistics.
    Benchmark {
        /// Path to the bytecode container file (.iplc).
        file: PathBuf,

        /// Number of measured scan cycles (default: 10000).
        #[arg(long, default_value_t = 10000)]
        cycles: u64,

        /// Number of warmup scan cycles before measurement (default: 1000).
        #[arg(long, default_value_t = 1000)]
        warmup: u64,
    },
    /// Prints the version number of the virtual machine.
    Version,
    /// Loads a bytecode container file and serves hot-edit commands on stdin/stdout.
    Serve {
        /// Path to the bytecode container file (.iplc).
        file: PathBuf,

        /// Listen on the given TCP address (host:port) instead of stdin/stdout:
        /// the same session protocol, one 4-byte little-endian length-prefixed
        /// frame per message, one engineering session at a time.
        #[arg(long, value_name = "ADDR")]
        listen: Option<SocketAddr>,

        /// Compose the HA redundancy shell with a simulated loopback peer
        /// (the demo binding) so every HA state is exercisable through the
        /// session; default is the honest standalone HA state.
        #[arg(long, conflicts_with = "ha_peer_bind")]
        ha_simulated_peer: bool,

        /// Compose the HA pair link over UDP: bind this pair-link address
        /// (host:port) and serve as one unit of a real two-process pair
        /// with the peer at --ha-peer-peer. The engineering session stays
        /// on stdin/stdout; the pair link stays live between commands.
        #[arg(
            long,
            value_name = "ADDR",
            requires = "ha_peer_peer",
            conflicts_with = "listen"
        )]
        ha_peer_bind: Option<SocketAddr>,

        /// The peer unit's pair-link UDP address (host:port).
        #[arg(long, value_name = "ADDR", requires = "ha_peer_bind")]
        ha_peer_peer: Option<SocketAddr>,

        /// This unit's configured pair role (required with --ha-peer-bind).
        #[arg(long, value_name = "ROLE", requires = "ha_peer_bind")]
        ha_role: Option<PairCliRole>,

        /// The pair identity both units present on the pair link (default:
        /// the demo pair identity).
        #[arg(long, value_name = "ID", default_value_t = 7)]
        ha_pair_id: u64,
    },
}

/// The configured pair role of one pair-mode unit (the CLI vocabulary
/// of `--ha-role`).
#[derive(clap::ValueEnum, Clone, Copy, Debug)]
enum PairCliRole {
    /// The unit the engineer flashed as the pair's initial owner.
    Primary,
    /// The unit that synchronizes from its peer.
    Secondary,
}

impl PairCliRole {
    /// The redundancy crate's configured role.
    fn configured(self) -> ironplc_redundancy::ConfiguredRole {
        match self {
            PairCliRole::Primary => ironplc_redundancy::ConfiguredRole::Primary,
            PairCliRole::Secondary => ironplc_redundancy::ConfiguredRole::Secondary,
        }
    }
}

pub fn main() -> ExitCode {
    let args = Args::parse();

    let result = logger::configure(args.verbose, args.log_file).and_then(|()| match args.action {
        Action::Run {
            file,
            dump_vars,
            scans,
        } => cli::run(&file, dump_vars.as_deref(), scans),
        Action::Benchmark {
            file,
            cycles,
            warmup,
        } => cli::benchmark(&file, cycles, warmup),
        Action::Version => {
            println!("ironplcvm version {VERSION}");
            Ok(())
        }
        Action::Serve {
            file,
            listen,
            ha_simulated_peer,
            ha_peer_bind,
            ha_peer_peer,
            ha_role,
            ha_pair_id,
        } => match (listen, ha_peer_bind) {
            (Some(addr), None) => tcp::serve_tcp(&file, addr, ha_simulated_peer),
            (None, Some(bind)) => {
                // Clap enforces --ha-role only when it is given; a pair
                // bind without a role is a usage error, answered here.
                match ha_role {
                    Some(role) => {
                        let config = ironplc_redundancy::RedundancyConfig::pair(
                            ironplc_redundancy::PairId::new(ha_pair_id),
                            role.configured(),
                        );
                        ha_pair::serve_pair(&file, config, bind, ha_peer_peer.unwrap_or(bind))
                    }
                    None => Err(error::VmError::io(
                        error::SESSION_IO,
                        "--ha-peer-bind requires --ha-role <primary|secondary>".to_string(),
                    )),
                }
            }
            (None, None) => serve::serve(&file, ha_simulated_peer),
            // Clap conflicts the two transports; the defensive arm keeps
            // the dispatch total without a panicking construct.
            (Some(_), Some(_)) => Err(error::VmError::io(
                error::SESSION_IO,
                "--listen and --ha-peer-bind cannot be combined".to_string(),
            )),
        },
    });

    match result {
        Ok(()) => ExitCode::SUCCESS,
        Err(e) => {
            eprintln!("Error: {e}");
            ExitCode::from(e.exit_code())
        }
    }
}
