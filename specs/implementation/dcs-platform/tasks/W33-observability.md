# W33. Fault facts, status projection и bounded telemetry

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P1/P6 |
| Зависимости до интеграции | [W02](W02-contracts.md), [W03](W03-composition.md), [W04](W04-verification.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §4, §14, §16 |
| Сценарии участия | T25, T31, T86 |
| Primary evidence owner | T86 |

## Задание LLM

Создать общий формат facts/status/events с provenance и freshness, сохранив reaction у владельца ресурса.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/runtime/src/commands.rs](../../../../compiler/runtime/src/commands.rs)
- [compiler/ironplc-redundancy/src/shell/views.rs](../../../../compiler/ironplc-redundancy/src/shell/views.rs)
- [compiler/runtime/resources/problem-codes.csv](../../../../compiler/runtime/resources/problem-codes.csv)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

Detector владеет fault latch; reaction owner — action/deadline; exporter только наблюдает. Ack/Clear отличаются от repair/restart.

**Разрешённая область изменений:** Fault/status/event/export modules и stable diagnostic registries; fast reaction только у owner. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Определить stable codes, bounded context/ring/loss counters и task/input/output/checkpoint/queue/storage metrics.
2. StatusSnapshot согласует scope/revisions/boot/clock, stale не выдаётся за current READY. Не делать authoritative global ControllerState.
3. Crash/full logs/export disconnect не блокируют control; support bundle redaction и capability checks.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W33-A01 | Exporter завис/убит, disk full | Fault reaction/scan продолжают по profile; loss markers видны после recovery. |
| W33-A02 | Observer читает старый RUNNING/READY после boot change | Stale provenance отражена; snapshot не выдаёт grant. |
| W33-A03 | Ack fault при незавершённом repair | Latch/outcome по policy; Start не возникает автоматически. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Новый diagnostic consumer использует bounded subscription, не прямой доступ к mutable host.

**Не принимать:** Logging queue обязательна для emergency revoke либо metric cardinality не ограничена.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
