using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace \u001A
{
	// Token: 0x0200022C RID: 556
	internal static class \u0008
	{
		// Token: 0x060024F3 RID: 9459 RVA: 0x0007ED90 File Offset: 0x0007CF90
		public static bool \u0001(this ICodegenerator \u0002, CodegeneratorProperties \u0003)
		{
			ICodegenerator3 codegenerator = \u0002 as ICodegenerator3;
			return codegenerator != null && codegenerator.GetProperty(\u0003);
		}
	}
}
