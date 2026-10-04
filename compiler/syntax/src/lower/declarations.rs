//! A declaration read apart: what its type is, and what its initial value is.
//!
//! A type declaration, a structure member and a variable declaration all write
//! `names : type [:= value]`, and what the legacy grammar built for them was
//! decided by ordered choice over the two parts together. Here the two parts
//! are classified once, and the decision is a row of a table:
//!
//! - the type is a [`Form`], chosen by the kind of the node that writes it
//!   (`FORMS`), refined by what the node holds;
//! - the value is an [`Init`], chosen by the kind of the node that writes it
//!   (`INITS`);
//! - a table of [`Row`]s, one for each place that reads declarations, names
//!   the forms and values it builds and the rule that builds each.
//!
//! A pair no row names is reported: a value that does not fit its type is a
//! mismatch of the initial value (P4022), and a type no row names is not
//! allowed where it stands. A form added to the language is a row, in the
//! tables that read it.

use super::names::{lower_type_ref, lower_type_token, type_keyword};
use super::tree::{child_of, significant_tokens};
use super::LowerCx;
use crate::syntax_kind::{SyntaxKind as K, SyntaxNode, SyntaxToken};
use ironplc_dsl::common::{ElementaryTypeName, TypeName};
use ironplc_dsl::core::Id;
use ironplc_dsl::diagnostic::{Diagnostic, Label};
use ironplc_problems::Problem;

/// What the type of a declaration is.
#[derive(Clone, Copy, PartialEq, Eq, Debug)]
pub enum Form {
    /// `ARRAY [ranges] OF T`.
    Array,
    /// `STRING[n]` and `WSTRING(n)`: a string with a length.
    SizedString,
    /// `STRING` and `WSTRING` without a length.
    BareString,
    /// `REF_TO T`, `REFERENCE TO T` and `POINTER TO T`.
    Reference,
    /// `PARAMS(n) OF T`.
    Params,
    /// `(A, B)`, with the values of a declaration and a base type.
    Enumeration,
    /// `INT(1..10)`.
    Subrange,
    /// `STRUCT ... END_STRUCT`.
    Struct,
    /// `UNION ... END_UNION`.
    Union,
    /// An elementary type, written as its keyword.
    Elementary,
    /// A type written as a name, which only a later stage can classify.
    Named,
    /// A name followed by the arguments of a function block instance.
    Call,
    /// No type at all: a global declaration that stops after its `:`.
    Absent,
}

impl Form {
    /// How a message names this form of type.
    pub fn describe(self) -> &'static str {
        match self {
            Form::Array => "an array",
            Form::SizedString | Form::BareString => "a string",
            Form::Reference => "a reference",
            Form::Params => "a parameter list",
            Form::Enumeration => "an enumeration",
            Form::Subrange => "a subrange",
            Form::Struct => "a structure",
            Form::Union => "a union",
            Form::Elementary => "an elementary type",
            Form::Named => "a named type",
            Form::Call => "a function block instance",
            Form::Absent => "no type",
        }
    }
}

/// What the initial value of a declaration is.
#[derive(Clone, Copy, PartialEq, Eq, Debug)]
pub enum Init {
    /// There is none.
    None,
    /// `[1, 2, 3]`.
    Array,
    /// `(a := 1, b := 2)`.
    Struct,
    /// `Type#Value`.
    Qualified,
    /// A name alone: an enumeration value, or the name of a constant.
    Name,
    /// Any other value: a literal, an expression, `NULL`, `REF(x)`.
    Value,
}

impl Init {
    /// How a message names this kind of initial value.
    pub fn describe(self) -> &'static str {
        match self {
            Init::None => "no value",
            Init::Array => "an array value",
            Init::Struct => "a structure value",
            Init::Qualified => "a qualified enumeration value",
            Init::Name => "a name",
            Init::Value => "a value",
        }
    }
}

/// The kinds of node that write a type, and the form each is. A node that
/// holds more than the kind says is refined by `refine`.
const FORMS: &[(K, Form)] = &[
    (K::ArrayType, Form::Array),
    (K::StringType, Form::BareString),
    (K::RefType, Form::Reference),
    (K::ParamsType, Form::Params),
    (K::EnumType, Form::Enumeration),
    (K::SubrangeType, Form::Subrange),
    (K::StructType, Form::Struct),
    (K::UnionType, Form::Union),
    (K::TypeRef, Form::Named),
];

