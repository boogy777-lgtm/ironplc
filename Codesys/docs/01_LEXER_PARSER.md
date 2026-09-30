# 01 — ST лексер/парсер CODESYS 3.5.22.10: карта компонента и план переноса на Rust

Цель проекта: воссоздать на Rust лексер и парсер ST, **идентичные** поведению
`Parser35220.plugin.dll` (CODESYS 3.5.22.10). Этот документ — техническая карта компонента:
что где лежит, как устроен сканер и парсер, какие контракты надо повторить, и чек-лист по методам.

Все пути записи — в зоне `C:\Codesys\...`; `C:\Program Files\CODESYS 3.5.22.10` — только чтение.

---

## 1. Карта компонента

```
CODESYS runtime
  ├─ Common\Compiler.dll                     ← общие контракты (интерфейсы/enum'ы)
  │    CODESYS.Parser.{IInternalParser, IParserService, IScannerService, IScannerOptionsService,
  │                     ITokenFactory, IErrorHandler, IMultiStringScanner, IOverflowChecker,
  │                     IPragmaScanner*, PragmaTokenType}
  │    _3S.CoDeSys.Core.LanguageModel.{TokenType, Operator(292), IECLanguage, IScanner9, IToken}
  │
  └─ PlugIns\58960fbc-…\3.5.22.10\Parser35220.plugin.dll   ← РЕАЛИЗАЦИЯ (наш объект)
       CODESYS.Parser35220.Scanner.InternalScanner   ← ЛЕКСЕР (посимвольный)
       CODESYS.Parser35220.Scanner.OperatorTable      ← таблица операторов/ключевых слов (TST)
       CODESYS.Parser35220.Scanner.{Token, OperatorDesc, OperatorFlags, OperatorNode,
                                    QuickStringBuilder, ReservedUnusedKeywords, MultiStringScanner,
                                    TokenFactoryClass}
       CODESYS.Parser35220.InternalParser             ← ПАРСЕР (recursive descent)
       CODESYS.Parser35220.{Declaration.*, Statements.*, Expressions.*, Pragmas.*}
       CODESYS.Parser35220.PragmaScanner.{PragmaScanner, PragmaToken}
       CODESYS.Parser35220.{ParserService, ScannerService}  ← регистрация плагина
```

Легаси-аналог с тем же исходником под языковую версию 3.5.21 — `Parser35210.plugin.dll`
(namespace `CODESYS.Parser35210.*`). Для переноса берём 3.5.22.

`WhiteParseTrees` (Common + plugin `977d3b32`) — **другой** компонент (concrete syntax tree с
сохранением пробелов/комментариев для форматтера), к компиляторному диагностическому разбору
не относится. В зону этой задачи не входит.

### Артефакты в репозитории

