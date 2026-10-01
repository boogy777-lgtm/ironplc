using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001FD RID: 509
	internal class ConversionExpression_Green : Expression_Green, _IConversionExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IConversionExpression
	{
		// Token: 0x060022D2 RID: 8914 RVA: 0x0005B682 File Offset: 0x0005A682
		internal ConversionExpression_Green(TypeClass tcFrom, TypeClass tcTo, _IExpression exp)
		{
			this.m_tcFrom = tcFrom;
			this.m_tcTo = tcTo;
			this.m_exp = exp;
		}

		// Token: 0x170009B7 RID: 2487
		// (get) Token: 0x060022D3 RID: 8915 RVA: 0x00004E6B File Offset: 0x00003E6B
		public virtual bool Implicit
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170009B8 RID: 2488
		// (get) Token: 0x060022D4 RID: 8916 RVA: 0x0005B69F File Offset: 0x0005A69F
		// (set) Token: 0x060022D5 RID: 8917 RVA: 0x0005A471 File Offset: 0x00059471
		public TypeClass From
		{
			get
			{
				return this.m_tcFrom;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x170009B9 RID: 2489
		// (get) Token: 0x060022D6 RID: 8918 RVA: 0x0005B6A7 File Offset: 0x0005A6A7
		// (set) Token: 0x060022D7 RID: 8919 RVA: 0x0005A471 File Offset: 0x00059471
		public TypeClass To
		{
			get
			{
				return this.m_tcTo;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x170009BA RID: 2490
		// (get) Token: 0x060022D8 RID: 8920 RVA: 0x0005B6AF File Offset: 0x0005A6AF
		public IExpression Exp
		{
			get
			{
				return this._Exp;
			}
		}

		// Token: 0x170009BB RID: 2491
		// (get) Token: 0x060022D9 RID: 8921 RVA: 0x0005B6B7 File Offset: 0x0005A6B7
		// (set) Token: 0x060022DA RID: 8922 RVA: 0x0005A471 File Offset: 0x00059471
		public _IExpression _Exp
		{
			get
			{
				if (this.m_exp == null)
				{
					return new NullExpression_Green();
				}
				return this.m_exp;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x060022DB RID: 8923 RVA: 0x0000D72F File Offset: 0x0000C72F
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060022DC RID: 8924 RVA: 0x0000D741 File Offset: 0x0000C741
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x040006AF RID: 1711
		protected TypeClass m_tcFrom;

		// Token: 0x040006B0 RID: 1712
		protected TypeClass m_tcTo;

		// Token: 0x040006B1 RID: 1713
		protected _IExpression m_exp;
	}
}
