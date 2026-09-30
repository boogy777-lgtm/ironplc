# 17. Остаточные мелочи компилятора: детерминизм, Phase6, обфусцированный типизатор, `__XSTRING#`/cp1252

Дополняет `docs\14_COMPILER_PHASES.md` и `docs\15_STRING_LITERALS.md`. Закрывает 5 пунктов
(детерминированный порядок, cp1252, Phase6, `\u0017.\u000E`, `__XSTRING#`+UTF-8).
Все ссылки — на файлы `C:\Codesys`; `file:line` даны по декомпилу и, где отмечено, по IL (`dnlib`).

> **Обозначения путей.** Обфусцированные типы `decompiled\Compiler35220.plugin\` разложены по
> папкам-«односимвольным» пространствам имён (коды 127..132). Ниже папка пространства-имени
> пишется как `plugin\-\-.N.cs` (файлы вида `-.N.cs`), т.к. символы неуправляемые.
> Пример: `\u0002.\u0014` (Phase6) лежит в `plugin\-\-.402.cs`; компаратор `\u0084.\u0005` — в
> `plugin\<папка 0x84>\-.5.cs`.

---

## 1. Детерминированный порядок многопоточных фаз Phase4/Phase5

### Правило

Обе фазы строят очередь **`ConcurrentQueue<_ICompiledPOU>`** из
`GetAllCompiledPOUsEx()` (базовый список), применяя LINQ-сортировку
**`OrderBy(identity, comparer)`**, где:

- **ключ (key selector) — тождественный** (`x => x`): в IL `CompilerPhase4_Typechecker/<>c::\u0001`
  (token `0600389C`) тело — `ldarg.1; ret`. Реальный ключ читается **внутри компаратора**;
- **компаратор `\u0084.\u0005 : IComparer<ICompiledPOU4>`** сравнивает
  **`_ICompiledPOU.NumberOfStatements`** и сортирует **по убыванию** числа инструкций
  (`plugin\<0x84>\-.5.cs:12-25`):
  `if (a.NumStmts < b.NumStmts) return 1; if (>) return -1; return 0;`
- `Enumerable.OrderBy` — **стабильная** сортировка: при равном `NumberOfStatements` сохраняется
  относительный порядок элементов из `GetAllCompiledPOUsEx()`.

Итоговое правило для Rust-порта:

```
// база — порядок обхода объектов компиляции (CompileContext.m_alCompiledPOUs)
let mut jobs = base_pous.clone();                     // GetAllCompiledPOUsEx() == копия списка
jobs.sort_by_stable(|a, b| b.number_of_statements.cmp(&a.number_of_statements)); // desc, stable
// затем обработка строго в порядке jobs
```

### Где именно

| Что | Файл:строка |
|---|---|
| OrderBy(identity, `\u0084.\u0005`) | `decompiled\Compiler35220.plugin\_3S\CoDeSys\Compiler35220\CompilerPhases\CompilerPhase4_Typechecker.cs:136` |
| Entry Phase4 `\u0003(bool)`, пул `Environment.ProcessorCount` | `...\CompilerPhase4_Typechecker.cs:131`,`:133` |
| Создание воркеров/потоков | `...\CompilerPhase4_Typechecker.cs:138-149` |
| Ожидание (retry `Join(50)`) | `...\CompilerPhase4_Typechecker.cs:156-163` |
| Воркер (nested `\u0001`): ctor, цикл, per-POU | `...\CompilerPhase4_Typechecker.cs:256`,`:309`,`:328` |
| Typecheck одного POU (scope/ConstantFolder/`TypeCheckerVisitor`) | `...\CompilerPhase4_Typechecker.cs:383`,`:392-395` |
| Компаратор `\u0084.\u0005` (Number OfStatements, desc) | `decompiled\Compiler35220.plugin\<0x84>\-.5.cs:12-25` |
| Phase5: тот же comparer, но только при многопоточности | `...\CompilerPhases\CompilerPhase5_Codegenerator.cs:215` (см. `:195-216`) |
| Phase5 воркер `global::\u0018.\u0013` | `...\CompilerPhase5_Codegenerator.cs:221`,`:249` |
| База: `GetAllCompiledPOUsEx()` = копия `m_alCompiledPOUs` | `decompiled\LanguageModelManager.plugin\_3S\CoDeSys\LanguageModelManager\CompileContext.cs:4614-4617`; поле `:4895`; наполнение вставкой `:2097`,`:2268` |

### Воркеры/пул (устройство)

- Phase4 (`\u0003(bool)`): массив воркеров `\u0001[processorCount]` и массив `Thread[]`;
  очередь — `ConcurrentQueue` в **детерминированном** порядке; таблица
  `CompactedTypifiedParseTreeInformation` — `ConcurrentDictionary`; воркер в цикле
  `TryDequeue` (`:314`) обрабатывает POU, ставит флаг `TypeCheckDone` (идемпотентность, `:330-334`).
  Потоки стартуют, затем джойнятся по одному с `Join(50)` в цикле.
- Phase5 (`\u0001(IList<ICompiledPOU4>)`): `num = IsCodegenMultithreadingAllowed() ? ProcessorCount : 1`;
  **если `num <= 1` сортировка НЕ применяется** — очередь = `\u0002.Cast<_ICompiledPOU>()` в исходном
  порядке (`:211`). Если `num > 1` — `OrderBy` тем же компаратором (`:215`).
- Precompile-ветка B0 (`PrecompileChecksWindows.cs:474`/`:492`) сортировку не делает: очередь
  «грязных» `LanguageModelResult` (`LMItemQueue`), порядок — от UI-событий.

### Следствие о недетерминизме

Детерминирован **состав и порядок очереди**, но не порядок *завершения* воркеров (потоки
вытесняют друг друга). Сообщения каждой POU пишутся в неё саму (`SetMessages`), а общая
агрегация `Messages.\u0001` может зависеть от порядка завершения. Для 100% идентичности Rust:
обрабатывать `jobs` **последовательно** в вычисленном порядке (или индексировать воркеров
детерминированно и собирать результаты в массив по индексу).

Файл-таблица: `tables\phase_worker_order.csv`.

---

## 2. Таблица cp1252 для байтов 0x80–0x9F

Код декодирования: `Parser35220.plugin\CODESYS\Parser35220\Scanner\InternalScanner.cs:1610-1626`
(`EscapeLocalEncodingCodepoint`):

```csharp
int n = int.Parse(s, HexNumber);          // 2 hex (1-байтные) или 4 hex (2-байтные)
if (n > 127 && n < 256)                   // ТОЛЬКО 0x80..0xFF
    target.Append(Encoding.GetEncoding(1252).GetString(new byte[]{(byte)n}));
