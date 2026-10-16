//! Syntax kinds and the rowan [`Language`] binding for the lossless tree.
//!
//! One enum names every element a green tree can hold: trivia tokens,
//! significant tokens, keywords, and nodes (design: parse-tree architecture,
//! section 3.1). The kinds are declared once in `syntax_kinds!`; the
//! classification predicates, the keyword table and `ALL` are all generated
//! from that single declaration, so adding a kind cannot leave a table behind.
//!
//! Keywords are lexed uniformly and case-insensitively. Whether a keyword is
//! reserved in a dialect is decided by the parser, from the gate each keyword
//! declares on its own row: `ungated`, or `gated` followed by the name of the
//! [`ParseOptions`] flag that makes it a keyword. A keyword row with neither
//! does not compile, and a flag that does not exist does not compile.

use rowan::Language;

use crate::parser::options::ParseOptions;

/// The value of a keyword's declared gate under `options`: `ungated` is
/// always on, `gated flag` is the value of that field of [`ParseOptions`].
macro_rules! keyword_gate {
    ($options:ident, ungated) => {
        true
    };
    ($options:ident, gated $flag:ident) => {
        $options.$flag
    };
}

macro_rules! syntax_kinds {
    (
        trivia { $($trivia:ident),* $(,)? }
        tokens { $($token:ident $(= $symbol:literal)?),* $(,)? }
        keywords { $($keyword:ident = [$first:literal $(, $alias:literal)*] $($gate:ident)+),* $(,)? }
        nodes { $($node:ident),* $(,)? }
    ) => {
        /// Every kind a green tree can contain.
        #[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash)]
        #[repr(u16)]
        pub enum SyntaxKind {
            $($trivia,)*
            $($token,)*
            $($keyword,)*
            $($node,)*
            /// Fallback for raw kinds this language never produces.
            Unknown,
        }

        impl SyntaxKind {
            /// All kinds in discriminant order.
            pub const ALL: &'static [SyntaxKind] = &[
                $(SyntaxKind::$trivia,)*
                $(SyntaxKind::$token,)*
                $(SyntaxKind::$keyword,)*
                $(SyntaxKind::$node,)*
                SyntaxKind::Unknown,
            ];

            /// Every keyword spelling (upper case) with the kind it lexes to.
            pub const KEYWORDS: &'static [(&'static str, SyntaxKind)] = &[
                $(($first, SyntaxKind::$keyword), $(($alias, SyntaxKind::$keyword),)*)*
            ];

            /// Every token that is written as a fixed symbol (`:=`, `;`),
            /// with the symbol, in declaration order. The lexer reads
            /// punctuation from this table, and an error that names a token
            /// takes the name from it, so a symbol is declared once.
            pub const SYMBOLS: &'static [(&'static str, SyntaxKind)] = &[
                $($(($symbol, SyntaxKind::$token),)?)*
            ];

            /// How the kind is written in source when that is always the
            /// same: the symbol of a punctuation token, or the first
            /// spelling of a keyword. `None` for a token whose text varies
            /// (an identifier, a literal), for trivia and for a node.
            pub const fn spelling(self) -> Option<&'static str> {
                match self {
                    $($(SyntaxKind::$token => Some($symbol),)?)*
                    $(SyntaxKind::$keyword => Some($first),)*
                    _ => None,
                }
            }

            /// True for tokens that are retained but never drive parsing.
            pub fn is_trivia(self) -> bool {
                matches!(self, $(SyntaxKind::$trivia)|*)
            }

            /// True when `options` leave this kind available: a keyword
            /// whose declared gate is a flag is available when the flag is
            /// on, and every other kind (an ungated keyword, a punctuation
            /// token, trivia, a node) always is. The gate is read from the
            /// keyword's own row in the declaration, and the match names
            /// every kind, so no arm decides a keyword by default.
            pub fn available_in(self, options: &ParseOptions) -> bool {
                match self {
                    $(SyntaxKind::$keyword => keyword_gate!(options, $($gate)+),)*
                    $(SyntaxKind::$trivia => true,)*
                    $(SyntaxKind::$token => true,)*
                    $(SyntaxKind::$node => true,)*
                    SyntaxKind::Unknown => true,
                }
            }

            /// True for keyword tokens.
            pub fn is_keyword(self) -> bool {
                matches!(self, $(SyntaxKind::$keyword)|*)
            }

            /// True for leaf tokens (as opposed to interior nodes).
            pub fn is_token(self) -> bool {
                !matches!(self, $(SyntaxKind::$node|)* SyntaxKind::Unknown)
            }

            /// The node kind this kind is, or `None` for a token, a keyword,
            /// trivia or `Unknown`.
            pub fn node(self) -> Option<NodeKind> {
                match self {
                    $(SyntaxKind::$node => Some(NodeKind::$node),)*
                    _ => None,
                }
            }

            /// The token kind this kind is, or `None` for a node or `Unknown`.
            pub fn token(self) -> Option<TokenKind> {
                match self {
                    $(SyntaxKind::$trivia => Some(TokenKind::$trivia),)*
                    $(SyntaxKind::$token => Some(TokenKind::$token),)*
                    $(SyntaxKind::$keyword => Some(TokenKind::$keyword),)*
                    _ => None,
                }
            }

            fn keyword_from_upper(upper: &str) -> Option<SyntaxKind> {
                match upper {
                    $($first $(| $alias)* => Some(SyntaxKind::$keyword),)*
                    _ => None,
                }
            }
        }

        /// The node kinds alone, generated from the same declaration.
        ///
        /// A table that has to decide something about every node matches on
        /// this type without a wildcard arm: adding a node kind to the
        /// declaration then fails to compile until the table says what to do
        /// with it.
        #[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash)]
        pub enum NodeKind {
            $($node,)*
        }

        impl NodeKind {
            /// The syntax kind of this node kind.
            pub fn syntax_kind(self) -> SyntaxKind {
                match self {
                    $(NodeKind::$node => SyntaxKind::$node,)*
                }
            }
        }

        /// The leaf kinds alone (trivia, tokens and keywords), generated from
        /// the same declaration.
        ///
        /// A table that has to decide something about every token matches on
        /// this type without a wildcard arm: adding a token kind to the
        /// declaration then fails to compile until the table says what to do
        /// with it.
        #[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash)]
        pub enum TokenKind {
            $($trivia,)*
            $($token,)*
            $($keyword,)*
        }

        impl TokenKind {
            /// The syntax kind of this token kind.
            pub fn syntax_kind(self) -> SyntaxKind {
                match self {
                    $(TokenKind::$trivia => SyntaxKind::$trivia,)*
                    $(TokenKind::$token => SyntaxKind::$token,)*
                    $(TokenKind::$keyword => SyntaxKind::$keyword,)*
                }
            }
        }
    };
}

