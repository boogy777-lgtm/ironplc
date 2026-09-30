using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000FF RID: 255
	internal class CheckForTaskLocalVariable : IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor
	{
		// Token: 0x0600126D RID: 4717 RVA: 0x00034B1C File Offset: 0x00033B1C
		private CheckForTaskLocalVariable(IScope5 scope)
		{
			this.TaskLocalVariableFound = false;
			this.Scope = scope;
			this.Unexpected = false;
		}

		// Token: 0x17000552 RID: 1362
		// (get) Token: 0x0600126E RID: 4718 RVA: 0x00034B39 File Offset: 0x00033B39
		// (set) Token: 0x0600126F RID: 4719 RVA: 0x00034B41 File Offset: 0x00033B41
		private bool TaskLocalVariableFound { get; set; }

		// Token: 0x17000553 RID: 1363
		// (get) Token: 0x06001270 RID: 4720 RVA: 0x00034B4A File Offset: 0x00033B4A
		// (set) Token: 0x06001271 RID: 4721 RVA: 0x00034B52 File Offset: 0x00033B52
		private bool Unexpected { get; set; }

		// Token: 0x17000554 RID: 1364
		// (get) Token: 0x06001272 RID: 4722 RVA: 0x00034B5B File Offset: 0x00033B5B
		// (set) Token: 0x06001273 RID: 4723 RVA: 0x00034B63 File Offset: 0x00033B63
		private IScope5 Scope { get; set; }

		// Token: 0x06001274 RID: 4724 RVA: 0x00034B6C File Offset: 0x00033B6C
		public static bool ContainsTaskLocalAccess(_IExpression exp, IScope5 scope)
		{
			CheckForTaskLocalVariable checkForTaskLocalVariable = new CheckForTaskLocalVariable(scope);
			exp.Accept(checkForTaskLocalVariable);
			return checkForTaskLocalVariable.TaskLocalVariableFound && !checkForTaskLocalVariable.Unexpected;
		}

		// Token: 0x06001275 RID: 4725 RVA: 0x00034B9C File Offset: 0x00033B9C
		public void visit(_IVariableExpression variable)
		{
			variable.GetSignatureEx(this.Scope);
			_IVariable ivariable = variable.GetVariable(this.Scope) as _IVariable;
			if (ivariable != null && ivariable.GetFlag(VarFlag.TaskLocal))
			{
				this.TaskLocalVariableFound = true;
			}
		}

		// Token: 0x06001276 RID: 4726 RVA: 0x00034BE3 File Offset: 0x00033BE3
		public void visit(_ICompoAccessExpression compo)
		{
			compo._Left.Accept(this);
			compo._Right.Accept(this);
		}

		// Token: 0x06001277 RID: 4727 RVA: 0x00034C00 File Offset: 0x00033C00
		public void visit(_IIndexAccessExpression indexaccess)
		{
			indexaccess._Var.Accept(this);
			for (int i = 0; i < indexaccess.NumAccesses; i++)
			{
				indexaccess.GetAccess(i).Accept(this);
			}
		}

		// Token: 0x06001278 RID: 4728 RVA: 0x00034C37 File Offset: 0x00033C37
		public void visit(_IDeRefAccessExpression deref)
		{
			deref._Base.Accept(this);
		}

		// Token: 0x06001279 RID: 4729 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_ILiteralExpression literal)
		{
		}

		// Token: 0x0600127A RID: 4730 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_ICompiledPOU cpou)
		{
			this.Unexpected = true;
		}

		// Token: 0x0600127B RID: 4731 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IRepeatStatement repeat)
		{
			this.Unexpected = true;
		}

		// Token: 0x0600127C RID: 4732 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IExitStatement exit)
		{
			this.Unexpected = true;
		}

		// Token: 0x0600127D RID: 4733 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_ISequenceStatement seq)
		{
			this.Unexpected = true;
		}

		// Token: 0x0600127E RID: 4734 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IIfStatement ifst)
		{
			this.Unexpected = true;
		}

		// Token: 0x0600127F RID: 4735 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IJumpStatement gotost)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001280 RID: 4736 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_ICommentStatement comment)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001281 RID: 4737 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IExpressionStatement expstat)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001282 RID: 4738 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IOperatorExpression op)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001283 RID: 4739 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IThisExpression thisexp)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001284 RID: 4740 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_ICopyScopeExpression copyexp)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001285 RID: 4741 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_ISystemScopeExpression systemscope)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001286 RID: 4742 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_ICaseRangeExpression caserange)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001287 RID: 4743 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_ICaseStatement casest)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001288 RID: 4744 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IErrorStatement errorst)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001289 RID: 4745 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_INullStatement errorst)
		{
			this.Unexpected = true;
		}

		// Token: 0x0600128A RID: 4746 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IVariableDeclarationStatement vds)
		{
			this.Unexpected = true;
		}

		// Token: 0x0600128B RID: 4747 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IPOUDeclarationStatement pds)
		{
			this.Unexpected = true;
		}

		// Token: 0x0600128C RID: 4748 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IEnumDeclarationStatement eds)
		{
			this.Unexpected = true;
		}

		// Token: 0x0600128D RID: 4749 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IMultipleIndexInitialization errorst)
		{
			this.Unexpected = true;
		}

		// Token: 0x0600128E RID: 4750 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IStructureInitialization errorst)
		{
			this.Unexpected = true;
		}

		// Token: 0x0600128F RID: 4751 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IVariableReference varref)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001290 RID: 4752 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IPouReference pouref)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001291 RID: 4753 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IResourceReference resref)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001292 RID: 4754 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IPragmaOperatorExpression popexp)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001293 RID: 4755 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IBreakPointStatement bpstate)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001294 RID: 4756 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IXRefExpression xref)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001295 RID: 4757 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IIsEnumTypeExpression isenumtype)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001296 RID: 4758 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IHasValueExpression hasvalue)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001297 RID: 4759 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IPragmaAssertion assertion)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001298 RID: 4760 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_ICastExpression castexp)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001299 RID: 4761 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_ITypeExpression typeexp)
		{
			this.Unexpected = true;
		}

		// Token: 0x0600129A RID: 4762 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_INewExpression typeref)
		{
			this.Unexpected = true;
		}

		// Token: 0x0600129B RID: 4763 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_ICompilerVersionExpression compiversionexp)
		{
			this.Unexpected = true;
		}

		// Token: 0x0600129C RID: 4764 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IHasConstantValueExpression hasvalue)
		{
			this.Unexpected = true;
		}

		// Token: 0x0600129D RID: 4765 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IHasAttributeExpression hasattribute)
		{
			this.Unexpected = true;
		}

		// Token: 0x0600129E RID: 4766 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IHasTypeExpression hastype)
		{
			this.Unexpected = true;
		}

		// Token: 0x0600129F RID: 4767 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IDefineStatement defstate)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012A0 RID: 4768 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IPragmaIfStatement pifst)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012A1 RID: 4769 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IDefinedExpression defexp)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012A2 RID: 4770 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_ITaskReference taskref)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012A3 RID: 4771 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_ITypeReference typeref)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012A4 RID: 4772 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IDefineReference defref)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012A5 RID: 4773 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IArrayInitialization errorexp)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012A6 RID: 4774 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IEnumDeclarationListStatement eds)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012A7 RID: 4775 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_ITypeDeclarationStatement tds)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012A8 RID: 4776 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IVariableDeclarationListStatement vdls)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012A9 RID: 4777 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IQualifiedNameExpression qne)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012AA RID: 4778 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_INullExpression errorexp)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012AB RID: 4779 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IErrorExpression errorexp)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012AC RID: 4780 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_ICaseLabelStatement caselabel)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012AD RID: 4781 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IEmptyStatement empty)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012AE RID: 4782 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IGlobalScopeExpression globexp)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012AF RID: 4783 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IAddressExpression address)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012B0 RID: 4784 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IBaseExpression baseexp)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012B1 RID: 4785 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IConversionExpression conv)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012B2 RID: 4786 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_ICallExpression call)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012B3 RID: 4787 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IPragmaStatement pragma)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012B4 RID: 4788 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_ILabelStatement label)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012B5 RID: 4789 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IReturnStatement returnst)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012B6 RID: 4790 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IAssignmentExpression assign)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012B7 RID: 4791 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IContinueStatement cont)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012B8 RID: 4792 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IForStatement forloop)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012B9 RID: 4793 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IWhileStatement whilst)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012BA RID: 4794 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_IPoolScopeExpression poolscope)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012BB RID: 4795 RVA: 0x00034C45 File Offset: 0x00033C45
		public void visit(_ICurrentTaskExpression currentTask)
		{
			this.Unexpected = true;
		}
	}
}
