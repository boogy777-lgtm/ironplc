//! Output writer for IEC61131-3 language elements. The writer transform
//! a parsed library into text.
//!
//! The writer is useful for debugging the parser and to understand the
//! internal representation.

use dsl::configuration::LocatedVarInit;
use ironplc_dsl::common::*;
use ironplc_dsl::core::Id;
use ironplc_dsl::member_qualifier::MemberQualifiers;
use ironplc_dsl::time::*;
use ironplc_dsl::{diagnostic::Diagnostic, visitor::Visitor};
use paste::paste;

/// Defines a macro for creating a comma separated list of items where
/// each item in the list is created by visiting the item.
macro_rules! visit_comma_separated {
    ($self:ident, $iter:expr, $struct_name:ident) => {
        paste! {
            {
                let mut it = $iter.peekable();
                while let Some(item) = it.next() {
                    $self.[<visit_ $struct_name:snake>](item)?;
                    if it.peek().is_some() {
                        $self.write_ws(",");
                    }
                }
            }
        }
    };
}

macro_rules! write_period_separated {
    ($self:ident, $iter:expr) => {
        paste! {
            {
                let mut it = $iter.peekable();
                while let Some(item) = it.next() {
                    $self.write(item);
                    if it.peek().is_some() {
                        $self.write(".");
                    }
                }
            }
        }
    };
}

mod configuration;
mod expression;
mod pou_declaration;
mod sfc;
mod statement;
mod text;
mod type_declaration;
mod variable_access;
mod variable_declaration;

use text::character_string_text;

pub fn apply(lib: &Library) -> Result<String, Vec<Diagnostic>> {
    let mut visitor = LibraryRenderer::new();
    visitor
        .walk(lib)
        .map(|_| visitor.buffer)
        .map_err(|e| vec![e])
}

struct LibraryRenderer {
    buffer: String,
    indents: usize,
}

impl LibraryRenderer {
    fn new() -> Self {
        Self {
            buffer: String::new(),
            indents: 0,
        }
    }

    fn write(&mut self, val: &str) {
        self.buffer.push_str(val);
    }

    fn write_qualifiers(&mut self, qualifiers: &MemberQualifiers) {
        for qualifier in qualifiers.iter() {
            self.write_ws(qualifier.kind.keyword());
        }
    }

    fn write_ws(&mut self, val: &str) {
        if self.buffer.ends_with('\n') {
            self.buffer.push_str("   ".repeat(self.indents).as_str());
        } else if !self.buffer.ends_with(' ') && !self.buffer.is_empty() {
            self.buffer.push(' ');
        }
        self.buffer.push_str(val);
    }

    fn newline(&mut self) {
        self.buffer.push('\n');
    }

    /// Writes the reference-type keyword for the given surface syntax:
    /// `REF_TO` for IEC, `REFERENCE TO` / `POINTER TO` for the
    /// TwinCAT/CODESYS variants.
    fn write_ref_keyword(&mut self, syntax: &RefSyntax) {
        match syntax {
            RefSyntax::RefTo => self.write_ws("REF_TO"),
            RefSyntax::ReferenceTo => {
                self.write_ws("REFERENCE");
                self.write_ws("TO");
            }
            RefSyntax::PointerTo => {
                self.write_ws("POINTER");
                self.write_ws("TO");
            }
        }
    }

    fn indent(&mut self) {
        self.indents += 1;
    }

    /// Writes a statement that is a keyword alone, such as `RETURN;`.
    fn write_keyword_statement(&mut self, keyword: &str) -> Result<(), Diagnostic> {
        self.write_ws(keyword);
        self.write_ws(";");
        self.newline();
        Ok(())
    }

    fn outdent(&mut self) {
        self.indents -= 1;
    }
}

impl Visitor<Diagnostic> for LibraryRenderer {
    type Value = ();

    fn visit_id(&mut self, node: &Id) -> Result<Self::Value, Diagnostic> {
        // TODO this is the wrong case
        self.write_ws(node.original().as_str());
        Ok(())
    }

    fn visit_integer(&mut self, node: &Integer) -> Result<Self::Value, Diagnostic> {
        self.write_ws(node.value.to_string().as_str());
        Ok(())
    }

