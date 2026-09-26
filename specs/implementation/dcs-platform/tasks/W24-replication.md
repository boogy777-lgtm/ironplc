# W24. Checkpoint commit, ACK ordering и effect outcome

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P5 |
| Зависимости до интеграции | [W08](W08-catalog.md), [W19](W19-capture.md), [W23](W23-ha-transport.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §9, §11 |
| Сценарии участия | T18, T80 |
| Primary evidence owner | T18, T80 |

## Задание LLM

Реализовать ReplicationEngine на общей state schema и catalog; ACK подтверждает целый restore-ready checkpoint.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/ironplc-redundancy/src/crossload.rs](../../../../compiler/ironplc-redundancy/src/crossload.rs)
- [compiler/runtime/src/snapshot.rs](../../../../compiler/runtime/src/snapshot.rs)
- [compiler/ironplc-redundancy/src/shell/mod.rs](../../../../compiler/ironplc-redundancy/src/shell/mod.rs)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

Engine владеет windows/peer ACK и checkpoint handles, не отдельным приложением/authority. Complete RAM ACK не означает durable NVM или applied actuator.

**Разрешённая область изменений:** Replication/checkpoint engine и catalog/capture adapters; не private copy Host state или output authority. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Checkpoint содержит lineage/schema/source boot/sequence/task-time/inputs/planned output/effect watermark. Delta проверяет base и completeness.
2. Reference ACK_BEFORE_PUBLISH: один bounded in-flight unit, deadline, no blocking VM socket. BOUNDED_LAG только отдельная qualified policy.
3. Для абсолютных setpoints определить idempotent replay после fence. Impulse/remote command требует EffectId+sink dedupe/query либо UnknownOutcome reconciliation.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W24-A01 | Missing fragment, corrupt delta base, stale checkpoint | Нет committed checkpoint/SYNC_READY; buffers освобождаются bounded. |
| W24-A02 | Primary crash после ACK до physical acceptance | Restore coherent state; absolute batch можно reconcile, impulse не повторяется blindly. |
| W24-A03 | ACK deadline нарушен и Primary продолжает degraded | HA guarantee снята явно; standby recovery age/progress envelope ограничен. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Full→delta меняет transfer strategy, не checkpoint commit и recovery contract.

**Не принимать:** ACK последнего fragment назван checkpoint commit; exactly-once обещан без sink contract.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
