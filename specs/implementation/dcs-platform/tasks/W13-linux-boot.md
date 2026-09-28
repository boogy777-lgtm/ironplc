# W13. Linux composition, boot и независимое containment

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P2 |
| Зависимости до интеграции | [W03](W03-composition.md), [W06](W06-execution-budget.md), [W12](W12-effects.md), [W20](W20-durable-store.md), [W29](W29-security.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §5, §13, §14, §17 |
| Сценарии участия | T01, T06, T55, T67, T89 |
| Primary evidence owner | T01, T06, T55, T67, T89 |

## Задание LLM

Собрать первый реальный controller target: persistent execution, bounded management boundary, boot/no-app/recovery и progress-qualified watchdog.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/vm-cli/src/main.rs](../../../../compiler/vm-cli/src/main.rs)
- [compiler/vm-cli/src/slot_store.rs](../../../../compiler/vm-cli/src/slot_store.rs)
- [specs/design/linux-controller-architecture.md](../../../../specs/design/linux-controller-architecture.md)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

BootCoordinator только lifecycle; health monitor не хранит ControllerState всех owners. Process/MMU/isolation claims относятся к конкретному Linux deployment.

**Разрешённая область изменений:** Linux composition/ports/boot/progress supervisor и packaging target; не common VM fork. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Pin OS/kernel/driver/toolchain/profile; настроить execution context, память/IRQ/queues и зафиксировать фактические limits.
2. При no/corrupt app сохранить authenticated engineering recovery с закрытыми outputs. Отличать unbootable firmware recovery.
3. Следить за releases/завершением tasks/I/O, не только живым process. Watchdog/inhibit с внешним наблюдением; bounded shutdown/crash loop.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W13-A01 | Scheduler не released, diagnostic heartbeat жив | Progress monitor отзывает effects; независимый путь соблюдает D_containment. |
| W13-A02 | Kill management process и memory/flood load | Заявленная isolation измерена; control продолжается либо qualified fallback. |
| W13-A03 | Cold boot без app; app corrupted; kernel/CPU stall | Разные recovery результаты; последний случай покрывает внешний sink, не daemon. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Другая board/NIC меняет providers/profile; Linux controller не получает отдельную VM.

**Не принимать:** Один thread назван memory isolation; watchdog кормится без execution progress.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