else
    target.Append((char)n);               // ASCII 0x00..0x7F
```

Артефакт: **`tables\cp1252.csv`** (32 строки данных + заголовок, колонки
`byte_hex,byte_dec,unicode_cp,unicode_hex,char,cp1252_defined,source_check`).

Проверка:
- значения сгенерированы вызовом **реального** `[System.Text.Encoding]::GetEncoding(1252)`
  (Windows PowerShell 5.1 / .NET Framework 4.x — та же среда, что у CODESYS Desktop);
- перекрёстная сверка с Python `cp1252`: 0 расхождений на определённых байтах;
- `A0..FF` — тождественны Latin-1 (`0xA0`→U+00A0 … `0xFF`→U+00FF), проверено.

**Важный нюанс (5 «undefined» байт).** Для `0x81,0x8D,0x8F,0x90,0x9D` классическая cp1252
не определена, но .NET Framework `GetEncoding(1252).GetString` выполняет **pass-through в C1
control**, т.е. возвращает U+0081/U+008D/U+008F/U+0090/U+009D (проверено). Значит
`'$81'` даёт символ U+0081, а не ошибку/`U+FFFD` (в .NET Core 5+ было бы `U+FFFD` — но CODESYS
3.5.22 Desktop не на .NET Core).

Ключевые строки таблицы: `0x80`→U+20AC (€), `0x82`→U+201A, `0x83`→U+0192, `0x8A`→U+0160 (Š),
`0x91/0x92`→U+2018/U+2019, `0x93/0x94`→U+201C/U+201D, `0x96/0x97`→U+2013/U+2014, `0x99`→U+2122,
`0x9F`→U+0178 (Ÿ).

---

## 3. Phase6 (`AfterCodegeneration`), тип `\u0002.\u0014`

- Тип: **`\u0002.\u0014` = `CompilerPhase6_AfterCodegeneration`**, файл
  `decompiled\Compiler35220.plugin\-\-.402.cs:28` (namespace `\u0002`, class `\u0014`).
- Создаётся в фабрике конвейера: `plugin\-\-.399.cs:28`
  (`u001B.CompilerPhase6_AfterCodegeneration = new global::\u0002.\u0014(...)`).
- Поле контейнера: `plugin\-\-.398.cs:212`.

### Основной вызов — `\u0001(ICodegenerator, ref bool)` (`-.402.cs:92-109`)

Из `CompilerPhaseControllerGenerateCode.cs:568` (внутри `\u0002()`, шаг «Codegeneration»):

```
566  CompilerPhase5_Codegenerator.\u0001(codegenerator);   // C5
568  CompilerPhase6_AfterCodegeneration.\u0001(codegenerator, ref allOk); // C6
```

Порядок шагов Phase6 (`-.402.cs`):

1. `:94` `allOk &= Messages.\u0001(ComconNew, MessageStorage, MessageCategory)` — сверка с
   накопленными сообщениями.
2. `:95-98` если `allOk && OnlineChange && OnlineChangeDetails != null` → `\u0003()`
   (`:163` → `global::\u0010.\u0010.\u0001(ComconNew, OnlineChangeDetails)`) — финализация
   online-change.
3. `:99` `ComconNew.ResetExprementHashTables()`.
4. `:100` **`codegenerator.EndGeneration()`** — закрытие генератора (после этого код фиксируется).
5. `:101` `\u0001()` (`:112-127`) — сброс флагов переменных
   `LocationChanged|OnlChangeCopy|OnlChangeInit|OnlChangeVFInit|OnlChangeExit|OnlChangeReInit`
   (если есть `OnlineChangeDetails.ResetOnlineChangeFlags()`, иначе — обход всех сигнатур).
6. `:102` `\u0004()` (`:176-219`) — **контрольные суммы**:
   `CheckSumCode/CheckSumData`, `CheckSumCodeLast/CheckSumDataLast`, сборка `CodeId`/`DataId`
   (Guid из 4 байт checksum). Если OEM `DisableChecksumComputation` — случайные значения.
7. `:103-107` если **не** OnlineChange → `LastCodeId = CodeId`, `LastDataId = DataId`.
8. `:108` `\u0002()` (`:130-160`) — при `CompactDownload`: резерв глобальной памяти и проверка
   соответствия code-сегмента (`Err_NotEnoughMemoryForCompactDownload`).

### Дополнительные (частичные) вызовы — FabstOnlineChange

`CompilerPhaseControllerGenerateCode.cs`:

- `:418` `CompilerPhase6_AfterCodegeneration.\u0003();` — только финализация online-change
  (после успешного FastOnlineChange).
- `:424` `CompilerPhase6_AfterCodegeneration.\u0004();` — только checksum/compute id
  (если OEM не выставил `DisableChecksumComputationForFastOnlineChange`).

### Место в конвейере

Полный порядок в `\u000F()` (`CompilerPhaseControllerGenerateCode.cs:218`):
setup (`:225-227`) → FastOnlineChange (`:228`, может задеть `:418`/`:424`) →
`Compile_Phase` C1..C4 (`:234`→`:510`) → `Location` C3 (`:246`→`:527`) →
`Codegeneration` (`:258`→`\u0002()` `:560`): **C5 (`:566`) → C6 (`:568`)**. То есть Phase6 —
завершающий шаг внутри шага «Codegeneration», а не отдельная управляющая фаза.

---

## 4. Обфусцированный конвейер типизации `\u0017.\u000E`

- Тип: `\u0017.\u000E`, файл `decompiled\Compiler35220.plugin\-\-.143.cs:27`. Это **визитор типизации**
  red-tree, реализующий `IExprementVisitor*` и `IOperatorExpressionVisitor*` (все версии API —
  `IExprementVisitor352000 … IExprementVisitor`, `IOperatorExpressionVisitor6 … 1`).
- Роль в конвейере — **шаг типизации precompile-дерева (B2/B3)**: вход — «сырое» red-tree
  (`_IStatement`/`_IExpression`) + `_IPreCompileContext` + scope (`\u000F.\u0007`); выход —
  `TypifiedRedParseTree` (копия дерева, у выражений заполнены `.Type`/`._CompiledType`,
  раскрыты generic-параметры).

### Состояние и вход

| Элемент | Строка |
|---|---|
| `TypifiedRedParseTree { get; private set; }` | `-.143.cs:32` |
| `Scope` (`\u000F.\u0007`), `PreComLocal` (`_IPreCompileContext`), `Signature`, `GenericTypeStack` | `:37`,`:42`,`:47`,`:52` |
| ctor `(_IPreCompileContext, Scope)` | `:55-60` |
| static `\u0001(_IExpression, _IPreCompileContext)` — визит на месте (создаёт scope) | `:63-67` |
| static `\u0001(_IExpression, _IPreCompileContext)` — `Duplicate()` затем визит | `:70-75` |
| static `\u0001(_IExpression, _IPrecompileScope)` — резолвит контекст по `ApplicationGuid` | `:78-82` |
| `\u0001(IStatement, ISignature)` — установить `Signature`, `Accept(this)` | `:85-94` |
| `\u0001(_ICompiledPOU)` — `Duplicate()` дерева → `TypifiedRedParseTree`, визит | `:97-108` |

### Шаги (по типам узлов)

- **Statement-визиторы**: `_ISequenceStatement` `:111-129` (последовательно, с per-statement
  `try/catch` → `AddError/AddMessage`), `while/repeat/for/if/return/jump/assignment/expr-statement`
  `:159-255`; декларации (`VAR/POU/TYPE/ENUM`) — **no-op** `:259-291` (декларации строит фаза C1,
  см. док.14).
- **`_ICallExpression`** `:294-321`: обход `_Condition` → push generic-типа вызываемого
  (`\u0001(null)`/`Accept`/`\u0001()`) → резолв сигнатуры по `_UserdefType` → `\u0002(\u0002, isignature)`
  (заполнение недостающих формальных параметров, `:358-373`) → обход `ParamExpressions`,`OutputExpressions`
  → установка `.Type` из `Outputs[0]` (`:324-339`) → generic-ветка `\u0001(_ICallExpression, IGenericUserdefType, _ISignature)`
  `:342-355` (для каждого входа push/pop generic-типа).
- **Операторные выражения**: результат резолвится helper'ом
  `\u001D.\u0005.\u0001(Operator, bool, int, IList<_IExpression>, ICompiledType, Scope, null)`
  (`:394-397`); далее `.Type`/`._CompiledType` заполняются в `visit(...)`.
- **Enum-подстановка**: `\u0003(_IExpression)` `:1935-1946` (для enum-сигнатуры меняет тип на
  `\u0019.\u0003.\u0001(OrgName, PrecompileId)`).
- **inferred type**: `\u0001(_IVariableExpression, IVariable)` `:1949-1960` (атрибут `inferredtype`).

### Generic-инстанцирование (`:1963-2000`)

- `GenericTypeStack` — стек `\u0017.\u000E.\u0001 { UserdefTypeIn, UserdefTypeOut }` (`:2022-2042`);
  push `:132-139`, pop `:142-145`, обёртка визита `:148-156` (push → Accept → pop).
- `\u0002(_IVariableExpression)` `:1963-1972`: берёт `UserdefTypeIn` со стекa, пересобирает
  `_CompiledType = \u0001(_CompiledType, UserdefTypeIn)`, и если результат `IGenericUserdefType`,
  кладёт его в `UserdefTypeOut`.
- `\u0001(ICompiledType, IGenericUserdefType)` `:1975-2000`:
  1. `new CheckerScope(ApplicationGuid, Signature, PreComLocal, Pool)` `:1977`;
  2. `Dictionary<string,_IExpression>` `:1978`;
  3. берёт сигнатуру generic-типа `this.Scope.\u0001(\u0003)` `:1981`; если нет — вернуть тип как есть;
  4. параметры `isignature.\u0002()` + `\u0003.GenericConstantsInitializations` → словарь
     `имя → выражение` `:1986-1992`;
  5. `GenericTypeReplacer.\u0001((_IType)type, new ConstantFolder(), scope)` `:1998-1999` — сворачивает
     границы/длины/размерности по словарю (см. док.14 §b).

### Кто вызывает (фаза)

| Вызов | Файл:строка |
|---|---|
| `CreateTypifiedParseTree(guid, options)` → `new \u0017.\u000E(...).\u0001(pou)` | `...\PreCompile\Typification\PreCompileTypifier.cs:76-78` |
| `TypifyStatement(stmt, guid, options)` → `SimpleTypeChecker` затем `new \u0017.\u000E(...).\u0001(stmt, sig)` | `...\PreCompile\Typification\PreCompileTypifier.cs:108-120` |
| `SimpleTypeChecker.cs:528` — `\u0017.\u000E.\u0001(initial, ctx)` для начального значения | `...\PreCompile\Typification\SimpleTypeChecker.cs:528` |

Порядок внутри precompile: `SimpleTypeChecker` (имена/базовые типы) → `\u0017.\u000E` (полная
типизация + generic) → при `options != TypesOnly` — инъекция явных конверсий
`global::\u0013.\u0005` (`PreCompileTypifier.cs:81`,`:117`).

---

## 5. `__XSTRING#` + UTF-8: как кодировка (не) влияет на выбор `STRING`/`WSTRING`

