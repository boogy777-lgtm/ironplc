# IronPLC / RegulBUS — Redundancy Architecture Summary

## 1. Что проектируем

Проектируем production-grade hot-redundancy для IronPLC/RegulBUS в духе ControlLogix High Availability, но с более формальной моделью на Rust.

Базовая идея:

```text
Hardware / RTOS
        │
        ▼
Redundancy Supervisor
        │
        ├── qualification
        ├── synchronization policy
        ├── heartbeat / link health
        ├── State Crossload
        ├── ReplicationCheckpoint
        ├── fencing / ownership
        └── switchover
        │
        ▼
IronPLC Runtime
        │
        ├── IEC scheduler
        ├── ApplicationExecutor
        ├── StateStore
        └── I/O
```

Redundancy находится **выше application runtime**. IEC-приложение само не решает, Primary оно или Secondary.

---

## 2. Модель Primary / Secondary

В проекте хранится только:

```text
REDUNDANCY_ENABLED
redundancy configuration
```

Но не:

```text
PLC_A = Primary
PLC_B = Secondary
```

`Primary/Secondary` — исключительно runtime role.

У физических PLC есть постоянная identity:

```text
PairId
ControllerId
hardware identity
```

А роли могут меняться после switchover.

Это соответствует подходу Rockwell: одна application для пары, роли определяются состоянием redundancy system.

---

## 3. Secondary не выполняет application

Это один из самых важных утверждённых принципов.

После boot/download резервный PLC может иметь:

```text
Application loaded
CodeGeneration validated
StateSchema validated
```

но:

```text
IEC application execution = forbidden
Physical output control    = forbidden
```

Secondary в нормальной работе пассивен с точки зрения IEC execution.

Он получает state от Primary через State Crossload.

Только после успешного takeover Redundancy Supervisor выдаёт ему capabilities:

```text
ExecutionPermit
OutputOwnership
```

и тогда бывший Secondary становится Primary и начинает/продолжает IEC execution.

---

## 4. State machine Secondary

Утверждённая цепочка:

```text
BOOT
  ↓
DISQUALIFIED
  ↓
QUALIFYING
  ↓
SYNCHRONIZING
  ↓
SYNCHRONIZED_SECONDARY
  ↓
TAKEOVER_READY_SECONDARY
```

`SynchronizedSecondary` и `TakeoverReadySecondary` — разные состояния.

Для `TakeoverReadySecondary` недостаточно просто иметь актуальный StateStore. Нужны также валидный execution checkpoint, готовность I/O path и остальные HA-условия.

Новый или перезагрузившийся узел никогда не считается готовым автоматически.

---

## 5. Auto-Synchronization

Как у Rockwell, synchronization — отдельная политика redundancy supervisor.

Утверждены три режима:

| Policy | Поведение |
|---|---|
| `Always` | Secondary автоматически пытается снова пройти qualification/synchronization |
| `Never` | остаётся Disqualified до явной команды |
| `Conditional` | автоматическое поведение включается/выключается командами |

`Synchronize Secondary` — отдельная **асинхронная команда**.

Пока Secondary находится в:

```text
Disqualified
Qualifying
Synchronizing
```

он не takeover-ready.

---

## 6. State Crossload

Принят термин и концепция Rockwell:

```text
State Crossload
```

Но реализуем его поверх нашего semantic StateStore.

Primary отслеживает state, в который была запись:

```text
StateStore
   ↓
StateChangeTracker
   ↓
ReplicationJournal
```

Важно:

> учитывается **факт записи**, а не только отличие конечного значения.

То есть если:

```text
X = 5
```

и программа снова записала:

```text
X := 5;
```

этот state всё равно считается written для текущего replication interval.

Crossload логически содержит:

```text
CodeGeneration
StateSchemaGeneration
ExecutionEpoch
ExecutionCursor
WrittenStateSet / StateDelta
```

Secondary сначала получает это как pending state.

Только после завершённого checkpoint появляется:

```text
CommittedCheckpoint
```

который может использоваться как точка восстановления.

---

## 7. ReplicationCheckpoint

Мы отделили:

```text
State Crossload
```

от:

```text
ReplicationCheckpoint
```

Crossload переносит state.

Checkpoint говорит:

> всё до этой execution boundary образует согласованную точку state + execution.

Наш checkpoint должен содержать примерно:

```text
ExecutionEpoch
ExecutionCursor
CodeGeneration
StateSchemaGeneration
DeltaSequence
```

Checkpoint существует только на **семантических IEC boundaries**, например:

```text
ProgramEnd
TaskEnd
```

