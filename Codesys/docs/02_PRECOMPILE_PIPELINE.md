# 02. Пайплайн предкомпиляции (Vorcompilierung) CODESYS 3.5.22.10

Расширение `docs\PRECOMPILE_PIPELINE_RE.md` (RE-отчёт из рабочей папки
`C:\Users\HALLIBURTON\Desktop\1.1.0.0`). Дополнено декомпилированными исходниками
(`C:\Codesys\decompiled\...`) и данными по прекомпиляции/макросам.

> Ссылки `файл:строка` — на декомпилированные файлы в `C:\Codesys\decompiled\...`
> (dnSpy-экспорт) или на исходники `C:\Codesys\Parser35220.plugin\...`.
> Ссылки вида `_dump\*.txt:NNNN` — сгенерированные dnlib-дампы API в рабочей папке.

---

## 0. Артефакты (пути/размеры/число типов)

| Артефакт | Путь | Типов (dnlib) | .cs (dnSpy) | Размер |
|---|---|---|---|---|
| Бинарь компилятора | `binaries\Compiler35220.plugin.dll` | 1039 | — | 2 418 944 Б |
| Бинарь сообщений | `binaries\MessageStorage.dll` | 17 | — | 19 712 Б |
| Бинарь объектов | `binaries\Objects.dll` | 347 | — | 144 128 Б |
| Бинарь LMM | `binaries\LanguageModelManager.plugin.dll` | 742 | — | 1 997 056 Б |
| Декомпил компилятора | `decompiled\Compiler35220.plugin\` | — | 762 | ~6.7 МБ |
| Декомпил сообщений | `decompiled\MessageStorage\` | — | 17 | ~13 КБ |
| Декомпил объектов | `decompiled\Objects\` | — | 347 | ~385 КБ |
| Декомпил LMM | `decompiled\LanguageModelManager.plugin\` | — | 647 | ~5.1 МБ |

Версии/типы (dnlib): `Compiler35220.plugin.dll` обфусцирована **SmartAssembly**
(в декомпиле присутствует namespace `SmartAssembly`, обфусцированные имена типов
санитизированы в имена вида `_`/`_XXXX`).

Ключевые пространства имён в `decompiled\Compiler35220.plugin\_3S\CoDeSys\Compiler35220\`:
`PreCompile`, `PreCompile\Typification`, `CompilerPhases`, `Compile`, `Phase1_Typification`,
`Phase2_AfterTypification`, `Phase3_Location`, `Phase4_TypeCheck`, `Phase5_Codegeneration`,
`Messaging`, `Services`, `Tools`, `CompilerVersion`, `Resources`.

---

## 1. Что такое «предкомпиляция» (Vorcompilierung) и чем отличается от компиляции

| | Предкомпиляция (precompile) | Полная компиляция (compile) |
|---|---|---|
| Цель | парсинг + типизация/проверка ссылок **без генерации кода** | + локация (адреса) + генерация кода + линковка |
| Сервис-контракт | `ILMPreCompileService`, `ILMPreCompileCheckerService`, `ILMPreCompileCrossReferenceService`, `ILMPreCompileSmartCodingService` | `ILMCommandService.Compile/GenerateCode`, `ILMCompileService` |
| Реализация | `LanguageModelManager.Services.PreCompileService`, `…PreCompileCrossReferenceService` | `LanguageModelManager.Services.CommandService`, `CompileService` |
| Категория сообщений | `PreCompileMessageCategory` | `CompilerMessageCategory` |
| Набор проверок | синтаксис, типы, ссылки (syntax/types/references) | + linker/layout/pragma-ошибки |
| Триггер в UI | фоновая проверка (checker thread) + «проверить все объекты» | «Компилировать/Собрать» (GenerateCode) |

Оригинальный MCP-инструмент формулирует это так
(`_archive\decomp\decomp_DevMCP_plugin\...\CheckForErrorsParams.cs:13`):
`Compile=false` — Pre-compile, быстрые статические проверки; `Compile=true` — полная
компиляция, медленно, ловит linker/layout/pragma.

---

## 2. Пошаговая схема пайплайна

### 2.1. Предкомпиляция (режим `!compile`)

```
[MCP codesys_check_errors] / UI (фоновая проверка)
        │
        ▼
