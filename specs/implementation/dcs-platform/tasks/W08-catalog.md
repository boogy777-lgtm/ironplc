# W08. ApplicationGeneration, catalog, pins и resource admission

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P1/P3 |
| Зависимости до интеграции | [W03](W03-composition.md), [W07](W07-semantic-schema.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §9, §10 |
| Сценарии участия | T03, T09, T42, T73 |
| Primary evidence owner | T03, T09, T42, T73 |

## Задание LLM

Подготовить immutable generation manifest и catalog с bounded residency, integrity, lifetime pins и target admission.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/container/src/load_verify.rs](../../../../compiler/container/src/load_verify.rs)
- [compiler/runtime/src/generation.rs](../../../../compiler/runtime/src/generation.rs)
- [compiler/runtime/src/host.rs](../../../../compiler/runtime/src/host.rs)
- [compiler/project/src/compile.rs](../../../../compiler/project/src/compile.rs)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

Catalog владеет artifacts/pins; RuntimeHost выбирает active binding. ApplicationGeneration — manifest identity, не один заменяющий всё counter.

**Разрешённая область изменений:** Generation manifest/catalog/admission и их callers; не local binding commit или authority issuer. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Связать code/schema/tasks/bindings/policy/library versions/capabilities/provenance; node-local config отделить.
2. Резервировать active+candidate+rollback+HA+capture+trace ресурсы одновременно. Trust/profile revision привязать к qualification.
3. Поддержать два slot и bounded pool через один pin/retire contract. GC/destruction выполнять вне RT boundary.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W08-A01 | Candidate corrupted или trust/config изменились после verify | Candidate отказан; Original остаётся pinned и исполним. |
| W08-A02 | Третий candidate при двух занятых slots; затем профиль с тремя | Capacity reject либо admission через тот же lifecycle, без потери rollback. |
| W08-A03 | Последний reader/replica pin освобождён во время activation | Нет use-after-retire; тяжёлое освобождение не попадает в commit pause. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Дополнительный slot меняет capacity/backend, не deployment mechanism.

**Не принимать:** Artifact mutable под прежним digest, unbounded cache либо force-delete active generation.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
