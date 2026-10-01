# AST: конкретное построение red-tree (самый сложный слой)

Дополняет `04_AST_MODEL.md` (интерфейсы/иерархия) и `06_AST_BUILDER_MAP.md` (парсер → builder).
Здесь — **где физически живёт алгоритм построения AST** и **как узел создаётся**.

Источник (декомпил, CODESYS 3.5.22.10):
- `decompiled\Compiler35220.plugin\_3S\CoDeSys\Compiler35220\TreeConversion\RedTreeBuilder.cs` — visitor, строящий red-tree.
- `decompiled\LanguageModelManager.plugin\_3S\CoDeSys\LanguageModelManager\RedTrees\Builder\` — конкретные fluent-билдеры.
- `decompiled\Compiler\_3S\CoDeSys\{Core\LanguageModel, Compiler\LanguageModelBuilder, LanguageModelManager\InternalInterfaces}` — интерфейсы.

## 1. Модель «трёх деревьев» CODESYS

| Дерево | Тип | Кто создаёт | Назначение |
|---|---|---|---|
| **Green tree** | `_IExprement` (зелёный, неизменяемый) | парсер `Parser35220` через `ILanguageModelBuilder7` | «сырой» разбор |
| **Red tree** | `_IStatement` / `_IExpression` (красный, изменяемый) | `ITreeFactory` | рабочее AST (типизация, ссылки, позиции) |
| **White tree** | `CODESYS.WhiteParseTrees.*` | `WhiteParsetrees.plugin` | сохранение форматирования (для редактора) |

Конвейер построения AST:

```
InternalScanner.GetNext
   → InternalParser.ParseST / ParsePOU / ...
      → sub-parsers (ExpressionParser/StatementParser/...)      [green nodes]
         → ILanguageModelBuilder7.*  (создание green-узлов)
   → LanguageModelOfRawST.CreateLanguageModelOfRawST            [InternalParser.cs:396]
```
Типизированная (red) форма — из green через visitor:

```
RedTreeBuilder.BuildRedTree(greenExp, ITreeFactory, ICompactedParseTreeInformation)   RedTreeBuilder.cs:19
   → exp.Accept(visitor)          // обход green-дерева
   → ITreeFactory.CreateXxx(...)  // создание red-узла (см. таблицу методов)
   → PrecompileParseTreeInformationSetter.SetInformationInParseTree(...)   // позиции/типы
