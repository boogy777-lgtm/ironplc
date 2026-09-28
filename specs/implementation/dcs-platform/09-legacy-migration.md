# 09. Миграция из существующего кода

Норма — v3.0. Исторические ADR и реализация объясняют текущее поведение; их полезные части сохраняются. W01 оформляет supersession с доступным номером ADR и точным migration scope, не переименовывает старые решения задним числом.

| Legacy / существующий механизм | Целевой переход | Compatibility и задачи |
|---|---|---|
| Hot-edit orchestration в RuntimeHost | Host сохраняет local commit/state, Deployment ведёт operation | Legacy command adapter вызывает новый coordinator; W18/W30 |
| HostMode Normal/Testing | Внутренний выбор edit generation отдельно от PROGRAM/TEST/RUN | Wire/UI не смешивают TEST mode; W09/W32 |
| swap_buffers exact copy | Same live arena + checked StateAddressMap | Instrumentation подтверждает отсутствие allocation/full copy; W16 |
| run_session→load сбрасывает TaskState | Initialize один раз, attach/resume сохраняют history | Multi-period regression до изменения; W05 |
| Layout/size compatibility | Exact semantic schema identity/type | Explicit conversion/preserve не exact; W07/W17 |
| Volatile per-round Epoch | Раздельные checkpoint seq, activation rev, durable authority term | Wire version/reboot/exhaustion; W02/W25 |
| 0/0 всегда запрещает takeover | Полная формула qualified state+RecoveryAdmission+whole-group fence | Conservative policy только как отдельный ограниченный profile; W26 |
| Crossload собственных candidate/host snapshots | Common catalog + semantic checkpoint protocol | Versioned migration wire; no second application manager; W24/W27 |
| Два banks как единственный lifecycle | Bounded pool/pins; два banks — backend | Не ломать embedded capacity; W08 |
| Single engineering TCP writer/session | Многие read clients + resource-scoped mutating serialization | Auth/quotas/revisions нужны до изменения; W29/W30 |
| Stage/Accept как будто PREPARED автоматически | RECEIVING/VERIFIED/PREPARED milestones | UI показывает доказанный progress; W18/W32 |
| SlotStore file fsync | Declared whole-root durability и directory/device assumptions | Recovery fixtures сохраняются, дополняются physical proof; W20 |
| Auto-Untest после switchover по vendor analogy | Resume generation разрешена RecoveryAdmission | Буквальная vendor policy не включается UI flag; W27 |
| +1000 protocol loss counter / EMA bound | Consecutive sequence, separate loss metric, calibrated conservative bounds | Метрика совместима только через versioned adapter; W23 |

## Переход без большой одновременной переписи

Сначала regression fixtures существующего полезного поведения и explicit target differences. Затем extract contract/owner seam без изменения behavior, после него targeted semantic change. Старый public API временно адаптирует к новому owner только если не теряется смысл outcome/capability. Если новый wire результат непредставим старым ACK, version negotiation отклоняет несовместимого client вместо ложного success.

В каждый момент authoritative writer один. Нельзя на период миграции сохранить active_generation в старом Host и завести независимую копию в новом Supervisor. In-flight records/markers/schema versions либо мигрируют согласованно, либо требуют контролируемого maintenance boundary. Новая version не даёт права исполнять mixed root.

Данное задание не меняет статусы accepted ADR и не удаляет старый код. Это конкретная очередь migration obligations для code PR. Список W01 определяет, какие accepted решения уже superseded целевой спецификацией, а какие сохраняются частично.
