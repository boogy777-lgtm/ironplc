# 07. N+1: механизм, а не число модулей

```text
N := required same-class behaviors
M(N) := independent mechanisms covering that class
prefer min M(N)
```

При сохранении correctness/bounds/ownership:
`same_class(N+1) → prefer M(N+1)=M(N)`; иначе проверить абстракцию.

Одинаковый класс означает одинаковые semantic obligations, authority и failure model. Число structs/crates/adapters/tests не равно M. Universal event bus с десятью скрытыми частными протоколами не уменьшает M.

| Расширение | Сохраняемый механизм | Допустимые изменения | Требуемое evidence |
|---|---|---|---|
| Второй AI module | Native binding + IoCycle | Descriptor/config/capacity | Quality/fallback/replacement suite |
| Следующий field transport | Binding/IoCycle/effect contract | Provider/codec/composition | Actual transport semantics, real device conformance |
| External CIP → OPC UA data | ExternalData binding | Provider/mapping/profile | Stale/sequence/time/write outcome |
| CLI → второй IDE/Web | Controller command API | Client/auth binding | Revision conflicts/reconnect/dedupe |
| Linux → embedded OS | Runtime/state/deployment/HA semantics | Safe ports/composition/profile | Complete closure + same traces + target HIL |
| Два code slots → три | Pins/admission/ActivationTransaction | Capacity/backend | Third candidate/retirement/max memory |
| File → flash | DurableRecordStore contract | Backend/partition/wear policy | Actual power cut/GC/interference |
| Новый alarm/historian consumer | Bounded facts subscription | Adapter/quotas | Removal/overflow/gaps/no scan blocking |
| Additional qualifier/filter instance | Один pure-step/state owner pattern | Instance data/config | Independent histories/capacity |

## Где новый механизм обоснован

Single→HA добавляет отказ другого controller и exclusive physical authority. Concurrent migration добавляет изменяющееся во время copy state и bounded catch-up. Protected deployment добавляет fault containment и IPC obligations. Active-active, три competing writers и SIS меняют класс гарантии. Их нельзя скрыть за feature flag «plugin=true».

## N+1 record для PR

1. Какое semantic обязательство уже обеспечивает baseline mechanism?
2. Что добавлено и почему это same-class либо new-class?
3. Какие owners/contracts/failure domains изменились? Где находится единственный writer?
4. `M_before`, `M_after`, перечень механизмов словами. Не считать crate rename снижением M.
5. Allowed diff paths и actual diff; новые resource/timing limits; старые traces и новые counterexamples.
6. Если core пришлось менять: contract bug, legitimate new invariant либо заплатка? Исправить причину или честно расширить контракт.

W44 проверяет это реальными добавлениями. Более общий trait принимается после двух реальных implementations/consumers, когда он уменьшает дублирование без потери bounds. Abstraction на один будущий callback сама по себе не является улучшением.
