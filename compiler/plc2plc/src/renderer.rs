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

/// How one kind of node is written out. A kind is rendered by exactly one
/// `impl Render`, which lives in the file of its family. Both directions are
/// checked by the compiler: a kind listed in [`render_rows`] without an impl
/// fails at `node.render(self)`, and an impl of a kind without a row fails
/// because [`Listed`] is a supertrait that only the rows implement.
trait Render: Listed {
    fn render(&self, r: &mut LibraryRenderer) -> Result<(), Diagnostic>;
}

/// Marks a kind of node that has a row in [`render_rows`]. Only
/// [`mark_listed`] implements it, so a `Render` impl needs a row.
#[diagnostic::on_unimplemented(
    message = "`{Self}` has a `Render` impl but no row in `render_rows!`",
    label = "add `visit_<kind> => {Self}` to `render_rows!` in renderer.rs"
)]
trait Listed {}

/// Defines, for each row `visit_x => T`, the [`Visitor`] method that renders a
/// `T` with its [`Render`] impl. The guard for the other direction, an impl
/// without a row, is the [`Listed`] supertrait of [`Render`].
macro_rules! render_visits {
    ($($visit:ident => $node:ty),* $(,)?) => {
        $(
            fn $visit(&mut self, node: &$node) -> Result<Self::Value, Diagnostic> {
                node.render(self)
            }
        )*
    };
}

/// Implements [`Listed`] for the kind of each row.
macro_rules! mark_listed {
    ($($visit:ident => $node:ty),* $(,)?) => {
        $( impl Listed for $node {} )*
    };
}

/// The rows: the one place that names the kinds of node the renderer writes
/// through [`Render`]; every other kind is written by the method of the same
/// name in the [`Visitor`] impl or left to the walk. The macro passes the rows
/// to the macro it is given, which is how one list serves the [`Visitor`]
/// methods ([`render_visits`]) and the guard ([`mark_listed`]).
macro_rules! render_rows {
    ($consumer:ident) => {
        $consumer! {
            visit_data_type_declaration_kind => DataTypeDeclarationKind,
            visit_enumerated_specification_init => EnumeratedSpecificationInit,
            visit_enumerated_value => EnumeratedValue,
            visit_subrange_declaration => SubrangeDeclaration,
            visit_subrange_initializer => SubrangeInitializer,
            visit_structure_declaration => StructureDeclaration,
            visit_union_declaration => UnionDeclaration,
            visit_structure_initialization_declaration => StructureInitializationDeclaration,
            visit_late_resolved_initializer => LateResolvedInitializer,
            visit_params_specification => ParamsSpecification,
            visit_string_declaration => StringDeclaration,
            visit_array_specification_kind => ArraySpecificationKind,
            visit_array_subranges => ArraySubranges,
            visit_subrange => ironplc_dsl::common::Subrange,
            visit_program_access_decl => ProgramAccessDecl,
            visit_var_decl => VarDecl,
            visit_address_assignment => AddressAssignment,
            visit_simple_initializer => SimpleInitializer,
            visit_string_initializer => StringInitializer,
            visit_enumerated_values_initializer => EnumeratedValuesInitializer,
            visit_array_initial_value_assignment => ArrayInitialValueAssignment,
            visit_enumerated_initial_value_assignment => EnumeratedInitialValueAssignment,
            visit_direct_variable_identifier => DirectVariableIdentifier,
            visit_function_declaration => FunctionDeclaration,
            visit_function_block_declaration => FunctionBlockDeclaration,
            visit_method_declaration => MethodDeclaration,
            visit_property_declaration => PropertyDeclaration,
            visit_namespace_declaration => NamespaceDeclaration,
            visit_interface_declaration => InterfaceDeclaration,
            visit_program_declaration => ProgramDeclaration,
            visit_network => dsl::sfc::Network,
            visit_transition => dsl::sfc::Transition,
            visit_action => dsl::sfc::Action,
            visit_action_association => dsl::sfc::ActionAssociation,
            visit_resource_declaration => dsl::configuration::ResourceDeclaration,
            visit_program_configuration => dsl::configuration::ProgramConfiguration,
            visit_configuration_declaration => dsl::configuration::ConfigurationDeclaration,
            visit_function_block_init => dsl::configuration::FunctionBlockInit,
            visit_located_var_init => LocatedVarInit,
            visit_task_configuration => dsl::configuration::TaskConfiguration,
            visit_fb_call => dsl::textual::FbCall,
            visit_method_call => dsl::textual::MethodCall,
            visit_stmt_kind => dsl::textual::StmtKind,
            visit_assignment => dsl::textual::Assignment,
            visit_compare_expr => dsl::textual::CompareExpr,
            visit_binary_expr => dsl::textual::BinaryExpr,
            visit_repeat => dsl::textual::Repeat,
            visit_try_catch => dsl::textual::TryCatch,
            visit_catch_clause => dsl::textual::CatchClause,
            visit_jump => dsl::textual::Jump,
            visit_conditional_call => dsl::textual::ConditionalCall,
            visit_wait => dsl::textual::Wait,
            visit_throw => dsl::textual::Throw,
            visit_if => dsl::textual::If,
            visit_else_if => dsl::textual::ElseIf,
            visit_case => dsl::textual::Case,
            visit_case_statement_group => dsl::textual::CaseStatementGroup,
            visit_for => dsl::textual::For,
            visit_while => dsl::textual::While,
            visit_expr_kind => dsl::textual::ExprKind,
            visit_reference_initializer => ReferenceInitializer,
            visit_reference_initial_value => ReferenceInitialValue,
            visit_array_variable => dsl::textual::ArrayVariable,
        }
    };
}

