# 13. Как `__XSTRING(n)`/`__XWSTRING(n)` получает целое `n` (`TypeHelper.GetInt`)

Область: resolution `__XSTRING` (`Parser35210`) → `TypeCompiler35220` → размер через
`StringType`/`WStringType.Size()` → `TypeHelper.GetInt(m_expSize, …)` → константная свёртка
(`IConstantFolder3`, `Compiler35220`).

> Ссылки `файл:строка` — на `C:\Codesys\decompiled\...` (dnSpy-export) и
> `C:\Codesys\binaries\*.dll` (IL через `tools\dump_method_il.ps1`).
> Имена `\u00XX` — обфускация SmartAssembly; `\u0002.\u0002` = constant folder.

Вход/итог для машинной обработки: `C:\Codesys\tables\string_size_eval.csv`.

---

## 0. Главный вывод (TL;DR)

1. `TypeHelper.GetInt` — **не** «вычислитель выражений». Он просит у AST-узла
   `_IExpression2.LiteralWithRecursionCheck(...)` (или `_IExpression.Literal(...)`) уже
   готовый `ILiteralValue`, и превращает его в `int` через `ILiteralValue.GetInt(out bValid)`.
2. Вся «постоянная свёртка» — **вне** `TypeHelper`: делегируется `IConstantFolder3`
   (`Compiler35220`, класс `\u0002.\u0002`), который регистрируется в
   `VersionedCompilerFactory._ConstantFolder_OrNull`. `TypeHelper` лишь проверяет
   `literalValue != null && !recursionError` и знаковость литерала.
3. Контракт возврата жёсткий: **`-1` + `bValid=false` = ошибка/не-константа**; любое
   валидное целое (в т.ч. сами `-1`/`0`) отдаётся как есть, `bValid=true`.
4. `n` — это **свёрнутое целое значение** выражения в скобках. Скобок нет ⇒
   `m_expSize == null` ⇒ `GetInt` даёт `-1/bValid=false` ⇒ **default**.
5. Формулы: `STRING` → `n+1` (default **81**); `WSTRING` → `(n+1)*2` (default **162**,
   а до компилятора 3.5.3.50 — **81**). `__XSTRING` резолвится в `WSTRING` (или `STRING`
   при `NO_UNICODE_SUPPORT`), поэтому **`__XSTRING` без скобок = 162**.

---

## 1. Цепочка вычисления

```
ParseXStringType            (Parser35210\...\TypeParser.cs:334-354)
  -> XStringType.m_expSize = <expr>  ИЛИ null (нет '(' )
TypeCompiler.\u0001(_IXStringType)   (Compiler35220\...\TypeCompiler.cs:733-745)
  -> WStringType.Length = m_expSize          (обычно)
     StringType.Length  = m_expSize          (если IsDefined("NO_UNICODE_SUPPORT"))
WStringType.Size/SizeChecked/SizeWithRecursionCheck  (WStringType.cs:109-181)
  -> TypeHelper.GetInt(m_expSize, scope as IScope5, ...)   (TypeHelper.cs:171-222)
       -> IExpression.Literal / IExpression2.LiteralWithRecursionCheck
            -> IConstantFolder3 (зарегистрированный Compiler35220)   [свёртка]
       -> ILiteralValue.GetInt(out bValid)                        [int-каст]
  -> (n+1)*2  либо  DefaultSize*2 (162) при !bValid / n<=0 / null
```

`m_expSize` объявлен в трёх типах одинаково:
`[DefaultSerialization("SizeExpression")] private _IExpression m_expSize;`
(`XStringType.cs:88-91`, `StringType.cs:208-211`, `WStringType.cs:271-274`).

---

## 2. `TypeHelper.GetInt` — точное поведение

Файл: `decompiled\LanguageModelManager.plugin\_3S\CoDeSys\LanguageModelManager\TypeHelper.cs`

### 2.1 Четыре перегрузки (сигнатуры + token)

