# 20 — Типизация операторного выражения (`_IOperatorExpression`)

Дата: 2026-09-29. Источник: `C:\Codesys\binaries\Compiler35220.plugin.dll` (managed),
декомпил — `C:\Codesys\decompiled\Compiler35220.plugin\`. Дисассемблер: dnlib
(`tools\dump_method_by_token.ps1`). Цель — пошаговый алгоритм выбора результирующего
типа оператора для 100% идентичного порта на Rust.

> В `decompiled\Compiler35220.plugin\-\-.NNN.cs` имена типов/методов обфусцированы
> (`\u001D.\u0005` = namespace U+001D, класс U+0005). Числовой суффикс escape'а =
> кодовая точка: `\u0093` = метод с именем U+0093 (десятичное 147) и т.д.

---

## 0. Карта вызовов

```
_IOperatorExpression
  └─ \u0017.\u000E.\u0001(_IOperatorExpression)              -.143.cs:400   (visitor entry)
       ├─ рекурсивно типизирует операнды (op._OperandsList)
       └─ op.AcceptOperatorVisitor(this)                     -.143.cs:416
            └─ оператор-специфичный visitXxx (LMM dispatch)   OperatorExpression_Green.cs:108
                 ├─ прямое присвоение op.Type                 (Bool/DWord/Int/...)
                 └─ \u0017.\u000E.\u0001(op, startIdx, flag, operands)   -.143.cs:1670
                      └─ GLOBAL::\u001D.\u0005.\u0001(op, flag, startIdx, operands, preferredType, scope, ctx)
                                                               -.151.cs:15 / :21
                           ├─ \u001D.\u0005.\u0001(op, operands, ctx)          -.151.cs:144  (X-type / pointer special)
                           ├─ \u001D.\u0005.\u0001(scope, operands, idx)       -.151.cs:186  (strict-enum)
                           ├─ \u001D.\u0005.\u0001(flag, scope, acc, e, t)     -.151.cs:107  (accumulate operand)
                           │    └─ \u001D.\u0005.\u0001(e, acc)               -.151.cs:232  (literal accumulate)
                           ├─ \u001D.\u0005.\u0001(op, preferred, acc)         -.151.cs:62   (finalize signedness)
                           ├─ \u001D.\u0005.\u0001(op, acc, out _IType)        -.151.cs:84   (bool/bit/real decision)
                           ├─ \u001D.\u0005.\u0001(preferred, ctx, literals)   -.151.cs:276  (real constant fallback)
                           ├─ \u001D.\u0005.\u0001(signed, size)               -.151.cs:298  (size -> type)
                           └─ \u001D.\u0005.\u0001(op)                         -.151.cs:344  (is-comparison)
