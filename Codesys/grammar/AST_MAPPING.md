# AST_MAPPING — привязка правил `ST_GRAMMAR.ebnf` к парсеру и узлу AST

Зона: только этот файл. Источники: `grammar/ST_GRAMMAR.ebnf`,
`Parser35220.plugin/CODESYS/Parser35220/*`, `tables/ast_builder_map.csv`.
Пути к коду — относительно `C:\Codesys\Parser35220.plugin\CODESYS\Parser35220\`.

Колонки: `EBNF-правило` → `парсер (file:line)` → `builder` → `узел AST`.

---

## 0. Верхний уровень

| EBNF | Парсер | Builder | Узел AST |
|---|---|---|---|
| `compilationUnit` | `InternalParser.ParseST:177` / `ParseRawST:353` | `CreateSequenceStatement` | `_ISequenceStatement` |
| `topElement` (pragma/comment/pouUnit/varSection/typeDecl/`;`) | `InternalParser.ParseST:183` | `CreateSequenceStatement.Add` | `_ISequenceStatement` (элементы) |

## 1. Лексика

| EBNF | Парсер | Builder | Узел AST |
|---|---|---|---|
| `identifier` (`iecIdentifier`/`escapedIdentifier`) | `InternalScanner.ScanIdentifierOrOperator` (`InternalScanner.cs:2319` `ScanWeirdIdentifier`) | — | `Identifier(13)` |
| `booleanLiteral` | `InternalScanner.ScanForTrueFalseOrIdentifier:2435` | `CreateLiteralExpression` | `_ILiteralExpression` (TypeClass 0) |
| `integerLiteral` (dec/based) | `InternalScanner.ScanInteger:2478`, `ScanBasedInteger:2519` | `CreateLiteralExpression` | `_ILiteralExpression` (33) |
| `realLiteral` (+`OptionalExponent`) | `InternalScanner.OptionalExponent:2560` | `CreateLiteralExpression` | `_ILiteralExpression` (35) |
| typed integer/real/boolean | `InternalScanner.ScanTypedIntegerLiteral/…Real/…` | `CreateLiteralExpression` | `_ILiteralExpression` |
| `singleByteString`/`doubleByteString`/`xByteString`/`unicodeString` | `InternalScanner.ValidateStringToken:3298`, `ScanUnicodeLiteral:2624` | `CreateLiteralExpression` (`FactoryExtension.cs:43`) | `_ILiteralExpression` (16/17/42/12) |
| `durationLiteral`/`dateLiteral`/`timeOfDayLiteral`/`dateAndTimeLiteral` (+L-варианты) | `InternalScanner.ScanTimeLiteral:3001`, `ScanTypedDateLiteral:2846`, `ScanTypedDateAndTimeLiteral:2933`, `ScanTypedTimeOfDayLiteral:2784` | `CreateLiteralExpression` | `_ILiteralExpression` (18-21/46-48) |
| `directVariable` | `InternalScanner.ScanDirectVariable:3817` (`ScanLocationPrefix:3912`, `ScanSizePrefix:3855`) | `CreateAddressExpression`+`CreateDirectVariable` | `_IAddressExpression` / `IDirectVariable` |
| `partialAccess` | `InternalScanner.ScanPartialAccess:3934` | `CreatePartialAccessExpression` | `_IPartialAccessExpression` |
| `pragma` (лексика) | `InternalScanner.ScanPragma:2080`, `ReadScannerPragma` | `CreatePragmaStatement(2)` | `_IPragmaStatement` |
| `comment` (block/line/doc) | `InternalScanner.ScanComment:2230`, `ScanSingleLineComment`, `GetDocComment:1668` | `CreateCommentStatement` | `_ICommentStatement` |

## 2. Объявления POU

| EBNF | Парсер | Builder | Узел AST |
|---|---|---|---|
| `pouUnit` | `Declaration/POUSyntaxParser.cs:56 NextTopLevelPOUSyntax` | `CreateSequenceStatement` | `IPOUSyntax` |
| `accessModifier` | `Declaration/POUDeclarationParser.cs:359 ScanAccessSpecifier2`, `Declaration/TypeDeclarationParser.cs:231 ReadAccessSpecifiers` | `IPouDeclarationBuilderAccess.Access` | `SignatureFlag` на POU/типе |
| `program`/`functionBlock`/`function`/`method`/`property`/`interface`/`action`/`transition`/`namespace` | `Declaration/POUDeclarationParser.cs:178 ParsePOUDeclarationInternal` | `CreatePOUDeclarationStatement`/`CreateMethodDeclarationStatement` → `IPouDeclarationBuilder.AsProgram()/AsFunction()/AsFunctionBlock()/AsInterface()/AsMethod()/AsAction()` | `_IPOUDeclarationStatement` (Class: 93/88/87/118/119/284/285/287/290/60) |
| `pouDeclaration` | `Statements/StatementParser.cs:968 ParsePOUDeclaration` | то же | `_IPOUDeclarationStatement` |
| `body` | `InternalParser.cs:187 NextStatement` → `ParseSTStatement` | `CreateSequenceStatement` | `_ISequenceStatement` |
| `returnType` | `Declaration/TypeParser.cs:167 ParseType` | `CreateXType` | `_IType` |
| END-операторы (`GetExpectedEndOfOperator`) | `Declaration/POUSyntaxParser.cs:361`, `IsStartEndOperatorMatch` `StatementParser.cs:421` | — | завершение `IPOUSyntax` |

## 3. Секции переменных

| EBNF | Парсер | Builder | Узел AST |
|---|---|---|---|
| `varSection` | `Statements/StatementParser.cs:740` (IL_2EA) → `Declaration/VariableListParser.cs:87` | `CreateVariableDeclarationListStatement` (`IVariableDeclarationListBuilder`) | `_IVariableDeclarationListStatement` |
| `varKeyword` (`VAR`/`VAR_INPUT`/…) | `VariableListParser.SetVariableFlagByOperator:183` (op 105/107-114/244/280) | `Flags(VarFlag)` | `Flags` на списке |
| `varModifier` (`CONSTANT`/`RETAIN`/…) | `VariableListParser.ScanForAdditionalFlags:124` (66/97/91/94/95) | `Flags(VarFlag)` | `Flags` на списке |
| `variableDeclaration` (`a,b : T := v`) | `Declaration/VariableDeclarationParser.cs:149 ParseVariableDeclaration` | `CreateVariableDeclarationStatement` (`IVariableDeclarationBuilderAddDecls.Declaration`) | `_IVariableDeclarationStatement` |
| `varDeclItem` (`AT %IW..`) | `VariableDeclarationParser.cs:157/230 ParseAddressReturnError` (op 63) | `CreateDirectVariable`/`CreateIncompleteDirectVariable` | `Address` на `_IVariableDeclarationStatement` |
| `identifierList` | `VariableDeclarationParser.cs:265 ParseNamesReturnError` | `CreateVariableExpression`+`AddName` | имена декларации |
| `configDeclaration` (VAR_CONFIG) | `VariableDeclarationParser.cs:272` (`AllowPaths`) | `CreateVariableExpression`/`ParseQualifiedNameExpression` | `_IVariableDeclarationStatement` |
| `initializer` (init/array/struct/enum/pragma) | `Expressions/InitializationParser.cs:70 ParseInitialisation` | `CreateStructureInitialisation`/`CreateArrayInitialisation`/`CreateAssignmentExpression` | `_IStructureInitialization`/`_IArrayInitialization` |

## 4. Объявления типов

| EBNF | Парсер | Builder | Узел AST |
|---|---|---|---|
| `typeDeclaration` (`TYPE..END_TYPE`) | `Declaration/TypeDeclarationParser.cs:116 ParseTypeDeclaration` | `CreateTypeDeclarationStatement` (`ITypeDeclarationStatementBuilder`) | `_ITypeDeclarationStatement` |
| `typeDefItem` | `TypeDeclarationParser.cs:116` + `Declaration/TypeParser.cs:167` | `Alias()`/`StructOrUnion()`/`Enum()`/`InitialValue()`/`Flags()` | `_ITypeDeclarationStatement` |
| `dataType` / `elementaryType` / `safetyType` | `Declaration/TypeParser.cs:191 HandleOperatorCase` (hashSet, `TypeTable.Get`) | `_ILanguageModelBuilder6` (тип из `ITypeTable3`) | `_IType` |
| `derivedType` — диспетчер | `Declaration/TypeParser.cs:191 HandleOperatorCase` (op 26/27/61/62/92/167/186/192/243/251/257/…) | `CreateXType` | `_IType` |
| `userdefType` | `TypeParser.cs:393 ParseUserdefType` | `CreateUserdefType` | `_IUserdefType` |
| `qualifiedName` (`__SYSTEM`/`__POOL`) | `TypeParser.cs:341`, `Expressions/OperandParser.cs:323 ParseQualifiedNameExpression` | `CreateSystemScopeExpression`/`CreatePoolScopeExpression` + `CreateUserdefType` | `_IType` |
| `subrangeType` (`a..b`) | `TypeParser.cs:123 ParseSubrangeType` | `CreateSubrangeType` | `_ISubrangeType` |
| `stringType` | `TypeParser.cs:540 ParseStringType` | `CreateStringType` | `_IStringType` |
| `wstringType` | `TypeParser.cs:510 ParseWStringType` | `CreateWStringType` | `_IWStringType` |
| `xstringType` | `TypeParser.cs:480 ParseXStringType` | `CreateXStringtype` | `_IXStringType` |
| `arrayType` / `indexRange` | `TypeParser.cs:572 ParseArrayType` | `CreateArrayType`/`CreateVariableLengthArrayType`+`AddDimension` | `_IArrayType`/`_IVariableLengthArrayType` |
| `pointerType` | `TypeParser.cs:675 ParsePointerType` | `CreatePointerType` | `_IPointerType` |
| `referenceType` | `TypeParser.cs:650 ParseReferenceType` | `CreateReferenceType` | `_IReferenceType` |
| `enumType` / `enumMember` | `Declaration/EnumListParser.cs:109 ParseEnumList` | `CreateEnumDeclarationListStatement` (`IEnumDeclarationListBuilder.Enumeration`) | `_IEnumDeclarationListStatement` |
| `paramsType` | `TypeParser.cs:297 HandleOperatorCase(62)` | `CreateParamsType` | params-тип |
| `vectorType` | `TypeParser.cs:450 ParseVectorType` | `CreateVectorType` | `_IType` |
| `genericType` / `typeArg` | `TypeParser.cs:408 ParseGenericUserdefType` | `CreateGenericUserdefType`+`AddGenericConstantInitialization` | `IGenericUserdefType` |
| `anyType` | `TypeParser.cs:193` (hashSet `Any*`) | тип из `TypeTable` | `_IType` |
| `structuredType` / `unionType` / `structMember` | `VariableListParser.cs:87` (op 99/100 `STRUCT`/`UNION`) | `CreateVariableDeclarationListStatement` | `_IVariableDeclarationListStatement` |

## 5. Инструкции

| EBNF | Парсер | Builder | Узел AST |
|---|---|---|---|
| `statement` (диспетчер) | `Statements/StatementParser.cs:216 ParseSTStatement` → `:242 ParseSTStatementHelp` | `switch(TokenType)` | `_IStatement` |
| `assignmentOrCall` | `StatementParser.cs:278` / `TryParseDeclarationOrLabel:908` | `CreateExpressionStatement` | `_IExpressionStatement` |
| `assignment` / `assignmentOperator` | `Expressions/ExpressionParser.cs:114 ParseAssignment` (`:=`164, `S=`165, `R=`166, `REF=`185, `=:`189) | `CreateAssignmentExpression` | `_IAssignmentExpression` |
| `ifStatement` | `Statements/IfStatementParser.cs:106 ParseIf` | `IIfBuilder.Condition()->Then()->ElseIfs()/Else()` | `_IIfStatement` |
| `caseStatement` / `caseLabel` / `caseLabelRange` | `Statements/CaseStatementParser.cs:146` / `:367` / `:400` | `ICaseBuilder.Switch()->Case()->Else()`; `CreateCaseLabelStatement`/`CreateCaseRangeExpression` | `_ICaseStatement`/`_ICaseLabelStatement`/`_ICaseRangeExpression` |
| `forStatement` | `Statements/ForStatementParser.cs:113 ParseFor` | `IForBuilder.Counter()->Range()->By()/ByOne()->Controlled()` | `_IForStatement` |
| `whileStatement` | `Statements/WhileStatementParser.cs:106 ParseWhile` | `IWhileBuilder.Condition()->Controlled()` | `_IWhileStatement` |
| `repeatStatement` | `Statements/RepeatStatementParser.cs:107 ParseRepeat` | `CreateRepeatStatement` | `_IRepeatStatement` |
| `jumpStatement` | `Statements/JumpStatementParser.cs:94 ParseJump` (op 143) | `CreateJumpStatement` | `_IJumpStatement` |
| `returnStatement` | `Statements/ReturnStatementParser.cs:71 ParseReturn` (op 98) | `CreateReturnStatement` | `_IReturnStatement` |
| `exitStatement` | `StatementParser.cs:547` (op 83) | `CreateExitStatement` | `_IExitStatement` |
| `continueStatement` | `StatementParser.cs:549` (op 84) | `CreateContinueStatement` | `_IContinueStatement` |
| `conditionalCall` | `Statements/ConditionalCallParser.cs:94` (op 141 `CALC`) | `CreateNullExpression`+`CreateExpressionStatement` | `_IExpressionStatement`+`_ICallExpression._Condition` |
| `waitStatement` | `StatementParser.cs:852 ParseWaitStatement` (op 205 `__WAIT`) | `CreateWhileStatement`+`CreateCallExpression("SynchWait")` | `_IWhileStatement` |
| `implementationBlock` | `Statements/ImplementationBlockParser.cs:31` (op 286) | `CreateEmbeddedLanguageStatement` | `_IEmbeddedLanguageStatement` |
| `tryCatchStatement` | `Statements/TryCatchStatementParser.cs:93` (238-241) | `CreateTryCatchStatement` | `_ITryCatchStatement` |
| `throwStatement` | `StatementParser.cs:675` область (op 242) | `CreateErrorStatement`/call | — |
| `emptyStatement` | `StatementParser.cs:626/772` (op 172) | `CreateEmptyStatement` | `_IEmptyStatement` |
| `label` / `label:` | `StatementParser.cs:929` | `CreateLabelStatement` | `_ILabelStatement` |
| `pragmaStatement` | `Pragmas/PragmaStatementParser.cs:114` | `CreatePragmaStatement(2)` | `_IPragmaStatement` |
| `pragmaIf` / `pragmaElsif` / `pragmaElse` | `Pragmas/PragmaIfStatementParser.cs:21` | `CreatePragmaIfStatement` | `_IPragmaIfStatement` |
| `pragmaCondition` / `pragmaOperand` | `Pragmas/PragmaOperandParser.cs:20`, `DefinedPragmaOperandParser.cs:19/26` | `CreateOperatorExpression`/`CreateDefinedExpression` | `_IOperatorExpression`/`IDefinedExpression` |
| `comment` | `StatementParser.cs:254/261` | `CreateCommentStatement` | `_ICommentStatement` |

## 6. Выражения

| EBNF | Парсер | Builder | Узел AST |
|---|---|---|---|
| `expression` | `Expressions/ExpressionParser.cs:114 ParseAssignment` | `CreateAssignmentExpression` | `_IExpression` |
| `assignmentExpr` (правоассоц.) | `ExpressionParser.cs:123` (рекурсия) | `CreateAssignmentExpression` | `_IAssignmentExpression` |
| `orExpr` (`OR`/`OR_ELSE`/`XOR`: 129/235/131) | `Expressions/InfixOperationParser.cs:192 ParseORExp` | `CreateOperatorExpression`+`AddOperandHelp` | `_IOperatorExpression` |
| `andExpr` (127/234) | `InfixOperationParser.cs:156 ParseANDExp` | то же | `_IOperatorExpression` |
| `compareExpr` (179/180/175/177/176/178) | `InfixOperationParser.cs:126 ParseCompareExp` | то же | `_IOperatorExpression` |
| `addExpr` (157/158, `__VCADD/__VCSUB`) | `InfixOperationParser.cs:98 ParseADDExp` | то же | `_IOperatorExpression` |
| `mulExpr` (159/161/126, `__VCMUL/__VCDIV/__VCDOT`) | `InfixOperationParser.cs:70 ParseMULExp` | то же | `_IOperatorExpression` |
| `unaryExpr` / `unaryOperator` (`NOT`133, `+`/`-`157/158) | `ExpressionParser.cs:164 ParseSTPrefixOperator` | `CreateOperatorExpression` | `_IOperatorExpression` |
| `primary` — диспетчер | `OperandParser.cs:420 ParseSTOperandHelp` (`switch TokenType`) | — | `_IExpression` |
| `literal` | `OperandParser.cs:430-446` + `Utilities/FactoryExtension.cs:43` | `CreateLiteralExpression` | `_ILiteralExpression` |
| `directVariable` | `OperandParser.cs:447` | `CreateAddressExpression`+`CreateDirectVariable` | `_IAddressExpression` |
| `identifier`/`primary` | `OperandParser.cs:465` | `CreateVariableExpression` | `_IVariableExpression` |
| `"(" expression ")"` | `Expressions/ParenthesizedExpressionParser.cs:83` | (прозрачно, позиция к `(`) | `_IExpression` |
| `prefixedOperator` (`ADR`/`ABS`/…) | `Expressions/PrefixedOperatorParser.cs:102 ParsePrefixedOperator` | `CreateOperatorExpression` | `_IOperatorExpression` |
| `"SIZEOF"/"__TYPEOF"` (тип-аргумент) | `PrefixedOperatorParser.cs:170 HandleSizeOfOperator` | `CreateTypeExpression` | `_ITypeExpression` |
| `conversionOperator` (`INT_TO_REAL`, `ANY_TO_*`) | `Expressions/ConversionExpressionParser.cs:106` | `CreateConversionExpression` | `_IConversionExpression` |
| `newExpression` (`__NEW`) | `Expressions/NewExpressionParser.cs:99` | `CreateNewExpression` | `_INewExpression` |
| `currentTaskExpression` (`__CURRENTTASK`255) | `Expressions/CurrentTaskExpressionParser.cs:55` | `CreateCurrentTaskExpression` | `_ICurrentTaskExpression` |
| `"THIS"/"SUPER"` (120/121) | `Expressions/ThisAndBaseExpressionParser.cs:49` | `CreateThisExpression`/`CreateBaseExpression` | `_IThisExpression`/`_IBaseExpression` |
| `"MIN"/"MAX"` (40/41) | `Expressions/MinMaxOperatorParser.cs:91` | `CreateOperatorExpression(40/41)` | `_IOperatorExpression` |
| `postfix` `.` | `OperandParser.cs:158 ParseComponentAccess` (162) | `CreateCompoAccessExpression` | `_ICompoAccessExpression` |
| `postfix` `(...)` | `Expressions/FunctionCallParser.cs:81` (167) | `CreateCallExpression` (`ICallBuilder`) | `_ICallExpression` |
| `postfix` `[...]` | `OperandParser.cs:89 ParseArrayAccess` (169) | `CreateIndexAccessExpression`+`AddAccess` | `_IIndexAccessExpression` |
| `postfix` `^` | `OperandParser.cs:268` (183) | `CreateDeRefAccessExpression` | `_IDeRefAccessExpression` |
| `postfix` `#` | `OperandParser.cs:132 ParseNamespaceAccess` (271) | `CreateNamespaceAccessExpression` | `_INamespaceAccessExpression` |
| `partialAccess` (в postfix) | `OperandParser.cs:189` (27) | `CreatePartialAccessExpression` | `_IPartialAccessExpression` |
| `global/system/pool scope` | `Expressions/ScopeExpressionParser.cs:76`, `OperandParser.cs:397` | `CreateGlobal/System/Pool/CopyScopeExpression` | `_I…ScopeExpression` |
| `__CAST` | `Expressions/ImplicitCastOperatorParser.cs:93` (op 202) | `CreateCastExpression` | `_ICastExpression` |

---

## 7. Сводка по диспетчерам (точки расширения)

| Диспетчер | Файл:строка | Ключ |
|---|---|---|
| Инструкции по `TokenType` | `Statements/StatementParser.cs:249` | `2,3,4,7,13,14,15,21` |
| Инструкции по `Operator` | `Statements/StatementParser.cs:487` | 292 значения |
| Префиксные операторы | `Expressions/ExpressionParser.cs:169` | `Operator` |
| Операнд по `TokenType` | `Expressions/OperandParser.cs:428` | `1,5,6,7,9,10,11,13,14,15,16,17,18,22,23,24,25` |
| Типы по `Operator`/`Identifier` | `Declaration/TypeParser.cs:181/171` | `Operator`, `Identifier(13)` |
| Лексика по символу | `Scanner/InternalScanner.cs:3500` | `char` |
| Ресинхронизация ST/IF | `Utilities/ResynchronizerTables.cs:26` | 23 / 12 операторов |
