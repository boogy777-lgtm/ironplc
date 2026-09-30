# 12. X-типы CODESYS — закрытие 3 пробелов (IL-трейс)

Продолжение `docs\10_X_TYPES.md` и `docs\11_X_TYPES_USAGE.md`. Закрыты пробелы §8
из `10_X_TYPES.md` (пп. 2, 3, 4) и п. 1/5 частично. Все выводы подтверждены дампом IL
через `dnlib` (`tools\dump_method_il.ps1`, новый `tools\dump_method_by_token.ps1`).

> Все `file:line` — относительно `C:\Codesys\`. DLL для IL:
> `binaries\Parser35220.plugin.dll` и `binaries\Compiler35220.plugin.dll`.
> Имена `\uXXXX` — SmartAssembly name-obfuscation (не шифрование строк).

---

## 0. TL;DR

1. **Строки X-операторов не зашифрованы и не берутся из ресурсов.** Они лежат
   обычными `ldstr` в таблице сканера `Parser35220` и попадают в
   `_textOfOperatorLong/Short` через `FillOperatorTable`. Точные значения:
   `__XINT` (237), `__XWORD` (231), `__UXINT` (232), `__XSTRING` (243), `__XADD` (253).
2. **`XStringType.Size()` = -1** — это «сырой» тип; реальный размер появляется только
   после типификации, когда `TypeCompiler` конвертирует `_IXStringType` в `WString`
   (или `String` при `NO_UNICODE_SUPPORT`), копируя выражение длины `m_expSize`.
   Формула: **`XSTRING(n)` → `WSTRING(n)` = `(n+1)*2` байт** (по умолчанию 162);
   при `NO_UNICODE_SUPPORT` → `STRING(n)` = `n+1` байт (по умолчанию 81).
3. **Static-init `TypeTable`** (cctor `_3S.CoDeSys.Compiler35220.Tools.TypeTable`,
   IL token `0x06000734`) присваивает X-поля через обфусцированный ленивый фасад
   `\u0019.\u0003.\u0001()` (файл `decompiled\Compiler35220.plugin\-\-.71.cs`),
   который делегирует в `LanguageModelBuilder.CreateX*Type()`. Карта — в §3.
4. **Знаковость подтверждена IL-ом `TypeTable.IsSigned`** (token `0x06000740`):
   `XInt` → signed, `XWord`/`UXInt`/`XString` → unsigned. Прежнее подозрение
   («switch-lowering даёт true для XWord») — артефакт декомпилятора (знаковое `>`
   вместо беззнакового `bgt.un`), IL корректен.

---

## 1. Пробел №1 — строки операторов X-типов (точные значения + trace)

### 1.1 Значения

| `Operator` | код | строковый текст | где зарегистрировано |
|---|---|---|---|
| `Operator.__XWord` | 231 | `__XWORD` | `Parser35220.plugin\CODESYS\Parser35220\Scanner\OperatorTable.cs:638` |
| `Operator.__UXInt` | 232 | `__UXINT` | `...\OperatorTable.cs:639` |
| `Operator.__XInt` | 237 | `__XINT` | `...\OperatorTable.cs:640` |
| `Operator.__XString` | 243 | `__XSTRING` | `...\OperatorTable.cs:645` |
| `Operator.__XAdd` | 253 | `__XADD` | `...\OperatorTable.cs:693` |

`RegisterOperator` — метод `AddDataTypeNames` (`OperatorTable.cs:599-658`),
`AddSpecialOperators` (`:661-705`). В обоих случаях ключ словаря (строка) становится
текстом оператора (см. §1.2).

### 1.2 Цепочка вызовов (кто возвращает строку)

```
XIntType.ToString()                       LanguageModelManager\XIntType.cs:16-19
  -> CompilerProxy.GetTextOfOperator(op)  CompilerProxy.cs:855-858
    -> _Helper.GetTextOfOperator(op)      Compiler35220\Services\Helper.cs:1343-1346
      -> Scanner.GetTextOfOperator(op)    Compiler35220\PreCompile\Scanner.cs:48-51
        -> LanguageServices.ScannerService.GetTextOfOperator(op)  (IScannerService)
          -> ScannerService.GetTextOfOperator  Parser35220\ScannerService.cs:38-41
            -> OperatorTable.Instance.GetTextOfOperator(op)
              -> GetTextOfOperatorLong(op)   OperatorTable.cs:286-292
                -> _textOfOperatorLong[op]   заполнен в FillOperatorTable
