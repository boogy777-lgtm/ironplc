#![allow(clippy::type_complexity)]

//! The compiler as individual stages (to enable testing).

use ironplc_dsl::{
    core::{FileId, Id, SourceSpan},
    diagnostic::{Diagnostic, Label},
    stack::within_stack_budget,
};
use ironplc_parser::options::CompilerOptions;
use ironplc_problems::Problem;
use log::debug;

use crate::{
    function_environment::FunctionEnvironmentBuilder,
    intermediates::special_operator::special_operator_signatures,
    ironplc_dsl::common::Library,
    observe::{self, Observer, Unobserved},
    pass_runner::{direct, pass},
    semantic_context::SemanticContext,
    semantic_rules::semantic,
    symbol_environment::{ScopeKind, SymbolEnvironment, SymbolKind},
    system_globals::SYSTEM_UPTIME_GLOBALS,
    type_environment::{TypeEnvironment, TypeEnvironmentBuilder},
    type_table, xform_fold_constant_expressions, xform_fold_initializer_expressions,
    xform_insert_implicit_deref, xform_int_to_bool_initializer, xform_mark_unwritten_constants,
    xform_named_to_positional_args, xform_remove_unsigned_abs, xform_resolve_adr,
    xform_resolve_constant_expressions, xform_resolve_decl_types, xform_resolve_expr_types,
    xform_resolve_late_bound_expr_kind, xform_resolve_late_bound_type_initializer,
    xform_resolve_symbol_and_function_environment, xform_resolve_type_aliases,
    xform_resolve_type_decl_environment, xform_toposort_declarations,
};

/// Analyze runs semantic analysis on the set of files as a self-contained and complete unit.
///
/// Returns `Ok((Library, SemanticContext))` containing the type-resolved AST and all type,
/// function, and symbol information gathered during analysis. If any analysis step found
/// errors, they are stored in `context.diagnostics()` rather than causing an `Err` return.
///
/// Returns `Err` only when no sources are provided or when the environments of the
/// language cannot be built, which no input causes. A recursive cycle is the error of
/// its members: it is reported with the other messages and the rest of the project is
/// analyzed.
///
/// The analysis runs on the stack budget (`ironplc_dsl::stack`): every pass
/// recurses as deep as the tree it is given.
pub fn analyze(
    sources: &[&Library],
    options: &CompilerOptions,
) -> Result<(Library, SemanticContext), Vec<Diagnostic>> {
    analyze_observed(sources, options, &Unobserved)
}

/// [`analyze`], told what each step of the analysis does: every pass and every
/// semantic rule is run through `observer` (see [`crate::observe`]). The result
/// is the one [`analyze`] gives.
pub fn analyze_observed<O: Observer>(
    sources: &[&Library],
    options: &CompilerOptions,
    observer: &O,
) -> Result<(Library, SemanticContext), Vec<Diagnostic>> {
    within_stack_budget(|| analyze_in_budget(sources, options, observer))
}

fn analyze_in_budget<O: Observer>(
    sources: &[&Library],
    options: &CompilerOptions,
    observer: &O,
) -> Result<(Library, SemanticContext), Vec<Diagnostic>> {
    if sources.is_empty() {
        let span = SourceSpan::range(0, 0).with_file_id(&FileId::default());
        return Err(vec![Diagnostic::problem(
            Problem::NoContent,
            Label::span(span, "First location"),
        )]);
    }
    let (library, mut context) = resolve_types_in_budget(sources, options, observer)?;

    if let Err(diagnostics) = semantic(&library, &context, options, observer) {
        context.add_diagnostics(diagnostics);
    }

    // TODO this is currently in progress. It isn't clear to me yet how this will influence
    // semantic analysis, but it should because the type table should influence rule checking.
    // For now, this is just after the rules as they were originally written.
    match direct!(observer, type_table(&library)) {
        Ok(type_table_result) => {
            debug!("{type_table_result:?}");
        }
        Err(diagnostics) => {
            context.add_diagnostics(diagnostics);
        }
    }

    Ok((library, context))
}

/// The environment of the elementary types and the standard function blocks,
/// which every analysis starts a type environment from.
fn build_type_environment<O: Observer>(observer: &O) -> Result<TypeEnvironment, Diagnostic> {
    observer.observe(observe::setup("type environment"), || {
        TypeEnvironmentBuilder::new()
            .with_elementary_types()
            .with_stdlib_function_blocks()
            .build()
    })
}

