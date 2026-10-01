# IronPLC — Hot Edit POC / Runtime Handoff

## 1. Контекст

Продолжаем разработку собственной PLC runtime/IDE архитектуры на базе **IronPLC**.

Цель текущего этапа — получить работающий **POC hot edit / online change**.

Аппаратную часть, redundancy hardware, optical links, fencing hardware и отдельный HA control-plane **сейчас не проектируем**.

IronPLC рассматривается как базовая runtime/VM, которую мы дорабатываем под собственную архитектуру.

---

## 2. Текущий главный вопрос

Практический вопрос:

> Достаточно ли текущей IronPLC runtime как основы для POC и что минимально нужно изменить, чтобы получить настоящий hot edit без архитектурного тупика перед будущей redundancy?

Текущий ответ:

> **Да, IronPLC runtime достаточно готова как фундамент, но hot edit нужно реализовать как новую архитектурную capability.**

Не переписываем VM с нуля.

Не строим сейчас production HA.

Не проектируем сейчас hardware.

Главная задача POC:

```text
ST edit
    ↓
compile changed logic
    ↓
versioned immutable LogicArtifact
    ↓
validate
    ↓
stage
    ↓
wait safe execution boundary
    ↓
atomic commit
    ↓
continue execution
    ↓
existing runtime state survives
```

---

# 3. Ключевая архитектурная идея

Нельзя связывать IEC state с конкретным экземпляром executable code.

Нужно разделить:

```text
Runtime
├── Scheduler
├── StateStore
├── CodeStore
└── Executor
```

Где:

```text
StateStore
    = долгоживущий semantic IEC state

CodeStore
    = versioned immutable executable Logic artifacts

Executor
    = выполняет активную generation Logic

Scheduler
    = определяет IEC execution boundaries
```

---

# 4. Единица hot edit

Не следует делать hot edit как полную перезагрузку всего PLC process/runtime.

Предпочтительная модель:

```text
POU
├── Declaration / Interface
├── StateSchema
└── Logic
```

`Logic` — заменяемая executable часть.

POU / FB instance state живёт отдельно.

Пример:

```text
Motor instance
├── State
│   ├── Running
│   ├── Timer
│   └── Counter
│
└── LogicId = MotorLogic
        │
        ├── Generation 17
        └── Generation 18
```

Hot edit:

```text
MotorLogic G17
      ↓
stage G18
      ↓
validate
      ↓
safe boundary
      ↓
atomic swap
      ↓
MotorLogic G18

State остаётся тем же.
```

---

# 5. Code identity

У executable logic должен быть стабильный identity.

Концептуально:

```text
LogicId
LogicGeneration
CodeHash
```

Например:

```text
LogicId = MotorControl
Generation = 18
```

Generation immutable.

После публикации executable artifact не меняется.

Следующее изменение создаёт:

```text
MotorControl G19
```

а не мутирует `G18`.

---

# 6. State identity

Runtime state не должен определяться:

```text
memory address
bytecode offset
compiler allocation order
pointer
```

Нужен semantic stable identity.

Концептуально:

```text
StableStateId
ExactSemanticType
StateSchemaGeneration
```

Например:

```text
Main.Counter
Motor1.Timer.ET
Motor1.Running
Globals.BatchCount
```

Физическое представление ID может быть иным, но identity должен сохраняться между recompilation.

---

# 7. Минимальный StateStore

Целевая граница:

```text
StateStore
├── StableStateId
├── ExactSemanticType
├── Value
└── metadata required by runtime
```

Executable code получает state через semantic/runtime binding.

Критический invariant:

```text
Replacing executable Logic
must not implicitly destroy IEC state.
```

---

# 8. StateSchema

Отделяем:

```text
LogicGeneration
```

от:

```text
StateSchemaGeneration
```

Это принципиально важно.

Можно изменить алгоритм POU:

```text
Logic G17 → Logic G18
```

при этом:

```text
StateSchemaGeneration = 5
```

остаётся той же.

Пример:

```text
IF Temperature > 80 THEN
```

заменяем на:

```text
IF Temperature > 75 THEN
```

