# 04 — AST-модель (red-tree) CODESYS ST

> Область: только чтение метаданных и декомпиляция разрешённых сборок.
> Временная папка `decompiled\_scan` удалена (её содержимое перенесено в этот документ).
> `decompiled\LanguageModelManager*` НЕ декомпилировалась — по ней только метаданные dnlib (см. §1, §7).

## 1. Локализация конкретной реализации AST / red-tree

Сканирование dnlib (тип → интерфейс-реализации). «AST-узлы» = интерфейсы
`_3S.CoDeSys.Core.LanguageModel.{IExprement,IExpression,IStatement,IType}` и их наследники.

| Сборка | Что определяет | Типов | AST-роль |
|---|---|---|---|
| `Compiler.dll` | **все интерфейсы узлов** `_3S.CoDeSys.Core.LanguageModel.*`, `IRedTreeBuilderFactory`, fluent-builder'ы `_3S.CoDeSys.Compiler.LanguageModelBuilder.*`, `ILanguageModelBuilder…12` | 1407 | **Спецификация модели** (интерфейсы) |
| `LanguageModelUtilities.plugin.dll` | `StructuredLanguageModel.*` (POU-билдеры: `Function`, `FunctionBlock`, `GVL`, `Method`, `Interface`, `Public`), хелперы/визиторы | 222 | **Конкретные POU-узлы** + утилиты |
| `LanguageModelUtilities.dll` | только интерфейсы `ILanguageModelUtilities*`, `ICrossReferenceService*`, `I*Builder` | 121 | Интерфейсы |
| `ComponentModel.dll` | COM/компонентная инфраструктура, обфусцирована SmartAssembly | 256 | Не относится к AST |
| `Core.dll` | один тип `<Module>` (нет декомпилируемых типов) | 1 | Пусто |
| **`LanguageModelManager.plugin.dll`** ⛔ | **конкретные классы red-tree**: `RedTrees.Builder.RedTreeBuilderFactory` (impl `IRedTreeBuilderFactory`), `RedTrees.Builder.Statements.*`, `RedTrees.Builder.Expressions.*`, `LanguageModelBuilder`, `GreenTrees.RedTreeFactory` | 742 | **Реальная реализация AST** |
| `Compiler35220.plugin.dll` (вне зоны) | `TreeConversion.RedTreeBuilder` — визитор white→red | 1039 | Конвертер дерева |
| `WhiteParsetrees.plugin.dll` (вне зоны) | `Nodes.Factories.BuilderFactory` (`IWhiteParseTreeBuilderFactory3`), узлы CST | 643 | White-tree (CST) |
| `WhiteParseTrees.dll` (вне зоны) | интерфейсы white-tree | 583 | Интерфейсы CST |

**Вывод по локализации:** конкретные узлы AST и фабрика red-tree лежат в **`LanguageModelManager.plugin.dll`**
(201 класс-реализация `_IStatement/_IExpression/ILanguageModel/IRedTreeBuilderFactory`), которая в моей зоне запрещена.
В моих четырёх сборках конкретика есть только на уровне POU-модели (`StructuredLanguageModel`) и утилит.

## 2. Архитектура «3 дерева»

```
Исходник ST
   │  Parser35220.plugin.dll  (вне зоны)
   ▼
White parse tree (CST)        CODESYS.WhiteParseTrees.*   (WhiteParseTrees.dll / .plugin.dll)
   │  Compiler35220.TreeConversion.RedTreeBuilder  (вне зоны)
   ▼
Red tree (AST, семантика)     _3S.CoDeSys.Core.LanguageModel.*  (Compiler.dll)
   │  фабрика: RedTreeBuilderFactory  (LanguageModelManager.plugin.dll ⛔)
   ▼
ILanguageModel (проектная модель: Pous / GlobalVariableLists / DataTypes)
```

