using System;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0081;

namespace \u000E
{
	// Token: 0x02000399 RID: 921
	internal static class \u0018
	{
		// Token: 0x06003563 RID: 13667 RVA: 0x000D3744 File Offset: 0x000D1944
		static \u0018()
		{
			\u0018.\u0001.Add(MessageId.None, "");
			\u0018.\u0006();
			\u0018.\u0005();
			\u0018.\u0004();
			\u0018.\u0003();
			\u0018.\u0001();
			\u0018.\u0002();
			\u0018.\u0007();
		}

		// Token: 0x06003564 RID: 13668 RVA: 0x000D3784 File Offset: 0x000D1984
		private static void \u0001()
		{
			\u0018.\u0001.Add(MessageId.Err_ImplicitRefTypeAllClassesNeedAttrib, \u0081.\u0002.Err_ImplicitRefTypeAllClassesNeedAttrib);
			\u0018.\u0001.Add(MessageId.Err_ImplicitRefTypeDeclarationNotAllowed, \u0081.\u0002.Err_ImplicitRefTypeDeclarationNotAllowed);
			\u0018.\u0001.Add(MessageId.Err_ImplicitRefTypeSignNotSupported, \u0081.\u0002.Err_ImplicitRefTypeSignNotSupported);
			\u0018.\u0001.Add(MessageId.Err_MultipleAssignmentsToInterfaceVariables, \u0081.\u0002.Err_MultipleAssignmentsToInterfaceVariables);
			\u0018.\u0001.Add(MessageId.Wrn_ImplicitCheckFunctionShadowed, \u0081.\u0002.Wrn_ImplicitCheckFunctionShadowed);
			\u0018.\u0001.Add(MessageId.Err_AddressOfNonInstanceVar, \u0081.\u0002.Err_AddressOfNonInstanceVar);
			\u0018.\u0001.Add(MessageId.Err_ImplicitReferenceTypeOnlChangeError, \u0081.\u0002.Err_ImplicitReferenceTypeOnlChangeError);
			\u0018.\u0001.Add(MessageId.Err_NoResolutionForLazyVariable, \u0081.\u0002.Err_NoResolutionForLazyVariable);
			\u0018.\u0001.Add(MessageId.Err_NoVarInputInPropertyAccessors, \u0081.\u0002.Err_NoVarInputInPropertyAccessors);
			\u0018.\u0001.Add(MessageId.Err_MultipleAssignsToSameInputInCall, \u0081.\u0002.Err_MultipleAssignsToSameInputInCall);
			\u0018.\u0001.Add(MessageId.Err_RefAssignOnlyForReferenceTypes, \u0081.\u0002.Err_RefAssignOnlyForReferenceTypes);
			\u0018.\u0001.Add(MessageId.Err_OutOfPersistentMemoryExplicit, \u0081.\u0002.Err_OutOfPersistentMemoryExplicit);
			\u0018.\u0001.Add(MessageId.Err_OutOfPersistentMemoryImplicit, \u0081.\u0002.Err_OutOfPersistentMemoryImplicit);
			\u0018.\u0001.Add(MessageId.Err_LibraryNamespaceNotValid, \u0081.\u0002.Err_LibraryNamespaceNotValid);
			\u0018.\u0001.Add(MessageId.Err_LValueForVarinoutStrings, \u0081.\u0002.Err_LValueForVarinoutStrings);
			\u0018.\u0001.Add(MessageId.Err_StringTooShortForVarInOut, \u0081.\u0002.Err_StringTooShortForVarInOut);
			\u0018.\u0001.Add(MessageId.Err_NoInputsWithSlotAttributes, \u0081.\u0002.Err_NoInputsWithSlotAttributes);
			\u0018.\u0001.Add(MessageId.Err_NotEnoughMemoryForCompactDownload, \u0081.\u0002.Err_NotEnoughMemoryForCompactDownload);
			\u0018.\u0001.Add(MessageId.Wrn_ExtendsForInterfaces, \u0081.\u0002.Wrn_ExtendsForInterfaces);
			\u0018.\u0001.Add(MessageId.Wrn_GranularityMismatchForDirectVariable, \u0081.\u0002.Wrn_GranularityMismatchForDirectVariable);
			\u0018.\u0001.Add(MessageId.Err_MultipleAssignmentWithReferences, \u0081.\u0002.Err_MultipleAssignmentWithReferences);
			\u0018.\u0001.Add(MessageId.Err_MultipleAssignmentWithChangingValueTypes, \u0081.\u0002.Err_MultipleAssignmentWithChangingValueTypes);
			\u0018.\u0001.Add(MessageId.Err_NoMemoryReserveForExternal, \u0081.\u0002.Err_NoMemoryReserveForExternal);
			\u0018.\u0001.Add(MessageId.Wrn_AtLeastOneExpected, \u0081.\u0002.Wrn_AtLeastOneExpected);
			\u0018.\u0001.Add(MessageId.Err_UnknownMaxStackSize, \u0081.\u0002.Err_UnknownMaxStackSize);
			\u0018.\u0001.Add(MessageId.Err_AbstractOnFunctionBlocksAndMethodsOnly, \u0081.\u0002.Err_AbstractOnFunctionBlocksAndMethodsOnly);
			\u0018.\u0001.Add(MessageId.Err_AbstractAndFinalNotPossible, \u0081.\u0002.Err_AbstractAndFinalNotPossible);
			\u0018.\u0001.Add(MessageId.Err_AbstractAndPrivateNotPossible, \u0081.\u0002.Err_AbstractAndPrivateNotPossible);
			\u0018.\u0001.Add(MessageId.Err_AbstractMethodNotImplemented, \u0081.\u0002.Err_AbstractMethodNotImplemented);
			\u0018.\u0001.Add(MessageId.Err_AbstractMethodOnlyInAbstractFunctionblock, \u0081.\u0002.Err_AbstractMethodOnlyInAbstractFunctionblock);
			\u0018.\u0001.Add(MessageId.Err_AbstractMethodMustNotContainAnyStatements, \u0081.\u0002.Err_AbstractMethodMustNotContainAnyStatements);
			\u0018.\u0001.Add(MessageId.Err_AbstractFunctionBlockInstance, \u0081.\u0002.Err_AbstractFunctionBlockInstance);
			\u0018.\u0001.Add(MessageId.Err_AbstractPropertyNotImplemented, \u0081.\u0002.Err_AbstractPropertyNotImplemented);
			\u0018.\u0001.Add(MessageId.Err_StringLengthIsNoConstant, \u0081.\u0002.Err_StringLengthIsNoConstant);
			\u0018.\u0001.Add(MessageId.Err_NotAllowedInTaskLocalVariables, \u0081.\u0002.Err_NotAllowedInTaskLocalVariables);
			\u0018.\u0001.Add(MessageId.Err_TaskLocalVariablesWriterTaskNotDefined, \u0081.\u0002.Err_TaskLocalVariablesWriterTaskNotDefined);
			\u0018.\u0001.Add(MessageId.Err_TaskLocalVariablesAccessNotAllowed, \u0081.\u0002.Err_TaskLocalVariablesAccessNotAllowed);
			\u0018.\u0001.Add(MessageId.Err_TaskLocalVariablesNoOnlineChangePossible, \u0081.\u0002.Err_TaskLocalVariablesNoOnlineChangePossible);
			\u0018.\u0001.Add(MessageId.Wrn_VarInOutUnitializedInInitialValue, \u0081.\u0002.Wrn_VarInOutUnitializedInInitialValue);
			\u0018.\u0001.Add(MessageId.Err_AbstractMethodStaticCall, \u0081.\u0002.Err_AbstractMethodStaticCall);
			\u0018.\u0001.Add(MessageId.Err_AbstractPropertyStaticCall, \u0081.\u0002.Err_AbstractPropertyStaticCall);
			\u0018.\u0001.Add(MessageId.Err_InconsistentUseOfCPPCompatibility, \u0081.\u0002.Err_InconsistentUseOfCPPCompatibility);
			\u0018.\u0001.Add(MessageId.Err_AbstractWrongVarInMethod, \u0081.\u0002.Err_AbstractWrongVarInMethod);
			\u0018.\u0001.Add(MessageId.Err_NoValuePassingForCPPExternal, \u0081.\u0002.Err_NoValuePassingForCPPExternal);
			\u0018.\u0001.Add(MessageId.Wrn_OnlyConstantInitialValueForMappedPersistentVar, \u0081.\u0002.Wrn_OnlyConstantInitialValueForMappedPersistentVar);
			\u0018.\u0001.Add(MessageId.Err_OperatorNotSupportedVersion, \u0081.\u0002.Err_OperatorNotSupportedVersion);
			\u0018.\u0001.Add(MessageId.Err_ComparisonOperatorExpected, \u0081.\u0002.Err_ComparisonOperatorExpected);
			\u0018.\u0001.Add(MessageId.Err_StringLiteralExpected, \u0081.\u0002.Err_StringLiteralExpected);
			\u0018.\u0001.Add(MessageId.Err_VersionOverflow, \u0081.\u0002.Err_VersionOverflow);
			\u0018.\u0001.Add(MessageId.Err_VersionPartNegative, \u0081.\u0002.Err_VersionPartNegative);
			\u0018.\u0001.Add(MessageId.Err_VersionInvalidFormat, \u0081.\u0002.Err_VersionInvalidFormat);
			\u0018.\u0001.Add(MessageId.Err_NoNewAssignmentInOtherExpression, \u0081.\u0002.Err_NoNewAssignmentInOtherExpression);
			\u0018.\u0001.Add(MessageId.Err_ExplicitTransitionAssignMissing, \u0081.\u0002.Err_ExplicitTransitionAssignMissing);
			\u0018.\u0001.Add(MessageId.Wrn_AmbiguousCheckfunctionInLibrary, \u0081.\u0002.Wrn_AmbiguousCheckfunctionInLibrary);
		}