State schema не изменилась.

Такой edit должен быть самым простым случаем POC.

---

# 9. Scope первого POC

Первый POC должен разрешать только **state-schema-compatible hot edit**.

Разрешаем:

```text
изменение выражений
изменение ветвлений
изменение алгоритма
изменение констант
изменение порядка вычислений
изменение тела POU
```

если существующий runtime state совместим.

Пока можно запрещать:

```text
INT → DINT
delete retained variable
rename state without mapping
move state between incompatible owners
change FB internal state layout
delete active FB instance
```

Ответ runtime:

```text
HotEditRejected::StateSchemaIncompatible
```

Это допустимое ограничение POC.

---

# 10. State migration — следующий слой, не первый POC

Архитектура должна позволять позже добавить:

```text
StateMigrationPlan
```

Но первый POC не обязан реализовывать полноценную migration engine.

Будущая цепочка:

```text
Old StateSchema
      ↓
compatibility analysis
      ↓
StateMigrationPlan
      ↓
Candidate State
      ↓
commit
```

Но сначала доказываем замену Logic при неизменной schema.

---

# 11. Hot edit должен быть транзакцией

Никаких partial mutation работающей runtime.

Концептуальная модель:

```text
HotEditTransaction
├── TransactionId
├── BaseLogicGeneration
├── CandidateLogicGeneration
├── BaseStateSchemaGeneration
├── CandidateStateSchemaGeneration
├── CodeHash
└── CompatibilityResult
```

Lifecycle:

```text
Compiling
    ↓
Prepared
    ↓
Validated
    ↓
WaitingForBoundary
    ↓
Committed
```

Ошибки:

```text
Rejected
RolledBack
```

---

# 12. Commit boundary

Нельзя менять executable body:

```text
inside bytecode instruction
inside arbitrary memory operation
at arbitrary CPU PC
```

Нужен semantic IEC boundary.

Для первого POC предпочтительно:

```text
TaskEnd
```

Схема:

```text
Task cycle N
    ↓
TaskEnd
    ↓
atomic generation swap
    ↓
Task cycle N+1
```

Это максимально простая и детерминированная модель.

Позже можно рассмотреть более мелкие boundaries:

```text
ProgramEnd
```

но POC не требует этого.

---

# 13. Почему TaskEnd важен для будущей redundancy

В уже принятой HA архитектуре checkpoint существует только на semantic IEC boundaries.

Примеры:

```text
ProgramEnd
TaskEnd
```

Мы сознательно не реплицируем:

```text
CPU instruction pointer
bytecode PC посреди инструкции
arbitrary stack frame
```

Допускается controlled replay.

Поэтому hot-edit commit на semantic boundary естественно совместим с будущим redundancy checkpointing.

---

# 14. Active generation manifest

Хотя hot edit делается на уровне Logic, runtime должна уметь однозначно сообщить полный активный executable set.

Например:

```text
ApplicationGeneration = 105

LogicManifest:
    Main          → G21
    Motor         → G8
    Valve         → G14
    PID           → G4

StateSchemaGeneration = 17
```

После изменения только Valve:

```text
ApplicationGeneration 104 → 105

Valve G13 → G14
```

Остальные Logic generation остаются прежними.

---

# 15. ApplicationGeneration

Можно иметь два уровня generations:

```text
LogicGeneration
```

и:

```text
ApplicationGeneration
```

`LogicGeneration`:

```text
версия конкретного executable Logic artifact
```

`ApplicationGeneration`:

```text
immutable manifest активного набора Logic generations
```

То есть:

```text
ApplicationGeneration 105
    ↓
{
    Main: 21,
    Motor: 8,
    Valve: 14,
    PID: 4
}
```

Это особенно полезно для будущей redundancy.

---

# 16. Atomic commit

Commit должен менять активный manifest атомарно.

Не:

```text
replace Motor
then replace Valve
then replace Main
```

если transaction содержит несколько изменений.

А:

```text
Manifest G104
      ↓
single commit point
      ↓
Manifest G105
```

Executor никогда не должен видеть половину старой и половину новой transaction.