- **White tree** — синтаксис + токены/позиции.
- **Red tree** — типизированные узлы `IStatement`/`IExpression` с `ISourcePosition`.
- **Green tree** — промежуточный слой (`GreenTrees.RedTreeFactory`, `ITreeFactory…7`).
- **StructuredLanguageModel** (в моей зоне) — высокоуровневые POU-билдеры, собирающие red-tree через `ILanguageModelBuilder`.

## 3. Базовые интерфейсы узлов

`decompiled/Compiler/_3S/CoDeSys/Core/LanguageModel/IExprement.cs:7`
```csharp
public interface IExprement {                 // корень всех узлов
    ISourcePosition Position { get; }         // :9  — позиция в исходнике
    IBREakpoint Breakpoint { get; }           // :12 obsoleted
    ICodeGeneratorAttributes CGAttributes { get; set; }
    void AddError(string stError);            // :20
    void AddWarning(string stWarning);
    void AcceptVisitor(IExprVisitor visitor); // :24  — паттерн Visitor
    string ToString();
}
```
`IExpression.cs:7` — `IExpression : IExprement` + `Type` (`ICompiledType`), `ScratchOffset`, `IsLiteral`, `Literal(scope)`, `DataLocation(scope)`.
`IStatement.cs:6` — `IStatement : IExprement` + `bool GetFlag(StatementFlag)`.
`IType` (в `Compiler.dll`) — базовый для `IArrayType/IEnumType/IReferenceType/ISubrangeType/IVectorType/IStringType/IPointerType/IUserdefType/ISafetyType`.
`ILanguageModel.cs:7` — корневой контейнер: `Pous`, `GlobalVariableLists`, `DataTypes`, `LMDevice/LMApplication/LMTaskList/LMLibraryList`.

Иерархия: `IExprement` ← `IExpression`/`IStatement`; у каждого конкретного узла есть «версионные» расширения
(`IExpression2…6`, `ICallExpression2…4`, `ISequenceStatement2/3` и т.п.) — это OCP-совместимое добавление полей без лома API.

### Версионирование (`_IEmbeddedLanguageStatement`)
`_IEmbeddedLanguageStatement : IStatement, IExprement` — узел-обёртка вложенного языка
(`decompiled/Compiler/_3S/CoDeSys/Core/LanguageModel/_IEmbeddedLanguageStatement.cs`), база — `LanguageModelManager.InternalInterfaces._IStatement` (⛔).

## 4. Устройство узлов (поля и позиции)

Поля — read-only свойства интерфейса (полный список — `tables/ast_nodes.csv`, колонка `key_fields`).

Примеры (`decompiled/Compiler/_3S/CoDeSys/Core/LanguageModel/`):
- `IIfStatement.cs:6` → `Condition:IExpression; IfThen:IStatement; IfElse:IStatement; ElseIf:IElseIf[]`
- `IWhileStatement.cs:6` → `Condition; Controlled`
- `IForStatement.cs:6` → `CounterStart; UpperBound; By; Condition; Controlled; Counter`
- `ICaseStatement.cs:6` → `Switch:IExpression; Cases:ICase[]; Else`
- `IOperatorExpression.cs:6` → `Code:Operator; Operands:IExpression[]`
- `ICallExpression.cs:6` → `Callee; Condition; InputAssigns:IAssignmentExpression[]; OutputAssigns:IAssignmentExpression[]`

**Позиция** — `ISourcePosition.cs:7`:
`ProjectHandle:int; ObjectGuid:Guid; Position:long; PositionOffset:short; PositionCombination:long; Length:short`.
Создаётся через `ILanguageModelBuilder.CreateExprementPosition(long)` / `(long, short)`
(`ILanguageModelBuilder.cs:44`).

**Паттерн Visitor** — `IExprVisitor.cs:6`: 45 методов `visit(IXxx)` — фактически перечень листовых/узловых kind'ов.

