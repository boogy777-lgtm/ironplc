# W29. Security с первого slice: identity, capabilities, ingress

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P1/P6 |
| Зависимости до интеграции | [W02](W02-contracts.md), [W03](W03-composition.md), [W04](W04-verification.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §13, §15, §17 |
| Сценарии участия | T07, T42, T58, T66, T82 |
| Primary evidence owner | T82 |

## Задание LLM

Встроить trust/provisioning/authentication/scoped authorization и quotas в owner admission до первого реального external control path.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/container/src/load_verify.rs](../../../../compiler/container/src/load_verify.rs)
- [compiler/runtime/src/commands.rs](../../../../compiler/runtime/src/commands.rs)
- [compiler/vm-cli/src/main.rs](../../../../compiler/vm-cli/src/main.rs)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

Security выдаёт проверяемый principal/capabilities, но не заменяет mode/runtime/sink guards. Нельзя закрыть P2 внешним API с production allow-all.

**Разрешённая область изменений:** Security ports/ingress/auth/admission/audit adapters; не shared mutable execution access. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Составить threat model и TCB: network zones, package parsers, native dependencies, boot/update, secrets и authority peers.
2. Ограничивать size/nesting/rate/decompression/handshakes до дорогой работы. Revalidate revoke перед commit; policy для UTC uncertainty/cert rotation.
3. Audit bounded, scoped и redacted; full audit может запрещать новые privileged mutations, не блокируя control/inhibit.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W29-A01 | Auth/key revoke после prepare при заполненной command queue | Privileged commit/effect запрещён в bound, critical path не ждёт audit. |
| W29-A02 | Spoof peer, malicious package, excessive nesting/flood | Reject с bounded resources; нет grant/SYNC_READY от поддельной identity. |
| W29-A03 | Cert rotation/expiry, clock uncertainty и support export | Поведение по declared policy; keys не утекли; no unlimited offline bypass. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Новый transport привязывает тот же principal/capability contract; scope не расширяется по умолчанию.

**Не принимать:** TLS назван authorization; signature названа correctness; lint/unsafe fence отключён ради port.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