---

# 17. Rollback

До commit:

```text
running generation remains authoritative
```

Candidate может:

```text
не скомпилироваться
не пройти validation
оказаться schema-incompatible
```

и это не должно влиять на running control application.

После failed candidate:

```text
ActiveManifest = unchanged
StateStore = unchanged
Scheduler = continues
```

---

# 18. Что должно продолжать работать во время edit

Hot edit не должен означать restart PLC runtime.

Не перезапускаются:

```text
runtime process
scheduler
StateStore
I/O subsystem
communication subsystem
```

Меняется только executable generation после безопасного commit.

---

# 19. POC acceptance test

Исходный код:

```iecst
PROGRAM Main
VAR
    Counter : DINT;
END_VAR

Counter := Counter + 1;
```

Runtime до edit:

```text
Counter = 12537
LogicGeneration = 1
StateSchemaGeneration = 1
```

Новый код:

```iecst
Counter := Counter + 10;
```

Compile:

```text
LogicGeneration = 2
StateSchemaGeneration = 1
```

Candidate проходит validation.

Runtime ждёт:

```text
TaskEnd
```

После boundary:

```text
LogicGeneration 1 → 2
```

State должен остаться:

```text
Counter = 12537
```

а не:

```text
Counter = 0
```

Следующий execution:

```text
Counter = 12547
```

---

# 20. Дополнительные acceptance criteria

POC считается успешным, если одновременно доказано:

```text
1. Runtime process не перезапущен.
2. Scheduler не перезапущен.
3. Existing IEC state не потерян.
4. Candidate compile происходит отдельно от running Logic.
5. Invalid candidate не влияет на active Logic.
6. Commit происходит только на deterministic semantic boundary.
7. Generation switch атомарный.
8. Runtime может сообщить active LogicGeneration.
9. Runtime может сообщить StateSchemaGeneration.
10. После commit новые scans используют новую Logic.
11. Старый runtime state доступен новой Logic.
12. Failed transaction оставляет старую generation running.
```

---

# 21. Redundancy — только архитектурная оглядка

Production redundancy сейчас НЕ реализуем.

Но hot edit не должен мешать будущему HA.

Уже принята модель:

```text
Redundancy Supervisor
        │
        ▼
IronPLC Runtime
```

Redundancy находится выше application runtime.

IEC application сама не определяет:

```text
Primary
Secondary
```

Это runtime role.

---

# 22. Passive Secondary

Будущая Secondary:

```text
Application loaded
CodeGeneration validated
StateSchema validated
```

но:

```text
IEC execution = forbidden
Physical output ownership = forbidden
```

До takeover она не исполняет standard application.

Это означает, что будущая Secondary должна уметь:

```text
получить LogicArtifact
проверить LogicArtifact
сохранить LogicArtifact
проверить StateSchema
```

без исполнения application.

Следовательно CodeStore нельзя проектировать так, чтобы загрузка Logic автоматически означала execution.

---

# 23. ExecutionPermit и OutputOwnership

В HA уже разделены два capability:

```text
ExecutionPermit
OutputOwnership
```

Это разные вещи.

Hot edit POC не реализует их полностью, но runtime API не должен предполагать:

```text
loaded code == allowed to execute
```

или:

```text
executing code == allowed to drive outputs
```

---

# 24. CodeGeneration и HA

В HA state crossload/checkpoint уже предполагает:

```text
CodeGeneration
StateSchemaGeneration
ExecutionEpoch
ExecutionCursor
WrittenStateSet / StateDelta
```

Поэтому generations для hot edit — не временный механизм.

Они будут использоваться и redundancy.

---

# 25. Future HA compatibility rule

Будущая Secondary должна быть takeover-ready только если её executable set совпадает с Primary.

Концептуально:

```text
Primary:
    ApplicationGeneration = 105
    StateSchemaGeneration = 17

Secondary:
    ApplicationGeneration = 105
    StateSchemaGeneration = 17
```

и валиден replicated execution/state checkpoint.

Если Secondary имеет:

```text
ApplicationGeneration = 104
```