```

Аналогично для `XWordType.cs:16-19`, `UXIntType.cs:16-19`, `XStringType.cs:42-45`.

### 1.3 Как формируется значение (IL)

`FillOperatorTable` (`OperatorTable.cs:168-225`) берёт **ключ** словаря как текст:

```
(IL FillOperatorTable, token 0x060001AD)
IL_003B: DictionaryEntry::get_Key()
IL_0042: isinst System.String          -> value = (string)dictionaryEntry.Key
...
IL_0189: ... _textOfOperatorLong ... set_Item(op, value)
IL_019B: ... _textOfOperatorShort ... set_Item(op, value)
```

Дамп `AddDataTypeNames` (token `0x060001BF`), X-строки (IL-адреса):

```
IL_0382: ldstr "__XWORD"   ; Operator code 231
IL_039C: ldstr "__UXINT"   ; Operator code 232
IL_03B6: ldstr "__XINT"    ; Operator code 237
IL_042C: ldstr "__XSTRING" ; Operator code 243
```

Дамп `AddSpecialOperators` (token `0x060001C0`):

```
IL_0301: ldstr "__XADD"    ; Operator code 253
```

Дамп `GetTextOfOperatorLong` (token `0x060001B1`):

```
IL_0000: ldfld _textOfOperatorLong
IL_0007: ContainsKey -> if false: IL_000E ldstr "ERROR"; ret
IL_001B: Dictionary::get_Item -> ret
```

`GetTextOfOperatorShort` (token `0x060001B0`) идентичен и возвращает тот же текст для
X-типов (в `FillOperatorTable` long и short заполняются одним и тем же `value`,
спец. ветки только для `TIME_OF_DAY`/`DATE_AND_TIME`/`LDATE_AND_TIME`/`LTIME_OF_DAY`).

### 1.4 Примечания

- **Шифрования строк нет**: значения — прямые `ldstr` в IL сканера; SmartAssembly
  применял только переименование (`\uXXXX`), а не string-encryption. `resources` для
  этих операторов не используются.
- То же подтверждает legacy-таблица `Parser35210`:
  `decompiled\Parser35210.plugin\CODESYS\Parser35210\Scanner\OperatorTable.cs:591-593,598,645`
  (`operators["__XWORD"]`, `["__UXINT"]`, `["__XINT"]`, `["__XSTRING"]`, `["__XADD"]`).
- Литеральный префикс XSTRING в сканере: `InternalScanner.cs:1473`
  (`GetTextOfOperator(243) + "#"` → `__XSTRING#`).

---

## 2. Пробел №2 — размер `__XSTRING(n)` / `__XWSTRING(n)`

### 2.1 Где хранится длина

`XStringType.m_expSize` (тип `_IExpression`) —
`LanguageModelManager.plugin\_3S\CoDeSys\LanguageModelManager\XStringType.cs:87-91`:

```csharp
[DefaultSerialization("SizeExpression")]
[StorageVersion("3.5.5.0")]
private _IExpression m_expSize;
```

Доступ: свойства `LengthExpression` (`:18-24`) и `Length` get/set (`:29-39`).
Заполняется парсером: `Parser35220.plugin\...\Declaration\TypeParser.cs:480-507`
(`ParseXStringType` — при `(` читает выражение и делает `xstringType.Length = length`),
а также `Parser35210` `TypeParser.cs:334-353`. Оператор `__XSTRING` распознаётся по
коду **243** (`Parser35220\...\TypeParser.cs:328-330`).

