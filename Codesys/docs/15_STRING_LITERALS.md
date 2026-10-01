# 15. Строковые литералы CODESYS ST — полная грамматика (Parser35220)

Область: ST-лексер/парсер CODESYS 3.5.20 (plugin `Parser35220.plugin` 3.5.22.10,
`Compiler35220.plugin`, `LanguageModelManager.plugin`). Все ссылки — на файлы в `C:\Codesys`.
Всё ниже подтверждено двумя способами: (1) чтением декомпилированного IL/C# и
(2) **прогоном реального сканера** `Parser35220.plugin.dll` через рефлексию
(полные дампы токенов и unescape приведены в тексте).

Артефакты задачи: `grammar\STRING_LITERALS.ebnf`, `tables\string_escapes.csv`, этот файл.

---

## 0. TL;DR для Rust-порта

- Одинарные кавычки `'…'` → `TokenType.SingleByteString = 17`.
- Двойные кавычки `"…"` → `TokenType.DoubleByteString = 9`.
- `__XSTRING#"…"` → `TokenType.XByteString = 22`. **Только двойная кавычка.**
- `UTF8#'…'` и `UCHAR#'…'` → `TokenType.SingleByteString = 17` (префикс разбирается отдельно).
- `STRING#…`, `WSTRING#…`, `XSTRING#…` **НЕ являются литералами** в Parser35220:
  сканер выдаёт `Error(20)` для первого токена (см. п.6). Единственный рабочий
  X-префикс — `__XSTRING#`.
- Escape `$…`; **сдвоение кавычек `''` НЕ поддерживается** (`'it''s'` = два литерала).
- Многострочность: CR/LF внутри литерала **разрешены**.
- `$hh` (2 hex) — 1-байтные строки, `$hhhh` (4 hex) — 2-байтные, `$U` + ровно 8 hex — Unicode.

---

## 1. Токены

`decompiled\Compiler\_3S\CoDeSys\Core\LanguageModel\TokenType.cs`:

| # | Имя | Когда возникает |
|---|-----|-----------------|
| 9 | `DoubleByteString` | `"…"` (и `WSTRING#` ошибочно оставляет следующий токен этим) |
| 17 | `SingleByteString` | `'…'`, `UTF8#'…'`, `UCHAR#'…'` |
| 20 | `Error` | незакрытая строка, битый escape, **`STRING#`/`WSTRING#`**, `XSTRING#…#` без `__` |
| 22 | `XByteString` | `__XSTRING#"…"` |

Типы 16 (`String`) / 17 (`WString`) здесь — это `TypeClass`, не `TokenType` (см. п.8).

---

## 2. Точка входа и ветка кавычек

`InternalScanner.GetNextInternal` (`Parser35220.plugin\...\Scanner\InternalScanner.cs:3488`),
ветка символов `"` и `'` (`:3545-3556`):

```csharp
case '"':
case '\'':
    if (!this.ValidateStringToken(out flag))      // false == успех
        empty.Type = flag ? 9 : 17;               // DoubleByteString | SingleByteString
    else
        empty.Type = 20;                          // Error
```

`ValidateStringToken` (`:3298`) — ядро:

```csharp
bDoubleByte = (input[off] == 34 /* '"' */);
while (true) {
    c = _input[SourceOffset];
    if (c == '\0')      { result = true; break; }        // незакрытая -> Error
    if (c == '\n' || c == '\r') { EndOfLine(); continue; }  // переносы ВНУТРИ строки
    if (c == '$')       { if (!ValidateEscapeSequence(bDoubleByte)) result = true; continue; }
    if (c == '\'' )     { SourceOffset++; if (!bDoubleByte) break; continue; }
    if (c == '"' )      { SourceOffset++; if ( bDoubleByte) break; continue; }
    SourceOffset++;
}
```

Следствия:

1. Терминатор — **первая неэкранированная кавычка того же вида**, что и открывающая.
2. Противоположная кавычка — обычный символ (`"a'b"`, `'a"b'` валидны).
3. `\r`/`\n` внутри литерала — валидны (строка многострочная уже на уровне сканера;
   `EndOfLine()` учитывает `\r\n` и инкрементит `_nSourceLine`).