**Полный реестр конструкторов узлов** — `ILanguageModelBuilder.cs:10-242`
(`CreateWhileStatement`, `CreateIfStatement`, `CreateForStatement`, `CreateCaseStatement`,
`CreateOperatorExpression`, `CreateCallExpression`, `CreateLiteralExpression`, `CreateVariableExpression`, …).
Это лучший источник «поля узла ↔ параметры».

## 5. Фабрика red-tree `IRedTreeBuilderFactory` и её реализация

**Интерфейс (моя зона, `Compiler.dll`):** `decompiled/Compiler/_3S/CoDeSys/Compiler/LanguageModelBuilder/IRedTreeBuilderFactory.cs:8`
```csharp
public interface IRedTreeBuilderFactory {
    ICalleeBuilderOptionalPosStep CallBuilder { get; }
    IOperatorOptionalPosStep     OperatorBuilder { get; }
    IIfOptionalPosStep           IfBuilder { get; }
    IForOptionalPosStep          ForBuilder { get; }
    ICaseOptionalPosStep         CaseBuilder { get; }
    IWhileOptionalPosStep        WhileBuilder { get; }
    ITypeOptionalPosStep         TypeDeclarationBuilder { get; }
    IElseIfOptionalPosStep       ElseIfBuilder { get; }
    IPouDeclarationBuilderOptionalPosStep PouDeclarationBuilder { get; }
    IEnumOptionalPosStep         EnumDeclarationListBuilder { get; }
    IVariableDeclarationListOptionalPosStep VariableDeclarationListBuilder { get; }
}
```
Fluent-контракт шагов (`decompiled/Compiler/_3S/CoDeSys/Compiler/LanguageModelBuilder/`):
- `ILmbPositional.cs:7` — `TNext At(IExprementPosition position);`
- `ILmbExprementBuilder.cs:6` — `T Build();`
- пример шага: `Statements/IIfOptionalPosStep.cs:6`, `Statements/IIfBuilder.cs:6`, `Expressions/IOperatorOptionalPosStep.cs:6`.

**Реализация (⛔ чужая зона, только метаданные dnlib):**
`_3S.CoDeSys.LanguageModelManager.RedTrees.Builder.RedTreeBuilderFactory`
(реализует `_3S.CoDeSys.Compiler.LanguageModelBuilder.IRedTreeBuilderFactory`), сборка `LanguageModelManager.plugin.dll`.

## 6. Соответствие «builder-интерфейс → конкретный узел AST»

Входной builder — из `IRedTreeBuilderFactory`; узел — из `IExpression`/`IStatement`/`IElseIf`:

| Builder (вход) | Целевой узел AST | Источник |
|---|---|---|
| `Expressions.ICalleeBuilderOptionalPosStep` | `ICallExpression` | `ICalleeOptionalConditionBuilder.cs:7` |
| `Expressions.IOperatorOptionalPosStep` | `IOperatorExpression` | `IOperatorRhsBuilder.cs:9` |
| `Statements.IIfOptionalPosStep` | `IIfStatement` | `IIfElseBuilder.cs:9`, `IOptionalIfElseOrElseIfsBuilder.cs:7` |
| `Statements.IForOptionalPosStep` | `IForStatement` | `IForControlledBuilder.cs:9` |
| `Statements.ICaseOptionalPosStep` | `ICaseStatement` | `ICaseBuilderCase.cs:7` |
| `Statements.IWhileOptionalPosStep` | `IWhileStatement` | `IWhileControlledBuilder.cs:9` |
| `Statements.ITypeOptionalPosStep` | `ITypeDeclarationStatement` | `ITypeBuilderFlag.cs:9` |
| `Statements.IElseIfOptionalPosStep` | `IElseIf` | `IElseIfControlledBuilder.cs:9` |
| `Statements.IPouDeclarationBuilderOptionalPosStep` | `IPOUDeclarationStatement` | `IPouDeclarationBuilderDeclaration.cs:7` |
| `Statements.IEnumOptionalPosStep` | `IEnumDeclarationListStatement` | `IEnumBuilderBaseType.cs:9` |
| `Statements.IVariableDeclarationListOptionalPosStep` | `IVariableDeclarationListStatement` | `IVariableDeclarationBuilderFlags.cs:9` |