render_rows!(mark_listed);

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

    /// Writes the `:= [ ... ]` initial value of an array, or nothing when
    /// the array has none.
    fn render_array_initial_values(
        &mut self,
        values: &[ArrayInitialElementKind],
    ) -> Result<(), Diagnostic> {
        if !values.is_empty() {
            self.write_ws(":=");

            self.write_ws("[");
            visit_comma_separated!(self, values.iter(), ArrayInitialElementKind);
            self.write_ws("]");
        }
        Ok(())
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

    render_rows!(render_visits);

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

    fn visit_integer_literal(&mut self, node: &IntegerLiteral) -> Result<Self::Value, Diagnostic> {
        // A literal that names its type (`LINT#5`) keeps it: without the
        // prefix it re-parses as an untyped literal, which resolves to
        // another type.
        match node.data_type {
            Some(_) => {
                self.write_ws(node.to_string().as_str());
                Ok(())
            }
            None => self.visit_signed_integer(&node.value),
        }
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

    fn visit_enumerated_specification_values(
        &mut self,
        node: &EnumeratedSpecificationValues,
    ) -> Result<Self::Value, Diagnostic> {
        self.write_ws("(");

        visit_comma_separated!(self, node.values.iter(), EnumeratedValue);

        self.write_ws(")");
        Ok(())
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

    fn visit_structure_element_declaration(
        &mut self,
        node: &StructureElementDeclaration,
    ) -> Result<Self::Value, Diagnostic> {
        self.write_attributes(node);
        self.visit_id(&node.name)?;
        self.write_ws(":");
        self.visit_initial_value_assignment_kind(&node.init)
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

        self.render_array_initial_values(&node.init)
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

    // 2.6.2
    fn visit_step(&mut self, node: &dsl::sfc::Step) -> Result<Self::Value, Diagnostic> {
        self.write_ws("STEP");
        self.write_step_body(node)
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

    fn visit_label_statement(
        &mut self,
        node: &dsl::textual::LabelStatement,
    ) -> Result<Self::Value, Diagnostic> {
        self.visit_id(&node.name)?;
        self.write(":");
        self.newline();

        Ok(())
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

    fn visit_reference_target(
        &mut self,
        node: &ReferenceTarget,
    ) -> Result<Self::Value, Diagnostic> {
        match node {
            ReferenceTarget::Named(type_name) => self.visit_type_name(type_name),
            ReferenceTarget::Array(subranges) => self.visit_array_subranges(subranges),
        }
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