Не пытаемся реплицировать CPU instruction pointer или продолжать bytecode с середины инструкции/routine.

Controlled replay допускается.

---

## 8. Три redundancy profile

Утвердили три политики частоты checkpoint:

| Profile | Поведение |
|---|---|
| `REDUNDANCY_FINE` | checkpoint после каждого Program / крупной execution unit |
| `REDUNDANCY_BALANCED` | checkpoint после выбранных Program + обязательный Task-end |
| `REDUNDANCY_PERFORMANCE` | только обязательный Task-end |

Это **один и тот же redundancy engine**.

Меняется только:

```text
CheckpointPolicy
```

а не архитектура replication.

В Rust это примерно:

```rust
trait CheckpointPolicy {
    fn checkpoint_after(
        &self,
        boundary: ExecutionBoundary,
    ) -> CheckpointDecision;
}
```

---

## 9. Rust-идиомы

Утвердили, что HA должен использовать typestate/capability, а не разбросанные runtime bool.

Концептуально:

```text
RedundancyNode<Disqualified>
RedundancyNode<Qualifying>
RedundancyNode<Synchronizing>
RedundancyNode<SynchronizedSecondary>
RedundancyNode<TakeoverReadySecondary>
RedundancyNode<Primary>
```

Secondary не должен иметь API вроде:

```text
run_application()
publish_physical_outputs()
```

Опасные действия доступны только типам/capabilities, которым это разрешено.

Отдельно:

```text
ExecutionPermit
OutputOwnership
```

Это два разных разрешения.

Rust защищает software invariant, но физический split-brain должен дополнительно исключаться hardware/protocol mechanism.

---

## 10. Optical redundancy links

Утверждены **два выделенных optical redundancy порта**.

Они физически отделены от:

```text
SCADA
engineering
OPC UA
MQTT
ordinary plant Ethernet
```

Через optical HA path идут:

```text
State Crossload
ReplicationCheckpoint
ACK
qualification
generation/schema sync
heartbeat
HA control traffic
```

Ключевое правило состояния двух links:

```text
L1=1, L2=1 → REDUNDANCY ACTIVE / HEALTHY

L1=1, L2=0 → REDUNDANCY ACTIVE / DEGRADED

L1=0, L2=1 → REDUNDANCY ACTIVE / DEGRADED

L1=0, L2=0 → REDUNDANCY LOST
              Secondary → DISQUALIFIED
              automatic takeover forbidden
```

То есть:

> **Пока жив хотя бы один валидный optical sync-link — redundancy остаётся активным.**

---

## 11. Самое жёсткое правило link partition

Это отдельно зафиксировали как архитектурный invariant:

```text
L1=0 && L2=0
    ↓
Secondary → DISQUALIFIED
```

После полного разрыва redundancy links Secondary:

```text
не является SynchronizedSecondary
не является TakeoverReady
не может автоматически стать Primary
```

Даже если затем он перестал видеть heartbeat Primary.

Формула:

```text
No valid redundancy link
        =>
No automatic takeover
```

Это специально выбрано для исключения split-brain через network partition.

---

## 12. Heartbeat / ping-pong

Эксплуатационная схема:

```text
peer seen    → counter += 1
peer missing → counter += 1000
```

сохраняется.

Но она служит диагностикой, а не окончательным арбитром HA.

Для protocol-level liveness используем:

```text
HeartbeatSeq
LastPeerSeqSeen
BootEpoch
local monotonic timeout
```

То есть heartbeat доказывает:

```text
я жив
я вижу тебя
ты видишь меня
это не старый пакет от прошлого boot
```

`+1/+1000` остаётся удобной эксплуатационной диагностикой на HMI.

---

## 13. Один optical link потерян

Это **не** приводит к requalification Secondary.

Например:

```text
L1 = lost
L2 = healthy
```

тогда:

```text
RedundancyPath = DEGRADED
Secondary      = synchronized
TakeoverReady  = сохраняется
Crossload      = продолжается
```

В эксплуатации появляется alarm, но redundancy остаётся полностью функциональным через оставшийся link.

---

## 14. Оба optical link потеряны

Это принципиально не интерпретируется как:

```text
Primary dead
```

Мы различаем:

```text
LinkLost
PeerDead
PrimaryAuthorityLost
```

Полный optical partition приводит прежде всего к:

```text
Secondary → DISQUALIFIED
```

Текущий Primary продолжает application.

Это один из самых важных утверждённых safety choices.

---

## 15. Split-brain должен быть физически недостижим

Зафиксирован абсолютный invariant:

```text
NOT (
    A has OutputOwnership
    AND
    B has OutputOwnership
)
```

Нельзя полагаться только на:

```text
if role == Primary
```

или Rust enum.

Нужен физический/protocol-level ownership/fencing mechanism.

Remote I/O или HA authority layer должен принимать physical output только от актуального owner.

Концептуально:

```text
ControllerId
OwnershipEpoch
OutputImageEpoch
Payload
Integrity
```

Remote I/O знает:

```text
CurrentOwner
CurrentOwnershipEpoch
```

и отвергает stale owner.

Например:

```text
current owner epoch = 101

old PLC sends epoch 100
→ REJECT
```

---

## 16. Takeover condition

Takeover нельзя сводить к:

```text
heartbeat timeout
```

Условия должны быть существенно строже.

Концептуальная формула:

```text
TAKEOVER_ALLOWED =
    TakeoverReadySecondary
 && valid StateCheckpoint
 && valid ExecutionCheckpoint
 && IoPathReady
 && ownership/fencing authority valid
```

При полном loss обоих optical links это условие автоматически становится false из-за немедленной disqualification Secondary.

---

## 17. После switchover нет automatic failback

Утвердили:

```text
A Primary
B Secondary

A fails

B → Primary

A returns
```

A не возвращает роль Primary автоматически.

Он проходит:

```text
Disqualified
→ Qualifying
→ Synchronizing
→ SynchronizedSecondary
→ TakeoverReadySecondary
```

B остаётся Primary.

Это уменьшает число опасных role transitions.

---

## 18. Что взяли у Rockwell

Из Rockwell как operational reference решили сохранить:

```text
Redundancy Enabled как controller/project capability
одна application для пары
runtime Primary/Secondary roles
passive Secondary для standard application
State Crossload
Program/Task synchronization points
Disqualified / Synchronizing / Synchronized lifecycle
Always / Never / Conditional auto synchronization
dual optical redundancy channels
controlled replay после последнего sync point
жёсткие feature restrictions там, где HA semantics не доказана
```

Но не копируем их внутренности вслепую.

Наши преимущества greenfield:

```text
StableStateId
ExactSemanticType
StateSchemaGeneration
immutable CodeGeneration
Rust ownership/typestate
semantic StateStore
```

---

## 19. Что показала «проверка дьяволом»

Архитектурный фундамент выдержал.

Но выявились области, где нельзя ограничиться красивой software-моделью.

Самые серьёзные:

```text
I/O ownership/fencing
independent HA fault-containment domain
external side effects/replay
multi-task ExecutionFrontier
initial pair bootstrap
I/O readiness qualification
```

Также решили не делать обязательную модель:

```text
Secondary ACK
    ↓
только после этого physical outputs
```

как default, потому что это может превратить отказ Secondary/link в нарушение realtime Primary.

Принцип:

```text
healthy Primary must not miss
its control deadline because
Secondary became unhealthy
```

---

## 20. Что разрабатывать дальше

Рекомендуемый порядок:

1. **HA hardware/control-plane architecture.** Определить, нужен ли отдельный MCU/FPGA/arbiter, как он наблюдает основной CPU, как управляет authority и чем отличается от main runtime.

2. **OutputOwnership / OwnershipEpoch.** Формально определить, кто выдаёт ownership, как remote I/O проверяет owner, как отзывается старый epoch и почему два owner физически невозможны.

3. **Bootstrap пары.** Что происходит при одновременном включении A и B; как выбирается первый Primary без нарушения утверждённого правила о partition.

4. **I/O readiness.** Secondary может иметь synchronized state, но неисправный I/O path. Нужно определить критерий `TakeoverReadySecondary`.

5. **ExecutionFrontier.** Формализовать checkpoints для нескольких periodic tasks, priorities и preemption, не уходя в replication CPU PC.

6. **External effects.** Что делать с Modbus writes, MQTT publish, commands и другими side effects при controlled replay.

7. **Rust API/state machines.** После фиксации hardware invariants окончательно оформить `RedundancyNode<State>`, `ExecutionPermit`, `OutputOwnership`, `CheckpointPolicy`, `CrossloadEngine`.

---

## Главные инварианты

```text
ONE VALID OPTICAL LINK
    → redundancy remains active

NO VALID OPTICAL LINKS
    → Secondary DISQUALIFIED
    → automatic takeover impossible
```

И поверх него:

```text
Two controllers must never
be able to own physical outputs
at the same time.
```

Следующий логичный шаг: проектирование **аппаратного `OutputOwnership / fencing`**.
