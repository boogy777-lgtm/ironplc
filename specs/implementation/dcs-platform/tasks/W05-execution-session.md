# W05. Непрерывная ExecutionSession и IEC scheduler

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P1 |
| Зависимости до интеграции | [W02](W02-contracts.md), [W04](W04-verification.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §3, §7, §9, §17 |
| Сценарии участия | T47 |
| Primary evidence owner | T47 |

## Задание LLM

Устранить reset task history при повторных host rounds. Сохранить caller-owned buffers и один mutable execution owner.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/runtime/src/host.rs](../../../../compiler/runtime/src/host.rs)
- [compiler/vm/src/vm.rs](../../../../compiler/vm/src/vm.rs)
- [compiler/vm/src/scheduler.rs](../../../../compiler/vm/src/scheduler.rs)
- [compiler/vm/src/buffers.rs](../../../../compiler/vm/src/buffers.rs)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

ExecutionSession владеет live state и releases. VM borrow может быть кратким, но его повторное создание не означает повторную инициализацию TaskState.

**Разрешённая область изменений:** runtime/host, VM load/resume/scheduler/buffers и их tests; не HA role policy. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Разделить cold initialize, attach/resume и explicit reset. Описать public API переходов без self-referential unsafe конструкции.
2. Определить period/phase/tie-break/missed-release semantics и сохранение истории при неизменном schedule; изменение schedule требует explicit activation policy.
3. Добавить reference scheduler trace по времени и минимум двум периодическим задачам. Проверить call batching независимо от числа run-вызовов.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W05-A01 | Tasks 10/25 ticks, горизонты 0..100; run одним блоком и по одному round | Одинаковые releases/scan counters/values; задача 25 не становится due на каждом вызове. |
| W05-A02 | Clock gap и одновременно due tasks | Order и missed-release policy совпадают с контрактом; нет накопленного неограниченного catch-up. |
| W05-A03 | STOP/START и explicit reset | История меняется только по объявленной reset policy, не из-за reborrow. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Третья task добавляет конфигурацию и capacity; не OS thread и не новый scheduler.

**Не принимать:** Сохранён только общий rounds, а next_due/overrun/task history сбрасываются.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
