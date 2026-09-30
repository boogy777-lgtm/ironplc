# 05. Система типов, области видимости и семантический резолвинг (CODESYS 3.5.22.10)

Цель документа — дать модель, по которой можно перенести систему типов, scopes и
резолвинг имён/членов/перегрузок на Rust **идентично** оригиналу.

> Все ссылки `файл:строка` — на декомпилированные исходники в `C:\Codesys\decompiled\...`
> (dnSpy-экспорт) и `binaries\*.dll`. Имена `\u00XX` — обфускация SmartAssembly
> (стабильны в пределах сборки; при порте восстанавливать по IL).
> Не путать: **реализации** живут в `Compiler35220.plugin` и `LanguageModelManager.plugin`,
> а НЕ в `LanguageModelManagerConfigurators.plugin` / `LanguageModelManagerLegacy.plugin`.

---

## 0. Итог локализации (где что лежит)

| Слой | Сборка | Реализация | Путь |
|---|---|---|---|
| Контракты (интерфейсы/enum) | `Compiler.dll` | `ITypeTable/2/3`, `_IType`, `_ICompiledPOU`, `_IScope`, `ICommonScope`, `_IPrecompileScope*`, `_ISignature`, `ITypeComparer`, `TypeClass`, `SignatureFlag`, `VarFlag` | `decompiled\Compiler\_3S\CoDeSys\{LanguageModelManager\InternalInterfaces, Core\LanguageModel}\` |
| Таблица типов | `Compiler35220.plugin.dll` | `TypeTableClass` (Singleton, : `ITypeTable3`), статический `TypeTable` | `decompiled\Compiler35220.plugin\_3S\CoDeSys\Compiler35220\Tools\{TypeTableClass,TypeTable}.cs` |
| Scopes + резолвер | `Compiler35220.plugin.dll` | `CheckerScope`, `SymbolTable`, resolver оверлоадов `\u001F.\u0010` | `…\Compiler35220\Scopes\{CheckerScope,SymbolTable}.cs`, `\-\-.291.cs` |
| Типы (IECType-семейство) | `LanguageModelManager.plugin.dll` | `IECType` + 83 конкретных `*Type`, `CompiledPOU`, `Signature`, `SubSignatureTable`, `LanguageModelBuilder`, `TypeComparerProxy` | `decompiled\LanguageModelManager.plugin\_3S\CoDeSys\LanguageModelManager\` |
| Конвертации | `Compiler35220.plugin.dll` | `ITypeComparer` impl `\u0006.\u0011` | `\-\-.304.cs:14` (создаётся `Services\CompilerServices.cs:41`) |
| Configurators / Legacy | — | **НЕ содержат** системы типов (UI опций и legacy-facade) | 65 и 4 класса соответственно |
| `Core.dll` | — | **0 managed-типов** (native/resource) — не источник | `decompiled\Core\` пусто |

Числа: `decompiled\Compiler\...\InternalInterfaces` = 482 `.cs`; `…\Core\LanguageModel` = 798 `.cs`;
`decompiled\LanguageModelManager.plugin\...\LanguageModelManager` (верхний уровень) = 409 `.cs`
(671 с подкаталогами). Типы: 84 класса в иерархии `IECType` = 83 конкретных + 1 abstract (`tables\iec_types.csv`).

---

## 1. Модель типов

### 1.1. Три уровня абстракции

```
public API (Core.LanguageModel)          internal (LanguageModelManager.InternalInterfaces)
  IType  ICompiledType ..5                  _IType : ICompiledType5..IType,IArchivable
  IUserdefType/IEnumType/IArrayType/...     _IUserdefType/_IEnumType/_IArrayType/_IPointerType/...
  IScope..IScope5                           _IScope, _IScope2, ICommonScope/2
  ISignature..ISignature7                   _ISignature
  IPrecompileScope..8                       _IPrecompileScope, _IPrecompileScope2/3
