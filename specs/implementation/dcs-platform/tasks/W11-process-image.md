# W11. IoCycle, ExternalData и coherent snapshots

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P2 |
| Зависимости до интеграции | [W05](W05-execution-session.md), [W10](W10-binding.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §7, §8 |
| Сценарии участия | T62, T71 |
| Primary evidence owner | T62, T71 |

## Задание LLM

Заменить I/O no-op контрактом freeze/import→execute→seal. Дать provider ingress и исключить его асинхронные writes в IEC state.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/vm/src/vm.rs](../../../../compiler/vm/src/vm.rs)
- [compiler/vm/src/buffers.rs](../../../../compiler/vm/src/buffers.rs)
- [compiler/runtime/src/host.rs](../../../../compiler/runtime/src/host.rs)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

IoCycle владеет images/quality/plan; VM временно получает working output view. ExternalData владеет subscriptions/queues, импорт происходит на объявленной boundary.

**Разрешённая область изменений:** IoCycle/ExternalData и execution views; VM integration только по согласованному W05/W16 seam. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Определить consistency unit, value+quality+age+generation envelope и borrow lifetimes. %M остаётся application memory.
2. Реализовать last-qualified/sample-at-boundary/queued-event варианты только по declared policy и bounded storage.
3. На trap отменять working batch; на stale/replacement менять quality и применять policy, не подставлять good zero.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W11-A01 | Ingress меняется посередине task | Task видит coherent выбранную версию; value и quality не из разных samples. |
| W11-A02 | Module заменён на том же slot/IP; delayed completion | Чужая identity/session отвергнута, outputs требуют requalification. |
| W11-A03 | Queue overflow или task fault после части writes | Gaps/quality отражены; незавершённый batch не опубликован. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Новые channels увеличивают admitted capacity; freeze/seal не получают ветки по vendor.

**Не принимать:** Provider меняет frozen image или UI видит half-old/half-new snapshot.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