| Артефакт | Путь | Размер/состав |
|---|---|---|
| Декомпил Parser35220 | `C:\Codesys\Parser35220.plugin\` + `Parser35220.plugin.sln` | 94 `.cs` (93 top-level типов + `AssemblyInfo.cs`), 703 435 Б |
| Декомпил Parser35210 | `C:\Codesys\decompiled\Parser35210.plugin\` | 98 `.cs`, 504 450 Б (89 top-level + ресурсы/атрибуты) |
| Декомпил Compiler (контракты) | `C:\Codesys\decompiled\Compiler\` | 1405 `.cs`, 802 077 Б (1406 top-level) |
| Оригинальные сборки | `C:\Codesys\binaries\` | 5 DLL, 1 833 728 Б |
| Машиночитаемые таблицы | `C:\Codesys\tables\` | см. §6 |
| Инструменты dnlib | `C:\Codesys\tools\` | `dnlib.dll`, `re_*.ps1`, `dump_method_il.ps1`, `extract_tables.ps1` |

---

## 2. Модель Token и позиции

`struct Token` (`…\Scanner\Token.cs`):

| Поле | Тип | Смысл |
|---|---|---|
| `Type` | `TokenType` | категория токена |
| `SourceOffset` | `int` | абсолютный индекс начала токена в `char[]` |
| `Length` | `int` | длина в символах (`_nPosition`-независимая) |
| `SourceLine` | `int` | номер строки (0-based) |
| `SourceColumn` | `int` | колонка = `SourceOffset - _nLineStartSourceOffset` |
| `Position` | `long` | логическая позиция (может переопределяться прагмой `{position:=N}`) |
| `PositionOffset` | `short` | `SourceOffset - _nTokenStartSourceOffset` |
| `CharactersToSkipSeen` | `long` | счётчик «пропущенных» символов (для внешних инструментов) |
| `Empty` | `static readonly Token` | шаблон-заготовка |

`enum TokenType` (`Common\Compiler.dll`) — 28 значений:

```
None=0 Boolean=1 Comment=2 DocComment=3 Pragma=4 Date=5 DateAndTime=6
DirectVariable=7 IncompleteDirectVariable=8 DoubleByteString=9 Duration=10 LDuration=11
EndOfLine=12 Identifier=13 Integer=14 Operator=15 Real=16 SingleByteString=17 TimeOfDay=18
Whitespace=19 Error=20 End=21 XByteString=22 LDate=23 LTimeOfDay=24 LDateAndTime=25
Unused=26 PartialAccess=27
```

Состояние сканера (`InternalScanner.cs`, приватные поля):
```
char[] _input; long _nPosition; int _nLineStartSourceOffset; int _nTokenStartSourceOffset;
int _nSourceLine; QuickStringBuilder _buffer; bool _bUnicodeIdentifiers; long _charactersToSkipSeen;
/* + свойства: SourceOffset, CurrentToken, IncludeWhitespaces/EndOfLines/Comments/Pragmas/
   PositionPragmas, IgnoreCase, AllowNestedComments, AllowMultipleUnderlines, PositionsStartAtOne */
```

`QuickStringBuilder` — zero-alloc буфер токена: хранит ссылку на общий `char[] _input`, начало
`_offset` и длину; `Sync(pos)`/`SetAlternativeString()`; сравнение без создания `string`
(`IsEqual`, `Equals`, `EndsWith`). В Rust — слайс `&[char]`/`&str` вместо буфера.

---

## 3. Лексер: посимвольный алгоритм

Файл: `CODESYS\Parser35220\Scanner\InternalScanner.cs` (4185 строк).

### 3.1 Вход и инициализация
- `Initialize(string)`: копирует текст в `char[]` длиной `len+1` и дописывает `'\0'` (сентиел).
- `Initialize(char[])`: массив **обязан** заканчиваться `'\0'` (иначе `ArgumentOutOfRangeException`).
- `InitializeInternal`: `SourceOffset=0`; `_nPosition = PositionsStartAtOne ? 1 : 0`;
  `_nLineStartSourceOffset=0`; `_nTokenStartSourceOffset=0`; `AutoIncrementPositionOnLineBreaks=false`;
  `SetScanningOptions()` (utf/non-compliant идентификаторы из `IScannerOptionsService`).
- `CreateScanner(string)` — фабрика нового сканера на подстроку.

### 3.2 Точка входа: `GetNextIterated`
```
public virtual TokenType GetNext(out IToken token):
    do { type = GetNextInternal(out token, out bPosPragma); }
    while ( type==Whitespace && !IncludeWhitespaces
         || type==EndOfLine  && !IncludeEndOfLines
         || type==Pragma && bPosPragma && !IncludePositionPragmas );