/// Resolves the types of the set of files, on the stack budget like
/// [`analyze`], which runs it first.
pub fn resolve_types(
    sources: &[&Library],
    options: &CompilerOptions,
) -> Result<(Library, SemanticContext), Vec<Diagnostic>> {
    within_stack_budget(|| resolve_types_in_budget(sources, options, &Unobserved))
}

fn resolve_types_in_budget<O: Observer>(
    sources: &[&Library],
    options: &CompilerOptions,
    observer: &O,
) -> Result<(Library, SemanticContext), Vec<Diagnostic>> {
    let mut diagnostics: Vec<Diagnostic> = vec![];

    // We want to analyze this as a complete set, so we need to join the items together
    // into a single library. Extend owns the item so after this we are free to modify
    let mut library = Library::new();
    for x in sources {
        let copy = observer.observe(observe::setup("merge sources"), || (*x).clone());
        library = library.extend(copy);
    }

    // Hard failures: these are foundational and all subsequent steps depend on them.
    let mut type_environment = build_type_environment(observer).map_err(|err| vec![err])?;

    let mut function_environment = observer.observe(observe::setup("function environment"), || {
        FunctionEnvironmentBuilder::new()
            .with_stdlib_functions()
            .build()
    });

    // Conditionally register dialect-extension functions gated by allow flags.
    if options.allow_sizeof {
        use crate::intermediates::stdlib_function::get_sizeof_function;
        function_environment
            .insert(get_sizeof_function())
            .map_err(|d| vec![d])?;
    }
    if options.allow_special_operators {
        for signature in special_operator_signatures() {
            function_environment
                .insert(signature)
                .map_err(|d| vec![d])?;
        }
    }

    let mut symbol_environment = SymbolEnvironment::new();

    // Register implicit system globals when the uptime feature is enabled.
    if options.allow_system_uptime_global {
        for global in &SYSTEM_UPTIME_GLOBALS {
            symbol_environment
                .insert_compiler_provided(
                    &Id::from(global.name),
                    SymbolKind::Variable,
                    &ScopeKind::Global,
                )
                .map_err(|e| vec![e])?;
        }
    }

    // Resolve constant references in type parameters (STRING lengths, array bounds).
    // Must run before toposort so that concrete integer values are available.
    // An unresolvable reference is diagnosed and left as a `Constant`.
    library = pass!(
        observer,
        library,
        &mut diagnostics,
        xform_resolve_constant_expressions,
        options
    );

    // Declaration ordering is required for all subsequent transforms. It also
    // computes the set of declarations reachable from PROGRAM roots, which
    // codegen uses to skip unused functions. A repeated declaration name
    // survives the sort; the environments built below diagnose it. A cycle is
    // the error of its members only: each is reported once and entered in the
    // type environment as a declaration with an error, and the other
    // declarations are ordered as if it were not there.
    let sorted = direct!(
        observer,
        xform_toposort_declarations(library, &mut type_environment)
    );
    let (mut library, reachable) = (sorted.library, sorted.reachable);
    diagnostics.extend(sorted.diagnostics);

    // A repeated type or function block name is diagnosed by the type
    // environment, which keeps the first declaration, and a declaration that
    // cannot be resolved is diagnosed and left out of it, so the rest of the
    // library still resolves.
    let before = diagnostics.len();
    library = pass!(
        observer,
        library,
        &mut diagnostics,
        xform_resolve_type_decl_environment,
        &mut type_environment
    );
    let first_derivation_found = diagnostics.len() - before;

    // An unresolvable declaration is diagnosed and kept as it was; the rest of
    // the library's declarations are resolved.
    library = pass!(
        observer,
        library,
        &mut diagnostics,
        xform_resolve_late_bound_expr_kind,
        &mut type_environment
    );
    library = pass!(
        observer,
        library,
        &mut diagnostics,
        xform_resolve_late_bound_type_initializer,
        &mut type_environment
    );

    // Give TwinCAT `REFERENCE TO` variables their auto-dereferencing semantics
    // (bare reads/writes go through the reference) and lower `__ISVALIDREF`.
    // Runs after late-bound expression resolution (so bare identifiers are
    // already `ExprKind::Variable`) but before symbol/function resolution (so
    // `__ISVALIDREF` is lowered before it would be flagged as undeclared) and
    // before the reference semantic rules. See
    // specs/design/reference-to-twincat.md (PR 2).
    library = pass!(
        observer,
        library,
        &mut diagnostics,
        xform_insert_implicit_deref,
        options
    );

    // Rewrite the `ADR(x)` address-of operator into `ExprKind::Ref` when
    // `allow_adr` is set. Runs after implicit-deref (so a `REFERENCE TO`
    // operand is not mis-addressed) and before symbol/function resolution
    // (so a recognized `ADR` is not reported as an undeclared function).
    // A diagnosed call is lowered to a placeholder.
    library = pass!(
        observer,
        library,
        &mut diagnostics,
        xform_resolve_adr,
        options
    );

    // Fold constant-expression VAR initializers (e.g. `scaled : LREAL := SCALE*4.0;`)
    // back into ordinary literal initializers, or diagnose. Must run before
    // any other pass touches `InitialValueAssignmentKind::SimpleExpr`.
    // A diagnosed initializer is still normalized, so no `SimpleExpr` reaches
    // later passes.
    library = pass!(
        observer,
        library,
        &mut diagnostics,
        xform_fold_initializer_expressions,
        options
    );

    // Rewrite integer 0/1 initializers on BOOL variables to boolean literals.
    // Short-circuits internally when allow_int_to_bool_initializer is false.
    library = pass!(
        observer,
        library,
        &mut diagnostics,
        xform_int_to_bool_initializer,
        &mut type_environment,
        options
    );

    // A structure member's declared value is only in its final form now:
    // late-bound resolution and folding have turned `c : Color := Blue` and
    // `n : INT := N * 2` into values. The environment derived the structure
    // types before that, so derive them again from the resolved
    // declarations; every structure, alias of one and aggregate holding one
    // then carries the members' values. Nothing holds an id from the first
    // derivation, which already diagnosed any repeated or unresolvable
    // declaration, so what the second derivation finds is not reported again.
    // It replaces the first environment unless it found more than the first
    // did, a declaration the first resolved and this one cannot.
    if let Ok(mut resolved_environment) = build_type_environment(observer) {
        let second = direct!(
            observer,
            xform_resolve_type_decl_environment(library, &mut resolved_environment)
        );
        library = second.library;
        if second.diagnostics.len() <= first_derivation_found {
            type_environment = resolved_environment;
        }
    }

    // A repeated declaration name is diagnosed here, by the environments, and
    // the first declaration is kept, so the rest of the library still resolves.
    library = pass!(
        observer,
        library,
        &mut diagnostics,
        xform_resolve_symbol_and_function_environment,
        &mut symbol_environment,
        &mut function_environment
    );

    // Convert named function call arguments to positional.
    // A diagnosed call keeps its named arguments.
    library = pass!(
        observer,
        library,
        &mut diagnostics,
        xform_named_to_positional_args,
        &function_environment
    );

    // Record the type id each declaration declares, entering types spelled
    // out in place as anonymous types.
    library = pass!(
        observer,
        library,
        &mut diagnostics,
        xform_resolve_decl_types,
        &mut type_environment
    );

    // Resolve expression types using the function environment.
    library = pass!(
        observer,
        library,
        &mut diagnostics,
        xform_resolve_expr_types,
        &mut type_environment,
        &function_environment,
        options
    );

    // Fold constant binary and unary expressions.
    library = pass!(
        observer,
        library,
        &mut diagnostics,
        xform_fold_constant_expressions
    );

    // ABS of an unsigned value is the value itself; no back end sees it.
    library = pass!(
        observer,
        library,
        &mut diagnostics,
        xform_remove_unsigned_abs,
        &type_environment
    );

    library = pass!(
        observer,
        library,
        &mut diagnostics,
        xform_resolve_type_aliases,
        &type_environment,
        &mut symbol_environment
    );

    // Mark every variable the program never writes as CONSTANT, so the
    // semantic rules and codegen see one notion of a constant variable.
    // Runs last: it needs bare identifiers resolved to variables, `ADR`
    // rewritten to `Ref`, user functions in the function environment and
    // named arguments made positional. Infallible.
    // See specs/design/constant-variable-inference.md.
    let library = direct!(
        observer,
        xform_mark_unwritten_constants(
            library,
            &type_environment,
            &function_environment,
            &symbol_environment
        )
    );

    // Generate and display useful symbol table information
    debug!("Type Environment:");
    debug!("{type_environment:?}");

    debug!("Symbol Environment:");
    debug!("{symbol_environment:?}");

    let mut context = SemanticContext::new(
        type_environment,
        function_environment,
        symbol_environment,
        reachable,
        *options,
    );
    context.add_diagnostics(diagnostics);

    Ok((library, context))
}

