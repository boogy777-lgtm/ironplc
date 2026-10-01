# Review record template

Reviewer / дата / reviewed commit / W-ID:

1. Выполнен заявленный observable outcome? Какие criteria остаются открытыми?
2. Один writer на authoritative fact? Нет скрытого global manager/private state bypass?
3. Evidence oracle независим от проверяемого code path?
4. Revoke/stale completion/overflow/reset/crash/retry покрыты в нужном scope?
5. METHOD соответствует claim? Где hardware/model assumptions и limitations?
6. N+1: class, M before/after, разрешённый и фактический diff?
7. Сохранены compiler pipeline, IEC syntax, lint fences, required gates и прежние semantic tests?

| Finding | Reproducible counterexample | Violated requirement | Severity / scope | Exit criterion |
|---|---|---|---|---|
| | | | | |

Результат: ACCEPT для указанного scope / CHANGES_REQUIRED / QUALIFICATION_BLOCKED. Принятие software PR не означает разрешение всех product profiles. Merge/release следует действующему workflow и actual gates.
