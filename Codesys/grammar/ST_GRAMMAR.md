# ST_GRAMMAR — грамматика Structured Text CODESYS 3.5.22.10

Восстановлено из декомпилированного `Parser35220.plugin.dll`
(`C:\Codesys\Parser35220.plugin\CODESYS\Parser35220\...`, исходники другого агента).
Сопутствующий файл: `ST_GRAMMAR.ebnf` (**132 продукции**, 8 уровней приоритета).

Ключевые интерфейсы/типы (из `C:\Users\HALLIBURTON\Desktop\1.1.0.0\_dump\compiler_types.txt`):

| Сущность | Тип | Где определено |
|---|---|---|
| Токен-тип | `enum _3S.CoDeSys.Core.LanguageModel.TokenType` (29 значений) | `compiler_types.txt:6837` |
| Оператор/ключевое слово | `enum _3S.CoDeSys.Core.LanguageModel.Operator` (293 значения) | `compiler_types.txt:3151` |
| Тип direct variable | `enum DirectVariableLocation` (None/Input/Output/Memory) / `enum DirectVariableSize` (None/X/B/W/D/L) | `compiler_types.txt:4651` / `:4656` |
| Прагма-токен | `enum CODESYS.Parser.PragmaTokenType` (None/Identifier/Integer/Operator/SingleByteString/Error/End) | `compiler_types.txt:87` |

---

## 1. Лексика (лексер)

Точка входа — `InternalScanner.GetNextInternal` (`InternalScanner.cs:3488`). Основные правила:

| Входные символы | Токен | Код |
|---|---|---|
| `\0` | `End` (21) | `:3506` |
| `\n` `\r` | `EndOfLine` (12) | `:3517` |
| space, `\t`, `\v`, `\f`, NBSP | `Whitespace` (19) | `:3697` |
| `"` `'` | `DoubleByteString` (9) / `SingleByteString` (17) | `:3545`, `:3298 ValidateStringToken` |
| `%` | `DirectVariable` (7) / `IncompleteDirectVariable` (8) | `:3565`, `:3817 ScanDirectVariable` |
| `(*` | `Comment` (2)/`DocComment` (3) | `:3573`, `:2230 ScanComment` |
| `//` | комментарий до конца строки | `:3603 ScanSingleLineComment` |
| `(`…`;` `[` `]` `^` `|` `#` `&` `)` `+` `,` `-` | односимвольный оператор | `:3700` |
| `*`, `**`, `/`, `**`(=Operator 160), `.`, `..` | операторы с двойным символом | `:3580-3599` |
| `:`,`>`,`<`,`=`,`<>`,`<=`,`>=`,`=>`,`:=`,`=:` | составные операторы | `:3610-3643` |
| `{` | `Pragma` (4) | `:3685`, `:2080 ReadScannerPragma` |
| буква/`_` | `Identifier` (13) или ключевое слово/оператор | `:3712 ScanIdentifierOrOperator` |
| `` ` `` | экранированный идентификатор | `:2382 ScanWeirdIdentifier` |
| цифра | `Integer`(14)/`Real`(16)/`Date`/`Time`/… | `:2478 ScanInteger` |

Особенности:
- Идентификаторы IEC: начальный символ `:2004 IsIdentifierStartCharacter`, продолжение `:2301 ScanIdentifierCharacters`; двойные подчёркивания — опция (`AllowMultipleUnderlines`).
- `TRUE`/`FALSE` → `Boolean` (1), `:2435 ScanForTrueFalseOrIdentifier`.
- Числа: десятичные, `2#`/`8#`/`10#`/`16#` (`:2519 ScanBasedInteger`), `_` внутри цифр (`:2547 ScanDigits`), вещественные с `.`/экспонентой (`:2560 OptionalExponent`).
- Типизированные литералы (`SINT#`, `REAL#`, `BOOL#`, `T#`, `TIME#`, `LT#`, `D#`, `LD#`, `TOD#`, `LTOD#`, `DT#`, `LDT#`, `UTF8'…'`, `UCHAR'…'`, `__XSTRING"…"`): `:2640 ScanTypedLiteral`, `:3001 ScanTimeLiteral`, `:2784/:2846/:2933`.
- Строки: одинарные — однобайтные, двойные — двухбайтные (WSTRING); escape-символ `$`: `:3298 ValidateStringToken`, `:3373 ValidateEscapeSequence`.
- Direct variables: `%` + location (`I|Q|M`) + size (`X|B|W|D|L`) + адрес `Int(.Int)*`; `*` → incomplete (`:3855 ScanSizePrefix`, `:3912 ScanLocationPrefix`). Partial access (напр. `%B3.0`): `:3934 ScanPartialAccess`.
- Вложенность комментариев — настройка `AllowNestedComments`.

