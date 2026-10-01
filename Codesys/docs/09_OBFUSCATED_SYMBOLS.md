# 09. Восстановление обфусцированных имён (SmartAssembly) — Compiler35220.plugin

Область: `decompiled/Compiler35220.plugin/*` (чтение). Связанные: `docs/05_TYPE_SYSTEM_SCOPES.md`
(типы/scopes уже ссылается на `\u001F.\u0010` и `\u0006.\u0011`), `docs/06_AST_BUILDER_MAP.md`
(builder-фабрика), `tables/obfuscated_map.csv` (машиночитаемая карта).

> **Главный вывод.** Обфускация SmartAssembly сосредоточена **только** в `Compiler35220.plugin`.
> В `LanguageModelManager.plugin` (декомпилированные 647 `.cs`) **не найдено ни одного** `\uXXXX`-имени —
> этот плагин читаемый и правок не требует (проверено скриптом `tools/find_obfuscated_symbols.ps1`).
> Исходные имена SmartAssembly **стёрты необратимо** (члены переименованы в `\u0001..\u0010` по ordinal),
> поэтому «восстановление» = установление **роли и сигнатур** по IL/использованиям, а не исходной строки.

## Нотация путей

Декомпилятор CODESYS разложил типы по каталогам-«мусоркам» с управляющими символами:

| В доке | Реально | Пример |
|---|---|---|
| `-` | каталог с именем `-` (0x2D) | `Compiler35220.plugin/-/-.291.cs` |
| `[ns0x80]` | каталог U+0080 | `Compiler35220.plugin/[ns0x80]/-.15.cs` |
| `[ns0x81]` | каталог U+0081 | `Compiler35220.plugin/[ns0x81]/-.7.cs` |
| `[ns0x84]` | каталог U+0084 | `Compiler35220.plugin/[ns0x84]/-.21.cs` |

`\\uXXXX` в именах — это SmartAssembly-эскейпы контрольных символов (namespace/class = ordinal).

## 1. Сводная таблица восстановленных символов

Полный список с `file:line`, `evidence`, `used_by` — в `tables/obfuscated_map.csv` (22 записи).
Ключевые:

| # | Символ | Определение | Роль | Критично для парсинга/типизации |
|---|---|---|---|---|
| 1 | `\u001F.\u0010` | `-/-.291.cs:24` | **OverloadResolver** (static) | ✔ резолвинг перегрузок вызовов |
| 2 | `\u0006.\u0011` | `-/-.304.cs:14` | **ITypeComparer impl** | ✔ совместимость/неявные конверсии типов |
| 3 | `\u0019.\u0003` | `-/-.71.cs:14` | **LanguageModelBuilder factory** | ✔ построение AST-узлов (парсер) |
| 4 | `\u0011.\u0005` | `-/-.111.cs:6` | **SymbolTable visibility-level enum** | ✔ порядок/видимость символов |
| 5 | `\u0007.\u0006` | `-/-.113.cs:10` | SymbolEntry | ✔ запись таблицы символов |
| 6 | `\u0010.\u0002` | `-/-.114.cs:11` | SymbolOverrides | ✔ переопределение сигнатур |
| 7 | `\u001C.\u0011` | `-/-.288.cs:9` | Overload-lookup service (интерфейс) | ✔ |
| 8 | `\u0018.\u000F` | `-/-.292.cs:17` | lookup service (precompile) | ✔ |
| 9 | `\u0080.\u0014` | `[ns0x80]/-.15.cs:10` | lookup service (compile) | ✔ |
| 10 | `\u0084.\u001A` | `[ns0x84]/-.21.cs:10` | Argument-provider (интерфейс) | ✔ |
| 11 | `\u001F.\u000F` | `-/-.287.cs:11` | Argument-provider из `_ICallExpression` | ✔ |
| 12 | `\u0084.\u001B` | `[ns0x84]/-.22.cs:10` | Компаратор по mangled-имени | ✔ |
| 13 | `\u000E.\u0016` | `-/-.289.cs:8` | delegate error-reporter | ✔ (диагностика резолвера) |
| 14 | `\u0012.\u0013` | `-/-.290.cs:8` | delegate message-sink | ✔ |
| 15 | `\u0003.\u0006` | `-/-.118.cs:15` | форматтер сообщений + IErrorHandler | — |
| 16 | `\u0011.\u0006` | `-/-.120.cs:17` | **Parser** (`_IParser`, `IRawSTParser`) | ✔ парсинг ST |
| 17 | `\u0007.\u0005` | `-/-.110.cs:19` | **Scope impl** (`ICommonScope2`/`IScope5`) | ✔ |
| 18 | `\u0081.\u0007` | `[ns0x81]/-.7.cs:28` | PrecompileScope impl | ✔ |
| 19 | `\u0081.\u0008` | `[ns0x81]/-.8.cs:10` | Перечислитель библиотечных контекстов | ✔ |
| 20–22 | `\u0011.\u0001`, `\u0081.\u0001`, `\u0081.\u0002` | `-/-.2.cs:15`, `[ns0x81]/-.cs:11`, `[ns0x81]/-.2.cs:15` | **Ресурс-аксессоры (UI)** — игнорировать | ✘ |