/// The kinds of node that write an initial value, and the kind of value each
/// is. Any other node is a [`Init::Value`].
const INITS: &[(K, Init)] = &[
    (K::ArrayInit, Init::Array),
    (K::StructInit, Init::Struct),
    (K::EnumValueRef, Init::Qualified),
    (K::NameRef, Init::Name),
];

/// A declaration, read apart.
pub struct Parts {
    /// The declaration node.
    pub node: SyntaxNode,
    /// The node that writes the type: the declaration itself when it writes
    /// none ([`Form::Absent`]).
    pub spec: SyntaxNode,
    /// The value after `:=`, when there is one.
    pub value: Option<SyntaxNode>,
    pub form: Form,
    pub init: Init,
}

/// True when `token` is the keyword of an elementary type: not a generic type,
/// not a name, and not a keyword the dialect leaves available as a name.
fn is_elementary(cx: &LowerCx, token: &SyntaxToken) -> bool {
    type_keyword(cx, token)
        .is_some_and(|spelling| ElementaryTypeName::try_from(&Id::from(spelling)).is_ok())
}

/// A form that a node of the kind of another form holds more than: it is the
/// form `to` when `when` holds of the type node and its declaration.
struct Refinement {
    from: Form,
    when: fn(&LowerCx, &SyntaxNode, &SyntaxNode) -> bool,
    to: Form,
}

/// The refinements, the first that holds deciding. A string with a length
/// differs from one without; a name followed by arguments is an instance, and
/// a keyword type differs from a name.
const REFINEMENTS: &[Refinement] = &[
    Refinement {
        from: Form::BareString,
        when: |_, spec, _| spec.first_child().is_some(),
        to: Form::SizedString,
    },
    Refinement {
        from: Form::Named,
        when: |_, _, declaration| child_of(declaration, K::ArgList).is_some(),
        to: Form::Call,
    },
    Refinement {
        from: Form::Named,
        when: |cx, spec, _| {
            significant_tokens(spec)
                .first()
                .is_some_and(|token| is_elementary(cx, token))
        },
        to: Form::Elementary,
    },
];

/// The form a type node is: its row of `FORMS`, refined by what the node
/// holds.
fn form_of(cx: &LowerCx, spec: &SyntaxNode, declaration: &SyntaxNode) -> Option<Form> {
    let (_, form) = FORMS.iter().find(|(kind, _)| *kind == spec.kind())?;
    Some(
        REFINEMENTS
            .iter()
            .find(|refinement| refinement.from == *form && (refinement.when)(cx, spec, declaration))
            .map_or(*form, |refinement| refinement.to),
    )
}

/// The kind of value a node writes.
fn init_of(value: Option<&SyntaxNode>) -> Init {
    match value {
        None => Init::None,
        Some(node) => INITS
            .iter()
            .find(|(kind, _)| *kind == node.kind())
            .map_or(Init::Value, |(_, init)| *init),
    }
}

/// Reads a declaration node apart: the node that writes its type, and the
/// value that follows `:=`.
pub fn read(cx: &LowerCx, node: &SyntaxNode) -> Result<Parts, Diagnostic> {
    let written = node
        .children()
        .find(|child| FORMS.iter().any(|(kind, _)| *kind == child.kind()));
    let (spec, form) = match written {
        Some(spec) => {
            let form = form_of(cx, &spec, node).ok_or_else(|| cx.unsupported(&spec))?;
            (spec, form)
        }
        None => (node.clone(), Form::Absent),
    };
    let value = child_of(node, K::Initializer).and_then(|initializer| initializer.first_child());
    let init = init_of(value.as_ref());
    Ok(Parts {
        node: node.clone(),
        spec,
        value,
        form,
        init,
    })
}

impl Parts {
    /// The type the declaration is written against, when its type is one
    /// name: the keyword or name of an elementary or a named type, or the
    /// keyword of a string without a length.
    pub fn base_name(&self, cx: &LowerCx) -> Result<TypeName, Diagnostic> {
        match self.spec.kind() {
            K::TypeRef => lower_type_ref(cx, &self.spec),
            _ => significant_tokens(&self.spec)
                .first()
                .map(|token| lower_type_token(cx, token))
                .ok_or_else(|| cx.missing(&self.spec, "a type name")),
        }
    }

