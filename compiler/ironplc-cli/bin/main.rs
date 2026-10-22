use std::path::PathBuf;

use clap::Parser;

use ironplc_cli::cli;
use ironplc_cli::logger;
use ironplc_cli::lsp;
use ironplc_container::integrity::{self, HASH_LEN};
use ironplc_dsl::diagnostic::WarningList;
use ironplc_dsl::stack::within_stack_budget;
use ironplc_parser::options::{
    describe_dialects, BehaviorPolicy, CompilerOptions, Dialect, EnumerationStrictness,
    StringToNumFailure, StringToNumNonNumeric,
};
use ironplc_problems::WarningSet;
use ironplc_project::download_record::DeviceGenerations;
use ironplc_project::path_record;
use ironplc_sources::LibraryName;

const VERSION: &str = env!("CARGO_PKG_VERSION");

#[derive(Parser, Debug)]
#[command(name = "ironplcc", about = "IronPLC compiler")]
struct Args {
    /// Turn on verbose logging. Repeat to increase verbosity.
    #[arg(short, long, action = clap::ArgAction::Count)]
    verbose: u8,

    /// Sets the logging to write to a file.
    #[arg(short, long)]
    log_file: Option<PathBuf>,

    /// Selects the subcommand.
    #[command(subcommand)]
    action: Action,
}

/// Newtype around [`Dialect`] used only as a `clap::ValueEnum` adapter.
///
/// We cannot implement `ValueEnum` directly on `Dialect` because of Rust's
/// orphan rule: both `clap::ValueEnum` and `Dialect` live in foreign crates.
/// Adding clap as a dep to the parser crate is undesirable.
#[derive(Clone, Copy, Debug, Default)]
struct ClapDialect(Dialect);

impl From<ClapDialect> for Dialect {
    fn from(c: ClapDialect) -> Self {
        c.0
    }
}

impl clap::ValueEnum for ClapDialect {
    fn value_variants<'a>() -> &'a [Self] {
        // Mirrors `Dialect::ALL`. A test below guards the two against drift.
        &[
            ClapDialect(Dialect::Iec61131_3Ed2),
            ClapDialect(Dialect::Iec61131_3Ed3),
            ClapDialect(Dialect::Rusty),
            ClapDialect(Dialect::Codesys),
            ClapDialect(Dialect::TwinCat),
        ]
    }

    fn to_possible_value(&self) -> Option<clap::builder::PossibleValue> {
        Some(clap::builder::PossibleValue::new(self.0.cli_name()).help(self.0.description()))
    }
}

/// Declares a `clap::ValueEnum` newtype around a behavior policy enum, for
/// the same orphan-rule reason as [`ClapDialect`]. The variants, their CLI
/// names and their help text all come from the policy's own
/// [`BehaviorPolicy`] impl, so the CLI cannot drift from the compiler.
macro_rules! clap_policy {
    ($name:ident, $policy:ident) => {
        #[derive(Clone, Copy, Debug)]
        struct $name($policy);

        impl clap::ValueEnum for $name {
            fn value_variants<'a>() -> &'a [Self] {
                // SAFETY-free transmute avoidance: build the slice once from
                // the policy's own list.
                static VARIANTS: std::sync::OnceLock<Vec<$name>> = std::sync::OnceLock::new();
                VARIANTS.get_or_init(|| $policy::ALL.iter().map(|p| $name(*p)).collect())
            }

            fn to_possible_value(&self) -> Option<clap::builder::PossibleValue> {
                Some(
                    clap::builder::PossibleValue::new(self.0.cli_name()).help(self.0.description()),
                )
            }
        }
    };
}

clap_policy!(ClapStringToNumNonNumeric, StringToNumNonNumeric);
clap_policy!(ClapStringToNumFailure, StringToNumFailure);
clap_policy!(ClapEnumerationStrictness, EnumerationStrictness);

/// How `check` writes its messages.
#[derive(clap::ValueEnum, Clone, Copy, Debug, Default)]
enum MessageFormat {
    /// For a person: stderr, with the words of the CODESYS Messages view
    /// (Error, Warning, Information) and the source snippet.
    #[default]
    Text,
    /// For a program: one JSON array on stdout with a message object (the
    /// fields of the CODESYS ScriptMessage) for each message, suppressed
    /// messages included.
    Json,
}

impl From<MessageFormat> for cli::Output {
    fn from(format: MessageFormat) -> Self {
        match format {
            MessageFormat::Text => cli::Output::Terminal,
            MessageFormat::Json => cli::Output::Json,
        }
    }
}

