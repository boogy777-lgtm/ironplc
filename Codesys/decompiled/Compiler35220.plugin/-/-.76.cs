using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace \u0012
{
	// Token: 0x02000103 RID: 259
	internal sealed class \u0006 : IExpressionInfo
	{
		// Token: 0x06001340 RID: 4928 RVA: 0x00035224 File Offset: 0x00033424
		public \u0006(IExpression \u001D\u0002, IType \u0017)
		{
			this.\u0001 = \u001D\u0002;
			this.\u0001 = \u0017;
		}

		// Token: 0x170004C5 RID: 1221
		// (get) Token: 0x06001341 RID: 4929 RVA: 0x0003523C File Offset: 0x0003343C
		public IType Type
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x170004C6 RID: 1222
		// (get) Token: 0x06001342 RID: 4930 RVA: 0x00035244 File Offset: 0x00033444
		public IExpression Expression
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x04000337 RID: 823
		private IExpression \u0001;

		// Token: 0x04000338 RID: 824
		private IType \u0001;
	}
}
