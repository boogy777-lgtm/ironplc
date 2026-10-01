# Отчёт воркспейса CODESYS — приёмка (2026-10-01)

Ветка `lint-fences`. Воркспейс закрывал три потока: верификацию каталога
`MessageId`, P0-бэклог синтаксиса ST и S0 архитектуры CST. Ниже — что
проверено, что закрыто и что осталось; детали — по ссылкам.

## 1. Верификация каталога `MessageId` — завершена (514/514)

- **Проверено 514/514 записей**: `MessageId` 0–506 (507 слотов: 423 привязанных
  + 84 незанятых) и `InternalErrorIds` 0–6.
- **Вердикт: 412 verified, 2 fixed, 100 gap**; полный итог — в
  [`ERROR-CODES-STUDY.md` §6](ERROR-CODES-STUDY.md).
- **Исправлено (2)**: id 245 → ресурсный ключ `Err_MissingObjectForPersistent`,
  id 362 → `Wrn_InvalidStringSize`; официальные тексты восстановлены во всех
  10 локалях (`tables/errors/error_messages.json` и per-locale CSV).
- **Пробелы (100) задокументированы**: 84 незанятых значения enum, 6
  `InternalErrorIds` (внутренние, не локализуются), 10 `MessageId` без текста в
  3.5.22.10 (200, 210, 223, 315, 349, 350, 370, 394, 404, 410); id 508/510/523 —
  вне диапазона прогона.
- **Orphan-ключи: 9 → 7** (развязаны 245/362).
- §(f) и сводка [`docs/03_ERROR_CATALOG.md`](docs/03_ERROR_CATALOG.md) приведены
  в соответствие с §6.
- Коммиты: `114ff378e` (верификация и починка таблиц), `7c5aacf82`
  (реклассификация 200/210/223 в пробелы).

## 2. Синтаксический P0-бэклог — закрыто 14/17

Верификация: прогон 2 (0.247.0, `d8ddf54f5`) — §13 и журнал §15
([`LEXER-GAP-ANALYSIS.md`](LEXER-GAP-ANALYSIS.md)); интеграция групп — merge
`def8ba954`; журнал и статусы после прогона — `9ce888e27`.

