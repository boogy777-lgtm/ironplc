# W36. Commissioning, backup/replace/restore и fleet

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P6 |
| Зависимости до интеграции | [W21](W21-retain-reset.md), [W22](W22-firmware.md), [W30](W30-engineering-api.md), [W35](W35-package-sdk.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §12, §13, §16 |
| Сценарии участия | T28, T92 |
| Primary evidence owner | T28, T92 |

## Задание LLM

Обеспечить эксплуатационный путь от installed к commissioned и восстановление после замены CPU/I/O через поддержанные команды.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/vm-cli/src/slot_store.rs](../../../../compiler/vm-cli/src/slot_store.rs)
- [integrations/vscode/src/connectionProfiles.ts](../../../../integrations/vscode/src/connectionProfiles.ts)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

Commissioning хранит accepted configuration/calibration provenance; fleet inventory не участвует в scan или выдаче output grants.

**Разрешённая область изменений:** Commissioning/maintenance API clients/runbooks и backup validation; не manual memory poke. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Проверить identity/topology/calibration/loop checks/forces registry и bounded first-start procedure; нужны actual accepted output policies.
2. Backup включает compatible app/config/retain/firmware refs и redacted secrets; authority identity/active grants не клонируются.
3. Описать replacement/rejoin/decommission, certificate recovery, rollback compatibility и support bundle; reference CLI runbooks исполняемы.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W36-A01 | CPU replacement из старого backup | Fresh BootId/local resources, authority requalification; старый term не восстановлен. |
| W36-A02 | I/O заменён тем же типом в том же slot | Identity/calibration/config policy выполнена до outputs. |
| W36-A03 | Commission→backup→restore→HA rejoin, fleet offline | Завершённый trace; control не требует fleet, скрытых ручных bypass нет. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Новый тип узла использует тот же commissioning/backup envelope с совместимыми descriptors.

**Не принимать:** Runbook требует memory poke, отключение guard или копирование private keys без policy.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