/// Shared arguments for commands that operate on source files.
#[derive(clap::Args, Debug)]
struct FileArgs {
    /// Files to include. Directory names can be given to
    /// add all files in the given directory.
    files: Vec<PathBuf>,

    /// Select the language dialect.
    /// Defaults to strict IEC 61131-3:2003 (Edition 2).
    /// Individual --allow-* flags can override the dialect's defaults.
    #[arg(long, default_value = "iec61131-3-ed2")]
    dialect: ClapDialect,

    /// Allow missing semicolons after keyword statements like END_IF and END_STRUCT.
    #[arg(long)]
    allow_missing_semicolon: bool,

    /// Allow VAR_GLOBAL declarations at the top level (outside CONFIGURATION).
    /// This is an extension not part of the IEC 61131-3 standard.
    #[arg(long)]
    allow_top_level_var_global: bool,

    /// Allow constant references in type parameters (e.g., STRING[MY_CONST]).
    /// This is an extension not part of the IEC 61131-3 standard.
    #[arg(long)]
    allow_constant_type_params: bool,

    /// Allow empty variable blocks (VAR END_VAR, VAR_INPUT END_VAR, etc.).
    /// This is an extension not part of the IEC 61131-3 standard.
    #[arg(long)]
    allow_empty_var_blocks: bool,

    /// Allow TIME to be used as a function name (e.g., TIME()).
    /// Required for OSCAT compatibility where TIME() reads the PLC system clock.
    #[arg(long)]
    allow_time_as_function_name: bool,

    /// Allow C-style comments (// line comments and /* block comments */).
    /// These are not part of the IEC 61131-3 standard.
    #[arg(long)]
    allow_c_style_comments: bool,

    /// Allow the IEC 61131-3:2013 long-time-type keywords (LTIME, LDATE, LTOD,
    /// LDT). Without this flag those words remain available as identifiers.
    #[arg(long)]
    allow_long_time_types: bool,

    /// Allow REF_TO, REF(), and NULL syntax (standardized in IEC 61131-3:2013)
    /// without enabling the rest of Edition 3. This is useful for libraries like
    /// OSCAT that use references but also use Edition 3 type names (LDT, LTIME)
    /// as identifiers.
    #[arg(long)]
    allow_ref_to: bool,

    /// Allow the Beckhoff TwinCAT/CODESYS `REFERENCE TO` reference type and the
    /// `REF=` binding operator. This is an extension, an alternative to
    /// `--allow-ref-to`; the `twincat` and `codesys` dialects enable it.
    #[arg(long)]
    allow_reference_to: bool,

    /// Allow the Beckhoff TwinCAT/CODESYS `POINTER TO` pointer type with
    /// explicit dereference (^). This is an extension; the `twincat` and
    /// `codesys` dialects enable it.
    #[arg(long)]
    allow_pointer_to: bool,

    /// Allow the ADR() address-of operator, which returns a typed pointer to
    /// a variable. This is an extension; the `twincat` and `codesys` dialects
    /// enable it.
    #[arg(long)]
    allow_adr: bool,

    /// Allow the Beckhoff TwinCAT/CODESYS `PERSISTENT` variable qualifier.
    /// This is an extension, not part of the IEC 61131-3 standard; the
    /// `twincat` and `codesys` dialects enable it.
    #[arg(long)]
    allow_persistent_var: bool,

    /// Allow arithmetic (+, -) and ordering comparisons (<, >, <=, >=) on REF_TO types.
    /// This is an extension not part of the IEC 61131-3 standard.
    #[arg(long)]
    allow_ref_arithmetic: bool,

    /// Allow REF() on stack-allocated variables (VAR_TEMP, FUNCTION VAR_INPUT/VAR_OUTPUT).
    /// Required for OSCAT type-punning patterns where the reference doesn't escape.
    #[arg(long)]
    allow_ref_stack_variables: bool,

    /// Allow assigning between REF_TO types of different base types (type punning).
    /// Required for OSCAT patterns like interpreting REAL bits as DWORD via
    /// REF(real_var) into a REF_TO DWORD.
    #[arg(long)]
    allow_ref_type_punning: bool,

    /// Allow integer literals (0 or 1) as BOOL variable initializers.
    /// This is an extension supported by CoDeSys, TwinCAT, and RuSTy.
    #[arg(long)]
    allow_int_to_bool_initializer: bool,

    /// Allow SIZEOF() operator that returns the size in bytes of a variable or type.
    /// This is an extension supported by CODESYS, TwinCAT, and RuSTy.
    #[arg(long)]
    allow_sizeof: bool,

