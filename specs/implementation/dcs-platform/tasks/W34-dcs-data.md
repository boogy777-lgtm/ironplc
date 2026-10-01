# W34. DCS tags, commands, alarms/SOE и historian contracts

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P6 |
| Зависимости до интеграции | [W11](W11-process-image.md), [W29](W29-security.md), [W30](W30-engineering-api.md), [W33](W33-observability.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §16 |
| Сценарии участия | T87, T88 |
| Primary evidence owner | T87, T88 |

## Задание LLM

Реализовать reference DCS adapter с value/quality/time/generation и полным reconnect/gap contract; HMI/historian могут оставаться внешними.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [specs/design/dcs-plc-production-platform-spec-ru.md](../../../../specs/design/dcs-plc-production-platform-spec-ru.md)
- [compiler/runtime/src/commands.rs](../../../../compiler/runtime/src/commands.rs)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

Tag identity отличается от raw memory address. Alarm condition/event/ack имеют явного owner; runtime fault clear не равен alarm acknowledgement.

**Разрешённая область изменений:** DCS namespace/adapters/contract fixtures; не control-critical HMI/historian database. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Version tag namespace, units/scaling/time quality и symbol rebind. Выбрать конкретный information/security profile для OPC UA или другого adapter.
2. Operator commands проходят общий operation/effect API. SOE использует source sequence и clock uncertainty; total order не обещать без модели времени.
3. Historian buffering/backfill ограничить bytes/time/flash wear; reconnect, duplicate и overflow явно видны.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W34-A01 | Bad/stale input и потеря UTC/PTP quality | Exporter сохраняет quality/uncertainty, не публикует good interpolated value. |
| W34-A02 | Alarm duplicates/gaps, reconnect и out-of-order source time | Stable IDs и ack owner обеспечивают определённый outcome; gap не скрыт. |
| W34-A03 | Historian отключён и buffer заполнен | Scan/containment не ждёт consumer; loss/backfill policy наблюдаема. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Новый HMI/historian adapter подключает контракт фактов/команд; не новый control owner.

**Не принимать:** Открытый TCP port объявлен OPC UA conformance; commercial HMI переписывается ради первого release.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
