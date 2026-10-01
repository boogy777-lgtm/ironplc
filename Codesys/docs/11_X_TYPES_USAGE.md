# 11. X-типы CODESYS (__XINT / __XWORD / __UXINT / __XSTRING и resolved-формы) — пайплайн использования

Область: ST-язык CODESYS 3.5.20 (Parser35220 + Compiler35220 + LanguageModelManager).
Все ссылки — на декомпилированные/исходные файлы в `C:\Codesys`.

## 0. Главный вывод (важно для Rust-порта)

X-типы делятся на две группы:

1. **Лексируемые X-типы** — реальные ключевые слова ST, есть в `OperatorTable`:
   `__XINT`(237), `__XWORD`(231), `__UXINT`(232), `__XSTRING`(243).
   Это единственные `__X*`, которые парсер видит из исходника.
2. **Resolved X-типы** — `__XDINT`, `__XDWORD`, `__XLINT`, `__XLWORD`, `__XUDINT`, `__XULINT`.
   Литералов этих имён в коде НЕТ. Это внутренние CLR-классы (`XDIntType : DIntType` и т.п.),
   которые компилятор СОЗДАЁТ на этапе типизации из лексируемых X-типов в зависимости от
   разрядности указателя. Они наследуют `TypeClass` родителя (DInt/DWord/LInt/LWord/UDInt/ULInt),
   поэтому с точки зрения совместимости ведут себя как родитель, а не как отдельный тип.

Для Rust-порта: парсить как типы нужно только `__XINT`, `__XWORD`, `__UXINT`, `__XSTRING(n)`
(+ оператор `__XADD`). Остальные шесть — результат `TypeCompiler`, а не синтаксис.

## 1. Лексер/сканер: где распознаются

- Keywords-таблица сканера (регистрация токена → числовой `Operator`-код):
  `Parser35220.plugin\CODESYS\Parser35220\Scanner\OperatorTable.cs:638-645`
  - `"__XWORD"` → `OperatorDesc(231, ...)` (строка 638)
  - `"__UXINT"` → `OperatorDesc(232, ...)` (639)
  - `"__XINT"`  → `OperatorDesc(237, ...)` (640)
  - `"__XSTRING"` → `OperatorDesc(243, DataType|Internal)` (645)
- Оператор (не тип): `"__XADD"` → `OperatorDesc(253, ...)` (693).
- Комментарии/устаревший MUX-текст: `LanguageModelUtilities.plugin\...\Legacy\LegacyQualifiedExpressionTextVisitor.cs:230-232`
  (`__XWORD`, `__XINT`); компиляторный текст операторов — `Compiler35220.plugin\-\-.145.cs:201,203`,
  `\-.2.cs:273,275`.
- `__XSTRING#` — префикс строкового литерала: `Parser35220\...\Scanner\InternalScanner.cs:1473`.
- `ReservedUnusedKeywords` X-типы не содержит (там CHAR/WCHAR/ANY_*): `Parser35220\...\Scanner\ReservedUnusedKeywords.cs:10-30`.

## 2. Парсер: где создаются типы

- Точка разбора типа: `Parser35220\...\Declaration\TypeParser.cs:191` `HandleOperatorCase`.
  Набор числовых кодов, которые становятся типами через `TypeTable.Get(op)`:
  строки 193-237 (`hashSet` содержит 232/231/237 + все числовые примитивы), затем
  `TypeParser.cs:363` `this.TypeTable.Get(opGlobal)`.
- `__XSTRING(n)` — отдельная ветка `TypeParser.cs:328-330` → `ParseXStringType()`
  (`TypeParser.cs:480-507`), который вызывает `LMItemFactory.CreateXStringtype()` (строка 484)
  и заполняет `Length`.
- Регистрация фабричных методов (декларация интерфейса): `_ILanguageModelBuilder`
  (`decompiled\Compiler\_3S\CoDeSys\LanguageModelManager\InternalInterfaces\_ILanguageModelBuilder.cs`):
  `CreateXDWordType:103`, `CreateXLWordType:105`, `CreateXDIntType:107`, `CreateUXIntType:147`,
  `CreateXIntType:149`, `CreateXWordType:151`, `CreateXUDIntType:153`, `CreateXULIntType:155`,
  `CreateXLIntType:157`, `CreateXStringtype:649` (наследуется через `_ILanguageModelBuilder6`).
- Реализация фабрики: `decompiled\LanguageModelManager.plugin\_3S\CoDeSys\LanguageModelManager\LanguageModelBuilder.cs`
  `CreateXStringtype:1984`, `CreateXDWordType:3464`, `CreateXLWordType:3470`, `CreateXDIntType:3476`,
  `CreateUXIntType:3596`, `CreateXIntType:3602`, `CreateXWordType:3608`, `CreateXUDIntType:3614`,
  `CreateXULIntType:3620`, `CreateXLIntType:3626`.
- Классы типов: `LanguageModelManager.plugin\...\XIntType.cs:13`, `XWordType.cs:13`,
  `UXIntType.cs:13`, `XStringType.cs:14`, `XDIntType.cs:13`, `XDWordType.cs:13`, `XLIntType.cs:13`,
  `XLWordType.cs:13`, `XUDIntType.cs:13`, `XULIntType.cs:13`.
