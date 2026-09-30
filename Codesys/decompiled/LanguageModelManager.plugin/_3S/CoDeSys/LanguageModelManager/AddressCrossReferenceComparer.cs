using System;
using System.Collections.Generic;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200014C RID: 332
	internal class AddressCrossReferenceComparer : IComparer<AddressCrossReference>
	{
		// Token: 0x06001B72 RID: 7026 RVA: 0x0004DD32 File Offset: 0x0004CD32
		public int Compare(AddressCrossReference cfx, AddressCrossReference cfy)
		{
			if (cfx.CodeId < cfy.CodeId)
			{
				return -1;
			}
			if (cfx.CodeId == cfy.CodeId)
			{
				return 0;
			}
			return 1;
		}
	}
}