| Пункт | Коммит(ы) | Тесты (пути от `compiler/`) |
|---|---|---|
| P0-1 `CONTINUE` | `88396827e` (#1898, из `main`) | `parser/src/tests/continue_statement.rs`, `analyzer/src/rule_loop_control_inside_loop.rs`, `codegen/tests/it/end_to_end_continue.rs` |
| P0-2 `PROPERTY` | `7af4ef644` (#1871, из `main`) | `parser/src/tests/property.rs` + `plc2plc/src/tests/property.rs` |
| P0-3 `UNION` | `9657cb5f5` | `parser/src/tests/union.rs` + `plc2plc/src/tests/union.rs` |
| P0-5 `PARAMS(n) OF T` | `83e7c19c7` | `parser/src/tests/types_and_returns.rs` + `plc2plc/src/tests/params_of.rs` |
| P0-6 `ARRAY[*]` | `83e7c19c7` | `parser/src/tests/arrays.rs` + `plc2plc/src/tests/incomplete_array.rs` |
| P0-7 `NAMESPACE`/`__BEGIN_IMPLEMENTATION` | `9657cb5f5` | `parser/src/tests/namespaces.rs` + `plc2plc/src/tests/namespaces.rs` |
| P0-9 typed strings (`__XSTRING#`, `UTF8#`, `UCHAR#`) | `af5d2e32a` | `parser/src/tests/literals.rs`, `parser/src/spec_conformance_string_literals.rs` |
| P0-10 `$U`+8 hex и cp1252 | `8625a7c4c` | `dsl/src/string_escape.rs`, `parser/src/rule_token_string_escape.rs` |
| P0-11 `10#`, `BOOL#1/0`, `BIT#` | `d5f13cb82` | `parser/src/tests/literals.rs` |
| P0-12 `us`/`ns`, `LT#`/`LD#`, `TOD#hh:mm` | `7b6e5fdf4` (+`91a579557`/#1940 из `main`) | `parser/src/tests/duration.rs`, `parser/src/tests/literals.rs` |
| P0-13 `\|` как OR | `83e7c19c7` | `parser/src/tests/whitespace.rs`, `parser/src/tests/types_and_returns.rs` |
| P0-14 backtick-идентификаторы, unicode, `__`-политика | `83e7c19c7` | `parser/src/tests/identifiers.rs`, `parser/src/tests/comments_and_errors.rs` + `plc2plc/src/tests/escaped_identifiers.rs` |
| P0-15 `__TRY/__CATCH/__FINALLY/__THROW` | `e22e7cbdd` | `parser/src/tests/try_catch.rs` + e2e |
| P0-17 `JMP`/метки, `CALC`, `__WAIT`, вложенные комментарии, `{IF}`-прагмы | `d516c7dfe` | `parser/src/tests/jumps.rs`, `parser/src/tests/pragmas.rs`, `parser/src/tests/comments_and_errors.rs` + `plc2plc/src/tests/jumps.rs` |

Остаток (задокументирован):

- **P0-4/P0-8** — дефект взаимодействия с метками JMP
  ([§15.1](LEXER-GAP-ANALYSIS.md)): базовые формы уже парсятся
  (`9657cb5f5`, `92dc79b14`, `7e71e3d36`), но в `--dialect codesys` мешают
  метки; фикс `region_closer` + проверки имён с квалификаторами — следующий шаг.
- **P0-16** — лексер/парсер принимают весь ST-visible набор `__*`
  (`83e7c19c7`), резолвинг на анализаторе (`__NEW`/`__DELETE`/`__TYPEOF`/
  `__XADD`, scope-префиксы) вынесен в P1.

## 3. S0 архитектуры CST — поставлен

- [`Parse-Tree S0 Audit`](../specs/design/parse-tree-s0-audit.md) (`12fa92760`) —
  аудит preprocessing/provenance (`compiler/parser`), находки F1–F11 и
  кандидаты предрефакторинга.
- [`Parse-Tree S0 Experiment`](../specs/design/parse-tree-s0-experiment.md)
  (`3fa7db8b9`) — rowan/Salsa-спайк (Salsa 0.28.5), выбор парсера (in-tree
  recursive descent + Pratt на rowan) и baseline производительности.
- Спайк-крейт `compiler/s0-spike` — test-only, без production-зависимостей,
  удаляется на cutover S1.
- Issue S1: [#2](https://github.com/boogy777-lgtm/ironplc/issues/2)
  (`7b5c12aa5`); статус архитектуры — `partially implemented`.

## 4. Гейты

- `cd compiler && just` — **зелёный** на приёмочном состоянии: compile (вкл.
  `no_std`-сборку `ironplc-container`), coverage **93.60 %** (порог 85 %),
  clippy, fmt, dupes (7.0 % exact / 2.2 % near).
- Specs-чеки (`adr-numbers`, `adr-front-matter`, `design-front-matter`,
  `plan-citations`) — пройдены (recipe-тела через Git Bash).
- Docs-валидаторы (расширения Sphinx `ironplc_flags`, `ironplc_problemcode`)
  исправлены в этом close-out: добавлены страницы P0023/P0024/P0027/P0042 и
  описания пяти флагов P0-17 (`ironplcc.rst` + списки `codesys`/`twincat`).
  Полная сборка Sphinx локально не поднималась (нет `docs/.venv`) — проверены
  те же инварианты, что и расширения.
- Gate-fix `da2aca9a0`: rustc 1.97–1.98 на MSVC в неанглоязычной локали
  выдаёт локализованную прогресс-строку линкера как warning линта
  `linker_messages`, а `build.warnings = "deny"` в `compiler/.cargo/config.toml`
  превращает его в ошибку сборки (rust-lang/rust#159133); добавлен
  `linker_messages = "allow"` с пояснением — снимается после выхода исправления
  (rust-lang/rust#160445).

## 5. Что осталось

- Синтаксис: P0-4/P0-8 (фикс §15.1 и включение в `--dialect codesys`),
  P0-16 на анализаторе (P1); далее P1/P2 из §13.
- Каталог: 100 задокументированных пробелов (§6), 7 orphan-ключей,
  id 508/510/523 вне диапазона верификации.
- Архитектура: S1 — lossless CST и recovery (issue [#2](https://github.com/boogy777-lgtm/ironplc/issues/2)), затем S2 (lowering
  CST → `dsl`), S3 (tracked analysis), S4 (API snapshots), S5 (локальный
  reparse при доказанной необходимости).
- Docs: полная Sphinx-сборка в этом окружении не прогонялась.