syntax_kinds! {
    trivia {
        Whitespace,
        Newline,
        LineComment,
        DocComment,
        BlockComment,
        Pragma,
        RangedComment,
        InactiveRegion
    }
    tokens {
        Ident,
        EscapedIdent,
        IntegerLit,
        FixedPointLit,
        FloatingPointLit,
        HexLit,
        OctLit,
        BinLit,
        StringLit,
        WStringLit,
        DirectAddress,
        DirectAddressIncomplete,
        PartialAccess,
        LeftParen = "(",
        RightParen = ")",
        RightBrace = "}",
        LeftBracket = "[",
        RightBracket = "]",
        Comma = ",",
        Semicolon = ";",
        Colon = ":",
        Period = ".",
        Range = "..",
        Hash = "#",
        Equal = "=",
        NotEqual = "<>",
        Less = "<",
        Greater = ">",
        LessEqual = "<=",
        GreaterEqual = ">=",
        Div = "/",
        Star = "*",
        Plus = "+",
        Minus = "-",
        Power = "**",
        Caret = "^",
        Assignment = ":=",
        RightArrow = "=>",
        ErrorToken
    }
    keywords {
        Action = ["ACTION"] ungated,
        EndAction = ["END_ACTION"] ungated,
        Array = ["ARRAY"] ungated,
        Of = ["OF"] ungated,
        Params = ["PARAMS"] gated allow_params_of,
        At = ["AT"] ungated,
        Case = ["CASE"] ungated,
        Else = ["ELSE"] ungated,
        EndCase = ["END_CASE"] ungated,
        Constant = ["CONSTANT"] ungated,
        Configuration = ["CONFIGURATION"] ungated,
        EndConfiguration = ["END_CONFIGURATION"] ungated,
        En = ["EN"] ungated,
        Eno = ["ENO"] ungated,
        Exit = ["EXIT"] ungated,
        Continue = ["CONTINUE"] gated allow_continue,
        Try = ["__TRY"] gated allow_try_catch,
        EndTry = ["__ENDTRY"] gated allow_try_catch,
        Catch = ["__CATCH"] gated allow_try_catch,
        Finally = ["__FINALLY"] gated allow_try_catch,
        Throw = ["__THROW"] gated allow_try_catch,
        Jmp = ["JMP"] gated allow_jump_statement,
        Calc = ["CALC"] gated allow_calc_statement,
        Wait = ["__WAIT"] gated allow_wait_statement,
        False = ["FALSE"] ungated,
        FEdge = ["F_EDGE"] ungated,
        For = ["FOR"] ungated,
        To = ["TO"] ungated,
        By = ["BY"] ungated,
        Do = ["DO"] ungated,
        EndFor = ["END_FOR"] ungated,
        Function = ["FUNCTION"] ungated,
        EndFunction = ["END_FUNCTION"] ungated,
        FunctionBlock = ["FUNCTION_BLOCK"] ungated,
        EndFunctionBlock = ["END_FUNCTION_BLOCK"] ungated,
        Extends = ["EXTENDS"] gated allow_fb_inheritance,
        Implements = ["IMPLEMENTS"] gated allow_fb_inheritance,
        Interface = ["INTERFACE"] gated allow_fb_inheritance,
        EndInterface = ["END_INTERFACE"] gated allow_fb_inheritance,
        Abstract = ["ABSTRACT"] gated allow_fb_inheritance,
        Method = ["METHOD"] gated allow_fb_inheritance,
        EndMethod = ["END_METHOD"] gated allow_fb_inheritance,
        Property = ["PROPERTY"] gated allow_fb_inheritance,
        EndProperty = ["END_PROPERTY"] gated allow_fb_inheritance,
        EndGet = ["END_GET"] gated allow_fb_inheritance,
        EndSet = ["END_SET"] gated allow_fb_inheritance,
        This = ["THIS"] gated allow_fb_inheritance,
        Super = ["SUPER"] gated allow_fb_inheritance,
        If = ["IF"] ungated,
        Then = ["THEN"] ungated,
        Elsif = ["ELSIF"] ungated,
        EndIf = ["END_IF"] ungated,
        InitialStep = ["INITIAL_STEP"] ungated,
        EndStep = ["END_STEP"] ungated,
        Program = ["PROGRAM"] ungated,
        With = ["WITH"] ungated,
        EndProgram = ["END_PROGRAM"] ungated,
        Namespace = ["NAMESPACE"] gated allow_namespace,
        EndNamespace = ["END_NAMESPACE"] gated allow_namespace,
        BeginImplementation = ["__BEGIN_IMPLEMENTATION"] gated allow_begin_implementation,
        REdge = ["R_EDGE"] ungated,
        ReadOnly = ["READ_ONLY"] ungated,
        ReadWrite = ["READ_WRITE"] ungated,
        Repeat = ["REPEAT"] ungated,
        Until = ["UNTIL"] ungated,
        EndRepeat = ["END_REPEAT"] ungated,
        Resource = ["RESOURCE"] ungated,
        On = ["ON"] ungated,
        EndResource = ["END_RESOURCE"] ungated,
        Retain = ["RETAIN"] ungated,
        NonRetain = ["NON_RETAIN"] ungated,
        Persistent = ["PERSISTENT"] gated allow_persistent_var,
        Return = ["RETURN"] ungated,
        Step = ["STEP"] ungated,
        Struct = ["STRUCT"] ungated,
        EndStruct = ["END_STRUCT"] ungated,
        Union = ["UNION"] gated allow_union_type,
        EndUnion = ["END_UNION"] gated allow_union_type,
        Task = ["TASK"] ungated,
        EndTask = ["END_TASK"] ungated,
        Transition = ["TRANSITION"] ungated,
        From = ["FROM"] ungated,
        EndTransition = ["END_TRANSITION"] ungated,
        True = ["TRUE"] ungated,
        Type = ["TYPE"] ungated,
        EndType = ["END_TYPE"] ungated,
        Var = ["VAR"] ungated,
        EndVar = ["END_VAR"] ungated,
        VarInput = ["VAR_INPUT"] ungated,
        VarOutput = ["VAR_OUTPUT"] ungated,
        VarInOut = ["VAR_IN_OUT"] ungated,
        VarTemp = ["VAR_TEMP"] ungated,
        VarExternal = ["VAR_EXTERNAL"] ungated,
        VarAccess = ["VAR_ACCESS"] ungated,
        VarConfig = ["VAR_CONFIG"] ungated,
        VarGlobal = ["VAR_GLOBAL"] ungated,
        VarStat = ["VAR_STAT"] gated allow_var_stat,
        VarInst = ["VAR_INST"] gated allow_var_inst,
        VarGeneric = ["VAR_GENERIC"] gated allow_var_generic,
        While = ["WHILE"] ungated,
        EndWhile = ["END_WHILE"] ungated,
        Bool = ["BOOL"] ungated,
        Bit = ["BIT"] gated allow_bit_type,
        Sint = ["SINT"] ungated,
        Int = ["INT"] ungated,
        Dint = ["DINT"] ungated,
        Lint = ["LINT"] ungated,
        Usint = ["USINT"] ungated,
        Uint = ["UINT"] ungated,
        Udint = ["UDINT"] ungated,
        Ulint = ["ULINT"] ungated,
        Real = ["REAL"] ungated,
        Lreal = ["LREAL"] ungated,
        Time = ["TIME"] ungated,
        Ltime = ["LTIME"] gated allow_long_time_types,
        RefTo = ["REF_TO"] gated allow_ref_to,
        Ref = ["REF"] gated allow_ref_to,
        Null = ["NULL"] gated allow_ref_to,
        Reference = ["REFERENCE"] gated allow_reference_to,
        Pointer = ["POINTER"] gated allow_pointer_to,
        Date = ["DATE"] ungated,
        TimeOfDay = ["TIME_OF_DAY", "TOD"] ungated,
        DateAndTime = ["DATE_AND_TIME", "DT"] ungated,
        Ldate = ["LDATE"] gated allow_long_time_types,
        Ltod = ["LTIME_OF_DAY", "LTOD"] gated allow_long_time_types,
        Ldt = ["LDATE_AND_TIME", "LDT"] gated allow_long_time_types,
        String = ["STRING"] ungated,
        Byte = ["BYTE"] ungated,
        Word = ["WORD"] ungated,
        Dword = ["DWORD"] ungated,
        Lword = ["LWORD"] ungated,
        WString = ["WSTRING"] ungated,
        Any = ["ANY"] ungated,
        AnyDerived = ["ANY_DERIVED"] ungated,
        AnyElementary = ["ANY_ELEMENTARY"] ungated,
        AnyMagnitude = ["ANY_MAGNITUDE"] ungated,
        AnyNum = ["ANY_NUM"] ungated,
        AnyReal = ["ANY_REAL"] ungated,
        AnyInt = ["ANY_INT"] ungated,
        AnyBit = ["ANY_BIT"] ungated,
        AnyString = ["ANY_STRING"] ungated,
        AnyDate = ["ANY_DATE"] ungated,
        Xor = ["XOR"] ungated,
        AndThen = ["AND_THEN"] gated allow_short_circuit_operators,
        OrElse = ["OR_ELSE"] gated allow_short_circuit_operators,
        Or = ["OR"] ungated,
        And = ["AND"] ungated,
        Mod = ["MOD"] ungated,
        Not = ["NOT"] ungated,
    }
    nodes {
        SourceFile,
        ErrorNode,
        ProgramDecl,
        FunctionDecl,
        FunctionBlockDecl,
        InterfaceDecl,
        NamespaceDecl,
        MethodDecl,
        PropertyDecl,
        GetAccessor,
        SetAccessor,
        MemberQualifier,
        ExtendsClause,
        ImplementsClause,
        Name,
        VarBlock,
        VarDecl,
        Location,
        EdgeSpec,
        AccessDecl,
        InstanceInit,
        TypeBlock,
        TypeDecl,
        ArrayType,
        Subrange,
        StringType,
        RefType,
        ParamsType,
        SubrangeType,
        EnumType,
        EnumValue,
        StructType,
        UnionType,
        StructMember,
        Initializer,
        ArrayInit,
        RepeatedInit,
        StructInit,
        StructInitElement,
        EnumValueRef,
        ConfigurationDecl,
        ResourceDecl,
        TaskDecl,
        TaskInit,
        TaskInitItem,
        ProgramConfig,
        TaskBinding,
        ProgramConnection,
        SfcBody,
        InitialStepDecl,
        StepDecl,
        ActionAssociation,
        ActionQualifier,
        ActionDecl,
        TransitionDecl,
        TransitionPriority,
        StepList,
        TransitionCondition,
        StatementList,
        EmptyStmt,
        AssignStmt,
        AssignOp,
        CallStmt,
        IfStmt,
        ElsifClause,
        ElseClause,
        CaseStmt,
        CaseBranch,
        CaseLabel,
        ForStmt,
        WhileStmt,
        RepeatStmt,
        ExitStmt,
        ContinueStmt,
        ReturnStmt,
        TryStmt,
        CatchClause,
        FinallyClause,
        ThrowStmt,
        JmpStmt,
        LabelStmt,
        CalcStmt,
        WaitStmt,
        ImplementationMarker,
        BinaryExpr,
        UnaryExpr,
        ParenExpr,
        NameRef,
        FieldExpr,
        IndexExpr,
        BitAccessExpr,
        PartialAccessExpr,
        DerefExpr,
        SelfRefExpr,
        DirectAddressExpr,
        CallExpr,
        ArgList,
        PositionalArg,
        NamedArg,
        OutputArg,
        RefExpr,
        SpecialOpExpr,
        TypeRef,
        NullLiteral,
        BoolLiteral,
        IntLiteral,
        RealLiteral,
        BitStringLiteral,
        StringLiteral,
        DurationLiteral,
        TimeOfDayLiteral,
        DateLiteral,
        DateTimeLiteral
    }
}

