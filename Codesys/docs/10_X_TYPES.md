# 10. X-типы CODESYS (`__XINT`/`__XWORD`/`__UXINT`/`__XSTRING` и resolved XL*/XD*/XUD*)

Область: только `X*Type`/`UXIntType` из `LanguageModelManager.plugin`. Не путать
*неразрешённые* X-типы (`XIntType`, `XWordType`, `UXIntType`, `XStringType`) с
*разрешёнными* (resolved) `XDIntType/XDWordType/XLIntType/XLWordType/XUDIntType/XULIntType`.

> Ссылки `файл:строка` — на `C:\Codesys\decompiled\...` (dnSpy-экспорт).
> Имена `\u00XX` — обфускация SmartAssembly.

---

## 0. Главный вывод (TL;DR)

1. В ST существуют **только 4 ключевых слова**: `__XINT`, `__XWORD`, `__UXINT`,
   `__XSTRING` (`Parser35210.plugin\...\Scanner\OperatorTable.cs:591-593,598`).
   Ключевых слов `__XDINT`/`__XDWORD`/`__XLINT`/`__XLWORD`/`__XUDINT`/`__XULINT`
   **НЕТ** ни в одной таблице (глобальный поиск — 0 совпадений).
2. Значит `XDIntType`, `XDWordType`, `XLIntType`, `XLWordType`, `XUDIntType`,
   `XULIntType` — **внутренние (resolved) представления** без собственного ST-имени.
   Компилятор получает `__XINT`/`__XWORD`/`__UXINT` и по размеру указателя цели
   сохраняет их как `XD*`/`XL*` (`Compiler35220.plugin\...\Tools\TypeTable.cs:156-192`).
3. Производные X-типы **НЕ переопределяют `Class` и `ToString`** — печатаются и
   сравниваются как базовый `DINT`/`DWORD`/`LINT`/`LWORD`/`UDINT`/`ULINT`.
4. «Вид» (X-ness) задаётся только: (а) `TypeClass` + `ToString` у 4 неразрешённых, и
   (б) .NET-типом + marker-интерфейсом `_IX*Type` у 6 разрешённых — лексически они
   неотличимы от базы.

---

## 1. Таблица X-типов (9 запрошенных + UXIntType)

| X-тип | base_class | TypeClass | ST-ключ | бит | знак | Отличается от базы |
|---|---|---|---|---|---|---|
| `XIntType` | `IECType` | `XInt` (**override**) | `__XINT` | ptr (32/64) | signed | `ToString="__XINT"`, `Class=XInt` |
| `XWordType` | `IECType` | `XWord` (**override**) | `__XWORD` | ptr (32/64) | unsigned | `ToString="__XWORD"`, `Class=XWord` |
| `UXIntType` *(бонус, 10-й)* | `IECType` | `UXInt` (**override**) | `__UXINT` | ptr (32/64) | unsigned | `ToString="__UXINT"`, `Class=UXInt` |
| `XStringType` | `IECType` | `XString` (**override**) | `__XSTRING` | var (len-expr) | n/a | `Length/m_expSize`, implements `_IStringType`+`_IWStringType` |
| `XDIntType` | `DIntType` | `DInt` (inherited) | — | 32 | signed | только `Accept(ITypeVisitor3)` |
| `XDWordType` | `DWordType` | `DWord` (inherited) | — | 32 | unsigned | только `Accept(ITypeVisitor3)` |
| `XLIntType` | `LIntType` | `LInt` (inherited) | — | 64 | signed | только `Accept` |
| `XLWordType` | `LWordType` | `LWord` (inherited) | — | 64 | unsigned | только `Accept(ITypeVisitor3)` |
| `XUDIntType` | `UDIntType` | `UDInt` (inherited) | — | 32 | unsigned | только `Accept` |
| `XULIntType` | `ULIntType` | `ULInt` (inherited) | — | 64 | unsigned | только `Accept` |

`parent_equiv` (resolved ↔ база): `XDInt↔DINT`, `XDWord↔DWORD`, `XLInt↔LINT`,
`XLWord↔LWORD`, `XUDInt↔UDINT`, `XULInt↔ULINT`; неразрешённые:
`__XINT→DINT|LINT`, `__XWORD→DWORD|LWORD`, `__UXINT→UDINT|ULINT` (по `PointerSize`).

