# 06. Трассируемость требований и доказательств

Источник IDs — [спецификация v3.0](../../design/dcs-plc-production-platform-spec-ru.md). Здесь назначается **один primary evidence owner** для каждого сценария. Он собирает end-to-end результат участников, а не становится новым owner их runtime state. Остальные карточки могут ссылаться на тот же T как contributors.

Все строки имеют статус **NOT_RUN**. METHOD — требуемый набор методов, не выполненное испытание. TARGET/HIL означает target software плюс аппаратную проверку там, где есть physical claim; simulation остаётся отдельным precursor. Для неподдержанного профиля применимость обосновывается W43, при этом feature admission закрыт.

Канонический будущий artifact каждого T: `evidence/<profile>/<build>/Tnn/manifest.json` с links+hashes raw traces/logs/captures; существование этого пути пока не заявляется. Формат фиксирует W04. Source claim и scope никогда не заменяются кратким названием строки.

## T01–T95

| ID | Воздействие из baseline | Primary task | Минимальный METHOD |
|---|---|---|---|
| T01 | Scheduler не стартовал | [W13](tasks/W13-linux-boot.md) | INTEGRATION/SIM + TARGET/HIL |
| T02 | OS/CPU завис после последнего валидного frame | [W12](tasks/W12-effects.md) | INTEGRATION/SIM + TARGET/HIL |
| T03 | Нет application либо подпись/manifest неверны | [W08](tasks/W08-catalog.md) | INTEGRATION/SIM |
| T04 | Ключ дребезжит/обрыв; запрет приходит между validate и commit | [W09](tasks/W09-mode-key.md) | INTEGRATION/SIM + TARGET/HIL |
| T05 | Запоздалый grant от прежнего publisher boot | [W25](tasks/W25-output-authority.md) | MODEL + INTEGRATION/SIM |
| T06 | Потеря необязательного NIC, затем обязательного ресурса | [W13](tasks/W13-linux-boot.md) | INTEGRATION/SIM + TARGET/HIL |
| T07 | Переполнение очереди, network/engineering flood | [W03](tasks/W03-composition.md) | INTEGRATION/SIM + TARGET/HIL |
| T08 | Бесконечный scan / deadlock / panic | [W06](tasks/W06-execution-budget.md) | INTEGRATION/SIM + TARGET/HIL |
| T09 | Candidate испорчен при работающем Original | [W08](tasks/W08-catalog.md) | INTEGRATION/SIM |
| T10 | Exact-match online change под нагрузкой | [W16](tasks/W16-exact-activation.md) | INTEGRATION/SIM |
| T11 | Структурная миграция при постоянных записях и journal overflow | [W17](tasks/W17-migration.md) | INTEGRATION/SIM |
| T12 | TEST + попытка записи через protocol FB/force path | [W12](tasks/W12-effects.md) | INTEGRATION/SIM |
| T13 | Потеря одного sync-link | [W23](tasks/W23-ha-transport.md) | INTEGRATION/SIM + TARGET/HIL |
| T14 | 0/0: живой PRIMARY, внезапный power loss PRIMARY, затем expired recovery age | [W26](tasks/W26-roles.md) | MODEL + INTEGRATION/SIM + TARGET/HIL |
| T15 | PRIMARY execution fault при сохраняющемся допустимом sync path | [W26](tasks/W26-roles.md) | INTEGRATION/SIM + TARGET/HIL |
| T16 | Partition, reboot старого PRIMARY, replay output frames | [W25](tasks/W25-output-authority.md) | MODEL + INTEGRATION/SIM + TARGET/HIL |
| T17 | Link loss во время CLAIMING и partial acquire | [W25](tasks/W25-output-authority.md) | MODEL + INTEGRATION/SIM + TARGET/HIL |
| T18 | Checkpoint неполный/повреждён/устарел | [W24](tasks/W24-replication.md) | INTEGRATION/SIM |
| T19 | HA failover во время каждого этапа Test/Untest/Finalize | [W27](tasks/W27-ha-deployment.md) | MODEL + INTEGRATION/SIM + TARGET/HIL |
| T20 | Power loss на каждом durable step firmware/config/retain | [W20](tasks/W20-durable-store.md) | MODEL + INTEGRATION/SIM + TARGET/HIL |
| T21 | Неизвестная mixed-version пара при rolling update | [W22](tasks/W22-firmware.md) | INTEGRATION/SIM + TARGET/HIL |
| T22 | Повтор engineering request, потеря ACK, reconnect/reboot | [W30](tasks/W30-engineering-api.md) | MODEL + INTEGRATION/SIM |
| T23 | Wall clock jump, monotonic restart/wrap, peer clock mismatch и takeover с активными timers | [W06](tasks/W06-execution-budget.md) | INTEGRATION/SIM + TARGET/HIL |
| T24 | Выходное значение достигло предельного age | [W12](tasks/W12-effects.md) | INTEGRATION/SIM + TARGET/HIL |
| T25 | Исчезло storage / исчерпан audit log | [W20](tasks/W20-durable-store.md) | INTEGRATION/SIM + TARGET/HIL |
| T26 | Linux provider заменён Zephyr/RT-Thread/Ariel provider при одинаковом поддержанном semantic profile | [W38](tasks/W38-portable-core.md) | INTEGRATION/SIM + TARGET/HIL |
| T27 | Поворот RUN при boot, после fault и после Clear | [W09](tasks/W09-mode-key.md) | INTEGRATION/SIM |
| T28 | CPU/I/O replacement, stale restore, чужая hardware/config identity | [W36](tasks/W36-operations.md) | INTEGRATION/SIM + TARGET/HIL |
| T29 | Закрытие IDE, потеря engineering link и попытка debug halt в RUN | [W32](tasks/W32-ide.md) | INTEGRATION/SIM |
| T30 | Новая board revision/BSP с прежними domain contracts | [W39](tasks/W39-zephyr.md) | INTEGRATION/SIM + TARGET/HIL |
| T31 | Добавление/удаление optional middleware и его overload | [W03](tasks/W03-composition.md) | INTEGRATION/SIM + TARGET/HIL |
| T32 | Protocol FB / debug agent пытается использовать raw driver write вместо output admission | [W12](tasks/W12-effects.md) | INTEGRATION/SIM + TARGET/HIL |
| T33 | Driver/NIC reset с inflight DMA, stale completion и восстановлением session | [W14](tasks/W14-ethernet-ip.md) | INTEGRATION/SIM + TARGET/HIL |
| T34 | Замена OS/middleware component с совместимым API, но иным ABI или timing profile | [W22](tasks/W22-firmware.md) | INTEGRATION/SIM + TARGET/HIL |
| T35 | Две истории PID/TON/edge/hysteresis с одинаковыми текущими входами | [W37](tasks/W37-iec-libraries.md) | STATIC/PROPERTY + INTEGRATION/SIM |
| T36 | Повтор полного входа pure evaluator; изменение ambient clock/global state | [W02](tasks/W02-contracts.md) | STATIC/PROPERTY + INTEGRATION/SIM |
| T37 | Cache READY при смене config/binding, revoke или истечении freshness без нового сообщения | [W09](tasks/W09-mode-key.md) | MODEL + INTEGRATION/SIM |
| T38 | Два параллельных запроса получили allow по одной revision; между ними revoke | [W30](tasks/W30-engineering-api.md) | MODEL + INTEGRATION/SIM |
| T39 | Cancel/reset, новая операция, затем late completion/timeout старой | [W02](tasks/W02-contracts.md) | MODEL + INTEGRATION/SIM |
| T40 | Cold/warm/provider restart и HA с native handles в локальной памяти | [W21](tasks/W21-retain-reset.md) | INTEGRATION/SIM |
| T41 | Stateful algorithm реализован pure step, максимальный StateStore и максимальные входы | [W19](tasks/W19-capture.md) | INTEGRATION/SIM |
| T42 | Artifact проверен; сменились trust/runtime/config prerequisites; Candidate upload отклонён | [W08](tasks/W08-catalog.md) | INTEGRATION/SIM |
| T43 | Добавлен N+1 экземпляр того же filter/key qualifier/diagnostic resource | [W44](tasks/W44-n-plus-one.md) | STATIC/PROPERTY + INTEGRATION/SIM |
| T44 | Power loss между journal, логическим commit, physical effect и ACK | [W18](tasks/W18-deployment.md) | MODEL + INTEGRATION/SIM + TARGET/HIL |
| T45 | Bare-metal build полного portable dependency closure и OS-specific import в одном общем модуле | [W38](tasks/W38-portable-core.md) | STATIC/PROPERTY + INTEGRATION/SIM |
| T46 | Exact-match Test/Untest с максимальным live state и instrumentation allocation/copy | [W16](tasks/W16-exact-activation.md) | STATIC/PROPERTY + INTEGRATION/SIM |
| T47 | Непрерывная сессия и серия host rounds; минимум две IEC tasks с разными периодами | [W05](tasks/W05-execution-session.md) | STATIC/PROPERTY + INTEGRATION/SIM |
| T48 | NIC queue/reassembly/pool исчерпаны, giant/corrupt/reordered frames | [W23](tasks/W23-ha-transport.md) | INTEGRATION/SIM + TARGET/HIL |
| T49 | Power cut между каждым data/name/marker durable step; flash erase при работающем scan | [W20](tasks/W20-durable-store.md) | INTEGRATION/SIM + TARGET/HIL |
| T50 | Epoch около максимума, promotion, reboot одного/обоих peers, delayed old frame | [W25](tasks/W25-output-authority.md) | MODEL + INTEGRATION/SIM + TARGET/HIL |
| T51 | В manifest два logical channels через один интерфейс или общую зависимость; Ariel штатный single-interface stack | [W23](tasks/W23-ha-transport.md) | INTEGRATION/SIM + TARGET/HIL |
| T52 | Engineering, HA revoke и TEST/Test Edits/START одновременно; completion между validate и barrier | [W27](tasks/W27-ha-deployment.md) | INTEGRATION/SIM |
| T53 | IEC infinite loop при software watchdog, затем отказ control thread/OS | [W06](tasks/W06-execution-budget.md) | INTEGRATION/SIM + TARGET/HIL |
| T54 | Одинаковый I32 layout при изменении semantic type; I32→F32 на 16 777 217; explicit preserve | [W07](tasks/W07-semantic-schema.md) | STATIC/PROPERTY + INTEGRATION/SIM |
| T55 | Отказ/повреждение management domain и давление на память | [W13](tasks/W13-linux-boot.md) | INTEGRATION/SIM + TARGET/HIL |
| T56 | Async task долго не yield, высокий IRQ/network load, flash/crypto/DMA contention | [W42](tasks/W42-budgets.md) | INTEGRATION/SIM + TARGET/HIL |
| T57 | Частичный pair persist, потеря ACK, reboot I/O authority, старый backup | [W25](tasks/W25-output-authority.md) | MODEL + INTEGRATION/SIM + TARGET/HIL |
| T58 | Порт требует отсутствующий safe API; попытка добавить lint suppression/unsafe обход | [W38](tasks/W38-portable-core.md) | STATIC/PROPERTY + INTEGRATION/SIM |
| T59 | Cross-target artifact: другая word size/endian/alignment/FP implementation и schema version | [W07](tasks/W07-semantic-schema.md) | STATIC/PROPERTY + INTEGRATION/SIM |
| T60 | Добавление ещё одной ОС/платы/NIC и protocol adapter того же класса | [W44](tasks/W44-n-plus-one.md) | STATIC/PROPERTY + INTEGRATION/SIM + TARGET/HIL |
| T61 | Сквозной cold boot→Start→input change→scan→output без подключённой IDE | [W15](tasks/W15-single-slice.md) | INTEGRATION/SIM + TARGET/HIL |
| T62 | Stale/bad inputs, исчезновение и замена I/O module на том же slot | [W11](tasks/W11-process-image.md) | INTEGRATION/SIM + TARGET/HIL |
| T63 | Два writers одного output, force и IEC write, TEST и protocol FB | [W12](tasks/W12-effects.md) | INTEGRATION/SIM + TARGET/HIL |
| T64 | Активация generation с новым BindingPlan при inflight I/O/force и crash каждого commit step | [W18](tasks/W18-deployment.md) | MODEL + INTEGRATION/SIM + TARGET/HIL |
| T65 | Warm/cold/origin reset, retain schema mismatch, power cut и stale restore | [W21](tasks/W21-retain-reset.md) | INTEGRATION/SIM + TARGET/HIL |
| T66 | Engineering flood/disconnect/reconnect, failed privileged request и full logs | [W30](tasks/W30-engineering-api.md) | INTEGRATION/SIM + TARGET/HIL |
| T67 | Control task не получает release, хотя process/diagnostic heartbeat продолжается | [W13](tasks/W13-linux-boot.md) | INTEGRATION/SIM + TARGET/HIL |
| T68 | Controller process restart, firmware trial failure, repeated recovery/reboot | [W22](tasks/W22-firmware.md) | INTEGRATION/SIM + TARGET/HIL |
| T69 | Один EtherNet/IP provider обслуживает native rack и external PLC variable | [W14](tasks/W14-ethernet-ip.md) | INTEGRATION/SIM + TARGET/HIL |
| T70 | Один physical device имеет output owner и несколько input-only/listen-only consumers | [W10](tasks/W10-binding.md) | INTEGRATION/SIM + TARGET/HIL |
| T71 | External updates приходят во время task, subscription теряет сообщения | [W11](tasks/W11-process-image.md) | INTEGRATION/SIM |
| T72 | Native mapping/scaling/quality policy меняется при старых frames и подписчиках | [W10](tasks/W10-binding.md) | INTEGRATION/SIM + TARGET/HIL |
| T73 | Третий candidate при занятых двух banks; тот же scenario с bounded pool из трёх slots | [W08](tasks/W08-catalog.md) | INTEGRATION/SIM |
| T74 | Structural trial записал новые retain values, затем power loss до finalize | [W21](tasks/W21-retain-reset.md) | MODEL + INTEGRATION/SIM |
| T75 | Crash вокруг new-generation replica ACK и первого output нового кода | [W27](tasks/W27-ha-deployment.md) | MODEL + INTEGRATION/SIM + TARGET/HIL |
| T76 | Полный power loss PRIMARY одновременно обрывает оба optical links | [W28](tasks/W28-ha-qualification.md) | MODEL + INTEGRATION/SIM + TARGET/HIL |
| T77 | Один sink не подтверждает fence, authority link пропал либо authority перезагрузился | [W25](tasks/W25-output-authority.md) | MODEL + INTEGRATION/SIM + TARGET/HIL |
| T78 | Scan/checkpoint seq растёт, owner term неизменен; reboot/overflow/restore authority | [W25](tasks/W25-output-authority.md) | MODEL + INTEGRATION/SIM + TARGET/HIL |
| T79 | EtherNet/IP endpoint не поддерживает требуемое fencing, direct bypass gateway доступен | [W14](tasks/W14-ethernet-ip.md) | INTEGRATION/SIM + TARGET/HIL |
| T80 | Impulse/external command применён, ACK потерян, затем takeover/retry | [W24](tasks/W24-replication.md) | MODEL + INTEGRATION/SIM + TARGET/HIL |
| T81 | Два инженерных клиента отправили конфликтующие commands с одной revision | [W30](tasks/W30-engineering-api.md) | MODEL + INTEGRATION/SIM |
| T82 | Auth/key revoke во время prepare/commit/force, critical queue переполнена | [W29](tasks/W29-security.md) | MODEL + INTEGRATION/SIM + TARGET/HIL |
| T83 | HA nodes имеют разные node-local IP/MAC/BootId при одной logical generation | [W26](tasks/W26-roles.md) | INTEGRATION/SIM |
| T84 | Медленные retain/HA/migration consumers одновременно читают MutationCapture | [W19](tasks/W19-capture.md) | INTEGRATION/SIM |
| T85 | Trial firmware не подтверждена; security counter изменён; old image запрещена | [W22](tasks/W22-firmware.md) | INTEGRATION/SIM + TARGET/HIL |
| T86 | Diagnostic exporter упал, log storage full, наблюдатель читает stale RUNNING | [W33](tasks/W33-observability.md) | INTEGRATION/SIM |
| T87 | UTC/PTP quality потеряна, OPC UA exporter получает bad/stale source value | [W34](tasks/W34-dcs-data.md) | INTEGRATION/SIM + TARGET/HIL |
| T88 | HMI/historian reconnect, alarm duplicates/overflow и out-of-order timestamps | [W34](tasks/W34-dcs-data.md) | INTEGRATION/SIM |
| T89 | Отсутствующая app, испорченная app и отдельно неbootable firmware | [W13](tasks/W13-linux-boot.md) | INTEGRATION/SIM + TARGET/HIL |
| T90 | Runtime binary update замаскирован под application hot edit; неизвестная mixed firmware HA pair | [W22](tasks/W22-firmware.md) | INTEGRATION/SIM + TARGET/HIL |
| T91 | N+1 OS/provider/board того же класса, но с меньшими capacities или иным jitter | [W44](tasks/W44-n-plus-one.md) | STATIC/PROPERTY + INTEGRATION/SIM + TARGET/HIL |
| T92 | Commissioning→backup→CPU replacement→restore→HA rejoin | [W36](tasks/W36-operations.md) | INTEGRATION/SIM + TARGET/HIL |
| T93 | Degraded edit на PRIMARY без связи со standby, затем power loss; standby имеет свежий old-generation checkpoint | [W27](tasks/W27-ha-deployment.md) | MODEL + INTEGRATION/SIM + TARGET/HIL |
| T94 | Pair Stop/maintenance и одновременно PRIMARY crash или standby acquire | [W27](tasks/W27-ha-deployment.md) | MODEL + INTEGRATION/SIM + TARGET/HIL |
| T95 | Power loss/потеря receipt во время RecoveryAdmission update, затем abort/revert | [W27](tasks/W27-ha-deployment.md) | MODEL + INTEGRATION/SIM + TARGET/HIL |

