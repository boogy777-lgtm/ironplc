# W04. Каркас доказательств и воспроизводимый simulator

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P1 |
| Зависимости до интеграции | [W02](W02-contracts.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §18, §19, §20, §22 |
| Сценарии участия | T14, T19, T44, T95 |
| Primary evidence owner | Собственные критерии ниже; участие в общих сценариях не заменяет их primary owner. |

## Задание LLM

Связать нормативные IDs, реализацию, тестовый oracle и evidence. Создать deterministic harness с virtual clock, sinks, faults и replay.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/spec_requirements_gen/src/lib.rs](../../../../compiler/spec_requirements_gen/src/lib.rs)
- [compiler/spec_test_macro/src/lib.rs](../../../../compiler/spec_test_macro/src/lib.rs)
- [compiler/runtime/build.rs](../../../../compiler/runtime/build.rs)
- [compiler/ironplc-redundancy/build.rs](../../../../compiler/ironplc-redundancy/build.rs)
- [compiler/ironplc-redundancy/src/simulator.rs](../../../../compiler/ironplc-redundancy/src/simulator.rs)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

Harness наблюдает публичные контракты, не изменяет private state для получения успешного результата. Модель и implementation oracle не должны повторять один алгоритм.

**Разрешённая область изменений:** Test harness, spec registration/build.rs, evidence validators и models; не изменение production guards для облегчения тестов. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Подключить REQ-PORT к фактическим owning crates: текущие runtime/HA build.rs генерируют V-codes, их работу сохранить. Добавить negative registration fixture.
2. Сделать fault schedule, seeded trace/replay, independent sink observer и evidence manifest с hashes/commands/profile/assumptions.
3. Для authority, activation/recovery и HA effect ordering задать отдельные конечные модели; проверять safety и conditional liveness, сохранять bounds и counterexamples.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W04-A01 | Требование не зарегистрировано/файл specs отсутствует | Gate обнаруживает пропуск; пустой ALL/UNTESTED не считается успехом. |
| W04-A02 | В модель внесён второй grant или stale recovery | Model check выдаёт counterexample; исправленная модель проходит тот же scope. |
| W04-A03 | Повтор trace с тем же seed и virtual time | Совпадают решения/эффекты; crash/reorder schedule сохранён как fixture. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Новый сценарий добавляет fixture+oracle+traceability, не второй simulator runtime.

**Не принимать:** Тест assert(true), зелёный if из production code или simulation latency названы аппаратным доказательством.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
