using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ICompiler3 : ICompiler2, ICompiler
	{
		IDownloadInfo GetRelocatedDownloadInfo(Guid guidApplication, bool bOnlineChange, bool bBootProject, bool bOfflineBootProject, int[] nAreaMapping);
	}
}
