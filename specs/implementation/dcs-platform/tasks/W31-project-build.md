# W31. Проект, manifests, libraries и воспроизводимая сборка

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P6 |
| Зависимости до интеграции | [W07](W07-semantic-schema.md), [W08](W08-catalog.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §9, §16 |
| Сценарии участия | T42, T54, T59 |
| Primary evidence owner | Собственные критерии ниже; участие в общих сценариях не заменяет их primary owner. |

## Задание LLM

Сохранить единый compiler pipeline и определить versioned project model, library lock и provenance для всей tool ecosystem.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/project/src/project.rs](../../../../compiler/project/src/project.rs)
- [compiler/project/src/compile.rs](../../../../compiler/project/src/compile.rs)
- [compiler/project/src/sidecar.rs](../../../../compiler/project/src/sidecar.rs)
- [compiler/sources/src/lib.rs](../../../../compiler/sources/src/lib.rs)
- [specs/design/per-pou-code-artifacts.md](../../../../specs/design/per-pou-code-artifacts.md)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

Workstation хранит sources/Pending Edits; target получает verified ApplicationGeneration. .iplc — контейнер, не альтернативный язык или отдельный owner POU.

**Разрешённая область изменений:** project/sources/manifests/lock/provenance и frontend adapters; не отдельный codegen для GUI. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Определить canonical build inputs: sources, stable IDs, compiler/options, task/binding/profile refs, library versions/hashes. Node-local deployment config отделить.
2. Если нужны directory/container формы проекта, они раскрываются в одну logical model. Archive limits/path traversal/ordering проверять до compiler.
3. Сравнение builds использовать pinned inputs и независимость от checkout path; signatures/time metadata отделить от deterministic payload identity.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W31-A01 | Один проект на двух paths/Windows и Linux | Canonical artifact identity совпадает при одинаковом declared toolchain/numeric profile либо различие точно объяснено. |
| W31-A02 | Library изменена без lock/hash update | Build/admission reject, silent substitution невозможна. |
| W31-A03 | Rename файла/POU и archive traversal | Stable IDs сохранены по policy; malicious extraction отклонена. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Новый frontend/packaging adapter вызывает project::compile; отдельный pipeline запрещён.

**Не принимать:** Build version/tag заменяет content identity; POU packaging объявлен уже реализованным без source evidence.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
