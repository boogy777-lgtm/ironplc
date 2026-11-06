//! Writing a whole file so that it has reached the disk before the program
//! goes on.
//!
//! Two operations, for the two promises the command line programs make:
//!
//! - [`write_synced`]: the bytes of a file are on the disk when it returns.
//! - [`replace_file`]: a file is the old content or the new, never a torn mix
//!   of both, and the temporary file used to get there never survives a
//!   failure.
//!
//! The functions return [`std::io::Result`]: each program maps the error onto
//! its own error type and message at its boundary (see the crate docs).
//!
//! Neither function makes the *directory entry* durable. The standard library
//! has no portable way to fsync a directory, so a crash right after a rename
//! can still lose the rename itself. Callers that need more than that (the
//! slot store) heal the residue at the next start.

use std::fs::{self, File};
use std::io::{self, Write};
use std::path::Path;

/// Writes `bytes` to the file at `path`, creating it or truncating it, and
/// flushes and fsyncs it before returning.
///
/// The file data has reached the disk when this returns `Ok`. A failure part
/// way leaves a partial file at `path`; use [`replace_file`] when the old
/// content must survive a failure.
pub fn write_synced(path: &Path, bytes: &[u8]) -> io::Result<()> {
    let mut file = File::create(path)?;
    file.write_all(bytes)?;
    file.flush()?;
    file.sync_all()
}

/// Replaces the file at `path` with `bytes`: the bytes are written and
/// fsynced to the file `temporary` ([`write_synced`]), which is then renamed
/// over `path`. After a failure `path` holds what it held before, and
/// `temporary` is removed.
///
/// The caller names `temporary`, and must name a path on the same file system
/// as `path` (a sibling), because a rename does not cross file systems. The
/// file data is fsynced before the rename; the rename itself is not made
/// durable (see the module docs).
///
/// `std::fs::rename` replaces an existing file at `path` on every supported
/// platform, Windows included, so no delete-before-rename step is needed.
///
/// The error returned is the one that made the replace fail. If removing the
/// temporary after that failure fails too, the temporary is left behind and a
/// warning names it, so the residue is never silent.
pub fn replace_file(path: &Path, temporary: &Path, bytes: &[u8]) -> io::Result<()> {
    let replaced = write_synced(temporary, bytes).and_then(|()| fs::rename(temporary, path));
    if replaced.is_err() {
        match fs::remove_file(temporary) {
            Ok(()) => {}
            Err(err) if err.kind() == io::ErrorKind::NotFound => {}
            Err(err) => log::warn!("unable to remove {}: {err}", temporary.display()),
        }
    }
    replaced
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn write_synced_when_file_is_missing_then_creates_it_with_the_bytes() {
        let dir = tempfile::tempdir().unwrap();
        let path = dir.path().join("out.bin");

        write_synced(&path, b"hello").unwrap();

        assert_eq!(fs::read(&path).unwrap(), b"hello");
    }

    #[test]
    fn write_synced_when_file_exists_then_truncates_to_the_bytes() {
        let dir = tempfile::tempdir().unwrap();
        let path = dir.path().join("out.bin");
        fs::write(&path, b"a much longer old content").unwrap();

        write_synced(&path, b"new").unwrap();

        assert_eq!(fs::read(&path).unwrap(), b"new");
    }

    #[test]
    fn replace_file_when_file_exists_then_content_is_replaced_and_temporary_is_gone() {
        let dir = tempfile::tempdir().unwrap();
        let path = dir.path().join("store.json");
        let temporary = dir.path().join("store.json.tmp");
        fs::write(&path, b"old").unwrap();

        replace_file(&path, &temporary, b"new").unwrap();

        assert_eq!(fs::read(&path).unwrap(), b"new");
        assert!(!temporary.exists());
    }

    #[test]
    fn replace_file_when_file_is_missing_then_creates_it() {
        let dir = tempfile::tempdir().unwrap();
        let path = dir.path().join("store.json");
        let temporary = dir.path().join("store.json.tmp");

        replace_file(&path, &temporary, b"new").unwrap();

        assert_eq!(fs::read(&path).unwrap(), b"new");
        assert!(!temporary.exists());
    }

    #[test]
    fn replace_file_when_rename_fails_then_returns_error_and_leaves_no_temporary() {
        let dir = tempfile::tempdir().unwrap();
        // A directory at the target path: a file cannot be renamed over it.
        let path = dir.path().join("target");
        fs::create_dir(&path).unwrap();
        let temporary = dir.path().join("target.tmp");

        let result = replace_file(&path, &temporary, b"new");

        assert!(result.is_err());
        assert!(!temporary.exists());
        assert!(path.is_dir());
    }

    #[test]
    fn replace_file_when_temporary_cannot_be_created_then_returns_error() {
        let dir = tempfile::tempdir().unwrap();
        let path = dir.path().join("store.json");
        fs::write(&path, b"old").unwrap();
        let temporary = dir.path().join("missing-dir").join("store.json.tmp");

        let result = replace_file(&path, &temporary, b"new");

        assert!(result.is_err());
        assert_eq!(fs::read(&path).unwrap(), b"old");
    }
}
