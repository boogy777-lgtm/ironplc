using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ITreeFactory
	{
		_ISequenceStatement CreateSequenceStatement(_IStatement[] statements);

		_IErrorStatement CreateErrorStatement();

		_IEmptyStatement CreateEmptyStatement();

		_IWhileStatement CreateWhileStatement(_IExpression expCondition, _IStatement stateControlled);

		_IRepeatStatement CreateRepeatStatement(_IExpression expCondition, _IStatement stateControlled);

		_ICaseRangeExpression CreateCaseRangeExpression(_IExpression expLow, _IExpression expHigh);

		_ICaseLabelStatement CreateCaseLabelStatement(_IExpression[] cases);

		_ICase CreateCase(_ICaseLabelStatement caselabel, _IStatement controlled);

		_ICaseStatement CreateCaseStatement(_IExpression expswitch, _ICase[] cases, _IStatement elsecase);

		_IForStatement CreateForStatement(_IExpression counterstart, _IExpression counter, _IExpression by, _IExpression upper, _IExpression condition, _IStatement controlled);

		_IExitStatement CreateExitStatement();

		_IContinueStatement CreateContinueStatement();

		_IAssignmentExpression CreateAssignmentExpression(_IExpression expLValue, _IExpression expRValue, Operator kindof);

		_IElseIf CreateElseIf(_IExpression expCondition, _IStatement controlled);

		_IIfStatement CreateIfStatement(_IExpression condition, _IStatement stateThen, _IStatement stateElse, _IElseIf[] elsifs);

		_ITryCatchStatement CreateTryCatchStatement(_ISequenceStatement seqTry, _ISequenceStatement seqCatch, _ISequenceStatement seqFinally, _IExpression expException);

		_IReturnStatement CreateReturnStatement(_IExpression expCondition);

		_IJumpStatement CreateJumpStatement(_IExpression expCondition, string stLabel);

		_ILabelStatement CreateLabelStatement(string stLabel);

		_ICommentStatement CreateCommentStatement(string stComment, bool bDocComment);

		_IPragmaStatement CreatePragmaStatement(string stPragma);

		_IMessageGuidPragmaStatement CreateMessageGuidPragmaStatement(Guid mguid, string stText);

		_IWarningDisableRestorePragmaStatement CreateWarningDisableRestorePragmaStatement(bool bRestore, string stId, string stText);

		_IExpressionStatement CreateExpressionStatement(_IExpression exp);

		_IErrorExpression CreateErrorExpression();

		_INamespaceAccessExpression CreateNamespaceAccessExpression(_IExpression expNamespace, _IExpression expAccess);

		_ICallExpression CreateCallExpression(_IExpression expCallee, _IExpression expCondition, _IType typeExpected, _IExpression[] actparams, _IExpression[] formparams, _IExpression[] actualoutputs, _IExpression[] formoutputs, _IExpression[] emptyassigns);

		_IOperatorExpression CreateOperatorExpression(Operator oc, _IExpression[] expoperands);

		_IConversionExpression CreateConversionExpression(TypeClass from, TypeClass to, _IExpression exp);

		_INewExpression CreateNewExpression(_IType typeIn, _IExpression expCount, IAssignmentExpression[] fbinitparams);

		_ICastExpression CreateCastExpression(_IExpression expWithType, _IExpression expBase, ICompiledType type);

		_IThisExpression CreateThisExpression();

		_IBaseExpression CreateBaseExpression();

		_ILiteralExpression CreateIntegerLiteralExpression(long lValue, TypeClass constantType, bool bNegative);

		_ILiteralExpression CreateBasedIntegerLiteralExpression(long lValue, TypeClass constantType, int nbase, bool bNegative);

		_ILiteralExpression CreateStringLiteralExpression(string stValue, TypeClass constantType);

		_ILiteralExpression CreateFloatLiteralExpression(double dValue, TypeClass constantType);

		_ITypeExpression CreateTypeExpression(ICompiledType cType);

		_IAddressExpression CreateAddressExpression(IDirectVariable dirvar);

		_IVariableExpression CreateVariableExpression(string stName);

		_IIndexAccessExpression CreateIndexAccessExpression(_IExpression expBase, _IExpression[] expAccesses);

		_ICompoAccessExpression CreateCompoAccessExpression(_IExpression expLeft, _IExpression expRight);

		_IDeRefAccessExpression CreateDeRefAccessExpression(_IExpression exp);

		_IGlobalScopeExpression CreateGlobalScopeExpression(_IExpression exp);

		_ISystemScopeExpression CreateSystemScopeExpression(_IExpression exp);

		_IPoolScopeExpression CreatePoolScopeExpression(_IExpression exp);

		_ICurrentTaskExpression CreateCurrentTaskExpression(_IExpression exp);

		_INullExpression CreateNullExpression();

		_INullStatement CreateNullStatement();

		_IHasValueExpression CreateHasValueExpression(string stDefine, string stValue);

		_IHasConstantValueExpression CreateHasConstantValueExpression(_IExpression Constant, _IExpression Value, Operator opComparison);

		_IPragmaAssertion CreatePragmaAssertion(_IExpression Condition, string ErrorOutput);

		_IPragmaElseIf CreatePragmaElseIf(_IPragmaExpression expCondition, _IStatement stControlled);

		_IDefineStatement CreateDefineStatement(bool bDefine, string stIdent, string stValue);

		_IArrayInitialization CreateArrayInitialisation(IList<_IExpression> initexprs);

		_IStructureInitialization CreateStructureInitialisation(IList<_IAssignmentExpression> explist);

		_IMultipleIndexInitialization CreateMultipleIndexInitialisation(_IExpression expValue, _IExpression expNumber);

		_IDefineReference CreateDefineReference(string stDefine);

		_IVariableReference CreateVariableReference(_IExpression instancePath);

		_ITypeReference CreateTypeReference(_IExpression instancePath);

		_IPouReference CreatePouReference(_IExpression instancePath);

		_ITaskReference CreateTaskReference(string stTask);

		_IResourceReference CreateResourceReference(string stResource);

		_IDefinedExpression CreateDefinedExpression(_IItemReference itref);

		_IXRefExpression CreateXRefExpression(_IItemReference itref, _IItemReference itrefFrom);

		_ICompilerVersionExpression CreateCompilerVersionExpression(Version versionToTest, Operator opComparison);

		_IPragmaOperatorExpression CreatePragmaOperatorExpression(PragmaOperator op, _IExpression[] Operands);

		_IPragmaIfStatement CreatePragmaIfStatement(_IExpression expCond, _IStatement ifthen, _IStatement ifelse, _IPragmaElseIf[] elsifs);

		_IHasCompatibleTypeExpression CreateHasCompatibleTypeExpression(_IVariableReference varref, ICompiledType type);

		_IHasTypeExpression CreateHasTypeExpression(_IVariableReference varref, ICompiledType type);

		_IIsEnumTypeExpression CreateIsEnumTypeExpression(ICompiledType type);

		_IHasAttributeExpression CreateHasAttributeExpression(_IItemReference itref, string stAttribute);

		_ITypeExpression CreateTypeExpression(ICompiledType2 type);

		_IRuntimeVersionExpression CreateRuntimeVersionExpression(Version v, Operator test);
	}
}