/// Longest keyword spelling, in bytes (`__BEGIN_IMPLEMENTATION` is 22).
const MAX_KEYWORD_LEN: usize = 24;

impl SyntaxKind {
    /// The keyword kind spelled by `text`, ignoring ASCII case.
    pub fn keyword_from_str(text: &str) -> Option<SyntaxKind> {
        if text.len() > MAX_KEYWORD_LEN || !text.is_ascii() {
            return None;
        }
        let mut buffer = [0u8; MAX_KEYWORD_LEN];
        let upper = buffer.get_mut(..text.len())?;
        upper.copy_from_slice(text.as_bytes());
        upper.make_ascii_uppercase();
        SyntaxKind::keyword_from_upper(core::str::from_utf8(upper).ok()?)
    }

    fn from_u16(value: u16) -> SyntaxKind {
        SyntaxKind::ALL
            .get(usize::from(value))
            .copied()
            .unwrap_or(SyntaxKind::Unknown)
    }
}

/// The rowan language marker for structured text.
#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash)]
pub enum StLanguage {}

impl Language for StLanguage {
    type Kind = SyntaxKind;

    fn kind_from_raw(raw: rowan::SyntaxKind) -> SyntaxKind {
        SyntaxKind::from_u16(raw.0)
    }

    fn kind_to_raw(kind: SyntaxKind) -> rowan::SyntaxKind {
        rowan::SyntaxKind(kind as u16)
    }
}

