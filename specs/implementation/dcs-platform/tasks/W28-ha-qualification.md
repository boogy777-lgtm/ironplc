# W28. Квалификация HA на реальном fault envelope

| Поле | Значение |
|---|---|
| Статус реализации | NOT_STARTED — это задание, не отчёт о готовности |
| Этап | P5 |
| Зависимости до интеграции | [W15](W15-single-slice.md), [W22](W22-firmware.md), [W27](W27-ha-deployment.md), [W42](W42-budgets.md) |
| Основание | [Спецификация v3.0](../../../design/dcs-plc-production-platform-spec-ru.md), §11, §18, §19, §20, §22 |
| Сценарии участия | T14, T16, T19, T76, T77, T80 |
| Primary evidence owner | T76 |

## Задание LLM

Подтвердить HA свойства на exact hardware/software/I/O tuple и закрыть границу между model/software tests и physical effects.

Перед работой прочитать [правила исполнения](../05-evidence-and-llm-contract.md) и [каркас](../02-modular-framework.md). Номер W — identity, а не последовательность запуска; зависимости обязательны для интеграции. Открытые hardware inputs не мешают portable реализации, но блокируют её квалификацию.

## Исходный код и документы

- [compiler/ironplc-redundancy/src/simulator.rs](../../../../compiler/ironplc-redundancy/src/simulator.rs)
- [specs/design/dcs-plc-production-platform-spec-ru.md](../../../../specs/design/dcs-plc-production-platform-spec-ru.md)

Baseline источников — `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Перед изменениями сверить текущий SHA и semantic diff; перечисленные файлы являются точками чтения, а не заявлением, что целевой модуль уже существует.

## Владение и границы

Verification package не выдаёт authority и не меняет guards ради успешного теста. Непокрытые fault combinations остаются exclusions.

**Разрешённая область изменений:** Models/test fixtures/fault rigs/HIL evidence; не ослабление tested guards. Любое расширение затрагиваемых owners/contracts сначала объяснить в change-impact record. Имена новых модулей закрепляются по [карте реализации](../02-modular-framework.md), без создания пустых crates.

## Последовательность реализации

1. Использовать внешний timestamp/output acceptance observer и управляемые power/link/authority/storage faults. Сохранять raw captures и calibration.
2. Проверить 0/0 live/dead Primary, partial fencing, old replay, standby reboot, trial/finalize, non-idempotent lost ACK и common-mode software fault.
3. RTO считать до первого accepted output: detection+expiry+fence+restore+connection+acceptance; RPO и degraded age отдельно.

## Доказательство результата

| ID критерия | Воздействие | Независимый наблюдаемый oracle |
|---|---|---|
| W28-A01 | Полное отключение питания Primary рвёт оба optical paths | Takeover укладывается в declared RTO только при qualified checkpoint/authority; physical capture подтверждает. |
| W28-A02 | Authority/sink недоступен | Нет split-brain; fallback по profile, availability claim честно ограничен. |
| W28-A03 | Совместная нагрузка edit+HA+trace+IRQ | Admission либо measured bounds сохранены; percentile не заменяет hard bound. |

Для каждого критерия указать METHOD, точную команду/fixture, profile/build, numeric bounds по применимости, полученный outcome и artifact hash. [Уровни evidence](../05-evidence-and-llm-contract.md) не взаимозаменяемы. Эти локальные IDs пока не зарегистрированы как Rust spec tests; связь с REQ оформляется в реализации через W04.

**N+1:** Новая topology/version tuple повторяет contract suite и HIL; старое evidence автоматически не наследуется.

**Не принимать:** Pass по enum Primary или simulator time; сертификат reliability/SIL из одного soak run.

## Передача результата

Передать scoped PR, component/state/contract record, tests/trace/model evidence и [отчёт](../templates/evidence-report.md). Указать implemented/verified/qualified раздельно. Если аппаратуры или допустимого API нет, вернуть работающую доступную часть и конкретный BLOCKED item; не ставить PASS. Следующие задачи получают versioned contracts, fixtures и known limits, а не только текст «готово».
