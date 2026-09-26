# 10. Проверка «адвокатом дьявола»

Reviewer проверяет гарантии через наблюдаемый контрпример. Для каждой проверки нужны detector, reaction owner, живой enforcement path, bound, recoverable outcome и evidence. Ответ «Rust безопасный» либо «есть FSM» не закрывает ни одного сценария.

| Атака на архитектуру | Что должно сломать плохую реализацию | Куда направить |
|---|---|---|
| Модуль выглядит plug-in, но пишет &mut global ControllerState | Private API/capability/dependency test; лишний writer недоступен | W02/W03 |
| Scheduler жив только в рамках одного run(N) | Те же release times при N вызовах run(1), включая редкую task | W05 |
| Timeout после завершения infinite loop | Budget внутри dispatch плюс independent endpoint при остановке CPU | W06/W13 |
| Exact edit сделал одну строку swap, внутри большой copy/drop | Allocation/copy/destructor instrumentation при max state и whole pause | W16 |
| Native и external — два названия одного unrestricted provider | Read-only subscriber пытается получить output grant; quota/failure coupling | W10/W14 |
| Существуют bypass %Q через protocol FB/force/commissioning | Capability closure и physical sink rejection во всех путях TEST/revoke | W12 |
| Second writer получил majority ACK, один sink остался old | Whole-group acquisition не завершён; частичная fencing recovery | W25 |
| Изолированный standby не услышал invalidate после degraded edit | Authority required-generation record не допускает старое restore | W27 |
| Pair Stop применён локально, standby стартовал при crash | Local/pair receipts различны, RecoveryAdmission запрещает auto-recovery | W27 |
| Перепутали peer checkpoint ACK и physical impulse outcome | EffectId/query либо UnknownOutcome без blind retry | W24 |
| Новый retain перекрыл единственную root для старого boot code | Crash после каждого trial/retain/finalize step | W20/W21 |
| Authority term вернулся из backup/overflow/reboot | Incarnation/reconciliation и controlled refusal | W25/W36 |
| Network daemon продолжает feed watchdog при omitted cycle | Watchdog evidence зависит от execution progress | W13 |
| Diagnostic/HMI service отказал и заблокировал scan | Remove/kill/full ring/storage/flood tests | W33/W34 |
| Scope no_std объявлен по контейнеру | Full closure bare-metal build и std import negative fixture | W38 |
| Новый provider API такой же, но latency хуже | Capability/admission/profile qualification, а не core workaround | W42/W44 |

## Вопросы для review каждого PR

1. Где linearization point и кто единственный writer? Какие observers видят atomic whole unit?
2. Что произойдёт между проверкой и commit при revoke/reboot/config change?
3. Как обнаруживается отказ, если сам owner больше не выполняется? Что останется живо?
4. Где bound по bytes/work/time, кто следит за expiry без нового сообщения?
5. Что знает система при потерянном receipt? Можно ли повторить операцию без дополнительного effect?
6. Какая точная аппаратная/модельная предпосылка требуется? Где она проверена?
7. Расширение N+1 использует существующий механизм или прячет частный случай?

Reviewer возвращает конкретный trace/event ordering и violated invariant. Исполнитель добавляет fixture/counterexample regression и чинит общий mechanism. Для документационного PR достаточно проверить согласованность назначения и отсутствие неподтверждённых claims; запуска будущего hardware validation он не имитирует.