```

Точка входа в типизацию (в визиторе типизации) — **`-.143.cs:394`**:

```csharp
private _IType \u0001(Operator \u0002, bool \u0003, int \u0004, IList<_IExpression> \u0005, ICompiledType \u0006)
{
    return global::\u001D.\u0005.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006, this.Scope, null);
}
```

Параметры: `op`, `flag` (разрешать ли «знаковую» промоцию литералов), `startIdx`
(индекс первого учитываемого операнда; для MUX/SEL = 1), `operands`, `preferredType`.

Центральный хелпер визитора (`-.143.cs:1670`, token `0x06001CFC`):

```csharp
internal void \u0001(_IOperatorExpression op, int startIdx, bool flag, IList<_IExpression> operands)
{
    if (op.Type == null) op.Type = this.\u0001(op.Code, flag, startIdx, operands, null);
    if (op.Type == null)                       // fallback
    {
        if (operands.Count == 0) { op.Type = TypeTable.Get(TypeClass.Int); return; }
        op.Type = operands[0].Type.DeRefType;
    }
}
```

---

## 1. Диспетчеризация оператора (Operator.Code → visitXxx)

`decompiled\LanguageModelManager.plugin\_3S\CoDeSys\LanguageModelManager\GreenTrees\OperatorExpression_Green.cs:108`
(`AcceptOperatorVisitor`). Группы и соответствующие методы обфусцированного визитора
(`-.143.cs`, mapping получен через dnlib `Overrides`):

| Operator-группа | visitXxx | обфусц. метод | строка `-.143.cs` |
|---|---|---|---|
| Add,Sub,Mul,Div,Mod,Plus,Minus,Times,Divide,Power | visitArithmetics | `\u0094` | 1592 |
| Eq,Ne,Ge,Gt,Le,Lt,Less,Greater,LessEqual,GreaterEqual,Equal,NotEqual | visitComparisons | `\u0092` | 1507 |
| And,AndN,Or,OrN,Xor,XorN,Not,Ampersand,VerticalLine,And_Then,Or_Else | visitBoolOps | `\u0093` | 1513 |
| Limit,Min,Max,Mux,Sel | visitSelection | `\u0099` | 1891 |
| Rol,Ror,Shl,Shr | visitShiftOps | `\u008B` | 1405 |
| Exp,Expt,Sqrt,Ln,Log,Sin,Cos,Tan,ASin,ACos,ATan | visitTrigonometrics | `\u0091` | 1482 |
| Time | visitTime | `\u0089` | 1393 |
| LTime | visitLTime | `\u008A` | 1399 |
| __vc* (13 операторов) | visitVector | `\u0095` | 1605 |
| Adr,BitAdr,IndexOf,SizeOf,Ini,Abs,Trunc,TruncInt,Move,TestAndSet,__* | прямые методы | `\u0002`..`\u0090` | 1048–1478 |

Полный машиночитаемый список «operator → правило → тип результата»:
`tables\operator_type_rules.csv` (292 строки данных, см. §5).

Операторы-типы (`Byte`, `Int`, `Bool`, …) в `AcceptOperatorVisitor` уходят на
`IL_50B: Debug.Assert(false)` — они НЕ типизируются как оператор, а обрабатываются
как `_IConversionExpression`/`_ICastExpression`.

---

## 2. Алгоритм `\u001D.\u0005.\u0001` (token `0x06001DC6`, RVA 0x6084C)

Источник: `decompiled\Compiler35220.plugin\-\-.151.cs:21`. IL получен
`tools\dump_method_by_token.ps1 -Tokens 06001DC6`.

### Шаг 1. X-type / pointer для сравнений (`-.151.cs:144`, token `0x06001DCA`)

```csharp
if (ctx == null || !(ctx.ApplicationGuid == Guid.Empty)
    || !isComparison(op) || operands.Count != 2
    || (!IsResolvedXType(operands[0]._CompiledType) && !IsResolvedXType(operands[1]._CompiledType))
    || (operands[0]._CompiledType.Class != Pointer && operands[1]._CompiledType.Class != Pointer))
    return null;
return IsResolvedXType(operands[0]._CompiledType)
     ? (operands[0]._CompiledType as _IType)
     : (operands[1]._CompiledType as _IType);
```

Правило: **comparison**, ровно 2 операнда, один — resolved X-type
(`_IXDWordType/_IXLWordType/_IXUDIntType/_IXULIntType/_IXDIntType/_IXLIntType`),
второй — `Pointer` (и контекст без ApplicationGuid) ⇒ результат = resolved X-type.
IL:

```
IL_0011: ... Guid::op_Equality -> brfalse IL_00B9   (ctx.ApplicationGuid == Guid.Empty)
IL_001C: call \u001E.\u000E::\u0001(Operator)       (isComparison)
IL_0039: call TypeTable::IsResolvedXType
IL_005F: callvirt IType::get_Class ; ldc.i4.s 22     (TypeClass.Pointer)
IL_00A1: isinst _IType ; ret
IL_00B9: ldnull ; ret
```

### Шаг 2. strict-enum (`-.151.cs:186`, token `0x06001DCD`)

```csharp
if (scope == null || operands.Count <= idx+1) return false;
if (operands[idx] == null || operands[idx].Type == null) return false;
enum = UEnumType(operands[idx].Type);
if (enum == null) return false;
sig = scope.FindSignature(enum);
if (sig == null || !sig.HasAttribute("strict")) return false;
for (i = idx+1 .. count-1)
    if (!SameEnumSignature(enum, operands[i])) return false;   // _IEnumType.SignatureId >= 0 && равны
