# Таблицы (`tables/`) — оглавление

Наверх: [`../README.md`](../README.md) · Документы: [`../docs/README.md`](../docs/README.md).
Все `*.csv` — UTF-8 без BOM, разделитель `,`, первая строка — заголовок (кроме `*.txt`).
Число в скобках — строк данных (без заголовка).

## Лексика (лексер ST)

| Файл | Строк | Что |
|---|---|---|
| [`st_keywords.csv`](st_keywords.csv) | 77 | ST-ключевые слова (`OperatorTable.AddKeywords`) |
| [`operators.csv`](operators.csv) | 292 | enum `Operator` (name=value) |
| [`token_types.csv`](token_types.csv) | 28 | enum `TokenType` |
| [`operator_flags.csv`](operator_flags.csv) | 12 | enum `OperatorFlags` |
| [`operator_symbols.csv`](operator_symbols.csv) | 30 | символьные операторы |
| [`standard_operators.csv`](standard_operators.csv) | 40 | стандартные операторы |
| [`datatype_names.csv`](datatype_names.csv) | 57 | имена типов данных |
| [`special_operators.csv`](special_operators.csv) | 42 | спецоператоры |
| [`ilo_operators.csv`](ilo_operators.csv) | 26 | IL-операторы |
| [`oo_keywords.csv`](oo_keywords.csv) | 8 | OO-ключевые слова |
| [`vector_operators.csv`](vector_operators.csv) | 13 | `__vc*` |
| [`conversion_operators.csv`](conversion_operators.csv) | 5 | конверсии |
| [`ieclanguage.csv`](ieclanguage.csv) | 4 | enum `IECLanguage` |
| [`reserved_unused_keywords.txt`](reserved_unused_keywords.txt) | 17 | зарезервированные/неиспользуемые |

## AST

| Файл | Строк | Что |
|---|---|---|
| [`ast_nodes.csv`](ast_nodes.csv) | 786 | узлы AST: интерфейс → concrete/green → factory → producer |
| [`ast_builder_map.csv`](ast_builder_map.csv) | 117 | «конструкция → парсер → builder → узел» |
| [`red_tree_nodes.csv`](red_tree_nodes.csv) | 184 | конкретные red-классы узлов |
| [`red_tree_builders.csv`](red_tree_builders.csv) | 11 | fluent-билдеры → узлы |
| [`red_tree_factory_methods.csv`](red_tree_factory_methods.csv) | 79 | `ITreeFactory.CreateX` → узел (file:line) |
| [`white_tree_nodes.csv`](white_tree_nodes.csv) | 219 | узлы white tree/CST |

## Система типов

| Файл | Строк | Что |
|---|---|---|
| [`type_system.csv`](type_system.csv) | 100 | классы системы типов |
| [`iec_types.csv`](iec_types.csv) | 84 | классы `IECType` (+ ST-синтаксис) |
| [`x_types.csv`](x_types.csv) | 10 | X-типы (класс/ширина/знаковость) |
| [`x_type_operators.csv`](x_type_operators.csv) | 9 | X-типы в лексере/TypeTable |
| [`x_types_resolved.csv`](x_types_resolved.csv) | 11 | resolved X-формы + static-init |
| [`scopes.csv`](scopes.csv) | 20 | области видимости и правила поиска |
| [`obfuscated_map.csv`](obfuscated_map.csv) | 22 | обфусцированные символы → роли |

## Семантика / компиляция / детерминизм

| Файл | Строк | Что |
|---|---|---|
| [`operator_type_rules.csv`](operator_type_rules.csv) | 292 | оператор → правило → тип результата |
| [`compiler_phases.csv`](compiler_phases.csv) | 31 | порядок фаз компиляции |
| [`string_size_eval.csv`](string_size_eval.csv) | 17 | вычисление размера `STRING/WSTRING/XSTRING(n)` |
| [`compiled_pous_order.csv`](compiled_pous_order.csv) | 26 | траектория `m_alCompiledPOUs` |
| [`message_aggregation.csv`](message_aggregation.csv) | 17 | шаги агрегации сообщений |
| [`phase_worker_order.csv`](phase_worker_order.csv) | 4 | порядок воркеров Phase4/5 |
| [`string_escapes.csv`](string_escapes.csv) | 20 | escape-последовательности строк |
| [`cp1252.csv`](cp1252.csv) | 32 | Windows-1252 для байтов 0x80–0xFF |

## Ошибки — [`errors/`](errors/)

| Файл | Строк | Что |
|---|---|---|
| [`errors/message_ids.csv`](errors/message_ids.csv) | 514 | enum `MessageId` (value,name) |
| [`errors/error_messages.json`](errors/error_messages.json) | 508 | `{id:{name,key,ru,en,locales}}` |
| [`errors/error_messages_ru.csv`](errors/error_messages_ru.csv) | 507 | RU-тексты |
| [`errors/error_messages_en.csv`](errors/error_messages_en.csv) | 500 | EN-тексты |
| `error_messages_{de,es,fr,it,ja,pt-BR,tr,zh-CHS}.csv` | 500 | прочие локали |
| [`errors/error_messages_ru_provenance.csv`](errors/error_messages_ru_provenance.csv) | 507 | источник RU-перевода |
| [`errors/parser35220_message_ids.csv`](errors/parser35220_message_ids.csv) | 48 | ошибки, генерируемые парсером |
| [`errors/error_severity.csv`](errors/error_severity.csv) | 16 | префиксы ключей → severity |

## Обозначения

- `file:line` — по декомпилу в [`../decompiled/`](../decompiled/) (обфусцированные имена вида `\u001D.\u0005`).
- `Operator`/`TypeClass` значения — числовые коды из `operators.csv` / `type_system.csv`.
