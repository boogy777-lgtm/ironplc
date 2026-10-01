using System;
using System.Runtime.CompilerServices;
using \u0004;
using \u0011;
using \u0019;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0017
{
	// Token: 0x02000238 RID: 568
	internal sealed class \u0011 : IIndexInfo
	{
		// Token: 0x170006DF RID: 1759
		// (get) Token: 0x06002579 RID: 9593 RVA: 0x00081E00 File Offset: 0x00080000
		// (set) Token: 0x0600257A RID: 9594 RVA: 0x00081E08 File Offset: 0x00080008
		public _IExpression IndexExpressionRaw { get; set; }

		// Token: 0x0600257B RID: 9595 RVA: 0x00081E14 File Offset: 0x00080014
		public void \u0001(global::\u0004.\u000E \u0002)
		{
			if (this.IndexExpressionRaw == null)
			{
				return;
			}
			global::\u0011.\u0007 u = new global::\u0011.\u0007
			{
				AccessModeFlags = AccessModeFlags.Read
			};
			_IExpressionStatement iexpressionStatement = \u0019.\u0003.\u0001(this.IndexExpressionRaw);
			iexpressionStatement = \u0002.\u0001<_IExpressionStatement>(iexpressionStatement, u, true, true, false);
			this.\u0001 = iexpressionStatement._Expr;
			this.IndexExpressionRaw = null;
		}

		// Token: 0x170006E0 RID: 1760
		// (get) Token: 0x0600257C RID: 9596 RVA: 0x00081E64 File Offset: 0x00080064
		public IExpression IndexExpression
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x170006E1 RID: 1761
		// (get) Token: 0x0600257D RID: 9597 RVA: 0x00081E6C File Offset: 0x0008006C
		// (set) Token: 0x0600257E RID: 9598 RVA: 0x00081E74 File Offset: 0x00080074
		public int BaseSize
		{
			get
			{
				return this.\u0001;
			}
			set
			{
				this.\u0001 = value;
				if (this.\u0001 != 1 && this.\u0001 != 2 && this.\u0001 != 4 && this.\u0001 != 8)
				{
					throw new ArgumentException("BaseSize unequals 1,2,4,8");
				}
			}
		}

		// Token: 0x040006C0 RID: 1728
		private _IExpression \u0001;

		// Token: 0x040006C1 RID: 1729
		private int \u0001 = 1;

		// Token: 0x040006C2 RID: 1730
		[CompilerGenerated]
		private _IExpression \u0002;
	}
}
