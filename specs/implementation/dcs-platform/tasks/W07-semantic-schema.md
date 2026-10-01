# W07. StateSchema, StableStateId и compiler/container bridge

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P1/P3 |
| Зависимости до интеграции | [W02](W02-contracts.md), [W04](W04-verification.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §9, §17 |
| Сценарии участия | T54, T59 |
| Primary evidence owner | T54, T59 |

## Задание LLM

Связать существующие stable variable/FB IDs с полной semantic schema и проверяемой StateAddressMap.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/project/src/sidecar.rs](../../../../compiler/project/src/sidecar.rs)
- [compiler/project/src/compile.rs](../../../../compiler/project/src/compile.rs)
- [compiler/container/src/type_section.rs](../../../../compiler/container/src/type_section.rs)
- [compiler/container/src/header.rs](../../../../compiler/container/src/header.rs)
- [compiler/runtime/src/migration.rs](../../../../compiler/runtime/src/migration.rs)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

Project отвечает за устойчивость IDs, compiler выпускает descriptors, target проверяет. Layout hash и width не подменяют semantic type. IEC syntax остаётся стандартным.

**Разрешённая область изменений:** project/sidecar, compiler schema emission, container format/verifier и runtime schema reader; не alternate compiler pipeline. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Задать canonical descriptors: nested instances, arrays/strings, reset/time/storage class и versioned FB semantics. Проверить uniqueness/bounds/alignment/non-overlap.
2. Version wire format, fixed widths/endian, content integrity и compatibility. Old readers явно reject unsupported mandatory sections.
3. Дать state-delta report: Reuse/Convert/Initialize/Drop/Reject. Раскрыть semantic scope таймеров, edge memory, PID и task history.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W07-A01 | Rename/reorder при тех же IDs/types | Состояние сопоставлено корректно независимо от нового layout. |
| W07-A02 | DINT→TIME того же размера; I32→F32 для 16777217 | Нет ложного exact; loss/change явно требует policy. |
| W07-A03 | Duplicate ID, nested overlap, forged lengths и cross-endian fixture | Target reject до execution; bounded parse, без native Rust layout. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Новый поддержанный FB descriptor использует одну schema/evolution model; новый memory class требует явного расширения контракта.

**Не принимать:** Сравниваются только byte size/hash; ID генерируется заново при каждом build.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
