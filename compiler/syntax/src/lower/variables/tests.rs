use super::*;
use crate::lower::{INTERNAL_ERROR, NOT_IMPLEMENTED};
use crate::{parse_expression, ParseOptions, SyntaxKind, MAX_DEPTH};
use ironplc_dsl::common::{LocationPrefix, SizePrefix};
use ironplc_dsl::core::FileId;
use ironplc_dsl::textual::ExprKind;
use ironplc_problems::Problem;
use rowan::{GreenNode, NodeOrToken};

fn file() -> FileId {
    FileId::from_string("t.st")
}

/// Every flag on, so that `THIS` and `SUPER` are words of the language.
fn all() -> ParseOptions {
    ParseOptions::all()
}

/// The first node of `source`, ignoring any error the parse reported, so that
/// malformed trees reach the rules.
fn node_of(source: &str) -> Option<SyntaxNode> {
    parse_expression(source, &all()).root.first_child()
}

fn lower_result(source: &str) -> Result<Variable, Diagnostic> {
    let node = node_of(source).ok_or_else(Diagnostic::internal_error)?;
    lower_variable(&LowerCx::new(file()), &node)
}

fn lower(source: &str) -> Variable {
    lower_result(source).expect("the variable lowers")
}

fn symbolic(source: &str) -> SymbolicVariableKind {
    match lower(source) {
        Variable::Symbolic(symbolic) => symbolic,
        other => unreachable!("{source} is not symbolic: {other:?}"),
    }
}

fn code(result: Result<Variable, Diagnostic>) -> Option<String> {
    result.err().map(|diagnostic| diagnostic.code)
}

#[test]
fn lower_variable_when_selectors_then_written_back_the_same() {
    // `Display` writes a variable as the selectors are applied, so what is
    // read back is the nesting.
    let rows = [
        ("a", "a"),
        ("a.b", "a.b"),
        ("a.b.c", "a.b.c"),
        ("a[1]", "a[1]"),
        ("a[1, 2]", "a[1, 2]"),
        ("a[1][2]", "a[1][2]"),
        ("a.b[1].c", "a.b[1].c"),
        ("a.1", "a.1"),
        ("a.%X3", "a.3"),
        ("a.%B1", "a.%B1"),
        ("a.%W2", "a.%W2"),
        ("a.%D3", "a.%D3"),
        ("a.%L4", "a.%L4"),
        ("a[1].%W0", "a[1].%W0"),
        ("a.b.%D0", "a.b.%D0"),
        ("a^.b", "a^.b"),
        ("a^[1]", "a^[1]"),
        ("a^[1]^.b", "a^[1]^.b"),
        ("THIS^", "THIS^"),
        ("THIS^.x", "THIS^.x"),
        ("SUPER^.x[1]", "SUPER^.x[1]"),
        ("__CURRENTTASK^.Index", "__CURRENTTASK^.Index"),
    ];
    for (source, expected) in rows {
        assert_eq!(lower(source).to_string(), expected, "{source}");
    }
}

#[test]
fn lower_variable_when_chain_then_each_selector_wraps_what_comes_before_it() {
    // `a.b[1].c`: the member `c` of the element 1 of the member `b` of `a`.
    let SymbolicVariableKind::Structured(outer) = symbolic("a.b[1].c") else {
        unreachable!("not a member");
    };
    assert_eq!(outer.field.original(), "c");
    let SymbolicVariableKind::Array(array) = *outer.record else {
        unreachable!("not a subscript");
    };
    assert_eq!(array.subscripts.len(), 1);
    let SymbolicVariableKind::Structured(inner) = *array.subscripted_variable else {
        unreachable!("not a member");
    };
    assert_eq!(inner.field.original(), "b");
    assert!(matches!(*inner.record, SymbolicVariableKind::Named(_)));
}

