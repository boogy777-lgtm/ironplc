using System;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0082;

namespace \u0011
{
	// Token: 0x02000291 RID: 657
	internal sealed class \u000E : \u0011, IExprementVisitor3590, IExprementVisitor
	{
		// Token: 0x06002968 RID: 10600 RVA: 0x000910F0 File Offset: 0x0008F2F0
		public \u000E(IScope5 \u009B\u0002)
		{
			this.\u0001 = \u009B\u0002;
		}

		// Token: 0x06002969 RID: 10601 RVA: 0x00091100 File Offset: 0x0008F300
		private void \u0001(_IExpression \u0002)
		{
			try
			{
				if (!\u0002.IsConstant(this.\u0001, true))
				{
					_IVariable ivariable = \u0002.GetVariable(this.\u0001) as _IVariable;
					if (ivariable != null && TypeTable.IsBlock(ivariable.Type.Class) && ivariable.Initial != null)
					{
						ivariable._Initial.Accept(this);
					}
					else
					{
						base.\u0002(\u0002);
					}
				}
			}
			catch
			{
				base.\u0002(\u0002);
			}
		}

		// Token: 0x0600296A RID: 10602 RVA: 0x0009117C File Offset: 0x0008F37C
		public void \u0001(_IArrayInitialization \u0002)
		{
			foreach (_IExpression iexpression in \u0002._InitValues)
			{
				iexpression.Accept(this);
			}
		}

		// Token: 0x0600296B RID: 10603 RVA: 0x000911C8 File Offset: 0x0008F3C8
		public void \u0001(_IMultipleIndexInitialization \u0002)
		{
			int num;
			if (!\u0002._Number.Literal(this.\u0001, true).GetInt(out num))
			{
				base.\u0002(\u0002);
			}
			for (int i = 0; i < num; i++)
			{
				\u0002._Value.Accept(this);
			}
		}

		// Token: 0x0600296C RID: 10604 RVA: 0x00091210 File Offset: 0x0008F410
		public void \u0001(_IStructureInitialization \u0002)
		{
			_IUserdefType iuserdefType = \u0002.Type as _IUserdefType;
			if (iuserdefType == null)
			{
				base.\u0002(\u0002);
				return;
			}
			if (this.\u0001[iuserdefType.SignatureId] == null)
			{
				base.\u0002(\u0002);
			}
			foreach (_IAssignmentExpression iassignmentExpression in \u0002._CompoInits)
			{
				iassignmentExpression._RValue.Accept(this);
			}
		}

		// Token: 0x0600296D RID: 10605 RVA: 0x00091294 File Offset: 0x0008F494
		public void \u0001(_IOperatorExpression \u0002)
		{
			if (!Helper.\u0001(\u0002) && \u0002.Code != Operator.Adr)
			{
				this.\u0001(\u0002);
			}
		}

		// Token: 0x0600296E RID: 10606 RVA: 0x000912B0 File Offset: 0x0008F4B0
		public void \u0001(_IConversionExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x0600296F RID: 10607 RVA: 0x000912BC File Offset: 0x0008F4BC
		public void \u0001(_ILiteralExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06002970 RID: 10608 RVA: 0x000912C8 File Offset: 0x0008F4C8
		public void \u0001(_IVariableExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06002971 RID: 10609 RVA: 0x000912D4 File Offset: 0x0008F4D4
		public void \u0001(_IIndexAccessExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06002972 RID: 10610 RVA: 0x000912E0 File Offset: 0x0008F4E0
		public void \u0001(_ICompoAccessExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06002973 RID: 10611 RVA: 0x000912EC File Offset: 0x0008F4EC
		public void \u0001(_IGlobalScopeExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06002974 RID: 10612 RVA: 0x000912F8 File Offset: 0x0008F4F8
		public void \u0001(_IPoolScopeExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06002975 RID: 10613 RVA: 0x00091304 File Offset: 0x0008F504
		public void \u0001(_INamespaceAccessExpression \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x0400079A RID: 1946
		private readonly IScope5 \u0001;
	}
}
