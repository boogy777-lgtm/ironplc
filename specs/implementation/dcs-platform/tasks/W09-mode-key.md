# W09. ModePolicy, HW_KEY и запрет скрытого Start

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P1 |
| Зависимости до интеграции | [W02](W02-contracts.md), [W04](W04-verification.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §4, §15 |
| Сценарии участия | T04, T27, T37 |
| Primary evidence owner | T04, T27, T37 |

## Задание LLM

Реализовать независимые mode intent и qualified key facts; отделить PROGRAM/TEST/RUN от Normal/Testing и VM typestate.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/runtime/src/host.rs](../../../../compiler/runtime/src/host.rs)
- [compiler/runtime/src/commands.rs](../../../../compiler/runtime/src/commands.rs)
- [compiler/ironplc-redundancy/src/admission.rs](../../../../compiler/ironplc-redundancy/src/admission.rs)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

HW_KEY владеет samples/debounce/validity; ModePolicy — intent/revision. RuntimeHost хранит execution fact. READY/UI не выдаёт authority.

**Разрешённая область изменений:** Mode/key modules и command admission adapters; HostMode меняется только при явном migration plan callers. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Использовать таблицу policy §15.1. Decode pure; qualifier stateful; restrictive revoke имеет собственный bound.
2. Start требует intent+actual revisions+capabilities+local readiness. Clear/Ack/разрешающий поворот ключа не создают неявный Start.
3. Фиксировать policy для boot в RUN, bounce, UNKNOWN, reset/restart и потери key. Hardware значения остаются параметрами target.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W09-A01 | Key revoke между validate и boundary | Commit отказан либо effect отозван в declared bound; нет старого allow. |
| W09-A02 | Clear fault, reconnect, прежний cached READY | Application не стартует; freshness/revisions перепроверяются. |
| W09-A03 | TEST mode и Test Edits при RUN | Первый блокирует application effects; второй является реальным trial управления. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Новый источник intent использует существующее admission; дополнительный контакт меняет decoder/qualifier.

**Не принимать:** Один enum объединяет connection/project match/CPU/edit/HA; key заменяет аутентификацию.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
