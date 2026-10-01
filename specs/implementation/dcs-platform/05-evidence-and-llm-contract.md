# 05. Как LLM доказывает выполненную работу

LLM должна поставлять работающую реализацию с наблюдаемым evidence. Описание архитектуры, зелёный status в UI и список тестов сами по себе результата не доказывают.

## Вход и выход одной задачи

Вход: W-карточка, source/spec SHAs, applicable AGENTS/ironplc-dev/steering, завершённые dependency contracts, profile и known blockers. Выполнять существующий repository PR workflow. Если для compiler/** требуется skill, использовать актуальный установленный `ironplc-dev` либо предоставленный пользователем файл; не считать этот dossier заменой skill.

Выход: scoped diff, обновлённые component/state/contract records, точные команды и results, fixtures/traces/models/raw measurements, requirement links, N+1 delta и честные limitations. Новые diagnostics через CSV/docs/tests; lint fences, single compiler pipeline и IEC syntax сохраняются. Уточнять контракт до большой переписи затронутых callers.

## Уровни evidence

| METHOD | Что проверяет | Чего не доказывает |
|---|---|---|
| STATIC | Visibility, newtypes, dependency closure, lint/build checks | Physical exclusivity, durability, deadlines |
| UNIT/PROPERTY | Локальные semantic laws на явных inputs, boundaries и trajectories | Полную distributed ordering или реальную аппаратуру |
| MODEL | Invariant/liveness в указанной конечной модели и assumptions | Корректность Rust implementation или неограниченную модель автоматически |
| INTEGRATION/SIM | Реальные domain owners с fake ports, races/fault schedules/replay | Target timing, electrical output, power-fail hardware |
| TARGET | Собранные release bytes на exact OS/board/config под нагрузкой | Scope, который не был испытан/обоснован |
| HIL/POWER | Независимо измеренные physical effects, reset/fencing/power cuts | SIL, MTBF, все будущие firmware/hardware revisions |

Для critical distributed guarantees нужны MODEL + implementation traces + TARGET/HIL в их разных scopes. Не все простые pure функции требуют model checker. Выбор метода должен следовать риску и гарантиям, а не одинаковому checklist для каждого getter.

## Acceptance record

Каждая W-A/T/REQ запись содержит: target/workload/profile; initial state; injection point и event ordering; oracle из спецификации; commands/tool versions/seeds; expected и actual results; numeric bound и measurement uncertainty; recovery outcome; evidence path+hash; assumptions; статус `PASS/FAIL/BLOCKED/NOT_RUN/NOT_APPLICABLE`. `NOT_APPLICABLE` требует причины и технически отключённой capability. Missing evidence не проходит release gate.

Разделять:

- **IMPLEMENTED**: код и integration path существуют; dead code/stub не считается feature.
- **VERIFIED**: заявленный software/model scope проверен указанными методами.
- **QUALIFIED**: конкретный target/process/failure envelope прошёл применимые release gates.

Карточки сейчас NOT_STARTED. Нельзя массово заменить статус на PASS после link checker: он проверяет только комплект документов.

## Независимый oracle

Выводить ожидания из §18/20 и task contract. Не вызывать production admission evaluator ещё раз как expected result. Для scheduler использовать заранее выведенный timeline; для sinks — независимый acceptance observer; для storage — crash-prefix/recovery checker; для migration — reference semantic objects и explicit expected conversion; для UI — API facts/reasons и controller conformance.

Mutation/challenge test вводит конкретный дефект: пропуск revision check, premature ACK, второй GrantReady, потеря directory durability или scheduler reset. Соответствующее доказательство должно обнаружить дефект. Это не требование массовых бессмысленных mutation tests на весь repository.

## Минимальные formal models

| Модель | State / действия | Свойства и необходимые assumptions |
|---|---|---|
| Authority | 2 controllers, authority, минимум 2 sinks, terms/leases/record, acquire/revoke/reboot/delay | Нет двух accepted owners; unknown record не выдаёт grant; eventual success только при доступных sinks, bounded delay и progressing requester |
| Activation/recovery | old/new generations, pins, prepared/applied/durable roots, crash/receipt loss | Нет mixed binding, premature free или incompatible boot/retain; reconciliation завершается при доступном store |
| HA first effect / Stop | replica ACK, RecoveryAdmission, first effect, isolated standby, Stop/Acquire | После new first effect stale generation не recover; подтверждённый pair-stop не создаёт скрытый restart |

TLA+/PlusCal допустимы; это выбранный формат model evidence, не production dependency. Проверять invariants и deadlock/conditional liveness, фиксировать fairness, bounds/abstractions и dropped/corrupt/reordered/reboot messages. Сохранить tool command, explored states, output log и counterexample. Затем проиграть существенные counterexamples в integration harness. Модель, из которой исключён offending interleaving, не закрывает проблему.

## Repository gates

Для code changes выполнять applicable gates из AGENTS: `cd compiler && just`, `cd specs && just`, плюс VS Code gates для её изменений. При отсутствии just использовать документированный fallback, а отсутствие Rust/toolchain отмечать BLOCKED, не success. Coverage/dupes thresholds не уменьшать. Новые tests используют repository rstest/spec_test naming и meaningful assertions; test bodies не содержат запрещённые branching/loop/global-state patterns.

W04 сохраняет действующую генерацию V-codes и добавляет spec registration; `UNTESTED.is_empty()` — необходимая регистрационная проверка, не behavioral proof. Отдельный completeness check должен заметить отсутствующий spec-файл, orphan REQ или нулевой набор требований. T/INV/HIL IDs не надо автоматически превращать в фиктивные Rust tests.

## Граница автономной работы

Рутинные implementation choices внутри scope решает исполнитель. Если новый mandatory invariant меняет архитектуру, фиксирует change-impact/ADR и конкретный blocker, продолжая доступную работу. Аппаратные измерения не изобретаются. При недоступном стенде portable код и simulator evidence можно завершить, а profile qualification остаётся BLOCKED. Отчёт всегда указывает, какой именно результат готов и что ещё нельзя утверждать.