### 2.2 Почему `XStringType.Size()` = -1

`XStringType` **не переопределяет** `Size` (см. `XStringType.cs` — есть только
`ToString`, `Class`, `Accept`, `CanConvertRaw/ConvertRaw/CanConvertToRaw/ConvertToRaw`).
Значит работает `IECType.Size` (`LanguageModelManager\IECType.cs:147-158`):

```
Size(scope) -> TypeTable.GetSize(this.Class, scope)   // Class = TypeClass.XString
```

`Compiler35220\...\Tools\TypeTable.cs:756-758` → `GetSize2(tc, scope)`
(`TypeTable.cs:762-820`) — в `switch` **нет** `case TypeClass.XString/XWord/XInt/UXInt`,
поэтому для `XString` и остальных unresolved X-типов возвращается `-1`.

### 2.3 Реальный размер — компиляторный путь (конверсия XSTRING → WSTRING/STRING)

`Compiler35220.plugin\...\Phase1_Typification\TypeCompiler.cs:733-745`:

```csharp
public void \u0001(_IXStringType \u0002)
{
    if (this.Comcon.IsDefined("NO_UNICODE_SUPPORT"))
    {
        _IStringType istringType = \u0019.\u0003.\u0001();   // CreateStringType()
        istringType.Length = \u0002.Length;                 // копия m_expSize
        this.GeneratedType = istringType;
        return;
    }
    _IWStringType iwstringType = \u0019.\u0003.\u0001();     // CreateWStringType()
    iwstringType.Length = \u0002.Length;
    this.GeneratedType = iwstringType;
}
```

То есть `__XSTRING(n)` **по умолчанию** резолвится в `WSTRING(n)`; при
`NO_UNICODE_SUPPORT` — в `STRING(n)`. Размер считается уже у целевого типа:

- `WStringType.Size(scope)` — `WStringType.cs:165-181`:
  `n = TypeHelper.GetInt(m_expSize,...)`; если невалидно/`n<0` → `WStringType.DefaultSize*2`;
  иначе **`(n+1)*2`** байт.
- `StringType.Size(scope)` — `StringType.cs:111-150`:
  если `m_expSize == null` → `DefaultSize`; иначе `n<0`/невалидно → `DefaultSize`,
  иначе **`n+1`** байт.
- `CompilerConstants.cs:8` `StringTypeDefaultSize => 81`;
  `CompilerConstants.cs:10` `WStringTypeDefaultSize => 81`
  (⇒ WSTRING default = `162` байта).

**Формула-итог:**

```
XSTRING(n)  (по умолчанию, wide)  = (n + 1) * 2   байт   (default 162)
XSTRING(n)  (NO_UNICODE_SUPPORT)  =  n + 1        байт   (default 81)
XWSTRING(n) == XSTRING(n), wide-ветка: (n + 1) * 2 байт
```

Здесь `+1` — терминатор (для wide — 2 байта нуля). `TypeHelper.GetInt(m_expSize, scope)`
вычисляет выражение длины в scope (константные/параметризованные длины).

---

## 3. Пробел №3 — обфусцированная static-инициализация `TypeTable`

### 3.1 Что за метод

Обфусцированный фасад — статический класс **`\u0019.\u0003`**
(файл `C:\Codesys\decompiled\Compiler35220.plugin\-\-.71.cs`, `namespace \u0019`,
`internal static class \u0003`). Методы `\u0001()` — перегруженные ленивые аксессоры,
каждый возвращает свой интерфейс `_I<Type>`:

