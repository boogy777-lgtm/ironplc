# W44. N+1 challenge и независимое расширение каркаса

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P6/P7 |
| Зависимости до интеграции | [W03](W03-composition.md), [W10](W10-binding.md), [W14](W14-ethernet-ip.md), [W20](W20-durable-store.md), [W30](W30-engineering-api.md), [W38](W38-portable-core.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §21, §22 |
| Сценарии участия | T43, T60, T91 |
| Primary evidence owner | T43, T60, T91 |

## Задание LLM

Испытать расширяемость через реальные additions/removal, измеряя ΔM и diff границы, а не количество абстракций.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/Cargo.toml](../../../../compiler/Cargo.toml)
- [specs/design/dcs-plc-production-platform-spec-ru.md](../../../../specs/design/dcs-plc-production-platform-spec-ru.md)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

Same-class определяется semantics/authority/failure obligation. Новый safety class, multi-master или concurrent migration не маскируются под простой plugin.

**Разрешённая область изменений:** Extension fixtures/providers/config/client и diff/evidence review; прежние core tests сохраняются. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Добавить второй provider той же роли, новый device descriptor, ещё клиента и optional exporter; удалить exporter под control load.
2. Поменять pool slots 2→3 и file→flash contract fixture; выполнить второй реальный OS port через W39/W40/W41 до platform claim.
3. Приложить before/after mechanisms, allowed/actual paths, capabilities, complexity/resources и старые conformance traces; провести challenge-review по контрпримеру.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W44-A01 | N+1 device/provider/client/filter instance | Core policies/VM/activation lifecycle не fork; изменения в разрешённых modules/config/tests. |
| W44-A02 | Capacity меньше либо effect semantics новые | Честный admission reject/new-class ADR, не скрытая semantic подмена. |
| W44-A03 | Optional service отсутствует/перегружен | Control invariant и qualified budgets сохранены, отдельного controller mode не появилось. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Это исполняемая проверка M(N+1)=M(N) для same-class; тест не требует min числа crates.

**Не принимать:** Новые if vendor/os в VM, giant generic manager или одинаковые тесты, которые копируют реализацию.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
