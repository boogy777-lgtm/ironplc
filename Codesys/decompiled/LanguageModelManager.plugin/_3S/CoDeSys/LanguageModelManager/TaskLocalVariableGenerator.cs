using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000103 RID: 259
	internal class TaskLocalVariableGenerator : IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor
	{
		// Token: 0x060012D9 RID: 4825 RVA: 0x00034CC2 File Offset: 0x00033CC2
		private TaskLocalVariableGenerator(_IScope scope, int iTaskIndex)
		{
			this.Scope = scope;
			this.TaskIndex = iTaskIndex;
			this.TaskLocalVariableFound = false;
			this.Unexpected = false;
		}

		// Token: 0x060012DA RID: 4826 RVA: 0x00034CF4 File Offset: 0x00033CF4
		public static string GenerateTaskLocalVariable(_IScope scope, _IExpression exp, int iTaskIndex)
		{
			TaskLocalVariableGenerator taskLocalVariableGenerator = new TaskLocalVariableGenerator(scope, iTaskIndex);
			exp.Accept(taskLocalVariableGenerator);
			string result = null;
			if (taskLocalVariableGenerator.TaskLocalVariableFound && !taskLocalVariableGenerator.Unexpected)
			{
				result = taskLocalVariableGenerator.GeneratedAccess();
			}
			return result;
		}

		// Token: 0x17000558 RID: 1368
		// (get) Token: 0x060012DB RID: 4827 RVA: 0x00034D2A File Offset: 0x00033D2A
		// (set) Token: 0x060012DC RID: 4828 RVA: 0x00034D32 File Offset: 0x00033D32
		private _IScope Scope { get; set; }

		// Token: 0x17000559 RID: 1369
		// (get) Token: 0x060012DD RID: 4829 RVA: 0x00034D3B File Offset: 0x00033D3B
		// (set) Token: 0x060012DE RID: 4830 RVA: 0x00034D43 File Offset: 0x00033D43
		private bool TaskLocalVariableFound { get; set; }

		// Token: 0x1700055A RID: 1370
		// (get) Token: 0x060012DF RID: 4831 RVA: 0x00034D4C File Offset: 0x00033D4C
		// (set) Token: 0x060012E0 RID: 4832 RVA: 0x00034D54 File Offset: 0x00033D54
		private int TaskIndex { get; set; }

		// Token: 0x1700055B RID: 1371
		// (get) Token: 0x060012E1 RID: 4833 RVA: 0x00034D5D File Offset: 0x00033D5D
		// (set) Token: 0x060012E2 RID: 4834 RVA: 0x00034D65 File Offset: 0x00033D65
		private bool Unexpected { get; set; }

		// Token: 0x060012E3 RID: 4835 RVA: 0x00034D6E File Offset: 0x00033D6E
		private string GeneratedAccess()
		{
			return this._strbNewExpression.ToString();
		}

		// Token: 0x060012E4 RID: 4836 RVA: 0x00034D7C File Offset: 0x00033D7C
		public void visit(_IVariableExpression variable)
		{
			_ISignature isignature = variable.GetSignatureEx(this.Scope) as _ISignature;
			_IVariable ivariable = variable.GetVariable(this.Scope) as _IVariable;
			if (ivariable != null && isignature != null && ivariable.GetFlag(VarFlag.TaskLocal))
			{
				string taskLocalVariablesGVLName = IdentifierConstants.GetTaskLocalVariablesGVLName(isignature);
				string taskLocalVariablesArrayName = IdentifierConstants.GetTaskLocalVariablesArrayName(isignature);
				string taskLocalVariablesComponentName = IdentifierConstants.GetTaskLocalVariablesComponentName(ivariable);
				this._strbNewExpression.AppendFormat("{0}.{1}[{2}].{3}", new object[]
				{
					taskLocalVariablesGVLName,
					taskLocalVariablesArrayName,
					this.TaskIndex,
					taskLocalVariablesComponentName
				});
				this.TaskLocalVariableFound = true;
				return;
			}
			this._strbNewExpression.Append(variable.Name);
		}

		// Token: 0x060012E5 RID: 4837 RVA: 0x00034E26 File Offset: 0x00033E26
		public void visit(_ICompoAccessExpression compo)
		{
			compo._Left.Accept(this);
			this._strbNewExpression.Append(".");
			compo._Right.Accept(this);
		}

		// Token: 0x060012E6 RID: 4838 RVA: 0x00034E54 File Offset: 0x00033E54
		public void visit(_IIndexAccessExpression indexaccess)
		{
			indexaccess._Var.Accept(this);
			this._strbNewExpression.Append("[");
			for (int i = 0; i < indexaccess.NumAccesses; i++)
			{
				indexaccess.GetAccess(i).Accept(this);
				if (i < indexaccess.NumAccesses - 1)
				{
					this._strbNewExpression.Append(", ");
				}
			}
			this._strbNewExpression.Append("]");
		}

		// Token: 0x060012E7 RID: 4839 RVA: 0x00034EC9 File Offset: 0x00033EC9
		public void visit(_IDeRefAccessExpression deref)
		{
			deref._Base.Accept(this);
			this._strbNewExpression.Append("^");
		}

		// Token: 0x060012E8 RID: 4840 RVA: 0x00034EE8 File Offset: 0x00033EE8
		public void visit(_ILiteralExpression literal)
		{
			this._strbNewExpression.Append(literal.ToString());
		}

		// Token: 0x060012E9 RID: 4841 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_ICompiledPOU cpou)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012EA RID: 4842 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IRepeatStatement repeat)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012EB RID: 4843 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IExitStatement exit)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012EC RID: 4844 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_ISequenceStatement seq)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012ED RID: 4845 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IIfStatement ifst)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012EE RID: 4846 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IJumpStatement gotost)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012EF RID: 4847 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_ICommentStatement comment)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012F0 RID: 4848 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IExpressionStatement expstat)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012F1 RID: 4849 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IOperatorExpression op)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012F2 RID: 4850 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IThisExpression thisexp)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012F3 RID: 4851 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_ICopyScopeExpression copyexp)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012F4 RID: 4852 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_ISystemScopeExpression systemscope)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012F5 RID: 4853 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_ICaseRangeExpression caserange)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012F6 RID: 4854 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_ICaseStatement casest)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012F7 RID: 4855 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IErrorStatement errorst)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012F8 RID: 4856 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_INullStatement errorst)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012F9 RID: 4857 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IVariableDeclarationStatement vds)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012FA RID: 4858 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IPOUDeclarationStatement pds)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012FB RID: 4859 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IEnumDeclarationStatement eds)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012FC RID: 4860 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IMultipleIndexInitialization errorst)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012FD RID: 4861 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IStructureInitialization errorst)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012FE RID: 4862 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IVariableReference varref)
		{
			this.Unexpected = true;
		}

		// Token: 0x060012FF RID: 4863 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IPouReference pouref)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001300 RID: 4864 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IResourceReference resref)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001301 RID: 4865 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IPragmaOperatorExpression popexp)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001302 RID: 4866 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IBreakPointStatement bpstate)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001303 RID: 4867 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IXRefExpression xref)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001304 RID: 4868 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IIsEnumTypeExpression isenumtype)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001305 RID: 4869 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IHasValueExpression hasvalue)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001306 RID: 4870 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IPragmaAssertion assertion)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001307 RID: 4871 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_ICastExpression castexp)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001308 RID: 4872 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_ITypeExpression typeexp)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001309 RID: 4873 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_INewExpression typeref)
		{
			this.Unexpected = true;
		}

		// Token: 0x0600130A RID: 4874 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_ICompilerVersionExpression compiversionexp)
		{
			this.Unexpected = true;
		}

		// Token: 0x0600130B RID: 4875 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IHasConstantValueExpression hasvalue)
		{
			this.Unexpected = true;
		}

		// Token: 0x0600130C RID: 4876 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IHasAttributeExpression hasattribute)
		{
			this.Unexpected = true;
		}

		// Token: 0x0600130D RID: 4877 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IHasTypeExpression hastype)
		{
			this.Unexpected = true;
		}

		// Token: 0x0600130E RID: 4878 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IDefineStatement defstate)
		{
			this.Unexpected = true;
		}

		// Token: 0x0600130F RID: 4879 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IPragmaIfStatement pifst)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001310 RID: 4880 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IDefinedExpression defexp)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001311 RID: 4881 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_ITaskReference taskref)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001312 RID: 4882 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_ITypeReference typeref)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001313 RID: 4883 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IDefineReference defref)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001314 RID: 4884 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IArrayInitialization errorexp)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001315 RID: 4885 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IEnumDeclarationListStatement eds)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001316 RID: 4886 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_ITypeDeclarationStatement tds)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001317 RID: 4887 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IVariableDeclarationListStatement vdls)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001318 RID: 4888 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IQualifiedNameExpression qne)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001319 RID: 4889 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_INullExpression errorexp)
		{
			this.Unexpected = true;
		}

		// Token: 0x0600131A RID: 4890 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IErrorExpression errorexp)
		{
			this.Unexpected = true;
		}

		// Token: 0x0600131B RID: 4891 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_ICaseLabelStatement caselabel)
		{
			this.Unexpected = true;
		}

		// Token: 0x0600131C RID: 4892 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IEmptyStatement empty)
		{
			this.Unexpected = true;
		}

		// Token: 0x0600131D RID: 4893 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IGlobalScopeExpression globexp)
		{
			this.Unexpected = true;
		}

		// Token: 0x0600131E RID: 4894 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IAddressExpression address)
		{
			this.Unexpected = true;
		}

		// Token: 0x0600131F RID: 4895 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IBaseExpression baseexp)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001320 RID: 4896 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IConversionExpression conv)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001321 RID: 4897 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_ICallExpression call)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001322 RID: 4898 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IPragmaStatement pragma)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001323 RID: 4899 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_ILabelStatement label)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001324 RID: 4900 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IReturnStatement returnst)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001325 RID: 4901 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IAssignmentExpression assign)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001326 RID: 4902 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IContinueStatement cont)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001327 RID: 4903 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IForStatement forloop)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001328 RID: 4904 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IWhileStatement whilst)
		{
			this.Unexpected = true;
		}

		// Token: 0x06001329 RID: 4905 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_IPoolScopeExpression poolscope)
		{
			this.Unexpected = true;
		}

		// Token: 0x0600132A RID: 4906 RVA: 0x00034EFC File Offset: 0x00033EFC
		public void visit(_ICurrentTaskExpression currentTask)
		{
			this.Unexpected = true;
		}

		// Token: 0x04000462 RID: 1122
		private readonly LStringBuilder _strbNewExpression = new LStringBuilder();
	}
}
