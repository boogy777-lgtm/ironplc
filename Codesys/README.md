# CODESYS ST — база совместимости синтаксиса и архитектурных исследований

Источник: **CODESYS Development System 3.5.22.10**. Всё получено статически (декомпиляция +
извлечение ресурсов/таблиц); Program Files не изменялся.

Это **главное оглавление (routing)**. Под-индексы: [`docs/README.md`](docs/README.md),
[`tables/README.md`](tables/README.md), [`grammar/README.md`](grammar/README.md),
[`decompiled/README.md`](decompiled/README.md).

---

## ЗАДАНИЕ ДЛЯ НОВОЙ LLM-СЕССИИ (читай первым)

**Цель:**

- **(а)** Довести наш Rust-лексер/парсер (`F:\IronPLC\compiler\parser`) до **100 % покрытия
  синтаксиса ST** по таблицам CODESYS 3.5.22.10, собранным в этом каталоге.
- **(б)** Построить **одно полное CST + единую семантическую модель + отслеживание
  зависимостей** по утверждённой архитектуре ниже. CST использует rowan: green —
  неизменяемое хранилище, red — представление того же синтаксиса.
- **(в)** Изучить и применять **систему кодов SYNTAX-ошибок CODESYS** и наш порядок оформления
  `P####`-кодов (`compiler/problems`: CSV + docs + code + test).

> **Внимание:** syntax-ошибки (scanner/parser) **≠** build/компиляционные **≠** семантические.
> Границы классов — [`ERROR-CODES-STUDY.md`](ERROR-CODES-STUDY.md) §2 («Границы класса SYNTAX»).

**Текущее состояние (2026-10-01):**

- **Исследование CODESYS завершено** — доки, таблицы, грамматики, декомпил собраны.
  Эти файлы — **эталон**: не переделывать и не править под нашу реализацию.
- **Архитектура утверждена, код не написан.** Сейчас фронтенд — logos-лексер + PEG
  (`compiler/parser`); этапы S0–S5 существуют только на бумаге.
- **P0-бэклог открыт** — 17 пунктов; статус каждого — в
  [`LEXER-GAP-ANALYSIS.md` §13](LEXER-GAP-ANALYSIS.md) (столбец «Статус»).
  Baseline-прогон 0.246.0 — журнал §15.