### Путь сканер → фабрика → типизация

1. **Сканер.** `InternalScanner.ScanTypedLiteral`, ветка `case 243` (`__XSTRING`): только при
   следующем символе `"` ставится `TokenType.XByteString = 22` (см. док.15 §6.2). Декод —
   `GetXByteString` (`InternalScanner.cs:1467-1490`):
   `UnescapeString(..., bDoubleByte: **false**)` → **однобайтное** декодирование (2 hex на `$hh`),
   `StringEncoding` не возвращается.
2. **Фабрика.** `Parser35220.plugin\...\Utilities\FactoryExtension.cs:43` `CreateLiteralExpression`
   диспетчеризует по `token.Type`:
   - `case 22`: `:140-145` — `typeClass = 42` (**XString**), вызов
     `factory.CreateLiteralExpression(xbyteString, 42, token)` — **без** `StringEncoding`;
   - `case 17` (`SingleByteString`): `:128-130` → `GetSingleByteString(token, out StringEncoding, out bool)`
     → `:184-213` — здесь и только здесь `StringEncoding` (Default=0 / UTF8=1) попадает в литерал;
     `UTF8#'…'` даёт `StringEncoding.UTF8` (`InternalScanner.cs:1410-1423`), `UCHAR#` → флаг UChar.