| # | строка | сигнатура | token |
|---|---|---|---|
| 1 | `TypeHelper.cs:171-174` | `static int GetInt(IExpression exp, IScope scope, out bool bValid)` | `06001BE3` |
| 2 | `TypeHelper.cs:177-193` | `static int GetInt(IExpression exp, IScope scope, bool bAllocatedOK, IRecursionGuard recursionGuard, out bool bValid)` | `06001BE4` |
| 3 | `TypeHelper.cs:196-206` | `static int GetInt(IExpression exp, IScope scope, bool bAllocatedOK, out bool bValid)` | `06001BE5` |
| 4 | `TypeHelper.cs:209-222` | `static int GetInt(IExpression exp, IPrecompileScope scope, out bool bValid)` | `06001BE6` |

«`TypeHelper.GetInt(m_expSize)`» из задачи — это собирательное имя; реально всегда
вызывается одна из 4 перегрузок (для размера строк — №2 или №3, см. §4).

### 2.2 Семантика (декомпил + IL)

Перегрузка №2 (главная, с guard), `TypeHelper.cs:177-193`:

```csharp
bValid = false;
if (exp == null) return -1;                                   // null -> -1, invalid
bool flag = false;                                            // flag = recursionError
_IExpression2 iexpression = exp as _IExpression2;
ILiteralValue literalValue = (iexpression != null)
    ? iexpression.LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out flag)
    : null;
bValid = (literalValue != null && !flag);                     // null ЛИБО рекурсия -> invalid
if (!bValid) return -1;
return literalValue.GetInt(out bValid);                       // int-каст, переустанавливает bValid
```

IL (`LanguageModelManager.plugin.dll::TypeHelper::GetInt` token `06001BE4`):

```
IL_0000: ldarg.s bValid
IL_0002: ldc.i4.0
IL_0003: stind.i1
IL_0004: ldarg.0
IL_0005: brtrue.s IL_0009
IL_0007: ldc.i4.m1          ; -1
IL_0008: ret
IL_0009: ...
IL_000C: isinst _...InternalInterfaces._IExpression2
IL_001D: callvirt ILiteralValue _IExpression2::LiteralWithRecursionCheck(IScope,IRecursionGuard,Boolean,Boolean&)
IL_0026: brfalse.s IL_002E
IL_0028: ldloc.0            ; recursionError
IL_0029: ldc.i4.0
IL_002A: ceq
IL_002F: stind.i1           ; bValid = (literal != null && !flag)
IL_0033: brtrue.s IL_0037
IL_0035: ldc.i4.m1
IL_0036: ret
IL_003A: callvirt Int32 ILiteralValue::GetInt(Boolean&)
IL_003F: ret
```

Перегрузка №3 (`TypeHelper.cs:196-206`) — та же идея, но через
`_IExpression.Literal(scope, bAllocatedOK)` и **без** recursion-guard:

```csharp
bValid = false;
_IExpression iexpression = exp as _IExpression;
ILiteralValue literalValue = (iexpression != null) ? iexpression.Literal(scope, bAllocatedOK) : null;
if (literalValue == null) return -1;
return literalValue.GetInt(out bValid);
```

Перегрузка №1 (`TypeHelper.cs:171-174`) просто делегирует №3 с `bAllocatedOK=false`.
Перегрузка №4 (`TypeHelper.cs:209-222`) — то же для `IPrecompileScope`
(`_IExpression.Literal(IPrecompileScope)`); в размере строк не участвует.

### 2.3 Что возвращается (таблица)

| ситуация | возврат | `bValid` |
|---|---|---|
| `exp == null` (нет скобок) | `-1` | `false` |
| `exp` не `_IExpression`/`_IExpression2` | `-1` | `false` |
| не-константа (`Literal*` вернул `null`) | `-1` | `false` |
| рекурсивная константа (`flag=true`) | `-1` | `false` |
| свёрнутый целочисленный литерал (в диапазоне) | значение | `true` |
| литерал `-1` (реальный) | `-1` | **`true`** (отличимо от ошибки только по `bValid`) |
| литерал `< int.MinValue` или `> uint.MaxValue` | `-1` | `false` |
| `Float` / `Bool` / `None` | `-1` | `false` |

Ключевое: **`-1` неоднозначен**; потребитель обязан смотреть `bValid`.

---

## 3. `ILiteralValue.GetInt` (нижний слой)

