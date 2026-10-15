//! Renders the data type declarations: enumerations, subranges, structures, unions, arrays, strings and references.

use super::text::character_string_text;
use super::LibraryRenderer;
use ironplc_dsl::common::*;
use ironplc_dsl::{diagnostic::Diagnostic, visitor::Visitor};
use paste::paste;

impl LibraryRenderer {
    // 2.3.3.1
    pub(super) fn render_data_type_declaration_kind(
        &mut self,
        node: &DataTypeDeclarationKind,
    ) -> Result<(), Diagnostic> {
        self.write_declaration_keyword(node, "TYPE");
        self.newline();

        self.indent();
        node.recurse_visit(self)?;
        self.write_ws(";");
        self.outdent();
        self.newline();

        self.write_ws("END_TYPE");
        self.newline();
        Ok(())
    }

    pub(super) fn render_enumerated_specification_init(
        &mut self,
        node: &EnumeratedSpecificationInit,
    ) -> Result<(), Diagnostic> {
        self.visit_enumerated_specification_kind(&node.spec)?;

        // CODESYS/TwinCAT enum base-type suffix, e.g. `(A, B) BYTE;`.
        if let Some(underlying_type) = &node.underlying_type {
            self.write_ws(&underlying_type.to_string());
        }

        if let Some(default) = &node.default {
            self.write_ws(":=");
            self.visit_enumerated_default(default)?;
        }

        Ok(())
    }

    pub(super) fn render_enumerated_value(
        &mut self,
        node: &EnumeratedValue,
    ) -> Result<(), Diagnostic> {
        // Matches visit_structured_variable's pattern: a raw write around
        // `#` since visit_id's write_ws would insert an unwanted space
        // (this was a pre-existing bug -- COLOR#RED rendered as
        // "COLOR RED", found while adding explicit_value rendering below).
        match &node.type_name {
            Some(type_name) => {
                self.write_ws(type_name.name.original().as_str());
                self.write("#");
                self.write(node.value.original().as_str());
            }
            None => self.visit_id(&node.value)?,
        }

        // CODESYS/TwinCAT explicit per-member enum value, e.g.
        // `Type_UNDEFINED := 0`. Only ever set on a member in an enum
        // declaration's value list, never on a reference (default value,
        // case label, expression).
        if let Some(explicit_value) = &node.explicit_value {
            self.write_ws(":=");
            self.write_ws(&explicit_value.to_i64().to_string());
        }

        Ok(())
    }

    pub(super) fn render_subrange_declaration(
        &mut self,
        node: &SubrangeDeclaration,
    ) -> Result<(), Diagnostic> {
        self.visit_type_name(&node.type_name)?;

        self.write_ws(":");

        self.visit_subrange_specification_kind(&node.spec)?;

        if let Some(init) = &node.default {
            self.write_ws(":=");
            self.visit_signed_integer(init)?;
        }

        Ok(())
    }

    pub(super) fn render_subrange_initializer(
        &mut self,
        node: &SubrangeInitializer,
    ) -> Result<(), Diagnostic> {
        self.visit_subrange_specification_kind(&node.spec)?;

        if let Some(init) = &node.initial_value {
            self.write_ws(":=");
            self.visit_signed_integer(init)?;
        }

        Ok(())
    }

    pub(super) fn render_structure_declaration(
        &mut self,
        node: &StructureDeclaration,
    ) -> Result<(), Diagnostic> {
        self.visit_type_name(&node.type_name)?;

        self.write_ws(":");

        self.write_ws("STRUCT");

        self.indent();
        self.newline();
        for item in node.elements.iter() {
            self.visit_structure_element_declaration(item)?;
            self.write_ws(";");
            self.newline();
        }
        self.outdent();

        self.write_ws("END_STRUCT");

        Ok(())
    }

    // CODESYS `UNION ... END_UNION`. The members reuse the structure
    // element rendering; only the enclosing keywords differ.
    pub(super) fn render_union_declaration(
        &mut self,
        node: &UnionDeclaration,
    ) -> Result<(), Diagnostic> {
        self.visit_type_name(&node.type_name)?;

        self.write_ws(":");

        self.write_ws("UNION");

        self.indent();
        self.newline();
        for item in node.elements.iter() {
            self.visit_structure_element_declaration(item)?;
            self.write_ws(";");
            self.newline();
        }
        self.outdent();

        self.write_ws("END_UNION");

        Ok(())
    }

    // 2.3.3.1
    pub(super) fn render_structure_initialization_declaration(
        &mut self,
        node: &StructureInitializationDeclaration,
    ) -> Result<(), Diagnostic> {
        self.visit_type_name(&node.type_name)?;

        if !node.elements_init.is_empty() {
            self.write_ws(":=");
            self.write_ws("(");

            visit_comma_separated!(self, node.elements_init.iter(), StructureElementInit);

            self.write_ws(")");
        }

        Ok(())
    }

