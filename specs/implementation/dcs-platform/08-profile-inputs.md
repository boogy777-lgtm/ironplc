# 08. Что надо определить до qualification

Архитектурную реализацию можно выполнять сейчас. Числовые гарантии присваиваются конкретному profile. Здесь **нет придуманных defaults** для production: каждый незаполненный обязательный параметр делает соответствующую capability unqualified.

| Input | Ответственный task | Приёмка значения | Что блокирует |
|---|---|---|---|
| CPU/board/RAM/flash/MMU, OS/BSP/driver/toolchain/config versions | W13/W39–W41 | Exact tuple и artifact hashes | Любое TARGET/HIL claim |
| Tasks/periods/deadlines/phases/budgets, max code/state/I/O | W05/W06/W42 | Workload + schedulability/measurement | G03 и Start workload admission |
| D_revoke, D_stop, D_fallback, max input/output age | W09/W12/W42 | Process envelope + target/receiver measurements | G01/G04; не выбирать автоматически 100 ms |
| Active/candidate/rollback/migration/capture/HA/security/trace memory peak | W03/W08/W42 | Reservation и max concurrency pressure | No allocation failure claim |
| Prepare/quiescence/commit/pause/retirement budgets | W16/W17/W42 | Раздельные measurements; max state | Online-edit timing |
| Logical timer downtime, numeric/FP/overflow semantics | W07/W37 | Declared semantics и regression vectors | Exact/HA semantic compatibility |
| External authority/sinks/bypass closure и failure domains | W25/W28 | Конкретная аппаратная схема + enforcement tests | Полный DCS-HA |
| Detection/lease/fence/checkpoint/restore/connection/first acceptance, RTO/RPO | W23–W28/W42 | End-to-end measurements под заявленной нагрузкой | HA availability; не универсальные 10–30 ms |
| Dual optical paths и authority path при отказе Primary | W23/W28 | Shared dependency audit + hardware faults | Link independence |
| Store atomicity/power-fail model/fs cache или flash geometry/endurance | W20/W21 | Crash matrix + actual power-cut scope | Durable/RPO и G05 |
| Reset/key/force policy, automatic recovery conditions | W09/W12/W21/W27 | Approved product/process policy | Hidden Start/force transfer |
| Trust root/provisioning/entropy/rotation/time uncertainty | W29 | Threat model + verified safe APIs | G07 |
| App/runtime/firmware/bootloader/HA schema compatibility | W22/W43 | Version-pair test matrix | Rolling/rollback claim |
| Tag/event identity, ack owner, quality/gap/time profile | W34 | Reference DCS conformance | G09 |
| FAT/SAT/environment/power/EMC/endurance scope | W43 | Изделие/рынок/process-specific validation plan | G10 |

## Механизм blockers

Blocker record содержит ID, affected task/capability/profile, missing fact/evidence, источник требования, ответственного и проверяемый exit condition. Пример: `HA-AUTH-01`: не выбран sink с enforceable fencing; W25 model/software work продолжает выполняться, full DCS-HA admission закрыт до whole-group hardware tests.

Не нужны upfront ответы пользователя на все строки. LLM реализует portable modules, fixtures и явный profile schema; значения берутся из выбранной аппаратуры и qualification. Узкий вопрос задаётся тогда, когда конкретный выбор действительно определяет следующую реализацию. Непроверенную функцию нельзя назвать supported только ради заполненной таблицы.