В проекте две эквивалентные реализации, возвращаемые фабрикой литералов:

- struct `_3S.CoDeSys.LanguageModelManager.LiteralValue` (`LiteralValue.cs:240-273`) —
  основная (её `box` виден в IL `VariableExpression::LiteralWithRecursionCheck` IL_008C
  `newobj LiteralValue::.ctor(Boolean); box`).
- class `_3S.CoDeSys.LanguageModelUtilities.LiteralValue` (`LiteralValue.cs:155-188`) —
  утилитарная копия.

Логика `GetInt(out bool bValid)` (обе):

```csharp
KindOfLiteral.SignedInteger:
    if (v < int.MinValue || v > uint.MaxValue) return -1;   // invalid, bValid=false
    result = (int)v;                                        // bValid=true
KindOfLiteral.UnsignedInteger:
    if (v > uint.MaxValue) return -1;                       // invalid
    result = (int)v;                                        // bValid=true
KindOfLiteral.Float / Bool / None:  return -1;              // invalid
```

Конструкторы: `long` → `SignedInteger`, `ulong` → `UnsignedInteger`,
`double` → `Float`, `bool` → `Bool`, `string` → `String` (`LiteralValue.cs:11-73`).
Т.е. тип целого берётся из типа выражения-литерала, а не из размера.

**Следствие для размера:** `n` берётся как `(int)` от свёрнутого целого. Всё, что не
целое (REAL/BOOL/STRING) или вне `[int.MinValue, uint.MaxValue]`, — не «size».

---

## 4. Как это вызывают `StringType`/`WStringType`

### 4.1 `StringType` (`STRING`, `StringType.cs:111-150`)

```
SizeWithRecursionCheck(scope, recursionGuard, out bValid):
  if m_expSize == null                -> return DefaultSize (81)                     :121-123
  if recursionGuard != null           -> GetInt(...,true,recursionGuard,out)   (№2)   :135
  else if comp >= 3.5.7.0             -> GetInt(...,true,out)                  (№3)   :139
  else                                -> GetInt(...,out)                       (№1)   :143
  if !bValid || n < 0                 -> return 81                                   :145-147
  return n + 1                                                                        :149
DefaultSize => CompilerConstants.StringTypeDefaultSize (=81)                          :95-101
```

### 4.2 `WStringType` (`WSTRING`, `WStringType.cs:109-181`)

```
SizeWithRecursionCheck(scope, recursionGuard, out bValid):
  if m_expSize == null:
      comp >= 3.5.3.50 -> return 81*2 = 162        :112-117
      else             -> return 81
  else:
      guard dup/Has -> bValid=false; return 81     :122-131
      n = GetInt(m_expSize, scope, true, recursionGuard, out bValid)   (№2)  :132
      if !bValid || n <= 0 -> return 81*2 = 162    :133-136
      return (n+1)*2                               :137

SizeChecked(scope,out)  -> GetInt(...,out) (№1)  ; !bValid||n<=0 -> 162 ; (n+1)*2   :142-162
Size(scope)             -> GetInt(...,out) (№1)  ; comp>=3.5.15.0: n<0 -> 162, иначе n<=0 -> 162 ; (n+1)*2  :165-181
DefaultSize => CompilerConstants.WStringTypeDefaultSize (=81)                       :100-106
```

`CompilerConstants`: `StringTypeDefaultSize => 81`, `WStringTypeDefaultSize => 81`
(`Compiler\_3S\CoDeSys\LanguageModelManager\InternalInterfaces\CompilerConstants.cs:8,10`).

**Асимметрия нуля (важно):** `StringType` использует `n < 0` (ноль допустим → size 1),
а `WStringType.SizeWithRecursionCheck`/`SizeChecked` — `n <= 0` (ноль → default 162).
Только `WStringType.Size` при `comp >= 3.5.15.0` переходит на `n < 0`.

IL `WStringType::SizeWithRecursionCheck` (token `06001D1C`), ключевой хвост:

