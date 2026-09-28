# W41. Порт Ariel OS и ограничения capability

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P7 |
| Зависимости до интеграции | [W35](W35-package-sdk.md), [W38](W38-portable-core.md), [W42](W42-budgets.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §17, §21, §23 |
| Сценарии участия | T26, T49, T51, T56, T91 |
| Primary evidence owner | Собственные критерии ниже; участие в общих сценариях не заменяет их primary owner. |

## Задание LLM

Реализовать Ariel OS port по проверенной версии и честно ограничить networking/storage/HA capabilities.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/Cargo.toml](../../../../compiler/Cargo.toml)
- [specs/design/no-std-vm.md](../../../../specs/design/no-std-vm.md)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

Не считать Ariel только cooperative async. Наличие Rust не отменяет TCB/flash layout/IRQ/independence проверки.

**Разрешённая область изменений:** Ariel composition/providers/layout/profile/tests; не обещание dual-link поверх одного failure domain. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Проверить актуальную pinned версию, safe HAL и scheduling model. Не переносить ограничение latest страницы без проверки release.
2. Закрепить persistent partitions независимо от размера firmware; описать layout migration и interrupted update recovery.
3. Если штатный network path один, полный dual-independent HA reject; новый adapter требует отдельной physical qualification.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W41-A01 | Firmware выросла, persistent partition прежняя | Данные не сдвинуты/перетёрты; incompatibility rejected до update. |
| W41-A02 | Два logical links на одном interface | DCS-HA capability reject, SINGLE может быть отдельно qualified. |
| W41-A03 | Async work не yield, IRQ/flash contention | Actual scheduling/stop/scan bounds измерены либо workload reject. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Новый NIC adapter не меняет role/replication semantics, но требует independence evidence.

**Не принимать:** Rust-only объявлен гарантией no_std/RT/HA; mutable storage location не проверено.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
