# 18. Базовый порядок объектов компиляции: `m_alCompiledPOUs` / `GetAllCompiledPOUsEx()`

Закрывает пункт №1 «Что осталось неясным» из `docs\17_COMPILER_DETERMINISM.md` — прослежена
полная траектория наполнения `CompileContext.m_alCompiledPOUs`.

Все ссылки — на файлы `C:\Codesys`; `file:line` — по декомпилу, где отмечено — по IL (`dnlib`).
Обфусцированные пути: папка-«односимвольное» пространство имён пишется как `<0xNN>\-.M.cs`
(как в док.17) либо буквально `\-\-.M.cs` (dnSpy кладёт большинство обфусцированных типов в
папку с именем `-`); ниже дублируется `namespace`-код.
Артефакты: `tables\compiled_pous_order.csv` (26 строк).

---

## 0. TL;DR

1. `m_alCompiledPOUs` — **append-only** `LList<_ICompiledPOU>`
   (`CompileContext.cs:4895`), наполняется **только** вставкой в конец
   (`:2097` в `AddCompiledPOUSimple`, `:2268` в `_AddCompiledPOU`). **Никакой сортировки
   самой коллекции нет** (`.Sort()`/`OrderBy` по `m_alCompiledPOUs` отсутствуют).
2. `GetAllCompiledPOUsEx()` (`:4614-4617`) — это **копия** списка:
   `new List<ICompiledPOU4>(m_alCompiledPOUs)`. То есть «порядок в `GetAllCompiledPOUsEx()`»
   **тождественно равен порядку вставки** в `m_alCompiledPOUs`.
3. Базовый порядок задаётся **порядком обхода `PreCompileContext._AllSignatures`** в
   `CompiledSignatureCreator` при создании compiled-контекста
   (`<0x81>\-.6.cs:203-213` → `\-\-.108.cs:83-102`).
4. `_AllSignatures` = `m_alSignatures` **++** `m_alGVLSignatures`
   (`PreCompileContext.cs:2741-2755`). `m_alSignatures` наполняется **в порядке построения
   Language Model** (`\-\-.131.cs:258` `foreach (ILMPOU in languageModel.Pous)` …), а
   `LanguageModel.Pous` — это append-only `LList` (`LanguageModel.cs:71-83`). Итог:
   **порядок объектов в проекте / порядок построения LM**, **не** GUID, **не** индекс, **не**
   алфавит.
5. Под-POU (методы/действия Function Block) добавляются в порядке
   `Signature.GetSubSignatures()` = `CaseInsensitiveHashtable.Values`
   (`SubSignatureTable.cs:195-200`) — это **порядок хеш-корзин**, а не порядок объявления и не
   алфавит. Единственный «неочевидный» источник порядка (см. §5, пробел P1).

---

## 1. Полная цепочка наполнения `m_alCompiledPOUs`

### Стадия A. Построение Language Model (порядок = порядок объектов проекта)

| # | Что | Где |
|---|---|---|
| A1 | LM собирается по объектам проекта; каждый POU добавляется вызовом `languageModel.AddPou(...)` | `decompiled\LanguageModelUtilities.plugin\...\StructuredLanguageModel\AbstractPOU.cs:28-56` (add — :53) |
| A2 | GVL: `languageModel.AddGlobalVariableList(...)` | `...\StructuredLanguageModel\GVL.cs:58-104` (add — :104) |
| A3 | DUT: `languageModel.AddDataType(...)` | `decompiled\LanguageModelManager.plugin\...\LanguageModel.cs:112-115` |
| A4 | Три **append-only** `LList`: `_lmpoulist`, `_lmgvllist`, `_lmdutlist`; `Pous`/`GlobalVariableLists`/`DataTypes` возвращают `ToArray()` | `LanguageModel.cs:18-21`, `:71-83`, `:87-99`, `:103-115`, `:133-143` |
| A5 | Точка входа построения для одного объекта | `decompiled\LanguageModelUtilities.plugin\...\LanguageModelBuilderHelper.cs:177-180` `AddToLanguageModel(IHasVarDeclaration pou)` → `pou.AddToLanguageModel(_lastPosition, _lm)` |

