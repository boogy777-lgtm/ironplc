# W06. Явное время и ограниченное исполнение

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P1 |
| Зависимости до интеграции | [W05](W05-execution-session.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §7, §14, §17 |
| Сценарии участия | T08, T23, T53 |
| Primary evidence owner | T08, T23, T53 |

## Задание LLM

Ограничить работу внутри execution loop и убрать зависимость portable execution от ambient clock.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/vm/src/vm.rs](../../../../compiler/vm/src/vm.rs)
- [compiler/vm/src/debug_hook.rs](../../../../compiler/vm/src/debug_hook.rs)
- [compiler/vm/src/scheduler.rs](../../../../compiler/vm/src/scheduler.rs)
- [compiler/runtime/src/host.rs](../../../../compiler/runtime/src/host.rs)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

Instruction budget, completed overrun и independent containment — разные механизмы. Нативная зависшая функция не покрывается bytecode budget.

**Разрешённая область изменений:** VM dispatch/time/budget и runtime adapter, соответствующие trap codes/tests; не kernel/watchdog driver. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Передавать monotonic/logical time и измерение execution через узкий port. Разделить UTC timestamps и deadlines с BootId/ClockId.
2. Проверять work budget внутри dispatch; учесть переменную стоимость string/array/builtin операций, init и debug resume. Ограничить либо отвергать unbounded native calls.
3. Описать исход trap: working outputs не publish; progress отражает реально завершённую единицу. Omitted release проверяется отдельным monitor.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W06-A01 | WHILE TRUE и длинная builtin операция | Работа ограничена budget; fault до publication; одна инструкция не скрывает неограниченный loop. |
| W06-A02 | UTC вперёд/назад, monotonic wrap/reset | Lease/TTL не продлены; invalid clock вызывает определённый отказ/requalification. |
| W06-A03 | Execution thread остановлен извне | Software budget не объявлен защитой; независимый sink/watchdog тест связан с W13. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Новая инструкция входит в общий work-accounting contract с собственной bound, без частного watchdog.

**Не принимать:** Timeout проверяется только после возврата scan; средний scan объявлен WCET.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
