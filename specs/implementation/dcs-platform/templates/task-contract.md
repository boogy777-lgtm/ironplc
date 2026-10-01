# Component / task contract template

Заполняется в implementation PR. Пустые поля — неизвестные факты, не defaults.

| Поле | Значение |
|---|---|
| Work package / local criteria / T / INV / REQ | |
| Source commit / spec revision / dependency versions | |
| Owner / authoritative fields / single writer | |
| Pure functions и explicit inputs | |
| Stateful history, identity, schema/type | |
| Public commands/views / visibility boundary | |
| Execution plane / protection domain / update unit | |
| Capacity bytes/entries/instances/work-per-call | |
| Lifetime/borrow/pins/retirement | |
| Clock domain / deadline / freshness / expiry | |
| Admission, revalidation и linearization point | |
| Physical effect sink / acceptance / propagation bound | |
| Errors/retry/cancel/UnknownOutcome/reconciliation | |
| Reset/retain/migration/replication/power-fail unit | |
| Principal/capabilities/issuer/revoke/audit | |
| Platform ports/TCB/failure coupling | |
| METHOD/oracle/fixtures/commands/evidence | |

Allowed change paths, запрещённые dependencies, integration prerequisites и N+1 case указать отдельным коротким текстом. Contract types следуют ответственности, не стремлению создать максимальное число traits.