```
-.71.cs:18-28   Builder { get } -> APEnvironmentFacade.Instance.LanguageModelMgr
                                        .CreateLanguageModelBuilder() as _ILanguageModelBuilder7
-.71.cs:301-304 internal static _IXDWordType \u0001() => Builder.CreateXDWordType();
-.71.cs:307-310 internal static _IXLWordType \u0001() => Builder.CreateXLWordType();
-.71.cs:313-316 internal static _IXDIntType  \u0001() => Builder.CreateXDIntType();
-.71.cs:433-436 internal static _IUXIntType  \u0001() => Builder.CreateUXIntType();
-.71.cs:439-442 internal static _IXIntType   \u0001() => Builder.CreateXIntType();
-.71.cs:445-448 internal static _IXWordType  \u0001() => Builder.CreateXWordType();
-.71.cs:451-454 internal static _IXUDIntType \u0001() => Builder.CreateXUDIntType();
-.71.cs:457-460 internal static _IXULIntType \u0001() => Builder.CreateXULIntType();
-.71.cs:463-466 internal static _IXLIntType  \u0001() => Builder.CreateXLIntType();
-.71.cs:619-622 internal static _IPointerType \u0001() => Builder.CreatePointerType();
-.71.cs:655-658 internal static _IStringType  \u0001() => Builder.CreateStringType();
-.71.cs:661-664 internal static _IWStringType \u0001() => Builder.CreateWStringType();
-.71.cs:1387-1390 internal static _IXStringType \u0001() => Builder.CreateXStringtype();
```

IL-proof (`tools\dump_method_by_token.ps1`, DLL `Compiler35220.plugin.dll`):

```
[token 0x06001137] _IXIntType  .().()  -> IL: call Builder; callvirt _ILanguageModelBuilder::CreateXIntType()
[token 0x06001138] _IXWordType            -> CreateXWordType()
[token 0x06001136] _IUXIntType            -> CreateUXIntType()
[token 0x060011D5] _IXStringType          -> CreateXStringtype()
[token 0x06001120] _IXDWordType           -> CreateXDWordType()
[token 0x06001121] _IXLWordType           -> CreateXLWordType()
[token 0x06001122] _IXDIntType            -> CreateXDIntType()
[token 0x06001139] _IXUDIntType           -> CreateXUDIntType()
[token 0x0600113A] _IXULIntType           -> CreateXULIntType()
[token 0x0600113B] _IXLIntType            -> CreateXLIntType()
[token 0x0600115B] _IStringType           -> CreateStringType()
[token 0x0600115C] _IWStringType          -> CreateWStringType()
```

Соответствие тип→строитель (реализации) в
`LanguageModelManager.plugin\...\LanguageModelBuilder.cs`:
`CreateXDWordType:3464`, `CreateXLWordType:3470`, `CreateXDIntType:3476`,
`CreateUXIntType:3596`, `CreateXIntType:3602`, `CreateXWordType:3608`,
`CreateXUDIntType:3614`, `CreateXULIntType:3620`, `CreateXLIntType:3626`,
`CreateXStringtype:1984` (и дубль `CreateXStringType:3920`).

### 3.2 Кто вызывает фасад: cctor `TypeTable`

`_3S.CoDeSys.Compiler35220.Tools.TypeTable` — статический ctor `TypeTable()`
(исходник `...\Tools\TypeTable.cs:12-64`, IL token `0x06000734`) присваивает
поля. X-записи в IL (адреса вызовов фабрик):

