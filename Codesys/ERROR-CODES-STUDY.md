# CODESYS: формирование SYNTAX-ошибок (сканер/парсер) и маппинг на парсер IronPLC

Учебный разбор: как CODESYS 3.5.22.10 порождает **синтаксические** ошибки
(фазы сканера/парсера), как они отделены от ошибок предкомпиляции/типов/сборки,
и как это соотносится с кодами `P####` нашего парсера (`compiler/parser`).

Целевая аудитория: инженер/LLM, которому нужно понять систему кодов без grep.
Все `file:line` — по артефактам этого каталога (`Codesys/`), кроме явно указанных
`Program Files` (только чтение).

Быстрые ссылки на «сырьё»:

- Пайплайн предкомпиляции: [`docs/02_PRECOMPILE_PIPELINE.md`](docs/02_PRECOMPILE_PIPELINE.md)
- Каталог `MessageId`: [`docs/03_ERROR_CATALOG.md`](docs/03_ERROR_CATALOG.md)
- Агрегация/дедуп: [`docs/19_MESSAGE_AGGREGATION.md`](docs/19_MESSAGE_AGGREGATION.md)
- Границы фаз: [`docs/14_COMPILER_PHASES.md`](docs/14_COMPILER_PHASES.md)
- Лексер/парсер: [`docs/01_LEXER_PARSER.md`](docs/01_LEXER_PARSER.md), [`docs/06_AST_BUILDER_MAP.md`](docs/06_AST_BUILDER_MAP.md) (§5.5)
- Таблицы: [`tables/errors/`](tables/errors/)
- Наш парсер: `../compiler/parser/src/`
- Наши P-коды: `../compiler/problems/resources/problem-codes.csv`

---

## 1. Как CODESYS формирует syntax-ошибки

### 1.1. Кто поднимает сообщение

| Источник | Что делает | Есть ли `MessageId` | Где |
|---|---|---|---|
| **Сканер** (`InternalScanner`) | токенизация; при лексической ошибке отдаёт **error-token** (`TokenType.Error=20`), без `MessageId` | **нет** | `Parser35220.plugin/CODESYS/Parser35220/Scanner/InternalScanner.cs`; таблица типов `tables/token_types.csv` (`Error=20`, `End=21`) |
| **Парсер** (`Parser35220`) | `IErrorHandler.AddError/AddErrorST/AddErrorSTWithToken/AddWarningST` c числовым `MessageId` | **да** (48 id, см. §3) | `Parser35220.plugin/.../Statements/*.cs`, `Declaration/*.cs`, `Expressions/*.cs`, `Pragmas/*.cs`, `Utilities/ScannerExtensions.cs` |
| **Precompile-checker** (фаза B1) | `SimpleTypeChecker`/`SimpleTypeInferrer`: резолвинг имён/типов без кода | да (другой набор) | `decompiled/Compiler35220.plugin/.../PreCompile/Typification/SimpleTypeChecker.cs:1028`; вход `PreCompileContext.CheckSignature/CheckPOUCode` (см. `docs/14_COMPILER_PHASES.md` §(a) B1) |
| **Компилятор** (фазы C1–C5) | типизация/typecheck/codegen/linking/online-change | да (основная масса каталога) | `decompiled/Compiler35220.plugin/_3S/CoDeSys/Compiler35220/{Phase1_Typification,Phase4_TypeCheck,...}` |

Текст сообщений живёт отдельно от кода: парсер кладёт `MessageId` + аргументы,
а строка берётся из ресурса `ErrorMessages` текущей культуры (§1.3).

Ключевой архитектурный факт: **сам разбор исходника — ленивый и общий** для
precompile и compile. Точка входа парсинга POU — `CompiledPOU.AfterDeserialize`
(`decompiled/LanguageModelManager.plugin/_3S/CoDeSys/LanguageModelManager/CompiledPOU.cs:830`):
`CompilerProxy.CreateParser(this.m_stCode).ParseST(this.NoAccess)`.
Парсер знает только про токены и `IErrorHandler` (`Parser35220.plugin/.../Utilities/ParserContext.cs:13-29`).

### 1.2. `MessageId`: enum, имена ключей, числовой id, severity

- Enum: `_3S.CoDeSys.LanguageModelManager.InternalInterfaces.MessageId`
  (`decompiled/Compiler/_3S/CoDeSys/LanguageModelManager/InternalInterfaces/MessageId.cs:5`);
  в LMM-типах указан как `_dump/compiler_types.txt:10706` (`docs/02_PRECOMPILE_PIPELINE.md:188-189`).
- Числовые значения: `tables/errors/message_ids.csv` (515 строк с заголовком:
  7 значений enum `InternalErrorIds` + **507** значений `MessageId`;
  значения `MessageId` 0..591, **есть пропуски** — алиасов нет).
- Соглашение об именах: ключ ресурса = `Err_*`/`Wrn_*`/`Inf_*`/`Txt_*`
  (`tables/errors/error_severity.csv:1-6`); `None=0` = «сообщения нет».
  Перепись по enum: `Err_`=426, `Wrn_`=77, `Inf_`=2, `Txt_`=1, `None`=1;
  перепись по union locale-ресурсов — 500 ключей (`docs/03_ERROR_CATALOG.md:8-19`
  даёт `Err_=427, Wrn_=66, Inf_=4, Txt_=1` — метрика другого множества).
- Severity — битовая маска `enum Severity : uint` в `decompiled/MessageStorage/Severity.cs:9-32`:
  `FatalError=1, Error=2, Warning=4, Information=8, Text=16, SuppressedWarning=32, SuppressedInformation=64`.
  Префикс ключа — только соглашение; фактическая severity задаётся при создании
  сообщения: `AddWarningST` → `AddWarning` (`decompiled/Compiler35220.plugin/-/-.118.cs:108-111`),
  `AddError` → `Severity.Error` жёстко (`-/-.118.cs:66-83`).
