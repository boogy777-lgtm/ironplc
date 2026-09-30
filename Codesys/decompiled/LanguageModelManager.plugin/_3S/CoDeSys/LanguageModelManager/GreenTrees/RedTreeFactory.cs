using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x0200023E RID: 574
	[SuppressMessage("Major Code Smell", "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)", Justification = "Visitor pattern")]
	public class RedTreeFactory : ITreeFactory, ITreeFactory7, ITreeFactory6, ITreeFactory5, ITreeFactory4, ITreeFactory3, ITreeFactory2
	{
		// Token: 0x0600260F RID: 9743 RVA: 0x00002476 File Offset: 0x00001476
		private RedTreeFactory()
		{
		}

		// Token: 0x17000AED RID: 2797
		// (get) Token: 0x06002610 RID: 9744 RVA: 0x0005E670 File Offset: 0x0005D670
		public static RedTreeFactory Singleton { get; } = new RedTreeFactory();

		// Token: 0x06002611 RID: 9745 RVA: 0x0003EBDB File Offset: 0x0003DBDB
		public _IErrorStatement CreateErrorStatement()
		{
			return new ErrorStatement();
		}

		// Token: 0x06002612 RID: 9746 RVA: 0x0003EBE2 File Offset: 0x0003DBE2
		public _IEmptyStatement CreateEmptyStatement()
		{
			return new EmptyStatement();
		}

		// Token: 0x06002613 RID: 9747 RVA: 0x0003EC01 File Offset: 0x0003DC01
		public _IWhileStatement CreateWhileStatement(_IExpression expCondition, _IStatement stateControlled)
		{
			return new WhileStatement(expCondition, stateControlled);
		}

		// Token: 0x06002614 RID: 9748 RVA: 0x0003EC1B File Offset: 0x0003DC1B
		public _IRepeatStatement CreateRepeatStatement(_IExpression expCondition, _IStatement stateControlled)
		{
			return new RepeatStatement(expCondition, stateControlled);
		}

		// Token: 0x06002615 RID: 9749 RVA: 0x0005E677 File Offset: 0x0005D677
		public _ICaseRangeExpression CreateCaseRangeExpression(_IExpression expLow, _IExpression expHigh)
		{
			return new CaseRangeExpression
			{
				_Low = expLow,
				_High = expHigh
			};
		}

		// Token: 0x06002616 RID: 9750 RVA: 0x0005E68C File Offset: 0x0005D68C
		public _ICaseLabelStatement CreateCaseLabelStatement(_IExpression[] cases)
		{
			CaseLabelStatement caseLabelStatement = new CaseLabelStatement();
			for (int i = 0; i < cases.Length; i++)
			{
				caseLabelStatement.AddCase(cases[i]);
			}
			return caseLabelStatement;
		}

		// Token: 0x06002617 RID: 9751 RVA: 0x0005E6B7 File Offset: 0x0005D6B7
		public _ICase CreateCase(_ICaseLabelStatement caselabel, _IStatement controlled)
		{
			return new Case(caselabel, controlled);
		}

		// Token: 0x06002618 RID: 9752 RVA: 0x0005E6C0 File Offset: 0x0005D6C0
		public _ICaseStatement CreateCaseStatement(_IExpression expswitch, _ICase[] cases, _IStatement elsecase)
		{
			CaseStatement caseStatement = new CaseStatement();
			caseStatement._Switch = expswitch;
			foreach (_ICase @case in cases)
			{
				caseStatement.AddCase(@case);
			}
			caseStatement._Else = elsecase;
			return caseStatement;
		}

		// Token: 0x06002619 RID: 9753 RVA: 0x0005E6FD File Offset: 0x0005D6FD
		public _IForStatement CreateForStatement(_IExpression counterstart, _IExpression counter, _IExpression by, _IExpression upper, _IExpression condition, _IStatement controlled)
		{
			return new ForStatement
			{
				_CounterStart = counterstart,
				_By = by,
				_UpperBound = upper,
				_Controlled = controlled,
				_Counter = counter,
				_Condition = condition
			};
		}

		// Token: 0x0600261A RID: 9754 RVA: 0x0003EC96 File Offset: 0x0003DC96
		public _IExitStatement CreateExitStatement()
		{
			return new ExitStatement();
		}

		// Token: 0x0600261B RID: 9755 RVA: 0x0003EC9D File Offset: 0x0003DC9D
		public _IContinueStatement CreateContinueStatement()
		{
			return new ContinueStatement();
		}

		// Token: 0x0600261C RID: 9756 RVA: 0x0005E734 File Offset: 0x0005D734
		public _ISequenceStatement CreateSequenceStatement(_IStatement[] statements)
		{
			SequenceStatement sequenceStatement = new SequenceStatement(statements.Length);
			for (int i = 0; i < statements.Length; i++)
			{
				sequenceStatement.AddStatement(statements[i]);
			}
			return sequenceStatement;
		}

		// Token: 0x0600261D RID: 9757 RVA: 0x0005E762 File Offset: 0x0005D762
		public _IAssignmentExpression CreateAssignmentExpression(_IExpression expLValue, _IExpression expRValue, Operator kindof)
		{
			return new AssignmentExpression
			{
				_LValue = expLValue,
				_RValue = expRValue,
				KindOf = kindof
			};
		}

		// Token: 0x0600261E RID: 9758 RVA: 0x0003ECE2 File Offset: 0x0003DCE2
		public _IElseIf CreateElseIf(_IExpression expCondition, _IStatement controlled)
		{
			return new ElseIf(expCondition, controlled);
		}

		// Token: 0x0600261F RID: 9759 RVA: 0x0005E780 File Offset: 0x0005D780
		public _IIfStatement CreateIfStatement(_IExpression condition, _IStatement stateThen, _IStatement stateElse, _IElseIf[] elsifs)
		{
			IfStatement ifStatement = new IfStatement();
			ifStatement._Condition = condition;
			ifStatement._IfThen = stateThen;
			ifStatement._IfElse = stateElse;
			foreach (_IElseIf elseIf in elsifs)
			{
				ifStatement.AddElseIf(elseIf);
			}
			return ifStatement;
		}

		// Token: 0x06002620 RID: 9760 RVA: 0x0005E7C5 File Offset: 0x0005D7C5
		public _ITryCatchStatement CreateTryCatchStatement(_ISequenceStatement seqTry, _ISequenceStatement seqCatch, _ISequenceStatement seqFinally, _IExpression expException)
		{
			return new TryCatchStatement
			{
				_Try = seqTry,
				_Catch = seqCatch,
				_Finally = seqFinally,
				_Exception = expException
			};
		}

		// Token: 0x06002621 RID: 9761 RVA: 0x0005E7E9 File Offset: 0x0005D7E9
		public _IReturnStatement CreateReturnStatement(_IExpression expCondition)
		{
			return new ReturnStatement
			{
				_Condition = expCondition
			};
		}

		// Token: 0x06002622 RID: 9762 RVA: 0x0005E7F7 File Offset: 0x0005D7F7
		public _IJumpStatement CreateJumpStatement(_IExpression expCondition, string stLabel)
		{
			return new JumpStatement(stLabel)
			{
				_Condition = expCondition
			};
		}

		// Token: 0x06002623 RID: 9763 RVA: 0x0003ED47 File Offset: 0x0003DD47
		public _ILabelStatement CreateLabelStatement(string stLabel)
		{
			return new LabelStatement(stLabel);
		}

		// Token: 0x06002624 RID: 9764 RVA: 0x0005E806 File Offset: 0x0005D806
		public _ICommentStatement CreateCommentStatement(string stComment, bool bDocComment)
		{
			return new CommentStatement(stComment)
			{
				DocComment = bDocComment
			};
		}

		// Token: 0x06002625 RID: 9765 RVA: 0x0005E815 File Offset: 0x0005D815
		public _IPragmaStatement CreatePragmaStatement(string stPragma)
		{
			return new PragmaStatement
			{
				Text = stPragma
			};
		}

		// Token: 0x06002626 RID: 9766 RVA: 0x0005E823 File Offset: 0x0005D823
		public _IMessageGuidPragmaStatement CreateMessageGuidPragmaStatement(Guid mguid, string stText)
		{
			return new MessageGuidPragmaStatement
			{
				MessageGuid = mguid,
				Text = stText
			};
		}

		// Token: 0x06002627 RID: 9767 RVA: 0x0005E838 File Offset: 0x0005D838
		public _IImplicitCodeSectionPragma CreateImplicitCodeSectionPragma(bool bOn, string stText)
		{
			return new ImplicitCodeSectionPragmaStatement
			{
				ImplicitOn = bOn,
				Text = stText
			};
		}

		// Token: 0x06002628 RID: 9768 RVA: 0x0005E84D File Offset: 0x0005D84D
		public _ILocalSignatureIdPragma CreateLocalSignatureIdPragma(int nId, string stText)
		{
			return new LocalSignatureIdPragma
			{
				LocalSignatureId = nId,
				Text = stText
			};
		}

		// Token: 0x06002629 RID: 9769 RVA: 0x0005E862 File Offset: 0x0005D862
		public _IWarningDisableRestorePragmaStatement CreateWarningDisableRestorePragmaStatement(bool bRestore, string stId, string stText)
		{
			return new WarningDisableRestorePragmaStatement
			{
				Restore = bRestore,
				Id = stId,
				Text = stText
			};
		}

		// Token: 0x0600262A RID: 9770 RVA: 0x0003ED97 File Offset: 0x0003DD97
		public _IExpressionStatement CreateExpressionStatement(_IExpression exp)
		{
			return new ExpressionStatement(exp);
		}

		// Token: 0x0600262B RID: 9771 RVA: 0x0003EDDD File Offset: 0x0003DDDD
		public _IErrorExpression CreateErrorExpression()
		{
			return new ErrorExpression();
		}

		// Token: 0x0600262C RID: 9772 RVA: 0x0005E87E File Offset: 0x0005D87E
		public _INamespaceAccessExpression CreateNamespaceAccessExpression(_IExpression expNamespace, _IExpression expAccess)
		{
			return new NamespaceAccessExpression
			{
				_Namespace = expNamespace,
				_Access = expAccess
			};
		}

		// Token: 0x0600262D RID: 9773 RVA: 0x0005E894 File Offset: 0x0005D894
		public _ICallExpression CreateCallExpression(_IExpression expCallee, _IExpression expCondition, _IType typeExpected, _IExpression[] actparams, _IExpression[] formparams, _IExpression[] actualoutputs, _IExpression[] formoutputs, _IExpression[] emptyassigns)
		{
			CallExpression callExpression = new CallExpression();
			callExpression._Callee = expCallee;
			callExpression._Condition = expCondition;
			callExpression.ExpectedType = typeExpected;
			for (int i = 0; i < actparams.Length; i++)
			{
				callExpression.AddParam(actparams[i]);
			}
			for (int j = 0; j < formparams.Length; j++)
			{
				callExpression.SetFormalParam(formparams[j], j);
			}
			for (int k = 0; k < actualoutputs.Length; k++)
			{
				callExpression.AddOutput(null, actualoutputs[k]);
			}
			for (int l = 0; l < formoutputs.Length; l++)
			{
				callExpression.SetFormalOutput(formoutputs[l], l);
			}
			for (int m = 0; m < emptyassigns.Length; m++)
			{
				callExpression.AddEmptyAssign(emptyassigns[m]);
			}
			return callExpression;
		}

		// Token: 0x0600262E RID: 9774 RVA: 0x0005E948 File Offset: 0x0005D948
		public _IOperatorExpression CreateOperatorExpression(Operator oc, _IExpression[] expoperands)
		{
			OperatorExpression operatorExpression = new OperatorExpression(oc);
			foreach (_IExpression exp in expoperands)
			{
				operatorExpression.AddOperand(exp);
			}
			return operatorExpression;
		}

		// Token: 0x0600262F RID: 9775 RVA: 0x0005E978 File Offset: 0x0005D978
		public _IConversionExpression CreateConversionExpression(TypeClass from, TypeClass to, _IExpression exp)
		{
			return new ConversionExpression(from, to)
			{
				_Exp = exp
			};
		}

		// Token: 0x06002630 RID: 9776 RVA: 0x0005E988 File Offset: 0x0005D988
		public _INewExpression CreateNewExpression(_IType typeIn, _IExpression expCount, IAssignmentExpression[] fbinitparams)
		{
			NewExpression newExpression = new NewExpression();
			newExpression._TypeToCast = typeIn;
			newExpression._Count = expCount;
			if (fbinitparams != null)
			{
				foreach (IAssignmentExpression assexp in fbinitparams)
				{
					newExpression.AddFBInitParam(assexp);
				}
			}
			return newExpression;
		}

		// Token: 0x06002631 RID: 9777 RVA: 0x0005E9C8 File Offset: 0x0005D9C8
		public _ICastExpression CreateCastExpression(_IExpression expWithType, _IExpression expBase, ICompiledType type)
		{
			return new CastExpression
			{
				ExpWithType = expWithType,
				BaseExpression = expBase,
				ExplicitelySpecifiedType = type
			};
		}

		// Token: 0x06002632 RID: 9778 RVA: 0x0003EE92 File Offset: 0x0003DE92
		public _IThisExpression CreateThisExpression()
		{
			return new ThisExpression();
		}

		// Token: 0x06002633 RID: 9779 RVA: 0x0003EEA1 File Offset: 0x0003DEA1
		public _IBaseExpression CreateBaseExpression()
		{
			return new BaseExpression();
		}

		// Token: 0x06002634 RID: 9780 RVA: 0x0005E9E4 File Offset: 0x0005D9E4
		public _ILiteralExpression CreateIntegerLiteralExpression(long lValue, TypeClass constantType, bool bNegative)
		{
			return new IntegerLiteralExpression(lValue, constantType, bNegative);
		}

		// Token: 0x06002635 RID: 9781 RVA: 0x0005E9EE File Offset: 0x0005D9EE
		public _ILiteralExpression CreateBasedIntegerLiteralExpression(long lValue, TypeClass constantType, int nbase, bool bNegative)
		{
			return new BasedIntegerLiteralExpression(lValue, constantType, nbase, bNegative);
		}

		// Token: 0x06002636 RID: 9782 RVA: 0x0005E9FA File Offset: 0x0005D9FA
		public _ILiteralExpression CreateStringLiteralExpression(string stValue, TypeClass constantType)
		{
			return new StringLiteralExpression(stValue, constantType);
		}

		// Token: 0x06002637 RID: 9783 RVA: 0x0005EA03 File Offset: 0x0005DA03
		public _ILiteralExpression CreateStringLiteralExpression(string stValue, TypeClass constantType, StringEncoding stringEncoding)
		{
			return new StringLiteralExpression(stValue, constantType, stringEncoding);
		}

		// Token: 0x06002638 RID: 9784 RVA: 0x0005EA0D File Offset: 0x0005DA0D
		public _ILiteralExpression CreateFloatLiteralExpression(double dValue, TypeClass constantType)
		{
			return new FloatLiteralExpression(dValue, constantType);
		}

		// Token: 0x06002639 RID: 9785 RVA: 0x0003EEB7 File Offset: 0x0003DEB7
		public _ITypeExpression CreateTypeExpression(ICompiledType cType)
		{
			return new TypeExpression(cType);
		}

		// Token: 0x0600263A RID: 9786 RVA: 0x0003EEB7 File Offset: 0x0003DEB7
		public _ITypeExpression CreateTypeExpression(ICompiledType2 type)
		{
			return new TypeExpression(type);
		}

		// Token: 0x0600263B RID: 9787 RVA: 0x0003EEC6 File Offset: 0x0003DEC6
		public _IAddressExpression CreateAddressExpression(IDirectVariable dirvar)
		{
			return new AddressExpression(dirvar);
		}

		// Token: 0x0600263C RID: 9788 RVA: 0x0003EED7 File Offset: 0x0003DED7
		public _IVariableExpression CreateVariableExpression(string stName)
		{
			return new VariableExpression(stName);
		}

		// Token: 0x0600263D RID: 9789 RVA: 0x0005EA18 File Offset: 0x0005DA18
		public _IIndexAccessExpression CreateIndexAccessExpression(_IExpression expBase, _IExpression[] expAccesses)
		{
			IndexAccessExpression indexAccessExpression = new IndexAccessExpression();
			indexAccessExpression._Var = expBase;
			if (expAccesses != null)
			{
				foreach (_IExpression expAcc in expAccesses)
				{
					indexAccessExpression.AddAccess(expAcc);
				}
			}
			return indexAccessExpression;
		}

		// Token: 0x0600263E RID: 9790 RVA: 0x0005EA51 File Offset: 0x0005DA51
		public _ICompoAccessExpression CreateCompoAccessExpression(_IExpression expLeft, _IExpression expRight)
		{
			return new CompoAccessExpression
			{
				_Left = expLeft,
				_Right = expRight
			};
		}

		// Token: 0x0600263F RID: 9791 RVA: 0x0003EF35 File Offset: 0x0003DF35
		public _IDeRefAccessExpression CreateDeRefAccessExpression(_IExpression exp)
		{
			return new DeRefAccessExpression(exp);
		}

		// Token: 0x06002640 RID: 9792 RVA: 0x0003EF8C File Offset: 0x0003DF8C
		public _IGlobalScopeExpression CreateGlobalScopeExpression(_IExpression exp)
		{
			return new GlobalScopeExpression(exp);
		}

		// Token: 0x06002641 RID: 9793 RVA: 0x0003EFAC File Offset: 0x0003DFAC
		public _ISystemScopeExpression CreateSystemScopeExpression(_IExpression exp)
		{
			return new SystemScopeExpression(exp);
		}

		// Token: 0x06002642 RID: 9794 RVA: 0x0003EFBD File Offset: 0x0003DFBD
		public _IPoolScopeExpression CreatePoolScopeExpression(_IExpression exp)
		{
			return new PoolScopeExpression(exp);
		}

		// Token: 0x06002643 RID: 9795 RVA: 0x0005EA66 File Offset: 0x0005DA66
		public _ICurrentTaskExpression CreateCurrentTaskExpression(_IExpression exp)
		{
			return new CurrentTaskExpression(exp);
		}

		// Token: 0x06002644 RID: 9796 RVA: 0x0003EFEA File Offset: 0x0003DFEA
		public _INullExpression CreateNullExpression()
		{
			return new NullExpression();
		}

		// Token: 0x06002645 RID: 9797 RVA: 0x0003EFF9 File Offset: 0x0003DFF9
		public _INullStatement CreateNullStatement()
		{
			return new NullStatement();
		}

		// Token: 0x06002646 RID: 9798 RVA: 0x0005EA6E File Offset: 0x0005DA6E
		public _IHasValueExpression CreateHasValueExpression(string stDefine, string stValue)
		{
			return new HasValueExpression
			{
				Define = stDefine,
				DefineValue = stValue
			};
		}

		// Token: 0x06002647 RID: 9799 RVA: 0x0005EA83 File Offset: 0x0005DA83
		public _IHasConstantValueExpression CreateHasConstantValueExpression(_IExpression Constant, _IExpression Value, Operator opComparison)
		{
			return new HasConstantValueExpression
			{
				_Constant = Constant,
				_ConstantValue = Value,
				_OpComparison = opComparison
			};
		}

		// Token: 0x06002648 RID: 9800 RVA: 0x0005EA9F File Offset: 0x0005DA9F
		public _IHasConstantTypeExpression CreateHasConstantTypeExpression(_IExpression Constant, bool bConstantTypeReplaced)
		{
			return new HasConstantTypeExpression
			{
				_Constant = Constant,
				_ConstantTypeReplaced = bConstantTypeReplaced
			};
		}

		// Token: 0x06002649 RID: 9801 RVA: 0x0005EAB4 File Offset: 0x0005DAB4
		public _IPragmaOperatorExpression CreatePragmaOperatorExpression(PragmaOperator op, _IExpression[] Operands)
		{
			PragmaOperatorExpression pragmaOperatorExpression = new PragmaOperatorExpression(op);
			foreach (_IExpression exp in Operands)
			{
				pragmaOperatorExpression.AddOperand(exp);
			}
			return pragmaOperatorExpression;
		}

		// Token: 0x0600264A RID: 9802 RVA: 0x0003F1D8 File Offset: 0x0003E1D8
		public _IPragmaAssertion CreatePragmaAssertion(_IExpression Condition, string ErrorOutput)
		{
			return new PragmaAssertion(Condition, ErrorOutput);
		}

		// Token: 0x0600264B RID: 9803 RVA: 0x0003F1F2 File Offset: 0x0003E1F2
		public _IPragmaElseIf CreatePragmaElseIf(_IPragmaExpression expCondition, _IStatement stControlled)
		{
			return new PragmaElseIf(expCondition, stControlled);
		}

		// Token: 0x0600264C RID: 9804 RVA: 0x0005EAE4 File Offset: 0x0005DAE4
		public _IDefineStatement CreateDefineStatement(bool bDefine, string stIdent, string stValue)
		{
			return new DefineStatement
			{
				Define = bDefine,
				Ident = stIdent,
				Value = stValue
			};
		}

		// Token: 0x0600264D RID: 9805 RVA: 0x0005EB00 File Offset: 0x0005DB00
		public _IArrayInitialization CreateArrayInitialisation(IList<_IExpression> initexprs)
		{
			ArrayInitialisation arrayInitialisation = new ArrayInitialisation();
			foreach (_IExpression exp in initexprs)
			{
				arrayInitialisation.AddInitValue(exp);
			}
			return arrayInitialisation;
		}

		// Token: 0x0600264E RID: 9806 RVA: 0x0005EB50 File Offset: 0x0005DB50
		public _IStructureInitialization CreateStructureInitialisation(IList<_IAssignmentExpression> explist)
		{
			StructureInitialisation structureInitialisation = new StructureInitialisation();
			foreach (_IAssignmentExpression assign in explist)
			{
				structureInitialisation.AddInitValue(assign);
			}
			return structureInitialisation;
		}

		// Token: 0x0600264F RID: 9807 RVA: 0x0005EBA0 File Offset: 0x0005DBA0
		public _IMultipleIndexInitialization CreateMultipleIndexInitialisation(_IExpression expValue, _IExpression expNumber)
		{
			return new MultipleIndexInitialisation
			{
				_Value = expValue,
				_Number = expNumber
			};
		}

		// Token: 0x06002650 RID: 9808 RVA: 0x0005EBB5 File Offset: 0x0005DBB5
		public _IDefineReference CreateDefineReference(string stDefine)
		{
			return new DefineReference
			{
				Define = stDefine
			};
		}

		// Token: 0x06002651 RID: 9809 RVA: 0x0005EBC3 File Offset: 0x0005DBC3
		public _IProjectDefinedExpression CreateProjectDefinedExpression(_IDefineReference defineReference)
		{
			return new ProjectDefinedExpression
			{
				DefineReference = defineReference
			};
		}

		// Token: 0x06002652 RID: 9810 RVA: 0x0005EBD1 File Offset: 0x0005DBD1
		public _IVariableReference CreateVariableReference(_IExpression instancePath)
		{
			return new VariableReference
			{
				InstancePath = instancePath
			};
		}

		// Token: 0x06002653 RID: 9811 RVA: 0x0005EBDF File Offset: 0x0005DBDF
		public _ITypeReference CreateTypeReference(_IExpression instancePath)
		{
			return new TypeReference
			{
				InstancePath = instancePath
			};
		}

		// Token: 0x06002654 RID: 9812 RVA: 0x0005EBED File Offset: 0x0005DBED
		public _IPouReference CreatePouReference(_IExpression instancePath)
		{
			return new PouReference
			{
				InstancePath = instancePath
			};
		}

		// Token: 0x06002655 RID: 9813 RVA: 0x0005EBFB File Offset: 0x0005DBFB
		public _ITaskReference CreateTaskReference(string stTask)
		{
			return new TaskReference
			{
				TaskName = stTask
			};
		}

		// Token: 0x06002656 RID: 9814 RVA: 0x0005EC09 File Offset: 0x0005DC09
		public _IResourceReference CreateResourceReference(string stResource)
		{
			return new ResourceReference
			{
				ResourceName = stResource
			};
		}

		// Token: 0x06002657 RID: 9815 RVA: 0x0005EC17 File Offset: 0x0005DC17
		public _IDefinedExpression CreateDefinedExpression(_IItemReference itref)
		{
			return new DefinedExpression
			{
				ItemReference = itref
			};
		}

		// Token: 0x06002658 RID: 9816 RVA: 0x0005EC25 File Offset: 0x0005DC25
		public _IXRefExpression CreateXRefExpression(_IItemReference itref, _IItemReference itrefFrom)
		{
			return new XRefExpression
			{
				XRef = itref,
				XRefFrom = itrefFrom
			};
		}

		// Token: 0x06002659 RID: 9817 RVA: 0x0005EC3A File Offset: 0x0005DC3A
		public _ICompilerVersionExpression CreateCompilerVersionExpression(Version versionToTest, Operator opComparison)
		{
			return new CompilerVersionExpression(versionToTest, opComparison);
		}

		// Token: 0x0600265A RID: 9818 RVA: 0x0005EC44 File Offset: 0x0005DC44
		public _IPragmaIfStatement CreatePragmaIfStatement(_IExpression expCond, _IStatement ifthen, _IStatement ifelse, _IPragmaElseIf[] elsifs)
		{
			PragmaIfStatement pragmaIfStatement = new PragmaIfStatement(expCond);
			pragmaIfStatement.IfThen = ifthen;
			pragmaIfStatement.IfElse = ifelse;
			if (elsifs != null)
			{
				foreach (_IPragmaElseIf elseIf in elsifs)
				{
					pragmaIfStatement.AddElseIf(elseIf);
				}
			}
			return pragmaIfStatement;
		}

		// Token: 0x0600265B RID: 9819 RVA: 0x0005EC87 File Offset: 0x0005DC87
		public _IHasCompatibleTypeExpression CreateHasCompatibleTypeExpression(_IVariableReference varref, ICompiledType type)
		{
			return new HasCompatibleTypeExpression
			{
				Variable = varref,
				ReferencedType = type
			};
		}

		// Token: 0x0600265C RID: 9820 RVA: 0x0005EC9C File Offset: 0x0005DC9C
		public _IHasTypeExpression CreateHasTypeExpression(_IVariableReference varref, ICompiledType type)
		{
			return new HasTypeExpression
			{
				Variable = varref,
				ReferencedType = type
			};
		}

		// Token: 0x0600265D RID: 9821 RVA: 0x0005ECB1 File Offset: 0x0005DCB1
		public _IIsEnumTypeExpression CreateIsEnumTypeExpression(ICompiledType type)
		{
			return new IsEnumTypeExpression
			{
				ReferencedType = type
			};
		}

		// Token: 0x0600265E RID: 9822 RVA: 0x0005ECBF File Offset: 0x0005DCBF
		public _IHasAttributeExpression CreateHasAttributeExpression(_IItemReference itref, string stAttribute)
		{
			return new HasAttributeExpression
			{
				ItemReference = itref,
				Attribute = stAttribute
			};
		}

		// Token: 0x0600265F RID: 9823 RVA: 0x0005ECD4 File Offset: 0x0005DCD4
		public _IRuntimeVersionExpression CreateRuntimeVersionExpression(Version v, Operator test)
		{
			return new RuntimeVersionExpression(v, test);
		}

		// Token: 0x06002660 RID: 9824 RVA: 0x0005ECDD File Offset: 0x0005DCDD
		public _IBreakPointStatement CreateBreakPointStatement(long bpPosition, long successorPosition)
		{
			return new BreakPointStatement
			{
				BPPosition = bpPosition,
				SuccessorPosition = successorPosition
			};
		}

		// Token: 0x06002661 RID: 9825 RVA: 0x0003EF00 File Offset: 0x0003DF00
		public _IPartialAccessExpression CreatePartialAccessExpression(_IExpression left, DirectVariableSize partSize, int partOffset)
		{
			return new PartialAccessExpression(left)
			{
				PartSize = partSize,
				PartOffset = partOffset
			};
		}

		// Token: 0x06002662 RID: 9826 RVA: 0x0005C8CF File Offset: 0x0005B8CF
		public _IImplicitConversionExpression CreateImplicitConversionExpression(TypeClass from, TypeClass to, _IExpression expression)
		{
			return new ImplicitConversionExpression(from, to)
			{
				_Exp = expression
			};
		}
	}
}