## INV01–INV30

| ID | Инвариант | Primary integration owner |
|---|---|---|
| INV01 | Authoritative state изменяет только его owner/явно переданное право записи | [W02](tasks/W02-contracts.md) |
| INV02 | Start commit требует актуальных prerequisites и принятого intent | [W09](tasks/W09-mode-key.md) |
| INV03 | Принятый application-driven effect связан с RUN, совместимым binding и действующим permit/owner | [W12](tasks/W12-effects.md) |
| INV04 | В output consistency group нет двух одновременно принимаемых физических владельцев | [W25](tasks/W25-output-authority.md) |
| INV05 | Replayed/stale frame не восстанавливает отозванное право | [W25](tasks/W25-output-authority.md) |
| INV06 | Automatic promotion требует qualified resume state, local readiness, действующего RecoveryAdmission и whole-group exclusive grant; link timeout сам по себе недостаточен | [W26](tasks/W26-roles.md) |
| INV07 | Execution не использует освобождённую/непроверенную generation или несовместимую schema | [W08](tasks/W08-catalog.md) |
| INV08 | Task/consistency unit не видит половину старого/нового execution binding | [W16](tasks/W16-exact-activation.md) |
| INV09 | Неудачная candidate preparation не инвалидирует работающий Original | [W08](tasks/W08-catalog.md) |
| INV10 | TEST не допускает application-driven live effects ни через один обходной путь | [W12](tasks/W12-effects.md) |
| INV11 | Revocation/fault вызывает предписанную физическую реакцию в квалифицированный bound | [W13](tasks/W13-linux-boot.md) |
| INV12 | Power loss даёт целое допустимое durable state либо recovery, не mixed version | [W20](tasks/W20-durable-store.md) |
| INV13 | Ack/Clear/расширение permissions сами по себе не создают Start | [W09](tasks/W09-mode-key.md) |
| INV14 | Presentation/diagnostic status не является источником operational authority | [W33](tasks/W33-observability.md) |
| INV15 | Readiness cache используется только при актуальных dependencies/revisions/freshness | [W09](tasks/W09-mode-key.md) |
| INV16 | Semantic state scope и time semantics сохранены независимо от выбранного FSM/struct представления | [W07](tasks/W07-semantic-schema.md) |
| INV17 | Старый operation/boot/session completion не завершает новую операцию | [W02](tasks/W02-contracts.md) |
| INV18 | Pure evaluator с одинаковыми полными входами даёт одинаковый результат в объявленной числовой модели | [W02](tasks/W02-contracts.md) |
| INV19 | Замена OS/board provider не меняет domain semantics; отсутствующая capability явно блокирует операцию | [W38](tasks/W38-portable-core.md) |
| INV20 | После acknowledged durable commit восстанавливается он либо допустимый последующий; durable не означает send queued | [W20](tasks/W20-durable-store.md) |
| INV21 | Commit/owner identity не повторяется после overflow/reboot/restore | [W25](tasks/W25-output-authority.md) |
| INV22 | Последовательные host rounds не сбрасывают scheduler history вне lifecycle | [W05](tasks/W05-execution-session.md) |
| INV23 | Native/External класс принадлежит binding relation; provider не выдаёт write ownership по input subscription | [W10](tasks/W10-binding.md) |
| INV24 | Value, quality, binding generation и age одного snapshot согласованы | [W11](tasks/W11-process-image.md) |
| INV25 | Trial activation не уничтожает обещанную compatible boot/retain recovery версию | [W21](tasks/W21-retain-reset.md) |
| INV26 | Неизвестный исход неидемпотентного эффекта не превращается в автоматический безопасный retry | [W24](tasks/W24-replication.md) |
| INV27 | После first effect новой generation HA не восстанавливает несовместимую старую; изолированная реплика не обходит RecoveryAdmission | [W27](tasks/W27-ha-deployment.md) |
| INV28 | Потеря management/diagnostics не блокирует обязательный control/containment path в заявленной isolation model | [W13](tasks/W13-linux-boot.md) |
| INV29 | Outputs не удерживаются бесконечно replay/старой freshness; expiry исполняется даже при остановленной CPU | [W12](tasks/W12-effects.md) |
| INV30 | Production capability заявляется только для прошедшего gates versioned target/workload profile | [W43](tasks/W43-release.md) |