Итого: **19 значимых** символов восстановлено + **3** распознаны как нерелевантные (ресурсы/Strings).

## 2. Разбор 4 названных «проблемных» символов

### 2.1. `\u001F.\u0010` — OverloadResolver (`-/-.291.cs:24`)
`internal static class`, все методы — `\u0001`. Публичный вход:
`IList<_ISignature> \u0001(ICommonScope, _IPreCompileContext, _ICallExpression, IList<ICompiledType>, _ISignature, _ISignature4)`
и перегрузки для `_ICompileContext` / `IAssignmentExpression[]`.
Алгоритм (по IL):
1. строит lookup-сервис `\u001C.\u0011` (`\u0018.\u000F` для precompile, `\u0080.\u0014` для compile);
2. берёт под-сигнатуры по имени (`u.\u0001(sig4, name)`);
3. если ровно одна/ноль — ранний выход;
4. иначе фильтрует кандидатов через provider аргументов (`\u0084.\u001A`: `\u001F.\u000F` или `FBInitParameterService`),
   сравнивая типы либо `ITypeComparer` (`\u0006.\u0011`), либо mangled-компаратором (`\u0084.\u001B`);
5. диагностика: `MessageId.Err_NoMatchingOverload`, `MessageId.Err_Ambiguity` (+ `Inf_RelatedPosition`).

Использование: `TypifierAndCrossReferenceCollector.cs:665,684`, `SimpleTypeChecker.cs:450,1949`,
`-.310.cs:84`, `-.313.cs:69`, `?-.21.cs:492` — то есть **точка разрешения вызовов POU/FB/функций**.

### 2.2. `\u0006.\u0011` — ITypeComparer (`-/-.304.cs:14`)
`internal sealed class \u0011 : ITypeComparer`. Члены идут ровно в порядке интерфейса
`Compiler/_3S/CoDeSys/LanguageModelManager/InternalInterfaces/ITypeComparer.cs:9-37`, что даёт
однозначную реконструкцию имён:

| член | имя интерфейса |
|---|---|
| `\u0001(TypeClass,TypeClass,bool,bool,bool)` | `Imitates` |
| `\u0001(t1,t2,scope)` | `IsEqual` |
| `\u0001(t1,t2,s1,s2)` | `IsEqual` |
| `\u0001(t1,t2,s1,s2,bool)` | `IsEqual` |
| `\u0001(_IUserdefType,_IUserdefType,IScope2,IScope2)` | `IsEqual` |
| `\u0001(sig,sig,IScope2)` | `IsImplicitConvertable` |
| `\u0002(t1,t2,scope)` | `IsImplicitConvertable` |
| `\u0003(t1,t2,scope)` | `IsImplicitPointerConversion` |
| `\u0002(t1,t2,s1,s2)` | `IsCopyable` |
| `\u0001(sig,sig,s1,s2)` | `IsImplicitConvertable` |
| `\u0001(t,scope)` | `IsInterface` |
| `\u0001(litexp,...)` | `IsImplicitLiteralConvertable` |
| `ICompiledType \u0001(t,scope)` | `EvaluateAliasAndEnumType` |
| `\u0003(t1,t2,s1,s2)` | `IsImplicitPointerConversion` |
| `\u0004(t1,t2,s1,s2)` | `IsImplicitConvertable` |

