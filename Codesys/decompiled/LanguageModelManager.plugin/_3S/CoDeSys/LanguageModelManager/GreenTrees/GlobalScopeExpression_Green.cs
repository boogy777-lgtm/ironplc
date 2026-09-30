using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x0200020D RID: 525
	internal class GlobalScopeExpression_Green : Expression_Green, _IGlobalScopeExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IGlobalScopeExpression
	{
		// Token: 0x06002383 RID: 9091 RVA: 0x0005BC4E File Offset: 0x0005AC4E
		public GlobalScopeExpression_Green(_IExpression expBase)
		{
			this.m_expBase = expBase;
		}

		// Token: 0x06002384 RID: 9092 RVA: 0x0000EA5B File Offset: 0x0000DA5B
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06002385 RID: 9093 RVA: 0x0000EA6D File Offset: 0x0000DA6D
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x17000A00 RID: 2560
		// (get) Token: 0x06002386 RID: 9094 RVA: 0x0005BC5D File Offset: 0x0005AC5D
		public IExpression Base
		{
			get
			{
				return this._Base;
			}
		}

		// Token: 0x17000A01 RID: 2561
		// (get) Token: 0x06002387 RID: 9095 RVA: 0x0005BC65 File Offset: 0x0005AC65
		// (set) Token: 0x06002388 RID: 9096 RVA: 0x0005A471 File Offset: 0x00059471
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

		// Token: 0x040006CB RID: 1739
		private readonly _IExpression m_expBase;
	}
}
