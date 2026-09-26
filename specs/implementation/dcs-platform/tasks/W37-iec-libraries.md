# W37. IEC libraries и эволюция FB state

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P6 |
| Зависимости до интеграции | [W07](W07-semantic-schema.md), [W19](W19-capture.md), [W31](W31-project-build.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §9, §16 |
| Сценарии участия | T35, T54, T59 |
| Primary evidence owner | T35 |

## Задание LLM

Сделать tested library packages частью reproducible build и semantic state evolution, включая timers/counters/edge/PID.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/sources/resources/libs](../../../../compiler/sources/resources/libs)
- [specs/design/compatibility-libraries.md](../../../../specs/design/compatibility-libraries.md)
- [specs/design/function-block-infrastructure-design.md](../../../../specs/design/function-block-infrastructure-design.md)
- [compiler/project/src/sidecar.rs](../../../../compiler/project/src/sidecar.rs)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

Library author определяет numeric/state/reset/time semantics; общий schema/deployment механизм переносит состояние. Стандарт IEC и vendor shim терминология сохраняются.

**Разрешённая область изменений:** IEC library resources/contracts/schema fixtures и lock metadata; не custom IEC variable attributes. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Определить public FB contracts, semantic versions, initial/reset values, bounds и stable instance/field identities.
2. Создать trajectory fixtures: одинаковые текущие inputs при разной истории, reset/downtime, library upgrade exact/convert/reject.
3. Library lock/provenance и target supported features входят в generation; неподдержанный FB/time class rejected до Start.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W37-A01 | TON/edge/PID с двумя разными prehistories | Различие outputs объясняется явным state; capture/migration не теряют internal fields. |
| W37-A02 | Version изменяет FB semantics при одинаковом layout | Exact refused либо доказанная compatible policy; нет silent state reuse. |
| W37-A03 | Same library на двух qualified numeric targets | Conformance по заявленным numeric tolerances/bit policy, без необоснованного FP equality. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Новая library использует общий package/schema/test mechanism; не новый loader/runtime.

**Не принимать:** Версия библиотеки не влияет на semantic compatibility; тесты только steady-state values.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
