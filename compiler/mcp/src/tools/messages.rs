//! The tools `get_message_objects` and `get_messages`.
//!
//! They are the two queries of the CODESYS scripting API for the messages of a
//! build: `system.get_message_objects(category, severities=0xFFFFFFFF)` returns
//! the message objects of the categories in the mask, and
//! `system.get_messages(category)` returns the texts alone. The messages are
//! those of a `check`.

use schemars::JsonSchema;
use serde::{Deserialize, Serialize};

use super::check::{check, Checked};
use super::common::{serialize_message, ParseCheckInput};

/// Every category: the default of `system.get_message_objects`.
pub const ALL_SEVERITIES: u32 = 0xFFFF_FFFF;

/// Input of `get_message_objects`: the input of `check` and a mask of categories.
#[derive(Debug, Deserialize, JsonSchema)]
pub struct MessageObjectsInput {
    #[serde(flatten)]
    pub check: ParseCheckInput,
    /// The categories to return, as the sum of their `severity_value`:
    /// FatalError 1, Error 2, Warning 4, Information 8, Text 16,
    /// SuppressedWarning 32, SuppressedInformation 64. The default is every
    /// category (4294967295).
    #[serde(default)]
    pub severities: Option<u32>,
}

/// Response returned by `get_message_objects`.
#[derive(Debug, Serialize)]
pub struct MessageObjectsResponse {
    pub ok: bool,
    pub messages: Vec<serde_json::Value>,
}

/// Response returned by `get_messages`.
#[derive(Debug, Serialize)]
pub struct MessagesResponse {
    pub ok: bool,
    pub messages: Vec<String>,
}

/// The messages of a check whose category is in `severities` (every category
/// when it is `None`). Both tools read them from here.
fn selected(input: &ParseCheckInput, severities: Option<u32>) -> Checked {
    let mask = severities.unwrap_or(ALL_SEVERITIES);
    let mut checked = check(&input.sources, &input.options);
    checked.messages.retain(|message| message.is_in(mask));
    checked
}

/// The message objects of a check whose category is in `severities`.
pub fn build_objects_response(input: &MessageObjectsInput) -> MessageObjectsResponse {
    let checked = selected(&input.check, input.severities);
    MessageObjectsResponse {
        ok: checked.ok,
        messages: checked.messages.iter().map(serialize_message).collect(),
    }
}

/// The text of every message of a check, whatever its category, and nothing
/// else: the objects of `get_message_objects` with every category, read for
/// their `text`.
pub fn build_texts_response(input: &ParseCheckInput) -> MessagesResponse {
    let checked = selected(input, None);
    MessagesResponse {
        ok: checked.ok,
        messages: checked
            .messages
            .into_iter()
            .map(|message| message.text)
            .collect(),
    }
}
