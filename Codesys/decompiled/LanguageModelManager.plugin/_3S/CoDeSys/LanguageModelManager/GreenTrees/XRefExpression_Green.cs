using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x0200021C RID: 540
	internal class XRefExpression_Green : PragmaExpression_Green, _IXRefExpression, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression
	{
		// Token: 0x060023DA RID: 9178 RVA: 0x0005BE86 File Offset: 0x0005AE86
		public XRefExpression_Green(_IItemReference itref, _IItemReference itrefFrom)
		{
			this.m_itref = itref;
			this.m_itrefFrom = itrefFrom;
		}

		// Token: 0x17000A1A RID: 2586
		// (get) Token: 0x060023DB RID: 9179 RVA: 0x0005BE9C File Offset: 0x0005AE9C
		// (set) Token: 0x060023DC RID: 9180 RVA: 0x0005A471 File Offset: 0x00059471
		public _IExpression XRef
		{
			get
			{
				if (this.m_itref == null)
				{
					return new NullExpression_Green();
				}
				return this.m_itref;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x17000A1B RID: 2587
		// (get) Token: 0x060023DD RID: 9181 RVA: 0x0005BEB2 File Offset: 0x0005AEB2
		// (set) Token: 0x060023DE RID: 9182 RVA: 0x0005A471 File Offset: 0x00059471
		public _IExpression XRefFrom
		{
			get
			{
				if (this.m_itrefFrom == null)
				{
					return new NullExpression_Green();
				}
				return this.m_itrefFrom;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x060023DF RID: 9183 RVA: 0x00013F96 File Offset: 0x00012F96
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060023E0 RID: 9184 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public override void AcceptVisitor(IExprVisitor visitor)
		{
		}

		// Token: 0x040006D9 RID: 1753
		private readonly _IItemReference m_itref;

		// Token: 0x040006DA RID: 1754
		private readonly _IItemReference m_itrefFrom;
	}
}
