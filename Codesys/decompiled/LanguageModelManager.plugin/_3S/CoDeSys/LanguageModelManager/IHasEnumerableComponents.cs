using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000159 RID: 345
	internal interface IHasEnumerableComponents
	{
		// Token: 0x06001BCF RID: 7119
		IEnumerable<string> GetComponents(IScope scope, out bool bValid);
	}
}