return true;
```

Ветка в главном методе (`-.151.cs:28`):

```csharp
if (\u001D.\u0005.\u0001(scope, operands, startIdx))
    return operands[startIdx].Type as _IType;
```

Правило: если базовый операнд — пользовательский enum с атрибутом `strict` и все
последующие операнды того же enum-сигнатуры ⇒ **тип результата = сам enum-тип**
(а не BOOL). `UEnumType` (token `0x06001DCB`, `-.151.cs:158`) разворачивает
`_IEnumType` либо `IReferenceType2.OriginalBase`.

### Шаг 3. Спец-обработка X-type операнда (`-.151.cs:33-43`)

```csharp
for (i = startIdx; i < operands.Count; i++)
{
    e  = operands[i];
    Debug.\u0002(e.Type != null);
    dt = e.Type.DeRefType;
    if (TypeTable.IsXType(dt))                 // _IXWordType/_IUXIntType/_IXIntType
        return TypeTable.Get(dt.Class);        // НЕразрешённый X-type -> его же класс-тип
    \u0001(flag, scope, acc, e, dt);           // иначе накапливаем
}
```

IL: `IL_0065 call TypeTable::IsXType` → `IL_0073 call TypeTable::Get(Class)` → `ret`.

### Шаг 4. Аккумулятор `global::\u0006.\u0004` (`-.152.cs:8`)

Поля (все — авто-свойства):

| Поле | Тип | смысл |
|---|---|---|
| `Typebiggest` | TypeClass | класс самого «широкого» операнда |
| `SizeBiggest` | int | его размер (байт) |
| `Signed` | bool | итог «знаковый» |
| `AtLeastOneSigned` | bool | был хотя бы один signed |
| `AtLeastOneUnsigned` | bool | был хотя бы один unsigned |
| `OnlyLiterals` | bool (=true) | все операнды — литералы |
| `LiteralSigned` | bool | был отрицательный литерал |
| `LiteralSignedSizeBiggest` | int | минимальный размер signed-литерала (1/2/4/8) |
| `ConcreteTypes` | bool | были литералы с concrete-типом |
| `RealType` / `LRealType` | bool | был REAL / LREAL |
| `RealConstant` | bool | только LREAL-литералы без LREAL-константного типа |

### Шаг 5. Накопление операнда (`-.151.cs:107`, token `0x06001DC9`)

```csharp
num = scope.GetSize(deRef);
if (deRef is ISpecialSizeType) num = ((ISpecialSizeType)deRef).CompatibilitySize;

