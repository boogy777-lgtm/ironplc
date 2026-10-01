# W10. Topology, DeviceDescriptor и BindingResolver

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P1/P2 |
| Зависимости до интеграции | [W03](W03-composition.md), [W07](W07-semantic-schema.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §8, §16 |
| Сценарии участия | T70, T72 |
| Primary evidence owner | T70, T72 |

## Задание LLM

Скомпилировать semantic relation в immutable BindingPlan до выбора исполняющего provider.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/project/src/project.rs](../../../../compiler/project/src/project.rs)
- [compiler/container/src/type_section.rs](../../../../compiler/container/src/type_section.rs)
- [specs/design/dcs-plc-production-platform-spec-ru.md](../../../../specs/design/dcs-plc-production-platform-spec-ru.md)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

Topology владеет membership; resolver pure. Native xor External действует на relation/scope; remote rack не становится network variable из-за Ethernet.

**Разрешённая область изменений:** Topology/descriptors/resolver и validated binding formats; не vendor branch в VM. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Описать Endpoint/Channel/Binding IDs, direction/type/units/scaling/access/quality/timing/groups/provider capacities и assumptions digest.
2. Проверять duplicate writers, overlapping mappings, invalid conversion, неподходящий transport и mismatch device identity.
3. Binding change включить в PreparedBinding; discovery только сообщает facts. Подготовить rebind старых clients и invalidation frames/forces.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W10-A01 | Один IP: native output, два input consumers и external diagnostics | Relations различны; read subscription не получает write grant. |
| W10-A02 | Два writers/overlap; transport не выдерживает deadline | Plan reject до RUN с локализованным conflict/capability reason. |
| W10-A03 | Новый mapping при inflight старом frame | Old frame не пишет новый channel; publication относится к одной generation. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Новый тип AI добавляет descriptor+instance; новый protocol — provider, VM и schema semantics не ветвятся.

**Не принимать:** Классификация по названию протокола или целому physical device; custom свойства IEC variables.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
