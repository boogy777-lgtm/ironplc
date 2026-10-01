# IronPLC: задания на реализацию production PLC/DCS ecosystem

Комплект переводит [спецификацию v3.0](../../design/dcs-plc-production-platform-spec-ru.md) в **44 задания для LLM**, зависимости, модульные границы и доказательства приёмки. Подготовлен 2026-09-23, завершён 2026-09-26. Source baseline: [`0fbc8e4`](https://github.com/boogy777-lgtm/ironplc/tree/0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3); вершина исходной ветки повторно проверена 2026-09-26 и не изменилась.

Это implementation dossier. Все задания имеют статус **NOT_STARTED**. Здесь не заявлены выполненные Rust tests, model checking, hardware timing или production readiness.

## Читать от общего к частному

1. [Фактический baseline и пробелы](01-repository-audit.md): что есть в коде и что нужно изменить.
2. [Модульный каркас](02-modular-framework.md): owners, dependency DAG, ports, plugins, границы процессов.
3. [Контракты и реестр состояния](03-contracts-and-state.md): что каждый owner хранит и кому что разрешено.
4. [Порядок реализации](04-roadmap-and-dependencies.md): сквозные milestones, prerequisites и первые PR.
5. [Доказательства и правила LLM](05-evidence-and-llm-contract.md): definition of done, методы и запреты ложной приёмки.
6. [Трассируемость](06-traceability.md): primary owners всех T01–T95, INV01–INV30, S01–S12, 16 REQ-PORT, DA01–DA28 и G01–G10.
7. [Проверка N+1](07-n-plus-one.md): одинаковый класс, допустимый diff и реальное расширение.
8. [Параметры и блокеры](08-profile-inputs.md): какие измерения и решения нужны для qualification.
9. [Миграция старых решений](09-legacy-migration.md): сохранить механизмы, устранить противоречия.
10. [Адвокат дьявола](10-adversarial-review.md): контрпримеры на границах компонентов.
11. [Стартовый запрос для LLM](11-start-here.md): как передавать комплект порциями.
12. [Проверка комплекта](12-document-verification.md): выполненные проверки документов и ограничения.

Текущую норму задаёт спецификация; этот комплект её декомпозирует. Новые Rust module/crate paths — предложения размещения. При расхождении исправить dossier либо оформить изменение нормы, не молча менять механизм.

## Что означает «всё — модуль или плагин»

Каждая ответственность имеет модуль и контракт. Варианты подключаются в предусмотренных местах: static provider, descriptor/policy package, IEC library или external API client. Произвольная hot loading Rust binary в RT scan не требуется. Один модуль не обязательно равен crate, thread, process, firmware image или actor.

Расширение того же класса добавляет implementation/configuration и evidence к существующему механизму. Новая гарантия изоляции, multi-master или SIS — отдельный класс архитектуры.

## Каталог заданий

| ID | Результат | Этап |
|---|---|---|
| [W01](tasks/W01-baseline.md) | Закрепить baseline и разрешить архитектурные расхождения | P1 |
| [W02](tasks/W02-contracts.md) | Типизированные контракты, идентичности и исходы операций | P1 |
| [W03](tasks/W03-composition.md) | Модульный каркас и admission профиля | P1 |
| [W04](tasks/W04-verification.md) | Каркас доказательств и воспроизводимый simulator | P1 |
| [W05](tasks/W05-execution-session.md) | Непрерывная ExecutionSession и IEC scheduler | P1 |
| [W06](tasks/W06-execution-budget.md) | Явное время и ограниченное исполнение | P1 |
| [W07](tasks/W07-semantic-schema.md) | StateSchema, StableStateId и compiler/container bridge | P1/P3 |
| [W08](tasks/W08-catalog.md) | ApplicationGeneration, catalog, pins и resource admission | P1/P3 |
| [W09](tasks/W09-mode-key.md) | ModePolicy, HW_KEY и запрет скрытого Start | P1 |
| [W10](tasks/W10-binding.md) | Topology, DeviceDescriptor и BindingResolver | P1/P2 |
| [W11](tasks/W11-process-image.md) | IoCycle, ExternalData и coherent snapshots | P2 |
| [W12](tasks/W12-effects.md) | EffectGate, force overlay и конечный sink | P2 |
| [W13](tasks/W13-linux-boot.md) | Linux composition, boot и независимое containment | P2 |
| [W14](tasks/W14-ethernet-ip.md) | Первый реальный EtherNet/IP provider | P2 |
| [W15](tasks/W15-single-slice.md) | Сквозной PLC-SINGLE acceptance | P2 |
| [W16](tasks/W16-exact-activation.md) | PreparedBinding и exact reuse без копирования | P3 |
| [W17](tasks/W17-migration.md) | Явная и ограниченная state migration | P3 |
| [W18](tasks/W18-deployment.md) | Единый deployment lifecycle и reconciliation | P3/P4 |
| [W19](tasks/W19-capture.md) | Общий StateView и MutationCapture | P3 |
| [W20](tasks/W20-durable-store.md) | DurableRecordStore и crash-consistent roots | P1/P4 |
| [W21](tasks/W21-retain-reset.md) | Retention, reset classes и совместимые recovery roots | P4 |
| [W22](tasks/W22-firmware.md) | Firmware units, trial boot и maintenance | P4 |
| [W23](tasks/W23-ha-transport.md) | Bounded HA transport и два независимых links | P5 |
| [W24](tasks/W24-replication.md) | Checkpoint commit, ACK ordering и effect outcome | P5 |
| [W25](tasks/W25-output-authority.md) | Независимый OutputAuthority и RecoveryAdmission | P5 |
| [W26](tasks/W26-roles.md) | RoleCoordinator, takeover и handover | P5 |
| [W27](tasks/W27-ha-deployment.md) | HA deployment, first effect и pair Stop | P5 |
| [W28](tasks/W28-ha-qualification.md) | Квалификация HA на реальном fault envelope | P5 |
| [W29](tasks/W29-security.md) | Security с первого slice: identity, capabilities, ingress | P1/P6 |
| [W30](tasks/W30-engineering-api.md) | Controller API, multi-client и operation receipts | P6 |
| [W31](tasks/W31-project-build.md) | Проект, manifests, libraries и воспроизводимая сборка | P6 |
| [W32](tasks/W32-ide.md) | VS Code: correlation, Match и общий command workflow | P6 |
| [W33](tasks/W33-observability.md) | Fault facts, status projection и bounded telemetry | P1/P6 |
| [W34](tasks/W34-dcs-data.md) | DCS tags, commands, alarms/SOE и historian contracts | P6 |
| [W35](tasks/W35-package-sdk.md) | Device/provider SDK и conformance packages | P6 |
| [W36](tasks/W36-operations.md) | Commissioning, backup/replace/restore и fleet | P6 |
| [W37](tasks/W37-iec-libraries.md) | IEC libraries и эволюция FB state | P6 |
| [W38](tasks/W38-portable-core.md) | no_std closure и platform conformance | P7 |
| [W39](tasks/W39-zephyr.md) | Первый embedded port: Zephyr | P7 |
| [W40](tasks/W40-rt-thread.md) | Порт RT-Thread по тем же contracts | P7 |
| [W41](tasks/W41-ariel.md) | Порт Ariel OS и ограничения capability | P7 |
| [W42](tasks/W42-budgets.md) | Числовой профиль ресурсов и temporal qualification | P2–P7 |
| [W43](tasks/W43-release.md) | Release case, FAT/SAT, support и lifecycle | P6/P7 |
| [W44](tasks/W44-n-plus-one.md) | N+1 challenge и независимое расширение каркаса | P6/P7 |

## Использование и хранение

Передавать LLM один work package, общие правила и его dependency contracts. Одна карточка может потребовать несколько небольших PR; код всех 44 задач не поручается одной неподконтрольной итерации. [Шаблоны](templates/task-contract.md) задают handoff/review/evidence, а не дополнительный runtime framework.

Каталог `specs/implementation/` сохраняет запрошенные пользователем долгоживущие задания и acceptance contracts. Он не заменяет временные implementation plans каждого code PR и не является публичным Sphinx reference. Исходные ADR и спецификация не объявлены реализованными от добавления этих файлов.