```
Т.е. фильтрация whitespace/EOL/position-прагм — на уровне `GetNext`, а `GetNextInternal` всегда
возвращает «сырой» токен.

### 3.3 `GetNextInternal` — диспетчер по первому символу
Псевдокод (по декомпилу 3488–3745):
```
c = _input[SourceOffset]
switch (c):
  '\0'            -> End(21)
  '\n','\r'       -> EndOfLine(); EndOfLine(12)
  ' ','\t','\v','\f','\u00A0' -> ScanWhitespace(); Whitespace(19)
  '"', '\''       -> ValidateStringToken(); DoubleByteString(9) | SingleByteString(17) | Error(20)
  '%'             -> ++off; ScanPercentLeading(...)     // direct-var / partial-access
  '('             -> ++off; if next=='*' ScanComment() Comment(2) else Operator(15)
  '*'             -> ++off; if next=='*' ++off; Operator(15)
  '.'             -> ++off; if next=='.' ++off; Operator(15)   // '..'
  '/'             -> ++off; if next=='/' ScanSingleLineComment() else Operator(15)
  ':','>'         -> ++off; if next=='=' ++off; Operator(15)
  '<'             -> ++off; if next=='=' or '>' ++off; Operator(15)
  '='             -> ++off; if next=='>' or ':' ++off; Operator(15)
  'S','s'         -> if next=='=' { off+=2; Operator }        // S= (Set)
  'R','r'         -> if next=='=' { off+=2; Operator }        // R=
                     else if next in {E,e} && next2 in {F,f} && next3=='=' { off+=4; Operator } // REF=
                     else ScanIdentifierOrOperator()
  '#' '&' ')' '+' ',' '-' ';' '[' ']' '^' '|' -> ++off; Operator(15)
  '{'             -> ++off; ScanPragma(); Pragma(4)
  default         -> ScanIdentifierOrOperator()   // буквы, '_', '`', цифры, unicode
token.Length = SourceOffset - token.SourceOffset
if type in {Comment(2), DocComment(3)} -> HandleCommentToken()
if type == Pragma(4)                   -> HandlePragmaToken()
```

### 3.4 Классы символов
```csharp
IsIdentifierStartCharacter(c) = 'A'..'Z' | 'a'..'z' | '_' | (unicodeIdentifiers && char.IsLetter(c))
IsIdentifierCharacter(c)      = 'A'..'Z' | 'a'..'z' | '0'..'9' | '_' | (unicodeIdentifiers && char.IsLetterOrDigit(c))
```
Whitespace: `' ' '\t' '\v' '\f' '\u00A0'` (в `ScanWhitespace`); EOL — `'\n' '\r'` (в `EndOfLine`).
Цифры: `'0'..'9'` + `'_'` (разделитель — `ScanDigits`).

### 3.5 Идентификаторы
- `ScanIdentifierOrOperator`:
  - первый символ не `IsIdentifierStartCharacter` и не `` '`' `` → если цифра — `ScanInteger`, иначе
    выход (обычно вызывает длинный «weird» путь).
  - `ScanIdentifierCharacters` → IEC-режим (`ScanIECIdentifier`) или «weird» (`ScanWeirdIdentifier`).
  - «weird»: разрешает символ `` '`' `` как escape-переключатель; внутри escape запрещены CR/LF/NUL
    → `bUnderlineError`.
  - `_` подряд без `AllowMultipleUnderlines` → `bUnderlineError`.
  - После идентификатора: если следующий `'#'` → это typed-literal префикс
    (`ScanTypedTimeLiteral` + `ScanTypedLiteral`), например `T#1s`, `TOD#…`, `DT#…`, `D#…`.
  - Иначе `ScanIdentifierOperatorOrTrueFalse`: lookup в `OperatorTable` (case-sensitive; при
    `IgnoreCase` — регистронезависимо) → `Operator(15)`; если это *contextual* оператор и он не
    разрешён (`ContextualOperatorToRecognize`) → откат; иначе TRUE/FALSE → `Boolean(1)`, иначе
    `Identifier(13)`.

### 3.6 Числа
- `ScanInteger`: цифры (`ScanDigits`), затем:
  - `'#'` → `ScanBasedInteger` (база 2/8/10/16 из уже набранного префикса) → `Integer(14)`;
  - `'.'` и это не `..` → дробная часть + `OptionalExponent` → `Real(16)`;
  - `E`/`e` → `OptionalExponent` → `Real(16)`;
  - иначе `Integer(14)`.
- `Integer(base)`: посимвольно цифры базы (hex `0-9A-Fa-f`, десятичные, и т.д.).
- `OptionalExponent`: `E`/`e` `[+|-]` цифры.
- `ScanDigits` пропускает `_`.

