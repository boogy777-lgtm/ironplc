# W02. Типизированные контракты, идентичности и исходы операций

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P1 |
| Зависимости до интеграции | [W01](W01-baseline.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §4, §6, §9, §15 |
| Сценарии участия | T36, T39 |
| Primary evidence owner | T36, T39 |

## Задание LLM

Определить минимальные domain contracts, через которые владельцы обмениваются намерениями, handles, фактами и результатами.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/runtime/src/generation.rs](../../../../compiler/runtime/src/generation.rs)
- [compiler/runtime/src/commands.rs](../../../../compiler/runtime/src/commands.rs)
- [compiler/ironplc-redundancy/src/epoch.rs](../../../../compiler/ironplc-redundancy/src/epoch.rs)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

Каждая identity имеет issuer/scope/lifetime. Контракты не владеют общей mutable БД. OperationId, ActivationRevision, CheckpointSeq, OwnerTerm и BootId несовместимы по типу.

**Разрешённая область изменений:** Нижележащие contract modules; adapters в runtime/HA commands. Не protocol engine или VM dispatch. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Ввести newtypes, envelopes и явные clocks/revisions; сохранить адаптер совместимости старого wire API там, где его семантика представима.
2. Разделить Rejected/Accepted/Applied/Durable/Failed/Cancelled/UnknownOutcome. Указать linearization point, dedupe scope+principal+digest, expiry и outcome query.
3. Pure admission получает полный snapshot/time/policy. При commit owner заново проверяет revocations и revisions; поздние completions коррелируются с boot/operation.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W02-A01 | Перестановка ActivationRevision и OwnerTerm | Compile-fail/API check запрещает подстановку без явного преобразования. |
| W02-A02 | Cancel A; start B; completion A | B не завершена; неизвестный физический исход A передан reconciliation. |
| W02-A03 | Тот же OperationId, другой digest/principal; ambient time изменён | Конфликт отклонён; pure result одинаков при одинаковых явных входах. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Новая однотипная команда использует envelope и outcome model; условия остаются у её владельца.

**Не принимать:** Universal Command enum/JSON bus проник в RT path либо grant представлен bool без provenance.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
