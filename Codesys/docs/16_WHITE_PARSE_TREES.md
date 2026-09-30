# 16. White Parse Trees (CST) CODESYS ST

> Источник: декомпиляция `WhiteParseTrees.dll` (контракт) и `WhiteParsetrees.plugin.dll` (реализация)
> из CODESYS 3.5.22.10. Инструмент: `McpFree\tools\decompiler\DecompileHost.exe`.
> Артефакты: `decompiled\WhiteParseTrees\`, `decompiled\WhiteParsetrees.plugin\`,
> таблица `tables\white_tree_nodes.csv`.
> Связанные документы: `04_AST_MODEL.md` (green/red-tree), `07_AST_RED_TREE_CONSTRUCTION.md`
> (green→red), `01_LEXER_PARSER.md` (лексер `IScanner`), `06_AST_BUILDER_MAP.md` (builder'ы AST).

## 0. TL;DR

**White parse tree (CST) — независимый, самодостаточный сервис редактора CODESYS,
который строит дерево разбора с ПОЛНЫМ сохранением форматирования (пробелы, комментарии,
переносы строк) и умеет реконструировать исходный текст 1:1 (round-trip).**

- Он **не участвует** в компиляции/построении AST: green→red конвейер идёт через
  `Parser35220` + `Compiler35220.TreeConversion.RedTreeBuilder` (см. `07`).
- Он использует **тот же лексер**, что и green-tree (`_3S.CoDeSys.Core.LanguageModel.IScanner7`),
  но **собственный** рекурсивный парсер (`CODESYS.WhiteParseTrees.Parser.WhiteTreeParser`).
- Для порта **ST-парсера/AST/лексера на Rust WhiteParseTrees не обязателен**. Он нужен только
  если требуется бинарно-совместимо воспроизвести сервисы редактора: форматирование,
  сохранение форматирования при правках, отображение позиции каретки ↔ смещение в тексте.

## 1. Сборки и карта

| Сборка | Путь | Типов | Роль |
|---|---|---|---|
| `WhiteParseTrees.dll` | `CODESYS\Common\` | **583** | Контракт: интерфейсы узлов (`IWhite*`, `I*Token`), перечисление `WhiteTokenType`, интерфейсы посетителей/фабрик/сервисов |
| `WhiteParsetrees.plugin.dll` | `PlugIns\977d3b32-.../3.5.22.10\` | **643** | Реализация: узлы (Nodes), парсер, фабрики, билдеры, сервисы, форматтер |

Ссылки на сборки (dnlib):

- `WhiteParseTrees.dll` → `mscorlib 2.0.0.0`, `ComponentModel`, `Compiler`.
- `WhiteParsetrees.plugin.dll` → `mscorlib 4.0.0.0`, `ComponentModel`, `System`,
  `System.Core`, `Compiler`, **`WhiteParseTrees`**.

`WhiteParseTrees.dll` — «тонкая» контрактная сборка (только `[ReleasedInterface]`-типы, тип `enum`),
реализация целиком вынесена в plugin. Единственный внешний контракт-владелец —
`_3S.CoDeSys.Core.LanguageModel` (`Compiler.dll`): лексер `IScanner7`, `Operator`,
`ISourcePosition`, `IExpression`.

## 2. Пространства имён

`WhiteParseTrees.dll` (контракт):

- `CODESYS.WhiteParseTrees` — узлы и сервисные интерфейсы (≈130 `IWhite*` + 350 `I*Token`).
- `CODESYS.WhiteParseTrees.Factories` — интерфейсы фабрик и fluent-шагов билдеров (77 файлов).
- `System.Runtime.CompilerServices` — nullable-атрибуты; `Microsoft.CodeAnalysis` — атрибуты анализа.

`WhiteParsetrees.plugin.dll` (реализация):

| Пространство | Содержимое | Кол-во *.cs |
|---|---|---|
| `...WhiteParseTrees.Nodes.Statements` | классы операторов (`WhiteIfStatement`, ...) | 54 реальных класса |
| `...WhiteParseTrees.Nodes.Expressions` | классы выражений (`WhiteBinaryOperatorExpression`, ...) | 54 реальных класса |
| `...WhiteParseTrees.Nodes.Tokens` | классы лексем (`IdentifierToken`, `AddToken`, ...) | 310 |
| `...WhiteParseTrees.Nodes.Factories` | `BuilderFactory`, `ExpressionFactory`, `StatementFactory`, `TokenFactory<T>`, `OperatorTokenFactory`, `TypeTable` | 21 |
| `...WhiteParseTrees.Nodes.Factories.Builder` | fluent-билдеры (`IfBuilder`, `ForBuilder`, `CaseBuilder`, ...) | 16 |
| `...WhiteParseTrees.Parser` | `WhiteTreeParser`, `TokenStream`, `TokenControl`, `SourcePositionMap`, `WhiteTreeInformation`, `WhiteParserMessages` | 16 |
| `...WhiteParseTrees.Services` | `ParserService`, `ExpressionDeterminationService`, `TokenSerializer`, `StringConverter`, `TextLengthCalculator`, курсоры | 53 |
| `...WhiteParseTrees.Services.Formatter[.Passes]` | `WhiteParseTreeFormatter`, `FormatterPipeline`, `WhiteSpaceRemover/Formatter`, `StmtExprFormatterVisitor`, `InlineCommentFixer` | 53 |

Полная карта узлов — `tables\white_tree_nodes.csv` (колонки `node,kind,base,key_fields,factory/file:line,purpose`).

## 3. Модель узлов

### 3.1 Ядро

```
INode                         GetChildren()                                 INode.cs:9
└─ IWhiteExprement            GetTextLength(), GetTextLengthNetto()          IWhiteExprement.cs:6
   ├─ IWhiteStatement : IStatementSyntax        (посетитель операторов)       IWhiteStatement.cs:6
   │  └─ IWhitePOUSyntax → IWhitePOU (BeforeDeclarationStatements,
   │       DeclarationStatement, Implementation, SubPOUs, EndPOU)              IWhitePOU.cs:9
   └─ IWhiteExpression : IExpressionSyntax      (посетитель выражений)        IWhiteExpression.cs:6

