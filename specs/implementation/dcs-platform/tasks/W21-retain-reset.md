# W21. Retention, reset classes и совместимые recovery roots

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P4 |
| Зависимости до интеграции | [W07](W07-semantic-schema.md), [W19](W19-capture.md), [W20](W20-durable-store.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §9, §10, §12 |
| Сценарии участия | T40, T65, T74 |
| Primary evidence owner | T40, T65, T74 |

## Задание LLM

Реализовать declared retained projection и reset matrix; связать boot code/config/state только совместимыми roots.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/runtime/src/snapshot.rs](../../../../compiler/runtime/src/snapshot.rs)
- [compiler/runtime/src/migration.rs](../../../../compiler/runtime/src/migration.rs)
- [compiler/vm-cli/src/slot_store.rs](../../../../compiler/vm-cli/src/slot_store.rs)
- [compiler/container/src/type_section.rs](../../../../compiler/container/src/type_section.rs)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

Retention не равна HA snapshot и не содержит grants/role/native handles. Поддержка RETAIN/PERSISTENT заявляется по реально реализованному language contract.

**Разрешённая область изменений:** Retention/reset projections и durable root coordination; не authority/security namespace backup. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Определить warm/cold/origin/watchdog/factory и Stop→Start semantics для каждого storage/time class; unsupported class отклонять manifest admission.
2. Структурный trial сохраняет compatible old root до finalize/reconciliation; новый retain не перезаписывает единственную rollback версию.
3. Restore проверяет schema/generation/device/security scope, создаёт новые local resources и не запускает controller по сохранённому RUNNING.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W21-A01 | Reset matrix с TON/edge/retained counter | Каждый класс имеет ожидаемые defaults/elapsed/restore values и источник root. |
| W21-A02 | Trial записал новый retain; power cut до finalize | Old code не получает new incompatible schema; allowed whole root либо recovery. |
| W21-A03 | Checkpoint содержит старый socket/grant/force | Reject/exclude; ресурсы создаются и квалифицируются заново. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Дополнительный reset cause выбирает существующую policy matrix; новый persistence class требует явного contract.

**Не принимать:** Сохранение только при shutdown выдано за power-loss RPO; role восстановлена из RETAIN.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
