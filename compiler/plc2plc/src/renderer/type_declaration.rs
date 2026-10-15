//! Renders the data type declarations: enumerations, subranges, structures, unions, arrays, strings and references.

use super::text::character_string_text;
use super::{LibraryRenderer, Render};
use ironplc_dsl::common::*;
use ironplc_dsl::{diagnostic::Diagnostic, visitor::Visitor};
use paste::paste;

// 2.3.3.1
impl Render for DataTypeDeclarationKind {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.write_declaration_keyword(self, "TYPE");
        r.newline();

        r.indent();
        self.recurse_visit(r)?;
        r.write_ws(";");
        r.outdent();
        r.newline();

        r.write_ws("END_TYPE");
        r.newline();
        Ok(())
    }
}

impl Render for EnumeratedSpecificationInit {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.visit_enumerated_specification_kind(&self.spec)?;

        // CODESYS/TwinCAT enum base-type suffix, e.g. `(A, B) BYTE;`.
        if let Some(underlying_type) = &self.underlying_type {
            r.write_ws(&underlying_type.to_string());
        }

        if let Some(default) = &self.default {
            r.write_ws(":=");
            r.visit_enumerated_default(default)?;
        }

        Ok(())
    }
}

impl Render for EnumeratedValue {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        // Matches visit_structured_variable's pattern: a raw write around
        // `#` since visit_id's write_ws would insert an unwanted space
        // (this was a pre-existing bug -- COLOR#RED rendered as
        // "COLOR RED", found while adding explicit_value rendering below).
        match &self.type_name {
            Some(type_name) => {
                r.write_ws(type_name.name.original().as_str());
                r.write("#");
                r.write(self.value.original().as_str());
            }
            None => r.visit_id(&self.value)?,
        }

        // CODESYS/TwinCAT explicit per-member enum value, e.g.
        // `Type_UNDEFINED := 0`. Only ever set on a member in an enum
        // declaration's value list, never on a reference (default value,
        // case label, expression).
        if let Some(explicit_value) = &self.explicit_value {
            r.write_ws(":=");
            r.write_ws(&explicit_value.to_i64().to_string());
        }

        Ok(())
    }
}

impl Render for SubrangeDeclaration {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.visit_type_name(&self.type_name)?;

        r.write_ws(":");

        r.visit_subrange_specification_kind(&self.spec)?;

        if let Some(init) = &self.default {
            r.write_ws(":=");
            r.visit_signed_integer(init)?;
        }

        Ok(())
    }
}

impl Render for SubrangeInitializer {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.visit_subrange_specification_kind(&self.spec)?;

        if let Some(init) = &self.initial_value {
            r.write_ws(":=");
            r.visit_signed_integer(init)?;
        }

        Ok(())
    }
}

impl Render for StructureDeclaration {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.visit_type_name(&self.type_name)?;

        r.write_ws(":");

        r.write_ws("STRUCT");

        r.indent();
        r.newline();
        for item in self.elements.iter() {
            r.visit_structure_element_declaration(item)?;
            r.write_ws(";");
            r.newline();
        }
        r.outdent();

        r.write_ws("END_STRUCT");

        Ok(())
    }
}

// CODESYS `UNION ... END_UNION`. The members reuse the structure
// element rendering; only the enclosing keywords differ.
impl Render for UnionDeclaration {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.visit_type_name(&self.type_name)?;

        r.write_ws(":");

        r.write_ws("UNION");

        r.indent();
        r.newline();
        for item in self.elements.iter() {
            r.visit_structure_element_declaration(item)?;
            r.write_ws(";");
            r.newline();
        }
        r.outdent();

        r.write_ws("END_UNION");

        Ok(())
    }
}

// 2.3.3.1
impl Render for StructureInitializationDeclaration {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.visit_type_name(&self.type_name)?;

        if !self.elements_init.is_empty() {
            r.write_ws(":=");
            r.write_ws("(");

            visit_comma_separated!(r, self.elements_init.iter(), StructureElementInit);

            r.write_ws(")");
        }

