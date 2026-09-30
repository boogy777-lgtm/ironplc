using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0084
{
	// Token: 0x020000D4 RID: 212
	internal sealed class \u0003 : IComparer<Tuple<ISignature, _IVariable>>
	{
		// Token: 0x06000EE9 RID: 3817 RVA: 0x000293E8 File Offset: 0x000275E8
		public int \u0001(Tuple<ISignature, _IVariable> \u0002, Tuple<ISignature, _IVariable> \u0003)
		{
			if (\u0002.Item2.DataLocation.Offset < \u0003.Item2.DataLocation.Offset)
			{
				return -1;
			}
			if (\u0002.Item2.DataLocation.Offset == \u0003.Item2.DataLocation.Offset)
			{
				return 0;
			}
			return 1;
		}
	}
}