она не может считаться fully synchronized/takeover-ready.

---

# 26. Hot edit + redundancy в будущем

Будущая схема:

```text
Engineering
    │
    ▼
Candidate Logic G19
    │
    ▼
Redundancy Supervisor
    │
    ├── stage on Primary
    │
    └── stage on Secondary
            │
            ▼
      both validated
            │
            ▼
      semantic commit protocol
```

Но этот distributed commit сейчас НЕ реализуем.

Важно только, чтобы локальный runtime уже имел:

```text
immutable LogicArtifact
stable IDs
explicit generations
stage
validate
commit boundary
atomic swap
rollback
active manifest
```

---

# 27. Главные HA invariants, которые нельзя сломать

Зафиксировано:

```text
ONE VALID OPTICAL LINK
    → redundancy remains active

NO VALID OPTICAL LINKS
    → Secondary DISQUALIFIED
    → automatic takeover impossible
```

Полный loss двух redundancy links никогда не означает автоматически:

```text
Primary dead
```

---

# 28. Split-brain invariant

Будущий абсолютный invariant:

```text
NOT (
    A has OutputOwnership
    AND
    B has OutputOwnership
)
```

Это не решается одним Rust enum.

В production будет нужен physical/protocol-level fencing / ownership mechanism.

Но это сейчас вне POC.

---

# 29. Что НЕ делаем сейчас

Не проектируем:

```text
HA hardware
separate HA MCU/FPGA
optical redundancy PHY
output fencing hardware
OwnershipEpoch transport
pair bootstrap
remote I/O authority
crossload network protocol
heartbeat implementation
automatic takeover
multi-controller distributed hot-edit commit
```

Это следующий этап.

---

# 30. Что НЕ нужно переписывать сейчас

Не нужно автоматически переписывать:

```text
parser
ST frontend
всю VM
весь scheduler
весь I/O layer
всю project model
```

Сначала найти в IronPLC минимальные seam points для:

```text
CodeStore
StateStore
Executor
Scheduler boundary
```

и построить вертикальный POC.

---

# 31. Главный архитектурный вопрос для аудита IronPLC

Нужно исследовать текущий repository и точно определить:

```text
1. Где живёт compiled executable representation?
2. Где хранится runtime variable/instance state?
3. Насколько code и state сейчас связаны?
4. Где scheduler завершает task cycle?
5. Можно ли заменить executable body без restart runtime?
6. Какие структуры держат pointers/references на compiled code?
7. Можно ли построить immutable LogicArtifact?
8. Можно ли отделить instance state от code artifact?
9. Где лучше реализовать atomic active-generation indirection?
10. Какие existing abstractions IronPLC нужно сохранить, а какие минимально расширить?
```

---

# 32. Предпочтительный runtime abstraction

Целевая форма примерно:

```text
Runtime
│
├── Scheduler
│
├── Executor
│
├── StateStore
│
├── CodeStore
│
└── ActiveManifest
```

Execution:

```text
Scheduler
    ↓
Executor
    ↓
resolve LogicId
    ↓
ActiveManifest
    ↓
LogicGeneration
    ↓
CodeStore
    ↓
execute
    ↓
StateStore
```

---

# 33. Почему indirection важна

Executor не должен навечно держать:

```text
pointer directly to mutable compiled function body
```

Лучше иметь:

```text
LogicId
    ↓
ActiveManifest
    ↓
immutable LogicGeneration
```

Тогда hot edit — это изменение mapping:

```text
MotorLogic → G17
```

на:

```text
MotorLogic → G18
```

в безопасной точке.

---

# 34. Rust-идиома

Предпочтительно использовать immutable data + explicit ownership.

Например концептуально:

```rust
struct LogicId(...);
struct LogicGeneration(...);
struct StateSchemaGeneration(...);

struct LogicArtifact {
    id: LogicId,
    generation: LogicGeneration,
    // immutable executable representation
}

struct ActiveManifest {
    // LogicId -> LogicGeneration
}
```

Не нужно превращать runtime в набор глобальных mutable bool.

---

# 35. Правило POU/state

