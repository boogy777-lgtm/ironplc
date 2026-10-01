using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x02000210 RID: 528
	internal class CurrentTaskExpression_Green : Expression_Green, _ICurrentTaskExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, ICurrentTaskExpression
	{
		// Token: 0x06002396 RID: 9110 RVA: 0x0005BCFF File Offset: 0x0005ACFF
		internal CurrentTaskExpression_Green(_IExpression expBase)
		{
			this.m_expBase = expBase;
		}

		// Token: 0x06002397 RID: 9111 RVA: 0x0000E0E4 File Offset: 0x0000D0E4
		public override void Accept(IExprementVisitor visitor)
		{
			if (visitor is IExprementVisitor351300)
			{
				(visitor as IExprementVisitor351300).visit(this);
			}
		}

		// Token: 0x06002398 RID: 9112 RVA: 0x0005BD0E File Offset: 0x0005AD0E
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			this.m_expBase.AcceptVisitor(visitor);
		}

		// Token: 0x17000A06 RID: 2566
		// (get) Token: 0x06002399 RID: 9113 RVA: 0x0005BD1C File Offset: 0x0005AD1C
		public IExpression Base
		{
			get
			{
				return this._Base;
			}
		}

		// Token: 0x17000A07 RID: 2567
		// (get) Token: 0x0600239A RID: 9114 RVA: 0x0005BD24 File Offset: 0x0005AD24
		// (set) Token: 0x0600239B RID: 9115 RVA: 0x0005A471 File Offset: 0x00059471
		public _IExpression _Base
		{
			get
			{
				if (this.m_expBase == null)
				{
					return new NullExpression_Green();
				}
				return this.m_expBase;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x040006CE RID: 1742
		private readonly _IExpression m_expBase;
	}
}
