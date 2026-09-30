# 14. Полный порядок фаз компиляции/типизации CODESYS 3.5.22.20 и роль TypeAcceptor / GenericTypeReplacer

Документ дополняет `docs\02_PRECOMPILE_PIPELINE.md`. Здесь — **упорядоченная** цепочка
фаз от текста до машинного кода и точки построения DECLARATION-узлов и резолвинга имён.
Проверено по декомпилу `C:\Codesys\decompiled\...` и исходникам `C:\Codesys\Parser35220.plugin\...`.

> **Обозначение путей.** Обфусцированные (SmartAssembly) типы компилятора лежат в корне
> `decompiled\Compiler35220.plugin\` в папке `-` (файлы вида `-.143.cs`). Ниже такие пути
> сокращены как `plugin\-.143.cs:LINE`. Логическое имя класса указано рядом (например
> `\u0017.\u000E` = построитель типизированного red-tree / precompile-typifier).
>
> Пространства: `CompilerPhases`, `Phase1_Typification`, `Phase2_AfterTypification`,
> `Phase3_Location`, `Phase4_TypeCheck`, `Phase5_Codegeneration`, `PreCompile`,
> `PreCompile\Typification`, `Scopes`, `Services`, `TreeConversion`.

---

## 0. Две ветки исполнения: precompile vs compile

| | Precompile (фон, «только синтаксис/типы») | Full compile (GenerateCode) |
|---|---|---|
| Триггер | `ILMPreCompileCheckerService.FinishPrecompileChecks` | `CompilerProxy.GenerateCode` → `CompilerPhaseControllerGenerateCode` |
| Исполнитель | `PrecompileChecksWindows` (UI) / `PrecompileChecksNone` (NoUI) | `CompilerPhaseControllerCompile`, `…GenerateCode` |
| Контекст | `_IPreCompileContext` (`PreCompileContext`) | `_ICompileContext` (`ComconNew`, класс `\u000E.\u001B`) |
| Резолвинг | `CheckerScope` + `SymbolTable` (read-only из precompile-контекстов) | `CheckerScope` + `SymbolTable` + запись результатов типизации в `_ICompiledPOU` |
| Категория сообщений | `PreCompileMessageCategory` | `CompilerMessageCategory` |
| Типизация | `SimpleTypeChecker` / `SimpleTypeInferrer` / `PreCompileTypifier` | `CompilerPhase1_Typifier` … `CompilerPhase5_Codegenerator` |

---

## (a) Упорядоченный список фаз

### Слой A. Разбор исходника объекта (lazy, на каждый POU/GVL/DUT)

| # | Фаза | Вход | Что делает | Результат | Классы / методы (`file:line`) |
|---|---|---|---|---|---|
| A1 | **Scanner / lexer** | текст ST | создаёт `InternalScanner` (`IScanner9`) с опциями (`IgnoreCase=true`, `AllowMultipleUnderlines=false`, `AllowNestedComments` из LMM, include-comments/pragmas/whitespaces) | поток `IToken` | `PreCompile\Scanner.cs:15` `CreateInternalScanner`; `:27` `CreateMultiStringScanner`; `ScannerOptionsService`; `LanguageServices.ScannerService` |
| A2 | **Parser** | токены | `InternalParser.ParseST` строит `_ISequenceStatement`; параллельно парсеры деклараций строят DECLARATION-узлы (см. §c) | raw red-tree (`_IStatement`) | `Parser35220.plugin\...\InternalParser.cs:177` `ParseST`, `:303` `ParseSTStatement`; `ParserService.cs:30` `CreateParser`; вход — `CompiledPOU.cs:830` `CreateParser(m_stCode).ParseST` |
| A3 | **MacroReplacement** | red-tree | подстановка макро-операторов через `MacroInfoProvider` (интерфейс + тело POU) | изменённый red-tree | `PreCompile\MacroReplacement.cs:11`,`:18` |
| A4 | **Green/RawTree conversion** | red-tree | при precompile-POU: чек-сумма + конвертация в green-tree | `Checksum`, green-tree | `PreCompileContext.cs:2274`–`:2278` (`ConvertParseTreeToGreenTree`) |
| A5 | **InterfaceParser** | текст интерфейса | парсит интерфейс (`ParseInterfaceStatement`) в `_ISignature` + `PragmaVisitor` | `_ISignature` | `PreCompile\InterfaceParser.cs:63` (→ `:81`), `ParserHelper.cs` |

### Слой B. Precompile-проверки (без ComconNew, «только синтаксис»)

| # | Фаза | Вход | Что делает | Результат | Классы / методы (`file:line`) |
|---|---|---|---|---|---|
| B0 | **Checker-thread** | precompile-контекст | воркер-потоки берут `LanguageModelResult` из очереди, зовут `CheckSignature`/`CheckPOUCode`, `SetSignatureChecked/SetPouChecked` | `_ICompilerMessage[]` | `PreCompile\PrecompileChecksWindows.cs:280` `FinishPrecompileChecks`, `:492` воркер, `:563` `AddRecentLMResult`; `PrecompileChecksNone.cs:20` (NoUI-заглушка) |
| B1 | **Precompile checker** | `_ISignature`/`_ICompiledPOU` | `SimpleTypeChecker` обходит дерево, резолвит имена через `CheckerScope`, копит сообщения | сообщения | `PreCompileContext.cs:2124` `CheckSignature`, `:2151` `CheckPOUCode`; `Services\Helper.cs:475` `CreatePrecompileChecker` → `SimpleTypeChecker.cs:39` |
| B2 | **PreCompileTypifier** | `_ICompiledPOU` | строит **типизированное** red-tree (TypesOnly / AddExplicitConversions) | `IStatement` | `PreCompile\Typification\PreCompileTypifier.cs:69` `CreateTypifiedParseTree`, `:114` `TypifyStatement` |
| B3 | **Typed red-tree builder / generic instantiation** | POU | visitor `\u0017.\u000E` типизирует выражения, инстанцирует generic-типы через `GenericTypeReplacer` | `TypifiedRedParseTree` | `plugin\-.143.cs:27`,`:97`, generic — `:1963`–`:1999`; `GenericTypeReplacer.cs` (§b) |

### Слой C. Полная компиляция (Compile → Location → Codegen)

Точка сборки конвейера: `plugin\-.399.cs:14`–`:45` (`\u001E.\u001A.\u0001` создаёт все фазы),
контейнер состояния — `plugin\-.398.cs:187`–`:217` (класс `\u000E.\u001B`, поля `CompilerPhase1_Typifier`
… `CompilerPhase6_AfterCodegeneration`, `CompilerPhaseControllerCompile`).

Контроллер `GenerateCode` (`CompilerPhaseControllerGenerateCode.cs`), порядок вызовов:

```
\000F()  :218
 ├─ \0001(out flag,out flag2)          :227   setup: ComconNew, UpToDate, OnlineChange
 ├─ \0001(flag,flag2)                  :228   FastOnlineChange
 ├─ \0002(flag2)  "Compile_Phase"      :234 → :510  → CompilerPhaseControllerCompile.\0001
 ├─ \0012()       "Location"           :246 → :527  → CompilerPhase3_Locator.\0001()
 └─ \0002()       "Codegeneration"     :258 → :563  → CompilerPhase5_Codegenerator + Phase6