#[cfg(test)]
mod tests {
    use crate::stages::analyze;
    use ironplc_dsl::common::Library;
    use ironplc_dsl::core::FileId;
    use ironplc_parser::options::CompilerOptions;
    use ironplc_parser::parse_program;
    use ironplc_test::read_shared_resource;

    #[test]
    fn analyze_when_first_steps_then_result_is_ok() {
        let lib = parse_shared_library("first_steps.st");
        let res = analyze(&[&lib], &CompilerOptions::default());
        assert!(res.is_ok());
    }

    #[test]
    fn analyze_when_first_steps_semantic_error_then_ok_with_diagnostics() {
        let lib = parse_shared_library("first_steps_semantic_error.st");
        let res = analyze(&[&lib], &CompilerOptions::default());
        let (_library, context) = res.unwrap();
        assert!(context.has_diagnostics());
    }

    /// Issue #1566: a second file must not displace the first file's
    /// diagnostics. Every undefined variable across both files is reported.
    #[test]
    fn analyze_when_semantic_errors_in_two_files_then_reports_all() {
        let file_a = "
PROGRAM a
VAR
  x : INT;
END_VAR
  x := AAA_ONE;
  x := AAA_TWO;
END_PROGRAM";

        let file_b = "
PROGRAM b
VAR
  y : INT;
END_VAR
  y := BBB_ONE;
END_PROGRAM";

        let options = CompilerOptions::default();
        let library_a = parse_program(file_a, &FileId::from_string("a.st"), &options).unwrap();
        let library_b = parse_program(file_b, &FileId::from_string("b.st"), &options).unwrap();

        let (_library, context) = analyze(&[&library_a, &library_b], &options).unwrap();

        let reported: Vec<&String> = context
            .diagnostics()
            .iter()
            .flat_map(|d| &d.described)
            .collect();
        for expected in ["variable=AAA_ONE", "variable=AAA_TWO", "variable=BBB_ONE"] {
            assert!(
                reported.iter().any(|d| d.as_str() == expected),
                "expected {expected}, got {reported:?}"
            );
        }
    }

