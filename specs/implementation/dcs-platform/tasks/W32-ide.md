# W32. VS Code: correlation, Match и общий command workflow

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P6 |
| Зависимости до интеграции | [W30](W30-engineering-api.md), [W31](W31-project-build.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §15, §16 |
| Сценарии участия | T29, T37, T81 |
| Primary evidence owner | T29 |

## Задание LLM

Развить текущий VS Code extension вокруг target API и независимых project/session/deployment/execution/HA facts.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [integrations/vscode/src/connectionState.ts](../../../../integrations/vscode/src/connectionState.ts)
- [integrations/vscode/src/connection.ts](../../../../integrations/vscode/src/connection.ts)
- [integrations/vscode/src/hotEditSession.ts](../../../../integrations/vscode/src/hotEditSession.ts)
- [integrations/vscode/src/hotEdit.ts](../../../../integrations/vscode/src/hotEdit.ts)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

IDE владеет workspace/Pending и projection; final admission на controller. EQUAL не RUN и не takeover-ready; Connected не project correlation.

**Разрешённая область изменений:** integrations/vscode; shared protocol schema/fixtures только согласованно с W30; target guards не в UI. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Сделать Match(A,B) с scope/identities/revisions/freshness для project↔active, pair и active↔boot.
2. UI показывает available commands+reasons, Accepted/Applied/Durable/UnknownOutcome, trial в RUN и отличия TEST mode.
3. После reconnect/hot edit refresh facts и rebind symbols; local edits во время trial не заменяют accepted candidate. Logic вне vscode API для unit tests.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W32-A01 | Project EQUAL, CPU stopped; pair EQUAL, checkpoint stale | UI показывает независимые факты и не разрешает Start/takeover по одному match. |
| W32-A02 | Кнопка разрешена, target revision изменилась | Controller reject отображён; UI не повторяет mutation автоматически. |
| W32-A03 | IDE закрыта в trial; reconnect после lost receipt | Control автономен; operation reconciled, workspace Pending не подменяет PLC candidate. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Новый экран — проекция и команды того же API; второй backend в webview запрещён.

**Не принимать:** Connection/edit/CPU/role слиты в один enum; c8/invariants gates ослаблены.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