```

| # | Фаза | Вход | Что делает | Результат | Классы / методы (`file:line`) |
|---|---|---|---|---|---|
| C0 | **Compile controller** | `ComconNew` | BeforeCompile, создаёт compile-context, определяет AddressSize/ByteOrder/Platform defs, запускает Phase1 | `_ICompileContext` | `plugin\-.404.cs:270` `\u0001(bool,bool,bool)`, `:213` статический вход; `CompilerPhaseControllerGenerateCode.cs:510` `\u0002` |
| C1 | **Phase1 — Typifier** | red-tree POUs + signatures | `NameManglingService`, `CompiledSymbolTables`, интерфейсная компиляция (`InterfaceCompiler`), late language models, `Locator` по сигнатурам, детект объектов к типизации; потом `POUTypifier` типизирует тела | `CompactedTypifiedParseTreeInformation`, флаг `Typified` | `CompilerPhases\CompilerPhase1_Typifier.cs:119`/`:134`; `:159`,`:179` (interface compile); `:819` internal `\u0001.\u0001`; `POUTypifier` `:416`,`:638`; `ObjectsToTypifyDetector.cs:53`; `NameManglingService.cs:43` |
| C2 | **Phase2 — AfterTypification** | `ComconNew` | де-типификация/чистка, детект ambiguity GVL/enum, рекурсия вызовов, task-references, `HASANYTYPE` | модифицированный контекст | `CompilerPhases\CompilerPhase2_AfterTypification.cs:65` `\u0001(bool)`; вызывается из Phase1 `:122` |
| C3 | **Phase3 — Locator** | `ComconNew` | конфигурация памяти, `Locator.\u0001`, поздние языковые модели, vftables, детект изменений (Interfaces/Code/InitValues), адресация | адреса (`DataLocation`) | `CompilerPhases\CompilerPhase3_Locator.cs:63` `\u0001()`; вызывается `CompilerPhaseControllerGenerateCode.cs:527`; `:246` `CreateVirtualFunctionTable`, `:262` детект изменений |
| C4 | **Phase4 — Typechecker** | типизированные POU + `CompactedTypifiedParseTreeInformation` | многопоточная (по `Environment.ProcessorCount`) полная проверка типов, `ConstantFolder`, `TypeCheckerVisitor`, `ErrorVisitor;` устанавливает `ILMCompiledParseTreeService` | ошибки/предупреждения | `CompilerPhases\CompilerPhase4_Typechecker.cs:131` `\u0003(bool)`, `:171` `\u0004(bool)`, `:88` factory; внутренний воркер `:256`,`:383` `TypeCheckerVisitor` |
| C5 | **Phase5 — Codegenerator** | POU + codegenerator-плагин | генерация кода (многопоточно), special POUs, global init/exit, CodeInit, relocations, проверка stack, распределение памяти | `CompiledCode` (адреса) | `CompilerPhases\CompilerPhase5_Codegenerator.cs:192` `\u0001(IList<ICompiledPOU4>)`, `:438` `\u0001(ICodegenerator)`, `:330` создать codegenerator |
| C6 | **Phase6 — AfterCodegeneration** | `ComconNew`, codegenerator | финализация/чек-суммы (FastOnlineChange использует `\u0003`,`\u0004`) | финальный контекст | `plugin\-.399.cs:28` (`\u0002.\u0014`); вызов `CompilerPhaseControllerGenerateCode.cs:418`,`:424`,`:568` |

Порядок фаз задан фабрикой явно: `plugin\-.399.cs:23`–`:28`
(`CompilerPhase1_Typifier` → `CompilerPhase2_AfterTypification` → `CompilerPhase3_Locator` →
`CompilerPhase4_Typechecker=null` (создаётся в C1) → `CompilerPhase5_Codegenerator` →
`CompilerPhase6_AfterCodegeneration`).

---

## (b) Роль `TypeAcceptor` и `GenericTypeReplacer`

### `TypeAcceptor<T>` — механизм двойной диспетчеризации типов

`TypeAcceptor` (static, generic) — это **не часть pipeline**, а инфраструктура обхода
иерархии `_IType`. Он принимает `_IType` и `ITypeVisitorX<T>`, делает цепочку `is`-проверок
по конкретным типам и вызывает соответствующий `visit(...)`; при отсутствии совпадения
возвращает `default(T)`.

- Файл: `PreCompile\Typification\TypeAcceptor.cs:7` (класс), `:10` `Accept(_IType, ITypeVisitorX<T>)`.
- Порядок ветвлений (важен!): `_IAliasType` → `IGenericUserdefType` → `_IXStringType` →
  `_IBitConstType` → `_IBitType` → … → `_IXLWordType` (`:12`–`:302`).
- Интерфейс посетителя: `PreCompile\Typification\ITypeVisitorX.cs:7`
  (`T visit(_IBitConstType)`, …, `T visit(IGenericUserdefType)`), 60 перегрузок.

### `GenericTypeReplacer` — посетитель, сворачивающий параметры generic-типов

`GenericTypeReplacer : ITypeVisitorX<_IType>` — **один конкретный** посетитель, применяемый
через `TypeAcceptor`. Назначение: заменить в типе выражения-параметры (границы `ARRAY`,
длины строк, `ARRAY[..] OF <generic>`, generic-константы) на **вычисленные литералы**.

- Файл: `PreCompile\Typification\GenericTypeReplacer.cs:12`.
- Вход/фасад: `\u0001(_IType type, IConstantFolder3 folder, Scope scope)` `:30`,
  внутри — `TypeAcceptor<_IType>.Accept(type, visitor)` `:33`.
- Зависимости: `IConstantFolder3` (`GetLiteralValue`, `:53`) + `\u0017.\u0006 Scope`;
  пересоздаёт типы через LM-builder-facade `global::\u0019.\u0003` (`plugin\-.71.cs:14`
  `class \u0003`, `:18` `_ILanguageModelBuilder7 Builder`).
- **Что именно заменяет** (остальные `visit` — identity, возвращают тип как есть):
  - `_ISubrangeType` `:37` — нижняя/верхняя границы → литералы;
  - `_IArrayType` `:62` — все измерения (`_Dimensions`);
  - `_IVectorType` `:84` — размерность;
  - `IGenericUserdefType` `:95` — `NameExpression` + `GenericConstantsInitializations`
    (свёртка констант), сохраняет `SignatureId`;
  - `_IXStringType`/`_IStringType`/`_IWStringType` `:112`/`:125`/`:138` — длина строки.
- Порядок применения: сначала подстановка значений generic-констант
  (`Dictionary<string,_IExpression>`, см. ниже), затем `TypeAcceptor` выбирает ветку, затем
  `GenericTypeReplacer` перестраивает тип. Один проход по типу, без рекурсии в `_Base`
  (но `_IArrayType._Base`/`_ISubrangeType._Base` переносятся как есть).

### Связь с `ILanguageModelBuilder`

- И `TypeAcceptor`, и `GenericTypeReplacer` **не зависят** от `ILanguageModelBuilder` напрямую;
  типы для пересборки создаёт facade `\u0019.\u0003` (статическая обёртка над
  `_ILanguageModelBuilder7 = LanguageModelBuilder`), напр. `\u0019.\u0003.\u0001(expr,expr)`.
- Generic-инстанцирование вызывается из построителя типизированного дерева `\u0017.\u000E`
  (`plugin\-.143.cs`): `\u0002\u0001(IVariableExpression)` `:1963` → `\u0001(ICompiledType, IGenericUserdefType)`
  `:1975`, где создаётся `CheckerScope` `:1977`, словарь generic-констант
  `Dictionary<string,_IExpression>` `:1978`–`:1992` и вызов
  `GenericTypeReplacer.\u0001(type, new ConstantFolder(), scope)` `:1998`–`:1999`.
- `\u0017.\u000E` вызывается из `PreCompileTypifier.CreateTypifiedParseTree` `:76`/`:114`
  (`TypifiedRedParseTree`), т.е. **generic-инстанцирование происходит в precompile-типификации**
  (B2/B3), и повторно переиспользуется полной компиляцией.
- `GenericTypeChecker` (`Phase1_Typification\GenericTypeChecker.cs:14`) — отдельная проверка
  корректности generic-параметров (`Err_GenericParamsAllExplicitOrNone`, `:34`).

Разбор `ConversionOptions` (влияет на типизированное дерево):
`Compiler\_3S\CoDeSys\Core\LanguageModel\ConversionOptions.cs:5`
(`TypesOnly`, `AddExplicitConversions`, `AddExplicitConversionsAndExplicitReferences`).

---

## (c) Где строятся DECLARATION-узлы и где резолвятся имена/типы

### c.1. Построение DECLARATION-узлов (POU / VAR / TYPE / ENUM)

Строятся **парсером** (`Parser35220.plugin`) через фабрику `LMItemFactory`
(= `_ILanguageModelBuilder7`, реализация `LanguageModelManager.LanguageModelBuilder`).

| Узел | Producer (парсер) | Factory (LMM) |
|---|---|---|
| POU (`FUNCTION_BLOCK`/`FUNCTION`/`PROGRAM`/`METHOD`/`INTERFACE`) | `POUDeclarationParser.cs:25` | `LanguageModelBuilder.cs:946` `CreatePOUDeclarationStatement`, `:962` (token-only), `:2366`/`:2372` (`_I...`), `CreateMethodDeclarationStatement` `:2378` |
| VAR (список) | `VariableListParser.cs` | `LanguageModelBuilder.cs:973` `CreateVariableDeclarationListStatement`, `:2384`/`:2390` |
| VAR (переменная) | `VariableDeclarationParser.cs:18` | `LanguageModelBuilder.cs:1009`,`:1015`,`:1021` |
| TYPE alias | `TypeDeclarationParser.cs:17` | `LanguageModelBuilder.cs:1054` `CreateAliasDeclaration` |
| TYPE struct/union | `TypeDeclarationParser.cs` | `:1066` `CreateStructDeclaration`, `:1079` `CreateUnionDeclaration` |
| TYPE ENUM | `EnumListParser.cs` | `:1099` `CreateEnumTypeDeclaration`, `:1111` `CreateEnumDeclarationListStatement`, `:1121` `CreateEnumDeclarationStatement` |
| top-level сборка POU | `POUSyntaxParser.cs:472` | `LanguageModelBuilder.cs:946` |
| Диспетчер деклараций | `DeclarationParser.cs:37`–`:58` | — |

Компилятор (`Compiler35220`) **не** строит DECLARATION-узлы; он их только читает/типизирует
и через `InterfaceCompiler` отображает `_ISignature` (фаза C1, `CompilerPhase1_Typifier.cs:159`–`:192`),
плюс генерирует implicit-узлы (task-cycle, `plugin\-.404.cs:617`–`:651`).

### c.2. Резолвинг имён и типов

- `CheckerScope` (`Scopes\CheckerScope.cs:23`) — «область видимости» компилятора и precompile
  (реализует `_IPrecompileScope*`, `ICommonScope*`, `global::\u0017.\u0006`, `global::\u0015.\u0002`).
  Конструкторы: `:26`,`:61`,`:96`; статический фабричный `CheckerScope.\u0001(...)` `:137`
  (возвращает scope; `:143` `new CheckerScope`). Создание дочерних scope: `:245` (`\u0001(_ISignature)`),
  `:275` (`\u0001(_IPreCompileContext)`), `:301`,`:307`. AllowPaths-движок `SymbolTable`.
- `SymbolTable` (`Scopes\SymbolTable.cs:15`) — **case-insensitive** индексы:
  - `\u0001` `:290` — подписи (`global::\u0007.\u0006`), `\u0002` `:293` — переменные,
    `\u0001` `:296` — `List<_ISignature>` (properties);
  - ctor `:18` → `\u0001(Precom, ComconPool, Comcon, overrides)` `:37`, наполнение
    `:234` (GVL-переменные), `:252` (properties), `:277` (все сигнатуры);
  - lookups: indexer `:141`, `\u0002(name)` `:156`, `\u0001(name)` (list) `:164`.
- Владелец кэша — `\u007F.\u0004 : ISymbolTables` (`plugin\-.4.cs:11`) с
  `\u0001(Precom, Pool)` → `new SymbolTable(...)` `:28`/`:32`; точка доступа компилятора —
  `\u0001.\u0008` (`plugin\-.8.cs:388`
  `PrimaryContext.SymbolTables as \u007F.\u0004).\u0001(precom, pool)` и поиск `\u0002(name)` `:251`).
- Построение таблицы у контекста: `CompileContext.cs:1531` `SymbolTables` → `:1537`
  `CompilerProxy.CreatePrecompileSymbolTables(this)` → `CompilerProxy.cs:515`;
  реализация — `Services\CompilerServicesInternal.cs:690`.
- Разрешение типов/имён по дереву:
  - precompile — `SimpleTypeChecker` (`PreCompile\Typification\SimpleTypeChecker.cs:39`,
    scope из `CheckerScope.\u0001(...)` `:48`), `SimpleTypeInferrer`, `TypeAcceptor`/`GenericTypeReplacer`;
  - compile — `TypeCheckerVisitor` (Phase4, `CompilerPhase4_Typechecker.cs:394`),
    `ExpressionTypifierWithSpecialTasks` (`Phase1_Typification\ExpressionTypifierWithSpecialTasks.cs`),
    `SignatureChecker`, `VariableChecker`, `VarStatInitValueChecker`;
  - `TypesOnlyTypifier` (`Phase1_Typification\TypesOnlyTypifier.cs:23`) — «только типы».

---

## (d) Точки вмешательства: «только синтаксис» vs «полная типизация»

| Хочу | Точка входа | Что получится | Не задействовано |
|---|---|---|---|
| **Только синтаксис/парсинг** | `Parser35220` `InternalParser.ParseST` (`InternalParser.cs:177`) напрямую + `CompiledPOU.cs:830` | red-tree, DECLARATION-узлы, позиции | типизация, scopes, ComconNew |
| **Синтаксис + сквозной резолвинг имён (типы без кода)** | `PreCompileService.FinishPrecompileChecks` (`PreCompileService.cs:144`) → `PrecompileChecksWindows.cs:280` → `PreCompileContext.CheckSignature/CheckPOUCode` (`:2124`/`:2151`) → `SimpleTypeChecker` | `_ICompilerMessage[]` в `PreCompileMessageCategory`; `CheckerScope`+`SymbolTable` | `_ICompileContext`, Phase1..5 |
| **Precompile-типизированное дерево (generic раскрыт)** | `PreCompileTypifier.CreateTypifiedParseTree` (`PreCompileTypifier.cs:69`), `bAddImplicitConversions` | типизированный red-tree | генерация кода |
| **Полная типизация без кода** | `CompilerProxy.Compile(comconPrecompiled,…)` (`CompilerProxy.cs:203`) → `CompilerPhaseControllerCompile.\u0001` (`plugin\-.404.cs:270`) | `ComconNew` с типизированными POU; Phase4 прогнан (`:224`) | Phase3/Phase5 |
| **Полная компиляция + локация + код** | `CompilerProxy.GenerateCode` (`CompilerProxy.cs:210`) → `CompilerPhaseControllerGenerateCode.\u000F()` (`CompilerPhaseControllerGenerateCode.cs:218`) | адреса, `CompiledCode`, сообщения `CompilerMessageCategory` | — |

Ключевое разделение: precompile-ветка работает с `_IPreCompileContext`
(`PreCompileContext`) и никогда не создаёт `ComconNew`; полная компиляция создаёт
`ComconNew = \u000E.\u001B` (`plugin\-.398.cs`) и проходит C1..C6.

---

## (e) Пробелы / что не подтверждено

1. **Имена обфусцированных классов** (`\u0017.\u000E`, `\u0004.\u001B`, `\u000E.\u001B`,
   `\u007F.\u0004`, `\u0081.\u0008`, `\u0001.\u0008`, `\u0002.\u0014`) восстановлены
   логически, а не из имён метаданных. Точные имена — только через `dnlib`/PDB (нет).
2. **`PreCompileContext`↔parser**: не найден единый метод, который из `PreCompileContext`
   синхронно вызывает `ParserService`; вход найден в `CompiledPOU.AfterDeserialize`
   (`:830`) и в per-object `CheckPOUCode` → `GetCompiledPOU` (lazy). Полная цепочка
   «изменение текста → создание `CompiledPOU` → parser» лежит в UI/ObjectModel-слое и не добита.
3. **`MacroReplacement`** вызывается для `ISequenceStatement`/`ILMPOU`, но точная фаза
   (до/после построения DECLARATION-узлов) по декомпилу неоднозначна — вероятно после парсинга,
   до типизации.
4. **Phase6** (`\u0002.\u0014` / `CompilerPhase6_AfterCodegeneration`) не читался целиком —
   известны только точки вызова (`CompilerPhaseControllerGenerateCode.cs:418`,`:424`,`:568`).
5. **`TypeAcceptor`/`GenericTypeReplacer`** имеют ровно один runtime-вызов
   (`plugin\-.143.cs:1999`); второй посетитель-наследник `ITypeVisitorX` в декомпиле не найден
   (возможно, есть в других версиях `Compiler35xxx.plugin`).
6. **Очередь generic vs non-generic**: `GenericTypeChecker` (проверка) и
   `GenericTypeReplacer` (подстановка) вызываются в разных местах; их взаимный порядок
   для одного выражения не трассирован до конца.
7. **Многопоточность** Phase4/Phase5 (`Thread`, `Environment.ProcessorCount`) делает
   порядок сообщений недетерминированным — для 100% идентичности Rust-порта нужен
   детерминированный обход (сортировка как в `CompilerPhase4_Typechecker.cs:136`).

---

## Ссылки

- `docs\02_PRECOMPILE_PIPELINE.md` — precompile-контракты и сообщения.
- `docs\05_TYPE_SYSTEM_SCOPES.md` — модель типов и scopes.
- `tables\compiler_phases.csv` — машиночитаемая версия списка фаз (этот же анализ).
- Декомпил: `decompiled\Compiler35220.plugin\`, `decompiled\LanguageModelManager.plugin\`,
  `decompiled\Compiler\`.
- Исходники парсера: `Parser35220.plugin\CODESYS\Parser35220\`.