4. `\0` до закрывающей кавычки → `result = true` → `TokenType.Error (20)`.
5. **Сдвоения кавычек нет.** `''` не является escape'ом: `'it''s'` → `'it'` + `'s'`.

### Эмпирика (живой сканер)

```
'abc'          -> type=17 len=5  text=['abc']
"abc"          -> type=9  len=5  text=["abc"]
'it''s'        -> type=17 text=['it']   ; type=17 text=['s']
'a$'b'         -> type=17 text=['a$'b'] ; (один токен, escape работает)
'a\r\nb'       -> type=17 text=['a\r\nb'] ; (один токен, две физические строки)
'abc           -> type=20 text=['abc]   ; (нет закрывающей)
'' / ""        -> type=17 / type=9, пустые
```

---

## 3. Escape-последовательности `$`

Обработка — `UnescapeString_Insecure` (`:1516`), валидация — `ValidateEscapeSequence`
(`:3373`), `ValidateLocalCodepoint` (`:3439`), `ValidateUnicodeCodepoint` (`:3455`).

| Последовательность | Результат | Где допустимо | Валидация |
|---|---|---|---|
| `$$` | `$` (U+0024) | везде | 1 символ |
| `$'` | `'` | везде | 1 символ |
| `$"` | `"` | везде | 1 символ |
| `$L` / `$l` | LF (U+000A) | везде | 1 символ |
| `$N` / `$n` | LF (U+000A) | везде | 1 символ |
| `$R` / `$r` | CR (U+000D) | везде | 1 символ |
| `$T` / `$t` | TAB (U+0009) | везде | 1 символ |
| `$P` / `$p` | FF (U+000C) | везде | 1 символ |
| `$hh` — 2 hex | кодовая точка | 1-байтные (`'…'`, `UTF8#`, `UCHAR#`, декод `__XSTRING#`) | ровно 2 hex |
| `$hhhh` — 4 hex | кодовая точка | 2-байтные (`"…"`, валидация `__XSTRING#`) | ровно 4 hex |
| `$Uxxxxxxxx` — 8 hex | UTF-32 codepoint | везде | ровно 8 hex |
| `$` + буква не из {L,N,R,T,P} | **ошибка** | — | `ValidateLocalCodepoint` провалится |
| `$` в конце строки | **ошибка** | — | escape-символ = `\0` |
| `$U` с ≠8 hex | **ошибка** | — | `ValidateUnicodeCodepoint` |

Декодирование локальной кодовой точки (`EscapeLocalEncodingCodepoint`, `:1610`):

```csharp
int num = bDoubleByte ? 4 : 2;
int v = int.Parse(hex, NumberStyles.HexNumber);
if (v > 127 && v < 256) target.Append(Encoding.GetEncoding(1252).GetString(new[]{(byte)v}));
else                    target.Append((char)v);
```

- Т.е. значения `0x80..0xFF` мапятся через **Windows-1252**, например `$80` → U+20AC (€),
  `$C4` → U+00C4 (Ä), `$FF` → U+00FF (ÿ).
- `$U` (`:1629`): `BitConverter.GetBytes(uint.Parse(s, HexNumber))` → `Encoding.UTF32.GetString`,
  поэтому результат может быть surrogate pair (2 UTF-16 code units).
- 9-я hex-цифра после `$Uxxxxxxxx` не поглощается: `$U000000421` → 'B' + '1'.

### Эмпирика (реальные getter'ы сканера)

```
'a$$b'            -> [a$b]        (97,36,98)
'a$Nb' / '$L'     -> (97,10,98)   LF
'a$Rb'            -> (97,13,98)   CR
'a$Tb'            -> (97,9,98)    TAB
'a$Pb'            -> (97,12,98)   FF
'a$41b'           -> [aAb]        0x41='A'
'a$80b'           -> (97,8364,98) 0x80 -> cp1252 -> U+20AC
'a$C4b'           -> (97,196,98)  cp1252/latin1
'a$U00000042b'    -> [aBb]
'a$U000020ACb'    -> [a€b]        (8364)
'a$U0041b'        -> type=20      ERROR (нужно 8 hex)
'a$zzb'           -> type=20      ERROR
'a$Mb'            -> type=20      ERROR
'ab$'             -> type=20      ERROR
'a$4b'            -> [aKb]        0x4B = 'K'
"a$0041b"         -> [aAb]        2-байтная: 4 hex = U+0041
"a$41b"           -> type=20      ERROR (2-байтной нужно 4 hex)
```