3. **Типизация.** `TypeCompiler.\u0001(_IXStringType)` (`...Phase1_Typification\TypeCompiler.cs:733-745`):

   ```csharp
   if (Comcon.IsDefined("NO_UNICODE_SUPPORT")) {
       _IStringType t = builder.CreateStringtype();  t.Length = xstring.Length; GeneratedType = t; // STRING
   } else {
       _IWStringType t = builder.CreateWStringtype(); t.Length = xstring.Length; GeneratedType = t; // WSTRING
   }
   ```

   Подтверждено IL (token `0600322F`): `ICompileContext18::IsDefined("NO_UNICODE_SUPPORT")`;
   ветви вызывают `_IStringType ?.\u0003::\u0001()` и `_IWStringType ?.\u0003::\u0001()`; в обоих случаях
   копируется только `_IXStringType::get_Length()`.

### Вывод

- Тип `__XSTRING` **не хранит `StringEncoding`** — только `Length`; скан литерала `__XSTRING#`
  идёт однобайтно (`bDoubleByte:false`) и кодировку не несёт.
- Выбор `StringType`/`WStringType` определяется **исключительно define `NO_UNICODE_SUPPORT`**
  (`не определено` → `WSTRING`, определено → `STRING`), **кодировка/UTF-8 на выбор не влияют**.