- `MessageId` — это ordinal в отдельном поле сообщения (`CompilerMessage.MessageId`,
  `decompiled/LanguageModelManager.plugin/.../CompilerMessage.cs:222-230`), а не часть текста.

### 1.3. Локализация

- Строки лежат в ресурсе `ErrorMessages.resources` сборки `Compiler35220.plugin`
  (~500 ключей `Err_*`/`Wrn_*`, `docs/06_AST_BUILDER_MAP.md:493-495`),
  плюс satellite-сборки локалей: `Compiler35220.ru.resources.dll`,
  `Compiler35220.en.resources.dll` и т.д. (`docs/03_ERROR_CATALOG.md:21-31`).
- Таблицы в этом каталоге: 10 локалей — de, en, es, fr, it, ja, pt-BR, ru, tr, zh-CHS
  (`tables/errors/error_messages_<loc>.csv`, формат `id,key,text`); 11-й CSV
  `error_messages_ru_provenance.csv` — не локаль, а трассировка происхождения
  RU-строк (`id,key,text,source`). JSON-версия: `tables/errors/error_messages.json`
  (`id → {names, key, ru, en, otherLocales}`).
- RU-таблица расширена до 100% (507 ключей) там, где штатная `ru` содержала 320
  (`docs/03_ERROR_CATALOG.md:14`).
- Резолвинг текста в рантайме: `IErrorHandler.LoadString(MessageId, args)`
  (`decompiled/Compiler/CODESYS/Parser/IErrorHandler.cs:18`); реализация —
  статический `LDictionary<MessageId,string>` (`-/-.353.cs:6-22,554-567`),
  наполняемый строками ресурса `\u0081.\u0002.Err_*` (например `-/-.353.cs:27-80`),
  плюс `string.Format` аргументов (`-/-.118.cs:102-105`).
- Важно: `MessageId` **никогда не показывается как ключ** — на выход идут
  локализованный текст + числовой код (§1.4).

### 1.4. Как сообщение превращается в `(code, message, file, line, column)`

1. Парсер: `ErrorHandler.AddErrorST(node, id, args)` → узел red-tree копит
   `MessagesList` (`-/-.118.cs:108-126`).
2. Сбор ошибок дерева: `ErrorVisitor` обходит узлы и превращает их в
   `_ICompilerMessage` (`decompiled/Compiler35220.plugin/_3S/CoDeSys/Compiler35220/Messaging/ErrorVisitor.cs:84-123`).
3. Агрегация: `Messages.\u0001(...)` последовательно собирает глобальные
   диагностики → сигнатуры → POU → суммаризацию библиотек → статистику памяти
   (`Messaging/Messages.cs:28-41`; `docs/19_MESSAGE_AGGREGATION.md:18-37`).
4. Запись: `IMessageStorage.AddMessage(category, msg)` — append-only
   (`-/-.352.cs:157`; IL `AddMessage` в `docs/19_MESSAGE_AGGREGATION.md:118-128`).
5. Чтение: `GetMessages(category, severityMask)` копирует в порядке вставки;
   маска «с предупреждениями» = `(Severity)6` (`docs/02_PRECOMPILE_PIPELINE.md:204`).
6. Рендер полей (`docs/02_PRECOMPILE_PIPELINE.md:193-207`):

| Поле | Источник |
|---|---|
| `code` | `IMessage4.Prefix + Number.Value.ToString("D4")`; `Number = (uint)MessageId`, `Prefix = "C"` |
| `message` | `msg.Text` (локализованный, уже с подстановками) |
| `file` | `ObjectGuid` → `IObjectManager8.GetObjectNamePath` |
| `line/column` | `IObject.GetPositionText(msg.Position)` (позиция локализована) |
| `severity` | `(int)msg.Severity`: 1/2 — error, 4/32 — warning |
| диапазон | `msg.Position / PositionOffset / Length` (диапазон из токена, `-/-.118.cs:86-90`) |

Сканер уже считает `SourceLine`/`SourceColumn` (`Scanner/Token.cs:38-43`), но в
сообщение идут `Position`/`PositionOffset`, а строка/колонка вычисляются слоем
объектов при показе.

### 1.5. Порядок и дедупликация

- Отдельной сортировки сообщений нет ни в агрегации, ни в хранилище: порядок =
  порядок вставки (`docs/19_MESSAGE_AGGREGATION.md:7-12,149-151`).
- Шаги агрегации последовательны (fan-in после `Thread.Join` воркеров), списки
  обходятся детерминированно (`docs/19_MESSAGE_AGGREGATION.md:39-68`).
- Дедуп: ключ `(MessageId, Position, PositionOffset, Text, Severity, ObjectGuid)`,
  первое вхождение выигрывает; `HashSet` порядок не меняет
  (`docs/19_MESSAGE_AGGREGATION.md:70-89`). `ProjectHandle` в ключ не входит — GAP-2.
- Фильтры (не сортировка): маска severity, лимиты warning/error (500),
  суммаризация ошибок библиотек (`docs/19_MESSAGE_AGGREGATION.md:100-102`).

### 1.6. Почему «C0xxx» нет в таблицах как отдельной сущности

C-код существует, но он **вычисляется из enum**, а не хранится:

- `CompilerMessage.Prefix => "C"` (`decompiled/LanguageModelManager.plugin/.../CompilerMessage.cs:299-305`);
  `Number => (uint)MessageId` (или `null` при `None`, `:285-295`).
- Та же формула используется внутри для подавления предупреждений по коду:
  `string.Format("{0}{1:d4}", Prefix, Number)` (`Compiler35220/.../Messaging/ErrorVisitor.cs:108-115`).
