using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using \u0003;
using \u000E;
using \u000F;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;

namespace \u0014
{
	// Token: 0x020002C3 RID: 707
	internal sealed class \u0011 : global::\u0003.\u0011
	{
		// Token: 0x17000777 RID: 1911
		// (get) Token: 0x06002B0B RID: 11019 RVA: 0x00097BD0 File Offset: 0x00095DD0
		private global::\u000E.\u0011 Context { get; }

		// Token: 0x06002B0C RID: 11020 RVA: 0x00097BD8 File Offset: 0x00095DD8
		public \u0011(\u0081.\u0010 \u0096\u0007, ISpecificExpressionReplacer \u0018\u0006, global::\u000E.\u0011 \u0083\u0005) : base(\u0096\u0007, \u0018\u0006, \u0083\u0005)
		{
			this.Context = \u0083\u0005;
		}

		// Token: 0x06002B0D RID: 11021 RVA: 0x00097BEC File Offset: 0x00095DEC
		public override void \u0001(_IExpressionStatement \u0002)
		{
			_IAssignmentExpression iassignmentExpression = \u0002._Expr as _IAssignmentExpression;
			if (iassignmentExpression != null)
			{
				AssignmentInfo assignmentInfo;
				this.\u0001(new AssignmentInfo(iassignmentExpression), out assignmentInfo, false);
				iassignmentExpression._LValue = assignmentInfo.LValue;
				iassignmentExpression._RValue = assignmentInfo.RValue;
				return;
			}
			\u0002._Expr.Accept(this);
		}

		// Token: 0x06002B0E RID: 11022 RVA: 0x00097C40 File Offset: 0x00095E40
		public override void \u0001(_IOperatorExpression \u0002)
		{
			if (\u0002.Code == Operator.__IsValidRef || \u0002.Code == Operator.__RefAdr)
			{
				for (int i = 0; i < \u0002._OperandsList.Count; i++)
				{
					\u0002[i].Accept(this);
				}
				return;
			}
			base.\u0001(\u0002);
		}

		// Token: 0x06002B0F RID: 11023 RVA: 0x00097C94 File Offset: 0x00095E94
		public override void \u0001(_ICallExpression \u0002)
		{
			\u0002._Callee = this.\u0001(\u0002._Callee, true);
			if (\u0002._Condition != null)
			{
				\u0002._Condition = this.\u0001(\u0002._Condition, true);
			}
			CallParameterEnumerable callParameterEnumerable = \u0002.\u0001();
			int num = 0;
			foreach (AssignmentInfo u in callParameterEnumerable)
			{
				AssignmentInfo assignmentInfo;
				this.\u0001(u, out assignmentInfo, true);
				\u0002.SetActualParam(assignmentInfo.RValue, num);
				\u0002.SetFormalParam(assignmentInfo.LValue, num);
				num++;
			}
			IEnumerable<_IAssignmentExpression> outputAssigns = \u0002._OutputAssigns;
			num = 0;
			foreach (_IAssignmentExpression iassignmentExpression in outputAssigns)
			{
				if (iassignmentExpression != null)
				{
					_IExpression exp = this.\u0001(iassignmentExpression._LValue, true);
					\u0002.SetActualOutput(exp, num);
				}
				num++;
			}
		}

		// Token: 0x06002B10 RID: 11024 RVA: 0x00097D80 File Offset: 0x00095F80
		public override void \u0001(_IAssignmentExpression \u0002)
		{
			AssignmentInfo assignmentInfo;
			this.\u0001(new AssignmentInfo(\u0002), out assignmentInfo, false);
			\u0002._LValue = assignmentInfo.LValue;
			\u0002._RValue = assignmentInfo.RValue;
		}

		// Token: 0x06002B11 RID: 11025 RVA: 0x00097DB8 File Offset: 0x00095FB8
		private new void \u0001(_IExpression \u0002)
		{
			_ICopyScopeExpression icopyScopeExpression = \u0002 as _ICopyScopeExpression;
			if (icopyScopeExpression != null)
			{
				icopyScopeExpression._Base.Accept(this);
				return;
			}
			\u0002.Accept(this);
		}

		// Token: 0x06002B12 RID: 11026 RVA: 0x00097DE4 File Offset: 0x00095FE4
		private new bool \u0001(IExpression \u0002)
		{
			IOperatorExpression operatorExpression = \u0002 as IOperatorExpression;
			return operatorExpression != null && operatorExpression.Code == Operator.Adr;
		}

		// Token: 0x06002B13 RID: 11027 RVA: 0x00097E08 File Offset: 0x00096008
		private new void \u0001(AssignmentInfo \u0002, out AssignmentInfo \u0003, bool \u0004)
		{
			bool flag = global::\u0014.\u0010.\u0001(\u0002, \u0004, this.Context._Scope);
			bool flag2 = flag && \u0002.RValue.Type.Class == TypeClass.Reference;
			flag2 = (flag2 && !this.\u0001(\u0002.RValue));
			_IExpression iexpression = \u0002.LValue;
			_IExpression iexpression2 = \u0002.RValue;
			if (!flag)
			{
				iexpression = this.\u0001(\u0002.LValue, false);
			}
			else
			{
				this.\u0001(iexpression);
			}
			if (!flag2)
			{
				iexpression2 = this.\u0001(\u0002.RValue, true);
			}
			else
			{
				this.\u0001(iexpression2);
			}
			\u0003 = new AssignmentInfo(iexpression, iexpression2, \u0002.KindOf);
		}

		// Token: 0x04000828 RID: 2088
		[CompilerGenerated]
		private new readonly global::\u000E.\u0011 \u0001;
	}
}