```
IL_004E: ; n = TypeHelper.GetInt(m_expSize, scope as IScope5, true, recursionGuard, out bValid)
IL_0063: ldarg.3
IL_0064: ldind.u1          ; bValid
IL_0065: brfalse.s IL_006B ; !bValid -> default
IL_0067: ldloc.0
IL_0068: ldc.i4.0
IL_0069: bgt.s IL_0073     ; n > 0 -> compute
IL_006B: get_DefaultSize   ; == 81
IL_0070: ldc.i4.2
IL_0071: mul               ; 162
IL_0072: ret
IL_0073: ldloc.0
IL_0074: ldc.i4.1
IL_0075: add
IL_0076: ldc.i4.2
IL_0077: mul               ; (n+1)*2
IL_0078: ret
```

### 4.3 Резолв `__XSTRING` (`Compiler35220\...\TypeCompiler.cs:733-745`)

```csharp
public void \u0001(_IXStringType \u0002) {
    if (this.Comcon.IsDefined("NO_UNICODE_SUPPORT")) {
        _IStringType  s = \u0019.\u0003.\u0001(); s.Length = \u0002.Length; GeneratedType = s; return;
    }
    _IWStringType w = \u0019.\u0003.\u0001(); w.Length = \u0002.Length; GeneratedType = w;
}
```

`.Length` — это то же `m_expSize` (get/set над `m_expSize`: `XStringType.cs:29-39`).
Поэтому вся арифметика размера — уже у `WStringType`/`StringType` (см. §4.1-4.2),
сам `XStringType` `Size` не переопределяет (`XStringType.cs:14`, док. 10 §8.3).

### 4.4 Парсинг (наличие/отсутствие скобок) — `Parser35210\...\TypeParser.cs:334-354`

```csharp
_IXStringType t = LMItemFactory.CreateXStringtype();
Next(out token); Scanner.SetPosition(token);
if (MatchOperator(false, Operator.LeftParenthesis) == Operator.None) {
    Scanner.SetPosition(token); return t;          // m_expSize ОСТАЁТСЯ null
}
_IExpression length = ExpressionParser.ParseAssignment(out bError);
if (bError) { AddErrorIF(token, MessageId.Err_StringSizeExpected); return t; }  // m_expSize null
t.Length = length; MatchOperator(Operator.RightParenthesis); return t;
```

То же для `WSTRING` (`TypeParser.cs:356-376`) и `STRING` (`:378-398`), причём `STRING`
принимает и `(...)`, и `[...]`.

---

## 5. Откуда берётся свёртка (`IConstantFolder3`)

`TypeHelper` не сворачивает. Регистрация и реализация:

- `VersionedCompilerFactory._ConstantFolder_OrNull` (`VersionedCompilerFactory.cs:243-257`):
  `(FindFactoryForCompilerVersion() as IConstantFoldingFactory).CreateConstantFolder()`.
- `Compiler35220\...\Services\CompilerServices.cs:89-92`:
  `public IConstantFolder CreateConstantFolder() => new global::\u0002.\u0002();`
- Сам фолдер: `Compiler35220.plugin\-\-.46.cs:21`
  `internal sealed class \u0002 : IConstantFolder3, IConstantFolder2, IConstantFolder`.
- Интерфейсы: `Compiler\...\IConstantFolder3.cs:9-21` (`GetLiteralValue(...)`,
  `LiteralWithRecursionCheck(...)`), `IConstantFolder2.cs`, `IConstantFolder.cs`.

`IConstantFolder3.GetLiteralValue` путь для скалярных выражений реализован в
`\u0080.\u0001` (visitor), файл `Compiler35220.plugin\<0x80>\-.cs`:

- вход: `internal static _ILiteralValue \u0001(_IExpression2, \u0002.\u0002 folder, ICommonScope, IRecursionGuard)` (`:43-61`);
- `_ILiteralExpression` → `new ConstantFoldingResult(e.LiteralValue)` (`:419-422`);
- `_IVariableExpression` → через scope достаёт `Initial` и сворачивает рекурсивно (`:425-440`);
  не найдено initial → `null` (=> не-константа) (`:434-438`);
- `_IOperatorExpression` → `folder.\u0001(op, scope, guard, true, out flag)` (`:390-399`);
  в фолдере `-\-.46.cs:240-290` арифметика берётся из операндов и `ConstantFoldingHelper`.
