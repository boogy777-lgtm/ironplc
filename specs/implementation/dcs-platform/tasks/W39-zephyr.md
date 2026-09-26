# W39. Первый embedded port: Zephyr

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P7 |
| Зависимости до интеграции | [W35](W35-package-sdk.md), [W38](W38-portable-core.md), [W42](W42-budgets.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §17, §21, §23 |
| Сценарии участия | T26, T30, T56, T60, T91 |
| Primary evidence owner | T30 |

## Задание LLM

Реализовать второй реальный target по общим ports и подтвердить N+1 на Zephyr с конкретной board.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/Cargo.toml](../../../../compiler/Cargo.toml)
- [specs/design/no-std-vm.md](../../../../specs/design/no-std-vm.md)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

OS и board axes независимы. Safe APIs, IRQ/DMA/clock/storage/watchdog — TCB target, domain owners не меняются.

**Разрешённая область изменений:** Zephyr composition/providers/profile/TCB/tests; common core только для доказанного общего contract gap. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Pin board/kernel/toolchain/Rust integration и перечень supported capabilities. Выбрать fixed pools/threads/priority/MPU configuration.
2. Поднять clock, execution context, mailbox, transport, storage и independent inhibit; unsupported boot/HA capabilities оставить deny.
3. Прогнать same-domain traces, hardware reset/clock/flash/IRQ/DMA contention и actual I/O slice.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W39-A01 | Общий application/sample на Linux и Zephyr | Semantic traces совпадают в common profile; artifact compatibility проверена. |
| W39-A02 | Flash erase и IRQ storm при scan | Measured bounds либо workload rejected; low priority не скрывает stalls. |
| W39-A03 | Board revision/reset с stale DMA completion | Lifetime/identity requalified; нет old buffer write. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Другая Zephyr board меняет BSP/providers/profile, не новую ZephyrVm.

**Не принимать:** Compile-only port выдан за PLC-SINGLE release; отсутствующее MPU названо isolation.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
