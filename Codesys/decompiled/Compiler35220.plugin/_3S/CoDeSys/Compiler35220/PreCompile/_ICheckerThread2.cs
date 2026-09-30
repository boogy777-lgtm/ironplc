using System;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.PreCompile
{
	// Token: 0x0200016B RID: 363
	public interface _ICheckerThread2 : _ICheckerThread
	{
		// Token: 0x060018BD RID: 6333
		bool ChecksDone();

		// Token: 0x060018BE RID: 6334
		void AddRecentLMResult(LanguageModelResult result);
	}
}