#[test]
fn lower_variable_when_dereference_inside_a_chain_then_a_dereference_variable() {
    let SymbolicVariableKind::Structured(member) = symbolic("a^.b") else {
        unreachable!("not a member");
    };
    let SymbolicVariableKind::Deref(deref) = *member.record else {
        unreachable!("not a dereference");
    };
    assert!(matches!(*deref.variable, SymbolicVariableKind::Named(_)));
}

#[test]
fn lower_variable_when_name_then_original_spelling_lower_case_and_position() {
    let SymbolicVariableKind::Named(named) = symbolic("  Speed ") else {
        unreachable!("not a name");
    };
    assert_eq!(named.name.original(), "Speed");
    assert_eq!(named.name.lower_case(), "speed");
    assert_eq!(
        (
            named.name.span.start,
            named.name.span.end,
            &named.name.span.file_id
        ),
        (2, 7, &file())
    );
}

#[test]
fn lower_variable_when_names_the_legacy_grammar_gives_no_position_then_positioned() {
    for name in ["STEP", "ON", "R_EDGE", "F_EDGE", "__CURRENTTASK"] {
        let SymbolicVariableKind::Named(named) = symbolic(name) else {
            unreachable!("{name} is not a name");
        };
        assert_eq!(
            (named.name.span.start, named.name.span.end),
            (0, name.len()),
            "{name}"
        );
    }
}

#[test]
fn lower_variable_when_member_then_the_field_is_positioned_at_its_name() {
    let SymbolicVariableKind::Structured(member) = symbolic("counter . OUT") else {
        unreachable!("not a member");
    };
    assert_eq!((member.field.span.start, member.field.span.end), (10, 13));
}

#[test]
fn lower_variable_when_bit_then_the_index_is_positioned_at_its_digits() {
    let SymbolicVariableKind::BitAccess(bit) = symbolic("a.12") else {
        unreachable!("not a bit");
    };
    assert_eq!(bit.index.value, 12);
    assert_eq!((bit.index.span.start, bit.index.span.end), (2, 4));
    assert_eq!(bit.index.span.file_id, file());
}

#[test]
fn lower_variable_when_partial_access_then_the_size_of_its_letter_and_the_digits_as_index() {
    let rows = [
        ("a.%B1", PartialAccessSize::Byte, 1),
        ("a.%W2", PartialAccessSize::Word, 2),
        ("a.%D13", PartialAccessSize::DWord, 13),
        ("a.%L4", PartialAccessSize::LWord, 4),
        ("a.%w2", PartialAccessSize::Word, 2),
    ];
    for (source, size, index) in rows {
        let SymbolicVariableKind::PartialAccess(partial) = symbolic(source) else {
            unreachable!("{source} is not a partial access");
        };
        assert_eq!(partial.size, size, "{source}");
        assert_eq!(partial.index.value, index, "{source}");
    }
}

#[test]
fn lower_variable_when_partial_access_then_the_index_is_positioned_at_the_digits_after_the_letter()
{
    let SymbolicVariableKind::PartialAccess(partial) = symbolic("a.%W12") else {
        unreachable!("not a partial access");
    };
    assert_eq!((partial.index.span.start, partial.index.span.end), (4, 6));
    assert_eq!(partial.index.span.file_id, file());
}

#[test]
fn lower_variable_when_bit_letter_then_the_same_bit_access_as_the_digit_form() {
    let letter = symbolic("a.%X3");
    let digit = symbolic("a.3");
    assert_eq!(letter, digit);
    let SymbolicVariableKind::BitAccess(bit) = letter else {
        unreachable!("not a bit");
    };
    assert_eq!((bit.index.span.start, bit.index.span.end), (4, 5));
}

#[test]
fn lower_variable_when_subscripts_then_each_is_an_expression() {
    let SymbolicVariableKind::Array(array) = symbolic("a[i + 1, 2]") else {
        unreachable!("not a subscript");
    };
    assert_eq!(array.subscripts.len(), 2);
    assert!(matches!(array.subscripts[0].kind, ExprKind::BinaryOp(_)));
    assert!(matches!(array.subscripts[1].kind, ExprKind::Const(_)));
}