- Потребитель рендерит `Prefix + Number:D4` (`docs/02_PRECOMPILE_PIPELINE.md:197`).
  Пример: `Err_UnexpectedTokenFound = 9` → `C0009`.
- Префикс — свойство конкретного класса сообщения: подкласс
  `SpecialCompilerMessage` может задать свои `Prefix/Number`
  (`decompiled/LanguageModelManager.plugin/.../SpecialCompilerMessage.cs:22-58`).

Следствия, которые надо усвоить:

1. В CSV-таблицах хранятся **enum + ключ**, C-номера в них нет и не должно быть.
2. Числа — сквозные для **всех** фаз и подсистем; значения 0..591 с пропусками.
3. Нет диапазонов под фазы (в отличие от наших `P0001-1999` = parse). Признак
   «это syntax-ошибка» — **кто поднял** (сборка `Parser35220.plugin`) и
   категория (`PreCompileMessageCategory` / `CompilerMessageCategory`),
   а не номер.
4. Максимум сейчас 591 < 1000, поэтому коды физически выглядят как «C0xxx»;
   id ≥ 1000 отрендерился бы как `C1000+`.

---

## 2. Границы класса SYNTAX

Определение для этого каталога: **SYNTAX = всё, что детектируется на потоке
токенов и на parse-tree без резолвинга имён/типов и без генерации кода.**

| Слой | Кто | Класс ошибок | Где продолжать читать |
|---|---|---|---|
| A1. Scanner | `InternalScanner` | лексические: неизвестный символ, незакрытая строка/escape, незакрытый комментарий/прагма, `__` подряд. **`MessageId` не назначается** — отдаётся token `Error(20)` | `Scanner/InternalScanner.cs`; §3b |
| A2/A5. Parser | `Parser35220.*` | структурный синтаксис: expected/unexpected token, `;`, скобки, ветки EOF, разбор деклараций и типов по грамматике, прагмы | 48 id в `tables/errors/parser35220_message_ids.csv`; §3 |
| B. Precompile-проверки | `SimpleTypeChecker`, `SimpleTypeInferrer`, `PreCompileTypifier` | резолвинг имён/типов, ссылки, простые типовые правила; категория `PreCompileMessageCategory` | `docs/02_PRECOMPILE_PIPELINE.md` §2.1; `docs/14_COMPILER_PHASES.md` §(a) B |
| C. Компиляция | Phase1..Phase5 + linker/layout | типизация, typecheck, память/адресация, online-change, линковка; категория `CompilerMessageCategory` | `docs/14_COMPILER_PHASES.md` §(a) C; `docs/02_PRECOMPILE_PIPELINE.md` §2.2 |
| Другие плагины/слои | SFC, XML, менеджер объектов | в enum `MessageId` этого каталога не входят (свой учёт) | XML/PLCopen у нас: `../specs/steering/plcopen-xml-module.md`; SFC — отдельный плагин CODESYS |

Примеры «не syntax» (чтобы не путать): `Err_IdentNotDefined=46`,
`Err_UnknownType=77` — резолвинг (B1); `Err_TypeMismatch=32`,
`Err_TypesNotComparable=66` — typecheck (C4); `Err_OutOfMemory=104`,
`Err_AddressOutOfRange=111` — локация/память (C3/C5);
`Err_NoOnlineChangePossible=184` — online-change (C5).

Особенности границы:

- Сканер сам сообщений не создаёт: ошибка живёт как «плохой» токен, и только
  парсер присваивает ей `MessageId` (обычно из ожиданий — `6`, `9`, `189`),
  подставляя сырой текст токена (`ScannerExtensions.cs:194-205`,
  `StatementParser.cs:169-173,189-190,893-902`).
- Часть id из 48 — «пограничные»: формально поднимаются в парсере, но по смыслу
  близки к типам/семантике (например `Err_NoPointerToBit=205`,
  `Err_ReferenceNotAllowed=261`, `Err_AnyTypeOnlyInFunction=311`). Для нашего
  порта такие проверки уместны в analyzer-диапазонах (§4.3).
- `LanguageModelManager.LanguageModelBuilder.cs:1901` тоже поднимает
  `MessageId.Err_UnexpectedTokenFound` (сборка LMM) — тот же id может приходить
  не из `Parser35220`; ещё одна причина не считать «id ⇒ фаза».

---

## 3. Полный список syntax-релевантных кодов

### 3a. 48 кодов `Parser35220` (полностью)

Источник: `tables/errors/parser35220_message_ids.csv` (48 уникальных id; колонка
`count` — число мест вызова в исходниках, `files`/`examples` — файлы/строки).
EN/RU сверены с `tables/errors/error_messages_en.csv` и `…_ru.csv` (в RU-таблице
100% покрытие; в parser-CSV часть RU была пустой и заполнена из неё).