**Именно здесь зашита версионная матрица совместимости** (Byte/USInt, DWord/UDInt/Pointer, LWord/ULInt,
64-bit pointer и т.п., строки/массивы/перечисления/subrange) и `IsImplicitConvertable`.
Регистрируется как сервис: `_3S/CoDeSys/Compiler35220/Services/CompilerServices.cs:41`
(`CreateTypeComparer`), проксируется в LMM через `TypeComparerProxy`/`VersionedCompilerFactory`.

### 2.3. `\u0019.\u0003` — фабрика LanguageModel (`-/-.71.cs:14`)
`internal static class \u0003`. Свойство `Builder` = `APEnvironmentFacade.Instance.LanguageModelMgr.CreateLanguageModelBuilder()`
приведённое к `_ILanguageModelBuilder7`; далее ~200 методов `\u0001(...)` = обёртки `Builder.CreateXxx(...)`
(`CreateCompiledPOU`, `CreateSignature`, `CreateVariable`, `CreateDataLocation`, `CreateArea`, десятки `CreateXxxType`,
`CreateSequenceStatement`, `CreateCallExpression`, `CreateLiteralExpression`, ...).
Это тот же builder, что использует парсер через `ParserContext.LMItemFactory`
(`docs/06_AST_BUILDER_MAP.md`). В Rust — модуль `builder::create_*`, singleton не нужен.

### 2.4. `\u0011.\u0005` — уровни видимости SymbolTable (`-/-.111.cs:6`)
`internal enum \u0005 { \u0001..\u0008, \u000E, \u000F }` (10 членов). Ordinal-уровни проходов импорта
символов в `Scopes/SymbolTable.cs`:

| Член | Где присваивается | Смысл (инференс) |
|---|---|---|
| `\u0001` | `:41,:126` | собственный контекст (+ SystemContext) |
| `\u0002` | `:57` | цепочка родительских приложений (1-й проход) |
| `\u0003` | `:70` | подключённые библиотеки (1-й проход) |
| `\u0004` | `:75` | pool |
| `\u0005` | `:80` | pool-библиотеки |
| `\u0006` | `:84,:85` | symbol-overrides (`\u0010.\u0002`) |
| `\u0007` | `:101` | родительские приложения (2-й проход, `\u0002(...)`) |
| `\u0008` | `:108,:109` | библиотеки (2-й проход) |
| `\u000E` | `:114,:127` | pool (2-й проход) |
| `\u000F` | `:119,:120` | pool-библиотеки (2-й проход) |

Уровень также управляет фильтрацией `SignatureFlag.Internal`: для уровней `\u0003/\u0005` (`:239`) и
`\u0008/\u000F` (`:282`) внутренние сигнатуры библиотек **не** добавляются. Уровень хранится в
`SymbolEntry \u0007.\u0006` (`-/-.113.cs:13,274`), методы `\u0002()/\u0003()` (`:73,:79`) классифицируют
«близкие» vs «библиотечные/системные» уровни.

## 3. Как это работает в конвейере (связь с `docs/05`)

```
Parser35220  --Builder(\u0019.\u0003)-->  AST (_ISequenceStatement/...)
      |
Phase1_Typification: TypifierAndCrossReferenceCollector
      |   SymbolTable(\u0011.\u0005 + \u0007.\u0006 + \u0010.\u0002)  -> поиск символа
      |   OverloadResolver \u001F.\u0010
      |        |-- lookup \u001C.\u0011 (\u0018.\u000F precompile | \u0080.\u0014 compile)
      |        |-- args    \u0084.\u001A (\u001F.\u000F | FBInitParameterService)
      |        |-- compare \u0006.\u0011 (ITypeComparer) | \u0084.\u001B (mangled)
      |        `-- errors  \u000E.\u0016 / \u0012.\u0013 / \u0003.\u0006
      |
      Scope: \u0007.\u0005 (compile) | \u0081.\u0007 (precompile) | \u0081.\u0008 (lib contexts)