- `XStringType` реализует и узкий, и широкий строковый интерфейс
  (`XStringType.cs:14` — `_IXStringType, _IStringType, IStringType, _IWStringType, IWStringType`).

## 3. `TypeClass` и `Operator` коды

`TypeClass` (decompiled\Compiler\_3S\CoDeSys\Core\LanguageModel\TypeClass.cs): только
`UXInt`(87), `XWord`(89), `XInt`(91), `XString`(93) — собственных членов для
XDInt/XDWord/XLInt/XLWord/XUDInt/XULInt нет (они наследуют Class родителя).

`Operator` (decompiled\Compiler\_3S\CoDeSys\Core\LanguageModel\Operator.cs:471,473,483,495):
`__XWord=231`, `__UXInt=232`, `__XInt=237`, `__XString=243` (сверить с `tables\operators.csv:233-245`).

Отображение Operator → TypeClass: `Compiler35220\...\Tools\TypeTable.cs:571-578`
(`GetOperatorByType`) и `:718-728` (`GetTypeByOperator`; UXInt→UXInt, XWord→XWord, XInt→XInt,
XString→XString). Operator → тип: `TypeTable.cs:1372-1386` (`Get(Operator)`).
Размеры: `TypeTable.GetSize2` опирается на `TypeClass` (UXInt/XWord/XInt разрешаются по PointerSize).

## 4. Типизация/компиляция: как появляются resolved-типы

`TypeCompiler` (visitor Phase1): `Compiler35220\...\Phase1_Typification\TypeCompiler.cs`:

- `_IXIntType` (`__XINT`) → `TypeTable.XLInt` при PointerSize==8, иначе `TypeTable.XDInt`
  (строки 682-690).
- `_IXWordType` (`__XWORD`) → `XLWord`/`XDWord` (693-701).
- `_IUXIntType` (`__UXINT`) → `XULInt`/`XUDInt` (722-730) — обратите внимание: не ULInt/UDInt,
  а именно X-формы.
- `_IXStringType` (`__XSTRING`) → `WString`, либо `String` при `NO_UNICODE_SUPPORT` (733-745).
- `_IXDIntType` компилируется как `_IDIntType`, `_IXDWordType` как `_IDWordType`,
  `_IXLWordType` как `_ILWordType` (762-777) — то есть resolved XDInt/XDWord/XLWord просто
  прозрачны как родители.

Дополнительная X-специфика:
- `IsResolvedXType` / `IsXType` / `IsLikePointer` / `GetEquivalent64BitTypeOfResolvedXType` /
  `ResolveUXIntType`: `Compiler35220\...\Tools\TypeTable.cs:195-238`; проксирование через
  `TypeTableClass.cs:940,1054,1090,1114`.
- `__UXINT`: замена на UDInt/ULInt по PointerSize — `TypeTable.cs:195-202`,
  `TypifierAndCrossReferenceCollector.cs:2709`, `UnknownIdentVisitor.cs:1338`.
- `__XINT`/`__XWORD`/`__UXINT` как «pointer-like» в присваиваниях/сравнениях:
  `TypeCheckerVisitor.cs:1598`, `\-.299.cs:232`, `\-.151.cs:38,146-150`.
- Десериализация сохранённых X-типов: `Compiler35220.plugin\-\-.29.cs:102-124`
  (`CreateXWordType/XDWordType/XLWordType/XULIntType/UXIntType/XDIntType/XLIntType/XUDIntType/XIntType/XStringType`).

## 5. Конверсии и совместимость

- Генерация операторов конверсий: `Parser35220\...\Scanner\OperatorTable.cs`
  `AddConversionOperators:708-747` и `AddOverloadedConversions:751-768`.
  Собираются все операторы с флагом `DataType` (и `SafetyDataType`) — включая `__XINT`,
  `__XWORD`, `__UXINT`, `__XSTRING`. Для каждой пары (i≠j) создаётся `<A>_TO_<B>`
  с кодом `184`, а также `TO_<T>`, `ANY_TO_<T>`, `ANY_NUM_TO_<T>`.
  Следствие: существуют конверсии вида `__XINT_TO_DINT`, `DINT_TO___XINT`,
  `__XWORD_TO_DINT`, `__UXINT_TO_LINT`, `__XSTRING_TO_STRING` и т.д.
  Форма текста берётся из `GetTextOfOperator` (`_textOfOperatorLong/Short`), т.е. как в
  `OperatorTable.cs:160-215` (для X-типов длинная и короткая формы совпадают).
- `ITypeComparer` (интерфейс): `decompiled\Compiler\_3S\CoDeSys\LanguageModelManager\InternalInterfaces\ITypeComparer.cs`.
  Прокси: `LanguageModelManager.plugin\...\TypeComparerProxy.cs:21-23`.
