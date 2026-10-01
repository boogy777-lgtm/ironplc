using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001E8 RID: 488
	internal class ElseIf_Green : Exprement_Green, _IElseIf, _IExprement, IExprement3, IExprement2, IExprement, IElseIf2, IElseIf
	{
		// Token: 0x0600220B RID: 8715 RVA: 0x0005A8EE File Offset: 0x000598EE
		public ElseIf_Green(_IExpression expCondition, _IStatement stControlled)
		{
			this.m_expCondition = expCondition;
			this.m_stControlled = stControlled;
		}

		// Token: 0x1700096A RID: 2410
		// (get) Token: 0x0600220C RID: 8716 RVA: 0x0005A904 File Offset: 0x00059904
		public IExpression Condition
		{
			get
			{
				return this._Condition;
			}
		}

		// Token: 0x1700096B RID: 2411
		// (get) Token: 0x0600220D RID: 8717 RVA: 0x0005A90C File Offset: 0x0005990C
		// (set) Token: 0x0600220E RID: 8718 RVA: 0x0005A609 File Offset: 0x00059609
		public _IExpression _Condition
		{
			get
			{
				if (this.m_expCondition == null)
				{
					return new NullExpression_Green();
				}
				return this.m_expCondition;
			}
			set
			{
				throw new NotSupportedException("no manipulation of green tree expressions");
			}
		}

		// Token: 0x1700096C RID: 2412
		// (get) Token: 0x0600220F RID: 8719 RVA: 0x0005A922 File Offset: 0x00059922
		public IStatement Controlled
		{
			get
			{
				return this._Controlled;
			}
		}

		// Token: 0x1700096D RID: 2413
		// (get) Token: 0x06002210 RID: 8720 RVA: 0x0005A92A File Offset: 0x0005992A
		// (set) Token: 0x06002211 RID: 8721 RVA: 0x0005A609 File Offset: 0x00059609
		public _IStatement _Controlled
		{
			get
			{
				if (this.m_stControlled == null)
				{
					return new NullStatement_Green();
				}
				return this.m_stControlled;
			}
			set
			{
				throw new NotSupportedException("no manipulation of green tree expressions");
			}
		}

		// Token: 0x06002212 RID: 8722 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public override void Accept(IExprementVisitor visitor)
		{
		}

		// Token: 0x06002213 RID: 8723 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public override void AcceptVisitor(IExprVisitor visitor)
		{
		}

		// Token: 0x04000687 RID: 1671
		private readonly _IExpression m_expCondition;

		// Token: 0x04000688 RID: 1672
		private readonly _IStatement m_stControlled;
	}
}
