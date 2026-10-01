# W22. Firmware units, trial boot и maintenance

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P4 |
| Зависимости до интеграции | [W13](W13-linux-boot.md), [W20](W20-durable-store.md), [W29](W29-security.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §12, §13 |
| Сценарии участия | T21, T34, T68, T85, T90 |
| Primary evidence owner | T21, T34, T68, T85, T90 |

## Задание LLM

Спроектировать и реализовать firmware transaction отдельно от ApplicationGeneration activation; закрепить recovery root и qualification matrix.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/vm-cli/src/slot_store.rs](../../../../compiler/vm-cli/src/slot_store.rs)
- [specs/design/linux-controller-architecture.md](../../../../specs/design/linux-controller-architecture.md)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

FirmwareUpdater владеет update operation, bootloader — boot selection/security floor. Runtime binary, OS/firmware и bootloader не обновляются application hot edit.

**Разрешённая область изменений:** Firmware updater/boot ports/target packaging; не ApplicationGeneration API для runtime binary. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Описать target layouts, signed manifests, dependencies и maintenance/rolling conditions. Выбрать реальный boot backend под target, а не обязательный общий A/B для всех ОС.
2. Stage→verify→durable intent→stop/handover→trial boot→health→confirm/revert. Health не требует наличия user app.
3. Power-cut steps, bounded retries, anti-rollback floor и recovery image order. Без доказанного bootloader recovery field update capability закрыта.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W22-A01 | Trial не bootable или confirm reply потерян | Ограниченный recovery к допустимой confirmed image/режиму, outputs закрыты. |
| W22-A02 | Security floor поднят в неправильный момент | Тест выявляет потерю единственного recovery path; реализация не подтверждается. |
| W22-A03 | Unknown mixed HA firmware либо binary update через app API | Admission reject; применима только квалифицированная maintenance procedure. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Ещё одна image использует versioned update-unit dependencies/boot port; не общий active-bank flag с application RAM.

**Не принимать:** HA названа гарантией любого rolling upgrade; поломанный bootloader восстанавливается обычным daemon.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
