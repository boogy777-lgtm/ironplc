# 06 — Сквозная карта `токен → парсер → builder → узел AST`

Документ описывает полную цепочку построения языковой модели ST в
`Parser35220.plugin.dll` (CODESYS 3.5.22.10) и служит мостом к порту на Rust.
Зона: только `docs/06_*`, `grammar/AST_MAPPING.md`, `tables/ast_builder_map.csv`.

Обозначения:
- `file:line` — строки в `C:\Codesys\Parser35220.plugin\CODESYS\Parser35220\...`
  (если не указан другой корень).
- Числа операторов — значения `enum Operator` (`tables/operators.csv`, 292 значения).
- Числа токенов — `enum TokenType` (`tables/token_types.csv`, 28 значений).

---

## 1. Сквозная цепочка

```
Вход (char[] с '\0'-сентиелом)
   │
   ▼
InternalScanner.GetNext(out token)                 Scanner/InternalScanner.cs:3476
   │   фильтр: Whitespace(19) / EndOfLine(12) / position-pragma(4+flag)
   ▼
InternalScanner.GetNextInternal(out token,..)      Scanner/InternalScanner.cs:3488
   │   switch(char) → Comment(2)/DocComment(3)/Pragma(4)/Identifier(13)/
   │   Integer(14)/Real(16)/Operator(15)/Boolean(1)/строки/даты/%var/…
   ▼
InternalParser.ParseST | ParseSTStatement | ParseSTSExpression
   ├─ InternalParser.cs:177  ParseST(bool)        → тело POU (_ISequenceStatement)
   ├─ InternalParser.cs:303  ParseSTStatement     → StatementParser
   ├─ InternalParser.cs:275  ParseAssignExp       → ExpressionParser
   ├─ InternalParser.cs:287  ParseSTOperand       → ExpressionParser
   ├─ InternalParser.cs:317  ParseType            → TypeParser
   └─ InternalParser.cs:333  ParsePOUs            → POUSyntaxParser
   │
   ▼
Диспетчеры:
   StatementParser.ParseSTStatementHelp    Statements/StatementParser.cs:242  (switch по TokenType)
   StatementParser.ParseOperatorStatement  Statements/StatementParser.cs:485  (switch по Operator)
   ExpressionParser.ParseSTPrefixOperator  Expressions/ExpressionParser.cs:164 (switch по Operator)
   OperandParser.ParseSTOperandHelp        Expressions/OperandParser.cs:420  (switch по TokenType)
   TypeParser.HandleOperatorCase           Declaration/TypeParser.cs:191      (switch по Operator)
   │
   ▼
Специализированные под-парсеры (StatementParser/ExpressionParser/…)
   │
   ▼
Builder-фабрика (Language Model):
   ParserContext.LMItemFactory : _ILanguageModelBuilder8   Utilities/ParserContext.cs:37
        ├─ InternalParser   → _ILanguageModelBuilder7      InternalParser.cs:88
        ├─ StatementParser  → _ILanguageModelBuilder6      Statements/StatementParser.cs:39
        ├─ ExpressionParser → _ILanguageModelBuilder6      Expressions/ExpressionParser.cs:29
        └─ EnumListParser   → _ILanguageModelBuilder7      Declaration/EnumListParser.cs:51
   │   вызовы вида LMItemFactory.CreateX(...)
   ▼
Узел AST (интерфейсы _3S.CoDeSys.Core.LanguageModel / …InternalInterfaces):
   _ISequenceStatement, _IIfStatement, _ICaseStatement, _IOperatorExpression, _ICallExpression, …
```

### 1.1 Fluent ред-три билдеры (второй равнозначный API)

Интерфейсы — `decompiled/Compiler/_3S/CoDeSys/Compiler/LanguageModelBuilder/` (77
файлов), **реализации** — `decompiled/LanguageModelManager.plugin/_3S/CoDeSys/
LanguageModelManager/RedTrees/Builder/` (12 классов). Точка входа —
`RedTreeBuilderFactory : IRedTreeBuilderFactory`
(`…/Builder/RedTreeBuilderFactory.cs:13`, `TypeGuid {A06288A8-B84B-433C-8EA7-1AE3C85E1A73}`),
свойства возвращают `XxxBuilder.Init()`.

Ключевой факт: `Build()` этих билдеров вызывает **тот же** singleton-фабрику
`LanguageModelBuilder.Singleton.CreateX(...)`, что и парсер через
`_ILanguageModelBuilder6/7/8`. То есть fluent-API и `CreateX`-API конструируют
идентичные узлы. Поэтому колонки `builder_interface`/`builder_call_method` в CSV
заполнены fluent-цепочками (дословно из кода реализации), а `ast_node_hint`
даёт узел.

Точные цепочки (`Init()->…->Build()`, `At()` — позиция, `NoPosition()` — нулевая):

| Билдер | Цепочка | Файл реализации |
|---|---|---|
| `IIfBuilder` | `At()/NoPosition() → Condition() → Then() → ElseIfs()/Else() → Build()` | `Statements/IfBuilder.cs:96` |
| `IElseIfBuilder` | `At() → Condition() → Controlled() → Build()` | `Statements/ElseIfBuilder.cs:52` |
| `ICaseBuilder` | `At() → Switch() → Case(ICase)/Case(label,seq) → Else() → Build()` | `Statements/CaseBuilder.cs:70` |
| `IForBuilder` | `At()/NoPosition() → Counter() → Range(lo,hi) → By()/ByOne() → Controlled() → Build()` | `Statements/ForBuilder.cs:71` |
| `IWhileBuilder` | `At() → Condition() → Controlled() → Build()` | `Statements/WhileBuilder.cs:43` |
| `IOperatorBuilder` | `At() → Lhs() → Op() → Rhs() → Build()` (строго бинарный) | `Expressions/OperatorBuilder.cs:50` |
| `ICallBuilder` | `At() → Callee() → Condition() → AddParam()/AddOutput() → Build()` | `Expressions/CallBuilder.cs:67` |
| `IPouDeclarationBuilder` | `At() → AsProgram()/AsFunction()/AsFunctionBlock()/AsInterface()/AsMethod()/AsAction() → Access() → Name() → Extends()/Implements()/Returns() → AddVariableDeclarations() → Build()` | `Statements/PouDeclarationBuilder.cs:128` |
| `ITypeDeclarationStatementBuilder` | `At() → Alias()/Name() → Extends()/StructOrUnion()/Enum() → InitialValue() → Flags() → Build()` | `Statements/TypeDeclarationBuilder.cs:87` |
| `IEnumDeclarationListBuilder` | `At() → Enumeration(name,init)/Enumeration(eds) → BaseType() → Build()` | `Statements/EnumDeclarationListBuilder.cs:52` |
| `IVariableDeclarationListBuilder` | `At() → Flags() → Declaration(names,type,initial)/Declaration(vds) → Build()` | `Statements/VariableDeclarationListBuilder.cs:53` |