- `StringEncoding.UTF8` относится к однобайтным литералам (`UTF8#'…'`, token 17) и к runtime-байтам
  `STRING` (см. док.15 §8), но не к `__XSTRING#`.
- Предупреждение 555 `Wrn_NonAsciiStringLiteral` (`FactoryExtension.cs:202`) для XString не
  применяется — оно только в ветке `case 17`.

---

## Что осталось неясным

1. **Базовый порядок `m_alCompiledPOUs`** прослежен как «порядок вставки»
   (`CompileContext.cs:2097`/`:2268`), но сама траектория обхода объектов проекта (кто и в каком
   порядке вызывает `AddCompiledPOUSimple`/`AddCompiledPOU`) до конца не пройдена — для 1:1 нужен
   этот порядок как «базовый индекс».
2. **Порядок глобальной агрегации сообщений**: `Messages.\u0001` не трассирован; не проверено,
   сортируются ли сообщения перед выдачей. Даже с детерминированной очередью порядок завершения
   воркеров может менять порядок сообщений.
3. **Phase5 single-thread**: при `num<=1` сортировка не применяется (база — `GetCompiledPOUsToCompileEx`).
   Это ещё один источник расхождения порядка между одно- и многопоточным режимами.
4. **Правило `\u0017.\u000E` для не-generic выражений** делегировано `\u001D.\u0005.\u0001`
   (резолв типа оператора) — сам алгоритм не разбирался (вне этой задачи).