```

`RedTreeBuilder` — под `_IExprementVisitor*` (версионные интерфейсы 3590…352000), т.е. AST строится обходом green-дерева, узел за узлом, с дублированием типов (`_IType.Duplicate`) и переносом `Flags`.

## 2. Два способа создания узлов

### 2.1 Fluent-билдеры (использует парсер напрямую)
Фабрика `RedTreeBuilderFactory : IRedTreeBuilderFactory` (`[TypeGuid {A06288A8-B84B-433C-8EA7-1AE3C85E1A73}]`), `RedTrees\Builder\RedTreeBuilderFactory.cs:13`.
Свойства-билдеры → конкретные классы (полная таблица: `tables\red_tree_builders.csv`, 11 строк):

`CallBuilder, OperatorBuilder, IfBuilder, ForBuilder, CaseBuilder, WhileBuilder, TypeDeclarationBuilder, ElseIfBuilder, PouDeclarationBuilder, EnumDeclarationListBuilder, VariableDeclarationListBuilder`.

### 2.2 Фабрика узлов `ITreeFactory` (использует TreeConversion)
Метод `BuildRedTree` вызывает `_factory.CreateXxx(...)`. Полный список методов → узлы и `file:line`:
**`tables\red_tree_factory_methods.csv`** (82 строки: 29 statement + 53 expression).

Узлы-интерфейсы (`_3S.CoDeSys.Core.LanguageModel.*`): `_ISequenceStatement`, `_IIfStatement`, `_IElseIf`, `_IWhileStatement`, `_IRepeatStatement`, `_IForStatement`, `_ICaseStatement`/`_ICase`/`_ICaseLabelStatement`, `_IReturnStatement`, `_IJumpStatement`, `_IAssignmentExpression`, `_ICallExpression`, `_IOperatorExpression`, `_IConversionExpression`, `_ICastExpression`, `_INewExpression`, `_IVariableExpression`, `_ICompoAccessExpression`, `_IIndexAccessExpression`, `_IDeRefAccessExpression`, `_ILiteralExpression` (+Integer/Float/String/BasedInteger), `_ITryCatchStatement`, `_IPragma*`, `_IError*`/`_INull*` и др.

Часть фабрик версионная (`ITreeFactory2..7`): `CreateBreakPointStatement` (v2), `CreateStringLiteralExpression`/`CreateHasConstantTypeExpression` (v3), `CreatePartialAccessExpression` (v4), `CreateProjectDefinedExpression` (v5), `CreateLocalSignatureIdPragma`/`CreateImplicitCodeSectionPragma` (v7).

Исключения (в `RedTreeBuilder` кидают `NotImplementedException`, т.к. обрабатываются иначе): `visit(_ICompiledPOU)`, `_ICopyScopeExpression`, `_IQualifiedNameExpression`, `_IVariableDeclarationStatement`, `_IVariableDeclarationListStatement`, `_ITypeDeclarationStatement`, `_IEnumDeclarationStatement`, `_IEnumDeclarationListStatement` (декларации строятся фазой типизации, не TreeConversion).

## 3. Где что (карта файлов)

| Что | Путь |
|---|---|
| Visitor (green→red) | `decompiled\Compiler35220.plugin\...\TreeConversion\RedTreeBuilder.cs` |
| Перенос позиций/типов в red | `...\TreeConversion\Precompile*ParseTreeInformation{Collector,Setter}.cs` |
| Green-дерево/GUI-мост | `...\TreeConversion\GreenTreeBuilder.cs`, `SimpleGreenTreeVisitor.cs`, `IGreenTreeVisitor.cs` |
| Fluent-билдеры | `decompiled\LanguageModelManager.plugin\...\RedTrees\Builder\**` |
| Фабрики узлов | `_3S.CoDeSys.LanguageModelManager.InternalInterfaces.ITreeFactory[2..7]` |
| Red-узлы (классы) | `decompiled\LanguageModelManager.plugin\...\` (реализации `_IStatement/_IExpression`) |

## 4. Чек-лист порта на Rust

1. **AST как tagged enum** по `tables\red_tree_factory_methods.csv`: `enum Expr { Call(..), Operator(..), Conversion(..), Cast(..), New(..), Variable(..), Literal(Lit), Compo(..), Index(..), DeRef(..), .. }`, `enum Stmt { If{..}, Case{..}, For{..}, While{..}, Repeat{..}, Return(..), Jump(..), TryCatch{..}, .. }`.
2. **Построение** — два прохода как в CODESYS: парсер выдаёт green-узлы (`builder`-trait), затем visitor `red_build(green) -> Red` (порт `RedTreeBuilder::visit_*`), с `Duplicate` типов и переносом `Flags`.
3. **Позиции** — переносить в red при построении (аналог `PrecompileParseTreeInformationSetter`): `Span{start,end}`, `SourceOffset`, `Line`, `Column`.
4. **Версионные фабрики** (`ITreeFactory2..7`) — смоделировать опциональными методами/feature-флагами по версии компилятора (для 3.5.22.10 доступны все).
5. **Декларации** (POU/VAR/TYPE/ENUM) — не через TreeConversion, а отдельным проходом типизации (`Compiler35220.Phase1..4`). Учесть при порте.

## 5. Пробелы
1. Конкретный класс-фабрика (реализация `ITreeFactory`) — уточнить имя (кандидаты: `GreenTrees.RedTreeFactory`, фабрики в `LanguageModelManager.plugin`); методы `CreateXxx` перечислены по вызывающей стороне.
2. Логика `RedTrees.Builder.*` (fluent `Init()/Condition()/Then()`) прочитана по `RedTreeBuilderFactory`; при порте сверить тела билдеров.
3. `NotImplementedException`-ветки visitor'а — декларации строятся фазой типизации (см. п.4).