## S01–S12

| ID | Правило | Primary owner |
|---|---|---|
| S01 | Для каждого компонента указать границу и какую историю он хранит между вызовами; объяснить необходимость этой истории | [W03](tasks/W03-composition.md) |
| S02 | Pure evaluator получает все влияющие данные явно, включая time/policy; не читает ambient globals и не выполняет эффекты | [W02](tasks/W02-contracts.md) |
| S03 | FSM/EFSM вводится для существенных фаз протокола, ожидания, cancellation и recovery. Алгоритму фильтра или immutable manifest FSM не навязывается | [W03](tasks/W03-composition.md) |
| S04 | Каждый компонент имеет owner/state/contract/failure/RT/persistence/security/platform запись. Готовые OS subsystems оцениваются на используемой контрактной границе | [W03](tasks/W03-composition.md) |
| S05 | Каждый authoritative mutable факт имеет одного логического writer. Передача ownership — протокол; readers используют coherent snapshots | [W02](tasks/W02-contracts.md) |
| S06 | READY/health/status — проекции с provenance, revisions, freshness. Latched fault и acknowledgement — самостоятельная история, а не кэш | [W33](tasks/W33-observability.md) |
| S07 | Реестр состояния содержит identity, owner, type/schema, capacity, writers/readers, atomicity, lifetime, reset, retain, replication, migration, security и failure reaction | [W07](tasks/W07-semantic-schema.md) |
| S08 | Память между scans не означает сохранность после power loss. Raw pointers, OS handles и timestamps чужого clock domain не входят в переносимый checkpoint | [W21](tasks/W21-retain-reset.md) |
| S09 | Pure decision → owner admission/revalidation → local commit → effect enforcement → receipt. Decision не является бессрочным полномочием | [W02](tasks/W02-contracts.md) |
| S10 | Никаких full-state copy, heap allocation, remote store или неограниченного event log по требованию функционального стиля на RT path | [W16](tasks/W16-exact-activation.md) |
| S11 | Rust ownership/typestate укрепляют локальные границы; distributed ownership, deadline и durability доказываются отдельно | [W04](tasks/W04-verification.md) |
| S12 | Алгоритмы проверяются траекториями, protocols — traces/races/recovery, containers — capacity/lifetime, система — межкомпонентными инвариантами | [W04](tasks/W04-verification.md) |

