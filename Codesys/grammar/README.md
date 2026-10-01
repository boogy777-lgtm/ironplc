# Грамматика ST (`grammar/`) — оглавление

Наверх: [`../README.md`](../README.md) · Документы: [`../docs/README.md`](../docs/README.md) ·
Таблицы: [`../tables/README.md`](../tables/README.md).

| Файл | Что |
|---|---|
| [`ST_GRAMMAR.md`](ST_GRAMMAR.md) | грамматика ST (объяснения, приоритеты операторов, ссылки на методы парсера) |
| [`ST_GRAMMAR.ebnf`](ST_GRAMMAR.ebnf) | формальная EBNF грамматики ST |
| [`STRING_LITERALS.ebnf`](STRING_LITERALS.ebnf) | EBNF строковых литералов (escape, typed `__XSTRING#`/`UTF8#`/`UCHAR#`) |
| [`AST_MAPPING.md`](AST_MAPPING.md) | привязка правил EBNF к парсеру и узлам AST |

## Связи

- Лексема/токены и таблицы — [`../docs/01_LEXER_PARSER.md`](../docs/01_LEXER_PARSER.md),
  [`../tables/st_keywords.csv`](../tables/st_keywords.csv), [`../tables/operators.csv`](../tables/operators.csv).
- Строки — [`../docs/15_STRING_LITERALS.md`](../docs/15_STRING_LITERALS.md),
  [`../tables/string_escapes.csv`](../tables/string_escapes.csv).
- Построение AST — [`../docs/06_AST_BUILDER_MAP.md`](../docs/06_AST_BUILDER_MAP.md),
  [`../docs/07_AST_RED_TREE_CONSTRUCTION.md`](../docs/07_AST_RED_TREE_CONSTRUCTION.md),
  [`../tables/ast_builder_map.csv`](../tables/ast_builder_map.csv).