> Тонкость: в **однобайтной** строке `$0041` — это `$00` (NUL) + литералы `4`,`1`
> (2 hex за раз); в **двухбайтной** `$0041` — U+0041.

Машиночитаемая таблица: `tables\string_escapes.csv`.

---

## 4. `UTF8#` и `UCHAR#` (ScanUnicodeLiteral)

`ScanUnicodeLiteral` (`:2624`) вызывается первым в `ScanTypedLiteral`
(до lookup в OperatorTable):

```csharp
if ((_buffer.IsEqual("UTF8", true) || _buffer.IsEqual("UCHAR", true)) && _input[SourceOffset] == '\'') {
    bError = ValidateStringToken(out flag);
    if (!bError && !flag) tempToken.Type = 17;      // SingleByteString
    return true;
}
```

- Регистронезависимо; обязательна **одинарная** кавычка.
- Токен единый: `UTF8#'…'` / `UCHAR#'…'` целиком, `Type = 17`.
- `GetSingleByteString(token, out StringEncoding, out bool bIsUChar)` (`:1401`)
  распознаёт префикс **по тексту токена**:
  - `UTF8#` → срезается, `StringEncoding = UTF8 (1)`, unescape (2 hex);
  - `UCHAR#` → срезается, unescape; если результат длины 1 → `bIsUChar = true`.

### Эмпирика (prefix-aware getter)

```
plain  'A'          -> val=[A]  encoding=Default isUChar=False
UTF8#'a$41b'        -> val=[aAb] encoding=UTF8   isUChar=False
UCHAR#'A'           -> val=[A]  encoding=Default isUChar=True
UCHAR#'AB'          -> val=[CHAR#'AB] isUChar=False   <-- дефект, см. п.10
```

Одиночный getter `GetSingleByteString(token)` префикс **не** срезает
(`UTF8#'a$41b'` → `TF8#'aAb`). Парсер всегда использует 3-аргументную версию
через `FactoryExtension.CreateSingleByteStringOrUCharLiteralExpression`
(`Utilities\FactoryExtension.cs:184`).

---

## 5. Escape для локальной кодовой точки внутри `__XSTRING#`

Здесь есть **несогласованность** (реальный дефект движка):

- валидация идёт по `bDoubleByte = ('"' == первый символ)` = **true**
  (`ValidateStringToken`), т.е. локальный escape требует **4 hex**;
- декодирование (`GetXByteString` → `UnescapeString(..., bDoubleByte:false)`, `:1482`)
  идёт по **1-байтному** правилу — **2 hex**.

Практически: `__XSTRING#"a$41b"` → `Error(20)` (валидатор хочет 4 hex);
`__XSTRING#"a$0041b"` валиден, но декодируется как `$00` (NUL) + `4` `1`,
а не как U+0041. Проверено:

```
__XSTRING#"a$Rb"     -> type=22, value=(97,13,98)      OK
__XSTRING#'a'        -> type=20 (одинарная кавычка запрещена)
__XSTRING#"a$41b"    -> type=20 (нужно 4 hex на валидации)
```

Обычные символы (`$R`, `$N`, `$$`, `$'`, `$"`) и `$U` в `__XSTRING#` работают нормально.

---

## 6. Типизированные строковые литералы с `#` — точный синтаксис и где распознаётся

### 6.1 Как вообще ищется `#`

`ScanIdentifierOrOperator` (`:2374`): после чтения идентификатора

```csharp
if (this._input[this.SourceOffset] == '#') {   // :2395
    SourceOffset++;
    this.ScanTypedTimeLiteral();               // DATE/DT/TOD/... aliases
    this.ScanTypedLiteral(ref tempToken, ref c, ref flag);
    return;
}
```

### 6.2 Таблица операторов-префиксов и что делает `ScanTypedLiteral`

