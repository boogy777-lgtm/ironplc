# 03. Каталог сообщений компилятора/парсера ST (CODESYS 3.5.22.10)

Машинно-читаемые таблицы: `C:\Codesys\tables\errors\`. Сырые дампы и DLL: `C:\Codesys\resources\`.
Метод: `ResourceReader` по манифест-ресурсам satellite-сборок; ID — enum `MessageId`/`InternalErrorIds` из `Common\Compiler.dll` (ReflectionOnlyLoad + `FieldInfo.GetRawConstantValue`); запись UTF-8 без BOM.

## Сводка

| Показатель | Значение |
|---|---|
| Записей enum `MessageId` (вкл. алиасы) | 507 |
| Уникальных числовых значений `MessageId` | 507 (алиасов нет) |
| Записей enum `InternalErrorIds` | 7 (0..6) |
| Локалей с `ErrorMessages` | 10 (de,en,es,fr,it,ja,pt-BR,ru,tr,zh-CHS) |
| Ключей в каждой locale | en/de/es/fr/it/ja/pt-BR/tr = 500; zh-CHS = 499; **ru = 507 (расширено до 100%; штатно было 320)** |
| Объединение ключей (union) | 500 |
| Префиксы в union | Err_ = 427, Wrn_ = 66, Inf_ = 4, Txt_ = 1 |
| MessageId без текста ни в одной локали | 16 (вкл. `None`/id 0) → 15 реальных |
| Ключей ресурса, отсутствующих в enum | 9 |
| Ключей, отсутствующих в штатном RU (fallback EN) | 180 (в расширенном RU — 0) |

## (a) Сборки-источники

| Размер, байт | Сборка | Путь |
|---|---|---|
| 898816 | Common\Compiler.dll | `C:\Program Files\CODESYS 3.5.22.10\CODESYS\Common\Compiler.dll` |
| 2418944 | Compiler35220.plugin.dll | `...\PlugIns\22222222-4050-40ca-8734-808abf3dcd09\3.5.22.10\Compiler35220.plugin.dll` |
| 93952 | Compiler35220.ru.resources.dll | `...\22222222-...\3.5.22.10\ru\Compiler35220.plugin.resources.dll` |
| 102144 | Compiler35220.en.resources.dll | `...\22222222-...\3.5.22.10\en\Compiler35220.plugin.resources.dll` |
| 222976 | Parser35220.plugin.dll | `...\58960fbc-9383-453d-a871-1061a37a69da\3.5.22.10\Parser35220.plugin.dll` |
| 208640 | Parser35210.plugin.dll | `...\03cc6aad-2805-4048-a27e-62553eef1b43\3.5.22.10\Parser35210.plugin.dll` |
| 393472 | WhiteParsetrees.plugin.dll | `...\977d3b32-f34f-4f5b-864c-b843b44550dd\3.5.22.10\WhiteParsetrees.plugin.dll` |

## (b) Метод извлечения

```powershell
$asm=[System.Reflection.Assembly]::LoadFile($dll)
foreach($n in $asm.GetManifestResourceNames()){
  $s=$asm.GetManifestResourceStream($n)
  $r=New-Object System.Resources.ResourceReader($s)   # try/catch: не все потоки = .resources
  $e=$r.GetEnumerator(); while($e.MoveNext()){ "{0}`t{1}`t{2}" -f $n,$e.Key,$e.Value }
  $r.Close(); $s.Close() }
