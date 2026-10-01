using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x0200020F RID: 527
	internal class PoolScopeExpression_Green : Expression_Green, _IPoolScopeExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IPoolScopeExpression
	{
		// Token: 0x0600238F RID: 9103 RVA: 0x0005BCB6 File Offset: 0x0005ACB6
		internal PoolScopeExpression_Green(_IExpression expBase)
		{
			this.m_expBase = expBase;
		}

		// Token: 0x06002390 RID: 9104 RVA: 0x0005BCC5 File Offset: 0x0005ACC5
		internal PoolScopeExpression_Green(string stBaseName) : this(new VariableExpression_Green(stBaseName))
		{
		}

		// Token: 0x06002391 RID: 9105 RVA: 0x00011F71 File Offset: 0x00010F71
		public override void Accept(IExprementVisitor visitor)
		{
			if (visitor is IExprementVisitor3590)
			{
				(visitor as IExprementVisitor3590).visit(this);
			}
		}

		// Token: 0x06002392 RID: 9106 RVA: 0x0005BCD3 File Offset: 0x0005ACD3
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			this.m_expBase.AcceptVisitor(visitor);
		}

		// Token: 0x17000A04 RID: 2564
		// (get) Token: 0x06002393 RID: 9107 RVA: 0x0005BCE1 File Offset: 0x0005ACE1
		public IExpression Base
		{
			get
			{
				return this._Base;
			}
		}

		// Token: 0x17000A05 RID: 2565
		// (get) Token: 0x06002394 RID: 9108 RVA: 0x0005BCE9 File Offset: 0x0005ACE9
		// (set) Token: 0x06002395 RID: 9109 RVA: 0x0005A471 File Offset: 0x00059471
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

		// Token: 0x040006CD RID: 1741
		private readonly _IExpression m_expBase;
	}
}
