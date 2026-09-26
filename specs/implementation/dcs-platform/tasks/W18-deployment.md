# W18. Единый deployment lifecycle и reconciliation

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P3/P4 |
| Зависимости до интеграции | [W16](W16-exact-activation.md), [W17](W17-migration.md), [W20](W20-durable-store.md), [W21](W21-retain-reset.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §6, §10, §12 |
| Сценарии участия | T44, T64 |
| Primary evidence owner | T44, T64 |

## Задание LLM

Вынести orchestration из RuntimeHost в DeploymentCoordinator, сохранив у Host окончательный binding commit.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/runtime/src/host.rs](../../../../compiler/runtime/src/host.rs)
- [compiler/runtime/src/commands.rs](../../../../compiler/runtime/src/commands.rs)
- [compiler/vm-cli/src/slot_store.rs](../../../../compiler/vm-cli/src/slot_store.rs)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

Одна mutating activation на resource. Stage/Trial/Finalize/Revert/Discard и cold/config deploy используют одну transaction; firmware update остаётся отдельным unit.

**Разрешённая область изменений:** Deployment operation module и owner adapters; local execution commit остаётся в runtime. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Реализовать phases RECEIVING/VERIFIED/PREPARED/APPLIED_TRIAL/FINALIZING/FINALIZED и explicit failure/cancel/reconcile/revert исходы.
2. Перед irrecoverable step сохранить intent согласно promised durability; receipts различают Applied и Durable. Boot selection не меняется от trial.
3. Cancel/timeout/late completion/повтор команд обрабатывать по operation scope. Pins удерживать до известного исхода; no deadlock с takeover reservation.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W18-A01 | Crash/receipt loss до/после local commit и durable root | ReadOperation/recovery определяет исход либо UnknownOutcome; повтор не создаёт вторую activation. |
| W18-A02 | Binding edit при inflight force/frame | Code/schema/tasks/map/effects переходят одной generation; stale effects отклонены. |
| W18-A03 | Untest после running trial | Текущий совместимый state сохранён; физическая история не отматывается. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Cold download и config-only change — варианты одной activation, не новые Managers.

**Не принимать:** Accepted назван applied; Finalize только меняет bool; shared state у Host и Coordinator.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
