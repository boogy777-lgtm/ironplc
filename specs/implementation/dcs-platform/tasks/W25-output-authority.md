# W25. Независимый OutputAuthority и RecoveryAdmission

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P5 |
| Зависимости до интеграции | [W02](W02-contracts.md), [W04](W04-verification.md), [W12](W12-effects.md), [W20](W20-durable-store.md), [W29](W29-security.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §11 |
| Сценарии участия | T05, T16, T17, T50, T57, T77, T78 |
| Primary evidence owner | T05, T16, T17, T50, T57, T77, T78 |

## Задание LLM

Реализовать authority вне обеих CPU execution failure domains и whole-group sink enforcement. Это P0 блокер полной DCS-HA квалификации.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/ironplc-redundancy/src/fencing.rs](../../../../compiler/ironplc-redundancy/src/fencing.rs)
- [compiler/ironplc-redundancy/src/epoch.rs](../../../../compiler/ironplc-redundancy/src/epoch.rs)
- [compiler/ironplc-redundancy/src/lease.rs](../../../../compiler/ironplc-redundancy/src/lease.rs)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

Только authority выдаёт durable incarnation/term/grant и хранит RecoveryAdmission. RoleCoordinator не mint-ит tokens; checkpoint sequence не authority term.

**Разрешённая область изменений:** Authority protocol/core и отдельный external target/sink adapter; старый epoch migration с explicit compatibility. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Выбрать gateway/native endpoints и закрыть bypass. Определить admitted group/membership, leases на receiver clock, identity/reboot/exhaustion.
2. Acquire: prepare requester→revoke/expiry old→new term→fence всех required sinks→GrantReady. Partial acquire не восстанавливает прежний term.
3. RecoveryAdmission versioned durable record: required generation/policy/membership/auto-recovery. Unknown record deny; reboot reconciles fences до grants.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W25-A01 | Partition/old Primary reboot/replay/authority reset | Внешний observer ни разу не видит двух accepted writers одной group. |
| W25-A02 | Один sink не ACK fence, late grant старого boot | GrantReady не выдаётся; receiver fallback/expiry в профиле. |
| W25-A03 | Scan seq растёт, owner term около overflow; stale backup restore | Term не меняется каждый scan и никогда не повторяется; controlled refusal/recovery. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Ещё один sink того же класса добавляет membership/capacity/evidence; redundant arbiter — отдельный класс availability.

**Не принимать:** Grant проверен только в PLC RAM; successful majority разрешает whole-group output.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
