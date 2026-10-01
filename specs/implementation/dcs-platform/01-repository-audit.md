# 01. Основание в текущем форке

Прочитана целевая спецификация целиком, инструкции/steering, дерево ветки и выбранные implementation paths compiler/container/VM/runtime/HA/CLI/IDE. Рабочая ветка `docs/dcs-plc-platform-v3`, commit `0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3`. Полный source checkout выполнен; **это focused source review, не утверждение о чтении всех файлов и не выполненный build**.

Спецификация содержит результат более раннего аудита `lint-fences@8a7f6d0`; в этом комплекте ключевые выводы заново сопоставлены с кодом указанной документационной ветки. `main` не подставлялся вместо baseline. Существующие tests прочитаны выборочно, их прохождение не проверялось отсутствующим Rust toolchain.

| Подтверждённая точка | Факт чтения / следствие | Задания |
|---|---|---|
| `compiler/project/src/compile.rs::compile` | Один parse/analyze/codegen pipeline; diagnostics блокируют artifact; stable IDs приходят из project sidecar | W07/W31 |
| `compiler/container/src/lib.rs`, `Cargo.toml` | no_std контейнер и отдельная std feature; это не no_std всей VM | W38 |
| `compiler/vm/src/vm.rs::load` | При каждом load заново записывает TaskState: next_due, scan count и execution history | W05 |
| `compiler/runtime/src/host.rs::run_session` | Повторно вызывает Vm::new().load(...).resume(...); сохранение rounds не сохраняет всю scheduler history | W05 |
| `compiler/vm/src/vm.rs::run_round` | INPUT_FREEZE/OUTPUT_FLUSH — no-op; elapsed/watchdog после возврата run_instance, std::time::Instant | W06/W11/W13 |
| `compiler/runtime/src/online_change.rs::swap_buffers` | VmBuffers::from_container и copy vars/data_region, затем reload scheduler | W16 |
| `compiler/runtime/src/host.rs::apply_migration_swap` | Allocate/init/copy migration на boundary; не доказана bounded pause | W17 |
| `compiler/runtime/src/snapshot.rs` | Snapshot vars+data_region и layout hash; целевой semantic checkpoint шире | W07/W19/W24 |
| `compiler/runtime/src/generation.rs` | LogicGeneration/ApplicationGeneration — u32 newtypes; immutable manifest model предстоит расширить | W02/W08 |
| `compiler/runtime/src/commands.rs` | Полезные commands/status/identity, но direct execute(host) и прежняя response модель не закрывают v3 operation/durability/auth | W18/W29/W30 |
| `compiler/ironplc-redundancy/src/hal.rs::NicPort` | poll возвращает Vec; port seam есть, bounded caller pool ещё требуется | W23 |
| `compiler/ironplc-redundancy/src/crossload.rs` | Собственный обмен candidate/snapshot, Vec/BTreeMap и работа с HostMode; переносится на common catalog/capture | W24/W27 |
| `compiler/ironplc-redundancy/src/epoch.rs` | Volatile saturating u32 выдаётся на rounds; не durable OutputAuthority term | W02/W25 |
| `compiler/ironplc-redundancy/src/fencing.rs` | Есть acquisition abstraction/simulation; physical external enforcement этим чтением не доказан | W12/W25/W28 |
| `compiler/vm-cli/src/slot_store.rs` | A/B files, markers, file sync; whole durability/parent metadata и exhaustion требуют отдельного доказательства | W20/W21/W22 |
| `integrations/vscode/src/connectionState.ts` | Connection/reconnect уже отделены в тестируемую логику; сохранить этот seam | W32 |
| `integrations/vscode/src/hotEditSession.ts` | Есть command/protocol/migration structures; size-equality advice не semantic exact proof | W07/W30/W32 |
| `compiler/spec_requirements_gen/src/lib.rs` | Формат REQ содержит crate slug; generator фильтрует по package без префикса ironplc-, сканирует spec_test markers | W04 |
| `compiler/runtime/build.rs`, `ironplc-redundancy/build.rs` | Генерируют V-code constants; включение platform spec conformance пока не выполнено | W04 |
| `compiler/justfile` | Build/coverage 85%/clippy/fmt/dupes; no_std compile только container на host, недостаточно для port claim | W04/W38/W43 |
| `specs/steering/compiler-architecture.md` | В обзорном pipeline ещё написано «Code Generation (future)», хотя codegen и его вызов существуют. Это documentation drift; не основание создавать codegen заново | W01 |

## Что сохранять

Safe Rust fences; один compiler pipeline; typed diagnostics и generated registries; curated exports; VM typestate; caller-owned buffers; scoped host permit/revalidation; stable-ID sidecar; имеющиеся pure policy/test seams. Рефакторинг извлекает orchestration и укрепляет смысл контрактов, не переписывает проверяемые compiler stages заново.

## Что source review не установил

Не измерены WCET, physical output acceptance, независимость optical links, power-fail durability, full dependency closure на MCU, interoperability EtherNet/IP и реальная protected deployment. Наличие trait/test/README не считается выполнением этих обязательств. Никакое предложение crate map ниже не называется существующим crate.

Ссылки на конкретные baseline files приведены в каждой карточке и разрешаются в repository. Для нового execution branch сначала W01 сравнивает изменения с этим snapshot.
