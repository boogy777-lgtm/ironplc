# 19. Агрегация сообщений компилятора (Compiler35220, класс `Messages`)

Ссылки: `docs\14_COMPILER_PHASES.md`, `docs\17_COMPILER_DETERMINISM.md`, `docs\03_ERROR_CATALOG.md`.
Область: `C:\Codesys`. `file:line` — по декомпилу (`decompiled\`), `token` — IL-токены, снятые `dnlib`
(скрипты `tools\dump_method_il.ps1`, `tools\re_dump.ps1`). Program Files — только чтение.

> **Главный вывод.** Отдельного шага «сортировки сообщений» НЕТ. Ни в агрегации, ни в хранилище
> сообщения не переупорядочиваются по `(object, line, column, severity, code)`. Порядок =
> **порядок вставки** (append-only `LList<IMessage>` на категорию) и он **детерминирован**, потому что
> агрегация выполняется однопоточно ПОСЛЕ `Thread.Join` воркеров, а воркеры пишут сообщения не в общее
> хранилище, а в объекты своих POU/сигнатур. Дубликаты отбрасываются по ключу
> `(MessageId, Position, PositionOffset, Text, Severity, ObjectGuid)` — **первое вхождение выигрывает**.

---

## 1. Цепочка агрегации

### 1.1. Точка входа — `Messages.\u0001(_ICompileContext, IMessageStorage, IMessageCategory)`

`decompiled\Compiler35220.plugin\_3S\CoDeSys\Compiler35220\Messaging\Messages.cs:28` (token `0600352D`):

```
30: APEnvironmentFacade.Instance.LanguageModelMgr.OnBeforeMessageOutput(...);
31: \u0018.\u0011 u = new \u0018.\u0011(\u0002, \u0003, \u0004);      // контекст агрегации
32: if (Messages.\u0001(\u0002.ApplicationGuid, \u0003, \u0004, u))  // fatal: пропущенная библиотека
34:     return u.\u0001;
36: Messages.\u0001(u);                                  // (2) глобальные диагностики
37: Messages.\u0004(u);                                  // (3) все сигнатуры
38: Messages.\u0003(u);                                  // (4) скомпилированные POU
39: Messages.\u0002(u);                                  // (5) суммаризованные ошибки библиотек
40: Messages.\u0001(\u0002, \u0003, \u0004, u);          // (6) статистика памяти
41: APEnvironmentFacade.Instance.LanguageModelMgr.OnAfterMessageOutput(...);
```

Шаги 36–40 идут **строго последовательно** и задают порядок попадания в хранилище. Каждый шаг перебирает
детерминированный список языковой модели и вызывает `IMessageStorage.AddMessage(category, msg)`.
Никакого `.OrderBy`/`.Sort` в этой цепочке нет.

### 1.2. Потокобезопасный сбор у воркеров Phase4/Phase5

Воркеры НЕ пишут в общее хранилище:

- `_3S\CoDeSys\Compiler35220\CompilerPhases\CompilerPhase5_Codegenerator.cs:215` — очередь задач строится
  `OrderBy(identity, \u0084.\u0005)` (desc по `NumberOfStatements`, устойчивая сортировка), потоки стартуют
  на `:222`, `Join(500)` — на `:249`.
- `_3S\CoDeSys\Compiler35220\CompilerPhases\CompilerPhase4_Typechecker.cs:234` — воркер вызывает
  `SetMessages(...)`, т.е. сообщения прикрепляются к самому POU/сигнатуре.
- Прямые `APEnvironmentFacade.Instance.AddMessage(...)` в `CompilerPhase5_Codegenerator.cs`
  (`:289, :536, :595, :617, :639, :716, :763, :941, :986, :991`) находятся в главных методах
  (прогресс/online-change), а не в теле воркера `global::\u0018.\u0013`.
- Нормальный вызов агрегации — после завершения генерации: `-\-.402.cs:94`
  (`Messages.\u0001(ComconNew, MessageStorage, ...)`); аварийные — `CompilerPhaseControllerGenerateCode.cs:248,254,706`.

Итог: параллелизм влияет только на то, КТО посчитал сообщение, но не на порядок в хранилище.

### 1.3. Переборы внутри агрегации (источник порядка)

| Шаг | Источник порядка | Где |
|---|---|---|
| (2) глобальные | `LList`, собранный `Messages.\u0001(comCon)` | `Messages.cs:46` → `Messages.cs:306` |
| (3) сигнатуры | `icompileContext.AllSignatureList` | `Messages.cs:136-168` |
| (4) POU | `icompileContext.CompiledPOUList` | `Messages.cs:83-95` |
| (5) суммаризация | `IDictionary<string,int>.Keys` (`SummarizedLibErrors`) | `Messages.cs:60-80` |

`Messages.\u0001(comCon)` (`Messages.cs:306`, token `0600353A`) собирает свой список в фиксированном порядке:
имя устройства (`:315`), проверки интерфейсов (`:328`), container-lib (`:332`), конфликты имён (`:333`),
валидность namespace библиотек (`:334`), строка в `var_in_out` (`:337`), конфликты приложений (`i<j`, `:342-360`),
слишком много приложений (`:365`).

### 1.4. Дедупликация (в контексте `\u0018.\u0011`)

`decompiled\Compiler35220.plugin\-\-.351.cs:66` (token `06003550`):

```
68: \u0015.\u0004 item = new \u0015.\u0004(\u0002);      // ключ по сообщению
69: if (this.\u0001.Contains(item)) return;            // дубликат -> пропустить (первое выигрывает)
73: this.\u0001.Add(item);
74: new \u001E.\u0018(\u0002, this, \u0003, \u0004).\u0001();  // посетитель вывода
```

Ключ `\u0015.\u0004` (token типа `020001AC`, ctor `06001EBD`, `Equals` `06001EBE`, `GetHashCode` `06001EC0`)
берёт из сообщения ровно шесть полей:

```
MessageId, Position (Int64), PositionOffset (Int16), Text (String), Severity, ObjectGuid (Guid)
```

`Equals` — сравнение всех шести; `HashSet` не влияет на порядок (используется только `Contains`/`Add`), поэтому
порядок остаётся порядком первого вхождения. Заметьте: `ProjectHandle` в ключ **не** входит.

### 1.5. Запись в хранилище

`decompiled\Compiler35220.plugin\-\-.352.cs:157` (класс `\u001E.\u0018`, метод `\u0002()`, token `0600355C`):

```
155: if (flag)
157:     this.Messagestorage.AddMessage(this.CMC, icompilerMessage);
```

Перед этим `flag` вычисляется с учётом фильтра `OnFilterMessageOutput`, лимитов warning/error (500) и
суммаризации ошибок библиотек (`\u0004()`, `\u0003()`, `\u0002()` в том же файле) — это только отбор,
не сортировка.

### 1.6. Само хранилище

Конкретная реализация — `_3S.CoDeSys.MessageStorage.MessageStorage : IMessageStorage3`
(`C:\Program Files\CODESYS 3.5.22.10\CODESYS\PlugIns\8f33458b-b754-4ae7-b805-7e841c23781a\3.5.22.10\MessageStorage.plugin.dll`).
Интерфейсы — `decompiled\MessageStorage\IMessageStorage.cs`, `IMessageStorage2.cs`.

Поля (из IL ctor, token `06000025`):

```
LDictionary<Guid, LList<IMessage>> _messages       // категория GUID -> список сообщений
LList<IMessageCategory>            _categories
LList<IMessageCategory>            _queuedCategories
```

`AddMessage` (token `06000012`), ключевой фрагмент:

```
IL_000F: call GetMessageCategoryGuid
IL_001C: callvirt LDictionary<Guid,LList<IMessage>>::ContainsKey
IL_0023..IL_0042: (нет категории) создать LList, _categories.Add, FireCategoryAdded
IL_0047: get_Item(guid)                    // per-category LList
IL_0053: dup; IL_0055: ldnull; IL_0057: Debug.Assert
IL_005D: callvirt LList<IMessage>::Add(message)   // <-- APPEND в конец, без сортировки
IL_0065: FireMessageAdded
```

`GetMessages(category, severityMask)` (token `0600000E`), ключевой фрагмент:

```
IL_0016: get_Item(guid)                    // исходный LList (порядок вставки)
IL_0021: new LList<IMessage>()             // результат
IL_0037: callvirt IMessage::get_Severity
IL_003C: ldarg.2                           // severityMask
IL_003D: and                               // Severity & mask
IL_003E: brfalse.s IL_0047                 // не совпало -> пропустить
IL_0042: callvirt LList<IMessage>::Add     // копирование в порядке итерирования
IL_005B: callvirt LList<IMessage>::ToArray // <-- порядок вставки сохраняется
```

`GetMessages(category)` (token `0600000F`): `IL_0002: ldc.i4.m1` — передаёт `(Severity)(-1)` (все биты),
т.е. «все»; далее тот же метод.

`Severity` (`decompiled\MessageStorage\Severity.cs:9`) — `[Flags] uint`: FatalError=1, Error=2, Warning=4,
Information=8, Text=16, SuppressedWarning=32, SuppressedInformation=64.

**Сортировки по `(object, line, column, severity, code)` нет нигде.** Есть только фильтр по битовой маске
severity и копирование в порядке вставки. `ClearMessages` (`06000010`) и `RemoveMessages` (`06000011`)
также не сортируют (последний — `FindAll` + `RemoveAll`, оставшиеся сохраняют относительный порядок).

---

## 2. Детерминирован ли порядок?

Да, при соблюдении трёх условий:

1. Агрегация идёт **после** `Thread.Join` воркеров (однопоточный fan-in) — `-\-.402.cs:94`,
   `CompilerPhase5_Codegenerator.cs:249`.
2. Переборы (3)/(4) идут по спискам языковой модели (`AllSignatureList`, `CompiledPOUList`), порядок которых
   не зависит от порядка завершения воркеров (база — `CompileContext.m_alCompiledPOUs`; см.
   `docs\17_COMPILER_DETERMINISM.md`).
3. Хранилище — append-only список, выдача — копирование в порядке итерации.

`HashSet` дедупа порядок не меняет (только фильтрует первое вхождение).

### Пробелы (риск недетерминизма)

- **GAP-1.** `Messages.\u0002(u)` (`Messages.cs:60`) и namespace-conflict (`Messages.cs:199`) перебирают
  `LDictionary<...>.Keys`. Семантика `LDictionary` (вставка vs хеш-порядок) не подтверждена — класс лежит в
  `_3S.CoDeSys.Utilities`, декомпил/бинарь в `C:\Codesys` отсутствует. Если это не ordered-dictionary,
  порядок шага (5) нестабилен.
- **GAP-2.** Ключ дедупа `(MessageId, Position, PositionOffset, Text, Severity, ObjectGuid)` не содержит
  `ProjectHandle`: у сообщений из разных проектов с одинаковыми прочими полями одно будет молча потеряно.
- **GAP-3.** В `.text`-сравнении используется `String.op_Equality` (ordinal), т.е. дубли при разном регистре
  текста не склеиваются. Для локализованных строк `Text` может содержать подстановки — тогда дедуп зависит от
  содержимого.
- **GAP-4.** `LList`/`LDictionary` не декомпилированы; поведение `Add`/`ToArray`/`FindAll` выведено из
  типа и семантики вызовов, а не из IL.

---

## 3. Как повторить в Rust (100% порядок)

```rust
use std::collections::{HashMap, HashSet};

#[derive(Clone, Copy, PartialEq, Eq)]
pub struct Severity(pub u32); // битовая маска, соответствует [Flags] Severity

#[derive(Clone, PartialEq, Eq, Hash)]
pub struct MsgKey {
    message_id: i32,      // MessageId
    position: i64,        // Position
    position_offset: i16, // PositionOffset
    text: String,         // Text (ordinal)
    severity: u32,        // Severity
    object_guid: [u8; 16],// ObjectGuid
}

pub struct Message { pub key: MsgKey, pub severity: u32 }

#[derive(Default)]
pub struct MessageStorage {
    // порядок категорий и сообщений = порядок вставки (аналог LList)
    categories: Vec<[u8; 16]>,
    messages: HashMap<[u8; 16], Vec<Message>>,
}

impl MessageStorage {
    pub fn add_message(&mut self, category: [u8; 16], m: Message) {
        // AddMessage: get-or-create + push в конец, БЕЗ сортировки
        if !self.messages.contains_key(&category) {
            self.categories.push(category);              // _categories.Add
        }
        self.messages.entry(category).or_default().push(m); // LList.Add
    }

    pub fn get_messages(&self, category: [u8; 16], mask: u32) -> Vec<&Message> {
        // порядок вставки + фильтр (Severity & mask) != 0
        match self.messages.get(&category) {
            None => Vec::new(),
            Some(list) => list.iter().filter(|m| (m.severity & mask) != 0).collect(),
        }
    }

    pub fn get_all(&self, category: [u8; 16]) -> Vec<&Message> {
        self.get_messages(category, u32::MAX) // соответствует (Severity)-1
    }
}

/// Дедуп агрегации: первое вхождение выигрывает.
#[derive(Default)]
pub struct Deduper(HashSet<MsgKey>);

impl Deduper {
    /// Соответствует \u0018.\u0011.\u0001(msg,sig,lib): false => уже было, сообщение пропускается.
    pub fn insert_first_wins(&mut self, key: &MsgKey) -> bool {
        self.0.insert(key.clone()) // HashSet::insert == true, если ключа не было
    }
}
```

Порядок вызовов при агрегации в Rust повторить строго как в §1.1:

```
1) global   -> для каждого msg из build_global_lists(com_con): storage.add_message(cat, msg)
2) signatures -> for sig in com_con.all_signature_list { ... }        // Messages.\u0004
3) pous     -> for pou in com_con.compiled_pou_list { ... }           // Messages.\u0003
4) summarized -> for lib in summarized_lib_errors.keys_ordered() {...} // Messages.\u0002 (см. GAP-1)
5) memstats -> при all_ok: ...                                        // Messages.\u0001
```

Перед `add_message` прогнать `deduper.insert_first_wins(&key)`; при `false` — пропустить.
Вместо `ldc.i4.m1`-перегрузки — маска `u32::MAX`. Списки (2)/(3) подавать в том же порядке, что и
`AllSignatureList`/`CompiledPOUList` (порядок `m_alCompiledPOUs`, см. doc 17) — тогда результат побайтно
совпадёт.

---

## 4. Артефакты

- Таблица: `tables\message_aggregation.csv` (17 строк).
- Этот документ: `docs\19_MESSAGE_AGGREGATION.md`.

## 5. Пробелы (сводно)

- GAP-1: порядок `LDictionary.Keys` не подтверждён (нужен IL `_3S.CoDeSys.Utilities.LDictionary`).
- GAP-2: ключ дедупа без `ProjectHandle`.
- GAP-3: дедуп текста ordinal (регистрозависим).
- GAP-4: `LList`/`LDictionary` не декомпилированы, семантика Add/ToArray выведена косвенно.
- GAP-5: порядок `AllSignatureList`/`CompiledPOUList` наследуется из `CompileContext`; подтверждён косвенно
  (doc 17), отдельного IL-доказательства порядка этих двух списков здесь не снято.