### 3.7 Typed-литералы (`ScanTypedLiteral` и семейство)
После `#` (или `тип#`) распознаются:
`ScanUnicodeLiteral` (`"..."`), `ScanTypedIntegerLiteral(char type)`, `ScanTypedRealLiteral`,
`ScanTypedDateLiteral`, `ScanTypedDateAndTimeLiteral`, `ScanTypedTimeOfDayLiteral`,
`ScanTypedTimeLiteral`/`ScanTypedLTimeLiteral` (TIME/LTIME форматы `d/h/m/s/ms/us/ns`).
Строки валидируются `ValidateStringToken(out bool bDoubleByte)` (одинарные/двойные кавычки,
escape-последовательности, локальная/unicode кодовая точка).

### 3.8 Комментарии
- Блочный `(* … *)` — `ScanComment`: счётчик вложенности `num`; при `AllowNestedComments` `(*`
  внутри увеличивает `num`; `*)` уменьшает; `\0` без закрытия → тип остаётся `Error(20)`, иначе
  `Comment(2)`.
- Строчный `//` — `ScanSingleLineComment`: если сразу после `//` идёт ещё `/` → `DocComment(3)`,
  иначе `Comment(2)`; до CR/LF/NUL.

### 3.9 Прагмы
- `{ … }` — `ScanPragma`: до `}`; при `\0`/EOL без `}` → не `Pragma`. Затем `HandlePragmaToken`
  различает position-прагмы (`{position:=N}`, `{autoincrementpositiononlinebreaks}`,
  см. `ReadScannerPragma`/`ReadScannerPositionPragma`/`ReadScannerAutoIncPositionPragma`) и
  возвращает их отдельным флагом `bPositionPragma`.
- Полноценные ST-прагмы (`{IF}`, `{DEFINE}`, `{attribute …}`) разбирает уже парсер
  (`CODESYS.Parser35220.Pragmas.*`) через `PragmaScanner`.

### 3.10 Direct-variable и partial access
- `ScanPercentLeading`: после `%`:
  - `I`/`i`/`Q`/`q`/`M`/`m` → `ScanDirectVariable` (`%IX6.0`, `%QW3`, `%MB10`);
  - `B/b/D/d/L/l/W/w/X/x` → `ScanPartialAccess` (`%B2`, `%W5`, `%X0`).
- `ScanDirectVariable`: `ScanLocationPrefix` (I/M/Q) → `ScanSizePrefix` (`B/W/D/L/X/*`; `*` →
  `IncompleteDirectVariable(8)`) → последовательность `Integer(10)` через `'.'` → `DirectVariable(7)`.
- `ScanPartialAccess` → `PartialAccess(27)`.

### 3.11 Прочее
- `ReadTokenUntilTerminator(string)`: «сырое» чтение до подстроки-терминатора — возвращает
  `Operator(15)`-токен на терминаторе; используется парсером (напр., для сырых вставок).
- `EndOfLine()` — обновляет `_nSourceLine`, `_nLineStartSourceOffset`, `_nPosition`
  (при `AutoIncrementPositionOnLineBreaks`), `SourceOffset` (CRLF как один EOL).
- `HandleCommentToken`/`HandlePragmaToken` — финальная обработка (учёт `IncludeComments`/`IncludePragmas`,
  позиционные прагмы).
- `ToUpper` — быстрый ASCII-аплоу через общий буфер.