		// Token: 0x06003565 RID: 13669 RVA: 0x000D3BCC File Offset: 0x000D1DCC
		private static void \u0002()
		{
			\u0018.\u0001.Add(MessageId.Err_VectorSizeIsNoConstant, \u0081.\u0002.Err_VectorSizeIsNoConstant);
			\u0018.\u0001.Add(MessageId.Err_VectorSizeNotValid, \u0081.\u0002.Err_VectorSizeNotValid);
			\u0018.\u0001.Add(MessageId.Err_VectorNoPersistentRetain, \u0081.\u0002.Err_VectorNoPersistentRetain);
			\u0018.\u0001.Add(MessageId.Err_VectorBaseMustBeRealType, \u0081.\u0002.Err_VectorBaseMustBeRealType);
			\u0018.\u0001.Add(MessageId.Err_VectorTypesNotCompatible, \u0081.\u0002.Err_VectorTypesNotCompatible);
			\u0018.\u0001.Add(MessageId.Err_VectorTypeCantBePlacedInUnion, \u0081.\u0002.Err_VectorTypeCantBePlacedInUnion);
			\u0018.\u0001.Add(MessageId.Err_LowerUpperBoundOperandNotInRange, \u0081.\u0002.Err_LowerUpperBoundOperandNotInRange);
			\u0018.\u0001.Add(MessageId.Err_LowerUpperBoundOperandNotExactly, \u0081.\u0002.Err_LowerUpperBoundOperandNotExactly);
			\u0018.\u0001.Add(MessageId.Wrn_Ambiguity, \u0081.\u0002.Err_Ambiguity);
			\u0018.\u0001.Add(MessageId.Err_MultipleAssignmentsNotAllowedForOperator, \u0081.\u0002.Err_MultipleAssignmentsNotAllowedForOperator);
			\u0018.\u0001.Add(MessageId.Err_AbstractFunctionBlockAssigned, \u0081.\u0002.Err_AbstractFunctionBlockAssigned);
			\u0018.\u0001.Add(MessageId.Wrn_CallOfPrivateProperty, \u0081.\u0002.Wrn_CallOfPrivateProperty);
			\u0018.\u0001.Add(MessageId.Wrn_AccessToInternalProperty, \u0081.\u0002.Wrn_AccessToInternalProperty);
			\u0018.\u0001.Add(MessageId.Wrn_CallOfProtectedProperty, \u0081.\u0002.Wrn_CallOfProtectedProperty);
			\u0018.\u0001.Add(MessageId.Wrn_AccessToInternalVariable, \u0081.\u0002.Wrn_AccessToInternalVariable);
			\u0018.\u0001.Add(MessageId.Wrn_AccessToInternalObject, \u0081.\u0002.Wrn_AccessToInternalObject);
			\u0018.\u0001.Add(MessageId.Err_InvalidAccessPathForNamespaceAccess, \u0081.\u0002.Err_InvalidAccessPathForNamespaceAccess);
			\u0018.\u0001.Add(MessageId.Err_InvalidNamespaceForNamespaceAccess, \u0081.\u0002.Err_InvalidNamespaceForNamespaceAccess);
			\u0018.\u0001.Add(MessageId.Err_UnknownCompilerVersionInCompiledLib, \u0081.\u0002.Err_UnknownCompilerVersionInCompiledLib);
			\u0018.\u0001.Add(MessageId.Wrn_PersistentVariableOnStack, \u0081.\u0002.Wrn_PersistentVariableOnStack);
			\u0018.\u0001.Add(MessageId.Err_InconsistentUseOfCPPCompatibilityMissingParent, \u0081.\u0002.Err_InconsistentUseOfCPPCompatibility_MissingParent);
			\u0018.\u0001.Add(MessageId.Err_WrongReInit, \u0081.\u0002.Err_WrongReInit);
			\u0018.\u0001.Add(MessageId.Wrn_InvalidDefaultValue, \u0081.\u0002.Wrn_InvalidDefaultValue);
			\u0018.\u0001.Add(MessageId.Wrn_DefaultValueNotConstant, \u0081.\u0002.Wrn_DefaultValueNotConstant);
			\u0018.\u0001.Add(MessageId.Wrn_DefaultValueTopLevel, \u0081.\u0002.Wrn_DefaultValueTopLevel);
			\u0018.\u0001.Add(MessageId.Err_WrongCallAfterGlobalInitSlotSignature, \u0081.\u0002.Err_WrongCallAfterGlobalInitSlotSignature);
			\u0018.\u0001.Add(MessageId.Inf_UseXSizeOfOperator, \u0081.\u0002.Inf_UseXSizeOfOperator);
			\u0018.\u0001.Add(MessageId.Err_InvalidInitialisationForVarInst, \u0081.\u0002.Err_InvalidInitialisationForVarInst);
			\u0018.\u0001.Add(MessageId.Err_NoPropertyForVarInout, \u0081.\u0002.Err_NoPropertyForVarInout);
			\u0018.\u0001.Add(MessageId.Err_InterfaceChanged_NumberOfInputsOutputsDifferent, \u0081.\u0002.Err_InterfaceChanged_NumberOfInputsOutputsDifferent);
			\u0018.\u0001.Add(MessageId.Err_InterfaceChanged_VariableDifferent, \u0081.\u0002.Err_InterfaceChanged_VariableDifferent);
			\u0018.\u0001.Add(MessageId.Wrn_MissingAttributeNoAssign, \u0081.\u0002.Wrn_MissingAttributeNoAssign);
			\u0018.\u0001.Add(MessageId.Err_NoMemoryAllocationCallback, \u0081.\u0002.Err_NoMemoryAllocationCallback);
			\u0018.\u0001.Add(MessageId.Wrn_NoInheritanceForUnions, \u0081.\u0002.Wrn_NoInheritanceForUnions);
			\u0018.\u0001.Add(MessageId.Wrn_ReservedUnusedKeyword, \u0081.\u0002.Wrn_ReservedUnusedKeyword);
			\u0018.\u0001.Add(MessageId.Err_NoStaticVariableInitialisationForValue, \u0081.\u0002.Err_NoStaticVariableInitialisationForValue);
			\u0018.\u0001.Add(MessageId.Err_AttributeNotAllowedFor, \u0081.\u0002.Err_AttributeNotAllowedFor);
			\u0018.\u0001.Add(MessageId.Err_NoExplicitCall, \u0081.\u0002.Err_NoExplicitCall);
			\u0018.\u0001.Add(MessageId.Wrn_NonAsciiStringLiteral, \u0081.\u0002.Wrn_NonAsciiStringLiteral);
			\u0018.\u0001.Add(MessageId.Err_FCallWrongNumberOfArguments, \u0081.\u0002.Err_FCallWrongNumberOfArguments);
			\u0018.\u0001.Add(MessageId.Err_FCallWrongCallPatternSignature, \u0081.\u0002.Err_FCallWrongCallPatternSignature);
			\u0018.\u0001.Add(MessageId.Err_FCallExpectedPointerAsSecondArgument, \u0081.\u0002.Err_FCallExpectedPointerAsSecondArgument);
			\u0018.\u0001.Add(MessageId.Err_NewOnInterfaceNotPossible, \u0081.\u0002.Err_NewOnInterfaceNotPossible);
			\u0018.\u0001.Add(MessageId.Err_PartialAccess_OnlyOnBitTypes, \u0081.\u0002.Err_PartialAccess_OnlyOnBitTypes);
			\u0018.\u0001.Add(MessageId.Err_PartialAccess_NotAValidComponent, \u0081.\u0002.Err_PartialAccess_NotAValidComponent);
			\u0018.\u0001.Add(MessageId.Wrn_ReferenceToUninitializedVariable, \u0081.\u0002.Wrn_ReferenceToUninitializedVariable);
			\u0018.\u0001.Add(MessageId.Wrn_WrongDestructor, \u0081.\u0002.Wrn_WrongDestructor);
			\u0018.\u0001.Add(MessageId.Wrn_WrongReInit, \u0081.\u0002.Wrn_WrongReInit);
			\u0018.\u0001.Add(MessageId.Wrn_NotAllowedInInterfaceLib, \u0081.\u0002.Wrn_NotAllowedInInterfaceLib);
			\u0018.\u0001.Add(MessageId.Wrn_InterfaceChangedBase, \u0081.\u0002.Wrn_InterfaceChangedBase);
			\u0018.\u0001.Add(MessageId.Wrn_MissingInstancePathForPersistent, \u0081.\u0002.Wrn_MissingInstancePathForPersistent);
			\u0018.\u0001.Add(MessageId.Err_ProjectDefinedNotSupportedFor, \u0081.\u0002.Err_ProjectDefinedNotSupportedFor);
			\u0018.\u0001.Add(MessageId.Wrn_ExitForRetainInstances, \u0081.\u0002.Wrn_ExitForRetainInstances);
			\u0018.\u0001.Add(MessageId.Wrn_UninitialisedVariableUsedInInitialisation, \u0081.\u0002.Wrn_UninitialisedVariableUsedInInitialisation);
			\u0018.\u0001.Add(MessageId.Wrn_AbstractKeywordMissing, \u0081.\u0002.Wrn_AbstractKeywordMissing);
			\u0018.\u0001.Add(MessageId.Err_NoOnlineChangeOnGenericConstantType, \u0081.\u0002.Err_NoOnlineChangeOnGenericConstantType);
			\u0018.\u0001.Add(MessageId.Err_NoOnlineChangeOnGenericConstantBaseType, \u0081.\u0002.Err_NoOnlineChangeOnGenericConstantBaseType);
			\u0018.\u0001.Add(MessageId.Err_AccessVarInstFromOutsideTheDeclaringMethod, \u0081.\u0002.Err_AccessVarInstFromOutsideTheDeclaringMethod);
			\u0018.\u0001.Add(MessageId.Err_NoCopyCodeForVariableAtDirectAddress, \u0081.\u0002.Err_NoCopyCodeForVariableAtDirectAddress);
			\u0018.\u0001.Add(MessageId.Err_UnexpectedStatement, \u0081.\u0002.Err_UnexpectedStatement);
			\u0018.\u0001.Add(MessageId.Err_UnsupportedFeature, \u0081.\u0002.Err_UnsupportedFeature);
			\u0018.\u0001.Add(MessageId.Wrn_ConstantInStructDeclaration, \u0081.\u0002.Wrn_ConstantInStructDeclaration);
			\u0018.\u0001.Add(MessageId.Err_NoMatchingOverload, \u0081.\u0002.Err_NoMatchingOverload);
			\u0018.\u0001.Add(MessageId.Err_OverloadNeedsAttribute, \u0081.\u0002.Err_OverloadNeedsAttribute);
			\u0018.\u0001.Add(MessageId.Err_OverloadWithSameInputs, \u0081.\u0002.Err_OverloadWithSameInputs);
			\u0018.\u0001.Add(MessageId.Err_MaxNestingDepthExceeded, \u0081.\u0002.Err_MaxNestingDepthExceeded);
			\u0018.\u0001.Add(MessageId.Err_MissingImplementationTerminator, \u0081.\u0002.Err_MissingImplementationTerminator);
			\u0018.\u0001.Add(MessageId.Err_UnknownEmbeddedLanguageType, \u0081.\u0002.Err_UnknownEmbeddedLanguageType);
			\u0018.\u0001.Add(MessageId.Err_NoResolutionForSomeLazyVariables, \u0081.\u0002.Err_NoResolutionForSomeLazyVariables);
			\u0018.\u0001.Add(MessageId.Wrn_ChangeOfAccessModifier, \u0081.\u0002.Wrn_ChangeOfAccessModifier);
		}

