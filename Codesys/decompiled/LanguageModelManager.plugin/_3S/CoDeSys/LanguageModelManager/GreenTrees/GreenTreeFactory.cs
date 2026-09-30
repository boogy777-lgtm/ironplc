using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x02000230 RID: 560
	[SuppressMessage("Major Code Smell", "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)", Justification = "Factory pattern")]
	public class GreenTreeFactory : ITreeFactory, ITreeFactory7, ITreeFactory6, ITreeFactory5, ITreeFactory4, ITreeFactory3, ITreeFactory2
	{
		// Token: 0x17000A5E RID: 2654
		// (get) Token: 0x0600248C RID: 9356 RVA: 0x0005C5DE File Offset: 0x0005B5DE
		public static GreenTreeFactory Singleton { get; } = new GreenTreeFactory();

		// Token: 0x0600248D RID: 9357 RVA: 0x0005C5E5 File Offset: 0x0005B5E5
		public _IErrorStatement CreateErrorStatement()
		{
			return new ErrorStatement_Green();
		}

		// Token: 0x0600248E RID: 9358 RVA: 0x0005C5EC File Offset: 0x0005B5EC
		public _IEmptyStatement CreateEmptyStatement()
		{
			return new EmptyStatement_Green();
		}

		// Token: 0x0600248F RID: 9359 RVA: 0x0005C5F3 File Offset: 0x0005B5F3
		public _IWhileStatement CreateWhileStatement(_IExpression expCondition, _IStatement stateControlled)
		{
			return new WhileStatement_Green(expCondition, stateControlled);
		}

		// Token: 0x06002490 RID: 9360 RVA: 0x0005C5FC File Offset: 0x0005B5FC
		public _IRepeatStatement CreateRepeatStatement(_IExpression expCondition, _IStatement stateControlled)
		{
			return new RepeatStatement_Green(expCondition, stateControlled);
		}

		// Token: 0x06002491 RID: 9361 RVA: 0x0005C605 File Offset: 0x0005B605
		public _ICaseRangeExpression CreateCaseRangeExpression(_IExpression expLow, _IExpression expHigh)
		{
			return new CaseRangeExpression_Green(expLow, expHigh);
		}

		// Token: 0x06002492 RID: 9362 RVA: 0x0005C60E File Offset: 0x0005B60E
		public _ICaseLabelStatement CreateCaseLabelStatement(_IExpression[] cases)
		{
			return new CaseLabelStatement_Green(cases);
		}

		// Token: 0x06002493 RID: 9363 RVA: 0x0005C616 File Offset: 0x0005B616
		public _ICase CreateCase(_ICaseLabelStatement caselabel, _IStatement controlled)
		{
			return new Case_Green(caselabel, controlled);
		}

		// Token: 0x06002494 RID: 9364 RVA: 0x0005C61F File Offset: 0x0005B61F
		public _ICaseStatement CreateCaseStatement(_IExpression expswitch, _ICase[] cases, _IStatement elsecase)
		{
			return new CaseStatement_Green(expswitch, cases, elsecase);
		}

		// Token: 0x06002495 RID: 9365 RVA: 0x0005C629 File Offset: 0x0005B629
		public _IForStatement CreateForStatement(_IExpression counterstart, _IExpression counter, _IExpression by, _IExpression upper, _IExpression condition, _IStatement controlled)
		{
			return new ForStatement_Green(counterstart, counter, by, upper, condition, controlled);
		}

		// Token: 0x06002496 RID: 9366 RVA: 0x0005C639 File Offset: 0x0005B639
		public _IExitStatement CreateExitStatement()
		{
			return new ExitStatement_Green();
		}

		// Token: 0x06002497 RID: 9367 RVA: 0x0005C640 File Offset: 0x0005B640
		public _IContinueStatement CreateContinueStatement()
		{
			return new ContinueStatement_Green();
		}

		// Token: 0x06002498 RID: 9368 RVA: 0x0005C647 File Offset: 0x0005B647
		public _ISequenceStatement CreateSequenceStatement(_IStatement[] statements)
		{
			return new SequenceStatement_Green(statements);
		}

		// Token: 0x06002499 RID: 9369 RVA: 0x0005C64F File Offset: 0x0005B64F
		public _IAssignmentExpression CreateAssignmentExpression(_IExpression expLValue, _IExpression expRValue, Operator kindof)
		{
			return new AssignmentExpression_Green(expLValue, expRValue, kindof);
		}

		// Token: 0x0600249A RID: 9370 RVA: 0x0005C659 File Offset: 0x0005B659
		public _IElseIf CreateElseIf(_IExpression expCondition, _IStatement controlled)
		{
			return new ElseIf_Green(expCondition, controlled);
		}

		// Token: 0x0600249B RID: 9371 RVA: 0x0005C662 File Offset: 0x0005B662
		public _IIfStatement CreateIfStatement(_IExpression condition, _IStatement stateThen, _IStatement stateElse, _IElseIf[] elsifs)
		{
			return new IfStatement_Green(condition, stateThen, stateElse, elsifs);
		}

		// Token: 0x0600249C RID: 9372 RVA: 0x0005C66E File Offset: 0x0005B66E
		public _ITryCatchStatement CreateTryCatchStatement(_ISequenceStatement seqTry, _ISequenceStatement seqCatch, _ISequenceStatement seqFinally, _IExpression expException)
		{
			return new TryCatchStatement_Green(seqTry, seqCatch, seqFinally, expException);
		}

		// Token: 0x0600249D RID: 9373 RVA: 0x0005C67A File Offset: 0x0005B67A
		public _IReturnStatement CreateReturnStatement(_IExpression expCondition)
		{
			return new ReturnStatement_Green(expCondition);
		}

		// Token: 0x0600249E RID: 9374 RVA: 0x0005C682 File Offset: 0x0005B682
		public _IJumpStatement CreateJumpStatement(_IExpression expCondition, string stLabel)
		{
			return new JumpStatement_Green(expCondition, stLabel);
		}

		// Token: 0x0600249F RID: 9375 RVA: 0x0005C68B File Offset: 0x0005B68B
		public _ILabelStatement CreateLabelStatement(string stLabel)
		{
			return new LabelStatement_Green(stLabel);
		}

		// Token: 0x060024A0 RID: 9376 RVA: 0x0005C693 File Offset: 0x0005B693
		public _ICommentStatement CreateCommentStatement(string stComment, bool bDocComment)
		{
			return new CommentStatement_Green(stComment, bDocComment);
		}

		// Token: 0x060024A1 RID: 9377 RVA: 0x0005C69C File Offset: 0x0005B69C
		public _IPragmaStatement CreatePragmaStatement(string stPragma)
		{
			return new PragmaStatement_Green(stPragma);
		}

		// Token: 0x060024A2 RID: 9378 RVA: 0x0005C6A4 File Offset: 0x0005B6A4
		public _IMessageGuidPragmaStatement CreateMessageGuidPragmaStatement(Guid mguid, string stText)
		{
			return new MessageGuidPragmaStatement_Green(mguid, stText);
		}

		// Token: 0x060024A3 RID: 9379 RVA: 0x0005C6AD File Offset: 0x0005B6AD
		public _IWarningDisableRestorePragmaStatement CreateWarningDisableRestorePragmaStatement(bool bRestore, string stId, string stText)
		{
			return new WarningDisableRestorePragmaStatement_Green(bRestore, stId, stText);
		}

		// Token: 0x060024A4 RID: 9380 RVA: 0x0005C6B7 File Offset: 0x0005B6B7
		public _IExpressionStatement CreateExpressionStatement(_IExpression exp)
		{
			return new ExpressionStatement_Green(exp);
		}

		// Token: 0x060024A5 RID: 9381 RVA: 0x0005C6BF File Offset: 0x0005B6BF
		public _IErrorExpression CreateErrorExpression()
		{
			return new ErrorExpression_Green();
		}

		// Token: 0x060024A6 RID: 9382 RVA: 0x0005C6C6 File Offset: 0x0005B6C6
		public _INamespaceAccessExpression CreateNamespaceAccessExpression(_IExpression expNamespace, _IExpression expAccess)
		{
			return new NamespaceAccessExpression_Green(expNamespace, expAccess);
		}

		// Token: 0x060024A7 RID: 9383 RVA: 0x0005C6CF File Offset: 0x0005B6CF
		public _ICallExpression CreateCallExpression(_IExpression expCallee, _IExpression expCondition, _IType typeExpected, _IExpression[] actparams, _IExpression[] formparams, _IExpression[] actualoutputs, _IExpression[] formoutputs, _IExpression[] emptyassigns)
		{
			if (emptyassigns.Length != 0)
			{
				return new CallExpression_Green_WithEmptyAssigns(expCallee, expCondition, typeExpected, actparams, formparams, actualoutputs, formoutputs, emptyassigns);
			}
			return new CallExpression_Green(expCallee, expCondition, typeExpected, actparams, formparams, actualoutputs, formoutputs);
		}

		// Token: 0x060024A8 RID: 9384 RVA: 0x0005C6F9 File Offset: 0x0005B6F9
		public _IOperatorExpression CreateOperatorExpression(Operator oc, _IExpression[] expoperands)
		{
			return new OperatorExpression_Green(oc, expoperands);
		}

		// Token: 0x060024A9 RID: 9385 RVA: 0x0005C702 File Offset: 0x0005B702
		public _IConversionExpression CreateConversionExpression(TypeClass from, TypeClass to, _IExpression exp)
		{
			return new ConversionExpression_Green(from, to, exp);
		}

		// Token: 0x060024AA RID: 9386 RVA: 0x0005C70C File Offset: 0x0005B70C
		public _INewExpression CreateNewExpression(_IType typeIn, _IExpression expCount, IAssignmentExpression[] fbinitparams)
		{
			return new NewExpression_Green(typeIn, expCount, fbinitparams);
		}

		// Token: 0x060024AB RID: 9387 RVA: 0x0005C716 File Offset: 0x0005B716
		public _ICastExpression CreateCastExpression(_IExpression expWithType, _IExpression expBase, ICompiledType type)
		{
			return new CastExpression_Green(expWithType, expBase, type);
		}

		// Token: 0x060024AC RID: 9388 RVA: 0x0005C720 File Offset: 0x0005B720
		public _IThisExpression CreateThisExpression()
		{
			return new ThisExpression_Green();
		}

		// Token: 0x060024AD RID: 9389 RVA: 0x0005C727 File Offset: 0x0005B727
		public _IBaseExpression CreateBaseExpression()
		{
			return new BaseExpression_Green();
		}

		// Token: 0x060024AE RID: 9390 RVA: 0x0005C72E File Offset: 0x0005B72E
		public _ILiteralExpression CreateIntegerLiteralExpression(long lValue, TypeClass constantType, bool bNegative)
		{
			return new IntegerLiteralExpression_Green(lValue, constantType, bNegative);
		}

		// Token: 0x060024AF RID: 9391 RVA: 0x0005C738 File Offset: 0x0005B738
		public _ILiteralExpression CreateBasedIntegerLiteralExpression(long lValue, TypeClass constantType, int nbase, bool bNegative)
		{
			return new BasedIntegerLiteralExpression_Green(lValue, constantType, nbase, bNegative);
		}

		// Token: 0x060024B0 RID: 9392 RVA: 0x0005C744 File Offset: 0x0005B744
		public _ILiteralExpression CreateStringLiteralExpression(string stValue, TypeClass constantType)
		{
			return new StringLiteralExpression_Green(stValue, constantType);
		}

		// Token: 0x060024B1 RID: 9393 RVA: 0x0005C74D File Offset: 0x0005B74D
		public _ILiteralExpression CreateStringLiteralExpression(string stValue, TypeClass constantType, StringEncoding stringEncoding)
		{
			return new StringLiteralExpression_Green(stValue, constantType, stringEncoding);
		}

		// Token: 0x060024B2 RID: 9394 RVA: 0x0005C757 File Offset: 0x0005B757
		public _ILiteralExpression CreateFloatLiteralExpression(double dValue, TypeClass constantType)
		{
			return new FloatLiteralExpression_Green(dValue, constantType);
		}

		// Token: 0x060024B3 RID: 9395 RVA: 0x0005C760 File Offset: 0x0005B760
		public _ITypeExpression CreateTypeExpression(ICompiledType cType)
		{
			return new TypeExpression_Green(cType);
		}

		// Token: 0x060024B4 RID: 9396 RVA: 0x0005C760 File Offset: 0x0005B760
		public _ITypeExpression CreateTypeExpression(ICompiledType2 type)
		{
			return new TypeExpression_Green(type);
		}

		// Token: 0x060024B5 RID: 9397 RVA: 0x0005C768 File Offset: 0x0005B768
		public _IAddressExpression CreateAddressExpression(IDirectVariable dirvar)
		{
			return new AddressExpression_Green(dirvar);
		}

		// Token: 0x060024B6 RID: 9398 RVA: 0x0005C770 File Offset: 0x0005B770
		public _IVariableExpression CreateVariableExpression(string stName)
		{
			return new VariableExpression_Green(stName);
		}

		// Token: 0x060024B7 RID: 9399 RVA: 0x0005C778 File Offset: 0x0005B778
		public _IIndexAccessExpression CreateIndexAccessExpression(_IExpression expBase, _IExpression[] expAccesses)
		{
			return new IndexAccessExpression_Green(expBase, expAccesses);
		}

		// Token: 0x060024B8 RID: 9400 RVA: 0x0005C781 File Offset: 0x0005B781
		public _ICompoAccessExpression CreateCompoAccessExpression(_IExpression expLeft, _IExpression expRight)
		{
			return new CompoAccessExpression_Green(expLeft, expRight);
		}

		// Token: 0x060024B9 RID: 9401 RVA: 0x0005C78A File Offset: 0x0005B78A
		public _IDeRefAccessExpression CreateDeRefAccessExpression(_IExpression exp)
		{
			return new DeRefAccessExpression_Green(exp);
		}

		// Token: 0x060024BA RID: 9402 RVA: 0x0005C792 File Offset: 0x0005B792
		public _IGlobalScopeExpression CreateGlobalScopeExpression(_IExpression exp)
		{
			return new GlobalScopeExpression_Green(exp);
		}

		// Token: 0x060024BB RID: 9403 RVA: 0x0005C79A File Offset: 0x0005B79A
		public _ISystemScopeExpression CreateSystemScopeExpression(_IExpression exp)
		{
			return new SystemScopeExpression_Green(exp);
		}

		// Token: 0x060024BC RID: 9404 RVA: 0x0005C7A2 File Offset: 0x0005B7A2
		public _IPoolScopeExpression CreatePoolScopeExpression(_IExpression exp)
		{
			return new PoolScopeExpression_Green(exp);
		}

		// Token: 0x060024BD RID: 9405 RVA: 0x0005C7AA File Offset: 0x0005B7AA
		public _ICurrentTaskExpression CreateCurrentTaskExpression(_IExpression exp)
		{
			return new CurrentTaskExpression_Green(exp);
		}

		// Token: 0x060024BE RID: 9406 RVA: 0x0005C7B2 File Offset: 0x0005B7B2
		public _INullExpression CreateNullExpression()
		{
			return new NullExpression_Green();
		}

		// Token: 0x060024BF RID: 9407 RVA: 0x0005C7B9 File Offset: 0x0005B7B9
		public _INullStatement CreateNullStatement()
		{
			return new NullStatement_Green();
		}

		// Token: 0x060024C0 RID: 9408 RVA: 0x0005C7C0 File Offset: 0x0005B7C0
		public _IHasValueExpression CreateHasValueExpression(string stDefine, string stValue)
		{
			return new HasValueExpression_Green(stDefine, stValue);
		}

		// Token: 0x060024C1 RID: 9409 RVA: 0x0005C7C9 File Offset: 0x0005B7C9
		public _IHasConstantValueExpression CreateHasConstantValueExpression(_IExpression Constant, _IExpression Value, Operator opComparison)
		{
			return new HasConstantValueExpression_Green(Constant, Value, opComparison);
		}

		// Token: 0x060024C2 RID: 9410 RVA: 0x0005C7D3 File Offset: 0x0005B7D3
		public _IHasConstantTypeExpression CreateHasConstantTypeExpression(_IExpression Constant, bool bConstantTypeReplaced)
		{
			return new HasConstantTypeExpression_Green(Constant, bConstantTypeReplaced);
		}

		// Token: 0x060024C3 RID: 9411 RVA: 0x0005C7DC File Offset: 0x0005B7DC
		public _IPragmaOperatorExpression CreatePragmaOperatorExpression(PragmaOperator op, _IExpression[] Operands)
		{
			return new PragmaOperatorExpression_Green(op, Operands);
		}

		// Token: 0x060024C4 RID: 9412 RVA: 0x0005C7E5 File Offset: 0x0005B7E5
		public _IPragmaAssertion CreatePragmaAssertion(_IExpression Condition, string ErrorOutput)
		{
			return new PragmaAssertion_Green(Condition, ErrorOutput);
		}

		// Token: 0x060024C5 RID: 9413 RVA: 0x0005C7EE File Offset: 0x0005B7EE
		public _IPragmaElseIf CreatePragmaElseIf(_IPragmaExpression expCondition, _IStatement stControlled)
		{
			return new PragmaElseIf_Green(expCondition, stControlled);
		}

		// Token: 0x060024C6 RID: 9414 RVA: 0x0005C7F7 File Offset: 0x0005B7F7
		public _IDefineStatement CreateDefineStatement(bool bDefine, string stIdent, string stValue)
		{
			return new DefineStatement_Green(bDefine, stIdent, stValue);
		}

		// Token: 0x060024C7 RID: 9415 RVA: 0x0005C801 File Offset: 0x0005B801
		public _IArrayInitialization CreateArrayInitialisation(IList<_IExpression> initexprs)
		{
			return new ArrayInitialisation_Green(initexprs);
		}

		// Token: 0x060024C8 RID: 9416 RVA: 0x0005C809 File Offset: 0x0005B809
		public _IStructureInitialization CreateStructureInitialisation(IList<_IAssignmentExpression> explist)
		{
			return new StructureInitialisation_Green(explist);
		}

		// Token: 0x060024C9 RID: 9417 RVA: 0x0005C811 File Offset: 0x0005B811
		public _IMultipleIndexInitialization CreateMultipleIndexInitialisation(_IExpression expValue, _IExpression expNumber)
		{
			return new MultipleIndexInitialisation_Green(expValue, expNumber);
		}

		// Token: 0x060024CA RID: 9418 RVA: 0x0005C81A File Offset: 0x0005B81A
		public _IDefineReference CreateDefineReference(string stDefine)
		{
			return new DefineReference_Green(stDefine);
		}

		// Token: 0x060024CB RID: 9419 RVA: 0x0005C822 File Offset: 0x0005B822
		public _IProjectDefinedExpression CreateProjectDefinedExpression(_IDefineReference defineReference)
		{
			return new ProjectDefinedExpression_Green
			{
				DefineReference = defineReference
			};
		}

		// Token: 0x060024CC RID: 9420 RVA: 0x0005C830 File Offset: 0x0005B830
		public _IVariableReference CreateVariableReference(_IExpression instancePath)
		{
			return new VariableReference_Green(instancePath);
		}

		// Token: 0x060024CD RID: 9421 RVA: 0x0005C838 File Offset: 0x0005B838
		public _ITypeReference CreateTypeReference(_IExpression instancePath)
		{
			return new TypeReference_Green(instancePath);
		}

		// Token: 0x060024CE RID: 9422 RVA: 0x0005C840 File Offset: 0x0005B840
		public _IPouReference CreatePouReference(_IExpression instancePath)
		{
			return new PouReference_Green(instancePath);
		}

		// Token: 0x060024CF RID: 9423 RVA: 0x0005C848 File Offset: 0x0005B848
		public _ITaskReference CreateTaskReference(string stTask)
		{
			return new TaskReference_Green(stTask);
		}

		// Token: 0x060024D0 RID: 9424 RVA: 0x0005C850 File Offset: 0x0005B850
		public _IResourceReference CreateResourceReference(string stResource)
		{
			return new ResourceReference_Green(stResource);
		}

		// Token: 0x060024D1 RID: 9425 RVA: 0x0005C858 File Offset: 0x0005B858
		public _IDefinedExpression CreateDefinedExpression(_IItemReference itref)
		{
			return new DefinedExpression_Green(itref);
		}

		// Token: 0x060024D2 RID: 9426 RVA: 0x0005C860 File Offset: 0x0005B860
		public _IXRefExpression CreateXRefExpression(_IItemReference itref, _IItemReference itrefFrom)
		{
			return new XRefExpression_Green(itref, itrefFrom);
		}

		// Token: 0x060024D3 RID: 9427 RVA: 0x0005C869 File Offset: 0x0005B869
		public _ICompilerVersionExpression CreateCompilerVersionExpression(Version versionToTest, Operator opComparison)
		{
			return new CompilerVersionExpression_Green(versionToTest, opComparison);
		}

		// Token: 0x060024D4 RID: 9428 RVA: 0x0005C872 File Offset: 0x0005B872
		public _IPragmaIfStatement CreatePragmaIfStatement(_IExpression expCond, _IStatement ifthen, _IStatement ifelse, _IPragmaElseIf[] elsifs)
		{
			return new PragmaIfStatement_Green(expCond, ifthen, ifelse, elsifs);
		}

		// Token: 0x060024D5 RID: 9429 RVA: 0x0005C87E File Offset: 0x0005B87E
		public _IHasCompatibleTypeExpression CreateHasCompatibleTypeExpression(_IVariableReference varref, ICompiledType type)
		{
			return new HasCompatibleTypeExpression_Green(varref, type);
		}

		// Token: 0x060024D6 RID: 9430 RVA: 0x0005C887 File Offset: 0x0005B887
		public _IHasTypeExpression CreateHasTypeExpression(_IVariableReference varref, ICompiledType type)
		{
			return new HasTypeExpression_Green(varref, type);
		}

		// Token: 0x060024D7 RID: 9431 RVA: 0x0005C890 File Offset: 0x0005B890
		public _IIsEnumTypeExpression CreateIsEnumTypeExpression(ICompiledType type)
		{
			return new IsEnumTypeExpression_Green(type);
		}

		// Token: 0x060024D8 RID: 9432 RVA: 0x0005C898 File Offset: 0x0005B898
		public _IHasAttributeExpression CreateHasAttributeExpression(_IItemReference itref, string stAttribute)
		{
			return new HasAttributeExpression_Green(itref, stAttribute);
		}

		// Token: 0x060024D9 RID: 9433 RVA: 0x0005C8A1 File Offset: 0x0005B8A1
		public _IRuntimeVersionExpression CreateRuntimeVersionExpression(Version v, Operator test)
		{
			return new RuntimeVersionExpression_Green(v, test);
		}

		// Token: 0x060024DA RID: 9434 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void ExtendCounter(_IForStatement red)
		{
		}

		// Token: 0x060024DB RID: 9435 RVA: 0x0005C8AA File Offset: 0x0005B8AA
		public _IBreakPointStatement CreateBreakPointStatement(long bpPosition, long successorPosition)
		{
			return new BreakpointStatement_Green(bpPosition, successorPosition);
		}

		// Token: 0x060024DC RID: 9436 RVA: 0x0005C8B3 File Offset: 0x0005B8B3
		public _IPartialAccessExpression CreatePartialAccessExpression(_IExpression left, DirectVariableSize partSize, int partOffset)
		{
			return new PartialAccessExpression_Green
			{
				_Left = left,
				PartSize = partSize,
				PartOffset = partOffset
			};
		}

		// Token: 0x060024DD RID: 9437 RVA: 0x0005C8CF File Offset: 0x0005B8CF
		public _IImplicitConversionExpression CreateImplicitConversionExpression(TypeClass from, TypeClass to, _IExpression expression)
		{
			return new ImplicitConversionExpression(from, to)
			{
				_Exp = expression
			};
		}

		// Token: 0x060024DE RID: 9438 RVA: 0x0005C8DF File Offset: 0x0005B8DF
		public _IImplicitCodeSectionPragma CreateImplicitCodeSectionPragma(bool bOn, string stText)
		{
			return new ImplicitCodeSectionPragmaStatement_Green(bOn, stText);
		}

		// Token: 0x060024DF RID: 9439 RVA: 0x0005C8E8 File Offset: 0x0005B8E8
		public _ILocalSignatureIdPragma CreateLocalSignatureIdPragma(int nId, string stText)
		{
			return new LocalSignatureIdPragmaStatement_Green(nId, stText);
		}
	}
}