    /// Expose __SYSTEM_UP_TIME (TIME) and __SYSTEM_UP_LTIME (LTIME) as implicit
    /// VAR_GLOBALs containing the VM's monotonic uptime in milliseconds.
    #[arg(long)]
    allow_system_uptime_global: bool,

    /// Allow implicit widening from a bit-string type to a strictly wider
    /// integer type (e.g. BYTE→INT). This is an extension.
    #[arg(long)]
    allow_cross_family_widening: bool,

    /// Allow implicit conversion between UDINT and DWORD, in both directions,
    /// at equal width. The two share a 32-bit slot, so this converts rather
    /// than widens. This is an extension.
    #[arg(long)]
    allow_cross_family_conversion: bool,

    /// Allow a bare integer literal where a bit-string type is expected
    /// (e.g. 0 where BYTE is expected). This is an extension.
    #[arg(long)]
    allow_int_literal_to_bit_string: bool,

    /// Allow arithmetic on bit-string operands (e.g. BYTE + 1), treating a
    /// bit string as the unsigned integer of its width. This is an
    /// extension.
    #[arg(long)]
    allow_bit_string_arithmetic: bool,

    /// Allow IEC 61131-3:2013 partial-access syntax: the bit form `.%Xn` (an
    /// alias for the short form `.n`) and the byte/word/dword/lword forms
    /// `.%Bn`, `.%Wn`, `.%Dn`, `.%Ln`.
    #[arg(long)]
    allow_partial_access_syntax: bool,

    /// Allow curly-brace pragmas; an attribute pragma ({attribute 'name'}) is attached
    /// to the declaration it stands before, any other is skipped.
    /// This is an extension not part of the IEC 61131-3 standard.
    #[arg(long)]
    allow_pragmas: bool,

    /// Allow the AND_THEN and OR_ELSE short-circuit boolean operators
    /// (Beckhoff/CODESYS extension).
    /// This is an extension not part of the IEC 61131-3 standard.
    #[arg(long)]
    allow_short_circuit_operators: bool,

    /// Allow an AT-located variable inside an otherwise plain
    /// VAR/VAR_INPUT/VAR_OUTPUT block, instead of requiring its own dedicated
    /// block. This is an extension not part of the IEC 61131-3 standard.
    #[arg(long)]
    allow_mixed_located_var_declarations: bool,

    /// Allow a VAR initializer to be a constant expression (e.g. SCALE*4.0)
    /// rather than only a bare literal. This is an extension not part
    /// of the IEC 61131-3 standard.
    #[arg(long)]
    allow_constant_initializer_expressions: bool,

    /// Allow hex/binary/octal bit-string literals (e.g. 16#D012, 2#1010) as
    /// CASE labels. This is an extension not part of the IEC 61131-3
    /// standard.
    #[arg(long)]
    allow_bit_string_case_labels: bool,

    /// Allow the STRING(n)/WSTRING(n) parenthesis length delimiter in addition
    /// to the standard STRING[n]/WSTRING[n] brackets. This is a dialect
    /// extension not part of the IEC 61131-3 standard.
    #[arg(long)]
    allow_paren_string_length: bool,

    /// Allow a general (non-constant) expression as the value in a
    /// structured/call-style initializer's `name := value` pairs (e.g.
    /// `tonDelta : TON := (PT := pDevice^.Delta);`). This is a dialect
    /// extension not part of the IEC 61131-3 standard.
    #[arg(long)]
    allow_struct_initializer_expressions: bool,

    /// Allow IEC 61131-3:2013 object-oriented syntax: EXTENDS/IMPLEMENTS/
    /// ABSTRACT on FUNCTION_BLOCK declarations, INTERFACE declarations,
    /// METHOD and PROPERTY declarations, and THIS/SUPER. Enabled by
    /// `--dialect=iec61131-3-ed3` and by the vendor dialects.
    #[arg(long)]
    allow_fb_inheritance: bool,

    /// Allow explicit per-member values in an enumeration declaration (e.g.
    /// `(Deutsch := 1, English := 2)`). Standardized in IEC 61131-3:2013, so
    /// not part of Edition 2.
    #[arg(long)]
    allow_enum_explicit_values: bool,

    /// Allow the base-type suffix on an enumeration declaration (e.g.
    /// `(A, B) WORD`), naming the elementary type the members are stored in.
    /// This is a dialect extension not part of the IEC 61131-3 standard.
    #[arg(long)]
    allow_enum_base_type: bool,

    /// Allow the BIT one-bit type and its literals BIT#0 and BIT#1. This is
    /// a dialect extension not part of the IEC 61131-3 standard; with the
    /// flag off, `bit` is an ordinary identifier.
    #[arg(long)]
    allow_bit_type: bool,

