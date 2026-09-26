# W35. Device/provider SDK и conformance packages

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P6 |
| Зависимости до интеграции | [W10](W10-binding.md), [W14](W14-ethernet-ip.md), [W31](W31-project-build.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §8, §16, §17, §21 |
| Сценарии участия | T34, T60, T69, T79 |
| Primary evidence owner | Собственные критерии ниже; участие в общих сценариях не заменяет их primary owner. |

## Задание LLM

Сделать повторяемый путь добавления N+1 устройства/provider через descriptors, safe ports и conformance suite.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/sources/src/lib.rs](../../../../compiler/sources/src/lib.rs)
- [compiler/project/src/project.rs](../../../../compiler/project/src/project.rs)
- [specs/design/compatibility-library-format.md](../../../../specs/design/compatibility-library-format.md)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

Descriptor package — данные; native provider — qualified code в firmware/process update unit. Установка descriptor не загружает Rust dylib в scan.

**Разрешённая область изменений:** Device/provider package schemas/SDK/examples/conformance; не runtime native dylib loader. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Schema package: identity/version/hash/signature/dependencies/channels/types/units/limits/fallback/firmware compatibility/capabilities.
2. Опубликовать reference provider template с safe lifetime/cancel/restart/quotas/error contract и tests against virtual+real endpoints.
3. Проверять dependency lock, unsupported mandatory fields, capability downgrade и license/provenance. Пакеты доступны offline; marketplace не обязателен.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W35-A01 | Новый AI module на существующем provider | Добавляются descriptor/config/tests; VM и policy core неизменны. |
| W35-A02 | Package подписан, но fallback/fence capability недостаточна | Profile admission reject; подпись не подменяет conformance. |
| W35-A03 | Provider upgraded с ABI/timing изменением | Firmware maintenance/requalification, не скрытый application hot edit. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Второй независимый разработчик подключает образец по SDK без правок core; diff и suite — обязательное evidence.

**Не принимать:** Один универсальный Plugin trait с raw state access и mutable global registry.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