| id | key | EN | RU | Когда возникает |
|---|---|---|---|---|
| 1 | Err_ConstantOverflow | Constant '{0}' too large for type '{1}' | Константа '{0}' слишком велика для типа '{1}' | литерал не влезает в типизированный литерал (`FactoryExtension.cs:174`) |
| 2 | Err_Operator1of2Expected | '{0}' or '{1}' expected instead of '{2}' | '{0}' или '{1}' требуется вместо '{2}' | ожидался один из двух токенов (9 мест: var-decl, вызовы, min/max, NEW, операнды, pragmas) |
| 3 | Err_BitNrOverflow | '{0}' is no valid bit number for '{1}' | '{0}' не является корректным битовым номером для '{1}' | битовый индекс вне типа (`OperandParser.cs:173,192`) |
| 4 | Err_NoComponentOf | '{0}' is no component of '{1}' | '{0}' не является компонентом '{1}' | компонентный доступ к чужому полю (`OperandParser.cs:146,213`) |
| 5 | Err_OverflowInAddress | Constant overflow in address '{0}' | Постоянное переполнение по адресу '{0}' | переполнение константы в `%I...` (`OperandParser.cs:457`) |
| 6 | Err_OperatorExpected | '{0}' expected instead of '{1}' | '{0}' требуется вместо '{1}' | основной «ожидался оператор» (52 места) |
| 7 | Err_ExpressionExpectedInstead | Expression expected instead of '{0}' | Вместо '{0}' требуется выражение | операнд выражения/прагма (`OperandParser.cs:482`) |
| 8 | Err_Operator1of3ExpectedInsteadofEOF | Unexpected End-of-file found: '{0}', '{1}' or '{2}' expected | Недопустимый End-of-file: требуется '{0}', '{1}' или '{2}' | ветки IF/PRAGMA_IF/TRY-CATCH на EOF |
| 9 | Err_UnexpectedTokenFound | Unexpected token '{0}' found | Обнаружен недопустимый символ '{0}' | общий unexpected (в т.ч. текст scanner-error-токена) |
| 10 | Err_OperatorExpectedInsteadofEOF | Unexpected End-of-file found: '{0}' expected | Недопустимый End-of-file: требуется '{0}' | EOF в CASE/FOR/REPEAT/WHILE |
| 11 | Err_NoCaseLabelFound | No CASE label found | Метка не найдена | пустая ветка CASE (`CaseStatementParser.cs:360`) |
| 22 | Err_OpNeedsExactInputs | '{0}' needs exactly '{1}' operands | Для '{0}' требуется ровно '{1}' операндов | `WAIT`/оператор с фиксированным числом операндов (`StatementParser.cs:869`) |
| 24 | Err_IllegalOperator | '{0}' is no valid ST operator | '{0}' не является корректным ST-оператором | оператор, недопустимый в ST (`HasConstantValueOrTypePragmaParser.cs:250`) |
| 26 | Err_IdentifierExpected | Identifier expected instead of '{0}' | Вместо '{0}' требуется идентификатор | 14 мест: ENUM, POU, TYPE, var-decl, NEW, операнды, pragmas |
| 27 | Err_StringSizeExpected | size of string expected after "(" | После "(" требуется размер строки | `STRING(...)`/`WSTRING(...)` без размера (`TypeParser.cs:498,528,559`) |
| 30 | Err_AddressExpected | Direct address expected after AT instead of {0} | После "AT" вместо {0} требуется прямой адрес | `AT` без адреса (`VariableDeclarationParser.cs:246`) |
| 31 | Err_TypeExpected | Type definition expected instead of '{0}' | Вместо '{0}' требуется определение типа | ожидался тип (`TypeParser.cs:383`) |
| 51 | Err_AttributeNameExpected | Single byte string expected for an attribute value instead of '{0}' | Для значения атрибута вместо '{0}' требуется однобайтовая строка | значение атрибута не string (`HasAttributePragmaParser.cs:105`) |
| 81 | Err_UnexpectedPragmaif | Unexpected pragma: '{0}' found without matching 'if' | Недопустимая директива: '{0}' обнаружено без соответствующего 'if' | `{ELSIF}/{ELSE}/{END_IF}` без `{IF}` (`PragmaStatementParser.cs:185`) |
| 85 | Err_DefineValueExpected | Define value expected instead of '{0}' | Значение требуется вместо '{0}' | `{DEFINE x value}` без значения (`HasValuePragmaParser.cs:109`) |
| 98 | Err_FunctionBlockNoLongerValid | The keyword FUNCTIONBLOCK is no longer supported. Use FUNCTION_BLOCK instead. | Ключевое слово FUNCTIONBLOCK больше не поддерживается. Используйте FUNCTION_BLOCK | legacy-ключевое слово (`StatementParser.cs:915-916`) |
| 114 | Err_InvalidJumpDestination | Invalid destination {0} for JMP | Некорректная цель {0} для JMP | разбор цели JMP (`JumpStatementParser.cs:113`) |
| 115 | Err_CalcNeedsCall | Second parameter of conditional call must be a valid call statement | Второй параметр условного вызова должен соответствующим оператором вызова | `CALC`/условный вызов (`ConditionalCallParser.cs:135`) |
| 182 | Err_ReturnTypeForNonFunction | Return type is only possible for POUs of type FUNCTION and METHOD | Возвращаемый тип допустим только для POU типа FUNCTION и METHOD | тип у PROGRAM/FB (`POUSyntaxParser.cs:270`) |
| 189 | Err_SemicolonExpected | ';' expected instead of '{0}' | ';' требуется вместо '{0}' | отсутствует `;` (`StatementParser.cs:899`) |
| 190 | Err_SemicolonExpectedInsteadOfEnd | ';' expected instead of end of POU | Вместо конца POU требуется ';' | `;` перед EOF (`StatementParser.cs:893`) |
| 205 | Err_NoPointerToBit | POINTER TO BIT is not allowed | POINTER TO BIT недопустим | тип `POINTER TO BIT` (`TypeParser.cs:689`) |
| 206 | Err_NoArrayOfBit | BIT is not allowed as base type of an array | BIT недопустим в качестве базового типа массива | `ARRAY OF BIT` (`TypeParser.cs:643`) |
| 248 | Err_NewNeedsType | Type definition expected as operand for __NEW | Определение типа требуется в качестве операнда для __NEW | `__NEW` без типа (`NewExpressionParser.cs:116`) |
| 261 | Err_ReferenceNotAllowed | A reference type is not allowed as base type of an array, pointer, or reference | Ссылочный тип недопустим в качестве базового типа массива, указателя или ссылки | тип-ограничение (`TypeParser.cs:669`) |
| 272 | Err_NoReferenceToBits | References to bits are not possible | Ссылки на биты недопустимы | REFERENCE TO BIT (`TypeParser.cs:664`) |
| 303 | Err_StructureInitialisationNotPossible | A structure initialisation is not possible as Parameter of an Init-function call. Use a variable instead. | Инициализация структуры недопустима в качестве параметра вызова Init-функции. Используйте переменную. | struct-инициализатор в init-func (`VariableDeclarationParser.cs:408`, `NewExpressionParser.cs:238`) |
| 304 | Err_ArrayInitialisationNotPossible | An array initialisation is not possible as parameter of an initial function call. Use a variable instead | Инициализация массива недопустима в качестве параметра вызова Init-функции. Используйте переменную | array-инициализатор в init-func (`VariableDeclarationParser.cs:413`, `NewExpressionParser.cs:243`) |
| 311 | Err_AnyTypeOnlyInFunction | Variables of type '{0}' only allowed as input of functions | Переменные типа '{0}' допустимы только в качестве входов функции. | `ANY*` вне входа функции (`TypeParser.cs:370`) |
| 317 | Err_LiteralExpected | Literal expected instead of '{0}' | Вместо '{0}' требуется литерал | прагма `hasconstantvalue` (`HasConstantValueOrTypePragmaParser.cs:202`) |
| 372 | Err_DuplicateElseInCaseStatement | Duplicate definition of ELSE in CASE statement | Повторное определение блока CASE | второй `ELSE` в CASE (`CaseStatementParser.cs:320`) |
| 386 | Err_VarLengthArrayTopLevel | A variable length array type has to be on top level position of a type declaration | Тип массива переменной длины должен быть на верхней позиции в объявлении типа | `ARRAY[*]` не top-level (`TypeParser.cs:583`) |
| 449 | Err_ComparisonOperatorExpected | Comparison operator expected instead of '{0}' | Оператор сравнения требуется вместо '{0}' | версия в прагме (`PragmaVersionOperandParser.cs:127`) |
| 450 | Err_StringLiteralExpected | String literal expected instead of '{0}' | Строковый литерал требуется вместо '{0}' | версия в прагме (`PragmaVersionOperandParser.cs:147`) |
| 451 | Err_VersionOverflow | At least one part of the version '{0}' has a too big value | По крайней мере одна часть версии '{0}' имеет слишком большое значение | `PragmaVersionOperandParser.cs:193` |
| 452 | Err_VersionPartNegative | At least one part of the version '{0}' has a negative value | По крайней мере одна часть версии '{0}' имеет отрицательное значение | `PragmaVersionOperandParser.cs:177` |
| 453 | Err_VersionInvalidFormat | Token '{0}' is no valid version | Символ '{0}' имеет некорректную версию | `PragmaVersionOperandParser.cs:185,201` |
| 500 | Err_VectorSizeNotValid | The size of a vector must be a integer greater than 0 and less than 9 | Размер вектора должен быть целым числом больше 0 и меньше 9 | `TypeParser.cs:460` |
| 570 | Err_ProjectDefinedNotSupportedFor | The condition 'project_defined' is not supported for this syntax, since it might affect the public interface of '{0}'. | Условие «project_defined» не поддерживается для данного синтаксиса, поскольку может повлиять на открытый интерфейс «{0}». | `project_defined` в ENUM (`EnumListParser.cs:239`) |
| 578 | Err_UnexpectedStatement | Unexpected statement | Неожиданный оператор | 7 мест: checker'ы дерева (`POUSyntaxParser.cs:140,182,252,285`; `CodeStatementChecker.cs:29`; `DeclarationStatementChecker.cs:42`) |
| 579 | Err_UnsupportedFeature | The compiler feature '{0}' is only supported with compiler version {1} or newer | Компиляторная функция «{0}» поддерживается только начиная с версии компилятора {1} | гейт по версии компилятора (`ParserContext.cs:116-123`) |
| 584 | Err_MaxNestingDepthExceeded | Maximum nesting depth exceeded. | Превышена максимальная глубина вложенности. | переполнение глубины (`InternalParser.cs:203-208`; `CodeStatementChecker.cs:31,57`) |
| 588 | Err_MissingImplementationTerminator | Could not find the terminator '{0}' for the implementation block. | Не удалось найти терминатор «{0}» для блока реализации. | нет `END_*` (`ImplementationBlockParser.cs:75`) |