    /// Allow the CONTINUE statement, which goes on with the next iteration
    /// of the innermost loop. Standardized in IEC 61131-3:2013, so not part
    /// of Edition 2.
    #[arg(long)]
    allow_continue: bool,

    /// Allow UNION ... END_UNION type declarations inside a TYPE block.
    /// Standardized in IEC 61131-3:2013, so not part of Edition 2.
    #[arg(long)]
    allow_union_type: bool,

    /// Allow VAR_STAT variable sections, whose declarations keep their value
    /// between calls of the POU. This is a dialect extension not part of the
    /// IEC 61131-3 standard.
    #[arg(long)]
    allow_var_stat: bool,

    /// Allow VAR_INST variable sections in a method, whose declarations
    /// belong to the method's instance. This is a dialect extension not part
    /// of the IEC 61131-3 standard.
    #[arg(long)]
    allow_var_inst: bool,

    /// Allow VAR_GENERIC variable sections directly after a function block
    /// name, declaring the function block's generic constants. This is a
    /// dialect extension not part of the IEC 61131-3 standard.
    #[arg(long)]
    allow_var_generic: bool,

    /// Allow NAMESPACE ... END_NAMESPACE grouping of declarations. This is a
    /// dialect extension not part of the IEC 61131-3 standard.
    #[arg(long)]
    allow_namespace: bool,

    /// Allow the __BEGIN_IMPLEMENTATION marker that begins a POU's
    /// implementation section in a CODESYS textual export. This is a dialect
    /// extension not part of the IEC 61131-3 standard.
    #[arg(long)]
    allow_begin_implementation: bool,

    /// Allow the __TRY/__CATCH/__FINALLY/__ENDTRY exception handling
    /// statements and the __THROW statement.
    /// This is an extension not part of the IEC 61131-3 standard.
    #[arg(long)]
    allow_try_catch: bool,

    /// Allow the JMP statement and `label:` statement labels.
    /// This is an extension not part of the IEC 61131-3 standard.
    #[arg(long)]
    allow_jump_statement: bool,

    /// Allow the CALC conditional call statement (`CALC(cond, call)`).
    /// This is an extension not part of the IEC 61131-3 standard.
    #[arg(long)]
    allow_calc_statement: bool,

    /// Allow the __WAIT wait statement.
    /// This is an extension not part of the IEC 61131-3 standard.
    #[arg(long)]
    allow_wait_statement: bool,

    /// Allow nested block comments (`(* a (* b *) c *)`).
    /// This is an extension not part of the IEC 61131-3 standard.
    #[arg(long)]
    allow_nested_comments: bool,

    /// Allow the conditional-compilation pragmas `{IF}`/`{ELSIF}`/`{ELSE}`/
    /// `{END_IF}` together with `{DEFINE}` and `{UNDEFINE}`.
    /// This is an extension not part of the IEC 61131-3 standard.
    #[arg(long)]
    allow_pragma_if: bool,

    /// Allow the `PARAMS(n) OF T` parameter-list data type. A CODESYS
    /// extension, not part of the IEC 61131-3 standard.
    #[arg(long)]
    allow_params_of: bool,

    /// Allow the incomplete array type `ARRAY[*] OF T`, whose bounds the
    /// caller supplies. A CODESYS extension, not part of the IEC 61131-3
    /// standard.
    #[arg(long)]
    allow_incomplete_array: bool,

    /// Allow backtick-escaped identifiers such as `` `my name` ``. A CODESYS
    /// extension (non-compliant identifiers), not part of the IEC 61131-3
    /// standard.
    #[arg(long)]
    allow_escaped_identifiers: bool,

    /// Allow the CODESYS special operators `__NEW`, `__DELETE`, `__TYPEOF`
    /// and `__XADD`. They are typed but not executable; `__CURRENTTASK`,
    /// `__SYSTEM` and `__POOL` are recognised and reported as unsupported.
    /// A CODESYS extension, not part of the IEC 61131-3 standard.
    #[arg(long)]
    allow_special_operators: bool,

    /// Allow arrays of function block instances (`ARRAY[0..3] OF TON`) and
    /// calling an element (`fbs[i](IN := x);`). A CODESYS extension, not part
    /// of the IEC 61131-3 standard.
    #[arg(long)]
    allow_fb_instance_arrays: bool,

    /// Allow identifiers with letters outside the ASCII range. An extension
    /// (a CODESYS scanner option, off there by default too).
    #[arg(long)]
    allow_unicode_identifiers: bool,