Машиночитаемо: `C:\Codesys\tables\x_types.csv` (10 data-строк + заголовок).

---

## 2. Композиция классов (что и где переопределено)

### 2.1 Неразрешённые X-типы (`IECType` прямые наследники)

`XIntType` (`XIntType.cs:13`):
- `ToString()` → `CompilerProxy.GetTextOfOperator(Operator.__XInt)` (`:16-19`)
- `Class` → `TypeClass.XInt` (`:23-29`) — **переопределён**
- `Accept(ITypeVisitor)` → `typvis.visit(this)` (`:32-35`)
- `CanConvertRaw`/`ConvertRaw`/`CanConvertToRaw`/`ConvertToRaw` → `false`/`null` (`:38-59`)

`XWordType` (`XWordType.cs:13`) — идентично, `Operator.__XWord`, `TypeClass.XWord` (`:16-35`).
`UXIntType` (`UXIntType.cs:13`) — `Operator.__UXInt`, `TypeClass.UXInt` (`:16-35`).

`XStringType` (`XStringType.cs:14`):
- `LengthExpression`/`Length` get/set над `m_expSize` (`:18-39`, `:88-91`)
- `ToString()` → `GetTextOfOperator(Operator.__XString)` (`:42-45`) — **без** length
- `Class` → `TypeClass.XString` (`:49-55`) — **переопределён**
- `Accept` (`:58-61`), Raw-конверсии → `false`/`null` (`:64-85`)
- НЕ переопределяет `Size`, `_Duplicate`, `IsEqual`, `GetConstantString` (в отличие от `StringType`).

### 2.2 Разрешённые X-типы (наследники конкретных базовых)

Все шесть (`XDIntType.cs:13` : `DIntType`, `XDWordType.cs:13` : `DWordType`,
`XLIntType.cs:13` : `LIntType`, `XLWordType.cs:13` : `LWordType`,
`XUDIntType.cs:13` : `UDIntType`, `XULIntType.cs:13` : `ULIntType`) содержат **ровно один
override — `Accept`**. Ни `Class`, ни `ToString`, ни `Size`, ни `IsEqual`, ни `_Duplicate`
не переопределены.