Эта же карта занесена в `tables/ast_nodes.csv` (колонка `builder_interface`).

## 7. Конкретные POU-узлы в разрешённой зоне

`_3S.CoDeSys.LanguageModelUtilities.StructuredLanguageModel.*`
(`decompiled/LanguageModelUtilities.plugin/_3S/CoDeSys/LanguageModelUtilities/StructuredLanguageModel/`):

```
IHasVarDeclaration  (HasVarDeclaration.cs:8)
  ├─ GVL                  (IGVLBuilder2)          → ILMGlobVarlist
  └─ AbstractPOU          (IPouBuilder)           (AbstractPOU.cs)
       ├─ AbstractPOUWithAttributes
       │    ├─ AbstractPOUWithReturnValue
       │    │    ├─ Function   (IFunctionBuilder2)  Function.cs:8
       │    │    └─ Method     (IMethodBuilder2)
       │    └─ AbstractPOUWithMethods
       │         ├─ FunctionBlock (IFunctionBlockBuilder2)
       │         │    └─ Interface
       │         └─ Public         (IProgramBuilder)
```
Сборка узла идёт через `ILanguageModelBuilder` (композиция, не наследование) —
см. `HasVarDeclaration.cs:83-95` (`AddToLanguageModel` → `CreateVariableDeclarationStatement`, `CreateSequenceStatement`).
Утилита-обёртка: `LanguageModelBuilderHelper.cs:9` (`ILanguageModelBuilder6`, методы `Var/Op/Compo/Call/Assign`).

## 8. Чек-лист переноса на Rust

**8.1 Span / позиция (зеркало `ISourcePosition`)**
```rust
struct Span { project_handle: i32, object_guid: Uuid, position: i64,
              position_offset: i16, position_combination: i64, length: i16 }
```
**8.2 Общий трейт узла**
```rust
trait AstNode { fn span(&self) -> Span; fn accept(&self, v: &mut dyn ExprVisitor); }
```
**8.3 enum-ы узлов (по `kind` из CSV: 65 expr + 36 stmt + 22 type + 2 token = 125)**
```rust
enum Node { Stmt(Stmt), Expr(Expr), Type(TypeNode), Token(Token) }

enum Stmt {                       // IStatement-семейство
  Empty, Sequence(Vec<Stmt>),
  If { cond: Box<Expr>, then_: Box<Stmt>, elseifs: Vec<ElseIf>, else_: Option<Box<Stmt>> },
  While { cond: Box<Expr>, body: Box<Stmt> },
  Repeat { cond: Box<Expr>, body: Box<Stmt> },
  For { counter: Box<Expr>, start: Box<Expr>, upper: Box<Expr>, by: Option<Box<Expr>>,
        cond: Option<Box<Expr>>, body: Box<Stmt> },
  Case { switch: Box<Expr>, cases: Vec<Case>, else_: Option<Box<Stmt>> },
  Return(Option<Box<Expr>>), Exit, Continue, Jump { label: String, cond: Option<Box<Expr>> },
  Label(String), Comment(String), ExprStmt(Box<Expr>),
  PouDecl { .. }, TypeDecl { .. }, VarDeclList { .. }, EnumDeclList { .. },
  Pragma(..), BreakPoint{..}, Define{..}, Subroutine{..}, WarningDisableRestorePragma,
}

enum ElseIf { cond: Box<Expr>, body: Box<Stmt> }     // IElseIf / IElseIf2
struct Case  { labels: CaseLabel, body: Box<Stmt> }  // ICase

enum Expr {                       // IExpression-семейство
  Literal(Literal), Variable(String), This, Base,
  Operator { code: Operator, operands: Vec<Expr> },
  Assign { lv: Box<Expr>, rv: Box<Expr>, kind: Operator },
  CompoAccess { left: Box<Expr>, right: Box<Expr> },
  IndexAccess { base: Box<Expr>, index: Vec<Expr> },
  DeRefAccess(Box<Expr>), Call { callee: Box<Expr>, cond: Option<Box<Expr>>,
    inputs: Vec<Assign>, outputs: Vec<Assign> },
  Conversion{..}, Cast{..}, Address{..}, New{..}, TypeExpr{..}, TypeRef{..}, PouRef{..},
  VarRef{..}, Defined{..}, CompilerVersion{..}, HasType{..}, IsEnumType{..},
  HasAttribute{..}, HasValue{..}, PragmaOperator{..}, GlobalScope{..}, SystemScope{..},
  ArrayInit(Vec<Expr>), StructInit(Vec<Assign>), IndexInit{..}, Null, Error,
  QualifiedName{..}, NamespaceAccess{..}, PoolScope{..}, PartialAccess{..}, CallInstance{..},
  RuntimeVersion, CurrentTask, BitAccess{..}, CompoBit{..}, RefAccess{..},
}
```
**8.4 Обязательные элементы для 100% совместимости**
- [ ] `Operator` enum — синхронизировать с `tables/operators.csv`.
- [ ] `StatementFlag` (`GetFlag`) — `StatementFlag.cs`.
- [ ] Copy-on-write «красного» дерева: узел — Rc/Arc + `Span`; правки создают новую ветвь (red-tree semantics).
- [ ] Версионные расширения (`IExpression2…6`, `ICallExpression2…4`, `ISequenceStatement2/3`, `ILabelStatement2`) — не enum-варианты, а дополнительные `Option`-поля на узле.
- [ ] `IExprVisitor` (45 методов) → один `visit_*` на вариант enum (или `NodeKind` + dyn).
- [ ] `ILanguageModelBuilder` (`ILanguageModelBuilder.cs:10-242`) — фабрика всех узлов; реализовать 1-в-1 как набор `create_*`.
- [ ] POU-модель `StructuredLanguageModel` — отдельный слой поверх фабрики.

