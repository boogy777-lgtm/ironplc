# AST: конкретные классы узлов (red/green) — завершение карты

Закрывает пробел «логика конкретных узлов не декомпилирована / `producer` пуст» из
`04_AST_MODEL.md` и добивает сопоставление «интерфейс ↔ конкретный класс» до максимума.
Источник — `decompiled\LanguageModelManager.plugin\_3S\CoDeSys\LanguageModelManager\`
(пространство `_3S.CoDeSys.LanguageModelManager`), фабрики — `RedTreeBuilder` +
`LanguageModelBuilder`, парсер — `Parser35220.plugin`.

## 1. Метод сопоставления (по интерфейсу, а не по имени)

Раньше `concrete_impl` заполнялся по совпадению имени класса (`IXxx` → `Xxx`), поэтому
пропускались узлы с иным именем класса и все классы, реализующие `_IExprement`
(например `ElseIf`). Теперь скрипт `tools\build_concrete_map.py`:

1. сканирует **все** классы в `LanguageModelManager` (кроме `GreenTrees\` и `RedTrees\`);
2. для каждого класса собирает реализованные интерфейсы в обеих формах — внутренней
   `_IXxx` и публичной `IXxx`, включая версионные `IXxx2..12`;
3. строит отображение «интерфейс → класс» и для каждой строки `ast_nodes.csv` берёт
   интерфейс из `full_name`. Поэтому корректно разрешаются:
   * `_IElseIf` → `ElseIf`;
   * `_IArrayInitialization` → `ArrayInitialisation` (британское написание);
   * `_IExpressionStatement` → `ExpressionStatement`;
   * `_IIntType` → `IntType` и т.п.;
4. для абстрактных корневых интерфейсов берёт базовый класс
   (`IExpression`→`Expression`, `IStatement`→`Statement`, `IExprement`→`Exprement`,
   `IType`/`ICompiledType*`→`IECType`);
5. зелёный двойник — `<Класс>_Green` (в `GreenTrees\`).

## 2. Числа: было → стало

| Метрика | Было | Стало | Δ |
|---|---|---|---|
| строк в `ast_nodes.csv` | 691 | **786** | +95 (добавлены пропущенные интерфейсы-узлы) |
| `concrete_impl` | 84 | **508** | +424 |
| `green_impl` (новая колонка) | 0 | **100** | +100 |
| `red_factory_method` | 118 | **313** | +195 |
| `producer` | 81 | **226** | +145 |

AST-узлы (kind = statement/expression/type): **218**, из них concrete — **216**,
factory — 198, producer — 218 (103 метод парсера + 115 «нет (декларация/типизация)»).
Все 95 новых строк — реальные узлы/типы, отсутствовавшие в таблице (перечислены ниже).

## 3. Спецразбор проблемных семейств

| Интерфейс | Класс-реализация | Файл |
|---|---|---|
| `_IElseIf` / `_IElseIf2` | `ElseIf` (наследник `Exprement`, не `Statement`!) | `ElseIf.cs:15` |
| `_ICase` | `Case` | `Case.cs:14` |
| `_ICaseLabelStatement` | `CaseLabelStatement` | `CaseLabelStatement.cs` |
| `_ICaseStatement` | `CaseStatement` | `CaseStatement.cs` |
| `_IExpressionStatement` | `ExpressionStatement` | `ExpressionStatement.cs` |
| `_IArrayInitialization` | `ArrayInitialisation` | `ArrayInitialisation.cs` |
| `_IMultipleIndexInitialization` | `MultipleIndexInitialisation` | `MultipleIndexInitialisation.cs` |
| `_IStructureInitialization` | `StructureInitialisation` | `StructureInitialisation.cs` |
| `_ICompiledType*`, `IType` | `IECType` (абстрактный корень) | `IECType.cs` |
| `IArrayType` | `ArrayType` | `ArrayType.cs` |
| `IEnumType` | `EnumType` | `EnumType.cs` |
| `IPointerType` | `PointerType` | `PointerType.cs` |
| `IReferenceType` | `ReferenceType` | `ReferenceType.cs` |
| `IStringType` | `StringType` | `StringType.cs` |
| `IWStringType` | `WStringType` | `WStringType.cs` |
| `IUserdefType` | `UserdefType` | `UserdefType.cs` |
| `ISubrangeType` | `SubrangeType` | `SubrangeType.cs` |
| `IVectorType` | `VectorType` | `VectorType.cs` |
| `IIntType`, `IRealType`, `IBoolType`, … | `IntType`, `RealType`, `BoolType`, … | `*Type.cs` |

**Ключевой случай `_IElseIf`.** Класс `ElseIf : Exprement, _IElseIf, _IExprement, IElseIf2, IElseIf`
(`decompiled\...\LanguageModelManager\ElseIf.cs:15`) не является наследником `Statement`,
поэтому прежний фильтр по `_IStatement/_IExpression/_IType` его не находил. Сопоставление
по интерфейсу решает проблему.

**Добавленные строки (95).** Пропущенные интерфейсы-узлы: `IItemReference`, `IPragmaExpression`,
`IProgramCounterExpression`, `ICopyScopeExpression`, `IFramePointerExpression`, `IHasCompatibleTypeExpression`,
`IImplicitConversionExpression`, `IImplicitDeRefAccessExpression`, `IFloatLiteralExpression`,
`IIntegerLiteralExpression`, `IStringLiteralExpression`, `IProjectDefinedExpression`, `IXRefExpression`,
`IResourceReference`, `ITaskReference`, `ILocalSignatureIdPragma`, `IMessageGuidPragmaStatement`,
`IMethodDeclarationStatement`, `INullStatement`, `ISubRoutineStatement`, `ITryCatchStatement`,
`IVarInitialEmptyStatement` и скалярные IEC-типы (`IIntType`, `IRealType`, `ISafe*Type`, `IRetain*Type`,
`IX*Type`, `IL*Type`, `Any*Type`, `GenericUserdefType`, `ParamsType`, `VariableLengthArrayType`, …).

## 4. `red_factory_method` и `producer`

* `red_factory_method` (313) восстановлен из реального кода:
  * `RedTreeBuilder.visit(_IXxx)` → `ITreeFactory.CreateYyy` — 70 узлов
    (`decompiled\Compiler35220.plugin\...\TreeConversion\RedTreeBuilder.cs`);
  * `LanguageModelBuilder.CreateYyy` — типы/сервисы (`LanguageModelBuilder.cs`);
  * литералы выбираются по рантайм-типу (`CreateFloatLiteralExpression`,
    `CreateIntegerLiteralExpression`, `CreateStringLiteralExpression`);
  * явные вызовы builder из `tables\ast_builder_map.csv`.
* `producer` (226): метод в `Parser35220.plugin` в формате `путь:строка`
  (`\Statements\IfStatementParser.cs:110` и т.п.). Курированные значения закреплены в
  `PRODUCER_OVERRIDE` внутри `tools\build_concrete_map.py`, чтобы регенерация базовой
  таблицы из бинарников их не теряла. Узлы, которые парсер не создаёт (декларации,
  типы, строящиеся фазой типизации), получают `нет (декларация/типизация)` (115 строк
  среди AST-узлов).

## 5. Полное покрытие: узлы без `concrete` (278)

Остались **только** сервисные/инфраструктурные интерфейсы; среди AST-узлов без
конкретного класса ровно **2**:

| Узел | Причина |
|---|---|
| `ISafetyType` | маркерный интерфейс: реализуют 16 классов `Safe*Type` (нет одного конкретного) |
| `INewExpression2` | `_INewExpression2` не реализуется ни одним классом в LMM (версионный интерфейс) |

Группировка остальных 276 по причине:

### 261 — сервисный интерфейс (реализация вне LMM / Core)
IAccessInfo, IAccessInfo2, IAccessMode, IAddressCalculator, IAddressCalculatorUnittestSupport, IApplicationContent, IApplicationContent2, IArchiveAuxiliaryWriter, IAtomicLoadStore64BackEnd, IBreakPointTable, IC541MessageDecoratorInfoProvider, ICPPCompatibleBackEnd, ICPPCompatibleCodegenerator, ICheckAllPoolObjectsConfigurationProvider, ICodeAdapter, ICodeAdapter2, ICodeAdapter3, ICodeAdapter4, ICodeAdapter5, ICodeAdapter6, ICodeGeneratorAttributes, ICodePiece, ICodePiece2, ICodePiece3, ICodeRelocater, ICodeRelocater2, ICodeRelocator64, ICodegenerator, ICodegenerator10, ICodegenerator11, ICodegenerator12, ICodegenerator2, ICodegenerator3, ICodegenerator4, ICodegenerator5, ICodegenerator6, ICodegenerator7, ICodegenerator8, ICodegenerator9, ICompiledElementInfoStruct, IContainerLibIssue, IContainerLibraryChecker, IDUTInfoStruct, IDataLocationInformation, IDataLocationInformation2, IDataLocationInformation3, IDeclarationInfo, IDeclarationInfo2, IDeclarationInfo3, IDeclarationInfo4, IDeferrableLanguageModelProvider, IDeferrableLanguageModelProvider2, IDeviceSpecificProperties, IDirectCallRelocation, IDisassembler, IDisassembler2, IDisassembler3, IDisplayNameParser, IDownloadInfo, IDownloadInfo2, IDownloadInfo3, IDownloadInfo4, IDownloadInfo5, IDownloadInfo6, IDownloadInfo7, IDownloadInfo8, IExplicitExpressionAtSourcePositionProvider, IExprVisitor, IExprVisitor2, IExprVisitor3, IExprVisitor4, IExprVisitor5, IExprVisitor6, IExprVisitor7, IExprVisitor8, IExprVisitor9, IExpressionInfo, IExpressionTypifier, IExpressionTypifier2, IExpressionTypifier3, IExpressionTypifier4, IExpressionTypifier5, IExpressionTypifier6, IExternalPluginFoundIssue, IExternalReference, IExternalReference2, IFBInfoStruct, IGVLInfoStruct, IImplicitDeclarationSnippetProvider, IIndexInfo, IJumpTable, ILMCallTreeService, ILMCompiledParseTreeService, ILMNameManglingService, ILMPreCompileTypifier, ILMPreCompileTypifier2, ILMQualifierService, ILMStringEncodingService, ILMTransitionUserCodeAnalyzerService, ILMTypeService, ILanguageModelBuildPropertiesControl, ILanguageModelManager, ILanguageModelManager10, ILanguageModelManager11, ILanguageModelManager12, ILanguageModelManager13, ILanguageModelManager14, ILanguageModelManager15, ILanguageModelManager16, ILanguageModelManager17, ILanguageModelManager18, ILanguageModelManager19, ILanguageModelManager2, ILanguageModelManager20, ILanguageModelManager21, ILanguageModelManager22, ILanguageModelManager23, ILanguageModelManager24, ILanguageModelManager25, ILanguageModelManager26, ILanguageModelManager27, ILanguageModelManager28, ILanguageModelManager29, ILanguageModelManager3, ILanguageModelManager30, ILanguageModelManager4, ILanguageModelManager5, ILanguageModelManager6, ILanguageModelManager7, ILanguageModelManager8, ILanguageModelManager9, ILanguageModelProvider, ILanguageModelProvider2, ILanguageModelProvider3, ILanguageModelProviderBuildPropertiesControl, ILanguageModelProviderBuildPropertiesControl2, ILanguageModelProviderWithDependencies, ILanguageModelSnippetProvider, ILibraryPlaceholderResolution, ILibraryPlaceholderResolution2, ILibraryPlaceholderResolution3, ILibraryPlaceholderResolution4, ILibraryPlaceholderResolutionEx, ILibraryPlaceholderResolutionEx2, ILibraryPlaceholderResolutionEx3, ILicensedSoftwareMetricCheckableByGeneratedCode, ILicensedSoftwareMetricInformationProvider, ILicensedSoftwareMetricInformationProvider2, ILiteralValue, ILiteralValue2, IMemoryAllocationCallback, IMemoryAllocationCallbackEmbedded, IMemorySettingsProvider, IMemoryStatisticsOutputProvider, IMethodInfoStruct, IMissingPublishSymbolsInContainerIssue, INamespaceConflictChecker, INamespaceConflictIssue, INotAllowedSignatureInContainerLibIssue, IOnlineChangeDetails, IOnlineChangeDetails2, IOnlineChangeDetails3, IPOUInfoStruct, IPOUMethodInfoStruct, IPOUSyntax, IParameterAddressInfo, IParseTreeStreamProvider, IParser, IParser2, IParser3, IParser4, IParser5, IPragmaNotifier, IPrecompilePositionInfo, IPrecompilePositionInfo2, IPrecompilePositionInfo3, IPrecompilePositionInfo4, IPrecompilePositionInfo5, IPrecompileScope, IPrecompileScope2, IPrecompileScope3, IPrecompileScope4, IPrecompileScope5, IPrecompileScope6, IPrecompileScope7, IPrecompileScope8, IPrecompileScopeWithAliasService, IPredefinedCalleeTypeProvider, IReferenceCompileContextService, IRegister, IRegister2, IRegisterManager, IRegisterManager2, IRegisterManager3, IRelocationEntry, IRiscBackEnd, IRiscBackEnd10, IRiscBackEnd11, IRiscBackEnd12, IRiscBackEnd13, IRiscBackEnd2, IRiscBackEnd3, IRiscBackEnd4, IRiscBackEnd5, IRiscBackEnd6, IRiscBackEnd7, IRiscBackEnd8, IRiscBackEnd9, IRiscBackEndDirectCall, IRiscBackEndDisassembler, IRiscBackEndExceptionInfo, IRiscBackEndMisalignedAccess, IRiscBackEndVectorUnit, IRiscBackEndWithMemoryOperands, IRiscBackEndWithMemoryOperands2, IRiscCompiledCode, IRiscCompiledCode2, IRiscCompiledCode3, IRiscCompiledCode4, IRiscCompiledCode5, IRiscCompiledCodeEntry, IRiscFrontEnd, IRiscFrontEnd2, IRiscFrontEnd3, IRiscJumpTable, IRiscLabel, IRiscOptions, IRiscRelocationList, IRiscRelocationList2, IScanner, IScanner2, IScanner3, IScanner4, IScanner5, IScanner6, IScanner7, IScanner8, IScanner9, IScope, IScope2, IScope3, IScope4, IScope5, ISecondLevelPlaceholderResolution, ISecondLevelPlaceholderResolution2, ISignatureAttribute, IStackUsage, IStackUsageEntry, IStructuredImplicitDeclarationSnippetProvider, IStructuredLanguageModelProvider, IStructuredLanguageModelProviderDelayedSupport, IStructuredLanguageModelSnippetProvider, ISubroutineCodegenerator, ITaskCrossref, ITaskStackSizeProvider, IToken, ITransitionUserCodeAnalyzationResult, ITryCatchBackEnd, IVarConfigCodeGenerator, IVariableInfo, IVersionedLicensedSoftwareMetricCheckableByGeneratedCode

### 3 — маркерный интерфейс: реализуют 15 классов
IAddressInfo2, IAddressInfo3, IAddressInfo4

### 2 — маркерный интерфейс: реализуют 16 классов
IAddressInfo, ISafetyType

### 2 — неоднозначно: CompileContext, PreCompileContext
ICompileContextCommon, ILMPouSet

### 2 — неоднозначно: FunctionPointerEntry, InterfaceOffsetEntry
IVFTableEntry, IVFTableEntry2

### 1 — неоднозначно: Signature, Variable
IHasAttributes

### 1 — неоднозначно: GenericAttribute, GenericCheckedAttribute
IAttribute

### 1 — неоднозначно: FBVisuCreatorAttributeProvider, LMMAttributeProvider
IAttributeProvider

### 1 — неоднозначно: BreakpointList, LittleBreakpointList
IBreakpointCollection

### 1 — неоднозначно: LibraryTableExtern, LibraryTableWithPlaceholders, LibraryTableWithoutPlaceholders
ILibraryTable

### 1 — неоднозначно: LibraryTableExtern, LibraryTableWithoutPlaceholders
ILibraryTable2

### 1 — нет red-класса в LMM
INewExpression2

### 1 — неоднозначно: PropertyAddressInfo, PropertyAddressInfoExtended
IPropertyAdressInfoWithOffset

## 6. Инструменты (в зоне `tools\`)

| Скрипт | Назначение |
|---|---|
| `extract_ast_nodes.ps1` | регенерирует `tables\ast_nodes.csv` из `Compiler.dll` (691 интерфейс `_3S.CoDeSys.Core.LanguageModel`) — источник истины для колонок `kind/base_type/full_name/key_fields` |
| `build_concrete_map.py` | джойн интерфейсов с классами/фабриками/парсером, добавляет пропущенные строки, обновляет `ast_nodes.csv` и `red_tree_nodes.csv` |
| `verify_ast_map.py` | печатает статистику покрытия и список узлов без `concrete` с причинами |

Порядок запуска:

```
powershell -File tools\extract_ast_nodes.ps1
python tools\build_concrete_map.py
python tools\verify_ast_map.py
```

`red_tree_nodes.csv` (184 класса) регенерируется вместе с `ast_nodes.csv`: исправлены
`primary_interface` (теперь самый специфичный `_IXxx`, а не `IECType`) и добавлена колонка
`interfaces` (все реализованные интерфейсы класса).

## 7. Rust-порт (уточнения)

1. Каждый узел — `enum`-вариант с полями `m_*`; позицию хранить в базовом `Span`
   (аналог `PositionStatement`/`PositionExpression`).
2. Повторить **Null-полиморфизм**: `Condition()`/`IfThen()` возвращают Null-узлы,
   а не `None` — иначе расхождение с CODESYS.
3. Реализовать `Accept`/`visit` как `trait AstNode { fn accept(&self, v: &mut dyn Visitor); }`.
4. `Duplicate()` — deep clone; нужен при построении red из green.
5. Учитывать `[DefaultSerialization]`/`[StorageVersion]`/`[TypeGuid]` только если нужна
   бинарная совместимость.
6. `ElseIf` — отдельный вариант, не часть `Statement`; при переносе `IF` держать его
   наравне с `IfStatement` (наследуется от `Exprement`).
7. Типы (`IECType`-производные) — `Trait`/`enum` со статическими `IEC`-экземплярами;
   `ICompiledType*` — один базовый тип-контейнер.
