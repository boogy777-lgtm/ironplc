using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMDownloadedApplicationService2 : ILMDownloadedApplicationService
	{
		void LoadDownloadedApplicationSetInBackground(Guid guidApplication);
	}
}