    // 2.1.2.
    fn visit_signed_integer(
        &mut self,
        node: &ironplc_dsl::common::SignedInteger,
    ) -> Result<Self::Value, Diagnostic> {
        if node.is_neg {
            // The sign belongs to the literal, so the separating space goes
            // before it, never between it and the digits.
            self.write_ws("-");
            self.write(node.value.value.to_string().as_str());
            return Ok(());
        }
        self.visit_integer(&node.value)
    }

    fn visit_real_literal(&mut self, node: &RealLiteral) -> Result<Self::Value, Diagnostic> {
        let mut val = String::new();
        if let Some(data_type) = &node.data_type {
            val.push_str(data_type.as_id().original());
            val.push('#');
        }
        // A real literal always carries a decimal point; `f64::to_string`
        // drops it for whole values.
        let mut value = node.value.to_string();
        if node.value.is_finite() && !value.contains(['.', 'e', 'E']) {
            value.push_str(".0");
        }
        val.push_str(value.as_str());

        self.write_ws(val.as_str());
        Ok(())
    }

    fn visit_boolean_literal(&mut self, node: &BooleanLiteral) -> Result<Self::Value, Diagnostic> {
        let val = match node.value {
            Boolean::True => "BOOL#TRUE",
            Boolean::False => "BOOL#FALSE",
        };
        self.write_ws(val);
        Ok(())
    }

    fn visit_character_string_literal(
        &mut self,
        node: &CharacterStringLiteral,
    ) -> Result<Self::Value, Diagnostic> {
        // Single quotes delimit a STRING, double quotes a WSTRING, per
        // IEC 61131-3 section 2.2.2. `node.value` holds the decoded
        // characters, so they are escaped on the way out.
        self.write_ws(&character_string_text(&node.width, &node.value));
        Ok(())
    }

    // Every temporal literal is written with the prefix of the type it names,
    // taken from the literal itself. Writing the 32-bit prefix for all of them
    // narrowed the type on round trip -- `LTIME#30d` came back as
    // `TIME#2592000000ms`, which is a different type and, since that count does
    // not fit a `TIME`, no longer compiles.

    fn visit_duration_literal(
        &mut self,
        node: &DurationLiteral,
    ) -> Result<Self::Value, Diagnostic> {
        // The unit spelling is the literal's own (`unit_text`), so a literal
        // with a sub-millisecond part (`T#1us`, `T#1.5ms`) re-parses to the
        // same value instead of being written as a whole number of
        // milliseconds.
        let val = format!("{}#{}", node.type_name(), node.unit_text());
        self.write_ws(val.as_str());
        Ok(())
    }

    fn visit_time_of_day_literal(
        &mut self,
        node: &TimeOfDayLiteral,
    ) -> Result<Self::Value, Diagnostic> {
        self.write_ws(format!("{}#{}", node.type_name(), node.daytime_text()).as_str());
        Ok(())
    }

    fn visit_date_literal(&mut self, node: &DateLiteral) -> Result<Self::Value, Diagnostic> {
        let (year, month, day) = node.ymd();
        self.write_ws(format!("{}#{year:0>4}-{month:0>2}-{day:0>2}", node.type_name()).as_str());
        Ok(())
    }

    fn visit_date_and_time_literal(
        &mut self,
        node: &DateAndTimeLiteral,
    ) -> Result<Self::Value, Diagnostic> {
        let (year, month, day) = node.ymd();
        self.write_ws(
            format!(
                "{}#{year:0>4}-{month:0>2}-{day:0>2}-{}",
                node.type_name(),
                node.daytime_text()
            )
            .as_str(),
        );
        Ok(())
    }

    fn visit_bit_string_literal(
        &mut self,
        node: &BitStringLiteral,
    ) -> Result<Self::Value, Diagnostic> {
        let mut s = String::new();
        if let Some(data_type) = &node.data_type {
            s.push_str(data_type.as_id().to_string().as_str());
            s.push('#');
        }
        s.push_str(&node.value.value.to_string());

        self.write_ws(&s);
        Ok(())
    }

    fn visit_data_type_declaration_kind(
        &mut self,
        node: &DataTypeDeclarationKind,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_data_type_declaration_kind(node)
    }

    fn visit_late_bound_declaration(
        &mut self,
        node: &LateBoundDeclaration,
    ) -> Result<Self::Value, Diagnostic> {
        self.visit_type_name(&node.data_type_name)?;

        self.write_ws(":");

        self.visit_type_name(&node.base_type_name)
    }