---

## 2. Приоритет операторов (доказано по коду)

`ExpressionParser.ParseAssignment` (`ExpressionParser.cs:114`) вызывает `InfixOperationParser` (`InfixOperationParser.cs`),
который задаёт **левую ассоциативность** всех бинарных уровней (циклы `for … while IsXOperator`).
Единственный правоассоциативный уровень — присваивание (`ParseAssignment` рекурсивно вызывает себя, `:123`).

| Ур. | Категория | Операторы (Operator ID) | Направление | Доказательство |
|---|---|---|---|---|
| 0 (низший) | Присваивание | `:=`164, `S=`165, `R=`166, `REF=`185, `=:`189 | **правая** | `ExpressionParser.cs:114-137` |
| 1 | OR / XOR | `OR`129, `OR_ELSE`235, `XOR`131 | левая | `InfixOperationParser.cs:192-219` |
| 2 | AND | `AND`127, `AND_THEN`234 | левая | `InfixOperationParser.cs:156-183` |
| 3 | Сравнение | `=`179, `<>`180, `<`175, `<=`177, `>`176, `>=`178 | левая (цепочка) | `InfixOperationParser.cs:126-153` |
| 4 | Аддитивные | `+`157, `-`158 (и `__VCADD`258, `__VCSUB`259) | левая | `InfixOperationParser.cs:98-123` (`IsAddOperator:64`) |
| 5 | Мультипликативные | `*`159, `/`161, `MOD`126 (и `__VCMUL`260, `__VCDIV`261, `__VCDOT`262) | левая | `InfixOperationParser.cs:70-95` (`IsMulOperator:58`) |
| 6 | Префиксные (унарные) | `NOT`133, унарные `+`157/`-`158, `ADR/BITADR/SIZEOF/ABS/MIN/MAX/ROL/…` | префиксный | `ExpressionParser.cs:164-389`, `OperandParser.cs:472-475` |
| 7 | Постфиксные | `.`162, `(`167 вызов, `[`169 индекс, `^`183 разыменование, `#`271 namespace | левая цепочка | `OperandParser.cs:225-294` |
| 8 (высший) | Первичные (операнд) | литерал, идентификатор, `(expr)`, `%directvar`, `__NEW`, префиксные функции | — | `OperandParser.cs:420-490` |

Замечания:
- `**` (Operator 160) зарегистрирован в таблице сканера как `Operator|Declaration|Internal`, но **в грамматике выражений не участвует** — возведение в степень — функция `EXPT` (см. `ExpressionParser.ParseSTPrefixOperator`, где 160 разбирается только как размер/тип).
- Унарные `+`/`-` разбираются `UnaryPlusMinusParser`, `NOT` — `UnaryNotParser`, `MIN`/`MAX` — `MinMaxOperatorParser`, `THIS`/`SUPER` — `ThisAndBaseExpressionParser` (`ExpressionParser.cs:237-263`).
- Условные `AND_THEN`/`OR_ELSE` — в тех же уровнях, что `AND`/`OR` (short-circuit), `XOR` — на уровне OR.
- Оператор `CALC`141 — условный вызов (ConditionalCallParser).
- Приведение типов — не оператор, а функция `TYPE_TO_TYPE(...)` / `ANY_TO_TYPE(...)` (Operator 184, `ConversionExpressionParser`), конверсии генерируются в `OperatorTable.AddConversionOperators` (`OperatorTable.cs:708`).

---

## 3. Таблицы ключевых слов и операторов

Строятся в `OperatorTable.UpdateOperatorTable` (`OperatorTable.cs:360`) через:
`AddOperatorSymbols:375`, `AddKeywords:459`, `AddStandardOperators:541`, `AddOOKeywords:586`,
`AddDataTypeNames:599`, `AddSpecialOperators:661`, `AddILOperators:410`, `AddVectorOperatos:441`,
`AddConversionOperators:708`.

### 3.1. Символы-операторы (`OperatorTable.cs:377-406`)

| Символ | ID | Символ | ID | Символ | ID |
|---|---|---|---|---|---|
| `+` | 157 | `.` | 162 | `]` | 170 |
| `-` | 158 | `#` | 271 | `,` | 171 |
| `*` | 159 | `:` | 163 | `;` | 172 |
| `**` | 160 | `:=` | 164 | `..` | 173 |
| `/` | 161 | `=:` | 189 | `=>` | 174 |
| `S=` | 165 | `REF=` | 185 | `<` | 175 |
| `R=` | 166 | `(` | 167 | `>` | 176 |
| `)` | 168 | `[` | 169 | `<=` | 177 |
| `>=` | 178 | `=` | 179 | `<>` | 180 |
| `&` | 181 | `|` | 182 | `^` | 183 |