Классификация по происхождению: чистая лексика/синтаксис — 2, 6, 7, 8, 9, 10,
24, 26, 27, 30, 31, 51, 81, 85, 98, 114, 115, 189, 190, 578, 584, 588;
декларации/типы в парсере — 1, 3, 4, 5, 11, 22, 182, 205, 206, 248, 261, 272,
303, 304, 311, 317, 372, 386, 449–453, 500, 570, 579.

### 3b. Scanner: условия, где рождается «плохой» токен (без `MessageId`)

Токен `Error(20)` позже переоткрывается парсером одним из кодов 6/9/189/190
(текст берётся `Scanner.GetTokenText`, `InternalScanner.cs:2141-2148`).

| Условие | Где (InternalScanner.cs) | Результат |
|---|---|---|
| Неизвестный/недопустимый символ (не identifier/digit/operator) | `GetNextInternal` IL_51A → `ScanIdentifierOrOperator:2382-2388` (выход с сохранённым `Type=20`) | `Error(20)` |
| Незакрытая/некорректная строка или плохой `$`-escape | `GetNextInternal:3545-3555` + `ValidateStringToken:3298-3363` (+`ValidateEscapeSequence:3373`) | `Error(20)` |
| Незакрытый блочный комментарий `(* ... <EOF>` | `ScanComment:2230-2298` (тип `2` ставится только при найденном `*)`) | `Error(20)` |
| Незакрытая прагма `{ ... <EOF>` | `ScanPragma:3990-4033` (тип `4` только при `}`) | `Error(20)` |
| Двойные `__` при `AllowMultipleUnderlines=false` | `ScanForTrueFalseOrIdentifier:2435-2445` (`bUnderlineError → Type=20`) | `Error(20)` |
| Конец файла | `GetNextInternal:3506-3509` | `End(21)` (не ошибка; парсер отдаёт EOF-ветки 8/10/190) |
| `GetError(token)` | `:954-961` (требует `Type=20`, возвращает сырой текст) | вызовов в декомпиле не найдено (GAP; парсер использует `GetTokenText`) |