Очень важно не смешать:

```text
POU type
POU instance
Logic artifact
State schema
runtime state
```

Концептуально:

```text
POU type
    ↓ instantiate
POU instance
    ↓ owns
State

POU type
    ↓ references
LogicId

LogicId
    ↓ resolves
LogicGeneration
```

То есть executable code разделяется всеми совместимыми instances, а state принадлежит instance.

---

# 36. `.iplc`

`.iplc` следует рассматривать как контейнер/project packaging layer, а не как саму runtime object model.

Внутри может быть много POU/logic/state declarations.

Runtime должна оперировать semantic entities, а не зависеть от физического layout `.iplc` файла.

---

# 37. IEC compatibility

Не добавлять нестандартные custom attributes к IEC переменным только ради runtime metadata.

Stable IDs / generation / runtime metadata должны существовать вне IEC language semantics либо генерироваться compiler/runtime infrastructure.

IEC 61131-3 model variables должна оставаться стандартной.

---

# 38. Service Manager

**Service Manager не используется.**

Не возвращать его в архитектуру I/O/runtime/HA.

---

# 39. Итоговый приоритет

Сейчас порядок такой:

```text
1. Audit current IronPLC runtime internals.
2. Найти границу code vs runtime state.
3. Ввести stable LogicId.
4. Ввести immutable LogicGeneration.
5. Ввести StateStore boundary.
6. Ввести StateSchemaGeneration.
7. Ввести ActiveManifest.
8. Реализовать stage candidate.
9. Реализовать compatibility validation.
10. Реализовать TaskEnd atomic commit.
11. Доказать сохранение state.
12. Реализовать rollback/rejection.
13. После этого считать hot-edit POC закрытым.
14. Только затем возвращаться к production redundancy.
```

---

# 40. Definition of Done для текущего этапа

Текущий этап завершён, если можно продемонстрировать:

```text
PLC application continuously executes.

Engineer changes Logic of one POU.

Only changed Logic is recompiled/staged.

Runtime remains alive.

Existing instance state remains alive.

Candidate is validated.

At TaskEnd active generation atomically changes.

Next scan executes new Logic.

No state reset occurs.

Invalid edit can be rejected without disturbing running application.

Runtime exposes exact active generations.

Architecture не препятствует будущему Primary/Secondary synchronization.
```

---

# 41. Следующий шаг для нового чата

Не продолжать абстрактное проектирование.

Следующий шаг:

> **Провести аудит актуального IronPLC repository именно относительно этой модели hot edit.**

Нужно найти конкретные Rust crates/modules/types/functions и ответить:

```text
что уже существует,
что можно переиспользовать,
что нужно разрезать,
что нужно добавить,
какой минимальный patch-set даст первый working hot edit POC.
```

Главный принцип:

> **Не проектировать новую runtime поверх воображаемого IronPLC. Сначала привязать архитектуру к реальному current codebase.**

После repository audit сформировать конкретный implementation plan по files/modules/traits/types.

---

# 42. Краткий контекст HA для сохранения

Production redundancy позже строится поверх runtime:

```text
Hardware / RTOS
      │
      ▼
Redundancy Supervisor
      │
      ▼
IronPLC Runtime
```

Secondary passive.

State Crossload отдельный от ReplicationCheckpoint.

Checkpoint только на semantic IEC boundaries.

Полный loss обоих redundancy links:

```text
Secondary → DISQUALIFIED
automatic takeover forbidden
```

Один живой redundancy link:

```text
redundancy remains active
```

Split-brain должен быть физически/protocol-level недостижим.

После switchover automatic failback отсутствует.

Эти правила сейчас считаются зафиксированными и не должны перерабатываться в ходе hot-edit POC без отдельной причины.

---

# 43. Working thesis

Самая короткая формула текущей архитектуры:

```text
State belongs to the IEC object instance.

Logic belongs to an immutable generation.

Hot edit changes Logic generation,
not object identity and not runtime state.

Commit happens only at a semantic execution boundary.

Redundancy later synchronizes the same generations and state,
but does not define the local hot-edit mechanism.
```