Замечания:
- Парсер `Parser35220` **не использует** `IRedTreeBuilderFactory` (потребителей в
  декомпиле нет) — это экспортируемый API (`[ReleasedInterface]`) для внешних
  плагинов/редактора; семантика узлов совпадает.
- `IOperatorBuilder` строго бинарный, тогда как `InfixOperationParser` строит
  n-арные `_IOperatorExpression` через `AddOperandHelp` — при порте это различие
  учесть (n-арная свёртка vs бинарное дерево).
- **Квази-баг в `IfBuilder.Build()`** (`Statements/IfBuilder.cs:45-48`): при
  `_else != null` присваивается `ifStatement._IfElse = this._then` (вместо `_else`).
  Логика else-if-флаттенинга (строки 56-77) при этом 1:1 повторяет
  `IfStatementParser.RemoveElseIfs`. При переносе — воспроизвести поведение
  парсера (`_IfElse = seq`), а не билдера; проверить по IL/тестам.
- `IfBuilder/ForBuilder/ElseIfBuilder` содержат перегрузку `Controlled(IEnumerable<IStatement>)`,
  синтезирующую `ISequenceStatement2` через `CreateSequenceStatementEx`.

### 1.2 Интерфейсы-factory `_ILanguageModelBuilder*` (CreateX-API)

`_ILanguageModelBuilder6/7/8` — **тонкие маркеры** (11/13/17 строк): они лишь
наследуют всю иерархию. Реальные `CreateX`-методы лежат в:

```
_3S.CoDeSys.LanguageModelManager.InternalInterfaces (decompiled/Compiler/…/InternalInterfaces):
  _ILanguageModelBuilder.cs   198 CreateX   ← базовый внутренний
  _ILanguageModelBuilder2.cs    8 CreateX
  _ILanguageModelBuilder3.cs    1 CreateX
  _ILanguageModelBuilder4.cs    3 CreateX   (в т.ч. CreateGenericUserdefType)
  _ILanguageModelBuilder5.cs    1 CreateX   (CreatePartialAccessExpression)
  _ILanguageModelBuilder6.cs  = _ILanguageModelBuilder5 + …
  _ILanguageModelBuilder7.cs  = _ILanguageModelBuilder6 + …
  _ILanguageModelBuilder8.cs  = _ILanguageModelBuilder7 + …

_3S.CoDeSys.Core.LanguageModel (released, публичный):
  ILanguageModelBuilder.cs     91 CreateX   (CreatePou/DataType/GlobVarlist/…)
  ILanguageModelBuilder2..12  : Create*Ex / LDate* / Generic / Pointer / RuntimeVersion / …
```

Итого ~320 `CreateX`-методов в двух иерархиях. Парсер обращается к ним как
`LMItemFactory.CreateX(...)`; для Rust это один trait с default-реализациями
(создание узлов AST), унаследованный «уровнями» версий. Отдельно портировать
сигнатуры 1:1 не нужно — в `ast_builder_map.csv` для каждого конструкта указан
конкретный `CreateX`, а список выше даёт полный инвентарь для автогенерации.