Вывод A: порядок POU в LM = порядок, в котором LM-builder обрабатывает объекты проекта
(для исходного приложения — порядок деклараций/объектов), затем GVL, затем DUT.

### Стадия B. LM → precompile-контекст (появление сигнатур)

`\u001D.\u0004` (класс `\u0004`, namespace `\u001D`) реализует `ILanguageModelHandling2`.
`decompiled\Compiler35220.plugin\-\-.131.cs:223-295`:

```
248  \u0001(\u0002, \u0003.LMApplication, deviceGuid, applicationGuid, ipreCompileContext, \u0005);
...
258  foreach (ILMPOU ilmpou in \u0003.Pous)              // порядок LM!
271      this.\u0001(\u0002, ilmpou, languageModelObject, ipreCompileContext, libraryId);
273  foreach (ILMGlobVarlist ilmglobVarlist in \u0003.GlobalVariableLists)
278      this.\u0001(...);
280  foreach (ILMDataType ilmdataType in \u0003.DataTypes)
289      this.\u0001(...);
```

Обработчик POU (`\-\-.131.cs:587-635`):

```
615  _ISignature isignature = ...\u0001(\u0002, \u0003, \u0005, \u0006, ilmentity, u);   // (668) создать сигнатуру
616  _ICompiledPOU icompiledPOU = ...\u0001(\u0002, \u0003, \u0005, \u0006, isignature);  // (730) green-POU
```

- сигнатура → `PreCompileContext.AddSignature(isignature, true)` — `\-\-.131.cs:699`;
- green-POU → `_IPreCompileContext.AddCompiledPOU(icompiledPOU, true)` — `\-\-.131.cs:770`
  (это **не** `CompileContext`, а green-хранилище `PreCompileContext.m_htCompiledPOUs` —
  словарь по `ObjectGuid`, `PreCompileContext.cs:2265-2311`; порядок там не важен, POUs берутся
  по `GetPOU(ObjectGuid)`).

Роутинг сигнатур — `PreCompileContext._AddSignature` (`PreCompileContext.cs:1783-1890`):

```
1827  if (sign.POUType == Operator.Method || sign.POUType == Operator.Action) {
1831      this.SubSignatureTable.InsertSubSignature(sign);      // НЕ в m_alSignatures
1841  } else {
1844      if (PreCompileContext.IsGlobalSign(sign)) {           // VarGlobal|VarAccess|VarConfig|Enum
1851/1855     this.m_alGVLSignatures.Add(sign);
1859      } else
1859          this.m_alSignatures.Add(sign);                    // append-only
```

`_AllSignatures` (`PreCompileContext.cs:2741-2755`), IL token `06001052`:

```il
IL_002E: ldloc.0
IL_002F: ... ldfld m_alSignatures
IL_0035: callvirt LList::AddRange
IL_003A: ldloc.0
IL_003B: ... ldfld m_alGVLSignatures
IL_0041: callvirt LList::AddRange
```

⇒ `_AllSignatures` = `m_alSignatures` (POU/DUT/не-глобальные, в порядке LM) ++
`m_alGVLSignatures` (GVL/enum, в порядке LM). Под-POU (methods/actions) в `_AllSignatures`
отсутствуют — добираются позже через родителя (см. §4/§5).

> Отдельная ветка загрузки **сериализованного green-LM** (precompiled libraries):
> `decompiled\Compiler35220.plugin\-\-.38.cs:291-298` — массивы `_ISignature2[]` / `_ICompiledPOU2[]`
> читаются из `BinaryReader` и по порядку отдаются в `AddGreenSignature`/`AddGreenCompiledPOU`.
> Порядок тут — порядок байтов в файле (сохранённый порядок LM), т.е. тот же принцип.