- ⚠ **Ветка `lint-fences` отстаёт от `main`**: на `main` уже есть `CONTINUE` (#1898),
  `PROPERTY` (#1871), qualifiers на FB/METHOD (#1899), дробные секунды TOD/DT (#1940),
  CASE-метки по radix (#1922). **Первый шаг сессии — слить `main` в `lint-fences`
  и перепроверить статусы P0-1/P0-2/P0-8/P0-12** — журнал §15 для них устарел.

**Архитектурное решение владельца, 2026-10-01:** развиваем фронтенд IronPLC:
одно lossless CST на rowan, lowering в существующий `dsl` AST, существующий analyzer
как единственный владелец семантики и один механизм кешированных запросов с
отслеживанием зависимостей. Salsa — предпочтительный кандидат для эксперимента S0.
**Это утверждённый план, не реализованная возможность.** Подробные границы,
доказательства из исходников CODESYS и этапы S0–S5 — в
[`Parse-Tree Architecture`](../specs/design/parse-tree-architecture.md).

**Границы заимствования:** не копировать три дерева CODESYS и не подключать truST
HIR/IDE/LSP как второй семантический backend. Green/red rowan — хранилище и навигация
одного CST. Сначала проверить CST/recovery на существующей PEG-грамматике;
замена парсера (в том числе scoped reuse `trust-syntax`) требует сравнения в S0,
а не запуска второго production-парсера. Сохраняем исходный текст **до**
препроцессора и token transforms. Полный разбор файла допустим на первом этапе;
локальный reparse — отдельная измеряемая оптимизация S5. Ни CST, ни кеширование
семантики сами по себе не доказывают инкрементальный парсинг.

**Порядок чтения (ровно этот, не грепать всё подряд):**

1. **этот README** — целиком;
2. [`Parse-Tree Architecture`](../specs/design/parse-tree-architecture.md) — целиком:
   текущий/целевой фронтенд, один semantic owner, evidence CODESYS и этапы §5;
3. [`LEXER-GAP-ANALYSIS.md`](LEXER-GAP-ANALYSIS.md) — **что чинить**: сводная таблица (§1)
   и приоритетный P0-бэклог (§13); доказательства `file:line` уже внутри;
4. [`ERROR-CODES-STUDY.md`](ERROR-CODES-STUDY.md) — **как оформлять ошибки**: `MessageId`-механика
   (§1–§3) + маппинг на наши `P####` (§4);
5. только потом точечно: [`docs/01_LEXER_PARSER.md`](docs/01_LEXER_PARSER.md),
   [`grammar/ST_GRAMMAR.ebnf`](grammar/ST_GRAMMAR.ebnf), конкретные таблицы из
   [§0 ниже](#0-маршрутизация-по-задачам-начни-отсюда).

**Где работать:**

| Что | Где |
|---|---|
| Репозиторий / ветка | `F:\IronPLC`, ветка `lint-fences` |
| Лексер: токены, опции | `compiler/parser/src/lexer.rs`, `token.rs`, `options.rs` |
| Парсер и правила | `compiler/parser/src/parser.rs`, `rule_token_*.rs` |
| Тесты | `compiler/parser/src/tests/*.rs`, `spec_conformance*.rs` |
| Правила проекта | `specs/steering/`: [`syntax-support-guide.md`](../specs/steering/syntax-support-guide.md), [`compiler-standards.md`](../specs/steering/compiler-standards.md), [`development-standards.md`](../specs/steering/development-standards.md); safe Rust; **no `unwrap/expect/panic` в prod**; dialect gating через `CompilerOptions`; P-код = CSV + docs + code + test (4-tuple) |
| Гейты | `cargo test -p ironplc-parser`; `cd compiler && just`; доки — `cd specs && just` (см. `AGENTS.md`) |

**Первый рабочий шаг (после обязательного чтения выше):**

1. Слить `main` в `lint-fences`, прогнать `cargo test -p ironplc-parser`;
   перепроверить P0-1/P0-2/P0-8/P0-12 по журналу §15 и обновить их статус в §13.
2. Задача по умолчанию — **синтаксический бэклог**: следующий открытый P0-пункт §13,
   ритм «таблица → тесты → реализация → spec conformance» по
   [`syntax-support-guide`](../specs/steering/syntax-support-guide.md), тестовая пирамида — §14.3.
3. **Архитектура (S0+)** — отдельная задача: сначала issue (по фронтенду открытых нет)
   и план в `specs/plans/` (CLAUDE.md), предрефакторинг первым коммитом; S1 не начинать до S0.
   **SYNTAX-коды (Ф5)** — по [`ERROR-CODES-STUDY.md` §4](ERROR-CODES-STUDY.md).
4. Каждая задача — своя ветка от `main` + PR; перед PR — `cd compiler && just` зелёный.

**Порядок развития фронтенда:** S0 — сравнение интеграционных вариантов и baseline;
S1 — полное CST и recovery; S2 — lowering CST → `dsl`; S3 — анализ деклараций/тел POU
с отслеживанием зависимостей; S4 — общий API snapshots для CLI/LSP/MCP/build и
редакторских изменений. S5 — локальный reparse только при доказанной необходимости.
Выходные критерии и владельцы — в единственном
[архитектурном плане, §5](../specs/design/parse-tree-architecture.md#5-evolution-steps).
W32 потребляет этот фронтенд, но не владеет им и не блокирует его начало.

**Синтаксический бэклог Ф1–Ф5** продолжается в рамках одной выбранной грамматики;
S0 фиксирует seam до расширения фронтенда. Детали и порядок P0 — в
[`LEXER-GAP-ANALYSIS.md`](LEXER-GAP-ANALYSIS.md) §13:

- **Ф1. Keywords/tokens:** 15 отсутствующих ST-слов + 7 OO (`CONTINUE`, `PROPERTY`, `UNION`, `VAR_STAT/INST/GENERIC`, `PARAMS`, `NAMESPACE`, модификаторы доступа/`OVERRIDE`).
- **Ф2. Literals:** `$U`+8 hex, typed `__XSTRING#`/`UTF8#`/`UCHAR#`, `10#`, `us/ns`, `BOOL#1`, `LT#/LD#`, `TOD#hh:mm`.
- **Ф3. Operators/символы:** `|`, stdlib-операторы (`LOWER_BOUND`…`TRUNC_INT`), `__*`-спецоператоры (TRY, `__NEW`…).
- **Ф4. Parser-level:** `CONTINUE`, `ARRAY[*]`, `PROPERTY`/OO, `UNION`.
- **Ф5. Syntax-коды ошибок:** по [`ERROR-CODES-STUDY.md`](ERROR-CODES-STUDY.md) §4.

**Ритм фазы:** слить `main` → таблица → тесты → реализация → spec conformance → обновить статус пункта в §13 и журнал §15.
**Закрытие фазы:** тесты зелёные + `cd compiler && just` зелёный.

**Definition of Done для синтаксического бэклога:**

1. Все **P0-пункты** [`LEXER-GAP-ANALYSIS.md`](LEXER-GAP-ANALYSIS.md) §13 закрыты тестами.
2. Новые `P####`-коды оформлены по [`ERROR-CODES-STUDY.md`](ERROR-CODES-STUDY.md) §4.
3. LLM-фенсы не нарушены (no `unwrap/expect/panic` в prod; warnings = deny; `just` зелёный).

Закрытие P0 не означает доказанные «100 %» всего CODESYS ST: scope, исключения и
покрытие фиксируются по корпусу и таблицам. Принятый синтаксис не означает готовую
семантику или выполнение.

**Закрытие архитектурных этапов:** точное восстановление исходного ST-текста,
включая ошибочный ввод и trivia; lowering без изменения существующей семантики;
единственный analyzer для всех клиентов; после каждого edit результаты tracked
analysis совпадают с чистым пересчётом того же snapshot. Счётчики выполнения
запросов показывают reuse незатронутых POU и пересчёт затронутых зависимостей.
Позиции/диагностика привязаны к revision; проверяются изменения сигнатур, тел,
типов, отсутствующих имён, настроек диалекта, библиотек и target. Требования и
conformance-тесты добавляются вместе с реализацией этапа, без пустых заглушек.
Канонический `plc2plc` round-trip и lossless CST round-trip — отдельные контракты.

---

## 0. Маршрутизация по задачам (начни отсюда)

| Твоя задача | Куда смотреть |
|---|---|
| **Архитектура CST и tracked analysis** | [`Parse-Tree Architecture`](../specs/design/parse-tree-architecture.md) — целевая архитектура, evidence CODESYS и этапы S0–S5 |
| **Задание на усиление лексера** | [`LEXER-GAP-ANALYSIS.md`](LEXER-GAP-ANALYSIS.md) + [`ERROR-CODES-STUDY.md`](ERROR-CODES-STUDY.md) |
| Лексер/токены ST | [`docs/01_LEXER_PARSER.md`](docs/01_LEXER_PARSER.md) → [`grammar/ST_GRAMMAR.ebnf`](grammar/ST_GRAMMAR.ebnf) → [`tables/st_keywords.csv`](tables/st_keywords.csv), [`tables/operators.csv`](tables/operators.csv), [`tables/token_types.csv`](tables/token_types.csv) |
| Строковые литералы/escape | [`docs/15_STRING_LITERALS.md`](docs/15_STRING_LITERALS.md) → [`grammar/STRING_LITERALS.ebnf`](grammar/STRING_LITERALS.ebnf) → [`tables/string_escapes.csv`](tables/string_escapes.csv) |
| Исследование CODESYS: парсинг → AST (green/red) | [`docs/06_AST_BUILDER_MAP.md`](docs/06_AST_BUILDER_MAP.md) → [`docs/07_AST_RED_TREE_CONSTRUCTION.md`](docs/07_AST_RED_TREE_CONSTRUCTION.md) → [`docs/08_AST_CONCRETE_NODES.md`](docs/08_AST_CONCRETE_NODES.md) → [`tables/ast_nodes.csv`](tables/ast_nodes.csv); границы нашей реализации — в [`Parse-Tree Architecture`](../specs/design/parse-tree-architecture.md) |
| Система типов и scopes | [`docs/05_TYPE_SYSTEM_SCOPES.md`](docs/05_TYPE_SYSTEM_SCOPES.md) → [`tables/iec_types.csv`](tables/iec_types.csv), [`tables/type_system.csv`](tables/type_system.csv), [`tables/scopes.csv`](tables/scopes.csv) |
| X-типы (`__XINT` …) | [`docs/10_X_TYPES.md`](docs/10_X_TYPES.md) → [`docs/11_X_TYPES_USAGE.md`](docs/11_X_TYPES_USAGE.md) → [`docs/12_X_TYPES_GAPS.md`](docs/12_X_TYPES_GAPS.md) → [`tables/x_types.csv`](tables/x_types.csv) |
| Типизация оператора | [`docs/20_OPERATOR_TYPE_RESOLUTION.md`](docs/20_OPERATOR_TYPE_RESOLUTION.md) → [`tables/operator_type_rules.csv`](tables/operator_type_rules.csv) |
| Порядок фаз компиляции | [`docs/14_COMPILER_PHASES.md`](docs/14_COMPILER_PHASES.md) → [`tables/compiler_phases.csv`](tables/compiler_phases.csv) |
| Детерминизм (порядок POU/сообщений) | [`docs/17_COMPILER_DETERMINISM.md`](docs/17_COMPILER_DETERMINISM.md), [`docs/18_COMPILED_POUS_ORDER.md`](docs/18_COMPILED_POUS_ORDER.md), [`docs/19_MESSAGE_AGGREGATION.md`](docs/19_MESSAGE_AGGREGATION.md) |
| Предкомпиляция / проверка синтаксиса | [`docs/02_PRECOMPILE_PIPELINE.md`](docs/02_PRECOMPILE_PIPELINE.md) → [`docs/03_ERROR_CATALOG.md`](docs/03_ERROR_CATALOG.md) → [`tables/errors/`](tables/errors/) |
| Форматирование/редактор (white tree) | [`docs/16_WHITE_PARSE_TREES.md`](docs/16_WHITE_PARSE_TREES.md) → [`tables/white_tree_nodes.csv`](tables/white_tree_nodes.csv) |
| Обфусцированные имена | [`docs/09_OBFUSCATED_SYMBOLS.md`](docs/09_OBFUSCATED_SYMBOLS.md) → [`tables/obfuscated_map.csv`](tables/obfuscated_map.csv) |
| Размер `XSTRING(n)` / `TypeHelper.GetInt` | [`docs/13_TYPEHELPER_GETINT.md`](docs/13_TYPEHELPER_GETINT.md) → [`tables/string_size_eval.csv`](tables/string_size_eval.csv) |
| Ошибки и тексты (локали) | [`docs/03_ERROR_CATALOG.md`](docs/03_ERROR_CATALOG.md) → [`tables/errors/message_ids.csv`](tables/errors/message_ids.csv), [`tables/errors/error_messages.json`](tables/errors/error_messages.json) |
| Исходники RE | [`decompiled/`](decompiled/), [`Parser35220.plugin/`](Parser35220.plugin/), [`binaries/`](binaries/) |

---

## 1. Карта каталога

| Путь | Что |
|---|---|
| [`README.md`](README.md) | это оглавление + [задание для LLM](#задание-для-новой-llm-сессии-читай-первым) |
| [`LEXER-GAP-ANALYSIS.md`](LEXER-GAP-ANALYSIS.md) | дыры CODESYS ↔ IronPLC: сводка, evidence `file:line`, P0-бэклог |
| [`ERROR-CODES-STUDY.md`](ERROR-CODES-STUDY.md) | SYNTAX-коды CODESYS и маппинг на наши `P####` |
| [`docs/`](docs/) | 20 отчётов RE (`01…20`) + под-индекс |
| [`tables/`](tables/) | 35 лексических/семантических таблиц (CSV/TXT) + `errors/` |
| [`grammar/`](grammar/) | EBNF грамматики ST и строк + привязка к AST |
| [`decompiled/`](decompiled/) | декомпил 14 сборок — см. [`decompiled/README.md`](decompiled/README.md) |
| [`Parser35220.plugin/`](Parser35220.plugin/) + `.sln` | декомпил основного ST-лексер/парсера (94 файла) |
| [`binaries/`](binaries/) | оригинальные DLL для сверки (15 файлов) |
| [`resources/`](resources/) | сырые дампы ресурсов + satellite-DLL (72 файла) |
| [`tools/`](tools/) | скрипты декомпила/дампа/генерации таблиц |

---

## 2. Документация (`docs/`)

| # | Файл | О чём |
|---|---|---|
| 01 | [`01_LEXER_PARSER.md`](docs/01_LEXER_PARSER.md) | где лексер/парсер, модель Token/позиций, алгоритм сканера/парсера, чек-лист Rust |
| 02 | [`02_PRECOMPILE_PIPELINE.md`](docs/02_PRECOMPILE_PIPELINE.md) | пайплайн предкомпиляции, сбор `(code,message,file,line,column)` |
| 03 | [`03_ERROR_CATALOG.md`](docs/03_ERROR_CATALOG.md) | каталог ошибок (`MessageId` ↔ ключ ↔ RU/EN ↔ Rust-variant) |
| 04 | [`04_AST_MODEL.md`](docs/04_AST_MODEL.md) | интерфейсы узлов AST, иерархия, `IRedTreeBuilderFactory` |
| 05 | [`05_TYPE_SYSTEM_SCOPES.md`](docs/05_TYPE_SYSTEM_SCOPES.md) | система типов, scopes, резолвинг имени/члена/перегрузки |
| 06 | [`06_AST_BUILDER_MAP.md`](docs/06_AST_BUILDER_MAP.md) | карта «парсер → builder → узел» |
| 07 | [`07_AST_RED_TREE_CONSTRUCTION.md`](docs/07_AST_RED_TREE_CONSTRUCTION.md) | построение AST: `RedTreeBuilder` + `ITreeFactory` + fluent-билдеры |
| 08 | [`08_AST_CONCRETE_NODES.md`](docs/08_AST_CONCRETE_NODES.md) | конкретные red/green-классы узлов (184) |
| 09 | [`09_OBFUSCATED_SYMBOLS.md`](docs/09_OBFUSCATED_SYMBOLS.md) | восстановленные SmartAssembly-имена |
| 10 | [`10_X_TYPES.md`](docs/10_X_TYPES.md) | X-типы: классы, `Class`/`ToString`, ширина/знаковость |
| 11 | [`11_X_TYPES_USAGE.md`](docs/11_X_TYPES_USAGE.md) | X-типы в тулчейне (лексер/TypeParser/TypeTable/конверсии) |
| 12 | [`12_X_TYPES_GAPS.md`](docs/12_X_TYPES_GAPS.md) | IL-закрытие пробелов X-типов |
| 13 | [`13_TYPEHELPER_GETINT.md`](docs/13_TYPEHELPER_GETINT.md) | `TypeHelper.GetInt` и размер `XSTRING(n)` |
| 14 | [`14_COMPILER_PHASES.md`](docs/14_COMPILER_PHASES.md) | порядок фаз (Scanner→…→Phase6), `TypeAcceptor`/`GenericTypeReplacer` |
| 15 | [`15_STRING_LITERALS.md`](docs/15_STRING_LITERALS.md) | строковые литералы, escape, typed `__XSTRING#`/`UTF8#`/`UCHAR#` |
| 16 | [`16_WHITE_PARSE_TREES.md`](docs/16_WHITE_PARSE_TREES.md) | white tree/CST (форматирование редактора) |
| 17 | [`17_COMPILER_DETERMINISM.md`](docs/17_COMPILER_DETERMINISM.md) | детерминизм Phase4/5, cp1252, Phase6, `\u0017.\u000E` |
| 18 | [`18_COMPILED_POUS_ORDER.md`](docs/18_COMPILED_POUS_ORDER.md) | базовый порядок `m_alCompiledPOUs` |
| 19 | [`19_MESSAGE_AGGREGATION.md`](docs/19_MESSAGE_AGGREGATION.md) | порядок агрегации сообщений, дедуп |
| 20 | [`20_OPERATOR_TYPE_RESOLUTION.md`](docs/20_OPERATOR_TYPE_RESOLUTION.md) | алгоритм типизации оператора |

### Анализ для нашей реализации (корень `Codesys/`)

| Файл | О чём |
|---|---|
| [`LEXER-GAP-ANALYSIS.md`](LEXER-GAP-ANALYSIS.md) | сводная таблица покрытия (§1), дыры по классам (§2–§11), вне-лексерное (§12), P0-бэклог (§13), верификация (§14), журнал эмпирики (§15) |
| [`ERROR-CODES-STUDY.md`](ERROR-CODES-STUDY.md) | `MessageId`-механика (§1), границы класса SYNTAX (§2), 48 кодов `Parser35220` (§3), наши `P####` + маппинг (§4), routing (§5) |

---

## 3. Грамматика (`grammar/`)

- [`ST_GRAMMAR.md`](grammar/ST_GRAMMAR.md) / [`ST_GRAMMAR.ebnf`](grammar/ST_GRAMMAR.ebnf) — грамматика ST.
- [`STRING_LITERALS.ebnf`](grammar/STRING_LITERALS.ebnf) — строковые литералы (escape и typed).
- [`AST_MAPPING.md`](grammar/AST_MAPPING.md) — привязка правил EBNF к парсеру и узлам AST.

---

## 4. Таблицы (`tables/`)

Полный индекс: [`tables/README.md`](tables/README.md).

- **Лексика:** `st_keywords`(77), `operators`(292), `token_types`(28), `operator_flags`(12),
  `standard_operators`(40), `operator_symbols`(30), `datatype_names`(57), `special_operators`(42),
  `ilo_operators`(26), `oo_keywords`(8), `vector_operators`(13), `conversion_operators`(5),
  `ieclanguage`(4), `reserved_unused_keywords`(17).
- **AST/типы:** `ast_nodes`(786), `ast_builder_map`(117), `red_tree_nodes`(184),
  `red_tree_builders`(11), `red_tree_factory_methods`(79), `type_system`(100), `iec_types`(84),
  `x_types`(10), `x_type_operators`(9), `x_types_resolved`(11), `scopes`(20),
  `obfuscated_map`(22), `white_tree_nodes`(219).
- **Семантика/детерминизм:** `operator_type_rules`(292), `compiler_phases`(31),
  `string_size_eval`(17), `compiled_pous_order`(26), `message_aggregation`(17),
  `phase_worker_order`(4), `string_escapes`(20), `cp1252`(32).
- **Ошибки:** [`tables/errors/`](tables/errors/) — `message_ids`(514),
  `error_messages.json`(508), 11 локалей, `parser35220_message_ids`(48), `error_severity`(16).

---

## 5. Ключевые факты

- **ST-лексер/парсер** = `PlugIns\58960fbc-…\3.5.22.10\Parser35220.plugin.dll`
  (`AssemblyDescription="Parser and Scanner for extended structured text"`).
  Лексер `CODESYS.Parser35220.Scanner.InternalScanner`, парсер `CODESYS.Parser35220.InternalParser`.
- **Ошибки** — не `C0xxx`, а enum `MessageId` (507) + ключи `Err_*`/`Wrn_*`/`Inf_*`/`Txt_*`.
- **X-типы**: ST-ключи только `__XINT/__XWORD/__UXINT/__XSTRING`; `XDInt/XLInt/…` — внутренние
  resolved-формы (по `PointerSize`). См. docs 10–12.
- **Типизация оператора** — таблично-арифметическая, без overload-resolution (docs 20).
- **Порядок** POU/сообщений — append-only, детерминирован (docs 17–19).

---

## 6. Воспроизведение

- Декомпилятор: `..\McpFree\tools\decompiler\DecompileHost.exe <in.dll> <outdir>` (ICSharpCode.Decompiler).
- Для больших/обфусцированных: `tools\dnspy\launch.exe -o <outdir> --asm-path "<CODESYS>\Common" --asm-path <dir> --no-sln --no-resx --no-baml <dll>`.
- Метаданные/IL: `dnlib.dll`, скрипты в [`tools/`](tools/).
- Регенерация AST-карт: `tools\extract_ast_nodes.ps1` → `tools\build_concrete_map.py` → `tools\verify_ast_map.py`.

---

## 7. Остаточные пробелы (не критичные для порта)

1. `ScannerOptionsService.GetScanningOptions` — реализация в `decompiled/Compiler35220.plugin/.../PreCompile/ScannerOptionsService.cs`.
2. Языковая модель (`ITypeTable`/`ILanguageModelBuilder7`/`ParserContext`) — исследовать поведение; наша реализация по утверждённому плану понижает CST в существующий `dsl` AST и использует существующий analyzer.
3. Динамические конверсии (`TO_<T>`, `ANY_TO_<T>`) — восстановить отбор по `OperatorFlags`/`GetTextOfOperator`.
4. `GetNextInternal` декомпилирован в goto-граф — восстановить `switch` по IL.
5. 15 `MessageId` без текста; 9 orphan-ключей; порядок под-POU (`Hashtable`); порядок `LDictionary.Keys` при суммаризации.