    #[test]
    fn analyze_2() {
        let lib = parse_shared_library("main.st");
        let res = analyze(&[&lib], &CompilerOptions::default());
        assert!(res.is_ok());
    }

    #[test]
    fn analyze_when_split_across_multiple_files_then_ok() {
        let program1 = "
TYPE
LOGLEVEL : (CRITICAL) := CRITICAL;
END_TYPE";

        let program2 = "
FUNCTION_BLOCK LOGGER
VAR_EXTERNAL CONSTANT
ResetCounterValue : LOGLEVEL;
END_VAR

END_FUNCTION_BLOCK";

        let program1 =
            parse_program(program1, &FileId::default(), &CompilerOptions::default()).unwrap();
        let program2 =
            parse_program(program2, &FileId::default(), &CompilerOptions::default()).unwrap();

        let result = analyze(&[&program1, &program2], &CompilerOptions::default());
        assert!(result.is_ok())
    }

    fn parse_shared_library(name: &'static str) -> Library {
        let src = read_shared_resource(name);
        parse_program(&src, &FileId::default(), &CompilerOptions::default()).unwrap()
    }

    // ---------------------------------------------------------------------
    // A diagnosed constant-expression initializer must report only its own
    // problem. The initializer-fold transform used to be reverted when it
    // diagnosed, leaking `SimpleExpr` nodes to later rules and raising a
    // P9998 internal error after every legitimate P4037.
    // ---------------------------------------------------------------------

