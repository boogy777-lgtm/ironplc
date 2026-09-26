# W40. Порт RT-Thread по тем же contracts

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P7 |
| Зависимости до интеграции | [W35](W35-package-sdk.md), [W38](W38-portable-core.md), [W42](W42-budgets.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §17, §21, §23 |
| Сценарии участия | T26, T30, T56, T58, T91 |
| Primary evidence owner | Собственные критерии ниже; участие в общих сценариях не заменяет их primary owner. |

## Задание LLM

Выполнить capability audit и реализацию RT-Thread port, не создавая отдельную семантику scheduler/binding/HA.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/Cargo.toml](../../../../compiler/Cargo.toml)
- [specs/design/no-std-vm.md](../../../../specs/design/no-std-vm.md)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

SAL/device framework — реализации networking/device access, а не доказательство real-time. Safe Rust bridge — обязательный admission prerequisite.

**Разрешённая область изменений:** RT-Thread composition/providers/profile/TCB/tests; не local unsafe workaround. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Pin RT-Thread/board/toolchain/safe binding/linker/allocator/ABI и TCB. При недопустимом unsafe API выдать конкретный blocker.
2. Реализовать узкие ports с fixed capacities, ISR/thread/cancel/storage/reset contracts; проверить SMP/affinity только для выбранной config.
3. Сравнить common traces и выполнить hardware timing/isolation/power-cut/I/O tests для заявленных profiles.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W40-A01 | SAL connection flood и thread priority interference | Bounded resources; declared scan/containment либо admission отказ. |
| W40-A02 | Safe bridge отсутствует | Port остаётся unsupported; no fence bypass, no pretend stub. |
| W40-A03 | Restart provider при shared buffers | Старая session/completion отвергнута; memory lifetime доказана. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Новый RT-Thread board/stack — provider instance/class conformance, common core unchanged.

**Не принимать:** Одинаковая socket API выдана за одинаковый jitter/ownership contract.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
