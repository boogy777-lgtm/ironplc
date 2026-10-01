using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x02000204 RID: 516
	internal class BasedIntegerLiteralExpression_Green : IntegerLiteralExpression_Green
	{
		// Token: 0x06002323 RID: 8995 RVA: 0x0005B9F3 File Offset: 0x0005A9F3
		internal BasedIntegerLiteralExpression_Green(long lVal, TypeClass tc, int nBase) : this(lVal, tc, nBase, false)
		{
		}

		// Token: 0x06002324 RID: 8996 RVA: 0x0005B9FF File Offset: 0x0005A9FF
		internal BasedIntegerLiteralExpression_Green(long lVal, TypeClass tc, int nBase, bool negative) : base(lVal, tc)
		{
			this.m_nBase = nBase;
			this.m_bNegative = negative;
		}

		// Token: 0x170009D9 RID: 2521
		// (get) Token: 0x06002325 RID: 8997 RVA: 0x0005BA18 File Offset: 0x0005AA18
		// (set) Token: 0x06002326 RID: 8998 RVA: 0x0005A471 File Offset: 0x00059471
		public override int Base
		{
			get
			{
				return this.m_nBase;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x040006BD RID: 1725
		protected int m_nBase;
	}
}
