# IronPLC / RegulBUS: задание на архитектуру firmware DCS

> Owner-provided reference document (v2.0, 2026-09-21). Preserved verbatim for audit:
> [historical audit](https://github.com/boogy777-lgtm/ironplc/blob/8a7f6d0d09b00436daebfd669babd39f2e0f2e13/specs/design/dcs-firmware-task-audit.md). Not a project spec — a requirements task
> against which the project architecture is audited.

Версия: 2.0 — после проверки «адвокатом дьявола».
Дата: 2026-09-21.
Базовая платформа для примеров: FreeRTOS. Второй порт: Linux с квалифицированным профилем исполнения.
Статус: исправленное задание на проектирование и верификацию, **не свидетельство готовности продукта к эксплуатации**.

## Как использовать документ

Передать документ целиком LLM, системному архитектору или команде разработки. Часть A фиксирует найденные дефекты исходного задания. Часть B — самодостаточное исправленное задание. Часть C задаёт проверку результата и допуск к production.

Проектируется firmware контроллера для DCS/BPCS: программная платформа, PLC runtime, управление приложениями, I/O, резервирование и эксплуатационные контракты. Это не проект всей DCS, включая операторские станции и historian, и не спецификация сертифицированной SIS.

Требования ниже — проектные решения и требования к будущим доказательствам. В этом документе не проводились аудит исходников IronPLC, испытания аппаратуры или model checking. Существующему upstream IronPLC перечисленные возможности не приписываются.

Термины **MUST**, **MUST NOT**, **SHOULD** обозначают соответственно обязательное требование, запрет и рекомендацию с документируемым исключением. `TBD` разрешён при проектировании, но не в обязательных параметрах выпускаемого production-профиля.

---

# Часть A. Результат проверки «адвокатом дьявола»

## A.1. Вердикт

Декомпозицию HW_KEY / OS / HW_DIAG / RUNTIME / APPLICATION сохранить. Исходное задание, однако, нельзя считать достаточным для production DCS: оно описывало логические состояния, но недоопределяло исполнение при отказах, владение выходами, временные ограничения и восстановление.

Ортогональность означает раздельное владение состоянием и отсутствие зависимости от чужой внутренней FSM. Она **не означает** отсутствие взаимодействия, независимость отказов или возможность продолжать программную реакцию после остановки общей ОС.

## A.2. Найденные проблемы и внесённые исправления

`P0` — дефект, способный нарушить управление, эксклюзивность выходов или достоверность гарантий. `P1` — существенный дефект расширяемости, диагностики или эксплуатационной модели.

| ID | Приоритет | Возражение | Исправление |
|---|---|---|---|
| D01 | P1 | «Слои не знают друг о друге» буквально невыполнимо: потребитель обязан понимать смысл входов. | Запрещено знание реализации и внутренних enum; разрешены узкие публичные контракты и направленные зависимости. |
| D02 | P0 | Независимые FSM не являются независимыми failure domains. При остановке RTOS они могут замереть вместе. | Разделены логическое владение, execution context и физический fault-containment boundary. |
| D03 | P0 | `Runtime.RUNNING → os.ready` не может мгновенно сохраняться при асинхронном отказе ОС. | Заданы инварианты на точках принятия решений и ограниченное время реакции; физические выходы защищает отдельный путь. |
| D04 | P0 | HW_DIAG с `execution_safe` / `outputs_safe` незаметно принимает технологические решения. | Диагностика публикует ресурсные факты и достоверность; пригодность выводит владелец конкретного действия по утверждённому профилю. |
| D05 | P0 | Изменение permission не определяет, должен ли поворот ключа запускать PLC. | Разделены ограничения полномочий, intent переключателя, Start request и restart policy. |
| D06 | P0 | Не определено, кто действительно запрещает запись в физические выходы. | Назначен единственный авторитетный output-enforcement boundary, включая доступ по протоколам и режим force. |
| D07 | P0 | Несколько атомарных `bool` не образуют согласованный снимок. | Добавлены версии, boot identity, freshness, совместимость поколений и точка линеаризации решения. |
| D08 | P0 | После `can_start=true` ключ или capability могут быть отозваны до выполнения команды. | Повторная проверка на commit, приоритет revocation и явная ограниченная задержка после commit. |
| D09 | P1 | Запрет bootstrap/orchestration делает порядок запуска неявным. | Разрешены composition root и небольшой boot pipeline; запрещён универсальный управляющий Service Manager. |
| D10 | P0 | Задача внутри той же ОС не гарантирует реакцию на её зависание. | Требуется независимый watchdog / output timeout / аппаратный inhibit с доказанным покрытием отказов. |
| D11 | P1 | Замена RTOS на Linux трактуется как автоматическая эквивалентность real-time. | Контракты и доменная логика переносимы; timing, isolation, драйверы и storage заново квалифицируются. |
| D12 | P0 | Загрузка Candidate может сделать Application «не готовой», хотя Original работает. | Lifecycle моделируется на экземпляр immutable artifact; активное execution binding имеет одного владельца. |
| D13 | P0 | Не разделены режим TEST и Test Edits. | TEST — запрет application-driven физических воздействий; Test Edits — пробное выполнение новой генерации в выбранном режиме. |
| D14 | P0 | Совпадение StableStateId и типов объявляется отсутствием любого скачка процесса. | Гарантия ограничена сохранением state; поведение нового алгоритма и траектория выходов не доказаны этим условием. |
| D15 | P0 | O(1) pointer switch скрывает стоимость ожидания barrier и миграции изменяемого state. | Отдельные бюджеты подготовки, quiescence и commit; миграция обязана доказать согласованность с продолжающимися записями. |
| D16 | P0 | Синхронный резерв ошибочно считается законным владельцем I/O. | Разделены sync readiness, решение о роли и fencing, реально исполняемый на границе записи. |
| D17 | P0 | Правило двух потерянных link противоречит обещанию takeover при любом отказе PRIMARY. | Правило сохранено; отказ с потерей обоих валидных каналов явно исключён из гарантии автоматического takeover. |
| D18 | P0 | `+1000` в sequence counter смешивает номер пакета и статистику потерь. | Протокольный sequence идёт последовательно; потери считаются отдельно. Legacy-индикатор возможен только как UI-проекция. |
| D19 | P0 | EMA по 10/100 измерениям принимается за верхнюю границу latency/jitter. | EMA оставлена как оценка тренда; timeout обосновывается отдельно, с jitter, scheduling delay и запасом. |
| D20 | P0 | Hold-last на 100 ms трактуется как универсальная безопасная мера. | Для каждого output group заданы допустимый age, причина удержания, конечная реакция и процессное обоснование. |
| D21 | P0 | Firmware update смешивается с application online change. | Раздельные транзакции, совместимость версий, recovery после power loss и запрет неподтверждённых rolling updates. |
| D22 | P0 | Ключ и permission-биты подменяют authentication/security. | Физические ограничения пересекаются с авторизацией пользователя и сессионными/транзакционными проверками. |
| D23 | P1 | Минимизация M(N) провоцирует один универсальный механизм для разных инвариантов. | Определён same-class test; новый класс гарантий может законно добавлять механизм. |
| D24 | P0 | «Production-ready» выводится из красивых FSM и Rust. | Введены измеримые release gates: fault injection, timing, recovery, cybersecurity, FAT/SAT и закрытие блокеров. |

Замечания закрыты **на уровне задания**: вместо недоказуемых обещаний введены обязанности проектировщика и критерии проверки. Реализационное закрытие P0 возможно только после получения соответствующих доказательств.

---

# Часть B. Исправленное задание для LLM / архитектурной команды

## 1. Миссия, границы и ожидаемый результат

Ты — системный архитектор firmware промышленного PLC/DCS-контроллера. Спроектируй production-oriented архитектуру на Rust с ортогональными FSM/statechart и контрактными границами. Примеры исполнения дай для FreeRTOS; покажи замену платформы на Linux без изменения доменных правил.

Не начинай с кода. Сначала определи онтологию, владельцев состояния, классы отказов, полномочия и временные обязательства. Затем выведи контракты, автоматы и реализационный профиль.

В состав результата входят:

- пять основных контекстов HW_KEY, OS, HW_DIAG, RUNTIME, APPLICATION;
- границы bootstrap, I/O enforcement, application deployment, firmware maintenance и redundancy;
- управление конфигурацией, persistent state, engineering requests, диагностикой и аудитом;
- правила переноса RTOS → Linux и квалификации платформ;
- доказуемые инварианты, сценарии отказов, тесты и production release gates.

Нельзя объявлять всю DCS безопасной на основании надёжности одного контроллера. Технологические interlock, безопасные положения исполнительных механизмов и допустимое время потери управления задаёт анализ конкретного объекта. Firmware обеспечивает проверяемое исполнение этих требований.

Этот профиль не заявляет SIL и не заменяет SIS. Для safety-related применения нужен отдельный lifecycle и обоснование по применимым стандартам. Область IEC 61511 — жизненный цикл SIS в процессной промышленности; она не сводится к выбору языка или FSM.

## 2. Зафиксированные ограничения проекта

Следующие решения сохраняются; нельзя молча менять их ради удобства архитектуры.

1. Нет универсального `ServiceManager`, `SystemManager` или аналога, знающего все внутренние состояния и управляющего всем жизненным циклом.
2. Пользовательский язык и переменные остаются IEC 61131-3. Идентичности state, конфигурация deployment и метаданные контрактов находятся вне нестандартного синтаксиса переменных.
3. Active code immutable. Для application hot edit используются два code bank; полная Candidate generation готовится в spare bank с ограниченным бюджетом.
4. Runtime — конечный исполнитель проверки совместимости и переключения; IDE не может навязать ему небезопасное состояние.
5. Для exact-match state используется тот же LiveStateStore; переключение root не требует полного копирования state на barrier.
6. Структурные изменения требуют явного MigrationPlan и доказанного протокола согласованности.
7. Роли резервирования называются `PRIMARY` / `SECONDARY`; статусы синхронизации — `SYNC_*`.
8. Redundancy использует два выделенных optical sync-link, отдельно от engineering, SCADA и технологического I/O.
9. На наблюдённом состоянии `!L1 && !L2` SECONDARY теряет право автоматического takeover. Действующий PRIMARY не прекращает application только по этой причине.
10. EtherNet/IP рассматривается как I/O-интеграция. Нельзя предполагать, что протокол автоматически обеспечивает fencing, сохранение соединений или нужное время восстановления.

При обнаружении несовместимости требований покажи её и возможные решения. Не объявляй несовместимые свойства одновременно выполненными. Особенно это касается гарантии takeover при полном исчезновении PRIMARY и обоих sync-link.

## 3. Правило N+1: механизм, а не заплатка

```text
N := required same-class behaviors
M(N) := independent mechanisms required to cover N

prefer min M(N)

same_class(N+1) → prefer M(N+1) = M(N)

same_class(N+1) ∧ M(N+1) > M(N)
    → reconsider abstraction
```

`M` — не число файлов, crates, состояний, адаптеров, потоков, таблиц конфигурации или тестов. Это число независимых способов обеспечить один и тот же класс поведения.

Same-class означает совпадение семантического обязательства, модели полномочий и класса отказов. Если появился новый инвариант — например эксклюзивное владение I/O при сетевом разделении — новый механизм может быть оправдан.

Минимизируй M при сохранении bounded execution, понятных границ ответственности, проверяемости и fault containment. Универсальная event bus, через которую неявно управляется всё, не считается успехом N+1.

Для каждого N+1 теста представь: исходный механизм, новое требование, аргумент same-class/new-class, изменяемые части, неизменяемые части, новые доказательства и изменение M.

## 4. Владение состоянием и механизмами

### 4.1. Основные контексты

| Контекст | Единственный владелец | Публичный результат | Что ему запрещено |
|---|---|---|---|
| HW_KEY | Декодирование, стабильность и валидность физического селектора; аппаратные ограничения | Authority constraints и явный selector intent | Читать runtime enum, ставить Runtime в RUN, выдавать права пользователя |
| OS / Platform | Доступность необходимых платформенных сервисов и квалификация execution domain | Platform evidence/capabilities; узкие platform ports | Знать IEC POU, Test Edits, режим приложения или PRIMARY/SECONDARY |
| HW_DIAG | Результаты диагностики идентифицированных ресурсов и качество evidence | Health records по ресурсам | Объявлять технологический процесс безопасным; выбирать режим Runtime |
| RUNTIME | Actual execution, task/scan state, faults исполнения, активный execution binding и barrier | Execution status, progress, results запросов | Декодировать контакты ключа, управлять RTOS boot, обновлять firmware |
| APPLICATION | Immutable artifacts, их manifest/schema, результаты проверки и жизненный цикл экземпляров | Проверенный artifact handle и compatibility evidence | Дублировать actual execution или владеть изменяемым LiveStateStore |

Название «слой» не означает линейный стек. Это bounded contexts. Отдельная FSM не обязана иметь отдельный thread или process.

### 4.2. Дополнительные границы, необходимые для production

| Граница | Узкая ответственность | Почему это не god object |
|---|---|---|
| Composition root / bootstrap | Создать ресурсы и connections; выполнить зависимые pre-scheduler этапы | Не содержит политику RUN, hot edit, HA или технологических fault |
| I/O output enforcement | Пропускать только допустимые output frames; выполнять timeout/fallback и проверку owner | Владеет только физическим write boundary, а не жизнью всех компонентов |
| Deployment transaction | Stage, пробное переключение, commit/discard application generation | Опирается на контракт barrier; не пишет во внутреннее состояние Runtime |
| Firmware maintenance transaction | Проверить, установить, активировать и восстановить firmware image | Не является операционным режимом PLC и не управляет обычными scan |
| Redundancy | Replication, sync qualification, role transition и запрос ownership | Не может обходить I/O fencing или подменять Runtime state |
| Watchdog enforcement | Контролировать срок доказанного progress и выполнить recovery action | Не обязан знать все FSM; проверяет ограниченный health/progress contract |

Не делай отдельную FSM автоматически для каждого существительного. Новый автомат нужен, если есть собственный temporal lifecycle, различимые права переходов или восстановление после частичного выполнения. Функция-проекция, валидатор или конфигурация сами по себе FSM не требуют.

### 4.3. Что означает ортогональность

Состояние системы представимо произведением локальных состояний, но достижимое пространство — его подмножество, ограниченное контрактами и инвариантами.

```text
RepresentableState = Key × Platform × Health × Runtime × Artifacts × Transactions × HA
ReachableState ⊆ RepresentableState
```

Не перечисляй все комбинации в глобальном enum. При этом ортогональные regions **не устраняют** сложность верификации произведения: проверяй существенные взаимодействия и недопустимые комбинации.

## 5. Контракты: значение, достоверность и срок действия

### 5.1. Разделение понятий

| Понятие | Смысл | Пример |
|---|---|---|
| Fact | Наблюдённое обстоятельство с качеством и возрастом | Контакт селектора замкнут |
| Capability | Подтверждённая возможность с условиями и ограничениями | Доступен квалифицированный cyclic execution profile |
| Authority constraint | Ограничение допустимого действия | Application replacement запрещён аппаратным селектором |
| Intent / request | Запрос на действие | Перейти в RUN |
| Command | Принятый исполнителем запрос с идентичностью и lifecycle | Транзакция изменения режима принята |
| Event | Уведомление о факте изменения | Публикация новой версии diagnostic record |
| State | Авторитетное состояние конкретного владельца | Runtime фактически CYCLIC |
| Projection | Неавторитетный вывод для наблюдения | RUN_DEGRADED |

Публичные request/response API разрешены. Запрещён доступ к чужому внутреннему `set_state(...)`, а не любое межкомпонентное взаимодействие.

### 5.2. Обязательная спецификация каждого контракта

Для каждого контракта задай:

- owner, потребителей, область действия и trust boundary;
- допустимые значения, единицы измерения, идентификаторы и semantic version;
- publisher boot/session identity и монотонную revision внутри этой identity;
- время наблюдения, clock domain, правила freshness и максимальную задержку публикации;
- `Unknown`, `Valid`, `Stale`, `Invalid` либо эквивалентную типобезопасную модель;
- кто оценивает expiry, если publisher больше не работает;
- atomicity, memory ordering, срок жизни snapshot и правила reclamation;
- значения после boot, reboot издателя, потери связи и version mismatch;
- необходимость periodic refresh либо событийной публикации с lease;
- порядок revocation/grant, приоритеты и поведение при переполнении транспорта;
- схемы совместимости с config revision, application generation и schema identity;
- стоимость чтения и публикации; предел объёма данных; диагностику отказа.

Эти поля задаются по смыслу контракта: не заставляй неизменяемый проверенный artifact имитировать heartbeat живого датчика. Для artifact важны идентичность, целостность, совместимость и lifetime pin; для live evidence — freshness.

### 5.3. Никакого boolean soup

Вместо набора разрешений с невозможными комбинациями используй semantic types. Например:

```rust
// Эскиз интерфейса, не готовая реализация и не доказательство lock-free.
enum Evidence<T> {
    Unknown,
    Valid { value: T, stamp: EvidenceStamp },
    Invalid { reason: ReasonCode, stamp: EvidenceStamp },
}

struct EvidenceStamp {
    publisher_boot: BootId,
    revision: Revision,
    observed_at: MonotonicInstant,
    max_age: Duration,
}

enum RequestedMode { Program, Test, Run }

struct HardwareConstraints {
    allowed_modes: ModeSet,
    engineering_ops: OperationSet,
    selector_intent: Option<ModeIntent>,
}

struct ExecutableHandle {
    generation: GenerationId,
    manifest_digest: Digest,
    state_schema: SchemaId,
    compatibility: CompatibilityEvidence,
    // Реализация гарантирует pin/lifetime независимо от имени в каталоге.
}
```

`Stale` может вычисляться потребителем по stamp; publisher не обязан успеть опубликовать его перед собственным отказом. Реальные сообщения MUST иметь ограниченный размер; произвольные строки и контейнеры не переносятся в hard real-time path.

### 5.4. Согласованность без мифа о глобальном snapshot

Атомарное чтение каждого контракта не означает одновременный снимок всех физических источников.

Потребитель формирует свой `DecisionSnapshot`: выбранные версии, quality, age и ссылки на совместимые generations/configuration. Он проверяет отношения совместимости, а не равенство независимых revision.

Для локальной транзакции определена точка commit. Перед ней повторно проверяются критические запреты, identity и preconditions. После неё изменения среды обрабатываются за установленное время. Никакой snapshot не отменяет будущую неисправность.

Clock одного контроллера нельзя напрямую сравнивать с monotonic timestamp другого. Для remote evidence нужны session sequence, локальное время приёма и доказанная freshness-семантика. UTC/PTP/NTP нужны для корреляции по отдельному контракту и не подменяют локальные safety/timeout таймеры.

## 6. Исполнение автоматов и конкурентность

### 6.1. Детерминированная модель

Выбери и обоснуй модель `event + owned state → next state + effects` для каждой FSM. Чистая transition function предпочтительна; effect исполняется через ограниченный контракт и возвращает результат владельцу.

Не требуй общей очереди или одного thread для всех FSM. Для каждой укажи context, WCET обработки события, максимальный backlog, синхронизацию и поведение при overload.

Запреты и trip нельзя передавать только как теряемые edge events. Используй level evidence / latch / срок действия permission с проверкой потребителем. Телеметрия может быть lossy с счётчиком потерь; управляющий запрос должен получить результат либо явную неопределённость, разрешаемую query по ID.

### 6.2. Приоритет и conflict resolution

На одной точке решения приоритет имеет отзыв права или блокирующий fault, затем контролируемый stop, затем grant/start и обслуживающие операции. Для конфликтов maintenance, deployment, switchover и изменения конфигурации нужна явная матрица совместимости.

Операции одного конфликтного домена сериализуются по его владельцу и epoch. Это не даёт такому владельцу право изменять соседние FSM.

Каждый запрос содержит `request_id`, controller boot/session, principal, target, expected revision/generation и ограничение времени действия. Повтор запроса не должен повторно менять мир. Где дедупликация должна пережить reboot или failover, задай durable/replicated journal; без него не обещай exactly-once.

Различай `Rejected`, `Accepted`, `Applied`, `Failed`, `Cancelled` и `OutcomeUnknown`. Потеря ответа не доказывает отмену уже выполненного действия.

### 6.3. Rust и память

Определи bounded ownership для queues, StateStore, code banks и DMA/I/O buffers. Для real-time path запрети неограниченную аллокацию, блокирующие сетевые вызовы, форматирование произвольных логов и неограниченное ожидание mutex.

Использование `Arc`, atomics, lock-free queues или `unsafe` само по себе не доказывает bounded latency. Укажи условия reclamation, отсутствие use-after-free/ABA и предел числа повторов; не допускай unbounded spin на busy publisher.

Документируй panic policy, FFI/DMA границы, обработку stack overflow, corrupted state и поведение после частичного эффекта. Memory safety Rust не доказывает deadline, функциональную корректность или безопасность `unsafe`/драйверов.

## 7. HW_KEY: authority отдельно от желаемого режима

### 7.1. Локальная FSM

Выведи автомат наблюдения: например `UNKNOWN → QUALIFYING → VALID`, с `INPUT_FAULT` при недопустимой комбинации/обрыве. Позиция стабильного ключа — значение локального состояния, не глобальный режим PLC.

Определи debounce, время принятия restrictive/permissive изменений, промежуточные положения, повторный boot и loss of sampling. Нельзя симметричным большим debounce незаметно задержать необходимый отзыв полномочий.

### 7.2. Рекомендуемый профиль для проектирования

Это собственная семантика продукта, не утверждение о внутренней реализации Rockwell или Modicon.

| Позиция | Hardware constraints | Intent | Чего она не гарантирует |
|---|---|---|---|
| RUN_LOCK | Допустим RUN; удалённые изменения режима и кода заблокированы | Запрос RUN после квалифицированного переключения, если профиль разрешает | Не гарантирует готовность application, отсутствие fault или немедленный старт |
| REMOTE | PROGRAM / TEST / RUN разрешены; engineering ops ограничены security policy | Не выдаёт Start сама по себе | Не аутентифицирует инженера |
| PROGRAM_LOCK | Циклическое IEC execution запрещено; программирование разрешается только согласно профилю | PROGRAM / controlled stop | Не задаёт универсальные безопасные значения выходов |
| UNKNOWN / INPUT_FAULT | Новые grants и engineering mutations запрещены | Нет автоматического Start | Реакция на уже работающий процесс определяется отдельным утверждённым fault policy |

Для PROGRAM_LOCK LLM обязана выдать точную матрицу разрешённых операций: имя позиции не определяет права записи автоматически.

Intent обрабатывает Runtime по общему контракту запросов; физический источник не получает прямой доступ к Runtime state. Переключение в RUN, cold boot с уже установленным RUN и восстановление после fault — три разных события с отдельной restart policy.

Инвариант: расширение прав не запускает приложение без принятого intent/request. Emergency protection, отказы платформы и независимый output inhibit не блокируются RUN_LOCK.

## 8. OS / Platform: возможность исполнения, а не управляющий PLC-автомат

### 8.1. Что именно моделируется

OS FSM — это lifecycle платформенной обвязки и наблюдаемая доступность сервисов, а не попытка переписать внутреннюю FSM FreeRTOS/Linux.

Уточни владельцев evidence до scheduler и после него. Если scheduler не стартовал, обычные RTOS tasks не продолжают исполнять FSM. Сохранившиеся значения — последнее известное состояние, а не свежие факты. Для наблюдения отказа нужен ранний fault path, reset reason или независимый наблюдатель.

### 8.2. Узкие порты вместо универсальной POSIX-копии

Выдели только необходимые потребителям интерфейсы, например:

- `MonotonicClockPort`: разрешение, drift, wrap, reset и clock identity;
- `CyclicExecutionPort`: release, deadlines, priorities и overload semantics;
- `BoundedSignalPort`: ограниченная доставка notifications;
- `StoragePort`: atomic publish/durability/error semantics;
- `WatchdogPort`: arm/feed/status с фактическими пределами аппаратуры;
- `NetworkTransportPort`: bounded submit/receive и link/session evidence;
- `PlatformPowerPort`: shutdown/reset request с причиной.

HAL/BSP, storage и network stack могут быть отдельными adapters: FreeRTOS adapter не обязан сам реализовывать всё перечисленное. Зависимости домена направлены на контракты; composition root выбирает реализации.

### 8.3. Execution profile

`realtime_available: bool` недостаточно. Контракт должен ссылаться на квалифицированный профиль: период/дедлайн задач, максимальная wakeup latency, поддерживаемая нагрузка, isolation assumptions, доступная память и ресурсы.

Для Linux определи профиль kernel/config, scheduling policy, приоритеты threads/IRQ, memory locking/prefault, CPU/power policy, I/O interference и запрет неквалифицированной виртуализации. PREEMPT_RT изменяет preemption/locking/interrupt handling; это не доказательство конкретной задержки на выбранной плате.

Одинаковый доменный trace при одинаковых событиях должен сохраняться между портами. Одинаковые timing bounds обязаны подтверждаться отдельно. Непрошедшая квалификацию платформа допускается только в явно обозначенный simulation/development profile, без незаявленного physical control.

## 9. HW_DIAG: evidence о ресурсах

Диагностика выдаёт записи по `ResourceId`: источник, тип проверки, severity, quality, время, latch/recovery condition и затронутую capability. Для двух MCU или N interfaces применяется одна модель записи, а не дополнительные глобальные режимы.

Раздели:

- startup tests, destructive/offline tests и допустимые background tests;
- отказ ресурса и отказ самого диагностического канала;
- исправность, подозрение, подтверждённый отказ и неизвестность;
- transient event, latched condition, acknowledgement и реальное восстановление.

`HEALTHY / DEGRADED / FAILED` допустимы как локальный aggregate, но нельзя приравнивать `DEGRADED` к разрешению продолжать все операции. Потеря резервного NIC и неисправность RAM могут иметь разные последствия при одинаковом уровне сообщения.

Пригодность вычисляет потребитель по declared resource requirements. Runtime оценивает возможность исполнения; I/O enforcement — возможность применения output group; maintenance — возможность безопасной записи storage.

Задай бюджеты background tests: тест RAM/Flash не должен разрушать данные работающего приложения или создавать необоснованный jitter. Результат диагностики не является доказательством отсутствия всех отказов.

## 10. RUNTIME: режим, фактическое исполнение и fault

### 10.1. Раздельные оси внутри одного владельца

Не смешивай выбранный операционный режим, фазу исполнения и причину остановки.

Рекомендуемая исходная модель для проверки:

- `ModeSelection = PROGRAM | TEST | RUN` — принятый режим/цель;
- `ExecutionPhase = STOPPED | STARTING | CYCLIC | QUIESCING | FAULTED`;
- `ExecutionFault` — отдельная запись причины, scope и recovery requirements;
- `ExecutionBinding` — единственная авторитетная ссылка на исполняемую генерацию и state.

Имена и минимальность FSM можно уточнить, но семантическое разделение обязательно. STARTING/QUIESCING нужны только там, где есть реальная асинхронная работа с deadline. Safe point/barrier — точка синхронизации, а не обязательный операционный режим.

| Комбинация | Значение |
|---|---|
| PROGRAM + STOPPED | IEC cyclic tasks не исполняются; communication/diagnostics могут работать |
| TEST + CYCLIC | IEC logic исполняется; её команды не применяются к физическим исполнительным выходам |
| RUN + CYCLIC | IEC logic исполняется; запись выходов всё равно зависит от I/O permissions, health и owner |
| RUN + FAULTED | Выбранный RUN не выполнен из-за fault; это не фактический RUN |
| PROGRAM + CYCLIC | Недопустимая комбинация модели |

Для TEST отдельно задай источник inputs, использование simulated/real inputs, timer semantics и правила всех внешних side effects. Запрет касается не только DO/AO image: POU не должен через протокольный FB обойти TEST и записать реальный привод. Наблюдение реальных inputs возможно только в явно выбранном профиле.

### 10.2. Переходы и guards

START требует одновременно:

1. принятого request/selector intent/restart intent;
2. действующих hardware constraints и полномочий источника;
3. квалифицированного execution profile и пригодных требуемых ресурсов;
4. pinned executable artifact, совместимых schema/config/I/O mapping;
5. выбранного способа initialisation/retain recovery;
6. отсутствия конфликтующей транзакции или blocking fault.

Для допуска application-driven outputs дополнительно нужны RUN, право output ownership и актуальный output permit. Возможность выполнять вычисления не тождественна праву воздействовать на процесс.

Определи шаги RUN → PROGRAM/TEST: запрет новых неподходящих output frames, завершение допустимых текущих работ, bounded quiescence, commit режима, применение соответствующего output policy. Укажи, какие значения/frames могут ещё находиться в транспорте, как они отвергаются и где фиксируется результат.

Если scan завис и barrier недостижим, нельзя бесконечно ждать controlled stop. Истечение deadline передаёт исполнение recovery policy и независимому output-enforcement path. Мгновенное прекращение опасного воздействия не должно зависеть от выполнения зависшей задачи.

### 10.3. Fault lifecycle

Раздели `detected`, `latched`, `acknowledged`, `condition_cleared`, `reset_authorized` и `restart_requested`. Acknowledge не устраняет fault; очистка fault не означает автоматический RUN.

Для recoverable fault нужны восстановленные preconditions, заданная initialisation policy и разрешённый restart. Unrecoverable/corruption fault может требовать reboot/reload; продолжение после panic без доказанного восстановления запрещено.

Для multi-task Runtime определи resource ownership, общий или локальный scope fault и допустимость остановки отдельной task. Независимость tasks нельзя предполагать, если они разделяют state или один output group.

## 11. APPLICATION и application deployment

### 11.1. Один lifecycle на artifact instance

Для каждого artifact выведи автомат, например:

`RECEIVING → VERIFYING → AVAILABLE` либо `REJECTED`, затем контролируемая retirement/reclamation.

`ABSENT` относится к отсутствию подходящего экземпляра/назначения, а не к обязательному состоянию всей подсистемы при каждом upload. Original может быть AVAILABLE и pinned Runtime одновременно с RECEIVING Candidate. Отказ Candidate не делает Original невалидным.

Manifest включает generation identity, hashes, target/runtime ABI, compiler/container versions, semantic state schema, требуемые ports/features, resource limits и I/O mapping identity. Состав полей обоснуй угрозами и compatibility policy.

Artifact verification не доказывает корректность технологического алгоритма. Проверка подписи доказывает происхождение в рамках trust policy, но не заменяет проверку bounds, bytecode и resource admission.

### 11.2. Единственный владелец активной привязки

Runtime владеет `ExecutionBinding`; Application владеет проверенными artifacts; deployment transaction владеет intent и журналом своей операции.

Не поддерживай три независимо изменяемых `active_generation`. В других контекстах может быть только observed binding с revision либо durable committed boot selection, семантически отличающийся от currently executing generation.

### 11.3. Контракт online change

Сохрани согласованную семантику:

| Этап | Где находится изменение | Что исполняет контроллер |
|---|---|---|
| Pending | Engineering workstation | Original |
| Stage / Accept | Candidate проверяется и размещается в контроллере | Original |
| Test Edits | На согласованном barrier выбирается Candidate | Candidate в текущем допустимом режиме Runtime |
| Untest | Повторная проверка и переключение на Original | Original с явно определённой state policy |
| Finalize | Candidate становится committed application | Текущая выбранная генерация; durable policy задана отдельно |
| Discard | Удаляется неисполняемая Candidate / незавершённая подготовка | Исполняемый код не удаляется |

**Test Edits не означает TEST mode.** В RUN новая логика может реально воздействовать на I/O. UI, protocol и audit не должны смешивать эти понятия.

Публичные названия этапов можно менять, но нельзя незаметно менять их смысл. Для каждого этапа определи accepted/applied/durable boundary и восстановление после reboot/HA switchover.

### 11.4. State preservation и предел bumpless-гарантии

Гарантия exact preservation требует StableStateId и точного совпадения semantic type каждого сохраняемого state object; имя, порядок и layout не являются достаточным критерием. Включи nested FB, timers, counters, edge memory, PID/integrators и иное persistent execution state.

Результат существующей классификации `BUMPLESS_PROVEN` MUST сопровождаться scope: **доказано сохранение совместимого state при переключении**, а не отсутствие изменения физических outputs или динамики процесса.

Контрпример: при прежнем state новый код заменяет `Output := 10` на `Output := 90`. Схема state совпадает, но выход меняется. Поэтому отдельно фиксируются state-compatibility result, behaviour-change risk и commissioning/engineering acceptance.

Для missing ID/type mismatch: `NOT_PROVEN` и state-delta report. Явная conversion policy допускается лишь в пределах валидированной MigrationPlan. Подтверждение инженера не снимает требований memory integrity, output ownership и process envelope.

Совпадение типа не делает переносимыми raw pointers, OS handles и timestamps другого boot/clock domain. Такие значения запрещены в переносимом IEC state либо представлены логической identity с проверяемым rebinding. Для timers задай logical execution time / elapsed-time semantics, учёт паузы и downtime. При crossload нельзя сравнивать сохранённый monotonic timestamp PRIMARY с независимым clock SECONDARY без отдельной доказанной модели преобразования.

Semantic state preservation и корректность reference/time-domain binding проверяются отдельно. Иначе одинаковые bytes таймера могут дать совершенно другое время срабатывания после takeover.

### 11.5. Barrier и миграция

Exact-match путь:

1. Полная immutable Candidate generation готова и проверена вне критического barrier.
2. Все затрагиваемые tasks достигают согласованной границы; нет живых execution references на изменяемый binding.
3. StateStore согласован, I/O publication boundary определён, grants/revocations перепроверены.
4. Переключается один immutable binding descriptor, указывающий на code/state/schema/config identity.
5. Старые ресурсы освобождаются только после подтверждённого окончания lifetime; не в неограниченной работе внутри barrier.

O(1) относится к commit descriptor, **не** к ожиданию quiescence, подготовке или очистке ресурсов. Multi-core требует доказанного rendezvous и memory-ordering; lock-free инструкции не отменяют этого требования.

Для Reuse(LiveStateArena) Candidate обязан обращаться к существующим slots по проверенному binding StableStateId → slot либо эквивалентному механизму. Нельзя после изменения compiler layout оставить в новом коде неподтверждённые старые offsets. Binding подготавливается до seal/activation Candidate; Active code не патчится.

Для Rebuild(MigrationArena) проверяются capacity, alignment, bounds, отсутствие недопустимых overlaps, unique identities, расширение nested FB и completeness. Область подготавливается с определёнными начальными значениями; новые объекты получают defaults, сохраняемые и преобразуемые — значения по плану. Неинициализированная память никогда не становится рабочим state.

Для structural migration Runtime продолжает менять LiveStateStore. Поэтому простое копирование объектов в течение нескольких scans некорректно без протокола snapshot + journal/catch-up либо эквивалента. Укажи точку согласованности, верхнюю границу dirty backlog, обработку переполнения и ограничение финального catch-up.

Если доказать bounded migration нельзя, online operation отклоняется; допускается отдельная остановочная процедура. Нельзя обещать произвольную structural migration с нулевой паузой.

Untest/rollback возвращает код, но не отменяет совершённые физические действия. Для совместимого state обычно используется текущий live state; возврат исторического snapshot — отдельная потенциально опасная операция. Если Original schema больше не совместима, нужен проверенный обратный план либо online rollback запрещён.

Два code bank ограничивают число одновременно resident generations. Пока Original нужен для Untest и Candidate выполняется, третий Candidate не допускается без освобождения bank по утверждённой транзакции. Не маскируй нехватку памяти дополнительным неограниченным storage.

## 12. I/O и физическое enforcement

### 12.1. Единственная граница допустимой записи

Runtime формирует output proposal. I/O enforcement допускает его только при выполнении всех условий для конкретного output group. Это логически единственный авторитетный write boundary, хотя технически проверки могут существовать и локально, и в remote I/O.

Frame привязан к controller/boot identity, owner epoch, execution generation, I/O mapping revision, sequence и freshness contract. Конкретный протокол может переносить не все поля: тогда эквивалентность должна обеспечиваться доверенным adapter/endpoint и быть доказана.

Смена поколения или режима инвалидирует не только новые запросы, но и неподходящие queued/replayed frames. Запись из maintenance utility, protocol FB, force и recovery path подчиняется той же политике authority и ownership.

### 12.2. Политика выходов задаётся по группам

| Причина | Что обязательно определить |
|---|---|
| PROGRAM | Заданное значение / разрешённое удержание; длительность и причина выбора |
| TEST | Неприменение application-driven writes; отдельная политика физического канала |
| Execution fault | Немедленная реакция либо ограниченное удержание, согласованное с hazard analysis |
| I/O communication loss | Timeout на модуле, начало отсчёта от последнего принятого frame, конечное действие |
| HA handover | Допустимый промежуток без новых значений, age сохранённого значения, owner handoff |
| Firmware maintenance | Вывод из управления / handover / остановочная процедура |
| Force | Полномочия, scope, срок действия, индикация и поведение при потере сессии |

`safe = 0` не является универсальным правилом. De-energize, hold, ramp или predefined fallback выбираются для конкретной функции. Если нужна независимая защитная функция, она не подменяется обычным DCS timeout.

Hold-time не продлевает срок полномочий старого владельца и не разрешает принимать его новые frames. Удержание последнего значения и право записывать новое — разные свойства.

### 12.3. Отказ CPU/OS

Физические outputs должны получить предписанную реакцию даже при невозможности выполнить следующую инструкцию Runtime. Выбери и докажи один или несколько путей: remote I/O watchdog, аппаратный gate, независимый watchdog/reset с доказанной reset-state электрической схемой, отдельный supervisor.

Независимость проверяется по clock, power, reset domain, wiring и failure coverage. «Отдельная task» не является независимым аппаратным механизмом. После reset также нужно доказать значения выходов, а не только факт перезагрузки CPU.

### 12.4. DCS data semantics

Для inputs/outputs задать value, quality, source identity, age, mapping revision и timestamp semantics. Потеря связи не должна выглядеть как новое достоверное измерение с прежним числом.

Настройки каналов, диапазоны, единицы и mapping обновляются атомарно в пределах объявленного consistency domain. Определи ошибочную установку модуля, replacement module, mismatched config, reconnect и повторное принятие outputs.

## 13. Резервирование: отдельно синхронизация, роль и ownership

### 13.1. Обязательные оси

Не объединяй в один enum role, sync status, link status и право записи. Предлагаемая основа:

- role: PRIMARY / SECONDARY и обоснованные transitional states;
- sync: SYNC_UNQUALIFIED / SYNC_UPDATING / SYNC_READY;
- L1/L2 supervision evidence с freshness и session identity;
- output ownership: отдельный доказанный grant/epoch по output group.

`SYNC_READY` требует совместимых code/schema/config, целого committed checkpoint, допустимого replication lag и валидных prerequisites. Это не просто «сеть подключена».

### 13.2. Зафиксированное правило двух каналов

| Валидные каналы | SECONDARY | PRIMARY |
|---|---|---|
| 1 / 1 | Может быть SYNC_READY при остальных выполненных условиях | Продолжает работу |
| 1 / 0 или 0 / 1 | Degraded path; takeover eligibility может сохраняться | Продолжает работу |
| 0 / 0 | Немедленный отзыв auto-takeover eligibility на точке обработки факта | Продолжает application при сохранении собственных execution/I/O условий |

Под «немедленно» понимается атомарное решение при распознавании `0/0`, раньше обработки start/promotion в том же decision cycle. Физическая неисправность обнаруживается не мгновенно: bound detection должен быть измерен и включён в анализ.

Определи `Li` явно: физический link, валидность peer/session и supervision freshness. Нельзя считать канал валидным бесконечно по старому ACK. При восстановлении одного канала qualification выполняется заново; старый SYNC_READY сам собой не возвращается.

**Неустранимый в рамках данного правила trade-off:** полный power loss PRIMARY может одновременно уничтожить оба канала. После распознавания `0/0` автоматический takeover запрещён. Следовательно, профиль не гарантирует автоматическое продолжение при любом полном отказе PRIMARY. Этот класс отказов должен быть явно указан в availability model и принят владельцем продукта.

Не обходи правило скрытым grace period, cached ready, timeout или неоговорённым третьим witness. Если бизнес-требование требует takeover и в таком случае, это изменение исходной HA policy, требующее отдельного решения пользователя.

### 13.3. Split-brain и точка promotion

Наличие sync-link не доказывает смерть старого владельца. Перед разрешением physical outputs новый PRIMARY должен получить и проверить exclusive ownership на всех обязательных output groups.

Fence должен реально отвергать старого владельца на write boundary. Локальное `role = PRIMARY`, смена IP или неинтерпретируемый модулем epoch такой гарантии не дают.

Задай точку commit роли относительно fencing. Если оба link утрачены до commit автоматической promotion, eligibility отзывается и операция прекращается; частичные grants освобождаются/нейтрализуются. После завершённой promotion контроллер уже PRIMARY, и применяется правило продолжения работы действующего PRIMARY.

Локальный epoch без авторитетного арбитража может совпасть у двух узлов. Определи issuer/fencing authority, reboot identity, replay protection, handover и восстановление после частичного acquire. Если выбранное I/O это не поддерживает, production HA-профиль не принят.

Для EtherNet/IP отдельно докажи supported ownership/connection semantics, transfer либо reconnect, timeout, очереди старого owner и время первого принятого нового output frame. Seamless connection transfer не предполагается.

### 13.4. Replication

Сохрани State Crossload по факту записи: журнал содержит изменяемые state objects, включая запись того же значения. Задай checkpoint boundary, sequence, ACK, commit marker, integrity и восстановление после пропуска.

SECONDARY применяет целый checkpoint, не произвольный префикс. Неполный checkpoint не даёт новую qualification. Journal overflow, неизвестная schema или слишком большой lag снимают SYNC_READY.

Профили FINE / BALANCED / PERFORMANCE различаются частотой согласованных checkpoints; для каждого нужны CPU/network budget и максимальная потеря прогресса. Состояние к моменту takeover может соответствовать последнему checkpoint, а не последней инструкции PRIMARY.

Согласуй ordering checkpoint и I/O публикации. Уже отправленное воздействие нельзя «откатить» возвратом state. Impulse/transactional outputs требуют sequence/idempotency или явно допускаемого поведения при replay; одного state crossload недостаточно.

Online change и role change сериализуются или координируются одним явно заданным deployment/HA протоколом. Обе стороны должны доказать совместимые executing generation, schema и checkpoint; требуются crash recovery и определённое поведение при разрыве на каждом этапе. Это не обещание глобальной атомарности при произвольном partition.

### 13.5. Supervision и калибровка

Протокольный `tx_sequence` увеличивается последовательно в своей session; отдельно ведутся missing/duplicate/out-of-order counters и потери по каждому link. `+1000` можно сохранить только как legacy UI-индикатор, не участвующий в timeout, freshness или fencing.

Ping/pong измеряет RTT и включает обработку/планирование peer. Без обоснованной синхронизации или модели асимметрии нельзя объявлять RTT/2 доказанной односторонней задержкой.

EMA по 10/100 ticks — оценка, не worst-case bound. Калибровка сохраняет topology/profile identity, условия нагрузки, статистику, запас и срок актуальности. После изменения cable/PHY/OS/load profile нужна повторная квалификация либо переход на подтверждённые conservative defaults.

`link_calibrated=false` даёт предупреждение. Default timing разрешён в production только если он квалифицирован для текущего профиля; иначе готовность к takeover не выдаётся. «Vendor values» не существуют автоматически у собственного продукта.

## 14. Firmware update — отдельная maintenance-транзакция

### 14.1. Состав firmware

Версия firmware может менять runtime, OS/BSP/drivers, network/security stack, loader, диагностику, persistent formats, boot/recovery и protocol compatibility. Bootloader при этом может иметь отдельный image/version/update procedure.

Отдели:

- firmware package identity;
- application generation и StateSchema;
- device/project configuration;
- retain/checkpoint data;
- security policy, credentials и trust anchors.

Для пакета задаётся compatibility manifest: hardware revision, bootloader requirements, runtime/application ABI, storage schema, redundancy protocol и I/O feature requirements. Один номер SemVer не заменяет compatibility matrix.

### 14.2. Защита и восстановление

Требуются проверка происхождения и целостности, защищённые trust anchors, ограничение downgrade, recovery path и power-fail consistency. Ориентир — разделение protection/detection/recovery в NIST SP 800-193. Конкретный A/B-протокол ниже — проектное требование, не буквальное воспроизведение NIST.

Предпочтительно staged inactive image с проверкой до activation, trial boot, health confirmation и определённым rollback. Если аппаратная память не позволяет A/B, предложи иной восстановимый механизм и докажи его; не выдавай запись поверх единственной рабочей копии за эквивалент.

Потеря питания на любом durable step должна приводить к прежней либо новой целой допустимой версии или recovery mode, но не к частично смешанной версии. Firmware rollback обязан учитывать уже изменённые persistent formats и security monotonic counters.

### 14.3. Условия выполнения

Update не запускается одним permission-битом. Нужны авторизованный request, аппаратное разрешение, maintenance admission, совместимый пакет, доказанная возможность вывода узла из управления и отсутствие конфликтующих операций.

Maintenance не является режимом ExecutionMode. Если требуется остановить/передать управление, это request к соответствующему владельцу с подтверждением завершения. Потеря management session после durable acceptance не должна оставлять неопределённый полуобновлённый образ.

Rolling update резервированной пары допускается только при отдельно квалифицированных version pairs, schema/replication compatibility и handover. По умолчанию неподдержанная смешанная версия означает запрет online rolling update, а не оптимистичную попытку.

## 15. Cold boot, warm restart и shutdown

### 15.1. Bootstrap — допустимая необходимая последовательность

Спроектируй отдельные стадии:

1. Reset entry: предусмотренное аппаратурой состояние outputs и причина reset.
2. Boot validation/recovery: выбор целого доверенного firmware image по политике продукта.
3. Early platform init: память, clock, минимальная диагностика, начальный watchdog и safe I/O configuration.
4. Composition: создание ресурсов, портов и bounded queues, привязка владельцев.
5. Start execution platform; переход к task-driven диагностике и сервисам.
6. Загрузка и проверка configuration/application/retain с определёнными fallback.
7. Runtime остаётся STOPPED до принятого restart intent и достаточных contracts.

Зависимость по ресурсам естественна: не пытайся исполнить RTOS task до scheduler ради «ортогональности». Bootstrap не управляет последующей логикой RUN/HA/online edit.

### 15.2. Restart policy

Различи cold start, warm restart, power restoration, watchdog reset, firmware trial boot, application replacement и recoverable fault reset. Для каждого задай:

- источник start intent и допустимость auto-restart;
- доверие к retain/checkpoint, integrity и schema compatibility;
- новый boot identity и аннулирование старых requests/leases;
- outputs до первого валидного цикла;
- поведение при повторной серии reset и переход в recovery lockout.

Production default до утверждения объектного профиля: без самопроизвольного возобновления physical control после неизвестного/corruption fault. Более доступный auto-restart разрешается отдельной обоснованной политикой.

### 15.3. Shutdown

Плановый shutdown: прекратить admission новых конфликтующих изменений; запросить stop/handover; подтвердить output policy; завершить допустимые bounded persistence actions; запросить platform shutdown/reset.

Каждый этап имеет deadline и escalation. Экстренный power loss нельзя заставить ждать flush; восстановимость обеспечивается заранее power-fail-safe форматом, а не надеждой успеть сохранить всё при выключении.

Persistent state, committed application selection и обновление firmware имеют разные durability contracts. STOPPED не доказывает, что последний retain уже сохранён.

## 16. Временные и ресурсные гарантии

### 16.1. Budget, а не слова «быстро» и «немедленно»

Для каждого обязательства укажи начало/конец измерения, maximum bound, worst-case operating envelope, способ измерения и действие при превышении. Среднее и p99 не являются worst-case guarantee.

Минимально задать:

| Параметр | Семантика |
|---|---|
| `T_period[i]`, `D_task[i]`, `C_task[i]` | Период, deadline и бюджет вычисления каждой task |
| `J_release_max` | Наибольшая допустимая задержка release |
| `T_revoke_max` | От обнаруженного запрета до прекращения приёма новых недопустимых output frames |
| `T_stop_max` | Предельное время controlled stop до escalation |
| `T_barrier_wait_max`, `T_commit_max` | Раздельные пределы ожидания quiescence и commit |
| `T_wd_detect`, `T_wd_effect` | Обнаружение отсутствия progress и физический результат watchdog path |
| `A_input_max`, `A_checkpoint_max` | Допустимый возраст input evidence и checkpoint |
| `T_hold[g]`, `T_fallback[g]` | Удержание и физическое достижение fallback для output group |
| `Q_max`, `Journal_max`, `Retry_max` | Очереди, replication/migration backlog и допустимые повторы |
| `Mem_active`, `Mem_candidate`, `Mem_state`, `Mem_migration` | Верхние границы памяти и reserve для каждого arena |

Для software-detected fault выведи консервативный bound всей цепи: обнаружение, доставка, blocking/interference, decision, commit, I/O transport и реакция устройства. Для CPU/OS stall выведи отдельный bound независимого пути. Оба должны укладываться в выделенное объектом время реакции для соответствующего класса отказов.

### 16.2. Takeover и удержание

Для допустимого сценария promotion можно использовать консервативную модель последовательных неперекрывающихся этапов:

```text
T_resume_max = T_detect + T_decide + T_fence + T_restore
             + T_connection + T_first_valid_output

A0 + T_resume_max + Margin < T_hold
T_hold + T_fallback + ClockError <= T_process_stale_limit
```

`A0` — возраст последнего принятого выходного frame к моменту отказа; отсчёт hold начинается от его приёма, а не от detection. Определи границы этапов так, чтобы не считать одно ожидание дважды. Возраст самого входного измерения и динамику процесса анализируй отдельно.

Первое неравенство проверяет возможность пережить handover без срабатывания fallback. Второе ограничивает допустимость такого удержания для процесса. Если общего диапазона нет, нельзя просто увеличить hold: нужно улучшить время восстановления, изменить допустимый сценарий либо пересмотреть process policy.

10–30 ms для takeover и 100 ms для hold — только ранее обсуждавшиеся проектные ориентиры, не production defaults, не измерения и не универсальная формула вендора. При запрете promotion по `L1=L2=0` первое неравенство не создаёт право takeover.

### 16.3. Реальная ограниченность

Покажи scheduling analysis с interference, IRQ, critical sections, DMA/cache, I/O и background work. Одного условия суммы средних нагрузок меньше 100% недостаточно.

Не допускай starvation PLC tasks из-за TLS/crypto, upload, discovery, storm traffic, diagnostics или trace. Эти работы имеют admission/rate/budget limits. Для memory/queue exhaustion описывается не только ошибка, но и сохранение уже работающего control path.

Для watchdog feeding используй доказанный progress нужных участников, а не periodic tick любой живой задачи. В PROGRAM допускается иной явно заданный progress profile; нормальная остановка PLC не должна вызывать бесконечные reboot.

## 17. Cybersecurity и trust boundaries

Определи threat model для engineering, SCADA, I/O, redundancy и local maintenance. Раздели случайные отказы, malicious protocol input, compromised peer и физическое вмешательство. Crash-fault HA не объявляй устойчивым к Byzantine peer без отдельного протокола.

Обязательные проектные требования:

1. Authenticated identity и минимально необходимые права на Start/Stop, force, download, online change, firmware, security/config changes.
2. Пересечение digital authorization и hardware constraints; ключ не выдаёт credentials, network login не отменяет физический запрет.
3. Replay/session binding, expiry, request correlation и повторная проверка полномочий на commit.
4. Защищённые engineering/update channels; для I/O и HA — явно выбранная защита и documented trust assumptions.
5. Bounded parsing, frame/package size limits, quotas, fuzzing, rate limiting и изоляция management load от cyclic path.
6. Secure boot/update policy, credential provisioning/rotation/revocation, защита debug/recovery interfaces.
7. Отсутствие скрытого production bypass, универсального пароля и unaudited force path.
8. SBOM, зависимые компоненты, provenance сборки, vulnerability response и поддерживаемая patch procedure.
9. Integrity/audit trail для критических действий; сбой log storage не блокирует аварийный inhibit, но ограничивает новые engineering mutations по явной policy.

Свяжи требования с выбранным security profile и планом верификации. Серия ISA/IEC 62443 разделяет требования к lifecycle разработки и технические требования компонентов; соответствие нельзя объявить по наличию TLS. Учитывать performance, reliability и safety одновременно — также рамка NIST SP 800-82 Rev. 3.

Не выдумывай номера пунктов стандартов или сертификационный уровень. Для нормативной матрицы используй согласованные редакции полных стандартов; эти открытые обзоры не заменяют нормативную проверку.

## 18. DCS-наблюдаемость, конфигурация и эксплуатация

### 18.1. ControllerStatusView

Derived UI status — read-only projection. Ни Runtime, ни HA, ни I/O не принимают решение на основании строк `RUN_DEGRADED` / `NO_APPLICATION` / `FAULT`.

Показывай отдельно actual execution, selected mode, application generation, fault, authority, HA role/sync, output ownership и data freshness. Одной статусной строкой нельзя скрывать несколько одновременных проблем.

Если источник status умер, показывается stale/unknown и последнее известное значение со временем; UI не выдаёт старый RUN за подтверждённый текущий RUN. Укажи правила LED, local panel, IDE и SCADA projection.

### 18.2. События и audit

Каждое критическое событие имеет стабильный reason code, источник, object ID, boot identity, sequence, local monotonic time и optional UTC с качеством синхронизации. Текст локализуется на представлении, а не становится машинным протоколом.

Fault acknowledgement, alarm acknowledgement и event logging — разные действия. Потери журнала и alarm flood должны быть видимы. Не создавай global total order между контроллерами только по несогласованным wall clocks.

Для engineering request HMI обязана отличать accepted от applied и показывать pending/failed/unknown. После восстановления соединения состояние запроса запрашивается по ID, а не угадывается по цвету кнопки.

### 18.3. Configuration и retain

Configuration — versioned immutable snapshot после validation, с контролируемым commit. Изменение периода task, I/O timeout или HA policy во время работы не является безобидной записью произвольного параметра: нужен соответствующий admission и requalification.

Retain format имеет schema identity, integrity, consistency boundary, wear/endurance budget и recovery policy. Необоснованное копирование RAM в Flash на каждом scan запрещено.

Определи backup/restore, замену CPU, перенос на другую hardware revision, потерю storage, corruption и partial restore. Restore credentials/identity не должен случайно создавать второй контроллер с тем же авторитетом управления.

## 19. Пример реализации FreeRTOS и перенос на Linux

### 19.1. Декомпозиция кода

Покажи dependency graph пакетов: contracts/domain не импортируют FreeRTOS/Linux/BSP; platform adapters реализуют потребляемые порты; composition root связывает их; presentation зависит от публичных наблюдаемых контрактов.

Для каждой FSM представь две независимые схемы:

- compile-time dependency graph;
- runtime event/request graph с разрешёнными обратными ответами.

Request/response создаёт двусторонний обмен, но не обязан создавать циклическую зависимость модулей. Запрещены синхронные циклы ожидания и бесконечные event cascades, а не любые стрелки в обе стороны.

### 19.2. FreeRTOS example

Приведи таблицу tasks/ISR: priority, period/trigger, stack, WCET/budget, shared resources, queue bounds и failure effect. Покажи, какие FSM разделяют task и почему это допустимо.

BSP/ISR выполняют минимальную bounded работу; тяжёлые diagnostics, validation и crypto уходят в budgeted background. Укажи static allocation, допустимые blocking primitives, priority inversion control и правила FFI.

Логические контракты сами по себе не обеспечивают spatial isolation. Если MPU/privilege separation отсутствуют, это отражается в fault model: повреждение общей памяти может затронуть соседние FSM. Требуемый profile либо допускает это с независимой защитой, либо требует иной платформы.

### 19.3. Linux example

Используй те же доменные transitions и семантические request/results. Покажи mapping на процессы/threads, IPC/memory, priority/CPU policy, watchdog и storage durability.

Runtime-процесс не управляет жизненным циклом всего Linux kernel. Его platform agent получает/проверяет необходимые условия; failure процесса, kernel и board — разные сценарии.

Linux может требовать других BSP/diagnostic/storage/network реализаций. N+1 запрещает менять доменные правила ради `pthread`/FreeRTOS API, но не запрещает менять реализацию этих портов и повторять испытания.

## 20. Формальные свойства и пределы доказательств

Разделяй always-invariants на дискретных commit points, bounded-response properties во времени и liveness с явными предпосылками. Нельзя проверкой одного `if` доказать систему с независимыми clock, I/O и отказами.

| ID | Свойство | Проверяемая граница |
|---|---|---|
| INV01 | Только владелец меняет authoritative state своего контекста | API/dependency review и tests |
| INV02 | Start commit требует действующих preconditions и принятого intent | Runtime decision point |
| INV03 | Каждый принятый application-driven output соответствует RUN, совместимому binding и действующему owner/permit | Авторитетный write boundary |
| INV04 | На output group не существует двух одновременно принимаемых владельцев | Fencing authority + endpoint model |
| INV05 | Недопустимый/replayed frame не восстанавливает отозванное право | Endpoint и session/epoch checks |
| INV06 | Наблюдённое `!L1 && !L2` запрещает автоматический promotion commit SECONDARY | HA decision point |
| INV07 | Исполнение не использует освобождённую/непроверенную generation или несовместимую state schema | Binding lifetime и loader verification |
| INV08 | В пределах consistency domain task не видит половину старого/половину нового binding | Barrier model |
| INV09 | Failed Candidate не инвалидирует работающий Original | Artifact/deployment transitions |
| INV10 | TEST не позволяет обходной application-driven physical write | Все effect ports, не только process image |
| INV11 | Revocation/fault приводит к предусмотренной реакции не позднее установленного bound | Software либо independent fault path |
| INV12 | Power loss на durable step восстанавливает целое допустимое состояние или recovery, а не смешанное | Storage/update model |
| INV13 | Ack/clear fault сами по себе не порождают Start | Request/restart model |
| INV14 | Presentation не является источником управления | Dependency checks |

INV03 проверяется по доказательствам, которые реально может проверить endpoint; это не обещание мгновенного знания ещё не обнаруженного отказа. INV11 закрывает временной промежуток обнаружения и реакции.

Для liveness задай условие: при устойчиво выполненных prerequisites, отсутствии higher-priority revocation, допустимой нагрузке и progress платформы принятый запрос достигает результата за заданное время. При потере prerequisites допустим Failed/Cancelled, а не бесконечный pending.

Для critical protocols подготовь небольшую TLA+/PlusCal-модель либо эквивалент: start/revoke race, generation switch, deployment recovery, HA ownership и потери сообщений. Задай assumptions, state bounds и свойства. Model checking одной модели не доказывает отсутствия ошибок реализации, поэтому нужны traceability и implementation tests.

## 21. Формат результата проектирования

LLM должна выдать результат от общего к частному:

1. Архитектурный тезис, scope, assumptions и открытые release blockers.
2. Ownership matrix и реестр механизмов M с обоснованием границ.
3. Contract graph, dependency graph и execution/failure-domain mapping.
4. Пять отдельных statechart: HW_KEY, OS/Platform, HW_DIAG, RUNTIME, APPLICATION per artifact.
5. Обоснованные дополнительные автоматы deployment, firmware maintenance и HA/I/O ownership.
6. Полную спецификацию каждого public contract по разделу 5.
7. Startup, restart, shutdown и fault containment с временными границами.
8. Application hot edit, firmware update и HA coordination, включая crash recovery.
9. FreeRTOS implementation profile и Linux replacement profile.
10. Timing/memory/admission analysis, security и DCS observable behaviour.
11. Invariants, test matrix, N+1 audit, devil's advocate и release gates.
12. Краткие Rust-like interfaces только после фиксации смыслов.

Для **каждой** FSM обязательны: purpose, owned state, inputs, outputs, accepted requests, statechart, полная таблица переходов, invariants и failure semantics.

Таблица переходов:

| Current | Event / request | Guard | Owned state change | Contract effects | Next | Deadline / failure |
|---|---|---|---|---|---|---|
| Явное состояние | Типизированный input | Условия с freshness | Только своё состояние | Request/event/result без чужого set_state | Явное состояние | Timeout и escalation |

Покрой duplicate, late, unexpected и invalid events; отсутствие строки не считается определённым поведением. Для автоматов с retained state задай восстановление после reboot.

Statechart должны иметь однозначную event ordering и guard semantics. Mermaid допустим для обзора; XState v5 JSON/SCXML или таблица могут служить машинно-проверяемой формой. Визуальная диаграмма сама по себе не является реализацией scheduler или доказательством.

Не приписывай Rockwell, Schneider, Siemens или CODESYS собственную реконструкцию внутренних FSM. Вендорские UI/документированные поведения — референс только при точной ссылке на модель/версию; внутреннюю архитектуру продукта помечай как неизвестную, если она не опубликована.

---

# Часть C. Проверка результата и production release gates

## 22. Обязательная fault-injection матрица

Каждый тест содержит исходные условия, последовательность/момент инъекции, ожидаемые transitions и outputs, максимальное время реакции и сохраняемый evidence. Для races перебрать допустимые порядки событий, а не один удачный trace.

| Test | Воздействие | Критерий прохождения | Связь |
|---|---|---|---|
| T01 | Scheduler не стартовал | Outputs в предписанном состоянии; boot failure обнаруживается допустимым путём; нет фиктивно свежего status | D02/D09, INV11 |
| T02 | OS/CPU завис после последнего валидного frame | Независимый путь выполняет output policy в заданный bound | D03/D10, INV11 |
| T03 | Нет application либо подпись/manifest неверны | Start отклонён с reason; diagnostics/engineering доступны по профилю | INV02/INV07 |
| T04 | Ключ дребезжит/обрыв; запрет приходит между validate и commit | Нет unauthorized commit; реакция укладывается в bound | D05/D08, INV02/INV11 |
| T05 | Запоздалый grant от прежнего publisher boot | Grant отвергнут; старое сообщение не восстанавливает authority | D07, INV05 |
| T06 | Потеря необязательного NIC, затем обязательного ресурса | Разные обоснованные reactions; нет общего STOP по любому DEGRADED | D04 |
| T07 | Переполнение очереди, network/engineering flood | Управляющий path сохраняет заявленные bounds либо выполняет controlled rejection/fault policy | D07/D24 |
| T08 | Бесконечный scan / deadlock / panic | Не требуется недостижимый barrier для физического containment | D10/D15 |
| T09 | Candidate испорчен при работающем Original | Original продолжает; Candidate не получает execution binding | D12, INV09 |
| T10 | Exact-match online change под нагрузкой | Сохранение state доказано; latency barrier/commit в профиле | D14/D15, INV08 |
| T11 | Структурная миграция при постоянных записях и journal overflow | Целый согласованный результат либо отказ; нет torn state | D15, INV07/INV08 |
| T12 | TEST + попытка записи через protocol FB/force path | Запись подчиняется заявленной effect policy; обход запрещён | D13, INV10 |
| T13 | Потеря одного sync-link | Degraded, qualification сохранена только при остальных выполненных guards | INV06 |
| T14 | Потеря обоих link, затем отказ PRIMARY | SECONDARY не делает автоматический takeover; доступность ограничена явно | D17, INV06 |
| T15 | PRIMARY execution fault при сохраняющемся допустимом sync path | Promotion возможен лишь после qualification и fencing, в измеренный bound | D16, INV04 |
| T16 | Partition, reboot старого PRIMARY, replay output frames | Два writer не принимаются; старый owner не возобновляет запись | D16, INV04/INV05 |
| T17 | Link loss во время CLAIMING / частичного acquire | До promotion commit операция прекращается по policy; partial ownership не оставляет двойную запись | INV04/INV06 |
| T18 | Checkpoint неполный/повреждён/устарел | Не используется как committed state; SYNC_READY снимается | INV07 |
| T19 | HA failover во время каждого этапа Test/Untest/Finalize | Исполняется одна допустимая целая generation со своей schema; исход определён | D14/D16 |
| T20 | Power loss на каждом durable step firmware/config/retain | Восстановление целой версии либо recovery; mixed version не запускается | D21, INV12 |
| T21 | Неизвестная mixed-version пара при rolling update | Операция запрещена до прекращения управления/утверждённой процедуры | D21 |
| T22 | Повтор engineering request, потеря ACK, reconnect/reboot | Нет повторного эффекта; результат доступен либо честно неизвестен с recovery protocol | D22 |
| T23 | Wall clock jump, monotonic restart/wrap, peer clock mismatch и takeover с активными timers | Timeout/freshness не получают ложное продление; timers сохраняют заявленную временную семантику | D07/D14/D19 |
| T24 | Выходное значение достигло предельного age | Выполняется утверждённый fallback; hold не продлевается старым owner/replay | D20 |
| T25 | Исчезло storage / исчерпан audit log | Cyclic и emergency path ведут себя по профилю; новые mutations ограничены явно | D21/D22 |
| T26 | FreeRTOS заменён Linux, одинаковые входные traces | Те же доменные результаты; платформенные timing tests проходят независимо | D11 |
| T27 | Поворот RUN при boot, после fault и после Clear | Запуск происходит только в разрешённых restart сценариях | D05, INV13 |
| T28 | CPU/I/O replacement, stale restore, чужая hardware/config identity | Недопустимое восстановление управления запрещено с reason | INV02/INV07 |

Ожидаемый исход T14 не является успешным восстановлением процесса: это проверка соблюдения выбранной HA policy. Availability acceptance отдельно определяет, допустим ли такой сценарий остановки для объекта.

## 23. N+1 acceptance tests

| Расширение | Что разрешено изменить | Что обязано сохраниться | Классификация |
|---|---|---|---|
| FreeRTOS → Linux | Ports/BSP, composition, execution profile, квалификацию | Доменные FSM, semantics contracts, invariants | Same-class при одинаковом заявленном profile; timing не наследуется |
| Новый тип physical key | Decoder и конфигурацию mapping/qualification | Runtime и Application | Same-class при прежней authority semantics |
| Один → два diagnostic MCU | Provider instances, resource catalog, trust/freshness settings | Модель diagnostic evidence | Same-class; независимость двух MCU доказывается отдельно |
| Один → N network interfaces | Resource instances и routing/config | Доменные контракты идентифицированных ресурсов | Same-class, пока не заявляется новый гарантийный класс сети |
| Original → Original + Candidate | Дополнительный artifact и deployment transaction | Per-artifact lifecycle и Runtime binding contract | Artifact — same-class; atomic hot edit добавляет отдельное обязательство |
| Single PLC → redundant pair | Replication/role qualification и ownership protocol | Исполнение IEC, artifact validation, output admission interface | New-class: split-brain/replication требуют новых механизмов |
| Новый протокол I/O | Protocol adapter и compatibility tests | Data quality, output policy, effect admission semantics | Same-class только если adapter выполняет требуемый contract |
| BPCS → safety-related SIS | Отдельный safety lifecycle, platform qualification и доказательства | Не обещать прежний набор гарантий достаточным | New-class; не решается ещё одним enum или флагом |

Если существующий контракт не выражает новое фундаментальное свойство, версионируй/пересмотри контракт явно. Не прячь необходимое изменение в adapter, формально заявляя «ничего не поменялось».

## 24. Повторная проверка «адвокатом дьявола»

Проверяющий должен предъявить конкретный counterexample/trace, а не только оценку стиля.

Обязательные вопросы:

- Кто исполняет защитную реакцию, когда обычные FSM больше не получают CPU?
- Где реально блокируется write и может ли его обойти другой protocol/API?
- Кто отзовёт capability умершего publisher? Что означает freshness без работающего local clock?
- Что происходит, если revocation приходит сразу после commit?
- Какие предположения делают две логические FSM зависимыми от одного fault?
- Не перепутано ли принятие запроса с его физическим выполнением?
- Согласован ли Candidate со state, которое продолжает изменяться?
- Что означает rollback после уже совершённого воздействия на процесс?
- Может ли новый PRIMARY стать writer без подтверждённого fencing?
- Не маскирует ли «bumpless» только сохранение памяти?
- Какой отказ одновременно уничтожает redundancy и возможность takeover?
- Какая очередь/повтор/миграция может расти без ограничений?
- Проходит ли boot/recovery без свежих contracts, которые пока некому опубликовать?
- Где Linux adapter обещает свойства, которые платформа не подтвердила?
- Не появился ли скрытый Service Manager под именем reducer, policy engine или event bus?
- Не скрывает ли минимизация M новый класс требований?

Для каждой проблемы оформить: `counterexample → нарушенное свойство → минимальное изменение → regression test → residual risk`. Отсутствие найденного контрпримера не равно доказательству безопасности.

## 25. Production release gates

Готовность заявляется для точной комбинации firmware build, hardware revision, OS/BSP profile, application class, I/O configuration и failure model, а не для абстрактного проекта «на Rust».

| Gate | Требуемый evidence | Причина отказа в выпуске |
|---|---|---|
| G01 Scope / hazards | Утверждённые failure model, operational envelope и output policy по группам | Неизвестна допустимая реакция/время потери управления |
| G02 Architecture | Ownership, полные transitions/contracts, закрытые P0 и traceability | Есть неопределённый writer, lifecycle или recovery |
| G03 Time / resources | Scheduling evidence и target measurements под worst-case нагрузкой, memory/queue bounds | Нет предела latency/backlog/stop/barrier |
| G04 Physical containment | HIL-тесты CPU/OS stall, watchdog, remote I/O timeout и reset-state | Защита зависит только от живой task Runtime |
| G05 Deployment / persistence | Тесты hot edit, migration, rollback и power-fail consistency | Torn state/image/config либо неограниченный catch-up |
| G06 HA profile | Fencing proof/tests, checkpoint ordering, fault coverage и утверждённый trade-off 0/0 | Обещан takeover в запрещённом сценарии или допускаются два writers |
| G07 Security | Threat model, attack-surface review, fuzzing, access tests, SBOM и patch plan | Неуправляемый bypass/update/force path |
| G08 Verification | Traceability requirements → tests, model results с assumptions, regression suite | Критические требования не имеют evidence |
| G09 Operations | Backup/recovery, replacement, maintenance и incident procedures; operator diagnostics | Эксплуатация требует недокументированного ручного обхода |
| G10 Release / site | Идентифицируемая сборка, compatibility matrix, FAT и объектный SAT по применимости | Нельзя воспроизвести/идентифицировать поставленную конфигурацию |

Для disabled feature задаётся явное `not supported` и технический запрет использования, а не отсутствие тестов. Например single-controller release может не поддерживать HA; такой выпуск нельзя называть production redundant DCS controller.

FAT/SAT, environmental/EMC/power qualification, endurance и длительность soak/load tests задаются утверждённым validation plan для выбранной аппаратуры и объекта. Нельзя получать MTBF, SIL или «пять девяток» из одного длительного безошибочного прогона.

## 26. Открытые данные: блокеры квалификации

LLM должна продолжить проектирование с явно обозначенными assumptions, но не заполнять измеряемые параметры вымышленными значениями.

| Необходимое решение | Ответственный | Что нельзя подтвердить до решения |
|---|---|---|
| CPU/board, memory, MPU/MMU, watchdog/clock/reset topology | Platform/hardware engineering | Isolation, independent containment, memory budget |
| Точные OS/BSP/kernel versions и configurations | Platform engineering | Execution profile и timing |
| Tasks, периоды, deadlines, нагрузка и instruction/resource limits | Runtime + application engineering | Schedulability и admission |
| Output groups, fallback, допустимый age/hold, process response budget | Process/control engineering | Безопасность и достаточность I/O reactions |
| Реальные I/O adapters/modules и ownership/fencing support | I/O/HA engineering | Exclusive writer и handover time |
| Acceptance ограничения `L1=L2=0` | Product owner + plant/control engineering | Заявленная availability/fault coverage |
| Restart/retain/force policies | Control + operations + security | Допустимость автоматических действий |
| Migration limits и разрешённые изменения state | Runtime/compiler engineering | Online structural change и Untest |
| Security profile, threat assumptions, provisioning/update authority | Security + product owner | Выполнение security requirements |
| Поддерживаемые firmware/application/HA version pairs | Release engineering | Rolling update и compatibility |
| Test loads, environmental envelope и acceptance thresholds | Verification/validation | Достоверность production claim |

Эти блокеры не мешают составить архитектуру. Они мешают честно объявить её реализацию готовой к эксплуатации до проверки.

## 27. Итоговое условие качества

Результат принимается как архитектурная спецификация, если:

- каждый authoritative state и каждое physical effect имеют однозначного владельца;
- пять основных контекстов взаимодействуют через явные semantic contracts;
- отказ общей платформы не требует исполнения умершей платформы для заявленной физической реакции;
- timing, freshness, authority, generation consistency и ownership определены измеримо;
- hot edit, firmware update, TEST, faults и HA не смешаны в один глобальный режим;
- RTOS/Linux меняют реализации и квалификацию, а не смысл доменных автоматов;
- N+1 минимизирует механизмы одного класса, не уничтожая необходимые границы разных классов;
- опасные противоречия закрыты решением либо отмечены как блокеры, а не скрыты словом «production-ready».

**Главный проверочный вопрос:** это механизм с определённым владельцем, контрактом, пределом времени и тестом — или заплатка, работающая только пока все участники исправны?

