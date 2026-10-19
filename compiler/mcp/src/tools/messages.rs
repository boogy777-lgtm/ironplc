//! The tools `get_message_objects` and `get_messages`.
//!
//! They are the two queries of the CODESYS scripting API for the messages of an
//! area: `system.get_message_objects(category, severities=0xFFFFFFFF)` returns
//! the message objects of the categories in the mask, and
//! `system.get_messages(category)` returns the texts alone. The `category` of
//! CODESYS is the `area` of these tools: `Build` by default, the messages of a
//! `check` and the line that closes it, "Compile complete", which CODESYS stores
//! among the messages of a compile as a `Text`; or `Precompile`, the messages
//! the language server holds while editing, with no closing line.

use ironplc_dsl::diagnostic::Area;
use schemars::JsonSchema;
use serde::{Deserialize, Deserializer, Serialize};

use super::check::{check, Checked};
use super::common::{serialize_message, ParseCheckInput};

/// Every category: the default of `system.get_message_objects`.
pub const ALL_SEVERITIES: u32 = 0xFFFF_FFFF;

/// The area a query reads, by the name the area declares (`Area::name`). The
/// names are those of `Area::ALL`, so an area added there is accepted and listed
/// in the schema without a change here.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct AreaName(pub Area);

impl<'de> Deserialize<'de> for AreaName {
    fn deserialize<D: Deserializer<'de>>(deserializer: D) -> Result<Self, D::Error> {
        let name = String::deserialize(deserializer)?;
        Area::ALL
            .into_iter()
            .find(|area| area.name().eq_ignore_ascii_case(&name))
            .map(AreaName)
            .ok_or_else(|| {
                let names: Vec<&str> = Area::ALL.iter().map(|area| area.name()).collect();
                serde::de::Error::custom(format!(
                    "unknown area `{name}`, expected one of: {}",
                    names.join(", ")
                ))
            })
    }
}

impl JsonSchema for AreaName {
    fn schema_name() -> std::borrow::Cow<'static, str> {
        "AreaName".into()
    }

    fn json_schema(_generator: &mut schemars::SchemaGenerator) -> schemars::Schema {
        let names: Vec<&str> = Area::ALL.iter().map(|area| area.name()).collect();
        schemars::json_schema!({
            "type": "string",
            "enum": names,
            "description": "The area of messages to read, as `get_message_areas` lists them. The default is Build."
        })
    }
}

/// The area a query reads: the one named, or the Build area by default.
fn area_of(name: Option<AreaName>) -> Area {
    name.map_or(Area::Build, |name| name.0)
}

/// Input of `get_message_objects`: the input of `check`, a mask of categories
/// and the area.
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
    /// The area to read: `Build` (the default) or `Precompile`. See
    /// `get_message_areas`.
    #[serde(default)]
    pub area: Option<AreaName>,
}

/// Input of `get_messages`: the input of `check` and the area.
#[derive(Debug, Deserialize, JsonSchema)]
pub struct MessagesInput {
    #[serde(flatten)]
    pub check: ParseCheckInput,
    /// The area to read: `Build` (the default) or `Precompile`. See
    /// `get_message_areas`.
    #[serde(default)]
    pub area: Option<AreaName>,
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

/// The messages of a check in `area` whose category is in `severities` (every
/// category when it is `None`). Both tools read them from here.
fn selected(input: &ParseCheckInput, severities: Option<u32>, area: Option<AreaName>) -> Checked {
    let mask = severities.unwrap_or(ALL_SEVERITIES);
    let mut checked = check(&input.sources, &input.options, area_of(area));
    checked.messages.extend(checked.summary.take());
    checked.messages.retain(|message| message.is_in(mask));
    checked
}

/// The message objects of a check in the area whose category is in
/// `severities`.
pub fn build_objects_response(input: &MessageObjectsInput) -> MessageObjectsResponse {
    let checked = selected(&input.check, input.severities, input.area);
    MessageObjectsResponse {
        ok: checked.ok,
        messages: checked.messages.iter().map(serialize_message).collect(),
    }
}

/// The text of every message of a check in the area, whatever its category, and
/// nothing else: the objects of `get_message_objects` with every category, read
/// for their `text`.
pub fn build_texts_response(input: &MessagesInput) -> MessagesResponse {
    let checked = selected(&input.check, None, input.area);
    MessagesResponse {
        ok: checked.ok,
        messages: checked
            .messages
            .into_iter()
            .map(|message| message.text)
            .collect(),
    }
}
