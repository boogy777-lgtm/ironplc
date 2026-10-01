using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x0200020E RID: 526
	internal class SystemScopeExpression_Green : Expression_Green, _ISystemScopeExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, ISystemScopeExpression
	{
		// Token: 0x06002389 RID: 9097 RVA: 0x0005BC7B File Offset: 0x0005AC7B
		internal SystemScopeExpression_Green(_IExpression expBase)
		{
			this.m_expBase = expBase;
		}

		// Token: 0x0600238A RID: 9098 RVA: 0x00012CA4 File Offset: 0x00011CA4
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x0600238B RID: 9099 RVA: 0x0005BC8A File Offset: 0x0005AC8A
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			this.m_expBase.AcceptVisitor(visitor);
		}

		// Token: 0x17000A02 RID: 2562
		// (get) Token: 0x0600238C RID: 9100 RVA: 0x0005BC98 File Offset: 0x0005AC98
		public IExpression Base
		{
			get
			{
				return this._Base;
			}
		}

		// Token: 0x17000A03 RID: 2563
		// (get) Token: 0x0600238D RID: 9101 RVA: 0x0005BCA0 File Offset: 0x0005ACA0
		// (set) Token: 0x0600238E RID: 9102 RVA: 0x0005A471 File Offset: 0x00059471
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

		// Token: 0x040006CC RID: 1740
		private readonly _IExpression m_expBase;
	}
}