    fn visit_simple_declaration(
        &mut self,
        node: &SimpleDeclaration,
    ) -> Result<Self::Value, Diagnostic> {
        self.visit_type_name(&node.type_name)?;

        self.write_ws(":");

        self.visit_initial_value_assignment_kind(&node.spec_and_init)
    }

    fn visit_enumeration_declaration(
        &mut self,
        node: &EnumerationDeclaration,
    ) -> Result<Self::Value, Diagnostic> {
        self.visit_type_name(&node.type_name)?;

        self.write_ws(":");

        self.visit_enumerated_specification_init(&node.spec_init)
    }

    fn visit_enumerated_specification_init(
        &mut self,
        node: &EnumeratedSpecificationInit,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_enumerated_specification_init(node)
    }

    fn visit_enumerated_specification_values(
        &mut self,
        node: &EnumeratedSpecificationValues,
    ) -> Result<Self::Value, Diagnostic> {
        self.write_ws("(");

        visit_comma_separated!(self, node.values.iter(), EnumeratedValue);

        self.write_ws(")");
        Ok(())
    }

    fn visit_enumerated_value(
        &mut self,
        node: &EnumeratedValue,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_enumerated_value(node)
    }

    fn visit_subrange_declaration(
        &mut self,
        node: &SubrangeDeclaration,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_subrange_declaration(node)
    }

    fn visit_subrange_initializer(
        &mut self,
        node: &SubrangeInitializer,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_subrange_initializer(node)
    }

    fn visit_subrange_specification(
        &mut self,
        node: &SubrangeSpecification,
    ) -> Result<Self::Value, Diagnostic> {
        self.visit_id(&node.type_name.as_id())?;
        self.write_ws("(");
        self.visit_subrange(&node.subrange)?;
        self.write_ws(")");

        Ok(())
    }

    fn visit_structure_declaration(
        &mut self,
        node: &StructureDeclaration,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_structure_declaration(node)
    }

    fn visit_union_declaration(
        &mut self,
        node: &UnionDeclaration,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_union_declaration(node)
    }

    fn visit_structure_element_declaration(
        &mut self,
        node: &StructureElementDeclaration,
    ) -> Result<Self::Value, Diagnostic> {
        self.write_attributes(node);
        self.visit_id(&node.name)?;
        self.write_ws(":");
        self.visit_initial_value_assignment_kind(&node.init)
    }

    fn visit_structure_initialization_declaration(
        &mut self,
        node: &StructureInitializationDeclaration,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_structure_initialization_declaration(node)
    }

    fn visit_late_resolved_initializer(
        &mut self,
        node: &LateResolvedInitializer,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_late_resolved_initializer(node)
    }

    // 2.3.3.1
    fn visit_structure_element_init(
        &mut self,
        node: &StructureElementInit,
    ) -> Result<Self::Value, Diagnostic> {
        self.visit_id(&node.name)?;

        self.write_ws(":=");

        self.visit_struct_initial_value_assignment_kind(&node.init)
    }

    fn visit_array_declaration(
        &mut self,
        node: &ArrayDeclaration,
    ) -> Result<Self::Value, Diagnostic> {
        self.visit_type_name(&node.type_name)?;

        self.write_ws(":");

        self.visit_array_specification_kind(&node.spec)?;

        Ok(())
    }

    /// Renders the CODESYS `PARAMS(n) OF T` type declaration.
    fn visit_params_declaration(
        &mut self,
        node: &ParamsDeclaration,
    ) -> Result<Self::Value, Diagnostic> {
        self.visit_type_name(&node.type_name)?;

        self.write_ws(":");

        self.visit_params_specification(&node.spec)?;

        Ok(())
    }

    fn visit_params_specification(
        &mut self,
        node: &ParamsSpecification,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_params_specification(node)
    }

    fn visit_string_declaration(
        &mut self,
        node: &StringDeclaration,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_string_declaration(node)
    }

    fn visit_array_specification_kind(
        &mut self,
        node: &ArraySpecificationKind,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_array_specification_kind(node)
    }

    fn visit_array_subranges(&mut self, node: &ArraySubranges) -> Result<Self::Value, Diagnostic> {
        self.render_array_subranges(node)
    }

    fn visit_subrange(
        &mut self,
        node: &ironplc_dsl::common::Subrange,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_subrange(node)
    }

