using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;

namespace \u001F
{
	// Token: 0x020000D3 RID: 211
	internal sealed class \u0003 : IComparer<ISignature>
	{
		// Token: 0x06000EE7 RID: 3815 RVA: 0x000293C4 File Offset: 0x000275C4
		public int \u0001(ISignature \u0002, ISignature \u0003)
		{
			if (\u0002.Id < \u0003.Id)
			{
				return -1;
			}
			if (\u0002 == \u0003)
			{
				return 0;
			}
			return 1;
		}
	}
}