- `LiteralExpression.LiteralWithRecursionCheck` напрямую зовёт `GetLiteralValue`
  (`LiteralExpression.cs:323-356`).
- `OperatorExpression.LiteralWithRecursionCheck` для `IScope` зовёт
  `constantFolder.LiteralWithRecursionCheck(this, scope, guard, bAllocatedOK, out eResult)`
  (`OperatorExpression.cs:634-711`), а `Literal(IScope,bool)` → base путь через
  `GetLiteralValue` (`:869-880`).

Флаги результата `EConstantFoldingResult` (`EConstantFoldingResult.cs:9-17`):
`None=0`, `RecursionError=1`, `Overflow=2`. Из них `TypeHelper` (через
`out bRecursionError`) учитывает **только `RecursionError`**; `Overflow` наружу по этому
пути не выносится (см. §7).

### `bAllocatedOK` — значение и где реально работает

- Перегрузка №2 (`TypeHelper.cs:177-193`) передаёт `bAllocatedOK` в
  `_IExpression2.LiteralWithRecursionCheck`.
- Для `IScope`-ветки `VariableExpression` (`VariableExpression.cs:445-451`) при
  зарегистрированном фолдере **сразу** уходит в `constantFolder.GetLiteralValue(...)` —
  маска флагов `Constant|ReplacedConstant` (`:470`) недостижима. IL подтверждает
  фиксированный `ldc.i4.s 96` в этой ветке (token `060007D8`, IL_00AF-00B7).
- `bAllocatedOK` реально влияет только в `IPrecompileScope`-ветке
  `VariableExpression.cs:503-511` (`96` если true, иначе `32`), т.е. на precompile-путь.
- Вывод: для размера строк (`IScope5`) `bAllocatedOK` фактически **dead**;
  CONST-переменные разрешаются всегда, что видно и по `StringType.cs:135/139`
  (`true`), и по `WStringType.cs:132` (`true`).

---

## 6. Формула и граничные случаи

### Формула (эффективный размер в байтах)

```
STRING(n)     = (n < 0 || !valid) ? 81  : n + 1
WSTRING(n)    = (!valid || n <= 0) ? 162 : (n + 1) * 2        // SizeWithRecursionCheck/SizeChecked
WSTRING(n)    = (!valid || n <  0) ? 162 : (n + 1) * 2        // Size(), только >= 3.5.15.0
__XSTRING(n)  = WSTRING(n)                                     // обычно
__XSTRING(n)  = STRING(n)                                      // NO_UNICODE_SUPPORT
отсутствие скобок: m_expSize == null => valid=false => default (81 / 162)
```

Версии-гейты (`CompilerVersionManager.cs`):
`V35350=3.5.3.50` (`:1584,2714`), `V35600=3.5.6.0` (`:1592,2690`),
`V35700=3.5.7.0` (`:1599,2669`), `V35900=3.5.9.0` (`:1603,2657`),
`V351500=3.5.15.0` (`:1621,2603`), `V352000=3.5.20.0` (`:1636,2558`).

### Граничные случаи (сводка; полная машиночитаемая таблица — `string_size_eval.csv`)

| случай | что происходит | итог |
|---|---|---|
| нет `(` | `m_expSize == null`; `GetInt` → `-1/false` | STRING 81, WSTRING/`__XSTRING` 162 |
| `(k)`, k ≥ 1 | свёртка → `k` | `k+1` / `(k+1)*2` |
| `(0)` | `bValid=true, n=0` | `SizeWithRecursionCheck`/`SizeChecked`: 162; `Size` (≥3.5.15.0): 2 |
| `(-1)` / любой `< 0` | `bValid=true, n<0` | 81 / 162 |
| `(> 4294967295)` | `GetInt` → `-1/false` | default |
| REAL/BOOL/строковый литерал | `GetInt` → `-1/false` | default |
| не-константа (обычная `VAR`, вызов, неразрешённый идентификатор) | `Literal*` → `null` | default |
| рекурсивная/циклическая константа | `RecursionError` → `null`, `bValid=false` | default |
| const-выражение `(2+3)` | свёртка → `5` | как `(5)` |
| CONST-переменная `(ciVar)` | разворачивается до `Initial` и сворачивается | как у значения |
| `NO_UNICODE_SUPPORT` | резолв в `STRING` | `n+1`, default 81 |

