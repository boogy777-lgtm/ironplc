# W14. Первый реальный EtherNet/IP provider

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P2 |
| Зависимости до интеграции | [W10](W10-binding.md), [W11](W11-process-image.md), [W12](W12-effects.md), [W29](W29-security.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §8, §11, §17 |
| Сценарии участия | T33, T69, T79 |
| Primary evidence owner | T33, T69, T79 |

## Задание LLM

Реализовать и квалифицировать один выбранный EtherNet/IP stack через native-I/O и external-data adapters. Не создавать второй binding механизм.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/ironplc-redundancy/src/hal.rs](../../../../compiler/ironplc-redundancy/src/hal.rs)
- [specs/design/dcs-plc-production-platform-spec-ru.md](../../../../specs/design/dcs-plc-production-platform-spec-ru.md)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

Protocol stack владеет connection/sequence/buffers; смысл данных определяет binding. Stack selection/license/safe API фиксируются отдельным implementation decision.

**Разрешённая область изменений:** Provider/codec/connection adapters и qualified stack dependency; common binding только если выявлен versioned contract gap. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. До интеграции pin stack/version/поддерживаемые CIP services, devices, limits и ownership semantics; не заявлять общий ODVA conformance без испытаний.
2. Дать quotas native/external traffic, RPI/timeout contracts, reconnect/session generation и scoped errors.
3. Исследовать takeover подключения на реальном device: reuse либо reconnect+requalify. Для HA доказать native fence или закрытый gateway path; exclusive connection сама по себе недостаточна.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W14-A01 | Один stack: native rack и external PLC variable | Разные quality/import/write policies; runtime не знает CIP. |
| W14-A02 | NIC reset с inflight completion; malformed/flood traffic | Старый buffer/session не используется; RT budget/queue limits соблюдены. |
| W14-A03 | Endpoint без fencing или доступен direct bypass | Full DCS-HA admission запрещён; SINGLE capability описана отдельно. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Следующий native protocol добавляет adapter+descriptor+conformance, не новое IoCycle.

**Не принимать:** UDP echo/simulator назван реальным I/O; persistent connection при takeover обещана без device evidence.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
