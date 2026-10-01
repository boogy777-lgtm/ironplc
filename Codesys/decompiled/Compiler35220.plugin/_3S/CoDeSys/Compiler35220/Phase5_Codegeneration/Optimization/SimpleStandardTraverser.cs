using System;
using System.Collections.Generic;
using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x0200028C RID: 652
	public sealed class SimpleStandardTraverser : IExprementVisitor352000, IExprementVisitor351900, IExprementVisitor351800, IExprementVisitor351500, IExprementVisitor351400, IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor
	{
		// Token: 0x060028E3 RID: 10467 RVA: 0x0008F044 File Offset: 0x0008D244
		public SimpleStandardTraverser(IExprementVisitor352000 expCalledForAll)
		{
			this.\u0001 = expCalledForAll;
		}

		// Token: 0x060028E4 RID: 10468 RVA: 0x0008F054 File Offset: 0x0008D254
		public void visit(_IAddressExpression address)
		{
			this.\u0001.visit(address);
		}

		// Token: 0x060028E5 RID: 10469 RVA: 0x0008F064 File Offset: 0x0008D264
		public void visit(_IVariableExpression variable)
		{
			this.\u0001.visit(variable);
		}

		// Token: 0x060028E6 RID: 10470 RVA: 0x0008F074 File Offset: 0x0008D274
		public void visit(_ICompiledPOU cpou)
		{
			if (cpou.GetFlag(CompiledPOUFlags.ContainsDirVarAccess))
			{
				cpou.GetParseTree().Accept(this);
			}
			this.\u0001.visit(cpou);
		}

		// Token: 0x060028E7 RID: 10471 RVA: 0x0008F09C File Offset: 0x0008D29C
		public void visit(_IWhileStatement whilst)
		{
			whilst._Condition.Accept(this);
			whilst._Controlled.Accept(this);
			this.\u0001.visit(whilst);
		}

		// Token: 0x060028E8 RID: 10472 RVA: 0x0008F0C4 File Offset: 0x0008D2C4
		public void visit(_IRepeatStatement repeat)
		{
			repeat._Condition.Accept(this);
			repeat._Controlled.Accept(this);
			this.\u0001.visit(repeat);
		}

		// Token: 0x060028E9 RID: 10473 RVA: 0x0008F0EC File Offset: 0x0008D2EC
		public void visit(_IForStatement forloop)
		{
			forloop._CounterStart.Accept(this);
			forloop._UpperBound.Accept(this);
			if (forloop.By != null)
			{
				forloop._By.Accept(this);
			}
			forloop._Controlled.Accept(this);
			this.\u0001.visit(forloop);
		}

		// Token: 0x060028EA RID: 10474 RVA: 0x0008F140 File Offset: 0x0008D340
		public void visit(_ISequenceStatement seq)
		{
			IList<_IStatement> statementList = seq._StatementList;
			for (int i = 0; i < statementList.Count; i++)
			{
				statementList[i].Accept(this);
			}
			this.\u0001.visit(seq);
		}

		// Token: 0x060028EB RID: 10475 RVA: 0x0008F180 File Offset: 0x0008D380
		public void visit(_IIfStatement ifst)
		{
			ifst._Condition.Accept(this);
			ifst._IfThen.Accept(this);
			foreach (_IElseIf ielseIf in ifst._ElseIf)
			{
				ielseIf._Condition.Accept(this);
				ielseIf._Controlled.Accept(this);
			}
			_IStatement ifElse = ifst._IfElse;
			if (ifElse != null)
			{
				ifElse.Accept(this);
			}
			this.\u0001.visit(ifst);
		}

		// Token: 0x060028EC RID: 10476 RVA: 0x0008F214 File Offset: 0x0008D414
		public void visit(_IExpressionStatement expstat)
		{
			expstat._Expr.Accept(this);
			this.\u0001.visit(expstat);
		}

		// Token: 0x060028ED RID: 10477 RVA: 0x0008F230 File Offset: 0x0008D430
		public void visit(_IAssignmentExpression assign)
		{
			assign._LValue.Accept(this);
			assign._RValue.Accept(this);
			this.\u0001.visit(assign);
		}

		// Token: 0x060028EE RID: 10478 RVA: 0x0008F258 File Offset: 0x0008D458
		public void visit(_ICallExpression call)
		{
			call._Callee.Accept(this);
			if (call._Condition != null)
			{
				call._Condition.Accept(this);
			}
			IList<_IExpression> paramExpressions = call.ParamExpressions;
			for (int i = 0; i < paramExpressions.Count; i++)
			{
				_IExpression iexpression = paramExpressions[i];
				if (iexpression != null)
				{
					iexpression.Accept(this);
				}
			}
			IList<_IExpression> outputExpressions = call.OutputExpressions;
			for (int j = 0; j < outputExpressions.Count; j++)
			{
				outputExpressions[j].Accept(this);
			}
			this.\u0001.visit(call);
		}

		// Token: 0x060028EF RID: 10479 RVA: 0x0008F2E8 File Offset: 0x0008D4E8
		public void visit(_IOperatorExpression op)
		{
			IList<_IExpression> operandsList = op._OperandsList;
			for (int i = 0; i < operandsList.Count; i++)
			{
				operandsList[i].Accept(this);
			}
			this.\u0001.visit(op);
		}

		// Token: 0x060028F0 RID: 10480 RVA: 0x0008F328 File Offset: 0x0008D528
		public void visit(_ICastExpression castexp)
		{
			castexp.BaseExpression.Accept(this);
			this.\u0001.visit(castexp);
		}

		// Token: 0x060028F1 RID: 10481 RVA: 0x0008F344 File Offset: 0x0008D544
		public void visit(_INewExpression typeref)
		{
			typeref._Count.Accept(this);
			if (typeref._FBInitParams != null)
			{
				foreach (_IAssignmentExpression iassignmentExpression in typeref._FBInitParams.OfType<_IAssignmentExpression>())
				{
					iassignmentExpression.Accept(this);
				}
			}
			this.\u0001.visit(typeref);
		}

		// Token: 0x060028F2 RID: 10482 RVA: 0x0008F3B4 File Offset: 0x0008D5B4
		public void visit(_ITypeExpression typeexp)
		{
			this.\u0001.visit(typeexp);
		}

		// Token: 0x060028F3 RID: 10483 RVA: 0x0008F3C4 File Offset: 0x0008D5C4
		public void visit(_IConversionExpression conv)
		{
			conv._Exp.Accept(this);
			this.\u0001.visit(conv);
		}

		// Token: 0x060028F4 RID: 10484 RVA: 0x0008F3E0 File Offset: 0x0008D5E0
		public void visit(_IIndexAccessExpression indexaccess)
		{
			for (int i = 0; i < indexaccess.NumAccesses; i++)
			{
				indexaccess.GetAccess(i).Accept(this);
			}
			indexaccess._Var.Accept(this);
			this.\u0001.visit(indexaccess);
		}

		// Token: 0x060028F5 RID: 10485 RVA: 0x0008F424 File Offset: 0x0008D624
		public void visit(_ILiteralExpression literal)
		{
			this.\u0001.visit(literal);
		}

		// Token: 0x060028F6 RID: 10486 RVA: 0x0008F434 File Offset: 0x0008D634
		public void visit(_ICompoAccessExpression compo)
		{
			compo._Left.Accept(this);
			this.\u0001.visit(compo);
		}

		// Token: 0x060028F7 RID: 10487 RVA: 0x0008F450 File Offset: 0x0008D650
		public void visit(_IDeRefAccessExpression deref)
		{
			deref._Base.Accept(this);
			this.\u0001.visit(deref);
		}

		// Token: 0x060028F8 RID: 10488 RVA: 0x0008F46C File Offset: 0x0008D66C
		public void visit(_ICopyScopeExpression copyexp)
		{
			copyexp._Base.Accept(this);
			this.\u0001.visit(copyexp);
		}

		// Token: 0x060028F9 RID: 10489 RVA: 0x0008F488 File Offset: 0x0008D688
		public void visit(_IGlobalScopeExpression globexp)
		{
			globexp._Base.Accept(this);
			this.\u0001.visit(globexp);
		}

		// Token: 0x060028FA RID: 10490 RVA: 0x0008F4A4 File Offset: 0x0008D6A4
		public void visit(_ISystemScopeExpression systemscope)
		{
			systemscope._Base.Accept(this);
			this.\u0001.visit(systemscope);
		}

		// Token: 0x060028FB RID: 10491 RVA: 0x0008F4C0 File Offset: 0x0008D6C0
		public void visit(_IPoolScopeExpression poolscope)
		{
			poolscope._Base.Accept(this);
			this.\u0001.visit(poolscope);
		}

		// Token: 0x060028FC RID: 10492 RVA: 0x0008F4DC File Offset: 0x0008D6DC
		public void visit(_INamespaceAccessExpression namespaceaccess)
		{
			namespaceaccess._Namespace.Accept(this);
			_IExpression access = namespaceaccess._Access;
			if (access != null)
			{
				access.Accept(this);
			}
			IExprementVisitorNoTraversion351500 exprementVisitorNoTraversion = this.\u0001 as IExprementVisitorNoTraversion351500;
			if (exprementVisitorNoTraversion == null)
			{
				return;
			}
			exprementVisitorNoTraversion.visit(namespaceaccess);
		}

		// Token: 0x060028FD RID: 10493 RVA: 0x0008F514 File Offset: 0x0008D714
		public void visit(_ICurrentTaskExpression currentTask)
		{
			currentTask._Base.Accept(this);
			this.\u0001.visit(currentTask);
		}

		// Token: 0x060028FE RID: 10494 RVA: 0x0008F530 File Offset: 0x0008D730
		public void visit(_ICaseRangeExpression caserange)
		{
			caserange._Low.Accept(this);
			caserange._High.Accept(this);
			this.\u0001.visit(caserange);
		}

		// Token: 0x060028FF RID: 10495 RVA: 0x0008F558 File Offset: 0x0008D758
		public void visit(_ICaseLabelStatement caselabel)
		{
			foreach (_IExpression iexpression in caselabel._cases)
			{
				iexpression.Accept(this);
			}
			this.\u0001.visit(caselabel);
		}

		// Token: 0x06002900 RID: 10496 RVA: 0x0008F5B0 File Offset: 0x0008D7B0
		public void visit(_ICaseStatement casest)
		{
			casest._Switch.Accept(this);
			foreach (_ICase icase in casest._Cases)
			{
				icase._Label.Accept(this);
				icase._Controlled.Accept(this);
			}
			if (casest._Else != null)
			{
				casest._Else.Accept(this);
			}
			this.\u0001.visit(casest);
		}

		// Token: 0x06002901 RID: 10497 RVA: 0x0008F638 File Offset: 0x0008D838
		public void visit(_IExitStatement exit)
		{
			this.\u0001.visit(exit);
		}

		// Token: 0x06002902 RID: 10498 RVA: 0x0008F648 File Offset: 0x0008D848
		public void visit(_IContinueStatement cont)
		{
			this.\u0001.visit(cont);
		}

		// Token: 0x06002903 RID: 10499 RVA: 0x0008F658 File Offset: 0x0008D858
		public void visit(_IThisExpression thisexp)
		{
			this.\u0001.visit(thisexp);
		}

		// Token: 0x06002904 RID: 10500 RVA: 0x0008F668 File Offset: 0x0008D868
		public void visit(_IBaseExpression baseexp)
		{
			this.\u0001.visit(baseexp);
		}

		// Token: 0x06002905 RID: 10501 RVA: 0x0008F678 File Offset: 0x0008D878
		public void visit(_IEmptyStatement empty)
		{
			this.\u0001.visit(empty);
		}

		// Token: 0x06002906 RID: 10502 RVA: 0x0008F688 File Offset: 0x0008D888
		public void visit(_IReturnStatement returnst)
		{
			if (returnst._Condition != null)
			{
				returnst._Condition.Accept(this);
			}
			this.\u0001.visit(returnst);
		}

		// Token: 0x06002907 RID: 10503 RVA: 0x0008F6AC File Offset: 0x0008D8AC
		public void visit(_IJumpStatement gotost)
		{
			if (gotost._Condition != null)
			{
				gotost._Condition.Accept(this);
			}
			this.\u0001.visit(gotost);
		}

		// Token: 0x06002908 RID: 10504 RVA: 0x0008F6D0 File Offset: 0x0008D8D0
		public void visit(_ILabelStatement label)
		{
			this.\u0001.visit(label);
		}

		// Token: 0x06002909 RID: 10505 RVA: 0x0008F6E0 File Offset: 0x0008D8E0
		public void visit(_ICommentStatement comment)
		{
			this.\u0001.visit(comment);
		}

		// Token: 0x0600290A RID: 10506 RVA: 0x0008F6F0 File Offset: 0x0008D8F0
		public void visit(_IPragmaStatement pragma)
		{
			this.\u0001.visit(pragma);
		}

		// Token: 0x0600290B RID: 10507 RVA: 0x0008F700 File Offset: 0x0008D900
		public void visit(_IErrorExpression errorexp)
		{
			this.\u0001.visit(errorexp);
		}

		// Token: 0x0600290C RID: 10508 RVA: 0x0008F710 File Offset: 0x0008D910
		public void visit(_IErrorStatement errorst)
		{
			this.\u0001.visit(errorst);
		}

		// Token: 0x0600290D RID: 10509 RVA: 0x0008F720 File Offset: 0x0008D920
		public void visit(_INullExpression errorexp)
		{
			this.\u0001.visit(errorexp);
		}

		// Token: 0x0600290E RID: 10510 RVA: 0x0008F730 File Offset: 0x0008D930
		public void visit(_INullStatement errorst)
		{
			this.\u0001.visit(errorst);
		}

		// Token: 0x0600290F RID: 10511 RVA: 0x0008F740 File Offset: 0x0008D940
		public void visit(_IQualifiedNameExpression qne)
		{
			this.\u0001.visit(qne);
		}

		// Token: 0x06002910 RID: 10512 RVA: 0x0008F750 File Offset: 0x0008D950
		public void visit(_IVariableDeclarationStatement vds)
		{
		}

		// Token: 0x06002911 RID: 10513 RVA: 0x0008F754 File Offset: 0x0008D954
		public void visit(_IVariableDeclarationListStatement vdls)
		{
		}

		// Token: 0x06002912 RID: 10514 RVA: 0x0008F758 File Offset: 0x0008D958
		public void visit(_IPOUDeclarationStatement pds)
		{
		}

		// Token: 0x06002913 RID: 10515 RVA: 0x0008F75C File Offset: 0x0008D95C
		public void visit(_ITypeDeclarationStatement tds)
		{
		}

		// Token: 0x06002914 RID: 10516 RVA: 0x0008F760 File Offset: 0x0008D960
		public void visit(_IEnumDeclarationStatement eds)
		{
		}

		// Token: 0x06002915 RID: 10517 RVA: 0x0008F764 File Offset: 0x0008D964
		public void visit(_IEnumDeclarationListStatement eds)
		{
		}

		// Token: 0x06002916 RID: 10518 RVA: 0x0008F768 File Offset: 0x0008D968
		public void visit(_IMultipleIndexInitialization errorst)
		{
			errorst._Value.Accept(this);
			errorst._Number.Accept(this);
			this.\u0001.visit(errorst);
		}

		// Token: 0x06002917 RID: 10519 RVA: 0x0008F790 File Offset: 0x0008D990
		public void visit(_IArrayInitialization errorexp)
		{
			foreach (_IExpression iexpression in errorexp._InitValues)
			{
				iexpression.Accept(this);
			}
			this.\u0001.visit(errorexp);
		}

		// Token: 0x06002918 RID: 10520 RVA: 0x0008F7E8 File Offset: 0x0008D9E8
		public void visit(_IStructureInitialization errorst)
		{
			foreach (_IAssignmentExpression iassignmentExpression in errorst._CompoInits)
			{
				iassignmentExpression.Accept(this);
			}
			this.\u0001.visit(errorst);
		}

		// Token: 0x06002919 RID: 10521 RVA: 0x0008F840 File Offset: 0x0008DA40
		public void visit(_IDefineReference defref)
		{
		}

		// Token: 0x0600291A RID: 10522 RVA: 0x0008F844 File Offset: 0x0008DA44
		public void visit(_IVariableReference varref)
		{
		}

		// Token: 0x0600291B RID: 10523 RVA: 0x0008F848 File Offset: 0x0008DA48
		public void visit(_ITypeReference typeref)
		{
		}

		// Token: 0x0600291C RID: 10524 RVA: 0x0008F84C File Offset: 0x0008DA4C
		public void visit(_IPouReference pouref)
		{
		}

		// Token: 0x0600291D RID: 10525 RVA: 0x0008F850 File Offset: 0x0008DA50
		public void visit(_ITaskReference taskref)
		{
		}

		// Token: 0x0600291E RID: 10526 RVA: 0x0008F854 File Offset: 0x0008DA54
		public void visit(_IResourceReference resref)
		{
		}

		// Token: 0x0600291F RID: 10527 RVA: 0x0008F858 File Offset: 0x0008DA58
		public void visit(_IDefinedExpression defexp)
		{
		}

		// Token: 0x06002920 RID: 10528 RVA: 0x0008F85C File Offset: 0x0008DA5C
		public void visit(_IPragmaOperatorExpression popexp)
		{
		}

		// Token: 0x06002921 RID: 10529 RVA: 0x0008F860 File Offset: 0x0008DA60
		public void visit(_IPragmaIfStatement pifst)
		{
		}

		// Token: 0x06002922 RID: 10530 RVA: 0x0008F864 File Offset: 0x0008DA64
		public void visit(_IBreakPointStatement bpstate)
		{
		}

		// Token: 0x06002923 RID: 10531 RVA: 0x0008F868 File Offset: 0x0008DA68
		public void visit(_IDefineStatement defstate)
		{
		}

		// Token: 0x06002924 RID: 10532 RVA: 0x0008F86C File Offset: 0x0008DA6C
		public void visit(_IXRefExpression xref)
		{
		}

		// Token: 0x06002925 RID: 10533 RVA: 0x0008F870 File Offset: 0x0008DA70
		public void visit(_IHasTypeExpression hastype)
		{
		}

		// Token: 0x06002926 RID: 10534 RVA: 0x0008F874 File Offset: 0x0008DA74
		public void visit(_IIsEnumTypeExpression isenumtype)
		{
		}

		// Token: 0x06002927 RID: 10535 RVA: 0x0008F878 File Offset: 0x0008DA78
		public void visit(_IHasAttributeExpression hasattribute)
		{
		}

		// Token: 0x06002928 RID: 10536 RVA: 0x0008F87C File Offset: 0x0008DA7C
		public void visit(_IHasValueExpression hasvalue)
		{
		}

		// Token: 0x06002929 RID: 10537 RVA: 0x0008F880 File Offset: 0x0008DA80
		public void visit(_IHasConstantValueExpression hasvalue)
		{
		}

		// Token: 0x0600292A RID: 10538 RVA: 0x0008F884 File Offset: 0x0008DA84
		public void visit(_IHasConstantTypeExpression hasConstantTypeExpression)
		{
		}

		// Token: 0x0600292B RID: 10539 RVA: 0x0008F888 File Offset: 0x0008DA88
		public void visit(_IPragmaAssertion assertion)
		{
		}

		// Token: 0x0600292C RID: 10540 RVA: 0x0008F88C File Offset: 0x0008DA8C
		public void visit(_ICompilerVersionExpression compiversionexp)
		{
		}

		// Token: 0x0600292D RID: 10541 RVA: 0x0008F890 File Offset: 0x0008DA90
		public void visit(_IRuntimeVersionExpression runtimeversionexp)
		{
		}

		// Token: 0x0600292E RID: 10542 RVA: 0x0008F894 File Offset: 0x0008DA94
		public void visit(_IPartialAccessExpression partialAccessExpression)
		{
			partialAccessExpression._Left.Accept(this);
			this.\u0001.visit(partialAccessExpression);
		}

		// Token: 0x0600292F RID: 10543 RVA: 0x0008F8B0 File Offset: 0x0008DAB0
		public void visit(_IProjectDefinedExpression projectDefinedExpression)
		{
			_IDefineReference defineReference = projectDefinedExpression.DefineReference;
			if (defineReference != null)
			{
				defineReference.Accept(this);
			}
			this.\u0001.visit(projectDefinedExpression);
		}

		// Token: 0x0400078D RID: 1933
		private readonly IExprementVisitor352000 \u0001;
	}
}
