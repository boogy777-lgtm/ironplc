# Evidence report template

| Поле | Значение |
|---|---|
| W-ID / commit / PR | |
| Spec SHA / profile / build hash | |
| Hardware/OS/BSP/toolchain/config/workload | |
| Реализованный scope и технически закрытые capabilities | |
| IMPLEMENTED / VERIFIED / QUALIFIED | |

| Claim / criterion / T / REQ | METHOD | Fixture / command / seed | Expected / numeric bound | Actual / status | Artifact / SHA-256 |
|---|---|---|---|---|---|
| | | | | | |

Для MODEL: assumptions, fairness, finite bounds, explored states, counterexamples и mapping к implementation traces. Для HIL: rig/sensor timing uncertainty, calibration, raw capture, injection point, accepted physical outputs и recovery. Для crash storage: commit prefixes и promised durability model.

Указать failed/blocked/not-run tests отдельно. `NOT_APPLICABLE` требует reason и unsupported admission. Приложить existing gates logs, affected behavior, residual risk и N+1 before/after mechanisms. Следующему исполнителю передать public contracts, fixtures, known limits и exact commit.