### Стадия C. precompile → compiled-контекст (`ComconNew`)

Современный создатель compiled-контекста — `\u0081.\u0006`
(`decompiled\Compiler35220.plugin\<0x81>\-.6.cs`, namespace `\u0081`; создаётся в
`\-\-.404.cs:163-170`, вызывается на `\-\-.404.cs:309`).

```
93  public void \u0001(bool \u0002, out IList<_ICompilerMessage> \u0003)
95      this.ComconNew = this.Precomp.CreateEmptyCompiledContext(this.ComconOld, this.ComconParent, \u0002, out \u0003);
96      this.CompiledSignatureCreator = new \u0012.\u000E(this.ComconNew, this.ComconOld, this.Precomp);
97      this.\u0001(\u0002);
```

`\u0001(bool)` (`<0x81>\-.6.cs:124-177`) задаёт порядок стадий:

```
127  this.\u0001(this.Precomp, this.Precomp.ApplicationGuid == Guid.Empty || \u0002); // (203) application signatures
132  this.\u0001(APEnvironmentFacade.Instance.LanguageModelMgr.Pool, false);         // pool
135  source = this.ComconNew._LibraryTable.GetVisibleLibraries(this.Precomp).ToArray();
136  foreach (ipreCompileContext in source...)  { ...; this.\u0001(ipreCompileContext, u); } // libraries
144  this.\u0001(LanguageModelMgr._SystemContext, false);                            // system
151  codegenerator = ...; this.\u0001(functionsToLinkAlways, (CompiledPOUFlags)0);   // FunctionsToLinkAlways
171  this.\u0001(llist2.ToArray(), CompiledPOUFlags.TopLevel);                       // executionpoint logging
```

`\u0001(_IPreCompileContext, bool bAll)` (`<0x81>\-.6.cs:203-213`):

```
206  foreach (_ISignature u2 in \u0002._AllSignatures)          // базовый порядок!
208      if (\u0081.\u0006.\u0001(u2, \u0003, u))               // TopLevel || bAll || (linkAll && global && !superglobal)
210          this.CompiledSignatureCreator.\u0001(u2, \u0002);  // -> \-\-.108.cs:83
```

`\u0012.\u000E.\u0001(sig, precomp)` (`\-\-.108.cs:83-102`):

```
85   _ISignature isignature = this.\u0001(\u0002, \u0003);   // 39-80: CreateCompiledSignature + AddSignature
86   _ICompiledPOU pou = \u0003.GetPOU(\u0002.ObjectGuid);
87-90 if (pou != null) this.ComconNew.AddCompiledPOU(pou.CreateCompiledPOU(), isignature, this.ComconOld);
91   foreach (_ISignature isignature2 in isignature.GetSubSignatures()) {   // hashtable order
95        pou = \u0003.GetPOU(isignature2.ObjectGuid);
97-98    if (pou != null) this.ComconNew.AddCompiledPOU(pou.CreateCompiledPOU(), isignature2, this.ComconOld);
```

### Стадия D. Вставка в `m_alCompiledPOUs`

`CompileContext._AddCompiledPOU` (`CompileContext.cs:2207-2271`), IL token `06000DE5`:

```
IL_00E6: leave IL_0219        ; dedup: GreaterEqualV33102 && m_htCompiledPOUsById.ContainsKey(SignatureId)
                              ;        && cpou.ObjectGuid == Guid.Empty  -> пропустить вставку
...
IL_01EF: ldarg.0
IL_01F0: ldfld  m_alCompiledPOUs
IL_01F5: ldarg.1
IL_01F6: callvirt LList<_ICompiledPOU>::Add(_ICompiledPOU)
IL_01FB: ldarg.0
IL_01FC: ldfld  _compiledPOUsByObjectGuid
...
```

`AddCompiledPOUSimple` (`CompileContext.cs:2091-2100`), IL token `06000DD7`:

```
IL_0011..IL_001D  m_htCompiledPOUsById[cpou.SignatureId] = cpou
IL_0023..IL_002A  m_alCompiledPOUs.Add(cpou)        ; <-- чистый append
IL_002F..IL_003C  _compiledPOUsByObjectGuid[cpou.ObjectGuid] = cpou
```

Оба метода обёрнуты в `lock (m_alCompiledPOUs)`/`Monitor`. Никакой сортировки.

### Стадия E. Выдача

```
4614  public IList<ICompiledPOU4> GetAllCompiledPOUsEx()
4616      return new List<ICompiledPOU4>(this.m_alCompiledPOUs);      // копия, порядок сохранён
```

- `AllPOUs` (`:4621`) — read-only обёртка над тем же списком;
- `GetCompiledPOUsToCompileEx()` (`:4636-4657`) — тот же порядок, **исключая**
  `POUType == Operator.Interface` (`:4644-4652`), которые помечаются `ToCompile=false`;
- `POUsToCompile` (`:4661`) — то же.

### Стадия F. Потребители порядка

| Фаза | Что | Где |
|---|---|---|
| Phase4 | `GetAllCompiledPOUsEx().OrderBy(x => x, \u0084.\u0005)` → `ConcurrentQueue`; компаратор — **NumberOfStatements desc**, `OrderBy` стабильный | `...\CompilerPhases\CompilerPhase4_Typechecker.cs:136` |
| Phase5 | `OrderBy` тем же компаратором **только если** `num > 1`; при `num <= 1` берётся исходный порядок `POUsToCompile` | `...\CompilerPhases\CompilerPhase5_Codegenerator.cs:211-216` |

⇒ базовый порядок из `m_alCompiledPOUs` — это тай-брейкер стабильной сортировки в Phase4
(при равном `NumberOfStatements`), и он же — итоговый порядок в single-thread Phase5.

---

## 2. Что именно задаёт базовый порядок

Базовый порядок = **порядок вставки`_ICompiledPOU`** = порядок обхода
`PreCompileContext._AllSignatures` (по стадиям C17) = для каждого элемента: сам POU, затем его
под-POU.

`_AllSignatures` детерминированно строится из:

1. `m_alSignatures` — не-глобальные подписанные объекты в порядке **построения LM** (Pous → DUTs);
2. затем `m_alGVLSignatures` — GVL/enum в порядке LM;
3. под-POU (methods/actions) — **не** в `_AllSignatures`, а внутри родителя через
   `GetSubSignatures()` (см. §5).

Чего в базовом порядке **НЕТ**:
- сортировки по имени/GUID/индексу POUs — не найдено ни одного `.Sort(`/`OrderBy` над
  `m_alCompiledPOUs`/`m_alSignatures` (проверено по всему `decompiled`);
- `CompileContextSerializer.Sort` (по `Id`) применяется только к **сериализации** контекста
  (`...\Serialization\CompileContextSerializer.cs:24-56`), не к `m_alCompiledPOUs`.

### Стабилен ли порядок?

- **Да, на уровне top-level POU**: это порядок объектов проекта/LM (append-only списки), он
  воспроизводим и не зависит от хешей.
- **Да, между запусками на одной и той же среде .NET Framework** (без
  `UseRandomizedStringHashAlgorithm`): порядок под-POU фиксирован для фиксированного набора имён.
- **Гарантия переносимости отсутствует** для порядка под-POU: он определяется внутренним
  порядком корзин `System.Collections.Hashtable` (см. §5) — это implementation detail .NET,
  а не документированная сортировка. Для 1:1 порта его нужно эмулировать явно.

---

## 3. Как воспроизвести в Rust