    #[test]
    fn analyze_when_initializer_expression_and_flag_disabled_then_p4037_only() {
        let program = "
FUNCTION func : LREAL
VAR CONSTANT
d2r : LREAL := 3.0/180.0;
END_VAR
func := d2r;
END_FUNCTION";
        let lib = parse_program(program, &FileId::default(), &CompilerOptions::default()).unwrap();

        let (_library, context) = analyze(&[&lib], &CompilerOptions::default()).unwrap();

        let codes: Vec<&str> = context
            .diagnostics()
            .iter()
            .map(|d| d.code.as_str())
            .collect();
        assert!(codes.contains(&"P4037"), "expected P4037, got: {codes:?}");
        // No internal error from a rule observing an unfolded initializer.
        assert!(!codes.contains(&"P9998"), "unexpected P9998 in: {codes:?}");
        // No cascaded "constant must have initializer" — the declaration
        // does carry an initializer, it was merely diagnosed.
        assert!(!codes.contains(&"P4008"), "unexpected P4008 in: {codes:?}");
    }

    // ---------------------------------------------------------------------
    // Don't revert a whole library's type resolution because one unrelated
    // declaration failed to resolve.
    // ---------------------------------------------------------------------

    #[test]
    fn analyze_when_unrelated_pou_has_undeclared_type_then_valid_pou_unaffected() {
        let program = "
FUNCTION_BLOCK FB_A
VAR
    x : Undeclared_Type;
END_VAR
END_FUNCTION_BLOCK

FUNCTION_BLOCK FB_Callee
END_FUNCTION_BLOCK

FUNCTION_BLOCK FB_B
VAR
    inst : FB_Callee;
END_VAR
    inst();
END_FUNCTION_BLOCK
        ";
        let lib = parse_program(program, &FileId::default(), &CompilerOptions::default()).unwrap();
        let (_library, context) = analyze(&[&lib], &CompilerOptions::default()).unwrap();

        let diagnostics = context.diagnostics();
        assert_eq!(
            1,
            diagnostics.len(),
            "expected exactly one diagnostic, got {diagnostics:?}"
        );
        assert_eq!("P2008", diagnostics[0].code);
    }

    // ---------------------------------------------------------------------
    // Constant-expression VAR initializers.
    // ---------------------------------------------------------------------

    fn opts_with_constant_initializer_expressions() -> CompilerOptions {
        CompilerOptions {
            allow_constant_initializer_expressions: true,
            // Constants are only collected from true top-level VAR_GLOBAL
            // declarations, which since #1251 (P4028) require this flag.
            allow_top_level_var_global: true,
            ..CompilerOptions::default()
        }
    }

    #[test]
    fn analyze_when_constant_initializer_expression_and_flag_enabled_then_resolves() {
        let program = "
VAR_GLOBAL CONSTANT
    SCALE : LREAL := 2.5;
END_VAR
FUNCTION_BLOCK FB_Example
VAR
    scaled : LREAL := SCALE*4.0;
END_VAR
END_FUNCTION_BLOCK";
        let lib = parse_program(
            program,
            &FileId::default(),
            &opts_with_constant_initializer_expressions(),
        )
        .unwrap();
        let (_library, context) =
            analyze(&[&lib], &opts_with_constant_initializer_expressions()).unwrap();

        assert!(
            !context.has_diagnostics(),
            "unexpected diagnostics: {:?}",
            context.diagnostics()
        );
    }

    #[test]
    fn analyze_when_constant_initializer_expression_and_flag_disabled_then_diagnostics() {
        let program = "
VAR_GLOBAL CONSTANT
    SCALE : LREAL := 2.5;
END_VAR
FUNCTION_BLOCK FB_Example
VAR
    scaled : LREAL := SCALE*4.0;
END_VAR
END_FUNCTION_BLOCK";
        let lib = parse_program(program, &FileId::default(), &CompilerOptions::default()).unwrap();
        let (_library, context) = analyze(&[&lib], &CompilerOptions::default()).unwrap();

        assert!(context.has_diagnostics());
    }

    // ---------------------------------------------------------------------
    // FB-instance call-style initializer (distinct node).
    // ---------------------------------------------------------------------

