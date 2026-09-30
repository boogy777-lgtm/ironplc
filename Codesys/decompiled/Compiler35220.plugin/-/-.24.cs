using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0084;

namespace \u0012
{
	// Token: 0x02000078 RID: 120
	internal sealed class \u0003 : ICompilerServiceExprementWriter
	{
		// Token: 0x06000A39 RID: 2617 RVA: 0x000145F4 File Offset: 0x000127F4
		public string \u0001(_IExprement \u0002, WriteExprementFlags \u0003)
		{
			\u0002 u = new \u0002(false, false, (\u0003 & WriteExprementFlags.MinimalParentheses) > WriteExprementFlags.None);
			\u0002.Accept(u);
			return u.Output;
		}
	}
}