IdeContext.LanguageModelMgr (ILanguageModelManager28)
   ├─ .PrecompileInformationUpToDate      // ILanguageModelManager24  (_dump/compiler_types.txt:5227,5230)
   └─ .FinishPrecompileChecks()           // ILanguageModelManager22  (_dump/compiler_types.txt:5222)
        │
        ▼
Services.PreCompileCrossReferenceService.FinishPrecompileChecks()   // lmm_types.txt:16068
Services.PreCompileService.FinishPrecompileChecks()                 // lmm_types.txt:16141
        │  (оба: CompilerProxy.GetCheckerThread().FinishPrecompileChecks())
        ▼
CompilerProxy.GetCheckerThread()          // lmm_types.txt:5077
        │  → ICompilerHelper.GetCheckerThread()   (_dump/compiler_types.txt:7434)
        ▼
_ICheckerThread.FinishPrecompileChecks()   // _dump/compiler_types.txt:11234
        │  РЕАЛИЗАЦИЯ (Compiler35220.plugin):
        │  _3S.CoDeSys.Compiler35220.PreCompile.PrecompileChecksWindows : _ICheckerThread
        │  _3S.CoDeSys.Compiler35220.PreCompile.PrecompileChecksNone    : _ICheckerThread
        ▼
_3S.CoDeSys.Compiler35220.PreCompile.*
   Scanner.cs (создаёт InternalScanner через LanguageServices.ScannerService)
   InterfaceParser.cs / ParserHelper.cs      // парсинг ST
   Typification\PreCompileTypifier.cs, SimpleTypeChecker.cs, SimpleTypeInferrer.cs
   IfStatementConverter.cs, MacroReplacement.cs, MacroInfoProvider.cs
        │  сообщения: _ICompilerMessage[] / IList<IMessage4>
        ▼
PreCompileContext.MessageOutput(IMessageStorage, IMessageCategory, Guid)   // lmm_types.txt:6867
   RemoveMessages/AddNewMessagesToMessageStorage                           // lmm_types.txt:6863,6868
        ▼
IMessageStorage2.GetMessages(PreCompileMessageCategory, severityMask)       // messagestorage_types.txt:60
        ▼
рендер (code/message/путь/позиция)
```

Точечная проверка объекта (без общего прогона):
`ILMServiceProvider.PreCompileSmartCodingService.CheckPOUCode(precompileSet, guidObject, messages)`
(`_dump/compiler_types.txt:4067,4074`; реализация `Services.PreCompileSmartCodingService`).

### 2.2. Полная компиляция (`compile=true`)

```
ILMServiceProvider3.CommandService (ILMCommandService)
   .CheckAllApplicationObjects(appGuid)     // _dump/compiler_types.txt:3687
        │  Services.CommandService.CheckAllApplicationObjects  // lmm_types.txt:15844
        │  → CompilerProxy.Compile(guid, cb, bCheckAll:1, bKeepCompileInformation:0)
        ▼
LanguageModelManagerConsolidated.Compile(guid)  // lmm_types.txt:9446
        ▼
CompilerProxy.Compile(...)                      // lmm_types.txt:5073-5074
        ▼
VersionedCompilerFactory.GetCompilerOrNull(Version) → ICompiler4
        ▼
_3S.CoDeSys.Compiler35220.Services.CompilerServices : ICompilerServiceFactory
        ▼
_3S.CoDeSys.Compiler35220.CompilerPhases.*
   Phase1_Typifier → Phase2_AfterTypification → Phase3_Locator
   → Phase4_Typechecker → Phase5_Codegenerator
        ▼
MessageStorage, категория CompilerMessageCategory
        ▼