### 3.12 `OperatorTable` — таблица операторов и ключевых слов
Файл `…\Scanner\OperatorTable.cs`. Singleton `Instance` (ctor → `UpdateOperatorTable`).
Порядок наполнения (важен для приоритетов/перезаписи):
```
1 AddSpecialOperators     (__COPY, __RELOC, …)                42
2 AddDataTypeNames        (ANY*, BOOL..LREAL, SAFE*, CHAR..)  57
3 AddOOKeywords           (ABSTRACT, PUBLIC, …)                8
4 AddStandardOperators    (LOWER_BOUND, ADR, ABS, MIN, MAX, …) 40
5 AddKeywords             (ST/declaration keywords)           77
6 AddVectorOperatos       (__VCLOAD_*, __VCADD, …)            13
7 AddILOperators          (XOR, XORN, NOT, EQ, CAL, …)        26
8 AddOperatorSymbols      (+,-,*,/,:=,…,..,=>,<,<=,<>,&,|,^)   30
9 AddConversionOperators  (динамические TO_* )               3 + 2
```
Хранилище — **ternary search tree** (`OperatorNode { char Char; Less; Equal; Greater; OperatorDesc }`),
поиск `Operator this[string, bool ignoreCase]` — регистронезависимый вариант через `Compare`.
`OperatorDesc { Operator Operator; OperatorFlags Flags }`.
`OperatorFlags` (12 значений): `DataType=1, Operator=2, Keyword=4, NumericDataType=8,
SafetyDataType=16, Contextual=32, StructuredText=0x10000, InstructionList=0x20000,
FunctionBlockDiagram=0x40000, Declaration=0x80000, Internal=0x100000, AllLanguages=0xFFFF0000`.
`GetKeywords(IECLanguage)` / `GetOperators(IECLanguage)` фильтруют TST по `GetByFlags`.

`ReservedUnusedKeywords` (17): `CHAR WCHAR ANY_DERIVED ANY_ELEMENTARY ANY_MAGNITUDE ANY_SIGNED
ANY_DURATION ANY_CHARS ANY_CHAR CHAR_TO TO_CHAR WCHAR_TO …` (`HashSet`, OrdinalIgnoreCase).

### 3.13 `MultiStringScanner` / `PragmaScanner`
- `MultiStringScanner : InternalScanner` — сканер по списку готовых строк (`InitializeMulti`),
  `MultiStringToken : IToken`; нужен для подстановок/редакторских операций.
- `PragmaScanner` (namespace `…PragmaScanner`) — отдельная машина разбора текста прагм поверх
  `IScanner9`; `PragmaToken : IPragmaToken`, `PragmaTokenType`.

---

## 4. Парсер: `InternalParser`

Файл `CODESYS\Parser35220\InternalParser.cs` (404 строки) + под-парсеры
`Declaration.* / Statements.* / Expressions.* / Pragmas.*`.

Поля/свойства: `Scanner (IScanner9)`, `ErrorHandler (IErrorHandler)`, `MessageGuid`,
`InDeclaration`, `InLibrary`, `Context (ParserContext)`, `LanguageModelBuilder`, `TokenFactory`,
`DeclarationParser`, `StatementParser`, `ExpressionParser`.

Ключевые методы (реализуют `CODESYS.Parser.IInternalParser`):
```
public  TokenType Next(out IToken token, bool bWithPragma, bool bWithComment)
private void      Next(out IToken)
public  _IStatement ParseST(bool bLibrary)          // точка входа тела POU
private _IStatement ParseST()
public  _IStatement ParseSTStatement(out bool bError)
private _IStatement ParseSTStatement(out bool bError, bool bTopLevel)
private _IStatement NextStatement()
public  _IExpression[] ParseSTSnippet()
public  _IExpression ParseAssignExp(out bool bError)
private _IExpression ParseInitialisationExp(out bool bError)
public  _IExpression ParseSTOperand(out bool bError)
public  _IExpression ParseInitialisation()
public  _IType   ParseType()
public  _IStatement ParseInterfaceStatement()
public  _IStatement ParsePragma(out bool bError, IMinimalPosition, string)
public  Operator MatchOperator(params Operator[] ops)
public  void ExtendForLoop(_IForStatement)
public  IPOUSyntax[] ParsePOUs()
public  IPOUSyntax[] ParsePOUs(out IEnumerable<IMessage> parserErrors)
public  _ISequenceStatement ParseRawST()
public  void CreateLanguageModelOfRawST(IPOUSyntax[], Guid source, Guid parent, ILanguageModel)
private static void AddStatementAndHandleDeclarations(_IStatement, _ISequenceStatement)
```

Структура разбора (recursive descent):
- **Объявления** — `Declaration.Parser`: `DeclarationParser`, `POUDeclarationParser`,
  `POUSyntaxParser`, `SyntaxElementParser`, `TypeParser`, `TypeDeclarationParser`,
  `VariableDeclarationParser`, `VariableListParser`, `EnumListParser`, `DeclarationStatementDetector`,
  `ContextualOperatorHandler`.