    /// Allow consecutive underscores inside an identifier, e.g. `my__var`.
    /// CODESYS/TwinCAT reject them; the leading `__` compiler prefix is
    /// always reserved and always allowed.
    #[arg(long)]
    allow_multiple_underscores: bool,

    /// What STRING_TO_<numeric> treats as convertible when the string has
    /// non-numeric characters. A behavior policy: the dialect selects an
    /// alternative and this flag replaces it.
    #[arg(long, value_name = "ALTERNATIVE")]
    policy_string_to_num_non_numeric: Option<ClapStringToNumNonNumeric>,

    /// What STRING_TO_<numeric> does when the string is not convertible. A
    /// behavior policy: the dialect selects an alternative and this flag
    /// replaces it.
    #[arg(long, value_name = "ALTERNATIVE")]
    policy_string_to_num_failure: Option<ClapStringToNumFailure>,

    /// Which enumerations are no number: arithmetic, numerical functions and
    /// FOR are refused for them, and a number is not stored into them. A
    /// behavior policy: the dialect selects an alternative and this flag
    /// replaces it.
    #[arg(long, value_name = "ALTERNATIVE")]
    policy_enumeration_strictness: Option<ClapEnumerationStrictness>,

    /// Do not show, and do not count, the warning with this code. The code is
    /// written as the problem list writes it (P4083) or as CODESYS writes it
    /// (C0354), with the case exact; only the code of a warning is accepted.
    /// Repeat the flag for more warnings.
    #[arg(
        long = "disable-warning",
        value_name = "CODE",
        value_parser = parse_warning_code
    )]
    disable_warning: Vec<WarningSet>,

    /// Report the warning with this code as an error, with the same code: it
    /// counts as an error and stops the build. It wins over --disable-warning
    /// for the same code. The code is written as for --disable-warning. Repeat
    /// the flag for more warnings.
    #[arg(
        long = "warning-as-error",
        value_name = "CODE",
        value_parser = parse_warning_code
    )]
    warning_as_error: Vec<WarningSet>,
}

/// Reads the code of a warning given to a flag, with the one function every
/// front end reads a code with. A code that is not accepted is a usage error
/// that names the code and lists the ones that are.
fn parse_warning_code(code: &str) -> Result<WarningSet, String> {
    WarningSet::from_code(code).map_err(|text| text.to_string())
}

impl FileArgs {
    /// The warnings the flags of a list gave. The `match` has an arm for each
    /// list, so a list added to [`WarningList`] does not compile until it has a
    /// flag here.
    fn warnings_given(&self, list: WarningList) -> &[WarningSet] {
        match list {
            WarningList::Disabled => &self.disable_warning,
            WarningList::AsError => &self.warning_as_error,
        }
    }