if (num > acc.SizeBiggest || acc.Typebiggest==Bit || acc.Typebiggest==BitConst) {
    acc.SizeBiggest = num; acc.Typebiggest = deRef.Class;
}
if (TypeTable.IsSigned(deRef.Class)) acc.AtLeastOneSigned = true;
else                                acc.AtLeastOneUnsigned = true;
acc.Signed |= (!flag && !(e is _ILiteralExpression) && TypeTable.IsSigned(deRef.Class));
acc.RealType |= (deRef.Class == Real);
LiteralAccumulate(e, acc);                                 // шаг 5a
if (e.Type.DeRefType.Class != LReal || !(e is _ILiteralExpression)) {
    acc.LRealType |= (e.Type.DeRefType.Class == LReal); return;
}
if (((_ILiteralExpression)e).ConstantType != LReal) { acc.RealConstant = true; return; }
acc.LRealType = true;
```

IL: `IL_0003 ICommonScope::GetSize`, `IL_0019 ISpecialSizeType::CompatibilitySize`,
`IL_0056 TypeTable::IsSigned`, `IL_0074 !flag && !literal && IsSigned`,
`IL_00C6 class==LReal` (ldc.i4.s 15).

### Шаг 5a. Накопление литерала (`-.151.cs:232`, token `0x06001DCF`)

```csharp
lit = e as _ILiteralExpression;
if (lit == null) { acc.OnlyLiterals = false; return; }
acc.LiteralSigned |= lit.Negative;
if (TypeTable.IsConcreteType(lit.ConstantType)) {
    acc.Signed |= TypeTable.IsSigned(e.Type.DeRefType.Class);
    acc.ConcreteTypes = true; return;
}
if (lit.LiteralValue.GetUnsignedLong(out ok) && ok) {
    int size = v>2147483647 ? 8 : v>32767 ? 4 : v>127 ? 2 : 1;
    if (size > acc.LiteralSignedSizeBiggest) acc.LiteralSignedSizeBiggest = size;
}
```

IL: пороги `0x7FFFFFFF -> 8`, `0x7FFF -> 4`, `0x7F -> 2`, иначе `1`.

### Шаг 6. Финализация знаковости (`-.151.cs:62`, token `0x06001DC7`)

```csharp
acc.Signed = acc.Signed || (acc.OnlyLiterals && acc.LiteralSigned);
if (!acc.Signed && acc.OnlyLiterals && !acc.ConcreteTypes && (op==Sub || op==Minus)) {
    acc.Signed = true;
    if (acc.LiteralSignedSizeBiggest > acc.SizeBiggest) acc.SizeBiggest = acc.LiteralSignedSizeBiggest;
}
if (!acc.ConcreteTypes && acc.OnlyLiterals && !acc.Signed
    && preferredType != null && TypeTable.IsConcreteType(preferredType.Class)
    && TypeTable.IsSigned(preferredType.Class))
    acc.Signed = true;
if (acc.Signed && acc.LiteralSignedSizeBiggest > acc.SizeBiggest)
    acc.SizeBiggest = acc.LiteralSignedSizeBiggest;
```

IL-константы: `Operator.Sub = 123` (`ldc.i4.s 123`), `Operator.Minus = 158` (`ldc.i4 158`).

### Шаг 7. Решение bool / real (`-.151.cs:84`, token `0x06001DC8`)

```csharp
flag = isComparison(op)
    || ((op==Mux || op==Sel) && acc.Typebiggest != BitConst)
    || op==And || op==Or;

if ((acc.Typebiggest==Bit || acc.Typebiggest==BitConst || acc.Typebiggest==Bool) && flag)
    return TypeTable.Get(TypeClass.Bit);          // = 1
if (acc.LRealType) return TypeTable.Get(TypeClass.LReal);   // = 15
if (acc.RealType)  return TypeTable.Get(TypeClass.Real);    // = 14
return false;   // решение не принято -> шаг 8/9
```

IL-константы: `Operator.Mux=43`, `Operator.Sel=44`, `TypeClass.BitConst=38`,
`Operator.And=127`, `Operator.Or=129`, `TypeClass.Bit=1`, `Bool=0`,
`LReal=15`, `Real=14`.
Т.е. **сравнения/AND/OR над BOOL/BIT/BitConst дают BIT** (не BOOL!); MUX/SEL над
BOOL/BIT (кроме BitConst) — тоже BIT; любые REAL/LREAL-операнды → REAL/LREAL.

### Шаг 8. Real-константа (`-.151.cs:50-52` → `-.151.cs:276`, token `0x06001DD0`)

```csharp
if (acc.RealConstant)
    return \u0001(preferredType, ctx, acc.OnlyLiterals);