#[test]
fn lower_variable_when_self_reference_then_the_keyword_kind_and_the_range_to_the_caret() {
    for (source, kind, end) in [
        ("THIS^", SelfRefKind::This, 5),
        ("SUPER^.x", SelfRefKind::Super, 6),
        ("this ^", SelfRefKind::This, 6),
    ] {
        let SymbolicVariableKind::SelfRef(self_ref) = (match symbolic(source) {
            SymbolicVariableKind::Structured(member) => *member.record,
            other => other,
        }) else {
            unreachable!("{source} is not a self reference");
        };
        assert_eq!(self_ref.kind, kind, "{source}");
        assert_eq!(
            (self_ref.position.start, self_ref.position.end),
            (0, end),
            "{source}"
        );
        assert_eq!(self_ref.position.file_id, file());
    }
}

#[test]
fn lower_variable_when_direct_address_then_location_size_address_and_position() {
    let rows = [
        ("%IX0.1", LocationPrefix::I, SizePrefix::X, vec![0, 1]),
        ("%QW4", LocationPrefix::Q, SizePrefix::W, vec![4]),
        ("%MD10", LocationPrefix::M, SizePrefix::D, vec![10]),
        ("%MB7", LocationPrefix::M, SizePrefix::B, vec![7]),
        ("%ML1", LocationPrefix::M, SizePrefix::L, vec![1]),
        ("%M7", LocationPrefix::M, SizePrefix::Nil, vec![7]),
        ("%IX10.11", LocationPrefix::I, SizePrefix::X, vec![10, 11]),
    ];
    for (source, location, size, address) in rows {
        match lower(source) {
            Variable::Direct(direct) => {
                assert_eq!(direct.location, location, "{source}");
                assert_eq!(direct.size, size, "{source}");
                assert_eq!(direct.address, address, "{source}");
                assert_eq!(
                    (direct.position.start, direct.position.end),
                    (0, source.len()),
                    "{source}"
                );
            }
            other => unreachable!("{source} is not a direct address: {other:?}"),
        }
    }
}

#[test]
fn lower_variable_when_selectors_are_as_many_as_the_tree_allows_then_lowered_without_recursion() {
    // On the smallest stack the compiler runs on, 1 MiB. The root, one node
    // for each selector and the name at the bottom make the depth.
    let depth = std::thread::Builder::new()
        .stack_size(1024 * 1024)
        .spawn(|| {
            let source = format!("a{}", ".b".repeat(MAX_DEPTH - 2));
            let parse = parse_expression(&source, &all());
            assert!(parse.is_ok(), "{:?}", parse.errors);
            let node = parse.root.first_child().expect("a node");
            let variable = lower_variable(&LowerCx::new(file()), &node).expect("lowers");
            let mut depth = 0;
            let mut current = match &variable {
                Variable::Symbolic(symbolic) => symbolic,
                Variable::Direct(_) => return 0,
            };
            while let SymbolicVariableKind::Structured(member) = current {
                depth += 1;
                current = &member.record;
            }
            depth
        })
        .ok()
        .and_then(|thread| thread.join().ok());
    assert_eq!(depth, Some(MAX_DEPTH - 2));
}

#[test]
fn lower_variable_when_index_is_too_large_then_syntax_error_over_the_selector() {
    let too_large = "99999999999999999999999999999999999999999";
    for source in [
        format!("a.{too_large}"),
        format!("a.%W{too_large}"),
        "%MW99999999999".to_string(),
    ] {
        assert_eq!(
            code(lower_result(&source)),
            Some(Problem::SyntaxError.code().to_string()),
            "{source}"
        );
    }
}

#[test]
fn lower_variable_when_subscript_list_is_empty_then_internal_error() {
    // `a[]` does not parse; the rule that finds the tree without the part the
    // grammar guarantees says so.
    assert_eq!(code(lower_result("a[]")), Some(INTERNAL_ERROR.to_string()));
}