- Реализация `ITypeComparer`: `Compiler35220.plugin\-\-.304.cs:14` (`internal sealed class \u0011 : ITypeComparer`)
  (фабрика — `CompilerServices.cs:41`).
  - Нормализация `UXInt/XWord/XInt` по PointerSize (4→UDInt/DWord/DInt, 8→ULInt/LWord/LInt):
    `\-.304.cs:965-986`.
  - `Imitates`: для source-классов `UXInt/XWord/XInt` (1246-1254) истинно только если
    dest — `Pointer` или один из `UXInt..XInt`; для остальных X (dest-набор 1286-1288) истинно
    от числовых source. То есть **X-типы совместимы друг с другом и с Pointer, но не
    неявно-конвертируемы в/из произвольного DINT**.
  - `Iterating` по Pointer: `TypeTable.IsXType` даёт pointer-подобную совместимость (1196-1205).

### `__XDINT` vs `DINT`
`XDIntType` не имеет собственного `TypeClass` (наследует `DIntType`, `XDIntType.cs:13`), поэтому
после компиляции (`TypeCompiler.cs:762-765`) он типизуется как `DINT`. Отдельного сравнения
`XDIntType`/`DINT` нет. Если пользовательский код объявляет `DINT` — это обычный `DIntType`;
`XDIntType` возникает только внутри, из `__XINT` при 32-битной цели.

## 6. Влияние на перегрузки (overload resolution)
Отдельной X-ветки в выборе перегрузки нет. Выбор опирается на `ITypeComparer`:
`IsImplicitConvertable` учитывает нормализованный по PointerSize `TypeClass` и правила
`Imitates` (`\-.304.cs:1041-1088`, `1091-1312`). Поскольку resolved X-типы имеют Class родителя,
они участвуют в перегрузках как родитель; unresolved `__XINT/__XWORD/__UXINT` — как
отдельные X-классы (сравнимо с pointer-подобными).

## 7. Листинг ключевых слов (для лексера Rust)

| keyword      | Operator | flags                    | тип-объект  |
|--------------|----------|--------------------------|-------------|
| `__XINT`     | 237      | DataType\|Numeric|AllLang | `XIntType` (TypeClass.XInt) |
| `__XWORD`    | 231      | DataType\|Numeric|AllLang | `XWordType` (TypeClass.XWord) |
| `__UXINT`    | 232      | DataType\|Numeric|AllLang | `UXIntType` (TypeClass.UXInt) |
| `__XSTRING`  | 243      | DataType\|Internal      | `XStringType` (TypeClass.XString), опц. `__XSTRING(n)` |
| `__XADD`     | 253      | Operator\|Internal      | оператор (не тип) |

Resolved (не лексируются, создаются компилятором): `XDIntType`, `XDWordType`, `XLIntType`,
`XLWordType`, `XUDIntType`, `XULIntType` — Class соответственно DInt/DWord/LInt/LWord/UDInt/ULInt.

## 8. Что это значит для Rust-порта
1. Лексер: добавить 4 ключа X-типов (регистронезависимо) + `__XADD`; поддерживать суффикс
   `__XSTRING(n)`.
2. Парсер типов: `__XINT/__XWORD/__UXINT` → соответствующие `X*Type` (не сводить к DINT/DWORD/…);
   `__XSTRING` → `XStringType` с `Length`.
3. Типизация: реализовать резолвинг по разрядности указателя ровно как в `TypeCompiler.cs:682-745`
   (XInt→XDInt/XLInt, XWord→XDWord/XLWord, UXInt→XUDInt/XULInt, XString→WString/String).
4. Совместимость: X-типы (`XInt/XWord/UXInt`) неявно совместимы друг с другом и с Pointer по
   правилам `Imitates`; resolved X ведут себя как родительский Class.
5. Конверсии: генерировать `<T>_TO_<U>`, `TO_<T>`, `ANY_TO_<T>` для всех DataType-операторов,
   включая X (код 184), по `AddConversionOperators`.

## 9. Пробелы / что не найдено
- Литералов `__XDINT/__XDWORD/__XLINT/__XLWORD/__XUDINT/__XULINT` нет ни в одном файле
  `C:\Codesys` (проверено по .cs/.xml/.csv/.md/.txt). Эти имена — условные: реально это
  CLR-классы, вызываемые только из десериализатора (`\-.29.cs`) и `TypeCompiler`.
- `GetTextOfOperator` для X в Parser35220 делегирует `OperatorTable.GetTextOfOperator`
  (`InternalScanner.cs:1923-1935`); словари `_textOfOperatorLong/Short` заполняются в
  `FillOperatorTable` (`OperatorTable.cs:168-215`), прямая строка для X-ов там не выведена —
  форма `__XWORD/__XINT` подтверждена legacy-визитором и компиляторными `GetTextOfOperator`
  (`\-.145.cs:201/203`), для `__XSTRING/__UXINT` прямых строк в дампе нет.
- Класс-фабрика `\u0019.\u0003.\u0001()`, которой `static TypeTable` (TypeTable.cs:15-63)
  инициализирует все X-поля, обфусцирована; её точная реализация `CreateXxx` не восстановлена
  (декомпилятор показал одинаковый вызов для всех типов).