Асимметрия `Accept` (важно для Rust-порта и для visitor'ов):
- `XDIntType.cs:16-25`, `XDWordType.cs:16-25`, `XLWordType.cs:16-25` — пробуют
  `ITypeVisitor3.visit(this)`, иначе fallback `typvis.visit(this)`.
- `XLIntType.cs:16-19`, `XUDIntType.cs:16-19`, `XULIntType.cs:16-19` — просто `typvis.visit(this)`.
- `ITypeVisitor3` объявляет `visit(_IDAliasType)`, `visit(_IXDIntType)`, `visit(_IXDWordType)`,
  `visit(_IXLWordType)` (`ITypeVisitor3.cs:9-14`) — ровно 3 совпадающих X-типа; для
  `_IXLIntType/_IXUDIntType/_IXULIntType` спец-overload отсутствует, поэтому и override не нужен.

---

## 3. Ответы на прямые вопросы

### (a) Переопределяют ли производные X-типы `Class`?
**Нет.** `XDIntType/XDWordType/XLIntType/XLWordType/XUDIntType/XULIntType` наследуют
`Class` от `DIntType`/`DWordType`/`LIntType`/`LWordType`/`UDIntType`/`ULIntType`
(`DIntType.cs:24-30` → `TypeClass.DInt` и т.д.).
Переопределяют `Class` только прямые наследники `IECType`: `XIntType` → `XInt`,
`XWordType` → `XWord`, `UXIntType` → `UXInt`, `XStringType` → `XString`.

### (b) Задаёт ли «вид» только `ToString()`/оператор?
- Для `XInt/XWord/UXInt/XString`: вид задаётся **парой** `Class` + `ToString()`
  (не только `ToString`). Оба переопределены и согласованы.
- Для 6 производных: вид **НЕ** задаётся ни `Class`, ни `ToString`. `ToString()`
  наследуется и возвращает текст базового оператора (`"DINT"`, `"DWORD"`, `"LINT"`,
  `"LWORD"`, `"UDINT"`, `"ULINT"`). X-ness существует только как marker-интерфейс
  `_IXDIntType` и как результат `TypeTable.IsResolvedXType()`/`IsLikePointer()`
  (`TypeTable.cs:205-220`). Лексически и по `Class` они эквивалентны базе.

### (c) Фактическая ширина/знаковость
Ширина берётся из `IECType.Size()` → `TypeTable.GetSize(this.Class, scope)`
(`IECType.cs:147-158`). Для `Class=DInt/LInt/DWord/LWord/UDInt/ULInt` таблица даёт
4/8/4/8/4/8 байт (`TypeTable.cs:762-821`).

| тип | Class | ширина | знак |
|---|---|---|---|
| `XDIntType` | `DInt` | 32 | signed |
| `XDWordType` | `DWord` | 32 | unsigned |
| `XLIntType` | `LInt` | 64 | signed |
| `XLWordType` | `LWord` | 64 | unsigned |
| `XUDIntType` | `UDInt` | 32 | unsigned |
| `XULIntType` | `ULInt` | 64 | unsigned |
| `XIntType` | `XInt` | по указателю (32→DInt, 64→LInt) | signed |
| `XWordType` | `XWord` | по указателю (32→DWord, 64→LWord) | unsigned |
| `UXIntType` | `UXInt` | по указателю (32→UDInt, 64→ULInt) | unsigned |
| `XStringType` | `XString` | переменная (length-expr) | n/a |

Для `XInt/XWord/UXInt/XString` `TypeTable.GetSize2()` не содержит case → возвращает
**-1** (`TypeTable.cs:762-821`). Т.е. «сырой» X-тип нельзя аллоцировать: его надо
сначала разрешить (`GetEquivalent64BitTypeOfResolvedXType`, `TypeTable.cs:223-238`).

---

## 4. Совместимость и равенство (`__XDINT` vs `DINT`)

- `IsEqual`: `IECType.IsEqual` сначала сравнивает `Class`; при равенстве классов и
  не-«блочном» классе возвращает `true` (`IECType.cs:181-198`). `__XDINT.Class == DINT.Class
  == TypeClass.DInt` ⇒ **`IsEqual(__XDINT, DINT) == true`** (аналогично все 6 resolved).
- `IsCompatible`: `IECType.IsCompatible` делегирует `TypeComparerProxy.IsImplicitConvertable`
  (`IECType.cs:96-99`, `TypeComparerProxy.cs:21-24`). Для resolved X классов компилятор
  считает типы эквивалентными по `Class`; для неразрешённых применяет
  `TypeTable.IsEquivalentTypeIncludeXTypes(tc1, tc2, PointerSize)`
  (`TypeTable.cs:144-153`), которая маппит `UXInt/XWord/XInt` в `UDInt/DWord/DInt` (ptr=4)
  или `ULInt/LWord/LInt` (ptr=8) (`:156-192`). Практическое подтверждение использования —
  `SimpleTypeChecker.cs:1578-1584`.
  - `__XDINT` ↔ `DINT`: **совместимы** (одинаковый `Class=DInt`).
  - `__XDWORD` ↔ `DWORD`, `__XLINT` ↔ `LINT` и т.д.: совместимы.
  - `__XINT` ↔ `DINT`: совместимы **только если `PointerSize==4`**; при 8 — уже `LINT`.
  - `__XSTRING` ↔ `STRING`: `Class` разный (`XString` vs `String`) ⇒ `IsEqual=false`;
    совместимость (implicit convert) — по правилам компилятора для строк, не по `Class`.

---

## 5. Доказательства (`file:line`)

- Объявления/члены X-типов: `XIntType.cs:13,16-59`; `XWordType.cs:13,16-59`;
  `UXIntType.cs:13,16-59`; `XStringType.cs:14,18-91`; `XDIntType.cs:13,16-25`;
  `XDWordType.cs:13,16-25`; `XLIntType.cs:13,16-19`; `XLWordType.cs:13,16-25`;
  `XUDIntType.cs:13,16-19`; `XULIntType.cs:13,16-19`.
- Базовые `Class`: `DIntType.cs:24-30`, `LIntType.cs:24-30`, `DWordType.cs:24-30`,
  `LWordType.cs:24-30`, `UDIntType.cs:24-30`, `ULIntType.cs:24-30`.
- База `IECType`: `Class` abstract `:172`; `Size` → `TypeTable.GetSize` `:147-158`;
  `IsCompatible` → `TypeComparerProxy` `:96-99`; `IsEqual` по `Class` `:181-198`;
  `_Duplicate`→`this` `:47-50`; `BaseType/DeRefType`→`this` `:64-80`.
- Enum'ы: `TypeClass.cs` (`XWord=40`,`XInt=41`,`XString=42`,`UXInt=39` — порядок);
  `Operator.cs:471(__XWord),473(__UXInt),483(__XInt),495(__XString)`.
- Ключевые слова ST: `Parser35210.plugin\...\Scanner\OperatorTable.cs:591-593,598`.
- Резолвинг по размеру указателя: `Compiler35220.plugin\...\Tools\TypeTable.cs:144-192`
  (`IsEquivalentTypeIncludeXTypes`, `\u0001(tc, ptr)`); `:205-214` (`IsResolvedXType`/`IsXType`);
  `:223-238` (`GetEquivalent64BitTypeOfResolvedXType`); `:762-821` (`GetSize2`, X→-1).
- Фабрики: `LanguageModelBuilder.cs:3464-3479,3596-3629,1984-1987`.
- Visitor: `Compiler\...\ITypeVisitor3.cs:9-14`.
- Рендер XSTRING: `LanguageModelUtilities.plugin\...\PreCompileUtilities.cs:807-826` (`"__XSTRING(" + text + ")"`).
- Legacy map: `LanguageModelUtilities.plugin\...\Legacy\LegacyQualifiedExpressionTextVisitor.cs:217-236`.

---

## 6. Отличия от базовых типов (сводка)

| | базовый (`DIntType` и т.п.) | X-тип |
|---|---|---|
| `Class` | `DInt`/… | `XDIntType` — тот же `DInt`; `XIntType` — свой `XInt` |
| `ToString` | `"DINT"` | `XDIntType` — тот же `"DINT"`; `XIntType` — `"__XINT"` |
| `Size` | 4/8 (по таблице) | resolved: как база; unresolved: `-1` |
| Raw-конверсии | реализованы (байты) | resolved: как база; unresolved: `false`/`null` |
| `Accept` | `visit(this)` | `XDInt/XDWord/XLWord` — через `ITypeVisitor3` |
| Роль | обычный тип | portability-маркер: «размер = размер указателя/слова» |

---

## 7. Rust: как моделировать X-типы

**Рекомендация: один `IecType` + флаг `explicit_size`, а НЕ 10 отдельных типов.**

Обоснование из кода: производные X-типы не меняют ни `Class`, ни `ToString`, ни `Size`,
ни конверсии — их единственная семантика это «не фиксированная, а указателе-зависимая
ширина + маркер resolved/unresolved». Заводить 6 отдельных Rust-структур =
дублирование без выгоды (нарушает DRY/KISS).

Модель:

```rust
enum Width { B1, B2, B4, B8, Ptr, Len(ExprId) }   // Ptr = размер указателя цели
enum Sign  { Signed, Unsigned, NotNumeric }

struct IecType {
    class: TypeClass,          // DInt, DWord, LInt, LWord, UDInt, ULInt, XInt, XWord, UXInt, XString
    width: Width,
    sign:  Sign,
    // x-семантика:
    x: XKind,
}

enum XKind {
    None,                       // обычный IEC-тип (в т.ч. resolved база без флага)
    UnresolvedX { op: XOp },    // __XINT/__XWORD/__UXINT/__XSTRING (есть ST-ключ, size=-1)
    ResolvedX   { base: TypeClass }, // XDInt/XLInt/... (Class==base, ToString==base)
}
```

Правила, которые надо воспроизвести 1:1:

1. **Разрешение (parse)**: при встрече `__XINT/__XWORD/__UXINT` создать
   `XKind::UnresolvedX`; `ToUpperString()` = `__XINT`/`__XWORD`/`__UXINT`; `size` = `None`
   (не -1 в аллокаторе, а «не разрешено»).
2. **Резолвинг**: по `pointer_size` (4/8) заменить на `XKind::ResolvedX{base}`,
   где `__XINT→DInt|LInt`, `__XWORD→DWord|LWord`, `__UXINT→UDInt|ULInt`
   (`TypeTable.cs:156-192`). После этого `Class`, `ToString`, `Size`, `ConvertRaw`
   наследуются как у базы.
3. **Эквивалентность**: `eq_class(a,b)` — по `Class`; `is_resolved_x(t)` —
   `matches!(t.x, ResolvedX{..})` (не по имени!); `is_like_pointer(t, ptr)` —
   `is_resolved_x`/`UnresolvedX`-маппинг + `class==Pointer`.
4. **`__XSTRING`**: отдельная ветка `Width::Len(m_expSize)`; поддерживать ОБА
   `IsString` и `IsWString` (в оригинале `XStringType` реализует и `_IStringType`, и
   `_IWStringType`, `XStringType.cs:14`). Рендер — `__XSTRING(<expr>)`
   (`PreCompileUtilities.cs:822`), а `ToString()` самого типа — без длины.
5. **Visitor**: не отдельные `visit`-методы для 6 resolved (в оригинале спец-overload
   есть только у `_IXDIntType/_IXDWordType/_IXLWordType`, `ITypeVisitor3.cs:9-14`);
   эмулируй как `visit_resolved_x(base)` — три специальных, остальные через общий `visit`.
6. **Запреты**: `UnresolvedX` не должен получать `Size`/аллокацию/`ConvertRaw`
   (в оригинале → `false`/`null`/`-1`). Это инвариант «резолв до типизации».

Если всё же нужны отдельные типы — делай их newtype-обёртками над базой с единственным
отличием «marker», но это хуже по DRY; предпочтителен флаг `XKind`.

---

## 8. Пробелы (не подтверждено / требует IL/рантайма)

1. **Нет `__XDINT`/`__XDWORD`/… в ST.** Задача предполагала ключ `__XDINT` у
   `XDIntType.cs`; поиск по всему `decompiled` — 0 совпадений. Вывод: resolved X-типы
   не имеют ST-ключа. Требуется подтверждение по IL/рантайм-дампу (`TypeParser`).
2. **Реальный текст `ToString()`.** `GetTextOfOperator` уходит в
   `ScannerService.GetTextOfOperator` (обфусцированные ресурсы). Канонические строки
   `__XINT/__XWORD/__UXINT/__XSTRING` подтверждены таблицей сканера
   (`OperatorTable.cs:591-598`) и legacy-мапой (`LegacyQualifiedExpressionTextVisitor.cs:217-236`),
   но прямое значение `GetTextOfOperator(Operator.__XInt)` не исполнялось.
3. **`XStringType.Size()` = -1.** `XStringType` не переопределяет `Size`, а `GetSize2`
   не знает `XString` (`TypeTable.cs:762-821`). Как именно LM получает размер
   `__XSTRING(n)` (момент конверсии в `STRING`) — не найдено; кандидат: компилятор
   конвертирует `XString→String` в precompile. Нужен рантайм-эксперимент.
4. **`IsSigned` декомпилирован некорректно** (`TypeTable.cs:269-290`): switch-lowering
   даёт `true` даже для `XWord`/`USInt`. Знаковость в таблице выведена из `Class`
   (signed = SInt/Int/DInt/LInt; unsigned = Byte/Word/DWord/LWord/USInt/UInt/UDInt/ULInt),
   а не из этого метода. Нужен IL для точной семантики `IsSigned`.
5. **Точная точка резолвинга `__XINT`→`XDInt/XLInt`** не локализована в одном месте:
   есть `TypeTable.\u0001(tc, ptr)` и `IsResolvedXType`, но вызов «заменить type на
   resolved» находится в обфусцированном коде компилятора (`Compiler35220`). Нужен
   трассировщик по IL.
6. **`TypeGuid`/`StorageVersion`** (для сериализации/совместимости):
   `XIntType` `{C23D607D-…}` v3.5.4.30; `XWordType` `{70846456-…}` v3.5.0.0;
   `UXIntType` `{D81AE1B4-…}` v3.5.0.0; `XStringType` `{964B6B19-…}` v3.5.5.0;
   `XDIntType` `{C43C8B26-…}` v3.5.4.30; `XDWordType` `{119284D1-…}` v3.5.2.0;
   `XLIntType` `{0562E96E-…}` v3.5.4.30; `XLWordType` `{859036BA-…}` v3.5.2.0;
   `XUDIntType` `{1B39B304-…}` v3.5.2.0; `XULIntType` `{B297BF25-…}` v3.5.2.0.
