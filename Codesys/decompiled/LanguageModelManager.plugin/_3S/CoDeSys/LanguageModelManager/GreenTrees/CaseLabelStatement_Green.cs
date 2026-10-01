using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001E1 RID: 481
	internal class CaseLabelStatement_Green : Statement_Green, _ICaseLabelStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, ICaseLabelStatement
	{
		// Token: 0x060021C7 RID: 8647 RVA: 0x0005A6D7 File Offset: 0x000596D7
		public CaseLabelStatement_Green(_IExpression[] cases)
		{
			this.m_cases = cases;
		}

		// Token: 0x1700094C RID: 2380
		// (get) Token: 0x060021C8 RID: 8648 RVA: 0x0005A6E8 File Offset: 0x000596E8
		public IExpression[] cases
		{
			get
			{
				return this.m_cases;
			}
		}

		// Token: 0x1700094D RID: 2381
		// (get) Token: 0x060021C9 RID: 8649 RVA: 0x0005A6FD File Offset: 0x000596FD
		public IList<_IExpression> _cases
		{
			get
			{
				return this.m_cases;
			}
		}

		// Token: 0x1700094E RID: 2382
		public _IExpression this[int i]
		{
			get
			{
				return this.m_cases[i];
			}
			set
			{
				throw new NotSupportedException("no manipulation of green tree expressions");
			}
		}

		// Token: 0x060021CC RID: 8652 RVA: 0x0005A609 File Offset: 0x00059609
		[Obsolete("no list access")]
		public void AddCase(_IExpression expCase)
		{
			throw new NotSupportedException("no manipulation of green tree expressions");
		}

		// Token: 0x060021CD RID: 8653 RVA: 0x000146E2 File Offset: 0x000136E2
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060021CE RID: 8654 RVA: 0x000146F4 File Offset: 0x000136F4
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x0400067A RID: 1658
		private readonly _IExpression[] m_cases;
	}
}