`OperatorTable.AddDataTypeNames` (`Scanner\OperatorTable.cs:599-657`).
`ScanTypedLiteral` (`:2640`), IL-switch (читается буквально):

| Оператор | Код | Случай | Тип токена |
|---|---|---|---|
| `BIT` | 10 | 10 | `Boolean(1)` (0/1) |
| `BOOL` | 11 | 11 | `Boolean(1)` |
| `BYTE`,`WORD`,`DWORD`,`LWORD`,`SINT`,`INT`,`DINT`,`LINT`,`USINT`,`UINT`,`UDINT`,`ULINT` | 12–23 | 12–23 | `Integer(14)` |
| `REAL`,`LREAL` | 24,25 | 24,25 | `Real(16)` |
| `STRING` | 26 | 26 | **нет (остаётся Error 20)** |
| `WSTRING` | 27 | 27 | **нет (остаётся Error 20)** |
| `TIME`,`LTIME` | 28,29 | 28,29 | `Duration(10)` / `LDuration(11)` |
| `DATE` | 30 | 30 | `Date(5)` |
| `DATE_AND_TIME`/`DT` | 31 | 31 | `DateAndTime(6)` |
| `TIME_OF_DAY`/`TOD` | 32 | 32 | `TimeOfDay(18)` |
| `__XSTRING` | 243 | default→243 | `XByteString(22)`, **только если след. `"`** |
| `LDATE` | 274 | 274 | `LDate(23)` |
| `LDATE_AND_TIME`/`LDT` | 275 | 275 | `LDateAndTime(25)` |
| `LTIME_OF_DAY`/`LTOD` | 276 | 276 | `LTimeOfDay(24)` |
| `UTF8`,`UCHAR` | — | `ScanUnicodeLiteral` до switch | `SingleByteString(17)` |

Псевдо-код XSTRING-ветки:

```csharp
case 243:                                   // __XSTRING
    if (_input[SourceOffset] == '"') {
        bError = ValidateStringToken(out _);
        if (!bError) tempToken.Type = 22;   // XByteString
        return;
    }
    break;                                  // иначе ничего -> Error
```

### 6.3 Главный вывод: `STRING#` / `WSTRING#` / `XSTRING#` НЕ работают

`STRING`(26) и `WSTRING`(27) в `ScanTypedLiteral` **не выставляют тип** — попадают
в группу `case 1..9, 26, 27: break;` (`:2656-2667`, подтверждено IL).
`tempToken` инициализируется как `Error(20)` в `GetNextInternal:3491`, значит
токен остаётся `Error`. Другое имя (`XSTRING` без `__`) вообще не в OperatorTable →
ветка `case Operator.None → Identifier(13)` (`:2649-2654`), и `#` уходит отдельным оператором.

**Живой сканер:**

```
STRING#'abc'      -> type=20 text=[STRING#]   ; type=17 text=['abc']
WSTRING#"abc"     -> type=20 text=[WSTRING#]  ; type=9  text=["abc"]
XSTRING#"abc"     -> type=13 text=[XSTRING]   ; type=15 text=[#] ; type=9 text=["abc"]
__XSTRING#"abc"   -> type=22 text=[__XSTRING#"abc"]
__XSTRING#'abc'   -> type=20 text=[__XSTRING#'abc']   (только двойная кавычка)
```

Итого:

- **XSTRING-литерал = `__XSTRING#` + двойная кавычка** (исходное имя оператора —
  `__XSTRING`; `GetXByteString` срезает именно `GetTextOfOperator(243) + "#"` =
  `__XSTRING#`, `InternalScanner.cs:1473`).
- Ранее в `docs\11_X_TYPES_USAGE.md:34` и `docs\12_X_TYPES_GAPS.md:378` это уже
  зафиксировано как `__XSTRING#`.
- **`STRING#` и `WSTRING#` — не грамматика**, а диагностический мусор в 35220/35210
  (в `Parser35210` switch тоже нет case для STRING/WSTRING). Если Rust-порт должен
  быть 100% идентичен — эти формы нужно воспроизводить как `Error(20)` + отдельный
  строковый литерал, а не как типизированный литерал.

