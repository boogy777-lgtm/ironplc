# Декомпилированные сборки (`decompiled/`) — оглавление

Наверх: [`../README.md`](../README.md). Все папки — результат статической декомпиляции
(ICSharpCode.Decompiler / dnSpy), по одному `.cs` на тип. Program Files не изменялся.

| Папка | Файлов | Исходная сборка | Что содержит | Связанные отчёты |
|---|---|---|---|---|
| [`Compiler/`](Compiler/) | 1405 | `Common\Compiler.dll` | КОНТРАКТЫ/enum: `CODESYS.Parser.*`, `Core.LanguageModel.*` (`TokenType`, `Operator`, `IECLanguage`, `_IStatement/_IExpression`), `LanguageModelBuilder.*` | 01, 04, 05 |
| [`Compiler35220.plugin/`](Compiler35220.plugin/) | 762 | `PlugIns\22222222-…\Compiler35220.plugin.dll` | версионный компилятор: `PreCompile.*` (Scanner/Typification/`PrecompileChecksWindows`), `TreeConversion\RedTreeBuilder.cs`, `CompilerPhases\*`, `Tools\TypeTable.cs`, `Messaging` | 02, 07, 14, 17–20 |
| [`LanguageModelManager.plugin/`](LanguageModelManager.plugin/) | 647 | `PlugIns\f0b1693d-…\LanguageModelManager.plugin.dll` | **реализация red/green-деревьев** (`RedTrees.Builder.*`), все `*Type` (84), `LanguageModelBuilder`, `TypeHelper`, `CompileContext`, `Signature` | 04–08, 10–13 |
| [`LanguageModelUtilities.plugin/`](LanguageModelUtilities.plugin/) | 221 | `PlugIns\…\LanguageModelUtilities.plugin.dll` | POU-узлы (`StructuredLanguageModel.*`: Function/FB/GVL/Method/…) + утилиты | 04, 05 |
| [`Objects/`](Objects/) | 347 | `Common\Objects.dll` | объектная модель проекта: `IObject.GetPositionText`, позиции | 02 |
| [`LanguageModelUtilities/`](LanguageModelUtilities/) | 120 | `Common\LanguageModelUtilities.dll` | интерфейсы утилит (`I*Builder`, `ILanguageModelUtilities*`) | 04, 05 |
| [`Parser35210.plugin/`](Parser35210.plugin/) | 98 | `PlugIns\03cc6aad-…\Parser35210.plugin.dll` | ST-лексер/парсер legacy (язык 3.5.21) — второй источник | 01 |
| [`LanguageModelManagerConfigurators.plugin/`](LanguageModelManagerConfigurators.plugin/) | 65 | `PlugIns\7ee3127e-…` | конфигураторы/опции компиляции | 05, 14 |
| [`MessageStorage/`](MessageStorage/) | 17 | `Common\MessageStorage.dll` | `IMessage/IMessage4/IMessageStorage2/Severity` | 02, 19 |
| [`ComponentModel/`](ComponentModel/) | 7 | `Common\ComponentModel.dll` | COM-инфраструктура (частично обфусцирована) | 13 |
| [`LanguageModelManagerLegacy.plugin/`](LanguageModelManagerLegacy.plugin/) | 4 | `PlugIns\4e5bfa8e-…` | legacy-facade | 05 |
| [`Core/`](Core/) | 0 | `Common\Core.dll` | native — декомпилировать нечего (`TypeClass`/`Operator` берутся из `Compiler.dll`) | 20 |
| [`WhiteParsetrees.plugin/`](WhiteParsetrees.plugin/) | 642 | `PlugIns\977d3b32-…\WhiteParsetrees.plugin.dll` | **white tree/CST**: узлы/фабрики/парсер/форматтер | 16 |
| [`WhiteParseTrees/`](WhiteParseTrees/) | 572 | `Common\WhiteParseTrees.dll` | контракт white tree | 16 |

## Смежное

- [`../Parser35220.plugin/`](../Parser35220.plugin/) — **основной ST-лексер/парсер** (94 файла),
  `Scanner\InternalScanner.cs`, `Scanner\OperatorTable.cs`, `Statements\*`, `Expressions\*`,
  `Declaration\*`, `Pragmas\*` (+ `Parser35220.plugin.sln`). Декомпил отдельно от `decompiled/`.
- [`../binaries/`](../binaries/) — оригинальные DLL (15) для сверки.
- [`../resources/`](../resources/) — сырые дампы ресурсов + satellite-DLL.

## Заметки

- Обфусцированные типы: префикс `\uXXXX` / `-/-.NNN.cs` (SmartAssembly) — см.
  [`../docs/09_OBFUSCATED_SYMBOLS.md`](../docs/09_OBFUSCATED_SYMBOLS.md) и
  [`../tables/obfuscated_map.csv`](../tables/obfuscated_map.csv).
- `file:line` во всех отчётах/таблицах — отсюда.