    fn compiler_options(&self) -> CompilerOptions {
        let mut options = CompilerOptions::from_dialect(self.dialect.into());
        for list in WarningList::ALL {
            for warnings in self.warnings_given(list) {
                options.messages.add(list, *warnings);
            }
        }
        // A policy flag replaces the dialect's selection (unlike an
        // `--allow-*` flag, which can only enable).
        if let Some(policy) = self.policy_string_to_num_non_numeric {
            options.policy_string_to_num_non_numeric = policy.0;
        }
        if let Some(policy) = self.policy_string_to_num_failure {
            options.policy_string_to_num_failure = policy.0;
        }
        if let Some(policy) = self.policy_enumeration_strictness {
            options.policy_enumeration_strictness = policy.0;
        }
        // Individual flags override (can only enable, never disable).
        options.allow_missing_semicolon |= self.allow_missing_semicolon;
        options.allow_top_level_var_global |= self.allow_top_level_var_global;
        options.allow_constant_type_params |= self.allow_constant_type_params;
        options.allow_empty_var_blocks |= self.allow_empty_var_blocks;
        options.allow_time_as_function_name |= self.allow_time_as_function_name;
        options.allow_c_style_comments |= self.allow_c_style_comments;
        options.allow_long_time_types |= self.allow_long_time_types;
        options.allow_ref_to |= self.allow_ref_to;
        options.allow_reference_to |= self.allow_reference_to;
        options.allow_pointer_to |= self.allow_pointer_to;
        options.allow_adr |= self.allow_adr;
        options.allow_persistent_var |= self.allow_persistent_var;
        options.allow_ref_arithmetic |= self.allow_ref_arithmetic;
        options.allow_ref_stack_variables |= self.allow_ref_stack_variables;
        options.allow_ref_type_punning |= self.allow_ref_type_punning;
        options.allow_int_to_bool_initializer |= self.allow_int_to_bool_initializer;
        options.allow_sizeof |= self.allow_sizeof;
        options.allow_system_uptime_global |= self.allow_system_uptime_global;
        options.allow_cross_family_widening |= self.allow_cross_family_widening;
        options.allow_cross_family_conversion |= self.allow_cross_family_conversion;
        options.allow_int_literal_to_bit_string |= self.allow_int_literal_to_bit_string;
        options.allow_bit_string_arithmetic |= self.allow_bit_string_arithmetic;
        options.allow_partial_access_syntax |= self.allow_partial_access_syntax;
        options.allow_pragmas |= self.allow_pragmas;
        options.allow_short_circuit_operators |= self.allow_short_circuit_operators;
        options.allow_mixed_located_var_declarations |= self.allow_mixed_located_var_declarations;
        options.allow_constant_initializer_expressions |=
            self.allow_constant_initializer_expressions;
        options.allow_bit_string_case_labels |= self.allow_bit_string_case_labels;
        options.allow_paren_string_length |= self.allow_paren_string_length;
        options.allow_struct_initializer_expressions |= self.allow_struct_initializer_expressions;
        options.allow_fb_inheritance |= self.allow_fb_inheritance;
        options.allow_enum_explicit_values |= self.allow_enum_explicit_values;
        options.allow_enum_base_type |= self.allow_enum_base_type;
        options.allow_bit_type |= self.allow_bit_type;
        options.allow_continue |= self.allow_continue;
        options.allow_union_type |= self.allow_union_type;
        options.allow_var_stat |= self.allow_var_stat;
        options.allow_var_inst |= self.allow_var_inst;
        options.allow_var_generic |= self.allow_var_generic;
        options.allow_namespace |= self.allow_namespace;
        options.allow_begin_implementation |= self.allow_begin_implementation;
        options.allow_try_catch |= self.allow_try_catch;
        options.allow_jump_statement |= self.allow_jump_statement;
        options.allow_calc_statement |= self.allow_calc_statement;
        options.allow_wait_statement |= self.allow_wait_statement;
        options.allow_nested_comments |= self.allow_nested_comments;
        options.allow_pragma_if |= self.allow_pragma_if;
        options.allow_params_of |= self.allow_params_of;
        options.allow_incomplete_array |= self.allow_incomplete_array;
        options.allow_escaped_identifiers |= self.allow_escaped_identifiers;
        options.allow_special_operators |= self.allow_special_operators;
        options.allow_fb_instance_arrays |= self.allow_fb_instance_arrays;
        options.allow_unicode_identifiers |= self.allow_unicode_identifiers;
        options.allow_multiple_underscores |= self.allow_multiple_underscores;
        options
    }
}

