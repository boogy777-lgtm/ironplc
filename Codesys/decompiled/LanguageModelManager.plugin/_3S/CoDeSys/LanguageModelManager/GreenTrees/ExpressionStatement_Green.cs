using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001F4 RID: 500
	internal class ExpressionStatement_Green : Statement_Green, _IExpressionStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IExpressionStatement
	{
		// Token: 0x06002267 RID: 8807 RVA: 0x0005AC90 File Offset: 0x00059C90
		internal ExpressionStatement_Green(_IExpression exp)
		{
			this._Expr = exp;
		}

		// Token: 0x1700098F RID: 2447
		// (get) Token: 0x06002268 RID: 8808 RVA: 0x0005AC9F File Offset: 0x00059C9F
		public IExpression Expr
		{
			get
			{
				return this._Expr;
			}
		}

		// Token: 0x17000990 RID: 2448
		// (get) Token: 0x06002269 RID: 8809 RVA: 0x0005ACA7 File Offset: 0x00059CA7
		// (set) Token: 0x0600226A RID: 8810 RVA: 0x0005ACBD File Offset: 0x00059CBD
		public _IExpression _Expr
		{
			get
			{
				if (this.m_exp == null)
				{
					return new NullExpression_Green();
				}
				return this.m_exp;
			}
			set
			{
				this.m_exp = value;
			}
		}

		// Token: 0x0600226B RID: 8811 RVA: 0x000150B1 File Offset: 0x000140B1
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x0600226C RID: 8812 RVA: 0x000150C3 File Offset: 0x000140C3
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x0400069D RID: 1693
		private _IExpression m_exp;
	}
}
