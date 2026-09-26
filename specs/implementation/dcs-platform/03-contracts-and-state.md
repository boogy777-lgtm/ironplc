# 03. Владельцы состояния и contracts

State Inventory — обязательный результат каждой реализации, не общий runtime database. Таблица задаёт границы будущего кода по §4/6 спецификации. Полный schema/lifetime/capacity record оформляется [по шаблону](templates/task-contract.md).

| Owner | Authoritative mutable state | Вход / результат | Кто не может писать |
|---|---|---|---|
| RuntimeHost | ExecutionBinding, LiveStateStore, task history | advance / prepare / commit / capture | IDE, provider callbacks, HA transport, Deployment |
| Catalog | Immutable artifacts и pins/qualification | stage/verify/pin/retire | VM не удаляет active artifact |
| Deployment | Operation phase, reservation, receipts | prepare/trial/finalize/revert | Host не заводит второй deployment journal |
| ModePolicy | Accepted intent/revision/restrictions | request/evaluate/current constraints | UI projection не force-transition |
| HW_KEY | Samples, debounce history, validity | qualified facts и one-shot intent | ModePolicy не подменяет raw contacts |
| IoCycle | Images/plan, quality, force overlay | freeze/seal/device facts | Provider ingress не меняет frozen image |
| ExternalData | Subscription/session/queue/sample sequence | import snapshot / authorized outbound request | Async worker не пишет IEC RAM |
| Replication | Windows, complete checkpoints, peer ACK | transfer/restore-ready facts | Не mint-ит physical grants |
| RoleCoordinator | Handover operation и observed role | eligibility/acquire/restore orchestration | Не дублирует state migration/manifest |
| OutputAuthority | Holder/term/lease/membership/RecoveryAdmission | acquire/renew/revoke/read-record | PLC role или backup не меняет durable term |
| Receiver/sink | Last accepted owner/sequence/age/value | accept/reject/fallback | Replay не renew-ит freshness |
| Persistence backend | Durable transactions и namespace records | commit/read/reconcile receipts | Не выбирает retention/reset semantics |
| FirmwareUpdater/bootloader | Update operation / slot/security metadata | stage/trial/confirm/revert | App deployment не меняет runtime binary |
| Detector / reaction owner | Fault latch / reaction state | fact / action with deadline | Logger не выбирает технологическую реакцию |
| EngineeringSession | Auth context, subscriptions, connection | command/query/watch | Session disconnect не отменяет committed operation |
| StatusProjection/exporter | Derived snapshots, bounded ring/counters | read/events with freshness | READY/RUNNING не authority |

## Минимальный contract record

Указать: owner и single writer; identity/schema/version; consumers; execution plane; protection domain; capacity/work-per-call; lifetime/borrow/pins; clock/deadline; preconditions/revalidation; linearization point; effect acceptance; errors/retry/cancel; reset/durability/replication/migration; security issuer/scope/revoke; dependency/profile evidence.

`Result<Receipt, Error>` сам по себе не описывает distributed outcome. Типизированные ошибки разделяют Denied, StaleRevision, Busy, Unsupported, Incompatible, Exhausted, DeadlineExpired и UnknownOutcome. Wire numbers выделяются по реальному registry, не заранее из головы.

## Важные связи

| Связь | Передаваемые данные | Существенная гарантия |
|---|---|---|
| Engineering → operation owner | CommandEnvelope с target/boot/op/digest/principal/revisions/deadline | Idempotency scope и повторная проверка перед commit |
| Provider → IoCycle | Ingress sample, endpoint/session/sequence/time quality | Callback не даёт выходное право и не мутирует frozen state |
| IoCycle ↔ Host | Immutable input + exclusive working output views | Borrow ограничен phase; seal только после successful unit |
| Deployment → Host | PreparedBinding с pins/maps/resources/proofs/source revision | Whole descriptor, no allocation/network/storage на commit |
| Host → capture consumers | Coherent StateView/checkpoint boundary | Независимые projections/cursors, incomplete не restore-ready |
| Gate → sink | EffectTicket/batch с binding/mode/owner/sequence/age | Gate permission и конечное receiver enforcement различны |
| Authority → candidate | Qualified grant + RecoveryAdmission revision | Fresh compatible resume и all-required sinks fence |
| Store → owner | Durable receipt/root identity | Данные, metadata, power-fail model соответствуют обещанию |
| Facts → UI/DCS | Scoped snapshot/status/quality/time/revisions | Stale data видны; match и readiness не дают authority |

## Состояние и time

В memory inventory отдельно перечисляются nested FB, timers, counters, edge memory, integrators, scheduler releases. Raw addresses/OS handles/DMA buffers/local clock origins не crossload. Logical time переносится по policy; изменение wall clock не продлевает timeout. Checkpoint RAM ACK не durable receipt, а applied output может оставаться unknown после lost ACK.

Local gate не обладает мгновенным знанием будущего revoke, receiver не знает внутренний mode автоматически. Для каждого пересечения задаётся propagation/expiry bound. Требование проверяют как отсутствие запрещённых новых effects после `revoke_time + D_revoke`, с явно оговорённой in-flight boundary и physical fallback, а не как невозможную мгновенную глобальную синхронность.

## Resource accounting

Peak budget суммирует одновременно resident active/candidate/rollback generations, execution state/scratch, migration/capture consumers, HA windows, I/O/DMA, security/network/diagnostic pools и stacks. Только admitted combinations могут выполняться совместно. Bound имеет единицу измерения и owner; значение UNKNOWN блокирует соответствующую qualification. Финальный `drop` большого контейнера учитывается как работа, даже если source code commit выглядит одной строкой.