```

`docs/05` описывает `ITypeComparer` и `TypeComparerProxy`, но раньше не расшифровывал **реализацию**
(`\u0006.\u0011`) и порядок членов; `docs/09` закрывает этот пробел. Резолвер `\u001F.\u0010`
в `docs/05` упоминался без деталей — здесь описан алгоритм и точки вызова.

## 4. Прочие «нечитаемые» имена (значимые / незначимые)

Помимо перечисленных, найдены и классифицированы:

**Значимые (включены в CSV):** `\u0011.\u0006` (Parser — точка входа ST-парсинга),
`\u0007.\u0005`/`\u0081.\u0007`/`\u0081.\u0008` (реализации Scope и перечислитель библиотек).

**Незначимые — НЕ портировать:** `\u0011.\u0001`, `\u0081.\u0001`, `\u0081.\u0002` — сгенерированные
`StronglyTypedResourceBuilder`-аксессоры `Resources.Strings / CompilerStrings / ErrorMessages`
(самые частые по упоминаниям — 365/263/1538 ссылок — но это только строки UI/ошибок).

**Остались без полной расшифровки (см. §5):** высокочастотные `\u0007.\u0005` уже разобраны; частично —
`\u000E.\u0011` (struct codegen-контекста Phase5, не парсинг), `\u000F.\u0003` (большой enum : ushort),
`\u0001.\u0001` (подкласс `StandardTraverser`).

## 5. Что критично для Rust-порта

1. **Порядок членов `ITypeComparer`** (`\u0006.\u0011`) — зафиксировать таблицей выше и реализовать
   матрицу `Imitates`/`IsImplicitConvertable` дословно (версионно-зависимо; тесты — на `TypeClass`).
2. **Алгоритм `\u001F.\u0010`**: сбор кандидатов → фильтр по фактическим типам аргументов →
   `Err_NoMatchingOverload`/`Err_Ambiguity`, при неоднозначности — `Inf_RelatedPosition`.
3. **Уровни `\u0011.\u0005`** — точный порядок проходов SymbolTable и правило скрытия `Internal`
   для библиотечных уровней (`\u0003/\u0005/\u0008/\u000F`).
4. **lookup-сервис `\u001C.\u0011`** + `NameManglingService` (канон. имена) — ключ к воспроизводимости
   резолвинга; `\u0018.\u000F` (PrecompileBaseSignatureId) vs `\u0080.\u0014` (BaseSignatureId).
5. **Builder `\u0019.\u0003`** — просто каталог конструкторов AST, singleton не воспроизводим.
6. Ресурс-аксессоры (`\u0011.\u0001`, `\u0081.\u0001`, `\u0081.\u0002`) — исключить из порта.

## 6. Что осталось невосстановленным и почему

- **Оригинальные строковые имена** — невосстановимы: SmartAssembly заменил имена на ordinal
  (`\u0001..`), метаданных об исходниках нет. Восстановлена только роль.
- **Полная семантика матрицы `\u0006.\u0011`** — читается из IL, но это ~1500 строк switch; перенос
  требует пошаговой сверки с `tables/` (TypeClass) и не проверялся запуском.
- **Часть приватных хелперов** (`\u0001.\u0001` StandardTraverser-subclass, `\u000E.\u0011` codegen-struct,
  `\u000F.\u0003` enum) не расшифрованы: они относятся к Phase5/codegen или вспомогательному обходу,
  а не к парсингу/типизации — вне приоритета.
- **SmartAssembly string-encryption**: если runtime-строки (имена атрибутов/сообщений) зашифрованы,
  их литералы в декомпиляции могут быть недостоверны — для порта нужно сверять с `tables/` и IL
  (утилита `tools/dump_method_il.ps1`).

## Артефакты

- `tables/obfuscated_map.csv` — 22 записи (asm, obfuscated_name, file:line, inferred_role, evidence, used_by).
- `tools/find_obfuscated_symbols.ps1` — сканер `\uXXXX`-имён по декомпилированным `.cs` (для регрессии).