> **Закрыто (#2)**: интерфейсы и их методы найдены в
> `decompiled/Compiler/…/InternalInterfaces` и `…/Core/LanguageModel`.

### Важные режимы `ParserContext`
- `StatementParser.InDeclaration` — переключает `VAR`-декларации vs код.
- `StatementParser.InCase` — разрешает разбор `CASE`-меток.
- `StatementParser.Implicit / ImplicitAnyway` — включает `ParseInitialisationExp`.
- `InternalParser.SetInSTCode` (`{ST_IMPLEMENTATION}`) → `InitializationParser.InSTCode`
  меняет трактовку `=:` (Operator 189).
- `ParserContext._bReportSP18/19/20Feature` — гейты по версии компилятора.

---

## 2. Таблица «конструкция ST → парсер → builder → узел»

Полная машинночитаемая версия — `tables/ast_builder_map.csv` (колонки
`construct, parser_method(file:line), builder_interface, builder_call_method, ast_node_hint`).
Ниже — срез по категориям.

### 2.1 Инструкции (`Statements/`)

| Конструкция | Парсер | Builder-вызов | Узел |
|---|---|---|---|
| Диспетчер инструкций | `StatementParser.cs:242` | `switch(TokenType)` | `_IStatement` |
| `//` / `(**)` | `StatementParser.cs:254/261` | `CreateCommentStatement` | `_ICommentStatement` |
| `{pragma}` | `StatementParser.cs:268` | `PragmaStatementParser.ParsePragma` | `_IPragmaStatement` |
| `expr;` | `StatementParser.cs:278` | `CreateExpressionStatement` | `_IExpressionStatement` |
| `;` | `StatementParser.cs:626/772` | `CreateEmptyStatement` | `_IEmptyStatement` |
| `IF` | `IfStatementParser.cs:106` | `CreateIfStatement` (`IIfBuilder`) | `_IIfStatement` |
| `ELSIF` | `IfStatementParser.cs:196` | `CreateElseIf` (`IElseIfBuilder`) | `_IElseIf` |
| `CASE` | `CaseStatementParser.cs:146` | `CreateCaseStatement` (`ICaseBuilder`) | `_ICaseStatement` |
| `CASE`-метка/диапазон | `CaseStatementParser.cs:367/400` | `CreateCaseLabelStatement` / `CreateCaseRangeExpression` | `_ICaseLabelStatement` / `_ICaseRangeExpression` |
| `FOR` | `ForStatementParser.cs:113` | `CreateForStatement` (`IForBuilder`) | `_IForStatement` |
| `WHILE` | `WhileStatementParser.cs:106` | `CreateWhileStatement` (`IWhileBuilder`) | `_IWhileStatement` |
| `REPEAT` | `RepeatStatementParser.cs:107` | `CreateRepeatStatement` | `_IRepeatStatement` |
| `__TRY/__CATCH/__FINALLY` | `TryCatchStatementParser.cs:93` | `CreateTryCatchStatement` | `_ITryCatchStatement` |
| `EXIT` / `CONTINUE` | `StatementParser.cs:547/549` | `CreateExitStatement` / `CreateContinueStatement` | `_IExitStatement` / `_IContinueStatement` |
| `RETURN` | `ReturnStatementParser.cs:71` | `CreateReturnStatement` | `_IReturnStatement` |
| `JMP` | `JumpStatementParser.cs:94` | `CreateJumpStatement` | `_IJumpStatement` |
| `CALC` | `ConditionalCallParser.cs:94` | `CreateExpressionStatement`+`CreateNullExpression` | `_IExpressionStatement`, `_ICallExpression._Condition` |
| `__WAIT` | `StatementParser.cs:852` | `CreateWhileStatement`+`CreateCallExpression("SynchWait")` | синтетический `_IWhileStatement` |
| `__BEGIN_IMPLEMENTATION` | `ImplementationBlockParser.cs:31` | `CreateEmbeddedLanguageStatement` | `_IEmbeddedLanguageStatement` |
| `label:` | `StatementParser.cs:929` | `CreateLabelStatement` | `_ILabelStatement` |
| `VAR...END_VAR` (список) | `VariableListParser.cs:87` | `CreateVariableDeclarationListStatement` (`IVariableDeclarationListBuilder`) | `_IVariableDeclarationListStatement` |
| Ошибка | `StatementParser.cs:300/311` | `CreateErrorStatement` | `_IErrorStatement` |

#### 2.1.1 Под-парсеры инструкций (явный список, все входят в порт)

`StatementParser.ParseOperatorStatement` делегирует специализированным под-парсерам.
Каждый — отдельный класс со своей точкой входа; в `ast_builder_map.csv` указан
первым (caller — в скобках), поэтому покрытие проверяется по имени файла.

| Под-парсер | Точка входа | Вызов из |
|---|---|---|
| `IfStatementParser` | `IfStatementParser.cs:106 ParseIf` | `StatementParser.cs:561` |
| `CaseStatementParser` | `CaseStatementParser.cs:146 ParseCaseStatement`, `:367 ParseCaseLabelStatement`, `:400 ParseCaseRangeExpression` | `StatementParser.cs:509/794` |
| `ForStatementParser` | `ForStatementParser.cs:113 ParseFor` | `StatementParser.cs:551` |
| `WhileStatementParser` | `WhileStatementParser.cs:106 ParseWhile` | `StatementParser.cs:593` |
| `RepeatStatementParser` | `RepeatStatementParser.cs:107 ParseRepeat` | `StatementParser.cs:565` |
| `ReturnStatementParser` | `ReturnStatementParser.cs:71 ParseReturn` | `StatementParser.cs:569` |
| `JumpStatementParser` | `JumpStatementParser.cs:94 ParseJump` | `StatementParser.cs:612` |
| `ConditionalCallParser` | `ConditionalCallParser.cs:94 ParseConditionalCall` | `StatementParser.cs:604` |
| `TryCatchStatementParser` | `TryCatchStatementParser.cs:93 ParseTryCatchStatement` | `StatementParser.cs:675` |
| `ImplementationBlockParser` | `ImplementationBlockParser.cs:31 ParseImplementationBlock` | `StatementParser.cs:724` |
| `POUDeclarationParser` | `POUDeclarationParser.cs:178 ParsePOUDeclarationInternal` | `StatementParser.cs:753/759` |

Плюс `Declaration/POUSyntax.cs:9` — не парсер, а модель-контейнер `IPOUSyntax`
(`Declaration`/`Implementation`/`SubPOUs`), наполняется `POUSyntaxParser`.

### 2.2 Выражения (`Expressions/`)

| Конструкция | Парсер | Builder-вызов | Узел |
|---|---|---|---|
| `lhs := rhs` | `ExpressionParser.cs:114` | `CreateAssignmentExpression` | `_IAssignmentExpression` |
| `:=` в инициализации | `InitializationParser.cs:95` | `CreateAssignmentExpression` | `_IAssignmentExpression` |
| `(a:=1,b:=2)` | `StructureInitializationParser.cs:95` | `CreateStructureInitialisation` | `_IStructureInitialization` |
| `[1,2,3]` | `ArrayInitializationParser.cs:94` | `CreateArrayInitialisation` | `_IArrayInitialization` |
| `[i:=v]` | `ArrayInitializationParser.cs:157/215` | `CreateMultipleIndexInitialization` | `_IMultipleIndexInitialization` |
| `OR/AND/=..` (инфикс) | `InfixOperationParser.cs:192/156/126/98/70` | `CreateOperatorExpression`+`AddOperandHelp` (`IOperatorBuilder`) | `_IOperatorExpression` |
| `NOT x` | `UnaryNotParser.cs:55` | `CreateOperatorExpression(133)` | `_IOperatorExpression` |
| `+x/-x` | `UnaryPlusMinusParser.cs:77` | `CreateOperatorExpression(158)` / literal | `_IOperatorExpression`/`_ILiteralExpression` |
| `ADR(x)/ABS(x)/…` | `PrefixedOperatorParser.cs:102` | `CreateOperatorExpression` | `_IOperatorExpression` |
| `SIZEOF(t)`/`__TYPEOF(t)` | `PrefixedOperatorParser.cs:170` | `CreateTypeExpression` | `_ITypeExpression` |
| `INT_TO_REAL(x)` | `ConversionExpressionParser.cs:106` | `CreateConversionExpression` | `_IConversionExpression` |
| литералы | `OperandParser.cs:420` + `FactoryExtension.cs:43` | `CreateLiteralExpression` | `_ILiteralExpression` |
| `%IX6.0` | `OperandParser.cs:447` | `CreateAddressExpression`+`CreateDirectVariable` | `_IAddressExpression` |
| идентификатор | `OperandParser.cs:465` | `CreateVariableExpression` | `_IVariableExpression` |
| `.x` / `[i]` / `#ns` / `^` / `%B2` | `OperandParser.cs:158/89/132/268/189` | `CreateCompoAccess/IndexAccess/NamespaceAccess/DeRefAccess/PartialAccessExpression` | соответствующие `_I…AccessExpression` |
| `F(...)` | `FunctionCallParser.cs:81` | `CreateCallExpression` (`ICallBuilder`) | `_ICallExpression` |
| `(expr)` | `ParenthesizedExpressionParser.cs:83` | (прозрачно) | `_IExpression` |
| `__SYSTEM/__POOL/__GLOBAL/__COPY` | `ScopeExpressionParser.cs:76` | `CreateSystem/Pool/Global/CopyScopeExpression` | `_I…ScopeExpression` |
| `THIS/SUPER` | `ThisAndBaseExpressionParser.cs:49` | `CreateThisExpression`/`CreateBaseExpression` | `_IThisExpression`/`_IBaseExpression` |
| `MIN/MAX(...)` | `MinMaxOperatorParser.cs:91` | `CreateOperatorExpression(40/41)` | `_IOperatorExpression` |
| `__NEW(t,..)` | `NewExpressionParser.cs:99` | `CreateNewExpression` | `_INewExpression` |
| `__CAST(...)` | `ImplicitCastOperatorParser.cs:93` | `CreateCastExpression` | `_ICastExpression` |
| `__CURRENTTASK` | `CurrentTaskExpressionParser.cs:55` | `CreateCurrentTaskExpression` | `_ICurrentTaskExpression` |

### 2.3 Объявления (`Declaration/`)

| Конструкция | Парсер | Builder-вызов | Узел |
|---|---|---|---|
| Диспетчер типов | `TypeParser.cs:167` | `switch(Operator/Identifier)` | `_IType` |
| userdef / generic | `TypeParser.cs:393/408` | `CreateUserdefType`/`CreateGenericUserdefType` | `_IUserdefType`/`IGenericUserdefType` |
| subrange `a..b` | `TypeParser.cs:123` | `CreateSubrangeType` | `_ISubrangeType` |
| `ARRAY[..] OF` | `TypeParser.cs:572` | `CreateArrayType`/`CreateVariableLengthArrayType` | `_IArrayType` |
| `STRING/WSTRING/__XSTRING` | `TypeParser.cs:540/510/480` | `CreateString/WString/XStringType` | `_IStringType`/`_IWStringType`/`_IXStringType` |
| `POINTER/REFERENCE TO` | `TypeParser.cs:675/650` | `CreatePointerType`/`CreateReferenceType` | `_IPointerType`/`_IReferenceType` |
| `PARAMS(n) OF` / `__VECTOR` | `TypeParser.cs:297/450` | `CreateParamsType`/`CreateVectorType` | `_IType` |
| `(A,B,C)` enum | `EnumListParser.cs:109` | `CreateEnumDeclarationListStatement` (`IEnumDeclarationListBuilder`) | `_IEnumDeclarationListStatement` |
| `TYPE` п. | `TypeDeclarationParser.cs:116` | `CreateTypeDeclarationStatement` (`ITypeDeclarationStatementBuilder`) | `_ITypeDeclarationStatement` |
| `VAR` п. | `VariableListParser.cs:87` | `CreateVariableDeclarationListStatement` | `_IVariableDeclarationListStatement` |
| `a,b : T := v` | `VariableDeclarationParser.cs:149` | `CreateVariableDeclarationStatement` | `_IVariableDeclarationStatement` |
| `PROGRAM/FB/FUNCTION/…` | `POUDeclarationParser.cs:178` | `CreatePOUDeclarationStatement` (`IPouDeclarationBuilder`) | `_IPOUDeclarationStatement` |
| `METHOD` | `POUDeclarationParser.cs:22` | `CreateMethodDeclarationStatement` | `_IMethodDeclarationStatement` |

### 2.4 Прагмы (`Pragmas/`)

| Конструкция | Парсер | Builder-вызов | Узел |
|---|---|---|---|
| `{...}` диспетчер | `PragmaStatementParser.cs:114` | `CreatePragmaStatement`/`CreatePragmaStatement2` | `_IPragmaStatement` |
| `{IF}/{ELSIF}/{ELSE}` | `PragmaIfStatementParser.cs:21` | `CreatePragmaIfStatement` | `_IPragmaIfStatement` |
| `{error/warning/text}` | `ErrorPragmaParser.cs:20/27/34` | `CreateErrorStatement` | `_IErrorStatement` |
| `{attribute ...}` | `HasAttributePragmaParser.cs:19` | `CreateHasAttributeExpression` | `_IHasAttributeExpression` |
| `{hastype/isenumtype}` | `HasTypePragmaParser.cs:19/26` | `CreateHasTypeExpression` | `_IHasTypeExpression` |
| `{hasvalue}` | `HasValuePragmaParser.cs:19` | `CreateHasValueExpression` | `_IHasValueExpression` |
| `{hasconstantvalue/type}` | `HasConstantValueOrTypePragmaParser.cs:20/27` | `CreateHasConstantValueExpression` | `_IHasConstantValueExpression` |
| `{defined/project_defined}` | `DefinedPragmaOperandParser.cs:19/26` | `CreateDefinedExpression` | `IDefinedExpression` |
| `{xref/COMPILERVERSION/...}` | `XRefPragmaOperandParser.cs:19`, `PragmaVersionOperandParser.cs:19` | операнд-выражение | `IExpression` |

---

### 2.5 Пост-проверки (не билдеры)

| Проверка | Файл:строка | Условие | MessageId |
|---|---|---|---|
| Неожиданные инструкции в реализации | `Statements/CodeStatementChecker.cs:22` | `POUDecl/TypeDecl/VarDeclList/VarDecl/EnumDecl` внутри кода → подмена на `CreateErrorStatement` | 578 |
| Глубина вложенности | `Statements/CodeStatementChecker.cs:52/123` | `_nStackDepth > 5000` → `MaximumNestingDepthExceededException` | 584 |
| Код после декларации | `Statements/DeclarationStatementChecker.cs:39` | после `POUDecl`/`TypeDecl` не-декларация → ошибка | 578 |
| Есть ли декларативный синтаксис | `Declaration/DeclarationStatementDetector.cs:12` | визитор: `VarDecl/VarDeclList` → `true` | — |

`CodeStatementChecker` — визитор `IExprementVisitor`; `DeclarationStatementChecker`
проверяет флаги VAR-списка `GetFlag(8192)/GetFlag(4096)` (RETAIN/PERSISTENT).

> **Закрыто (#6)**: семантика и MessageId зафиксированы.

## 3. Построение итогового дерева POU

```
InternalParser.ParsePOUs()                         InternalParser.cs:331
   └─ POUSyntaxParser.ParsePOUSyntax(context)      Declaration/POUSyntaxParser.cs:29
        ├─ SyntaxElementParser.ParseSyntaxElements Declaration/SyntaxElementParser.cs:63
        │    цикл: CheckForEndOfPOU() → EndOfPOUElement | NextStatement() → StatementElement
        ├─ NextTopLevelPOUSyntax(index)            Declaration/POUSyntaxParser.cs:56
        │    первый _IPOUDeclarationStatement → pouSyntax.Declaration
        │    следующий _ISequenceStatement    → pouSyntax.Implementation
        │    вложенные POU                    → ReadSubPou(...)  :177
        ├─ FixPouSyntaxPosition / MoveTrailingCommentsAndPragmasToImplementation
        │    (комментарии/прагмы из декларации переносятся в реализацию)
        └─ s_htValidChildPouTypes :502  — валидность вложенности POU
             (PROGRAM/FB → ACTION, METHOD, PROPERTY, TRANSITION; INTERFACE → METHOD/PROPERTY; …)
   ▼
IPOUSyntax[]  (Declaration, Implementation, SubPOUs)

InternalParser.CreateLanguageModelOfRawST(...)     InternalParser.cs:396
   └─ LanguageModelOfRawST.CreateLanguageModelOfRawST LanguageModelOfRawST.cs:15
        GetPOUName/GetTypeName (getter/setter, overload suffix)  :83/:72
        CreateLanguageModelOfPOUSyntax           :34
           ├─ _IPOUDeclarationStatement.Class==287 (Namespace)   → CreateGlobVarlist
           ├─ _ITypeDeclarationStatement                         → CreateDataType
           └─ иначе                                              → CreatePou (Interface=Declaration, Body=Implementation)
   ▼
ILanguageModel (AddPou / AddDataType / AddGlobalVariableList)
```

`InternalParser.ParseRawST()` (`InternalParser.cs:353`) — «сырой» поток без
классификации POU: включает pragmas/comments/auto-increment и наполняет один
`_ISequenceStatement`; `AddStatementAndHandleDeclarations` (`:373`) переносит
«осиротевшие» декларации из интерфейса POU в последовательность.

`ForLoopExtender.ExtendForLoop` (`ForLoopExtender.cs`) синтезирует
`_Condition`/`_Counter` для `FOR` (при `BY`-расширении).

---

## 4. Обработка ошибок и ресинхронизация

- Точки генерации: `IErrorHandler.AddErrorST / AddErrorSTWithToken / AddWarningST`
  и `AddError` (в `TypeParser.AddErrorIF`, `ScannerExtensions.OneOfNOperatorsExpected`).
- Узел-ошибка: `CreateErrorStatement` / `CreateErrorExpression`; позиция через
  `CreateMinimalPosition` или токен.
- **Таблицы ресинхронизации** — `Utilities/ResynchronizerTables.cs:26`:
  - `ReSyncTableST` (23): `89 IF, 101 THEN, 68 ELSE, 69 ELSIF, 75 END_IF, 172 ';',
    85 FOR, 102 TO, 64 BY, 67 DO, 72 END_FOR, 65 CASE, 90 OF, 71 END_CASE, 115 WHILE,
    82 END_WHILE, 96 REPEAT, 77 END_REPEAT, 104 UNTIL, 163 ':', 83 EXIT, 98 RETURN, 164 ':='`.
  - `ReSyncTableIF` (12): `172, 81 END_VAR, 105 VAR, 106, 107..112, 114, 244` —
    только на уровне объявлений (declaration resync).
- Алгоритм: `ScannerExtensions.ParseReSync` (`:27`) читает токены, пока не найдёт
  оператор из `ops ∪ таблица` или `End(21)`; сохраняет `(op, token)`.
- `MatchOperator` (`ScannerExtensions.cs:138`) при несовпадении генерирует
  `MessageId 6` (`"<список> OR <токен>"`) и вызывает `ParseReSyncIF`.
- Ошибка незакрытой конструкции → `MessageId 10` (`END_x expected`),
  пропущенная `;` → `189`/`190`, `MaximumNestingDepthExceeded` → `584`.
- «Локальные» ошибки (`out bool bError` из под-парсеров) — сигнал вызывающему
  выполнить `ParseReSyncST` и продолжить с точки сброса.

Полный каталог MessageId — `docs/03_ERROR_CATALOG.md`.

---

## 5. План порта на Rust

### 5.1 Модель позиций

```rust
#[repr(u8)]
pub enum TokenType { None=0, Boolean=1, Comment=2, DocComment=3, Pragma=4, Date=5,
    DateAndTime=6, DirectVariable=7, IncompleteDirectVariable=8, DoubleByteString=9,
    Duration=10, LDuration=11, EndOfLine=12, Identifier=13, Integer=14, Operator=15,
    Real=16, SingleByteString=17, TimeOfDay=18, Whitespace=19, Error=20, End=21,
    XByteString=22, LDate=23, LTimeOfDay=24, LDateAndTime=25, Unused=26, PartialAccess=27 }

#[repr(u16)]
pub enum Operator { /* 292 значения 1:1 из tables/operators.csv */ }

pub struct Token {          // src: Scanner/Token.cs
    pub ty: TokenType,
    pub source_offset: u32,     // абсолютный индекс в char[]
    pub length: u32,
    pub source_line: u32,       // 0-based
    pub source_column: u32,     // = source_offset - line_start
    pub position: i64,          // логическая позиция (переопределяется {position:=N})
    pub position_offset: i16,   // source_offset - token_start
    pub characters_to_skip_seen: i64,
}

pub struct Scanner {        // src: Scanner/InternalScanner.cs
    input: Vec<char>,           // ОБЯЗАН заканчиваться '\0'
    pos: i64,
    line_start: u32,
    token_start: u32,
    source_line: u32,
    characters_to_skip_seen: i64,
    pub options: ScanOptions,   // include_whitespaces/end_of_lines/comments/pragmas,
                                // position_pragmas, ignore_case, allow_nested_comments,
                                // allow_multiple_underlines, unicode_ids, non_compliant_ids,
                                // auto_increment_position_on_line_breaks, positions_start_at_one
    op_table: &'static OperatorTable,
}
```

`QuickStringBuilder` → `&'a [char]` / `&'a str`-срез по `input` (без аллокаций).

### 5.2 Структура AST-enum'ов/struct'ов

Дерево — tagged enum для выражений и инструкций; у всех узлов есть позиция и
`Vec<Message>` (диагностика «на узле»), как в `_IExprement`/`_IStatement`.

```rust
pub struct Span { pub start: u32, pub len: u32, pub logical: i64 }

pub enum Expr {
    Literal(Literal), Variable(VarExpr), Address(DirectVar),
    Assignment(Box<Expr>, Box<Expr>, AssignKind),
    Operator { op: Operator, operands: Vec<Expr> },     // _IOperatorExpression
    Call(CallExpr),                                     // _ICallExpression
    Index { base: Box<Expr>, indices: Vec<Expr> },
    Compose { base: Box<Expr>, right: Box<Expr> },
    Namespace { base: Box<Expr>, access: Box<Expr> },
    DeRef(Box<Expr>), Partial { base: Box<Expr>, size: DirectVariableSize },
    GlobalScope(Box<Expr>), SystemScope(Box<Expr>), PoolScope(Box<Expr>), CopyScope(Box<Expr>),
    Conversion { from: TypeId, to: TypeId, exp: Box<Expr> },
    Cast { base: Box<Expr>, exp_with_type: Option<Box<Expr>>, explicit_ty: Option<TypeId> },
    New { ty: TypeId, count: Box<Expr>, inputs: Vec<CallParam> },
    This, Base, CurrentTask(Box<Expr>),
    StructInit(Vec<Expr>), ArrayInit { values: Vec<Expr>, multi: Vec<(Expr,Expr)> },
    Paren(Box<Expr>), Error(ErrorExpr),
}

pub enum Stmt {
    Seq(Vec<Stmt>), Comment(CommentKind), Empty, ExprStmt(Expr),
    If { cond: Expr, then: Box<Stmt>, else_ifs: Vec<(Expr, Box<Stmt>)>, else_: Option<Box<Stmt>> },
    Case { switchexpr: Expr, cases: Vec<(CaseLabel, Stmt)>, else_: Option<Box<Stmt>> },
    For { counter: Expr, lower: Expr, upper: Expr, by: Expr, body: Box<Stmt> },
    While { cond: Expr, body: Box<Stmt> },
    Repeat { body: Box<Stmt>, until: Expr },
    TryCatch { try_: Box<Stmt>, catch: Option<Box<Stmt>>, exc: Option<Expr>, finally_: Option<Box<Stmt>> },
    Exit, Continue, Return(Option<Expr>), Jump { label: String, cond: Option<Expr> },
    Label(String), CaseLabel(Vec<CaseLabel>), Pragma(Pragma), Error(ErrorStmt),
    Embedded { kind: Option<String>, raw: String },
    TypeDecl(TypeDecl), VarList(VarList), PouDecl(PouDecl),
}

pub struct Pou { pub decl: Stmt, pub impl_: Stmt, pub sub: Vec<Pou> }
```

Соответствие `enum` ↔ интерфейсы LM фиксируется в `tables/ast_builder_map.csv`
(колонка `ast_node_hint`) — это контракт совместимости узлов.

### 5.3 Таблица диспетчеризации сканера

Точная таблица из декомпилированного `GetNextInternal` (`InternalScanner.cs:3488`,
диспетчер `switch(char)`; строки 3500–3745). Ветвление начинается с `if (c <= 'S')`:

| Первый символ `c` | Ветка | Результат |
|---|---|---|
| `'\0'` | `InternalScanner.cs:3506` | `End(21)` |
| `'\n' '\r'` | `EndOfLine():3519` | `EndOfLine(12)` |
| `' ' '\t' '\v' '\f' '\u00A0'` | `ScanWhitespace():3697` | `Whitespace(19)` |
| `'"' '\''` | `ValidateStringToken():3549` | `DoubleByteString(9)` / `SingleByteString(17)` (ошибка → `Error(20)`) |
| `'%'` | `ScanPercentLeading():3568` | `DirectVariable(7)`/`Incomplete(8)`/`PartialAccess(27)` |
| `'('` | `:3570`; если след. `'*'` → `ScanComment` | `Comment(2)` иначе `Operator(15)` |
| `'*'` | `:3580`; если след. `'*'` → `**` | `Operator(15)` |
| `'.'` | `:3590`; если след. `'.'` → `..` | `Operator(15)` |
| `'/'` | `:3600`; если след. `'/'` → `ScanSingleLineComment` | `DocComment(3)`/`Comment(2)` иначе `Operator(15)` |
| `':' '>'` | `:3610`; если след. `'='` → `:=`/`>=` | `Operator(15)` |
| `'<'` | `:3621`; если след. `'='` или `'>'` | `Operator(15)` |
| `'='` | `:3634`; если след. `'>'` или `':'` → `=>`/`=:` | `Operator(15)` |
| `'S'/'s'` | `:3705 IL_394`; если след. `'='` → `S=` | `Operator(15)` иначе `ScanIdentifierOrOperator` |
| `'R'/'r'` | `:3714 IL_3D0`; `R=`, `REF=` | `Operator(15)` иначе `ScanIdentifierOrOperator` |
| `'#' '&' ')' '+' ',' '-' ';' '[' ']' '^' '\|'` | `:3700 IL_2B9` | `Operator(15)` |
| `'{'` | `ScanPragma():3685` + `HandlePragmaToken` | `Pragma(4)` |
| `'!' '$' '\\' '0'..'9'`, буквы, `` '`' ``, прочее | `:3729 IL_51A` | `ScanIdentifierOrOperator` → `Identifier(13)`/`Operator(15)`/`Integer(14)`/`Real(16)` |
| непечатаемое/`'\'` | `ScanIdentifierOrOperator` | (см. §3 для weird-идентификаторов) |

Пост-обработка (`:3731 IL_522`): `token.Length = SourceOffset - token.SourceOffset`;
для `Comment(2)`/`DocComment(3)` → `HandleCommentToken(empty, out token)`;
для `Pragma(4)` → `HandlePragmaToken(empty, out token, out bPositionPragma)`.

`GetNext` (`:3476`) поверх — фильтр `Whitespace(19)` / `EndOfLine(12)` /
position-`Pragma(4)` (`!IncludeWhitespaces` и т.д.).

> **Закрыто (#4)**: таблица снята напрямую из декомпилированного `GetNextInternal`,
> а не из `docs/01`. Сверка с IL (`tools/dump_method_il.ps1`) больше не требуется
> для базовой формы; остаётся только порядок `case`-меток (на поведение не влияет —
> ветвление по значению символа).

### 5.4 Стратегия рекурсивного спуска

1. 1:1 порт классов-парсеров: `ScannerExt` (Next/MatchOperator/ParseReSync),
   затем `StatementParser`, `ExpressionParser`, `OperandParser`,
   `InfixOperationParser`, `FunctionCallParser`, `TypeParser`, `DeclarationParser`,
   `POUSyntaxParser`, прагмы.
2. Диспетчеризация через `match` по `TokenType`/`Operator` (в C# — `switch`,
   декомпилированный в goto-граф; в Rust восстанавливаем исходную форму).
3. Приоритеты выражений — точно как в `InfixOperationParser`
   (OR → AND → compare → ADD → MUL → unary → operand), левая ассоциативность.
4. Правоассоциативное присваивание — рекурсивный вызов `ParseAssignment`
   (`ExpressionParser.cs:123`).
5. Позиции: каждый узел получает `Span`; `PositionLength` считается от
   `SourceOffset` до и после разбора (как `StatementParser.ParseSTStatement:216`).
6. Ошибки/ресинхронизация — `Result<_, Diagnostics>`; при «локальной» ошибке
   вернуть частичный узел + флаг, вызывающий выполнит `resync(st_table)`.
7. Инварианты: `MaximumNestingDepthExceeded` при глубине > 2000
   (`OperandParser.cs:493`, `ParserContext` → MessageId 584).

### 5.5 MessageId → ошибки

`MessageId` — это **enum** (`_3S.CoDeSys.LanguageModelManager.InternalInterfaces.
MessageId`, файл `decompiled/Compiler/.../InternalInterfaces/MessageId.cs`), а не
произвольный `u32`. Значение = ordinal; тексты — в `ErrorMessages.resources`
(`decompiled/Compiler35220.plugin/.../Resources/ErrorMessages.resources`, 500 ключей
`Err_*`/`Wrn_*`/`Inf_*`/`Txt_*`). Полная цепочка:

```
parser.AddErrorST(node, MessageId.Err_IdentifierExpected /*26*/, args)
   → ErrorMessages.resources["Err_IdentifierExpected"] = "Identifier expected instead of '{0}'"
```

Для порта:

```rust
#[repr(u32)]
pub enum MessageId { None=0, Err_ConstantOverflow=1, Err_Operator1of2Expected=2, /* ...507 шт... */ }

pub fn add_error_st(node: &mut impl HasMessages, id: MessageId, args: Vec<Arg>);
pub fn add_error_st_with_token(node: &mut impl HasMessages, tok: &Token, id: MessageId, args: Vec<Arg>);
pub fn add_warning_st(node: &mut impl HasMessages, id: MessageId, args: Vec<Arg>);
```

**Все MessageId, реально используемые парсером `Parser35220` (34 шт), с текстами:**

| id | enum | текст (`ErrorMessages.resources`) | где |
|---|---|---|---|
| 1 | `Err_ConstantOverflow` | Constant '{0}' too large for type '{1}' | TypeParser |
| 2 | `Err_Operator1of2Expected` | '{0}' or '{1}' expected instead of '{2}' | MinMaxOperatorParser:163 |
| 3 | `Err_BitNrOverflow` | '{0}' is no valid bit number for '{1}' | сканер/литералы |
| 4 | `Err_NoComponentOf` | '{0}' is no component of '{1}' | OperandParser |
| 5 | `Err_OverflowInAddress` | Constant overflow in address '{0}' | direct-var |
| 6 | `Err_OperatorExpected` | '{0}' expected instead of '{1}' | MatchOperator/все |
| 7 | `Err_ExpressionExpectedInstead` | Expression expected instead of '{0}' | выражения |
| 8 | `Err_Operator1of3ExpectedInsteadofEOF` | Unexpected End-of-file found: '{0}', '{1}' or '{2}' expected | EOF-ветки |
| 9 | `Err_UnexpectedTokenFound` | Unexpected token '{0}' found | общий |
| 10 | `Err_OperatorExpectedInsteadofEOF` | Unexpected End-of-file found: '{0}' expected | EOF-ветки |
| 11 | `Err_NoCaseLabelFound` | No CASE label found | CASE |
| 22 | `Err_OpNeedsExactInputs` | '{0}' needs exactly '{1}' operands | операторы |
| 24 | `Err_IllegalOperator` | '{0}' is no valid ST operator | выражения |
| 26 | `Err_IdentifierExpected` | Identifier expected instead of '{0}' | структ. иниц. :178 |
| 30 | `Err_AddressExpected` | Direct address expected after AT instead of {0} | var-decl |
| 51 | `Err_AttributeNameExpected` | Single byte string expected for an attribute value instead of '{0}' | прагмы |
| 81 | `Err_UnexpectedPragmaif` | Unexpected pragma: '{0}' found without matching 'if' | прагмы |
| 85 | `Err_DefineValueExpected` | Define value expected instead of '{0}' | `{DEFINE}` |
| 98 | `Err_FunctionBlockNoLongerValid` | The keyword FUNCTIONBLOCK is no longer supported. Use FUNCTION_BLOCK instead. | POU |
| 114 | `Err_InvalidJumpDestination` | Invalid destination {0} for JMP | JumpParser |
| 115 | `Err_CalcNeedsCall` | Second parameter of conditional call must be a valid call statement | CALC |
| 182 | `Err_ReturnTypeForNonFunction` | Return type is only possible for POUs of type FUNCTION and METHOD | POU |
| 189 | `Err_SemicolonExpected` | ';' expected instead of '{0}' | ParseSemicolonIfNecessary:886 |
| 190 | `Err_SemicolonExpectedInsteadOfEnd` | ';' expected instead of end of POU | :886 |
| 248 | `Err_NewNeedsType` | Type definition expected as operand for __NEW | NewExpr:116 |
| 303 | `Err_StructureInitialisationNotPossible` | A structure initialisation is not possible as Parameter of an Init-function call… | NewExpr:238 |
| 304 | `Err_ArrayInitialisationNotPossible` | An array initialisation is not possible as parameter of an initial function call… | NewExpr:243 |
| 317 | `Err_LiteralExpected` | Literal expected instead of '{0}' | операнды |
| 372 | `Err_DuplicateElseInCaseStatement` | Duplicate definition of ELSE in CASE statement | CASE |
| 570 | `Err_ProjectDefinedNotSupportedFor` | The condition 'project_defined' is not supported for this syntax… | прагмы |
| 578 | `Err_UnexpectedStatement` | Unexpected statement | CodeStatementChecker:43, DeclarationStatementChecker:56 |
| 579 | `Err_UnsupportedFeature` | The compiler feature '{0}' is only supported with compiler version {1} or newer | ParserContext |
| 584 | `Err_MaxNestingDepthExceeded` | Maximum nesting depth exceeded. | CodeStatementChecker:31,57 |
| 588 | `Err_MissingImplementationTerminator` | Could not find the terminator '{0}' for the implementation block. | impl-block |

Полный enum — 507 членов (не только используемые парсером; компилятор берёт остальные).
Для 1:1 полного каталога достаточно `MessageId.cs` + `ErrorMessages.resources`
(оба в дампе). Автогенерация `message_ids.rs`/`.csv` возможна скриптом из этих двух файлов.

- Таблицы ресинхронизации портируются из `ResynchronizerTables.cs` как `const
  [Operator; 23]` / `[Operator; 12]`.

### 5.6 Тестирование совместимости

- Диф-тест сканера: одинаковый вход → сравнение `(TokenType, SourceOffset,
  Length, Position, line, column)`.
- Диф-тест AST: сериализация дерева узлов Rust ↔ дамп `_IStatement/_IExpression`
  из CODESYS (по позициям и типу узла).
- Граничные случаи из `docs/01_LEXER_PARSER.md §7.5`.

---

### 5.7 Опции сканера и динамические конверсии

**Опции сканера** — `_3S.CoDeSys.Compiler35220.PreCompile.ScannerOptionsService`
(`decompiled/Compiler35220.plugin/.../PreCompile/ScannerOptionsService.cs:15`):

```csharp
bUnicodeIdentifiers = false;                 // по умолчанию выключено
bSupportNonCompliantIdentifiers = true;      // по умолчанию включено
if (APEnvironmentFacade.Instance.InjectionCompleted) {
    bUnicodeIdentifiers = CompileOptions.UnicodeIdentifiers;
    if (OEMCustomization.HasValue("LanguageModelManager","NonCompliantIdentifiers"))
        bSupportNonCompliantIdentifiers = OEMCustomization.GetBoolValue(...);
    bSupportNonCompliantIdentifiers &= CompilerVersionToUseInternal() >= 3.5.18.0;
}
```

В Rust — `ScanOptions { unicode_identifiers: bool, non_compliant_identifiers: bool, ... }`
с этими дефолтами; источник — compile options + OEM-ключ.

**Динамические конверсии** — `OperatorTable.AddConversionOperators:708` и
`AddOverloadedConversions:751`:

1. Собирает `dataTypes` = все операторы с `Flags & (DataType | SafetyDataType)`;
   `numericTypes` = операторы с `Flags & DataType & NumericDataType`.
2. `AddOverloadedConversions`: для каждой пары `i≠j` из `dataTypes` создаёт
   `"<T>_TO_<U>"` и `"TO_<T>"` → `OperatorDesc(184, Internal|AllLanguages)`.
3. `GetTextOfOperator(186) + "_TO_" + GetTextOfOperator(92)` (операторы `Reference`=186,
   `Pointer`=92 из `tables/operators.csv`) → соответствующий ключ конверсии.
4. `"ANY_NUM_TO_<U>"` для каждого `numericTypes`.
5. `"ANY_TO_<U>"` для каждого `dataTypes`.
   `GetTextOfOperator(op, short=true)` — короткое имя типа (для литералов).

Все эти ключи указывают на `Operator.Conversion = 184`; парсер ST разбирает их в
`ConversionExpressionParser` → `_IConversionExpression`. Итоговое число динамических
записей: `|dataTypes|² + |dataTypes| + |numericTypes| + 1` (порядок вставки в TST
влияет только на перезапись ключей).

> **Закрыто (#5, #7)**: условия сканера и алгоритм конверсий сняты из
> декомпилированного кода.

## 6. Пробелы / открытые вопросы

Статус: **7/7 закрыто** (все — из уже декомпилированных исходников, без новой
декомпиляции сборок).

| # | Пробел | Как закрыт | Источник |
|---|---|---|---|
| 1 | `MessageId → текст` | номер = ordinal `MessageId`, текст = `ErrorMessages.resources`; 34 используемых кода выписаны с текстами | `…/InternalInterfaces/MessageId.cs`, `Compiler35220.plugin/…/Resources/ErrorMessages.resources` (§5.5) |
| 2 | `_ILanguageModelBuilder6/7/8` | найдены интерфейсы и ~320 `CreateX`; «уровни» — маркеры наследования | `…/InternalInterfaces/_ILanguageModelBuilder*.cs`, `…/Core/LanguageModel/ILanguageModelBuilder*.cs` (§1.2) |
| 3 | Fluent red-tree цепочки | реализации в `LanguageModelManager.plugin/.../RedTrees/Builder/` (12 классов), цепочки в CSV | §1.1 |
| 4 | `GetNextInternal` | точная `switch(char)` снята из декомпила | `Scanner/InternalScanner.cs:3488-3745` (§5.3) |
| 5 | `ScannerOptionsService` | алгоритм опций снят (дефолты + OEM + версия ≥3.5.18) | `Compiler35220.plugin/.../PreCompile/ScannerOptionsService.cs:15` (§5.7) |
| 6 | Checkers | семантика, флаги и MessageId (578/584) зафиксированы | `Statements/CodeStatementChecker.cs`, `DeclarationStatementChecker.cs` (§2.5) |
| 7 | Dynamic conversions | алгоритм `AddConversionOperators`/`AddOverloadedConversions` | `Scanner/OperatorTable.cs:708/751` (§5.7) |

Остаточные (не «пробелы анализа», а задачи порта):
- автогенерация `message_ids.rs`/`.csv` и `operator.rs`/`token.rs` из дампов;
- решение по квази-багу `IfBuilder.Build()` (`_IfElse = _then`) — воспроизводить
  как в парсере (`_IfElse = seq`), подтвердить дифф-тестом;
- `ITypeTable3`/`ILMCompileOptions3` остаются внешними контрактами типизации
  (не входят в парсер; для лексера/парсера 1:1 не нужны).