/// Red (navigable) node over the green storage.
pub type SyntaxNode = rowan::SyntaxNode<StLanguage>;
/// Red token over the green storage.
pub type SyntaxToken = rowan::SyntaxToken<StLanguage>;
/// Node-or-token element.
pub type SyntaxElement = rowan::SyntaxElement<StLanguage>;

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn all_kinds_when_indexed_then_match_discriminants() {
        for (index, kind) in SyntaxKind::ALL.iter().enumerate() {
            assert_eq!(*kind as usize, index, "ALL out of order at {index}");
        }
    }

    #[test]
    fn kind_roundtrip_when_converted_then_identity() {
        for kind in SyntaxKind::ALL {
            let raw = StLanguage::kind_to_raw(*kind);
            assert_eq!(StLanguage::kind_from_raw(raw), *kind);
        }
    }

    #[test]
    fn unknown_kind_when_out_of_range_then_fallback() {
        assert_eq!(
            StLanguage::kind_from_raw(rowan::SyntaxKind(u16::MAX)),
            SyntaxKind::Unknown
        );
    }

    #[test]
    fn classification_when_kinds_sampled_then_trivia_keyword_token_are_distinct() {
        assert!(SyntaxKind::Pragma.is_trivia());
        assert!(!SyntaxKind::Ident.is_trivia());
        assert!(SyntaxKind::EndProgram.is_keyword());
        assert!(!SyntaxKind::Ident.is_keyword());
        assert!(SyntaxKind::Ident.is_token());
        assert!(!SyntaxKind::SourceFile.is_token());
        assert!(!SyntaxKind::Unknown.is_token());
    }

    #[test]
    fn node_when_kind_is_a_node_then_node_kind_and_back_and_otherwise_none() {
        for kind in SyntaxKind::ALL {
            match kind.node() {
                Some(node) => {
                    assert!(!kind.is_token(), "{kind:?}");
                    assert_eq!(node.syntax_kind(), *kind);
                }
                None => assert!(kind.is_token() || *kind == SyntaxKind::Unknown, "{kind:?}"),
            }
        }
        assert_eq!(SyntaxKind::Ident.node(), None);
        assert_eq!(SyntaxKind::SourceFile.node(), Some(NodeKind::SourceFile));
    }

    #[test]
    fn token_when_kind_is_a_leaf_then_token_kind_and_back_and_otherwise_none() {
        for kind in SyntaxKind::ALL {
            match kind.token() {
                Some(token) => {
                    assert!(kind.is_token(), "{kind:?}");
                    assert_eq!(token.syntax_kind(), *kind);
                }
                None => assert!(!kind.is_token(), "{kind:?}"),
            }
        }
        assert_eq!(SyntaxKind::Ident.token(), Some(TokenKind::Ident));
        assert_eq!(SyntaxKind::SourceFile.token(), None);
    }

    #[test]
    fn keyword_from_str_when_mixed_case_then_keyword() {
        assert_eq!(
            SyntaxKind::keyword_from_str("End_Program"),
            Some(SyntaxKind::EndProgram)
        );
        assert_eq!(
            SyntaxKind::keyword_from_str("tod"),
            Some(SyntaxKind::TimeOfDay)
        );
    }

    #[test]
    fn keyword_from_str_when_not_keyword_then_none() {
        assert_eq!(SyntaxKind::keyword_from_str("speed"), None);
        assert_eq!(SyntaxKind::keyword_from_str(""), None);
        assert_eq!(SyntaxKind::keyword_from_str("PROGRAMS"), None);
        assert_eq!(SyntaxKind::keyword_from_str("ÄND"), None);
        assert_eq!(
            SyntaxKind::keyword_from_str("A_VERY_LONG_IDENTIFIER_THAT_EXCEEDS_ANY_KEYWORD"),
            None
        );
    }

    #[test]
    fn keywords_when_listed_then_spellings_are_upper_case_and_round_trip() {
        for (spelling, kind) in SyntaxKind::KEYWORDS {
            assert_eq!(*spelling, spelling.to_ascii_uppercase());
            assert!(spelling.len() <= MAX_KEYWORD_LEN);
            assert_eq!(SyntaxKind::keyword_from_str(spelling), Some(*kind));
        }
    }

    #[test]
    fn spelling_when_kind_is_a_keyword_then_its_first_spelling() {
        for (_, kind) in SyntaxKind::KEYWORDS {
            let first = SyntaxKind::KEYWORDS
                .iter()
                .find(|(_, other)| other == kind)
                .map(|(first, _)| *first);
            assert_eq!(kind.spelling(), first, "{kind:?}");
        }
        assert_eq!(SyntaxKind::TimeOfDay.spelling(), Some("TIME_OF_DAY"));
        assert_eq!(SyntaxKind::EndVar.spelling(), Some("END_VAR"));
    }

    #[test]
    fn spelling_when_kind_is_a_symbol_then_the_symbol() {
        for (symbol, kind) in SyntaxKind::SYMBOLS {
            assert_eq!(kind.spelling(), Some(*symbol), "{kind:?}");
        }
        assert_eq!(SyntaxKind::Assignment.spelling(), Some(":="));
        assert_eq!(SyntaxKind::Semicolon.spelling(), Some(";"));
    }

    #[test]
    fn spelling_when_kind_is_neither_keyword_nor_symbol_then_none() {
        for kind in SyntaxKind::ALL {
            let spelled = SyntaxKind::KEYWORDS.iter().any(|(_, other)| other == kind)
                || SyntaxKind::SYMBOLS.iter().any(|(_, other)| other == kind);
            assert_eq!(kind.spelling().is_some(), spelled, "{kind:?}");
        }
        assert_eq!(SyntaxKind::Ident.spelling(), None);
        assert_eq!(SyntaxKind::SourceFile.spelling(), None);
    }

    #[test]
    fn symbols_when_listed_then_no_symbol_is_empty_repeated_or_a_keyword() {
        for (index, (symbol, kind)) in SyntaxKind::SYMBOLS.iter().enumerate() {
            assert!(!symbol.is_empty(), "{kind:?}");
            assert!(
                SyntaxKind::SYMBOLS[..index]
                    .iter()
                    .all(|(seen, _)| seen != symbol),
                "{symbol}"
            );
            assert_eq!(SyntaxKind::keyword_from_str(symbol), None, "{symbol}");
        }
    }
}