## 16 REQ-PORT

IDs сохранены. Обозначение ниже — ссылка на исходное требование, не повторная declaration для generator. При перемещении обязанности в новый crate W04 сохраняет traceability старого ID и явно решает bridge/supersession; нельзя молча фильтровать прежний owner slug.

| Requirement | Owning task | Integration target / evidence |
|---|---|---|
| `REQ-PORT-vm-001` | [W38](tasks/W38-portable-core.md) | owning crate slug `vm`; registration + meaningful software tests + target evidence по scope |
| `REQ-PORT-vm-002` | [W05](tasks/W05-execution-session.md) | owning crate slug `vm`; registration + meaningful software tests + target evidence по scope |
| `REQ-PORT-vm-003` | [W06](tasks/W06-execution-budget.md) | owning crate slug `vm`; registration + meaningful software tests + target evidence по scope |
| `REQ-PORT-runtime-001` | [W05](tasks/W05-execution-session.md) | owning crate slug `runtime`; registration + meaningful software tests + target evidence по scope |
| `REQ-PORT-runtime-002` | [W16](tasks/W16-exact-activation.md) | owning crate slug `runtime`; registration + meaningful software tests + target evidence по scope |
| `REQ-PORT-runtime-003` | [W07](tasks/W07-semantic-schema.md) | owning crate slug `runtime`; registration + meaningful software tests + target evidence по scope |
| `REQ-PORT-runtime-004` | [W09](tasks/W09-mode-key.md) | owning crate slug `runtime`; registration + meaningful software tests + target evidence по scope |
| `REQ-PORT-runtime-005` | [W08](tasks/W08-catalog.md) | owning crate slug `runtime`; registration + meaningful software tests + target evidence по scope |
| `REQ-PORT-runtime-006` | [W18](tasks/W18-deployment.md) | owning crate slug `runtime`; registration + meaningful software tests + target evidence по scope |
| `REQ-PORT-runtime-007` | [W38](tasks/W38-portable-core.md) | owning crate slug `runtime`; registration + meaningful software tests + target evidence по scope |
| `REQ-PORT-redundancy-001` | [W23](tasks/W23-ha-transport.md) | owning crate slug `redundancy`; registration + meaningful software tests + target evidence по scope |
| `REQ-PORT-redundancy-002` | [W25](tasks/W25-output-authority.md) | owning crate slug `redundancy`; registration + meaningful software tests + target evidence по scope |
| `REQ-PORT-redundancy-003` | [W23](tasks/W23-ha-transport.md) | owning crate slug `redundancy`; registration + meaningful software tests + target evidence по scope |
| `REQ-PORT-redundancy-004` | [W26](tasks/W26-roles.md) | owning crate slug `redundancy`; registration + meaningful software tests + target evidence по scope |
| `REQ-PORT-redundancy-005` | [W44](tasks/W44-n-plus-one.md) | owning crate slug `redundancy`; registration + meaningful software tests + target evidence по scope |
| `REQ-PORT-container-001` | [W07](tasks/W07-semantic-schema.md) | owning crate slug `container`; registration + meaningful software tests + target evidence по scope |