    /// The value after `:=`, which the caller knows is there.
    pub fn value(&self, cx: &LowerCx) -> Result<&SyntaxNode, Diagnostic> {
        self.value
            .as_ref()
            .ok_or_else(|| cx.missing(&self.node, "an initial value"))
    }

    /// The diagnostic for a value that does not fit the type (P4022).
    pub fn mismatch(&self, cx: &LowerCx) -> Diagnostic {
        let range = self
            .value
            .as_ref()
            .map_or_else(|| self.node.text_range(), |value| value.text_range());
        Diagnostic::problem(
            Problem::InitializerTypeMismatch,
            Label::span(
                cx.span(range),
                format!(
                    "{} is not an initial value of {}",
                    self.init.describe(),
                    self.form.describe()
                ),
            ),
        )
    }
}

/// A rule: builds what a declaration of the forms and values of a row means.
pub type Build<T> = fn(&LowerCx, &Parts) -> Result<T, Diagnostic>;

/// One row of a table: the forms of type and the kinds of value it builds
/// with `build`.
pub struct Row<T> {
    pub forms: &'static [Form],
    pub init: &'static [Init],
    pub build: Build<T>,
}

/// A row of forms and kinds of value built by `build`.
pub const fn row<T>(forms: &'static [Form], init: &'static [Init], build: Build<T>) -> Row<T> {
    Row { forms, init, build }
}

