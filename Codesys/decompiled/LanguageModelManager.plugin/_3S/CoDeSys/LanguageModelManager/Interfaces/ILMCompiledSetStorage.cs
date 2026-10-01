using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.Interfaces
{
	// Token: 0x020001D3 RID: 467
	public interface ILMCompiledSetStorage
	{
		// Token: 0x0600211F RID: 8479
		ILMCompiledApplicationSet GetCompiledApplicationSet(Guid guidApplication);

		// Token: 0x06002120 RID: 8480
		void SetCompiledApplicationSet(Guid guidApplication, ILMCompiledApplicationSet set);

		// Token: 0x06002121 RID: 8481
		void RemoveCompiledApplicationSet(Guid guidApplication);

		// Token: 0x1700091C RID: 2332
		// (get) Token: 0x06002122 RID: 8482
		IEnumerable<ILMCompiledApplicationSet> CompiledApplicationSets { get; }

		// Token: 0x06002123 RID: 8483
		ILMCompiledApplicationSet GetDownloadedApplicationSet(Guid guidApplication);

		// Token: 0x06002124 RID: 8484
		ILMCompiledApplicationSet GetDownloadedApplicationSetSynchronLoad(Guid guidApplication);

		// Token: 0x06002125 RID: 8485
		ILMCompiledApplicationSet GetDownloadedApplicationSetForBootApplicationSynchronLoad(Guid guidApplication);

		// Token: 0x06002126 RID: 8486
		void LoadDownloadedApplicationSetInBackground(Guid guidApplication);

		// Token: 0x06002127 RID: 8487
		void SetDownloadedApplicationSet(Guid guidApplication, ILMCompiledApplicationSet set, bool bAbortStorageOfPrevious);

		// Token: 0x06002128 RID: 8488
		void RemoveDownloadedApplicationSet(Guid guidApplication);

		// Token: 0x1700091D RID: 2333
		// (get) Token: 0x06002129 RID: 8489
		IEnumerable<ILMCompiledApplicationSet> DownloadedApplicationSets { get; }

		// Token: 0x0600212A RID: 8490
		void Clear();

		// Token: 0x0600212B RID: 8491
		void SetCompiledApplicationSetWithStackoverflow(Guid guidApplication, ILMCompiledApplicationSet set);

		// Token: 0x0600212C RID: 8492
		ILMCompiledApplicationSet GetCompiledApplicationSetWithStackoverflow(Guid guidApplication);

		// Token: 0x0600212D RID: 8493
		void RemoveCompiledApplicationSetWithStackoverflow(Guid guidApplication);
	}
}