- **Операторы** — `Statements.Parser`: `StatementParser` + `If/Case/For/While/Repeat/Return/Jump/
  TryCatch/ConditionalCall/ImplementationBlock`-парсеры; `CodeStatementChecker`,
  `DeclarationStatementChecker`.
- **Выражения** — `Expressions.Parser`: `ExpressionParser`, `OperandParser`, `InfixOperationParser`,
  `FunctionCallParser`, `ParenthesizedExpressionParser`, `PrefixedOperatorParser`,
  `UnaryNotParser`, `UnaryPlusMinusParser`, `MinMaxOperatorParser`, `ArrayInitializationParser`,
  `StructureInitializationParser`, `NewExpressionParser`, `ConversionExpressionParser`,
  `ImplicitCastOperatorParser`, `ScopeExpressionParser`, `ThisAndBaseExpressionParser`,
  `CurrentTaskExpressionParser`, `InitializationParser`.
- **Прагмы** — `Pragmas.Parser`: `PragmaStatementParser`, `PragmaIfStatementParser`,
  `ErrorPragmaParser`, `HasAttributePragmaParser`, `HasTypePragmaParser`, `HasValuePragmaParser`,
  `HasConstantValueOrTypePragmaParser`, `PragmaOperandParser` (+`Defined`/`Version`/`XRef`/
  `ItemReference`).
- Ошибки — через `ErrorHandler.AddErrorST/AddErrorSTWithToken/AddWarningST` (интерфейс в
  `Common\Compiler.dll`).

Регистрация плагина: `ParserService : IParserService` (`CreateParser` → `InternalParser`,
статическое `s_LanguageVersion`), `ScannerService : IScannerService, IScannerService2`
(`CreateInternalScanner` → `InternalScanner`, `UpdateOperatorTable`, `IsReservedUnusedKeyword`).

---

## 5. Что НЕ входит (соседние компоненты)
- `Common\WhiteParseTrees.dll` + `WhiteParsetrees.plugin.dll` — concrete syntax tree / форматтер.
- `Compiler35220.plugin.dll` (`_3S.CoDeSys.Compiler35220.PreCompile.Scanner`) — препроцессорный
  сканер (макросы/`{IF}`) и кодогенерация фаз 1–5.
- `MessageStorage` — резолв `MessageId` → текст (см. §8).

---

## 6. Таблицы (`C:\Codesys\tables\`) — полнота доказана

Все таблицы сгенерированы скриптом `tools\extract_tables.ps1` из IL через dnlib
(правило `ldstr` + `ldc.i4`×2 + `newobj OperatorDesc`). Полнота подтверждена сверкой с числом
`newobj OperatorDesc::.ctor` (и `set_Item`) в каждом методе — совпадение 1:1.

| Файл | Записей | Источник / метод | Проверка |
|---|---|---|---|
| `st_keywords.csv` | 77 | `OperatorTable.AddKeywords` | ctor=77, set_Item=77 |
| `standard_operators.csv` | 40 | `AddStandardOperators` | 40/40 |
| `operator_symbols.csv` | 30 | `AddOperatorSymbols` | 30/30 |
| `datatype_names.csv` | 57 | `AddDataTypeNames` | 57/57 |
| `special_operators.csv` | 42 | `AddSpecialOperators` | 42/42 |
| `ilo_operators.csv` | 26 | `AddILOperators` | 26/26 |
| `oo_keywords.csv` | 8 | `AddOOKeywords` | 8/8 |
| `vector_operators.csv` | 13 | `AddVectorOperatos` | 13/13 |
| `conversion_operators.csv` | 5 | `AddConversionOperators`(3) + `AddOverloadedConversions`(2) | 3/3 и 2/2; имена динамические (`TO_<T>`, `ANY_TO_<T>`, `<T>_TO_<U>`) |
| `operators.csv` | 292 | enum `Operator` (`Common\Compiler.dll`) | значения совпадают с ordinal в таблицах |
| `token_types.csv` | 28 | enum `TokenType` | — |
| `ieclanguage.csv` | 4 | enum `IECLanguage` | — |
| `operator_flags.csv` | 12 | enum `OperatorFlags` | + hex |
| `reserved_unused_keywords.txt` | 17 | `ReservedUnusedKeywords.cctor` | все `ldstr` |