		// Token: 0x06003566 RID: 13670 RVA: 0x000D4154 File Offset: 0x000D2354
		private static void \u0003()
		{
			\u0018.\u0001.Add(MessageId.Err_PersistentVariablesChangeMessageText, \u0081.\u0002.Err_PersistentVariablesChangeMessageText);
			\u0018.\u0001.Add(MessageId.Err_CancelledByUser, \u0081.\u0002.Err_CancelledByUser);
			\u0018.\u0001.Add(MessageId.Err_StructureInitialisationNotPossible, \u0081.\u0002.Err_StructureInitialisationNotPossible);
			\u0018.\u0001.Add(MessageId.Err_ArrayInitialisationNotPossible, \u0081.\u0002.Err_ArrayInitialisationNotPossible);
			\u0018.\u0001.Add(MessageId.Err_StructuredValueTypeInExternalCall, \u0081.\u0002.Err_StructuredValueTypeInExternalCall);
			\u0018.\u0001.Add(MessageId.Err_RecursiveConstantInitialisation, \u0081.\u0002.Err_RecursiveConstantInitialisation);
			\u0018.\u0001.Add(MessageId.Wrn_ShiftExceedsTypeSize, \u0081.\u0002.Wrn_ShiftExceedsTypeSize);
			\u0018.\u0001.Add(MessageId.Err_OperatorNotSupported, \u0081.\u0002.Err_OperatorNotSupported);
			\u0018.\u0001.Add(MessageId.Err_LValueForAnyVar, \u0081.\u0002.Err_LValueForAnyVar);
			\u0018.\u0001.Add(MessageId.Err_AnyTypeOnlyInFunction, \u0081.\u0002.Err_AnyTypeOnlyInFunction);
			\u0018.\u0001.Add(MessageId.Wrn_ConcurrentAccessOfBitInSameByte, \u0081.\u0002.Wrn_ConcurrentAccessOfBitInSameByte);
			\u0018.\u0001.Add(MessageId.Err_NoInitialForInoutConstant, \u0081.\u0002.Err_NoInitialForInoutConstant);
			\u0018.\u0001.Add(MessageId.Err_VariableForVarinoutConstant, \u0081.\u0002.Err_VariableForVarinoutConstant);
			\u0018.\u0001.Add(MessageId.Wrn_MethodAlreadyCalledImplicitly, \u0081.\u0002.Wrn_MethodAlreadyCalledImplicitly);
			\u0018.\u0001.Add(MessageId.Err_LiteralExpected, \u0081.\u0002.Err_LiteralExpected);
			\u0018.\u0001.Add(MessageId.Err_ExprNoConstantError, \u0081.\u0002.Err_ExprNoConstantError);
			\u0018.\u0001.Add(MessageId.Err_NotAllowedInInterfaceLib, \u0081.\u0002.Err_NotAllowedInInterfaceLib);
			\u0018.\u0001.Add(MessageId.Err_NotAllowedInContainerLib, \u0081.\u0002.Err_NotAllowedInContainerLib);
			\u0018.\u0001.Add(MessageId.Err_InterfaceMethodImplementationNotPublic, \u0081.\u0002.Err_InterfaceMethodImplementationNotPublic);
			\u0018.\u0001.Add(MessageId.Err_NoVarConfigInMethodOrFunction, \u0081.\u0002.Err_NoVarConfigInMethodOrFunction);
			\u0018.\u0001.Add(MessageId.Err_CantHaveBaseClass, \u0081.\u0002.Err_CantHaveBaseClass);
			\u0018.\u0001.Add(MessageId.Err_NoIndirectPropertyCallOnStringWithSize, \u0081.\u0002.Err_NoIndirectPropertyCallOnStringWithSize);
			\u0018.\u0001.Add(MessageId.Wrn_GlobalInitSlotNotForVariables, \u0081.\u0002.Wrn_GlobalInitSlotNotForVariables);
			\u0018.\u0001.Add(MessageId.Err_LibraryWithUnicodeIdentifiers, \u0081.\u0002.Err_LibraryWithUnicodeIdentifiers);
			\u0018.\u0001.Add(MessageId.Wrn_ImplicitEnumConversion, \u0081.\u0002.Wrn_ImplicitEnumConversion);
			\u0018.\u0001.Add(MessageId.Err_NoAssign, \u0081.\u0002.Err_NoAssign);
			\u0018.\u0001.Add(MessageId.Err_DuplicateVarConfig, \u0081.\u0002.Err_DuplicateVarConfig);
			\u0018.\u0001.Add(MessageId.Err_TryCatchNotSupportedCodegenerator, \u0081.\u0002.Err_TryCatchNotSupportedCodegenerator);
			\u0018.\u0001.Add(MessageId.Err_TryCatchNotSupportedVersion, \u0081.\u0002.Err_TryCatchNotSupportedVersion);
			\u0018.\u0001.Add(MessageId.Err_MappedVarWrittenInDiffTasks, \u0081.\u0002.Err_MappedVarWrittenInDiffTasks);
			\u0018.\u0001.Add(MessageId.Err_LibNamespaceConflict, \u0081.\u0002.Err_LibNamespaceConflict);
			\u0018.\u0001.Add(MessageId.Wrn_ComplexExpression, \u0081.\u0002.Wrn_ComplexExpression);
			\u0018.\u0001.Add(MessageId.Err_VarInstOnlyInMethods, \u0081.\u0002.Err_VarInstOnlyInMethods);
			\u0018.\u0001.Add(MessageId.Err_LibSupports32BitOnly, \u0081.\u0002.Err_LibSupports32BitOnly);
			\u0018.\u0001.Add(MessageId.Wrn_BoolNotAtBitAddress, \u0081.\u0002.Wrn_BoolNotAtBitAddress);
			\u0018.\u0001.Add(MessageId.Err_CheckLicenseNeedsSysTarget, \u0081.\u0002.Err_CheckLicenseNeedsSysTarget);
			\u0018.\u0001.Add(MessageId.Err_OperatorNotAllowedAtPosition, \u0081.\u0002.Err_OperatorNotAllowedAtPosition);
			\u0018.\u0001.Add(MessageId.Err_InstanceNotAllowedInRetain, \u0081.\u0002.Err_InstanceNotAllowedInRetain);
			\u0018.\u0001.Add(MessageId.Err_ReferenceToInput, \u0081.\u0002.Err_ReferenceToInput);
			\u0018.\u0001.Add(MessageId.Wrn_StruturedTypePropertyNotMonitorable, \u0081.\u0002.Wrn_StruturedTypePropertyNotMonitorable);
			\u0018.\u0001.Add(MessageId.Err_TryCatchNotSupported, \u0081.\u0002.Err_TryCatchNotSupported);
			\u0018.\u0001.Add(MessageId.Err_NoDirectAddressInSubsequentVarDecl, \u0081.\u0002.Err_NoDirectAddressInSubsequentVarDecl);
			\u0018.\u0001.Add(MessageId.Err_UnexpectedOperandForCallInitFunction, \u0081.\u0002.Err_UnexpectedOperandForCallInitFunction);
			\u0018.\u0001.Add(MessageId.Err_BitAdrOnOperation, \u0081.\u0002.Err_BitAdrOnOperation);
			\u0018.\u0001.Add(MessageId.Wrn_AttributeCheck, \u0081.\u0002.Wrn_AttributeCheck);
			\u0018.\u0001.Add(MessageId.Err_MaxArraySizeExceeded, \u0081.\u0002.Err_MaxArraySizeExceeded);
			\u0018.\u0001.Add(MessageId.Err_RefAssignNeedsLValue, \u0081.\u0002.Err_RefAssignNeedsLValue);
			\u0018.\u0001.Add(MessageId.Wrn_EnumComparison, \u0081.\u0002.Wrn_EnumComparison);
			\u0018.\u0001.Add(MessageId.Wrn_NoPointerToBit, \u0081.\u0002.Wrn_NoPointerToBit);
			\u0018.\u0001.Add(MessageId.Err_AttributeNotValidForNonExternalPOU, \u0081.\u0002.Err_AttributeNotValidForNonExternalPOU);
			\u0018.\u0001.Add(MessageId.Wrn_Obsolete, \u0081.\u0002.Wrn_Obsolete);
			\u0018.\u0001.Add(MessageId.Err_StrictEnumNotAMember, \u0081.\u0002.Err_StrictEnumNotAMember);
			\u0018.\u0001.Add(MessageId.Err_StrictEnumNoArithmeticAllowed, \u0081.\u0002.Err_StrictEnumNoArithmeticAllowed);
			\u0018.\u0001.Add(MessageId.Err_ParameterlistNotConst, \u0081.\u0002.Err_ParameterlistNotConst);
			\u0018.\u0001.Add(MessageId.Err_NoMixExternalIECInheritance, \u0081.\u0002.Err_NoMixExternalIECInheritance);
			\u0018.\u0001.Add(MessageId.Err_InvalidStringSize, \u0081.\u0002.Wrn_InvalidStringSize);
			\u0018.\u0001.Add(MessageId.Err_UserCheckFunctionsNotSupported, \u0081.\u0002.Err_UserCheckFunctionsNotSupported);
			\u0018.\u0001.Add(MessageId.Err_CallAfterInitHasInputs, \u0081.\u0002.Err_CallAfterInitHasInputs);
			\u0018.\u0001.Add(MessageId.Err_GeneratingVarInitializations, \u0081.\u0002.Err_GeneratingVarInitializations);
			\u0018.\u0001.Add(MessageId.Err_ImplicitEnumerationTypeNotExpected, \u0081.\u0002.Err_ImplicitEnumerationTypeNotExpected);
			\u0018.\u0001.Add(MessageId.Err_InvalidEnumDefaultValue, \u0081.\u0002.Err_InvalidEnumDefaultValue);
			\u0018.\u0001.Add(MessageId.Err_FeatureNotImplemented, \u0081.\u0002.Err_FeatureNotImplemented);
			\u0018.\u0001.Add(MessageId.Wrn_NonLocalAccessToVarInOut, \u0081.\u0002.Wrn_NonLocalAccessToVarInOut);
			\u0018.\u0001.Add(MessageId.Err_DuplicateElseInCaseStatement, \u0081.\u0002.Err_DuplicateElseInCaseStatement);
			\u0018.\u0001.Add(MessageId.Wrn_Pragma, \u0081.\u0002.Wrn_Pragma);
			\u0018.\u0001.Add(MessageId.Err_DivisionByZero, \u0081.\u0002.Err_DivisionByZero);
			\u0018.\u0001.Add(MessageId.Err_InvalidEnumBaseType, \u0081.\u0002.Err_InvalidEnumBaseType);
			\u0018.\u0001.Add(MessageId.Err_TooFewParametersForExtensibleFunction, \u0081.\u0002.Err_TooFewParametersForExtensibleFunction);
			\u0018.\u0001.Add(MessageId.Err_TooManyParametersForExtensibleFunction, \u0081.\u0002.Err_TooManyParametersForExtensibleFunction);
			\u0018.\u0001.Add(MessageId.Err_NoFormalParamsCallsForExtensibleFunction, \u0081.\u0002.Err_NoFormalParamsCallsForExtensibleFunction);
			\u0018.\u0001.Add(MessageId.Err_IntegerLiteralExpected, \u0081.\u0002.Err_IntegerLiteralExpected);
			\u0018.\u0001.Add(MessageId.Err_LowerUpperBoundOnVariableLengthArrayOnly, \u0081.\u0002.Err_LowerUpperBoundOnVariableLengthArrayOnly);
			\u0018.\u0001.Add(MessageId.Err_OperatorNoValidVariableName, \u0081.\u0002.Err_OperatorNoValidVariableName);
			\u0018.\u0001.Add(MessageId.Err_InconsistentInheritanceOfCPPCompatibility, \u0081.\u0002.Err_InconsistentInheritanceOfCPPCompatibility);
			\u0018.\u0001.Add(MessageId.Err_IncompletePOUDeclaration, \u0081.\u0002.Err_IncompletePOUDeclaration);
			\u0018.\u0001.Add(MessageId.Err_SelMuxOnlyEqualUserDefTypes, \u0081.\u0002.Err_SelMuxOnlyEqualUserDefTypes);
			\u0018.\u0001.Add(MessageId.Err_VarLengthArrayInOut, \u0081.\u0002.Err_VarLengthArrayInOut);
			\u0018.\u0001.Add(MessageId.Err_VarLengthArrayTopLevel, \u0081.\u0002.Err_VarLengthArrayTopLevel);
			\u0018.\u0001.Add(MessageId.Err_OutParamNotEqual, \u0081.\u0002.Err_OutParamNotEqual);
			\u0018.\u0001.Add(MessageId.Wrn_LibWithStringInVarInOut, \u0081.\u0002.Wrn_LibWithStringInVarInOut);
			\u0018.\u0001.Add(MessageId.Wrn_LValueForVarinoutStrings, \u0081.\u0002.Wrn_LValueForVarinoutStrings);
			\u0018.\u0001.Add(MessageId.Err_AddressSourceIsAddressDest, \u0081.\u0002.Err_AddressSourceIsAddressDest);
			\u0018.\u0001.Add(MessageId.Err_NoCopyCodeAllowed, \u0081.\u0002.Err_NoCopyCodeAllowed);
			\u0018.\u0001.Add(MessageId.Err_ATDeclarationNotAllowed, \u0081.\u0002.Err_ATDeclarationNotAllowed);
			\u0018.\u0001.Add(MessageId.Err_ArrayBorderNoValidSignedInteger, \u0081.\u0002.Err_ArrayBorderNoValidSignedInteger);
			\u0018.\u0001.Add(MessageId.Err_NumOfInitializersDoNotMatch, \u0081.\u0002.Err_NumOfInitializersDoNotMatch);
			\u0018.\u0001.Add(MessageId.Err_IsValidRefNeedsReference, \u0081.\u0002.Err_IsValidRefNeedsReference);
			\u0018.\u0001.Add(MessageId.Err_ImplicitMethodImplementationNotPublic, \u0081.\u0002.Err_ImplicitMethodImplementationNotPublic);
			\u0018.\u0001.Add(MessageId.Err_SystemOutOfMemory, \u0081.\u0002.Err_SystemOutOfMemory);
			\u0018.\u0001.Add(MessageId.Err_ImplicitReferenceTypeDeclNotAllowed, \u0081.\u0002.Err_ImplicitReferenceTypeDeclNotAllowed);
			\u0018.\u0001.Add(MessageId.Err_ImplicitRefTypeIsnotAllowedAsBase, \u0081.\u0002.Err_ImplicitRefTypeIsnotAllowedAsBase);
		}