    #[test]
    fn analyze_when_fb_call_style_init_references_earlier_declared_fb_then_only_not_implemented() {
        // End-to-end: the call-style initializer references an earlier-declared
        // FB. It must produce exactly the "not yet supported" diagnostic
        // (P9999 NotImplemented) from the deferring rule -- and crucially NOT
        // a spurious P2011 "Parent type is not declared", which would appear
        // if the new FunctionBlockCall node were not wired into toposort/type
        // resolution like the FunctionBlock node.
        use ironplc_problems::Problem;

        let program = "
FUNCTION_BLOCK FB_Comm
VAR_INPUT
    retries : INT;
END_VAR
END_FUNCTION_BLOCK

FUNCTION_BLOCK FB_Example
VAR
    comm : FB_Comm(retries := 3);
END_VAR
END_FUNCTION_BLOCK";
        let lib = parse_program(program, &FileId::default(), &CompilerOptions::default()).unwrap();
        let (_library, context) = analyze(&[&lib], &CompilerOptions::default()).unwrap();

        let codes: Vec<&str> = context
            .diagnostics()
            .iter()
            .map(|d| d.code.as_str())
            .collect();
        // P9999 == Problem::NotImplemented; the enum variant is #[deprecated]
        // (must be constructed via Diagnostic::not_implemented), so assert on
        // the stable code string rather than referencing the variant.
        assert!(codes.contains(&"P9999"), "expected P9999, got: {codes:?}");
        assert!(
            !codes.contains(&Problem::ParentTypeNotDeclared.code()),
            "unexpected spurious P2011: {codes:?}"
        );
    }

    // ---------------------------------------------------------------------
    // THIS^ / SUPER^ (parsed, not analyzed or executed).
    // ---------------------------------------------------------------------

    /// A program using `THIS^` is rejected, and P9999 is among the reasons.
    ///
    /// Deliberately asserts presence rather than an exact diagnostic set:
    /// several passes meet the construct and each says so, and pinning the
    /// set would turn every later improvement into a test edit. What must
    /// hold is that no pass quietly accepts it.
    #[rstest::rstest]
    #[case::this_field_write("    THIS^.count := 1;")]
    #[case::super_field_read("    count := SUPER^.count;")]
    #[case::this_method_call("    THIS^.Start();")]
    fn analyze_when_self_ref_then_rejected_with_not_implemented(#[case] body: &str) {
        let options = CompilerOptions {
            allow_fb_inheritance: true,
            ..CompilerOptions::default()
        };
        let program = format!(
            "
FUNCTION_BLOCK FB_Motor
VAR
    count : INT;
END_VAR
METHOD Start
    count := 1;
END_METHOD
METHOD Run
{body}
END_METHOD
END_FUNCTION_BLOCK"
        );
        let lib = parse_program(&program, &FileId::default(), &options).unwrap();
        let (_library, context) = analyze(&[&lib], &options).unwrap();

        let codes: Vec<&str> = context
            .diagnostics()
            .iter()
            .map(|d| d.code.as_str())
            .collect();
        assert!(
            codes.contains(&"P9999"),
            "expected P9999 among diagnostics, got: {codes:?}"
        );
    }

    /// The same function block without `THIS^` analyzes cleanly -- the new
    /// arms must not report anything for programs that do not use it.
    #[test]
    fn analyze_when_no_self_ref_then_no_not_implemented() {
        let options = CompilerOptions {
            allow_fb_inheritance: true,
            ..CompilerOptions::default()
        };
        let program = "
FUNCTION_BLOCK FB_Motor
VAR
    count : INT;
END_VAR
METHOD Start
    count := 1;
END_METHOD
END_FUNCTION_BLOCK";
        let lib = parse_program(program, &FileId::default(), &options).unwrap();
        let (_library, context) = analyze(&[&lib], &options).unwrap();

        let codes: Vec<&str> = context
            .diagnostics()
            .iter()
            .map(|d| d.code.as_str())
            .collect();
        assert!(codes.is_empty(), "expected no diagnostics, got: {codes:?}");
    }

    // ---------------------------------------------------------------------
    // The call hierarchy's other direction: a program is not a type, so no
    // POU can declare an instance of one or invoke one. Nothing enforces
    // this in a rule; it falls out of a program not being in the type
    // environment, which makes it the pipeline's behaviour to pin.
    // ---------------------------------------------------------------------

    /// The codes reported for a POU that names `Target`, a program, as the
    /// type of a variable and then invokes that variable.
    fn codes_for_pou_referencing_a_program(pou: &str) -> Vec<String> {
        let program = format!(
            "
PROGRAM Target
VAR
    x : INT;
END_VAR
    x := 1;
END_PROGRAM

{pou}"
        );
        let lib = parse_program(&program, &FileId::default(), &CompilerOptions::default()).unwrap();
        let (_library, context) = analyze(&[&lib], &CompilerOptions::default()).unwrap();
        context
            .diagnostics()
            .iter()
            .map(|d| d.code.clone())
            .collect()
    }

