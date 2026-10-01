# W38. no_std closure и platform conformance

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P7 |
| Зависимости до интеграции | [W03](W03-composition.md), [W05](W05-execution-session.md), [W06](W06-execution-budget.md), [W07](W07-semantic-schema.md), [W19](W19-capture.md), [W23](W23-ha-transport.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §5, §17, §21 |
| Сценарии участия | T26, T45, T58 |
| Primary evidence owner | T26, T45, T58 |

## Задание LLM

Замкнуть portable execution/policy/state/checkpoint dependency closure и провести common traces с разными port implementations.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/container/Cargo.toml](../../../../compiler/container/Cargo.toml)
- [compiler/vm/Cargo.toml](../../../../compiler/vm/Cargo.toml)
- [compiler/runtime/Cargo.toml](../../../../compiler/runtime/Cargo.toml)
- [compiler/ironplc-redundancy/Cargo.toml](../../../../compiler/ironplc-redundancy/Cargo.toml)
- [compiler/justfile](../../../../compiler/justfile)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

No_std — dependency property, не timing proof. Alloc при prepare допустим только в bounded profile; RT path без heap. std host adapters отделены.

**Разрешённая область изменений:** Portable dependency closure и platform seams; перестройка module boundaries должна сохранить previous API traces. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Использовать cargo metadata/tree/features для полного closure. Исключить ambient clock/socket/fs/OS branches из common core.
2. Добавить настоящий bare-metal build выбранного target и negative fixture std import; host --no-default-features container недостаточен.
3. Сохранить safe Rust fences; отсутствующий audited safe API — port blocker, не перенос unsafe в соседний workspace.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W38-A01 | Transitively добавлен std в portable dependency | Bare-metal gate падает; исправление в adapter/dependency boundary. |
| W38-A02 | Linux и virtual/embedded ports при тех же domain inputs | Совпадают normalized decisions; timing/isolation квалифицируются отдельно. |
| W38-A03 | Новый provider имеет меньшую capacity/другой clock resolution | Admission explicit reject либо новый qualified profile, core semantics сохранена. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Следующая ОС меняет composition/providers/profile; fork VM/HA запрещён.

**Не принимать:** Успешная сборка container названа no_std готовностью runtime/HA; unsafe fence ослаблен.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
