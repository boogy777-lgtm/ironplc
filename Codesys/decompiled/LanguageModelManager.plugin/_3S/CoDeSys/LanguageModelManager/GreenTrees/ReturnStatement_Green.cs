using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001EB RID: 491
	internal class ReturnStatement_Green : Statement_Green, _IReturnStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IReturnStatement
	{
		// Token: 0x06002239 RID: 8761 RVA: 0x0005AB2C File Offset: 0x00059B2C
		public ReturnStatement_Green(_IExpression expCondition)
		{
			this.m_expCondition = expCondition;
		}

		// Token: 0x0600223A RID: 8762 RVA: 0x000164AB File Offset: 0x000154AB
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x0600223B RID: 8763 RVA: 0x000164BD File Offset: 0x000154BD
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x17000980 RID: 2432
		// (get) Token: 0x0600223C RID: 8764 RVA: 0x0005AB3B File Offset: 0x00059B3B
		public IExpression Condition
		{
			get
			{
				return this._Condition;
			}
		}

		// Token: 0x17000981 RID: 2433
		// (get) Token: 0x0600223D RID: 8765 RVA: 0x0005AB43 File Offset: 0x00059B43
		// (set) Token: 0x0600223E RID: 8766 RVA: 0x0005A471 File Offset: 0x00059471
		public _IExpression _Condition
		{
			get
			{
				return this.m_expCondition;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x04000691 RID: 1681
		private readonly _IExpression m_expCondition;
	}
}