		// Token: 0x06003567 RID: 13671 RVA: 0x000D4880 File Offset: 0x000D2A80
		private static void \u0004()
		{
			\u0018.\u0001.Add(MessageId.Err_InOutParamNotEqual, \u0081.\u0002.Err_InOutParamNotEqual);
			\u0018.\u0001.Add(MessageId.Err_ApplicationConflict, \u0081.\u0002.Err_ApplicationConflict);
			\u0018.\u0001.Add(MessageId.Err_BitsForStructureOnly, \u0081.\u0002.Err_BitsForStructureOnly);
			\u0018.\u0001.Add(MessageId.Err_BitsWrongScope, \u0081.\u0002.Err_BitsWrongScope);
			\u0018.\u0001.Add(MessageId.Err_NoPointerToBit, \u0081.\u0002.Err_NoPointerToBit);
			\u0018.\u0001.Add(MessageId.Err_NoArrayOfBit, \u0081.\u0002.Err_NoArrayOfBit);
			\u0018.\u0001.Add(MessageId.Err_NoSystemDefine, \u0081.\u0002.Err_NoSystemDefine);
			\u0018.\u0001.Add(MessageId.Err_NoNamespace, \u0081.\u0002.Err_NoNamespace);
			\u0018.\u0001.Add(MessageId.Err_ModNotReal, \u0081.\u0002.Err_ModNotReal);
			\u0018.\u0001.Add(MessageId.Wrn_TooManyApplications, \u0081.\u0002.Wrn_TooManyApplications);
			\u0018.\u0001.Add(MessageId.Err_VariableDeclarationExpected, \u0081.\u0002.Err_VariableDeclarationExpected);
			\u0018.\u0001.Add(MessageId.Err_VariableListExpected, \u0081.\u0002.Err_VariableListExpected);
			\u0018.\u0001.Add(MessageId.Err_GlobalVariableListExpected, \u0081.\u0002.Err_GlobalVariableListExpected);
			\u0018.\u0001.Add(MessageId.Err_PersistentWrongVariable, \u0081.\u0002.Err_PersistentWrongVariable);
			\u0018.\u0001.Add(MessageId.Err_PersistentNoAddress, \u0081.\u0002.Err_PersistentNoAddress);
			\u0018.\u0001.Add(MessageId.Err_CaseLabelDuplicate, \u0081.\u0002.Err_CaseLabelDuplicate);
			\u0018.\u0001.Add(MessageId.Err_CaseLabelInCaseRange, \u0081.\u0002.Err_CaseLabelInCaseRange);
			\u0018.\u0001.Add(MessageId.Err_CaseLabelNoConstant, \u0081.\u0002.Err_CaseLabelNoConstant);
			\u0018.\u0001.Add(MessageId.Err_CaseRangesOverlapping, \u0081.\u0002.Err_CaseRangesOverlapping);
			\u0018.\u0001.Add(MessageId.Wrn_DeviceNotInstalledForSimulation, \u0081.\u0002.Wrn_DeviceNotInstalledForSimulation);
			\u0018.\u0001.Add(MessageId.Err_MalformedAddress, \u0081.\u0002.Err_MalformedAddress);
			\u0018.\u0001.Add(MessageId.Err_NoReferenceToOutput, \u0081.\u0002.Err_NoReferenceToOutput);
			\u0018.\u0001.Add(MessageId.Err_CallRecursion, \u0081.\u0002.Err_CallRecursion);
			\u0018.\u0001.Add(MessageId.Err_NotAnInstanceOf, \u0081.\u0002.Err_NotAnInstanceOf);
			\u0018.\u0001.Add(MessageId.Err_NotEnoughMemoryForVariableInit, \u0081.\u0002.Err_NotEnoughMemoryForVariableInit);
			\u0018.\u0001.Add(MessageId.Err_NoConstantInitialisationForValue, \u0081.\u0002.Err_NoConstantInitialisationForValue);
			\u0018.\u0001.Add(MessageId.Wrn_NoInitialValueForConstant, \u0081.\u0002.Wrn_NoInitialValueForConstant);
			\u0018.\u0001.Add(MessageId.Err_BlobInitError, \u0081.\u0002.Err_BlobInitError);
			\u0018.\u0001.Add(MessageId.Err_UnexpectedTypeName, \u0081.\u0002.Err_UnexpectedTypeName);
			\u0018.\u0001.Add(MessageId.Err_SpecialTypeExpected, \u0081.\u0002.Err_SpecialTypeExpected);
			\u0018.\u0001.Add(MessageId.Err_ArrayInitializationExpected, \u0081.\u0002.Err_ArrayInitializationExpected);
			\u0018.\u0001.Add(MessageId.Err_StructureInitializationExpected, \u0081.\u0002.Err_StructureInitializationExpected);
			\u0018.\u0001.Add(MessageId.Err_QueryInterfaceErrorP1, \u0081.\u0002.Err_QueryInterfaceErrorP1);
			\u0018.\u0001.Add(MessageId.Err_QueryInterfaceErrorP2, \u0081.\u0002.Err_QueryInterfaceErrorP2);
			\u0018.\u0001.Add(MessageId.Err_WrongTypeForExternal, \u0081.\u0002.Err_WrongTypeForExternal);
			\u0018.\u0001.Add(MessageId.Err_NoVarForExternal, \u0081.\u0002.Err_NoVarForExternal);
			\u0018.\u0001.Add(MessageId.Err_NoInitialForExternal, \u0081.\u0002.Err_NoInitialForExternal);
			\u0018.\u0001.Add(MessageId.Err_QueryInterfaceP1NoIQuery, \u0081.\u0002.Err_QueryInterfaceP1NoIQuery);
			\u0018.\u0001.Add(MessageId.Err_QueryPointerErrorP1, \u0081.\u0002.Err_QueryPointerErrorP1);
			\u0018.\u0001.Add(MessageId.Err_QueryPointerErrorP2, \u0081.\u0002.Err_QueryPointerErrorP2);
			\u0018.\u0001.Add(MessageId.Err_DeleteNeedsPointer, \u0081.\u0002.Err_DeleteNeedsPointer);
			\u0018.\u0001.Add(MessageId.Err_NamesNotEqual, \u0081.\u0002.Err_NamesNotEqual);
			\u0018.\u0001.Add(MessageId.Err_MissingInstancePathForPersistent, \u0081.\u0002.Err_MissingInstancePathForPersistent);
			\u0018.\u0001.Add(MessageId.Wrn_MissingObjectForPersistent, \u0081.\u0002.Err_MissingObjectForPersistent);
			\u0018.\u0001.Add(MessageId.Err_NoBytesInRetain, \u0081.\u0002.Err_NoBytesInRetain);
			\u0018.\u0001.Add(MessageId.Err_NoPropertiesInOutAssignment, \u0081.\u0002.Err_NoPropertiesInOutAssignment);
			\u0018.\u0001.Add(MessageId.Err_NewNeedsType, \u0081.\u0002.Err_NewNeedsType);
			\u0018.\u0001.Add(MessageId.Err_NewArrayOnUserdefNotAllowed, \u0081.\u0002.Err_NewArrayOnUserdefNotAllowed);
			\u0018.\u0001.Add(MessageId.Err_NewPositionNotOK, \u0081.\u0002.Err_NewPositionNotOK);
			\u0018.\u0001.Add(MessageId.Err_ReferenceNotAllowed, \u0081.\u0002.Err_ReferenceNotAllowed);
			\u0018.\u0001.Add(MessageId.Err_ReferenceNotAllowedForVarInOut, \u0081.\u0002.Err_ReferenceNotAllowedForVarInOut);
			\u0018.\u0001.Add(MessageId.Err_NewOnlyWithAttribute, \u0081.\u0002.Err_NewOnlyWithAttribute);
			\u0018.\u0001.Add(MessageId.Err_NoOnlineChangeOnDynamicObjects, \u0081.\u0002.Err_NoOnlineChangeOnDynamicObjects);
			\u0018.\u0001.Add(MessageId.Err_DynamicMemoryNotSupported, \u0081.\u0002.Err_DynamicMemoryNotSupported);
			\u0018.\u0001.Add(MessageId.Wrn_LoopExitConditionConstant, \u0081.\u0002.Wrn_LoopExitConditionConstant);
			\u0018.\u0001.Add(MessageId.Err_UninitialisedVariableUsedInInitialisation, \u0081.\u0002.Err_UninitialisedVariableUsedInInitialisation);
			\u0018.\u0001.Add(MessageId.Wrn_ValueAssignViaPointerMayChangeVFTable, \u0081.\u0002.Wrn_ValueAssignViaPointerMayChangeVFTable);
			\u0018.\u0001.Add(MessageId.Err_RetainsNotSupported, \u0081.\u0002.Err_RetainsNotSupported);
			\u0018.\u0001.Add(MessageId.Err_NoReferenceToBits, \u0081.\u0002.Err_NoReferenceToBits);
			\u0018.\u0001.Add(MessageId.Err_ImplicitMethodNameForVariable, \u0081.\u0002.Err_ImplicitMethodNameForVariable);
			\u0018.\u0001.Add(MessageId.Err_DeviceNameNoIdent, \u0081.\u0002.Err_DeviceNameNoIdent);
			\u0018.\u0001.Add(MessageId.Err_NoVarTempInSubsequentPrograms, \u0081.\u0002.Err_NoVarTempInSubsequentPrograms);
			\u0018.\u0001.Add(MessageId.Err_RetainNotAccessibleVariable, \u0081.\u0002.Err_RetainNotAccessibleVariable);
			\u0018.\u0001.Add(MessageId.Err_RetainNotAccessiblePOU, \u0081.\u0002.Err_RetainNotAccessiblePOU);
			\u0018.\u0001.Add(MessageId.Err_InvalidInitialisationForArray, \u0081.\u0002.Err_InvalidInitialisationForArray);
			\u0018.\u0001.Add(MessageId.Err_SummarizedLibraryErrors, \u0081.\u0002.Err_SummarizedLibraryErrors);
			\u0018.\u0001.Add(MessageId.Err_FinalOnFunctionBlocksAndMethodsOnly, \u0081.\u0002.Err_FinalOnFunctionBlocksAndMethodsOnly);
			\u0018.\u0001.Add(MessageId.Err_PrivateOnMethodsOnly, \u0081.\u0002.Err_PrivateOnMethodsOnly);
			\u0018.\u0001.Add(MessageId.Err_NoInheritanceOnFinalType, \u0081.\u0002.Err_NoInheritanceOnFinalType);
			\u0018.\u0001.Add(MessageId.Err_NoOverrideOnFinalMethod, \u0081.\u0002.Err_NoOverrideOnFinalMethod);
			\u0018.\u0001.Add(MessageId.Err_NoOverrideOnPrivateMethod, \u0081.\u0002.Err_NoOverrideOnPrivateMethod);
			\u0018.\u0001.Add(MessageId.Err_NoOverrideOnInternalMethod, \u0081.\u0002.Err_NoOverrideOnInternalMethod);
			\u0018.\u0001.Add(MessageId.Err_NoChangeOnAccessModifier, \u0081.\u0002.Err_NoChangeOnAccessModifier);
			\u0018.\u0001.Add(MessageId.Err_CallOfProtectedMethod, \u0081.\u0002.Err_CallOfProtectedMethod);
			\u0018.\u0001.Add(MessageId.Err_CallOfPrivateMethod, \u0081.\u0002.Err_CallOfPrivateMethod);
			\u0018.\u0001.Add(MessageId.Err_CallOfProtectedProperty, \u0081.\u0002.Err_CallOfProtectedProperty);
			\u0018.\u0001.Add(MessageId.Err_CallOfPrivateProperty, \u0081.\u0002.Err_CallOfPrivateProperty);
			\u0018.\u0001.Add(MessageId.Err_AccessToInternalVariable, \u0081.\u0002.Err_AccessToInternalVariable);
			\u0018.\u0001.Add(MessageId.Err_AccessToInternalObject, \u0081.\u0002.Err_AccessToInternalObject);
			\u0018.\u0001.Add(MessageId.Err_AccessToInternalProperty, \u0081.\u0002.Err_AccessToInternalProperty);
			\u0018.\u0001.Add(MessageId.Err_InOutAssignedInActionCall, \u0081.\u0002.Err_InOutAssignedInActionCall);
			\u0018.\u0001.Add(MessageId.Err_SlotFunctionHiddenByVariable, \u0081.\u0002.Err_SlotFunctionHiddenByVariable);
			\u0018.\u0001.Add(MessageId.Err_SlotFunctionAmbiguousName, \u0081.\u0002.Err_SlotFunctionAmbiguousName);
			\u0018.\u0001.Add(MessageId.Err_StackOverflowDetected, \u0081.\u0002.Err_StackOverflowDetected);
			\u0018.\u0001.Add(MessageId.Wrn_StackCheckIncompleteDueToRecursion, \u0081.\u0002.Wrn_StackCheckIncompleteDueToRecursion);
			\u0018.\u0001.Add(MessageId.Err_NoCodegenerator, \u0081.\u0002.Err_NoCodegenerator);
			\u0018.\u0001.Add(MessageId.Err_RelatedPositionTaskX, \u0081.\u0002.Err_RelatedPositionTaskX);
		}

