//! Syntax kinds and the rowan [`Language`] binding for the lossless tree.
//!
//! One enum names every element a green tree can hold: trivia tokens,
//! significant tokens, keywords, and nodes (design: parse-tree architecture,
//! section 3.1). The kinds are declared once in `syntax_kinds!`; the
//! classification predicates, the keyword table and `ALL` are all generated
//! from that single declaration, so adding a kind cannot leave a table behind.
//!
//! Keywords are lexed uniformly and case-insensitively. Whether a keyword is
//! reserved in a dialect is decided by the parser, not here.

use rowan::Language;

macro_rules! syntax_kinds {
    (
        trivia { $($trivia:ident),* $(,)? }
        tokens { $($token:ident),* $(,)? }
        keywords { $($keyword:ident = [$($spelling:literal),+]),* $(,)? }
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
                $($(($spelling, SyntaxKind::$keyword),)+)*
            ];

            /// True for tokens that are retained but never drive parsing.
            pub fn is_trivia(self) -> bool {
                matches!(self, $(SyntaxKind::$trivia)|*)
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

            fn keyword_from_upper(upper: &str) -> Option<SyntaxKind> {
                match upper {
                    $($($spelling)|+ => Some(SyntaxKind::$keyword),)*
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
        LeftParen,
        RightParen,
        RightBrace,
        LeftBracket,
        RightBracket,
        Comma,
        Semicolon,
        Colon,
        Period,
        Range,
        Hash,
        Equal,
        NotEqual,
        Less,
        Greater,
        LessEqual,
        GreaterEqual,
        Div,
        Star,
        Plus,
        Minus,
        Power,
        Caret,
        Assignment,
        RightArrow,
        ErrorToken
    }
    keywords {
        Action = ["ACTION"],
        EndAction = ["END_ACTION"],
        Array = ["ARRAY"],
        Of = ["OF"],
        Params = ["PARAMS"],
        At = ["AT"],
        Case = ["CASE"],
        Else = ["ELSE"],
        EndCase = ["END_CASE"],
        Constant = ["CONSTANT"],
        Configuration = ["CONFIGURATION"],
        EndConfiguration = ["END_CONFIGURATION"],
        En = ["EN"],
        Eno = ["ENO"],
        Exit = ["EXIT"],
        Continue = ["CONTINUE"],
        Try = ["__TRY"],
        EndTry = ["__ENDTRY"],
        Catch = ["__CATCH"],
        Finally = ["__FINALLY"],
        Throw = ["__THROW"],
        Jmp = ["JMP"],
        Calc = ["CALC"],
        Wait = ["__WAIT"],
        False = ["FALSE"],
        FEdge = ["F_EDGE"],
        For = ["FOR"],
        To = ["TO"],
        By = ["BY"],
        Do = ["DO"],
        EndFor = ["END_FOR"],
        Function = ["FUNCTION"],
        EndFunction = ["END_FUNCTION"],
        FunctionBlock = ["FUNCTION_BLOCK"],
        EndFunctionBlock = ["END_FUNCTION_BLOCK"],
        Extends = ["EXTENDS"],
        Implements = ["IMPLEMENTS"],
        Interface = ["INTERFACE"],
        EndInterface = ["END_INTERFACE"],
        Abstract = ["ABSTRACT"],
        Method = ["METHOD"],
        EndMethod = ["END_METHOD"],
        Property = ["PROPERTY"],
        EndProperty = ["END_PROPERTY"],
        EndGet = ["END_GET"],
        EndSet = ["END_SET"],
        This = ["THIS"],
        Super = ["SUPER"],
        If = ["IF"],
        Then = ["THEN"],
        Elsif = ["ELSIF"],
        EndIf = ["END_IF"],
        InitialStep = ["INITIAL_STEP"],
        EndStep = ["END_STEP"],
        Program = ["PROGRAM"],
        With = ["WITH"],
        EndProgram = ["END_PROGRAM"],
        Namespace = ["NAMESPACE"],
        EndNamespace = ["END_NAMESPACE"],
        BeginImplementation = ["__BEGIN_IMPLEMENTATION"],
        REdge = ["R_EDGE"],
        ReadOnly = ["READ_ONLY"],
        ReadWrite = ["READ_WRITE"],
        Repeat = ["REPEAT"],
        Until = ["UNTIL"],
        EndRepeat = ["END_REPEAT"],
        Resource = ["RESOURCE"],
        On = ["ON"],
        EndResource = ["END_RESOURCE"],
        Retain = ["RETAIN"],
        NonRetain = ["NON_RETAIN"],
        Persistent = ["PERSISTENT"],
        Return = ["RETURN"],
        Step = ["STEP"],
        Struct = ["STRUCT"],
        EndStruct = ["END_STRUCT"],
        Union = ["UNION"],
        EndUnion = ["END_UNION"],
        Task = ["TASK"],
        EndTask = ["END_TASK"],
        Transition = ["TRANSITION"],
        From = ["FROM"],
        EndTransition = ["END_TRANSITION"],
        True = ["TRUE"],
        Type = ["TYPE"],
        EndType = ["END_TYPE"],
        Var = ["VAR"],
        EndVar = ["END_VAR"],
        VarInput = ["VAR_INPUT"],
        VarOutput = ["VAR_OUTPUT"],
        VarInOut = ["VAR_IN_OUT"],
        VarTemp = ["VAR_TEMP"],
        VarExternal = ["VAR_EXTERNAL"],
        VarAccess = ["VAR_ACCESS"],
        VarConfig = ["VAR_CONFIG"],
        VarGlobal = ["VAR_GLOBAL"],
        VarStat = ["VAR_STAT"],
        VarInst = ["VAR_INST"],
        VarGeneric = ["VAR_GENERIC"],
        While = ["WHILE"],
        EndWhile = ["END_WHILE"],
        Bool = ["BOOL"],
        Bit = ["BIT"],
        Sint = ["SINT"],
        Int = ["INT"],
        Dint = ["DINT"],
        Lint = ["LINT"],
        Usint = ["USINT"],
        Uint = ["UINT"],
        Udint = ["UDINT"],
        Ulint = ["ULINT"],
        Real = ["REAL"],
        Lreal = ["LREAL"],
        Time = ["TIME"],
        Ltime = ["LTIME"],
        RefTo = ["REF_TO"],
        Ref = ["REF"],
        Null = ["NULL"],
        Reference = ["REFERENCE"],
        Pointer = ["POINTER"],
        Date = ["DATE"],
        TimeOfDay = ["TIME_OF_DAY", "TOD"],
        DateAndTime = ["DATE_AND_TIME", "DT"],
        Ldate = ["LDATE"],
        Ltod = ["LTIME_OF_DAY", "LTOD"],
        Ldt = ["LDATE_AND_TIME", "LDT"],
        String = ["STRING"],
        Byte = ["BYTE"],
        Word = ["WORD"],
        Dword = ["DWORD"],
        Lword = ["LWORD"],
        WString = ["WSTRING"],
        Any = ["ANY"],
        AnyDerived = ["ANY_DERIVED"],
        AnyElementary = ["ANY_ELEMENTARY"],
        AnyMagnitude = ["ANY_MAGNITUDE"],
        AnyNum = ["ANY_NUM"],
        AnyReal = ["ANY_REAL"],
        AnyInt = ["ANY_INT"],
        AnyBit = ["ANY_BIT"],
        AnyString = ["ANY_STRING"],
        AnyDate = ["ANY_DATE"],
        Xor = ["XOR"],
        AndThen = ["AND_THEN"],
        OrElse = ["OR_ELSE"],
        Or = ["OR"],
        And = ["AND"],
        Mod = ["MOD"],
        Not = ["NOT"],
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
}
