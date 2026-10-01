using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001DE RID: 478
	internal class WhileStatement_Green : Statement_Green, _IWhileStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IWhileStatement
	{
		// Token: 0x060021AC RID: 8620 RVA: 0x0005A5D5 File Offset: 0x000595D5
		internal WhileStatement_Green(_IExpression expCond, _IStatement stateControlled)
		{
			this.m_expCond = expCond;
			this.m_stateControlled = stateControlled;
		}

		// Token: 0x17000940 RID: 2368
		// (get) Token: 0x060021AD RID: 8621 RVA: 0x0005A5EB File Offset: 0x000595EB
		public IExpression Condition
		{
			get
			{
				return this._Condition;
			}
		}

		// Token: 0x17000941 RID: 2369
		// (get) Token: 0x060021AE RID: 8622 RVA: 0x0005A5F3 File Offset: 0x000595F3
		// (set) Token: 0x060021AF RID: 8623 RVA: 0x0005A609 File Offset: 0x00059609
		public _IExpression _Condition
		{
			get
			{
				if (this.m_expCond == null)
				{
					return new NullExpression_Green();
				}
				return this.m_expCond;
			}
			set
			{
				throw new NotSupportedException("no manipulation of green tree expressions");
			}
		}

		// Token: 0x17000942 RID: 2370
		// (get) Token: 0x060021B0 RID: 8624 RVA: 0x0005A615 File Offset: 0x00059615
		public IStatement Controlled
		{
			get
			{
				return this._Controlled;
			}
		}

		// Token: 0x17000943 RID: 2371
		// (get) Token: 0x060021B1 RID: 8625 RVA: 0x0005A61D File Offset: 0x0005961D
		// (set) Token: 0x060021B2 RID: 8626 RVA: 0x0005A609 File Offset: 0x00059609
		public _IStatement _Controlled
		{
			get
			{
				if (this.m_stateControlled == null)
				{
					return new NullStatement_Green();
				}
				return this.m_stateControlled;
			}
			set
			{
				throw new NotSupportedException("no manipulation of green tree expressions");
			}
		}

		// Token: 0x060021B3 RID: 8627 RVA: 0x000172C2 File Offset: 0x000162C2
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060021B4 RID: 8628 RVA: 0x000172D4 File Offset: 0x000162D4
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x04000674 RID: 1652
		private readonly _IExpression m_expCond;

		// Token: 0x04000675 RID: 1653
		private readonly _IStatement m_stateControlled;
	}
}