## DA01–DA28

| Контрпример | Primary task |
|---|---|
| DA01. IEC infinite loop | [W06](tasks/W06-execution-budget.md) |
| DA02. Native panic/deadlock | [W13](tasks/W13-linux-boot.md) |
| DA03. Task вообще не released | [W13](tasks/W13-linux-boot.md) |
| DA04. Потерян I/O connection | [W14](tasks/W14-ethernet-ip.md) |
| DA05. Модуль заменён на том же slot | [W11](tasks/W11-process-image.md) |
| DA06. Force остался при disconnect | [W12](tasks/W12-effects.md) |
| DA07. Flash power cut | [W20](tasks/W20-durable-store.md) |
| DA08. Retain corrupted | [W21](tasks/W21-retain-reset.md) |
| DA09. Filesystem read-only/full | [W20](tasks/W20-durable-store.md) |
| DA10. Firmware не стартует | [W22](tasks/W22-firmware.md) |
| DA11. App отсутствует/испорчен | [W13](tasks/W13-linux-boot.md) |
| DA12. Exact edit даёт скачок output | [W16](tasks/W16-exact-activation.md) |
| DA13. Migration dirty backlog растёт | [W17](tasks/W17-migration.md) |
| DA14. Инженер исчез после commit | [W30](tasks/W30-engineering-api.md) |
| DA15. PRIMARY погиб при hot edit | [W27](tasks/W27-ha-deployment.md) |
| DA16. SECONDARY reset | [W24](tasks/W24-replication.md) |
| DA17. Потерян один optical path | [W23](tasks/W23-ha-transport.md) |
| DA18. Потеря 0/0, PRIMARY жив | [W26](tasks/W26-roles.md) |
| DA19. PRIMARY power loss и 0/0 | [W28](tasks/W28-ha-qualification.md) |
| DA20. Authority reboot/partial fencing | [W25](tasks/W25-output-authority.md) |
| DA21. Старый backup/epoch wrap | [W25](tasks/W25-output-authority.md) |
| DA22. Clock jump/reset | [W06](tasks/W06-execution-budget.md) |
| DA23. Protocol/management crash | [W13](tasks/W13-linux-boot.md) |
| DA24. Diagnostic service crash | [W33](tasks/W33-observability.md) |
| DA25. ACK потерян после impulse | [W24](tasks/W24-replication.md) |
| DA26. Native и external через один stack | [W14](tasks/W14-ethernet-ip.md) |
| DA27. Новый OS «собирается» | [W38](tasks/W38-portable-core.md) |
| DA28. Ошибка all-in-one Supervisor | [W03](tasks/W03-composition.md) |

