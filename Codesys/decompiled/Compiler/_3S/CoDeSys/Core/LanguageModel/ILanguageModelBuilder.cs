using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Device;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelBuilder
	{
		ILanguageModel CreateLanguageModelOfXml(string stContent, List<List<string>> stringlistTable);

		ILanguageModel CreateApplicationLanguageModel(Guid applicationGuid, Guid deviceGuid);

		ILanguageModel CreateLibraryLanguageModel(string stLibraryId);

		ILanguageModel CreateLanguageModel(Guid applicationGuid, Guid deviceGuid, Guid languageModelControlObjectGuid, string stLibraryId);

		IDeviceIdentification CreateDeviceIdentification(int nType, string stId, string stVersion);

		ILMDevice CreateLMForDevice(string stName, IDeviceIdentification devid);

		ILMApplication CreateLMForApplication(Guid parentApp, string stDeviceName, string stApplicationName, IDeviceIdentification devid);

		ITaskInfo CreateTaskInfo(Guid guidTask, string stTaskName);

		ILMTaskList CreateTaskList(Guid guidTaskConfig);

		ILibParameterTable CreateLibraryParameterTable();

		ILMLibraryInfo CreateLibInfo(string stId, string stDefaultNamespace, string stNamespace, bool bSystemLibrary, bool bPublishSymbols, bool bLinkAllContent, bool bLinkInSimulation, bool bQualifiedOnly);

		ILMPlaceholderInfo CreateLibraryPlaceholder(string stName, string stDefaultLibraryId, string stNamespace, bool bPublishSymbols, bool bLinkAllContent, bool bLinkInSimulation, Guid guidResolver);

		ILMLibraryList CreateLibraryList(Guid guidLibMan);

		ILMPOU CreatePou(string stName, Guid guidPOU);

		ILMGlobVarlist CreateGlobVarlist(string stName, Guid guidGVL);

		ILMDataType CreateDataType(string stName, Guid guidDUT);

		IExprementPosition CreateExprementPosition(long lPosition);

		IExprementPosition CreateExprementPosition(long lPosition, short sPositionOffset);

		IMessage CreateCompilerMessage(IExprementPosition position, IExprement exp, string stMessage, Severity severity, ShowAttribute showatt);

		IExprement DuplicateExprement(IExprement expIn);

		ISequenceStatement2 ParseSTSnippet(string stSnippet);

		IExpression ParseExpression(string stExpression);

		ICompiledType ParseType(string stType);

		IDirectVariable CreateDirectVariable(DirectVariableLocation location, DirectVariableSize size, int nOffset);

		IDirectVariable CreateDirectVariable(DirectVariableLocation location, DirectVariableSize size, int nOffset, int nBitOffset);

		IDirectVariable CreateDirectVariable(DirectVariableLocation location, DirectVariableSize size, int[] nComponents);

		ICompiledType CreateSimpleType(TypeClass type);

		ICompiledType CreateComplexType(string stType, out IMessage message);

		IEmptyStatement CreateEmptyStatement(IExprementPosition pos);

		IWhileStatement CreateWhileStatement(IExprementPosition pos, IExpression expCondition, ISequenceStatement2 seqControlled);

		IRepeatStatement CreateRepeatStatement(IExprementPosition pos, IExpression expCondition, ISequenceStatement2 seqControlled);

		ICaseRangeExpression CreateCaseRangeExpression(IExprementPosition pos, IExpression expLow, IExpression expHigh);

		ICaseLabelStatement CreateCaseLabelStatement(IExprementPosition pos, List<IExpression> cases);

		ICase CreateCase(ICaseLabelStatement caslabst, ISequenceStatement2 stateControlled);

		ICaseStatement CreateCaseStatement(IExprementPosition pos, IExpression expSwitch, List<ICase> cases, ISequenceStatement2 stateElse);

		IForStatement CreateForStatement(IExprementPosition pos, IAssignmentExpression assCounterStart, IExpression expUpper, IExpression expBy, IStatement stateControlled);

		IExitStatement CreateExitStatement(IExprementPosition pos);

		IContinueStatement CreateContinueStatement(IExprementPosition pos);

		ISequenceStatement2 CreateSequenceStatement(IExprementPosition pos);

		ISequenceStatement2 CreateSequenceStatement(IExprementPosition pos, List<IStatement> statements);

		IAssignmentExpression CreateAssignmentExpression(IExprementPosition pos, IExpression expLeft, IExpression expRight);

		IExpressionStatement CreateAssignmentStatement(IExprementPosition pos, IExpression expLeft, IExpression expRight);

		IAssignmentExpression CreateAssignmentExpression(IExprementPosition pos, IExpression expLValue, IExpression expRValue, Operator kindof);

		IIfStatement CreateIfStatement(IExprementPosition pos, IExpression expCondition, ISequenceStatement2 seqThen);

		IIfStatement CreateIfStatement(IExprementPosition pos, IExpression expCondition, ISequenceStatement2 seqThen, ISequenceStatement2 seqElse);

		IReturnStatement CreateReturnStatement(IExprementPosition pos, IExpression expCondition);

		IJumpStatement CreateJumpStatement(IExprementPosition pos, IExpression expCondition, string stLabel);

		ILabelStatement CreateLabelStatement(IExprementPosition pos, string stLabel);

		ICommentStatement CreateCommentStatement(IExprementPosition pos, string stComment);

		[Obsolete("Please do not this method any more. Use ILanguageModelBuilder3.CreatePragmaStatement2() instead.")]
		IPragmaStatement CreatePragmaStatement(IExprementPosition pos, string stPragma);

		IPragmaStatement CreateMessageGuidPragmaStatement(Guid guidMessage);

		IExpressionStatement CreateExpressionStatement(IExpression expInner);

		IPOUDeclarationStatement CreatePOUDeclarationStatement(IExprementPosition pos, Operator opClass, string stName, ICompiledType typeReturnValue, List<IVariableDeclarationListStatement> vardecls, List<IExpression> extends, List<IExpression> implements, SignatureFlag accessflags);

		IPOUDeclarationStatement CreatePOUDeclarationStatement(IExprementPosition pos, Operator opClass, string stName);

		IVariableDeclarationListStatement CreateVariableDeclarationListStatement(IExprementPosition pos, VarFlag varflag, ISequenceStatement seq);

		IVariableDeclarationStatement CreateSimpleVariableDeclarationStatement(IExprementPosition pos, string stName, ICompiledType type);

		IVariableDeclarationStatement CreateVariableDeclarationStatement(IExprementPosition pos, string stName, ICompiledType type, IExpression expInitial, IDirectVariable diraddr);

		IVariableDeclarationStatement CreateVariableDeclarationStatement(IExprementPosition pos, List<string> stNames, ICompiledType type, IExpression expInitial);

		IVariableDeclarationStatement CreateInstanceVariableDeclarationStatement(IExprementPosition pos, string stName, ICompiledType type, IExpression expInitial, IDirectVariable diraddr, List<IAssignmentExpression> expInitMethodAssignments);

		ITypeDeclarationStatement CreateAliasDeclaration(IExprementPosition pos, ICompiledType type, string stName, IExpression expInitial, SignatureFlag sfAccess);

		ITypeDeclarationStatement CreateStructDeclaration(IExprementPosition pos, string stName, IExpression expExtends, IExpression expInitial, ISequenceStatement seq, SignatureFlag sfAccess);

		ITypeDeclarationStatement CreateUnionDeclaration(IExprementPosition pos, string stName, IExpression expExtends, IExpression expInitial, ISequenceStatement seq, SignatureFlag sfAccess);

		ITypeDeclarationStatement CreateEnumTypeDeclaration(IExprementPosition pos, string stName, IExpression expInitial, IEnumDeclarationListStatement enumdecllist, SignatureFlag sfAccess);

		IEnumDeclarationListStatement CreateEnumDeclarationListStatement(IExprementPosition pos, ICompiledType basetype, List<IEnumDeclarationStatement> enums);

		IEnumDeclarationStatement CreateEnumDeclarationStatement(IExprementPosition pos, string stName, IExpression expValue);

		ICallExpression2 CreateCallExpression(IExprementPosition pos, IExpression expCallee, IExpression expCondition, ICompiledType typeExpected, List<IAssignmentExpression> inputassignments, List<IAssignmentExpression> outputassignments);

		IExpressionStatement CreateCallStatement(IExprementPosition pos, IExpression expCallee, IExpression expCondition, ICompiledType typeExpected, List<IAssignmentExpression> inputassignments, List<IAssignmentExpression> outputassignments);

		IOperatorExpression CreateOperatorExpression(IExprementPosition pos, Operator op, IExpression expOp1, IExpression expOp2);

		IOperatorExpression CreateOperatorExpression(IExprementPosition pos, Operator op, IExpression expSingleOp);

		IOperatorExpression CreateOperatorExpression(IExprementPosition pos, Operator op, List<IExpression> expOperands);

		IConversionExpression CreateConversionExpression(IExprementPosition pos, TypeClass tcFrom, TypeClass tcTo, IExpression exp);

		IThisExpression CreateThisExpression(IExprementPosition pos);

		IBaseExpression CreateSuperExpression(IExprementPosition pos);

		ILiteralExpression CreateLiteralExpression(IExprementPosition pos, long lVal, TypeClass tc);

		ILiteralExpression CreateLiteralExpression(IExprementPosition pos, long lVal, TypeClass tc, int nBase);

		ILiteralExpression CreateLiteralExpression(IExprementPosition pos, ulong ulVal, TypeClass tc);

		ILiteralExpression CreateLiteralExpression(IExprementPosition pos, string stVal, TypeClass tc);

		ILiteralExpression CreateLiteralExpression(IExprementPosition pos, double dVal, TypeClass tc);

		ILiteralExpression CreateLiteralExpression(IExprementPosition pos, long lVal);

		ILiteralExpression CreateLiteralExpression(IExprementPosition pos, ulong ulVal);

		ILiteralExpression CreateLiteralExpression(IExprementPosition pos, string stVal);

		ILiteralExpression CreateLiteralExpression(IExprementPosition pos, double dVal);

		ILiteralExpression CreateLiteralExpression(IExprementPosition pos, bool bVal);

		ITypeExpression CreateTypeExpression(IExprementPosition pos, ICompiledType ctype);

		INewExpression CreateNewExpression(IExprementPosition pos, ICompiledType ctypeIn, IExpression expCount);

		ICastExpression CreateCastExpression(IExprementPosition pos, IExpression expWithType, IExpression expBase);

		ICastExpression CreateCastExpression(IExprementPosition pos, ICompiledType type, IExpression expBase);

		IAddressExpression CreateAddressExpression(IExprementPosition pos, IDirectVariable dirvar);

		IVariableExpression2 CreateVariableExpression(IExprementPosition pos, string stName);

		IIndexAccessExpression CreateIndexAccessExpression(IExprementPosition pos, IExpression expBase, IExpression expAccess);

		IIndexAccessExpression CreateIndexAccessExpression(IExprementPosition pos, IExpression expBase, List<IExpression> expAccesses);

		ICompoAccessExpression CreateCompoAccessExpression(IExprementPosition pos, IExpression expLeft, IVariableExpression2 expRight);

		ICompoAccessExpression CreateBitAccessExpression(IExprementPosition pos, IExpression expLeft, ILiteralExpression expRight);

		IDeRefAccessExpression CreateDeRefAccessExpression(IExprementPosition pos, IExpression expBase);

		IGlobalScopeExpression CreateGlobalScopeExpression(IExprementPosition pos, IExpression expBase);

		ISystemScopeExpression CreateSystemScopeExpression(IExprementPosition pos, IExpression expBase);

		IMultipleIndexInitialization CreateMultipleIndexInitialisation(IExprementPosition pos, IExpression expNumber, IExpression expValue);

		IArrayInitialization CreateArrayInitialisation(IExprementPosition pos, List<IExpression> expInitvalues);

		IStructureInitialization CreateStructureInitialisation(IExprementPosition pos, List<IAssignmentExpression> initAssigns);

		IDefineReference CreateDefineReference(IExprementPosition pos, string stDefine);

		IVariableReference CreateVariableReference(IExprementPosition pos, IExpression expInstancePath);

		ITypeReference2 CreateTypeReference(IExprementPosition pos, IExpression expInstancePath);

		IPouReference2 CreatePouReference(IExprementPosition pos, IExpression expInstancePath);

		IDefinedExpression CreateDefinedExpression(IExprementPosition pos, IExpression expItemReference);

		ICompilerVersionExpression CreateCompilerVersioExpression(IExprementPosition pos, Version version, Operator opComparison);

		IHasTypeExpression CreateHasTypeExpression(IExprementPosition pos, IVariableReference varref, ICompiledType type);

		IIsEnumTypeExpression CreateIsEnumTypeExpression(IExprementPosition pos, ICompiledType type);

		IHasTypeExpression CreateHasCompatibleTypeExpression(IExprementPosition pos, IVariableReference varref, ICompiledType type);

		IHasAttributeExpression CreateHasAttributeExpression(IExprementPosition pos, IExpression expItemReference, string stAttribute);

		IHasValueExpression CreateHasValueExpression(IExprementPosition pos, string stDefine, string stValue);

		IPragmaOperatorExpression CreatePragmaOperatorExpression(IExprementPosition pos, Operator op, IExpression expOperand1, IExpression expOperand2);

		IPragmaAssertion CreatePragmaAssertion(IExprementPosition pos, IExpression expCondition, string stAssertionText);

		IPragmaIfStatement CreatePragmaIfStatement(IExprementPosition pos, IExpression expCondition, ISequenceStatement2 seqThen, ISequenceStatement2 seqElse);

		IBreakPointStatement CreateBreakpointStatement(IExprementPosition pos, long lBPPosition, long lSuccessorPosition);

		IDefineStatement CreateDefineStatement(IExprementPosition pos, bool bDefine, string stIdent, string stValue);
	}
}
