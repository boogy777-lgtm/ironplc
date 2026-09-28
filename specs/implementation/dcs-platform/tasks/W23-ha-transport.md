# W23. Bounded HA transport и два независимых links

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P5 |
| Зависимости до интеграции | [W03](W03-composition.md), [W04](W04-verification.md), [W29](W29-security.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §7, §11, §17 |
| Сценарии участия | T13, T48, T51 |
| Primary evidence owner | T13, T48, T51 |

## Задание LLM

Отделить protocol engine от конкретного NIC, убрать неограниченные Vec/frame allocations на critical path и квалифицировать dual optical transport.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/ironplc-redundancy/src/hal.rs](../../../../compiler/ironplc-redundancy/src/hal.rs)
- [compiler/ironplc-redundancy/src/pair_link.rs](../../../../compiler/ironplc-redundancy/src/pair_link.rs)
- [compiler/ironplc-redundancy/src/liveness.rs](../../../../compiler/ironplc-redundancy/src/liveness.rs)
- [compiler/ironplc-redundancy/src/calibration.rs](../../../../compiler/ironplc-redundancy/src/calibration.rs)
- [compiler/ironplc-redundancy/src/udp.rs](../../../../compiler/ironplc-redundancy/src/udp.rs)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

Per-link owner хранит Boot/Session/Seq/window; transport не принимает решение о promotion. Ping/pong sequence и lost counters раздельны.

**Разрешённая область изменений:** HA protocol transport/frame buffers, NIC/UDP adapters и calibration; не RoleCoordinator guards. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Caller-owned pools, bounded reassembly/window/work-per-call, explicit backpressure и send milestones. Ограничить hostile frame parser до allocation.
2. Ввести consecutive sequences без +1000, duplicate/reorder/gap handling, wrap/restart и authenticated peer identity.
3. Calibration хранит tuple/load/clock/bounds/expiry; EMA — telemetry. Независимость PHY/NIC/DMA/IRQ/power и authority path проверить физически.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W23-A01 | Giant/corrupt/reordered fragments, pool exhausted | Bounded CPU/memory, typed refusal; нет ложного peer checkpoint ACK. |
| W23-A02 | Один path потерян; затем logical два channels через один NIC | Первый degraded по guards; второй не проходит dual-independent qualification. |
| W23-A03 | Peer reset и delayed pong; asymmetric latency | Old session reject; RTT/2 не выдан за one-way upper bound. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Третий поддержанный NIC adapter не меняет HA domain protocol; capacities квалифицируются вновь.

**Не принимать:** Успешный UDP loopback назван optical HIL; одна VLAN-пара названа независимыми links.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
