using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x0200021D RID: 541
	internal class DefinedExpression_Green : PragmaExpression_Green, _IDefinedExpression, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IDefinedExpression
	{
		// Token: 0x060023E1 RID: 9185 RVA: 0x0005BEC8 File Offset: 0x0005AEC8
		public DefinedExpression_Green(_IItemReference itref)
		{
			this.m_itref = itref;
		}

		// Token: 0x17000A1C RID: 2588
		// (get) Token: 0x060023E2 RID: 9186 RVA: 0x0005BED7 File Offset: 0x0005AED7
		// (set) Token: 0x060023E3 RID: 9187 RVA: 0x0005BEED File Offset: 0x0005AEED
		public _IExpression ItemReference
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
				this.m_itref = (value as _IItemReference);
			}
		}

		// Token: 0x17000A1D RID: 2589
		// (get) Token: 0x060023E4 RID: 9188 RVA: 0x0005BEFB File Offset: 0x0005AEFB
		// (set) Token: 0x060023E5 RID: 9189 RVA: 0x0005A471 File Offset: 0x00059471
		public IExpression ReferencedItem
		{
			get
			{
				return this.ItemReference;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x060023E6 RID: 9190 RVA: 0x0000E302 File Offset: 0x0000D302
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060023E7 RID: 9191 RVA: 0x0000E314 File Offset: 0x0000D314
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x040006DB RID: 1755
		private _IItemReference m_itref;
	}
}