/// Builds the declaration in `node` with the row of `rows` that names its
/// form of type and its kind of value.
///
/// A form no row names is not allowed where it stands, and a form that a row
/// names with another kind of value has a value that does not fit it.
pub fn build<T>(cx: &LowerCx, rows: &[Row<T>], node: &SyntaxNode) -> Result<T, Diagnostic> {
    let parts = read(cx, node)?;
    let named = rows.iter().filter(|row| row.forms.contains(&parts.form));
    match named.clone().find(|row| row.init.contains(&parts.init)) {
        Some(row) => (row.build)(cx, &parts),
        None if named.count() > 0 => Err(parts.mismatch(cx)),
        None => Err(cx.syntax_error(
            parts.spec.text_range(),
            format!("{} is not allowed here", parts.form.describe()),
        )),
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::{parse_source_file, ParseOptions};
    use ironplc_dsl::core::FileId;

    /// The parts of the first declaration of `kind` in the source.
    fn parts_of(source: &str, kind: K) -> Parts {
        let parse = parse_source_file(source, &ParseOptions::all());
        let node = parse
            .root
            .descendants()
            .find(|node| node.kind() == kind)
            .expect("a declaration");
        read(&LowerCx::new(FileId::default()), &node).expect("a readable declaration")
    }

    fn form_and_init(type_text: &str) -> (Form, Init) {
        let parts = parts_of(&format!("TYPE t : {type_text}; END_TYPE"), K::TypeDecl);
        (parts.form, parts.init)
    }

    #[test]
    fn read_when_each_form_of_type_then_its_form_and_no_value() {
        let rows = [
            ("ARRAY[1..2] OF INT", Form::Array),
            ("STRING[10]", Form::SizedString),
            ("WSTRING(10)", Form::SizedString),
            ("STRING", Form::BareString),
            ("REF_TO INT", Form::Reference),
            ("REFERENCE TO INT", Form::Reference),
            ("POINTER TO INT", Form::Reference),
            ("PARAMS(3) OF INT", Form::Params),
            ("(A, B)", Form::Enumeration),
            ("INT(1..10)", Form::Subrange),
            ("STRUCT a : INT; END_STRUCT", Form::Struct),
            ("UNION a : INT; END_UNION", Form::Union),
            ("INT", Form::Elementary),
            ("TOD", Form::Elementary),
            ("my_type", Form::Named),
            ("ANY_NUM", Form::Named),
        ];
        for (text, form) in rows {
            assert_eq!(form_and_init(text), (form, Init::None), "{text}");
        }
    }

    #[test]
    fn read_when_name_has_arguments_then_an_instance_and_when_a_keyword_is_demoted_then_a_name() {
        let call = parts_of(
            "PROGRAM p VAR a : Fb(x := 1); END_VAR END_PROGRAM",
            K::VarDecl,
        );
        assert_eq!(call.form, Form::Call);
        let parse = parse_source_file("TYPE t : BIT; END_TYPE", &ParseOptions::default());
        let node = parse
            .root
            .descendants()
            .find(|node| node.kind() == K::TypeDecl)
            .expect("a declaration");
        let cx = LowerCx::new(FileId::default()).with_options(ParseOptions::default());
        let demoted = read(&cx, &node).expect("a declaration");
        assert_eq!(demoted.form, Form::Named);
    }

    #[test]
    fn read_when_declaration_stops_after_the_colon_then_no_type_and_the_declaration_stands_for_it()
    {
        let parts = parts_of("VAR_GLOBAL g :; END_VAR", K::VarDecl);
        assert_eq!((parts.form, parts.init), (Form::Absent, Init::None));
        assert_eq!(parts.spec, parts.node);
    }

    #[test]
    fn read_when_each_kind_of_value_then_its_kind() {
        let rows = [
            ("ARRAY[1..2] OF INT := [1, 2]", Init::Array),
            ("my_type := (a := 1)", Init::Struct),
            ("my_type := Color#Red", Init::Qualified),
            ("my_type := Red", Init::Name),
            ("INT := 5", Init::Value),
            ("INT := -5", Init::Value),
        ];
        for (text, init) in rows {
            assert_eq!(form_and_init(text).1, init, "{text}");
        }
    }

    #[test]
    fn base_name_when_keyword_or_name_or_string_then_the_type_it_names() {
        let name = |text: &str| {
            let parts = parts_of(&format!("TYPE t : {text}; END_TYPE"), K::TypeDecl);
            parts
                .base_name(&LowerCx::new(FileId::default()))
                .map(|name| name.name.original().to_string())
        };
        assert_eq!(name("tod").ok().as_deref(), Some("TIME_OF_DAY"));
        assert_eq!(name("MyType").ok().as_deref(), Some("MyType"));
        assert_eq!(name("wstring").ok().as_deref(), Some("WSTRING"));
    }

    #[test]
    fn build_when_value_does_not_fit_the_type_then_initializer_mismatch_at_the_value() {
        let parse = parse_source_file("TYPE t : INT := 5; END_TYPE", &ParseOptions::all());
        let node = parse
            .root
            .descendants()
            .find(|node| node.kind() == K::TypeDecl)
            .expect("a declaration");
        let cx = LowerCx::new(FileId::default());
        let rows: &[Row<()>] = &[row(&[Form::Elementary], &[Init::None], |_, _| Ok(()))];
        let error = build(&cx, rows, &node).expect_err("a mismatch");
        assert_eq!(error.code, Problem::InitializerTypeMismatch.code());
        assert_eq!(
            (error.primary.location.start, error.primary.location.end),
            (16, 17)
        );
        assert!(
            error.primary.message.contains("a value"),
            "{}",
            error.primary.message
        );
    }

    #[test]
    fn build_when_no_row_names_the_form_then_syntax_error_at_the_type() {
        let parse = parse_source_file("TYPE t : INT; END_TYPE", &ParseOptions::all());
        let node = parse
            .root
            .descendants()
            .find(|node| node.kind() == K::TypeDecl)
            .expect("a declaration");
        let cx = LowerCx::new(FileId::default());
        let rows: &[Row<()>] = &[row(&[Form::Array], &[Init::None], |_, _| Ok(()))];
        let error = build(&cx, rows, &node).expect_err("a type not allowed");
        assert_eq!(error.code, Problem::SyntaxError.code());
        assert_eq!(
            (error.primary.location.start, error.primary.location.end),
            (9, 12)
        );
    }

    #[test]
    fn describe_when_any_form_or_kind_of_value_then_words_for_a_message() {
        for form in [
            Form::Array,
            Form::SizedString,
            Form::BareString,
            Form::Reference,
            Form::Params,
            Form::Enumeration,
            Form::Subrange,
            Form::Struct,
            Form::Union,
            Form::Elementary,
            Form::Named,
            Form::Call,
            Form::Absent,
        ] {
            assert!(!form.describe().is_empty(), "{form:?}");
        }
        for init in [
            Init::None,
            Init::Array,
            Init::Struct,
            Init::Qualified,
            Init::Name,
            Init::Value,
        ] {
            assert!(!init.describe().is_empty(), "{init:?}");
        }
    }
}
