using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x02000224 RID: 548
	internal class HasAttributeExpression_Green : PragmaExpression_Green, _IHasAttributeExpression, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IHasAttributeExpression
	{
		// Token: 0x06002417 RID: 9239 RVA: 0x0005C058 File Offset: 0x0005B058
		public HasAttributeExpression_Green(_IItemReference itref, string stAttribute)
		{
			this.m_itref = itref;
			this.m_stAttribute = stAttribute;
		}

		// Token: 0x17000A2F RID: 2607
		// (get) Token: 0x06002418 RID: 9240 RVA: 0x0005C06E File Offset: 0x0005B06E
		// (set) Token: 0x06002419 RID: 9241 RVA: 0x0005A471 File Offset: 0x00059471
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
				throw new NotSupportedException();
			}
		}

		// Token: 0x17000A30 RID: 2608
		// (get) Token: 0x0600241A RID: 9242 RVA: 0x0005C084 File Offset: 0x0005B084
		// (set) Token: 0x0600241B RID: 9243 RVA: 0x0005A471 File Offset: 0x00059471
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

		// Token: 0x17000A31 RID: 2609
		// (get) Token: 0x0600241C RID: 9244 RVA: 0x0005C08C File Offset: 0x0005B08C
		// (set) Token: 0x0600241D RID: 9245 RVA: 0x0005A471 File Offset: 0x00059471
		public string Attribute
		{
			get
			{
				return this.m_stAttribute;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x0600241E RID: 9246 RVA: 0x0000ED40 File Offset: 0x0000DD40
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x0600241F RID: 9247 RVA: 0x0000ED52 File Offset: 0x0000DD52
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x040006E5 RID: 1765
		private readonly _IItemReference m_itref;

		// Token: 0x040006E6 RID: 1766
		private readonly string m_stAttribute;
	}
}