---

## 7. Доказательства (`file:line`)

- `TypeHelper.GetInt`: `TypeHelper.cs:171-174, 177-193, 196-206, 209-222`; IL tokens
  `06001BE3/BE4/BE5/BE6` (dump получен из `binaries\LanguageModelManager.plugin.dll`).
- `LiteralValue.GetInt`: `LanguageModelManager\LiteralValue.cs:240-273`;
  `LiteralValue.cs:11-73` (ctor); `LanguageModelUtilities\LiteralValue.cs:155-188`.
- Размеры строк: `StringType.cs:95-101, 111-150`; `WStringType.cs:100-106, 109-181`;
  IL `06001D00` (String), `06001D1C` (WString).
- `CompilerConstants.cs:8,10` (`81`/`81`).
- `m_expSize`: `XStringType.cs:88-91`; `StringType.cs:208-211`; `WStringType.cs:271-274`.
- Резолв: `TypeCompiler.cs:733-745`.
- Парсинг: `TypeParser.cs:334-354, 356-376, 378-398`.
- Свёртка: `VersionedCompilerFactory.cs:243-257`; `CompilerServices.cs:89-92`;
  `Compiler35220.plugin\-\-.46.cs:21,138-173,176-207,226-290,442-464`;
  `Compiler35220.plugin\<0x80>\-.cs:43-61,390-440`; `EConstantFoldingResult.cs:9-17`.
- Скалярные узлы: `Expression.cs:200-262`; `LiteralExpression.cs:323-356, 358-...`;
  `OperatorExpression.cs:556-631, 634-711, 869-895`; `VariableExpression.cs:412-478, 483-534`.
- Альтернативный (не задействованный для `Size`) путь: `SizeCalculatorHelpFunctions.cs:60-72`;
  `PreCompileUtilities.cs:424-447` (`_GetIntValue`).

---

## 8. Что осталось неясным / нужно IL-подтверждение

1. **`Overflow` игнорируется на пути размера.** `EConstantFoldingResult.Overflow=2`
   выставляется фолдером, но `OperatorExpression.\u0001(..., out bool bRecursionError)`
   извлекает только `RecursionError` (`-\-.46.cs:227-237`), и перегрузка `TypeHelper`
   тоже смотрит лишь `bRecursionError`. Возможно, переполнение даёт `int`, прошедший
   `GetInt` (с «заворачиванием»), а возможно — `null`. Нужен IL/эксперимент с
   `__XSTRING(2147483647+1)`.
2. **Свёртка не-CONST `VAR` с инициализатором.** Visitor-ветка `_IVariableExpression`
   (`<0x80>\-.cs:425-440`) не проверяет флаг `Constant` и пытается достать `Initial`
   через scope. Проверка «константности» может быть спрятана в
   `Scope.\u0001(_IExpression2)` (гейт `:51`). Не подтверждено, свернётся ли
   `VAR x : INT := 5; __XSTRING(x)`.
3. **`Scope.GetInitialExpression` / `global::\u0017.\u0006`** (обфусцированный scope
   Compiler35220) — точные условия резолва initial не декомпилированы (модуль 2.4 МБ
   не грузится dnlib в текущем окружении — OOM).
4. **Реальный тип `ILiteralValue`** (struct `LanguageModelManager.LiteralValue` vs class
   `LanguageModelUtilities.LiteralValue`) для конкретного `m_expSize`. IL
   `VariableExpression` показывает `box LanguageModelManager.LiteralValue`, но
   фабрика `\u0019.\u0003.\u0001(...)` может отдавать и утилитарную копию. Обе
   `GetInt` семантически идентичны — на результат не влияет.
5. **`STRING` vs `WSTRING` ноль.** Не подтверждено рантаймом, что `STRING(0)` на
   практике даёт 1 байт (символ NUL), а `WSTRING(0)` в части путей — 162.