| TypeTable-поле | вызов фабрики (IL) | далее | .NET-класс | TypeClass |
|---|---|---|---|---|
| `TypeTable.UXInt` | `_IUXIntType \u0001()` (IL_00FA) | `CreateUXIntType` | `UXIntType` | `TypeClass.UXInt` |
| `TypeTable.XInt` | `_IXIntType \u0001()` (IL_0104) | `CreateXIntType` | `XIntType` | `TypeClass.XInt` |
| `TypeTable.XWord` | `_IXWordType \u0001()` (IL_010E) | `CreateXWordType` | `XWordType` | `TypeClass.XWord` |
| `TypeTable.XDWord` | `_IXDWordType \u0001()` (IL_0118) | `CreateXDWordType` | `XDWordType` | `TypeClass.DWord` |
| `TypeTable.XLWord` | `_IXLWordType \u0001()` (IL_0122) | `CreateXLWordType` | `XLWordType` | `TypeClass.LWord` |
| `TypeTable.XUDInt` | `_IXUDIntType \u0001()` (IL_012C) | `CreateXUDIntType` | `XUDIntType` | `TypeClass.UDInt` |
| `TypeTable.XULInt` | `_IXULIntType \u0001()` (IL_0136) | `CreateXULIntType` | `XULIntType` | `TypeClass.ULInt` |
| `TypeTable.XDInt` | `_IXDIntType \u0001()` (IL_0140) | `CreateXDIntType` | `XDIntType` | `TypeClass.DInt` |
| `TypeTable.XLInt` | `_IXLIntType \u0001()` (IL_014A) | `CreateXLIntType` | `XLIntType` | `TypeClass.LInt` |
| `TypeTable.XString` | `_IXStringType \u0001()` (IL_0276) | `CreateXStringtype` | `XStringType` | `TypeClass.XString` |
| `TypeTable.Pointer` | `_IPointerType \u0001()` (IL_0280) | `CreatePointerType` | `PointerType` | `TypeClass.Pointer` |
| (`String`/`WString`) | `_IStringType`/`_IWStringType` | `CreateStringType`/`CreateWStringType` | `StringType`/`WStringType` | для резолва XSTRING |

Фрагмент IL (token `0x06000734`) для X-полей:

```
IL_00FA: call ..._IUXIntType  .\x03::\u0001()
IL_00FF: call TypeTable::set_UXInt
IL_0104: call ..._IXIntType   .\x03::\u0001()
IL_0109: call TypeTable::set_XInt
IL_010E: call ..._IXWordType  .\x03::\u0001()
IL_0113: call TypeTable::set_XWord
IL_0118: call ..._IXDWordType ...
IL_0122: call ..._IXLWordType ...
IL_012C: call ..._IXUDIntType ...
IL_0136: call ..._IXULIntType ...
IL_0140: call ..._IXDIntType   ...
IL_014A: call ..._IXLIntType   ...
...
IL_0276: call ..._IXStringType .\x03::\u0001()
IL_027B: call TypeTable::set_XString
IL_0280: call ..._IPointerType ...
```

### 3.3 Карта `X-тип → запись в таблице` (сводка)

```
__XINT     -> Operator.__XInt   (237) -> TypeClass.XInt   -> TypeTable.XInt    = XIntType
__XWORD    -> Operator.__XWord  (231) -> TypeClass.XWord  -> TypeTable.XWord   = XWordType
__UXINT    -> Operator.__UXInt  (232) -> TypeClass.UXInt  -> TypeTable.UXInt   = UXIntType
__XSTRING  -> Operator.__XString(243) -> TypeClass.XString-> TypeTable.XString = XStringType
```

Резолвинг unresolved → resolved (после типификации, `TypeCompiler.cs:682-745`):

```
__XINT  --ptr4--> TypeTable.XDInt (XDIntType, Class=DInt)   --ptr8--> TypeTable.XLInt  (XLIntType,  Class=LInt)
__XWORD --ptr4--> TypeTable.XDWord(XDWordType,Class=DWord)  --ptr8--> TypeTable.XLWord (XLWordType, Class=LWord)
__UXINT --ptr4--> TypeTable.XUDInt(XUDIntType,Class=UDInt)  --ptr8--> TypeTable.XULInt (XULIntType, Class=ULInt)
__XSTRING ------ WStringType (default) / StringType (NO_UNICODE_SUPPORT)
```

Маппинг `TypeClass → Operator` (`TypeTable.cs:571-578`) и
`Operator → TypeClass` (`TypeTable.cs:718-729`) согласован с этой картой;
`TypeTable.Get(Operator)` возвращает поле по оператору/классу
(`TypeTable.cs:1372-1386`).

### 3.4 Регистрация в `LanguageModelManager` (параллельная таблица)

