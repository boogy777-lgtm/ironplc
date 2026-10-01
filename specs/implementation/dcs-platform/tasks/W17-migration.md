# W17. Явная и ограниченная state migration

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P3 |
| Зависимости до интеграции | [W16](W16-exact-activation.md), [W19](W19-capture.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §9, §10 |
| Сценарии участия | T11, T54, T74 |
| Primary evidence owner | T11 |

## Задание LLM

Реализовать MigrationPlan с preserve/convert/init/drop и доказанным pause budget. Сначала bounded stopped migration; concurrent capture только по потребности профиля.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/runtime/src/migration.rs](../../../../compiler/runtime/src/migration.rs)
- [compiler/runtime/src/conversion.rs](../../../../compiler/runtime/src/conversion.rs)
- [compiler/runtime/src/migration/decision.rs](../../../../compiler/runtime/src/migration/decision.rs)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

Planner pure; Host владеет state. Migration arena не становится live до whole validation. Engineer approval не отменяет bounds/ownership.

**Разрешённая область изменений:** Migration planner/arena/catch-up; capture contract changes согласовать с W19, не вводить второй журнал. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Инициализировать новые объекты, проверить ID/type/layout и resource completeness; conversion policy target+project должна совпасть.
2. Для concurrent варианта использовать coherent base+tracked mutations+bounded final catch-up, independent cursor и deadline. Недостижимый bound → abort/stop-required.
3. Указать reverse-plan feasibility и trial recovery roots. Несуществующий безопасный Revert возвращает отказ до обещания trial.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W17-A01 | Постоянные writes быстрее catch-up, overflow capture | Candidate отменён/stop-required; Original цел, partially migrated state не выбран. |
| W17-A02 | DINT/TIME, narrowing, float precision loss, new nested FB | Явный delta/policy outcome, defaults определены; нет ложного state-preservation proof. |
| W17-A03 | Migration fail перед commit и power loss в trial | Whole compatible root либо recovery; обещанная rollback версия не уничтожена. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Новый тип conversion добавляется в один planner. Concurrent migration — новый класс pause obligation, ΔM обосновать.

**Не принимать:** Растянутое memcpy живой RAM или неограниченный catch-up на barrier.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