## Release gates

| Gate | Primary evidence collector | Integration dependency |
|---|---|---|
| G01. Scope/process | [W42](tasks/W42-budgets.md) | W43 проверяет applicable profile; missing evidence → deny |
| G02. Architecture/build | [W03](tasks/W03-composition.md) | W43 проверяет applicable profile; missing evidence → deny |
| G03. Time/resources | [W42](tasks/W42-budgets.md) | W43 проверяет applicable profile; missing evidence → deny |
| G04. Physical containment | [W13](tasks/W13-linux-boot.md) | W43 проверяет applicable profile; missing evidence → deny |
| G05. Deployment/storage | [W18](tasks/W18-deployment.md) | W43 проверяет applicable profile; missing evidence → deny |
| G06. HA | [W28](tasks/W28-ha-qualification.md) | W43 проверяет applicable profile; missing evidence → deny |
| G07. Security | [W29](tasks/W29-security.md) | W43 проверяет applicable profile; missing evidence → deny |
| G08. Verification | [W04](tasks/W04-verification.md) | W43 проверяет applicable profile; missing evidence → deny |
| G09. Operations/ecosystem | [W36](tasks/W36-operations.md) | W43 проверяет applicable profile; missing evidence → deny |
| G10. Release/site | [W43](tasks/W43-release.md) | W43 проверяет applicable profile; missing evidence → deny |


## Completeness не равно correctness

Проверка комплекта сверяет множество IDs с pinned spec, ровно одного primary owner, существующие W links, DAG и локальные acceptance IDs. Она не запускает future tests. Реализация добавляет двустороннюю связь requirement→test/model/HIL→actual result→profile, а release gate проверяет её полноту и смысл.

Для compiler/runtime conformance нужны и registry checks, и behavioral oracle. Для hardware claim нужен referenced physical evidence. Закрыть T76 ссылкой на `assert!(can_takeover(...))` недопустимо.
