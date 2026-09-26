# W15. Сквозной PLC-SINGLE acceptance

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P2 |
| Зависимости до интеграции | [W08](W08-catalog.md), [W13](W13-linux-boot.md), [W14](W14-ethernet-ip.md), [W29](W29-security.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §7, §8, §13, §22 |
| Сценарии участия | T61 |
| Primary evidence owner | T61 |

## Задание LLM

Замкнуть путь source→verified artifact→boot→Start→input→scan→accepted output→fault fallback→recovery без IDE.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/runtime/src/host.rs](../../../../compiler/runtime/src/host.rs)
- [compiler/vm-cli/src/main.rs](../../../../compiler/vm-cli/src/main.rs)
- [tests/e2e/library/README.md](../../../../tests/e2e/library/README.md)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

Это интеграционный work package, не новый controller owner. Simulator и target исполняют общий core и contract, hardware measurements ведутся отдельно.

**Разрешённая область изменений:** Integration fixtures, target launch/config и acceptance evidence; исправления owners — отдельными scoped changes. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Подготовить reproducible sample с минимум двумя tasks, TON/edge и native AI/DO; capabilities и fallback задать manifest/profile.
2. CLI/bootstrap использует те же owner contracts; начальная узкая команда не создаёт второй deployment backend до W30.
3. Сохранить trace sensors/commands/state/output acceptance и отдельное physical capture; неизвестные bounds отмечать blockers.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W15-A01 | Power-on с approved config, IDE отсутствует | Тот же manifest загрузился; Start принят по policy; выход следует принятому input/scan. |
| W15-A02 | I/O disconnect и восстановление | Stale quality видна; physical fallback в D_fallback; requalification до следующего effect. |
| W15-A03 | Candidate invalid или telemetry снята | Действующий control продолжает по admission; orphan resources bounded. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Второй sample/application использует тот же lifecycle; не специальный demo runtime.

**Не принимать:** Работает только manual session на workstation; success только по role/status без измеренного output.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
