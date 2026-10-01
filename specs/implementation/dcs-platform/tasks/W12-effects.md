# W12. EffectGate, force overlay и конечный sink

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P2 |
| Зависимости до интеграции | [W09](W09-mode-key.md), [W11](W11-process-image.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §8, §11, §14, §15 |
| Сценарии участия | T02, T12, T24, T32, T63 |
| Primary evidence owner | T02, T12, T24, T32, T63 |

## Задание LLM

Провести каждый физический путь через admission и квалифицируемый sink, включая external commands, protocol FB и commissioning writes.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/runtime/src/host.rs](../../../../compiler/runtime/src/host.rs)
- [compiler/ironplc-redundancy/src/fencing.rs](../../../../compiler/ironplc-redundancy/src/fencing.rs)
- [compiler/ironplc-redundancy/src/lease.rs](../../../../compiler/ironplc-redundancy/src/lease.rs)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

Local gate проверяет mode/binding/permit; receiver выполняет freshness/fallback независимо от PLC. Force — отдельный scoped overlay, не retained IEC value.

**Разрешённая область изменений:** Effect gate/force/sink contracts и reference receiver; не обход драйвера или полномочий. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Задать EffectTicket/EffectId, receiver timeout/sequence, output groups и отсутствие raw bypass. Для SINGLE определить собственный qualified output grant/inhibit.
2. Force связывать с principal/reason/TTL/generation/precedence; revoke при reset/rebind по default policy. Maintenance имеет отдельную authority.
3. Задать boot/TEST/fault/link-loss output contract и максимальный age; повтор старого frame не продлевает hold.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W12-A01 | TEST, force и protocol FB пытаются писать actuator | Все application paths закрыты; отказ виден по reason, без driver bypass. |
| W12-A02 | CPU завис; приходят replay frames старой session | Независимый sink достигает fallback за D_fallback профиля. |
| W12-A03 | Force против IEC write и смена binding | Единая precedence; старый overlay отозван; нет двух writers. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Новый effect path подключает тот же gate/sink contract; impulse с dedupe — отдельное обязательство, не простая absolute write.

**Не принимать:** Send queued принят за физический applied; fallback зависит от живого controller loop.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
