using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.Interfaces
{
	// Token: 0x020001D4 RID: 468
	public interface _ILMServiceProvider3 : ILMServiceProvider5, ILMServiceProvider4, ILMServiceProvider3, ILMServiceProvider2, ILMServiceProvider
	{
		// Token: 0x1700091E RID: 2334
		// (get) Token: 0x0600212E RID: 8494
		ILMCompiledSetStorage CompiledSetStorage { get; }
	}
}