5. **cp1252 на .NET Core**: подтверждено поведение .NET Framework (pass-through C1 для 5
   undefined-байт). Поведение .NET 5+ (`U+FFFD` или `?`) для CODESYS 3.5.22 Desktop неактуально,
   но не проверялось.
6. **Phase6 `\u0002()` (CompactDownload)** и `\u0003()`/`\u0004()` в FastOnlineChange детально
   разобраны только по коду; влияние на итоговые байты не верифицировано live-компиляцией.

---

## Ссылки

- `docs\14_COMPILER_PHASES.md` — фазы C1..C6, `TypeAcceptor`/`GenericTypeReplacer`.
- `docs\15_STRING_LITERALS.md` — токены строк, escape, `__XSTRING#`, cp1252-контекст.
- `tables\cp1252.csv` — явная таблица 0x80..0x9F (проверена через .NET/Windows-1252).
- `tables\phase_worker_order.csv` — порядок обхода Phase4/Phase5/B0.
- `decompiled\Compiler35220.plugin\...\CompilerPhases\CompilerPhase4_Typechecker.cs`,
  `CompilerPhase5_Codegenerator.cs`, `CompilerPhaseControllerGenerateCode.cs`;
  `decompiled\Compiler35220.plugin\-\-.143.cs`, `-\-.402.cs`, `-\-.399.cs`, `-\-.398.cs`;
  `decompiled\LanguageModelManager.plugin\...\CompileContext.cs`;
  `Parser35220.plugin\...\Scanner\InternalScanner.cs`, `Utilities\FactoryExtension.cs`.