```

- `_IType` (`InternalInterfaces\_IType.cs:8`) расширяет публичный `IType` и добавляет
  `IsCompiled`, `Duplicate/_Duplicate(bool)`, `GetConstantString(IScope)`,
  `GetComponent(int,IScope5)`, `GetComponents(IScope5,out bool)`, `GetNumOfElements(IScope5)`,
  `SizeChecked`, `Accept(ITypeVisitor)`, `ToUpperString`.
- `ICompiledType` (`Core\LanguageModel\ICompiledType.cs:8`) — `IsInteger`, `BaseType`, `DeRefType`,
  `IsCompatible`, `IsEqual`, `Size(IScope)`.

### 1.2. Конкретные классы типов (LanguageModelManager.plugin)

Базовый класс — **`IECType`** (`IECType.cs:14`), абстрактный, `: GenericObject2, _IType, …`.
Даёт: `IsCompiled`, `EffectiveType` (по умолчанию `this`), `Size/SizeChecked`
(через `TypeTable.GetSize(Class, scope)`), `IsEqual` (для составных — через `Class` + `ToString`),
`IsCompatible(type,scope) => TypeComparerProxy.IsImplicitConvertable(this,type,scope)`,
`ConvertRaw/ConvertToRaw` (абстрактно).

Полный реестр (файл — `LanguageModelManager\*.cs`, строка — объявление класса).
Всего в иерархии `IECType`: **84 класса = 83 конкретных + 1 абстрактный `IECType`**.
Машиночитаемая копия: `tables\iec_types.csv` (84 строки).

| Класс | База | ST-синтаксис | TypeClass | source |
|---|---|---|---|---|
| IECType | GenericObject2 | (abstract) | — | `IECType.cs:14` |
| BoolType | IECType | BOOL | Bool | `BoolType.cs:13` |
| BitType | IECType | BIT | Bit | `BitType.cs:13` |
| BitConstType | BitType | BOOL#0 / BOOL#1 | BitConst | `BitConstType.cs:13` |
| ByteType | IECType | BYTE | Byte | `ByteType.cs:13` |
| WordType | IECType | WORD | Word | `WordType.cs:14` |
| DWordType | IECType | DWORD | DWord | `DWordType.cs:14` |
| LWordType | IECType | LWORD | LWord | `LWordType.cs:14` |
| SIntType | IECType | SINT | SInt | `SIntType.cs:13` |
| IntType | IECType | INT | Int | `IntType.cs:14` |
| DIntType | IECType | DINT | DInt | `DIntType.cs:14` |
| LIntType | IECType | LINT | LInt | `LIntType.cs:14` |
| USIntType | IECType | USINT | USInt | `USIntType.cs:13` |
| UIntType | IECType | UINT | UInt | `UIntType.cs:14` |
| UDIntType | IECType | UDINT | UDInt | `UDIntType.cs:14` |
| ULIntType | IECType | ULINT | ULInt | `ULIntType.cs:14` |
| RealType | IECType | REAL | Real | `RealType.cs:14` |
| LRealType | IECType | LREAL | LReal | `LRealType.cs:14` |
| TimeType | IECType | TIME | Time | `TimeType.cs:15` |
| LTimeType | IECType | LTIME | LTime | `LTimeType.cs:15` |
| DateType | IECType | DATE | Date | `DateType.cs:15` |
| LDateType | IECType | LDATE | LDate | `LDateType.cs:15` |
| DateAndTimeType | IECType | DATE_AND_TIME (DT) | DateAndTime | `DateAndTimeType.cs:15` |
| LDateAndTimeType | IECType | LDATE_AND_TIME (LDT) | LDateAndTime | `LDateAndTimeType.cs:15` |
| TimeOfDayType | IECType | TIME_OF_DAY (TOD) | TimeOfDay | `TimeOfDayType.cs:15` |
| LTimeOfDayType | IECType | LTIME_OF_DAY (LTOD) | LTimeOfDay | `LTimeOfDayType.cs:15` |
| StringType | IECType | STRING(n) | String | `StringType.cs:14` |
| WStringType | IECType | WSTRING(n) | WString | `WStringType.cs:14` |
| XStringType | IECType | XSTRING(n) / XWSTRING(n) | XString | `XStringType.cs:14` |
| Bool16Type | RetainBoolType | BOOL16 | Bool (inherited) | `Bool16Type.cs:13` |
| DirectAdressBitType | BoolType | BOOL (AT %..X) | Bool (inherited) | `DirectAdressBitType.cs:13` |
| RetainBoolType | BoolType | BOOL (RETAIN) | Bool (inherited) | `RetainBoolType.cs:13` |
| RetainByteType | ByteType | BYTE (RETAIN) | Byte | `RetainByteType.cs:14` |
| RetainSIntType | SIntType | SINT (RETAIN) | SInt | `RetainSIntType.cs:14` |
| RetainUSIntType | USIntType | USINT (RETAIN) | USInt | `RetainUSIntType.cs:14` |
| SafeBoolType | BoolType | SAFEBOOL | Bool | `SafeBoolType.cs:13` |
| SafeByteType | ByteType | SAFEBYTE | Byte | `SafeByteType.cs:13` |
| SafeSIntType | SIntType | SAFESINT | SInt | `SafeSIntType.cs:13` |
| SafeUSIntType | USIntType | SAFEUSINT | USInt | `SafeUSIntType.cs:13` |
| SafeWordType | WordType | SAFEWORD | Word | `SafeWordType.cs:13` |
| SafeIntType | IntType | SAFEINT | Int | `SafeIntType.cs:13` |
| SafeUIntType | UIntType | SAFEUINT | UInt | `SafeUIntType.cs:13` |
| SafeDWordType | DWordType | SAFEDWORD | DWord | `SafeDWordType.cs:13` |
| SafeDIntType | DIntType | SAFEDINT | DInt | `SafeDIntType.cs:13` |
| SafeUDIntType | UDIntType | SAFEUDINT | UDInt | `SafeUDIntType.cs:13` |
| SafeLWordType | LWordType | SAFELWORD | LWord | `SafeLWordType.cs:13` |
| SafeLIntType | LIntType | SAFELINT | LInt | `SafeLIntType.cs:13` |
| SafeULIntType | ULIntType | SAFEULINT | ULInt | `SafeULIntType.cs:13` |
| SafeTimeType | TimeType | SAFETIME | Time | `SafeTimeType.cs:13` |
| SafeRealType | RealType | SAFEREAL | Real | `SafeRealType.cs:13` |
| SafeLRealType | LRealType | SAFELREAL | LReal | `SafeLRealType.cs:13` |
| UXIntType | IECType | __UXINT | UXInt | `UXIntType.cs:13` |
| XWordType | IECType | __XWORD | XWord | `XWordType.cs:13` |
| XIntType | IECType | __XINT | XInt | `XIntType.cs:13` |
| XDIntType | DIntType | __XDINT | DInt (inherited) | `XDIntType.cs:13` |
| XDWordType | DWordType | __XDWORD | DWord (inherited) | `XDWordType.cs:13` |
| XLIntType | LIntType | __XLINT | LInt (inherited) | `XLIntType.cs:13` |
| XLWordType | LWordType | __XLWORD | LWord (inherited) | `XLWordType.cs:13` |
| XUDIntType | UDIntType | __XUDINT | UDInt (inherited) | `XUDIntType.cs:13` |
| XULIntType | ULIntType | __XULINT | ULInt (inherited) | `XULIntType.cs:13` |
| AnyType | IECType | ANY | Any | `AnyType.cs:13` |
| AnyBitType | IECType | ANY_BIT | AnyBit | `AnyBitType.cs:13` |
| AnyBitButBoolIsPreferred | AnyBitType | ANY_BIT (BOOL pref.) | AnyBit (inherited) | `AnyBitButBoolIsPreferred.cs:13` |
| AnyDateType | IECType | ANY_DATE | AnyDate | `AnyDateType.cs:13` |
| AnyIntType | IECType | ANY_INT | AnyInt | `AnyIntType.cs:13` |
| RangeAwareAnyIntType | AnyIntType | ANY_INT (range-aware) | AnyInt (inherited) | `RangeAwareAnyIntType.cs:15` |
| AnyNumType | IECType | ANY_NUM | AnyNum | `AnyNumType.cs:13` |
| AnyRealType | IECType | ANY_REAL | AnyReal | `AnyRealType.cs:13` |
| AnyStringType | IECType | ANY_STRING | AnyString | `AnyStringType.cs:13` |
| ArrayType | IECType | ARRAY[a..b; c..d] OF .. | Array | `ArrayType.cs:19` |
| VariableLengthArrayType | IECType | ARRAY[*] OF .. | VarLenArray | `VariableLengthArrayType.cs:15` |
| PointerType | IECType | POINTER TO .. | Pointer | `PointerType.cs:16` |
| ReferenceType | IECType | REFERENCE TO .. | Reference | `ReferenceType.cs:14` |
| ImplicitReferenceType | ReferenceType | (implicit REFERENCE) | Reference | `ImplicitReferenceType.cs:13` |
| InOutReferenceType | ReferenceType | (VAR_IN_OUT ref) | Reference | `InOutReferenceType.cs:13` |
| SubrangeType | IECType | base(low..high) | Subrange | `SubrangeType.cs:14` |
| VectorType | IECType | __VECTOR[n] OF .. | __Vector | `VectorType.cs:14` |
| ParamsType | IECType | __PARAMS(n) OF .. | Params | `ParamsType.cs:15` |
| LazyType | IECType | __LAZY .. | Lazy | `LazyType.cs:13` |
| EnumType | IECType | TYPE n:(..); n | Enum | `EnumType.cs:15` |
| ImplicitEnumerationType | IECType | (m1; m2; ..) | Enum | `ImplicitEnumerationType.cs:13` |
| UserdefType | IECType | name / STRUCT..END_STRUCT / FUNCTION_BLOCK / INTERFACE / UNION | Userdef | `UserdefType.cs:19` |
| GenericUserdefType | UserdefType | name<args> | Userdef | `GenericUserdefType.cs:17` |
| AliasType | UserdefType | ALIAS base | Userdef | `AliasType.cs:15` |

`UserdefType` (`UserdefType.cs:19`):
- `ToString()`: срезает `__Union`, для `__PARAMS` печатает `T(...) OF base` (`:107-138`).
- `Size/SizeChecked`/`GetComponent(s)` — через сигнатуру Fb/Struct (`:213-418`).
- Компоненты (`GetSignatureComponents`, `:421`): рекурсия base-signature → variables → interfaces;
  для `Operator.FunctionBlock` дополнительно `__MAIN`.
- Равенство: `SignatureId` + `ScopeId` (иначе — по имени, `:163`).

`EnumType` (`EnumType.cs:15`): `_Base` по умолчанию `Int`; `ConvertRaw` — int↔имя члена;
`GetNumericEnumValue` ищет член по `OrgName` (регистронезависимо, `:286`).

### 1.2.1. STRUCT / UNION / FUNCTION_BLOCK / INTERFACE / METHOD — отдельных классов НЕТ

Важно для порта: в `_3S.CoDeSys.LanguageModelManager` **не существует** классов
`StructureType`, `UnionType`, `FunctionBlockType`, `InterfaceType`, `MethodType`,
`RangeType`, `EnumerationType`, `GenericType` (проверено grep по всему дереву — 0 совпадений).
Они моделируются так:

- `STRUCT` / `UNION` / `FUNCTION_BLOCK` / `INTERFACE` / `ALIAS`-носитель — один класс
  **`UserdefType`** (`UserdefType.cs:19`), `Class = TypeClass.Userdef`. Он ссылается на
  `Signature` (поле `m_iSignatureId`/`ScopeId`); признак «вид» лежит во флагах сигнатуры
  `SignatureFlag` (`Core\LanguageModel\SignatureFlag.cs`): `Structure=1`, `Union=0x4000`,
  `ImplicitInterfaceUnion=0x8000`, `Abstract`, а интерфейсность/принадлежность — в
  `Signature`/`SubSignatureTable`.
- `UNION` дополнительно «узнаётся» в `UserdefType.ToString()` по префиксу `__Union`
  (`UserdefType.cs:107-138`); `__PARAMS` печатается как `T(...) OF base`.
- `METHOD` / `ACTION` — не тип, а **sub-signature** внутри `SubSignatureTable`
  (`Signature\SubSignatureTable.cs:13`), см. §2.6.
- `ALIAS` — `AliasType : UserdefType` (`AliasType.cs:15`), хранит `m_orgType` и
  `EffectiveType` (переопределён, `AliasType.cs:30`).
- `RangeType` = `SubrangeType` (`SubrangeType.cs:14`); `EnumerationType` = `EnumType`
  (`EnumType.cs:15`); `GenericType` = `GenericUserdefType` (`GenericUserdefType.cs:17`)
  и/или `AnyType` (`AnyType.cs:13`).

### 1.2.2. Полнота инвентаризации (доказательство)

- grep `class .*_IType` по `LanguageModelManager\**\*.cs` → **89** совпадений; из них 5 не
  являются языковыми типами: `TypeReference_Green`, `TypeExpression_Green`,
  `TypeDeclarationStatement`, `TypeReference`, `TypeExpression` (это AST-узлы). Остаётся
  **84** класса в иерархии `IECType`.
- Из 84: `IECType` — `abstract` (`IECType.cs:14`), значит **83 конкретных** класса.
- `LMDataType : LMEntity, ILMDataType` (`LMDataType.cs:13`) **не** входит в иерархию
  `IECType` — это DUT-метаданные, не тип IEC.
- Сверка с фабрикой: в `LanguageModelBuilder` (`LanguageModelBuilder.cs:457-3930`)
  ровно **83** различных метода `Create<X>Type` (по одному на каждый конкретный класс) плюс
  диспетчеры `CreateSimpleType(TypeClass)` (`:457`) и `CreateComplexType(string,out IMessage)`
  (`:463`), и `CreateDataType` (`:323`, для `LMDataType`). `Create*Type` **отсутствуют** в
  `RedTreeBuilder` (там только `_factory.CreateTypeReference/TypeExpression/HasTypeExpression`
  — AST-узлы, `TreeConversion\RedTreeBuilder.cs:670,912`) и `FactoryExtension`
  (`Parser35210\Utilities\FactoryExtension.cs` — литералы, не типы).
- Итог: **83 = 83**, расхождений нет.

### 1.2.3. Как парсер создаёт типы (`TypeParser` → `ILanguageModelBuilder6.CreateXType`)

Единственное место, где ST-синтаксис превращается в объекты типов —
`CODESYS.Parser35210.Declaration.TypeParser` (`Parser35210.plugin\...\Declaration\TypeParser.cs:12`).
Ключ: `LMItemFactory => Context.LMItemFactory` (`:18`), где `LMItemFactory` —
`_ILanguageModelBuilder6` (`:18`); `TypeTable` — `ITypeTable3` (`:20`).

| ST-синтаксис | Метод парсера | Вызов фабрики | file:line |
|---|---|---|---|
| `BOOL/INT/SAFEBOOL/ANY_INT/...` | `HandleOperatorCase` | `TypeTable.Get(Operator)` → `ParseSubrangeType` | `TypeParser.cs:122-193` |
| `base(low..high)` | `ParseSubrangeType` | `CreateSubrangeType(expLower,expUpper)` + `_Base` | `TypeParser.cs:63-79` |
| `POINTER TO t` | `ParsePointerType` | `CreatePointerType()` + `_Base` | `TypeParser.cs:470-485` |
| `REFERENCE TO t` | `ParseReferenceType` | `CreateReferenceType()` + `_Base` | `TypeParser.cs:449-468` |
| `STRING(n)` | `ParseStringType` | `CreateStringType()` + `Length` | `TypeParser.cs:378-398` |
| `WSTRING(n)` | `ParseWStringType` | `CreateWStringType()` + `Length` | `TypeParser.cs:356-376` |
| `XSTRING(n)` (`__XSTRING`) | `ParseXStringType` | `CreateXStringtype()` + `Length` | `TypeParser.cs:334-354` |
| `__VECTOR[n] OF t` | `ParseVectorType` | `CreateVectorType(base,dim)` | `TypeParser.cs:314-332` |
| `ARRAY[a..b,..] OF t` | `ParseArrayType` | `CreateArrayType()` + `AddDimension` | `TypeParser.cs:400-447` |
| `ARRAY[*] OF t` | `ParseArrayType` | `CreateVariableLengthArrayType()` | `TypeParser.cs:403-423` |
| `__PARAMS(n) OF t` | `ParseType` (case `Operator.Params`) | `CreateParamsType(base,count)` | `TypeParser.cs:210-224` |
| `(m1; m2; ..)` | `ParseEnumList` | `CreateImplicitEnumerationType(decls,"__IMPLICIT__ENUM")` | `TypeParser.cs:225-229` |
| `name<args>` | `ParseGenericUserdefType` | `CreateGenericUserdefType(qne)` | `TypeParser.cs:280-312` |
| `name` / `__SYSTEM.x` | `ParseUserdefType` | `CreateUserdefType(expname)` | `TypeParser.cs:266-278` |

### 1.2.4. Интерфейсы-маркеры и декораторы типов

| Интерфейс | Назначение | Кто реализует | source |
|---|---|---|---|
| `ISafetyType` | маркер F-варианта (`SAFE*`) | все `Safe*Type` | `Core\LanguageModel\ISafetyType.cs:6` |
| `ISpecialSizeType` | `CompatibilitySize`; `CodegeneratorType` — особый размер | `Retain*`, `Bool16Type` | `InternalInterfaces\ISpecialSizeType.cs:7` |
| `ITypeWithRecursiveTypeCheck` | защита от рекурсии при обходе | `Array`, `String`, `WString`, `XString`, `Vector` | `InternalInterfaces\ITypeWithRecursiveTypeCheck.cs:7` |
| `IHasEnumerableComponents` | перечислимые компоненты | `ArrayType` | `ArrayType.cs:19` |

### 1.3. `TypeClass` — порядок важен

`Core\LanguageModel\TypeClass.cs:6`: 49 значений в фиксированном порядке
(Bool=0 … LTimeOfDay=48). **Логика классификаторов в `TypeTable` арифметическая**
(`tc - TypeClass.X`), поэтому в Rust сохранить точную нумерацию:

```
Bool Bit Byte Word DWord LWord SInt Int DInt LInt USInt UInt UDInt ULInt
Real LReal String WString Time Date DateAndTime TimeOfDay Pointer Reference
Subrange Enum Array Params Userdef None Any AnyBit AnyDate AnyInt AnyNum
AnyReal Lazy LTime BitConst UXInt XWord XInt XString VarLenArray AnyString
__Vector LDate LDateAndTime LTimeOfDay
```

### 1.4. Таблица типов (`ITypeTable`)

Иерархия: `ITypeTable` → `ITypeTable2` (+SafeReal/SafeLReal) → `ITypeTable3`
(+`GetDirectVariableSizeInBits`, `IsPartialAccessSupportedType`).

Реализация — **`TypeTableClass`** (`Tools\TypeTableClass.cs:8`), singleton, **делегирует всё**
статическому **`TypeTable`** (`Tools\TypeTable.cs:10`) — там вся логика:

- builtin-инстансы конструируются в статическом конструкторе (`TypeTable.cs:13`).
- `Get(TypeClass)` (`:930`), `Get(string)` (`:1048`, hash-switch; fallback ANY* `:1017`), `Get(Operator)` (`:1275`).
- `GetTypeByOperator` (`:592`), `GetOperatorByType` (`:501`), `GetExternalOperatorName` (`:824`).
- классификаторы: `IsBlock :241`, `IsSigned :269`, `IsLType :293`, `IsBoolean :310`, `IsBit :316`,
  `IsNumber :322`, `IsTimeOrDateType :328`, `IsInteger :394`, `IsLInteger2 :406`, `IsReal :427`,
  `IsString :433`, `IsConcreterType :439`, `IsAnyType :468`, `IsConcreteType :495`.
- размеры: `GetSize2` (`:762`, -1 => вычислять по сигнатуре), `PointerSize :2385`,
  `ReferenceSize :2395`, `InterfaceSize :2401`, `GetDirectVariableSizeInBits :2448`,
  `IsPartialAccessSupportedType :2468`.
- диапазоны: `GetTypeRangeHigh :334`, `GetTypeRangeLow :364`; `GetCorrespondingSignedType :2407`.
- X-types: `IsResolvedXType :205`, `IsXType :211`, `IsLikePointer :217`,
  `GetEquivalent64BitTypeOfResolvedXType :223`, `ResolveUXIntType :195`.
- `GetStaticType(ICompiledType)` (`:67`): safety → Safe*, Bool16 → Bool16, иначе `Get(Class)`.
- `IsEquivalent` (`:115`): Byte↔USInt, Word↔UInt, DWord↔UDInt, LWord↔ULInt.

Размеры (константы, `TypeTable.cs:2687-2756`): Bool/Bit/BitConst/Byte/SInt/USInt=1;
Word/Int/UInt=2; DWord/DInt/UDInt/Real/Time/Date/DateAndTime/TimeOfDay=4; LWord/LInt/ULInt/LReal/
LTime/LDate/LDateAndTime/LTimeOfDay=8; Pointer/Reference/Interface = pointer size (default 4).

LMM-фасад: `LanguageModelManager\TypeTable.cs:8` (`=> CompilerProxy._TypeTable`), а
`CompilerProxy._TypeTable` (`CompilerProxy.cs:61`) берётся из `VersionedCompilerFactory`
(выбор версионного компилятора).

---

## 2. Области видимости (`CheckerScope`) и резолвинг

### 2.1. Иерархия scope-интерфейсов

`IScope` → `IScope2` (expression lookup) → `IScope3` (+PointerSize) → `IScope4` (+`GetCompiledPOUById`)
→ `IScope5` (`this[string]`, `SystemScope`, `ApplicationContext`, `LocalScope`, `FindVariableGlobal/Local`,
`CreateLocalScope`, …). Публичные precompile-scope: `IPrecompileScope` → `…2` (expression) → `…3`
(namespace) → `…4` (ApplicationGuid) → `…5` (`GetIdentifierInfo`) → `…6` (`GetAllDeclarations`) →
`…7` (`FindScope`) → `…8` (`GetAllDeclarations(EWhichDeclarations)`).

Внутренние: `_IScope` (+`PoolScope`), `_IScope2` (+`CreateLibraryScope`), `ICommonScope`
(`PointerSize`, `GetSize`, `FindSignature(IUserdefType|IExpression|IEnumType)`, `GetLiteralValue`,
`IsImplicitConvertable`, `IsEqual(ud1,ud2)`), `ICommonScope2` (+recursion-guarded `GetSize`).

### 2.2. Реализация `CheckerScope`

`Scopes\CheckerScope.cs:23` — единственная реализация. Поля/состояние:
- `LocalSignature` (`:1338`), `LocalContext` (`:148`) — текущий precompile-контекст,
- `PoolContext`, `RootApplicationGuid`, `ApplicationGuid` (`:1412`),
- `PointerSize` (по умолчанию 4; 8 если `CodegeneratorPropery LWordPointer`, `:187-211`),
- `IgnoreImplicitEnumMembers`, `IgnoreActions`, `LocalScope`.

Создание scope: конструкторы (`:26,:61,:96`), фабрики `CreatePrecompileScope` в `CompilerProxy.cs:231-255`.

### 2.3. Алгоритм разрешения имени (главный)

Вход: `FindDeclaration(string/expr)`. Ядро — `\u0001(name, out IVariable[], out ISignature[], out scope)`
(`CheckerScope.cs:1275`). Порядок ровно такой:

1. **Локальная переменная** — `\u0001(name, out sig)` (`:888`):
   `LocalContext[name]` (индексер `Signature.cs:1664`, регистронезависимо по `VersionedName`);
   если не найдено и у сигнатуры `contains_implicit_enum` — член неявного enum (`:713`);
   если есть `__GET<name>`/`__SET<name>` — синтез property-переменной (`:785`).
   Если `variable.Temp` и POU≠Action — игнор (`:880`).
2. **Sub-signature (метод/action/вложенная POU)** — `\u0001(name)` (`:1133`): DFS по
   `FindSubSignatureSet(sig.Name)` → `BaseExpression` (базовый FB) → `InterfaceExpressions`
   (интерфейсы); `Operator.Action` пропускается при `IgnoreActions` (`:1174`).
3. **Глобальные переменные** — `\u0001(name, out ISignature[])` (`:921`), 4 прохода
   (только если ещё не найдено):
   a) локальное приложение и родительские (`:940`);
   b) видимые библиотеки локального приложения (`:981`);
   c) Pool (`:1018`);
   d) видимые библиотеки родительских приложений (`:1043`).
   В каждом: `GetSignsForGlobalVar(name)` → `GetSignatureForPrecompileID` → `sig[name]`;
   фильтры `ATTRIBUTE_QUALIFIED_ONLY` и `SignatureFlag.Internal`.
4. **`this[name]`** (индексер, `:1086`): SuperGlobal-контекст (флаг `SuperGlobal`), затем
   `Pool[name]` (флаг `SuperGlobal`).
5. **Иерархический поиск** — `\u0002(name)` (`:1531`): перебор контекстов
   `GetContexts()` (`:1649`: локальное приложение + видимые библиотеки → корневое приложение +
   его видимые библиотеки → Pool + видимые библиотеки → SystemContext). Для каждого `ctx[name]`
   применяется фильтр `\u0001(name, libtable, ctx)` (`:1554`): отклонить, если чужой контекст и
   `GetQualifiedOnly`, или `SystemNamespaceForced`, или `Internal`.
6. **Namespace** — `\u0001(namespace)` (`:628`): `GetLibraryContextByNamespace` → scope; затем
   поиск члена (для `ICompoAccessExpression`/`INamespaceAccessExpression`, `:653/:675`).

Для `FindDeclaration` из строки (`:1621`): сначала пробуется разбор access-path (`:1590`);
если путь scope-квалифицирован (`__SYSTEM`/`@pool`/namespace/`.`), делегируем соответствующему
`CheckerScope`, иначе — шаги 1–6.

### 2.4. `FindSignature(IExpression)` (`:568`)
- `IVariableExpression` → по имени;
- `ISystemScopeExpression` → scope на `_SystemContext`;
- `IPoolScopeExpression` → scope на `Pool`;
- `INamespaceAccessExpression` → scope namespace → member;
- `_ICompoAccessExpression` → рекурсивно левый, затем правый;
- `ITypeExpression` → `_IUserdefType` → сигнатура.

### 2.5. Таблица символов `SymbolTable`

`Scopes\SymbolTable.cs:15` — предкомпилированный кеш: два слоя `ICaseInsensitiveDictionary<symbol>`
+ список property-сигнатур; наполнение (`:37`) с приоритетами (enum `global::\u0011.\u0005`,
значения `.u0001..\u000F`): локальный GVL → родительские приложения → видимые библиотеки →
локальные POUSignatures → system-context → pool → прочее. Индексер `:141`.
Это отдельный от `CheckerScope` путь для быстрой диагностики/оверлоада.

### 2.6. Член, перегрузки и конверсии

- **Член сигнатуры**: `Signature.this[string]` (`Signature\Signature.cs:1664`) — `m_htVariables`
  (`VersionedName`, регистронезависимо).
- **Sub-signatures/перегрузки**: `Signature\SubSignatureTable.cs:13`:
  `Add` (`:108`) — при коллизии имени помечает `shadowed`; если `overloaded`-атрибут — mangled-имя
  `` `Name@Type1@@Type2@` `` (`:164`); `GetSubSignature` (`:240`), `GetOverloadedSignatures` (`:260`),
  `CreateOverloadPlaceholderSignatures` (`:282`).
- **Разрешение перегрузки при вызове**: `\-\-.291.cs:27`:
  один кандидат → используется; ноль → базовая сигнатура; иначе точный проход (`:140`), затем
  «lazy/implicit-параметр» проход (`:114`); фильтрация по совместимости входов (`:121`);
  0 результатов → `Err_NoMatchingOverload` (`:249`), >1 → `Err_Ambiguity` (`:257`)
  (с `Inf_RelatedPosition` на каждую сигнатуру).
- **Контракт конверсий `ITypeComparer`** (`InternalInterfaces\ITypeComparer.cs:7`, `[ReleasedInterface]`):
  `Imitates(TypeClass,TypeClass,bTreatLRealAsReal,bTreatInt64AsInt32,b64BitPointer)` (`:9`),
  `IsEqual(ICompiledType,ICompiledType,ICommonScope[,scope2[,bForVarInout]])` (`:11,:13,:15`),
  `IsEqual(_IUserdefType,_IUserdefType,IScope2,IScope2)` (`:17`),
  `IsImplicitConvertable(ISignature,ISignature,IScope2)` (`:19`),
  `IsImplicitConvertable(ICompiledType,ICompiledType,ICommonScope[,scope2])` (`:21,:37`),
  `IsImplicitPointerConversion` (`:23,:35`), `IsCopyable` (`:25`), `IsInterface` (`:29`),
  `IsImplicitLiteralConvertable(_ILiteralExpression,...)` (`:31`),
  `EvaluateAliasAndEnumType` (`:33`).
- **Реализация** — `\u0006.\u0011` (`\-\-.304.cs:14`), `Imitates` (`:107`); создаётся
  `Services\CompilerServices.cs:41`.
- **Точка входа для типов** — `IECType.IsCompatible(ICompiledType,IScope2)` (`IECType.cs:96`) →
  `TypeComparerProxy.IsImplicitConvertable` (`TypeComparerProxy.cs:21`) →
  `VersionedCompilerFactory._TypeComparer` (`TypeComparerProxy.cs:12-16`). Устаревший
  `IECType.IsCompatible(ICompiledType)` — `IECType.cs:103`.
- **`IECType.IsEqual`** (`IECType.cs:181`): требует равный `Class`; для compound-классов
  (`Array/Userdef/Pointer/Enum/Reference/String/WString/__Vector`) дополнительно сравнивает
  `ToString()` регистронезависимо (`:191-197`). Отдельный precompile-путь —
  `IECType.IsEqualPreCompile` (`IECType.cs:201`, ветка GC ≥ 3.5.13.0 для `Enum`).
- **`BaseType`/`DeRefType`/`EffectiveType`**: по умолчанию `this` (`IECType.cs:64,74,162`);
  `AliasType.EffectiveType` разворачивает алиас (`AliasType.cs:30`), `ReferenceType.DeRefType`
  снимает ссылку (`ReferenceType.cs:72`), `SubrangeType.DeRefType` — базу (`SubrangeType.cs:113`).

### 2.7. Типизация выражений

`PreCompile\Typification\SimpleTypeInferrer.cs:23` (наследует visitor `\u0001.\u0007`):
- нормализация «предпочтительного» типа (`AnyBitButBoolIsPreferred→Bool`, Any*→конкретика `:64`);
- вывод типа переменных/операторов/вызовов (`_IVariableExpression :234`, `_IOperatorExpression :277`);
- сбор ссылок (`\u0018.\u0001`), дедуп и выбор «шире» по размеру (`:430`).
`SimpleTypeChecker` — проверка типов операторов; `PreCompileTypifier` — драйвер.

---

## 3. `ITypeTable` и `ICompiledPOU` — контракты для Rust

### 3.1. `ITypeTable`
Rust-аналог: `trait TypeTable { fn bool_(&self)->TypeRef; … fn get_by_class(&self, c:TypeClass)->Option<TypeRef>; fn get_by_name(&self,&str)->Option<TypeRef>; fn size(&self,c:TypeClass,scope:&dyn Scope)->i32; fn pointer_size(&self,scope:Option<&dyn CommonScope>)->i32; fn is_*(&self,c:TypeClass)->bool; fn range_high/low; fn get_static_type(&self,&dyn CompiledType)->TypeRef; }`
Семантика — ровно из `Tools\TypeTable.cs` (см. §1.4).

### 3.2. `ICompiledPOU`
`Core\LanguageModel\ICompiledPOU.cs:7` (публичный) + `InternalInterfaces\_ICompiledPOU.cs:9`
(внутренний). Реализация — `CompiledPOU` (`LanguageModelManager\CompiledPOU.cs:23`):
`ParseTree` (через `ParseTreeProvider`), `Name`, `SignatureId`, `Checksum`, `Messages`/`PrecompileMessages`,
`GetFlag(CompiledPOUFlags)`, `Accept(IExprementVisitor)`, `GetFullName(_ICompileContext)`, `Duplicate`.
Rust: `struct CompiledPou { name:String, signature_id:u32, checksum:u32, parse_tree:Option<Statement>, messages:Vec<CompilerMessage>, flags:CompiledPouFlags }`.

---

## 4. Чек-лист переноса на Rust

1. **TypeClass** — скопировать enum в точном порядке 0..48; тесты на `tc as i32 ± N`.
2. **Sizes/ranges** — перенести таблицы из `Tools\TypeTable.cs` (`GetSize2`, `GetTypeRange*`, `*_Size`).
   Pointer/Reference/Interface = `scope.pointer_size` (default 4).
3. **Классификаторы** — перенести как есть (арифметика диапазонов), покрыть unit-тестами по всем 49 классам.
4. **X-types** — `UXInt/XWord/XInt` резолвятся в UDInt/ULInt… по pointer size (4/8) (`IsResolvedXType`, `GetEquivalent64BitTypeOfResolvedXType`).
5. **IsEquivalent** — Byte↔USInt, Word↔UInt, DWord↔UDInt, LWord↔ULInt.
6. **Типы** — базовый `trait IecType { fn class(&self)->TypeClass; fn base_type; fn deref_type; fn size(&self,&dyn Scope)->i32; fn is_equal; fn components(&self,&dyn Scope)->Vec<Component>; }`; реализации: scalar-синглтоны, Array{base,dims}, Pointer, Reference, Subrange, Enum{sig}, Userdef{name,sig,scope}, String/WString/XString, Any*, Safe*/Retain* (делегат), X-types.
7. **UserdefType** — размер и члены — из сигнатуры; `ToString` особые случаи `__Union`/`__PARAMS`; сравнение по `SignatureId`+`ScopeId`.
8. **EnumType** — `_Base` (default Int), значение члена = литерал init; конверсия int↔имя.
9. **Scope** — `trait Scope { fn local_signature; fn find_declaration(&self,name)->Option<Resolved>; fn find_signature(&self,expr); fn this_name(&self,&str)->Vec<SigRef>; }`; реализовать порядок §2.3 (S0→S9) **буквально**.
10. **Регистронезависимость** — все словари и сравнения имён — case-insensitive.
11. **SubSignatureTable** — таблица sub-сигнатур + оверлоады + mangled-имена.
12. **Overload resolution** — 2-проходный алгоритм `\-\-.291.cs` с кодами ошибок `Err_NoMatchingOverload`/`Err_Ambiguity`.
13. **ITypeComparer** — `Imitates`, `IsImplicitConvertable(type,type,scope)`, `IsImplicitLiteralConvertable`, `IsCopyable`; тесты на byte↔usint, int-promotion, literal-fit, pointer/interface.
14. **SymbolTable** — 2-слойный case-insensitive индекс с приоритетами (для диагностики/оверлоада).
15. **CompiledPOU** — контейнер parse tree + signature_id + messages.
16. **TypeParser** — перенести соответствие ST-синтаксис → `Create*Type` из
    `Parser35210\...\Declaration\TypeParser.cs` (§1.2.3); учесть `TypeTable.Get(Operator)`,
    `__XSTRING`/`__VECTOR`/`__PARAMS`/`ARRAY[*]`/inline-enum; ST-ключевые слова SAFE/BOOL16/__X*.
17. **Инвентаризация типов** — сверить с `tables\iec_types.csv` (84 класса); проверить, что на
    каждый конкретный класс приходится ровно один `Create<X>Type` (`LanguageModelBuilder.cs:457-3930`).

---

## 5. Пробелы / риски

1. **Обфускация**: ключевые классы/поля названы `\u00XX` (overload resolver `\u001F.\u0010`,
   comparer `\u0006.\u0011`, factory `\u0019.\u0003`, base-inferrer `\u0001.\u0007`, enum приоритетов
   `\u0011.\u0005` в `SymbolTable`). file:line стабильны, имена нужно восстанавливать по IL.
2. **`Core.dll` не managed** — декомпиляция дала 0 типов; если в нём ожидались `IType`,
   они фактически в `Compiler.dll`.
3. **Configurators/Legacy** — системы типов в них нет (только UI опций и legacy-facade);
   исходная гипотеза о расположении реализаций не подтвердилась.
4. **`\-\-.304.cs` (comparer) и `\-\-.291.cs` (overload)** — разобраны частично; при порте
   переносить целиком по IL (1526 и 305 строк).
5. **`IArrayDimension`, `SubrangeType` border-выражения, `IStructureInitialization`** —
   вычисления границ/инициализаторов зависят от `scope` и constant folder (`ConstantFolding*`).
6. **Версионные ветки** (`CompilerVersionMgr.GreaterEqualV35xxx`) пронизывают типы/скопы —
   для 3.5.22.10 зафиксировать ветки `>= 3.5.2200`.
7. **Нет отдельных классов** `RangeType` / `EnumerationType` / `StructureType` / `UnionType` /
   `FunctionBlockType` / `InterfaceType` / `MethodType` / `GenericType` — не искать их в порту;
   см. §1.2.1 (`UserdefType` + `Signature` + `SubSignatureTable`).
8. **`LMDataType : LMEntity`** (`LMDataType.cs:13`) — DUT-метаданные (`DUTGuid`), **вне**
   иерархии `IECType`; не включать в `trait IecType`.
9. **Типы создаются вне LMM** — фабрика вызывается из парсера
   (`Parser35210.plugin\...\TypeParser.cs`), а builtin-синглтоны живут в
   `Compiler35220\Tools\TypeTable.cs`; при порте нужны оба источника, а не только LMM.
10. **`XStringType`** реализует одновременно `_IStringType` и `_IWStringType` — узкий/широкий
    режим различается по `Class=TypeClass.XString` и context (`DefaultStringEncodingService`).
11. **Производные X-типы** (`XDInt/XDWord/XLInt/XLWord/XUDInt/XULInt`) не переопределяют
    `Class` — наследуют `DInt/DWord/LInt/LWord/UDInt/ULInt`; в Rust проверить, что
    `Class`≠`__X*`, а вид определяется `ToString()`/оператором (`Operator.__XDINT` и т.п.).
12. **`RangeAwareAnyIntType`** (`AnyIntType`, `:15`) несёт `MinValue/MaxValue` и
    `ResolveIntegerType(comcon)` (`:28`) — отдельная логика выбора INT/DINT по диапазону
    (`_integerTypes`); легко потерять при «сведении» любого ANY_INT к синглтону.

---

## 6. Ссылки
- Интерфейсы: `decompiled\Compiler\_3S\CoDeSys\LanguageModelManager\InternalInterfaces\`, `…\Core\LanguageModel\`
- Таблица типов: `decompiled\Compiler35220.plugin\_3S\CoDeSys\Compiler35220\Tools\TypeTable.cs`, `TypeTableClass.cs`
- Scopes/резолвинг: `…\Compiler35220\Scopes\CheckerScope.cs`, `SymbolTable.cs`; `-\-.291.cs`, `-\-.304.cs`
- Типы: `decompiled\LanguageModelManager.plugin\_3S\CoDeSys\LanguageModelManager\` (`IECType.cs`, `*Type.cs`, `Signature\Signature.cs`, `Signature\SubSignatureTable.cs`, `LanguageModelBuilder.cs`, `CompiledPOU.cs`, `CompilerProxy.cs`, `TypeComparerProxy.cs`)
- Парсер типов: `decompiled\Parser35210.plugin\CODESYS\Parser35210\Declaration\TypeParser.cs`, `...\Utilities\FactoryExtension.cs`
- Флаги сигнатур: `decompiled\Compiler\_3S\CoDeSys\Core\LanguageModel\SignatureFlag.cs`
- Маркеры типов: `...\Core\LanguageModel\ISafetyType.cs`, `...\InternalInterfaces\ISpecialSizeType.cs`, `...\InternalInterfaces\ITypeWithRecursiveTypeCheck.cs`
- Таблицы: `tables\type_system.csv`, `tables\iec_types.csv` (84 строки), `tables\scopes.csv`
