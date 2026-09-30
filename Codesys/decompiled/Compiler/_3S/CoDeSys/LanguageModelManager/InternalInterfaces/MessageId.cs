using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	public enum MessageId
	{
		[ReleasedEnumMember]
		None = 0,
		[ReleasedEnumMember]
		Err_ConstantOverflow = 1,
		[ReleasedEnumMember]
		Err_Operator1of2Expected = 2,
		[ReleasedEnumMember]
		Err_BitNrOverflow = 3,
		[ReleasedEnumMember]
		Err_NoComponentOf = 4,
		[ReleasedEnumMember]
		Err_OverflowInAddress = 5,
		[ReleasedEnumMember]
		Err_OperatorExpected = 6,
		[ReleasedEnumMember]
		Err_ExpressionExpectedInstead = 7,
		[ReleasedEnumMember]
		Err_Operator1of3ExpectedInsteadofEOF = 8,
		[ReleasedEnumMember]
		Err_UnexpectedTokenFound = 9,
		[ReleasedEnumMember]
		Err_OperatorExpectedInsteadofEOF = 10,
		[ReleasedEnumMember]
		Err_NoCaseLabelFound = 11,
		[ReleasedEnumMember]
		Err_NoValidCondition = 12,
		[ReleasedEnumMember]
		Err_AtLeastOneExpected = 13,
		[ReleasedEnumMember]
		Err_ConditionExpected = 14,
		[ReleasedEnumMember]
		Err_CounterStartExpected = 15,
		[ReleasedEnumMember]
		Err_UpperBoundExpected = 16,
		[ReleasedEnumMember]
		Err_InvalidLoopIncrement = 17,
		[ReleasedEnumMember]
		Err_IsNoLValue = 18,
		[ReleasedEnumMember]
		Err_RValueRequired = 19,
		[ReleasedEnumMember]
		Err_NoValidStatement = 20,
		[ReleasedEnumMember]
		Err_NoValidOperand = 21,
		[ReleasedEnumMember]
		Err_OpNeedsExactInputs = 22,
		[ReleasedEnumMember]
		Err_OpNeedsAtLeastInputs = 23,
		[ReleasedEnumMember]
		Err_IllegalOperator = 24,
		[ReleasedEnumMember]
		Err_Operator1of4Expected = 25,
		[ReleasedEnumMember]
		Err_IdentifierExpected = 26,
		[ReleasedEnumMember]
		Err_StringSizeExpected = 27,
		[ReleasedEnumMember]
		Err_Operator1of3Expected = 28,
		[ReleasedEnumMember]
		Err_AddressExpected = 30,
		[ReleasedEnumMember]
		Err_TypeExpected = 31,
		[ReleasedEnumMember]
		Err_TypeMismatch = 32,
		[ReleasedEnumMember]
		Wrn_PointerMisatch = 33,
		[ReleasedEnumMember]
		Err_Multiassigninfor = 34,
		[ReleasedEnumMember]
		Err_CalleeInvalidType = 35,
		[ReleasedEnumMember]
		Err_WrongObjectType = 36,
		[ReleasedEnumMember]
		Err_IsNoInput = 37,
		[ReleasedEnumMember]
		Err_IsNoOutput = 38,
		[ReleasedEnumMember]
		Err_InOutNotAssigned = 39,
		[ReleasedEnumMember]
		Err_FunNeedsNInputs = 40,
		[ReleasedEnumMember]
		Err_LValueForVarinout = 41,
		[ReleasedEnumMember]
		Err_FunctionCallMixedStyle = 42,
		[ReleasedEnumMember]
		Err_WrongFormalParameter = 43,
		[ReleasedEnumMember]
		Err_InputMissing = 44,
		[ReleasedEnumMember]
		Err_ThisNotAllowed = 45,
		[ReleasedEnumMember]
		Err_IdentNotDefined = 46,
		[ReleasedEnumMember]
		Err_IndexingInvalid = 47,
		[ReleasedEnumMember]
		Err_ArrayIndexNumWrong = 48,
		[ReleasedEnumMember]
		Err_ConstantIndexOutOfRange = 49,
		[ReleasedEnumMember]
		Err_Bitaccessnoconst = 50,
		[ReleasedEnumMember]
		Err_AttributeNameExpected = 51,
		[ReleasedEnumMember]
		Txt_ParentContextNotUpToDate = 52,
		[ReleasedEnumMember]
		Err_CompilerVersionError = 53,
		[ReleasedEnumMember]
		Err_NoBitAccessOnFunctionCall = 61,
		[ReleasedEnumMember]
		Err_CompoAccessNoStruct = 62,
		[ReleasedEnumMember]
		Err_ContainsNoDefinition = 63,
		[ReleasedEnumMember]
		Err_DerefNoPointer = 64,
		[ReleasedEnumMember]
		Err_NoGlobalDefine = 65,
		[ReleasedEnumMember]
		Err_TypesNotComparable = 66,
		[ReleasedEnumMember]
		Err_CompareNotPossible1 = 68,
		[ReleasedEnumMember]
		Err_CompareNotPossible2 = 69,
		[ReleasedEnumMember]
		Err_IniNeedsUserdefType = 70,
		[ReleasedEnumMember]
		Err_CannotAddMultipleTime = 71,
		[ReleasedEnumMember]
		Err_OperationNotPossibleOnType = 72,
		[ReleasedEnumMember]
		Err_CannotMultiplyMultipleTime = 73,
		[ReleasedEnumMember]
		Err_UnexpectedArrayInitialisation = 74,
		[ReleasedEnumMember]
		Err_TooManyInitializer = 75,
		[ReleasedEnumMember]
		Err_UnexpectedStructureInitialisation = 76,
		[ReleasedEnumMember]
		Err_UnknownType = 77,
		[ReleasedEnumMember]
		Err_UnsupportedType = 78,
		[ReleasedEnumMember]
		Err_FunctionBlockNeedsInstance = 80,
		[ReleasedEnumMember]
		Err_UnexpectedPragmaif = 81,
		[ReleasedEnumMember]
		Err_NoValidConditionforPragma = 82,
		[ReleasedEnumMember]
		Err_UnexpectedOperandForIndexOf = 83,
		[ReleasedEnumMember]
		Err_NoValidOperandforPragma = 84,
		[ReleasedEnumMember]
		Err_DefineValueExpected = 85,
		[ReleasedEnumMember]
		Err_InterfaceNotFound = 86,
		[ReleasedEnumMember]
		Err_NoMethodImplementation = 87,
		[ReleasedEnumMember]
		Err_SomethingOverridingMethod = 88,
		[ReleasedEnumMember]
		Err_InterfaceChanged = 89,
		[ReleasedEnumMember]
		Err_BaseClassNotFound = 90,
		[ReleasedEnumMember]
		Err_SelfInheritanceBase = 91,
		[ReleasedEnumMember]
		Err_OnlyMethodsOverride = 92,
		[ReleasedEnumMember]
		Err_SomethingOverridingMethodBase = 93,
		[ReleasedEnumMember]
		Err_InterfaceChangedBase = 94,
		[ReleasedEnumMember]
		Err_SelfInheritance = 95,
		[ReleasedEnumMember]
		Err_NoMultipleInheritance = 96,
		[ReleasedEnumMember]
		Err_VariableOverride = 97,
		[ReleasedEnumMember]
		Err_FunctionBlockNoLongerValid = 98,
		[ReleasedEnumMember]
		Err_NoLocalEnum = 99,
		[ReleasedEnumMember]
		Wrn_LibraryNotInstalled = 100,
		[ReleasedEnumMember]
		Err_DataRecursion = 101,
		[ReleasedEnumMember]
		Err_OutOfRetainMemory = 102,
		[ReleasedEnumMember]
		Err_OutOfRetainMemoryWithWholeSize = 103,
		[ReleasedEnumMember]
		Err_OutOfMemory = 104,
		[ReleasedEnumMember]
		Err_OutOfMemoryFunctionPointer = 105,
		[ReleasedEnumMember]
		Err_OutOfMemoryWithWholeSize = 106,
		[ReleasedEnumMember]
		Err_VarTooBig = 107,
		[ReleasedEnumMember]
		Err_NoInputMemory = 108,
		[ReleasedEnumMember]
		Err_NoOutputMemory = 109,
		[ReleasedEnumMember]
		Err_NoMemoryMemory = 110,
		[ReleasedEnumMember]
		Err_AddressOutOfRange = 111,
		[ReleasedEnumMember]
		Err_AddressMisaligned = 112,
		[ReleasedEnumMember]
		Err_NoBitTypeOnBitAddress = 113,
		[ReleasedEnumMember]
		Err_InvalidJumpDestination = 114,
		[ReleasedEnumMember]
		Err_CalcNeedsCall = 115,
		[ReleasedEnumMember]
		Err_DuplicateLabelDefinition = 116,
		[ReleasedEnumMember]
		Err_NoSuchLabel = 117,
		[ReleasedEnumMember]
		Wrn_LabelNoReference = 118,
		[ReleasedEnumMember]
		Err_WrongConstructor = 119,
		[ReleasedEnumMember]
		Err_WrongDestructor = 120,
		[ReleasedEnumMember]
		Err_BaseNotAllowed = 122,
		[ReleasedEnumMember]
		Err_NoValidEnumInit = 124,
		[ReleasedEnumMember]
		Wrn_EnumValueDuplicate = 125,
		[ReleasedEnumMember]
		Err_IndexNumWrong = 126,
		[ReleasedEnumMember]
		Err_OutOfCodeMemory = 127,
		[ReleasedEnumMember]
		Err_NoInstancePath = 128,
		[ReleasedEnumMember]
		Err_NoValidInstancePath = 129,
		[ReleasedEnumMember]
		Err_MissingParameterList = 130,
		[ReleasedEnumMember]
		Err_WrongTypeForAdr = 131,
		[ReleasedEnumMember]
		Err_NoEnclosingLoopExit = 132,
		[ReleasedEnumMember]
		Err_NotSupportedInInterface = 135,
		[ReleasedEnumMember]
		Err_Ambiguity = 136,
		[ReleasedEnumMember]
		Err_NoMatchingInitMethodFound = 138,
		[ReleasedEnumMember]
		Wrn_StatementNoEffect = 139,
		[ReleasedEnumMember]
		Err_LValueNoRefType = 140,
		[ReleasedEnumMember]
		Err_LValueForReference = 141,
		[ReleasedEnumMember]
		Err_VariableDuplicate = 142,
		[ReleasedEnumMember]
		Err_NoReadAccessToProperty = 143,
		[ReleasedEnumMember]
		Err_InheritanceError = 144,
		[ReleasedEnumMember]
		Err_InterfaceImplementationError = 145,
		[ReleasedEnumMember]
		Err_ParamsNotAllowed = 146,
		[ReleasedEnumMember]
		Err_NoVarsinInterface = 149,
		[ReleasedEnumMember]
		Err_OneAccessorRequired = 150,
		[ReleasedEnumMember]
		Err_ArrayBorderIsNoConstant = 161,
		[ReleasedEnumMember]
		Err_ArrayInitialisationCountNoConstant = 162,
		[ReleasedEnumMember]
		Err_LowerGreaterUpperBorder = 163,
		[ReleasedEnumMember]
		Err_POUCalledFromDifferentTasks = 164,
		[ReleasedEnumMember]
		Err_OutputByteWrittenFromDifferentTasks = 165,
		[ReleasedEnumMember]
		Err_VarAccessNotSupported = 167,
		[ReleasedEnumMember]
		Err_VarConfigNotAllowed = 168,
		[ReleasedEnumMember]
		Err_VarGlobalNotAllowed = 169,
		[ReleasedEnumMember]
		Err_StructureNotAllowed = 170,
		[ReleasedEnumMember]
		Err_UnionNotAllowed = 171,
		[ReleasedEnumMember]
		Err_StaticNotAllowed = 172,
		[ReleasedEnumMember]
		Err_DeclarationKeywordNotAllowed = 173,
		[ReleasedEnumMember]
		Err_VarTempNotAllowed = 174,
		[ReleasedEnumMember]
		Err_RetainOrPersistentNotAllowed = 175,
		[ReleasedEnumMember]
		Err_WrongVarInInterfaceMethod = 176,
		[ReleasedEnumMember]
		Err_NoInstanceObject = 177,
		[ReleasedEnumMember]
		Err_InOutAccessOutside = 178,
		[ReleasedEnumMember]
		Err_StructInitOnlyInput = 179,
		[ReleasedEnumMember]
		Err_LibraryConflict = 180,
		[ReleasedEnumMember]
		Inf_RelatedPosition = 181,
		[ReleasedEnumMember]
		Err_ReturnTypeForNonFunction = 182,
		[ReleasedEnumMember]
		Err_InvalidBaseForGlobalScopeExpression = 183,
		[ReleasedEnumMember]
		Err_NoOnlineChangePossible = 184,
		[ReleasedEnumMember]
		Err_NoCallInInstancePath = 185,
		[ReleasedEnumMember]
		Err_NoCallInInterfaceComparison = 186,
		[ReleasedEnumMember]
		Wrn_ExternalReferenceIgnored = 187,
		[ReleasedEnumMember]
		Err_DeviceNotInstalled = 188,
		[ReleasedEnumMember]
		Err_SemicolonExpected = 189,
		[ReleasedEnumMember]
		Err_SemicolonExpectedInsteadOfEnd = 190,
		[ReleasedEnumMember]
		Err_IndexOfNotSupported = 191,
		[ReleasedEnumMember]
		Err_AccessOutsideOfCall = 192,
		[ReleasedEnumMember]
		Err_InvalidSignatureName = 193,
		[ReleasedEnumMember]
		Err_CaseLabelOutsideOfCase = 194,
		[ReleasedEnumMember]
		Wrn_ImplicitSignedToUnsigned = 195,
		[ReleasedEnumMember]
		Wrn_ImplicitUnsignedToSigned = 196,
		[ReleasedEnumMember]
		Wrn_ImplicitIntToReal = 197,
		[ReleasedEnumMember]
		Wrn_StringConstantTooLong = 198,
		[ReleasedEnumMember]
		Err_InterfaceNeedsInstance = 199,
		[ReleasedEnumMember]
		Wrn_PlaceholderNotResolved = 200,
		[ReleasedEnumMember]
		Err_InOutParamNotEqual = 201,
		[ReleasedEnumMember]
		Err_ApplicationConflict = 202,
		[ReleasedEnumMember]
		Err_BitsForStructureOnly = 203,
		[ReleasedEnumMember]
		Err_BitsWrongScope = 204,
		[ReleasedEnumMember]
		Err_NoPointerToBit = 205,
		[ReleasedEnumMember]
		Err_NoArrayOfBit = 206,
		[ReleasedEnumMember]
		Err_NoSystemDefine = 207,
		[ReleasedEnumMember]
		Err_ModNotReal = 208,
		[ReleasedEnumMember]
		Wrn_TooManyApplications = 209,
		[ReleasedEnumMember]
		Wrn_InsertSpecialPersistent = 210,
		[ReleasedEnumMember]
		Err_VariableDeclarationExpected = 211,
		[ReleasedEnumMember]
		Err_VariableListExpected = 212,
		[ReleasedEnumMember]
		Err_GlobalVariableListExpected = 213,
		[ReleasedEnumMember]
		Err_PersistentWrongVariable = 214,
		[ReleasedEnumMember]
		Err_PersistentNoAddress = 215,
		[ReleasedEnumMember]
		Err_CaseLabelDuplicate = 216,
		[ReleasedEnumMember]
		Err_CaseLabelInCaseRange = 217,
		[ReleasedEnumMember]
		Err_CaseLabelNoConstant = 218,
		[ReleasedEnumMember]
		Err_CaseRangesOverlapping = 219,
		[ReleasedEnumMember]
		Wrn_DeviceNotInstalledForSimulation = 220,
		[ReleasedEnumMember]
		Err_MalformedAddress = 221,
		[ReleasedEnumMember]
		Err_NoReferenceToOutput = 222,
		[ReleasedEnumMember]
		Wrn_CompoRefAssignCompatibilityWarning = 223,
		[ReleasedEnumMember]
		Err_CallRecursion = 224,
		[ReleasedEnumMember]
		Err_NotAnInstanceOf = 225,
		[ReleasedEnumMember]
		Err_NotEnoughMemoryForVariableInit = 226,
		[ReleasedEnumMember]
		Err_NoConstantInitialisationForValue = 227,
		[ReleasedEnumMember]
		Wrn_NoInitialValueForConstant = 228,
		[ReleasedEnumMember]
		Err_BlobInitError = 229,
		[ReleasedEnumMember]
		Err_UnexpectedTypeName = 230,
		[ReleasedEnumMember]
		Err_SpecialTypeExpected = 231,
		[ReleasedEnumMember]
		Err_ArrayInitializationExpected = 232,
		[ReleasedEnumMember]
		Err_StructureInitializationExpected = 233,
		[ReleasedEnumMember]
		Err_QueryInterfaceErrorP1 = 234,
		[ReleasedEnumMember]
		Err_QueryInterfaceErrorP2 = 235,
		[ReleasedEnumMember]
		Err_WrongTypeForExternal = 236,
		[ReleasedEnumMember]
		Err_NoVarForExternal = 237,
		[ReleasedEnumMember]
		Err_NoInitialForExternal = 238,
		[ReleasedEnumMember]
		Err_QueryInterfaceP1NoIQuery = 239,
		[ReleasedEnumMember]
		Err_QueryPointerErrorP1 = 240,
		[ReleasedEnumMember]
		Err_QueryPointerErrorP2 = 241,
		[ReleasedEnumMember]
		Err_DeleteNeedsPointer = 242,
		[ReleasedEnumMember]
		Err_NamesNotEqual = 243,
		[ReleasedEnumMember]
		Err_MissingInstancePathForPersistent = 244,
		[ReleasedEnumMember]
		Wrn_MissingObjectForPersistent = 245,
		[ReleasedEnumMember]
		Err_NoBytesInRetain = 246,
		[ReleasedEnumMember]
		Err_NoPropertiesInOutAssignment = 247,
		[ReleasedEnumMember]
		Err_NewNeedsType = 248,
		[ReleasedEnumMember]
		Err_NewArrayOnUserdefNotAllowed = 249,
		[ReleasedEnumMember]
		Err_NewPositionNotOK = 250,
		[ReleasedEnumMember]
		Err_ReferenceNotAllowed = 261,
		[ReleasedEnumMember]
		Err_ReferenceNotAllowedForVarInOut = 262,
		[ReleasedEnumMember]
		Err_NewOnlyWithAttribute = 263,
		[ReleasedEnumMember]
		Err_NoOnlineChangeOnDynamicObjects = 264,
		[ReleasedEnumMember]
		Err_DynamicMemoryNotSupported = 265,
		[ReleasedEnumMember]
		Wrn_LoopExitConditionConstant = 266,
		[ReleasedEnumMember]
		Err_UninitialisedVariableUsedInInitialisation = 268,
		[ReleasedEnumMember]
		Wrn_ValueAssignViaPointerMayChangeVFTable = 269,
		[ReleasedEnumMember]
		Err_RetainsNotSupported = 270,
		[ReleasedEnumMember]
		Err_NoReferenceToBits = 272,
		[ReleasedEnumMember]
		Err_ImplicitMethodNameForVariable = 273,
		[ReleasedEnumMember]
		Err_DeviceNameNoIdent = 274,
		[ReleasedEnumMember]
		Err_NoVarTempInSubsequentPrograms = 275,
		[ReleasedEnumMember]
		Err_RetainNotAccessibleVariable = 276,
		[ReleasedEnumMember]
		Err_RetainNotAccessiblePOU = 277,
		[ReleasedEnumMember]
		Err_InvalidInitialisationForArray = 278,
		[ReleasedEnumMember]
		Err_SummarizedLibraryErrors = 279,
		[ReleasedEnumMember]
		Err_FinalOnFunctionBlocksAndMethodsOnly = 280,
		[ReleasedEnumMember]
		Err_PrivateOnMethodsOnly = 281,
		[ReleasedEnumMember]
		Err_NoInheritanceOnFinalType = 282,
		[ReleasedEnumMember]
		Err_NoOverrideOnFinalMethod = 283,
		[ReleasedEnumMember]
		Err_NoOverrideOnPrivateMethod = 284,
		[ReleasedEnumMember]
		Err_NoOverrideOnInternalMethod = 285,
		[ReleasedEnumMember]
		Err_NoChangeOnAccessModifier = 286,
		[ReleasedEnumMember]
		Err_CallOfProtectedMethod = 287,
		[ReleasedEnumMember]
		Err_CallOfPrivateMethod = 288,
		[ReleasedEnumMember]
		Err_CallOfProtectedProperty = 289,
		[ReleasedEnumMember]
		Err_CallOfPrivateProperty = 290,
		[ReleasedEnumMember]
		Err_AccessToInternalVariable = 291,
		[ReleasedEnumMember]
		Err_AccessToInternalObject = 292,
		[ReleasedEnumMember]
		Err_AccessToInternalProperty = 293,
		[ReleasedEnumMember]
		Err_InOutAssignedInActionCall = 294,
		[ReleasedEnumMember]
		Err_SlotFunctionHiddenByVariable = 295,
		[ReleasedEnumMember]
		Err_SlotFunctionAmbiguousName = 296,
		[ReleasedEnumMember]
		Err_StackOverflowDetected = 297,
		[ReleasedEnumMember]
		Wrn_StackCheckIncompleteDueToRecursion = 298,
		[ReleasedEnumMember]
		Err_NoCodegenerator = 299,
		[ReleasedEnumMember]
		Err_RelatedPositionTaskX = 300,
		[ReleasedEnumMember]
		Err_PersistentVariablesChangeMessageText = 301,
		[ReleasedEnumMember]
		Err_CancelledByUser = 302,
		[ReleasedEnumMember]
		Err_StructureInitialisationNotPossible = 303,
		[ReleasedEnumMember]
		Err_ArrayInitialisationNotPossible = 304,
		[ReleasedEnumMember]
		Err_StructuredValueTypeInExternalCall = 306,
		[ReleasedEnumMember]
		Err_RecursiveConstantInitialisation = 307,
		[ReleasedEnumMember]
		Wrn_ShiftExceedsTypeSize = 308,
		[ReleasedEnumMember]
		Err_OperatorNotSupported = 309,
		[ReleasedEnumMember]
		Err_LValueForAnyVar = 310,
		[ReleasedEnumMember]
		Err_AnyTypeOnlyInFunction = 311,
		[ReleasedEnumMember]
		Wrn_ConcurrentAccessOfBitInSameByte = 312,
		[ReleasedEnumMember]
		Err_NoInitialForInoutConstant = 313,
		[ReleasedEnumMember]
		Err_VariableForVarinoutConstant = 314,
		[ReleasedEnumMember]
		Wrn_StringTooShortForVarInOut = 315,
		[ReleasedEnumMember]
		Wrn_MethodAlreadyCalledImplicitly = 316,
		[ReleasedEnumMember]
		Err_LiteralExpected = 317,
		[ReleasedEnumMember]
		Err_ExprNoConstantError = 318,
		[ReleasedEnumMember]
		Err_NotAllowedInInterfaceLib = 319,
		[ReleasedEnumMember]
		Err_NotAllowedInContainerLib = 320,
		[ReleasedEnumMember]
		Err_InterfaceMethodImplementationNotPublic = 321,
		[ReleasedEnumMember]
		Err_NoVarConfigInMethodOrFunction = 322,
		[ReleasedEnumMember]
		Err_CantHaveBaseClass = 323,
		[ReleasedEnumMember]
		Err_NoIndirectPropertyCallOnStringWithSize = 324,
		[ReleasedEnumMember]
		Wrn_GlobalInitSlotNotForVariables = 325,
		[ReleasedEnumMember]
		Err_LibraryWithUnicodeIdentifiers = 326,
		[ReleasedEnumMember]
		Wrn_ImplicitEnumConversion = 327,
		[ReleasedEnumMember]
		Err_NoAssign = 328,
		[ReleasedEnumMember]
		Err_DuplicateVarConfig = 329,
		[ReleasedEnumMember]
		Err_TryCatchNotSupportedVersion = 330,
		[ReleasedEnumMember]
		Err_TryCatchNotSupportedCodegenerator = 331,
		[ReleasedEnumMember]
		Err_MappedVarWrittenInDiffTasks = 332,
		[ReleasedEnumMember]
		Err_OpTakesAtMostInputs = 333,
		[ReleasedEnumMember]
		Err_LibNamespaceConflict = 334,
		[ReleasedEnumMember]
		Wrn_ComplexExpression = 335,
		[ReleasedEnumMember]
		Err_BitAccessOnlyOnInt = 336,
		[ReleasedEnumMember]
		Err_VarInstOnlyInMethods = 337,
		[ReleasedEnumMember]
		Err_LibSupports32BitOnly = 338,
		[ReleasedEnumMember]
		Wrn_BoolNotAtBitAddress = 339,
		[ReleasedEnumMember]
		Err_CheckLicenseNeedsSysTarget = 340,
		[ReleasedEnumMember]
		Err_OperatorNotAllowedAtPosition = 341,
		[ReleasedEnumMember]
		Err_InstanceNotAllowedInRetain = 342,
		[ReleasedEnumMember]
		Err_ReferenceToInput = 343,
		[ReleasedEnumMember]
		Wrn_StruturedTypePropertyNotMonitorable = 344,
		[ReleasedEnumMember]
		Err_TryCatchNotSupported = 345,
		[ReleasedEnumMember]
		Err_NoDirectAddressInSubsequentVarDecl = 346,
		[ReleasedEnumMember]
		Err_UnexpectedOperandForCallInitFunction = 347,
		[ReleasedEnumMember]
		Err_BitAdrOnOperation = 348,
		[ReleasedEnumMember]
		Wrn_InterfaceInVarInOut = 349,
		[ReleasedEnumMember]
		Wrn_ReferenceToInterface = 350,
		[ReleasedEnumMember]
		Wrn_AttributeCheck = 351,
		[ReleasedEnumMember]
		Err_MaxArraySizeExceeded = 352,
		[ReleasedEnumMember]
		Err_RefAssignNeedsLValue = 353,
		[ReleasedEnumMember]
		Wrn_EnumComparison = 354,
		[ReleasedEnumMember]
		Wrn_NoPointerToBit = 355,
		[ReleasedEnumMember]
		Err_AttributeNotValidForNonExternalPOU = 356,
		[ReleasedEnumMember]
		Wrn_Obsolete = 357,
		[ReleasedEnumMember]
		Err_StrictEnumNotAMember = 358,
		[ReleasedEnumMember]
		Err_StrictEnumNoArithmeticAllowed = 359,
		[ReleasedEnumMember]
		Err_ParameterlistNotConst = 360,
		[ReleasedEnumMember]
		Err_NoMixExternalIECInheritance = 361,
		[ReleasedEnumMember]
		Err_InvalidStringSize = 362,
		[ReleasedEnumMember]
		Err_UserCheckFunctionsNotSupported = 363,
		[ReleasedEnumMember]
		Err_CallAfterInitHasInputs = 364,
		[ReleasedEnumMember]
		Err_GeneratingVarInitializations = 365,
		[ReleasedEnumMember]
		Err_ImplicitEnumerationTypeNotExpected = 366,
		[ReleasedEnumMember]
		Err_InternalErrorProhibitingOnlineChange = 367,
		[ReleasedEnumMember]
		Err_InvalidEnumDefaultValue = 368,
		[ReleasedEnumMember]
		Err_FeatureNotImplemented = 369,
		[ReleasedEnumMember]
		Wrn_InstanceCalledMoreThenOnce = 370,
		[ReleasedEnumMember]
		Wrn_NonLocalAccessToVarInOut = 371,
		[ReleasedEnumMember]
		Err_DuplicateElseInCaseStatement = 372,
		[ReleasedEnumMember]
		Wrn_Pragma = 373,
		[ReleasedEnumMember]
		Err_DivisionByZero = 374,
		[ReleasedEnumMember]
		Err_InvalidEnumBaseType = 375,
		[ReleasedEnumMember]
		Err_TooFewParametersForExtensibleFunction = 376,
		[ReleasedEnumMember]
		Err_TooManyParametersForExtensibleFunction = 377,
		[ReleasedEnumMember]
		Err_NoFormalParamsCallsForExtensibleFunction = 378,
		[ReleasedEnumMember]
		Err_IntegerLiteralExpected = 379,
		[ReleasedEnumMember]
		Err_LowerUpperBoundOnVariableLengthArrayOnly = 380,
		[ReleasedEnumMember]
		Err_OperatorNoValidVariableName = 381,
		[ReleasedEnumMember]
		Err_InconsistentInheritanceOfCPPCompatibility = 382,
		[ReleasedEnumMember]
		Err_IncompletePOUDeclaration = 383,
		[ReleasedEnumMember]
		Err_SelMuxOnlyEqualUserDefTypes = 384,
		[ReleasedEnumMember]
		Err_VarLengthArrayInOut = 385,
		[ReleasedEnumMember]
		Err_VarLengthArrayTopLevel = 386,
		[ReleasedEnumMember]
		Err_OutParamNotEqual = 387,
		[ReleasedEnumMember]
		Wrn_LibWithStringInVarInOut = 388,
		[ReleasedEnumMember]
		Wrn_LValueForVarinoutStrings = 389,
		[ReleasedEnumMember]
		Err_AddressSourceIsAddressDest = 390,
		[ReleasedEnumMember]
		Err_NoCopyCodeAllowed = 391,
		[ReleasedEnumMember]
		Err_ATDeclarationNotAllowed = 392,
		[ReleasedEnumMember]
		Err_ArrayBorderNoValidSignedInteger = 393,
		[ReleasedEnumMember]
		Wrn_FBExitCalledForStackInstance = 394,
		[ReleasedEnumMember]
		Err_NumOfInitializersDoNotMatch = 395,
		[ReleasedEnumMember]
		Err_IsValidRefNeedsReference = 396,
		[ReleasedEnumMember]
		Err_ImplicitMethodImplementationNotPublic = 397,
		[ReleasedEnumMember]
		Err_SystemOutOfMemory = 398,
		[ReleasedEnumMember]
		Err_ImplicitReferenceTypeDeclNotAllowed = 399,
		[ReleasedEnumMember]
		Err_ImplicitRefTypeIsnotAllowedAsBase = 400,
		[ReleasedEnumMember]
		Err_ImplicitRefTypeAllClassesNeedAttrib = 401,
		[ReleasedEnumMember]
		Err_ImplicitRefTypeDeclarationNotAllowed = 402,
		[ReleasedEnumMember]
		Err_ImplicitRefTypeSignNotSupported = 403,
		[ReleasedEnumMember]
		Wrn_CompilerVersionDeprecated = 404,
		[ReleasedEnumMember]
		Err_MultipleAssignmentsToInterfaceVariables = 405,
		[ReleasedEnumMember]
		Wrn_ImplicitCheckFunctionShadowed = 406,
		[ReleasedEnumMember]
		Err_AddressOfNonInstanceVar = 407,
		[ReleasedEnumMember]
		Err_ImplicitReferenceTypeOnlChangeError = 408,
		[ReleasedEnumMember]
		Err_NoResolutionForLazyVariable = 409,
		[ReleasedEnumMember]
		Wrn_CompatibilityProblemForRefProperty = 410,
		[ReleasedEnumMember]
		Err_NoVarInputInPropertyAccessors = 411,
		[ReleasedEnumMember]
		Err_MultipleAssignsToSameInputInCall = 412,
		[ReleasedEnumMember]
		Err_RefAssignOnlyForReferenceTypes = 413,
		[ReleasedEnumMember]
		Err_OutOfPersistentMemoryImplicit = 414,
		[ReleasedEnumMember]
		Err_OutOfPersistentMemoryExplicit = 415,
		[ReleasedEnumMember]
		Err_LibraryNamespaceNotValid = 416,
		[ReleasedEnumMember]
		Err_LValueForVarinoutStrings = 417,
		[ReleasedEnumMember]
		Err_StringTooShortForVarInOut = 418,
		[ReleasedEnumMember]
		Err_NoInputsWithSlotAttributes = 419,
		[ReleasedEnumMember]
		Err_NotEnoughMemoryForCompactDownload = 420,
		[ReleasedEnumMember]
		Wrn_ExtendsForInterfaces = 421,
		[ReleasedEnumMember]
		Wrn_GranularityMismatchForDirectVariable = 422,
		[ReleasedEnumMember]
		Err_MultipleAssignmentWithReferences = 423,
		[ReleasedEnumMember]
		Err_MultipleAssignmentWithChangingValueTypes = 424,
		[ReleasedEnumMember]
		Err_NoMemoryReserveForExternal = 425,
		[ReleasedEnumMember]
		Wrn_AtLeastOneExpected = 426,
		[ReleasedEnumMember]
		Err_UnknownMaxStackSize = 427,
		[ReleasedEnumMember]
		Err_AbstractOnFunctionBlocksAndMethodsOnly = 428,
		[ReleasedEnumMember]
		Err_AbstractAndFinalNotPossible = 429,
		[ReleasedEnumMember]
		Err_AbstractAndPrivateNotPossible = 430,
		[ReleasedEnumMember]
		Err_AbstractMethodNotImplemented = 431,
		[ReleasedEnumMember]
		Err_AbstractMethodOnlyInAbstractFunctionblock = 432,
		[ReleasedEnumMember]
		Err_AbstractMethodMustNotContainAnyStatements = 433,
		[ReleasedEnumMember]
		Err_AbstractFunctionBlockInstance = 434,
		[ReleasedEnumMember]
		Err_AbstractPropertyNotImplemented = 435,
		[ReleasedEnumMember]
		Err_StringLengthIsNoConstant = 436,
		[ReleasedEnumMember]
		Err_NotAllowedInTaskLocalVariables = 437,
		[ReleasedEnumMember]
		Err_TaskLocalVariablesWriterTaskNotDefined = 438,
		[ReleasedEnumMember]
		Err_TaskLocalVariablesAccessNotAllowed = 439,
		[ReleasedEnumMember]
		Err_TaskLocalVariablesNoOnlineChangePossible = 440,
		[ReleasedEnumMember]
		Wrn_VarInOutUnitializedInInitialValue = 441,
		[ReleasedEnumMember]
		Err_AbstractMethodStaticCall = 442,
		[ReleasedEnumMember]
		Err_AbstractPropertyStaticCall = 443,
		[ReleasedEnumMember]
		Err_InconsistentUseOfCPPCompatibility = 444,
		[ReleasedEnumMember]
		Err_AbstractWrongVarInMethod = 445,
		[ReleasedEnumMember]
		Err_NoValuePassingForCPPExternal = 446,
		[ReleasedEnumMember]
		Wrn_OnlyConstantInitialValueForMappedPersistentVar = 447,
		[ReleasedEnumMember]
		Err_OperatorNotSupportedVersion = 448,
		[ReleasedEnumMember]
		Err_ComparisonOperatorExpected = 449,
		[ReleasedEnumMember]
		Err_StringLiteralExpected = 450,
		[ReleasedEnumMember]
		Err_VersionOverflow = 451,
		[ReleasedEnumMember]
		Err_VersionPartNegative = 452,
		[ReleasedEnumMember]
		Err_VersionInvalidFormat = 453,
		[ReleasedEnumMember]
		Err_VectorSizeNotValid = 500,
		[ReleasedEnumMember]
		Err_VectorSizeIsNoConstant = 501,
		[ReleasedEnumMember]
		Err_VectorNoPersistentRetain = 502,
		[ReleasedEnumMember]
		Err_VectorBaseMustBeRealType = 503,
		[ReleasedEnumMember]
		Err_VectorTypesNotCompatible = 504,
		[ReleasedEnumMember]
		Err_VectorTypeCantBePlacedInUnion = 505,
		[ReleasedEnumMember]
		Err_LowerUpperBoundOperandNotInRange = 506,
		[ReleasedEnumMember]
		Err_LowerUpperBoundOperandNotExactly = 507,
		[ReleasedEnumMember]
		Wrn_Ambiguity = 508,
		[ReleasedEnumMember]
		Err_MultipleAssignmentsNotAllowedForOperator = 509,
		[ReleasedEnumMember]
		Err_ConfiguredCompilerVersionNotAvailable = 510,
		[ReleasedEnumMember]
		Err_AbstractFunctionBlockAssigned = 511,
		[ReleasedEnumMember]
		Wrn_CallOfPrivateProperty = 513,
		[ReleasedEnumMember]
		Wrn_AccessToInternalProperty = 514,
		[ReleasedEnumMember]
		Wrn_CallOfProtectedProperty = 515,
		[ReleasedEnumMember]
		Wrn_AccessToInternalVariable = 516,
		[ReleasedEnumMember]
		Wrn_AccessToInternalObject = 517,
		[ReleasedEnumMember]
		Err_NoNamespace = 518,
		[ReleasedEnumMember]
		Err_InvalidAccessPathForNamespaceAccess = 519,
		[ReleasedEnumMember]
		Err_InvalidNamespaceForNamespaceAccess = 520,
		[ReleasedEnumMember]
		Err_UnknownCompilerVersionInCompiledLib = 521,
		[ReleasedEnumMember]
		Wrn_PersistentVariableOnStack = 522,
		[ReleasedEnumMember]
		Err_InconsistentUseOfCPPCompatibilityMissingParent = 523,
		[ReleasedEnumMember]
		Err_WrongReInit = 524,
		[ReleasedEnumMember]
		Wrn_InvalidDefaultValue = 525,
		[ReleasedEnumMember]
		Wrn_DefaultValueNotConstant = 526,
		[ReleasedEnumMember]
		Wrn_DefaultValueTopLevel = 527,
		[ReleasedEnumMember]
		Err_NoBranchOutOfFinally = 528,
		[ReleasedEnumMember]
		Err_NoExitOrContinueOutOfTry = 529,
		[ReleasedEnumMember]
		Err_NoExitOrContinueOutOfCatch = 530,
		[ReleasedEnumMember]
		Err_MultipleAssignmentWithProperty = 531,
		[ReleasedEnumMember]
		Err_FunNeedsAtLeastNInputs = 532,
		[ReleasedEnumMember]
		Wrn_ObsoleteOutputInAbstractMethod = 533,
		[ReleasedEnumMember]
		Err_WrongCallAfterGlobalInitSlotSignature = 534,
		[ReleasedEnumMember]
		Inf_UseXSizeOfOperator = 535,
		[ReleasedEnumMember]
		Err_InvalidInitialisationForVarInst = 536,
		[ReleasedEnumMember]
		Err_NoPropertyForVarInout = 537,
		[ReleasedEnumMember]
		Err_InterfaceChanged_NumberOfInputsOutputsDifferent = 538,
		[ReleasedEnumMember]
		Err_InterfaceChanged_VariableDifferent = 539,
		[ReleasedEnumMember]
		Wrn_MissingAttributeNoAssign = 540,
		[ReleasedEnumMember]
		Err_NoMemoryAllocationCallback = 541,
		[ReleasedEnumMember]
		Wrn_NoInheritanceForUnions = 542,
		[ReleasedEnumMember]
		Wrn_ReservedUnusedKeyword = 543,
		[ReleasedEnumMember]
		Err_GenericOnWrongPosition = 544,
		[ReleasedEnumMember]
		Err_GenericOnlyConst = 545,
		[ReleasedEnumMember]
		Err_TypeIsNotGeneric = 546,
		[ReleasedEnumMember]
		Err_GenericWrongNumberOfInitializer = 547,
		[ReleasedEnumMember]
		Err_GenericNotConstant = 548,
		[ReleasedEnumMember]
		Err_NoStaticVariableInitialisationForValue = 549,
		[ReleasedEnumMember]
		Err_AttributeNotAllowedFor = 550,
		[ReleasedEnumMember]
		Err_GenericNoInteger = 551,
		[ReleasedEnumMember]
		Err_NoOutsideAccessToGenericVariable = 552,
		[ReleasedEnumMember]
		Err_GenericDeclarationProducesErrorInGeneratedCode = 553,
		[ReleasedEnumMember]
		Err_NoExplicitCall = 554,
		[ReleasedEnumMember]
		Wrn_NonAsciiStringLiteral = 555,
		[ReleasedEnumMember]
		Err_NoGenericInstanceInVarConst = 556,
		[ReleasedEnumMember]
		Err_FCallWrongNumberOfArguments = 557,
		[ReleasedEnumMember]
		Err_FCallWrongCallPatternSignature = 558,
		[ReleasedEnumMember]
		Err_FCallExpectedPointerAsSecondArgument = 559,
		[ReleasedEnumMember]
		Err_NewOnInterfaceNotPossible = 560,
		[ReleasedEnumMember]
		Wrn_CallRecursion = 561,
		[ReleasedEnumMember]
		Err_PartialAccess_OnlyOnBitTypes = 562,
		[ReleasedEnumMember]
		Err_PartialAccess_NotAValidComponent = 563,
		[ReleasedEnumMember]
		Wrn_ReferenceToUninitializedVariable = 564,
		[ReleasedEnumMember]
		Wrn_WrongDestructor = 565,
		[ReleasedEnumMember]
		Wrn_WrongReInit = 566,
		[ReleasedEnumMember]
		Wrn_NotAllowedInInterfaceLib = 567,
		[ReleasedEnumMember]
		Wrn_InterfaceChangedBase = 568,
		[ReleasedEnumMember]
		Wrn_MissingInstancePathForPersistent = 569,
		[ReleasedEnumMember]
		Err_ProjectDefinedNotSupportedFor = 570,
		[ReleasedEnumMember]
		Wrn_ExitForRetainInstances = 571,
		[ReleasedEnumMember]
		Wrn_UninitialisedVariableUsedInInitialisation = 572,
		[ReleasedEnumMember]
		Wrn_AbstractKeywordMissing = 573,
		[ReleasedEnumMember]
		Err_NoOnlineChangeOnGenericConstantType = 574,
		[ReleasedEnumMember]
		Err_NoOnlineChangeOnGenericConstantBaseType = 575,
		[ReleasedEnumMember]
		Err_AccessVarInstFromOutsideTheDeclaringMethod = 576,
		[ReleasedEnumMember]
		Err_NoCopyCodeForVariableAtDirectAddress = 577,
		[ReleasedEnumMember]
		Err_UnexpectedStatement = 578,
		[ReleasedEnumMember]
		Err_UnsupportedFeature = 579,
		[ReleasedEnumMember]
		Wrn_ConstantInStructDeclaration = 580,
		[ReleasedEnumMember]
		Err_NoMatchingOverload = 581,
		[ReleasedEnumMember]
		Err_OverloadNeedsAttribute = 582,
		[ReleasedEnumMember]
		Err_OverloadWithSameInputs = 583,
		[ReleasedEnumMember]
		Err_MaxNestingDepthExceeded = 584,
		[ReleasedEnumMember]
		Err_GenericParamsAllExplicitOrNone = 585,
		[ReleasedEnumMember]
		Err_GenericParamMissing = 586,
		[ReleasedEnumMember]
		Err_GenericParamUnknown = 587,
		[ReleasedEnumMember]
		Err_MissingImplementationTerminator = 588,
		[ReleasedEnumMember]
		Err_UnknownEmbeddedLanguageType = 589,
		[ReleasedEnumMember]
		Err_NoResolutionForSomeLazyVariables = 590,
		[ReleasedEnumMember]
		Wrn_ChangeOfAccessModifier = 591,
		[ReleasedEnumMember]
		Err_NoNewAssignmentInOtherExpression = 454,
		[ReleasedEnumMember]
		Err_ExplicitTransitionAssignMissing = 455,
		[ReleasedEnumMember]
		Wrn_AmbiguousCheckfunctionInLibrary = 456
	}
}
