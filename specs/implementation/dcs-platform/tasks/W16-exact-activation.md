# W16. PreparedBinding и exact reuse без копирования

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P3 |
| Зависимости до интеграции | [W05](W05-execution-session.md), [W07](W07-semantic-schema.md), [W08](W08-catalog.md), [W11](W11-process-image.md), [W12](W12-effects.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §9, §10, §17 |
| Сценарии участия | T10, T46 |
| Primary evidence owner | T10, T46 |

## Задание LLM

Подготовить новый code/state/task/I/O binding заранее и переключить совместимый descriptor на quiescent boundary при той же live arena.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/runtime/src/online_change.rs](../../../../compiler/runtime/src/online_change.rs)
- [compiler/runtime/src/host.rs](../../../../compiler/runtime/src/host.rs)
- [compiler/vm/src/buffers.rs](../../../../compiler/vm/src/buffers.rs)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

RuntimeHost — единственный commit writer. IoCycle передаёт подготовленные views по фазовому контракту; не допускается отдельный несогласованный I/O commit.

**Разрешённая область изменений:** Host/VM state binding, prepared I/O handles и retire path; не storage sync/network round на commit. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. StateAddressMap связывает новый code с существующими slots; pinned code и scratch подготовлены вне barrier.
2. Revalidate source ActivationRevision, mode/revoke, resources и participants. Descriptor publication — одна linearization boundary; retirement отложен.
3. Определить task history при unchanged/changed schedule. Revert выполняется с текущим state, без snapshot rewind.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W16-A01 | Exact Test/Untest с small/max state и renamed declarations | Arena identity и state values сохранены; zero full-state copy/alloc в commit instrumentation. |
| W16-A02 | Revoke/config change после prepare | PreparedBinding reject; old whole binding остаётся либо prescribed stop. |
| W16-A03 | Active readers и большой old object при commit | Нет premature free; отдельно измерены wait/commit/pause/retire. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Config-only activation использует тот же PreparedBinding; число slots не меняет commit protocol.

**Не принимать:** Сохранён swap_buffers с memcpy; O(1) publication выдана за O(1) полную паузу.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