    pub(super) fn render_late_resolved_initializer(
        &mut self,
        node: &LateResolvedInitializer,
    ) -> Result<(), Diagnostic> {
        self.visit_type_name(&node.type_name)?;

        match &node.initial_value {
            None => {}
            Some(LateResolvedInitialValue::Members(elements)) => {
                self.write_ws(":=");
                self.write_ws("(");

                visit_comma_separated!(self, elements.iter(), StructureElementInit);

                self.write_ws(")");
            }
            Some(LateResolvedInitialValue::Value(value)) => {
                self.write_ws(":=");
                self.visit_id(value)?;
            }
        }

        Ok(())
    }

    /// Renders the CODESYS parameter-list type, `PARAMS(n) OF T`.
    pub(super) fn render_params_specification(
        &mut self,
        node: &ParamsSpecification,
    ) -> Result<(), Diagnostic> {
        self.write_ws("PARAMS");
        self.write_ws("(");
        match &node.count {
            IntegerRef::Literal(count) => self.visit_integer(count)?,
            IntegerRef::Constant(id) => self.visit_id(id)?,
        }
        self.write_ws(")");
        self.write_ws("OF");
        self.visit_type_name(&node.type_name)?;

        Ok(())
    }

    // 2.3.3.1
    pub(super) fn render_string_declaration(
        &mut self,
        node: &StringDeclaration,
    ) -> Result<(), Diagnostic> {
        self.visit_type_name(&node.type_name)?;

        self.write_ws(":");

        self.write_ws(node.width.keyword());

        self.write_ws("[");
        self.visit_integer(
            node.length
                .as_integer()
                .ok_or_else(Diagnostic::internal_error)?,
        )?;
        self.write_ws("]");

        if let Some(init) = &node.init {
            self.write_ws(":=");
            self.write(&character_string_text(&init.width, &init.value));
        }

        Ok(())
    }

    pub(super) fn render_array_specification_kind(
        &mut self,
        node: &ArraySpecificationKind,
    ) -> Result<(), Diagnostic> {
        match &node {
            SpecificationKind::Named(id) => self.visit_type_name(id)?,
            SpecificationKind::Inline(subranges) => {
                self.visit_array_subranges(subranges)?;
            }
        }

        Ok(())
    }

    // 2.3.3.1
    pub(super) fn render_array_subranges(
        &mut self,
        node: &ArraySubranges,
    ) -> Result<(), Diagnostic> {
        self.write_ws("ARRAY");
        self.write_ws("[");
        match &node.bounds {
            ArrayBounds::Ranges(ranges) => visit_comma_separated!(self, ranges.iter(), Subrange),
            // The incomplete array type has no range list, so `*` is the
            // whole extent.
            ArrayBounds::Incomplete(_) => self.write("*"),
        }
        self.write_ws("]");
        self.write_ws("OF");

        if let Some(syntax) = &node.ref_to {
            self.write_ref_keyword(syntax);
        }

        match &node.type_name {
            ArrayElementType::Named(tn) => self.visit_type_name(tn)?,
            ArrayElementType::String(spec) | ArrayElementType::WString(spec) => {
                self.write_ws(spec.width.keyword());
                if let Some(len) = &spec.length {
                    self.write_ws("[");
                    self.visit_integer(len.as_integer().ok_or_else(Diagnostic::internal_error)?)?;
                    self.write_ws("]");
                }
            }
        }
        Ok(())
    }

    // 2.4.2.1
    pub(super) fn render_subrange(
        &mut self,
        node: &ironplc_dsl::common::Subrange,
    ) -> Result<(), Diagnostic> {
        self.visit_signed_integer(
            node.start
                .as_signed_integer()
                .ok_or_else(Diagnostic::internal_error)?,
        )?;
        self.write("..");
        self.visit_signed_integer(
            node.end
                .as_signed_integer()
                .ok_or_else(Diagnostic::internal_error)?,
        )
    }

    pub(super) fn render_reference_initializer(
        &mut self,
        node: &ReferenceInitializer,
    ) -> Result<(), Diagnostic> {
        self.write_ref_keyword(&node.syntax);
        self.visit_reference_target(&node.target)?;

        if let Some(init) = &node.initial_value {
            self.write_ws(":=");
            self.visit_reference_initial_value(init)?;
        }

        Ok(())
    }

    pub(super) fn render_reference_initial_value(
        &mut self,
        node: &ReferenceInitialValue,
    ) -> Result<(), Diagnostic> {
        match node {
            ReferenceInitialValue::Null(_) => {
                self.write_ws("NULL");
                Ok(())
            }
            ReferenceInitialValue::Ref(var) => {
                self.write_ws("REF(");
                self.visit_variable(var)?;
                self.write_ws(")");
                Ok(())
            }
        }
    }
}