    fn visit_program_access_decl(
        &mut self,
        node: &ProgramAccessDecl,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_program_access_decl(node)
    }

    fn visit_var_decl(&mut self, node: &VarDecl) -> Result<Self::Value, Diagnostic> {
        self.render_var_decl(node)
    }

    fn visit_address_assignment(
        &mut self,
        node: &AddressAssignment,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_address_assignment(node)
    }

    fn visit_simple_initializer(
        &mut self,
        node: &SimpleInitializer,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_simple_initializer(node)
    }

    // Extension: constant-expression VAR initializer
    // (e.g. `PI/180.0`), not yet folded to a literal.
    fn visit_simple_expr_initializer(
        &mut self,
        node: &SimpleExprInitializer,
    ) -> Result<Self::Value, Diagnostic> {
        self.visit_type_name(&node.type_name)?;
        self.write_ws(":=");
        self.visit_expr(&node.initial_value)?;

        Ok(())
    }

    fn visit_string_initializer(
        &mut self,
        node: &StringInitializer,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_string_initializer(node)
    }

    fn visit_enumerated_values_initializer(
        &mut self,
        node: &EnumeratedValuesInitializer,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_enumerated_values_initializer(node)
    }

    // 2.4.3.2
    fn visit_function_block_initial_value_assignment(
        &mut self,
        node: &FunctionBlockInitialValueAssignment,
    ) -> Result<Self::Value, Diagnostic> {
        self.visit_type_name(&node.type_name)
    }

    // CODESYS/TwinCAT call-style FB instance initializer:
    // `name : FB_Type(args)`. Render the type name followed by the
    // parenthesized constructor argument list.
    fn visit_function_block_call_initializer(
        &mut self,
        node: &FunctionBlockCallInitializer,
    ) -> Result<Self::Value, Diagnostic> {
        self.visit_type_name(&node.type_name)?;
        self.write_ws("(");
        visit_comma_separated!(self, node.params.iter(), ParamAssignmentKind);
        self.write_ws(")");
        Ok(())
    }

    fn visit_array_initial_value_assignment(
        &mut self,
        node: &ArrayInitialValueAssignment,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_array_initial_value_assignment(node)
    }

    /// Renders a repeated array element, `count(value)`.
    fn visit_repeated(
        &mut self,
        node: &ironplc_dsl::common::Repeated,
    ) -> Result<Self::Value, Diagnostic> {
        self.visit_integer(&node.size)?;
        self.write_ws("(");
        if let Some(init) = node.init.as_ref() {
            self.visit_array_initial_element_kind(init)?;
        }
        self.write_ws(")");
        Ok(())
    }

    fn visit_enumerated_initial_value_assignment(
        &mut self,
        node: &EnumeratedInitialValueAssignment,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_enumerated_initial_value_assignment(node)
    }

    fn visit_direct_variable_identifier(
        &mut self,
        node: &DirectVariableIdentifier,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_direct_variable_identifier(node)
    }

    fn visit_function_declaration(
        &mut self,
        node: &FunctionDeclaration,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_function_declaration(node)
    }

    fn visit_function_block_declaration(
        &mut self,
        node: &FunctionBlockDeclaration,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_function_block_declaration(node)
    }

    fn visit_method_declaration(
        &mut self,
        node: &MethodDeclaration,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_method_declaration(node)
    }

    fn visit_property_declaration(
        &mut self,
        node: &PropertyDeclaration,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_property_declaration(node)
    }

    fn visit_namespace_declaration(
        &mut self,
        node: &NamespaceDeclaration,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_namespace_declaration(node)
    }

    fn visit_interface_declaration(
        &mut self,
        node: &InterfaceDeclaration,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_interface_declaration(node)
    }

    fn visit_program_declaration(
        &mut self,
        node: &ProgramDeclaration,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_program_declaration(node)
    }

    fn visit_network(&mut self, node: &dsl::sfc::Network) -> Result<Self::Value, Diagnostic> {
        self.render_network(node)
    }

    // 2.6.2
    fn visit_step(&mut self, node: &dsl::sfc::Step) -> Result<Self::Value, Diagnostic> {
        self.write_ws("STEP");
        self.write_step_body(node)
    }

    fn visit_transition(&mut self, node: &dsl::sfc::Transition) -> Result<Self::Value, Diagnostic> {
        self.render_transition(node)
    }