Подтверждающие описания: `docs/01_LEXER_PARSER.md:75,124,188,304`;
`docs/06_AST_BUILDER_MAP.md:352,442`.

---

## 4. Наш проект: `compiler/parser` и коды `P####`

### 4.1. Существующие коды (диапазон parse: P0001–1999)

Конвейер (`compiler/parser/src/lib.rs:62-108`):
`preprocess → tokenize` (`lexer.rs`) → `xform_collapse_pragmas` →
`xform_split_duration_units` → `insert_keyword_statement_terminators` →
`xform_demote_keywords` → `check_tokens` (список правил `lib.rs:86-93`) → plc-parser.

| Код | Имя | Где эмитится | Статус |
|---|---|---|---|
| P0001 | OpenComment | — (эмиттера нет во всём репозитории) | зарезервирован; кандидат на подключение для незакрытого `(*` |
| P0002 | SyntaxError | `parser.rs:136-159` (`syntax_error`, общий отказ парсера) | активен |
| P0003 | UnexpectedToken | `lexer.rs:75-92` (logos `Err` — нераспознанный текст) | активен |
| P0004 | CStyleComment | `rule_token_no_c_style_comment.rs:21` | активен (гейт `--allow-c-style-comments`) |
| P0005 | UnexpectedElement | — (эмиттера нет) | зарезервирован |
| P0006–P0009 | XmlMalformed, XmlSchemaViolation, SfcMissingInitialStep, TwinCatMalformed | другие модули (источники/XML) | вне парсера |
| P0010 | Std2013Feature | — (эмиттера не найдено) | зарезервирован |
| P0011 | EmptyVarBlock | `rule_no_empty_var_blocks.rs:68` | активен |
| P0012 | InvalidStringEscape | `rule_token_string_escape.rs:36` | активен |
| P4033 | PartialAccessSyntaxDisabled | `rule_token_no_partial_access_syntax.rs:34` | активен, но лежит в semantic-диапазоне (гейт синтаксиса) |
| P4042 | ParenStringLengthNotAllowed | `rule_token_no_paren_string_length.rs:54` | активен, тоже в P4-диапазоне |

Важное расхождение с CODESYS: у нас незакрытый `(*` сейчас даёт **P0003**
(logos не матчит regex комментария), тогда как у CODESYS это scanner `Error(20)`
→ parser-message. Правильный «дом» для нашего случая — уже существующий
**P0001 OpenComment** (не новый код).

### 4.2. Правила оформления нового P-кода

По `specs/steering/problem-code-management.md:21-76` (+ skill `ironplc-dev`,
раздел Problem codes):

1. Выбрать следующий свободный код своего диапазона: **parse = P0001–P1999**
   (сейчас свободен следующий `P0013`; `P0001–P0012` заняты).
2. Добавить строку в `compiler/problems/resources/problem-codes.csv`:
   `P####,PascalCaseName,Brief message` (стабильный код; удалять нельзя).
3. Создать страницу `docs/reference/compiler/problems/P####.rst` по шаблону
   (`=== / P#### / === / .. problem-summary:: P#### / описание / Example / fix`).
4. Эмитить через `Diagnostic::problem(Problem::X, Label::span(...))`
   (в `rule_token_*` — вернуть `Err(Vec<Diagnostic>)`; в парсере — заменить
   generic `syntax_error`).
5. Добавить тест, проверяющий код (`assert d.code() == "P####"`); для `P0xxx`
   тест обычно в `#[cfg(test)] mod test` самого rule-модуля или
   `compiler/parser/src/tests/*`.
6. `build.rs` (`compiler/problems/build.rs:22-143`) сам сгенерирует
   `Problem` enum + `code()`/`message()`; править сгенерированное нельзя.

### 4.3. Рекомендованный маппинг CODESYS syntax → наш P-код

**Подтверждено существующими кодами** (менять ничего не нужно):

| CODESYS | Условие | Наш код |
|---|---|---|
| 9 `Err_UnexpectedTokenFound` | нераспознанный текст / unexpected token | **P0003** (лексика) или **P0002** (парсер) |
| 6 `Err_OperatorExpected` (+2, 8, 10, 25, 28) | ожидался оператор/токен, вкл. EOF-ветки | **P0002** |
| 189/190 `Err_Semicolon*` | нет `;` / `;` перед EOF | **P0002** |
| 426 `Wrn_AtLeastOneExpected` | пустой VAR-блок | **P0011** |
| 1 `Err_ConstantOverflow` | переполнение типизированного литерала | **P2026** (analyzer) |
| scanner: незакрытый `(* ...` | comment EOF | **P0001** (уже зарезервирован; подключить) |

**Предложения новых кодов (НЕ добавлять в CSV на этом шаге).**
Все имена — PascalCase по конвенции; исходный сигнал — id CODESYS и/или
scanner-условие.