IMessageStorage2.GetMessages(CompilerMessageCategory, severityMask)
```

---

## 3. API: сигнатуры и как получить `(code, message, file, line, column)`

### 3.1. Ключевые контракты (`_dump/compiler_types.txt`)

```csharp
interface ILMCommandService {                       // :3665
    bool Compile(Guid app);                         // :3683
    bool CheckAllApplicationObjects(Guid app);      // :3687
    bool CompileAndLocate(Guid app);                // :3686
    bool GenerateCode(Guid app, bool onlineChange, bool keepInfo,
                      out IMessage[] errors, out IMessage[] warnings); // :3685
}
interface ILMPreCompileCrossReferenceService {      // :3951
    bool PrecompileInformationUpToDate { get; }     // :3952
    void FinishPrecompileChecks();                  // :3954
    bool CheckAllPOUs(ILMPreCompileSet set);        // :3957
}
interface ILMPreCompileCheckerService {             // :4039
    bool PrecompileChecksDone { get; set; }         // :4040
    event CompileEventHandler AfterPrecompileChecksDone; // :4041
    void FinishPrecompileChecks();                  // :4046
}
interface ILMPreCompileSmartCodingService {         // :4067
    bool CheckPOUCode(ILMPreCompileSet, Guid, IList<IMessage4>);      // :4074
    bool CheckSignature(ILMPreCompileSet, Guid, IList<IMessage4>);    // :4075
}
interface ILMServiceProvider {                      // :4148
    ILMCommandService CommandService { get; }
    ILMPreCompileService PreCompileService { get; }
    ILMPreCompileCrossReferenceService PreCompileCrossReferenceService { get; }
    ILMPreCompileSmartCodingService PreCompileSmartCodingService { get; }
}
```

### 3.2. Модель сообщений (`_dump/messagestorage_types.txt`)

```csharp
interface IMessage {                    // :2
    int ProjectHandle { get; }          // :3
    Guid ObjectGuid { get; }            // :4
    long Position { get; }              // :5
    short PositionOffset { get; }       // :6
    short Length { get; }               // :7
    string Text { get; }                // :8
    Severity Severity { get; }          // :9
}
interface IMessage4 {                   // :25
    Icon Icon { get; }                  // :26
    uint? Number { get; }               // :27  ← код
    string Prefix { get; }              // :28  ← префикс кода
}
enum Severity {                         // :32  (значения — степени двойки, используются как маска)
    FatalError=1; Error=2; Warning=4; Information=8; Text=16; SuppressedWarning=32; SuppressedInformation=64;
}
interface IMessageStorage {             // :53
    IMessage[] GetMessages(IMessageCategory category);                       // :59
    IMessage[] GetMessages(IMessageCategory category, Severity severityMask);// :60
}
interface IMessageStorage2 {            // :69  RemoveMessages(cat, Predicate<IMessage>)
}
```

Коды — `enum _3S.CoDeSys.LanguageModelManager.InternalInterfaces.MessageId`
(`_dump/compiler_types.txt:10706`): `None=0, Err_ConstantOverflow=1, Err_Operator1of2Expected=2, …`.
Конкретный класс сообщения компилятора — `_3S.CoDeSys.LanguageModelManager.CompilerMessage`
(`lmm_types.txt:10981`), реализует `_ICompilerMessage, IMessage4, IMessage3, IMessage2, IMessage`.

### 3.3. Сборка полей

| Поле | Источник |
|---|---|
| code | `(msg as IMessage4)?.Prefix + msg.Number.Value.ToString("D4")` |
| message | `msg.Text` |
| file | путь объекта по `msg.ObjectGuid` (`IObjectManager8.GetObjectNamePath(handle, guid)`) |
| line/column | `IObject.GetPositionText(msg.Position)` (`_dump/objects_types.txt:2467`); точный диапазон — `msg.Position/PositionOffset/Length` |
| severity | `(int)msg.Severity`: 1/2 = error; 4/32 = warning |
| scope | `msg.ObjectGuid` (+ `msg.ProjectHandle`) |

Маска: «с предупреждениями» → `(Severity)6` (Error|Warning), иначе `(Severity)2`.
Категории: `PreCompileMessageCategory` / `CompilerMessageCategory`
(классы `_3S.CoDeSys.LanguageModelManager.PreCompileMessageCategory` `lmm_types.txt:11306`,
`…CompilerMessageCategory` `lmm_types.txt:11298`).

---

## 4. Прекомпиляция: сканер и макросы `{IF}`

В версионном компиляторе прекомпиляция реализована в `_3S.CoDeSys.Compiler35220.PreCompile`
(декомпил: `decompiled\Compiler35220.plugin\_3S\CoDeSys\Compiler35220\PreCompile\`):

- **Scanner** (`PreCompile\Scanner.cs`) — фасад над `InternalScanner`
  (`LanguageServices.ScannerService.CreateInternalScanner(...)`), задаёт опции
  (`AllowMultipleUnderlines=false`, `AllowNestedComments` из настроек LMM, `IgnoreCase=true`,
  `IncludeComments/EndOfLines/Pragmas/Whitespaces`).
- **MacroReplacement** (`PreCompile\MacroReplacement.cs:16-38`):
  `ReplaceMacroOperators(int projectHandle, Guid objectGuid, ISequenceStatement)` и
  перегрузка для `ILMPOU` (обрабатывает `Interface` и `Body`) через `MacroInfoProvider`.
- **MacroInfoProvider**, **IfStatementConverter** — данные макросов и обработка условно
  компилируемых блоков.
- **Typification** — `PreCompileTypifier`, `SimpleTypeChecker`, `SimpleTypeInferrer`,
  контроллеры подавления сообщений (`MessageSuppression\{NoSuppressions,IgnoreWarnings,
  IgnoreWarningsAndErrors,OnlineChangeSuppressions}`).
- Реализация потока проверок — `PrecompileChecksWindows` (UI) и `PrecompileChecksNone` (NoUI),
  обе `_ICheckerThread`.

Макросы `{IF}/{ELSIF}/{ELSE}/{END_IF}` на уровне синтаксиса разбирает `Parser35220`
(`Pragmas\PragmaIfStatementParser.cs:115 ParsePragmaIf`); словарь прагм —
`PragmaScanner\PragmaScanner.cs:40-88` (включая `defined`, `project_defined`, `COMPILERVERSION`,
`RUNTIMEVERSION`, `attribute`, `error`, `warning`, `info`, `text`, `define`, `undefine`, …).
Подробнее — `C:\Codesys\grammar\ST_GRAMMAR.md` (раздел 6).

---

## 5. Что уже делает McpFree и чего не хватает

Реализация: `McpFree\src\Plugin\Tools\ReadOnlyTools.cs` (`CheckForErrorsTool`, :558; tool `check_errors`),
сервисы — `IdeContext`/`DependencyBag` (`:32,34,35,37,40,41`).

Реализовано:
- precompile-режим: `PrecompileInformationUpToDate` → `FinishPrecompileChecks()` →
  `GetMessages(PreCompileMessageCategory, mask)` (`ReadOnlyTools.cs:638-646`);
- compile-режим: `CommandService.CheckAllApplicationObjects(appGuid)` →
  `GetMessages(CompilerMessageCategory, mask)` (`:618-637`);
- код `Prefix+Number:D4`, путь объекта, позиция через `GetPositionText`.

Не хватает (кандидаты на доработку):
1. Нет контейнер-/локальной позиции (`ObjectMapping`/`VirtualLineMap`); теряется диапазон и
   вложенный путь (ST sub-POU/SFC).
2. `line/column` — из локализованной строки `GetPositionText` (RU-регекс), не числа.
3. Не используется `PreCompileSmartCodingService.CheckPOUCode` (per-object, без `MessageStorage`).
4. Полная компиляция = `CheckAllApplicationObjects` (кода не генерирует) — для linker/layout
   нужен `CompileAndLocate(guid)` (`_dump/compiler_types.txt:3686`).
5. При NoUI — нет `EnablePrecompileChecksInNoUIMode()` (`compiler_types.txt:3955`).
6. Вывод — текстовые строки (нет типизированного `(code,severity,file,line,col)`).

---

## 6. Ссылки

- Декомпил компилятора: `C:\Codesys\decompiled\Compiler35220.plugin\`
- Декомпил LMM: `C:\Codesys\decompiled\LanguageModelManager.plugin\`
- Декомпил сообщений/объектов: `C:\Codesys\decompiled\MessageStorage\`, `…\Objects\`
- Бинари: `C:\Codesys\binaries\{Compiler35220.plugin,MessageStorage,Objects,LanguageModelManager.plugin}.dll`
- Грамматика ST: `C:\Codesys\grammar\ST_GRAMMAR.ebnf`, `C:\Codesys\grammar\ST_GRAMMAR.md`
- RE-отчёт-предшественник: `C:\Users\HALLIBURTON\Desktop\1.1.0.0\docs\PRECOMPILE_PIPELINE_RE.md`
