# W27. HA deployment, first effect и pair Stop

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P5 |
| Зависимости до интеграции | [W18](W18-deployment.md), [W24](W24-replication.md), [W25](W25-output-authority.md), [W26](W26-roles.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §10, §11, §15 |
| Сценарии участия | T19, T52, T75, T93, T94, T95 |
| Primary evidence owner | T19, T52, T75, T93, T94, T95 |

## Задание LLM

Связать existing activation с replica readiness и authority RecoveryAdmission, исключив stale-generation takeover после нового effect.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/ironplc-redundancy/src/crossload.rs](../../../../compiler/ironplc-redundancy/src/crossload.rs)
- [compiler/ironplc-redundancy/src/commands.rs](../../../../compiler/ironplc-redundancy/src/commands.rs)
- [compiler/runtime/src/host.rs](../../../../compiler/runtime/src/host.rs)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

Deployment reservation сериализует activation/takeover; она не physical grant. Pair Stop имеет local и pair-wide enforcement outcomes.

**Разрешённая область изменений:** Activation participants/HA/authority coordination и operation receipts; не изменение semantics core отдельно для HA. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Stage both→reserve source revision→local new binding без effects→new checkpoint+ACK→durable RecoveryAdmission→first effect.
2. Degraded edit допускается только policy+explicit engineer decision и durable authority update; isolated standby со старым checkpoint физически reject.
3. Stop/maintenance отключает automatic recovery на authority; emergency local inhibit не ждёт её receipt. Abort/revert reconciles неизвестный record, pins сохраняются.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W27-A01 | Crash на каждом шаге вокруг ACK/record/first effect | Recovery выбирает поколение по authoritative record; stale incompatible restore после first effect невозможен. |
| W27-A02 | Degraded edit, standby не получил invalidate, Primary умер | Old generation acquire отвергнут; недоступный authority запрещал новые live effects edit. |
| W27-A03 | Pair Stop одновременно с acquire/crash; receipt потерян | Нет скрытого restart после подтверждённого запрета; unknown pair outcome честно виден. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Config-only HA activation проходит ту же sequence; отдельная HARuntimeVersionStore запрещена.

**Не принимать:** Безопасность основана на доставке invalidate; автоматический Rockwell-like Untest нарушает RecoveryAdmission.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
