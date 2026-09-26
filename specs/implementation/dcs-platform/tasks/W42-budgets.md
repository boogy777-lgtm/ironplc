# W42. Числовой профиль ресурсов и temporal qualification

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P2–P7 |
| Зависимости до интеграции | [W03](W03-composition.md), [W04](W04-verification.md), [W06](W06-execution-budget.md), [W11](W11-process-image.md), [W13](W13-linux-boot.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §7, §17, §22, §23 |
| Сценарии участия | T07, T48, T56, T91 |
| Primary evidence owner | T56 |

## Задание LLM

Превратить qualitative bounded требования в числовой admission envelope каждого target/workload.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/vm/src/scheduler.rs](../../../../compiler/vm/src/scheduler.rs)
- [compiler/vm/src/buffers.rs](../../../../compiler/vm/src/buffers.rs)
- [compiler/ironplc-redundancy/src/timing.rs](../../../../compiler/ironplc-redundancy/src/timing.rs)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

Product/process задаёт допустимые deadlines/output ages, platform измеряет выполнение; LLM не выдумывает process-safe hold или universal 30 ms.

**Разрешённая область изменений:** Profile/admission constraints, instrumentation и qualification data; не магические defaults в core. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Заполнить task T/D/C/J/B, memory peak/stack/DMA/pools/queues, prepare/commit/pause/reclaim, stop/ACK/recovery bounds.
2. Задать одновременно разрешённые edit/HA/trace/security operations; admission проверяет worst allowed combination, не отдельные isolated budgets.
3. Определить метод измерения, инструмент uncertainty, adversarial IRQ/network/flash/crypto load и margin; average/percentile отделить от hard claim.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W42-A01 | Active+candidate+rollback+capture+HA+trace max | Memory reservation в пределах exact profile; no steady-state allocator failure. |
| W42-A02 | Long async task, storage/XIP/IRQ/network storm | End-to-end response и containment проверены независимо; failed bound блокирует claim. |
| W42-A03 | Не заполнен D_fallback/RTO или instrumentation uncertainty | Profile не получает qualified/production status. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Меньшая память/иной jitter новой board меняет admission/profile, не domain algorithm.

**Не принимать:** EMA/percentile объявлены WCET или неизвестный параметр молча получил удобный default.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
