using System;
using System.IO;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Device;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ILanguageModelBuilder : ILanguageModelBuilder12, ILanguageModelBuilder11, ILanguageModelBuilder10, ILanguageModelBuilder9, ILanguageModelBuilder8, ILanguageModelBuilder7, ILanguageModelBuilder6, ILanguageModelBuilder5, ILanguageModelBuilder4, ILanguageModelBuilder3, ILanguageModelBuilder2, ILanguageModelBuilder
	{
		_IVariable CreateVariable(ISourcePosition sp);

		IDataLocation2 CreateDataLocation(ushort usArea, int iOffset);

		IDataLocation2 CreateBitDataLocation(ushort usArea, int iOffset, byte byBitLocation);

		IDataLocation2 CreateRelativeDataLocation(int iOffset);

		IDataLocation2 CreateRelativeDataLocation(int iOffset, DataLocationFlag dlf);

		IDataLocation2 CreateRelativeBitDataLocation(int iOffset, byte byBitLocation);

		_IArea CreateArea(DataSegmentFlags dsf, AreaFlags af);

		IPersistentArea CreateAreaPersistent(IArea area, uint uiChecksum);

		_IMemoryManager CreateMemMan(int nSize, int nBaseAddress);

		_IStepInPosition CreateStepInPosition(int nSignId, IBreakpoint stepoutBreakpoint);

		ILateCompileContext CreateLateCompileContext(_ICompileContext comconNew, _ICompileContext comconRef);

		ILanguageModelList CreateLanguageModelList();

		ICodePosition CreateCodePosition(ISourcePosition sp, AccessFlag access);

		ILibPlaceholderIdentification CreateLibPlaceholderIdentification(IDeviceIdentification devid, ILibraryPlaceholder2 libplc);

		_ICompileContext CreateCompileContext(KindOfContext kindof, _ICompileContext comconOld, _ICompileContext comconParent, Guid objectGuid);

		_ISignature CreateSignature();

		_ICompiledPOU CreateCompiledPOU(string stName);

		_IVariableExpression CreateVariableExpression(_IVariable var, _ISignature sign);

		_ILiteralExpression CreateDefaultLiteralExpression(TypeClass tc);

		_IForStatement CreateForStatement();

		_IForStatement CreateForStatement(IToken token);

		_ILiteralValue CreateLiteralValue(double d);

		_ILiteralValue CreateLiteralValue(long l);

		_ILiteralValue CreateLiteralValue(ulong ul);

		_ILiteralValue CreateLiteralValue(string s);

		_ILiteralValue CreateLiteralValue(bool b);

		_ISafeBoolType CreateSafeBoolType();

		_ISafeByteType CreateSafeByteType();

		_ISafeSIntType CreateSafeSIntType();

		_ISafeUSIntType CreateSafeUSIntType();

		_ISafeWordType CreateSafeWordType();

		_ISafeIntType CreateSafeIntType();

		_ISafeUIntType CreateSafeUIntType();

		_ISafeDWordType CreateSafeDWordType();

		_ISafeDIntType CreateSafeDIntType();

		_ISafeUDIntType CreateSafeUDIntType();

		_ISafeLWordType CreateSafeLWordType();

		_ISafeLIntType CreateSafeLIntType();

		_ISafeULIntType CreateSafeULIntType();

		_ISafeTimeType CreateSafeTimeType();

		_IRetainBoolType CreateRetainBoolType();

		_IBool16Type CreateBool16Type();

		_IRetainByteType CreateRetainByteType();

		_IRetainSIntType CreateRetainSIntType();

		_IRetainUSIntType CreateRetainUSIntType();

		_IXDWordType CreateXDWordType();

		_IXLWordType CreateXLWordType();

		_IXDIntType CreateXDIntType();

		_IBoolType CreateBoolType();

		_IDirectAddressBitType CreateDirectAddressBitType();

		_IByteType CreateByteType();

		_ISIntType CreateSIntType();

		_IUSIntType CreateUSIntType();

		_IIntType CreateIntType();

		_IUIntType CreateUIntType();

		_IWordType CreateWordType();

		_IDIntType CreateDIntType();

		_IUDIntType CreateUDIntType();

		_IDWordType CreateDWordType();

		_ILIntType CreateLIntType();

		_IULIntType CreateULIntType();

		_ILWordType CreateLWordType();

		_IRealType CreateRealType();

		_ILRealType CreateLRealType();

		_ILazyType CreateLazyType();

		_IBitConstType CreateBitConstType();

		_IBitType CreateBitType();

		_IUXIntType CreateUXIntType();

		_IXIntType CreateXIntType();

		_IXWordType CreateXWordType();

		_IXUDIntType CreateXUDIntType();

		_IXULIntType CreateXULIntType();

		_IXLIntType CreateXLIntType();

		_IDateType CreateDateType();

		_ITimeOfDayType CreateTimeOfDayType();

		_IDateAndTimeType CreateDateAndTimeType();

		_ITimeType CreateTimeType();

		_ILTimeType CreateLTimeType();

		_IAnyType CreateAnyType();

		_IAnyRealType CreateAnyRealType();

		_IAnyStringType CreateAnyStringType();

		_IAnyIntType CreateAnyIntType();

		_IAnyNumType CreateAnyNumType();

		_IAnyBitType CreateAnyBitType();

		_IAnyDateType CreateAnyDateType();

		_IAnyBitButBoolIsPreferred CreateAnyBitButBoolIsPreferredType();

		_IVariableLengthArrayType CreateVariableLengthArrayType();

		_IArrayType CreateArrayType();

		_IArrayType CreateArrayType(_IType typeBase);

		_IVectorType CreateVectorType(_IType typeBase, _IExpression expDim);

		_IRangeAwareAnyIntType CreateRangeAwareAnyIntType();

		_IUserdefType CreateUserdefType();

		_IUserdefType CreateUserdefType(_IExpression expname);

		_IUserdefType CreateUserdefType(string stname);

		_IPointerType CreatePointerType();

		_IPointerType CreatePointerType(_IType typeBase);

		_IReferenceType CreateReferenceType();

		_IReferenceType CreateReferenceType(_IType typeBase);

		_IImplicitReferenceType CreateImplicitReferenceType(_IType typeBase);

		_IInOutReferenceType CreateInOutReferenceType(_IType typeBase);

		_IStringType CreateStringType();

		_IWStringType CreateWStringType();

		_IAliasType CreateAliasType(_IType orgType);

		_IEnumType CreateEnumType(string stName, int idSignature);

		_IEnumType CreateEnumType(string stName);

		_IParamsType CreateParamsType(_IType typeBase, _IExpression count);

		IImplicitEnumerationType CreateImplicitEnumerationType(_IEnumDeclarationListStatement enumdecls, string stImplicitName);

		_ICompilerMessage CreateCompilerMessage();

		_ICompilerMessage CreateCompilerMessage(ISourcePosition position, string stError, Severity severity, MessageId Number);

		_ICompilerMessage CreateCompilerMessage(IMinimalPosition position, string stError, Severity severity, short sLength, MessageId Number);

		_ISourcePosition CreateFixedSourcePosition();

		IMinimalPosition CreateMinimalPosition(_ISourcePosition sp);

		IMinimalPosition CreateMinimalPosition(long nPosition, short sPositionOffset);

		IDirectVariable CreateIncompleteDirectVariable(DirectVariableLocation dirvarlocation);

		_IErrorStatement CreateErrorStatement();

		_IEmptyStatement CreateEmptyStatement();

		_IEmptyStatement CreateEmptyStatement(IToken token);

		_IVarInitialEmptyStatement CreateVarInitialEmptyStatement(_IVariable varInitial, _ISignature signInitial);

		_IWhileStatement CreateWhileStatement();

		_IWhileStatement CreateWhileStatement(_IExpression expCond, _IStatement stateControlled);

		_IWhileStatement CreateWhileStatement(_IExpression expCond, _IStatement stateControlled, IToken token);

		_IRepeatStatement CreateRepeatStatement();

		_IRepeatStatement CreateRepeatStatement(_IExpression expCond, _IStatement stateControlled);

		_IRepeatStatement CreateRepeatStatement(_IExpression expCond, _IStatement stateControlled, IToken token);

		_ICaseRangeExpression CreateCaseRangeExpression();

		_ICaseRangeExpression CreateCaseRangeExpression(_IExpression expLow, _IExpression expHigh, IToken token);

		_ICaseLabelStatement CreateCaseLabelStatement();

		_ICaseLabelStatement CreateCaseLabelStatement(_IExpression exp);

		_ICaseLabelStatement CreateCaseLabelStatement(_IExpression exp, IToken token);

		_ICaseLabelStatement CreateCaseLabelStatement(IToken token);

		_ICase CreateCase();

		_ICase CreateCase(_ICaseLabelStatement caselabel, _IStatement statement);

		_ICaseStatement CreateCaseStatement();

		_ICaseStatement CreateCaseStatement(_IExpression expSwitch);

		_ICaseStatement CreateCaseStatement(_IExpression expSwitch, IToken token);

		_IExitStatement CreateExitStatement();

		_IContinueStatement CreateContinueStatement();

		_ISequenceStatement CreateSequenceStatement();

		_ISequenceStatement CreateSequenceStatement(int nCount);

		_ISequenceStatement CreateSequenceStatement(IToken token);

		_ISubRoutineStatement CreateSubRoutineStatement();

		_IAssignmentExpression CreateAssignmentExpression();

		_IAssignmentExpression CreateAssignmentExpression(_IExpression expLValue);

		_IAssignmentExpression CreateAssignmentExpression(_IExpression expLValue, IToken token);

		_IElseIf CreateElseIf(_IExpression expCondition, _IStatement stControlled);

		_IElseIf CreateElseIf();

		_IIfStatement CreateIfStatement();

		_IIfStatement CreateIfStatement(_IExpression expCond);

		_IIfStatement CreateIfStatement(_IExpression expCond, IToken token);

		_ITryCatchStatement CreateTryCatchStatement();

		_ITryCatchStatement CreateTryCatchStatement(IToken token);

		_IReturnStatement CreateReturnStatement();

		_IReturnStatement CreateReturnStatement(IToken token);

		_IJumpStatement CreateJumpStatement();

		_IJumpStatement CreateJumpStatement(string stLabel);

		_IJumpStatement CreateJumpStatement(string stLabel, IToken token);

		_ILabelStatement CreateLabelStatement();

		_ILabelStatement CreateLabelStatement(string stLabel);

		_ILabelStatement CreateLabelStatement(string stLabel, IToken token);

		_ICommentStatement CreateCommentStatement();

		_ICommentStatement CreateCommentStatement(string stText);

		_ICommentStatement CreateCommentStatement(string stText, IToken token);

		_IPragmaStatement CreatePragmaStatement();

		_IPragmaStatement CreatePragmaStatement(IToken token);

		_IMessageGuidPragmaStatement CreateMessageGuidPragmaStatement(IToken token, Guid guid);

		_IWarningDisableRestorePragmaStatement CreateWarningDisableRestorePragmaStatement();

		_IWarningDisableRestorePragmaStatement CreateWarningDisableRestorePragmaStatement(IToken token, bool bRestore, string stId);

		_IExpressionStatement CreateExpressionStatement();

		_IExpressionStatement CreateExpressionStatement(_IExpression exp);

		_IExpressionStatement CreateExpressionStatement(_IExpression exp, IToken token);

		_IPOUDeclarationStatement CreatePOUDeclarationStatement();

		_IPOUDeclarationStatement CreatePOUDeclarationStatement(IToken token);

		_IVariableDeclarationListStatement CreateVariableDeclarationListStatement();

		_IVariableDeclarationListStatement CreateVariableDeclarationListStatement(IToken token);

		_IVariableDeclarationStatement CreateVariableDeclarationStatement();

		_IVariableDeclarationStatement CreateVariableDeclarationStatement(IToken token);

		_IErrorExpression CreateErrorExpression();

		_IErrorExpression CreateErrorExpression(IToken token);

		_IProgramCounterExpression CreateProgramCounterExpression();

		_IFramePointerExpression CreateFramePointerExpression();

		_ICallInstanceExpression CreateCallInstanceExpression(bool bWriteAccess, ICompiledType ctype, IIntermediateValueLocation ivl);

		_ICallExpression CreateCallExpression();

		_ICallExpression CreateCallExpression(_IExpression expCallee);

		_ICallExpression CreateCallExpression(_IExpression expCallee, IToken token);

		_IOperatorExpression CreateOperatorExpression();

		_IOperatorExpression CreateOperatorExpression(Operator op);

		_IOperatorExpression CreateOperatorExpression(Operator op, IToken token);

		_IConversionExpression CreateConversionExpression();

		_IConversionExpression CreateConversionExpression(TypeClass tcFrom, TypeClass tcTo);

		_IConversionExpression CreateConversionExpression(TypeClass tcFrom, TypeClass tcTo, IToken token);

		_IImplicitConversionExpression CreateImplicitConversionExpression();

		_IImplicitConversionExpression CreateImplicitConversionExpression(TypeClass tcFrom, TypeClass tcTo);

		_IImplicitConversionExpression CreateImplicitConversionExpression(TypeClass tcFrom, TypeClass tcTo, IToken token);

		_INewExpression CreateNewExpression();

		_INewExpression CreateNewExpression(_IType typeIn, _IExpression expCount);

		_INewExpression CreateNewExpression(_IType typeIn, _IExpression expCount, IToken token);

		_ICastExpression CreateCastExpression();

		_ICastExpression CreateCastExpression(_IExpression expWithType, _IExpression expBase);

		_IThisExpression CreateThisExpression();

		_IThisExpression CreateThisExpression(IToken token);

		_IBaseExpression CreateBaseExpression();

		_IBaseExpression CreateBaseExpression(IToken token);

		_ITypeExpression CreateTypeExpression();

		_ITypeExpression CreateTypeExpression(ICompiledType cType);

		_IAddressExpression CreateAddressExpression();

		_IAddressExpression CreateAddressExpression(IDirectVariable dirvar);

		_IAddressExpression CreateAddressExpression(IDirectVariable dirvar, IToken token);

		_IVariableExpression CreateVariableExpression(string stName);

		_IVariableExpression CreateVariableExpression(string stName, IToken token);

		_ICompoAccessExpression CreateCompoAccessExpression();

		_ICompoAccessExpression CreateCompoAccessExpression(_IExpression expLeft);

		_ICompoAccessExpression CreateCompoAccessExpression(_IExpression expLeft, IToken token);

		_IDeRefAccessExpression CreateDeRefAccessExpression();

		_IDeRefAccessExpression CreateDeRefAccessExpression(_IExpression exp);

		_IDeRefAccessExpression CreateDeRefAccessExpression(_IExpression exp, IToken token);

		_IDeRefAccessExpression CreateImplicitDeRefAccessExpression(_IExpression etoken);

		_IIndexAccessExpression CreateIndexAccessExpression();

		_IIndexAccessExpression CreateIndexAccessExpression(_IExpression expBase);

		_IIndexAccessExpression CreateIndexAccessExpression(_IExpression expBase, IToken token);

		_ICopyScopeExpression CreateCopyScopeExpression();

		_ICopyScopeExpression CreateCopyScopeExpression(_IExpression expBase);

		_ICopyScopeExpression CreateCopyScopeExpression(_IExpression expBase, IToken token);

		_IGlobalScopeExpression CreateGlobalScopeExpression();

		_IGlobalScopeExpression CreateGlobalScopeExpression(_IExpression expBase);

		_IGlobalScopeExpression CreateGlobalScopeExpression(_IExpression expBase, IToken token);

		_ISystemScopeExpression CreateSystemScopeExpression();

		_ISystemScopeExpression CreateSystemScopeExpression(string stBaseName);

		_ISystemScopeExpression CreateSystemScopeExpression(_IExpression expBase);

		_ISystemScopeExpression CreateSystemScopeExpression(_IExpression expBase, IToken token);

		_IPoolScopeExpression CreatePoolScopeExpression(_IExpression expBase);

		_IPoolScopeExpression CreatePoolScopeExpression(_IExpression expBase, IToken token);

		_ICurrentTaskExpression CreateCurrentTaskExpression(_IExpression expBase, IToken token);

		_INamespaceAccessExpression CreateNamespaceAccessExpression(_IExpression expNamespace, _IExpression expAccess);

		_INamespaceAccessExpression CreateNamespaceAccessExpression(_IExpression expNamespace, _IExpression expAccess, IToken token);

		_INullExpression CreateNullExpression();

		_INullExpression CreateNullExpression(IToken token);

		_INullStatement CreateNullStatement();

		_INullStatement CreateNullStatement(IToken token);

		_IMultipleIndexInitialization CreateMultipleIndexInitialisation();

		_IMultipleIndexInitialization CreateMultipleIndexInitialisation(IToken token);

		_IArrayInitialization CreateArrayInitialisation();

		_IArrayInitialization CreateArrayInitialisation(IToken token);

		_IStructureInitialization CreateStructureInitialisation();

		_IStructureInitialization CreateStructureInitialisation(IToken token);

		_IDefineReference CreateDefineReference();

		_IDefineReference CreateDefineReference(IToken token);

		_IVariableReference CreateVariableReference();

		_IVariableReference CreateVariableReference(IToken token);

		_ITypeReference CreateTypeReference();

		_ITypeReference CreateTypeReference(IToken token);

		_ITypeReference CreateTypeReference(IToken token, _IExpression expPath);

		_IPouReference CreatePouReference();

		_IPouReference CreatePouReference(IToken token);

		_IPouReference CreatePouReference(IToken token, _IExpression expPath);

		ITaskInfo2 CreateTaskInfo(Guid guidTaskInfo, string stName, string stParentTaskName);

		_ITaskReference CreateTaskReference();

		_ITaskReference CreateTaskReference(IToken token);

		_ITaskReference CreateTaskReference(IToken token, string stTaskName);

		_IResourceReference CreateResourceReference();

		_IResourceReference CreateResourceReference(IToken token);

		_IResourceReference CreateResourceReference(IToken token, string stResourceName);

		_IXRefExpression CreateXRefExpression();

		_IXRefExpression CreateXRefExpression(IToken token);

		_IXRefExpression CreateXRefExpression(IToken token, _IItemReference itref, _IItemReference itrefFrom);

		_IDefinedExpression CreateDefinedExpression();

		_IDefinedExpression CreateDefinedExpression(IToken token);

		_IDefinedExpression CreateDefinedExpression(IToken token, _IItemReference itref);

		_ICompilerVersionExpression CreateCompilerVersionExpression();

		_ICompilerVersionExpression CreateCompilerVersionExpression(IToken token);

		_ICompilerVersionExpression CreateCompilerVersionExpression(IToken token, Version versionToTest, Operator opComparison);

		_IRuntimeVersionExpression CreateRuntimeVersionExpression();

		_IRuntimeVersionExpression CreateRuntimeVersionExpression(IToken token);

		_IRuntimeVersionExpression CreateRuntimeVersionExpression(IToken token, Version versionToTest, Operator opComparison);

		_IHasTypeExpression CreateHasTypeExpression();

		_IHasTypeExpression CreateHasTypeExpression(IToken token);

		_IIsEnumTypeExpression CreateIsEnumTypeExpression();

		_IIsEnumTypeExpression CreateIsEnumTypeExpression(IToken token);

		_IHasAttributeExpression CreateHasAttributeExpression();

		_IHasAttributeExpression CreateHasAttributeExpression(IToken token);

		_IHasAttributeExpression CreateHasAttributeExpression(IToken token, _IItemReference itref, string stAttribute);

		_IHasValueExpression CreateHasValueExpression();

		_IHasValueExpression CreateHasValueExpression(IToken token);

		_IHasValueExpression CreateHasValueExpression(IToken token, string stDefine, string stValue);

		_IHasConstantValueExpression CreateHasConstantValueExpression();

		_IHasConstantValueExpression CreateHasConstantValueExpression(IToken token);

		_IHasConstantValueExpression CreateHasConstantValueExpression(IToken token, _IExpression constant, _IExpression value, Operator comparison);

		_IPragmaOperatorExpression CreatePragmaOperatorExpression();

		_IPragmaOperatorExpression CreatePragmaOperatorExpression(PragmaOperator op);

		_IPragmaOperatorExpression CreatePragmaOperatorExpression(PragmaOperator op, IToken token);

		_IPragmaAssertion CreatePragmaAssertion();

		_IPragmaAssertion CreatePragmaAssertion(_IExpression expCondition, string stErrorOutput);

		_IPragmaAssertion CreatePragmaAssertion(_IExpression expCondition, string stErrorOutput, IToken token);

		_IPragmaElseIf CreatePragmaElseIf();

		_IPragmaElseIf CreatePragmaElseIf(_IPragmaExpression expCondition, _IStatement stControlled);

		_IPragmaIfStatement CreatePragmaIfStatement();

		_IPragmaIfStatement CreatePragmaIfStatement(_IExpression expCond);

		_IPragmaIfStatement CreatePragmaIfStatement(_IExpression expCond, IToken token);

		_IBreakPointStatement CreateBreakPointStatement();

		_IBreakPointStatement CreateBreakPointStatement(IToken token, long lPosition, long lSuccessorPosition);

		_IDefineStatement CreateDefineStatement();

		_IDefineStatement CreateDefineStatement(IToken token, bool bDefine, string stIdent, string stValue);

		_IDefineStatement CreateDefineStatement(IToken token, bool bDefine, string stIdent);

		_IBitAccess CreateBitAccess();

		_IBitAccess CreateBitAccess(_IExpression expBase, byte byBitNr);

		IBitWriteAccess CreateBitWriteAccess(int nSignatureId, int nArea, int nOffset, byte byBitNr, IMinimalPosition position, string stSymbol);

		_ILiteralExpression CreateLiteralExpression(long lVal, TypeClass tc, IToken token);

		_ILiteralExpression CreateLiteralExpression(long lVal, TypeClass tc, IToken token, int nBase);

		_ILiteralExpression CreateLiteralExpression(long lVal, TypeClass tc, IToken token, int nBase, bool negative);

		_ILiteralExpression CreateLiteralExpression(ulong ulVal, TypeClass tc, IToken token);

		_ILiteralExpression CreateLiteralExpression(string stVal, TypeClass tc, IToken token);

		_ILiteralExpression CreateLiteralExpression(double dVal, TypeClass tc, IToken token);

		_ITypeExpression CreateTypeExpression(ICompiledType ctype, IToken token);

		_IEnumDeclarationListStatement CreateEnumDeclarationListStatement(IToken token);

		_ITypeDeclarationStatement CreateTypeDeclarationStatement(IToken token);

		_IErrorStatement CreateErrorStatement(IToken token);

		_IExitStatement CreateExitStatement(IToken token);

		_IContinueStatement CreateContinueStatement(IToken token);

		_ISubrangeType CreateSubrangeType(_IExpression expLower, _IExpression expUpper);

		_IXStringType CreateXStringtype();

		_IDefineReference CreateDefineReference(IToken token, string stDefine);

		_IHasCompatibleTypeExpression CreateHasCompatibleTypeExpression();

		_IAddressCodePosition CreateAddressCodePosition(ISourcePosition sp, AccessFlag access, int nTypeSize);

		_IPreCompileContext CreatePrecompileContext(string stLibraryPath, Guid applicationGuid, KindOfContext kindof);

		IIntermediateValueLocation CreateIntermediateValueLocation();

		ICompiledCode CreateCompiledCodeDataStub(ICompiledCode compiledcode);

		_ICompiledCodeData CreateCompiledCodeData(Stream stream, int nRelatedId);

		ICompiledCode CreateCompiledCodeDataPlaceholder(int nSize, IDataLocation datloc);

		ICompiledCode CreateCompiledCodeDataReloc(Stream stream, bool bMotorola);

		ICompiledCode CreateCompiledCodeDataReloc(byte[] bytes, bool bMotorolaByteOrder);

		_IRelocationList CreateRelocationList();

		_ICompilerAttribute CreateCompilerAttribute(string stName, string stValue);

		_IDataSegment CreateDataSegment(ushort usArea, int nAddress, int nSize, DataSegmentFlags flags);

		_IDataManager CreateDataManager();

		ILanguageModelManagerTargetSettings CreateTargetSettings();
	}
}