#[derive(clap::Subcommand, Debug)]
enum Action {
    /// The check action checks a file (or set of files) for syntax and semantic correctness.
    ///
    /// When multiple files specified, then the files are checked as a single
    /// compilation unit (essentially by combining the files) for analysis.
    Check {
        #[command(flatten)]
        file_args: FileArgs,

        /// Activate a compatibility library by name (repeatable), e.g.
        /// `--library Tc2_System`. Activation injects the library's declarations
        /// (such as the TwinCAT `PI` constant) so they resolve under their
        /// exact vendor names. Use for source that has no project context; a
        /// discovered project file activates its referenced libraries
        /// automatically.
        #[arg(long = "library")]
        libraries: Vec<LibraryName>,

        /// How to write the messages: `text` for a person or `json` for a
        /// program.
        #[arg(long, value_enum, default_value_t)]
        format: MessageFormat,
    },
    /// Compiles source files into a bytecode container (.iplc) file.
    ///
    /// When multiple files are specified, then the files are compiled as a single
    /// compilation unit (essentially by combining the files).
    Compile {
        #[command(flatten)]
        file_args: FileArgs,

        /// Output file path for the compiled bytecode container (.iplc).
        #[arg(short, long)]
        output: PathBuf,

        /// Activate a compatibility library by name (repeatable), e.g.
        /// `--library Tc2_System`. See `check --library`.
        #[arg(long = "library")]
        libraries: Vec<LibraryName>,

        /// Build even when nothing changed. Without it, a compile whose inputs
        /// are those recorded in `<output>.build.iplcinfo` (named after the
        /// output file) and whose output file is the container that record
        /// describes does not compile, and reports that the application is up to
        /// date. The record is written after every successful build.
        #[arg(long)]
        force: bool,
    },
    /// The echo action reads (parses) the libraries and writes the context to the
    /// standard output.
    ///
    /// The echo acton is primarily for diagnostics to understand the internal
    /// structure of the parsed files.
    Echo {
        #[command(flatten)]
        file_args: FileArgs,
    },
    /// The tokenize action checks a file if it can be tokenized with all content
    /// matching a token.
    ///
    /// The tokenize acton is primarily for diagnostics to understand the internal
    /// structure of the parsed files.
    Tokenize {
        #[command(flatten)]
        file_args: FileArgs,
    },
    /// Run in Language Server Protocol mode to integrate with development tools.
    Lsp {
        #[arg(long)]
        stdio: bool,
    },
    /// Show available dialects and which features each enables.
    Dialects,
    /// Refactoring commands for maintaining a project's stable variable UID
    /// sidecar (ADR 0053). The sidecar stores each persistent declaration's
    /// entity UID next to the project so renames keep variable identity
    /// across hot edits.
    Refactor {
        #[command(subcommand)]
        command: RefactorAction,
    },
    /// Records that a device received a container over a connection: writes
    /// the download record `<application>.<connection>.download.iplcinfo`
    /// beside the application container (ADR 0083). The editor calls this
    /// after the device acknowledged a download, and compares the record with
    /// the content hash the device reports when it connects. Prints nothing
    /// when the record is written.
    RecordDownload {
        /// The application container: the record is named after it and sits
        /// beside it. It is also the container that was sent, unless
        /// `--container` names another file.
        application: PathBuf,

        /// The name of the connection profile that keys the record. It must
        /// not be empty.
        #[arg(long)]
        connection: String,

        /// The container file that was sent to the device, when it is not the
        /// application container itself (for example a container compiled
        /// for the occasion). Its header gives the identity that is recorded.
        #[arg(long)]
        container: Option<PathBuf>,

        /// The content hash the device reports (64 hexadecimal digits). When
        /// given, the record is written only if it is the content hash of the
        /// container, because the record says what the device received.
        #[arg(long, value_name = "HASH", value_parser = parse_content_hash)]
        device_content_hash: Option<[u8; HASH_LEN]>,

        /// Generation of the artifact that was executing, as the device
        /// reported it after the download. Given together with `--normal` and
        /// `--application`.
        #[arg(long, value_name = "N", requires_all = ["normal", "application_generation"])]
        active: Option<u32>,

        /// Generation of the normal (committed) artifact, as the device
        /// reported it after the download.
        #[arg(long, value_name = "N", requires_all = ["active", "application_generation"])]
        normal: Option<u32>,

        /// Generation of the application manifest, as the device reported it
        /// after the download.
        #[arg(
            long = "application",
            value_name = "N",
            requires_all = ["active", "normal"]
        )]
        application_generation: Option<u32>,
    },
    /// Prints the path of the download record of a connection for an
    /// application container, which `record-download` writes. The rule that
    /// names the file is here and nowhere else, so the editor asks instead of
    /// deriving the name. The record need not exist.
    RecordDownloadPath {
        /// The application container the record is named after.
        application: PathBuf,

        /// The name of the connection profile. It must not be empty.
        #[arg(long)]
        connection: String,
    },
    /// Records the controller last chosen for an application: writes the path
    /// record `<application>.path.iplcinfo` beside the application container
    /// (ADR 0083, ADR 0084). It holds the device id, the address that reached
    /// the controller as a route hint (a loopback address is never the hint,
    /// because the file travels with the project to another PC) and the time.
    /// The editor calls this when the engineer chooses a controller. Prints
    /// nothing when the record is written.
    RecordPath {
        /// The application container: the record is named after it and sits
        /// beside it. It need not exist.
        application: PathBuf,

        /// The id of the chosen controller: 32 lowercase hexadecimal digits.
        #[arg(long, value_name = "ID", value_parser = parse_device_id)]
        device_id: String,

        /// The address that reached the controller: an IP address or a host
        /// name. A loopback address (127.0.0.0/8, ::1, localhost) is not stored
        /// as the route hint.
        #[arg(long, value_name = "ADDRESS", value_parser = parse_address)]
        address: String,

        /// The session port of the controller at the address.
        #[arg(long, value_name = "N", value_parser = clap::value_parser!(u16).range(1..))]
        port: u16,
    },
    /// Prints the path of the path record for an application container, which
    /// `record-path` writes. The rule that names the file is here and nowhere
    /// else, so the editor asks instead of deriving the name. The record need
    /// not exist.
    RecordPathPath {
        /// The application container the record is named after.
        application: PathBuf,
    },
    /// Prints the version number of the compiler.
    Version,
}

/// Reads the id of a controller: 32 lowercase hexadecimal digits.
fn parse_device_id(text: &str) -> Result<String, String> {
    path_record::check_device_id(text).map(|()| text.to_owned())
}