#[test]
fn lower_variable_when_node_is_not_a_variable_then_internal_error() {
    for source in ["5", "a + b", "f(1)", "(a)"] {
        assert_eq!(
            code(lower_result(source)),
            Some(INTERNAL_ERROR.to_string()),
            "{source}"
        );
    }
}

#[test]
fn lower_variable_when_node_is_pending_then_not_implemented() {
    let parse = crate::parse_statements("x := 1;", &all());
    let statement = parse
        .root
        .descendants()
        .find(|node| node.kind() == SyntaxKind::AssignStmt)
        .expect("an assignment");
    let diagnostic = lower_variable(&LowerCx::new(file()), &statement).err();
    assert_eq!(
        diagnostic.map(|diagnostic| diagnostic.code),
        Some(NOT_IMPLEMENTED.to_string())
    );
}

fn empty(kind: SyntaxKind) -> SyntaxNode {
    SyntaxNode::new_root(GreenNode::new(rowan::SyntaxKind(kind as u16), vec![]))
}

#[test]
fn lower_variable_when_node_lacks_what_its_kind_requires_then_internal_error() {
    let cx = LowerCx::new(file());
    for kind in [
        SyntaxKind::NameRef,
        SyntaxKind::FieldExpr,
        SyntaxKind::IndexExpr,
        SyntaxKind::BitAccessExpr,
        SyntaxKind::PartialAccessExpr,
        SyntaxKind::DerefExpr,
        SyntaxKind::SelfRefExpr,
        SyntaxKind::DirectAddressExpr,
    ] {
        let diagnostic = lower_variable(&cx, &empty(kind)).err();
        assert_eq!(
            diagnostic.map(|diagnostic| diagnostic.code),
            Some(INTERNAL_ERROR.to_string()),
            "{kind:?}"
        );
    }
}

#[test]
fn lower_variable_when_partial_access_token_is_not_a_selector_then_internal_error() {
    // A token of the right kind with a letter no row names: not a tree the
    // lexer builds.
    let token = rowan::GreenToken::new(rowan::SyntaxKind(SyntaxKind::PartialAccess as u16), "%Z1");
    let name = rowan::GreenToken::new(rowan::SyntaxKind(SyntaxKind::Ident as u16), "a");
    let base = GreenNode::new(
        rowan::SyntaxKind(SyntaxKind::NameRef as u16),
        vec![NodeOrToken::Token(name)],
    );
    let green = GreenNode::new(
        rowan::SyntaxKind(SyntaxKind::PartialAccessExpr as u16),
        vec![NodeOrToken::Node(base), NodeOrToken::Token(token)],
    );
    let diagnostic = lower_variable(&LowerCx::new(file()), &SyntaxNode::new_root(green)).err();
    assert_eq!(
        diagnostic.map(|diagnostic| diagnostic.code),
        Some(INTERNAL_ERROR.to_string())
    );
}

#[test]
fn lower_self_ref_when_node_is_not_a_self_reference_then_internal_error() {
    let node = node_of("a").expect("a node");
    let diagnostic = lower_self_ref(&LowerCx::new(file()), &node).err();
    assert_eq!(
        diagnostic.map(|diagnostic| diagnostic.code),
        Some(INTERNAL_ERROR.to_string())
    );
}

#[test]
fn lower_variable_when_selectors_table_then_every_row_is_a_selector_kind_of_the_variable_area() {
    for (kind, _) in SELECTORS {
        assert_eq!(
            crate::lower::disposition(*kind),
            crate::lower::Disposition::Lowered(crate::lower::Area::Variable),
            "{kind:?}"
        );
        assert!(is_selector(*kind));
    }
    assert!(!is_selector(SyntaxKind::NameRef));
    assert_eq!(PARTIAL_ACCESS.len(), 5);
}