Итого статических записей `OperatorDesc`: 77+40+30+57+42+26+13+8 = **293** (совпадает с числом
ctor-вызовов), + 5 динамических конверсий = **298** мест создания дескриптора.

Колонки `*_operators/*_keywords`: `text|keyword, operator_ordinal, flags(dec), flags_decoded,
language` (`language` = `ST|IL|FBD|Declaration|Internal` или `All`).

---

## 7. Перенос на Rust — чек-лист

### 7.1 Модель данных
- [ ] `enum TokenType` — 28 значений (см. §2), `#[repr(u8)]`.
- [ ] `enum Operator` — 292 значения, точные числовые значения (ordinal **обязателен**: он зашит
      в `OperatorDesc`).
- [ ] `bitflags OperatorFlags` — 12 бит (значения из `operator_flags.csv`).
- [ ] `enum IECLanguage { StructuredText=0, InstructionList=1, FunctionBlockDiagram=2, Declaration=3 }`.
- [ ] `struct Token` (поля §2) и `struct OperatorDesc { operator, flags }`.
- [ ] `QuickStringBuilder` → слайс/интервал по общему буферу (без аллокаций `String`).

### 7.2 Лексер (`InternalScanner`)
- [ ] `Initialize(char[])` + требование `'\0'`-сентиела; `Initialize(string)`.
- [ ] Поля позиции: `SourceOffset, nPosition, nLineStartSourceOffset, nTokenStartSourceOffset,
      nSourceLine, charactersToSkipSeen`; `PositionsStartAtOne`.
- [ ] Опции: `IncludeWhitespaces, IncludeEndOfLines, IncludeComments, IncludePragmas,
      IncludePositionPragmas, IgnoreCase, AllowNestedComments, AllowMultipleUnderlines,
      SupportUnicodeIdentifiers, SupportNonCompliantIdentifiers, AutoIncrementPositionOnLineBreaks`.
- [ ] `GetNext` (фильтр whitespace/EOL/position-pragma) и `GetNextInternal` (диспетчер §3.3).
- [ ] Классы символов (§3.4), `EndOfLine`, `ScanWhitespace`, `ConsumeWhitespace`.
- [ ] `ScanIdentifierOrOperator`, `ScanIdentifierCharacters`, `ScanIECIdentifier`,
      `ScanWeirdIdentifier`, `CheckForUnderlineError`, `ScanForTrueFalseOrIdentifier`.
- [ ] Числа: `ScanInteger`, `ScanBasedInteger`, `ScanDigits`, `Integer(base)`, `OptionalExponent`.
- [ ] Typed-литералы: `ScanUnicodeLiteral`, `ScanTypedLiteral` и все `ScanTyped*Literal`,
      `ScanTimeLiteral`/`ScanTypedLTimeLiteral`/`ScanTypedTimeLiteral`, `ValidateStringToken`.
- [ ] Комментарии: `ScanComment` (вложенность), `ScanSingleLineComment` (doc).
- [ ] Прагмы: `ScanPragma`, `HandlePragmaToken`, `ReadScannerPragma`,
      `ReadScannerPositionPragma`, `ReadScannerAutoIncPositionPragma`.
- [ ] Direct-variable/partial-access: `ScanPercentLeading`, `ScanDirectVariable`,
      `ScanPartialAccess`, `ScanLocationPrefix`, `ScanSizePrefix`.
- [ ] `ReadTokenUntilTerminator`, `Match`, `SetPosition`, `RecognizeContextualOperator`.
- [ ] Распаковки значений: `GetIdentifier, GetInteger(+Intern), GetReal, GetRealAsFloat,
      GetBoolean, GetDate*/GetTimeOfDay*/GetDuration*, GetDirectVariable, GetPartialAccess,
      GetTokenText(+flags), GetOperator, GetConversion, GetPragma, строки/Unescape`.
