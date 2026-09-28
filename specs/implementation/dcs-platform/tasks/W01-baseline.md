# W01. Закрепить baseline и разрешить архитектурные расхождения

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P1 |
| Зависимости до интеграции | Нет; начать здесь. |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §2, §3, §22 |
| Сценарии участия | Baseline/ADR и G02/G08; новые functional passes не заявляются. |
| Primary evidence owner | Собственные критерии ниже; участие в общих сценариях не заменяет их primary owner. |

## Задание LLM

Согласовать действующий код, ADR и норму v3.0 до функциональных изменений. Подготовить карту сохраняемых механизмов и заменяемых контрактов.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [AGENTS.md](../../../../AGENTS.md)
- [specs/steering/glossary.md](../../../../specs/steering/glossary.md)
- [specs/adrs/0052-online-change-performed-by-the-runtime-host.md](../../../../specs/adrs/0052-online-change-performed-by-the-runtime-host.md)
- [specs/adrs/0064-online-change-on-a-redundant-pair.md](../../../../specs/adrs/0064-online-change-on-a-redundant-pair.md)
- [specs/adrs/0065-engineering-session-exclusivity-and-ide-side-pending-edits.md](../../../../specs/adrs/0065-engineering-session-exclusivity-and-ide-side-pending-edits.md)
- [specs/adrs/0066-linux-execution-platform-and-global-epoch.md](../../../../specs/adrs/0066-linux-execution-platform-and-global-epoch.md)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

Владелец — архитектурный baseline, без нового runtime owner. Не переписывать историю ADR и не менять код ради соответствия старой формулировке.

**Разрешённая область изменений:** ADR/glossary и baseline audit; код — только отдельными последующими заданиями. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Зафиксировать commit, dirty state и ветку реализации. Сравнить фактические public API с §2/3 и аудитом комплекта.
2. Для A01–A10 определить сохраняемые части, superseding ADR и impacted callers/tests. Отдельно: epoch, 0/0, multi-client, exact reuse и immutable pool.
3. Номера ADR брать из актуального дерева. Статусы отражают выполненную работу; согласованное намерение не выдавать за реализованный код.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W01-A01 | Старое accepted ADR противоречит v3.0 | Есть явная цепочка supersession и задача миграции; обе нормы не применяются одновременно. |
| W01-A02 | Новый разработчик выбирает main вместо рабочей ветки | Baseline check фиксирует несовпадение SHA до изменений. |
| W01-A03 | Исторический тест закрепляет прежний 0/0 запрет | Тест классифицирован как legacy-policy либо заменяемый; удаление обосновано новой нормой. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Следующее расхождение добавляет строку реестра и superseding решение; не новый слой управления.

**Не принимать:** Просто переименованы enums или переписана спецификация под удобство текущего кода.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
