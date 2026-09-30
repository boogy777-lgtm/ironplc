using System;

namespace _3S.CoDeSys.LanguageModelManager.Interfaces
{
	// Token: 0x020001D5 RID: 469
	internal interface IDelayedLoadItem
	{
		// Token: 0x1700091F RID: 2335
		// (get) Token: 0x0600212F RID: 8495
		string ProcessText { get; }

		// Token: 0x06002130 RID: 8496
		void ProcessLoad(bool bLastLibraryItem, AsyncLogger asyncLogger);

		// Token: 0x06002131 RID: 8497
		void PostProcessLoad(bool bLastLibraryItem);

		// Token: 0x06002132 RID: 8498
		bool IsEqual(IDelayedLoadItem item);

		// Token: 0x17000920 RID: 2336
		// (get) Token: 0x06002133 RID: 8499
		// (set) Token: 0x06002134 RID: 8500
		bool IsCompleted { get; set; }

		// Token: 0x17000921 RID: 2337
		// (get) Token: 0x06002135 RID: 8501
		// (set) Token: 0x06002136 RID: 8502
		bool DuringPostProcess { get; set; }
	}
}
