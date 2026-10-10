//! Sending to the language-server client.

use crossbeam_channel::Sender;
use lsp_server::Message;

/// Sends `message` to the client.
///
/// A send fails only when the client has disconnected, and then there is nobody
/// to answer, so the failure is not reported to the caller.
pub(crate) fn send_to_client(sender: &Sender<Message>, message: Message) {
    #[expect(
        clippy::let_underscore_must_use,
        reason = "send fails only when the client has disconnected, and then there is nobody to answer"
    )]
    let _ = sender.send(message);
}