		// Token: 0x06003568 RID: 13672 RVA: 0x000D4F5C File Offset: 0x000D315C
		private static void \u0005()
		{
			\u0018.\u0001.Add(MessageId.Err_DataRecursion, \u0081.\u0002.Err_DataRecursion);
			\u0018.\u0001.Add(MessageId.Err_OutOfRetainMemory, \u0081.\u0002.Err_OutOfRetainMemory);
			\u0018.\u0001.Add(MessageId.Err_OutOfRetainMemoryWithWholeSize, \u0081.\u0002.Err_OutOfRetainMemoryWithWholeSize);
			\u0018.\u0001.Add(MessageId.Err_OutOfMemory, \u0081.\u0002.Err_OutOfMemory);
			\u0018.\u0001.Add(MessageId.Err_OutOfMemoryFunctionPointer, \u0081.\u0002.Err_OutOfMemoryFunctionPointer);
			\u0018.\u0001.Add(MessageId.Err_OutOfMemoryWithWholeSize, \u0081.\u0002.Err_OutOfMemoryWithWholeSize);
			\u0018.\u0001.Add(MessageId.Err_VarTooBig, \u0081.\u0002.Err_VarTooBig);
			\u0018.\u0001.Add(MessageId.Err_NoInputMemory, \u0081.\u0002.Err_NoInputMemory);
			\u0018.\u0001.Add(MessageId.Err_NoOutputMemory, \u0081.\u0002.Err_NoOutputMemory);
			\u0018.\u0001.Add(MessageId.Err_NoMemoryMemory, \u0081.\u0002.Err_NoMemoryMemory);
			\u0018.\u0001.Add(MessageId.Err_AddressOutOfRange, \u0081.\u0002.Err_AddressOutOfRange);
			\u0018.\u0001.Add(MessageId.Err_AddressMisaligned, \u0081.\u0002.Err_AddressMisaligned);
			\u0018.\u0001.Add(MessageId.Err_NoBitTypeOnBitAddress, \u0081.\u0002.Err_NoBitTypeOnBitAddress);
			\u0018.\u0001.Add(MessageId.Err_InvalidJumpDestination, \u0081.\u0002.Err_InvalidJumpDestination);
			\u0018.\u0001.Add(MessageId.Err_CalcNeedsCall, \u0081.\u0002.Err_CalcNeedsCall);
			\u0018.\u0001.Add(MessageId.Err_DuplicateLabelDefinition, \u0081.\u0002.Err_DuplicateLabelDefinition);
			\u0018.\u0001.Add(MessageId.Err_NoSuchLabel, \u0081.\u0002.Err_NoSuchLabel);
			\u0018.\u0001.Add(MessageId.Wrn_LabelNoReference, \u0081.\u0002.Wrn_LabelNoReference);
			\u0018.\u0001.Add(MessageId.Err_WrongConstructor, \u0081.\u0002.Err_WrongConstructor);
			\u0018.\u0001.Add(MessageId.Err_WrongDestructor, \u0081.\u0002.Err_WrongDestructor);
			\u0018.\u0001.Add(MessageId.Err_BaseNotAllowed, \u0081.\u0002.Err_BaseNotAllowed);
			\u0018.\u0001.Add(MessageId.Err_NoValidEnumInit, \u0081.\u0002.Err_NoValidEnumInit);
			\u0018.\u0001.Add(MessageId.Wrn_EnumValueDuplicate, \u0081.\u0002.Wrn_EnumValueDuplicate);
			\u0018.\u0001.Add(MessageId.Err_IndexNumWrong, \u0081.\u0002.Err_IndexNumWrong);
			\u0018.\u0001.Add(MessageId.Err_OutOfCodeMemory, \u0081.\u0002.Err_OutOfCodeMemory);
			\u0018.\u0001.Add(MessageId.Err_NoInstancePath, \u0081.\u0002.Err_NoInstancePath);
			\u0018.\u0001.Add(MessageId.Err_NoValidInstancePath, \u0081.\u0002.Err_NoValidInstancePath);
			\u0018.\u0001.Add(MessageId.Err_MissingParameterList, \u0081.\u0002.Err_MissingParameterList);
			\u0018.\u0001.Add(MessageId.Err_WrongTypeForAdr, \u0081.\u0002.Err_WrongTypeForAdr);
			\u0018.\u0001.Add(MessageId.Err_NoEnclosingLoopExit, \u0081.\u0002.Err_NoEnclosingLoopExit);
			\u0018.\u0001.Add(MessageId.Err_NotSupportedInInterface, \u0081.\u0002.Err_NotSupportedInInterface);
			\u0018.\u0001.Add(MessageId.Err_Ambiguity, \u0081.\u0002.Err_Ambiguity);
			\u0018.\u0001.Add(MessageId.Err_NoMatchingInitMethodFound, \u0081.\u0002.Err_NoMatchingInitMethodFound);
			\u0018.\u0001.Add(MessageId.Wrn_StatementNoEffect, \u0081.\u0002.Wrn_StatementNoEffect);
			\u0018.\u0001.Add(MessageId.Err_LValueNoRefType, \u0081.\u0002.Err_LValueNoRefType);
			\u0018.\u0001.Add(MessageId.Err_LValueForReference, \u0081.\u0002.Err_LValueForReference);
			\u0018.\u0001.Add(MessageId.Err_VariableDuplicate, \u0081.\u0002.Err_VariableDuplicate);
			\u0018.\u0001.Add(MessageId.Err_NoReadAccessToProperty, \u0081.\u0002.Err_NoReadAccessToProperty);
			\u0018.\u0001.Add(MessageId.Err_InheritanceError, \u0081.\u0002.Err_InheritanceError);
			\u0018.\u0001.Add(MessageId.Err_InterfaceImplementationError, \u0081.\u0002.Err_InterfaceImplementationError);
			\u0018.\u0001.Add(MessageId.Err_ParamsNotAllowed, \u0081.\u0002.Err_ParamsNotAllowed);
			\u0018.\u0001.Add(MessageId.Err_NoVarsinInterface, \u0081.\u0002.Err_NoVarsinInterface);
			\u0018.\u0001.Add(MessageId.Err_OneAccessorRequired, \u0081.\u0002.Err_OneAccessorRequired);
			\u0018.\u0001.Add(MessageId.Err_ArrayBorderIsNoConstant, \u0081.\u0002.Err_ArrayBorderIsNoConstant);
			\u0018.\u0001.Add(MessageId.Err_ArrayInitialisationCountNoConstant, \u0081.\u0002.Err_ArrayInitialisationCountNoConstant);
			\u0018.\u0001.Add(MessageId.Err_LowerGreaterUpperBorder, \u0081.\u0002.Err_LowerGreaterUpperBorder);
			\u0018.\u0001.Add(MessageId.Err_POUCalledFromDifferentTasks, \u0081.\u0002.Err_POUCalledFromDifferentTasks);
			\u0018.\u0001.Add(MessageId.Err_OutputByteWrittenFromDifferentTasks, \u0081.\u0002.Err_OutputByteWrittenFromDifferentTasks);
			\u0018.\u0001.Add(MessageId.Err_VarAccessNotSupported, \u0081.\u0002.Err_VarAccessNotSupported);
			\u0018.\u0001.Add(MessageId.Err_VarConfigNotAllowed, \u0081.\u0002.Err_VarConfigNotAllowed);
			\u0018.\u0001.Add(MessageId.Err_VarGlobalNotAllowed, \u0081.\u0002.Err_VarGlobalNotAllowed);
			\u0018.\u0001.Add(MessageId.Err_StructureNotAllowed, \u0081.\u0002.Err_StructureNotAllowed);
			\u0018.\u0001.Add(MessageId.Err_UnionNotAllowed, \u0081.\u0002.Err_UnionNotAllowed);
			\u0018.\u0001.Add(MessageId.Err_StaticNotAllowed, \u0081.\u0002.Err_StaticNotAllowed);
			\u0018.\u0001.Add(MessageId.Err_DeclarationKeywordNotAllowed, \u0081.\u0002.Err_DeclarationKeywordNotAllowed);
			\u0018.\u0001.Add(MessageId.Err_VarTempNotAllowed, \u0081.\u0002.Err_VarTempNotAllowed);
			\u0018.\u0001.Add(MessageId.Err_RetainOrPersistentNotAllowed, \u0081.\u0002.Err_RetainOrPersistentNotAllowed);
			\u0018.\u0001.Add(MessageId.Err_WrongVarInInterfaceMethod, \u0081.\u0002.Err_WrongVarInInterfaceMethod);
			\u0018.\u0001.Add(MessageId.Err_NoInstanceObject, \u0081.\u0002.Err_NoInstanceObject);
			\u0018.\u0001.Add(MessageId.Err_InOutAccessOutside, \u0081.\u0002.Err_InOutAccessOutside);
			\u0018.\u0001.Add(MessageId.Err_StructInitOnlyInput, \u0081.\u0002.Err_StructInitOnlyInput);
			\u0018.\u0001.Add(MessageId.Err_LibraryConflict, \u0081.\u0002.Err_LibraryConflict);
			\u0018.\u0001.Add(MessageId.Inf_RelatedPosition, \u0081.\u0002.Inf_RelatedPosition);
			\u0018.\u0001.Add(MessageId.Err_ReturnTypeForNonFunction, \u0081.\u0002.Err_ReturnTypeForNonFunction);
			\u0018.\u0001.Add(MessageId.Err_InvalidBaseForGlobalScopeExpression, \u0081.\u0002.Err_InvalidBaseForGlobalScopeExpression);
			\u0018.\u0001.Add(MessageId.Err_NoOnlineChangePossible, \u0081.\u0002.Err_NoOnlineChangePossible);
			\u0018.\u0001.Add(MessageId.Err_NoCallInInstancePath, \u0081.\u0002.Err_NoCallInInstancePath);
			\u0018.\u0001.Add(MessageId.Err_NoCallInInterfaceComparison, \u0081.\u0002.Err_NoCallInInterfaceComparison);
			\u0018.\u0001.Add(MessageId.Wrn_ExternalReferenceIgnored, \u0081.\u0002.Wrn_ExternalReferenceIgnored);
			\u0018.\u0001.Add(MessageId.Err_DeviceNotInstalled, \u0081.\u0002.Err_DeviceNotInstalled);
			\u0018.\u0001.Add(MessageId.Err_SemicolonExpected, \u0081.\u0002.Err_SemicolonExpected);
			\u0018.\u0001.Add(MessageId.Err_SemicolonExpectedInsteadOfEnd, \u0081.\u0002.Err_SemicolonExpectedInsteadOfEnd);
			\u0018.\u0001.Add(MessageId.Err_IndexOfNotSupported, \u0081.\u0002.Err_IndexOfNotSupported);
			\u0018.\u0001.Add(MessageId.Err_AccessOutsideOfCall, \u0081.\u0002.Err_AccessOutsideOfCall);
			\u0018.\u0001.Add(MessageId.Err_InvalidSignatureName, \u0081.\u0002.Err_InvalidSignatureName);
			\u0018.\u0001.Add(MessageId.Err_CaseLabelOutsideOfCase, \u0081.\u0002.Err_CaseLabelOutsideOfCase);
			\u0018.\u0001.Add(MessageId.Wrn_ImplicitSignedToUnsigned, \u0081.\u0002.Wrn_ImplicitSignedToUnsigned);
			\u0018.\u0001.Add(MessageId.Wrn_ImplicitUnsignedToSigned, \u0081.\u0002.Wrn_ImplicitUnsignedToSigned);
			\u0018.\u0001.Add(MessageId.Wrn_ImplicitIntToReal, \u0081.\u0002.Wrn_ImplicitIntToReal);
			\u0018.\u0001.Add(MessageId.Wrn_StringConstantTooLong, \u0081.\u0002.Wrn_StringConstantTooLong);
			\u0018.\u0001.Add(MessageId.Err_InterfaceNeedsInstance, \u0081.\u0002.Err_InterfaceNeedsInstance);
		}

