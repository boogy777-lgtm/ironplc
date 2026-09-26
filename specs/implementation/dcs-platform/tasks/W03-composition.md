# W03. Модульный каркас и admission профиля

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P1 |
| Зависимости до интеграции | [W02](W02-contracts.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §5, §6, §7, §17, §21 |
| Сценарии участия | T07, T31 |
| Primary evidence owner | T07, T31 |

## Задание LLM

Собрать первый composition root с явными зависимостями, capability/resource admission и bounded пересечением execution planes.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/Cargo.toml](../../../../compiler/Cargo.toml)
- [compiler/runtime/src/lib.rs](../../../../compiler/runtime/src/lib.rs)
- [compiler/ironplc-redundancy/src/lib.rs](../../../../compiler/ironplc-redundancy/src/lib.rs)
- [compiler/vm-cli/src/main.rs](../../../../compiler/vm-cli/src/main.rs)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

Root конструирует owners; не дублирует active generation, mode или role. RT модули статические. Подключение provider не даёт ему доступ к private VM.

**Разрешённая область изменений:** Composition/contract modules, Cargo dependency wiring, bounded queue adapters; не внутренние FSM других owners. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Выделять crate только для реально нужной dependency/no_std/qualification границы. Начать с модулей и curated exports, без пустых пакетов.
2. Проверять DAG, версии ports, обязательные capabilities и совокупный memory/queue budget до Start. Зафиксировать ownership buffers при каждом handoff.
3. Обычные команды идут через ограниченные typed queues; revoke имеет отдельный latch/path. В composition запрещены скрытые default providers.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W03-A01 | Два компонента создают цикл зависимостей или двух writers | Сборка/config admission отклонены с конкретной причиной. |
| W03-A02 | Telemetry/command flood при critical revoke | Overflow типизирован; revoke укладывается в profile bound и не ждёт logger. |
| W03-A03 | Optional exporter удалён; required sink отсутствует | В первом случае управление продолжается; во втором Start запрещён. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Второй provider того же port меняет adapter/composition/profile. M — число механизмов, не crates.

**Не принимать:** Общий Service Manager, mutable ContractStore, runtime discovery в scan или dependency на Tokio в domain core.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
