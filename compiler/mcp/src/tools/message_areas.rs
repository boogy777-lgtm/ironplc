//! The `get_message_areas` MCP tool.
//!
//! The counterpart of `system.get_message_categories` and
//! `system.get_message_category_description` of the CODESYS scripting API: it
//! lists the areas of messages a query can read, each with its description. The
//! list is `Area::ALL`, so an area added there is listed here.

use ironplc_dsl::diagnostic::Area;
use serde::Serialize;

/// One area, as `get_message_areas` lists it.
#[derive(Debug, Serialize)]
pub struct AreaInfo {
    /// The name to give as the `area` of `get_message_objects` and `get_messages`.
    pub name: &'static str,
    /// The `source` an editor shows for a diagnostic of the area.
    pub source: &'static str,
    /// One line that says which messages the area holds and who writes it.
    pub description: &'static str,
}

/// Response returned by `get_message_areas`.
#[derive(Debug, Serialize)]
pub struct MessageAreasResponse {
    pub areas: Vec<AreaInfo>,
}

/// Lists every area.
pub fn build_response() -> MessageAreasResponse {
    MessageAreasResponse {
        areas: Area::ALL
            .iter()
            .map(|area| AreaInfo {
                name: area.name(),
                source: area.source(),
                description: area.description(),
            })
            .collect(),
    }
}