    fn visit_action(&mut self, node: &dsl::sfc::Action) -> Result<Self::Value, Diagnostic> {
        self.render_action(node)
    }

    fn visit_action_association(
        &mut self,
        node: &dsl::sfc::ActionAssociation,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_action_association(node)
    }

    fn visit_resource_declaration(
        &mut self,
        node: &dsl::configuration::ResourceDeclaration,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_resource_declaration(node)
    }

    fn visit_program_configuration(
        &mut self,
        node: &dsl::configuration::ProgramConfiguration,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_program_configuration(node)
    }

    fn visit_configuration_declaration(
        &mut self,
        node: &dsl::configuration::ConfigurationDeclaration,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_configuration_declaration(node)
    }

    fn visit_function_block_init(
        &mut self,
        node: &dsl::configuration::FunctionBlockInit,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_function_block_init(node)
    }

    fn visit_located_var_init(&mut self, node: &LocatedVarInit) -> Result<Self::Value, Diagnostic> {
        self.render_located_var_init(node)
    }

    fn visit_task_configuration(
        &mut self,
        node: &dsl::configuration::TaskConfiguration,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_task_configuration(node)
    }

    fn visit_fb_call(&mut self, node: &dsl::textual::FbCall) -> Result<Self::Value, Diagnostic> {
        self.render_fb_call(node)
    }

    fn visit_method_call(
        &mut self,
        node: &dsl::textual::MethodCall,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_method_call(node)
    }

    fn visit_stmt_kind(
        &mut self,
        node: &dsl::textual::StmtKind,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_stmt_kind(node)
    }

    fn visit_assignment(
        &mut self,
        node: &dsl::textual::Assignment,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_assignment(node)
    }

    fn visit_compare_expr(
        &mut self,
        node: &dsl::textual::CompareExpr,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_compare_expr(node)
    }

    fn visit_binary_expr(
        &mut self,
        node: &dsl::textual::BinaryExpr,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_binary_expr(node)
    }

    fn visit_unary_expr(
        &mut self,
        node: &dsl::textual::UnaryExpr,
    ) -> Result<Self::Value, Diagnostic> {
        let op = match node.op {
            dsl::textual::UnaryOp::Neg => "-",
            dsl::textual::UnaryOp::Not => "NOT",
        };
        self.write_ws(op);

        self.visit_expr(&node.term)
    }

    fn visit_function(&mut self, node: &dsl::textual::Function) -> Result<Self::Value, Diagnostic> {
        self.visit_id(&node.name)?;

        self.write_ws("(");
        visit_comma_separated!(self, node.param_assignment.iter(), ParamAssignmentKind);
        self.write_ws(")");

        Ok(())
    }

    fn visit_repeat(&mut self, node: &dsl::textual::Repeat) -> Result<Self::Value, Diagnostic> {
        self.render_repeat(node)
    }

    fn visit_try_catch(
        &mut self,
        node: &dsl::textual::TryCatch,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_try_catch(node)
    }

    fn visit_catch_clause(
        &mut self,
        node: &dsl::textual::CatchClause,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_catch_clause(node)
    }

    fn visit_jump(&mut self, node: &dsl::textual::Jump) -> Result<Self::Value, Diagnostic> {
        self.render_jump(node)
    }

    fn visit_label_statement(
        &mut self,
        node: &dsl::textual::LabelStatement,
    ) -> Result<Self::Value, Diagnostic> {
        self.visit_id(&node.name)?;
        self.write(":");
        self.newline();

        Ok(())
    }

    fn visit_conditional_call(
        &mut self,
        node: &dsl::textual::ConditionalCall,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_conditional_call(node)
    }

    fn visit_wait(&mut self, node: &dsl::textual::Wait) -> Result<Self::Value, Diagnostic> {
        self.render_wait(node)
    }

    fn visit_throw(&mut self, node: &dsl::textual::Throw) -> Result<Self::Value, Diagnostic> {
        self.render_throw(node)
    }

    fn visit_if(&mut self, node: &dsl::textual::If) -> Result<Self::Value, Diagnostic> {
        self.render_if(node)
    }

    fn visit_else_if(&mut self, node: &dsl::textual::ElseIf) -> Result<Self::Value, Diagnostic> {
        self.render_else_if(node)
    }

    fn visit_case(&mut self, node: &dsl::textual::Case) -> Result<Self::Value, Diagnostic> {
        self.render_case(node)
    }