        Ok(())
    }
}

impl Render for LateResolvedInitializer {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.visit_type_name(&self.type_name)?;

        match &self.initial_value {
            None => {}
            Some(LateResolvedInitialValue::Members(elements)) => {
                r.write_ws(":=");
                r.write_ws("(");

                visit_comma_separated!(r, elements.iter(), StructureElementInit);

                r.write_ws(")");
            }
            Some(LateResolvedInitialValue::Value(value)) => {
                r.write_ws(":=");
                r.visit_id(value)?;
            }
        }

        Ok(())
    }
}

/// Renders the CODESYS parameter-list type, `PARAMS(n) OF T`.
impl Render for ParamsSpecification {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.write_ws("PARAMS");
        r.write_ws("(");
        match &self.count {
            IntegerRef::Literal(count) => r.visit_integer(count)?,
            IntegerRef::Constant(id) => r.visit_id(id)?,
        }
        r.write_ws(")");
        r.write_ws("OF");
        r.visit_type_name(&self.type_name)?;

        Ok(())
    }
}

// 2.3.3.1
impl Render for StringDeclaration {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.visit_type_name(&self.type_name)?;

        r.write_ws(":");

        r.write_ws(self.width.keyword());

        r.write_ws("[");
        r.visit_integer(
            self.length
                .as_integer()
                .ok_or_else(Diagnostic::internal_error)?,
        )?;
        r.write_ws("]");

        if let Some(init) = &self.init {
            r.write_ws(":=");
            r.write(&character_string_text(&init.width, &init.value));
        }

        Ok(())
    }
}

impl Render for ArraySpecificationKind {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        match &self {
            SpecificationKind::Named(id) => r.visit_type_name(id)?,
            SpecificationKind::Inline(subranges) => {
                r.visit_array_subranges(subranges)?;
            }
        }

        Ok(())
    }
}

// 2.3.3.1
impl Render for ArraySubranges {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.write_ws("ARRAY");
        r.write_ws("[");
        match &self.bounds {
            ArrayBounds::Ranges(ranges) => visit_comma_separated!(r, ranges.iter(), Subrange),
            // The incomplete array type has no range list, so `*` is the
            // whole extent.
            ArrayBounds::Incomplete(_) => r.write("*"),
        }
        r.write_ws("]");
        r.write_ws("OF");

        if let Some(syntax) = &self.ref_to {
            r.write_ref_keyword(syntax);
        }

        match &self.type_name {
            ArrayElementType::Named(tn) => r.visit_type_name(tn)?,
            ArrayElementType::String(spec) | ArrayElementType::WString(spec) => {
                r.write_ws(spec.width.keyword());
                if let Some(len) = &spec.length {
                    r.write_ws("[");
                    r.visit_integer(len.as_integer().ok_or_else(Diagnostic::internal_error)?)?;
                    r.write_ws("]");
                }
            }
        }
        Ok(())
    }
}

// 2.4.2.1
impl Render for ironplc_dsl::common::Subrange {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.visit_signed_integer(
            self.start
                .as_signed_integer()
                .ok_or_else(Diagnostic::internal_error)?,
        )?;
        r.write("..");
        r.visit_signed_integer(
            self.end
                .as_signed_integer()
                .ok_or_else(Diagnostic::internal_error)?,
        )
    }
}

impl Render for ReferenceInitializer {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        r.write_ref_keyword(&self.syntax);
        r.visit_reference_target(&self.target)?;

        if let Some(init) = &self.initial_value {
            r.write_ws(":=");
            r.visit_reference_initial_value(init)?;
        }

        Ok(())
    }
}

impl Render for ReferenceInitialValue {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic> {
        match self {
            ReferenceInitialValue::Null(_) => {
                r.write_ws("NULL");
                Ok(())
            }
            ReferenceInitialValue::Ref(var) => {
                r.write_ws("REF(");
                r.visit_variable(var)?;
                r.write_ws(")");
                Ok(())
            }
        }
    }
}
