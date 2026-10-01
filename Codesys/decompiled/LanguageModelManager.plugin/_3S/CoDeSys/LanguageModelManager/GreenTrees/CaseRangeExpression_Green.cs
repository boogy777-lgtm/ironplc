using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001E0 RID: 480
	internal class CaseRangeExpression_Green : Expression_Green, _ICaseRangeExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, ICaseRangeExpression
	{
		// Token: 0x060021BE RID: 8638 RVA: 0x0005A685 File Offset: 0x00059685
		internal CaseRangeExpression_Green(_IExpression expLow, _IExpression expHigh)
		{
			this.m_expLow = expLow;
			this.m_expHigh = expHigh;
		}

		// Token: 0x17000948 RID: 2376
		// (get) Token: 0x060021BF RID: 8639 RVA: 0x0005A69B File Offset: 0x0005969B
		public IExpression Low
		{
			get
			{
				return this._Low;
			}
		}

		// Token: 0x17000949 RID: 2377
		// (get) Token: 0x060021C0 RID: 8640 RVA: 0x0005A6A3 File Offset: 0x000596A3
		// (set) Token: 0x060021C1 RID: 8641 RVA: 0x0005A609 File Offset: 0x00059609
		public _IExpression _Low
		{
			get
			{
				if (this.m_expLow == null)
				{
					return new NullExpression_Green();
				}
				return this.m_expLow;
			}
			set
			{
				throw new NotSupportedException("no manipulation of green tree expressions");
			}
		}

		// Token: 0x1700094A RID: 2378
		// (get) Token: 0x060021C2 RID: 8642 RVA: 0x0005A6B9 File Offset: 0x000596B9
		public IExpression High
		{
			get
			{
				return this._High;
			}
		}

		// Token: 0x1700094B RID: 2379
		// (get) Token: 0x060021C3 RID: 8643 RVA: 0x0005A6C1 File Offset: 0x000596C1
		// (set) Token: 0x060021C4 RID: 8644 RVA: 0x0005A609 File Offset: 0x00059609
		public _IExpression _High
		{
			get
			{
				if (this.m_expHigh == null)
				{
					return new NullExpression_Green();
				}
				return this.m_expHigh;
			}
			set
			{
				throw new NotSupportedException("no manipulation of green tree expressions");
			}
		}

		// Token: 0x060021C5 RID: 8645 RVA: 0x0000C4F5 File Offset: 0x0000B4F5
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060021C6 RID: 8646 RVA: 0x0000C507 File Offset: 0x0000B507
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x04000678 RID: 1656
		private readonly _IExpression m_expLow;

		// Token: 0x04000679 RID: 1657
		private readonly _IExpression m_expHigh;
	}
}