`_3S.CoDeSys.LanguageModelManager.TypeTable` имеет свойства-синглтоны
(`XString:802`, и т.д.) и статические поля, а фабрики — методы
`LanguageModelBuilder` (см. §3.1). Компиляторная `TypeTable` (`Compiler35220`)
берёт из неё объекты через `\u0019.\u0003` и хранит как `static`-свойства,
используя `Class`/`Accept` типа для диспетчеризации.

---

## 4. Бонус — знаковость `XWord`/`USInt` по IL (`IsSigned`)

`_3S.CoDeSys.Compiler35220.Tools.TypeTable.IsSigned` — IL token `0x06000740`
(исходник `TypeTable.cs:269-290`). Дизассемблированная беззнаковая логика:

```
if (tc <= 15)  return ((uint)(tc-6) <= 3) || ((uint)(tc-14) <= 1);
else           return ((uint)(tc-33) <= 2) || (tc == 41) || ((uint)(tc-46) <= 1);
```

С `TypeClass` (0-based, `Compiler\_3S\CoDeSys\Core\LanguageModel\TypeClass.cs`):

```
6..9  = SInt,Int,DInt,LInt      -> signed
14..15= Real,LReal              -> signed
33..35= AnyInt,AnyNum,AnyReal   -> signed
41    = XInt                    -> signed     << X-тип
46..47= LDate,LDateAndTime      -> signed
всё остальное                    -> unsigned
```

Вывод:

- `XInt` (41) → **signed** ✔ (совпадает с `x_types.csv`).
- `XWord` (40), `UXInt` (39) → **unsigned** ✔ (подтверждено; НЕ signed).
- `XString` (42) → unsigned (не числовой).
- Ранее замеченное «декомпилятор даёт true для XWord/USInt» — **артефакт**:
  исходник `TypeTable.cs:278,284` использует знаковые сравнения
  (`tc - TypeClass.AnyInt > 2`), тогда как IL сравнивает беззнаково
  (`bgt.un.s`). IL — авторитетен: `XWord`/`USInt` = false.

`XIntType`/`XWordType`/`UXIntType` собственного `IsSigned` не переопределяют;
знаковость берётся из `Class` (XInt→signed; XWord/UXInt→unsigned) и после резолва
становится знаковостью `DInt/LInt`/`DWord/LWord`/`UDInt/ULInt` соответственно.

---

## 5. Что осталось не разрешено / почему

1. **Численное значение `TypeHelper.GetInt`** для не-константного `m_expSize` в
   конкретном scope (runtime). Статически виден контракт (вычисляет выражение длины,
   `out bValid`), но точные значения зависят от scope/проекта — нужен runtime-эксперимент.
2. **Точная точка резолва unresolved→resolved на уровне парсер→LM** локализована лишь
   в `TypeCompiler` (Phase1_Typification, `_IXIntType`-обработчики `:682-745`). Более
   ранние хендлеры (`SimpleTypeChecker`, `GenericTypeReplacer`) переиспользуют типы без
   замены — отсюда `Size=-1` до фазы1. Полный порядок фаз (TypeAcceptor/GenericTypeReplacer)
   не трассирован по IL построчно.
3. **`GetTextOfOperatorShort` vs `Long`** для X-типов совпадают (доказано
   `FillOperatorTable`), поэтому неопределённости нет, но отдельного «короткого»
   представления X-типов в природе не существует.
4. **Sтруктура string-encryption SmartAssembly** не исследовалась глубже, т.к. для этих
   строк её нет (прямые `ldstr`). Если понадобится для других плагинов — см.
   `tools\re_strings.ps1`.
5. **`XSTRING#`-литералы**: префикс `__XSTRING#` подтверждён
   (`InternalScanner.cs:1473` для `Parser35220`, `:1314` для `Parser35210`), но полная
   грамматика литерных форм XSTRING не входила в 3 пробела.

---

## 6. Артефакты

- `tables\x_types_resolved.csv` — машиночитаемая карта X-тип → Operator/текст/размер/знак.
- `tools\dump_method_by_token.ps1` — дамп IL методов по MDToken (для `\uXXXX`-методов,
  у которых совпадают имена).