IWhiteToken : INode           Leading, WhiteTokenType Type, Text             IWhiteToken.cs:8
└─ INonSyntacticToken         + Trailing                                      INonSyntacticToken.cs:8
   ├─ IWhitespaceToken
   ├─ ICommentToken           Comment, IsBlockComment
   ├─ IEndOfLineToken
   └─ (IPragmaToken)
```

- Имена намеренно **похожи** на green-AST (`_IExprement`): интерфейс называется
  `IWhiteExprement` (опечатка скопирована из CODESYS), поэтому не путать с `_3S.CoDeSys.Core.LanguageModel._IExprement`.
- White-узлы имеют **собственные** интерфейсы посетителей `IStatementSyntax`/`IExpressionSyntax`
  (внутри контрактной сборки, версии `IStatementSyntax2`, `IExpressionSyntax2/3`), а не green-посетителей.

### 3.2 Хранение форматирования (ключевая идея)

Форматирование хранится **не в узле**, а в цепочке токенов через ссылки `Leading`/`Trailing`:

- `IWhiteToken.Leading` — предыдущий **несинтаксический** токен (пробелы/комментарий/EOL), `IWhiteToken.cs`.
- `INonSyntacticToken.Trailing` — следующий токен; связывание в обе стороны.

При разборе потока (`TokenStream.ReadTokenStream`, `TokenStream.cs:14`) каждый новый синтаксический
токен получает `Leading = предыдущий non-syntactic`, а у того выставляется `Trailing`. Поэтому
исходный текст восстанавливается простым обходом (`TokenSerializer.GetTokenList` → `StringConverter.ConvertToString`),
а узлы-операторы (`WhiteIfStatement`) содержат **только** синтаксические токены (`IF`, `THEN`, `END_IF`)
и дочерние выражения.

### 3.3 Классовая иерархия (реализация)

- `Nodes.Tokens.WhiteToken` (абстракт) → `NonSyntacticToken` (`WhitespaceToken`, `CommentToken`,
  `DocCommentToken`, `EndOfLineToken`, `WhitePragmaToken`, `ErrorToken`) и `WhiteOperatorToken`.
  Остальные 300 классов — по одному на `WhiteTokenType` (`IdentifierToken`, `IntegerToken`, `AddToken`, `SafeIntToken`, `__XIntToken`, ...).
- `Nodes.Statements.WhiteExprement` (абстракт) → `WhiteStatement` → конкретные операторы;
  `WhiteSequenceStatement` — список `IList<IWhiteStatement>` (тело POU/ветки).
- `Nodes.Expressions.WhiteExpression` (абстракт) → конкретные выражения.
- POU-контейнеры: `WhiteProgram`, `WhiteFunction`, `WhiteFunctionBlock`, `WhiteMethod`,
  `WhiteInterface`, `WhiteAction`, `WhiteTransition`, `WhiteDUT` (база `WhitePouHavingSubPous`).

### 3.4 Позиции

- `IWhiteTreeInformation` = `RootNode` + `ISourcePositionMap` (`IWhiteTreeInformation.cs:8`).
- `SourcePositionMap` (`Parser\SourcePositionMap.cs`): `SortedList<int textOffset, long editorPosition>` +
  `Dictionary<long, int>`; методы `GetPositionOffsetForTextOffset` / `GetTextOffsetForPositionOffset`.
- `WhiteTreeParser.ParseStImplementationWithSourcePositions` (`WhiteTreeParser.cs:275`) читает поток с
  картой позиций; `ParseStImplementation` (`:281`) — без неё.

## 4. Парсер и лексер

Точки входа `WhiteTreeParser` (все — статические):

| Метод | Строка | Возврат |
|---|---|---|
| `ParseStImplementation` | `WhiteTreeParser.cs:281` | `IWhiteSequenceStatement` (реализация ST) |
| `ParseStImplementationWithSourcePositions` | `:275` | `IWhiteTreeInformation` |
| `ParseStatement` | `:286` | `IWhiteSequenceStatement` |
| `ParseExpression` | `:291` | `IWhiteExpression` |
| `ParsePOUSyntax` | `:296` | `IWhitePOUSyntax[]` |

Лексер: `TokenStream.TryCreateScannerForText` (`TokenStream.cs:75`) вызывает
`APEnvironment.LMServiceProvider.CreatorService.CreateScanner(...)` с флагами
`bIncludeComments/bIncludeEndOfLines/bIncludePragmas/bIncludeWhitespaces = true` и приводит к
**`IScanner7`** (тот же интерфейс, что использует green-конвейер `Parser35220`). Версия ниже
3.5.16 → `TooOldCompilerversionException`.

Парсер — рукописный рекурсивный спуск по потоку токенов (`TokenControl.Next/LookAhead1`),
2501 строка, с восстановлением после ошибок (`bResynchroniseOnNullStatement: true`,
`WhiteErrorStatement`, `ResynchroniseException`). Обратите внимание: **это второй, независимый
парсер ST** — грамматика и приоритеты операторов дублируют green-парсер, но дерево другое (CST с форматированием).

## 5. Сервис, фабрики и билдеры

- Сервис: `ParserService` (`Services\ParserService.cs:17`) реализует `IWhiteParserService3`
  и помечен `[TypeGuid("f8e734f5-2639-4aeb-bde2-f54049a15e78")]` (`:16`) — так он публикуется в
  сервисной шине CODESYS. Он делегирует в `WhiteTreeParser` / `ExpressionDeterminationService` /
  `TokenSerializer` / фабрики.
- Фабрика билдеров (упоминалась ранее): `CODESYS.WhiteParseTrees.Nodes.Factories.BuilderFactory`
  (`Nodes\Factories\BuilderFactory.cs:10`) реализует `IWhiteParseTreeBuilderFactory3/2/1` и создаёт
  fluent-билдеры: `CreateIfBuilder()` (`:87`), `CreateForBuilder`, `CreateCaseBuilder`,
  `CreateProgramBuilder()`, `CreateFunctionBuilder()`, `CreateVariableDeclarationBuilder()` и т.д.
- Fluent-билдер — пошаговый конструктор CST без текста:
  `IfBuilder.WithCondition(...).WithThenStatement(...)...Build()` (`Nodes\Factories\Builder\IfBuilder.cs`);
  незаданное поле → `BuilderException`. Токены-ключевые слова создаются `TokenFactory<IIfToken>.Create("IF")`.
- Прочие фабрики: `ExpressionFactory` (`IWhiteParseTreeExpressionFactory/2`),
  `StatementFactory` (`IWhiteParseTreeStatementFactory3/2/1`), утилитарные `TokenFactory<T>` и
  `OperatorTokenFactory` (по `_3S.CoDeSys.Core.LanguageModel.Operator`).
- Форматтер: `WhiteParseTreeFormatter` → `FormatterPipeline.Format` (`Services\Formatter\FormatterPipeline.cs:11`)
  либо `WhiteSpaceRemover.RemoveWhiteSpaces`; всё конфигурируется `IFormatterSettings`.

## 6. Связь с green/red

```
Green tree (raw AST)          Red tree (typed AST)             White tree (CST, редактор)
_IExprement                   _IStatement/_IExpression         IWhiteStatement/IWhiteExpression
src: Parser35220              src: RedTreeBuilder (green→red)  src: WhiteTreeParser
lexer: IScanner7 ◄─────────── общий лексер ───────────────────► IScanner7 (общий!)
```

- **Общее звено — лексер `IScanner7`.** White и green читают один и тот же поток токенов
  (`_3S.CoDeSys.Core.LanguageModel`), но парсят его разными парсерами.
- White **не является** промежуточным звеном green→red. `Compiler35220.TreeConversion.RedTreeBuilder`
  работает с green (`_IExprementVisitor*`), а не с white (проверено: ни один из декомпилированных
  `Compiler*`, `Parser*`, `LanguageModelManager*` не ссылается на `CODESYS.WhiteParseTrees.*`).
- Единственная явная связь white↔red в API — сопоставление выражений:
  `ExpressionDeterminationService.FindMatchingWhiteExpressionForRedExpression(IWhiteTreeInformation, IExpression)`
  (`Services\ExpressionDeterminationService.cs:67`), где `IExpression` — red-выражение. Используется
  редактором, чтобы по red-узлу выделить соответствующий текст в CST.
- Потребитель white-tree — **не компилятор, а слой IDE/редактора** (в проанализированном наборе
  сборок потребителей нет; сервис адресуется по `TypeGuid` через сервисную шину CODESYS).

## 7. Назначение white tree

1. **Round-trip форматирование**: сохранение и восстановление исходного текста 1:1 — `StringConverter.ConvertToString`,
   `TextLengthCalculator.CalculateTextLength/WithoutLeadingWhitespace`.
2. **Хранение комментариев и пробелов**: `CommentToken`/`DocCommentToken`/`WhitespaceToken`/`EndOfLineToken`/`WhitePragmaToken`
   как non-syntactic токены в цепочке `Leading`/`Trailing`.
3. **Форматирование/рефакторинг**: `WhiteParseTreeFormatter` (`RemoveAllWhitespaces`, `FormatStatements`),
   проходы `WhiteSpaceRemover`, `WhiteSpaceFormatter`, `InlineCommentFixer`, `StmtExprFormatterVisitor`.
4. **Карта позиций для редактора**: `ISourcePositionMap` ↔ символьные offset'ы, поиск выражения под кареткой
   (`FindExpressionAtTextOffset`, `FindExpressionAtSourcePosition`).
5. **Программное построение ST** без текста: fluent-билдеры (`BuilderFactory`) — для инструментов/плагинов.
6. **Ошибки парсера** с токенами: `WhiteErrorStatement`/`WhiteErrorPOU`, `CollectErrorStatements`.

## 8. Нужен ли WhiteParseTrees для порта AST/лексера? (вывод)

**Для 100% идентичного ST-парсера, лексера и AST — НЕТ.** White tree не участвует в
компиляции и не является источником red-tree: AST строится green→red (`Parser35220` +
`Compiler35220.TreeConversion.RedTreeBuilder`, см. `07`). Портируйте `IScanner7`-лексер и
green/red-модель; white-дерево можно игнорировать.

**Нужен ТОЛЬКО при выполнении хотя бы одного условия:**

1. Rust-порт обязан воспроизводить **сервисы редактора**: сохранение форматирования, переносы строк,
   комментарии, форматтер (аналог `WhiteParseTreeFormatter`).
2. Требуется **round-trip**: «текст → CST → текст» без потерь (в т.ч. нестандартные отступы/комментарии).
3. Требуется **карта позиций редактора** (`ISourcePositionMap`) и поиск выражения по offset/каретке.
4. Нужен **fluent builder API** white-tree для внешних плагинов/совместимости с `IWhiteParserService3`.
5. Нужна **вторая, независимая проверка парсинга** (дифф green vs white) как тест-оракул для Rust-парсера.

Если ничего из этого не требуется — достаточно green/red; white-tree стоит использовать
лишь как **дополнительный источник грамматических правил** (приоритеты операторов, вид `WhiteTokenType`,
список токенов), т.к. грамматика white совпадает с ST.

**Осторожно:** не путать `_3S.CoDeSys.Core.LanguageModel._IExprement` (green) и
`CODESYS.WhiteParseTrees.IWhiteExprement` (white/CST) — это разные типы в разных сборках.

## 9. Пробелы и неясности

- Потребитель сервиса (код IDE/редактора) отсутствует в проанализированных сборках —
  подтвердить владельца `[TypeGuid("f8e734f5-...")]` можно только в бинарниках UI-плагинов.
- `WhiteParserMessages` (сообщения парсера) не сопоставлены с `MessageId` из `tables\errors\`.
- Часть `Nodes\*.cs` — компиляторно-генерированные (`_GetChildren_d__*`, `__c*`, `DisplayClass*`);
  они не являются отдельными типами модели (в счётчиках реальных классов исключены: 54 stmt + 54 expr + 310 токенов).
- Точное соответствие `WhiteTokenType` ↔ `_3S.CoDeSys.Core.LanguageModel.TokenType` не проверено
  (сопоставление строится в `Nodes\Factories\TokenFactory.cs` / `OperatorTokenFactory.cs`).
- Формат `SourcePositionMap` (`long position, short offset`) зависит от версии редактора —
  при порте карты позиций сверить с целевой версией CODESYS.

## 10. Быстрые ссылки

| Что | Путь |
|---|---|
| Контракт, сервис | `decompiled\WhiteParseTrees\CODESYS\WhiteParseTrees\IWhiteParserService*.cs` |
| Модель узлов | `decompiled\WhiteParseTrees\CODESYS\WhiteParseTrees\IWhite{Statement,Expression,POU,Token}.cs` |
| Парсер | `decompiled\WhiteParsetrees.plugin\CODESYS\WhiteParseTrees\Parser\WhiteTreeParser.cs` |
| Лексер-мост | `...\Parser\TokenStream.cs:75` (`IScanner7`) |
| Фабрика/билдеры | `...\Nodes\Factories\BuilderFactory.cs`, `...\Nodes\Factories\Builder\` |
| Форматтер | `...\Services\Formatter\` |
| Таблица узлов | `tables\white_tree_nodes.csv` |
