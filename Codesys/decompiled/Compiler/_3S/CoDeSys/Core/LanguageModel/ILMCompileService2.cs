using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMCompileService2 : ILMCompileService
	{
		IDownloadInfo GetRelocatedDownloadInfo(Guid guidApplication, bool bOnlineChange, bool bBootProject, bool bOfflineBootProject, int[] nAreaMapping);
	}
}