		// Token: 0x06003569 RID: 13673 RVA: 0x000D5574 File Offset: 0x000D3774
		private static void \u0006()
		{
			\u0018.\u0001.Add(MessageId.Err_ConstantOverflow, \u0081.\u0002.Err_ConstantOverflow);
			\u0018.\u0001.Add(MessageId.Err_Operator1of2Expected, \u0081.\u0002.Err_Operator1of2Expected);
			\u0018.\u0001.Add(MessageId.Err_BitNrOverflow, \u0081.\u0002.Err_BitNrOverflow);
			\u0018.\u0001.Add(MessageId.Err_NoComponentOf, \u0081.\u0002.Err_NoComponentOf);
			\u0018.\u0001.Add(MessageId.Err_OverflowInAddress, \u0081.\u0002.Err_OverflowInAddress);
			\u0018.\u0001.Add(MessageId.Err_OperatorExpected, \u0081.\u0002.Err_OperatorExpected);
			\u0018.\u0001.Add(MessageId.Err_ExpressionExpectedInstead, \u0081.\u0002.Err_ExpressionExpectedInstead);
			\u0018.\u0001.Add(MessageId.Err_Operator1of3ExpectedInsteadofEOF, \u0081.\u0002.Err_Operator1of3ExpectedInsteadofEOF);
			\u0018.\u0001.Add(MessageId.Err_UnexpectedTokenFound, \u0081.\u0002.Err_UnexpectedTokenFound);
			\u0018.\u0001.Add(MessageId.Err_OperatorExpectedInsteadofEOF, \u0081.\u0002.Err_OperatorExpectedInsteadofEOF);
			\u0018.\u0001.Add(MessageId.Err_NoCaseLabelFound, \u0081.\u0002.Err_NoCaseLabelFound);
			\u0018.\u0001.Add(MessageId.Err_NoValidCondition, \u0081.\u0002.Err_NoValidCondition);
			\u0018.\u0001.Add(MessageId.Err_AtLeastOneExpected, \u0081.\u0002.Err_AtLeastOneExpected);
			\u0018.\u0001.Add(MessageId.Err_ConditionExpected, \u0081.\u0002.Err_ConditionExpected);
			\u0018.\u0001.Add(MessageId.Err_CounterStartExpected, \u0081.\u0002.Err_CounterStartExpected);
			\u0018.\u0001.Add(MessageId.Err_UpperBoundExpected, \u0081.\u0002.Err_UpperBoundExpected);
			\u0018.\u0001.Add(MessageId.Err_InvalidLoopIncrement, \u0081.\u0002.Err_InvalidLoopIncrement);
			\u0018.\u0001.Add(MessageId.Err_IsNoLValue, \u0081.\u0002.Err_IsNoLValue);
			\u0018.\u0001.Add(MessageId.Err_RValueRequired, \u0081.\u0002.Err_RValueRequired);
			\u0018.\u0001.Add(MessageId.Err_NoValidStatement, \u0081.\u0002.Err_NoValidStatement);
			\u0018.\u0001.Add(MessageId.Err_NoValidOperand, \u0081.\u0002.Err_NoValidOperand);
			\u0018.\u0001.Add(MessageId.Err_OpNeedsExactInputs, \u0081.\u0002.Err_OpNeedsExactInputs);
			\u0018.\u0001.Add(MessageId.Err_OpNeedsAtLeastInputs, \u0081.\u0002.Err_OpNeedsAtLeastInputs);
			\u0018.\u0001.Add(MessageId.Err_OpTakesAtMostInputs, \u0081.\u0002.Err_OpTakesAtMostInputs);
			\u0018.\u0001.Add(MessageId.Err_IllegalOperator, \u0081.\u0002.Err_IllegalOperator);
			\u0018.\u0001.Add(MessageId.Err_Operator1of4Expected, \u0081.\u0002.Err_Operator1of4Expected);
			\u0018.\u0001.Add(MessageId.Err_IdentifierExpected, \u0081.\u0002.Err_IdentifierExpected);
			\u0018.\u0001.Add(MessageId.Err_StringSizeExpected, \u0081.\u0002.Err_StringSizeExpected);
			\u0018.\u0001.Add(MessageId.Err_Operator1of3Expected, \u0081.\u0002.Err_Operator1of3Expected);
			\u0018.\u0001.Add(MessageId.Err_AddressExpected, \u0081.\u0002.Err_AddressExpected);
			\u0018.\u0001.Add(MessageId.Err_TypeExpected, \u0081.\u0002.Err_TypeExpected);
			\u0018.\u0001.Add(MessageId.Err_TypeMismatch, \u0081.\u0002.Err_TypeMismatch);
			\u0018.\u0001.Add(MessageId.Wrn_PointerMisatch, \u0081.\u0002.Wrn_PointerMisatch);
			\u0018.\u0001.Add(MessageId.Err_Multiassigninfor, \u0081.\u0002.Err_Multiassigninfor);
			\u0018.\u0001.Add(MessageId.Err_CalleeInvalidType, \u0081.\u0002.Err_CalleeInvalidType);
			\u0018.\u0001.Add(MessageId.Err_WrongObjectType, \u0081.\u0002.Err_WrongObjectType);
			\u0018.\u0001.Add(MessageId.Err_IsNoInput, \u0081.\u0002.Err_IsNoInput);
			\u0018.\u0001.Add(MessageId.Err_IsNoOutput, \u0081.\u0002.Err_IsNoOutput);
			\u0018.\u0001.Add(MessageId.Err_InOutNotAssigned, \u0081.\u0002.Err_InOutNotAssigned);
			\u0018.\u0001.Add(MessageId.Err_FunNeedsNInputs, \u0081.\u0002.Err_FunNeedsNInputs);
			\u0018.\u0001.Add(MessageId.Err_LValueForVarinout, \u0081.\u0002.Err_LValueForVarinout);
			\u0018.\u0001.Add(MessageId.Err_FunctionCallMixedStyle, \u0081.\u0002.Err_FunctionCallMixedStyle);
			\u0018.\u0001.Add(MessageId.Err_WrongFormalParameter, \u0081.\u0002.Err_WrongFormalParameter);
			\u0018.\u0001.Add(MessageId.Err_InputMissing, \u0081.\u0002.Err_InputMissing);
			\u0018.\u0001.Add(MessageId.Err_ThisNotAllowed, \u0081.\u0002.Err_ThisNotAllowed);
			\u0018.\u0001.Add(MessageId.Err_IdentNotDefined, \u0081.\u0002.Err_IdentNotDefined);
			\u0018.\u0001.Add(MessageId.Err_IndexingInvalid, \u0081.\u0002.Err_IndexingInvalid);
			\u0018.\u0001.Add(MessageId.Err_ArrayIndexNumWrong, \u0081.\u0002.Err_ArrayIndexNumWrong);
			\u0018.\u0001.Add(MessageId.Err_ConstantIndexOutOfRange, \u0081.\u0002.Err_ConstantIndexOutOfRange);
			\u0018.\u0001.Add(MessageId.Err_Bitaccessnoconst, \u0081.\u0002.Err_Bitaccessnoconst);
			\u0018.\u0001.Add(MessageId.Err_BitAccessOnlyOnInt, \u0081.\u0002.Err_BitAccessOnlyOnInt);
			\u0018.\u0001.Add(MessageId.Err_AttributeNameExpected, \u0081.\u0002.Err_AttributeNameExpected);
			\u0018.\u0001.Add(MessageId.Err_NoBitAccessOnFunctionCall, \u0081.\u0002.Err_NoBitAccessOnFunctionCall);
			\u0018.\u0001.Add(MessageId.Err_CompoAccessNoStruct, \u0081.\u0002.Err_CompoAccessNoStruct);
			\u0018.\u0001.Add(MessageId.Err_ContainsNoDefinition, \u0081.\u0002.Err_ContainsNoDefinition);
			\u0018.\u0001.Add(MessageId.Err_DerefNoPointer, \u0081.\u0002.Err_DerefNoPointer);
			\u0018.\u0001.Add(MessageId.Err_NoGlobalDefine, \u0081.\u0002.Err_NoGlobalDefine);
			\u0018.\u0001.Add(MessageId.Err_TypesNotComparable, \u0081.\u0002.Err_TypesNotComparable);
			\u0018.\u0001.Add(MessageId.Err_CompilerVersionError, \u0081.\u0002.Err_CompilerVersionError);
			\u0018.\u0001.Add(MessageId.Txt_ParentContextNotUpToDate, \u0081.\u0002.Txt_ParentContextNotUpToDate);
			\u0018.\u0001.Add(MessageId.Err_CompareNotPossible1, \u0081.\u0002.Err_CompareNotPossible1);
			\u0018.\u0001.Add(MessageId.Err_CompareNotPossible2, \u0081.\u0002.Err_CompareNotPossible2);
			\u0018.\u0001.Add(MessageId.Err_IniNeedsUserdefType, \u0081.\u0002.Err_IniNeedsUserdefType);
			\u0018.\u0001.Add(MessageId.Err_CannotAddMultipleTime, \u0081.\u0002.Err_CannotAddMultipleTime);
			\u0018.\u0001.Add(MessageId.Err_OperationNotPossibleOnType, \u0081.\u0002.Err_OperationNotPossibleOnType);
			\u0018.\u0001.Add(MessageId.Err_CannotMultiplyMultipleTime, \u0081.\u0002.Err_CannotMultiplyMultipleTime);
			\u0018.\u0001.Add(MessageId.Err_UnexpectedArrayInitialisation, \u0081.\u0002.Err_UnexpectedArrayInitialisation);
			\u0018.\u0001.Add(MessageId.Err_TooManyInitializer, \u0081.\u0002.Err_TooManyInitializer);
			\u0018.\u0001.Add(MessageId.Err_UnexpectedStructureInitialisation, \u0081.\u0002.Err_UnexpectedStructureInitialisation);
			\u0018.\u0001.Add(MessageId.Err_UnknownType, \u0081.\u0002.Err_UnknownType);
			\u0018.\u0001.Add(MessageId.Err_UnsupportedType, \u0081.\u0002.Err_UnsupportedType);
			\u0018.\u0001.Add(MessageId.Err_FunctionBlockNeedsInstance, \u0081.\u0002.Err_FunctionBlockNeedsInstance);
			\u0018.\u0001.Add(MessageId.Err_UnexpectedPragmaif, \u0081.\u0002.Err_UnexpectedPragmaif);
			\u0018.\u0001.Add(MessageId.Err_NoValidConditionforPragma, \u0081.\u0002.Err_NoValidConditionforPragma);
			\u0018.\u0001.Add(MessageId.Err_UnexpectedOperandForIndexOf, \u0081.\u0002.Err_UnexpectedOperandForIndexOf);
			\u0018.\u0001.Add(MessageId.Err_NoValidOperandforPragma, \u0081.\u0002.Err_NoValidOperandforPragma);
			\u0018.\u0001.Add(MessageId.Err_DefineValueExpected, \u0081.\u0002.Err_DefineValueExpected);
			\u0018.\u0001.Add(MessageId.Err_InterfaceNotFound, \u0081.\u0002.Err_InterfaceNotFound);
			\u0018.\u0001.Add(MessageId.Err_NoMethodImplementation, \u0081.\u0002.Err_NoMethodImplementation);
			\u0018.\u0001.Add(MessageId.Err_SomethingOverridingMethod, \u0081.\u0002.Err_SomethingOverridingMethod);
			\u0018.\u0001.Add(MessageId.Err_InterfaceChanged, \u0081.\u0002.Err_InterfaceChanged);
			\u0018.\u0001.Add(MessageId.Err_BaseClassNotFound, \u0081.\u0002.Err_BaseClassNotFound);
			\u0018.\u0001.Add(MessageId.Err_SelfInheritanceBase, \u0081.\u0002.Err_SelfInheritanceBase);
			\u0018.\u0001.Add(MessageId.Err_OnlyMethodsOverride, \u0081.\u0002.Err_OnlyMethodsOverride);
			\u0018.\u0001.Add(MessageId.Err_SomethingOverridingMethodBase, \u0081.\u0002.Err_SomethingOverridingMethodBase);
			\u0018.\u0001.Add(MessageId.Err_InterfaceChangedBase, \u0081.\u0002.Err_InterfaceChangedBase);
			\u0018.\u0001.Add(MessageId.Err_SelfInheritance, \u0081.\u0002.Err_SelfInheritance);
			\u0018.\u0001.Add(MessageId.Err_NoMultipleInheritance, \u0081.\u0002.Err_NoMultipleInheritance);
			\u0018.\u0001.Add(MessageId.Err_VariableOverride, \u0081.\u0002.Err_VariableOverride);
			\u0018.\u0001.Add(MessageId.Err_FunctionBlockNoLongerValid, \u0081.\u0002.Err_FunctionBlockNoLongerValid);
			\u0018.\u0001.Add(MessageId.Err_NoLocalEnum, \u0081.\u0002.Err_NoLocalEnum);
			\u0018.\u0001.Add(MessageId.Wrn_LibraryNotInstalled, \u0081.\u0002.Wrn_LibraryNotInstalled);
			\u0018.\u0001.Add(MessageId.Err_NoBranchOutOfFinally, \u0081.\u0002.Err_NoBranchOutOfFinally);
			\u0018.\u0001.Add(MessageId.Err_NoExitOrContinueOutOfTry, \u0081.\u0002.Err_NoExitOrContinueOutOfTry);
			\u0018.\u0001.Add(MessageId.Err_NoExitOrContinueOutOfCatch, \u0081.\u0002.Err_NoExitOrContinueOutOfCatch);
			\u0018.\u0001.Add(MessageId.Err_MultipleAssignmentWithProperty, \u0081.\u0002.Err_MultipleAssignmentWithProperty);
			\u0018.\u0001.Add(MessageId.Err_FunNeedsAtLeastNInputs, \u0081.\u0002.Err_FunNeedsAtLeastNInputs);
			\u0018.\u0001.Add(MessageId.Wrn_ObsoleteOutputInAbstractMethod, \u0081.\u0002.Wrn_ObsoleteOutputInAbstractMethod);
			\u0018.\u0001.Add(MessageId.Wrn_CallRecursion, \u0081.\u0002.Wrn_CallRecursion);
		}