```rust
// 1) Собрать LM в порядке объектов проекта (append-only).
//    pou_list: Vec<LmPou>, gvl_list: Vec<LmGvl>, dut_list: Vec<LmDut>
//    Порядок = порядок, в котором builder обходит объекты (декларации/дерево проекта).

// 2) PrecompileContext._AllSignatures = m_alSignatures ++ m_alGVLSignatures.
//    m_alSignatures: не-глобальные (POU, DUT) в порядке LM; Method/Action НЕ сюда.
//    m_alGVLSignatures: VarGlobal/VarAccess/VarConfig/Enum (GVL/enum) в порядке LM.
//    global(sig) = matches!(sig.pou_type, VarGlobal|VarAccess|VarConfig) || sig.is_enum;
fn is_global(sig: &Sig) -> bool { ... }

// 3) compiled POU list (base order) — append-only:
let mut m_al_compiled_pous: Vec<CompiledPou> = Vec::new();
for sig in app_ctx.all_signatures()          // m_alSignatures ++ m_alGVLSignatures
        .filter(|s| insert_filter(s, b_all, link_all)) {
    let compiled_sig = create_compiled_signature(sig);
    if let Some(pou) = app_ctx.get_pou(sig.object_guid) {
        push_compiled(&mut m_al_compiled_pous, pou, compiled_sig);  // parent
    }
    // под-POU: порядок = GetSubSignatures() [Hashtable.Values], НЕ объявление/алфавит
    for sub in compiled_sig.get_sub_signatures_dotnet_hashtable_order() {
        if sub.object_guid != Guid::empty() {
            if let Some(pou) = app_ctx.get_pou(sub.object_guid) {
                push_compiled(&mut m_al_compiled_pous, pou, sub);
            }
        }
    }
}
// далее: Pool top-level, visible libraries (GetVisibleLibraries order, рекурсивно),
// System context, FunctionsToLinkAlways, executionpoint-logging — в этом порядке.

// 4) dedup-правило _AddCompiledPOU (иначе набор отличается):
//    skip если V>=3.3.1.2 && htCompiledPOUsById.contains_key(sig_id) && pou.object_guid == empty

// 5) GetAllCompiledPOUsEx() == клон m_al_compiled_pous (порядок сохранён).

// 6) Phase4: stable sort by number_of_statements DESC (ключ идентичен, компаратор читает NumStmts).
//    Phase5: тот же sort только при многопоточности; single-thread — исходный base order.
```

Практическое правило для 1:1:
- если в проекте нет FB с methods/actions — базовый порядок = **порядок объектов проекта**
  (`m_alSignatures` ++ `m_alGVLSignatures`); под-POU вопрос не возникает;
- если есть — нужно эмулировать **порядок корзин `Hashtable`** для имён под-подписей
  (`StringComparer.OrdinalIgnoreCase` + bucket-chain), иначе порядок методов FB разойдётся.

---

## 4. Ключевые IL-выдержки

`CompileContext::GetAllCompiledPOUsEx` (token `06000E71`):