### 3.2. Ключевые слова ST/объявлений (`AddKeywords`, `OperatorTable.cs:461-537`)

| Ключевое слово | ID | Ключевое слово | ID | Ключевое слово | ID |
|---|---|---|---|---|---|
| ACTION | 60 | ELSE | 68 | REPEAT | 96 |
| ARRAY | 61 | ELSIF | 69 | RETAIN | 97 |
| PARAMS | 62 | END_* | 70-82 | RETURN | 98 |
| AT | 63 | EXIT | 83 | STRUCT/UNION | 99/100 |
| BY | 64 | CONTINUE | 84 | THEN | 101 |
| CASE | 65 | FOR | 85 | TO | 102 |
| CONSTANT | 66 | FUNCTION | 87 | TYPE | 103 |
| DO | 67 | FUNCTION_BLOCK | 88 | UNTIL | 104 |
| IF | 89 | VAR…VAR_STAT | 105-114 | WHILE | 115 |
| OF | 90 | EXTENDS | 116 | IMPLEMENTS | 117 |
| METHOD | 118 | INTERFACE | 119 | THIS | 120 |
| SUPER | 121 | PROPERTY | 187 | VAR_INST | 244 |
| VAR_GENERIC | 280 | NAMESPACE | 287 | END_NAMESPACE | 288 |
| `__BEGIN_IMPLEMENTATION` | 286 | PERSISTENT | 91 | POINTER | 92 |
| REFERENCE | 186 | PROGRAM | 93 | READ_ONLY/READ_WRITE | 94/95 |
| PERSISTENT | 91 | — | — | — | — |

### 3.3. ООП-ключевые слова (`AddOOKeywords`, `OperatorTable.cs:588-595`)

`ABSTRACT`224, `OVERRIDE`225 (contextual), `PUBLIC`226, `PRIVATE`227, `PROTECTED`228,
`INTERNAL`229, `FINAL`230, `OVERLOAD`289 (contextual).

### 3.4. Спецоператоры и TRY/CATCH (`AddSpecialOperators`, `OperatorTable.cs:663-704`)

`__COPY`2, `__RELOC`1, `__LAZY`3, `__CRC`194, `__TYPEOF`193, `__VARINFO`191, `__SYSTEM`192,
`__POOL`251, `__INIT`196, `__CAST`202, `__FCALL`221, `__NEW`200, `__DELETE`201, `__WAIT`205,
`__TRY`238, `__ENDTRY`239, `__CATCH`240, `__FINALLY`241, `__THROW`242, `__ISVALIDREF`197,
`__QUERYINTERFACE`198, `__QUERYPOINTER`199, `__CURRENTTASK`255, `__POUNAME`278, `__POSITION`279,
`AND_THEN`234, `OR_ELSE`235, и др.

### 3.5. Приведение типов

- `TYPE1_TO_TYPE2(...)` и `ANY_TO_TYPE(...)` — Operator 184 (`ConversionExpressionParser`);
- `ANY_NUM_TO_TYPE`, `TO_TYPE` — генерируются в `AddOverloadedConversions` (`OperatorTable.cs:751`);
- `REFERENCE_TO_POINTER` (184) — `OperatorTable.cs:730-733`.

### 3.6. Зарезервированные, но не используемые (`ReservedUnusedKeywords.cs`)

`CHAR, WCHAR, ANY_DERIVED, ANY_ELEMENTARY, ANY_MAGNITUDE, ANY_SIGNED, ANY_DURATION, ANY_CHARS,
ANY_CHAR, CHAR_TO, TO_CHAR, WCHAR_TO, TO_WCHAR, ATAN2, USING, CLASS, NAMESPACE`.

---

## 4. Инструкции (Statements)

Диспетчер — `StatementParser.ParseSTStatement` (`StatementParser.cs:216`) → `ParseSTStatementHelp` (`:242`) →
`ParseOperatorStatement` (`:485`). Соответствие ключевого слова и парсера:

| Конструкция | Парсер | Ключ | Примечание |
|---|---|---|---|
| `IF … ELSIF … ELSE … END_IF` | `IfStatementParser.cs:19` | 89 | |
| `CASE … OF … END_CASE` | `CaseStatementParser.cs:21` | 65 | метки-диапазоны `a..b`, списки через `,`, `ELSE` |
| `FOR i := a TO b [BY s] DO … END_FOR` | `ForStatementParser.cs:19` | 85 | расширение `BY` (`ForLoopExtender.cs`) |
| `WHILE … DO … END_WHILE` | `WhileStatementParser.cs:19` | 115 | |
| `REPEAT … UNTIL … END_REPEAT` | `RepeatStatementParser.cs:19` | 96 | |
| `JMP label;` | `JumpStatementParser.cs:19` | 143 | |
| `RETURN;` | `ReturnStatementParser.cs:20` | 98 | |
| `EXIT;` | `StatementParser.cs:547` | 83 | |
| `CONTINUE;` | `StatementParser.cs:549` | 84 | |
| `__TRY … __CATCH [(e)] … __FINALLY … __ENDTRY` | `TryCatchStatementParser.cs:20` | 238-241 | порядок TRY→CATCH→FINALLY→ENDTRY (`:236-240`) |
| `CALC(...)` | `ConditionalCallParser.cs:19` | 141 | |
| `__WAIT;` | `StatementParser.cs:666` | 205 | |
| `__BEGIN_IMPLEMENTATION` | `ImplementationBlockParser.cs:20` | 286 | |
| `;`, выражение-инструкция | `StatementParser.cs:626/634` | 172/162 | |

## 5. Объявления (Declarations)

- POU: `POUSyntaxParser.cs` (вложенность `s_htValidChildPouTypes` `:502`, END-операторы `:361`/`:421`).
- Секции `VAR`: `StatementParser.cs:390`/`:485`, `VariableDeclarationParser.cs` (обработка `AT` — `:157`),
  `VariableListParser.cs`. Модификаторы: `CONSTANT`66, `RETAIN`97, `PERSISTENT`91, `READ_ONLY`94, `READ_WRITE`95.
- Типы: `TypeParser.cs:167 ParseType` → `HandleOperatorCase:191`; поддиапазон `:123`,
  массив `:572` (в т.ч. `ARRAY[*]` variable-length), `POINTER TO` `:675`, `REFERENCE TO` `:650`,
  `STRING/WSTRING/XSTRING` `:540/:510/:480`, `__VECTOR` `:450`, `PARAMS` `:269`, enum `EnumListParser.cs:111`,
  обобщённые типы `:408`.
- `TYPE … END_TYPE`: `TypeDeclarationParser.cs`.

## 6. Прагмы и макросы `{…}`

- Разбор: `Pragmas/PragmaStatementParser.cs:107 ParsePragmaIntern`, `:114` (диспетчер),
  `:178 ParsePragmaIf` (макро-`IF`), `PragmaScanner/PragmaScanner.cs`.
- Таблица ключевых слов прагм: `PragmaScanner.cs:40-88` (flow, noflow, bp, nobp, bp2, nobp2, p, bpdef,
  succ, error, fatalerror, warning, info, text, attribute, define, undefine, include, defined,
  project_defined, variable, type, task, xref, resource, implicit, allowpaths, messageguid, on, off,
  from, pou, hastype, hasattribute, assert, hasvalue, hasconstantvalue, hasconstanttype, OR, AND, NOT,
  show_compile, show_precompile, isenumtype, disable, restore, COMPILERVERSION, RUNTIMEVERSION).
- Макросы `{IF}/{ELSIF}/{ELSE}/{END_IF}` + `{DEFINED(x)}` → `PragmaIfStatementParser`
  (условие — `ParsePragmaORExp`, `PragmaIfStatementParser.cs:115-140`).
- Токены прагмы: `PragmaTokenType` (Identifier/Integer/Operator/String/Error/End).

---

## 7. Пробелы и допущения

1. Часть вспомогательных данных (числовые массивы операторов) компилятор C# вынес в
   `<PrivateImplementationDetails>`; конкретные ID восстановлены по `OperatorTable.cs` и контексту
   (напр. `EnumListParser.cs:126-134`).
2. Точные правила разбора IL-кода (реализация в Instruction List) намеренно не включены — задача
   ограничена ST.
3. Битовый доступ к direct variables (`%IX0.0`) описан как `PartialAccess`; полный синтаксис
   компонентов адреса — `GetDirectVariable` (`InternalScanner.cs:519`).
4. Инициализаторы массивов/структур описаны укрупнённо (`arrayInitializer`, `structInitializer`) —
   детали в `Expressions/ArrayInitializationParser.cs`, `StructureInitializationParser.cs`,
   `InitializationParser.cs`.