$a=[System.Reflection.Assembly]::ReflectionOnlyLoadFrom("...\Common\Compiler.dll")
$t=$a.GetType("_3S.CoDeSys.LanguageModelManager.InternalInterfaces.MessageId")
$t.GetFields("Public,Static,NonPublic") | % { $_.Name + " = " + $_.GetRawConstantValue() }
```

## (c) Полный дамп, сгруппированный по темам

### 1. Ожидался токен/конструкция (expected)

| ID | Rust variant | Key | RU | EN |
|---|---|---|---|---|
| 2 | `ErrOperator1of2Expected` | Err_Operator1of2Expected | '{0}' или '{1}' требуется вместо '{2}' | '{0}' or '{1}' expected instead of '{2}' |
| 6 | `ErrOperatorExpected` | Err_OperatorExpected | '{0}' требуется вместо '{1}' | '{0}' expected instead of '{1}' |
| 7 | `ErrExpressionExpectedInstead` | Err_ExpressionExpectedInstead | Вместо '{0}' требуется выражение | Expression expected instead of '{0}' |
| 8 | `ErrOperator1of3ExpectedInsteadofEOF` | Err_Operator1of3ExpectedInsteadofEOF | Недопустимый End-of-file: требуется '{0}', '{1}' или '{2}' | Unexpected End-of-file found: '{0}', '{1}' or '{2}' expected |
| 9 | `ErrUnexpectedTokenFound` | Err_UnexpectedTokenFound | Обнаружен недопустимый символ '{0}' | Unexpected token '{0}' found |
| 10 | `ErrOperatorExpectedInsteadofEOF` | Err_OperatorExpectedInsteadofEOF | Недопустимый End-of-file: требуется '{0}' | Unexpected End-of-file found: '{0}' expected |
| 13 | `ErrAtLeastOneExpected` | Err_AtLeastOneExpected | Требуется не менее одного оператора | At least one statement is expected |
| 14 | `ErrConditionExpected` | Err_ConditionExpected | Требуется условие | Condition expected |
| 15 | `ErrCounterStartExpected` | Err_CounterStartExpected | Требуется начальное значение счетчика | Counter initialisation expected |
| 16 | `ErrUpperBoundExpected` | Err_UpperBoundExpected | Требуется верхняя граница для FOR-цикла | Upper bound for FOR loop expected |
| 25 | `ErrOperator1of4Expected` | Err_Operator1of4Expected | '{0}', '{1}',  '{2}' или '{3}' требуется вместо '{4}' | '{0}', '{1}',  '{2}' or '{3}' expected instead of '{4}' |
| 26 | `ErrIdentifierExpected` | Err_IdentifierExpected | Вместо '{0}' требуется идентификатор | Identifier expected instead of '{0}' |
| 27 | `ErrStringSizeExpected` | Err_StringSizeExpected | После "(" требуется размер строки | size of string expected after "(" |
| 28 | `ErrOperator1of3Expected` | Err_Operator1of3Expected | '{0}', '{1}' или '{2}' требуется вместо '{3}' | '{0}', '{1}' or '{2}' expected instead of '{3}' |
| 30 | `ErrAddressExpected` | Err_AddressExpected | После "AT" вместо {0} требуется прямой адрес | Direct address expected after AT instead of {0} |
| 31 | `ErrTypeExpected` | Err_TypeExpected | Вместо '{0}' требуется определение типа | Type definition expected instead of '{0}' |
| 51 | `ErrAttributeNameExpected` | Err_AttributeNameExpected | Для значения атрибута вместо '{0}' требуется однобайтовая строка | Single byte string expected for an attribute value instead of '{0}' |
| 74 | `ErrUnexpectedArrayInitialisation` | Err_UnexpectedArrayInitialisation | Недопустимая инициализация массива | Unexpected array initialisation |
| 76 | `ErrUnexpectedStructureInitialisation` | Err_UnexpectedStructureInitialisation | Недопустимая инициализация структуры | Unexpected structure initialisation |
| 81 | `ErrUnexpectedPragmaif` | Err_UnexpectedPragmaif | Недопустимая директива: '{0}' обнаружено без соответствующего 'if' | Unexpected pragma: '{0}' found without matching 'if' |
| 83 | `ErrUnexpectedOperandForIndexOf` | Err_UnexpectedOperandForIndexOf | Недопустимый операнд '{0}' в '{1}' | Unexpected operand '{0}' found in '{1}' |
| 85 | `ErrDefineValueExpected` | Err_DefineValueExpected | Значение требуется вместо '{0}' | Define value expected instead of '{0}' |
| 189 | `ErrSemicolonExpected` | Err_SemicolonExpected | ';' требуется вместо '{0}' | ';' expected instead of '{0}' |
| 190 | `ErrSemicolonExpectedInsteadOfEnd` | Err_SemicolonExpectedInsteadOfEnd | Вместо конца POU требуется ';' | ';' expected instead of end of POU |
| 211 | `ErrVariableDeclarationExpected` | Err_VariableDeclarationExpected | Вместо {0} требуется объявление переменной | Variable declaration expected instead of {0} |
| 212 | `ErrVariableListExpected` | Err_VariableListExpected | VAR, VAR_INPUT, VAR_OUTPUT или VAR_INOUT требуется вместо {0} | VAR, VAR_INPUT, VAR_OUTPUT or VAR_IN_OUT expected instead of {0} |
| 213 | `ErrGlobalVariableListExpected` | Err_GlobalVariableListExpected | VAR_GLOBAL или VAR_CONFIG требуется вместо {0} | VAR_GLOBAL or VAR_CONFIG expected instead of {0} |
| 230 | `ErrUnexpectedTypeName` | Err_UnexpectedTypeName | Имя типа '{0}' здесь неуместно | Type name '{0}' not expected in this place |
| 231 | `ErrSpecialTypeExpected` | Err_SpecialTypeExpected | Здесь требуется выражение типа '{0}' | Expression of type '{0}' expected in this place |
| 232 | `ErrArrayInitializationExpected` | Err_ArrayInitializationExpected | Требуется инициализация массива | Array initialisation expected |
| 233 | `ErrStructureInitializationExpected` | Err_StructureInitializationExpected | Для {0} требуется список инициализации | Initialisation list for {0} expected |
| 317 | `ErrLiteralExpected` | Err_LiteralExpected | Вместо '{0}' требуется литерал | Literal expected instead of '{0}' |
| 347 | `ErrUnexpectedOperandForCallInitFunction` | Err_UnexpectedOperandForCallInitFunction | Неуместный операнд '{0}' в '{1}'. GVL или PRG должны передаваться как параметр. | Unexpected operand '{0}' found in '{1}'. A GVL or PRG must be passed as parameter. |
| 366 | `ErrImplicitEnumerationTypeNotExpected` | Err_ImplicitEnumerationTypeNotExpected | Здесь требуется неявное перечисление | Implicit enumeration type not expected in this place |
| 379 | `ErrIntegerLiteralExpected` | Err_IntegerLiteralExpected | — | Integer literal expected instead of {0} |
| 426 | `WrnAtLeastOneExpected` | Wrn_AtLeastOneExpected | Требуется не менее одного оператора | At least one statement is expected |
| 449 | `ErrComparisonOperatorExpected` | Err_ComparisonOperatorExpected | — | Comparison operator expected instead of '{0}' |
| 450 | `ErrStringLiteralExpected` | Err_StringLiteralExpected | — | String literal expected instead of '{0}' |
| 559 | `ErrFCallExpectedPointerAsSecondArgument` | Err_FCallExpectedPointerAsSecondArgument | — | The second argument of FCall must be a pointer to a function. |
| 578 | `ErrUnexpectedStatement` | Err_UnexpectedStatement | — | Unexpected statement |

### 2. Лишний/недопустимый токен (unexpected, illegal)

| ID | Rust variant | Key | RU | EN |
|---|---|---|---|---|
| 11 | `ErrNoCaseLabelFound` | Err_NoCaseLabelFound | Метка не найдена | No CASE label found |
| 12 | `ErrNoValidCondition` | Err_NoValidCondition | '{0}' не является корректным условием | '{0}' is no valid condition |
| 20 | `ErrNoValidStatement` | Err_NoValidStatement | '{0}' не является корректным заявлением | '{0}' is no valid statement |
| 21 | `ErrNoValidOperand` | Err_NoValidOperand | '{0}' не является корректным операндом | '{0}' is no valid operand |
| 24 | `ErrIllegalOperator` | Err_IllegalOperator | '{0}' не является корректным ST-оператором | '{0}' is no valid ST operator |
| 82 | `ErrNoValidConditionforPragma` | Err_NoValidConditionforPragma | '{0}' не является подходящим условием для директивы | '{0}' is no valid condition for pragma |
| 84 | `ErrNoValidOperandforPragma` | Err_NoValidOperandforPragma | '{0}' не является подходящим операндом для директивы | '{0}' is no valid operand for pragma |
| 124 | `ErrNoValidEnumInit` | Err_NoValidEnumInit | {0} не является подходящим значением для перечисления | {0} is no valid initialisation for an enumeration |
| 194 | `ErrCaseLabelOutsideOfCase` | Err_CaseLabelOutsideOfCase | Метка case не является частью выражения case | CASE label is not part of a CASE statement |
| 341 | `ErrOperatorNotAllowedAtPosition` | Err_OperatorNotAllowedAtPosition | Использование оператора '{0}' недопустимо в данном выражении | The usage of the operator '{0}' is not allowed in this statement |

### 3. Незакрытый блок / разделитель

| ID | Rust variant | Key | RU | EN |
|---|---|---|---|---|
| 383 | `ErrIncompletePOUDeclaration` | Err_IncompletePOUDeclaration | — | The POU declaration is incomplete without a name |
| 588 | `ErrMissingImplementationTerminator` | Err_MissingImplementationTerminator | — | Could not find the terminator '{0}' for the implementation block. |

### 4. Неизвестный идентификатор/тип/имя

| ID | Rust variant | Key | RU | EN |
|---|---|---|---|---|
| 46 | `ErrIdentNotDefined` | Err_IdentNotDefined | Идентификатор '{0}' не задан | Identifier '{0}' not defined |
| 63 | `ErrContainsNoDefinition` | Err_ContainsNoDefinition | '{0}' не содержит определения для '{1}' | '{0}' contains no definition for '{1}' |
| 65 | `ErrNoGlobalDefine` | Err_NoGlobalDefine | Отсутствует глобальне определение для '{0}' | There is no global definition for '{0}' |
| 77 | `ErrUnknownType` | Err_UnknownType | Неизвестный тип: '{0}' | Unknown type: '{0}' |
| 86 | `ErrInterfaceNotFound` | Err_InterfaceNotFound | Не найдено определения для интерфейса '{0}' | No definition found for interface '{0}' |
| 90 | `ErrBaseClassNotFound` | Err_BaseClassNotFound | Для базового класса '{0}' отсутствует определение | No definition found for base class '{0}' |
| 117 | `ErrNoSuchLabel` | Err_NoSuchLabel | Отсутствует метка '{0}' в области выражения JMP. | No such label '{0}' within the scope of the JMP statement |
| 136 | `ErrAmbiguity` | Err_Ambiguity | неоднозначное использование имени '{0}'  | Ambiguous use of name '{0}' |
| 207 | `ErrNoSystemDefine` | Err_NoSystemDefine | Отсутствует системное определение для '{0}' | There is no system definition for '{0}' |
| 225 | `ErrNotAnInstanceOf` | Err_NotAnInstanceOf | '{0}' не является экземпляром '{1}' | '{0}' is not an instance of '{1}' |
| 243 | `ErrNamesNotEqual` | Err_NamesNotEqual | Имя, используемое в интерфейсе, не совпадает с именем объекта | The name used in the signature is not identical to the object name |

### 5. Выражение/оператор/сравнение/тип

| ID | Rust variant | Key | RU | EN |
|---|---|---|---|---|
| 32 | `ErrTypeMismatch` | Err_TypeMismatch | Невозможно конвертировать тип '{0}' в тип '{1}' | Cannot convert type '{0}' to type '{1}' |
| 66 | `ErrTypesNotComparable` | Err_TypesNotComparable | Невозможно сравнить тип '{0}' с типом '{1}' | Cannot compare type '{0}' with type '{1}' |
| 68 | `ErrCompareNotPossible1` | Err_CompareNotPossible1 | Сравнение невозможно для объектов типа '{0}' | Compare not possible on objects of type '{0}' |
| 69 | `ErrCompareNotPossible2` | Err_CompareNotPossible2 | Сравнение невозможно для объектов типа '{0}' или '{1}' | Compare not possible on objects of type '{0}' or '{1}' |
| 115 | `ErrCalcNeedsCall` | Err_CalcNeedsCall | Второй параметр условного вызова должен соответствующим оператором вызова | Second parameter of conditional call must be a valid call statement |
| 183 | `ErrInvalidBaseForGlobalScopeExpression` | Err_InvalidBaseForGlobalScopeExpression | Операция глобальной области '.' некорректна над выражением '{0}' | Global scope operation '.' is not valid on expression '{0}' |
| 309 | `ErrOperatorNotSupported` | Err_OperatorNotSupported | Оператор '{0}' не поддерживается | Operator '{0}' not supported |
| 335 | `WrnComplexExpression` | Wrn_ComplexExpression | POU содержит очень сложное выражение. Попробуйте использовать промежуточные результаты. | The POU contains very complex expression. Consider using intermediate results. |
| 381 | `ErrOperatorNoValidVariableName` | Err_OperatorNoValidVariableName | — | The operator '{0}' is not a valid name for a variable |
| 448 | `ErrOperatorNotSupportedVersion` | Err_OperatorNotSupportedVersion | — | Operator '{0}' is not supported by your current target. At least runtime system version {1} is required. |
| 454 | `ErrNoNewAssignmentInOtherExpression` | Err_NoNewAssignmentInOtherExpression | — | It is not possible to use an assignment expression with the __NEW operator in another expression. Use the pointer variable instead. |

### 6. CASE / метки / ветвления

| ID | Rust variant | Key | RU | EN |
|---|---|---|---|---|
| 216 | `ErrCaseLabelDuplicate` | Err_CaseLabelDuplicate | Повторяющаяся метка case | CASE label duplicate |
| 217 | `ErrCaseLabelInCaseRange` | Err_CaseLabelInCaseRange | Метка Case {0} также содержится в диапазоне {1} .. {2} | CASE label {0} also contained in range {1} .. {2} |
| 218 | `ErrCaseLabelNoConstant` | Err_CaseLabelNoConstant | Для метки сase требуется литерал или символьная целочисленная константа | CASE label requires literal or symbolic integer constant |
| 219 | `ErrCaseRangesOverlapping` | Err_CaseRangesOverlapping | Блок содержит пересекающиеся диапазоны {0} .. {1} и {2} .. {3} | CASE contains overlapping range {0} .. {1} and {2} .. {3} |
| 372 | `ErrDuplicateElseInCaseStatement` | Err_DuplicateElseInCaseStatement | Повторное определение блока CASE | Duplicate definition of ELSE in CASE statement |

### 7. Присваивание / ссылки / индексация

| ID | Rust variant | Key | RU | EN |
|---|---|---|---|---|
| 18 | `ErrIsNoLValue` | Err_IsNoLValue | '{0}' не является корректным объектом присваивания | '{0}' is no valid assignment target |
| 19 | `ErrRValueRequired` | Err_RValueRequired | Присваиваие не имеет корректного источника | Assignment has no valid source |
| 33 | `WrnPointerMisatch` | Wrn_PointerMisatch | Тип '{0}', вероятно, не конвертируется в тип '{1}' | Type '{0}' possibly not convertible to type '{1}' |
| 47 | `ErrIndexingInvalid` | Err_IndexingInvalid | Невозможно применить индексацию с [] к выражению типа '{0}' | Cannot apply indexing with [] to an expression of type '{0}' |
| 48 | `ErrArrayIndexNumWrong` | Err_ArrayIndexNumWrong | Для массива требуется ровно {0} индексов | Array requires exactly {0} indexes |
| 64 | `ErrDerefNoPointer` | Err_DerefNoPointer | Для разыменования требуется указатель | Dereference requires a pointer |
| 105 | `ErrOutOfMemoryFunctionPointer` | Err_OutOfMemoryFunctionPointer | Недостаточно памяти глобальных данных: указатель функции для '{0}' не может быть выделен | Out of global data memory: Function pointer for function '{0}' could not be allocated |
| 118 | `WrnLabelNoReference` | Wrn_LabelNoReference | Метка '{0}' не используется | The label '{0}' has not been referenced |
| 126 | `ErrIndexNumWrong` | Err_IndexNumWrong | Переменная '{0}' требует хотя бы 1 индекса | Variable of type '{0}' requires exactly 1 Index |
| 141 | `ErrLValueForReference` | Err_LValueForReference | Требуется переменная с доступом для записи | Reference assign needs variable with write access |
| 187 | `WrnExternalReferenceIgnored` | Wrn_ExternalReferenceIgnored | Внешние ссылки допустимы только для функциональных блоков, методов и функций. Внешняя ссылка для {0} '{1}' проигнорирована. | External references are only possible for function blocks, methods, functions and constant global variable lists. External reference for {0} '{1}' is ignored. |
| 205 | `ErrNoPointerToBit` | Err_NoPointerToBit | POINTER TO BIT недопустим | POINTER TO BIT is not allowed |
| 222 | `ErrNoReferenceToOutput` | Err_NoReferenceToOutput | Выходы не могут быть типа REFERENCE TO | Outputs can't be of type REFERENCE TO |
| 227 | `ErrNoConstantInitialisationForValue` | Err_NoConstantInitialisationForValue | Начальное значение константной переменной '{0}' не является константой | Initialisation of constant variable '{0}' not constant |
| 240 | `ErrQueryPointerErrorP1` | Err_QueryPointerErrorP1 | Первый операнд __QueryPointer должен быть ссылкой интерфейса или экземпляром функционального блока | First operand of __QueryPointer must be an interface reference or the instance of a function block |
| 241 | `ErrQueryPointerErrorP2` | Err_QueryPointerErrorP2 | Второй операнд __QueryInterface должен быть указателем | Second operand of __QueryInterface must be a pointer |
| 242 | `ErrDeleteNeedsPointer` | Err_DeleteNeedsPointer | Операнд __DELETE должен быть указателем | Operand of __DELETE must be pointer |
| 261 | `ErrReferenceNotAllowed` | Err_ReferenceNotAllowed | Ссылочный тип недопустим в качестве базового типа массива, указателя или ссылки | A reference type is not allowed as base type of an array, pointer, or reference |
| 262 | `ErrReferenceNotAllowedForVarInOut` | Err_ReferenceNotAllowedForVarInOut | Тип ссылки является недопустим как тип VAR_IN_OUT | A reference type is not allowed as type of a VAR_IN_OUT |
| 269 | `WrnValueAssignViaPointerMayChangeVFTable` | Wrn_ValueAssignViaPointerMayChangeVFTable | Для экземпляра, на который указывает {0}, будет выполнена переинициализация для виртуальных вызовов функции. Убедитесь, что {0} не указывает на тип, полученный из {1} | The instance {0} points to will be reinitialized for virtual function calls. Make sure {0} doesn't point to a type derived from {1}  |
| 272 | `ErrNoReferenceToBits` | Err_NoReferenceToBits | Ссылки на биты недопустимы | References to bits are not possible |
| 343 | `ErrReferenceToInput` | Err_ReferenceToInput | Присваивание ссылки невозможно для переменной, соотнесенной с входным адресом. | Reference assign is not possible on a variable mapped to an input address |
| 353 | `ErrRefAssignNeedsLValue` | Err_RefAssignNeedsLValue | Для присваивания ссылки в качестве исходного выражения требуется переменная с доступом для записи | A reference assignment requires a variable with write access as the source expression |
| 355 | `WrnNoPointerToBit` | Wrn_NoPointerToBit | Ссылка на один бит невозможна. Будет сохранена ссылка на весь байт. | A single bit cannot be referenced. A reference to the complete byte will be stored. |
| 396 | `ErrIsValidRefNeedsReference` | Err_IsValidRefNeedsReference | — | Operand for __ISVALIDREF must be of type REFERENCE |
| 399 | `ErrImplicitReferenceTypeDeclNotAllowed` | Err_ImplicitReferenceTypeDeclNotAllowed | — | The declaration of an implicit reference type is not possible at this location |
| 408 | `ErrImplicitReferenceTypeOnlChangeError` | Err_ImplicitReferenceTypeOnlChangeError | — | It is not possible to add a local implicit reference type via online change |
| 413 | `ErrRefAssignOnlyForReferenceTypes` | Err_RefAssignOnlyForReferenceTypes | — | Initialisation with REF= is only allowed for variables of type REFERENCE TO |
| 423 | `ErrMultipleAssignmentWithReferences` | Err_MultipleAssignmentWithReferences | — | Multiple assignments of references to function blocks or data structures not allowed |
| 549 | `ErrNoStaticVariableInitialisationForValue` | Err_NoStaticVariableInitialisationForValue | — | Initialisation of static variable '{0}' not constant or replaced constants is disabled |
| 564 | `WrnReferenceToUninitializedVariable` | Wrn_ReferenceToUninitializedVariable | — | A reference to uninitialized variable {0} is used for initialization of {1}. Accessing the uninitialized variable may result in unexpected behavior. |

### 8. Объявления / секции / модификаторы

| ID | Rust variant | Key | RU | EN |
|---|---|---|---|---|
| 168 | `ErrVarConfigNotAllowed` | Err_VarConfigNotAllowed | Объявление VAR_CONFIG допустимо только в списке VAR_CONFIG | VAR_CONFIG declaration only allowed in VAR_CONFIG  list |
| 169 | `ErrVarGlobalNotAllowed` | Err_VarGlobalNotAllowed | Объявление в 'VAR_GLOBAL' допустимо только в списке глобальных переменных | VAR_GLOBAL declaration only allowed in global variable list |
| 170 | `ErrStructureNotAllowed` | Err_StructureNotAllowed | Объявление 'STRUCT' допустимо только в DUT | STRUCT declaration only allowed in data unit type |
| 171 | `ErrUnionNotAllowed` | Err_UnionNotAllowed | Объявление UNION допустимо только в DUT | UNION declaration only allowed in data unit type |
| 172 | `ErrStaticNotAllowed` | Err_StaticNotAllowed | Объявление 'VAR_STAT' здесь недопустимо | VAR_STAT declaration not allowed in this place |
| 173 | `ErrDeclarationKeywordNotAllowed` | Err_DeclarationKeywordNotAllowed | '{0}' здесь недопустимо | '{0}' not allowed in this place |
| 174 | `ErrVarTempNotAllowed` | Err_VarTempNotAllowed | Объявление 'VAR_TEMP' здесь недопустимо | VAR_TEMP declaration not allowed in this place |
| 175 | `ErrRetainOrPersistentNotAllowed` | Err_RetainOrPersistentNotAllowed | 'RETAIN' и 'PERSISTENT' здесь недопустимо | RETAIN or PERSISTENT not allowed in this place |
| 280 | `ErrFinalOnFunctionBlocksAndMethodsOnly` | Err_FinalOnFunctionBlocksAndMethodsOnly | FINAL можно применять только к методам и функциональным блокам | FINAL may only be applied on methods and function blocks |
| 281 | `ErrPrivateOnMethodsOnly` | Err_PrivateOnMethodsOnly | PRIVATE и PROTECTED можно применять только к методам и функциональным блокам | PRIVATE and PROTECTED may only be applied on methods of function blocks |
| 282 | `ErrNoInheritanceOnFinalType` | Err_NoInheritanceOnFinalType | '{0}' с атрибутом FINAL не может использоваться в качестве основы | '{0}' with attribute FINAL may not be used as base |
| 283 | `ErrNoOverrideOnFinalMethod` | Err_NoOverrideOnFinalMethod | — | Function block '{0}': No override possible on method {1}.{2} with access specifier FINAL |
| 284 | `ErrNoOverrideOnPrivateMethod` | Err_NoOverrideOnPrivateMethod | — | Function block '{0}': No override possible on method {1}.{2} with access specifier PRIVATE |
| 287 | `ErrCallOfProtectedMethod` | Err_CallOfProtectedMethod | Невозможно получить доступ к методу {0}.{1} | Cannot access protected method {0}.{1} |
| 288 | `ErrCallOfPrivateMethod` | Err_CallOfPrivateMethod | Не удается получить доступ к частному методу {0}.{1} | Cannot access private method {0}.{1} |
| 289 | `ErrCallOfProtectedProperty` | Err_CallOfProtectedProperty | Невозможно обратиться к защищенному свойству {0}.{1} | Cannot access protected property {0}.{1} |
| 290 | `ErrCallOfPrivateProperty` | Err_CallOfPrivateProperty | Обращение к частному свойству {0}.{1} невозможно | Cannot access private property {0}.{1} |
| 337 | `ErrVarInstOnlyInMethods` | Err_VarInstOnlyInMethods | Объявление 'VAR_INST' недопустимо в этом месте | VAR_INST declaration not allowed in this place |
| 392 | `ErrATDeclarationNotAllowed` | Err_ATDeclarationNotAllowed | — | An AT declaration is only allowed inside a VAR section |
| 402 | `ErrImplicitRefTypeDeclarationNotAllowed` | Err_ImplicitRefTypeDeclarationNotAllowed | — | The implicit reference type '{0}' cannot be declared in this type |
| 428 | `ErrAbstractOnFunctionBlocksAndMethodsOnly` | Err_AbstractOnFunctionBlocksAndMethodsOnly | — | ABSTRACT may only be applied on methods and function blocks |
| 429 | `ErrAbstractAndFinalNotPossible` | Err_AbstractAndFinalNotPossible | — | A method or functionblock cannot be ABSTRACT and FINAL |
| 430 | `ErrAbstractAndPrivateNotPossible` | Err_AbstractAndPrivateNotPossible | — | A PRIVATE method cannot be ABSTRACT |
| 431 | `ErrAbstractMethodNotImplemented` | Err_AbstractMethodNotImplemented | — | There is no implementation for ABSTRACT method '{0}' defined in function block '{1}' |
| 432 | `ErrAbstractMethodOnlyInAbstractFunctionblock` | Err_AbstractMethodOnlyInAbstractFunctionblock | — | The ABSTRACT method {0} requires, that the function block {1} is ABSTRACT too |
| 433 | `ErrAbstractMethodMustNotContainAnyStatements` | Err_AbstractMethodMustNotContainAnyStatements | — | The ABSTRACT method {0}.{1} must not contain any statements |
| 434 | `ErrAbstractFunctionBlockInstance` | Err_AbstractFunctionBlockInstance | — | Function block {0} is ABSTRACT and cannot be instantiated |
| 435 | `ErrAbstractPropertyNotImplemented` | Err_AbstractPropertyNotImplemented | — | There is no implementation for {0} of ABSTRACT property '{1}' defined in function block '{2}' |
| 442 | `ErrAbstractMethodStaticCall` | Err_AbstractMethodStaticCall | — | The ABSTRACT method '{0}' cannot be called by a static call |
| 443 | `ErrAbstractPropertyStaticCall` | Err_AbstractPropertyStaticCall | — | The ABSTRACT property '{0}' cannot be statically accessed |
| 445 | `ErrAbstractWrongVarInMethod` | Err_AbstractWrongVarInMethod | — | Only inputs, outputs and inouts allowed in ABSTRACT methods |
| 511 | `ErrAbstractFunctionBlockAssigned` | Err_AbstractFunctionBlockAssigned | — | Function block {0} is ABSTRACT and cannot be used as a target for an assignment |
| 513 | `WrnCallOfPrivateProperty` | Wrn_CallOfPrivateProperty | — | Should not access private property {0}.{1} |
| 515 | `WrnCallOfProtectedProperty` | Wrn_CallOfProtectedProperty | — | Should not access protected property {0}.{1} |
| 528 | `ErrNoBranchOutOfFinally` | Err_NoBranchOutOfFinally | — | Branch out of __FINALLY-Block is not allowed |
| 533 | `WrnObsoleteOutputInAbstractMethod` | Wrn_ObsoleteOutputInAbstractMethod | — | The default value for a VAR_OUTPUT is not used in abstract or interface methods |
| 536 | `ErrInvalidInitialisationForVarInst` | Err_InvalidInitialisationForVarInst | — | The initial value for a VAR_INST variable may not use local variables. |
| 553 | `ErrGenericDeclarationProducesErrorInGeneratedCode` | Err_GenericDeclarationProducesErrorInGeneratedCode | — | The type declaration '{0}' leads to errors in the Generic Functionblock code |
| 573 | `WrnAbstractKeywordMissing` | Wrn_AbstractKeywordMissing | — | The ABSTRACT keyword is missing |
| 576 | `ErrAccessVarInstFromOutsideTheDeclaringMethod` | Err_AccessVarInstFromOutsideTheDeclaringMethod | — | Cannot access VAR_INST '{0}' of '{1}' from outside the declaring method |
| 580 | `WrnConstantInStructDeclaration` | Wrn_ConstantInStructDeclaration | — | The keyword CONSTANT is ignored in the declaration of a structure or union |

### 9. Прагмы / атрибуты / языковые фичи

| ID | Rust variant | Key | RU | EN |
|---|---|---|---|---|
| 263 | `ErrNewOnlyWithAttribute` | Err_NewOnlyWithAttribute | Для функционального блока или структуры требуется атрибут '{{attribute 'enable_dynamic_creation'}}' для создания с помощью __NEW | A function block or structure needs the pragma '{{attribute 'enable_dynamic_creation'}}' to be created with __NEW |
| 330 | `ErrTryCatchNotSupportedVersion` | Err_TryCatchNotSupportedVersion | Структурная обработка исключений не поддерживается вашим текущим таргетом. Требуется система исполнения версии не ниже 3.5.6.0. | Structured exception handling is not supported by your current target. At least runtime system version 3.5.6.0 is required. |
| 331 | `ErrTryCatchNotSupportedCodegenerator` | Err_TryCatchNotSupportedCodegenerator | Генератор кода для текущего устройства не поддерживает структурную обработку исключений | The code generator for the current device does not support structured exception handling |
| 345 | `ErrTryCatchNotSupported` | Err_TryCatchNotSupported | Структурная обработка исключений пока не поддерживается | Structured exception handling is not supported yet |
| 351 | `WrnAttributeCheck` | Wrn_AttributeCheck | Неизвестный атрибут или некорректное значение | Attribute check failed |
| 356 | `ErrAttributeNotValidForNonExternalPOU` | Err_AttributeNotValidForNonExternalPOU | Атрибут '{0}' не подходит для невнешнего POU | Attribute '{0}' not valid for non-external POU |
| 369 | `ErrFeatureNotImplemented` | Err_FeatureNotImplemented | Функция '{0}' не реализована. | The feature '{0}' is not implemented. |
| 373 | `WrnPragma` | Wrn_Pragma | Пользовательское предупреждение, сгенерированное прагмой | User defined warning generated by warning pragma |
| 382 | `ErrInconsistentInheritanceOfCPPCompatibility` | Err_InconsistentInheritanceOfCPPCompatibility | — | Inconsistent inheritance of C++-compatibility. Missing attribute for '{0}'. |
| 419 | `ErrNoInputsWithSlotAttributes` | Err_NoInputsWithSlotAttributes | — | No inputs allowed in signature '{0}' with attribute '{1}' |
| 444 | `ErrInconsistentUseOfCPPCompatibility` | Err_InconsistentUseOfCPPCompatibility | — | Inconsistent use of C++ compatibility. Missing attribute for '{0}'. |
| 446 | `ErrNoValuePassingForCPPExternal` | Err_NoValuePassingForCPPExternal | — | CPP compatible functions can not contain inputs or outputs of Type STRING, ARRAY, or structured types. Use POINTER or REFEFRENCE to this type instead. |
| 500 | `ErrVectorSizeNotValid` | Err_VectorSizeNotValid | — | The size of a vector must be a integer greater than 0 and less than 9 |
| 501 | `ErrVectorSizeIsNoConstant` | Err_VectorSizeIsNoConstant | — | The size of a vector must be a constant |
| 502 | `ErrVectorNoPersistentRetain` | Err_VectorNoPersistentRetain | — | A vector cannot be declared persistent or retained |
| 503 | `ErrVectorBaseMustBeRealType` | Err_VectorBaseMustBeRealType | — | The base type of a vector must be either REAL or LREAL. |
| 504 | `ErrVectorTypesNotCompatible` | Err_VectorTypesNotCompatible | — | The vector types {0} and {1} are not compatible |
| 505 | `ErrVectorTypeCantBePlacedInUnion` | Err_VectorTypeCantBePlacedInUnion | — | Vector types cannot be placed in unions |
| 540 | `WrnMissingAttributeNoAssign` | Wrn_MissingAttributeNoAssign | — | Attribute 'no_assign' missing for POU '{0}'? The type of the variable '{1}' is attributed with 'no_assign'. |
| 544 | `ErrGenericOnWrongPosition` | Err_GenericOnWrongPosition | — | VAR_GENERIC declaration only allowed in Functionblocks after the function block name |
| 545 | `ErrGenericOnlyConst` | Err_GenericOnlyConst | — | Only CONSTANT generics are supported in VAR_GENERIC declaration |
| 546 | `ErrTypeIsNotGeneric` | Err_TypeIsNotGeneric | — | '{0}' is not a generic functionblock. |
| 547 | `ErrGenericWrongNumberOfInitializer` | Err_GenericWrongNumberOfInitializer | — | Generic Functionblock '{0}' expects exactly '{1}' number of Generic Constant Definitions |
| 548 | `ErrGenericNotConstant` | Err_GenericNotConstant | — | The definition '{0}' for the Generic Constant '{1}' is no constant value |
| 550 | `ErrAttributeNotAllowedFor` | Err_AttributeNotAllowedFor | — | Attribute '{0}' not allowed for '{1}' |
| 551 | `ErrGenericNoInteger` | Err_GenericNoInteger | — | Only integer types are allowed for Generic Constants |
| 552 | `ErrNoOutsideAccessToGenericVariable` | Err_NoOutsideAccessToGenericVariable | — | No external access to Generic Constant '{0}' of Functionblock '{1}' |
| 556 | `ErrNoGenericInstanceInVarConst` | Err_NoGenericInstanceInVarConst | — | Instances of a Generic Functionblock cannot be declared as constant |
| 570 | `ErrProjectDefinedNotSupportedFor` | Err_ProjectDefinedNotSupportedFor | — | The condition 'project_defined' is not supported for this syntax, since it might affect the public interface of '{0}'. |
| 574 | `ErrNoOnlineChangeOnGenericConstantType` | Err_NoOnlineChangeOnGenericConstantType | — | In declaration of Variable {0}, the value for the constant {1} changed from {2} to {3}: no online change possible! |
| 575 | `ErrNoOnlineChangeOnGenericConstantBaseType` | Err_NoOnlineChangeOnGenericConstantBaseType | — | In declaration of BaseType {0}, the value for the constant {1} changed from {2} to {3}: no online change possible! |
| 579 | `ErrUnsupportedFeature` | Err_UnsupportedFeature | — | The compiler feature '{0}' is only supported with compiler version {1} or newer |
| 581 | `ErrNoMatchingOverload` | Err_NoMatchingOverload | — | No Matching Overload found for method '{0}' |
| 582 | `ErrOverloadNeedsAttribute` | Err_OverloadNeedsAttribute | — | There is another method with the name '{0}'. Use the Attribute {{attribute 'overloaded'}}, if you want to define overloaded methods. |
| 583 | `ErrOverloadWithSameInputs` | Err_OverloadWithSameInputs | — | There is another overload for '{0}' with the same input-types. |
| 585 | `ErrGenericParamsAllExplicitOrNone` | Err_GenericParamsAllExplicitOrNone | — | Either all generic variables need to be explicitly assigned or none |
| 586 | `ErrGenericParamMissing` | Err_GenericParamMissing | — | Missing initialization for generic variable '{0}' |
| 587 | `ErrGenericParamUnknown` | Err_GenericParamUnknown | — | '{0}' is no declared generic constant of function block '{1}' |

### 10. Память / линковка / онлайн-замена

| ID | Rust variant | Key | RU | EN |
|---|---|---|---|---|
| 49 | `ErrConstantIndexOutOfRange` | Err_ConstantIndexOutOfRange | Константый индекс '{0}' находится вне диапазона от '{1}' до '{2}' | The constant index '{0}' is not within the range from '{1}' to '{2}' |
| 102 | `ErrOutOfRetainMemory` | Err_OutOfRetainMemory | Недостаточно энергонезависимой памяти: Переменная '{0}', {1} байт (Инкрементная компиляция может привести к фрагментации памяти. Выполните команду "Компиляция, очистка" для принудительного распределения всех данных и кода.) | Out of retain memory: Variable '{0}', {1} bytes  (Incremental compilation may produce fragmented memory. Perform "Build, Clean" to force a reallocation of all data and code.) |
| 103 | `ErrOutOfRetainMemoryWithWholeSize` | Err_OutOfRetainMemoryWithWholeSize | Недостаточно энергонезависимой памяти: Переменная '{0}', {1} байт (Наибольший промежуток в памяти - {2})  (Инкрементная компиляция может привести к фрагментации памяти. Выполните команду "Компилировать, Очистить" для принудительного перераспределения данных и кода.) | Out of retain memory: Variable '{0}', {1} bytes (largest contiguous memory gap {2})  (Incremental compilation may produce fragmented memory. Perform "Build, Clean" to force a reallocation of all data and code.) |
| 104 | `ErrOutOfMemory` | Err_OutOfMemory | Недостаточно памяти глобольных данных: Переменная '{0}', {1} байт. (Инкрементная компиляция может привести к фрагментации памяти. Выполните команду "Компилировать, Очистить" для принудительного перераспределения данных и кода.) | Out of global data memory: Variable '{0}', {1} bytes. (Incremental compilation may produce fragmented memory. Perform "Build, Clean" to force a reallocation of all data and code.) |
| 106 | `ErrOutOfMemoryWithWholeSize` | Err_OutOfMemoryWithWholeSize | Недостаточно памяти глобальных данных: переменная '{0}', {1} байт (Наибольший промежуток в памяти - {2})  (Инкрементная компиляция или добавление переменных в persistent-список может привести к фрагментации памяти. Выполните команду "Объявления, переупорядочить список и очистить промежутки" или "Компилировать, Очистить" для принудительного перераспределения прочих данных и кода.) | Out of global data memory: Variable '{0}', {1} bytes (Largest contiguous memory gap {2}). Incremental compilation or adding variables to persistent variable lists may produce fragmented memory. Perform "Declarations, Reorder list and clear gaps" to compact persistent variable lists or "Build, Clean" to force a reallocation of other data and code. |
| 108 | `ErrNoInputMemory` | Err_NoInputMemory | Нет зарезервированной входной памяти | There is no input memory reserved |
| 109 | `ErrNoOutputMemory` | Err_NoOutputMemory | Выходной памяти не зарезервировано | There is no output memory reserved |
| 110 | `ErrNoMemoryMemory` | Err_NoMemoryMemory | Нет зарезервированной памяти | There is no memory reserved |
| 111 | `ErrAddressOutOfRange` | Err_AddressOutOfRange | Адрес '{0}' вне диапазона: Pассчитанный сдвиг: {1}, выделенный размер: {2} | Address '{0}' out of range: Calculated offset: {1} bytes, allocated size: {2} bytes |
| 127 | `ErrOutOfCodeMemory` | Err_OutOfCodeMemory | Недостаточно кодовой памяти: POU '{0}', {1} байт. (Инкрементная компиляция может привести к фрагментации памяти. Выполните команду "Компилировать, Очистить" для принудительного перераспределения данных и кода.) | Out of code memory: POU '{0}', {1} bytes. (Incremental compilation may produce fragmented memory. Perform "Build, Clean" to force a reallocation of all data and code.) |
| 184 | `ErrNoOnlineChangePossible` | Err_NoOnlineChangePossible | Онлайн-замена невозможна, выполните полную загрузку | No online change possible. Perform full download |
| 226 | `ErrNotEnoughMemoryForVariableInit` | Err_NotEnoughMemoryForVariableInit | Недостаточно памяти для инициализации переменной '{0}'. Требуется {1} байт | Not enough memory for initialisation of variable '{0}'. {1} bytes needed |
| 246 | `ErrNoBytesInRetain` | Err_NoBytesInRetain | Структурированный тип, расположенный в энергонезависимых данных, не может содержать BYTE, SINT или USINT | A structured type located in retain data may not contain BYTE, SINT, or USINT |
| 264 | `ErrNoOnlineChangeOnDynamicObjects` | Err_NoOnlineChangeOnDynamicObjects | Динамически создаваемые типы недопустимы для онлайн-замен | Cannot perform a online change that changes the size of a dynamically created type. |
| 265 | `ErrDynamicMemoryNotSupported` | Err_DynamicMemoryNotSupported | Для приложения '{0}' не задано памяти для создания динамического объекта | No memory for dynamic object creation defined for application '{0}' |
| 270 | `ErrRetainsNotSupported` | Err_RetainsNotSupported | Использование энергонезависимых данных невозможно на этом устройстве | Retain data is not not supported on this device |
| 276 | `ErrRetainNotAccessibleVariable` | Err_RetainNotAccessibleVariable | Переменная {0} скрывает имя библиотеки. Доступ к библиотечному элементу невозможен. Код для сохранения энергонезависимой переменной {1} сгенерирован не будет. | Variable {0} hides library name. No access to library elements is possible. No implicit code for saving retain variable {1} will be generated. |
| 277 | `ErrRetainNotAccessiblePOU` | Err_RetainNotAccessiblePOU | POU {0} скрывает имя библиотеки, доступ к элементам библиотеки невозможен. Неявного кода для сохранения энергонезависимой переменной {1} сгенерировано не будет. | POU {0} hides library name, no access to library elements is possible. No implicit code for saving retain variable {1} will be generated. |
| 297 | `ErrStackOverflowDetected` | Err_StackOverflowDetected | — | Stack overflow detected in Task {0}. Maximal Stack Size: {1} bytes. Calculated Stack Size: {2} bytes. Call Hierarchie: |
| 298 | `WrnStackCheckIncompleteDueToRecursion` | Wrn_StackCheckIncompleteDueToRecursion | — | Calculation of stack usage incomplete because of recursive calls, starting at '{0}': |
| 342 | `ErrInstanceNotAllowedInRetain` | Err_InstanceNotAllowedInRetain | Экземпляры '{0}' недопустимы в энергонезависимых данных | Instances of '{0}' are not allowed in retain data |
| 367 | `ErrInternalErrorProhibitingOnlineChange` | Err_InternalErrorProhibitingOnlineChange | Внутренняя ошибка {0}, исключающая онлайн-замену! Требуется очистка приложения и загрузка. | Internal error {0} prohibiting online change. Clean application and download necessary. |
| 398 | `ErrSystemOutOfMemory` | Err_SystemOutOfMemory | Система разработки не располагает достаточной памятью для онлайн-замены. Перезапустите приложение. | The development system has not enough memory to process the online change. Please restart the application before continuing development. |
| 414 | `ErrOutOfPersistentMemoryImplicit` | Err_OutOfPersistentMemoryImplicit | — | Not enough persistent memory {0} |
| 415 | `ErrOutOfPersistentMemoryExplicit` | Err_OutOfPersistentMemoryExplicit | — | Out of persistent memory: Variable '{0}', {1} bytes (Largest contiguous memory gap {2}). Editing persistent variable lists may produce fragmented memory. Perform "Declarations, Reorder list and clear gaps" to compact persistent variable lists. {3} |
| 420 | `ErrNotEnoughMemoryForCompactDownload` | Err_NotEnoughMemoryForCompactDownload | — | Not enough memory left for compact download in first code area. '{0}' bytes could not be allocated |
| 425 | `ErrNoMemoryReserveForExternal` | Err_NoMemoryReserveForExternal | — | Usage of a memory reserve for external function blocks is not supported |
| 427 | `ErrUnknownMaxStackSize` | Err_UnknownMaxStackSize | — | Accessing information about the currently executed task requires knowledge about the maximal stack size supported on the target system |
| 440 | `ErrTaskLocalVariablesNoOnlineChangePossible` | Err_TaskLocalVariablesNoOnlineChangePossible | — | Task local variable list '{0}' changed. No online change possible. |
| 522 | `WrnPersistentVariableOnStack` | Wrn_PersistentVariableOnStack | — | Persistent variables in function block instances located on the stack are not supported: {0} |
| 529 | `ErrNoExitOrContinueOutOfTry` | Err_NoExitOrContinueOutOfTry | — | Exit or Continue out of __TRY-Block is not allowed |
| 530 | `ErrNoExitOrContinueOutOfCatch` | Err_NoExitOrContinueOutOfCatch | — | Exit or Continue out of __CATCH-Block is not allowed |
| 541 | `ErrNoMemoryAllocationCallback` | Err_NoMemoryAllocationCallback | — | Creating the memory allocator failed: The required plug-in is not installed |
| 571 | `WrnExitForRetainInstances` | Wrn_ExitForRetainInstances | — | FB_EXIT of instances in VAR_RETAIN is also called during Reset warm, but not FB_INIT. Avoid retain declaration of function blocks with FB_EXIT! |

### 11. Прочее / без тематической группы

| ID | Rust variant | Key | RU | EN |
|---|---|---|---|---|
| 1 | `ErrConstantOverflow` | Err_ConstantOverflow | Константа '{0}' слишком велика для типа '{1}' | Constant '{0}' too large for type '{1}' |
| 3 | `ErrBitNrOverflow` | Err_BitNrOverflow | '{0}' не является корректным битовым номером для '{1}' | '{0}' is no valid bit number for '{1}' |
| 4 | `ErrNoComponentOf` | Err_NoComponentOf | '{0}' не является компонентом '{1}' | '{0}' is no component of '{1}' |
| 5 | `ErrOverflowInAddress` | Err_OverflowInAddress | Постоянное переполнение по адресу '{0}' | Constant overflow in address '{0}' |
| 17 | `ErrInvalidLoopIncrement` | Err_InvalidLoopIncrement | Некорректный инкремент цикла | Loop increment invalid |
| 22 | `ErrOpNeedsExactInputs` | Err_OpNeedsExactInputs | Для '{0}' требуется ровно '{1}' операндов | '{0}' needs exactly '{1}' operands |
| 23 | `ErrOpNeedsAtLeastInputs` | Err_OpNeedsAtLeastInputs | Для '{0}' необходимо как минимум '{1}' операндов | '{0}' needs at least '{1}' operands |
| 34 | `ErrMultiassigninfor` | Err_Multiassigninfor | Для цикла требуется ровно один счетчик | Exactly one counter expected in FOR loop |
| 35 | `ErrCalleeInvalidType` | Err_CalleeInvalidType | Вместо '{0}' требуется имя программы, функция или экземпляр функционального блока | Program name, function or function block instance expected instead of '{0}' |
| 36 | `ErrWrongObjectType` | Err_WrongObjectType | Невозможно вызвать объект типа '{0}' | Cannot call object of type '{0}' |
| 37 | `ErrIsNoInput` | Err_IsNoInput | '{0}'не является входом '{1}' | '{0}' is no input of '{1}' |
| 38 | `ErrIsNoOutput` | Err_IsNoOutput | '{0}' не является выходом '{1}' | '{0}' is no output of '{1}' |
| 39 | `ErrInOutNotAssigned` | Err_InOutNotAssigned | VAR_IN_OUT '{0}' должен присваиваться в вызове '{1}' | VAR_IN_OUT '{0}' must be assigned in call of '{1}' |
| 40 | `ErrFunNeedsNInputs` | Err_FunNeedsNInputs | Для функции '{0}' требуется ровно '{1}' входов | Function '{0}' requires exactly '{1}' inputs |
| 41 | `ErrLValueForVarinout` | Err_LValueForVarinout | VAR_IN_OUT-параметр '{0}' из '{1}' требует переменной с доступом записи в качестве входа | VAR_IN_OUT respectively REFERENCE parameter '{0}' of '{1}' needs variable with write access as input |
| 42 | `ErrFunctionCallMixedStyle` | Err_FunctionCallMixedStyle | В вызове функции должны быть отмечены либо все, либо никакие формальные параметры | Either all or none formal parameter have to be denoted in function call |
| 43 | `ErrWrongFormalParameter` | Err_WrongFormalParameter | Неверный формальный параметр: '{0}' требуется здесь | Wrong formal parameter: '{0}' expected in this place |
| 44 | `ErrInputMissing` | Err_InputMissing | Присваивание для входа отсутствует для параметра '{0}' в вызове '{1}' | Assignment to input missing for parameter '{0}' in call of '{1}' |
| 45 | `ErrThisNotAllowed` | Err_ThisNotAllowed | Выражение THIS недопустимо в этом контексте | Expression THIS is not allowed in this context |
| 50 | `ErrBitaccessnoconst` | Err_Bitaccessnoconst | Для битового доступа требуется литерал или символьная целочисленная константа | Bit access requires literal or symbolic integer constant |
| 52 | `TxtParentContextNotUpToDate` | Txt_ParentContextNotUpToDate | "Контекст неактуален". | Parent context not up to date |
| 53 | `ErrCompilerVersionError` | Err_CompilerVersionError | Версия компилятора {0} отозвана. Используйте более новую версию. | Compiler version {0} has been withdrawn. Please use a higher compiler version instead. |
| 61 | `ErrNoBitAccessOnFunctionCall` | Err_NoBitAccessOnFunctionCall | Битовый доступ к вызову функции невозможен | Bit access on function call is not allowed |
| 62 | `ErrCompoAccessNoStruct` | Err_CompoAccessNoStruct | '{0}' не является структурированной переменной | '{0}' is no structured variable |
| 70 | `ErrIniNeedsUserdefType` | Err_IniNeedsUserdefType | Для оператора INI необходим экземпляр функционального блока или DUT-экземпляр | INI operator needs function block instance or data unit type instance |
| 71 | `ErrCannotAddMultipleTime` | Err_CannotAddMultipleTime | Невозможно сложить несколько операндов типа '{0}' | Cannot add multiple operands of type '{0}' |
| 72 | `ErrOperationNotPossibleOnType` | Err_OperationNotPossibleOnType | Операция '{0}' невозможна над типом '{1}' | Operation '{0}' is not possible on type '{1}' |
| 73 | `ErrCannotMultiplyMultipleTime` | Err_CannotMultiplyMultipleTime | Нельзя перемножить несколько операндов типа '{0}' | Cannot multiply multiple operands of type '{0}' |
| 75 | `ErrTooManyInitializer` | Err_TooManyInitializer | Слишком много инициализаторов для массива | Too many initializers for array |
| 78 | `ErrUnsupportedType` | Err_UnsupportedType | Неподдерживаемый тип: '{0}' | Unsupported type: '{0}' |
| 80 | `ErrFunctionBlockNeedsInstance` | Err_FunctionBlockNeedsInstance | Функциональный блок '{0}' должен иметь экземпляр | Function block '{0}' must be instantiated to be accessed |
| 87 | `ErrNoMethodImplementation` | Err_NoMethodImplementation | Отсутствует реализация для метода '{0}', заданного в интерфейсе '{1}' | There is no implementation for method '{0}' defined in interface '{1}' |
| 88 | `ErrSomethingOverridingMethod` | Err_SomethingOverridingMethod | {0} '{1}' перезаписывает метод интерфейса '{2}' | {0} '{1}' overrides method of interface '{2}' |
| 89 | `ErrInterfaceChanged` | Err_InterfaceChanged | Интерфейс перезаписанного метода '{0}' интерфейса '{1}' не соответствует объявлению | Interface of overridden method '{0}' of interface '{1}' doesn't match declaration |
| 91 | `ErrSelfInheritanceBase` | Err_SelfInheritanceBase | Рекурсия в списке базовых функциональных блоков: {0} | Recursion in base function block list: {0} |
| 92 | `ErrOnlyMethodsOverride` | Err_OnlyMethodsOverride | Невозможно заменить {0} {1} из {2}: только методы допустимы для перезаписи | Not possible to override {0} {1} of {2}: Only methods allowed to override |
| 93 | `ErrSomethingOverridingMethodBase` | Err_SomethingOverridingMethodBase | — | {0} '{1}' overrides method of base '{2}' |
| 94 | `ErrInterfaceChangedBase` | Err_InterfaceChangedBase | Интерфейс перезаписанного метода '{0}' основы '{1}' не соответствует объявлению | Interface of overridden method '{0}' of base '{1}' doesn't match declaration |
| 95 | `ErrSelfInheritance` | Err_SelfInheritance | Рекурсия в интерфейсах: {0} | Recursion in interfaces: {0} |
| 96 | `ErrNoMultipleInheritance` | Err_NoMultipleInheritance | В списке EXTENDS (расширение) можно задать только один функциональный блок | Only one base function block may be defined in EXTENDS list |
| 97 | `ErrVariableOverride` | Err_VariableOverride | Повторяющееся определение переменной '{0}' в функциональном блоке '{1}' и в основе '{2}' | Duplicate definition of variable '{0}' in function block '{1}' and in base '{2}' |
| 98 | `ErrFunctionBlockNoLongerValid` | Err_FunctionBlockNoLongerValid | Ключевое слово FUNCTIONBLOCK больше не поддерживается. Используйте FUNCTION_BLOCK | The keyword FUNCTIONBLOCK is no longer supported. Use FUNCTION_BLOCK instead. |
| 99 | `ErrNoLocalEnum` | Err_NoLocalEnum | Локально заданное перечисление больше не поддерживается. Вместо этого используйте определение типа данных. | Local defined enumeration are no longer supported. Use datatype definition instead. |
| 100 | `WrnLibraryNotInstalled` | Wrn_LibraryNotInstalled | Библиотека {0} не добавлена в Менеджер библиотек, либо не найдено корректной лицензии | Library {0} has not been added to the Library Manager, or no valid license could be found |
| 101 | `ErrDataRecursion` | Err_DataRecursion | Рекурсия данных: {0} | Data recursion: {0} |
| 107 | `ErrVarTooBig` | Err_VarTooBig | Переменная '{0}' слишком велика. (Размер переменной: {1}, Размер сегмента: {2}) | The variable '{0}' is too large. (variable size: {1}, segment size: {2}) |
| 112 | `ErrAddressMisaligned` | Err_AddressMisaligned | Адрес {0} некорректен для типа данных {1} | Address {0} misaligned for datatype {1} |
| 113 | `ErrNoBitTypeOnBitAddress` | Err_NoBitTypeOnBitAddress | Тип {0} недопустим для битового адреса {1} | Type {0} is not possible on bit adress {1} |
| 114 | `ErrInvalidJumpDestination` | Err_InvalidJumpDestination | Некорректная цель {0} для JMP | Invalid destination {0} for JMP |
| 116 | `ErrDuplicateLabelDefinition` | Err_DuplicateLabelDefinition | Метка '{0}' не уникальна | The label '{0}' is a duplicate |
| 119 | `ErrWrongConstructor` | Err_WrongConstructor | — | The FB_Init method of a function block or struct needs two inputs 'bInitRetains' and 'bInCopyCode' of type BOOL |
| 120 | `ErrWrongDestructor` | Err_WrongDestructor | — | The FB_Exit method of a function block or struct must have a single input 'bInCopyCode' of type BOOL and a return value of type BOOL. |
| 122 | `ErrBaseNotAllowed` | Err_BaseNotAllowed | Выражение SUPER недопустимо в данном контексте | Expression SUPER is not allowed in this context |
| 125 | `WrnEnumValueDuplicate` | Wrn_EnumValueDuplicate | Константа {0} присвоена нескольким перечислениям | The constant {0} is assigned to more than one enumeration |
| 128 | `ErrNoInstancePath` | Err_NoInstancePath | Отсутствует VAR_CONFIG для '{0}' | No VAR_CONFIG for '{0}' |
| 129 | `ErrNoValidInstancePath` | Err_NoValidInstancePath | '{0}' не является корректным путем экземпляра | '{0}' is not a valid instance path |
| 130 | `ErrMissingParameterList` | Err_MissingParameterList | {0} '{1}' указано без круглых скобок '()' | {0} '{1}' referenced without parentheses '()' |
| 131 | `ErrWrongTypeForAdr` | Err_WrongTypeForAdr | '{0}' недопустим в качестве операнда для ADR | '{0}' is not allowed as operand for ADR |
| 132 | `ErrNoEnclosingLoopExit` | Err_NoEnclosingLoopExit | Отсутствует замкнутый цикл, на который можно применить {0} | No enclosing loop of which to {0} |
| 135 | `ErrNotSupportedInInterface` | Err_NotSupportedInInterface | Такой код недопутим в разделе объявления | This code is not supported in declaration part |
| 138 | `ErrNoMatchingInitMethodFound` | Err_NoMatchingInitMethodFound | Для создания экземпляра {0} не найдено подходящего метода 'FB_Init' | No matching 'FB_Init' method found for instantiation of {0} |
| 139 | `WrnStatementNoEffect` | Wrn_StatementNoEffect | Код '{0}' не имеет действия. Это сделано намеренно? | The code '{0}' has no effect. Is this the intent? |
| 140 | `ErrLValueNoRefType` | Err_LValueNoRefType | Присвоение ссылки возможно только для переменных типа Reference  | Reference assign is only allowed to variables of reference type |
| 142 | `ErrVariableDuplicate` | Err_VariableDuplicate | Локальная переменная с именем '{0}' уже задана в '{1}' | A local variable named '{0}' is already defined in '{1}' |
| 143 | `ErrNoReadAccessToProperty` | Err_NoReadAccessToProperty | Свойство '{0}' не может использоваться в этом контексте, поскольку отсутствует средство доступа get | The property '{0}' cannot be used in this context because it lacks the get accessor |
| 144 | `ErrInheritanceError` | Err_InheritanceError | Наследование возможно только в функциональных блоках, интерфейсах и структурах | Inheritance only allowed in function blocks, interfaces and structures |
| 145 | `ErrInterfaceImplementationError` | Err_InterfaceImplementationError | Интерфейсы могут реализовываться только функциональными блоками | Interfaces can only be implemented by function blocks |
| 146 | `ErrParamsNotAllowed` | Err_ParamsNotAllowed | Параметр PARAMS должен быть последним входным параметром функции или метода | A PARAMS parameter must be the last input parameter of a function or method |
| 149 | `ErrNoVarsinInterface` | Err_NoVarsinInterface | Объявления переменных недопустимы в интерфейсах | Variable declarations are not allowed in interfaces |
| 150 | `ErrOneAccessorRequired` | Err_OneAccessorRequired | Для свойства '{0}' требуется хотя бы одно средство доступа | At least one accessor required for property '{0}' |
| 161 | `ErrArrayBorderIsNoConstant` | Err_ArrayBorderIsNoConstant | Граница '{0}' массива не является постоянным значением | Border '{0}' of array is no constant value |
| 162 | `ErrArrayInitialisationCountNoConstant` | Err_ArrayInitialisationCountNoConstant | Число '{0}' инициализаций массива не является постоянным значением | Number '{0}' of array initialisations is no constant value |
| 163 | `ErrLowerGreaterUpperBorder` | Err_LowerGreaterUpperBorder | Нижняя граница должна быть ниже верхней границы | Lower border must be lower than upper border |
| 164 | `ErrPOUCalledFromDifferentTasks` | Err_POUCalledFromDifferentTasks | POU {0} выполняет запись в выход {1} и вызывается в нескольких задачах | POU {0} writes to output {1} and is called in several tasks |
| 165 | `ErrOutputByteWrittenFromDifferentTasks` | Err_OutputByteWrittenFromDifferentTasks | В нескольких задачах записывается один и тот же байт выходной памяти | The same byte in output memory is written in different tasks |
| 167 | `ErrVarAccessNotSupported` | Err_VarAccessNotSupported | VAR_ACCESS не поддерживается | VAR_ACCESS is not supported |
| 176 | `ErrWrongVarInInterfaceMethod` | Err_WrongVarInInterfaceMethod | В методах интрефейсов допустимы только входы, выходы и Inout-объекты | Only inputs, outputs, and inouts allowed in interface methods |
| 177 | `ErrNoInstanceObject` | Err_NoInstanceObject | '{0}' имеет тип {1} и не может иметь экземпляры | '{0}' is of type {1} and cannot be instantiated |
| 178 | `ErrInOutAccessOutside` | Err_InOutAccessOutside | Невозможен внешний доступ к параметру VAR_IN_OUT '{0}' из '{1}'." | No external access to VAR_IN_OUT parameter '{0}' of '{1}'." |
| 179 | `ErrStructInitOnlyInput` | Err_StructInitOnlyInput | '{0}'не является входом '{1}' | '{0}' is no input of '{1}' |
| 180 | `ErrLibraryConflict` | Err_LibraryConflict | Неоднозначное пространство имен '{0}' задано библиотекой '{1}' | Ambiguous namespace '{0}' defined by library '{1}' |
| 181 | `InfRelatedPosition` | Inf_RelatedPosition | Относительная позиция | Related position |
| 182 | `ErrReturnTypeForNonFunction` | Err_ReturnTypeForNonFunction | Возвращаемый тип допустим только для POU типа FUNCTION и METHOD | Return type is only possible for POUs of type FUNCTION and METHOD |
| 185 | `ErrNoCallInInstancePath` | Err_NoCallInInstancePath | Невозможно применить компонентный доступ '.', индексный доступ '[]', или вызов '()' к результату вызова функции. Сначала присвойте результат help-переменной. | It is not possible to perform component access '.', index access '[]' or call '()' on result of function call. Assign result to help variable first. |
| 186 | `ErrNoCallInInterfaceComparison` | Err_NoCallInInterfaceComparison | Невозможно выполнить сравнение интерфейса, который является возвращаемым значением вызова. Сначала выполните присвоение переменнной | It is not possible to compare interface that is return value of call. Assign to variable first. |
| 188 | `ErrDeviceNotInstalled` | Err_DeviceNotInstalled | Устройство не установлено в систему. Генерация кода невозможна. | Device not installed to the system. No code generation possible. |
| 191 | `ErrIndexOfNotSupported` | Err_IndexOfNotSupported | Оператор INDEXOF больше не поддерживается. Используйте вместо него ADR. | The operator INDEXOF is no longer supported. Use ADR instead. ADR on a POU name returns a pointer to a pointer to the function code. |
| 192 | `ErrAccessOutsideOfCall` | Err_AccessOutsideOfCall | К выходу '{0}' из '{1}' нельзя обратиться из-за пределов вызова | Input '{0}' of '{1}' cannot be accessed outside of call |
| 193 | `ErrInvalidSignatureName` | Err_InvalidSignatureName | Имя '{0}' не является корректным идентификатором | Name '{0}' is no valid identifier |
| 195 | `WrnImplicitSignedToUnsigned` | Wrn_ImplicitSignedToUnsigned | Неявная конверсия типа со знаком '{0}' в тип без знака '{1}': возможно изменение знака | Implicit conversion from signed Type '{0}' to unsigned Type '{1}' : Possible change of sign |
| 196 | `WrnImplicitUnsignedToSigned` | Wrn_ImplicitUnsignedToSigned | Неявная конверсия из типа без знака '{0}' в тип со знаком '{1}': возможно изменение знака | Implicit conversion from unsigned Type '{0}' to signed Type '{1}' : Possible change of sign |
| 197 | `WrnImplicitIntToReal` | Wrn_ImplicitIntToReal | Неявная конверсия из '{0}' в '{1}': возможна потеря информации | Implicit conversion from '{0}' to '{1}': Possible loss of information |
| 198 | `WrnStringConstantTooLong` | Wrn_StringConstantTooLong | Строковая константа '{0}' слишком велика для типа цели '{1}' | String constant '{0}' too long for destination type '{1}' |
| 199 | `ErrInterfaceNeedsInstance` | Err_InterfaceNeedsInstance | Для доступа к интерфейсу '{0}' требуется его экземпляр | Interface '{0}' must be instantiated to be accessed |
| 201 | `ErrInOutParamNotEqual` | Err_InOutParamNotEqual | Тип '{0}' не совпадает с типом '{1}' VAR_IN_OUT '{2}' | Type '{0}' is not equal to type '{1}' of VAR_IN_OUT respectively REFERENCE '{2}' |
| 202 | `ErrApplicationConflict` | Err_ApplicationConflict | Существуют два приложения с одинаковым именем '{0}' на устройстве '{1}' | There are two applications with the same name '{0}' on device '{1}' |
| 203 | `ErrBitsForStructureOnly` | Err_BitsForStructureOnly | Только структуры и функциональные блоки могу содержать переменные типа BIT. | Only structures and function blocks can contain variables of type BIT |
| 204 | `ErrBitsWrongScope` | Err_BitsWrongScope | Переменные типа BIT должны объявляться в разделе VAR_INPUT, VAR_OUTPUT или VAR-block | Variables of type BIT must be declared within a VAR_INPUT, VAR_OUTPUT, or VAR section |
| 206 | `ErrNoArrayOfBit` | Err_NoArrayOfBit | BIT недопустим в качестве базового типа массива | BIT is not allowed as base type of an array |
| 208 | `ErrModNotReal` | Err_ModNotReal | MOD не задан для REAL  | MOD is not defined for REAL |
| 209 | `WrnTooManyApplications` | Wrn_TooManyApplications | Вы задали {0} приложений дя устройства {1}. Максимальное количество - {2}. Вы не сможете загрузить все приложения. | You have defined {0} applications for device {1}. The maximum number is {2}. So you will not be able to download all applications. |
| 214 | `ErrPersistentWrongVariable` | Err_PersistentWrongVariable | Переменные в списке Persistent должны быть либо VAR_GLOBAL PERSISTENT, либо VAR_GLOBAL PERSISTENT RETAIN | Variables in persistent list must all be either VAR_GLOBAL PERSISTENT or VAR_GLOBAL PERSISTENT RETAIN |
| 215 | `ErrPersistentNoAddress` | Err_PersistentNoAddress | Прямое объявление адреса невозможно в Persistent-списке | Direct address declaration is not possible in persistent list |
| 220 | `WrnDeviceNotInstalledForSimulation` | Wrn_DeviceNotInstalledForSimulation | Устройство не устанолвено в систему. При эмуляции будут использованы настройки по умолчанию. | Device not installed to the system. Simulation uses default settings. |
| 221 | `ErrMalformedAddress` | Err_MalformedAddress | Прямой адрес '{0}' поврежден | Direct address '{0}' malformed |
| 224 | `ErrCallRecursion` | Err_CallRecursion | Рекурсия вызова: {0} | Call recursion: {0} |
| 228 | `WrnNoInitialValueForConstant` | Wrn_NoInitialValueForConstant | Отсутствует начальное значение для константной переменной '{0}' | No initial value for constant variable '{0}' |
| 229 | `ErrBlobInitError` | Err_BlobInitError | Вместо '{0}' требуется константное значение | Constant value expected instead of '{0}' |
| 234 | `ErrQueryInterfaceErrorP1` | Err_QueryInterfaceErrorP1 | Первый операнд __QueryInterface должен быть указателем интерфейса или экземпляром функционального блока | First operand of __QueryInterface must be an interface reference or the instance of a function block |
| 235 | `ErrQueryInterfaceErrorP2` | Err_QueryInterfaceErrorP2 | Второй операнд __QueryInterface должен быть указателем интерфейса | Second operand of __QueryInterface must be an interface reference |
| 236 | `ErrWrongTypeForExternal` | Err_WrongTypeForExternal | Неверное определение типа для VAR_EXTERNAL {0} | Wrong type definition for VAR_EXTERNAL {0} |
| 237 | `ErrNoVarForExternal` | Err_NoVarForExternal | Не найдено глобального определения для VAR_EXTERNAL {0} | No global definition found for VAR_EXTERNAL {0} |
| 238 | `ErrNoInitialForExternal` | Err_NoInitialForExternal | Для VAR_EXTERNAL {0} начальное значение недопустимо | No initial value allowed for VAR_EXTERNAL {0} |
| 239 | `ErrQueryInterfaceP1NoIQuery` | Err_QueryInterfaceP1NoIQuery | Интерфейс {0} не является расширением для {1} | Interface {0} does not extend {1} |
| 244 | `ErrMissingInstancePathForPersistent` | Err_MissingInstancePathForPersistent | — | No matching instance path in VAR_PERSISTENT list found for variable {0}. Use the command "Add all instance paths" to add all instance paths to the VAR_PERSISTENT list. (See Help for details) |
| 247 | `ErrNoPropertiesInOutAssignment` | Err_NoPropertiesInOutAssignment | Свойство не может быть задано в выходном присваивании вызова. Используйте временную переменную. | A property cannot be assigned in an output assignment of a call. Use a temporary variable instead. |
| 248 | `ErrNewNeedsType` | Err_NewNeedsType | Определение типа требуется в качестве операнда для __NEW | Type definition expected as operand for __NEW |
| 249 | `ErrNewArrayOnUserdefNotAllowed` | Err_NewArrayOnUserdefNotAllowed | Создание массива с помощью __NEW невозможно для пользовательских типов | Creating an array with __NEW is not allowed on user defined types |
| 250 | `ErrNewPositionNotOK` | Err_NewPositionNotOK | Должен быть присвоен результат __NEW | The result of __NEW must be assigned |
| 266 | `WrnLoopExitConditionConstant` | Wrn_LoopExitConditionConstant | Условие выхода цикла '{0} {1} {2}' является константой FALSE. Возможен бесконечный цикл. | Loop exit condition '{0} {1} {2}' is constant FALSE. Possible endless loop. |
| 268 | `ErrUninitialisedVariableUsedInInitialisation` | Err_UninitialisedVariableUsedInInitialisation | Неинициализированная переменная {0} используется для инициализации {1}. Чтобы изменить порядок инициализации, используйте атрибут 'global_init_slot'. | The uninitialized variable {0} is used for initialization of {1}. Use the attribute 'global_init_slot' to change the order of initialisation. |
| 273 | `ErrImplicitMethodNameForVariable` | Err_ImplicitMethodNameForVariable | {0} - это имя неявно сгенерированного метода, и оно не подходит для переменных в функциональном блоке | {0} is the name of an implicitly generated method and illegal for variables in a function block |
| 274 | `ErrDeviceNameNoIdent` | Err_DeviceNameNoIdent | Имя устройства '{0}' некорректно. Требуется корректный МЭК-идентификатор | Device name '{0}' invalid. Must be a valid IEC 61131-3 identifier |
| 275 | `ErrNoVarTempInSubsequentPrograms` | Err_NoVarTempInSubsequentPrograms | Раздел VAR_TEMP недопустим в программах с атрибутом 'subsequent' | VAR_TEMP is not possible in programs with attribute 'subsequent' |
| 278 | `ErrInvalidInitialisationForArray` | Err_InvalidInitialisationForArray | {0} не является корректной инициализацией для константы {1}. Начальное значение константы должно быть постоянным. | {0} is an invalid initialisation for constant {1}. Initial values of constants must be constant. |
| 279 | `ErrSummarizedLibraryErrors` | Err_SummarizedLibraryErrors | {0} внутренних ошибок в библиотеке {1} | {0} internal errors in library {1} |
| 285 | `ErrNoOverrideOnInternalMethod` | Err_NoOverrideOnInternalMethod | — | Function block '{0}': No override possible on method {1}.{2} in library {3} with access specifier INTERNAL |
| 286 | `ErrNoChangeOnAccessModifier` | Err_NoChangeOnAccessModifier | Метод {0}.{1} не может изменять модификатор доступа "{2}" при перезаписи метода {3}.{1} | Method {0}.{1} cannot change access modifier "{2}" when overriding method {3}.{1} |
| 291 | `ErrAccessToInternalVariable` | Err_AccessToInternalVariable | Невозможно получить доступ к внутренней переменной {0} библиотеки {1} | Cannot access internal variable {0} of library {1} |
| 292 | `ErrAccessToInternalObject` | Err_AccessToInternalObject | Не удается получить доступ к внутреннему объекту {0} библиотеки {1} | Cannot access internal object {0} of library {1} |
| 293 | `ErrAccessToInternalProperty` | Err_AccessToInternalProperty | Обращение к внутреннему свойству {0} библиотеки {1} невозможно | Cannot access internal property {0} of library {1} |
| 294 | `ErrInOutAssignedInActionCall` | Err_InOutAssignedInActionCall | VAR_IN_OUT {0} нельзя присваивать в локальном вызове действия '{1}' | VAR_IN_OUT {0} can't be assigned in local action call '{1}' |
| 295 | `ErrSlotFunctionHiddenByVariable` | Err_SlotFunctionHiddenByVariable | POU {0} с атрибутом слота {1} не может быть вызван, поскольку он скрыт переменной '{2}' | POU {0} with slot attribute {1} not callable, because it is hidden by variable '{2}' |
| 296 | `ErrSlotFunctionAmbiguousName` | Err_SlotFunctionAmbiguousName | POU {0} с атрибутом слота {1} имеет неоднозначное имя | POU {0} with slot attribute {1} has an ambiguous name |
| 299 | `ErrNoCodegenerator` | Err_NoCodegenerator | Не удалось создать генератор кода: нужный плагин не установлен | Creating a code generator failed: The required plug-in is not installed |
| 300 | `ErrRelatedPositionTaskX` | Err_RelatedPositionTaskX | — | Related position: Access in task {0} |
| 301 | `ErrPersistentVariablesChangeMessageText` | Err_PersistentVariablesChangeMessageText | Persistent-переменные изменились. Загрузка отменена пользователем. | Persistent variables changed. Download cancelled by user. |
| 302 | `ErrCancelledByUser` | Err_CancelledByUser | Компиляция отменена пользователем | Compilation cancelled by user |
| 303 | `ErrStructureInitialisationNotPossible` | Err_StructureInitialisationNotPossible | Инициализация структуры недопустима в качестве параметра вызова Init-функции. Используйте переменную. | A structure initialisation is not possible as Parameter of an Init-function call. Use a variable instead. |
| 304 | `ErrArrayInitialisationNotPossible` | Err_ArrayInitialisationNotPossible | Инициализация массива недопустима в качестве параметра вызова Init-функции. Используйте переменную | An array initialisation is not possible as parameter of an initial function call. Use a variable instead |
| 306 | `ErrStructuredValueTypeInExternalCall` | Err_StructuredValueTypeInExternalCall | Структурированный тип '{0}' недопустим во внешнем вызове функции | Structured type '{0}' not allowed in external function call |
| 307 | `ErrRecursiveConstantInitialisation` | Err_RecursiveConstantInitialisation | Рекурсивное определение константного значения. | Recursive definition of constant value |
| 308 | `WrnShiftExceedsTypeSize` | Wrn_ShiftExceedsTypeSize | Сдвиг на {0} превышает размер типа {1} | Shift by {0} exceeds type size of {1} |
| 310 | `ErrLValueForAnyVar` | Err_LValueForAnyVar | Параметр ANY '{0}' из '{1}' требует переменную с доступом записи в качестве входа | ANY parameter '{0}' of '{1}' needs variable with write access as input |
| 311 | `ErrAnyTypeOnlyInFunction` | Err_AnyTypeOnlyInFunction | Переменные типа '{0}' допустимы только в качестве входов функции. | Variables of type '{0}' only allowed as input of functions |
| 312 | `WrnConcurrentAccessOfBitInSameByte` | Wrn_ConcurrentAccessOfBitInSameByte | Одновременный доступ к биту '{0}' в одном и том же байте | Concurrent access of bit '{0}' in same byte |
| 313 | `ErrNoInitialForInoutConstant` | Err_NoInitialForInoutConstant | Для VAR_INOUT CONSTANT {0} начальное значение недопустимо | No initial value allowed for VAR_INOUT CONSTANT {0} |
| 314 | `ErrVariableForVarinoutConstant` | Err_VariableForVarinoutConstant | Параметр VAR_IN_OUT CONSTANT '{0}' из '{1}' требует переменной в качестве входа | VAR_IN_OUT CONSTANT parameter '{0}' of '{1}' needs variable as input |
| 316 | `WrnMethodAlreadyCalledImplicitly` | Wrn_MethodAlreadyCalledImplicitly | Метод '{0}' уже вызван неявно | Method '{0}' already called implicitly |
| 318 | `ErrExprNoConstantError` | Err_ExprNoConstantError | Вместо '{0}' требуется константное значение | Constant value expected instead of '{0}' |
| 319 | `ErrNotAllowedInInterfaceLib` | Err_NotAllowedInInterfaceLib | '{0}' недопустим в интерфейсной библиотеке | '{0}' not allowed in interface library |
| 320 | `ErrNotAllowedInContainerLib` | Err_NotAllowedInContainerLib | '{0}' недопустимо в контейнерной библиотеке | '{0}' not allowed in container library |
| 321 | `ErrInterfaceMethodImplementationNotPublic` | Err_InterfaceMethodImplementationNotPublic | Реализация интерфейсного метода '{0}' функционального блока '{1}' должна быть PUBLIC | Implementation of interface method '{0}' of function block '{1}' must be PUBLIC |
| 322 | `ErrNoVarConfigInMethodOrFunction` | Err_NoVarConfigInMethodOrFunction | Неполные адреса недопустимы в методах и функциях | Incomplete adresses are not allowed in methods and functions |
| 323 | `ErrCantHaveBaseClass` | Err_CantHaveBaseClass | Ключевое слово EXTENDS не применимо к типу {0} | Keyword EXTENDS not applicable to type {0} |
| 324 | `ErrNoIndirectPropertyCallOnStringWithSize` | Err_NoIndirectPropertyCallOnStringWithSize | Косвенный вызов свойства с типом {0} невозможен, поскольку поддержваются только типы без явно заданной длины | Indirect call of property with type {0} not possible because only types without explicitly specified length are supported |
| 325 | `WrnGlobalInitSlotNotForVariables` | Wrn_GlobalInitSlotNotForVariables | Атрибут 'global_init_slot' всегда относится ко всему списку переменных и не должен применяться к отдельным переменным. | The attribute 'global_init_slot' always affects the whole variable list, and should not be applied on single variables |
| 326 | `ErrLibraryWithUnicodeIdentifiers` | Err_LibraryWithUnicodeIdentifiers | Библиотека '{0}' была сохранена с поддержкой идентификаторов unicode и может быть, таким образом, использована только при поддержке идентификаторов unicode (см. опции компиляции). | The library '{0}' was saved with support of unicode identifiers, and can only be used if the project also supports unicode identifiers (see compile options). |
| 327 | `WrnImplicitEnumConversion` | Wrn_ImplicitEnumConversion | Неявная конверсия из одного типа перечисления ({0}) в другой ({1}) | Implicit conversion from one enumeration type ({0}) to another ({1}) |
| 328 | `ErrNoAssign` | Err_NoAssign | Присваивание недоступно для типа {0} | Assignment not allowed for type {0} |
| 329 | `ErrDuplicateVarConfig` | Err_DuplicateVarConfig | Копировать VAR_CONFIG для '{0}' | Duplicate VAR_CONFIG for '{0}' |
| 332 | `ErrMappedVarWrittenInDiffTasks` | Err_MappedVarWrittenInDiffTasks | Переменная {0}, соотнесенная с адресом {1}, записывается в разных задачах | Variable {0}, which is mapped on address {1} is written in different tasks |
| 333 | `ErrOpTakesAtMostInputs` | Err_OpTakesAtMostInputs | Для '{0}' требуется не более '{1}' операндов | '{0}' takes at most '{1}' operands |
| 334 | `ErrLibNamespaceConflict` | Err_LibNamespaceConflict | Локальное пространство имен '{0}' библиотеки '{1}' скрывает пространство имен библиотеки '{2}' | Local namespace '{0}' of library '{1}' hides namespace of library '{2}' |
| 336 | `ErrBitAccessOnlyOnInt` | Err_BitAccessOnlyOnInt | Битовый доступ возможен только для целочисленных типов. | Biaccess is only possible on integer types |
| 338 | `ErrLibSupports32BitOnly` | Err_LibSupports32BitOnly | Библиотека '{0}' поддерживается только 32-битными приложениями | The Library '{0}' is only supported in 32 bit applications |
| 339 | `WrnBoolNotAtBitAddress` | Wrn_BoolNotAtBitAddress | Для типа данных BOOL требуется битовый адрес | Bit address expected for data type BOOL |
| 340 | `ErrCheckLicenseNeedsSysTarget` | Err_CheckLicenseNeedsSysTarget | Для оператора __CHECKLICENSE требуются библиотеки SysTarget и 3S License | The 3S License library is required for the __CHECKLICENSE operator |
| 344 | `WrnStruturedTypePropertyNotMonitorable` | Wrn_StruturedTypePropertyNotMonitorable | Атрибут monitoring игнорируется для свойства '{0}', возвращающего структурированный тип | The monitoring attribute is not supported for property '{0}' and will be ignored |
| 346 | `ErrNoDirectAddressInSubsequentVarDecl` | Err_NoDirectAddressInSubsequentVarDecl | Объявление переменной '{0}' с присваиванием адреса не может быть использовано в PRG или GVL с атрибутом 'subsequent' | The declaration of the variable '{0}' with an address assignment cannot be used in a PRG or GVL with the attribute 'subsequent' |
| 348 | `ErrBitAdrOnOperation` | Err_BitAdrOnOperation | Оператор 'BITADR' возможен только для переменных (не для oперация) | Operator BITADR is only possible on variables, not on operations |
| 352 | `ErrMaxArraySizeExceeded` | Err_MaxArraySizeExceeded | Достигнут максимальный размер массива. Либо увеличьте нижнюю границу, либо уменьшите верхнюю. | Maximum array size exceeded. Either increase the lower border or decrease the upper border. |
| 354 | `WrnEnumComparison` | Wrn_EnumComparison | Сравнение одного типа перечисления ({0}) с другим ({1}) | Comparison of one enumeration type ({0}) with another ({1}) |
| 357 | `WrnObsolete` | Wrn_Obsolete | POU '{0}' отмечен как устаревший: {1} | POU '{0}' has been marked as obsolete: {1} |
| 358 | `ErrStrictEnumNotAMember` | Err_StrictEnumNotAMember | '{0}' - неподходящее значение для типа ENUM '{1}' | '{0}' is not a valid value for strict ENUM type '{1}' |
| 359 | `ErrStrictEnumNoArithmeticAllowed` | Err_StrictEnumNoArithmeticAllowed | Арифметические действия недопустимы для строгих перечислений '{0}' | Arithmetics not allowed on strict ENUM type '{0}' |
| 360 | `ErrParameterlistNotConst` | Err_ParameterlistNotConst | Список параметров должен быть задан как константа | A parameter list must be declared as constant |
| 361 | `ErrNoMixExternalIECInheritance` | Err_NoMixExternalIECInheritance | — | An extending POU must be implemented the same way as its base (externally or in IEC) |
| 363 | `ErrUserCheckFunctionsNotSupported` | Err_UserCheckFunctionsNotSupported | Функции, заданные пользователем, не поддерживаются | User defined check functions are not supported |
| 364 | `ErrCallAfterInitHasInputs` | Err_CallAfterInitHasInputs | Метод с отметкой '{0}' не может иметь входы. | A method marked with '{0}' cannot have any inputs |
| 365 | `ErrGeneratingVarInitializations` | Err_GeneratingVarInitializations | Некорректные нач. значения переменных | One or more inital values for variables are invalid |
| 368 | `ErrInvalidEnumDefaultValue` | Err_InvalidEnumDefaultValue | — | Only local enumeration members can be used as default initialization values for an enumeration |
| 371 | `WrnNonLocalAccessToVarInOut` | Wrn_NonLocalAccessToVarInOut | Обращение к VAR_IN_OUT '{0}', объявленной в '{1}', из внешнего контекста '{2}'. | Access to VAR_IN_OUT '{0}' declared in '{1}' from external context '{2}' |
| 374 | `ErrDivisionByZero` | Err_DivisionByZero | — | Division by zero |
| 375 | `ErrInvalidEnumBaseType` | Err_InvalidEnumBaseType | — | Only integer types are supported as an enum base type |
| 376 | `ErrTooFewParametersForExtensibleFunction` | Err_TooFewParametersForExtensibleFunction | — | Extensible function {0} needs at least {1} inputs for parameter {2} |
| 377 | `ErrTooManyParametersForExtensibleFunction` | Err_TooManyParametersForExtensibleFunction | — | Extensible function {0} accepts at most {1} inputs for parameter {2} |
| 378 | `ErrNoFormalParamsCallsForExtensibleFunction` | Err_NoFormalParamsCallsForExtensibleFunction | — | No explicit input assignments possible for call of extensible function {0} |
| 380 | `ErrLowerUpperBoundOnVariableLengthArrayOnly` | Err_LowerUpperBoundOnVariableLengthArrayOnly | — | The operators LOWER_BOUND and UPPER_BOUND are only supported for arrays |
| 384 | `ErrSelMuxOnlyEqualUserDefTypes` | Err_SelMuxOnlyEqualUserDefTypes | — | All user defined types used in SEL or MUX must be equal |
| 385 | `ErrVarLengthArrayInOut` | Err_VarLengthArrayInOut | — | Variable length arrays are only possible as VAR_IN_OUT of function blocks or as VAR_IN_OUT and VAR_INPUT of methods and functions |
| 386 | `ErrVarLengthArrayTopLevel` | Err_VarLengthArrayTopLevel | — | A variable length array type has to be on top level position of a type declaration |
| 387 | `ErrOutParamNotEqual` | Err_OutParamNotEqual | — | Type '{0}' is not equal to type '{1}' of VAR_OUTPUT '{2}' |
| 388 | `WrnLibWithStringInVarInOut` | Wrn_LibWithStringInVarInOut | — | Consider declaring VAR_IN_OUT string variable '{0}' in '{1}' as VAR_IN_OUT CONSTANT to allow callers to pass literals or constant. |
| 389 | `WrnLValueForVarinoutStrings` | Wrn_LValueForVarinoutStrings | Для параметра VAR_IN_OUT '{0}' из '{1}' в качестве входа требуется переменная с доступом записи. В последующих версиях это будет ошибкой компиляции! | VAR_IN_OUT respectively REFERENCE parameter '{0}' of '{1}' needs variable with write access as input |
| 390 | `ErrAddressSourceIsAddressDest` | Err_AddressSourceIsAddressDest | — | Variable '{0}' was located on address '{1}'. At this address variable '{2}' is now located. This change is not possible with online change. |
| 391 | `ErrNoCopyCodeAllowed` | Err_NoCopyCodeAllowed | Перемещение этого POU при онлайн-замене невозможно | Moving this POU to a new location during online changes is not allowed |
| 393 | `ErrArrayBorderNoValidSignedInteger` | Err_ArrayBorderNoValidSignedInteger | — | Array border {0} does not evaluate to a valid signed integer constant |
| 395 | `ErrNumOfInitializersDoNotMatch` | Err_NumOfInitializersDoNotMatch | — | The number of 'FB_Init' initializers ({0}) does not match the number of array elements ({1}) |
| 397 | `ErrImplicitMethodImplementationNotPublic` | Err_ImplicitMethodImplementationNotPublic | — | Function block '{0}': The access to the methods 'FB_Init', 'FB_Exit', and 'FB_ReInit' must be PUBLIC |
| 400 | `ErrImplicitRefTypeIsnotAllowedAsBase` | Err_ImplicitRefTypeIsnotAllowedAsBase | — | Implicit reference types cannot be a base type of references, pointers, and arrays |
| 401 | `ErrImplicitRefTypeAllClassesNeedAttrib` | Err_ImplicitRefTypeAllClassesNeedAttrib | — | The inheriting functionblock differs in its usage of the attribute '{0}' |
| 403 | `ErrImplicitRefTypeSignNotSupported` | Err_ImplicitRefTypeSignNotSupported | — | Only function blocks and structures can be marked as an implicit reference type |
| 405 | `ErrMultipleAssignmentsToInterfaceVariables` | Err_MultipleAssignmentsToInterfaceVariables | — | Multiple assignments to interface variables not allowed |
| 406 | `WrnImplicitCheckFunctionShadowed` | Wrn_ImplicitCheckFunctionShadowed | — | The implicit check function '{0}' is hidden by another variable or function. Checks will not be performed! Resolve the conflict and clean the application to use the check function. |
| 407 | `ErrAddressOfNonInstanceVar` | Err_AddressOfNonInstanceVar | — | Instance required instead of type name in address-of expression '{0}' |
| 409 | `ErrNoResolutionForLazyVariable` | Err_NoResolutionForLazyVariable | — | Type of lazy typed variable '{0}' could not be resolved |
| 411 | `ErrNoVarInputInPropertyAccessors` | Err_NoVarInputInPropertyAccessors | — | It is not allowed to define input variables in property accessors: {0} : {1} |
| 412 | `ErrMultipleAssignsToSameInputInCall` | Err_MultipleAssignsToSameInputInCall | — | Multiple input assignments for parameter '{0}' |
| 416 | `ErrLibraryNamespaceNotValid` | Err_LibraryNamespaceNotValid | — | Namespace {0} of library {1} is no valid identifier |
| 417 | `ErrLValueForVarinoutStrings` | Err_LValueForVarinoutStrings | VAR_IN_OUT-параметр '{0}' из '{1}' требует переменной с доступом записи в качестве входа | VAR_IN_OUT respectively REFERENCE parameter '{0}' of '{1}' needs variable with write access as input |
| 418 | `ErrStringTooShortForVarInOut` | Err_StringTooShortForVarInOut | Строковая переменная '{0}' слишком коротка для VAR_IN_OUT-параметра '{1}' из '{2}' | String variable '{0}' too short for the VAR_IN_OUT parameter '{1}' of '{2}' |
| 421 | `WrnExtendsForInterfaces` | Wrn_ExtendsForInterfaces | — | Use keyword EXTENDS for inheritance of interfaces instead of IMPLEMENTS |
| 422 | `WrnGranularityMismatchForDirectVariable` | Wrn_GranularityMismatchForDirectVariable | — | Variable '{0}' has a granularity of {1} but is located at direct address {2} which is not aligned to {1} bytes |
| 424 | `ErrMultipleAssignmentWithChangingValueTypes` | Err_MultipleAssignmentWithChangingValueTypes | — | Multiple assignments of value types with implicit casts not allowed |
| 436 | `ErrStringLengthIsNoConstant` | Err_StringLengthIsNoConstant | — | String length '{0}' is no constant value |
| 437 | `ErrNotAllowedInTaskLocalVariables` | Err_NotAllowedInTaskLocalVariables | — | Task local variable list '{0}': '{1}' is not allowed |
| 438 | `ErrTaskLocalVariablesWriterTaskNotDefined` | Err_TaskLocalVariablesWriterTaskNotDefined | — | Task local variable list '{0}': Writer task not defined |
| 439 | `ErrTaskLocalVariablesAccessNotAllowed` | Err_TaskLocalVariablesAccessNotAllowed | — | Task local variable list '{0}': Write access only allowed in task '{1}' |
| 441 | `WrnVarInOutUnitializedInInitialValue` | Wrn_VarInOutUnitializedInInitialValue | — | Access to uninitialized VAR_IN_OUT variable |
| 447 | `WrnOnlyConstantInitialValueForMappedPersistentVar` | Wrn_OnlyConstantInitialValueForMappedPersistentVar | — | Only replaced constants can be applied as initial value for a mapped persistent variable |
| 451 | `ErrVersionOverflow` | Err_VersionOverflow | — | At least one part of the version '{0}' has a too big value |
| 452 | `ErrVersionPartNegative` | Err_VersionPartNegative | — | At least one part of the version '{0}' has a negative value |
| 453 | `ErrVersionInvalidFormat` | Err_VersionInvalidFormat | — | Token '{0}' is no valid version |
| 455 | `ErrExplicitTransitionAssignMissing` | Err_ExplicitTransitionAssignMissing | — | The code of transition contains multiple statements. The explicit assignment to transition output variable is required. |
| 456 | `WrnAmbiguousCheckfunctionInLibrary` | Wrn_AmbiguousCheckfunctionInLibrary | — | The implicit check function {0} is hidden by another implicit check function {1}. Checks will not be performed! Resolve the conflict and clean the application to use the check function. |
| 506 | `ErrLowerUpperBoundOperandNotInRange` | Err_LowerUpperBoundOperandNotInRange | — | Value of 2nd operand of the {0} operator must be between 1 and {1} in this case |
| 507 | `ErrLowerUpperBoundOperandNotExactly` | Err_LowerUpperBoundOperandNotExactly | — | Value of 2nd operand of the {0} operator must be 1 in this case |
| 509 | `ErrMultipleAssignmentsNotAllowedForOperator` | Err_MultipleAssignmentsNotAllowedForOperator | — | Multiple assignments are not allowed for operator '{0}'. |
| 514 | `WrnAccessToInternalProperty` | Wrn_AccessToInternalProperty | — | Should not access internal property {0} of library {1} |
| 516 | `WrnAccessToInternalVariable` | Wrn_AccessToInternalVariable | — | Should not access internal variable {0} of library {1} |
| 517 | `WrnAccessToInternalObject` | Wrn_AccessToInternalObject | — | Should not access internal object {0} of library {1} |
| 518 | `ErrNoNamespace` | Err_NoNamespace | — | '{0}' does not refer to a namespace |
| 519 | `ErrInvalidAccessPathForNamespaceAccess` | Err_InvalidAccessPathForNamespaceAccess | — | '{0}' is no valid access path |
| 520 | `ErrInvalidNamespaceForNamespaceAccess` | Err_InvalidNamespaceForNamespaceAccess | — | '{0}' is no valid namespace access |
| 521 | `ErrUnknownCompilerVersionInCompiledLib` | Err_UnknownCompilerVersionInCompiledLib | — | The compiler version "{1}" with which the compiled library "{0}" was created is newer than the compiler version of the project or unknown. |
| 524 | `ErrWrongReInit` | Err_WrongReInit | — | The FB_ReInit method of a function block or struct must have no inputs and a return value of type BOOL. The FB_ReInit will not be called automatically! |
| 525 | `WrnInvalidDefaultValue` | Wrn_InvalidDefaultValue | — | The type {0} cannot have a default value in this context |
| 526 | `WrnDefaultValueNotConstant` | Wrn_DefaultValueNotConstant | — | Default value is not constant |
| 527 | `WrnDefaultValueTopLevel` | Wrn_DefaultValueTopLevel | — | The input is only optional when this function is called from IEC code |
| 531 | `ErrMultipleAssignmentWithProperty` | Err_MultipleAssignmentWithProperty | — | Properties cannot be used in the middle of multiple assignments. |
| 532 | `ErrFunNeedsAtLeastNInputs` | Err_FunNeedsAtLeastNInputs | — | Function '{0}' requires at least '{1}' and maximum '{2}' inputs |
| 534 | `ErrWrongCallAfterGlobalInitSlotSignature` | Err_WrongCallAfterGlobalInitSlotSignature | — | A signature decorated with the 'call_after_global_init_slot' attribute must either have no arguments or a single input named 'bInitRetains'. |
| 535 | `InfUseXSizeOfOperator` | Inf_UseXSizeOfOperator | — | Use  the XSIZEOF operator for large types |
| 537 | `ErrNoPropertyForVarInout` | Err_NoPropertyForVarInout | — | Properties can't be assigned to VAR_IN_OUT. |
| 538 | `ErrInterfaceChangedNumberOfInputsOutputsDifferent` | Err_InterfaceChanged_NumberOfInputsOutputsDifferent | — | The number of inputs/outputs of the method '{0}' does not correspond to the interface '{1}'. |
| 539 | `ErrInterfaceChangedVariableDifferent` | Err_InterfaceChanged_VariableDifferent | — | The variable '{0}' of the method '{1}' does not correspond to the interface '{2}'. |
| 542 | `WrnNoInheritanceForUnions` | Wrn_NoInheritanceForUnions | — | Inheritance is not intended for data type "UNION": {0} |
| 543 | `WrnReservedUnusedKeyword` | Wrn_ReservedUnusedKeyword | — | The name '{0}' is a reserved keyword in the IEC61131-3 standard. An error will be reported in future versions. |
| 554 | `ErrNoExplicitCall` | Err_NoExplicitCall | — | No explicit calls for '{0}' allowed. {1}.  |
| 555 | `WrnNonAsciiStringLiteral` | Wrn_NonAsciiStringLiteral | — | The string literal '{0}...' contains non-representable characters. The project option 'UTF-8 Encoding for STRING' could be used. |
| 557 | `ErrFCallWrongNumberOfArguments` | Err_FCallWrongNumberOfArguments | — | FCall expects exactly 2+{0} arguments. |
| 558 | `ErrFCallWrongCallPatternSignature` | Err_FCallWrongCallPatternSignature | — | The first argument of FCall must be a signature type that is used as pattern for the call. |
| 560 | `ErrNewOnInterfaceNotPossible` | Err_NewOnInterfaceNotPossible | — | __NEW is not possible on interfaces |
| 561 | `WrnCallRecursion` | Wrn_CallRecursion | — | Call recursion: {0} |
| 562 | `ErrPartialAccessOnlyOnBitTypes` | Err_PartialAccess_OnlyOnBitTypes | — | Partial access is only supported on ANY_BIT types and not on '{0}' |
| 563 | `ErrPartialAccessNotAValidComponent` | Err_PartialAccess_NotAValidComponent | — | '%{1}{2}' is not a valid partial component of '{0}' |
| 565 | `WrnWrongDestructor` | Wrn_WrongDestructor | — | The FB_Exit method of a function block or struct must have a single input 'bInCopyCode' of type BOOL and a return value of type BOOL. |
| 566 | `WrnWrongReInit` | Wrn_WrongReInit | — | The FB_ReInit method of a function block or struct must have no inputs and a return value of type BOOL. The FB_ReInit will not be called automatically! |
| 567 | `WrnNotAllowedInInterfaceLib` | Wrn_NotAllowedInInterfaceLib | — | '{0}' not allowed in interface library |
| 568 | `WrnInterfaceChangedBase` | Wrn_InterfaceChangedBase | — | Interface of overridden method '{0}' of base '{1}' doesn't match declaration |
| 569 | `WrnMissingInstancePathForPersistent` | Wrn_MissingInstancePathForPersistent | — | No matching instance path in VAR_PERSISTENT list found for variable {0}. Use the command "Add all instance paths" to add all instance paths to the VAR_PERSISTENT list. (See Help for details) |
| 572 | `WrnUninitialisedVariableUsedInInitialisation` | Wrn_UninitialisedVariableUsedInInitialisation | — | The uninitialized variable {0} is used for initialization of {1}. Use the attribute 'global_init_slot' to change the order of initialisation. |
| 577 | `ErrNoCopyCodeForVariableAtDirectAddress` | Err_NoCopyCodeForVariableAtDirectAddress | — | a variable at a direct address cannot change its type during online change |
| 584 | `ErrMaxNestingDepthExceeded` | Err_MaxNestingDepthExceeded | — | Maximum nesting depth exceeded.  |
| 589 | `ErrUnknownEmbeddedLanguageType` | Err_UnknownEmbeddedLanguageType | — | Unknown embedded language type '{0}'. |
| 590 | `ErrNoResolutionForSomeLazyVariables` | Err_NoResolutionForSomeLazyVariables | — | Type of lazy variables in this signature could not be inferred. |
| 591 | `WrnChangeOfAccessModifier` | Wrn_ChangeOfAccessModifier | — | Method {0}.{1} changes access modifier "{2}" when overriding method {3}.{1} |

## (d) Соответствие «Rust enum variant ↔ MessageId ↔ RU/EN»

Предлагаемое имя варианта: префикс (`Err_`→`Err`, `Wrn_`→`Wrn`, `Inf_`→`Inf`, `Txt_`→`Txt`) сохраняется для различения одноимённых suffiх, остальные части — PascalCase. Все 507 значений.

| Rust variant | MessageId | Key | RU | EN |
|---|---|---|---|---|
| `None` | 0 | None | — | — |
| `ErrConstantOverflow` | 1 | Err_ConstantOverflow | Константа '{0}' слишком велика для типа '{1}' | Constant '{0}' too large for type '{1}' |
| `ErrOperator1of2Expected` | 2 | Err_Operator1of2Expected | '{0}' или '{1}' требуется вместо '{2}' | '{0}' or '{1}' expected instead of '{2}' |
| `ErrBitNrOverflow` | 3 | Err_BitNrOverflow | '{0}' не является корректным битовым номером для '{1}' | '{0}' is no valid bit number for '{1}' |
| `ErrNoComponentOf` | 4 | Err_NoComponentOf | '{0}' не является компонентом '{1}' | '{0}' is no component of '{1}' |
| `ErrOverflowInAddress` | 5 | Err_OverflowInAddress | Постоянное переполнение по адресу '{0}' | Constant overflow in address '{0}' |
| `ErrOperatorExpected` | 6 | Err_OperatorExpected | '{0}' требуется вместо '{1}' | '{0}' expected instead of '{1}' |
| `ErrExpressionExpectedInstead` | 7 | Err_ExpressionExpectedInstead | Вместо '{0}' требуется выражение | Expression expected instead of '{0}' |
| `ErrOperator1of3ExpectedInsteadofEOF` | 8 | Err_Operator1of3ExpectedInsteadofEOF | Недопустимый End-of-file: требуется '{0}', '{1}' или '{2}' | Unexpected End-of-file found: '{0}', '{1}' or '{2}' expected |
| `ErrUnexpectedTokenFound` | 9 | Err_UnexpectedTokenFound | Обнаружен недопустимый символ '{0}' | Unexpected token '{0}' found |
| `ErrOperatorExpectedInsteadofEOF` | 10 | Err_OperatorExpectedInsteadofEOF | Недопустимый End-of-file: требуется '{0}' | Unexpected End-of-file found: '{0}' expected |
| `ErrNoCaseLabelFound` | 11 | Err_NoCaseLabelFound | Метка не найдена | No CASE label found |
| `ErrNoValidCondition` | 12 | Err_NoValidCondition | '{0}' не является корректным условием | '{0}' is no valid condition |
| `ErrAtLeastOneExpected` | 13 | Err_AtLeastOneExpected | Требуется не менее одного оператора | At least one statement is expected |
| `ErrConditionExpected` | 14 | Err_ConditionExpected | Требуется условие | Condition expected |
| `ErrCounterStartExpected` | 15 | Err_CounterStartExpected | Требуется начальное значение счетчика | Counter initialisation expected |
| `ErrUpperBoundExpected` | 16 | Err_UpperBoundExpected | Требуется верхняя граница для FOR-цикла | Upper bound for FOR loop expected |
| `ErrInvalidLoopIncrement` | 17 | Err_InvalidLoopIncrement | Некорректный инкремент цикла | Loop increment invalid |
| `ErrIsNoLValue` | 18 | Err_IsNoLValue | '{0}' не является корректным объектом присваивания | '{0}' is no valid assignment target |
| `ErrRValueRequired` | 19 | Err_RValueRequired | Присваиваие не имеет корректного источника | Assignment has no valid source |
| `ErrNoValidStatement` | 20 | Err_NoValidStatement | '{0}' не является корректным заявлением | '{0}' is no valid statement |
| `ErrNoValidOperand` | 21 | Err_NoValidOperand | '{0}' не является корректным операндом | '{0}' is no valid operand |
| `ErrOpNeedsExactInputs` | 22 | Err_OpNeedsExactInputs | Для '{0}' требуется ровно '{1}' операндов | '{0}' needs exactly '{1}' operands |
| `ErrOpNeedsAtLeastInputs` | 23 | Err_OpNeedsAtLeastInputs | Для '{0}' необходимо как минимум '{1}' операндов | '{0}' needs at least '{1}' operands |
| `ErrIllegalOperator` | 24 | Err_IllegalOperator | '{0}' не является корректным ST-оператором | '{0}' is no valid ST operator |
| `ErrOperator1of4Expected` | 25 | Err_Operator1of4Expected | '{0}', '{1}',  '{2}' или '{3}' требуется вместо '{4}' | '{0}', '{1}',  '{2}' or '{3}' expected instead of '{4}' |
| `ErrIdentifierExpected` | 26 | Err_IdentifierExpected | Вместо '{0}' требуется идентификатор | Identifier expected instead of '{0}' |
| `ErrStringSizeExpected` | 27 | Err_StringSizeExpected | После "(" требуется размер строки | size of string expected after "(" |
| `ErrOperator1of3Expected` | 28 | Err_Operator1of3Expected | '{0}', '{1}' или '{2}' требуется вместо '{3}' | '{0}', '{1}' or '{2}' expected instead of '{3}' |
| `ErrAddressExpected` | 30 | Err_AddressExpected | После "AT" вместо {0} требуется прямой адрес | Direct address expected after AT instead of {0} |
| `ErrTypeExpected` | 31 | Err_TypeExpected | Вместо '{0}' требуется определение типа | Type definition expected instead of '{0}' |
| `ErrTypeMismatch` | 32 | Err_TypeMismatch | Невозможно конвертировать тип '{0}' в тип '{1}' | Cannot convert type '{0}' to type '{1}' |
| `WrnPointerMisatch` | 33 | Wrn_PointerMisatch | Тип '{0}', вероятно, не конвертируется в тип '{1}' | Type '{0}' possibly not convertible to type '{1}' |
| `ErrMultiassigninfor` | 34 | Err_Multiassigninfor | Для цикла требуется ровно один счетчик | Exactly one counter expected in FOR loop |
| `ErrCalleeInvalidType` | 35 | Err_CalleeInvalidType | Вместо '{0}' требуется имя программы, функция или экземпляр функционального блока | Program name, function or function block instance expected instead of '{0}' |
| `ErrWrongObjectType` | 36 | Err_WrongObjectType | Невозможно вызвать объект типа '{0}' | Cannot call object of type '{0}' |
| `ErrIsNoInput` | 37 | Err_IsNoInput | '{0}'не является входом '{1}' | '{0}' is no input of '{1}' |
| `ErrIsNoOutput` | 38 | Err_IsNoOutput | '{0}' не является выходом '{1}' | '{0}' is no output of '{1}' |
| `ErrInOutNotAssigned` | 39 | Err_InOutNotAssigned | VAR_IN_OUT '{0}' должен присваиваться в вызове '{1}' | VAR_IN_OUT '{0}' must be assigned in call of '{1}' |
| `ErrFunNeedsNInputs` | 40 | Err_FunNeedsNInputs | Для функции '{0}' требуется ровно '{1}' входов | Function '{0}' requires exactly '{1}' inputs |
| `ErrLValueForVarinout` | 41 | Err_LValueForVarinout | VAR_IN_OUT-параметр '{0}' из '{1}' требует переменной с доступом записи в качестве входа | VAR_IN_OUT respectively REFERENCE parameter '{0}' of '{1}' needs variable with write access as input |
| `ErrFunctionCallMixedStyle` | 42 | Err_FunctionCallMixedStyle | В вызове функции должны быть отмечены либо все, либо никакие формальные параметры | Either all or none formal parameter have to be denoted in function call |
| `ErrWrongFormalParameter` | 43 | Err_WrongFormalParameter | Неверный формальный параметр: '{0}' требуется здесь | Wrong formal parameter: '{0}' expected in this place |
| `ErrInputMissing` | 44 | Err_InputMissing | Присваивание для входа отсутствует для параметра '{0}' в вызове '{1}' | Assignment to input missing for parameter '{0}' in call of '{1}' |
| `ErrThisNotAllowed` | 45 | Err_ThisNotAllowed | Выражение THIS недопустимо в этом контексте | Expression THIS is not allowed in this context |
| `ErrIdentNotDefined` | 46 | Err_IdentNotDefined | Идентификатор '{0}' не задан | Identifier '{0}' not defined |
| `ErrIndexingInvalid` | 47 | Err_IndexingInvalid | Невозможно применить индексацию с [] к выражению типа '{0}' | Cannot apply indexing with [] to an expression of type '{0}' |
| `ErrArrayIndexNumWrong` | 48 | Err_ArrayIndexNumWrong | Для массива требуется ровно {0} индексов | Array requires exactly {0} indexes |
| `ErrConstantIndexOutOfRange` | 49 | Err_ConstantIndexOutOfRange | Константый индекс '{0}' находится вне диапазона от '{1}' до '{2}' | The constant index '{0}' is not within the range from '{1}' to '{2}' |
| `ErrBitaccessnoconst` | 50 | Err_Bitaccessnoconst | Для битового доступа требуется литерал или символьная целочисленная константа | Bit access requires literal or symbolic integer constant |
| `ErrAttributeNameExpected` | 51 | Err_AttributeNameExpected | Для значения атрибута вместо '{0}' требуется однобайтовая строка | Single byte string expected for an attribute value instead of '{0}' |
| `TxtParentContextNotUpToDate` | 52 | Txt_ParentContextNotUpToDate | "Контекст неактуален". | Parent context not up to date |
| `ErrCompilerVersionError` | 53 | Err_CompilerVersionError | Версия компилятора {0} отозвана. Используйте более новую версию. | Compiler version {0} has been withdrawn. Please use a higher compiler version instead. |
| `ErrNoBitAccessOnFunctionCall` | 61 | Err_NoBitAccessOnFunctionCall | Битовый доступ к вызову функции невозможен | Bit access on function call is not allowed |
| `ErrCompoAccessNoStruct` | 62 | Err_CompoAccessNoStruct | '{0}' не является структурированной переменной | '{0}' is no structured variable |
| `ErrContainsNoDefinition` | 63 | Err_ContainsNoDefinition | '{0}' не содержит определения для '{1}' | '{0}' contains no definition for '{1}' |
| `ErrDerefNoPointer` | 64 | Err_DerefNoPointer | Для разыменования требуется указатель | Dereference requires a pointer |
| `ErrNoGlobalDefine` | 65 | Err_NoGlobalDefine | Отсутствует глобальне определение для '{0}' | There is no global definition for '{0}' |
| `ErrTypesNotComparable` | 66 | Err_TypesNotComparable | Невозможно сравнить тип '{0}' с типом '{1}' | Cannot compare type '{0}' with type '{1}' |
| `ErrCompareNotPossible1` | 68 | Err_CompareNotPossible1 | Сравнение невозможно для объектов типа '{0}' | Compare not possible on objects of type '{0}' |
| `ErrCompareNotPossible2` | 69 | Err_CompareNotPossible2 | Сравнение невозможно для объектов типа '{0}' или '{1}' | Compare not possible on objects of type '{0}' or '{1}' |
| `ErrIniNeedsUserdefType` | 70 | Err_IniNeedsUserdefType | Для оператора INI необходим экземпляр функционального блока или DUT-экземпляр | INI operator needs function block instance or data unit type instance |
| `ErrCannotAddMultipleTime` | 71 | Err_CannotAddMultipleTime | Невозможно сложить несколько операндов типа '{0}' | Cannot add multiple operands of type '{0}' |
| `ErrOperationNotPossibleOnType` | 72 | Err_OperationNotPossibleOnType | Операция '{0}' невозможна над типом '{1}' | Operation '{0}' is not possible on type '{1}' |
| `ErrCannotMultiplyMultipleTime` | 73 | Err_CannotMultiplyMultipleTime | Нельзя перемножить несколько операндов типа '{0}' | Cannot multiply multiple operands of type '{0}' |
| `ErrUnexpectedArrayInitialisation` | 74 | Err_UnexpectedArrayInitialisation | Недопустимая инициализация массива | Unexpected array initialisation |
| `ErrTooManyInitializer` | 75 | Err_TooManyInitializer | Слишком много инициализаторов для массива | Too many initializers for array |
| `ErrUnexpectedStructureInitialisation` | 76 | Err_UnexpectedStructureInitialisation | Недопустимая инициализация структуры | Unexpected structure initialisation |
| `ErrUnknownType` | 77 | Err_UnknownType | Неизвестный тип: '{0}' | Unknown type: '{0}' |
| `ErrUnsupportedType` | 78 | Err_UnsupportedType | Неподдерживаемый тип: '{0}' | Unsupported type: '{0}' |
| `ErrFunctionBlockNeedsInstance` | 80 | Err_FunctionBlockNeedsInstance | Функциональный блок '{0}' должен иметь экземпляр | Function block '{0}' must be instantiated to be accessed |
| `ErrUnexpectedPragmaif` | 81 | Err_UnexpectedPragmaif | Недопустимая директива: '{0}' обнаружено без соответствующего 'if' | Unexpected pragma: '{0}' found without matching 'if' |
| `ErrNoValidConditionforPragma` | 82 | Err_NoValidConditionforPragma | '{0}' не является подходящим условием для директивы | '{0}' is no valid condition for pragma |
| `ErrUnexpectedOperandForIndexOf` | 83 | Err_UnexpectedOperandForIndexOf | Недопустимый операнд '{0}' в '{1}' | Unexpected operand '{0}' found in '{1}' |
| `ErrNoValidOperandforPragma` | 84 | Err_NoValidOperandforPragma | '{0}' не является подходящим операндом для директивы | '{0}' is no valid operand for pragma |
| `ErrDefineValueExpected` | 85 | Err_DefineValueExpected | Значение требуется вместо '{0}' | Define value expected instead of '{0}' |
| `ErrInterfaceNotFound` | 86 | Err_InterfaceNotFound | Не найдено определения для интерфейса '{0}' | No definition found for interface '{0}' |
| `ErrNoMethodImplementation` | 87 | Err_NoMethodImplementation | Отсутствует реализация для метода '{0}', заданного в интерфейсе '{1}' | There is no implementation for method '{0}' defined in interface '{1}' |
| `ErrSomethingOverridingMethod` | 88 | Err_SomethingOverridingMethod | {0} '{1}' перезаписывает метод интерфейса '{2}' | {0} '{1}' overrides method of interface '{2}' |
| `ErrInterfaceChanged` | 89 | Err_InterfaceChanged | Интерфейс перезаписанного метода '{0}' интерфейса '{1}' не соответствует объявлению | Interface of overridden method '{0}' of interface '{1}' doesn't match declaration |
| `ErrBaseClassNotFound` | 90 | Err_BaseClassNotFound | Для базового класса '{0}' отсутствует определение | No definition found for base class '{0}' |
| `ErrSelfInheritanceBase` | 91 | Err_SelfInheritanceBase | Рекурсия в списке базовых функциональных блоков: {0} | Recursion in base function block list: {0} |
| `ErrOnlyMethodsOverride` | 92 | Err_OnlyMethodsOverride | Невозможно заменить {0} {1} из {2}: только методы допустимы для перезаписи | Not possible to override {0} {1} of {2}: Only methods allowed to override |
| `ErrSomethingOverridingMethodBase` | 93 | Err_SomethingOverridingMethodBase | — | {0} '{1}' overrides method of base '{2}' |
| `ErrInterfaceChangedBase` | 94 | Err_InterfaceChangedBase | Интерфейс перезаписанного метода '{0}' основы '{1}' не соответствует объявлению | Interface of overridden method '{0}' of base '{1}' doesn't match declaration |
| `ErrSelfInheritance` | 95 | Err_SelfInheritance | Рекурсия в интерфейсах: {0} | Recursion in interfaces: {0} |
| `ErrNoMultipleInheritance` | 96 | Err_NoMultipleInheritance | В списке EXTENDS (расширение) можно задать только один функциональный блок | Only one base function block may be defined in EXTENDS list |
| `ErrVariableOverride` | 97 | Err_VariableOverride | Повторяющееся определение переменной '{0}' в функциональном блоке '{1}' и в основе '{2}' | Duplicate definition of variable '{0}' in function block '{1}' and in base '{2}' |
| `ErrFunctionBlockNoLongerValid` | 98 | Err_FunctionBlockNoLongerValid | Ключевое слово FUNCTIONBLOCK больше не поддерживается. Используйте FUNCTION_BLOCK | The keyword FUNCTIONBLOCK is no longer supported. Use FUNCTION_BLOCK instead. |
| `ErrNoLocalEnum` | 99 | Err_NoLocalEnum | Локально заданное перечисление больше не поддерживается. Вместо этого используйте определение типа данных. | Local defined enumeration are no longer supported. Use datatype definition instead. |
| `WrnLibraryNotInstalled` | 100 | Wrn_LibraryNotInstalled | Библиотека {0} не добавлена в Менеджер библиотек, либо не найдено корректной лицензии | Library {0} has not been added to the Library Manager, or no valid license could be found |
| `ErrDataRecursion` | 101 | Err_DataRecursion | Рекурсия данных: {0} | Data recursion: {0} |
| `ErrOutOfRetainMemory` | 102 | Err_OutOfRetainMemory | Недостаточно энергонезависимой памяти: Переменная '{0}', {1} байт (Инкрементная компиляция может привести к фрагментации памяти. Выполните команду "Компиляция, очистка" для принудительного распределения всех данных и кода.) | Out of retain memory: Variable '{0}', {1} bytes  (Incremental compilation may produce fragmented memory. Perform "Build, Clean" to force a reallocation of all data and code.) |
| `ErrOutOfRetainMemoryWithWholeSize` | 103 | Err_OutOfRetainMemoryWithWholeSize | Недостаточно энергонезависимой памяти: Переменная '{0}', {1} байт (Наибольший промежуток в памяти - {2})  (Инкрементная компиляция может привести к фрагментации памяти. Выполните команду "Компилировать, Очистить" для принудительного перераспределения данных и кода.) | Out of retain memory: Variable '{0}', {1} bytes (largest contiguous memory gap {2})  (Incremental compilation may produce fragmented memory. Perform "Build, Clean" to force a reallocation of all data and code.) |
| `ErrOutOfMemory` | 104 | Err_OutOfMemory | Недостаточно памяти глобольных данных: Переменная '{0}', {1} байт. (Инкрементная компиляция может привести к фрагментации памяти. Выполните команду "Компилировать, Очистить" для принудительного перераспределения данных и кода.) | Out of global data memory: Variable '{0}', {1} bytes. (Incremental compilation may produce fragmented memory. Perform "Build, Clean" to force a reallocation of all data and code.) |
| `ErrOutOfMemoryFunctionPointer` | 105 | Err_OutOfMemoryFunctionPointer | Недостаточно памяти глобальных данных: указатель функции для '{0}' не может быть выделен | Out of global data memory: Function pointer for function '{0}' could not be allocated |
| `ErrOutOfMemoryWithWholeSize` | 106 | Err_OutOfMemoryWithWholeSize | Недостаточно памяти глобальных данных: переменная '{0}', {1} байт (Наибольший промежуток в памяти - {2})  (Инкрементная компиляция или добавление переменных в persistent-список может привести к фрагментации памяти. Выполните команду "Объявления, переупорядочить список и очистить промежутки" или "Компилировать, Очистить" для принудительного перераспределения прочих данных и кода.) | Out of global data memory: Variable '{0}', {1} bytes (Largest contiguous memory gap {2}). Incremental compilation or adding variables to persistent variable lists may produce fragmented memory. Perform "Declarations, Reorder list and clear gaps" to compact persistent variable lists or "Build, Clean" to force a reallocation of other data and code. |
| `ErrVarTooBig` | 107 | Err_VarTooBig | Переменная '{0}' слишком велика. (Размер переменной: {1}, Размер сегмента: {2}) | The variable '{0}' is too large. (variable size: {1}, segment size: {2}) |
| `ErrNoInputMemory` | 108 | Err_NoInputMemory | Нет зарезервированной входной памяти | There is no input memory reserved |
| `ErrNoOutputMemory` | 109 | Err_NoOutputMemory | Выходной памяти не зарезервировано | There is no output memory reserved |
| `ErrNoMemoryMemory` | 110 | Err_NoMemoryMemory | Нет зарезервированной памяти | There is no memory reserved |
| `ErrAddressOutOfRange` | 111 | Err_AddressOutOfRange | Адрес '{0}' вне диапазона: Pассчитанный сдвиг: {1}, выделенный размер: {2} | Address '{0}' out of range: Calculated offset: {1} bytes, allocated size: {2} bytes |
| `ErrAddressMisaligned` | 112 | Err_AddressMisaligned | Адрес {0} некорректен для типа данных {1} | Address {0} misaligned for datatype {1} |
| `ErrNoBitTypeOnBitAddress` | 113 | Err_NoBitTypeOnBitAddress | Тип {0} недопустим для битового адреса {1} | Type {0} is not possible on bit adress {1} |
| `ErrInvalidJumpDestination` | 114 | Err_InvalidJumpDestination | Некорректная цель {0} для JMP | Invalid destination {0} for JMP |
| `ErrCalcNeedsCall` | 115 | Err_CalcNeedsCall | Второй параметр условного вызова должен соответствующим оператором вызова | Second parameter of conditional call must be a valid call statement |
| `ErrDuplicateLabelDefinition` | 116 | Err_DuplicateLabelDefinition | Метка '{0}' не уникальна | The label '{0}' is a duplicate |
| `ErrNoSuchLabel` | 117 | Err_NoSuchLabel | Отсутствует метка '{0}' в области выражения JMP. | No such label '{0}' within the scope of the JMP statement |
| `WrnLabelNoReference` | 118 | Wrn_LabelNoReference | Метка '{0}' не используется | The label '{0}' has not been referenced |
| `ErrWrongConstructor` | 119 | Err_WrongConstructor | — | The FB_Init method of a function block or struct needs two inputs 'bInitRetains' and 'bInCopyCode' of type BOOL |
| `ErrWrongDestructor` | 120 | Err_WrongDestructor | — | The FB_Exit method of a function block or struct must have a single input 'bInCopyCode' of type BOOL and a return value of type BOOL. |
| `ErrBaseNotAllowed` | 122 | Err_BaseNotAllowed | Выражение SUPER недопустимо в данном контексте | Expression SUPER is not allowed in this context |
| `ErrNoValidEnumInit` | 124 | Err_NoValidEnumInit | {0} не является подходящим значением для перечисления | {0} is no valid initialisation for an enumeration |
| `WrnEnumValueDuplicate` | 125 | Wrn_EnumValueDuplicate | Константа {0} присвоена нескольким перечислениям | The constant {0} is assigned to more than one enumeration |
| `ErrIndexNumWrong` | 126 | Err_IndexNumWrong | Переменная '{0}' требует хотя бы 1 индекса | Variable of type '{0}' requires exactly 1 Index |
| `ErrOutOfCodeMemory` | 127 | Err_OutOfCodeMemory | Недостаточно кодовой памяти: POU '{0}', {1} байт. (Инкрементная компиляция может привести к фрагментации памяти. Выполните команду "Компилировать, Очистить" для принудительного перераспределения данных и кода.) | Out of code memory: POU '{0}', {1} bytes. (Incremental compilation may produce fragmented memory. Perform "Build, Clean" to force a reallocation of all data and code.) |
| `ErrNoInstancePath` | 128 | Err_NoInstancePath | Отсутствует VAR_CONFIG для '{0}' | No VAR_CONFIG for '{0}' |
| `ErrNoValidInstancePath` | 129 | Err_NoValidInstancePath | '{0}' не является корректным путем экземпляра | '{0}' is not a valid instance path |
| `ErrMissingParameterList` | 130 | Err_MissingParameterList | {0} '{1}' указано без круглых скобок '()' | {0} '{1}' referenced without parentheses '()' |
| `ErrWrongTypeForAdr` | 131 | Err_WrongTypeForAdr | '{0}' недопустим в качестве операнда для ADR | '{0}' is not allowed as operand for ADR |
| `ErrNoEnclosingLoopExit` | 132 | Err_NoEnclosingLoopExit | Отсутствует замкнутый цикл, на который можно применить {0} | No enclosing loop of which to {0} |
| `ErrNotSupportedInInterface` | 135 | Err_NotSupportedInInterface | Такой код недопутим в разделе объявления | This code is not supported in declaration part |
| `ErrAmbiguity` | 136 | Err_Ambiguity | неоднозначное использование имени '{0}'  | Ambiguous use of name '{0}' |
| `ErrNoMatchingInitMethodFound` | 138 | Err_NoMatchingInitMethodFound | Для создания экземпляра {0} не найдено подходящего метода 'FB_Init' | No matching 'FB_Init' method found for instantiation of {0} |
| `WrnStatementNoEffect` | 139 | Wrn_StatementNoEffect | Код '{0}' не имеет действия. Это сделано намеренно? | The code '{0}' has no effect. Is this the intent? |
| `ErrLValueNoRefType` | 140 | Err_LValueNoRefType | Присвоение ссылки возможно только для переменных типа Reference  | Reference assign is only allowed to variables of reference type |
| `ErrLValueForReference` | 141 | Err_LValueForReference | Требуется переменная с доступом для записи | Reference assign needs variable with write access |
| `ErrVariableDuplicate` | 142 | Err_VariableDuplicate | Локальная переменная с именем '{0}' уже задана в '{1}' | A local variable named '{0}' is already defined in '{1}' |
| `ErrNoReadAccessToProperty` | 143 | Err_NoReadAccessToProperty | Свойство '{0}' не может использоваться в этом контексте, поскольку отсутствует средство доступа get | The property '{0}' cannot be used in this context because it lacks the get accessor |
| `ErrInheritanceError` | 144 | Err_InheritanceError | Наследование возможно только в функциональных блоках, интерфейсах и структурах | Inheritance only allowed in function blocks, interfaces and structures |
| `ErrInterfaceImplementationError` | 145 | Err_InterfaceImplementationError | Интерфейсы могут реализовываться только функциональными блоками | Interfaces can only be implemented by function blocks |
| `ErrParamsNotAllowed` | 146 | Err_ParamsNotAllowed | Параметр PARAMS должен быть последним входным параметром функции или метода | A PARAMS parameter must be the last input parameter of a function or method |
| `ErrNoVarsinInterface` | 149 | Err_NoVarsinInterface | Объявления переменных недопустимы в интерфейсах | Variable declarations are not allowed in interfaces |
| `ErrOneAccessorRequired` | 150 | Err_OneAccessorRequired | Для свойства '{0}' требуется хотя бы одно средство доступа | At least one accessor required for property '{0}' |
| `ErrArrayBorderIsNoConstant` | 161 | Err_ArrayBorderIsNoConstant | Граница '{0}' массива не является постоянным значением | Border '{0}' of array is no constant value |
| `ErrArrayInitialisationCountNoConstant` | 162 | Err_ArrayInitialisationCountNoConstant | Число '{0}' инициализаций массива не является постоянным значением | Number '{0}' of array initialisations is no constant value |
| `ErrLowerGreaterUpperBorder` | 163 | Err_LowerGreaterUpperBorder | Нижняя граница должна быть ниже верхней границы | Lower border must be lower than upper border |
| `ErrPOUCalledFromDifferentTasks` | 164 | Err_POUCalledFromDifferentTasks | POU {0} выполняет запись в выход {1} и вызывается в нескольких задачах | POU {0} writes to output {1} and is called in several tasks |
| `ErrOutputByteWrittenFromDifferentTasks` | 165 | Err_OutputByteWrittenFromDifferentTasks | В нескольких задачах записывается один и тот же байт выходной памяти | The same byte in output memory is written in different tasks |
| `ErrVarAccessNotSupported` | 167 | Err_VarAccessNotSupported | VAR_ACCESS не поддерживается | VAR_ACCESS is not supported |
| `ErrVarConfigNotAllowed` | 168 | Err_VarConfigNotAllowed | Объявление VAR_CONFIG допустимо только в списке VAR_CONFIG | VAR_CONFIG declaration only allowed in VAR_CONFIG  list |
| `ErrVarGlobalNotAllowed` | 169 | Err_VarGlobalNotAllowed | Объявление в 'VAR_GLOBAL' допустимо только в списке глобальных переменных | VAR_GLOBAL declaration only allowed in global variable list |
| `ErrStructureNotAllowed` | 170 | Err_StructureNotAllowed | Объявление 'STRUCT' допустимо только в DUT | STRUCT declaration only allowed in data unit type |
| `ErrUnionNotAllowed` | 171 | Err_UnionNotAllowed | Объявление UNION допустимо только в DUT | UNION declaration only allowed in data unit type |
| `ErrStaticNotAllowed` | 172 | Err_StaticNotAllowed | Объявление 'VAR_STAT' здесь недопустимо | VAR_STAT declaration not allowed in this place |
| `ErrDeclarationKeywordNotAllowed` | 173 | Err_DeclarationKeywordNotAllowed | '{0}' здесь недопустимо | '{0}' not allowed in this place |
| `ErrVarTempNotAllowed` | 174 | Err_VarTempNotAllowed | Объявление 'VAR_TEMP' здесь недопустимо | VAR_TEMP declaration not allowed in this place |
| `ErrRetainOrPersistentNotAllowed` | 175 | Err_RetainOrPersistentNotAllowed | 'RETAIN' и 'PERSISTENT' здесь недопустимо | RETAIN or PERSISTENT not allowed in this place |
| `ErrWrongVarInInterfaceMethod` | 176 | Err_WrongVarInInterfaceMethod | В методах интрефейсов допустимы только входы, выходы и Inout-объекты | Only inputs, outputs, and inouts allowed in interface methods |
| `ErrNoInstanceObject` | 177 | Err_NoInstanceObject | '{0}' имеет тип {1} и не может иметь экземпляры | '{0}' is of type {1} and cannot be instantiated |
| `ErrInOutAccessOutside` | 178 | Err_InOutAccessOutside | Невозможен внешний доступ к параметру VAR_IN_OUT '{0}' из '{1}'." | No external access to VAR_IN_OUT parameter '{0}' of '{1}'." |
| `ErrStructInitOnlyInput` | 179 | Err_StructInitOnlyInput | '{0}'не является входом '{1}' | '{0}' is no input of '{1}' |
| `ErrLibraryConflict` | 180 | Err_LibraryConflict | Неоднозначное пространство имен '{0}' задано библиотекой '{1}' | Ambiguous namespace '{0}' defined by library '{1}' |
| `InfRelatedPosition` | 181 | Inf_RelatedPosition | Относительная позиция | Related position |
| `ErrReturnTypeForNonFunction` | 182 | Err_ReturnTypeForNonFunction | Возвращаемый тип допустим только для POU типа FUNCTION и METHOD | Return type is only possible for POUs of type FUNCTION and METHOD |
| `ErrInvalidBaseForGlobalScopeExpression` | 183 | Err_InvalidBaseForGlobalScopeExpression | Операция глобальной области '.' некорректна над выражением '{0}' | Global scope operation '.' is not valid on expression '{0}' |
| `ErrNoOnlineChangePossible` | 184 | Err_NoOnlineChangePossible | Онлайн-замена невозможна, выполните полную загрузку | No online change possible. Perform full download |
| `ErrNoCallInInstancePath` | 185 | Err_NoCallInInstancePath | Невозможно применить компонентный доступ '.', индексный доступ '[]', или вызов '()' к результату вызова функции. Сначала присвойте результат help-переменной. | It is not possible to perform component access '.', index access '[]' or call '()' on result of function call. Assign result to help variable first. |
| `ErrNoCallInInterfaceComparison` | 186 | Err_NoCallInInterfaceComparison | Невозможно выполнить сравнение интерфейса, который является возвращаемым значением вызова. Сначала выполните присвоение переменнной | It is not possible to compare interface that is return value of call. Assign to variable first. |
| `WrnExternalReferenceIgnored` | 187 | Wrn_ExternalReferenceIgnored | Внешние ссылки допустимы только для функциональных блоков, методов и функций. Внешняя ссылка для {0} '{1}' проигнорирована. | External references are only possible for function blocks, methods, functions and constant global variable lists. External reference for {0} '{1}' is ignored. |
| `ErrDeviceNotInstalled` | 188 | Err_DeviceNotInstalled | Устройство не установлено в систему. Генерация кода невозможна. | Device not installed to the system. No code generation possible. |
| `ErrSemicolonExpected` | 189 | Err_SemicolonExpected | ';' требуется вместо '{0}' | ';' expected instead of '{0}' |
| `ErrSemicolonExpectedInsteadOfEnd` | 190 | Err_SemicolonExpectedInsteadOfEnd | Вместо конца POU требуется ';' | ';' expected instead of end of POU |
| `ErrIndexOfNotSupported` | 191 | Err_IndexOfNotSupported | Оператор INDEXOF больше не поддерживается. Используйте вместо него ADR. | The operator INDEXOF is no longer supported. Use ADR instead. ADR on a POU name returns a pointer to a pointer to the function code. |
| `ErrAccessOutsideOfCall` | 192 | Err_AccessOutsideOfCall | К выходу '{0}' из '{1}' нельзя обратиться из-за пределов вызова | Input '{0}' of '{1}' cannot be accessed outside of call |
| `ErrInvalidSignatureName` | 193 | Err_InvalidSignatureName | Имя '{0}' не является корректным идентификатором | Name '{0}' is no valid identifier |
| `ErrCaseLabelOutsideOfCase` | 194 | Err_CaseLabelOutsideOfCase | Метка case не является частью выражения case | CASE label is not part of a CASE statement |
| `WrnImplicitSignedToUnsigned` | 195 | Wrn_ImplicitSignedToUnsigned | Неявная конверсия типа со знаком '{0}' в тип без знака '{1}': возможно изменение знака | Implicit conversion from signed Type '{0}' to unsigned Type '{1}' : Possible change of sign |
| `WrnImplicitUnsignedToSigned` | 196 | Wrn_ImplicitUnsignedToSigned | Неявная конверсия из типа без знака '{0}' в тип со знаком '{1}': возможно изменение знака | Implicit conversion from unsigned Type '{0}' to signed Type '{1}' : Possible change of sign |
| `WrnImplicitIntToReal` | 197 | Wrn_ImplicitIntToReal | Неявная конверсия из '{0}' в '{1}': возможна потеря информации | Implicit conversion from '{0}' to '{1}': Possible loss of information |
| `WrnStringConstantTooLong` | 198 | Wrn_StringConstantTooLong | Строковая константа '{0}' слишком велика для типа цели '{1}' | String constant '{0}' too long for destination type '{1}' |
| `ErrInterfaceNeedsInstance` | 199 | Err_InterfaceNeedsInstance | Для доступа к интерфейсу '{0}' требуется его экземпляр | Interface '{0}' must be instantiated to be accessed |
| `WrnPlaceholderNotResolved` | 200 | Wrn_PlaceholderNotResolved | — | — |
| `ErrInOutParamNotEqual` | 201 | Err_InOutParamNotEqual | Тип '{0}' не совпадает с типом '{1}' VAR_IN_OUT '{2}' | Type '{0}' is not equal to type '{1}' of VAR_IN_OUT respectively REFERENCE '{2}' |
| `ErrApplicationConflict` | 202 | Err_ApplicationConflict | Существуют два приложения с одинаковым именем '{0}' на устройстве '{1}' | There are two applications with the same name '{0}' on device '{1}' |
| `ErrBitsForStructureOnly` | 203 | Err_BitsForStructureOnly | Только структуры и функциональные блоки могу содержать переменные типа BIT. | Only structures and function blocks can contain variables of type BIT |
| `ErrBitsWrongScope` | 204 | Err_BitsWrongScope | Переменные типа BIT должны объявляться в разделе VAR_INPUT, VAR_OUTPUT или VAR-block | Variables of type BIT must be declared within a VAR_INPUT, VAR_OUTPUT, or VAR section |
| `ErrNoPointerToBit` | 205 | Err_NoPointerToBit | POINTER TO BIT недопустим | POINTER TO BIT is not allowed |
| `ErrNoArrayOfBit` | 206 | Err_NoArrayOfBit | BIT недопустим в качестве базового типа массива | BIT is not allowed as base type of an array |
| `ErrNoSystemDefine` | 207 | Err_NoSystemDefine | Отсутствует системное определение для '{0}' | There is no system definition for '{0}' |
| `ErrModNotReal` | 208 | Err_ModNotReal | MOD не задан для REAL  | MOD is not defined for REAL |
| `WrnTooManyApplications` | 209 | Wrn_TooManyApplications | Вы задали {0} приложений дя устройства {1}. Максимальное количество - {2}. Вы не сможете загрузить все приложения. | You have defined {0} applications for device {1}. The maximum number is {2}. So you will not be able to download all applications. |
| `WrnInsertSpecialPersistent` | 210 | Wrn_InsertSpecialPersistent | — | — |
| `ErrVariableDeclarationExpected` | 211 | Err_VariableDeclarationExpected | Вместо {0} требуется объявление переменной | Variable declaration expected instead of {0} |
| `ErrVariableListExpected` | 212 | Err_VariableListExpected | VAR, VAR_INPUT, VAR_OUTPUT или VAR_INOUT требуется вместо {0} | VAR, VAR_INPUT, VAR_OUTPUT or VAR_IN_OUT expected instead of {0} |
| `ErrGlobalVariableListExpected` | 213 | Err_GlobalVariableListExpected | VAR_GLOBAL или VAR_CONFIG требуется вместо {0} | VAR_GLOBAL or VAR_CONFIG expected instead of {0} |
| `ErrPersistentWrongVariable` | 214 | Err_PersistentWrongVariable | Переменные в списке Persistent должны быть либо VAR_GLOBAL PERSISTENT, либо VAR_GLOBAL PERSISTENT RETAIN | Variables in persistent list must all be either VAR_GLOBAL PERSISTENT or VAR_GLOBAL PERSISTENT RETAIN |
| `ErrPersistentNoAddress` | 215 | Err_PersistentNoAddress | Прямое объявление адреса невозможно в Persistent-списке | Direct address declaration is not possible in persistent list |
| `ErrCaseLabelDuplicate` | 216 | Err_CaseLabelDuplicate | Повторяющаяся метка case | CASE label duplicate |
| `ErrCaseLabelInCaseRange` | 217 | Err_CaseLabelInCaseRange | Метка Case {0} также содержится в диапазоне {1} .. {2} | CASE label {0} also contained in range {1} .. {2} |
| `ErrCaseLabelNoConstant` | 218 | Err_CaseLabelNoConstant | Для метки сase требуется литерал или символьная целочисленная константа | CASE label requires literal or symbolic integer constant |
| `ErrCaseRangesOverlapping` | 219 | Err_CaseRangesOverlapping | Блок содержит пересекающиеся диапазоны {0} .. {1} и {2} .. {3} | CASE contains overlapping range {0} .. {1} and {2} .. {3} |
| `WrnDeviceNotInstalledForSimulation` | 220 | Wrn_DeviceNotInstalledForSimulation | Устройство не устанолвено в систему. При эмуляции будут использованы настройки по умолчанию. | Device not installed to the system. Simulation uses default settings. |
| `ErrMalformedAddress` | 221 | Err_MalformedAddress | Прямой адрес '{0}' поврежден | Direct address '{0}' malformed |
| `ErrNoReferenceToOutput` | 222 | Err_NoReferenceToOutput | Выходы не могут быть типа REFERENCE TO | Outputs can't be of type REFERENCE TO |
| `WrnCompoRefAssignCompatibilityWarning` | 223 | Wrn_CompoRefAssignCompatibilityWarning | — | — |
| `ErrCallRecursion` | 224 | Err_CallRecursion | Рекурсия вызова: {0} | Call recursion: {0} |
| `ErrNotAnInstanceOf` | 225 | Err_NotAnInstanceOf | '{0}' не является экземпляром '{1}' | '{0}' is not an instance of '{1}' |
| `ErrNotEnoughMemoryForVariableInit` | 226 | Err_NotEnoughMemoryForVariableInit | Недостаточно памяти для инициализации переменной '{0}'. Требуется {1} байт | Not enough memory for initialisation of variable '{0}'. {1} bytes needed |
| `ErrNoConstantInitialisationForValue` | 227 | Err_NoConstantInitialisationForValue | Начальное значение константной переменной '{0}' не является константой | Initialisation of constant variable '{0}' not constant |
| `WrnNoInitialValueForConstant` | 228 | Wrn_NoInitialValueForConstant | Отсутствует начальное значение для константной переменной '{0}' | No initial value for constant variable '{0}' |
| `ErrBlobInitError` | 229 | Err_BlobInitError | Вместо '{0}' требуется константное значение | Constant value expected instead of '{0}' |
| `ErrUnexpectedTypeName` | 230 | Err_UnexpectedTypeName | Имя типа '{0}' здесь неуместно | Type name '{0}' not expected in this place |
| `ErrSpecialTypeExpected` | 231 | Err_SpecialTypeExpected | Здесь требуется выражение типа '{0}' | Expression of type '{0}' expected in this place |
| `ErrArrayInitializationExpected` | 232 | Err_ArrayInitializationExpected | Требуется инициализация массива | Array initialisation expected |
| `ErrStructureInitializationExpected` | 233 | Err_StructureInitializationExpected | Для {0} требуется список инициализации | Initialisation list for {0} expected |
| `ErrQueryInterfaceErrorP1` | 234 | Err_QueryInterfaceErrorP1 | Первый операнд __QueryInterface должен быть указателем интерфейса или экземпляром функционального блока | First operand of __QueryInterface must be an interface reference or the instance of a function block |
| `ErrQueryInterfaceErrorP2` | 235 | Err_QueryInterfaceErrorP2 | Второй операнд __QueryInterface должен быть указателем интерфейса | Second operand of __QueryInterface must be an interface reference |
| `ErrWrongTypeForExternal` | 236 | Err_WrongTypeForExternal | Неверное определение типа для VAR_EXTERNAL {0} | Wrong type definition for VAR_EXTERNAL {0} |
| `ErrNoVarForExternal` | 237 | Err_NoVarForExternal | Не найдено глобального определения для VAR_EXTERNAL {0} | No global definition found for VAR_EXTERNAL {0} |
| `ErrNoInitialForExternal` | 238 | Err_NoInitialForExternal | Для VAR_EXTERNAL {0} начальное значение недопустимо | No initial value allowed for VAR_EXTERNAL {0} |
| `ErrQueryInterfaceP1NoIQuery` | 239 | Err_QueryInterfaceP1NoIQuery | Интерфейс {0} не является расширением для {1} | Interface {0} does not extend {1} |
| `ErrQueryPointerErrorP1` | 240 | Err_QueryPointerErrorP1 | Первый операнд __QueryPointer должен быть ссылкой интерфейса или экземпляром функционального блока | First operand of __QueryPointer must be an interface reference or the instance of a function block |
| `ErrQueryPointerErrorP2` | 241 | Err_QueryPointerErrorP2 | Второй операнд __QueryInterface должен быть указателем | Second operand of __QueryInterface must be a pointer |
| `ErrDeleteNeedsPointer` | 242 | Err_DeleteNeedsPointer | Операнд __DELETE должен быть указателем | Operand of __DELETE must be pointer |
| `ErrNamesNotEqual` | 243 | Err_NamesNotEqual | Имя, используемое в интерфейсе, не совпадает с именем объекта | The name used in the signature is not identical to the object name |
| `ErrMissingInstancePathForPersistent` | 244 | Err_MissingInstancePathForPersistent | — | No matching instance path in VAR_PERSISTENT list found for variable {0}. Use the command "Add all instance paths" to add all instance paths to the VAR_PERSISTENT list. (See Help for details) |
| `WrnMissingObjectForPersistent` | 245 | Wrn_MissingObjectForPersistent | — | — |
| `ErrNoBytesInRetain` | 246 | Err_NoBytesInRetain | Структурированный тип, расположенный в энергонезависимых данных, не может содержать BYTE, SINT или USINT | A structured type located in retain data may not contain BYTE, SINT, or USINT |
| `ErrNoPropertiesInOutAssignment` | 247 | Err_NoPropertiesInOutAssignment | Свойство не может быть задано в выходном присваивании вызова. Используйте временную переменную. | A property cannot be assigned in an output assignment of a call. Use a temporary variable instead. |
| `ErrNewNeedsType` | 248 | Err_NewNeedsType | Определение типа требуется в качестве операнда для __NEW | Type definition expected as operand for __NEW |
| `ErrNewArrayOnUserdefNotAllowed` | 249 | Err_NewArrayOnUserdefNotAllowed | Создание массива с помощью __NEW невозможно для пользовательских типов | Creating an array with __NEW is not allowed on user defined types |
| `ErrNewPositionNotOK` | 250 | Err_NewPositionNotOK | Должен быть присвоен результат __NEW | The result of __NEW must be assigned |
| `ErrReferenceNotAllowed` | 261 | Err_ReferenceNotAllowed | Ссылочный тип недопустим в качестве базового типа массива, указателя или ссылки | A reference type is not allowed as base type of an array, pointer, or reference |
| `ErrReferenceNotAllowedForVarInOut` | 262 | Err_ReferenceNotAllowedForVarInOut | Тип ссылки является недопустим как тип VAR_IN_OUT | A reference type is not allowed as type of a VAR_IN_OUT |
| `ErrNewOnlyWithAttribute` | 263 | Err_NewOnlyWithAttribute | Для функционального блока или структуры требуется атрибут '{{attribute 'enable_dynamic_creation'}}' для создания с помощью __NEW | A function block or structure needs the pragma '{{attribute 'enable_dynamic_creation'}}' to be created with __NEW |
| `ErrNoOnlineChangeOnDynamicObjects` | 264 | Err_NoOnlineChangeOnDynamicObjects | Динамически создаваемые типы недопустимы для онлайн-замен | Cannot perform a online change that changes the size of a dynamically created type. |
| `ErrDynamicMemoryNotSupported` | 265 | Err_DynamicMemoryNotSupported | Для приложения '{0}' не задано памяти для создания динамического объекта | No memory for dynamic object creation defined for application '{0}' |
| `WrnLoopExitConditionConstant` | 266 | Wrn_LoopExitConditionConstant | Условие выхода цикла '{0} {1} {2}' является константой FALSE. Возможен бесконечный цикл. | Loop exit condition '{0} {1} {2}' is constant FALSE. Possible endless loop. |
| `ErrUninitialisedVariableUsedInInitialisation` | 268 | Err_UninitialisedVariableUsedInInitialisation | Неинициализированная переменная {0} используется для инициализации {1}. Чтобы изменить порядок инициализации, используйте атрибут 'global_init_slot'. | The uninitialized variable {0} is used for initialization of {1}. Use the attribute 'global_init_slot' to change the order of initialisation. |
| `WrnValueAssignViaPointerMayChangeVFTable` | 269 | Wrn_ValueAssignViaPointerMayChangeVFTable | Для экземпляра, на который указывает {0}, будет выполнена переинициализация для виртуальных вызовов функции. Убедитесь, что {0} не указывает на тип, полученный из {1} | The instance {0} points to will be reinitialized for virtual function calls. Make sure {0} doesn't point to a type derived from {1}  |
| `ErrRetainsNotSupported` | 270 | Err_RetainsNotSupported | Использование энергонезависимых данных невозможно на этом устройстве | Retain data is not not supported on this device |
| `ErrNoReferenceToBits` | 272 | Err_NoReferenceToBits | Ссылки на биты недопустимы | References to bits are not possible |
| `ErrImplicitMethodNameForVariable` | 273 | Err_ImplicitMethodNameForVariable | {0} - это имя неявно сгенерированного метода, и оно не подходит для переменных в функциональном блоке | {0} is the name of an implicitly generated method and illegal for variables in a function block |
| `ErrDeviceNameNoIdent` | 274 | Err_DeviceNameNoIdent | Имя устройства '{0}' некорректно. Требуется корректный МЭК-идентификатор | Device name '{0}' invalid. Must be a valid IEC 61131-3 identifier |
| `ErrNoVarTempInSubsequentPrograms` | 275 | Err_NoVarTempInSubsequentPrograms | Раздел VAR_TEMP недопустим в программах с атрибутом 'subsequent' | VAR_TEMP is not possible in programs with attribute 'subsequent' |
| `ErrRetainNotAccessibleVariable` | 276 | Err_RetainNotAccessibleVariable | Переменная {0} скрывает имя библиотеки. Доступ к библиотечному элементу невозможен. Код для сохранения энергонезависимой переменной {1} сгенерирован не будет. | Variable {0} hides library name. No access to library elements is possible. No implicit code for saving retain variable {1} will be generated. |
| `ErrRetainNotAccessiblePOU` | 277 | Err_RetainNotAccessiblePOU | POU {0} скрывает имя библиотеки, доступ к элементам библиотеки невозможен. Неявного кода для сохранения энергонезависимой переменной {1} сгенерировано не будет. | POU {0} hides library name, no access to library elements is possible. No implicit code for saving retain variable {1} will be generated. |
| `ErrInvalidInitialisationForArray` | 278 | Err_InvalidInitialisationForArray | {0} не является корректной инициализацией для константы {1}. Начальное значение константы должно быть постоянным. | {0} is an invalid initialisation for constant {1}. Initial values of constants must be constant. |
| `ErrSummarizedLibraryErrors` | 279 | Err_SummarizedLibraryErrors | {0} внутренних ошибок в библиотеке {1} | {0} internal errors in library {1} |
| `ErrFinalOnFunctionBlocksAndMethodsOnly` | 280 | Err_FinalOnFunctionBlocksAndMethodsOnly | FINAL можно применять только к методам и функциональным блокам | FINAL may only be applied on methods and function blocks |
| `ErrPrivateOnMethodsOnly` | 281 | Err_PrivateOnMethodsOnly | PRIVATE и PROTECTED можно применять только к методам и функциональным блокам | PRIVATE and PROTECTED may only be applied on methods of function blocks |
| `ErrNoInheritanceOnFinalType` | 282 | Err_NoInheritanceOnFinalType | '{0}' с атрибутом FINAL не может использоваться в качестве основы | '{0}' with attribute FINAL may not be used as base |
| `ErrNoOverrideOnFinalMethod` | 283 | Err_NoOverrideOnFinalMethod | — | Function block '{0}': No override possible on method {1}.{2} with access specifier FINAL |
| `ErrNoOverrideOnPrivateMethod` | 284 | Err_NoOverrideOnPrivateMethod | — | Function block '{0}': No override possible on method {1}.{2} with access specifier PRIVATE |
| `ErrNoOverrideOnInternalMethod` | 285 | Err_NoOverrideOnInternalMethod | — | Function block '{0}': No override possible on method {1}.{2} in library {3} with access specifier INTERNAL |
| `ErrNoChangeOnAccessModifier` | 286 | Err_NoChangeOnAccessModifier | Метод {0}.{1} не может изменять модификатор доступа "{2}" при перезаписи метода {3}.{1} | Method {0}.{1} cannot change access modifier "{2}" when overriding method {3}.{1} |
| `ErrCallOfProtectedMethod` | 287 | Err_CallOfProtectedMethod | Невозможно получить доступ к методу {0}.{1} | Cannot access protected method {0}.{1} |
| `ErrCallOfPrivateMethod` | 288 | Err_CallOfPrivateMethod | Не удается получить доступ к частному методу {0}.{1} | Cannot access private method {0}.{1} |
| `ErrCallOfProtectedProperty` | 289 | Err_CallOfProtectedProperty | Невозможно обратиться к защищенному свойству {0}.{1} | Cannot access protected property {0}.{1} |
| `ErrCallOfPrivateProperty` | 290 | Err_CallOfPrivateProperty | Обращение к частному свойству {0}.{1} невозможно | Cannot access private property {0}.{1} |
| `ErrAccessToInternalVariable` | 291 | Err_AccessToInternalVariable | Невозможно получить доступ к внутренней переменной {0} библиотеки {1} | Cannot access internal variable {0} of library {1} |
| `ErrAccessToInternalObject` | 292 | Err_AccessToInternalObject | Не удается получить доступ к внутреннему объекту {0} библиотеки {1} | Cannot access internal object {0} of library {1} |
| `ErrAccessToInternalProperty` | 293 | Err_AccessToInternalProperty | Обращение к внутреннему свойству {0} библиотеки {1} невозможно | Cannot access internal property {0} of library {1} |
| `ErrInOutAssignedInActionCall` | 294 | Err_InOutAssignedInActionCall | VAR_IN_OUT {0} нельзя присваивать в локальном вызове действия '{1}' | VAR_IN_OUT {0} can't be assigned in local action call '{1}' |
| `ErrSlotFunctionHiddenByVariable` | 295 | Err_SlotFunctionHiddenByVariable | POU {0} с атрибутом слота {1} не может быть вызван, поскольку он скрыт переменной '{2}' | POU {0} with slot attribute {1} not callable, because it is hidden by variable '{2}' |
| `ErrSlotFunctionAmbiguousName` | 296 | Err_SlotFunctionAmbiguousName | POU {0} с атрибутом слота {1} имеет неоднозначное имя | POU {0} with slot attribute {1} has an ambiguous name |
| `ErrStackOverflowDetected` | 297 | Err_StackOverflowDetected | — | Stack overflow detected in Task {0}. Maximal Stack Size: {1} bytes. Calculated Stack Size: {2} bytes. Call Hierarchie: |
| `WrnStackCheckIncompleteDueToRecursion` | 298 | Wrn_StackCheckIncompleteDueToRecursion | — | Calculation of stack usage incomplete because of recursive calls, starting at '{0}': |
| `ErrNoCodegenerator` | 299 | Err_NoCodegenerator | Не удалось создать генератор кода: нужный плагин не установлен | Creating a code generator failed: The required plug-in is not installed |
| `ErrRelatedPositionTaskX` | 300 | Err_RelatedPositionTaskX | — | Related position: Access in task {0} |
| `ErrPersistentVariablesChangeMessageText` | 301 | Err_PersistentVariablesChangeMessageText | Persistent-переменные изменились. Загрузка отменена пользователем. | Persistent variables changed. Download cancelled by user. |
| `ErrCancelledByUser` | 302 | Err_CancelledByUser | Компиляция отменена пользователем | Compilation cancelled by user |
| `ErrStructureInitialisationNotPossible` | 303 | Err_StructureInitialisationNotPossible | Инициализация структуры недопустима в качестве параметра вызова Init-функции. Используйте переменную. | A structure initialisation is not possible as Parameter of an Init-function call. Use a variable instead. |
| `ErrArrayInitialisationNotPossible` | 304 | Err_ArrayInitialisationNotPossible | Инициализация массива недопустима в качестве параметра вызова Init-функции. Используйте переменную | An array initialisation is not possible as parameter of an initial function call. Use a variable instead |
| `ErrStructuredValueTypeInExternalCall` | 306 | Err_StructuredValueTypeInExternalCall | Структурированный тип '{0}' недопустим во внешнем вызове функции | Structured type '{0}' not allowed in external function call |
| `ErrRecursiveConstantInitialisation` | 307 | Err_RecursiveConstantInitialisation | Рекурсивное определение константного значения. | Recursive definition of constant value |
| `WrnShiftExceedsTypeSize` | 308 | Wrn_ShiftExceedsTypeSize | Сдвиг на {0} превышает размер типа {1} | Shift by {0} exceeds type size of {1} |
| `ErrOperatorNotSupported` | 309 | Err_OperatorNotSupported | Оператор '{0}' не поддерживается | Operator '{0}' not supported |
| `ErrLValueForAnyVar` | 310 | Err_LValueForAnyVar | Параметр ANY '{0}' из '{1}' требует переменную с доступом записи в качестве входа | ANY parameter '{0}' of '{1}' needs variable with write access as input |
| `ErrAnyTypeOnlyInFunction` | 311 | Err_AnyTypeOnlyInFunction | Переменные типа '{0}' допустимы только в качестве входов функции. | Variables of type '{0}' only allowed as input of functions |
| `WrnConcurrentAccessOfBitInSameByte` | 312 | Wrn_ConcurrentAccessOfBitInSameByte | Одновременный доступ к биту '{0}' в одном и том же байте | Concurrent access of bit '{0}' in same byte |
| `ErrNoInitialForInoutConstant` | 313 | Err_NoInitialForInoutConstant | Для VAR_INOUT CONSTANT {0} начальное значение недопустимо | No initial value allowed for VAR_INOUT CONSTANT {0} |
| `ErrVariableForVarinoutConstant` | 314 | Err_VariableForVarinoutConstant | Параметр VAR_IN_OUT CONSTANT '{0}' из '{1}' требует переменной в качестве входа | VAR_IN_OUT CONSTANT parameter '{0}' of '{1}' needs variable as input |
| `WrnStringTooShortForVarInOut` | 315 | Wrn_StringTooShortForVarInOut | — | — |
| `WrnMethodAlreadyCalledImplicitly` | 316 | Wrn_MethodAlreadyCalledImplicitly | Метод '{0}' уже вызван неявно | Method '{0}' already called implicitly |
| `ErrLiteralExpected` | 317 | Err_LiteralExpected | Вместо '{0}' требуется литерал | Literal expected instead of '{0}' |
| `ErrExprNoConstantError` | 318 | Err_ExprNoConstantError | Вместо '{0}' требуется константное значение | Constant value expected instead of '{0}' |
| `ErrNotAllowedInInterfaceLib` | 319 | Err_NotAllowedInInterfaceLib | '{0}' недопустим в интерфейсной библиотеке | '{0}' not allowed in interface library |
| `ErrNotAllowedInContainerLib` | 320 | Err_NotAllowedInContainerLib | '{0}' недопустимо в контейнерной библиотеке | '{0}' not allowed in container library |
| `ErrInterfaceMethodImplementationNotPublic` | 321 | Err_InterfaceMethodImplementationNotPublic | Реализация интерфейсного метода '{0}' функционального блока '{1}' должна быть PUBLIC | Implementation of interface method '{0}' of function block '{1}' must be PUBLIC |
| `ErrNoVarConfigInMethodOrFunction` | 322 | Err_NoVarConfigInMethodOrFunction | Неполные адреса недопустимы в методах и функциях | Incomplete adresses are not allowed in methods and functions |
| `ErrCantHaveBaseClass` | 323 | Err_CantHaveBaseClass | Ключевое слово EXTENDS не применимо к типу {0} | Keyword EXTENDS not applicable to type {0} |
| `ErrNoIndirectPropertyCallOnStringWithSize` | 324 | Err_NoIndirectPropertyCallOnStringWithSize | Косвенный вызов свойства с типом {0} невозможен, поскольку поддержваются только типы без явно заданной длины | Indirect call of property with type {0} not possible because only types without explicitly specified length are supported |
| `WrnGlobalInitSlotNotForVariables` | 325 | Wrn_GlobalInitSlotNotForVariables | Атрибут 'global_init_slot' всегда относится ко всему списку переменных и не должен применяться к отдельным переменным. | The attribute 'global_init_slot' always affects the whole variable list, and should not be applied on single variables |
| `ErrLibraryWithUnicodeIdentifiers` | 326 | Err_LibraryWithUnicodeIdentifiers | Библиотека '{0}' была сохранена с поддержкой идентификаторов unicode и может быть, таким образом, использована только при поддержке идентификаторов unicode (см. опции компиляции). | The library '{0}' was saved with support of unicode identifiers, and can only be used if the project also supports unicode identifiers (see compile options). |
| `WrnImplicitEnumConversion` | 327 | Wrn_ImplicitEnumConversion | Неявная конверсия из одного типа перечисления ({0}) в другой ({1}) | Implicit conversion from one enumeration type ({0}) to another ({1}) |
| `ErrNoAssign` | 328 | Err_NoAssign | Присваивание недоступно для типа {0} | Assignment not allowed for type {0} |
| `ErrDuplicateVarConfig` | 329 | Err_DuplicateVarConfig | Копировать VAR_CONFIG для '{0}' | Duplicate VAR_CONFIG for '{0}' |
| `ErrTryCatchNotSupportedVersion` | 330 | Err_TryCatchNotSupportedVersion | Структурная обработка исключений не поддерживается вашим текущим таргетом. Требуется система исполнения версии не ниже 3.5.6.0. | Structured exception handling is not supported by your current target. At least runtime system version 3.5.6.0 is required. |
| `ErrTryCatchNotSupportedCodegenerator` | 331 | Err_TryCatchNotSupportedCodegenerator | Генератор кода для текущего устройства не поддерживает структурную обработку исключений | The code generator for the current device does not support structured exception handling |
| `ErrMappedVarWrittenInDiffTasks` | 332 | Err_MappedVarWrittenInDiffTasks | Переменная {0}, соотнесенная с адресом {1}, записывается в разных задачах | Variable {0}, which is mapped on address {1} is written in different tasks |
| `ErrOpTakesAtMostInputs` | 333 | Err_OpTakesAtMostInputs | Для '{0}' требуется не более '{1}' операндов | '{0}' takes at most '{1}' operands |
| `ErrLibNamespaceConflict` | 334 | Err_LibNamespaceConflict | Локальное пространство имен '{0}' библиотеки '{1}' скрывает пространство имен библиотеки '{2}' | Local namespace '{0}' of library '{1}' hides namespace of library '{2}' |
| `WrnComplexExpression` | 335 | Wrn_ComplexExpression | POU содержит очень сложное выражение. Попробуйте использовать промежуточные результаты. | The POU contains very complex expression. Consider using intermediate results. |
| `ErrBitAccessOnlyOnInt` | 336 | Err_BitAccessOnlyOnInt | Битовый доступ возможен только для целочисленных типов. | Biaccess is only possible on integer types |
| `ErrVarInstOnlyInMethods` | 337 | Err_VarInstOnlyInMethods | Объявление 'VAR_INST' недопустимо в этом месте | VAR_INST declaration not allowed in this place |
| `ErrLibSupports32BitOnly` | 338 | Err_LibSupports32BitOnly | Библиотека '{0}' поддерживается только 32-битными приложениями | The Library '{0}' is only supported in 32 bit applications |
| `WrnBoolNotAtBitAddress` | 339 | Wrn_BoolNotAtBitAddress | Для типа данных BOOL требуется битовый адрес | Bit address expected for data type BOOL |
| `ErrCheckLicenseNeedsSysTarget` | 340 | Err_CheckLicenseNeedsSysTarget | Для оператора __CHECKLICENSE требуются библиотеки SysTarget и 3S License | The 3S License library is required for the __CHECKLICENSE operator |
| `ErrOperatorNotAllowedAtPosition` | 341 | Err_OperatorNotAllowedAtPosition | Использование оператора '{0}' недопустимо в данном выражении | The usage of the operator '{0}' is not allowed in this statement |
| `ErrInstanceNotAllowedInRetain` | 342 | Err_InstanceNotAllowedInRetain | Экземпляры '{0}' недопустимы в энергонезависимых данных | Instances of '{0}' are not allowed in retain data |
| `ErrReferenceToInput` | 343 | Err_ReferenceToInput | Присваивание ссылки невозможно для переменной, соотнесенной с входным адресом. | Reference assign is not possible on a variable mapped to an input address |
| `WrnStruturedTypePropertyNotMonitorable` | 344 | Wrn_StruturedTypePropertyNotMonitorable | Атрибут monitoring игнорируется для свойства '{0}', возвращающего структурированный тип | The monitoring attribute is not supported for property '{0}' and will be ignored |
| `ErrTryCatchNotSupported` | 345 | Err_TryCatchNotSupported | Структурная обработка исключений пока не поддерживается | Structured exception handling is not supported yet |
| `ErrNoDirectAddressInSubsequentVarDecl` | 346 | Err_NoDirectAddressInSubsequentVarDecl | Объявление переменной '{0}' с присваиванием адреса не может быть использовано в PRG или GVL с атрибутом 'subsequent' | The declaration of the variable '{0}' with an address assignment cannot be used in a PRG or GVL with the attribute 'subsequent' |
| `ErrUnexpectedOperandForCallInitFunction` | 347 | Err_UnexpectedOperandForCallInitFunction | Неуместный операнд '{0}' в '{1}'. GVL или PRG должны передаваться как параметр. | Unexpected operand '{0}' found in '{1}'. A GVL or PRG must be passed as parameter. |
| `ErrBitAdrOnOperation` | 348 | Err_BitAdrOnOperation | Оператор 'BITADR' возможен только для переменных (не для oперация) | Operator BITADR is only possible on variables, not on operations |
| `WrnInterfaceInVarInOut` | 349 | Wrn_InterfaceInVarInOut | — | — |
| `WrnReferenceToInterface` | 350 | Wrn_ReferenceToInterface | — | — |
| `WrnAttributeCheck` | 351 | Wrn_AttributeCheck | Неизвестный атрибут или некорректное значение | Attribute check failed |
| `ErrMaxArraySizeExceeded` | 352 | Err_MaxArraySizeExceeded | Достигнут максимальный размер массива. Либо увеличьте нижнюю границу, либо уменьшите верхнюю. | Maximum array size exceeded. Either increase the lower border or decrease the upper border. |
| `ErrRefAssignNeedsLValue` | 353 | Err_RefAssignNeedsLValue | Для присваивания ссылки в качестве исходного выражения требуется переменная с доступом для записи | A reference assignment requires a variable with write access as the source expression |
| `WrnEnumComparison` | 354 | Wrn_EnumComparison | Сравнение одного типа перечисления ({0}) с другим ({1}) | Comparison of one enumeration type ({0}) with another ({1}) |
| `WrnNoPointerToBit` | 355 | Wrn_NoPointerToBit | Ссылка на один бит невозможна. Будет сохранена ссылка на весь байт. | A single bit cannot be referenced. A reference to the complete byte will be stored. |
| `ErrAttributeNotValidForNonExternalPOU` | 356 | Err_AttributeNotValidForNonExternalPOU | Атрибут '{0}' не подходит для невнешнего POU | Attribute '{0}' not valid for non-external POU |
| `WrnObsolete` | 357 | Wrn_Obsolete | POU '{0}' отмечен как устаревший: {1} | POU '{0}' has been marked as obsolete: {1} |
| `ErrStrictEnumNotAMember` | 358 | Err_StrictEnumNotAMember | '{0}' - неподходящее значение для типа ENUM '{1}' | '{0}' is not a valid value for strict ENUM type '{1}' |
| `ErrStrictEnumNoArithmeticAllowed` | 359 | Err_StrictEnumNoArithmeticAllowed | Арифметические действия недопустимы для строгих перечислений '{0}' | Arithmetics not allowed on strict ENUM type '{0}' |
| `ErrParameterlistNotConst` | 360 | Err_ParameterlistNotConst | Список параметров должен быть задан как константа | A parameter list must be declared as constant |
| `ErrNoMixExternalIECInheritance` | 361 | Err_NoMixExternalIECInheritance | — | An extending POU must be implemented the same way as its base (externally or in IEC) |
| `ErrInvalidStringSize` | 362 | Err_InvalidStringSize | — | — |
| `ErrUserCheckFunctionsNotSupported` | 363 | Err_UserCheckFunctionsNotSupported | Функции, заданные пользователем, не поддерживаются | User defined check functions are not supported |
| `ErrCallAfterInitHasInputs` | 364 | Err_CallAfterInitHasInputs | Метод с отметкой '{0}' не может иметь входы. | A method marked with '{0}' cannot have any inputs |
| `ErrGeneratingVarInitializations` | 365 | Err_GeneratingVarInitializations | Некорректные нач. значения переменных | One or more inital values for variables are invalid |
| `ErrImplicitEnumerationTypeNotExpected` | 366 | Err_ImplicitEnumerationTypeNotExpected | Здесь требуется неявное перечисление | Implicit enumeration type not expected in this place |
| `ErrInternalErrorProhibitingOnlineChange` | 367 | Err_InternalErrorProhibitingOnlineChange | Внутренняя ошибка {0}, исключающая онлайн-замену! Требуется очистка приложения и загрузка. | Internal error {0} prohibiting online change. Clean application and download necessary. |
| `ErrInvalidEnumDefaultValue` | 368 | Err_InvalidEnumDefaultValue | — | Only local enumeration members can be used as default initialization values for an enumeration |
| `ErrFeatureNotImplemented` | 369 | Err_FeatureNotImplemented | Функция '{0}' не реализована. | The feature '{0}' is not implemented. |
| `WrnInstanceCalledMoreThenOnce` | 370 | Wrn_InstanceCalledMoreThenOnce | — | — |
| `WrnNonLocalAccessToVarInOut` | 371 | Wrn_NonLocalAccessToVarInOut | Обращение к VAR_IN_OUT '{0}', объявленной в '{1}', из внешнего контекста '{2}'. | Access to VAR_IN_OUT '{0}' declared in '{1}' from external context '{2}' |
| `ErrDuplicateElseInCaseStatement` | 372 | Err_DuplicateElseInCaseStatement | Повторное определение блока CASE | Duplicate definition of ELSE in CASE statement |
| `WrnPragma` | 373 | Wrn_Pragma | Пользовательское предупреждение, сгенерированное прагмой | User defined warning generated by warning pragma |
| `ErrDivisionByZero` | 374 | Err_DivisionByZero | — | Division by zero |
| `ErrInvalidEnumBaseType` | 375 | Err_InvalidEnumBaseType | — | Only integer types are supported as an enum base type |
| `ErrTooFewParametersForExtensibleFunction` | 376 | Err_TooFewParametersForExtensibleFunction | — | Extensible function {0} needs at least {1} inputs for parameter {2} |
| `ErrTooManyParametersForExtensibleFunction` | 377 | Err_TooManyParametersForExtensibleFunction | — | Extensible function {0} accepts at most {1} inputs for parameter {2} |
| `ErrNoFormalParamsCallsForExtensibleFunction` | 378 | Err_NoFormalParamsCallsForExtensibleFunction | — | No explicit input assignments possible for call of extensible function {0} |
| `ErrIntegerLiteralExpected` | 379 | Err_IntegerLiteralExpected | — | Integer literal expected instead of {0} |
| `ErrLowerUpperBoundOnVariableLengthArrayOnly` | 380 | Err_LowerUpperBoundOnVariableLengthArrayOnly | — | The operators LOWER_BOUND and UPPER_BOUND are only supported for arrays |
| `ErrOperatorNoValidVariableName` | 381 | Err_OperatorNoValidVariableName | — | The operator '{0}' is not a valid name for a variable |
| `ErrInconsistentInheritanceOfCPPCompatibility` | 382 | Err_InconsistentInheritanceOfCPPCompatibility | — | Inconsistent inheritance of C++-compatibility. Missing attribute for '{0}'. |
| `ErrIncompletePOUDeclaration` | 383 | Err_IncompletePOUDeclaration | — | The POU declaration is incomplete without a name |
| `ErrSelMuxOnlyEqualUserDefTypes` | 384 | Err_SelMuxOnlyEqualUserDefTypes | — | All user defined types used in SEL or MUX must be equal |
| `ErrVarLengthArrayInOut` | 385 | Err_VarLengthArrayInOut | — | Variable length arrays are only possible as VAR_IN_OUT of function blocks or as VAR_IN_OUT and VAR_INPUT of methods and functions |
| `ErrVarLengthArrayTopLevel` | 386 | Err_VarLengthArrayTopLevel | — | A variable length array type has to be on top level position of a type declaration |
| `ErrOutParamNotEqual` | 387 | Err_OutParamNotEqual | — | Type '{0}' is not equal to type '{1}' of VAR_OUTPUT '{2}' |
| `WrnLibWithStringInVarInOut` | 388 | Wrn_LibWithStringInVarInOut | — | Consider declaring VAR_IN_OUT string variable '{0}' in '{1}' as VAR_IN_OUT CONSTANT to allow callers to pass literals or constant. |
| `WrnLValueForVarinoutStrings` | 389 | Wrn_LValueForVarinoutStrings | Для параметра VAR_IN_OUT '{0}' из '{1}' в качестве входа требуется переменная с доступом записи. В последующих версиях это будет ошибкой компиляции! | VAR_IN_OUT respectively REFERENCE parameter '{0}' of '{1}' needs variable with write access as input |
| `ErrAddressSourceIsAddressDest` | 390 | Err_AddressSourceIsAddressDest | — | Variable '{0}' was located on address '{1}'. At this address variable '{2}' is now located. This change is not possible with online change. |
| `ErrNoCopyCodeAllowed` | 391 | Err_NoCopyCodeAllowed | Перемещение этого POU при онлайн-замене невозможно | Moving this POU to a new location during online changes is not allowed |
| `ErrATDeclarationNotAllowed` | 392 | Err_ATDeclarationNotAllowed | — | An AT declaration is only allowed inside a VAR section |
| `ErrArrayBorderNoValidSignedInteger` | 393 | Err_ArrayBorderNoValidSignedInteger | — | Array border {0} does not evaluate to a valid signed integer constant |
| `WrnFBExitCalledForStackInstance` | 394 | Wrn_FBExitCalledForStackInstance | — | — |
| `ErrNumOfInitializersDoNotMatch` | 395 | Err_NumOfInitializersDoNotMatch | — | The number of 'FB_Init' initializers ({0}) does not match the number of array elements ({1}) |
| `ErrIsValidRefNeedsReference` | 396 | Err_IsValidRefNeedsReference | — | Operand for __ISVALIDREF must be of type REFERENCE |
| `ErrImplicitMethodImplementationNotPublic` | 397 | Err_ImplicitMethodImplementationNotPublic | — | Function block '{0}': The access to the methods 'FB_Init', 'FB_Exit', and 'FB_ReInit' must be PUBLIC |
| `ErrSystemOutOfMemory` | 398 | Err_SystemOutOfMemory | Система разработки не располагает достаточной памятью для онлайн-замены. Перезапустите приложение. | The development system has not enough memory to process the online change. Please restart the application before continuing development. |
| `ErrImplicitReferenceTypeDeclNotAllowed` | 399 | Err_ImplicitReferenceTypeDeclNotAllowed | — | The declaration of an implicit reference type is not possible at this location |
| `ErrImplicitRefTypeIsnotAllowedAsBase` | 400 | Err_ImplicitRefTypeIsnotAllowedAsBase | — | Implicit reference types cannot be a base type of references, pointers, and arrays |
| `ErrImplicitRefTypeAllClassesNeedAttrib` | 401 | Err_ImplicitRefTypeAllClassesNeedAttrib | — | The inheriting functionblock differs in its usage of the attribute '{0}' |
| `ErrImplicitRefTypeDeclarationNotAllowed` | 402 | Err_ImplicitRefTypeDeclarationNotAllowed | — | The implicit reference type '{0}' cannot be declared in this type |
| `ErrImplicitRefTypeSignNotSupported` | 403 | Err_ImplicitRefTypeSignNotSupported | — | Only function blocks and structures can be marked as an implicit reference type |
| `WrnCompilerVersionDeprecated` | 404 | Wrn_CompilerVersionDeprecated | — | — |
| `ErrMultipleAssignmentsToInterfaceVariables` | 405 | Err_MultipleAssignmentsToInterfaceVariables | — | Multiple assignments to interface variables not allowed |
| `WrnImplicitCheckFunctionShadowed` | 406 | Wrn_ImplicitCheckFunctionShadowed | — | The implicit check function '{0}' is hidden by another variable or function. Checks will not be performed! Resolve the conflict and clean the application to use the check function. |
| `ErrAddressOfNonInstanceVar` | 407 | Err_AddressOfNonInstanceVar | — | Instance required instead of type name in address-of expression '{0}' |
| `ErrImplicitReferenceTypeOnlChangeError` | 408 | Err_ImplicitReferenceTypeOnlChangeError | — | It is not possible to add a local implicit reference type via online change |
| `ErrNoResolutionForLazyVariable` | 409 | Err_NoResolutionForLazyVariable | — | Type of lazy typed variable '{0}' could not be resolved |
| `WrnCompatibilityProblemForRefProperty` | 410 | Wrn_CompatibilityProblemForRefProperty | — | — |
| `ErrNoVarInputInPropertyAccessors` | 411 | Err_NoVarInputInPropertyAccessors | — | It is not allowed to define input variables in property accessors: {0} : {1} |
| `ErrMultipleAssignsToSameInputInCall` | 412 | Err_MultipleAssignsToSameInputInCall | — | Multiple input assignments for parameter '{0}' |
| `ErrRefAssignOnlyForReferenceTypes` | 413 | Err_RefAssignOnlyForReferenceTypes | — | Initialisation with REF= is only allowed for variables of type REFERENCE TO |
| `ErrOutOfPersistentMemoryImplicit` | 414 | Err_OutOfPersistentMemoryImplicit | — | Not enough persistent memory {0} |
| `ErrOutOfPersistentMemoryExplicit` | 415 | Err_OutOfPersistentMemoryExplicit | — | Out of persistent memory: Variable '{0}', {1} bytes (Largest contiguous memory gap {2}). Editing persistent variable lists may produce fragmented memory. Perform "Declarations, Reorder list and clear gaps" to compact persistent variable lists. {3} |
| `ErrLibraryNamespaceNotValid` | 416 | Err_LibraryNamespaceNotValid | — | Namespace {0} of library {1} is no valid identifier |
| `ErrLValueForVarinoutStrings` | 417 | Err_LValueForVarinoutStrings | VAR_IN_OUT-параметр '{0}' из '{1}' требует переменной с доступом записи в качестве входа | VAR_IN_OUT respectively REFERENCE parameter '{0}' of '{1}' needs variable with write access as input |
| `ErrStringTooShortForVarInOut` | 418 | Err_StringTooShortForVarInOut | Строковая переменная '{0}' слишком коротка для VAR_IN_OUT-параметра '{1}' из '{2}' | String variable '{0}' too short for the VAR_IN_OUT parameter '{1}' of '{2}' |
| `ErrNoInputsWithSlotAttributes` | 419 | Err_NoInputsWithSlotAttributes | — | No inputs allowed in signature '{0}' with attribute '{1}' |
| `ErrNotEnoughMemoryForCompactDownload` | 420 | Err_NotEnoughMemoryForCompactDownload | — | Not enough memory left for compact download in first code area. '{0}' bytes could not be allocated |
| `WrnExtendsForInterfaces` | 421 | Wrn_ExtendsForInterfaces | — | Use keyword EXTENDS for inheritance of interfaces instead of IMPLEMENTS |
| `WrnGranularityMismatchForDirectVariable` | 422 | Wrn_GranularityMismatchForDirectVariable | — | Variable '{0}' has a granularity of {1} but is located at direct address {2} which is not aligned to {1} bytes |
| `ErrMultipleAssignmentWithReferences` | 423 | Err_MultipleAssignmentWithReferences | — | Multiple assignments of references to function blocks or data structures not allowed |
| `ErrMultipleAssignmentWithChangingValueTypes` | 424 | Err_MultipleAssignmentWithChangingValueTypes | — | Multiple assignments of value types with implicit casts not allowed |
| `ErrNoMemoryReserveForExternal` | 425 | Err_NoMemoryReserveForExternal | — | Usage of a memory reserve for external function blocks is not supported |
| `WrnAtLeastOneExpected` | 426 | Wrn_AtLeastOneExpected | Требуется не менее одного оператора | At least one statement is expected |
| `ErrUnknownMaxStackSize` | 427 | Err_UnknownMaxStackSize | — | Accessing information about the currently executed task requires knowledge about the maximal stack size supported on the target system |
| `ErrAbstractOnFunctionBlocksAndMethodsOnly` | 428 | Err_AbstractOnFunctionBlocksAndMethodsOnly | — | ABSTRACT may only be applied on methods and function blocks |
| `ErrAbstractAndFinalNotPossible` | 429 | Err_AbstractAndFinalNotPossible | — | A method or functionblock cannot be ABSTRACT and FINAL |
| `ErrAbstractAndPrivateNotPossible` | 430 | Err_AbstractAndPrivateNotPossible | — | A PRIVATE method cannot be ABSTRACT |
| `ErrAbstractMethodNotImplemented` | 431 | Err_AbstractMethodNotImplemented | — | There is no implementation for ABSTRACT method '{0}' defined in function block '{1}' |
| `ErrAbstractMethodOnlyInAbstractFunctionblock` | 432 | Err_AbstractMethodOnlyInAbstractFunctionblock | — | The ABSTRACT method {0} requires, that the function block {1} is ABSTRACT too |
| `ErrAbstractMethodMustNotContainAnyStatements` | 433 | Err_AbstractMethodMustNotContainAnyStatements | — | The ABSTRACT method {0}.{1} must not contain any statements |
| `ErrAbstractFunctionBlockInstance` | 434 | Err_AbstractFunctionBlockInstance | — | Function block {0} is ABSTRACT and cannot be instantiated |
| `ErrAbstractPropertyNotImplemented` | 435 | Err_AbstractPropertyNotImplemented | — | There is no implementation for {0} of ABSTRACT property '{1}' defined in function block '{2}' |
| `ErrStringLengthIsNoConstant` | 436 | Err_StringLengthIsNoConstant | — | String length '{0}' is no constant value |
| `ErrNotAllowedInTaskLocalVariables` | 437 | Err_NotAllowedInTaskLocalVariables | — | Task local variable list '{0}': '{1}' is not allowed |
| `ErrTaskLocalVariablesWriterTaskNotDefined` | 438 | Err_TaskLocalVariablesWriterTaskNotDefined | — | Task local variable list '{0}': Writer task not defined |
| `ErrTaskLocalVariablesAccessNotAllowed` | 439 | Err_TaskLocalVariablesAccessNotAllowed | — | Task local variable list '{0}': Write access only allowed in task '{1}' |
| `ErrTaskLocalVariablesNoOnlineChangePossible` | 440 | Err_TaskLocalVariablesNoOnlineChangePossible | — | Task local variable list '{0}' changed. No online change possible. |
| `WrnVarInOutUnitializedInInitialValue` | 441 | Wrn_VarInOutUnitializedInInitialValue | — | Access to uninitialized VAR_IN_OUT variable |
| `ErrAbstractMethodStaticCall` | 442 | Err_AbstractMethodStaticCall | — | The ABSTRACT method '{0}' cannot be called by a static call |
| `ErrAbstractPropertyStaticCall` | 443 | Err_AbstractPropertyStaticCall | — | The ABSTRACT property '{0}' cannot be statically accessed |
| `ErrInconsistentUseOfCPPCompatibility` | 444 | Err_InconsistentUseOfCPPCompatibility | — | Inconsistent use of C++ compatibility. Missing attribute for '{0}'. |
| `ErrAbstractWrongVarInMethod` | 445 | Err_AbstractWrongVarInMethod | — | Only inputs, outputs and inouts allowed in ABSTRACT methods |
| `ErrNoValuePassingForCPPExternal` | 446 | Err_NoValuePassingForCPPExternal | — | CPP compatible functions can not contain inputs or outputs of Type STRING, ARRAY, or structured types. Use POINTER or REFEFRENCE to this type instead. |
| `WrnOnlyConstantInitialValueForMappedPersistentVar` | 447 | Wrn_OnlyConstantInitialValueForMappedPersistentVar | — | Only replaced constants can be applied as initial value for a mapped persistent variable |
| `ErrOperatorNotSupportedVersion` | 448 | Err_OperatorNotSupportedVersion | — | Operator '{0}' is not supported by your current target. At least runtime system version {1} is required. |
| `ErrComparisonOperatorExpected` | 449 | Err_ComparisonOperatorExpected | — | Comparison operator expected instead of '{0}' |
| `ErrStringLiteralExpected` | 450 | Err_StringLiteralExpected | — | String literal expected instead of '{0}' |
| `ErrVersionOverflow` | 451 | Err_VersionOverflow | — | At least one part of the version '{0}' has a too big value |
| `ErrVersionPartNegative` | 452 | Err_VersionPartNegative | — | At least one part of the version '{0}' has a negative value |
| `ErrVersionInvalidFormat` | 453 | Err_VersionInvalidFormat | — | Token '{0}' is no valid version |
| `ErrNoNewAssignmentInOtherExpression` | 454 | Err_NoNewAssignmentInOtherExpression | — | It is not possible to use an assignment expression with the __NEW operator in another expression. Use the pointer variable instead. |
| `ErrExplicitTransitionAssignMissing` | 455 | Err_ExplicitTransitionAssignMissing | — | The code of transition contains multiple statements. The explicit assignment to transition output variable is required. |
| `WrnAmbiguousCheckfunctionInLibrary` | 456 | Wrn_AmbiguousCheckfunctionInLibrary | — | The implicit check function {0} is hidden by another implicit check function {1}. Checks will not be performed! Resolve the conflict and clean the application to use the check function. |
| `ErrVectorSizeNotValid` | 500 | Err_VectorSizeNotValid | — | The size of a vector must be a integer greater than 0 and less than 9 |
| `ErrVectorSizeIsNoConstant` | 501 | Err_VectorSizeIsNoConstant | — | The size of a vector must be a constant |
| `ErrVectorNoPersistentRetain` | 502 | Err_VectorNoPersistentRetain | — | A vector cannot be declared persistent or retained |
| `ErrVectorBaseMustBeRealType` | 503 | Err_VectorBaseMustBeRealType | — | The base type of a vector must be either REAL or LREAL. |
| `ErrVectorTypesNotCompatible` | 504 | Err_VectorTypesNotCompatible | — | The vector types {0} and {1} are not compatible |
| `ErrVectorTypeCantBePlacedInUnion` | 505 | Err_VectorTypeCantBePlacedInUnion | — | Vector types cannot be placed in unions |
| `ErrLowerUpperBoundOperandNotInRange` | 506 | Err_LowerUpperBoundOperandNotInRange | — | Value of 2nd operand of the {0} operator must be between 1 and {1} in this case |
| `ErrLowerUpperBoundOperandNotExactly` | 507 | Err_LowerUpperBoundOperandNotExactly | — | Value of 2nd operand of the {0} operator must be 1 in this case |
| `WrnAmbiguity` | 508 | Wrn_Ambiguity | — | — |
| `ErrMultipleAssignmentsNotAllowedForOperator` | 509 | Err_MultipleAssignmentsNotAllowedForOperator | — | Multiple assignments are not allowed for operator '{0}'. |
| `ErrConfiguredCompilerVersionNotAvailable` | 510 | Err_ConfiguredCompilerVersionNotAvailable | — | — |
| `ErrAbstractFunctionBlockAssigned` | 511 | Err_AbstractFunctionBlockAssigned | — | Function block {0} is ABSTRACT and cannot be used as a target for an assignment |
| `WrnCallOfPrivateProperty` | 513 | Wrn_CallOfPrivateProperty | — | Should not access private property {0}.{1} |
| `WrnAccessToInternalProperty` | 514 | Wrn_AccessToInternalProperty | — | Should not access internal property {0} of library {1} |
| `WrnCallOfProtectedProperty` | 515 | Wrn_CallOfProtectedProperty | — | Should not access protected property {0}.{1} |
| `WrnAccessToInternalVariable` | 516 | Wrn_AccessToInternalVariable | — | Should not access internal variable {0} of library {1} |
| `WrnAccessToInternalObject` | 517 | Wrn_AccessToInternalObject | — | Should not access internal object {0} of library {1} |
| `ErrNoNamespace` | 518 | Err_NoNamespace | — | '{0}' does not refer to a namespace |
| `ErrInvalidAccessPathForNamespaceAccess` | 519 | Err_InvalidAccessPathForNamespaceAccess | — | '{0}' is no valid access path |
| `ErrInvalidNamespaceForNamespaceAccess` | 520 | Err_InvalidNamespaceForNamespaceAccess | — | '{0}' is no valid namespace access |
| `ErrUnknownCompilerVersionInCompiledLib` | 521 | Err_UnknownCompilerVersionInCompiledLib | — | The compiler version "{1}" with which the compiled library "{0}" was created is newer than the compiler version of the project or unknown. |
| `WrnPersistentVariableOnStack` | 522 | Wrn_PersistentVariableOnStack | — | Persistent variables in function block instances located on the stack are not supported: {0} |
| `ErrInconsistentUseOfCPPCompatibilityMissingParent` | 523 | Err_InconsistentUseOfCPPCompatibilityMissingParent | — | — |
| `ErrWrongReInit` | 524 | Err_WrongReInit | — | The FB_ReInit method of a function block or struct must have no inputs and a return value of type BOOL. The FB_ReInit will not be called automatically! |
| `WrnInvalidDefaultValue` | 525 | Wrn_InvalidDefaultValue | — | The type {0} cannot have a default value in this context |
| `WrnDefaultValueNotConstant` | 526 | Wrn_DefaultValueNotConstant | — | Default value is not constant |
| `WrnDefaultValueTopLevel` | 527 | Wrn_DefaultValueTopLevel | — | The input is only optional when this function is called from IEC code |
| `ErrNoBranchOutOfFinally` | 528 | Err_NoBranchOutOfFinally | — | Branch out of __FINALLY-Block is not allowed |
| `ErrNoExitOrContinueOutOfTry` | 529 | Err_NoExitOrContinueOutOfTry | — | Exit or Continue out of __TRY-Block is not allowed |
| `ErrNoExitOrContinueOutOfCatch` | 530 | Err_NoExitOrContinueOutOfCatch | — | Exit or Continue out of __CATCH-Block is not allowed |
| `ErrMultipleAssignmentWithProperty` | 531 | Err_MultipleAssignmentWithProperty | — | Properties cannot be used in the middle of multiple assignments. |
| `ErrFunNeedsAtLeastNInputs` | 532 | Err_FunNeedsAtLeastNInputs | — | Function '{0}' requires at least '{1}' and maximum '{2}' inputs |
| `WrnObsoleteOutputInAbstractMethod` | 533 | Wrn_ObsoleteOutputInAbstractMethod | — | The default value for a VAR_OUTPUT is not used in abstract or interface methods |
| `ErrWrongCallAfterGlobalInitSlotSignature` | 534 | Err_WrongCallAfterGlobalInitSlotSignature | — | A signature decorated with the 'call_after_global_init_slot' attribute must either have no arguments or a single input named 'bInitRetains'. |
| `InfUseXSizeOfOperator` | 535 | Inf_UseXSizeOfOperator | — | Use  the XSIZEOF operator for large types |
| `ErrInvalidInitialisationForVarInst` | 536 | Err_InvalidInitialisationForVarInst | — | The initial value for a VAR_INST variable may not use local variables. |
| `ErrNoPropertyForVarInout` | 537 | Err_NoPropertyForVarInout | — | Properties can't be assigned to VAR_IN_OUT. |
| `ErrInterfaceChangedNumberOfInputsOutputsDifferent` | 538 | Err_InterfaceChanged_NumberOfInputsOutputsDifferent | — | The number of inputs/outputs of the method '{0}' does not correspond to the interface '{1}'. |
| `ErrInterfaceChangedVariableDifferent` | 539 | Err_InterfaceChanged_VariableDifferent | — | The variable '{0}' of the method '{1}' does not correspond to the interface '{2}'. |
| `WrnMissingAttributeNoAssign` | 540 | Wrn_MissingAttributeNoAssign | — | Attribute 'no_assign' missing for POU '{0}'? The type of the variable '{1}' is attributed with 'no_assign'. |
| `ErrNoMemoryAllocationCallback` | 541 | Err_NoMemoryAllocationCallback | — | Creating the memory allocator failed: The required plug-in is not installed |
| `WrnNoInheritanceForUnions` | 542 | Wrn_NoInheritanceForUnions | — | Inheritance is not intended for data type "UNION": {0} |
| `WrnReservedUnusedKeyword` | 543 | Wrn_ReservedUnusedKeyword | — | The name '{0}' is a reserved keyword in the IEC61131-3 standard. An error will be reported in future versions. |
| `ErrGenericOnWrongPosition` | 544 | Err_GenericOnWrongPosition | — | VAR_GENERIC declaration only allowed in Functionblocks after the function block name |
| `ErrGenericOnlyConst` | 545 | Err_GenericOnlyConst | — | Only CONSTANT generics are supported in VAR_GENERIC declaration |
| `ErrTypeIsNotGeneric` | 546 | Err_TypeIsNotGeneric | — | '{0}' is not a generic functionblock. |
| `ErrGenericWrongNumberOfInitializer` | 547 | Err_GenericWrongNumberOfInitializer | — | Generic Functionblock '{0}' expects exactly '{1}' number of Generic Constant Definitions |
| `ErrGenericNotConstant` | 548 | Err_GenericNotConstant | — | The definition '{0}' for the Generic Constant '{1}' is no constant value |
| `ErrNoStaticVariableInitialisationForValue` | 549 | Err_NoStaticVariableInitialisationForValue | — | Initialisation of static variable '{0}' not constant or replaced constants is disabled |
| `ErrAttributeNotAllowedFor` | 550 | Err_AttributeNotAllowedFor | — | Attribute '{0}' not allowed for '{1}' |
| `ErrGenericNoInteger` | 551 | Err_GenericNoInteger | — | Only integer types are allowed for Generic Constants |
| `ErrNoOutsideAccessToGenericVariable` | 552 | Err_NoOutsideAccessToGenericVariable | — | No external access to Generic Constant '{0}' of Functionblock '{1}' |
| `ErrGenericDeclarationProducesErrorInGeneratedCode` | 553 | Err_GenericDeclarationProducesErrorInGeneratedCode | — | The type declaration '{0}' leads to errors in the Generic Functionblock code |
| `ErrNoExplicitCall` | 554 | Err_NoExplicitCall | — | No explicit calls for '{0}' allowed. {1}.  |
| `WrnNonAsciiStringLiteral` | 555 | Wrn_NonAsciiStringLiteral | — | The string literal '{0}...' contains non-representable characters. The project option 'UTF-8 Encoding for STRING' could be used. |
| `ErrNoGenericInstanceInVarConst` | 556 | Err_NoGenericInstanceInVarConst | — | Instances of a Generic Functionblock cannot be declared as constant |
| `ErrFCallWrongNumberOfArguments` | 557 | Err_FCallWrongNumberOfArguments | — | FCall expects exactly 2+{0} arguments. |
| `ErrFCallWrongCallPatternSignature` | 558 | Err_FCallWrongCallPatternSignature | — | The first argument of FCall must be a signature type that is used as pattern for the call. |
| `ErrFCallExpectedPointerAsSecondArgument` | 559 | Err_FCallExpectedPointerAsSecondArgument | — | The second argument of FCall must be a pointer to a function. |
| `ErrNewOnInterfaceNotPossible` | 560 | Err_NewOnInterfaceNotPossible | — | __NEW is not possible on interfaces |
| `WrnCallRecursion` | 561 | Wrn_CallRecursion | — | Call recursion: {0} |
| `ErrPartialAccessOnlyOnBitTypes` | 562 | Err_PartialAccess_OnlyOnBitTypes | — | Partial access is only supported on ANY_BIT types and not on '{0}' |
| `ErrPartialAccessNotAValidComponent` | 563 | Err_PartialAccess_NotAValidComponent | — | '%{1}{2}' is not a valid partial component of '{0}' |
| `WrnReferenceToUninitializedVariable` | 564 | Wrn_ReferenceToUninitializedVariable | — | A reference to uninitialized variable {0} is used for initialization of {1}. Accessing the uninitialized variable may result in unexpected behavior. |
| `WrnWrongDestructor` | 565 | Wrn_WrongDestructor | — | The FB_Exit method of a function block or struct must have a single input 'bInCopyCode' of type BOOL and a return value of type BOOL. |
| `WrnWrongReInit` | 566 | Wrn_WrongReInit | — | The FB_ReInit method of a function block or struct must have no inputs and a return value of type BOOL. The FB_ReInit will not be called automatically! |
| `WrnNotAllowedInInterfaceLib` | 567 | Wrn_NotAllowedInInterfaceLib | — | '{0}' not allowed in interface library |
| `WrnInterfaceChangedBase` | 568 | Wrn_InterfaceChangedBase | — | Interface of overridden method '{0}' of base '{1}' doesn't match declaration |
| `WrnMissingInstancePathForPersistent` | 569 | Wrn_MissingInstancePathForPersistent | — | No matching instance path in VAR_PERSISTENT list found for variable {0}. Use the command "Add all instance paths" to add all instance paths to the VAR_PERSISTENT list. (See Help for details) |
| `ErrProjectDefinedNotSupportedFor` | 570 | Err_ProjectDefinedNotSupportedFor | — | The condition 'project_defined' is not supported for this syntax, since it might affect the public interface of '{0}'. |
| `WrnExitForRetainInstances` | 571 | Wrn_ExitForRetainInstances | — | FB_EXIT of instances in VAR_RETAIN is also called during Reset warm, but not FB_INIT. Avoid retain declaration of function blocks with FB_EXIT! |
| `WrnUninitialisedVariableUsedInInitialisation` | 572 | Wrn_UninitialisedVariableUsedInInitialisation | — | The uninitialized variable {0} is used for initialization of {1}. Use the attribute 'global_init_slot' to change the order of initialisation. |
| `WrnAbstractKeywordMissing` | 573 | Wrn_AbstractKeywordMissing | — | The ABSTRACT keyword is missing |
| `ErrNoOnlineChangeOnGenericConstantType` | 574 | Err_NoOnlineChangeOnGenericConstantType | — | In declaration of Variable {0}, the value for the constant {1} changed from {2} to {3}: no online change possible! |
| `ErrNoOnlineChangeOnGenericConstantBaseType` | 575 | Err_NoOnlineChangeOnGenericConstantBaseType | — | In declaration of BaseType {0}, the value for the constant {1} changed from {2} to {3}: no online change possible! |
| `ErrAccessVarInstFromOutsideTheDeclaringMethod` | 576 | Err_AccessVarInstFromOutsideTheDeclaringMethod | — | Cannot access VAR_INST '{0}' of '{1}' from outside the declaring method |
| `ErrNoCopyCodeForVariableAtDirectAddress` | 577 | Err_NoCopyCodeForVariableAtDirectAddress | — | a variable at a direct address cannot change its type during online change |
| `ErrUnexpectedStatement` | 578 | Err_UnexpectedStatement | — | Unexpected statement |
| `ErrUnsupportedFeature` | 579 | Err_UnsupportedFeature | — | The compiler feature '{0}' is only supported with compiler version {1} or newer |
| `WrnConstantInStructDeclaration` | 580 | Wrn_ConstantInStructDeclaration | — | The keyword CONSTANT is ignored in the declaration of a structure or union |
| `ErrNoMatchingOverload` | 581 | Err_NoMatchingOverload | — | No Matching Overload found for method '{0}' |
| `ErrOverloadNeedsAttribute` | 582 | Err_OverloadNeedsAttribute | — | There is another method with the name '{0}'. Use the Attribute {{attribute 'overloaded'}}, if you want to define overloaded methods. |
| `ErrOverloadWithSameInputs` | 583 | Err_OverloadWithSameInputs | — | There is another overload for '{0}' with the same input-types. |
| `ErrMaxNestingDepthExceeded` | 584 | Err_MaxNestingDepthExceeded | — | Maximum nesting depth exceeded.  |
| `ErrGenericParamsAllExplicitOrNone` | 585 | Err_GenericParamsAllExplicitOrNone | — | Either all generic variables need to be explicitly assigned or none |
| `ErrGenericParamMissing` | 586 | Err_GenericParamMissing | — | Missing initialization for generic variable '{0}' |
| `ErrGenericParamUnknown` | 587 | Err_GenericParamUnknown | — | '{0}' is no declared generic constant of function block '{1}' |
| `ErrMissingImplementationTerminator` | 588 | Err_MissingImplementationTerminator | — | Could not find the terminator '{0}' for the implementation block. |
| `ErrUnknownEmbeddedLanguageType` | 589 | Err_UnknownEmbeddedLanguageType | — | Unknown embedded language type '{0}'. |
| `ErrNoResolutionForSomeLazyVariables` | 590 | Err_NoResolutionForSomeLazyVariables | — | Type of lazy variables in this signature could not be inferred. |
| `WrnChangeOfAccessModifier` | 591 | Wrn_ChangeOfAccessModifier | — | Method {0}.{1} changes access modifier "{2}" when overriding method {3}.{1} |

## (e) MessageId, используемые Parser35220 (`AddErrorST*`)

Извлечено из декомпилированных исходников `C:\Codesys\Parser35220.plugin` (обёртки `AddErrorST`, `AddErrorSTWithToken`, `AddErrorSTAndAdjustSourcePosition`).

| MessageId | Key | RU | EN |
|---|---|---|---|
| 1 | Err_ConstantOverflow | Константа '{0}' слишком велика для типа '{1}' | Constant '{0}' too large for type '{1}' |
| 2 | Err_Operator1of2Expected | '{0}' или '{1}' требуется вместо '{2}' | '{0}' or '{1}' expected instead of '{2}' |
| 3 | Err_BitNrOverflow | '{0}' не является корректным битовым номером для '{1}' | '{0}' is no valid bit number for '{1}' |
| 4 | Err_NoComponentOf | '{0}' не является компонентом '{1}' | '{0}' is no component of '{1}' |
| 5 | Err_OverflowInAddress | Постоянное переполнение по адресу '{0}' | Constant overflow in address '{0}' |
| 6 | Err_OperatorExpected | '{0}' требуется вместо '{1}' | '{0}' expected instead of '{1}' |
| 7 | Err_ExpressionExpectedInstead | Вместо '{0}' требуется выражение | Expression expected instead of '{0}' |
| 8 | Err_Operator1of3ExpectedInsteadofEOF | Недопустимый End-of-file: требуется '{0}', '{1}' или '{2}' | Unexpected End-of-file found: '{0}', '{1}' or '{2}' expected |
| 9 | Err_UnexpectedTokenFound | Обнаружен недопустимый символ '{0}' | Unexpected token '{0}' found |
| 10 | Err_OperatorExpectedInsteadofEOF | Недопустимый End-of-file: требуется '{0}' | Unexpected End-of-file found: '{0}' expected |
| 11 | Err_NoCaseLabelFound | Метка не найдена | No CASE label found |
| 22 | Err_OpNeedsExactInputs | Для '{0}' требуется ровно '{1}' операндов | '{0}' needs exactly '{1}' operands |
| 24 | Err_IllegalOperator | '{0}' не является корректным ST-оператором | '{0}' is no valid ST operator |
| 26 | Err_IdentifierExpected | Вместо '{0}' требуется идентификатор | Identifier expected instead of '{0}' |
| 27 | Err_StringSizeExpected | После "(" требуется размер строки | size of string expected after "(" |
| 30 | Err_AddressExpected | После "AT" вместо {0} требуется прямой адрес | Direct address expected after AT instead of {0} |
| 31 | Err_TypeExpected | Вместо '{0}' требуется определение типа | Type definition expected instead of '{0}' |
| 51 | Err_AttributeNameExpected | Для значения атрибута вместо '{0}' требуется однобайтовая строка | Single byte string expected for an attribute value instead of '{0}' |
| 81 | Err_UnexpectedPragmaif | Недопустимая директива: '{0}' обнаружено без соответствующего 'if' | Unexpected pragma: '{0}' found without matching 'if' |
| 85 | Err_DefineValueExpected | Значение требуется вместо '{0}' | Define value expected instead of '{0}' |
| 98 | Err_FunctionBlockNoLongerValid | Ключевое слово FUNCTIONBLOCK больше не поддерживается. Используйте FUNCTION_BLOCK | The keyword FUNCTIONBLOCK is no longer supported. Use FUNCTION_BLOCK instead. |
| 114 | Err_InvalidJumpDestination | Некорректная цель {0} для JMP | Invalid destination {0} for JMP |
| 115 | Err_CalcNeedsCall | Второй параметр условного вызова должен соответствующим оператором вызова | Second parameter of conditional call must be a valid call statement |
| 182 | Err_ReturnTypeForNonFunction | Возвращаемый тип допустим только для POU типа FUNCTION и METHOD | Return type is only possible for POUs of type FUNCTION and METHOD |
| 189 | Err_SemicolonExpected | ';' требуется вместо '{0}' | ';' expected instead of '{0}' |
| 190 | Err_SemicolonExpectedInsteadOfEnd | Вместо конца POU требуется ';' | ';' expected instead of end of POU |
| 205 | Err_NoPointerToBit | POINTER TO BIT недопустим | POINTER TO BIT is not allowed |
| 206 | Err_NoArrayOfBit | BIT недопустим в качестве базового типа массива | BIT is not allowed as base type of an array |
| 248 | Err_NewNeedsType | Определение типа требуется в качестве операнда для __NEW | Type definition expected as operand for __NEW |
| 261 | Err_ReferenceNotAllowed | Ссылочный тип недопустим в качестве базового типа массива, указателя или ссылки | A reference type is not allowed as base type of an array, pointer, or reference |
| 272 | Err_NoReferenceToBits | Ссылки на биты недопустимы | References to bits are not possible |
| 303 | Err_StructureInitialisationNotPossible | Инициализация структуры недопустима в качестве параметра вызова Init-функции. Используйте переменную. | A structure initialisation is not possible as Parameter of an Init-function call. Use a variable instead. |
| 304 | Err_ArrayInitialisationNotPossible | Инициализация массива недопустима в качестве параметра вызова Init-функции. Используйте переменную | An array initialisation is not possible as parameter of an initial function call. Use a variable instead |
| 311 | Err_AnyTypeOnlyInFunction | Переменные типа '{0}' допустимы только в качестве входов функции. | Variables of type '{0}' only allowed as input of functions |
| 317 | Err_LiteralExpected | Вместо '{0}' требуется литерал | Literal expected instead of '{0}' |
| 372 | Err_DuplicateElseInCaseStatement | Повторное определение блока CASE | Duplicate definition of ELSE in CASE statement |
| 386 | Err_VarLengthArrayTopLevel | — | A variable length array type has to be on top level position of a type declaration |
| 449 | Err_ComparisonOperatorExpected | — | Comparison operator expected instead of '{0}' |
| 450 | Err_StringLiteralExpected | — | String literal expected instead of '{0}' |
| 451 | Err_VersionOverflow | — | At least one part of the version '{0}' has a too big value |
| 452 | Err_VersionPartNegative | — | At least one part of the version '{0}' has a negative value |
| 453 | Err_VersionInvalidFormat | — | Token '{0}' is no valid version |
| 500 | Err_VectorSizeNotValid | — | The size of a vector must be a integer greater than 0 and less than 9 |
| 570 | Err_ProjectDefinedNotSupportedFor | — | The condition 'project_defined' is not supported for this syntax, since it might affect the public interface of '{0}'. |
| 578 | Err_UnexpectedStatement | — | Unexpected statement |
| 579 | Err_UnsupportedFeature | — | The compiler feature '{0}' is only supported with compiler version {1} or newer |
| 584 | Err_MaxNestingDepthExceeded | — | Maximum nesting depth exceeded.  |
| 588 | Err_MissingImplementationTerminator | — | Could not find the terminator '{0}' for the implementation block. |

## (f) Пробелы (до расширения RU): MessageId без текста ни в одной официальной локали (15)

| MessageId | Key |
|---|---|
| 200 | Wrn_PlaceholderNotResolved |
| 210 | Wrn_InsertSpecialPersistent |
| 223 | Wrn_CompoRefAssignCompatibilityWarning |
| 245 | Wrn_MissingObjectForPersistent |
| 315 | Wrn_StringTooShortForVarInOut |
| 349 | Wrn_InterfaceInVarInOut |
| 350 | Wrn_ReferenceToInterface |
| 362 | Err_InvalidStringSize |
| 370 | Wrn_InstanceCalledMoreThenOnce |
| 394 | Wrn_FBExitCalledForStackInstance |
| 404 | Wrn_CompilerVersionDeprecated |
| 410 | Wrn_CompatibilityProblemForRefProperty |
| 508 | Wrn_Ambiguity |
| 510 | Err_ConfiguredCompilerVersionNotAvailable |
| 523 | Err_InconsistentUseOfCPPCompatibilityMissingParent |

Эти 15 `MessageId` не имеют текста **ни в одной** локали, включая нейтральный встроенный ресурс main-плагина `_3S.CoDeSys.Compiler35220.Resources.ErrorMessages.resources` (ровно 500 записей). Тексты для них отсутствуют в поставке ⇒ при генерации такого сообщения доступно только имя/код, локализованного текста нет.

### Ключи ресурса, отсутствующие в enum `MessageId` (9)

- Err_GenericNoInitialValueSupported
- Err_InconsistentUseOfCPPCompatibility_MissingParent
- Err_MissingObjectForPersistent
- Err_RelatedPositionInterface
- Inf_RelatedPositionRecursion
- Inf_RelatedPositionStackoverflow
- Info_PersistentMemoryConfiguration
- PublishSymbolsMustBeSet
- Wrn_InvalidStringSize

### WhiteParseTrees (EN, не локализованы, 10 сообщений)

См. `C:\Codesys\resources\whiteparsetrees.json`. Ключи: FailedToParseDeclaration, FailedToParseStatement, FailedToParseAssignmentExpression, UnexpectedToken, UnexpectedOperator, UnexpectedEndOfInput, UnexpectedTokenExpected, UnexpectedOperatorExpected, ExpectedOneOf, InternalError.

### Нелокализованные в RU (180 ключей)

Полный список: ключи из union, отсутствующие в `error_messages_ru.csv` (RU содержит 320 из 500; для них используется английский текст).


## (g) Расширение русского перевода до 507/507 (100%)

Штатный ресурс `ru` содержит 320 ключей; для 100% покрытия добавлены официальные переводы из других RU-спутников и авторские переводы.

| Источник | Кол-во | Комментарий |
|---|---|---|
| native (`Compiler35220.ru`) | 317 | штатный ресурс (3 из 320 — orphan-ключи вне enum) |
| harvest (другие RU-спутники) | 133 | официальные переводы CODESYS |
| authored (ручной перевод) | 57 | нет RU ни в одном спутнике |
| **Итого MessageId** | **507** | **100%** |

Harvest по компиляторам: Compiler35170=123, Compiler35200=10

Файлы: `tables\errors\error_messages_ru.csv` (507 строк), `tables\errors\error_messages_ru_provenance.csv` (id,key,text,source), `tables\errors\error_messages.json` (поле `ru` заполнено для всех 507).

### Авторские переводы (57) — не найдено ни в одном RU-спутнике

| ID | Key | RU (новый) | EN (источник) |
|---|---|---|---|
| 0 | None | Нет | — |
| 245 | Wrn_MissingObjectForPersistent | Объект для persistent-переменной «{0}» не найден | — |
| 362 | Err_InvalidStringSize | Недопустимый размер строки «{0}» | — |
| 454 | Err_NoNewAssignmentInOtherExpression | Недопустимо использовать выражение присваивания с оператором __NEW внутри другого выражения. Используйте вместо этого переменную-указатель. | It is not possible to use an assignment expression with the __NEW operator in another expression. Use the pointer variable instead. |
| 455 | Err_ExplicitTransitionAssignMissing | Код перехода содержит несколько операторов. Требуется явное присваивание выходной переменной перехода. | The code of transition contains multiple statements. The explicit assignment to transition output variable is required. |
| 456 | Wrn_AmbiguousCheckfunctionInLibrary | Неявная проверочная функция {0} скрыта другой неявной проверочной функцией {1}. Проверки выполняться не будут! Устраните конфликт и очистите приложение, чтобы использовать проверочную функцию. | The implicit check function {0} is hidden by another implicit check function {1}. Checks will not be performed! Resolve the conflict and clean the application to use the check function. |
| 508 | Wrn_Ambiguity | Неоднозначное использование имени «{0}» | — |
| 510 | Err_ConfiguredCompilerVersionNotAvailable | Настроенная версия компилятора «{0}» недоступна | — |
| 523 | Err_InconsistentUseOfCPPCompatibilityMissingParent | Непоследовательное использование совместимости с C++. Отсутствует атрибут для родительского POU «{0}». | — |
| 544 | Err_GenericOnWrongPosition | Объявление VAR_GENERIC допустимо в функциональных блоках только после имени функционального блока | VAR_GENERIC declaration only allowed in Functionblocks after the function block name |
| 545 | Err_GenericOnlyConst | В объявлении VAR_GENERIC поддерживаются только константы (CONSTANT) | Only CONSTANT generics are supported in VAR_GENERIC declaration |
| 546 | Err_TypeIsNotGeneric | «{0}» не является обобщённым функциональным блоком. | '{0}' is not a generic functionblock. |
| 547 | Err_GenericWrongNumberOfInitializer | Обобщённый функциональный блок «{0}» ожидает ровно «{1}» определений обобщённых констант | Generic Functionblock '{0}' expects exactly '{1}' number of Generic Constant Definitions |
| 548 | Err_GenericNotConstant | Определение «{0}» для обобщённой константы «{1}» не является константным значением | The definition '{0}' for the Generic Constant '{1}' is no constant value |
| 549 | Err_NoStaticVariableInitialisationForValue | Инициализация статической переменной «{0}» не является константой либо замена констант отключена | Initialisation of static variable '{0}' not constant or replaced constants is disabled |
| 550 | Err_AttributeNotAllowedFor | Атрибут «{0}» недопустим для «{1}» | Attribute '{0}' not allowed for '{1}' |
| 551 | Err_GenericNoInteger | Для обобщённых констант допускаются только целочисленные типы | Only integer types are allowed for Generic Constants |
| 552 | Err_NoOutsideAccessToGenericVariable | Внешний доступ к обобщённой константе «{0}» функционального блока «{1}» невозможен | No external access to Generic Constant '{0}' of Functionblock '{1}' |
| 553 | Err_GenericDeclarationProducesErrorInGeneratedCode | Объявление типа «{0}» приводит к ошибкам в коде обобщённого функционального блока | The type declaration '{0}' leads to errors in the Generic Functionblock code |
| 554 | Err_NoExplicitCall | Явные вызовы для «{0}» недопустимы. {1}. | No explicit calls for '{0}' allowed. {1}.  |
| 555 | Wrn_NonAsciiStringLiteral | Строковый литерал «{0}...» содержит непредставимые символы. Можно использовать опцию проекта «Кодировка UTF-8 для STRING». | The string literal '{0}...' contains non-representable characters. The project option 'UTF-8 Encoding for STRING' could be used. |
| 556 | Err_NoGenericInstanceInVarConst | Экземпляры обобщённого функционального блока нельзя объявлять как константы | Instances of a Generic Functionblock cannot be declared as constant |
| 557 | Err_FCallWrongNumberOfArguments | FCall ожидает ровно 2+{0} аргументов. | FCall expects exactly 2+{0} arguments. |
| 558 | Err_FCallWrongCallPatternSignature | Первый аргумент FCall должен быть сигнатурным типом, используемым в качестве шаблона вызова. | The first argument of FCall must be a signature type that is used as pattern for the call. |
| 559 | Err_FCallExpectedPointerAsSecondArgument | Второй аргумент FCall должен быть указателем на функцию. | The second argument of FCall must be a pointer to a function. |
| 560 | Err_NewOnInterfaceNotPossible | __NEW недопустим для интерфейсов | __NEW is not possible on interfaces |
| 561 | Wrn_CallRecursion | Рекурсия вызова: {0} | Call recursion: {0} |
| 562 | Err_PartialAccess_OnlyOnBitTypes | Частичный доступ поддерживается только для типов ANY_BIT, но не для «{0}» | Partial access is only supported on ANY_BIT types and not on '{0}' |
| 563 | Err_PartialAccess_NotAValidComponent | «%{1}{2}» не является допустимым частичным компонентом «{0}» | '%{1}{2}' is not a valid partial component of '{0}' |
| 564 | Wrn_ReferenceToUninitializedVariable | Ссылка на неинициализированную переменную {0} используется для инициализации {1}. Обращение к неинициализированной переменной может привести к непредсказуемому поведению. | A reference to uninitialized variable {0} is used for initialization of {1}. Accessing the uninitialized variable may result in unexpected behavior. |
| 565 | Wrn_WrongDestructor | Метод FB_Exit функционального блока или структуры должен иметь единственный вход «bInCopyCode» типа BOOL и возвращаемое значение типа BOOL. | The FB_Exit method of a function block or struct must have a single input 'bInCopyCode' of type BOOL and a return value of type BOOL. |
| 566 | Wrn_WrongReInit | Метод FB_ReInit функционального блока или структуры не должен иметь входов и должен возвращать значение типа BOOL. FB_ReInit не будет вызываться автоматически! | The FB_ReInit method of a function block or struct must have no inputs and a return value of type BOOL. The FB_ReInit will not be called automatically! |
| 567 | Wrn_NotAllowedInInterfaceLib | «{0}» недопустимо в интерфейсной библиотеке | '{0}' not allowed in interface library |
| 568 | Wrn_InterfaceChangedBase | Интерфейс перезаписанного метода «{0}» основы «{1}» не соответствует объявлению | Interface of overridden method '{0}' of base '{1}' doesn't match declaration |
| 569 | Wrn_MissingInstancePathForPersistent | Для переменной {0} не найден подходящий путь экземпляра в списке VAR_PERSISTENT. Используйте команду «Добавить все пути экземпляров», чтобы добавить все пути экземпляров в список VAR_PERSISTENT. (Подробнее см. справку) | No matching instance path in VAR_PERSISTENT list found for variable {0}. Use the command "Add all instance paths" to add all instance paths to the VAR_PERSISTENT list. (See Help for details) |
| 570 | Err_ProjectDefinedNotSupportedFor | Условие «project_defined» не поддерживается для данного синтаксиса, поскольку может повлиять на открытый интерфейс «{0}». | The condition 'project_defined' is not supported for this syntax, since it might affect the public interface of '{0}'. |
| 571 | Wrn_ExitForRetainInstances | FB_EXIT экземпляров в VAR_RETAIN вызывается также при тёплом сбросе (Reset warm), а FB_INIT — нет. Избегайте объявления энергонезависимыми функциональных блоков с FB_EXIT! | FB_EXIT of instances in VAR_RETAIN is also called during Reset warm, but not FB_INIT. Avoid retain declaration of function blocks with FB_EXIT! |
| 572 | Wrn_UninitialisedVariableUsedInInitialisation | Неинициализированная переменная {0} используется для инициализации {1}. Чтобы изменить порядок инициализации, используйте атрибут «global_init_slot». | The uninitialized variable {0} is used for initialization of {1}. Use the attribute 'global_init_slot' to change the order of initialisation. |
| 573 | Wrn_AbstractKeywordMissing | Отсутствует ключевое слово ABSTRACT | The ABSTRACT keyword is missing |
| 574 | Err_NoOnlineChangeOnGenericConstantType | В объявлении переменной {0} значение константы {1} изменилось с {2} на {3}: онлайн-замена невозможна! | In declaration of Variable {0}, the value for the constant {1} changed from {2} to {3}: no online change possible! |
| 575 | Err_NoOnlineChangeOnGenericConstantBaseType | В объявлении базового типа {0} значение константы {1} изменилось с {2} на {3}: онлайн-замена невозможна! | In declaration of BaseType {0}, the value for the constant {1} changed from {2} to {3}: no online change possible! |
| 576 | Err_AccessVarInstFromOutsideTheDeclaringMethod | Невозможно обратиться к VAR_INST «{0}» из «{1}» извне объявляющего метода | Cannot access VAR_INST '{0}' of '{1}' from outside the declaring method |
| 577 | Err_NoCopyCodeForVariableAtDirectAddress | переменная с прямым адресом не может изменить свой тип при онлайн-замене | a variable at a direct address cannot change its type during online change |
| 578 | Err_UnexpectedStatement | Неожиданный оператор | Unexpected statement |
| 579 | Err_UnsupportedFeature | Компиляторная функция «{0}» поддерживается только начиная с версии компилятора {1} | The compiler feature '{0}' is only supported with compiler version {1} or newer |
| 580 | Wrn_ConstantInStructDeclaration | Ключевое слово CONSTANT игнорируется в объявлении структуры или объединения | The keyword CONSTANT is ignored in the declaration of a structure or union |
| 581 | Err_NoMatchingOverload | Для метода «{0}» не найдено подходящей перегрузки | No Matching Overload found for method '{0}' |
| 582 | Err_OverloadNeedsAttribute | Существует другой метод с именем «{0}». Используйте атрибут {{attribute 'overloaded'}}, если хотите определить перегруженные методы. | There is another method with the name '{0}'. Use the Attribute {{attribute 'overloaded'}}, if you want to define overloaded methods. |
| 583 | Err_OverloadWithSameInputs | Существует другая перегрузка для «{0}» с теми же типами входов. | There is another overload for '{0}' with the same input-types. |
| 584 | Err_MaxNestingDepthExceeded | Превышена максимальная глубина вложенности. | Maximum nesting depth exceeded.  |
| 585 | Err_GenericParamsAllExplicitOrNone | Либо все обобщённые переменные должны быть заданы явно, либо ни одна | Either all generic variables need to be explicitly assigned or none |
| 586 | Err_GenericParamMissing | Отсутствует инициализация обобщённой переменной «{0}» | Missing initialization for generic variable '{0}' |
| 587 | Err_GenericParamUnknown | «{0}» не является объявленной обобщённой константой функционального блока «{1}» | '{0}' is no declared generic constant of function block '{1}' |
| 588 | Err_MissingImplementationTerminator | Не удалось найти терминатор «{0}» для блока реализации. | Could not find the terminator '{0}' for the implementation block. |
| 589 | Err_UnknownEmbeddedLanguageType | Неизвестный тип встраиваемого языка «{0}». | Unknown embedded language type '{0}'. |
| 590 | Err_NoResolutionForSomeLazyVariables | Не удалось вывести тип ленивых переменных в этой сигнатуре. | Type of lazy variables in this signature could not be inferred. |
| 591 | Wrn_ChangeOfAccessModifier | Метод {0}.{1} изменяет модификатор доступа «{2}» при перезаписи метода {3}.{1} | Method {0}.{1} changes access modifier "{2}" when overriding method {3}.{1} |

### Восстановлено из других компиляторов (133)

| ID | Key | Источник |
|---|---|---|
| 93 | Err_SomethingOverridingMethodBase | Compiler35170 |
| 119 | Err_WrongConstructor | Compiler35170 |
| 120 | Err_WrongDestructor | Compiler35170 |
| 200 | Wrn_PlaceholderNotResolved | Compiler35200 |
| 210 | Wrn_InsertSpecialPersistent | Compiler35200 |
| 223 | Wrn_CompoRefAssignCompatibilityWarning | Compiler35200 |
| 244 | Err_MissingInstancePathForPersistent | Compiler35170 |
| 283 | Err_NoOverrideOnFinalMethod | Compiler35170 |
| 284 | Err_NoOverrideOnPrivateMethod | Compiler35170 |
| 285 | Err_NoOverrideOnInternalMethod | Compiler35170 |
| 297 | Err_StackOverflowDetected | Compiler35170 |
| 298 | Wrn_StackCheckIncompleteDueToRecursion | Compiler35170 |
| 300 | Err_RelatedPositionTaskX | Compiler35170 |
| 315 | Wrn_StringTooShortForVarInOut | Compiler35200 |
| 349 | Wrn_InterfaceInVarInOut | Compiler35200 |
| 350 | Wrn_ReferenceToInterface | Compiler35200 |
| 361 | Err_NoMixExternalIECInheritance | Compiler35170 |
| 368 | Err_InvalidEnumDefaultValue | Compiler35170 |
| 370 | Wrn_InstanceCalledMoreThenOnce | Compiler35200 |
| 374 | Err_DivisionByZero | Compiler35170 |
| 375 | Err_InvalidEnumBaseType | Compiler35170 |
| 376 | Err_TooFewParametersForExtensibleFunction | Compiler35170 |
| 377 | Err_TooManyParametersForExtensibleFunction | Compiler35170 |
| 378 | Err_NoFormalParamsCallsForExtensibleFunction | Compiler35170 |
| 379 | Err_IntegerLiteralExpected | Compiler35170 |
| 380 | Err_LowerUpperBoundOnVariableLengthArrayOnly | Compiler35170 |
| 381 | Err_OperatorNoValidVariableName | Compiler35170 |
| 382 | Err_InconsistentInheritanceOfCPPCompatibility | Compiler35170 |
| 383 | Err_IncompletePOUDeclaration | Compiler35170 |
| 384 | Err_SelMuxOnlyEqualUserDefTypes | Compiler35170 |
| 385 | Err_VarLengthArrayInOut | Compiler35170 |
| 386 | Err_VarLengthArrayTopLevel | Compiler35170 |
| 387 | Err_OutParamNotEqual | Compiler35170 |
| 388 | Wrn_LibWithStringInVarInOut | Compiler35170 |
| 390 | Err_AddressSourceIsAddressDest | Compiler35170 |
| 392 | Err_ATDeclarationNotAllowed | Compiler35170 |
| 393 | Err_ArrayBorderNoValidSignedInteger | Compiler35170 |
| 394 | Wrn_FBExitCalledForStackInstance | Compiler35200 |
| 395 | Err_NumOfInitializersDoNotMatch | Compiler35170 |
| 396 | Err_IsValidRefNeedsReference | Compiler35170 |
| 397 | Err_ImplicitMethodImplementationNotPublic | Compiler35170 |
| 399 | Err_ImplicitReferenceTypeDeclNotAllowed | Compiler35170 |
| 400 | Err_ImplicitRefTypeIsnotAllowedAsBase | Compiler35170 |
| 401 | Err_ImplicitRefTypeAllClassesNeedAttrib | Compiler35170 |
| 402 | Err_ImplicitRefTypeDeclarationNotAllowed | Compiler35170 |
| 403 | Err_ImplicitRefTypeSignNotSupported | Compiler35170 |
| 404 | Wrn_CompilerVersionDeprecated | Compiler35200 |
| 405 | Err_MultipleAssignmentsToInterfaceVariables | Compiler35170 |
| 406 | Wrn_ImplicitCheckFunctionShadowed | Compiler35170 |
| 407 | Err_AddressOfNonInstanceVar | Compiler35170 |
| 408 | Err_ImplicitReferenceTypeOnlChangeError | Compiler35170 |
| 409 | Err_NoResolutionForLazyVariable | Compiler35170 |
| 410 | Wrn_CompatibilityProblemForRefProperty | Compiler35200 |
| 411 | Err_NoVarInputInPropertyAccessors | Compiler35170 |
| 412 | Err_MultipleAssignsToSameInputInCall | Compiler35170 |
| 413 | Err_RefAssignOnlyForReferenceTypes | Compiler35170 |
| 414 | Err_OutOfPersistentMemoryImplicit | Compiler35170 |
| 415 | Err_OutOfPersistentMemoryExplicit | Compiler35170 |
| 416 | Err_LibraryNamespaceNotValid | Compiler35170 |
| 419 | Err_NoInputsWithSlotAttributes | Compiler35170 |
| 420 | Err_NotEnoughMemoryForCompactDownload | Compiler35170 |
| 421 | Wrn_ExtendsForInterfaces | Compiler35170 |
| 422 | Wrn_GranularityMismatchForDirectVariable | Compiler35170 |
| 423 | Err_MultipleAssignmentWithReferences | Compiler35170 |
| 424 | Err_MultipleAssignmentWithChangingValueTypes | Compiler35170 |
| 425 | Err_NoMemoryReserveForExternal | Compiler35170 |
| 427 | Err_UnknownMaxStackSize | Compiler35170 |
| 428 | Err_AbstractOnFunctionBlocksAndMethodsOnly | Compiler35170 |
| 429 | Err_AbstractAndFinalNotPossible | Compiler35170 |
| 430 | Err_AbstractAndPrivateNotPossible | Compiler35170 |
| 431 | Err_AbstractMethodNotImplemented | Compiler35170 |
| 432 | Err_AbstractMethodOnlyInAbstractFunctionblock | Compiler35170 |
| 433 | Err_AbstractMethodMustNotContainAnyStatements | Compiler35170 |
| 434 | Err_AbstractFunctionBlockInstance | Compiler35170 |
| 435 | Err_AbstractPropertyNotImplemented | Compiler35170 |
| 436 | Err_StringLengthIsNoConstant | Compiler35170 |
| 437 | Err_NotAllowedInTaskLocalVariables | Compiler35170 |
| 438 | Err_TaskLocalVariablesWriterTaskNotDefined | Compiler35170 |
| 439 | Err_TaskLocalVariablesAccessNotAllowed | Compiler35170 |
| 440 | Err_TaskLocalVariablesNoOnlineChangePossible | Compiler35170 |
| 441 | Wrn_VarInOutUnitializedInInitialValue | Compiler35170 |
| 442 | Err_AbstractMethodStaticCall | Compiler35170 |
| 443 | Err_AbstractPropertyStaticCall | Compiler35170 |
| 444 | Err_InconsistentUseOfCPPCompatibility | Compiler35170 |
| 445 | Err_AbstractWrongVarInMethod | Compiler35170 |
| 446 | Err_NoValuePassingForCPPExternal | Compiler35170 |
| 447 | Wrn_OnlyConstantInitialValueForMappedPersistentVar | Compiler35170 |
| 448 | Err_OperatorNotSupportedVersion | Compiler35170 |
| 449 | Err_ComparisonOperatorExpected | Compiler35170 |
| 450 | Err_StringLiteralExpected | Compiler35170 |
| 451 | Err_VersionOverflow | Compiler35170 |
| 452 | Err_VersionPartNegative | Compiler35170 |
| 453 | Err_VersionInvalidFormat | Compiler35170 |
| 500 | Err_VectorSizeNotValid | Compiler35170 |
| 501 | Err_VectorSizeIsNoConstant | Compiler35170 |
| 502 | Err_VectorNoPersistentRetain | Compiler35170 |
| 503 | Err_VectorBaseMustBeRealType | Compiler35170 |
| 504 | Err_VectorTypesNotCompatible | Compiler35170 |
| 505 | Err_VectorTypeCantBePlacedInUnion | Compiler35170 |
| 506 | Err_LowerUpperBoundOperandNotInRange | Compiler35170 |
| 507 | Err_LowerUpperBoundOperandNotExactly | Compiler35170 |
| 509 | Err_MultipleAssignmentsNotAllowedForOperator | Compiler35170 |
| 511 | Err_AbstractFunctionBlockAssigned | Compiler35170 |
| 513 | Wrn_CallOfPrivateProperty | Compiler35170 |
| 514 | Wrn_AccessToInternalProperty | Compiler35170 |
| 515 | Wrn_CallOfProtectedProperty | Compiler35170 |
| 516 | Wrn_AccessToInternalVariable | Compiler35170 |
| 517 | Wrn_AccessToInternalObject | Compiler35170 |
| 518 | Err_NoNamespace | Compiler35170 |
| 519 | Err_InvalidAccessPathForNamespaceAccess | Compiler35170 |
| 520 | Err_InvalidNamespaceForNamespaceAccess | Compiler35170 |
| 521 | Err_UnknownCompilerVersionInCompiledLib | Compiler35170 |
| 522 | Wrn_PersistentVariableOnStack | Compiler35170 |
| 524 | Err_WrongReInit | Compiler35170 |
| 525 | Wrn_InvalidDefaultValue | Compiler35170 |
| 526 | Wrn_DefaultValueNotConstant | Compiler35170 |
| 527 | Wrn_DefaultValueTopLevel | Compiler35170 |
| 528 | Err_NoBranchOutOfFinally | Compiler35170 |
| 529 | Err_NoExitOrContinueOutOfTry | Compiler35170 |
| 530 | Err_NoExitOrContinueOutOfCatch | Compiler35170 |
| 531 | Err_MultipleAssignmentWithProperty | Compiler35170 |
| 532 | Err_FunNeedsAtLeastNInputs | Compiler35170 |
| 533 | Wrn_ObsoleteOutputInAbstractMethod | Compiler35170 |
| 534 | Err_WrongCallAfterGlobalInitSlotSignature | Compiler35170 |
| 535 | Inf_UseXSizeOfOperator | Compiler35170 |
| 536 | Err_InvalidInitialisationForVarInst | Compiler35170 |
| 537 | Err_NoPropertyForVarInout | Compiler35170 |
| 538 | Err_InterfaceChanged_NumberOfInputsOutputsDifferent | Compiler35170 |
| 539 | Err_InterfaceChanged_VariableDifferent | Compiler35170 |
| 540 | Wrn_MissingAttributeNoAssign | Compiler35170 |
| 541 | Err_NoMemoryAllocationCallback | Compiler35170 |
| 542 | Wrn_NoInheritanceForUnions | Compiler35170 |
| 543 | Wrn_ReservedUnusedKeyword | Compiler35170 |

