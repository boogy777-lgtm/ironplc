# Отчёты RE (`docs/`) — оглавление

Наверх: [`../README.md`](../README.md) · Таблицы: [`../tables/README.md`](../tables/README.md) ·
Грамматика: [`../grammar/README.md`](../grammar/README.md) · Исходники: [`../decompiled/README.md`](../decompiled/README.md).

| # | Документ | О чём | Связанные таблицы |
|---|---|---|---|
| 01 | [`01_LEXER_PARSER.md`](01_LEXER_PARSER.md) | лексер/парсер, Token/позиции, алгоритмы, чек-лист Rust | `st_keywords`, `operators`, `token_types`, `operator_symbols` |
| 02 | [`02_PRECOMPILE_PIPELINE.md`](02_PRECOMPILE_PIPELINE.md) | пайплайн предкомпиляции, сбор `(code,message,file,line,column)` | `errors/*`, `compiler_phases` |
| 03 | [`03_ERROR_CATALOG.md`](03_ERROR_CATALOG.md) | полный каталог ошибок (RU/EN + Rust-variant) | `errors/message_ids`, `errors/error_messages.json` |
| 04 | [`04_AST_MODEL.md`](04_AST_MODEL.md) | интерфейсы узлов AST, иерархия, `IRedTreeBuilderFactory` | `ast_nodes`, `red_tree_*` |
| 05 | [`05_TYPE_SYSTEM_SCOPES.md`](05_TYPE_SYSTEM_SCOPES.md) | система типов, scopes, резолвинг имени/перегрузки | `iec_types`, `type_system`, `scopes` |
| 06 | [`06_AST_BUILDER_MAP.md`](06_AST_BUILDER_MAP.md) | карта «парсер → builder → узел» | `ast_builder_map` |
| 07 | [`07_AST_RED_TREE_CONSTRUCTION.md`](07_AST_RED_TREE_CONSTRUCTION.md) | построение AST: `RedTreeBuilder` + `ITreeFactory` | `red_tree_builders`, `red_tree_factory_methods` |
| 08 | [`08_AST_CONCRETE_NODES.md`](08_AST_CONCRETE_NODES.md) | конкретные red/green-классы узлов (184) | `red_tree_nodes`, `ast_nodes` |
| 09 | [`09_OBFUSCATED_SYMBOLS.md`](09_OBFUSCATED_SYMBOLS.md) | восстановленные SmartAssembly-имена | `obfuscated_map` |
| 10 | [`10_X_TYPES.md`](10_X_TYPES.md) | X-типы: классы, `Class`/`ToString`, ширина/знаковость | `x_types`, `x_types_resolved` |
| 11 | [`11_X_TYPES_USAGE.md`](11_X_TYPES_USAGE.md) | X-типы в тулчейне (лексер/TypeParser/TypeTable) | `x_type_operators` |
| 12 | [`12_X_TYPES_GAPS.md`](12_X_TYPES_GAPS.md) | IL-закрытие пробелов X-типов | `x_types_resolved` |
| 13 | [`13_TYPEHELPER_GETINT.md`](13_TYPEHELPER_GETINT.md) | `TypeHelper.GetInt` и размер `XSTRING(n)` | `string_size_eval` |
| 14 | [`14_COMPILER_PHASES.md`](14_COMPILER_PHASES.md) | порядок фаз (Scanner→…→Phase6) | `compiler_phases` |
| 15 | [`15_STRING_LITERALS.md`](15_STRING_LITERALS.md) | строковые литералы, escape, typed | `string_escapes`, `../grammar/STRING_LITERALS.ebnf` |
| 16 | [`16_WHITE_PARSE_TREES.md`](16_WHITE_PARSE_TREES.md) | white tree/CST (форматирование) | `white_tree_nodes` |
| 17 | [`17_COMPILER_DETERMINISM.md`](17_COMPILER_DETERMINISM.md) | детерминизм Phase4/5, cp1252, Phase6 | `cp1252`, `phase_worker_order` |
| 18 | [`18_COMPILED_POUS_ORDER.md`](18_COMPILED_POUS_ORDER.md) | базовый порядок `m_alCompiledPOUs` | `compiled_pous_order` |
| 19 | [`19_MESSAGE_AGGREGATION.md`](19_MESSAGE_AGGREGATION.md) | порядок агрегации сообщений, дедуп | `message_aggregation` |
| 20 | [`20_OPERATOR_TYPE_RESOLUTION.md`](20_OPERATOR_TYPE_RESOLUTION.md) | алгоритм типизации оператора | `operator_type_rules` |

## Маршруты (зависимости)

- **Лексер:** 01 → 15 → 10/11 → `st_keywords`, `operators`, `token_types`, `string_escapes`.
- **Парсер/AST:** 01 → 06 → 07 → 08 → 04 → `ast_*`, `red_tree_*`.
- **Семантика:** 05 → 20 → 19 → `iec_types`, `scopes`, `operator_type_rules`, `message_aggregation`.
- **Предкомпиляция/ошибки:** 02 → 03 → 14 → `errors/*`, `compiler_phases`.
- **Детерминизм:** 14 → 17 → 18 → 19 → `compiled_pous_order`, `message_aggregation`.