		// Token: 0x0600356A RID: 13674 RVA: 0x000D5C28 File Offset: 0x000D3E28
		private static void \u0007()
		{
			\u0018.\u0001.Add(MessageId.Err_GenericOnWrongPosition, \u0081.\u0002.Err_GenericOnWrongPosition);
			\u0018.\u0001.Add(MessageId.Err_GenericOnlyConst, \u0081.\u0002.Err_GenericOnlyConst);
			\u0018.\u0001.Add(MessageId.Err_TypeIsNotGeneric, \u0081.\u0002.Err_TypeIsNotGeneric);
			\u0018.\u0001.Add(MessageId.Err_GenericWrongNumberOfInitializer, \u0081.\u0002.Err_GenericWrongNumberOfInitializer);
			\u0018.\u0001.Add(MessageId.Err_GenericNotConstant, \u0081.\u0002.Err_GenericNotConstant);
			\u0018.\u0001.Add(MessageId.Err_GenericNoInteger, \u0081.\u0002.Err_GenericNoInteger);
			\u0018.\u0001.Add(MessageId.Err_NoOutsideAccessToGenericVariable, \u0081.\u0002.Err_NoOutsideAccessToGenericVariable);
			\u0018.\u0001.Add(MessageId.Err_GenericDeclarationProducesErrorInGeneratedCode, \u0081.\u0002.Err_GenericDeclarationProducesErrorInGeneratedCode);
			\u0018.\u0001.Add(MessageId.Err_NoGenericInstanceInVarConst, \u0081.\u0002.Err_NoGenericInstanceInVarConst);
			\u0018.\u0001.Add(MessageId.Err_GenericParamsAllExplicitOrNone, \u0081.\u0002.Err_GenericParamsAllExplicitOrNone);
			\u0018.\u0001.Add(MessageId.Err_GenericParamMissing, \u0081.\u0002.Err_GenericParamMissing);
			\u0018.\u0001.Add(MessageId.Err_GenericParamUnknown, \u0081.\u0002.Err_GenericParamUnknown);
		}

		// Token: 0x0600356B RID: 13675 RVA: 0x000D5D28 File Offset: 0x000D3F28
		internal static string \u0001(MessageId \u0002, params object[] \u0003)
		{
			return string.Format(\u0018.\u0001(\u0002), \u0003);
		}

		// Token: 0x0600356C RID: 13676 RVA: 0x000D5D38 File Offset: 0x000D3F38
		internal static string \u0001(MessageId \u0002)
		{
			if (!\u0018.\u0001.ContainsKey(\u0002))
			{
				return "";
			}
			return \u0018.\u0001[\u0002];
		}

		// Token: 0x04000A69 RID: 2665
		private static LDictionary<MessageId, string> \u0001 = new LDictionary<MessageId, string>();
	}
}