```il
IL_0000: ldarg.0
IL_0001: ldfld  ...::m_alCompiledPOUs
IL_0006: newobj List`1<ICompiledPOU4>::.ctor(IEnumerable`1<ICompiledPOU4>)
IL_000B: ret
```

`CompileContext::AddCompiledPOUSimple` (token `06000DD7`) — `m_alCompiledPOUs.Add` в IL_002A.

`CompileContext::_AddCompiledPOU` (token `06000DE5`) — `m_alCompiledPOUs.Add` в IL_01F6,
ранний `leave IL_0219` (пропуск вставки) в IL_00E6.

`PreCompileContext::get__AllSignatures` (token `06001052`) — `m_alSignatures.AddRange` +
`m_alGVLSignatures.AddRange` (см. §1 стадия B).

`SubSignatureTable::GetSubSignatures` (token `0600299B`):

```il
IL_0000: ldarg.0
IL_0001: ldfld  ...::m_htSignatures               ; CaseInsensitiveHashtable
IL_0011: ...
IL_0017: callvirt System.Collections.Hashtable::get_Values()
IL_001E: callvirt System.Collections.ICollection::CopyTo(System.Array, System.Int32)
```

---

## 5. Пробелы / ограничения

- **P1 (важный).** Порядок под-POU (methods/actions) берётся из
  `Signature.GetSubSignatures()` → `SubSignatureTable.GetSubSignatures()`
  (`SubSignatureTable.cs:195-200`) = `CaseInsensitiveHashtable.Values`
  (`CaseInsensitiveHashtable.cs:12-22`, `StringComparer.OrdinalIgnoreCase`).
  Это **порядок корзин Hashtable**, implementation-defined. Полной пошаговой эмуляции
  `System.Collections.Hashtable` (capacity growth, bucket-chain, `StringComparer.OrdinalIgnoreCase`
  hash) не делалось — нужна отдельная проверка на проекте с FB+method, чтобы подтвердить, что
  порядок методов в `m_alCompiledPOUs` совпадает с hash-order. Также требует `--config`
  проверки, что `UseRandomizedStringHashAlgorithm` выключен в CODESYS Desktop.
- **P2.** Источник порядка `LanguageModel.Pous` доведён до
  `LanguageModelBuilderHelper.AddToLanguageModel` (`:177-180`) →
  `StructuredLanguageModel.AbstractPOU.AddToLanguageModel` (`:28-56`). Драйвер, который вызывает
  его по каждому объекту проекта (и, соответственно, порядок объектов), в этой задаче не
  трассирован до конца — предполагается порядок объектов/деклараций проекта.
- **P3.** Инкрементальный путь `PreCompileContext.RemoveTimeStampOnlyObjects`
  (`PreCompileContext.cs:2901-2942`) пере-добавляет оставшиеся сигнатуры через
  `AddSignature(sign, false)` в порядке `_AllSignatures` — это **меняет** `PrecompileId`, но
  сохраняет группировку (не-глобальные, затем глобальные) и может удалять TimeStampOnly-объекты.
  Влияние на итоговый `m_alCompiledPOUs` при частичной перекомпиляции не проверялось live.
- **P4.** Пул/библиотеки/система/`FunctionsToLinkAlways` добавляются **после** объектов
  приложения (стадия C). Их точный состав/порядок зависит от
  `_LibraryTable.GetVisibleLibraries(...).ToArray()` и `codegenerator.FunctionsToLinkAlways`;
  внутренний порядок этих источников здесь не разбирался (вне «базового порядка приложения»).
- **P5.** `LList<T>` (`_3S.CoDeSys.Utilities`) — внешний тип, в `decompiled` отсутствует;
  append-семантика установлена по API (`Add`/`AddRange`/`RemoveAt`/`Count`/индексатор), но
  реализация не читалась.

---

## 6. Ссылки

- `tables\compiled_pous_order.csv` — 26 строк, пошаговая карта порядка.
- `docs\17_COMPILER_DETERMINISM.md` — Phase4/5, компаратор `\u0084.\u0005`, cp1252, Phase6.
- `docs\14_COMPILER_PHASES.md` — фазы C1..C6.
- Код:
  `decompiled\LanguageModelManager.plugin\...\CompileContext.cs`,
  `...\PreCompileContext.cs`, `...\LanguageModel.cs`,
  `...\Signature\SubSignatureTable.cs`, `...\Signature\Signature.cs`,
  `...\Signature\CompiledSignatureCreator.cs`, `...\CaseInsensitiveHashtable.cs`,
  `...\Legacy\CompileContextCreator.cs`;
  `decompiled\Compiler35220.plugin\<0x81>\-.6.cs` (CompileContextCreator),
  `\-\-.108.cs` (`\u0012.\u000E` CompiledSignatureCreator),
  `\-\-.131.cs` (`\u001D.\u0004` LM builder), `\-\-.38.cs` (green-LM deserializer),
  `\-\-.404.cs` (compile driver);
  `decompiled\LanguageModelUtilities.plugin\...\StructuredLanguageModel\*.cs`,
  `...\LanguageModelBuilderHelper.cs`.