```

`\u0001(preferred, ctx, onlyLiterals)`:
```
preferred!=null && preferred.Class==LReal                       -> LReal
preferred!=null && preferred.Class==Real                        -> Real
ctx!=null && ctx.TypeIsSupported(LReal)                         -> LReal
иначе                                                           -> Real
```

### Шаг 9. Итоговый целочисленный тип (`-.151.cs:54-58`)

```csharp
if (acc.AtLeastOneSigned && acc.AtLeastOneUnsigned && acc.SizeBiggest < 4
    && isComparison(op))                      // token 0x06001DD2
    acc.SizeBiggest = 4;                      // смешанные signed/unsigned в сравнении -> минимум 4 байта
return \u0001(acc.Signed, acc.SizeBiggest);   // token 0x06001DD1
```

`isComparison` (`-.151.cs:344`, token `0x06001DD2`), IL:
```
op - 134 <= 5 (unsigned)  -> Eq..Lt          (134..139)
op - 175 <= 5 (unsigned)  -> Less..NotEqual  (175..180)
```
то есть оба набора операторов сравнения (IEC-стиль `=`/`<>`/`<`/`>` и IL-стиль
`Eq`/`Ne`/`<`/`>`).

`\u0001(signed, size)` (`-.151.cs:298`, token `0x06001DD1`) — таблица «размер → тип»:

| size | signed=true | signed=false |
|---|---|---|
| 1 | `SInt` (6) | `USInt` (10) |
| 2 | `Int` (7) | `UInt` (11) |
| 4 | `DInt` (8) | `UDInt` (12) |
| 8 | `LInt` (9) | `ULInt` (13) |
| иначе | `null` | `null` |

---

## 3. Значения `TypeClass` / `Operator`

`TypeClass` (48 членов, семантический порядок — арифметика по значениям):
`Bool=0, Bit=1, Byte=2, Word=3, DWord=4, LWord=5, SInt=6, Int=7, DInt=8, LInt=9,
USInt=10, UInt=11, UDInt=12, ULInt=13, Real=14, LReal=15, String=16, WString=17,
Time=18, Date=19, DateAndTime=20, TimeOfDay=21, Pointer=22, Reference=23,
Subrange=24, Enum=25, Array=26, Params=27, Userdef=28, None=29, Any=30, AnyBit=31,
AnyDate=32, AnyInt=33, AnyNum=34, AnyReal=35, Lazy=36, LTime=37, BitConst=38,
UXInt=39, XWord=40, XInt=41, XString=42, VarLenArray=43, AnyString=44, __Vector=45,
LDate=46, LDateAndTime=47, LTimeOfDay=48`
(см. `tables\type_system.csv:14`). Используемые в алгоритме:
`Bool=0, Bit=1, Real=14, LReal=15, Pointer=22, BitConst=38`.

`Operator` — `tables\operators.csv` (292 члена). Арифметика: `Add=122, Sub=123,
Mul=124, Div=125, Mod=126`; побитовые/логические: `And=127, AndN=128, Or=129,
OrN=130, Xor=131, XorN=132, Not=133`; сравнения: `Eq=134..Lt=139`,
`Less=175..NotEqual=180`; знак: `Plus=157, Minus=158, Times=159, Power=160,
Divide=161`; `Mux=43, Sel=44`; `And_Then=234, Or_Else=235`.

Вспомогательные предикаты `TypeTable` (`decompiled\Compiler35220.plugin\_3S\CoDeSys\Compiler35220\Tools\TypeTable.cs`):

| Метод | строка | определение |
|---|---|---|
| `IsSigned` | 269 | `SInt..LInt`, `Real/LReal`, `AnyInt..AnyNum`, `XInt`, `LTime/LDate..` |
| `IsBoolean` | 310 | `tc <= Bit || tc == BitConst` |
| `IsConcreteType` | 495 | `tc - None > 7 && tc != AnyString` |
| `IsXType` | 211 | `_IXWordType/_IUXIntType/_IXIntType` |
| `IsResolvedXType` | 205 | `_IX{D,L}{Word,UDInt,Int}Type` (fixed-width) |
| `Get(TypeClass)` | 930 | прямой маппинг класс→singleton |
| `Get(Operator)` | 1275 | маппинг operator-типа→singleton + X-types |

---

## 4. Связь с overload-resolution и `ITypeComparer`

`\u001D.\u0005.\u0001` (типизация оператора) **не вызывает** overload-resolution и
**не вызывает** `ITypeComparer` напрямую. Их разделение:

* **Overload-resolution** — `\u001F.\u0010` (`-.291.cs:24`), только для вызовов
  (`_ICallExpression`), не для операторов. Основной вход
  `\u0001(scope, ctx, call, sig, sig4)` (`-.291.cs:52`):
  * собирает кандидатов (`\u001C.\u0011`), фильтрует по совместимости типов входов
    через `Func<ICompiledType,ICompiledType,ICommonScope,bool>` =
    `\u0006.\u0011.\u0002` (imitation / `IsImplicitConvertable`), `-.291.cs:110/117`;
  * `0` совпадений → `MessageId.Err_NoMatchingOverload` (`-.291.cs:249`);
  * `>= 2` совпадений → `MessageId.Err_Ambiguity` (`-.291.cs:257`).
* **`ITypeComparer`** — impl `\u0006.\u0011` (`-.304.cs:14`). Ключевые методы,
  используемые при типизации/проверке:
  * `Imitates(TypeClass a, TypeClass b, bool, bool, bool)` — `-.304.cs:107`:
    byte↔usint, word↔uint, dword↔udint(lword/ulint при 64-бит),
    pointer↔dword/udint, lword↔ulint, time/date/ltime, real↔real;
  * `IsEqual(ICompiledType, ICompiledType, ICommonScope[, ...])` — `-.304.cs:303`
    (по `Class`, String/WString по размеру, Pointer/Reference/Array/Enum/Userdef
    рекурсивно);
  * `IsImplicitConvertable` — `-.304.cs:1041` (`\u0008`): комбинирует
    `\u0001(TypeClass,TypeClass,out ...)` (`-.304.cs:1091`, таблица
    «источник→цель») и `\u0001(TypeClass,TypeClass)` (`-.304.cs:1494`, ANY-семейство);
  * `IsImplicitLiteralConvertable` — `-.304.cs:758` (литерал влезает в диапазон
    целевого типа; subrange-проверка `-.304.cs:861`);
  * `IsCopyable`/`\u0007` и X-type→pointer (`GetSize2`=4/8), `-.304.cs:498/1192`.
* Общие с операторной типизацией точки: `TypeTable.IsSigned/IsConcreteType/
  IsBoolean/IsXType`, `TypeClass` и `_IType.DeRefType`. Overload-resolver
  использует `ITypeComparer` для аргументов вызова; операторная типизация — нет
  (кроме generic-фолбэка на `ITypeComparer` в других ветках визитора, например
  assignment-совместимость, но не в `\u001D.\u0005`).

Итог: для встроенных операторов тип считается **таблично-арифметически**
(§2), без overload-resolution; overload-resolution относится к `_ICallExpression`.

---

## 5. Таблица правил

Файл: `C:\Codesys\tables\operator_type_rules.csv`
Колонки: `op_value, op_name, category, visitor_method, result_rule, result_type, source`.
**Строк данных: 292** (по одной на каждый член `Operator`; +1 строка заголовка).
Генератор: `_gen_operator_rules.py` (в рабочей папке; читает `tables\operators.csv`).

Распределение по категориям:

| category | кол-во | смысл |
|---|---|---|
| keyword/IL | 91 | не `_IOperatorExpression` (ключевое слово/IL) |
| conversion/type | 54 | обрабатывается как `_IConversionExpression`/`_ICastExpression` |
| special | 42 | прямое присвоение `op.Type` в конкретном visitXxx |
| unhandled | 39 | уходит на `Debug.Assert(false)` (`-.143.cs:439`) |
| vector | 13 | `__vc*` |
| comparison | 12 | → BOOL (или strict-enum) |
| trig/math | 11 | → REAL/LREAL |
| bool/bitwise | 11 | → BOOL/BIT/BitConst, иначе generic |
| arithmetic | 10 | TIME-калькулятор, иначе generic |
| selection | 5 | LIMIT/MIN/MAX/MUX/SEL |
| shift | 4 | operand0 / UDINT |

Отдельно (в коде, не в CSV): TIME/DATE-арифметика —
`TimeOperationTypeCalculator` (`decompiled\Compiler35220.plugin\_3S\CoDeSys\Compiler35220\PreCompile\Typification\TimeOperationTypeCalculator.cs:187`):

* `+` (Add/Plus): один DATE/DT/TOD + только TIME ⇒ этот DATE/DT/TOD; только TIME ⇒ TIME; только LTIME ⇒ LTIME; pointer+1 ⇒ pointer (`:15`).
* `-` (Sub/Minus): DATE/DT/TOD − TIME ⇒ первый; одинаковые DATE/DT/TOD ⇒ TIME; LTIME−LTIME ⇒ LTIME; pointer−int ⇒ pointer; pointer−pointer ⇒ DWORD (`:81`).
* `*` (Mul/Times): один TIME (без не-целых/LInt) ⇒ TIME; один LTIME ⇒ LTIME (`:118`).
* `/` (Div/Divide): TIME/int ⇒ TIME; LTIME/int ⇒ LTIME (`:167`).

---

## 6. Пробелы / не выяснено

1. **`\u0017.\u000E::\u0001` (token `0x06001C95`) вызывается также из `\u0013.\u0005::\u0001`
   (`0x06001E73`) и `SimpleTypeChecker::\u0001` (`0x06001F34`)** — контекст этих
   вызовов (какие операнды/предпочтительный тип передаются) не разобран; это
   Phase1/PreCompile-пути, отличные от Phase4-визитора.
2. **IL методов `\u0096`/`\u0097`/`\u0098`** (частные случаи LIMIT/MIN-MAX/MUX-SEL) не
   выгружался — только декомпил (`-.143.cs:1688/1712/1742`).
3. **`Debug.\u0002`/`Debug.\u0001`** и `TypeTable.IsConcreteType` генерируют
   `Debug.Assert`/`throw` — не влияет на типизацию в release, но порт должен
   сохранить порядок побочек (assert до `DeRefType`).
4. **`Nonliteral`/`flag`-семантика** для не-`arithmetic`/`boolops` (например,
   пользовательские операторы `Operator.Overload`/`Class`...) не выяснена, т.к.
   они уходят на assert.
5. **`ISpecialSizeType.CompatibilitySize`** (retain/Bool16) — реализация в
   `LanguageModelManager.plugin\...\Retain*Type.cs`; влияние на `SizeBiggest`
   требует отдельной проверки.
6. **`UnknownIdentVisitor`** внутри `TimeOperationTypeCalculator.cs:192` не
   раскрыт (влияет на TIME-ветку при неизвестных типах операндов).
7. Значения `TypeClass.Pointer=22`, `BitConst=38` и т.д. взяты из
   `tables\type_system.csv` — прямой выгрузки enum из managed-`Core.dll` получить
   не удалось (в `binaries\Core.dll` dnlib видит 1 тип; реальный `TypeClass`
   приходит из другого модуля/деобфусцированного дерева `decompiled\Compiler`).

---

## 7. Артефакты

* Документ: `C:\Codesys\docs\20_OPERATOR_TYPE_RESOLUTION.md` (этот файл).
* CSV: `C:\Codesys\tables\operator_type_rules.csv` — **292 строки данных**.
* Генератор: `_gen_operator_rules.py` (рабочая папка).
* IL: `tools\dump_method_by_token.ps1 -Dll binaries\Compiler35220.plugin.dll
  -Tokens 06001DC6..06001DD3`.