| Предлагаемый | Имя | Источник (CODESYS id / условие) |
|---|---|---|
| P0013 | OperatorExpected | 2, 6, 8, 10, 25, 28 |
| P0014 | IdentifierExpected | 26 |
| P0015 | TypeDefinitionExpected | 31, 248 |
| P0016 | SemicolonExpected | 189, 190 |
| P0017 | StringLiteralNotClosed | scanner `ValidateStringToken` (`:3549-3555`) |
| P0018 | PragmaNotClosed | scanner `ScanPragma` (`:3990-4033`) |
| P0019 | MaxNestingDepthExceeded | 584 |
| P0020 | LiteralExpected | 27, 317, 450 |
| P0021 | ExpressionExpected | 7 |
| P0022 | CaseLabelRequired | 11 |
| P0023 | PragmaIfUnmatched | 81 |
| P0024 | PragmaValueExpected | 51, 85 |
| P0025 | PragmaVersionInvalid | 451, 452, 453 |
| P0026 | AddressExpected | 30 |
| P0027 | JumpDestinationInvalid | 114 |
| P0028 | LoopIncrementInvalid | 17 |
| P0029 | ConditionalCallInvalid | 115 |
| P0030 | LegacyFunctionBlockKeyword | 98 |
| P0031 | UnexpectedStatement | 578 |
| P0032 | DuplicateElseInCase | 372 |
| P0033 | MissingImplementationTerminator | 588 |
| P0034 | IncompletePouDeclaration | 383 |
| P0035 | VariableDeclarationExpected | 211, 212, 213 |
| P0036 | ProjectDefinedNotSupported | 570 |
| P0037 | FeatureRequiresCompilerVersion | 579 |
| P0038 | ReturnTypeForNonFunction | 182 |
| P0039 | OperandCountInvalid | 22, 24 |
| P0040 | StructureInitialisationNotPossible | 303, 304 |
| P0041 | DirectAddressInvalid | 3, 4, 5 |

Для id, которые CODESYS проверяет в парсере, а у нас логичнее в анализаторе
(типы/декларации), предлагается **типовой диапазон P2000–P3999** (следующий
свободный — `P2042`):

| Предлагаемый | Имя | Источник |
|---|---|---|
| P2042 | PointerToBitNotAllowed | 205 |
| P2043 | ArrayOfBitNotAllowed | 206 |
| P2044 | ReferenceTypeBaseNotAllowed | 261, 272 |
| P2045 | AnyTypeOnlyInFunction | 311 |
| P2046 | VarLengthArrayTopLevel | 386 |
| P2047 | VectorSizeNotValid | 500 |

Итого: подтверждено существующими — 6 связок; предложено — 35 новых кодов
(P0013–P0041, P2042–P2047). Ни одна строка в CSV не добавлена.

---

## 5. Routing: куда смотреть, чтобы продолжить без grep

### 5.1. Документы

| Тема | Файл | Что внутри |
|---|---|---|
| Пайплайн, поля сообщения | `docs/02_PRECOMPILE_PIPELINE.md` | §2.1/2.2 поток, §3 поля `(code,message,file,line,column)` |
| Каталог `MessageId` | `docs/03_ERROR_CATALOG.md` | сводка, метод извлечения, полный дамп по темам |
| Агрегация/дедуп | `docs/19_MESSAGE_AGGREGATION.md` | порядок, ключ дедупа, лимиты, Rust-повтор |
| Фазы | `docs/14_COMPILER_PHASES.md` | A1/A2 (scanner/parser), B (precompile), C1–C5 |
| Лексер/парсер | `docs/01_LEXER_PARSER.md` | токены, `GetNextInternal`, точки `AddError*` |
| `MessageId` → ошибки | `docs/06_AST_BUILDER_MAP.md` §5.5 | текст-таблица 34 кодов и где поднимаются |
| Обфускация | `docs/09_OBFUSCATED_SYMBOLS.md` | `\u0003.\u0006` = форматтер + `IErrorHandler` |

### 5.2. Таблицы

- `tables/errors/message_ids.csv` — enum `MessageId` (507) + `InternalErrorIds` (7).
- `tables/errors/parser35220_message_ids.csv` — 48 syntax-кодов с файлами/строками.
- `tables/errors/error_messages_en.csv` / `…_ru.csv` / 8 других локалей — тексты.
- `tables/errors/error_messages_ru_provenance.csv` — источник каждой RU-строки.
- `tables/errors/error_severity.csv` — префиксы и их смысл.
- `tables/errors/error_messages.json` — всё вместе (id → ru/en/otherLocales).

### 5.3. Декомпил (CODESYS)

- Сканер: `Parser35220.plugin/CODESYS/Parser35220/Scanner/InternalScanner.cs`
  (`GetNextInternal:3488`, `GetError:954`, `ValidateStringToken:3298`,
  `ScanComment:2230`, `ScanPragma:3990`, `ScanForTrueFalseOrIdentifier:2435`).
- Парсер: `Parser35220.plugin/.../InternalParser.cs` (`ParseST:177`, 584 на `:203`),
  `Statements/StatementParser.cs` (`:86-95`, `:886-905`, `:907-923`),
  `Utilities/ScannerExtensions.cs` (`:120-206` — `MatchOperator`/id 6),
  `Utilities/ParserContext.cs` (`:116-123` — id 579).
- Обработчик ошибок парсера: `decompiled/Compiler35220.plugin/-/-.118.cs`
  (`IErrorHandler2`, `LoadString`, `AddErrorST`, severity).
- Текст-таблица `MessageId`→строка: `decompiled/Compiler35220.plugin/-/-.353.cs:554-567`.
- Сбор/агрегация: `decompiled/Compiler35220.plugin/.../Messaging/ErrorVisitor.cs`,
  `Messaging/Messages.cs`, `-/-.352.cs:157`.
- Модель сообщения: `decompiled/LanguageModelManager.plugin/.../CompilerMessage.cs`
  (`Prefix:299`, `Number:285`), `SpecialCompilerMessage.cs:22-58`;
  `decompiled/MessageStorage/{IMessage4.cs,Severity.cs}`.
