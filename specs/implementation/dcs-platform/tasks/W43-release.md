# W43. Release case, FAT/SAT, support и lifecycle

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P6/P7 |
| Зависимости до интеграции | [W15](W15-single-slice.md), [W18](W18-deployment.md), [W21](W21-retain-reset.md), [W22](W22-firmware.md), [W29](W29-security.md), [W30](W30-engineering-api.md), [W34](W34-dcs-data.md), [W35](W35-package-sdk.md), [W36](W36-operations.md), [W37](W37-iec-libraries.md), [W42](W42-budgets.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §16, §22, §23 |
| Сценарии участия | T28, T61, T76, T85, T88, T92 |
| Primary evidence owner | Собственные критерии ниже; участие в общих сценариях не заменяет их primary owner. |

## Задание LLM

Собрать проверяемый release package для точного profile tuple с применимыми G01–G10 и отрицательными capability claims.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/justfile](../../../../compiler/justfile)
- [.github/workflows/integration.yaml](../../../../.github/workflows/integration.yaml)
- [.github/workflows/deployment.yaml](../../../../.github/workflows/deployment.yaml)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

Qualified относится к build+board+OS/BSP+config+workload+I/O+failure/security scope. Без W28 полный HA release запрещён; новый port требует своего task evidence.

**Разрешённая область изменений:** Release CI/packaging/manifests/evidence/runbooks; gates не ослаблять. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Reproducible signed artifacts/SBOM/licenses/provenance/version compatibility и upgrade/rollback policy. Проверять release bytes, не только debug build.
2. FAT/SAT и manufacturing: power/brownout/endurance/environmental/EMC/electrical по изделию; commissioning/replacement/support runbooks.
3. Вести vulnerability intake, supported lifetime, patch qualification и evidence review. BLOCKED/NOT_RUN/NOT_SUPPORTED не превращать в PASS.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W43-A01 | Невыполненный применимый T/G либо нет physical evidence | Release admission deny; dashboard completeness не сертификат. |
| W43-A02 | Заявлено HA, но authority tuple отсутствует | Profile reject; можно выпустить отдельно прошедший SINGLE scope. |
| W43-A03 | Installed release package и backup/recovery exercise | Идентичные qualified bytes, documented recovery, redacted support bundle. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Новая product tuple повторно использует gates, но получает собственную qualification; версия по маркетингу не заменяет evidence.

**Не принимать:** Тестовый prototype назван production по количеству tests; SIL/MTBF обещаны без lifecycle/data.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
