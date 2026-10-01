using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.Interfaces;

namespace _3S.CoDeSys.LanguageModelManager.Services
{
	// Token: 0x0200023F RID: 575
	[TypeGuid("{B3B7A465-8D6A-4620-9DCF-5D03FA3B563B}")]
	public class DownloadHelperService : IDownloadHelperService
	{
		// Token: 0x06002664 RID: 9828 RVA: 0x0005ED00 File Offset: 0x0005DD00
		public void DownloadWithoutChanges(Guid appGuid)
		{
			ILMCompiledSetStorage compiledSetStorage = APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage;
			ILMCompiledApplicationSet compiledApplicationSet = compiledSetStorage.GetCompiledApplicationSet(appGuid);
			ILMCompiledApplicationSet downloadedApplicationSet = compiledSetStorage.GetDownloadedApplicationSet(appGuid);
			if (compiledApplicationSet != downloadedApplicationSet)
			{
				compiledSetStorage.SetCompiledApplicationSet(appGuid, downloadedApplicationSet);
				APEnvironmentFacade.Instance.LanguageModelMgr.OnCodeChanged(new CodeChangeEventArgs(appGuid, null, (ICompileContext8)compiledApplicationSet));
			}
		}
	}
}