- [ ] `MultiStringScanner` (сканер по списку строк).

### 7.3 Таблицы (`OperatorTable`)
- [ ] Ternary search tree (`OperatorNode`) + `Compare` (с учётом `ignoreCase`).
- [ ] Порядок `UpdateOperatorTable` (§3.12) и все 8 статических `Add*` + динамические конверсии.
- [ ] `GetKeywords/GetOperators(IECLanguage)`, `GetByFlags`, `GetTextOfOperator(short/long)`,
      `GetOperatorFromText`, `IsContextualOperator`.
- [ ] `ReservedUnusedKeywords` (17).

### 7.4 Парсер (`InternalParser` + под-парсеры)
- [ ] Входные точки: `ParseST`, `ParseSTSnippet`, `ParseSTStatement`, `ParseRawST`, `ParsePOUs`,
      `CreateLanguageModelOfRawST`.
- [ ] Объявления (§4), операторы, выражения, прагмы — по списку типов.
- [ ] Контракты `_IStatement/_IExpression/_IType/_ISequenceStatement/IPOUSyntax` — либо полный
      порт языковой модели, либо собственная AST с сохранением позиций.
- [ ] `IErrorHandler`: `AddErrorST`, `AddErrorSTWithToken`, `AddWarningST` (+ `MessageId`).
- [ ] `ParserContext` (режимы `SetInDeclaration`, `SetInLibrary`, `SetInSTCode`).
- [ ] `PragmaScanner`/`PragmaToken`.

### 7.5 Совместимость и тесты
- [ ] Числовые значения `Operator`/`TokenType` и биты `OperatorFlags` — байт-в-байт как в CSV.
- [ ] Граничные случаи: `'\0'`-сентиел, CRLF, `..` vs дробь, `S=`/`R=`/`REF=`, `%I/%Q/%M*/%B`,
      вложенные комментарии, position-прагмы, `_`-подряд, `` ` ``-escape, `#`-базисные числа,
      typed-литералы, unicode-идентификаторы.
- [ ] Дифференциальное тестирование: один и тот же текст через CODESYS-сканер и Rust-сканер,
      сравнение последовательности (TokenType, SourceOffset, Length, Position, line/column).

---

## 8. Открытые вопросы / пробелы

1. **Тексты ошибок**. `MessageId` резолвится через `Common\MessageStorage.dll` (в дамп не входил).
   Для 100% совпадения диагностик нужно сопоставить `MessageId` ↔ строка (ресурсы 9 языков).
2. **`ScannerOptionsService.GetScanningOptions`** — источник флагов utf/non-compliant; нужно найти
   его реализацию вне Parser35220 (влияет на классы символов).
3. **`ParserContext`/`ITypeTable`/`ILanguageModelBuilder7`** — парсер тесно связан с моделью языка
   (`_3S.CoDeSys.LanguageModelManager.InternalInterfaces`); объём контрактов в `decompiled\Compiler\`
   большой (1406 top-level типов). Нужно решить: портировать модель целостно или спроектировать
   свою AST.
4. **Динамические конверсии** (`AddConversionOperators`/`AddOverloadedConversions`) строятся в
   рантайме из списка типов (`TO_<T>`, `ANY_NUM_TO_<T>`, `ANY_TO_<T>`, `<T>_TO_<U>`) — для 1:1 надо
   повторить логику отбора (`flags & DataType`, `flags & SafetyDataType`, `flags & DataType &
   NumericDataType`) и `GetTextOfOperator(short=true)`.
5. **`GetNextInternal` декомпилирован в goto-граф** (Roslyn switch); при порте удобнее восстановить
   исходный `switch` по первому символу — сверять с IL (`tools\dump_method_il.ps1`).
6. **Порядок/перезапись в `OperatorTable`**: словарь допускает перезапись ключей — при совпадении
   имён (напр. `NOT` в стандартных и IL-операторах) побеждает более поздний `Add*`; проверить
   отсутствие конфликтов по CSV перед портом.