/// Reads the address of a route: an IP address or a host name.
fn parse_address(text: &str) -> Result<String, String> {
    path_record::check_address(text).map(|()| text.to_owned())
}

/// Reads the content hash a device reports: 64 hexadecimal digits.
fn parse_content_hash(text: &str) -> Result<[u8; HASH_LEN], String> {
    integrity::hash_from_hex(text)
        .ok_or_else(|| format!("'{text}' is not a hash: expected 64 hexadecimal digits"))
}

#[derive(clap::Subcommand, Debug)]
enum RefactorAction {
    /// Reconcile the stable variable UID sidecar with the project's
    /// declarations: new variables are assigned UIDs, removed variables are
    /// dropped, and rename/swap candidates are reported for the user to
    /// resolve with `refactor map-uid`. When candidates are reported the
    /// sidecar is left unchanged so the resolution stays possible; it is
    /// rewritten only by an unambiguous sync.
    SyncUids {
        #[command(flatten)]
        file_args: FileArgs,
    },
    /// Record an explicit rename or swap resolution by moving the UID of the
    /// old (scope, name) key to the new one. Fails when the old key has no
    /// UID or the new key already has one.
    MapUid {
        /// The project file or directory whose sidecar is updated.
        project: PathBuf,

        /// Scope path of the existing key (the declaring program's name, or
        /// `global` for top-level VAR_GLOBAL declarations).
        old_scope: String,

        /// Name of the existing key.
        old_name: String,

        /// Scope path the UID moves to.
        new_scope: String,

        /// Name the UID moves to.
        new_name: String,
    },
}

pub fn main() -> Result<(), String> {
    // The Err variant is a String so that the command line shows a nice message.
    let args = Args::parse();

    logger::configure(args.verbose, args.log_file)?;

    run(args.action)
}

/// Runs the command, on the stack budget. This is where the program is given
/// the budget: every command, whatever stages it reaches, runs inside the one
/// thread, so the stages that it calls find the budget and make no thread of
/// their own. A command added to [`Action`] gets it with its arm.
fn run(action: Action) -> Result<(), String> {
    within_stack_budget(|| execute(action))
}

fn execute(action: Action) -> Result<(), String> {
    match action {
        Action::Lsp { stdio: _ } => lsp::start(),
        Action::Check {
            file_args,
            libraries,
            format,
        } => cli::check(
            &file_args.files,
            file_args.compiler_options(),
            &libraries,
            format.into(),
        ),
        Action::Compile {
            file_args,
            output,
            libraries,
            force,
        } => cli::compile(
            &file_args.files,
            &output,
            file_args.compiler_options(),
            &libraries,
            force,
            false,
        ),
        Action::Echo { file_args } => {
            cli::echo(&file_args.files, file_args.compiler_options(), false)
        }
        Action::Tokenize { file_args } => {
            cli::tokenize(&file_args.files, file_args.compiler_options(), false)
        }
        Action::Dialects => {
            print!("{}", describe_dialects());
            Ok(())
        }
        Action::Refactor { command } => match command {
            RefactorAction::SyncUids { file_args } => {
                cli::sync_uids(&file_args.files, file_args.compiler_options(), false)
            }
            RefactorAction::MapUid {
                project,
                old_scope,
                old_name,
                new_scope,
                new_name,
            } => cli::map_uid(&project, &old_scope, &old_name, &new_scope, &new_name),
        },
        Action::RecordDownload {
            application,
            connection,
            container,
            device_content_hash,
            active,
            normal,
            application_generation,
        } => cli::record_download(&cli::RecordDownload {
            application: &application,
            container: container.as_deref(),
            connection: &connection,
            device_content_hash,
            device: match (active, normal, application_generation) {
                (Some(active), Some(normal), Some(application)) => Some(DeviceGenerations {
                    active,
                    normal,
                    application,
                }),
                _ => None,
            },
        }),
        Action::RecordDownloadPath {
            application,
            connection,
        } => {
            println!(
                "{}",
                cli::record_download_path(&application, &connection)?.display()
            );
            Ok(())
        }
        Action::RecordPath {
            application,
            device_id,
            address,
            port,
        } => cli::record_path(&cli::RecordPath {
            application: &application,
            device_id: &device_id,
            address: &address,
            session_port: port,
        }),
        Action::RecordPathPath { application } => {
            println!("{}", cli::record_path_path(&application)?.display());
            Ok(())
        }
        Action::Version => {
            println!("ironplcc version {VERSION}");
            Ok(())
        }
    }
}

#[cfg(test)]
mod tests;