- Точка парсинга POU: `decompiled/LanguageModelManager.plugin/.../CompiledPOU.cs:830`.
- LMM-парсер-адъяцент: `LanguageModelBuilder.cs:1901` (id 9).

### 5.4. Наш код

- `compiler/parser/src/token.rs` — `TokenType` (lexer), `token.rs:128` Identifier;
- `compiler/parser/src/lexer.rs` — P0003 (`:75-92`);
- `compiler/parser/src/parser.rs` — P0002 (`:136-159`);
- `compiler/parser/src/rule_token_*.rs` — токен-правила (P0004, P0012, P4033, P4042);
- `compiler/parser/src/rule_no_empty_var_blocks.rs` — P0011;
- `compiler/parser/src/lib.rs:62-108` — конвейер и регистрация правил;
- `compiler/problems/resources/problem-codes.csv` — реестр кодов;
- `compiler/problems/build.rs` — генерация enum/`code()`/`message()`;
- `docs/reference/compiler/problems/` — страницы P-кодов (шаблон: `P0001.rst`);
- `specs/steering/problem-code-management.md` — правила добавления кодов.

### 5.5. Как добавить ссылку из `Codesys/README.md`

Строка для таблицы маршрутизации README (добавить при следующей правке README):

```markdown
| Ошибки: SYNTAX-коды CODESYS и маппинг на наши P-коды | [`ERROR-CODES-STUDY.md`](ERROR-CODES-STUDY.md) |
```

---

## 6. Верификация каталога (2026-10-01)

Батч-верификация всех записей каталога против декомпила и сырых ресурсов 3.5.22.10
(первичные отчёты `verify-reports/*.json` удалены после консолидации; результаты сведены ниже
и в таблицы).

Диапазон: `MessageId` 0–506 (507 слотов: 423 привязанных + 84 незанятых) и `InternalErrorIds` 0–6.
Значения 507–591 в этот прогон не входили.

**Итог: 514 проверено — 415 verified, 2 fixed, 97 gap.**

### Исправлено (2)

| Id | Key | Коррекция |
|---|---|---|
| 245 | Wrn_MissingObjectForPersistent | Был EN=null и «authored» RU. Декомпил (`Compiler35220.plugin/-/-.353.cs:300`) привязывает член к ресурсному ключу `Err_MissingObjectForPersistent`; RU/EN + 8 локалей взяты из `resources/compiler35220/*.json` (подтверждено в satellite-DLL). |
| 362 | Err_InvalidStringSize | Был EN=null и «authored» RU. Привязка — ключ `Wrn_InvalidStringSize` (`-/-.353.cs:216`); официальный текст есть во всех 10 локалях. |

Обновлены `tables/errors/error_messages.json` (ru/en/otherLocales) и CSV: `error_messages_ru.csv`,
`error_messages_ru_provenance.csv` (source → `native_35220.ru`), а в 8 per-locale CSV
orphan-строки `Err_MissingObjectForPersistent`/`Wrn_InvalidStringSize` привязаны к id 245/362.
`message_ids.csv` не менялся: отчёты подтвердили все проверенные id/имена.

### Оставшиеся пробелы (97)

1. Незанятые значения enum — 84: 29, 54–60, 67, 79, 121, 123, 133–134, 137, 147–148, 151–160,
   166, 251–260, 267, 271, 305, 457–499. Члена в enum нет, текста нет нигде — пропуск в снимке верен.
2. `InternalErrorIds` 1–6 — 6: ErrInBlobLink, ErrInFindObjectsToTypify, ErrCodeDataLocationConflict,
   ErrReLinkError, ErrInRelocation, ErrInVirtualFunctionCall. Внутренние id, не локализуются;
   их номера пересекаются с `MessageId` 1–6, текст MessageId нельзя мапить на InternalErrorIds.
3. `MessageId` без текста в 3.5.22.10 — 7: 315 Wrn_StringTooShortForVarInOut, 349 Wrn_InterfaceInVarInOut,
   350 Wrn_ReferenceToInterface, 370 Wrn_InstanceCalledMoreThenOnce, 394 Wrn_FBExitCalledForStackInstance,
   404 Wrn_CompilerVersionDeprecated, 410 Wrn_CompatibilityProblemForRefProperty. Ни в одной из 10
   локалей 3.5.22.10 текста нет, raise-site/привязки в декомпиле нет; RU — только harvest из
   Compiler35200 (DLL нет в репозитории — не верифицируемо). У 315 RU совпадает с текстом id 418,
   у 394 удвоенные плейсхолдеры `{{0}}`/`{{1}}`, у 410 RU-текст английский.

Сверка с прежним списком «15 без текста» (`docs/03_ERROR_CATALOG.md` §f): 2 исправлены (245, 362),
3 подтверждены как harvest-only (200, 210, 223), 7 остались пробелом (список выше),
3 (508, 510, 523) вне диапазона этого прогона.

### Orphan-ключи (9 → 7)

Из 9 ключей ресурса вне enum `MessageId` два развязаны отчётами:
`Err_MissingObjectForPersistent` → id 245, `Wrn_InvalidStringSize` → id 362 (см. «Исправлено»).
Остаются 7 orphan-ключей без enum-id: Err_GenericNoInitialValueSupported,
Err_InconsistentUseOfCPPCompatibility_MissingParent, Err_RelatedPositionInterface,
Inf_RelatedPositionRecursion, Inf_RelatedPositionStackoverflow, Info_PersistentMemoryConfiguration,
PublishSymbolsMustBeSet.

Причина обеих «потерь» одна: каталог собран по именам enum-членов, а код привязывает id к
ресурсному ключу с другой приставкой (`Err_` ↔ `Wrn_`), поэтому реальные ключи попали в
orphan-строки CSV с пустым id.