> Проверка выполнена вызовом `Parser35220.plugin.dll` (реальный `InternalScanner`),
> не только по декомпиляции.

---

## 7. Парсер: как литералу присваивается тип

`Utilities\FactoryExtension.cs:43` `CreateLiteralExpression(factory, context, token)`
диспетчеризует по `token.Type`:

| token.Type | Метод сканера | `TypeClass` (ConstantType) | Код |
|---|---|---|---|
| 9 `DoubleByteString` | `GetDoubleByteString(token)` | `WString` | 17 |
| 17 `SingleByteString` | `GetSingleByteString(token,out enc,out uchar)` | `String` = 16, либо `UDInt` = 12 (UChar) | 16 / 12 |
| 22 `XByteString` | `GetXByteString(token)` | `XString` | 42 |

`TypeClass` (`decompiled\Compiler\_3S\CoDeSys\Core\LanguageModel\TypeClass.cs`):
`String=16`, `WString=17`, `XString=42`, `UDInt=12`.

Создание выражений:

- `CreateSingleByteStringOrUCharLiteralExpression` (`:184`) → 3-арг. getter;
  - `CreateSingleByteStringLiteralExpression` (`:198`): `tc=16`,
    `factory.CreateLiteralExpression(str, 16, token, stringEncoding)`;
  - `CreateUCharLiteralExpression` (`:216`): `ulong = char.ConvertToUtf32(str,0)`,
    `tc=12` (`UDInt`), `CreateLiteralExpression(ulVal, 12, token)`.
- `CreateLiteralExpression` для XString: `typeClass = 42` (`:143`).

«FileOffset»/позиция: результат — `_ILiteralExpression`, базовый объект получает
`token` (`LiteralExpression.cs:145`, `StringLiteralExpression.cs`), из которого
доступны `SourceOffset`/`SourceLine`/`SourceColumn`/`PositionOffset`/`Position`
(структура `Token.cs`), т.е. позиция литерала = позиция первого токена.

Для XSTRING-типа (не литерала, а типа) — `Declaration\TypeParser.cs:328-330` →
`ParseXStringType()` (`:480`), `LMItemFactory.CreateXStringtype()`, длина опционально
в `__XSTRING(n)`. `STRING`/`WSTRING` как типы: `TypeParser.cs:254-260` →
`ParseStringType`/`ParseWStringType` (`:510`).

---

## 8. Кодировки, StringEncoding, предупреждение 555

- `StringEncoding`: `Default=0`, `UTF8=1`
  (`decompiled\Compiler\_3S\CoDeSys\Core\LanguageModel\StringEncoding.cs`).
- `UTF8#'…'` → `StringEncoding.UTF8`. Остальные однобайтные — `Default`.
- Предупреждение 555 `Wrn_NonAsciiStringLiteral`
  (`FactoryExtension.cs:202`, `Helper.CheckEncoding` `Utilities\Helper.cs:200`):
  для `Default`-строки при выключенной опции проекта «UTF-8 Encoding for STRING»
  (`CompileOptions.UTF8Encoding`) и наличии не-1252/непредставимых символов.
- Для 3.5.18+ (целевой компилятор < 3.5.18, `_bReportSP18Feature`,
  `ParserContext.cs:26,126`):
  `UTF8#` → unsupported-feature `CompilerFeature_UTF8_Strings`;
  `UCHAR#` → `CompilerFeature_UCHAR_Literals`.
- Runtime-кодирование (`DefaultStringEncodingService.cs`):
  - `String` → `Encoding.Default` + 1 нулевой байт-терминатор (`:148`);
  - `WString` → UTF-16 (`Encoding.Unicode`/`BigEndianUnicode`) + 2 нулевых байта (`:154`);
  - при включённой `UTF8Encoding` STRING трактуется как UTF-8 (`:48`).
- Длина `XSTRING(n)`: `ParseXStringType` читает выражение `(n)` и кладёт в
  `ixstringType.Length` (`TypeParser.cs:495-501`).

### Что происходит с XSTRING при отключённом UNICODE

`TypeCompiler.\u0001(_IXStringType)` (`Compiler35220\...\Phase1_Typification\TypeCompiler.cs:733`):

