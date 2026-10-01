# W19. Общий StateView и MutationCapture

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P3 |
| Зависимости до интеграции | [W05](W05-execution-session.md), [W07](W07-semantic-schema.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §4, §9, §11, §12 |
| Сценарии участия | T41, T84 |
| Primary evidence owner | T41, T84 |

## Задание LLM

Дать execution/migration/retention/HA общий semantic capture contract с разными projections и bounded consumers.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/runtime/src/snapshot.rs](../../../../compiler/runtime/src/snapshot.rs)
- [compiler/runtime/src/host.rs](../../../../compiler/runtime/src/host.rs)
- [compiler/vm/src/variable_table.rs](../../../../compiler/vm/src/variable_table.rs)
- [compiler/vm/src/vm.rs](../../../../compiler/vm/src/vm.rs)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

Host остаётся writer live state. Capture фиксирует state writes, включая nested data region, а не только %M или изменившиеся values.

**Разрешённая область изменений:** Runtime state views и все VM write sites нужного semantic scope; не произвольная сериализация process RAM. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Начать с bounded coherent full snapshot, если он укладывается в профиль; dirty bitmap/journal вводить по измеренной потребности.
2. Для writes определить coalescing и separate observable effects; одинаковое значение учитывается как факт write, где контракт этого требует.
3. Независимые cursors/budgets/overflow policies для HA, retain, migration; reader не удерживает RT indefinite.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W19-A01 | Две записи того же value, nested TON/PID/data region | Capture содержит нужную semantic history; effect journal не подменён dirty bitmap. |
| W19-A02 | Медленный retain, HA и migration одновременно | Quotas изолированы; снимается затронутая qualification, scan не блокируется. |
| W19-A03 | Large state и partial snapshot delivery | Публикуется только coherent committed boundary; incomplete не принимается restore. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Ещё один bounded consumer получает cursor/projection, без второй StateStore модели.

**Не принимать:** Общий infinite event log, raw pointers в checkpoint или snapshot copy навязан exact commit.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
