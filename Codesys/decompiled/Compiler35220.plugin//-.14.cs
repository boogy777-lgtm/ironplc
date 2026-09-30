using System;
using System.Collections.Generic;
using \u0019;
using \u001E;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0084
{
	// Token: 0x02000217 RID: 535
	internal sealed class \u0013 : IExprementVisitor352000, IExprementVisitor351900, IExprementVisitor351800, IExprementVisitor351500, IExprementVisitor351400, IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor, IExprementVisitor2
	{
		// Token: 0x0600237B RID: 9083 RVA: 0x0007A27C File Offset: 0x0007847C
		public \u0013(\u001E.\u000E \u000E\u0004)
		{
			this.\u0001 = \u000E\u0004;
		}

		// Token: 0x0600237C RID: 9084 RVA: 0x0007A298 File Offset: 0x00078498
		public void \u0001(_ICompiledPOU \u0002)
		{
			this.\u0001 = (\u0002.BreakpointList as _IBreakpointList);
			\u0002.GetParseTree().Accept(this);
			int[] array = new int[this.\u0001.Count];
			for (int i = 0; i < this.\u0001.Count; i++)
			{
				array[i] = i;
			}
			foreach (object obj in this.\u0001)
			{
				((_IBreakpoint)obj).AddSuccessors(array);
			}
			this.\u0001.FirstIndex = this.\u0001.Count - 1;
			\u0002.SetBreakpointList(this.\u0001);
		}

		// Token: 0x0600237D RID: 9085 RVA: 0x0007A35C File Offset: 0x0007855C
		public void \u0001(_ISequenceStatement \u0002)
		{
			IList<_IStatement> statementList = \u0002._StatementList;
			for (int i = statementList.Count - 1; i >= 0; i--)
			{
				_IStatement istatement = statementList[i];
				try
				{
					istatement.Accept(this);
				}
				catch (Exception)
				{
				}
			}
		}

		// Token: 0x0600237E RID: 9086 RVA: 0x0007A3A8 File Offset: 0x000785A8
		public void \u0001(_IWhileStatement \u0002)
		{
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x0600237F RID: 9087 RVA: 0x0007A3B8 File Offset: 0x000785B8
		public void \u0001(_IRepeatStatement \u0002)
		{
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x06002380 RID: 9088 RVA: 0x0007A3C8 File Offset: 0x000785C8
		public void \u0001(_IForStatement \u0002)
		{
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x06002381 RID: 9089 RVA: 0x0007A3D8 File Offset: 0x000785D8
		public void \u0001(_IExitStatement \u0002)
		{
		}

		// Token: 0x06002382 RID: 9090 RVA: 0x0007A3DC File Offset: 0x000785DC
		public void \u0001(_IContinueStatement \u0002)
		{
		}

		// Token: 0x06002383 RID: 9091 RVA: 0x0007A3E0 File Offset: 0x000785E0
		public void \u0001(_IAssignmentExpression \u0002)
		{
			\u0002._LValue.Accept(this);
			\u0002._RValue.Accept(this);
		}

		// Token: 0x06002384 RID: 9092 RVA: 0x0007A3FC File Offset: 0x000785FC
		public void \u0001(_IIfStatement \u0002)
		{
			if (\u0002.IfElse != null)
			{
				\u0002._IfElse.Accept(this);
			}
			IList<_IElseIf> elseIf = \u0002._ElseIf;
			if (elseIf != null)
			{
				for (int i = elseIf.Count - 1; i >= 0; i--)
				{
					elseIf[i]._Controlled.Accept(this);
				}
			}
			\u0002._IfThen.Accept(this);
			if (\u0002.GetFlag(StatementFlag.GenerateBP2))
			{
				_IBreakpoint ibreakpoint = this.\u0001.\u0001(\u0002) as _IBreakpoint;
				if (ibreakpoint == null)
				{
					return;
				}
				int num = this.\u0001.Add(ref ibreakpoint);
				this.\u0001.Push(num);
				\u0002._Condition.Accept(this);
				this.\u0001.Pop();
			}
		}

		// Token: 0x06002385 RID: 9093 RVA: 0x0007A4AC File Offset: 0x000786AC
		public void \u0001(_IReturnStatement \u0002)
		{
			_IBreakpoint ibreakpoint = this.\u0001.\u0001(\u0002) as _IBreakpoint;
			if (ibreakpoint == null)
			{
				return;
			}
			this.\u0001.Add(ref ibreakpoint);
			if (\u0002._Condition != null)
			{
				\u0002._Condition.Accept(this);
			}
		}

		// Token: 0x06002386 RID: 9094 RVA: 0x0007A4F4 File Offset: 0x000786F4
		public void \u0001(_IJumpStatement \u0002)
		{
		}

		// Token: 0x06002387 RID: 9095 RVA: 0x0007A4F8 File Offset: 0x000786F8
		public void \u0001(_ILabelStatement \u0002)
		{
		}

		// Token: 0x06002388 RID: 9096 RVA: 0x0007A4FC File Offset: 0x000786FC
		public void \u0001(_ICommentStatement \u0002)
		{
		}

		// Token: 0x06002389 RID: 9097 RVA: 0x0007A500 File Offset: 0x00078700
		public void \u0001(_IPragmaStatement \u0002)
		{
		}

		// Token: 0x0600238A RID: 9098 RVA: 0x0007A504 File Offset: 0x00078704
		public void \u0001(_IExpressionStatement \u0002)
		{
			_IBreakpoint ibreakpoint = this.\u0001.\u0001(\u0002) as _IBreakpoint;
			if (ibreakpoint == null)
			{
				return;
			}
			bool flag = \u0002.GetFlag(StatementFlag.GenerateBP2);
			if (flag)
			{
				int num = this.\u0001.Add(ref ibreakpoint);
				this.\u0001.Push(num);
			}
			\u0002._Expr.Accept(this);
			if (flag)
			{
				this.\u0001.Pop();
			}
		}

		// Token: 0x0600238B RID: 9099 RVA: 0x0007A568 File Offset: 0x00078768
		public void \u0001(_ICallExpression \u0002)
		{
			ICallExprInfo callInfo = \u0002.CallInfo;
			if (callInfo != null && callInfo.InstanceAssignment != null)
			{
				callInfo.InstanceAssignment.Accept(this);
			}
			if (\u0002._Condition != null)
			{
				\u0002._Condition.Accept(this);
			}
			foreach (_IExpression iexpression in \u0002.ParamExpressions)
			{
				if (iexpression != null)
				{
					iexpression.Accept(this);
				}
			}
			foreach (_IExpression iexpression2 in \u0002.OutputExpressions)
			{
				if (iexpression2 != null)
				{
					iexpression2.Accept(this);
				}
			}
			IBreakpoint breakpoint = \u0002.CallBreakpoint;
			if (breakpoint == null)
			{
				breakpoint = this.\u0001.\u0001(\u0002);
			}
			if (callInfo != null)
			{
				_IStepInPosition istepInPosition = \u0019.\u0003.\u0001(callInfo.IdCalledSignature, breakpoint);
				istepInPosition.KindOfCall = callInfo.KindOfCall;
				if (this.\u0001.Count > 0)
				{
					int nIndex = this.\u0001.Peek();
					(this.\u0001[nIndex] as _IBreakpoint).AddStepInSuccessor(istepInPosition);
				}
			}
		}

		// Token: 0x0600238C RID: 9100 RVA: 0x0007A698 File Offset: 0x00078898
		public void \u0001(_IOperatorExpression \u0002)
		{
			foreach (_IExpression iexpression in \u0002._OperandsList)
			{
				iexpression.Accept(this);
			}
		}

		// Token: 0x0600238D RID: 9101 RVA: 0x0007A6E4 File Offset: 0x000788E4
		public void \u0001(_IConversionExpression \u0002)
		{
			\u0002._Exp.Accept(this);
		}

		// Token: 0x0600238E RID: 9102 RVA: 0x0007A6F4 File Offset: 0x000788F4
		public void \u0001(_ICastExpression \u0002)
		{
		}

		// Token: 0x0600238F RID: 9103 RVA: 0x0007A6F8 File Offset: 0x000788F8
		public void \u0001(_INewExpression \u0002)
		{
			\u0002._Count.Accept(this);
			if (\u0002._FBInitParams != null)
			{
				foreach (IAssignmentExpression assignmentExpression in \u0002._FBInitParams)
				{
					((_IAssignmentExpression)assignmentExpression).Accept(this);
				}
			}
		}

		// Token: 0x06002390 RID: 9104 RVA: 0x0007A75C File Offset: 0x0007895C
		public void \u0001(_ITypeExpression \u0002)
		{
		}

		// Token: 0x06002391 RID: 9105 RVA: 0x0007A760 File Offset: 0x00078960
		public void \u0001(_IThisExpression \u0002)
		{
		}

		// Token: 0x06002392 RID: 9106 RVA: 0x0007A764 File Offset: 0x00078964
		public void \u0001(_IBaseExpression \u0002)
		{
		}

		// Token: 0x06002393 RID: 9107 RVA: 0x0007A768 File Offset: 0x00078968
		public void \u0001(_ILiteralExpression \u0002)
		{
		}

		// Token: 0x06002394 RID: 9108 RVA: 0x0007A76C File Offset: 0x0007896C
		public void \u0001(_IAddressExpression \u0002)
		{
		}

		// Token: 0x06002395 RID: 9109 RVA: 0x0007A770 File Offset: 0x00078970
		public void \u0001(_IQualifiedNameExpression \u0002)
		{
		}

		// Token: 0x06002396 RID: 9110 RVA: 0x0007A774 File Offset: 0x00078974
		public void \u0001(_IVariableExpression \u0002)
		{
			IVariableExprInfo varInfo = \u0002.VarInfo;
			if (varInfo != null && varInfo.IndexInfo != null && varInfo.IndexInfo.IndexExpression != null)
			{
				(varInfo.IndexInfo.IndexExpression as _IExpression).Accept(this);
			}
		}

		// Token: 0x06002397 RID: 9111 RVA: 0x0007A7B8 File Offset: 0x000789B8
		public void \u0001(_IIndexAccessExpression \u0002)
		{
			\u0002._Var.Accept(this);
			for (int i = 0; i < \u0002.NumAccesses; i++)
			{
				\u0002.GetAccess(i).Accept(this);
			}
		}

		// Token: 0x06002398 RID: 9112 RVA: 0x0007A7F0 File Offset: 0x000789F0
		public void \u0001(_ICompoAccessExpression \u0002)
		{
			\u0002._Left.Accept(this);
			\u0002._Right.Accept(this);
		}

		// Token: 0x06002399 RID: 9113 RVA: 0x0007A80C File Offset: 0x00078A0C
		public void \u0001(_IDeRefAccessExpression \u0002)
		{
			\u0002._Base.Accept(this);
		}

		// Token: 0x0600239A RID: 9114 RVA: 0x0007A81C File Offset: 0x00078A1C
		public void \u0001(_ICopyScopeExpression \u0002)
		{
			\u0002._Base.Accept(this);
		}

		// Token: 0x0600239B RID: 9115 RVA: 0x0007A82C File Offset: 0x00078A2C
		public void \u0001(_IGlobalScopeExpression \u0002)
		{
			\u0002._Base.Accept(this);
		}

		// Token: 0x0600239C RID: 9116 RVA: 0x0007A83C File Offset: 0x00078A3C
		public void \u0001(_ISystemScopeExpression \u0002)
		{
			\u0002._Base.Accept(this);
		}

		// Token: 0x0600239D RID: 9117 RVA: 0x0007A84C File Offset: 0x00078A4C
		public void \u0001(_IPoolScopeExpression \u0002)
		{
			\u0002._Base.Accept(this);
		}

		// Token: 0x0600239E RID: 9118 RVA: 0x0007A85C File Offset: 0x00078A5C
		public void \u0001(_INamespaceAccessExpression \u0002)
		{
			\u0002._Access.Accept(this);
		}

		// Token: 0x0600239F RID: 9119 RVA: 0x0007A86C File Offset: 0x00078A6C
		public void \u0001(_ICurrentTaskExpression \u0002)
		{
			\u0002._Base.Accept(this);
		}

		// Token: 0x060023A0 RID: 9120 RVA: 0x0007A87C File Offset: 0x00078A7C
		public void \u0001(_IEmptyStatement \u0002)
		{
		}

		// Token: 0x060023A1 RID: 9121 RVA: 0x0007A880 File Offset: 0x00078A80
		public void \u0001(_ICaseRangeExpression \u0002)
		{
			\u0002._Low.Accept(this);
			\u0002._High.Accept(this);
		}

		// Token: 0x060023A2 RID: 9122 RVA: 0x0007A89C File Offset: 0x00078A9C
		public void \u0001(_ICaseLabelStatement \u0002)
		{
			foreach (_IExpression iexpression in \u0002._cases)
			{
				iexpression.Accept(this);
			}
		}

		// Token: 0x060023A3 RID: 9123 RVA: 0x0007A8E8 File Offset: 0x00078AE8
		public void \u0001(_ICaseStatement \u0002)
		{
			if (\u0002._Else != null)
			{
				\u0002._Else.Accept(this);
			}
			IList<_ICase> cases = \u0002._Cases;
			for (int i = cases.Count - 1; i >= 0; i--)
			{
				cases[i]._Controlled.Accept(this);
			}
		}

		// Token: 0x060023A4 RID: 9124 RVA: 0x0007A938 File Offset: 0x00078B38
		public void \u0001(_IErrorExpression \u0002)
		{
		}

		// Token: 0x060023A5 RID: 9125 RVA: 0x0007A93C File Offset: 0x00078B3C
		public void \u0001(_IErrorStatement \u0002)
		{
		}

		// Token: 0x060023A6 RID: 9126 RVA: 0x0007A940 File Offset: 0x00078B40
		public void \u0001(_INullExpression \u0002)
		{
		}

		// Token: 0x060023A7 RID: 9127 RVA: 0x0007A944 File Offset: 0x00078B44
		public void \u0001(_INullStatement \u0002)
		{
		}

		// Token: 0x060023A8 RID: 9128 RVA: 0x0007A948 File Offset: 0x00078B48
		public void \u0001(_IVariableDeclarationStatement \u0002)
		{
		}

		// Token: 0x060023A9 RID: 9129 RVA: 0x0007A94C File Offset: 0x00078B4C
		public void \u0001(_IVariableDeclarationListStatement \u0002)
		{
		}

		// Token: 0x060023AA RID: 9130 RVA: 0x0007A950 File Offset: 0x00078B50
		public void \u0001(_IPOUDeclarationStatement \u0002)
		{
		}

		// Token: 0x060023AB RID: 9131 RVA: 0x0007A954 File Offset: 0x00078B54
		public void \u0001(_ITypeDeclarationStatement \u0002)
		{
		}

		// Token: 0x060023AC RID: 9132 RVA: 0x0007A958 File Offset: 0x00078B58
		public void \u0001(_IEnumDeclarationStatement \u0002)
		{
		}

		// Token: 0x060023AD RID: 9133 RVA: 0x0007A95C File Offset: 0x00078B5C
		public void \u0001(_IEnumDeclarationListStatement \u0002)
		{
		}

		// Token: 0x060023AE RID: 9134 RVA: 0x0007A960 File Offset: 0x00078B60
		public void \u0001(_IMultipleIndexInitialization \u0002)
		{
		}

		// Token: 0x060023AF RID: 9135 RVA: 0x0007A964 File Offset: 0x00078B64
		public void \u0001(_IArrayInitialization \u0002)
		{
		}

		// Token: 0x060023B0 RID: 9136 RVA: 0x0007A968 File Offset: 0x00078B68
		public void \u0001(_IStructureInitialization \u0002)
		{
		}

		// Token: 0x060023B1 RID: 9137 RVA: 0x0007A96C File Offset: 0x00078B6C
		public void \u0001(_IDefineReference \u0002)
		{
		}

		// Token: 0x060023B2 RID: 9138 RVA: 0x0007A970 File Offset: 0x00078B70
		public void \u0001(_IVariableReference \u0002)
		{
		}

		// Token: 0x060023B3 RID: 9139 RVA: 0x0007A974 File Offset: 0x00078B74
		public void \u0001(_ITypeReference \u0002)
		{
		}

		// Token: 0x060023B4 RID: 9140 RVA: 0x0007A978 File Offset: 0x00078B78
		public void \u0001(_IPouReference \u0002)
		{
		}

		// Token: 0x060023B5 RID: 9141 RVA: 0x0007A97C File Offset: 0x00078B7C
		public void \u0001(_ITaskReference \u0002)
		{
		}

		// Token: 0x060023B6 RID: 9142 RVA: 0x0007A980 File Offset: 0x00078B80
		public void \u0001(_IResourceReference \u0002)
		{
		}

		// Token: 0x060023B7 RID: 9143 RVA: 0x0007A984 File Offset: 0x00078B84
		public void \u0001(_IDefinedExpression \u0002)
		{
		}

		// Token: 0x060023B8 RID: 9144 RVA: 0x0007A988 File Offset: 0x00078B88
		public void \u0001(_IPragmaOperatorExpression \u0002)
		{
		}

		// Token: 0x060023B9 RID: 9145 RVA: 0x0007A98C File Offset: 0x00078B8C
		public void \u0001(_IPragmaIfStatement \u0002)
		{
		}

		// Token: 0x060023BA RID: 9146 RVA: 0x0007A990 File Offset: 0x00078B90
		public void \u0001(_IPragmaAssertion \u0002)
		{
		}

		// Token: 0x060023BB RID: 9147 RVA: 0x0007A994 File Offset: 0x00078B94
		public void \u0001(_ICompilerVersionExpression \u0002)
		{
		}

		// Token: 0x060023BC RID: 9148 RVA: 0x0007A998 File Offset: 0x00078B98
		public void \u0001(_IRuntimeVersionExpression \u0002)
		{
		}

		// Token: 0x060023BD RID: 9149 RVA: 0x0007A99C File Offset: 0x00078B9C
		public void \u0001(_IBreakPointStatement \u0002)
		{
		}

		// Token: 0x060023BE RID: 9150 RVA: 0x0007A9A0 File Offset: 0x00078BA0
		public void \u0001(_IDefineStatement \u0002)
		{
		}

		// Token: 0x060023BF RID: 9151 RVA: 0x0007A9A4 File Offset: 0x00078BA4
		public void \u0001(_IXRefExpression \u0002)
		{
		}

		// Token: 0x060023C0 RID: 9152 RVA: 0x0007A9A8 File Offset: 0x00078BA8
		public void \u0001(_IHasTypeExpression \u0002)
		{
		}

		// Token: 0x060023C1 RID: 9153 RVA: 0x0007A9AC File Offset: 0x00078BAC
		public void \u0001(_IIsEnumTypeExpression \u0002)
		{
		}

		// Token: 0x060023C2 RID: 9154 RVA: 0x0007A9B0 File Offset: 0x00078BB0
		public void \u0001(_IHasAttributeExpression \u0002)
		{
		}

		// Token: 0x060023C3 RID: 9155 RVA: 0x0007A9B4 File Offset: 0x00078BB4
		public void \u0001(_IHasValueExpression \u0002)
		{
		}

		// Token: 0x060023C4 RID: 9156 RVA: 0x0007A9B8 File Offset: 0x00078BB8
		public void \u0001(_IHasConstantValueExpression \u0002)
		{
		}

		// Token: 0x060023C5 RID: 9157 RVA: 0x0007A9BC File Offset: 0x00078BBC
		public void \u0001(_IHasConstantTypeExpression \u0002)
		{
		}

		// Token: 0x060023C6 RID: 9158 RVA: 0x0007A9C0 File Offset: 0x00078BC0
		public void \u0001(_ITryCatchStatement \u0002)
		{
		}

		// Token: 0x060023C7 RID: 9159 RVA: 0x0007A9C4 File Offset: 0x00078BC4
		public void \u0001(_IPartialAccessExpression \u0002)
		{
		}

		// Token: 0x060023C8 RID: 9160 RVA: 0x0007A9C8 File Offset: 0x00078BC8
		public void \u0001(_IProjectDefinedExpression \u0002)
		{
		}

		// Token: 0x04000639 RID: 1593
		private _IBreakpointList \u0001;

		// Token: 0x0400063A RID: 1594
		private readonly \u001E.\u000E \u0001;

		// Token: 0x0400063B RID: 1595
		private readonly LStack<int> \u0001 = new LStack<int>();
	}
}
