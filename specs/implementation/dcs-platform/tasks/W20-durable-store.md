# W20. DurableRecordStore и crash-consistent roots

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P1/P4 |
| Зависимости до интеграции | [W02](W02-contracts.md), [W03](W03-composition.md), [W04](W04-verification.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §10, §12, §13 |
| Сценарии участия | T20, T25, T49 |
| Primary evidence owner | T20, T25, T49 |

## Задание LLM

Превратить имеющийся A/B store в квалифицируемый durable-record backend с явным transaction/recovery contract.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/vm-cli/src/slot_store.rs](../../../../compiler/vm-cli/src/slot_store.rs)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

Backend владеет storage commits; namespace owner определяет semantic unit. Persist operation не выбирает mode/role и не сохраняет всю RAM.

**Разрешённая область изменений:** Persistence port/backend и migration SlotStore callers; не policy выбора mode/HA role. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Разделить namespaces и whole commit roots, schema/identity/integrity/length/revision. Durable receipt только после объявленных data+metadata durability steps.
2. Linux: проверить file sync, rename и parent-directory sync, cache/filesystem assumptions. Flash port отдельно определяет erase/program/torn-write/GC model.
3. Обрабатывать ENOSPC/read-only/failed sync/corruption и sequence exhaustion; retention/authority records не восстанавливать из случайного older marker.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W20-A01 | Power cut между каждым write/sync/rename/marker step | После recovery old либо new whole root; acknowledged Durable не теряется в заявленной модели. |
| W20-A02 | Full storage и failed directory sync | Нет ложного Durable; новые mutations rejected, admitted control по policy продолжается. |
| W20-A03 | Marker/record около overflow, duplicate/stale backup | Identity не повторена; fail-closed/recovery определён. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Flash backend сохраняет значение Durable, но получает собственные power-cut/endurance доказательства.

**Не принимать:** Rename или fsync одного file названы полной power-fail гарантией; simulator заменяет physical cut.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