    #[test]
    fn analyze_when_function_block_declares_program_instance_then_undeclared_type() {
        let codes = codes_for_pou_referencing_a_program(
            "
FUNCTION_BLOCK Caller
VAR
    p : Target;
END_VAR
    p();
END_FUNCTION_BLOCK",
        );
        assert_eq!(vec!["P2008", "P4012"], codes);
    }

    #[test]
    fn analyze_when_function_declares_program_instance_then_undeclared_type() {
        let codes = codes_for_pou_referencing_a_program(
            "
FUNCTION Caller : BOOL
VAR
    p : Target;
END_VAR
    p();
    Caller := TRUE;
END_FUNCTION",
        );
        assert_eq!(vec!["P2008", "P4012"], codes);
    }

    // A program may not invoke a program either; only a resource instantiates
    // one.
    #[test]
    fn analyze_when_program_declares_program_instance_then_undeclared_type() {
        let codes = codes_for_pou_referencing_a_program(
            "
PROGRAM Caller
VAR
    p : Target;
END_VAR
    p();
END_PROGRAM",
        );
        assert_eq!(vec!["P2008", "P4012"], codes);
    }

    #[test]
    fn analyze_when_function_block_invokes_program_by_name_then_not_in_scope() {
        let codes = codes_for_pou_referencing_a_program(
            "
FUNCTION_BLOCK Caller
VAR
    y : INT;
END_VAR
    Target();
    y := 2;
END_FUNCTION_BLOCK",
        );
        assert_eq!(vec!["P4012"], codes);
    }

    // The one legitimate way to instantiate a program stays legitimate.
    #[test]
    fn analyze_when_resource_instantiates_program_then_ok() {
        let codes = codes_for_pou_referencing_a_program(
            "
CONFIGURATION Config
RESOURCE Res ON PLC
    TASK T(INTERVAL := T#100ms, PRIORITY := 1);
    PROGRAM Inst WITH T : Target;
END_RESOURCE
END_CONFIGURATION",
        );
        assert!(codes.is_empty(), "expected no diagnostics, got: {codes:?}");
    }
    // ---------------------------------------------------------------------
    // Arrays of function block instances, through the whole pipeline: the
    // dialect decides whether the declaration is accepted, and an accepted
    // array is an instance like any other for every rule that follows.
    // ---------------------------------------------------------------------

    const TIMER_ARRAY_PROGRAM: &str = "
PROGRAM main
VAR
    timers : ARRAY[0..2] OF TON;
    done : BOOL;
    i : DINT;
END_VAR
    FOR i := 0 TO 2 DO
        timers[i](IN := TRUE, PT := T#1s, Q => done);
        IF timers[i].Q THEN
            done := TRUE;
        END_IF;
    END_FOR;
END_PROGRAM";

    fn codes_under(options: &CompilerOptions, program: &str) -> Vec<String> {
        let lib = parse_program(program, &FileId::default(), options).unwrap();
        let (_library, context) = analyze(&[&lib], options).unwrap();
        context
            .diagnostics()
            .iter()
            .map(|d| d.code.clone())
            .collect()
    }

    #[test]
    fn analyze_when_array_of_instances_and_dialect_has_no_arrays_then_declaration_rejected_once() {
        let codes = codes_under(&CompilerOptions::default(), TIMER_ARRAY_PROGRAM);
        assert_eq!(vec!["P4075"], codes);
    }

    #[test]
    fn analyze_when_array_of_instances_and_codesys_dialect_then_no_diagnostics() {
        let codes = codes_under(
            &CompilerOptions::from_dialect(ironplc_parser::options::Dialect::Codesys),
            TIMER_ARRAY_PROGRAM,
        );
        assert!(codes.is_empty(), "expected no diagnostics, got: {codes:?}");
    }

    #[test]
    fn analyze_when_undeclared_instance_called_then_reported_once_as_not_in_scope() {
        let codes = codes_under(
            &CompilerOptions::default(),
            "PROGRAM main VAR x : INT; END_VAR missing(IN := TRUE); END_PROGRAM",
        );
        assert_eq!(vec!["P4012"], codes);
    }
}