    fn visit_case_statement_group(
        &mut self,
        node: &dsl::textual::CaseStatementGroup,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_case_statement_group(node)
    }

    fn visit_for(&mut self, node: &dsl::textual::For) -> Result<Self::Value, Diagnostic> {
        self.render_for(node)
    }

    fn visit_while(&mut self, node: &dsl::textual::While) -> Result<Self::Value, Diagnostic> {
        self.render_while(node)
    }

    fn visit_expr_kind(
        &mut self,
        node: &dsl::textual::ExprKind,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_expr_kind(node)
    }

    fn visit_deref_variable(
        &mut self,
        node: &dsl::textual::DerefVariable,
    ) -> Result<Self::Value, Diagnostic> {
        self.visit_symbolic_variable_kind(&node.variable)?;
        self.write("^");
        Ok(())
    }

    // OOP extension: `THIS^` / `SUPER^`. The caret is part of the node's
    // spelling, so it can never be split from the keyword, and whatever
    // element follows (`.field`, `[i]`) is written with no gap -- which is
    // what the parser requires after a caret.
    fn visit_self_ref_variable(
        &mut self,
        node: &dsl::textual::SelfRefVariable,
    ) -> Result<Self::Value, Diagnostic> {
        self.write_ws(node.kind.spelling());
        Ok(())
    }

    fn visit_method_receiver(
        &mut self,
        node: &dsl::textual::MethodReceiver,
    ) -> Result<Self::Value, Diagnostic> {
        match node {
            dsl::textual::MethodReceiver::Instance(name) => self.visit_id(name),
            dsl::textual::MethodReceiver::SelfRef(self_ref) => {
                self.visit_self_ref_variable(self_ref)
            }
        }
    }

    fn visit_reference_declaration(
        &mut self,
        node: &ReferenceDeclaration,
    ) -> Result<Self::Value, Diagnostic> {
        self.visit_type_name(&node.type_name)?;
        self.write_ws(":");
        self.write_ref_keyword(&node.syntax);
        self.visit_reference_target(&node.target)
    }

    fn visit_reference_initializer(
        &mut self,
        node: &ReferenceInitializer,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_reference_initializer(node)
    }

    fn visit_reference_target(
        &mut self,
        node: &ReferenceTarget,
    ) -> Result<Self::Value, Diagnostic> {
        match node {
            ReferenceTarget::Named(type_name) => self.visit_type_name(type_name),
            ReferenceTarget::Array(subranges) => self.visit_array_subranges(subranges),
        }
    }

    fn visit_reference_initial_value(
        &mut self,
        node: &ReferenceInitialValue,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_reference_initial_value(node)
    }

    fn visit_named_input(
        &mut self,
        node: &dsl::textual::NamedInput,
    ) -> Result<Self::Value, Diagnostic> {
        self.visit_id(&node.name)?;
        self.write_ws(":=");
        self.visit_expr(&node.expr)
    }

    fn visit_output(&mut self, node: &dsl::textual::Output) -> Result<Self::Value, Diagnostic> {
        if node.not {
            self.write_ws("NOT");
        }
        self.visit_id(&node.src)?;
        self.write_ws("=>");
        self.visit_variable(&node.tgt)
    }

    fn visit_array_variable(
        &mut self,
        node: &dsl::textual::ArrayVariable,
    ) -> Result<Self::Value, Diagnostic> {
        self.render_array_variable(node)
    }

    fn visit_structured_variable(
        &mut self,
        node: &dsl::textual::StructuredVariable,
    ) -> Result<Self::Value, Diagnostic> {
        self.visit_symbolic_variable_kind(&node.record)?;
        self.write(".");
        self.write(node.field.original().as_str());

        Ok(())
    }

    fn visit_bit_access_variable(
        &mut self,
        node: &dsl::textual::BitAccessVariable,
    ) -> Result<Self::Value, Diagnostic> {
        self.visit_symbolic_variable_kind(&node.variable)?;
        self.write(".");
        self.write(&node.index.value.to_string());
        Ok(())
    }

    fn visit_partial_access_variable(
        &mut self,
        node: &dsl::textual::PartialAccessVariable,
    ) -> Result<Self::Value, Diagnostic> {
        self.visit_symbolic_variable_kind(&node.variable)?;
        self.write(".");
        self.write(node.size.prefix());
        self.write(&node.index.value.to_string());
        Ok(())
    }
}
