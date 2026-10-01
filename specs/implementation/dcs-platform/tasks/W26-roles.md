# W26. RoleCoordinator, takeover и handover

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P5 |
| Зависимости до интеграции | [W09](W09-mode-key.md), [W13](W13-linux-boot.md), [W24](W24-replication.md), [W25](W25-output-authority.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §11 |
| Сценарии участия | T14, T15, T83 |
| Primary evidence owner | T14, T15, T83 |

## Задание LLM

Ввести PRIMARY/SECONDARY и SYNC_* как независимые факты; promotion допускается по полной формуле v3.0.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/ironplc-redundancy/src/statechart.rs](../../../../compiler/ironplc-redundancy/src/statechart.rs)
- [compiler/ironplc-redundancy/src/admission.rs](../../../../compiler/ironplc-redundancy/src/admission.rs)
- [compiler/ironplc-redundancy/src/shell/mod.rs](../../../../compiler/ironplc-redundancy/src/shell/mod.rs)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

RoleCoordinator владеет handover operation, не checkpoint state и не authority. 0/0 не универсальный запрет/разрешение; legacy conservative policy явно ограничена.

**Разрешённая область изменений:** HA role/admission/orchestration и public host restore adapters; не новый StateStore. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Detect→qualify resume/local resources→prepare without outputs→acquire/fence→revalidate→commit execution/role→reconcile first batch.
2. Учитывать source/peer boots, node-local IP/MAC, checkpoint age, timer downtime и restart policy. Вернувшийся Primary начинает unqualified.
3. Разделить planned handover и failover; непригодный checkpoint не становится пригодным после получения свободного grant.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W26-A01 | 0/0 и Primary жив с valid lease | Второго grant/accepted writer нет; Primary действует по degraded policy. |
| W26-A02 | Primary power loss, 0/0, fresh либо stale checkpoint | Takeover только при всех guards; stale/no-fence → отказ/fallback. |
| W26-A03 | Execution fault при живом network heartbeat; node-local config различается | Progress loss замечен; compatibility не требует одинаковых native handles/IP. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Новая причина takeover подаёт facts в существующий admission; третья active CPU не same-class.

**Не принимать:** SYNC_READY или роль дают write право; persisted Primary после reboot сразу публикует.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