```csharp
if (this.Comcon.IsDefined("NO_UNICODE_SUPPORT")) {
    _IStringType t = ...; t.Length = xstring.Length; GeneratedType = t;   // -> STRING
} else {
    _IWStringType t = ...; t.Length = xstring.Length; GeneratedType = t;  // -> WSTRING
}
```

То есть `__XSTRING` (и `__XSTRING#`-литерал) резолвится в `STRING` при определённом
`NO_UNICODE_SUPPORT` (define) и в `WSTRING` иначе. К `STRING#`/`WSTRING#` это не
относится — они до типизации не доживают (см. п.6).

---

## 9. Многострочность и `MultiStringScanner` / `LineScanner`

- Сканер принимает CR/LF внутри литерала (§2) — это «многострочные» литералы
  на уровне полного текста.
- Редакторский `LineScanner` (`decompiled\LanguageManager...\CommonCompilerData\LineScanner.cs`)
  сканирует построчно и переносит незакрытый литерал между строками:
  - `CheckForIncompleteTokens` (`:178`) по чётности числа `'`/`"`, учитывая escape
    `$` (`:303`): `flag = ((!flag || stLine[i] != '$') && stLine[i] == '$')` —
    так `$'`/`$"` не закрывают строку, а `$$` снимает escape;
  - `GetPrefix` (`:326`) добавляет `'`/`"`/`(*`/`{`/`` ` `` к началу строки,
    `GetSuffix` (`:388`) — закрывающие.
- `MultiStringScanner` (`Scanner\MultiStringScanner.cs`) — сканер по списку готовых
  строк (`InitializeMulti`), `MultiStringToken.StringIndex`; на `End(21)` переходит к
  следующей строке. Это не про грамматику ST, а про подстановки/редактор.

---

## 10. Найденные дефекты/особенности (важно для 1:1)

1. **`STRING#`/`WSTRING#`** не выставляют тип → `Error(20)` (§6.3).
2. **`__XSTRING#`**: валидация double-byte (4 hex), декодирование single-byte (2 hex) (§5).
3. **`UCHAR#'AB'`** (длина ≠ 1): 3-арг. getter не возвращает значение и падает в
   1-арг. `GetSingleByteString(token)` **на исходном токене с префиксом** →
   значение `CHAR#'AB` (мусор). `FactoryExtension.cs:184`, `InternalScanner.cs:1424-1442`.
4. **`XSTRING#` без `__`** — не литерал (`Identifier` + `#` + строка).
5. **`''` не escape** — сдвоение кавычек не поддерживается.
6. `$U` требует ровно 8 hex (C#-подобного `\uXXXX` с 4 hex нет).

---

## 11. Пробелы / что не закрыто

- Точная семантика `wcs`/`cp1252` для диапазона 128–159 зависит от кодовой страницы
  хоста (`Encoding.GetEncoding(1252)` в .NET обычно даёт западноевропейскую таблицу,
  но на некоторых локалях `1252` может быть недоступна/заменена). Для 1:1 нужен
  явный табличный маппинг cp1252, а не зависимость от ОС.
- Формально не проверял **компиляторный** путь (`ConstantFolder`, `TypeChecker`),
  как именно XSTRING-литерал складывается в WSTRING/STRING и как влияет
  `StringEncoding.UTF8` на байтовое представление `__XSTRING#`-литерала
  (декод идёт single-byte, но валидация double-byte — см. §5). Рекомендуется
  runtime-эксперимент компиляции `s : WSTRING := __XSTRING#"Ä";`.
- `docs\01_LEXER_PARSER.md`, `grammar\ST_GRAMMAR.ebnf` (строки 70–77) содержат
  упрощённую форму (`x6ByteString = ("__XSTRING"|"__XWSTRING")`, `escape = "$", (... "...")`).
  Здесь эти две неточности исправлены: `__XWSTRING` в OperatorTable **нет**
  (только `__XSTRING`=243), а полный escape-набор — в `tables\string_escapes.csv`.
- `STRING#`/`WSTRING#` могут быть поддержаны в других версиях парсера (не 35210/35220);
  в исследуемых сборках — нет.
