# W30. Controller API, multi-client и operation receipts

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P6 |
| Зависимости до интеграции | [W02](W02-contracts.md), [W09](W09-mode-key.md), [W18](W18-deployment.md), [W29](W29-security.md), [W33](W33-observability.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §6, §15 |
| Сценарии участия | T22, T38, T66, T81 |
| Primary evidence owner | T22, T38, T66, T81 |

## Задание LLM

Дать transport-neutral controller API и reference CLI/client conformance вместо direct private RuntimeHost access.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/runtime/src/commands.rs](../../../../compiler/runtime/src/commands.rs)
- [compiler/vm-cli/src/main.rs](../../../../compiler/vm-cli/src/main.rs)
- [specs/design/engineering-connection.md](../../../../specs/design/engineering-connection.md)
- [specs/adrs/0065-engineering-session-exclusivity-and-ide-side-pending-edits.md](../../../../specs/adrs/0065-engineering-session-exclusivity-and-ide-side-pending-edits.md)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

Session владеет connection/auth context; operation owner сохраняет outcome. Много read clients, conflicting mutations serialized по resource/revision.

**Разрешённая область изменений:** Engineering API/schema/framing/reference client; direct Host access мигрирует через owners. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Version API/schemas/errors и discovery capabilities; stable symbol read/write/force/debug/maintenance идут через owner boundary.
2. Submit/query/cancel/watch/reconcile с OperationId/digest/expected revisions. На reconnect сначала query outcome, не автоматический repeat physical command.
3. Сделать CLI conformance vectors и limits для framing/uploads/subscriptions. Legacy exclusive-session behavior мигрировать явно с ADR.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W30-A01 | Два клиента mutate одну revision; третий читает | Один commit, другой stale/busy; read не блокирует execution. |
| W30-A02 | Applied/Durable reply потерян, session reboot/reconnect | Outcome query восстанавливает известный результат; неизвестный impulse не повторён. |
| W30-A03 | Client пишет старым numeric address после hot edit | Generation/schema mismatch либо stable-id rebind; чужой symbol не изменён. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** CLI/Web/automation — новые клиенты одного API, не новая target FSM.

**Не принимать:** Public force_transition/private pointer, бесконечный pending или global mutex, удерживаемый upload.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
