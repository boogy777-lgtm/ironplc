# LEXER-GAP-ANALYSIS — покрытие ST-синтаксиса: CODESYS 3.5.22.10 vs IronPLC

> **Назначение.** Готовая карта разрывов между эталонным парсером CODESYS (Parser35220.plugin,
> CODESYS 3.5.22.10) и нашими Rust-лексером/парсером. Файл предназначен для свежего LLM-агента:
> описаны **точные списки отсутствующих элементов**, доказательства `file:line` по обеим сторонам,
> приоритеты и метод проверки каждого закрытия — **без grep по репозиторию**.
>
> Наверх: [`README.md`](README.md) · Таблицы: [`tables/README.md`](tables/README.md) ·
> Грамматика: [`grammar/README.md`](grammar/README.md) · Отчёты: [`docs/README.md`](docs/README.md).
>
> **Цель задания:** лексер обязан иметь 100 % покрытие поверхностного синтаксиса ST CODESYS
> (без исключений вида «дойдём потом»); парсер/анализатор/кодоген-дыры собраны в отдельном
> разделе [§12 «За пределами лексера»](#12-за-пределами-лексера).

---

## 0. Метод, источники, легенда

**Эталон (CODESYS):**

| Источник | Что берём |
|---|---|
| `tables/st_keywords.csv` (77), `oo_keywords.csv` (8), `datatype_names.csv` (57) | инвентарь ключевых слов и типов |
| `tables/operator_symbols.csv` (30), `standard_operators.csv` (40), `special_operators.csv` (42), `ilo_operators.csv` (26), `vector_operators.csv` (13), `conversion_operators.csv` (5) | все таблицы операторов |
| `tables/token_types.csv` (28), `tables/operators.csv` (292) | модель токенов/операторов |
| `tables/reserved_unused_keywords.txt` (17), `tables/string_escapes.csv` (20 строк), `tables/string_size_eval.csv` (17) | резерв, escape-таблица, размеры XSTRING |
| `grammar/ST_GRAMMAR.ebnf`, `grammar/STRING_LITERALS.ebnf` | восстановленная грамматика |
| `docs/01_LEXER_PARSER.md`, `docs/15_STRING_LITERALS.md`, `docs/10_X_TYPES.md` | алгоритмы и обоснования |

**Наша сторона (IronPLC, `compiler/parser` и `compiler/dsl`):**

| Файл | Что там |
|---|---|
| `compiler/parser/src/token.rs` | `enum TokenType`, 172 варианта (`token.rs:63-519`) |
| `compiler/parser/src/lexer.rs` | logos-обёртка, построение span/line/col (`lexer.rs:24-97`) |
| `compiler/parser/src/xform_demote_keywords.rs` | единственная демоция keyword→Identifier (`:46-83`), контекстный `TIME` (`:125-147`) |
| `compiler/parser/src/parser.rs` | PEG-грамматика целиком (2200 строк) |
| `compiler/parser/src/options.rs` | `Dialect` + `--allow-*`/`--policy-*` (`:331-509`) |
| `compiler/parser/src/lib.rs` | конвейер `tokenize_program` (`:62-83`), `check_tokens` (`:86-108`) |
| `compiler/parser/src/rule_token_*.rs` | валидирующие правила (флаг-гейты) |
| `compiler/dsl/src/string_escape.rs` | единая таблица escape-ов `decode`/`encode`/`named_escape` |
| `compiler/dsl/src/textual.rs` | AST выражений/инструкций (`StmtKind` — `:869-886`) |

**Подсчёты** сделаны PowerShell-скриптом поверх CSV и `token.rs`:
`Import-Csv … | Where-Object { -not <regex '#\[token\("TEXT", ignore\(case\)\)' в token.rs> }`.
Все цифры в таблицах воспроизводимы этим методом.

**Статусы:**

| Статус | Значение |
|---|---|
| **ПОЛНОСТЬЮ** | поверхность поддержана; различия только в семантике — помечено отдельно |
| **ЧАСТИЧНО** | часть элементов отсутствует или работает только под флагом/диалектом |
| **ОТСУТСТВУЕТ** | не лексируется/не разбирается вовсе |
| **ВНЕ SCOPE** | признано нецелевым для IronPLC (обосновано) |

**Эмпирика.** Все спорные случаи прогнаны через уже собранный
`compiler/target/debug/ironplcc.exe check --dialect codesys <snippet>` (версия 0.246.0).
Журнал — [§15](#15-журнал-эмпирических-проверок). Прогоны подтверждают FAIL/OK ниже;
там, где в тексте «принято, но семантики нет», в журнале стоит P9999/P4017, а не P0002.

---

## 1. Сводная таблица

| # | Класс | CODESYS | Наш результат | Статус | Ключевые дыры (детали в §) |
|---|---|---|---|---|---|
| 1 | Ключевые слова ST (`st_keywords.csv`) | 77 | 62 токена (11 из них флаго-зависимы) | **ЧАСТИЧНО** | 15 отсутствуют: `PROPERTY/END_PROPERTY/PROPERTY_GET/PROPERTY_SET`, `UNION/END_UNION`, `CONTINUE`, `PARAMS`, `VAR_STAT`, `VAR_INST`, `VAR_GENERIC`, `__VECTOR`, `NAMESPACE/END_NAMESPACE`, `__BEGIN_IMPLEMENTATION` [§2](#2-класс-1-ключевые-слова-и-резерв) |
| 2 | OO-ключевые слова (`oo_keywords.csv`) | 8 | 1 (`ABSTRACT`) | **ЧАСТИЧНО** | `PUBLIC PRIVATE PROTECTED INTERNAL FINAL OVERRIDE OVERLOAD` [§2.3](#23-oo-ключевые-слова) |
| 3 | Символы операторов (`operator_symbols.csv`) | 30 | 25 лексем + 3 парсерных (`S=/R=/REF=`) | **ЧАСТИЧНО** | `\|` не лексируется; `=:` — internal (не нужен) [§3.1](#31-символы-operator_symbols30) |
| 4 | Стандартные операторы-функции (`standard_operators.csv`) | 40 | 31 в stdlib (2 из них под флагом) | **ЧАСТИЧНО** | `LOWER_BOUND UPPER_BOUND BITADR INDEXOF XSIZEOF INI TRUNC_INT` (+`ANDN/ORN` — IL) [§3.2](#32-стандартные-операторы-функции-standard_operators40) |
| 5 | Special `__*` (`special_operators.csv`) | 42 | 2 (`AND_THEN`, `OR_ELSE`) | **ОТСУТСТВУЕТ** | TRY-семейство (5), ST-visible (`__NEW`, `__DELETE`, `__ISVALIDREF`, `__SYSTEM`, `__POOL`, …) 15, internal ~20 [§3.3](#33-special-operators42) |
| 6 | ILO-операторы (`ilo_operators.csv`) | 26 | 5 ST-релевантных (`XOR NOT AND OR MOD`) | **ЧАСТИЧНО** (IL — вне scope) | `TEST_AND_SET`; IL как язык отсутствует целиком [§3.4](#34-ilo-операторы26) |
| 7 | Vector-операторы (`vector_operators.csv`) | 13 | 0 | **ВНЕ SCOPE** | `__VC*` + `__VECTOR` [§3.5](#35-vector-операторы13--вне-scope) |
| 8 | Конверсии (`conversion_operators.csv`) | 5 динамических префиксов | префиксы лексятся как Identifier; stdlib частичен | **ЧАСТИЧНО** (analyzer-уровень) | полнота каталога `X_TO_Y` [§3.6](#36-конверсии-conversion_operators5) |
| 9 | Имена типов (`datatype_names.csv`) | 57 | 36 | **ЧАСТИЧНО** | `BIT`, 16×`SAFE*`, `__XINT/__XWORD/__UXINT/__XSTRING` [§4](#4-класс-2-имена-типов-datatype_names57) |
| 10 | Модель токенов (`token_types.csv`) | 28 | 23 эквивалента | **ЧАСТИЧНО** | реальные дыры: `XByteString`, `DocComment` (остальные — иная модель: `Error`→P0003, `End/Unused/None` n/a) [§5](#5-класс-3-модель-токенов-token_types28) |
| 11 | Литералы | — | числа/строки/время/дата частично | **ЧАСТИЧНО** | `10#`, `BOOL#1/0`, `BIT#`, `__XSTRING#`/`UTF8#`/`UCHAR#`, `$U`+8hex, `us/ns`, `LT#/LD#`, `TOD#hh:mm` без секунд [§6](#6-класс-4-литералы) |
| 12 | Direct variables `%I/%Q/%M` | — | лексер+парсер полные | **ПОЛНОСТЬЮ** | partial-access `%X/%B/%W/%D/%L` — под `--allow-partial-access-syntax` [§7](#7-класс-5-direct-variables-iqm) |
| 13 | Прагмы и комментарии | — | прагмы — opaque trivia, C-style под флагом | **ЧАСТИЧНО** | нет вложенных комментариев, `DocComment`, вычисления `{IF}`, position-прагм [§8](#8-класс-6-прагмы-и-комментарии) |
| 14 | OO-синтаксис | — | METHOD/EXTENDS/IMPLEMENTS/ABSTRACT/THIS/SUPER; INTERFACE — только заголовок | **ЧАСТИЧНО** | `PROPERTY`, модификаторы доступа, члены INTERFACE, `__NEW/__DELETE`, `THIS^` в анализаторе = P9999 [§9](#9-класс-7-oo-синтаксис) |
| 15 | POU/секции/SFC | — | PROGRAM/FB/FUNCTION/METHOD/ACTION/TRANSITION + SFC-элементы | **ЧАСТИЧНО** | `NAMESPACE`, `PROPERTY`, `VAR_STAT/INST/GENERIC`, `PARAMS`, `UNION`, SFC не компилируется [§10](#10-класс-8-pou-секции-sfc) |
| 16 | Ссылки/указатели/partial access | — | `REF_TO`, `REFERENCE TO`, `POINTER TO`, `REF=`, `^`, `%X…` | **ПОЧТИ ПОЛНОСТЬЮ** | `ARRAY[*]`; `S=`/`R=` только без пробела (как в CODESYS) [§11](#11-класс-9-partial-access-ссылки-указатели-массивы) |
| 17 | X-типы / конверсии / ILO / vector | — | см. §12 | **ВНЕ SCOPE** (по решению задания) | `__XINT` и др. — при необходимости портировать по `docs/10_X_TYPES.md` [§12](#12-за-пределами-лексера) |

Базовые числовые ориентиры: `TokenType` у нас — 172 варианта против 28 категорий CODESYS
(у CODESYS один `Operator(15)`, у нас — конкретные типы; это не дыра, а иная модель).

---

## 2. Класс 1: Ключевые слова и резерв

### 2.1 Инвентарь

CODESYS `tables/st_keywords.csv` — 77 позиций (`ACTION` … `END_NAMESPACE`), плюс отдельные
категории `datatype_names.csv` (57), `oo_keywords.csv` (8), `reserved_unused_keywords.txt` (17).
Источник истины по таблицам — [`docs/01_LEXER_PARSER.md`](docs/01_LEXER_PARSER.md) §3.12/§6
(`AddKeywords` = 77, `AddOOKeywords` = 8, `AddDataTypeNames` = 57).

### 2.2 Чего нет у нас из `st_keywords.csv` (15)

| Элемент | CODESYS-источник | Уровень | Куда добавлять (наш код) | Проверка |
|---|---|---|---|---|
| `CONTINUE` | `st_keywords.csv:27`; `ST_GRAMMAR.ebnf:301` | lexer+parser+codegen | `token.rs` (рядом с `Exit`, `:184`), демоция в `xform_demote_keywords.rs:57-76`; правило `statement()` `parser.rs:2011`; `StmtKind` `dsl/textual.rs:869-886` | FAIL `continue_stmt` [§15](#15-журнал-эмпирических-проверок); тест `tests/dialect_flags.rs`/новый файл `tests/continue.rs` |
| `PROPERTY`, `END_PROPERTY`, `PROPERTY_GET`, `PROPERTY_SET` | `st_keywords.csv:66-69`; `ST_GRAMMAR.ebnf:126-129` | lexer+parser+AST | токены после `Method` (`token.rs:227-230`); правило рядом с `method_declaration()` (`parser.rs:1515`); AST в `dsl/` | FAIL `property` [§15](#15-журнал-эмпирических-проверок) |
| `UNION`, `END_UNION` | `st_keywords.csv:21,45`; `ST_GRAMMAR.ebnf:247` | lexer+parser+AST | токены после `Struct/EndStruct` (`token.rs:295-298`); тип-правило после `structure_type_declaration__with_constant()` (`parser.rs:819-856`) | FAIL `union_type` [§15](#15-журнал-эмпирических-проверок) |
| `PARAMS` | `st_keywords.csv:34`; `ST_GRAMMAR.ebnf:240` | lexer+parser+AST | токен; тип-правило `PARAMS (expr) OF type` рядом с `array_subranges()` (`parser.rs:804`) | FAIL `params` [§15](#15-журнал-эмпирических-проверок) |
| `VAR_STAT` | `st_keywords.csv:60`; `ST_GRAMMAR.ebnf:172` | lexer+parser+AST | токен + блок по образцу `temp_var_decls()` (`parser.rs:1589`) | FAIL `var_stat` [§15](#15-журнал-эмпирических-проверок) |
| `VAR_INST` | `st_keywords.csv:74`; `ST_GRAMMAR.ebnf:172` | lexer+parser+AST | то же; семантика — «статические члены FB» | FAIL `var_inst` [§15](#15-журнал-эмпирических-проверок) |
| `VAR_GENERIC` | `st_keywords.csv:75`; `ST_GRAMMAR.ebnf:172` | lexer+parser+AST | то же; связан с generic-типами (`Type<T>`, `ST_GRAMMAR.ebnf:242`) | FAIL `var_generic` [§15](#15-журнал-эмпирических-проверок) |
| `NAMESPACE`, `END_NAMESPACE` | `st_keywords.csv:77-78`; `ST_GRAMMAR.ebnf:135` | lexer+parser+AST | токены; правило уровня `pouUnit` (`parser.rs:394-401`) | FAIL `namespace` [§15](#15-журнал-эмпирических-проверок) |
| `__BEGIN_IMPLEMENTATION` | `st_keywords.csv:76`; `ST_GRAMMAR.ebnf:304` | lexer+parser | токен; `implementationBlock` в `statement()` | FAIL `begin_impl` [§15](#15-журнал-эмпирических-проверок) |
| `__VECTOR` | `st_keywords.csv:4`; `ST_GRAMMAR.ebnf:241` | lexer+parser | см. [§3.5](#35-vector-операторы13--вне-scope) — **ВНЕ SCOPE** | FAIL `vector` [§15](#15-журнал-эмпирических-проверок) |

### 2.3 OO-ключевые слова

Отсутствуют 7 из 8 (`oo_keywords.csv:2-9`): `PUBLIC`, `PRIVATE`, `PROTECTED`, `INTERNAL`,
`FINAL`, `OVERRIDE`, `OVERLOAD`. У нас есть только `ABSTRACT` (`token.rs:225-226`, гейт
`allow_fb_inheritance`). Подтверждение: FAIL `access_modifier` [§15](#15-журнал-эмпирических-проверок).

* `PUBLIC/PRIVATE/PROTECTED/INTERNAL/FINAL` — `accessModifier` в `ST_GRAMMAR.ebnf:110-111`;
* `OVERRIDE`/`OVERLOAD` — контекстные (`oo_keywords.csv:8-9`, флаг `Contextual`);
* добавление — токены + демоция (иначе ломаются идентификаторы `public` и т. п.) по образцу
  `xform_demote_keywords.rs:46-83`; тест — `tests/fb_inheritance.rs` (leverage) или новый файл.

### 2.4 Резерв (`reserved_unused_keywords.txt`, 17)

CODESYS **запрещает** эти слова как идентификаторы (`ReservedUnusedKeywords`, см.
`docs/01_LEXER_PARSER.md:240-241`), но не делает их ключевыми токенами:
`CHAR WCHAR ANY_DERIVED ANY_ELEMENTARY ANY_MAGNITUDE ANY_SIGNED ANY_DURATION ANY_CHARS
ANY_CHAR CHAR_TO TO_CHAR WCHAR_TO TO_WCHAR ATAN2 USING CLASS NAMESPACE`.

У нас: `ANY_DERIVED/ANY_ELEMENTARY/ANY_MAGNITUDE` — **токены** (`token.rs:424-429`), т. е.
запрещены даже сильнее; остальные 14 — обычные Identifier, т. е. **разрешены** там, где CODESYS
запрещает (`CLASS`, `CHAR`, `USING`, `ATAN2`, …). Это расхождение в «разрешающую» сторону;
для 1:1 нужен separate reserved-set (validation на Identifier), а не токены. Приоритет P2.

### 2.5 Флаго-зависимые ключевые слова (11 из 62 присутствующих)

`PERSISTENT` (`allow_persistent_var`, `options.rs:387-390`), `REFERENCE` (`allow_reference_to`,
`:372-375`), `POINTER` (`allow_pointer_to`, `:377-380`),
`EXTENDS IMPLEMENTS INTERFACE END_INTERFACE ABSTRACT METHOD END_METHOD THIS SUPER`
(`allow_fb_inheritance`, `:482-485`). Демоция — `xform_demote_keywords.rs:57-76`.

Под `--dialect codesys` активны все 11 (эмпирика: `ref_bind`, `reference_to`, `pointer_to`,
`long_dt_types` — OK). Сверх `st_keywords.csv` у нас есть «настоящие» IEC-ключевые слова
конфигурации/SFC, которых нет в таблице CODESYS-ST: `CONFIGURATION/END_CONFIGURATION`,
`RESOURCE/END_RESOURCE`, `TASK/END_TASK`, `ON`, `WITH`, `EN`, `ENO`, `STEP/INITIAL_STEP/END_STEP`,
`F_EDGE/R_EDGE`, `NON_RETAIN` — это не дыры (эти элементы CODESYS обрабатывает другими
компонентами, см. `docs/01_LEXER_PARSER.md` §5).

---

## 3. Класс 2: Операторы (все таблицы)

### 3.1 Символы (`operator_symbols.csv`, 30)

| Символ | CODESYS | У нас |
|---|---|---|
| `+ - * ** / . # : := ( ) [ ] , ; .. => < > <= >= = <> & ^` | 25 символов | **есть** (`token.rs:84-518`); `**` — `Power` (`token.rs:507`, у CODESYS `internal/Declaration` — мы даже шире) |
| `S=` `R=` `REF=` | одиночные Operator-токены | **парсер-уровень**: `set_bind_op`/`reset_bind_op`/`ref_bind_op` (`parser.rs:1240-1252`) — работают только при слитном написании, как и в CODESYS; `REF=` под `allow_reference_to` |
| `\|` | `operator_symbols.csv:30` (OR) | **ОТСУТСТВУЕТ**: нет ни токена, ни альтернативы — FAIL `pipe_or` P0003 [§15](#15-журнал-эмпирических-проверок) |
| `=:` | `operator_symbols.csv:11`, `Internal` | не нужен (внутренний оператор компилятора CODESYS, `ST_GRAMMAR.md:280` помечает как internal) |

**Действие:** добавить `|` как `Or`-вариант (`#[token("|")]` рядом с `And`/`&`, `token.rs:475-477`)
и альтернативу в `orExpr` (`parser.rs:1910`). Проверка: `b := a | b;` + plc2plc round-trip.
Замечание: `ST_GRAMMAR.ebnf:342` описывает `orExpr` только через ключевые слова — `|` может
понадобиться только для совместимости с таблицей; при 1:1 ориентироваться на живой CODESYS-сканер.

### 3.2 Стандартные операторы-функции (`standard_operators.csv`, 40)

У CODESYS это **операторы** (`PrefixedOperatorParser`, `ST_GRAMMAR.ebnf:372-384`); у нас —
обычные вызовы функций по Identifier (`function_expression()`, `parser.rs:1998-2004`).
Лексических дыр нет; дыры — в analyzer-stdlib (`compiler/analyzer/src/intermediates/stdlib_function.rs`).

| Есть в stdlib | Отсутствуют | Примечание |
|---|---|---|
| `ADR`* , `SIZEOF`* , `ABS`, `LIMIT`, `MIN`, `MAX`, `TRUNC`, `MUX`, `SEL`, `ROL`, `ROR`, `SHL`, `SHR`, `EXP`, `EXPT`, `SQRT`, `LN`, `LOG`, `SIN`, `COS`, `TAN`, `ASIN`, `ACOS`, `ATAN`, `ADD`, `SUB`, `MUL`, `DIV`, `MOD`, `AND`, `OR` = **31** | `LOWER_BOUND`, `UPPER_BOUND`, `BITADR`, `INDEXOF`, `XSIZEOF`, `INI`, `TRUNC_INT`, `ANDN`*, `ORN`* = **9** | `*` — `ANDN/ORN` в CODESYS относятся только к IL; `ADR`/`SIZEOF` гейтятся `--allow-adr`/`--allow-sizeof` (`options.rs:382-385,412-415`) |

Также у нас есть дополнительный `ATAN2` (сверх таблицы — не дыра). При закрытии очередного
имени: подпись — одна строка в таблице `stdlib_function.rs` (механизм — см. skill/`keyword-function-forms.md`),
тест — analyzer spec (`#[spec_test(REQ-…)]`) + e2e. Эмпирика: `size_of` OK, `adr` OK
(`ADR(x)` возвращает указатель; в журнал не включён, т. к. требует корректного типа-приёмника).

### 3.3 Special operators (42)

Из 42 (`special_operators.csv`) поддержаны только `AND_THEN`/`OR_ELSE` (`token.rs:481-484`).
Все 40 `__*` отсутствуют; `grep` по `compiler/**` не находит ни `__NEW`, ни TRY-семейства.

| Группа | Список | Уровень | Приоритет |
|---|---|---|---|
| ST-ключевые слова (Try/Catch) | `__TRY` `__ENDTRY` `__CATCH` `__FINALLY` `__THROW` (`special_operators.csv:26-30`, `ST_GRAMMAR.ebnf:307-311`) | lexer+parser+codegen | P0 |
| ST-visible (AllLanguages), реально встречаются | `__NEW` `__DELETE` `__QUERYINTERFACE` `__QUERYPOINTER` `__ISVALIDREF` `__SYSTEM` `__POOL` `__TYPEOF` `__CURRENTTASK` `__POUNAME` `__POSITION` `__XADD` `__COMPARE_AND_SWAP` `__MEMORYBARRIER` `__CHECKLICENSE` `__CHECKLICENSEBIT` `__WAIT` (`special_operators.csv:8,10-11,14,21-25,32-37,40-41`) | lexer+parser+analyzer | P1 (кроме `__WAIT`, `__POUNAME`, `__POSITION` — P2) |
| Internal | `__COPY` `__RELOC` `__LAZY` `__CRC` `__MAXOFFSET` `__LOCALOFFSET` `__VARINFO` `__INIT` `__CAST` `__FCALL` `__PROPERTYINFO` `__ADRINST` `__REFADR` `__MEMORYSET` `__GETLTICK` `__BITOFFSET` `__CALLINITFUNCTION` `__LATECOMPILEDEXPR` (`flags=Internal`) | — | **ВНЕ SCOPE** |

Эмпирика: `__XSTRING#"abc"` → P0002 (лексер), TRY-семейство и `__NEW` не тестировались
отдельно, т. к. токенов заведомо нет. Для P0: токены (`ignore(case)`!), демоция под
CODESYS/TwinCAT-флаг, правила `tryCatchStatement`/`throwStatement`, AST и кодоген.

### 3.4 ILO-операторы (26)

ST-релевантны 5: `XOR`, `NOT`, `AND`, `OR`, `MOD` — у нас есть как токены (`token.rs:471-512`)
и как функции (`keyword-function-forms.md`). `MOVE` — есть в stdlib. `TEST_AND_SET`
(`ilo_operators.csv:25`, флаги `StructuredText`) — **ОТСУТСТВУЕТ**. Остальные 19 (`XORN`,
`EQ/NE/GE/GT/LE/LT`, `CAL/CALC/CALCN`, `JMP/JMPC/JMPCN`, `RET/RETC/RETCN`, `LD/LDN/ST/STN`,
`R/S`) — мнемоники IL; язык IL у нас не реализован вовсе (`parser.rs:1904-1905`: «B.2.1
Instruction List — TODO this entire section»). Помечаем **IL — вне scope для ST-лексера**.

### 3.5 Vector-операторы (13) — вне scope

`__VCLOAD_REAL/LREAL`, `__VCSTORE`, `__VCSET_REAL/LREAL`, `__VCADD/SUB/MUL/DIV/DOT/SQRT/MAX/MIN`
(`vector_operators.csv`) + тип `__VECTOR[n] OF T` (`ST_GRAMMAR.ebnf:241`). В IronPLC нет ни
поэлементной математики, ни векторного типа; экспериментально `vector` → P0002. **ВНЕ SCOPE**
(зафиксировать явно, чтобы не считалось дырой при аудите «100 % синтаксиса»: это расширение
CODESYS, а не ST-поверхность общего назначения).

### 3.6 Конверсии (`conversion_operators.csv`, 5)

Префиксы `_TO_`, `ANY_NUM_TO_`, `ANY_TO_`, `TO_`, `<T>_TO_<U>` — динамические; у нас
лексируются как идентификаторы и разбираются как обычные вызовы (`INT_TO_REAL` — corpus,
`tests/corpus.rs:105`). Дыра чисто семантическая: каталог пар неполон
(см. `compiler/analyzer/src/intermediates/stdlib_function.rs`; есть `DINT_TO_WORD`, но нет,
например, `DINT_TO_UDINT`; нет `SINT_TO_INT`). Метод закрытия и правила — таблица
конверсий + `specs/design/type-conversion-stdlib.md`. Лексер не трогается.

---

## 4. Класс 2: Имена типов (`datatype_names.csv`, 57)

Есть (36): `BOOL`, `SINT..ULINT`, `BYTE/WORD/DWORD/LWORD`, `REAL/LREAL`, `STRING/WSTRING`,
`TIME/LTIME`, `DATE/LDATE`, `TIME_OF_DAY/TOD/LTIME_OF_DAY/LTOD`,
`DATE_AND_TIME/DT/LDATE_AND_TIME/LDT`, все 7 `ANY*`.

Отсутствуют (21):

| Элемент | CODESYS-источник | Примечание |
|---|---|---|
| `BIT` | `datatype_names.csv:26`; `elementaryType` `ST_GRAMMAR.ebnf:199` | тип-бит; нужен и для `BIT#0/1` |
| 16× `SAFE*` | `datatype_names.csv:9-24` (`SAFEBOOL … SAFETIME`, `SAFEREAL`, `SAFELREAL`) | Safety-расширение; P2/ВНЕ SCOPE |
| `__XINT`, `__XWORD`, `__UXINT` | `datatype_names.csv:39-41` | portability-маркеры, порт по `docs/10_X_TYPES.md`; P2 |
| `__XSTRING` | `datatype_names.csv:46` | строковый X-тип (`__XSTRING(n)`); связан с литералом `__XSTRING#…` — см. §6; P0 для литерала, P2 для типа |

У нас `elementary_type_name()` (`parser.rs:610-624`) знает только список выше и не знает `BIT`.
Проверка: тест типа `VAR x : BIT;` + (для X-типов) таблица из `docs/10_X_TYPES.md`.

---

## 5. Класс 3: Модель токенов (`token_types.csv`, 28)

У CODESYS — 28 категорий (`== 292` нет: отдельно `operators.csv`). Отображение:

| TokenType CODESYS | У нас | Статус |
|---|---|---|
| `None 0` | — | n/a |
| `Boolean 1` | `True`/`False` | OK |
| `Comment 2` | `Comment` (`token.rs:79`) | OK |
| `DocComment 3` | — | **ОТСУТСТВУЕТ** (нет отдельного типа; `///` и `(**` идут как обычный Comment) |
| `Pragma 4` | `Pragma` (`token.rs:95`) | OK, но только после `xform_collapse_pragmas` |
| `Date 5`, `DateAndTime 6`, `TimeOfDay 18`, `LDate 23`, `LTimeOfDay 24`, `LDateAndTime 25` | токены `Date/DateAndTime/TimeOfDay/Ldate/Ltod/Ldt` + правила `parser.rs:581-601` | OK (лексер режет литерал на несколько токенов, парсер собирает) |
| `DirectVariable 7` | `DirectAddress` (`token.rs:445`) | OK |
| `IncompleteDirectVariable 8` | `DirectAddressIncomplete` (`token.rs:443`) | OK |
| `DoubleByteString 9` | `DoubleByteString` (`token.rs:123`) | OK |
| `Duration 10`, `LDuration 11` | `Time`/`Ltime` + `Hash` + интервалы (`parser.rs:541-578`) | OK (нет отдельного токена — иного и не нужно) |
| `EndOfLine 12` | `Newline` (`token.rs:65-68`) | OK |
| `Identifier 13` | `Identifier` (`token.rs:128`) | OK |
| `Integer 14` | `Digits`/`HexDigits`/`OctDigits`/`BinDigits` (`token.rs:132-149`) | OK |
| `Operator 15` | конкретные токены | OK |
| `Real 16` | `FloatingPoint`/`FixedPoint` (`token.rs:139-144`) | OK |
| `SingleByteString 17` | `SingleByteString` (`token.rs:121`) | OK |
| `Whitespace 19` | `Whitespace` (`token.rs:70`) | OK; CODESYS также `\v`/`\u00A0` — расхождение P3 |
| `Error 20` | — (logos отдаёт `Err` → Diagnostic P0003, `lexer.rs:75-92`) | функциональный эквивалент; отдельного токена нет |
| `End 21` | — | n/a (EOF без токена) |
| `XByteString 22` | — | **ОТСУТСТВУЕТ** (см. `__XSTRING#` в §6) |
| `Unused 26` | — | n/a |
| `PartialAccess 27` | 5 вариантов `PartialAccess*` (`token.rs:451-468`) | OK |

---

## 6. Класс 4: Литералы

### 6.1 Целые

| Элемент | CODESYS | У нас | Статус |
|---|---|---|---|
| decimal, `_`-разделители | `decInteger` (`ST_GRAMMAR.ebnf:59`) | `Digits` (`token.rs:148`), `Integer::new` | OK |
| `2#`, `8#`, `16#` | `basedInteger` (`:60-62`) | `BinDigits/OctDigits/HexDigits` (`token.rs:132-137`) | OK |
| **`10#`** | база `10` (`ST_GRAMMAR.ebnf:61`) | нет | **ДЫРА**: FAIL `based10` [§15](#15-журнал-эмпирических-проверок). Фикс: `#[regex(r"10#[0-9][0-9_]*")]` → `DecDigits` или расширение `Digits`-ветки в `integer_literal()` (`parser.rs:458`) |
| typed `SINT#..ULINT#` | `typedIntegerLiteral` (`:66`) | `integer_literal_type()` (`parser.rs:449-457`) | OK (эмпирика `int_typed_neg` OK) |
| `BIT#`/`BOOL#0/1` | `typedBooleanLiteral` (`:68`) | `BIT` нет, `BOOL#1` FAIL | **ДЫРА**: правило `boolean_literal()` (`parser.rs:501-508`) требует `id_eq("1")` (тип `Identifier`), но `1` лексируется как `Digits` → не матчится. Фикс: матчить `Digits` c текстом «0»/«1» либо добавить `Bit`-тип (`BIT#`) |

### 6.2 Вещественные

`decInteger (. decInteger [exp] | exp)` (`ST_GRAMMAR.ebnf:63-64`) — у нас `FloatingPoint`
(обязателен exp) / `FixedPoint` (`token.rs:139-144`) + typed `REAL#/LREAL#` (`parser.rs:469-485`).
Покрытие полное; `REAL#1.5`, `2E-3`, `1.5E+2` — OK (тесты `literals.rs:63-140`).

### 6.3 Строки (`STRING_LITERALS.ebnf`, `string_escapes.csv`)

| Форма | CODESYS | У нас | Статус |
|---|---|---|---|
| `'…'` → `SingleByteString` | `:58` | `token.rs:121` | OK |
| `"…"` → `DoubleByteString` | `:62` | `token.rs:123` | OK |
| `STRING#'…'`, `WSTRING#"…"` | **Error(20)** (`docs/15` §6.3) | **поддержано** (`parser.rs:517-531`, тесты `literals.rs:293-315`) | расхождение в разрешающую сторону; при 1:1 — осознанно оставить/задокументировать |
| `__XSTRING#"…"` → `XByteString(22)` | `STRING_LITERALS.ebnf:131`; `docs/15` §4 | — | **ДЫРА** (эмпирика `xstring` P0002); только двойная кавычка, одинарная → Error в эталоне |
| `UTF8#'…'`, `UCHAR#'…'` | `STRING_LITERALS.ebnf:138,145` | — | **ДЫРА** (`utf8`, `uchar` → P0002) |
| escape `$$ $' $" $L/l $N/n $R/r $T/t $P/p` | `string_escapes.csv:2-14` | все есть (`dsl/string_escape.rs`, `named_escape`) | OK |
| `$hh` (2 hex, узкие) / `$hhhh` (4 hex, широкие) | `:15-16` | есть (`hex_digit_count`, decode) | OK |
| **`$U` + ровно 8 hex (UTF-32)** | `:17` | — | **ДЫРА**: `named_escape` не знает `U/u` → P0012. Фикс — в `dsl/string_escape.rs` (`decode`/`encode`): 8 hex, `char::from_u32`, ошибка при суррогате/нехватке (эталон: `<8` → Error) |
| cp1252 для `$80-$FF` | `:15` (`EscapeLocalEncodingCodepoint`) | Latin-1 напрямую (`char::from_u32`) | **расхождение декодирования**: `$80` у CODESYS = `€` (U+20AC), у нас U+0080. Нужна явная cp1252-таблица (`tables/cp1252.csv`) |
| многострочность (CR/LF внутри) | да (`docs/15` §2) | да (`[^'$]` матчит `\n`) | OK |
| сдвоение `''` внутри строки | нет (два литерала) | нет (regex `token.rs:121`) | OK |

Правило валидации escape — `rule_token_string_escape.rs:19-63` (P0012, help-текст).

### 6.4 Time/Date литералы

| Элемент | CODESYS | У нас | Статус |
|---|---|---|---|
| префиксы `T#/TIME#/LTIME#` | `ST_GRAMMAR.ebnf:79` | `duration_prefix()` (`parser.rs:556`) | OK |
| **`LT#`** | `LT` в списке префиксов (`:79`) | нет (`dt_sep("T")` не матчит `LT`) | **ДЫРА**: FAIL `lt_shorthand` |
| единицы `d h m s ms` | `:80` | `duration_unit()` (`parser.rs:566-571`) | OK |
| **`us`, `ns`** | `:80` | нет; задокументировано как отказ (REQ-TL-010, `specs/design/time-literals.md:66`) | **ДЫРА для 1:1**: эмпирика `dur_us`/`dur_ns` FAIL |
| `D#/DATE#/LDATE#` | `:81` | `date_prefix()` (`parser.rs:590`) | OK |
| **`LD#`** | `"LD"` в списке (`:81`) | нет | **ДЫРА**: FAIL `ld_shorthand` |
| `TOD#/LTOD#/TIME_OF_DAY#/LTIME_OF_DAY#` | `:82` | `time_of_day_prefix()` (`parser.rs:582`) | OK |
| **`TOD#hh:mm` без секунд** | `[":" ss ["." nnn]]` (`:82`) | `daytime()` требует `h:m:s` (`parser.rs:583-585`) | **ДЫРА**: FAIL `tod_no_seconds` |
| `DT#/LDT#/DATE_AND_TIME#/LDATE_AND_TIME#` | `:83` | `date_and_time_prefix()` (`parser.rs:601`) | OK |
| знак `T#-5s` | да | да (`parser.rs:541`) | OK |

---

## 7. Класс 5: Direct variables `%I/%Q/%M`

**ПОЛНОСТЬЮ** на уровне лексера+парсера:

* `DirectAddress` — `#[regex(r"%[IQM]([XBWDL])?(\d(\.\d)*)", ignore(case))]` (`token.rs:445`);
  `%IX0.0`, `%QW3`, `%MB10`, `%MD2` — OK; `%I*`/`%Q*`/`%M*` → `DirectAddressIncomplete`
  (`token.rs:443`, `incompl_location()` `parser.rs:1457-1459`).
* Парсер: `direct_variable()` (`parser.rs:1077-1079`) → `AddressAssignment`; используется в
  `AT`-декларациях (`location()`, `:1387`), `variable()` (`:991-993`), конфигурациях.
* Partial access: `.%X3` (bit) / `.%B3` / `.%W3` / `.%D3` / `.%L3` (`token.rs:451-468`,
  `symbolic_variable_element()` `parser.rs:1014-1020`), гейт — `rule_token_no_partial_access_syntax`
  + `--allow-partial-access-syntax` (`options.rs:442-445`); спека — `specs/design/partial-access-bit-syntax.md`,
  тесты — `tests/partial_access.rs`.
* Семантика образов процесса (ADR-0071, `specs/adrs/0071-iocycle-owned-process-image-with-effect-gated-output-flush.md`) —
  вне лексера; лексер покрыт.

Единственное расхождение: CODESYS выделяет `%B2` и т. п. в отдельный `PartialAccess(27)` **в любом**
месте; у нас они принимаются после `.` (что и есть осмысленный синтаксис) и под флагом. Действие:
не требуется (иначе `%B`-последовательность станет неотличима от адреса).

---

## 8. Класс 6: Прагмы и комментарии

| Элемент | CODESYS | У нас | Статус |
|---|---|---|---|
| `{ … }` → `Pragma(4)` | `docs/01` §3.9 | `LeftBrace`/`RightBrace` → `xform_collapse_pragmas` → `Pragma` (`token.rs:88-95`), пропуск как trivia (`parser.rs:372-373`) | ЧАСТИЧНО: прагма непрозрачна, `{attribute …}` «работает» только потому, что вырезается |
| условная компиляция `{IF} {ELSIF} {ELSE} {END_IF}`, `{DEFINE}`, `{UNDEFINE}`, `{ERROR}`, … | `Pragmas/PragmaIfStatementParser.cs`; `ST_GRAMMAR.ebnf:318-330` | нет вычисления (эмпирика `pragma_if` OK — обе ветви просто пропущены как прагмы) | **ДЫРА** (P1): нужен препроцессор условных прагм |
| `{position:=N}`, `{autoincrementpositiononlinebreaks}` | `docs/01` §3.9 | нет | P2 |
| `(* … *)`, вложенность | `ScanComment`, `AllowNestedComments` (`docs/01` §3.8) | regex без вложенности (`token.rs:73`); `nested_comment` FAIL | **ДЫРА** (P1): при `allow_nested_comments` — счётчик глубины |
| `//` | `Comment(2)` | есть, но под `--allow-c-style-comments` + правило P0004 (`rule_token_no_c_style_comment.rs`) | OK (гейт-политика сходится) |
| `/* … */` | нет в CODESYS! | есть под тем же флагом | не дыра (наше расширение) |
| `///` → `DocComment(3)` | `docs/01` §3.8 | как обычный Comment | **ДЫРА** (P2; влияет только на документацию/LS) |
| `(** … *)` doc | `ST_GRAMMAR.ebnf:101` | как обычный Comment | P2 |
| OSCAT-комментарии `(*@KEY@:…*)` | нет | вырезает `preprocessor.rs:19-66` | наше расширение |

---

## 9. Класс 7: OO-синтаксис

| Элемент | CODESYS | У нас | Статус |
|---|---|---|---|
| `FUNCTION_BLOCK … EXTENDS`, `IMPLEMENTS` список, `ABSTRACT` | `ST_GRAMMAR.ebnf:116-119` | `function_block_declaration()` (`parser.rs:1529-1575`) | OK (гейт `allow_fb_inheritance`) |
| `METHOD … END_METHOD` | `:123-125` | `method_declaration()` (`parser.rs:1515-1527`), тесты `tests/methods.rs` | OK |
| `PROPERTY` + `PROPERTY_GET/PROPERTY_SET` | `:126-129` | — | **ОТСУТСТВУЕТ** (P0) |
| `INTERFACE … END_INTERFACE` с членами | `:130-132` | только заголовок (`parser.rs:1581-1586`; комментарий там же: «method/property signatures are not yet supported»); `interface_method` FAIL | ЧАСТИЧНО (P1) |
| `THIS^` / `SUPER^` | `:364` | токены + `self_ref()` (`parser.rs:1004-1010`); анализатор: P9999 (`xform_resolve_expr_types.rs:736`, `rule_unsupported_extension.rs:61`) | ЧАСТИЧНО: парсер OK, семантики нет (P1) |
| `__NEW` / `__DELETE` | `ST_GRAMMAR.ebnf:389` | — | **ОТСУТСТВУЕТ** (P1) |
| модификаторы доступа `PUBLIC/PRIVATE/…`, `OVERRIDE`, `OVERLOAD`, `FINAL` | `oo_keywords.csv`; `:110-111` | — | **ОТСУТСТВУЕТ** (P0 токены, P1 семантика) |
| `namespace`-доступ `IDENT#name` в постфиксе | `ST_GRAMMAR.ebnf:397` | `#` поддержан только в typed-литералах/enum | связано с `NAMESPACE` (P0 токены, P1 семантика) |

---

## 10. Класс 8: POU, секции, SFC

| Элемент | CODESYS | У нас | Статус |
|---|---|---|---|
| `PROGRAM`/`FUNCTION`/`FUNCTION_BLOCK` | `ST_GRAMMAR.ebnf:113-122` | `parser.rs:1478-1632` | OK |
| `ACTION`, `TRANSITION` | как отдельные POU (`:133-134`), вложенность по `s_htValidChildPouTypes` (`:136-141`) | только как SFC-элементы (`parser.rs:1664-1733`); top-level `ACTION`/`TRANSITION` как POU нет | ЧАСТИЧНО (P1) |
| `NAMESPACE` | `:135` | — | **ОТСУТСТВУЕТ** (P0 токены) |
| `PROPERTY` | `:126-129` | — | **ОТСУТСТВУЕТ** (P0) |
| Секции `VAR/VAR_INPUT/VAR_OUTPUT/VAR_IN_OUT/VAR_TEMP/VAR_EXTERNAL/VAR_GLOBAL/VAR_ACCESS/VAR_CONFIG` | `varKeyword` `:162-172` | `parser.rs:1104-1467,1588-1594,1644` | OK |
| `VAR_STAT`, `VAR_INST`, `VAR_GENERIC` | `:171-172` | — | **ОТСУТСТВУЕТ** (P0) |
| Квалификаторы `CONSTANT/RETAIN/PERSISTENT/READ_ONLY/READ_WRITE` | `:173-177` | `Constant`, `Retain`, `NonRetain`, `Persistent` (`options.rs:387-390`), `ReadOnly/ReadWrite` (direction) | OK; `READ_ONLY/READ_WRITE` только в VAR_ACCESS-направлении — в CODESYS это var-модификаторы (P2) |
| SFC: `INITIAL_STEP/STEP/END_STEP/TRANSITION/FROM/TO/ACTION/END_ACTION`, квалификаторы действий, `PRIORITY:=` | OO/ST-грамматика SFC не описана в `ST_GRAMMAR.ebnf` (SFC — отдельный компонент) | `parser.rs:1656-1733`; тесты `tests/sfc.rs` | OK для парсера; **codegen SFC → `not_implemented`** (`codegen/src/compile_stmt.rs:52`) — P1 |
| `PARAMS(n) OF T` | `:240` | — | **ОТСУТСТВУЕТ** (P0) |
| `UNION` | `:247` | — | **ОТСУТСТВУЕТ** (P0) |
| пустая инструкция `;` | `:276` | `statements_or_empty` (`parser.rs:2010`) | OK |

---

## 11. Класс 9: Partial access, ссылки, указатели, массивы

| Элемент | CODESYS | У нас | Статус |
|---|---|---|---|
| `REF_TO T` (Ed3) | `referenceType` `:237` | `ref_to_keyword()` (`parser.rs:1227-1230`), спека `specs/design/ref-to.md`, тесты `tests/reference_to.rs` | OK |
| `REFERENCE TO T` (TwinCAT/CODESYS) | `:237` | то же, `RefSyntax::ReferenceTo`, гейт `allow_reference_to` | OK |
| `POINTER TO T`, разыменование `^` | `pointerType` `:236` | `RefSyntax::PointerTo` + `Caret` (`parser.rs:1025,1953`), спека `adr-and-pointer-to.md`, тесты `tests/pointer_to.rs` | OK |
| `REF(x)`, `NULL` | `:363,253-255` | `ExprKind::Ref`/`Null` (`parser.rs:1974-1979`), гейт `allow_ref_to` | OK |
| `REF=`, `S=`, `R=` | `operator_symbols.csv:12-14` | `parser.rs:2019-2054` (слитное написание) | OK |
| `AT %…`, `AT %I*` | `:180,89-92` | `location()/incompl_location()` | OK |
| `.%Xn`, `.%Bn`, `.%Wn`, `.%Dn`, `.%Ln` | `PartialAccess(27)` | под `--allow-partial-access-syntax` | OK |
| **`ARRAY[*]`** (незавершённый массив) | `arrayType = "ARRAY" "[" ("*" \| ranges) "]"` (`:234`) | `array_subranges()` требует `ranges` (`parser.rs:804`); `array_star` FAIL | **ДЫРА** (P0): добавить альтернативу `*` + AST-маркер |
| `ARRAY[a..b, c..d] OF T` | `:234-235` | OK (`array_2d` OK) | OK |
| `STRING[n]` / `STRING(n)` / `__XSTRING(n)` | `:231-233` | `[n]` и `(n)` — OK (`string_length_spec()`, `parser.rs:1431-1433`); `__XSTRING(n)` — нет (см. §4) | ЧАСТИЧНО |

---

## 12. За пределами лексера

Короткий реестр дыр **не-лексического** уровня (чтобы не смешивать с §2-§11):

1. **SFC не исполняется**: `codegen/src/compile_stmt.rs:52` → `Diagnostic::not_implemented`
   (парсер и AST есть).
2. **`THIS^`/`SUPER^`**: парсер строит `SelfRefVariable`, анализатор отвечает P9999
   (`analyzer/src/xform_resolve_expr_types.rs:736`, `rule_unsupported_extension.rs:61`).
3. **`TIME()` как функция**: парсер демотирует `TIME` под `allow_time_as_function_name`
   (`xform_demote_keywords.rs:125-147`), но в stdlib нет `TIME` → P4017 (эмпирика `time_as_fn`).
4. **Операторы-функции без реализации**: `LOWER_BOUND`, `UPPER_BOUND`, `BITADR`, `INDEXOF`,
   `XSIZEOF`, `INI`, `TRUNC_INT`, `TEST_AND_SET` → P4017 (analyzer).
5. **Каталог конверсий неполон** (analyzer; `type-conversion-stdlib.md`).
6. **CODESYS-ключевые слова конфигурации/задач** (`TASK`, `INTERVAL`, `PRIORITY`): парсер
   читает (`parser.rs:1805-1827`), семантика — своя (IronPLC scheduler), это не CODESYS-1:1.
7. **`F_EDGE/R_EDGE`** — есть (`token.rs:190-193,260-261`), работает (не gap).
8. **Прагмы `{IF}`** — семантика препроцессора отсутствует (см. §8) — пограничный уровень
   между лексер-платформой и препроцессором.

---

## 13. Приоритетный бэклог

**P0 — блокирует «100 % ST-синтаксиса» (лексер/парсер).** Каждый пункт = токен + демоция
(если может быть идентификатором) + правило грамматики + тест. Рекомендуемый флаг/диалект —
по правилам `specs/steering/syntax-support-guide.md` §Non-Standard Syntax Gating.

| ID | Пункт | Уровень | Статус | Метод проверки |
|---|---|---|---|---|
| P0-1 | `CONTINUE` (statement + codegen) | lexer+parser+AST+codegen | закрыт на ветке после merge `main` (#1898) — перепроверить тестами | новый `tests/continue.rs` (AST-форма) + `codegen/tests/it/end_to_end_continue.rs` (rounds) |
| P0-2 | `PROPERTY`/`PROPERTY_GET`/`PROPERTY_SET`/`END_PROPERTY` + AST | lexer+parser+AST | закрыт на ветке после merge `main` (#1871) — перепроверить тестами | `tests/properties.rs` + plc2plc round-trip resource |
| P0-3 | `UNION`/`END_UNION` + AST | lexer+parser+AST | открыт | `tests/union.rs` + resource |
| P0-4 | `VAR_STAT` / `VAR_INST` / `VAR_GENERIC` блоки | lexer+parser+AST | открыт | `tests/var_declarations.rs` (расширить) |
| P0-5 | `PARAMS(n) OF T` | lexer+parser+AST | открыт | `tests/types_and_returns.rs` |
| P0-6 | `ARRAY[*]` | parser+AST | открыт | `tests/arrays.rs` |
| P0-7 | `NAMESPACE`/`END_NAMESPACE` + `__BEGIN_IMPLEMENTATION` | lexer+parser+AST | открыт | `tests/namespaces.rs` |
| P0-8 | модификаторы доступа + `OVERRIDE`/`OVERLOAD` | lexer+parser+AST | частично: на ветке после merge `main` (#1899 — qualifiers на FB/METHOD); `OVERLOAD` и остальное открыто | `tests/fb_inheritance.rs` |
| P0-9 | `__XSTRING#"…"` → `XByteString`; `UTF8#'…'`; `UCHAR#'…'` | lexer+parser+AST | открыт | `tests/literals.rs` (кейсы) + `spec_conformance_string_literals.rs`; эталон — `tables/string_escapes.csv`, `docs/15` |
| P0-10 | escape `$U`+8 hex (+ cp1252-таблица `$80-$FF`) | dsl+lexer-валидация | открыт | `dsl/src/string_escape.rs` unit-кейсы + `rule_token_string_escape.rs` |
| P0-11 | `10#`, `BOOL#1/0`, `BIT`-тип, `BIT#` | lexer+parser | открыт | `tests/literals.rs` |
| P0-12 | `us`/`ns`, `LT#`, `LD#`, `TOD#hh:mm` | parser | открыт; на ветке после merge — #1940 (дробные секунды TOD/DT) — перепроверить | `tests/duration.rs`, `tests/literals.rs`; обновить `specs/design/time-literals.md` (REQ-TL-010/012) |
| P0-13 | `\|` как OR | lexer+parser | открыт | `tests/whitespace.rs`/`tests/types_and_returns.rs` |
| P0-14 | escape-идентификаторы `` `…` `` + unicode-идентификаторы + правило «несколько `_` подряд» | lexer+parser | открыт | `tests/comments_and_errors.rs`/новый `tests/identifiers.rs`; CODESYS-источник — `ST_GRAMMAR.ebnf:49-54` |
| P0-15 | `__TRY/__CATCH/__FINALLY/__ENDTRY/__THROW` | lexer+parser+AST+codegen | открыт | `tests/try_catch.rs` + e2e |
| P0-16 | ST-visible special (`__NEW`, `__DELETE`, `__ISVALIDREF`, `__SYSTEM`, `__POOL`, `__TYPEOF`, `__CURRENTTASK`, `__XADD`, …) | lexer+parser+analyzer | открыт | `tests/special_operators.rs`; внутренние `__*` — не трогать |
| P0-17 | `JMP`+метки, `CALC`, `__WAIT`, вложенные комментарии, `DocComment`, pragma-`{IF}` | lexer+parser | открыт | `tests/jumps.rs`, `tests/pragmas.rs` (расширить), `tests/comments_and_errors.rs` |

Статусы проверены 2026-10-01; `main` слит в `lint-fences` (03d1f2982).
«Открыт» = нет в токенах/правилах ветки; пункты, закрытые upstream (#1898/#1871/#1899/#1940),
отмечены «на ветке после merge» — перед началом работы прогнать их сниппеты из §15.

**P1 — семантика для уже принятого синтаксиса.** `THIS^/SUPER^` (P9999), члены `INTERFACE`,
codegen SFC, `PROPERTY`-доступ, `VAR_STAT/INST/GENERIC`-размещение, `UNION`-память, `PARAMS`,
`__NEW`-аллокация, TRY-кодоген, `TIME()` и отсутствующие stdlib-операторы, полнота конверсий,
namespace-резолвинг.

**P2 — редкое/внутреннее.** position-прагмы, `DocComment`-модель для LSP, READ_ONLY/READ_WRITE
как var-модификаторы, reserved-unused-запреты (`CLASS`, `USING`, …), whitespace `\v`/`\u00A0`,
`__SYSTEM`/`__POOL` scope-префиксы.

**ВНЕ SCOPE (зафиксировать в README/доке).** IL как язык и `ilo_operators`-мнемоники, vector
(`__VECTOR`, `__VC*`), Safety-типы `SAFE*`, X-типы `__XINT/__XWORD/__UXINT`, внутренние
`__*`-операторы, `__VECTOR`/`__VCSET_*`, динамические `ANY_*_TO_*`-нюансы, position-прагмы
компилятора CODESYS.

---

## 14. Как верифицировать закрытие

### 14.1 Обязательные прогоны

```powershell
# точечно по парсеру (быстро, во время работы)
cargo test -p ironplc-parser
cargo test -p ironplc-parser <имя_теста>        # например continue

# полный гейт перед PR (обязателен по AGENTS.md)
cd compiler
just                # build + coverage >= 85% + clippy + fmt + dupes (10% exact / 5% near)
just format         # авто-формат
```

```bash
# спеки (ADR-номера/фронт-маттер/plan-citations); на Windows рецепты just ломаются —
cd specs && just    # запускать recipe-тела через Git Bash: cd /f/IronPLC && sh.exe ...
```

### 14.2 Правило spec_test

Новая синтаксическая возможность = новый requirement `**REQ-<AREA>-<crate-slug>-<NNN>**` в
`specs/design/<тема>.md`, затем `#[spec_test(REQ_...)]` над тестом (**до** `#[rstest]`).
`spec_requirements_gen` генерирует таблицу из `specs/design`; мета-тест требует
`UNTESTED.is_empty()`. Номера не переиспользуются. Пример — `tests/partial_access.rs:6-8`.

### 14.3 Тестовая пирамида для каждого P0-пункта

Согласно `specs/steering/syntax-support-guide.md`:

1. **Parser-тест** в `compiler/parser/src/tests/<feature>.rs` (+ одна строка `mod` в `tests/mod.rs`)
   — проверяет форму AST, не «просто парсится».
2. **plc2plc round-trip** — `compiler/resources/test/<feature>.st` + тест в
   `compiler/plc2plc/src/tests/` (`assert_round_trips`; рендер обязан перепарситься).
3. **codegen e2e** — `compiler/codegen/tests/it/end_to_end_<feature>.rs` (если генерируется код).
4. **whitespace** — если правило добавляет `_`-зазоры: строка в `tests/whitespace.rs`.
5. **Флаг** (если синтаксис не Ed2): новое поле в `define_compiler_options!` (`options.rs:331-509`),
   CLI-аргумент, `extract_compiler_options` в LSP, доки `.rst` — чек-лист там же.

### 14.4 Проверка по таблицам CODESYS (дифференциальный метод)

* Сниппет-журнал: `ironplcc tokenize <file>` (все ли символы покрыты токенами) и
  `ironplcc check --dialect codesys <file>` (поверхность + семантика).
* Для каждого пункта из §13 завести по одной строке-сниппету из таблиц-источников
  (`tables/*.csv`) и держать их одним набором в `compiler/resources/test/`.
* Контрольные суммы покрытия повторить скриптом из §0 и сверить числа в §1
  (62→77, 36→57, 26+3→30, …). Любое расхождение — регресс.

---

## 15. Журнал эмпирических проверок

Команда: `ironplcc.exe check --dialect codesys <file>` (0.246.0, debug build).

| Сниппет | Результат | Классификация |
|---|---|---|
| `x := BOOL#1;` | FAIL P0002 | лексер/парсер (§6.1) |
| `x := 10#123;` | FAIL P0002 | лексер (§6.1) |
| `TYPE T : ARRAY[*] OF INT;` | FAIL P0002 | парсер (§11) |
| `VAR_STAT x : INT;` | FAIL P0002 | лексер (§2.2) |
| `VAR_INST x : INT;` | FAIL P0002 | лексер (§2.2) |
| `VAR_GENERIC g : INT;` | FAIL P0002 | лексер (§2.2) |
| `FOR i := 0 TO 10 DO CONTINUE; END_FOR` | FAIL P0002 | лексер/парсер (§2.2) |
| `TYPE U : UNION … END_UNION` | FAIL P0002 | лексер/парсер (§2.2) |
| `x : PARAMS(3) OF INT` | FAIL P0002 | лексер/парсер (§2.2) |
| `JMP lbl; lbl: ;` | FAIL P0002 | лексер/парсер (§13 P0-17) |
| `t := T#1us;`, `t := T#1ns;` | FAIL P0002 | парсер (§6.4) |
| `t := LT#5s;` | FAIL P0002 | парсер (§6.4) |
| `d := LD#2024-01-01;` | FAIL P0002 | парсер (§6.4) |
| `t := TOD#12:30;` | FAIL P0002 | парсер (§6.4) |
| `s := __XSTRING#"abc";` | FAIL P0002 | лексер (§6.3) |
| `s := UTF8#'abc';` | FAIL P0002 | лексер (§6.3) |
| `s := UCHAR#'a';` | FAIL P0002 | лексер (§6.3) |
| `(* a (* b *) c *)` | FAIL P0002 | лексер (§8) |
| `PROPERTY p : INT … END_PROPERTY` | FAIL P0002 | лексер/парсер (§9) |
| `PUBLIC METHOD m : INT …` | FAIL P0002 | лексер (§2.3) |
| ``VAR `my var` : INT;`` | FAIL P0003 | лексер (§13 P0-14) |
| `INTERFACE i METHOD m…` (члены) | FAIL P0002 | парсер (§9) |
| `NAMESPACE ns … END_NAMESPACE` | FAIL P0002 | лексер (§2.2) |
| `__BEGIN_IMPLEMENTATION` | FAIL P0002 | лексер (§2.2) |
| `__VECTOR[4] OF REAL` | FAIL P0002 | вне scope (§3.5) |
| `a := a \| b;` | FAIL P0003 | лексер (§3.1) |
| `THIS^.x := 1;` | FAIL P9999 | анализатор (парсер OK) (§9) |
| `t := TIME();` | FAIL P4017 | анализатор (§12) |
| `s : STRING; s := WSTRING#"abc";` | FAIL P4035 | семантика (CODESYS-сканер дал бы Error(20); у нас принято — расхождение) |
| `ARRAY[1..3, 0..2] OF INT` | OK | — |
| `a := a & b;` | OK | — |
| `x := 2 ** 3;` | OK | (у CODESYS `**` internal — мы шире) |
| `{attribute 'qualified_only'}` | OK (trivia) | прагма не вычисляется |
| `{IF defined(FOO)} … {END_IF}` | OK (trivia) | условная компиляция не вычисляется (§8) |
| `x AT %Q* : BOOL;` | OK | — |
| `S=`, `REF=` | OK | — |
| `POINTER TO`, `REFERENCE TO`, `REF_TO`, `NULL` | OK | — |
| `SIZEOF(x)`, `ADR(x)` | OK | — |
| `INT#-5`, `WORD#16#FF` | OK | — |
| `String(10)` под флагом | OK | — |
| `16#FF:` как CASE-метка | OK | — |
| `TYPE E : (A := 1, B := 2) DINT;` | OK | — |
| `my__var` (двойное `_`) | OK у нас | у CODESYS по умолчанию запрещено (`` AllowMultipleUnderlines=false ``, `ST_GRAMMAR.ebnf:51`) |

---

## 16. Что читать дальше (маршрут для агента)

1. `specs/steering/syntax-support-guide.md` — как добавлять синтаксис (обязательно).
2. `compiler/parser/src/token.rs`, `xform_demote_keywords.rs`, `parser.rs` — точки входа.
3. `Codesys/docs/01_LEXER_PARSER.md` §3/§7 — алгоритмы сканера и чек-лист порта.
4. `Codesys/docs/15_STRING_LITERALS.md` + `tables/string_escapes.csv` — строки.
5. `Codesys/docs/10_X_TYPES.md` — если X-типы будут в scope.
6. `Codesys/grammar/ST_GRAMMAR.ebnf` — грамматика-эталон (номера строк цитируются выше).