## 9. Артефакты и полнота

- `tables/ast_nodes.csv` — 691 строка (все интерфейсы `_3S.CoDeSys.Core.LanguageModel`):
  `expression=65, statement=36, type=22, token=2, other=566` → **125 узлов AST** + 566 сервисных интерфейсов.
  Полнота подтверждена: это **все** интерфейсы namespace `_3S.CoDeSys.Core.LanguageModel` (dnlib, `Compiler.dll`), отфильтрованные по наследованию от `IExprement/IExpression/IStatement/IType`.
- Декомпил: `decompiled/LanguageModelUtilities.plugin` (221 файл), `decompiled/LanguageModelUtilities` (120 файлов),
  `decompiled/ComponentModel` (7 файлов; обфусцированные типы пропущены), `decompiled/Core` — **пусто** (`Core.dll` содержит только `<Module>`).

## 10. Пробелы (что не удалось закрыть в зоне)

1. **Конкретные классы узлов red-tree** и **`RedTreeBuilderFactory`** физически в `LanguageModelManager.plugin.dll` (⛔). В моей зоне только интерфейсы — фактическая логика полей/`Build()` не декомпилирована.
2. **`producer`** (метод парсера) в CSV пуст: парсер — `CODESYS.Parser35220.*` (`Parser35220.plugin.dll`, вне зоны). Связь «правило грамматики → узел» требует сканирования парсера.
3. **`ComponentModel.dll`** обфусцирована SmartAssembly (имена namespace/типов нечитаемы); декомпилированы только 7 нелакированных типов `_3S.CoDeSys.Core.Licensing/*`.
4. **`Core.dll`** не содержит полезных типов (только `<Module>`).
5. Точное сопоставление builder-шагов (`ILmbPositional.At`) с методами парсера не проверено — builder вызывается из парсера вне зоны.
